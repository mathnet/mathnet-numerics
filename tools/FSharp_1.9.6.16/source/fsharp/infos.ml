// (c) Microsoft Corporation. All rights reserved

/// tinfos, minfos, finfos, pinfos - summaries of information for references
/// to .NET and F# constructs.

#light

module (* internal *) Microsoft.FSharp.Compiler.Infos

open Internal.Utilities
open Internal.Utilities.Pervasives
open Microsoft.FSharp.Compiler.AbstractIL 
open Microsoft.FSharp.Compiler.AbstractIL.IL 
open Microsoft.FSharp.Compiler.AbstractIL.Internal 
open Microsoft.FSharp.Compiler.AbstractIL.Internal.Library
open Microsoft.FSharp.Compiler 

open Microsoft.FSharp.Compiler.AbstractIL.Diagnostics
open Microsoft.FSharp.Compiler.Range
open Microsoft.FSharp.Compiler.Ast
open Microsoft.FSharp.Compiler.ErrorLogger
open Microsoft.FSharp.Compiler.Tast
open Microsoft.FSharp.Compiler.Tastops
open Microsoft.FSharp.Compiler.Env
open Microsoft.FSharp.Compiler.AbstractIL.IL (* Abstract IL  *)
open Microsoft.FSharp.Compiler.Lib
open Microsoft.FSharp.Text.Printf

//-------------------------------------------------------------------------
// From IL types to F# types
//------------------------------------------------------------------------- 

/// importInst gives the context for interpreting type variables 
let ImportType scoref amap m importInst ilty = 
    ilty |> rescope_typ scoref |>  Import.ImportILType amap m importInst 

//-------------------------------------------------------------------------
// Fold the hierarchy. 
//  REVIEW: this code generalizes the iteration used below for member lookup.
//------------------------------------------------------------------------- 

let is_fsobjmodel_or_exn_typ g typ = 
    is_fsobjmodel_typ g typ  || 
    (is_stripped_tyapp_typ g typ && (tcref_of_stripped_typ g typ).IsExceptionDecl)
    
let SuperTypeOfType g amap m typ = 
    let typ = strip_tpeqns_and_tcabbrevs_and_measureable g typ
    if is_il_named_typ g typ then 
        let tcref,tinst = dest_stripped_tyapp_typ g typ
        let scoref,_,tdef = tcref.ILTyconInfo
        match tdef.tdExtends with 
        | None -> None
        | Some ilty -> Some (ImportType scoref amap m tinst ilty)
    elif is_fsobjmodel_or_exn_typ g typ then 
        let tcref,tinst = dest_stripped_tyapp_typ g typ
        Some (InstType (mk_inst_for_stripped_typ g typ) (super_of_tycon g (deref_tycon tcref)))
    elif is_any_array_typ g typ then
        Some(g.system_Array_typ)
    elif is_ref_typ g typ && not (is_obj_typ g typ) then 
        Some(g.obj_ty)
    elif is_tuple_struct_typ g typ then 
        //Some(g.system_Value_typ)
        Some(g.obj_ty)
    else None

let mk_System_Collections_Generic_IList_ty g ty = TType_app(g.tcref_System_Collections_Generic_IList,[ty])


let ImplementsOfType g amap m typ = 
    let itys = 
        if is_stripped_tyapp_typ g typ then
            let tcref,tinst = dest_stripped_tyapp_typ g typ
            if tcref.IsMeasureableReprTycon then             
                [g.mk_IComparable_ty; 
                 g.mk_IConvertible_ty; 
                 g.mk_IFormattable_ty; 
                 mk_tyapp_ty g.system_GenericIComparable_tcref [typ]; 
                 mk_tyapp_ty g.system_GenericIEquatable_tcref [typ]]
            elif tcref.IsILTycon then 
                let scoref,_,tdef = tcref.ILTyconInfo
                List.map (ImportType scoref amap m tinst) tdef.tdImplements
            else  
                let inst = mk_inst_for_stripped_typ g typ
                List.map (fun (x,_,_) -> InstType inst x) tcref.TypeContents.tcaug_implements
        else []
        
    let itys =
        if is_il_arr1_typ g typ then 
            mk_System_Collections_Generic_IList_ty g (dest_il_arr1_typ g typ) :: itys
        else 
            itys
    itys
        
    
// Traverse the type hierarchy, e.g. f D (f C (f System.Object acc)). 
let rec FoldHierarchyOfTypeAux ndeep followInterfaces f g amap m typ (visited,acc) =
    if ListSet.mem (type_equiv g) typ visited  then visited,acc else
    let state = typ::visited, acc
    if verbose then  dprintf "--> FoldHierarchyOfTypeAux, ndeep = %d, typ = %s...\n" ndeep ((DebugPrint.showType typ));
    if ndeep > 100 then (errorR(Error("recursive class hierarchy (detected in FoldHierarchyOfTypeAux), typ = "^(DebugPrint.showType typ),m)); (visited,acc)) else
    let visited,acc = 
        if is_interface_typ g typ then 
            List.foldBack 
               (FoldHierarchyOfTypeAux (ndeep+1) followInterfaces f g amap m) 
               (ImplementsOfType g amap m typ) 
                  (FoldHierarchyOfTypeAux ndeep followInterfaces f g amap m g.obj_ty state)
        elif is_typar_typ g typ then 
            let tp = dest_typar_typ g typ
            let state = FoldHierarchyOfTypeAux (ndeep+1) followInterfaces f g amap m g.obj_ty state 
            List.foldBack 
                (fun x vacc -> 
                  match x with 
                  | TTyparMayResolveMemberConstraint _
                  | TTyparDefaultsToType _
                  | TTyparIsEnum _
                  | TTyparIsDelegate _
                  | TTyparSupportsNull _
                  | TTyparIsNotNullableValueType _ 
                  | TTyparIsReferenceType _ 
                  | TTyparSimpleChoice _ 
                  | TTyparRequiresDefaultConstructor _ -> vacc
                  | TTyparCoercesToType(cty,_) -> 
                          FoldHierarchyOfTypeAux (ndeep + 1)  followInterfaces f g amap m cty vacc) 
                tp.Constraints 
                state
        else 
            let state = 
                if followInterfaces then 
                    List.foldBack 
                      (FoldHierarchyOfTypeAux (ndeep+1) followInterfaces f g amap m) 
                      (ImplementsOfType g amap m typ) 
                      state 
                else 
                    state
            let state = 
                Option.fold_right 
                  (FoldHierarchyOfTypeAux (ndeep+1) followInterfaces f g amap m) 
                  (SuperTypeOfType g amap m typ) 
                  state
            state
                                
    (visited,f typ acc) 

/// Fold, do not follow interfaces
let FoldPrimaryHierarchyOfType f g amap m typ acc = FoldHierarchyOfTypeAux 0 false f g amap m typ ([],acc) |> snd

/// Fold, following interfaces
let FoldEntireHierarchyOfType f g amap m typ acc = FoldHierarchyOfTypeAux 0 true f g amap m typ ([],acc) |> snd

/// Iterate, following interfaces
let IterateEntireHierarchyOfType f g amap m typ = FoldHierarchyOfTypeAux 0 true (fun ty () -> f ty) g amap m typ ([],())  |> snd

let ExistsInEntireHierarchyOfType f g amap m typ = 
    FoldHierarchyOfTypeAux 0 true (fun ty acc -> acc || f ty ) g amap m typ ([],false) |> snd

let SearchEntireHierarchyOfType f g amap m typ = 
    FoldHierarchyOfTypeAux 0 true 
        (fun ty acc -> 
            match acc with 
            | None -> if f ty then Some(ty) else None 
            | Some _ -> acc) 
        g amap m typ ([],None) 
    |> snd

let AllSuperTypesOfType g amap m ty = FoldHierarchyOfTypeAux 0 true (ListSet.insert (type_equiv g)) g amap m ty ([],[]) |> snd



let mdef_is_ctor md = md.mdName = ".ctor" 
let mdef_is_cctor md = md.mdName = ".cctor" 

let mdef_is_protected md = 
    not (mdef_is_ctor md) &&
    not (mdef_is_cctor md) &&
    (md.mdAccess = MemAccess_family) &&
    not md.mdCallconv.IsStatic 

let ImportTypeFromMetadata amap m scoref tinst minst ilty = 
    ImportType scoref amap m (tinst@minst) ilty

//-------------------------------------------------------------------------
// Predicates and properties on values and members
//------------------------------------------------------------------------- 
 
let PropertyNameOfMemberValRef (vref:ValRef) = 
    assert(vref.IsMember)
    (the (vref.MemberInfo)).PropertyName

let MemberRefIsVirtual (vref:ValRef) = 
    let flags = vref.MemberInfo.Value.MemberFlags
    flags.MemberIsVirtual || flags.MemberIsDispatchSlot || flags.MemberIsOverrideOrExplicitImpl

// REVIEW: This whole predicate is very dubious. We should not need the notion of "DefiniteFSharpOverride" at all
let MemberRefIsDefiniteFSharpOverride (vref:ValRef) = 
    let membInfo = vref.MemberInfo.Value
    let flags = membInfo.MemberFlags
    not flags.MemberIsDispatchSlot && (flags.MemberIsOverrideOrExplicitImpl || nonNil membInfo.ImplementedSlotSigs)

let MemberRefIsDispatchSlot (vref:ValRef) =  
    let membInfo = vref.MemberInfo.Value
    membInfo.MemberFlags.MemberIsDispatchSlot 

let MemberRefIsAbstract (vref:ValRef) =  
    let membInfo = vref.MemberInfo.Value
    membInfo.MemberFlags.MemberIsDispatchSlot && not membInfo.IsImplemented

type ValRef with 
    member x.IsFSharpEventProperty(g) = 
        x.IsMember && CompileAsEvent g x.Attribs && not x.IsExtensionMember

//-------------------------------------------------------------------------
// Basic infos
//------------------------------------------------------------------------- 

type ILTypeInfo = 
    | ILTypeInfo of TyconRef * ILTypeRef * tinst * ILTypeDef
    member x.TyconRef    = let (ILTypeInfo(tcref,_,_,_)) = x in tcref
    member x.ILTypeRef   = let (ILTypeInfo(_,tref,_,_))  = x in tref
    member x.TypeInst    = let (ILTypeInfo(_,_,tinst,_)) = x in tinst
    member x.RawMetadata = let (ILTypeInfo(_,_,_,tdef))  = x in tdef
    member x.ToType   = TType_app(x.TyconRef,x.TypeInst)
    member x.ILScopeRef = x.ILTypeRef.Scope
    member x.Name     = x.ILTypeRef.Name
    member x.IsValueType = is_value_or_enum_tdef x.RawMetadata


type TypeInfo = 
    | ILType of ILTypeInfo
    | FSType of Tast.typ

type ILMethInfo =
    | ILMethInfo of ILTypeInfo * ILTypeRef option (* extension? *) * ILMethodDef * typars (* typars are the uninstantiated generic method args *) 

    member x.ILTypeInfo = let (ILMethInfo(tinfo,_,_,_)) = x in tinfo
    member x.RawMetadata = let (ILMethInfo(_,_,md,_)) = x in md
    member x.ExtensionMethodInfo = let (ILMethInfo(_,extInfo,_,_)) = x in extInfo
    member x.ILTypeRef = x.ILTypeInfo.ILTypeRef
    member x.ILName       = x.RawMetadata.Name

    // methods to hide logic related to extension methods
    member x.IsCSharpExtensionMethod = x.ExtensionMethodInfo.IsSome

    member x.ActualILTypeRef   = 
        match x.ExtensionMethodInfo with 
        | None -> x.ILTypeRef
        | Some info -> info

    member x.ActualTypeInst = 
        match x.ExtensionMethodInfo with 
        | None -> x.ILTypeInfo.TypeInst
        | Some info -> []

    member x.MetadataScope   = x.ActualILTypeRef.Scope
    
    member x.ParamMetadata = 
        let ps = x.RawMetadata.mdParams in 
        if x.IsCSharpExtensionMethod then List.tl ps else ps

    member x.NumParams = x.ParamMetadata.Length
    
    member x.GenericArity = x.RawMetadata.mdGenericParams.Length 
   
    member x.IsConstructor = x.RawMetadata |> mdef_is_ctor 
    member x.IsClassConstructor = x.RawMetadata |> mdef_is_cctor
    member x.IsProtectedAccessibility = x.RawMetadata |> mdef_is_protected
    member x.IsVirtual = x.RawMetadata.IsVirtual
    member x.IsFinal = x.RawMetadata.IsFinal

    member x.IsAbstract = 
        match x.RawMetadata.mdKind with 
        | MethodKind_virtual vinfo -> vinfo.virtAbstract 
        | _ -> false

    /// Does it appear to the user as a static method?
    member x.IsStatic = 
        not x.IsCSharpExtensionMethod &&  // all C# extension methods are instance
        x.RawMetadata.mdCallconv.IsStatic

    /// Does it have the .NET IL 'newslot' flag set, and is also a virtual?
    member x.IsNewSlot = 
        match x.RawMetadata.mdKind with 
        | MethodKind_virtual vinfo -> vinfo.virtNewslot 
        | _ -> false
    
    /// Does it appear to the user as an instance method?
    member x.IsInstance = not x.IsConstructor &&  not x.IsStatic

    member x.ArgTypes(amap,m,minst) = 
        x.ParamMetadata |> List.map (fun p -> ImportTypeFromMetadata amap m x.MetadataScope x.ActualTypeInst minst p.Type) 

    member x.ParamInfos(amap,m,minst) = 
        x.ParamMetadata |> List.map (fun p -> p.Name, ImportTypeFromMetadata amap m x.MetadataScope x.ActualTypeInst minst p.Type) 

    member x.EnclosingType = x.ILTypeInfo.ToType



type MethInfo = 
    | FSMeth of TcGlobals * Tast.typ * ValRef  
    | ILMeth of TcGlobals * ILMethInfo
    | DefaultStructCtor of TcGlobals * Tast.typ
    /// Get the enclosing ("parent") type of the method info. 
    member x.EnclosingType = 
      match x with
      | ILMeth(g,x) -> x.EnclosingType
      | FSMeth(g,typ,_) -> typ
      | DefaultStructCtor(g,typ) -> typ

type ILFieldInfo = 
    | ILFieldInfo of ILTypeInfo * ILFieldDef (* .NET IL fields *)

    member x.ILTypeInfo = (let (ILFieldInfo(tinfo,_)) = x in tinfo)
    member x.RawMetadata = (let (ILFieldInfo(_,pd)) = x in pd)
    member x.ScopeRef = x.ILTypeInfo.ILScopeRef
    member x.ILTypeRef = x.ILTypeInfo.ILTypeRef
    member x.TypeInst = x.ILTypeInfo.TypeInst
    member x.FieldName = x.RawMetadata.fdName
    member x.IsInitOnly = x.RawMetadata.fdInitOnly
    member x.IsValueType = x.ILTypeInfo.IsValueType
    member x.IsStatic = x.RawMetadata.fdStatic
    member x.LiteralValue = if x.RawMetadata.fdLiteral then x.RawMetadata.fdInit else None
    member x.ILFieldRef= rescope_fref x.ScopeRef (mk_fref_in_tref(x.ILTypeRef,x.FieldName,x.RawMetadata.fdType))

type RecdFieldInfo = 
    | RecdFieldInfo of tinst * Tast.RecdFieldRef (* F# fields *)
    member x.TypeInst = let (RecdFieldInfo(tinst,_)) = x in tinst
    member x.RecdFieldRef = let (RecdFieldInfo(_,rfref)) = x in rfref
    member x.RecdField = x.RecdFieldRef.RecdField
    member x.IsStatic = x.RecdField.IsStatic
    member x.LiteralValue = x.RecdField.LiteralValue
    member x.TyconRef = x.RecdFieldRef.TyconRef
    member x.Tycon = x.RecdFieldRef.Tycon
    member x.Name = x.RecdField.Name
    member x.FieldType = actual_rtyp_of_rfref x.RecdFieldRef x.TypeInst
    member x.EnclosingType = TType_app (x.RecdFieldRef.TyconRef,x.TypeInst)
    
type UnionCaseInfo = 
    | UnionCaseInfo of tinst * Tast.UnionCaseRef 
    member x.TypeInst = let (UnionCaseInfo(tinst,_)) = x in tinst
    member x.UnionCaseRef = let (UnionCaseInfo(_,ucref)) = x in ucref
    member x.UnionCase = x.UnionCaseRef.UnionCase
    member x.TyconRef = x.UnionCaseRef.TyconRef
    member x.Tycon = x.UnionCaseRef.Tycon
    member x.Name = x.UnionCase.DisplayName


type ILPropInfo = 
    | ILPropInfo of ILTypeInfo * ILPropertyDef 

    member x.ILTypeInfo = match x with (ILPropInfo(tinfo,_)) -> tinfo
    member x.RawMetadata = match x with (ILPropInfo(_,pd)) -> pd
    member x.PropertyName = x.RawMetadata.propName

    member x.GetterMethod = 
        assert (x.HasGetter)
        let mdef = resolve_mref x.ILTypeInfo.RawMetadata (the x.RawMetadata.propGet)
        ILMethInfo(x.ILTypeInfo,None,mdef,[]) 

    member x.SetterMethod = 
        assert (x.HasSetter)
        let mdef = resolve_mref x.ILTypeInfo.RawMetadata (the x.RawMetadata.propSet)
        ILMethInfo(x.ILTypeInfo,None,mdef,[]) 

    member x.HasGetter = isSome x.RawMetadata.propGet 
    member x.HasSetter = isSome x.RawMetadata.propSet 
    member x.IsStatic = (x.RawMetadata.propCallconv = CC_static) 


type PropInfo = 
    | FSProp of TcGlobals * Tast.typ * ValRef option * ValRef option
    | ILProp of TcGlobals * ILPropInfo

type ILEventInfo = 
    | ILEventInfo of ILTypeInfo * ILEventDef
    member x.RawMetadata = match x with (ILEventInfo(_,ed)) -> ed
    member x.ILTypeInfo = match x with (ILEventInfo(tinfo,_)) -> tinfo
    member x.AddMethod =
        let mdef = resolve_mref x.ILTypeInfo.RawMetadata x.RawMetadata.eventAddOn
        ILMethInfo(x.ILTypeInfo,None,mdef,[]) 

    member x.RemoveMethod =
        let mdef = resolve_mref x.ILTypeInfo.RawMetadata x.RawMetadata.eventRemoveOn
        ILMethInfo(x.ILTypeInfo,None,mdef,[]) 

    member x.TypeRef = x.ILTypeInfo.ILTypeRef
    member x.Name = x.RawMetadata.eventName
    member x.IsStatic = x.AddMethod.IsStatic


type EventInfo = 
    | FSEvent of TcGlobals * PropInfo * ValRef * ValRef
    | ILEvent of TcGlobals * ILEventInfo


/// Copy constraints.  If the constraint comes from a type parameter associated
/// with a type constructor then we are simply renaming type variables.  If it comes
/// from a generic method in a generic class (e.g. typ.M<_>) then we may be both substituting the
/// instantiation associated with 'typ' as well as copying the type parameters associated with 
/// M and instantiating their constraints
///
/// Note: this now looks identical to constraint instantiation.

let CopyTyparConstraints m tprefInst (tporig:Typar) =
    tporig.Constraints 
    |>  List.map (fun tpc -> 
           match tpc with 
           | TTyparCoercesToType(ty,_) -> 
               TTyparCoercesToType (InstType tprefInst ty,m)
           | TTyparDefaultsToType(priority,ty,_) -> 
               TTyparDefaultsToType (priority,InstType tprefInst ty,m)
           | TTyparSupportsNull _ -> 
               TTyparSupportsNull m
           | TTyparIsEnum(uty,_) -> 
               TTyparIsEnum (InstType tprefInst uty,m)
           | TTyparIsDelegate(aty, bty,_) -> 
               TTyparIsDelegate (InstType tprefInst aty,InstType tprefInst bty,m)
           | TTyparIsNotNullableValueType _ -> 
               TTyparIsNotNullableValueType m
           | TTyparIsReferenceType _ -> 
               TTyparIsReferenceType m
           | TTyparSimpleChoice (tys,_) -> 
               TTyparSimpleChoice (List.map (InstType tprefInst) tys,m)
           | TTyparRequiresDefaultConstructor _ -> 
               TTyparRequiresDefaultConstructor m
           | TTyparMayResolveMemberConstraint(traitInfo,_) -> 
               TTyparMayResolveMemberConstraint (inst_trait tprefInst traitInfo,m))

/// The constraints for each typar copied from another typar can only be fixed up once 
/// we have generated all the new constraints, e.g. f<A :> List<B>, B :> List<A>> ... 
let FixupNewTypars m ftctps tinst tpsorig tps =
    let renaming,tptys = (mk_typar_to_typar_renaming tpsorig tps)
    let tprefInst = (mk_typar_inst ftctps tinst) @ renaming
    List.iter2 (fun tporig tp -> fixup_typar_constraints tp (CopyTyparConstraints  m tprefInst tporig)) tpsorig tps;
    renaming,tptys


//-------------------------------------------------------------------------
// tinfos
//------------------------------------------------------------------------- 

let inst_il_tinfo inst (ILTypeInfo(tcref,tref,tinst,tdef)) = ILTypeInfo(tcref,tref,inst_types inst tinst,tdef)

let FormalTyparsOfILTypeInfo m (x:ILTypeInfo) = x.TyconRef.Typars(m)

let tdef_of_il_typ g ty = (tcref_of_stripped_typ g ty).ILTyconRawMetadata

let tinfo_of_il_typ g ty = 
    if is_il_named_typ g ty then 
        let tcref,tinst = dest_stripped_tyapp_typ g ty
        let scoref,enc,tdef = tcref.ILTyconInfo
        let tref = tref_for_nested_tdef scoref (enc,tdef)
        ILTypeInfo(tcref,tref,tinst,tdef)
    else 
        failwith "tinfo_of_il_typ"


/// Build IL method infos.  
let mk_il_minfo amap m (tinfo:ILTypeInfo) (extInfo:ILTypeRef option) md =     
    let tinst,scoref =  
        match extInfo with 
        | None -> 
            tinfo.TypeInst,tinfo.ILScopeRef
        | Some tref -> 
            // C# extension methods have no type typars
            [], tref.Scope
    let mtps = Import.ImportIlTypars (fun () -> amap) m scoref tinst md.mdGenericParams
    ILMeth (amap.g,ILMethInfo(tinfo,extInfo, md,mtps))

//-------------------------------------------------------------------------
// il_minfo, il_pinfo
//------------------------------------------------------------------------- 


// Get the logical object parameters of a type
let objtys_of_il_minfo amap m (x:ILMethInfo) minst =
    // all C# extension methods are instance
    if x.IsCSharpExtensionMethod then 
        x.RawMetadata.Parameters |> List.hd |> (fun p -> [ImportTypeFromMetadata amap m x.MetadataScope x.ActualTypeInst minst p.Type]) 
    elif x.IsInstance then 
        [x.EnclosingType]
    else
        []
        

let ImportReturnTypeFromMetaData amap m ty scoref tinst minst =
    match ty with 
    | Type_void -> None
    | retTy ->  Some (ImportTypeFromMetadata amap m scoref tinst minst retTy)

let GetCompiledReturnTyOfILMethod amap m (x:ILMethInfo) minst =
    ImportReturnTypeFromMetaData amap m x.RawMetadata.mdReturn.Type x.MetadataScope x.ActualTypeInst minst 

let GetFSharpReturnTyOfILMethod amap m minfo minst = 
    GetCompiledReturnTyOfILMethod amap m minfo minst 
    |> GetFSharpViewOfReturnType amap.g

let mref_of_il_minfo (minfo:ILMethInfo) = 
    let mref = mk_mref_to_mdef (minfo.ActualILTypeRef,minfo.RawMetadata)
    rescope_mref minfo.MetadataScope mref 


let inst_il_minfo amap m inst (x:ILMethInfo) = 
    mk_il_minfo amap m (inst_il_tinfo inst x.ILTypeInfo) x.ExtensionMethodInfo x.RawMetadata

let il_minfo_is_DllImport g (minfo:ILMethInfo) = 
    let (AttribInfo(tref,_)) = g.attrib_DllImportAttribute
    minfo.RawMetadata.mdCustomAttrs |> ILThingDecodeILAttrib g tref |> isSome

/// Build an expression node that is a call to a .NET method. *)
let mk_il_minfo_call g amap m isProp (minfo:ILMethInfo) vFlags minst direct args = 
    let isStatic = not (minfo.IsConstructor || minfo.IsInstance)
    let valu = minfo.ILTypeInfo.IsValueType
    let ctor = minfo.IsConstructor
    if minfo.IsClassConstructor then 
        error (InternalError (minfo.ILName^": cannot call a class constructor",m));
    let useCallvirt = 
        not valu  && not direct && minfo.IsVirtual
    let isProtected = minfo.IsProtectedAccessibility
    let mref = mref_of_il_minfo minfo
    let newobj = ctor && (vFlags = NormalValUse)
    let exprty = if ctor then minfo.EnclosingType else GetFSharpReturnTyOfILMethod amap m minfo minst
    // The thing might be an extension method, in which case adjust the instantiations
    let actualTypeInst = minfo.ActualTypeInst
    let actualMethInst = minst
    let retTy = (if not ctor && (mref.ReturnType = IL.Type_void) then [] else [exprty])
    let isDllImport = il_minfo_is_DllImport g minfo
    TExpr_op(TOp_ilcall((useCallvirt,isProtected,valu,newobj,vFlags,isProp,isDllImport,None,mref),actualTypeInst,actualMethInst, retTy),[],args,m),
    exprty
  
let mk_obj_ctor_call g m =
    let mref = (mk_nongeneric_ctor_mspec(g.ilg.tref_Object,AsObject,[])).MethodRef
    TExpr_op(TOp_ilcall((false,false,false,false,CtorValUsedAsSuperInit,false,false,None,mref),[],[],[g.obj_ty]),[],[],m)

//-------------------------------------------------------------------------
// .NET Property Infos
//------------------------------------------------------------------------- 

let pdef_accessibility tdef pd =   
    match pd.propGet with 
    | Some mref -> (resolve_mref tdef mref).mdAccess 
    | None -> 
        match pd.propSet with 
          None -> MemAccess_public
        | Some mref -> (resolve_mref tdef mref).mdAccess

let il_pinfo_is_protected (pinfo:ILPropInfo) =  
    (pinfo.HasGetter && pinfo.GetterMethod.IsProtectedAccessibility) ||
    (pinfo.HasSetter && pinfo.SetterMethod.IsProtectedAccessibility) 

let il_pinfo_is_virt (pinfo:ILPropInfo) = 
    (pinfo.HasGetter && pinfo.GetterMethod.IsVirtual) ||
    (pinfo.HasSetter && pinfo.SetterMethod.IsVirtual) 

let il_pinfo_is_newslot (pinfo:ILPropInfo) = 
    (pinfo.HasGetter && pinfo.GetterMethod.IsNewSlot) ||
    (pinfo.HasSetter && pinfo.SetterMethod.IsNewSlot) 

let il_pinfo_is_abstract (pinfo:ILPropInfo) = 
    (pinfo.HasGetter && pinfo.GetterMethod.IsAbstract) ||
    (pinfo.HasSetter && pinfo.SetterMethod.IsAbstract) 

let params_of_il_pinfo amap m (ILPropInfo (tinfo,pdef)) =
    pdef.propArgs |> List.map (fun ty -> None, ImportTypeFromMetadata amap m tinfo.ILScopeRef tinfo.TypeInst [] ty) 

let vtyp_of_il_pinfo amap m (ILPropInfo(tinfo,pdef)) =
    ImportTypeFromMetadata amap m tinfo.ILScopeRef tinfo.TypeInst [] pdef.propType

//-------------------------------------------------------------------------
// .NET Event Infos
//------------------------------------------------------------------------- 


let edef_accessibility tdef ed = (resolve_mref tdef ed.eventAddOn).mdAccess 

let DelegateTypeOfILEventInfo amap m (ILEventInfo(tinfo,edef)) =
    ImportTypeFromMetadata amap m tinfo.ILScopeRef tinfo.TypeInst [] (the edef.eventType)


//-------------------------------------------------------------------------
// Testing equality & calculating hash codes of method/prop/event infos
//------------------------------------------------------------------------- 

/// Do two minfos have the same underlying definitions? 
/// Used to merge operator overloads collected from left and right of an operator constraint 
let MethInfosUseIdenticalDefinitions g x1 x2 = 
    match x1,x2 with 
    | ILMeth(g,x1), ILMeth(_,x2) -> (x1.RawMetadata ===  x2.RawMetadata)
    | FSMeth(g,ty1,vref1), FSMeth(_,ty2,vref2)  -> g.vref_eq vref1 vref2 
    | DefaultStructCtor(g,ty1), DefaultStructCtor(_,ty2) -> tcref_eq g (tcref_of_stripped_typ g ty1) (tcref_of_stripped_typ g ty2) 
    | _ -> false

/// Tests whether two property infos have the same underlying defintion
/// (uses the same techniques as pervious 'MethInfosUseIdenticalDefinitions')
let PropInfosUseIdenticalDefinitions x1 x2 = 
    let optVrefEq g = function 
      | Some(v1), Some(v2) -> g.vref_eq v1 v2
      | None, None -> true
      | _ -> false    
    match x1,x2 with 
    | ILProp(g, x1), ILProp(_, x2) -> (x1.RawMetadata === x2.RawMetadata)
    | FSProp(g, ty1, vrefa1, vrefb1), FSProp(_, ty2, vrefa2, vrefb2) ->
        (optVrefEq g (vrefa1, vrefa2)) && (optVrefEq g (vrefb1, vrefb2))
    | _ -> false

/// Test whether two event infos have the same underlying defintion (similar as above)
let EventInfosUseIdenticalDefintions x1 x2 =
    match x1, x2 with
    | FSEvent(g, pi1, vrefa1, vrefb1), FSEvent(_, pi2, vrefa2, vrefb2) ->
        PropInfosUseIdenticalDefinitions pi1 pi2 && g.vref_eq vrefa1 vrefa2 && g.vref_eq vrefb1 vrefb2
    | ILEvent(g, x1), ILEvent(_, x2) -> (x1.RawMetadata === x2.RawMetadata)
    | _ -> false
  
/// Calculates a hash code of method info. Note: this is a very imperfect implementation,
/// but it works decently for comparing methods in the language service...
let GetMethInfoHashCode mi = 
    match mi with 
    | ILMeth(_,x1) -> hash x1.RawMetadata.Name
    | FSMeth(_,_,vref) -> hash vref.CompiledName
    | DefaultStructCtor(_,ty) -> hash ty

/// Calculates a hash code of property info (similar as previous)
let GetPropInfoHashCode mi = 
    match mi with 
    | ILProp(_, x1) -> hash x1.RawMetadata.Name
    | FSProp(_,_,vrefOpt1, vrefOpt2) -> 
        // Value to hash is option<string>*option<string>, which can be hashed efficiently
        let vth = vrefOpt1 |> Option.map (fun vr -> vr.CompiledName), vrefOpt2 |> Option.map (fun vr -> vr.CompiledName)
        hash(vth)

/// Calculates a hash code of event info (similar as previous)
let GetEventInfoHashCode mi = 
    match mi with 
    | ILEvent(_, x1) -> hash x1.RawMetadata.Name
    | FSEvent(_, pi, vref1, vref2) -> hash (GetPropInfoHashCode pi, vref1.CompiledName, vref2.CompiledName)

//-------------------------------------------------------------------------
// minfo, pinfo
//------------------------------------------------------------------------- 

/// Apply a type instantiation to a method info, i.e. apply the instantiation to the enclosing type. 
let InstMethInfo amap m inst = function 
  | ILMeth(g,x) -> inst_il_minfo amap m inst x
  | FSMeth(g,typ,vref) -> FSMeth(g,InstType inst typ,vref)
  | DefaultStructCtor(g,typ) -> DefaultStructCtor(g,InstType inst typ)

let AnalyzeTypeOfMemberVal g (typ,vref) = 
    (* if vref.RecursiveValInfo then retTy else *)
    let tps,_,retTy,_ = GetTypeOfMemberInMemberForm g vref
    
    let parentTyargs = tinst_of_stripped_typ g typ
    let memberParentTypars,memberMethodTypars = List.chop parentTyargs.Length tps

    memberParentTypars,memberMethodTypars,retTy,parentTyargs


type MethInfo with 
    member x.LogicalName = 
        match x with 
        | ILMeth(g,y) -> y.ILName
        | FSMeth(_,_,vref) -> vref.MemberInfo.Value.LogicalName
        | DefaultStructCtor _ -> ".ctor"

    member x.ActualTypeInst = 
        match x with 
        | ILMeth(g,y) -> y.ActualTypeInst
        | FSMeth(g,_,_) | DefaultStructCtor(g,_) -> tinst_of_stripped_typ g x.EnclosingType

    member x.FormalMethodTypars = 
        match x with 
        | ILMeth(g,ILMethInfo(tinfo,extInfo,_,mtps)) ->  mtps
        | FSMeth(g,typ,vref) ->  
           let _,mtps,_,_ = AnalyzeTypeOfMemberVal g (typ,vref)
           mtps 
        | DefaultStructCtor _ -> []
           
    member x.FormalMethodInst = generalize_typars x.FormalMethodTypars

    member x.XmlDoc = 
        match x with 
        | ILMeth(_,x) -> emptyXmlDoc
        | FSMeth(_,_,vref) -> vref.XmlDoc
        | DefaultStructCtor _ -> emptyXmlDoc

    member x.ArbitraryValRef = 
        match x with 
        | ILMeth(g,x) -> None
        | FSMeth(g,_,vref) -> Some(vref)
        | DefaultStructCtor _ -> None

let mk_fs_minfo_tinst ttps mtps tinst minst = (mk_typar_inst ttps tinst @ mk_typar_inst mtps minst) 

let CompiledReturnTyOfMeth amap m minfo minst = 
    match minfo with 
    | ILMeth(g,ilminfo) -> GetCompiledReturnTyOfILMethod amap m ilminfo minst
    | FSMeth(g,typ,vref) -> 
       let ttps,mtps,retTy,tinst = AnalyzeTypeOfMemberVal g (typ,vref)
       Option.map (InstType (mk_fs_minfo_tinst ttps mtps tinst minst)) retTy
    | DefaultStructCtor _ -> None

let FSharpReturnTyOfMeth amap m minfo minst =
    CompiledReturnTyOfMeth amap m minfo minst |> GetFSharpViewOfReturnType amap.g
       
let ParamOfArgInfo (ty,TopArgInfo(_,id)) = (Option.map text_of_id id,ty)

let ParamsOfMember g vref = ArgInfosOfMember g vref |> List.mapSquared ParamOfArgInfo

let InstParam inst param = 
    map2'2 (InstType inst) param

let InstParams inst paramTypes = 
    paramTypes |> List.mapSquared (InstParam inst)

let ParamTypesOfMethInfo amap m minfo minst = 
    match minfo with 
    | ILMeth(g,ilminfo) -> 
        [ ilminfo.ArgTypes(amap,m,minst) ]
    | FSMeth(g,typ,vref) -> 
        let ttps,mtps,_,tinst = AnalyzeTypeOfMemberVal g (typ,vref)
        let paramTypes = ParamsOfMember g vref
        let inst = (mk_fs_minfo_tinst ttps mtps tinst minst)
        paramTypes |> List.mapSquared (snd  >> InstType inst) 
    | DefaultStructCtor _ -> []

let ObjTypesOfMethInfo amap m minfo minst = 
    match minfo with 
    | ILMeth(g,ilminfo) -> objtys_of_il_minfo amap m ilminfo minst
    | FSMeth(g,typ,vref) -> if vref.IsInstanceMember then [typ] else []
    | DefaultStructCtor _ -> []


/// The caller-side value for the optional arg, is any 
type OptionalArgCallerSideValue = 
    | Constant of IL.ILFieldInit
    | DefaultValue
    | MissingValue
    | WrapperForIDispatch 
    | WrapperForIUnknown
    | PassByRef of Tast.typ * OptionalArgCallerSideValue
    
type OptionalArgInfo = 
    /// The argument is not optional
    | NotOptional
    /// The argument is optional, and is an F# callee-side optional arg 
    | CalleeSide
    /// The argument is optional, and is a caller-side .NET optional or default arg 
    | CallerSide of OptionalArgCallerSideValue 
    

let ParamAttribsOfMethInfo amap m minfo = 
    match minfo with 
    | ILMeth(g,x) -> 
        x.ParamMetadata
        |> List.map (fun p -> 
             let isParamArrayArg = ILThingHasAttrib g.attrib_ParamArrayAttribute p.paramCustomAttrs
             let isOutArg = (p.paramOut && not p.paramIn)
             (* Note: we get default argument values frmo VB and other .NET language metadata *)
             let optArgInfo = 
                 if p.paramOptional then 
                     CallerSide (match p.paramDefault with 
                                 | None -> 
                                        let rec analyze ty = 
                                            if is_byref_typ g ty then 
                                                let ty = dest_byref_typ g ty
                                                PassByRef (ty, analyze ty)
                                            elif is_obj_typ g ty then
                                                if ILThingHasAttrib g.attrib_IDispatchConstantAttribute p.paramCustomAttrs then
                                                    WrapperForIDispatch
                                                elif ILThingHasAttrib g.attrib_IUnknownConstantAttribute p.paramCustomAttrs then
                                                    WrapperForIUnknown
                                                else 
                                                    MissingValue
                                            else 
                                                DefaultValue
                                        analyze (ImportTypeFromMetadata amap m x.MetadataScope x.ActualTypeInst [] p.Type)

                                 | Some v -> Constant v)
                 else NotOptional
             (isParamArrayArg, isOutArg, optArgInfo))
        |> List.singleton
    | FSMeth(g,_,vref) -> 
        vref 
        |> ArgInfosOfMember g 
        |> List.mapSquared (fun (ty,TopArgInfo(attrs,nm)) -> 
            let isParamArrayArg = HasAttrib g g.attrib_ParamArrayAttribute attrs
            // Design Suggestion 1427: Can't specify "out" args in F# 
            let isOutArg = false 
            let isOptArg = HasAttrib g g.attrib_OptionalArgumentAttribute attrs
            // Note: can't specify caller-side default arguments in F#, by design (default is specified on the callee-side) 
            let optArgInfo = if isOptArg then CalleeSide else NotOptional
            (isParamArrayArg,isOutArg,optArgInfo))
    | DefaultStructCtor _ -> 
        [[]]

// REVIEW: should attributes always be empty here? 
let mk_slotparam (ty,TopArgInfo(attrs,nm)) = TSlotParam(Option.map text_of_id nm, ty, false,false,false,attrs) 

let mk_slotsig (nm,typ,ctps,mtps,paraml,retTy) = copy_slotsig (TSlotSig(nm,typ,ctps,mtps,paraml, retTy))

// slotsigs must contain the formal types for the arguments and return type 
// a _formal_ 'void' return type is represented as a 'unit' type. 
// slotsigs are independent of instantiation: if an instantiation 
// happens to make the return type 'unit' (i.e. it was originally a variable type 
// then that does not correspond to a slotsig compiled as a 'void' return type. 
// REVIEW: should we copy down attributes to slot params? 
let SlotsigOfILMethInfo g amap m  (ILMethInfo(tinfo,_,mdef,filmtps)) =
    let tcref = tcref_of_stripped_typ g tinfo.ToType
    let filtctps = tcref.Typars(m)
    let ftctps = CopyTypars filtctps
    let _,ftctptys = FixupNewTypars m [] [] filtctps ftctps
    let ftinfo = tinfo_of_il_typ g (TType_app(tcref,ftctptys))
    let fmtps = CopyTypars filmtps
    let _,fmtptys = FixupNewTypars m ftctps ftctptys filmtps fmtps
    let frty = ImportReturnTypeFromMetaData amap m mdef.mdReturn.Type ftinfo.ILScopeRef ftinfo.TypeInst fmtptys
    let fparams = 
        [ mdef.mdParams |> List.map (fun p -> 
            TSlotParam(p.paramName, ImportTypeFromMetadata amap m ftinfo.ILScopeRef ftinfo.TypeInst fmtptys p.Type,p.paramIn, p.paramOut, p.paramOptional,[])) ]
    mk_slotsig(mdef.mdName,tinfo.ToType,ftctps, fmtps,fparams, frty)

let SlotSigOfMethodInfo amap m minfo = 
    match minfo with 
    | ILMeth(g,x) -> SlotsigOfILMethInfo g amap m x
    | FSMeth(g,typ,vref) -> 
        match vref.RecursiveValInfo with 
        | ValInRecScope(false) -> error(Error("Invalid recursive reference to an abstract slot",m));
        | _ -> ()

        let tps,_,retTy,_ = GetTypeOfMemberInMemberForm g vref
        let ctps = (tcref_of_stripped_typ g typ).Typars(m)
        let ctpsorig,fmtps = List.chop ctps.Length tps
        let crenaming,_ = mk_typar_to_typar_renaming ctpsorig ctps
        let fparams = 
            vref 
            |> ArgInfosOfMember g 
            //|> (function [argInfos] -> argInfos | _ -> error(Error("An abstract slot may not have a curried type. Use a type 'M : arg1 * ... * argN -> result'",m)))
            |> List.mapSquared (map1'2 (InstType crenaming) >> mk_slotparam )
        let frty = Option.map (InstType crenaming) retTy
        mk_slotsig(minfo.LogicalName,minfo.EnclosingType,ctps,fmtps,fparams, frty)
    | DefaultStructCtor _ -> error(InternalError("no slotsig for DefaultStructCtor",m))

// The slotsig returned by SlotSigOfMethodInfo is in terms of the type parameters on the parent type of the overriding method,
//
// Reverse-map the slotsig so it is in terms of the type parameters for the overriding method 
let ReparentSlotSigToUseMethodTypars g amap m ovByMethValRef slotsig = 

    match PartitionValRefTypars g ovByMethValRef with
    | Some(_,ctps,_,_,_) -> 
        let parentToMemberInst,_ = mk_typar_to_typar_renaming (ovByMethValRef.MemberApparentParent.Typars(m)) ctps
        let res = inst_slotsig parentToMemberInst slotsig
        if verbose then dprintf "adjust slot %s, #parentToMemberInst = %d, before = %s, after = %s\n" (Layout.showL (ValRefL ovByMethValRef)) (List.length parentToMemberInst) (Layout.showL(SlotSigL slotsig)) (Layout.showL(SlotSigL res));
        res
    | None -> 
        (* Note: it appears PartitionValRefTypars should never return 'None' *)
        slotsig

type MethInfo with 
    member x.NumArgs = 
        match x with 
        | ILMeth(g,x) -> [x.NumParams]
        | FSMeth(g,_,vref) -> ParamsOfMember  g vref |> List.map List.length 
        | DefaultStructCtor _ -> [0]

    /// Does the method appear to the user as an instance method?
    member x.IsInstance = 
        match x with 
        | ILMeth(g,x) -> x.IsInstance
        | FSMeth(g,_,vref) -> vref.IsInstanceMember
        | DefaultStructCtor _ -> false

    member x.GenericArity = 
        match x with 
        | ILMeth(g,x) -> x.GenericArity
        | FSMeth(g,typ,vref) -> 
            let _,mtps,_,_ = AnalyzeTypeOfMemberVal g (typ,vref)
            mtps.Length
        | DefaultStructCtor _ -> 0

    member x.IsProtectedAccessiblity = 
        match x with 
        | ILMeth(g,x) -> x.IsProtectedAccessibility
        | FSMeth _ -> false
        | DefaultStructCtor _ -> false

    member x.IsVirtual =
        match x with 
        | ILMeth(g,x) -> x.IsVirtual
        | FSMeth(g,_,vref) -> MemberRefIsVirtual vref
        | DefaultStructCtor _ -> false

    member x.IsConstructor = 
        match x with 
        | ILMeth(g,x) -> x.IsConstructor
        | FSMeth(g,_,vref) ->
            let flags = (the (vref.MemberInfo)).MemberFlags
            (flags.MemberKind = MemberKindConstructor)
        | DefaultStructCtor _ -> true

    member x.IsClassConstructor =
        match x with 
        | ILMeth(g,x) -> x.IsClassConstructor
        | FSMeth _ -> false
        | DefaultStructCtor _ -> false

    // REVIEW: check this for consistency between IL and F# metadata
    member x.IsDispatchSlot = 
        match x with 
        | ILMeth(g,x) -> 
            x.IsVirtual
        | FSMeth(g,_,vref) as x -> 
            is_interface_typ g x.EnclosingType  || 
            (let membInfo = (the (vref.MemberInfo))
             membInfo.MemberFlags.MemberIsDispatchSlot)
        | DefaultStructCtor _ -> false

    member x.IsFinal = 
        not x.IsVirtual || 
        match x with 
        | ILMeth(g,x) -> x.IsFinal
        | FSMeth(g,_,vref) as x -> false
        | DefaultStructCtor _ -> true

    member x.IsAbstract = 
        match x with 
        | ILMeth(g,x) -> x.IsAbstract
        | FSMeth(g,_,vref) as x -> 
            is_interface_typ g x.EnclosingType  || 
            MemberRefIsAbstract vref
        | DefaultStructCtor _ -> false

    member x.TcGlobals = 
        match x with 
        | ILMeth(g,_) -> g
        | FSMeth(g,_,_) -> g
        | DefaultStructCtor (g,_) -> g

    member x.IsNewSlot = 
        is_interface_typ x.TcGlobals x.EnclosingType  || 
        (x.IsVirtual && 
          (match x with 
           | ILMeth(g,x) -> x.IsNewSlot
           | FSMeth(g,_,vref) -> MemberRefIsDispatchSlot vref
           | DefaultStructCtor _ -> false))

    member x.IsDefiniteFSharpOverride = 
        match x with 
        | ILMeth(g,x) -> false
        | FSMeth(g,_,vref) -> MemberRefIsDefiniteFSharpOverride vref
        | DefaultStructCtor _ -> false


    member x.IsExtensionMember = 
        match x with 
        | ILMeth(g,x) -> x.ExtensionMethodInfo.IsSome
        | FSMeth(g,_,vref) -> vref.IsExtensionMember
        | DefaultStructCtor _ -> false

    member x.IsFSharpEventProperty = 
        match x with 
        | FSMeth(g,_,vref)  -> vref.IsFSharpEventProperty(g)
        | _ -> false

/// Type-qualified static property accessors for properties commonly used as first-class values
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module MethInfo =
    let IsVirtual(m:MethInfo) = m.IsVirtual
    let IsNewSlot(m:MethInfo) = m.IsNewSlot
    let IsDefiniteFSharpOverride(m:MethInfo) = m.IsDefiniteFSharpOverride
    let LogicalName(m:MethInfo) = m.LogicalName


let minfo_is_nullary (minfo:MethInfo) = (minfo.NumArgs = [0])
let minfo_is_struct g (x:MethInfo) = x.EnclosingType|> is_struct_typ g



type PropInfo with 
    member x.PropertyName = 
        match x with 
        | ILProp(_,x) -> x.PropertyName
        | FSProp(_,typ,Some vref,_) 
        | FSProp(_,typ,_, Some vref) -> 
            PropertyNameOfMemberValRef vref
        | FSProp _ -> failwith "unreachable"

    member x.GetterMethod = 
        match x with
        | ILProp(g,x) -> ILMeth(g,x.GetterMethod)
        | FSProp(g,typ,Some vref,_) -> FSMeth(g,typ,vref) 
        | FSProp _ -> failwith "no getter method"

    member x.SetterMethod = 
        match x with
        | ILProp(g,x) -> ILMeth(g,x.SetterMethod)
        | FSProp(g,typ,_,Some vref) -> FSMeth(g,typ,vref)
        | FSProp _ -> failwith "no setter method"

    member x.HasGetter = 
        match x with
        | ILProp(_,x) -> x.HasGetter
        | FSProp(_,_,x,_) -> isSome x 

    member x.HasSetter = 
        match x with
        | ILProp(_,x) -> x.HasSetter
        | FSProp(_,_,_,x) -> isSome x 

    member x.EnclosingType = 
        match x with 
        | ILProp(_,x) -> x.ILTypeInfo.ToType
        | FSProp(_,typ,_,_) -> typ


    /// True if the getter (or, if absent, the setter) is a virtual method
    // REVIEW: for IL properties this is getter OR setter. For F# properties it is getter ELSE setter
    member x.IsVirtualProperty = 
        match x with 
        | ILProp(_,x) -> il_pinfo_is_virt x
        | FSProp(_,typ,Some vref,_) 
        | FSProp(_,typ,_, Some vref) -> MemberRefIsVirtual vref
        | FSProp _-> failwith "unreachable"

    
    // REVIEW: this doesn't accord precisely with the IsNewSlot definition for members
    member x.IsNewSlot = 
        match x with 
        | ILProp(_,x) -> il_pinfo_is_newslot x
        | FSProp(_,typ,Some vref,_) 
        | FSProp(_,typ,_, Some vref) -> MemberRefIsDispatchSlot vref
        | FSProp(_,_,None,None) -> failwith "unreachable"


    /// True if the getter (or, if absent, the setter) for the property is a dispatch slot
    // REVIEW: for IL properties this is getter OR setter. For F# properties it is getter ELSE setter
    member x.IsDispatchSlot = 
        match x with 
        | ILProp(_,x) -> il_pinfo_is_virt x
        | FSProp(g,typ,Some vref,_) 
        | FSProp(g,typ,_, Some vref) ->
            is_interface_typ g typ  || 
            (let membInfo = (the (vref.MemberInfo))
             membInfo.MemberFlags.MemberIsDispatchSlot)
        | FSProp _ -> failwith "unreachable"

    member x.IsAbstract = 
        match x with 
        | ILProp(_,x) -> il_pinfo_is_abstract x
        | FSProp(_,typ,Some vref,_) 
        | FSProp(_,typ,_, Some vref) -> MemberRefIsAbstract vref
        | FSProp _ -> failwith "unreachable"

    member x.IsStatic =
        match x with 
        | ILProp(_,x) -> x.IsStatic
        | FSProp(_,_,Some vref,_) 
        | FSProp(_,_,_, Some vref) -> not vref.IsInstanceMember
        | FSProp(_,_,None,None) -> failwith "unreachable"

    member x.IsDefiniteFSharpOverride = 
        match x with 
        | ILProp _ -> false
        | FSProp(_,_,Some vref,_) 
        | FSProp(_,_,_,Some vref) -> MemberRefIsDefiniteFSharpOverride vref
        | FSProp(_,_,None,None) -> failwith "unreachable"

    member x.IsIndexer = 
        match x with 
        | ILProp(_,ILPropInfo(tinfo,pdef)) -> pdef.propArgs <> []
        | FSProp(g,typ,Some vref,_)  ->
            // A getter has signature  { OptionalObjectType } -> Unit -> PropertyType 
            // A getter indexer has signature  { OptionalObjectType } -> TupledIndexerArguments -> PropertyType 
            let arginfos = ArgInfosOfMember g vref
            arginfos.Length = 1 && arginfos.Head.Length >= 1
        | FSProp(g,typ,_, Some vref) -> 
            // A setter has signature  { OptionalObjectType } -> PropertyType -> Void 
            // A setter indexer has signature  { OptionalObjectType } -> TupledIndexerArguments -> PropertyType -> Void 
            let arginfos = ArgInfosOfMember g vref
            arginfos.Length = 1 && arginfos.Head.Length >= 2
        | FSProp(_,typ,None,None) -> 
            failwith "unreachable"

    member x.IsFSharpEventProperty = 
        match x with 
        | FSProp(g,typ,Some vref,None)  -> vref.IsFSharpEventProperty(g)
        | _ -> false

    member x.XmlDoc = 
        match x with 
        | ILProp(_,x) -> emptyXmlDoc
        | FSProp(_,typ,Some vref,_) 
        | FSProp(_,typ,_, Some vref) -> vref.XmlDoc
        | FSProp(_,typ,None,None) -> failwith "unreachable"

    member x.IsValueType =
        match x with 
        | ILProp(g,_) -> x.EnclosingType |> is_struct_typ g 
        | FSProp(g,_,_,_) -> x.EnclosingType |> is_struct_typ g

    member x.ArbitraryValRef = 
        match x with 
        | ILProp(_,x) -> None
        | FSProp(_,typ,Some vref,_) 
        | FSProp(_,typ,_, Some vref) -> Some(vref)
        | FSProp(_,typ,None,None) -> failwith "unreachable"

     

/// Type-qualified static property accessors for properties commonly used as first-class values
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module PropInfo = 
    let HasGetter(m:PropInfo) = m.HasGetter
    let IsVirtualProperty(m:PropInfo) = m.IsVirtualProperty
    let IsDefiniteFSharpOverride(m:PropInfo) = m.IsDefiniteFSharpOverride
    let IsNewSlot(m:PropInfo) = m.IsNewSlot
    let HasSetter(m:PropInfo) = m.HasSetter
    let EnclosingType(m:PropInfo) = m.EnclosingType
    let PropertyName(m:PropInfo) = m.PropertyName

let ParamNamesAndTypesOfPropInfo amap m = function
    | ILProp (_,x) -> params_of_il_pinfo amap m x
    | FSProp (g,typ,Some vref,_) 
    | FSProp (g,typ,_,Some vref) -> 
        let ttps,mtps,retTy,tinst = AnalyzeTypeOfMemberVal amap.g (typ,vref)
        let inst = mk_typar_inst ttps tinst
        ArgInfosOfPropertyVal g (deref_val vref) |> List.map (ParamOfArgInfo >> InstParam inst)
    | FSProp _ -> failwith "param_typs_of_pinfo: unreachable"

let PropertyTypeOfPropInfo amap m = function
    | ILProp (_,x) -> vtyp_of_il_pinfo amap m x
    | FSProp (g,typ,Some vref,_) 
    | FSProp (g,typ,_,Some vref) -> 
        let ttps,mtps,retTy,tinst = AnalyzeTypeOfMemberVal amap.g (typ,vref)
        let inst = mk_typar_inst ttps tinst
        ReturnTypeOfPropertyVal g (deref_val vref)
        |> InstType inst
        
    | FSProp _ -> failwith "vtyp_typs_of_pinfo: unreachable"

let ParamTypesOfPropInfo amap m pinfo = List.map snd (ParamNamesAndTypesOfPropInfo amap m pinfo) 

/// Used to hide/filter members from super classes based on signature *)
let PropInfosEquivByNameAndPartialSig erasureFlag g amap m (pinfo:PropInfo) (pinfo2:PropInfo) = 
    pinfo.PropertyName = pinfo2.PropertyName &&
    let argtys = ParamTypesOfPropInfo amap m pinfo
    let argtys2 = ParamTypesOfPropInfo amap m pinfo2
    List.lengthsEqAndForall2 (type_equiv_aux erasureFlag g) argtys argtys2 
  
//-------------------------------------------------------------------------
// events
//------------------------------------------------------------------------- 

exception BadEventTransformation of range

/// Properties compatible with type IDelegateEvent and atributed with CLIEvent are special: we generate metadata and add/remove methods 
/// to make them into a .NET event, and mangle the name of a property.  
/// We don't handle static, indexer or abstract properties correctly. 
/// Note the name mangling doesn't affect the name of the get/set methods for the property 
/// and so doesn't affect how we compile F# accesses to the property. 
let TypConformsToIDelegateEvent g ty = 
   is_fslib_IDelegateEvent_ty g ty && is_delegate_typ g (dest_fslib_IDelegateEvent_ty g ty) 
   

/// Create an error object to raise should an event not have the shape expected by the .NET idiom described further below 
let nonStandardEventError nm m = 
    Error ("The event '"^nm^" has a non-standard type. If this event is declared in another .NET language, you may need to access this event using the explicit add_"^nm^" and remove_"^nm^" methods for the event. If this event is declared in F#, make the type of the event an instantiation of either 'IDelegateEvent<_>' or 'IEvent<_,_>'",m)

let FindDelegateTypeOfPropertyEvent g amap nm m ty =
    match SearchEntireHierarchyOfType (TypConformsToIDelegateEvent g) g amap m ty with
    | None -> error(nonStandardEventError nm m)
    | Some ty -> dest_fslib_IDelegateEvent_ty g ty

type EventInfo with
    member x.EventName = match x with ILEvent(_,e) -> e.Name | FSEvent (_,p,_,_) -> p.PropertyName

    member x.IsStatic = match x with ILEvent(_,e) -> e.IsStatic | FSEvent (_,p,_,_) -> p.IsStatic
    member x.GetDelegateType(amap,m) = 
        match x with 
        | ILEvent(_,e) -> 
            if isNone e.RawMetadata.eventType then error (nonStandardEventError x.EventName m);

            DelegateTypeOfILEventInfo amap m e
        | FSEvent(g,p,_,_) -> 
            FindDelegateTypeOfPropertyEvent g amap x.EventName m (PropertyTypeOfPropInfo amap m p) 
        
    member x.IsValueType = 
        match x with 
        | ILEvent(_,e) -> e.ILTypeInfo.IsValueType 
        | FSEvent (_,p,_,_) -> p.IsValueType
    member x.GetAddMethod(m) = 
        match x with 
        | ILEvent(g,e) -> ILMeth(g,e.AddMethod)
        | FSEvent(g,p,addValRef,_) -> FSMeth(g,p.EnclosingType,addValRef)
    member x.GetRemoveMethod(m) = 
        match x with 
        | ILEvent(g,e) -> ILMeth(g,e.RemoveMethod)
        | FSEvent(g,p,_,removeValRef) -> FSMeth(g,p.EnclosingType,removeValRef)
    




  
//-------------------------------------------------------------------------
// finfo
//------------------------------------------------------------------------- 

   
let FieldTypeOfILFieldInfo amap m (ILFieldInfo (tinfo,fdef)) =
    ImportTypeFromMetadata amap m tinfo.ILScopeRef tinfo.TypeInst [] fdef.fdType


type ParamData = ParamData of bool * bool * OptionalArgInfo * string option * typ

let ParamDatasOfMethInfo amap m minfo minst = 
    let paramInfos = 
        match minfo with 
        | ILMeth(g,ilminfo) -> 
            [ ilminfo.ParamInfos(amap,m,minst)  ]
        | FSMeth(g,typ,vref) -> 
            let ttps,mtps,_,tinst = AnalyzeTypeOfMemberVal g (typ,vref)
            let paramTypes = ParamsOfMember g vref
            let inst = (mk_fs_minfo_tinst ttps mtps tinst minst)
            paramTypes |> InstParams inst 
        | DefaultStructCtor _ -> 
            [[]]
    let paramAttribs = ParamAttribsOfMethInfo amap m minfo
    (paramAttribs,paramInfos) ||> List.map2 (List.map2 (fun (isParamArrayArg,isOutArg,optArgInfo) (nmOpt,pty)-> 
         ParamData(isParamArrayArg,isOutArg,optArgInfo,nmOpt,pty)))


//-------------------------------------------------------------------------
// Printing
//------------------------------------------------------------------------- 

let FormatMethArgToBuffer denv os (ParamData(isParamArrayArg,isOutArg,optArgInfo,nmOpt,pty)) =
    let isOptArg = optArgInfo <> NotOptional
    match nmOpt, isOptArg, try_dest_option_ty denv.g pty with 
    // Layout an optional argument 
    | Some(nm), true, Some(pty) -> 
        bprintf os "?%s: %a" nm (NicePrint.output_typ denv) pty
    // Layout an unnamed argument 
    | None, _,_ -> 
        bprintf os "%a" (NicePrint.output_typ denv) pty;
    // Layout a named argument 
    | Some nm,_,_ -> 
        bprintf os "%s: %a" nm (NicePrint.output_typ denv) pty


let FormatMethInfoToBuffer amap m denv os minfo =
    match minfo with 
    | DefaultStructCtor(g,typ) -> 
        bprintf os "%a()" (NicePrint.output_tcref denv) (tcref_of_stripped_typ g minfo.EnclosingType);
    | FSMeth(g,_,vref) -> 
        NicePrint.output_qualified_val_spec denv os (deref_val vref)
    | ILMeth(g,ilminfo) -> 
        // Prettify this baby
        let minfo,minst = 
            let (ILMethInfo(ILTypeInfo(tcref,tref,tinst,tdef),extInfo,mdef,filmtps)) = ilminfo
            let _,tys,_ = PrettyTypes.PrettifyTypesN g (tinst @ minfo.FormalMethodInst)
            let tinst,minst = List.chop tinst.Length tys
            let minfo = mk_il_minfo amap m (ILTypeInfo(tcref,tref,tinst,tdef)) extInfo mdef
            minfo,minst
        
        let retTy = FSharpReturnTyOfMeth amap m minfo minst
        bprintf os "%a" (NicePrint.output_tcref denv) (tcref_of_stripped_typ g minfo.EnclosingType);
        if minfo.LogicalName = ".ctor" then  
          bprintf os "("
        else
          bprintf os ".%a(" (NicePrint.output_typars denv minfo.LogicalName) minfo.FormalMethodTypars;
        let paramDatas = ParamDatasOfMethInfo amap m minfo minst
        paramDatas |> List.iter (List.iteri (fun i arg -> 
              if i > 0 then bprintf os ", "; 
              FormatMethArgToBuffer denv os arg))
        bprintf os ") : %a"  (NicePrint.output_typ denv) retTy

let string_of_minfo amap m denv d = bufs (fun buf -> FormatMethInfoToBuffer amap m denv buf d)
let string_of_param_data denv paramData = bufs (fun buf -> FormatMethArgToBuffer denv buf paramData)


(*-------------------------------------------------------------------------
!* Basic accessibility logic
 *------------------------------------------------------------------------- *)

/// What keys do we have to access other constructs? 
type AccessorDomain = 
    | AccessibleFrom of 
        CompilationPath list * (* we have the keys to access any members private to the given paths *)
        TyconRef option        (* we have the keys to access any protected members of the super types of 'TyconRef' *)
    | AccessibleFromEverywhere
    | AccessibleFromSomeFSharpCode // everything but .NET private/internal stuff
    | AccessibleFromSomewhere // everything

module AccessibilityLogic = 


    let private il_tyaccess_accessible access =
        access = TypeAccess_public || access = TypeAccess_nested MemAccess_public

    let private  IsAccessible ad taccess = 
        match ad with 
        | AccessibleFromEverywhere -> can_access_from_everywhere taccess
        | AccessibleFromSomeFSharpCode -> can_access_from_somewhere taccess
        | AccessibleFromSomewhere -> true
        | AccessibleFrom (cpaths,tcrefViewedFromOption) -> 
            (* REVIEW: protected access in F# code *)
            List.exists (can_access_from taccess) cpaths

    let private CheckILMemberAccess g amap m (ILTypeInfo(tcrefOfViewedItem,_,_,_)) ad access = 
        match ad with 
        | AccessibleFromEverywhere -> 
              access = MemAccess_public
        | AccessibleFromSomeFSharpCode -> 
             (access = MemAccess_public || 
              access = MemAccess_family  || 
              access = MemAccess_famorassem) 
        | AccessibleFrom (cpaths,tcrefViewedFromOption) ->
             let accessibleByFamily =
                  ((access = MemAccess_family  || 
                    access = MemAccess_famorassem) &&
                   match tcrefViewedFromOption with 
                   | None -> false
                   | Some tcrefViewedFrom ->
                      ExistsInEntireHierarchyOfType (fun typ -> is_stripped_tyapp_typ g typ && tcref_eq g (tcref_of_stripped_typ g typ) tcrefOfViewedItem) g amap m (generalize_tcref tcrefViewedFrom |> snd))     
             let accessibleByInternalsVisibleTo = 
                  (access = MemAccess_assembly && can_access_cpath_from_one_of cpaths tcrefOfViewedItem.CompilationPath)
             (access = MemAccess_public) || accessibleByFamily || accessibleByInternalsVisibleTo
        | AccessibleFromSomewhere -> 
             true

    let private tdef_accessible tdef =        
        il_tyaccess_accessible tdef.tdAccess 

    // is tcref visible through the AccessibleFrom(cpaths,_)? note: InternalsVisibleTo extends those cpaths.
    let private tcref_accessible_via_visible_to ad (tcrefOfViewedItem:TyconRef) =
        match ad with 
        | AccessibleFromEverywhere | AccessibleFromSomewhere | AccessibleFromSomeFSharpCode -> false
        | AccessibleFrom (cpaths,tcrefViewedFromOption) ->
            can_access_cpath_from_one_of cpaths tcrefOfViewedItem.CompilationPath

    let private il_tinfo_accessible ad (ILTypeInfo(tcrefOfViewedItem,_,tinst,tdef)) =       
        tdef_accessible tdef || tcref_accessible_via_visible_to ad tcrefOfViewedItem
                       
    let private il_mem_accessible g amap m ad tinfo access = 
        il_tinfo_accessible ad tinfo && CheckILMemberAccess g amap m tinfo ad access

    let IsEntityAccessible ad (tcref:TyconRef) = 
        if tcref.IsILTycon then 
            (tcref_accessible_via_visible_to ad tcref) ||  // either: visibleTo (e.g. InternalsVisibleTo)              
              (let scoref,enc,tdef = tcref.ILTyconInfo   // or:     accessible, along with all enclosing types
               List.forall tdef_accessible enc && 
               tdef_accessible tdef)
        else  
             tcref.Accessibility |> IsAccessible ad

    let CheckTyconAccessible m ad tcref =
        let res = IsEntityAccessible ad tcref
        if not res then  
            errorR(Error("The type '"^tcref.DisplayName^"' is not accessible from this code location",m))
        res

    let IsTyconReprAccessible ad tcref =
        IsEntityAccessible ad tcref &&
        IsAccessible ad tcref.TypeReprAccessibility
            
    let CheckTyconReprAccessible m ad tcref =
        CheckTyconAccessible m ad tcref &&
        (let res = IsAccessible ad tcref.TypeReprAccessibility
         if not res then 
            errorR (Error ("The union cases or fields of the type '"^tcref.DisplayName^"' are not accessible from this code location",m));
         res)
            
    let rec IsTypeAccessible g ad ty = 
        not (is_stripped_tyapp_typ g ty) ||
        let tcref,tinst = dest_stripped_tyapp_typ g ty
        IsEntityAccessible ad tcref && IsTypeInstAccessible g ad tinst

    and IsTypeInstAccessible g ad tinst = 
        match tinst with 
        | [] -> true 
        | _ -> List.forall (IsTypeAccessible g ad) tinst

    let IsILFieldInfoAccessible g amap m ad (ILFieldInfo (tinfo,fd)) =
        il_mem_accessible g amap m ad tinfo fd.fdAccess

    let IsILEventInfoAccessible g amap m ad (ILEventInfo (tinfo,edef)) =
        il_mem_accessible g amap m ad tinfo (edef_accessibility tinfo.RawMetadata edef)

    let IsILMethInfoAccessible g amap m ad (ILMethInfo (tinfo,_,mdef,_)) =
        il_mem_accessible g amap m ad tinfo mdef.mdAccess 

    let IsILPropInfoAccessible g amap m ad (ILPropInfo(tinfo,pdef)) =
        il_mem_accessible g amap m ad tinfo (pdef_accessibility tinfo.RawMetadata pdef)

    let IsValAccessible ad (vref:ValRef) = 
        vref.Accessibility |> IsAccessible ad

    let CheckValAccessible  m ad (vref:ValRef) = 
        if not (IsValAccessible ad vref) then 
            errorR (Error ("The value '"^vref.MangledName^"' is not accessible from this code location",m))
        
    let IsUnionCaseAccessible ad (ucref:UnionCaseRef) =
        IsTyconReprAccessible ad ucref.TyconRef &&
        IsAccessible ad ucref.UnionCase.Accessibility

    let CheckUnionCaseAccessible m ad (ucref:UnionCaseRef) =
        CheckTyconReprAccessible m ad ucref.TyconRef &&
        (let res = IsAccessible ad ucref.UnionCase.Accessibility
         if not res then 
            errorR (Error ("The union case '"^ucref.CaseName^"' is not accessible from this code location",m))
         res)

    let IsRecdFieldAccessible ad (rfref:RecdFieldRef) =
        IsTyconReprAccessible ad rfref.TyconRef &&
        IsAccessible ad rfref.RecdField.Accessibility

    let CheckRecdFieldAccessible m ad (rfref:RecdFieldRef) =
        CheckTyconReprAccessible m ad rfref.TyconRef &&
        (let res = IsAccessible ad rfref.RecdField.Accessibility
         if not res then 
            errorR (Error ("The record, struct or class field '"^rfref.FieldName^"' is not accessible from this code location",m))
         res)

    let CheckRecdFieldInfoAccessible m ad (rfinfo:RecdFieldInfo) = 
        CheckRecdFieldAccessible m ad rfinfo.RecdFieldRef |> ignore

    let CheckILFieldInfoAccessible g amap m ad finfo =
        if not (IsILFieldInfoAccessible g amap m ad finfo) then 
            errorR (Error (sprintf "The struct or class field '%s' is not accessible from this code location" finfo.FieldName,m))

    let IsMethInfoAccessible amap m ad = function 
        | ILMeth (g,x) -> IsILMethInfoAccessible g amap m ad x
        | FSMeth (_,_,vref) -> IsValAccessible ad vref
        | DefaultStructCtor(g,typ) -> IsTypeAccessible g ad typ

    let IsPropInfoAccessible g amap m ad = function 
        | ILProp (_,x) -> IsILPropInfoAccessible g amap m ad x
        | FSProp (_,_,Some vref,_) 
        | FSProp (_,_,_,Some vref) -> IsValAccessible ad vref
        | _ -> false

open AccessibilityLogic
(*-------------------------------------------------------------------------
!* Check custom attributes
 *------------------------------------------------------------------------- *)

exception Obsolete of string * range

module AttributeChecking = 


    let private bindMethInfoAttributes minfo f1 f2 = 
        match minfo with 
        | ILMeth (_,x) -> f1 x.RawMetadata.mdCustomAttrs 
        | FSMeth (_,_,vref) -> f2 vref.Attribs
        | DefaultStructCtor _ -> f2 []


    let private checkILAttributes g cattrs m = 
        let (AttribInfo(tref,_)) = g.attrib_SystemObsolete
        match ILThingDecodeILAttrib g tref cattrs with 
        | Some ([CustomElem_string (Some(msg)) ],_) -> 
             WarnD(Obsolete(msg,m))
        | Some ([CustomElem_string (Some(msg)); CustomElem_bool isError ],_) -> 
            (if isError then ErrorD else WarnD) (Obsolete(msg,m))
        | Some ([CustomElem_string None ],_) -> 
            WarnD(Obsolete("",m))
        | Some _ -> 
            WarnD(Obsolete("",m))
        | None -> 
            CompleteD

    let TryBindMethInfoAttribute g (AttribInfo(atref,_) as attribSpec) minfo f1 f2 = 
        bindMethInfoAttributes minfo 
            (fun ilAttribs -> ILThingDecodeILAttrib g atref ilAttribs |> Option.bind f1)
            (fun fsAttribs -> TryFindAttrib g attribSpec fsAttribs |> Option.bind f2)

      
    let CheckFSharpAttributes g attribs m = 
        if isNil attribs then CompleteD 
        else 
            (match TryFindAttrib g g.attrib_SystemObsolete attribs with
            | Some(Attrib(_,_,[ AttribStringArg(s) ],_,_)) ->
                WarnD(Obsolete(s,m))
            | Some(Attrib(_,_,[ AttribStringArg(s); AttribBoolArg(isError) ],_,_)) -> 
                (if isError then ErrorD else WarnD) (Obsolete(s,m))
            | Some _ -> 
                WarnD(Obsolete("", m))
            | None -> 
                CompleteD
            ) ++ (fun () -> 
            
            match TryFindAttrib g g.attrib_OCamlCompatibilityAttribute attribs with
            | Some(Attrib(_,_,[ AttribStringArg(s) ],_,_)) -> 
                WarnD(OCamlCompatibility(s,m))
            | Some _ -> 
                WarnD(OCamlCompatibility("", m))
            | None -> 
                CompleteD
            ) ++ (fun () -> 
            
            match TryFindAttrib g g.attrib_ExperimentalAttribute attribs with
            | Some(Attrib(_,_,[ AttribStringArg(s) ],_,_)) -> 
                WarnD(Experimental(s,m))
            | Some _ -> 
                WarnD(Experimental("This construct is experimental", m))
            | _ ->  
                CompleteD
            ) ++ (fun () -> 

            match TryFindAttrib g g.attrib_UnverifiableAttribute attribs with
            | Some _ -> 
                WarnD(PossibleUnverifiableCode(m))
            | _ ->  
                CompleteD
            )

    let CheckILAttribsForUnseen g cattrs m = 
        let (AttribInfo(tref,_)) = g.attrib_SystemObsolete
        isSome (ILThingDecodeILAttrib g tref cattrs)

    let CheckAttribsForUnseen g attribs m = 
        nonNil attribs && 
        (isSome (TryFindAttrib g g.attrib_SystemObsolete attribs) ||
         (not g.mlCompatibility && isSome (TryFindAttrib g g.attrib_OCamlCompatibilityAttribute attribs))
        )
        // REVIEW: consider filter out experimental and unverifiable depending on context
      
    let CheckPropInfoAttributes pinfo m = 
        match pinfo with
        | ILProp(g,ILPropInfo(tinfo,pdef)) -> checkILAttributes g pdef.propCustomAttrs m
        | FSProp(g,typ,Some vref,_) 
        | FSProp(g,typ,_,Some vref) -> CheckFSharpAttributes g vref.Attribs m
        | FSProp _ -> failwith "CheckPropInfoAttributes: unreachable"
      
    let CheckILFieldAttributes g (finfo:ILFieldInfo) m = 
        checkILAttributes g finfo.RawMetadata.fdCustomAttrs m |> CommitOperationResult

    let CheckMethInfoAttributes g m minfo = 
        match bindMethInfoAttributes minfo 
                  (fun ilAttribs -> Some(checkILAttributes g ilAttribs m)) 
                  (fun fsAttribs -> Some(CheckFSharpAttributes g fsAttribs m)) with
        | Some res -> res
        | None -> CompleteD (* no attribute = no errors *)

    let MethInfoIsUnseen g m minfo = 
        match bindMethInfoAttributes minfo 
                  (fun ilAttribs -> Some(CheckILAttribsForUnseen g ilAttribs m)) 
                  (fun fsAttribs -> Some(CheckAttribsForUnseen g fsAttribs m)) with
        | Some res -> res
        | None -> false

    let PropInfoIsUnseen m pinfo = 
        match pinfo with
        | ILProp (g,ILPropInfo(tinfo,pdef)) -> CheckILAttribsForUnseen g pdef.propCustomAttrs m
        | FSProp (g,typ,Some vref,_) 
        | FSProp (g,typ,_,Some vref) -> CheckAttribsForUnseen g vref.Attribs m
        | FSProp _ -> failwith "CheckPropInfoAttributes: unreachable"
     
    let CheckEntityAttributes g (x:TyconRef) m = 
        if x.IsILTycon then 
            let tdef = x.ILTyconRawMetadata
            checkILAttributes g tdef.tdCustomAttrs m
        else CheckFSharpAttributes g x.Attribs m

    let CheckUnionCaseAttributes g (x:UnionCaseRef) m =
        CheckEntityAttributes g x.TyconRef m ++ (fun () ->
        CheckFSharpAttributes g x.Attribs m)

    let CheckRecdFieldAttributes g (x:RecdFieldRef) m =
        CheckEntityAttributes g x.TyconRef m ++ (fun () ->
        CheckFSharpAttributes g x.PropertyAttribs m)

    let CheckValAttributes g (x:ValRef) m =
        CheckFSharpAttributes g x.Attribs m

    let CheckRecdFieldInfoAttributes g (x:RecdFieldInfo) m =
        CheckRecdFieldAttributes g x.RecdFieldRef m


open AttributeChecking
    
//-------------------------------------------------------------------------
// Build calls to F# methods
//------------------------------------------------------------------------- 

/// Consume the arguments in chunks and build applications.  This copes with various F# calling signatures
/// all of which ultimately become 'methods'.
/// QUERY: this looks overly complex considering that we are doing a fundamentally simple 
/// thing here. 
let BuildFSharpMethodApp g m vref vexp vexprty (args: expr list) =
    let arities =  (arity_of_val (deref_val vref)).AritiesOfArgs
    
    let args3,(leftover,retTy) = 
        List.mapfold 
            (fun (args,fty) arity -> 
                match arity,args with 
                | (0|1),[] when type_equiv g (domain_of_fun_typ g fty) g.unit_ty -> mk_unit g m, (args, range_of_fun_typ g fty)
                | 0,(arg::argst)-> 
                    warning(InternalError(sprintf "Unexpected zero arity, args = %s" (Layout.showL (Layout.sepListL (Layout.rightL ";") (List.map ExprL args))),m));
                    arg, (argst, range_of_fun_typ g fty)
                | 1,(arg :: argst) -> arg, (argst, range_of_fun_typ g fty)
                | 1,[] -> error(InternalError("expected additional arguments here",m))
                | _ -> 
                    if args.Length < arity then error(InternalError("internal error in getting arguments, n = "^string arity^", #args = "^string args.Length,m));
                    let tupargs,argst = List.chop arity args
                    let tuptys = tupargs |> List.map (type_of_expr g) 
                    (mk_tupled g m tupargs tuptys),
                    (argst, range_of_fun_typ g fty) )
            (args,vexprty)
            arities
    if not leftover.IsEmpty then error(InternalError("Unexpected "^string(leftover.Length)^" remaining arguments in method application",m));
    mk_appl g ((vexp,vexprty),[],args3,m), 
    retTy
    
let BuildFSharpMethodCall g m (typ,vref:ValRef) vFlags minst args =
    let vexp = TExpr_val (vref,vFlags,m)
    let vexpty = vref.Type
    let tpsorig,tau =  vref.TypeScheme
    let vtinst = tinst_of_stripped_typ g typ @ minst
    if tpsorig.Length <> vtinst.Length then error(InternalError("BuildFSharpMethodCall: unexpected List.length mismatch",m));
    let expr = mk_tyapp m (vexp,vexpty) vtinst
    let exprty = InstType (mk_typar_inst tpsorig vtinst) tau
    /// REVIEW: this is passing in the instantiated type.  Should this be the formal type? 
    BuildFSharpMethodApp g m vref expr exprty args
    
//-------------------------------------------------------------------------
// Sets of methods up the hierarchy, ignoring duplicates by name and sig.
// Used to collect sets of virtual methods, protected methods, protected
// properties etc. 
//  REVIEW: this code generalizes the iteration used below for member lookup.
//  REVIEW: this doesn't take into account newslot decls.
//------------------------------------------------------------------------- 

let MemberIsExplicitImpl g (membInfo:ValMemberInfo) = 
    membInfo.MemberFlags.MemberIsOverrideOrExplicitImpl &&
    match membInfo.ImplementedSlotSigs with 
    | [] -> false
    | slotsigs -> slotsigs |> List.forall (fun slotsig -> is_interface_typ g slotsig.ImplementedType )


let SelectFromMemberVals g f (tcref:TyconRef) = 
    let aug = tcref.TypeContents

    aug.tcaug_adhoc |> NameMultiMap.chooseRange (fun vref ->
        match vref.MemberInfo with 
        // The 'when' condition is a workaround for the fact that values providing 
        // override and interface implementations are published in inferred module types 
        // These cannot be selected directly via the "." notation. 
        // However, it certainly is useful to be able to publish these values, as we can in theory 
        // optimize code to make direct calls to these methods. 
        | Some membInfo when (not (MemberIsExplicitImpl g membInfo)) -> 
            f membInfo vref
        | _ ->  
            None) 

let checkFilter optFilter nm = match optFilter with None -> true | Some n2 -> nm = n2

let TrySelectMemberVal g optFilter typ membInfo vref =
    if checkFilter optFilter membInfo.CompiledName then 
        let tinst = tinst_of_stripped_typ g typ
        let ntinst = List.length tinst
        Some(FSMeth(g,typ,vref))
    else None

let GetImmediateIntrinsicMethInfosOfType (optFilter,ad) g amap m typ =
    let minfos =
        if is_il_named_typ g typ then 
            let tinfo = tinfo_of_il_typ g typ
            let mdefs = (match optFilter with None -> dest_mdefs | Some(nm) -> find_mdefs_by_name nm)  tinfo.RawMetadata.tdMethodDefs
            mdefs |> List.map (mk_il_minfo amap m  tinfo None) 
        elif not (is_stripped_tyapp_typ g typ) then []
        else SelectFromMemberVals g (TrySelectMemberVal g optFilter typ) (tcref_of_stripped_typ g typ)
    let minfos = minfos |> List.filter (IsMethInfoAccessible amap m ad)
    minfos

/// Join up getters and setters which are not associated in the F# data structure 
type PropertyCollector(g,amap,m,typ,optFilter,ad) = 

    let hashIdentity = 
        Microsoft.FSharp.Collections.HashIdentity.FromFunctions 
            (PropInfo.PropertyName >> hash) 
            (fun pinfo1 pinfo2 -> 
                PropInfosEquivByNameAndPartialSig EraseNone g amap m pinfo1 pinfo2 &&
                pinfo1.IsDefiniteFSharpOverride = pinfo2.IsDefiniteFSharpOverride )
    let props = new System.Collections.Generic.Dictionary<PropInfo,PropInfo>(hashIdentity)
    let add pinfo =
        if props.ContainsKey(pinfo) then 
            match props.[pinfo], pinfo with 
            | FSProp (_,typ,Some vref1,_), FSProp (_,_,_,Some vref2)
            | FSProp (_,typ,_,Some vref2), FSProp (_,_,Some vref1,_)  -> 
                let pinfo = FSProp (g,typ,Some vref1,Some vref2)
                props.[pinfo] <- pinfo 
            | _ -> 
                // This assert first while editing bad code. We will give a warning later in check.ml
                //assert ("unexpected case"= "")
                ()
        else
            props.[pinfo] <- pinfo

    member x.Collect(membInfo,vref) = 
        match membInfo.MemberFlags.MemberKind with 
        | MemberKindPropertyGet ->
            let nm = PropertyNameOfMemberValRef vref
            let pinfo = FSProp(g,typ,Some vref,None) 
            if checkFilter optFilter nm && IsPropInfoAccessible g amap m ad pinfo then
                add pinfo
        | MemberKindPropertySet ->
            let nm = PropertyNameOfMemberValRef vref
            let pinfo = FSProp(g,typ,None,Some vref)
            if checkFilter optFilter nm  && IsPropInfoAccessible g amap m ad pinfo then 
                add pinfo
        | _ -> 
            ()

    member x.Close() = [ for KeyValue(_,pinfo) in props -> pinfo ]



let GetImmediateIntrinsicPropInfosOfType (optFilter,ad) g amap m typ =
    let pinfos =
        if is_il_named_typ g typ then 
            let tinfo = tinfo_of_il_typ g typ
            let pdefs = (match optFilter with None -> dest_pdefs | Some(nm) -> find_pdefs nm)  tinfo.RawMetadata.tdProperties
            pdefs |> List.map (fun pd -> ILProp(g,ILPropInfo(tinfo,pd))) 
        elif not (is_stripped_tyapp_typ g typ) then []
        else
            let propCollector = new PropertyCollector(g,amap,m,typ,optFilter,ad)
            SelectFromMemberVals g 
                       (fun membInfo vref -> propCollector.Collect(membInfo,vref); None)
                       (tcref_of_stripped_typ g typ) |> ignore
            propCollector.Close()
         
    let pinfos = pinfos |> List.filter (IsPropInfoAccessible g amap m ad)
    pinfos




//---------------------------------------------------------------------------
// 
//------------------------------------------------------------------------- 

type HierarchyItem = 
    | MethodItem of MethInfo list list
    | PropertyItem of PropInfo list list
    | RecdFieldItem of RecdFieldInfo
    | ILEventItem of ILEventInfo list
    | ILFieldItem of ILFieldInfo list

/// An InfoReader is an object to help us read and cache infos. 
/// We create one of these for each file we typecheck. 
///
/// REVIEW: We could consider sharing one InfoReader across an entire compilation 
/// run or have one global one for each (g,amap) pair.
type InfoReader(g:TcGlobals, amap:Import.ImportMap) =

    let getImmediateIntrinsicILFieldsOfType (optFilter,ad) m typ =
        let infos =
            if is_il_named_typ g typ then 
                let tinfo = tinfo_of_il_typ g typ
                let fdefs = (match optFilter with None -> dest_fdefs | Some(nm) -> find_fdefs nm)  tinfo.RawMetadata.tdFieldDefs
                List.map (fun pd -> ILFieldInfo(tinfo,pd)) fdefs
            elif not (is_stripped_tyapp_typ g typ) then []
            else []
        let infos = infos |> List.filter (IsILFieldInfoAccessible g amap m  ad)
        infos           

    let getImmediateIntrinsicILEventsOfType (optFilter,ad) m typ =
        let infos =
            if is_il_named_typ g typ then 
                let tinfo = tinfo_of_il_typ g typ
                let edefs = (match optFilter with None -> dest_edefs | Some(nm) -> find_edefs nm)  tinfo.RawMetadata.tdEvents
                List.map (fun pd -> ILEventInfo(tinfo,pd)) edefs 
            elif not (is_stripped_tyapp_typ g typ) then []
            else []
        let infos = infos |> List.filter (IsILEventInfoAccessible g amap m ad)
        infos 

    let mk_rfinfo g typ tcref fspec = RecdFieldInfo(tinst_of_stripped_typ g typ,rfref_of_rfield tcref fspec)

    let getImmediateIntrinsicRecdFieldsOfType nm typ =
        match try_tcref_of_stripped_typ g typ with 
        | None -> None
        | Some tcref -> 
           (* Note;secret fields are not allowed in lookups here, as we're only looking *)
           (* up user-visible fields in name resolution. *)
           match tcref.GetFieldByName nm with
           | Some rfield when not rfield.IsCompilerGenerated -> Some (mk_rfinfo g typ tcref rfield)
           | _ -> None


    /// The primitive reader for the method info sets up a hierarchy
    let readRawIntrinsicMethodSetsUncached ((optFilter,ad),m,typ) =
        FoldPrimaryHierarchyOfType (fun typ acc -> GetImmediateIntrinsicMethInfosOfType (optFilter,ad) g amap m typ :: acc) g amap m typ []

    /// The primitive reader for the property info sets up a hierarchy
    let readRawIntrinsicPropertySetsUncached ((optFilter,ad),m,typ) =
        FoldPrimaryHierarchyOfType (fun typ acc -> GetImmediateIntrinsicPropInfosOfType (optFilter,ad) g amap m typ :: acc) g amap m typ []

    let readIlFieldInfosUncached ((optFilter,ad),m,typ) =
        FoldPrimaryHierarchyOfType (fun typ acc -> getImmediateIntrinsicILFieldsOfType (optFilter,ad) m typ @ acc) g amap m typ []

    let readIlEventInfosUncached ((optFilter,ad),m,typ) =
        FoldPrimaryHierarchyOfType (fun typ acc -> getImmediateIntrinsicILEventsOfType (optFilter,ad) m typ @ acc) g amap m typ []

    let findRecdFieldInfoUncached (nm,m,typ) =
        FoldPrimaryHierarchyOfType (fun typ acc -> match getImmediateIntrinsicRecdFieldsOfType nm typ with None -> acc | Some v -> Some v) g amap m typ None
    
    let readEntireTypeHierachyUncached ((),m,typ) =
        FoldEntireHierarchyOfType (fun typ acc -> typ :: acc) g amap m typ [] 

    let readPrimaryTypeHierachyUncached ((),m,typ) =
        FoldPrimaryHierarchyOfType (fun typ acc -> typ :: acc) g amap m typ [] 

    /// The primitive reader for the named items up a hierarchy
    let readRawIntrinsicNamedItemsUncached ((nm,ad),m,typ) =
        let optFilter = Some(nm)
        FoldPrimaryHierarchyOfType (fun typ acc -> 
             let minfos = GetImmediateIntrinsicMethInfosOfType (optFilter,ad) g amap m typ
             let pinfos = GetImmediateIntrinsicPropInfosOfType (optFilter,ad) g amap m typ 
             let finfos = getImmediateIntrinsicILFieldsOfType (optFilter,ad) m typ 
             let einfos = getImmediateIntrinsicILEventsOfType (optFilter,ad) m typ 
             let rfinfos = getImmediateIntrinsicRecdFieldsOfType nm typ 
             match acc with 
             | Some(MethodItem(inheritedMethSets)) when nonNil minfos -> Some(MethodItem (minfos::inheritedMethSets))
             | _ when nonNil minfos -> Some(MethodItem ([minfos]))
             | Some(PropertyItem(inheritedPropSets)) when nonNil pinfos -> Some(PropertyItem(pinfos::inheritedPropSets))
             | _ when nonNil pinfos -> Some(PropertyItem([pinfos]))
             | _ when nonNil finfos -> Some(ILFieldItem(finfos))
             | _ when nonNil einfos -> Some(ILEventItem(einfos))
             | _ when isSome rfinfos -> Some(RecdFieldItem(rfinfos.Value))
             | _ -> acc)
          g amap m 
          typ
          None

    let makeInfoCache g f = 
        new MemoizationTable<_,_>
             (compute=f,
              // Only cache closed, monomorphic types (closed = all members for the type
              // have been processed). Also don't cache anything involving an inference equations or 
              // type abbreviations. Generic type instantiations could be processed, but we have
              // to be very careful not to cache anything that depends on inference equations or
              // type abbreviations, and most instantiations involve some type variables anyway
              canMemoize=(fun (flags,(_:range),typ) -> 
                                    match typ with 
                                    | TType_app(tcref,[]) -> tcref.TypeContents.tcaug_closed 
                                    | _ -> false),
              
              keyEquals=(fun (flags1,_,typ1) (flags2,_,typ2) ->
                                    // Ignore the ranges!
                                    (flags1 = flags2) && 
                                    match typ1,typ2 with 
                                    | TType_app(tcref1,[]),TType_app(tcref2,[]) -> tcref_eq g tcref1 tcref2
                                    | _ -> false),
              keyHash= (fun (flags,_,typ) -> 
                                    hash flags + 
                                    (match typ with 
                                     | TType_app(tcref,[]) -> hash tcref.MangledName
                                     | _ -> 0)))

    
    let methodInfoCache = makeInfoCache g readRawIntrinsicMethodSetsUncached
    let propertyInfoCache = makeInfoCache g readRawIntrinsicPropertySetsUncached
    let ilFieldInfoCache = makeInfoCache g readIlFieldInfosUncached
    let ilEventInfoCache = makeInfoCache g readIlEventInfosUncached
    let recdFieldInfoCache = makeInfoCache g findRecdFieldInfoUncached
    let namedItemsCache = makeInfoCache g readRawIntrinsicNamedItemsUncached
    let entireTypeHierarchyCache = makeInfoCache g readEntireTypeHierachyUncached
    let primaryTypeHierarchyCache = makeInfoCache g readPrimaryTypeHierachyUncached
                                            
    member x.g = g
    member x.amap = amap
    
    /// Read the method infos for a type
    ///
    /// Cache the result for monomorphic types
    member x.GetRawIntrinsicMethodSetsOfType (optFilter,ad,m,typ) =
        methodInfoCache.Apply(((optFilter,ad),m,typ))

    member x.GetRawIntrinsicPropertySetsOfType (optFilter,ad,m,typ) =
        propertyInfoCache.Apply(((optFilter,ad),m,typ))

    member x.GetILFieldInfosOfType (optFilter,ad,m,typ) =
        ilFieldInfoCache.Apply(((optFilter,ad),m,typ))

    member x.GetILEventInfosOfType (optFilter,ad,m,typ) =
        ilEventInfoCache.Apply(((optFilter,ad),m,typ))

    member x.TryFindRecdFieldInfoOfType (nm,m,typ) =
        recdFieldInfoCache.Apply((nm,m,typ))

    member x.TryFindNamedItemOfType (nm,ad,m,typ) =
        namedItemsCache.Apply(((nm,ad),m,typ))

    member x.ReadEntireTypeHierachy (m,typ) =
        entireTypeHierarchyCache.Apply(((),m,typ))

    member x.ReadPrimaryTypeHierachy (m,typ) =
        primaryTypeHierarchyCache.Apply(((),m,typ))


(*-------------------------------------------------------------------------
!* Constructor infos
 *------------------------------------------------------------------------- *)


let private ConstructorInfosOfILType g amap m typ = 
    let tdef = tdef_of_il_typ g typ
    tdef.Methods 
    |> IL.find_mdefs_by_name ".ctor" 
    |> List.filter (fun md -> match md.mdKind with MethodKind_ctor -> true | _ -> false) 
    |> List.map (mk_il_minfo amap m (tinfo_of_il_typ g typ) None) 
    
let GetIntrinsicConstructorInfosOfType (infoReader:InfoReader) m ty = 
    let g = infoReader.g
    let amap = infoReader.amap 
    if verbose then   dprintf "--> GetIntrinsicConstructorInfosOfType\n"; 
    if is_stripped_tyapp_typ g ty then
        if is_il_named_typ g ty then 
            ConstructorInfosOfILType g amap m ty
        else 
            let tcref = tcref_of_stripped_typ g ty
            let nm = ".ctor"
            let aug = tcref.TypeContents
            (* tcaug_adhoc cleanup: this should select from all accessible/available vrefs *)
            (* that are part of any augmentation of this type. That's assuming that constructors can *)
            (* be in augmentations. *)
            let vrefs = NameMultiMap.find nm aug.tcaug_adhoc
            vrefs 
            |> List.choose(fun vref -> 
                match vref.MemberInfo with 
                | Some membInfo when (membInfo.MemberFlags.MemberKind = MemberKindConstructor) -> Some(vref) 
                | _ -> None) 
            |> List.map (fun x -> FSMeth(g,ty,x)) 
    else []
    

(*-------------------------------------------------------------------------
!* Method signatures
 *------------------------------------------------------------------------- *)

let FormalTyparsOfEnclosingTypeOfMethInfo m minfo = 
    match minfo with 
    | ILMeth(g,ilminfo) -> 
        // For extension methods all type variables are on the method
        if ilminfo.IsCSharpExtensionMethod then 
            [] 
        else
             ilminfo.ILTypeInfo |> FormalTyparsOfILTypeInfo m
    | FSMeth(g,typ,vref) -> 
        let ttps,_,_,_ = AnalyzeTypeOfMemberVal g (typ,vref)
        ttps
    | DefaultStructCtor(g,typ) -> 
        (tcref_of_stripped_typ g typ).Typars(m)

let CompiledSigOfMeth g amap m (minfo:MethInfo) = 
    let fmtps = minfo.FormalMethodTypars
    let fminst = generalize_typars fmtps
    let vargtys = ParamTypesOfMethInfo amap m minfo fminst
    let vrty = CompiledReturnTyOfMeth amap m minfo fminst

    // The formal method typars returned are completely formal - they don't take into account the instantiation 
    // of the enclosing type. For example, they may have constraints involving the _formal_ type parameters 
    // of the enclosing type. This instaniations can be used to interpret those type parameters 
    let fmtpinst = 
        let tinst = tinst_of_stripped_typ g minfo.EnclosingType
        let ttps  = FormalTyparsOfEnclosingTypeOfMethInfo m minfo
        mk_typar_inst ttps tinst
            
    vargtys,vrty,fmtps,fmtpinst

/// Used to hide/filter members from super classes based on signature 
let MethInfosEquivByNameAndPartialSig erasureFlag g amap m (minfo:MethInfo) (minfo2:MethInfo) = 
    (minfo.LogicalName = minfo2.LogicalName) &&
    (minfo.GenericArity = minfo2.GenericArity) &&
    let fmtps = minfo.FormalMethodTypars
    let fminst = generalize_typars fmtps
    let fmtps2 = minfo2.FormalMethodTypars
    let fminst2 = generalize_typars fmtps2
    let argtys = ParamTypesOfMethInfo amap m minfo fminst
    let argtys2 = ParamTypesOfMethInfo amap m minfo2 fminst2
    (argtys,argtys2) ||> List.lengthsEqAndForall2 (List.lengthsEqAndForall2 (type_aequiv_aux erasureFlag g (mk_tyeq_env fmtps fmtps2)))

/// Used to hide/filter members from super classes based on signature 
let MethInfosEquivByNameAndSig erasureFlag g amap m minfo minfo2 = 
    MethInfosEquivByNameAndPartialSig erasureFlag g amap m minfo minfo2 &&
    let argtys,retTy,fmtps,_ = CompiledSigOfMeth g amap m minfo
    let argtys2,rty2,fmtps2,_ = CompiledSigOfMeth g amap m minfo2
    match retTy,rty2 with 
    | None,None -> true
    | Some retTy,Some rty2 -> type_aequiv_aux erasureFlag g (mk_tyeq_env fmtps fmtps2) retTy rty2 
    | _ -> false

/// Used to hide/filter members from super classes based on signature *)
let PropInfosEquivByNameAndSig erasureFlag g amap m pinfo pinfo2 = 
    PropInfosEquivByNameAndPartialSig erasureFlag g amap m pinfo pinfo2 &&
    let retTy = PropertyTypeOfPropInfo amap m pinfo
    let rty2 = PropertyTypeOfPropInfo amap m pinfo2
    type_equiv_aux erasureFlag g retTy rty2
  
/// nb. Prefer items toward the top of the hierarchy if the items are virtual 
/// but not when resolving base calls. Also get overrides instead 
/// of abstract slots when measuring whether a class/interface implements all its 
/// required slots. 

type FindMemberFlag = 
  | IgnoreOverrides 
  | PreferOverrides

/// The input list is sorted from most-derived to least-derived type, so any System.Object methods 
/// are at the end of the list. Return a filtered list where prior/subsequent members matching by name and 
/// that are in the same equivalence class have been removed. We keep a name-indexed table to 
/// be more efficient when we check to see if we've already seen a particular named method. 
type IndexedList<'a>(itemLists: 'a list list, itemsByName: 'a NameMultiMap) = 
    member x.Items = itemLists
    member x.ItemsWithName(nm)  = NameMultiMap.find nm itemsByName
    member x.AddItems(items,nmf) = IndexedList<'a>(items::itemLists,List.foldBack (fun x acc -> NameMultiMap.add (nmf x) x acc) items itemsByName )
    
/// Add all the items to the IndexedList if better items are not already present. This is used to hide methods
/// in super classes and/or hide overrides of methods in subclasses.
///
/// Assume no items in 'items' are equivalent according to 'equiv'. This is valid because each step in a
/// .NET class hierarchy introduces a consistent set of methods, none of which hide each other within the 
/// given set. This is an important optimization because it means we don't have to List.filter for equivalence between the 
/// large overload sets introduced by methods like System.WriteLine.
///
/// Assume items can be given names by 'nmf', where two items with different names are
/// not equivalent.
let private addItemsToIndexedList noBetterThan nmf items (ilist:IndexedList<_>) = 
    // Have we already seen an item with the same name and that is in the same equivalence class?
    // If so, ignore this one. Note we can check against the original incoming 'ilist' because we are assuming that
    // none the elements of 'items' are equivalent. 
    let items = items |> List.filter (fun item -> not (List.exists (noBetterThan item) (ilist.ItemsWithName(nmf item))))
    ilist.AddItems(items,nmf)

let private emptyIndexedList() = IndexedList([],NameMultiMap.empty)

let private excludePriorItems noBetterThan nmf = 
    let rec loop itemLists = 
        match itemLists with
        | [] -> emptyIndexedList()
        | items :: rest -> addItemsToIndexedList noBetterThan nmf items (loop rest)
    fun itemLists -> (loop itemLists).Items

let private excludeSubsequentItems equiv nmf = 
    let rec loop itemLists (acc:IndexedList<_>) = 
        match itemLists with
        | [] -> List.rev acc.Items
        | items :: rest ->  loop rest (addItemsToIndexedList equiv nmf items acc)
    fun itemLists -> loop itemLists (emptyIndexedList())

let private filterOverrides findFlag (isvirt:'a->bool,isNewSlot,isDefiniteOverride,equivSigs,nmf:'a->string) items = 
    let equivVirts x y = isvirt x && isvirt y && equivSigs x y
    match findFlag with 
    | PreferOverrides -> 
        items
        // For each F#-declared override, get rid of any equivalent abstract member in the same type
        // This is because F# abstract members with default overrides give rise to two members with the
        // same logical signature in the same type, e.g.
        // type ClassType1() =
        //      abstract VirtualMethod1: string -> int
        //      default x.VirtualMethod1(s) = 3
        
        |> List.map (fun items -> 
            let definiteOverrides = items |> List.filter isDefiniteOverride 
            items |> List.filter (fun item -> (isDefiniteOverride item || not (List.exists (equivVirts item) definiteOverrides))))
       
        // get rid of any virtuals that are signature-equivalent to virtuals in supertypes
        |> excludeSubsequentItems equivVirts nmf 
    | IgnoreOverrides ->  
        (* A new virtual with the same signature is no better than the original unless it is new slot *)
        let noBetterThan item orig = not (isNewSlot item) &&  equivVirts item orig
        items
        // Get rid of any F#-declared overrides. THese may occur in the same type as the abstract member (unlike with .NET metadata)
        // Include any 'newslot' declared methods.
        |> List.map (List.filter (fun x -> not (isDefiniteOverride x))) 
        // get rid of any virtuals that are signature-equivalent to virtuals in subtypes
        |> excludePriorItems noBetterThan nmf 
    
let FilterOverridesOfMethInfos findFlag g amap m minfos = 
    filterOverrides findFlag (MethInfo.IsVirtual,MethInfo.IsNewSlot,MethInfo.IsDefiniteFSharpOverride,MethInfosEquivByNameAndSig EraseNone g amap m,MethInfo.LogicalName) minfos

let FilterOverridesOfPropInfos findFlag g amap m props = 
    filterOverrides findFlag (PropInfo.IsVirtualProperty,PropInfo.IsNewSlot,PropInfo.IsDefiniteFSharpOverride,PropInfosEquivByNameAndSig EraseNone g amap m, PropInfo.PropertyName) props

let ExcludeHiddenOfMethInfos g amap m (minfos:MethInfo list list) = 
    minfos
    |> excludeSubsequentItems 
        (fun m1 m2 -> 
             (* only hide those truly from super classes *)
             not (tcref_eq g (tcref_of_stripped_typ g m1.EnclosingType) (tcref_of_stripped_typ g m2.EnclosingType)) &&
             MethInfosEquivByNameAndPartialSig EraseNone g amap m m1 m2)
        MethInfo.LogicalName
    |> List.concat

let ExcludeHiddenOfPropInfos g amap m pinfos = 
    pinfos 
    |> excludeSubsequentItems (PropInfosEquivByNameAndPartialSig EraseNone g amap m) PropInfo.PropertyName
    |> List.concat

let GetIntrinsicMethInfoSetsOfType (infoReader:InfoReader) (optFilter,ad) findFlag m typ = 
    infoReader.GetRawIntrinsicMethodSetsOfType(optFilter,ad,m ,typ)
    |> FilterOverridesOfMethInfos findFlag infoReader.g infoReader.amap m
  
let GetIntrinsicPropInfoSetsOfType (infoReader:InfoReader) (optFilter,ad) findFlag m typ = 
    infoReader.GetRawIntrinsicPropertySetsOfType(optFilter,ad, m,typ) 
    |> FilterOverridesOfPropInfos findFlag infoReader.g infoReader.amap m

let GetIntrinsicMethInfosOfType infoReader (optFilter,ad)  findFlag m typ = 
    GetIntrinsicMethInfoSetsOfType infoReader (optFilter,ad)  findFlag m typ |> List.concat
  
let GetIntrinsicPropInfosOfType infoReader (optFilter,ad)  findFlag m typ = 
    GetIntrinsicPropInfoSetsOfType infoReader (optFilter,ad)  findFlag m typ  |> List.concat

let TryFindIntrinsicNamedItemOfType (infoReader:InfoReader) (nm,ad) findFlag m typ = 
    match infoReader.TryFindNamedItemOfType(nm,ad, m,typ) with
    | Some item -> 
        match item with 
        | PropertyItem psets -> Some(PropertyItem (psets |> FilterOverridesOfPropInfos findFlag infoReader.g infoReader.amap m))
        | MethodItem msets -> Some(MethodItem (msets |> FilterOverridesOfMethInfos findFlag infoReader.g infoReader.amap m))
        | _ -> Some(item)
    | None -> None

/// Try to detect the existence of a method on a type 
/// Used for 
///     -- getting the GetEnumerator, get_Current, MoveNext methods for enumerable types 
///     -- getting the Dispose method when resolving the 'use' construct 
///     -- getting the various methods used to desugar the computation expression syntax 
let TryFindMethInfo infoReader m ad nm ty = 
    GetIntrinsicMethInfosOfType infoReader (Some(nm),ad) IgnoreOverrides m ty 

let TryFindPropInfo infoReader m ad nm ty = 
    GetIntrinsicPropInfosOfType infoReader (Some(nm),ad) IgnoreOverrides m ty 


/// Make a call to a method info. Used by the optimizer only to build 
/// calls to the type-directed resolutions of overloaded operators 
let MakeMethInfoCall amap m minfo minst args =
    let vFlags = NormalValUse in (* correct unless if we allow wild trait constraints like "T has a ctor and can be used as a parent class" *)
    match minfo with 
    | ILMeth(g,ilminfo) -> 
        let direct = not minfo.IsVirtual
        let isProp = false in (* not necessarily correct, but this is only used post-creflect where this flag is irrelevant *)
        mk_il_minfo_call g amap m isProp ilminfo vFlags minst direct args |> fst
    | FSMeth(g,typ,vref) -> 
        BuildFSharpMethodCall g m (typ,vref) vFlags minst args |> fst
    | DefaultStructCtor(_,typ) -> 
       mk_ilzero (m,typ)

/// Given a delegate type work out the minfo, argument types, return type 
/// and F# function type by looking at the Invoke signature of the delegate. 
let GetSigOfFunctionForDelegate (infoReader:InfoReader) delty m ad =
    let g = infoReader.g
    let amap = infoReader.amap
    let minfo = 
        match GetIntrinsicMethInfosOfType infoReader (Some "Invoke",ad) IgnoreOverrides m delty with 
        | [h] -> h
        | [] -> error(Error("No Invoke methods found for delegate type",m))
        | h :: _ -> warning(InternalError("More than one Invoke method found for delegate type",m)); h
    
    let minst = []   // a delegate's Invoke method is never generic 
    let basicDelArgTys = ParamTypesOfMethInfo amap m minfo minst
    if basicDelArgTys.Length <> 1 then error(Error("Delegates are not allowed to have curried signatures",m))
    let basicDelArgTys = basicDelArgTys.Head
    let delArgTys = if isNil basicDelArgTys then [g.unit_ty] else basicDelArgTys
    let delRetTy = FSharpReturnTyOfMeth amap m minfo minst
        
    CheckMethInfoAttributes g m minfo |> CommitOperationResult;
    let fty = mk_iterated_fun_ty delArgTys delRetTy
    minfo,basicDelArgTys,delRetTy,fty

let TryDestStandardDelegateTyp (infoReader:InfoReader) m ad delTy =
    let g = infoReader.g
    let amap = infoReader.amap
    let minfo,delArgTys,delRetTy,_ = GetSigOfFunctionForDelegate infoReader delTy m ad
    match delArgTys with 
    | senderTy :: argTys when (is_obj_typ g senderTy)  -> Some(mk_tupled_ty g argTys,delRetTy)
    | _ -> None


(* We take advantage of the following idiom to simplify away the bogus "object" parameter of the 
   of the "Add" methods associated with events.  If you want to access it you
   can use AddHandler instead.
   
   The .NET Framework guidelines indicate that the delegate type used for
   an event should take two parameters, an "object source" parameter
   indicating the source of the event, and an "e" parameter that
   encapsulates any additional information about the event. The type of
   the "e" parameter should derive from the EventArgs class. For events
   that do not use any additional information, the .NET Framework has
   already defined an appropriate delegate type: EventHandler.
   (from http://msdn.microsoft.com/library/default.asp?url=/library/en-us/csref/html/vcwlkEventsTutorial.asp) 
 *)
let IsStandardEventInfo (infoReader:InfoReader) m ad (einfo:EventInfo) =
    let dty = einfo.GetDelegateType(infoReader.amap,m)
    match TryDestStandardDelegateTyp infoReader m ad dty with
    | Some _ -> true
    | None -> false

/// Get the (perhaps tupled) argument type accepted by an event 
let ArgsTypOfEventInfo (infoReader:InfoReader) m ad (einfo:EventInfo)  =
    let g = infoReader.g
    let amap = infoReader.amap
    let dty = einfo.GetDelegateType(amap,m)
    match TryDestStandardDelegateTyp infoReader m ad dty with
    | Some(argtys,_) -> argtys
    | None -> error(nonStandardEventError einfo.EventName m)

/// Get the type of the event when looked at as if it is a property 
/// Used when displaying the property in Intellisense 
let PropTypOfEventInfo (infoReader:InfoReader) m ad (einfo:EventInfo) =  
    let g = infoReader.g
    let amap = infoReader.amap
    let delTy = einfo.GetDelegateType(amap,m)
    let argsTy = ArgsTypOfEventInfo infoReader m ad einfo 
    mk_fslib_IEvent2_ty g delTy argsTy
