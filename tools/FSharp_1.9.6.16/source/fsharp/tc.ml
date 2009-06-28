// (c) Microsoft Corporation. All rights reserved

#light

/// The typechecker.  Left-to-right constrained type checking 
/// with generalization at appropriate points.
module (* internal *) Microsoft.FSharp.Compiler.TypeChecker

open Internal.Utilities
open Internal.Utilities.Pervasives
open Microsoft.FSharp.Compiler.AbstractIL 
open Microsoft.FSharp.Compiler.AbstractIL.Internal 
open Microsoft.FSharp.Compiler.AbstractIL.Internal.Library
open Microsoft.FSharp.Compiler.AbstractIL.Diagnostics 

open Microsoft.FSharp.Compiler 

open Microsoft.FSharp.Compiler.Range
open Microsoft.FSharp.Compiler.Ast
open Microsoft.FSharp.Compiler.ErrorLogger
open Microsoft.FSharp.Compiler.Tast
open Microsoft.FSharp.Compiler.Tastops
open Microsoft.FSharp.Compiler.Patcompile
open Microsoft.FSharp.Compiler.Env
open Microsoft.FSharp.Compiler.AbstractIL.IL (* Abstract IL  *)
open Microsoft.FSharp.Compiler.Lib
open Microsoft.FSharp.Compiler.Layout
open Microsoft.FSharp.Compiler.Outcome
open Microsoft.FSharp.Compiler.Infos
open Microsoft.FSharp.Compiler.Infos.AccessibilityLogic
open Microsoft.FSharp.Compiler.Infos.AttributeChecking
open Microsoft.FSharp.Compiler.Typrelns
open Microsoft.FSharp.Compiler.ConstraintSolver
open Microsoft.FSharp.Compiler.Nameres
open Microsoft.FSharp.Compiler.PrettyNaming

let generalizeInnerPolymorphism = true
let polyrec = false
let recTyparsRigid = TyparWarnIfNotRigid
let verboseCC = false
let verboseGeneralization = try System.Environment.GetEnvironmentVariable("FSharp_Verbose") <> null with _ -> false

//-------------------------------------------------------------------------
// Helpers that should be elsewhere
//------------------------------------------------------------------------- 


let isThreadOrContextStatic g attrs' = 
    HasAttrib g g.attrib_ThreadStaticAttribute attrs' ||
    HasAttrib g g.attrib_ContextStaticAttribute attrs' 

let mk_nil_pat g m ty = TPat_unioncase(g.nil_ucref,[ty],[],m)
let mk_cons_pat g ty ph pt = TPat_unioncase(g.cons_ucref,[ty],[ph;pt],union_ranges (RangeOfPat ph) (RangeOfPat pt))

let mk_compgen_let_in m nm ty e f = 
    let v,ve = mk_compgen_local m nm ty
    mk_compgen_let m v e (f (v,ve))

let mk_unit_delay_lambda g m e =
    let uv,ue = mk_compgen_local m "unitVar" g.unit_ty
    mk_lambda m uv (e,type_of_expr g e) 

let mk_coerce_if_needed g tgtTy srcTy expr =
    //if type_definitely_subsumes_type_no_coercion 0 cenv.g cenv.amap m tgtTy srcTy then 
    if type_equiv g tgtTy srcTy then 
        expr
    else 
        mk_coerce(expr,tgtTy,range_of_expr expr,srcTy)


//-------------------------------------------------------------------------
// Errors.
//------------------------------------------------------------------------- 

exception BakedInMemberConstraintName of string * range
exception FunctionExpected of DisplayEnv * Tast.typ * range
exception NotAFunction of DisplayEnv * Tast.typ * range * range
exception Recursion of DisplayEnv * ident * Tast.typ * Tast.typ  * range
exception RecursiveUseCheckedAtRuntime of DisplayEnv * ValRef * range
exception LetRecEvaluatedOutOfOrder of DisplayEnv * ValRef * ValRef * range
exception LetRecCheckedAtRuntime of range
exception LetRecUnsound of DisplayEnv * ValRef list * range
exception TyconBadArgs of DisplayEnv * TyconRef * int * range
exception UnionCaseWrongArguments of DisplayEnv * int * int * range
exception UnionCaseWrongNumberOfArgs of DisplayEnv * int * int * range
exception FieldsFromDifferentTypes of DisplayEnv * RecdFieldRef * RecdFieldRef * range
exception FieldGivenTwice of DisplayEnv * Tast.RecdFieldRef * range
exception MissingFields of string list * range
exception FunctionValueUnexpected of DisplayEnv * Tast.typ * range
exception UnitTypeExpected of DisplayEnv * Tast.typ * bool * range
exception UnionPatternsBindDifferentNames of range
exception VarBoundTwice of ident
exception ValueRestriction of DisplayEnv * bool * Val * Typar * range
exception FieldNotMutable of DisplayEnv * Tast.RecdFieldRef * range
exception ValNotMutable of DisplayEnv * ValRef * range
exception ValNotLocal of DisplayEnv * ValRef * range
exception InvalidRuntimeCoercion of DisplayEnv * Tast.typ * Tast.typ * range
exception IndeterminateRuntimeCoercion of DisplayEnv * Tast.typ * Tast.typ * range
exception IndeterminateStaticCoercion of DisplayEnv * Tast.typ * Tast.typ * range
exception RuntimeCoercionSourceSealed of DisplayEnv * Tast.typ * range
exception CoercionTargetSealed of DisplayEnv * Tast.typ * range
exception UpcastUnnecessary of range
exception TypeTestUnnecessary of range
exception StaticCoercionShouldUseBox of DisplayEnv * Tast.typ * Tast.typ * range
exception SelfRefObjCtor of bool * range
exception VirtualAugmentationOnNullValuedType of range
exception NonVirtualAugmentationOnNullValuedType of range
exception UseOfAddressOfOperator of range
exception DeprecatedThreadStaticBindingWarning of range
exception IntfImplInIntrinsicAugmentation of range
exception IntfImplInExtrinsicAugmentation of range
exception OverrideInIntrinsicAugmentation of range
exception OverrideInExtrinsicAugmentation of range
exception NonUniqueInferredAbstractSlot of TcGlobals * DisplayEnv * string * MethInfo * MethInfo * range
exception IndexOutOfRangeExceptionWarning of range
exception StandardOperatorRedefinitionWarning of string * range
       

/// Is this a 'base' call (in the sense of C#) 
let IsBaseCall objArgs = 
    match objArgs with 
    | [TExpr_val(v,_,_)] when v.BaseOrThisInfo  = BaseVal -> true
    | _ -> false

let RecdFieldInstanceChecks g ad m (rfinfo:RecdFieldInfo) = 
    if rfinfo.IsStatic then error (Error ("a static field was used where an instance field is expected",m));
    CheckRecdFieldInfoAttributes g rfinfo m |> CommitOperationResult;        
    CheckRecdFieldInfoAccessible m ad rfinfo

let ILFieldInstanceChecks  g amap ad m (finfo :ILFieldInfo) =
    if finfo.IsStatic then error (Error ("a static field was used where an instance field is expected",m));
    CheckILFieldInfoAccessible g amap m ad finfo;
    CheckILFieldAttributes g finfo m

let MethInfoChecks g amap isInstance objArgs ad m (minfo:MethInfo)  =
    if minfo.IsInstance <> isInstance then
      error (Error (minfo.LogicalName^" is not "^(if isInstance then "an instance" else "a static")^" method",m));

    // Eliminate the 'protected' portion of the accessibility domain for instance accesses
    let ad = 
        match objArgs,ad with 
        | [objArg],AccessibleFrom(paths,Some tcref) -> 
            let objArgTy = type_of_expr g objArg 
            let ty = snd (generalize_tcref tcref)
            // We get to keep our rights if the type we're in subsumes the object argument type
            if type_feasibly_subsumes_type 0 g amap m ty CanCoerce objArgTy then
                ad
            // We get to keep our rights if this is a base call
            elif IsBaseCall objArgs then 
                ad
            else
                AccessibleFrom(paths,None) 
        | _ -> ad

    if not (IsMethInfoAccessible amap m  ad minfo) then 
      error (Error ("method '"^minfo.LogicalName^"' is not accessible from this code location",m));
    CheckMethInfoAttributes g m minfo |> CommitOperationResult


let CheckRecdFieldMutation m denv (rfinfo:RecdFieldInfo) ftinst = 
    if not rfinfo.RecdField.IsMutable then error (FieldNotMutable(denv,rfinfo.RecdFieldRef,m));
    if nonNil(ftinst) then error (Error("Fields which are type functions may not be mutated",m))

//-------------------------------------------------------------------------
// Type environments. 
//    - Named items in scope (values)
//    - Record of type variables that can't be generalized
//    - Our 'location' as a concrete compilation path
//    - mutable accumulator for the module type currently being accumulated 
//------------------------------------------------------------------------- 

[<StructuralEquality(false); StructuralComparison(false)>]
type UngeneralizableItem = 
      { computeFreeTyvars : (unit -> FreeTyvars);
        // Flag is for: have we determined that this item definitely has 
        // no free type inference variables? This implies that
        //   (a)  it will _never_ have any free type inference variables as further constraints are added to the system.
        //   (b)  its set of FreeTycons will not change as further constraints are added to the system
        mutable WillNeverHaveFreeTypars: bool;
        // If WillNeverHaveFreeTypars then we can cache the computation of FreeTycons, since they are invariant.
        mutable CachedFreeLocalTycons: FreeTycons; 
        // If WillNeverHaveFreeTypars then we can cache the computation of FreeTraitSolutions, since they are invariant.
        mutable CachedFreeTraitSolutions: FreeLocals; }

      member item.GetFreeTyvars() = 
          let fvs = item.computeFreeTyvars()
          if fvs.FreeTypars.IsEmpty then 
              item.WillNeverHaveFreeTypars <- true; 
              item.CachedFreeLocalTycons <- fvs.FreeTycons
              item.CachedFreeTraitSolutions <- fvs.FreeTraitSolutions
          fvs
      
[<StructuralEquality(false); StructuralComparison(false)>]
type tcEnv =
    { /// Name resoultion information 
      eNameResEnv : NameResolutionEnv; 

      /// The list of items in the environment that may contain free inference 
      /// variables (which may not be generalized). The relevant types may 
      /// change as a result of inference equations being asserted, hence may need to 
      /// be recomputed. 
      eUngeneralizableItems: UngeneralizableItem list;
      
      // Two (!) versions of the current module path 
      // These are used to: 
      //    - Look up the appropriate point in the corresponding signature 
      //      see if an item is public or not 
      //    - Change fslib canonical module type to allow compiler references to these items 
      //    - Record the cpath for concrete modul_specs, tycon_specs and excon_specs so they can cache their generated IL representation where necessary 
      //    - Record the pubpath of public, concrete {val,tycon,modul,excon}_specs.  
      //      This information is used mainly when building non-local references 
      //      to public items. 
      // 
      // Of the two, 'ePath' is the one that's barely used. It's only 
      // used by curr_stable_fslib_nlpath to modify the CCU while compiling FSharp.Core
      ePath: ident list; 
      eCompPath: CompilationPath; 
      eAccessPath: CompilationPath; 
      eInternalsVisibleCompPaths: CompilationPath list; // internals under these should be accessible

      /// Mutable accumulator for the current module type 
      eMtypeAcc: ModuleOrNamespaceType ref; 

      /// Here Some(tcref) indicates we can access protected members in all super types 
      eFamilyType: TyconRef option; 

      // Information to enforce special restrictions on valid expressions 
      // for .NET constructors. 
      eCtorInfo : ctorInfo option
    } 
    member tenv.DisplayEnv = tenv.eNameResEnv.DisplayEnv

and ctorInfo = 
    {
      // Object model constructors have a very specific form to satisfy .NET limitations.
      // For "new = \arg. { new C with ... }"; 
      //     ctor = 3 indicates about to type check "\arg. (body)", 
      //     ctor = 2 indicates about to type check "body" 
      //     ctor = 1 indicates actually type checking the body expression 
      // 0 indicates everywhere else, including auxiliary expressions such e1 in "let x = e1 in { new ... }" 
      // REVIEW: clean up this rather odd approach ... 
      ctorShapeCounter: int;
      
      /// A handle to a reference cell to hold results of initialized 'this' for 'type X() as x = ...' constructs 
      /// The reference cell is used throughout the object constructor for any recursive references 
      /// to 'this'. 
      ctorThisRefCellVarOpt: ValRef option; 
      
      /// Are we in an object constructor, and if so have we complete construction of the 
      /// ctor?  Used to reduce #of reactive recursion warnings 
      ctorPreConstruct: bool;
      ctorIsImplicit: bool  
    }
    
let empty_tenv g =
    let cpath = CompPath (IL.ecma_mscorlib_scoref,[])
    { eNameResEnv = NameResolutionEnv.Empty(g);
      eUngeneralizableItems=[];
      ePath=[];
      eCompPath=cpath; (* dummy *)
      eAccessPath=cpath; (* dummy *)
      eInternalsVisibleCompPaths=[];
      eMtypeAcc= ref (empty_mtype Namespace);
      eFamilyType=None;
      eCtorInfo=None }

let nenv_of_tenv tenv = tenv.eNameResEnv
let items_of_tenv tenv = tenv.eNameResEnv.UnqualifiedItems

//-------------------------------------------------------------------------
// Helpers related to determining if we're in a constructor and/or a class
// that may be able to access "protected" members.
//------------------------------------------------------------------------- 

let InitialExplicitCtorInfo(ctorThisVarRefCellOpt) =
    { ctorShapeCounter=3; 
      ctorThisRefCellVarOpt = ctorThisVarRefCellOpt;
      ctorPreConstruct=true; 
      ctorIsImplicit=false} 

let InitialImplicitCtorInfo() =
    { ctorShapeCounter=0; 
      ctorThisRefCellVarOpt = None; 
      ctorPreConstruct=true; 
      ctorIsImplicit=true }
      
let EnterFamilyRegion tcref env = { env with eFamilyType = Some tcref }
let ExitFamilyRegion env = 
    match env.eFamilyType with 
    | None -> env  // optimization to avoid reallocation  
    | _ -> { env with eFamilyType = None }

let AreWithinCtorShape         env = match env.eCtorInfo with None -> false    | Some ctorInfo -> ctorInfo.ctorShapeCounter > 0
let AreWithinCtorPreConstruct  env = match env.eCtorInfo with None -> false    | Some ctorInfo -> ctorInfo.ctorPreConstruct 
let AreWithinImplicitCtor      env = match env.eCtorInfo with None -> false    | Some ctorInfo -> ctorInfo.ctorIsImplicit
let GetCtorShapeCounter        env = match env.eCtorInfo with None -> 0        | Some ctorInfo -> ctorInfo.ctorShapeCounter
let GetRecdInfo                env = match env.eCtorInfo with None -> RecdExpr | Some ctorInfo -> if ctorInfo.ctorShapeCounter = 1 then RecdExprIsObjInit else RecdExpr

let ExitCtorPreConstructRegion    env = {env with eCtorInfo = Option.map (fun ctorInfo -> { ctorInfo with ctorPreConstruct = false }) env.eCtorInfo }
let AdjustCtorShapeCounter      f env = {env with eCtorInfo = Option.map (fun ctorInfo -> { ctorInfo with ctorShapeCounter = f ctorInfo.ctorShapeCounter }) env.eCtorInfo }
let ExitCtorShapeRegion           env = AdjustCtorShapeCounter (fun x -> 0) env

//-------------------------------------------------------------------------
// Add stuff to environments and register things as ungeneralizeable.
//------------------------------------------------------------------------- 

let free_tyvars_is_empty ftyvs = 
    Zset.is_empty ftyvs.FreeTypars &&
    Zset.is_empty ftyvs.FreeTycons 

let add_free_item_of_typ typ eUngeneralizableItems = 
    let fvs = free_in_type CollectAllNoCaching typ
    if free_tyvars_is_empty fvs then eUngeneralizableItems 
    else { computeFreeTyvars = (fun () -> free_in_type CollectAllNoCaching typ);
           WillNeverHaveFreeTypars=false; 
           CachedFreeLocalTycons=empty_free_loctycons; 
           CachedFreeTraitSolutions=empty_free_locvals } :: eUngeneralizableItems

let rec acc_free_in_mtyp (mtyp:ModuleOrNamespaceType) acc =
    NameMap.foldRange (type_of_val >> acc_free_in_type CollectAllNoCaching) mtyp.AllValuesAndMembers
      (NameMap.foldRange (fun (mspec:ModuleOrNamespace) acc -> acc_free_in_mtyp mspec.ModuleOrNamespaceType acc) mtyp.ModulesAndNamespacesByDemangledName acc)
let free_in_mtyp mtyp = acc_free_in_mtyp mtyp empty_free_tyvars

let add_free_item_of_mtyp mtyp eUngeneralizableItems = 
    let fvs = free_in_mtyp mtyp
    if free_tyvars_is_empty fvs then eUngeneralizableItems 
    else { computeFreeTyvars = (fun () -> free_in_mtyp mtyp);
           WillNeverHaveFreeTypars=false; 
           CachedFreeLocalTycons=empty_free_loctycons; 
           CachedFreeTraitSolutions=empty_free_locvals } :: eUngeneralizableItems

let add_vrefs_to_nenv vs nenv = 
    NameMap.foldRange (fun v nenv -> AddValRefToNameEnv (mk_local_vref v) nenv) vs nenv
    
    
let add_internals_accessibility env (ccu:ccu) =
    let compPath = CompPath (ccu.ILScopeRef,[])    
    {env with eInternalsVisibleCompPaths = compPath :: env.eInternalsVisibleCompPaths }

let curr_cpath env = env.eCompPath
let curr_access_cpath env = env.eAccessPath
let AccessRightsOfEnv env = AccessibleFrom (curr_access_cpath env :: env.eInternalsVisibleCompPaths ,env.eFamilyType)

let ModifyNameResEnv f env = { env with eNameResEnv = f env.eNameResEnv } 

let AddLocalValPrimitive (v:Val) env =
    let env = ModifyNameResEnv (AddValRefToNameEnv (mk_local_vref v)) env
    {env with eUngeneralizableItems =  add_free_item_of_typ v.Type env.eUngeneralizableItems;   } 


let AddLocalValMap scopem (vals:Val NameMap) env =
    let env = ModifyNameResEnv (add_vrefs_to_nenv vals) env
    let env = {env with eUngeneralizableItems =  NameMap.foldRange (type_of_val >> add_free_item_of_typ) vals env.eUngeneralizableItems;   }
    CallEnvSink(scopem,nenv_of_tenv env,AccessRightsOfEnv env);
    env

let AddLocalVal scopem v env = 
    let env = ModifyNameResEnv (AddValRefToNameEnv (mk_local_vref v)) env
    let env = {env with eUngeneralizableItems =  add_free_item_of_typ v.Type env.eUngeneralizableItems;   }
    CallEnvSink(scopem,nenv_of_tenv env,AccessRightsOfEnv env);
    env

let AddLocalExnDefn scopem (exnc:Tycon) env =
    let env = ModifyNameResEnv (AddExceptionDeclsToNameEnv (mk_local_ecref exnc)) env
    (* Also make VisualStudio think there is an identifier in scope at the range of the identifier text of its binding location *)
    CallEnvSink(exnc.Range,nenv_of_tenv env,AccessRightsOfEnv env);
    CallEnvSink(scopem,nenv_of_tenv env,AccessRightsOfEnv env);
    env
 
let AddLocalTycons g amap m tycons env = 
     ModifyNameResEnv (AddTyconRefsToNameEnv g amap m (List.map mk_local_tcref tycons)) env 

let AddLocalTyconsAndReport g amap scopem tycons env = 
    let env = AddLocalTycons g amap scopem tycons env
    CallEnvSink(scopem,nenv_of_tenv env,AccessRightsOfEnv env);
    env

//-------------------------------------------------------------------------
// Open a structure or an IL namespace 
//------------------------------------------------------------------------- 



let top_modrefs_of_nonlocal_ccu (ccu:ccu) = ccu.TopModulesAndNamespaces |> NameMap.map (mk_nonlocal_ccu_top_tcref ccu)
let top_tcrefs_of_nonlocal_ccu (ccu:ccu)  = ccu.TopTypeAndExceptionDefinitions |> List.map (mk_nonlocal_ccu_top_tcref ccu)

let modrefs_of_top_rooted_mtyp (mtyp:ModuleOrNamespaceType) = mtyp.ModulesAndNamespacesByDemangledName |> NameMap.map mk_local_modref
let tcrefs_of_top_rooted_mtyp (mtyp:ModuleOrNamespaceType)  = mtyp.TypeAndExceptionDefinitions |> List.map mk_local_tcref

let open_modul g amap scopem env modref =
    let env = ModifyNameResEnv (AddModuleOrNamespaceContentsToNameEnv g amap (AccessRightsOfEnv env) scopem modref) env
    CallEnvSink(scopem,nenv_of_tenv env,AccessRightsOfEnv env);
    env

let open_moduls g amap scopem env mvvs =
    List.foldBack (fun (_,modref,_) acc -> open_modul g amap scopem acc modref) mvvs env

let add_top_rooted_mrefs g amap m env modrefs  = ModifyNameResEnv (AddModrefsToNameEnv g amap m true (AccessRightsOfEnv env) modrefs) env

let add_nonlocal_ccu g amap scopem env (ccu,internalsVisible) = 
    let env = add_top_rooted_mrefs g amap scopem env (top_modrefs_of_nonlocal_ccu ccu)
    let env = ModifyNameResEnv (AddTyconRefsToNameEnv g amap scopem (top_tcrefs_of_nonlocal_ccu ccu)) env
    let env = if internalsVisible then add_internals_accessibility env ccu else env
    CallEnvSink(scopem,nenv_of_tenv env,AccessRightsOfEnv env);
    env

let AddLocalTopRootedModuleOrNamespace g amap scopem env mtyp = 
    let env = add_top_rooted_mrefs g amap scopem env (modrefs_of_top_rooted_mtyp mtyp)
    let env = ModifyNameResEnv (AddTyconRefsToNameEnv g amap scopem (tcrefs_of_top_rooted_mtyp mtyp)) env
    let env = {env with eUngeneralizableItems = add_free_item_of_mtyp mtyp env.eUngeneralizableItems}
    CallEnvSink(scopem,nenv_of_tenv env,AccessRightsOfEnv env);
    env

let AddModuleAbbreviation scopem id modrefs env =
    let env = ModifyNameResEnv (AddModuleAbbrevToNameEnv id modrefs) env
    CallEnvSink(scopem,nenv_of_tenv env,AccessRightsOfEnv env);
    CallNameResolutionSink(id.idRange,nenv_of_tenv env,Item_modrefs(modrefs),ItemOccurence.Use,env.DisplayEnv,AccessRightsOfEnv env)
    env

let AddLocalSubModule g amap m scopem env nm (modul:ModuleOrNamespace) =
    let env = ModifyNameResEnv (AddModrefToNameEnv g amap m false (AccessRightsOfEnv env) (mk_local_modref modul)) env
    let env = {env with eUngeneralizableItems = add_free_item_of_mtyp modul.ModuleOrNamespaceType env.eUngeneralizableItems}
    CallEnvSink(scopem,nenv_of_tenv env,AccessRightsOfEnv env);
    env
 
let RegisterDeclaredTypars typars env = 
    {env with eUngeneralizableItems =  List.foldBack (mk_typar_ty >> add_free_item_of_typ) typars env.eUngeneralizableItems }

let AddDeclaredTypars check typars env = 
    let env = ModifyNameResEnv (AddDeclaredTyparsToNameEnv check typars) env
    RegisterDeclaredTypars typars env

/// Compilation environment for typechecking a compilation unit. Contains the
/// F# and .NET modules loaded from disk, the search path, a table indicating
/// how to List.map F# modules to assembly names, and some nasty globals 
/// related to type inference. These are:
///   - all the type variables generated for this compilation unit
///   - the set of active fixups for "letrec" type inference 
[<StructuralEquality(false); StructuralComparison(false)>]
type cenv = 
    { 

      g: Env.TcGlobals;

      /// Push an entry every time a recursive value binding is used, 
      /// in order to be able to fix up recursive type applications as 
      /// we infer type parameters 
      /// REVIEW: cleanup this use of mutation? 
      mutable recUses: ValMultiMap<(expr ref * range * bool)>;
      
      /// Are we in a script? if so relax the reporting of discarded-expression warnings at the top level
      isScript: bool; 

      /// Environment needed to convert IL types to F# types in the importer. 
      amap: Import.ImportMap; 

      /// Holds a reference to the component being compiled. 
      /// This field is very rarely used (mainly when fixing up forward references to fslib. 
      topCcu: ccu; 
      
      /// Holds the current inference constraints 
      css: ConstraintSolverState;
      
      /// Are we compiling the signature of a module from fslib? 
      compilingCanonicalFslibModuleType: bool;
      isSig: bool;
      haveSig: bool;
      
      niceNameGen: NiceNameGenerator;
      infoReader: InfoReader;
      nameResolver: NameResolver;
      
      conditionalDefines: string list;
            
  }

let new_cenv (g,isScript,niceNameGen,amap,topCcu,isSig,haveSig,conditionalDefines) =
    
    let infoReader = new InfoReader(g,amap)
    let instantiationGenerator m tpsorig = ConstraintSolver.freshen_tps m tpsorig
    let nameResolver = new NameResolver(g,amap,infoReader,instantiationGenerator)
    { g=g;
      amap= amap;
      recUses=vspec_mmap_empty(); 
      topCcu = topCcu;
      isScript=isScript;
      css= { css_g=g; css_amap=amap; css_cxs= Hashtbl.create 10; css_InfoReader=infoReader } ;
      infoReader=infoReader;
      nameResolver=nameResolver;
      niceNameGen=niceNameGen;
      isSig=isSig;
      haveSig=haveSig;
      compilingCanonicalFslibModuleType=(isSig || not haveSig) && g.compilingFslib;
      conditionalDefines=conditionalDefines }


let new_anon_inference_var cenv info = ConstraintSolver.new_anon_inference_var info
let new_compgen_inference_var cenv info = ConstraintSolver.new_compgen_inference_var info
let new_error_tyvar cenv () = ConstraintSolver.new_error_tyvar ()
let new_inference_typ cenv () = ConstraintSolver.new_inference_typ ()
let new_error_typ cenv () = ConstraintSolver.new_error_typ ()
let new_error_measure cenv () = ConstraintSolver.new_error_measure ()
let new_inference_typs cenv l = ConstraintSolver.new_inference_typs l
let copy_and_fixup_typars m rigid tpsorig = ConstraintSolver.freshen_and_fixup_typars m rigid [] [] tpsorig
let new_tinst cenv m tpsorig = ConstraintSolver.new_tinst m tpsorig
let freshen_tps cenv m tpsorig = ConstraintSolver.freshen_tps m tpsorig
let FreshenMethInfo cenv m minfo = ConstraintSolver.FreshenMethInfo m minfo
let unifyE cenv (env:tcEnv) m ty1 ty2 = ConstraintSolver.AddCxTypeEqualsType env.DisplayEnv cenv.css m ty1 ty2



//-------------------------------------------------------------------------
// Generate references to the module being generated - used for
// public items.
//------------------------------------------------------------------------- 


let MakeInnerEnv env nm modKind = 
    let path = env.ePath @ [nm]
    (* Note: here we allocate a new module type accumulator *)
    let mtypeAcc = ref (empty_mtype modKind)
    let cpath = mk_cpath env.eCompPath nm.idText modKind
    { env with ePath = path; 
               eCompPath = cpath;
               eAccessPath = cpath;
               eNameResEnv = { env.eNameResEnv with eDisplayEnv = denv_add_open_path (path_of_lid path) env.DisplayEnv};
               eMtypeAcc = mtypeAcc  },mtypeAcc


let MkInnerEnvForTyconRef cenv env tcref isExtrinsicExtension = 
    if isExtrinsicExtension then 
        // Extension members don't get access to protected stuff 
        env  
    else
        // Regular members get access to protected stuff 
        let env = (EnterFamilyRegion tcref env) 
        // Note: assumes no nesting 
        let env = { env with eAccessPath = mk_cpath env.eCompPath tcref.MangledName FSharpModule }

        env

let MakeInnerEnvForMember cenv env (v:Val) = 
    match v.MemberInfo with 
    | None -> env
    | Some(memberInfo) -> MkInnerEnvForTyconRef cenv env v.MemberApparentParent v.IsExtensionMember

let GetCurrAccumulatedModuleOrNamespaceType env = !(env.eMtypeAcc) 
let SetCurrAccumulatedModuleOrNamespaceType env x =  env.eMtypeAcc := x

/// Optimized unification routine that avoids creating new inference 
/// variables unnecessarily
let UnifyTupleType cenv denv m ty ps = 
    let ptys = 
        if is_tuple_typ cenv.g ty then 
            let ptys = dest_tuple_typ cenv.g ty
            if (List.length ps) = (List.length ptys) then ptys 
            else new_inference_typs cenv ps
        else new_inference_typs cenv ps
    AddCxTypeEqualsType denv cenv.css m ty (TType_tuple ptys);
    ptys

/// Optimized unification routine that avoids creating new inference 
/// variables unnecessarily
let UnifyFunctionTypeUndoIfFailed cenv denv m ty =
    if verbose then  dprintf "--> UnifyFunctionTypeUndoIfFailed\n";
    if is_fun_typ cenv.g ty then Some(dest_fun_typ cenv.g ty) else
    let domainTy = new_inference_typ cenv ()
    let resultTy = new_inference_typ cenv ()
    if AddCxTypeEqualsTypeUndoIfFailed  denv cenv.css m ty (domainTy --> resultTy) then 
        Some(domainTy,resultTy)
    else 
        None

/// Optimized unification routine that avoids creating new inference 
/// variables unnecessarily
let UnifyFunctionType extraInfo cenv denv funm ty =
    match UnifyFunctionTypeUndoIfFailed cenv denv funm ty with
    | Some res -> res
    | None -> 
        match extraInfo with 
        | Some argm -> error (NotAFunction(denv,ty,funm,argm))
        | None ->    error (FunctionExpected(denv,ty,funm))


let UnifyUnitType cenv denv m ty exprOpt =
    if not (AddCxTypeEqualsTypeUndoIfFailed denv cenv.css m ty cenv.g.unit_ty) then 
      let domainTy = new_inference_typ cenv ()
      let resultTy = new_inference_typ cenv ()
      if AddCxTypeEqualsTypeUndoIfFailed denv cenv.css m ty (domainTy --> resultTy) then 
          warning (FunctionValueUnexpected(denv,ty,m))
      else
          let perhapsProp = 
              type_equiv cenv.g cenv.g.bool_ty ty &&
              match exprOpt with 
              | Some(TExpr_app(TExpr_val(vf,_,_),_,_,[_;_],_)) when vf.MangledName = opname_Equals -> true
              | _ -> false
          warning (UnitTypeExpected (denv,ty,perhapsProp,m)); 
      false
    else
      true

//-------------------------------------------------------------------------
// Attribute target flags
//------------------------------------------------------------------------- 

// See System.AttributeTargets
let attrTgtAssembly    = 0x00000001
let attrTgtModule      = 0x00000002
let attrTgtClass       = 0x00000004
let attrTgtStruct      = 0x00000008
let attrTgtEnum        = 0x00000010
let attrTgtConstructor = 0x00000020
let attrTgtMethod      = 0x00000040
let attrTgtProperty    = 0x00000080
let attrTgtField       = 0x00000100
let attrTgtEvent       = 0x00000200
let attrTgtInterface   = 0x00000400
let attrTgtParameter   = 0x00000800
let attrTgtDelegate    = 0x00001000
let attrTgtReturnValue = 0x00002000
let attrTgtGenericParameter = 0x00004000
let attrTgtAll         = 0x00007FFF

let attrTgtBinding    = attrTgtField    ||| attrTgtMethod    ||| attrTgtEvent    ||| attrTgtProperty
let attrTgtFieldDecl  = attrTgtField    ||| attrTgtProperty
let attrTgtUnionCaseDecl = attrTgtMethod    ||| attrTgtProperty
let attrTgtTyconDecl  = attrTgtClass    ||| attrTgtInterface ||| attrTgtDelegate ||| attrTgtStruct ||| attrTgtEnum
let attrTgtExnDecl    = attrTgtClass
let attrTgtModuleDecl = attrTgtClass
let attrTgtTop        = attrTgtAssembly ||| attrTgtModule    ||| attrTgtMethod

/// Typecheck constant terms in expressions and patterns
let TcConst cenv ty m env c =
    let rec TcMeasure ms =
        match ms with
        | Measure_One -> MeasureOne      
        | Measure_Con(tc,m) ->
            let ad = AccessRightsOfEnv env
            let tcref = ForceRaise(ResolveTypeLongIdent cenv.nameResolver ItemOccurence.Use OpenQualified env.eNameResEnv ad tc 0)
            match tcref.TypeOrMeasureKind with
            | KindType -> error(Error("Expected unit-of-measure, not type", m))
            | KindMeasure -> MeasureCon tcref

        | Measure_Power(ms, exponent, m) -> MeasurePower (TcMeasure ms) exponent
        | Measure_Prod(ms1,ms2,m) -> MeasureProd(TcMeasure ms1, TcMeasure ms2)     
        | Measure_Quot(ms1, ((Measure_Seq (_::(_::_), _)) as ms2), m) -> 
          (warning(Error("Implicit product of measures following /",m));
          MeasureProd(TcMeasure ms1, MeasureInv (TcMeasure ms2)))
        | Measure_Quot(ms1,ms2,m) -> 
          MeasureProd(TcMeasure ms1, MeasureInv (TcMeasure ms2))
        | Measure_Seq(mss,m) -> ProdMeasures (List.map TcMeasure mss)
        | Measure_Anon _ -> error(InternalError("Unexpected Measure_Anon",m))
        | Measure_Var(_,m) -> error(Error("Non-zero constants cannot have generic units. For generic zero, write 0.0<_>",m))
   
    let unif ty2 = unifyE cenv env m ty ty2
    let unif_measure_arg iszero tcr c =
        let measureTy = 
            match c with 
            | Const_measure(_, Measure_Anon _) ->
              (mk_tyapp_ty tcr [TType_measure (MeasureVar (new_anon_inference_var cenv (KindMeasure,m,TyparAnon,(if iszero then NoStaticReq else HeadTypeStaticReq),NoDynamicReq)))])

            | Const_measure(_, ms) -> TType_app(tcr, [TType_measure (TcMeasure ms)])
            | _ -> TType_app(tcr, [TType_measure MeasureOne])
        unif measureTy

   
    match c with 
    | Const_unit         -> unif cenv.g.unit_ty;       TConst_unit
    | Const_bool i       -> unif cenv.g.bool_ty;       TConst_bool i
    | Const_int8 i       -> unif cenv.g.sbyte_ty;      TConst_sbyte i
    | Const_int16 i      -> unif cenv.g.int16_ty;      TConst_int16 i
    | Const_int32 i      -> unif cenv.g.int_ty;        TConst_int32 i
    | Const_int64 i      -> unif cenv.g.int64_ty;      TConst_int64 i
    | Const_nativeint i  -> unif cenv.g.nativeint_ty;  TConst_nativeint i
    | Const_uint8 i      -> unif cenv.g.byte_ty;       TConst_byte i
    | Const_uint16 i     -> unif cenv.g.uint16_ty;     TConst_uint16 i
    | Const_uint32 i     -> unif cenv.g.uint32_ty;     TConst_uint32 i
    | Const_uint64 i     -> unif cenv.g.uint64_ty;     TConst_uint64 i
    | Const_unativeint i -> unif cenv.g.unativeint_ty; TConst_unativeint i
    | Const_measure(Const_float32 f, _) | Const_float32 f -> unif_measure_arg (f=0.0f) cenv.g.pfloat32_tcr c; TConst_float32 f
    | Const_measure(Const_float   f, _) | Const_float   f -> unif_measure_arg (f=0.0)  cenv.g.pfloat_tcr c; TConst_float f
    | Const_measure(Const_decimal s, _) | Const_decimal s -> unif_measure_arg false    cenv.g.pdecimal_tcr c; TConst_decimal s
    | Const_measure(Const_int8   i, _)  | Const_int8    i -> unif_measure_arg (i=0y)   cenv.g.pint8_tcr c; TConst_sbyte i
    | Const_measure(Const_int16   i, _) | Const_int16   i -> unif_measure_arg (i=0s)   cenv.g.pint16_tcr c; TConst_int16 i
    | Const_measure(Const_int32   i, _) | Const_int32   i -> unif_measure_arg (i=0)    cenv.g.pint_tcr c; TConst_int32 i
    | Const_measure(Const_int64   i, _) | Const_int64   i -> unif_measure_arg (i=0L)   cenv.g.pint64_tcr c; TConst_int64 i
    | Const_char c       -> unif cenv.g.char_ty;       TConst_char c
    | Const_string (s,m) -> unif cenv.g.string_ty;     TConst_string s
    | Const_bignum s     -> error(InternalError("Unexpected big rational constant", m))
    | Const_measure _    -> error(Error("Units-of-measure supported only on float, float32, decimal and signed integer types", m))

    | Const_uint16array arr -> error(InternalError("Unexpected Const_uint16array",m))
    | Const_bytearray _ -> error(InternalError("Unexpected Const_bytearray",m))
 
/// Convert an Abstract IL ILFieldInit value read from .NET metadata to a TAST constant
let TcFieldInit m lit = 
    match lit with 
    | FieldInit_string s   -> TConst_string s
    | FieldInit_ref       -> error (InternalError("An error occurred importing a literal value for a field",m))
    | FieldInit_bool    b -> TConst_bool b
    | FieldInit_char    c -> TConst_char (char (int c))
    | FieldInit_int8    x -> TConst_sbyte x
    | FieldInit_int16   x -> TConst_int16 x
    | FieldInit_int32   x -> TConst_int32 x
    | FieldInit_int64   x -> TConst_int64 x
    | FieldInit_uint8   x -> TConst_byte x
    | FieldInit_uint16  x -> TConst_uint16 x
    | FieldInit_uint32  x -> TConst_uint32 x
    | FieldInit_uint64  x -> TConst_uint64 x
    | FieldInit_single f -> TConst_float32 f
    | FieldInit_double f -> TConst_float f 


(*-------------------------------------------------------------------------
 * Arities. These serve two roles in the system: 
 *  1. syntactic arities come from the syntactic forms found
 *     signature files and the syntactic forms of function and member definitions.
 *  2. compiled arities representing representation choices w.r.t. internal representations of
 *     functions and members.
 *------------------------------------------------------------------------- *)

// Adjust the arities that came from the parsing of the toptyp (arities) to be a valSynData. 
// This means replacing the "[unitArg]" arising from a "unit -> ty" with a "[]".
let AdjustValSynInfoInSignature g ty (ValSynInfo(argsData,retData) as sigMD) = 
    if is_fun_typ g ty && type_equiv g g.unit_ty (domain_of_fun_typ g ty) && argsData.Length >= 1 && argsData.Head.Length >= 1  then 
        ValSynInfo(argsData.Head.Tail :: argsData.Tail, retData)
    else 
        sigMD 

/// The TopValInfo for a value, except the number of typars is not yet inferred 
type PartialTopValInfo = PartialTopValInfo of TopArgInfo list list * TopArgInfo 

let TranslateTopArgSynInfo isArg m tcAttribute (ArgSynInfo(attrs,isOpt,nm)) = 
    let optAttrs = if  isOpt then [Attr(path_to_lid m ["Microsoft";"FSharp";"Core";"OptionalArgument"], mksyn_unit m,None,m)] else []
    if isArg && nonNil(attrs) && isNone(nm) then 
        warning(Error("A parameter with attributes must also be given a name, e.g. '[<Attribute>] paramName : paramType'",m));

    if not(isArg) && isSome(nm) then 
        errorR(Error("Return values may not have names",m));
       
    TopArgInfo(tcAttribute (optAttrs@attrs),nm)

/// Members have an arity inferred from their syntax. This "valSynData" is not quite the same as the arities 
/// used in the middle and backends of the compiler ("topValInfo"). 
/// "0" in a valSynData (see Ast.arity_of_pat) means a "unit" arg in a topValInfo 
/// Hence remove all "zeros" from arity and replace them with 1 here. 
/// Note we currently use the compiled form for choosing unique names, to distinguish overloads because this must match up 
/// between signature and implementation, and the signature just has "unit". 
let TranslateTopValSynInfo m tcAttribute (ValSynInfo(argsData,retData)) = 
    PartialTopValInfo (argsData |> List.mapSquared (TranslateTopArgSynInfo true m (tcAttribute attrTgtParameter)), 
                       retData |> TranslateTopArgSynInfo false m (tcAttribute attrTgtReturnValue))

let TranslatePartialArity tps (PartialTopValInfo (argsData,retData)) = 
    TopValInfo(TopValInfo.InferTyparInfo tps,argsData,retData)


(*-------------------------------------------------------------------------
 * Members
 *------------------------------------------------------------------------- *)

let ComputeLogicalCompiledName (id:ident) memberFlags = 
    match memberFlags.MemberKind with 
    | MemberKindClassConstructor -> ".cctor"
    | MemberKindConstructor -> ".ctor"
    | MemberKindMember -> id.idText 
    | MemberKindPropertyGetSet ->  error(InternalError("MemberKindPropertyGetSet only expected in parse trees",id.idRange))
    | MemberKindPropertyGet ->  "get_"^id.idText
    | MemberKindPropertySet ->  "set_"^id.idText 

/// Make the unique "name" for a member.
//
// Note: the use of a mangled string as the unique name for a member is one of the reasons 
// we end up needing things like OverloadID (which is used to make the name unique). 
//
// optImplSlotTy = None (for classes) or Some ty (when implementing interface type ty) 
let MkMemberDataAndUniqueId(g,tcref,isExtrinsic,attrs,optImplSlotTys,memberFlags,valSynData,id)  =
    let logicalCompiledName = ComputeLogicalCompiledName id memberFlags
    let optIntfSlotTys = if optImplSlotTys |> List.forall (is_interface_typ g) then optImplSlotTys else []
    let qualifiedCompiledName = List.foldBack (tcref_of_stripped_typ g >> qualified_mangled_name_of_tcref) optIntfSlotTys logicalCompiledName
    let memberInfo = 
        { ApparentParent=tcref; 
          MemberFlags=memberFlags; 
          IsImplemented=false;
          // If this is an interface method impl then the name we use for the method changes 
          CompiledName=qualifiedCompiledName;
          // NOTE: This value is initially only set for interface implementations and those overrides 
          // where we manage to pre-infer which abstract is overriden by the method. It is filled in  
          // properly when we check the allImplemented implementation checks at the end of the inference scope. 
          ImplementedSlotSigs=optImplSlotTys |> List.map (fun ity -> TSlotSig(logicalCompiledName,ity,[],[],[],None)) }
    let isInstance = MemberIsCompiledAsInstance g tcref isExtrinsic memberInfo attrs
    if (memberFlags.MemberIsVirtual  || memberFlags.MemberIsDispatchSlot || nonNil optIntfSlotTys) then 
        if not isInstance then
          errorR(VirtualAugmentationOnNullValuedType(id.idRange));
    elif memberFlags.MemberIsInstance then 
        if not isExtrinsic && not isInstance then
            warning(NonVirtualAugmentationOnNullValuedType(id.idRange))
    let id = 
       let tname = tcref.MangledName
       let text = 
           match memberFlags.MemberKind with 
           | MemberKindClassConstructor -> tname^".cctor"
           | MemberKindConstructor -> tname^".ctor"
           | MemberKindMember -> tname^"."^id.idText 
           | MemberKindPropertyGetSet ->  failwith "MkMemberDataAndUniqueId"
           | MemberKindPropertyGet ->  tname^".get_"^id.idText
           | MemberKindPropertySet ->  tname^".set_"^id.idText
       let text = 
           match memberFlags.OverloadQualifier with
           | None   -> text^"."^string (List.sum (SynInfo.AritiesOfArgs valSynData))
           | Some t -> text^".Overloaded."^t
       let text = if memberFlags.MemberKind <> MemberKindConstructor && memberFlags.MemberKind <> MemberKindClassConstructor && not memberFlags.MemberIsInstance then text^".Static" else text
       let text = if memberFlags.MemberIsOverrideOrExplicitImpl then text^".Override" else text
       let text = List.foldBack (tcref_of_stripped_typ g >> qualified_mangled_name_of_tcref) optIntfSlotTys text
       ident(text,id.idRange)
    memberInfo,id


type OverridesOK = 
    | OverridesOK 
    | WarnOnOverrides
    | ErrorOnOverrides

/// A type to represent information associated with values to indicate what explicit (declared) type parameters
/// are given and what additional type parameters can be inferred, if any.
///
/// The declared type parameters, e.g. let f<'a> (x:'a) = x, plus an indication 
/// of whether additional polymorphism may be inferred, e.g. let f<'a,..> (x:'a) y = x 
type ExplicitTyparInfo = ExplicitTyparInfo of Tast.typars * bool
let permitInferTypars = ExplicitTyparInfo ([],true) 
let dontInferTypars = ExplicitTyparInfo ([],false) 

type ArgAndRetAttribs = ArgAndRetAttribs of Tast.Attribs list list * Tast.Attribs
let noArgOrRetAttribs = ArgAndRetAttribs ([],[])

/// A flag to represent the sort of bindings are we processing.
/// Processing "declaration" and "class" bindings that make up a module (such as "let x = 1 let y = 2") 
/// shares the same code paths (e.g. TcLetBinding and TcLetrec) as processing expression bindings (such as "let x = 1 in ...") 
/// Member bindings also use this path. 
//
/// However there are differences in how different bindings get processed,
/// i.e. module bindings get published to the implicitly accumulated module type, but expression 'let' bindings don't. 
type DeclKind = 
    | ModuleOrMemberBinding 
    /// Extensions to a type within the same assembly
    | IntrinsicExtensionBinding 
    /// Extensions to a type in a different assembly
    | ExtrinsicExtensionBinding 
    | ClassLetBinding 
    | ObjectExpressionOverrideBinding
    | ExpressionBinding 

    static member MustHaveArity x = 
        match x with 
        | ModuleOrMemberBinding -> true
        | IntrinsicExtensionBinding  -> true
        | ExtrinsicExtensionBinding -> true
        | ClassLetBinding -> false
        | ObjectExpressionOverrideBinding -> false
        | ExpressionBinding -> false

    static member IsAccessModifierPermitted  x = 
        match x with 
        | ModuleOrMemberBinding -> true
        | IntrinsicExtensionBinding  -> true
        | ExtrinsicExtensionBinding -> true
        | ClassLetBinding 
        | ObjectExpressionOverrideBinding 
        | ExpressionBinding -> false

    static member ImplicitlyStatic  x = 
        match x with 
        | ModuleOrMemberBinding -> true
        | IntrinsicExtensionBinding  -> true
        | ExtrinsicExtensionBinding -> true
        | ClassLetBinding -> false
        | ObjectExpressionOverrideBinding -> false
        | ExpressionBinding -> false

    static member MustAlwaysGeneralize  x = 
        match x with 
        | ModuleOrMemberBinding -> true
        | IntrinsicExtensionBinding  -> true
        | ExtrinsicExtensionBinding -> true
        | ClassLetBinding -> true
        | ObjectExpressionOverrideBinding -> false
        | ExpressionBinding -> false

    static member CanHaveAttributes  x = 
        match x with 
        | ModuleOrMemberBinding -> true
        | IntrinsicExtensionBinding  -> true
        | ExtrinsicExtensionBinding -> true
        | ClassLetBinding -> false
        | ObjectExpressionOverrideBinding -> true
        | ExpressionBinding -> false

    // Note: now always true
    static member CanGeneralizeConstrainedTypars  x = 
        match x with 
        | ModuleOrMemberBinding -> true
        | IntrinsicExtensionBinding  -> true
        | ExtrinsicExtensionBinding -> true
        | ClassLetBinding -> true
        | ObjectExpressionOverrideBinding -> true
        | ExpressionBinding -> true
        
    static member ConvertToLinearBindings  x = 
        match x with 
        | ModuleOrMemberBinding -> true
        | IntrinsicExtensionBinding  -> true
        | ExtrinsicExtensionBinding -> true
        | ClassLetBinding -> true
        | ObjectExpressionOverrideBinding -> true
        | ExpressionBinding -> false 

    static member CanOverrideOrImplement x = 
        match x with 
        | ModuleOrMemberBinding -> OverridesOK
        | IntrinsicExtensionBinding -> WarnOnOverrides
        | ExtrinsicExtensionBinding -> ErrorOnOverrides
        | ClassLetBinding -> ErrorOnOverrides 
        | ObjectExpressionOverrideBinding -> OverridesOK
        | ExpressionBinding -> ErrorOnOverrides 

//-------------------------------------------------------------------------
// Data structures that track the gradual accumualtion of information
// about values and members during inference.
//------------------------------------------------------------------------- 

/// The results of preliminary pass over patterns to extract variables being declared.
type PrelimValScheme1 = 
    PrelimValScheme1 of 
        ident * 
        ExplicitTyparInfo * 
        Tast.typ * 
        PartialTopValInfo option *
        ValMemberInfo option * 
        bool * 
        ValInlineInfo * 
        ValBaseOrThisInfo * 
        ArgAndRetAttribs * 
        access option * 
        bool

/// The results of applying let-style generalization after type checking. 
type PrelimValScheme2 = 
    PrelimValScheme2 of 
        ident * 
        TypeScheme * 
        PartialTopValInfo option *
        ValMemberInfo option  * 
        bool * 
        ValInlineInfo * 
        ValBaseOrThisInfo * 
        ArgAndRetAttribs * 
        access option * 
        bool *
        bool (* hasDeclaredTypars *) 
        

/// The results of applying arity inference to PrelimValScheme2 
type ValScheme = 
    ValScheme of 
        ident * 
        TypeScheme * 
        ValTopReprInfo option * 
        ValMemberInfo option  * 
        bool * 
        ValInlineInfo * 
        ValBaseOrThisInfo * 
        access option * 
        bool * (* compgen *) 
        bool * (* isIncrClass *) 
        bool (* isTyFunc *) 

        
//-------------------------------------------------------------------------
// Data structures that track the whole process of taking a syntactic binding and
// checking it.
//------------------------------------------------------------------------- 

/// Translation of patterns is List.unzip into three phases. The first collects names. 
/// The second is run after val_specs have been created for those names and inference 
/// has been resolved. The second phase is run by applying a function returned by the 
/// first phase. The input to the second phase is a List.map that gives the Val and type scheme 
/// for each value bound by the pattern. 
type TcPatPhase2Input = 
    TcPatPhase2Input of (Val * TypeScheme) NameMap


/// The first phase of translation of binding leaves a whole goop of information. 
/// This is a bit of a mess: much of this information is carried on a per-value basis by the 
/// "PrelimValScheme1 NameMap". 
type TBindingInfo = 
    TBindingInfo of 
       ValInlineInfo * 
       bool *   (* immutable? *)
       Tast.Attribs * 
       XmlDoc * 
       (TcPatPhase2Input -> Patcompile.pat) * 
       ExplicitTyparInfo * 
       PrelimValScheme1 NameMap * 
       expr * 
       ArgAndRetAttribs * 
       Tast.typ * 
       range *
       SequencePointInfoForBinding * 
       bool * (* compiler generated? *)
       Constant option (* literal value? *)

//-------------------------------------------------------------------------
// Helpers related to type schemes
//------------------------------------------------------------------------- 

let GeneralizedTypeForTypeScheme typeScheme = 
    let (TypeScheme(generalizedTypars,_,tau)) = typeScheme
    try_mk_forall_ty generalizedTypars tau

let NonGenericTypeScheme ty = TypeScheme([],[],ty)

//-------------------------------------------------------------------------
// Helpers related to publishing values, types and members into the
// elaborated representation.
//------------------------------------------------------------------------- 

let curr_stable_fslib_nlpath cenv env = NLPath(cenv.topCcu, arr_path_of_lid env.ePath)

let UpdateAccModuleOrNamespaceType cenv env f = 
    // Change fslib CCU to ensure forward stable references used by 
    // the compiler can be resolved ASAP. Not at all pretty but it's hard to 
    // find good ways to do references from the compiler into a term graph 
    if cenv.compilingCanonicalFslibModuleType then 
        let modul = (curr_stable_fslib_nlpath cenv env).Deref
        if verbose then dprintf "updating contents of CCU-held fslib module %s in case forward references occur form the compiler to this construct\n" modul.MangledName;
        modul.Data.entity_modul_contents <- notlazy (f true modul.ModuleOrNamespaceType);
    SetCurrAccumulatedModuleOrNamespaceType env (f false (GetCurrAccumulatedModuleOrNamespaceType env))
  
let PublishModuleDefn cenv env mspec = 
    UpdateAccModuleOrNamespaceType cenv env (fun intoFslibCcu mty -> 
       if intoFslibCcu then mty
       else mty.AddEntity(mspec))
    CallNameResolutionSink(mspec.Range,nenv_of_tenv env,Item_modrefs([mk_local_modref mspec]),ItemOccurence.Binding,env.DisplayEnv,AccessRightsOfEnv env)

let PublishTypeDefn cenv env tycon = 
    UpdateAccModuleOrNamespaceType cenv env (fun _ mty -> 
       mty.AddEntity(tycon))

let PublishValueDefnPrim cenv env (vspec:Val) = 
    UpdateAccModuleOrNamespaceType cenv env (fun _ mty -> 
        if verbose then dprintf "adding value %s#%d\n" vspec.MangledName vspec.Stamp;
        mty.AddVal(vspec))

let CheckForValueNameClashes cenv env (v:Val) =
    if v.IsCompilerGenerated then () else
    match v.ActualParent with 
    | ParentNone ->  ()
    | Parent tcref -> 
        let hasDefaultAugmentation = 
            match TryFindAttrib cenv.g cenv.g.attrib_DefaultAugmentationAttribute tcref.Attribs with
            | Some(Attrib(_,_,[ AttribBoolArg(b) ],_,_)) -> b
            | _ -> true (* not hiddenRepr *)

        let kind = (if v.IsMember then "member" else "value")
        if (GetCurrAccumulatedModuleOrNamespaceType env).AllValuesAndMembers.ContainsKey(v.MangledName) then
            error(Duplicate(kind,v.DisplayName,v.Range));
        let check skipValCheck nm = 
            if not skipValCheck && v.IsModuleBinding && (GetCurrAccumulatedModuleOrNamespaceType env).AllValuesAndMembers.ContainsKey(nm) then
                error(Duplicate(kind,v.DisplayName,v.Range));
            if (hasDefaultAugmentation) then 
                match tcref.GetUnionCaseByName(nm) with 
                | Some(uc) -> error(NameClash(nm,kind,v.DisplayName,v.Range,"union case",uc.DisplayName,uc.Range));
                | None -> ()
            // Default augmentation contains the nasty 'Case<UnionCase>' etc.
            if nm.StartsWith "Case" then
                match tcref.GetUnionCaseByName(nm.[4..]) with 
                | Some(uc) -> error(NameClash(nm,kind,v.DisplayName,v.Range,"compiled form of the union case",uc.DisplayName,uc.Range));
                | None -> ()
            // Default augmentation contains the nasty 'Is<UnionCase>' etc.
            if nm.StartsWith "Is" && hasDefaultAugmentation then
                match tcref.GetUnionCaseByName(nm.[2..]) with 
                | Some(uc) -> error(NameClash(nm,kind,v.DisplayName,v.Range,"default augmentation of the union case",uc.DisplayName,uc.Range));
                | None -> ()
            // Default augmentation contains the nasty 'Get<UnionCase>1' etc.
            if nm.StartsWith "Get" && hasDefaultAugmentation  then
                // Check it has a number on the end
                let baseName = nm.[3..]
                let trimmedName = baseName.TrimEnd [| '0' .. '9' |]
                if trimmedName <> baseName then 
                    match tcref.GetUnionCaseByName(trimmedName) with 
                    | Some(uc) -> error(NameClash(nm,kind,v.DisplayName,v.Range,"default augmentation of the union case", uc.DisplayName,uc.Range));
                    | None -> ()

            match tcref.GetFieldByName(nm) with 
            | Some(rf) -> error(NameClash(nm,kind,v.DisplayName,v.Range,"field",rf.Name,rf.Range));
            | None -> ()

        check false v.MangledName
        check false v.DisplayName
        check false v.CompiledName
        // Properties get 'get_X'
        match v.TopValInfo with 
        | Some arity when arity.NumCurriedArgs = 0 && arity.NumTypars = 0 -> check false ("get_"^v.DisplayName)
        | _ -> ()
        match v.TopValInfo with 
        | Some arity when v.IsMutable && arity.NumCurriedArgs = 0 && arity.NumTypars = 0 -> check false ("set_"^v.DisplayName)
        | _ -> ()
        match TryChopPropertyName v.DisplayName with 
        | Some res -> check true res 
        | None -> ()
           
        
let PublishValueDefn cenv env declKind (vspec:Val) =
    if (declKind = ModuleOrMemberBinding) && 
       ((GetCurrAccumulatedModuleOrNamespaceType env).ModuleOrNamespaceKind = Namespace) && 
       (isNone vspec.MemberInfo) then 
           errorR(Error("Namespaces may not contain values. Consider using a module to hold your value declarations",vspec.Range));

    if (declKind = ExtrinsicExtensionBinding) && 
       ((GetCurrAccumulatedModuleOrNamespaceType env).ModuleOrNamespaceKind = Namespace) then 
           errorR(Error("Namespaces may not contain extension members except in the same file and namespace where the type is defined. Consider using a module to hold declarations of extension members",vspec.Range));

    try CheckForValueNameClashes cenv env vspec with e -> errorRecovery e vspec.Range; 

    // Publish the value to the module type being generated. 
    if (match declKind with 
        | ModuleOrMemberBinding -> true
        | ExtrinsicExtensionBinding -> true
        | IntrinsicExtensionBinding -> true
        | _ -> false) then 
        PublishValueDefnPrim cenv env vspec

    match vspec.MemberInfo with 
    | Some memberInfo when 
        (not vspec.IsCompilerGenerated && 
         // Extrinsic extensions don't get added to the tcaug
         not (declKind = ExtrinsicExtensionBinding) && 
         // Static initializers don't get published to the tcaug 
         not (memberInfo.MemberFlags.MemberKind = MemberKindClassConstructor)) -> 
        
        // tcaug_adhoc cleanup: we should not need this table nor this mutation 
        // The values should be carried in the environment and in the generated module type. 
        // Opening module types should add the values to the environment. 
        let tcaug = vspec.MemberApparentParent.TypeContents
        tcaug.tcaug_adhoc <- NameMultiMap.add memberInfo.CompiledName (mk_local_vref vspec) tcaug.tcaug_adhoc
    |  _ -> ()

let CombineVisibilityAttribs vis1 vis2 m = 
   if isSome vis1 && isSome vis2 then 
        errorR(Error("Multiple visibility attributes have been specified for this identifier",m));
   if isSome vis1 then vis1 else vis2

let ComputeAccessAndCompPath env declKindOpt m vis actualParent = 
    let accessPath = curr_access_cpath env
    let accessModPermitted = 
        match declKindOpt with 
        | None -> true
        | Some declKind -> DeclKind.IsAccessModifierPermitted declKind

    if isSome vis && not accessModPermitted then 
        errorR(Error("Multiple visibility attributes have been specified for this identifier. 'let' bindings in classes are always private, as are any 'let' bindings inside expressions",m)); 
    let vis = 
        match vis with 
        | None -> taccessPublic (* a module or member binding defaults to "public" *)
        | Some a when a = accessPublic -> taccessPublic
        | Some a when a = accessPrivate -> TAccess [accessPath]
        | Some a when a = accessInternal -> 
            let (CompPath(scoref,_)) = accessPath
            TAccess [CompPath(scoref,[])]
        | _ -> 
            errorR(InternalError("Unrecognized accessibility specification",m));
            taccessPublic

    let vis = 
        match actualParent with 
        | ParentNone -> vis 
        | Parent tcref -> 
             combineAccess vis tcref.Accessibility
    let cpath = curr_cpath env
    let cpath = (if accessModPermitted then Some cpath else None)
    vis,cpath 

let CheckForAbnormalOperatorNames cenv idRange opName isMember =
    

    if (end_col_of_range idRange - start_col_of_range idRange <= 5) && 
       not cenv.g.compilingFslib  then 
        
        match opName, isMember with 
        | PrettyNaming.Relational   ,true  -> warning(StandardOperatorRedefinitionWarning(sprintf "The name '(%s)' should not be used as a member name. To define comparison semantics for a type, implement the 'System.IComparable' interface. If defining a static member for use from other .NET languages then use the name '%s' instead" opName (CompileOpName opName),idRange))
        | PrettyNaming.Equality ,true  -> warning(StandardOperatorRedefinitionWarning(sprintf "The name '(%s)' should not be used as a member name. To define equality semantics for a type, override the 'Object.Equals' member. If defining a static member for use from other .NET languages then use the name '%s' instead" opName (CompileOpName opName),idRange))
        | PrettyNaming.Control,true  -> warning(StandardOperatorRedefinitionWarning(sprintf "The name '(%s)' should not be used as a member name. If defining a static member for use from other .NET languages then use the name '%s' instead" opName (CompileOpName opName),idRange))
        | PrettyNaming.FixedTypes,true  -> warning(StandardOperatorRedefinitionWarning(sprintf "The name '(%s)' should not be used as a member name because it is given a standard definition in the F# library over fixed types" opName,idRange))
        | PrettyNaming.Indexer,true  -> ()
        | PrettyNaming.Relational  ,false -> warning(StandardOperatorRedefinitionWarning(sprintf "The '%s' operator should not normally be redefined. To define overloaded comparison semantics for a particular type, implement the 'System.IComparable' interface in the definition of that type" opName,idRange))
        | PrettyNaming.Equality ,false -> warning(StandardOperatorRedefinitionWarning(sprintf "The '%s' operator should not normally be redefined. To define equality semantics for a type, override the 'Object.Equals' member in the definition of that type" opName,idRange))
        | PrettyNaming.Control,false -> warning(StandardOperatorRedefinitionWarning(sprintf "The '%s' operator should not normally be redefined. Consider using a different operator name" opName,idRange))
        | PrettyNaming.Indexer,false -> error(StandardOperatorRedefinitionWarning(sprintf "The '%s' operator may not be redefined. Consider using a different operator name" opName,idRange))
        | PrettyNaming.FixedTypes,_ -> ()
        | PrettyNaming.Other,_ -> ()

let MakeAndPublishVal cenv env (altActualParent,inSig,declKind,vrec,(ValScheme(id,typeScheme,topValData,memberInfoOpt,mut,inlineFlag,baseOrThis,vis,compgen,isIncrClass,isTyFunc)),attrs,doc,konst) =
    (* dprintf "MakeAndPublishVal, id = %s\n" id.idText; *)
    let ty = GeneralizedTypeForTypeScheme typeScheme
    let m = id.idRange
    let isIntfImpl = 
        match memberInfoOpt with 
        | None -> false 
        | Some memberInfo -> MemberIsExplicitImpl cenv.g memberInfo

    let isTopBinding = 
        match declKind with 
        | ModuleOrMemberBinding -> true
        | ExtrinsicExtensionBinding -> true
        | IntrinsicExtensionBinding -> true
        | _ -> false

    let isExtrinsic = (declKind=ExtrinsicExtensionBinding)
    let actualParent = 
        // Use the parent of the member if it's available 
        // If it's an extrinsic extension member of not a member then use the containing module. 
        match memberInfoOpt with 
        | Some memberInfo when not isExtrinsic -> 
            if memberInfo.ApparentParent.IsModuleOrNamespace then 
                errorR(InternalError("expected module or namespace parent "^id.idText,m));

            Parent(memberInfo.ApparentParent)
        | _ -> altActualParent
             
    let vis,cpath = ComputeAccessAndCompPath env (Some declKind) id.idRange vis actualParent

    let inlineFlag = 
        if HasAttrib cenv.g cenv.g.attrib_DllImportAttribute attrs then begin 
            if inlineFlag = PseudoValue || inlineFlag = AlwaysInline then 
              errorR(Error("DLLImport stubs may not be inlined",m)); 
            NeverInline 
        end else inlineFlag

    let vspec = 
        NewVal (id,ty,
                      (if ((* (is_byref_typ cenv.g ty) || *) mut) then Mutable else Immutable),
                      compgen,topValData,cpath,vis,vrec,memberInfoOpt,baseOrThis,attrs,inlineFlag,doc, isTopBinding, isExtrinsic,isIncrClass,isTyFunc,konst,actualParent)

    CheckForAbnormalOperatorNames cenv id.idRange (DecompileOpName vspec.CoreDisplayName) (isSome memberInfoOpt)

    PublishValueDefn cenv env declKind vspec;

  (*  dprintf "register val, inSig = %b, v = %s, scopem = %a\n" inSig vspec.MangledName output_range vspec.Range ; *)

    // Notify the Language Service or another development environment that 
    // the value in scope at the binding point (i.e. the range of the 
    // identifier text for the value). 'unqualified' is true if the name at the 
    // binding location in the original source appears unqualified, as 
    // in the case of both static and instance members in signatures 
    // and all static members in classes. 
    // REVIEW: this logic is to some extent part of the name resolution rules, i.e. how 
    // different kinds of 'values' (including members) appear in the name environment at 
    // different points. 
    begin 
        let unqualified = 
             match vspec.MemberInfo with 
             | None -> true
             | Some memberInfo -> inSig || not memberInfo.MemberFlags.MemberIsInstance

        match !GlobalTypecheckResultsSink with 
        | None -> ()
        | Some _ -> 
            if unqualified && not vspec.IsCompilerGenerated && not (String.hasPrefix vspec.MangledName "_") then 
                let nenv = AddFakeNamedValRefToNameEnv vspec.DisplayName (mk_local_vref vspec) (nenv_of_tenv env)
                CallEnvSink(vspec.Range,nenv,AccessRightsOfEnv env)
                CallNameResolutionSink(vspec.Range,nenv,Item_val(mk_local_vref vspec),ItemOccurence.Binding,nenv.eDisplayEnv,AccessRightsOfEnv env);
    end;

    vspec

let MakeAndPublishVals cenv env (altActualParent,inSig,declKind,vrec,valSchemes,attrs,doc,konst) =
    Map.fold_right
        (fun name (ValScheme(_,typeScheme,_,_,_,_,_,_,_,_,_) as valscheme) values -> 
          Map.add name (MakeAndPublishVal cenv env (altActualParent,inSig,declKind,vrec,valscheme,attrs,doc,konst), typeScheme) values)
        valSchemes
        Map.empty

let MakeAndPublishBaseVal cenv env baseIdOpt ty = 
    baseIdOpt |> Option.map (fun (id:ident) ->
       let valscheme = ValScheme(id,NonGenericTypeScheme(ty),None,None,false,NeverInline,BaseVal,None,false,false,false)
       MakeAndPublishVal cenv env (ParentNone,false,ExpressionBinding,ValNotInRecScope,valscheme,[],emptyXmlDoc,None))

let MakeAndPublishCtorThisRefCellVal cenv env (thisIdOpt: ident option) thisTy = 
    match thisIdOpt with 
    | Some thisId -> 
        if not (is_fsobjmodel_typ cenv.g thisTy) then 
            errorR(Error("Structs may only bind a 'this' parameter at member declarations",thisId.idRange));

        let thisValScheme = ValScheme(thisId,NonGenericTypeScheme(mk_refcell_ty cenv.g thisTy),None,None,false,NeverInline,CtorThisVal,None,false,false,false)
        Some(MakeAndPublishVal cenv env (ParentNone,false,ExpressionBinding,ValNotInRecScope,thisValScheme,[],emptyXmlDoc,None))
    | None -> None 


//-------------------------------------------------------------------------
// Helpers for type inference for recursive bindings
//------------------------------------------------------------------------- 

/// Fixup the type instantiation at recursive references. Used after the bindings have been
/// checked. The fixups are applied by using mutation.
let AdjustAndForgetUsesOfRecValue cenv vrefTgt (ValScheme(id,typeScheme,toparity,_,_,_,_,_,_,_,_)) =
    let (TypeScheme(generalizedTypars,_,ty)) = typeScheme
    let fty = GeneralizedTypeForTypeScheme typeScheme
    let lvrefTgt = deref_val vrefTgt
    if nonNil(generalizedTypars) then begin
        // Find all the uses of this recursive binding and use mutation to adjust the expressions 
        // at those points in order to record the inferred type parameters. 
        let recUses = vspec_mmap_find lvrefTgt cenv.recUses
        recUses |> List.iter  (fun (fixupPoint,m,isComplete) -> 
          if not isComplete then 
              // Keep any values for explicit type arguments 
              let fixedUpExpr = 
                  let vrefFlags,tyargs0 = 
                      match !fixupPoint with 
                      | TExpr_app(TExpr_val (_,vrefFlags,_),_,tyargs0,[],m) -> vrefFlags,tyargs0
                      | TExpr_val(_,vrefFlags,_) -> vrefFlags,[] 
                      | _ -> errorR(Error("Unexpected expression at recursive inference point",m)); NormalValUse,[]
                  
                  let ityargs = generalize_typars (List.drop (List.length tyargs0) generalizedTypars)
                  prim_mk_app (TExpr_val (vrefTgt,vrefFlags,m),fty) (tyargs0 @ ityargs) [] m
              fixupPoint :=   fixedUpExpr)
    end;
    cenv.recUses <- vspec_map_remove lvrefTgt cenv.recUses
     

/// Set the properties of recursive values that are only fully known after inference is complete 
let AdjustRecType cenv (vspec:Val) (ValScheme(id,typeScheme,topValData,_,_,_,_,_,_,_,_)) =
    let fty = GeneralizedTypeForTypeScheme typeScheme
    vspec.Data.val_type <- fty;
    vspec.Data.val_top_repr_info <- topValData;
    set_vrec_of_vflags vspec.Data ValNotInRecScope
       
/// Record the generated value expression as a place where we will have to 
/// adjust using AdjustAndForgetUsesOfRecValue at a letrec point. Every use of a value 
/// under a letrec gets used at the _same_ type instantiation. 
let RecordUseOfRecValue cenv vrec vrefTgt vexp m = 
    match vrec with 
    | ValInRecScope isComplete -> 
        let fixupPoint = ref vexp
        cenv.recUses <- vspec_mmap_add (deref_val vrefTgt) (fixupPoint,m,isComplete) cenv.recUses;
        TExpr_link (fixupPoint)
    | ValNotInRecScope -> 
        vexp

type RecursiveUseFixupPoints = RecursiveUseFixupPoints of (expr ref * range) list

/// Get all recursive references, for fixing up delayed recursion using laziness 
let GetAllUsesOfRecValue cenv vrefTgt = 
    RecursiveUseFixupPoints (vspec_mmap_find vrefTgt cenv.recUses |> List.map (fun (fixupPoint,m,isComplete) -> (fixupPoint,m)))


//-------------------------------------------------------------------------
// Helpers for Generalization
//------------------------------------------------------------------------- 

let ChooseCanonicalDeclaredTyparsAfterInference g denv declaredTypars m =

    declaredTypars |> List.iter (fun tp -> 
      let ty = mk_typar_ty tp
      if not (is_anypar_typ g ty) then 
          error(Error("This code is less generic than required by its annotations because the explicit type variable '"^tp.Name ^"' could not be generalized. It was constrained to be '"^NicePrint.pretty_string_of_typ denv ty^"'",tp.Range)));
    
    let declaredTypars = NormalizeDeclaredTyparsForEquiRecursiveInference g declaredTypars
    if (ListSet.setify typar_ref_eq declaredTypars).Length <> declaredTypars.Length then 
        errorR(Error("One or more of the explicit class or function type variables for this binding could not be generalized, because they were constrained to other types",m));
    declaredTypars

let ChooseCanonicalValSchemeAfterInference g denv valscheme m =
    let (ValScheme(id,typeScheme,arityInfo,memberInfoOpt,mut,inlineFlag,baseOrThis,vis,compgen,isIncrClass,isTyFunc)) = valscheme
    let (TypeScheme(generalizedTypars,freeChoiceTypars,ty)) = typeScheme
    let generalizedTypars = ChooseCanonicalDeclaredTyparsAfterInference g denv generalizedTypars m
    let typeScheme = TypeScheme(generalizedTypars,freeChoiceTypars,ty)
    let valscheme = ValScheme(id,typeScheme,arityInfo,memberInfoOpt,mut,inlineFlag,baseOrThis,vis,compgen,isIncrClass,isTyFunc)
    valscheme

let PlaceTyparsInDeclarationOrder declaredTypars generalizedTypars  =
    declaredTypars @ (generalizedTypars |> List.filter (fun tp -> not (ListSet.mem typar_ref_eq tp declaredTypars)))

let SetTyparRigid g denv m (tp:Typar) = 
    begin match tp.Solution with 
    | None -> ()
    | Some ty -> 
        if tp.IsCompilerGenerated then 
            errorR(Error("A generic type parameter has been used in a way that constrains it to always be '"^NicePrint.pretty_string_of_typ denv ty^"'",m))
        else 
            errorR(Error("This type parameter has been used in a way that constrains it to always be '"^NicePrint.pretty_string_of_typ denv ty^"'",tp.Range))
    end;
    set_rigid_of_tpdata tp.Data TyparRigid

let GeneralizeVal cenv denv enclosingDeclaredTypars generalizedTyparsForRecursiveBlock generalizedTyparsForThisBinding 
        (PrelimValScheme1(id,iflex,ty,partialTopValInfo,memberInfoOpt,mut,inlineFlag,baseOrThis,argAttribs,vis,compgen)) = 
    let (ExplicitTyparInfo(declaredTypars,_)) = iflex
    let m = id.idRange
    let allDeclaredTypars = enclosingDeclaredTypars@declaredTypars
    let allDeclaredTypars = ChooseCanonicalDeclaredTyparsAfterInference  cenv.g denv allDeclaredTypars m
    (* Trim out anything not in type of the value (as opposed to the type of the r.h.s) *)
    (* This is important when a single declaration binds *)
    (* multiple generic items, where each item does not use all the polymorphism *)
    (* of the r.h.s. , e.g. let x,y = None,[] *)
    let computeRelevantTypars thruFlag = 
        let ftps = (free_in_type_lr cenv.g thruFlag ty)
        let generalizedTypars = generalizedTyparsForThisBinding |> List.filter (fun tp -> ListSet.mem typar_ref_eq tp ftps)
        (* Put declared typars first *)
        let generalizedTypars = PlaceTyparsInDeclarationOrder allDeclaredTypars generalizedTypars  
        generalizedTypars

    let generalizedTypars = computeRelevantTypars false

    // Check stability of existence and ordering of type parameters under erasure of type abbreviations
    let generalizedTyparsLookingThroughTypeAbbreviations = computeRelevantTypars true
    if not (generalizedTypars.Length = generalizedTyparsLookingThroughTypeAbbreviations.Length && 
            List.for_all2 typar_ref_eq generalizedTypars generalizedTyparsLookingThroughTypeAbbreviations) then
        warning(Error("the type parameters inferred for this value are not stable under the erasure of type abbreviations. This is due to the use of type abbreviations which drop or reorder type parameters, e.g. \n\ttype taggedInt<'a> = int   or\n\ttype swap<'a,'b> = 'b * 'a.\nConsider declaring the type parameters for this value explicitly, e.g.\n\tlet f<'a,'b> ((x,y) : swap<'b,'a>) : swap<'a,'b> = (y,x)",m));

    // Some recursive bindings result in free type variables, e.g. 
    //    let rec f (x:'a) = ()  
    //    and g() = (\y. f y); () 
    // What is the type of y? Type inference equates it to 'a. 
    // But "g" is not polymorphic in 'a. Hence we get a free choice of "'a" 
    // in the scope of "g". Thus at each individual recursive binding we record all 
    // type variables for which we have a free choice, which is precisely the difference 
    // between the union of all sets of generalized type variables and the set generalized 
    // at each particular binding. 
    //
    // We record an expression node that indicates that a free choice can be made 
    // for these. This expression node effectively binds the type variables. 
    let freeChoiceTypars = ListSet.subtract typar_ref_eq generalizedTyparsForRecursiveBlock generalizedTypars
    (* printf "GeneralizeVal: %s generalizedTypars=%s epts=%s\n" id.idText (TyparsL generalizedTypars |> showL) (TyparsL freeChoiceTypars |> showL); *)

    (* if nonNil freeChoiceTypars then dprintf "#freeChoiceTypars = %d, #generalizedTypars=%d, #generalizedTyparsForRecursiveBlock=%d, #declaredTypars=%d @ %a\n" (List.length freeChoiceTypars) (List.length generalizedTypars) (List.length generalizedTyparsForRecursiveBlock) (List.length declaredTypars) output_range m; *)

    let hasDeclaredTypars = nonNil(declaredTypars)
    // This is just about the only place we form a TypeScheme 
    let tyScheme = TypeScheme(generalizedTypars, freeChoiceTypars, ty)
    PrelimValScheme2(id,tyScheme,partialTopValInfo,memberInfoOpt,mut,inlineFlag,baseOrThis,argAttribs,vis,compgen,hasDeclaredTypars)

let GeneralizeVals cenv denv enclosingDeclaredTypars generalizedTyparsForRecursiveBlock generalizedTypars types = 
    NameMap.map (GeneralizeVal cenv denv enclosingDeclaredTypars generalizedTyparsForRecursiveBlock generalizedTypars) types

let DontGeneralizeVals types = 
    let dontGeneralizeVal (PrelimValScheme1(id,_,ty,partialTopValInfoOpt,memberInfoOpt,mut,inlineFlag,baseOrThis,argAttribs,vis,compgen)) = 
        PrelimValScheme2(id, NonGenericTypeScheme(ty), partialTopValInfoOpt,memberInfoOpt,mut,inlineFlag,baseOrThis,argAttribs,vis,compgen,false)
    NameMap.map dontGeneralizeVal types

let InferGenericArityFromTyScheme (TypeScheme(generalizedTypars,_,_)) partialTopValInfo =
    TranslatePartialArity generalizedTypars partialTopValInfo

let ComputeIsTyFunc(id:ident,hasDeclaredTypars,arityInfo:ValTopReprInfo option) = 
    hasDeclaredTypars && 
    (match arityInfo with 
     | None -> error(Error("Explicit type parameters may only be used on module or member bindings",id.idRange)) 
     | Some info -> info.NumCurriedArgs = 0) 

let UseSyntacticArity declKind typeScheme partialTopValInfo = 
    if DeclKind.MustHaveArity declKind then 
        Some(InferGenericArityFromTyScheme typeScheme partialTopValInfo)
    else 
        None

/// Combine the results of InferValSynData and InferArityOfExpr. 
//
// The F# spec says that we infer arities from declaration forms and types.
//
// For example
//     let f (a,b) c = 1                  // gets arity [2;1] 
//     let f (a:int*int) = 1              // gets arity [2], based on type
//     let f () = 1                       // gets arity [0]
//     let f = (fun (x:int) (y:int) -> 1) // gets arity [1;1]
//     let f = (fun (x:int*int) y -> 1)   // gets arity [2;1]
//
// Some of this arity inference is purely syntax directed and done in InferValSynData in ast.ml
// Some is done by InferArityOfExpr. 
//
// However, there are some corner cases in this specification. In particular, consider
//   let f () () = 1             // [0;1] or [0;0]?  Answer: [0;1]
//   let f (a:unit) = 1          // [0] or [1]?      Answer: [1]
//   let f = (fun () -> 1)       // [0] or [1]?      Answer: [0]
//   let f = (fun (a:unit) -> 1) // [0] or [1]?      Answer: [1]
//
// The particular choice of [1] for
//   let f (a:unit) = 1          
// is intended to give a disambiguating form for members that override methods taking a single argument 
// instantiated to type "unit", e.g.
//    type Base<'a> = 
//        abstract M : 'a -> unit
//
//    { new Base<int> with 
//        member x.M(v:int) = () }
//
//    { new Base<unit> with 
//        member x.M(v:unit) = () }
//
let CombineSyntacticAndInferredArities g declKind rhsExpr prelimScheme = 
    let (PrelimValScheme2(id,typeScheme,partialTopValInfoOpt,memberInfoOpt,mut,_,_,ArgAndRetAttribs(argAttribs,retAttribs),_,_,_)) = prelimScheme
    match partialTopValInfoOpt, DeclKind.MustHaveArity declKind with
    | _        ,false -> None
    | None     ,true  -> Some(PartialTopValInfo([],TopValInfo.unnamedRetVal))
    // Don't use any expression information for members, where syntax dictates the arity completely
    | _ when memberInfoOpt.IsSome -> 
        partialTopValInfoOpt
    | Some(partialTopValInfoFromSyntax),true  -> 
        let (PartialTopValInfo(curriedArgInfosFromSyntax,retInfoFromSyntax)) = partialTopValInfoFromSyntax
        let partialArityInfo = 
            if mut then 
                PartialTopValInfo ([],retInfoFromSyntax)
            else
            
                let (TopValInfo (_,curriedArgInfosFromExpression,_)) = 
                    InferArityOfExpr g (GeneralizedTypeForTypeScheme typeScheme) argAttribs retAttribs rhsExpr

                // Choose between the syntactic arity and the expression-inferred arity
                // If the syntax specifies an eliminated unit arg, then use that
                let choose ai1 ai2 = 
                    match ai1,ai2 with 
                    | [],ai -> []
                    // Dont infer eliminated unit args from the expression if they don't occur syntactically.
                    | ai,[] -> ai
                    // If we infer a tupled argument from the expression and/or type then use that
                    | _ when ai1.Length < ai2.Length -> ai2
                    | _ -> ai1
                let rec loop ais1 ais2 =
                    match ais1,ais2 with 
                    // If the expression infers additional arguments then use those (this shouldn't happen, since the
                    // arity inference done on the syntactic form should give identical results)
                    | [],ais | ais,[] -> ais
                    | (h1::t1),(h2::t2) -> choose h1 h2 :: loop t1 t2
                let curriedArgInfos = loop curriedArgInfosFromSyntax curriedArgInfosFromExpression
                PartialTopValInfo (curriedArgInfos,retInfoFromSyntax)

        Some(partialArityInfo)

let BuildValScheme declKind partialArityInfoOpt prelimScheme = 
    let (PrelimValScheme2(id,typeScheme,_,memberInfoOpt,mut,inlineFlag,baseOrThis,ArgAndRetAttribs(argAttribs,retAtrribs),vis,compgen,hasDeclaredTypars)) = prelimScheme
    let topValInfo = 
        if DeclKind.MustHaveArity declKind then 
            Option.map (InferGenericArityFromTyScheme typeScheme) partialArityInfoOpt
        else
            None
    let isTyFunc = ComputeIsTyFunc(id,hasDeclaredTypars,topValInfo)
    ValScheme(id,typeScheme,topValInfo,memberInfoOpt,mut,inlineFlag,baseOrThis,vis,compgen,false,isTyFunc)

let UseCombinedArity g declKind rhsExpr prelimScheme = 
    let partialArityInfoOpt = CombineSyntacticAndInferredArities g declKind rhsExpr prelimScheme 
    BuildValScheme declKind partialArityInfoOpt prelimScheme
    
let UseNoArity prelimScheme = 
    BuildValScheme ExpressionBinding None prelimScheme

let MakeSimpleVals cenv env m names =
    let tyschemes  = DontGeneralizeVals names
    let valSchemes = NameMap.map UseNoArity tyschemes
    let values     = MakeAndPublishVals cenv env (ParentNone,false,ExpressionBinding,ValNotInRecScope,valSchemes,[],emptyXmlDoc,None)
    let vspecMap   = NameMap.map fst values
    values,vspecMap
    
let MakeAndPublishSimpleVals cenv env m names =
    let values,vspecMap   = MakeSimpleVals cenv env m names
    let envinner   = AddLocalValMap m vspecMap env
    envinner,values,vspecMap



//-------------------------------------------------------------------------
// Helpers to freshen existing types and values, i.e. when a reference
// to C<_> occurs then generate C<?ty> for a fresh type inference variable ?ty.
//------------------------------------------------------------------------- 
   
// REVIEW: we should ideally never need to pass in rigid=false here 
// REVIEW: this is related to how we typecheck recursive bindings in the presence 
// REVIEW: of explicit type parameters, including type parameters arising from classes. 
let FreshenTyconRef m rigid (tcref:TyconRef) declaredTyconTypars = 
    let tpsorig = declaredTyconTypars
    let tps = CopyTypars tpsorig
    if rigid <> TyparRigid then 
      tps |> List.iter (fun tp -> set_rigid_of_tpdata tp.Data rigid);  
        
    let renaming,tinst = FixupNewTypars m [] [] tpsorig tps
    TType_app(tcref,List.map mk_typar_ty tpsorig),tps,renaming,TType_app(tcref,tinst)
    
let FreshenPossibleForallTy g m rigid ty = 
    let tpsorig,tau =  try_dest_forall_typ g ty
    if isNil tpsorig then [],[],tau
    else
        // tps may be have been equated to other tps in equi-recursive type inference and units-of-measure type inference. Normalize them here 
        let tpsorig = NormalizeDeclaredTyparsForEquiRecursiveInference g tpsorig
        let tps,renaming,tinst = copy_and_fixup_typars m rigid tpsorig
        tps,tinst,InstType renaming tau

let info_of_tcref cenv m env (tcref:TyconRef) = 
    let tps,renaming,tinst = new_tinst cenv m (tcref.Typars(m))
    tps,renaming,tinst,TType_app (tcref,tinst)


/// Given a abstract method, which may be a generic method, freshen the type in preparation 
/// to apply it as a constraint to the method that implements the abstract slot 
let FreshenAbstractSlot g amap m synTyparDecls absMethInfo = 

    // Work out if an explicit instantiation has been given. If so then the explicit type 
    // parameters will be made rigid and checked for generalization. If not then auto-generalize 
    // by making the copy of the type parameters on the virtual being overriden rigid. 

    let typarsFromAbsSlotAreRigid = 
        
        match synTyparDecls with 
        | SynValTyparDecls(synTypars,infer,_) -> 
            if nonNil synTypars && infer then errorR(Error("You must explicitly declare either all or no type parameters when overriding a generic abstract method",m));
            isNil synTypars
            
    let argtys,retTy,fmtps,_ = CompiledSigOfMeth g amap m absMethInfo
    (* dprintf "nm = %s, #fmtps = %d\n" absMethInfo.LogicalName (List.length fmtps); *) 
    
    (* If the virual method is a generic method then copy its type parameters *)
    let typarsFromAbsSlot,typarInstFromAbsSlot,_ = 
        let ttps = (FormalTyparsOfEnclosingTypeOfMethInfo m absMethInfo)
        let ttinst = tinst_of_stripped_typ g absMethInfo.EnclosingType
        let rigid = (if typarsFromAbsSlotAreRigid then TyparRigid else TyparWarnIfNotRigid)
        ConstraintSolver.freshen_and_fixup_typars m rigid ttps ttinst fmtps

   (*  dprintf "#typarsFromAbsSlot = %d\n" (List.length typarsFromAbsSlot); *)

    (* Work out the required type of the member *)
    let argTysFromAbsSlot = argtys |> List.mapSquared (InstType typarInstFromAbsSlot) 
    let retTyFromAbsSlot = retTy  |> GetFSharpViewOfReturnType g |> InstType typarInstFromAbsSlot 
    typarsFromAbsSlotAreRigid,typarsFromAbsSlot,argTysFromAbsSlot, retTyFromAbsSlot


//-------------------------------------------------------------------------
// Helpers to typecheck expressions and patterns
//------------------------------------------------------------------------- 

let BuildFieldMap cenv env isPartial ty flds m = 
    let ad = AccessRightsOfEnv env
    if isNil flds then invalidArg "flds" "BuildFieldMap";
    
    let frefSets = 
        flds |> List.map (fun (fld,fldExpr) -> 
            let frefSet = ResolveField cenv.nameResolver env.eNameResEnv ad ty fld
            fld,frefSet, fldExpr)
    let relevantTypeSets = 
        frefSets |> List.map (fun (_,frefSet,_) -> frefSet |> List.choose (fun (tcref,isRecdField) -> if isRecdField then Some(tcref.TyconRef) else None))
    
    let tcref = 
        match List.fold (ListSet.intersect (tcref_eq cenv.g)) (List.hd relevantTypeSets) (List.tl relevantTypeSets) with
        | [tcref] -> tcref
        | _ -> 
            if isPartial then 
                warning (Error("The field labels and expected type of this record expression or pattern do not uniquely determine a corresponding record type",m));
            // OK, there isn't a unique type dictated by the intersection for the field refs. 
            // We're going to get an error of some kind below. 
            // Just choose one field ref and let the error come later 
            let (_,frefSet1,_) = List.hd frefSets
            let fref1 = fst (List.hd frefSet1)
            fref1.TyconRef
    
    let fldsmap,rfldsList = 
        ((Map.empty,[]), frefSets) ||> List.fold (fun (fs,rfldsList) (fld,frefs,fldExpr) -> 
                match frefs |> List.filter (fun (fref2,isRecdField) -> tcref_eq cenv.g tcref fref2.TyconRef) with
                | [(fref2,isRecdField)] -> 

                    // Record the precise resolution of the field for intellisense
                    CallNameResolutionSink(snd(fld).idRange,nenv_of_tenv env,FreshenRecdFieldRef cenv.nameResolver m fref2,ItemOccurence.Use,env.DisplayEnv,ad)

                    if not isRecdField then errorR(DeprecatedClassFieldInference(m));
                    CheckRecdFieldAccessible m (AccessRightsOfEnv env) fref2 |> ignore;
                    CheckFSharpAttributes cenv.g fref2.PropertyAttribs m |> CommitOperationResult;        
                    if  Map.mem fref2.FieldName fs then 
                        errorR (Error("The field "^fref2.FieldName^" appears twice in this record expression or pattern",m));
                    if  not (tcref_eq cenv.g tcref fref2.TyconRef) then 
                        let (_,frefSet1,_) = List.hd frefSets
                        let fref1 = fst (List.hd frefSet1)
                        errorR (FieldsFromDifferentTypes(env.DisplayEnv,fref1,fref2,m));
                        (fs,rfldsList)
                    else (Map.add fref2.FieldName fldExpr fs,
                          (fref2.FieldName,fldExpr)::rfldsList)
                | _ -> error(Error("This record contains fields from inconsistent types",m)))
    tcref,fldsmap,List.rev rfldsList

let rec gen_constr_unify (mk_ucasef,mk_exnconstrf) m cenv env ty item =
    let ad = AccessRightsOfEnv env
    match item with 
    | Item_ecref ecref -> 
        CheckEntityAttributes cenv.g ecref m  |> CommitOperationResult;
        unifyE cenv env m ty cenv.g.exn_ty;
        CheckTyconAccessible m ad ecref |> ignore;
        let mkf = mk_exnconstrf(ecref)
        mkf,typs_of_ecref_rfields ecref
    | Item_ucase ucinfo ->   
        let ucref = ucinfo.UnionCaseRef 
        CheckUnionCaseAttributes cenv.g ucref m  |> CommitOperationResult;
        CheckUnionCaseAccessible m ad ucref |> ignore;
        //let _,inst,tinst,_ = info_of_tcref cenv m env ucref.TyconRef
        let gtyp2 = rty_of_uctyp ucref ucinfo.TypeInst
        let inst = mk_typar_inst ucref.TyconRef.TyparsNoRange ucinfo.TypeInst
        unifyE cenv env m ty gtyp2;
        let mkf = mk_ucasef(ucref,ucinfo.TypeInst)
        mkf,typs_of_ucref_rfields inst ucref 
    | _ -> invalidArg "item" "not a union case or exception reference"

let expr_constr_unify m cenv env ty c = 
  gen_constr_unify ((fun (a,b) args -> mk_ucase(a,b,args,m)),
                    (fun a args -> mk_exnconstr (a,args,m))) m cenv env ty c
      
let pat_constr_unify m cenv env ty c = 
  gen_constr_unify ((fun (a,b) args -> TPat_unioncase(a,b,args,m)),
                    (fun a args -> TPat_exnconstr(a,args,m))) m cenv env ty c

let gen_constr_check (env:tcEnv) nargtys nargs m =
  if nargs <> nargtys then error (UnionCaseWrongArguments(env.DisplayEnv,nargtys,nargs,m))

let TcUnionCaseField cenv (env:tcEnv) ty1 m c n funcs =
    let ad = AccessRightsOfEnv env
    let mkf,argtys = 
      match ResolvePatternLongIdent cenv.nameResolver AllIdsOK false m ad env.eNameResEnv DefaultTypeNameResInfo c with
      | (Item_ucase _ | Item_ecref _) as item ->
        gen_constr_unify funcs m cenv env ty1 item
      | _ -> error(Error("unknown union case",m))
    if n >= List.length argtys then 
      error (UnionCaseWrongNumberOfArgs(env.DisplayEnv,List.length argtys,n,m));
    let ty2 = List.nth argtys n
    mkf,ty2

//-------------------------------------------------------------------------
// Environment of explicit type parameters, e.g. 'a in "(x : 'a)"
//------------------------------------------------------------------------- 

type SyntacticUnscopedTyparEnv = UnscopedTyparEnv of NameMap<Typar>

let emptyTyparEnv : SyntacticUnscopedTyparEnv = UnscopedTyparEnv (Map.empty)

let AddUnscopedTypar n p (UnscopedTyparEnv tab) = UnscopedTyparEnv (Map.add n p tab)

let TryFindUnscopedTypar n (UnscopedTyparEnv tab) = Map.tryfind n tab

let HideUnscopedTypars typars (UnscopedTyparEnv tab) = 
    UnscopedTyparEnv (List.fold (fun acc (tp:Typar) -> Map.remove tp.Name acc) tab typars)

//-------------------------------------------------------------------------
// Helpers for generalizing type variables
//------------------------------------------------------------------------- 

type GeneralizeConstrainedTyparOptions = 
    | CanGeneralizeConstrainedTypars 
    | DoNotGeneralizeConstrainedTypars


module GeneralizationHelpers = 
    let ComputeUngeneralizableTypars env = 
        let acc_in_free_item acc item =
           if item.WillNeverHaveFreeTypars then acc else
           let ftps = 
               let ftyvs = item.GetFreeTyvars()
               ftyvs.FreeTypars
           if ftps.IsEmpty then acc else union_free_loctypars ftps acc

        List.fold acc_in_free_item empty_free_loctypars env.eUngeneralizableItems 

    let ComputeUnabstractableTycons env = 
        let acc_in_free_item acc item = 
            let ftycs = 
                if item.WillNeverHaveFreeTypars then item.CachedFreeLocalTycons else 
                let ftyvs = item.GetFreeTyvars()
                ftyvs.FreeTycons
            if ftycs.IsEmpty then acc else union_free_loctycons ftycs acc

        List.fold acc_in_free_item empty_free_loctycons env.eUngeneralizableItems 

    let ComputeUnabstractableTraitSolutions env = 
        let acc_in_free_item acc item = 
            let ftycs = 
                if item.WillNeverHaveFreeTypars then item.CachedFreeTraitSolutions else 
                let ftyvs = item.GetFreeTyvars()
                ftyvs.FreeTraitSolutions
            if ftycs.IsEmpty then acc else union_free_locvals ftycs acc

        List.fold acc_in_free_item empty_free_locvals env.eUngeneralizableItems 

    let rec IsGeneralizableValue g t = 
        match t with 
        | TExpr_lambda _ | TExpr_tlambda _ | TExpr_const _ | TExpr_val _ -> true

        // Look through coercion nodes corresponding to introduction of subsumption 
        | TExpr_op(TOp_coerce,[inputTy;actualTy],[e1],_) when is_fun_typ g actualTy && is_fun_typ g inputTy -> 
            IsGeneralizableValue g e1

        | TExpr_op(op,_,args,_) ->
            match op with 
            | TOp_tuple  -> true
            | TOp_ucase uc -> not (ucref_alloc_observable uc)
            | TOp_recd(ctorInfo,tcref) -> 
                match ctorInfo with 
                | RecdExpr ->  not (tcref_alloc_observable tcref)
                | RecdExprIsObjInit -> false
            | TOp_array ->  isNil args
            | TOp_exnconstr ec -> not (ecref_alloc_observable ec)

            | TOp_asm([],_) -> true

            | _ -> false
            && List.forall (IsGeneralizableValue g) args

        | TExpr_letrec(binds,body,_,_)  ->
            FlatList.forall (rhs_of_bind >> IsGeneralizableValue g) binds &&
            IsGeneralizableValue g body
        | TExpr_let(bind,body,_,_) -> 
            (rhs_of_bind >> IsGeneralizableValue g) bind &&
            IsGeneralizableValue g body


        // Applications of type functions are _not_ normally generalizable unless explicitly marked so 
        | TExpr_app(TExpr_val (vref,_,_),_,_,[],_) when vref.IsTypeFunction -> 
            HasAttrib g g.attrib_GeneralizableValueAttribute vref.Attribs
             
        
        | TExpr_app(e1,_,_,[],_) -> IsGeneralizableValue g e1
        | TExpr_tchoose(_,b,_) -> IsGeneralizableValue g b
    (*
        | TExpr_quote _ -> true
    *)
        | TExpr_obj (_,ty,_,_,_,_,_,_) -> is_interface_typ g ty || is_delegate_typ g ty
        | _ -> false  

    let CanGeneralizeConstrainedTyparsForDecl declKind = 
        if DeclKind.CanGeneralizeConstrainedTypars declKind 
        then CanGeneralizeConstrainedTypars 
        else DoNotGeneralizeConstrainedTypars
        
    /// Recursively knock out typars we can't generalize. 
    /// For non-generalized type variables be careful to iteratively knock out 
    /// both the typars and any typars free in the constraints of the typars
    /// into the set that are considered free in the environment. 
    let rec TrimUngeneralizableTypars genConstrainedTyparFlag inlineFlag (generalizedTypars:Typar list) freeInEnv = 
        // Do not generalize type variables with a static requirement unless function is marked 'inline' 
        let generalizedTypars,ungeneralizableTypars1 =  
            if inlineFlag = PseudoValue then generalizedTypars,[]
            else generalizedTypars |> List.partition (fun tp -> tp.StaticReq = NoStaticReq) 

        // Do not generalize type variables which would escape their scope 
        // because they are free in the environment 
        let generalizedTypars,ungeneralizableTypars2 = 
            List.partition (fun x -> not (Zset.mem x freeInEnv)) generalizedTypars

        // Some situations, e.g. implicit class constructions that represent functions as fields, 
        // do not allow generalisation over constrained typars. (since they can not be represented as fields 
        let generalizedTypars,ungeneralizableTypars3 = 
            generalizedTypars |> List.partition (fun tp -> 
                genConstrainedTyparFlag = CanGeneralizeConstrainedTypars || 
                tp.Constraints.IsEmpty) 

        if isNil ungeneralizableTypars1 && isNil ungeneralizableTypars2 && isNil ungeneralizableTypars3 then
            generalizedTypars, freeInEnv
        else 
            let freeInEnv = 
                union_free_loctypars 
                    (acc_free_tprefs CollectAllNoCaching ungeneralizableTypars1 
                        (acc_free_tprefs CollectAllNoCaching ungeneralizableTypars2 
                            (acc_free_tprefs CollectAllNoCaching ungeneralizableTypars3 empty_free_tyvars))).FreeTypars 
                    freeInEnv
            TrimUngeneralizableTypars genConstrainedTyparFlag inlineFlag generalizedTypars freeInEnv

    /// Condense type variables in positive position
    let rec CondenseTypars(cenv,env:tcEnv,m,freeInEnv,generalizedTypars:typars,tauTy) =

        // The type of the value is ty11 * ... * ty1N -> ... -> tyM1 * ... * tyMM -> retTy
        // This is computed REGARDLESS of the arity of the expression.
        let curriedArgTys,retTy = strip_fun_typ cenv.g tauTy
        let allUntupledArgTys = curriedArgTys |> List.collect (try_dest_tuple_typ cenv.g)
        if verboseGeneralization then dprintfn "#allUntupledArgTys = %d" allUntupledArgTys.Length

        // Compute the type variables in 'retTy'
        let returnTypeFreeTypars = free_in_type_lr cenv.g false retTy
        let allUntupledArgTysWithFreeVars = allUntupledArgTys |> List.map (fun ty -> (ty, free_in_type_lr cenv.g false ty))

        let relevantUniqueSubtypeConstraint (tp:Typar) = 
            // Find a single subtype constraint
            match tp.Constraints |> List.partition (function (TTyparCoercesToType(cxty,_)) -> true | _ -> false) with 
            | ([TTyparCoercesToType(cxty,_)] as cxs), others -> 
                 // Throw away null constraints if they are implied 
                 match others |> List.filter (function (TTyparSupportsNull(_)) -> not (TypeSatisfiesNullConstraint cenv.g cxty) | _ -> true) with
                 | [] -> Some cxty
                 | _ -> None
            | _ -> None
                 

        // Condensation typars can't be used in the constraints of any candidate condensation typars. So compute all the
        // typars free in the constraints of tyIJ

        let lhsConstraintTypars = 
            allUntupledArgTys |> List.collect (fun ty -> 
                if is_typar_typ cenv.g ty then 
                    let tp = dest_typar_typ cenv.g ty 
                    match relevantUniqueSubtypeConstraint tp with 
                    | Some cxty -> free_in_type_lr cenv.g false cxty
                    | None -> []
                else [])

        let IsCondensationTypar (tp:Typar) = 
            // A condensation typar may not a user-generated type variable nor has it been unified with any user type variable
            (tp.DynamicReq = NoDynamicReq) && 
            // A condensation typar must have a single constraint "'a :> A"
            (isSome (relevantUniqueSubtypeConstraint tp)) &&
            // This is type variable is not used on the r.h.s. of the type
            not (ListSet.mem typar_ref_eq tp returnTypeFreeTypars) &&
            // A condensation typar can't be used in the constraints of any candidate condensation typars
            not (ListSet.mem typar_ref_eq tp lhsConstraintTypars) &&
            // A condensation typar must occur precisely once in tyIJ, and must not occur free in any other tyIJ
            (match allUntupledArgTysWithFreeVars |> List.partition (fun (ty,_) -> is_typar_typ cenv.g ty && typar_ref_eq (dest_typar_typ cenv.g ty) tp) with
             | [_], rest -> not (rest |> List.exists (fun (_,fvs) -> ListSet.mem typar_ref_eq tp fvs))
             | _ -> false)
             
        let condensationTypars, generalizedTypars = generalizedTypars |> List.partition IsCondensationTypar

        if verboseGeneralization then dprintfn "#condensationTypars = %d" condensationTypars.Length

        // Condensation solves type variables eagerly and removes them from the generalization set 
        condensationTypars |> List.iter (fun tp -> 
            ConstraintSolver.ChooseTyparSolutionAndSolve cenv.css env.DisplayEnv tp);
        generalizedTypars

    let verboseA = false

    let CanonicalizePartialInferenceProblem (cenv,denv,m) tps =
        // Canonicalize constraints prior to generalization 
        let csenv = (MakeConstraintSolverEnv cenv.css m denv)
        TryD (fun () -> ConstraintSolver.CanonicalizeRelevantMemberConstraints csenv 0 NoTrace tps)
             (fun res -> ErrorD (ErrorFromAddingConstraint(denv,res,m))) 
        |> RaiseOperationResult

    let ComputeGeneralizedTypars (cenv,env:tcEnv,m,immut,
                                  freeInEnv:FreeTypars,
                                  canInferTypars,
                                  genConstrainedTyparFlag,
                                  inlineFlag,
                                  exprOpt,
                                  allDeclaredTypars:typars,
                                  maxInferredTypars:typars,
                                  tauTy,
                                  resultFirst) =
        let prTypar tp = showL (typeL (mk_typar_ty tp))
        if verboseA then 
            dprintf "%a: ComputeGeneralizedTypars (0), freeInEnv = %s maxInferredTypars = %s allDeclaredTypars = %s tauTy = %s\n" output_range m 
              (String.concat "," (freeInEnv |> Zset.elements |> List.map (fun tp -> tp.Name)))
              (String.concat "," (maxInferredTypars |> List.map (fun tp -> tp.Name)))
              (String.concat "," (allDeclaredTypars |> List.map prTypar))
              (showL (typeL tauTy));
        let denv = env.DisplayEnv

        let allDeclaredTypars = NormalizeDeclaredTyparsForEquiRecursiveInference cenv.g allDeclaredTypars
        let typarsToAttemptToGeneralize = 
            if immut && (match exprOpt with None -> true | Some e -> IsGeneralizableValue cenv.g e) 
            then (ListSet.unionFavourLeft typar_ref_eq  allDeclaredTypars maxInferredTypars)
            else allDeclaredTypars

        if verboseA then dprintf "ComputeGeneralizedTypars (1), resultFirst = %s typarsToAttemptToGeneralize = %s\n" (if resultFirst then "yes" else "no") (String.concat "," (typarsToAttemptToGeneralize  |> List.map prTypar));

        let generalizedTypars,freeInEnv = 
            TrimUngeneralizableTypars genConstrainedTyparFlag inlineFlag typarsToAttemptToGeneralize freeInEnv

        if verboseA then dprintf "ComputeGeneralizedTypars (2), tauTy = %s generalizedTypars = %s\n" (showL (typeL tauTy)) (String.concat "," (generalizedTypars  |> List.map prTypar));

        allDeclaredTypars |> List.iter (fun tp -> 
              if Zset.mem_of freeInEnv tp then
                let ty =  mk_typar_ty tp
                error(Error(sprintf "This code is not sufficiently generic. The type variable %s could not be generalized because it would escape its scope" (NicePrint.pretty_string_of_typ denv ty),m)));
            
        let generalizedTypars = CondenseTypars(cenv,env,m,freeInEnv,generalizedTypars,tauTy)    

        let generalizedTypars =  
            if canInferTypars then generalizedTypars 
            else generalizedTypars |> List.filter (fun tp -> ListSet.mem typar_ref_eq tp allDeclaredTypars)

        let generalizedTypars = ConstraintSolver.SimplifyMeasuresInTypeScheme cenv.g resultFirst generalizedTypars tauTy

        if verboseA then dprintf "ComputeGeneralizedTypars (3), tauTy = %s generalized = %s\n" (showL (typeL tauTy)) (String.concat "," (generalizedTypars  |> List.map prTypar));

        let allDeclaredTypars = ChooseCanonicalDeclaredTyparsAfterInference cenv.g denv allDeclaredTypars m

        if verboseA then dprintf "ComputeGeneralizedTypars (4), allDeclaredTypars = %s\n" (String.concat "," (allDeclaredTypars  |> List.map prTypar));
        
        // Generalization turns inference type variables into rigid, quantified type variables. 
        generalizedTypars |> List.iter (SetTyparRigid cenv.g denv m);
        
        // Generalization removes constraints related to generalized type variables
        let csenv = MakeConstraintSolverEnv cenv.css m denv
        EliminateConstraintsForGeneralizedTypars csenv NoTrace generalizedTypars;
        
        generalizedTypars

    //-------------------------------------------------------------------------
    // Helpers to freshen existing types and values, i.e. when a reference
    // to C<_> occurs then generate C<?ty> for a fresh type inference variable ?ty.
    //------------------------------------------------------------------------- 


    let private ComputeExtraTyparsFeasible(memFlagsOpt,declaredTypars,m) = 
        (* Properties and Constructors may only generalize the variables associated with the containing class (retrieved from the 'this' pointer) *)
        (* Also check they don't declare explicit typars. *)
        match memFlagsOpt with 
        | None -> true
        | Some(memberFlags) -> 
            match memberFlags.MemberKind with 
            (* can't infer extra polymorphism for properties *)
            | MemberKindPropertyGet 
            | MemberKindPropertySet  -> 
                 if nonNil(declaredTypars) then 
                     errorR(Error("A property may not have explicit type parameters. Consider using a method instead",m));
                 false
            (* can't infer extra polymorphism for class constructors *)
            | MemberKindClassConstructor ->  
                 false
            (* can't infer extra polymorphism for constructors *)
            | MemberKindConstructor -> 
                 if nonNil(declaredTypars) then 
                     errorR(Error("A constructor may not have explicit type parameters. Consider using a static construction method instead",m));
                 false 
            | _ -> 
                 (* feasible to infer extra polymorphism *)
                 true                     

    let ComputeCanInferTypars(declKind,parentRef,canInferTypars,memFlagsOpt,declaredTypars,m) =  
            (match parentRef with Parent tcref -> not tcref.IsFSharpDelegateTycon | _ -> true) &&  // no generic paramters inferred for 'Invoke' method
            ComputeExtraTyparsFeasible(memFlagsOpt,declaredTypars,m) &&
            canInferTypars &&
            (generalizeInnerPolymorphism || DeclKind.MustAlwaysGeneralize declKind) 


//-------------------------------------------------------------------------
// ComputeInlineFlag 
//-------------------------------------------------------------------------

let ComputeInlineFlag memFlagsOption pseudo mut attrs = 
    // Mutable values may never be inlined 
    // Constructors may never be inlined 
    // Calls to virtual/abstract slots may never be inlined 
    if mut || 
       (match memFlagsOption with 
        | None -> false
        | Some x -> (x.MemberKind = MemberKindConstructor) || x.MemberIsDispatchSlot) 
    then NeverInline 
    elif pseudo then PseudoValue 
    else OptionalInline


//-------------------------------------------------------------------------
// Binding normalization.
//
// Determine what sort of value is being bound (normal value, instance
// member, normal function, static member etc.) and make some
// name-resolution-sensitive adjustments to the syntax tree.
//
// One part of this "normalization" ensures: 
//        "let Pat_lid(f) = e" when f not a datatype constructor --> let Pat_var(f) = e" 
//        "let Pat_lid(f) pat = e" when f not a datatype constructor --> let Pat_var(f) = \pat. e" 
//        "let (Pat_lid(f) : ty) = e" when f not a datatype constructor --> let (Pat_var(f) : ty) = e" 
//        "let (Pat_lid(f) : ty) pat = e" when f not a datatype constructor --> let (Pat_var(f) : ty) = \pat. e" 
// 
// This is because the first lambda in a function definition "let F x = e" 
// now looks like a constructor application, i.e. let (F x) = e ... 
//  also let A.F x = e ... 
//  also let f x = e ... 
//
// The other parts turn property definitions into method definitions.
//------------------------------------------------------------------------- 

 
let PushOnePatternToRhs isMember p (BindingRhs(spatsL,rtyOpt,rhsExpr)) = 
    let spats,rhsExpr = PushPatternToExpr isMember p rhsExpr
    BindingRhs(spats::spatsL, rtyOpt,rhsExpr)

type NormalizedBindingPatternInfo = 
    NormalizedBindingPat of SynPat * BindingRhs * ValSynData * SynValTyparDecls (* pat,rhsExpr,memberInfo,typars *)

/// Represents a syntactic, unchecked binding after the resolution of the name resolution status of pattern
/// constructors and after "pushing" all complex patterns to the right hand side.
type NormalizedBinding = 
  | NormalizedBinding of 
      access option *
      SynBindingKind *
      bool *  (* pesudo/mustinline value? *)
      bool *  (* mutable *)
      SynAttributes * 
      XmlDoc *
      SynValTyparDecls * 
      ValSynData * 
      SynPat * 
      BindingRhs *
      range *
      SequencePointInfoForBinding


type IsObjExprBinding = 
    | ObjExprBinding 
    | ValOrMemberBinding



module BindingNormalization =
    /// Push a bunch of pats at once. They may contain patterns, e.g. let f (A x) (B y) = ... 
    /// In this case the sematnics is let f a b = let A x = a in let B y = b 
    let private PushMultiplePatternsToRhs isMember ps (BindingRhs(spatsL,rtyOpt,rhsExpr)) = 
        let spatsL2,rhsExpr = PushCurriedPatternsToExpr (range_of_synexpr rhsExpr) isMember ps rhsExpr
        BindingRhs(spatsL2@spatsL, rtyOpt,rhsExpr)


    let private MkNormalizedStaticOrValBinding isObjExprBinding id vis typars args rhsExpr valSynData : NormalizedBindingPatternInfo = 
        let (ValSynData(memberFlagsOpt,_,_)) = valSynData 
        NormalizedBindingPat(mksyn_pat_var vis id, PushMultiplePatternsToRhs ((isObjExprBinding = ObjExprBinding) or isSome memberFlagsOpt) args rhsExpr,valSynData,typars)

    let private MkNormalizedInstanceMemberBinding thisId memberId vis m typars args rhsExpr valSynData = 
        NormalizedBindingPat(Pat_instance_member(thisId, memberId,vis,m), PushMultiplePatternsToRhs true args rhsExpr,valSynData,typars)

    let private NormalizeStaticMemberBinding memberFlags valSynData id vis typars args m rhsExpr = 
        let (ValSynData(_,valSynInfo,thisIdOpt)) = valSynData 
        if memberFlags.MemberIsInstance then 
            (* instance method without adhoc "this" argument *)
            error(Error("This instance member needs a parameter to represent the object being invoked. Make the member static or use the notation 'member x.Member(args) = ...'",m));
        match args, memberFlags.MemberKind  with 
        | _,MemberKindPropertyGetSet    -> error(Error("Unexpected source-level property specification in syntax tree",m))
        | [],MemberKindClassConstructor -> error(Error("A static initializer requires an argument",m))
        | [],MemberKindConstructor     -> error(Error("An object constructor requires an argument",m))
        | [_],MemberKindClassConstructor  
        | [_],MemberKindConstructor  -> MkNormalizedStaticOrValBinding ValOrMemberBinding id vis typars args rhsExpr valSynData
        // Static property declared using 'static member P = expr': transformed to a method taking a "unit" argument 
        // static property: these transformed into methods taking one "unit" argument 
        | [],MemberKindMember -> 
            let memberFlags = {memberFlags with MemberKind = MemberKindPropertyGet} 
            let valSynData = ValSynData(Some(memberFlags),valSynInfo,thisIdOpt)
            NormalizedBindingPat(mksyn_pat_var vis id,
                                 PushOnePatternToRhs true (Pat_const(Const_unit,m)) rhsExpr,
                                 valSynData,
                                 typars)
        | _ -> MkNormalizedStaticOrValBinding ValOrMemberBinding id vis typars args rhsExpr valSynData

    let private NormalizeInstanceMemberBinding memberFlags valSynData thisId memberId vis typars args m rhsExpr = 
        let (ValSynData(_,valSynInfo,thisIdOpt)) = valSynData 
        if not memberFlags.MemberIsInstance then 
            // static method with adhoc "this" argument 
            error(Error("This static member should not have a 'this' parameter. Consider using the notation 'member Member(args) = ...'",m));
        match args, memberFlags.MemberKind  with 
        | _,MemberKindClassConstructor  -> error(Error("An explicit static initializer should use the syntax 'static new(args) = expr'",m))
        | _,MemberKindConstructor  -> error(Error("An explicit object constructor should use the syntax 'new(args) = expr'",m))
        | _,MemberKindPropertyGetSet  -> error(Error("Unexpected source-level property specification",m))
        // Instance property declared using 'x.Member': transformed to methods taking a "this" and a "unit" argument 
        // We push across the 'this' arg in mk_rec_binds 
        | [],MemberKindMember -> 
            let memberFlags = {memberFlags with MemberKind = MemberKindPropertyGet}
            NormalizedBindingPat
                (Pat_instance_member(thisId, memberId,vis,m), 
                 PushOnePatternToRhs true (Pat_const(Const_unit,m)) rhsExpr,
                 (* Update the member info to record that this is a MemberKindPropertyGet *)
                 ValSynData(Some(memberFlags),valSynInfo,thisIdOpt),
                 typars)

        | _ -> MkNormalizedInstanceMemberBinding thisId memberId vis m typars args rhsExpr valSynData

    let private NormalizeBindingPattern nameResolver isObjExprBinding (env:tcEnv) valSynData pat rhsExpr =
        let ad = AccessRightsOfEnv env
        let (ValSynData(memberFlagsOpt,valSynInfo,thisIdOpt)) = valSynData 
        let rec normPattern pat = 
            // One major problem with versions of F# prior to 1.9.x was that data constructors easily 'pollute' the namespace 
            // of available items, to the point that you can't even define a function with the same name as an existing union case. 
            match pat with 
            | Pat_lid (lid,tyargs,args,vis,m) ->
                let typars = (match tyargs with None -> inferredTyparDecls | Some typars -> typars)
                match memberFlagsOpt with 
                | None ->                
                    match ResolvePatternLongIdent nameResolver AllIdsOK true m ad env.eNameResEnv DefaultTypeNameResInfo lid with
                    | Item_newdef id -> 
                        if id.idText = opname_Cons  then
                            NormalizedBindingPat(pat,rhsExpr,valSynData,typars)
                        else
                            if (isObjExprBinding = ObjExprBinding) then 
                                warning(Deprecated("This form of object expression is deprecated. Use 'member this.MemberName ... = ...' to define member implementations in object expressions",m))
                            MkNormalizedStaticOrValBinding isObjExprBinding id vis typars args rhsExpr valSynData
                    | res -> 
                        error(Error("Invalid declaration",m))

                | Some memberFlags ->                
                    match lid with 
                    // x.Member in member binding patterns. 
                    | [thisId;memberId] -> NormalizeInstanceMemberBinding memberFlags valSynData thisId memberId vis typars args m rhsExpr 
                    | [memberId]        -> NormalizeStaticMemberBinding memberFlags valSynData memberId vis typars args m rhsExpr 
                    | _                 -> NormalizedBindingPat(pat,rhsExpr,valSynData,typars)

            // Object constructors are normalized in TcLetrec 
            // Here we are normalizing member definitions with simple (not long) ids, 
            // e.g. "static member x = 3" and "member x = 3" (instance with missing "this." comes through here. It is trapped and generates a warning) 
            | Pat_as (Pat_wild _, id, false, vis, m) 
                when 
                   (match memberFlagsOpt with 
                    | None -> false 
                    | Some(memberFlags) -> 
                         not (memberFlags.MemberKind = MemberKindConstructor) &&
                         not (memberFlags.MemberKind = MemberKindClassConstructor)) ->            
                NormalizeStaticMemberBinding (the memberFlagsOpt) valSynData id vis inferredTyparDecls [] m rhsExpr 

            | Pat_typed(pat',x,y) ->             
                let (NormalizedBindingPat(pat'',e'',valSynData,typars)) = normPattern pat'
                NormalizedBindingPat(Pat_typed(pat'',x,y), e'',valSynData,typars)

            | Pat_attrib(pat',x,m) ->             
                error(Error("Attributes are not allowed within patterns",m));
                //let (NormalizedBindingPat(pat'',e'',valSynData,typars)) = normPattern pat'
                //NormalizedBindingPat(Pat_attrib(pat'',x,m), e'',valSynData,typars)

            | _ ->
                NormalizedBindingPat(pat,rhsExpr,valSynData,inferredTyparDecls) 
        normPattern pat

    let NormalizeBinding isObjExprBinding cenv (env:tcEnv) b = 
        match b with 
        | Binding (vis,bkind,pseudo,mut,attrs,doc,valSynData,p,rhsExpr,bindingRange,spBind) ->
            let (NormalizedBindingPat(pat,rhsExpr,valSynData,typars)) = 
                NormalizeBindingPattern cenv.nameResolver isObjExprBinding env valSynData p rhsExpr
            NormalizedBinding(vis,bkind,pseudo,mut,attrs,doc.ToXmlDoc(),typars,valSynData,pat,rhsExpr,bindingRange,spBind)

//-------------------------------------------------------------------------
// input is:
//    [<CompileAsEvent>]
//    member x.P with get = fun () -> e
// --> 
//    member x.add_P(argName) = (e).AddHandler(argName)
//    member x.remove_P(argName) = (e).RemoveHandler(argName)

module EventDeclarationNormalization = 
    let ConvertSynInfo m (ValSynInfo(argInfos,retInfo)) = 
       // reconstitute valSynInfo by adding the argument
       let argInfos = 
           match argInfos with 
           | [[thisArgInfo];[]] ->  [[thisArgInfo];SynInfo.unnamedTopArg] // instance property getter
           | [[]] -> [SynInfo.unnamedTopArg] // static property getter
           | _ -> error(BadEventTransformation(m))

       // reconstitute valSynInfo
       ValSynInfo(argInfos,retInfo)

    // THe property x.P becomes methods x.add_P and x.remove_P
    let ConvertMemberFlags  memberFlags = { memberFlags with MemberKind= MemberKindMember } 

    let private ConvertMemberFlagsOpt m memberFlagsOpt =
        match memberFlagsOpt with 
        | Some memberFlags -> Some (ConvertMemberFlags memberFlags)
        | _ -> error(BadEventTransformation(m))

    let private ConvertSynData m valSynData =
        let (ValSynData(memberFlagsOpt,valSynInfo,thisIdOpt)) = valSynData 
        let memberFlagsOpt = ConvertMemberFlagsOpt m memberFlagsOpt
        let valSynInfo = ConvertSynInfo m valSynInfo
        ValSynData(memberFlagsOpt,valSynInfo,thisIdOpt)
     
    let rec private  RenameBindingPattern f declPattern = 
        match declPattern with  
        | Pat_typed(pat',cty,_) -> RenameBindingPattern f pat'
        | Pat_as (Pat_wild m1, id,x2,vis2,m) -> Pat_as (Pat_wild m1, ident(f id.idText,id.idRange) ,x2,vis2,m) 
        | Pat_instance_member(thisId, id,vis2,m) -> Pat_instance_member(thisId, ident(f id.idText,id.idRange),vis2,m)
        | _ -> error(Error("only simple variable patterns can be bound in 'let rec' constructs",range_of_synpat declPattern))

    let GenerateExtraBindings (g,bindingAttribs,binding) =
        let (NormalizedBinding(vis1,bindingKind,isInline,isMutable,bindingSynAttribs,bindingXmlDoc,synTyparDecls,valSynData,declPattern,bindingRhs,bindingRange,spBind)) = binding
        if CompileAsEvent g bindingAttribs then 

            let MakeOne (prefix,target) = 
                let declPattern = RenameBindingPattern (fun s -> prefix^s) declPattern
                let argName = "handler"
                // modify the rhs and argument data
                let bindingRhs,valSynData = 
                   let (BindingRhs(_,_,rhsExpr)) = bindingRhs
                   let m = range_of_synexpr rhsExpr
                   // reconstitute valSynInfo by adding the argument
                   let valSynData = ConvertSynData m valSynData

                   match rhsExpr with 
                   // Detect 'fun () -> e'
                   | Expr_lambda (_,_,SPats([],_), trueRhsExpr,m) ->
                       let rhsExpr = Expr_app(ExprAtomicFlag.NonAtomic,Expr_lvalue_get(Expr_paren(trueRhsExpr,m),[ident(target,m)],m),Expr_id_get(ident(argName,m)),m)
                       
                       // reconstitute rhsExpr
                       let bindingRhs = BindingRhs([],None,rhsExpr)

                       // add the argument to the expression 
                       let bindingRhs = PushOnePatternToRhs true (mksyn_pat_var None (ident (argName,bindingRange))) bindingRhs 
                       
                       bindingRhs,valSynData
                   | _ -> 
                       error(BadEventTransformation(m))

                // reconstitute the binding
                NormalizedBinding(vis1,bindingKind,isInline,isMutable,[],bindingXmlDoc,synTyparDecls,valSynData,declPattern,bindingRhs,bindingRange,spBind) 

            [ MakeOne ("add_","AddHandler"); MakeOne ("remove_","RemoveHandler") ]
        else 
            []

(*-------------------------------------------------------------------------
 * Helpers to adjust the 'this' pointer before making a call.
 *------------------------------------------------------------------------- *)

type ObjectArgCoercionInfo = 
    | CoerceThisBeforeCall of Tast.typ * Tast.typ 
    | DontCoerceThisBeforeCall

/// Compute whether we insert a 'coerce' on the 'this' pointer for an object model call 
/// For example, when calling an interface method on a struct, or a method on a constrained 
/// variable type. 
let ComputeObjectArgCoercion cenv m (objArgs,methArgTy) =
    match objArgs with 
    | [objArgExpr] -> 
        let objArgExprTy = type_of_expr cenv.g objArgExpr
        if type_definitely_subsumes_type_no_coercion 0 cenv.g cenv.amap m methArgTy objArgExprTy then 
            DontCoerceThisBeforeCall
        else
            CoerceThisBeforeCall (objArgExprTy, methArgTy)
    | _ -> 
        DontCoerceThisBeforeCall

/// Adjust the 'this' pointer before making a call 
/// Take the address of a struct, and coerce to an interface/base/constraint type if necessary 
let TakeObjAddr cenv (minfo:MethInfo) mut m objArgs f =
    let boxopts = ComputeObjectArgCoercion cenv m (objArgs,minfo.EnclosingType) 
    let valu = minfo_is_struct cenv.g minfo && not minfo.IsExtensionMember  // don't take the address of a struct when passing to an extension member
    let wrap,objArgs = 
        match objArgs with
        | [h] -> 
            let hty = type_of_expr cenv.g h
            let wrap,h' = mk_expra_of_expr cenv.g valu mut h m 
            let h' = 
              match boxopts with 
              | CoerceThisBeforeCall(srcTy,tgtTy) -> mk_coerce(h',tgtTy,m,srcTy)
              | DontCoerceThisBeforeCall -> h'
            wrap,[h'] 
        | _ -> (fun x -> x), objArgs
    let e,ety = f objArgs
    wrap e,ety

let FreshenObjectArgType cenv m rigid tcref isExtrinsic declaredTyconTypars = 
    let tcrefObjTy,enclosingDeclaredTypars,renaming,objTy = FreshenTyconRef m rigid tcref declaredTyconTypars
    // Struct members have a byref 'this' type (unless they are extrinsic extension members)
    let thisTy = 
        if is_struct_tcref tcref && not isExtrinsic then 
            mk_byref_typ cenv.g objTy 
        else 
            objTy
    tcrefObjTy,enclosingDeclaredTypars,renaming,objTy,thisTy



/// TcVal. "Use" a value, normally at a fresh type instance (unless optInst is
/// given). optInst is set when an explicit type instantiation is given, e.g. 
///     Seq.empty<string>
/// In this case the vrefFlags inside optInst are just NormalValUse.
///
/// optInst is is also set when building the final call for a reference to an
/// F# object model member, in which case the optInst is the type instantiation
/// inferred by member overload resolution, and vrefFlags indicate if the
/// member is being used in a special way, i.e. may be one of:
///    | CtorValUsedAsSuperInit    "inherit Panel()"
///    | CtorValUsedAsSelfInit     "new() = new OwnType(3)"
///    | VSlotDirectCall           "base.OnClick(eventArgs)"
let TcVal cenv env tpenv (vref:ValRef) optInst m =
    let v = deref_val vref
    let vrec = v.RecursiveValInfo
    CheckValAccessible m (AccessRightsOfEnv env) vref;
    CheckValAttributes cenv.g vref m  |> CommitOperationResult;
    let vty = vref.Type
    (* byref types get dereferenced *)
    if is_byref_typ cenv.g vty then 
        let isSpecial = true
        mk_lval_get m vref,isSpecial,dest_byref_typ cenv.g vty,tpenv
    else 
      match v.LiteralValue with 
      | Some c -> 
          // Literal values go to constants 
          let isSpecial = true
          // The value may still be generic, e.g. 
          //   [<Literal>]
          //   let Null = null
          let _,_,tau = FreshenPossibleForallTy cenv.g m TyparFlexible vty 
          TExpr_const(c,m,tau),isSpecial,tau,tpenv

      | None -> 
          // References to 'this' in classes get dereferenced from their implicit reference cell and poked
        if v.BaseOrThisInfo = CtorThisVal then 
            let exprForVal = expr_for_vref m vref
            if AreWithinCtorPreConstruct env then 
                warning(SelfRefObjCtor(AreWithinImplicitCtor env, m));

            let ty = (dest_refcell_ty cenv.g vty)
            let isSpecial = true
            mk_nonnull_poke cenv.g m (mk_refcell_get cenv.g m ty exprForVal), isSpecial,ty, tpenv
        else 
          // Instantiate the value 
          let vrefFlags,tinst,tau,tpenv = 
              // Have we got an explicit instantiation? 
              match optInst with 
              // No explicit instantiation (the normal case)
              | None -> 
                  if HasAttrib cenv.g cenv.g.attrib_RequiresExplicitTypeArgumentsAttribute v.Attribs then
                       errorR(Error("The generic function '"^v.DisplayName^"' must be given explicit type argument(s)",m));
              
                  match vrec with 
                  | ValInRecScope(false) -> 
                      let tps,tau =  vref.TypeScheme
                      let tinst = tps |> List.map mk_typar_ty
                      NormalValUse,tinst,tau,tpenv
                  | ValInRecScope(true) 
                  | ValNotInRecScope ->
                      let tps,tinst,tau = FreshenPossibleForallTy cenv.g m TyparFlexible vty 
                      NormalValUse,tinst,tau,tpenv

              // If we have got an explicit instantiation then use that 
              | Some(vrefFlags,checkTys) -> 
                      match vrec with 
                      | ValInRecScope(false) -> 
                          let tpsorig,tau =  vref.TypeScheme
                          let (tinst:Tast.tinst),tpenv = checkTys tpenv (tpsorig |> List.map (fun tp -> tp.Kind))
                          if tpsorig.Length <> tinst.Length then error(Error(sprintf "This value expects %d type parameter(s) but is given %d" tpsorig.Length tinst.Length,m));
                          let tau2 = InstType (mk_typar_inst tpsorig tinst) tau
                          List.iter2
                            (fun tp ty -> 
                              try unifyE cenv env m (mk_typar_ty tp) ty
                              with _ -> error (Recursion(env.DisplayEnv,v.Id,tau2,tau,m))) 
                            tpsorig 
                            tinst;
                          vrefFlags,tinst,tau2,tpenv  
                      | ValInRecScope(true) 
                      | ValNotInRecScope ->
                          let tps,tptys,tau = FreshenPossibleForallTy cenv.g m TyparFlexible vty 
                          //dprintfn "After Freshen: tau = %s" (Layout.showL (typeL tau));
                          let (tinst:Tast.tinst),tpenv = checkTys tpenv (tps |> List.map (fun tp -> tp.Kind))
                          //dprintfn "After Check: tau = %s" (Layout.showL (typeL tau));
                          if tptys.Length <> tinst.Length then error(Error(sprintf "This value expects %d type parameter(s) but is given %d" (List.length tps) (List.length tinst),m));
                          List.iter2 (unifyE cenv env m) tptys tinst;
                          //dprintfn "After Unify: tau = %s" (Layout.showL (typeL tau));
                          vrefFlags,tinst,tau,tpenv  
                  
          let exprForVal = TExpr_val (vref,vrefFlags,m)
          let exprForVal = mk_tyapp m (exprForVal,vty) tinst
          //dprintfn "typeof(vty) = %s" (Layout.showL (typeL vty));
          //for ty in tinst do 
          //    dprintfn "(tinst) : ty = %s" (Layout.showL (typeL ty));
          //dprintfn "typeof(exprForVal) = %s" (Layout.showL (typeL (type_of_expr cenv.g exprForVal)));
          //if is_forall_typ vty then 
          //    dprintfn "reduce_forall_typ vty tinst = %s" (Layout.showL (typeL (reduce_forall_typ cenv.g vty tinst)));
          //dprintfn "After All: tau = %s" (Layout.showL (typeL tau));
          let isSpecial = 
              (vrefFlags <> NormalValUse) ||  
              cenv.g.vref_eq vref cenv.g.splice_expr_vref || 
              cenv.g.vref_eq vref cenv.g.splice_raw_expr_vref 
          
          let exprForVal =  RecordUseOfRecValue cenv vrec vref exprForVal m

          exprForVal,isSpecial,tau,tpenv

/// Mark points where we decide whether an expression will support automatic
/// decondensation or not. This is somewhat a relic of a previous implementation of decondensation and could
/// be removed

type ApplicableExpr = 
    | ApplicableExpr of 
           // context
           cenv * 
           // the function-valued expression
           Tast.expr
    member x.Range = 
        match x with 
        | ApplicableExpr (cenv,e) -> range_of_expr e
    member x.Type = 
        match x with 
        | ApplicableExpr (cenv,e) -> type_of_expr cenv.g e 
    member x.SupplyArgument(e2,m) =
        match x with 
        | ApplicableExpr (cenv,e) -> ApplicableExpr(cenv,mk_appl cenv.g ((e,type_of_expr cenv.g e),[],[e2],m))
    member x.Expr =
        match x with 
        | ApplicableExpr(_,e) ->  e
 
let MkApplicableExprNoFlex cenv expr =
    ApplicableExpr (cenv,expr)

/// This function reverses the effect of condensation for a named function value (indeed it can
/// work for any expression, though we only invoke it immediately after a call to TcVal).
///
/// De-condensation is determined BEFORE any arguments are checked. Thus
///      let f (x:'a) (y:'a) = ()
///
///      f  (new obj()) "string"
///
/// does not type check (the argument instantiates 'a to "obj" but there is no flexibility on the
/// second argument position.
///
/// De-condensation is applied AFTER taking into account an explicit type instantiation. This
///      let f<'a> (x:'a) = ()
///
///      f<obj>("string)"
///
/// will type check but
///
/// Sealed types and 'obj' do not introduce generic flexibility when functions are used as first class
/// values. 
///
/// For 'obj' this is because introducing this flexibility would NOT be the reverse of condensation,
/// since we don't condense 
///     f : 'a -> unit
/// to
///     f : obj -> unit
///
/// We represent the flexibility in the TAST by leaving a function-to-function coercion node in the tree
/// This "special" node is immediately eliminated by the use of IteratedFlexibleAdjustArityOfLambdaBody as soon as we 
/// first transform the tree (currently in optimization)

let MkApplicableExprWithFlex cenv (env:tcEnv) expr =
    let exprTy = type_of_expr cenv.g expr
    let m = range_of_expr expr
    
    let IsNonFlexibleType ty = is_sealed_typ cenv.g ty (* || is_obj_typ cenv.g ty *)
    
    let argTys,retTy = strip_fun_typ cenv.g exprTy
    let curriedActualTypes = argTys |> List.map (try_dest_tuple_typ cenv.g)
    if (curriedActualTypes.IsEmpty ||
        curriedActualTypes |> List.exists (List.exists (is_byref_typ cenv.g)) ||
        curriedActualTypes |> List.forall (List.forall IsNonFlexibleType)) then 
       
        ApplicableExpr (cenv,expr)
    else
        let curriedFlexibleTypes = 
            curriedActualTypes |> List.mapSquared (fun actualType -> 
                if IsNonFlexibleType actualType 
                then actualType 
                else 
                   let flexibleType = new_inference_typ cenv ()
                   AddCxTypeMustSubsumeType env.DisplayEnv cenv.css m NoTrace actualType flexibleType;
                   flexibleType)

        // Create a coercion to represent the expansion of the application
        let expr = mk_coerce (expr,mk_iterated_fun_ty (List.map (mk_tupled_ty cenv.g) curriedFlexibleTypes) retTy,m,exprTy)
        ApplicableExpr (cenv,expr)


///  Checks, warnings and constraint assertions for downcasts 
let TcRuntimeTypeTest cenv denv m tgty srcTy =
    if type_definitely_subsumes_type_no_coercion 0 cenv.g cenv.amap m tgty srcTy then 
      warning(TypeTestUnnecessary(m));

    if is_typar_typ cenv.g srcTy then 
        error(IndeterminateRuntimeCoercion(denv,srcTy,tgty,m));

    if is_sealed_typ cenv.g srcTy then 
        error(RuntimeCoercionSourceSealed(denv,srcTy,m));

    if is_sealed_typ cenv.g tgty ||
       is_typar_typ cenv.g tgty ||
       not (is_interface_typ cenv.g srcTy) then 
        AddCxTypeMustSubsumeType denv cenv.css m NoTrace srcTy tgty

///  Checks, warnings and constraint assertions for upcasts 
let TcStaticUpcast cenv denv m tgty srcTy =
    if is_typar_typ cenv.g tgty then 
        error(IndeterminateStaticCoercion(denv,srcTy,tgty,m)); 

    if is_sealed_typ cenv.g tgty then 
        warning(CoercionTargetSealed(denv,tgty,m));

    if type_equiv cenv.g srcTy tgty then 
        warning(UpcastUnnecessary(m)); 

    AddCxTypeMustSubsumeType denv cenv.css m NoTrace tgty srcTy


/// Build an expression that calls a given method info. 
/// This is called after overload resolution, and also to call other 
/// methods such as 'setters' for properties. 
//   isProp: is it a property get? 
//   minst: the instantiation to apply for a generic method 
//   objArgs: the 'this' argument, if any 
//   args: the arguments, if any 
let BuildMethodCall cenv env mut m isProp minfo vFlags minst objArgs args =
    let direct = IsBaseCall objArgs
    let vFlags = if (direct && vFlags = NormalValUse) then VSlotDirectCall else vFlags

    let conditionalCallDefine = 
        TryBindMethInfoAttribute cenv.g cenv.g.attrib_ConditionalAttribute minfo 
                     (function ([CustomElem_string (Some(msg)) ],_) -> Some(msg) | _ -> None) 
                     (function (Attrib(_,_,[ AttribStringArg(msg) ],_,_)) -> Some(msg) | _ -> None)

    match conditionalCallDefine with 
    | Some(d) when not (List.mem d cenv.conditionalDefines) -> 

        // Methods marked with 'Conditional' must return 'unit' 
        unifyE cenv env m cenv.g.unit_ty (FSharpReturnTyOfMeth cenv.amap m minfo minst);
        mk_unit cenv.g m, cenv.g.unit_ty

    | _ -> 

        TakeObjAddr cenv minfo mut m objArgs (fun objArgs -> 
            let allArgs = (objArgs @ args)
            match minfo with 
            
            // Build a call to a .NET method 
            | ILMeth(g,ilminfo) -> 
                mk_il_minfo_call cenv.g cenv.amap m isProp ilminfo vFlags minst direct allArgs

            // Build a call to an F# method 
            | FSMeth(g,typ,vref) -> 

                // Go see if this is a use of a recursive definition... Note we know the value instantiation 
                // we want to use so we pass that in in order not to create a new one. 
                let vexp, _,vexpty, _ = TcVal cenv env emptyTyparEnv vref (Some(vFlags,fun tpenv _ -> (tinst_of_stripped_typ cenv.g typ @ minst, tpenv))) m

                if verbose then dprintf "--> Build Method Call to %s, typ = %s\n" minfo.LogicalName (showL (typeL typ));
                if verbose then dprintf "--> Build Method Call to %s, vexpty = %s\n" minfo.LogicalName (showL (typeL vexpty));

                BuildFSharpMethodApp cenv.g m vref vexp vexpty allArgs

            // Build a 'call' to a struct default constructor 
            | DefaultStructCtor (g,typ) -> 
                if not (TypeHasDefaultValue g typ) then 
                    errorR(Deprecated("The default, zero-initializing constructor of a struct type may only be called directly if all the fields of the struct type admit default initialization",m));
                mk_ilzero (m,typ), typ)



/// Build the 'test and dispose' part of a 'use' statement 
let BuildDisposableCleanup cenv env m  (v:Val) = 
    let ad = AccessRightsOfEnv env
    let disv,dise = Tastops.mk_compgen_local m "objectToDispose" cenv.g.system_IDisposable_typ
    let disposeMethod = 
        match TryFindMethInfo cenv.infoReader m ad "Dispose" cenv.g.system_IDisposable_typ with 
        | [x] ->  x 
        | _ -> error(InternalError("Couldn't find Dispose on IDisposable, or it was overloaded",m)) 
    let disposeE,_ = BuildMethodCall cenv env PossiblyMutates   m false disposeMethod NormalValUse [] [dise] []
    let inpe = mk_coerce(expr_for_val v.Range v,cenv.g.obj_ty,m,v.Type)
    mk_isinst_cond cenv.g m cenv.g.system_IDisposable_typ inpe disv disposeE (mk_unit cenv.g m) 

let BuildILFieldGet g amap m objExpr (finfo:ILFieldInfo) = 
    let fref = finfo.ILFieldRef
    let isValueType = finfo.IsValueType
    let valu = if isValueType then AsValue else AsObject
    let tinst = finfo.TypeInst
    let fieldType = FieldTypeOfILFieldInfo amap m  finfo

    let wrap,objExpr = mk_expra_of_expr g isValueType NeverMutates objExpr m 
      // The empty instantiation on the AbstractIL fspec is OK, since we make the correct fspec in Ilxgen.GenAsm 
      // This ensures we always get the type instantiation right when doing this from 
      // polymorphic code, after inlining etc. *
    let fspec = mk_fspec(fref,mk_named_typ valu fref.EnclosingTypeRef [])
    // Add an I_nop if this is an initonly field to make sure we never recognize it as an lvalue. See mk_expra_of_expr. 
    wrap (mk_asm (([ mk_normal_ldfld fspec ] @ (if finfo.IsInitOnly then [ I_arith AI_nop ] else [])), tinst,[objExpr],[fieldType],m)) 

let BuildILFieldSet g m objExpr (finfo:ILFieldInfo) argExpr = 
    let fref = finfo.ILFieldRef
    let isValueType = finfo.IsValueType
    let valu = if isValueType then AsValue else AsObject
    let tinst = finfo.TypeInst
      // The empty instantiation on the AbstractIL fspec is OK, since we make the correct fspec in Ilxgen.gen_asm 
      // This ensures we always get the type instantiation right when doing this from 
      // polymorphic code, after inlining etc. *
    let fspec = mk_fspec(fref,mk_named_typ valu fref.EnclosingTypeRef [])
    if finfo.IsInitOnly then error (Error ("this field is readonly",m));
    let wrap,objExpr = mk_expra_of_expr g isValueType DefinitelyMutates objExpr m 
    (mk_asm ([ mk_normal_stfld fspec ], tinst,[objExpr; argExpr],[],m)) 

let BuildILStaticFieldSet g m (finfo:ILFieldInfo) argExpr = 
    let fref = finfo.ILFieldRef
    let isValueType = finfo.IsValueType
    let valu = if isValueType then AsValue else AsObject
    let tinst = finfo.TypeInst
      // The empty instantiation on the AbstractIL fspec is OK, since we make the correct fspec in Ilxgen.gen_asm 
      // This ensures we always get the type instantiation right when doing this from 
      // polymorphic code, after inlining etc. 
    let fspec = mk_fspec(fref,mk_named_typ valu fref.EnclosingTypeRef [])
    if finfo.IsInitOnly then error (Error ("this field is readonly",m));
    mk_asm ([ mk_normal_stsfld fspec ], tinst,[argExpr],[],m)
    
let BuildRecdFieldSet g m denv objExpr (rfinfo:RecdFieldInfo) ftinst argExpr = 
    let tgty = rfinfo.EnclosingType
    let valu = is_struct_typ g tgty
    let objExpr = if valu then objExpr else mk_coerce(objExpr,tgty,m,type_of_expr g objExpr)
    mk_recd_field_set g (objExpr,rfinfo.RecdFieldRef,rfinfo.TypeInst,argExpr,m) 
    
    
(*-------------------------------------------------------------------------
 * Helpers dealing with named and optional args at callsites
 *------------------------------------------------------------------------- *)

/// Detect a named argument at a callsite
let TryGetNamedArg e = 
    match e with 
    | Expr_app (_, Expr_app(_, Expr_single_id_get(nm), 
                         Expr_lid_or_id_get(isOpt,[a],_),_),b,_) when nm = opname_Equals ->
        Some(isOpt,a,b)
    | _ -> None 

let IsNamedArg e = isSome (TryGetNamedArg e)

/// Get the method arguments at a callsite, taking into account optional arguments
let GetMethodArgs arg =
    if verbose then  dprintf "--> tc_get_method_args\n";
    let args = 
        match arg with 
        | Expr_const (Const_unit,m) -> []
        | Expr_paren(Expr_tuple (args,m),_) | Expr_tuple (args,m) -> args
        | Expr_paren(arg,_) | arg -> [arg]
    let unnamedCallerArgs,namedCallerArgs = List.takeUntil IsNamedArg args
    let namedCallerArgs = 
        namedCallerArgs |> List.choose (fun e -> 
          if not (IsNamedArg e) then 
              error(Error("Named arguments must appear after all other arguments",range_of_synexpr e)); 
          TryGetNamedArg e)
    if verbose then  dprintf "in GetMethodArgs\n";
    unnamedCallerArgs, namedCallerArgs


//-------------------------------------------------------------------------
// Helpers dealing with adhoc conversions (functions to delegates)
//------------------------------------------------------------------------- 

/// Implements the elaborated form of adhoc conversions from functions to delegates at member callsites
let BuildNewDelegateExpr (eventInfoOpt:EventInfo option) cenv delty (minfo,delArgTys) (f,fty) m =
    if verbose then dprintf "--> BuildNewDelegateExpr\n";
    let slotsig = SlotSigOfMethodInfo cenv.amap m minfo
    let delArgVals,expr = 
        let topValInfo = TopValInfo([],List.replicate (List.length delArgTys) TopValInfo.unnamedTopArg, TopValInfo.unnamedRetVal)

        // Try to pull apart an explicit lambda and use it directly 
        // Don't do this in the case where we're adjusting the arguments of a function used to build a .NET-compatible event handler 
        match (if isSome eventInfoOpt then None 
               else try_dest_top_lambda_upto cenv.g cenv.amap topValInfo (f, fty)) with 
        | None -> 
        
            if List.exists (is_byref_typ cenv.g) delArgTys then
                    error(Error("This function value is being used to construct a delegate type whose signature includes a byref argument. You must use an explicit lambda expression taking "^string(List.length delArgTys)^" arguments",m)); 

            let delArgVals = delArgTys |> List.map (fun argty -> fst (mk_compgen_local m "delegateArg" argty)) 
            let expr = 
                let args = 
                    match eventInfoOpt with 
                    | Some einfo -> 
                        match delArgVals with 
                        | [] -> error(nonStandardEventError einfo.EventName m)
                        | h :: _ when not (is_obj_typ cenv.g h.Type) -> error(nonStandardEventError einfo.EventName m)
                        | _ :: t -> [mk_tupled_vars cenv.g m t] 
                    | None -> if isNil delArgTys then [mk_unit cenv.g m] else List.map (expr_for_val m) delArgVals
                mk_appl cenv.g ((f,fty),[],args,m)
            delArgVals,expr
            
        | Some _ -> 
           if isNil delArgTys then [], mk_appl cenv.g ((f,fty),[],[mk_unit cenv.g m],m) 
           else 
               let _,_,vsl,body,_ = IteratedAdjustArityOfLambda cenv.g cenv.amap topValInfo f
               List.concat vsl, body
            
    let meth = TObjExprMethod(slotsig,[],[delArgVals],expr,m)
    mk_obj_expr(delty,None,mk_obj_ctor_call cenv.g m,[meth],[],m)


//-------------------------------------------------------------------------
// Helpers dealing with pattern match compilation
//------------------------------------------------------------------------- 

let CompilePatternForMatch cenv (env:tcEnv) exprm matchm warnOnUnused actionOnFailure (v,generalizedTypars) clauses resultTy =
    let dtree,targets = CompilePattern cenv.g env.DisplayEnv cenv.amap exprm matchm warnOnUnused actionOnFailure (v,generalizedTypars) clauses resultTy
    mk_and_optimize_match NoSequencePointAtInvisibleBinding exprm matchm resultTy dtree targets

/// Compile a pattern
let CompilePatternForMatchClauses cenv env exprm matchm warnOnUnused actionOnFailure inputTy resultTy tclauses = 
    // avoid creating a dummy in the common cases where we are about to bind a name for the expression 
    // REVIEW: avoid code duplication with code further below, i.e.all callers should call CompilePatternForMatch 
    match tclauses with 
    | [TClause(TPat_as (pat1,PBind (v,TypeScheme(generalizedTypars,_,_)),m1),None,TTarget(vs,e,spTarget),m2) as clause] ->
        let expr = CompilePatternForMatch cenv env exprm matchm warnOnUnused actionOnFailure (v,generalizedTypars) [TClause(pat1,None,TTarget(FlatListSet.remove vspec_eq v vs,e,spTarget),m2)] resultTy
        v,expr
    | _ -> 
        let idv,idve = Tastops.mk_compgen_local exprm "matchValue" inputTy
        let expr = CompilePatternForMatch cenv env exprm matchm warnOnUnused actionOnFailure (idv,[]) tclauses resultTy
        idv,expr


//-------------------------------------------------------------------------
// Helpers dealing with sequence expressions
//------------------------------------------------------------------------- 

   
/// Get the fragmentary expressions resulting from turning 
/// an expression into an enumerable value, e.g. at 'for' loops 

// REVIEW: do this in a 'lowering' phase later in compilation 
// to ensure the quotation contains a higher level view of the expression 
let AnalyzeArbitraryExprAsEnumerable cenv (env:tcEnv) m exprty expr =
    let ad = AccessRightsOfEnv env
    let exprToSearchForGetEnumeratorAndItem,tyToSearchForGetEnumeratorAndItem = 
        let argty = new_inference_typ cenv ()
        let exprty_as_seq = mk_seq_ty cenv.g argty
        if (AddCxTypeMustSubsumeTypeUndoIfFailed env.DisplayEnv cenv.css m exprty_as_seq exprty) then 
           mk_coerce(expr,exprty_as_seq,range_of_expr expr,exprty),exprty_as_seq
        else
           expr,exprty

    let findMethInfo g amap m nm ty = 
        match TryFindMethInfo cenv.infoReader m ad nm ty with 
        | [] -> error(Error("The type '"^NicePrint.pretty_string_of_typ env.DisplayEnv ty^"' is not a type whose values can be enumerated with this syntax, i.e. is not compatible with either seq<_>, IEnumerable<_> or IEnumerable and does not have a GetEnumerator method",m));
        | res :: _ -> res  
        (* We can't apply this condition because IEnumerable<_> itself has multiple GetEnumerator etc. methods *)
        (* | _ -> error(Error("The type '"^NicePrint.pretty_string_of_typ env.DisplayEnv ty^"' has an overloaded '"^nm^" method and may not be enumerated using an enumeration loop",m)) *)
       
      
    let getEnumerator_minfo    = findMethInfo cenv.g cenv.amap m "GetEnumerator" tyToSearchForGetEnumeratorAndItem
    let retTypeOfGetEnumerator = FSharpReturnTyOfMeth cenv.amap m getEnumerator_minfo []

    let moveNext_minfo         = findMethInfo cenv.g cenv.amap m "MoveNext" retTypeOfGetEnumerator
    let get_Current_minfo      = findMethInfo cenv.g cenv.amap m "get_Current" retTypeOfGetEnumerator
    let argty                  = FSharpReturnTyOfMeth cenv.amap m get_Current_minfo []
    
    // Compute the element type of the strongly typed enumerator
    //
    // Like C#, we detect the 'GetEnumerator' pattern for .NET version 1.x abstractions that don't 
    // support the correct generic interface. However unlike C# we also go looking for a 'get_Item' or 'Item' method
    // with a single integer indexer argument to try to get a strong type for the enumeration should the Enumerator
    // not provide anything useful. For some legacy COM APIs the single integer indexer argument is allowed to have type 'object'
    //
    let argty = 
        if is_obj_typ cenv.g argty then 

            // Look for an 'Item' property, or a set of these with consistent return types 
            let allEquivReturnTypes minfo others = 
                let returnTy = FSharpReturnTyOfMeth cenv.amap m minfo []
                others |> List.forall (fun other -> type_equiv cenv.g (FSharpReturnTyOfMeth cenv.amap m other []) returnTy)
            
            let isInt32Indexer minfo = 
                match ParamTypesOfMethInfo cenv.amap m minfo [] with
                | [[ty]] -> 
                    // e.g. MatchCOlelction
                    type_equiv cenv.g cenv.g.int32_ty ty || 
                    // e.g. EnvDTE.Documents.Item
                    type_equiv cenv.g cenv.g.obj_ty ty
                | _ -> false
            
            match TryFindMethInfo cenv.infoReader m ad "get_Item" tyToSearchForGetEnumeratorAndItem with
            | (minfo :: others) when allEquivReturnTypes minfo others -> 

            //| (minfo :: others) when (allEquivReturnTypes minfo others &&
            //                          List.forall isInt32Indexer (minfo :: others)) -> 
                FSharpReturnTyOfMeth cenv.amap m minfo []
            
            | _ -> 
            
            // Some types such as XmlNodeList have only an Item method  
            match TryFindMethInfo cenv.infoReader m ad "Item" tyToSearchForGetEnumeratorAndItem with
            | (minfo :: others) when allEquivReturnTypes minfo others -> 
            //| (minfo :: others) when (allEquivReturnTypes minfo others &&
            //                          List.forall isInt32Indexer (minfo :: others)) -> 
                FSharpReturnTyOfMeth cenv.amap m minfo []
            
            | _ -> argty
        else 
            argty

    if verbose then  dprintf "argty = %s @ %a\n" (NicePrint.pretty_string_of_typ (empty_denv cenv.g) argty) output_range m;
    let ienumeratorv,ienumeratore = Tastops.mk_mut_compgen_local m "enumerator" retTypeOfGetEnumerator
    let getEnumE  ,getEnumTy  = BuildMethodCall cenv env PossiblyMutates   m false getEnumerator_minfo NormalValUse [] [exprToSearchForGetEnumeratorAndItem] []
    let guarde  ,guardty      = BuildMethodCall cenv env DefinitelyMutates m false moveNext_minfo      NormalValUse [] [ienumeratore] []
    let currente,currentty    = BuildMethodCall cenv env DefinitelyMutates m true get_Current_minfo   NormalValUse [] [ienumeratore] []
    let better_currente  = mk_coerce(currente,argty,range_of_expr currente,currentty)
    ienumeratorv, ienumeratore,retTypeOfGetEnumerator,argty,getEnumE,getEnumTy, guarde,guardty, better_currente
    
    
let ConvertArbitraryExprToEnumerable cenv ty (env:tcEnv) expr =
    let m = (range_of_expr expr)
    let argty = new_inference_typ cenv ()
    if (AddCxTypeMustSubsumeTypeUndoIfFailed env.DisplayEnv cenv.css m ( mk_seq_ty cenv.g argty) ty) then 
        expr,argty
    else          
        let enumv,enume = mk_compgen_local m "inputSequence" ty
        let ienumeratorv, _,retTypeOfGetEnumerator,argty,getEnumE,getEnumTy,guarde,guardty,better_currente = AnalyzeArbitraryExprAsEnumerable cenv env m ty enume
        if is_struct_typ cenv.g getEnumTy then errorR(Error("This expression has a method called GetEnumerator, but its return type is a value type. Methods returning struct enumerators cannot be used in this expression form",m));
        let getEnumE = mk_unit_delay_lambda cenv.g m getEnumE
        let expr = 
           mk_compgen_let m enumv expr (mk_call_seq_of_functions cenv.g m retTypeOfGetEnumerator argty getEnumE 
                                             (mk_lambda m ienumeratorv (guarde,guardty)) 
                                             (mk_lambda m ienumeratorv (better_currente,argty)))
        expr,argty           

let mk_seq_empty cenv env m genTy =
    (* We must discover the 'zero' of the monadic algebra being generated in order to compile failing matches. *)
    let genResultTy = new_inference_typ cenv ()
    unifyE cenv env m genTy (mk_seq_ty cenv.g genResultTy);
    mk_call_seq_empty cenv.g m genResultTy 

let mk_seq_map_concat cenv env m enumElemTy genTy lam enumExpr =
    let genResultTy = new_inference_typ cenv ()
    unifyE cenv env m genTy (mk_seq_ty cenv.g genResultTy);
    let enumExpr = mk_coerce_if_needed cenv.g (mk_seq_ty cenv.g enumElemTy) (type_of_expr cenv.g enumExpr) enumExpr
    mk_call_seq_map_concat cenv.g m enumElemTy genResultTy lam enumExpr

let mk_seq_using cenv (env:tcEnv) m resourceTy genTy resourceExpr lam =
    AddCxTypeMustSubsumeType env.DisplayEnv cenv.css m NoTrace cenv.g.system_IDisposable_typ resourceTy;
    let genResultTy = new_inference_typ cenv ()
    unifyE cenv  env m genTy (mk_seq_ty cenv.g genResultTy);
    mk_call_seq_using cenv.g m resourceTy genResultTy resourceExpr lam 

let mk_seq_delay cenv env m genTy lam =
    let genResultTy = new_inference_typ cenv ()
    unifyE cenv env m genTy (mk_seq_ty cenv.g genResultTy);
    mk_call_seq_delay cenv.g m genResultTy (mk_unit_delay_lambda cenv.g m lam) 


let mk_seq_append cenv env m genTy e1 e2 =
    let genResultTy = new_inference_typ cenv ()
    unifyE cenv env m genTy (mk_seq_ty cenv.g genResultTy);
    let e1 = mk_coerce_if_needed cenv.g (mk_seq_ty cenv.g genResultTy) (type_of_expr cenv.g e1) e1
    let e2 = mk_coerce_if_needed cenv.g (mk_seq_ty cenv.g genResultTy) (type_of_expr cenv.g e2) e2
    mk_call_seq_append cenv.g m genResultTy e1 e2 

let mk_seq_generated cenv env m genTy e1 e2 =
    let genResultTy = new_inference_typ cenv ()
    unifyE cenv env m genTy (mk_seq_ty cenv.g genResultTy);
    let e2 = mk_coerce_if_needed cenv.g (mk_seq_ty cenv.g genResultTy) (type_of_expr cenv.g e2) e2
    mk_call_seq_generated cenv.g m genResultTy e1 e2 

let mk_seq_finally cenv env m genTy e1 e2 =
    let genResultTy = new_inference_typ cenv ()
    unifyE cenv env m genTy (mk_seq_ty cenv.g genResultTy);
    let e1 = mk_coerce_if_needed cenv.g (mk_seq_ty cenv.g genResultTy) (type_of_expr cenv.g e1) e1
    mk_call_seq_finally cenv.g m genResultTy e1 e2 

let mk_monadic_match_clauses (pat',vspecs) innerExpr = 
    [ TClause(pat',None,TTarget(vspecs, innerExpr,SequencePointAtTarget),RangeOfPat pat') ]

let mk_seq_match_clauses cenv env (pat',vspecs) genTy innerExpr m = 
    [TClause(pat',None,TTarget(vspecs, innerExpr,SequencePointAtTarget),RangeOfPat pat') ] 

let conv_tcomp_match_clauses cenv env inputExprMark (pat',vspecs) innerExpr bindPatTy genInnerTy wholeExprMark = 
    let patMark = (RangeOfPat pat')
    let tclauses = mk_seq_match_clauses cenv env (pat',vspecs) genInnerTy innerExpr wholeExprMark
    CompilePatternForMatchClauses cenv env inputExprMark patMark false Incomplete bindPatTy genInnerTy tclauses 


let elim_fast_integer_for_loop (spBind,id,start,dir,finish,innerExpr,m) = 
    let pseudoEnumExpr = 
        if dir then mksyn_infix m m start ".." finish
        else  mksyn_trifix m ".. .." start (Expr_const(Const_int32 (-1),range_of_synexpr start)) finish
    Expr_foreach (spBind,SeqExprOnly(false),mksyn_pat_var None id,pseudoEnumExpr,innerExpr,m)

/// Determine if a syntactic expression inside 'seq { ... }' or '[...]' counts as a "simple sequence
/// of semicolon separated values". For example [1;2;3].
/// 'acceptDeprecated' is true for the '[ ... ]' case, where we allow the syntax '[ if g then t else e ]' but ask it to be parenthesized
///
let (|SimpleSemicolonSequence|_|) acceptDeprecated c = 

    let rec YieldFree expr = 
        match expr with 
        | Expr_seq (_,_,e1,e2,_) -> YieldFree e1 && YieldFree e2
        | Expr_cond (_,e2,e3opt,_,_,_) -> YieldFree e2 && Option.for_all YieldFree e3opt
        | Expr_try_catch (e1,_,clauses,_,_,_,_) -> YieldFree e1 && clauses |> List.forall (fun (Clause(_,_,e,_,_)) -> YieldFree e)
        | Expr_match (_,_,clauses,_,_) -> clauses |> List.forall (fun (Clause(_,_,e,_,_)) -> YieldFree e)
        | Expr_for (_,_,_,_,_,body,_) 
        | Expr_try_finally (body,_,_,_,_)
        | Expr_let (_,_,_,body,_) 
        | Expr_while (_,_,body,_) 
        | Expr_foreach (_,_,_,_,body,_) -> YieldFree body
        | Comp_yieldm _ 
        | Comp_yield _ 
        | Comp_bind _ 
        | Comp_zero _ 
        | Expr_do _ -> false
        | e -> true

    let rec IsSimpleSemicolonSequenceElement expr = 
        match expr with 
        | Expr_cond _ when acceptDeprecated && YieldFree expr -> true
        | Expr_cond _ 
        | Expr_try_catch _ 
        | Expr_match _ 
        | Expr_for _ 
        | Expr_foreach _ 
        | Expr_try_finally _ 
        | Comp_yieldm _ 
        | Comp_yield _ 
        | Expr_let _ 
        | Expr_do _ 
        | Comp_bind _ 
        | Comp_zero _ 
        | Expr_while _ -> false
        | e -> true

    let rec GetSimpleSemicolonSequenceOfComprehension expr acc = 
        match expr with 
        | Expr_seq(_,true,e1,e2,_) -> 
            if IsSimpleSemicolonSequenceElement e1 then 
                GetSimpleSemicolonSequenceOfComprehension e2 (e1::acc)
            else
                None 
        | e -> 
            if IsSimpleSemicolonSequenceElement e then 
                Some(List.rev (e::acc))
            else 
                None 

    if YieldFree c then 
        GetSimpleSemicolonSequenceOfComprehension c []
    else
        None


//-------------------------------------------------------------------------
// Post-transform initialization graphs using the 'lazy' interpretation.
// See ML workshop paper.
//------------------------------------------------------------------------- 

type InitializationGraphAnalysisState = 
    | Top
    | InnerTop
    | DefinitelyStrict
    | MaybeLazy
    | DefinitelyLazy

let EliminateInitializationGraphs g mustHaveArity denv fixupsAndBindingsWithoutLaziness bindsm =
    (* BEGIN INITIALIZATION GRAPHS *)
    (* Check for safety and determine if we need to insert lazy thunks *)
    let fixupsl,bindsWithoutLaziness = List.unzip fixupsAndBindingsWithoutLaziness
    let fixupsl : RecursiveUseFixupPoints list = fixupsl
    let rvs = bindsWithoutLaziness |> List.map (fun (TBind(v,_,_)) -> mk_local_vref v) 
    let outOfOrder = ref false
    let runtimeChecks = ref false
    let directRecursiveData = ref false
    let reportedEager = ref false
    let definiteDependencies = ref []
    let check availIfInOrder boundv expr = 
        let strict = function
            | MaybeLazy -> MaybeLazy
            | DefinitelyLazy -> DefinitelyLazy
            | Top | DefinitelyStrict | InnerTop -> DefinitelyStrict
        let lzy = function 
            | Top | InnerTop | DefinitelyLazy -> DefinitelyLazy 
            | MaybeLazy | DefinitelyStrict -> MaybeLazy
        let fixable = function 
            | Top | InnerTop -> InnerTop
            | DefinitelyStrict -> DefinitelyStrict
            | MaybeLazy -> MaybeLazy
            | DefinitelyLazy -> DefinitelyLazy

        let rec CheckExpr st e = 
            if verbose then  dprintf "--> CheckExpr@%a\n" output_range (range_of_expr e);
            match strip_expr e with 
              (* Expressions with some lazy parts *)
            | TExpr_lambda (_,_,_,b,_,_,_) | TExpr_tlambda (_,_,b,_,_,_) -> checkDelayed st b
            | TExpr_obj (_,ty,_,e,overrides,extraImpls,_,_) -> 
                (* NOTE: we can't fixup recursive references inside delegates since the closure delegee of a delegate is not accessible *)
                (* from outside. Object expressions implementing interfaces can, on the other hand, be fixed up. See FSharp 1.0 bug 1469 *)
                if is_interface_typ g ty (* || is_delegate_typ ty *) then 
                    List.iter (fun (TObjExprMethod(_,_,_,e,m)) ->  checkDelayed st e) overrides;
                    List.iter (snd >> List.iter (fun (TObjExprMethod(_,_,_,e,m)) ->  checkDelayed st e)) extraImpls;
                else 
                    CheckExpr (strict st) e;
                    List.iter (fun (TObjExprMethod(_,_,_,e,m)) ->  CheckExpr (lzy (strict st)) e) overrides;
                    List.iter (snd >> List.iter (fun (TObjExprMethod(_,_,_,e,m)) ->  CheckExpr (lzy (strict st)) e)) extraImpls;
                
              (* Expressions where fixups may be needed *)
            | TExpr_val (v,_,m) -> CheckValSpec st v m

              (* Expressions where subparts may be fixable *)
            | TExpr_op((TOp_tuple | TOp_ucase _ | TOp_recd _),_,args,_) -> 
                     (* REVIEW: ONLY FIXABLE WHEN (item_ref_in_this_assembly accessible_ccus tycon) *)
                List.iter (CheckExpr (fixable st)) args

              (* Composite expressions *)
            | TExpr_const _ -> ()
            | TExpr_letrec (binds,e,_,_)  ->
                binds |> FlatList.iter (CheckBinding (strict st)) ; 
                CheckExpr (strict st) e
            | TExpr_let (bind,e,_,_) ->  
                CheckBinding (strict st) bind; 
                CheckExpr (strict st) e
            | TExpr_match (_,_,pt,targets,_,_,_) -> 
                CheckDecisionTree (strict st) pt; 
                Array.iter (CheckDecisionTreeTarget (strict st)) targets 
            | TExpr_app(e1,_,_,args,_) -> 
                CheckExpr (strict st) e1;  
                List.iter (CheckExpr (strict st)) args 
          (* Binary expressions *)
            | TExpr_seq (e1,e2,_,_,_)
            | TExpr_static_optimization (_,e1,e2,_) ->
                 CheckExpr (strict st) e1;  CheckExpr (strict st) e2
          (* n-ary expressions *)
            | TExpr_op(op,_,args,m)  -> CheckExprOp st op m;  List.iter (CheckExpr (strict st)) args
          (* misc *)
            | TExpr_link(eref) -> CheckExpr st !eref
            | TExpr_tchoose (_,b,_)  -> CheckExpr st b
            | TExpr_quote _  -> ()

        and CheckMethod st (TObjExprMethod(_,_,_,e,m)) =  CheckExpr (lzy (strict st)) e
        and CheckBinding st (TBind(v,e,_)) = CheckExpr st e 
        and CheckDecisionTree st = function
            | TDSwitch(e1,csl,dflt,_) -> CheckExpr st e1; List.iter (fun (TCase(_,d)) -> CheckDecisionTree st d) csl; Option.iter (CheckDecisionTree st) dflt
            | TDSuccess (es,n) -> es |> FlatList.iter (CheckExpr st) 
            | TDBind(bind,e) -> CheckBinding st bind; CheckDecisionTree st e
        and CheckDecisionTreeTarget st (TTarget(vs,e,_)) = CheckExpr st e

        and CheckExprOp st op m = 
            match op with 
            | TOp_lval_op (kind,lvr) -> CheckValSpec (strict st) lvr m
            | _ -> ()
          
        and CheckValSpec st v m = 
            match st with 
            | MaybeLazy -> 
                if ListSet.mem g.vref_eq v rvs then 
                    warning (RecursiveUseCheckedAtRuntime (denv,v,m)); 
                    if not !reportedEager then 
                      (warning (LetRecCheckedAtRuntime m); reportedEager := true);
                    runtimeChecks := true;

            | Top | DefinitelyStrict ->
                if ListSet.mem g.vref_eq v rvs then 
                    if not (ListSet.mem g.vref_eq v availIfInOrder) then 
                        warning (LetRecEvaluatedOutOfOrder (denv,boundv,v,m)); 
                        outOfOrder := true;
                        if not !reportedEager then 
                          (warning (LetRecCheckedAtRuntime m); reportedEager := true);
                    definiteDependencies := (boundv,v) :: !definiteDependencies
            | InnerTop -> 
                if ListSet.mem g.vref_eq v rvs then 
                    directRecursiveData := true
            | DefinitelyLazy -> () 
        and checkDelayed st b = 
            match st with 
            | MaybeLazy | DefinitelyStrict -> CheckExpr MaybeLazy b
            | DefinitelyLazy | Top | InnerTop -> () 
          
       
        CheckExpr Top expr
   

    List.fold 
         (fun availIfInOrder (TBind(v,e,_)) -> 
           check availIfInOrder (mk_local_vref v) e; 
           (mk_local_vref v::availIfInOrder))
         [] bindsWithoutLaziness |> ignore;
    
     (* ddg = definiteDependencyGraph *)
    let ddgNodes = bindsWithoutLaziness |> List.map (fun (TBind(v,_,_)) -> mk_local_vref v) 
    let ddg = NodeGraph.mk_graph ((fun (v:ValRef) -> v.Stamp), Int64.order) ddgNodes (!definiteDependencies |> List.map (fun (v1,v2) -> v1.Stamp, v2.Stamp))
    ddg |> NodeGraph.iterate_cycles (fun path -> error (LetRecUnsound (denv,path,path.Head.Range))) ;

    let requiresLazyBindings = !runtimeChecks || !outOfOrder
    if !directRecursiveData && requiresLazyBindings then 
        error(Error("This recursive binding uses an invalid mixture of recursive forms",bindsm));

    let bindsBefore, bindsAfter = 
      if requiresLazyBindings then 
          let bindsBeforeL, bindsAfterL = 
            
              (fixupsl, bindsWithoutLaziness) 
              ||> List.map2 (fun (RecursiveUseFixupPoints(fixupPoints)) (TBind(v,e,seqPtOpt)) -> 
                   match strip_expr e with
                   | TExpr_lambda _ | TExpr_tlambda _ -> 
                       [mk_invisible_bind v e],[] 
                   | _ -> 
                       if verbose then  dprintf "value '%s' will use lazy thunks and runtime checks\n" v.MangledName;
                       let ty = v.Type
                       let m = v.Range
                       let vty = (mk_lazy_ty g ty)

                       let fty = (g.unit_ty --> ty)
                       let flazy,felazy = Tastops.mk_compgen_local m  v.MangledName fty 
                       let frhs = mk_unit_delay_lambda g m e
                       if mustHaveArity then flazy.Data.val_top_repr_info <- Some(InferArityOfExpr g fty [] [] frhs);

                       let vlazy,velazy = Tastops.mk_compgen_local m  v.MangledName vty 
                       let vrhs = (mk_lazy_delayed g m ty felazy)
                       
                       if mustHaveArity then vlazy.Data.val_top_repr_info <- Some(InferArityOfExpr g vty [] [] vrhs);
                       fixupPoints |> List.iter (fun (fp,m) -> fp := mk_lazy_force g (range_of_expr !fp) ty velazy);

                       [mk_invisible_bind flazy frhs; mk_invisible_bind vlazy vrhs],
                       [mk_bind seqPtOpt v (mk_lazy_force g m ty velazy)])
               |> List.unzip
          List.concat bindsBeforeL, List.concat bindsAfterL
      else
          bindsWithoutLaziness,[]
    bindsBefore @ bindsAfter

//-------------------------------------------------------------------------
// Check the shape of an object constructor and rewrite calls 
//------------------------------------------------------------------------- 

let CheckAndRewriteObjectCtor g env ctorLambaExpr =

    let m = range_of_expr ctorLambaExpr
    let tps,vsl,body,returnTy = dest_top_lambda (ctorLambaExpr,type_of_expr g ctorLambaExpr)

    // Rewrite legitimate self-construction calls to CtorValUsedAsSelfInit 
    let error(expr) = 
        errorR(Error(sprintf "This is not a valid object construction expression. Object constructors can only be implemented by a limited range of expressions",range_of_expr expr));
        expr

    // Build an assignment into the mutable reference cell that holds recursive references to 'this' 
    let rewriteContruction recdExpr = 
       match env.eCtorInfo with 
       | None -> recdExpr
       | Some ctorInfo -> 
           match ctorInfo.ctorThisRefCellVarOpt with 
           | None -> recdExpr
           | Some vref -> 
               let ty = type_of_expr g recdExpr
               TExpr_seq(recdExpr,(mk_refcell_set g  m ty (expr_for_vref m vref) (mk_ldarg0 m ty)),ThenDoSeq,SuppressSequencePointOnExprOfSequential,m)

    let rec checkAndRewrite expr = 
        match expr with 
        (* <ctor-body> = { fields } *)
        // The constructor ends in an object initialization expression - good 
        | TExpr_op(TOp_recd(RecdExprIsObjInit,_),_,_,_) -> rewriteContruction expr

        // <ctor-body> = "a; <ctor-body>" 
        | TExpr_seq(a,body,NormalSeq,spSeq,b)  -> TExpr_seq(a,checkAndRewrite body,NormalSeq,spSeq,b) 

        // <ctor-body> = "<ctor-body> then <expr>" 
        | TExpr_seq(body,a,ThenDoSeq,spSeq,b) -> TExpr_seq(checkAndRewrite body,a,ThenDoSeq,spSeq,b)

        // <ctor-body> = "let pat = expr in <ctor-body>" 
        | TExpr_let(bind,body,m,_)  -> mk_let_bind m bind (checkAndRewrite body)

        // The constructor is a sequence "let pat = expr in <ctor-body>" 
        | TExpr_match(spBind,a,b,targets,c,d,e)  -> TExpr_match(spBind,a,b, (targets |> Array.map (fun (TTarget(vs,body,spTarget)) -> TTarget(vs, checkAndRewrite body,spTarget))),c,d,e)

        // <ctor-body> = "let rec binds in <ctor-body>" 
        | TExpr_letrec(a,body,_,b) -> TExpr_letrec (a,checkAndRewrite body ,m,NewFreeVarsCache())

        // <ctor-body> = "new C(...)" 
        | TExpr_app(f,b,c,d,m) -> 
            // The application had better be an application of a ctor 
            let f = checkAndRewriteCtorUsage f
            let expr = TExpr_app(f,b,c,d,m)
            rewriteContruction expr 

        | _ -> 
            error(expr)

    and checkAndRewriteCtorUsage expr = 
         match expr with 
         | TExpr_link eref -> 
               let e = checkAndRewriteCtorUsage !eref
               eref := e;
               expr
               
         // Type applications are ok, e.g. 
         //     type C<'a>(x:int) = 
         //         new() = C<'a>(3) 
         | TExpr_app(f,fty,tyargs,[],m) -> 
             let f = checkAndRewriteCtorUsage f
             TExpr_app(f,fty,tyargs,[],m)

         // Self-calls are OK and get rewritten. 
         | TExpr_val(vref,NormalValUse,a) ->
           let isCtor = 
               match vref.MemberInfo with 
               | None -> false
               | Some(memberInfo) -> (memberInfo.MemberFlags.MemberKind = MemberKindConstructor)

           if not isCtor then error(expr) else
           TExpr_val(vref,CtorValUsedAsSelfInit,a)
         | _ -> 
            error(expr)
    
    let body = checkAndRewrite body
    mk_multi_lambdas m tps vsl (body, returnTy) 
    


/// Post-typechecking normalizations to enforce semantic constraints
/// lazy and, lazy or, rethrow, address-of
let build_app cenv expr exprty arg m = 
    match expr,arg with        
    | ApplicableExpr(_, TExpr_app(TExpr_val(vf,_,_),_,_,[x0],_)) , _ 
         when cenv.g.vref_eq vf cenv.g.and_vref 
           || cenv.g.vref_eq vf cenv.g.and2_vref  -> 
        MkApplicableExprNoFlex cenv (mk_lazy_and cenv.g m x0 arg)
    | ApplicableExpr(_, TExpr_app(TExpr_val(vf,_,_),_,_,[x0],_)), _ 
         when cenv.g.vref_eq vf cenv.g.or_vref
           || cenv.g.vref_eq vf cenv.g.or2_vref -> 
        MkApplicableExprNoFlex cenv (mk_lazy_or cenv.g m x0 arg )
    | ApplicableExpr(_, TExpr_app(TExpr_val(vf,_,_),_,_,[],_)), _ 
         when cenv.g.vref_eq vf cenv.g.rethrow_vref -> 
        // exprty is of type: "unit -> 'a". Break it and store the 'a type here, used later as return type. 
        let _unit_ty,rtn_ty = dest_fun_typ cenv.g exprty 
        MkApplicableExprNoFlex cenv (mk_compgen_seq m arg (mk_rethrow m rtn_ty))
    | ApplicableExpr(_, TExpr_app(TExpr_val(vf,_,_),_,_,[],_)), _ 
         when (cenv.g.vref_eq vf cenv.g.addrof_vref || 
               cenv.g.vref_eq vf cenv.g.addrof2_vref) -> 
        if cenv.g.vref_eq vf cenv.g.addrof2_vref then warning(UseOfAddressOfOperator(m));
        let wrap,e1a' = mk_expra_of_expr cenv.g true DefinitelyMutates arg m 
        MkApplicableExprNoFlex cenv (wrap(e1a'))
    | _ -> 
        expr.SupplyArgument(arg,m) 
            

//-------------------------------------------------------------------------
// Additional data structures used by type checking
//------------------------------------------------------------------------- 

type DelayedItem = 
  | DelayedTypeApp of Ast.SynType list * Range.range
  | DelayedApp of ExprAtomicFlag * Ast.SynExpr * Range.range
  | DelayedDotLookup of Ast.ident list * Range.range
  | DelayedSet of Ast.SynExpr * Range.range

let MakeDelayedSet(e,m) = 
    // We have lid <- e. Wrap 'e' in another pair of parentheses to ensure it's never interpreted as 
    // a named argument, e.g. for "el.Checked <- (el = el2)" 
    DelayedSet (Expr_paren (e, range_of_synexpr e), m)

type NewSlotsOK = 
    | NewSlotsOK 
    | NoNewSlots


type ImplictlyBoundTyparsAllowed = 
    | NewTyparsOKButWarnIfNotRigid 
    | NewTyparsOK 
    | NoNewTypars

type CheckConstraints = 
    | CheckCxs 
    | NoCheckCxs

type TypeRealizationPass = 
    | FirstPass 
    | SecondPass 

/// Provides information about the context for a value or member definition 
type ContainerInfo = 
    | ContainerInfo of 
                                      (*parent:*)
                                      ParentRef *  // The nearest containing module. Used as the 'actual' parent for extension members and values 
                                      // For members:
                                      Option<(TyconRef *                        // tcref: The logical apparent parent of a value/member, either a module, type or exception 
                                              (Tast.typ * SlotImplSet) option * // optIntfSlotTy
                                              Val option *                      // optBaseVal
                                              typars                            // declaredTyconTypars
                                             )>
    member x.ParentRef = (let (ContainerInfo(v,_)) = x in v)
    
/// Indicates a declaration is contained in an expression 
let ExprContainerInfo = ContainerInfo(ParentNone,None)
/// Indicates a declaration is contained in the given module 
let ModuleOrNamespaceContainerInfo modref = ContainerInfo(Parent(modref),Some(modref,None,None,[]))
/// Indicates a declaration is contained in the given type definition in the given module 
let TyconContainerInfo (parent,tcref,declaredTyconTypars) = ContainerInfo(parent,Some(tcref,None,None,declaredTyconTypars))

type RecursiveBindingDefnInfo = RecBindingDefn of ContainerInfo * NewSlotsOK * DeclKind * SynBinding

type NormalizedRecBindingDefn = NormalizedRecBindingDefn of ContainerInfo * NewSlotsOK * DeclKind * NormalizedBinding

type TyconBindingDefn = TyconBindingDefn of ContainerInfo * NewSlotsOK * DeclKind * SynClassMemberDefn * range

type TyconBindingDefns = TyconBindingDefns of TyconRef * TyconBindingDefn list

type TyconMemberData = TyconMemberData of DeclKind * TyconRef * Val option * typars * SynClassMemberDefn list * range * NewSlotsOK

type ValSpecResult = ValSpecResult of ParentRef * ValMemberInfo option * ident * typars * typars * Tast.typ * PartialTopValInfo * DeclKind 


/// RecursiveBindingInfo - flows through initial steps of TcLetrec 
type RecursiveBindingInfo =
    RBInfo of
      ContainerInfo * 
      typars * 
      ValInlineInfo * 
      Val * 
      ExplicitTyparInfo * 
      PartialTopValInfo * 
      ValMemberInfo option  * 
      Val option * 
      Val option * 
      access option * 
      Tast.typ * 
      DeclKind


/// Check specifications of contraints on type parameters 
let rec TcTyparConstraint ridx cenv newOk checkCxs env tpenv c = 
    match c with 
    | WhereTyparEqualsType(tp,ty,m) ->
       let ty',tpenv = TcTypeAndRecover cenv newOk checkCxs env tpenv ty
       let tp',tpenv = TcTypar cenv env newOk tpenv tp
       if (newOk = NoNewTypars) then errorR(Error("invalid constraint",m));
       unifyE cenv env m (mk_typar_ty tp') ty';
       tpenv

    | WhereTyparDefaultsToType(tp,ty,m) ->
       let ty',tpenv = TcTypeAndRecover cenv newOk checkCxs env tpenv ty
       let tp',tpenv = TcTypar cenv env newOk tpenv tp
       let csenv = (MakeConstraintSolverEnv cenv.css m env.DisplayEnv)
       AddConstraint csenv 0 m NoTrace tp' (TTyparDefaultsToType(ridx,ty',m)) |> CommitOperationResult;
       tpenv

    | WhereTyparSubtypeOfType(tp,ty,m) ->
       let ty',tpenv = TcTypeAndRecover cenv newOk checkCxs env tpenv ty
       let tp',tpenv = TcTypar cenv env newOk tpenv tp
       if (newOk = NoNewTypars) && is_sealed_typ cenv.g ty' then 
           errorR(Error("invaid constraint: the type used for the constraint is sealed, which means the constraint could only be satisfied by at most one solution",m));
       AddCxTypeMustSubsumeType env.DisplayEnv cenv.css m NoTrace  ty' (mk_typar_ty tp') ;
       tpenv

    | WhereTyparSupportsNull(tp,m) ->
       let tp',tpenv = TcTypar cenv env newOk tpenv tp
       AddCxTypeMustSupportNull env.DisplayEnv cenv.css m NoTrace (mk_typar_ty tp') ;
       tpenv

    | WhereTyparIsReferenceType(tp,m) ->
       let tp',tpenv = TcTypar cenv env newOk tpenv tp
       AddCxTypeIsReferenceType env.DisplayEnv cenv.css m NoTrace (mk_typar_ty tp') ;
       tpenv

    | WhereTyparIsValueType(tp,m) ->
       let tp',tpenv = TcTypar cenv env newOk tpenv tp
       AddCxTypeIsValueType env.DisplayEnv cenv.css m NoTrace (mk_typar_ty tp') ;
       tpenv

    | WhereTyparIsEnum(tp,tyargs,m) ->
       let tp',tpenv = TcTypar cenv env newOk tpenv tp
       match tyargs with 
       | [underlying] -> 
           let underlying',tpenv = TcTypeAndRecover cenv newOk checkCxs env tpenv underlying
           AddCxTypeIsEnum env.DisplayEnv cenv.css m NoTrace (mk_typar_ty tp') underlying';
       | _ -> errorR(Error("an 'enum' constraint must be of the form 'enum<type>'",m));
       tpenv

    | WhereTyparIsDelegate(tp,tyargs,m) ->
       let tp',tpenv = TcTypar cenv env newOk tpenv tp
       match tyargs with 
       | [a;b] -> 
           let a',tpenv = TcTypeAndRecover cenv newOk checkCxs env tpenv a
           let b',tpenv = TcTypeAndRecover cenv newOk checkCxs env tpenv b
           AddCxTypeIsDelegate env.DisplayEnv cenv.css m NoTrace (mk_typar_ty tp') a' b';
           tpenv
       | _ -> 
           errorR(Error("an 'enum' constraint must be of the form 'enum<type>'",m));
           tpenv

    | WhereTyparSupportsMember(tps,memSpfn,m) ->
        let tps',tpenv = List.mapfold (TcTypar cenv env newOk) tpenv tps
        let traitInfo,tpenv = TcPseudoMemberSpec cenv newOk checkCxs env tps tpenv memSpfn m
        match traitInfo with 
        | TTrait(objtys,".ctor",memberFlags,argtys,returnTy,_) when (memberFlags.MemberKind=MemberKindConstructor) ->
            match objtys,argtys with 
            | [ty],[] when type_equiv cenv.g ty (GetFSharpViewOfReturnType cenv.g returnTy) ->
                AddCxTypeMustSupportDefaultCtor env.DisplayEnv cenv.css m NoTrace ty ;
                tpenv
            | _ ->            
                errorR(Error("'new' constraints must take one argument of type 'unit' and return the constructed type",m));
                tpenv
        | _ ->  
            AddCxMethodConstraint env.DisplayEnv cenv.css m NoTrace traitInfo;
            tpenv
      
and TcPseudoMemberSpec cenv newOk checkCxs env tps tpenv memSpfn m = 
    let tps',tpenv = List.mapfold (TcTypar cenv env newOk) tpenv tps
    let tys = List.map mk_typar_ty tps'
    match memSpfn with 
    | ClassMemberSpfn_binding (valSpfn,memberFlags,m) ->
        let members,tpenv = TcValSpec cenv env ModuleOrMemberBinding (ExprContainerInfo) (Some memberFlags) (Some (List.hd tys)) tpenv valSpfn []
        match members with 
        | [ValSpecResult(_,_,id,_,_,ty',partialTopValInfo,_)] -> 
            (* REVIEW: Test pseudo constraints cannot refer to polymorphic methods, *)
            let tps,_ = try_dest_forall_typ cenv.g ty'
            let topValInfo = TranslatePartialArity tps partialTopValInfo
            let tps,curriedArgInfos,returnTy,_ = GetTopValTypeInCompiledForm cenv.g topValInfo ty' m
            let argtys = List.concat curriedArgInfos
            let argtys = List.map fst argtys
            let logicalCompiledName = ComputeLogicalCompiledName id memberFlags
            TTrait(tys,logicalCompiledName,memberFlags,argtys,returnTy, ref None),tpenv
        | _ -> error(Error("This constraint is invalid",m))
    | _ -> error(Error("This constraint is invalid",m))


/// Check a value specification, e.g. in a signature or a constraint
and TcValSpec cenv env declKind containerInfo memFlagsOpt thisTyOpt tpenv (ValSpfn(attrs,id,SynValTyparDecls(synTypars,_,synTyparConstraints), ty, valSynInfo, _,mutableFlag,doc, vis,literalExprOpt,m)) attrs' =
    (*printf "TcValSpec: id=%s\n" id.idText;*)
    let declaredTypars = TcTyparDecls cenv env synTypars
    let (ContainerInfo(altActualParent,tcrefContainerInfo)) = containerInfo
    let enclosingDeclaredTypars,memberContainerInfo,thisTyOpt,declKind = 
        match tcrefContainerInfo with 
        | Some(tcref,_,_,declaredTyconTypars) -> 
            let isExtrinsic = (declKind = ExtrinsicExtensionBinding)
            let _,enclosingDeclaredTypars,_,_,thisTy = FreshenObjectArgType cenv m TyparRigid tcref isExtrinsic declaredTyconTypars
            // An implemented interface type is in terms of the type's type parameters. 
            // We need a signature in terms of the values' type parameters. 
            // let optIntfSlotTy = Option.map (InstType renaming) optIntfSlotTy in  
            enclosingDeclaredTypars,Some(tcref),Some(thisTy),declKind
        | None -> 
            [],None,thisTyOpt, ModuleOrMemberBinding
    let allDeclaredTypars = (enclosingDeclaredTypars@declaredTypars)
    let envinner = AddDeclaredTypars NoCheckForDuplicateTypars allDeclaredTypars env
    let newOk = NewTyparsOK
    let checkCxs = CheckCxs
    let tpenv = TcTyparConstraints cenv newOk checkCxs envinner tpenv synTyparConstraints
    // Process the type, including any constraints 
    // REVIEW: check full ramifications of allowing NewTyparsOK here 
    let declaredTy,tpenv = TcTypeAndRecover cenv newOk checkCxs envinner tpenv ty  

    // Enforce "no more constraints allowed on declared typars "
    allDeclaredTypars |> List.iter (SetTyparRigid cenv.g env.DisplayEnv m);
    
    match memFlagsOpt,thisTyOpt with 
    | Some(memberFlags),Some(thisTy) -> 
        let generateOneMember(memberFlags) = 
        
            // Decode members in the signature
            let ty',valSynInfo = 
                match memberFlags.MemberKind with 
                | MemberKindClassConstructor
                | MemberKindConstructor
                | MemberKindMember -> 
                    declaredTy,valSynInfo
                | MemberKindPropertyGet 
                | MemberKindPropertySet ->  
                    let fakeTopArgInfos = [ for n in SynInfo.AritiesOfArgs valSynInfo do yield [ for i in 1 .. n do yield TopValInfo.unnamedTopArg1 ] ]
                    let arginfos,returnTy = GetTopTauTypeInFSharpForm cenv.g fakeTopArgInfos declaredTy m
                    if arginfos.Length > 1 then error(Error("This property has an invalid type. Properties taking multiple indexer arguments should have types of the form 'ty1 * ty2 -> ty3'. Properties returning functions should have types of the form '(ty1 -> ty2)'",m))
                    match memberFlags.MemberKind with 
                    | MemberKindPropertyGet ->
                        if SynInfo.HasNoArgs valSynInfo then 
                          (cenv.g.unit_ty --> declaredTy), (SynInfo.IncorporateEmptyTupledArg valSynInfo)
                        else
                          declaredTy,valSynInfo
                    | _ -> 
                        let setterTy = (mk_tupled_ty cenv.g (List.map fst (List.concat arginfos) @ [returnTy]) --> cenv.g.unit_ty)
                        let synInfo = SynInfo.IncorporateSetterArg valSynInfo
                        setterTy, synInfo
                | MemberKindPropertyGetSet -> 
                    error(InternalError("Unexpected MemberKindPropertyGetSet from signature parsing",m))

            // Take "unit" into account in the signature
            let valSynInfo = AdjustValSynInfoInSignature cenv.g ty' valSynInfo

            let ty',valSynInfo = 
                if memberFlags.MemberIsInstance then 
                  (thisTy --> ty'), (SynInfo.IncorporateSelfArg valSynInfo)
                else
                  ty',valSynInfo

            let reallyGenerateOneMember(id,valSynInfo,ty',memberFlags) = 
                let (PartialTopValInfo(argsData,retData)) as partialTopValInfo = 
                    TranslateTopValSynInfo m (TcAttributes cenv env) valSynInfo


                // Fold in the optional arugment information 
                // Resort to using the syntactic arugment information since that is what tells us 
                // what is optional and what is not. 
                let ty' = 

                    if SynInfo.HasOptionalArgs valSynInfo then 
                        let argtysl,returnTy = GetTopTauTypeInFSharpForm cenv.g argsData ty' m
                        let argtysl = 
                            (List.zip (List.mapSquared fst argtysl) valSynInfo.ArgInfos) 
                            |> List.map (fun (argtys,argInfos) ->
                                 (List.zip argtys argInfos)
                                 |> List.map (fun (argty,argInfo) ->
                                     if SynInfo.IsOptionalArg argInfo then mk_option_ty cenv.g argty
                                     else argty))
                        mk_iterated_fun_ty (List.map (mk_tupled_ty cenv.g) argtysl) returnTy
                    else ty' 
                        
                let memberInfoOpt,id = 
                    match memberContainerInfo with 
                    | Some(tcref) -> 
                        let isExtrinsic = (declKind = ExtrinsicExtensionBinding)
                        let memberInfo,id = MkMemberDataAndUniqueId(cenv.g,tcref,isExtrinsic,attrs',[],memberFlags,valSynInfo,id)
                        Some(memberInfo),id
                    | None -> 
                        None,id
            
                ValSpecResult(altActualParent,memberInfoOpt,id,enclosingDeclaredTypars,declaredTypars,ty',partialTopValInfo,declKind)

            [ yield reallyGenerateOneMember(id,valSynInfo,ty',memberFlags)
              if CompileAsEvent cenv.g attrs' then 
                    let valSynInfo = EventDeclarationNormalization.ConvertSynInfo m valSynInfo
                    let memberFlags = EventDeclarationNormalization.ConvertMemberFlags memberFlags
                    let delTy = FindDelegateTypeOfPropertyEvent cenv.g cenv.amap id.idText m declaredTy 
                    let ty = 
                       if memberFlags.MemberIsInstance then 
                         thisTy --> (delTy --> cenv.g.unit_ty)
                       else 
                         (delTy --> cenv.g.unit_ty)
                    yield reallyGenerateOneMember(ident("add_"^id.idText,id.idRange),valSynInfo,ty,memberFlags)
                    yield reallyGenerateOneMember(ident("remove_"^id.idText,id.idRange),valSynInfo,ty,memberFlags) ]
                
              
            
        match memberFlags.MemberKind with 
        | MemberKindClassConstructor
        | MemberKindConstructor
        | MemberKindMember 
        | MemberKindPropertyGet 
        | MemberKindPropertySet ->
            generateOneMember(memberFlags), tpenv
        | MemberKindPropertyGetSet ->
            [ yield! generateOneMember({memberFlags with MemberKind=MemberKindPropertyGet});
              yield! generateOneMember({memberFlags with MemberKind=MemberKindPropertySet}); ], tpenv
    | _ ->
        let valSynInfo = AdjustValSynInfoInSignature cenv.g declaredTy valSynInfo
        let partialTopValInfo = TranslateTopValSynInfo m (TcAttributes cenv env) valSynInfo
        [ ValSpecResult(altActualParent,None,id,enclosingDeclaredTypars,declaredTypars,declaredTy,partialTopValInfo,declKind) ], tpenv


(*-------------------------------------------------------------------------
 * Bind types 
 *------------------------------------------------------------------------- *)

/// Check and elaborate a type or measure parameter occurrence
/// If optKind=Some kind, then this is the kind we're expecting (we're in *analysis* mode)
/// If optKind=None, we need to determine the kind (we're in *synthesis* mode)
///
and TcTyparOrMeasurePar optKind cenv env newOk tpenv (Typar(id,_,_) as tp) =
    let checkRes (res:Typar) =
        match optKind, res.Kind with
        | Some KindMeasure, KindType -> error (Error("Expected unit-of-measure parameter, not type parameter. Explicit unit-of-measure parameters must be marked with the [<Measure>] attribute", id.idRange)); res, tpenv
        | Some KindType, KindMeasure -> error (Error("Expected type parameter, not unit-of-measure parameter", id.idRange)); res, tpenv
        | _, _ -> res, tpenv
    let key = id.idText
    match Map.tryfind key env.eNameResEnv.eTypars with
    | Some res -> checkRes res
    | None -> 
    match TryFindUnscopedTypar key tpenv with
    | Some res -> checkRes res
    | None -> 
        if newOk = NoNewTypars then error (UndefinedName(0,"type parameter",id,["<unimplemented>"]));
        // OK, this is an implicit declaration of a type parameter 
        // The kind defaults to Type
        let tp' = NewTypar ((match optKind with None -> KindType | Some kind -> kind), TyparWarnIfNotRigid,tp,false,DynamicReq,[])
        tp',AddUnscopedTypar key tp' tpenv

and TcTypar cenv env newOk tpenv tp =
    TcTyparOrMeasurePar (Some KindType) cenv env newOk tpenv tp

and TcTyparDecl cenv env (TyparDecl(attrs,(Typar(id,_,_) as tp))) =
    let attrs' = TcAttributes cenv env attrTgtGenericParameter  attrs
    let hasMeasureAttr = HasAttrib cenv.g cenv.g.attrib_MeasureAttribute attrs'
    let attrs' = attrs' |> List.filter (IsMatchingAttrib cenv.g cenv.g.attrib_MeasureAttribute >> not)
    NewTypar ((if hasMeasureAttr then KindMeasure else KindType), TyparWarnIfNotRigid,tp,false,DynamicReq,attrs')

and TcTyparDecls cenv env synTypars = List.map (TcTyparDecl cenv env) synTypars

/// Check and elaborate a syntactic type or measure
/// If optKind=Some kind, then this is the kind we're expecting (we're in *analysis* mode)
/// If optKind=None, we need to determine the kind (we're in *synthesis* mode)
///
and TcTypeOrMeasure optKind cenv newOk checkCxs env (tpenv:SyntacticUnscopedTyparEnv) ty =
    if verbose then  dprintf "--> tc_t ype@%a\n" output_range (range_of_syntype ty); 

    match ty with 
    | Type_lid(tc,m) -> 
        let ad = AccessRightsOfEnv env
        let tcref = ForceRaise(ResolveTypeLongIdent cenv.nameResolver ItemOccurence.Use OpenQualified env.eNameResEnv ad tc 0)
        //if tcref_eq cenv.g tcref cenv.g.system_IndexOutOfRangeException_tcref then 
        //    warning(IndexOutOfRangeExceptionWarning(m)); 
        match optKind, tcref.TypeOrMeasureKind with
        | Some KindType, KindMeasure ->
          error(Error("Expected type, not unit-of-measure", m)); 
          new_error_typ cenv (), tpenv
        | Some KindMeasure, KindType ->
          error(Error("Expected unit-of-measure, not type", m)); 
          TType_measure (new_error_measure cenv ()), tpenv
        | _, KindMeasure ->
          TType_measure (MeasureCon tcref), tpenv
        | _, KindType ->
          TcTypeApp cenv newOk checkCxs env tpenv m tcref [] []

    | Type_app (Type_lid(tc,_),args,postfix,m) -> 
        let ad = AccessRightsOfEnv env
        let tcref = ForceRaise(ResolveTypeLongIdent cenv.nameResolver ItemOccurence.Use OpenQualified env.eNameResEnv ad tc (List.length args))
        //if tcref_eq cenv.g tcref cenv.g.system_IndexOutOfRangeException_tcref then 
        //    warning(IndexOutOfRangeExceptionWarning(m)); 
        match optKind, tcref.TypeOrMeasureKind with
        | Some KindType, KindMeasure ->
          error(Error("Expected type, not unit-of-measure", m)); 
          new_error_typ cenv (), tpenv
        | Some KindMeasure, KindType ->
          error(Error("Expected unit-of-measure, not type", m)); 
          TType_measure (new_error_measure cenv ()), tpenv
        | _, KindType ->
          if postfix && tcref.Typars(m) |> List.exists (fun tp -> match tp.Kind with KindMeasure -> true | _ -> false) 
          then error(Error("Units-of-measure cannot be used as prefix arguments to a type. Rewrite as postfix arguments in angle brackets", m));
          TcTypeApp cenv newOk checkCxs env tpenv m tcref [] args 
        | _, KindMeasure ->
          match args,postfix with
            [arg], true ->
            let ms,tpenv = TcMeasure cenv newOk checkCxs env tpenv arg m
            TType_measure (MeasureProd(MeasureCon tcref, ms)), tpenv
            
          | _, _ ->
            errorR(Error("Unit-of-measure cannot be used in type constructor application", m));
            new_error_typ cenv (), tpenv          

    | Type_proj_then_app (ltyp,lid,args,m) -> 
        let ad = AccessRightsOfEnv env
        let ltyp,tpenv = TcType cenv newOk checkCxs env tpenv ltyp
        if not (is_stripped_tyapp_typ cenv.g ltyp) then error(Error("This type has no nested types",m));
        let tcref,tinst = dest_stripped_tyapp_typ cenv.g ltyp
        let tcref = ResolveTypeLongIdentInType cenv.nameResolver env.eNameResEnv (ResolveTypeNamesToTypeRefs,Some(List.length args)) ad m tcref lid 
        TcTypeApp cenv newOk checkCxs env tpenv m tcref tinst args 

    | Type_tuple(args,m) ->
        match optKind with
          Some KindMeasure ->
          let ms,tpenv = TcMeasuresAsTuple cenv newOk checkCxs env tpenv args m
          TType_measure ms,tpenv

        | _ ->
          let args',tpenv = TcTypesAsTuple cenv newOk checkCxs env tpenv args m
          TType_tuple(args'),tpenv

    | Type_fun(domainTy,resultTy,m) -> 
        let domainTy',tpenv = TcTypeAndRecover cenv newOk checkCxs env tpenv domainTy
        let resultTy',tpenv = TcTypeAndRecover cenv newOk checkCxs env tpenv resultTy
        (domainTy' --> resultTy'), tpenv

    // Deprecated OCaml-compat generic record field type
    | Type_forall (td,resultTy,m) -> 
        (* note: no constraints allowed here *)
        let tp' = TcTyparDecl cenv env td
        SetTyparRigid cenv.g env.DisplayEnv m tp';
        let resultTy',tpenv = TcTypeAndRecover cenv newOk checkCxs (AddDeclaredTypars NoCheckForDuplicateTypars [tp'] env) tpenv resultTy
        ([tp'] +-> resultTy'), tpenv

    | Type_arr (n,elemTy,m) -> 
        let elemTy,tpenv = TcTypeAndRecover cenv newOk checkCxs env tpenv elemTy
        mk_multi_dim_array_typ cenv.g n elemTy, tpenv

    | Type_lazy (elemTy,m) -> 
        let elemTy,tpenv = TcTypeAndRecover cenv newOk checkCxs env tpenv elemTy
        mk_lazy_ty cenv.g elemTy, tpenv

    | Type_var (tp,m) -> 
        let tp',tpenv = TcTyparOrMeasurePar optKind cenv env newOk tpenv tp
        match tp'.Kind with
        | KindMeasure -> TType_measure (MeasureVar tp'), tpenv
        | KindType -> mk_typar_ty tp',tpenv

    | Type_anon m ->           (* _ *)
        let tp:Typar = TcAnonTypeOrMeasure optKind cenv TyparAnon NoDynamicReq newOk m
        match tp.Kind with
        | KindMeasure -> TType_measure (MeasureVar tp), tpenv
        | KindType -> mk_typar_ty tp,tpenv

    | Type_with_global_constraints(ty,wcs,m) ->
        let cty,tpenv = TcTypeAndRecover cenv newOk checkCxs env tpenv ty
        let tpenv = TcTyparConstraints cenv newOk checkCxs env tpenv wcs
        cty,tpenv

    | Type_anon_constraint(ty,m) ->  (* #typ *)
        let tp' = TcAnonTypeOrMeasure (Some KindType) cenv TyparWarnIfNotRigid DynamicReq newOk m
        let ty',tpenv = TcTypeAndRecover cenv newOk checkCxs env tpenv ty
        AddCxTypeMustSubsumeType env.DisplayEnv cenv.css m NoTrace  ty' (mk_typar_ty tp') ;
        TType_var tp', tpenv

    | Type_dimensionless m ->
        match optKind with
          Some KindType -> 
          errorR(Error("Unexpected integer literal in type expression", m)); 
          new_error_typ cenv (), tpenv
        | _ -> 
          TType_measure MeasureOne, tpenv

    | Type_power(typ, exponent, m) ->
        match optKind with
          Some KindType -> 
          errorR(Error("Unexpected ^ in type expression", m)); 
          new_error_typ cenv (), tpenv
        | _ ->          
          let ms,tpenv = TcMeasure cenv newOk checkCxs env tpenv typ m
          TType_measure (Tastops.MeasurePower ms exponent), tpenv

    | Type_quotient(typ1, typ2, m) -> 
        match optKind with
          Some KindType -> 
          errorR(Error("Unexpected / in type expression", m)); 
          new_error_typ cenv (), tpenv
        | _ ->
          let ms1,tpenv = TcMeasure cenv newOk checkCxs env tpenv typ1 m
          let ms2,tpenv = TcMeasure cenv newOk checkCxs env tpenv typ2 m
          TType_measure (MeasureProd(ms1,MeasureInv ms2)), tpenv

    | Type_app((Type_var(_,m1) | Type_power(_,_,m1)) as arg1,args,postfix,m) ->
        match optKind, args, postfix with
        | (None | Some KindMeasure), [arg2], true ->
          let ms1,tpenv = TcMeasure cenv newOk checkCxs env tpenv arg1 m1
          let ms2,tpenv = TcMeasure cenv newOk checkCxs env tpenv arg2 m
          TType_measure (MeasureProd(ms1, ms2)), tpenv

        | _, _, _ ->
          errorR(Error("Type parameter cannot be used as type constructor", m)); 
          new_error_typ cenv (), tpenv

    | Type_app(_, _, _, m) ->
        errorR(Error("Illegal syntax in type expression", m));
        new_error_typ cenv (), tpenv

and TcType cenv newOk checkCxs env (tpenv:SyntacticUnscopedTyparEnv) ty = 
    TcTypeOrMeasure (Some KindType) cenv newOk checkCxs env tpenv ty

and TcMeasure cenv newOk checkCxs env (tpenv:SyntacticUnscopedTyparEnv) ty m = 
    match ty with
    | Type_anon m ->
      error(Error("Anonymous unit-of-measure cannot be nested inside another unit-of-measure expression", m));
      new_error_measure cenv (), tpenv
    | _ ->
      match TcTypeOrMeasure (Some KindMeasure) cenv newOk checkCxs env tpenv ty with
      | TType_measure ms, tpenv -> ms,tpenv
      | _, _ -> 
        error(Error("Expected unit-of-measure, not type", m)); 
        new_error_measure cenv (), tpenv


and TcAnonTypeOrMeasure optKind cenv rigid dyn newOk m =
    if newOk = NoNewTypars then errorR (Error("anonymous type variables are not permitted in this declaration",m));
    let rigid = (if rigid = TyparAnon && newOk = NewTyparsOKButWarnIfNotRigid then TyparWarnIfNotRigid else rigid)
    let kind = match optKind with Some KindMeasure -> KindMeasure | _ -> KindType
    new_anon_inference_var cenv (kind,m,rigid,NoStaticReq,dyn)
 
and TcTypes cenv newOk checkCxs env tpenv args =
    List.mapfold (TcTypeAndRecover cenv newOk checkCxs env) tpenv args 

and TcTypesAsTuple cenv newOk checkCxs env tpenv args m = 
    match args with
    | [] -> error(InternalError("empty tuple type",m))
    | [(_,typ)] -> let typ,tpenv = TcTypeAndRecover cenv newOk checkCxs env tpenv typ in [typ],tpenv
    | (isquot,typ)::args -> 
      let ty,tpenv = TcTypeAndRecover cenv newOk checkCxs env tpenv typ
      let tys,tpenv = TcTypesAsTuple cenv newOk checkCxs env tpenv args m
      if isquot then errorR(Error("Unexpected / in type",m));
      ty::tys,tpenv

// Type-check a list of measures separated by juxtaposition, * or /
and TcMeasuresAsTuple cenv newOk checkCxs env (tpenv:SyntacticUnscopedTyparEnv) args m = 
  let rec gather args tpenv isquot acc =
    match args with
    | [] -> acc,tpenv
    | (nextisquot,typ)::args -> 
      let ms1,tpenv = TcMeasure cenv newOk checkCxs env tpenv typ m
      gather args tpenv nextisquot (if isquot then MeasureProd(acc,MeasureInv ms1) else MeasureProd(acc,ms1))
  gather args tpenv false MeasureOne


and TcTypesOrMeasures optKinds cenv newOk checkCxs env tpenv args m =
    match optKinds with
      None ->
      List.mapfold (TcTypeOrMeasure None cenv newOk checkCxs env) tpenv args
    | Some kinds ->
      if List.length kinds = List.length args
      then List.mapfold (fun tpenv (arg,kind) -> TcTypeOrMeasure (Some kind) cenv newOk checkCxs env tpenv arg) tpenv (List.zip args kinds)
      else if kinds.Length = 0
      then error(Error("Unexpected type arguments", m))
      else error(Error(sprintf "Expected %d type parameter(s) but was given %d" (List.length kinds) (List.length args), m))

and TcTyparConstraints cenv newOk checkCxs env tpenv wcs =
    (* Mark up default constraints with a priority in reverse order: last gets 0, second last gets 1 etc. See comment on TTyparDefaultsToType *)
    let _,tpenv = List.fold (fun (ridx,tpenv) tc -> ridx - 1, TcTyparConstraint ridx cenv newOk checkCxs env tpenv tc) (List.length wcs - 1, tpenv) wcs
    tpenv

// Note the args may only be the instantation of a suffix of the tps. In this case, pathTypeArgs should 
// contain the right instantation for the prefix. However we have to check that the right number of arguments 
// are given!
and TcTypeApp cenv newOk checkCxs env tpenv m tcref pathTypeArgs args =
    CheckTyconAccessible m (AccessRightsOfEnv env) tcref |> ignore;
    CheckEntityAttributes cenv.g tcref m |> CommitOperationResult;
    let tps,inst,tinst,gtyp = info_of_tcref cenv m env tcref
    // If we're not checking constraints, i.e. when we first assert the super/interfaces of a type definition, then just 
    // clear the constaint lists of the freshly generated type variables. A little ugly but fairly localized. 
    if checkCxs = NoCheckCxs then tps |> List.iter (fun tp -> tp.Data.typar_constraints <- []);
    if tinst.Length <> pathTypeArgs.Length + args.Length then 
        error (TyconBadArgs(env.DisplayEnv,tcref,pathTypeArgs.Length + args.Length,m));
    let args',tpenv = 
        // Get the suffix of typars
        let tpsForArgs = List.drop (tps.Length - args.Length) tps
        let kindsForArgs = tpsForArgs |> List.map (fun tp -> tp.Kind)
        TcTypesOrMeasures (Some kindsForArgs) cenv newOk checkCxs env tpenv args m
    let args' = pathTypeArgs @ args'
    List.iter2 (unifyE cenv env m) tinst args';
    TType_app(tcref,args'),tpenv
    
and TcTypeOrMeasureAndRecover optKind cenv newOk checkCxs env tpenv ty   =
    try TcTypeOrMeasure optKind cenv newOk checkCxs env tpenv ty 
    with e -> 
        errorRecovery e (range_of_syntype ty); 
        (if newOk <> NoNewTypars then new_error_typ cenv () else cenv.g.obj_ty),tpenv 

and TcTypeAndRecover cenv newOk checkCxs env tpenv ty   =
    TcTypeOrMeasureAndRecover (Some KindType) cenv newOk checkCxs env tpenv ty

and TcNestedTypeApplication cenv newOk checkCxs env tpenv m typ tyargs =
    if not (is_stripped_tyapp_typ cenv.g typ) then error(Error("This type has no nested types",m));
    match typ with 
    | TType_app(tcref,tinst) -> 
        let pathTypeArgs,_ = List.chop (max (tinst.Length - tcref.Typars(m).Length) 0) tinst
        TcTypeApp cenv newOk checkCxs env tpenv m tcref pathTypeArgs tyargs 
    | _ -> error(InternalError("TcNestedTypeApplication: expected type application",m))


/// Bind the patterns used in a lambda. Not clear why we don't use TcPat.
and TcSimplePat optArgsOK cenv ty env (tpenv,names,takenNames) p = 
    match p with 
    | SPat_as (id,compgen,isMemberThis,isOpt,m) -> 
        if isOpt && not optArgsOK then errorR(Error("optional arguments are only permitted on type members",m));
        if isOpt then 
            let tyarg = new_inference_typ cenv ()
            unifyE cenv env m ty (mk_option_ty cenv.g tyarg);
                
        let _,names,takenNames = TcPatBindingName cenv env id ty isMemberThis None None (OptionalInline,permitInferTypars,noArgOrRetAttribs,false,None,compgen) (names,takenNames)
        id.idText, 
        (tpenv,names,takenNames)

    | SPat_typed (p,cty,m) ->
        let cty',tpenv = TcTypeAndRecover cenv NewTyparsOK CheckCxs env tpenv cty
        match p with 
        // Optional arguments on members 
        | SPat_as(_,_,_,true,_) -> unifyE cenv env m ty (mk_option_ty cenv.g cty');
        | _ -> unifyE cenv env m ty cty';

        TcSimplePat optArgsOK cenv ty env (tpenv,names,takenNames) p

    | SPat_attrib (p,_,m) ->
        TcSimplePat optArgsOK cenv ty env (tpenv,names,takenNames) p

/// Bind the patterns used in argument position for a function, method or lambda. 
and TcSimplePats cenv optArgsOK  ty env (tpenv,names,takenNames:Set<_>) p = 
    match p with 
    | SPats ([],m) -> 
        // Unit "()" patterns in argument position become SPats([],_) in the
        // syntactic translation when building bindings. This is done because the
        // use of "()" has special significance for arity analysis and argument counting.
        //
        // Here we give a name to the single argument implied by those patterns.
        // This is a little awkward since it would be nice if this was
        // uniform with the process where we give names to other (more complex)
        // patterns used in argument position, e.g. "let f (D(x)) = ..."
        let id = ident("unitVar"^string takenNames.Count,m)
        unifyE cenv env m ty cenv.g.unit_ty;
        let _,names,takenNames = TcPatBindingName cenv env id ty false None None (OptionalInline,permitInferTypars,noArgOrRetAttribs,false,None,true) (names,takenNames)
        [id.idText],(tpenv,names,takenNames)

    | SPats ([p],m) -> 
        let v,(tpenv,names,takenNames) = TcSimplePat optArgsOK cenv ty env (tpenv,names,takenNames) p
        [v],(tpenv,names,takenNames)

    | SPats (ps,m) -> 
        let ptys = UnifyTupleType cenv env.DisplayEnv m ty ps
        let ps',(tpenv,names,takenNames) = List.mapfold (fun tpenv (ty,e) -> TcSimplePat optArgsOK cenv ty env tpenv e) (tpenv,names,takenNames) (List.zip ptys ps)
        ps',(tpenv,names,takenNames)

    | SPats_typed (p,cty,m) ->
        let cty',tpenv = TcTypeAndRecover cenv NewTyparsOK CheckCxs env tpenv cty

        match p with 
        // Solitary optional arguments on members 
        | SPats([SPat_as(_,_,_,true,_)],_) -> unifyE cenv env m ty (mk_option_ty cenv.g cty');
        | _ -> unifyE cenv env m ty cty';

        TcSimplePats cenv optArgsOK  ty env (tpenv,names,takenNames) p

and TcPatBindingName cenv env id ty isMemberThis vis1 topValData (inlineFlag,declaredTypars,argAttribs,mut,vis2,compgen) (names,takenNames:Set<string>) = 
    let vis = if isSome vis1 then vis1 else vis2
    if takenNames.Contains id.idText then errorR (VarBoundTwice id);
    let baseOrThis = if isMemberThis then MemberThisVal else NormalVal
    let names = Map.add id.idText (PrelimValScheme1(id,declaredTypars,ty,topValData,None,mut,inlineFlag,baseOrThis,argAttribs,vis,compgen)) names
    let takenNames = Set.add id.idText takenNames
    (fun (TcPatPhase2Input values) -> 
        let (vspec,typeScheme) = 
            match Map.tryfind id.idText values with
            | Some x -> x
            | None -> error(Error("Name "^id.idText^" not bound in pattern context",id.idRange))
        PBind(vspec,typeScheme)),
    names,takenNames

/// Typecheck a pattern. Patterns are type-checked in three phases: 
/// 1. TcPat builds a List.map from simple variable names to inferred types for 
///   those variables. It also returns a function to perform the second phase.
/// 2. The second phase assumes the caller has built the actual value_spec's 
///    for the values being defined, and has decided if the types of these 
///    variables are to be generalized. The caller hands this information to
///    the second-phase function in terms of a List.map from names to actual
///    value specifications. 
and TcPat warnOnUpper cenv env topValInfo vFlags (tpenv,names,takenNames) ty pat = 
    let ad = AccessRightsOfEnv env
    if verbose then  dprintf "--> TcPat@%a\n" output_range (range_of_synpat pat);
    match pat with 
    | Pat_const (c,m) -> 
        match c with 
        | Const_bytearray (bytes,m) -> 
            unifyE cenv env m ty (mk_bytearray_ty cenv.g); 
            TcPat warnOnUpper cenv env None vFlags (tpenv,names,takenNames) ty (Pat_array_or_list (true,[ for b in bytes -> Pat_const(Const_uint8 b,m) ],m))
        | Const_bignum _ -> 
            error(Error("Non-primitive numeric literal constants may not be used in pattern matches because they can be mapped to multiple different types through the use of a NumericLiteral module. Consider using replacing with a variable, and use 'when <variable> = <constant>' at the end of the match clause",m))
        | _ -> 
            let c' = TcConst cenv ty m env c
            (fun (_:TcPatPhase2Input) -> TPat_const(c',m)),(tpenv,names,takenNames)
        
    | Pat_wild m ->
        (fun _ -> TPat_wild m), (tpenv,names,takenNames)

    | Pat_isinst(cty,m) 
    | Pat_as (Pat_isinst(cty,m),_,_,_,_) -> 
        let srcTy = ty
        let tgty,tpenv = TcTypeAndRecover cenv NewTyparsOKButWarnIfNotRigid CheckCxs env tpenv cty
        TcRuntimeTypeTest cenv env.DisplayEnv m tgty srcTy;
        match pat with 
        | Pat_isinst(cty,m) ->
            (fun _ -> TPat_isinst (srcTy,tgty,None,m)),(tpenv,names,takenNames)
        | Pat_as (Pat_isinst(cty,_),id,isMemberThis,vis,m) -> 
            let bindf,names,takenNames = TcPatBindingName cenv env id tgty isMemberThis vis None vFlags (names,takenNames)
            (fun values -> TPat_isinst (srcTy,tgty,Some(bindf values),m)),
            (tpenv,names,takenNames)
        | _ -> failwith "TcPat"

    | Pat_opt_var (_,m) -> 
        error(Error("optional arguments are only permitted on type members",m))

    | Pat_as (p,id,isMemberThis,vis,m) -> 
        let bindf,names,takenNames = TcPatBindingName cenv env id ty isMemberThis vis topValInfo vFlags (names,takenNames)
        let pat',acc = TcPat warnOnUpper cenv env None vFlags (tpenv,names,takenNames) ty p
        (fun values -> TPat_as (pat' values,bindf values,m)), 
        acc

    | Pat_typed (p,cty,m) ->
        let cty',tpenv = TcTypeAndRecover cenv NewTyparsOK CheckCxs env tpenv cty
        unifyE cenv env m ty cty';
        TcPat warnOnUpper cenv env topValInfo vFlags (tpenv,names,takenNames) ty p

    | Pat_attrib (p,attrs,m) ->
        error(Error("Attributes are not allowed within patterns",m));
        //let attrs' = TcAttributes cenv env attrTgtBinding  attrs
        //TcPat warnOnUpper cenv env None vFlags (tpenv,names,takenNames) ty p

    | Pat_disj (pat1,pat2,m) ->
        let pat1',(tpenv,names1,takenNames1) = TcPat warnOnUpper cenv env None vFlags (tpenv,names,takenNames) ty pat1
        let pat2',(tpenv,names2,takenNames2) = TcPat warnOnUpper cenv env None vFlags (tpenv,names,takenNames) ty pat2
        if not (takenNames1 = takenNames2) then
          // We don't try to recover from this error since we get later bad internal errors during pattern
          // matching 
          error (UnionPatternsBindDifferentNames m);
        names1 |> Map.iter (fun u (PrelimValScheme1(id1,_,ty1,_,_,_,_,_,_,_,_)) -> 
          match Map.tryfind id1.idText names2 with 
          | None -> () 
          | Some (PrelimValScheme1(id2,_,ty2,_,_,_,_,_,_,_,_)) -> 
              unifyE cenv env m ty1 ty2);
        (fun values -> TPat_disjs ([pat1' values;pat2' values],m)), (tpenv,names1,takenNames1)

    | Pat_conjs (pats,m) ->
        let pats',acc = TcPatterns warnOnUpper cenv env vFlags (tpenv,names,takenNames) (List.map (fun pat -> ty) pats) pats
        (fun values -> TPat_conjs(List.map (fun f -> f values) pats',m)), acc

    | Pat_lid (lid,tyargs,args,vis,m) ->
        if isSome tyargs then errorR(Error("Type arguments may not be specified here",m));
        let warnOnUpper = if isNil args then warnOnUpper else AllIdsOK
        begin match ResolvePatternLongIdent cenv.nameResolver warnOnUpper false m ad env.eNameResEnv DefaultTypeNameResInfo lid with
        | Item_newdef id -> 
          match args with 
          | [] -> TcPat warnOnUpper cenv env topValInfo vFlags (tpenv,names,takenNames) ty (mksyn_pat_var vis id)
          | _ -> error (UndefinedName(0,"pattern discriminator",id,[]))

        | Item_apelem(APElemRef(apinfo,vref,idx) as apref) as item -> 
            (* TOTAL/PARTIAL ACTIVE PATTERNS *)
            let vexp, _,_, _ = TcVal cenv env tpenv vref None m
            let vexp = MkApplicableExprWithFlex cenv env vexp
            let vexpty = vexp.Type

            let exprargs,patarg = match args with | [x] -> [],x | [] -> [],Pat_const(Const_unit,m) | _ -> List.frontAndBack args
            if nonNil exprargs && List.length (names_of_apinfo apinfo) <> 1 then 
                error(Error("Only active patterns returning exactly one result may accept arguments.",m));
            (* if nonNil exprargs then warning(Experimental("The syntax for parameterized active patterns is under review and may change in a future release",m)); *)

            (* Note we parse arguments to parameterized pattern labels as patterns, not expressions. *)
            (* This means the range of syntactic expression forms that can be used here is limited. *)
            let rec convSynPatToSynExpr x = 
                match x with
                | Pat_const (c,m) -> Expr_const(c,m)
                | Pat_as (Pat_wild _,id,_,None,m) -> Expr_id_get(id)
                | Pat_typed (p,cty,m) -> Expr_typed (convSynPatToSynExpr p,cty,m)
                | Pat_lid (lid,tyargs,args,None,m) -> List.fold (fun f x -> Expr_app(ExprAtomicFlag.NonAtomic, f,convSynPatToSynExpr x,m)) (Expr_lid_get(false,lid,m)) args
                | Pat_tuple (args,m) -> Expr_tuple(List.map convSynPatToSynExpr args,m)
                | Pat_paren (p,m) -> convSynPatToSynExpr p
                | Pat_array_or_list (isArray,args,m) -> Expr_array_or_list(isArray,List.map convSynPatToSynExpr args,m)
                | Pat_expr (e,m) -> e
                | Pat_null m -> Expr_null(m)
                | _ -> error(Error("Invalid argument to parameterized pattern label",range_of_synpat x))
            let exprargs = List.map convSynPatToSynExpr exprargs

            let restys = new_inference_typs cenv (names_of_apinfo apinfo)
            let act_pat_ty = mk_apinfo_typ cenv.g m apinfo ty restys 

            let pexprty = new_inference_typ cenv ()
            let pexp, tpenv = PropagateThenTcDelayed cenv act_pat_ty env tpenv m vexp vexpty ExprAtomicFlag.NonAtomic (List.map (fun e -> DelayedApp(ExprAtomicFlag.NonAtomic,e,range_of_synexpr e)) exprargs)

            if idx >= restys.Length then error(Error("intenal error: Invalid index into active pattern array",m));
            let argty = List.nth restys idx 
                
            let arg',(tpenv,names,takenNames) = TcPat warnOnUpper cenv env None vFlags (tpenv,names,takenNames) argty patarg
            (* If there are any expression args then we've lost identity. *)
            let identityVrefOpt = (if nonNil exprargs then None else Some vref)
            (fun values -> 
                // Report information about the 'active recognizer' occurence to IDE
                CallNameResolutionSink(range_of_lid lid,nenv_of_tenv env,item,ItemOccurence.Pattern,env.DisplayEnv,AccessRightsOfEnv env)
                TPat_query((pexp, restys, identityVrefOpt,idx,apinfo), arg' values, m)), 
            (tpenv,names,takenNames)

        | (Item_ucase _ | Item_ecref _) as item ->
            (* DATA MATCH CONSTRUTORS *)
            let mkf,argtys = pat_constr_unify m cenv env ty item
            let nargtys = argtys.Length
            let args = 
              match args with 
              | []-> []
              (* note: the next will always be parenthesized *)
              | [(Pat_tuple (args,m)) | Pat_paren(Pat_tuple (args,m),_)] when nargtys > 1 -> args

              (* note: like OCaml we allow both 'C _' and 'C (_)' regardless of number of argument of the pattern *)
              | [(Pat_wild m as e) | Pat_paren(Pat_wild m as e,_)] -> Array.to_list (Array.create nargtys e)
              | [arg] -> [arg] 
              | _ when nargtys = 0 -> error(Error("This union case does not take arguments",m)) 
              | _ when nargtys = 1 -> error(Error("This union case takes one argument",m)) 
              | _ -> error(Error("This union case expects "^string nargtys^" arguments in tupled form",m))
            gen_constr_check env nargtys args.Length m;
            let args',acc = TcPatterns warnOnUpper cenv env vFlags (tpenv,names,takenNames) argtys args
            (fun values -> 
                // Report information about the case occurence to IDE
                CallNameResolutionSink(range_of_lid lid,nenv_of_tenv env,item,ItemOccurence.Pattern,env.DisplayEnv,AccessRightsOfEnv env)
                mkf(List.map (fun f -> f values) args')), acc
                
        | Item_il_field finfo ->
            (* LITERAL .NET FIELDS *)
            CheckILFieldInfoAccessible cenv.g cenv.amap m (AccessRightsOfEnv env) finfo;
            if not finfo.IsStatic then errorR (Error (sprintf "field '%s' is not static" finfo.FieldName,m));
            CheckILFieldAttributes cenv.g finfo m;
            match finfo.LiteralValue with 
            | None -> error (Error("this field is not a literal and cannot be used in a pattern", m));
            | Some lit -> 
                unifyE cenv env m ty (FieldTypeOfILFieldInfo cenv.amap m finfo);
                let c' = TcFieldInit m lit
                (fun _ -> TPat_const (c',m)),(tpenv,names,takenNames)             
            
        | Item_recdfield rfinfo ->
            (* LITERAL F# FIELDS *)
            CheckRecdFieldInfoAccessible m (AccessRightsOfEnv env) rfinfo;
            if not rfinfo.IsStatic then errorR (Error (sprintf "field '%s' is not static" rfinfo.Name,m));
            CheckRecdFieldInfoAttributes cenv.g rfinfo m  |> CommitOperationResult;        
            match rfinfo.LiteralValue with 
            | None -> error (Error("this field is not a literal and cannot be used in a pattern", m));
            | Some lit -> 
                unifyE cenv env m ty rfinfo.FieldType;
                (fun _ -> TPat_const (lit,m)),(tpenv,names,takenNames)             

        | Item_val vref ->
            match vref.LiteralValue with 
            | None -> error (Error("this value is not a literal and cannot be used in a pattern", m));
            | Some lit -> 
                let (vexp,_,vexpty,_) = TcVal cenv env tpenv vref None m
                CheckValAccessible m (AccessRightsOfEnv env) vref;
                CheckFSharpAttributes cenv.g vref.Attribs m |> CommitOperationResult;
                unifyE cenv env m ty vexpty;
                (fun _ -> TPat_const (lit,m)),(tpenv,names,takenNames)             

        |  _ -> error (Error("this is not a variable, constant, active recognizer or literal",m))
        end

    | Pat_expr(exp,m) -> error (Error("this is not a valid pattern",m))
          
    | Pat_tuple (args,m) ->
        let argtys = new_inference_typs cenv args
        unifyE cenv env m ty (TType_tuple argtys);
        let args',acc = TcPatterns warnOnUpper cenv env vFlags (tpenv,names,takenNames) argtys args
        (fun values -> TPat_tuple(List.map (fun f -> f values) args',argtys,m)), acc

    | Pat_paren (p,m) ->
        TcPat warnOnUpper cenv env None vFlags (tpenv,names,takenNames) ty p

    | Pat_array_or_list (isArray,args,m) ->
        let argty = new_inference_typ cenv ()
        unifyE cenv env m ty (if isArray then mk_array_typ cenv.g argty else Tastops.mk_list_ty cenv.g argty);
        let args',acc = TcPatterns warnOnUpper cenv env vFlags (tpenv,names,takenNames) (List.map (fun _ -> argty) args) args
        (fun values -> 
            let args' = List.map (fun f -> f values) args'
            if isArray then TPat_array(args', argty, m)
            else List.foldBack (mk_cons_pat cenv.g argty) args' (mk_nil_pat cenv.g m argty)), acc

    | Pat_recd (flds,m) ->
        let tcref,fldsmap,fldsList (* REVIEW: use this *) = BuildFieldMap cenv env true ty flds m
        (* REVIEW: use fldsList to type check pattern in code order not field defn order *) 
        let _,inst,tinst,gtyp = info_of_tcref cenv m env tcref
        unifyE cenv env m ty gtyp;
        let fields = tcref.TrueInstanceFieldsAsList
        let ftys = List.map (fun fsp -> FreshenPossibleForallTy cenv.g m TyparFlexible (typ_of_rfield inst fsp),fsp) fields
        let fldsmap',acc = 
          List.mapfold 
            (fun s ((_,ftinst,ty),fsp) -> 
              if Map.mem fsp.rfield_id.idText  fldsmap then 
                let f,s = TcPat warnOnUpper cenv env None vFlags s ty (Map.find fsp.rfield_id.idText fldsmap)
                (ftinst,f),s
              else ([],(fun values -> TPat_wild m)),s)
            (tpenv,names,takenNames)
            ftys
        (fun values -> TPat_recd (tcref,tinst,List.map (fun (ftinst,f) -> (ftinst,f values)) fldsmap',m)), 
        acc

    | Pat_range (c1,c2,m) -> 
        warning(Deprecated("Character range matches will be removed in a future version of F#. Consider using a 'when' pattern guard instead",m));
        unifyE cenv env m ty (cenv.g.char_ty);
        (fun values -> TPat_range(c1,c2,m)),(tpenv,names,takenNames)

    | Pat_null m -> 
        AddCxTypeMustSupportNull env.DisplayEnv cenv.css m NoTrace ty;
        (fun _ -> TPat_null m),(tpenv,names,takenNames)

    | Pat_instance_member (_,_,_,m) -> 
        errorR(Error("illegal pattern",range_of_synpat pat));
        (fun _ -> TPat_wild m), (tpenv,names,takenNames)

and TcPatterns warnOnUpper cenv env vFlags s argtys args = 
    assert (List.length args  = List.length argtys);
    List.mapfold (fun s (ty,pat) -> TcPat warnOnUpper cenv env None vFlags s ty pat) s (List.zip argtys args)


and solveTypAsError cenv denv m ty =
    let ty2 = new_error_typ cenv ()
    assert((dest_typar_typ cenv.g ty2).IsFromError);
    SolveTypEqualsTypKeepAbbrevs (MakeConstraintSolverEnv cenv.css m denv) 0 m NoTrace ty ty2 |> ignore

and TcExprOfUnknownType cenv env tpenv expr =
    let exprty = new_inference_typ cenv ()
    let expr',tpenv = TcExpr cenv exprty env tpenv expr
    expr',exprty,tpenv

and TcExprFlex cenv flex ty (env:tcEnv) tpenv e =
    if flex then
        let argty = new_inference_typ cenv ()
        let m = range_of_synexpr e
        AddCxTypeMustSubsumeType env.DisplayEnv cenv.css m NoTrace ty argty ;
        let e',tpenv  = TcExpr cenv argty env tpenv e 
        let e' = mk_coerce_if_needed cenv.g ty argty e'
        e',tpenv
    else
        TcExpr cenv ty env tpenv e
    

and TcExpr cenv ty (env:tcEnv) tpenv expr =
    let m = range_of_synexpr expr

    if verbose then  dprintf "--> TcExpr@%a\n" output_range (range_of_synexpr expr);

    // Start an error recovery handler 
    // Note the try/catch can lead to tail-recursion problems for iterated constructs, e.g. let... in... 
    // So be careful! 
    try 
        /// Count our way through the expression shape that makes up an object constructor 
        /// See notes at definition of "ctor" re. object model constructors. 
        let env = 
            if GetCtorShapeCounter env > 0 then AdjustCtorShapeCounter (fun x -> x - 1) env 
            else env

        let tm,tpenv = TcExprThen cenv ty env tpenv expr []

        if verbose then  dprintf "<-- TcExpr@%a\n" output_range (range_of_synexpr expr);
        tm,tpenv
    with e -> 
        if verbose then  dprintf "!!! TcExpr@%a\n" output_range (range_of_synexpr expr);

        // Error recovery - return some rubbish expression, but replace/annotate 
        // the type of the current expression with a type variable that indicates an error 
        errorRecovery e m; 
        solveTypAsError cenv env.DisplayEnv m ty;
        if verbose then  dprintf "<-- TcExpr@%a\n" output_range (range_of_synexpr expr);
        mk_throw m ty (mk_one cenv.g m), tpenv


/// This is used to typecheck legitimate 'main body of constructor' expressions 
and TcExprThatIsCtorBody ctorThisVarRefCellOpt cenv ty env tpenv expr =
    let env = {env with eCtorInfo = Some(InitialExplicitCtorInfo(ctorThisVarRefCellOpt)) }
    let expr,tpenv = TcExpr cenv ty env tpenv expr
    let expr = CheckAndRewriteObjectCtor cenv.g env expr
    expr,tpenv

/// This is used to typecheck all ordinary expressions including constituent 
/// parts of ctor. 
and TcExprThatCanBeCtorBody cenv ty env tpenv expr =
    let env = if AreWithinCtorShape env then AdjustCtorShapeCounter (fun x -> x + 1) env else env
    TcExpr cenv ty env tpenv expr

/// This is used to typecheck legitimate 'non-main body of object constructor' expressions 
and TcExprThatCantBeCtorBody cenv ty env tpenv expr =
    let env = if AreWithinCtorShape env then ExitCtorShapeRegion env else env
    TcExpr cenv ty env tpenv expr

/// This is used to typecheck legitimate 'non-main body of object constructor' expressions 
and TcStmtThatCantBeCtorBody cenv env tpenv expr =
    let env = if AreWithinCtorShape env then ExitCtorShapeRegion env else env
    TcStmt cenv env tpenv expr

and TcStmt cenv env tpenv expr =
    let expr',ty,tpenv = TcExprOfUnknownType cenv env tpenv expr
    let m = range_of_synexpr expr
    let wasUnit = UnifyUnitType cenv env.DisplayEnv m ty (Some expr');
    if wasUnit then
      expr',tpenv
    else
      mk_compgen_seq m expr' (mk_unit cenv.g m),tpenv



/// During checking of expressions of the form (x(y)).z(w1,w2) 
/// keep a stack of things on the right. This lets us recognize 
/// method applications. 
and TcExprThen cenv ty env tpenv expr delayed =
    match expr with 

    | Expr_lid_or_id_get (isOpt,lid,m) ->
        if isOpt then errorR(Error("syntax error - unexpected '?' symbol",m));
        TcLongIdentThen cenv ty env tpenv lid delayed

    | Expr_app (hpa,f,x,m) ->
        TcExprThen cenv ty env tpenv f ((DelayedApp (hpa,x,m)):: delayed)

    | Expr_tyapp (f,x,m) ->
        TcExprThen cenv ty env tpenv f ((DelayedTypeApp (x,m)):: delayed)

    | Expr_lvalue_get (e1,lid,m) ->
        TcExprThen cenv ty env tpenv e1 ((DelayedDotLookup (lid,m))::delayed)
           
    | Expr_lbrack_get (e1,idx,mDot,m) 
    | Expr_lbrack_set (e1,idx,_,mDot,m) ->
        TcIndexerThen cenv env ty m mDot tpenv expr e1 idx delayed
    
    | _  ->
        match delayed with 
        | [] -> TcExprUndelayed cenv ty env tpenv expr
        | _ -> 
            let expr',exprty,tpenv = TcExprUndelayedNoType cenv env tpenv expr
            PropagateThenTcDelayed cenv ty env tpenv (range_of_expr expr') (MkApplicableExprNoFlex cenv expr') exprty ExprAtomicFlag.NonAtomic delayed

and TcExprs cenv env m tpenv flexes argtys args = 
    if (List.length args  <> List.length argtys) then error(Error(sprintf "expected %d expressions, got %d" (List.length argtys) (List.length args),m));
    (tpenv, List.zip3 flexes argtys args) ||> List.mapfold (fun tpenv (flex,ty,e) -> 
         TcExprFlex cenv flex ty env tpenv e)


//-------------------------------------------------------------------------
// TcExprUndelayed
//------------------------------------------------------------------------- 

and TcExprUndelayedNoType cenv env tpenv expr =
    let exprty = new_inference_typ cenv ()
    let expr',tpenv = TcExprUndelayed cenv exprty env tpenv expr
    expr',exprty,tpenv

and TcExprUndelayed cenv ty env tpenv expr =
    let m = range_of_synexpr expr
    (* dprintf "TcExprUndelayed: %a: isSome(env.eFamilyType) = %b\n" output_range m (isSome env.eFamilyType); *)

    if verbose then  dprintf "--> TcExprUndelayed@%a\n" output_range (range_of_synexpr expr); 
    match expr with 
    | Expr_paren (expr2,m2) -> 
        (* We invoke CallExprHasTypeSink for every construct which is atomic in the syntax, i.e. where a '.' immediately following the *)
        (* construct is a dot-lookup for the result of the construct. *)
        CallExprHasTypeSink(m,nenv_of_tenv env,ty, env.DisplayEnv,AccessRightsOfEnv env);
        TcExpr cenv ty env tpenv expr2

    | Expr_lbrack_get _ | Expr_lbrack_set _
    | Expr_tyapp _ | Expr_id_get _ | Expr_lid_get _ | Expr_app _ | Expr_lvalue_get _ -> error(Error("TcExprUndelayed: delayed", range_of_synexpr expr))

    | Expr_const (Const_string (s,m),_) -> 
        CallExprHasTypeSink(m,nenv_of_tenv env,ty, env.DisplayEnv,AccessRightsOfEnv env);
        TcConstStringExpr cenv ty env m tpenv s

    | Expr_const (c,m) -> 
        CallExprHasTypeSink(m,nenv_of_tenv env,ty, env.DisplayEnv,AccessRightsOfEnv env);
        TcConstExpr cenv ty env m tpenv c

    | Expr_lambda _ -> TcIteratedLambdas cenv true env ty Set.empty tpenv expr

    | Expr_match (spMatch,x,matches,isExnMatch,m) ->
        if verbose then  dprintn "tc Expr_match";
        let x',inputTy,tpenv = TcExprOfUnknownType cenv env tpenv x
        let exprm = range_of_expr x'
        let v,e, tpenv = TcAndPatternCompileMatchClauses exprm m (if isExnMatch then Throw else Incomplete) cenv inputTy ty env tpenv matches
        (mk_let spMatch exprm v x'  e,tpenv)

    | Expr_assert (x,m) ->
        TcAssertExpr cenv ty env m tpenv x

    | Expr_typed (e,cty,m) ->
        let tgty,tpenv = TcTypeAndRecover cenv NewTyparsOK CheckCxs env tpenv cty
        unifyE cenv env m ty tgty;
        let e',tpenv = TcExpr cenv ty env tpenv e 
        e',tpenv

    | Expr_isinst (e,tgty,m) ->
        let e',srcTy,tpenv = TcExprOfUnknownType cenv env tpenv e 
        unifyE cenv env m ty cenv.g.bool_ty;
        let tgty,tpenv = TcType cenv NewTyparsOK CheckCxs env tpenv tgty
        TcRuntimeTypeTest cenv env.DisplayEnv m tgty srcTy;        
        let e' = mk_call_istype cenv.g m tgty  e'
        e', tpenv
    
    (* Expr_addrof is noted in the syntax ast in order to recognize it as concrete type information *)
    (* during type checking, in particular prior to resolving overloads. This helps distinguish *)
    (* its use at method calls from the use of the conflicting 'ref' mechanism for passing byref parameters *)
    | Expr_addrof(byref,e,opm,m) -> 
        TcExpr cenv ty env tpenv (mksyn_prefix opm m (if byref then "~&" else "~&&") e) 
        
    | Expr_upcast (e,_,m) | Expr_inferred_upcast (e,m) -> 
        let e',srcTy,tpenv = TcExprOfUnknownType cenv env tpenv e 
        let tgty,tpenv = 
          match expr with
          | Expr_upcast (e,tgty,m) -> 
              let tgty,tpenv = TcType cenv NewTyparsOK CheckCxs env tpenv tgty
              unifyE cenv env m tgty ty;
              tgty,tpenv
          | Expr_inferred_upcast (e,m) -> ty,tpenv 
          | _ -> failwith "upcast"
        TcStaticUpcast cenv env.DisplayEnv m tgty srcTy;
        mk_coerce(e',tgty,m,srcTy),tpenv

    | Expr_downcast(e,_,m) | Expr_inferred_downcast (e,m) ->
        let e',srcTy,tpenv = TcExprOfUnknownType cenv env tpenv e 
        let tgty,tpenv = 
          match expr with
          | Expr_downcast (e,tgty,m) -> 
              let tgty,tpenv = TcType cenv NewTyparsOK CheckCxs env tpenv tgty
              unifyE cenv env m tgty ty;
              tgty,tpenv
          | Expr_inferred_downcast (e,m) -> ty,tpenv 
          | _ -> failwith "downcast"
        TcRuntimeTypeTest cenv env.DisplayEnv m tgty srcTy;
        (* TcRuntimeTypeTest ensures tgty is a nominal type. Hence we can insert a check here *)
        (* based on the nullness semantics of the nominal type. *)
        let e' = mk_call_unbox cenv.g m tgty  e'
        e',tpenv

    | Expr_null (m) ->
        AddCxTypeMustSupportNull env.DisplayEnv cenv.css m NoTrace ty;
        mk_null m ty,tpenv

    | Expr_lazy (e,m) ->
        let ety = new_inference_typ cenv ()
        unifyE cenv env m ty (mk_lazy_ty cenv.g ety);
        let e',tpenv = TcExpr cenv ety env tpenv e 
        mk_lazy_delayed cenv.g m ety (mk_unit_delay_lambda cenv.g m e'), tpenv

    | Expr_ifnull (e1,e2,m) ->
        let e1',tpenv = TcExpr cenv ty env tpenv e1 
        let e2',tpenv = TcExpr cenv ty env tpenv e2 
        AddCxTypeMustSupportNull env.DisplayEnv cenv.css m NoTrace ty;
        mk_compgen_let_in m "nullTest" ty e1' (fun (v,ve) -> 
        mk_nonnull_cond cenv.g m ty ve ve e2'),tpenv

    | Expr_tuple (args,m) -> 
        let argtys = UnifyTupleType cenv env.DisplayEnv m ty args
        // No subsumption at tuple construction
        let flexes = argtys |> List.map (fun _ -> false)
        let args',tpenv = TcExprs cenv env m tpenv flexes argtys args
        mk_tupled cenv.g m args' argtys, tpenv

    | Expr_array_or_list (isArray,args,m) -> 
        CallExprHasTypeSink(m,nenv_of_tenv env,ty, env.DisplayEnv,AccessRightsOfEnv env);

        let argty = new_inference_typ cenv ()
        unifyE cenv env m ty (if isArray then mk_array_typ cenv.g argty else Tastops.mk_list_ty cenv.g argty);
        // Always allow subsumption once a nominal type is known
        let args',tpenv = List.mapfold (TcExpr cenv argty env) tpenv args
        
        let expr = 
            if isArray then TExpr_op(TOp_array, [argty],args',m)
            else List.foldBack (mk_cons cenv.g argty) args' (mk_nil cenv.g m argty)
        expr,tpenv

    | Expr_new (superInit,objTy,arg,m) -> 
        let objTy',tpenv = TcType cenv NewTyparsOK CheckCxs env tpenv objTy
        unifyE cenv env m ty objTy';
        TcNewExpr cenv env tpenv objTy' (Some (range_of_syntype objTy)) superInit arg m

    | Expr_impl(objTy,argopt,binds,extraImpls,m) ->
        CallExprHasTypeSink(m,nenv_of_tenv env,ty, env.DisplayEnv,AccessRightsOfEnv env);
        TcObjectExpr cenv ty env tpenv (objTy,argopt,binds,extraImpls,m)
            
    | Expr_recd (inherits,optOrigExpr, flds, m) -> 
        CallExprHasTypeSink(m,nenv_of_tenv env,ty, env.DisplayEnv,AccessRightsOfEnv env);
        TcRecdExpr cenv ty env tpenv (inherits,optOrigExpr,flds,m)

    | Expr_while (spWhile,e1,e2,m) ->
        unifyE cenv env m ty cenv.g.unit_ty;
        let e1',tpenv = TcExpr cenv (cenv.g.bool_ty) env tpenv e1
        let e2',tpenv = TcStmt cenv env tpenv e2
        mk_while cenv.g (spWhile,e1',e2',m),tpenv

    | Expr_for (spBind,id,start,dir,finish,body,m) ->
        unifyE cenv env m ty cenv.g.unit_ty;
        let start' ,tpenv = TcExpr cenv (cenv.g.int_ty) env tpenv start
        let finish',tpenv = TcExpr cenv (cenv.g.int_ty) env tpenv finish
        let idv,ide = Tastops.mk_local id.idRange  id.idText cenv.g.int_ty
        let envinner = AddLocalVal m idv env
        let body',tpenv = TcStmt cenv envinner tpenv body
        mk_fast_for_loop  cenv.g (spBind,m,idv,start',dir,finish',body'), tpenv
        
    | Expr_foreach (spBind,SeqExprOnly(seqExprOnly),pat,enumExpr,body,m) ->
        if seqExprOnly then warning (Error("This expression form may only be used in sequence and computation expressions",m));
        TcForEachExpr cenv ty env tpenv (pat,enumExpr,body,m,spBind)

    | Expr_comprehension (isArrayOrList,isNotNakedRefCell,comp,m) ->
        let env = ExitFamilyRegion env
        if not isArrayOrList then 
            match comp with 
            | Expr_new _ -> 
                errorR(Error("Invalid object expression. Objects without overrides or interfaces should use the expression form 'new Type(args)' without braces",m));
            | SimpleSemicolonSequence false _ -> 
                errorR(Error("Invalid object, sequence or record expression",m));
            | _ -> 
                ()
        if not !isNotNakedRefCell && not cenv.g.compilingFslib then 
            warning(Error("Sequence expressions should be of the form 'seq { ... }'. Sequence expressions without a 'seq' prefix are deprecated",m));
        
        TcComputationExpression cenv env ty m None tpenv comp
        
    | Expr_array_or_list_of_seq (isArray,comp,m)  ->
        CallExprHasTypeSink(m,nenv_of_tenv env,ty, env.DisplayEnv,AccessRightsOfEnv env);

        
        match comp with 
        | Expr_comprehension(_,_,(SimpleSemicolonSequence true elems as body),_) -> 
            match body with 
            | SimpleSemicolonSequence false _ -> 
                ()
            | _ -> 
                warning(Deprecated("This list or array expression includes an element of the form 'if ... then ... else'. Parenthesize this expression to indicate it is an individual element of the list or array, to disambiguate this from a list generated using a sequence expression",m));

            let replacementExpr = 
                if isArray then 
                    (* This are to improve parsing/processing speed for parser tables by converting to an array blob ASAP *)
                    let nelems = elems.Length 
                    if nelems > 0 && List.forall (function Expr_const(Const_uint16 _,_) -> true | _ -> false) elems 
                    then Expr_const (Const_uint16array (Array.of_list (List.map (function Expr_const(Const_uint16 x,_) -> x | _ -> failwith "unreachable") elems)), m)
                    elif nelems > 0 && List.forall (function Expr_const(Const_uint8 _,_) -> true | _ -> false) elems 
                    then Expr_const (Const_bytearray (Array.of_list (List.map (function Expr_const(Const_uint8 x,_) -> x | _ -> failwith "unreachable") elems), m), m)
                    else Expr_array_or_list(isArray, elems, m)
                else 
                    if List.length elems > 500 then 
                        error(Error("This list expression exceeds the maximum size for list literals. Use an array for larger literals and call Array.ToList",m));
                    Expr_array_or_list(isArray, elems, m)

            TcExprUndelayed cenv ty env tpenv replacementExpr
        | _ -> 
            let genCollElemTy = new_inference_typ cenv ()
            let genCollTy =  (if isArray then mk_array_typ else mk_list_ty) cenv.g genCollElemTy
            unifyE cenv env m ty genCollTy;
            let exprty = new_inference_typ cenv ()
            let genEnumTy =  mk_seq_ty cenv.g genCollElemTy
            AddCxTypeMustSubsumeType env.DisplayEnv cenv.css m NoTrace genEnumTy exprty; 
            let expr,tpenv = TcExpr cenv exprty env tpenv comp
            let expr = mk_coerce_if_needed cenv.g genEnumTy (type_of_expr cenv.g expr) expr
            (if isArray then mk_call_seq_to_array else mk_call_seq_to_list) cenv.g m genCollElemTy 
                // We add a call to 'seq ... ' to make sure sequence expression compilation gets applied to the contents of the
                // comprehension. But don't do this in FSharp.Core.dll since 'seq' may not yet be defined.
                ((if cenv.g.compilingFslib then id else mk_call_seq cenv.g m genCollElemTy)
                    (mk_coerce(expr,genEnumTy,range_of_expr expr,exprty))),tpenv

    | Expr_let (isRec,isUse,binds,body,m) ->
        TcLinearLetExprs (TcExprThatCanBeCtorBody cenv) cenv env ty (fun x -> x) tpenv (true(*consume use bindings*),isRec,isUse,binds,body,m) 

    | Expr_try_catch (e1,mTryToWith,clauses,mWithToLast,mTryToLast,spTry,spWith) ->
        let e1',tpenv = TcExpr cenv ty env tpenv e1
        (* Compile the pattern twice, once as a List.filter with all succeeding targets returning "1", and once as a proper catch block. *)
        let filterClauses = clauses |> List.map (function (Clause(pat,optWhenExpr,e,m,_)) -> Clause(pat,optWhenExpr,(Expr_const(Const_int32 1,m)),m,SuppressSequencePointAtTarget))
        let checkedFilterClauses, tpenv = TcMatchClauses cenv cenv.g.exn_ty cenv.g.int_ty env tpenv filterClauses
        let checkedHandlerClauses, tpenv = TcMatchClauses cenv cenv.g.exn_ty ty env tpenv clauses
        let v1,filter_expr = CompilePatternForMatchClauses cenv env mWithToLast mWithToLast true FailFilter cenv.g.exn_ty cenv.g.int_ty checkedFilterClauses
        let v2,handler_expr = CompilePatternForMatchClauses cenv env mWithToLast mWithToLast true Rethrow cenv.g.exn_ty ty checkedHandlerClauses
        mk_try_catch cenv.g (e1',v1,filter_expr,v2,handler_expr,mTryToLast,ty,spTry,spWith),tpenv

    | Expr_try_finally (e1,e2,mTryToLast,spTry,spFinally) ->
        let e1',tpenv = TcExpr cenv ty env tpenv e1
        let e2',tpenv = TcStmt cenv env tpenv e2
        mk_try_finally cenv.g (e1',e2',mTryToLast,ty,spTry,spFinally),tpenv

    | Expr_arb m -> 
        mk_ilzero(m,ty), tpenv

    | Expr_throwaway (e1,m) -> 
        let _,_,tpenv = TcExprOfUnknownType cenv env tpenv e1
        mk_ilzero(m,ty),tpenv

    | Expr_seq (sp,dir,e1,e2,m) ->
        if dir then 
            // Use continuations to cope with long linear sequences 
            let rec TcLinearSeqs expr cont = 
                match expr with 
                | Expr_seq (sp,true,e1,e2,m) ->
                  let e1',tpenv = TcStmtThatCantBeCtorBody cenv env tpenv e1
                  TcLinearSeqs e2 (fun (e2',tpenv) -> 
                      cont (TExpr_seq(e1',e2',NormalSeq,sp,m),tpenv))

                | _ -> 
                  cont (TcExprThatCanBeCtorBody cenv ty env tpenv expr)
            TcLinearSeqs expr (fun res -> res)
        else 
          (* Constructors using "new (...) = <ctor-expr> then <expr>" *)
          let e1',tpenv = TcExprThatCanBeCtorBody cenv ty env tpenv e1
          if (GetCtorShapeCounter env) <> 1 then 
              errorR(Error("the expression form 'expr then expr' may only be used as part of an explicit object constructor",m));
          let e2',tpenv = TcStmtThatCantBeCtorBody cenv (ExitCtorPreConstructRegion env) tpenv e2
          TExpr_seq(e1',e2',ThenDoSeq,sp,m),tpenv
    | Expr_do (e1,m) ->
          unifyE cenv env m ty cenv.g.unit_ty;
          TcStmtThatCantBeCtorBody cenv env tpenv e1

    | Expr_cond (e1,e2,e3opt,spIfToThen,mIfToThen,m) ->
        let e1',tpenv = TcExprThatCantBeCtorBody cenv cenv.g.bool_ty env tpenv e1  
        (if isNone e3opt then unifyE cenv env m ty cenv.g.unit_ty);
        let e2',tpenv = TcExprThatCanBeCtorBody cenv ty env tpenv e2
        let e3',tpenv = 
            match e3opt with 
            | None -> mk_unit cenv.g mIfToThen,tpenv // the fake 'unit' value gets exactly the same range as spIfToThen
            | Some e3 -> TcExprThatCanBeCtorBody cenv ty env tpenv e3 
        mk_cond spIfToThen SequencePointAtTarget m ty e1' e2' e3', tpenv

    (* This is for internal use in the libraries only *)
    | Expr_static_optimization (constraints,e2,e3,m) ->
        let constraints',tpenv = List.mapfold (TcStaticOptimizationConstraint cenv env) tpenv constraints
        (* Do not force the types of the two expressions to be equal *)
        (* REVIEW: check the types are the same after applying the constraints *)
        let e2',_, tpenv = TcExprOfUnknownType cenv env tpenv e2
        let e3',tpenv = TcExpr cenv ty env tpenv e3
        TExpr_static_optimization(constraints',e2',e3',m), tpenv

    | Expr_lvalue_set (e1,f,e2,m) ->
        TcExprThen cenv ty env tpenv e1 [DelayedDotLookup(f,union_ranges (range_of_synexpr e1) (range_of_lid f)); MakeDelayedSet(e2,m)]

    | Expr_lvalue_indexed_set (e1,f,e2,e3,m) ->
        TcExprThen cenv ty env tpenv e1 [DelayedDotLookup(f,union_ranges (range_of_synexpr e1) (range_of_lid f)); DelayedApp(ExprAtomicFlag.Atomic,e2,m); MakeDelayedSet(e3,m)]

    | Expr_lid_set (lid,e2,m) -> 
        TcLongIdentThen cenv ty env tpenv lid [ MakeDelayedSet(e2, m) ]
    
    (* Type.Items(e1) <- e2 *)
    | Expr_lid_indexed_set (lid,e1,e2,m) ->
        TcLongIdentThen cenv ty env tpenv lid [ DelayedApp(ExprAtomicFlag.Atomic,e1,m); MakeDelayedSet(e2,m) ]

    | Expr_trait_call(tps,memSpfn,arg,m) ->
        let (TTrait(_,logicalCompiledName,_,argtys,returnTy,_) as traitInfo),tpenv = TcPseudoMemberSpec cenv NewTyparsOK CheckCxs env tps  tpenv memSpfn m
        if List.mem logicalCompiledName BakedInTraitConstraintNames then 
            warning(BakedInMemberConstraintName(logicalCompiledName,m))
        
        let returnTy = GetFSharpViewOfReturnType cenv.g returnTy
        let args,namedCallerArgs = GetMethodArgs arg 
        if nonNil namedCallerArgs then errorR(Error("Named arguments may not be given to member trait calls",m));
        // Subsumption at trait calls if arguments have nominal type prior to unification of any arguments or return type
        let flexes = argtys |> List.map (is_typar_typ cenv.g >> not)
        let args',tpenv = TcExprs cenv env m tpenv flexes argtys args
        AddCxMethodConstraint env.DisplayEnv cenv.css m NoTrace traitInfo;
        unifyE cenv env m ty returnTy;      
        TExpr_op(TOp_trait_call(traitInfo), [], args', m), tpenv
          
    | Expr_typeof(sty,m) ->
        if not cenv.g.compilingFslib then 
            errorR(Deprecated("Use 'typeof<_>' instead",m));
        let sty',tpenv = TcType cenv NewTyparsOK CheckCxs env tpenv sty
        unifyE cenv env m ty cenv.g.system_Type_typ;      
        mk_call_typeof cenv.g m sty', tpenv

    | Expr_constr_field_get (e1,c,n,m) ->
        let e1',ty1,tpenv = TcExprOfUnknownType cenv env tpenv e1
        let mkf,ty2 = TcUnionCaseField cenv env ty1 m c n 
                          ((fun (a,b) n -> mk_ucase_field_get_unproven(e1',a,b,n,m)),
                           (fun a n -> mk_exnconstr_field_get(e1',a,n,m)))
        unifyE cenv env m ty ty2;
        mkf n,tpenv

    | Expr_constr_field_set (e1,c,n,e2,m) ->
        unifyE cenv env m ty cenv.g.unit_ty;
        let e1',ty1,tpenv = TcExprOfUnknownType cenv env tpenv e1
        let mkf,ty2 = TcUnionCaseField cenv  env ty1 m c n
                          ((fun (a,b) n e2' -> 
                             if not (ucref_rfield_mutable cenv.g a n) then errorR(Error("this field is not mutable",m));
                             mk_ucase_field_set(e1',a,b,n,e2',m)),
                           (fun a n e2' -> 
                             if not (ecref_rfield_mutable a n) then errorR(Error("this field is not mutable",m));
                             mk_exnconstr_field_set(e1',a,n,e2',m)))
        let e2',tpenv = TcExpr cenv ty2 env tpenv e2
        mkf n e2',tpenv

    | Expr_asm (s,tyargs,args,rtys,m) ->
        let argtys = new_inference_typs cenv args
        let tyargs',tpenv = TcTypes cenv NewTyparsOK CheckCxs env tpenv tyargs
        // No subsumption at uses of IL assembly code
        let flexes = argtys |> List.map (fun _ -> false)
        let args',tpenv = TcExprs cenv env m tpenv flexes argtys args
        let rtys',tpenv = TcTypes cenv NewTyparsOK CheckCxs env tpenv rtys
        let returnTy = 
            match rtys' with 
            | [] -> cenv.g.unit_ty
            | [ returnTy ] -> returnTy
            | _ -> error(InternalError("Only zero or one pushed items are permitted in IL assembly code",m))
        unifyE cenv env m ty returnTy;
        mk_asm(Array.to_list s,tyargs',args',rtys',m),tpenv

    | Expr_quote(oper,raw,ast,m) ->
        CallExprHasTypeSink(m,nenv_of_tenv env,ty, env.DisplayEnv,AccessRightsOfEnv env);
        TcQuotationExpr cenv ty env tpenv (oper,raw,ast,m) 

    | Comp_yield ((isTrueYield,isTrueReturn),_,m)
    | Comp_yieldm ((isTrueYield,isTrueReturn),_,m) when isTrueYield -> 
         error(Error("This construct may only be used within list, array and sequence expressions, e.g. expressions of the form 'seq { ... }', '[ ... ]' or '[| ... |]'. These use the syntax 'for ... in ... do ... yield...' to generate elements",m))
    | Comp_yield ((isTrueYield,isTrueReturn),_,m)
    | Comp_yieldm ((isTrueYield,isTrueReturn),_,m) when isTrueReturn -> 
         error(Error("This construct may only be used within computation expressions. To return a value from an ordinary function simply write the expression without 'return'",m))
    | Comp_yield (_,_,m)
    | Comp_yieldm (_,_,m) 
    | Comp_zero (m) ->
         error(Error("This construct may only be used within sequence or computation expressions",m))
    | Comp_do_bind  (_,m) 
    | Comp_bind  (_,_,_,_,_,m) -> 
         error(Error("This construct may only be used within computation expressions",m))

/// Check lambdas as a group, to catch duplicate names in patterns
and TcIteratedLambdas cenv isFirst (env:tcEnv) ty takenNames tpenv e = 
    match e with 
    | Expr_lambda (isMember,isSubsequent,spats,bodyExpr,m) when isMember || isFirst || isSubsequent ->
        let domainTy,resultTy = UnifyFunctionType None cenv env.DisplayEnv m ty
        let vs, (tpenv,names,takenNames) = TcSimplePats cenv isMember domainTy env (tpenv,Map.empty,takenNames) spats
        let envinner,_,vspecMap = MakeAndPublishSimpleVals cenv env m names
        let envinner = if isMember then envinner else ExitFamilyRegion envinner
        let bodyExpr,tpenv = TcIteratedLambdas cenv false envinner resultTy takenNames tpenv bodyExpr
        mk_multi_lambda m (List.map (fun nm -> NameMap.find nm vspecMap) vs) (bodyExpr,resultTy),tpenv 
    | e -> 
        TcExpr cenv ty env tpenv e

// Check expr.[idx] 
// This is a little over complicated for my liking. Basically we want to intepret e1.[idx] as e1.Item(idx). 
// However it's not so simple as all that. First "Item" can have a different name according to an attribute in 
// .NET metadata. Next, we want to give good warning messages for F#'s "old" way of doing things for OCaml 
// compatibility. This means we manually typecheck 'e1' and look to see if it has a nominal type. We then 
// do the right thing in each case. 
and TcIndexerThen cenv env ty m mDot tpenv expr e1 idxs delayed = 
    
    let ad = AccessRightsOfEnv env
    let e1',e1ty,tpenv = TcExprOfUnknownType cenv env tpenv e1
    
    // Find the first type in the effective hierarchy that either has a DefaultMember attribute OR 
    // has a member called 'Item' 
    let propName = 
        match idxs with 
        | [_] -> 
            FoldPrimaryHierarchyOfType (fun typ acc -> 
                match acc with
                | None ->
                    let isNominal = is_stripped_tyapp_typ cenv.g typ
                    if isNominal then 
                        let tcref = tcref_of_stripped_typ cenv.g typ
                        TyconRefTryBindAttrib cenv.g cenv.g.attrib_DefaultMemberAttribute tcref 
                                 (function ([CustomElem_string (Some(msg)) ],_) -> Some msg | _ -> None)
                                 (function (Attrib(_,_,[ AttribStringArg(msg) ],_,_))  -> Some(msg) | _ -> None)
                     else
                        match AllPropInfosOfTypeInScope cenv.infoReader (nenv_of_tenv env).eExtensionMembers (Some("Item"), ad) IgnoreOverrides m typ with
                        | [] -> None
                        | _ -> Some("Item")
                 | _ -> acc)
              cenv.g 
              cenv.amap 
              m 
              e1ty
              None
        | _ -> Some "GetSlice"

    let isNominal = is_stripped_tyapp_typ cenv.g e1ty
    
    
    (* NOTE: It looks like we need to make "array" different on this codepath *)
    (* NOTE: Array lookups should map to ArrayGet nodes in quotation trees. This is not currently happening *)
    (*       because they map to one of two (.[]) operators (one for legacy non-nominal lookups and one for nominal lookups *)
    (* NOTE: String lookups should map to direct calls to the Chars indexer property *)
    
    // REVIEW: it would be best to adjust this so that array goes through the nominal codepath 
    // so we can scrap the variable path altogether
    let isArray = IsArrayTypeWithIndexer cenv.g e1ty 
    let isString = type_equiv cenv.g cenv.g.string_ty e1ty 

    let idxRange = List.reduce_left union_ranges (List.map range_of_synexpr idxs)
    let MakeIndexParam vopt = 
        match idxs @ Option.to_list vopt with 
        | []  -> failwith "unexpected empty index list"
        | [h] -> Expr_paren(h,idxRange)
        | es -> Expr_paren(Expr_tuple(es,idxRange),idxRange)

    if isArray || isString then 

        let indexOpPath = ["Microsoft";"FSharp";"Core";"LanguagePrimitives";"IntrinsicFunctions"]
        let sliceOpPath = ["Microsoft";"FSharp";"Core";"Operators";"OperatorIntrinsics"]
        let mk_args es = Expr_tuple (es,m)
        let path,fnm,idxs = 
            match isString,isArray,expr with 
            | false,true,Expr_lbrack_get(_,[Expr_tuple ([_;_] as idxs,_)],_,_)         -> indexOpPath,"GetArray2D", idxs
            | false,true,Expr_lbrack_get(_,[Expr_tuple ([_;_;_] as idxs,_)],_,_)       -> indexOpPath,"GetArray3D", idxs
            | false,true,Expr_lbrack_get(_,[Expr_tuple ([_;_;_;_] as idxs,_)],_,_)     -> indexOpPath,"GetArray4D", idxs
            | false,true,Expr_lbrack_get(_,[_],_,_)                                    -> indexOpPath,"GetArray", idxs
            | false,true,Expr_lbrack_set(_,[Expr_tuple ([_;_] as idxs,_)] ,e3,_,_)    -> indexOpPath,"SetArray2D", (idxs @ [e3])
            | false,true,Expr_lbrack_set(_,[Expr_tuple ([_;_;_] as idxs,_)] ,e3,_,_)   -> indexOpPath,"SetArray3D", (idxs @ [e3])
            | false,true,Expr_lbrack_set(_,[Expr_tuple ([_;_;_;_] as idxs,_)] ,e3,_,_) -> indexOpPath,"SetArray4D", (idxs @ [e3])
            | false,true,Expr_lbrack_set(_,[_],e3,_,_)                        -> indexOpPath,"SetArray", (idxs @ [e3])
            | true,false,Expr_lbrack_get(_,[_;_],_,_)                -> sliceOpPath,"GetStringSlice", idxs
            | true,false,Expr_lbrack_get(_,[_],_,_)                  -> indexOpPath,"GetString", idxs
            | false,true,Expr_lbrack_get(_,[_;_],_,_)                -> sliceOpPath,"GetArraySlice", idxs
            | false,true,Expr_lbrack_get(_,[_;_;_;_],_,_)            -> sliceOpPath,"GetArraySlice2D", idxs
            | false,true,Expr_lbrack_get(_,[_;_;_;_;_;_],_,_)        -> sliceOpPath,"GetArraySlice3D", idxs
            | false,true,Expr_lbrack_get(_,[_;_;_;_;_;_;_;_],_,_)    -> sliceOpPath,"GetArraySlice4D", idxs
            | false,true,Expr_lbrack_set(_,[_;_],e3,_,_)             -> sliceOpPath,"SetArraySlice", (idxs @ [e3])
            | false,true,Expr_lbrack_set(_,[_;_;_;_],e3,_,_)         -> sliceOpPath,"SetArraySlice2D", (idxs @ [e3])
            | false,true,Expr_lbrack_set(_,[_;_;_;_;_;_],e3,_,_)     -> sliceOpPath,"SetArraySlice3D", (idxs @ [e3])
            | false,true,Expr_lbrack_set(_,[_;_;_;_;_;_;_;_],e3,_,_) -> sliceOpPath,"SetArraySlice4D", (idxs @ [e3])
            | _ -> error(Error("invalid indexer expression",m))
        let operPath = (mksyn_lid_get mDot path (CompileOpName fnm))
        let f,fty,tpenv = TcExprOfUnknownType cenv env tpenv operPath
        let domainTy,resultTy = UnifyFunctionType (Some m) cenv env.DisplayEnv m fty
        unifyE cenv env m domainTy e1ty; 
        let f' = build_app cenv (MkApplicableExprNoFlex cenv f) fty e1' m
        let delayed = DelayedApp(ExprAtomicFlag.Atomic,(match idxs with [idx] -> idx | _ -> Expr_tuple(idxs,m)),m) :: delayed // atomic, otherwise no ar.[1] <- xyz
        PropagateThenTcDelayed cenv ty env tpenv m f' resultTy ExprAtomicFlag.Atomic delayed 

    elif (isNominal || isSome propName) then 

        let nm = 
            match propName with 
            | None -> "Item"
            | Some(nm) -> nm
        let delayed = 
            match expr with 
            | Expr_lbrack_get _ -> 
                DelayedDotLookup([ident(nm,m)],m) :: DelayedApp(ExprAtomicFlag.Atomic,MakeIndexParam None,m) :: delayed
            | Expr_lbrack_set(_,_,e3,_,_) -> 
                match idxs with 
                | [idx] -> DelayedDotLookup([ident(nm,m)],m) :: DelayedApp(ExprAtomicFlag.Atomic,MakeIndexParam None,m) :: MakeDelayedSet(e3,m) :: delayed
                | _ -> DelayedDotLookup([ident("SetSlice",m)],m) :: DelayedApp(ExprAtomicFlag.Atomic,MakeIndexParam (Some e3),m) :: delayed
                
            | _ -> error(InternalError("unreachable",m))
        PropagateThenTcDelayed cenv ty env tpenv mDot (MkApplicableExprNoFlex cenv e1') e1ty ExprAtomicFlag.Atomic delayed 

    else 
        // Build an old fashioned, deprecated constrained lookup 
        error(Error("The operator 'expr.[idx]' has been used an object of indeterminate type based on information prior to this program point. This is deprecated. Consider adding further type constraints",m));


(* Check a 'new Type(args)' expression, also an 'inheritedTys declaration in an implicit or explicit class *)
and TcNewExpr cenv env tpenv objTy objTyRangeOpt superInit arg m =
    let ad = AccessRightsOfEnv env
    (* Handle the case 'new 'a()' *)
    if (is_typar_typ cenv.g objTy) then 
        if superInit then error(Error("cannot inherit from a variable type",m));
        AddCxTypeMustSupportDefaultCtor env.DisplayEnv cenv.css m NoTrace objTy;
        
        match arg with 
        | Expr_const (Const_unit,_) -> ()
        | _ -> errorR(Error("Calls to object constructors on type parameters can not be given arguments",m))
        
        mk_call_create_instance cenv.g m objTy ,tpenv
    else 
        if not (is_stripped_tyapp_typ cenv.g objTy) then error(Error(sprintf "'%s' may only be used with named types" (if superInit then "inherit" else "new"),m));
        let item,rest = ForceRaise (ResolveObjectConstructor cenv.nameResolver env.DisplayEnv m ad objTy)
        
        let nenv = nenv_of_tenv env
        // Re-record the name resolution since we now know it's a constructor call
        match objTyRangeOpt with 
        | Some objTyRange -> CallNameResolutionSink(objTyRange,nenv,item,ItemOccurence.Use,nenv.eDisplayEnv,AccessRightsOfEnv env);
        | None -> ()
        
        TcCtorCall false cenv env tpenv objTy objTy item superInit arg m (delay_rest rest m [])

(* Check an 'inheritedTys declaration in an implicit or explicit class *)
and TcCtorCall isNaked cenv env tpenv typ objTy item superInit arg m delayed =
    let ad = AccessRightsOfEnv env
    let isSuperInit = (if superInit then CtorValUsedAsSuperInit else NormalValUse)

    if is_interface_typ cenv.g objTy then 
      error(Error((if superInit then  "'inherit' may not be used on interface types. Consider implementing the interface by using 'interface ... with ... end' instead"
                   else            "'new' may not be used on interface types. Consider using an object expression '{ new ... with ... }' instead"),m));
    let tycon = (deref_tycon (tcref_of_stripped_typ cenv.g objTy))
    if not superInit && is_partially_implemented_tycon tycon then 
      error(Error("Instances of this type cannot be created since it has been marked 'abstract' or not all methods have been given implementations. Consider using an object expression '{ new ... with ... }' instead",m));
    match item with 
    | Item_ctor_group(methodName,minfos) ->

        let meths = List.map (fun minfo -> minfo,None) minfos
        if isNaked && type_feasibly_subsumes_type 0 cenv.g cenv.amap m cenv.g.system_IDisposable_typ NoCoerce objTy then
          warning(Error("It is recommended that objects that support the IDisposable interface are created using 'new Type(args)' rather than 'Type(args)' to indicate that resources may be owned by the generated value",m));
        TcMethodApplicationThen cenv env typ tpenv None [] m methodName ad PossiblyMutates false meths isSuperInit [arg] ExprAtomicFlag.NonAtomic delayed 
    | Item_delegate_ctor typ ->
        TcNewDelegateThen cenv objTy env tpenv m typ arg ExprAtomicFlag.NonAtomic delayed
    | _ -> error(Error(sprintf "'%s' may only be used to construct object types" (if superInit then "inherit" else "new"),m))


//-------------------------------------------------------------------------
// TcRecordConstruction
//------------------------------------------------------------------------- 
  
// Check a record consutrction expression 
and TcRecordConstruction cenv ty env tpenv optOrigExpr objTy fldsList m =
    let tcref = tcref_of_stripped_typ cenv.g objTy
    let tycon = deref_tycon tcref
    let tinst = Tastops.tinst_of_stripped_typ cenv.g objTy
    unifyE cenv env m ty objTy;

    // Types with implicit constructors can't use record or object syntax: all constructions must go through the implicit constructor 
    if tycon.TypeContents.tcaug_adhoc |> NameMultiMap.existsInRange (fun v -> v.IsIncrClassConstructor) then 
        errorR(Error(sprintf "Constructors for the type '%s' must directly or indirectly call its implicit object constructor. Use a call to the implicit object constructor instead of a record expression" (tycon.DisplayName),m));
                
    let fspecs = tycon.TrueInstanceFieldsAsList
    // Freshen types and work out their subtype flexibility
    let fldsList = 
        fldsList |> List.map (fun (fname,fexpr) -> 
              let fspec = try  fspecs |> List.find (fun fspec -> fspec.Name = fname) 
                          with Not_found -> error (Error("The field '" ^ fname ^ "' has been been given a value, but is not present in the type '"^NicePrint.pretty_string_of_typ env.DisplayEnv objTy^"'",m))
              let (declaredTypars,ftinst,fty) = FreshenPossibleForallTy cenv.g m TyparRigid (actual_typ_of_rfield tycon tinst fspec)
              let flex = not (is_typar_typ cenv.g fty)
              (fname,fexpr,declaredTypars,ftinst,fty,flex))

    // Type check and generalize the supplied bindings 
    let fldsList,tpenv = 
        (tpenv,fldsList) ||> List.mapfold (fun tpenv (fname,fexpr,declaredTypars,ftinst,fty,flex) -> 
              let fieldExpr,tpenv = TcExprFlex cenv flex fty env tpenv fexpr
              (* Polymorphic fields require generializeable expressions. *)
              if nonNil(declaredTypars) then
              
                  (* Canonicalize constraints prior to generalization *)
                  let denv = env.DisplayEnv
                  GeneralizationHelpers.CanonicalizePartialInferenceProblem (cenv,denv,m) declaredTypars;

                  let freeInEnv = GeneralizationHelpers.ComputeUngeneralizableTypars env
                  let generalizedTypars = GeneralizationHelpers.ComputeGeneralizedTypars(cenv,env,m,true,freeInEnv,false,CanGeneralizeConstrainedTypars,OptionalInline,Some(fieldExpr),declaredTypars,[],fty,false)
                  (fname,mk_tlambda m declaredTypars (fieldExpr,fty)), tpenv
              else  
                  (fname,fieldExpr),tpenv)
              
    (* Add rebindings for unbound field when an "old value" is available *)
    let oldFldsList = 
      match optOrigExpr with
      | None -> []
      | Some (_,_,oldve') -> 
             (* When we have an "old" value, append bindings for the unbound fields. *)
             (* Effect order - mutable fields may get modified by other bindings... *)
             let fieldNameUnbound nom = List.forall (fun (name,exp) -> name <> nom) fldsList
             fspecs 
             |> List.filter (fun rfld -> rfld.Name |> fieldNameUnbound)
             |> List.map (fun fspec ->
                 (* CODE REVIEW: check next line is ok to be repeated *)
                 let (_,ftinst,fty) = FreshenPossibleForallTy cenv.g m TyparRigid (actual_typ_of_rfield tycon tinst fspec)
                 fspec.Name, mk_recd_field_get cenv.g (oldve',rfref_of_rfield tcref fspec,tinst,ftinst,m))

    let fldsList = fldsList @ oldFldsList

    (* From now on only interested in fspecs that truly need values. *)
    let fspecs = fspecs |> List.filter (fun f -> not f.IsZeroInit)
    
    (* Check all fields are bound *)
    fspecs |> List.iter (fun fspec ->
      if not (fldsList |> List.exists (fun (fname,fexpr) -> fname = fspec.Name)) then
        error(Error("no assignment given for field '"^fspec.rfield_id.idText^"'",m)));

    (* Other checks (overlap with above check now clear) *)
    if isNone optOrigExpr then begin
      let ns1 = Nameset.of_list (List.map fst fldsList)
      let ns2 = Nameset.of_list (List.map (fun x -> x.rfield_id.idText) fspecs)
      if  not (Zset.subset ns2 ns1) then 
        error (MissingFields(Zset.elements (Zset.diff ns2 ns1),m));
      if  not (Zset.subset ns1 ns2) then 
        error (Error("Extraneous fields have been given values",m));
    end;
    (* Build record *)
    let rfrefs = List.map (fst >> mk_rfref tcref) fldsList

    (* Check accessibility: this is also done in BuildFieldMap, but also need to check *)
    (* for fields in { new R with a=1 and b=2 } constructions and { r with a=1 }  copy-and-update expressions *)
    rfrefs |> List.iter (fun rfref -> 
        CheckRecdFieldAccessible m (AccessRightsOfEnv env) rfref |> ignore;
        CheckFSharpAttributes cenv.g rfref.PropertyAttribs m |> CommitOperationResult);        

    let args   = List.map snd fldsList
    
    let expr = mk_recd cenv.g (GetRecdInfo env, tcref, tinst, rfrefs, args, m)

    let expr = 
      match optOrigExpr with 
      | None ->
          (* '{ recd fields }'. *)
          expr
          
      | Some (old',oldv',_) -> 
          (* '{ recd with fields }'. *)
          (* Assign the first object to a tmp and then construct *)
          mk_compgen_let m oldv' old' expr

    expr, tpenv

//-------------------------------------------------------------------------
// TcObjectExpr
//------------------------------------------------------------------------- 

and GetNameAndArityOfObjExprBinding cenv env b =
    let (NormalizedBinding (_,_,_,_,_,_,_,valSynData,pat,rhsExpr,bindingRange,_)) = b
    let (ValSynData(memberFlagsOpt,valSynInfo,_)) = valSynData 
    match pat,memberFlagsOpt with 

    // This is the normal case for F# 'with member x.M(...) = ...'
    | Pat_instance_member(thisId,memberId,None,m),Some(memberFlags) ->
         let logicalMethId = ident (ComputeLogicalCompiledName memberId memberFlags,memberId.idRange)
         logicalMethId.idText,valSynInfo

    | _ -> 
        // This is for the deprecated form 'with M(...) = ...'
        let rec lookPat pat =
            match pat with 
            | Pat_typed(pat,_,_) -> lookPat pat
            | Pat_as (Pat_wild _, id,_,None,_) -> 
                 (* let e = PushOnePatternToRhs (mksyn_this_pat_var (ident "_this" id.idRange)) e in  *)
                let (BindingRhs(pushedPats,_,_)) = rhsExpr
                let infosForExplicitArgs = pushedPats |> List.map SynInfo.InferArgSynInfoFromSimplePats
                let infosForExplicitArgs = SynInfo.AdjustMemberArgs MemberKindMember infosForExplicitArgs
                let infosForExplicitArgs = SynInfo.AdjustArgsForUnitElimination infosForExplicitArgs 
                let argInfos = [SynInfo.selfMetadata] @ infosForExplicitArgs
                let retInfo = SynInfo.unnamedRetVal //SynInfo.InferSynReturnData pushedRetInfoOpt
                let valSynData = ValSynInfo(argInfos,retInfo)
                (id.idText,valSynData)

            | _ -> error(Error("Only overrides of abstract and virtual members may be specified in object expressions",bindingRange)); 

        lookPat pat


and FreshenObjExprAbstractSlot cenv (env:tcEnv) implty virtNameAndArityPairs (bind,bindName,absSlots:(_ * MethInfo) list) = 
    let (NormalizedBinding (_,_,_,_,_,_,synTyparDecls,valSynData,_,_,bindingRange,_)) = bind 
    match absSlots with 
    | [] -> 
        let absSlotsByName = List.filter (fst >> fst >> (=) bindName) virtNameAndArityPairs
        
        match absSlotsByName with 
        | []              -> errorR(Error(sprintf "The member %s does not correspond to any abstract or virtual method available to override or implement" bindName,bindingRange));
        | [(_,absSlot:MethInfo)]     -> errorR(Error(sprintf "The member %s does not accept the correct number of arguments, %d arguments are expected" bindName (List.sum absSlot.NumArgs),bindingRange));
        | (_,absSlot:MethInfo) :: _  -> errorR(Error(sprintf "The member %s does not accept the correct number of arguments. One overload accepts %d arguments" bindName (List.sum absSlot.NumArgs),bindingRange));
        
        None
        
    | [(_,absSlot)] -> 
        (* dprintf "nm = %s, #fmtps = %d\n" absSlot.LogicalName (List.length fmtps); *) 
        
        let typarsFromAbsSlotAreRigid,typarsFromAbsSlot,argTysFromAbsSlot, retTyFromAbsSlot
           = FreshenAbstractSlot cenv.g cenv.amap bindingRange synTyparDecls absSlot

        // Work out the required type of the member 
        let bindingTy = implty --> (mk_meth_ty cenv.g argTysFromAbsSlot retTyFromAbsSlot) 
        
        Some(typarsFromAbsSlotAreRigid,typarsFromAbsSlot,bindingTy)
        
    | (_,absSlot1) :: (_,absSlot2) :: _ -> 
        //warning(NonUniqueInferredAbstractSlot(cenv.g,env.DisplayEnv, bindName, absSlot1, absSlot2,bindingRange));
        //fail()
        None


and TcObjectExprBinding cenv (env:tcEnv) tpenv (absSlotInfo,bind) =
    // 4a1. normalize the binding (note: needlessly repeating what we've done above) 
    let (NormalizedBinding(vis,bkind,pseudo,mut,attrs,doc,synTyparDecls,valSynData,p,bindingRhs,bindingRange,spBind)) = bind
    let (ValSynData(memberFlagsOpt,_,_)) = valSynData 
    // 4a2. adjust the binding, especially in the "member" case, a subset of the logic of AnalyzeAndMakeRecursiveValue 
    let bindingRhs,logicalMethId,memberFlags = 
        let rec pat p = 
            match p,memberFlagsOpt with  
            | Pat_as (Pat_wild _, id,_,_,_),None -> 
                let bindingRhs = PushOnePatternToRhs true (mksyn_this_pat_var (ident (CompilerGeneratedName "this",id.idRange))) bindingRhs 
                let logicalMethId = id
                let memberFlags = OverrideMemberFlags None MemberKindMember
                bindingRhs,logicalMethId,memberFlags

            | Pat_instance_member(thisId, memberId,_,_),Some(memberFlags) -> 
                CheckMemberFlags cenv.g None  NewSlotsOK OverridesOK memberFlags bindingRange;
                let bindingRhs = PushOnePatternToRhs true (mksyn_this_pat_var thisId) bindingRhs
                let logicalMethId = ident (ComputeLogicalCompiledName memberId memberFlags,memberId.idRange)
                bindingRhs,logicalMethId,memberFlags
            | _ -> 
                error(InternalError("unexpect member binding",bindingRange))
        pat p
    let bind = NormalizedBinding (vis,bkind,pseudo,mut,attrs,doc,synTyparDecls,valSynData,mksyn_pat_var vis logicalMethId,bindingRhs,bindingRange,spBind) 
    
    (* 4b. typecheck the binding *)
    let bindingTy = 
        match absSlotInfo with
        | Some(_,_,memberTyFromAbsSlot) -> memberTyFromAbsSlot
        | _ -> new_inference_typ cenv ()

    let (TBindingInfo(inlineFlag,immut,_,_,_,ExplicitTyparInfo(declaredTypars,_),nameToPrelimValSchemeMap,rhsExpr,_,_,m,_,_,_),tpenv) = 
        let flex = TcNonrecBindingTyparDecls cenv env tpenv bind
        TcNormalizedBinding ObjectExpressionOverrideBinding cenv env tpenv bindingTy None ([],flex) bind

    (* 4c. generalize the binding - only relevant when implementing a generic virtual method *)
    
    match NameMap.range nameToPrelimValSchemeMap with 
    | [PrelimValScheme1(id,_,_,_,_,_,_,_,_,_,_)] -> 
        let denv = env.DisplayEnv

        let declaredTypars = 
            match absSlotInfo with
            | Some(typarsFromAbsSlotAreRigid,typarsFromAbsSlot,_) -> 
                if typarsFromAbsSlotAreRigid then typarsFromAbsSlot else declaredTypars
            | _ -> 
                declaredTypars
        (* Canonicalize constraints prior to generalization *)
        GeneralizationHelpers.CanonicalizePartialInferenceProblem (cenv,denv,m) declaredTypars;

        let freeInEnv = GeneralizationHelpers.ComputeUngeneralizableTypars env

        (* dprintf "id = %s, typarsFromAbsSlotAreRigid = %b, #declaredTypars = %d, #freeInEnv = %d\n" id.idText typarsFromAbsSlotAreRigid (List.length declaredTypars) (List.length (Zset.elements freeInEnv)); *) 

        let generalizedTypars = GeneralizationHelpers.ComputeGeneralizedTypars(cenv,env,m,immut,freeInEnv,false,CanGeneralizeConstrainedTypars,inlineFlag,Some(rhsExpr),declaredTypars,[],bindingTy,false)
        let declaredTypars = ChooseCanonicalDeclaredTyparsAfterInference cenv.g  env.DisplayEnv declaredTypars m

        (* dprintf "ungen = %b\n" (declaredTypars |> List.exists (Zset.mem_of freeInEnv)); *) 

        let generalizedTypars = PlaceTyparsInDeclarationOrder declaredTypars generalizedTypars  
        (* dprintf "#generalizedTypars = %d\n" (List.length generalizedTypars); *) 
        (id,memberFlags,(generalizedTypars +-> bindingTy),rhsExpr),tpenv
    | _ -> error(Error("A simple method name is required here",m))
    
and ComputeObjectExprOverrides cenv (env:tcEnv) tpenv impls =
    let slotImplSets = DispatchSlotChecking.GetSlotImplSets cenv.infoReader env.DisplayEnv true (impls |> List.map (fun (m,ty,_) -> ty,m))

    let allImpls = 
        (impls,slotImplSets) ||>  List.map2 (fun (m,ty,binds) implTySet -> 
            let binds = binds |> List.map (BindingNormalization.NormalizeBinding ObjExprBinding cenv env)
            m, ty,binds,implTySet) 

    let overridesAndVirts,tpenv = 
        (tpenv,allImpls) ||>  List.mapfold (fun tpenv (m,implty,binds, SlotImplSet(dispatchSlots,dispatchSlotsKeyed,availPriorOverrides,_) ) ->
                
            // 2. collect all name/arity of all overrides 
            let virtNameAndArityPairs = dispatchSlots |> List.map (fun virt -> 
                let vkey = (virt.LogicalName,virt.NumArgs) 
                //dprintfn "vkey = %A" vkey
                (vkey,virt)) 
            let bindNameAndSynInfoPairs = binds |> List.map (GetNameAndArityOfObjExprBinding cenv env) 
            let bindNames = bindNameAndSynInfoPairs |> List.map fst
            let bindKeys = 
                bindNameAndSynInfoPairs |> List.map (fun (name,valSynData) -> 
                    // Compute the argument counts of the member arguments
                    let argCounts = (SynInfo.AritiesOfArgs valSynData).Tail
                    //dprintfn "name = %A, argCounts = %A" name argCounts
                    (name,argCounts))

            // 3. infer must-have types by name/arity 
            let preAssignedVirtsPerBinding = 
                bindKeys |> List.map (fun bkey  -> List.filter (fst >> (=) bkey) virtNameAndArityPairs) 
            
            let absSlotInfo = 
               (List.zip3 binds bindNames preAssignedVirtsPerBinding)  
               |> List.map (FreshenObjExprAbstractSlot cenv env implty virtNameAndArityPairs)

            (* 4. typecheck/typeinfer/generalizer overrides using this information *)
            let overrides,tpenv = (tpenv,List.zip absSlotInfo binds) ||> List.mapfold (TcObjectExprBinding cenv env)

            (* Convert the syntactic info to actual info *)
            let overrides = 
                (overrides,bindNameAndSynInfoPairs) ||> List.map2 (fun (id:ident,memberFlags,ty,e) (_,valSynData) -> 
                    let partialValInfo = TranslateTopValSynInfo id.idRange (TcAttributes cenv env) valSynData
                    let tps,_ = try_dest_forall_typ cenv.g ty
                    let valInfo = TranslatePartialArity tps partialValInfo
                    DispatchSlotChecking.GetObjectExprOverrideInfo cenv.g cenv.amap (implty,id,memberFlags,ty,valInfo,e))

            (m,implty,dispatchSlots,dispatchSlotsKeyed,availPriorOverrides,overrides),tpenv)

    overridesAndVirts,tpenv

and CheckSuperType cenv typ m = 
    if type_equiv cenv.g typ cenv.g.system_Value_typ ||
       type_equiv cenv.g typ cenv.g.system_Enum_typ ||
       type_equiv cenv.g typ cenv.g.system_Array_typ ||
       type_equiv cenv.g typ cenv.g.system_MulticastDelegate_typ ||
       type_equiv cenv.g typ cenv.g.system_Delegate_typ then 
         error(Error("The types System.ValueType, System.Enum, System.Delegate, System.MulticastDelegate and System.Array may not be used as super types in an object expression or class",m));
       
   
and TcObjectExpr cenv ty env tpenv (objTy,argopt,binds,extraImpls,m) = 
    let objTy',tpenv = TcType cenv NewTyparsOK CheckCxs env tpenv objTy
    if not (is_stripped_tyapp_typ cenv.g objTy') then error(Error("'new' must be used with a named type",m));
    if not (is_recd_typ cenv.g objTy') && not (is_interface_typ cenv.g objTy') && is_sealed_typ cenv.g objTy' then errorR(Error("Cannot create an extension of a sealed type",m));
    
    CheckSuperType cenv objTy' (range_of_syntype objTy); 
       
    (* Object expression members can access protected members of the implemented type *)
    let env = EnterFamilyRegion (tcref_of_stripped_typ cenv.g objTy') env
    let ad = AccessRightsOfEnv env
    
    if (* record construction *)
       (is_recd_typ cenv.g objTy') || 
       (* object construction *)
       (is_fsobjmodel_typ cenv.g objTy' && not (is_interface_typ cenv.g objTy') && isNone argopt) then  

        if isSome argopt then error(Error("No arguments may be given when constructing a record value",m));
        if nonNil extraImpls then error(Error("Interface implementations may not be given on construction expressions",m));
        if is_fsobjmodel_typ cenv.g objTy' && GetCtorShapeCounter env <> 1 then 
            error(Error("Object construction expressions may only be used to implement constructors in class types",m));
        let fldsList = 
            binds |> List.map (fun b -> 
                match BindingNormalization.NormalizeBinding ObjExprBinding cenv env b with 
                | NormalizedBinding (_,_,_,_,[],_,_,_,Pat_as(Pat_wild _, id,_,_,_),BindingRhs(_,_,rhsExpr),_,_) -> id.idText,rhsExpr
                | _ -> error(Error("Only simple bindings of the form 'id = expr' can be used in construction expressions",m)))
        
        TcRecordConstruction cenv ty env tpenv None objTy' fldsList m
    else
        let item,rest = ForceRaise (ResolveObjectConstructor cenv.nameResolver env.DisplayEnv m ad objTy')

        if nonNil rest then error(InternalError("Unexpected rest from ResolveObjectConstructor",m));

        if is_fsobjmodel_typ cenv.g objTy' && GetCtorShapeCounter env = 1 then 
            error(Error("Objects must be initialized by an object construction expression that calls an inherited object constructor and assigns a value to each field",m));

      (* Work out the type of any interfaces to implement *)
        let extraImpls,tpenv = 
          (tpenv , extraImpls) ||> List.mapfold (fun tpenv (InterfaceImpl(ity,overrides,m)) -> 
              let ity',tpenv = TcType cenv NewTyparsOK CheckCxs env tpenv ity
              if not (is_interface_typ cenv.g ity') then
                error(Error("Expected an interface type",m));
              (m,ity',overrides),tpenv)

        let realObjTy = (if is_obj_typ cenv.g objTy' && nonNil extraImpls then (p23 (List.hd extraImpls)) else objTy')
        unifyE cenv env m ty realObjTy;

        let ctorCall,baseIdOpt,tpenv =
            match item,argopt with 
            | Item_ctor_group(methodName,minfos),Some (arg,baseIdOpt) -> 
                let meths = minfos |> List.map (fun minfo -> minfo,None) 
                let ad = AccessRightsOfEnv env
                let expr,tpenv = TcMethodApplicationThen cenv env objTy' tpenv None [] m methodName ad PossiblyMutates false meths CtorValUsedAsSuperInit [arg] ExprAtomicFlag.Atomic [] 
                // The 'base' value is always bound
                let baseIdOpt = (match baseIdOpt with None -> Some(ident("base",m)) | Some id -> Some(id))
                expr,baseIdOpt,tpenv
            | Item_fake_intf_ctor ityp,None -> 
                unifyE cenv env m objTy' ityp;
                let expr = mk_obj_ctor_call cenv.g m
                expr,None,tpenv
            | Item_fake_intf_ctor _,Some _ -> 
                error(Error("Constructor expressions for interfaces do not take arguments",m));
            | Item_ctor_group _,None -> 
                error(Error("This object constructor requires arguments",m));
            | _ -> error(Error("'new' may only be used with object constructors",m))

        let baseValOpt = MakeAndPublishBaseVal cenv env baseIdOpt objTy'
        let env = Option.fold_right (AddLocalVal m) baseValOpt env
        
        
        let impls = (m,objTy',binds) :: extraImpls
        
        
        (* 1. collect all the relevant abstract slots for each type we have to implement *)
        
        let overridesAndVirts,tpenv = ComputeObjectExprOverrides cenv env tpenv impls

    
        overridesAndVirts |> List.iter (fun (m,implty,dispatchSlots,dispatchSlotsKeyed,availPriorOverrides,overrides) -> 
            let ovspecs = overrides |> List.map fst
            DispatchSlotChecking.CheckOverridesAreAllUsedOnce env.DisplayEnv cenv.g cenv.amap (m,implty,dispatchSlotsKeyed,ovspecs);
            DispatchSlotChecking.CheckDispatchSlotsAreImplemented (env.DisplayEnv,cenv.g,cenv.amap,m,false,implty,dispatchSlots,availPriorOverrides,ovspecs) |> ignore);
        
      (* 6c. create the specs of overrides *)
        let allTypeImpls = 
          overridesAndVirts |> List.map (fun (m,implty,_,dispatchSlotsKeyed,_,overrides) -> 
              let overrides' = overrides |> List.map (fun overrideMeth -> 
                    let (Override(_, id,(mtps,_),_,_,_) as ovinfo),(_,thisVal,methodVars,body) = overrideMeth
                    let overridden = 
                        match NameMultiMap.find id.idText dispatchSlotsKeyed |> List.tryfind (fun virt -> DispatchSlotChecking.is_exact_match cenv.g cenv.amap m virt ovinfo) with 
                        | Some x -> x
                        | None -> error(Error("At least one override did not correctly implement its corresponding abstract member",range_of_syntype objTy))
                    //let _,_,methodVars,body',_ = IteratedAdjustArityOfLambdaBody cenv.g (List.map List.length vs) vs body 
                    TObjExprMethod(SlotSigOfMethodInfo cenv.amap m overridden, mtps, [thisVal]::methodVars,body,id.idRange))
              (implty,overrides'))
            
        let (objTy',overrides') = allTypeImpls.Head
        let extraImpls = allTypeImpls.Tail
        
      (* 7. Build the implementation *)
        let expr = mk_obj_expr(objTy', baseValOpt, ctorCall, overrides',extraImpls,m)
        let expr = mk_coerce_if_needed cenv.g realObjTy objTy' expr
        expr,tpenv



//-------------------------------------------------------------------------
// TcConstStringExpr
//------------------------------------------------------------------------- 

(* Check a constant string expression. It might be a 'printf' format string *)
and TcConstStringExpr cenv ty env m tpenv s  =

    if (AddCxTypeEqualsTypeUndoIfFailed env.DisplayEnv cenv.css m ty cenv.g.string_ty) then 
      mk_string cenv.g m s,tpenv
    else 
      let aty = new_inference_typ cenv ()
      let bty = new_inference_typ cenv ()
      let cty = new_inference_typ cenv ()
      let dty = new_inference_typ cenv ()
      let ety = new_inference_typ cenv ()
      let ty' = mk_format_ty cenv.g aty bty cty dty ety
      if (not (is_obj_typ cenv.g ty) && AddCxTypeMustSubsumeTypeUndoIfFailed env.DisplayEnv cenv.css m ty ty') then 
        // Parse the format string to work out the phantom types 
        let aty',ety' = (try Formats.ParseFormatString m cenv.g s bty cty dty with Failure s -> error (Error(s,m)))
        unifyE cenv env m aty aty';
        unifyE cenv env m ety ety';
        mk_call_new_format cenv.g m aty bty cty dty ety (mk_string cenv.g m s),tpenv
      else 
        unifyE cenv env m ty cenv.g.string_ty;
        mk_string cenv.g m s,tpenv

//-------------------------------------------------------------------------
// TcConstExpr
//------------------------------------------------------------------------- 

(* Check a constant expression. *)
and TcConstExpr cenv ty env m tpenv c  =
    match c with 

    (* NOTE: these aren't "really" constants *)
    | Const_bytearray (bytes,m) -> 
       unifyE cenv env m ty (mk_bytearray_ty cenv.g); 
       TExpr_op(TOp_bytes bytes,[],[],m),tpenv

    | Const_uint16array arr -> 
       unifyE cenv env m ty (mk_array_typ cenv.g cenv.g.uint16_ty); TExpr_op(TOp_uint16s arr,[],[],m),tpenv

    | Const_bignum (s,suffix) -> 
        let expr = 
            let modName = ("NumericLiteral"^suffix)
            let ad = AccessRightsOfEnv env
            match ResolveLongIndentAsModuleOrNamespace OpenQualified env.eNameResEnv ad [ident (modName,m)] with 
            | Result []
            | Exception _ -> error(Error(sprintf "This numeric literal requires that a module '%s' defining functions FromZero, FromOne, FromInt32, FromInt64 and FromString be in scope" modName,m))
            | Result ((_,mref,mtyp) :: _) -> 
                let expr = 
                    try 
                        let i32 = int32 s  
                        if i32 = 0 then Expr_app(ExprAtomicFlag.Atomic, mksyn_lid_get m [modName] "FromZero",Expr_const(Const_unit,m),m)
                        elif i32 = 1 then Expr_app(ExprAtomicFlag.Atomic, mksyn_lid_get m [modName] "FromOne",Expr_const(Const_unit,m),m)
                        else Expr_app(ExprAtomicFlag.Atomic, mksyn_lid_get m [modName] "FromInt32",Expr_const(Const_int32 i32,m),m)
                    with _ -> 
                      try 
                         let i64 = int64 s  
                         Expr_app(ExprAtomicFlag.Atomic, mksyn_lid_get m [modName] "FromInt64",Expr_const(Const_int64 i64,m),m)
                      with _ ->             
                        Expr_app(ExprAtomicFlag.Atomic, mksyn_lid_get m [modName] "FromString",Expr_const(Const_string (s,m),m),m) 
                let ccu = ccu_of_tcref mref
                if isSome ccu && ccu_eq ccu.Value cenv.g.fslibCcu && suffix = "I" then 
                    Expr_typed(expr,Type_lid(path_to_lid m ["Microsoft";"FSharp";"Math";"bigint"],m),m)
                else
                    expr

        TcExpr cenv ty env tpenv expr

    | _ -> 
        let c' = TcConst cenv ty m env c
        TExpr_const (c',m,ty),tpenv


//-------------------------------------------------------------------------
// TcAssertExpr
//------------------------------------------------------------------------- 

(* Check an 'assert(x)' expression. *)
and TcAssertExpr cenv ty env m tpenv x  =
    let callDiagnosticsExpr = Expr_app(ExprAtomicFlag.Atomic, mksyn_lid_get m ["System";"Diagnostics";"Debug"] "Assert", 
                                       (* wrap an extra parentheses so 'assert(x=1) isn't considered a named argument to a method call *)
                                       Expr_paren(x,m), m)

    TcExpr cenv ty env tpenv callDiagnosticsExpr



//-------------------------------------------------------------------------
// TcRecdExpr
//------------------------------------------------------------------------- 

and TcRecdExpr cenv ty env tpenv (inherits,optOrigExpr,flds,m) =

    let requiresCtor = (GetCtorShapeCounter env = 1) (* Get special expression forms for constructors *)
    let haveCtor = (isSome inherits)

    let optOrigExpr,tpenv = 
      match optOrigExpr with 
      | None -> None, tpenv 
      | Some e -> 
          if isSome inherits then error(Error("Invalid record construction",m));
          let e',tpenv = TcExpr cenv ty env tpenv e
          let v',ve' = Tastops.mk_compgen_local m "inputRecord" ty
          Some (e',v',ve'), tpenv

    let fldsList = 
        match flds with 
        | [] -> []
        | _ -> 
            let tcref,fldsmap,fldsList = BuildFieldMap cenv env (isSome(optOrigExpr)) ty flds m
            let _,_,_,gtyp = info_of_tcref cenv m env tcref
            unifyE cenv env m ty gtyp;      
            fldsList

    if isSome optOrigExpr && not (is_recd_typ cenv.g ty) then 
        errorR(Error("The expression form { expr with ... } may only be used with record types. To build object types use { new Type(...) with ... }",m));

    if requiresCtor || haveCtor then 
        if not (is_fsobjmodel_typ cenv.g ty) then 
            // Deliberate no-recovery failure here to prevent cascading internal errors
            error(Error("The inherited type is not an object model type",m));
        if not requiresCtor then 
            errorR(Error("Object construction expressions (i.e. record expressions with inheritance specifications) may only be used to implement constructors in object model types. Use 'new ObjectType(args)' to construct instances of object model types outside of constructors",m));
    else
        if isNil flds then error(Error("'{ }' is not a valid expression. Records must include at least one field. Empty sequences are specified by using Seq.empty or an empty list '[]'",m));
        if is_fsobjmodel_typ cenv.g ty then errorR(Error("This type is not a record type. Values of class and struct types must be created using calls to object constructors",m))
        elif not (is_recd_typ cenv.g ty) then errorR(Error("This type is not a record type",m));

    let superTy,tpenv = 
        match inherits, SuperTypeOfType cenv.g cenv.amap m ty with 
        | Some (superTyp,arg,m), Some(realSuperTyp) ->
            (* Constructor expression, with an explicit 'inheritedTys clause. Check the inherits clause. *)
            let e,tpenv = TcExpr cenv realSuperTyp  env tpenv (Expr_new(true,superTyp,arg,m))
            Some(e),tpenv
        | None, Some(realSuperTyp) when requiresCtor -> 
            (* Constructor expression, No 'inheritedTys clause, hence look for a default constructor *)
            let e,tpenv = TcNewExpr cenv env tpenv realSuperTyp None true (Expr_const (Const_unit,m)) m
            Some(e),tpenv
        | None,_ -> 
            None,tpenv
        | _, None -> 
            errorR(InternalError("Unexpected failure in getting super type",m));
            None,tpenv

    let expr,tpenv = 
        TcRecordConstruction cenv ty env tpenv optOrigExpr  ty fldsList m

    let expr = 
        match superTy with 
        | _ when is_struct_typ cenv.g ty -> expr
        | Some(e) -> mk_compgen_seq m e expr
        | None -> expr
    expr,tpenv


//-------------------------------------------------------------------------
// TcForEachExpr 
//------------------------------------------------------------------------- 
 
and TcForEachExpr cenv ty env tpenv (pat,enumExpr,body,m,spForLoop)  =
    unifyE cenv env m ty cenv.g.unit_ty;
    let enumExpr',iety,tpenv = 
        let env = ExitFamilyRegion env
        TcExprOfUnknownType cenv env tpenv enumExpr
    let ienumeratorv, ienumeratore,_,enumElemTy,getEnumE,getEnumTy,guarde,_,currente = AnalyzeArbitraryExprAsEnumerable cenv env (range_of_expr enumExpr') iety enumExpr'
    let pat',_,vspecs,envinner,tpenv = TcMatchPattern cenv enumElemTy env tpenv (pat,None)
    let idv,ide,pat'' =      
        (* nice: don't introduce awful temporary for r.h.s. in the 99% case where we know what we're binding it to *)
        match pat' with
        | TPat_as (pat1,PBind(v,TypeScheme([],_,_)),m1) -> 
              v,expr_for_val v.Range v, pat1
        | _ -> 
              let tmp,tmpe = Tastops.mk_compgen_local m "forLoopVar" enumElemTy
              tmp,tmpe,pat'

(*      let idv,ide = Tastops.mk_local id.idRange id.idText enumElemTy in *)
(*      let envinner = AddLocalVal m idv env in  *)
    let body',tpenv = TcStmt cenv envinner tpenv body
    let body' = 
        let valsDefinedByMatching = FlatListSet.remove vspec_eq idv vspecs
        CompilePatternForMatch cenv env m m false Incomplete (idv,[]) 
            [TClause(pat'',None,TTarget(valsDefinedByMatching,body',SequencePointAtTarget),m);
             TClause(TPat_wild(range_of_expr body'),None,TTarget(FlatList.empty,mk_unit cenv.g m,SuppressSequencePointAtTarget),m) ] ty
    let forLoop =  
        match enumExpr' with 
        (* optimize 'for i in n .. m do' *)
        | TExpr_app(TExpr_val(vf,_,_),_,[tyarg],[start';finish'],_) 
             when cenv.g.vref_eq vf cenv.g.range_op_vref && type_equiv cenv.g tyarg cenv.g.int_ty -> 
               mk_fast_for_loop  cenv.g (spForLoop,m,idv,start',true,finish',body')
        | _ -> 

            let cleanupE = BuildDisposableCleanup cenv env m ienumeratorv
            let spBind = (match spForLoop with SequencePointAtForLoop(spStart) -> SequencePointAtBinding(spStart) | NoSequencePointAtForLoop -> NoSequencePointAtStickyBinding)
            (mk_let spBind (range_of_expr getEnumE)  ienumeratorv getEnumE
                (mk_try_finally cenv.g (mk_while cenv.g (NoSequencePointAtWhileLoop, guarde,mk_compgen_let (range_of_expr body') idv currente body',m),cleanupE,m,cenv.g.unit_ty,NoSequencePointAtTry,NoSequencePointAtFinally)))
    forLoop, tpenv

//-------------------------------------------------------------------------
// TcQuotationExpr
//------------------------------------------------------------------------- 

and TcQuotationExpr cenv ty env tpenv (oper,raw,ast,m) =
    let astTy = new_inference_typ cenv ()

    // Assert the overall type for the domain of the quotation template
    unifyE cenv env m ty (if raw then mk_raw_expr_ty cenv.g else mk_expr_ty cenv.g astTy); 

    // Check the expression 
    let expr,tpenv = TcExpr cenv astTy env tpenv ast  
    
    // Wrap the expression
    let expr = TExpr_quote(expr,ref None, m,ty)

    // Coerce it if needed
    let expr = if raw then mk_coerce(expr,(mk_raw_expr_ty cenv.g),m,(type_of_expr cenv.g expr)) else expr

    // We serialize the quoted expression to bytes in Ilxgen after type inference etc. is complete. 
    expr,tpenv


//-------------------------------------------------------------------------
// TcComputationExpression
//------------------------------------------------------------------------- 

and TcComputationExpression cenv (env:tcEnv) ty m interpValOpt tpenv comp = 
    //dprintfn "TcComputationExpression, comp = \n%A\n-------------------\n" comp
    let ad = AccessRightsOfEnv env

    match interpValOpt with 
    
    // This case is used for all computation expressions except sequence expressions
    | Some (interpExpr,interpExprTy) -> 

        let interpVarName = CompilerGeneratedName "builder"
        let interpVarRange = range_of_expr interpExpr
        let interpVar = mksyn_item interpVarRange interpVarName

        let mksynCall nm m args = 
            let args = 
                match args with 
                | [] -> Expr_const(Const_unit,m)
                | [arg] -> Expr_paren(Expr_paren(arg,m),m)
                | args -> Expr_paren(Expr_tuple(args,m),m)
                
            Expr_app (ExprAtomicFlag.NonAtomic, Expr_lvalue_get(interpVar,[mksyn_id m nm], m), args,m)

        let rec tryTrans comp =
            match comp with 
            | Expr_foreach (spForLoop,SeqExprOnly(seqExprOnly),pat,pseudoEnumExpr,innerComp,_) -> 
                let m = (range_of_synexpr pseudoEnumExpr)
                let spBind = (match spForLoop with SequencePointAtForLoop(spStart) -> SequencePointAtBinding(spStart) | NoSequencePointAtForLoop -> NoSequencePointAtStickyBinding)
                Some(mksynCall "For" m [pseudoEnumExpr; mksyn_match_lambda(false,false,m,[Clause(pat,None, trans innerComp,m,SequencePointAtTarget)],spBind) ])

            | Expr_for (spBind,id,start,dir,finish,innerComp,m) ->
                Some(trans (elim_fast_integer_for_loop (spBind,id,start,dir,finish,innerComp,m)))

            | Expr_while (spWhile,guardExpr,innerComp,m) -> 
                let guardRange = (range_of_synexpr guardExpr)
                let innerRange = (range_of_synexpr innerComp)
                Some(mksynCall "While" m [mksyn_delay guardRange guardExpr; mksynCall "Delay" innerRange [mksyn_delay innerRange (trans innerComp)]])

            | Expr_try_finally (innerComp,unwindExpr,mTryToLast,spTry,spFinally) ->
                let m = (range_of_synexpr unwindExpr)
                Some(mksynCall "TryFinally" mTryToLast [mksynCall "Delay" mTryToLast [mksyn_delay (range_of_synexpr innerComp) (trans innerComp)]; mksyn_delay m unwindExpr])

            | Expr_paren (_,m) -> 
                error(Error("This construct is ambiguous as part of a computation expression. Nested expressions may be written using 'let _ = (...)' and nested computations using 'let! res = builder { ... }'",m))

            | Comp_zero m -> 
                Some(mksynCall "Zero" m [])

            // "do! expr; cexpr" is treated as { let! () = expr in cexpr }
            // "expr; cexpr" is treated as sequential execution
            // "cexpr; cexpr" is treated as builder.Combine(cexpr1,cexpr1)
            | Expr_seq(sp,true,innerComp1, innerComp2,m) -> 
                match tryTrans innerComp1 with 
                | Some c -> 
                    let m1 = (range_of_synexpr innerComp1)
                    let m2 = (range_of_synexpr innerComp2)
                    Some(mksynCall "Combine" m1 [c; mksynCall "Delay" m2 [mksyn_delay m2 (trans innerComp2)]])
                | None -> 
                    match innerComp1 with 
                    | Comp_do_bind(synInputExpr,m) -> 
                        let sp = 
                           match sp with 
                           | SuppressSequencePointOnStmtOfSequential -> SequencePointAtBinding m
                           | SuppressSequencePointOnExprOfSequential -> NoSequencePointAtDoBinding 
                           | SequencePointsAtSeq -> SequencePointAtBinding m
                        Some(trans (Comp_bind(sp, false,Pat_const(Const_unit,range_of_synexpr synInputExpr), synInputExpr,innerComp2,m)))
                    | _ -> 
                        Expr_seq(sp,true, innerComp1, trans innerComp2,m) |> Some

            | Expr_cond (guardExpr,thenComp,elseCompOpt,spIfToThen,mIfToThen,mIfToEndOfElseBranch) ->
                //if seqExprOnly then warning (Error("'when' clauses should only be used inside compact sequence expressions. Consider using 'if guardExpr then ...' in computation expressions",m));
                let elseComp = (match elseCompOpt with Some c -> c | None -> Comp_zero mIfToThen)
                Some(Expr_cond(guardExpr, trans thenComp, Some(trans elseComp), spIfToThen,mIfToThen,mIfToEndOfElseBranch))

            // 'let binds in expr'
            | Expr_let (isRec,false,binds,innerComp,m) ->
                Some(Expr_let (isRec,false,binds,trans innerComp,m))

            // 'use x = expr in expr'
            | Expr_let (isRec,true,[Binding (vis,NormalBinding,pseudo,mut,attrs,doc,valSynData,pat,BindingRhs([],_,rhsExpr),bindingRange,spBind)],innerComp,m) ->
                let consumeExpr = mksyn_match_lambda(false,false,m,[Clause(pat,None, trans innerComp,range_of_synexpr innerComp,SequencePointAtTarget)],spBind)
                Some(mksynCall "Using" m [rhsExpr; consumeExpr ])

            // 'let! x = expr in expr'
            | Comp_bind(spBind,false,pat,synInputExpr,innerComp,m) -> 

                let bindRange = match spBind with SequencePointAtBinding(m) -> m | _ -> range_of_synexpr synInputExpr
                let innerRange = range_of_synexpr innerComp
                let consumeExpr = mksyn_match_lambda(false,false,innerRange,[Clause(pat,None, trans innerComp,innerRange,SequencePointAtTarget)],spBind)
                Some(mksynCall "Bind"  bindRange [synInputExpr; consumeExpr ])

            // 'use! x = e1 in e2' --> build.Bind(e1,(fun x -> build.Using(x,(fun () -> e2))))
            | Comp_bind(spBind,true,(Pat_as (Pat_wild _, id, false, _, _) as pat) ,synInputExpr,innerComp,m) -> 

                let consumeExpr = mksyn_match_lambda(false,false,m,[Clause(pat,None, trans innerComp,range_of_synexpr innerComp,SequencePointAtTarget)],spBind)
                let consumeExpr = mksynCall "Using" m [Expr_id_get(id); consumeExpr ]
                let consumeExpr = mksyn_match_lambda(false,false,m,[Clause(pat,None, consumeExpr,m,SequencePointAtTarget)],spBind)
                Some(mksynCall "Bind" m [synInputExpr; consumeExpr])

            | Expr_match (spMatch,expr,clauses,false,m) ->
                let clauses = clauses |> List.map (fun (Clause(pat,cond,innerComp,patm,sp)) -> Clause(pat,cond,trans innerComp,patm,sp))
                Some(Expr_match(spMatch,expr, clauses, false,m))

            | Expr_try_catch (innerComp,mTryToWith,clauses,mWithToLast,mTryToLast,spTry,spWith) ->
                let clauses = clauses |> List.map (fun (Clause(pat,cond,innerComp,patm,sp)) -> Clause(pat,cond,trans innerComp,patm,sp))
                let consumeExpr = mksyn_match_lambda(false,true,m,clauses,NoSequencePointAtStickyBinding)
                Some(mksynCall "TryWith" mTryToLast [mksynCall "Delay" m [mksyn_delay m (trans innerComp)]; consumeExpr])

            | Comp_yieldm((isTrueYield,isTrueReturn),yieldExpr,m) -> 
                if not isTrueReturn then errorR(Error("Within computation expressions, use 'return!' instead of '->>' or 'yield! ",m));
                Some(yieldExpr )

            | Comp_yield((isTrueYield,isTrueReturn),yieldExpr,m) -> 
                Some(mksynCall (if isTrueYield then "Yield" else "Return") m [yieldExpr])

            | e -> None
        and trans c = 
            match tryTrans c with 
            | Some e -> e
            | None -> 
                // This only occurs in final position in a sequence
                match c with 
                // "do! expr;" in final position is treated as { let! () = expr in return () }
                | Comp_do_bind(synInputExpr,m) -> 
                    trans (Comp_bind(NoSequencePointAtDoBinding, false,Pat_const(Const_unit,range_of_synexpr synInputExpr), synInputExpr,Comp_yield((false,true),Expr_const(Const_unit,m),m),m))
                // "expr;" in final position is treated as { expr; zero }
                // Suppress the sequence point on the "zero"
                | _ -> 
                    Expr_seq(SuppressSequencePointOnStmtOfSequential,true, c,trans (Comp_zero m),m) 

        let coreSynExpr = trans comp

        let delayedExpr = 
            let m = range_of_synexpr coreSynExpr
            match TryFindMethInfo cenv.infoReader m ad "Delay" interpExprTy with 
            | [] -> coreSynExpr
            | _ -> mksynCall "Delay" m [(mksyn_delay m coreSynExpr)]
            
        let runExpr = 
            let m = range_of_synexpr delayedExpr
            match TryFindMethInfo cenv.infoReader m ad "Run" interpExprTy with 
            | [] -> delayedExpr
            | _ -> mksynCall "Run" m [delayedExpr]

        let lambdaExpr = Expr_lambda (false,false,SPats ([mksyn_spat_var false (mksyn_id interpVarRange interpVarName)],interpVarRange), runExpr, interpVarRange)

        let lambdaExpr ,tpenv= TcExpr cenv (interpExprTy --> ty) env tpenv lambdaExpr
        // beta-var-reduce to bind the builder using a 'let' binding
        let coreExpr = mk_appl cenv.g ((lambdaExpr,type_of_expr cenv.g lambdaExpr),[],[interpExpr],interpVarRange)

        coreExpr,tpenv

    // This case is used for sequence expressions
    | None -> 

        let mkDelayedExpr coreExpr = 
            let m = range_of_expr coreExpr
            let ty = type_of_expr cenv.g coreExpr
            mk_seq_delay cenv env m ty coreExpr

        let rec try_tc_comp env genOuterTy tpenv comp =
            match comp with 
            | Expr_foreach (spBind,SeqExprOnly(seqExprOnly),pat,pseudoEnumExpr,innerComp,m) -> 
                // This expression is not checked with the knowledge it is an IEnumerable, since we permit other enumerable types with GetEnumerator/MoveNext methods, as does C# 
                let pseudoEnumExpr,arb_ty,tpenv = TcExprOfUnknownType cenv env tpenv pseudoEnumExpr
                let enumExpr,enumElemTy = ConvertArbitraryExprToEnumerable cenv arb_ty env pseudoEnumExpr
                let pat',_,vspecs,envinner,tpenv = TcMatchPattern cenv enumElemTy env tpenv (pat,None)
                let innerExpr,tpenv = tc_comp envinner genOuterTy tpenv innerComp
                
                match pat', vspecs, innerExpr with 
                // peephole optimization: "for x in e1 -> e2" == "e1 |> List.map (fun x -> e2)" *)
                | (TPat_as (TPat_wild _,PBind (v,_),_), 
                   vs,  
                   TExpr_app(TExpr_val(vf,_,_),_,[genEnumElemTy],[yexpr],_)) 
                      when vs.Length = 1 && cenv.g.vref_eq vf cenv.g.seq_singleton_vref ->
          
                    let enumExprMark = (range_of_expr enumExpr)
                    let lam = mk_lambda enumExprMark v (yexpr,genEnumElemTy)
                    
                    // SEQUENCE POINTS: need to build a let here consuming spBind
                    let enumExpr = mk_coerce_if_needed cenv.g (mk_seq_ty cenv.g enumElemTy) (type_of_expr cenv.g enumExpr) enumExpr
                    Some(mk_call_seq_map cenv.g m enumElemTy genEnumElemTy lam enumExpr,tpenv)

                | _ -> 
                    let enumExprMark = (range_of_expr enumExpr)

                    // SEQUENCE POINTS: need to build a let here consuming spBind

                    let matchv,matchExpr = conv_tcomp_match_clauses cenv env enumExprMark (pat',vspecs) innerExpr enumElemTy genOuterTy m
                    let lam = mk_lambda enumExprMark matchv (matchExpr,type_of_expr cenv.g matchExpr)
                    Some(mk_seq_map_concat cenv env m enumElemTy genOuterTy lam enumExpr , tpenv)

            | Expr_for (spBind,id,start,dir,finish,innerComp,m) ->
                Some(tc_comp env genOuterTy tpenv (elim_fast_integer_for_loop (spBind,id,start,dir,finish,innerComp,m)))

            | Expr_while (spWhile,guardExpr,innerComp,m) -> 
                let guardExpr,tpenv = TcExpr cenv cenv.g.bool_ty env tpenv guardExpr
                let innerExpr,tpenv = tc_comp env genOuterTy tpenv innerComp
    
                let guardExprMark = (range_of_expr guardExpr)
                let guardExpr = mk_unit_delay_lambda cenv.g guardExprMark guardExpr
                let innerExpr = mkDelayedExpr innerExpr
                Some(mk_seq_generated cenv env guardExprMark genOuterTy guardExpr innerExpr, tpenv)

            | Expr_try_finally (innerComp,unwindExpr,mTryToLast,spTry,spFinally) ->
                let m = range_of_synexpr unwindExpr
                let innerExpr,tpenv = tc_comp env genOuterTy tpenv innerComp
                let unwindExpr,tpenv = TcExpr cenv cenv.g.unit_ty env tpenv unwindExpr
            
                let unwindExprMark = (range_of_expr unwindExpr)
                let unwindExpr = mk_unit_delay_lambda cenv.g unwindExprMark unwindExpr
                let innerExpr = mkDelayedExpr innerExpr
                let innerExprMark = (range_of_expr innerExpr)
                
                Some(mk_seq_finally cenv env innerExprMark genOuterTy innerExpr unwindExpr, tpenv)
            | Expr_paren (_,m) -> 
                error(Error("This construct is ambiguous as part of a sequence expression. Nested expressions may be written using 'let _ = (...)' and nested sequences using 'yield! seq {... }'",m))

            | Comp_zero m -> 
                Some(mk_seq_empty cenv env m genOuterTy,tpenv )

            | Comp_do_bind(synInputExpr,m) -> 
                error(Error("'do!' may not be used within sequence expressions",m))

            | Expr_seq(sp,true,innerComp1, innerComp2,m) -> 
                // "expr; cexpr" is treated as sequential execution
                // "cexpr; cexpr" is treated as append
                match try_tc_comp env genOuterTy tpenv innerComp1 with 
                | None -> 
                    let innerExpr1,tpenv = TcStmtThatCantBeCtorBody cenv env tpenv innerComp1
                    let innerExpr2,tpenv = tc_comp env genOuterTy tpenv innerComp2

                    Some(TExpr_seq(innerExpr1,innerExpr2,NormalSeq,sp,m),tpenv)

                | Some (innerExpr1,tpenv) ->
                    let innerExpr2,tpenv = tc_comp env genOuterTy tpenv innerComp2
                    let innerExpr2 = mkDelayedExpr innerExpr2
                    Some(mk_seq_append cenv env (range_of_synexpr innerComp1) genOuterTy innerExpr1 innerExpr2, tpenv)

            | Expr_cond (guardExpr,thenComp,elseCompOpt,spIfToThen,mIfToThen,mIfToEndOfElseBranch) ->
                let guardExpr',tpenv = TcExpr cenv cenv.g.bool_ty env tpenv guardExpr
                let thenExpr,tpenv = tc_comp env genOuterTy tpenv thenComp
                let elseComp = (match elseCompOpt with Some c -> c | None -> Comp_zero mIfToThen)
                let elseExpr,tpenv = tc_comp env genOuterTy tpenv elseComp
                Some(mk_cond spIfToThen SequencePointAtTarget mIfToEndOfElseBranch genOuterTy guardExpr' thenExpr elseExpr, tpenv)

            // 'let x = expr in expr'
            | Expr_let (isRec,false (* not a 'use' binding *),binds,body,m) ->
                TcLinearLetExprs 
                    (fun ty envinner tpenv e -> tc_comp envinner ty tpenv e) 
                    cenv env ty 
                    (fun x -> x) 
                    tpenv 
                    (false(* don't consume 'use' bindings*),isRec,false,binds,body,m)  |> Some

            // 'use x = expr in expr'
            | Expr_let (isRec,true,[Binding (vis,NormalBinding,_,_,_,_,_,pat,BindingRhs([],_,synInputExpr),_,spBind)],innerComp,wholeExprMark) ->

                let bindPatTy = new_inference_typ cenv ()
                let inputExprTy = new_inference_typ cenv ()
                let pat',_,vspecs,envinner,tpenv = TcMatchPattern cenv bindPatTy env tpenv (pat,None)
                unifyE cenv env m inputExprTy bindPatTy;
                let inputExpr,tpenv = TcExpr cenv inputExprTy env tpenv synInputExpr
                let innerExpr,tpenv = tc_comp envinner genOuterTy tpenv innerComp
                let inputExprTy = type_of_expr cenv.g inputExpr
                let innerExprMark = range_of_expr innerExpr  
                let inputExprMark = range_of_expr inputExpr
                let matchv,matchExpr = conv_tcomp_match_clauses cenv env inputExprMark (pat',vspecs) innerExpr bindPatTy genOuterTy wholeExprMark
                let consumeExpr = mk_lambda wholeExprMark matchv (matchExpr,genOuterTy)
                //SEQPOINT NEEDED - we must consume spBind on this path
                Some(mk_seq_using cenv env wholeExprMark bindPatTy genOuterTy inputExpr consumeExpr, tpenv)

            | Comp_bind(spBind,isUse,pat,synInputExpr,innerComp,m) -> 
                error(Error("The use of 'let! x = coll' in sequence expressions is no longer permitted. Use 'for x in coll' instead",m))

            | Expr_match (spMatch,expr,clauses,false,m) ->
                let inputExpr,matchty,tpenv = TcExprOfUnknownType cenv env tpenv expr
                let tclauses,tpenv = 
                    List.mapfold 
                        (fun tepnv (Clause(pat,cond,innerComp,patm,sp)) ->
                              let pat',cond',vspecs,envinner,tpenv = TcMatchPattern cenv matchty env tpenv (pat,cond)
                              let innerExpr,tpenv = tc_comp envinner genOuterTy tpenv innerComp
                              TClause(pat',cond',TTarget(vspecs, innerExpr,sp),RangeOfPat pat'),tpenv)
                        tpenv
                        clauses
                let inputExprTy = type_of_expr cenv.g inputExpr
                let inputExprMark = (range_of_expr inputExpr)
                let matchv,matchExpr = CompilePatternForMatchClauses cenv env inputExprMark inputExprMark true Incomplete inputExprTy genOuterTy tclauses 
                Some(mk_let spMatch inputExprMark matchv inputExpr matchExpr, tpenv)

            | Expr_try_catch (innerComp,mTryToWith,clauses,mWithToLast,mTryToLast,spTry,spWith) ->
                error(Error("'try'/'with' may not be used within sequence expressions",mTryToWith))

            | Comp_yieldm((isTrueYield,isTrueReturn),yieldExpr,m) -> 
                let resultExpr,genExprTy,tpenv = TcExprOfUnknownType cenv env tpenv yieldExpr

                if not isTrueYield then errorR(Error("In sequence expressions, multiple results are generated using 'yield!'",m)) ;

                AddCxTypeMustSubsumeType env.DisplayEnv cenv.css m  NoTrace genOuterTy genExprTy;
                Some(mk_coerce(resultExpr,genOuterTy,m,genExprTy), tpenv)

            | Comp_yield((isTrueYield,isTrueReturn),yieldExpr,m) -> 
                let genResultTy = new_inference_typ cenv ()
                if not isTrueYield then errorR(Error("In sequence expressions, results are generated using 'yield'",m)) ;
                unifyE cenv env m genOuterTy (mk_seq_ty cenv.g genResultTy);

                let resultExpr,tpenv = TcExpr cenv genResultTy env tpenv yieldExpr
                Some(mk_call_seq_singleton cenv.g m genResultTy resultExpr, tpenv )

            | e -> None
                
        and tc_comp env genOuterTy tpenv comp =
            match try_tc_comp env genOuterTy tpenv comp with 
            | Some e -> e
            | None -> 
                // seq { ...; expr } is treated as 'seq { ... ; expr; yield! Seq.empty }'
                // Note this means seq { ...; () } is treated as 'seq { ... ; (); yield! Seq.empty }'
                let m = range_of_synexpr comp 
                let expr,tpenv = TcStmtThatCantBeCtorBody cenv env tpenv comp
                TExpr_seq(expr,mk_seq_empty cenv env m genOuterTy,NormalSeq,SuppressSequencePointOnStmtOfSequential,m),tpenv

        let genEnumElemTy = new_inference_typ cenv ()
        unifyE cenv env m ty (mk_seq_ty cenv.g genEnumElemTy);

        let coreExpr,tpenv = tc_comp env ty tpenv comp
        let delayedExpr = mkDelayedExpr coreExpr
        delayedExpr,tpenv

//-------------------------------------------------------------------------
// Typecheck "expr ... " constructs where "..." is a sequence of applications,
// type applications and dot-notation projections. First extract known
// type information from the "..." part to use during type checking.
//
// 'ty' is the type expected for the entire chain of expr + lookups.
// 'exprty' is the type of the expression on the left of the lookup chain.
//
// Unsophisticated applications can propagate information from the expected overall type 'ty' 
// through to the leading function type 'exprty'. This is because the application 
// unambiguously implies a function type 
//------------------------------------------------------------------------- 

and PropagateThenTcDelayed cenv ty env tpenv m expr exprty (atomicFlag:ExprAtomicFlag) delayed = 
    
    let rec propagate delayed' exprm exprty = 
      match delayed' with 
      | [] -> 
          // Avoid unifying twice: we're about to unify in TcDelayed 
          if nonNil delayed then 
              unifyE cenv env exprm ty exprty
      | DelayedSet _ :: _
      | DelayedDotLookup _ :: _ -> ()
      | DelayedTypeApp (x,appm) :: delayed'' ->
          // Note this case should not occur: would eventually give an "Unexpected type application" error in TcDelayed 
          propagate delayed'' appm exprty 

      | DelayedApp (_, arg,appm) :: delayed'' ->
          let denv = env.DisplayEnv
          match UnifyFunctionTypeUndoIfFailed cenv denv m exprty with
          | Some (_,resultTy) -> 
              propagate delayed'' appm resultTy 
          | None -> 
              let argm = range_of_synexpr arg
              match arg with 
              | Expr_comprehension (_,isNotNakedRefCell,comp,_) -> ()
              | _ -> 
                 error (NotAFunction(denv,ty,exprm,argm)) 
              
    propagate delayed expr.Range exprty;
    TcDelayed cenv ty env tpenv m expr exprty atomicFlag delayed


/// Typecheck "expr ... " constructs where "..." is a sequence of applications,
/// type applications and dot-notation projections.
and TcDelayed cenv ty env tpenv m expr exprty (atomicFlag:ExprAtomicFlag) delayed = 

    // OK, we've typechecked the thing on the left of the delayed lookup chain. 
    // We can now record for posterity the type of this expression and the location of the expression. 
    if (atomicFlag = ExprAtomicFlag.Atomic) then
        CallExprHasTypeSink(m,nenv_of_tenv env,exprty, env.DisplayEnv,AccessRightsOfEnv env);

    match delayed with 
    | [] -> unifyE cenv env m ty exprty; expr.Expr,tpenv
    // expr.m(args) where x.m is a .NET method or index property 
    // expr.m<tyargs>(args) where x.m is a .NET method or index property 
    // expr.m where x.m is a .NET method or index property 
    | DelayedDotLookup (lid,m) :: delayed' ->
         TcLookupThen cenv ty env tpenv expr.Expr exprty lid delayed' m
    // f x 
    | DelayedApp (hpa,arg,appm) :: delayed' ->
        TcFunctionApplicationThen cenv ty env tpenv appm expr exprty arg hpa delayed'
    // f<tyargs> 
    | DelayedTypeApp (x,m) :: delayed' ->
        error(Error("Unexpected type application",m))
    | DelayedSet _ :: delayed' ->      
        error(Error("Invalid assignment",m))


and delay_rest rest m delayed = 
    match rest with 
    | [] -> delayed 
    | lid -> (DelayedDotLookup (rest,union_ranges m (range_of_lid lid)) :: delayed)


//-------------------------------------------------------------------------
// TcFunctionApplicationThen: Typecheck "expr x" + projections
//------------------------------------------------------------------------- 

and TcFunctionApplicationThen cenv ty env tpenv appm expr exprty arg atomicFlag delayed' = 
    
    let denv = env.DisplayEnv
    let argm = range_of_synexpr arg
    let funm = expr.Range
    match UnifyFunctionTypeUndoIfFailed cenv denv funm exprty with
    | Some (domainTy,resultTy) -> 

        // Notice the special case 'seq { ... }'
        // Set a flag in the syntax tree to say we noticed a leading 'seq'
        match arg with 
        | Expr_comprehension (false,isNotNakedRefCell,comp,m) -> 
            isNotNakedRefCell := 
                !isNotNakedRefCell
                || 
                (match expr with 
                 | ApplicableExpr(_,TExpr_op(TOp_coerce,_,[TExpr_app(TExpr_val(vf,_,_),_,_,_,_)],_)) when cenv.g.vref_eq vf cenv.g.seq_vref -> 
                    //dprintfn "FOUND 'seq { ... }'"
                    true 
                 | _ -> 
                    //dprintfn "DID NOT FIND 'seq { ... }'"
                    false)
        | _ -> ()

        let arg',tpenv = TcExpr cenv domainTy env tpenv arg
        let expr' = build_app cenv expr exprty arg' appm
        TcDelayed cenv ty env tpenv appm expr' resultTy atomicFlag delayed'
    | None -> 
        match arg with 
        | Expr_comprehension (false,isNotNakedRefCell,comp,m) -> 
            let expr',tpenv = TcComputationExpression cenv env ty funm (Some(expr.Expr,exprty)) tpenv comp
            TcDelayed cenv ty env tpenv appm (MkApplicableExprNoFlex cenv expr') (type_of_expr cenv.g expr') ExprAtomicFlag.NonAtomic delayed' 
        | _ -> 
            error (NotAFunction(denv,ty,funm,argm)) 

//-------------------------------------------------------------------------
// TcLongIdentThen : Typecheck "A.B.C<D>.E.F ... " constructs
//------------------------------------------------------------------------- 

and TcLongIdentThen cenv ty env tpenv lid delayed =

    let ad = AccessRightsOfEnv env
    let typeNameResInfo = 
        (* Given 'MyOverloadedType<int>.MySubType...' use arity of #given type arguments to help *)
        (* resolve type name lookup of 'MyOverloadedType' *)
        (* Also determine if type names should resolve to Item_typs or Item_ctor_group *)
        match delayed with 
        | DelayedTypeApp (tyargs,_) :: DelayedApp _ :: _ -> 
            (ResolveTypeNamesToCtors, Some(List.length tyargs))

        | DelayedTypeApp (tyargs,_) :: _ -> 
            // cases like 'MyType<int>.Sth' but also only 'MyType<int>.' 
            // (without LValue_get), which is needed for VS (when typing)
            (ResolveTypeNamesToTypeRefs, Some(List.length tyargs)) 

        | _ -> DefaultTypeNameResInfo

    let itemAndRest = ResolveLongIdentAsExprAndComputeRange cenv.nameResolver (range_of_lid lid) ad env.eNameResEnv typeNameResInfo lid
    TcItemThen cenv ty env tpenv itemAndRest delayed

//-------------------------------------------------------------------------
// Typecheck "item+projections" 
//------------------------------------------------------------------------- *)

and TcItemThen cenv overallTy env tpenv (item,m,rest) delayed =

    let delayed = delay_rest rest m delayed
    let ad = AccessRightsOfEnv env
    match item with
    (* x where x is a union case or active pattern result tag. *)
    | (Item_ucase _ | Item_ecref _ | Item_apres _) as item -> 
        (* ucaseAppTy is the type of the union constructor applied to its (optional) argument *)
        let ucaseAppTy = new_inference_typ cenv ()
        let mkConstrApp,argtys = 
          match item with 
          | Item_apres(apinfo, _, n) -> 
              let aparity = List.length (names_of_apinfo apinfo)
              match aparity with 
              | 0 | 1 -> 
                  let mkConstrApp = function [arg] -> arg | _ -> error(InternalError("gen_constr_unify",m))
                  mkConstrApp, [ucaseAppTy]
              | _ ->
                  let ucref = mk_choices_ucref cenv.g m aparity n
                  let _,inst,tinst,_ = info_of_tcref cenv m env ucref.TyconRef
                  let ucinfo = UnionCaseInfo(tinst,ucref)
                  expr_constr_unify m cenv env ucaseAppTy (Item_ucase ucinfo)
          | _ -> 
              expr_constr_unify m cenv env ucaseAppTy item
        let nargtys = List.length argtys
        // Subsumption at data constructions if argument type is nominal prior to equations for any arguments or return types
        let flexes = argtys |> List.map (is_typar_typ cenv.g >> not)
        match delayed with 
        // This is where the constructor is applied to an argument 
        | ((DelayedApp (atomicFlag, arg,appm))::delayed') ->
            if isNil(delayed') then 
                unifyE cenv env appm overallTy ucaseAppTy; 
                  
            let args = 
              match arg with 
              | Expr_paren(Expr_tuple(args,appm),_)
              | Expr_tuple(args,appm)     when nargtys > 1 -> args
              | Expr_paren(arg,_)
              | arg -> [arg]
            let nargs = List.length args
            gen_constr_check env nargtys nargs appm;
            let args',tpenv = TcExprs cenv env appm tpenv flexes argtys args
            PropagateThenTcDelayed cenv overallTy env tpenv appm (MkApplicableExprNoFlex cenv (mkConstrApp args')) ucaseAppTy atomicFlag delayed'
        | DelayedTypeApp (x,tyappm) :: delayed' ->
            error(Error("Unexpected type application",tyappm))
        | _ -> 
            // Work out how many syntactic arguments we really expect. Also return a function that builds the overall 
            // expression, but don't apply this function until after we've checked that the number of arguments is OK 
            // (or else we would be building an invalid expression) 
            
            // Unit-taking active pattern result can be applied to no args 
            let nargs,mkExpr = 
                // This is where the constructor is an active pattern result applied to no argument 
                // Unit-taking active pattern result can be applied to no args 
                if (nargtys = 1 && match item with Item_apres _ -> true | _ -> false) then 
                    unifyE cenv env m (List.hd argtys) cenv.g.unit_ty;
                    1,(fun () -> mkConstrApp [mk_unit cenv.g m])

                // This is where the constructor expects no arguments and is applied to no argument 
                elif nargtys = 0 then 
                    0,(fun () -> mkConstrApp []) 
                else 
                    // This is where the constructor expects arguments but is not applied to arguments, hence build a lambda 
                    nargtys, 
                    (fun () -> 
                        let vs,args = argtys |> List.mapi (fun i ty -> mk_compgen_local m ("arg"^string i) ty) |> List.unzip
                        let constrApp = mkConstrApp args
                        let lam = mk_multi_lambda m vs (constrApp, type_of_expr cenv.g constrApp)
                        lam)
            gen_constr_check env nargtys nargs m;
            let expr = mkExpr()
            let exprTy = type_of_expr cenv.g expr
            PropagateThenTcDelayed cenv overallTy env tpenv m (MkApplicableExprNoFlex cenv expr) exprTy ExprAtomicFlag.Atomic delayed 

    | Item_typs(nm,(typ::_)) -> 
    
        match delayed with 
        | ((DelayedTypeApp(tyargs,tyappm))::(DelayedDotLookup (lid,_))::delayed') ->

            // If Item_typs is returned then the typ will be of the form TType_app(tcref,genericTyargs) where tyargs 
            // is a fresh instantiation for tcref. TcNestedTypeApplication will chop off precisely #genericTyargs args 
            // and replace them by 'tyargs' 
            let typ,tpenv = TcNestedTypeApplication cenv NewTyparsOK CheckCxs env tpenv tyappm typ tyargs

            // Report information about the whole expression including type arguments to VS
            CallNameResolutionSink(tyappm,nenv_of_tenv env,Item_typs(nm, [typ]),ItemOccurence.Use,env.DisplayEnv,AccessRightsOfEnv env)
            TcItemThen cenv overallTy env tpenv (ResolveExprDotLongIdentAndComputeRange cenv.nameResolver m ad env.eNameResEnv typ lid IgnoreOverrides) delayed'
            
        | ((DelayedTypeApp(tyargs,tyappm))::delayed') ->
            // A case where we have an incomplete name e.g. 'Foo<int>.' - we still want to report it to VS!
            let typ,tpenv = TcNestedTypeApplication cenv NewTyparsOK CheckCxs env tpenv tyappm typ tyargs
            CallNameResolutionSink(tyappm,nenv_of_tenv env,Item_typs(nm, [typ]),ItemOccurence.Use,env.DisplayEnv,AccessRightsOfEnv env)
            
            // Same error as in the following case
            error(Error("Invalid use of a type name",m));
            
        | _ -> 
            (* In this case the type is not generic, and indeed we should never have returned Item_typs. *)
            (* That's because ResolveTypeNamesToCtors should have been set at the original *)
            (* call to ResolveLongIdentAsExprAndComputeRange *)
            error(Error("Invalid use of a type name",m));

    | Item_meth_group (methodName,minfos) -> 
        // Static method calls Type.Foo(arg1,...,argn) 
        let meths = List.map (fun minfo -> minfo,None) minfos
        match delayed with 
        | (DelayedApp (atomicFlag, arg,m)::delayed') ->
            TcMethodApplicationThen cenv env overallTy tpenv None [] m methodName ad NeverMutates false meths NormalValUse [arg] atomicFlag delayed'

        | (DelayedTypeApp(tys,tyappm)::DelayedApp (atomicFlag, arg,m)::delayed') ->
            let tyargs,tpenv = TcTypesOrMeasures None cenv NewTyparsOK CheckCxs env tpenv tys m
            
            // NOTE: This doesn't take instantiation into account
            CallNameResolutionSink(tyappm,nenv_of_tenv env,item (* ! *),ItemOccurence.Use,env.DisplayEnv,AccessRightsOfEnv env)                        
            TcMethodApplicationThen cenv env overallTy tpenv (Some tyargs) [] m methodName ad NeverMutates false meths NormalValUse [arg] atomicFlag delayed'
        | _ -> 
            TcMethodApplicationThen cenv env overallTy tpenv None [] m methodName ad NeverMutates false meths NormalValUse [] ExprAtomicFlag.Atomic delayed 

    | Item_ctor_group(_,minfos) ->
        let objTy = 
            match minfos with 
            | (minfo :: _) -> minfo.EnclosingType
            | [] -> error(Error("This type has no accessible object constructors",m))
        match delayed with 
        | ((DelayedApp (_, arg,argm))::delayed') ->

            // REVIEW: the langauge currently allows 'KeyValuePair(k,v)' 
            // REVIEW: this should perhaps be reconsidered. 
            // Here is the code to enforce the check: 
            //   This checks that the type really doesn't expect any type arguments. 
            //   let objTy,tpenv = TcNestedTypeApplication cenv NewTyparsOK CheckCxs env tpenv m objTy [] in 
            //   This should be a nop but is included to keep symmetry with the next case 
            //   minfos |> List.iter (fun minfo -> unifyE cenv env m (typ_of_minfo minfo) objTy); 
            TcCtorCall true cenv env tpenv overallTy objTy item false arg argm delayed'

        | ((DelayedTypeApp(tyargs,tyappm))::(DelayedApp (_, arg,m))::delayed') ->
            let objTy,tpenv = TcNestedTypeApplication cenv NewTyparsOK CheckCxs env tpenv tyappm objTy tyargs
            minfos |> List.iter (fun minfo -> unifyE cenv env tyappm minfo.EnclosingType objTy);
            
            // NOTE: This doesn't take instantiation into account
            CallNameResolutionSink(tyappm,nenv_of_tenv env,item (* ! *),ItemOccurence.Use,env.DisplayEnv,AccessRightsOfEnv env)
            TcCtorCall true cenv env tpenv overallTy objTy item false arg m delayed'

        | _ -> 
            let text = List.map (string_of_minfo cenv.amap m env.DisplayEnv) minfos
            error(Error("Invalid use of a type name and/or object constructor. If necessary use 'new' and apply the constructor to its arguments, e.g. 'new Type(args)'"^
                         (if nonNil minfos then ". Overloads are:  \n\t"^String.concat "\n\t" text else ""),m))

    | Item_fake_intf_ctor _ ->
        error(Error("Invalid use of an interface type",m))
    | Item_implicit_op id ->

        let isPrefix = PrettyNaming.IsPrefixOperator id.idText

        let argData = 
            if isPrefix then 
                [ Typar(mksyn_id m (new_arg_name()), HeadTypeStaticReq,true) ]
            else
                [ Typar(mksyn_id m (new_arg_name()), HeadTypeStaticReq,true);
                  Typar(mksyn_id m (new_arg_name()), HeadTypeStaticReq,true) ]
                
        let retTyData = Typar(mksyn_id m (new_arg_name()), HeadTypeStaticReq,true)
        let argTypars = argData |> List.map (fun d -> NewTypar (KindType, TyparFlexible,d,false,DynamicReq,[]))
        let retTypar = NewTypar (KindType, TyparFlexible,retTyData,false,DynamicReq,[])
        let argTys = argTypars |> List.map mk_typar_ty 
        let retTy = mk_typar_ty retTypar

        let vs,ves = argTys |> List.mapi (fun i ty -> mk_compgen_local m ("arg"^string i) ty) |> List.unzip

        let memberFlags = StaticMemberFlags None MemberKindMember
        let logicalCompiledName = ComputeLogicalCompiledName id memberFlags
        let traitInfo = TTrait(argTys,logicalCompiledName,memberFlags,argTys,Some retTy,ref None)

        AddCxMethodConstraint env.DisplayEnv cenv.css m NoTrace traitInfo;
      
        let expr = TExpr_op(TOp_trait_call(traitInfo), [], ves, m)
        let expr = mk_lambdas m [] vs (expr,retTy)
        PropagateThenTcDelayed cenv overallTy env tpenv m (MkApplicableExprNoFlex cenv expr) (type_of_expr cenv.g expr) ExprAtomicFlag.NonAtomic delayed
        
    | Item_delegate_ctor typ ->
        match delayed with 
        | ((DelayedApp (atomicFlag, arg,m))::delayed') ->
            TcNewDelegateThen cenv overallTy env tpenv m typ arg atomicFlag delayed'
        | ((DelayedTypeApp(tyargs,tyappm))::(DelayedApp (atomicFlag, arg,m))::delayed') ->
            let typ,tpenv = TcNestedTypeApplication cenv NewTyparsOK CheckCxs env tpenv tyappm typ tyargs
            
            // NOTE: This doesn't take instantiation into account
            CallNameResolutionSink(tyappm,nenv_of_tenv env,item (* ! *),ItemOccurence.Use,env.DisplayEnv,AccessRightsOfEnv env)            
            TcNewDelegateThen cenv overallTy env tpenv m typ arg atomicFlag delayed'
        | _ -> error(Error("Invalid use of a delegate constructor. Use the syntax 'new Type(args)' or just 'Type(args)'",m))

    | Item_val vref -> 

        match delayed with 
        // Mutable value set: 'v <- e' 
        | DelayedSet(e2,m) :: delayed' ->
            if nonNil(delayed') then error(Error("Invalid assignment",m));
            unifyE cenv env m overallTy (cenv.g.unit_ty);
            let vty = vref.Type
            let vty2 = if is_byref_typ cenv.g vty then dest_byref_typ cenv.g vty else vty 
            // Always allow subsumption on asignment to fields
            let e2',tpenv = TcExprFlex cenv true vty2 env tpenv e2
            let vexp,tepnv = 
                if is_byref_typ cenv.g vty then 
                  mk_lval_set m vref e2', tpenv
                else 
                  if not vref.IsMutable then error (ValNotMutable(env.DisplayEnv,vref,m));
                  mk_val_set m vref e2', tpenv
                
            PropagateThenTcDelayed cenv overallTy env tpenv m (MkApplicableExprNoFlex cenv vexp) (type_of_expr cenv.g vexp) ExprAtomicFlag.NonAtomic delayed'

        // Value instantiation: v<tyargs> ... 
        | (DelayedTypeApp(tys,tyappm)::delayed') ->
            // Note: we know this is a NormalValUse because: 
            //   - it isn't a CtorValUsedAsSuperInit 
            //   - it isn't a CtorValUsedAsSelfInit 
            //   - it isn't a VSlotDirectCall (uses of base values do not take type arguments 
            let checkTys tpenv kinds = TcTypesOrMeasures (Some kinds) cenv NewTyparsOK CheckCxs env tpenv tys m
            let (vexp,isSpecial,_,tpenv) = TcVal cenv env tpenv vref (Some (NormalValUse, checkTys)) m
            let vexp = (if isSpecial then MkApplicableExprNoFlex cenv vexp else MkApplicableExprWithFlex cenv env vexp)
            // type of the expression (e.g. For the source text "sizeof<float>" vexpty will be the TAST type for int32)
            let vexpty = vexp.Type 
            
            // We need to eventually record the type resolution for an expression, but this is done
            // inside PropagateThenTcDelayed, so we don't have to explicitly call 'CallExprHasTypeSink' here            
            PropagateThenTcDelayed cenv overallTy env tpenv tyappm vexp vexpty ExprAtomicFlag.Atomic delayed'

        // Value get 
        | _ ->  
            let (vexp,isSpecial ,_,tpenv) = TcVal cenv env tpenv vref None m
            let vexp = (if isSpecial then MkApplicableExprNoFlex cenv vexp else MkApplicableExprWithFlex cenv env vexp)
            let vexpty = vexp.Type
            PropagateThenTcDelayed cenv overallTy env tpenv m vexp vexpty ExprAtomicFlag.Atomic delayed
        
    | Item_property (nm,pinfos) ->
        if isNil pinfos then error (InternalError ("Unexpected error: empty property list",m));
        let pinfo = List.hd pinfos
        let _, tyargsOpt,args,delayed,tpenv = 
            if pinfo.IsIndexer 
            then GetMemberApplicationArgs delayed cenv env tpenv m 
            else ExprAtomicFlag.Atomic,None,[mksyn_unit m],delayed,tpenv
        if not pinfo.IsStatic then error (Error (sprintf "property '%s' is not static" nm,m));
        match delayed with 
        | DelayedSet(e2,m) :: delayed' ->
            let args = if pinfo.IsIndexer then args else []
            if nonNil(delayed') then error(Error("Invalid assignment",m));
            // Static Property Set (possibly indexer) 
            unifyE cenv env m overallTy (cenv.g.unit_ty);
            let meths = pinfos |> List.choose (fun pinfo -> if pinfo.HasSetter then Some(pinfo.SetterMethod,Some pinfo) else None)
            TcMethodApplicationThen cenv env overallTy tpenv tyargsOpt [] m nm ad DefinitelyMutates true meths NormalValUse (args@[e2]) ExprAtomicFlag.NonAtomic delayed'
        | _ -> 
            // Static Property Get (possibly indexer) 
            let meths = pinfos |> List.choose (fun pinfo -> if pinfo.HasGetter then Some(pinfo.GetterMethod,Some pinfo) else None) 
            if isNil(meths) then error (Error (sprintf "property '%s' is not readable" nm,m));
            TcMethodApplicationThen cenv env overallTy tpenv tyargsOpt [] m nm ad PossiblyMutates true meths NormalValUse args ExprAtomicFlag.Atomic delayed

    | Item_il_field finfo -> 

        CheckILFieldInfoAccessible cenv.g cenv.amap m ad finfo;
        if not finfo.IsStatic then error (Error (sprintf "field '%s' is not static" finfo.FieldName,m));
        CheckILFieldAttributes cenv.g finfo m;
        let fref = finfo.ILFieldRef
        let exprty = FieldTypeOfILFieldInfo cenv.amap m  finfo
        match delayed with 
        | DelayedSet(e2,m) :: delayed' ->
            unifyE cenv env m overallTy (cenv.g.unit_ty);
            // Always allow subsumption on asignment to fields
            let e2',tpenv = TcExprFlex cenv true exprty env tpenv e2
            let expr = BuildILStaticFieldSet cenv.g m finfo e2'
            expr,tpenv
        | _ -> 
           (* Get static IL field *)
            let expr = 
              match finfo.LiteralValue with 
              | Some lit -> 
                  TExpr_const(TcFieldInit m lit,m,exprty) 
              | None -> 
                let isValueType = finfo.IsValueType
                let valu = if isValueType then AsValue else AsObject
                (* The empty instantiation on the fspec is OK, since we make the correct fspec in Ilxgen.gen_asm *)
                (* This ensures we always get the type instantiation right when doing this from *)
                (* polymorphic code, after inlining etc. *) 
                (* REVIEW: stop generating ABSIL instructions here. This causes a number of minor problems (e.g. with quotation reflection) *)
                let fspec = mk_fspec(fref,mk_named_typ valu fref.EnclosingTypeRef [])
                (* Add an I_nop if this is an initonly field to make sure we never recognize it as an lvalue. See mk_expra_of_expr. *)
                mk_asm ([ mk_normal_ldsfld fspec ] @ (if finfo.IsInitOnly then [ I_arith AI_nop ] else []), 
                        finfo.TypeInst,[],[exprty],m)
            PropagateThenTcDelayed cenv overallTy env tpenv m (MkApplicableExprWithFlex cenv env expr) exprty ExprAtomicFlag.Atomic delayed

    | Item_recdfield rfinfo -> 
        (* Get static F# field or literal *)
        CheckRecdFieldInfoAccessible m ad rfinfo;
        if not rfinfo.IsStatic then error (Error (sprintf "field '%s' is not static" rfinfo.Name,m));
        CheckRecdFieldInfoAttributes cenv.g rfinfo m |> CommitOperationResult;        
        let fref = rfinfo.RecdFieldRef
        let fieldTy = rfinfo.FieldType
        match delayed with 
        | DelayedSet(e2,m) :: delayed' ->
            if nonNil(delayed') then error(Error("Invalid assignment",m));
        
            // Set static F# field 
            CheckRecdFieldMutation m env.DisplayEnv rfinfo [];
            unifyE cenv env m overallTy cenv.g.unit_ty;
            let fieldTy = rfinfo.FieldType
            // Always allow subsumption on asignment to fields
            let e2',tpenv = TcExprFlex cenv true fieldTy env tpenv e2
            let expr = mk_static_rfield_set (rfinfo.RecdFieldRef,rfinfo.TypeInst,e2',m)
            expr,tpenv
            
        | _  ->
            let exprty = fieldTy
            let expr = 
              match rfinfo.LiteralValue with 
              (* Get literal F# field *)
              | Some lit -> TExpr_const(lit,m,exprty)
              (* Get static F# field *)
              | None -> mk_static_rfield_get (fref,rfinfo.TypeInst,m) 
            PropagateThenTcDelayed cenv overallTy env tpenv m (MkApplicableExprWithFlex cenv env expr) exprty ExprAtomicFlag.Atomic delayed

    | Item_event einfo -> 
        // Instance IL event (fake up event-as-value) 
        TcEventValueThen cenv overallTy env tpenv m None einfo delayed
     
    | _ -> error(Error("This lookup may not be used here", m))


//-------------------------------------------------------------------------
// Typecheck "expr.A.B.C ... " constructs
//------------------------------------------------------------------------- 

and GetMemberApplicationArgs delayed cenv env tpenv m =
    match delayed with 
    | DelayedApp (atomicFlag, arg,m) :: delayed' -> atomicFlag,None,[arg],delayed',tpenv
    | DelayedTypeApp(tyargs,_) :: DelayedApp (atomicFlag, arg,m) :: delayed' ->
        let tyargs,tpenv = TcTypesOrMeasures None cenv NewTyparsOK CheckCxs env tpenv tyargs m
        atomicFlag,Some(tyargs),[arg],delayed',tpenv
    | delayed' ->
        ExprAtomicFlag.NonAtomic,None,[],delayed',tpenv


and TcLookupThen cenv overallTy env tpenv objExpr objExprTy lid delayed m =
    let objArgs = [objExpr]
    let ad = AccessRightsOfEnv env

    // 'base' calls use a different resolution strategy when finding methods. 
    let findFlag = 
        let baseCall = IsBaseCall objArgs
        (if baseCall then PreferOverrides else IgnoreOverrides)
        
    // Canonicalize inference problem prior to '.' lookup on variable types 
    if is_typar_typ cenv.g objExprTy then 
        GeneralizationHelpers.CanonicalizePartialInferenceProblem (cenv,env.DisplayEnv,m) (free_in_type_lr cenv.g false objExprTy);
    
    let item,m,rest = ResolveExprDotLongIdentAndComputeRange cenv.nameResolver m ad env.eNameResEnv objExprTy lid findFlag
    let delayed = delay_rest rest m delayed

    match item with
    | Item_meth_group (methodName,minfos) -> 
        let atomicFlag,tyargsOpt,args,delayed,tpenv = GetMemberApplicationArgs delayed cenv env tpenv m
        let meths = List.map (fun minfo -> minfo,None) minfos
        (* We pass PossiblyMutates here because these may actually mutate a value type object *) 
        (* To get better warnings we special case some of the few known mutate-a-struct method names *) 
        let mutates = (if methodName = "MoveNext" || methodName = "GetNextArg" then DefinitelyMutates else PossiblyMutates)
        TcMethodApplicationThen cenv env overallTy tpenv tyargsOpt objArgs m methodName ad mutates false meths NormalValUse args atomicFlag delayed 

    | Item_property (nm,pinfos) ->
        // Instance property 
        if isNil pinfos then error (Error ("Unexpected error: empty property list",m));
        let pinfo = List.hd pinfos
        let atomicFlag,tyargsOpt,args,delayed,tpenv = 
            if pinfo.IsIndexer
            then GetMemberApplicationArgs delayed cenv env tpenv m 
            else ExprAtomicFlag.Atomic,None,[mksyn_unit m],delayed,tpenv
        if pinfo.IsStatic then error (Error (sprintf "property '%s' is static" nm,m));
        match delayed with 
        | DelayedSet(e2,m) :: delayed' ->
            let args = if pinfo.IsIndexer then args else []
            if nonNil(delayed') then error(Error("Invalid assignment",m));
        
            // Instance property setter 
            unifyE cenv env m overallTy (cenv.g.unit_ty);
            let meths = pinfos |> List.choose (fun pinfo -> if pinfo.HasSetter then Some(pinfo.SetterMethod,Some pinfo) else None) 
            if isNil meths then error (Error (sprintf "Property '%s' may not be set" nm,m));
            TcMethodApplicationThen cenv env overallTy tpenv tyargsOpt objArgs m nm ad DefinitelyMutates true meths NormalValUse (args @ [e2]) atomicFlag [] 
        | _ ->                   
            // Instance property getter
            let meths = pinfos |> List.choose (fun pinfo -> if pinfo.HasGetter then Some(pinfo.GetterMethod,Some pinfo) else None) 
            if isNil meths then error (Error (sprintf "Property '%s' is not readable" nm,m));
            TcMethodApplicationThen cenv env overallTy tpenv tyargsOpt objArgs m nm ad PossiblyMutates true meths NormalValUse args atomicFlag delayed 
        
    | Item_recdfield rfinfo ->
        // Get or set instance F# field or literal 
        RecdFieldInstanceChecks cenv.g ad m rfinfo;
        let tgty = rfinfo.EnclosingType
        let valu = is_struct_typ cenv.g tgty
        AddCxTypeMustSubsumeType env.DisplayEnv cenv.css m NoTrace tgty objExprTy; 
        let objExpr = if valu then objExpr else mk_coerce(objExpr,tgty,m,objExprTy)
        let _,ftinst,fieldTy = FreshenPossibleForallTy cenv.g m TyparFlexible (rfinfo.FieldType)
        match delayed with 
        | DelayedSet(e2,stmtRange) :: delayed' ->
            // Mutable value set: 'v <- e' 
            if nonNil(delayed') then error(Error("Invalid assignment",m));
            CheckRecdFieldMutation m env.DisplayEnv rfinfo ftinst;
            unifyE cenv env stmtRange overallTy (cenv.g.unit_ty);
            // Always allow subsumption on asignment to fields
            let e2',tpenv = TcExprFlex cenv true fieldTy env tpenv e2
            BuildRecdFieldSet cenv.g stmtRange env.DisplayEnv objExpr rfinfo ftinst e2',tpenv

        | _ ->

            // Instance F# Record or Class field 
            let objExpr' = mk_recd_field_get cenv.g (objExpr,rfinfo.RecdFieldRef,rfinfo.TypeInst,ftinst,m)
            PropagateThenTcDelayed cenv overallTy env tpenv m (MkApplicableExprWithFlex cenv env objExpr') fieldTy ExprAtomicFlag.Atomic delayed 
        
    | Item_il_field (ILFieldInfo(tinfo,fdef) as finfo) -> 
        // Get or set instance IL field 
        ILFieldInstanceChecks  cenv.g cenv.amap ad m finfo;
        let fref = finfo.ILFieldRef
        let exprty = FieldTypeOfILFieldInfo cenv.amap m  finfo
        let isValueType = finfo.IsValueType
        let valu = if isValueType then AsValue else AsObject
        let tinst = tinfo.TypeInst
        
        match delayed with 
        // Set instance IL field 
        | DelayedSet(e2,m) :: delayed' ->
            unifyE cenv env m overallTy (cenv.g.unit_ty);
            // Always allow subsumption on asignment to fields
            let e2',tpenv = TcExprFlex cenv true exprty env tpenv e2
            let expr = BuildILFieldSet cenv.g m objExpr finfo e2'
            expr,tpenv
        | _ ->        
            let expr = BuildILFieldGet cenv.g cenv.amap m objExpr finfo 
            PropagateThenTcDelayed cenv overallTy env tpenv m (MkApplicableExprWithFlex cenv env expr) exprty ExprAtomicFlag.Atomic delayed 

    | Item_event einfo -> 
        // Instance IL event (fake up event-as-value) 
        TcEventValueThen cenv overallTy env tpenv m (Some(objExpr,objExprTy)) einfo delayed
     
    | (Item_fake_intf_ctor _ | Item_delegate_ctor _) -> error (Error ("Constructors must be applied to arguments and cannot be used as first-class values. If necessary use an anonymous function '(fun arg1 ... argN -> new Type(arg1,...,argN))'", m))
    | _ -> error (Error ("The syntax 'expr.id' may only be used with record labels, properties and fields", m))

and TcEventValueThen cenv overallTy env tpenv m objDetails (einfo:EventInfo) delayed = 
    // Instance IL event (fake up event-as-value) 
    //let (ILEventInfo(tinfo,edef)) = einfo 
    let nm = einfo.EventName
    let ad = AccessRightsOfEnv env
    match objDetails, einfo.IsStatic with 
    | Some _, true -> error (Error (sprintf "event '%s' is static" nm,m));
    | None, false -> error (Error (sprintf "event '%s' is not static" nm,m));
    | _ -> ()

    let delegateType = einfo.GetDelegateType(cenv.amap,m)
    let invoke_minfo,delArgTys,delRetTy,_ = GetSigOfFunctionForDelegate cenv.infoReader delegateType m ad
    let objArgs = (Option.to_list (Option.map fst objDetails))
    MethInfoChecks cenv.g cenv.amap true objArgs (AccessRightsOfEnv env) m invoke_minfo;
    
    (* This checks for and drops the 'object' sender *)
    let args_ty = ArgsTypOfEventInfo cenv.infoReader m ad einfo
    if not (slotsig_has_void_rty (SlotSigOfMethodInfo cenv.amap m invoke_minfo)) then errorR (nonStandardEventError einfo.EventName m);
    let devent_ty = mk_fslib_IEvent2_ty cenv.g delegateType args_ty
    let ctorCall = mk_obj_ctor_call cenv.g m

    let valu = einfo.IsValueType

    let bindObjArgs f =
        match objDetails with 
        | None -> f []
        | Some (objExpr,objExprTy) -> mk_compgen_let_in m "eventTarget" objExprTy objExpr (fun (_,ve) -> f [ve]) 

    (* Bind the object target expression to make sure we only run its sdie effects once, and to make *)
    (* sure if it's a mutable reference then we dereference it - see FSharp 1.0 bug 942 *)
    let expr = 
        bindObjArgs (fun objVars -> 
            let mk_event_override nm (minfo:MethInfo) = 
               let thisVal,thise = mk_compgen_local m "this" devent_ty
               let dv,de = mk_compgen_local m "eventDelegate" delegateType
               let callExpr,_ = BuildMethodCall cenv env PossiblyMutates m false minfo NormalValUse [] objVars [de]
               TObjExprMethod(mk_slotsig(nm,devent_ty,[vara;varb], [],[[mk_slotparam (vara_ty,TopValInfo.unnamedTopArg1)]], None),[], [[thisVal];[dv]],callExpr,m)
       
            let overrides =
              let add_minfo = einfo.GetAddMethod(m)
              let remove_minfo = einfo.GetRemoveMethod(m)
              [ mk_event_override "AddHandler" add_minfo;
                mk_event_override "RemoveHandler" remove_minfo;
                (let fvty = (args_ty  --> cenv.g.unit_ty)
                 let fv,fe = mk_compgen_local m "eventDelegate" fvty
                 let thisVal,_ = mk_compgen_local m "this" devent_ty
                 let de = BuildNewDelegateExpr (Some einfo) cenv delegateType (invoke_minfo,delArgTys) (fe,fvty) m
                 let callExpr,_ = BuildMethodCall cenv env PossiblyMutates m false add_minfo NormalValUse [] objVars [de]
                 TObjExprMethod(mk_slotsig("Add",devent_ty,[vara;varb], [],
                                    [[mk_slotparam (varb_ty  --> cenv.g.unit_ty,TopValInfo.unnamedTopArg1)]], None),
                         [], [[thisVal];[fv]],callExpr,m));
               ]
            TExpr_obj(new_uniq(), devent_ty, None, ctorCall, overrides,[],m,SkipFreeVarsCache()))
    let exprty = devent_ty
    PropagateThenTcDelayed cenv overallTy env tpenv m (MkApplicableExprNoFlex cenv expr) exprty ExprAtomicFlag.Atomic delayed 
 

//-------------------------------------------------------------------------
// Method uses can calls
//------------------------------------------------------------------------- 

/// Typecheck method/member calls and uses of members as first-class values.
and TcMethodApplicationThen 
       cenv 
       env
       overallTy           // The type of the overall expression including "delayed". THe method "application" may actually be a use of a member as 
                    // a first-class function value, when this would be a function type. 
       tpenv 
       userTypeArgs // The return type of the overall expression including "delayed" 
       objArgs      // The 'obj' arguments in obj.M(...) and obj.M, if any 
       m           // The range of the whole application (REVIEW: check this in all cases)
       methodName  // string, name of the method 
       ad          // accessibility rights of the caller 
       mut         // what do we know/assume about whether this method will mutate or not? 
       isProp      // is this a property call? Used for better error messages and passed to BuildMethodCall 
       meths       // the set of methods we may be calling 
       isSuperInit // is this a special invocation, e.g. a super-class constructor call. Passed through to BuildMethodCall 
       args        // the _syntactic_ method arguments, not yet type checked. 
       atomicFlag  // is the expression atomic or not? 
       delayed     // further lookups and applications that follow this 
     =

    // Nb. args is always of List.length <= 1 except for indexed setters, when it is 2  
    let m = List.fold (fun m arg -> union_ranges m (range_of_synexpr arg)) m args

    // Work out if we know anything about the return type of the overall expression. If there are any delayed 
    // lookups then we don't know anything. 
    let exprTy = if isNil delayed then overallTy else new_inference_typ cenv ()

    // Call the helper below to do the real checking 
    let (expr,attributeAssignedNamedItems,delayed),tpenv = 
        TcMethodApplication false cenv env tpenv userTypeArgs objArgs m methodName ad mut isProp meths isSuperInit args exprTy delayed

    (* Give errors if some things couldn't be assigned *)
    if nonNil attributeAssignedNamedItems then (
        let (CallerNamedArg(id,CallerArg(_,m,_,_))) = List.hd attributeAssignedNamedItems
        errorR(Error(sprintf "The named argument '%s' did not match any argument or mutable property" id.idText,id.idRange));
    );


    // Resolve the "delayed" lookups 
    let exprty = (type_of_expr cenv.g expr)

    PropagateThenTcDelayed cenv overallTy env tpenv m (MkApplicableExprNoFlex cenv expr) exprty atomicFlag delayed 

and GetNewInferenceTypeForMethodArg cenv x =
    match x with 
    | Expr_paren(a,_) -> GetNewInferenceTypeForMethodArg cenv a
    | Expr_addrof(true,a,_,_) -> mk_byref_typ cenv.g (GetNewInferenceTypeForMethodArg cenv a)
    | Expr_lambda(_,_,_,a,_) -> (new_inference_typ cenv () --> GetNewInferenceTypeForMethodArg cenv a)
    | _ -> new_inference_typ cenv ()

/// Method calls, property lookups, attribute constructions etc. get checked through here 
and TcMethodApplication 
        checkingAttributeCall 
        cenv 
        env 
        tpenv 
        tyargsOpt
        objArgs 
        m 
        methodName 
        ad 
        mut 
        isProp 
        calledMethsAndProps 
        isSuperInit 
        curriedCallerArgs 
        exprTy 
        delayed
    =
 
    let denv = env.DisplayEnv
    let nenv = nenv_of_tenv env

    let isSimpleFormalArg (isParamArrayArg,isOutArg,optArgInfo) = not isParamArrayArg && not isOutArg && (optArgInfo = NotOptional)    
    
    let objArgTys = objArgs |> List.map (type_of_expr cenv.g)

    let calledMeths = calledMethsAndProps |> List.map fst

    // Uses of curried members are ALWAYS treated as if they are first class uses of members. 
    // Curried members may not be overloaded (checked at use-site for curried members brought into scope through extension members)
    let curriedCallerArgs,exprTy,delayed = 
        match calledMeths with 
        | [calledMeth] when calledMeth.NumArgs.Length > 1 ->
            [], new_inference_typ cenv (),[ for x in curriedCallerArgs -> DelayedApp(ExprAtomicFlag.NonAtomic,x,range_of_synexpr x) ] @ delayed
        | _ when calledMeths |> List.exists (fun calledMeth -> calledMeth.NumArgs.Length > 1) ->
            // This condition should only apply when multiple conflicting curried extension members are brought into scope
            error(Error("One or more of the overloads of this method has curried arguments. Consider redesigning these members to take arguments in tupled form",m))
        | _ -> 
            curriedCallerArgs,exprTy,delayed

    let candidates = calledMeths |> List.filter (IsMethInfoAccessible cenv.amap m ad)

    // Split the syntactic arguments (if any) into named and unnamed parameters 
    //
    // In one case (the second "single named item" rule) we delay the application of a
    // argument until we've produced a lambda that detuples an input tuple
    let curriedCallerArgsOpt, unnamedDelayedCallerArgExprOpt, exprTy = 
      match curriedCallerArgs with 
      | [] -> 
          None,None,exprTy
      | _ -> 
          let unnamedCurriedCallerArgs,namedCurriedCallerArgs = curriedCallerArgs |> List.map GetMethodArgs |> List.unzip 
          
          // There is an mismatch when _uses_ of indexed property setters in the tc.ml code that calls this function. 
          // The arguments are passed as if they are curried with arity [numberOfIndexParameters;1], however in the TAST, indexed property setters
          // are uncurried and have arity [numberOfIndexParameters+1].
          //
          // Here we work around this mismatch by crunching all property argument lists to uncirred form. 
          // Ideally the problem needs to be solved at its root cause at the callsites to this function
          let unnamedCurriedCallerArgs,namedCurriedCallerArgs = 
              if isProp then 
                  [List.concat unnamedCurriedCallerArgs], [List.concat namedCurriedCallerArgs]
              else 
                  unnamedCurriedCallerArgs,namedCurriedCallerArgs
          
          let MakeUnnamedCallerArgInfo x = (x, GetNewInferenceTypeForMethodArg cenv x,range_of_synexpr x)

          // "single named item" rule. This is where we have a single accessible method 
          //      member x.M(arg1) 
          // being used with  
          //      x.M (x,y) 
          // Without this rule this requires 
          //      x.M ((x,y)) 
          match candidates with 
          | [calledMeth] 
                when (namedCurriedCallerArgs |> List.forall (fun x -> List.length x = 0) && 
                      let curriedCalledArgs = calledMeth |> ParamAttribsOfMethInfo cenv.amap m 
                      curriedCalledArgs.Length = 1 &&
                      curriedCalledArgs.Head.Length = 1 && 
                      curriedCalledArgs.Head.Head |> isSimpleFormalArg) ->
              let unnamedCurriedCallerArgs = curriedCallerArgs |> List.map (MakeUnnamedCallerArgInfo >> List.singleton)
              let namedCurriedCallerArgs = namedCurriedCallerArgs |> List.map (fun _ -> [])
              (Some (unnamedCurriedCallerArgs,namedCurriedCallerArgs), None, exprTy)

          // "single named item" rule. This is where we have a single accessible method 
          //      member x.M(arg1,arg2) 
          // being used with  
          //      x.M p
          // We typecheck this as if it has been written "(fun (v1,v2) -> x.M(v1,v2))  p" 
          // Without this rule this requires 
          //      x.M (fst p,snd p) 
          | [calledMeth] 
                when (namedCurriedCallerArgs |> List.forall (fun x -> List.length x = 0) && 
                      unnamedCurriedCallerArgs.Length = 1 &&
                      unnamedCurriedCallerArgs.Head.Length = 1 && 
                      let curriedCalledArgs = calledMeth |> ParamAttribsOfMethInfo cenv.amap m 
                      curriedCalledArgs.Length = 1 &&
                      curriedCalledArgs.Head.Length > 1 &&
                      curriedCalledArgs.Head |> List.forall isSimpleFormalArg) ->

              // The call lambda has function type
              let exprTy = mk_fun_ty (new_inference_typ cenv ()) exprTy
              
              (None, Some unnamedCurriedCallerArgs.Head.Head, exprTy)

          | _ ->
              let unnamedCurriedCallerArgs = unnamedCurriedCallerArgs |> List.mapSquared MakeUnnamedCallerArgInfo
              let namedCurriedCallerArgs = namedCurriedCallerArgs |> List.mapSquared (fun (isOpt,nm,x) -> nm,isOpt,x,GetNewInferenceTypeForMethodArg cenv x,range_of_synexpr x)

              (Some (unnamedCurriedCallerArgs, namedCurriedCallerArgs), None, exprTy)
    

    let CalledMethHasSingleArgumentGroupOfThisLength n (calledMeth:MethInfo) =
       let curriedMethodArgAttribs = ParamAttribsOfMethInfo cenv.amap m calledMeth
       curriedMethodArgAttribs.Length = 1 && 
       curriedMethodArgAttribs.Head.Length = n

    let GenerateMatchingSimpleArgumentTypes (calledMeth:MethInfo) =
        let curriedMethodArgAttribs = ParamAttribsOfMethInfo cenv.amap m calledMeth
        curriedMethodArgAttribs 
        |> List.map (List.filter isSimpleFormalArg)
        |> List.map (new_inference_typs cenv)

    let UnifyMatchingSimpleArgumentTypes exprTy (calledMeth:MethInfo) =
        let curriedArgTys = GenerateMatchingSimpleArgumentTypes calledMeth
        let returnTy = 
            (exprTy,curriedArgTys) ||>  List.fold (fun exprTy argTys -> 
                let domainTy,resultTy = UnifyFunctionType None cenv denv m exprTy
                unifyE cenv env m  domainTy (mk_tupled_ty cenv.g argTys);
                resultTy);
        curriedArgTys,returnTy

    // STEP 1. UnifyUniqueOverloading. This happens BEFORE we type check the arguments. 
    // Extract what we know about the caller arguments, either type-directed if 
    // no arguments are given or else based on the syntax of the arguments. 
    let uniquelyResolved,preArgumentTypeCheckingCalledMethGroup = 
        let dummyExpr = mk_unit cenv.g m
      
        // Build the CallerArg values for the caller's arguments. 
        // Fake up some arguments if this is the use of a method as a first class function 
        let unnamedCurriedCallerArgs,namedCurriedCallerArgs = 

            match curriedCallerArgsOpt,candidates with 
            // "single named item" rule. This is where we have a single accessible method 
            //      memeber x.M(arg1,...,argN) 
            // being used in a first-class way, i.e. 
            //      x.M  
            // Because there is only one accessible method info available based on the name of the item 
            // being accessed we know the number of arguments the first class use of this 
            // method will take. Optional and out args are _not_ included, which means they will be resolved 
            // to their default values (for optionals) and be part of the return tuple (for out args). 
            | None,[calledMeth] -> 
                let curriedArgTys,_ = UnifyMatchingSimpleArgumentTypes exprTy calledMeth
                let unnamedCurriedCallerArgs = curriedArgTys |> List.mapSquared (fun ty -> CallerArg(ty,m,false,dummyExpr))  
                let namedCurriedCallerArgs = unnamedCurriedCallerArgs |> List.map (fun _ -> [])
                unnamedCurriedCallerArgs, namedCurriedCallerArgs
                
            // "type directed" rule for first-class uses of ambiguous methods. 
            // By context we know a type for the input argument. If it's a tuple 
            // this gives us the a potential number of arguments expected. Indeed even if it's a variable 
            // type we assume the number of arguments is just "1". 
            | None,_ ->
            
                let domainTy,resultTy = UnifyFunctionType None cenv denv m exprTy
                let argTys = if is_unit_typ cenv.g domainTy then [] else  try_dest_tuple_typ cenv.g domainTy
                // Only apply this rule if a candidate method exists with this number of arguments
                let argTys = 
                    if candidates |> List.exists (CalledMethHasSingleArgumentGroupOfThisLength argTys.Length) then 
                       argTys
                    else 
                       [domainTy]
                let unnamedCurriedCallerArgs = [argTys |> List.map (fun ty -> CallerArg(ty,m,false,dummyExpr)) ]
                let namedCurriedCallerArgs = unnamedCurriedCallerArgs |> List.map (fun _ -> [])
                unnamedCurriedCallerArgs, namedCurriedCallerArgs
                

            | Some (unnamedCurriedCallerArgs,namedCurriedCallerArgs),_ -> 
                let unnamedCurriedCallerArgs = unnamedCurriedCallerArgs |> List.mapSquared (fun (x,xty,xm) -> CallerArg(xty,xm,false,dummyExpr))
                let namedCurriedCallerArgs = namedCurriedCallerArgs |> List.mapSquared (fun (id,isOpt,x,xty,xm) -> CallerNamedArg(id,CallerArg(xty,xm,isOpt,dummyExpr))) 
                unnamedCurriedCallerArgs, namedCurriedCallerArgs

        let callerArgCounts = (List.sumBy List.length unnamedCurriedCallerArgs, List.sumBy List.length namedCurriedCallerArgs)

        let mk_CalledMeth (minfo,pinfoOpt,allowParamArgs) = 
            let minst = FreshenMethInfo cenv m minfo
            let userTypeArgs = Option.otherwise tyargsOpt minst
            let allArgs = List.zip unnamedCurriedCallerArgs namedCurriedCallerArgs
            MakeCalledMeth(cenv.infoReader,checkingAttributeCall, FreshenMethInfo cenv, m,ad,minfo,minst,userTypeArgs,pinfoOpt,objArgTys,allArgs,allowParamArgs)

        let preArgumentTypeCheckingCalledMethGroup = 
            [ for (minfo,pinfoOpt) in calledMethsAndProps do
                let meth = mk_CalledMeth (minfo,pinfoOpt,true) 
                yield meth
                if meth.UsesParamArrayConversion then 
                    yield mk_CalledMeth (minfo,pinfoOpt,false) ]
                    
        let csenv = (MakeConstraintSolverEnv cenv.css m denv)
        let uniquelyResolved = UnifyUniqueOverloading csenv callerArgCounts methodName ad preArgumentTypeCheckingCalledMethGroup |> CommitOperationResult
        uniquelyResolved,preArgumentTypeCheckingCalledMethGroup

    // STEP 2. Type check arguments 
    let unnamedCurriedCallerArgs,namedCurriedCallerArgs,lambdaVars,returnTy,tpenv =  
    
        // STEP 2a. First extract what we know about the caller arguments, either type-directed if 
        // no arguments are given or else based on the syntax of the arguments. 
        let unnamedCurriedCallerArgs,namedCurriedCallerArgs,lambdaVars,returnTy,tpenv = 
            match curriedCallerArgsOpt with 
            | None ->
                let curriedArgTys,returnTy = 
                    match candidates with 
                    // "single named item" rule. This is where we have a single accessible method 
                    //      member x.M(arg1,...,argN) 
                    // being used in a first-class way, i.e. 
                    //      x.M  
                    // Because there is only one accessible method info available based on the name of the item 
                    // being accessed we know the number of arguments the first class use of this 
                    // method will take. Optional and out args are _not_ included, which means they will be resolved 
                    // to their default values (for optionals) and be part of the return tuple (for out args). 
                    | [calledMeth] -> 
                        UnifyMatchingSimpleArgumentTypes exprTy calledMeth
                    | _ -> 
                        let domainTy,returnTy = UnifyFunctionType None cenv denv m exprTy
                        let argTys = if is_unit_typ cenv.g domainTy then [] else  try_dest_tuple_typ cenv.g domainTy
                        // Only apply this rule if a candidate method exists with this number of arguments
                        let argTys = 
                            if candidates |> List.exists (CalledMethHasSingleArgumentGroupOfThisLength argTys.Length) then 
                                argTys                                  
                            else
                                [domainTy]
                        [argTys],returnTy
                        
                let lambdaVarsAndExprs = curriedArgTys |> List.mapiSquared (fun i j ty -> mk_compgen_local m ("arg"^string i^string j) ty)
                let unnamedCurriedCallerArgs = lambdaVarsAndExprs |> List.mapSquared (fun (v,e) -> CallerArg(type_of_expr cenv.g e,range_of_expr e,false,e))
                let namedCurriedCallerArgs = lambdaVarsAndExprs |> List.map (fun _ -> [])
                unnamedCurriedCallerArgs,namedCurriedCallerArgs,Some(List.map (List.map fst) lambdaVarsAndExprs), returnTy,tpenv

            | Some (unnamedCurriedCallerArgs,namedCurriedCallerArgs) ->
                let unnamedCurriedCallerArgs = unnamedCurriedCallerArgs |> List.mapSquared (fun (x,xty,xm) -> CallerArg(xty,xm,false,x)) 
                let unnamedCurriedCallerArgs,tpenv =  TcMethodArgs cenv env tpenv unnamedCurriedCallerArgs
                unnamedCurriedCallerArgs,namedCurriedCallerArgs,None,exprTy,tpenv

        // Now check the named arguments
        let namedCurriedCallerArgs = namedCurriedCallerArgs |> List.mapSquared (fun (id,isOpt,x,xty,xm) -> CallerNamedArg(id,CallerArg(xty,xm,isOpt,x))) 
        let namedCurriedCallerArgs,tpenv =  TcMethodNamedArgs cenv env tpenv namedCurriedCallerArgs
        unnamedCurriedCallerArgs,namedCurriedCallerArgs,lambdaVars,returnTy,tpenv

    let preArgumentTypeCheckingCalledMethGroup = 
       preArgumentTypeCheckingCalledMethGroup |> List.map (fun cmeth -> (cmeth.Method, cmeth.CalledTyArgs, cmeth.AssociatedPropertyInfo, cmeth.UsesParamArrayConversion))
    
    // STEP 3. Resolve overloading 
    /// Select the called method that's the result of overload resolution
    let (CalledMeth(finalCalledMethInfo,
                    finalCalledMethInst,
                    _,
                    _,
                    argSets,
                    _,
                    assignedNamedProps,
                    finalCalledPropInfoOpt,_, 
                    attributeAssignedNamedItems,
                    unnamedCalledOptArgs,
                    unnamedCalledOutArgs) as finalCalledMeth) = 

        let mk_CalledMeth2 (minfo:MethInfo,minst,pinfoOpt,usesParamArrayConversion) = 
            let userTypeArgs = Option.otherwise tyargsOpt minst
            
            if verbose then dprintf "--> minfo.Type = %s" (showL (typeL minfo.EnclosingType));
            
            let callerArgs = List.zip unnamedCurriedCallerArgs namedCurriedCallerArgs
            MakeCalledMeth(cenv.infoReader,checkingAttributeCall, FreshenMethInfo cenv, m,ad,minfo,minst,userTypeArgs,pinfoOpt,objArgTys,callerArgs,usesParamArrayConversion)
          
        let postArgumentTypeCheckingCalledMethGroup = List.map mk_CalledMeth2 preArgumentTypeCheckingCalledMethGroup

        let rty2 = (returnTy, (* dummy : this is unused *) mk_unit cenv.g m)

        let callerArgCounts = (unnamedCurriedCallerArgs.Length, namedCurriedCallerArgs.Length)
        let csenv = (MakeConstraintSolverEnv cenv.css m denv)
        
        // Commit unassociated constraints prior to member overload resolution where there is ambiguity 
        // about the possible target of the call. 
        if not uniquelyResolved then 
            GeneralizationHelpers.CanonicalizePartialInferenceProblem (cenv,denv,m)
                 (free_in_type_lr cenv.g false returnTy @
                  (unnamedCurriedCallerArgs |> List.collectSquared  (fun (CallerArg(xty,_,_,_)) -> free_in_type_lr cenv.g false xty)));

        if verbose then dprintf "--> TcMethodApplication (resolve overloading) @%a\n" output_range m;
        let result, errors = 
            ResolveOverloading csenv NoTrace methodName callerArgCounts ad postArgumentTypeCheckingCalledMethGroup true (Some rty2) 
        
        // Raise the errors from the constraint solving 
        RaiseOperationResult errors;
        match result with 
        | None -> error(InternalError("at least one error should be returned by failed method overloading",m))
        | Some res ->  res

    let assignedNamedArgs = argSets |> List.collect (fun argSet -> argSet.AssignedNamedArgs)
    let paramArrayCallerArgs = argSets |> List.collect (fun argSet -> argSet.ParamArrayCallerArgs)
    let unnamedCalledArgs = argSets |> List.collect (fun argSet -> argSet.UnnamedCalledArgs)
    let unnamedCallerArgs = argSets |> List.collect (fun argSet -> argSet.UnnamedCallerArgs)
    
    // STEP 4. Check the attributes on the method and the corresponding event/property, if any 

    finalCalledPropInfoOpt |> Option.iter (fun pinfo -> CheckPropInfoAttributes pinfo m |> CommitOperationResult) ;

    let isInstance = nonNil objArgs
    MethInfoChecks cenv.g cenv.amap isInstance objArgs ad m finalCalledMethInfo;

    if (argSets |> List.existsi (fun i argSet -> argSet.UnnamedCalledArgs |> List.existsi (fun j ca -> ca.Position <> (i,j)))) then
        warning(Deprecated("The unnamed arguments do not form a prefix of the arguments of the method called",m));


    // STEP 5. Build the argument list. Adjust for optional arguments, byref arguments and coercions.
    // For example, if you pass an F# reference cell to a byref then we must get the address of the 
    // contents of the ref. Likewise lots of adjustments are made for optional arguments etc.

    // Some of the code below must allocate temporary variables or bind other variables to particular values. 
    // As usual we represent variable allocators by expr -> expr functions 
    // which we then use to wrap the whole expression. These will either do nothing or pre-bind a variable. It doesn't
    // matter what order they are applied in as long as they are all composed together.
    let emptyPreBinder (e:expr) = e
    
    // For unapplied 'e.M' we first evaluate 'e' outside the lambda, i.e. 'let v = e in (fun arg -> v.M(arg))' 
    let objArgPreBinder,objArgs = 
        match objArgs,lambdaVars with 
        | [objArg],Some _   -> 
            let objArgTy = type_of_expr cenv.g objArg
            let v,ve = mk_compgen_local m "objectArg" objArgTy
            (fun body -> mk_compgen_let m v objArg body), [ve]

        | _ -> 
            emptyPreBinder,objArgs

  
    // Handle optional arguments
    let optArgPreBinder,allArgs,outArgExprs,outArgTmpBinds = 

        let normalUnnamedArgs = 
          (unnamedCalledArgs,unnamedCallerArgs) ||> List.map2 (fun called caller -> AssignedCalledArg(None,called,caller)) 

        let paramArrayArgs = 
          match finalCalledMeth.ParamArrayCalledArgOpt with 
          | None -> []
          | Some paramArrayCalledArg -> 
               let paramArrayCalledArgElementType = dest_il_arr1_typ cenv.g paramArrayCalledArg.Type
               let es = paramArrayCallerArgs  |> List.map (fun (CallerArg(callerArgTy,_,_,callerArgExpr)) -> mk_coerce_if_needed cenv.g paramArrayCalledArgElementType callerArgTy callerArgExpr)
               [ AssignedCalledArg(None,paramArrayCalledArg,CallerArg(paramArrayCalledArg.Type,m,false,TExpr_op(TOp_array,[paramArrayCalledArgElementType], es ,m))) ]

        // REVIEW: Move all this code into some isolated file, e.g. "optional.fs"
        //
        // Handle CallerSide optional arguments. 
        //
        // CallerSide optional arguments are largely for COM interop, e.g. to PIA assemblies for Word etc.
        // As a result we follow the VB spec here. To quote from an email exchange between the C# and VB teams.
        //
        //   "1.        If the parameter is statically typed as System.Object and does not have a value, then there are two cases:
        //       a.     The parameter may have the IDispatchConstantAttribute or IUnknownConstantAttribute attribute. If this is the case, the VB compiler then create an instance of the System.Runtime.InteropServices.DispatchWrapper /System.Runtime.InteropServices.UnknownWrapper type at the call site to wrap the value Nothing/null.
        //       b.     If the parameter does not have those two attributes, we will emit Missing.Value.
        //    2.        Otherwise, if there is a value attribute, then emit the default value.
        //    3.        Otherwise, we emit default(T).
        //    4.        Finally, we apply conversions from the value to the parameter type. This is where the nullable conversions take place for VB.
        //    - VB allows you to mark ref parameters as optional. The semantics of this is that we create a temporary 
        //        with type = type of parameter, load the optional value to it, and call the method. 
        //    - VB also allows you to mark arrays with Nothing as the optional value.
        //    - VB also allows you to pass intrinsic values as optional values to parameters 
        //        typed as Object. What we do in this case is we box the intrinsic value."
        //
        let optArgs,optArgPreBinder = 
          (emptyPreBinder,unnamedCalledOptArgs)
            ||> List.mapfold (fun wrapper (CalledArg(_,_,optArgInfo,_,_,calledArgTy) as calledArg) -> 
                  let wrapper2,expr = 

                      match optArgInfo with 
                      | NotOptional -> 
                          error(InternalError("Unexpected NotOptional",m))
                      | CallerSide dfltVal ->
                          let rec build = function 
                              | MissingValue -> 
                                  (* Add an I_nop if this is an initonly field to make sure we never recognize it as an lvalue. See mk_expra_of_expr. *)
                                  emptyPreBinder,mk_asm ([ mk_normal_ldsfld (fspec_Missing_Value cenv.g.ilg); I_arith AI_nop ],[],[],[calledArgTy],m)
                              | DefaultValue -> 
                                  emptyPreBinder,mk_ilzero(m,calledArgTy)
                              | Constant fieldInit -> 
                                  emptyPreBinder,TExpr_const(TcFieldInit m fieldInit,m,calledArgTy)  
                              | WrapperForIDispatch ->
                                  let tref = mk_tref(cenv.g.ilg.mscorlib_scoref ,"System.Runtime.InteropServices.DispatchWrapper")
                                  let mref = mk_ctor_mspec_for_nongeneric_boxed_tref(tref,[cenv.g.ilg.typ_Object]).MethodRef
                                  let expr = TExpr_op(TOp_ilcall((false,false,false,false,CtorValUsedAsSuperInit,false,false,None,mref),[],[],[cenv.g.obj_ty]),[],[mk_ilzero(m,calledArgTy)],m)
                                  emptyPreBinder,expr
                              | WrapperForIUnknown ->
                                  let tref = mk_tref(cenv.g.ilg.mscorlib_scoref ,"System.Runtime.InteropServices.UnknownWrapper")
                                  let mref = mk_ctor_mspec_for_nongeneric_boxed_tref(tref,[cenv.g.ilg.typ_Object]).MethodRef
                                  let expr = TExpr_op(TOp_ilcall((false,false,false,false,CtorValUsedAsSuperInit,false,false,None,mref),[],[],[cenv.g.obj_ty]),[],[mk_ilzero(m,calledArgTy)],m)
                                  emptyPreBinder,expr
                              | PassByRef (ty, dfltVal2) -> 
                                  let v,ve = mk_compgen_local m "defaultByrefArg" ty
                                  let wrapper2,rhs = build dfltVal2 
                                  (wrapper2 >> mk_compgen_let m v rhs), mk_val_addr m (mk_local_vref v)
                          build dfltVal

                      | CalleeSide -> 
                          emptyPreBinder,mk_ucase(mk_none_ucref cenv.g,[calledArgTy],[],m)

                  // Combine the variable allocators (if any)
                  let wrapper = (wrapper >> wrapper2)
                  let callerArg = CallerArg(calledArgTy,m,false,expr)
                  AssignedCalledArg(None,calledArg,callerArg),wrapper)


        // Handle optional arguments
        let wrapOptionalArg (AssignedCalledArg(idOpt,(CalledArg(_,_,optArgInfo,_,_,calledArgTy) as calledArg) ,CallerArg(callerArgTy,m,isOptCallerArg,expr)) as assignedArg) = 
            match optArgInfo with 
            | NotOptional -> 
                if isOptCallerArg then errorR(Error("The corresponding formal argument is not optional",m));
                assignedArg

            | _ -> 
                let expr = 
                    match optArgInfo with 
                    | CallerSide _ -> 
                        if isOptCallerArg then 
                            mk_ucase_field_get_unproven(expr,mk_some_ucref cenv.g,[dest_option_ty cenv.g callerArgTy],0,m) 
                        else 
                            expr
                    | CalleeSide -> 
                        if isOptCallerArg then 
                            // M(?x=bopt) when M(A) --> M(?x=Some(b.Value))
                            expr 
                        else                            
                            // M(x=b) when M(A) --> M(?x=Some(b :> A))
                            if is_option_ty cenv.g calledArgTy then 
                                let calledNonOptTy = dest_option_ty cenv.g calledArgTy 
                                mk_ucase(mk_some_ucref cenv.g,[calledNonOptTy],[mk_coerce_if_needed cenv.g calledNonOptTy callerArgTy expr],m)
                            else 
                                expr // should be unreachable 
                            
                    | _ -> failwith "Unreachable"
                AssignedCalledArg(idOpt,calledArg,CallerArg((type_of_expr cenv.g expr),m,isOptCallerArg,expr))

        let outArgsAndExprs,outArgTmpBinds = 
            unnamedCalledOutArgs 
              |> List.map (fun (CalledArg(_,_,_,_,_,calledArgTy) as calledArg) -> 
                let outArgTy = dest_byref_typ cenv.g calledArgTy
                let outv,outArgExpr = Tastops.mk_mut_compgen_local m "outArg" outArgTy // mutable! 
                let expr = mk_ilzero(m,outArgTy)
                let callerArg = CallerArg(calledArgTy,m,false,mk_val_addr m (mk_local_vref outv))
                (AssignedCalledArg(None,calledArg,callerArg), outArgExpr), mk_compgen_bind outv expr) 
              |> List.unzip

        let outArgs, outArgExprs = List.unzip outArgsAndExprs

        let allArgs =
            List.map wrapOptionalArg normalUnnamedArgs @ 
            List.map wrapOptionalArg assignedNamedArgs @ 
            paramArrayArgs @
            optArgs @ 
            outArgs
        
        let allArgs = 
            allArgs |> List.sortBy (fun x -> x.Position)

        optArgPreBinder,allArgs,outArgExprs,outArgTmpBinds
  
    // Handle adhoc argument conversions
    let coerce (AssignedCalledArg(_,CalledArg(_,_,optArgInfo,isOutArg,_,calledArgTy),CallerArg(callerArgTy,m,isOptCallerArg,e))) = 
    
       if is_byref_typ cenv.g calledArgTy && is_refcell_ty cenv.g callerArgTy then 
           TExpr_op(TOp_get_ref_lval,[dest_refcell_ty cenv.g callerArgTy],[e],m) 

       elif is_delegate_typ cenv.g calledArgTy && is_fun_typ cenv.g callerArgTy then 
           let minfo,delArgTys,delRetTy,_ = GetSigOfFunctionForDelegate cenv.infoReader calledArgTy m ad
           BuildNewDelegateExpr None cenv calledArgTy (minfo,delArgTys)  (e,callerArgTy) m

       // Note: out args do not need to be coerced 
       elif isOutArg then 
           e
       // Note: not all these casts are not reported in quotations 
       else 
           mk_coerce_if_needed cenv.g calledArgTy callerArgTy e 

    // Record the resolution of the named argument for the Language Service
    allArgs |> List.iter (fun (AssignedCalledArg(idOpt,CalledArg(_,_,_,_,_,calledArgTy),_)) ->
        match idOpt with 
        | None -> ()
        | Some id -> CallNameResolutionSink(id.idRange,nenv,Item_param_name(id),ItemOccurence.Use,nenv.eDisplayEnv,ad));

    let allArgsCoerced = List.map coerce  allArgs


    // Make the call expression 
    let expr,exprty = 
        BuildMethodCall cenv env mut m isProp finalCalledMethInfo isSuperInit finalCalledMethInst objArgs allArgsCoerced

    // Bind "out" parameters as part of the result tuple 
    let expr,exprty = 
        if isNil outArgTmpBinds then expr,exprty
        else 
            let outArgTys = outArgExprs |> List.map (type_of_expr cenv.g)
            let expr = if is_unit_typ cenv.g exprty then mk_compgen_seq m expr  (mk_tupled cenv.g  m outArgExprs outArgTys)
                       else  mk_tupled cenv.g  m (expr :: outArgExprs) (exprty :: outArgTys)
            let expr = mk_lets_bind m outArgTmpBinds expr
            expr, type_of_expr cenv.g expr

    // Handle post-hoc property assignments 
    let expr = 
        if isNil assignedNamedProps then expr else 
        // This holds the result of the call 
        let objv,objExpr = Tastops.mk_mut_compgen_local m "returnVal" exprty // mutable in case it's a struct 
        // This expression  mutates the properties on the result of the call
        let propSetExpr = 
            List.fold 
                (fun acc (AssignedItemSetter(id,setter,CallerArg(callerArgTy,m,isOptCallerArg,argExpr))) ->
                    if isOptCallerArg then error(Error("invalid optional assignment to a property or field",m));
                    
                    let action = 
                        match setter with 
                        | AssignedPropSetter(pinfo,pminfo,pminst) -> 
                            MethInfoChecks cenv.g cenv.amap true [objExpr] ad m pminfo;
                            let calledArgTy = List.hd (List.hd (ParamTypesOfMethInfo cenv.amap m pminfo pminst))
                            let argExpr = mk_coerce(argExpr,calledArgTy,m,callerArgTy)
                            BuildMethodCall cenv env DefinitelyMutates m true pminfo NormalValUse pminst [objExpr] [argExpr] |> fst 

                        | AssignedIlFieldSetter(finfo) ->
                            // Get or set instance IL field 
                            ILFieldInstanceChecks  cenv.g cenv.amap ad m finfo;
                            BuildILFieldSet cenv.g m objExpr finfo argExpr 
                        
                        | AssignedRecdFieldSetter(rfinfo) ->
                            RecdFieldInstanceChecks cenv.g ad m rfinfo; 
                            let _,ftinst,_ = FreshenPossibleForallTy cenv.g m TyparFlexible (rfinfo.FieldType)
                            CheckRecdFieldMutation m denv rfinfo ftinst;
                            BuildRecdFieldSet cenv.g m denv objExpr rfinfo ftinst argExpr 

                    // Record the resolution for the Language Service
                    CallNameResolutionSink(id.idRange,nenv,Item_prop_name(id),ItemOccurence.Use,nenv.eDisplayEnv,ad);

                    mk_compgen_seq m acc action)
                (mk_unit cenv.g m) 
                assignedNamedProps

        // now put them together 
        let expr = mk_compgen_let m objv expr  (mk_compgen_seq m propSetExpr objExpr)
        expr

    // Build the lambda expression if any 
    let expr = 
        match lambdaVars with 
        | None -> expr
        | Some curriedLambdaVars -> 
            let mkLambda vs expr = 
                match vs with 
                | [] -> mk_unit_delay_lambda cenv.g m expr 
                | _ -> mk_multi_lambda m vs (expr, type_of_expr cenv.g expr)
            List.foldBack mkLambda curriedLambdaVars expr

    let expr = 
        match unnamedDelayedCallerArgExprOpt with 
        | Some synArgExpr -> 
            match lambdaVars with 
            | Some [lambdaVars] -> 
                let argExpr,tpenv = TcExpr cenv (mk_tupled_vars_ty cenv.g lambdaVars) env tpenv synArgExpr 
                mk_appl cenv.g ((expr,type_of_expr cenv.g expr),[],[argExpr],m)
            | _ -> 
                error(InternalError("unreachable - expected some lambda vars for a tuple mismatch",m))
        | None -> 
            expr

    // Apply the PreBinders, if any 
    let expr = optArgPreBinder expr
    let expr = objArgPreBinder expr
    
    (expr,attributeAssignedNamedItems,delayed),tpenv
            
and TcMethodArgs cenv env tpenv args =  
    List.mapfoldSquared (TcMethodArg cenv env) tpenv args

and TcMethodArg  cenv env tpenv (CallerArg(ty,m,isOpt,e)) = 
    let e',tpenv = TcExpr cenv ty env tpenv e 
    CallerArg(ty,m,isOpt,e'),tpenv

and TcMethodNamedArgs cenv env tpenv args =  
    List.mapfoldSquared (TcMethodNamedArg cenv env) tpenv args

and TcMethodNamedArg  cenv env tpenv (CallerNamedArg(id,arg)) = 
    let arg',tpenv = TcMethodArg cenv env tpenv arg 
    CallerNamedArg(id,arg'),tpenv

/// Typecheck "new Delegate(fun x y z -> ...)" constructs
and TcNewDelegateThen cenv overallTy env tpenv m delty arg atomicFlag delayed =
    let ad = AccessRightsOfEnv env
    unifyE cenv env m overallTy delty;
    let minfo,delArgTys,delRetTy,fty = GetSigOfFunctionForDelegate cenv.infoReader delty m ad
    // We pass isInstance = true here because we're checking the rights to access the "Invoke" method
    MethInfoChecks cenv.g cenv.amap true [] (AccessRightsOfEnv env) m minfo;
    let args = GetMethodArgs arg
    match args with 
    | [farg],[] -> 
        let m = range_of_synexpr arg
        let (CallerArg(_,_,_,farg')),tpenv =  TcMethodArg cenv env tpenv (CallerArg(fty,m,false,farg))
        let expr = BuildNewDelegateExpr None cenv delty (minfo,delArgTys) (farg',fty) m 
        PropagateThenTcDelayed cenv overallTy env tpenv m (MkApplicableExprNoFlex cenv expr) delty atomicFlag delayed  
    | _ ->  error(Error("A delegate constructor must be passed a single function value",m))



and bind_letrec (binds:Bindings) m e = 
    if FlatList.isEmpty binds then 
        e 
    else 
        TExpr_letrec (binds,e,m,NewFreeVarsCache()) 

/// Process a sequence of iterated lets "let ... in let ... in ..." in a tail recursive way 
/// This avoids stack overflow on really larger "let" and "letrec" lists
and TcLinearLetExprs bodyChecker cenv env overallTy builder tpenv (processUseBindings,isRec,isUse,binds,body,m) =
    assert (not isUse || processUseBindings)

    if isRec then 
        // TcLinearLetExprs processes at most one recursive binding
        let binds = List.map (fun x -> RecBindingDefn(ExprContainerInfo,NoNewSlots,ExpressionBinding,x)) binds
        if isUse then errorR(Error("A binding may not be marked both 'use' and 'rec'",m));
        let binds,envinner,tpenv = 
          TcLetrec ErrorOnOverrides cenv env tpenv (binds,m,m)
        let body',tpenv = bodyChecker overallTy envinner tpenv body 
        let body' = bind_letrec (FlatList.of_list binds) m body'
        fst (builder (body',overallTy)),tpenv
    else 
        // TcLinearLetExprs processes multiple 'let' bindings in a tail recursive way
        // We process one binding, then look for additional linear bindings and accumulate the builder continuation.
        // Don't processes 'use' bindings (e.g. in sequence expressions) unless directed to.
        let mkf,envinner,tpenv =
          TcLetBinding cenv isUse env ExprContainerInfo ExpressionBinding tpenv (binds,m,range_of_synexpr body)
        let builder' x = builder (mkf x)
        match body with 
        | Expr_let (isRec',isUse',binds',body',m') when (not isUse' || processUseBindings) ->
            TcLinearLetExprs bodyChecker cenv envinner overallTy builder' tpenv (processUseBindings,isRec',isUse',binds',body',m')
        | _ -> 
            let body',tpenv = bodyChecker overallTy envinner tpenv body 
            fst (builder' (body',overallTy)),tpenv

/// Typecheck and compile pattern-matching constructs
and TcAndPatternCompileMatchClauses exprm matchm actionOnFailure cenv inputTy resultTy env tpenv clauses =
    let tclauses, tpenv = TcMatchClauses cenv inputTy resultTy env tpenv clauses
    let v,expr = CompilePatternForMatchClauses cenv env exprm matchm true actionOnFailure inputTy resultTy tclauses
    v,expr,tpenv

and TcMatchPattern cenv inputTy env tpenv (pat,optWhenExpr) =
    let m = range_of_synpat pat
    if verbose then  dprintf "--> TcMatchPattern@%a\n" output_range m;
    let patf',(tpenv,names,takenNames) = TcPat WarnOnUpperCase cenv env None (OptionalInline,permitInferTypars,noArgOrRetAttribs,false,None,false) (tpenv,Map.empty,Set.empty) inputTy pat
    let envinner,values,vspecMap = MakeAndPublishSimpleVals cenv env m names
    let optWhenExpr',tpenv = Option.mapfold (TcExpr cenv cenv.g.bool_ty envinner) tpenv optWhenExpr
    patf' (TcPatPhase2Input values),optWhenExpr',FlatList.of_list (NameMap.range vspecMap),envinner,tpenv

and TcMatchClauses cenv inputTy resultTy env tpenv clauses =
    List.mapfold (TcMatchClause cenv inputTy resultTy env) tpenv clauses 

and TcMatchClause cenv inputTy resultTy env tpenv (Clause(pat,optWhenExpr,e,patm,spTgt)) =
    let pat',optWhenExpr',vspecs,envinner,tpenv = TcMatchPattern cenv inputTy env tpenv (pat,optWhenExpr)
    let e',tpenv = TcExprThatCanBeCtorBody cenv resultTy envinner tpenv e
    TClause(pat',optWhenExpr',TTarget(vspecs, e',spTgt),patm),tpenv

and TcStaticOptimizationConstraint cenv env tpenv c = 
    match c with 
    | WhenTyparTyconEqualsTycon(tp,ty,m) ->
        if not cenv.g.compilingFslib then 
            errorR(Error("Static optimization conditionals are only for use within the F# library",m));
        let ty',tpenv = TcType cenv NewTyparsOK CheckCxs env tpenv ty
        let tp',tpenv = TcTypar cenv env NewTyparsOK tpenv tp
        TTyconEqualsTycon(mk_typar_ty tp', ty'),tpenv
    | WhenInlined(m) ->
        if not cenv.g.compilingFslib then 
            errorR(Error("Static optimization conditionals are only for use within the F# library",m));
        TTyconEqualsTycon(cenv.g.obj_ty, cenv.g.obj_ty),tpenv

/// Binding checking code, for all bindings including let bindings, let-rec bindings, member bindings and object-expression bindings and 
and TcNormalizedBinding declKind cenv env tpenv overallTy ctorThisVarRefCellOpt (enclosingDeclaredTypars,(ExplicitTyparInfo(declaredTypars,infer) as flex)) bind =
    let envinner = AddDeclaredTypars NoCheckForDuplicateTypars (enclosingDeclaredTypars@declaredTypars) env
    match bind with 

    | NormalizedBinding(vis,bkind,pseudo,mut,attrs,doc,_,valSynData,pat,BindingRhs(spatsL,rtyOpt,rhsExpr),bindingRange,spBind) ->
        
        let (ValSynData(memberFlagsOpt,valSynInfo,_)) = valSynData 
        (* Check the attributes of the binding *)
        let TcAttrs tgt attrs = 
            if not (DeclKind.CanHaveAttributes declKind) && nonNil(attrs) then errorR(Error("Attributes are not permitted on 'let' bindings in expressions or classes",bindingRange));
            TcAttributes cenv envinner tgt attrs
    
        let valAttribs = TcAttrs attrTgtBinding  attrs
        let inlineFlag = ComputeInlineFlag memberFlagsOpt pseudo mut valAttribs
        let argAttribs = 
            spatsL |> List.map (SynInfo.InferArgSynInfoFromSimplePats >> List.map (SynInfo.AttribsOfArgData >> TcAttrs attrTgtParameter))
        let retAttribs = 
            match rtyOpt with 
            | Some (_,_,retAttrs) -> TcAttrs attrTgtReturnValue retAttrs 
            | None -> [] 

        let argAndRetAttribs = ArgAndRetAttribs(argAttribs, retAttribs)

        if HasAttrib cenv.g cenv.g.attrib_DefaultValueAttribute valAttribs then 
            errorR(Error("The 'DefaultValue' attribute may only be used on 'val' declarations",bindingRange));
        
        let isThreadStatic = isThreadOrContextStatic cenv.g valAttribs
        if isThreadStatic then warning(DeprecatedThreadStaticBindingWarning(bindingRange));

        if HasAttrib cenv.g cenv.g.attrib_ConditionalAttribute valAttribs && isNone(memberFlagsOpt) then 
            errorR(Error("The 'ConditionalAttribute' attribute may only be used on members",bindingRange));

        if HasAttrib cenv.g cenv.g.attrib_EntryPointAttribute valAttribs then 
            if isSome(memberFlagsOpt) then 
                errorR(Error("The 'EntryPointAttribute' attribute may only be used on function definitions in modules",bindingRange))
            else 
                unifyE cenv env bindingRange overallTy (mk_array_typ cenv.g cenv.g.string_ty --> cenv.g.int_ty)

        if mut && pseudo then errorR(Error("Mutable values may not be marked 'inline'",bindingRange));
        if mut && nonNil declaredTypars then errorR(Error("Mutable values may not have generic parameters",bindingRange));
        let flex = if mut then dontInferTypars else flex
        if mut && nonNil spatsL then errorR(Error("Mutable function values should be written 'let mutable f = (fun args -> ...)'",bindingRange));
        let pseudo = 
            if pseudo && isNil(spatsL) && isNil(declaredTypars) then 
                errorR(Error("Only functions may be marked 'inline'",bindingRange));
                false
            else 
                pseudo 

        let compgen = false
        
        // Use the syntactic arity if we're defining a function 
        let partialTopValInfo = TranslateTopValSynInfo bindingRange (TcAttributes cenv env) valSynInfo

        // Check the pattern of the l.h.s. of the binding 
        let tcPatPhase2,(tpenv,nameToPrelimValSchemeMap,takenNames) = 
            TcPat AllIdsOK cenv envinner (Some(partialTopValInfo)) (inlineFlag,flex,argAndRetAttribs,mut,vis,compgen) (tpenv,NameMap.empty,Set.empty) overallTy pat
        
        // Add active pattern result names to the environment 
        let envinner = 
            match NameMap.range nameToPrelimValSchemeMap with 
            | [PrelimValScheme1(id,_,ty,_,_,_,_,_,_,_,_) ] ->
                begin match apinfo_of_vname (id.idText, id.idRange) with 
                | None -> envinner
                | Some apinfo -> ModifyNameResEnv (AddActivePatternResultTagsToNameEnv apinfo ty) envinner 
                end
            | _ -> envinner
        
        // Now tc the r.h.s. 
        // If binding a ctor then set the ugly counter that permits us to write ctor expressions on the r.h.s. 
        let isCtor = (match memberFlagsOpt with Some(memberFlags) -> memberFlags.MemberKind = MemberKindConstructor | _ -> false)
        let tc = 
            if isCtor then TcExprThatIsCtorBody ctorThisVarRefCellOpt 
            else TcExprThatCantBeCtorBody

        let rhsExpr',tpenv = tc cenv overallTy envinner tpenv rhsExpr

        if bkind = StandaloneExpression && not cenv.isScript then 
            UnifyUnitType cenv env.DisplayEnv bindingRange overallTy (Some rhsExpr') |> ignore<bool>;

        let hasLiteralAttr,konst = TcLiteral cenv overallTy env tpenv (valAttribs,rhsExpr)
        if hasLiteralAttr && isThreadStatic then 
            errorR(Error("A literal value may not be given the [<ThreadStatic>] or [<ContextStatic>] attributes",bindingRange));
        if hasLiteralAttr && mut then 
            errorR(Error("A literal value may not be marked 'mutable'",bindingRange));
        if hasLiteralAttr && pseudo then 
            errorR(Error("A literal value may not be marked 'inline'",bindingRange));
        if hasLiteralAttr && nonNil declaredTypars then 
            errorR(Error("Literal values may not have generic parameters",bindingRange));

        TBindingInfo(inlineFlag,true,valAttribs,doc,tcPatPhase2,flex,nameToPrelimValSchemeMap,rhsExpr',argAndRetAttribs,overallTy,bindingRange,spBind,compgen,konst),tpenv

and TcLiteral cenv overallTy env tpenv (attrs',e) = 
    let hasLiteralAttr = HasAttrib cenv.g cenv.g.attrib_LiteralAttribute attrs'
    if not hasLiteralAttr then  hasLiteralAttr,None else 
        let expr',tpenv = TcExpr cenv overallTy env tpenv e
        let rec eval e = 
            match strip_expr e with 
            | TExpr_const(c,_,_) -> c
            | _ -> 
                errorR(Error("This is not a valid constant expression",range_of_expr e));
                TConst_unit
        hasLiteralAttr,Some(eval expr') 
    
and TcBindingTyparDecls rigid cenv env m tpenv (SynValTyparDecls(synTypars,infer,synTyparConstraints)) = 
    let declaredTypars = TcTyparDecls cenv env synTypars
    let envinner = AddDeclaredTypars CheckForDuplicateTypars declaredTypars env
    let tpenv = TcTyparConstraints cenv NoNewTypars CheckCxs envinner tpenv synTyparConstraints
    if rigid then declaredTypars |> List.iter (SetTyparRigid cenv.g env.DisplayEnv m);
    ExplicitTyparInfo(declaredTypars,infer) 

and TcNonrecBindingTyparDecls cenv env tpenv bind = 
    match bind with 
    | NormalizedBinding(_,_,_,_,_,_,synTyparDecls,_,_,_,m,_) -> TcBindingTyparDecls true cenv env m tpenv synTyparDecls

and TcNonRecursiveBinding declKind cenv env tpenv ty b =
    let b = BindingNormalization.NormalizeBinding ValOrMemberBinding cenv env b
    let flex = TcNonrecBindingTyparDecls cenv env tpenv b
    TcNormalizedBinding declKind cenv env tpenv ty None ([],flex) b 

//-------------------------------------------------------------------------
// TcAttribute*
//------------------------------------------------------------------------

and TcAttribute cenv (env:tcEnv) attrTgt (Attr(tycon,arg,targetIndicator,m))  =
    let (typath,tyid) = List.frontAndBack tycon
    let tpenv = emptyTyparEnv
    let ty,tpenv =  
        let try1 n = 
            let tyid = mksyn_id tyid.idRange n
            let tycon = (typath @ [tyid])
            let ad = AccessRightsOfEnv env
            match ResolveTypeLongIdent cenv.nameResolver ItemOccurence.Use OpenQualified env.eNameResEnv ad tycon 0 with
            | Exception err -> raze(err)
            | _ ->  success(TcTypeAndRecover cenv NoNewTypars CheckCxs env tpenv (Type_app(Type_lid(tycon,m),[],false,m)) )in
        ForceRaise ((try1 (tyid.idText^"Attribute")) |> Outcome.otherwise (fun () -> (try1 tyid.idText)))
  (*   if not (ty <: System.Attribute) then error (Error("A custom attribute must be a subclass of System.Attribute",m)); *)


    let ad = AccessRightsOfEnv env

    if not (IsTypeAccessible cenv.g ad ty) then  errorR(Error("This type is not accessible from this code location",m));

    let tcref = tcref_of_stripped_typ cenv.g ty

    let conditionalCallDefine = 
        TyconRefTryBindAttrib cenv.g cenv.g.attrib_ConditionalAttribute tcref 
                 (function ([CustomElem_string (Some(msg)) ],_) -> Some msg | _ -> None)
                 (function (Attrib(_,_,[ AttribStringArg(msg) ],_,_))  -> Some(msg) | _ -> None)

    match conditionalCallDefine with 
    | Some(d) when not (List.mem d cenv.conditionalDefines) -> 
        []
    | _ ->

         (* REVIEW: take notice of inherited? *)
        let validOn,inherited = 
            let validOnDefault = 0x7fff
            let inheritedDefault = true
            if tcref.IsILTycon then 
                let tdef = tcref.ILTyconRawMetadata
                let tref = cenv.g.attrib_AttributeUsageAttribute.TypeRef
                
                match ILThingDecodeILAttrib cenv.g tref tdef.tdCustomAttrs with 
                | Some ([CustomElem_int32 validOn ],named) -> 
                    let inherited = 
                      (List.tryPick (function ("Inherited",_,_,CustomElem_bool res) -> Some res | _ -> None) named) 
                      +? (fun () -> inheritedDefault)
                    (validOn, inherited)
                | Some ([CustomElem_int32 validOn; CustomElem_bool allowMultiple; CustomElem_bool inherited ],_) -> 
                    (validOn, inherited)
                | _ -> 
                    (validOnDefault, inheritedDefault)
            else
                match (TryFindAttrib cenv.g cenv.g.attrib_AttributeUsageAttribute tcref.Attribs) with
                | Some(Attrib(_,_,[ AttribInt32Arg(validOn) ],_,_)) ->
                    (validOn, inheritedDefault)
                | Some(Attrib(_,_,[ AttribInt32Arg(validOn);
                                    AttribBoolArg(allowMultiple);
                                    AttribBoolArg(inherited)],_,_)) ->
                    (validOn, inherited)
                | Some _  ->
                    warning(Error("Unexpected condition in imported assembly: failed to decode AttributeUsage attribute",m))
                    (validOnDefault, inheritedDefault)                    
                | _ -> 
                    (validOnDefault, inheritedDefault)
        let possibleTgts = validOn &&& attrTgt
        let possibleTgts = 
            match targetIndicator with
            | Some id when id.idText = "assembly" -> attrTgtAssembly
            | Some id when id.idText = "module" -> attrTgtModule
            | Some id when id.idText = "return" -> attrTgtReturnValue
            | Some id when id.idText = "field" -> attrTgtField
            | Some id when id.idText = "property" -> attrTgtProperty
            | Some id when id.idText = "param" -> attrTgtParameter
            | Some id when id.idText = "type"    -> attrTgtTyconDecl
            | Some id when id.idText = "constructor"    -> attrTgtConstructor
            | Some id when id.idText = "event"    -> attrTgtEvent
            | Some id     -> 
                errorR(Error("Unrecognized attribute target. Valid attribute targets are 'assembly', 'module', 'type', 'method', 'property', 'return', 'param', 'field', 'event', 'constructor'",id.idRange)); 
                possibleTgts
            | _ -> possibleTgts
        if (possibleTgts &&& attrTgt) = 0 then 
            if (possibleTgts = attrTgtAssembly || possibleTgts = attrTgtModule) then 
                errorR(Error("This attribute is not valid for use on this language element. Assembly attributes should be attached to a 'do ()' declaration, if necessary within an F# module",m))
            else
                errorR(Error("This attribute is not valid for use on this language element",m));

        let item,rest = ForceRaise (ResolveObjectConstructor cenv.nameResolver env.DisplayEnv m ad ty)
        let attrib = 
            match item with 
            | Item_ctor_group(methodName,minfos) ->
                let meths = minfos |> List.map (fun minfo -> minfo,None) 

                let (expr,namedCallerArgs,_),tpenv = 
                  TcMethodApplication true cenv env tpenv None [] m methodName ad PossiblyMutates false meths NormalValUse [arg] (new_inference_typ cenv ())  []

                unifyE cenv env m ty (type_of_expr cenv.g expr);
                
                let mkAttribExpr e = 
                    AttribExpr(e,EvalAttribArg cenv.g e)

                let namedAttribArgMap = 
                  namedCallerArgs |> List.map (fun (CallerNamedArg(id,CallerArg(argtyv,m,isOpt,expr))) ->
                    if isOpt then error(Error("Optional arguments may not be used in custom attributes",m));
                    let m = range_of_expr expr
                    let nm,isProp,argty = 
                      let item,rest = ResolveLongIdentInType cenv.nameResolver (nenv_of_tenv env) Nameres.LookupKind.Expr m ad [id] IgnoreOverrides DefaultTypeNameResInfo ty
                      let nenv = nenv_of_tenv env 
                      CallNameResolutionSink(id.idRange,nenv,Item_prop_name(id),ItemOccurence.Use,nenv.eDisplayEnv,ad);
                      match item with   
                      | Item_property (_,[pinfo]) -> 
                          if not pinfo.HasSetter then 
                            errorR(Error("This property may not be set",m));
                          id.idText,true,PropertyTypeOfPropInfo cenv.amap m pinfo
                      | Item_il_field finfo -> 
                          CheckILFieldInfoAccessible cenv.g cenv.amap m ad finfo;
                          CheckILFieldAttributes cenv.g finfo m;
                          id.idText,false,FieldTypeOfILFieldInfo cenv.amap m finfo
                      | Item_recdfield rfinfo when not rfinfo.IsStatic -> 
                          CheckRecdFieldInfoAttributes cenv.g rfinfo m  |> CommitOperationResult;        
                          CheckRecdFieldInfoAccessible m ad rfinfo;
                          (* This uses the F# backend name mangling of fields.... *)
                          let nm =  gen_field_name rfinfo.Tycon rfinfo.RecdField
                          nm,false,rfinfo.FieldType
                      |  _ -> 
                          errorR(Error("This property or field was not found on this custom attribute type",m)); 
                          id.idText,false,cenv.g.unit_ty

                    AddCxTypeMustSubsumeType env.DisplayEnv cenv.css m NoTrace argty argtyv;

                    AttribNamedArg(nm,argty,isProp,mkAttribExpr expr))

                //if tcref_eq cenv.g tcref cenv.g.

                match expr with 
                | TExpr_op(TOp_ilcall((virt,protect,valu,_,_,_,_,_,mref),[],[],rtys),[],args,m) -> 
                    if valu then error (Error("A custom attribute must be a reference type",m));
                    if args.Length <> mref.ArgTypes.Length then error (Error("The number of args for a custom attribute does not match the expected number of args for the attribute constructor",m));
                    let args = args |> List.map mkAttribExpr
                    Attrib(tcref,ILAttrib(mref),args,namedAttribArgMap,m)

                | TExpr_app(TExpr_val(vref,_,_),_,_,args,_) -> 
                    let try_dest_unit_or_tuple = function TExpr_const(TConst_unit,_,_) -> [] | expr -> try_dest_tuple expr
                    let args = args |> List.collect (try_dest_unit_or_tuple)  |> List.map mkAttribExpr
                    Attrib(tcref,FSAttrib(vref),args,namedAttribArgMap,m)

                | _ -> 
                    error (Error("A custom attribute must invoke an object constructor",m))

            | _ -> 
                error(Error("Attribute expressions must be calls to object constructors",m))

        [ (possibleTgts, attrib) ]

and TcAttributesWithPossibleTargets cenv env attrTgt attrs = 

    attrs |> List.collect (fun attr -> 
        try 
            let attribs = TcAttribute cenv env attrTgt attr
            
            // This is where we place any checks that completely exclude the use of some particular 
            // attributes from F#.
            if HasAttrib cenv.g cenv.g.attrib_TypeForwardedToAttribute (List.map snd attribs) then 
                let m = match attr with Attr(_,_,_,m) -> m
                errorR(Error("This attribute may not be used in this version of F#",m));
            attribs
        with e -> 
            let m = match attr with Attr(_,_,_,m) -> m
            errorRecovery e m; 
            []) 

and TcAttributes cenv env attrTgt attrs = 
    TcAttributesWithPossibleTargets cenv env attrTgt attrs |> List.map snd

//-------------------------------------------------------------------------
// TcLetBinding
//------------------------------------------------------------------------

and TcLetBinding cenv isUse env containerInfo declKind tpenv (binds,bindsm,scopem) =

    // Typecheck all the bindings...
    let binds',tpenv = List.mapfold (fun tpenv b -> TcNonRecursiveBinding declKind cenv env tpenv (new_inference_typ cenv ()) b) tpenv binds
    let (ContainerInfo(altActualParent,tcrefContainerInfo)) = containerInfo
    
    // Canonicalize constraints prior to generalization 
    let denv = env.DisplayEnv
    GeneralizationHelpers.CanonicalizePartialInferenceProblem (cenv,denv,bindsm) 
        (binds' |> List.collect (fun tbinfo -> 
            let (TBindingInfo(inlineFlag,immut,attrs,doc,tcPatPhase2,flex,nameToPrelimValSchemeMap,rhsExpr,_,tauTy,m,_,_,_)) = tbinfo
            let (ExplicitTyparInfo(declaredTypars,canInferTypars)) = flex
            let maxInferredTypars = (free_in_type_lr cenv.g false tauTy)
            declaredTypars @ maxInferredTypars));

    let freeInEnv = GeneralizationHelpers.ComputeUngeneralizableTypars env

    // Generalize the bindings...
    (((fun x -> x), env, tpenv), binds') ||> List.fold (fun (mkf_sofar,env,tpenv) tbinfo -> 
        let (TBindingInfo(inlineFlag,immut,attrs,doc,tcPatPhase2,flex,nameToPrelimValSchemeMap,rhsExpr,_,tauTy,m,spBind,_,konst)) = tbinfo
        let enclosingDeclaredTypars  = []
        let (ExplicitTyparInfo(declaredTypars,canInferTypars)) = flex
        let allDeclaredTypars  =  enclosingDeclaredTypars @ declaredTypars
        let generalizedTypars,prelimValSchemes2 = 
            let canInferTypars = GeneralizationHelpers. ComputeCanInferTypars(declKind,containerInfo.ParentRef,canInferTypars,None,declaredTypars,m)

            let maxInferredTypars = (free_in_type_lr cenv.g false tauTy)

            let generalizedTypars = GeneralizationHelpers.ComputeGeneralizedTypars(cenv,env, m, immut, freeInEnv, canInferTypars, GeneralizationHelpers.CanGeneralizeConstrainedTyparsForDecl(declKind), inlineFlag, Some rhsExpr, allDeclaredTypars, maxInferredTypars,tauTy,false)

            let prelimValSchemes2 = GeneralizeVals cenv denv enclosingDeclaredTypars  [] generalizedTypars nameToPrelimValSchemeMap

            generalizedTypars,prelimValSchemes2

        // REVIEW: this scopes generalized type variables. Ensure this is handled properly 
        // on all other paths. 
        let tpenv = HideUnscopedTypars generalizedTypars tpenv
        let valSchemes = NameMap.map (UseCombinedArity cenv.g declKind rhsExpr) prelimValSchemes2
        let values = MakeAndPublishVals cenv env (altActualParent,false,declKind,ValNotInRecScope,valSchemes,attrs,doc,konst)
        let pat' = tcPatPhase2 (TcPatPhase2Input values)
        let prelimRecValues = NameMap.map fst values
        
        // Now bind the r.h.s. to the l.h.s. 
        let rhse = mk_tlambda m generalizedTypars (rhsExpr,tauTy)

        match pat' with 
        // Don't introduce temporary or 'let' for 'match against wild' or 'match against unit' 

        | (TPat_wild _ | TPat_const (TConst_unit,_)) when not isUse && isNil generalizedTypars ->
            let mk_seq_bind (tm,tmty) = (mk_seq SequencePointsAtSeq m rhse tm, tmty)
            (mk_seq_bind << mkf_sofar,env,tpenv)
            
        | _ -> 

        // nice: don't introduce awful temporary for r.h.s. in the 99% case where we know what we're binding it to 
        let tmp,tmpe,pat'' = 
                match pat' with 
                // nice: don't introduce awful temporary for r.h.s. in the 99% case where we know what we're binding it to 
                | TPat_as (pat1,PBind(v,TypeScheme(generalizedTypars',_,_)),m1) 
                    when List.lengthsEqAndForall2 tpspec_eq generalizedTypars generalizedTypars' -> 
                      v,expr_for_val v.Range v, pat1

                | _ when mustinline(inlineFlag)  -> error(Error("invalid inline specification",m))

                | _ -> 
                    let tmp,tmpe = Tastops.mk_compgen_local m "patternInput" (generalizedTypars +-> tauTy)
                    if isUse then 
                        errorR(Error("'use' bindings must be of the form 'use <var> = <expr>'",m));
                    
                    // This assignement forces representation as module value, to maintain the invariant from the 
                    // type checker that anything related to binding module-level values is marked with an 
                    // val_top_repr_info, val_actual_parent and is_topbind
                    if (DeclKind.MustHaveArity declKind) then 
                        AdjustValToTopVal tmp altActualParent (InferArityOfExprBinding cenv.g tmp rhse);
                    tmp,tmpe,pat'
        let mk_rhs_bind (tm,tmty) = (mk_let spBind m tmp rhse tm),tmty
        let allValsDefinedByPattern = (NameMap.range prelimRecValues |> FlatList.of_list)
        let mk_pat_bind (tm,tmty) =
            let valsDefinedByMatching = FlatListSet.remove vspec_eq tmp allValsDefinedByPattern
            let matchx = CompilePatternForMatch cenv env m m true Incomplete (tmp,generalizedTypars) [TClause(pat'',None,TTarget(valsDefinedByMatching,tm,SuppressSequencePointAtTarget),m)] tmty
            let matchx = if (DeclKind.ConvertToLinearBindings declKind) then LinearizeTopMatch cenv.g altActualParent matchx else matchx
            matchx,tmty

        let mk_cleanup (tm,tmty) =
            if isUse then 
                (allValsDefinedByPattern,(tm,tmty)) ||> FlatList.foldBack (fun v (tm,tmty) ->
                    AddCxTypeMustSubsumeType denv cenv.css v.Range NoTrace cenv.g.system_IDisposable_typ v.Type;
                    let cleanupE = BuildDisposableCleanup cenv env m v
                    mk_try_finally cenv.g (tm,cleanupE,m,tmty,NoSequencePointAtTry,NoSequencePointAtFinally),tmty)
            else (tm,tmty) 
                
        ((mk_rhs_bind << mk_pat_bind << mk_cleanup << mkf_sofar),
         AddLocalValMap scopem prelimRecValues env,
         tpenv))

/// Return binds corresponding to the linearised let-bindings.
/// This reveals the bound items, e.g. when the lets occur in incremental object defns.
/// RECAP:
///   The LHS of let-bindings are patterns.
///   These patterns could fail, e.g. "let Some x = ...".
///   So letbindings could contain a fork at a match construct, with one branch being the match failure.
///   If bindings are linearised, then this fork is pushed to the RHS.
///   In this case, the let bindings type check to a sequence of bindings.
and TcLetBindings cenv env containerInfo declKind tpenv (binds,bindsm,scopem) =
    assert(DeclKind.ConvertToLinearBindings declKind);
    let isUse = false // 'use' bindings not valid in classes 
    let mkf,env,tpenv = TcLetBinding cenv false env containerInfo declKind tpenv (binds,bindsm,scopem)
    let unite = mk_unit cenv.g bindsm
    let expr,ty = mkf (unite,cenv.g.unit_ty)
    let rec StripLets acc = function
        | TExpr_let (bind,body,m,_)      ->  StripLets (TMDefLet(bind,m) :: acc) body
        | TExpr_seq (e1,e2,NormalSeq,_,m)      ->  StripLets (TMDefDo(e1,m) :: acc) e2
        | TExpr_const (TConst_unit,_,_) -> List.rev acc
        | _ -> failwith "TcLetBindings: let sequence is non linear. Maybe a LHS pattern was not linearised?"
    let binds = StripLets [] expr
    binds,env,tpenv

and CheckMemberFlags g optIntfSlotTy newslotsOK overridesOK memberFlags m = 
    if newslotsOK = NoNewSlots && (memberFlags.MemberIsVirtual || memberFlags.MemberIsDispatchSlot) then 
      errorR(Error("Abstract members are not permitted in an augmentation - they must be defined as part of the type itself",m));
    if overridesOK = WarnOnOverrides && memberFlags.MemberIsOverrideOrExplicitImpl && isNone optIntfSlotTy then 
      warning(OverrideInIntrinsicAugmentation(m))
    if overridesOK = ErrorOnOverrides && memberFlags.MemberIsOverrideOrExplicitImpl then 
      error(Error("Method overrides and interface implementations are not permitted here",m))
    
/// Apply the pre-assumed knowledge available to type inference prior to looking at 
/// the _body_ of the binding. For example, in a letrec we may assume this knowledge 
/// for each binding in the letrec prior to any type inference. This might, for example, 
/// tell us the type of the arguments to a recursive function. 
and ApplyTypesFromArgumentPatterns (cenv,env,optArgsOK,ty,m,tpenv,BindingRhs(pushedPats,retInfoOpt,e),memberFlagsOpt:MemberFlags option) =  
    match pushedPats with
    | [] ->
        match retInfoOpt with 
        | None -> ()
        | Some (retInfoTy,m,_) -> 
            let retInfoTy,_ = TcTypeAndRecover cenv NewTyparsOK CheckCxs env tpenv retInfoTy
            unifyE cenv env m ty retInfoTy
        // Property setters always have "unit" return type
        match memberFlagsOpt with 
        | Some memFlags when memFlags.MemberKind = MemberKindPropertySet -> 
            unifyE cenv env m ty cenv.g.unit_ty
        | _ -> ()
            
    | p :: t -> 
        let domainTy,resultTy = UnifyFunctionType None cenv env.DisplayEnv m ty
        // We apply the type information from the patterns by type checking the
        // "simple" patterns against 'domainTy'. They get re-typechecked later. 
        ignore (TcSimplePats cenv optArgsOK domainTy env (tpenv,Map.empty,Set.empty) p);
        ApplyTypesFromArgumentPatterns (cenv,env,optArgsOK,resultTy,m,tpenv,BindingRhs(t,retInfoOpt,e),memberFlagsOpt)


/// Do the type annotations give the full and complete generic type? If so, enable generic recursion 
and ComputeIsComplete enclosingDeclaredTypars declaredTypars ty = 
    Zset.is_empty (List.fold (fun acc v -> Zset.remove v acc) 
                                  (free_in_type CollectAllNoCaching ty).FreeTypars 
                                  (enclosingDeclaredTypars@declaredTypars)) 


/// Determine if a uniquely-identified-abstract-slot exists for an override member (or interface member implementation) based on the information available 
/// at the syntactic definition of the member (i.e. prior to type inference). If so, we know the expected signature of the override, and the full slotsig 
/// it implements. Apply the inferred slotsig. *)
and ApplyAbstractSlotInference cenv denv envinner (bindingTy,m,synTyparDecls,declaredTypars,memberId,tcrefObjTy,renaming,objTy,optIntfSlotTy,valSynData,memberFlags,attribs) = 

    let ad = AccessRightsOfEnv envinner
    let typToSearchForAbstractMembers = 
        match optIntfSlotTy with 
        | Some (ty, abstractSlots) -> 
            // The interface type is in terms of the type's type parameters. 
            // We need a signature in terms of the values' type parameters. 
            ty,Some(abstractSlots) 
        | None -> 
            tcrefObjTy,None

    // Determine if a uniquely-identified-override exists based on the information 
    // at the member signature. If so, we know the type of this member, and the full slotsig 
    // it implements. Apply the inferred slotsig. 
    if memberFlags.MemberIsOverrideOrExplicitImpl then 
         
        let makeUniqueBySig meths = meths |> ListSet.setify (MethInfosEquivByNameAndSig EraseNone cenv.g cenv.amap m)
        match memberFlags.MemberKind with 
        | MemberKindMember -> 
             let dispatchSlots,dispatchSlotsArityMatch = 
                 GetAbstractMethInfosForSynMethodDecl(cenv.infoReader,ad,memberId,m,typToSearchForAbstractMembers,valSynData)

             let uniqueAbstractMethSigs = 
                 match dispatchSlots with 
                 | [] -> 
                     errorR(Error("No abstract or interface member was found that corresponds to this override",memberId.idRange));
                     []

                 | _ -> 
                     match dispatchSlotsArityMatch with 
                     | meths when meths |> makeUniqueBySig |> List.length = 1 -> 
                         meths
                     | [] -> 
                         errorR(Error("This override takes a different number of arguments to the corresponding abstract member",memberId.idRange));
                         []
                     | _ -> 
                         //printfn "no unique matching dispatch slot for %s at %s" memberId.idText (string_of_range memberId.idRange)
                         // We hit this case when it is ambiguous which abstract method is being implemented. 
                         []
               
             
             // If we determined a unique member then utilize the type information from the slotsig 
             let declaredTypars = 
                 match uniqueAbstractMethSigs with 
                 | uniqueAbstractMeth :: _ -> 

                     let uniqueAbstractMeth = InstMethInfo cenv.amap m renaming uniqueAbstractMeth
                     
                     let typarsFromAbsSlotAreRigid,typarsFromAbsSlot,argTysFromAbsSlot, retTyFromAbsSlot = 
                         FreshenAbstractSlot cenv.g cenv.amap m synTyparDecls uniqueAbstractMeth

                     let declaredTypars = (if typarsFromAbsSlotAreRigid then typarsFromAbsSlot else declaredTypars)

                     let absSlotTy = mk_meth_ty cenv.g argTysFromAbsSlot retTyFromAbsSlot

                     unifyE cenv envinner m bindingTy absSlotTy;
                     declaredTypars
                 | _ -> declaredTypars 

             // OK, detect 'default' members in the same type and mark them as implemented. 
             // REVIEW: consider moving this to the all-implemented analysis at the end of the 
             // type-checking scope, since we have inferred full signature types at that point. 
             // HOWEVER: we need to know which members have default implementations in order to be 
             // able to typecheck obejct expressions and sub-classes. 
             match dispatchSlotsArityMatch 
                       |> List.filter (fun virt -> tcref_eq cenv.g (tcref_of_stripped_typ cenv.g objTy) (tcref_of_stripped_typ cenv.g virt.EnclosingType)) with 
             | [] -> ()
             | meths when meths |> makeUniqueBySig |> List.length = 1 -> 
                 meths |> List.iter (fun meth ->
                     match meth with 
                     | (FSMeth(_,_,vref)) -> 
                         let virtMember = vref.MemberInfo.Value
                         if virtMember.IsImplemented then errorR(Error("This method already has a default implementation",memberId.idRange));
                         virtMember.IsImplemented <- true
                     | _ -> 
                         ())

             | _ ->  
                 errorR(Error("The method implemented by this default is ambiguous",memberId.idRange));

             // What's the type containing the abstract slot we're implementing? Used later on in MkMemberDataAndUniqueId. 
             // This type must be in terms of the enclosing type's formal type parameters, hence the application of revRenaming 

             let optInferredImplSlotTys = 
                 match optIntfSlotTy with 
                 | Some (x,_) -> [x]
                 | None -> uniqueAbstractMethSigs |> List.map (fun x -> x.EnclosingType)

             optInferredImplSlotTys,declaredTypars

        | MemberKindPropertyGet 
        | MemberKindPropertySet as k ->
           let dispatchSlots = GetAbstractPropInfosForSynPropertyDecl(cenv.infoReader,ad,memberId,m,typToSearchForAbstractMembers,k,valSynData)

           // Only consider those abstract slots where the get/set flags match the value we're defining 
           let dispatchSlots = 
               dispatchSlots 
               |> List.filter (fun pinfo -> 
                     (pinfo.HasGetter && k=MemberKindPropertyGet) ||
                     (pinfo.HasSetter && k=MemberKindPropertySet))
                                       
           // Find the unique abstract slot if it exists 
           let uniqueAbstractPropSigs = 
               match dispatchSlots with 
               | [] when not (CompileAsEvent cenv.g attribs) -> 
                   errorR(Error("No abstract property was found that corresponds to this override",memberId.idRange)); 
                   []
               | [uniqueAbstractProp] -> [uniqueAbstractProp]
               | _ -> 
                   // We hit this case when it is ambiguous which abstract property is being implemented. 
                   []

           // If we determined a unique member then utilize the type information from the slotsig 
           uniqueAbstractPropSigs |> List.iter (fun uniqueAbstractProp -> 

               let kIsGet = (k = MemberKindPropertyGet)

               if not (if kIsGet then uniqueAbstractProp.HasGetter else uniqueAbstractProp.HasSetter) then 
                   error(Error("This property overrides or implements an abstract property but the abstract property doesn't have a corresponding "^(if kIsGet then "getter" else "setter"),memberId.idRange));

               let uniqueAbstractMeth = if kIsGet then uniqueAbstractProp.GetterMethod else uniqueAbstractProp.SetterMethod

               let uniqueAbstractMeth = InstMethInfo cenv.amap m renaming uniqueAbstractMeth

               let typarsFromAbsSlotAreRigid,typarsFromAbsSlot,argTysFromAbsSlot, retTyFromAbsSlot = 
                    FreshenAbstractSlot cenv.g cenv.amap m synTyparDecls uniqueAbstractMeth

               if nonNil(typarsFromAbsSlot) then 
                   errorR(InternalError("Unexpected generic property",memberId.idRange));

               let absSlotTy = 
                   if (memberFlags.MemberKind = MemberKindPropertyGet) 
                   then mk_meth_ty cenv.g argTysFromAbsSlot retTyFromAbsSlot 
                   else 
                     match argTysFromAbsSlot with 
                     | [argTysFromAbsSlot] -> mk_tupled_ty cenv.g argTysFromAbsSlot --> cenv.g.unit_ty
                     | _ -> 
                         error(Error("Invalid signature for set member",memberId.idRange)); 
                         retTyFromAbsSlot --> cenv.g.unit_ty

               unifyE cenv envinner m bindingTy absSlotTy);
           

           // Now detect 'default' members in the type being defined and mark them as implemented. 
           // REVIEW: consider moving this to the all-implemented analysis at the end of the 
           // type-checking scope, since we have inferred full signature types at that point. 
           // HOWEVER: we need to know which members have default implementations in order to be 
           // able to typecheck object expressions and sub-classes. Hence it MUST be here. 
           begin 
               let assertImplements(vref:ValRef) =
                   let virtMember = the vref.MemberInfo
                   if virtMember.IsImplemented then errorR(Error("This property already has a default implementation",memberId.idRange));
                   virtMember.IsImplemented <- true
               let relevant = 
                   dispatchSlots 
                   |> List.filter (fun virt -> tcref_eq cenv.g (tcref_of_stripped_typ cenv.g objTy) (tcref_of_stripped_typ cenv.g virt.EnclosingType))
               match relevant  with 
               | [] -> ()
               | [FSProp(_,_,Some vref,_)] when k=MemberKindPropertyGet -> assertImplements(vref)
               | [FSProp(_,_,_,Some vref)] when k=MemberKindPropertySet -> assertImplements(vref)
               | _ ->  errorR(Error("The property implemented by this default is ambiguous",memberId.idRange));
           end;
           
           (* What's the type containing the abstract slot we're implementing? Used later on in MkMemberDataAndUniqueId. *)
           (* This type must be in terms of the enclosing type's formal type parameters, hence the application of revRenaming *)
           
           let optInferredImplSlotTys = 
               match optIntfSlotTy with 
               | Some (x,_) -> [ x ]
               | None -> List.map PropInfo.EnclosingType uniqueAbstractPropSigs

           optInferredImplSlotTys,declaredTypars

        | _ -> 
           match optIntfSlotTy with 
           | Some (x,_) -> [x], declaredTypars 
           | None -> [], declaredTypars

    else
       CheckForHiddenAbstractSlot (cenv,envinner,typToSearchForAbstractMembers,memberId,valSynData,memberFlags,m)

       [], declaredTypars 

and CheckForHiddenAbstractSlot (cenv,envinner,typToSearchForAbstractMembers,memberId,valSynData,memberFlags,m) =
     let ad = AccessRightsOfEnv envinner
     let denv = envinner.DisplayEnv
     // Check for definition of an instance member that obscures an abstract member. 
     // REVIEW: consider moving this to the all-implemented analysis at the end of the 
     // type-checking scope, since we have inferred full signature types at that point. 
     if memberFlags.MemberIsInstance then 
         match memberFlags.MemberKind with 
         | MemberKindMember -> 
             let dispatchSlots,dispatchSlotsArityMatch = GetAbstractMethInfosForSynMethodDecl(cenv.infoReader,ad,memberId,m,typToSearchForAbstractMembers,valSynData)
             match dispatchSlotsArityMatch with 
             | uniqueAbstractMeth2 :: _ -> warning(Error(sprintf "This new member hides the abstract member '%s'. Rename the member or use 'override' instead" (string_of_minfo cenv.amap m denv uniqueAbstractMeth2),memberId.idRange))
             | [] ->  ()
         | MemberKindPropertyGet 
         | MemberKindPropertySet as k ->
             let dispatchSlots = GetAbstractPropInfosForSynPropertyDecl(cenv.infoReader,ad,memberId,m,typToSearchForAbstractMembers,k,valSynData)
             let dispatchSlots = dispatchSlots |> List.filter (fun v -> if k = MemberKindPropertyGet then v.HasGetter else v.HasSetter)
             match dispatchSlots with 
             | uniqueAbstractProp :: _ -> 
                 let uniqueAbstractMeth = (if k = MemberKindPropertyGet then uniqueAbstractProp.GetterMethod else uniqueAbstractProp.SetterMethod) 
                 warning(Error(sprintf "This new member hides the abstract member '%s'. Rename the member or use 'override' instead" (string_of_minfo cenv.amap m denv uniqueAbstractMeth),memberId.idRange))
             | [] -> ()

         | _ -> ()
         (* end of lack of "override" warning analysis *)

and CheckForNonAbstractInterface declKind tcref memberFlags m =
    if is_interface_tcref tcref then 
        if memberFlags.MemberKind = MemberKindClassConstructor then 
            error(Error("Interfaces may not contain definitions of static initializers",m))
        elif memberFlags.MemberKind = MemberKindConstructor then 
            error(Error("Interfaces may not contain definitions of object constructors",m))
        elif memberFlags.MemberIsOverrideOrExplicitImpl or memberFlags.MemberIsVirtual then 
            error(Error("Interfaces may not contain definitions of member overrides",m))
        elif not (declKind=ExtrinsicExtensionBinding || memberFlags.MemberIsDispatchSlot ) then
            error(Error("Interfaces may not contain definitions of concrete members. You may need to define a constructor on your type, or use implicit class construction, to indicate that the type is a concrete implementation class",m))


//-------------------------------------------------------------------------
// TcLetrec - AnalyzeAndMakeRecursiveValue(s)
// 
//------------------------------------------------------------------------

and AnalyzeRecursiveStaticMemberOrValDecl (cenv,envinner:tcEnv,tpenv,declKind,newslotsOK,overridesOK,tcrefContainerInfo,vis1,id:ident,vis2,declaredTypars,memberFlagsOpt,thisIdOpt,bindingAttribs,valSynInfo,ty,bindingRhs,bindingRange,flex) =
    let vis = CombineVisibilityAttribs vis1 vis2 bindingRange
    // Check if we're defining a member, in which case generate the internal unique 
    // name for the member and the information about which type it is agumenting 
      
    match tcrefContainerInfo,memberFlagsOpt with 
    | (Some(tcref,optIntfSlotTy,baseValOpt,declaredTyconTypars),Some(memberFlags)) -> 
        assert (isNone(optIntfSlotTy))
      
        CheckMemberFlags cenv.g None newslotsOK overridesOK memberFlags bindingRange;
        CheckForNonAbstractInterface declKind tcref memberFlags id.idRange;
              
        if (deref_tycon tcref).IsExceptionDecl && 
           (memberFlags.MemberKind = MemberKindConstructor) then 
            error(Error("Constructors may not be specified in exception augmentations",id.idRange));                  

        let isExtrinsic = (declKind = ExtrinsicExtensionBinding)
        let _,enclosingDeclaredTypars,_,objTy,thisTy = FreshenObjectArgType cenv bindingRange recTyparsRigid tcref isExtrinsic declaredTyconTypars
        let envinner = AddDeclaredTypars CheckForDuplicateTypars enclosingDeclaredTypars envinner
        let envinner = MkInnerEnvForTyconRef cenv envinner tcref (declKind = ExtrinsicExtensionBinding)

        (* dprintf "AnalyzeAndMakeRecursiveValue, : enclosingDeclaredTypars = %s, ty = %s\n" (Layout.showL  (TyparsL enclosingDeclaredTypars)) (Layout.showL  (typeL ty));   *)

        let ctorThisVarRefCellOpt,baseValOpt = 
            match memberFlags.MemberKind with 
            | MemberKindConstructor  ->
                // A fairly adhoc place to put this check 
                if is_struct_tcref tcref && (match valSynInfo with ValSynInfo([[]],_) -> true | _ -> false) then
                    errorR(Error("Structs may not have an object constructor with no arguments. This is a restriction imposed on all .NET languages as structs automatically support a default constructor",bindingRange));

                if not tcref.IsFSharpObjectModelTycon then 
                    errorR(Error("Constructors may not be defined for this type",id.idRange));

                let ctorThisVarRefCellOpt = MakeAndPublishCtorThisRefCellVal cenv envinner thisIdOpt thisTy
                  
                // baseValOpt is the 'base' variable associated with the inherited portion of a class 
                // It is declared once on the 'inheritedTys clause, but a fresh binding is made for 
                // each member that may use it. 
                let baseValOpt = 
                    match SuperTypeOfType cenv.g cenv.amap bindingRange objTy with 
                    | Some(superTy) -> MakeAndPublishBaseVal cenv envinner (Option.map id_of_val baseValOpt) superTy 
                    | None -> None

                let domainTy = new_inference_typ cenv ()

                // This is the type we pretend a constructor has, because its implementation must ultimately appear to return a value of the given type 
                // This is somewhat awkward later in codegen etc. 
                unifyE cenv envinner bindingRange ty (domainTy --> objTy);

                ctorThisVarRefCellOpt,baseValOpt
                
            | _ -> 
                None,None
          
        let memberInfo,uniqueName = 
            let isExtrinsic = (declKind = ExtrinsicExtensionBinding)
            MkMemberDataAndUniqueId(cenv.g,tcref,isExtrinsic,bindingAttribs,[],memberFlags,valSynInfo,id)

        envinner,tpenv,uniqueName,Some(memberInfo),vis,vis2,ctorThisVarRefCellOpt,enclosingDeclaredTypars,baseValOpt,flex,bindingRhs,declaredTypars
        
    // non-member bindings. How easy. 
    | _ -> 
        envinner,tpenv,id,None,vis,vis2,None,[],None,flex,bindingRhs,declaredTypars
    

and AnalyzeRecursiveInstanceMemberDecl (cenv,envinner:tcEnv,tpenv,declKind,synTyparDecls,valSynInfo,flex,newslotsOK,overridesOK,vis1,thisId,memberId:ident,bindingAttribs,vis2,tcrefContainerInfo,memberFlagsOpt,ty,bindingRhs,bindingRange) =
    let vis = CombineVisibilityAttribs vis1 vis2 bindingRange
    let denv = envinner.DisplayEnv
    let (ExplicitTyparInfo(declaredTypars,infer)) = flex
    match tcrefContainerInfo,memberFlagsOpt with 
     // Normal instance members. 
     | (Some(tcref,optIntfSlotTy,baseValOpt,declaredTyconTypars),Some(memberFlags)) -> 
       
         CheckMemberFlags cenv.g optIntfSlotTy newslotsOK overridesOK memberFlags bindingRange;
       
         // Syntactically push the "this" variable across to be a lambda on the right 
         let bindingRhs = PushOnePatternToRhs true (mksyn_this_pat_var thisId) bindingRhs
       
         // The type being augmented tells us the type of 'this' 
         let isExtrinsic = (declKind = ExtrinsicExtensionBinding)
         let tcrefObjTy,enclosingDeclaredTypars,renaming,objTy,thisTy = FreshenObjectArgType cenv bindingRange recTyparsRigid tcref isExtrinsic declaredTyconTypars

         let envinner = AddDeclaredTypars CheckForDuplicateTypars enclosingDeclaredTypars envinner

         // If private, the member's accessibility is related to 'tcref' 
         let envinner = MkInnerEnvForTyconRef cenv envinner tcref (declKind = ExtrinsicExtensionBinding)

         let baseValOpt = if tcref.IsFSharpObjectModelTycon then baseValOpt else None

         // Apply the known type of 'this' 
         let bindingTy = new_inference_typ cenv ()
         unifyE cenv envinner bindingRange ty (thisTy --> bindingTy);

         CheckForNonAbstractInterface declKind tcref memberFlags memberId.idRange; 
         
         // Determine if a uniquely-identified-override List.exists based on the information 
         // at the member signature. If so, we know the type of this member, and the full slotsig 
         // it implements. Apply the inferred slotsig. 
         let optInferredImplSlotTys, declaredTypars = 
             ApplyAbstractSlotInference cenv denv envinner (bindingTy,bindingRange,synTyparDecls,declaredTypars,memberId,tcrefObjTy,renaming,objTy,optIntfSlotTy,valSynInfo,memberFlags,bindingAttribs)

         // Update the ExplicitTyparInfo to reflect the declaredTypars inferred from the abstract slot 
         let flex = ExplicitTyparInfo(declaredTypars,infer)

         // baseValOpt is the 'base' variable associated with the inherited portion of a class 
         // It is declared once on the 'inheritedTys clause, but a fresh binding is made for 
         // each member that may use it. 
         let baseValOpt = 
             match SuperTypeOfType cenv.g cenv.amap bindingRange objTy with 
             | Some(superTy) -> MakeAndPublishBaseVal cenv envinner (Option.map id_of_val baseValOpt) superTy 
             | None -> None

         let memberInfo,uniqueName = 
             MkMemberDataAndUniqueId(cenv.g,tcref,isExtrinsic,bindingAttribs,optInferredImplSlotTys,memberFlags,valSynInfo,memberId)

         envinner,tpenv,uniqueName,Some(memberInfo),vis,vis2,None,enclosingDeclaredTypars,baseValOpt,flex,bindingRhs,declaredTypars
     | _ -> 
         error(Error("recursive bindings that include member specifications can only occur as a direct augmentation of a type",bindingRange)) 

and AnalyzeRecursiveDecl (cenv,envinner,tpenv,declKind,synTyparDecls,declaredTypars,thisIdOpt,valSynInfo,flex,newslotsOK,overridesOK,vis1,declPattern,bindingAttribs,tcrefContainerInfo,memberFlagsOpt,ty,bindingRhs,bindingRange) =
    let rec AnalyzeRecursiveDeclPat p = 
        match p with  
        | Pat_typed(pat',cty,_) -> 
            let cty',tpenv = TcTypeAndRecover cenv NewTyparsOK CheckCxs envinner tpenv cty
            unifyE cenv envinner bindingRange ty cty';
            AnalyzeRecursiveDeclPat pat' 
        | Pat_attrib(pat',attribs,m) -> 
            error(Error("Attributes are not allowed within patterns",m));
            //AnalyzeRecursiveDeclPat pat' 

        // This is for the construct
        //    'let rec x = ... and do ... and y = ...' 
        // DEPRECATED IN pars.mly 
        | Pat_const (Const_unit, m) -> 
             let id = ident ("doval",m)
             AnalyzeRecursiveDeclPat (Pat_as (Pat_wild m, id,false,None,m))
             
        | Pat_as (Pat_wild _, id,_,vis2,m) -> 
            AnalyzeRecursiveStaticMemberOrValDecl (cenv,envinner,tpenv,declKind,newslotsOK,overridesOK,tcrefContainerInfo,vis1,id,vis2,declaredTypars,memberFlagsOpt,thisIdOpt,bindingAttribs,valSynInfo,ty,bindingRhs,bindingRange,flex)
            
        | Pat_instance_member(thisId, memberId,vis2,m) -> 
            AnalyzeRecursiveInstanceMemberDecl (cenv,envinner,tpenv,declKind,synTyparDecls,valSynInfo,flex,newslotsOK,overridesOK,vis1,thisId,memberId,bindingAttribs,vis2,tcrefContainerInfo,memberFlagsOpt,ty,bindingRhs,bindingRange)

        | _ -> error(Error("only simple variable patterns can be bound in 'let rec' constructs",bindingRange))
    AnalyzeRecursiveDeclPat declPattern


/// This is a major routine that generates the Val for a recursive binding 
/// prior to the analysis of the definition of the binding. This includes
/// members of all flavours (including properties, implicit class constructors
/// and overrides). At this point we perform override inference, to infer
/// which method we are overriding, in order to add constraints to the
/// implementation of the method.
and AnalyzeAndMakeRecursiveValue overridesOK cenv (env:tcEnv) (tpenv,nameValueMap) (NormalizedRecBindingDefn(containerInfo,newslotsOK,declKind,binding)) =

    // Pull apart the inputs
    let (NormalizedBinding(vis1,bindingKind,isInline,isMutable,bindingSynAttribs,bindingXmlDoc,synTyparDecls,valSynData,declPattern,bindingRhs,bindingRange,spBind)) = binding
    let (BindingRhs(_,_,bindingExpr)) = bindingRhs
    let (ValSynData(memberFlagsOpt,valSynInfo,thisIdOpt)) = valSynData 
    let (ContainerInfo(altActualParent,tcrefContainerInfo)) = containerInfo

    // Check the attributes on the declaration
    let bindingAttribs = TcAttributes cenv env attrTgtBinding bindingSynAttribs

    // Allocate the type inference variable for the inferred type
    let ty = new_inference_typ cenv () 
        
        
    let inlineFlag = ComputeInlineFlag memberFlagsOpt isInline isMutable bindingAttribs
    if isMutable then errorR(Error("only record fields and simple 'let' bindings may be marked mutable",bindingRange));


    // Typecheck the typar decls, if any
    let flex = TcBindingTyparDecls false cenv env bindingRange tpenv synTyparDecls
    let (ExplicitTyparInfo(declaredTypars,infer)) = flex
    let envinner = AddDeclaredTypars CheckForDuplicateTypars declaredTypars env
    
    (* dprintf "AnalyzeAndMakeRecursiveValue: declaredTypars = %s, ty = %s\n" (Layout.showL  (TyparsL declaredTypars)) (Layout.showL  (typeL ty));  *)
    
    // OK, analyze the declaration and return lots of information about it
    let envinner,tpenv,uniqueName,memberInfoOpt,vis,vis2,ctorThisVarRefCellOpt,enclosingDeclaredTypars,baseValOpt,flex,bindingRhs,declaredTypars = 
        AnalyzeRecursiveDecl (cenv,envinner,tpenv,declKind,synTyparDecls,declaredTypars,thisIdOpt,valSynInfo,flex,newslotsOK,overridesOK,vis1,declPattern,bindingAttribs,tcrefContainerInfo,memberFlagsOpt,ty,bindingRhs,bindingRange)

    let optArgsOK = isSome(memberFlagsOpt)

    // Assert the types given in the argument patterns
    (* dprintf "ApplyTypesFromArgumentPatterns cenv envinner (2, before): ty = %s\n" (Layout.showL  (typeL ty));  *)
    ApplyTypesFromArgumentPatterns(cenv,envinner,optArgsOK,ty,bindingRange,tpenv,bindingRhs,memberFlagsOpt);

    (* dprintf "ApplyTypesFromArgumentPatterns (%s): ty = %s, isComplete = %b\n" memberId.idText (Layout.showL  (typeL ty)) isComplete;  *) 

    // Do the type annotations give the full and complete generic type? 
    // If so, generic recursion can be used when using this type. 
    let isComplete =  ComputeIsComplete enclosingDeclaredTypars declaredTypars ty
    
    // NOTE: The type scheme here is normally not 'complete'!!!! The type is more or less just a type variable at this point. 
    // NOTE: toparity, type and typars get fixed-up after inference 
    let prelimTyscheme = TypeScheme(enclosingDeclaredTypars@declaredTypars,[],ty)
    let partialTopValInfo = TranslateTopValSynInfo bindingRange (TcAttributes cenv envinner) valSynInfo
    let topValInfo = UseSyntacticArity declKind prelimTyscheme partialTopValInfo
    let prelimValScheme = ValScheme(uniqueName,prelimTyscheme,topValInfo,memberInfoOpt,false,inlineFlag,NormalVal,vis,false,false,false)

    // Check the literal r.h.s., if any
    let _, konst = TcLiteral cenv ty env tpenv (bindingAttribs,bindingExpr)

    // Create the value 
    let vspec = MakeAndPublishVal cenv envinner (altActualParent,false,declKind,ValInRecScope(isComplete),prelimValScheme,bindingAttribs,bindingXmlDoc ,konst)

    let extraBindings,(tpenv,nameValueMap) = 
       let extraBindings = 
          [ for extraBinding in EventDeclarationNormalization.GenerateExtraBindings(cenv.g,bindingAttribs,binding) do
               yield (NormalizedRecBindingDefn(containerInfo,newslotsOK,declKind,extraBinding)) ]
       List.collectFold (AnalyzeAndMakeRecursiveValue overridesOK cenv env) (tpenv,nameValueMap) extraBindings

    // Reconstitute the binding with the unique name
    let revisedBinding = NormalizedBinding (vis1,bindingKind,isInline,isMutable,bindingSynAttribs,bindingXmlDoc,synTyparDecls,valSynData,mksyn_pat_var vis2 uniqueName,bindingRhs,bindingRange,spBind)

    // Create the RBInfo to use in later phases
    let rbinfo = RBInfo(containerInfo,enclosingDeclaredTypars,inlineFlag,vspec,flex,partialTopValInfo,memberInfoOpt,baseValOpt,ctorThisVarRefCellOpt,vis,ty,declKind)

    // Done - add the declared name to the List.map and return the bundle for use by TcLetrec 
    let nameValueMap = Map.add uniqueName.idText (vspec,prelimTyscheme) nameValueMap
    let primaryBinding = (rbinfo,revisedBinding)
    (primaryBinding::extraBindings),(tpenv,nameValueMap)
    // REVIEW: no more constraints allowed on declared typars 
    // REVIEW: this is related to changing recTyparsRigid to 'true' 
    // allDeclaredTypars |> List.iter SetTyparRigid; 


and AnalyzeAndMakeRecursiveValues  overridesOK cenv env tpenv binds = 
    List.collectFold (AnalyzeAndMakeRecursiveValue  overridesOK cenv env) (tpenv,Map.empty) binds


//-------------------------------------------------------------------------
// TcLetrecBinding
//-------------------------------------------------------------------------

and TcLetrecBinding cenv env scopem prelimRecValues tpenv (rbinfo,bind:NormalizedBinding) = 
    let (RBInfo(_,enclosingDeclaredTypars,_,v,flex,_,_,baseValOpt,ctorThisVarRefCellOpt,_,tau,declKind)) = rbinfo
    let (ExplicitTyparInfo(declaredTypars,_)) = flex
    let allDeclaredTypars = enclosingDeclaredTypars @ declaredTypars

    // dprintf "TcLetrec (before): tau = %s\n" (Layout.showL  (typeL tau));  

    // Notes on FSharp 1.0, 3187:
    //    - Progressively collect the "eligible for early generalization" set of bindings
    //    - After checking each binding, check this set to find generalizable bindings
    //    - The only reason we can't generalize is if a binding refers to type variables to which 
    //      additional constraints may be applied as part of checking a later binding
    //    - Compute the set by iteratively knocking out bindings that refer to type variables free in later bindings
    //    - Implementation notes:
    //         - Generalize by remap/substitution
    //         - Pass in "free in later bindings" by passing in the set of inference variables for the bindings, i.e. the binding types
    //         - For classes the bindings will include all members in a recursive group of types
    //
    
    //  Example 1: 
    //    let f() = g()   f : unit -> ?b
    //    and g() = 1     f : unit -> int, can generalize (though now monomorphic)

    //  Example 2: 
    //    let f() = g()   f : unit -> ?b
    //    and g() = []    f : unit -> ?c list, can generalize
    
    //  Example 3: 
    //    let f() = []   f : unit -> ?b, can generalize immediately
    //    and g() = []
    let envrec = AddLocalValMap scopem prelimRecValues env 
    let envrec = Option.fold_right (AddLocalVal scopem) baseValOpt envrec
    let envrec = Option.fold_right (AddLocalVal scopem) ctorThisVarRefCellOpt envrec

    // Members can access protected members of parents of the type, and private members in the type 
    let envrec = MakeInnerEnvForMember cenv envrec v

    let tbinding',tpenv = 
        TcNormalizedBinding declKind cenv envrec tpenv tau (Option.map mk_local_vref ctorThisVarRefCellOpt) (enclosingDeclaredTypars,flex) bind
        
    // dprintf "TcLetrec (%s, after): tau = %s\n" v.MangledName (Layout.showL  (typeL tau));    
    (try unifyE cenv env v.Range (allDeclaredTypars +-> tau) v.Type with e -> error (Recursion(env.DisplayEnv,v.Id,tau,v.Type,v.Range)));

    // dprintf "TcLetrec (%s, after unify): type_of_val v = %s\n" v.MangledName (Layout.showL  (typeL v.Type));  
    
    (rbinfo,tbinding'),tpenv

//-------------------------------------------------------------------------
// TcLetrecComputeGeneralizedTyparsForBinding
//-------------------------------------------------------------------------

/// Compute the type variables which may be generalized 
and TcLetrecComputeGeneralizedTyparsForBinding cenv env freeInEnv (rbinfo,tbinding) =
    let (RBInfo(containerInfo,enclosingDeclaredTypars,_,vspec,flex,_,memberInfoOpt,_,_,_,_,declKind)) = rbinfo
    let m = vspec.Range
    let (TBindingInfo(inlineFlag,immut,_,_,_,_,_,expr,_,_,m,_,_,_)) = tbinding
    let (ExplicitTyparInfo(declaredTypars,canInferTypars)) = flex
    let allDeclaredTypars = enclosingDeclaredTypars @ declaredTypars

    let memFlagsOpt = vspec.MemberInfo |> Option.map (fun memInfo -> memInfo.MemberFlags)
    let isCtor = (match memFlagsOpt with None -> false | Some memberFlags -> memberFlags.MemberKind = MemberKindConstructor)

    let canInferTypars = GeneralizationHelpers.ComputeCanInferTypars(declKind,containerInfo.ParentRef,canInferTypars,memFlagsOpt,declaredTypars,m)

    let tau = vspec.TauType
    let maxInferredTypars = (free_in_type_lr cenv.g false tau)
     
    GeneralizationHelpers.ComputeGeneralizedTypars (cenv,env,m,immut,freeInEnv,canInferTypars,GeneralizationHelpers.CanGeneralizeConstrainedTyparsForDecl(declKind),inlineFlag, Some(expr), allDeclaredTypars, maxInferredTypars,tau,isCtor)

/// Compute the type variables which may have member constraints that need to be canonicalized prior to generalization 
and TcLetrecComputeSupportForBinding cenv (rbinfo,tbinding) =
    let (RBInfo(_,enclosingDeclaredTypars,_,vspec,flex,_,_,_,_,_,_,_)) = rbinfo
    let (ExplicitTyparInfo(declaredTypars,canInferTypars)) = flex
    let allDeclaredTypars = enclosingDeclaredTypars @ declaredTypars
    let maxInferredTypars = free_in_type_lr cenv.g false vspec.TauType
    allDeclaredTypars @ maxInferredTypars

//-------------------------------------------------------------------------
// TcLetrecGeneralizeBinding
//------------------------------------------------------------------------

and TcLetrecGeneralizeBinding cenv denv generalizedTyparsForRecursiveBlock freeInEnv generalizedTypars (rbinfo,tbind) =

    // Generalise generalizedTypars from tbinding.
    // Any tp in generalizedTyparsForRecursiveBlock \ generalizedTypars has free choice, see comment in GeneralizeVal 
    let (RBInfo(_,enclosingDeclaredTypars,_,vspec,flex,partialTopValInfo,memberInfoOpt,_,_,vis,_,declKind)) = rbinfo
    let (TBindingInfo(inlineFlag,immut,attrs,_,_,_,_,expr,argAttribs,_,m,spBind,compgen,_)) = tbind
     
    let tps,tau = vspec.TypeScheme

    let pvalscheme1 = PrelimValScheme1(vspec.Id,flex,tau,Some(partialTopValInfo),memberInfoOpt,false,inlineFlag,NormalVal,argAttribs,vis,compgen)
    let pvalscheme2 = GeneralizeVal cenv denv enclosingDeclaredTypars generalizedTyparsForRecursiveBlock generalizedTypars pvalscheme1

    (*dprintf "TcLetrec (%s, before adjust): tau = %s, #generalizedTyparsForThisBinding = %d\n" vspec.MangledName (Layout.showL  (typeL tau)) (List.length generalizedTyparsForThisBinding); *)
    let valscheme = UseCombinedArity cenv.g declKind expr pvalscheme2 
    AdjustRecType cenv vspec valscheme;

    (*dprintf "TcLetrec (%s, after adjust): ty = %s\n" vspec.MangledName (Layout.showL  (typeL vspec.Type));  *)
    valscheme,TBind(vspec,expr,spBind)  (* NOTE: (vspec,'e) could be a TBind(vspec,'e) : Tast.Binding *)


and TcLetrecComputeCtorThisVarRefCellBinding cenv ctorThisVarRefCellOpt =
    ctorThisVarRefCellOpt |> Option.map (fun (v:Val) -> 
        let m = v.Range
        let ty = dest_refcell_ty cenv.g v.Type
        mk_compgen_bind v (mk_refcell cenv.g m ty (mk_null m ty)))

and TcLetrecBindCtorThisVarRefCell cenv (x,TBind(vspec,expr,spBind)) rbinfo = 
    let (RBInfo(_,_,_,_,_,_,_,baseValOpt,ctorThisVarRefCellOpt,_,_,_)) = rbinfo
    let expr = 
        match TcLetrecComputeCtorThisVarRefCellBinding cenv ctorThisVarRefCellOpt with 
        | None -> expr
        | Some bind -> 
            let m = range_of_expr expr
            let tps,vsl,body,returnTy = dest_top_lambda (expr,vspec.Type)
            mk_multi_lambdas m tps vsl (mk_let_bind m bind body, returnTy)
    let expr = 
        match baseValOpt with 
        | None -> expr
        | _ -> 
            let m = range_of_expr expr
            let tps,vsl,body,returnTy = dest_top_lambda (expr,vspec.Type)
            mk_basev_multi_lambdas m tps baseValOpt vsl (body, returnTy)
              
    x,TBind(vspec,expr,spBind)

and FixupLetrecBind cenv denv (valscheme,(TBind(vspec,expr,spBind))) =

    // Check coherence of generalization of variables for memberInfo members in generic classes 
    match vspec.MemberInfo with 
    | Some(memberInfo) -> 
       match PartitionValTypars cenv.g vspec with
       | Some(parentTypars,memberParentTypars,_,_,tinst) -> 
          ignore(SignatureConformance.CheckTypars cenv.g denv vspec.Range tyeq_env_empty memberParentTypars parentTypars)
       | None -> 
          errorR(Error("this member is not sufficiently generic",vspec.Range))
    | None -> ()

    // Fixup recursive references... 
    let (ValScheme(_,typeScheme,_,_,_,_,_,_,_,_,_)) = valscheme
    let fixupPoints = GetAllUsesOfRecValue cenv vspec

    AdjustAndForgetUsesOfRecValue cenv (mk_local_vref vspec) valscheme;

    // dprintf "TcLetrec (%s, after gen): #fixupPoints = %d, ty = %s\n" vspec.MangledName (List.length fixupPoints) (Layout.showL  (typeL vspec.Type)); 

    let expr = mk_poly_bind_rhs vspec.Range typeScheme expr

    fixupPoints,TBind(vspec,expr,spBind)
    
//-------------------------------------------------------------------------
// TcLetrec
//------------------------------------------------------------------------

and unionGeneralizedTypars typarSets = List.foldBack (ListSet.unionFavourRight typar_ref_eq) typarSets [] 
    

and TcLetrec  overridesOK cenv env tpenv (binds,bindsm,scopem) =

    // create prelimRecValues for the recursive items (includes type info from LHS of bindings) *)
    let binds = binds |> List.map (fun (RecBindingDefn(a,b,c,bind)) -> NormalizedRecBindingDefn(a,b,c,BindingNormalization.NormalizeBinding ValOrMemberBinding cenv env bind))
    let rbinfosAndBinds,(tpenv,nameToPrelimValueMap) = AnalyzeAndMakeRecursiveValues  overridesOK cenv env tpenv binds
    let prelimRecValues  = NameMap.map fst nameToPrelimValueMap

    // typecheck bindings 
    let bindingInfos,tpenv = 
        List.mapfold (TcLetrecBinding cenv env scopem prelimRecValues) tpenv rbinfosAndBinds

    // Decide which type parameters to generalize per binding 
    let denv = env.DisplayEnv
    
    let supportForBindings = bindingInfos |> List.collect (TcLetrecComputeSupportForBinding cenv)
    GeneralizationHelpers.CanonicalizePartialInferenceProblem (cenv,denv,bindsm) supportForBindings; 
     

    let freeInEnv = GeneralizationHelpers.ComputeUngeneralizableTypars env
    let generalizedTyparsL = List.map (TcLetrecComputeGeneralizedTyparsForBinding cenv env freeInEnv) bindingInfos
    let generalizedTyparsForRecursiveBlock = unionGeneralizedTypars generalizedTyparsL

    if verboseCC then List.iter (fun tp -> printf "TcLetrec: generalizedTyparsForRecursiveBlock = %s\n" (TyparL tp |> Layout.showL)) generalizedTyparsForRecursiveBlock;
    
    // Generalize the bindings. This gives valscheme and projects (vspec,x) 
    let vxbinds = List.map2 (TcLetrecGeneralizeBinding cenv env.DisplayEnv generalizedTyparsForRecursiveBlock freeInEnv) generalizedTyparsL bindingInfos
    let tpenv = HideUnscopedTypars (List.concat generalizedTyparsL) tpenv

    let vxbinds = List.map2 (TcLetrecBindCtorThisVarRefCell cenv) vxbinds (List.map fst bindingInfos)

    // Now that we know what we've generalized we can adjust the recursive references 
    let vxbinds = vxbinds |> List.map (FixupLetrecBind cenv env.DisplayEnv) 
    
    // Now eliminate any initialization graphs 
    let binds = 
        let bindsWithoutLaziness = vxbinds
        let mustHaveArity = 
            match rbinfosAndBinds with 
            | [] -> false
            | ((rbinfo,_) :: _) -> 
                let (RBInfo(_,_,_,_,_,_,_,_,_,_,_,declKind)) = rbinfo
                DeclKind.MustHaveArity declKind
            
        EliminateInitializationGraphs cenv.g mustHaveArity env.DisplayEnv bindsWithoutLaziness bindsm
    
    // Post letrec env 
    let envbody = AddLocalValMap scopem prelimRecValues env 
    binds,envbody,tpenv

//-------------------------------------------------------------------------
// Bind specifications of values
//------------------------------------------------------------------------- 

let TcAndPublishValSpec (cenv,env,containerInfo,declKind,memFlagsOpt,tpenv,valSpfn) = 
  let ((ValSpfn(attrs,id,SynValTyparDecls(_,canInferTypars,_), _, _, pseudo, mutableFlag,doc, vis,literalExprOpt,m))) = valSpfn 
  let attrs' = TcAttributes cenv env attrTgtBinding attrs
  let valinfos,tpenv = TcValSpec cenv env declKind containerInfo memFlagsOpt None tpenv valSpfn attrs'
  let denv = env.DisplayEnv
  
  List.mapfold 
      (fun tpenv (ValSpecResult(altActualParent,memberInfoOpt,id:ident,enclosingDeclaredTypars,declaredTypars,ty,partialTopValInfo,declKind)) -> 
          let inlineFlag = ComputeInlineFlag (memberInfoOpt |> Option.map (fun memberInfo -> memberInfo.MemberFlags)) pseudo mutableFlag attrs'
          
          let freeInType = free_in_type_lr cenv.g false ty

          let allDeclaredTypars = enclosingDeclaredTypars @ declaredTypars

          let flex = ExplicitTyparInfo(declaredTypars,canInferTypars)
          
          let canInferTypars = GeneralizationHelpers.ComputeCanInferTypars(declKind,containerInfo.ParentRef,canInferTypars,memFlagsOpt,declaredTypars,m)
          
          let generalizedTypars = GeneralizationHelpers.ComputeGeneralizedTypars(cenv,env,id.idRange,canInferTypars,empty_free_loctypars,canInferTypars,CanGeneralizeConstrainedTypars,inlineFlag,None,allDeclaredTypars,freeInType,ty,false)
          
          let valscheme1 = PrelimValScheme1(id,flex,ty,Some(partialTopValInfo),memberInfoOpt,mutableFlag,inlineFlag,NormalVal,noArgOrRetAttribs,vis,false)

          let valscheme2 = GeneralizeVal cenv denv enclosingDeclaredTypars  [] generalizedTypars valscheme1

          let tpenv = HideUnscopedTypars generalizedTypars tpenv

          let valscheme = BuildValScheme declKind (Some(partialTopValInfo)) valscheme2 

          let konst = 
              match literalExprOpt with 
              | None -> 
                  let hasLiteralAttr = HasAttrib cenv.g cenv.g.attrib_LiteralAttribute attrs'
                  if hasLiteralAttr then 
                      errorR(Error("A declaration may only be the [<Literal>] attribute if a constant value is also given, e.g. 'val x : int = 1'",m));
                  None

              
              | Some(e) -> 
                  let hasLiteralAttr,konst = TcLiteral cenv ty env tpenv (attrs',e)
                  if not hasLiteralAttr then 
                      errorR(Error("A declaration may only be given a value in a signature if the declaration has the [<Literal>] attribute",range_of_synexpr e));
                  konst

          let vspec = MakeAndPublishVal cenv env (altActualParent,true,declKind,ValNotInRecScope,valscheme,attrs',doc.ToXmlDoc(),konst)
          assert(vspec.InlineInfo = inlineFlag);

          vspec,tpenv)
      tpenv
      valinfos


//-------------------------------------------------------------------------
// Bind elements of data definitions for exceptions and types (fields, etc.)
//------------------------------------------------------------------------- 

let CombineReprAccess parent vis = 
    match parent with 
    | ParentNone -> vis 
    | Parent tcref -> combineAccess vis tcref.TypeReprAccessibility

let MakeRecdFieldSpec cenv env parent (stat,konst,ty',attrsForProperty,attrsForField,id,mut,xmldoc,vis,m) =
    let vis,_ = ComputeAccessAndCompPath env None m vis parent
    let vis = CombineReprAccess parent vis
    NewRecdField stat konst id ty' mut attrsForProperty attrsForField xmldoc vis false

let TcFieldDecl cenv env parent  isIncrClass tpenv (stat,attrs,id,ty,mut,xmldoc,vis,m) =
    let attrs' = TcAttributesWithPossibleTargets cenv env attrTgtFieldDecl attrs
    let attrsForProperty,attrsForField = attrs' |> List.partition (fun (attrTargets,_) -> (attrTargets &&& attrTgtProperty) <> 0) 
    let attrsForProperty = (List.map snd attrsForProperty) 
    let attrsForField = (List.map snd attrsForField)
    let ty',_ = TcTypeAndRecover cenv NoNewTypars CheckCxs env tpenv ty
    let zeroInit = HasAttrib cenv.g cenv.g.attrib_DefaultValueAttribute attrsForField
    
    let isThreadStatic = isThreadOrContextStatic cenv.g attrsForField
    if isThreadStatic && (not zeroInit || not stat) then 
        error(Error("Thread-static and context-static variables must be static and given the [<DefaultValue>] attribute to indicate that the value is initialized to the default value on each new thread",m));

    if isIncrClass && (not zeroInit || not mut) then errorR(Error("Unintialized 'val' fields in implicit construction types must be mutable and marked with the '[<DefaultValue>]' attribute. Consider using a 'let' binding instead of a 'val' field",m));
    if stat && (not zeroInit || not mut) then errorR(Error("Static 'val' fields in types must be mutable and marked with the '[<DefaultValue>]' attribute. They are initialized to the 'null' or 'zero' value for their type. Consider also using a 'let' binding in a module",m));
    let konst = if zeroInit then Some(TConst_zero) else None
    MakeRecdFieldSpec cenv env parent  (stat,konst,ty',attrsForProperty,attrsForField,id,mut,xmldoc,vis,m)

let TcAnonFieldDecl cenv env parent tpenv nm (Field(attribs,stat,id,ty,mut,xmldoc,vis,m)) =
    let id = (match id with None -> mksyn_id m nm | Some id -> id)
    TcFieldDecl cenv env parent false tpenv (stat,attribs,id,ty,mut,xmldoc.ToXmlDoc(),vis,m) 

let TcNamedFieldDecl cenv env parent isIncrClass tpenv (Field(attribs,stat,id,ty,mut,xmldoc,vis,m)) =
    match id with 
    | None -> error (Error("this field requires a name",m))
    | Some(id) -> TcFieldDecl cenv env parent isIncrClass  tpenv (stat,attribs,id,ty,mut,xmldoc.ToXmlDoc(),vis,m) 

let CheckDuplicates (idf : _ -> ident) k elems = 
    elems |> List.iteri (fun i uc1 -> 
        elems |> List.iteri (fun j uc2 -> 
            let id1 = (idf uc1)
            let id2 = (idf uc2)
            if j > i &&  id1.idText = id2.idText then 
                errorR (Duplicate(k,id1.idText,id1.idRange))));
    elems

let TcNamedFieldDecls cenv env parent isIncrClass tpenv fields =
    fields |> List.map (TcNamedFieldDecl cenv env parent isIncrClass tpenv) |> CheckDuplicates (fun f -> f.Id) "field" 


//-------------------------------------------------------------------------
// Bind other elements of type definitions (constructors etc.)
//------------------------------------------------------------------------- 

exception NotUpperCaseConstructor of range

let CheckNamespaceModuleOrTypeName (id:ident) = 
    if id.idText.Contains "." then errorR(Error("Invalid namespace, module, type or union case name",id.idRange))


let CheckUnionCaseName realUnionCaseName m =
    CheckNamespaceModuleOrTypeName (mksyn_id m realUnionCaseName);
    if not (String.isUpper realUnionCaseName) && realUnionCaseName <> opname_Cons && realUnionCaseName <> opname_Nil then 
        errorR(NotUpperCaseConstructor(m));

let TcUnionCaseDecl cenv env parent thisTy tpenv (UnionCase (attrs,id,args,xmldoc,vis,m)) =
    let attrs' = TcAttributes cenv env attrTgtUnionCaseDecl attrs // the attributes of a union case decl get attached to the generated "static factory" method
    let vis,cpath = ComputeAccessAndCompPath env None m vis parent
    let vis = CombineReprAccess parent vis
    let realUnionCaseName =  
        if id.idText = opname_Cons then "Cons" 
        elif id.idText = opname_Nil then "Empty"
        else id.idText
    
    CheckUnionCaseName realUnionCaseName id.idRange;
    let mkName i = (realUnionCaseName^string (i+1))
    let rfields,recordTy = 
        match args with
        | UnionCaseFields flds -> 
            let rfields = flds |> List.mapi (fun i fld -> TcAnonFieldDecl cenv env parent tpenv (mkName i) fld) 
            rfields,thisTy
        | UnionCaseFullType (ty,arity) -> 
            let ty',tpenv = TcTypeAndRecover cenv NoNewTypars CheckCxs env tpenv ty
            let argtysl,recordTy = GetTopTauTypeInFSharpForm cenv.g (arity |> TranslateTopValSynInfo m (TcAttributes cenv env) |> TranslatePartialArity []).ArgInfos ty' m
            if argtysl.Length > 1 then 
                errorR(Error("Explicit type declarations for constructors must be of the form 'ty1 * ... * tyN -> resTy'. Parentheses may be required around 'resTy'",m));   
            let rfields = 
                argtysl |> List.concat |> List.mapi (fun i (argty,TopArgInfo(_,nmOpt)) ->
                    let id = (match nmOpt with Some id -> id | None -> mksyn_id m (mkName i))
                    MakeRecdFieldSpec cenv env parent (false,None,argty,[],[],id,false,emptyXmlDoc,None,m))
            if not (type_equiv cenv.g recordTy thisTy) then 
                error(Error("Return types of union cases must be identical to the type being defined, up to abbreviations",m))
            rfields,recordTy
    NewUnionCase id realUnionCaseName rfields recordTy attrs' (xmldoc.ToXmlDoc()) vis

let TcUnionCaseDecls cenv env parent thisTy tpenv unionCases =
    let unionCases' = unionCases |> List.map (TcUnionCaseDecl cenv env parent thisTy tpenv) 
    unionCases' |> CheckDuplicates (fun uc -> uc.Id) "union case" 

let TcEnumDecl cenv env parent thisTy fieldTy (EnumCase (attrs,id,v,xmldoc,m)) =
    let attrs' = TcAttributes cenv env attrTgtField attrs
    match v with 
    | Const_bytearray _
    | Const_uint16array _
    | Const_bignum _ -> error(Error("This is not a valid value for an enumeration literal",m))
    | _ -> 
        let v = TcConst cenv fieldTy m env v
        let vis,cpath = ComputeAccessAndCompPath env None m None parent
        let vis = CombineReprAccess parent vis
        if id.idText = "value__" then errorR(Error("This is not a valid name for an enumeration case",id.idRange));
        NewRecdField true (Some v) id thisTy false [] attrs' emptyXmlDoc vis false
  
let TcEnumDecls cenv env parent thisTy enumCases =
    let fieldTy = new_inference_typ cenv ()
    let enumCases' = enumCases |> List.map (TcEnumDecl cenv env parent thisTy fieldTy)  |> CheckDuplicates (fun f -> f.Id) "enum element"
    fieldTy,enumCases'


//-------------------------------------------------------------------------
// Bind elements of classes
//------------------------------------------------------------------------- 

let PublishInterface cenv denv (tcref:TyconRef) m compgen ty' = 
    if not (is_interface_typ cenv.g ty') then errorR(Error(sprintf "The type '%s' is not an interface type" (NicePrint.pretty_string_of_typ denv ty'),m));
    let tcaug = tcref.TypeContents
    if tcaug_has_interface cenv.g tcaug ty'  then 
        errorR(Error("duplicate specification of an interface",m));
    tcaug.tcaug_implements <- (ty',compgen,m) :: tcaug.tcaug_implements

let TcAndPublishMemberSpec newslotsOK cenv env containerInfo declKind tcref tpenv memb = 
    match memb with 
    | ClassMemberSpfn_field(fdecl,m) -> error(Error("A field/val declaration is not permitted here",m))
    | ClassMemberSpfn_inherit(typ,m) -> error(Error("A inheritance declaration is not permitted here",m))
    | ClassMemberSpfn_tycon(_,m) -> error(Error("Types may not contain nested type definitions",m))
    | ClassMemberSpfn_binding(valSpfn,memberFlags,m) -> 
        TcAndPublishValSpec (cenv,env,containerInfo,declKind,Some(memberFlags),tpenv,valSpfn)
    | ClassMemberSpfn_interface(ty,m) -> 
        // These are done in TcTyconDefnCores
        [],tpenv

  
let TcTyconMemberSpecs newslotsOK cenv env containerInfo declKind tcref tpenv (augSpfn: SynClassSpfn)  =
    let members,tpenv = List.mapfold (TcAndPublishMemberSpec newslotsOK cenv env containerInfo declKind tcref) tpenv augSpfn
    List.concat members,tpenv


//-------------------------------------------------------------------------
// Bind 'open' declarations
//------------------------------------------------------------------------- 

let TcModuleOrNamespaceLidAndPermitAutoResolve env lid =
    let ad = AccessRightsOfEnv env
    match ResolveLongIndentAsModuleOrNamespace OpenQualified env.eNameResEnv ad lid  with 
    | Result res -> Result res
    | Exception err ->  raze err

let TcOpenDecl g amap m scopem env (lid : ident list)  = 
    let modrefs = ForceRaise (TcModuleOrNamespaceLidAndPermitAutoResolve env lid)

    let IsPartiallyQualifiedNamespace (modref: ModuleOrNamespaceRef) = 
        let (CompPath(_,p)) = modref.CompilationPath 
        // Bug FSharp 1.0 3274: FSI paths don't count when determining this warning
        let p = 
            match p with 
            | [] -> []
            | (h,_):: t -> if h.StartsWith(DynamicModulePrefix,System.StringComparison.Ordinal) then t else p
        modref.IsNamespace && p.Length >= lid.Length 

    modrefs |> List.iter (fun (_,modref,_) ->
       if modref.IsModule && HasAttrib g g.attrib_RequireQualifiedAccessAttribute modref.Attribs then 
           warning(Error(sprintf "This declaration opens the module '%s', which is marked as 'RequireQualifiedAccess'. In a future version of F#, it will be an error to open this module. Adjust your code to use qualified references to the elements of the module instead, e.g. 'List.map' instead of 'map'. This change will ensure that your code is robust as new constructs are added to libraries" (full_display_text_of_modref modref),m)))

    // Bug FSharp 1.0 3133: 'open Lexing'. Skip this warning if we successfully resolved to at least a module name
    if not (modrefs |> List.exists (fun (_,modref,_) -> modref.IsModule && not (HasAttrib g g.attrib_RequireQualifiedAccessAttribute modref.Attribs))) then
        modrefs |> List.iter (fun (_,modref,_) ->
            if IsPartiallyQualifiedNamespace modref  then 
                 warning(Error(sprintf "This declaration opens the namespace or module '%s' through a partially qualified path. Please adjust this code to use the full path of the namespace. A future version of F# will require this. This change will make your code more robust as new constructs are added to the F# and .NET libraries" (full_display_text_of_modref modref),m)))
        
    modrefs |> List.iter (fun (_,modref,_) -> CheckEntityAttributes g modref m |> CommitOperationResult);        

    let env = open_moduls g amap scopem env modrefs 
    env    


exception ParameterlessStructCtor of range

/// Incremental class definitions
module IncrClassChecking = 

    type IncrClassBindingGroup = 
      | IncrClassBindingGroup of Tast.Binding list * (*isStatic:*) bool* (*recursive:*) bool
      | IncrClassDo of Tast.expr * (*isStatic:*) bool

    /// Typechecked info for implicit constructor and it's arguments 
    type IncrClassCtorLhs = 
        {incrClassTcref                            : TyconRef;
         incrClassInstanceCtorDeclaredTypars       : typars;     
         incrClassRevTypeInst                      : TyparInst;
         // Lazy to ensure the static ctor value is ony published if needed
         incrClassStaticCtorValInfo                : Lazy<(Val list * Val * ValScheme)>;
         incrClassInstanceCtorVal                  : Val;
         incrClassInstanceCtorValScheme            : ValScheme;
         incrClassInstanceCtorArgs                 : Val list;
         incrClassInstanceCtorThisVarRefCellOpt    : Val option;
         incrClassInstanceCtorBaseValOpt           : Val option;
         incrClassInstanceCtorThisVar              : Val;
         incrClassNameGenerator : NiceNameGenerator;
        }

    let TcSimplePatsOfUnknownType cenv optArgsOK env tpenv spats =
        let argty = new_inference_typ cenv ()
        TcSimplePats cenv optArgsOK argty env (tpenv,NameMap.empty,Set.empty) spats

    /// Check and elaborate the "left hand side" of the implicit class construction 
    /// syntax.
    let TcImplictCtorLhsPassA(cenv,env,tpenv,tcref:TyconRef,vis,attrs,spats,thisIdOpt,baseValOpt,m) =

        // Make fresh version of the class type for type checking the members and lets *
        let isExtrinsic = false
        let tcrefObjTy,ctorDeclaredTypars,renaming,objTy,thisTy = FreshenObjectArgType cenv m TyparRigid tcref isExtrinsic (tcref.Typars(m))

        // Note: tcrefObjTy contains the original "formal" typars, thisTy is the "fresh" one... f<>fresh. 
        let incrClassRevTypeInst = List.zip ctorDeclaredTypars (tcref.Typars(m) |> List.map mk_typar_ty)

        let baseValOpt = 
            match SuperTypeOfType cenv.g cenv.amap m objTy with 
            | Some(superTy) -> MakeAndPublishBaseVal cenv env (Option.map id_of_val baseValOpt) superTy
            | None -> None

        // Use class type directly for type checking the members and lets 
        // The (instance) members get fresh tyvar if their "this" requires it.
        // Those typar are not "rigid" so they can be equated to each other through their mutual recursion.
        // The types typar are made rigid in the "cores" pass.
         
        // These typars should not be instantiated.
        // Post TC they should still be unique typars.
        // Setting rigid ensures they do not get substituted for another typar either.
        ctorDeclaredTypars |> List.iter (SetTyparRigid cenv.g env.DisplayEnv m) ; 

        if verboseCC then dprintf "ctorDeclaredTypars: %s\n" (showL (TyparsL ctorDeclaredTypars));    

        // Add class typars to env 
        let env = AddDeclaredTypars CheckForDuplicateTypars ctorDeclaredTypars env

        // Type check arguments by processing them as 'simple' patterns 
        //     NOTE: if we allow richer patterns here this is where we'd process those patterns 
        let ctorArgNames,(tpenv,names,takenNames) = TcSimplePatsOfUnknownType cenv true env tpenv (SPats (spats,m))
        
        // Create the values with the given names 
        let _,vspecs = MakeSimpleVals cenv env m names

        if is_struct_tcref tcref && isNil spats then 
            errorR (ParameterlessStructCtor(tcref.Range));
        
        // Put them in order 
        let ctorArgs = List.map (fun v -> NameMap.find v vspecs) ctorArgNames
        let ctorThisVarRefCellOpt = MakeAndPublishCtorThisRefCellVal cenv env thisIdOpt thisTy
        
        // NOTE: the type scheme here is not complete!!! The ctorTy is more or less 
        // just a type variable. The type and typars get fixed-up after inference 
        let ctorValScheme,ctorVal = 
            let argty = mk_tupled_ty cenv.g (types_of_vals ctorArgs)
            // Initial type has known information 
            let ctorTy = mk_fun_ty argty objTy    
            // NOTE: no OverloadID can be specified for the implicit constructor 
            let OverloadQualifier = None  
            // REVIEW: no attributes can currently be specified for the implicit constructor 
            let attribs = TcAttributes cenv env (attrTgtConstructor ||| attrTgtMethod) attrs
            let memberFlags      = CtorMemberFlags OverloadQualifier
                                  
            let synArgInfos   = List.map (SynInfo.InferArgSynInfoFromSimplePat []) spats
            let valSynData = ValSynInfo([synArgInfos],SynInfo.unnamedRetVal)
            let id            = ident ("new",m)

            CheckForNonAbstractInterface ModuleOrMemberBinding tcref memberFlags id.idRange;
            let memberInfo,uniqueName  = MkMemberDataAndUniqueId(cenv.g,tcref,false,attribs,[],memberFlags,valSynData,id)
            let partialTopValInfo = TranslateTopValSynInfo m (TcAttributes cenv env) valSynData
            let prelimTyschemeG = TypeScheme(ctorDeclaredTypars,[],ctorTy)
            let isComplete = ComputeIsComplete ctorDeclaredTypars [] ctorTy
            let topValInfo = InferGenericArityFromTyScheme prelimTyschemeG partialTopValInfo
            let ctorValScheme = ValScheme(uniqueName,prelimTyschemeG,Some(topValInfo),Some(memberInfo),false,NeverInline,NormalVal,vis,false,true,false)
            let ctorVal = MakeAndPublishVal cenv env (Parent(tcref),false,ModuleOrMemberBinding,ValInRecScope(isComplete),ctorValScheme,attribs,emptyXmlDoc,None) 
            ctorValScheme,ctorVal

        // We only generate the cctor on demand, because wew don't need it if there are no cctor actions. 
        // The code below has a side-effect (MakeAndPublishVal), so we only want to run it once if at all. 
        // The .cctor is never referenced by any other code.
        let cctorValInfo = 
            lazy 
               (let cctorArgs = [ fst(mk_compgen_local m "unitVar" cenv.g.unit_ty) ]

                let cctorTy = mk_fun_ty cenv.g.unit_ty cenv.g.unit_ty
                let valSynData = ValSynInfo([[]],SynInfo.unnamedRetVal)
                let id = ident ("cctor",m)
                CheckForNonAbstractInterface ModuleOrMemberBinding tcref ClassCtorMemberFlags id.idRange;
                let memberInfo,uniqueName  = MkMemberDataAndUniqueId(cenv.g,tcref,false,[(*no attributes*)],[],ClassCtorMemberFlags,valSynData,id)
                let partialTopValInfo = TranslateTopValSynInfo m (TcAttributes cenv env) valSynData
                let prelimTyschemeG = TypeScheme(ctorDeclaredTypars,[],cctorTy)
                let topValInfo = InferGenericArityFromTyScheme prelimTyschemeG partialTopValInfo
                let cctorValScheme = ValScheme(uniqueName,prelimTyschemeG,Some(topValInfo),Some(memberInfo),false,NeverInline,NormalVal,None,false,true,false)
                 
                let cctorVal = MakeAndPublishVal cenv env (Parent(tcref),false,ModuleOrMemberBinding,ValNotInRecScope,cctorValScheme,[(* no attributes*)],emptyXmlDoc,None) 
                cctorArgs,cctorVal,cctorValScheme)

        let thisVal = 
            // --- Create this for use inside constructor 
            let thisId  = ident ("this",m)
            let thisValScheme  = ValScheme(thisId,NonGenericTypeScheme(thisTy),None,None,false,NeverInline,NormalVal,None,false,false,false)
            let thisVal    = MakeAndPublishVal cenv env (ParentNone,false,ClassLetBinding,ValNotInRecScope,thisValScheme,[],emptyXmlDoc,None)
            if verboseCC then dprintf "mk_thisVar: v = %s\n" (showL (TypeOfvalL thisVal));
            thisVal

        {incrClassTcref                         = tcref;
         incrClassInstanceCtorDeclaredTypars    = ctorDeclaredTypars;
         incrClassRevTypeInst                   = incrClassRevTypeInst;
         incrClassStaticCtorValInfo             = cctorValInfo;
         incrClassInstanceCtorArgs              = ctorArgs;
         incrClassInstanceCtorVal               = ctorVal;
         incrClassInstanceCtorValScheme         = ctorValScheme;
         incrClassInstanceCtorBaseValOpt        = baseValOpt;
         incrClassInstanceCtorThisVarRefCellOpt = ctorThisVarRefCellOpt;
         incrClassInstanceCtorThisVar           = thisVal;
         // For generating names of local fields
         incrClassNameGenerator                 = NiceNameGenerator()

        }


    // Partial class defns - local val mapping to fields
      
    /// Create the field for a "let" binding in a type definition.
    ///
    /// The "v" is the local typed w.r.t. tyvars of the implicit ctor.
    /// The formalTyparInst does the formal-typars/implicit-ctor-typars subst.
    /// Field specifications added to a tcref must be in terms of the tcrefs formal typars.
    let MakeIncrClassField(cpath,formalTyparInst:TyparInst,v:Val,isStatic,rfref:RecdFieldRef) =
        let name = rfref.FieldName
        let id  = ident (name,v.Range)
        let ty  = v.Type |> InstType formalTyparInst
        let mut = v.IsMutable

        let taccess = TAccess [cpath]
        NewRecdField isStatic None id ty mut [(*no property attributes*)] [(*no field attributes *)] emptyXmlDoc taccess (*secret:*)true

    type IncrClassValRepr = 
        // e.g representation for 'let v = 3' if it is not used in anything given a method representation
        | InVar of (* isArg: *) bool 
        // e.g representation for 'let v = 3'
        | InField of (*isStatic:*)bool * RecdFieldRef
        // e.g representation for 'let f x = 3'
        | InMethod of (*isStatic:*)bool * Val * ValTopReprInfo

    /// IncrClassReprInfo represents the decisions we make about the representation of 'let' and 'do' bindings in a
    /// type defined with implicit class construction.
    type IncrClassReprInfo = 
        { TakenFieldNames:Set<string>;
          RepInfoTcGlobals:TcGlobals;
          /// vals mapped to representations
          ValReprs  : Zmap.t<Val,IncrClassValRepr>; 
          /// vals represented as fields or members from this point on 
          ValsWithRepresentation  : Val Zset.t; }

        static member Empty(g) = 
            { TakenFieldNames=Set.empty;
              RepInfoTcGlobals=g;
              ValReprs = Zmap.empty val_spec_order; 
              ValsWithRepresentation = Zset.empty val_spec_order }

        /// Find the representation of a value
        member localRep.LookupRepr (v:Val) = 
            let g = localRep.RepInfoTcGlobals 
            match Zmap.tryfind v localRep.ValReprs with 
            | None -> error(InternalError("LookupRepr: failed to find representation for value",v.Range))
            | Some res -> res

        static member IsMethodRepr cenv (bind:Binding) = 
            let v = bind.Var
            // unit fields are not stored, just run rhs for effects
            if is_unit_typ cenv.g v.Type then 
                false
            else 
                let arity = InferArityOfExprBinding cenv.g v bind.Expr 
                not arity.HasNoArgs && not v.IsMutable


        /// Choose how a binding is represented
        static member ChooseRepresentation (cenv,env:tcEnv,isStatic,isCtorArg,
                                            ctorInfo:IncrClassCtorLhs,
                                            /// The vars forced to be fields due to static member bindings, instance initialization expressions or instance member bindings
                                            staticForcedFieldVars:FreeLocals,
                                            /// The vars forced to be fields due to instance member bindings
                                            instanceForcedFieldVars:FreeLocals,
                                            takenFieldNames: Set<string>,
                                            bind:Binding) = 
            let g = cenv.g 
            let v = bind.Var
            let relevantForcedFieldVars = (if isStatic then staticForcedFieldVars else instanceForcedFieldVars)
            
            // REVIEW: we would very much like to avoid generating a fresh compiler generated name here
            let tcref = ctorInfo.incrClassTcref
            let name,takenFieldNames = 
                let nm = 
                    if isSome (tcref.GetFieldByName(v.MangledName)) || takenFieldNames.Contains(v.MangledName) then
                        ctorInfo.incrClassNameGenerator.FreshCompilerGeneratedName (v.MangledName,v.Range)
                    else 
                        v.MangledName
                nm, takenFieldNames.Add(nm)
                 
            let repr = 
                match InferArityOfExprBinding g v bind.Expr with 
                | arity when arity.HasNoArgs || v.IsMutable -> 
                    // all mutable variables are forced into fields, since they may escape into closures within the implicit constructor
                    // e.g. 
                    //     type C() =  
                    //        let mutable m = 1
                    //        let n = ... (fun () -> m) ....
                    
                    if v.IsMutable || relevantForcedFieldVars.Contains v then 
                        //dprintfn "Representing %s as a field %s" v.MangledName name
                        let rfref = RFRef(tcref,name)
                        InField (isStatic,rfref)
                    else
                        //dprintfn 
                        //    "Representing %s as a local variable %s, staticForcedFieldVars = %s, instanceForcedFieldVars = %s" 
                        //    v.MangledName name 
                        //    (staticForcedFieldVars |> Seq.map (fun v -> v.MangledName) |> String.concat ",")
                        //    (instanceForcedFieldVars |> Seq.map (fun v -> v.MangledName) |> String.concat ",")
                        InVar isCtorArg
                | topValInfo -> 
                    //dprintfn "Representing %s as a method %s" v.MangledName name
                    let tps, argInfos, _, _ = GetTopValTypeInCompiledForm g topValInfo v.Type v.Range

                    let valSynInfo = ValSynInfo(argInfos |> List.mapSquared (fun (_,TopArgInfo(_,nm)) -> ArgSynInfo([],false,nm)),SynInfo.unnamedRetVal)
                    let memberFlags = (if isStatic then StaticMemberFlags else NonVirtualMemberFlags) None MemberKindMember
                    let memberInfo,id = MkMemberDataAndUniqueId(g,tcref,false,[],[],memberFlags,valSynInfo,mksyn_id v.Range name)
                    let curriedArgTys = argInfos |> List.mapSquared fst |> List.map (mk_tupled_ty g)

                    let ctorDeclaredTypars = ctorInfo.incrClassInstanceCtorDeclaredTypars
                    // Add the 'this' pointer on to the function
                    let memberTauTy,topValInfo = 
                        let tauTy = v.TauType
                        if isStatic then 
                            tauTy,topValInfo 
                        else 
                            let tauTy = ctorInfo.incrClassInstanceCtorThisVar.Type --> v.TauType
                            let (TopValInfo(tpNames,args,ret)) = topValInfo
                            let topValInfo = TopValInfo(tpNames, TopValInfo.selfMetadata::args, ret)
                            tauTy, topValInfo
                    // Add the enclosing type parameters on to the function
                    let topValInfo = 
                        let (TopValInfo(tpNames,args,ret)) = topValInfo
                        TopValInfo(tpNames@TopValInfo.InferTyparInfo(ctorDeclaredTypars), args, ret)
                                          
                    //let synArgInfos   = List.map (SynInfo.InferArgSynInfoFromSimplePat []) spats
                    //let valSynData = ValSynInfo([synArgInfos],SynInfo.unnamedRetVal)

                    let prelimTyschemeG = TypeScheme(ctorDeclaredTypars@tps,[],memberTauTy)
                    let memberValScheme = ValScheme(id,prelimTyschemeG,Some(topValInfo),Some(memberInfo),false,NeverInline,NormalVal,None,true (* isCompilerGenerated *) ,true (* isIncrClass *) ,false)
                    let methodVal = MakeAndPublishVal cenv env (Parent(tcref),false,ModuleOrMemberBinding,ValNotInRecScope,memberValScheme,[(* no attributes*)],emptyXmlDoc,None) 
                    InMethod(isStatic,methodVal,topValInfo)
            repr, takenFieldNames

        /// Extend the known local representations by choosing a representation for a binding
        member localRep.ChooseAndAddRepresentation(cenv,env:tcEnv,isStatic,isCtorArg,ctorInfo:IncrClassCtorLhs,staticForcedFieldVars:FreeLocals,instanceForcedFieldVars: FreeLocals,bind:Binding) = 
            let g = localRep.RepInfoTcGlobals 
            let v = bind.Var
            let tcref = ctorInfo.incrClassTcref
            let repr,takenFieldNames = IncrClassReprInfo.ChooseRepresentation (cenv,env,isStatic,isCtorArg,ctorInfo,staticForcedFieldVars,instanceForcedFieldVars,localRep.TakenFieldNames,bind )
            // OK, representation chosen, now add it 
            {localRep with 
                TakenFieldNames=takenFieldNames; 
                ValReprs = Zmap.add v repr localRep.ValReprs}  

        member localRep.ValNowWithRepresentation (v:Val) = 
            {localRep with ValsWithRepresentation = Zset.add v localRep.ValsWithRepresentation}

        member localRep.IsValWithRepresentation (v:Val) = 
                localRep.ValsWithRepresentation.Contains(v) 

        /// Make the elaborated expression that represents a use of a 
        /// a "let v = ..." class binding
        member localRep.MakeValueLookup thisVarOpt tinst v tyargs m =
            let g = localRep.RepInfoTcGlobals 
            match localRep.LookupRepr v, thisVarOpt with 
            | InVar _,_ -> 
                expr_for_val m v
            | InField(false,rfref),Some(thisVal) -> 
                let thise = expr_for_val m thisVal
                mk_recd_field_get_via_expra(thise,rfref,tinst,m)
            | InField(false,rfref),None -> 
                error(InternalError("Unexpected missing 'this' variable in MakeValueLookup",m))
            | InField(true,rfref),_ -> 
                mk_static_rfield_get(rfref,tinst,m)
            | InMethod(isStatic,methodVal,topValInfo),_ -> 
                //dprintfn "Rewriting application of %s to be call to method %s" v.MangledName methodVal.MangledName
                let expr,exprty = AdjustValForExpectedArity g m (mk_local_vref methodVal) NormalValUse topValInfo 
                // Prepend the the type arguments for the class
                let tyargs = tinst @ tyargs 
                let thisArgs =
                    if isStatic then []
                    else Option.to_list (Option.map (expr_for_val m) thisVarOpt)
                MakeApplicationAndBetaReduce g (expr,exprty,[tyargs],thisArgs,m) 

        /// Make the elaborated expression that represents an assignment 
        /// to a "let mutable v = ..." class binding
        member localRep.MakeValueAssign thisVarOpt tinst v expr m =
            let g = localRep.RepInfoTcGlobals 
            match localRep.LookupRepr v, thisVarOpt with 
            | InField(false,rfref),Some(thisVal) -> 
                let thise = expr_for_val m thisVal
                mk_recd_field_set_via_expra(thise,rfref,tinst,expr,m)
            | InField(false,rfref),None -> 
                error(InternalError("Unexpected missing 'this' variable in MakeValueAssign",m))
            | InVar _,_ -> 
                mk_val_set m (mk_local_vref v) expr
            | InField(true,rfref),_ -> 
                mk_static_rfield_set(rfref,tinst,expr,m)
            | InMethod _,_ -> 
                error(InternalError("Local was given method storage, yet later it's been assigned to",m))
          
        member localRep.MakeValueGetAddress thisVarOpt tinst v m =
            let g = localRep.RepInfoTcGlobals 
            match localRep.LookupRepr v,thisVarOpt with 
            | InField(false,rfref),Some(thisVal) -> 
                let thise = expr_for_val m thisVal
                mk_recd_field_get_addr_via_expra(thise,rfref,tinst,m)
            | InField(false,rfref),None -> 
                error(InternalError("Unexpected missing 'this' variable in MakeValueGetAddress",m))
            | InField(true,rfref),_ -> 
                mk_static_rfield_get_addr(rfref,tinst,m)
            | InVar _,_ -> 
                mk_val_addr m (mk_local_vref v)
            | InMethod _,_ -> 
                error(InternalError("Local was given method storage, yet later it's address was required",m))

        /// Mutate a type definition by adding fields 
        /// Used as part of processing "let" bindings in a type definition. 
        member localRep.PublishIncrClassFields cpath (ctorInfo:IncrClassCtorLhs) =   

            let tcref = ctorInfo.incrClassTcref
            let rfspecs   = 
                [ for KeyValue(v,repr) in localRep.ValReprs do
                      match repr with 
                      | InField(isStatic,rfref) -> yield MakeIncrClassField(cpath,ctorInfo.incrClassRevTypeInst,v,isStatic,rfref)
                      | _ -> yield! [] ]

            let recdFields = MakeRecdFieldsTable (rfspecs @ tcref.AllFieldsAsList)
            let obspec = tcref.FSharpObjectModelTypeInfo
            let obspec = {obspec with fsobjmodel_rfields = recdFields}
            // Mutate the entity_tycon_repr to publish the field
            tcref.Deref.Data.entity_tycon_repr <- Some (TFsObjModelRepr obspec)  


        /// Given localRep saying how locals have been represented, e.g. as fields.
        /// Given an expr under a given thisVal context.
        //
        /// Fix up the references to the locals, e.g. 
        ///     v -> this.fieldv
        ///     f x -> this.method x
        member localRep.FixupIncrClassExprPassC thisVarOpt (thisTyInst:tinst) expr = 
            // fixup: intercept and expr rewrite
            let FixupExprNode e =
                //dprintfn "Fixup %s" (showL (ExprL e))
                match e with
                // Rewrite references to generic methods
                | TExpr_app(TExpr_val (ValDeref(v),flags,_),_,tyargs,args,m) 
                    when (localRep.IsValWithRepresentation(v) &&
                          (match localRep.LookupRepr(v) with 
                           | InMethod (_,methodVal,_)  -> (methodVal.Typars.Length > thisTyInst.Length)
                           | _ -> false )) -> 

                        //dprintfn "Found application of %s" v.MangledName
                        let g = localRep.RepInfoTcGlobals
                        let expr = localRep.MakeValueLookup thisVarOpt thisTyInst v tyargs m
                        Some (MakeApplicationAndBetaReduce g (expr,(type_of_expr g expr),[],args,m)) 
                        

                // Rewrite references to values stored as fields and non-generic methods
                | TExpr_val (ValDeref(v),flags,m)                         
                    when (localRep.IsValWithRepresentation(v) &&
                          (match localRep.LookupRepr(v) with 
                           | InMethod (_,methodVal,_) -> (methodVal.Typars.Length <= thisTyInst.Length)
                           | _ -> true)) -> 

                        //dprintfn "Found use of %s" v.MangledName
                        Some (localRep.MakeValueLookup thisVarOpt thisTyInst v [] m)

                // Rewrite assignments to mutable values stored as fields 
                | TExpr_op(TOp_lval_op (LSet,ValDeref(v))    ,[],[arg],m) 
                    when localRep.IsValWithRepresentation(v) ->
                        Some (localRep.MakeValueAssign thisVarOpt thisTyInst v arg m)

                // Rewrite taking the address of mutable values stored as fields 
                | TExpr_op(TOp_lval_op (LGetAddr,ValDeref(v)),[],[]   ,m) 
                    when localRep.IsValWithRepresentation(v) ->
                        Some (localRep.MakeValueGetAddress thisVarOpt thisTyInst v m)

                | other -> None
            Tastops.RewriteExpr { pre_intercept= None; 
                                   post_transform = FixupExprNode;
                                   under_quotations=true } expr 


    type IncrClassConstructionBindingsPassC =
      | PassCBindings of IncrClassBindingGroup list
      | PassCCtorComplete     

    /// Given a set of 'let' bindings (static or not, recursive or not) that make up a class, 
    /// generate their initialization expression(s).  
    let MakeCtorForIncrClassConstructionPassC 
               (cenv,
                env:tcEnv,
                tpenv ,
                /// The lhs information about the implicit constructor
                ctorInfo:IncrClassCtorLhs,
                /// The call to the super class constructor
                inheritsExpr,
                /// Should we place a sequence point at the 'inheritedTys call?
                inheritsIsVisible,
                /// The declarations
                decs : IncrClassConstructionBindingsPassC list,
                memberBinds : Binding list,
                /// Record any unconstrained type parameters generalized for the outer members as "free choices" in the let bindings 
                generalizedTyparsForRecursiveBlock) = 


        if verboseCC then dprintf "---- MakeCtorForIncrClassConstructionPassC\n";
        let denv = env.DisplayEnv 
        let thisVal      = ctorInfo.incrClassInstanceCtorThisVar 

        let thisTy    = thisVal.Type
          
        let m = thisVal.Range
        let ctorDeclaredTypars = ctorInfo.incrClassInstanceCtorDeclaredTypars
        if verboseCC then dprintf "ctorDeclaredTypars original : %s\n" (showL (TyparsL ctorDeclaredTypars));

        let ctorDeclaredTypars = ChooseCanonicalDeclaredTyparsAfterInference cenv.g denv ctorDeclaredTypars m
        if verboseCC then dprintf "ctorDeclaredTypars canonical: %s\n" (showL (TyparsL ctorDeclaredTypars));

        let freeChoiceTypars = ListSet.subtract typar_ref_eq generalizedTyparsForRecursiveBlock ctorDeclaredTypars

        let thisTyInst = List.map mk_typar_ty ctorDeclaredTypars

        let acc_free_in_expr acc expr =
            union_freevars acc (free_in_expr CollectLocalsNoCaching expr) 
            
        let acc_free_in_binding acc (bind:Binding) = 
            acc_free_in_expr acc bind.Expr
            
        let acc_free_in_bindings acc (binds:Binding list) = 
            (acc,binds) ||> List.fold acc_free_in_binding

        // Find all the variables used in any method. These become fields.
        //   staticForcedFieldVars:FreeLocals: the vars forced to be fields due to static member bindings, instance initialization expressions or instance member bindings
        //   instanceForcedFieldVars: FreeLocals: the vars forced to be fields due to instance member bindings
                                            
        let staticForcedFieldVars,instanceForcedFieldVars = 
             let (staticForcedFieldVars,instanceForcedFieldVars) = 
                 ((empty_freevars,empty_freevars),decs) ||> List.fold (fun (staticForcedFieldVars,instanceForcedFieldVars) dec -> 
                    match dec with 
                    // Construction is done so we can set the ref cell 
                    | PassCCtorComplete ->  (staticForcedFieldVars,instanceForcedFieldVars)
                    | PassCBindings decs ->
                        ((staticForcedFieldVars,instanceForcedFieldVars),decs) ||> List.fold (fun (staticForcedFieldVars,instanceForcedFieldVars) dec -> 
                            match dec with 
                            | IncrClassBindingGroup(binds,isStatic,isRec) -> 
                                let methodBinds = binds |> List.filter (IncrClassReprInfo.IsMethodRepr cenv) 
                                let staticForcedFieldVars = 
                                    if isStatic then 
                                        // Any references to static variables in any static method force the variable to be represented as a field
                                        (staticForcedFieldVars,methodBinds) ||> acc_free_in_bindings
                                    else
                                        // Any references to static variables in any instance bindings force the variable to be represented as a field
                                        (staticForcedFieldVars,binds) ||> acc_free_in_bindings
                                        
                                let instanceForcedFieldVars = 
                                    // Any references to instance variables in any methods force the variable to be represented as a field
                                    (instanceForcedFieldVars,methodBinds) ||> acc_free_in_bindings
                                        
                                (staticForcedFieldVars,instanceForcedFieldVars)
                            | IncrClassDo (e,isStatic) -> 
                                let staticForcedFieldVars = 
                                    if isStatic then 
                                        staticForcedFieldVars
                                    else
                                        union_freevars staticForcedFieldVars (free_in_expr CollectLocalsNoCaching e)
                                (staticForcedFieldVars,instanceForcedFieldVars)))
             let staticForcedFieldVars  = (staticForcedFieldVars,memberBinds) ||> acc_free_in_bindings 
             let instanceForcedFieldVars = (instanceForcedFieldVars,memberBinds) ||> acc_free_in_bindings 
             
             // Any references to static variables in the 'inherits' expression force those static variables to be represented as fields
             let staticForcedFieldVars = (staticForcedFieldVars,inheritsExpr) ||> acc_free_in_expr

             (staticForcedFieldVars.FreeLocals,instanceForcedFieldVars.FreeLocals)


        // Compute the implicit construction side effects of single 
        // 'let' or 'let rec' binding in the implicit class construction sequence 
        let TransBind (reps:IncrClassReprInfo) (TBind(v,rhs,spBind)) =
            // move to CheckMembersForm?? 
            if v.MustInline then
                error(Error("Local class bindings may not be marked inline. Consider lifting the definition out of the class or else do not mark it as inline",v.Range));
            let rhs = reps.FixupIncrClassExprPassC (Some(thisVal)) thisTyInst rhs
            
            match reps.LookupRepr v with
            | InMethod(isStatic,methodVal,_) -> 
                let _,chooseTps,tauExpr,tauTy,m = 
                    match rhs with 
                    | TExpr_tchoose(chooseTps,b,_) -> [],chooseTps,b,(type_of_expr cenv.g b),m 
                    | TExpr_tlambda (_,tps,TExpr_tchoose(chooseTps,b,_),m,returnTy,_) -> tps,chooseTps,b,returnTy,m 
                    | TExpr_tlambda (_,tps,b,m,returnTy,_) -> tps,[],b,returnTy,m 
                    | e -> [],[],e,(type_of_expr cenv.g e),range_of_expr e
                    
                let chooseTps = chooseTps @ freeChoiceTypars
                // Add the 'this' variable as an argument
                let tauExpr,tauTy = 
                    if isStatic then 
                        tauExpr,tauTy
                    else
                        let e = mk_lambda m thisVal (tauExpr,tauTy)
                        e, type_of_expr cenv.g e
                // Replace the type parameters that used to be on the rhs with 
                // the full set of type parameters including the type parameters of the enclosing class
                let rhs = mk_tlambda m methodVal.Typars (mk_tchoose m chooseTps tauExpr,tauTy)
                (fun e -> e), [TBind (methodVal,rhs,spBind)]
            
            // If it's represented as a non-escaping local variable then just bind it to its value
            // If it's represented as a non-escaping local arg then no binding necessary (ctor args are already bound)
            
            | InVar isArg ->
                (fun e -> if isArg then e else mk_let_bind m (TBind(v,rhs,spBind)) e), []
            | _ ->
                 // Use spBind if it available as the span for the assignment into the field
                let m =
                     match spBind,rhs with 
                     // Don't generate big sequence points for functions in classes
                     | _, (TExpr_lambda  _ | TExpr_tlambda _) -> v.Range
                     | SequencePointAtBinding m,_ -> m 
                     | _ -> v.Range
                let assignExpr = reps.MakeValueAssign (Some(thisVal)) thisTyInst v rhs m
                (fun e -> mk_seq SequencePointsAtSeq m assignExpr e), []

        /// Work out the implicit construction side effects of a 'let', 'let rec' or 'do' 
        /// binding in the implicit class construction sequence 
        let TransTrueDec isCtorArg (reps:IncrClassReprInfo) dec = 
              match dec with 
              | (IncrClassBindingGroup(binds,isStatic,isRec)) ->
                  let actions,reps,methodBinds = 
                      let reps     = (reps,binds) ||> List.fold (fun rep bind -> rep.ChooseAndAddRepresentation(cenv,env,isStatic,isCtorArg,ctorInfo,staticForcedFieldVars,instanceForcedFieldVars,bind)) // extend
                      if isRec then
                          // Note: the recursive calls are made via members on the object
                          // or via access to fiels. THis means the recursive loop is "broken", 
                          // and we can collapse to sequential bindings 
                          let reps     = (reps,binds) ||> List.fold (fun rep bind -> rep.ValNowWithRepresentation bind.Var) // inscope before
                          let actions,methodBinds = binds |> List.map (TransBind reps) |> List.unzip // since can occur in RHS of own defns 
                          actions,reps,methodBinds
                      else 
                          if debug then dprintf "TransDec: %d bindings, isRec=%b\n" binds.Length isRec;
                          let actions,methodBinds = binds |> List.map (TransBind reps)  |> List.unzip
                          let reps     = (reps,binds) ||> List.fold (fun rep bind -> rep.ValNowWithRepresentation bind.Var) // inscope after
                          actions,reps,methodBinds
                  let methodBinds = List.concat methodBinds
                  if isStatic then (actions,[],methodBinds),reps
                  else ([],actions,methodBinds),reps

              | IncrClassDo (doExpr,isStatic) -> 
                  let doExpr = reps.FixupIncrClassExprPassC (Some(thisVal)) thisTyInst doExpr
                  let binder = (fun e -> mk_seq SequencePointsAtSeq (range_of_expr doExpr) doExpr e)
                  if isStatic then ([binder],[],[]),reps
                  else ([],[binder],[]),reps


        /// Work out the implicit construction side effects of each declaration 
        /// in the implicit class construction sequence 
        let TransDec (reps:IncrClassReprInfo) dec = 
            match dec with 
            // Construction is done so we can set the ref cell 
            | PassCCtorComplete ->  
                let binders = 
                    match ctorInfo.incrClassInstanceCtorThisVarRefCellOpt with 
                    | None ->  []
                    | Some v -> 
                        let thisVarRefCellSetExpr = reps.FixupIncrClassExprPassC (Some(thisVal)) thisTyInst (mk_refcell_set cenv.g m ctorInfo.incrClassInstanceCtorThisVar.Type (expr_for_val m v) (expr_for_val m ctorInfo.incrClassInstanceCtorThisVar))
                        let binder = (fun e -> mk_seq SequencePointsAtSeq (range_of_expr thisVarRefCellSetExpr) thisVarRefCellSetExpr e)
                        [ binder ]
                ([],binders,[]),reps
                
            | PassCBindings decs -> 
                let initActions,reps = List.mapfold (TransTrueDec false) reps decs 
                let cctorInitActions,ctorInitActions,methodBinds = List.unzip3 initActions
                (List.concat cctorInitActions, List.concat ctorInitActions, List.concat methodBinds), reps 

                

        let reps = (IncrClassReprInfo.Empty(cenv.g))

        // Bind the IsArg(true) representations of the object constructor arguments and assign them to fields
        // if they escape to the members. We do this by running the instance bindings 'let x = x' through TransTrueDec
        // for each constructor argument 'x', but with the special flag 'isCtorArg', which helps TransBind know that 
        // the value is already available as an argument, and that nothing special needs to be done unless the 
        // value is being stored into a field.
        let (cctorInitActions1, ctorInitActions1,methodBinds1),reps = 
            let binds = ctorInfo.incrClassInstanceCtorArgs |> List.map (fun v -> mk_invisible_bind v (expr_for_val v.Range v))
            TransTrueDec true reps (IncrClassBindingGroup(binds,false,false))

        // We expect that only ctorInitActions1 will be non-empty here, and even then only if some elements are stored in the field
        assert (isNil cctorInitActions1)
        assert (isNil methodBinds1)

        // Now deal with all the 'let' and 'member' declarations
        let initActions,reps = List.mapfold TransDec reps decs
        let cctorInitActions2, ctorInitActions2,methodBinds2 = List.unzip3 initActions
        let cctorInitActions = cctorInitActions1 @  List.concat cctorInitActions2
        let ctorInitActions = ctorInitActions1 @ List.concat ctorInitActions2
        let methodBinds = methodBinds1 @ List.concat methodBinds2

        let ctorBody =
            // REVIEW: the sequence points aren't right here.
            let ctorInitAction = List.foldBack (fun binder acc -> binder acc) ctorInitActions (mk_unit cenv.g m)
            let ldarg0   = mk_ldarg0 m thisTy
            let ctorBody = mk_seq (if inheritsIsVisible then SequencePointsAtSeq else SuppressSequencePointOnExprOfSequential) m inheritsExpr (mk_invisible_let m thisVal ldarg0 ctorInitAction)
            let ctorBody = mk_basev_multi_lambdas m [] ctorInfo.incrClassInstanceCtorBaseValOpt [ctorInfo.incrClassInstanceCtorArgs] (ctorBody,cenv.g.unit_ty)
            ctorBody

        let cctorBodyOpt =
            /// Omit the .cctor if it's empty 
            match cctorInitActions with
            | [] -> None 
            | _ -> 
                let cctorInitAction = List.foldBack (fun binder acc -> binder acc) cctorInitActions (mk_unit cenv.g m)
                let m = thisVal.Range
                let cctorArgs,_,_ = ctorInfo.incrClassStaticCtorValInfo.Force()
                let cctorBody = mk_basev_multi_lambdas m [] None [cctorArgs] (cctorInitAction,cenv.g.unit_ty)
                Some(cctorBody)
        
        ctorBody,cctorBodyOpt,methodBinds,reps

// Checking of members and 'let' bindings in classes
module TyconBindingChecking = begin 

    open IncrClassChecking 

    // Technique: multiple passes.
    //   - create val_specs for recursive items given names and args
    //   - type check AST to TAST collecting (sufficient) type constraints
    //   - determine typars to generalize over
    //   - generalize definitions (fixing up recursive instances)
    //   - build ctor binding
    //   - Yields set of recursive bindings for the ctors and members of the types.
    
    type TyconBindingsPassA =
      | PassAIncrClassCtor     of IncrClassCtorLhs
      | PassAInherit           of SynType * SynExpr * (*base:*)Val option * range
      | PassAIncrClassBindings of TyconRef * Ast.SynBinding list * (* isStatic:*) bool * (*recursive:*) bool * range
      | PassAMember            of RecursiveBindingInfo * NormalizedBinding
      | PassAOpen              of LongIdent * range
      // Indiates the last 'field' has been initialized, only 'do' comes after 
      | PassAIncrClassCtorComplete 

    type TyconBindingsPassB =
      | PassBIncrClassCtor     of IncrClassCtorLhs * Tast.Binding option
      | PassBInherit           of expr * Val option
      | PassBIncrClassBindings of IncrClassBindingGroup list
      | PassBMember            of RecursiveBindingInfo * TBindingInfo * typars
      // Indicates the last 'field' has been initialized, only 'do' comes after 
      | PassBIncrClassCtorComplete of FreeTypars 
      | PassBOpen              of LongIdent * range

    type TyconBindingsPassC =
      | PassCIncrClassCtor     of IncrClassCtorLhs * Tast.Binding option
      | PassCInherit           of expr * Val option
      | PassCIncrClassBindings of IncrClassBindingGroup list
      | PassCMember            of RecursiveUseFixupPoints * Tast.Binding
      | PassCOpen              of LongIdent * range
      // Indicates the last 'field' has been initialized, only 'do' comes after 
      | PassCIncrClassCtorComplete     

    /// Get the "this" variable from an instance member binding
    let GetInstanceMemberThisVariable (v:Val,x) =
        // Skip over LAM tps. Choose 'a. 
        if v.IsInstanceMember then
            let rec firstArg e =
              match e with
                | TExpr_tlambda (_,tps,b,_,returnTy,_) -> firstArg b
                | TExpr_tchoose (_,b,_) -> firstArg b
                | TExpr_lambda  (_,_,[v],b,_,_,_) -> Some v
                | _ -> failwith "GetInstanceMemberThisVariable: instance member did not have expected internal form"
           
            firstArg x
        else
            None


    /// Main routine
    let TcTyconBindings cenv (env:tcEnv) tpenv bindsm scopem (bindsl : TyconBindingDefns list) =
        
        let ad = AccessRightsOfEnv env
        let denv = env.DisplayEnv
        let envInitial = env
        let env = ()
        
        // PassA: create member prelimRecValues for "recursive" items, i.e. ctor val and member vals 
        // PassA: also processes their arg patterns - collecting type assertions 
        let defnsAs, (tpenv,prelimRecValues) =
            if verboseCC then dprintf "---------- passA: --------------------\n";

            ((tpenv,NameMap.empty),bindsl) ||> List.mapfold (fun (tpenv,prelimRecValues) (TyconBindingDefns(tcref, binds)) -> 

                (* Class members can access protected members of the implemented type *)
                (* Class members can access private members in the type *)
                let envForTycon = MkInnerEnvForTyconRef cenv envInitial tcref false
                // Re-add the type constructor to make it take precedence for record label field resolutions
                let envForTycon = AddLocalTycons cenv.g cenv.amap tcref.Range [tcref.Deref] envForTycon
                let cpath = curr_access_cpath envForTycon
                let defnAs,(_,envForTycon,tpenv,prelimRecValues) = 
                    ((None,envForTycon,tpenv,prelimRecValues),binds) ||> List.collectFold (fun (incrClassCtorLhsOpt,env,tpenv,prelimRecValues) (TyconBindingDefn(containerInfo,newslotsOK,declKind,classMemberDef,m)) ->

                        if tcref.IsTypeAbbrev then error(Error("Type abbreviations may not have members",m));
                        if is_enum_tcref tcref then error(Error("Enumerations may not have members",m));

                        match classMemberDef, containerInfo with
                        
                          | ClassMemberDefn_implicit_ctor (vis,attrs,spats,thisIdOpt, m), ContainerInfo(_,Some(tcref,_,baseValOpt,_)) ->
                              match tcref.TypeOrMeasureKind with KindMeasure -> error(Error("Measure declarations may have only static members", m)) | _ -> ();

                              (* note; ContainerInfo is certain to be an option *)
                              (* PassA: make incrClassCtorLhs - ctorv, thisVal etc, type depends on argty(s) *)
                              let incrClassCtorLhs = TcImplictCtorLhsPassA(cenv,env,tpenv,tcref,vis,attrs,spats,thisIdOpt,baseValOpt,m)
                              (* PassA: Add ctorDeclaredTypars from incrClassCtorLhs - or from tcref *)
                              let env = AddDeclaredTypars CheckForDuplicateTypars incrClassCtorLhs.incrClassInstanceCtorDeclaredTypars env
                              [PassAIncrClassCtor incrClassCtorLhs],
                              (Some(incrClassCtorLhs),env,tpenv,prelimRecValues)
                              
                          | ClassMemberDefn_implicit_inherit (typ,arg,baseIdOpt,m),_ ->
                              match tcref.TypeOrMeasureKind with KindMeasure -> error(Error("Measure declarations may have only static members", m)) | _ -> ();
                              (* PassA: inherit typ(arg) as base - pass through *)
                              let baseValOpt = incrClassCtorLhsOpt |> Option.bind (fun x -> x.incrClassInstanceCtorBaseValOpt)
                              [PassAInherit (typ,arg,baseValOpt,m)],   (* pick up baseValOpt! *)
                              (incrClassCtorLhsOpt,env,tpenv,prelimRecValues)
                              
                          | ClassMemberDefn_let_bindings (letBinds,isStatic,isRec,m),_ ->
                              match tcref.TypeOrMeasureKind,isStatic with KindMeasure,false -> error(Error("Measure declarations may have only static members", m)) | _,_ -> ();

                              if is_struct_tcref tcref && not isStatic then 
                                   let allDo = letBinds |> List.forall (function (Binding(_,DoBinding,_,_,_,_,_,_,_,_,_)) -> true | _ -> false)
                                   if allDo then 
                                      errorR(Deprecated("Structs may not contain 'do' bindings because the default constructor for structs would not execute these bindings",m));
                                   else
                                      errorR(Error("Structs may not contain 'let' bindings because the default constructor for structs will not execute these bindings. Consider adding additional arguments to the primary constructor for the type",m));

                              if isStatic && isNone incrClassCtorLhsOpt then 
                                  errorR(Error("Static 'let' bindings may only be defined in class types with implicit constructors",m));
                              
                              (* PassA: let-bindings - pass through *)
                              [PassAIncrClassBindings (tcref,letBinds,isStatic,isRec,m)],
                              (incrClassCtorLhsOpt,env,tpenv,prelimRecValues)     
                              
                          | ClassMemberDefn_member_binding (bind,m),_ ->
                              if verboseCC then dprintf "PassA: member\n";
                              (* PassA: member binding - create prelim valspec (for recursive reference) and RecursiveBindingInfo *)
                              let (NormalizedBinding(_,_,_,_,_,_,_,valSynData,_,_,_,_)) as bind = BindingNormalization.NormalizeBinding ValOrMemberBinding cenv env bind
                              let (ValSynData(memberFlagsOpt,_,_)) = valSynData 
                              match tcref.TypeOrMeasureKind with
                              | KindType -> ()
                              | KindMeasure ->
                                (match memberFlagsOpt with 
                                | None -> () 
                                | Some memberFlags -> 
                                  if memberFlags.MemberIsInstance then error(Error("Measure declarations may have only static members", m));
                                  match memberFlags.MemberKind with 
                                  | MemberKindConstructor -> error(Error("Measure declarations may have only static members: constructors are not available", m))
                                  | _ -> ()
                                );
                              let rbind = NormalizedRecBindingDefn(containerInfo,newslotsOK,declKind,bind)
                              let overridesOK  = DeclKind.CanOverrideOrImplement(declKind)
                              let binds,(tpenv,prelimRecValues) = AnalyzeAndMakeRecursiveValue overridesOK cenv env (tpenv,prelimRecValues) rbind
                              [ for (rbinfo,bind) in binds -> PassAMember (rbinfo,bind) ],
                              (incrClassCtorLhsOpt,env,tpenv,prelimRecValues)
                        
                          | ClassMemberDefn_open (mp,m),_ ->
                              [ PassAOpen (mp,m) ],
                              (incrClassCtorLhsOpt,env,tpenv,prelimRecValues)
                        
                          | _ -> 
                              error(InternalError("Unexpected definition",m)))

                let defnAs = 
                    (* Insert PassAIncrClassCtorComplete at the point where local construction is known to have been finished *)
                    (* REVIEW: this is quadratic *)
                    let rec insertDone defns = 
                        if defns |> List.forall (function 
                          | PassAOpen _ | PassAIncrClassCtor _ | PassAInherit _ | PassAIncrClassCtorComplete -> false
                          | PassAIncrClassBindings (_,binds,isStatic,isRec,_) -> 
                              (* Detect 'let _ =' and 'let () =' bindings, which are 'do' bindings. *)
                              binds |> List.forall (function (Binding (_,DoBinding,_,_,_,_,_,_,_,_,_)) -> true | _ -> false)
                          | PassAMember _ -> true)
                        then PassAIncrClassCtorComplete :: defns 
                        else List.hd defns :: insertDone (List.tl defns)
                    insertDone defnAs

                (envForTycon,defnAs),(tpenv,prelimRecValues))


        let prelimRecValues = NameMap.map fst prelimRecValues
     
        // PassB: type check pass, convert from ast to tast and collects type assertions 

        if verboseCC then dprintf "---------- passB: --------------------\n";        

        let defnsBs,tpenv = 


            // Loop through the types being defined...
            (tpenv,defnsAs) ||> List.mapfold (fun tpenv (envForTycon, defnAs) -> 
                // Add prelimRecValues to env (breaks recursion) and vrec=true 
                let envForTycon = AddLocalValMap scopem prelimRecValues envForTycon
                if verboseCC then 
                    prelimRecValues |> NameMap.iteri (fun name v -> dprintf "prelim %s : %s\n" name ((DebugPrint.showType v.Type))) ;
                
                // Set up the environment so use-before-definition warnings are given, at least 
                // until we reach a PassAIncrClassCtorComplete. 
                let envForTycon = { envForTycon with eCtorInfo = Some (InitialImplicitCtorInfo()) }
                    
                // Loop through the definition elements in a type...
                let defnBs,(tpenv,_,_) = 
                    ((tpenv,envForTycon,envForTycon),defnAs) ||> List.mapfold  (fun (tpenv,envInstance,envStatic) defnA -> 
                        match defnA with
                        | PassAIncrClassCtor incrClassCtorLhs ->
                            if verboseCC then dprintf "PassB: ctor\n";
                            (* PassB: enrich envInstance with implicit ctor args *)
                            let envInstance = match incrClassCtorLhs.incrClassInstanceCtorThisVarRefCellOpt with Some v -> AddLocalVal scopem v envInstance | None -> envInstance
                            let envInstance = List.foldBack AddLocalValPrimitive incrClassCtorLhs.incrClassInstanceCtorArgs envInstance 
                            let thisVarRefCellBindOpt = TcLetrecComputeCtorThisVarRefCellBinding cenv incrClassCtorLhs.incrClassInstanceCtorThisVarRefCellOpt
                            PassBIncrClassCtor (incrClassCtorLhs,thisVarRefCellBindOpt),
                            (tpenv,envInstance,envStatic)
                            
                        | PassAInherit (sty,arg,baseValOpt,m) ->
                            (* PassB: build new object expr for the inherit-call *)
                            let ty,tpenv = TcType cenv NoNewTypars CheckCxs envInstance tpenv sty
                            let inheritsExpr,tpenv = TcNewExpr cenv envInstance tpenv ty (Some (range_of_syntype sty)) true arg m
                            let envInstance = 
                                match baseValOpt with 
                                | Some baseVal -> AddLocalVal scopem baseVal envInstance 
                                | None -> envInstance
                            PassBInherit (inheritsExpr,baseValOpt),
                            (tpenv,envInstance,envStatic)
                            
                        | PassAIncrClassBindings (tcref,binds,isStatic,isRec,bindsm) ->
                            if verboseCC then dprintf "PassB: bindings\n";
                            let envForMember = if isStatic then envStatic else envInstance
                            (* PassB: let bindings *)
                            let binds,bindRs,env,tpenv = 
                                if isRec then
                                    (* type check local recursive binding *)
                                    let binds = binds |> List.map (fun bind -> RecBindingDefn(ExprContainerInfo,NoNewSlots,ClassLetBinding,bind))
                                    let binds,env,tpenv = TcLetrec ErrorOnOverrides cenv envForMember tpenv (binds,scopem(*bindsm*),scopem)
                                    let bindRs = [IncrClassBindingGroup(binds,isStatic,true)]
                                    binds,bindRs,env,tpenv 
                                else
                                    (* type check local binding *)
                                    let binds,env,tpenv = TcLetBindings cenv envForMember ExprContainerInfo ClassLetBinding tpenv (binds,bindsm,scopem)
                                    let binds,bindRs = 
                                        binds 
                                        |> List.map (function
                                            | TMDefLet(bind,_) -> [bind],IncrClassBindingGroup([bind],isStatic,false)
                                            | TMDefDo(e,_) -> [],IncrClassDo(e,isStatic)
                                            | _ -> error(InternalError("unexpected definition kind",tcref.Range)))
                                        |> List.unzip
                                    List.concat binds,bindRs,env,tpenv

                            // Check to see that local bindings and members don't have the same name
                            for bind in binds do 
                                let nm = bind.Var.DisplayName
                                let ty = snd (generalize_tcref tcref)
                                match TryFindMethInfo cenv.infoReader bind.Var.Range ad nm ty,
                                      TryFindPropInfo cenv.infoReader bind.Var.Range ad nm ty with 
                                | [],[] -> ()
                                | _ -> errorR (Error(sprintf "A member and a local class binding both have the name '%s'" nm,bind.Var.Range));

                            // Also add static entries to the envInstance if necessary 
                            let envInstance = (if isStatic then List.foldBack (var_of_bind >> AddLocalVal scopem) binds envInstance else env)
                            let envStatic = (if isStatic then env else envStatic)
                            PassBIncrClassBindings bindRs,
                            (tpenv,envInstance,envStatic)
                              
                        | PassAIncrClassCtorComplete -> 
                            (* Lift the restriction that results in use-before-initialization warnings *)
                            PassBIncrClassCtorComplete empty_free_loctypars, (* <-- PATCHED: the ungeneralisable typars are computed in PassB2 below *)
                            (tpenv, ExitCtorPreConstructRegion envInstance,envStatic)
                            
                        | PassAOpen(mp,m) -> 
                            let envInstance = TcOpenDecl cenv.g cenv.amap m scopem envInstance mp
                            let envStatic = TcOpenDecl cenv.g cenv.amap m scopem envStatic mp
                            PassBOpen(mp,m),
                            (tpenv, envInstance,envStatic  )

                        (* Note: this path doesn't add anything the environment, because the member is already available off via its type *)
                        | PassAMember (rbinfo,bind) ->
                            if verboseCC then dprintf "PassB: member\n";
                            (* PassB: Typecheck member binding, generalize them later, when all type constraints are known *)
                            (* static members are checked under envStatic.
                             * envStatic contains class typars and the (ungeneralized) members on the class(es).
                             * envStatic has no instance-variables (local let-bindings or ctor args). *)
                            let (RBInfo(_,_,_,v,_,_,_,_,_,_,_,_)) = rbinfo

                            let envForMember = if v.IsInstanceMember then envInstance else envStatic

                            let (rbinfo,tbind),tpenv = TcLetrecBinding cenv envForMember scopem prelimRecValues tpenv (rbinfo,bind)
                             
                            if verboseCC then dprintf "PassBMember: vspec = %s\n" (TypeOfvalL v |> showL);
                            
                            let uncomputedGeneralizedTypars= [] // <-- PATCHED: generalized typars are computed in PassB2 below 
                            
                            PassBMember (rbinfo,tbind,uncomputedGeneralizedTypars),
                            (tpenv,envInstance,envStatic))
                defnBs,tpenv)

        defnsBs |> List.iter (fun defnBs -> 
            // Take ctor (and inherit if present) and build ctor binding rhs expr.
            // Also yields fixup function to fixup references to local state.
            // That must be applied to the expr in the member definitions - and - doing so makes some type assertions on the member this typars.
            // Q: can the member this typars ever be anything other than free typars???
            //------
            // Is it enough to make all the "this" arguments type equal?
            // If they refer to a local state variable, then this assertion will be made.
            // If they dont, is there harm in making that assertion??
            // Surely, those typars must after typechecking still be a unique set of typar?
            match defnBs with 
            | PassBIncrClassCtor (incrClassCtorLhs,thisVarRefCellBindOpt) :: otherDefnBs ->
                (* Assert the types of 'this' *)
                otherDefnBs |> List.iter (function
                    | PassBMember (rbinfo,tbind,_) ->
                        let (RBInfo(_,_,_,vspec,_,_,_,_,_,_,_,_)) = rbinfo
                        let (TBindingInfo(_,_,_,_,_,_,_,expr,_,_,m,_,_,_)) = tbind
                        (match GetInstanceMemberThisVariable (vspec,expr) with
                           | None -> ()
                           | Some vx_thisv ->
                               let incrClassInstanceCtorThisVar = incrClassCtorLhs.incrClassInstanceCtorThisVar
                               if verboseCC then dprintf "vx_thisv.Type = %s, incrClassInstanceCtorThisVar.Type = %s \n" (TypeOfvalL vx_thisv |> showL) (showL (typeL incrClassInstanceCtorThisVar.Type));
                               unifyE cenv envInitial m vx_thisv.Type incrClassInstanceCtorThisVar.Type)
                    | _ -> ())
            | _ -> ());
          
        // Note: we now have enough information to proceed with generalisation.

        // Pass B2: This computes the ungeneralisable typars for the member block(s).
        // These typars are:
        // - those typars free in envInitial
        // - those typars free in the let-bindings inscope over the members,
        //   in particular, these dangling typars are allowed to remain,
        //   to be resolved later in the file, but they must not be generalized.
        //   CONSIDER: ungeneralized typars could generate a warning.
        // Implementation:
        //   For each type,
        //     We add the defns to an environment based on envInitial (not envinner).
        //     Note, envinner = envInitial + prelimRecValues, and the prelimRecValues are the members.
        //   The free typars of each type's env determines ungeneralisable typars.
        //   The union of the these are ungeneralisable over the members letrec block.
        let defnsBs = 
            defnsBs |> List.map (fun defnBs ->
                let defnBs,_ = 
                    (envInitial,defnBs) ||> List.mapfold (fun env defnB ->
                      match defnB with
                        | PassBIncrClassCtor (incrClassCtorLhs,_) ->
                            if verboseCC then dprintf "PassB2: ctor\n";
                            let env = match incrClassCtorLhs.incrClassInstanceCtorThisVarRefCellOpt with Some v -> AddLocalVal scopem v env | None -> env
                            let env = List.foldBack AddLocalValPrimitive incrClassCtorLhs.incrClassInstanceCtorArgs env 
                            defnB,env
                        | PassBInherit (inheritsExpr,baseValOpt) ->
                            let env = match baseValOpt with Some baseVal -> AddLocalVal scopem baseVal env | None -> env
                            defnB,env
                        | PassBIncrClassBindings bindRs ->
                            let collectBind env (bind:Binding) = AddLocalValPrimitive bind.Var env
                            let collectBindRs env b = 
                                match b with 
                                | (IncrClassBindingGroup(binds,_,_)) -> List.fold collectBind env binds
                                | IncrClassDo (e,_) -> env
                            let env = List.fold collectBindRs env bindRs
                            defnB,env
                        | PassBIncrClassCtorComplete ignoredSinceItIsComputedHere ->
                            PassBIncrClassCtorComplete (GeneralizationHelpers.ComputeUngeneralizableTypars env),  (* <--- PATCHING: this pass computes this typar set *)
                            env
                        | PassBMember _ ->
                            defnB,env
                        | PassBOpen (_,m) -> error(Error("'open' declarations may not be used in classes",m)))
                defnBs)

        begin         
            let supportForBindings = 
                defnsBs |> List.collect (fun defnBs -> 
                    defnBs |> List.collect (fun defnB -> 
                        match defnB with
                          | PassBOpen _ | PassBIncrClassCtor  _ | PassBInherit _  | PassBIncrClassBindings _ | PassBIncrClassCtorComplete _ -> []
                          | PassBMember (rbinfo ,tbind,ignoredSinceItIsComputedHere) -> TcLetrecComputeSupportForBinding cenv (rbinfo,tbind)))
            GeneralizationHelpers.CanonicalizePartialInferenceProblem (cenv,denv,bindsm) supportForBindings; 
        end;

        let freeInEnv = 
            let freeInEnvBase = GeneralizationHelpers.ComputeUngeneralizableTypars envInitial
            let freeInEnvForMembers = List.reduce_left Zset.union (defnsBs |> List.concat |> List.choose (function PassBIncrClassCtorComplete ungeneralizableTypars -> Some ungeneralizableTypars | _ -> None))
            (* The type variables on the type and associated with the constructor are generalizable in the members. *)
            (* UnGen = freeInEnv(Ctor) + (freeInEnv(Members) - freeInEnv(ctorTyVars)) *)
            
            let freeInEnvForMembersMinusCtorVars = 
                List.foldBack (fun defnBs acc -> 
                    match defnBs with 
                    | PassBIncrClassCtor (incrClassCtorLhs,thisVarRefCellBindOpt) :: otherDefnBs ->
                        List.foldBack Zset.remove incrClassCtorLhs.incrClassInstanceCtorDeclaredTypars acc 
                    | _ -> acc)
                    defnsBs
                    freeInEnvForMembers
            let ungeneralizableTypars  = Zset.union freeInEnvBase freeInEnvForMembersMinusCtorVars
            ungeneralizableTypars

        // PassB3: fill in generalizedTypars now all equations/constraints are known 
        if verboseCC then dprintf "---------- passB3: --------------------\n";    

        let defnsBs = 
            defnsBs |> List.map (fun defnBs -> 
                defnBs |> List.map (fun defnB -> 
                    match defnB with
                      | PassBOpen _ | PassBIncrClassCtor  _ | PassBInherit _  | PassBIncrClassBindings _ | PassBIncrClassCtorComplete _ -> defnB
                      | PassBMember (rbinfo ,tbind,ignoredSinceItIsComputedHere) ->
                          let generalizedTypars = TcLetrecComputeGeneralizedTyparsForBinding cenv envInitial freeInEnv (rbinfo,tbind)
                          if verboseCC then dprintf "PassB3: member generalizedTypars = %s\n" (showL (TyparsL generalizedTypars));
                          PassBMember (rbinfo,tbind,generalizedTypars))) // <--- PATCHING: filling in generalizedTypars 

        // Compute the entire set of generalized typars, including those on ctors 
        let generalizedTyparsForRecursiveBlock =
            defnsBs 
            |> List.map (List.choose (function
                               | PassBMember(_,_,generalizedTypars) -> Some generalizedTypars
                               | PassBIncrClassCtor (incrClassCtorLhs,_) -> Some (incrClassCtorLhs.incrClassInstanceCtorDeclaredTypars)
                               | _ -> None) 
                    >> unionGeneralizedTypars)
            |> unionGeneralizedTypars

        if verboseCC then dprintf "PassB3: generalizedTyparsForRecursiveBlock = %s\n" (TyparsL generalizedTyparsForRecursiveBlock |> Layout.showL);

        (* PassC: generalize - both ctor and members *)
        if verboseCC then dprintf "---------- passC: --------------------\n";

        let defnsCs,tpenv = 
            (tpenv,defnsBs) ||> List.mapfold (fun tpenv defnBs -> 
                (tpenv,defnBs) ||> List.mapfold (fun tpenv defnB -> 

                    // PassC: Generalise implicit ctor val 
                    match defnB with
                    | PassBIncrClassCtor (incrClassCtorLhs,thisVarRefCellBindOpt) ->
                        let valscheme = incrClassCtorLhs.incrClassInstanceCtorValScheme
                        let valscheme = ChooseCanonicalValSchemeAfterInference cenv.g denv valscheme scopem
                        AdjustRecType cenv incrClassCtorLhs.incrClassInstanceCtorVal valscheme;
                        PassCIncrClassCtor (incrClassCtorLhs,thisVarRefCellBindOpt),tpenv

                    | PassBInherit (inheritsExpr,basevOpt) -> 
                        PassCInherit (inheritsExpr,basevOpt),tpenv

                    | PassBIncrClassBindings bindRs             -> 
                        PassCIncrClassBindings bindRs,tpenv

                    | PassBIncrClassCtorComplete _ -> 
                        PassCIncrClassCtorComplete, tpenv

                    | PassBOpen(mp,m) -> 
                        PassCOpen(mp,m), tpenv

                    | PassBMember (rbinfo,tbind,generalizedTypars)  ->
                        // PassC: Generalise member bindings 
                        let vxbind = TcLetrecGeneralizeBinding cenv denv generalizedTyparsForRecursiveBlock freeInEnv generalizedTypars (rbinfo,tbind)
                        let tpenv = HideUnscopedTypars generalizedTypars tpenv
                        let vxbind = TcLetrecBindCtorThisVarRefCell cenv vxbind rbinfo
                        let fixups,vxbind = FixupLetrecBind cenv envInitial.DisplayEnv vxbind
                        PassCMember (fixups,vxbind),
                        tpenv))
                        
        if verboseCC then dprintf "---------- make inherit expression --------------------\n";

        // --- Extract local vals from let-bindings 
        let fixupValueExprBinds,methodBinds =
            defnsCs |> List.map (fun defnCs -> 
                match defnCs with 

                // Cover the case where this is not a class with an implicit constructor
                | PassCIncrClassCtor (incrClassCtorLhs,thisVarRefCellBindOpt) :: defnCs -> 

                    // This is the type definition we're processing  
                    let tcref = incrClassCtorLhs.incrClassTcref

                    // Assumes inhert call immediately follows implicit ctor. Checked by CheckMembersForm 
                    let inheritsExpr,inheritsIsVisible,baseValOpt,defnCs = 
                        match defnCs with
                        | PassCInherit (inheritsExpr,baseValOpt) :: defnCs -> 
                            inheritsExpr,true,baseValOpt,defnCs

                        | defnCs ->
                            if is_struct_tcref tcref then 
                                mk_unit cenv.g tcref.Range, false,None, defnCs
                            else
                                let inheritsExpr,tpenv = TcNewExpr cenv envInitial tpenv cenv.g.obj_ty None true (Expr_const(Const_unit,tcref.Range)) tcref.Range
                                inheritsExpr,false,None,defnCs
                       
                    let envForTycon = MkInnerEnvForTyconRef cenv envInitial tcref false

                    // Compute the cpath used when creating the hidden fields 
                    let cpath = curr_access_cpath envForTycon

                    let localDecs  = defnCs |> List.filter (function PassCIncrClassBindings _ | PassCIncrClassCtorComplete -> true | _ -> false)
                    let memberBindsWithFixups = defnCs |> List.choose (function PassCMember(fixups,vxbind) -> Some (fixups,vxbind) | _ -> None) 

                    // Extend localDecs with "let ctorv = ref null" if there is a ctorv 
                    let localDecs  = 
                        match thisVarRefCellBindOpt with 
                        | None -> localDecs 
                        | Some bind -> PassCIncrClassBindings [IncrClassBindingGroup([bind],false,false)] :: localDecs
                        
                    // // Extend localDecs with "let arg = arg" bindings for each ctor arg, which is 
                    // // current mechanism for storing them as fields when they are captured by something
                    // // represented as a method
                    // let localDecs  = 
                    //    let binds = incrClassCtorLhs.incrClassInstanceCtorArgs |> List.map (fun v -> mk_invisible_bind v (expr_for_val v.Range v))
                    //    let ctorArgBinds = IncrClassBindingGroup(binds,false,false)
                    //    PassCIncrClassBindings [ctorArgBinds] :: localDecs

                    // Carve out the initialization sequence and decide on the localRep 
                    let ctorBodyLambdaExpr,cctorBodyLambdaExprOpt,methodBinds,localReps = 
                        
                        let localDecs = 
                            localDecs |> List.collect (function PassCIncrClassBindings(binds) -> [PassCBindings binds]
                                                                 | PassCIncrClassCtorComplete    -> [PassCCtorComplete]
                                                                 | _ -> [])
                        MakeCtorForIncrClassConstructionPassC(cenv,envForTycon,tpenv,incrClassCtorLhs,inheritsExpr,inheritsIsVisible,localDecs,List.map snd memberBindsWithFixups,generalizedTyparsForRecursiveBlock)

                    // Generate the (value,expr) pairs for the implicit 
                    // object constructor and implicit static initializer 
                    let ctorValueExprBindings = 
                        [ (let ctorValueExprBinding = TBind(incrClassCtorLhs.incrClassInstanceCtorVal,ctorBodyLambdaExpr,NoSequencePointAtStickyBinding)
                           FixupLetrecBind cenv envInitial.DisplayEnv (incrClassCtorLhs.incrClassInstanceCtorValScheme ,ctorValueExprBinding)) ]
                        @ 
                        ( match cctorBodyLambdaExprOpt with 
                          | None -> []
                          | Some(cctorBodyLambdaExpr) -> 
                             [ (let _,cctorVal, cctorValScheme = incrClassCtorLhs.incrClassStaticCtorValInfo.Force()
                                let cctorValueExprBinding = TBind(cctorVal,cctorBodyLambdaExpr,NoSequencePointAtStickyBinding)
                                FixupLetrecBind cenv envInitial.DisplayEnv (cctorValScheme,cctorValueExprBinding)) ] ) 

                    // Publish the fields of the representation to the type 
                    begin 
                        if verboseCC then dprintf "add fields to tcref\n";
                        localReps.PublishIncrClassFields cpath incrClassCtorLhs; (* mutation *)    
                    end;
                    
                    (* --- Members *)
                    // REVIEW: We should report information about the member to the name resolution sink
                    
                    if verboseCC then dprintf "---- fixup members\n";      
                    let memberBindsWithFixups = 
                        let applySubToVXBind (TBind(v,x,spBind)) =
                            let m = v.Range
                            // Work out the 'this' variable and type instantiation for field fixups. 
                            // We use the instantiation from the instance member if any. Note: It is likely this is not strictly needed 
                            // since we unify the types of the 'this' variables with those of the ctor declared typars. 
                            let thisVarOpt = GetInstanceMemberThisVariable (v,x)
                            // Members have at least as many type parameters as the
                            // enclosing class. Just grab the type variables from the member
                            let thisTyInst = List.map mk_typar_ty (List.take (tcref.Typars(m).Length) v.Typars)
                                    
                            let x = localReps.FixupIncrClassExprPassC thisVarOpt thisTyInst x 
                            TBind(v,x,spBind)
                        
                        let memberBindsWithFixups = memberBindsWithFixups |> List.map (map2'2 applySubToVXBind) 
                        memberBindsWithFixups
                        
                    ctorValueExprBindings @ memberBindsWithFixups, methodBinds  
                
                // Cover the case where this is not a class with an implicit constructor
                | defnCs -> 
                    let memberBindsWithFixups = defnCs |> List.choose (function PassCMember(fixups,vxbind) -> Some (fixups,vxbind) | _ -> None) 
                    memberBindsWithFixups,[])
            |> List.unzip
        let fixupValueExprBinds = List.concat fixupValueExprBinds
        let methodBinds = List.concat methodBinds 
        if verboseCC then dprintf "---- init graphs\n";          
        
        // INITIALIZATION GRAPHS 
        let binds = EliminateInitializationGraphs cenv.g true envInitial.DisplayEnv fixupValueExprBinds bindsm

        let binds = binds @ methodBinds
        
        // Post letrec env 
        let envbody = AddLocalValMap scopem prelimRecValues envInitial 
        if verboseCC then dprintf "TcTyconBindings: done\n";
        binds,envbody,tpenv

end

//-------------------------------------------------------------------------
// The member portions of class defns
//------------------------------------------------------------------------- 
    
let TcTyconMemberDefns cenv env parent bindsm scopem membersl = 
    let interfacesFromTypeDefn (TyconMemberData(declKind,tcref,_,declaredTyconTypars,members,m,_)) =
        let overridesOK  = DeclKind.CanOverrideOrImplement(declKind)
        members |> List.collect (function 
            | ClassMemberDefn_interface(ity,defnOpt,m) -> 
                  if tcref.IsTypeAbbrev then error(Error("Type abbreviations may not have interface declarations",m));
                  if is_enum_tcref tcref then error(Error("Enumerations may not have interface declarations",m));

                  begin match defnOpt with 
                  | Some(defn) -> 
                      let tcaug = tcref.TypeContents
                      let ity' = 
                          let envinner = AddDeclaredTypars CheckForDuplicateTypars declaredTyconTypars env
                          TcTypeAndRecover cenv NoNewTypars CheckCxs envinner emptyTyparEnv ity |> fst
                      if not (is_interface_typ cenv.g ity') then errorR(Error("This type is not an interface type",range_of_syntype ity));
                      
                      if not (tcaug_has_interface cenv.g tcaug ity') then 
                          error(Error("All implemented interfaces should be declared on the initial declaration of the type",range_of_syntype ity));
                      if (type_equiv cenv.g ity' cenv.g.mk_IComparable_ty && isSome(tcaug.tcaug_compare)) || 
                          (type_equiv cenv.g ity' cenv.g.mk_IStructuralComparable_ty && isSome(tcaug.tcaug_compare_withc)) ||
                          (type_equiv cenv.g ity' cenv.g.mk_IStructuralEquatable_ty && isSome(tcaug.tcaug_hash_and_equals_withc)) then
                          errorR(Error("A default implementation of this interface has already been added because the explicit implementation of the interface was not specified at the definition of the type",range_of_syntype ity));
                      if overridesOK = WarnOnOverrides then  
                          warning(IntfImplInIntrinsicAugmentation(range_of_syntype ity));
                      if overridesOK = ErrorOnOverrides then  
                          errorR(IntfImplInExtrinsicAugmentation(range_of_syntype ity));
                      [ (ity',defn,m) ]
                  | _-> []
                  end
                  
            | _ -> []) 

    let interfaceMembersFromTypeDefn (TyconMemberData(declKind,tcref,baseValOpt,declaredTyconTypars,_,m,newslotsOK)) (ity',defn,m) implTySet =
        let tcaug = tcref.TypeContents
        let containerInfo = ContainerInfo(parent,Some(tcref,Some(ity',implTySet),baseValOpt,declaredTyconTypars))
        defn  |> List.choose (fun mem ->
                match mem with
                | ClassMemberDefn_member_binding(b,m) -> 
                    Some(TyconBindingDefn(containerInfo,newslotsOK,declKind,mem,m))
                | ClassMemberDefn_let_bindings(_,_,_,m)    (* <-- possible design suggestion: relax this *)
                | ClassMemberDefn_implicit_ctor(_,_,_,_,m)
                | ClassMemberDefn_implicit_inherit(_,_,_,m)
                | ClassMemberDefn_interface(_,_,m) 
                | ClassMemberDefn_slotsig(_,_,m)
                | ClassMemberDefn_inherit(_,_,m)
                | ClassMemberDefn_field(_,m)
                | ClassMemberDefn_open (_,m)
                | ClassMemberDefn_tycon(_,_,m) -> errorR(Error("This member is not permitted in an interface implementation",m)); None)

    let tpenv = emptyTyparEnv
    if verboseCC then dprintf "TcTyconMemberDefns:\n";
    if verboseCC then membersl |> List.iter (fun (TyconMemberData(declKind,tcref,baseValOpt,declaredTyconTypars,members,m,newslotsOK)) -> dprintf "members: %d\n" (List.length members));
    try
      (* Some preliminary checks *)
      membersl |> List.iter (fun (TyconMemberData(declKind,tcref,baseValOpt,declaredTyconTypars,members,m,newslotsOK)) -> 
             let tcaug = tcref.TypeContents
             if tcaug.tcaug_closed && declKind <> ExtrinsicExtensionBinding then 
               error(InternalError("Intrinsic augmentations of types are only permitted in the same file as the definition of the type",m));
             members |> List.iter (function 
                    | ClassMemberDefn_member_binding _ -> ()
                    | ClassMemberDefn_interface _ -> () 
                    | ClassMemberDefn_open _ 
                    | ClassMemberDefn_let_bindings _  (* accept local definitions *)
                    | ClassMemberDefn_implicit_ctor _ (* accept implicit ctor pattern, should be first! *)
                    | ClassMemberDefn_implicit_inherit _ when newslotsOK = NewSlotsOK -> () (* accept implicit ctor pattern, should be first! *)
                    (* The follow should have been removed by splitting, they belong to "core" (they are "shape" of type, not implementation) *)
                    | ClassMemberDefn_open (_,m) 
                    | ClassMemberDefn_let_bindings(_,_,_,m) 
                    | ClassMemberDefn_implicit_ctor(_,_,_,_,m)
                    | ClassMemberDefn_implicit_inherit(_,_,_,m) 
                    | ClassMemberDefn_slotsig(_,_,m)
                    | ClassMemberDefn_inherit(_,_,m)
                    | ClassMemberDefn_field(_,m)
                    | ClassMemberDefn_tycon(_,_,m) -> error(Error("This declaration element is not permitted in an augmentation",m))));

      let tyconBindingsOfTypeDefn (TyconMemberData(declKind,tcref,baseValOpt,declaredTyconTypars,members,m,newslotsOK)) =
          let containerInfo = ContainerInfo(parent,Some(tcref,None,baseValOpt,declaredTyconTypars))
          members 
          |> List.choose (fun memb ->
              match memb with 
              | ClassMemberDefn_implicit_ctor(_,_,_,_,m)
              | ClassMemberDefn_implicit_inherit(_,_,_,m) 
              | ClassMemberDefn_let_bindings(_,_,_,m) 
              | ClassMemberDefn_member_binding(_,m) 
              | ClassMemberDefn_open (_,m) 
                  -> Some(TyconBindingDefn(containerInfo,newslotsOK,declKind,memb,m))

              (* Interfaces exist in the member list - handled above in interfaceMembersFromTypeDefn *)
              | ClassMemberDefn_interface _  -> None

              (* The following should have been List.unzip out already in SplitTyconDefn *)
              | ClassMemberDefn_slotsig (_,_,m) 
              | ClassMemberDefn_field (_,m)             
              | ClassMemberDefn_inherit (_,_,m)    -> error(InternalError("Unexpected declaration element",m))
              | ClassMemberDefn_tycon (_,_,m)      -> error(Error("Types may not contain nested type definitions",m)))
          
      let binds  = 
          membersl |> List.map (fun (TyconMemberData(_,tcref,_,_,_,_,_) as tyconMemberData) -> 
              let obinds = tyconBindingsOfTypeDefn tyconMemberData
              let ibinds  = 
                      let intfTypes = interfacesFromTypeDefn tyconMemberData
                      let slotImplSets = DispatchSlotChecking.GetSlotImplSets cenv.infoReader env.DisplayEnv false (List.map (fun (ity,_,m) -> (ity,m)) intfTypes)
                      List.concat (List.map2 (interfaceMembersFromTypeDefn tyconMemberData) intfTypes slotImplSets)
              if verboseCC then dprintf "obinds: %d\n" (List.length obinds);
              if verboseCC then dprintf "ibinds: %d\n" (List.length ibinds);
              TyconBindingDefns(tcref, obinds @ ibinds))
      
      let results = TyconBindingChecking.TcTyconBindings cenv env tpenv bindsm scopem binds
      let binds,envbody,tpenv = results
      binds,envbody

    with e -> errorRecovery e scopem; [], env

//-------------------------------------------------------------------------
// Bind exception definitions
//------------------------------------------------------------------------- 

let tcaug_has_nominal_interface g tcaug tcref =
    tcaug.tcaug_implements |> List.exists (fun (x,_,_) -> 
        is_stripped_tyapp_typ g x && tcref_eq g (tcref_of_stripped_typ g x) tcref)

let AddGenericCompareDeclarations cenv (env:tcEnv) (tycon:Tycon) =
    if Augment.TyconIsAugmentedWithCompare cenv.g tycon then 
        let tcref = mk_local_tcref tycon
        let tcaug = tycon.TypeContents
        let m = tycon.Range
        let ty = if tycon.IsExceptionDecl then [], cenv.g.exn_ty else generalize_tcref tcref
        //let genericIComparableTy = mk_tyapp_ty cenv.g.system_GenericIComparable_tcref [ty]

        let hasExplicitIComparable = tcaug_has_interface cenv.g tcaug cenv.g.mk_IComparable_ty 
        let hasExplicitGenericIComparable = tcaug_has_nominal_interface cenv.g tcaug cenv.g.system_GenericIComparable_tcref    
        let hasExplicitIStructuralComparable = tcaug_has_interface cenv.g tcaug cenv.g.mk_IStructuralComparable_ty

        if hasExplicitIComparable then 
            errorR(Error("The struct, record or union type '"^tycon.DisplayName^"' implements the interface 'System.IComparable' explicitly. You must apply the '[<StructuralComparison(false)>]' attribute to the type",m)); 
  
        elif hasExplicitGenericIComparable then 
            errorR(Error("The struct, record or union type '"^tycon.DisplayName^"' implements the interface 'System.IComparable<_>' explicitly. You must apply the '[<StructuralComparison(false)>]' attribute to the type, and should also provide a consistent implementation of the non-generic interface System.IComparable",m)); 
        elif hasExplicitIStructuralComparable then
            errorR(Error("The struct, record or union type '"^tycon.DisplayName^"' implements the interface 'System.IStructuralComparable' explicitly. Apply the '[<StructuralComparison(false)>]' attribute to the type",m)); 
        else
            //let hasExplicitGenericIComparable = tcaug_has_interface cenv.g tcaug genericIComparableTy
            let cvspec1,cvspec2 = Augment.MakeValsForCompareAugmentation cenv.g tcref
            let cvspec3 = Augment.MakeValsForCompareWithComparerAugmentation cenv.g tcref

            PublishInterface cenv env.DisplayEnv tcref m true cenv.g.mk_IStructuralComparable_ty;
            PublishInterface cenv env.DisplayEnv tcref m true cenv.g.mk_IComparable_ty;
            //if not tycon.IsExceptionDecl && not hasExplicitGenericIComparable then 
            //    PublishInterface cenv env.DisplayEnv tcref m true genericIComparableTy;
            set_tcaug_compare tcaug (mk_local_vref cvspec1, mk_local_vref cvspec2);
            set_tcaug_compare_withc tcaug (mk_local_vref cvspec3);
            PublishValueDefn cenv env ModuleOrMemberBinding cvspec1
            PublishValueDefn cenv env ModuleOrMemberBinding cvspec2
            PublishValueDefn cenv env ModuleOrMemberBinding cvspec3

           
        

let AddGenericEqualityWithComparerDeclarations cenv (env:tcEnv) (tycon:Tycon) =
    if Augment.TyconIsAugmentedWithEquals cenv.g tycon then 
        let tcref = mk_local_tcref tycon
        let tcaug = tycon.TypeContents
        let m = tycon.Range

        let hasExplicitIStructuralEquatable = tcaug_has_interface cenv.g tcaug cenv.g.mk_IStructuralEquatable_ty
        let hasExplicitGenericIEquatable = tcaug_has_nominal_interface cenv.g tcaug cenv.g.system_GenericIEquatable_tcref

        // This type gets defined in prim-types, before we can add attributes to F# type definitions
        let isUnit = cenv.g.compilingFslib && tycon.DisplayName = "Unit"
        
        if hasExplicitIStructuralEquatable then
            errorR(Error("The struct, record or union type '"^tycon.DisplayName^"' implements the interface 'System.IStructuralEquatable' explicitly. Apply the '[<StructuralEquality(false)>]' attribute to the type",m)); 
        elif hasExplicitGenericIEquatable then 
            errorR(Error("The struct, record or union type '"^tycon.DisplayName^"' implements the interface 'System.IEquatable<_>' explicitly. You must apply the '[<StructuralEquality(false)>]' attribute to the type, and should also provide a consistent implementation of the non-generic override 'System.Object.Equals(obj)'",m)); 
        else
            let evspec1,evspec2 = Augment.MakeValsForEqualityWithComparerAugmentation cenv.g tcref
            PublishInterface cenv env.DisplayEnv tcref m true cenv.g.mk_IStructuralEquatable_ty;
            set_tcaug_hash_and_equals_withc tcaug (mk_local_vref evspec1, mk_local_vref evspec2)
            PublishValueDefn cenv env ModuleOrMemberBinding evspec1
            PublishValueDefn cenv env ModuleOrMemberBinding evspec2

            
let AddGenericCompareBindings cenv (env:tcEnv) (tycon:Tycon) =
    if Augment.TyconIsAugmentedWithCompare cenv.g tycon && isSome tycon.TypeContents.tcaug_compare then 
        Augment.MakeBindingsForCompareAugmentation cenv.g tycon
    else
        []
        
let AddGenericCompareWithComparerBindings cenv (env:tcEnv) (tycon:Tycon) =
    if Augment.TyconIsAugmentedWithCompare cenv.g tycon && isSome tycon.TypeContents.tcaug_compare_withc  then
         (Augment.MakeBindingsForCompareWithComparerAugmentation cenv.g tycon)
     else
        []
         
let AddGenericEqualityWithComparerBindings cenv (env:tcEnv) (tycon:Tycon) =
    if Augment.TyconIsAugmentedWithEquals cenv.g tycon  && isSome tycon.TypeContents.tcaug_hash_and_equals_withc then
        (Augment.MakeBindingsForEqualityWithComparerAugmentation cenv.g tycon)
    else
        []

let AddGenericHashAndComparisonDeclarations cenv env tycon =
    AddGenericCompareDeclarations cenv env tycon
    AddGenericEqualityWithComparerDeclarations cenv env tycon


let AddGenericHashAndComparisonBindings cenv env tycon =
    AddGenericCompareBindings cenv env tycon @ AddGenericCompareWithComparerBindings cenv env tycon @ AddGenericEqualityWithComparerBindings cenv env tycon


// We can only add the Equals override after we've done the augmentation becuase we have to wait until tcaug_has_override can give correct results 
let AddGenericEqualityBindings cenv env tycon =
    if Augment.TyconIsAugmentedWithEquals cenv.g tycon then 
        let tcref = mk_local_tcref tycon
        let tcaug = tycon.TypeContents
        // Note: tcaug_has_override only gives correct results after we've done the type augmentation 
        let hasExplicitObjectEqualsOverride = tcaug_has_override cenv.g tcaug "Equals" [cenv.g.obj_ty]

        // Note: only provide the equals method if IComparable not implemented explicitly 
        // Prior to F# 1.9.3.14 you only had to implement 'System.IComparable' to customize structural comparison AND equality on F# types 
        // Post 1.9.3.14 this is insufficient: you must override Equals as well. For compat we currently 
        // give a warning in this situation (see ilxgen.ml) and use the IComparable implementation instead 
        if not (Augment.TyconIsAugmentedWithCompare cenv.g tycon  && isNone tcaug.tcaug_compare) &&
           not hasExplicitObjectEqualsOverride then 

             let vspec1,vspec2 = Augment.MakeValsForEqualsAugmentation cenv.g tcref
             set_tcaug_equals tcaug (mk_local_vref vspec1, mk_local_vref vspec2);
             PublishValueDefn cenv env ModuleOrMemberBinding vspec1;
             PublishValueDefn cenv env ModuleOrMemberBinding vspec2;
             Augment.MakeBindingsForEqualsAugmentation cenv.g tycon
        else []
    else []

//-------------------------------------------------------------------------
// Bind exception definitions
//------------------------------------------------------------------------- 

let CheckForDuplicateConcreteType cenv env nm m  = 
    let curr = GetCurrAccumulatedModuleOrNamespaceType env
    if Map.mem nm curr.AllEntities then 
        (* Use 'error' instead of 'errorR' here to avoid cascading errors - see bug 1177 in FSharp 1.0 *)
        error (Duplicate("type, exception or module",nm,m))

let CheckForDuplicateModule cenv env nm m  = 
    let curr = GetCurrAccumulatedModuleOrNamespaceType env
    if curr.ModulesAndNamespacesByDemangledName.ContainsKey(nm) then 
        errorR (Duplicate("type or module",nm,m))

let TcExnDefnCore cenv env parent tpenv (ExconCore(attrs,UnionCase(_,id,args,_,_,_),repr,doc,vis,m), scopem) =
    let attrs' = TcAttributes cenv env attrTgtExnDecl attrs
    let args = match args with (UnionCaseFields args) -> args | _ -> error(Error("Explicit type specifications may not be used for exception constructors",m))
    let ad = AccessRightsOfEnv env
    
    let args' = List.mapi (fun i fdef -> TcAnonFieldDecl cenv env parent tpenv ("Data"^string i) fdef) args
    if not (String.isUpper id.idText) then errorR(NotUpperCaseConstructor(m));
    let vis,cpath = ComputeAccessAndCompPath env None m vis parent
    let vis = CombineReprAccess parent vis
    let exnc = 
      match repr with 
      | Some lid ->
          (* REVIEW: permit type arguments in this lid. *)
          begin match ResolveExprLongIdent cenv.nameResolver m ad env.eNameResEnv DefaultTypeNameResInfo lid with
          | Item_ecref exnc, [] -> 
              CheckTyconAccessible m (AccessRightsOfEnv env) exnc |> ignore;
              if List.length args' <> 0 then 
                errorR (Error("Exception abbreviations should not have argument lists",m));
              NewExn cpath id vis (TExnAbbrevRepr exnc) attrs' (doc.ToXmlDoc())
          | Item_ctor_group(_,meths) , [] -> 
              (* REVIEW: check this really is an exception type *)
              match args' with 
              | [] -> ()
              | _ -> error (Error("Abbreviations for .NET exceptions may not take arguments",m));
              let candidates = 
                  meths |> List.filter (fun minfo -> 
                      minfo.NumArgs = [args'.Length] &&
                      (* & (List.length args' <> 1 or  typ_of_param (List.hd md.mdParams) = Il.typ_String) *) 
                      minfo.GenericArity = 0) 
              match candidates with 
              | [minfo] -> 
                  let err() = 
                      Error("Exception abbreviations must refer to existing exceptions or F# types deriving from System.Exception",m)
                  if not (type_definitely_subsumes_type_no_coercion 0 cenv.g cenv.amap m cenv.g.exn_ty minfo.EnclosingType) then 
                    errorR(err());
                  let tref = 
                      match minfo with 
                      | ILMeth(_,minfo) -> minfo.ILTypeRef
                      | FSMeth _ -> 
                          match (tcref_of_stripped_typ cenv.g minfo.EnclosingType).CompiledRepresentation with 
                          | TyrepNamed (tref,_) -> tref
                          | _ -> 
                              error (err()) 
                      | _ -> error (err()) 
                  NewExn  cpath id vis (TExnAsmRepr tref) attrs' (doc.ToXmlDoc())
              | _ -> 
                  error (Error("Abbreviations for .NET exception types must have a matching object constructor",m))
          | _ ->
              error (Error("not an exception",m))
          end
      | None -> 
         NewExn cpath id vis (TExnFresh (MakeRecdFieldsTable args')) attrs' (doc.ToXmlDoc())

    let tcaug = exnc.TypeContents
    tcaug.tcaug_super <- Some cenv.g.exn_ty;

    CheckForDuplicateConcreteType cenv env (id.idText ^ "Exception") id.idRange;
    CheckForDuplicateConcreteType cenv env id.idText id.idRange;
    PublishTypeDefn cenv env exnc;
    (* let env = AddLocalExnDefn scopem exnc (AddLocalTycons g amap scopem [exnc] env) in  *)
    (* Augment the exception constructor with comparison and hash methods if needed *)
    AddGenericHashAndComparisonDeclarations cenv env exnc
    let binds = 
      match exnc.ExceptionInfo with 
      | TExnAbbrevRepr _ | TExnNone | TExnAsmRepr _ -> []
      | TExnFresh _ -> AddGenericHashAndComparisonBindings cenv env exnc
    binds,
    exnc,
    AddLocalExnDefn scopem exnc (AddLocalTycons cenv.g cenv.amap scopem [exnc] env)

let TcExnDefn cenv env parent tpenv (ExconDefn(core,aug,m),scopem) = 
    let binds1,exnc,env = TcExnDefnCore cenv env parent tpenv (core,scopem)
    let binds2,env = TcTyconMemberDefns cenv env parent m scopem [TyconMemberData(ModuleOrMemberBinding ,(mk_local_ecref exnc),None,[],aug,m,NoNewSlots)]
    (* Augment types with references to values that implement the pre-baked semantics of the type *)
    let binds3 = AddGenericEqualityBindings cenv env exnc
    binds1 @ binds2 @ binds3,exnc,env

let TcExnSignature cenv env parent tpenv (ExconSpfn(core,aug,m),scopem) = 
    let binds,exnc,env = TcExnDefnCore cenv env parent tpenv (core,scopem)
    let ecref = mk_local_ecref exnc
    let vals,_ = TcTyconMemberSpecs NoNewSlots cenv env (ContainerInfo(parent,Some(ecref,None,None,[]))) ModuleOrMemberBinding ecref tpenv aug
    binds,vals,ecref,env


let ComputeIsModule kind parms constraints im = 
    if nonNil parms || nonNil constraints then 
        error(Error("Unexpected constraints or parameters on module specification",im))
    match kind with 
     | TMK_Module -> true 
     | TMK_Namespace -> false 
     | TMK_Tycon -> error(Error("Unexpected constraint or type definition",im))

let ComputeModuleName (longPath: ident list) = 
    if longPath.Length <> 1 then error(Error("invalid module name",(List.hd longPath).idRange));
    List.hd longPath 

///-------------------------------------------------------------------------
/// Bind type definitions
///
/// We first establish the cores of a set of type definitions (i.e. everything
/// about the type definitions that doesn't involve values or expressions)
///
/// This is a non-trivial multi-phase algorithm. The technique used
/// is to gradually "fill in" the fields of the type constructors. 
///
/// This use of mutation is very problematic. This has many dangers, 
/// since the process of filling in the fields
/// involves creating, traversing and analyzing types that may recursively
/// refer to the types being defined. However a functional version of this
/// would need to re-implement certain type relations to work over a 
/// partial representation of types.
///------------------------------------------------------------------------- 
module EstablishTypeDefinitionCores = begin
 
    let private ComputeTyconName (longPath: ident list) doErase (typars:Typar list) = 
        if longPath.Length <> 1 then error(Error("invalid type extension",longPath.Head.idRange));
        let id = List.hd longPath
        let erasedArity = 
            if doErase then 
                List.foldBack (fun (tp:Typar) n -> if tp.IsErased then n else n+1) typars 0 
            else typars.Length
        mksyn_id id.idRange (if erasedArity = 0 then id.idText else id.idText ^ "`" ^string erasedArity)
 
    let private GetTyconAttribs g attrs = 
        let hasClassAttr         = HasAttrib g g.attrib_ClassAttribute attrs
        let hasAbstractClassAttr = HasAttrib g g.attrib_AbstractClassAttribute attrs
        let hasInterfaceAttr     = HasAttrib g g.attrib_InterfaceAttribute attrs
        let hasStructAttr        = HasAttrib g g.attrib_StructAttribute attrs
        let hasMeasureAttr       = HasAttrib g g.attrib_MeasureAttribute attrs
        (hasClassAttr,hasAbstractClassAttr,hasInterfaceAttr,hasStructAttr,hasMeasureAttr)

    //-------------------------------------------------------------------------
    // Type kind inference 
    //------------------------------------------------------------------------- 
       
    let private InferTyconKind g (kind,attrs',slotsigs,fields,inSig ,isConcrete,m) =
        let (hasClassAttr,hasAbstractClassAttr,hasInterfaceAttr,hasStructAttr,hasMeasureAttr) = GetTyconAttribs g attrs'
        let bi b = (if b then 1 else 0)
        if (bi hasClassAttr + bi hasInterfaceAttr + bi hasStructAttr + bi hasMeasureAttr) > 1 ||
           (bi hasAbstractClassAttr + bi hasInterfaceAttr + bi hasStructAttr + bi hasMeasureAttr) > 1 then
           error(Error("The attributes of this type specify multiple kinds for the type",m));
        
        match kind with 
        | TyconUnspecified ->
            if hasClassAttr || hasAbstractClassAttr || hasMeasureAttr then TyconClass        
            elif hasInterfaceAttr then TyconInterface
            elif hasStructAttr then TyconStruct
            elif isConcrete or nonNil(fields) (* or isNil(slotsigs) *)  then TyconClass
            elif isNil(slotsigs) && inSig  then TyconHiddenRepr
            else TyconInterface
        | k -> 
            if hasClassAttr && (k <> TyconClass) || 
               hasMeasureAttr && (k <> TyconClass && k <> TyconAbbrev && k <> TyconHiddenRepr) || 
               hasInterfaceAttr && (k <> TyconInterface) || 
               hasStructAttr && (k <> TyconStruct) then 
                error(Error("The kind of the type specified by its attributes does not match the kind implied by its definition",m));
            k


    // Establish 'type <vis1> C < T1... TN >  = <vis2> ...' including 
    //    - computing the mangled name for C
    // but 
    //    - we don't yet 'properly' establish constraints on type parameters
    let private TcTyconDefnCore_Phase0_BuildInitialTycon cenv env parent (synTyconInfo,synTyconRepr,_) = 
        let (ComponentInfo(attrs,kind,typars, cs,id,doc,preferPostfix, vis,_)) = synTyconInfo
        let typars' = TcTyparDecls cenv env typars
        id |> List.iter CheckNamespaceModuleOrTypeName;
        let id = ComputeTyconName id (match synTyconRepr with TyconCore_abbrev _ -> false | _ -> true) typars'

        // Augmentations of type definitions are allowed within the same file as long as no new type representation or abbreviation is given 
        CheckForDuplicateConcreteType cenv env id.idText id.idRange;
        CheckForDuplicateModule cenv env id.idText id.idRange;
        let vis,cpath = ComputeAccessAndCompPath env None id.idRange vis parent

        // Establish the visibility of the representation, e.g.
        //   type R = 
        //      private { f:int }
        //      member x.P = x.f + x.f
        let visOfRepr = 
            match synTyconRepr with 
            | TyconCore_no_repr _ -> None
            | TyconCore_abbrev _ -> None
            | TyconCore_union (vis,_,_) -> vis
            | TyconCore_asm _ -> None
            | TyconCore_recd (vis,_,_) -> vis
            | TyconCore_general _ -> None
            | TyconCore_enum _ -> None
         
        let visOfRepr,_ = ComputeAccessAndCompPath env None id.idRange visOfRepr parent
        let visOfRepr = combineAccess vis visOfRepr 
        // REVIEW: nested values, types and modules 
        let lmtyp = notlazy (empty_mtype FSharpModule)
        NewTycon cpath (id.idText,id.idRange) vis visOfRepr KindType (LazyWithContext.NotLazy typars') (doc.ToXmlDoc()) preferPostfix lmtyp


    //-------------------------------------------------------------------------
    /// Establishing type definitions: early phase: work out the basic kind of the type definition
    ///
    ///    On entry: the Tycon for the type definition has been created but many of its fields are not
    ///              yet filled in.
    ///    On exit: the entity_tycon_repr field of the tycon has been filled in with a dummy value that
    ///             indicates the kind of the type constructor
    /// Also, some adhoc checks are made.
    ///
    ///  synTyconInfo: Syntactic AST for the name, attributes etc. of the type constructor
    ///  synTyconRepr: Syntactic AST for the RHS of the type definition
    let private TcTyconDefnCore_Phase1_EstablishBasicKind cenv inSig envinner (synTyconInfo,synTyconRepr,_) (tycon:Tycon) = 
        let (ComponentInfo(attrs,_,typars, cs,id, _, _,_,_)) = synTyconInfo
        let m = tycon.Range
        let id = tycon.Id
        // 'Check' the attributes. We end up discarding the results from this 
        // particular check since we re-check them in all other phases. 
        let attrs' = TcAttributes cenv envinner attrTgtTyconDecl attrs
        let hasMeasureAttr = HasAttrib cenv.g cenv.g.attrib_MeasureAttribute attrs'
        let hasMeasureableAttr = HasAttrib cenv.g cenv.g.attrib_MeasureableAttribute attrs'

        if hasMeasureAttr then 
            tycon.Data.entity_kind <- KindMeasure;
            if nonNil typars then error(Error("Measure definitions may not have type parameters",m));

        let repr = 
            match synTyconRepr with 
            | TyconCore_no_repr m -> 
                // Run InferTyconKind to raise errors on inconsistent attribute sets
                InferTyconKind cenv.g (TyconHiddenRepr,attrs',[],[],inSig,true,m)  |> ignore
                if not inSig && not hasMeasureAttr then 
                    errorR(Error("This type requires a definition",m));
                if hasMeasureAttr
                then Some(TFsObjModelRepr { fsobjmodel_kind=TTyconClass; 
                                            fsobjmodel_vslots=[];
                                            fsobjmodel_rfields=MakeRecdFieldsTable [] })
                else None

            | TyconCore_abbrev(eq,_) -> 
                // Run InferTyconKind to raise errors on inconsistent attribute sets
                InferTyconKind cenv.g (TyconAbbrev,attrs',[],[],inSig,true,m) |> ignore
                None

            | TyconCore_union (_,_,m) -> 
                // Run InferTyconKind to raise errors on inconsistent attribute sets
                InferTyconKind cenv.g (TyconUnion,attrs',[],[],inSig,true,m) |> ignore
                // Note: the table of union cases is initially empty
                Some (MakeUnionRepr [])

            | TyconCore_asm (s,m) -> 
                // Run InferTyconKind to raise errors on inconsistent attribute sets
                InferTyconKind cenv.g (TyconILAssemblyCode,attrs',[],[],inSig,true,m) |> ignore
                Some (TAsmRepr s)

            | TyconCore_recd (_,_,m) -> 
                // Run InferTyconKind to raise errors on inconsistent attribute sets
                InferTyconKind cenv.g (TyconRecord,attrs',[],[],inSig,true,m) |> ignore
                // Note: the table of record fields is initially empty
                Some (TRecdRepr (MakeRecdFieldsTable  []) )

            | TyconCore_general (kind,inherits,slotsigs,fields,isConcrete,isIncrClass,_) ->
                let kind = InferTyconKind cenv.g (kind,attrs',slotsigs,fields,inSig,isConcrete,m)
                match kind with 
                | TyconHiddenRepr -> 
                    None
                | _ -> 
                    let kind = 
                        match kind with
                        | TyconClass               -> TTyconClass
                        | TyconInterface           -> TTyconInterface
                        | TyconDelegate (ty,arity) -> TTyconDelegate (mk_slotsig("Invoke",cenv.g.unit_ty,[],[],[], None))
                        | TyconStruct              -> TTyconStruct 
                        | _ -> error(InternalError("should have inferred tycon kind",m))
                    let repr = { fsobjmodel_kind=kind; 
                                 fsobjmodel_vslots=[];
                                 fsobjmodel_rfields=MakeRecdFieldsTable [] }
                    Some(TFsObjModelRepr repr)

            | TyconCore_enum (decls,m) -> 
                let kind = TTyconEnum
                let repr = { fsobjmodel_kind=kind; 
                             fsobjmodel_vslots=[];
                             fsobjmodel_rfields=MakeRecdFieldsTable [] }
                Some(TFsObjModelRepr repr)

        // OK, now fill in the (partially computed) type representation
        tycon.Data.entity_tycon_repr <- repr

    /// Establish any type abbreviations
    ///
    /// e.g. for  
    ///    type B<'a when 'a :  C> = DDD of C
    ///    and  C = B<int>
    ///
    /// we establish
    ///
    ///   Entity('B) 
    ///       TypeAbbrev = TType_app(Entity('int'),[])
    ///
    /// and for
    ///
    ///    type C = B
    ///
    /// we establish
    ///       TypeAbbrev = TType_app(Entity('B'),[])
    ///
    /// Note that for 
    ///              type PairOfInts = int * int
    /// then after running this phase and checking for cycles, operations 
    // such as 'is_tuple_typ' will return reliable results, e.g. is_tuple_typ on the 
    /// TAST type for 'PairOfInts' will report 'true' 
    //
    let private TcTyconDefnCore_Phase2_Phase4_EstablishAbbreviations cenv envinner tpenv inSig pass (tinfo,synTyconRepr,_) (tycon:Tycon ) =
        let m = tycon.Range
        let checkCxs = if (pass = SecondPass) then CheckCxs else NoCheckCxs
        let firstPass = (pass = FirstPass)
        try 
            let (ComponentInfo(attrs,compKind,typars, cs,id, _, _,_,_)) = tinfo
            let id = tycon.Id
            let thisTyconRef = mk_local_tcref tycon
            let attrs' = TcAttributes cenv envinner attrTgtTyconDecl attrs

            let hasMeasureAttr = HasAttrib cenv.g cenv.g.attrib_MeasureAttribute attrs'
            let hasMeasureableAttr = HasAttrib cenv.g cenv.g.attrib_MeasureableAttribute attrs'

            let envinner = AddDeclaredTypars CheckForDuplicateTypars (tycon.Typars(m)) envinner
            let envinner = MkInnerEnvForTyconRef cenv envinner thisTyconRef false

            match synTyconRepr with 

            // This unfortunate case deals with "type x = A" 
            // In F# this only defines a new type if A is not in scope 
            // as a type constructor, or if the form type A = A is used. 
            // "type x = | A" can always be used instead. 
            | TyconCore_abbrev(Type_lid([tcn],_),_) 
                          when 
                            (not hasMeasureAttr && 
                             (isNil (LookupTypeNameInEnvNoArity tcn.idText envinner.eNameResEnv) || 
                              id.idText = tcn.idText)) -> ()
            
            | TyconCore_abbrev(eq,_) ->
                /// This case deals with the "Measurable" attribute, which does not introduce a true abbreviation
                if not hasMeasureableAttr then 
                    let kind = if hasMeasureAttr then KindMeasure else KindType
                    let ty,_ = TcTypeOrMeasureAndRecover (Some kind) cenv NoNewTypars checkCxs envinner tpenv eq

                    if not firstPass then 
                        let ftyvs = free_in_type_lr cenv.g false ty 
                        let typars = tycon.Typars(m)
                        if ftyvs.Length <> typars.Length then 
                            warning(Deprecated("This type abbreviation has one or more declared type parameters that do not appear in the type being abbreviated. Type abbreviations must use all declared type parameters in the type being abbreviated. Consider removing one or more type parameters, or use a concrete type definition that wraps an underlying type, such as 'type C<'a> = C of ...'",tycon.Range))
                        //elif not ((ftyvs,typars) ||> List.for_all2 typar_ref_eq) then 
                        //    warning(Deprecated("The declared type parameters of this type abbreviation are not declared in the same order they are used in the type being abbreviated. Consider reordering the type parameters, or use a concrete type definition that wraps an underlying type, such as 'type C<'a,'b> = C of ...'",tycon.Range))

                    if firstPass then
                        tycon.Data.entity_tycon_abbrev <- Some ty

            | _ -> ()
        
        with e -> 
            errorRecovery e m

    // Third phase: check and publish the supr types. Run twice, once before constraints are established
    // and once after
    let private TcTyconDefnCore_Phase3_Phase5_EstablishSuperTypesAndInterfaceTypes cenv envinner tpenv inSig tdefs (tycons:Tycon list) pass = 
        let checkCxs = if (pass = SecondPass) then CheckCxs else NoCheckCxs
        let firstPass = (pass = FirstPass)

        // Publish the immediately declared interfaces. 
        let implementsL = 
            (tdefs,tycons) ||> List.map2 (fun (synTyconInfo,synTyconRepr,explicitImplements) tycon -> 
                let (ComponentInfo(attrs,_,_, cs,id, _, _,_,_)) = synTyconInfo
                let m = tycon.Range
                let tcref = mk_local_tcref tycon
                let envinner = AddDeclaredTypars CheckForDuplicateTypars (tycon.Typars(m)) envinner
                let envinner = MkInnerEnvForTyconRef cenv envinner tcref false
                
                let implementedTys,_ = List.mapfold (map_acc_fst (TcTypeAndRecover cenv NoNewTypars checkCxs envinner)) tpenv explicitImplements

                // Review: should skip checking constraints while checking attributes on first pass, though it's hard to imagine when that would matter 
                let attrs' = TcAttributes cenv envinner attrTgtTyconDecl attrs

                if firstPass then 
                    tycon.Data.entity_attribs <- attrs';

                let implementedTys,inheritedTys = 
                    match synTyconRepr with 
                    | TyconCore_general (kind,inherits,slotsigs,fields,isConcrete,isIncrClass,m) ->
                        let kind = InferTyconKind cenv.g (kind,attrs',slotsigs,fields,inSig,isConcrete,m)

                        let inherits = inherits |> List.map (fun (ty,m,baseIdOpt) -> (ty,m)) 
                        let inheritedTys = fst(List.mapfold (map_acc_fst (TcTypeAndRecover cenv NoNewTypars checkCxs envinner)) tpenv inherits)
                        let implementedTys,inheritedTys =   
                            if kind = TyconInterface then (implementedTys @ inheritedTys),[] 
                            else implementedTys,inheritedTys
                        implementedTys,inheritedTys 
                    | TyconCore_enum _ | TyconCore_no_repr _ | TyconCore_abbrev _
                    
                    | TyconCore_union _ | TyconCore_asm _ | TyconCore_recd _ -> 
                        // REVIEW: we could do the IComparable/IStructuralHash interface analysis here. 
                        // This would let the type satisfy more recursive IComparable/IStructuralHash constraints 
                        implementedTys,[]


                // Publish interfaces, but only on the first pass, to avoid a duplicate interface check 
                if firstPass then 
                    implementedTys |> List.iter (fun (ty,m) -> PublishInterface cenv envinner.DisplayEnv tcref m false ty) ;

                attrs',inheritedTys)

        // Publish the attributes and supertype  
        (implementsL,tdefs,tycons) |||> List.iter3 (fun (attrs',inheritedTys) (synTyconInfo,synTyconRepr,_) tycon -> 
          let m = tycon.Range
          try 
              if verbose then  dprintf "--> TcTyconDefnCores (representations of %s)@%a\n" tycon.MangledName output_range m;
              let (ComponentInfo(attrs,_,_, cs,id, _, _,_,_)) = synTyconInfo
              let thisTyconRef = mk_local_tcref tycon
              let envinner = AddDeclaredTypars CheckForDuplicateTypars (tycon.Typars(m)) envinner
              let envinner = MkInnerEnvForTyconRef cenv envinner thisTyconRef false
              let super = 
                  match synTyconRepr with 
                  | TyconCore_no_repr _ -> None
                  | TyconCore_abbrev _ -> None
                  | TyconCore_union _ -> None
                  | TyconCore_asm _ -> None
                  | TyconCore_recd _ -> None
                  | TyconCore_general (kind,_,slotsigs,fields,isConcrete,_,_) ->
                      let kind = InferTyconKind cenv.g (kind,attrs',slotsigs,fields,inSig,isConcrete,m)
                                           
                      match inheritedTys with 
                      | [] -> 
                          match kind with 
                          | TyconStruct -> Some(cenv.g.system_Value_typ)
                          | TyconDelegate _ -> Some(cenv.g.system_MulticastDelegate_typ )
                          | TyconHiddenRepr | TyconClass | TyconInterface -> None
                          | _ -> error(InternalError("should have inferred tycon kind",m)) 

                      | [(ty,m)] -> 
                          if not firstPass && kind <> TyconClass then 
                              errorR (Error("Structs, interfaces, enums and delegates may not inherit from other types",m)); 
                          CheckSuperType cenv ty m; 
                          if is_typar_typ cenv.g ty then 
                              if firstPass  then 
                                  errorR(Error("cannot inherit from a variable type",m)) 
                              Some cenv.g.obj_ty // a "super" that is a variable type causes grief later
                          else                          
                              Some ty 
                      | _ -> 
                          error(Error("Types may not inherit from multiple concrete types",m))

                  | TyconCore_enum _ -> 
                      Some(cenv.g.system_Enum_typ) 

              // Publish the super type
              tycon.TypeContents.tcaug_super <- super
              
           with e -> errorRecovery e m)

    /// Establish the fields, dispatch slots and union cases of a type
    let private TcTyconDefnCore_Phase6_EstablishRepresentation cenv envinner tpenv inSig (tinfo,synTyconRepr,_) (tycon:Tycon ) =
        let m = tycon.Range
        try 
            let (ComponentInfo(attrs,compKind,_, cs,id, _, _,_,_)) = tinfo
            let id = tycon.Id
            let thisTyconRef = mk_local_tcref tycon
            let innerParent = Parent(thisTyconRef)
            let thisTyInst,thisTy = generalize_tcref thisTyconRef
            let attrs' = TcAttributes cenv envinner attrTgtTyconDecl attrs


            let hasAbstractAttr = HasAttrib cenv.g cenv.g.attrib_AbstractClassAttribute attrs'
            let hasSealedAttr = TryFindBoolAttrib cenv.g cenv.g.attrib_SealedAttribute attrs'
            let hasMeasureAttr = HasAttrib cenv.g cenv.g.attrib_MeasureAttribute attrs'
            let hasMeasureableAttr = HasAttrib cenv.g cenv.g.attrib_MeasureableAttribute attrs'
            let hasStructLayoutAttr = HasAttrib cenv.g cenv.g.attrib_StructLayoutAttribute attrs'

            if hasAbstractAttr then 
                tycon.TypeContents.tcaug_abstract <- true;

            tycon.Data.entity_attribs <- attrs';
            let noAbstractClassAttributeCheck() = 
                if hasAbstractAttr then errorR (Error("Only classes may be given the 'AbstractClass' attribute",m))
                
            let structLayoutAttributeCheck(allowed) = 
                if hasStructLayoutAttr  then 
                    if allowed then 
                        warning(PossibleUnverifiableCode(m));
                    elif thisTyconRef.Typars(m).Length > 0 then 
                        errorR (Error("Generic types may not be given the 'StructLayout' attribute",m))
                    else
                        errorR (Error("Only structs and classes without implicit constructors may be given the 'StructLayout' attribute",m))
                
            let hiddenReprChecks(hasRepr) =
                 structLayoutAttributeCheck(false);
                 if hasSealedAttr = Some(false) || (hasRepr && hasSealedAttr <> Some(true) && not (id.idText = "Unit" && cenv.g.compilingFslib) ) then 
                    errorR(Error("The representation of this type is hidden by the signature. It must be given an attribute such as [<Sealed>], [<Class>] or [<Interface>] to indicate the characteristics of the type",m));
                 if hasAbstractAttr then 
                     errorR (Error("Only classes may be given the 'AbstractClass' attribute",m))

            let noMeasureAttributeCheck() = 
                if hasMeasureAttr then errorR (Error("Only types representing units-of-measure may be given the 'Measure' attribute",m))
                
            let noSealedAttributeCheck(k) = 
                if hasSealedAttr = Some(true) then errorR (Error(sprintf "%s types are always sealed" k,m));

            let noFieldsCheck(fields':RecdField list) = 
                match fields' with 
                | (rf :: _) -> errorR (Error("Interface types and delegate types may not contain fields",rf.Range))
                | _ -> ()

                
            let envinner = AddDeclaredTypars CheckForDuplicateTypars (tycon.Typars(m)) envinner
            let envinner = MkInnerEnvForTyconRef cenv envinner thisTyconRef false


            // Notify the Language Service about field names in record/class declaration
            let ad = AccessRightsOfEnv envinner
            let writeFakeRecordFieldsToSink (fields:RecdField list) =
                let nenv = nenv_of_tenv envinner
                // Record fields should be visible from IntelliSense, so add fake names for them (similarly to "let a = ..")
                for fspec in (fields |> List.filter (fun fspec -> not fspec.IsCompilerGenerated)) do
                    let info = RecdFieldInfo(thisTyInst, rfref_of_rfield thisTyconRef fspec)
                    let nenv' = AddFakeNameToNameEnv fspec.Name (Item_recdfield info) nenv
                    // Name resolution gives better info for tooltips
                    CallNameResolutionSink(fspec.Range,nenv,FreshenRecdFieldRef cenv.nameResolver m (rfref_of_rfield thisTyconRef fspec),ItemOccurence.Binding,nenv.eDisplayEnv,ad)
                    // Environment is needed for completions
                    CallEnvSink(fspec.Range, nenv', ad)

            // Notify the Language Service about constructors in discriminated union declaration
            let writeFakeUnionCtorsToSink unionCases = 
                let nenv = nenv_of_tenv envinner
                // Constructors should be visible from IntelliSense, so add fake names for them 
                for unionCase in unionCases do
                    let info = UnionCaseInfo(thisTyInst,mk_ucref thisTyconRef unionCase.ucase_id.idText)
                    let nenv' = AddFakeNameToNameEnv unionCase.ucase_id.idText (Item_ucase info) nenv
                    // Report to both - as in previous function
                    CallNameResolutionSink(unionCase.Range,nenv,Item_ucase info,ItemOccurence.Binding,nenv.eDisplayEnv,ad)
                    CallEnvSink(unionCase.ucase_id.idRange, nenv', ad)
            
            let theTypeRepresentation,baseValOpt = 
                match synTyconRepr with 

                | TyconCore_no_repr _ -> 
                    hiddenReprChecks(false)
                    if hasMeasureAttr
                    then Some(TFsObjModelRepr { fsobjmodel_kind=TTyconClass; 
                                                fsobjmodel_vslots=[];
                                                fsobjmodel_rfields= MakeRecdFieldsTable [] }), None
                    else None,None

                // This unfortunate case deals with "type x = A" 
                // In F# this only defines a new type if A is not in scope 
                // as a type constructor, or if the form type A = A is used. 
                // "type x = | A" can always be used instead. 
                | TyconCore_abbrev(Type_lid([tcn],_),_) 
                              when 
                                (not hasMeasureAttr && 
                                 (isNil (LookupTypeNameInEnvNoArity tcn.idText envinner.eNameResEnv) || 
                                  id.idText = tcn.idText)) -> 
                          
                    structLayoutAttributeCheck(false);
                    CheckUnionCaseName tcn.idText tcn.idRange;
                    let unionCase = NewUnionCase tcn tcn.idText [] thisTy [] emptyXmlDoc tycon.Accessibility
                    Some (MakeUnionRepr [ unionCase ]),None

                | TyconCore_abbrev(eq,_) ->
                    if hasSealedAttr = Some(true) then 
                        errorR (Error("Abbreviated types may not be given the 'Sealed' attribute",m));
                    noAbstractClassAttributeCheck();
                    if hasMeasureableAttr  then 
                        let kind = if hasMeasureAttr then KindMeasure else KindType
                        let theTypeAbbrev,_ = TcTypeOrMeasureAndRecover (Some kind) cenv NoNewTypars CheckCxs envinner tpenv eq

                        Some(TMeasureableRepr theTypeAbbrev),None
                    else 
                        None,None

                | TyconCore_union (_,unionCases,_) -> 
                    noMeasureAttributeCheck();
                    noSealedAttributeCheck "Discriminated union";
                    noAbstractClassAttributeCheck();
                    structLayoutAttributeCheck(false);
                    let unionCases = TcUnionCaseDecls cenv envinner innerParent thisTy tpenv unionCases
                    writeFakeUnionCtorsToSink unionCases
                    Some (MakeUnionRepr unionCases),None

                | TyconCore_recd (_,fields,_) -> 
                    noMeasureAttributeCheck();
                    noSealedAttributeCheck "Record";
                    noAbstractClassAttributeCheck();
                    structLayoutAttributeCheck(true);  // these are allowed for records
                    let recdFields = TcNamedFieldDecls cenv envinner innerParent false tpenv fields
                    writeFakeRecordFieldsToSink recdFields
                    Some (TRecdRepr (MakeRecdFieldsTable recdFields))  ,None

                | TyconCore_asm (s,_) -> 
                    noMeasureAttributeCheck();
                    noSealedAttributeCheck "Assembly code";
                    structLayoutAttributeCheck(false);
                    noAbstractClassAttributeCheck();
                    Some (TAsmRepr s),None

                | TyconCore_general (kind,inherits,slotsigs,fields,isConcrete,isIncrClass,_) ->
                    let fields' = TcNamedFieldDecls cenv envinner innerParent isIncrClass tpenv fields
                    writeFakeRecordFieldsToSink fields'
                    
                    let superTy = tycon.TypeContents.tcaug_super
                    let typToSearchForAbstractMembers = ((match superTy with None -> cenv.g.obj_ty | Some ty -> ty),None)
                    let containerInfo = (TyconContainerInfo(innerParent,thisTyconRef,thisTyconRef.Typars(m)))
                    let kind = InferTyconKind cenv.g (kind,attrs',slotsigs,fields,inSig,isConcrete,m)
                    match kind with 
                    | TyconHiddenRepr  -> 
                        hiddenReprChecks(true)
                        None,None
                    | _ ->

                        // Note: for a mutually recursive set we can't check this condition 
                        // until "is_sealed_typ" and "is_class_typ" give reliable results. 
                        superTy |> Option.iter (fun ty -> 
                            let m = match inherits with | [] -> m | ((_,m,_) :: _) -> m
                            if is_sealed_typ cenv.g ty then 
                                errorR(Error("Cannot inherit a sealed type",m))
                            elif not (is_class_typ cenv.g ty) then 
                                errorR(Error("Cannot inherit from interface type. Use interface ... with instead",m)));

                        let kind = 
                            match kind with 
                              | TyconStruct -> 
                                  noSealedAttributeCheck "Struct";
                                  noAbstractClassAttributeCheck();
                                  if nonNil slotsigs then 
                                    errorR (Error("Struct types may not not contain abstract members",m)); 
                                  structLayoutAttributeCheck(true);

                                  TTyconStruct
                              | TyconInterface -> 
                                  if hasSealedAttr = Some(true) then errorR (Error("Interface types may not be sealed",m))
                                  structLayoutAttributeCheck(false);
                                  noAbstractClassAttributeCheck();
                                  noFieldsCheck(fields');
                                  TTyconInterface
                              | TyconClass -> 
                                  structLayoutAttributeCheck(not isIncrClass);
                                  TTyconClass
                              | TyconDelegate (ty,arity) -> 
                                  noSealedAttributeCheck "Delegate";
                                  structLayoutAttributeCheck(false);
                                  noAbstractClassAttributeCheck();
                                  noFieldsCheck(fields');
                                  let ty',_ = TcTypeAndRecover cenv NoNewTypars CheckCxs envinner tpenv ty
                                  let _,curriedArgInfos,returnTy,_ = GetTopValTypeInCompiledForm cenv.g (arity |> TranslateTopValSynInfo m (TcAttributes cenv envinner)  |> TranslatePartialArity []) ty' m
                                  if curriedArgInfos.Length < 1 then error(Error("Delegate specifications must be of the form 'typ -> typ'",m));
                                  if curriedArgInfos.Length > 1 then error(Error("Delegate specifications must not be curried types. Use 'typ * ... * typ -> typ' for multi-argument delegates, and 'typ -> (typ -> typ)' for delegates returning function values",m));
                                  let ttps = thisTyconRef.Typars(m)
                                  let fparams = curriedArgInfos.Head |> List.map mk_slotparam 
                                  TTyconDelegate (mk_slotsig("Invoke",thisTy,ttps,[],[fparams], returnTy))
                              | _ -> 
                                  error(InternalError("should have inferred tycon kind",m))

                        let baseIdOpt = 
                            match synTyconRepr with 
                            | TyconCore_no_repr _ -> None
                            | TyconCore_abbrev _ -> None
                            | TyconCore_union _ -> None
                            | TyconCore_asm _ -> None
                            | TyconCore_recd _ -> None
                            | TyconCore_enum _ -> None
                            | TyconCore_general (_,inherits,slotsigs,fields,isConcrete,isIncrClass,m) ->
                                match inherits with 
                                | [] -> None
                                | ((ty,m,baseIdOpt) :: _) -> 
                                    match baseIdOpt with 
                                    | None -> Some(ident("base",m)) 
                                    | Some id -> Some(id)
                            
                        let abstractSlots = 
                            [ for (valSpfn,memberFlags) in slotsigs do 
                                  CheckMemberFlags cenv.g None NewSlotsOK OverridesOK memberFlags m;
                                  let ((ValSpfn(attrs,id,_, _, valSynData, pseudo, mutableFlag,doc, vis,literalExprOpt,m))) = valSpfn 
                                  CheckForHiddenAbstractSlot (cenv,envinner,typToSearchForAbstractMembers,id,valSynData,memberFlags,m)
                                  
                                  let slots = fst (TcAndPublishValSpec (cenv,envinner,containerInfo,ModuleOrMemberBinding,Some(memberFlags),tpenv,valSpfn))
                                  // Multiple slots may be returned, e.g. for 
                                  //    abstract P : int with get,set
                                  
                                  for slot in slots do 
                                      yield mk_local_vref slot ]

                        let baseValOpt = MakeAndPublishBaseVal cenv envinner baseIdOpt (super_of_tycon cenv.g tycon)
                        let repr = 
                            TFsObjModelRepr 
                                { fsobjmodel_kind=kind; 
                                  fsobjmodel_vslots= abstractSlots;
                                  fsobjmodel_rfields=MakeRecdFieldsTable fields'}
                        Some(repr), baseValOpt
                | TyconCore_enum (decls,m) -> 
                    let fieldTy,fields' = TcEnumDecls cenv envinner innerParent thisTy decls
                    let kind = TTyconEnum
                    structLayoutAttributeCheck(false);
                    noSealedAttributeCheck "Enum";
                    let vfld = NewRecdField false None (ident("value__",m))  fieldTy false [] [] emptyXmlDoc taccessPublic true
                    
                    if not (ListSet.mem (type_equiv cenv.g) fieldTy [ cenv.g.int32_ty; cenv.g.int16_ty; cenv.g.sbyte_ty; cenv.g.int64_ty; cenv.g.char_ty; cenv.g.bool_ty; cenv.g.uint32_ty; cenv.g.uint16_ty; cenv.g.byte_ty; cenv.g.uint64_ty ]) then 
                        errorR(Error("Literal enumerations must have type int, uint, int16, uint16, int64, uint64, byte, sbyte or char",m));

                    let nenv = nenv_of_tenv envinner
                    writeFakeRecordFieldsToSink fields' 
                    let repr = 
                        TFsObjModelRepr 
                            { fsobjmodel_kind=kind; 
                              fsobjmodel_vslots=[];
                              fsobjmodel_rfields= MakeRecdFieldsTable (vfld :: fields') }
                    Some(repr), None
            
            if verbose then  dprintf "--> EstablishTypeDefinitionCores (adjust representations)@%a\n" output_range m;
            tycon.Data.entity_tycon_repr <- theTypeRepresentation;
            baseValOpt
        with e -> 
            errorRecovery e m; None

    /// Check that a set of type definitions is free of cycles in abbreviations
    let private CheckForCyclicAbbreviations cenv tycons = 

        let edges (tycon:Tycon) =

            let insert tcref acc = 
                let tycon' = deref_tycon tcref
                if ListSet.mem (===) tycon' tycons  then (tycon,tycon') ::acc else acc
            let rec accInAbbrevType ty acc  = 
                match strip_tpeqns ty with 
                | TType_tuple l -> 
                    accInAbbrevTypes l acc
                
                // REVIEW: this algorithm is not yet correct for all cases 
                | TType_ucase (UCRef(tc,_),tinst) 
                | TType_app (tc,tinst) -> 
                    let tycon2 = (deref_tycon tc)
                    let acc = accInAbbrevTypes tinst  acc
                    // Record immediate recursive references 
                    if ListSet.mem (===) tycon2 tycons  then 
                        (tycon,tycon2) ::acc 
                    // Expand the representation of abbreviations 
                    elif tc.IsTypeAbbrev  then
                        accInAbbrevType (reduce_tcref_abbrev tc tinst) acc
                    // Otherwise H<inst> - explore the instantiation. 
                    else 
                        acc

                | TType_fun (d,r) -> 
                    accInAbbrevType d (accInAbbrevType r acc)
                
                | TType_var r -> acc
                
                | TType_forall (tps,r) -> accInAbbrevType r acc
                
                | TType_modul_bindings -> failwith "accInAbbrevType: naked unknown"

                | TType_measure ms -> accInMeasure ms acc
            and accInMeasure ms acc =
                match strip_upeqns ms with
                | MeasureCon tc when ListSet.mem (===) (deref_tycon tc) tycons  ->  
                    (tycon, (deref_tycon tc)) :: acc
                | MeasureCon tc when tc.IsTypeAbbrev  ->              
                    accInMeasure (reduce_tcref_abbrev_measure tc) acc
                | MeasureProd (ms1, ms2) -> accInMeasure ms1 (accInMeasure ms2 acc)
                | MeasureInv ms -> accInMeasure ms acc
                | _ -> acc

            and accInAbbrevTypes tys acc = 
                List.foldBack accInAbbrevType tys acc
                
            let acc = []
            let acc = 
                match tycon.TypeAbbrev with 
                | None -> acc
                | Some ty -> 
                    if not cenv.isSig && not cenv.haveSig && (tycon.Accessibility <> taccessPublic || tycon.TypeReprAccessibility <> taccessPublic) then 
                       errorR(Error("Type abbreviations must be public. If you want to use a private type abbreviation you must use an explicit signature",tycon.Range));
                    accInAbbrevType ty acc

            acc

        let graph = NodeGraph.mk_graph (stamp_of_tycon,Int64.order) tycons (List.map (fun (tc1:Tycon,tc2:Tycon) -> tc1.Stamp, tc2.Stamp) (List.collect edges tycons))
        graph |> NodeGraph.iterate_cycles (fun path -> 
            let tycon = path.Head 
            // The thing is cyclic. Set the abbreviation and representation to be "None" to stop later VS crashes
            tycon.Data.entity_tycon_abbrev <- None
            tycon.Data.entity_tycon_repr <- None 
            errorR(Error("This type definition involves an immediate cyclic reference through an abbreviation",tycon.Range)));


    /// Check that a set of type definitions is free of inheritance cycles
    let CheckForCyclicStructsAndInheritance cenv tycons = 
        let edges (tycon:Tycon) =

            let insert tcref acc = 
                let tycon' = deref_tycon tcref
                if ListSet.mem (===) tycon' tycons  then (tycon,tycon') ::acc else acc

            /// Analyze a type used as a struct field, perhaps in a nested struct
            let rec accStructField structTycon structTyInst fspec acc  = 
                let fieldTy = actual_typ_of_rfield structTycon structTyInst fspec
                accStructFieldType structTycon structTyInst fspec fieldTy acc

            and accStructFieldType structTycon structTyInst fspec fieldTy acc  = 
                match strip_tpeqns fieldTy with 
                // Analyze nested structs
                | TType_app (tcref2 ,tinst2) when is_struct_tcref tcref2  ->
                    let tycon2 = (deref_tycon tcref2)
                    // Of course structs are not allowed to contain instance fields of their own type:
                    //  type S = struct { field x : S } 
                    //
                    // In addition, see bug 3429. In the .NET IL structs are allowed to contain 
                    // static fields of their exact generic type, e.g.
                    //  type S = struct { static field x : S } 
                    //  type S<T> = struct { static field x : S<T> } 
                    // but not
                    //  type S<T> = struct { static field x : S<int> } 
                    //  type S<T> = struct { static field x : S<T[]> } 
                    // etc.
                    //
                    // Ideally structs would allow static fields of any type. However
                    // rhis is a restriction and exemption that originally stems from 
                    // the way the Microsoft desktop CLR class loader works.
                    if fspec.IsStatic && 
                       (structTycon === tycon2) && 
                       (structTyInst,tinst2) ||> List.lengthsEqAndForall2 (fun ty1 ty2 -> is_typar_typ cenv.g ty1 && is_typar_typ cenv.g ty2 && typar_ref_eq (dest_typar_typ cenv.g ty1) (dest_typar_typ cenv.g ty2)) 
                    then
                       acc
                    elif ListSet.mem (===) tycon2 tycons  then 
                        let visited = ListAssoc.containsKey (===) tycon2 acc
                        if visited then 
                            acc
                        else
                            let acc = (tycon,tycon2) ::acc 
                            accStructFields tycon2 tinst2 acc
                    else
                        accStructFields tycon2 tinst2 acc
                // Analyze abbreviations used within struct fields
                | TType_app (tcref2 ,tinst2) when tcref2.IsTypeAbbrev  ->
                    accStructFieldType structTycon structTyInst fspec (reduce_tcref_abbrev tcref2 tinst2) acc
                | _ -> acc

            and accStructFields (structTycon:Tycon) tinst acc  = 
                // Accumulate over _all_ struct fields - see bug 3429
                (structTycon.AllFieldsAsList,acc) ||> List.foldBack (accStructField  structTycon tinst )

            let acc = []

            let acc = 
                if tycon.IsStructTycon then
                    let tinst,ty = generalize_tcref (mk_local_tcref tycon)
                    accStructFields tycon tinst acc
                else
                    acc

            let checkInsert ty acc = 
                if is_stripped_tyapp_typ cenv.g ty then // guard against possible earlier failure
                    insert (tcref_of_stripped_typ cenv.g ty) acc
                else
                    acc

            let acc =
                // Note: only the nominal type counts 
                let super = super_of_tycon cenv.g tycon
                checkInsert super acc
            let acc =
                // Note: only the nominal type counts 
                let interfaces = implements_of_tycon cenv.g tycon
                List.foldBack checkInsert interfaces acc
            acc
        let edges = (List.collect edges tycons)
        let graph = NodeGraph.mk_graph (stamp_of_tycon,Int64.order) tycons (List.map (fun (tc1:Tycon,tc2:Tycon) -> tc1.Stamp, tc2.Stamp) edges)
        graph |> NodeGraph.iterate_cycles (fun path -> 
            let tycon = path.Head 
            // The thing is cyclic. Set the abbreviation and representation to be "None" to stop later VS crashes
            tycon.Data.entity_tycon_abbrev <- None
            tycon.Data.entity_tycon_repr <- None 
            errorR(Error("This type definition involves an immediate cyclic reference through a struct field or inheritance relation",tycon.Range)));


        
    let TcTyconDefnCores cenv env inSig parent tpenv (tdefs,m,scopem) =

        // First define the type constructors and the abbreviations, if any. 
        if verbose then  dprintf "--> TcTyconDefnCores@%a\n" output_range m;
        let tycons = tdefs |> List.map (TcTyconDefnCore_Phase0_BuildInitialTycon cenv env parent)

        // Publish the preliminary tycons 
        tycons |> List.iter (PublishTypeDefn cenv env);

        // Add them to the environment, though this does not add the fields and 
        // constructors (because we haven't established them yet). 
        // We re-add them to the original environment later on. 
        // We don't report them to the Language Service yet as we don't know if 
        // they are well-formed (e.g. free of abbreviation cycles - see bug 952) 
        let envinner = AddLocalTycons cenv.g cenv.amap scopem tycons env


        // Establish the kind of each type constructor 
        // Here we run InferTyconKind and record partial information about the kind of the type constructor. 
        // This means TyconObjModelKind is set, which means is_sealed_typ, is_interface_typ etc. give accurate results. 
        (tdefs,tycons) ||> List.iter2 (TcTyconDefnCore_Phase1_EstablishBasicKind cenv inSig envinner)
            
        // Establish the abbreviations (no constraint checking, because constraints not yet established)
        (tdefs,tycons) ||> List.iter2 (TcTyconDefnCore_Phase2_Phase4_EstablishAbbreviations cenv envinner tpenv inSig FirstPass)

        // Check for cyclic abbreviations. If this succeeds we can start reducing abbreviations safely.
        CheckForCyclicAbbreviations cenv tycons;

        // Establish the super type and interfaces  (no constraint checking, because constraints not yet established)     
        TcTyconDefnCore_Phase3_Phase5_EstablishSuperTypesAndInterfaceTypes cenv envinner tpenv inSig tdefs tycons FirstPass;

        // REVIEW: we should separate the checking for cyclic hierarchies and cyclic structs
        // REVIEW: this is because in some extreme cases the TcTyparConstraints call below could
        // exercise a cyclic hierarchy (and thus not terminate) before the cycle checking has been
        // performed. Likewise operations in phases 3-6 could also exercise a cyclic hierarchy
        
        // Check and publish the explicit constraints. 
        (tdefs,tycons) ||> List.iter2 (fun (synTyconInfo,synTyconRepr,_) tycon -> 
            let (ComponentInfo(_,kind,_, wcs,_,_,_, _,im)) = synTyconInfo
            let envinner = AddDeclaredTypars CheckForDuplicateTypars (tycon.Typars(m)) envinner
            let thisTyconRef = mk_local_tcref tycon
            let envinner = MkInnerEnvForTyconRef cenv envinner thisTyconRef false
            try TcTyparConstraints cenv NoNewTypars CheckCxs envinner tpenv  wcs |> ignore
            with e -> errorRecovery e m);

        // No more constraints allowed on declared typars 
        tycons |> List.iter (fun tc -> tc.Typars(m) |> List.iter (SetTyparRigid cenv.g env.DisplayEnv m));
        
        // OK, now recheck the abbreviations and super/interface types (this time checking constraints)
        (tdefs,tycons) ||> List.iter2 (TcTyconDefnCore_Phase2_Phase4_EstablishAbbreviations cenv envinner tpenv inSig SecondPass)
        TcTyconDefnCore_Phase3_Phase5_EstablishSuperTypesAndInterfaceTypes cenv envinner tpenv inSig tdefs tycons SecondPass;

        // Now all the type parameters, abbreviations, constraints and kind information is established.
        // Now do the representations.
        let baseVals = (tdefs,tycons) ||> List.map2 (TcTyconDefnCore_Phase6_EstablishRepresentation cenv envinner tpenv inSig)
                
        // Now check for cyclic structs and inheritance. It's possible these should be checked as separate conditions. 
        // REVIEW: checking for cyclic inheritance is happening too late. See note above.
        CheckForCyclicStructsAndInheritance cenv tycons;

        tycons |> List.iter (AddGenericHashAndComparisonDeclarations cenv env)

        // Add the tycons again to the environment (again) - this will add the constructors and fields. 
        let env = AddLocalTyconsAndReport cenv.g cenv.amap scopem tycons env

        (tycons,env,baseVals)

end // module EstablishTypeDefinitionCores


/// Given a type definition, compute whether its members form an extension of an existing type, and if so if it is an 
/// intrinsic or extrinsic extension
let ComputeTyconDeclKind isAtOriginalTyconDefn cenv env parent inSig m typars cs longPath = 
    let ad = AccessRightsOfEnv env
    let tcref = 
        match ResolveTypeLongIdent cenv.nameResolver ItemOccurence.Binding OpenQualified env.eNameResEnv ad longPath (List.length typars) with
        | Result res -> res
        | res when inSig && longPath.Length = 1 ->
            errorR(Deprecated("The syntax 'type X with ...' is reserved for augmentations. Types whose representations are hidden but which have members are now declared in signatures using 'type X = ...'. You may also need to add the '[<Sealed>] attribute to the type declaration in the signature",m));
            ForceRaise res
        | res -> ForceRaise res            

    let isInterfaceOrDelegateOrEnum = 
        (deref_tycon tcref).IsFSharpInterfaceTycon || 
        (deref_tycon tcref).IsFSharpDelegateTycon ||
        (deref_tycon tcref).IsFSharpEnumTycon

    let isInSameModuleOrNamespace = 
         match env.eMtypeAcc.Value.TypesByMangledName.TryFind(tcref.MangledName) with 
          | Some tycon -> (tycon_spec_order tcref.Deref tycon = 0)
          | None -> false
    
    let reqTypars = tcref.Typars(m)

    // Member definitions are intrinsic (added directly to the type) if:
    // a) For interfaces, only if it is in the original defn.
    //    Augmentations to interfaces via partial type defns will always be extensions, e.g. extension members on interfaces.
    // b) For other types, if the type is isInSameModuleOrNamespace
    let declKind,typars = 
        if isAtOriginalTyconDefn then 
            ModuleOrMemberBinding, reqTypars

        elif isInSameModuleOrNamespace && not isInterfaceOrDelegateOrEnum then 
            IntrinsicExtensionBinding, reqTypars
        else 
            if isInSameModuleOrNamespace && isInterfaceOrDelegateOrEnum then 
                errorR(Error("Members that extend interface, delegate or enum types must be placed in a module separate to the definition of the type. This module must either have the AutoOpen attribute or be opened explicitly by clioent code to bring the extension members into scope",tcref.Range))
            let nReqTypars = reqTypars.Length
            if nReqTypars <> typars.Length then 
                // not recoverable
                error(Error(sprintf "The declared type parameters for this type extension do not match the declared type parameters on the original type '%s'" tcref.DisplayNameWithUnderscoreTypars,m))

            let declaredTypars = TcTyparDecls cenv env typars
            let envinner = AddDeclaredTypars CheckForDuplicateTypars declaredTypars env
            let tpenv = TcTyparConstraints cenv NoNewTypars CheckCxs envinner emptyTyparEnv cs
            declaredTypars |> List.iter (SetTyparRigid cenv.g env.DisplayEnv m);
            if not (typar_decls_aequiv cenv.g tyeq_env_empty reqTypars declaredTypars) then 
                errorR(Error(sprintf "The declared type parameters for this type extension do not match the declared type parameters on the original type '%s'" tcref.DisplayNameWithUnderscoreTypars,m))
            ExtrinsicExtensionBinding, declaredTypars


    declKind, tcref, typars


let is_member          = function ClassMemberDefn_member_binding _   -> true | _ -> false
let is_implicit_ctor    = function ClassMemberDefn_implicit_ctor _    -> true | _ -> false
let is_implicit_inherit = function ClassMemberDefn_implicit_inherit _ -> true | _ -> false
let is_bindings         = function ClassMemberDefn_let_bindings _     -> true | _ -> false
let is_slotsig          = function ClassMemberDefn_slotsig _          -> true | _ -> false
let is_interface        = function ClassMemberDefn_interface _        -> true | _ -> false
let is_inherit          = function ClassMemberDefn_inherit _          -> true | _ -> false
let is_field            = function ClassMemberDefn_field _            -> true | _ -> false
let is_tycon            = function ClassMemberDefn_tycon _            -> true | _ -> false

let allFalse ps x = List.fold (fun acc p -> acc && not (p x)) true ps

/// Check the ordering on the bindings and members in a class construction
// Accepted forms:
//
// Implicit Construction:
//   implicit_ctor
//   optional implicit_inherit
//   multiple bindings
//   multiple member-binding(includes-overrides) or abstract-slot-declaration or interface-bindings
//
// Classic construction:
//   multiple (binding or slotsig or field or interface or inherit).
//   i.e. not local-bindings, implicit ctor or implicit inherit (or tycon?).
//   atMostOne inherit.
let CheckMembersForm ds = 
    match ds with
    | d::ds when is_implicit_ctor d ->
        // Implicit construction 
        let ds = match ds with
                 | d::ds when is_implicit_inherit d -> ds  (* skip inherit call if it comes next *)
                 | ds -> ds
        let localbindings ,ds = ds |> List.takeUntil (is_bindings >> not) 
        let memberbindings,ds = ds |> List.takeUntil (allFalse [is_member;is_slotsig;is_interface]) 
        match ds with
         | ClassMemberDefn_member_binding (_,m)       :: _ -> errorR(InternalError("List.takeUntil is wrong, have binding",m))
         | ClassMemberDefn_slotsig (_,_,m)            :: _ -> errorR(InternalError("List.takeUntil is wrong, have slotsig",m))
         | ClassMemberDefn_interface (_,_,m)          :: _ -> errorR(InternalError("List.takeUntil is wrong, have interface",m))
         | ClassMemberDefn_implicit_ctor (_,_,_,_,m)  :: _ -> errorR(InternalError("implicit class construction with two implicit constructions",m))
         | ClassMemberDefn_implicit_inherit (_,_,_,m) :: _ -> errorR(Error("Type definitions using implicit construction may only have one 'inherit' specification and it must be the first declaration",m))
         | ClassMemberDefn_let_bindings (_,_,_,m)     :: _ -> errorR(Error("Type definitions using implicit construction must have local let/do-bindings preceding member and interface definitions",m))
         | ClassMemberDefn_inherit (_,_,m)            :: _ -> errorR(Error("This 'inherit' declaration specifies the inherited type but no arguments. Consider supplying arguments, e.g. 'inherit BaseType(args)'",m))
(*         | ClassMemberDefn_field (_,m)              :: _ -> errorR(Error("Class definitions using both an implicit construction sequence and field specifications are not yet implemented",m)) *)
         | ClassMemberDefn_tycon (_,_,m)              :: _ -> errorR(Error("Types may not contain nested type definitions",m))
         | _ -> ()
    | ds ->
        // Classic class construction 
        let memberbindings,ds = List.takeUntil (allFalse [is_member;is_slotsig;is_interface;is_inherit;is_field;is_tycon]) ds
        match ds with
         | ClassMemberDefn_member_binding (_,m)       :: _ -> errorR(InternalError("CheckMembersForm: List.takeUntil is wrong",m))
         | ClassMemberDefn_implicit_ctor (_,_,_,_,m)  :: _ -> errorR(InternalError("CheckMembersForm: implicit ctor line should be first",m))
         | ClassMemberDefn_implicit_inherit (_,_,_,m) :: _ -> errorR(Error("This 'inherit' construction call is not part of an implicit construction sequence. Only the inherited type should be specified at this point. Calls to the inherited constructor should be placed inside the object intialization expression of your object constructor. Alternatively use an implicit construction sequence by modifying the type declaration to include arguments, e.g. 'type X(args) = ...'",m))
         | ClassMemberDefn_let_bindings (_,false,_,m) :: _ -> errorR(Error("'let' and 'do' bindings are not permitted in class definitions unless an implicit construction sequence is used. You can use an implicit construction sequence by modifying the type declaration to include arguments, e.g. 'type X(args) = ...'",m))
         | ClassMemberDefn_slotsig (_,_,m)            :: _ 
         | ClassMemberDefn_interface (_,_,m)          :: _ 
         | ClassMemberDefn_inherit (_,_,m)            :: _ 
         | ClassMemberDefn_field (_,m)                :: _ 
         | ClassMemberDefn_tycon (_,_,m)              :: _ -> errorR(InternalError("CheckMembersForm: List.takeUntil is wrong",m))
         | _ -> ()
                 

/// Parallels SplitTyconSignature/SplitTyconDefn]
/// Separates the definition into core (shape) and body.
/// core = synTyconInfo,simpleRepr,interfaceTypes
///        where simpleRepr can contain inherit type, declared fields and virtual slots.
/// body = members
///        where members contain methods/overrides, also implicit ctor, inheritCall and local definitions.
///------
/// The tinfos arg are the enclosing types when processing nested types...
/// The tinfos arg is not currently used... just stacked up.
let rec SplitTyconDefn tinfos (TyconDefn(synTyconInfo,trepr,extraMembers,_)) =
    let implements1 = List.choose (function ClassMemberDefn_interface (f,_,m) -> Some(f,m) | _ -> None) extraMembers
    match trepr with
    | TyconDefnRepr_class(kind,cspec,m) ->
        CheckMembersForm cspec;
        let fields      = cspec |> List.choose (function ClassMemberDefn_field (f,_) -> Some(f) | _ -> None)
        let implements2 = cspec |> List.choose (function ClassMemberDefn_interface (ty,_,m) -> Some(ty,m) | _ -> None)
        let inherits    = cspec |> List.choose (function 
                                                      | ClassMemberDefn_inherit          (ty,idOpt,m)     -> Some(ty,m,idOpt)
                                                      | ClassMemberDefn_implicit_inherit (ty,arg,idOpt,m) -> Some(ty,m,idOpt)
                                                      | _ -> None)
        let tycons      = cspec |> List.choose (function ClassMemberDefn_tycon (x,_,_) -> Some(x) | _ -> None)
        let slotsigs    = cspec |> List.choose (function ClassMemberDefn_slotsig (x,y,_) -> Some(x,y) | _ -> None)
        let members     = cspec |> List.filter (function 
                                                    | ClassMemberDefn_interface _
                                                    | ClassMemberDefn_member_binding _ 
                                                    | ClassMemberDefn_let_bindings _
                                                    | ClassMemberDefn_implicit_ctor _
                                                    | ClassMemberDefn_open _
                                                    | ClassMemberDefn_implicit_inherit _ -> true
                                                    | ClassMemberDefn_tycon  (_,_,m)  -> error(Error("Types may not contain nested type definitions",m)); false
                                                    | ClassMemberDefn_field _   -> false (* covered above *)
                                                    | ClassMemberDefn_inherit _ -> false (* covered above *)
                                                    | ClassMemberDefn_slotsig _ -> false (* covered above *)
                                                    )
        let a,b = SplitTyconDefns (tinfos @ [synTyconInfo]) tycons

        let isConcrete = 
            members |> List.exists (function 
                | ClassMemberDefn_member_binding(Binding(_,_,_,_,_,_,ValSynData(Some(memberFlags),_,_),_,_,_,_),_) -> not memberFlags.MemberIsDispatchSlot 
                | ClassMemberDefn_interface (_,defOpt,_) -> isSome defOpt
                | ClassMemberDefn_let_bindings _ -> true
                | ClassMemberDefn_implicit_ctor _ -> true
                | ClassMemberDefn_implicit_inherit _ -> true
                | _ -> false)

        let isIncrClass = 
            members |> List.exists (function 
                | ClassMemberDefn_implicit_ctor _ -> true
                | _ -> false)
                
        let core = (synTyconInfo, TyconCore_general(kind,inherits,slotsigs,fields,isConcrete,isIncrClass,m), implements2@implements1)
        core :: a,
        members :: b
    | TyconDefnRepr_simple(r,_) -> [(synTyconInfo,r,implements1)],[ [] ]

and SplitTyconDefns tinfos tycons = 
    let a,b = List.unzip (List.map (SplitTyconDefn tinfos) tycons)
    List.concat a, List.concat b 

let PrepareTyconMemberDefns isAtOriginalTyconDefn cenv env  parent (synTyconInfo,baseValOpt,members,m) =
    let (ComponentInfo(_,kind,typars, cs,longPath, _, _, _,im)) = synTyconInfo
    let declKind,tcref, declaredTyconTypars = ComputeTyconDeclKind isAtOriginalTyconDefn cenv env parent false m typars cs longPath

    let newslotsOK = (if isAtOriginalTyconDefn && tcref.IsFSharpObjectModelTycon then NewSlotsOK else NoNewSlots) // NewSlotsOK only on fsobjs 
    if nonNil(members) && tcref.IsTypeAbbrev then errorR(Error("Type abbreviations may not have augmentations",m));
    if verboseCC then dprintf "PrepareTyconMemberDefns: #members=%d\n" (List.length members);
    TyconMemberData(declKind,tcref,baseValOpt,declaredTyconTypars,members,m,newslotsOK)

//-------------------------------------------------------------------------
// Bind type definitions - main
//------------------------------------------------------------------------- 

let TcTyconDefns cenv env parent tpenv (tdefs: SynTyconDefn list,m,scopem) =
    let cores,membersl = SplitTyconDefns [] tdefs
    let tycons,env,baseValOpt = EstablishTypeDefinitionCores.TcTyconDefnCores cenv env false parent tpenv (cores,m,scopem)
    let augments = 
        (tdefs, baseValOpt, membersl) |||> List.map3 (fun (TyconDefn(synTyconInfo,_,extraMembers,m)) baseValOpt members -> 
               PrepareTyconMemberDefns true cenv env parent (synTyconInfo,baseValOpt,members@extraMembers,m))
          
    let valExprBuilders,env = TcTyconMemberDefns cenv env parent m scopem augments

    // Note: generating these bindings must come after generating the members, since some in the case of structs some fields
    // may be added by generating the implicit construction syntax 
    let binds = tycons |> List.collect (AddGenericHashAndComparisonBindings cenv env)
    let binds3 = tycons |> List.collect (AddGenericEqualityBindings cenv env)

    // Check for cyclic structs and inheritance all over again, since we may have added some fields to the struct when generating the implicit construction syntax 
    EstablishTypeDefinitionCores.CheckForCyclicStructsAndInheritance cenv tycons;

    (binds @ valExprBuilders @ binds3),tycons,env  


//-------------------------------------------------------------------------
// Bind type specifications
//------------------------------------------------------------------------- 

/// Parallels split_tycon[Spfn/Defn] 
let rec SplitTyconSignature tinfos (TyconSpfn(synTyconInfo,trepr,extraMembers,m)) = 
    let implements1 = 
        extraMembers |> List.choose (function ClassMemberSpfn_interface (f,m) -> Some(f,m) | _ -> None) 
    match trepr with
    | TyconSpfnRepr_class(kind,cspec,m) -> 
        let fields      = cspec |> List.choose (function ClassMemberSpfn_field (f,_) -> Some(f) | _ -> None)
        let implements2 = cspec |> List.choose (function ClassMemberSpfn_interface (ty,m) -> Some(ty,m) | _ -> None)
        let inherits    = cspec |> List.choose (function ClassMemberSpfn_inherit (ty,_) -> Some(ty,m,None) | _ -> None)
        let nestedTycons = cspec |> List.choose (function ClassMemberSpfn_tycon (x,_) -> Some(x) | _ -> None)
        let slotsigs    = cspec |> List.choose (function ClassMemberSpfn_binding (v,fl,_) when fl.MemberIsVirtual || fl.MemberIsDispatchSlot -> Some(v,fl) | _ -> None)
        let members     = cspec |> List.filter (function   
                                                      | ClassMemberSpfn_interface _ -> true
                                                      | ClassMemberSpfn_binding (_,memberFlags,_) when not memberFlags.MemberIsDispatchSlot -> true
                                                      | ClassMemberSpfn_tycon  (_,m) -> error(Error("Types may not contain nested type definitions",m)); false
                                                      | _ -> false)
        let isConcrete = 
            members |> List.exists (function 
                | ClassMemberSpfn_binding (_,memberFlags,_) -> memberFlags.MemberKind=MemberKindConstructor 
                | _ -> false)

        let a,b = nestedTycons |> SplitTyconSignatures (tinfos @ [synTyconInfo]) 
        [ (synTyconInfo, TyconCore_general(kind,inherits,slotsigs,fields,isConcrete,false,m),implements2@implements1) ] @ a,
        [ (synTyconInfo,true,members@extraMembers) ] @ b
    // 'type X with ...' in a signature is always interpreted as an extrinsic extension.
    // Representation-hidden types with members and interfaces are written 'type X = ...' 
    | TyconSpfnRepr_simple(TyconCore_no_repr _,_) when nonNil extraMembers -> 
        let isAtOriginalTyconDefn = false
        [],[ (synTyconInfo,isAtOriginalTyconDefn,extraMembers) ]
    | TyconSpfnRepr_simple(r,_) -> 
        [(synTyconInfo,r,implements1)],[ (synTyconInfo,true,extraMembers) ] 

and SplitTyconSignatures tinfos tycons = 
    let a,b = tycons |> List.map (SplitTyconSignature tinfos) |> List.unzip 
    List.concat a, List.concat b 

let TcTyconSignatureMemberSpecs cenv env parent tpenv membersl =
    (tpenv, membersl) ||> List.mapfold (fun tpenv (synTyconInfo,isAtOriginalTyconDefn,members) -> 
        let (ComponentInfo(_,_,typars,cs,longPath, _, _, _,m)) = synTyconInfo
        let declKind,tcref,declaredTyconTypars = ComputeTyconDeclKind isAtOriginalTyconDefn cenv env parent true m typars cs longPath

        let envinner = AddDeclaredTypars CheckForDuplicateTypars declaredTyconTypars env
        let envinner = MkInnerEnvForTyconRef cenv envinner tcref (declKind = ExtrinsicExtensionBinding)

        TcTyconMemberSpecs true cenv envinner (TyconContainerInfo(parent,tcref,declaredTyconTypars)) declKind tcref tpenv members)

let TcTyconSignatures cenv env parent tpenv (tspecs:SynTyconSpfn list,m,scopem) =
    let cores,membersl = SplitTyconSignatures [] tspecs
    let tycons,env,_ = EstablishTypeDefinitionCores.TcTyconDefnCores cenv env true parent tpenv (cores,m,scopem)
    let vals1,tpenv = TcTyconSignatureMemberSpecs cenv env parent tpenv membersl
    env

//-------------------------------------------------------------------------
// Bind module types
//------------------------------------------------------------------------- 

let AdjustModuleName modKind nm = (match modKind with FSharpModuleWithSuffix -> nm^FSharpModuleSuffix | _ -> nm)


let rec TcSignatureElement cenv parentModule endm (env:tcEnv) e =
    let parent = Parent(parentModule)
    let containerInfo = ModuleOrNamespaceContainerInfo(parentModule)
    try 
        match e with 
        | Spec_exn (edef,m) ->
            let scopem = union_ranges (end_range_of_range m) endm
            let _,_,_,env = TcExnSignature cenv env parent emptyTyparEnv (edef,scopem)
            env

        | Spec_tycon (tspecs,m) -> 
            let scopem = union_ranges m endm
            let env = TcTyconSignatures cenv env parent emptyTyparEnv (tspecs,m,scopem)
            env 

        | Spec_open (mp,m) -> 
            let scopem = union_ranges (end_range_of_range m) endm
            TcOpenDecl cenv.g cenv.amap m scopem env mp

        | Spec_val (vspec,m) -> 
            let idvs,_ = TcAndPublishValSpec (cenv,env,containerInfo,ModuleOrMemberBinding,None,emptyTyparEnv,vspec)
            let scopem = union_ranges m endm
            List.foldBack (AddLocalVal scopem) idvs env

        | Spec_module(ComponentInfo(attribs,kind,parms, constraints,longPath,xml,preferPostfix,vis,im),mdefs,m) ->
            let id = ComputeModuleName longPath
            let isModule =  ComputeIsModule kind parms constraints im
            let vis,_ = ComputeAccessAndCompPath env None im vis parent
            let mspec,_ = TcModuleOrNamespaceSignature cenv env (id,isModule,mdefs,xml,attribs,vis,m)
            let scopem = union_ranges m endm
            PublishModuleDefn cenv env mspec; 
            AddLocalSubModule cenv.g cenv.amap m  scopem env (text_of_id id) mspec
            
        | Spec_module_abbrev (id,p,m) -> 
            let ad = AccessRightsOfEnv env
            let mvvs = ForceRaise (ResolveLongIndentAsModuleOrNamespace OpenQualified env.eNameResEnv ad p)
            let scopem = union_ranges m endm
            let modrefs = mvvs |> List.map p23 
            if modrefs.Length > 0 && modrefs |> List.forall (fun modref -> modref.IsNamespace) then 
                warning(Error(sprintf "This module abbreviation abbreviates the namespace '%s'. This is deprecated and ignored: module abbreviations may now only abbreviate modules" (full_display_text_of_modref (List.hd modrefs)),m));
            let modrefs = modrefs |> List.filter (fun modref -> not modref.IsNamespace)
            modrefs |> List.iter (fun modref -> CheckEntityAttributes cenv.g modref m |> CommitOperationResult);        
            
            if modrefs.Length > 0 then AddModuleAbbreviation scopem id modrefs env else env

        | Spec_hash _ -> env

    with e -> errorRecovery e endm; env

and TcSignatureElements cenv parentModule endm env defs = 
    List.fold (TcSignatureElement cenv parentModule endm) env defs


and ComputeModuleOrNamespaceKind g isModule attribs = 
    if not isModule then Namespace 
    elif ModuleNameIsMangled g attribs then FSharpModuleWithSuffix 
    else FSharpModule

and TcModuleOrNamespaceSignature cenv env (id:ident,isModule,defs,xml,attribs,vis,m) =
    if verbose then  dprintf "TcModuleOrNamespaceSignature...\n";
    let attribs = TcAttributes cenv env attrTgtModuleDecl attribs
    CheckNamespaceModuleOrTypeName id;
    let modKind = ComputeModuleOrNamespaceKind cenv.g isModule attribs
    if isModule then CheckForDuplicateConcreteType cenv env (AdjustModuleName modKind id.idText) id.idRange;
    if isModule then CheckForDuplicateModule cenv env id.idText id.idRange;

    // Now typecheck the signature, accumulating and then recording the submodule description. 
    let id = ident (AdjustModuleName modKind id.idText, id.idRange)
    let envinner,mtypeAcc = MakeInnerEnv env id modKind
    let mspec = NewModuleOrNamespace  (Some(curr_cpath env)) vis id (xml.ToXmlDoc()) attribs (notlazy (empty_mtype modKind)) 

    let innerParent = mk_local_modref mspec
    
    let mtyp,envAtEnd = TcModuleOrNamespaceSignatureElements cenv innerParent env (id,modKind,defs,m,xml)

    if !verboseStamps then 
        dprintf "TcModuleOrNamespaceSignature: %s#%d, vis = %s\n" mspec.MangledName mspec.Stamp (string_of_access vis);

    mspec.Data.entity_modul_contents <- notlazy mtyp; 
    
    mspec, envAtEnd

and TcModuleOrNamespaceSignatureElements cenv parentModule env (id,modKind,defs,m,xml) =

    let endm = end_range_of_range m // use end of range for errors 

    if verbose then  dprintf "--> TcModuleOrNamespaceSignatureElements, endm = %a\n" output_range endm;
    // Create the module type that will hold the results of type checking.... 
    let envinner,mtypeAcc = MakeInnerEnv env id modKind

    // Ensure the deref_nlpath call in UpdateAccModuleOrNamespaceType succeeds 
    if cenv.compilingCanonicalFslibModuleType then 
        ensure_fslib_has_submodul_at cenv.topCcu envinner.ePath (curr_cpath envinner) (xml.ToXmlDoc());

    // Now typecheck the signature, using mutation to fill in the submodule description. 
    let envAtEnd = TcSignatureElements cenv parentModule endm envinner defs
    
    // mtypeAcc has now accumulated the module type 
    !mtypeAcc, envAtEnd

//-------------------------------------------------------------------------
// Bind definitions within modules
//------------------------------------------------------------------------- 

let rec TcModuleOrNamespaceElement cenv parentModule scopem env e = // : ((ModuleOrNamespaceExpr list -> ModuleOrNamespaceExpr list) * _) * tcEnv =
  eventually {
    let tpenv = emptyTyparEnv
    let parent = Parent(parentModule)
    let containerInfo = ModuleOrNamespaceContainerInfo(parentModule)
    try 
      if verbose then  dprintf "--> TcModuleOrNamespaceElement@%a\n" output_range (range_of_syndecl e);
      match e with 

      | Def_module_abbrev (id,p,m) -> 
          let ad = AccessRightsOfEnv env
          let mvvs = ForceRaise (ResolveLongIndentAsModuleOrNamespace OpenQualified env.eNameResEnv ad p)
          let modrefs = mvvs |> List.map p23 
          if modrefs.Length > 0 && modrefs |> List.forall (fun modref -> modref.IsNamespace) then 
              warning(Error(sprintf "This module abbreviation abbreviates the namespace '%s'. This is deprecated and ignored: module abbreviations may now only abbreviate modules" (full_display_text_of_modref (List.hd modrefs)),m));
          let modrefs = modrefs |> List.filter (fun mvv -> not mvv.IsNamespace)
          modrefs |> List.iter (fun modref -> CheckEntityAttributes cenv.g modref m |> CommitOperationResult);        
          return ((fun e -> e), []), (if modrefs.Length > 0 then AddModuleAbbreviation scopem id modrefs env else env)

      | Def_exn (edef,m) -> 
          let binds,decl,env = TcExnDefn cenv env parent tpenv (edef,scopem)
          return ((fun e -> TMDefRec([decl], FlatList.of_list binds, [],m) :: e),[]), env

      | Def_tycons (tdefs,m) -> 
          let scopem = union_ranges m scopem
          let binds,tycons,env' = TcTyconDefns cenv env parent tpenv (tdefs,m,scopem)
          (* check the non-escaping condition as we build the expression on the way back up *)
          let exprfWithEscapeCheck e = 
              let freeInEnv = GeneralizationHelpers.ComputeUnabstractableTycons env
              tycons |> List.iter(fun tycon -> 
                  let nm = tycon.DisplayName
                  if Zset.mem tycon freeInEnv then errorR(Error(sprintf "The type '%s' is used in an invalid way. A value prior to '%s' has an inferred type involving '%s', which is an invalid forward reference" nm nm nm, tycon.Range)));
              TMDefRec(tycons,FlatList.of_list binds,[],m) :: e
          return (exprfWithEscapeCheck,[]),env'
      | Def_partial_tycon (tcinfo,members,m) -> 
          let scopem = union_ranges m scopem
          let augment = PrepareTyconMemberDefns false cenv env parent (tcinfo,None,members,m)
          let binds,env = TcTyconMemberDefns cenv env parent m scopem [augment]
          // Check the non-escaping condition for potential trait solutions as we build the expression on the way back up 
          
          let exprfWithEscapeCheck e = 
              let freeInEnv = GeneralizationHelpers.ComputeUnabstractableTraitSolutions env
              binds |> List.iter(fun bind -> 
                  let nm = bind.Var.DisplayName
                  if Zset.mem bind.Var freeInEnv then errorR(Error(sprintf "The member '%s' is used in an invalid way. A use of '%s' has been inferred prior to the definition of '%s', which is an invalid forward reference" nm nm nm, bind.Var.Range)));
              TMDefRec([],FlatList.of_list binds,[],m) :: e

          (* REVIEW: record a TDecl_partial_tycon instead of mutating the type constructor *)
          return (exprfWithEscapeCheck,[]),env 

      | Def_open (mp,m) -> 
          let scopem = union_ranges (end_range_of_range m) scopem
          return ((fun e -> e),[]), TcOpenDecl cenv.g cenv.amap m scopem env mp

      | Def_let (letrec, binds, m) -> 
          if letrec then 
            let scopem = union_ranges m scopem
            let binds = binds |> List.map (fun bind -> RecBindingDefn(containerInfo,NoNewSlots,ModuleOrMemberBinding,bind))
            let binds,env,_ = TcLetrec  WarnOnOverrides cenv env tpenv (binds,m, scopem)
            return ((fun e -> TMDefRec([],FlatList.of_list binds,[],m) :: e),[]),env
          else 
            let binds,env,_ = TcLetBindings cenv env containerInfo ModuleOrMemberBinding tpenv (binds,m,scopem)
            return ((fun e -> binds@e),[]),env 

      | Def_expr (spExpr,expr, m) -> 

          let bind = 
              Binding (None,
                       StandaloneExpression,
                       false,false,[],emptyPreXmlDoc,SynInfo.emptyValSynData,
                       Pat_wild m,
                       BindingRhs([],None,expr),m,spExpr)

          return! TcModuleOrNamespaceElement cenv parentModule scopem env (Def_let(false,[bind],m))

      | Def_attributes (attrs,_) -> 
          let attrs' = TcAttributesWithPossibleTargets cenv env attrTgtTop attrs
          return ((fun e -> e), attrs'), env

      | Def_hash (i,m) -> 
          return ((fun e -> e), []), env

      | Def_module(ComponentInfo(attribs,kind,parms, constraints,longPath,xml,preferPostfix,vis,im),mdefs,explicitSigOpt,m) ->
          let id = ComputeModuleName longPath
          let isModule =  ComputeIsModule kind parms constraints im

          let modAttrs = TcAttributes cenv env attrTgtModuleDecl attribs
          let modKind = ComputeModuleOrNamespaceKind cenv.g isModule modAttrs
          if verbose then  dprintf "Def_module\n";
          if isModule then CheckForDuplicateConcreteType cenv env (AdjustModuleName modKind id.idText) im;
          if isModule then CheckForDuplicateModule cenv env id.idText id.idRange;
          let vis,_ = ComputeAccessAndCompPath env None id.idRange vis parent
             
          let! (topAttrsNew, _,TMBind(mspecPriorToOuterOrExplicitSig,mexpr)),_ =
              TcModuleOrNamespace cenv env (id,isModule,mdefs,xml,modAttrs,vis,m)

          let mspec,mdef = 
              match explicitSigOpt with 
              | None -> mspecPriorToOuterOrExplicitSig,TMDefRec([],FlatList.empty,[TMBind(mspecPriorToOuterOrExplicitSig,mexpr)],m)
              | Some (Sign_explicit signElements) -> 
                  error(Error("Explicit signatures within implementation files may no longer be used",m))
 
#if INNER_SIGNATURES
                  (* Take the implementation and build a new module with a different module type *)
                  let mspecExplicitSig = mspecPriorToOuterOrExplicitSig |> NewModifiedModuleOrNamespace (fun mtyp -> mtyp)
                  let explicitSigParent = mk_local_modref mspecExplicitSig
                  let explicitSig,_ = TcModuleOrNamespaceSignatureElements cenv explicitSigParent env (id,modKind,signElements,m,emptyXmlDoc)
                  mspecExplicitSig.Data.entity_modul_contents <- notlazy mtyp; 

                  (* Check the module signature *)
                  begin
                      (* Compute the alpha-conversion mapping between type contructors in signature and implementation *)
                      let aenv = 
                          let remapInfo ,hidingInfo = mk_mtyp_to_mtyp_remapping mspecPriorToOuterOrExplicitSig.ModuleOrNamespaceType explicitSig
                          { tyeq_env_empty with ae_tcrefs = tcref_map_of_list remapInfo.mrpiTycons }
                          
                      SignatureConformance.CheckModuleOrNamespace cenv.g cenv.amap env.DisplayEnv aenv (mk_local_modref mspecPriorToOuterOrExplicitSig) explicitSig |> ignore;
                  end;

                  mspecExplicitSig,TMAbstract(TMTyped(explicitSig,TMDefRec([],[],[TMBind(mspecPriorToOuterOrExplicitSig,mexpr)],m),im))
#endif

              |  Some (Sign_named _) -> error(Error("Modules may not use named module signature definitions",im))

          PublishModuleDefn cenv env mspec; 
          let env = AddLocalSubModule cenv.g cenv.amap m scopem env (text_of_id id) mspec
          return ((fun e -> mdef :: e),topAttrsNew), env
      
    with exn -> 
        errorRecovery exn (range_of_syndecl e); 
        return ((fun e -> e), []),env
 }
 
and TcModuleOrNamespaceElements cenv parent endm (defsSoFar,env) moreDefs =
 eventually {
    match moreDefs with 
    | (h1 :: t) ->
        (* lookahead one to find out the scope of the next declaration *)
        let scopem = 
            if isNil t then union_ranges (range_of_syndecl h1) endm
            else union_ranges (range_of_syndecl (List.hd t)) endm

        // Possibly better:
        //let scopem = union_ranges (end_range_of_range (range_of_syndecl h1)) endm
        
        let! h1',env' = TcModuleOrNamespaceElement cenv parent scopem env h1
        // tail recursive 
        return! TcModuleOrNamespaceElements  cenv parent endm ( (h1' :: defsSoFar), env') t
    | [] -> 
        return List.rev defsSoFar,env
 }
   
and TcModuleOrNamespace cenv env (id,isModule,defs,xml,modAttrs,vis,m) =
  eventually {
    if verbose then  dprintf "TcModuleOrNamespace...\n";
    let endm = end_range_of_range m
    let modKind = ComputeModuleOrNamespaceKind cenv.g isModule modAttrs
    let id = ident (AdjustModuleName modKind id.idText, id.idRange)
    CheckNamespaceModuleOrTypeName id;
    let envinner,mtypeAcc = MakeInnerEnv env id modKind
    let cpath = (curr_cpath envinner)
    
    // Ensure the deref_nlpath call in UpdateAccModuleOrNamespaceType succeeds 
    if cenv.compilingCanonicalFslibModuleType then 
        ensure_fslib_has_submodul_at cenv.topCcu envinner.ePath cpath (xml.ToXmlDoc());

    // Create the new module specification to hold the accumulated results of the type of the module 
    // Also record this in the environment as the accumulator 
    let mspec = NewModuleOrNamespace (Some(curr_cpath env)) vis id (xml.ToXmlDoc()) modAttrs (notlazy (empty_mtype modKind))

    if !verboseStamps then 
        dprintf "TcModuleOrNamespace: %s#%d\n" mspec.MangledName mspec.Stamp;

    let innerParent = mk_local_modref mspec

    // Now typecheck. 
    let! defs',envAtEnd = TcModuleOrNamespaceElements cenv innerParent endm ([],envinner) defs 

    // Get the inferred type of the decls. It's precisely the one we created before checking 
    // and mutated as we went. Record it in the mspec. 
    mspec.Data.entity_modul_contents <- notlazy !mtypeAcc ; 

    // Apply the functions for each declaration to build the overall expression-builder 
    let mexpr = TMDefs(List.foldBack (fun (f,_) x -> f x) defs' []) 

    if verbose then  dprintf "TcModuleOrNamespace %s, created %d, vis = %s\n" mspec.MangledName mspec.Stamp (string_of_access vis);

    // Collect up the attributes that are global to the file 
    let topAttrs = List.foldBack (fun (_,y) x -> y@x) defs' []
    
    return (topAttrs,mspec,TMBind(mspec,mexpr)), envAtEnd
 }


/// Set up the initial environment 
let LocateEnv ccu env enclosingNamespacePath =
    let cpath = cpath_of_ccu ccu
    let env = {env with ePath = []; eCompPath = cpath; eAccessPath=cpath }
    let env = List.fold (fun env id -> MakeInnerEnv env id Namespace |> fst) env enclosingNamespacePath
    env

let BuildTopRootedModul enclosingNamespacePath mspec = List.foldBack wrap_modul_in_namespace  enclosingNamespacePath mspec 
        
let BuildTopRootedModulBind enclosingNamespacePath mbind = List.foldBack wrap_mbind_in_namespace  enclosingNamespacePath mbind 

//--------------------------------------------------------------------------
// TypecheckOneImplFile - Typecheck all the namespace fragments in a file.
//-------------------------------------------------------------------------- 

let AddCcuToTcEnv(g,amap,scopem,env,ccu,autoOpens,internalsVisible) = 
    let env = add_nonlocal_ccu g amap scopem env (ccu,internalsVisible)

#if AUTO_OPEN_ATTRIBUTES_AS_OPEN
    let env = List.fold (fun env p -> TcOpenDecl g amap scopem scopem env (path_to_lid scopem (split_namespace p))) env autoOpens
#else
    let env = (env,autoOpens) ||> List.fold (fun env p -> 
                  let warn() = 
                      warning(Error(sprintf "The attribute 'AutoOpen(\"%s\")' in the assembly '%s' did not refer to a valid module or namespace in that assembly and has been ignored" p ccu.AssemblyName,scopem));
                      env
                  let p = split_namespace p in 
                  if isNil p then warn() else
                  let h,t = List.frontAndBack p 
                  let modref = mk_nonlocal_tcref (mk_top_nlpath ccu (Array.of_list h))  t
                  match modref.TryDeref with 
                  | None ->  warn()
                  | Some _ -> open_modul g amap scopem env modref) 
#endif
    env

let CreateInitialTcEnv(g,amap,scopem,ccus) =
    List.fold (fun env (ccu,autoOpens,internalsVisible) -> AddCcuToTcEnv(g,amap,scopem,env,ccu,autoOpens,internalsVisible)) (empty_tenv g) ccus

type conditionalDefines = 
    string list


/// The attributes that don't get attached to any declaration
type topAttribs =
    { mainMethodAttrs: Attribs;
      netModuleAttrs: Attribs;
      assemblyAttrs : Attribs  }

let EmptyTopAttrs =
    { mainMethodAttrs=[];
      netModuleAttrs=[];
      assemblyAttrs =[]  }

let CombineTopAttrs topAttrs1 topAttrs2 =
    { mainMethodAttrs = topAttrs1.mainMethodAttrs @ topAttrs2.mainMethodAttrs;
      netModuleAttrs  = topAttrs1.netModuleAttrs @ topAttrs2.netModuleAttrs;
      assemblyAttrs   = topAttrs1.assemblyAttrs @ topAttrs2.assemblyAttrs } 

let ImplicitlyOpenOwnNamespace g amap scopem isModule lid env = 
    assert (nonNil lid)
    let lid = 
        if isModule then fst (List.frontAndBack lid)
        else lid
    if isNil lid then 
        env
    else
        let ad = AccessRightsOfEnv env
        match ResolveLongIndentAsModuleOrNamespace OpenQualified env.eNameResEnv ad lid  with 
        | Result modrefs -> open_moduls g amap scopem env modrefs 
        | Exception _ ->  env


/// Check a single fragment within an implementation file
let TcNamespaceFragment cenv
                 (topAttrs,env,envAtEnd,fragTypesPriorToSig,implFileBinds) 
                 (ModuleOrNamespaceImpl(lid,isModule,defs,xml,attribs,vis,m)) =
  eventually {               
    if !progress then dprintn ("Typecheck implementation "^text_of_lid lid);
    let endm = end_range_of_range m

    let enclosingNamespacePath,moduleName = List.frontAndBack lid
    let envinner = LocateEnv cenv.topCcu env enclosingNamespacePath
    let envinner = ImplicitlyOpenOwnNamespace cenv.g cenv.amap m isModule lid envinner

    let vis,_ = ComputeAccessAndCompPath envinner None endm vis ParentNone

    let modAttrs = TcAttributes cenv envinner attrTgtModuleDecl attribs
    let! (topAttrsNew,mspec,mbind), envAtEnd  = TcModuleOrNamespace cenv envinner (moduleName,isModule,defs,xml,modAttrs,vis,m)

    let implFileSpecPriorToSig = BuildTopRootedModul enclosingNamespacePath mspec
    let mbindTopRooted = BuildTopRootedModulBind enclosingNamespacePath mbind
    let topAttrsNew = 
        let il_main_attrs,others = topAttrsNew |> List.partition (fun (possTargets,_) -> possTargets &&& attrTgtMethod <> 0) 
        let il_assem_attrs,others = others |> List.partition (fun (possTargets,_) -> possTargets &&& attrTgtAssembly <> 0) 
        let il_module_attrs,others = others |> List.partition (fun (possTargets,_) -> possTargets &&& attrTgtModule <> 0)
        { mainMethodAttrs = List.map snd il_main_attrs;
          netModuleAttrs  = List.map snd il_module_attrs;
          assemblyAttrs   = List.map snd il_assem_attrs}
          
    let env = AddLocalTopRootedModuleOrNamespace cenv.g cenv.amap m env (wrap_modul_as_mtyp_in_namespace implFileSpecPriorToSig)
    
    let topAttrs = CombineTopAttrs topAttrs topAttrsNew
    let fragTypesPriorToSig = fragTypesPriorToSig@[implFileSpecPriorToSig]
    let implFileBinds = implFileBinds @ [TMDefRec([],FlatList.empty,[mbindTopRooted],m)]
    return (topAttrs,env,envAtEnd,fragTypesPriorToSig,implFileBinds)
 }

let rec IterTyconsOfModuleOrNamespaceType f (mty:ModuleOrNamespaceType) = 
    mty.AllEntities |> Map.iter (fun _ tycon -> f tycon);
    mty.ModuleAndNamespaceDefinitions |> List.iter (fun v -> 
        IterTyconsOfModuleOrNamespaceType f v.ModuleOrNamespaceType)


/// Check an entire implementation file
/// Typecheck, then close the inference scope and then check the file meets its signature (if any)
let TypecheckOneImplFile 
       (* checkWeShouldContinue: A function to help us stop reporting cascading errors *)        
       (g,niceNameGen,amap,topCcu,checkWeShouldContinue,conditionalDefines) 
       env 
       (topRootedSigOpt : ModuleOrNamespaceType option)
       (ImplFile(fileName,isScript,qualNameOfFile,scopedPragmas,_,implFileFrags,canContainEntryPoint) as implFile) =

 eventually {
    let cenv = new_cenv (g,isScript,niceNameGen,amap,topCcu,false,isSome(topRootedSigOpt),conditionalDefines)    

    let! (topAttrs,env,envAtEnd,fragTypesPriorToSig,implFileBinds) = 
        ((EmptyTopAttrs,env,env,[],[]),implFileFrags) ||> Eventually.fold (TcNamespaceFragment cenv)

    // Note: we currently give errors w.r.t. the display environment that includes ALL 'opens' from ALL the namespace fragments 
    let denvAtEnd = envAtEnd.DisplayEnv
    let fragDefn = TMDefs(implFileBinds)

    // Note: unless we can do better, any errors are given w.r.t. 'qualNameOfFile.Range', i.e. the range of the leading module/namespace declaration 
    let m = qualNameOfFile.Range

    // Combine the fragments 
    let implFileTypePriorToSig = combine_mtyps [] m (List.map wrap_modul_as_mtyp_in_namespace fragTypesPriorToSig)
    let implFileSpecPriorToSig = wrap_mtyp_as_mspec qualNameOfFile.Id (cpath_of_ccu topCcu) implFileTypePriorToSig

    // Defaults get applied before the module signature is checked and before the implementation conditions on virtuals/overrides. 
    // Defaults get applied in priority order. Defaults listed last get priority 0 (lowest), 2nd last priority 1 etc. 

    let extraAttribs = topAttrs.mainMethodAttrs@topAttrs.netModuleAttrs@topAttrs.assemblyAttrs
    
    let _ = 
        try
            let unsolved = Microsoft.FSharp.Compiler.FindUnsolved.unsolved_typars_of_mdef g cenv.amap denvAtEnd (fragDefn,extraAttribs)

            if verboseCC then dprintf "calling CanonicalizePartialInferenceProblem\n" ;
            GeneralizationHelpers.CanonicalizePartialInferenceProblem (cenv,denvAtEnd,m) unsolved;

            if verboseCC then dprintf "applying defaults..." ;
    
            let applyDefaults priority =
                 if verboseCC then dprintf "assigning defaults to pseudo variables at priority %d...\n" priority;
                 unsolved |> List.iter (fun tp -> 
                    if not (tpref_is_solved tp) then 
                        (* Apply the first default. If we're defaulting one type variable to another then *)
                        (* the defaults will be propagated to the new type variable. *)
                        tp.Constraints |> List.iter (fun tpc -> 
                            match tpc with 
                            | TTyparDefaultsToType(priority2,ty2,m) when priority2 = priority -> 
                                let ty1 = (mk_typar_ty tp)
                                if (tpref_is_solved tp) || (type_equiv cenv.g ty1 ty2) then (
                                    if verbose then dprintf "skipping solved/equal default '%s' for variable '%s' near %a at priority %d\n" ((DebugPrint.showType ty2)) ((DebugPrint.showType ty1)) output_range m priority2;
                                ) else (
                                    if verbose then dprintf "assigning default '%s' for variable '%s' near %a at priority %d\n" ((DebugPrint.showType ty2)) ((DebugPrint.showType ty1)) output_range m priority2;
                                    let csenv = (MakeConstraintSolverEnv cenv.css m denvAtEnd)
                                    TryD (fun () -> ConstraintSolver.SolveTyparEqualsTyp csenv 0 m NoTrace ty1 ty2)
                                         (fun e -> solveTypAsError cenv denvAtEnd m ty1;
                                                   ErrorD(ErrorFromApplyingDefault(g,denvAtEnd,tp,ty2,e,m)))
                                    |> RaiseOperationResult;
                                )
                            | _ -> ()))
                    
            for priority = 10 downto 0 do
                applyDefaults priority
            done;

            (* OK, now apply defaults for any unsolved HeadTypeStaticReq *)
            unsolved |> List.iter (fun tp ->     
                if not (tpref_is_solved tp) then 
                    if (tp.StaticReq <> NoStaticReq) then
                        ConstraintSolver.ChooseTyparSolutionAndSolve cenv.css envAtEnd.DisplayEnv tp);
        with e -> errorRecovery e m

    // Check completion of all classes defined across this file. 
    // REVIEW: this is not a great technique if inner signatures are permitted to hide 
    // virtual dispatch slots. 
    if (checkWeShouldContinue()) then  
        if verboseCC then dprintf "checking all virtual promises implemented...\n";
        try implFileTypePriorToSig |> IterTyconsOfModuleOrNamespaceType (FinalTypeDefinitionChecksAtEndOfInferenceScope cenv.infoReader true denvAtEnd);
        with e -> errorRecovery e m 

    // Check the value restriction. Only checked if there is no signature.
    if (checkWeShouldContinue() && isNone topRootedSigOpt) then 
        if verboseCC then dprintf "checking generalization of exported things...\n";

        // REVIEW: This is checking things in a somewhat arbitrary order
        let rec check (mty:ModuleOrNamespaceType) =
            mty.AllValuesAndMembers |> Map.iter (fun _ v -> 
                let ftyvs = (free_in_val CollectTyparsNoCaching v).FreeTypars |> Zset.elements
                if not v.IsCompilerGenerated && not (ftyvs |> List.exists (fun tp -> tp.IsFromError)) then 
                  match ftyvs with 
                  | tp :: _ -> errorR (ValueRestriction(envAtEnd.eNameResEnv.eDisplayEnv,false,v, tp,v.Range))
                  | _ -> ());
            mty.ModulesAndNamespacesByDemangledName |> Map.iter (fun _ v -> check v.ModuleOrNamespaceType) 
        try check implFileTypePriorToSig with e -> errorRecovery e m


    // Solve unsolved internal type variables 
    if (checkWeShouldContinue()) then  
        if verboseCC then dprintf "solving non-default unresolved typars...\n";

        let unsolved = Microsoft.FSharp.Compiler.FindUnsolved.unsolved_typars_of_mdef g cenv.amap envAtEnd.DisplayEnv (fragDefn,extraAttribs)

        if verboseCC then dprintf "#unsolved = %d\n" (List.length unsolved);

        unsolved |> List.iter (fun tp -> 
                if (tp.Rigidity <> TyparRigid) && not (tpref_is_solved tp) then 
                    ConstraintSolver.ChooseTyparSolutionAndSolve cenv.css envAtEnd.DisplayEnv tp);

    // Check the module matches the signature 
    let implFileExprAfterSig = 
        if verboseCC then dprintf "checking implementation meets signature...\n";
        match topRootedSigOpt with 
        | None -> 
            (* Deep copy the inferred type of the module *)
            let implFileTypePriorToSigCopied = 
                if !verboseStamps then dprintf "Compilation unit type before copy:\n%s\n" (Layout.showL (Layout.squashTo 192 (EntityTypeL implFileTypePriorToSig)));
                let res = copy_mtyp g CloneAll implFileTypePriorToSig
                if !verboseStamps then dprintf "Compilation unit type after copy:\n%s\n" (Layout.showL (Layout.squashTo 192 (EntityTypeL res)));
                res

            TMTyped(implFileTypePriorToSigCopied,fragDefn,m)
            
        | Some sigFileType -> 

            if verbose then dprintf "Compilation unit constrained type:\n%s\n" (Layout.showL (Layout.squashTo 192 (EntityTypeL sigFileType)));
 
            (* We want to show imperative type variables in any types in error messages at this late point *)
            let denv = { envAtEnd.eNameResEnv.eDisplayEnv with showImperativeTyparAnnotations=true; }
            begin 
                try 
                
                    (* As typechecked the signature and implementation use different tycons etc. *)
                    (* Here we (a) check there are enough names, (b) match them up to build a renaming and   *)
                    (* (c) check subsumption up to this renaming. *)
                    if not (SignatureConformance.CheckNamesOfModuleOrNamespace denv (mk_local_tcref implFileSpecPriorToSig) sigFileType) then 
                        raise ReportedError;

                    let remapInfo ,hidingInfo = mk_mtyp_to_mtyp_remapping implFileTypePriorToSig sigFileType
                     
                    (*
                    dprintf "implFileSpecPriorToSig = \n-----------\n%s\n\n" (showL (EntityL implFileSpecPriorToSig));
                    dprintf "modulImplTopRootedPriorToSigRemapped = \n-----------\n%s\n\n" (showL (EntityL modulImplTopRootedPriorToSigRemapped));
                    dprintf "sigFileType = \n-----------\n%s\n\n" (showL (EntityL sigFileType));
                    *)
                    
                    let aenv = { tyeq_env_empty with ae_tcrefs = tcref_map_of_list remapInfo.mrpiTycons }
                    
                    if not (SignatureConformance.CheckModuleOrNamespace cenv.g cenv.amap denv aenv (mk_local_modref implFileSpecPriorToSig) sigFileType) then  (
                        (* we can just raise 'ReportedError' since CheckModuleOrNamespace raises its own error *)
                        raise ReportedError;
                    )
                with e -> errorRecovery e m;
            end;
            if verbose then dprintf "adjusting signature...\n";
            
            TMTyped(sigFileType,TMDefs(implFileBinds),m)

    let implFile = TImplFile(qualNameOfFile,scopedPragmas, implFileExprAfterSig)
    
    // We ALWAYS run the PostTypecheckSemanticChecks phase, though we if we have already encountered some
    // errors we turn off error reporting. THis is because it performs various fixups over the TAST, e.g. 
    // assigning nice names for inference variables.
    try  
        let reportErrors = checkWeShouldContinue()
        Microsoft.FSharp.Compiler.PostTypecheckSemanticChecks.CheckTopImpl (g,cenv.amap,reportErrors,cenv.infoReader,env.eInternalsVisibleCompPaths,cenv.topCcu,envAtEnd.DisplayEnv, implFile,extraAttribs,canContainEntryPoint);
    with e -> 
        errorRecovery e m


    if verbose then dprintf "<-- TypecheckOneImplFile, nm = %s\n" implFileSpecPriorToSig.MangledName;
    return (topAttrs,implFile,envAtEnd)
 } 
   
/// Check a single namespace fragment in a signature file
let TcSigFileFragment 
        (g,niceNameGen,amap,topCcu,checkWeShouldContinue,conditionalDefines) 
        env 
        (ModuleOrNamespaceSpec(lid,isModule,defs,xml,attribs,vis,m)) = 

    if verbose then  dprintn ("Typecheck interface "^text_of_lid lid);
    let cenv = new_cenv (g,false,niceNameGen,amap,topCcu,true,false,conditionalDefines)
    

    let enclosingNamespacePath,id = List.frontAndBack lid
    let envinner = LocateEnv cenv.topCcu env enclosingNamespacePath
    let envinner = ImplicitlyOpenOwnNamespace g amap m isModule lid envinner
    let vis,_ = ComputeAccessAndCompPath envinner None id.idRange vis ParentNone
    let spec = (id,isModule,defs,xml,attribs,vis,m)
      
    let mspec,envAtEnd = TcModuleOrNamespaceSignature cenv envinner spec
    
    // Record the declKind-level definition of the module type, partly to be able to check if all signatures 
    // have corresponding implementations.... 
    let sigFileFragType = wrap_modul_as_mtyp_in_namespace (BuildTopRootedModul enclosingNamespacePath mspec)

    if (checkWeShouldContinue()) then  
        if verboseCC then dprintf "checking all virtual promises implemented...\n";
        try sigFileFragType |> IterTyconsOfModuleOrNamespaceType (FinalTypeDefinitionChecksAtEndOfInferenceScope cenv.infoReader false envAtEnd.DisplayEnv);
        with e -> errorRecovery e m 

    sigFileFragType,envAtEnd

/// Check an entire sginature file
let TypecheckOneSigFile  
       (g,niceNameGen,amap,topCcu,checkWeShouldContinue,conditionalDefines) 
       tcEnv 
       (SigFile(fileName,qualNameOfFile,_, _,sigFileFrags)) = 
 eventually {       
    // Iterate through the namespace fragments in the signature file and check each one
    let (tcEnvAtEnd,tcEnv,sigFileFragTypes) = 
       ((tcEnv,tcEnv,[]) , sigFileFrags) ||> List.fold 
           (fun // The state
                (tcEnvAtEnd,tcEnv,sigFileFragTypes) 
                // The input
                (ModuleOrNamespaceSpec(lid,_,_,_,_,_,m) as mspec) ->
              let smodulTypeTopRooted,tcEnvAtEnd = TcSigFileFragment (g,niceNameGen,amap,topCcu,checkWeShouldContinue,conditionalDefines) tcEnv mspec
              let tcEnv = AddLocalTopRootedModuleOrNamespace g amap m tcEnv smodulTypeTopRooted
              let sigFileFragTypes = sigFileFragTypes@[smodulTypeTopRooted]
              tcEnvAtEnd, tcEnv,sigFileFragTypes)
    let m = qualNameOfFile.Range
    let sigFileType = combine_mtyps [] m sigFileFragTypes
    return (tcEnvAtEnd,tcEnv,sigFileType)
 }
