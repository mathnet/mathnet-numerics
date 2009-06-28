// (c) Microsoft Corporation. All rights reserved

#light

#if STANDALONE_METADATA

module (* internal *) FSharp.PowerPack.Metadata.Reader.Internal.Import

open System.Collections.Generic 
open FSharp.PowerPack.Metadata.Reader.Internal.AbstractIL.IL
open FSharp.PowerPack.Metadata.Reader.Internal.Prelude
open FSharp.PowerPack.Metadata.Reader.Internal.Tast

#else

module (* internal *) Microsoft.FSharp.Compiler.Import

open System.Collections.Generic
open Internal.Utilities
open Microsoft.FSharp.Compiler.AbstractIL.Internal.Library
open Microsoft.FSharp.Compiler 

open Microsoft.FSharp.Compiler.Range
open Microsoft.FSharp.Compiler.Tast
open Microsoft.FSharp.Compiler.Tastops
open Microsoft.FSharp.Compiler.AbstractIL.IL
open Microsoft.FSharp.Compiler.Env
open Microsoft.FSharp.Compiler.Ast
open Microsoft.FSharp.Compiler.ErrorLogger

#endif

type AssemblyLoader = 
     | AssemblyLoader of (range * ILAssemblyRef -> CcuResolutionResult)
     member x.LoadAssembly(m,aref) = (let (AssemblyLoader(f)) = x in f (m,aref))


//-------------------------------------------------------------------------
// Import an IL types as F# types.
//------------------------------------------------------------------------- 

// This is the context used for converting AbstractIL .NET types to F# internal compiler data structures.
// We currently cache the conversion of AbstractIL ILTypeRef nodes, based on hashes of these.
[<Sealed>]
type ImportMap(g:TcGlobals,assemMap:AssemblyLoader) =
    let typeRefToTyconRefCache = new System.Collections.Generic.Dictionary<ILTypeRef,TyconRef>()
    member this.g = g
    member this.assemMap = assemMap
    member this.IlTypeRefToTyconRefCache = typeRefToTyconRefCache

/// Import an IL type ref as an F# type constructor.
let ImportIlTypeRefUncached (env:ImportMap) m (tref:ILTypeRef) = 
    let tname = tref.Name
    let encl = tref.Enclosing
    let path,nsnb = (match encl with [] -> split_type_name_array tname | h :: t -> Array.append (split_namespace_array h) (Array.of_list t), tname)
    let ccu =  
        match tref.Scope with 
        | ScopeRef_local    -> error(InternalError("ImportILTypeRef: unexpected local scope",m))
        | ScopeRef_module _ -> error(InternalError("ImportILTypeRef: reference found to a type in an auxiliary module",m))
        | ScopeRef_assembly assref -> env.assemMap.LoadAssembly (m,assref)  // NOTE: only assemMap callsite

    // Do a dereference of a fake tcref for the type just to check it exists in the target assembly and to find
    // the corresponding Tycon.
    let ccu = 
        match ccu with
        | ResolvedCcu(ccu)->ccu
        | UnresolvedCcu(ccuName) -> error (Error("The type '"^tname^"' is required here and is unavailable. You must add a reference to assembly '"^ccuName^"' ",m))
    let fake_tcref = mk_nonlocal_tcref (NLPath(ccu,path)) nsnb
    let tycon = 
        try 
            deref_tycon fake_tcref
        with _ ->
            error (Error("A reference to the type '"^tref.FullName^"' in assembly "^ccu.AssemblyName^" was found, but the type could not be found in that assembly",m))
    match tycon.PublicPath with 
    | None -> error (Error ("An imported assembly uses the type '"^String.concat "." (Array.to_list path@[nsnb])^"' but that type is not public",m));
    | Some pubpath -> rescope_tycon_pubpath ccu pubpath tycon
    
    
let ImportILTypeRef (env:ImportMap) m (tref:ILTypeRef) =
    if env.IlTypeRefToTyconRefCache.ContainsKey(tref) then
        env.IlTypeRefToTyconRefCache.[tref]
    else 
        let tcref = ImportIlTypeRefUncached  env m tref
        env.IlTypeRefToTyconRefCache.[tref] <- tcref;
        tcref

/// Import an IL type as an F# type
/// - The F# type check does the job of making the "void" into a "unit" value, whatever the repr. of "unit" is. 
let rec ImportILType (env:ImportMap) m tinst typ = 
    match typ with
    | Type_void -> 
        mk_unit_typ env.g

    | Type_array(bounds,ty) -> 
        let n = bounds.Rank
        let elementType = ImportILType env m tinst ty
        mk_multi_dim_array_typ env.g n elementType 

    | Type_boxed  tspec | Type_value tspec ->
        let ninst = tspec.GenericArgs.Length
        let tcref = ImportILTypeRef env m tspec.TypeRef 
        let inst = tspec.GenericArgs |> List.map (ImportILType env m tinst) 
        // Prefer the F# abbreviation for some built-in types, e.g. 
        // 'string' rather than 'System.String', since users don't 
        // on the whole realise that these are defined in terms of their .NET equivalents 
        // Also on import we decompile uses of FastFunc and Tuple. 
#if STANDALONE_METADATA
#else
        match env.g.better_tcref_map tcref inst with 
        | Some res -> res
        | None -> 
#endif
        TType_app (tcref,inst) 

    | Type_byref ty -> mk_byref_typ env.g (ImportILType env m tinst ty)
    | Type_ptr ty  -> mk_nativeptr_typ env.g (ImportILType env m tinst ty)
    | Type_fptr _ -> mk_nativeint_typ env.g (* failwith "cannot import this kind of type (ptr, fptr)" *)
    | Type_modified(_,_,ty) -> 
         // All custom modifiers are ignored
         ImportILType env m tinst ty
    | Type_tyvar u16 -> 
         try List.nth tinst (int u16) 
         with _ -> 
              error(Error("internal error or badly formed meatdata: not enough type parameters were in scope while importing",m))

//-------------------------------------------------------------------------
// Load an IL assembly into the compiler's internal data structures
// Careful use is made of laziness here to ensure we don't read the entire IL
// assembly on startup.
//-------------------------------------------------------------------------- 


// tinst gives the type parameters for the enclosing type when converting the type parameters of a generic method
let ImportIlTypars amap m scoref tinst gps = 
    match gps with 
    | [] -> []
    | _ -> 
        let amap = amap()
        let tps = gps |> List.map (fun gp -> mk_rigid_typar gp.gpName m) 

        let tptys = tps |> List.map mk_typar_ty
        let importInst = tinst@tptys
        (tps,gps) ||> List.iter2 (fun tp gp -> 
            let constraints = gp.gpConstraints |> List.map (fun ilty -> TTyparCoercesToType(ImportILType amap m importInst (rescope_typ scoref ilty),m) )
            let constraints = if gp.gpReferenceTypeConstraint then (TTyparIsReferenceType(m)::constraints) else constraints
            let constraints = if gp.gpNotNullableValueTypeConstraint then (TTyparIsNotNullableValueType(m)::constraints) else constraints
            let constraints = if gp.gpDefaultConstructorConstraint then (TTyparRequiresDefaultConstructor(m)::constraints) else constraints
            fixup_typar_constraints tp constraints);
        tps


let MultisetDiscriminateAndMap nodef tipf (items: ('key list * 'value) list) = 
    // Find all the items with an empty key list and call 'tipf' 
    let tips = 
        [ for (keylist,v) in items do 
             match keylist with 
             | [] -> yield tipf v
             | _ -> () ]

    // Find all the items with a non-empty key list. Bucket them together by
    // the first key. For each bucket, call 'nodef' on that head key and the bucket.
    let nodes = 
        let buckets = new Dictionary<_,_>(10)
        for (keylist,v) in items do
            match keylist with 
            | [] -> ()
            | key::rest -> 
                buckets.[key] <- (rest,v) :: (if buckets.ContainsKey key then buckets.[key] else []);

        [ for (KeyValue(key,items)) in buckets -> nodef key items ]

    tips,nodes
 

let rec ImportIlTypeDef amap m scoref cpath enc nm (ltdef:Lazy<ILTypeDef>)  =
    let tdef = ltdef.Force()
    let lmtyp = 
        let nested = tdef.NestedTypes
        lazy 
            (let cpath = (mk_cpath cpath nm FSharpModule)
             ImportIlTypeDefs amap m scoref cpath (enc@[tdef]) nested) 
    // Add the type itself. 
    NewILTycon 
        (Some cpath) 
        (nm,m) 
#if STANDALONE_METADATA
        (LazyWithContext<_,_>.Create(fun _ -> ImportIlTypars amap m scoref [] tdef.GenericParams))
#else
        // The read of the type parameters may fail to resolve types. We pick up a new range from the point where that read is forced
        (LazyWithContext<_,_>.Create(fun m -> ImportIlTypars amap m scoref [] tdef.GenericParams))
#endif
        (scoref,enc,tdef) 
        lmtyp 
       

and ImportIlTypeDefList amap m scoref cpath enc items =
    // Split into the ones with namespaces and without 
    // This is a non-trivial function.  
    // Add the ones with namespaces in buckets 
    // That is, multi-set discriminate based in the first element of the namespace list (e.g. "System") 
    // and for each bag resulting from the discrimination fold-in a lazy computation to add the types under that bag 
    let rec add cpath items = 
        let tycons,namespaceModules = 
           items 
           |> MultisetDiscriminateAndMap 
              // nodef - called for each bucket, where 'n' is the head element of the namespace used
              // as a key in the discrimination, tgs is the remaining descriptors.  We create a sub-module for 'n' 
              (fun n tgs ->
                  let modty = lazy (add (mk_cpath cpath n Namespace) tgs)
                  let mspec = NewModuleOrNamespace (Some cpath) taccessPublic (mksyn_id m n) emptyXmlDoc [] modty
                  mspec)

              // tipf - called if there are no namespace items left to discriminate on. 
              (fun (n,info:Lazy<_>) -> 
                 //Note: this scoref looks like it will always be identical to 'scoref'
                 let (scoref2,attrs,ltdef) = info.Force()
                 ImportIlTypeDef amap m scoref2 cpath enc n ltdef)

        let kind = match enc with [] -> Namespace | _ -> FSharpModule
        (NewModuleOrNamespaceType kind (tycons@namespaceModules) [] )
      
    add cpath items

and ImportIlTypeDefs amap m scoref cpath enc tdefs =
    tdefs
    |> dest_lazy_tdefs 
    |> List.map (fun (ns,n,attrs,ltdef) -> (ns,(n,notlazy(scoref,attrs,ltdef))))
    |> ImportIlTypeDefList amap m scoref cpath enc

let ImportIlAssemblyMainTypeDefs amap m scoref modul = 
    modul.modulTypeDefs |> ImportIlTypeDefs amap m scoref (CompPath(scoref,[])) [] 

/// Read the "exported types" table for multi-module assemblies. 
let ImportIlAssemblyExportedType amap m auxModLoader (scoref:ILScopeRef) (exportedType:ILExportedType) = 
    // Forwarders are dealt with separately in the ref->def dereferencing logic in tast.ml as they effectively give rise to type equivalences
    if exportedType.IsForwarder then 
        []
    else
        let info = 
            lazy (match 
                    (try 
                        let modul = auxModLoader exportedType.ScopeRef
                        Some (lazy (find_tdef exportedType.Name modul.modulTypeDefs)) 
                     with :? System.Collections.Generic.KeyNotFoundException -> None)
                    with 
                  | None -> 
                     error(Error("A reference to the DLL "^exportedType.ScopeRef.QualifiedName ^" is required by assembly " ^ scoref.QualifiedName ^ ". The imported type "^exportedType.Name^" is located in the first assembly and could not be resolved"  ,m))
                  | Some (ltdef) -> 
                     scoref,exportedType.exportedTypeCustomAttrs,ltdef)
              
        let ns,n = split_type_name exportedType.Name
        [ ImportIlTypeDefList amap m scoref (CompPath(scoref,[])) [] [(ns,(n,info))]  ]

/// Read the "exported types" table for multi-module assemblies. 
let ImportIlAssemblyExportedTypes amap m auxModLoader scoref exportedTypes = 
    [ for exportedType in dest_exported_types exportedTypes do 
         yield! ImportIlAssemblyExportedType amap m auxModLoader scoref exportedType ]

let ImportIlAssemblyTypeDefs(amap,m,auxModLoader,aref,mainmod:ILModuleDef) = 
    let scoref = ScopeRef_assembly aref
    let mtypsForExportedTypes = ImportIlAssemblyExportedTypes amap m auxModLoader scoref mainmod.ManifestOfAssembly.manifestExportedTypes
    let mainmod = ImportIlAssemblyMainTypeDefs amap m scoref mainmod
    combine_mtyps [] m (mainmod :: mtypsForExportedTypes)

// Read the type forwarder table for an assembly
let ImportIlAssemblyTypeForwarders(amap,m,exportedTypes:ILExportedTypes) = 
    // Note 'td' may be in another module or another assembly!
    // Note: it is very important that we call auxModLoader lazily
    lazy 
      ([ //printfn "reading forwarders..." 
         for exportedType in dest_exported_types exportedTypes do 
             let ns,n = split_type_name exportedType.Name
             //printfn "found forwarder for %s..." n
             let tcref = ImportIlTypeRefUncached (amap()) m (ILTypeRef.Create(exportedType.ScopeRef,[],exportedType.Name))
             yield (Array.of_list ns,n),tcref
             let rec nested (net:ILNestedExportedTypes) enc = 
                 [ for net in dest_nested_exported_types exportedType.Nested do 
                    
                       //printfn "found nested forwarder for %s..." net.nestedExportedTypeName
                       let tcref = ImportIlTypeRefUncached (amap()) m (ILTypeRef.Create (exportedType.ScopeRef,enc,net.nestedExportedTypeName))
                       yield (Array.of_list enc,exportedType.Name),tcref 
                       yield! nested net.nestedExportedTypeNested (enc @ [ net.nestedExportedTypeName ]) ]
             yield! nested exportedType.Nested (ns@[n]) ] 
       |> Map.of_list) 
  

let ImportIlAssembly(amap,m,auxModuleLoader,sref,sourceDir,filename,ilModule:ILModuleDef) = 
        let aref =   
            match sref with 
            | ScopeRef_assembly aref -> aref 
            | _ -> error(InternalError("PrepareToImportReferencedIlDll: cannot reference .NET netmodules directly, reference the containing assembly instead",m))
        let nm = aref.Name
        let mty = ImportIlAssemblyTypeDefs(amap,m,auxModuleLoader,aref,ilModule)
        let ccuData = 
          { ccu_fsharp=false;
            ccu_usesQuotations=false;
            ccu_qname= Some sref.QualifiedName;
            ccu_contents = NewCcuContents sref m nm mty ;
            ccu_scoref = sref;
            ccu_stamp = new_stamp();
            ccu_code_dir = sourceDir;  // note: not an accurate value, but IL assemblies don't give us this information in any attributes. 
            ccu_filename = filename
            ccu_forwarders = 
               (match ilModule.Manifest with 
                | None -> lazy Map.empty
                | Some manifest -> ImportIlAssemblyTypeForwarders(amap,m,manifest.ExportedTypes)) }
                
        new_ccu nm ccuData
