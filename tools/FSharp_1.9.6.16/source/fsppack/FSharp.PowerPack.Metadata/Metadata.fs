// Copyright (c) Microsoft Corporation 2005-2008.
// This sample code is provided "as is" without warranty of any kind. 
// We disclaim all warranties, either express or implied, including the 
// warranties of merchantability and fitness for a particular purpose. 
//


namespace FSharp.PowerPack.Metadata

open System.IO
open System.Collections.Generic
open System.Reflection
open FSharp.PowerPack.Metadata.Reader.Internal
open FSharp.PowerPack.Metadata.Reader.Internal.AbstractIL.IL
open FSharp.PowerPack.Metadata.Reader.Internal.Tast
open FSharp.PowerPack.Metadata.Reader.Internal.Prelude
open FSharp.PowerPack.Metadata.Reader.Internal.Pickle

module Impl = 

    let readToEnd (s : Stream) = 
        let n = int s.Length 
        let res = Array.zeroCreate n 
        let mutable i = 0 
        while (i < n) do 
            i <- i + s.Read(res,i,n - i) 
        res 
        
    let makeReadOnlyCollection (arr:seq<'a>) = System.Collections.ObjectModel.ReadOnlyCollection<_>(Seq.to_array arr)
    
    let makeXmlDoc (XmlDoc x) = makeReadOnlyCollection(x)
    
    let isPublic a = (taccessPublic = a)

open Impl

type [<Sealed>] Env(typars:Typar list) = 
   let typars = Array.of_list typars
   member x.Typars = typars

type [<Sealed>] Range(range:range) = 
   member x.Document    = range.rangeFile
   member x.StartLine   = range.rangeBegin.posLine
   member x.StartColumn = range.rangeBegin.posCol
   member x.EndLine     = range.rangeEnd.posLine
   member x.EndColumn   = range.rangeEnd.posCol


type [<Sealed>] AssemblyLoader() = 
    static let table = Dictionary<string,CcuThunk>(100)
     
    static let fslib = AssemblyLoader.Add("FSharp.Core", typedefof<list<_>>.Assembly)
     
    static let globals = 
        let p = [| "Microsoft"; "FSharp"; "Core" |]
        { nativeptr_tcr = mk_nonlocal_tcref (NLPath(fslib.RawCcuThunk,p)) "nativeptr`1" 
          nativeint_tcr= mk_nonlocal_tcref (NLPath(fslib.RawCcuThunk,p)) "nativeint" 
          byref_tcr= mk_nonlocal_tcref (NLPath(fslib.RawCcuThunk,p)) "byref`1" 
          il_arr1_tcr= mk_nonlocal_tcref (NLPath(fslib.RawCcuThunk,p)) "[]`1" 
          il_arr2_tcr= mk_nonlocal_tcref (NLPath(fslib.RawCcuThunk,p)) "[,]`1" 
          il_arr3_tcr= mk_nonlocal_tcref (NLPath(fslib.RawCcuThunk,p)) "[,,]`1" 
          il_arr4_tcr= mk_nonlocal_tcref (NLPath(fslib.RawCcuThunk,p)) "[,,,]`1" 
          unit_tcr= mk_nonlocal_tcref (NLPath(fslib.RawCcuThunk,p)) "unit"  }


    static member FSharpLibrary with get()  = fslib
    static member TcGlobals with get() = globals

    
    static member TryLoad(name:string) : FSharpAssembly option = 
        if table.ContainsKey(name) then 
            Some(FSharpAssembly(table.[name]))
        else
            let assembly = Assembly.Load(name)
            AssemblyLoader.TryAdd(name,assembly)

    static member Add(name,assembly:Assembly) : FSharpAssembly = 
        match AssemblyLoader.TryAdd(name,assembly) with 
        | None -> invalidArg "name" (sprintf "could not produce an FSharpAssembly object for the assembly '%s' because this is not an F# assembly" name)
        | Some res -> res

    static member TryAdd(name,assembly:Assembly) : FSharpAssembly option = 
        let sref : ILScopeRef = ILScopeRef.Assembly(ILAssemblyRef.FromAssembly(assembly)) 
        match assembly.GetManifestResourceStream(FSharpSignatureDataResourceName ^ "." ^ name) with 
        | null -> None

        | resourceStream ->
            let bytes = resourceStream |> readToEnd
            let data = unpickle_obj_with_dangling_ccus assembly.FullName sref UnpickleModuleInfo bytes
            let info = data.RawData
            let ccuData = 
                { ccu_scoref=sref;
                  ccu_stamp = new_stamp();
                  ccu_filename = None; 
                  ccu_qname= Some(sref.QualifiedName);
                  ccu_code_dir = info.compile_time_working_dir; 
                  ccu_fsharp=true;
                  ccu_contents = info.mspec; 
                  ccu_usesQuotations = info.usesQuotations;
                  ccu_forwarders = lazy Map.empty }
                    
            let ccu = CcuThunk.Create(name, ccuData)
            table.[name] <- ccu
            let info = data.OptionalFixup(fun nm -> match AssemblyLoader.TryLoad(nm) with Some x -> Some(x.RawCcuThunk) | _ -> None )
            Some(FSharpAssembly(ccu))
        

and [<Sealed>] FSharpAssembly(ccu: CcuThunk) = 

    member x.RawCcuThunk = ccu

    static member FSharpLibrary with get() = AssemblyLoader.FSharpLibrary
    
    static member FromAssembly(assembly:Assembly) : FSharpAssembly = 
        AssemblyLoader.Add(assembly.GetName().Name , assembly)
    
    static member FromFile(fileName) =
        let assembly = Assembly.LoadFrom(fileName)
        FSharpAssembly.FromAssembly(assembly)
    
    member x.QualifiedName = ccu.QualifiedName.Value
      
    member x.CodeLocation = ccu.SourceCodeDirectory
      
    member x.ReflectionAssembly = Assembly.Load(x.QualifiedName)
    member x.GetEntity(name:string) = 
        let path = name.Split [| '.' |]
        if path.Length = 0 then invalidArg "name" "bad entity name"
        let path1 = path.[0..path.Length-2] 
        let nm = path.[path.Length-1]
        let tcref  = mk_nonlocal_tcref (NLPath(ccu,path1)) nm
        FSharpEntity(tcref)
      
    member x.Entities = 
        let rec loop(entity:Entity) = 
            [| if entity.IsNamespace then 
                  for KeyValue(_,entity) in entity.ModuleOrNamespaceType.AllEntities do 
                      yield! loop entity
               elif isPublic entity.Accessibility  then
                   yield FSharpEntity(mk_local_tcref entity) 
               else
                   () |]
        [| for KeyValue(_,entity) in ccu.TopModulesAndNamespaces do 
              yield! loop entity |] |> makeReadOnlyCollection
                 

and [<Sealed>] FSharpEntity(v:EntityRef) = 
    let isExternal() = 
        match v with 
        | ERef_nonlocal nlr -> 
            match nlr.nlr_nlpath with 
            | (NLPath(ccu,_)) -> ccu.IsUnresolvedReference
        | _ -> 
           false

    let isUnresolved() = 
        not (isExternal()) && v.TryDeref.IsNone 

    let poorQualifiedName() = v.nlr.nlr_nlpath.DisplayName + "." + v.nlr.nlr_item + ", "  + v.nlr.nlr_nlpath.AssemblyName

    let checkIsFSharp() = 
        if isExternal() then invalidOp (sprintf "The entity '%s' is external to F#. This operation may only be performed on an entity from an F# assembly" (poorQualifiedName()))
        if isUnresolved() then invalidOp (sprintf "The entity '%s' does not exist or is in an unresolved assembly." (poorQualifiedName()))

    static member FromType (ty: System.Type) = 
        let assembly = ty.Assembly
        let fassembly = FSharpAssembly.FromAssembly assembly
        let gty = (if ty.IsGenericType then ty.GetGenericTypeDefinition() else ty)
        let path = (if ty.IsGenericType then ty.GetGenericTypeDefinition() else ty).FullName.Split [| '.' |]
        let path1 = if path.Length = 0 then [| |] else path.[0..path.Length-2] 
        let nm = if path.Length = 0 then gty.Name else path.[path.Length-1]
        let tcref  = mk_nonlocal_tcref (NLPath(fassembly.RawCcuThunk,path1)) nm
        FSharpEntity(tcref)

    member x.Name = checkIsFSharp(); v.MangledName

    member x.QualifiedName = 
        if isExternal() || isUnresolved() then 
            poorQualifiedName()     
        else            
            if v.IsTypeAbbrev then invalidOp (sprintf "the type abbreviation '%s' does not have a qualified name" x.Name)
            match v.CompiledRepresentation with 
            | TyrepNamed(tref,_) -> tref.QualifiedName
            | TyrepOpen _ -> invalidOp (sprintf "the type %s does not have a qualified name" x.Name)
        

    member x.Range = checkIsFSharp(); Range(v.Range)

    member x.GenericParameters= 
        checkIsFSharp(); 
        let env = Env(v.TyparsNoRange)
        v.TyparsNoRange |> List.map (fun tp -> FSharpGenericParameter(env,tp)) |> List.to_array |> makeReadOnlyCollection

    member x.IsMeasure = checkIsFSharp(); (v.TypeOrMeasureKind = KindMeasure)
    member x.IsModule = checkIsFSharp(); v.IsModule
    member x.HasFSharpModuleSuffix = checkIsFSharp(); v.IsModule && (v.ModuleOrNamespaceType.ModuleOrNamespaceKind = ModuleOrNamespaceKind.FSharpModuleWithSuffix)
    member x.IsStruct  = checkIsFSharp(); v.IsStructTycon

#if TODO
    member x.IsClass = (not v.IsNamespace && not v.IsModule && v.TypeOrMeasureKind = TyparKind.KindType)
    member x.IsInterface = (not v.IsNamespace && not v.IsModule && v.TypeOrMeasureKind = TyparKind.KindType)
    member x.IsDelegate : bool
    member x.IsAbstract : bool;                       
    member x.IsEnum : bool
#endif
    
    member x.IsExceptionDeclaration = checkIsFSharp(); v.IsExceptionDecl

    member x.IsExternal = 
        // Don't call this: checkIsFSharp()   -- this one is valid on non-F# types
        isExternal()

    member x.IsAbbreviation = checkIsFSharp(); v.IsTypeAbbrev 

    member x.GetReflectionType() = 
        // Don't call this: checkIsFSharp()   -- this one is valid on non-F# types
        System.Type.GetType(x.QualifiedName) 

    member x.HasAssemblyCodeRepresentation = 
        checkIsFSharp(); 
        v.IsAsmReprTycon || v.IsMeasureableReprTycon


#if TODO
    member x.GetAssemblyCodeRepresentation : unit -> string 
    // member TyconDelegateSlotSig : SlotSig option
    member x.Accessibility: FSharpAccessibility; 
    member x.RepresentationAccessibility: FSharpAccessibility;
      

#endif

      /// Interface implementations - boolean indicates compiler-generated 
    member x.Implements = 
        checkIsFSharp(); 
        let env = Env(v.TyparsNoRange)
        v.TypeContents.tcaug_implements |> List.map (fun (ty,_,_) -> FSharpType(env,ty)) |> makeReadOnlyCollection

      /// Super type, if any 
    member x.BaseType = 
        checkIsFSharp(); 
        let env = Env(v.TyparsNoRange)
        match v.TypeContents.tcaug_super with 
        | None -> failwith "has no base type"
        | Some ty -> FSharpType(env,ty)
        
      /// Indicates the type prefers the "tycon&lt;a,b&gt;" syntax for display etc. 
    member x.UsesPrefixDisplay = 
        checkIsFSharp(); 
        v.Deref.Data.entity_uses_prefix_display


      /// Properties, methods etc. with implementations
    member x.MembersOrValues = 
        checkIsFSharp(); 
        ((v.TypeContents.tcaug_adhoc 
          |> NameMultiMap.range 
          |> List.filter (fun v -> not v.MemberInfo.Value.MemberFlags.MemberIsOverrideOrExplicitImpl) 
          |> List.map (fun x -> FSharpMemberOrVal(x.Deref)))
        @
         (v.ModuleOrNamespaceType.AllValuesAndMembers 
          |> NameMap.range 
          |> List.filter (fun v -> not v.IsMember) 
          |> List.filter (fun x -> isPublic x.Accessibility) 
          |> List.map (fun x -> FSharpMemberOrVal(x))))
           
          |> makeReadOnlyCollection

    member x.XmlDoc = 
        checkIsFSharp(); 
        v.XmlDoc |> makeXmlDoc

    member x.NestedEntities = 
        checkIsFSharp(); 
        v.ModuleOrNamespaceType.AllEntities 
        |> NameMap.range 
        |> List.filter (fun x -> isPublic x.Accessibility) 
        |> List.map (fun x -> FSharpEntity(mk_local_tcref x)) 
        |> makeReadOnlyCollection

    member x.UnionCases = 
        checkIsFSharp(); 
        let env = Env(v.TyparsNoRange)
        v.UnionCasesAsList 
        |> List.filter (fun x -> isPublic x.Accessibility) 
        |> List.map (fun x -> FSharpUnionCase(env,x)) 
        |> makeReadOnlyCollection

    member x.RecordFields =
        checkIsFSharp(); 
        let env = Env(v.TyparsNoRange)
        v.AllFieldsAsList 
        |> List.filter (fun x -> isPublic x.Accessibility) 
        |> List.map (fun x -> FSharpRecordField(env,x)) 
        |> makeReadOnlyCollection

    member x.AbbreviatedType   = 
        checkIsFSharp(); 
        let env = Env(v.TyparsNoRange)
        match v.TypeAbbrev with 
        | None -> failwith "not a type abbreviation"
        | Some ty -> FSharpType(env,ty)

    member x.Attributes = 
        checkIsFSharp(); 
        v.Attribs |> List.map (fun a -> FSharpAttribute(a)) |> makeReadOnlyCollection

and [<Sealed>] FSharpUnionCase(env:Env,v: UnionCase) =
    member x.Name = v.DisplayName
    member x.Range = Range(v.Range)
    member x.Fields = v.RecdFields |> List.map (fun r -> FSharpRecordField(env,r)) |> List.to_array |> makeReadOnlyCollection
    member x.ReturnType = FSharpType(env,v.ucase_rty)
    member x.CompiledName = v.ucase_il_name
    member x.XmlDoc = v.ucase_xmldoc |> makeXmlDoc
    member x.Attributes = v.Attribs |> List.map (fun a -> FSharpAttribute(a)) |> makeReadOnlyCollection
    //member x.Accessibility: FSharpAccessibility; 

and [<Sealed>] FSharpRecordField(env:Env,v: RecdField) =
    member x.IsMutable = v.IsMutable
    member x.XmlDoc = v.rfield_xmldoc |> makeXmlDoc
    member x.Type = FSharpType(env,v.FormalType)
    member x.IsStatic = v.IsStatic
    member x.Name = v.Name
    member x.IsCompilerGenerated = v.IsCompilerGenerated
    member x.Range = Range(v.Range)
    member x.FieldAttributes = v.FieldAttribs |> List.map (fun a -> FSharpAttribute(a)) |> makeReadOnlyCollection
    member x.PropertyAttributes = v.PropertyAttribs |> List.map (fun a -> FSharpAttribute(a)) |> makeReadOnlyCollection
    //member x.LiteralValue = v.Is
    //member x.Accessibility: FSharpAccessibility; 

and [<Sealed>] FSharpAccessibility() = 
    //member x.IsPublic : bool
    //member x.IsPrivate : bool
    //member x.IsInternal : bool
    
and [<Sealed>] FSharpGenericParameter(env:Env,v:Typar) = 

    member x.Name = v.DisplayName
    member x.Range = Range(v.Range)
       
    member x.IsMeasure = (v.Kind = TyparKind.KindMeasure)

    member x.XmlDoc = v.Data.typar_xmldoc |> makeXmlDoc

    member x.IsSolveAtCompileTime = (v.StaticReq = TyparStaticReq.HeadTypeStaticReq)
       
    member x.Attributes = v.Attribs |> List.map (fun a -> FSharpAttribute(a)) |> makeReadOnlyCollection

    //member x.Constraints: FSharpGenericParameterConstraint[]; 

and [<Sealed>] FSharpGenericParameterConstraint() = 
(*
    /// Indicates a constraint that a type is a subtype of the given type 
    member x.IsCoercesToType : bool
    member x.GetCoercesToTypeTarget : unit -> FSharpType 

    /// Indicates a default value for an inference type variable should it be netiher generalized nor solved 
    member x.IsDefaultsToType : bool
    member x.GetDefaultsToTypePriority : unit -> int
    member x.GetDefaultsToTypeTarget : unit -> FSharpType

    /// Indicates a constraint that a type has a 'null' value 
    member x.IsSupportsNull  : bool

    /// Indicates a constraint that a type has a member with the given signature 
    member x.IsMayResolveMemberConstraint : bool
    member x.GetMayResolveMemberConstraintSources : unit -> FSharpType[] 
    member x.GetMayResolveMemberConstraintMemberName : string 
    member x.GetMayResolveMemberConstraintIsStatc : bool
    member x.GetMayResolveMemberConstraintArgumentType : unit -> FSharpType[] 
    member x.GetMayResolveMemberConstraintReturnType : unit -> FSharpType 

    /// Indicates a constraint that a type is a non-Nullable value type 
    member x.IsIsNotNullableValueType : bool
    
    /// Indicates a constraint that a type is a reference type 
    member x.IsIsReferenceType  : bool

    /// Indicates a constraint that a type is a simple choice between one of the given ground types. See format.ml 
    member x.IsSimpleChoice : bool
    member x.GetSimpleChoiceChoices : unit -> FSharpType[]

    /// Indicates a constraint that a type has a parameterless constructor 
    member x.IsRequiresDefaultConstructor  : bool

    /// Indicates a constraint that a type is an enum with the given underlying 
    member x.IsIsEnum : bool
    member x.GetIsEnumUnderlying : unit -> FSharpType 
    
    /// Indicates a constraint that a type is a delegate from the given tuple of args to the given return type 
    member x.IsIsDelegate : bool
    member x.GetIsDelegateArgumentType : unit -> FSharpType 
    member x.GetIsDelegateReturnType : unit -> FSharpType 
*)
    
and FSharpInlineAnnotation = 
   | PsuedoValue = 3
   | AlwaysInline = 2
   | OptionalInline = 1
   | NeverInline = 0

and [<Sealed>] FSharpMemberOrVal(v:Val) = 

    let g = AssemblyLoader.TcGlobals
    let is_unit_typ ty = 
        match ty with 
        | TType_app (tcr,_) -> prim_tcref_eq false AssemblyLoader.FSharpLibrary.RawCcuThunk g.unit_tcr tcr 
        | _ -> false

    let is_fun_typ ty = match ty with TType_fun _ -> true | _ -> false
    let dest_fun_typ ty = match ty with TType_fun (d,r) -> (d,r) | _ -> failwith "dest_fun_typ"
    let dest_tuple_typ ty = if is_unit_typ ty then [] else match ty with TType_tuple tys -> tys | _ -> [ty]

    let rec strip_fun_typ_upto n ty = 
        assert (n >= 0);
        if n > 0 && is_fun_typ ty then 
            let (d,r) = dest_fun_typ ty
            let more,rty = strip_fun_typ_upto (n-1) r in d::more, rty
        else [],ty

    (* A 'tau' type is one with its type paramaeters stripped off *)
    let GetTopTauTypeInFSharpForm (curriedArgInfos: TopArgInfo list list) tau m =

        let argtys,rty = strip_fun_typ_upto curriedArgInfos.Length tau

        if curriedArgInfos.Length <> argtys.Length then 
            error(Error("Invalid member signature encountered because of an earlier error",m))

        let argtysl = 
            (curriedArgInfos,argtys) ||> List.map2 (fun argInfos argty -> 
                match argInfos with 
                | [] -> [] //else [ (mk_unit_typ g, TopValInfo.unnamedTopArg1) ]
                | [argInfo] -> [ (argty, argInfo) ]
                | _ -> List.zip (dest_tuple_typ argty) argInfos) 
        argtysl,rty

    member x.MangledName = v.MangledName
    member x.Range = Range(v.Range)

    member x.GenericParameters = 
        let env = Env(v.Typars)
        v.Typars |> List.map (fun tp -> FSharpGenericParameter(env,tp)) |> List.to_array |> makeReadOnlyCollection

    member x.Type = 
        FSharpType(Env(v.Typars),v.TauType)

    member x.IsCompilerGenerated = v.IsCompilerGenerated

    member x.InlineAnnotation = 
        match v.InlineInfo with 
        | ValInlineInfo.PseudoValue -> FSharpInlineAnnotation.PsuedoValue
        | ValInlineInfo.AlwaysInline -> FSharpInlineAnnotation.AlwaysInline
        | ValInlineInfo.OptionalInline -> FSharpInlineAnnotation.OptionalInline
        | ValInlineInfo.NeverInline -> FSharpInlineAnnotation.NeverInline

    member x.IsMutable = v.IsMutable

    member x.IsModuleValueOrMember = v.IsMember || v.IsModuleBinding

    member x.IsExtensionMember = v.IsExtensionMember

    member x.IsImplicitConstructor = v.IsIncrClassConstructor
    
    member x.IsTypeFunction = v.IsTypeFunction

    member x.CompiledName = v.CompiledName

    member x.XmlDoc = makeXmlDoc(v.XmlDoc)


    member x.CurriedParameterGroups = 
        let env = Env(v.Typars)
        match v.TopValInfo with 
        | None -> failwith "not a module let binding or member"
        | Some (TopValInfo(typars,curriedArgInfos,retInfo)) -> 
            let tau = v.TauType
            let argtysl,_ = GetTopTauTypeInFSharpForm curriedArgInfos tau range0
            let argtysl = if v.IsInstanceMember then argtysl.Tail else argtysl
            
            [ for argtys in argtysl do 
                 yield 
                   [ for argty, argInfo in argtys do 
                        yield FSharpParameter(env,argty,argInfo) ] 
                   |> makeReadOnlyCollection ]
             |> makeReadOnlyCollection

        
    member x.ReturnParameter  = 

        let env = Env(v.Typars)
        match v.TopValInfo with 
        | None -> failwith "not a module let binding or member"; 
        | Some (TopValInfo(typars,argInfos,retInfo)) -> 
        
            let tau = v.TauType
            let _,rty = GetTopTauTypeInFSharpForm argInfos tau range0
            
            FSharpParameter(env,rty,retInfo) 


    member x.Attributes = v.Attribs |> List.map (fun a -> FSharpAttribute(a)) |> makeReadOnlyCollection
     
(*
    /// Is this "base" in "base.M(...)"
    member x.IsBaseValue : bool

    /// Is this the "x" in "type C() as x = ..."
    member x.IsConstructorThisValue : bool

    /// Is this the "x" in "member x.M = ..."
    member x.IsMemberThisValue : bool

    /// Is this a [&lt;Literal&gt;] value, and if so what value?
    member x.LiteralValue : obj // may be null

      /// How visible is this? 
    member x.Accessibility : FSharpAccessibility

      /// Get the module, type or namespace where this value appears. For 
      /// an extension member this is the type being extended 
    member x.ApparentParent: FSharpEntity

     /// Get the module, type or namespace where this value is compiled
    member x.ActualParent: FSharpEntity;

*)


and [<Sealed>] FSharpType(env:Env, typ:typ) =

    member x.IsNamed = (match typ with TType_app _ -> true | _ -> false)
    member x.IsTuple = (match typ with TType_tuple _ -> true | _ -> false)

    member x.NamedEntity = 
        match typ with 
        | TType_app (tcref,_) -> FSharpEntity(tcref) 
        | _ -> invalidOp "not a named type"

    member x.GenericArguments = 
        match typ with 
        | TType_app (_,tyargs) 
        | TType_tuple (tyargs) -> (tyargs |> List.map (fun ty -> FSharpType(env,ty)) |> makeReadOnlyCollection) 
        | TType_fun(d,r) -> [| FSharpType(env,d); FSharpType(env,r) |] |> makeReadOnlyCollection
        | _ -> invalidOp "not a named type"


    member x.IsFunction = (match typ with TType_fun _ -> true | _ -> false)

    member x.IsGenericParameter= 
        match typ with 
        | TType_var _ -> true 
        | TType_measure (MeasureVar _) -> true 
        | _ -> false

    member x.GenericParameter = 
        match typ with 
        | TType_var tp 
        | TType_measure (MeasureVar tp) -> 
            FSharpGenericParameter (env, env.Typars |> Array.find (fun tp2 -> typar_ref_eq tp tp2)) 
        | _ -> invalidOp "not a generic parameter type"

    member x.GenericParameterIndex = 
        match typ with 
        | TType_var tp 
        | TType_measure (MeasureVar tp) -> 
            env.Typars |> Array.findIndex (fun tp2 -> typar_ref_eq tp tp2)
        | _ -> invalidOp "not a generic parameter type"

    //member IsMeasureProduct : bool
    //member GetMeasureProductLeft : FSharpType
    //member GetMeasureProductRight : FSharpType

    //member IsMeasureInverse : bool
    //member GetMeasureProductInverse : FSharpType

    //member IsMeasureOne : bool

and [<Sealed>] FSharpAttribute(attrib) = 

    let (Attrib(tcref,kind,unnamedArgs,propVals,m)) = attrib

    member x.GetReflectionType() : System.Type = 
        match kind with 
        | ILAttrib(mspec) -> 
            System.Type.GetType(mspec.EnclosingTypeRef.QualifiedName)
        | FSAttrib(vref) -> 
            System.Type.GetType(tcref.CompiledRepresentationForTyrepNamed.QualifiedName)

    member x.Value = 
        let ty = x.GetReflectionType()
        let fail() = failwith "This custom attribute has an argument that can not yet be converted using this API"
        let evalArg e = 
            match e with
            | TExpr_const(c,m,_) -> 
                match c with 
                | TConst_bool b -> box b
                | TConst_sbyte  i  -> box i
                | TConst_int16  i  -> box  i
                | TConst_int32 i   -> box i
                | TConst_int64 i   -> box i  
                | TConst_byte i    -> box i
                | TConst_uint16 i  -> box i
                | TConst_uint32 i  -> box i
                | TConst_uint64 i  -> box i
                | TConst_float i   -> box i
                | TConst_float32 i -> box i
                | TConst_char i    -> box i
                | TConst_zero -> null
                | TConst_string s ->  box s
                | _ -> fail()
            | _ -> fail()
        let args = unnamedArgs |> List.map (fun (AttribExpr(_,e)) -> evalArg e)
        let res = System.Activator.CreateInstance(ty,args)
        propVals |> List.iter (fun (AttribNamedArg(nm,_,isField,AttribExpr(_, e))) -> 
            ty.InvokeMember(nm,BindingFlags.Public ||| BindingFlags.NonPublic ||| (if isField then BindingFlags.SetField else BindingFlags.SetProperty),
                            null,res,[| evalArg e |]) |> ignore)
        res
        

    
and [<Sealed>] FSharpParameter(env:Env,typ:typ,topArgInfo:TopArgInfo) = 
    let (TopArgInfo(attribs,idOpt)) = topArgInfo
    member x.Name = match idOpt with None -> null | Some v -> v.idText
    member x.Type = FSharpType(env,typ)
    member x.Range = Range(match idOpt with None -> range0 | Some v -> v.idRange)
    member x.Attributes = attribs |> List.map (fun a -> FSharpAttribute(a)) |> makeReadOnlyCollection
    
    
