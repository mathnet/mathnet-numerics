// (c) Microsoft Corporation. All rights reserved

#light

//--------------------------------------------------------------------------
// The ILX generator. 
//
// NOTE: unit have NULL storage (no point storing units).
//-------------------------------------------------------------------------- 

module (* internal *) Microsoft.FSharp.Compiler.Ilxgen

open System.IO
open Internal.Utilities
open Internal.Utilities.Pervasives
open Microsoft.FSharp.Compiler.AbstractIL 
open Microsoft.FSharp.Compiler.AbstractIL.IL 
open Microsoft.FSharp.Compiler.AbstractIL.Internal 
open Microsoft.FSharp.Compiler.AbstractIL.Internal.Library
open Microsoft.FSharp.Compiler.AbstractIL.Extensions.ILX
open Microsoft.FSharp.Compiler.AbstractIL.Extensions.ILX.Types
open Microsoft.FSharp.Compiler.AbstractIL.Diagnostics 

open Microsoft.FSharp.Compiler 
open Microsoft.FSharp.Compiler.Tast
open Microsoft.FSharp.Compiler.Tastops
open Microsoft.FSharp.Compiler.Range
open Microsoft.FSharp.Compiler.Ast
open Microsoft.FSharp.Compiler.ErrorLogger
open Microsoft.FSharp.Compiler.PrettyNaming
open Microsoft.FSharp.Compiler.Env
open Microsoft.FSharp.Compiler.Layout
open Microsoft.FSharp.Compiler.Lib
open Microsoft.FSharp.Compiler.Typrelns

module Ilx = Microsoft.FSharp.Compiler.AbstractIL.Extensions.ILX.Types 


let verbose = false // (System.Environment.GetEnvironmentVariable("VERBOSE_ILXGEN") <> null)
let generatePublicAsInternal = ref false // flag can be set, see fscopts.ml
  
let IsNonErasedTypar (tp:Typar) = not tp.IsErased
let DropErasedTypars (tps:Typar list) = tps |> List.filter IsNonErasedTypar
let DropErasedTyargs tys = tys |> List.filter (fun ty -> match ty with TType_measure _ -> false | _ -> true) 
let AddSpecialNameFlag (mdef:ILMethodDef) = { mdef with mdSpecialName = true }

let AddNonUserCompilerGeneratedAttribs g (mdef:ILMethodDef) = add_mdef_generated_attrs  g.ilg mdef

let debugDisplayMethodName = "__DebugDisplay"

//--------------------------------------------------------------------------
// misc
//-------------------------------------------------------------------------- 

let i_pop = I_arith AI_pop  
let i_nop = I_arith AI_nop  
let i_dup = I_arith AI_dup
let i_ldnull = I_arith AI_ldnull
let i_ldc_i32_0 = I_arith (AI_ldc (DT_I4,NUM_I4 0))
let mk_ldc_i64 i = I_arith (AI_ldc (DT_I8,NUM_I8 i))
let mk_ldc_double i = I_arith (AI_ldc (DT_R8,NUM_R8 i))
let mk_ldc_single i = I_arith (AI_ldc (DT_R4,NUM_R4 i))

/// Make a method that simply loads a field
let mk_ldfld_method_def (methnm,reprAccess,stat,ilty,fldName,propType) =
   let il_fspec = mk_fspec_in_typ(ilty,fldName,propType)
   let ret = mk_return propType
   let mdef = 
       if stat then 
           mk_static_nongeneric_mdef (methnm,reprAccess,[],ret,MethodBody_il(mk_ilmbody(true,[],2,nonbranching_instrs_to_code([mk_normal_ldsfld il_fspec]),None)))
       else 
           mk_instance_mdef (methnm,reprAccess,[],ret,MethodBody_il(mk_ilmbody (true,[],2,nonbranching_instrs_to_code ([ ldarg_0; mk_normal_ldfld il_fspec]),None)))
   mdef |> AddSpecialNameFlag

let ChooseParamNames fieldNamesAndTypes = 
    let takenFieldNames = fieldNamesAndTypes |> List.map p23 |> Set.of_list

    fieldNamesAndTypes
    |> List.map (fun (propName,fldName,propType) -> 
        let lowerPropName = String.uncapitalize propName 
        let paramName = if takenFieldNames.Contains(lowerPropName) then propName else lowerPropName 
        paramName,fldName,propType)

let markup s = s |> Seq.mapi (fun i x -> i,x) 

// See prim-types.fs
let SourceLevelConstruct_SumType     = 1
let SourceLevelConstruct_RecordType  = 2
let SourceLevelConstruct_ObjectType  = 3
let SourceLevelConstruct_Field       = 4
let SourceLevelConstruct_Exception   = 5
let SourceLevelConstruct_Closure     = 6
let SourceLevelConstruct_Module      = 7
let SourceLevelConstruct_Alternative = 8
let SourceLevelConstruct_Value       = 9
let SourceLevelConstruct_PrivateRepresentation = 32

// Approximation for purposes of optimization and giving a warning when compiling definition-only files as EXEs 
let rec CheckCodeDoesSomething code = 
    match code with 
    | ILBasicBlock bb -> Array.fold (fun x i -> x || match i with I_arith (AI_ldnull | AI_nop | AI_pop) | I_ret |  I_seqpoint _ -> false | _ -> true) false bb.bblockInstrs
    | GroupBlock (_,codes) -> List.exists CheckCodeDoesSomething codes
    | RestrictBlock (_,code) -> CheckCodeDoesSomething code
    | TryBlock (code,seh) -> true 

let ChooseFreeVarNames takenNames ts =
    let tns = List.map (fun t -> (t,None)) ts
    let rec chooseName names (t,nOpt) = 
        let tn = match nOpt with None -> t | Some n -> t^string n
        if Zset.mem tn names then
          chooseName names (t,Some(match nOpt with None ->  0 | Some n -> (n+1)))
        else
          let names = Zset.add tn names
          names,tn
    let string_order = (compare : string -> string -> int)
    let names    = Zset.empty string_order |> Zset.addList takenNames
    let names,ts = List.fmap chooseName names tns
    ts

let ilxgenGlobalNng = NiceNameGenerator ()

(* cannot tailcall to methods taking byrefs *)
let is_byref  = function Type_byref _ -> true | _ -> false

let mainMethName = CompilerGeneratedName "main"

type AttributeDecoder(namedArgs) = 
    let nameMap = NameMap.of_list (List.map (fun (AttribNamedArg(s,a,b,c)) -> s,c) namedArgs)
    let findConst x = match NameMap.tryfind x nameMap with | Some(AttribExpr(_,TExpr_const(c,_,_))) -> Some c | _ -> None
    let findAppTr x = match NameMap.tryfind x nameMap with | Some(AttribExpr(_,TExpr_app(_,_,[TType_app(tr,ti)],_,_))) -> Some tr | _ -> None

    member self.FindInt16  x dflt = match findConst x with | Some(TConst_int16 x) -> x | _ -> dflt
    member self.FindInt32  x dflt = match findConst x with | Some(TConst_int32 x) -> x | _ -> dflt
    member self.FindBool   x dflt = match findConst x with | Some(TConst_bool x) -> x | _ -> dflt
    member self.FindString x dflt = match findConst x with | Some(TConst_string x) -> x | _ -> dflt
    member self.FindTypeName   x dflt = match findAppTr x with | Some(tr) -> tr.DisplayName | _ -> dflt

//--------------------------------------------------------------------------
// Statistics
//-------------------------------------------------------------------------- 

let report_ref = ref (fun oc -> ()) 
let add_report f = let old = report_ref.contents in report_ref := (fun oc -> old oc; f oc) 
let report (oc:TextWriter) = report_ref.contents oc

let NewCounter(nm) = 
    let count = ref 0
    add_report (fun oc -> if !count <> 0 then output_string oc (string !count ^ " "^nm^"\n"));
    (fun () -> incr count)

let CountClosure = NewCounter "closures"
let CountMethodDef = NewCounter "IL method defintitions corresponding to values"
let CountStaticFieldDef = NewCounter "IL field defintitions corresponding to values"
let CountCallFuncInstructions = NewCounter "callfunc instructions (indirect calls)"

//-------------------------------------------------------------------------
// Part of the last-minute tranformation performed by this file
// is to eliminate variables of static type "unit".  These are
// utility functions related to this.
//------------------------------------------------------------------------- 

let BindUnitVars g (mvs:Val list,paramInfos,body) = 
    match mvs,paramInfos with 
    | [v],[] -> 
        assert is_unit_typ g v.Type
        [], mk_let NoSequencePointAtInvisibleBinding v.Range v (mk_unit g v.Range) body 
    | _ -> mvs,body

//--------------------------------------------------------------------------
// Compilation environment for compiling a whole a module
//-------------------------------------------------------------------------- 

[<StructuralEquality(false); StructuralComparison(false)>]
type cenv = 
    { g: Env.TcGlobals;
      viewCcu: ccu;
      fragName: string;
      generateFilterBlocks: bool;
      workAroundReflectionEmitBugs: bool;
      emitConstantArraysUsingStaticDataBlobs:bool;
      amap: Import.ImportMap;
      (* mainMethodInfo: if this is set, then the last module becomes the "main" module and its toplevel bindings are executed at startup *)
      mainMethodInfo: Tast.Attribs option; 
      localOptimizationsAreOn: bool;
      debug: bool;
      emptyProgramOk : bool; }


type EmitSequencePointState = SPAlways | SPSuppress
//--------------------------------------------------------------------------
// scope, cloc, visibility
// Referencing other stuff, and descriptions of where items are to be placed
// within the generated IL namespace/typespace.  A bit of a mess.
//-------------------------------------------------------------------------- 
      
type cloc = 
    (* cloc = compilation location = path to a ccu, namespace or class *)
    { clocScope: IL.ILScopeRef; 
      clocTopImplQualifiedName: string;
      clocNamespace: string option;  
      clocEncl: string list;
      clocQualifiedNameOfFile : string }

//--------------------------------------------------------------------------
// Access this and other assemblies
//-------------------------------------------------------------------------- 

let mk_il_name pos n = match pos with [] -> n | _ -> String.concat "." pos^"."^n
let mk_private_name n = (CompilerGeneratedName n) 

let scoref_for_cloc cloc = cloc.clocScope

let CompLocForFragment fragName (ccu:ccu) = 
   { clocQualifiedNameOfFile =fragName;
     clocTopImplQualifiedName= fragName; 
     clocScope=ccu.ILScopeRef; 
     clocNamespace=None; 
     clocEncl=[]} 

let CompLocForCcu (ccu:ccu) =  CompLocForFragment ccu.AssemblyName ccu

let mk_topname ns n = String.concat "." (match ns with Some x -> [x;n] | None -> [n])

let CompLocForSubModuleOrNamespace cloc (submod:ModuleOrNamespace) =
    let n = submod.MangledName
    match submod.ModuleOrNamespaceType.ModuleOrNamespaceKind with 
    | FSharpModuleWithSuffix | FSharpModule -> { cloc with clocEncl= cloc.clocEncl @ [n]}
    | Namespace -> {cloc with clocNamespace=Some (mk_topname cloc.clocNamespace n)}

let CompLocForFixedPath fragName qname (CompPath(sref,cpath)) = 
    let ns,t = List.takeUntil (fun (_,mkind) -> mkind <> Namespace) cpath
    let ns = List.map fst ns
    let ns = text_of_path ns
    let encl = List.map (fun (s ,mkind)-> s) t
    if verbose then dprintn ("CompLocForFixedPath, ns = '"^ns^"', encl = '"^text_of_path encl^"'");
    let ns = if ns = "" then None else Some ns
    { clocQualifiedNameOfFile =fragName;
      clocTopImplQualifiedName=qname;
      clocScope=sref;
      clocNamespace=ns; 
      clocEncl=encl }

let CompLocForFixedModule fragName qname (mspec:ModuleOrNamespace) = 
   let cloc = CompLocForFixedPath fragName qname mspec.CompilationPath
   let cloc = CompLocForSubModuleOrNamespace cloc mspec
   cloc 

let NestedTypeRefForCompLoc cloc n = 
    match cloc.clocEncl with 
    | [] ->
        let tyname = mk_topname cloc.clocNamespace n
        mk_tref(scoref_for_cloc cloc,tyname)
    | h::t -> mk_nested_tref(scoref_for_cloc cloc,mk_topname cloc.clocNamespace h :: t,n)
        
let NestedTypeSpecForCompLoc cloc n tinst = 
    mk_tspec (NestedTypeRefForCompLoc cloc n,tinst)

let TypeNameForStatupCode cloc = "<StartupCode$"^(cloc.clocQualifiedNameOfFile.Replace(".","-"))^">.$"^cloc.clocTopImplQualifiedName 
let TypeNameForPrivateImplementationDetails cloc = "<PrivateImplementationDetails$"^(cloc.clocQualifiedNameOfFile.Replace(".","-"))^">"

let CompLocForStartupCode cloc = 
    {cloc with clocEncl=[TypeNameForStatupCode cloc];clocNamespace=None}

let CompLocForPrivateImplementationDetails cloc = 
    {cloc with 
        clocEncl=[TypeNameForPrivateImplementationDetails cloc];clocNamespace=None}

let rec TypeRefForCompLoc cloc  =
    match cloc.clocEncl with
    | [] ->  
      mk_tref(scoref_for_cloc cloc,TypeNameForPrivateImplementationDetails cloc)
    | [h] -> 
      let tyname = mk_topname cloc.clocNamespace h
      mk_tref(scoref_for_cloc cloc,tyname)
    | _ ->  
      let encl,n = List.frontAndBack cloc.clocEncl
      NestedTypeRefForCompLoc {cloc with clocEncl=encl} n 

let TypeSpecForCompLoc cloc = 
    mk_nongeneric_tspec (TypeRefForCompLoc cloc)

// Under --publicasinternal change Public to Internal,
// except when mayChange=false, e.g. for override implementations (no accessibility restriction permitted).
let ComputePublicMemberAccess  mayChange = if !generatePublicAsInternal && mayChange then MemAccess_assembly else MemAccess_public
let ComputeMemberAccess mayChange hidden = if hidden then MemAccess_assembly else ComputePublicMemberAccess mayChange

// Under --publicasinternal change types from Public to Private (internal for types)
let ComputePublicTypeAccess() = if !generatePublicAsInternal then TypeAccess_private else TypeAccess_public
let ComputeTypeAccess (tref:ILTypeRef) hidden = 
    match tref.Enclosing with 
    | [] -> if hidden then TypeAccess_private else ComputePublicTypeAccess() 
    | _ -> TypeAccess_nested (ComputeMemberAccess true hidden)


//--------------------------------------------------------------------------
// Representation of type constructors etc.
//
// How are module kinds, type parameters, local type constructors 
// etc. are mapped to IL types and IL type variables 
//-------------------------------------------------------------------------- 

[<StructuralEquality(false); StructuralComparison(false)>]
type TypeReprEnv = 
    { typar_reprs: (Typar * uint16 (* static_item_repr *) ) list;
      typar_count: int; (* How many type variables are in scope? *)
      tyenv_nativeptr_as_nativeint: bool (* Do we compile the "nativeptr<'a>" type as a machine integer, e.g. in closures? *) }

//--------------------------------------------------------------------------
// Lookup tyenv
//-------------------------------------------------------------------------- 

let repr_of_typar m tp tyenv = 
    try ListAssoc.find typar_ref_eq tp tyenv.typar_reprs
    with Not_found -> 
      errorR(InternalError("Undefined or unsolved type variable: "^showL(TyparL tp),m)); 
      uint16 666 (* random value for post-hoc diagnostic analysis on generated tree *)  

//--------------------------------------------------------------------------
// Type parameters and the environment
//-------------------------------------------------------------------------- 

let add_typar_as tyenv tp y =  {tyenv with typar_reprs=(tp,y) :: tyenv.typar_reprs }
let add_typar tyenv (tp:Typar) = 
  if IsNonErasedTypar tp then { (add_typar_as tyenv tp (uint16 tyenv.typar_count)) with typar_count= tyenv.typar_count + 1 } else tyenv

let add_typars tyenv tps = List.fold add_typar tyenv tps
let empty_tyenv =  { typar_count=0; 
                     typar_reprs=[]; 
                     tyenv_nativeptr_as_nativeint=false} 
let tyenv_for_typars tps = add_typars empty_tyenv tps

let tyenv_for_tycon (tycon:Tycon) = tyenv_for_typars (tycon.TyparsNoRange)
let tyenv_for_tcref tcref = tyenv_for_tycon (deref_tycon tcref) 

//--------------------------------------------------------------------------
// Generate type references
//-------------------------------------------------------------------------- 

let GenTcref (tcref:TyconRef) = 
    assert(not tcref.IsTypeAbbrev);
    tcref.CompiledRepresentation

type VoidNotOK = VoidNotOK | VoidOK
#if DEBUG 
let voidCheck m g permits ty = 
   if permits=VoidNotOK && is_void_typ g ty then 
       error(InternalError("System.Void unexpectedly detected in IL code generation. This should not occur.",m))
#endif

// When generating parameter and return types generate precise .NET IL pointer types 
// These can't be generated for generic instantiations, since .NET generics doesn't 
// permit this. But for 'naked' values (locals, parameters, return values etc.) machine 
// integer values and native pointer values are compatible (though the code is unverifiable). 
type PtrsOK = 
    | PtrTypesOK 
    | PtrTypesNotOK

// An F# multi dimension array type "int32[,]" should normally map to the ILDASM type "int32[0...,0...]", just like C#.
//
// However, System.Reflection.Emit has a nasty bug that means it can't emit calls to C# generic methods involving multi-dimensional arrays
//    void M<T>(int32[,])
// because MakeGenericMethod on this method returns a handle that causes an invalid call to be emitted by the IL code generator for dynamic assemblies
// 
// We have to pay a price here, either:
//    -- always emit no bounds, i.e. the ILDASM type "int32[,]" (without lower bounds), and not be able to implement C# virtual slots involving multi-dimensional array types
//    -- always emit bounds, i.e. the ILDASM type "int32[0...,0...]" (without lower bounds), and not be able to call C# or F# generic code such as the Array2 module
//    -- emit no bounds within the signatures of F# generic methods
// We follow the last one
type MultiDimArrayEmitFlag = 
    | EmitMultiDimArrayTypesWithoutBounds 
    | EmitMultiDimArrayTypesNormally

let rec GenTyargAux m g tyenv multiDimFlag tyarg =  GenTypeAux m g tyenv VoidNotOK PtrTypesNotOK multiDimFlag tyarg
and GenTyargsAux m g tyenv multiDimFlag tyargs = List.map (GenTyargAux m g tyenv multiDimFlag) (DropErasedTyargs tyargs)

and GenTyAppAux m g tyenv multiDimFlag repr tinst =  
    let ilTypeInst = GenTyargsAux m g tyenv multiDimFlag tinst
    match repr with  
    | TyrepOpen ty -> 
        let ty = IL.inst_typ ilTypeInst ty
        match multiDimFlag,ty with 
        | EmitMultiDimArrayTypesWithoutBounds, Type_array(ILArrayShape(arrayBounds),elemTy) when arrayBounds.Length > 1 -> 
             Type_array(ILArrayShape(arrayBounds |> List.map (fun _ -> (None,None))), elemTy)
        | _ -> ty
    | TyrepNamed (tref,boxity) -> IL.mk_typ boxity (mk_tspec (tref,ilTypeInst))

and GenNamedTyAppAux m g tyenv ptrsOK multiDimFlag tcref tinst = 
    let tinst = DropErasedTyargs tinst in
    (* See above note on ptrsOK *)
    if ptrsOK = PtrTypesOK && tcref_eq g tcref g.nativeptr_tcr then 
      GenNamedTyAppAux m g tyenv ptrsOK multiDimFlag g.ilsigptr_tcr tinst
    else
      GenTyAppAux m g tyenv multiDimFlag (GenTcref tcref) tinst

and GenTypeAux m g tyenv voidOK ptrsOK multiDimFlag  ty =
#if DEBUG 
    voidCheck m g voidOK ty;
#endif
    (* if verbose then dprintf "generating type '%s'\n" ((DebugPrint.showType ty)); *)
    match strip_tpeqns_and_tcabbrevs_and_measureable g ty with 
    | TType_app(tcref,tinst) -> GenNamedTyAppAux m g tyenv ptrsOK multiDimFlag  tcref tinst
    | TType_tuple(args) -> GenTypeAux m g tyenv VoidNotOK ptrsOK multiDimFlag  (compiled_tuple_ty g args)
    | TType_fun(dty,returnTy) -> Pubclo.typ_Func1 g.ilxPubCloEnv  (GenTyargAux m g tyenv multiDimFlag dty) (GenTyargAux m g tyenv multiDimFlag returnTy)

    | TType_ucase(ucref,args) -> 
        let cuspec,idx = GenUnionCaseSpec m g tyenv ucref args 
        EraseIlxClassunions.typ_of_alt cuspec idx

    | TType_forall(tps,tau) -> 
        let tps = DropErasedTypars tps 
        let tyenv = (add_typars tyenv tps) 
        List.foldBack (GenGenericParam m g tyenv >> Pubclo.typ_TyFunc g.ilxPubCloEnv) tps (GenTypeAux m g tyenv VoidNotOK ptrsOK multiDimFlag  tau)
    | TType_var(tp) -> Type_tyvar (repr_of_typar m tp tyenv)
    | TType_measure u -> g.ilg.typ_int32
    | _ -> failwith "GenTypeAux m: unexpected naked Unknown/Struct/Named type" 

and GenGenericParam m g tyenv (tp:Typar) = 
    let subTypeConstraints             = tp.Constraints |> List.choose (function | TTyparCoercesToType(ty,m) -> Some(ty) | _ -> None) |> List.map (GenTypeAux m g tyenv VoidNotOK PtrTypesNotOK EmitMultiDimArrayTypesNormally)
    let refTypeConstraint              = tp.Constraints |> List.exists (function TTyparIsReferenceType _ -> true | TTyparSupportsNull _ -> true | _ -> false)
    let notNullableValueTypeConstraint = tp.Constraints |> List.exists (function TTyparIsNotNullableValueType _ -> true | _ -> false)
    let defaultConstructorConstraint   = tp.Constraints |> List.exists (function TTyparRequiresDefaultConstructor _ -> true | _ -> false)
    { gpName= 
          (let nm :string = tp.Name 
           if nm.TrimEnd([| '0' .. '9' |]).Length = 1 then nm 
           //elif nm.Length >= 1 && System.Char.IsLower nm.[0] then "T"^nm >= "T" && nm <= "Z" then nm 
           else "T"^(String.capitalize nm)); (* ^(if tp.IsCompilerGenerated then string tp.Stamp else "") *)
      gpConstraints=subTypeConstraints;
      gpVariance=NonVariant;
      gpReferenceTypeConstraint=refTypeConstraint;
      gpNotNullableValueTypeConstraint=notNullableValueTypeConstraint;
      gpDefaultConstructorConstraint= defaultConstructorConstraint }

//--------------------------------------------------------------------------
// Generate ILX references to closures, classunions etc. given a tyenv
//-------------------------------------------------------------------------- 

and GenUnionCaseRef m g tyenv i (fspecs:RecdField array) = 
    fspecs |> Array.mapi (fun j fspec -> 
      let fieldDef = IL.mk_instance_fdef(fspec.Name,GenType m g tyenv fspec.FormalType, None, ComputePublicMemberAccess true)
      { fieldDef with 
          // These properties on the "field" of an alternative end up going on a property generated by cu_erase.ml
          fdCustomAttrs = mk_custom_attrs [(mk_CompilationMappingAttrWithVariantNumAndSeqNum g SourceLevelConstruct_Field i j )] } )
   

and GenUnionRef m g tcref = 
    let tycon = (deref_tycon tcref)
    assert(not tycon.IsTypeAbbrev);
    match tycon.UnionInfo with 
    | None -> failwith "GenUnionRef m"
    | Some funion -> 
      cached funion.funion_ilx_repr (fun () -> 
        let tyenvinner = tyenv_for_tycon tycon
        match tcref.CompiledRepresentation with
        | TyrepOpen _ -> failwith "GenUnionRef m: unexpected ASM tyrep"
        | TyrepNamed (tref,_) -> 
          let alternatives = 
              tycon.UnionCasesArray |> Array.mapi (fun i cspec -> 
                  { altName=cspec.ucase_il_name;
                    altCustomAttrs=mk_custom_attrs [];
                    altFields=GenUnionCaseRef m g tyenvinner i cspec.RecdFieldsArray })
          let nullPermitted = IsUnionTypeWithNullAsTrueValue g tycon
          Ilx.IlxUnionRef(tref,alternatives,nullPermitted))


and GenUnionSpec m g tyenv tcref tyargs = 
    let curef = GenUnionRef m g tcref
    let tinst = GenTypeArgs m g tyenv tyargs
    Ilx.IlxUnionSpec(curef,tinst) 

and GenUnionCaseSpec m g tyenv (ucref:UnionCaseRef) tyargs = 
    let cuspec = GenUnionSpec m g tyenv ucref.TyconRef tyargs
    let idx = ucref_index ucref 
    cuspec, idx 


and GenType m g tyenv ty = (GenTypeAux m g tyenv VoidNotOK PtrTypesNotOK EmitMultiDimArrayTypesNormally ty)


and GenTypes m g tyenv tys = List.map (GenType m g tyenv) tys
and GenTypePermitVoid m g tyenv ty = (GenTypeAux m g tyenv VoidOK PtrTypesNotOK EmitMultiDimArrayTypesNormally ty)
and GenTypesPermitVoid m g tyenv tys = List.map (GenTypePermitVoid m g tyenv) tys

and GenTyApp m g tyenv repr tyargs = GenTyAppAux m g tyenv EmitMultiDimArrayTypesNormally repr tyargs
and GenNamedTyApp m g tyenv tcref tinst = GenNamedTyAppAux m g tyenv PtrTypesNotOK EmitMultiDimArrayTypesNormally tcref tinst 

/// IL pointer types are only generated for DLL Import signatures *
/// IL void types are only generated for return types 
and ComputePtrTypesOK isDllImport = (if isDllImport then PtrTypesOK else PtrTypesNotOK)
and ComputeMultiDimArrayEmitFlag isGeneric = 
#if FX_ATLEAST_40
    // in Dev10, the CLR has fixed the bug (see comment above "type MultiDimArrayEmitFlag" earlier in this file)
    EmitMultiDimArrayTypesNormally
#else
    (if isGeneric then EmitMultiDimArrayTypesWithoutBounds else EmitMultiDimArrayTypesNormally)
#endif

and GenReturnType m g tyenv isDllImport isGeneric returnTyOpt = 
    match returnTyOpt with 
    | None -> Type_void
    | Some returnTy -> GenTypeAux m g tyenv VoidNotOK(*1*) (ComputePtrTypesOK isDllImport) (ComputeMultiDimArrayEmitFlag isGeneric) returnTy (*1: generate void from unit, but not accept void *)

and GenParamType m g tyenv isDllImport isGeneric ty = 
    ty |> GenTypeAux m g tyenv VoidNotOK (ComputePtrTypesOK isDllImport) (ComputeMultiDimArrayEmitFlag isGeneric)

and GenParamTypes m g tyenv isDllImport isGeneric tys = 
    tys |> List.map (GenTypeAux m g tyenv VoidNotOK (ComputePtrTypesOK isDllImport) (ComputeMultiDimArrayEmitFlag isGeneric)) 

and GenTypeArgs m g tyenv tyargs = GenTyargsAux m g tyenv EmitMultiDimArrayTypesNormally tyargs

let GenericParamHasConstraint gp = 
     nonNil gp.gpConstraints ||
     gp.gpVariance <> NonVariant ||
     gp.gpReferenceTypeConstraint ||
     gp.gpNotNullableValueTypeConstraint ||
     gp.gpDefaultConstructorConstraint


let repr_of_named_type cloc nm boxity =
    TyrepNamed (NestedTypeRefForCompLoc cloc nm,boxity)



(* Static fields generally go in a private StartupCode section. This is to ensure all static 
   fields are initialized only in their class constructors (we generate one primary 
   cctor for each file to ensure initialization coherence across the file, regardless 
   of how many modules are in the file). This means F# passes an extra check applied by SQL Server when it
   verifies stored procedures: SQL Server checks that all 'initionly' static fields are only initialized from
   their own class constructor. 
   
   However, mutable static fields must be accessible across compilation units. This means we place them in their "natural" location
   which may be in a nested module etc. This means mutable static fields can't be used in code to be loaded by SQL Server. *)
   
let UseGenuineStaticField g (vspec:Val) =
    let mut = vspec.IsMutable
    let attribs = vspec.Attribs
    let hasLiteralAttr = HasAttrib g g.attrib_LiteralAttribute attribs
    mut || hasLiteralAttr 

let GenFieldSpecForStaticField g ilTypeSpecForProperty vspec cloc fieldName il_ty =
    /// Where does the field live?
    let tspec = 
        if UseGenuineStaticField g vspec then ilTypeSpecForProperty 
        else TypeSpecForCompLoc  (CompLocForStartupCode cloc)

    mk_fspec_in_boxed_tspec (tspec,fieldName, il_ty) 

(* REVIEW: this logic is also duplicated in tc.ml's attribute type checking code *)
let GenFieldName tycon fld =  gen_field_name tycon fld

let GenRecdFieldRef m cenv tyenv (rfref:RecdFieldRef) tyargs = 
    let tyenvinner = tyenv_for_tycon rfref.Tycon
    mk_fspec_in_typ(GenTyApp m cenv.g tyenv rfref.TyconRef.CompiledRepresentation tyargs,
                    GenFieldName rfref.Tycon rfref.RecdField,
                    GenType m cenv.g tyenvinner rfref.RecdField.FormalType)

let GenExnType m g tyenv (ecref:TyconRef) = GenTyApp m g tyenv ecref.CompiledRepresentation []


//--------------------------------------------------------------------------
// Closure summaries
//-------------------------------------------------------------------------- 

type arityInfo = int list
      

[<StructuralEquality(false); StructuralComparison(false)>]
type IlxClosureInfo = 
    { clo_expr: expr;
      clo_name: string;
      clo_arity_info: arityInfo;
      clo_formal_il_rty: ILType;
      clo_il_frees: Ilx.IlxClosureFreeVar list;
      clo_clospec: Ilx.IlxClosureSpec;
      clo_attribs: Attribs;
      clo_il_gparams: IL.ILGenericParameterDefs;
      clo_freevars: Val list; (* nb. the freevars we actually close over *)
      clo_lambdas: Ilx.IlxClosureLambdas;

      (* local type func support *)
      /// The free type parameters occuring in the type of the closure (and not just its body)
      /// This is used for local type functions, whose contract class must use these types
      ///    type Contract<'fv> =
      ///        abstract DirectInvoke : ty['fv]
      ///    type Implementation<'fv,'fv2> : Contract<'fv> =
      ///        override DirectInvoke : ty['fv] = expr['fv,'fv2]
      ///
      ///   At the callsite we generate
      ///      unbox ty['fv]
      ///      callvirt clo.DirectInvoke
      ltyfunc_contract_il_gactuals: ILType list;
      ltyfunc_contract_ftyvs: Typar list;
      ltyfunc_direct_il_gparams: IL.ILGenericParameterDefs 
      ltyfunc_internal_ftyvs: Typar list;}


//--------------------------------------------------------------------------
// Representation of term declarations = Environments for compiling expressions.
//-------------------------------------------------------------------------- 

      
[<StructuralEquality(false); StructuralComparison(false)>]
type storage = 
    /// Indicates the value is always null
    | Null 
    /// Indicates the value is not stored, and no value is created 
    | Unrealized 
    /// Indicates the value is stored in a static field. 
    | StaticField of ILFieldSpec * ValRef * (*hasLiteralAttr:*)bool * ILTypeSpec * string * string * ILType * ILMethodRef  * ILMethodRef  * OptionalShadowLocal
    /// Indicates the value is "stored" as a IL static method (in a "main" class for a F# 
    /// compilation unit, or as a member) according to its inferred or specified arity.  
    | Method of  ValTopReprInfo * ValRef * ILMethodSpec * Range.range * TopArgInfo list * TopArgInfo
    /// Indicates the value is stored at the given position in the closure environment accessed via "ldarg 0"
    | Env of ILTypeSpec * int * NamedLocalIlxClosureInfo ref option  
    /// Indicates that the value is an argument of a method being generated
    | Arg of int 
    /// Indicates that the value is stored in local of the method being generated. NamedLocalIlxClosureInfo is normally empty.
    /// It is non-empty for 'local type functions', see comments on definition of NamedLocalIlxClosureInfo.
    | Local of int * NamedLocalIlxClosureInfo ref option 
and OptionalShadowLocal = 
   | NoShadowLocal
   | ShadowLocal of storage
/// The representation of a NamedLocalClosure is based on a cloinfo.  However we can't generate a cloinfo until we've 
/// decided the representations of other items in the recursive set. Hence we use two phases to decide representations in 
/// a recursive set. Yuck. 
and NamedLocalIlxClosureInfo = 
    | NamedLocalIlxClosureInfoGenerator of (ilxGenEnv -> IlxClosureInfo)
    | NamedLocalIlxClosureInfoGenerated of IlxClosureInfo
  
and ModuleStorage = 
    { storage_vals: Lazy<NameMap<storage>> ;
      storage_submoduls: Lazy<NameMap<ModuleStorage>>; }

/// BranchCallItems are those where a call to the value can be implemented as 
/// a branch. At the moment these are only used for generating branch calls back to 
/// the entry label of the method currently being generated. 
and BranchCallItem = 
    | BranchCallClosure of arityInfo
    | BranchCallMethod of 
        // Argument counts for compiled form  of F# method or value
        arityInfo * 
        // Arg infos for compiled form of F# method or value
        (Tast.typ * TopArgInfo) list list * 
        // Typars for F# method or value
        Tast.typars * 
        // Typars for F# method or value
        int *
        // num obj args 
        int 
      
and mark = Mark of ILCodeLabel (* places we can branch to  *)

and ilxGenEnv =
    { tyenv: TypeReprEnv; 
      someTspecInThisModule: ILTypeSpec;
      /// Where to place the stuff we're currently generating
      cloc: cloc; 
      /// Hiding information down the signature chain, used to compute what's public to the assembly 
      sigToImplRemapInfo: (Remap * SignatureHidingInfo) list; 
      /// All values in scope 
      valsInScope: ValMap<Lazy<storage>>; 
      /// For optimizing direct tail recusion to a loop - mark says where to branch to.  Length is 0 or 1. 
      /// REVIEW: generalize to arbitrary nested local loops?? 
      innerVals: (ValRef * (BranchCallItem * mark)) list; 
      /// Full list of enclosing bound values.  First non-compiler-generated element is used to help give nice names for closures and other expressions.  
      letBoundVars: ValRef list; 
      /// The set of IL local variable indexes currently in use by lexically scoped variables, to allow reuse on different branches. 
      /// Really an integer set. 
      liveLocals: unit Imap.t; 
      /// Are we under the scope of a try, catch or finally? If so we can't tailcall. SEH = structured exception handling
      withinSEH: bool;   }

let replace_tyenv tyenv (eenv: ilxGenEnv) = {eenv with tyenv = tyenv}
let env_for_typars tps eenv =  replace_tyenv (tyenv_for_typars tps) eenv
let AddTyparsToEnv typars (eenv:ilxGenEnv) = {eenv with tyenv = add_typars eenv.tyenv typars}

let AddSignatureRemapInfo msg (rpi,mhi) eenv = 
    if verbose then dprintf "AddSignatureRemapInfo, %s, #tycons = %s\n" msg (showL (Layout.sepListL (wordL ";") (List.map tyconL (Zset.elements mhi.mhiTycons))));
    if verbose then dprintf "AddSignatureRemapInfo, %s, #rpi.mrpiTycons = %d, #tyconReprs = %s\n" msg (List.length rpi.mrpiTycons) (showL (Layout.sepListL (wordL ";") (List.map tyconL (Zset.elements mhi.mhiTyconReprs))));
    if verbose then dprintf "AddSignatureRemapInfo, %s, #vals = %s\n" msg (showL (Layout.sepListL (wordL ";") (List.map valL (Zset.elements mhi.mhiVals))));
    if verbose then dprintf "AddSignatureRemapInfo, %s, #rfrefs = %s\n" msg (showL (Layout.sepListL (wordL ";") (List.map recdFieldRefL (Zset.elements mhi.mhiRecdFields))));
    { eenv with sigToImplRemapInfo = (mk_repackage_remapping rpi,mhi) :: eenv.sigToImplRemapInfo }
     
//--------------------------------------------------------------------------
// Print eenv
//-------------------------------------------------------------------------- 

let output_storage pps s = 
    match s with 
    | StaticField _ -> output_string pps "(top)" 
    | Method _ -> output_string pps "(top)" 
    | Local _ -> output_string pps "(local)" 
    | Arg _ -> output_string pps "(arg)" 
    | Env _ -> output_string pps "(env)" 
    | Null -> output_string pps "(null)"
    | Unrealized -> output_string pps "(no real value required)"

//--------------------------------------------------------------------------
// Augment eenv with values
//-------------------------------------------------------------------------- 

let AddStorageForVal g (v,s) eenv = 
    if verbose then dprintf "adding %s to value table\n" (showL (valL v));
    let eenv = { eenv with valsInScope = vspec_map_add v s eenv.valsInScope }
    // when compiling fslib also add an entry under the results of a non-local lookup 
    if g.compilingFslib then 
        match v.PublicPath with 
        | None -> eenv
        | Some pp -> 
            match try_deref_val (rescope_val_pubpath g.fslibCcu pp v) with
            | None -> eenv
            | Some gv -> 
                if verbose then dprintf "adding remapped %s to value table\n" (showL (valL gv));
                { eenv with valsInScope = vspec_map_add gv s eenv.valsInScope }
    else 
        eenv

let AddStorageForLocalVals g vals eenv = List.foldBack (fun (v,s) acc -> AddStorageForVal g (v,notlazy s) acc) vals eenv

//--------------------------------------------------------------------------
// Lookup eenv 
//-------------------------------------------------------------------------- 
  
open Microsoft.FSharp.Compiler.AbstractIL
open Microsoft.FSharp.Compiler.AbstractIL.Internal 
open Microsoft.FSharp.Compiler.AbstractIL.Internal.Library 

let storage_for_val m v eenv = 
    let v = 
        try vspec_map_find v eenv.valsInScope
        with Not_found ->
          (* REVIEW: The  binary will probably still be written under these error conditions.
           *         That is useful when debugging the compiler, but not in Retail mode.
           *         Fail with an internal error if Retail? *)
          (* // Diagnostics for bug://4046
           * let vals = eenv.valsInScope.imap |> Zmap.to_list
           * vals |> List.iter (printf "v,s = %A\n")          
           *)
          assert false
          errorR(Error(sprintf "undefined value: %s" (showL(vspecAtBindL v)),m)); 
          notlazy (Arg 668(* random value for post-hoc diagnostic analysis on generated tree *) )
    v.Force()

let storage_for_vref m v eenv = storage_for_val m (deref_val v) eenv

//--------------------------------------------------------------------------
// Imported modules and the environment
//
// How a top level value is represented depends on its type.  If it's a 
// function or is polymorphic, then it gets represented as a 
// method (possibly and instance method).  Otherwise it gets represented as a 
// static field.
//-------------------------------------------------------------------------- 

let vref_isDllImport g (vref:ValRef) = 
    vref.Attribs |> HasAttrib g g.attrib_DllImportAttribute 

let GetMethodSpecForMemberVal g memberInfo (vref:ValRef) = 
    let m = vref.Range
    let tps,curriedArgInfos,returnTy,retInfo = 
         assert(vref.TopValInfo.IsSome);
         GetTopValTypeInCompiledForm g (the vref.TopValInfo) vref.Type m
    let tyenv_under_typars = tyenv_for_typars tps
    let flatArgInfos = List.concat curriedArgInfos
    let isDllImport = vref_isDllImport g vref
    let isCtor = (memberInfo.MemberFlags.MemberKind = MemberKindConstructor)
    let cctor = (memberInfo.MemberFlags.MemberKind = MemberKindClassConstructor)
    let parentTcref = vref.MemberActualParent
    let parentTypars = parentTcref.TyparsNoRange
    let numParentTypars = parentTypars.Length
    if tps.Length < numParentTypars then error(InternalError("CodeGen check: type checking did not ensure that this method is sufficiently generic", m));
    let ctps,mtps = List.chop numParentTypars tps
    let isGeneric = nonNil (DropErasedTypars tps)
    let isCompiledAsInstance = ValRefIsCompiledAsInstanceMember g vref

    let ilActualRetTy = 
        let ilRetTy = GenReturnType m g tyenv_under_typars isDllImport isGeneric returnTy
        if isCtor || cctor then Type_void else ilRetTy
    let ilTy = GenType m g tyenv_under_typars (mk_tyapp_ty parentTcref (List.map mk_typar_ty ctps))
    if isCompiledAsInstance || isCtor then 
        // Find the 'this' argument type if any 
        let thisTy,flatArgInfos = 
            if isCtor then (GetFSharpViewOfReturnType g returnTy),flatArgInfos 
            else 
               match flatArgInfos with 
               | [] -> error(InternalError("This instance method '"^vref.MangledName^"' has no arguments", m))
               | (h,_):: t -> h,t

        let thisTy = if is_byref_typ g thisTy then dest_byref_typ g thisTy else thisTy
        let thisArgTys = tinst_of_stripped_typ g thisTy
        if ctps.Length <> thisArgTys.Length then
           warning(InternalError(Printf.sprintf "CodeGen check: type checking did not quantify the correct number of type variables for this method, #parentTypars = %d, #ctps = %d, #mtps = %d, #thisArgTys = %d" numParentTypars ctps.Length mtps.Length (List.length thisArgTys),m))
        else 
           List.iter2
              (fun gtp ty2 -> 
                if not (type_equiv g (mk_typar_ty gtp) ty2) then 
                  warning(InternalError("CodeGen check: type checking did not quantify the correct type variables for this method: generalization list contained "^gtp.Name^"#"^string gtp.Stamp^" and list from 'this' pointer contained "^ (showL(typeL ty2)), m)))
              ctps 
              thisArgTys;
        let methodArgTys,paramInfos = List.unzip flatArgInfos
        let ilMethodArgTys = GenParamTypes m g tyenv_under_typars isDllImport isGeneric methodArgTys
        let ilMethodInst = GenTypeArgs m g tyenv_under_typars (List.map mk_typar_ty mtps)
        let mspec = mk_instance_mspec_in_typ (ilTy,memberInfo.CompiledName,ilMethodArgTys,ilActualRetTy,ilMethodInst)
        
        mspec,ctps,mtps,paramInfos,retInfo
    else 
        let methodArgTys,paramInfos = List.unzip flatArgInfos
        let ilMethodArgTys = GenParamTypes m g tyenv_under_typars isDllImport isGeneric methodArgTys
        let ilMethodInst = GenTypeArgs m g tyenv_under_typars (List.map mk_typar_ty mtps)
        let mspec = mk_static_mspec_in_typ (ilTy,memberInfo.CompiledName,ilMethodArgTys,ilActualRetTy,ilMethodInst)
        
        mspec,ctps,mtps,paramInfos,retInfo

// This called via 2 routes.
// (a) alloc_or_import_{ccu,modval,top_vspec} - for vref from modulespec mtyp_vals.
// (b) AllocStorageForBind - if arity specified for vref. This route includes some compiler generated temporaries.
//
/// This function decides the storage for the val.
/// The decision is based on arityInfo.
let ComputeStorageForTopVal g optShadowLocal (vref:ValRef) cloc =

    if is_unit_typ g vref.Type  && not vref.IsMemberOrModuleBinding && not vref.IsMutable then  Null   else
    let topValInfo = 
        match vref.TopValInfo with 
        | None -> error(InternalError("ComputeStorageForTopVal: no arity found for "^showL(ValRefL vref),vref.Range))
        | Some a -> a
        
    let m = vref.Range
    let nm = vref.UniqueCompiledName 

    // This call to GetTopValTypeInFSharpForm is only needed to determine if this is a (type) function or a value
    // We should just look at the arity
    match GetTopValTypeInFSharpForm g topValInfo vref.Type vref.Range with 
    | [],[], returnTy,retInfo when not vref.IsMember ->
        // Mutable and literal static fields must have stable names and live in the "public" location 
        // See notes on GenFieldSpecForStaticField above. 
        let vspec = (deref_val vref)
        let fieldName = if UseGenuineStaticField g vspec then nm else ilxgenGlobalNng.FreshCompilerGeneratedName (nm,m)
        let il_ty = GenType m g empty_tyenv returnTy in (* empty_tyenv ok: not a field in a generic class *)
        let ilTypeSpecForProperty = TypeSpecForCompLoc cloc
        let attribs = vspec.Attribs
        let mut = vspec.IsMutable
        let hasLiteralAttr = HasAttrib g g.attrib_LiteralAttribute attribs

        let tref = ilTypeSpecForProperty.TypeRef
        let ilGetterMethRef = mk_mref(tref,ILCallingConv.Static,"get_"^nm,0,[],il_ty)
        let ilSetterMethRef = mk_mref(tref,ILCallingConv.Static,"set_"^nm,0,[il_ty],Type_void)
        let fspec = GenFieldSpecForStaticField g ilTypeSpecForProperty vspec cloc fieldName il_ty
        StaticField (fspec,vref,hasLiteralAttr,ilTypeSpecForProperty,fieldName,nm,il_ty,ilGetterMethRef,ilSetterMethRef,optShadowLocal)
          
    | _ -> 
        match vref.MemberInfo with 
        | Some(memberInfo) when not vref.IsExtensionMember -> 
            let mspec,_,_,paramInfos,retInfo = GetMethodSpecForMemberVal g memberInfo vref
            Method (topValInfo,vref,mspec, m,paramInfos,retInfo) 
        | _ -> 
            let (tps,curriedArgInfos,returnTy,retInfo) = GetTopValTypeInCompiledForm g topValInfo vref.Type m 
            let tyenv_under_typars = tyenv_for_typars tps
            let (methodArgTys,paramInfos) = curriedArgInfos |> List.concat |> List.unzip 
            let isDllImport = vref_isDllImport g vref
            let isGeneric = nonNil (DropErasedTypars tps)
            let ilMethodArgTys = GenParamTypes m g tyenv_under_typars isDllImport isGeneric methodArgTys
            let ilRetTy = GenReturnType m g tyenv_under_typars isDllImport isGeneric returnTy
            let tspec = TypeSpecForCompLoc cloc
            let ilMethodInst = GenTypeArgs m g tyenv_under_typars (List.map mk_typar_ty tps)
            let mspec = mk_static_mspec_in_boxed_tspec (tspec,nm,ilMethodArgTys,ilRetTy,ilMethodInst)
            
            Method(topValInfo,vref,mspec, m,paramInfos,retInfo)

let ComputeAndAddStorageForLocalTopVal g cloc optShadowLocal (v:Val) eenv =
    let storage = ComputeStorageForTopVal g optShadowLocal (mk_local_vref v) cloc
    AddStorageForVal g (v,notlazy storage) eenv

let ComputeStorageForNonLocalTopVal g cloc modref (v:Val) =
    //if inline_info_of_val v = PseudoValue then Unrealized else 
    match v.TopValInfo with 
    | None -> error(InternalError("ComputeStorageForNonLocalTopVal, expected an arity for "^v.MangledName,v.Range))
    | Some _ -> ComputeStorageForTopVal g NoShadowLocal (mk_vref_in_modref modref v) cloc

let rec ComputeStorageForNonLocalModuleOrNamespaceRef g cloc (modref:ModuleOrNamespaceRef) (modul:ModuleOrNamespace) acc = 
    if verbose then dprintn ("ComputeStorageForNonLocalModuleOrNamespaceRef for module "^demangled_name_of_modul modul);
    NameMap.foldRange 
        (fun v acc -> AddStorageForVal g (v, lazy (ComputeStorageForNonLocalTopVal g cloc modref v)) acc) 
        modul.ModuleOrNamespaceType.AllValuesAndMembers
        (NameMap.foldRange 
            (fun smodul acc -> ComputeStorageForNonLocalModuleOrNamespaceRef g (CompLocForSubModuleOrNamespace cloc smodul) (MakeNestedTcref modref smodul) smodul acc) 
            modul.ModuleOrNamespaceType.ModulesAndNamespacesByDemangledName
            acc)

let ComputeStorageForExternalCcu g  eenv (ccu:ccu) = 
    if not ccu.IsFSharp then eenv else
    let cloc = CompLocForCcu ccu
    if verbose then dprintn ("ComputeStorageForExternalCcu, ccu = "^ccu.AssemblyName);
    let eenv = 
       NameMap.foldRange
           (fun smodul acc -> 
               let cloc = CompLocForSubModuleOrNamespace cloc smodul
               let modref =  mk_nonlocal_ccu_top_tcref ccu smodul
               ComputeStorageForNonLocalModuleOrNamespaceRef g cloc modref smodul acc)
           ccu.TopModulesAndNamespaces
           eenv
    eenv
    
let rec AddBindingsForLocalModuleType allocVal cloc eenv (mty:ModuleOrNamespaceType) = 
    let eenv = NameMap.foldRange (fun submodul eenv -> AddBindingsForLocalModuleType allocVal (CompLocForSubModuleOrNamespace cloc submodul) eenv submodul.ModuleOrNamespaceType) mty.ModulesAndNamespacesByDemangledName eenv
    let eenv = NameMap.foldRange (allocVal cloc) mty.AllValuesAndMembers eenv
    eenv 

let AddExternalCcusToIlxGenEnv g eenv ccus = List.fold (ComputeStorageForExternalCcu g) eenv ccus

let AddBindingsForTycon allocVal (cloc:cloc) (tycon:Tycon) eenv  =
    let unrealized_slots = 
        if tycon.IsFSharpObjectModelTycon
        then tycon.FSharpObjectModelTypeInfo.fsobjmodel_vslots 
        else []
    List.foldBack (fun vref eenv -> allocVal cloc (deref_val vref) eenv) unrealized_slots eenv

let rec AddBindingsForModuleDefs allocVal (cloc:cloc) eenv  mdefs = 
    List.fold (AddBindingsForModuleDef allocVal cloc) eenv mdefs

and AddBindingsForModuleDef allocVal cloc eenv x = 
    match x with 
    | TMDefRec(tycons,vbinds,mbinds,m) -> 
        let eenv = FlatList.foldBack (allocVal cloc) (vars_of_Bindings vbinds) eenv
        (* Virtual don't have 'let' bindings and must be added to the environment *)
        let eenv = List.foldBack (AddBindingsForTycon allocVal cloc) tycons eenv
        let eenv = List.foldBack (AddBindingsForSubModules allocVal cloc) mbinds eenv
        eenv
    | TMDefLet(bind,m) -> 
        allocVal cloc bind.Var eenv
    | TMDefDo(e,m) -> 
        eenv
    | TMAbstract(TMTyped(mtyp,_,_)) -> 
        AddBindingsForLocalModuleType allocVal cloc eenv  mtyp
    | TMDefs(mdefs) -> 
        AddBindingsForModuleDefs allocVal cloc eenv  mdefs 

and AddBindingsForSubModules allocVal cloc (TMBind(mspec, mdef)) eenv = 
    let cloc = 
        if mspec.IsNamespace then cloc 
        else CompLocForFixedModule cloc.clocQualifiedNameOfFile cloc.clocTopImplQualifiedName mspec
        
    AddBindingsForModuleDef allocVal cloc eenv mdef

and AddBindingsForModuleTopVals g allocVal cloc eenv vs = 
    FlatList.foldBack allocVal vs eenv


// Put the partial results for a generated fragment (i.e. a part of a CCU generated by FSI) 
// into the stored results for the whole CCU.  
// isIncrementalExtension = true -->  "#use or typed input" 
// isIncrementalExtension = false -->  "#load" 
let AddIncrementalLocalAssmblyFragmentToIlxGenEnv isIncrementalExtension  g ccu fragName eenv (TAssembly impls) = 
    let cloc = CompLocForFragment fragName ccu
    let allocVal cloc v = ComputeAndAddStorageForLocalTopVal g cloc NoShadowLocal v
    List.fold (fun eenv (TImplFile(qname,_,mexpr)) -> 
        let cloc = { cloc with clocTopImplQualifiedName = qname.Text }
        if isIncrementalExtension then 
            match mexpr with
            | TMTyped(_,mdef,_) -> AddBindingsForModuleDef allocVal cloc eenv mdef
            (* | TMTyped(mtyp,_,m) -> error(Error("don't expect inner defs to have a constraint",m)) *)
        else
            AddBindingsForLocalModuleType allocVal cloc eenv (mtyp_of_mexpr mexpr)
            
        ) eenv  impls

//--------------------------------------------------------------------------
// Generate debugging marks 
//-------------------------------------------------------------------------- 

let GenILSourceMarker g m = 
  Some (ILSourceMarker.Create(document=g.memoize_file (file_idx_of_range m),
                            line=start_line_of_range m,
                            /// NOTE: .NET && VS  measure first column as column 1
                            column= (start_col_of_range m)+1, 
                            endLine= (end_line_of_range m),
                            endColumn=(end_col_of_range m)+1)) 

let GenPossibleILSourceMarker cenv m = if cenv.debug then GenILSourceMarker cenv.g m else None

//--------------------------------------------------------------------------
// Helpers for merging property definitions
//--------------------------------------------------------------------------

let hashtbl_range ht = 
    let res = ref []
    Hashtbl.iter (fun _ x -> res := x :: !res) ht; 
    !res

let merge_options m o1 o2 = 
    match o1,o2 with
    | Some x, None | None, Some x -> Some x
    | None, None -> None
    | Some x, Some _ -> 
#if DEBUG
       // This warning fires on some code that also triggers this warning:
       //          warning(Error("The implementation of a specified generic interface required a method implementation not fully supported by F# Interactive. In the unlikely event that the resulting class fails to load then compile the interface type into a statically-compiled DLL and reference it using '#r'",m));
       // THe code is OK so we don't print this.
       errorR(InternalError("merge_options: two values given",m)); 
#endif
       Some x 

let merge_pdefs m pd pdef = 
    {pd with propGet=merge_options m pd.propGet pdef.propGet;
             propSet=merge_options m pd.propSet pdef.propSet; } 

let add_pdef_to_hash m ht pdef =
    let nm = pdef.propName
    if Hashtbl.mem ht nm then
        let pd = Hashtbl.find ht nm
        Hashtbl.replace ht nm (merge_pdefs m pd pdef) 
    else 
        Hashtbl.add ht nm pdef
    

/// Merge a whole group of properties all at once 
let merge_pdef_list m propertyDefs = 
    let ht = Hashtbl.create 0
    propertyDefs |> List.iter (add_pdef_to_hash m ht);  
    hashtbl_range ht

//--------------------------------------------------------------------------
// Buffers for compiling modules. The entire assembly gets compiled via an AssemblyBuilder
//-------------------------------------------------------------------------- 

/// Information collected imperatively for each type definition 
type TypeDefBuilder(tdef) = 
    let gbasic     = tdef
    let gmethods   = new ResizeArray<ILMethodDef>(0)
    let gfields    = new ResizeArray<ILFieldDef>(0)
    let gproperties : Hashtbl.t<string,ILPropertyDef> = Hashtbl.create 0
    let gevents    = new ResizeArray<ILEventDef>(0)
    let gnested    = new TypeDefsBuilder()
    
    member b.Close() = 
        { tdef with 
            tdMethodDefs = mk_mdefs      (dest_mdefs tdef.tdMethodDefs @ ResizeArray.to_list gmethods);
            tdFieldDefs  = mk_fdefs      (dest_fdefs tdef.tdFieldDefs  @ ResizeArray.to_list gfields);
            tdProperties = mk_properties (dest_pdefs tdef.tdProperties @ hashtbl_range gproperties);
            tdEvents     = mk_events     (dest_edefs tdef.tdEvents     @ ResizeArray.to_list gevents);
            tdNested     = mk_tdefs      (dest_tdefs tdef.tdNested     @ gnested.Close()) }


    member b.AddEventDef(edef) = ResizeArray.add gevents edef
    member b.AddFieldDef(fieldDef) = ResizeArray.add gfields fieldDef
    member b.AddMethodDef(mdef) = ResizeArray.add gmethods mdef
    member b.NestedTypeDefs = gnested
    member b.GetCurrentFields() = gfields |> Seq.readonly

    /// Merge Get and Set property nodes, which we generate independently for F# code 
    /// when we come across their corresponding methods. 
    member b.AddOrMergePropertyDef(pdef,m) = add_pdef_to_hash m gproperties pdef

    member b.PrependInstructionsToSpecificMethodDef(cond,instrs,tag) = 
        match ResizeArray.tryfind_index cond gmethods with
        | Some idx -> gmethods.[idx] <-  prepend_instrs_to_mdef instrs gmethods.[idx]
        | None -> gmethods.Add(mk_cctor (mk_impl (false,[],1,nonbranching_instrs_to_code instrs,tag)))


and TypeDefsBuilder() = 
    let tdefs : Hashtbl.t<string,TypeDefBuilder> = Hashtbl.create 0

    member b.Close() = tdefs |> hashtbl_range |> List.map (fun b -> b.Close())

    member b.FindTypeDefBuilder(nm) = 
        try Hashtbl.find tdefs nm 
        with Not_found -> failwith ("find_gtdef: "^nm^" not found")

    member b.FindNestedTypeDefsBuilder(path) = 
        List.fold (fun (acc:TypeDefsBuilder) x -> acc.FindTypeDefBuilder(x).NestedTypeDefs) b path

    member b.FindNestedTypeDefBuilder(tref:ILTypeRef) = 
        b.FindNestedTypeDefsBuilder(tref.Enclosing).FindTypeDefBuilder(tref.Name)

    member b.AddTypeDef(tdef:ILTypeDef) = 
        Hashtbl.add tdefs tdef.tdName (new TypeDefBuilder(tdef))

/// Assembly generation buffers 
type AssemblyBuilder(cenv:cenv) as mgbuf = 
    // The Abstract IL table of types 
    let gtdefs= new TypeDefsBuilder() 
    // The definitions of top level values, as quotations. 
    let mutable reflectedDefinitions : (Tast.Val * Tast.expr) list = []
    // A memoization table for generating value types for big constant arrays  
    let vtgenerator=
         new MemoizationTable<(cloc * int) , ILTypeSpec>
               (fun (cloc,size) -> 
                 let name   = mk_private_name ("T" ^ string(new_uniq()) ^ "_" ^ string size ^ "Bytes") // Type names ending ...$T<unique>_37Bytes
                 let vtdef  = mk_rawdata_vtdef cenv.g.ilg (name,size,0us)
                 let vtspec = NestedTypeSpecForCompLoc cloc vtdef.tdName []
                 let vtref = vtspec.TypeRef
                 let vtdef = {vtdef with tdAccess= ComputeTypeAccess vtref true}
                 mgbuf.AddTypeDef(vtref,vtdef);
                 vtspec)

    let mutable explicitEntryPointInfo : ILTypeRef option  = None

    member mgbuf.GenerateRawDataValueType(cloc,size) = 
        // Byte array literals require a ValueType of size the required number of bytes.
        // With fsi.exe, S.R.Emit TypeBuilder CreateType has restrictions when a ValueType VT is nested inside a type T, and T has a field of type VT.
        // To avoid this situation, these ValueTypes are generated under the private implementation rather than in the current cloc. [was bug 1532].
        let cloc = CompLocForPrivateImplementationDetails cloc
        vtgenerator.Apply((cloc,size))

    member mgbuf.AddTypeDef(tref:ILTypeRef,tdef) = 
        gtdefs.FindNestedTypeDefsBuilder(tref.Enclosing).AddTypeDef(tdef)

    member mgbuf.GetCurrentFields(tref:ILTypeRef) =
        gtdefs.FindNestedTypeDefBuilder(tref).GetCurrentFields();

    member mgbuf.AddReflectedDefinition(vspec,expr) = 
        reflectedDefinitions <- (vspec,expr) :: reflectedDefinitions

    member mgbuf.AddMethodDef(tref:ILTypeRef,mdef) = 
        gtdefs.FindNestedTypeDefBuilder(tref).AddMethodDef(mdef);
        if mdef.mdEntrypoint then 
            explicitEntryPointInfo <- Some(tref)

    member mgbuf.AddExplicitInitToSpecificMethodDef(cond,tref,fspec,m) = 
        let instrs = 
            [ mk_ldc_i32 0; 
              mk_normal_stsfld fspec; 
              mk_normal_ldsfld fspec; 
              i_pop]   
        gtdefs.FindNestedTypeDefBuilder(tref).PrependInstructionsToSpecificMethodDef(cond,instrs,m) 

    member mgbuf.AddEventDef(tref,edef) = 
        gtdefs.FindNestedTypeDefBuilder(tref).AddEventDef(edef)

    member mgbuf.AddFieldDef(tref,fieldDef) = 
        gtdefs.FindNestedTypeDefBuilder(tref).AddFieldDef(fieldDef)

    member mgbuf.AddOrMergePropertyDef(tref,pdef,m) = 
        gtdefs.FindNestedTypeDefBuilder(tref).AddOrMergePropertyDef(pdef,m)

    member mgbuf.Close() = gtdefs.Close(), reflectedDefinitions
    member mgbuf.cenv = cenv
    member mgbuf.GetExplicitEntryPointInfo() = explicitEntryPointInfo

     
let code_label_of_mark (Mark(lab)) = lab

/// Record the types of the things on the evaluation stack. 
/// Used for the few times we have to flush the IL evaluation stack and to compute maxStack. 
type pushpop = 
    | Push of ILType 
    | Pop

let push ty = Push ty

/// Buffers for IL code generation
type CodeGenBuffer(m:range,
                   mgbuf: AssemblyBuilder,
                   methodName,
                   entryPointInfo: (ValRef * BranchCallItem) list,
                   alreadyUsedArgs:int,
                   alreadyUsedLocals:int,
                   zapFirstSeqPointToStart:bool) = 
    let locals:  ResizeArray<((string * (mark * mark)) list * ILType)> = ResizeArray.create 10
    let codebuf : ResizeArray<ILInstr> = ResizeArray.create 200 
    let exnSpecs:  ResizeArray<ILExceptionSpec> = ResizeArray.create 10

    // Keep track of the current stack so we can spill stuff when we hit a "try" when some stuff
    // is on the stack.        
    let mutable stack : ILType list = []
    let mutable nstack=0
    let mutable maxStack=0
    let mutable seqpoint= None
    
    let codeLabels : Hashtbl.t<ILCodeLabel,int> =Hashtbl.create 10 
    
    let mutable lastSeqPoint = None
    // Add a nop to make way for the first sequence point. There is always such a 
    // sequence point even when zapFirstSeqPointToStart=false
    do if mgbuf.cenv.debug  then codebuf.Add(i_nop);

    member cgbuf.DoAction a = 
        match a with 
        | Push ty -> 
           stack <- ty :: stack; 
           nstack <- nstack + 1;
           maxStack <- Operators.max maxStack nstack
        | Pop -> 
           match stack with
           | [] -> 
               warning(InternalError("pop on empty stack during code generation\n",m));
           | h::t -> 
               stack <- t; 
               nstack <- nstack - 1

    member cgbuf.GetCurrentStack() = stack
    member cgbuf.AssertEmptyStack() = 
        if nonNil stack then warning(InternalError("stack flush didn't work, or extraneous expressions left on stack before stack restore",m));
        ()

    member cgbuf.EmitInstr(pps,i) = 
        pps |> List.iter cgbuf.DoAction;
        ResizeArray.add codebuf i

    member cgbuf.EmitInstrs (pps,is) = 
        pps |> List.iter cgbuf.DoAction;
        is |> List.iter (ResizeArray.add codebuf) 

    member cgbuf.GetLastSequencePoint() = 
        lastSeqPoint
       
    member cgbuf.EmitSeqPoint(src) = 
        if mgbuf.cenv.debug then 
            // Always add a nop between sequence points to help .NET get the stepping right
            if (codebuf.Count > 0 && match codebuf.[codebuf.Count-1] with I_seqpoint _ -> true | _ -> false) then 
                codebuf.Add(i_nop);
            let attr = GenILSourceMarker mgbuf.cenv.g src
            assert(isSome(attr));
            let i = I_seqpoint (the attr)
            codebuf.Add(i);
            // Save the first sequence point away to snap it to the top of the method
            match seqpoint with 
            | Some _ -> ()
            | None -> seqpoint <- Some i
            // Save the last sequence point away so we can make a decision graph look consistent (i.e. reassert the sequence point at each target)
            lastSeqPoint <- Some src
            
    member cgbuf.EmitExceptionClause(clause) = 
         exnSpecs.Add clause

    member cgbuf.EmitDelayMark(nm) = 
         let lab = generate_code_label()
         //if verbose then dprintf " --> generated code label %s with name %s\n" (string_of_code_label lab) nm;
         Mark lab

    member cgbuf.SetCodeLabelToPC(lab,pc) = 
        //if verbose then dprintf " --> setting label %s to pc %d\n" (string_of_code_label lab) pc;
        if codeLabels.ContainsKey(lab) then 
            warning(InternalError(sprintf "two values for given for label %s" (string_of_code_label lab),m));
        codeLabels.[lab] <- pc 

    member cgbuf.SetMark (Mark lab1,Mark lab2) = 
        let pc = 
           try codeLabels.[lab2]
           with Not_found -> 
               error(InternalError(sprintf "cgbuf.SetMark code label has no pc specified yet",m))
        cgbuf.SetCodeLabelToPC(lab1,pc)
        
    member cgbuf.SetMarkToHere (Mark lab) =  
        cgbuf.SetCodeLabelToPC(lab,codebuf.Count)

    member cgbuf.SetStack(s) = 
        stack <- s; 
        nstack <- List.length s

    member cgbuf.Mark(s) = 
      let res = cgbuf.EmitDelayMark(s)
      cgbuf.SetMarkToHere(res);
      res 

    member cgbuf.mgbuf = mgbuf
    member cgbuf.MethodName = methodName
    member cgbuf.PreallocatedArgCount = alreadyUsedArgs

    member cgbuf.AllocLocal(ranges,ty) = 
         let j = locals.Count
         locals.Add((ranges,ty));
         j 

    member cgbuf.ReallocLocal(cond,ranges,ty) = 
         let j = 
             match ResizeArray.tryfind_indexi cond locals with 
             | Some j -> 
                 let (prevRanges,prevType) = locals.[j]
                 locals.[j] <- ((ranges@prevRanges),ty);
                 j
             | None -> 
                 cgbuf.AllocLocal(ranges,ty)
         let j = j + alreadyUsedLocals
         j

    member cgbuf.Close() = 
        let instrs = ResizeArray.to_array codebuf 
        let instrs = 
            // If we omitted ANY sequence points, then promote the first sequence point to be the first instruction in the
            // method. A bit ugly but .NET debuggers only honour "step into" if the sequence point is the first in the method.
            //
            match seqpoint with 
            | Some(I_seqpoint sp as i) ->
                let i = 
                    if zapFirstSeqPointToStart then 
                        i
                    else
                        // This special dummy sequence point seems to be the magic to indicate that the head of the 
                        // method has no sequence point
                        I_seqpoint (ILSourceMarker.Create(document = sp.Document,
                                                        line = 0x00feefee,
                                                        column = 0,
                                                        endLine = 0x00feefee,
                                                        endColumn = 0))

                // Note we use physical equality '==' to compare the instruction objects. Nasty.
                instrs |> Array.mapi (fun idx i2 -> if idx = 0 then i else if i == i2 then i_nop else i2)
            | _ -> 
                instrs
        ResizeArray.to_list locals ,
        maxStack,
        codeLabels,
        instrs,
        ResizeArray.to_list exnSpecs,
        isSome seqpoint

module CG = 
    let EmitInstr (cgbuf:CodeGenBuffer) pps i = cgbuf.EmitInstr(pps,i)
    let EmitInstrs (cgbuf:CodeGenBuffer) pps is = cgbuf.EmitInstrs(pps,is)
    let EmitSeqPoint (cgbuf:CodeGenBuffer) src = cgbuf.EmitSeqPoint(src)
    let EmitDelayMark (cgbuf:CodeGenBuffer) nm = cgbuf.EmitDelayMark(nm)
    let SetMark (cgbuf:CodeGenBuffer) m1 m2 = cgbuf.SetMark(m1,m2)
    let SetMarkToHere (cgbuf:CodeGenBuffer) m1 =  cgbuf.SetMarkToHere(m1)
    let SetStack (cgbuf:CodeGenBuffer) s = cgbuf.SetStack(s)
    let GenerateMark (cgbuf:CodeGenBuffer) s = cgbuf.Mark(s)

open CG


//--------------------------------------------------------------------------
// Compile constants 
//-------------------------------------------------------------------------- 

let GenString cenv cgbuf m s = 
    CG.EmitInstrs cgbuf [Push cenv.g.ilg.typ_String] [ I_ldstr s ]

let GenConstArray cenv (cgbuf:CodeGenBuffer) eenv m ilElementType (data:'a[]) (write : Bytes.Bytebuf.t -> 'a -> unit) = 
    let buf = Bytes.Bytebuf.create data.Length
    data |> Array.iter (write buf);
    let bytes = Bytes.Bytebuf.close buf
    let ilArrayType = mk_sdarray_ty ilElementType
    let len = data.Length
    if data.Length = 0 then 
        CG.EmitInstrs cgbuf [Push cenv.g.ilg.typ_int32; Push ilArrayType; Pop] [ mk_ldc_i32 (0);  I_newarr (Rank1ArrayShape,ilElementType); ]
    else        
        let vtspec = cgbuf.mgbuf.GenerateRawDataValueType(eenv.cloc,bytes.Length)
        let fldName = mk_private_name ("field"^string(new_uniq()))
        let fty = Type_value vtspec
        let fieldDef = mk_static_fdef (fldName,fty, None, Some bytes, MemAccess_assembly)
        let fieldDef = { fieldDef with fdCustomAttrs = mk_custom_attrs [ mk_DebuggerBrowsableNeverAttribute cenv.g.ilg ] }
        let fspec = mk_fspec_in_boxed_tspec (TypeSpecForCompLoc eenv.cloc,fldName, fty)
        CountStaticFieldDef();
        cgbuf.mgbuf.AddFieldDef(fspec.EnclosingTypeRef,fieldDef); 
        CG.EmitInstrs cgbuf 
          [ Push cenv.g.ilg.typ_int32; Pop; Push ilArrayType; 
            Push ilArrayType; Push cenv.g.ilg.typ_RuntimeFieldHandle; 
            Pop; Pop]
          [ mk_ldc_i32 data.Length;
            I_newarr (Rank1ArrayShape,ilElementType); 
            i_dup;
            I_ldtoken (Token_field fspec); 
            mk_normal_call (mspec_RuntimeHelpers_InitializeArray cenv.g.ilg) ]


//--------------------------------------------------------------------------
// We normally generate in the context of a "what to do next" continuation
//-------------------------------------------------------------------------- 

type sequel = 
  | EndFilter (* integer says which local to save result in *)
  | LeaveHandler of (bool (* finally? *) * int * mark)  (* integer says which local to save result in *)
  | Br of mark
  | CmpThenBrOrContinue of pushpop list * ILInstr
  | Continue
  | DiscardThen of sequel
  | Return
  | EndLocalScope of sequel * mark (* used at end of 'let' and 'let rec' blocks to get tail recursive setting of end-of-scope marks *)
(*
  | DiscardAndBr of mark
  | discardAndReturnVoid
*)
  | ReturnVoid

let discard = DiscardThen Continue
let discardAndReturnVoid = DiscardThen ReturnVoid


//-------------------------------------------------------------------------
// This is the main code generation routine.  It is used to generate 
// the bodies of methods in a couple of places
//------------------------------------------------------------------------- 
 
let CodeGenThen cenv mgbuf (zapFirstSeqPointToStart,entryPointInfo,methodName,eenv,alreadyUsedArgs,alreadyUsedLocals,codeGenFunction,m) = 
    let cgbuf = new CodeGenBuffer(m,mgbuf,methodName,entryPointInfo,alreadyUsedArgs,alreadyUsedLocals,zapFirstSeqPointToStart)
    let start = CG.GenerateMark cgbuf "mstart"
    let innerVals = entryPointInfo |> List.map (fun (v,kind) -> (v,(kind,start))) 

    (* Call the given code generator *)
    codeGenFunction cgbuf {eenv with withinSEH=false;
                                     liveLocals=Imap.empty();  
                                     innerVals = innerVals};

    let finish = CG.GenerateMark cgbuf "mfinish"
    
    let locals,maxStack,codeLabels,code,exnSpecs,hasSequencePoints = cgbuf.Close()
    
    let localDebugSpecs = 
      locals
      |> List.mapi (fun i (nms,_) -> List.map (fun nm -> (i,nm)) nms)
      |> List.concat
      |> List.map (fun (i,(nm,(start,finish))) -> 
          { locRange=(code_label_of_mark start, code_label_of_mark finish);
            locInfos= [{ localNum=i; localName=nm }] })

    if debug && List.length locals > 64 then dprintn ("Note, method "^methodName^" has "^string (List.length locals)^" locals (even before conversion from ILX to IL)."); 

    (List.map (snd >> IL.mk_local) locals, 
     maxStack,
     codeLabels,
     code,
     exnSpecs,
     localDebugSpecs,
     hasSequencePoints)

let CodeGenMethod cenv mgbuf (zapFirstSeqPointToStart,entryPointInfo,methodName,eenv,alreadyUsedArgs,alreadyUsedLocals,codeGenFunction,m) = 
    (* Codegen the method. REVIEW: change this to generate the AbsIL code tree directly... *)
    if verbose then dprintf "----------\ncodegen method %s\n" methodName;

    let locals,maxStack,codeLabels,instrs,exns,localDebugSpecs,hasSequencePoints = 
      CodeGenThen cenv mgbuf (zapFirstSeqPointToStart,entryPointInfo,methodName,eenv,alreadyUsedArgs,alreadyUsedLocals,codeGenFunction,m)

    let dump() = 
       instrs |> Array.iteri (fun i instr -> dprintf "%s: %d: %A\n" methodName i instr);
   
    if verbose then dump();

    let code = 
        // Build an Abstract IL code tree from the raw information 
        let lab2pc lbl = 
            try Hashtbl.find codeLabels lbl 
            with Not_found -> errorR(Error("label "^string_of_code_label lbl^" not found",m)); dump(); 676767
        
        build_code methodName lab2pc instrs exns localDebugSpecs
    
    let code = check_code code

    // Attach a source range to the method. Only do this is it has some sequence points, because .NET 2.0/3.5 
    // ILDASM has issues if you emit symbols with a source range but without any sequence points
    let sourceRange = if hasSequencePoints then GenPossibleILSourceMarker cenv m else None

    // Build an Abstract IL method     
    mk_ilmbody (true,locals,maxStack,code, sourceRange)

let StartDelayedLocalScope nm cgbuf =
    let startScope = CG.EmitDelayMark cgbuf ("start_"^nm) 
    let endScope = CG.EmitDelayMark cgbuf ("end_"^nm)
    startScope,endScope

let StartLocalScope nm cgbuf =
    let startScope = CG.GenerateMark cgbuf ("start_"^nm) 
    let endScope = CG.EmitDelayMark cgbuf ("end_"^nm)
    startScope,endScope
  
let LocalScope nm cgbuf (f : (mark * mark) -> 'a) : 'a =
    let startScope,endScope as scopeMarks = StartLocalScope nm cgbuf
    let res = f scopeMarks
    CG.SetMarkToHere cgbuf endScope;
    res

let compileSequenceExpressions = true // try (System.Environment.GetEnvironmentVariable("COMPILED_SEQ") <> null) with _ -> false

//-------------------------------------------------------------------------
// Generate expressions
//------------------------------------------------------------------------- 

let bindHasSeqPt = function (TBind(_,_,SequencePointAtBinding _)) -> true | _ -> false
let bindIsInvisible = function (TBind(_,_,NoSequencePointAtInvisibleBinding _)) -> true | _ -> false

let AlwaysSuppressSequencePoint sp expr = 
  match sp with 
  | SPAlways -> 
      // These extra cases have historically always had their sequence point suppressed 
      // However they do not fall under the definition of 'DoesExprCodeGenDefinitelyStartWithSequencePoint = false'
      match expr with 
      | TExpr_let (bind,_,_,_) when bindIsInvisible(bind) -> true
      | TExpr_letrec(binds,_,_,_) when (binds |> FlatList.exists bindHasSeqPt) || (binds |> FlatList.forall bindIsInvisible) -> true
      | TExpr_seq _ 
      | TExpr_match _  -> true
      | TExpr_op((TOp_label _ | TOp_goto _),_,_,_) -> true
      | _ -> false
  | SPSuppress -> 
      true
  
// This is the list of composite statement expressions where we're about to emit a sequence
// point for sure. They get sequence points on their sub-expressions
//
// Determine if expression code generation certainly starts with a sequence point. An approximation used
// to prevent the generation of duplicat sequence points for conditionals and pattern matching
let rec WillGenerateSequencePoint sp expr = 
  match sp with 
  | SPAlways -> 
      let definiteSequencePoint = 
          match expr with 
          | TExpr_let (bind,expr,_,_) 
               -> bindHasSeqPt(bind)  || 
                  (bind.Var.IsCompiledAsTopLevel && WillGenerateSequencePoint sp expr)
          | TExpr_letrec(binds,expr,_,_) 
               -> (binds |> FlatList.forall (fun bind -> bind.Var.IsCompiledAsTopLevel)) && WillGenerateSequencePoint sp expr
               
          | TExpr_seq (_, _, NormalSeq,spSeq,_) -> 
            (match spSeq with 
             | SequencePointsAtSeq -> true
             | SuppressSequencePointOnExprOfSequential -> true
             | SuppressSequencePointOnStmtOfSequential -> false)
          | TExpr_match (SequencePointAtBinding _,_,_,_,_,_,_)   -> true
          | TExpr_op((  TOp_try_catch (SequencePointAtTry _,_) 
                      | TOp_try_finally (SequencePointAtTry _,_) 
                      | TOp_for (SequencePointAtForLoop _,_) 
                      | TOp_while (SequencePointAtWhileLoop _)),_,_,_) -> true
          | _ -> false
      definiteSequencePoint 

   | SPSuppress -> 
      false                  

let DoesGenExprStartWithSequencePoint sp expr = 
    WillGenerateSequencePoint sp expr || not (AlwaysSuppressSequencePoint sp expr)

let rec GenExpr cenv (cgbuf:CodeGenBuffer) eenv sp expr sequel =
  if verbose then dprintf "GenExpr@%a, #stack = %A, sequel = %s\n" output_range (range_of_expr expr) (cgbuf.GetCurrentStack()) (StringOfSequel sequel);
  (* if verbose then dprintf "GenExpr@%a, #stack = %d, expr = %s, sequel = %s\n" output_range (range_of_expr expr) (List.length cgbuf.stack) (showL (ExprL expr)) (StringOfSequel sequel); *)
  let expr =  strip_expr expr

  if not (WillGenerateSequencePoint sp expr) && not (AlwaysSuppressSequencePoint sp expr) then 
      CG.EmitSeqPoint cgbuf (range_of_expr expr);

  match (if compileSequenceExpressions then Lowertop.LowerSeqExpr cenv.g cenv.amap expr else None) with
  | Some info ->
      GenSequenceExpr cenv cgbuf eenv info sequel
  | None ->

  match expr with 
  | TExpr_const(c,m,ty) -> 
      GenConstant cenv cgbuf eenv (c,m,ty) sequel
  | TExpr_match (spBind,exprm,tree,targets,m,ty,_) -> 
      GenMatch cenv cgbuf eenv (spBind,exprm,tree,targets,m,ty) sequel
  | TExpr_seq(e1,e2,dir,spSeq,m) ->  
      GenSequential cenv cgbuf eenv sp (e1,e2,dir,spSeq,m) sequel
  | TExpr_letrec (binds,body,m,_)  -> 
      GenLetRec cenv cgbuf eenv (binds,body,m) sequel
  | TExpr_let (bind,body,m,_)  -> 
     (* This case implemented here to get a guaranteed tailcall *)
     // Make sure we generate the sequence point outside the scope of the variable
     let startScope,endScope as scopeMarks = StartDelayedLocalScope "let" cgbuf
     let eenv = AllocStorageForBind cenv cgbuf scopeMarks eenv bind
     let sp = GenSequencePointForBind cenv cgbuf eenv bind
     CG.SetMarkToHere cgbuf startScope; 
     GenBindAfterSequencePoint cenv cgbuf eenv sp bind;

     let sp = if bindHasSeqPt bind || bindIsInvisible bind then SPAlways else SPSuppress 
     GenExpr cenv cgbuf eenv sp body (EndLocalScope(sequel,endScope)) 

  | TExpr_lambda _  | TExpr_tlambda _  -> 
      GenLambda cenv cgbuf eenv false None expr sequel
  | TExpr_app(f,fty,tyargs,args,m) -> 
      GenApp cenv cgbuf eenv (f,fty,tyargs,args,m) sequel
  | TExpr_val(v,flags,m) -> 
      GenGetVal cenv cgbuf eenv (v,m) sequel
  | TExpr_op(op,tyargs,args,m) -> 
      begin match op,args,tyargs with 
      | TOp_exnconstr(c),_,_      -> 
          GenAllocExn cenv cgbuf eenv (c,args,m) sequel
      | TOp_ucase(c),_,_        -> 
          GenAllocUnionCase cenv cgbuf eenv (c,tyargs,args,m) sequel
      | TOp_recd(isCtor,tycon),_,_ -> 
          GenAllocRecd cenv cgbuf eenv isCtor (tycon,tyargs,args,m) sequel
      | TOp_tuple_field_get(n),[e],_ -> 
          GenGetTupleField cenv cgbuf eenv (e,tyargs,n,m) sequel
      | TOp_exnconstr_field_get(constr,n),[e],_ -> 
          GenGetExnField cenv cgbuf eenv (e,constr,n,m) sequel
      | TOp_ucase_field_get(constr,n),[e],_ -> 
          GenGetUnionCaseField cenv cgbuf eenv (e,constr,tyargs,n,m) sequel
      | TOp_ucase_tag_get(constr),[e],_ -> 
          GenGetUnionCaseTag cenv cgbuf eenv (e,constr,tyargs,m) sequel
      | TOp_ucase_proof(constr),[e],_ -> 
          GenUnionCaseProof cenv cgbuf eenv (e,constr,tyargs,m) sequel
      | TOp_exnconstr_field_set(constr,n),[e;e2],_ -> 
          GenSetExnField cenv cgbuf eenv (e,constr,n,e2,m) sequel 
      | TOp_ucase_field_set(constr,n),[e;e2],_ -> 
          GenSetUnionCaseField cenv cgbuf eenv (e,constr,tyargs,n,e2,m) sequel
      | TOp_rfield_get(f),[e],_ -> 
         GenGetRecdField cenv cgbuf eenv (e,f,tyargs,m) sequel
      | TOp_rfield_get(f),[],_ -> 
         GenGetStaticField cenv cgbuf eenv (f,tyargs,m) sequel
      | TOp_field_get_addr(f),[e],_ -> 
         GenGetRecdFieldAddr cenv cgbuf eenv (e,f,tyargs,m) sequel
      | TOp_field_get_addr(f),[],_ -> 
         GenGetStaticFieldAddr cenv cgbuf eenv (f,tyargs,m) sequel
      | TOp_rfield_set(f),[e1;e2],_ -> 
         GenSetRecdField cenv cgbuf eenv (e1,f,tyargs,e2,m) sequel
      | TOp_rfield_set(f),[e2],_ -> 
         GenSetStaticField cenv cgbuf eenv (f,tyargs,e2,m) sequel
      | TOp_tuple,_,_ -> 
         GenAllocTuple cenv cgbuf eenv (args,tyargs,m) sequel
      | TOp_asm(code,rtys),_,_ ->  
         GenAsmCode cenv cgbuf eenv (code,tyargs,args,rtys,m) sequel 
      | TOp_while sp,[TExpr_lambda(_,_,[_],e1,_,_,_);TExpr_lambda(_,_,[_],e2,_,_,_)],[]  -> 
         GenWhileLoop cenv cgbuf eenv (sp,e1,e2,m) sequel 
      | TOp_for(spStart,dir),[TExpr_lambda(_,_,[_],e1,_,_,_);TExpr_lambda(_,_,[_],e2,_,_,_);TExpr_lambda(_,_,[v],e3,_,_,_)],[]  -> 
         GenForLoop cenv cgbuf eenv (spStart,v,e1,dir,e2,e3,m) sequel
      | TOp_try_finally(spTry,spFinally),[TExpr_lambda(_,_,[_],e1,_,_,_); TExpr_lambda(_,_,[_],e2,_,_,_)],[resty] -> 
         GenTryFinally cenv cgbuf eenv (e1,e2,m,resty,spTry,spFinally) sequel
      | TOp_try_catch(spTry,spWith),[TExpr_lambda(_,_,[_],e1,_,_,_); TExpr_lambda(_,_,[vf],ef,_,_,_);TExpr_lambda(_,_,[vh],eh,_,_,_)],[resty] -> 
         GenTryCatch cenv cgbuf eenv (e1,vf,ef,vh,eh,m,resty,spTry,spWith) sequel
      | TOp_ilcall(meth,enclTypeArgs,methTypeArgs,rtys),args,[] -> 
         GenIlCall cenv cgbuf eenv (meth,enclTypeArgs,methTypeArgs,args,rtys,m) sequel
      | TOp_get_ref_lval,[e],[ty]       -> GenGetAddrOfRefCellField cenv cgbuf eenv (e,ty,m) sequel
      | TOp_coerce,[e],[tgty;srcty]    -> GenCoerce cenv cgbuf eenv (e,tgty,m,srcty) sequel
      | TOp_rethrow,[],[rtnty]         -> GenRethrow cenv cgbuf eenv (rtnty,m) sequel
      | TOp_trait_call(ss),args,[] -> GenTraitCall cenv cgbuf eenv (ss,args, m) expr sequel
      | TOp_lval_op(LSet,v),[e],[]      -> GenSetVal cenv cgbuf eenv (v,e,m) sequel
      | TOp_lval_op(LByrefGet,v),[],[]  -> GenGetByref cenv cgbuf eenv (v,m) sequel
      | TOp_lval_op(LByrefSet,v),[e],[] -> GenSetByref cenv cgbuf eenv (v,e,m) sequel
      | TOp_lval_op(LGetAddr,v),[],[]   -> GenGetValAddr cenv cgbuf eenv (v,m) sequel
      | TOp_array,elems,[argty] ->  GenNewArray cenv cgbuf eenv (elems,argty,m) sequel
      | TOp_bytes bytes,[],[] -> 
          if cenv.emitConstantArraysUsingStaticDataBlobs then 
              GenConstArray cenv cgbuf eenv m cenv.g.ilg.typ_uint8 bytes Bytes.Bytebuf.emit_byte;
              GenSequel cenv eenv.cloc cgbuf sequel
          else
              GenNewArraySimple cenv cgbuf eenv (List.of_array (Array.map (mk_byte cenv.g m) bytes),cenv.g.byte_ty,m) sequel
      | TOp_uint16s arr,[],[] -> 
          if cenv.emitConstantArraysUsingStaticDataBlobs then 
              GenConstArray cenv cgbuf eenv m cenv.g.ilg.typ_uint16 arr Bytes.Bytebuf.emit_u16;
              GenSequel cenv eenv.cloc cgbuf sequel
          else
              GenNewArraySimple cenv cgbuf eenv (List.of_array (Array.map (mk_uint16 cenv.g m) arr),cenv.g.uint16_ty,m) sequel
      | TOp_goto(label),_,_ ->  
         CG.EmitInstr cgbuf [] (I_br label);
         // NOTE: discard sequel
      | TOp_return,[e],_ ->  
         GenExpr cenv cgbuf eenv SPSuppress e Return
         // NOTE: discard sequel
      | TOp_return,[],_ ->  
         GenSequel cenv eenv.cloc cgbuf ReturnVoid
         // NOTE: discard sequel
      | TOp_label(label),_,_ ->  
         cgbuf.SetMarkToHere (Mark label) 
         GenUnitThenSequel cenv eenv.cloc cgbuf sequel
      | _ -> error(InternalError("Unexpected operator node expression",range_of_expr expr))
     end 
  | TExpr_static_optimization(constraints,e2,e3,m) -> 
      GenStaticOptimization cenv cgbuf eenv (constraints,e2,e3,m) sequel
  | TExpr_obj(uniq,typ,_,_,[meth],[],m,_) when is_delegate_typ cenv.g typ -> 
      GenDelegateExpr cenv cgbuf eenv expr (meth,m) sequel
  | TExpr_obj(uniq,typ,basev,basecall,overrides,interfaceImpls,m,_) -> 
      GenObjectExpr cenv cgbuf eenv expr (typ,basev,basecall,overrides,interfaceImpls,m)  sequel

  | TExpr_quote(ast,conv,m,ty) -> GenQuotation cenv cgbuf eenv (ast,conv,m,ty) sequel
  | TExpr_link _ -> failwith "Unexpected reclink"
  | TExpr_tchoose (_,_,m) -> error(InternalError("Unexpected TExpr_tchoose",m))

and GenExprs cenv cgbuf eenv es = List.iter (fun e -> GenExpr cenv cgbuf eenv SPSuppress e Continue) es

and CodeGenMethodForExpr cenv mgbuf (spReq,entryPointInfo,methodName,eenv,alreadyUsedArgs,alreadyUsedLocals,expr0,sequel0) = 
  let zapFirstSeqPointToStart = (spReq = SPAlways)
  CodeGenMethod cenv mgbuf (zapFirstSeqPointToStart,entryPointInfo,methodName,eenv,alreadyUsedArgs,alreadyUsedLocals,
                             (fun cgbuf eenv -> GenExpr cenv cgbuf eenv spReq expr0 sequel0),
                             (range_of_expr expr0))



//--------------------------------------------------------------------------
// Generate sequels
//-------------------------------------------------------------------------- 

(* does the sequel discard its result, and if so what does it do next? *)
and sequelAfterDiscard sequel = 
  match sequel with 
   | DiscardThen sequel -> Some(sequel)
   | EndLocalScope(sq,mark) -> sequelAfterDiscard sq |> Option.map (fun sq -> EndLocalScope(sq,mark))
   | _ -> None

and sequel_ignoring_end_scopes_and_discard sequel =
    let sequel = sequel_ignore_end_scopes sequel
    match sequelAfterDiscard sequel with 
    | Some sq -> sq
    | None ->  sequel 

and sequel_ignore_end_scopes  sequel = 
    match sequel with 
    | EndLocalScope(sq,m) -> sequel_ignore_end_scopes sq
    | sq -> sq

(* commit any 'EndLocalScope' nodes in the sequel and return the residue *)
and GenSequelEndScopes cgbuf sequel =
    match sequel with 
    | EndLocalScope(sq,m) -> CG.SetMarkToHere cgbuf m; GenSequelEndScopes cgbuf sq
    | sq -> ()

and StringOfSequel sequel =
    match sequel with
    | Continue -> "continue"
    | DiscardThen sequel -> "discard; "^StringOfSequel sequel
    | ReturnVoid -> "ReturnVoid"
    | CmpThenBrOrContinue(pushpops,bri) -> "CmpThenBrOrContinue"
    | Return -> "Return"
    | EndLocalScope (sq,Mark k) -> "EndLocalScope("^StringOfSequel sq^","^string_of_code_label k^")"
    | Br (Mark x) -> sprintf "Br L%s" (string_of_code_label x)
    | LeaveHandler _ -> "LeaveHandler"
    | EndFilter -> "EndFilter"

and GenSequel cenv cloc cgbuf sequel =
  let sq = sequel_ignore_end_scopes sequel
  if verbose then dprintn ("GenSequel:" ^ StringOfSequel sequel);
  (match sq with 
  | Continue -> ()
  | DiscardThen sq -> 
      CG.EmitInstr cgbuf [Pop] (i_pop);
      GenSequel cenv cloc cgbuf sq 
  | ReturnVoid ->
      CG.EmitInstr cgbuf [] I_ret 
  | CmpThenBrOrContinue(pushpops,bri) ->
      CG.EmitInstr cgbuf pushpops bri
  | Return -> 
      CG.EmitInstr cgbuf [Pop] I_ret 
  | EndLocalScope _ -> failwith "EndLocalScope unexpected"
  | Br x -> 
      // Emit a NOP in debug code in case the branch instruction gets eliminated 
      // because it is a "branch to next instruction". This prevents two unrelated sequence points
      // (the one before the branch and the one after) being coalesced together
      if cgbuf.mgbuf.cenv.debug then 
         CG.EmitInstr cgbuf [] i_nop
      CG.EmitInstr cgbuf [] (I_br(code_label_of_mark x))  
  | LeaveHandler (isFinally, whereToSaveResult,x) ->
      if isFinally then 
        CG.EmitInstr cgbuf [Pop] (i_pop) 
      else
        EmitSetLocal cgbuf whereToSaveResult;
      CG.EmitInstr cgbuf [] (if isFinally then I_endfinally else I_leave(code_label_of_mark x))
  | EndFilter ->
      CG.EmitInstr cgbuf [Pop] I_endfilter
  );
  GenSequelEndScopes cgbuf sequel;
  if verbose then dprintn ("GenSequel: done");


//--------------------------------------------------------------------------
// Generate constants
//-------------------------------------------------------------------------- 

and GenConstant cenv cgbuf eenv (c,m,ty) sequel =
  let il_ty = GenType m cenv.g eenv.tyenv ty
  (* Check if we need to generate the value at all! *)
  begin match sequelAfterDiscard sequel with 
  | None -> 
    if verbose then dprintn ("GenConstant: generating ");
    begin match TryEliminateDesugaredConstants cenv.g m c with 
    | Some e -> GenExpr cenv cgbuf eenv SPSuppress e Continue
    | None ->
        match c with 
        | TConst_bool b -> CG.EmitInstr cgbuf [Push cenv.g.ilg.typ_bool] (mk_ldc_i32 (if b then 1 else 0))

        | TConst_sbyte(i) -> CG.EmitInstr cgbuf [Push il_ty] (mk_ldc_i32 (int32 i))
        | TConst_int16(i) -> CG.EmitInstr cgbuf [Push il_ty] (mk_ldc_i32 (int32 i))
        | TConst_int32(i) -> CG.EmitInstr cgbuf [Push il_ty] (mk_ldc_i32 i)
        | TConst_int64(i) -> CG.EmitInstr cgbuf [Push il_ty] (mk_ldc_i64 i)
        | TConst_nativeint(i) -> CG.EmitInstrs cgbuf [Push il_ty] [mk_ldc_i64 i; I_arith (AI_conv DT_I) ]
        | TConst_byte(i) -> CG.EmitInstr cgbuf [Push il_ty] (mk_ldc_i32 (int32 i))
        | TConst_uint16(i) -> CG.EmitInstr cgbuf [Push il_ty] (mk_ldc_i32 (int32 i))
        | TConst_uint32(i) -> CG.EmitInstr cgbuf [Push il_ty] (mk_ldc_i32 (int32 i))
        | TConst_uint64(i) -> CG.EmitInstr cgbuf [Push il_ty] (mk_ldc_i64 (int64 i))
        | TConst_unativeint(i) -> CG.EmitInstrs cgbuf [Push il_ty] [mk_ldc_i64 (int64 i); I_arith (AI_conv DT_U) ]
        | TConst_float(f) -> CG.EmitInstr cgbuf [Push il_ty] ( I_arith (AI_ldc (DT_R8,NUM_R8 f)) )
        | TConst_float32(f) -> CG.EmitInstr cgbuf [Push il_ty] ( I_arith (AI_ldc (DT_R4,NUM_R4 f)) )
        | TConst_char(c) -> CG.EmitInstr cgbuf [Push il_ty] ( mk_ldc_i32 (int c))
        | TConst_string(s) -> GenString cenv cgbuf m s
        | TConst_unit -> GenUnit cenv cgbuf
        | TConst_zero -> GenDefaultValue cenv cgbuf eenv (ty,m) 
        | TConst_decimal _ -> failwith "unreachable"
    end;
    GenSequel cenv eenv.cloc cgbuf sequel
  | Some sq -> 
    if verbose then dprintn ("GenConstant: skipping");
  (* Even if we didn't need to generate the value then maybe we still have to branch or return *)
    GenSequel cenv eenv.cloc cgbuf sq
  end

and GenUnit cenv cgbuf  = CG.EmitInstr cgbuf [Push cenv.g.ilg.typ_Object] i_ldnull

and GenUnitThenSequel cenv cloc cgbuf sequel =
  if verbose then dprintn ("GenUnitThenSequel:");  
  match sequelAfterDiscard sequel with 
  | Some(sq) -> GenSequel cenv cloc cgbuf sq
  | None -> GenUnit cenv cgbuf; GenSequel cenv cloc cgbuf sequel


//--------------------------------------------------------------------------
// Generate simple data-related constructs
//-------------------------------------------------------------------------- 

and GenAllocTuple cenv cgbuf eenv (args,argtys,m) sequel =
  if verbose then dprintn ("GenAllocTuple:");
  let tcref, tys, args, newm = compiled_mk_tuple cenv.g (argtys,args,m)
  let typ = GenNamedTyApp newm cenv.g eenv.tyenv tcref tys
  let ntyvars = if (tys.Length - 1) < goodTupleFields then (tys.Length - 1) else goodTupleFields
  let tyvars = [0 .. ntyvars] |> List.map (fun n -> mk_tyvar_ty (uint16 n))
  if verbose then dprintf "GenAllocTuple: #args = %d\n" (List.length args);  
  GenExprs cenv cgbuf eenv args;
  (* generate a reference to the constructor *)
  let tyenvinner = tyenv_for_tcref tcref
  if verbose then dprintf "GenAllocTuple: call, #args = %d" (List.length args);  
  CG.EmitInstr cgbuf (List.replicate args.Length Pop @ [ Push typ; ])
    (mk_normal_newobj 
        (mk_ctor_mspec_for_typ (typ,tyvars)));
  GenSequel cenv eenv.cloc cgbuf sequel


and GenGetTupleField cenv cgbuf eenv (e,tys,n,m) sequel =
    let rec compiled_get_system_tuple_item g (e,tys,n,m) =
        let ar = List.length tys
        if ar <= 0 then failwith "compiled_get_tuple_item"
        elif ar < maxTuple then
            let tcr' = compiled_tuple_tcref g tys
            let typ = GenNamedTyApp m g eenv.tyenv tcr' tys
            mk_call_Tuple_ItemN g m n typ e tys.[n]
        else
            let tysA,tysB = split_after (goodTupleFields) tys
            let tyB = compiled_tuple_ty g tysB
            let tys' = tysA@[tyB]
            let tcr' = compiled_tuple_tcref g tys'
            let typ' = GenNamedTyApp m g eenv.tyenv tcr' tys'
            let n' = (min n goodTupleFields)
            let elast = mk_call_Tuple_ItemN g m n' typ' e tys'.[n']
            if n < goodTupleFields then
                elast
            else
                compiled_get_system_tuple_item g (elast,tysB,n-goodTupleFields,m)
    GenExpr cenv cgbuf eenv SPSuppress (compiled_get_system_tuple_item cenv.g (e,tys,n,m)) sequel


and GenAllocExn cenv cgbuf eenv (c,args,m) sequel =
  GenExprs cenv cgbuf eenv args;
  let typ = GenExnType m cenv.g eenv.tyenv c
  let flds = rfields_of_ecref c
  let argtys = flds |> List.map (fun rfld -> GenType m cenv.g eenv.tyenv rfld.FormalType) 
  let mspec = mk_ctor_mspec (typ.TypeRef, AsObject,argtys,[])
  CG.EmitInstr cgbuf
    (List.replicate args.Length Pop @ [ Push typ])
    (mk_normal_newobj mspec) ;
  GenSequel cenv eenv.cloc cgbuf sequel

and GenAllocUnionCase cenv cgbuf eenv  (c,tyargs,args,m) sequel =
  if verbose then dprintn ("GenAllocUnionCase");  
  GenExprs cenv cgbuf eenv args;
  let cuspec,idx = GenUnionCaseSpec m cenv.g eenv.tyenv c tyargs
  CG.EmitInstr cgbuf (List.replicate args.Length Pop @ [ Push (objtype_of_cuspec cuspec)]) (mk_IlxInstr (EI_newdata (cuspec,idx)));
  GenSequel cenv eenv.cloc cgbuf sequel

and GenAllocRecd cenv cgbuf eenv ctorInfo (tcref,argtys,args,m) sequel =
  let typ = GenNamedTyApp m cenv.g eenv.tyenv tcref argtys

  (* Filter out fields with default initialization *)
  let relevantFields = 
      tcref.AllInstanceFieldsAsList
      |> List.filter (fun f -> not f.IsZeroInit)

  match ctorInfo with 
  | RecdExprIsObjInit  -> 
      if verbose then dprintn ("GenAllocRecd: class constructor");  
      (args,relevantFields) ||> List.iter2 (fun e f -> 
              CG.EmitInstr cgbuf [Push typ] ldarg_0; 
              GenExpr cenv cgbuf eenv SPSuppress e Continue;
              GenFieldStore false cenv cgbuf eenv (mk_rfref tcref f.Name,argtys,m) discard) 
      (* Object construction doesn't generate a true value. *)
      (* Object constructions will always just get thrown away so this is safe *)      
      GenSequel cenv eenv.cloc cgbuf sequel
  | RecdExpr -> 
      if verbose then dprintf "GenAllocRecd: normal record, #args = %d\n" (List.length args);  
      GenExprs cenv cgbuf eenv args;
          (* generate a reference to the record constructor *)
      let tyenvinner = tyenv_for_tcref tcref
      if verbose then dprintf "GenAllocRecd: call, #args = %d" (List.length args);  
      CG.EmitInstr cgbuf (List.replicate args.Length Pop @ [ Push typ; ])
        (mk_normal_newobj 
           (mk_ctor_mspec_for_typ (typ,relevantFields |> List.map (fun f -> GenType m cenv.g tyenvinner f.FormalType) )));
      GenSequel cenv eenv.cloc cgbuf sequel


and GenNewArraySimple cenv cgbuf eenv (elems,argty,m) sequel =
  let argty' = GenType m cenv.g eenv.tyenv argty
  let arrty = mk_array_ty_old (Rank1ArrayShape,argty')
  
  CG.EmitInstrs cgbuf [Push arrty] [ I_arith (AI_ldc (DT_I4,NUM_I4 ((List.length elems)))); I_newarr (Rank1ArrayShape,argty') ];
  List.iteri
    (fun i e ->             
      CG.EmitInstrs cgbuf [Push arrty; Push cenv.g.ilg.typ_int32] [ i_dup; I_arith (AI_ldc (DT_I4,NUM_I4 (i))) ];
      GenExpr cenv cgbuf eenv SPSuppress e Continue;          
      CG.EmitInstr cgbuf [Pop; Pop; Pop] (I_stelem_any (Rank1ArrayShape,argty'))) 
    elems;
  GenSequel cenv eenv.cloc cgbuf sequel

and GenNewArray cenv cgbuf eenv (elems: expr list,argty,m) sequel =
  if elems.Length <= 5 || not cenv.emitConstantArraysUsingStaticDataBlobs  then 
      GenNewArraySimple cenv cgbuf eenv (elems,argty,m) sequel 
  else
      (* Try to emit a constant byte-blob array *)
      let elems' = Array.of_list elems
      let test,write =  
          match elems'.[0] with 
          | TExpr_const(TConst_bool  _,_,_) -> (function TConst_bool  _ -> true | _ -> false), (fun buf -> function TConst_bool  b -> Bytes.Bytebuf.emit_bool_as_byte buf b | _ -> failwith "unreachable")
          | TExpr_const(TConst_char  _,_,_) -> (function TConst_char  _ -> true | _ -> false), (fun buf -> function TConst_char  b -> Bytes.Bytebuf.emit_i32_as_u16 buf (int b) | _ -> failwith "unreachable")
          | TExpr_const(TConst_byte _,_,_) -> (function TConst_byte _ -> true | _ -> false), (fun buf -> function TConst_byte b -> Bytes.Bytebuf.emit_byte buf b | _ -> failwith "unreachable")
          | TExpr_const(TConst_uint16 _,_,_) -> (function TConst_uint16 _ -> true | _ -> false), (fun buf -> function TConst_uint16 b -> Bytes.Bytebuf.emit_u16 buf b | _ -> failwith "unreachable")
          | TExpr_const(TConst_uint32 _,_,_) -> (function TConst_uint32 _ -> true | _ -> false), (fun buf -> function TConst_uint32 b -> Bytes.Bytebuf.emit_i32 buf (int32 b) | _ -> failwith "unreachable")
          | TExpr_const(TConst_uint64 _,_,_) -> (function TConst_uint64 _ -> true | _ -> false), (fun buf -> function TConst_uint64 b -> Bytes.Bytebuf.emit_i64 buf (int64 b) | _ -> failwith "unreachable")
          | TExpr_const(TConst_sbyte _,_,_) -> (function TConst_sbyte _ -> true | _ -> false), (fun buf -> function TConst_sbyte b -> Bytes.Bytebuf.emit_byte buf (byte b) | _ -> failwith "unreachable")
          | TExpr_const(TConst_int16 _,_,_) -> (function TConst_int16 _ -> true | _ -> false), (fun buf -> function TConst_int16 b -> Bytes.Bytebuf.emit_u16 buf (uint16 b) | _ -> failwith "unreachable")
          | TExpr_const(TConst_int32 _,_,_) -> (function TConst_int32 _ -> true | _ -> false), (fun buf -> function TConst_int32 b -> Bytes.Bytebuf.emit_i32 buf b | _ -> failwith "unreachable")
          | TExpr_const(TConst_int64 _,_,_) -> (function TConst_int64 _ -> true | _ -> false), (fun buf -> function TConst_int64 b -> Bytes.Bytebuf.emit_i64 buf b | _ -> failwith "unreachable")
          
          | _ -> (function _ -> false), (fun _ _ -> failwith "unreachable")
      if elems' |> Array.forall (function TExpr_const(c,_,_) -> test c | _ -> false) then
           let argty' = GenType m cenv.g eenv.tyenv argty
           GenConstArray cenv cgbuf eenv m argty' elems' (fun buf -> function TExpr_const(c,_,_) -> write buf c | _ -> failwith "unreachable");
           GenSequel cenv eenv.cloc cgbuf sequel

      else
           GenNewArraySimple cenv cgbuf eenv (elems,argty,m) sequel 

and GenCoerce cenv cgbuf eenv (e,tgty,m,srcty) sequel = 
  (* Is this an upcast? *)
  if Typrelns.type_definitely_subsumes_type_no_coercion 0 cenv.g cenv.amap m tgty srcty &&
     (* Do an extra check - should not be needed *)
     Typrelns.type_feasibly_subsumes_type 0 cenv.g cenv.amap m tgty Typrelns.NoCoerce srcty then
     begin 
       (* The .NET IL doesn't always support implict subsumption for interface types, e.g. at stack merge points *)
       (* Hence be conservative here and always cast explicitly. *)
       if (is_interface_typ cenv.g tgty) then (
           GenExpr cenv cgbuf eenv SPSuppress e Continue;
           let ilToTy = GenType m cenv.g eenv.tyenv tgty
           CG.EmitInstrs cgbuf [Pop; Push ilToTy] [ I_unbox_any ilToTy;  ];
           GenSequel cenv eenv.cloc cgbuf sequel
       ) else (
           GenExpr cenv cgbuf eenv SPSuppress e sequel;
       )
     end       
  else  
    GenExpr cenv cgbuf eenv SPSuppress e Continue;          
    if not (is_obj_typ cenv.g srcty) then 
       let ilFromTy = GenType m cenv.g eenv.tyenv srcty
       CG.EmitInstrs cgbuf [Pop; Push cenv.g.ilg.typ_Object] [ I_box ilFromTy;  ];
    if not (is_obj_typ cenv.g tgty) then 
        let ilToTy = GenType m cenv.g eenv.tyenv tgty
        CG.EmitInstrs cgbuf [Pop; Push ilToTy] [ I_unbox_any ilToTy;  ];
    GenSequel cenv eenv.cloc cgbuf sequel

and GenRethrow cenv cgbuf eenv (rtnty,m) sequel =     
    let ilReturnTy = GenType m cenv.g eenv.tyenv rtnty
    CG.EmitInstrs cgbuf [] [I_rethrow];
    // [See comment related to I_throw].
    // Rethrow does not return. Required to push dummy value on the stack.
    // This follows prior behaviour by prim-types rethrow<_>.
    CG.EmitInstrs cgbuf [Push ilReturnTy]  [i_ldnull;  I_unbox_any ilReturnTy ];
    GenSequel cenv eenv.cloc cgbuf sequel

and GenGetExnField cenv cgbuf eenv (e,ecref,fieldNum,m) sequel =
    GenExpr cenv cgbuf eenv SPSuppress e Continue;
    let exnc = strip_eqns_from_ecref ecref
    let typ = GenExnType m cenv.g eenv.tyenv ecref
    CG.EmitInstrs cgbuf [] [ I_castclass typ];

    let fld = List.nth (exnc.TrueInstanceFieldsAsList) fieldNum
    let ftyp = GenType m cenv.g eenv.tyenv fld.FormalType

    let mspec = mk_nongeneric_instance_mspec_in_typ (typ,"get_"^fld.Name, [], ftyp)
    CG.EmitInstr cgbuf ([Pop;Push ftyp]) (mk_normal_call mspec)

    GenSequel cenv eenv.cloc cgbuf sequel

and GenSetExnField cenv cgbuf eenv (e,ecref,fieldNum,e2,m) sequel = 
    GenExpr cenv cgbuf eenv SPSuppress e Continue;
    let exnc = strip_eqns_from_ecref ecref
    let typ = GenExnType m cenv.g eenv.tyenv ecref
    CG.EmitInstrs cgbuf [] [ I_castclass typ ];
    let fld = List.nth (exnc.TrueInstanceFieldsAsList) fieldNum
    let ftyp = GenType m cenv.g eenv.tyenv fld.FormalType
    let fldName = GenFieldName exnc fld
    GenExpr cenv cgbuf eenv SPSuppress e2 Continue;
    CG.EmitInstr cgbuf [Pop; Pop] (mk_normal_stfld(mk_fspec_in_typ (typ,fldName,ftyp)));
    GenUnitThenSequel cenv eenv.cloc cgbuf sequel


and GenUnionCaseProof cenv cgbuf eenv (e,constr,tyargs,m) sequel =
    GenExpr cenv cgbuf eenv SPSuppress e Continue;
    let cuspec,idx = GenUnionCaseSpec m cenv.g eenv.tyenv constr tyargs
    let fty = EraseIlxClassunions.typ_of_alt cuspec idx 
    CG.EmitInstrs cgbuf [Pop; Push fty]
      [ mk_IlxInstr (EI_castdata(false,cuspec,idx)); ];
    GenSequel cenv eenv.cloc cgbuf sequel

and GenGetUnionCaseField cenv cgbuf eenv (e,constr,tyargs,n,m) sequel =
    assert (is_proven_ucase_typ (type_of_expr cenv.g e));
    
    GenExpr cenv cgbuf eenv SPSuppress e Continue;
    let cuspec,idx = GenUnionCaseSpec m cenv.g eenv.tyenv constr tyargs
            (* ANALYSIS: don't use castdata where we've already done a typetest *)
    let fty = actual_typ_of_cuspec_field cuspec idx n
    CG.EmitInstrs cgbuf [Pop; Push fty]
      [ //mk_IlxInstr (EI_castdata(false,cuspec,idx));
        mk_IlxInstr (EI_lddata(cuspec,idx,n)) ];
    GenSequel cenv eenv.cloc cgbuf sequel

and GenGetUnionCaseTag cenv cgbuf eenv (e,tycon,tyargs,m) sequel =
    GenExpr cenv cgbuf eenv SPSuppress e Continue;
    let cuspec = GenUnionSpec m cenv.g eenv.tyenv tycon tyargs
    CG.EmitInstrs cgbuf [Pop; Push cenv.g.ilg.typ_int32] [ mk_IlxInstr (EI_lddatatag(cuspec)) ];
    GenSequel cenv eenv.cloc cgbuf sequel

and GenSetUnionCaseField cenv cgbuf eenv (e,constr,tyargs,n,e2,m) sequel = 
    GenExpr cenv cgbuf eenv SPSuppress e Continue;
    let cuspec,idx = GenUnionCaseSpec m cenv.g eenv.tyenv constr tyargs
    CG.EmitInstr cgbuf [Pop; Push (objtype_of_cuspec cuspec) ] (mk_IlxInstr (EI_castdata(false,cuspec,idx)));
    GenExpr cenv cgbuf eenv SPSuppress e2 Continue;
    CG.EmitInstr cgbuf [Pop; Pop] (mk_IlxInstr (EI_stdata(cuspec,idx,n)) );
    GenUnitThenSequel cenv eenv.cloc cgbuf sequel

and GenGetRecdFieldAddr cenv cgbuf eenv (e,f,tyargs,m) sequel = (* follows GenGetAddrOfRefCellField code *)
    GenExpr cenv cgbuf eenv SPSuppress e Continue;
    let fref = GenRecdFieldRef m cenv eenv.tyenv f tyargs
    CG.EmitInstrs cgbuf [Pop; Push (Type_byref (actual_typ_of_fspec fref))] [ I_ldflda fref ] ;
    GenSequel cenv eenv.cloc cgbuf sequel
         
and GenGetStaticFieldAddr cenv cgbuf eenv (f,tyargs,m) sequel = (* follows GenGetAddrOfRefCellField code *)
  let fspec = GenRecdFieldRef m cenv eenv.tyenv f tyargs
  CG.EmitInstrs cgbuf [Push (Type_byref (actual_typ_of_fspec fspec))] [ I_ldsflda fspec ] ;
  GenSequel cenv eenv.cloc cgbuf sequel
         
and GenGetRecdField cenv cgbuf eenv (e,f,tyargs,m) sequel =
    if verbose then dprintn ("GenGetRecdField");    
    GenExpr cenv cgbuf eenv SPSuppress e Continue;
    GenFieldGet false cenv cgbuf eenv (f,tyargs,m);
    GenSequel cenv eenv.cloc cgbuf sequel
  
and GenSetRecdField cenv cgbuf eenv (e1,f,tyargs,e2,m) sequel =
    GenExpr cenv cgbuf eenv SPSuppress e1 Continue;
    GenExpr cenv cgbuf eenv SPSuppress e2 Continue;
    GenFieldStore false cenv cgbuf eenv (f,tyargs,m) sequel
  
and GenGetStaticField cenv cgbuf eenv (f,tyargs,m) sequel =
    GenFieldGet true cenv cgbuf eenv (f,tyargs,m);
    GenSequel cenv eenv.cloc cgbuf sequel
  
and GenSetStaticField cenv cgbuf eenv (f,tyargs,e2,m) sequel =
    GenExpr cenv cgbuf eenv SPSuppress e2 Continue;
    GenFieldStore true cenv cgbuf eenv (f,tyargs,m) sequel

and mk_field_mspec isStatic = 
    (if isStatic then mk_static_nongeneric_mspec_in_typ else mk_nongeneric_instance_mspec_in_typ)
and mk_field_pops isStatic pops = if isStatic then pops else Pop::pops


and GenFieldGet isStatic cenv cgbuf eenv (rfref:RecdFieldRef,tyargs,m) =
    let fspec = GenRecdFieldRef m cenv eenv.tyenv rfref tyargs
    if use_genuine_field rfref.Tycon rfref.RecdField || tcref_in_this_assembly cenv.g.compilingFslib rfref.TyconRef then 
        CG.EmitInstrs cgbuf (mk_field_pops isStatic [ Push (actual_typ_of_fspec fspec)]) [ if isStatic then mk_normal_ldsfld fspec else mk_normal_ldfld fspec ] 
    else
        let mspec = mk_field_mspec isStatic (fspec.EnclosingType,"get_"^rfref.RecdField.rfield_id.idText, [], fspec.FormalType)
        CG.EmitInstr cgbuf (mk_field_pops isStatic [Push (actual_typ_of_fspec fspec)]) (mk_normal_call mspec)

and GenFieldStore isStatic cenv cgbuf eenv (rfref:RecdFieldRef,tyargs,m) sequel =
    let fspec = GenRecdFieldRef m cenv eenv.tyenv rfref tyargs
    let fld = rfref.RecdField
    if fld.IsMutable && not (use_genuine_field rfref.Tycon fld) then
        let mspec = mk_field_mspec isStatic (fspec.EnclosingType, "set_"^fld.rfield_id.idText, [fspec.FormalType],Type_void)
        
        CG.EmitInstr cgbuf (mk_field_pops isStatic [Pop]) (mk_normal_call mspec)
    else
        (* Within assemblies we do generate some set-field operations *)
        (* for immutable fields even when resolving recursive bindings. *)
        (* However we do not generate "set" properties for these. *)
        (* Hence we just set the field directly in this case. *)
        CG.EmitInstr cgbuf (mk_field_pops isStatic [Pop]) (if isStatic then mk_normal_stsfld fspec else mk_normal_stfld fspec); 
    GenUnitThenSequel cenv eenv.cloc cgbuf sequel

//--------------------------------------------------------------------------
// Generate arguments to calls
//-------------------------------------------------------------------------- 

/// Generate arguments to a call, unless the argument is the single lone "unit" value
/// to a method or value compiled as a method taking no arguments
and GenUntupledArgsDiscardingLoneUnit cenv cgbuf eenv m numObjArgs curriedArgInfos args  =
    match curriedArgInfos ,args with 
    // Type.M()
    // new C()
    | [[]],[arg] when numObjArgs = 0  -> 
        assert is_unit_typ cenv.g (type_of_expr cenv.g arg)
        GenExpr cenv cgbuf eenv SPSuppress arg discard
    // obj.M()
    | [[_];[]],[arg1;arg2] when numObjArgs = 1 -> 
        assert is_unit_typ cenv.g (type_of_expr cenv.g arg2) 
        GenExpr cenv cgbuf eenv SPSuppress arg1 Continue;
        GenExpr cenv cgbuf eenv SPSuppress arg2 discard
    | _ -> 
        (curriedArgInfos,args) ||> List.iter2 (fun argInfos x -> 
            GenUntupledArgExpr cenv cgbuf eenv m argInfos x Continue) 

/// Codegen arguments 
and GenUntupledArgExpr cenv cgbuf eenv m argInfos expr sequel =
    let numRequiredExprs = List.length argInfos
    assert (numRequiredExprs >= 1)
    if numRequiredExprs = 1 then
        GenExpr cenv cgbuf eenv SPSuppress expr sequel
    elif is_tuple expr then
        let es = try_dest_tuple expr
        if es.Length <> numRequiredExprs then error(InternalError("GenUntupledArgExpr (2)",m));
        es |> List.iter (fun x -> GenExpr cenv cgbuf eenv SPSuppress x Continue);
        GenSequel cenv eenv.cloc cgbuf sequel
    else
        let ty = type_of_expr cenv.g expr
        let locv,loce = mk_compgen_local m "arg" ty
        let bind = mk_compgen_bind locv expr
        LocalScope "untuple" cgbuf (fun scopeMarks ->
            let eenvinner = AllocStorageForBind cenv cgbuf scopeMarks eenv bind
            GenBind cenv cgbuf eenvinner bind;
            if verbose then dprintf "expr = %s\nty = %s\narity = %d\n" (showL (ExprL expr)) ((DebugPrint.showType ty)) numRequiredExprs;
            let tys = dest_tuple_typ cenv.g ty
            assert (tys.Length = numRequiredExprs)
            argInfos |> List.iteri (fun i fargty -> GenGetTupleField cenv cgbuf eenvinner (loce,tys,i,m) Continue);
            GenSequel cenv eenv.cloc cgbuf sequel
        )


//--------------------------------------------------------------------------
// Generate calls (try to detect direct calls)
//-------------------------------------------------------------------------- 
 
and GenApp cenv cgbuf eenv (f,fty,tyargs,args,m) sequel =
 if verbose then dprintn ("GenApp:");
 match (f,tyargs,args) with 
   (* Look for tailcall to turn into branch *)
  | (TExpr_val(v,_,_),_,_) when  
       ((ListAssoc.containsKey cenv.g.vref_eq v eenv.innerVals) && 
        not v.IsConstructor &&
        let (kind,_) = ListAssoc.find cenv.g.vref_eq v eenv.innerVals
        (* when branch-calling methods we must have the right type parameters *)
        begin match kind with
          | BranchCallClosure _ -> true
          | BranchCallMethod (_,_,tps,_,_)  ->  
              (List.lengthsEqAndForall2 (fun ty tp -> type_equiv cenv.g ty (mk_typar_ty tp)) tyargs tps)
        end &&
        (* must be exact #args, ignoring tupling - we untuple if needed below *)
        (let arityInfo = 
           match kind with
           | BranchCallClosure arityInfo
           | BranchCallMethod (arityInfo,_,_,_,_)  ->  arityInfo
         arityInfo.Length = args.Length
        ) &&
        (* no tailcall out of exception handler, etc. *)
        (match sequel_ignoring_end_scopes_and_discard sequel with Return | ReturnVoid -> true | _ -> false))
    -> 
        let (kind,mark) = ListAssoc.find cenv.g.vref_eq v eenv.innerVals
        let ntmargs = 
          match kind with
          | BranchCallClosure arityInfo ->
              let ntmargs = List.foldBack (+) arityInfo 0
              GenExprs cenv cgbuf eenv args;
              ntmargs
          | BranchCallMethod (arityInfo,curriedArgInfos,tps,ntmargs,numObjArgs)  ->
              assert (curriedArgInfos.Length = arityInfo.Length )
              assert (curriedArgInfos.Length = args.Length)
              //assert (curriedArgInfos.Length = ntmargs )
              GenUntupledArgsDiscardingLoneUnit cenv cgbuf eenv m numObjArgs curriedArgInfos args;
              ntmargs
        for i = ntmargs - 1 downto 0 do 
          CG.EmitInstrs cgbuf [Pop] [ I_starg (uint16 (i+cgbuf.PreallocatedArgCount)) ];
        done;
        CG.EmitInstrs cgbuf [] [ I_br (code_label_of_mark mark) ];
        GenSequelEndScopes cgbuf sequel
        
  // Similarly for PhysicalEquality becomes cheap reference equality for non-value-types 
  | (TExpr_val(v,_,_),[ty],[arg1;arg2]) when
    (cenv.g.vref_eq v cenv.g.poly_eq_inner_vref)  
    && (is_fsobjmodel_ref_typ cenv.g ty || 
        (is_il_ref_typ cenv.g ty 
         && not (type_equiv cenv.g ty cenv.g.system_Object_typ) 
         && not (type_equiv cenv.g ty cenv.g.system_Value_typ)
         && not (type_equiv cenv.g ty cenv.g.system_Enum_typ)) or
        is_delegate_typ cenv.g ty || 
        is_union_typ cenv.g ty || 
        is_recd_typ cenv.g ty || 
        is_repr_hidden_typ cenv.g ty || 
        is_tuple_typ cenv.g ty) ->
        
      GenExpr cenv cgbuf eenv SPSuppress arg1 Continue;
      GenExpr cenv cgbuf eenv SPSuppress arg2 Continue;
      CG.EmitInstr cgbuf [ Pop; Pop; Push cenv.g.ilg.typ_bool ] (I_arith AI_ceq);
      GenSequel cenv eenv.cloc cgbuf sequel

  // Optimize calls to top methods when given "enough" arguments. 
  | (TExpr_val(vref,vFlags,_),_,_) when
                     (let storage = storage_for_vref m vref eenv
                      match storage with   
                      | Method(topValInfo,vref,mspec,_,_,_) ->
                          (let tps,argtys,_,_ = GetTopValTypeInFSharpForm cenv.g topValInfo vref.Type m
                           tps.Length = tyargs.Length && 
                           argtys.Length <= args.Length)
                      | _ -> false) ->

      let storage = storage_for_vref m vref eenv
      begin match storage with   
      | Method (topValInfo,vref,mspec,_,_,_) ->
          if verbose then dprintn ("GenApp: Method");
          let nowArgs,laterArgs = 
              let _,curriedArgInfos,returnTy,retInfo = GetTopValTypeInFSharpForm cenv.g topValInfo vref.Type m
              List.chop curriedArgInfos.Length args

          let actualRetTy = apply_types cenv.g vref.Type (tyargs,nowArgs)
          let _,curriedArgInfos,returnTy,retInfo = GetTopValTypeInCompiledForm cenv.g topValInfo vref.Type m

          let ilTyArgs = GenTypeArgs m cenv.g eenv.tyenv tyargs

          // For instance method calls chop off some type arguments, which are already 
          // carried by the class.  Also work out if it's a virtual call. 
          let numEnclTypeArgs,virtualCall,newobj,isSuperInit,isSelfInit,_,_,_ = GetMemberCallInfo cenv.g (vref,vFlags) in

          // numEnclTypeArgs will include unit-of-measure args, unfortunately. For now, just cut-and-paste code from GetMemberCallInfo
          // @REVIEW: refactor this 
          let numEnclTypeArgs = 
              match vref.MemberInfo with 
              | Some(membInfo) when not (vref.IsExtensionMember) -> 
                  List.length(vref.MemberApparentParent.TyparsNoRange |> DropErasedTypars) 
              | _ -> 0

          let (ilClassArgTys,ilMethArgTys) = 
              if ilTyArgs.Length  < numEnclTypeArgs then error(InternalError("length mismatch",m));
              List.chop numEnclTypeArgs ilTyArgs

          let boxity = boxity_of_typ mspec.EnclosingType
          let mspec = mk_mspec (mspec.MethodRef, boxity,ilClassArgTys,ilMethArgTys)
          
          // "Unit" return types on static methods become "void" 
          let mustGenerateUnitAfterCall = isNone returnTy
          let isTailCall = 
              if isNil laterArgs && not isSelfInit then 
                  let isDllImport = vref_isDllImport cenv.g vref
                  let hasByrefArg = nowArgs |> List.exists (type_of_expr cenv.g >> is_byref_typ cenv.g) 
                  let makesNoCriticalTailcalls = vref.MakesNoCriticalTailcalls 
                  CanTailcall(boxity,eenv.withinSEH,hasByrefArg,mustGenerateUnitAfterCall,isDllImport,isSelfInit,makesNoCriticalTailcalls,sequel)
              else Normalcall
          
          let callInstr = 
              if virtualCall then I_callvirt (isTailCall, mspec, None) 
              elif newobj then I_newobj (mspec, None) 
              else I_call (isTailCall, mspec, None)

          // ok, now we're ready to generate 
          if isSuperInit || isSelfInit then 
              CG.EmitInstrs cgbuf [ Push mspec.EnclosingType ] [ ldarg_0 ] ;
          
          //dprintfn "mspec.Name = %s, curriedArgInfos = %A, nowArgs = %A" mspec.Name (List.map List.length curriedArgInfos) (List.length nowArgs)
          GenUntupledArgsDiscardingLoneUnit cenv cgbuf eenv m (GetNumObjArgsOfValRef vref) curriedArgInfos nowArgs;
          
          let nargs = mspec.FormalArgTypes.Length
          if verbose then dprintf "GenApp: call, nargs = %d, mspec.ILCallingConv.IsStatic = %b\n" nargs mspec.CallingConv.IsStatic;
          CG.EmitInstr cgbuf (List.replicate (nargs + (if mspec.CallingConv.IsStatic || newobj then 0 else 1)) Pop @ 
                               (if mustGenerateUnitAfterCall || isSuperInit || isSelfInit then [] else [Push (GenType m cenv.g eenv.tyenv actualRetTy)])) callInstr;
          if verbose then dprintn ("GenApp: after");

          // For isSuperInit, load the 'this' pointer as the pretend 'result' of the operation.  It will be popped agin in most cases 
          if isSuperInit then CG.EmitInstrs cgbuf [ Push mspec.EnclosingType ] [ ldarg_0 ] ;

          // When generating deubg code, generate a 'nop' after a 'call' that returns 'void'
          // This is what C# does, as it allows the call location to be maintained correctly in the stack frame
          if cenv.debug && mustGenerateUnitAfterCall && (isTailCall = Normalcall) then 
              CG.EmitInstrs cgbuf [  ] [ i_nop ] ;

          if isNil laterArgs then 
              (* Generate the "unit" value if necessary *)
              CommitCallSequel cenv eenv.cloc cgbuf mustGenerateUnitAfterCall sequel 
          else 
              GenIndirectCall cenv cgbuf eenv (actualRetTy,[],laterArgs,m) sequel

          if verbose then dprintn ("GenApp: Method Done");
      | _ -> failwith "??"
      end
        
    // This case is for getting/calling a value, when we can't call it directly. 
    // However, we know the type instantiation for the value.  
    // In this case we can often generate a type-specific local expression for the value. 
    // This reduces the number of dynamic type applications. 
  | (TExpr_val(vref,_,_),_,_)  -> 
     GenGetValRefAndSequel cenv cgbuf eenv m vref (Some (tyargs,args,m,sequel))
        
  | _ ->
    (* worst case: generate a first-class function value and call *)
    GenExpr cenv cgbuf eenv SPSuppress f Continue;
    GenIndirectCall cenv cgbuf eenv (fty,tyargs,args,m) sequel
        
and CanTailcall(boxity,withinSEH,hasByrefArg,mustGenerateUnitAfterCall,isDllImport,isSelfInit,makesNoCriticalTailcalls,sequel) = 
    if (boxity = AsObject) && not withinSEH && not hasByrefArg && not isDllImport && not isSelfInit && not makesNoCriticalTailcalls &&
        // We can tailcall even if we need to generate "unit", as long as we're about to throw the value away anyway as par of the return. 
        // We can tailcall if we don't need to generate "unit", as long as we're about to return. 
        (match sequel_ignore_end_scopes sequel with 
         | ReturnVoid | Return           -> not mustGenerateUnitAfterCall
         | DiscardThen ReturnVoid ->     mustGenerateUnitAfterCall
         | _                -> false) 
    then Tailcall 
    else Normalcall
        
and GenNamedLocalTyFuncCall cenv (cgbuf: CodeGenBuffer) eenv typ cloinfo tyargs m = 
    if verbose then dprintn ("Compiling local type func call in "^cgbuf.MethodName);
    
    let contract_tinst = cloinfo.ltyfunc_contract_ftyvs |> List.map mk_typar_ty |> GenTypeArgs m cenv.g eenv.tyenv
    let ilTyArgs = tyargs |> GenTypeArgs m cenv.g eenv.tyenv
    let _,contract_meth_il_gparams,(contract_clo_il_tspec:ILTypeSpec),contract_formal_il_rty = GenNamedLocalTypeFuncContractInfo cenv m cloinfo
    let contract_il_tspec = mk_tspec(contract_clo_il_tspec.TypeRef,contract_tinst)
    
    if not (List.length contract_meth_il_gparams = List.length tyargs) then errorR(Error("incorrect number of type arguments to local call",m));

    let contract_il_ty = Type_boxed contract_il_tspec
    // Local TyFunc are represented as a $contract type. they currently get stored in a value of type object
    // Recover result (value or reference types) via unbox_any.
    CG.EmitInstrs cgbuf [Pop;Push contract_il_ty]  [I_unbox_any contract_il_ty];
    let actual_rty = apply_types cenv.g typ (tyargs,[])

    let il_mspec = mk_instance_mspec_in_boxed_tspec(contract_il_tspec, "DirectInvoke", [], contract_formal_il_rty, ilTyArgs)
    let ilActualRetTy = GenType m cenv.g eenv.tyenv actual_rty
    CountCallFuncInstructions();
    CG.EmitInstr cgbuf [Pop;Push ilActualRetTy] (mk_normal_callvirt il_mspec);
    if verbose then dprintn "Done local type func call..."
    actual_rty

        
and GenIndirectCall cenv cgbuf eenv (functy,tyargs,args,m) sequel =
    if verbose then dprintn ("Compiling call in "^cgbuf.MethodName);
    GenExprs cenv cgbuf eenv args;
    if verbose then dprintn "Compiling call instruction...";
    (* Fold in the new types into the environment as we generate the formal types. *)
    let apps = 
        let typars,formal_functy = try_dest_forall_typ cenv.g functy
        if verbose then dprintf "length args = %d, formal_functy = %s\n" (List.length args) (showL(typeL formal_functy));

        let feenv = add_typars eenv.tyenv typars
        let mk_ty_apps = List.foldBack (fun tyarg apps -> Apps_tyapp(GenType m cenv.g eenv.tyenv tyarg,apps)) tyargs
        let formal_rty,mk_tm_apps = 
            List.fold 
              (fun (formal_functy,sofar) _ -> 
                let formal_dty,formal_rty = dest_fun_typ cenv.g formal_functy
                (formal_rty,(fun apps -> sofar (Apps_app(GenType m cenv.g feenv formal_dty,apps)))))
              (formal_functy,(fun x -> x))
              args
        if verbose then dprintn "Compiling return type...";
        let ret_apps = Apps_done (GenType m cenv.g feenv formal_rty)
        mk_ty_apps (mk_tm_apps ret_apps)
    let actual_rty = apply_types cenv.g functy (tyargs, args)
    let ilActualRetTy = GenType m cenv.g eenv.tyenv actual_rty
    let hasByrefArg = 
        let rec check x = 
          match x with 
          | Apps_tyapp(_,apps') -> check apps'
          | Apps_app(arg,apps') -> is_byref arg || check apps'
          | _ -> false
        check apps
        
    let isTailCall = CanTailcall(AsObject,eenv.withinSEH,hasByrefArg,false,false,false,false,sequel)
    CountCallFuncInstructions();
    CG.EmitInstr cgbuf (List.replicate (1+args.Length) Pop @ [Push ilActualRetTy]) (mk_IlxInstr (EI_callfunc(isTailCall,apps)));
    if verbose then dprintn "Done compiling indirect call...";
    GenSequel cenv eenv.cloc cgbuf sequel

//--------------------------------------------------------------------------
// Generate try expressions
//-------------------------------------------------------------------------- 

and GenTry cenv cgbuf eenv scopeMarks (e1,m,resty,spTry) =
    let sp = 
        match spTry with 
        | SequencePointAtTry m -> CG.EmitSeqPoint cgbuf m; SPAlways
        | NoSequencePointAtTry -> SPSuppress
    
    let stack,eenvinner = EmitSaveStack cenv cgbuf eenv m scopeMarks
    let start_try = CG.GenerateMark cgbuf "start_try"
    let end_try = CG.EmitDelayMark cgbuf "end_try"
    let after_handler = CG.EmitDelayMark cgbuf "after_handler"
    let eenvinner = {eenvinner with withinSEH = true}
    let il_resty = GenType m cenv.g eenvinner.tyenv resty
    let where_to_save_expr,eenvinner = AllocLocal cenv cgbuf eenvinner true (ilxgenGlobalNng.FreshCompilerGeneratedName ("tryres",m),il_resty) (start_try,end_try)

    // Generate the body of the try. In the normal case (SequencePointAtTry) we generate a sequence point
    // both on the 'try' keyword and on the start of the expression in the 'try'. For inlined code and
    // compiler generated 'try' blocks (i.e. NoSequencePointAtTry, used for the try/finally implicit 
    // in a 'use' or 'foreach'), we suppress the sequence point
    GenExpr cenv cgbuf eenvinner sp e1 (LeaveHandler (false, where_to_save_expr,after_handler));
    CG.SetMarkToHere cgbuf end_try;
    let tryMarks = (code_label_of_mark start_try, code_label_of_mark end_try)
    where_to_save_expr,eenvinner,stack,tryMarks,after_handler,il_resty

and GenTryCatch cenv cgbuf eenv (e1,vf:Val,ef,vh:Val,eh,m,resty,spTry,spWith) sequel =
    if verbose then dprintn ("GenTry");      
    (* Save the stack - gross because IL flushes the stack at the exn. handler *)
    (* note: eenvinner notes spill vars are live *)
    LocalScope "trystack" cgbuf (fun scopeMarks -> 
       let where_to_save_expr,eenvinner,stack,tryMarks,after_handler,il_resty = GenTry cenv cgbuf eenv scopeMarks (e1,m,resty,spTry) 

       (* Now the List.filter and catch blocks *)

       let seh = 
           if cenv.generateFilterBlocks then 
               let startOfFilter = CG.GenerateMark cgbuf "startOfFilter" 
               let afterFilter = CG.EmitDelayMark cgbuf "afterFilter"
               let (sequelOnBranches,afterJoin,stackAfterJoin,sequelAfterJoin) = GenJoinPoint cenv cgbuf "filter" eenv cenv.g.int_ty m EndFilter
               begin
                   // We emit the sequence point for the 'with' keyword span on the start of the List.filter
                   // block. However the targets of the List.filter block pattern matching should not get any
                   // sequence points (they will be 'true'/'false' values indicating if the exception has been
                   // caught or not).
                   //
                   // The targets of the handler block DO get sequence points. Thus the expected behaviour 
                   // for a try/with with a complex pattern is that we hit the "with" before the List.filter is run
                   // and then jump to the handler for the successful catch (or continue with exception handling
                   // if the List.filter fails)
                   match spWith with 
                   | SequencePointAtWith m -> CG.EmitSeqPoint cgbuf m
                   | NoSequencePointAtWith -> () 


                   CG.SetStack cgbuf [cenv.g.ilg.typ_Object];
                   let _,eenvinner = AllocLocalVal cenv cgbuf vf eenvinner None (startOfFilter,afterFilter)
                   CG.EmitInstr cgbuf [Pop; Push cenv.g.ilg.typ_Exception] (I_castclass cenv.g.ilg.typ_Exception);

                   GenStoreVal cenv cgbuf eenvinner vf.Range vf;

                   // Why SPSuppress? Because we do not emit a sequence point at the start of the List.filter - we've already put one on
                   // the 'with' keyword above
                   GenExpr cenv cgbuf eenvinner  SPSuppress ef sequelOnBranches;
                   CG.SetMarkToHere cgbuf afterJoin;
                   CG.SetStack cgbuf stackAfterJoin;
                   GenSequel cenv eenv.cloc cgbuf sequelAfterJoin;
               end;
               let endOfFilter = CG.GenerateMark cgbuf "endOfFilter"
               let filterMarks = (code_label_of_mark startOfFilter, code_label_of_mark endOfFilter)
               CG.SetMarkToHere cgbuf afterFilter;

               let startOfHandler = CG.GenerateMark cgbuf "startOfHandler" 
               begin
                   CG.SetStack cgbuf [cenv.g.ilg.typ_Object];
                   let _,eenvinner = AllocLocalVal cenv cgbuf vh eenvinner None (startOfHandler,after_handler)
                   CG.EmitInstr cgbuf [Pop; Push cenv.g.ilg.typ_Exception] (I_castclass cenv.g.ilg.typ_Exception);
                   GenStoreVal cenv cgbuf eenvinner vh.Range vh;

                   GenExpr cenv cgbuf eenvinner SPAlways eh (LeaveHandler (false, where_to_save_expr,after_handler));
               end;
               let endOfHandler = CG.GenerateMark cgbuf "endOfHandler"
               let handlerMarks = (code_label_of_mark startOfHandler, code_label_of_mark endOfHandler)
               SEH_filter_catch(filterMarks, handlerMarks)
           else 
               let startOfHandler = CG.GenerateMark cgbuf "startOfHandler" 
               begin
                   match spWith with 
                   | SequencePointAtWith m -> CG.EmitSeqPoint cgbuf m
                   | NoSequencePointAtWith -> () 

                   CG.SetStack cgbuf [cenv.g.ilg.typ_Object];
                   let _,eenvinner = AllocLocalVal cenv cgbuf vh eenvinner None (startOfHandler,after_handler)
                   CG.EmitInstr cgbuf [Pop; Push cenv.g.ilg.typ_Exception] (I_castclass cenv.g.ilg.typ_Exception);

                   GenStoreVal cenv cgbuf eenvinner m vh;

                   GenExpr cenv cgbuf eenvinner SPAlways eh (LeaveHandler (false, where_to_save_expr,after_handler));
               end;
               let endOfHandler = CG.GenerateMark cgbuf "endOfHandler"
               let handlerMarks = (code_label_of_mark startOfHandler, code_label_of_mark endOfHandler)
               SEH_type_catch(cenv.g.ilg.typ_Object, handlerMarks)

       cgbuf.EmitExceptionClause
         { exnClauses = [ seh ];
           exnRange= tryMarks } ;

       CG.SetMarkToHere cgbuf after_handler;
       CG.SetStack cgbuf [];

       match spWith with 
       | SequencePointAtWith m -> CG.EmitSeqPoint cgbuf m
       | NoSequencePointAtWith -> () 

       (* Restore the stack and load the result *)
       EmitRestoreStack cenv cgbuf stack; (* RESTORE *)

       EmitGetLocal cgbuf il_resty where_to_save_expr;
       GenSequel cenv eenv.cloc cgbuf sequel
   ) 


and GenTryFinally cenv cgbuf eenv (e1,e2,m,resty,spTry,spFinally) sequel =
    if verbose then dprintn ("GenTry");      
    (* Save the stack - gross because IL flushes the stack at the exn. handler *)
    (* note: eenvinner notes spill vars are live *)
    LocalScope "trystack" cgbuf (fun scopeMarks -> 
       let where_to_save_expr,eenvinner,stack,tryMarks,after_handler,il_resty = GenTry cenv cgbuf eenv scopeMarks (e1,m,resty,spTry) 

       (* Now the catch/finally block *)
       let startOfHandler = CG.GenerateMark cgbuf "startOfHandler" 
       CG.SetStack cgbuf [];
       
       let sp = 
           match spFinally with 
           | SequencePointAtFinally m -> CG.EmitSeqPoint cgbuf m; SPAlways
           | NoSequencePointAtFinally -> SPSuppress

       GenExpr cenv cgbuf eenvinner sp e2 (LeaveHandler (true, where_to_save_expr,after_handler));
       let endOfHandler = CG.GenerateMark cgbuf "endOfHandler"
       let handlerMarks = (code_label_of_mark startOfHandler, code_label_of_mark endOfHandler)
       cgbuf.EmitExceptionClause
         { exnClauses = [ SEH_finally(handlerMarks) ];
           exnRange   = tryMarks } ;

       CG.SetMarkToHere cgbuf after_handler;
       CG.SetStack cgbuf [];

       (* Restore the stack and load the result *)
       EmitRestoreStack cenv cgbuf stack; (* RESTORE *)
       EmitGetLocal cgbuf il_resty where_to_save_expr;
       GenSequel cenv eenv.cloc cgbuf sequel
   ) 

//--------------------------------------------------------------------------
// Generate for-loop
//-------------------------------------------------------------------------- 
    
and GenForLoop cenv cgbuf eenv (spFor,v,e1,dir,e2,loopBody,m) sequel =
    // The JIT/NGen eliminate array-bounds checks for C# loops of form:
    //   for(int i=0; i < (#ldlen arr#); i++) { ... arr[i] ... }
    // Here
    //     dir = BI_blt indicates an optimized for loop that fits C# form that evaluates its 'end' argument each time around
    //     dir = BI_ble indicates a normal F# for loop that evaluates its argument only once
    //
    // It is also important that we follow C# IL-layout exactly "prefix, jmp test, body, test, finish" for JIT/NGEN.
    let start = CG.GenerateMark cgbuf "for_start" 
    let finish = CG.EmitDelayMark cgbuf "for_finish"
    let inner = CG.EmitDelayMark cgbuf "for_inner"
    let test = CG.EmitDelayMark cgbuf "for_test"
    let stack,eenvinner = EmitSaveStack cenv cgbuf eenv m (start,finish)

    let isUp = (match dir with | FSharpForLoopUp | CSharpForLoopUp -> true | FSharpForLoopDown -> false);
    let isFSharpStyle = (match dir with FSharpForLoopUp | FSharpForLoopDown -> true | CSharpForLoopUp  -> false);
    
    let finishIdx,eenvinner = 
        if isFSharpStyle then 
            let v,eenvinner = AllocLocal cenv cgbuf eenvinner true (ilxgenGlobalNng.FreshCompilerGeneratedName ("endLoop",m), cenv.g.ilg.typ_int32) (start,finish)
            v, eenvinner
        else
            -1,eenvinner

    let _,eenvinner = AllocLocalVal cenv cgbuf v eenvinner None (start,finish) (* note: eenvStack noted stack spill vars are live *)
    match spFor with 
    | SequencePointAtForLoop(spStart) -> CG.EmitSeqPoint cgbuf  spStart;
    | NoSequencePointAtForLoop -> ()

    GenExpr cenv cgbuf eenv SPSuppress e1 Continue;
    GenStoreVal cenv cgbuf eenvinner m v;
    if isFSharpStyle then 
        GenExpr cenv cgbuf eenv SPSuppress e2 Continue;
        EmitSetLocal cgbuf finishIdx
        EmitGetLocal cgbuf cenv.g.ilg.typ_int32 finishIdx
        GenGetLocalVal cenv cgbuf eenvinner (range_of_expr e2) v None;        
        CG.EmitInstr cgbuf [Pop;Pop] (I_brcmp ((if isUp then BI_blt else BI_bgt),code_label_of_mark finish,code_label_of_mark inner));
    
    else
        CG.EmitInstr cgbuf [] (I_br (code_label_of_mark test));

    // .inner 
    CG.SetMarkToHere cgbuf inner;
    //    <loop body>
    GenExpr cenv cgbuf eenvinner SPAlways loopBody discard;
    //    v++ or v--
    GenGetLocalVal cenv cgbuf eenvinner (range_of_expr e2) v None;

    CG.EmitInstr cgbuf [Push cenv.g.ilg.typ_int32] (mk_ldc_i32 (1));
    CG.EmitInstr cgbuf [Pop] (I_arith (if isUp then AI_add else AI_sub));
    GenStoreVal cenv cgbuf eenvinner m v;

    // .text 
    CG.SetMarkToHere cgbuf test;

    // FSharpForLoopUp: if v <> e2 + 1 then goto .inner
    // FSharpForLoopDown: if v <> e2 - 1 then goto .inner
    // CSharpStyle: if v < e2 then goto .inner
    CG.EmitSeqPoint cgbuf  (range_of_expr e2);
    GenGetLocalVal cenv cgbuf eenvinner (range_of_expr e2) v None;
    let cmp = match dir with FSharpForLoopUp | FSharpForLoopDown -> BI_bne_un | CSharpForLoopUp -> BI_blt
    let e2Sequel =  (CmpThenBrOrContinue ( [Pop; Pop], I_brcmp(cmp,code_label_of_mark inner,code_label_of_mark finish)));

    if isFSharpStyle then 
        EmitGetLocal cgbuf cenv.g.ilg.typ_int32  finishIdx
        CG.EmitInstr cgbuf [Push cenv.g.ilg.typ_int32] (mk_ldc_i32 1);
        CG.EmitInstr cgbuf [Pop] (I_arith (if isUp then AI_add else AI_sub));
        GenSequel cenv eenv.cloc cgbuf e2Sequel
    else
        GenExpr cenv cgbuf eenv SPSuppress e2 e2Sequel;

    // .finish - loop-exit here 
    CG.SetMarkToHere cgbuf finish;

    // Restore the stack and load the result 
    EmitRestoreStack cenv cgbuf stack;
    GenUnitThenSequel cenv eenv.cloc cgbuf sequel

//--------------------------------------------------------------------------
// Generate while-loop 
//-------------------------------------------------------------------------- 
    
and GenWhileLoop cenv cgbuf eenv (spWhile,e1,e2,m) sequel =
    let finish = CG.EmitDelayMark cgbuf "while_finish" 
    let inner = CG.EmitDelayMark cgbuf "while_inner" 
    let start_test = CG.GenerateMark cgbuf "start_test"
    
    match spWhile with 
    | SequencePointAtWhileLoop(spStart) -> CG.EmitSeqPoint cgbuf  spStart;
    | NoSequencePointAtWhileLoop -> ()

    (* SEQUENCE POINTS: Emit a sequence point to cover all of 'while e do' *)
    GenExpr cenv cgbuf eenv SPSuppress e1 (CmpThenBrOrContinue ([Pop],(I_brcmp(BI_brfalse,code_label_of_mark finish,code_label_of_mark inner))));
    CG.SetMarkToHere cgbuf inner; 
    
    GenExpr cenv cgbuf eenv SPAlways e2 (DiscardThen (Br start_test));
    CG.SetMarkToHere cgbuf finish; 

    (* SEQUENCE POINTS: Emit a sequence point to cover 'done' if present *)

    GenUnitThenSequel cenv eenv.cloc cgbuf sequel

//--------------------------------------------------------------------------
// Generate seq
//-------------------------------------------------------------------------- 

and GenSequential cenv cgbuf eenv spIn (e1,e2,specialSeqFlag,spSeq,m) sequel =
    
    // Compiler generated sequential executions result in suppressions of sequence points on both 
    // left and right of the sequence
    let spAction,spExpr = 
        (match spSeq with 
         | SequencePointsAtSeq -> SPAlways,spIn 
         | SuppressSequencePointOnExprOfSequential -> SPSuppress,spIn
         | SuppressSequencePointOnStmtOfSequential -> spIn,SPSuppress)
    match specialSeqFlag with 
    | NormalSeq -> 
        if verbose then dprintf "GenSequential (normal), sequel = %s\n" (StringOfSequel sequel);
        GenExpr cenv cgbuf eenv spAction e1 discard; 
        GenExpr cenv cgbuf eenv spExpr e2 sequel
    | ThenDoSeq ->
        GenExpr cenv cgbuf eenv spExpr e1 Continue;
        GenExpr cenv cgbuf eenv spAction e2 discard;
        GenSequel cenv eenv.cloc cgbuf sequel

//--------------------------------------------------------------------------
// Generate IL assembly code.
// Polymorphic IL/ILX instructions may be instantiated when polymorphic code is inlined.
// We must implement this for the few uses of polymorphic instructions 
// in the standard libarary. 
//-------------------------------------------------------------------------- 

and GenAsmCode cenv cgbuf eenv (il,tyargs,args,rtys,m) sequel =
    if verbose then dprintf "GenAsmCode, #args = %d" (List.length args);  
    let ilTyArgs = GenTypesPermitVoid m cenv.g eenv.tyenv tyargs
    let il_rtys   = GenTypesPermitVoid m cenv.g eenv.tyenv rtys
    let il_after_inst = 
      il |> List.filter (function I_arith AI_nop -> false | _ -> true)
         |> List.map (fun i -> 
          let err s  = 
              errorR(InternalError(sprintf "%s: bad instruction: %A" s i,m))

          let mod_fspec fspec = 
                {fspec with fspecEnclosingType= 
                                   let ty = fspec.fspecEnclosingType
                                   let tspec = ty.TypeSpec
                                   mk_typ (boxity_of_typ ty) (ILTypeSpec.Create(tspec.TypeRef, ilTyArgs)) }          
          match i,ilTyArgs with   
            | I_unbox_any (Type_tyvar idx)           ,[tyarg] -> I_unbox_any (tyarg)
            | I_box (Type_tyvar idx)                 ,[tyarg] -> I_box (tyarg)
            | I_isinst (Type_tyvar idx)              ,[tyarg] -> I_isinst (tyarg)
            | I_castclass (Type_tyvar idx)           ,[tyarg] -> I_castclass (tyarg)
            | I_newarr (shape,Type_tyvar idx)        ,[tyarg] -> I_newarr (shape,tyarg)
            | I_ldelem_any (shape,Type_tyvar idx)    ,[tyarg] -> I_ldelem_any (shape,tyarg)
            | I_ldelema (ro,shape,Type_tyvar idx)    ,[tyarg] -> I_ldelema (ro,shape,tyarg)
            | I_stelem_any (shape,Type_tyvar idx)    ,[tyarg] -> I_stelem_any (shape,tyarg)
            | I_ldobj (a,b,Type_tyvar idx)           ,[tyarg] -> I_ldobj (a,b,tyarg)
            | I_stobj (a,b,Type_tyvar idx)           ,[tyarg] -> I_stobj (a,b,tyarg)
            | I_ldtoken (Token_type (Type_tyvar idx)),[tyarg] -> I_ldtoken (Token_type (tyarg))
            | I_sizeof (Type_tyvar idx)              ,[tyarg] -> I_sizeof (tyarg)
            | I_ldfld (al,vol,fspec)                 ,_       -> I_ldfld (al,vol,mod_fspec fspec)
            | I_ldflda (fspec)                       ,_       -> I_ldflda (mod_fspec fspec)
            | I_stfld (al,vol,fspec)                 ,_       -> I_stfld (al,vol,mod_fspec fspec)
            | I_stsfld (vol,fspec)                   ,_       -> I_stsfld (vol,mod_fspec fspec)
            | I_ldsfld (vol,fspec)                   ,_       -> I_ldsfld (vol,mod_fspec fspec)
            | I_ldsflda (fspec)                      ,_       -> I_ldsflda (mod_fspec fspec)
            | EI_ilzero(Type_tyvar idx)              ,[tyarg] -> EI_ilzero(tyarg)
            | I_other e,_ when is_ilx_ext_instr e -> 
                begin match (dest_ilx_ext_instr e),ilTyArgs with 
                |  _ -> 
                    if not (isNil tyargs) then err "Bad polymorphic ILX instruction"; 
                    i
                end
            | I_arith AI_nop,_ -> i  
                (* These are embedded in the IL for a an initonly ldfld, i.e. *)
                (* here's the relevant comment from tc.ml *)
                (*     "Add an I_nop if this is an initonly field to make sure we never recognize it as an lvalue. See mk_expra_of_expr." *)

            | _ -> 
                if not (isNil tyargs) then err "Bad polymorphic IL instruction"; 
                i)
    match il_after_inst,args,sequel,il_rtys with 

    // Strip off any ("ceq" x false) when the sequel is a comparison branch and change the BI_brfalse to a BI_brtrue
    // This is the instruction sequence for "not" 
    // For these we can just generate the argument and change the test (from a brfalse to a brtrue and vice versa) 
    | ([ I_arith AI_ceq ],
       [arg1; TExpr_const((TConst_bool false | TConst_sbyte 0y| TConst_int16 0s | TConst_int32 0 | TConst_int64 0L | TConst_byte 0uy| TConst_uint16 0us | TConst_uint32 0u | TConst_uint64 0UL),_,_) ], 
       CmpThenBrOrContinue([Pop],I_brcmp (((BI_brfalse | BI_brtrue) as bi) , label1,label2)),
       _) ->

            let bi = match bi with BI_brtrue -> BI_brfalse | _ -> BI_brtrue
            GenExpr cenv cgbuf eenv SPSuppress arg1 (CmpThenBrOrContinue([Pop],I_brcmp (bi, label1,label2)))

    // Query; when do we get a 'ret' in IL assembly code?
    | [ I_ret ], [arg1],sequel,[ilRetTy] -> 
          GenExpr cenv cgbuf eenv SPSuppress arg1 Continue;
          CG.EmitInstr cgbuf [Pop] I_ret;
          GenSequelEndScopes cgbuf sequel

    // Query; when do we get a 'ret' in IL assembly code?
    | [ I_ret ], [],sequel,[ilRetTy] -> 
          CG.EmitInstr cgbuf [Pop] I_ret;
          GenSequelEndScopes cgbuf sequel

    // 'throw' instructions are a bit of a problem - e.g. let x = (throw ...) in ... expects a value *)
    // to be left on the stack.  But dead-code checking by some versions of the .NET verifier *)
    // mean that we can't just have fake code after the throw to generate the fake value *)
    // (nb. a fake value can always be generated by a "ldnull unbox.any ty" sequence *)
    // So in the worst case we generate a fake (never-taken) branch to a piece of code to generate *)
    // the fake value *)
    | [ I_throw ], [arg1],sequel,[ilRetTy] -> 
        match sequel_ignore_end_scopes sequel with 
        | s when IsSequelImmediate  s -> 
            if verbose then dprintf "GenAsmCode: throw: A\n";  
            (* In most cases we can avoid doing this... *)
            GenExpr cenv cgbuf eenv SPSuppress arg1 Continue;
            CG.EmitInstr cgbuf [Pop] I_throw;
            GenSequelEndScopes cgbuf sequel
        | _ ->  
            if verbose then dprintf "GenAsmCode: throw: B\n";  
            let after1 = CG.EmitDelayMark cgbuf ("fake_join")
            let after2 = CG.EmitDelayMark cgbuf ("fake_join")
            let after3 = CG.EmitDelayMark cgbuf ("fake_join")
            CG.EmitInstrs cgbuf [] [mk_ldc_i32 0; 
                                     I_brcmp (BI_brfalse,code_label_of_mark after2,code_label_of_mark after1); ];

            CG.SetMarkToHere cgbuf after1;
            CG.EmitInstrs cgbuf [Push ilRetTy] [i_ldnull;  I_unbox_any ilRetTy; I_br (code_label_of_mark after3) ];
            
            CG.SetMarkToHere cgbuf after2;
            GenExpr cenv cgbuf eenv SPSuppress arg1 Continue;
            CG.EmitInstr cgbuf [Pop] I_throw;
            CG.SetMarkToHere cgbuf after3;
            GenSequel cenv eenv.cloc cgbuf sequel;
    | _ -> 
      // float or float32 or float<_> or float32<_>
      let g = cenv.g in 
      let anyfpType ty = type_equiv_aux EraseMeasures g g.float_ty ty ||  type_equiv_aux EraseMeasures g g.float32_ty ty 

      // Otherwise generate the arguments, and see if we can use a I_brcmp rather than a comparison followed by an I_brfalse/I_brtrue 
      GenExprs cenv cgbuf eenv args;
      match il_after_inst,sequel with

      (* NOTE: THESE ARE NOT VALID ON FLOATING POINT DUE TO NaN.  Hence INLINE ASM ON FP. MUST BE CAREFULLY WRITTEN  *)

      | [ I_arith AI_clt ], CmpThenBrOrContinue([Pop],I_brcmp (BI_brfalse, label1,label2)) when not (anyfpType (type_of_expr g args.Head)) ->
        CG.EmitInstr cgbuf [Pop; Pop] (I_brcmp(BI_bge,label1,label2));
      | [ I_arith AI_cgt ], CmpThenBrOrContinue([Pop],I_brcmp (BI_brfalse, label1,label2)) when not (anyfpType (type_of_expr g args.Head)) ->
        CG.EmitInstr cgbuf [Pop; Pop] (I_brcmp(BI_ble,label1, label2));
      | [ I_arith AI_clt_un ], CmpThenBrOrContinue([Pop],I_brcmp (BI_brfalse, label1,label2)) when not (anyfpType (type_of_expr g args.Head)) ->
        CG.EmitInstr cgbuf [Pop; Pop] (I_brcmp(BI_bge_un,label1,label2));
      | [ I_arith AI_cgt_un ], CmpThenBrOrContinue([Pop],I_brcmp (BI_brfalse, label1,label2)) when not (anyfpType (type_of_expr g args.Head)) ->
        CG.EmitInstr cgbuf [Pop; Pop] (I_brcmp(BI_ble_un,label1, label2));
      | [ I_arith AI_ceq ], CmpThenBrOrContinue([Pop],I_brcmp (BI_brfalse, label1,label2)) when not (anyfpType (type_of_expr g args.Head)) ->
        CG.EmitInstr cgbuf [Pop; Pop] (I_brcmp(BI_bne_un,label1, label2));
        
      // THESE ARE VALID ON FP w.r.t. NaN 
        
      | [ I_arith AI_clt ], CmpThenBrOrContinue([Pop],I_brcmp (BI_brtrue, label1,label2)) ->
        CG.EmitInstr cgbuf [Pop; Pop] (I_brcmp(BI_blt,label1, label2));
      | [ I_arith AI_cgt ], CmpThenBrOrContinue([Pop],I_brcmp (BI_brtrue, label1,label2)) ->
        CG.EmitInstr cgbuf [Pop; Pop] (I_brcmp(BI_bgt,label1, label2));
      | [ I_arith AI_clt_un ], CmpThenBrOrContinue([Pop],I_brcmp (BI_brtrue, label1,label2)) ->
        CG.EmitInstr cgbuf [Pop; Pop] (I_brcmp(BI_blt_un,label1, label2));
      | [ I_arith AI_cgt_un ], CmpThenBrOrContinue([Pop],I_brcmp (BI_brtrue, label1,label2)) ->
        CG.EmitInstr cgbuf [Pop; Pop] (I_brcmp(BI_bgt_un,label1, label2));
      | [ I_arith AI_ceq ], CmpThenBrOrContinue([Pop],I_brcmp (BI_brtrue, label1,label2)) ->
        CG.EmitInstr cgbuf [Pop; Pop] (I_brcmp(BI_beq,label1, label2));
      | _ -> 
        // Failing that, generate the real IL leaving value(s) on the stack 
        CG.EmitInstrs cgbuf (List.replicate args.Length Pop @ List.map push il_rtys) il_after_inst;

        // If no return values were specified generate a "unit" 
        if isNil rtys then 
          GenUnitThenSequel cenv eenv.cloc cgbuf sequel
        else 
          GenSequel cenv eenv.cloc cgbuf sequel

//--------------------------------------------------------------------------
// Generate expression quotations
//-------------------------------------------------------------------------- 

and GenQuotation cenv cgbuf eenv (ast,conv,m,ety) sequel =
    let argTypes,argExprs, astSpec = 
        match !conv with  
        | Some res -> res
        | None -> 
            try 
                Creflect.ConvExprPublic (cenv.g, cenv.amap, cenv.viewCcu) Creflect.empty_env ast 
            with 
                Creflect.InvalidQuotedTerm e -> error(e)
    let astPickledBytes = Sreflect.pickle astSpec
    let mk_typeof_expr ilty =  
        mk_asm ([  mk_normal_call (mspec_Type_GetTypeFromHandle cenv.g.ilg) ], [],
                       [mk_asm ([ I_ldtoken (Token_type ilty) ], [],[],[cenv.g.system_RuntimeTypeHandle_typ],m)],
                       [cenv.g.system_Type_typ],m)

    let someTypeInModuleExpr =  mk_typeof_expr(Type_boxed eenv.someTspecInThisModule)
    let rawTy = mk_raw_expr_ty cenv.g                          
    let mk_list ty els = List.foldBack (mk_cons cenv.g ty) els (mk_nil cenv.g m ty)
    let typeExprs = List.map (GenType m cenv.g eenv.tyenv >> mk_typeof_expr) argTypes 
    let typesExpr = mk_list cenv.g.system_Type_typ typeExprs 
    let argsExpr = mk_list rawTy argExprs 
    let bytesExpr = TExpr_op(TOp_bytes(astPickledBytes),[],[],m)
    let unpickledExpr = mk_call_unpickle_quotation cenv.g m someTypeInModuleExpr typesExpr argsExpr bytesExpr
    let afterCastExpr = 
        // Detect a typed quotation and insert the cast if needed. The cast should not fail but does
        // unfortunately involve a "typeOf" computation over a quotation tree.
        if tcref_eq cenv.g (tcref_of_stripped_typ cenv.g ety) cenv.g.expr_tcr then 
            mk_call_cast_quotation cenv.g m (List.hd (tinst_of_stripped_typ cenv.g ety)) unpickledExpr
        else
            unpickledExpr
    GenExpr cenv cgbuf eenv SPSuppress afterCastExpr sequel

//--------------------------------------------------------------------------
// Generate calls to IL methods
//-------------------------------------------------------------------------- 

and GenIlCall cenv cgbuf eenv ((virt,protect,valu,newobj,vFlags,_,isDllImport,_,mref),enclTypeArgs,methTypeArgs,args,rtys,m) sequel =
    if verbose then dprintn ("GenIlCall");     
    let hasByrefArg  =  mref.ArgTypes |> List.exists is_byref
    let isSuperInit = (vFlags = CtorValUsedAsSuperInit)
    let boxity = (if valu then AsValue else AsObject)
    let mustGenerateUnitAfterCall = (isNil rtys)
    let makesNoCriticalTailcalls = (newobj || not virt) // Don't tailcall for 'newobj', or 'call' to IL code
    let tail = CanTailcall(boxity,eenv.withinSEH,hasByrefArg,mustGenerateUnitAfterCall,isDllImport,false,makesNoCriticalTailcalls,sequel)
    
    let il_ttyargs = GenTypeArgs m cenv.g eenv.tyenv enclTypeArgs
    let ilMethArgTys = GenTypeArgs m cenv.g eenv.tyenv methTypeArgs
    let il_rtys = GenTypes m cenv.g eenv.tyenv rtys
    let mspec = mk_mspec (mref,boxity,il_ttyargs,ilMethArgTys)

    // Load the 'this' pointer to pass to the superclass constructor. This argument is not 
    // in the expression tree since it can't be treated like an ordinary value 
    if isSuperInit then CG.EmitInstrs cgbuf [ Push mspec.EnclosingType ] [ ldarg_0 ] ;
    GenExprs cenv cgbuf eenv args;
    let il = 
        if newobj then [ I_newobj(mspec,None) ] 
        elif virt then [ I_callvirt(tail,mspec,None) ] 
        else  [ I_call(tail,mspec,None) ]
    CG.EmitInstrs cgbuf (List.replicate (args.Length + (if isSuperInit then 1 else 0)) Pop @ (if isSuperInit then [] else List.map push il_rtys)) il;

    // Load the 'this' pointer as the pretend 'result' of the isSuperInit operation.  
    // It will be immediately popped in most cases, but may also be used as the target of ome "property set" oeprations. 
    if isSuperInit then CG.EmitInstrs cgbuf [ Push mspec.EnclosingType ] [ ldarg_0 ] ;
    CommitCallSequel cenv eenv.cloc cgbuf mustGenerateUnitAfterCall sequel

and CommitCallSequel cenv cloc cgbuf mustGenerateUnitAfterCall sequel =
    if mustGenerateUnitAfterCall 
    then GenUnitThenSequel cenv cloc cgbuf sequel
    else GenSequel cenv cloc cgbuf sequel


and GenTraitCall cenv cgbuf eenv (traitInfo, args, m) expr sequel =
    let minfoOpt = CommitOperationResult (ConstraintSolver.CodegenWitnessThatTypSupportsTraitConstraint cenv.g cenv.amap m traitInfo)
    match minfoOpt with 
    | None -> 
        let replacementExpr = 
            mk_throw m (type_of_expr cenv.g expr)
               (mk_exnconstr(mk_MFCore_tcref cenv.g.fslibCcu "DynamicInvocationNotSupportedException", 
                             [ mk_string cenv.g m traitInfo.MemberName],m)) 
        GenExpr cenv cgbuf eenv SPSuppress replacementExpr sequel
    | Some (minfo,methTypeArgs) -> 

        // Fix bug 1281:  If we resolve to an instance method on a struct and we haven't yet taken 
        // the address of the object then go do that 
        if Infos.minfo_is_struct cenv.g minfo && minfo.IsInstance && (match args with [] -> false | h::t -> not (is_byref_typ cenv.g (type_of_expr cenv.g h))) then 
            let h,t = List.headAndTail args
            let wrap,h' = mk_expra_of_expr cenv.g true PossiblyMutates h m 
            GenExpr cenv cgbuf eenv SPSuppress (wrap (TExpr_op(TOp_trait_call(traitInfo), [], (h' :: t), m))) sequel 
        else        
            let slotsig = Infos.SlotSigOfMethodInfo cenv.amap m minfo 
            let gty = minfo.EnclosingType
            let (il_gty:ILType),ilParams,(ilReturn:ILReturnValue) = GenFormalSlotsig m cenv eenv slotsig
            let ilArgTys = typs_of_params ilParams 
            let ilRetTy = ilReturn.Type 
            let mref = mk_mref(il_gty.TypeRef, (if minfo.IsInstance then ILCallingConv.Instance else ILCallingConv.Static), minfo.LogicalName, List.length (DropErasedTyargs methTypeArgs), ilArgTys, ilRetTy) 
            let tinst = snd(dest_stripped_tyapp_typ cenv.g gty) 
            let rtys = Option.to_list (actual_rty_of_slotsig tinst methTypeArgs slotsig)
            GenIlCall cenv cgbuf eenv ((minfo.IsVirtual,
                                         minfo.IsProtectedAccessiblity,
                                         Infos.minfo_is_struct cenv.g minfo,false,NormalValUse,false,false,None,mref),
                                       tinst,methTypeArgs,args,rtys,m) sequel

//--------------------------------------------------------------------------
// Generate byref-related operations
//-------------------------------------------------------------------------- 

and GenGetAddrOfRefCellField cenv cgbuf eenv (e,ty,m) sequel =
    if verbose then dprintn ("GenGetAddrOfRefCellField");     
    GenExpr cenv cgbuf eenv SPSuppress e Continue;
    let fref = GenRecdFieldRef m cenv eenv.tyenv (mk_refcell_contents_rfref cenv.g) [ty]
    CG.EmitInstrs cgbuf [Pop; Push (Type_byref (actual_typ_of_fspec fref))] [ I_ldflda fref ] ;
    GenSequel cenv eenv.cloc cgbuf sequel

and GenGetValAddr cenv cgbuf eenv (v,m) sequel =
    if verbose then dprintn ("GenGetValAddr");     
    let vspec = deref_val v
    let il_ty = GenTypeOfVal cenv eenv vspec
    match storage_for_vref m v eenv with 
    | Local (idx,None) ->
        CG.EmitInstrs cgbuf [ Push (Type_byref il_ty)] [ I_ldloca (uint16 idx) ] ;
    | Arg idx ->
        CG.EmitInstrs cgbuf [ Push (Type_byref il_ty)] [ I_ldarga (uint16 idx) ] ;
    | StaticField (fspec,vref,hasLiteralAttr,ilTypeSpecForProperty,fieldName,_,il_ty,_,_,_) ->  
        if hasLiteralAttr then errorR(Error("Taking the address of a literal field is invalid",m));
        EmitGetStaticFieldAddr cgbuf il_ty fspec
    | Env (_,i,localCloInfo) -> 
        CG.EmitInstr cgbuf [Push (Type_byref il_ty)] (mk_IlxInstr (EI_ldenva i)); 
    | Local (_,Some _) | Method _ | Env _ | Unrealized | Null ->  
        errorR(Error( "This operation involves taking the address of a value '"^v.DisplayName^"' represented using a local variable or other special representation. This is invalid",m));
        CG.EmitInstrs cgbuf [Pop; Push (Type_byref il_ty)] [ I_ldarga (uint16 669 (* random value for post-hoc diagnostic analysis on generated tree *) ) ] ;

    GenSequel cenv eenv.cloc cgbuf sequel

and GenGetByref cenv cgbuf eenv (v:ValRef,m) sequel =
    if verbose then dprintn ("GenGetByref");     
    GenGetLocalVRef cenv cgbuf eenv m v None;
    let ilty = GenType m cenv.g eenv.tyenv (dest_byref_typ cenv.g v.Type)
    CG.EmitInstrs cgbuf [Pop; Push ilty] [ mk_normal_ldobj ilty ];
    GenSequel cenv eenv.cloc cgbuf sequel

and GenSetByref cenv cgbuf eenv (v:ValRef,e,m) sequel =
    if verbose then dprintn ("GenSetByref");     
    GenGetLocalVRef cenv cgbuf eenv m v None;
    GenExpr cenv cgbuf eenv SPSuppress e Continue;
    let ilty = GenType m cenv.g eenv.tyenv (dest_byref_typ cenv.g v.Type)
    CG.EmitInstrs cgbuf [Pop; Pop] [ mk_normal_stobj ilty ];
    GenUnitThenSequel cenv eenv.cloc cgbuf sequel

and GenDefaultValue cenv cgbuf eenv (ty,m) =
    let il_ty = GenType m cenv.g eenv.tyenv ty
    if is_ref_typ cenv.g ty then 
        CG.EmitInstr cgbuf [Push il_ty] i_ldnull
    else
        match try_tcref_of_stripped_typ cenv.g ty with 
        | Some tcref when (tcref_eq cenv.g cenv.g.system_SByte_tcref tcref || 
                           tcref_eq cenv.g cenv.g.system_Int16_tcref tcref || 
                           tcref_eq cenv.g cenv.g.system_Int32_tcref tcref || 
                           tcref_eq cenv.g cenv.g.system_Bool_tcref tcref || 
                           tcref_eq cenv.g cenv.g.system_Byte_tcref tcref || 
                           tcref_eq cenv.g cenv.g.system_Char_tcref tcref || 
                           tcref_eq cenv.g cenv.g.system_UInt16_tcref tcref || 
                           tcref_eq cenv.g cenv.g.system_UInt32_tcref tcref) ->
            CG.EmitInstr cgbuf [Push il_ty] i_ldc_i32_0
        | Some tcref when (tcref_eq cenv.g cenv.g.system_Int64_tcref tcref || 
                           tcref_eq cenv.g cenv.g.system_UInt64_tcref tcref) ->
            CG.EmitInstr cgbuf [Push il_ty] (mk_ldc_i64 0L)
        | Some tcref when (tcref_eq cenv.g cenv.g.system_Single_tcref tcref) ->
            CG.EmitInstr cgbuf [Push il_ty] (mk_ldc_single 0.0f)
        | Some tcref when (tcref_eq cenv.g cenv.g.system_Double_tcref tcref) ->
            CG.EmitInstr cgbuf [Push il_ty] (mk_ldc_double 0.0)
        | _ -> 
            let il_ty = GenType m cenv.g eenv.tyenv ty
            LocalScope "ilzero" cgbuf (fun scopeMarks ->
                let loc_idx,eenvinner = AllocLocal cenv cgbuf eenv true (ilxgenGlobalNng.FreshCompilerGeneratedName ("default",m), il_ty) scopeMarks
                // "initobj" (Generated by EmitInitLocal) doesn't work on byref types 
                // But ilzero(&ty) only gets generated in the built-in get-address function so 
                // we can just rely on zeroinit of all IL locals. 
                match il_ty with 
                |  Type_byref _ -> ()
                | _ -> EmitInitLocal cgbuf il_ty loc_idx
                EmitGetLocal cgbuf il_ty loc_idx;
            )

//--------------------------------------------------------------------------
// Generate object expressions as ILX "closures"
//-------------------------------------------------------------------------- 

and GenSlotParam m cenv eenv isGeneric (TSlotParam(nm,ty,inFlag,outFlag,optionalFlag,attribs)) = 
    let inFlag2,outFlag2,optionalFlag2,paramMarshal2,attribs = GenParamAttribs cenv attribs
    
    { paramName=nm;
      paramType= GenParamType m cenv.g eenv.tyenv false isGeneric ty;
      paramDefault=None;  
      paramMarshal=paramMarshal2; 
      paramIn=inFlag || inFlag2;
      paramOut=outFlag || outFlag2;
      paramOptional=optionalFlag || optionalFlag2;
      paramCustomAttrs= mk_custom_attrs (GenAttrs cenv eenv attribs) }
    
and GenFormalSlotsig m cenv eenv (TSlotSig(nm,typ,ctps,mtps,paraml,returnTy) as slotsig) = 
    let paraml = List.concat paraml
    let ilTy = GenType m cenv.g eenv.tyenv typ
    let eenv_for_slotsig = env_for_typars (ctps @ mtps) eenv
    let isGeneric = nonNil (DropErasedTypars ctps) || nonNil (DropErasedTypars mtps)
    let ilParams = paraml |> List.map (GenSlotParam m cenv eenv_for_slotsig isGeneric) 
    let ilRetTy = GenReturnType m cenv.g eenv_for_slotsig.tyenv false isGeneric returnTy
    let ilReturn = mk_return  ilRetTy
    ilTy, ilParams,ilReturn

and inst_slotparam inst (TSlotParam(nm,ty,inFlag,fl2,fl3,attrs)) = TSlotParam(nm,InstType inst ty,inFlag,fl2,fl3,attrs) 

and GenActualSlotsig m cenv eenv (TSlotSig(nm,typ,ctps,mtps,paraml,returnTy) as SlotSig) methTyparsOfOverridingMethod = 
    let paraml = List.concat paraml
    let slotsig_inst = mk_typar_inst (ctps@mtps) (tinst_of_stripped_typ cenv.g typ @ generalize_typars methTyparsOfOverridingMethod)
    let isGeneric = nonNil (DropErasedTypars ctps) || nonNil (DropErasedTypars mtps)
    let ilParams = paraml |> List.map (inst_slotparam slotsig_inst >> GenSlotParam m cenv eenv isGeneric) 
    let ilRetTy = GenReturnType m cenv.g eenv.tyenv false isGeneric (Option.map (InstType slotsig_inst) returnTy)
    let ilReturn = mk_return ilRetTy
    ilParams,ilReturn

and GenMethodImpl cenv eenv (shouldUseMethodImpl,(TSlotSig(nameOfOverridenMethod,enclTypOfOverridenMethod,_,_,_,_) as slotsig)) m =
    let ov_il_typ,ov_il_params,ov_il_ret = GenFormalSlotsig m cenv eenv slotsig
    let reallyUseMethodImpl = 
      if shouldUseMethodImpl 
          && cenv.workAroundReflectionEmitBugs
          && inst_of_typ ov_il_typ <> [] 
          && ov_il_typ.TypeRef.Scope = ScopeRef_local then 

          warning(Error("The implementation of a specified generic interface required a method implementation not fully supported by F# Interactive. In the unlikely event that the resulting class fails to load then compile the interface type into a statically-compiled DLL and reference it using '#r'",m));
          false

      else 
          shouldUseMethodImpl

    let nameOfOverridingMethod = if reallyUseMethodImpl then qualified_mangled_name_of_tcref (tcref_of_stripped_typ cenv.g enclTypOfOverridenMethod) nameOfOverridenMethod else nameOfOverridenMethod

    reallyUseMethodImpl,nameOfOverridingMethod, 
    (fun (ilTypeSpecForOverriding,methTyparsOfOverridingMethod) -> 
        let ov_tref = ov_il_typ.TypeRef
        let ov_mref = mk_mref(ov_tref, ILCallingConv.Instance, nameOfOverridenMethod, List.length (DropErasedTypars methTyparsOfOverridingMethod), (typs_of_params ov_il_params), ov_il_ret.Type)
        let eenv_for_ovby = AddTyparsToEnv methTyparsOfOverridingMethod eenv 
        let ilParamsOfOverridingMethod,ilReturnOfOverridingMethod = GenActualSlotsig m cenv eenv_for_ovby slotsig methTyparsOfOverridingMethod
        let ovby_mgparams = GenGenericParams m cenv eenv_for_ovby.tyenv methTyparsOfOverridingMethod 
        let ovby_mgactuals = generalize_gparams ovby_mgparams
        let ovby = mk_instance_mspec_in_boxed_tspec(ilTypeSpecForOverriding, nameOfOverridingMethod, typs_of_params ilParamsOfOverridingMethod, ilReturnOfOverridingMethod.Type, ovby_mgactuals)
        { mimplOverrides = OverridesSpec(ov_mref,ov_il_typ);
          mimplOverrideBy = ovby })

and bindBaseVarOpt cenv eenv baseValOpt = 
    match baseValOpt with 
    | None -> eenv
    | Some basev -> AddStorageForVal cenv.g (basev,notlazy (Arg 0))  eenv  

and fixupVirtualSlotFlags mdef = 
    {mdef with
        mdHideBySig=true; 
        mdKind = (match mdef.mdKind with 
                   | MethodKind_virtual vinfo -> 
                      MethodKind_virtual
                         {vinfo with 
                             virtStrict=false }
                   | _ -> failwith "fixupVirtualSlotFlags") } 

and renameMethodDef nameOfOverridingMethod mdef = 
    {mdef with mdName=nameOfOverridingMethod }

and fixupMethodImplFlags mdef = 
    {mdef with mdAccess=MemAccess_private;
               mdHideBySig=true; 
               mdKind=(match mdef.mdKind with 
                         | MethodKind_virtual vinfo -> 
                            MethodKind_virtual
                               {vinfo with 
                                   virtStrict=false;
                                   virtFinal=true;
                                   virtNewslot=true;  }
                         | _ -> failwith "fixupMethodImpl") }

and GenObjectMethod cenv eenvinner (cgbuf:CodeGenBuffer) shouldUseMethodImpl (TObjExprMethod((TSlotSig(nameOfOverridenMethod,enclTypOfOverridenMethod,_,_,_,_) as slotsig),methTyparsOfOverridingMethod,methodParams,methodBodyExpr,m)) =

    let eenvUnderTypars = AddTyparsToEnv methTyparsOfOverridingMethod eenvinner
    let ilParamsOfOverridingMethod,ilReturnOfOverridingMethod = GenActualSlotsig m cenv eenvUnderTypars slotsig methTyparsOfOverridingMethod

    // Args are stored starting at #1
    let methodParams = List.concat methodParams
    let eenvForMeth = AddStorageForLocalVals cenv.g (methodParams  |> List.mapi (fun i v -> (v,Arg i)))  eenvUnderTypars
    let ilMethodBody = CodeGenMethodForExpr cenv cgbuf.mgbuf (SPAlways,[],nameOfOverridenMethod,eenvForMeth,0,0,methodBodyExpr,(if slotsig_has_void_rty slotsig then discardAndReturnVoid else Return))

    let reallyUseMethodImpl,nameOfOverridingMethod,methodImplGenerator = GenMethodImpl cenv eenvinner (shouldUseMethodImpl,slotsig) (range_of_expr methodBodyExpr)

    let mdef = 
        mk_generic_virtual_mdef
          (nameOfOverridingMethod,
           ComputePublicMemberAccess false, (* false = may not change accessibility (override) *)
           GenGenericParams m cenv eenvUnderTypars.tyenv methTyparsOfOverridingMethod,
           ilParamsOfOverridingMethod,
           ilReturnOfOverridingMethod,
           MethodBody_il ilMethodBody)
    // fixup attributes to generate a method impl 
    let mdef = if reallyUseMethodImpl then fixupMethodImplFlags mdef else mdef
    let mdef = fixupVirtualSlotFlags mdef
    (reallyUseMethodImpl,methodImplGenerator,methTyparsOfOverridingMethod),mdef

and GenObjectExpr cenv cgbuf eenvouter expr (baseType,baseValOpt,basecall,overrides,interfaceImpls,m)  sequel =
    if verbose then dprintn ("GenObjectExpr");     
    let cloinfo,body,eenvinner  = GetIlxClosureInfo cenv m false None eenvouter expr 

    let cloAttribs = cloinfo.clo_attribs
    let cloFreeVars = cloinfo.clo_freevars
    let cloLambdas = cloinfo.clo_lambdas
    let cloName = cloinfo.clo_name
    
    let ilxCloSpec = cloinfo.clo_clospec
    let ilCloFreeVars = cloinfo.clo_il_frees
    let ilCloGenericFormals = cloinfo.clo_il_gparams
    assert(isNil cloinfo.ltyfunc_direct_il_gparams);
    let ilCloGenericActuals = inst_of_clospec cloinfo.clo_clospec
    let ilCloRetTy = cloinfo.clo_formal_il_rty
    let ilCloTypeRef = tref_of_clospec cloinfo.clo_clospec
    let ilTypeSpecForOverriding = mk_tspec(ilCloTypeRef,ilCloGenericActuals)

    let eenvinner = bindBaseVarOpt cenv eenvinner baseValOpt
    let ilCtorBody = CodeGenMethodForExpr cenv cgbuf.mgbuf (SPAlways,[],cloName,eenvinner,1,0,basecall,discardAndReturnVoid)

    let mdefs = overrides |> List.map (GenObjectMethod cenv eenvinner cgbuf false >> snd) 
    
    // Generate a method impl. This looks overly contorted and should likely be de-functionalized
    let methodImplGenerator ((reallyUseMethodImpl,methodImplGeneratorFunction,methTyparsOfOverridingMethod),mdef) = 
        let mimpl = (if reallyUseMethodImpl then Some(methodImplGeneratorFunction (ilTypeSpecForOverriding,methTyparsOfOverridingMethod)) else None)
        mimpl,mdef

    let mimpls,interfaceImplMethodDefs = interfaceImpls |> List.collect (snd >> List.map (GenObjectMethod cenv eenvinner cgbuf true >> methodImplGenerator))  |> List.unzip 
    let mimpls = mimpls |> List.choose (fun x -> x) 
    let interfaceTys = interfaceImpls |> List.map (fst >> GenType m cenv.g eenvinner.tyenv) 

    let attrs = GenAttrs cenv eenvinner cloAttribs
    let super = (if is_interface_typ cenv.g baseType then cenv.g.ilg.typ_Object else ilCloRetTy)
    let interfaceTys = interfaceTys @ (if is_interface_typ cenv.g baseType then [ilCloRetTy] else [])
    let cloTypeDef = GenClosureTypeDef cenv (ilCloTypeRef,cloFreeVars,ilCloGenericFormals,attrs,m,ilCloFreeVars,cloLambdas,ilCtorBody,(interfaceImplMethodDefs @ mdefs),mimpls,super,interfaceTys)

    cgbuf.mgbuf.AddTypeDef(ilCloTypeRef,cloTypeDef);
    CountClosure();
    GenGetLocalVals cenv cgbuf eenvouter m cloFreeVars;
    CG.EmitInstr cgbuf (List.replicate ilCloFreeVars.Length Pop@ [ Push (Pubclo.typ_of_lambdas cenv.g.ilxPubCloEnv cloLambdas)]) (mk_IlxInstr (EI_newclo ilxCloSpec));
    GenSequel cenv eenvouter.cloc cgbuf sequel

and GenSequenceExpr cenv (cgbuf:CodeGenBuffer) eenvouter (nextEnumeratorValRef:ValRef,pcvref:ValRef,currvref:ValRef,stateVars,generateNextExpr,closeExpr,checkCloseExpr:expr,seqElemTy, m)  sequel =
    let stateVars = [ pcvref; currvref ] @ stateVars
    let stateVarsSet = stateVars |> List.map deref_val |> Zset.of_list val_spec_order 
    if verbose then dprintn ("GenSequenceExpr");     

    // pretend that the state variables are bound
    let eenvouter = 
        eenvouter |> AddStorageForLocalVals cenv.g (stateVars |> List.map (fun v -> v.Deref,Local(0,None)))
    
    // Get the free variables. Make a lambda to pretend that the 'nextEnumeratorValRef' is bound (it is an argument to GenerateNext)
    let (cloAttribs,_,_,cloFreeTyvars,cloFreeVars,ilCloTypeRef:ILTypeRef,eenvinner) = 
         GetIlxClosureFreeVars cenv m None eenvouter (mk_lambda m nextEnumeratorValRef.Deref (generateNextExpr, cenv.g.int32_ty))
    let ilCloFreeVars = GetClosureILFreeVars cenv m [] eenvouter eenvinner cloFreeVars

    let ilCloSeqElemTy = GenType m cenv.g eenvinner.tyenv seqElemTy
    let cloRetTy = mk_seq_ty cenv.g seqElemTy
    let ilCloRetTyInner = GenType m cenv.g eenvinner.tyenv cloRetTy
    let ilCloRetTyOuter = GenType m cenv.g eenvouter.tyenv cloRetTy
    let ilCloEnumeratorTy = GenType m cenv.g eenvinner.tyenv (mk_IEnumerator_ty cenv.g seqElemTy)
    let ilCloEnumerableTy = GenType m cenv.g eenvinner.tyenv (mk_seq_ty cenv.g seqElemTy)
    let ilCloBaseTy = GenType m cenv.g eenvinner.tyenv (mk_tyapp_ty cenv.g.seq_base_tcr [seqElemTy])  
    let ilCloGenericParams = GenGenericParams m cenv eenvinner.tyenv cloFreeTyvars

    // Create a new closure class with a single "MoveNext" method that implements the iterator. 
    let ilCloTypeSpecInner = mk_tspec (ilCloTypeRef, generalize_gparams ilCloGenericParams)
    let cloLambdas = Lambdas_return ilCloRetTyInner 
    let cloref = IlxClosureRef(ilCloTypeRef, cloLambdas, ilCloFreeVars)
    let ilxCloSpec = IlxClosureSpec(cloref, GenGenericArgs m eenvouter.tyenv cloFreeTyvars)
    let formalClospec = IlxClosureSpec(cloref, generalize_gparams ilCloGenericParams)

    let getFreshMethod = 
        let mbody =
            CodeGenMethod cenv cgbuf.mgbuf (true,[],"GetFreshEnumerator",eenvinner,1,0,
                                            (fun cgbuf eenv -> 
                                                for fv in cloFreeVars do 
(*  TODO: Emit CompareExchange 
                                                        if (System.Threading.Interlocked.CompareExchange(&__state, 1, 0) = 0) then
                                                            (x :> IEnumerator<'T>)
                                                        else
                                                            ...
*)
                                                   /// State variables always get zero-initialized
                                                   if stateVarsSet.Contains fv then 
                                                       GenDefaultValue cenv cgbuf eenv (fv.Type,m) 
                                                   else
                                                       GenGetLocalVal cenv cgbuf eenv m fv None;
                                                CG.EmitInstr cgbuf (List.replicate ilCloFreeVars.Length Pop@ [ Push ilCloRetTyInner ]) (mk_IlxInstr (EI_newclo formalClospec));
                                                GenSequel cenv eenv.cloc cgbuf Return),
                                            m)
        mk_virtual_mdef("GetFreshEnumerator",MemAccess_public, [], mk_return ilCloEnumeratorTy, MethodBody_il mbody)
        |> AddNonUserCompilerGeneratedAttribs cenv.g
    let closeMethod = 
        // Note: We suppress the first sequence point in the body of this method since it is the initial state machine jump
        let spReq = SPSuppress
        mk_virtual_mdef("Close",MemAccess_public, [], mk_return Type_void, MethodBody_il (CodeGenMethodForExpr cenv cgbuf.mgbuf (spReq,[],"Close",eenvinner,1,0,closeExpr,discardAndReturnVoid)))
    let checkCloseMethod = 
        // Note: We suppress the first sequence point in the body of this method since it is the initial state machine jump
        let spReq = SPSuppress
        mk_virtual_mdef("get_CheckClose",MemAccess_public, [], mk_return cenv.g.ilg.typ_Bool, MethodBody_il (CodeGenMethodForExpr cenv cgbuf.mgbuf (spReq,[],"get_CheckClose",eenvinner,1,0,checkCloseExpr,Return)))
    let generateNextMethod = 
        // Note: We suppress the first sequence point in the body of this method since it is the initial state machine jump
        let spReq = SPSuppress
        // the 'next enumerator' byref arg is at arg position 1 
        let eenvinner = eenvinner |> AddStorageForLocalVals cenv.g [ (nextEnumeratorValRef.Deref, Arg 1) ]
        mk_virtual_mdef("GenerateNext",MemAccess_public, [mk_named_param("next",Type_byref ilCloEnumerableTy)], mk_return cenv.g.ilg.typ_Int32, MethodBody_il (CodeGenMethodForExpr cenv cgbuf.mgbuf (spReq,[],"GenerateNext",eenvinner,2,0,generateNextExpr,Return)))
    let lastGeneratedMethod = 
        mk_virtual_mdef("get_LastGenerated",MemAccess_public, [], mk_return ilCloSeqElemTy, MethodBody_il (CodeGenMethodForExpr cenv cgbuf.mgbuf (SPSuppress,[],"get_LastGenerated",eenvinner,1,0,expr_for_vref m currvref,Return)))
        |> AddNonUserCompilerGeneratedAttribs cenv.g
    let ilCtorBody = mk_simple_storage_ctor(None, Some ilCloBaseTy.TypeSpec, ilCloTypeSpecInner, [], MemAccess_assembly) |> ilmbody_of_mdef 

    let attrs = GenAttrs cenv eenvinner cloAttribs
    let clo = GenClosureTypeDef cenv (ilCloTypeRef,cloFreeVars,ilCloGenericParams,attrs,m,ilCloFreeVars,cloLambdas,ilCtorBody,[generateNextMethod;closeMethod;checkCloseMethod;lastGeneratedMethod;getFreshMethod],[],ilCloBaseTy,[])
    cgbuf.mgbuf.AddTypeDef(ilCloTypeRef,clo);
    CountClosure();

    for fv in cloFreeVars do 
       /// State variables always get zero-initialized
       if stateVarsSet.Contains fv then 
           GenDefaultValue cenv cgbuf eenvouter (fv.Type,m) 
       else
           GenGetLocalVal cenv cgbuf eenvouter m fv None;
       
    CG.EmitInstr cgbuf (List.replicate ilCloFreeVars.Length Pop@ [ Push ilCloRetTyOuter ]) (mk_IlxInstr (EI_newclo ilxCloSpec));
    GenSequel cenv eenvouter.cloc cgbuf sequel



/// Generate the class for a closure type definition
and GenClosureTypeDef cenv (tref:ILTypeRef, fvs:list<Val>, gparams, attrs, m, ilCloFreeVars, cloLambdas, ilCtorBody, mdefs, mimpls,ext, intfs) =

      // Closure types should not be marked as Serializable if any of their free variable fields are non-serializable
    let tdSerializable = not (fvs |> List.exists (fun fv -> is_definitely_not_serializable cenv.g fv.Type)) 
    
    { tdName = tref.Name; 
      tdLayout = TypeLayout_auto;
      tdAccess =  ComputeTypeAccess tref true;
      tdGenericParams = gparams;
      tdCustomAttrs = mk_custom_attrs(attrs @ [mk_CompilationMappingAttr cenv.g SourceLevelConstruct_Closure ]);
      tdFieldDefs = mk_fdefs [];
      tdInitSemantics=TypeInit_beforefield;         
      tdSealed=true;
      tdAbstract=false;
      tdKind=mk_IlxTypeDefKind (ETypeDef_closure  { cloSource=None;
                                                       cloFreeVars=ilCloFreeVars;  
                                                       cloStructure=cloLambdas;
                                                       cloCode=notlazy ilCtorBody });
      tdEvents= mk_events [];
      tdProperties = mk_properties [];
      tdMethodDefs= mk_mdefs mdefs; 
      tdMethodImpls= mk_mimpls mimpls; 
      tdSerializable= tdSerializable;
      tdComInterop=false;    
      tdSpecialName= true;
      tdNested=mk_tdefs [];
      tdEncoding= TypeEncoding_autochar;
      tdImplements= intfs;  
      tdExtends= Some ext;
      tdSecurityDecls= mk_security_decls [];
      tdHasSecurity=false; } 

          
and GenGenericParams m cenv tyenv tps = List.map (GenGenericParam m cenv.g tyenv) (DropErasedTypars tps)
and GenGenericArgs m eenv tps = List.map (fun c -> (mk_tyvar_ty (repr_of_typar m c eenv))) (DropErasedTypars tps)

/// Generate the closure class for a function 
and GenLambdaClosure cenv (cgbuf:CodeGenBuffer) eenv isLocalTypeFunc selfv expr =
    if verbose then dprintn ("GenLambdaClosure:");      
    match expr with 
    | TExpr_lambda (_,_,_,_,m,_,_) 
    | TExpr_tlambda(_,_,_,m,_,_) -> 
          
        let cloinfo,body,eenvinner  = GetIlxClosureInfo cenv  m isLocalTypeFunc selfv eenv expr 
          
        let entryPointInfo = 
          match selfv with 
          | Some v -> [(v, BranchCallClosure (cloinfo.clo_arity_info))]
          | _ -> []
        let clo_body = CodeGenMethodForExpr cenv cgbuf.mgbuf (SPAlways,entryPointInfo,cloinfo.clo_name,eenvinner,1,0,body,Return)
        let clo_tref = tref_of_clospec cloinfo.clo_clospec
        let clo = 
            if isLocalTypeFunc then 

                // Work out the contract type and generate a class with an abstract method for this type
                let (ilContractGenericParams,contract_meth_il_gparams,contract_actual_tspec:ILTypeSpec,contract_formal_il_rty) = GenNamedLocalTypeFuncContractInfo cenv m cloinfo
                let contract_tref = contract_actual_tspec.TypeRef
                let contract_tspec = mk_tspec(contract_tref,generalize_gparams ilContractGenericParams)
                let contract_ctor =  mk_nongeneric_nothing_ctor None cenv.g.ilg.tref_Object []

                let contract_meths = [contract_ctor; mk_generic_virtual_mdef("DirectInvoke",MemAccess_assembly,contract_meth_il_gparams,[],mk_return contract_formal_il_rty, MethodBody_abstract) ]

                let contract_tdef = 
                    { tdName = contract_tref.Name; 
                      tdLayout = TypeLayout_auto;
                      tdAccess =  ComputeTypeAccess contract_tref true;
                      tdGenericParams = ilContractGenericParams;
                      tdCustomAttrs = mk_custom_attrs([mk_CompilationMappingAttr cenv.g SourceLevelConstruct_Closure ]);
                      tdFieldDefs = mk_fdefs [];
                      tdInitSemantics=TypeInit_beforefield;         
                      tdSealed=false;  // the contract type is an abstract type and not sealed
                      tdAbstract=true; // the contract type is an abstract type
                      tdKind=TypeDef_class;
                      tdEvents= mk_events [];
                      tdProperties = mk_properties [];
                      tdMethodDefs= mk_mdefs contract_meths; 
                      tdMethodImpls= mk_mimpls []; 
                      tdSerializable= true; 
                      tdComInterop=false;    
                      tdSpecialName= true;
                      tdNested=mk_tdefs [];
                      tdEncoding= TypeEncoding_autochar;
                      tdImplements= [];  
                      tdExtends= Some cenv.g.ilg.typ_Object;
                      tdSecurityDecls= mk_security_decls [];
                      tdHasSecurity=false; } 
                cgbuf.mgbuf.AddTypeDef(contract_tref,contract_tdef);
                
                let ilCtorBody =  mk_ilmbody (true,[ ] ,8,nonbranching_instrs_to_code (mk_call_superclass_constructor([],contract_tspec)), None )
                let cloMethods = [ mk_generic_virtual_mdef("DirectInvoke",MemAccess_assembly,cloinfo.ltyfunc_direct_il_gparams,[],mk_return (cloinfo.clo_formal_il_rty), MethodBody_il clo_body) ]
                let cloTypeDef = GenClosureTypeDef cenv (clo_tref,cloinfo.clo_freevars,cloinfo.clo_il_gparams,[],m,cloinfo.clo_il_frees,cloinfo.clo_lambdas,ilCtorBody,cloMethods,[],Type_boxed contract_tspec,[])
                cloTypeDef
                
            else 
                GenClosureTypeDef cenv (clo_tref,cloinfo.clo_freevars,cloinfo.clo_il_gparams,[],m,cloinfo.clo_il_frees,cloinfo.clo_lambdas,clo_body,[],[],cenv.g.ilg.typ_Object,[])
        CountClosure();
        cgbuf.mgbuf.AddTypeDef(clo_tref,clo);
        cloinfo,m
    |     _ -> failwith "GenLambda: not a lambda"
        
and GenLambdaVal cenv (cgbuf:CodeGenBuffer) eenv (cloinfo,m) =
    if verbose then dprintn ("Loading environment for "^cloinfo.clo_name ^" in "^cgbuf.MethodName);
    GenGetLocalVals cenv cgbuf eenv m cloinfo.clo_freevars;
    if verbose then dprintn ("Compiling newclo for "^cloinfo.clo_name ^" in "^cgbuf.MethodName);
    CG.EmitInstr cgbuf (List.replicate cloinfo.clo_il_frees.Length Pop@ [ Push (Pubclo.typ_of_lambdas cenv.g.ilxPubCloEnv cloinfo.clo_lambdas)]) (* REVIEW: more specific type when ILX supports them more explicitly  *) 
           (mk_IlxInstr (EI_newclo cloinfo.clo_clospec))

and GenLambda cenv cgbuf eenv isLocalTypeFunc selfv expr sequel =
    if verbose then dprintn ("GenLambda:");   
    let cloinfo,m = GenLambdaClosure cenv cgbuf eenv isLocalTypeFunc selfv expr
    GenLambdaVal cenv cgbuf eenv (cloinfo,m);
    if verbose then dprintn ("GenLambda: done val");
    GenSequel cenv eenv.cloc cgbuf sequel

and GenTypeOfVal cenv eenv (v:Val) = 
    if verbose then dprintn ("GenTypeOfVal");
    GenType v.Range cenv.g eenv.tyenv v.Type

and GenFreevar cenv m eenvouter eenvinner (fv:Val) = 
    match storage_for_val m fv eenvouter with 
    // Local type functions
    | Local(_,Some _) | Env(_,_,Some _) -> cenv.g.ilg.typ_Object
#if DEBUG
    // Check for things that should never make it into the free variable set. Only do this in debug for performance reasons
    | (StaticField _ | Method _ |  Unrealized | Null) -> error(InternalError("GenFreevar: compiler error: unexpected unrealized value",fv.Range))
#endif
    | _ -> GenType m cenv.g eenvinner.tyenv fv.Type

and GetIlxClosureFreeVars cenv m selfv eenvouter expr =

    // Choose a base name for the closure
    let basename = 
        let boundv = eenvouter.letBoundVars |> List.tryfind (fun v -> not v.IsCompilerGenerated) 
        match boundv with
        | Some v -> v.CompiledName
        | None -> "clo"

    // Get a unique stamp for the closure. This must be stable for things that can be part of a let rec.
    let uniq = 
        match expr with 
        | TExpr_obj (uniq,_,_,_,_,_,m,_) 
        | TExpr_lambda (uniq,_,_,_,m,_,_) 
        | TExpr_tlambda(uniq,_,_,m,_,_) -> uniq
        | _ -> new_uniq()

    // Choose a name for the closure
    let ilCloTypeRef = 
        // FSharp 1.0 bug 3404: System.Reflection doesn't like '.' and '`' in type names
        let basenameSafeForUseAsTypename = basename.Replace('.', '$').Replace('`', '$') 
        let suffixmark = range_of_expr expr
        let cloName = globalStableNameGenerator.GetUniqueCompilerGeneratedName(basenameSafeForUseAsTypename,suffixmark,uniq)
        NestedTypeRefForCompLoc eenvouter.cloc cloName

    // Collect the free variables of the closure
    let cloFreeVarResults =  free_in_expr CollectTyparsAndLocals expr

    // Partition the free variables when some can be accessed from places besides the immediate environment 
    // Also filter out the current value being bound, if any, as it is available from the "this" 
    // pointer which gives the current closure itself. This is in the case e.g. let rec f = ... f ... 
    let cloFreeVars = 
        cloFreeVarResults.FreeLocals
        |> Zset.elements 
        |> List.filter (fun fv -> 
            match storage_for_val m fv eenvouter with 
            | (StaticField _ | Method _ |  Unrealized | Null) -> false
            | _ -> 
                match selfv with 
                | Some v -> not (cenv.g.vref_eq (mk_local_vref fv) v) 
                | _ -> true)

    // The general shape is:
    //    {LAM <tyfunc-typars>. expr }[free-typars] : overall-type[contract-typars]
    // Then
    //    internal-typars = free-typars - contract-typars
    //
    // In other words, the free type variables get divided into two sets
    //  -- "contract" ones, which are part of the return type. We separate these to enable use to 
    //     bake our own function base contracts for local type functions
    //
    //  -- "internal" ones, which get used internally in the implementation
    let cloContractFreeTyvarSet = (free_in_type CollectTypars (type_of_expr cenv.g expr)).FreeTypars 
    
    let cloInternalFreeTyvars = Zset.diff  cloFreeVarResults.FreeTyvars.FreeTypars cloContractFreeTyvarSet |> Zset.elements
    let cloContractFreeTyvars = cloContractFreeTyvarSet |> Zset.elements
    
    let cloFreeTyvars = cloContractFreeTyvars @ cloInternalFreeTyvars
    
    let cloAttribs = []

    // If generating a named closure, add the closure itself as a var, available via "arg0" . 
    // The latter doesn't apply for the delegate implementation of closures. 
    let eenvinner = eenvouter |> env_for_typars cloFreeTyvars

    let ilCloTypeSpecInner = 
        let ilCloGenericParams = GenGenericParams m cenv eenvinner.tyenv cloFreeTyvars
        mk_tspec (ilCloTypeRef, generalize_gparams ilCloGenericParams)

    // Build the environment that is active inside the closure itself
    let eenvinner = { eenvinner with tyenv = { eenvinner.tyenv with tyenv_nativeptr_as_nativeint=true } } 
    let eenvinner = eenvinner |> AddStorageForLocalVals cenv.g (match selfv with | Some v  -> [(deref_val v,Arg 0)] | _ -> [])
    let eenvinner = eenvinner |> AddStorageForLocalVals cenv.g 
                                            (cloFreeVars |> List.mapi (fun i v -> 
                                                let localCloInfo = 
                                                    match storage_for_val m v eenvouter with 
                                                    | Local(_,localCloInfo) 
                                                    | Env(_,_,localCloInfo) -> localCloInfo
                                                    | _ -> None
                                                (v,Env(ilCloTypeSpecInner,i,localCloInfo))) )

    
    // Return a various results
    (cloAttribs,cloInternalFreeTyvars,cloContractFreeTyvars,cloFreeTyvars,cloFreeVars,ilCloTypeRef,eenvinner)

and GetClosureILFreeVars cenv m takenNames eenvouter eenvinner cloFreeVars =
    let ilCloFreeVarNames = ChooseFreeVarNames takenNames (List.map name_of_val cloFreeVars)   
    let ilCloFreeVars = (cloFreeVars,ilCloFreeVarNames) ||> List.map2 (fun fv nm -> mk_freevar (nm,fv.IsCompilerGenerated, GenFreevar cenv m eenvouter eenvinner fv))  
    ilCloFreeVars

and GetIlxClosureInfo cenv m isLocalTypeFunc  selfv eenvouter expr =
    if verbose then dprintn ("GetIlxClosureInfo");     
    let (cloAttribs,cloInternalFreeTyvars,cloContractFreeTyvars,cloFreeTyvars,cloFreeVars,ilCloTypeRef,eenvinner) = GetIlxClosureFreeVars cenv m selfv eenvouter expr

    if verbose then dprintn ("GetIlxClosureInfo: returnTy");     
    let returnTy = 
      match expr with 
      | TExpr_lambda (_,_,_,_,_,returnTy,_) | TExpr_tlambda(_,_,_,_,returnTy,_) -> returnTy
      | TExpr_obj(_,typ,_,_,_,_,_,_) -> typ
      | _ -> failwith "GetIlxClosureInfo: not a lambda expression"

    if verbose then dprintn ("GetIlxClosureInfo: getClosureArgs");     
    let rec getClosureArgs eenv ntmargs takenNames (e,returnTy) = 
        match e with 
        | TExpr_lambda (_,_,vs,body,m,bty,_) when not isLocalTypeFunc -> 

            // Transform a lambda taking untupled arguments into one 
            // taking only a single tupled argument if necessary.  REVIEW: do this earlier 
            let tupledv, body =  multi_lambda_to_tupled_lambda vs body 
            let nm = tupledv.MangledName
            let returnTy',l,arityInfo,takenNames,(body',bty'),eenv = 
                let eenv = AddStorageForVal cenv.g (tupledv,notlazy (Arg ntmargs)) eenv
                getClosureArgs eenv (ntmargs + 1) (nm :: takenNames) (body,bty)
            returnTy',Lambdas_lambda (mk_named_param(nm,GenTypeOfVal cenv eenv tupledv),l),1 :: arityInfo,takenNames,(body',bty'),eenv

        | TExpr_tlambda(_,tvs,body,m,bty,_) -> 
            let returnTy',l,arityInfo,takenNames,body,eenv = 
                  let eenv = AddTyparsToEnv tvs eenv
                  getClosureArgs eenv ntmargs takenNames (body,bty)
            returnTy',List.foldBack (fun tv sofar ->
                  let gp = GenGenericParam m cenv.g eenv.tyenv tv
                  Lambdas_forall(gp,sofar)) tvs l,arityInfo, takenNames,body,eenv

        | _ -> 
              let returnTy' = GenType m cenv.g eenv.tyenv returnTy
              returnTy',Lambdas_return returnTy', [],takenNames,(e,returnTy),eenv

    // start at arg number 1 as "this" pointer holds the current closure
    let (ilReturnTy,cloLambdas,narginfo,takenNames,(body,_),eenvinner) = getClosureArgs eenvinner 1 [] (expr,returnTy)

    // The general shape is:
    //    {LAM <tyfunc-typars>. expr }[free-typars] : overall-type[contract-typars]
    // Then
    //    internal-typars = free-typars - contract-typars
    //
    // For a local type function closure, this becomes
    //    class Contract<contract-typars> {
    //        abstract DirectInvoke<tyfunc-typars> : overall-type
    //    }
    //
    //    class ContractImplementation<contract-typars, internal-typars> : Contract<contract-typars>  {
    //        override DirectInvoke<tyfunc-typars> : overall-type { expr }
    //    }
    //
    // For a non-local type function closure, this becomes
    //
    //    class FunctionImplementation<contract-typars, internal-typars> : TypeFunc  {
    //        override Specialize<tyfunc-typars> : overall-type { expr }
    //    }
    //
    // For a normal function closure, <tyfunc-typars> is empty, and this becomes
    //
    //    class FunctionImplementation<contract-typars, internal-typars> : overall-type<contract-typars>  {
    //        override Invoke(..) { expr }
    //    }
    
    // In other words, the free type variables get divided into two sets
    //  -- "contract" ones, which are part of the return type. We separate these to enable use to 
    //     bake our own function base contracts for local type functions
    //
    //  -- "internal" ones, which get used internally in the implementation
    //
    // There are also "direct" and "indirect" type variables, which are part of the lambdas of the type function.
    // Direct type variables are only used for local type functions, and indirect type variables only used for first class
    // function values.

    /// Compute the contract if it is a local type function
    let ilContractGenericParams  = GenGenericParams m cenv eenvinner.tyenv cloContractFreeTyvars
    let ilContractGenericActuals = GenGenericArgs m eenvouter.tyenv cloContractFreeTyvars
    let ilInternalGenericParams  = GenGenericParams m cenv eenvinner.tyenv cloInternalFreeTyvars
    let ilInternalGenericActuals = GenGenericArgs m eenvouter.tyenv cloInternalFreeTyvars

    let ilCloGenericFormals = ilContractGenericParams @ ilInternalGenericParams
    let ilCloGenericActuals = ilContractGenericActuals @ ilInternalGenericActuals

    let ilCloFreeVars = GetClosureILFreeVars cenv m takenNames eenvouter eenvinner cloFreeVars
    
    let ilDirectGenericParams,ilReturnTy,cloLambdas = 
        if isLocalTypeFunc then 
            let rec strip lambdas acc = 
                match lambdas with 
                | Lambdas_forall(gp,r) -> strip r  (gp::acc)
                | Lambdas_return returnTy -> List.rev acc,returnTy,lambdas
                | _ -> failwith "AdjustNamedLocalTypeFuncIlxClosureInfo: local functions can currently only be type functions"
            strip cloLambdas []
        else 
            [],ilReturnTy,cloLambdas
        

    let ilxCloSpec = IlxClosureSpec(IlxClosureRef(ilCloTypeRef, cloLambdas, ilCloFreeVars), ilCloGenericActuals)
    let cloinfo = 
        { clo_expr=expr;
          clo_name=ilCloTypeRef.Name;
          clo_arity_info =narginfo;
          clo_lambdas=cloLambdas;
          clo_il_frees = ilCloFreeVars;
          clo_formal_il_rty=ilReturnTy;
          clo_clospec = ilxCloSpec;
          clo_il_gparams = ilCloGenericFormals;
          clo_freevars=cloFreeVars;
          clo_attribs=cloAttribs;
          ltyfunc_contract_ftyvs = cloContractFreeTyvars;
          ltyfunc_internal_ftyvs = cloInternalFreeTyvars; 
          
          ltyfunc_contract_il_gactuals = ilContractGenericActuals;
          ltyfunc_direct_il_gparams=ilDirectGenericParams; }
    if verbose then dprintn ("<-- GetIlxClosureInfo");     
    cloinfo,body,eenvinner

//--------------------------------------------------------------------------
// Named local type functions
//-------------------------------------------------------------------------- 

and IsNamedLocalTypeFuncVal g (v:Val) expr =
    not v.IsCompiledAsTopLevel &&
    is_forall_typ g v.Type && 
    (let tps,_ = dest_forall_typ g v.Type in tps |> List.exists (fun tp -> not tp.Constraints.IsEmpty)) && 
    (match strip_expr expr with TExpr_tlambda _ -> true | _ -> false)
  
/// Generate the information relecant to the contract portion of a named local type function
and GenNamedLocalTypeFuncContractInfo cenv m cloinfo = 
    let clo_tref = tref_of_clospec cloinfo.clo_clospec
    let contract_tref = ILTypeRef.Create(scope=clo_tref.Scope,enclosing=clo_tref.Enclosing,name=clo_tref.Name^"$contract")
    let contract_tyenv  = tyenv_for_typars cloinfo.ltyfunc_contract_ftyvs
    let ilContractGenericParams = GenGenericParams m cenv contract_tyenv cloinfo.ltyfunc_contract_ftyvs
    let tvs,contract_rty  = 
        match cloinfo.clo_expr with 
        | TExpr_tlambda(_,tvs,body,m,bty,_) -> tvs, bty
        | e -> [], type_of_expr cenv.g e
    let contract_tyenv = add_typars contract_tyenv tvs
    let contract_meth_il_gparams = GenGenericParams m cenv contract_tyenv tvs
    let contract_formal_il_rty = GenType m cenv.g contract_tyenv contract_rty
    ilContractGenericParams,contract_meth_il_gparams,mk_tspec(contract_tref,cloinfo.ltyfunc_contract_il_gactuals),contract_formal_il_rty

/// Generate a new delegate construction including a clousre class if necessary. This is a lot like generating function closures
/// and object expression closures, and most of the code is shared.
and GenDelegateExpr cenv cgbuf eenvouter expr (TObjExprMethod((TSlotSig(_,delegateTy, _,_,_, _) as slotsig),methTyparsOfOverridingMethod,tmvs,body,implm),m) sequel =
    if verbose then dprintn ("GenDelegateExpr");     
    // Get the instantiation of the delegate type 
    let ctxt_il_delty = GenType m cenv.g eenvouter.tyenv delegateTy
    let tmvs = List.concat tmvs

    // Yuck. TLBIMP.EXE generated APIs use UIntPtr for the delegate ctor. 
    let useUIntPtrForDelegateCtor = 
        try 
            if is_il_named_typ cenv.g delegateTy then 
                let tcref = tcref_of_stripped_typ cenv.g delegateTy
                let _,_,tdef = tcref.ILTyconInfo
                match find_mdefs_by_name ".ctor" tdef.tdMethodDefs with 
                | [ctorMDef] -> 
                    match ctorMDef.mdParams with 
                    | [_;p2] -> (p2.paramType.TypeSpec.Name = "System.UIntPtr")
                    | _ -> false
                | _ -> false
            else 
                false 
         with _ -> 
            false
        
    // Work out the free type variables for the morphing thunk 
    let (cloAttribs,_,_,cloFreeTyvars,cloFreeVars,delegee_tref,eenvinner) = GetIlxClosureFreeVars cenv m None eenvouter expr
    let takenNames = List.map name_of_val tmvs
    let ilCloFreeVars = GetClosureILFreeVars cenv m takenNames eenvouter eenvinner cloFreeVars
    let ilDelegeeGenericParams = GenGenericParams m cenv eenvinner.tyenv cloFreeTyvars
    let ilDelegeeTypeName = delegee_tref.Name
    let ilDelegeeGenericActualsInner = generalize_gparams ilDelegeeGenericParams

    // Create a new closure class with a single "delegee" method that implements the delegate. 
    let delegeeMethName = "Invoke"
    let ilDelegeeTypeSpecInner = mk_tspec (delegee_tref, ilDelegeeGenericActualsInner)

    let delegee_eenv_under_typars = AddTyparsToEnv methTyparsOfOverridingMethod eenvinner

    // The slot sig contains a formal instantiation.  When creating delegates we're only 
    // interested in the actual instantiation since we don't have to emit a method impl. 
    let ilDelegeeParams,ilDelegeeRet = GenActualSlotsig m cenv delegee_eenv_under_typars slotsig methTyparsOfOverridingMethod

    let numthis = 1
    let delegee_meth_env = AddStorageForLocalVals cenv.g (List.mapi (fun i v -> (v,Arg (i+numthis))) tmvs)  delegee_eenv_under_typars
    let ilMethodBody = CodeGenMethodForExpr cenv cgbuf.mgbuf (SPAlways,[],delegeeMethName,delegee_meth_env,1,0,body,(if slotsig_has_void_rty slotsig then discardAndReturnVoid else Return))
    let delegeeInvokeMeth =
        mk_instance_mdef
            (delegeeMethName,MemAccess_assembly, 
             ilDelegeeParams, 
             ilDelegeeRet,
             MethodBody_il ilMethodBody)
    let delegeeCtorMeth = mk_simple_storage_ctor(None, Some cenv.g.ilg.tspec_Object, ilDelegeeTypeSpecInner, [], MemAccess_assembly)
    let ilCtorBody = ilmbody_of_mdef delegeeCtorMeth

    let cloLambdas = Lambdas_return ctxt_il_delty
    let ilAttribs = GenAttrs cenv eenvinner cloAttribs
    let clo = GenClosureTypeDef cenv (delegee_tref,cloFreeVars,ilDelegeeGenericParams,ilAttribs,m,ilCloFreeVars,cloLambdas,ilCtorBody,[delegeeInvokeMeth],[],cenv.g.ilg.typ_Object,[])
    cgbuf.mgbuf.AddTypeDef(delegee_tref,clo);
    CountClosure();

    let ctxt_gactuals_for_delegee = GenGenericArgs m eenvouter.tyenv cloFreeTyvars
    let ilxCloSpec = IlxClosureSpec(IlxClosureRef(delegee_tref, cloLambdas, ilCloFreeVars), ctxt_gactuals_for_delegee)
    GenGetLocalVals cenv cgbuf eenvouter m cloFreeVars;
    CG.EmitInstr cgbuf (List.replicate ilCloFreeVars.Length Pop@ [ Push (Pubclo.typ_of_lambdas cenv.g.ilxPubCloEnv cloLambdas)]) (mk_IlxInstr (EI_newclo ilxCloSpec));

    let ilDelegeeTypeSpecOuter = mk_tspec (delegee_tref,ctxt_gactuals_for_delegee)
    let ilDelegeeInvokeMethOuter = mk_nongeneric_instance_mspec_in_boxed_tspec (ilDelegeeTypeSpecOuter,"Invoke",typs_of_params ilDelegeeParams, ilDelegeeRet.Type)
    let ilDelegeeCtorMethOuter = mk_ctor_mspec_for_delegate cenv.g.ilg (ctxt_il_delty.TypeRef,IL.inst_of_typ ctxt_il_delty,useUIntPtrForDelegateCtor)
    CG.EmitInstrs cgbuf 
      [Push cenv.g.ilg.typ_int32; Pop; Pop; Push ctxt_il_delty]
      [ I_ldftn ilDelegeeInvokeMethOuter; 
        I_newobj(ilDelegeeCtorMethOuter,None) ];
    GenSequel cenv eenvouter.cloc cgbuf sequel

//-------------------------------------------------------------------------
// Generate statically-resolved conditionals used for type-directed optimizations.
//------------------------------------------------------------------------- 
    
and GenStaticOptimization cenv cgbuf eenv (constraints,e2,e3,m) sequel = 
    let e = 
      if DecideStaticOptimizations cenv.g constraints = 1 then e2 
      else e3
    GenExpr cenv cgbuf eenv SPSuppress e sequel


//-------------------------------------------------------------------------
// Generate discrimination trees
//------------------------------------------------------------------------- 

and IsSequelImmediate  sequel = 
    match sequel with 
    (* All of these can be done at the end of each branch - we don't need a real join point *)
    | Return | ReturnVoid | Br _ | LeaveHandler _  -> true
    | DiscardThen sequel -> IsSequelImmediate  sequel
    | _ -> false

and GenJoinPoint cenv cgbuf pos eenv ty m sequel = 
    if verbose then dprintn ("GenJoinPoint");      
    match sequel with 
    // All of these can be done at the end of each branch - we don't need a real join point 
    | _ when IsSequelImmediate sequel -> 
        let stackAfterJoin = cgbuf.GetCurrentStack()
        let afterJoin = CG.EmitDelayMark cgbuf (pos^"_join") 
        sequel,afterJoin,stackAfterJoin,Continue

    // We end scopes at the join point, if any 
    | EndLocalScope(sq,mark) -> 
        let sequel_now,afterJoin,stackAfterJoin,sequelAfterJoin = GenJoinPoint cenv cgbuf pos eenv ty m sq 
        sequel_now,afterJoin,stackAfterJoin,EndLocalScope(sequelAfterJoin,mark)

    // If something non-trivial happens after a discard then generate a join point, but first discard the value (often this means we won't generate it at all) 
    | DiscardThen sequel -> 
        let stackAfterJoin =  cgbuf.GetCurrentStack()
        let afterJoin = CG.EmitDelayMark cgbuf (pos^"_join") 
        DiscardThen (Br afterJoin),afterJoin,stackAfterJoin,sequel
 
    // The others (e.g. Continue, LeaveFilter and CmpThenBrOrContinue) can't be done at the end of each branch. We must create a join point. 
    | _ -> 
        let pushed = GenType m cenv.g eenv.tyenv ty
        let stackAfterJoin = (pushed :: (cgbuf.GetCurrentStack()))
        let afterJoin = CG.EmitDelayMark cgbuf (pos^"_join") 
        // go to the join point 
        Br afterJoin, afterJoin,stackAfterJoin,sequel
        
and GenMatch cenv cgbuf eenv (spBind,exprm,tree,targets,m,ty) sequel =
    if verbose then dprintf "GenMatch, dtree = %s\n" (showL (DecisionTreeL tree));      

    match spBind with 
    | SequencePointAtBinding m -> CG.EmitSeqPoint cgbuf m
    | NoSequencePointAtDoBinding
    | NoSequencePointAtLetBinding
    | NoSequencePointAtInvisibleBinding 
    | NoSequencePointAtStickyBinding -> ()

    // The target of branch needs a sequence point.
    // If we don't give it one it will get entirely the wrong sequence point depending on earlier codegen
    // Note we're not interested in having pattern matching and decision trees reveal their inner working.
    // Hence at each branch target we 'reassert' the overall sequence point that was active as we came into the match.
    //
    // NOTE: sadly this causes multiple sequence points to appear for the "initial" location of an if/then/else or match.
    let activeSP = cgbuf.GetLastSequencePoint()
    let repeatSP() = 
        match activeSP with 
        | None -> () 
        | Some src -> 
            if activeSP <> cgbuf.GetLastSequencePoint() then 
                CG.EmitSeqPoint cgbuf src

    // First try the common cases where we don't need a join point. 
    match tree with 
    | TDSuccess(es,n) -> 
        failwith "internal error: matches that immediately succeed should have been normalized using mk_and_optimize_match"

    | _ -> 
        // Create a join point 
        let stackAtTargets = cgbuf.GetCurrentStack() in (* the stack at the r.h.s. of each clause *)  
        let (sequelOnBranches,afterJoin,stackAfterJoin,sequelAfterJoin) = GenJoinPoint cenv cgbuf "match" eenv ty m sequel

        // Stack: "stackAtTargets" is "stack prior to any match-testing" and also "stack at the start of each branch-RHS".
        //        match-testing (dtrees) should not contribute to the stack.
        //        Each branch-RHS (targets) may contribute to the stack, leaving it in the "stackAfterJoin" state, for the join point.
        //        Since code is branching and joining, the cgbuf stack is maintained manually.
        GenDecisionTreeAndTargets cenv cgbuf stackAtTargets eenv tree targets repeatSP sequelOnBranches; 
        CG.SetMarkToHere cgbuf afterJoin;

        //assert(cgbuf.GetCurrentStack() = stackAfterJoin);  // REVIEW: Since gen_dtree* now sets stack, stack should be stackAfterJoin at this point...
        CG.SetStack cgbuf stackAfterJoin;             
        GenSequel cenv eenv.cloc cgbuf sequelAfterJoin

// Accumulate the decision graph as we go
and GenDecisionTreeAndTargets cenv cgbuf stackAtTargets eenv tree targets repeatSP sequel = 
    ignore (GenDecisionTreeAndTargetsInner cenv cgbuf (CG.EmitDelayMark cgbuf "start_dtree") stackAtTargets eenv tree targets repeatSP (Imap.empty()) sequel)

and get_prev_target rgraph n =  Imap.tryfind n rgraph 

and GenDecisionTreeAndTargetsInner cenv cgbuf inplab stackAtTargets eenv tree targets repeatSP rgraph sequel = 
    if verbose then dprintf "GenDecisionTreeAndTargetsInner, dtree = %s\n" (showL (DecisionTreeL tree));    
    CG.SetStack cgbuf stackAtTargets;              // Set the expected initial stack.
    match tree with 
    | TDBind(bind,rest) -> 
       CG.SetMarkToHere cgbuf inplab;
       let startScope,endScope as scopeMarks = StartDelayedLocalScope "dtree_bind" cgbuf
       let eenv = AllocStorageForBind cenv cgbuf scopeMarks eenv bind
       let sp = GenSequencePointForBind cenv cgbuf eenv bind
       CG.SetMarkToHere cgbuf startScope;
       GenBindAfterSequencePoint cenv cgbuf eenv sp bind;
       // We don't get the scope marks quite right for dtree-bound variables.  This is because 
       // we effectively lose an EndLocalScope for all dtrees that go to the same target 
       // So we just pretend that the variable goes out of scope here. 
       CG.SetMarkToHere cgbuf endScope;
       let bodyLabel = CG.EmitDelayMark cgbuf "decisionTreeBindBody"
       CG.EmitInstr cgbuf [] (I_br (code_label_of_mark bodyLabel)); 
       GenDecisionTreeAndTargetsInner cenv cgbuf bodyLabel stackAtTargets eenv rest targets repeatSP rgraph sequel

    | TDSuccess (es,n) ->  
       GenDecisionTreeSuccess cenv cgbuf inplab stackAtTargets eenv es n targets repeatSP rgraph sequel 

    | TDSwitch(e, cases, dflt,m)  -> 
       GenDecisionTreeSwitch cenv cgbuf inplab stackAtTargets eenv e cases dflt m targets repeatSP rgraph sequel 

and GetTarget targets n =
    if n >= Array.length targets then failwith "GetTarget: target not found in decision tree";
    targets.[n]

and GenDecisionTreeSuccess cenv cgbuf inplab stackAtTargets eenv es n targets repeatSP rgraph sequel = 
    if verbose then dprintn ("GenDecisionTreeSuccess");          
    let (TTarget(vs,successExpr,spTarget)) = GetTarget targets n
    match get_prev_target rgraph n with
    | Some (success,eenvrhs) ->

        // If not binding anything we can go directly to the success point 
        // This is useful to avoid lots of branches e.g. in match A | B | C -> e 
        // In this case each case will just go straight to "e" 
        if FlatList.isEmpty vs then 
            CG.SetMark cgbuf inplab success;
            rgraph
        else 
            CG.SetMarkToHere cgbuf inplab;
            repeatSP();
            FlatList.iter2 (GenSetBindValue cenv cgbuf eenvrhs eenv ) vs es;
            CG.EmitInstr cgbuf [] (I_br (code_label_of_mark success)); 
            rgraph
    | None -> 
        CG.SetMarkToHere cgbuf inplab;
        // Repeat the sequence point to make sure each target branch has some sequence point (instead of inheriting
        // a random sequence point from the previously generated IL code from the previous block. See comment on 
        // repeatSP() above.
        //
        // Only repeat the sequence point if we really have to, i.e. if the target expression doesn't start with a
        // sequence point anyway
        let spTarget = (match spTarget with SequencePointAtTarget -> SPAlways | SuppressSequencePointAtTarget _ -> SPSuppress)
        if isNil vs && DoesGenExprStartWithSequencePoint spTarget successExpr then 
           () 
        else 
           repeatSP(); 
        let binds = mk_invisible_FlatBindings vs es
        let _,endScope as scopeMarks = StartLocalScope "matchrhs" cgbuf
        let eenvrhs = AllocStorageForBinds cenv cgbuf scopeMarks eenv binds
        GenBindings cenv cgbuf eenvrhs binds;
        let success = CG.GenerateMark cgbuf "matching_rhs"
        CG.SetStack cgbuf stackAtTargets;
        GenExpr cenv cgbuf eenvrhs spTarget successExpr (EndLocalScope(sequel,endScope));
        // add the generated rhs. to the graph 
        Imap.add n (success,eenvrhs) rgraph

and GenDecisionTreeSwitch cenv cgbuf inplab stackAtTargets eenv e cases dflt_opt switchm targets repeatSP rgraph sequel = 
    let m = range_of_expr e
    CG.SetMarkToHere cgbuf inplab;

    repeatSP();
    match cases with 
      // optimize a test against a boolean value, i.e. the all-important if-then-else 
      | TCase(TTest_const(TConst_bool b), success_dtree) :: _  ->  
       let failure_dtree = (match dflt_opt with None -> dest_of_case (List.hd (List.tl cases)) | Some d -> d)
       GenDecisionTreeTest cenv eenv.cloc cgbuf stackAtTargets e None eenv (if b then success_dtree else  failure_dtree) (if b then failure_dtree else success_dtree) targets repeatSP rgraph sequel 

      // optimize a single test for a type constructor to an "isdata" test - much 
      // more efficient code, and this case occurs in the generated equality testers where perf is important 
      | TCase(TTest_unionconstr(c,tyargs), success_dtree) :: rest when List.length rest = (match dflt_opt with None -> 1 | Some x -> 0)  ->  
        let failure_dtree = if dflt_opt = None then dest_of_case (List.hd (List.tl cases)) else the dflt_opt 
        let cuspec = GenUnionSpec m cenv.g eenv.tyenv c.TyconRef tyargs
        let idx = ucref_index c
        GenDecisionTreeTest cenv eenv.cloc cgbuf stackAtTargets e (Some ([Pop; Push cenv.g.ilg.typ_bool],(mk_IlxInstr (EI_isdata (cuspec, idx))))) eenv success_dtree failure_dtree targets repeatSP rgraph sequel

      | _ ->  
        let caseLabels = List.map (fun _ -> CG.EmitDelayMark cgbuf "switch_case") cases
        let dflt_label = match dflt_opt with None -> List.hd caseLabels | Some _ -> CG.EmitDelayMark cgbuf "switch_dflt"
        let fst_discrim =  discrim_of_case (List.hd cases)
        match fst_discrim with 
        // Iterated tests, e.g. exception constructors, nulltests, typetests and active patterns.
        // These should always have one positive and one negative branch 
        | TTest_isinst _  
        | TTest_array_length _
        | TTest_isnull 
        | TTest_const(TConst_zero) -> 
            if List.length cases <> 1 || isNone dflt_opt then failwith "internal error: GenDecisionTreeSwitch: TTest_isinst/isnull/query";
            let bi = 
              match fst_discrim with 
              | TTest_const(TConst_zero) ->
                  GenExpr cenv cgbuf eenv SPSuppress e Continue; 
                  BI_brfalse
              | TTest_isnull -> 
                  GenExpr cenv cgbuf eenv SPSuppress e Continue; 
                  let srcTy = type_of_expr cenv.g e
                  if is_typar_typ cenv.g srcTy then 
                      let ilFromTy = GenType m cenv.g eenv.tyenv srcTy
                      CG.EmitInstr cgbuf [Pop; Push cenv.g.ilg.typ_Object] (I_box ilFromTy);
                  BI_brfalse
              | TTest_isinst (srcty,tgty) -> 
                  let e = mk_call_istype cenv.g m tgty e
                  GenExpr cenv cgbuf eenv SPSuppress e Continue;
                  BI_brtrue
              | _ -> failwith "internal error: GenDecisionTreeSwitch"
            CG.EmitInstr cgbuf [Pop] (I_brcmp (bi,code_label_of_mark (List.hd caseLabels),code_label_of_mark dflt_label));
            GenDecisionTreeCases cenv cgbuf stackAtTargets eenv targets repeatSP rgraph caseLabels cases dflt_opt dflt_label sequel
              
        | TTest_query _ -> error(Error("internal error in codegen: TTest_query",switchm))
        | TTest_unionconstr (hdc,tyargs) -> 
            GenExpr cenv cgbuf eenv SPSuppress e Continue;
            let cuspec = GenUnionSpec m cenv.g eenv.tyenv hdc.TyconRef tyargs
            let dests = 
              if cases.Length <> caseLabels.Length then failwith "internal error: TTest_unionconstr";
              (cases , caseLabels) ||> List.map2 (fun case label  ->
                  match case with 
                  | TCase(TTest_unionconstr (c,_),_) -> (ucref_index c, code_label_of_mark label) 
                  | _ -> failwith "error: mixed constructor/const test?") 
            
            CG.EmitInstr cgbuf [Pop] (mk_IlxInstr (EI_datacase (false,cuspec,dests, code_label_of_mark dflt_label)));
            GenDecisionTreeCases cenv cgbuf stackAtTargets eenv  targets repeatSP rgraph caseLabels cases dflt_opt dflt_label sequel
              
        | TTest_const c ->
            GenExpr cenv cgbuf eenv SPSuppress e Continue;
            match c with 
            | (TConst_bool b) -> failwith "should have been done earlier"
            | (TConst_sbyte _)            
            | (TConst_int16 _)           
            | (TConst_int32 _)           
            | (TConst_byte _)           
            | (TConst_uint16 _)          
            | (TConst_uint32 _)          
            | (TConst_char _) ->
                if List.length cases <> List.length caseLabels then failwith "internal error: ";
                let dests = 
                  (cases,caseLabels) ||> List.map2 (fun case label  ->
                      let i = 
                        match discrim_of_case case with 
                          TTest_const c' ->
                            match c' with 
                            | TConst_sbyte i -> int32 i
                            | TConst_int16 i -> int32 i
                            | TConst_int32 i -> i
                            | TConst_byte i -> int32 i
                            | TConst_uint16 i -> int32 i
                            | TConst_uint32 i -> int32 i
                            | TConst_char c -> int32 c  
                            | _ -> failwith "internal error: badly formed const test"  

                        | _ -> failwith "internal error: badly formed const test" 
                      (i,code_label_of_mark label))
                let mn = List.foldBack (fst >> Operators.min) dests (fst(List.hd dests))
                let mx = List.foldBack (fst >> Operators.max) dests (fst(List.hd dests))
                // Check if it's worth using a switch 
                // REVIEW: this is using switches even for single integer matches! 
                if mx - mn = (List.length dests - 1) then
                    let dest_labels = dests |> List.sortBy fst |> List.map snd 
                    if mn <> 0 then 
                      CG.EmitInstrs cgbuf [Push cenv.g.ilg.typ_int32; Pop] [ mk_ldc_i32 mn;I_arith AI_sub ];
                    CG.EmitInstr cgbuf [Pop] (I_switch (dest_labels, code_label_of_mark dflt_label));
                else
                  error(InternalError("non-dense integer matches not implemented in codegen - these should have been removed by the pattern match compiler",switchm));
                GenDecisionTreeCases cenv cgbuf stackAtTargets eenv  targets repeatSP rgraph caseLabels cases dflt_opt dflt_label sequel
            | _ -> error(InternalError("these matches should never be needed",switchm))

and GenDecisionTreeCases cenv cgbuf stackAtTargets eenv targets repeatSP rgraph caseLabels cases dflt_opt dflt_label sequel =
    assert(cgbuf.GetCurrentStack() = stackAtTargets); // cgbuf stack should be unchanged over tests. [bug://1750].
    let rgraph = 
      match dflt_opt with 
      | Some dflt_rhs -> GenDecisionTreeAndTargetsInner cenv cgbuf dflt_label stackAtTargets eenv dflt_rhs targets repeatSP rgraph sequel
      | None -> rgraph
    let rgraph = 
      List.fold_left2 
        (fun rgraph case_label (TCase(_,case_rhs)) -> 
          GenDecisionTreeAndTargetsInner cenv cgbuf case_label stackAtTargets eenv case_rhs targets repeatSP rgraph sequel)
        rgraph
        caseLabels
        cases
    rgraph 

and (|BoolExpr|_|) = function TExpr_const(TConst_bool b1,_,_) -> Some(b1) | _ -> None

and GenDecisionTreeTest cenv cloc cgbuf stackAtTargets e tester eenv success_dtree failure_dtree targets repeatSP rgraph sequel =
    match success_dtree,failure_dtree with 
    // Peephole: if generating a boolean value or its negation then just leave it on the stack 
    // This comes up in the generated equality functions.  REVIEW: do this as a peephole optimization elsewhere 
    | TDSuccess(es1,n1), 
      TDSuccess(es2,n2) when 
         FlatList.isEmpty es1 && FlatList.isEmpty es2 &&
         (match GetTarget targets n1, GetTarget targets n2 with 
           TTarget(_,BoolExpr(b1),_),TTarget(_,BoolExpr(b2),_) -> b1 = not b2
         | _ -> false) ->

             match GetTarget targets n1, GetTarget targets n2 with 
             | TTarget(_,BoolExpr(b1),_),_ -> 
                 GenExpr cenv cgbuf eenv SPSuppress e Continue;
                 (match tester with Some (pushpop,i) -> CG.EmitInstr cgbuf pushpop i; | _ -> ());
                 if not b1 then 
                   CG.EmitInstrs cgbuf [Push cenv.g.ilg.typ_bool; Pop] [mk_ldc_i32 (0); I_arith AI_ceq];
                 GenSequel cenv cloc cgbuf sequel;
                 rgraph
             | _ -> failwith "internal error: GenDecisionTreeTest during bool elim"

    | _ ->
        let success = CG.EmitDelayMark cgbuf "test_success"
        let failure = CG.EmitDelayMark cgbuf "test_failure"
        (match tester with 
        | None -> 
            (* generate the expression, then test it for "false" *)
            GenExpr cenv cgbuf eenv SPSuppress e (CmpThenBrOrContinue([Pop],I_brcmp (BI_brfalse, code_label_of_mark failure,code_label_of_mark success)));

        (* Turn "EI_isdata" tests that branch into EI_brisdata tests *)
        | Some (_,I_other i) when is_ilx_ext_instr i && (match dest_ilx_ext_instr i with EI_isdata _ -> true | _ -> false) ->
            let (cuspec,idx) = match dest_ilx_ext_instr i with EI_isdata (cuspec,idx) -> (cuspec,idx) | _ -> failwith "??"
            GenExpr cenv cgbuf eenv SPSuppress e (CmpThenBrOrContinue([Pop],mk_IlxInstr (EI_brisdata (cuspec, idx, code_label_of_mark success,code_label_of_mark failure))));
        | Some (pushpop,i) ->
            GenExpr cenv cgbuf eenv SPSuppress e Continue;
            CG.EmitInstr cgbuf pushpop i;
            CG.EmitInstr cgbuf [Pop] (I_brcmp (BI_brfalse, code_label_of_mark failure,code_label_of_mark success)));
        let rgraph = GenDecisionTreeAndTargetsInner cenv cgbuf success stackAtTargets eenv success_dtree targets repeatSP rgraph sequel
        GenDecisionTreeAndTargetsInner cenv cgbuf failure stackAtTargets eenv failure_dtree targets repeatSP rgraph sequel 

//-------------------------------------------------------------------------
// Generate letrec bindings
//------------------------------------------------------------------------- 

and GenLetRecFixup cenv cgbuf eenv (ilxCloSpec,e,n,e2,m) =
    GenExpr cenv cgbuf eenv SPSuppress  e Continue;
    CG.EmitInstrs cgbuf [] [ mk_IlxInstr (EI_castclo ilxCloSpec) ];
    GenExpr cenv cgbuf eenv SPSuppress  e2 Continue;
    CG.EmitInstrs cgbuf [Pop; Pop] [ mk_IlxInstr (EI_stclofld(ilxCloSpec, n)) ]

and GenLetRecBinds cenv cgbuf eenv (allBinds: Bindings,m) =
    (* Fix up recursion for non-toplevel recursive bindings *)
    let bindsPossiblyRequiringFixup = 
        allBinds |> FlatList.filter (fun b -> 
            match (storage_for_val m b.Var eenv) with  
            | Method _ 
            | Unrealized 
            (* Note: Recursive data stored in static fields may require fixups e.g. let x = C(x) *) 
            (* | StaticField _  *)
            | Null -> false 
            | _ -> true)

    let computeFixupsForOneRecursiveVar boundv forwardReferenceSet fixups selfv access set e =
        match e with 
        | TExpr_lambda _ | TExpr_tlambda _ | TExpr_obj _ -> 
            let isLocalTypeFunc = (isSome selfv && (IsNamedLocalTypeFuncVal cenv.g (the selfv) e))
            let selfv = (match e with TExpr_obj _ -> None | _ when isLocalTypeFunc -> None | _ -> Option.map mk_local_vref selfv)
            let clo,_,eenvclo =  GetIlxClosureInfo cenv m isLocalTypeFunc selfv {eenv with  letBoundVars=(mk_local_vref boundv)::eenv.letBoundVars}  e 
            clo.clo_freevars |> List.iter (fun fv -> 
                if Zset.mem fv forwardReferenceSet then 
                    match storage_for_val m fv eenvclo with
                    | Env (_,n,_) -> fixups := (boundv, fv, (fun () -> GenLetRecFixup cenv cgbuf eenv (clo.clo_clospec,access,n,expr_for_val m fv,m))) :: !fixups
                    | _ -> error (InternalError("GenLetRec: "^fv.MangledName^" was not in the environment",m)) )
              
        | TExpr_val  (vref,_,m) -> 
            let fv = deref_val vref
            let needsFixup = Zset.mem fv forwardReferenceSet
            if needsFixup then fixups := (boundv, fv,(fun () -> GenExpr cenv cgbuf eenv SPSuppress  (set e) discard)) :: !fixups
        | _ -> failwith "compute real fixup vars"


    let fixups = ref []
    let recursiveVars = Zset.addFlatList (bindsPossiblyRequiringFixup |> FlatList.map (fun v -> v.Var)) (Zset.empty val_spec_order)
    FlatList.fold 
        (fun forwardReferenceSet (bind:Binding) ->
            let valBeingDefined = bind.Var
            // compute fixups 
            bind.Expr |> iter_letrec_fixups cenv.g (Some valBeingDefined)  (computeFixupsForOneRecursiveVar valBeingDefined forwardReferenceSet fixups) (expr_for_val m valBeingDefined, (fun e -> failwith ("internal error: should never need to set non-delayed recursive val: " ^ valBeingDefined.MangledName)));
            Zset.remove valBeingDefined forwardReferenceSet)
        recursiveVars
        bindsPossiblyRequiringFixup |> ignore;

    FlatList.fold
        (fun forwardReferenceSet (bind:Binding) ->
            let valBeingDefined = bind.Var
            if verbose then dprintn ("GenLetRec: generate binding for "^showL(valL valBeingDefined));
            GenBind cenv cgbuf eenv bind;
            // execute and discard any fixups that can now be committed 
            let forwardReferenceSet = Zset.remove valBeingDefined forwardReferenceSet
            fixups := !fixups |> List.filter (fun (boundv, fv, action) -> if (Zset.mem boundv forwardReferenceSet or Zset.mem fv forwardReferenceSet) then  true else (action(); false));
            forwardReferenceSet)
        recursiveVars
        allBinds |> ignore;


and GenLetRec cenv cgbuf eenv (binds,body,m) sequel =
    let _,endScope as scopeMarks = StartLocalScope "letrec" cgbuf
    if verbose then dprintn ("GenLetRec");
    let eenv = AllocStorageForBinds cenv cgbuf scopeMarks eenv binds
    GenLetRecBinds cenv cgbuf eenv (binds,m);
    if verbose then dprintn ("GenLetRec: body");      
    
    let sp = if FlatList.exists bindHasSeqPt binds || FlatList.forall bindIsInvisible binds then SPAlways else SPSuppress 
    GenExpr cenv cgbuf eenv sp body (EndLocalScope(sequel,endScope))

//-------------------------------------------------------------------------
// Generate simple bindings
//------------------------------------------------------------------------- 

and GenSequencePointForBind cenv cgbuf eenv (TBind(vspec,e,spBind)) =

    let emitSP() =
        match spBind,e with 
        | (( NoSequencePointAtInvisibleBinding 
           | NoSequencePointAtStickyBinding),_) -> SPSuppress
        | (NoSequencePointAtDoBinding,_) -> SPAlways
        | (NoSequencePointAtLetBinding,_) -> SPSuppress
        // Don't emit sequence points for lambdas.
        // SEQUENCE POINT REVIEW: don't emit for lazy either, nor any builder expressions
        | _, (TExpr_lambda _ | TExpr_tlambda _) -> SPSuppress
        | SequencePointAtBinding m,_ -> 
            CG.EmitSeqPoint cgbuf m;
            SPSuppress
    
    let m = vspec.Range
   
    match storage_for_val m vspec eenv with 
    | Unrealized -> SPSuppress
    | Null -> emitSP() 
    | Method (topValInfo,_,mspec,_,paramInfos,retInfo)  -> SPSuppress
    | StaticField _ ->  emitSP() 
    | _ -> emitSP() 

and GenBind cenv cgbuf eenv bind =
    let sp = GenSequencePointForBind cenv cgbuf eenv bind
    GenBindAfterSequencePoint cenv cgbuf eenv sp bind

and GenBindAfterSequencePoint cenv cgbuf eenv sp (TBind(vspec,e,spBind)) =
    if verbose then dprintn ("GenBind");     

    // Record the closed reflection definition if publishing 
    // There is no real reason we're doing this so late in the day
    match vspec.PublicPath, vspec.ReflectedDefinition with 
    | Some p, Some e -> cgbuf.mgbuf.AddReflectedDefinition(vspec,e)
    | _  -> ()

    if verbose then dprintn ("GenBind: " ^ showL(vspecAtBindL vspec));      
    let eenv = {eenv with letBoundVars= (mk_local_vref vspec) :: eenv.letBoundVars}
    //let access = ComputeMemberAccess (not vspec.IsOverride) (IsHiddenVal eenv.sigToImplRemapInfo vspec || not vspec.IsMemberOrModuleBinding)
    let access = 
        let isHidden =  
             IsHiddenVal eenv.sigToImplRemapInfo vspec ||  // anything hiden by a signature gets assembly visibility 
             not vspec.IsMemberOrModuleBinding ||          // anything that's not a module or member binding gets assembly visibility
             vspec.IsIncrClassGeneratedMember              // compiler generated members for class function 'let' bindings get assembly visibility
        ComputeMemberAccess (not vspec.IsOverride) isHidden

    // Workaround for .NET and Visual Studio restriction w.r.t debugger type proxys
    // Mark internal constructors in internal classes as public. 
    let access = 
        if access = MemAccess_assembly && vspec.IsConstructor && IsHiddenTycon eenv.sigToImplRemapInfo vspec.MemberApparentParent.Deref then 
            MemAccess_public
        else
            access
    
    let m = vspec.Range
   
    match storage_for_val m vspec eenv with 
    | Unrealized -> ()

    | Null -> 
        GenExpr cenv cgbuf eenv SPSuppress e discard

    | Method (topValInfo,_,mspec,_,paramInfos,retInfo)  ->
        let tps,baseValOpt,vsl,body',bodyty = IteratedAdjustArityOfLambda cenv.g cenv.amap topValInfo e
        let methodVars = List.concat vsl
        GenMethodForBinding cenv cgbuf eenv (vspec,mspec,access,paramInfos,retInfo) (topValInfo,baseValOpt,tps,methodVars, body', bodyty)

    | StaticField (fspec,vref,hasLiteralAttr,ilTypeSpecForProperty,fieldName,propName,fty,ilGetterMethRef,ilSetterMethRef,optShadowLocal) ->  
        let mut = vspec.IsMutable
        
        match mut,hasLiteralAttr,e with 
        | _,false,_ -> ()
        | true,true,_ -> errorR(Error("Values marked with 'LiteralAttribute' may not be mutable",m)) 
        | _,true,TExpr_const _ -> ()
        | _,true,_ -> errorR(Error("Values marked with 'LiteralAttribute' must currently be simple integer, character, Boolean, string or floating point constants",m)) 

        /// Generate a static field definition and the get/set properties to access it. 
        
        let ilAttribs = GenAttrs cenv eenv (vspec.Attribs)
        let fieldDef = 
            let access = ComputeMemberAccess true (not hasLiteralAttr or IsHiddenVal eenv.sigToImplRemapInfo vspec)
            let fieldDef = mk_static_fdef(fieldName,fty, None, None, access)
            { fieldDef with 
               fdCustomAttrs = mk_custom_attrs (ilAttribs @ [ mk_DebuggerBrowsableNeverAttribute cenv.g.ilg ]) }
          

        let fieldDef =
            match hasLiteralAttr,e with 
            | false,_ -> fieldDef
            | true,TExpr_const(konst,m,_) -> { fieldDef with fdLiteral=true; fdInit= Some(GenFieldInit m konst) }
            | true,_ -> fieldDef (* error given above *)
          
        let ilTypeRefForProperty = ilTypeSpecForProperty.TypeRef

        let fieldDef = 
            let isClassInitializer = (cgbuf.MethodName = ".cctor")
            if mut || (not isClassInitializer) || hasLiteralAttr then 
                fieldDef 
            else 
                {fieldDef with fdInitOnly=true }

        cgbuf.mgbuf.AddFieldDef(fspec.EnclosingTypeRef,fieldDef);
        CountStaticFieldDef();

        if not hasLiteralAttr then 
            let ilPropertyDef = 
                { propName=propName;
                  propRTSpecialName=false;
                  propSpecialName=false;
                  propSet=if mut then Some(ilSetterMethRef) else None;
                  propGet=Some(ilGetterMethRef);
                  propCallconv=CC_static;
                  propType=fty;          
                  propInit=None;
                  propArgs=[];
                  propCustomAttrs=mk_custom_attrs (ilAttribs @ [mk_CompilationMappingAttr cenv.g SourceLevelConstruct_Value]); }
            cgbuf.mgbuf.AddOrMergePropertyDef(ilTypeRefForProperty,ilPropertyDef,m);

            let getterMethod = 
                mk_static_mdef([],ilGetterMethRef.Name,access,[],mk_return fty,
                               MethodBody_il(mk_ilmbody(true,[],2,nonbranching_instrs_to_code([ mk_normal_ldsfld fspec ]),None))) 
                |> AddSpecialNameFlag
            cgbuf.mgbuf.AddMethodDef(ilTypeRefForProperty,getterMethod) ;
            if mut then 
                let setterMethod = 
                    mk_static_mdef([],ilSetterMethRef.Name,access,[mk_named_param("value",fty)],mk_return Type_void,
                                   MethodBody_il(mk_ilmbody(true,[],2,nonbranching_instrs_to_code([ ldarg_0;mk_normal_stsfld fspec]),None)))
                    |> AddSpecialNameFlag
                cgbuf.mgbuf.AddMethodDef(ilTypeRefForProperty,setterMethod)

            GenBindRhs cenv cgbuf eenv sp vspec e;
            match optShadowLocal with
            | NoShadowLocal -> EmitSetStaticField cgbuf fspec
            | ShadowLocal storage->  
                CG.EmitInstr cgbuf [Push fty]  i_dup
                EmitSetStaticField cgbuf fspec
                GenSetStorage cenv m cgbuf storage

    | _ ->
        GenSetBindValue cenv cgbuf eenv eenv vspec e

//-------------------------------------------------------------------------
// Generate method bindings
//------------------------------------------------------------------------- 

/// Spectacularly gross table encoding P/Invoke and COM marshalling information 
and GenMarshal cenv attribs = 
    let otherAttribs = 
        attribs 
        |> List.filter (IsMatchingAttrib cenv.g cenv.g.attrib_MarshalAsAttribute >> not)

    match TryFindAttrib cenv.g cenv.g.attrib_MarshalAsAttribute attribs with
    | Some (Attrib(_,_,[ AttribInt32Arg unmanagedType ],namedArgs,m))  -> 
        let decoder = AttributeDecoder namedArgs
        let rec decodeUnmanagedType unmanagedType = 
           (* enumeration values for System.Runtime.InteropServices.UnmanagedType taken from mscorlib.il *)
            match  unmanagedType with 
            | 0x0 -> NativeType_empty
            | 0x01 -> NativeType_void
            | 0x02 -> NativeType_bool
            | 0x03 -> NativeType_int8
            | 0x04 -> NativeType_unsigned_int8
            | 0x05 -> NativeType_int16
            | 0x06 -> NativeType_unsigned_int16
            | 0x07 -> NativeType_int32
            | 0x08 -> NativeType_unsigned_int32
            | 0x09 -> NativeType_int64
            | 0x0A -> NativeType_unsigned_int64
            | 0x0B -> NativeType_float32
            | 0x0C -> NativeType_float64
            | 0x0F -> NativeType_currency
            | 0x13 -> NativeType_bstr
            | 0x14 -> NativeType_lpstr
            | 0x15 -> NativeType_lpwstr
            | 0x16 -> NativeType_lptstr
            | 0x17 -> NativeType_fixed_sysstring (decoder.FindInt32 "SizeConst" 0x0)
            | 0x19 -> NativeType_iunknown
            | 0x1A -> NativeType_idsipatch
            | 0x1B -> NativeType_struct
            | 0x1C -> NativeType_interface
            | 0x1D -> 
                let safeArraySubType = 
                    match decoder.FindInt32 "SafeArraySubType" 0x0 with 
                    (* enumeration values for System.Runtime.InteropServices.VarType taken from mscorlib.il *)
                    | 0x0 -> VariantType_empty
                    | 0x1 -> VariantType_null                  
                    | 0x02 -> VariantType_int16                
                    | 0x03 -> VariantType_int32                
                    | 0x0C -> VariantType_variant              
                    | 0x04 -> VariantType_float32              
                    | 0x05 -> VariantType_float64              
                    | 0x06 -> VariantType_currency             
                    | 0x07 -> VariantType_date                 
                    | 0x08 -> VariantType_bstr                 
                    | 0x09 -> VariantType_idispatch            
                    | 0x0a -> VariantType_error                
                    | 0x0b -> VariantType_bool                 
                    | 0x0d -> VariantType_iunknown             
                    | 0x0e -> VariantType_decimal              
                    | 0x10 -> VariantType_int8                 
                    | 0x11 -> VariantType_unsigned_int8        
                    | 0x12 -> VariantType_unsigned_int16       
                    | 0x13 -> VariantType_unsigned_int32       
                    | 0x15 -> VariantType_unsigned_int64       
                    | 0x16 -> VariantType_int                  
                    | 0x17 -> VariantType_unsigned_int         
                    | 0x18 -> VariantType_void                 
                    | 0x19 -> VariantType_hresult              
                    | 0x1a -> VariantType_ptr                  
                    | 0x1c -> VariantType_carray               
                    | 0x1d -> VariantType_userdefined          
                    | 0x1e -> VariantType_lpstr                
                    | 0x1B -> VariantType_safearray            
                    | 0x1f -> VariantType_lpwstr               
                    | 0x24 -> VariantType_record               
                    | 0x40 -> VariantType_filetime             
                    | 0x41 -> VariantType_blob                 
                    | 0x42 -> VariantType_stream               
                    | 0x43 -> VariantType_storage              
                    | 0x44 -> VariantType_streamed_object      
                    | 0x45 -> VariantType_stored_object        
                    | 0x46 -> VariantType_blob_object          
                    | 0x47 -> VariantType_cf                   
                    | 0x48 -> VariantType_clsid                
                    | 0x14 -> VariantType_int64                
                    | _ -> VariantType_empty
                let safeArrayUserDefinedSubType =
                    // the argument is a System.Type obj, but it's written to MD as a UTF8 string
                    match decoder.FindTypeName "SafeArrayUserDefinedSubType" "" with 
                    | "" -> None
                    | res -> if (safeArraySubType = VariantType_idispatch) || (safeArraySubType = VariantType_iunknown) then Some(res) else None
                NativeType_safe_array(safeArraySubType,safeArrayUserDefinedSubType)
            | 0x1E -> NativeType_fixed_array  (decoder.FindInt32 "SizeConst" 0x0)
            | 0x1F -> NativeType_int
            | 0x20 -> NativeType_unsigned_int
            | 0x22 -> NativeType_byvalstr
            | 0x23 -> NativeType_ansi_bstr
            | 0x24 -> NativeType_tbstr
            | 0x25 -> NativeType_variant_bool
            | 0x26 -> NativeType_method
            | 0x28 -> NativeType_as_any
            | 0x2A -> 
               let sizeParamIndex = 
                    match decoder.FindInt16 "SizeParamIndex" -1s with 
                    | -1s -> None
                    | res -> Some ((int)res,None)
               let arraySubType = 
                    match decoder.FindInt32 "ArraySubType" -1 with 
                    | -1 -> None
                    | res -> Some (decodeUnmanagedType res)
               NativeType_array(arraySubType,sizeParamIndex) 
            | 0x2B -> NativeType_lpstruct
            | 0x2C -> 
               error(Error("Custom marshallers may not be specified in F# code. Consider using a C# helper function",m))
               (* NativeType_custom of bytes * string * string * bytes (* GUID,nativeTypeName,custMarshallerName,cookieString *) *)
               //NativeType_error  
            | 0x2D -> NativeType_error  
            | _ -> NativeType_empty
        Some(decodeUnmanagedType unmanagedType), otherAttribs
    | Some (Attrib(_,_,_,_,m))  -> 
        errorR(Error("The MarshalAs attribute could not be decoded",m));
        None, attribs 
    | _ -> 
        // No MarshalAs detected
        None, attribs 

and GenParamAttribs cenv attribs =
    let inFlag = HasAttrib cenv.g cenv.g.attrib_InAttribute attribs
    let outFlag = HasAttrib cenv.g cenv.g.attrib_OutAttribute attribs
    let optionalFlag = HasAttrib cenv.g cenv.g.attrib_OptionalAttribute attribs
    // Return the filtered attributes. Do not generate In, Out or Optional attributes 
    // as custom attributes in the code - they are implicit from the IL bits for these
    let attribs = 
        attribs 
        |> List.filter (IsMatchingAttrib cenv.g cenv.g.attrib_InAttribute >> not)
        |> List.filter (IsMatchingAttrib cenv.g cenv.g.attrib_OutAttribute >> not)
        |> List.filter (IsMatchingAttrib cenv.g cenv.g.attrib_OptionalAttribute >> not)

    let paramMarshal,attribs =  GenMarshal cenv attribs
    inFlag,outFlag,optionalFlag,paramMarshal,attribs

and GenParams cenv eenv (mspec:ILMethodSpec) (attribs:TopArgInfo list) (implValsOpt: Val list option) =
    let ilArgTys = mspec.FormalArgTypes
    let argInfosAndTypes = 
        if attribs.Length = ilArgTys.Length then List.zip ilArgTys attribs
        else ilArgTys  |> List.map (fun ilArgTy -> ilArgTy,TopValInfo.unnamedTopArg1) 

    let argInfosAndTypes = 
        match implValsOpt with 
        | Some(implVals) when (implVals.Length = ilArgTys.Length) ->
            List.map2 (fun x y -> x,Some y) argInfosAndTypes implVals
        | _ -> 
            List.map (fun x -> x,None) argInfosAndTypes

    (Set.empty,argInfosAndTypes)
    ||> List.mapfold (fun takenNames ((ilArgTy,TopArgInfo(attribs,isOpt)),implValOpt) -> 
        let inFlag,outFlag,optionalFlag,paramMarshal,attribs = GenParamAttribs cenv attribs
        
        let idOpt = (match isOpt with 
                     | Some v -> Some v 
                     | None -> match implValOpt with 
                               | Some v -> Some v.Id
                               | None -> None)

        let nmOpt,takenNames = 
            match idOpt with 
            | Some id -> 
                let nm = if takenNames.Contains(id.idText) then globalNng.FreshCompilerGeneratedName (id.idText, id.idRange) else id.idText
                Some nm, takenNames.Add(nm)
            | None -> 
                None, takenNames
            
        let param = 
            { paramName=nmOpt;
              paramType= ilArgTy;  
              paramDefault=None; (* REVIEW: support "default" attributes *)   
              paramMarshal=paramMarshal; 
              paramIn=inFlag;    
              paramOut=outFlag;  
              paramOptional=optionalFlag; 
              paramCustomAttrs= mk_custom_attrs (GenAttrs cenv eenv attribs) }

        param, takenNames)
    |> fst
    
and GenReturnInfo cenv eenv ilRetTy (TopArgInfo(attrs,_)) =
    let marshal,attrs = GenMarshal cenv attrs
    { returnType=ilRetTy;
      returnMarshal=marshal;
      returnCustomAttrs= mk_custom_attrs (GenAttrs cenv eenv attrs) }
       
and GenPropertyForMethodDef g compileAsInstance tref mdef (memberInfo:ValMemberInfo) ilArgTys ilPropTy ilAttrs =
    let name = memberInfo.PropertyName in  (* chop "get_" *)
    if verbose then dprintf "GenPropertyForMethodDef %s\n" name;
    
    { propName=name; 
      propRTSpecialName=false;
      propSpecialName=false;
      propSet=(if memberInfo.MemberFlags.MemberKind= MemberKindPropertySet then Some(mk_mref_to_mdef(tref,mdef)) else None);
      propGet=(if memberInfo.MemberFlags.MemberKind= MemberKindPropertyGet then Some(mk_mref_to_mdef(tref,mdef)) else None);
      propCallconv=(if compileAsInstance then CC_instance else CC_static);
      propType=ilPropTy;          
      propInit=None;
      propArgs= ilArgTys;
      propCustomAttrs=ilAttrs; }  

and GenEventForProperty cenv eenvForMeth (mspec:ILMethodSpec) (memberInfo:ValMemberInfo) ilAttrsThatGoOnPrimaryItem m returnTy =
    let evname = memberInfo.PropertyName
    let delegateTy = Infos.FindDelegateTypeOfPropertyEvent cenv.g cenv.amap evname m returnTy
    let ilDelegateTy = GenType m cenv.g eenvForMeth.tyenv delegateTy
    let ilThisTy = mspec.EnclosingType
    let addMethRef    = mk_mref (ilThisTy.TypeRef,mspec.CallingConv,"add_"   ^evname,0,[ilDelegateTy],Type_void)
    let removeMethRef = mk_mref (ilThisTy.TypeRef,mspec.CallingConv,"remove_"^evname,0,[ilDelegateTy],Type_void)
    { eventType = Some(ilDelegateTy); 
      eventName= evname; 
      eventRTSpecialName=false;
      eventSpecialName=false;
      eventAddOn = addMethRef; 
      eventRemoveOn = removeMethRef;
      eventFire= None;
      eventOther= [];
      eventCustomAttrs = mk_custom_attrs ilAttrsThatGoOnPrimaryItem; }


and ComputeFlagFixupsForMemberBinding cenv eenv (v:Val,memberInfo:ValMemberInfo) =

     if isNil memberInfo.ImplementedSlotSigs then 
         [fixupVirtualSlotFlags]
     else 
         memberInfo.ImplementedSlotSigs |> List.map (fun slotsig -> 
             let oty = slotsig.ImplementedType
             let tcref = v.MemberApparentParent
             let tcaug = tcref.TypeContents
             
             
             let shouldUseMethodImpl = 
                 // TODO: it would be good to get rid of this special casing of Compare and GetHashCode during code generation
                 let isCompare = 
                     (isSome tcaug.tcaug_compare && type_equiv cenv.g oty cenv.g.mk_IComparable_ty)
                 let isStructural =
                     (isSome tcaug.tcaug_compare_withc && type_equiv cenv.g oty cenv.g.mk_IStructuralComparable_ty) ||
                     (isSome tcaug.tcaug_hash_and_equals_withc && type_equiv cenv.g oty cenv.g.mk_IStructuralEquatable_ty)
                 is_interface_typ cenv.g oty && not isCompare && not isStructural


             let memberParentTypars = 
                 match PartitionValTypars cenv.g v with
                 | Some(_,memberParentTypars,_,_,_) -> memberParentTypars
                 | None -> errorR(InternalError("PartitionValTypars",v.Range)); []

             let eenvUnderTypars = env_for_typars memberParentTypars eenv

             let reallyUseMethodImpl,nameOfOverridingMethod, _ = 
                 GenMethodImpl cenv eenvUnderTypars (shouldUseMethodImpl,slotsig) v.Range

             (if reallyUseMethodImpl then fixupMethodImplFlags >> renameMethodDef nameOfOverridingMethod
              else fixupVirtualSlotFlags >> renameMethodDef nameOfOverridingMethod))

and GenMethodForBinding 
        cenv cgbuf eenv 
        (v:Val,mspec,access,paramInfos,retInfo) 
        (topValInfo,baseValOpt,tps,methodVars, body, returnTy) =
  
    let m = v.Range
    let selfMethodVars,nonSelfMethodVars,compileAsInstance =
        match v.MemberInfo with 
        | Some(memberInfo) when ValSpecIsCompiledAsInstance cenv.g v -> 
            match methodVars with 
            | [] -> error(Error("Internal error: empty argument list for instance method",v.Range))
            | h::t -> [h],t,true
        |  _ -> [],methodVars,false

    let nonUnitNonSelfMethodVars,body = BindUnitVars cenv.g (nonSelfMethodVars,paramInfos,body)
    let nonUnitMethodVars = selfMethodVars@nonUnitNonSelfMethodVars
    let cmtps,curriedArgInfos,_,_ = GetTopValTypeInCompiledForm cenv.g topValInfo v.Type v.Range
    let eenv = bindBaseVarOpt cenv eenv baseValOpt

    // The type parameters of the method's type are different to the type parameters 
    // for the big lambda ("tlambda") of the implementation of the method. 
    let eenv_under_meth_tlambda_typars = env_for_typars tps eenv
    let eenv_under_meth_type_typars = env_for_typars cmtps eenv

    // Add the arguments to the environment.  We add an implicit 'this' argument to constructors 
    let isCtor = v.IsConstructor 
    let eenvForMeth = 
        let eenvForMeth = eenv_under_meth_tlambda_typars
        let numImplicitArgs = if isCtor then 1 else 0
        let eenvForMeth = AddStorageForLocalVals cenv.g (List.mapi (fun i v -> (v,Arg (numImplicitArgs+i))) nonUnitMethodVars) eenvForMeth
        eenvForMeth

    let tailCallInfo = [(mk_local_vref v,BranchCallMethod (topValInfo.AritiesOfArgs,curriedArgInfos,tps,nonUnitMethodVars.Length,GetNumObjArgsOfVal v))]

    // Discard the result on a 'void' return type. For a constructor just return 'void'  
    let sequel = 
        if is_unit_typ cenv.g returnTy then discardAndReturnVoid 
        elif isCtor then ReturnVoid 
        else Return

    // Now generate the code.

    let ilMethodBody,preservesig = 
        match TryFindAttrib cenv.g cenv.g.attrib_DllImportAttribute v.Attribs with
        | Some (Attrib(_,_,[ AttribStringArg(dll) ],namedArgs,m))  -> 
            if tps <> [] then error(Error("The signature for this external function contains type parameters. Constrain the argument and return types to indicate the types of the corresponding C function",m)); 
            GenPInvokeMethod (v.CompiledName,dll,namedArgs), true

        | Some (Attrib(_,_,_,_,m))  -> 
            error(Error("The DllImport attribute could not be decoded",m));
        | _ -> 
            // Replace the body of PseudoValue "must inline" methods with a 'throw'
            // However still generate the code for reflection etc.
            let bodyExpr =
                if HasAttrib cenv.g cenv.g.attrib_NoDynamicInvocationAttribute v.Attribs then
                    mk_throw m returnTy
                         (mk_exnconstr(mk_MFCore_tcref cenv.g.fslibCcu "DynamicInvocationNotSupportedException", 
                                       [ mk_string cenv.g m v.CompiledName],m)) 
                else 
                    body 

            // This is the main code generation for most methods 
            MethodBody_il(CodeGenMethodForExpr cenv cgbuf.mgbuf (SPAlways,tailCallInfo, mspec.Name, eenvForMeth, 0, 0, bodyExpr, sequel)),
            false

    // Do not generate DllImport attributes into the code - they are implicit from the P/Invoke
    let attrs = v.Attribs |> List.filter (IsMatchingAttrib cenv.g cenv.g.attrib_DllImportAttribute >> not)

    // Do not push the attributes to the method for events and properties 
    // However OverloadIDAttribute does get pushed to the methods as this is 
    // required by the F# quotation/reflection implementation. 
    
    let attrsThatMustBeOnMethod, attrsThatGoOnPropertyIfItExists  = 
        attrs |> List.partition (IsMatchingAttrib cenv.g cenv.g.attrib_OverloadIDAttribute) 

    let ilAttrsCompilerGenerated = if v.IsCompilerGenerated then [ mk_CompilerGeneratedAttribute cenv.g.ilg ] else []
    let ilAttrsThatGoOnMethod      = GenAttrs cenv eenv attrsThatMustBeOnMethod
    let ilAttrsThatGoOnPrimaryItem = GenAttrs cenv eenv attrsThatGoOnPropertyIfItExists
    let ilTypars = GenGenericParams m cenv eenv_under_meth_tlambda_typars.tyenv tps
    let ilParams = GenParams cenv eenv mspec paramInfos (Some(nonUnitNonSelfMethodVars))
    let ilReturn = GenReturnInfo cenv eenv mspec.FormalReturnType retInfo
    let methName = mspec.Name
    let tref = mspec.MethodRef.EnclosingTypeRef

    let EmitTheMethodDef mdef = 
        // Does the function have an explicit [<EntryPoint>] attribute? 
        let isExplicitEntryPoint = HasAttrib cenv.g cenv.g.attrib_EntryPointAttribute attrs
        let mdef = {mdef with 
                        mdPreserveSig=preservesig;
                        mdEntrypoint = isExplicitEntryPoint; }

        let mdef = 
            if // operator names
               mdef.mdName.StartsWith("op_",System.StringComparison.Ordinal) || 
               // active pattern names
               mdef.mdName.StartsWith("|",System.StringComparison.Ordinal) then 
                {mdef with mdSpecialName=true} 
            else 
                mdef
        CountMethodDef();
        cgbuf.mgbuf.AddMethodDef(tref,mdef)
                

    match v.MemberInfo with 
    // don't generate unimplemented abstracts 
    | Some(memberInfo) when memberInfo.MemberFlags.MemberIsDispatchSlot && not memberInfo.IsImplemented -> 
         // skipping unimplemented abstract method
         ()    
    | Some(memberInfo) when not v.IsExtensionMember -> 

       let _,ilMethTypars = ilTypars |> List.chop (inst_of_typ mspec.EnclosingType).Length
       if memberInfo.MemberFlags.MemberKind = MemberKindConstructor then 
           assert (isNil ilMethTypars)

           // Constructors in abstract classes become protected
           let access = 
               if HasAttrib cenv.g cenv.g.attrib_AbstractClassAttribute v.MemberApparentParent.Attribs then 
                   MemAccess_family
               else 
                   access

           let mdef = mk_ctor (access,ilParams,ilMethodBody) 
           let mdef = { mdef with mdCustomAttrs= mk_custom_attrs (ilAttrsThatGoOnPrimaryItem @ ilAttrsThatGoOnMethod @ ilAttrsCompilerGenerated) }; 
           EmitTheMethodDef mdef

       elif memberInfo.MemberFlags.MemberKind = MemberKindClassConstructor then 
           assert (isNil ilMethTypars)
           let mdef = mk_cctor ilMethodBody 
           let mdef = { mdef with mdCustomAttrs= mk_custom_attrs (ilAttrsThatGoOnPrimaryItem @ ilAttrsThatGoOnMethod @ ilAttrsCompilerGenerated) }; 
           EmitTheMethodDef mdef

       // Generate virtual/override methods + method-impl information if needed
       else
           let mdef = 
               if not compileAsInstance then 
                   mk_static_mdef (ilMethTypars,memberInfo.CompiledName,access,ilParams,ilReturn,ilMethodBody) 

               elif memberInfo.MemberFlags.MemberIsVirtual || 
                    (memberInfo.MemberFlags.MemberIsDispatchSlot && memberInfo.IsImplemented) || 
                    memberInfo.MemberFlags.MemberIsOverrideOrExplicitImpl then 

                   // Virtual methods are used to implement interfaces and hence must currently be public 
                   // REVIEW: use method impls to implement the interfaces 
                   if access <> MemAccess_public && not v.IsCompilerGenerated then 
                       warning(FullAbstraction("This method will be made public in the underlying IL because it may implement an interface or override a method",v.Range));

                   let flagFixups = ComputeFlagFixupsForMemberBinding cenv eenv (v,memberInfo)
                   let mdef = mk_generic_virtual_mdef (memberInfo.CompiledName,ComputePublicMemberAccess false,ilMethTypars,ilParams,ilReturn,ilMethodBody)
                   let mdef = List.fold (fun mdef f -> f mdef) mdef flagFixups 
                   mdef
               else 
                   mk_generic_instance_mdef (memberInfo.CompiledName,access,ilMethTypars,ilParams,ilReturn,ilMethodBody) 

           let isAbstract = 
               memberInfo.MemberFlags.MemberIsDispatchSlot && 
               let tcref =  v.MemberApparentParent
               not (deref_tycon tcref).IsFSharpDelegateTycon

           let mdef = 
               {mdef with mdKind=match mdef.mdKind with 
                                 | MethodKind_virtual vinfo -> 
                                     MethodKind_virtual {vinfo with virtFinal=memberInfo.MemberFlags.MemberIsFinal;
                                                                    virtAbstract=isAbstract; } 
                                 | k -> k }

           match memberInfo.MemberFlags.MemberKind with 
               
           | (MemberKindPropertySet | MemberKindPropertyGet)  as k ->
               if nonNil ilMethTypars then 
                   error(InternalError("A property may not be more generic than the enclosing type - constrain the polymorphism in the expression",v.Range));
                   
               // Check if we're compiling the property as a .NET event
               if CompileAsEvent cenv.g v.Attribs  then 

                   // Emit the pseudo-property as an event, but not if its a private method impl
                   if mdef.mdAccess <> MemAccess_private then 
                       let edef = GenEventForProperty cenv eenvForMeth mspec memberInfo ilAttrsThatGoOnPrimaryItem m returnTy 
                       cgbuf.mgbuf.AddEventDef(tref,edef)
                   // The method def is dropped on the floor here
                   
               else
                   // Emit the property, but not if its a private method impl
                   if mdef.mdAccess <> MemAccess_private then 
                       let vtyp = ReturnTypeOfPropertyVal cenv.g v
                       let ilPropTy = GenType m cenv.g eenv_under_meth_type_typars.tyenv vtyp
                       let ilArgTys = v |> ArgInfosOfPropertyVal cenv.g |> List.map fst |> GenTypes m cenv.g eenv_under_meth_type_typars.tyenv
                       let ilPropertyDef = GenPropertyForMethodDef cenv.g compileAsInstance tref mdef memberInfo ilArgTys ilPropTy (mk_custom_attrs ilAttrsThatGoOnPrimaryItem)
                       cgbuf.mgbuf.AddOrMergePropertyDef(tref,ilPropertyDef,m)

                   // Add the special name flag for all properties
                   let mdef = mdef |> AddSpecialNameFlag
                   // Do not push the attributes to the method for events and properties 
                   // However OverloadIDAttribute does get pushed to the methods as this is 
                   // required by the F# quotation/reflection implementation. 
                   let mdef = { mdef with mdCustomAttrs= mk_custom_attrs (ilAttrsThatGoOnMethod @ ilAttrsCompilerGenerated) }; 
                   EmitTheMethodDef mdef
           | _ -> 
               let mdef = { mdef with mdCustomAttrs= mk_custom_attrs (ilAttrsThatGoOnPrimaryItem @ ilAttrsThatGoOnMethod @ ilAttrsCompilerGenerated) }; 
               EmitTheMethodDef mdef

    | _ -> 
        let mdef = mk_static_mdef (ilTypars, methName, access,ilParams,ilReturn,ilMethodBody)
        let mdef = { mdef with mdCustomAttrs= mk_custom_attrs (ilAttrsThatGoOnPrimaryItem @ ilAttrsThatGoOnMethod @ ilAttrsCompilerGenerated) } 
        EmitTheMethodDef mdef
        

and GenPInvokeMethod (nm,dll,namedArgs) =
    let decoder = AttributeDecoder namedArgs
    
    MethodBody_pinvoke 
      { pinvokeWhere=mk_simple_modref dll;
        pinvokeName=decoder.FindString "EntryPoint" nm;
        pinvokeCallconv=
            match decoder.FindInt32 "ILCallingConv" 0 with 
            | 1 -> PInvokeCallConvWinapi
            | 2 -> PInvokeCallConvCdecl
            | 3 -> PInvokeCallConvStdcall
            | 4 -> PInvokeCallConvThiscall
            | 5 -> PInvokeCallConvFastcall
            | _ -> PInvokeCallConvWinapi;
        PInvokeCharEncoding=
            match decoder.FindInt32 "CharSet" 0 with 
            | 1 -> PInvokeEncodingNone
            | 2 -> PInvokeEncodingAnsi
            | 3 -> PInvokeEncodingUnicode
            | 4 -> PInvokeEncodingAuto
            | _  -> PInvokeEncodingNone;
        pinvokeNoMangle= decoder.FindBool "ExactSpelling" false;
        pinvokeLastErr= decoder.FindBool "SetLastError" false;
        PInvokeThrowOnUnmappableChar= if (decoder.FindBool "ThrowOnUnmappableChar" false) then PInvokeThrowOnUnmappableCharEnabled else PInvokeThrowOnUnmappableCharUseAssem;
        PInvokeCharBestFit=if (decoder.FindBool "BestFitMapping" false) then PInvokeBestFitEnabled else PInvokeBestFitUseAssem }
      

and GenBindings cenv cgbuf eenv binds = FlatList.iter (GenBind cenv cgbuf eenv) binds

//-------------------------------------------------------------------------
// Generate locals and other storage of values
//------------------------------------------------------------------------- 

and GenSetVal cenv cgbuf eenv (vref,e,m) sequel =
    if verbose then dprintn ("GenSetVal");   
    let storage = storage_for_vref m vref eenv
    match storage with
    | Env (ilCloTypeSpec,i,localCloInfo) -> 
        CG.EmitInstr cgbuf [Push (Type_boxed ilCloTypeSpec) ] ldarg_0; 
    | _ -> 
        ()
    GenExpr cenv cgbuf eenv SPSuppress e Continue;
    GenSetStorage cenv vref.Range cgbuf storage
    GenUnitThenSequel cenv eenv.cloc cgbuf sequel
      
and GenGetValRefAndSequel cenv cgbuf eenv m (v:ValRef) fetch_sequel =
    let ty = v.Type
    GenGetStorageAndSequel cenv cgbuf eenv m (ty, GenType m cenv.g eenv.tyenv ty) (storage_for_vref m v eenv)  fetch_sequel

and GenGetVal cenv cgbuf eenv (v:ValRef,m) sequel =
    GenGetValRefAndSequel cenv cgbuf eenv m v None;
    GenSequel cenv eenv.cloc cgbuf sequel
      
and GenBindRhs cenv cgbuf eenv sp (vspec:Val) e =   
    match e with 
    | TExpr_tlambda _ | TExpr_lambda _ -> 
        let isLocalTypeFunc = IsNamedLocalTypeFuncVal cenv.g vspec e
        let selfv = if isLocalTypeFunc then None else Some (mk_local_vref vspec)
        GenLambda cenv cgbuf eenv isLocalTypeFunc selfv e Continue 
    | _ -> 
        GenExpr cenv cgbuf eenv sp e Continue;

and GenSetBindValue cenv cgbuf eenv eenv2 (vspec:Val) e =   
    GenBindRhs cenv cgbuf eenv2 SPSuppress vspec e;
    GenStoreVal cenv cgbuf eenv vspec.Range vspec
        
and EmitInitLocal cgbuf typ idx = CG.EmitInstrs cgbuf []  [I_ldloca (uint16 idx);  (I_initobj typ) ]
and EmitSetLocal cgbuf idx = CG.EmitInstr cgbuf [Pop] (I_stloc (uint16 idx))
and EmitGetLocal cgbuf typ idx = CG.EmitInstr cgbuf [Push typ] (I_ldloc (uint16 idx))
and EmitSetStaticField cgbuf fspec = CG.EmitInstr cgbuf [Pop] (mk_normal_stsfld fspec)
and EmitGetStaticFieldAddr cgbuf typ fspec = CG.EmitInstr cgbuf [Push typ]  (I_ldsflda fspec)
and EmitGetStaticField cgbuf typ fspec = CG.EmitInstr cgbuf [Push typ]  (mk_normal_ldsfld fspec)

and GenSetStorage cenv m cgbuf storage = 
    if verbose then dprintn ("GenSetStorage");        
    match storage with  
    | Local (idx,_)  ->   EmitSetLocal cgbuf idx
    | StaticField (fspec,vref,hasLiteralAttr,tspec,_,_,_,_,ilSetterMethRef,optShadowLocal) ->  
        if hasLiteralAttr then errorR(Error("Literal fields may not be set",m));
        CG.EmitInstr cgbuf [Pop]  (I_call(Normalcall,mk_mref_mspec_in_typ(ilSetterMethRef,mk_typ AsObject tspec,[]),None))
    | Method (_,_,mspec,m,_,_) -> 
        error(Error("GenSetStorage: "^mspec.Name^" was represented as a static method but was not an appropriate lambda expression",m))
    | Null ->  CG.EmitInstr cgbuf [Pop] (i_pop)
    | Arg _ -> error(Error("mutable variables may not escape their method",m))

    | Env (_,i,localCloInfo) -> 
        // Note: ldarg0 has already been emitted in GenSetVal
        CG.EmitInstr cgbuf [Pop;Pop] (mk_IlxInstr (EI_stenv i));   

    | Unrealized -> error(Error("compiler error: unexpected unrealized value",m))

and CommitGetStorageSequel cenv cgbuf eenv m typ localCloInfo storeSequel = 
    if verbose then dprintn ("CommitGetStorageSequel");          
    match localCloInfo,storeSequel with 
    | Some {contents =NamedLocalIlxClosureInfoGenerator cloinfo},_ -> error(InternalError("Unexpected generator",m))
    | Some {contents =NamedLocalIlxClosureInfoGenerated cloinfo},Some (tyargs,args,m,sequel) when nonNil tyargs ->
        let actual_rty = GenNamedLocalTyFuncCall cenv cgbuf eenv typ cloinfo tyargs m;
        CommitGetStorageSequel cenv cgbuf eenv m actual_rty None (Some ([],args,m,sequel))
    | _, None ->
            (if verbose then dprintn ("CommitGetStorageSequel: None");
             ())
    | _,Some ([],[],m,sequel) ->
        GenSequel cenv eenv.cloc cgbuf sequel 
    | _,Some (tyargs,args,m,sequel) ->
        GenIndirectCall cenv cgbuf eenv (typ,tyargs,args,m) sequel 

and GenGetStorageAndSequel cenv cgbuf eenv m (typ,ilTy) storage storeSequel =
    if verbose then dprintn ("GenGetStorageAndSequel:");   
    match storage with  
    | Local (idx,localCloInfo) ->
        if verbose then dprintn ("GenGetStorageAndSequel: Local...");            
        EmitGetLocal cgbuf ilTy idx;
        CommitGetStorageSequel cenv cgbuf eenv m typ localCloInfo storeSequel

    | StaticField (fspec,vref,hasLiteralAttr,ilTypeSpecForProperty,fieldName,_,_,ilGetterMethRef,_,_) ->  
        if verbose then dprintn ("GenGetStorageAndSequel: StaticField...");         
        // References to literals go directly to the field - no property is used
        if hasLiteralAttr then 
            EmitGetStaticField cgbuf ilTy fspec
        else
            CG.EmitInstr cgbuf [Push ilTy]  (I_call(Normalcall,mk_mref_mspec_in_typ(ilGetterMethRef,mk_typ AsObject ilTypeSpecForProperty,[]),None));
        CommitGetStorageSequel cenv cgbuf eenv m typ None storeSequel

    | Method (topValInfo,vref,mspec,_,_,_) -> 
        // Get a toplevel value as a first-class value. 
        // We generate a lambda expression and that simply calls 
        // the toplevel method. However we optimize the case where we are 
        // immediately applying the value anyway (to insufficient arguments). 

        // First build a lambda expression for the saturated use of the toplevel value... 
        // REVIEW: we should NOT be doing this in the backend... 
        if verbose then dprintn ("GenGetStorageAndSequel: Method...");      
        let expr,exprty = AdjustValForExpectedArity cenv.g m vref NormalValUse topValInfo

        // Then reduce out any arguments (i.e. apply the sequel immediately if we can...) 
        match storeSequel with 
        | None -> GenLambda cenv cgbuf eenv false None expr Continue
        | Some (tyargs',args,m,sequel) -> 
            let specialized_expr = 
                if verbose && tyargs' <> [] then dprintn ("creating type-specialized lambda at use of method "^mspec.Name);
                if verbose && args <> [] then dprintf "creating term-specialized lambda at use of method %s\n--> expr = %s\n--> exprty = %s\n--> #args = %d\n" mspec.Name (showL (ExprL expr)) (showL (typeL exprty)) (List.length args);
                if isNil args && isNil tyargs' then failwith ("non-lambda at use of method "^mspec.Name);
                MakeApplicationAndBetaReduce cenv.g (expr,exprty,[tyargs'],args,m)
            GenExpr cenv cgbuf eenv SPSuppress specialized_expr sequel

    | Null  ->   
        CG.EmitInstr cgbuf [Push ilTy] (i_ldnull); 
        CommitGetStorageSequel cenv cgbuf eenv m typ None storeSequel

    | Unrealized  ->
        error(InternalError(sprintf "getting an unrealized value of type '%s'" (showL(typeL typ)),m));

    | Arg i -> 
        CG.EmitInstr cgbuf [Push ilTy] (I_ldarg (uint16 i)); 
        CommitGetStorageSequel cenv cgbuf eenv m typ None storeSequel

    | Env (_,i,localCloInfo) -> 
        // Note: ldarg 0 is emitted in 'cu_erase' erasure of the ldenv instruction
        CG.EmitInstr cgbuf [Push ilTy] (mk_IlxInstr (EI_ldenv i)); 
        CommitGetStorageSequel cenv cgbuf eenv m typ localCloInfo storeSequel

and GenGetLocalVals cenv cgbuf eenvouter m fvs = 
    List.iter (fun v -> GenGetLocalVal cenv cgbuf eenvouter m v None) fvs;

and GenGetLocalVal cenv cgbuf eenv m (vspec:Val) fetch_sequel =
    GenGetStorageAndSequel cenv cgbuf eenv m (vspec.Type, GenTypeOfVal cenv eenv vspec) (storage_for_val m vspec eenv) fetch_sequel

and GenGetLocalVRef cenv cgbuf eenv m (vref:ValRef) fetch_sequel =
    GenGetStorageAndSequel cenv cgbuf eenv m (vref.Type, GenTypeOfVal cenv eenv (deref_val vref)) (storage_for_vref m vref eenv) fetch_sequel

and GenStoreVal cenv cgbuf eenv m (vspec:Val) =
    GenSetStorage cenv vspec.Range cgbuf (storage_for_val m vspec eenv)

//and gen_begin_end

//--------------------------------------------------------------------------
// Allocate locals for values
//-------------------------------------------------------------------------- 
 
and AllocLocal cenv cgbuf eenv compgen (v,ty) (scopeMarks : mark * mark) = 
     // The debug range for the local
     let ranges = if compgen then [] else [(v,scopeMarks)]
     // Get an index for the local
     let j = 
        if cenv.localOptimizationsAreOn 
        then cgbuf.ReallocLocal((fun i (_,ty') -> not (Imap.mem i eenv.liveLocals) && (ty = ty')),ranges,ty)
        else cgbuf.AllocLocal(ranges,ty)
     j, { eenv with liveLocals =  Imap.add j () eenv.liveLocals  }

and AllocLocalVal cenv cgbuf v eenv repr scopeMarks = 
    let repr,eenv = 
        let ty = v.Type
        if is_unit_typ cenv.g ty && not v.IsMutable then  Null,eenv
        elif isSome repr && IsNamedLocalTypeFuncVal cenv.g v (the repr) then 
            (* known, named, non-escaping type functions *)
            let cloinfoGenerate eenv = 
                let eenvinner = 
                    {eenv with 
                         letBoundVars=(mk_local_vref v)::eenv.letBoundVars}
                let cloinfo,_,_ = GetIlxClosureInfo cenv v.Range true None eenvinner (the repr)
                cloinfo
            
            let idx,eenv = AllocLocal cenv cgbuf eenv v.IsCompilerGenerated (v.MangledName, cenv.g.ilg.typ_Object) scopeMarks
            Local (idx,Some(ref (NamedLocalIlxClosureInfoGenerator cloinfoGenerate))),eenv
        else
            (* normal local *)
            let idx,eenv = AllocLocal cenv cgbuf eenv v.IsCompilerGenerated (v.MangledName, GenTypeOfVal cenv eenv v) scopeMarks
            Local (idx,None),eenv
    Some repr,AddStorageForVal cenv.g (v,notlazy repr) eenv

and AllocStorageForBind cenv cgbuf scopeMarks eenv bind = 
    AllocStorageForBinds cenv cgbuf scopeMarks eenv (FlatList.one bind)


and AllocStorageForBinds cenv cgbuf scopeMarks eenv binds = 
    // phase 1 - decicde representations - most are very simple. 
    let reps, eenv = FlatList.mapfold (AllocValForBind cenv cgbuf scopeMarks) eenv binds 

    // Phase 2 - run the cloinfo generators for NamedLocalClosure values against the environment recording the 
    // representation choices. 
    reps |> FlatList.iter (fun reprOpt -> 
       match reprOpt with 
       | Some repr -> 
           match repr with 
           | Local(_,Some g) 
           | Env(_,_,Some g) -> 
               match !g with 
               | NamedLocalIlxClosureInfoGenerator f -> g := NamedLocalIlxClosureInfoGenerated (f eenv) 
               | NamedLocalIlxClosureInfoGenerated _ -> ()
           | _ -> ()
       | _ -> ());

    eenv
   
and AllocValForBind cenv cgbuf (scopeMarks:mark*mark) eenv (TBind(v,repr,_)) =
    match v.TopValInfo with 
    | None -> 
        AllocLocalVal cenv cgbuf v eenv (Some repr) scopeMarks
    | Some _ -> 
        None,AllocTopValWithinExpr cenv cgbuf eenv.cloc scopeMarks v eenv


and AllocTopValWithinExpr cenv cgbuf cloc scopeMarks v eenv =
    // decide whether to use a shadow local or not
    let useShadowLocal = 
        cenv.debug && 
        not cenv.localOptimizationsAreOn &&
        not v.IsCompilerGenerated &&
        not v.IsMutable &&
        IsCompiledAsStaticValue cenv.g v

    let optShadowLocal,eenv = 
        if useShadowLocal then 
            let storageOpt, eenv = AllocLocalVal cenv cgbuf v eenv None scopeMarks 
            match storageOpt with 
            | None -> NoShadowLocal,eenv
            | Some storage -> ShadowLocal storage,eenv
            
        else 
            NoShadowLocal,eenv

    ComputeAndAddStorageForLocalTopVal cenv.g cloc optShadowLocal v eenv



//--------------------------------------------------------------------------
// Generate stack save/restore and assertions - pulled into letrec by alloc*
//-------------------------------------------------------------------------- 

/// Save the stack
/// - [gross] because IL flushes the stack at the exn. handler
/// - and     because IL requires empty stack following a forward br (jump).
and EmitSaveStack cenv cgbuf eenv m scopeMarks =
    if verbose then dprintn ("gen_save_stack");
    let stack_saved = (cgbuf.GetCurrentStack())
    let where_stack_saved,eenvinner = List.mapfold (fun eenv ty -> AllocLocal cenv cgbuf eenv true (ilxgenGlobalNng.FreshCompilerGeneratedName ("spill",m), ty) scopeMarks) eenv stack_saved
    List.iter (EmitSetLocal cgbuf) where_stack_saved;
    cgbuf.AssertEmptyStack();
    (stack_saved,where_stack_saved),eenvinner (* need to return, it marks locals "live" *)

/// Restore the stack and load the result 
and EmitRestoreStack cenv cgbuf (stack_saved,where_stack_saved) =
    cgbuf.AssertEmptyStack();
    List.iter2 (EmitGetLocal cgbuf) (List.rev stack_saved) (List.rev where_stack_saved)

//-------------------------------------------------------------------------
//GenAttr: custom attribute generation
//------------------------------------------------------------------------- 

and GenAttribArg cenv eenv x (ilArgTy:ILType) = 

    match x,ilArgTy with 

    (* Detect standard constants *)
    | TExpr_const(c,m,_),_ -> 
        let tynm = ilArgTy.TypeSpec.Name
        let isobj = (tynm = "System.Object")

        match c with 
        | TConst_bool b -> CustomElem_bool b
        | TConst_int32 i when isobj || tynm = "System.Int32" ->  CustomElem_int32 ( i)
        | TConst_int32 i when tynm = "System.SByte" ->  CustomElem_int8 (sbyte i)
        | TConst_int32 i when tynm = "System.Int16"  -> CustomElem_int16 (int16 i)
        | TConst_int32 i when tynm = "System.Byte"  -> CustomElem_uint8 (byte i)
        | TConst_int32 i when tynm = "System.UInt16" ->CustomElem_uint16 (uint16 i)
        | TConst_int32 i when tynm = "System.UInt32" ->CustomElem_uint32 (uint32 i)
        | TConst_int32 i when tynm = "System.UInt64" ->CustomElem_uint64 (uint64 (int64 i)) 
        | TConst_sbyte  i  -> CustomElem_int8 i
        | TConst_int16  i  -> CustomElem_int16 i
        | TConst_int32 i   -> CustomElem_int32 i
        | TConst_int64 i   -> CustomElem_int64 i  
        | TConst_byte i    -> CustomElem_uint8 i
        | TConst_uint16 i  -> CustomElem_uint16 i
        | TConst_uint32 i  -> CustomElem_uint32 i
        | TConst_uint64 i  -> CustomElem_uint64 i
        | TConst_float i   -> CustomElem_float64 i
        | TConst_float32 i -> CustomElem_float32 i
        | TConst_char i    -> CustomElem_char i
        | TConst_zero when  tynm = "System.String"  -> CustomElem_string None
        | TConst_string i  when isobj || tynm = "System.String" ->  CustomElem_string (Some(i))
        | _ -> error (InternalError ( "The type '"^tynm^"' may not be used as a custom attribute value",m))

    // Detect '[| ... |]' nodes 
    | TExpr_op(TOp_array,[elemTy],args,m),Type_array _ ->
        let ilElemTy = GenType m cenv.g eenv.tyenv elemTy
        CustomElem_array (List.map (fun arg -> GenAttribArg cenv eenv arg ilElemTy) args)

    // Detect 'typeof<ty>' calls 
    | TExpr_app(TExpr_val(vref,_,_),_,[ty],[],m),_ when is_typeof_vref cenv.g vref   ->
        CustomElem_type (GenType m cenv.g eenv.tyenv ty)

    // Detect 'typedefof<ty>' calls 
    | TExpr_app(TExpr_val(vref,_,_),_,[ty],[],m),_ when is_typedefof_vref cenv.g vref  ->
        CustomElem_tref ((GenType m cenv.g eenv.tyenv ty).TypeRef)    

    // Ignore upcasts 
    | TExpr_op(TOp_coerce,_,[arg2],_),_ ->
        GenAttribArg cenv eenv arg2 ilArgTy

    // Detect explicit enum values 
    | TExpr_app(TExpr_val(vref,_,_),_,_,[arg1],_),_ when cenv.g.vref_eq vref cenv.g.enum_vref  ->
        GenAttribArg cenv eenv arg1 ilArgTy
    

    // Detect bitwise or of attribute flags: one case of constant folding (a more general treatment is needed)
    
    | BitwiseOr cenv.g (arg1,arg2),_ ->
        let v1 = GenAttribArg cenv eenv arg1 ilArgTy 
        let v2 = GenAttribArg cenv eenv arg2 ilArgTy 
        match v1,v2 with 
        | CustomElem_int8 i1, CustomElem_int8 i2 -> CustomElem_int8 (i1 ||| i2) 
        | CustomElem_int16 i1, CustomElem_int16 i2-> CustomElem_int16 (i1 ||| i2)
        | CustomElem_int32 i1, CustomElem_int32 i2-> CustomElem_int32 (i1 ||| i2)
        | CustomElem_int64 i1, CustomElem_int64 i2-> CustomElem_int64 (i1 ||| i2)
        | CustomElem_uint8 i1, CustomElem_uint8 i2-> CustomElem_uint8 (i1 ||| i2)
        | CustomElem_uint16 i1, CustomElem_uint16 i2-> CustomElem_uint16 (i1 ||| i2) 
        | CustomElem_uint32 i1, CustomElem_uint32 i2-> CustomElem_uint32 (i1 ||| i2) 
        | CustomElem_uint64 i1, CustomElem_uint64 i2-> CustomElem_uint64 (i1 ||| i2)
        |  _ -> error (InternalError ("invalid custom attribute value (not a valid constant): "^showL (ExprL x),range_of_expr x))

    // Other expressions are not valid custom attribute values
    | _ -> 
        error (InternalError ("invalid custom attribute value (not a constant): "^showL (ExprL x),range_of_expr x))


and GenAttr cenv eenv (Attrib(_,k,args,props,m)) = 
    let props = 
        props |> List.map (fun (AttribNamedArg(s,ty,fld,AttribExpr(_,expr))) ->
            let m = (range_of_expr expr)
            let il_ty = GenType m cenv.g eenv.tyenv ty
            let cval = GenAttribArg cenv eenv expr il_ty
            (s,il_ty,fld,cval))
    let mspec = 
        match k with 
        | ILAttrib(mref) -> mk_mspec(mref,AsObject,[],[]) 
        | FSAttrib(vref) -> 
             assert(vref.IsMember); 
             let mspec,_,_,_,_ = GetMethodSpecForMemberVal cenv.g (the vref.MemberInfo) vref
             mspec
    let ilArgs = List.map2 (fun (AttribExpr(_,vexpr)) ty -> GenAttribArg cenv eenv vexpr ty) args mspec.FormalArgTypes
    mk_custom_attribute_mref cenv.g.ilg (mspec,ilArgs, props)
    
and GenAttrs cenv eenv attrs = List.map (GenAttr cenv eenv) attrs

//--------------------------------------------------------------------------
// Generate the set of modules for an assembly, and the declarations in each module
//-------------------------------------------------------------------------- 

and GenTypeDefForCompLoc cenv eenv (mgbuf: AssemblyBuilder) cloc hidden attribs = 
    let tref = TypeRefForCompLoc cloc
    let tdef = 
      mk_simple_tdef cenv.g.ilg
        (tref.Name, 
         ComputeTypeAccess tref hidden,
         mk_mdefs [], 
         mk_fdefs [],
         mk_properties [],
         mk_events [],
         mk_custom_attrs 
           (GenAttrs cenv eenv attribs @
            (if List.mem tref.Name [TypeNameForStatupCode cloc; TypeNameForPrivateImplementationDetails cloc]  
             then [ (* mk_CompilerGeneratedAttribute *) ] 
             else [mk_CompilationMappingAttr cenv.g SourceLevelConstruct_Module])))
    let tdef = { tdef with tdSealed=true; tdAbstract=true }
    mgbuf.AddTypeDef(tref,tdef)


and GenModuleExpr cenv cgbuf qname lazyInitInfo eenv cloc x   = 
    let (TMTyped(mty,def,m)) = x 
    // REVIEW: the scopeMarks are used for any shadow locals we create for the module bindings 
    // We use one scope for all the bindings in the module, which makes them all appear with their "default" values
    // rather than incrementally as we step through the  initializations in the module. This is a little unfortunate 
    // but stems from the way we add module values all at once before we generate the module itself.
    LocalScope "module" cgbuf (fun scopeMarks ->
        let sigToImplRemapInfo = mk_mdef_to_mtyp_remapping def mty
        let eenv = AddSignatureRemapInfo "defs" sigToImplRemapInfo eenv
        let eenv = 
            // Allocate all the values, including any shadow locals for static fields
            let allocVal cloc v = AllocTopValWithinExpr cenv cgbuf cloc scopeMarks v
            AddBindingsForModuleDef allocVal eenv.cloc eenv def
        GenModuleDef cenv cgbuf qname lazyInitInfo eenv def)

and GenModuleDefs cenv cgbuf qname lazyInitInfo eenv  mdefs = 
    List.iter (GenModuleDef cenv cgbuf qname lazyInitInfo eenv) mdefs
    
and GenModuleDef cenv (cgbuf:CodeGenBuffer) qname lazyInitInfo eenv  x = 

        
    if verbose then dprintf "GenModuleDef, tspec(cloc) = %A\n" (TypeSpecForCompLoc eenv.cloc);
    match x with 
    | TMDefRec(tycons,binds,mbinds,m) -> 
        tycons |> List.iter (fun tc -> 
            if tc.IsExceptionDecl 
            then GenExnDef cenv cgbuf.mgbuf eenv m tc 
            else GenTypeDef cenv cgbuf.mgbuf lazyInitInfo eenv m tc) ;
        GenLetRecBinds cenv cgbuf eenv (binds,m);
        mbinds |> List.iter (GenModuleBinding cenv cgbuf qname lazyInitInfo eenv) 

    | TMDefLet(bind,m) -> 
        GenBindings cenv cgbuf eenv (FlatList.one bind)

    | TMDefDo(e,m) -> 
        GenExpr cenv cgbuf eenv SPAlways e discard;

    | TMAbstract(mexpr) -> 
        GenModuleExpr cenv cgbuf qname lazyInitInfo eenv eenv.cloc mexpr

    | TMDefs(mdefs) -> 
        GenModuleDefs cenv cgbuf qname lazyInitInfo eenv  mdefs


// Generate a module binding
and GenModuleBinding cenv (cgbuf:CodeGenBuffer) (qname:QualifiedNameOfFile) lazyInitInfo eenv (TMBind(mspec, mdef)) = 
    let hidden = IsHiddenTycon eenv.sigToImplRemapInfo mspec

    let eenvinner = 
        if mspec.IsNamespace then eenv else 
        {eenv with cloc = CompLocForFixedModule cenv.fragName qname.Text mspec }

    // Create the class to hold the contents of this module.  No class needed if 
    // we're compiling it as a namespace 
    if not mspec.IsNamespace then 
        GenTypeDefForCompLoc cenv eenvinner cgbuf.mgbuf eenvinner.cloc hidden mspec.Attribs;
    GenModuleDef cenv cgbuf qname lazyInitInfo eenvinner  mdef;

    // Generate the declarations in the module and its initialization code 
    
    // Most module fields go into the startup code. If there are no fields we don't need a .cctor.
    // However modules with mutable values contain public mutable
    // static fields. In this case we need to ensure that if those fields are "touched" then the outer constructor
    // is forced. The outer constructor will fill ni the value of the field.
    if not mspec.IsNamespace && (cgbuf.mgbuf.GetCurrentFields(TypeRefForCompLoc eenvinner.cloc) |> Seq.is_empty |> not) then 
        GenForceOuterInitializationAsPartOfCCtor cenv cgbuf.mgbuf lazyInitInfo (TypeRefForCompLoc eenvinner.cloc) mspec.Range;


// Generate an entire file
and GenTopImpl cenv mgbuf mainInfo eenv (TImplFile(qname,_,mexpr) as impl)  =
    let fragName = qname.Text
    if verbose then dprintf "-----------------------------------------------------------------------------\ngen_top_impl %s\n" fragName;
    let eenv = {eenv with cloc = { eenv.cloc with clocTopImplQualifiedName = qname.Text } }

    // This is used to point the inner classes back to the startup module for initialization purposes 
    let clocStartup = CompLocForStartupCode eenv.cloc
    let startupTspec = mk_nongeneric_tspec (TypeRefForCompLoc clocStartup)

    // Create the class to hold the initialization code and static fields for this file.  
    GenTypeDefForCompLoc cenv eenv mgbuf clocStartup true []; 
    
    let eenv = {eenv with cloc = clocStartup;
                          someTspecInThisModule=mk_nongeneric_tspec (TypeRefForCompLoc clocStartup) } 

    let  createStaticInitializerFieldInStartupClass() = 
        let initFieldName = CompilerGeneratedName "init"
        let fieldDef = 
            mk_static_fdef (initFieldName,cenv.g.ilg.typ_Int32, None, None, ComputeMemberAccess true true)
            |> add_fdef_never_attrs cenv.g.ilg
            |> add_fdef_generated_attrs cenv.g.ilg

        let fspec = mk_fspec_in_boxed_tspec (startupTspec, initFieldName, cenv. g.ilg.typ_Int32)
        CountStaticFieldDef();
        mgbuf.AddFieldDef(startupTspec.TypeRef,fieldDef); 
        fspec

    let  lazyInitInfo = 
      match mainInfo with 
      | Some _ -> None 
      | None -> 
        // We keep an accumulator of the fragments needed to force the initialization semantics through the compiled code. 
        // These fragments only get executed/committed if we actually end up producing some code for the .cctor. 
        // NOTE: The existence of .cctors adds costs to execution so this is a half-sensible attempt to avoid adding them when possible. 
        let initSemanticsAcc = ref []
        let fspec = createStaticInitializerFieldInStartupClass()
        (*initSemanticsAcc := addCCtor :: !initSemanticsAcc; *)
        Some(fspec,initSemanticsAcc)

    if verbose then dprintn ("gen_top_impl_expr: codegen .cctor/main for outer module");
    let m = qname.Range
    let clocCcu = (CompLocForCcu cenv.viewCcu)
    let methodName = match mainInfo with None -> ".cctor" | _ -> mainMethName
    let topCode = CodeGenMethod cenv mgbuf (true,[],methodName,eenv,0,0,(fun cgbuf eenv -> 
                        GenModuleExpr cenv cgbuf qname lazyInitInfo eenv clocCcu mexpr;
                        CG.EmitInstr cgbuf [] I_ret),m)

    // Make a .cctor method to run the top level bindings.  This initializes all modules. 
    if verbose then dprintn ("Creating .cctor/main for outer module");
    let initmeths = 

        match mainInfo, lazyInitInfo with 

        | Some (main_attrs), None -> 

            // Generate an explicit main method. If necessary, make a class constructor as 
            // well for the bindings earlier in the file containing the entrypoint.  
            match mgbuf.GetExplicitEntryPointInfo() with
            | Some(tref) ->           
                if (CheckCodeDoesSomething topCode.ilCode) then
                    let fspec = createStaticInitializerFieldInStartupClass()
                    mgbuf.AddExplicitInitToSpecificMethodDef((fun md -> md.mdEntrypoint),tref,fspec,GenPossibleILSourceMarker cenv m);
                    [ mk_cctor (MethodBody_il topCode) ] 
                else 
                    []

            // Generate an implicit main method 
            | None ->

                let ilAttrs = mk_custom_attrs (GenAttrs cenv eenv main_attrs)
                if not cenv.emptyProgramOk && not (CheckCodeDoesSomething topCode.ilCode) then 
                  let errorM = mk_range (file_of_range m) (end_of_range m) (end_of_range m)
                  warning (Error("Main module of program is empty: nothing will happen when it is run", errorM));
                let mdef = mk_static_nongeneric_mdef(mainMethName,ComputePublicMemberAccess true,[],mk_return Type_void, MethodBody_il topCode)
                [ {mdef with mdEntrypoint= true; mdCustomAttrs = ilAttrs } ] 

        // Generate an on-demand .cctor for the file 
        | None, Some(fspec, initSemanticsAcc) ->
        
            if (CheckCodeDoesSomething topCode.ilCode) then 
                // Run the imperative (yuck!) actions that force the generation 
                // of references to the cctor for nested modules etc. 
                (List.rev !initSemanticsAcc) |> List.iter (fun f -> f());

                // Return the generated cctor 
                [ mk_cctor (MethodBody_il topCode) ] 
            else
                [] 

        | _ -> failwith "unreachable"

    initmeths |> List.iter (fun mdef -> mgbuf.AddMethodDef(TypeRefForCompLoc clocStartup,mdef)) ;

    // Compute the ilxgenEnv after the generation of the module, i.e. the residue need to generate anything that
    // uses the constructs exported from this module.
    // We add the module type all over again. Note no shadow locals for static fields needed here since they are only relevant to the main/.cctor
    let eenvafter = 
        let allocVal cloc v = ComputeAndAddStorageForLocalTopVal cenv.g cloc NoShadowLocal v
        AddBindingsForLocalModuleType allocVal  clocCcu eenv (mtyp_of_mexpr mexpr)

    eenvafter

and GenForceOuterInitializationAsPartOfCCtor cenv (mgbuf:AssemblyBuilder) lazyInitInfo tref m =
    // Authoring a .cctor with effects forces the cctor for the 'initialization' module by doing a dummy store & load of a field 
    // Doing both a store and load keeps FxCop happier because it thinks the field is useful 
    match lazyInitInfo with 
    | Some (fspec,initSemanticsAcc) -> 
        initSemanticsAcc := (fun () -> mgbuf.AddExplicitInitToSpecificMethodDef((fun md -> md.mdName = ".cctor"),tref,fspec,GenPossibleILSourceMarker cenv m)) :: !initSemanticsAcc
    | None -> ()


/// Generate an Equals method.  
and GenEqualsOverrideCallingIComparable cenv mgbuf eenv m (this_tcref,this_ilty,that_ilty) =
    let mspec = mk_nongeneric_instance_mspec_in_typ (cenv.g.ilg.typ_IComparable, "CompareTo", [cenv.g.ilg.typ_Object], cenv.g.ilg.typ_int32)
    
    mk_virtual_mdef
        ("Equals",MemAccess_public,
         [mk_named_param ("obj",cenv.g.ilg.typ_Object)], 
         mk_return cenv.g.ilg.typ_bool,
         MethodBody_il
             (mk_ilmbody(true,[],2,
                         nonbranching_instrs_to_code
                            [ yield ldarg_0;
                              yield I_ldarg 1us; 
                              if is_struct_tcref this_tcref then 
                                  yield I_callconstraint ( Normalcall, this_ilty,mspec,None)
                              else 
                                  yield I_callvirt ( Normalcall, mspec,None) 
                              yield mk_ldc_i32 (0)
                              yield I_arith AI_ceq ], 
                         None))) 
    |> AddNonUserCompilerGeneratedAttribs cenv.g

/// Generate a GetHashCode method.  
and GenHashOverride cenv mgbuf eenv m this_tcref this_ilty =
    let icomparer_iltref = (cenv.g.tcref_System_Collections_IComparer).CompiledRepresentationForTyrepNamed
    let icomparer_ilt = mk_boxed_typ (icomparer_iltref) []
    let iequalitycomparer_iltref = (cenv.g.tcref_System_Collections_IEqualityComparer).CompiledRepresentationForTyrepNamed
    let iequalitycomparer_ilt = mk_boxed_typ (iequalitycomparer_iltref) []
    let langprim_iltref = (cenv.g.tcref_LanguagePrimitives).CompiledRepresentationForTyrepNamed
    let langprim_ilt = mk_boxed_typ (langprim_iltref) []
    let mspec_getComparer = mk_static_nongeneric_mspec_in_typ (langprim_ilt,"FSharpEqualityComparer",[],iequalitycomparer_ilt)
    let mspec_IStructuralEquatable_GetHashCode = mk_nongeneric_instance_mspec_in_typ(this_ilty,"GetHashCode",[iequalitycomparer_ilt],cenv.g.ilg.typ_Int32)
    mk_virtual_mdef
        ("GetHashCode",MemAccess_public,[],
         mk_return cenv.g.ilg.typ_int32,
         MethodBody_il
            (mk_ilmbody(true,[],2,
                nonbranching_instrs_to_code
                    [   yield ldarg_0
                        yield I_call (Normalcall, mspec_getComparer, None)
                        yield I_call (Normalcall, mspec_IStructuralEquatable_GetHashCode, None) ],
                None)))
    |> AddNonUserCompilerGeneratedAttribs cenv.g
                        
                        
and GenFieldInit m c =
    match c with 
    | TConst_sbyte n   -> FieldInit_int8 n
    | TConst_int16 n   -> FieldInit_int16 n
    | TConst_int32 n   -> FieldInit_int32 n
    | TConst_int64 n   -> FieldInit_int64 n
    | TConst_byte n    -> FieldInit_uint8 n
    | TConst_uint16 n  -> FieldInit_uint16 n
    | TConst_uint32 n  -> FieldInit_uint32 n
    | TConst_uint64 n  -> FieldInit_uint64 n
    | TConst_bool n    -> FieldInit_bool n
    | TConst_char n    -> FieldInit_char (uint16 n)
    | TConst_float32 n -> FieldInit_single n
    | TConst_float n   -> FieldInit_double n
    | TConst_string s  -> FieldInit_string s
    | TConst_zero      -> FieldInit_ref
    | _ -> error(Error("This type may not be used for a literal field",m))


and GenAbstractBinding cenv eenv mgbuf tref (vref:ValRef) =
    assert(vref.IsMember);
    let m = vref.Range
    let memberInfo = (the (vref.MemberInfo))
    let attribs = vref.Attribs
    if memberInfo.MemberFlags.MemberIsDispatchSlot && not memberInfo.IsImplemented then 
        let ilAttrs = GenAttrs cenv eenv attribs
        
        let mspec,ctps,mtps,argInfos,retInfo = GetMethodSpecForMemberVal cenv.g memberInfo vref 
        let eenvForMeth = env_for_typars (ctps@mtps) eenv
        let ilMethTypars = GenGenericParams m cenv eenvForMeth.tyenv mtps
        let ilReturn = GenReturnInfo cenv eenvForMeth mspec.FormalReturnType retInfo
        let ilParams = GenParams cenv eenvForMeth mspec argInfos None
        let compileAsInstance = ValRefIsCompiledAsInstanceMember cenv.g vref
        let mdef = mk_generic_virtual_mdef (memberInfo.CompiledName,ComputePublicMemberAccess false,ilMethTypars,ilParams,ilReturn,MethodBody_abstract)
        let mdef = fixupVirtualSlotFlags mdef
        let mdef = 
          {mdef with 
            mdKind=match mdef.mdKind with 
                    | MethodKind_virtual vinfo -> 
                        MethodKind_virtual {vinfo with virtFinal=memberInfo.MemberFlags.MemberIsFinal;
                                                      virtAbstract=memberInfo.MemberFlags.MemberIsDispatchSlot; } 
                    | k -> k }
        
        match memberInfo.MemberFlags.MemberKind with 
        | MemberKindClassConstructor 
        | MemberKindConstructor 
        | MemberKindMember -> 
             let mdef = {mdef with mdCustomAttrs= mk_custom_attrs ilAttrs }
             [mdef], [], []
        | MemberKindPropertyGetSet -> error(Error("Unexpected GetSet annotation on a property",m));
        | MemberKindPropertySet | MemberKindPropertyGet ->
             let v = deref_val vref
             let vtyp = ReturnTypeOfPropertyVal cenv.g v
             if CompileAsEvent cenv.g attribs then 
                   
                 let edef = GenEventForProperty cenv eenvForMeth mspec memberInfo ilAttrs m vtyp 
                 [],[],[edef]
             else
                 let ilPropertyDef = 
                     let ilPropTy = GenType m cenv.g eenvForMeth.tyenv vtyp
                     let ilArgTys = v |> ArgInfosOfPropertyVal cenv.g |> List.map fst |> GenTypes m cenv.g eenvForMeth.tyenv
                     GenPropertyForMethodDef cenv.g compileAsInstance tref mdef memberInfo ilArgTys ilPropTy (mk_custom_attrs ilAttrs)
                 let mdef = mdef |> AddSpecialNameFlag
                 [mdef], [ilPropertyDef],[]

    else 
        [],[],[]

and GenTypeDef cenv mgbuf lazyInitInfo eenv m (tycon:Tycon) =
    let tcref = mk_local_tcref tycon
    if tycon.IsTypeAbbrev then () else
    match tycon.TypeReprInfo with 
    | None -> ()
    | Some (TAsmRepr _ | TILObjModelRepr _ | TMeasureableRepr _) -> () 
    | Some (TFsObjModelRepr _ | TRecdRepr _ | TFiniteUnionRepr _) -> 
        let eenvinner = replace_tyenv (tyenv_for_tycon tycon) eenv
        let _,thisTy = generalize_tcref tcref
        let ilty = GenType m cenv.g eenvinner.tyenv thisTy
        let tref = ilty.TypeRef
        let tname = tref.Name
        let hidden = IsHiddenTycon eenv.sigToImplRemapInfo tycon
        let hiddenRepr = hidden or IsHiddenTyconRepr eenv.sigToImplRemapInfo tycon
        let access = ComputeTypeAccess tref hidden
        let gparams = GenGenericParams m cenv eenvinner.tyenv tycon.TyparsNoRange
        let aug = tycon.TypeContents
        let intfs =  List.map (p13 >> GenType m cenv.g eenvinner.tyenv) aug.tcaug_implements

        let tcaug = tycon.TypeContents

        let augmentOverrideMethodDefs = 
            // The implicit augmentation doesn't actually create CompareTo(object) or Object.Equals 
            // So we do it here. 
            let specialCompareMethod = 

              // Note you only have to implement 'System.IComparable' to customize structural comparison AND equality on F# types 
              // See also FinalTypeDefinitionChecksAtEndOfInferenceScope in tc.ml 
              
              // Generate an Equals method implemented via IComparable if the type EXPLICITLY implements IComparable.
              // HOWEVER, if the type doesn't override Object.Equals already.  
              (if isNone tcaug.tcaug_compare &&
                  tcaug_has_interface cenv.g tcaug cenv.g.mk_IComparable_ty && 
                  not (tcaug_has_override cenv.g tcaug "Equals" [cenv.g.obj_ty]) &&
                  not tycon.IsFSharpInterfaceTycon
               then 
                  [ GenEqualsOverrideCallingIComparable cenv mgbuf eenv m (tcref,ilty,ilty) ] 
               else [])

            // The implicit augmentation doesn't actually create GetHashCode 
            let hashMethodDefs = 
              (if isSome tycon.TypeContents.tcaug_hash_and_equals_withc (* && not (tcref_alloc_observable tcref) *) && not tycon.TypeContents.tcaug_hasObjectGetHashCode 
               then [ GenHashOverride cenv mgbuf eenv m tcref ilty ] 
               else [])
        
            (specialCompareMethod @ hashMethodDefs) 
              // We can't reduce the accessibility because these implement virtual slots
              (* |> List.map (fun md -> { md with mdAccess=memberAccess }) *)
   
   
        // Generate the interface slots and abstract slots.  
        let abstractMethodDefs,abstractPropDefs, abstractEventDefs = 
            if tycon.IsFSharpDelegateTycon then 
                [],[],[]
            else
                // sort by order of declaration
                tycon.TypeContents.tcaug_adhoc
                |> NameMultiMap.range  
                |> List.sortWith (fun v1 v2 -> range_ord v1.DefinitionRange v2.DefinitionRange) 
                |> List.map (GenAbstractBinding cenv eenv mgbuf tref)
                |> List.unzip3 
                |> triple_map List.concat List.concat List.concat


        let abstractPropDefs = abstractPropDefs |> merge_pdef_list m
        let isAbstract =  is_partially_implemented_tycon tycon

        // Generate all the method impls showing how various abstract slots and interface slots get implemented
        // REVIEW: no method impl generated for IStructuralHash or ICompare 
        let methodImpls = 
            [ for vref in tycon.TypeContents.tcaug_adhoc |> NameMultiMap.range  do
                 assert(vref.IsMember);
                 let memberInfo = vref.MemberInfo.Value
                 if memberInfo.MemberFlags.MemberIsOverrideOrExplicitImpl && not (CompileAsEvent cenv.g vref.Attribs) then 

                     for slotsig in memberInfo.ImplementedSlotSigs do

                         if is_interface_typ cenv.g slotsig.ImplementedType then

                             match vref.TopValInfo with 
                             | Some arities -> 

                                 let memberParentTypars,memberMethodTypars = 
                                     match PartitionValRefTypars cenv.g vref with
                                     | Some(_,memberParentTypars,memberMethodTypars,_,_) -> memberParentTypars,memberMethodTypars
                                     | None -> [],[]

                                 let shouldUseMethodImpl = true
                                 let eenvUnderTypars = env_for_typars memberParentTypars eenv
                                 let reallyUseMethodImpl,_,methodImplGenerator = GenMethodImpl cenv eenvUnderTypars (shouldUseMethodImpl,slotsig) m
                                 if reallyUseMethodImpl then 
                                     yield methodImplGenerator (ilty.TypeSpec,memberMethodTypars)

                             | _ -> () ]
        
        let defaultMemberAttrs = 
            tycon.TypeContents.tcaug_adhoc
            |> NameMultiMap.range  
            |> List.tryPick (fun vref -> 
                let name = vref.DisplayName
                match vref.MemberInfo with 
                | None -> None
                | Some memberInfo -> 
                    match name, memberInfo.MemberFlags.MemberKind with 
                    | ("Item" | "op_IndexedLookup"), (MemberKindPropertyGet  | MemberKindPropertySet) when nonNil (ArgInfosOfPropertyVal cenv.g (deref_val vref)) ->
                        Some( mk_custom_attribute cenv.g.ilg (mk_tref (cenv.g.ilg.mscorlib_scoref,"System.Reflection.DefaultMemberAttribute"),[cenv.g.ilg.typ_String],[CustomElem_string(Some(name))],[]) ) 
                    | _ -> None)
            |> Option.to_list

        let tyconRepr = tycon.TypeReprInfo

        // DebugDisplayAttribute gets copied to the subtypes generated as part of DU compilation
        let debugDisplayAttrs,normalAttrs = tycon.Attribs |> List.partition (IsMatchingAttrib cenv.g cenv.g.attrib_DebuggerDisplayAttribute)
        let generateDebugDisplayAttribute = not cenv.g.compilingFslib && tycon.IsUnionTycon && isNil debugDisplayAttrs
        let generateDebugProxies = (not (tcref_eq cenv.g tcref cenv.g.unit_tcr_canon) &&
                                    not (HasAttrib cenv.g cenv.g.attrib_DebuggerTypeProxyAttribute tycon.Attribs))


        let ilDebugDisplayAttributes = 
            [ yield! GenAttrs cenv eenv debugDisplayAttrs
              if generateDebugDisplayAttribute then 
                  yield mk_DebuggerDisplayAttribute cenv.g.ilg ("{"^debugDisplayMethodName^"()}")  ]


        let tdCustomAttrs = 
          [ yield! defaultMemberAttrs 
            yield! normalAttrs 
                      |> List.filter (IsMatchingAttrib cenv.g cenv.g.attrib_StructLayoutAttribute >> not) 
                      |> GenAttrs cenv eenv
            yield! ilDebugDisplayAttributes  ]

        let reprAccess = ComputeMemberAccess true hiddenRepr


        let tdKind = 
           match  tyconRepr with 
           | Some (TFsObjModelRepr o) -> 
               match o.fsobjmodel_kind with 
               | TTyconClass      -> TypeDef_class
               | TTyconStruct     -> TypeDef_valuetype
               | TTyconInterface  -> TypeDef_interface
               | TTyconEnum       -> TypeDef_enum 
               | TTyconDelegate _ -> TypeDef_delegate 

           | _ -> TypeDef_class

        let isEmptyStruct = 
            (match tdKind with TypeDef_valuetype -> true | _ -> false) &&
            // All structs are sequential by default 
            // Structs with no instance fields get size 1, pack 0
            tycon.AllFieldsAsList |> List.exists (fun f -> not f.IsStatic)

        let requiresExtraField = 
            isEmptyStruct && cenv.workAroundReflectionEmitBugs && not tycon.TyparsNoRange.IsEmpty
        
        // Compute a bunch of useful thnigs for each field 
        let fieldSummaries = 
             [ for fspec in tycon.AllFieldsAsList do

                   let useGenuineField = use_genuine_field tycon fspec
                   
                   // The property (or genuine IL field) is hidden in these circumstances:  
                   //     - secret fields apart from "__value" fields for enums 
                   //     - the representation of the type is hidden 
                   //     - the F# field is hidden by a signature or private declaration 
                   let propHidden = 
                       ((fspec.IsCompilerGenerated && not (is_enum_tycon tycon)) ||
                        hiddenRepr ||
                        IsHiddenRecdField eenv.sigToImplRemapInfo (rfref_of_rfield tcref fspec))
                   let propType = GenType m cenv.g eenvinner.tyenv fspec.FormalType
                   let fldName = GenFieldName tycon fspec
                        
                   yield (useGenuineField,fldName,fspec.IsMutable, fspec.IsStatic, fspec.PropertyAttribs,propType,propHidden,fspec) ]
                    
        // Generate the IL fields 
        let fieldDefs = 
             [ for (useGenuineField,fldName,mut,stat,_,propType,propHidden,fspec) in fieldSummaries do

                  let literalValue = fspec.LiteralValue

                  let fdOffset = 
                     match TryFindAttrib cenv.g cenv.g.attrib_FieldOffsetAttribute fspec.FieldAttribs with
                     | Some (Attrib(_,_,[ AttribInt32Arg(fieldOffset) ],_,m))  -> 
                         Some fieldOffset
                     | Some (Attrib(_,_,_,_,m))  -> 
                         errorR(Error("The FieldOffset attribute could not be decoded",m));
                         None
                     | _ -> 
                         None

                  let attribs = 
                      [ // If using a field then all the attributes go on the field
                        // See also FSharp 1.0 Bug 4727: once we start compiling them as real mutable fields, you should not be able to target both "property" for "val mutable" fields in classes

                        if useGenuineField then yield! fspec.PropertyAttribs 
                        yield! fspec.FieldAttribs ]
                  let fattribs = 
                      attribs
                      |> List.filter (IsMatchingAttrib cenv.g cenv.g.attrib_FieldOffsetAttribute >> not) 

                            
                  let fieldMarshal, fattribs = GenMarshal cenv fattribs

                  let fdNotSerialized = HasAttrib cenv.g cenv.g.attrib_NonSerializedAttribute fattribs

                  // The IL field is hidden if the property/field is hidden OR we're using a property AND the field is not mutable (because we can take the address of a mutable field). *)
                  // Otherwise fields are always accessed via their property getters/setters *)
                  let fdHidden = propHidden || (not useGenuineField && not mut)
                  
                  let extraAttribs = 
                     match tyconRepr with 
                     | Some (TRecdRepr _) when not useGenuineField -> [  mk_DebuggerBrowsableNeverAttribute cenv.g.ilg ] // hide fields in records in debug display
                     | _ -> [] // don't hide fields in classes in debug display

                  yield
                      { fdName=fldName;
                        fdType=propType;
                        fdStatic=stat;
                        fdAccess=ComputeMemberAccess true fdHidden;
                        fdData=None; // REVIEW
                        fdInit= Option.map (GenFieldInit m) literalValue;
                        fdOffset=fdOffset;
                        fdSpecialName = (fldName="value__" && is_enum_tycon tycon);
                        fdMarshal=fieldMarshal
                        fdNotSerialized=fdNotSerialized; 
                        fdInitOnly = false;  // REVIEW
                        fdLiteral =isSome(literalValue); 
                        fdCustomAttrs=mk_custom_attrs (GenAttrs cenv eenv fattribs @ extraAttribs) } 
               if requiresExtraField then 
                   yield mk_instance_fdef("__dummy",cenv.g.ilg.typ_int32,None,MemAccess_assembly) ]
         
        // Generate property definitions for the fields compiled as properties 
        let propertyDefs = 
             [ for (i, (useGenuineField,fldName,mut,stat,propAttribs,propType,propHidden,fspec)) in markup fieldSummaries do
                 if not useGenuineField then 
                     let cc = if stat then ILCallingConv.Static else ILCallingConv.Instance
                     let propName = fspec.Name
                     let fattrs = GenAttrs cenv eenv propAttribs @ [mk_CompilationMappingAttrWithSeqNum cenv.g SourceLevelConstruct_Field i]
                     yield
                       { propName=propName;
                         propRTSpecialName=false;
                         propSpecialName=false;
                         propSet=(if mut then Some(mk_mref(tref,cc,"set_"^propName,0,[propType],Type_void)) else None);
                         propGet=Some(mk_mref(tref,cc,"get_"^propName,0,[],propType));
                         propCallconv=(if stat then CC_static else CC_instance);
                         propType=propType;          
                         propInit=None;
                         propArgs=[];
                         propCustomAttrs=mk_custom_attrs fattrs; } ] 
         
        let methodDefs = 
            [ // Generate property getter methods for those fields that have properties 
              for (useGenuineField,fldName,_,stat,_,propType,propHidden,fspec) in fieldSummaries do
                if not useGenuineField then 
                    let propName = fspec.Name
                    let methnm = "get_"^propName
                    let access = ComputeMemberAccess true propHidden
                    yield mk_ldfld_method_def (methnm,access,stat,ilty,fldName,propType) 

              // Generate property setter methods for the mutable fields 
              for (useGenuineField,fldName,mut,stat,_,propType,propHidden,fspec) in fieldSummaries do
                if mut && not useGenuineField then 
                    let propName = fspec.Name
                    let il_fspec = mk_fspec_in_typ(ilty,fldName,propType)
                    let methnm = "set_"^propName
                    let parms = [mk_named_param("value",propType)]
                    let ret = mk_return Type_void
                    let access = ComputeMemberAccess true propHidden
                    let mdef = 
                         if stat then 
                             mk_static_nongeneric_mdef
                               (methnm,access,parms,ret,MethodBody_il
                                  (mk_ilmbody(true,[],2,nonbranching_instrs_to_code ([ ldarg_0;mk_normal_stsfld il_fspec]),None)))
                         else 
                             mk_instance_mdef
                               (methnm,access,parms,ret,MethodBody_il
                                  (mk_ilmbody(true,[],2,nonbranching_instrs_to_code ([ ldarg_0;I_ldarg 1us;mk_normal_stfld il_fspec]),None)))
                    yield mdef |> AddSpecialNameFlag 

              if generateDebugDisplayAttribute then 
                  let (|Lazy|) (x:Lazy<_>) = x.Force()
                  match (vspec_map_tryfind cenv.g.sprintf_vref.Deref eenv.valsInScope,
                         vspec_map_tryfind cenv.g.new_format_vref.Deref eenv.valsInScope) with
                  | Some(Lazy(Method(_,_,sprintf_mspec,_,_,_))), Some(Lazy(Method(_,_,new_format_mspec,_,_,_))) ->
                      // The type returned by the 'sprintf' call
                      let funcTy = Pubclo.typ_Func1 cenv.g.ilxPubCloEnv ilty cenv.g.ilg.typ_String
                      // Give the instantiation of the printf format object, i.e. a Format`5 object compatible with StringFormat<ilty>
                      let new_format_mspec = mk_mspec(new_format_mspec.MethodRef,AsObject,
                                                      [// 'T -> string'
                                                       funcTy; 
                                                       // rest follow from 'StringFormat<T>'
                                                       GenType m cenv.g eenv.tyenv cenv.g.unit_ty;  
                                                       cenv.g.ilg.typ_String; 
                                                       cenv.g.ilg.typ_String; 
                                                       cenv.g.ilg.typ_String],[])
                      // Instantiate with our own type
                      let sprintf_mspec = mk_mspec(sprintf_mspec.MethodRef,AsObject,[],[funcTy])
                      // Here's the body of the method. Call printf, then invoke the function it returns
                      let mdef = mk_instance_mdef (debugDisplayMethodName,MemAccess_assembly,[],
                                                   mk_return cenv.g.ilg.typ_Object,
                                                   MethodBody_il
                                                      (mk_ilmbody 
                                                         (true,[],2,
                                                          nonbranching_instrs_to_code 
                                                             [ // load the hardwired format string
                                                               I_ldstr "%+0.8A";  
                                                               // make the printf format object
                                                               mk_normal_newobj new_format_mspec;
                                                               // call sprintf
                                                               mk_normal_call sprintf_mspec; 
                                                               // call the function returned by sprintf
                                                               ldarg_0; 
                                                               mk_IlxInstr (EI_callfunc(Normalcall,Apps_app(ilty, Apps_done cenv.g.ilg.typ_String)));
                                                               mk_normal_newobj (mspec_StringBuilder_string cenv.g.ilg) ],
                                                          None)))
                      yield mdef |> AddSpecialNameFlag 
                  | None,_ ->
                      printfn "sprintf not found"
                      ()
                  | _,None ->
                      printfn "new formatnot found"
                      ()
                  | _ ->
                      printfn "neither found, or non-method"
                      ()

              // Build record constructors and the funky methods that go with records and delegate types. 
              // Constructors and delegate methods have the same access as the representation 
              match tyconRepr with 
              | Some (TRecdRepr _) when not (is_enum_tycon tycon) ->
                 // No constructor for enum types 
                 // Otherwise find all the non-static, non zero-init fields and build a constructor 
                 let relevantFields = 
                     fieldSummaries 
                     |> List.filter (fun (_,_,_,stat,_,_,_,fspec) -> not stat && not fspec.IsZeroInit)

                 let takenFieldNames = 
                     relevantFields
                     |> List.map (fun (_,fldName,_,_,_,propType,_,fspec) -> fldName)
                     |> Set.of_list

                 let fieldNamesAndTypes = 
                     relevantFields
                     |> List.map (fun (_,fldName,_,_,_,propType,_,fspec) -> (fspec.Name,fldName,propType))

                 let mdef = mk_simple_storage_ctor_with_param_names(None, Some cenv.g.ilg.tspec_Object, ilty.TypeSpec, ChooseParamNames fieldNamesAndTypes, reprAccess)

                 yield mdef 
                 // FSharp 1.0 bug 1988: Explicitly setting the ComVisible(true)  attribute on an F# type causes an F# record to be emitted in a way that enables mutation for COM interop scenarios
                 if TryFindBoolAttrib cenv.g cenv.g.attrib_ComVisibleAttribute tycon.Attribs = Some(true) then
                     yield mk_simple_storage_ctor(None, Some cenv.g.ilg.tspec_Object, ilty.TypeSpec, [], reprAccess) 

              | Some (TFsObjModelRepr r) when tycon.IsFSharpDelegateTycon ->

                 // Build all the methods that go with a delegate type 
                 match r.fsobjmodel_kind with 
                 | TTyconDelegate ss ->
                     let p,r = 
                         // When "type delagateTy = delegate of unit -> returnTy",
                         // suppress the unit arg from delagate .Invoke vslot. 
                         let (TSlotSig(nm,typ,ctps,mtps,paraml,returnTy)) = ss
                         let paraml = 
                             match paraml with
                             | [[tsp]] when is_unit_typ cenv.g tsp.Type -> [] (* suppress unit arg *)
                             | paraml -> paraml
                         GenActualSlotsig m cenv eenvinner (TSlotSig(nm,typ,ctps,mtps,paraml,returnTy)) []
                     for mdef in mk_delegate_mdefs cenv.g.ilg (p,r) do
                        yield { mdef with mdAccess=reprAccess }
                 | _ -> 
                     ()

              | _ -> () ]

        let methods = methodDefs @ augmentOverrideMethodDefs @ abstractMethodDefs
        let properties = mk_properties (propertyDefs @ abstractPropDefs)
        let events = mk_events abstractEventDefs
        let fields = mk_fdefs fieldDefs
        
        let tdef = 
           let tdSerializable = (TryFindBoolAttrib cenv.g cenv.g.attrib_AutoSerializableAttribute tycon.Attribs <> Some(false))
                                       
           match tycon.TypeReprInfo with 
           | Some (TILObjModelRepr (_,_,td)) ->
               {td with tdAccess = access;
                        tdCustomAttrs = mk_custom_attrs tdCustomAttrs;
                        tdGenericParams = gparams; }

           | Some (TRecdRepr _ | TFsObjModelRepr _ as tyconRepr)  ->
               let super = super_of_tycon cenv.g tycon
               let super_il = GenType m cenv.g eenvinner.tyenv super
               
               // Build a basic type definition 
               let isObjectType = (match tyconRepr with TFsObjModelRepr _ -> true | _ -> false)
               let attrs = 
                   tdCustomAttrs @ 
                   [mk_CompilationMappingAttr cenv.g
                       (if isObjectType
                        then SourceLevelConstruct_ObjectType
                        elif hiddenRepr then SourceLevelConstruct_RecordType ||| SourceLevelConstruct_PrivateRepresentation
                        else SourceLevelConstruct_RecordType  )]
                                
               let tdef = mk_generic_class(tname,access,gparams,super_il, intfs,mk_mdefs methods,fields,properties,events,mk_custom_attrs attrs)

               // Set some the extra entries in the definition 
               let tcref_is_the_sealed_attribute tcref = tcref_eq cenv.g tcref cenv.g.attrib_SealedAttribute.TyconRef
               let tdef = { tdef with  tdSealed = is_sealed_typ cenv.g thisTy || tcref_is_the_sealed_attribute tcref;
                                       tdSerializable = tdSerializable;
                                       tdMethodImpls=mk_mimpls methodImpls; 
                                       tdAbstract=isAbstract;
                                       tdComInterop=IsComInteropType cenv.g thisTy }

               let tdLayout,tdEncoding = 
                    match TryFindAttrib cenv.g cenv.g.attrib_StructLayoutAttribute tycon.Attribs with
                    | Some (Attrib(_,_,[ AttribInt32Arg(layoutKind) ],namedArgs,m))  -> 
                        let decoder = AttributeDecoder namedArgs
                        let typePack = decoder.FindInt32 "Pack" 0x0
                        let typeSize = decoder.FindInt32 "Size" 0x0
                        let tdEncoding = 
                            match (decoder.FindInt32 "CharSet" 0x0) with
                            (* enumeration values for System.Runtime.InteropServices.CharSet taken from mscorlib.il *)
                            | 0x03 -> TypeEncoding_unicode
                            | 0x04 -> TypeEncoding_autochar
                            | _ -> TypeEncoding_ansi
                        let layoutInfo = 
                            if typePack = 0x0 && typeSize = 0x0 
                            then { typeSize=None; typePack=None } 
                            else { typeSize = Some typeSize; typePack = Some (uint16 typePack) }
                        let tdLayout = 
                          match layoutKind with
                          (* enumeration values for System.Runtime.InteropServices.LayoutKind taken from mscorlib.il *)
                          | 0x0 -> TypeLayout_sequential layoutInfo
                          | 0x2 -> TypeLayout_explicit layoutInfo
                          | _ -> TypeLayout_auto
                        tdLayout,tdEncoding
                    | Some (Attrib(_,_,_,_,m))  -> 
                        errorR(Error("The StructLayout attribute could not be decoded",m));
                        TypeLayout_auto, TypeEncoding_ansi

                    | _ when (match tdKind with TypeDef_valuetype -> true | _ -> false) ->
                        
                        // All structs are sequential by default 
                        // Structs with no instance fields get size 1, pack 0
                        if tycon.AllFieldsAsList |> List.exists (fun f -> not f.IsStatic) ||
                            // Reflection emit doesn't let us emit 'pack' and 'size' for generic structs.
                            // In that case we generate a dummy field instead
                           (cenv.workAroundReflectionEmitBugs && not tycon.TyparsNoRange.IsEmpty) 
                           then 
                            TypeLayout_sequential { typeSize=None; typePack=None }, TypeEncoding_ansi 
                        else
                            TypeLayout_sequential { typeSize=Some 1; typePack=Some 0us }, TypeEncoding_ansi 
                        
                    | _ -> 
                        TypeLayout_auto, TypeEncoding_ansi

               let tdef = { tdef with tdKind =  tdKind; tdLayout=tdLayout; tdEncoding=tdEncoding }
               let tdef = match tdKind with TypeDef_interface -> { tdef with tdExtends = None; tdAbstract=true } | _ -> tdef
               tdef

           | Some (TFiniteUnionRepr _) -> 
               let alternatives = 
                   tycon.UnionCasesArray |> Array.mapi (fun i ucspec -> 
                       { altName=ucspec.ucase_il_name;
                         altFields=GenUnionCaseRef m cenv.g eenvinner.tyenv i ucspec.RecdFieldsArray;
                         altCustomAttrs= mk_custom_attrs (GenAttrs cenv eenv ucspec.ucase_attribs @ [mk_CompilationMappingAttrWithSeqNum cenv.g SourceLevelConstruct_Alternative i]) })

               { tdName = tname;
                 tdLayout = TypeLayout_auto;
                 tdAccess = access;
                 tdGenericParams = gparams;
                 tdCustomAttrs = 
                     mk_custom_attrs (tdCustomAttrs @ 
                                      [mk_CompilationMappingAttr cenv.g
                                          (if hiddenRepr
                                           then SourceLevelConstruct_SumType ||| SourceLevelConstruct_PrivateRepresentation 
                                           else SourceLevelConstruct_SumType  )]);
                 tdInitSemantics=TypeInit_beforefield;      
                 tdSealed=true;
                 tdAbstract=false;
                 tdKind=
                     mk_IlxTypeDefKind
                       (ETypeDef_classunion
                          { cudReprAccess=reprAccess;
                            cudNullPermitted=IsUnionTypeWithNullAsTrueValue cenv.g tycon;
                            cudHelpersAccess=reprAccess;
                            cudHelpers=
                                (not (tcref_eq cenv.g tcref cenv.g.unit_tcr_canon) &&
                                 match TryFindAttrib cenv.g cenv.g.attrib_DefaultAugmentationAttribute tycon.Attribs with
                                 | Some(Attrib(_,_,[ AttribBoolArg (b) ],_,_)) -> b
                                 | Some (Attrib(_,_,_,_,m))  -> 
                                     errorR(Error("The DefaultAugmentation attribute could not be decoded",m));
                                     true
                                 | _ -> 
                                     true) (* not hiddenRepr *)
                            cudDebugProxies= generateDebugProxies;
                            cudDebugDisplayAttributes= ilDebugDisplayAttributes;
                            cudAlternatives= alternatives;
                            cudWhere = None});
                 tdFieldDefs = fields;
                 tdEvents= events;
                 tdProperties = properties;
                 tdMethodDefs= mk_mdefs methods; 
                 tdMethodImpls= mk_mimpls methodImpls; 
                 tdComInterop=false;    
                 tdSerializable= tdSerializable; 
                 tdSpecialName= false;
                 tdNested=mk_tdefs [];
                 tdEncoding= TypeEncoding_autochar;
                 tdImplements= intfs;
                 tdExtends= Some cenv.g.ilg.typ_Object;
                 tdSecurityDecls= mk_security_decls [];
                 tdHasSecurity=false; }

           | _ -> failwith "??"
        mgbuf.AddTypeDef(tref,tdef);

        // If a type has a .cctor, then the outer .cctor must be run before the inner .cctor
      
        if methods |> List.exists (fun md -> md.Name = ".cctor" &&  
                                             Option.for_all CheckCodeDoesSomething md.Code) then
          GenForceOuterInitializationAsPartOfCCtor cenv mgbuf lazyInitInfo tref m


        
/// Generate the type for an F# exception declaration. 
and GenExnDef cenv mgbuf eenv m (exnc:Tycon) =
    let exncref  = mk_local_ecref exnc
    match exnc.ExceptionInfo with 
    | TExnAbbrevRepr _ | TExnAsmRepr _ | TExnNone -> ()
    | TExnFresh _ ->
        let ilty = GenExnType m cenv.g eenv.tyenv exncref
        let tref = ilty.TypeRef
        let reprAccess = ComputeMemberAccess true (IsHiddenTyconRepr eenv.sigToImplRemapInfo exnc)
        let isHidden = IsHiddenTycon eenv.sigToImplRemapInfo exnc
        let access = ComputeTypeAccess tref isHidden
        let reprAccess = ComputeMemberAccess true isHidden
        let fspecs = exnc.TrueInstanceFieldsAsList 

        let propMethodDefs,fieldDefs,propertyDefs,fieldNamesAndTypes = 
            [ for i,fld in markup fspecs do 
               let propName = fld.Name
               let propType = GenType m cenv.g eenv.tyenv fld.FormalType
               let methnm = "get_"^fld.Name
               let fldName = GenFieldName exnc fld 
               let mdef = mk_ldfld_method_def (methnm,reprAccess,false,ilty,fldName,propType)
               let fieldDef = IL.mk_instance_fdef(fldName,propType, None, MemAccess_assembly)
               let ilPropertyDef = 
                     let cc = ILCallingConv.Instance
                     { propName=propName;
                       propRTSpecialName=false;
                       propSpecialName=false;
                       propSet=None;
                       propGet=Some(mk_mref(tref,cc,methnm,0,[],propType));
                       propCallconv=CC_instance;
                       propType=propType;          
                       propInit=None;
                       propArgs=[];
                       propCustomAttrs=mk_custom_attrs (GenAttrs cenv eenv fld.PropertyAttribs @ [mk_CompilationMappingAttrWithSeqNum cenv.g SourceLevelConstruct_Field i]); }
               yield (mdef,fieldDef,ilPropertyDef,(propName,fldName,propType)) ] 
             |> List.unzip4

        let ctorMethodDef = 
            mk_simple_storage_ctor_with_param_names(None, Some cenv.g.ilg.tspec_Exception, ilty.TypeSpec, ChooseParamNames fieldNamesAndTypes, reprAccess) 

        // In compiled code, all exception types get a parameterless constructor for use with XML serialization
        // This does default-initialization of all fields
        let ctorMethodDefNoArgs = 
            if nonNil fieldNamesAndTypes then 
                [ mk_simple_storage_ctor(None, Some cenv.g.ilg.tspec_Exception, ilty.TypeSpec, [], reprAccess) ]
            else
                []


        let ctorMethodDefForSerialization = 
            mk_ctor(MemAccess_family,
                    [mk_named_param("info",cenv.g.ilg.typ_SerializationInfo);mk_named_param("context",cenv.g.ilg.typ_StreamingContext)],
                    mk_impl
                      (false,[],8,
                       nonbranching_instrs_to_code
                          [ ldarg_0; 
                            I_ldarg 1us;
                            I_ldarg 2us;
                            mk_normal_call (mk_ctor_mspec_for_boxed_tspec (cenv.g.ilg.tspec_Exception,[cenv.g.ilg.typ_SerializationInfo;cenv.g.ilg.typ_StreamingContext])) ]
(*
                           preblock @
                           begin 
                             List.concat (List.mapi (fun n (pnm,nm,ty) -> 
                               [ ldarg_0;
                                 I_ldarg (uint16 (n+1));
                                 mk_normal_stfld (mk_fspec_in_boxed_tspec (tspec,nm,ty));
                               ])  flds)
                           end
*)
                       ,None))
                

        let getObjectDataMethodForSerialization = 
            
            let mdef = 
                mk_virtual_mdef
                    ("GetObjectData",MemAccess_public,
                     [mk_named_param ("info",cenv.g.ilg.typ_SerializationInfo);mk_named_param("context",cenv.g.ilg.typ_StreamingContext)], 
                     mk_return Type_void,
                     (let code = 
                        nonbranching_instrs_to_code
                          [ ldarg_0; 
                            I_ldarg 1us;
                            I_ldarg 2us;
                            mk_normal_call (mk_nongeneric_instance_mspec_in_typ (cenv.g.ilg.typ_Exception, "GetObjectData", [cenv.g.ilg.typ_SerializationInfo;cenv.g.ilg.typ_StreamingContext], Type_void))
                          ]
                      MethodBody_il(mk_ilmbody(true,[],8,code, None))))
            // Here we must encode: [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
            // In ILDASM this is: .permissionset demand = {[mscorlib]System.Security.Permissions.SecurityPermissionAttribute = {property bool 'SerializationFormatter' = bool(true)}}
            { mdef with mdSecurityDecls=mk_security_decls [ mk_permission_set cenv.g.ilg (SecAction_demand,[(cenv.g.ilg.tref_SecurityPermissionAttribute,[("SerializationFormatter",cenv.g.ilg.typ_Bool, CustomElem_bool(true))])])];
                        mdHasSecurity=true }
                

                

        let tname = tref.Name
        let compareMethodDefs = 
            (*if isSome exnc.TypeContents.tcaug_compare 
            then [ GenCompareOverride cenv mgbuf eenv m cenv.g.exn_ty (ilty,cenv.g.ilg.typ_Exception) ] 
            else*)
            []
        
        let hashMethodDefs = 
            if isSome exnc.TypeContents.tcaug_hash_and_equals_withc && not exnc.TypeContents.tcaug_hasObjectGetHashCode 
            then [ GenHashOverride cenv mgbuf eenv m exncref ilty ] 
            else []

        let interfaces =  List.map (p13 >> GenType m cenv.g eenv.tyenv) exnc.TypeContents.tcaug_implements
        let tdef = 
          mk_generic_class
            (tname,access,[],cenv.g.ilg.typ_Exception, 
             interfaces,  
 #if BE_SECURITY_TRANSPARENT
             mk_mdefs ([ctorMethodDef] @ compareMethodDefs @ ctorMethodDefNoArgs @ [ ctorMethodDefForSerialization ] @ propMethodDefs @ hashMethodDefs),
 #else
             mk_mdefs ([ctorMethodDef] @ compareMethodDefs @ ctorMethodDefNoArgs @ [ getObjectDataMethodForSerialization; ctorMethodDefForSerialization ] @ propMethodDefs @ hashMethodDefs),
 #endif
             mk_fdefs fieldDefs,
             mk_properties propertyDefs,
             mk_events [],
             mk_custom_attrs [mk_CompilationMappingAttr cenv.g SourceLevelConstruct_Exception])
        let tdef = { tdef with tdSerializable =  true }
        if verbose then dprintf "GenExnDef:  writing results\n";
        mgbuf.AddTypeDef(tref,tdef)

and CodegenAssembly cenv eenv mgbuf fileImpls = 
    if List.length fileImpls > 0 then 
      let a,b = List.frontAndBack fileImpls
      let eenv = List.fold (GenTopImpl cenv mgbuf None) eenv a
      let eenv = GenTopImpl cenv mgbuf cenv.mainMethodInfo eenv b
      () 

//-------------------------------------------------------------------------
// When generating a module we just write into mutable 
// structures representing the contents of the module. 
//------------------------------------------------------------------------- 

let GetEmptyIlxGenEnv ccu = 
    let thisCompLoc = CompLocForCcu ccu
    { tyenv=empty_tyenv;
      cloc = thisCompLoc;
      valsInScope=vspec_map_empty(); 
      someTspecInThisModule=ecmaILGlobals.tspec_Object; (* dummy value *)
      letBoundVars=[];
      liveLocals=Imap.empty();
      innerVals = [];
      sigToImplRemapInfo = []; (* "module remap info" *)
      withinSEH = false;}

type CodegenResults = 
    { ilTypeDefs: ILTypeDef list;
      ilAssemAttrs : ILAttribute list;
      ilNetModuleAttrs: ILAttribute list;
      quotationResourceBytes: byte[] list }


let GenerateCode cenv eenv (TAssembly fileImpls) (assemA,moduleA) =

    (* Generate the implementations into the mgbuf *)
    let mgbuf= new AssemblyBuilder(cenv)
    let eenv = { eenv with cloc = CompLocForFragment cenv.fragName cenv.viewCcu }
    GenTypeDefForCompLoc cenv eenv mgbuf (CompLocForPrivateImplementationDetails eenv.cloc) true [];
    CodegenAssembly cenv eenv mgbuf fileImpls;
    let ilAssemAttrs = GenAttrs cenv eenv assemA
    
    let tdefs,reflectedDefinitions = mgbuf.Close()
    let quotationResourceBytes = 
        match reflectedDefinitions with 
        | [] -> []
        | _ -> 
            if verbose then dprintf "creating quotation resource";
            let defnsResource = 
              reflectedDefinitions |> List.choose (fun (v,e) -> 
                    try 
                      let ety = type_of_expr cenv.g e
                      let tps,taue,tauty = 
                        match e with 
                        | TExpr_tlambda (_,tps,b,_,_,_) -> tps,b,reduce_forall_typ cenv.g ety (List.map mk_typar_ty tps)
                        | _ -> [],e,ety
                      let env = Creflect.BindTypars Creflect.empty_env tps
                      let freeTypes,argExprs, astExpr = Creflect.ConvExprPublic (cenv.g,cenv.amap,cenv.viewCcu) env taue
                      let m = range_of_expr e
                      if nonNil(freeTypes) then error(InternalError("A free type variable was detected in a reflected definition",m));
                      if nonNil(argExprs) then error(Error("Reflected definitions may not contain uses of the prefix splice operator '%'",m));
                      let crenv = Creflect.mk_cenv (cenv.g,cenv.amap,cenv.viewCcu)
                      let mbaseR = Creflect.ConvMethodBase crenv env v
                      
                      Some(mbaseR,astExpr) 
                    with 
                    | Creflect.InvalidQuotedTerm e -> warning(e); None)
              |> Sreflect.PickleDefns
            [ defnsResource ]
    let ilNetModuleAttrs = GenAttrs cenv eenv moduleA

    if verbose then dprintf "codegen complete";
    { ilTypeDefs= tdefs;
      ilAssemAttrs = ilAssemAttrs;
      ilNetModuleAttrs = ilNetModuleAttrs;
      quotationResourceBytes = quotationResourceBytes }

    

//-------------------------------------------------------------------------
// For printing values in fsi we want to lookup the value of given vrefs.
// The storage in the eenv says if the vref is stored in a static field.
// If we know how/where the field was generated, then we can lookup via reflection.
//------------------------------------------------------------------------- 

open System
open System.Reflection
let lookupGeneratedValue ((lookupFieldRef  : ILFieldRef  -> FieldInfo),
                          (lookupILMethodRef : ILMethodRef -> MethodInfo),
                          (lookupTypeRef   : ILTypeRef   -> Type),
                          (lookupType      : ILType   -> Type)) g eenv (v:Val) =
  // Top-level val bindings are stored (for example) in static fields.
  // In the FSI case, these fields are be created and initialised, so we can recover the object.
  // Ilxgen knows how v was stored, and then ilreflect knows how this storage was generated.
  // Ilxgen converts (v:Tast.Val) to AbsIL datatstructures.
  // Ilreflect converts from AbsIL datatstructures to emitted Type, FieldInfo, MethodInfo etc.
  //------
  // The lookup* functions are the conversions available from ilreflect.
  try
    // Convert the v.Type into a System.Type according to ilxgen and ilreflect.
    let objTyp =
        let il_ty = GenType range0 g empty_tyenv v.Type (* empty_tyenv ok, not expecting typars *)
        lookupType il_ty
    // Lookup the compiled v value (as an object).
    // ASIDE: this code like an "immediate" form of GenGetStorageAndSequel
    let storage = storage_for_val range0 v eenv
    match storage with
      | StaticField (fspec,vref,hasLiteralAttr,ilTypeSpecForProperty,fieldName,_,_,ilGetterMethRef,_,_) ->
          let obj =
              if hasLiteralAttr then
                  let staticTyp = lookupTypeRef fspec.EnclosingTypeRef
                  // Checked: This FieldInfo (FieldBuilder) supports GetValue().
                  staticTyp.GetField(fieldName).GetValue(null:obj)
              else
                  let staticTyp = lookupTypeRef ilTypeSpecForProperty.TypeRef
                  // Unfortunately we can not call .Invoke on the ILMethodRef's MethodInfo,
                  // because it is the MethodBuilder and that does not support .Invoke...
                  // Rather, we look for the getter MethodInfo from the built type and .Invoke on that.
                  if ilGetterMethRef.ArgCount <> 0 then
                      failwith "Expected ilGetterMethRef to have no arguments" (* immediately caught below! *)
                  let methInfo = staticTyp.GetMethod(ilGetterMethRef.Name,[||])
                  methInfo.Invoke((null:obj),(null:obj[]))
          Some (obj,objTyp)
      | Null ->
          Some (null,objTyp)
      | Local _ -> None     
      | Method _ -> None
      | Unrealized -> None
      | Arg _ -> None
      | Env _ -> None
  with
    e ->
#if DEBUG      
      printf "ilxGen.lookupGeneratedValue for v=%s caught exception:\n%A\n\n" v.MangledName e
#endif  
      None
    
let lookupGeneratedInfo ((lookupFieldRef  : ILFieldRef  -> FieldInfo),
                          (lookupILMethodRef : ILMethodRef -> MethodInfo),
                          (lookupTypeRef   : ILTypeRef   -> Type),
                          (lookupType      : ILType   -> Type)) g eenv (v:Val) =
  try
    // Convert the v.Type into a System.Type according to ilxgen and ilreflect.
    let objTyp =
        let il_ty = GenType range0 g empty_tyenv v.Type (* empty_tyenv ok, not expecting typars *)
        lookupType il_ty
    // Lookup the compiled v value (as an object).
    let storage = storage_for_val range0 v eenv
    match storage with
      | StaticField (fspec,vref,hasLiteralAttr,ilTypeSpecForProperty,fieldName,_,_,ilGetterMethRef,_,_) ->
          let staticTyp = lookupTypeRef ilTypeSpecForProperty.TypeRef
          if hasLiteralAttr then
              Some (staticTyp.GetField(fieldName) :> MemberInfo)
          else
              Some (staticTyp.GetMethod(ilGetterMethRef.Name,[||]) :> MemberInfo)
      | Null -> None
      | Local _ -> None     
      | Method _ -> None
      | Unrealized -> None
      | Arg _ -> None
      | Env _ -> None
  with
    e ->
#if DEBUG      
      printf "ilxGen.lookupGenertedInfo for v=%s caught exception:\n%A\n\n" v.MangledName e
#endif  
      None
    
    
    
