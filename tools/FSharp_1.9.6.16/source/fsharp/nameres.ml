// (c) Microsoft Corporation. All rights reserved

#light

module (* internal *) Microsoft.FSharp.Compiler.Nameres

//-------------------------------------------------------------------------
// Name environment and name resolution 
//------------------------------------------------------------------------- 

open Internal.Utilities
open Internal.Utilities.Pervasives
open Microsoft.FSharp.Compiler 
open Microsoft.FSharp.Compiler.Range
open Microsoft.FSharp.Compiler.Ast
open Microsoft.FSharp.Compiler.ErrorLogger
open Microsoft.FSharp.Compiler.Tast
open Microsoft.FSharp.Compiler.Tastops
open Microsoft.FSharp.Compiler.Import
open Microsoft.FSharp.Compiler.Env
open Microsoft.FSharp.Compiler.Lib
open Microsoft.FSharp.Compiler.AbstractIL 
open Microsoft.FSharp.Compiler.AbstractIL.Internal 
open Microsoft.FSharp.Compiler.AbstractIL.Internal.Library

open Microsoft.FSharp.Compiler.AbstractIL.Diagnostics
open Microsoft.FSharp.Compiler.AbstractIL.IL // Abstract IL 
open Microsoft.FSharp.Compiler.Outcome
open Microsoft.FSharp.Compiler.Infos
open Microsoft.FSharp.Compiler.Infos.AccessibilityLogic
open Microsoft.FSharp.Compiler.Infos.AttributeChecking
open Microsoft.FSharp.Compiler.Layout
open Microsoft.FSharp.Compiler.PrettyNaming

type NameResolver(g:TcGlobals, 
                  amap: Import.ImportMap, 
                  infoReader: InfoReader, 
                  instantiationGenerator: (range -> typars -> tinst)) =
    /// Used to transform typars into new inference typars 
    // instantiationGenerator is a function to help us create the
    // type parameters by copying them from type parameter specifications read
    // from IL code.  
    //
    // When looking up items in generic types we create a fresh instantiation 
    // of the type, i.e. instantiate the type with inference variables. 
    // This means the item is returned ready for use by the type inference engine 
    // without further freshening. However it does mean we end up plumbing 'instantiationGenerator' 
    // around a bit more than we would like to, which is a bit annoying. 
    member nr.instantiationGenerator = instantiationGenerator
    member nr.g = g
    member nr.amap = amap
    member nr.InfoReader = infoReader
    
//-------------------------------------------------------------------------
// Helpers for unionconstrs and recdfields
//------------------------------------------------------------------------- 

let UnionCaseRefsInTycon modref (tycon:Tycon) = 
    tycon.UnionCasesAsList |> List.map (ucref_of_ucase (MakeNestedTcref modref tycon)) 

let UnionCaseRefsInModuleOrNamespace (modref:ModuleOrNamespaceRef) = 
    List.foldBack (UnionCaseRefsInTycon modref >> (@)) (NameMap.range modref.ModuleOrNamespaceType.AllEntities) []

let IsUnionCaseInTycon (id:ident) (tycon :Tycon) = isSome (tycon.GetUnionCaseByName id.idText) 

let IsRecdFieldInTycon (id:ident) (tycon :Tycon) = tycon.GetFieldByName(id.idText).IsSome

let IsRecdFieldInUnionCase (id:ident) (ucase:UnionCase) = ucase.GetFieldByName id.idText |> isSome

let TryFindTypeWithUnionCase (modref:ModuleOrNamespaceRef) id = 
    NameMap.tryFindInRange (IsUnionCaseInTycon id) modref.ModuleOrNamespaceType.AllEntities

let TryFindTypeWithRecdField (mty:ModuleOrNamespaceType) id = 
    NameMap.tryFindInRange (IsRecdFieldInTycon id) mty.AllEntities

let ActivePatternElemsOfValRef vref = 
    match apinfo_of_vref vref with
    | Some (APInfo(_,nms,_) as apinfo) -> List.mapi (fun i _ -> APElemRef(apinfo,vref, i)) nms
    | None -> [] 

let ActivePatternElemsOfVal modref vspec = 
    ActivePatternElemsOfValRef (mk_vref_in_modref modref vspec)


let ActivePatternElemsOfModuleOrNamespace (modref:ModuleOrNamespaceRef) : ActivePatternElemRef NameMap = 
    let mtyp = modref.ModuleOrNamespaceType
    cacheOptRef mtyp.ActivePatternsLookupTable (fun () ->
       let aprefs = List.foldBack (ActivePatternElemsOfVal modref >> (@)) (NameMap.range mtyp.AllValuesAndMembers) []
       List.foldBack (fun apref acc -> NameMap.add (name_of_apref apref) apref acc) aprefs Map.empty)

//---------------------------------------------------------------------------
// 
//------------------------------------------------------------------------- 

// Note: Active patterns are encoded like this:
//   let (|A|B|) x = if x < 0 then A else B    // A and B are reported as results using 'Item_apres' 
//   match () with | A | B -> ()               // A and B are reported using 'ITem_apelem'

[<StructuralEquality(false); StructuralComparison(false)>]
type NamedItem = 
  (* These exist in the "eUnqualifiedItems" List.map in the type environment. *)
  | Item_val of  ValRef
  | Item_ucase of UnionCaseInfo
  | Item_apres of ActivePatternInfo * typ * int 
  | Item_apelem of ActivePatternElemRef 
  | Item_ecref of TyconRef 
  | Item_recdfield of RecdFieldInfo

  (* The following are never in the items table but are valid results of binding *)
  (* an identitifer in different circumstances. *)
  | Item_newdef of ident
  | Item_il_field of ILFieldInfo
  | Item_event of EventInfo
  | Item_property of string * PropInfo list
  | Item_meth_group of string * MethInfo list
  | Item_ctor_group of string * MethInfo list
  | Item_fake_intf_ctor of Tast.typ
  | Item_delegate_ctor of Tast.typ
  | Item_typs of string * Tast.typ list
  | Item_modrefs of Tast.ModuleOrNamespaceRef list
  | Item_implicit_op of ident
  | Item_param_name of ident 
  | Item_prop_name of ident 


let MakeMethGroup (nm,minfos:MethInfo list) = 
    let minfos = minfos |> List.sortBy (fun minfo -> minfo.NumArgs |> List.sum)
    Item_meth_group (nm,minfos)

let MakeCtorGroup (nm,minfos:MethInfo list) = 
    let minfos = minfos |> List.sortBy (fun minfo -> minfo.NumArgs |> List.sum)
    Item_ctor_group (nm,minfos)

  
//---------------------------------------------------------------------------
//
//------------------------------------------------------------------------- 

type ExtensionMember = 
   | FSExtMem of ValRef
   | ILExtMem of ILTypeRef * ILMethodDef
   static member Equality g e1 e2 = 
       match e1, e2 with 
       | FSExtMem vref1, FSExtMem vref2 -> g.vref_eq vref1 vref2
       | ILExtMem (_,md1), ILExtMem (_,md2) -> md1 === md2
       | _ -> false
       
[<StructuralEquality(false); StructuralComparison(false)>]
type NameResolutionEnv =
    { /// Display environment information for output 
      eDisplayEnv: DisplayEnv;  
      
      /// Values and Data Tags available by unqualified name 
      eUnqualifiedItems: NamedItem NameMap;

      /// Data Tags and Active Pattern Tags available by unqualified name 
      ePatItems: NamedItem NameMap;

      /// Modules accessible via "." notation. Note this is a multi-map. 
      /// Adding a module abbreviation adds it a local entry to this List.map. 
      /// Likewise adding a ccu or opening a path adds entries to this List.map. 
      
      
      /// REVIEW (old comment)
      /// "The boolean flag is means the namespace or module entry shouldn't 'really' be in the 
      ///  map, and if it is everr used to resolve a name then we give a warning. 
      ///  This is used to give warnings on unqualified namespace accesses, e.g. 
      ///    open System 
      ///    open Collections                            <--- give a warning 
      ///    let v = new Collections.Generic.List<int>() <--- give a warning" 
      
      eModulesAndNamespaces:  (Tast.ModuleOrNamespaceRef list) NameMap; 
      
      /// Fully qualified modules and namespaces. 'open' does not change this. 
      eFullyQualifiedModulesAndNamespaces:  (Tast.ModuleOrNamespaceRef list) NameMap; 
      
      /// RecdField labels in scope.  RecdField labels are those where type are inferred 
      /// by label rather than by known type annotation. 
      /// Bools indicate if from a record, where no warning is given on indeterminate lookup 
      eFieldLabels: (Tast.RecdFieldRef * bool) NameMultiMap; 

      /// Tycons indexed by the various names that may be used to access them, e.g. 
      ///     "List" --> multiple tycon_refs for the various tycons accessible by this name. 
      ///     "List`1" --> TyconRef 
      eTyconsByAccessNames: TyconRef NameMultiMap; 

      /// Tycons available by unqualified, demangled names (i.e. (List,1) --> TyconRef) 
      eTyconsByDemangledNameAndArity: Map<NameArityPair,TyconRef>; 

      /// Extension members by type and name 
      eExtensionMembers: ExtensionMember TcrefMultiMap; 

      /// Typars (always available by unqualified names). Further typars can be 
      /// in the tpenv, a structure folded through each top-level definition. 
      eTypars: Typar NameMap; 

    } 

    static member Empty(g) =
        { eDisplayEnv=empty_denv g;
          eModulesAndNamespaces=Map.empty;
          eFullyQualifiedModulesAndNamespaces = Map.empty;
          eFieldLabels=Map.empty;
          eUnqualifiedItems=Map.empty;
          ePatItems=Map.empty;
          eTyconsByAccessNames=Map.empty;
          eTyconsByDemangledNameAndArity=Map.empty;
          eExtensionMembers=tcref_map_empty();      
          eTypars=Map.empty; }
    member x.DisplayEnv = x.eDisplayEnv
    member x.UnqualifiedItems = x.eUnqualifiedItems


//-------------------------------------------------------------------------
// NamedItem functions
//------------------------------------------------------------------------- 

let DisplayNameOfItem g d = 
    match d with
    | Item_val v -> v.DisplayName
    | Item_apelem apref -> name_of_apref apref
    | Item_ucase(ucr) -> DecompileOpName ucr.UnionCase.DisplayName
    | Item_ecref(ecr) -> ecr.DemangledExceptionName
    | Item_recdfield(rfinfo) -> DecompileOpName rfinfo.RecdField.Name
    | Item_newdef(id) -> id.idText
    | Item_il_field(finfo) -> finfo.FieldName
    | Item_event(einfo) -> einfo.EventName
    | Item_property(nm,pinfos) -> nm
    | Item_meth_group(nm,_) -> nm
    | Item_ctor_group(nm,_) -> DemangleGenericTypeName nm
    | Item_fake_intf_ctor typ 
    | Item_delegate_ctor typ -> DemangleGenericTypeName (tcref_of_stripped_typ g typ).MangledName
    | Item_typs(nm,tcref) -> DemangleGenericTypeName nm
    | Item_modrefs(modref :: _) -> demangled_name_of_modref modref
    | Item_param_name(id) -> id.idText
    | Item_prop_name(id) -> id.idText
    | _ ->  ""


// Add a value to the relevant table
//
// Object model members are not added to the name resolution environment *)
// because they use compiler-internal mangled names. *)
let AddValRefToItems (vref:ValRef) eUnqualifiedItems =
    match vref.MemberInfo with 
    | Some _ -> eUnqualifiedItems
    | None -> NameMap.add vref.MangledName (Item_val vref) eUnqualifiedItems

let AddValRefToExtensionMembers (vref:ValRef) eExtensionMembers =
    if vref.IsMember && vref.IsExtensionMember then
        tcref_mmap_add vref.MemberApparentParent (FSExtMem vref) eExtensionMembers 
    else
        eExtensionMembers

let AddActivePatternRefToPatternItems apref tab = 
    NameMap.add (name_of_apref apref) (Item_apelem apref) tab


/// This entrypoint is used to add some extra items to the environment for Visual Studio, e.g. static members 
let AddFakeNamedValRefToNameEnv nm vref nenv =
    {nenv with eUnqualifiedItems= NameMap.add nm (Item_val vref) nenv.eUnqualifiedItems }

/// This entrypoint is used to add some extra items to the environment for Visual Studio, e.g. record members
let AddFakeNameToNameEnv nm item nenv =
    {nenv with eUnqualifiedItems= NameMap.add nm item nenv.eUnqualifiedItems }

let AddValRefToNameEnv vref nenv =
    {nenv with eUnqualifiedItems= AddValRefToItems vref nenv.eUnqualifiedItems;
               eExtensionMembers = AddValRefToExtensionMembers vref nenv.eExtensionMembers;
               ePatItems = 
                   (let ePatItems = List.foldBack AddActivePatternRefToPatternItems (ActivePatternElemsOfValRef vref) nenv.ePatItems

                    (* Add literal constants to the environment available for resolving items in patterns *)
                    let ePatItems = 
                        match vref.LiteralValue with 
                        | None -> ePatItems 
                        | Some _ -> NameMap.add vref.MangledName (Item_val vref) ePatItems

                    ePatItems) }

let AddActivePatternResultTagsToNameEnv apinfo ty nenv =
    let nms = names_of_apinfo apinfo
    let apresl = nms |> List.mapi (fun j nm -> nm, j)
    { nenv with  eUnqualifiedItems= List.foldBack (fun (nm,j) acc -> Map.add nm (Item_apres(apinfo,ty,j)) acc) apresl nenv.eUnqualifiedItems; } 

let GeneralizeUnionCaseRef (ucref:UnionCaseRef) = 
    UnionCaseInfo(fst(generalize_tcref ucref.TyconRef), ucref)
    
let private AddTyconRefToNameEnv (g:TcGlobals) amap m nenv (tcref:TyconRef) = 
    let AddRecdField (rfref:RecdFieldRef) tab = NameMultiMap.add rfref.FieldName (rfref,rfref.TyconRef.IsRecordTycon) tab
    let AddUnionCase tab (ucref:UnionCaseRef)  = Map.add ucref.CaseName (Item_ucase (GeneralizeUnionCaseRef ucref)) tab
    let AddUnionCases tab ucrefs = List.fold AddUnionCase tab ucrefs
    let isIL = tcref.IsILTycon
    let ucrefs = if isIL then [] else tcref.UnionCasesAsList |> List.map (ucref_of_ucase tcref) 
    let flds =  if isIL then [| |] else tcref.AllFieldsArray

    let eExtensionMembers = 
        let csharpExtensionMeths =
            if isIL  then 
                let scoref,enc,tdef = tcref.ILTyconInfo
                if ILThingHasExtensionAttribute tdef.tdCustomAttrs then 
                    let tref = ILTypeInfo(tcref,tref_for_nested_tdef scoref (enc,tdef),[],tdef)
                    
                    if verbose then dprintfn "found extension attribute on type %s" tcref.MangledName
                    
                    dest_mdefs tdef.tdMethodDefs |> List.collect (fun md ->
                          if ILThingHasExtensionAttribute md.mdCustomAttrs then
                            match md.mdParams with 
                            | thisParam :: _ -> 
                                let ilty = thisParam.Type
                                match ilty with 
                                | Type_boxed tspec 
                                | Type_value tspec -> 
                                    let tcref = (tspec |> rescope_tspec scoref).TypeRef |> Import.ImportILTypeRef amap m
                                    if verbose then dprintfn "found extension method %s on type %s" md.Name tcref.MangledName
                                    
                                    [(tcref, tref, md)]
                                // Do not import extension members whose 'this' is only a type parameter
                                | _ ->
                                    []
                            | _ -> 
                                []
                          else
                              [])
                else
                    []
            else 
                []
        if verbose then dprintfn "found %d extension members on type %s" csharpExtensionMeths.Length tcref.MangledName
        (nenv.eExtensionMembers,csharpExtensionMeths) ||> List.fold (fun tab (tcref,tref,md) -> 
            tcref_mmap_add tcref (ILExtMem (tref.ILTypeRef, md)) tab)  
        
    
    { nenv with 
        eFieldLabels= 
            (if isIL then nenv.eFieldLabels 
             else (nenv.eFieldLabels,flds) ||> Array.fold_left (fun acc f -> 
                       if f.IsStatic || f.IsCompilerGenerated then acc 
                       else AddRecdField (rfref_of_rfield tcref f) acc)) ;
        eUnqualifiedItems    = 
            (if isIL then nenv.eUnqualifiedItems else AddUnionCases nenv.eUnqualifiedItems    ucrefs);
        ePatItems = 
            (if isIL then nenv.ePatItems else AddUnionCases nenv.ePatItems ucrefs);
        eExtensionMembers = 
            eExtensionMembers;
        eTyconsByDemangledNameAndArity= 
            AddTyconsByDemangledNameAndArity tcref.MangledName (tcref.Typars(m)) tcref nenv.eTyconsByDemangledNameAndArity; 
        eTyconsByAccessNames= 
            AddTyconsByAccessNames tcref.MangledName tcref nenv.eTyconsByAccessNames } 
    
let AddTyconRefsToNameEnv g amap m tcrefs nenv = 
    List.fold (AddTyconRefToNameEnv g amap m) nenv tcrefs

let AddExceptionDeclsToNameEnv (ecref:TyconRef) nenv = 
    assert ecref.IsExceptionDecl
    let add_ecref_to_tab tab = NameMap.add ecref.DemangledExceptionName (Item_ecref ecref) tab
    {nenv with 
       eUnqualifiedItems=add_ecref_to_tab nenv.eUnqualifiedItems;
       ePatItems = add_ecref_to_tab nenv.ePatItems }

let AddModuleAbbrevToNameEnv (id:ident) modrefs nenv = 
    {nenv with
       eModulesAndNamespaces=
         let add old nw = nw @ old
         NameMap.layerAdditive add (Map.add id.idText modrefs Map.empty) nenv.eModulesAndNamespaces }


//-------------------------------------------------------------------------
// Open a structure or an IL namespace 
//------------------------------------------------------------------------- 

let modrefs_of_mtyp modref (mty:ModuleOrNamespaceType) = 
  mty.ModulesAndNamespacesByDemangledName |> NameMap.map (MakeNestedTcref modref)

let foldIf pred f x acc = if pred x then f x acc else acc

// Recursive because of "AutoOpen", i.e. adding a module reference may automatically open further modules

let rec AddModrefsToNameEnv g amap m topRooted ad modrefs nenv =
    let AddModrefs modrefs tab = 
         let add old nw = 
             if IsEntityAccessible ad nw then  
                 if verbose then  dprintf "AddModrefs, nm = %s, #old = %d\n" (demangled_name_of_modref nw) (List.length old);
                 let isPartialNamespace = not topRooted && not nw.IsNamespace
                 ((* isPartialNamespace, *) nw) :: old
             else 
                 old
         NameMap.layerAdditive add modrefs tab
    let nenv = 
        {nenv with
           eModulesAndNamespaces= AddModrefs modrefs nenv.eModulesAndNamespaces;
           eFullyQualifiedModulesAndNamespaces =
             (if topRooted  
              then AddModrefs modrefs nenv.eFullyQualifiedModulesAndNamespaces
              else nenv.eFullyQualifiedModulesAndNamespaces) } 
    let nenv = 
        (nenv,NameMap.range modrefs) ||> List.fold (fun nenv modref ->  
            if modref.IsModule && TryFindBoolAttrib g g.attrib_AutoOpenAttribute modref.Attribs = Some(true) then
                AddModuleOrNamespaceContentsToNameEnv g amap ad m modref nenv
            else
                nenv)
    nenv

and AddModuleOrNamespaceContentsToNameEnv (g:TcGlobals) amap (ad:AccessorDomain) m (modref:ModuleOrNamespaceRef) nenv = 
    let mty = modref.ModuleOrNamespaceType
    let tycons = mty.TypeAndExceptionDefinitions
    let exncs = mty.ExceptionDefinitions
    let nenv = { nenv with eDisplayEnv= denv_add_open_modref modref nenv.eDisplayEnv }
    let nenv = List.foldBack (MakeNestedTcref modref >> foldIf (IsEntityAccessible ad) (AddExceptionDeclsToNameEnv)) exncs nenv
    let nenv = NameMap.foldRange (mk_vref_in_modref modref >> foldIf (IsValAccessible ad) (AddValRefToNameEnv)) mty.AllValuesAndMembers nenv
    let nenv = AddTyconRefsToNameEnv g amap m (tycons |> List.map (MakeNestedTcref modref) |> List.filter (IsEntityAccessible ad) ) nenv
    let modrefs = modrefs_of_mtyp modref mty
    let nenv = AddModrefsToNameEnv g amap m false ad modrefs nenv
    nenv

let AddModrefToNameEnv g amap m topRooted ad modref nenv =  
    AddModrefsToNameEnv g amap m topRooted ad (Map.add (demangled_name_of_modref modref) modref Map.empty) nenv

  
type CheckForDuplicateTyparFlag = 
    | CheckForDuplicateTypars 
    | NoCheckForDuplicateTypars

let AddDeclaredTyparsToNameEnv check typars nenv = 
    let typarmap = 
      List.foldBack 
        (fun (tp:Typar) sofar -> 
          begin match check with 
          | CheckForDuplicateTypars -> 
              if Map.mem tp.Name sofar then errorR (Duplicate("type parameter",tp.DisplayName,tp.Range))
          | NoCheckForDuplicateTypars -> 
              ()
          end;
          Map.add tp.Name tp sofar) typars Map.empty 
    {nenv with eTypars=NameMap.layer typarmap nenv.eTypars }

//--------------------------------------------------------------------------
// Lookup tables
//-------------------------------------------------------------------------- 

let tryname s t (id:ident) = 
    try Map.find id.idText t 
    with Not_found -> error (UndefinedName(0,s,id,NameMap.domainL t))

//-------------------------------------------------------------------------
// FreshenTycon and instantiationGenerator.  
//------------------------------------------------------------------------- 

let FreshenTycon (ncenv: NameResolver) m (tcref:TyconRef) = 
    let tinst = ncenv.instantiationGenerator m (tcref.Typars(m))
    TType_app(tcref,tinst)

let FreshenUnionCaseRef (ncenv: NameResolver) m (ucref:UnionCaseRef) = 
    let tinst = ncenv.instantiationGenerator m (ucref.TyconRef.Typars(m))
    UnionCaseInfo(tinst,ucref)

/// This must be called after fetching unqualified items that may need to be freshened
let FreshenUnqualifiedItem (ncenv: NameResolver) m res = 
    match res with 
    | Item_ucase (UnionCaseInfo(_,ucref)) -> Item_ucase (FreshenUnionCaseRef ncenv m ucref)
    | _ -> res


//-------------------------------------------------------------------------
// Resolve module paths, value, field etc. lookups.  Doing this involves
// searching through many possibilities and disambiguating.  Hence first
// define some ways of combining multiple results and for carrying
// error information.  Errors are generally undefined names and are
// reported by returning the error that occurs at greatest depth in the
// sequence of identifiers. 
//------------------------------------------------------------------------- 

// Accumulate a set of possible results. 
// If neither operations succeed, return an approximate error. 
// If one succeeds, return that one. 
// Prefer the error associated with the first argument. 
let OneResult res = 
    match res with 
    | Result x -> Result [x]
    | Exception e -> Exception e

let OneSuccess x = Result [x]

let AddResults res1 res2 =
    match res1, res2 with 
    | Result [],_ -> res2
    | _,Result [] -> res1
    | Result x,Result l -> Result (x @ l)
    | Exception _,Result l -> Result l
    | Result x,Exception _ -> Result x
     (* This prefers error messages coming from deeper failing long identifier paths *)
    | Exception (UndefinedName(n1,_,_,_) as e1),Exception (UndefinedName(n2,_,_,_) as e2) -> 
        if n1 < n2 then Exception e2 else Exception e1
    (* Prefer more concrete errors about things being undefined *)
    | Exception (UndefinedName(n1,_,_,_) as e1),Exception (Error _) -> Exception e1
    | Exception (Error _),Exception (UndefinedName(n1,_,_,_) as e2) -> Exception e2
    | Exception e1,Exception _ -> Exception e1

let (+++) x y = AddResults x y
let NoResultsOrUsefulErrors = Result []

// REVIEW: make this tail recursive
let rec CollectResults f = function
    | [] -> NoResultsOrUsefulErrors
    | [h] -> OneResult (f h)
    | h :: t -> AddResults (OneResult (f h)) (CollectResults f t)

let AtMostOneResult m res = 
    match res with 
    | Exception err -> raze err
    | Result [] -> raze (Error("invalid module/expression/type",m))
    | Result [res] -> success res
    | Result (res :: _) -> success res (* raze (Error("this module/expression/type is ambiguous",m))*)

//-------------------------------------------------------------------------
// Resolve (possibly mangled) type names 
//------------------------------------------------------------------------- 
 
/// Qualified lookups where the number of generic arguments is known 
/// from context, e.g. Module.Type<args>.  In theory the full names suh as ``List`1`` can 
/// be used to qualify access if needed 
let LookupTypeNameInEntityHaveArity nm ntyargs (mty:ModuleOrNamespaceType) = 
    if IsMangledGenericName nm || ntyargs = 0 then 
        mty.TypesByMangledName.TryFind nm
    else
        mty.TypesByMangledName.TryFind (nm^"`"^string ntyargs)

/// Unqualified lookups where the number of generic arguments is known 
/// from context, e.g. List<arg>.  Rebindings due to 'open' may have rebound identifiers.
let LookupTypeNameInEnvHaveArity nm ntyargs nenv = 
    if IsMangledGenericName nm then 
      nenv.eTyconsByDemangledNameAndArity.TryFind(DecodeGenericTypeName nm) 
      +?? (fun () -> nenv.eTyconsByAccessNames.TryFind nm |> Option.map List.hd)
    else 
      nenv.eTyconsByDemangledNameAndArity.TryFind(NameArityPair(nm,ntyargs)) 
      +?? (fun () -> nenv.eTyconsByAccessNames.TryFind nm |> Option.map List.hd)

/// Unqualified lookups where the number of generic arguments is NOT known 
/// from context. This is used in five places: 
///     -  static member lookups, e.g. MyType.StaticMember(3) 
///     -                         e.g. MyModule.MyType.StaticMember(3) 
///     -  type-qualified field names, e.g. { RecordType.field = 3 } 
///     -  type-qualified constructor names, e.g. match x with UnionType.A -> 3 
///     -  identifiers to constructors for better error messages, e.g. 'String(3)' after 'open System' 
///     -  the special single-constructor rule in tc_tycon_cores 
/// 
/// Because of the potential ambiguity multiple results can be returned. 
/// Explicit type annotations can be added where needed to specify the generic arity. 
///  
/// In theory the full names such as ``RecordType`1`` can 
/// also be used to qualify access if needed, though this is almost never needed.  

let LookupTypeNameNoArity nm byDemangledNameAndArity byAccessNames = 
    if IsMangledGenericName nm then 
      match Map.tryfind (DecodeGenericTypeName nm) byDemangledNameAndArity with 
      | Some res -> [res]
      | None -> 
          match Map.tryfind nm byAccessNames with
          | Some res -> res
          | None -> []
    else 
      NameMultiMap.find nm byAccessNames

let LookupTypeNameInEnvNoArity nm nenv = 
    LookupTypeNameNoArity nm nenv.eTyconsByDemangledNameAndArity nenv.eTyconsByAccessNames 

let LookupTypeNameInEntityNoArity m nm (mtyp:ModuleOrNamespaceType) = 
    LookupTypeNameNoArity nm (mtyp.TypesByDemangledNameAndArity(m)) mtyp.TypesByAccessNames 

type TypeNameInExprOrPatFlag = ResolveTypeNamesToCtors | ResolveTypeNamesToTypeRefs
type TypeNameResInfo = TypeNameInExprOrPatFlag * int option
let DefaultTypeNameResInfo = (ResolveTypeNamesToCtors,None) 


let LookupTypeNameInEnvMaybeHaveArity nm ((_,numTyargsOpt):TypeNameResInfo)  nenv = 
    match numTyargsOpt with 
    | None -> LookupTypeNameInEnvNoArity nm nenv
    | Some ntyargs -> LookupTypeNameInEnvHaveArity nm ntyargs nenv |> Option.to_list

let LookupTypeNameInEntityMaybeHaveArity ad m nm numTyargsOpt (modref: ModuleOrNamespaceRef) = 
    let mtyp = modref.ModuleOrNamespaceType
    let tycons = 
        match numTyargsOpt with 
        | None -> 
            LookupTypeNameInEntityNoArity m nm mtyp
        | Some ntyargs -> 
            LookupTypeNameInEntityHaveArity nm ntyargs mtyp |> Option.to_list
    tycons 
       |> List.map (MakeNestedTcref modref) 
       |> List.filter (IsEntityAccessible ad)


let GetNestedTypesOfType ad (ncenv:NameResolver) (optFilter,numTyargsOpt) m typ =
    let g = ncenv.g
    ncenv.InfoReader.ReadPrimaryTypeHierachy(m,typ) |> List.collect (fun typ -> 
        if is_stripped_tyapp_typ g typ then 
            let tcref,tinst = dest_stripped_tyapp_typ g typ
            let tycon = deref_tycon tcref
            let mty = tycon.ModuleOrNamespaceType
            // Handle the .NET/C# business where nested generic types implictly accumulate the type parameters 
            // from their enclosing types.
            let MakeNestedType (tcrefNested:TyconRef) = 
                let _,tps = List.chop tinst.Length (tcrefNested.Typars(m))
                let tinstNested = ncenv.instantiationGenerator m tps
                mk_tyapp_ty tcrefNested (tinst @ tinstNested)

            match optFilter with 
            | Some nm -> 
                LookupTypeNameInEntityMaybeHaveArity ad m nm numTyargsOpt tcref 
                    |> List.map MakeNestedType 
            | None -> 
                mty.TypesByAccessNames 
                    |> NameMultiMap.range 
                    |> List.map (MakeNestedTcref tcref >> MakeNestedType)
                    |> List.filter (IsTypeAccessible g ad)
        else [])

//-------------------------------------------------------------------------
// Report environments to visual studio. We stuff intermediary results 
// into a global variable. A little unpleasant. 
// REVIEW: We could at least put the global in cenv!!!
//------------------------------------------------------------------------- 

// Represents a type of the occurence when reporting name in name resolution
type ItemOccurence = 
  // This is a binding / declaration of the item
  | Binding = 0
  // This is a usage of the item
  | Use = 1
  // Inside pattern matching
  | Pattern = 2
  
type ITypecheckResultsSink =
    abstract NotifyEnvWithScope : range * NameResolutionEnv * AccessorDomain -> unit
    abstract NotifyExprHasType : pos * typ * Tastops.DisplayEnv * NameResolutionEnv * AccessorDomain * range -> unit
    abstract NotifyNameResolution : pos * NamedItem * ItemOccurence * Tastops.DisplayEnv * NameResolutionEnv * AccessorDomain * range -> unit

let GlobalTypecheckResultsSink : ITypecheckResultsSink option ref  = ref None

let CallEnvSink(scopem,nenv,ad) = 
    match !GlobalTypecheckResultsSink with 
    | None -> () 
    | Some sink -> sink.NotifyEnvWithScope(scopem,nenv,ad)

let CallNameResolutionSink(m,nenv,item,occurenceType,denv,ad) = 
    match !GlobalTypecheckResultsSink with 
    | None -> () 
    | Some sink -> sink.NotifyNameResolution(end_of_range m,item,occurenceType,denv,nenv,ad,m)  

let CallExprHasTypeSink(m,nenv,typ,denv,ad) = 
    match !GlobalTypecheckResultsSink with 
    | None -> () 
    | Some sink -> sink.NotifyExprHasType(end_of_range m,typ,denv,nenv,ad,m)


/// Checks if the type variables associated with the result of a resolution are inferrable,
/// i.e. occur in the arguments or return type of the resolution. If not give a warning
/// about a type instantiation being needed.
type ResultTyparChecker = unit -> bool

let CheckAllTyparsInferrable g amap m  item = 
    match item with
    | Item_val v -> true
    | Item_apelem apref -> true
    | Item_ucase(ucr) -> true
    | Item_ecref(ecr) -> true
    | Item_recdfield(rfinfo) -> true
    | Item_newdef(id) -> true
    
    | Item_il_field(finfo) -> true
    | Item_event(einfo) -> true

    | Item_property(nm,pinfos) -> 
        pinfos |> List.forall (fun pinfo -> 
            let freeInEnclosingType = free_in_type CollectTyparsNoCaching pinfo.EnclosingType
            let freeInArgsAndRetType = 
                acc_free_in_types CollectTyparsNoCaching (ParamTypesOfPropInfo amap m pinfo) 
                       (free_in_type CollectTyparsNoCaching (PropertyTypeOfPropInfo amap m pinfo))
            let free = Zset.diff freeInEnclosingType.FreeTypars  freeInArgsAndRetType.FreeTypars
            free.IsEmpty)

    | Item_meth_group(nm,minfos) -> 
        minfos |> List.forall (fun minfo -> 
            let fminst = minfo.FormalMethodInst
            let freeInEnclosingType = free_in_type CollectTyparsNoCaching minfo.EnclosingType
            let freeInArgsAndRetType = 
                List.foldBack (acc_free_in_types CollectTyparsNoCaching) (ParamTypesOfMethInfo amap m minfo fminst) 
                   (acc_free_in_types CollectTyparsNoCaching (ObjTypesOfMethInfo amap m minfo fminst) 
                       (free_in_type CollectTyparsNoCaching (FSharpReturnTyOfMeth amap m minfo fminst)))
            let free = Zset.diff freeInEnclosingType.FreeTypars  freeInArgsAndRetType.FreeTypars
            free.IsEmpty)

    | Item_ctor_group(nm,_) -> true
    | Item_fake_intf_ctor typ 
    | Item_delegate_ctor typ -> true
    | Item_typs(nm,tcref) -> true
    | Item_modrefs(modref :: _) -> true
    | Item_param_name(id) -> true
    | Item_prop_name(id) -> true
    | _ ->  true
    
/// Keeps track of information relevant to the chosen resolution of a long identifier
///
/// When we resolve an item such as System.Console.In we
/// resolve it in one step to a property/val/method etc. item. However
/// Visual Studio needs to know about the exact resolutions of the names
/// System and Console, i.e. the 'entity path' of the resolution. 
///
/// Each of the resolution routines keeps track of the entity path and 
/// ultimately calls ResolutionInfo.SendToSink to record it for 
/// later use by Visual Studio.
type ResolutionInfo = 
    | ResolutionInfo of (*entityPath, reversed*)(range * EntityRef) list * (*warnings/errors*)(ResultTyparChecker -> unit)

    static member SendToSink(ncenv: NameResolver,nenv,ad,ResolutionInfo(entityPath,warnings),typarChecker) = 
        entityPath |> List.iter (fun (m,eref:EntityRef) -> 
            CheckEntityAttributes ncenv.g eref m |> CommitOperationResult;        
            CheckTyconAccessible m ad eref |> ignore;
            let item = if eref.IsModuleOrNamespace then Item_modrefs([eref]) else Item_typs(eref.DisplayName,[FreshenTycon ncenv m eref])
            CallNameResolutionSink(m,nenv,item,ItemOccurence.Use,nenv.eDisplayEnv,ad))
        warnings(typarChecker)
 
    static member Empty = 
        ResolutionInfo([],(fun typarChecker -> ()))

    member x.AddEntity info = 
        let (ResolutionInfo(entityPath,warnings)) = x
        ResolutionInfo(info::entityPath,warnings)

    member x.AddWarning f = 
        let (ResolutionInfo(entityPath,warnings)) = x
        ResolutionInfo(entityPath,(fun typarChecker -> f typarChecker; warnings typarChecker))



let CheckForMultipleGenericTypeAmbiguities (tcrefs:(ResolutionInfo * TyconRef) list) ((typeNameResFlag,numTyargsOpt):TypeNameResInfo) m = 
    // Given ambiguous C<>, C<_>    we resolve the ambiguous 'C.M' to C<> without warning
    // Given ambiguous C<_>, C<_,_> we resolve the ambiguous 'C.M' to C<_> with an ambiguity error
    // Given C<_>                   we resolve the ambiguous 'C.M' to C<_> with a warning if the argument or return types can't be inferred

    // Given ambiguous C<>, C<_>    we resolve the ambiguous 'C()' to C<> without warning
    // Given ambiguous C<_>, C<_,_> we resolve the ambiguous 'C()' to C<_> with an ambiguity error
    // Given C<_>                   we resolve the ambiguous 'C()' to C<_> with a warning if the argument or return types can't be inferred

    let tcrefs = 
        tcrefs 
        // remove later duplicates (if we've opened the same module more than once)
        |> Seq.distinct_by (fun (_,tcref) -> tcref.Stamp) 
        |> Seq.to_list                     
        // List.sort_by is a STABLE sort (the order matters!)
        |> List.sort_by (fun (_,tcref) -> tcref.Typars(m).Length)

    match tcrefs with 
    | ((resInfo,tcref) :: _) when 
            // multiple types
            tcrefs.Length > 1 && 
            // no explicit type instantiation
            isNone numTyargsOpt && 
            // some type arguments required on all types (note sorted by typar count above)
            tcref.Typars(m).Length > 0 && 
            // plausible types have different arities
            (tcrefs |> Seq.distinct_by (fun (_,tcref) -> tcref.Typars(m).Length) |> Seq.length > 1)  ->
        [ for (resInfo,tcref) in tcrefs do 
            let resInfo = resInfo.AddWarning (fun typarChecker -> errorR(Error(sprintf "Multiple types exist called '%s', taking different numbers of generic parameters. Provide a type instantiation to disambiguate the type resolution, e.g. '%s'" tcref.DisplayName tcref.DisplayNameWithUnderscoreTypars,m)))
            yield (resInfo,tcref) ]

    | [(resInfo,tcref)] when isNone numTyargsOpt && tcref.Typars(m).Length > 0 && typeNameResFlag = ResolveTypeNamesToTypeRefs ->
        let resInfo = 
            resInfo.AddWarning (fun typarChecker -> 
                if not (typarChecker()) then 
                    warning(Error(sprintf "The instantiation of the generic type '%s' is missing and can't be inferred from the arguments or return type of this member. Consider providing a type instantiation when accessing this type, e.g. '%s'" tcref.DisplayName tcref.DisplayNameWithUnderscoreTypars,m)))
        [(resInfo,tcref)]

    | _ -> 
        tcrefs
    


//-------------------------------------------------------------------------
// Consume ids that refer to a namespace
//------------------------------------------------------------------------- 

type FullyQualifiedFlag = 
    // Only resolve full paths.
    // This is urrently unused, but would be needed for a fully-qualified syntax, so leaving it in the code
    | FullyQualified 
    | OpenQualified 

let ResolveLongIndentAsModuleOrNamespace fullyQualified (nenv:NameResolutionEnv) ad (lid:ident list) =
    match lid with 
    | [] -> NoResultsOrUsefulErrors
    | id:: rest -> 
        let tab = 
            match fullyQualified with 
            | FullyQualified -> nenv.eFullyQualifiedModulesAndNamespaces 
            | OpenQualified -> nenv.eModulesAndNamespaces

        match tab.TryFind(id.idText) with
        | Some modrefs -> 
            
            /// Look through the sub-namespaces and/or modules
            let rec look depth  modref (mty:ModuleOrNamespaceType) (lid:ident list) =
                match lid with 
                | [] -> success (depth,modref,mty)
                | id:: rest ->
                    match mty.ModulesAndNamespacesByDemangledName.TryFind(id.idText) with
                    | Some mspec when IsEntityAccessible ad (MakeNestedTcref modref mspec) -> 
                        let subref = MakeNestedTcref modref mspec
                        look (depth+1) subref mspec.ModuleOrNamespaceType rest
                    | _ -> raze (UndefinedName(depth,"namespace",id,[]))

            modrefs |> CollectResults (fun modref -> 
                if IsEntityAccessible ad modref then 
                    look 1 modref modref.ModuleOrNamespaceType rest
                else 
                    raze (UndefinedName(0,"namespace or module",id,[]))) 
        | None -> 
            raze (UndefinedName(0,"namespace or module",id,[]))


let ResolveLongIndentAsModuleOrNamespaceThen fullyQualified (nenv:NameResolutionEnv) ad lid f =
    match lid with 
    | [] -> NoResultsOrUsefulErrors
    | id :: rest -> 
        match ResolveLongIndentAsModuleOrNamespace fullyQualified nenv ad [id] with
        |  Result modrefs -> 
              modrefs |> CollectResults (fun (depth,modref,mty) ->  
                  let resInfo = ResolutionInfo.Empty.AddEntity(id.idRange,modref) 
                  f resInfo (depth+1) id.idRange modref mty rest) 
        |  Exception err -> Exception err 

//-------------------------------------------------------------------------
// Bind name used in "new Foo.Bar(...)" constructs
//------------------------------------------------------------------------- 

let private ResolveObjectConstructorPrim (ncenv:NameResolver) edenv resInfo m ad typ = 
    let g = ncenv.g
    let amap = ncenv.amap
    if verbose then   dprintf "--> ResolveObjectConstructor\n"; 
    if is_delegate_typ g typ then 
        success (resInfo,Item_delegate_ctor typ,[])
    else 
        let cinfos =  GetIntrinsicConstructorInfosOfType ncenv.InfoReader m typ
        if is_interface_typ g typ && isNil cinfos then 
            success (resInfo,Item_fake_intf_ctor typ, [])
        else 
            let defaultStructCtorInfo = 
                if (is_struct_typ g typ && not(cinfos |> List.exists minfo_is_nullary)) then 
                    [DefaultStructCtor(g,typ)] 
                else []
            if verbose then   dprintf "--> ResolveObjectConstructor (2)\n"; 
            if (isNil defaultStructCtorInfo && isNil cinfos) or not (is_stripped_tyapp_typ g typ) then 
                raze (Error("No constructors are available for the type '"^NicePrint.pretty_string_of_typ edenv typ^"'",m))
            else 
                let cinfos = cinfos |> List.filter (IsMethInfoAccessible amap m ad)  
                success (resInfo,MakeCtorGroup ((tcref_of_stripped_typ g typ).MangledName, (defaultStructCtorInfo@cinfos)),[]) 

let ResolveObjectConstructor (ncenv:NameResolver) edenv m ad typ = 
    ResolveObjectConstructorPrim (ncenv:NameResolver) edenv [] m ad typ  |?> (fun (resInfo,item,rest) -> (item,rest))

//-------------------------------------------------------------------------
// Bind IL "." notation (member lookup or lookup in a type)
//------------------------------------------------------------------------- 

let IntrinsicPropInfosOfTypeInScope (infoReader:InfoReader) (optFilter, ad) findFlag m typ =
    let g = infoReader.g
    let amap = infoReader.amap
    let pinfos = GetIntrinsicPropInfoSetsOfType infoReader (optFilter, ad) findFlag m typ
    let pinfos = pinfos |> ExcludeHiddenOfPropInfos g amap m 
    pinfos

let ExtensionPropInfosOfTypeInScope (infoReader:InfoReader) eExtensionMembers (optFilter, ad) findFlag m typ =
    let g = infoReader.g
    let amap = infoReader.amap
    infoReader.ReadEntireTypeHierachy(m,typ) |> List.collect (fun typ -> 
         if (is_stripped_tyapp_typ g typ) then 
            let tcref = tcref_of_stripped_typ g typ
            (* NOTE: multiple "open"'s push multiple duplicate values into eExtensionMembers *)
            (* REVIEW: this looks a little slow: ListSet.setify is quadratic. *)
            let extValRefs = ListSet.setify (ExtensionMember.Equality g) (tcref_mmap_find tcref eExtensionMembers)
            let propCollector = new PropertyCollector(g,amap,m,typ,optFilter,ad)
            extValRefs |> List.iter (fun emem ->
                match emem with 
                | FSExtMem vref -> 
                   match vref.MemberInfo with 
                   | None -> ()
                   | Some(membInfo) -> propCollector.Collect(membInfo,vref)
                | ILExtMem _ -> 
                   // No extension properties coming from .NET
                   ())
            propCollector.Close()
         else [])

let AllPropInfosOfTypeInScope infoReader eExtensionMembers (optFilter, ad) findFlag m typ =
    IntrinsicPropInfosOfTypeInScope infoReader (optFilter, ad) findFlag m typ
    @ ExtensionPropInfosOfTypeInScope infoReader eExtensionMembers (optFilter, ad) findFlag m typ 

let IntrinsicMethInfosOfType (infoReader:InfoReader) (optFilter,ad) findFlag m typ =
    let g = infoReader.g
    let amap = infoReader.amap
    let minfos = GetIntrinsicMethInfoSetsOfType infoReader (optFilter,ad) findFlag m typ
    let minfos = minfos |> ExcludeHiddenOfMethInfos g amap m
    minfos

let ImmediateExtensionMethInfosOfTypeInScope (infoReader:InfoReader) eExtensionMembers (optFilter,ad) findFlag m typ =
    let g = infoReader.g
    if (is_stripped_tyapp_typ g typ) then 
        let tcref = tcref_of_stripped_typ g typ
        // NOTE: multiple "open"'s push multiple duplicate values into eExtensionMembers 
        // REVIEW: this looks a little slow: ListSet.setify is quadratic. 
        let extValRefs = ListSet.setify (ExtensionMember.Equality g) (tcref_mmap_find tcref eExtensionMembers)
        extValRefs |> List.choose (fun emem -> 
                match emem with 
                | FSExtMem vref -> 
                    match vref.MemberInfo with 
                    | None -> None
                    | Some(membInfo) -> TrySelectMemberVal g optFilter typ membInfo vref
                | ILExtMem (actualParent,md) when (match optFilter with None -> true | Some(nm) -> nm = md.mdName) ->
                    // 'typ' is the logical parent 
                    let tinfo = tinfo_of_il_typ g typ
                    Some(mk_il_minfo infoReader.amap m tinfo (Some(actualParent)) md)
                | _ -> 
                    None) 
    else []

let ExtensionMethInfosOfTypeInScope (infoReader:InfoReader) eExtensionMembers (optFilter,ad) findFlag m typ =
    let g = infoReader.g
    infoReader.ReadEntireTypeHierachy(m,typ) |> List.collect (fun typ -> 
        ImmediateExtensionMethInfosOfTypeInScope infoReader eExtensionMembers (optFilter,ad) findFlag m typ) 

let AllMethInfosOfTypeInScope infoReader eExtensionMembers (optFilter,ad) findFlag m typ =
    IntrinsicMethInfosOfType infoReader (optFilter,ad) findFlag m typ 
    @ ExtensionMethInfosOfTypeInScope infoReader eExtensionMembers (optFilter,ad) findFlag m typ          


exception IndeterminateType of range

type LookupKind = 
   | RecdField
   | Pattern
   | Expr
   | Type
   | Ctor


let TryFindUnionCaseOfType g typ nm =
    if is_stripped_tyapp_typ g typ then 
        let tcref,tinst = dest_stripped_tyapp_typ g typ
        match tcref.GetUnionCaseByName nm with 
        | None -> None
        | Some ucase -> Some(UnionCaseInfo(tinst,ucref_of_ucase tcref ucase))
    else 
        None
   
// REVIEW: this shows up on performance logs. Consider for example endles resolutions  of "List.map" to 
// the empty set of results, or "x.Length" for a list or array type. This indicates it could be worth adding a cache here.
let rec ResolveLongIdentInTypePrim (ncenv:NameResolver) nenv lookupKind (resInfo:ResolutionInfo) depth m ad (lid:ident list) findFlag typeNameResInfo typ =
    let g = ncenv.g
    match lid with 
    | [] -> error(InternalError("ResolveLongIdentInTypePrim",m))
    | id :: rest -> 
        let nm = id.idText // used to filter the searches of the tables 
        let optFilter = Some(nm) // used to filter the searches of the tables 
        let contentsSearchAccessible = 
           let unionCaseSearch = 
               if (match lookupKind with Expr | Pattern -> true | _ -> false) then 
                   TryFindUnionCaseOfType g typ nm 
               else 
                   None
           // Lookup: datatype constructors take precedence 
           match unionCaseSearch with 
           | Some ucase -> 
               success(resInfo,Item_ucase(ucase),rest)
           | None -> 
                match TryFindIntrinsicNamedItemOfType ncenv.InfoReader (nm,ad) findFlag m typ with
                | Some (PropertyItem psets) when (match lookupKind with Expr  -> true | _ -> false) -> 
                    let pinfos = psets |> ExcludeHiddenOfPropInfos g ncenv.amap m
                    let item = 
                        match pinfos with 
                        | [pinfo] when pinfo.IsFSharpEventProperty -> 
                            let minfos1 = GetImmediateIntrinsicMethInfosOfType (Some("add_"^nm),ad) g ncenv.amap m typ 
                            let minfos2 = GetImmediateIntrinsicMethInfosOfType (Some("remove_"^nm),ad) g ncenv.amap m typ 
                            match  minfos1,minfos2 with 
                            | [FSMeth(_,_,addValRef)],[FSMeth(_,_,removeValRef)] -> 
                                // FOUND PROPERTY-AS-EVENT AND CORRESPONDING ADD/REMOVE METHODS
                                Item_event(FSEvent(g,pinfo,addValRef,removeValRef))
                            | _ -> 
                                // FOUND PROPERTY-AS-EVENT BUT DIDN'T FIND CORRESPONDING ADD/REMOVE METHODS
                                Item_property (nm,pinfos)
                        | _ -> 
                            Item_property (nm,pinfos)


                    success (resInfo,item,rest) 

                | Some(MethodItem msets) when (match lookupKind with Expr  -> true | _ -> false) -> 
                    let minfos = msets |> ExcludeHiddenOfMethInfos g ncenv.amap m
                    success (resInfo,MakeMethGroup (nm,minfos),rest)

                | Some (ILFieldItem (finfo:: _))  when (match lookupKind with Expr | Pattern -> true | _ -> false) -> 
                    success (resInfo,Item_il_field finfo,rest)

                | Some (ILEventItem (einfo :: _)) when (match lookupKind with Expr -> true | _ -> false)  -> 
                    success (resInfo,Item_event (ILEvent(g,einfo)),rest)
                | Some (RecdFieldItem (rfinfo)) when (match lookupKind with Expr | RecdField | Pattern -> true | _ -> false) -> 
                    success(resInfo,Item_recdfield(rfinfo),rest)
                | _ ->
                let pinfos = ExtensionPropInfosOfTypeInScope ncenv.InfoReader nenv.eExtensionMembers (optFilter, ad) findFlag m typ
                if nonNil(pinfos) && (match lookupKind with Expr -> true | _ -> false)  then 
                    success (resInfo,Item_property (nm,pinfos),rest) else
                
                let minfos = ExtensionMethInfosOfTypeInScope ncenv.InfoReader nenv.eExtensionMembers (optFilter,ad) findFlag m typ
                if nonNil(minfos) && (match lookupKind with Expr -> true | _ -> false) then 
                    success (resInfo,MakeMethGroup (nm,minfos),rest) else 
                
                if is_typar_typ g typ then raze (IndeterminateType(union_ranges m id.idRange))
                else raze (UndefinedName (depth,"field, constructor or member", id,[]))
              
        let nestedSearchAccessible = 
            let nestedTypes = GetNestedTypesOfType ad ncenv (Some nm,(if isNil rest then snd typeNameResInfo else None)) m typ
            let typeNameResFlag,numTyargsOpt = typeNameResInfo
            if isNil rest then 
                if isNil nestedTypes then 
                    NoResultsOrUsefulErrors
                else 
                    match typeNameResFlag with 
                    | ResolveTypeNamesToCtors -> 
                        nestedTypes |> CollectResults (ResolveObjectConstructorPrim ncenv nenv.eDisplayEnv resInfo m ad) 
                    | ResolveTypeNamesToTypeRefs -> 
                        OneSuccess (resInfo,Item_typs (nm,nestedTypes),rest)
            else 
                ResolveLongIdentInTypes ncenv nenv lookupKind resInfo (depth+1) m ad rest findFlag typeNameResInfo nestedTypes
        (OneResult contentsSearchAccessible +++ nestedSearchAccessible)
        
and ResolveLongIdentInTypes (ncenv:NameResolver) nenv lookupKind resInfo depth m ad lid findFlag typeNameResInfo typs = 
    typs |> CollectResults (ResolveLongIdentInTypePrim ncenv nenv lookupKind resInfo depth m ad lid findFlag typeNameResInfo >> AtMostOneResult m) 

let ResolveLongIdentInType ncenv nenv lookupKind m ad lid findFlag typeNameResInfo typ =
    let resInfo,item,rest = 
        ResolveLongIdentInTypePrim (ncenv:NameResolver) nenv lookupKind ResolutionInfo.Empty 0 m ad lid findFlag typeNameResInfo typ
        |> AtMostOneResult m
        |> ForceRaise
    ResolutionInfo.SendToSink(ncenv,nenv,ad,resInfo,(fun () -> CheckAllTyparsInferrable ncenv.g ncenv.amap m item));
    item,rest

// QUERY (instantiationGenerator cleanup): it would be really nice not to flow instantiationGenerator to here. 
// This would help make it the separation between name resolution and 
// type inference more obvious. However this would mean each caller 
// would have to freshen. 
let private ResolveLongIdentInTyconRef (ncenv:NameResolver) nenv lookupKind resInfo depth m ad lid typeNameResInfo tcref =
    let typ = (FreshenTycon ncenv m tcref)
    typ |> ResolveLongIdentInTypePrim ncenv nenv lookupKind resInfo depth m ad lid IgnoreOverrides typeNameResInfo  

let private ResolveLongIdentInTyconRefs (ncenv:NameResolver) nenv lookupKind depth m ad lid typeNameResInfo idRange tcrefs = 
    // The basic search
    tcrefs |> CollectResults (fun (resInfo:ResolutionInfo,tcref) -> 
        let resInfo = resInfo.AddEntity(idRange,tcref)
        tcref |> ResolveLongIdentInTyconRef ncenv nenv lookupKind resInfo depth m ad lid typeNameResInfo |> AtMostOneResult m) 

//-------------------------------------------------------------------------
// ResolveExprLongIdentInModuleOrNamespace 
//------------------------------------------------------------------------- 

let (|AccessibleEntityRef|_|) ad modref mspec = 
    let eref = MakeNestedTcref modref mspec
    if IsEntityAccessible ad eref then Some(eref) else None


let rec ResolveExprLongIdentInModuleOrNamespace (ncenv:NameResolver) nenv typeNameResInfo ad resInfo depth m modref (mty:ModuleOrNamespaceType) (lid :ident list) =
    // resInfo records the modules or namespaces actually relevant to a resolution
    let g = ncenv.g
    match lid with 
    | [] -> raze (InternalError("ResolveExprLongIdentInModuleOrNamespace",m))
    | id :: rest ->
        match mty.AllValuesAndMembers.TryFind(id.idText) with
        | Some vspec when IsValAccessible ad (mk_vref_in_modref modref vspec) -> 
            success(resInfo,Item_val (mk_vref_in_modref modref vspec),rest)
        | _->
        match  TryFindTypeWithUnionCase modref id with
        | Some tycon when IsTyconReprAccessible ad (MakeNestedTcref modref tycon) -> 
            let ucref = mk_ucref (MakeNestedTcref modref tycon) id.idText 
            let ucinfo = FreshenUnionCaseRef ncenv m ucref
            success (resInfo,Item_ucase ucinfo,rest)
        | _ -> 
        match mty.ExceptionDefinitionsByDemangledName.TryFind(id.idText) with
        | Some excon when IsTyconReprAccessible ad (MakeNestedTcref modref excon) -> 
            success (resInfo,Item_ecref (MakeNestedTcref modref excon),rest)
        | _ ->

            (* Something in a type? *)
            let tyconSearch = 
                let tcrefs = LookupTypeNameInEntityMaybeHaveArity ad id.idRange id.idText (if nonNil rest then None else snd typeNameResInfo) modref  
                let tcrefs = tcrefs |> List.map (fun tcref -> (resInfo,tcref))
                if (nonNil rest) then 
                    let tcrefs = CheckForMultipleGenericTypeAmbiguities tcrefs (ResolveTypeNamesToTypeRefs,None) m 
                    ResolveLongIdentInTyconRefs ncenv nenv  LookupKind.Expr (depth+1) m ad rest typeNameResInfo id.idRange tcrefs
                (* check if we've got some explicit type arguments *)
                else 
                    let tcrefs = CheckForMultipleGenericTypeAmbiguities tcrefs typeNameResInfo m 
                    match fst typeNameResInfo with 
                    | ResolveTypeNamesToTypeRefs -> 
                        success [ for (resInfo,tcref) in tcrefs do 
                                      let typ = FreshenTycon ncenv m tcref
                                      let item = (resInfo,Item_typs(id.idText,[typ]),[])
                                      yield item ]
                    | ResolveTypeNamesToCtors -> 
                        let typs = tcrefs |> List.map (fun (resInfo, tcref) -> resInfo, FreshenTycon ncenv m tcref) 
                        typs |> CollectResults (fun (resInfo,typ) -> ResolveObjectConstructorPrim ncenv nenv.eDisplayEnv resInfo id.idRange ad typ) 

            (* Something in a sub-namespace or sub-module *)
            let moduleSearch = 
                if (nonNil rest) then 
                    match mty.ModulesAndNamespacesByDemangledName.TryFind(id.idText) with
                    | Some(AccessibleEntityRef ad modref submodref) -> 
                        let resInfo = resInfo.AddEntity(id.idRange,submodref)

                        OneResult (ResolveExprLongIdentInModuleOrNamespace ncenv nenv typeNameResInfo ad resInfo (depth+1) m submodref submodref.ModuleOrNamespaceType rest)
                    | _ -> 
                        NoResultsOrUsefulErrors
                else 
                    NoResultsOrUsefulErrors

            AtMostOneResult id.idRange ( tyconSearch +++   moduleSearch +++ raze (UndefinedName(depth,"value, constructor, namespace or type",id,[])))


//-------------------------------------------------------------------------

/// Resolve F# "A.B.C" syntax in expressions
/// Not all of the sequence will necessarily be swallowed, i.e. we return some identifiers 
/// that may represent further actions, e.g. further lookups. 

let ResolveExprLongIdent (ncenv:NameResolver) m ad nenv typeNameResInfo lid =
    let g = ncenv.g
    let resInfo = ResolutionInfo.Empty
    match lid with 
    | [] -> error (Error("invalid expression: "^text_of_lid lid, m))
    | [id] ->
          // Single identifier.  This is the basic rule: lookup the environment! simple enough 
          match nenv.eUnqualifiedItems.TryFind(id.idText) with
          | Some res -> 
              FreshenUnqualifiedItem ncenv m res, []
          | None -> 
              // Check if it's a type name, e.g. a constructor call or a type instantiation 
              let ctorSearch = 
                  let tcrefs = LookupTypeNameInEnvMaybeHaveArity id.idText typeNameResInfo nenv
                  let tcrefs = tcrefs |> List.map (fun tcref -> (resInfo,tcref))
                  let tcrefs = CheckForMultipleGenericTypeAmbiguities tcrefs typeNameResInfo m 
                  match fst typeNameResInfo with 
                  | ResolveTypeNamesToCtors ->
                      let tcrefs = tcrefs |> List.filter (fun (_,tcref) -> tcref.IsILTycon || tcref.IsFSharpObjectModelTycon)
                      let typs = tcrefs |> List.map (fun (resInfo,tcref) -> (resInfo,FreshenTycon ncenv m tcref)) 
                      typs 
                          |> CollectResults (fun (resInfo,typ) -> ResolveObjectConstructorPrim ncenv nenv.eDisplayEnv resInfo id.idRange ad typ) 
                  | ResolveTypeNamesToTypeRefs ->
                      let typs = tcrefs |> List.map (fun (resInfo,tcref) -> (resInfo,FreshenTycon ncenv m tcref)) 
                      success (typs |> List.map (fun (resInfo,typ) -> (resInfo,Item_typs(id.idText,[typ]),[])))


              let implicitOpSearch = 
                  if IsMangledOpName id.idText then 
                      success [(resInfo,Item_implicit_op id,[])] 
                  else NoResultsOrUsefulErrors

              let failingCase = raze (UndefinedName(0,"value or constructor",id,[]))
              let search = ctorSearch +++ implicitOpSearch +++ failingCase
              let resInfo,item,rest = ForceRaise (AtMostOneResult m search)
              ResolutionInfo.SendToSink(ncenv,nenv,ad,resInfo,(fun () -> CheckAllTyparsInferrable ncenv.g ncenv.amap m item));
              item,rest
              
            
    // A compound identifier. 
    // It still might be a value in the environment, or something in an F# module, namespace, typ, or nested type 
    | id :: rest -> 
    
        // Values in the environment take total priority, but contructors do NOT for compound lookups, e.g. if someone in some imported  
        // module has defined a constructor "String" (common enough) then "String.foo" doesn't give an error saying 'constructors have no members' 
        // Instead we go lookup the String module or type.
        let ValIsInEnv nm = 
            (match nenv.eUnqualifiedItems.TryFind(nm) with Some(Item_val _) -> true | _ -> false)

        if ValIsInEnv id.idText &&
           (* Workaround for bug 908: adding "int", "float" etc. as functions has broken their use as types, and now System.Int32 etc. have *)
           (* to be used instead.  Here we are friendly and allow them to be used as types and just give a warning instead. *)
           (* Here we check that the thing being referenced is indeed a function value, and we know that we're doing a "int.Foo" lookup *)
           (* which doesn't make sense on a function value, so we give a warning and revert to the old interpretation. *)
           match nenv.eUnqualifiedItems.[id.idText] with 
           | Item_val vref when 
                (let nm = vref.MangledName
                 (match nm with "string" | "int" | "float" | "float32" | "single" | "double" | "sbyte" | "byte" | "int16" | "uint16" | "int32" | "uint32"  -> true | _ -> false)
                 && let _,tau = vref.TypeScheme in is_fun_typ g tau)
                 -> 
                     warning(Error(sprintf "The standard definition of the identifier '%s' is now a function used to convert values to type '%s', or will be in a future release. Access static methods via the canonical uppercase name for the type, e.g. replace 'int.Parse(...)' with 'System.Int32.Parse(...)'" vref.MangledName vref.MangledName,id.idRange));
                     false
           | _ -> true
        then
              nenv.eUnqualifiedItems.[id.idText], rest
        else
          // Otherwise modules are searched first. REVIEW: modules and types should be searched together. 
          // For each module referenced by 'id', search the module as if it were an F# module and/or a .NET namespace. 
          let moduleSearch ad = 
               ResolveLongIndentAsModuleOrNamespaceThen OpenQualified nenv ad lid 
                   (ResolveExprLongIdentInModuleOrNamespace ncenv nenv typeNameResInfo ad)

          // REVIEW: somewhat surprisingly, this shows up on performance traces, with tcrefs non-nil.
          // This seems strange since we would expect in the vast majority of cases tcrefs is empty here.
          let tyconSearch ad = 
              let tcrefs = LookupTypeNameInEnvNoArity id.idText nenv
              let tcrefs = tcrefs |> List.map (fun tcref -> (resInfo,tcref))
              let tcrefs  = CheckForMultipleGenericTypeAmbiguities tcrefs (ResolveTypeNamesToTypeRefs,None) m 
              ResolveLongIdentInTyconRefs ncenv nenv LookupKind.Expr 1 m ad rest typeNameResInfo id.idRange tcrefs

          let envSearch = 
              match Map.tryfind id.idText nenv.eUnqualifiedItems with
              | Some res -> OneSuccess (resInfo,FreshenUnqualifiedItem ncenv m res,rest)
              | None -> NoResultsOrUsefulErrors

          let search = moduleSearch ad +++ tyconSearch ad +++ envSearch 

          let resInfo,item,rest = 
              match AtMostOneResult m search with 
              | Result _ as res -> 
                  ForceRaise res
              | _ ->  
                  let failingCase = raze (UndefinedName(0,"value, namespace, type or module",id,[]))               
                  ForceRaise (AtMostOneResult m (search +++ moduleSearch AccessibleFromSomeFSharpCode +++ tyconSearch AccessibleFromSomeFSharpCode +++ failingCase))
          ResolutionInfo.SendToSink(ncenv,nenv,ad,resInfo,(fun () -> CheckAllTyparsInferrable ncenv.g ncenv.amap m item));
          item,rest


//-------------------------------------------------------------------------
// Resolve F#/IL "." syntax in patterns
//------------------------------------------------------------------------- 

let rec ResolvePatternLongIdentInModuleOrNamespace (ncenv:NameResolver) nenv numTyArgsOpt ad resInfo depth m modref (mty:ModuleOrNamespaceType) lid =
    let g = ncenv.g
    if verbose then  dprintf "--> ResolvePatternLongIdentInModuleOrNamespace edenv, lid = %s@%a\n" (text_of_lid lid) output_range m ;
    match lid with 
    | [] -> raze (Error("ResolvePatternLongIdentInModuleOrNamespace edenv",m))
    | id :: rest ->
        match TryFindTypeWithUnionCase modref id with
        | Some tycon when IsTyconReprAccessible ad (MakeNestedTcref modref tycon) -> 
            let tcref = MakeNestedTcref modref tycon
            let ucref = mk_ucref tcref id.idText
            let ucinfo = FreshenUnionCaseRef ncenv m ucref
            success (resInfo,Item_ucase ucinfo,rest)
        | _ -> 
        match mty.ExceptionDefinitionsByDemangledName.TryFind(id.idText) with
        | Some exnc when IsEntityAccessible ad (MakeNestedTcref modref exnc) -> 
            success (resInfo,Item_ecref (MakeNestedTcref modref exnc),rest)
        | _ ->
        // An active pattern constructor in a module 
        match (ActivePatternElemsOfModuleOrNamespace modref).TryFind(id.idText) with
        | Some ( APElemRef(_,vref,_) as apref) when IsValAccessible ad vref -> 
            success (resInfo,Item_apelem apref,rest)
        | _ -> 
        match mty.AllValuesAndMembers.TryFind(id.idText) with
        | Some vspec  when IsValAccessible ad (mk_vref_in_modref modref vspec) -> 
            success(resInfo,Item_val (mk_vref_in_modref modref vspec),rest)
        | _ ->
        // Something in a type? e.g. a literal field 
        let tcrefs = LookupTypeNameInEntityMaybeHaveArity ad id.idRange id.idText None modref
        let tcrefs = tcrefs |> List.map (fun tcref -> (resInfo,tcref))
        let tyconSearch = 
            match lid with 
            | tn:: rest when nonNil rest ->
                ResolveLongIdentInTyconRefs (ncenv:NameResolver) nenv LookupKind.Pattern (depth+1) m ad rest numTyArgsOpt id.idRange tcrefs
            | _ -> 
                NoResultsOrUsefulErrors
        // Constructor of a type? 
        let ctorSearch = 
            let typs = tcrefs |> List.map (fun (resInfo,tcref) -> (resInfo,FreshenTycon ncenv m tcref)) 
            typs |> CollectResults (fun (resInfo,typ) -> ResolveObjectConstructorPrim ncenv nenv.eDisplayEnv resInfo id.idRange ad typ) 
        // Something in a sub-namespace or sub-module or nested-type 
        let moduleSearch = 
            if (nonNil rest) then 
                match mty.ModulesAndNamespacesByDemangledName.TryFind(id.idText) with
                | Some(AccessibleEntityRef ad modref submodref) -> 
                    let resInfo = resInfo.AddEntity(id.idRange,submodref)
                    OneResult (ResolvePatternLongIdentInModuleOrNamespace ncenv nenv numTyArgsOpt ad resInfo (depth+1) m submodref submodref.ModuleOrNamespaceType rest)
                | _ -> 
                    NoResultsOrUsefulErrors
             else NoResultsOrUsefulErrors
        let res = AtMostOneResult id.idRange ( tyconSearch +++   ctorSearch +++ moduleSearch +++ raze (UndefinedName(depth,"constructor, module or namespace",id,[])))
        res
        
exception UpperCaseIdentifierInPattern of range
type WarnOnUpperFlag = WarnOnUpperCase | AllIdsOK


// Long ID in a pattern 
let ResolvePatternLongIdent (ncenv:NameResolver) warnOnUpper newDef m ad nenv numTyArgsOpt (lid:ident list) =
    let g = ncenv.g
    match lid with 
    // Single identifiers in patterns 
    | [id] ->
          // Single identifiers in patterns - bind to constructors and active patterns 
          // For the special case of 
          //   let C = x 
          match nenv.ePatItems.TryFind(id.idText) with
          | Some res when not newDef  -> FreshenUnqualifiedItem ncenv m res
          | _ -> 
          // Single identifiers in patterns - variable bindings 
          if not newDef &&
             (warnOnUpper = WarnOnUpperCase) && 
             id.idText.Length >= 3 && 
             System.Char.ToLowerInvariant id.idText.[0] <> id.idText.[0] then 
            warning(UpperCaseIdentifierInPattern(m));
          Item_newdef id
        
    // Long identifiers in patterns 
    | _ -> 
        if verbose then  dprintf "--> ResolvePatternLongIdent, lid = %s@%a\n" (text_of_lid lid) output_range m ;
        let moduleSearch ad = 
            ResolveLongIndentAsModuleOrNamespaceThen OpenQualified nenv ad lid 
                (ResolvePatternLongIdentInModuleOrNamespace ncenv nenv numTyArgsOpt ad)
        let tyconSearch ad = 
            match lid with 
            | tn:: rest when nonNil rest ->
                let tcrefs = LookupTypeNameInEnvNoArity tn.idText nenv
                let tcrefs = tcrefs |> List.map (fun tcref -> (ResolutionInfo.Empty,tcref))
                ResolveLongIdentInTyconRefs ncenv nenv LookupKind.Pattern 1 tn.idRange ad rest numTyArgsOpt tn.idRange tcrefs 
            | _ -> 
                NoResultsOrUsefulErrors
        let resInfo,res,rest = 
            match AtMostOneResult m (tyconSearch ad +++  moduleSearch ad) with 
            | Result _ as res -> ForceRaise res
            | _ ->  
                ForceRaise (AtMostOneResult m (tyconSearch AccessibleFromSomeFSharpCode +++ moduleSearch AccessibleFromSomeFSharpCode))
        ResolutionInfo.SendToSink(ncenv,nenv,ad,resInfo,(fun () -> true));
  
        if nonNil rest then error(Error("this is not a constructor or literal, or a constructor is being used incorrectly",(List.hd rest).idRange));
        res


//-------------------------------------------------------------------------
// Resolve F#/IL "." syntax in types
//------------------------------------------------------------------------- 

let rec ResolveTypeLongIdentInTypePrim (ncenv:NameResolver) typeNameResInfo ad resInfo depth m tcref (lid: ident list) =
    match lid with 
    | [] -> error(Error("Unexpected empty long identifier",m))
    | [id] -> 
        let tcrefs = LookupTypeNameInEntityMaybeHaveArity ad id.idRange id.idText (snd typeNameResInfo) tcref 
        let tcrefs = tcrefs |> List.map (fun tcref -> (resInfo,tcref))
        let tcrefs = CheckForMultipleGenericTypeAmbiguities tcrefs typeNameResInfo m 
        match tcrefs with 
        | tcref :: _ -> success tcref
        | [] -> raze (UndefinedName(depth,"type",id,[]))
    | id::rest ->
        // Search nested types
        let tyconSearch = 
            let tcrefs = LookupTypeNameInEntityMaybeHaveArity ad id.idRange id.idText None tcref
            let tcrefs = tcrefs |> List.map (fun tcref -> (resInfo,tcref))
            let tcrefs  = CheckForMultipleGenericTypeAmbiguities tcrefs (fst typeNameResInfo,None) m 
            tcrefs |> CollectResults (fun (resInfo,tcref) -> ResolveTypeLongIdentInTypePrim ncenv typeNameResInfo ad resInfo (depth+1) m tcref rest)
        AtMostOneResult m tyconSearch

let ResolveTypeLongIdentInType (ncenv:NameResolver) nenv typeNameResInfo ad m tcref (lid: ident list) =
    let resInfo,tcref = ForceRaise (ResolveTypeLongIdentInTypePrim ncenv typeNameResInfo ad ResolutionInfo.Empty 0 m tcref lid)
    ResolutionInfo.SendToSink(ncenv,nenv,ad,resInfo,(fun () -> true));
    tcref


let rec private ResolveTypeLongIdentInModuleOrNamespace (ncenv:NameResolver) typeNameResInfo ad (resInfo:ResolutionInfo) depth m modref mty (lid: ident list) =
    let g = ncenv.g
    match lid with 
    | [] -> error(Error("Unexpected empty long identifier",m))
    | [id] -> 
        // On all paths except error reporting we have isSome(numTyargsOpt), hence get at most one result back 
        let tcrefs = LookupTypeNameInEntityMaybeHaveArity ad id.idRange id.idText (snd typeNameResInfo) modref  
        if nonNil tcrefs then
            tcrefs |> CollectResults (fun tcref -> success(resInfo,tcref))
        else 
            raze (UndefinedName(depth,"type",id,[]))
    | id::rest ->
        let modulSearch = 
            match modref.ModuleOrNamespaceType.ModulesAndNamespacesByDemangledName.TryFind(id.idText) with
            | Some(AccessibleEntityRef ad modref submodref) -> 
                let resInfo = resInfo.AddEntity(id.idRange,submodref)
                ResolveTypeLongIdentInModuleOrNamespace ncenv typeNameResInfo ad resInfo (depth+1) m submodref submodref.ModuleOrNamespaceType rest
            | _ ->  
                raze (UndefinedName(depth,"module or namespace",id,[]))
        let tyconSearch = 
            let tcrefs = LookupTypeNameInEntityMaybeHaveArity ad id.idRange id.idText None modref
            tcrefs |> CollectResults (fun tcref -> ResolveTypeLongIdentInTypePrim ncenv typeNameResInfo ad resInfo (depth+1) m tcref rest)
        tyconSearch +++ modulSearch

let ResolveTypeLongIdentPrim (ncenv:NameResolver) fullyQualified m nenv ad (lid: ident list) ntyargs =
    let g = ncenv.g
    match lid with 
    | [] -> error(Error("Unexpected empty long identifier",m))
    | [id]  ->  
        match LookupTypeNameInEnvHaveArity id.idText ntyargs nenv with
        | Some res -> success(ResolutionInfo.Empty,res)
        | None -> 
            (* For Good Error Reporting! *)
            let tcrefs = LookupTypeNameInEnvNoArity id.idText nenv
            match tcrefs with
            | tcref :: tcrefs -> 
                // Note: This path is only for error reporting
                //CheckForMultipleGenericTypeAmbiguities tcref rest typeNameResInfo m;
                success(ResolutionInfo.Empty,tcref)
            | [] -> 
                raze (UndefinedName(0,"type",id,NameMap.domainL nenv.eTyconsByAccessNames))
    | id::rest ->
        let typeNameResInfo = (ResolveTypeNamesToTypeRefs,Some(ntyargs))
        let tyconSearch = 
            match LookupTypeNameInEnvHaveArity id.idText ntyargs nenv with
            | Some tcref when IsEntityAccessible ad tcref -> 
                OneResult (ResolveTypeLongIdentInTypePrim ncenv typeNameResInfo ad ResolutionInfo.Empty 1 m tcref rest)
            | _ -> 
                NoResultsOrUsefulErrors
        let modulSearch = 
            ResolveLongIndentAsModuleOrNamespaceThen fullyQualified nenv ad lid 
                (ResolveTypeLongIdentInModuleOrNamespace ncenv typeNameResInfo ad)
            |?> (fun res -> List.concat res)

        let modulSearchFailed = 
            ResolveLongIndentAsModuleOrNamespaceThen fullyQualified nenv AccessibleFromSomeFSharpCode lid 
                (ResolveTypeLongIdentInModuleOrNamespace ncenv (ResolveTypeNamesToTypeRefs,None) ad)
            |?> (fun res -> List.concat res)
(*
        let tyconSearchFailed = 
            (* For Good Error Reporting! *)
            match LookupTypeNameInEnvNoArity id.idText nenv with 
            | [] -> NoResultsOrUsefulErrors
            | tcrefs -> success(tcrefs)
*)
        match tyconSearch +++ modulSearch with 
        | Result results -> 
            // NOTE: we delay checking the CheckForMultipleGenericTypeAmbiguities condition until right at the end after we've
            // collected all possible resolutions of the type
            let tcrefs = CheckForMultipleGenericTypeAmbiguities results typeNameResInfo m 
            match tcrefs with 
            | (resInfo,tcref) :: _ -> 
                // We've already reported the ambiguity, possibly as an error. Now just take the first possible result.
                success(resInfo,tcref)
            | [] -> 
                // failing case - report nice ambiguity errors even in this case
                AtMostOneResult m ((tyconSearch +++ modulSearch +++ modulSearchFailed) |?> (fun tcrefs -> CheckForMultipleGenericTypeAmbiguities tcrefs typeNameResInfo m))
            
        | _ ->  
            // failing case - report nice ambiguity errors even in this case
            AtMostOneResult m ((tyconSearch +++ modulSearch +++ modulSearchFailed) |?> (fun tcrefs -> CheckForMultipleGenericTypeAmbiguities tcrefs typeNameResInfo m))


let ResolveTypeLongIdent (ncenv:NameResolver) occurence fullyQualified nenv ad (lid: ident list) ntyargs =
    let m = range_of_lid lid
    let res = ResolveTypeLongIdentPrim ncenv fullyQualified m nenv ad lid ntyargs 
    // Register the result as a name resolution
    match res with 
    | Result (resInfo,tcref) -> 
        ResolutionInfo.SendToSink(ncenv,nenv,ad,resInfo,(fun () -> true));
        CallNameResolutionSink(m,nenv,Item_typs(tcref.DisplayName,[FreshenTycon ncenv m tcref]),occurence,nenv.eDisplayEnv,ad)
    | _ -> ()
    res |?> snd

//-------------------------------------------------------------------------
// Resolve F#/IL "." syntax in records etc.
//------------------------------------------------------------------------- 

exception DeprecatedClassFieldInference of range

let rec ResolveFieldInModuleOrNamespace (ncenv:NameResolver) nenv ad (resInfo:ResolutionInfo) depth m modref mty lid = 
    let typeNameResInfo = DefaultTypeNameResInfo
    let g = ncenv.g
    if verbose then  dprintf "--> ResolveFieldInModuleOrNamespace edenv, lid = %s@%a\n" (text_of_lid lid) output_range m ;
    match lid with 
    | id::rest -> 
        let error = raze (UndefinedName(depth,"record label or namespace",id,[]))
        (* search for module-qualified names, e.g. { Microsoft.FSharp.Core.contents = 1 } *)
        let modulScopedFieldNames = 
            match TryFindTypeWithRecdField mty id  with
            | Some tycon when IsEntityAccessible ad (MakeNestedTcref modref tycon) -> 
                success(mk_rfref_in_tcref modref tycon id, rest)
            | _ -> error
        // search for type-qualified names, e.g. { Microsoft.FSharp.Core.Ref.contents = 1 } 
        let tyconSearch = 
            match lid with 
            | tn:: rest when nonNil rest ->
                let tcrefs = LookupTypeNameInEntityMaybeHaveArity ad id.idRange id.idText None modref
                let tcrefs = tcrefs |> List.map (fun tcref -> (ResolutionInfo.Empty,tcref))
                let tyconSearch = ResolveLongIdentInTyconRefs ncenv nenv LookupKind.RecdField  (depth+1) m ad rest typeNameResInfo id.idRange tcrefs
                // choose only fields 
                let tyconSearch = tyconSearch |?> List.choose (function (_,Item_recdfield(RecdFieldInfo(_,rfref)),rest) -> Some(rfref,rest) | _ -> None)
                tyconSearch
            | _ -> 
                NoResultsOrUsefulErrors
        (* search for names in nested modules, e.g. { Microsoft.FSharp.Core.contents = 1 } *)
        let modulSearch = 
            if nonNil rest then 
                match mty.ModulesAndNamespacesByDemangledName.TryFind(id.idText) with
                | Some(AccessibleEntityRef ad modref submodref) -> 
                    let resInfo = resInfo.AddEntity(id.idRange,submodref)
                    ResolveFieldInModuleOrNamespace ncenv nenv ad resInfo (depth+1) m submodref submodref.ModuleOrNamespaceType  rest 
                | _ -> 
                    error
            else error
        AtMostOneResult m (OneResult modulScopedFieldNames +++ tyconSearch +++ OneResult modulSearch)
    | [] -> failwith "ResolveFieldInModuleOrNamespace edenv"

let ResolveField (ncenv:NameResolver) nenv ad typ (mp,id:ident) =
    let typeNameResInfo = DefaultTypeNameResInfo
    let g = ncenv.g
    let m = id.idRange
    match mp with 
    | [] -> 
        if is_stripped_tyapp_typ g typ then 
            match ncenv.InfoReader.TryFindRecdFieldInfoOfType(id.idText,m,typ) with
            | Some (RecdFieldInfo(_,rfref)) -> [(rfref,true)]
            | None -> error(Error(sprintf "The type %s does not contain a field %s" (NicePrint.pretty_string_of_typ nenv.eDisplayEnv typ) id.idText,m))
        else 
            let frefs = tryname "record label" nenv.eFieldLabels id 
            (* Eliminate duplicates arising from multiple 'open' *)
            let frefs = frefs |> ListSet.setify (fun (fref1,_) (fref2,_) -> tcref_eq g fref1.TyconRef fref2.TyconRef)
            frefs
                        
    | _ -> 
        let lid = (mp@[id])
        let tyconSearch ad = 
            match lid with 
            | tn:: (_ :: _ as rest) -> 
                let m = tn.idRange
                let tcrefs = LookupTypeNameInEnvNoArity tn.idText nenv
                let tcrefs = tcrefs |> List.map (fun tcref -> (ResolutionInfo.Empty,tcref))
                let tyconSearch = ResolveLongIdentInTyconRefs ncenv nenv LookupKind.RecdField 1 m ad rest typeNameResInfo tn.idRange tcrefs
                // choose only fields 
                let tyconSearch = tyconSearch |?> List.choose (function (_,Item_recdfield(RecdFieldInfo(_,rfref)),rest) -> Some(rfref,rest) | _ -> None)
                tyconSearch
            | _ -> NoResultsOrUsefulErrors
        let modulSearch ad = 
            ResolveLongIndentAsModuleOrNamespaceThen OpenQualified nenv ad lid 
                (ResolveFieldInModuleOrNamespace ncenv nenv ad)
        let item,rest = ForceRaise (AtMostOneResult m (modulSearch ad +++ tyconSearch ad +++ modulSearch AccessibleFromSomeFSharpCode +++ tyconSearch AccessibleFromSomeFSharpCode))
        if nonNil rest then errorR(Error("invalid field label",(List.hd rest).idRange));
        [(item,true)]

/// Generate a new reference to a record field with a fresh type instantiation
let FreshenRecdFieldRef (ncenv:NameResolver) m (rfref:RecdFieldRef) =
    Item_recdfield(RecdFieldInfo(ncenv.instantiationGenerator m (rfref.Tycon.Typars(m)), rfref))



/// Resolve F#/IL "." syntax in expressions (2).
/// We have an expr. on the left, and we do an access, e.g. 
/// (f obj).field or (f obj).meth.  The basic rule is that if l-r type 
/// inference has determined the outer type then we can proceed in a simple fashion. The exception 
/// to the rule is for field types, which applies if l-r was insufficient to 
/// determine any valid members 
//
// QUERY (instantiationGenerator cleanup): it would be really nice not to flow instantiationGenerator to here. 
let private ResolveExprDotLongIdent (ncenv:NameResolver) m ad nenv typ lid findFlag =
    let g = ncenv.g
    let typeNameResInfo = DefaultTypeNameResInfo
    let adhoctDotSearchAccessible = AtMostOneResult m (ResolveLongIdentInTypePrim ncenv nenv LookupKind.Expr ResolutionInfo.Empty 1 m ad lid findFlag typeNameResInfo typ)
    match adhoctDotSearchAccessible with 
    | Exception _ ->
        // If the dot is not resolved by adhoc overloading then look for a record field 
        // that can resolve the name. 
        let dotFieldIdSearch = 
            match lid with 
            // A unique record label access, e.g  expr.field  
            | id::rest when nenv.eFieldLabels.ContainsKey(id.idText) -> 
                match nenv.eFieldLabels.[id.idText] with
                | [] -> NoResultsOrUsefulErrors
                | (rfref,isRecdField) :: _ ->
                    if not isRecdField then errorR(DeprecatedClassFieldInference(m));
                    // NOTE (instantiationGenerator cleanup): we need to freshen here because we don't know the type. 
                    // But perhaps the caller should freshen?? 
                    let item = FreshenRecdFieldRef ncenv m rfref
                    OneSuccess (ResolutionInfo.Empty,item,rest)
            | _ -> NoResultsOrUsefulErrors 
        
        // A unique record label access qualified by a module, e.g  expr.Module.field
        // Really for OCaml compat. only 
        let moduleFieldIdSearch =
            match lid with 
            | id::(_ :: _ as rest) when nenv.eModulesAndNamespaces.ContainsKey(id.idText) ->
                ResolveLongIndentAsModuleOrNamespaceThen OpenQualified nenv ad lid 
                    (ResolveFieldInModuleOrNamespace ncenv nenv ad)
                (* QUERY: should caller freshen? *)
                |?> List.map (fun (rfref,rest) -> 
                       warning(OCamlCompatibility("This lookup resolves to a record field by using the syntax 'expr.Module.field'. Although this is allowed for OCaml compatibility, the style is considered deprecated for F#. Consider using a simple field lookup 'expr.field', perhaps with an annotation to constrain the type of 'expr'",m));
                       (ResolutionInfo.Empty,FreshenRecdFieldRef ncenv m rfref,rest))
            | _  -> NoResultsOrUsefulErrors
        let search = dotFieldIdSearch +++ moduleFieldIdSearch
        match AtMostOneResult m search with 
        | Result _ as res -> ForceRaise res
        | _ -> 
            let adhoctDotSearchAll = ResolveLongIdentInTypePrim ncenv nenv LookupKind.Expr ResolutionInfo.Empty 1 m AccessibleFromSomeFSharpCode lid findFlag typeNameResInfo typ 
            ForceRaise (AtMostOneResult m (search +++ adhoctDotSearchAll))

    | Result _ -> 
        ForceRaise adhoctDotSearchAccessible

let ComputeItemRange wholem (lid: ident list) rest =
    match rest with
    | [] -> wholem
    | _ -> 
        let ids,_ = List.chop (max 0 (lid.Length - rest.Length)) lid
        match ids with 
        | [] -> wholem
        | _ -> range_of_lid ids

/// Filters method groups that will be sent to Visual Studio IntelliSense
/// to include only static/instance members
let filterMethodGroups (ncenv:NameResolver) itemRange item staticOnly =
    match item with
    | Item_meth_group(nm, minfos) -> 
        let minfos = minfos |> List.filter  (fun minfo -> 
           staticOnly = (ObjTypesOfMethInfo ncenv.amap itemRange minfo minfo.FormalMethodInst |> isNil))
        Item_meth_group(nm, minfos)
    | item -> item

/// Called for 'TypeName.Bar' - for VS IntelliSense, we can filter out instance members from method groups
let ResolveLongIdentAsExprAndComputeRange (ncenv:NameResolver) wholem ad nenv typeNameResInfo lid = 
    let item,rest = ResolveExprLongIdent ncenv wholem ad nenv typeNameResInfo lid
    let itemRange = ComputeItemRange wholem lid rest
    
    // Record the precise resolution of the field for intellisense
    CallNameResolutionSink(itemRange, nenv, filterMethodGroups ncenv itemRange item true, ItemOccurence.Use, nenv.DisplayEnv, ad);
    item, itemRange, rest

/// Called for 'expression.Bar' - for VS IntelliSense, we can filter out static members from method groups
let ResolveExprDotLongIdentAndComputeRange (ncenv:NameResolver) wholem ad nenv typ lid findFlag = 
    let resInfo,item,rest = ResolveExprDotLongIdent ncenv wholem ad nenv typ lid findFlag
    let itemRange = ComputeItemRange wholem lid rest
    ResolutionInfo.SendToSink(ncenv,nenv,ad,resInfo,(fun () -> CheckAllTyparsInferrable ncenv.g ncenv.amap itemRange item));
    
    // Record the precise resolution of the field for intellisense
    CallNameResolutionSink(itemRange, nenv, filterMethodGroups ncenv itemRange item false, ItemOccurence.Use, nenv.DisplayEnv, ad);
    item, itemRange, rest

//-------------------------------------------------------------------------
// Given an nenv resolve partial paths to sets of names, used by interactive
// environments (Visual Studio)
//
// ptc = partial type check
// ptci = partial type check item
//
// There are some inefficiencies in this code - e.g. we often 
// create potentially large lists of methods/fields/properties and then
// immiediately List.filter them.  We also use lots of "map/concats".  Dosen't
// seem to hit the interactive experience too badly though.
//------------------------------------------------------------------------- 

let FakeInstantiationGenerator (m:range) gps = List.map mk_typar_ty gps 

// note: making local refs is ok since it is only used by VS 
let ptci_of_vref v = Item_val(v)
let ptci_of_ucref v = Item_ucase v
let ptci_of_ecref v = Item_ecref v
let ptci_of_submodul v = Item_modrefs [v]
let ptci_of_recdfield v = Item_recdfield v
let ptci_of_il_finfo finfo = Item_il_field finfo
let ptci_of_einfo x = Item_event x
let ptci_of_pinfo (pinfo:PropInfo) = Item_property (pinfo.PropertyName,[pinfo])
let ptci_of_minfos (nm,minfos) = MakeMethGroup(nm,minfos)

let IsTyconUnseenObsoleteSpec ad g m (x:TyconRef) allowObsolete = 
    not (IsEntityAccessible ad x) ||
    ((not allowObsolete) &&
      (if x.IsILTycon then 
          CheckILAttribsForUnseen g x.ILTyconRawMetadata.tdCustomAttrs m
       else 
          CheckAttribsForUnseen g x.Attribs m))

let IsTyconUnseen ad g m (x:TyconRef) = IsTyconUnseenObsoleteSpec ad g m x false

let IsValUnseen ad g m (v:ValRef) = 
    not (IsValAccessible ad v) ||
    v.IsCompilerGenerated ||
    CheckAttribsForUnseen g v.Attribs m

let IsUnionCaseUnseen ad g m (ucref:UnionCaseRef) = 
    not (IsUnionCaseAccessible ad ucref) ||
    IsTyconUnseen ad g m ucref.TyconRef || 
    CheckAttribsForUnseen g ucref.Attribs m

let ItemIsUnseen ad g m item = 
    match item with 
    | Item_val x -> IsValUnseen ad  g m x
    | Item_ucase x -> IsUnionCaseUnseen ad g m x.UnionCaseRef
    | Item_ecref x -> IsTyconUnseen ad g m x
    | _ -> false

let ptci_of_tycon ncenv m (x:TyconRef) = 
    Item_typs (x.DisplayName,[FreshenTycon ncenv m x])

let ptci_of_typ g x = 
    let nm = if is_stripped_tyapp_typ g x then (tcref_of_stripped_typ g x).DisplayName else "?"
    Item_typs (nm,[x])

// Filter out 'PrivateImplementationDetail' classes 
let IsInterestingModuleName nm =
    String.length nm >= 1 &&
    String.sub nm 0 1 <> "<"

let rec PartialResolveLookupInModuleOrNamespaceAsModuleOrNamespaceThen f plid (modref:ModuleOrNamespaceRef) =
    if verbose then  dprintf "PartialResolveLookupInModuleOrNamespaceAsModuleOrNamespaceThen, plid = %s\n" (text_of_path plid); 
    let mty = modref.ModuleOrNamespaceType
    match plid with 
    | [] -> f modref
    | id:: rest -> 
        match mty.ModulesAndNamespacesByDemangledName.TryFind(id) with
        | Some mty -> PartialResolveLookupInModuleOrNamespaceAsModuleOrNamespaceThen f rest (MakeNestedTcref modref mty) 
        | None -> []

let PartialResolveLongIndentAsModuleOrNamespaceThen (nenv:NameResolutionEnv) plid f =
    if verbose then  dprintf "PartialResolveLongIndentAsModuleOrNamespaceThen, plid = %s\n" (text_of_path plid); 
    match plid with 
    | id:: rest -> 
        match Map.tryfind id nenv.eModulesAndNamespaces with
        | Some(modrefs) -> 
            List.collect (PartialResolveLookupInModuleOrNamespaceAsModuleOrNamespaceThen f rest) modrefs
        | None ->
            []
    | [] -> []

let ResolveCompletionsInType (ncenv: NameResolver) nenv m ad statics typ =
    let g = ncenv.g
    let amap = ncenv.amap
    let rfinfos = 
        if is_stripped_tyapp_typ g typ then 
            let tc,tinst = dest_stripped_tyapp_typ g typ
            (all_rfrefs_of_tcref tc)
            |> List.filter (IsRecdFieldAccessible ad)
            |> List.filter (fun fref -> fref.RecdField.IsStatic = statics)
            |> List.filter (fun fref -> not fref.RecdField.IsCompilerGenerated)
            |> List.map (fun fref -> RecdFieldInfo(tinst,fref)) 
        else []

    let ucinfos = 
        if statics  && is_stripped_tyapp_typ g typ then 
            let tc,tinst = dest_stripped_tyapp_typ g typ
            ucrefs_of_tcref tc 
            |> List.filter (IsUnionCaseUnseen ad g m >> not)
            |> List.map (fun ucref ->  Item_ucase(UnionCaseInfo(tinst,ucref)))
        else []

    let einfos = 
        ncenv.InfoReader.GetILEventInfosOfType(None,ad,m,typ)
        |> List.filter (IsILEventInfoAccessible g amap m ad)
        |> List.map (fun x -> ILEvent(g,x))
        |> List.filter (fun x -> 
            IsStandardEventInfo ncenv.InfoReader m ad x &&
            x.IsStatic = statics)

    let nestedTypes = 
        typ
        |> GetNestedTypesOfType ad ncenv (None,None) m 

    let finfos = 
        ncenv.InfoReader.GetILFieldInfosOfType(None,ad,m,typ)
        |> List.filter (fun (ILFieldInfo(ty,fld) as x) -> 
            fld.fdSpecialName = false &&
            x.IsStatic = statics && 
            IsILFieldInfoAccessible g amap m ad x)

    let pinfos = 
        AllPropInfosOfTypeInScope ncenv.InfoReader nenv.eExtensionMembers (None,ad) IgnoreOverrides m typ
        |> List.filter (fun x -> 
            x.IsStatic = statics && 
            not (PropInfoIsUnseen m x) &&
            IsPropInfoAccessible g amap m ad x)

    // Exclude get_ and set_ methods accessed by properties 
    let pinfoMethNames = 
      (pinfos 
       |> List.filter PropInfo.HasGetter
       |> List.map (fun pinfo -> pinfo.GetterMethod.LogicalName))
      @
      (pinfos 
       |> List.filter PropInfo.HasSetter
       |> List.map (fun pinfo -> pinfo.SetterMethod.LogicalName))
    
    let einfoMethNames = 
        [ for e in einfos do 
             match e with 
             | ILEvent(_, e) -> 
                 yield e.AddMethod.ILName
                 yield e.RemoveMethod.ILName
             | _ -> 
                 () ]

    let names = Zset.addList pinfoMethNames (Zset.addList einfoMethNames (Zset.empty String.order))

    let minfo_filter (minfo:MethInfo) = 
        // Only show the Finalize, MemberwiseClose etc. methods on System.Object for values whose static type really is 
        // System.Object. Few of these are typically used from F#.  
        (type_equiv g typ g.obj_ty 
         || minfo.LogicalName = "GetType" 
         || minfo.LogicalName = "GetHashCode" 
         || minfo.LogicalName = "ToString" 
         || (minfo.IsInstance && minfo.LogicalName = "Equals")
         || not (type_equiv g minfo.EnclosingType g.obj_ty)) &&
        not minfo.IsInstance = statics &&
        IsMethInfoAccessible amap m ad minfo &&
        not (MethInfoIsUnseen g m minfo) &&
        not minfo.IsConstructor &&
        not minfo.IsClassConstructor &&
        not (names.Contains minfo.LogicalName)
    
    // REVIEW: add a name List.filter here in the common cases?
    let minfos = 
        AllMethInfosOfTypeInScope ncenv.InfoReader nenv.eExtensionMembers (None,ad) IgnoreOverrides m typ 
        |> List.filter minfo_filter

    // Partition methods into overload sets
    let rec partitionl (l:MethInfo list) acc = 
        match l with
        | [] -> acc
        | h::t -> 
            let nm = h.LogicalName
            partitionl t (NameMultiMap.add nm h acc)

    // Build the results
    ucinfos @
    List.map ptci_of_recdfield rfinfos @
    List.map ptci_of_pinfo pinfos @
    List.map ptci_of_il_finfo finfos @
    List.map ptci_of_einfo einfos @
    List.map (ptci_of_typ g) nestedTypes @
    List.map ptci_of_minfos (NameMap.to_list (partitionl minfos Map.empty))
      

let rec ResolvePartialLongIdentInType (ncenv: NameResolver) nenv m ad statics plid typ =
    let g = ncenv.g
    let amap = ncenv.amap
    if verbose then   dprintf "ResolvePartialLongIdentInType , typ = '%s', plid = '%s'\n" (NicePrint.pretty_string_of_typ (empty_denv g) typ) (text_of_path plid);
    match plid with
    | [] -> ResolveCompletionsInType ncenv nenv m ad statics typ
    | id :: rest ->
  
      let rfinfos = 
        if is_stripped_tyapp_typ g typ then 
            let tc,tinst = dest_stripped_tyapp_typ g typ
            (all_rfrefs_of_tcref tc)
            |> List.filter (IsRecdFieldAccessible ad)
            |> List.filter (fun fref -> fref.RecdField.IsStatic = statics)
            |> List.filter (fun fref -> not fref.RecdField.IsCompilerGenerated)
            |> List.map (fun fref -> RecdFieldInfo(tinst,fref)) 
        else 
            []
  
      let nestedTypes = 
          typ 
          |> GetNestedTypesOfType ad ncenv (Some(id),None) m  

      let ucinfos = 
          if statics && is_stripped_tyapp_typ g typ then 
              let tc,tinst = dest_stripped_tyapp_typ g typ
              ucrefs_of_tcref tc
              |> List.filter (IsUnionCaseUnseen ad g m >> not)
              |> List.map (fun ucref ->  Item_ucase(UnionCaseInfo(tinst,ucref))) 
          else []
 
      // e.g. <val-id>.<recdfield-id>.<more> 
      (rfinfos |> List.filter (fun x -> x.Name = id)
               |> List.collect (fun x -> x.FieldType |> ResolvePartialLongIdentInType ncenv nenv m ad false rest)) @

      // e.g. <val-id>.<property-id>.<more> 
      (typ
         |> AllPropInfosOfTypeInScope ncenv.InfoReader nenv.eExtensionMembers (Some(id),ad) IgnoreOverrides m 
         |> List.filter (fun x -> x.IsStatic = statics)
         |> List.filter (IsPropInfoAccessible g amap m ad) 
         |> List.collect (PropertyTypeOfPropInfo amap m >> ResolvePartialLongIdentInType ncenv nenv m ad false rest)) @

      // e.g. <val-id>.<event-id>.<more> 
      (ncenv.InfoReader.GetILEventInfosOfType(Some(id),ad,m,typ)
         |> List.map (fun x -> ILEvent(g,x))
         |> List.collect (PropTypOfEventInfo ncenv.InfoReader m ad >> ResolvePartialLongIdentInType ncenv nenv m ad false rest)) @

      // nested types! 
      (nestedTypes 
         |> List.collect (ResolvePartialLongIdentInType ncenv nenv m ad statics rest)) @

      // e.g. <val-id>.<il-field-id>.<more> 
      (ncenv.InfoReader.GetILFieldInfosOfType(Some(id),ad,m,typ)
         |> List.filter (fun x -> 
             not x.RawMetadata.fdSpecialName &&
             x.IsStatic = statics && 
             IsILFieldInfoAccessible g amap m ad x)
         |> List.collect (FieldTypeOfILFieldInfo amap m >> ResolvePartialLongIdentInType ncenv nenv m ad false rest))
     
let ptcis_of_tycon_ctors (ncenv:NameResolver) m ad (tcref:TyconRef) = 
    let g = ncenv.g
    let amap = ncenv.amap
    // Don't show constructors for type abbreviations. See FSharp 1.0 bug 2881
    if tcref.IsTypeAbbrev then 
        []
    else 
        let typ = FreshenTycon ncenv m tcref
        match ResolveObjectConstructor ncenv (empty_denv g) m ad typ with 
        | Result (item,_) -> 
            begin match item with 
            | Item_ctor_group(nm,cinfos) -> 
                cinfos 
                |> List.filter (IsMethInfoAccessible amap m ad)
                |> List.filter (MethInfoIsUnseen g m >> not)
                |> List.map (fun minfo -> MakeCtorGroup(nm,[minfo])) 
            | item -> 
                [item]
            end
        | Exception _ -> []

(* import.ml creates somewhat fake modules for nested members of types (so that *)
(* types never contain other types) *)
let not_fake_container_modul tyconNames nm = 
  not (Set.mem nm tyconNames)

/// Check is a namesapce or module contains something accessible 
let rec private EntityRefContainsSomethingAccessible (ncenv: NameResolver) m ad (modref:ModuleOrNamespaceRef) = 
    let g = ncenv.g
    let mty = modref.ModuleOrNamespaceType

    // Search the values in the module for an accessible value 
    // BUG: we're not applying accessibility checks here, just looking for any value 
    (mty.AllValuesAndMembers
     |> NameMap.exists (fun _ v -> 
         let vref = mk_vref_in_modref modref v
         not vref.IsCompilerGenerated && 
         not (IsValUnseen ad g m vref) &&
         isNone(vref.MemberInfo))) ||

    // Search the types in the namespace/module for an accessible tycon 
    (mty.AllEntities 
     |> NameMap.exists (fun _ tc ->  
          not tc.IsModuleOrNamespace && 
          not (IsTyconUnseen ad g m (MakeNestedTcref modref tc)))) ||

    // Search the sub-modules of the namespace/modulefor something accessible 
    (mty.ModulesAndNamespacesByDemangledName 
     |> NameMap.exists (fun _ submod -> 
        let submodref = MakeNestedTcref modref submod
        EntityRefContainsSomethingAccessible ncenv m ad submodref)) 

let rec ResolvePartialLongIdentInModuleOrNamespace (ncenv: NameResolver) nenv m ad (modref:ModuleOrNamespaceRef) plid allowObsolete =
    let g = ncenv.g
    let amap = ncenv.amap
    if verbose then   dprintf "ResolvePartialLongIdentInModuleOrNamespace, plid = %s\n" (text_of_path plid); 
    let mty = modref.ModuleOrNamespaceType
    
    let tycons = 
        mty.TypeDefinitions
        |> List.filter (fun tycon -> not (IsTyconUnseen ad g m (MakeNestedTcref modref tycon)))

    let iltyconNames = 
        mty.TypesByAccessNames
        |> NameMultiMap.range
        |> List.choose (fun (tycon:Tycon) -> if tycon.IsILTycon then Some(tycon.DisplayName) else None)
        |> Set.of_seq      
    
    match plid with 
    | [] -> 

         // Collect up the accessible values in the module, excluding the members
         (mty.AllValuesAndMembers
          |> NameMap.range
          |> List.map (mk_vref_in_modref modref)
          |> List.filter (fun v -> v.MemberInfo.IsNone)
          |> List.filter (IsValUnseen ad g m >> not) 
          |> List.map ptci_of_vref)

         // Collect up the accessible discriminated union cases in the module 
       @ (UnionCaseRefsInModuleOrNamespace modref 
          |> List.filter (IsUnionCaseUnseen ad g m >> not)
          |> List.map GeneralizeUnionCaseRef 
          |> List.map ptci_of_ucref)

         // Collect up the accessible active patterns in the module 
       @ (ActivePatternElemsOfModuleOrNamespace modref 
          |> NameMap.range
          |> List.filter (fun apref -> apref.ActivePatternVal |> IsValUnseen ad g m |> not) 
          |> List.map Item_apelem)


         // Collect up the accessible F# exception declarations in the module 
       @ (mty.ExceptionDefinitionsByDemangledName 
          |> NameMap.range 
          |> List.map (MakeNestedTcref modref)
          |> List.filter (IsTyconUnseen ad g m >> not)
          |> List.map ptci_of_ecref)

         // Collect up the accessible sub-modules 
       @ (mty.ModulesAndNamespacesByDemangledName 
          |> NameMap.range 
          |> List.filter (demangled_name_of_modul >> not_fake_container_modul iltyconNames)
          |> List.filter (demangled_name_of_modul >> IsInterestingModuleName)
          |> List.map (MakeNestedTcref modref)
          |> List.filter (IsTyconUnseen ad g m >> not)
          |> List.filter (EntityRefContainsSomethingAccessible ncenv m ad)
          |> List.map ptci_of_submodul)

    (* Get all the types and .NET constructor groups accessible from here *)
       @ (tycons 
          |> List.map (MakeNestedTcref modref >> ptci_of_tycon ncenv m) )

       @ (tycons 
          |> List.map (MakeNestedTcref modref >> ptcis_of_tycon_ctors ncenv m ad) |> List.concat)

    | id :: rest  -> 
        (match mty.ModulesAndNamespacesByDemangledName.TryFind(id) with
         | Some mspec 
             when not (IsTyconUnseenObsoleteSpec ad g m (MakeNestedTcref modref mspec) allowObsolete) -> 
             let allowObsolete = rest <> [] && allowObsolete
             ResolvePartialLongIdentInModuleOrNamespace ncenv nenv m ad (MakeNestedTcref modref mspec) rest allowObsolete
         | _ -> [])

      @ (LookupTypeNameInEntityNoArity m id mty 
         |> List.collect (fun tycon ->
             let tcref = MakeNestedTcref modref tycon 
             if not (IsTyconUnseenObsoleteSpec ad g m tcref allowObsolete) then 
                 tcref |> generalize_tcref |> snd |> ResolvePartialLongIdentInType ncenv nenv m ad true rest
             else 
                 []))

/// allowObsolete - specifies whether we should return obsolete types & modules 
///   as (no other obsolete items are returned)
let ResolvePartialLongIdent (ncenv: NameResolver) nenv m ad plid allowObsolete = 
    let g = ncenv.g
    match  plid with
    |  [] -> 
       let iltyconNames =
          nenv.eTyconsByAccessNames
          |> NameMultiMap.range
          |> List.choose (fun (tyconRef) -> if tyconRef.IsILTycon then Some(tyconRef.DisplayName) else None)
          |> Set.of_seq      
       
       let items = 
           nenv.eUnqualifiedItems
           |> NameMap.range
           |> List.filter (ItemIsUnseen ad g m >> not)

       let apats = 
           nenv.ePatItems
           |> NameMap.range
           |> List.filter (function Item_apelem v -> true | _ -> false)

       let mods = 
           nenv.eModulesAndNamespaces 
           |> NameMultiMap.range 
           |> List.filter (demangled_name_of_modref >> IsInterestingModuleName  )
           |> List.filter (demangled_name_of_modref >> not_fake_container_modul iltyconNames)
           |> List.filter (EntityRefContainsSomethingAccessible ncenv m ad)
           |> List.filter (IsTyconUnseen ad g m >> not)
           |> List.map ptci_of_submodul

       let tycons = 
           nenv.eTyconsByDemangledNameAndArity
           |> NameMap.range
           |> List.filter (fun tcref -> not tcref.IsExceptionDecl) 
           |> List.filter (IsTyconUnseen ad g m >> not)
           |> List.map (ptci_of_tycon ncenv m)

       // Get all the constructors accessible from here
       let constructors =  
           nenv.eTyconsByDemangledNameAndArity
           |> NameMap.range
           |> List.filter (IsTyconUnseen ad g m >> not)
           |> List.collect (ptcis_of_tycon_ctors ncenv m ad)

       items @ apats @ mods @ tycons @ constructors 

    | id :: rest -> 
    
        (* Look in the namespaces 'id' *)
        PartialResolveLongIndentAsModuleOrNamespaceThen nenv [id] (fun modref -> 
          let allowObsolete = rest <> [] && allowObsolete
          if EntityRefContainsSomethingAccessible ncenv m ad modref then 
            ResolvePartialLongIdentInModuleOrNamespace ncenv nenv m ad modref rest allowObsolete
          else 
            [])

        (* Look for values called 'id' that accept the dot-notation *)
      @ (if nenv.eUnqualifiedItems.ContainsKey(id) then 
                 (* v.lookup : member of a value *)
          let v = Map.find id nenv.eUnqualifiedItems
          match v with 
          | Item_val x -> 
              if verbose then   dprintf "ResolvePartialLongIdent (through val), plid = %s\n" (text_of_path plid); 
              let typ = x.Type
              let typ = if x.BaseOrThisInfo = CtorThisVal then dest_refcell_ty g typ else typ
              ResolvePartialLongIdentInType ncenv nenv m ad false rest  typ
          | _ -> []
         else [])
      @ 
    (* type.lookup : lookup a static something in a type *)
        (LookupTypeNameInEnvNoArity id nenv |> List.collect (FreshenTycon ncenv m >> ResolvePartialLongIdentInType ncenv nenv m ad true rest))


