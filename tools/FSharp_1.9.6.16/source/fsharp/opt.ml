// (c) Microsoft Corporation. All rights reserved
//-------------------------------------------------------------------------
// A fairly simple optimizer. The main aim is to inline simple, known functions
// and constant values, and to eliminate non-side-affecting bindings that 
// are never used.
//------------------------------------------------------------------------- 

#light

module (* internal *) Microsoft.FSharp.Compiler.Opt

open Internal.Utilities
open Internal.Utilities.Pervasives
open Microsoft.FSharp.Compiler.AbstractIL
open Microsoft.FSharp.Compiler.AbstractIL.Internal
open Microsoft.FSharp.Compiler.AbstractIL.Internal.Library
open Microsoft.FSharp.Compiler.AbstractIL.Extensions.ILX
open Microsoft.FSharp.Compiler

open Microsoft.FSharp.Compiler.AbstractIL.IL
open Microsoft.FSharp.Compiler.AbstractIL.Diagnostics

open Microsoft.FSharp.Compiler.Range
open Microsoft.FSharp.Compiler.Ast
open Microsoft.FSharp.Compiler.ErrorLogger
open Microsoft.FSharp.Compiler.PrettyNaming 
open Microsoft.FSharp.Compiler.Tast 
open Microsoft.FSharp.Compiler.Tastops
open Microsoft.FSharp.Compiler.Env
open Microsoft.FSharp.Compiler.Lib
open Microsoft.FSharp.Compiler.Layout
open Microsoft.FSharp.Compiler.Typrelns

let verboseOptimizationInfo = false
let verboseOptimizations = false

let i_ldlen = [ I_ldlen; I_arith (AI_conv DT_I4)  ] 

let callSize = 1  // size of a function call 
let forAndWhileLoopSize = 5 // size of a for/while loop
let tryCatchSize = 5  // size of a try/catch 
let tryFinallySize = 5 // size of a try/finally
let closureTotalSize = 10 // Total cost of a closure. Each closure adds a class definition
let methodDefnTotalSize = 1  // Total cost of a method definition

//-------------------------------------------------------------------------
// Info returned up the tree by optimization.
// Partial information about an expression.
// Some ExprValueInfo can 
// 
// We store one of these for each value in the environment, including values 
// which we know little or nothing about. 
//------------------------------------------------------------------------- 

type TypeValueInfo =
  | UnknownTypeValue

type ExprValueInfo =
  | UnknownValue
  /// record size info (max_depth) for ExprValueInfo 
  | SizeValue   of int * ExprValueInfo        
  (* RECURSIVE cases *)
  /// "equal to another identifier about which we know some further detail" 
  | ValValue    of ValRef * ExprValueInfo    
  /// used for  when optimizing module expressions 
  | ModuleValue of ModuleInfo              
  | TupleValue  of ExprValueInfo array 
  | RecdValue   of TyconRef * ExprValueInfo array         (* INVARIANT: in field definition order *)
  | UnionCaseValue of UnionCaseRef * ExprValueInfo array 
  (* NON RECURSIVE cases *)
  | ConstValue of Constant * Tast.typ
  | CurriedLambdaValue of
      ( uniq        (* id *)
      * int        (* arities, i.e. number of bunches of untupled args, and number of args in each bunch. INCLUDE TYPE ARGS. *)
      * int        (* size *)
      * Tast.expr  (* value, a lambda term *)
      * Tast.typ   (* type of lamba term *))
  | ConstExprValue of
      ( int        (* size *)
      * Tast.expr  (* value, a term *))

and ValInfo =
    { ValMakesNoCriticalTailcalls: bool;
      ValExprInfo: ExprValueInfo }
and ModuleInfo = 
    { ValInfos: (ValRef * ValInfo) NameMap;
      ModuleOrNamespaceInfos: LazyModuleInfo NameMap }
and LazyModuleInfo = Lazy<ModuleInfo>
let braceL x = leftL "{" $$ x $$ rightL "}"  
    
let namemapL xL xmap = NameMap.fold (fun nm x z -> z @@ xL nm x) xmap emptyL
let rec exprValueInfoL = function
  | ConstValue (x,_)         -> NicePrint.constL x
  | UnknownValue             -> wordL "?"
  | SizeValue (_,vinfo)      -> exprValueInfoL vinfo
  | ValValue (vr,vinfo)      -> bracketL ((ValRefL vr $$ wordL "alias") --- exprValueInfoL vinfo)
  | ModuleValue minfo        -> wordL "struct<...>"
  | TupleValue vinfos        -> bracketL (exprValueInfosL vinfos)
  | RecdValue (_,vinfos)     -> braceL   (exprValueInfosL vinfos)
  | UnionCaseValue (ucr,vinfos) -> UnionCaseRefL ucr $$ bracketL (exprValueInfosL vinfos)
  | CurriedLambdaValue(lambdaId,arities,bsize,expr',ety) -> wordL "lam" ++ ExprL expr' (* (Printf.sprintf "lam(size=%d)" bsize) *)
  | ConstExprValue (size,x)  -> ExprL x
and exprValueInfosL vinfos = commaListL (List.map exprValueInfoL (Array.to_list vinfos))
and moduleInfoL (x:LazyModuleInfo) = 
    let x = x.Force()
    braceL ((wordL "Modules: " @@ namemapL (fun nm x -> wordL nm $$ moduleInfoL x) x.ModuleOrNamespaceInfos) 
            @@ (wordL "Values:" @@ namemapL (fun nm (vref,x) -> ValRefL vref $$ valInfoL x) x.ValInfos))

and valInfoL (x:ValInfo) = 
    braceL ((wordL "ValExprInfo: " @@ exprValueInfoL x.ValExprInfo) 
            @@ (wordL "ValMakesNoCriticalTailcalls:" @@ wordL (if x.ValMakesNoCriticalTailcalls then "true" else "false")))

type summary<'a> =
    { Info: 'a; 
      /// What's the contribution to the size of this function?
      FunctionSize: int; 
      /// What's the total contribution to the size of the assembly, including closure classes etc.?
      TotalSize: int; 
      /// Meaning: could mutate, could non-terminate, could raise exception 
      /// One use: an effect expr can not be eliminated as dead code (e.g. sequencing)
      /// One use: an effect=false expr can not throw an exception? so try-catch is removed.
      HasEffect: bool  
      /// Indicates that a function may make a useful tailcall, hence when called should itself be tailcalled
      MightMakeCriticalTailcall: bool  
    }

type expr_summary = ExprValueInfo summary
type modul_summary = ModuleInfo summary
    
//-------------------------------------------------------------------------
// BoundValueInfoBySize
// Note, this is a different notion of "size" to the one used for inlining heuristics
//------------------------------------------------------------------------- 

let rec SizeOfValueInfos arr = 
    let n = Array.length arr
    let rec go i acc = if i >= n then acc else max acc (SizeOfValueInfo arr.[i])
    go 0 0
and SizeOfValueInfo x =
    match x with
    | SizeValue (vdepth,v)     -> vdepth (* terminate recursion at CACHED size nodes *)
    | ConstValue (x,_)                  -> 1
    | UnknownValue             -> 1
    | ValValue (vr,vinfo)      -> SizeOfValueInfo vinfo + 1
    | ModuleValue minfo        -> 1 (* do not care about size of these, they do not nest heavily... *)
    | TupleValue vinfos        
    | RecdValue (_,vinfos)
    | UnionCaseValue (_,vinfos)   -> 1 + SizeOfValueInfos vinfos
    | CurriedLambdaValue(lambdaId,arities,bsize,expr',ety) -> 1
    | ConstExprValue (size,_)  -> 1

let rec MakeValueInfoWithCachedSize vdepth v =
    match v with
      | SizeValue(_,v) -> MakeValueInfoWithCachedSize vdepth v
      | _ -> let minDepthForASizeNode = 5 in (* for small vinfos do not record size info, save space *)
             if vdepth > minDepthForASizeNode then SizeValue(vdepth,v) else v (* add nodes to stop recursion *)
    
let MakeSizedValueInfo v =
    let vdepth = SizeOfValueInfo v
    MakeValueInfoWithCachedSize vdepth v

let BoundValueInfoBySize vinfo =
    let rec bound depth x =
        if depth<0 then UnknownValue else
        match x with
        | SizeValue (vdepth,vinfo) -> if vdepth < depth then x else MakeSizedValueInfo (bound depth vinfo)
        | ValValue (vr,vinfo)      -> ValValue (vr,bound (depth-1) vinfo)
        | TupleValue vinfos        -> TupleValue (Array.map (bound (depth-1)) vinfos)
        | RecdValue (tcref,vinfos) -> RecdValue  (tcref,Array.map (bound (depth-1)) vinfos)
        | UnionCaseValue (ucr,vinfos) -> UnionCaseValue (ucr,Array.map (bound (depth-1)) vinfos)
        | ModuleValue minfo        -> x
        | ConstValue _                  -> x
        | UnknownValue             -> x
        | CurriedLambdaValue(lambdaId,arities,bsize,expr',ety) -> x
        | ConstExprValue (size,_)  -> x
    let max_depth  = 6 in  (* beware huge constants! *)
    let trim_depth = 3
    let vdepth = SizeOfValueInfo vinfo
    if vdepth > max_depth 
    then MakeSizedValueInfo (bound trim_depth vinfo)
    else MakeValueInfoWithCachedSize vdepth vinfo

//-------------------------------------------------------------------------
// What we know about the world 
//------------------------------------------------------------------------- 

let jitOptDefault = true
let localOptDefault = true
let crossModuleOptDefault = true

type OptimizationSettings = 
    { abstractBigTargets : bool;
      jitOptUser : bool option;
      localOptUser : bool option;
      crossModuleOptUser : bool option; 
      /// size after which we start chopping methods in two, though only at match targets 
      bigTargetSize : int   
      /// size after which we start enforcing splitting sub-expressions to new methods, to avoid hitting .NET IL limitations 
      veryBigExprSize : int 
      /// The size after which we don't inline
      lambdaInlineThreshold : int;  
      /// For unit testing
      reportingPhase : bool
      reportNoNeedToTailcall: bool;
      reportFunctionSizes : bool 
      reportHasEffect : bool 
      reportTotalSizes : bool }

    static member Defaults = 
        { abstractBigTargets = false;
          jitOptUser = None;
          localOptUser = None
          /// size after which we start chopping methods in two, though only at match targets 
          bigTargetSize = 100  
          /// size after which we start enforcing splitting sub-expressions to new methods, to avoid hitting .NET IL limitations 
          veryBigExprSize = 3000 
          crossModuleOptUser = None;
          /// The size after which we don't inline
          lambdaInlineThreshold = 6;
          reportingPhase = false;
          reportNoNeedToTailcall = false;
          reportFunctionSizes = false
          reportHasEffect = false
          reportTotalSizes = false
        }

    member x.jitOpt() = (match x.jitOptUser with Some f -> f | None -> jitOptDefault)
    member x.localOpt () = (match x.localOptUser with Some f -> f | None -> localOptDefault)
    member x.crossModuleOpt () = x.localOpt () && (match x.crossModuleOptUser with Some f -> f | None -> crossModuleOptDefault)

    member x.KeepOptimizationValues() = x.crossModuleOpt ()
    /// inline calls *
    member x.InlineLambdas () = x.localOpt ()  
    /// eliminate unused bindings with no effect 
    member x.EliminateUnusedBindings () = x.localOpt () 
    /// eliminate try around expr with no effect 
    member x.EliminateTryCatchAndTryFinally () = x.localOpt () 
    /// eliminate first part of seq if no effect 
    member x.EliminateSequential () = x.localOpt () 
    /// determine branches in pattern matching
    member x.EliminateSwitch () = x.localOpt () 
    member x.EliminateRecdFieldGet () = x.localOpt () 
    member x.EliminateTupleFieldGet () = x.localOpt () 
    member x.EliminatUnionCaseFieldGet () = x.localOpt () 
    /// eliminate non-copiler generated immediate bindings 
    member x.EliminateImmediatelyConsumedLocals() = x.localOpt () 
    /// expand "let x = (exp1,exp2,...)" bind fields as prior tmps 
    member x.ExpandStructrualValues() = x.localOpt () 

type cenv =
    { g: Env.TcGlobals;
      amap: Import.ImportMap;
      optimizing: bool;
      scope: ccu; 
      localInternalVals: System.Collections.Generic.Dictionary<stamp,ValInfo> 
      settings: OptimizationSettings }



type IncrementalOptimizationEnv =
    { // An identifier to help with name generation
      latestBoundId: ident option;
      // The set of lambda IDs we've inlined to reach this point. Helps to prevent recursive inlining 
      dontInline: Zset.t<uniq>;  
      // Recursively bound vars. If an sub-expression that is a candidate for method splitting
      // contains any of these variables then don't split it, for fear of mucking up tailcalls.
      // See FSharp 1.0 bug 2892
      dontSplitVars: ValMap<unit>;  
      /// The Val for the function binding being generated, if any. 
      functionVal: (Val * Tast.ValTopReprInfo) option;
      typarInfos: (Typar * TypeValueInfo) list; 
      localExternalVals: ValInfo ValMap;
      globalModuleInfos: LazyModuleInfo NameMap;   }

let empty_env = 
    { latestBoundId = None; 
      dontInline = Zset.empty Int64.order;
      typarInfos = [];
      functionVal = None; 
      dontSplitVars = vspec_map_empty();
      localExternalVals = vspec_map_empty(); 
      globalModuleInfos = NameMap.empty }

//-------------------------------------------------------------------------
// IsPartialExprVal - is the expr fully known?
//------------------------------------------------------------------------- 

let rec IsPartialExprVal x = (* IsPartialExprVal can not rebuild to an expr *)
    match x with
    | UnknownValue -> true
    | ModuleValue ss -> IsPartialStructVal ss
    | TupleValue args | RecdValue (_,args) | UnionCaseValue (_,args) -> Array.exists IsPartialExprVal args
    | ConstValue _ | CurriedLambdaValue _ | ConstExprValue _ -> false
    | ValValue (_,a) 
    | SizeValue(_,a) -> IsPartialExprVal a

and IsPartialStructVal (ss:ModuleInfo) =
    (ss.ModuleOrNamespaceInfos  |> Map.exists (fun _ x -> IsPartialStructVal (x.Force()))) ||
    (ss.ValInfos |> Map.exists (fun _ (_,x) -> IsPartialExprVal x.ValExprInfo)) 

let CheckInlineValueIsComplete (v:Val) res =
    if v.MustInline && IsPartialExprVal res then
        errorR(Error("The value '"^v.MangledName^"' was marked inline but its value was incomplete", v.Range))
        //System.Diagnostics.Debug.Assert(false,sprintf "Break for incomplete inline value %s" v.MangledName)

let check msg m vref (res:ValInfo)  =
    CheckInlineValueIsComplete (deref_val vref) res.ValExprInfo;
    (vref,res)

//-------------------------------------------------------------------------
// Bind information about values 
//------------------------------------------------------------------------- 

let EmptyModuleInfo = notlazy { ValInfos = Map.empty; ModuleOrNamespaceInfos = Map.empty }
let rec UnionModuleInfo (m1:LazyModuleInfo) (m2:LazyModuleInfo) = 
    let m1 = m1.Force()
    let m2 = m2.Force()
    notlazy
       { ValInfos =  NameMap.layer m1.ValInfos m2.ValInfos;
         ModuleOrNamespaceInfos = NameMap.union UnionModuleInfo  m1.ModuleOrNamespaceInfos m2.ModuleOrNamespaceInfos }

let UnionModuleInfos (minfos : LazyModuleInfo list) = List.foldBack UnionModuleInfo minfos EmptyModuleInfo

let FindOrCreateModuleInfo n ss = 
    match Map.tryfind n ss with 
    | Some res -> res
    | None -> EmptyModuleInfo

let rec BindValueInSubModule mp i (v:Val) vval ss =
    if i >= Array.length mp 
    then {ss with ValInfos = Map.add v.MangledName (mk_local_vref v,vval) ss.ValInfos }
    else {ss with ModuleOrNamespaceInfos = BindValueInModule mp.[i] mp (i+1) v vval ss.ModuleOrNamespaceInfos }

and BindValueInModule n mp i v vval ss =
    let old =  FindOrCreateModuleInfo n ss
    Map.add n (notlazy (BindValueInSubModule mp i v vval (old.Force()))) ss

let bind_val_in_env_for_fslib (NLPath(ccu,mp)) v vval env =
    // We eventually need to allow multiple CCUs with the same 'name' but different assemblies.
    // So at some point we should remove this use of ccu.AssemblyName
    {env with globalModuleInfos = BindValueInModule ccu.AssemblyName mp 0 v vval env.globalModuleInfos }

let rec  bind_top_module_in_modul n mp i mval ss =
    if i >= Array.length mp 
    then 
        match Map.tryfind n ss with 
        | Some res -> Map.add n (UnionModuleInfo mval res) ss
        | None -> Map.add n mval ss
    else
        let old =  (FindOrCreateModuleInfo n ss).Force()
        Map.add n (notlazy {old with ModuleOrNamespaceInfos =  bind_top_module_in_modul mp.[i] mp (i+1) mval old.ModuleOrNamespaceInfos}) ss

let UnknownValInfo = { ValExprInfo=UnknownValue; ValMakesNoCriticalTailcalls=false }

let MkValInfo info (v:Val)  = { ValExprInfo=info.Info; ValMakesNoCriticalTailcalls= v.MakesNoCriticalTailcalls }

(* Bind a value *)
let bind_internal_local_vspec cenv (v:Val) vval env = 
    let vval = if v.IsMutable then UnknownValInfo else vval
#if CHECKED
#else
    match vval.ValExprInfo with 
    | UnknownValue -> env
    | _ -> 
#endif
        cenv.localInternalVals.[v.Stamp] <- vval;
        env
        
let bind_escaping_local_vspec cenv (v:Val) vval env = 
#if CHECKED
    CheckInlineValueIsComplete v vval;
#endif

    if verboseOptimizationInfo then dprintn ("*** Binding "^v.MangledName); 
    let vval = if v.IsMutable then {vval with ValExprInfo=UnknownValue } else vval
    let env = 
#if CHECKED
#else
        match vval.ValExprInfo with 
        | UnknownValue -> env  
        | _ -> 
#endif
            { env with localExternalVals=vspec_map_add v vval env.localExternalVals }
    (* If we're compiling fslib then also bind the value as a non-local path to allow us to resolve the compiler-non-local-refereneces *)
    let env = 
        if cenv.g.compilingFslib && isSome (v.PublicPath) 
        then bind_val_in_env_for_fslib (enclosing_nlpath_of_pubpath cenv.g.fslibCcu (the (v.PublicPath))) v vval env 
        else env
    env

let rec bind_module_vspecs cenv (mval:LazyModuleInfo) env =
    let mval = mval.Force()
    NameMap.foldRange (fun (v,vval) env -> bind_escaping_local_vspec cenv (deref_val v) vval env) mval.ValInfos
      (NameMap.foldRange  (bind_module_vspecs cenv) mval.ModuleOrNamespaceInfos env)


let bind_internal_vspec_to_unknown cenv v env = 
#if CHECKED
    bind_internal_local_vspec cenv v UnknownValue env
#else
    env
#endif
let bind_internal_vspecs_to_unknown cenv vs env = 
#if CHECKED
    List.foldBack (bind_internal_vspec_to_unknown cenv) vs env
#else
    env
#endif

let BindTypeVar tyv typeinfo env = { env with typarInfos= (tyv,typeinfo)::env.typarInfos } 

let BindTypeVarsToUnknown (tps:Typar list) env = 
    if isNil tps then env else
    // The optimizer doesn't use the type values it could track. 
    // However here we mutate to provide better names for generalized type parameters 
    let nms = PrettyTypes.PrettyTyparNames (fun _ -> true) (env.typarInfos |> List.map (fun (tp,_) -> tp.Name) ) tps
    (tps,nms) ||> List.iter2 (fun tp nm -> 
            if PrettyTypes.NeedsPrettyTyparName tp  then 
                tp.Data.typar_id <- ident (nm,tp.Range));      
    List.fold (fun sofar arg -> BindTypeVar arg UnknownTypeValue sofar) env tps 

let BindCcu (ccu:Tast.ccu) mval env = 
    if verboseOptimizationInfo then 
        dprintf "*** Reloading optimization data for assembly %s, info = \n%s\n" ccu.AssemblyName (showL (Layout.squashTo 192 (moduleInfoL mval)));  

    { env with globalModuleInfos=Map.add ccu.AssemblyName mval env.globalModuleInfos }

let mk_cenv settings scope g amap = 
    { settings=settings;
      scope=scope; 
      g=g; 
      amap=amap;
      optimizing=true;
      localInternalVals=new System.Collections.Generic.Dictionary<_,_>(10000) }


//-------------------------------------------------------------------------
// Lookup information about values 
//------------------------------------------------------------------------- 


let GetInfoForLocalValue cenv env (v:Val) m = 
    (* Abstract slots do not have values *)
    match v.MemberInfo with 
    | Some(vspr) when vspr.MemberFlags.MemberIsDispatchSlot -> UnknownValInfo
    | _ -> 
        let mutable res = Unchecked.defaultof<_> 
        let ok = cenv.localInternalVals.TryGetValue(v.Stamp, &res)
        if ok then res else
        match vspec_map_tryfind v env.localExternalVals with 
        | Some vval -> vval
        | None -> 
            if v.MustInline then
                errorR(Error("The value '"^full_display_text_of_vref (mk_local_vref v) ^"' was marked inline but was not bound in the optimization environment", m));
#if CHECKED
            warning(Error ("*** Local value "^v.MangledName^" not found during optimization. Please report this problem",m)); 
#endif
            UnknownValInfo 

let TryGetInfoForCcu env (ccu:ccu) = env.globalModuleInfos.TryFind(ccu.AssemblyName)

let rec TryGetInfoForPath sv p i = 
    if i >= Array.length p then Some sv else 
    match Map.tryfind p.[i] sv.ModuleOrNamespaceInfos with 
    | Some info -> 
        TryGetInfoForPath (info.Force()) p (i+1)
    | None -> 
        if verboseOptimizationInfo then 
            dprintn ("\n\n*** Optimization info for submodule "^p.[i]^" not found in parent module which contains submodules: "^String.concat "," (NameMap.domainL sv.ModuleOrNamespaceInfos)); 
        None

let TryGetInfoForNonLocalPath env (NLPath(ccu,p)) = 
    match TryGetInfoForCcu env ccu with 
    | Some ccuinfo -> TryGetInfoForPath (ccuinfo.Force()) p 0
    | None -> None
          
let GetInfoForNonLocalVal cenv env (v:ValRef) =
    match v.MemberInfo with 
    | Some(vspr) when vspr.MemberFlags.MemberIsDispatchSlot -> UnknownValInfo
    | _ -> 
        if (* in_this: REVIEW: optionally turn x-module on/off on per-module basis  or  *)
          cenv.settings.crossModuleOpt () || 
          v.MustInline then 
            let smv = nlpath_of_nlref v.nlr
            let n = item_of_nlref v.nlr
            match TryGetInfoForNonLocalPath env smv with
            | Some(structInfo) ->
                match structInfo.ValInfos.TryFind(n) with 
                | Some ninfo -> snd ninfo
                | None -> 
                      //dprintn ("\n\n*** Optimization info for value "^n^" from module "^(full_name_of_nlpath smv)^" not found, module contains values: "^String.concat "," (NameMap.domainL structInfo.ValInfos));  
                      //System.Diagnostics.Debug.Assert(false,sprintf "Break for module %s, value %s" (full_name_of_nlpath smv) n)
                      UnknownValInfo
            | None -> 
                //dprintf "\n\n*** Optimization info for module %s from ccu %s not found." (full_name_of_nlpath smv) (ccu_of_nlpath smv).AssemblyName;  
                //System.Diagnostics.Debug.Assert(false,sprintf "Break for module %s, ccu %s" (full_name_of_nlpath smv) (ccu_of_nlpath smv).AssemblyName)
                UnknownValInfo
        else UnknownValInfo

let GetInfoForVal cenv env m (v:ValRef) =  
    let res = 
        match v.IsLocalRef with 
        | true -> GetInfoForLocalValue cenv env v.binding m
        | false -> GetInfoForNonLocalVal cenv env v
    check "its stored value was incomplete" m v res |> ignore;
    res

//-------------------------------------------------------------------------
// Try to get information about values of particular types
//------------------------------------------------------------------------- 

let rec strip_value = function
  | ValValue(_,details) -> strip_value details (* step through ValValue "aliases" *) 
  | SizeValue(_,details) -> strip_value details (* step through SizeValue "aliases" *) 
  | vinfo               -> vinfo

let (|StripConstValue|_|) ev = 
  match strip_value ev with
  | ConstValue(c,_) -> Some c
  | _ -> None

let (|StripLambdaValue|_|) ev = 
  match strip_value ev with 
  | CurriedLambdaValue info -> Some info
  | _ -> None

let dest_tuple_value ev = 
  match strip_value ev with 
  | TupleValue info -> Some info
  | _ -> None

let dest_recd_value ev = 
  match strip_value ev with 
  | RecdValue (tcref,info) -> Some info
  | _ -> None

let (|StripUnionCaseValue|_|) ev = 
  match strip_value ev with 
  | UnionCaseValue (c,info) -> Some (c,info)
  | _ -> None

let mk_bool_value g n = ConstValue(TConst_bool n, g.bool_ty)
let mk_int8_value g n = ConstValue(TConst_sbyte n, g.sbyte_ty)
let mk_int16_value g n = ConstValue(TConst_int16 n, g.int16_ty)
let mk_int32_value g n = ConstValue(TConst_int32 n, g.int32_ty)
let mk_int64_value g n = ConstValue(TConst_int64 n, g.int64_ty)
let mk_uint8_value g n = ConstValue(TConst_byte n, g.byte_ty)
let mk_uint16_value g n = ConstValue(TConst_uint16 n, g.uint16_ty)
let mk_uint32_value g n = ConstValue(TConst_uint32 n, g.uint32_ty)
let mk_uint64_value g n = ConstValue(TConst_uint64 n, g.uint64_ty)

let (|StripInt32Value|_|) = function StripConstValue(TConst_int32 n) -> Some n | _ -> None
      
//-------------------------------------------------------------------------
// mk value_infos
//------------------------------------------------------------------------- 

let MakeValueInfoForValue g m vref vinfo            = 
    let rec check x = 
        match x with 
        | ValValue (vref2,detail)  -> if g.vref_eq vref vref2 then error(Error("recursive ValValue "^showL(exprValueInfoL vinfo),m)) else check detail
        | SizeValue (n,detail) -> check detail
        | _ -> ()
    check vinfo;
    ValValue (vref,vinfo)       |> BoundValueInfoBySize

let MakeValueInfoForRecord tcref tyargs argvals = RecdValue (tcref,argvals)   |> BoundValueInfoBySize
let MakeValueInfoForTuple argvals               = TupleValue argvals          |> BoundValueInfoBySize
let MakeValueInfoForUnionCase cspec argvals     = UnionCaseValue (cspec,argvals) |> BoundValueInfoBySize
let MakeValueInfoForConst c ty                  = ConstValue(c,ty)

// Helper to evaluate a unary integer operation over known values
let inline IntegerUnaryOp g f8 f16 f32 f64 fu8 fu16 fu32 fu64 a = 
     match a with
     | StripConstValue(c) -> 
         match c with 
         | TConst_bool  a -> Some(mk_bool_value g (f32 (if a then 1 else 0) <> 0))
         | TConst_int32  a -> Some(mk_int32_value g (f32 a))
         | TConst_int64  a -> Some(mk_int64_value g (f64 a))
         | TConst_int16  a -> Some(mk_int16_value g (f16 a))
         | TConst_sbyte   a  -> Some(mk_int8_value g (f8 a))
         | TConst_byte  a  -> Some(mk_uint8_value g (fu8 a))
         | TConst_uint32 a -> Some(mk_uint32_value g (fu32 a))
         | TConst_uint64 a -> Some(mk_uint64_value g (fu64 a))
         | TConst_uint16 a -> Some(mk_uint16_value g (fu16 a))
         | _ -> None
     | _ -> None

// Helper to evaluate a unary signed integer operation over known values
let inline SignedIntegerUnaryOp g f8 f16 f32 f64 a = 
     match a with
     | StripConstValue(c) -> 
         match c with 
         | TConst_int32 a -> Some(mk_int32_value g (f32 a))
         | TConst_int64 a -> Some(mk_int64_value g (f64 a))
         | TConst_int16 a -> Some(mk_int16_value g (f16 a))
         | TConst_sbyte  a -> Some(mk_int8_value g (f8 a))
         | _ -> None
     | _ -> None
         
// Helper to evaluate a binary integer operation over known values
let inline IntegerBinaryOp g f8 f16 f32 f64 fu8 fu16 fu32 fu64 a b = 
     match a,b with
     | StripConstValue(c1),StripConstValue(c2) -> 
         match c1,c2 with 
         | (TConst_bool a),(TConst_bool  b) -> Some(mk_bool_value  g (f32  (if a then 1 else 0) (if b then 1 else 0) <> 0))
         | (TConst_int32  a),(TConst_int32  b) -> Some(mk_int32_value  g (f32  a b))
         | (TConst_int64  a),(TConst_int64  b) -> Some(mk_int64_value  g (f64  a b))
         | (TConst_int16  a),(TConst_int16  b) -> Some(mk_int16_value  g (f16  a b))
         | (TConst_sbyte   a),(TConst_sbyte   b) -> Some(mk_int8_value   g (f8   a b))
         | (TConst_byte  a),(TConst_byte  b) -> Some(mk_uint8_value  g (fu8  a b))
         | (TConst_uint16 a),(TConst_uint16 b) -> Some(mk_uint16_value g (fu16 a b))
         | (TConst_uint32 a),(TConst_uint32 b) -> Some(mk_uint32_value g (fu32 a b))
         | (TConst_uint64 a),(TConst_uint64 b) -> Some(mk_uint64_value g (fu64 a b))
         | _ -> None
     | _ -> None

module Unchecked = Microsoft.FSharp.Core.Operators
         
/// Evaluate primitives based on interpretation of IL instructions. 
//
// The implementation
// utilizes F# arithmetic extensively, so a mistake in the implementation of F# arithmetic  
// in the core library used by the F# compiler will propagate to be a mistake in optimization. 
// The IL instructions appear in the tree through inlining.
let MakeAssemblyCodeValueInfo g instrs argvals tys =
  match instrs,argvals,tys with
    | [ I_arith AI_add ],[t1;t2],_ -> 
         // Note: each use of Unchecked.(+) gets instantiated at a different type and inlined
         match IntegerBinaryOp g Unchecked.(+) Unchecked.(+) Unchecked.(+) Unchecked.(+) Unchecked.(+) Unchecked.(+) Unchecked.(+) Unchecked.(+) t1 t2 with 
         | Some res -> res
         | _ -> UnknownValue
    | [ I_arith AI_sub ],[t1;t2],_ -> 
         // Note: each use of Unchecked.(+) gets instantiated at a different type and inlined
         match IntegerBinaryOp g Unchecked.(-) Unchecked.(-) Unchecked.(-) Unchecked.(-) Unchecked.(-) Unchecked.(-) Unchecked.(-) Unchecked.(-) t1 t2 with 
         | Some res -> res
         | _ -> UnknownValue
    | [ I_arith AI_mul ],[a;b],_ -> (match IntegerBinaryOp g Unchecked.( * )  Unchecked.( * ) Unchecked.( * )  Unchecked.( * ) Unchecked.( * )  Unchecked.( * ) Unchecked.( * )  Unchecked.( * ) a b with Some res -> res | None -> UnknownValue)
    | [ I_arith AI_and ],[a;b],_ -> (match IntegerBinaryOp g (&&&) (&&&) (&&&) (&&&) (&&&) (&&&) (&&&) (&&&) a b  with Some res -> res | None -> UnknownValue)
    | [ I_arith AI_or  ],[a;b],_ -> (match IntegerBinaryOp g (|||) (|||) (|||) (|||) (|||) (|||) (|||) (|||) a b  with Some res -> res | None -> UnknownValue)
    | [ I_arith AI_xor ],[a;b],_ -> (match IntegerBinaryOp g (^^^) (^^^) (^^^) (^^^) (^^^) (^^^) (^^^) (^^^) a b  with Some res -> res | None -> UnknownValue)
    | [ I_arith AI_not ],[a],_ -> (match IntegerUnaryOp g (~~~) (~~~) (~~~) (~~~) (~~~) (~~~) (~~~) (~~~) a with Some res -> res | None -> UnknownValue)
    | [ I_arith AI_neg ],[a],_ -> (match SignedIntegerUnaryOp g (~-) (~-) (~-) (~-) a with Some res -> res | None -> UnknownValue)

    | [ I_arith AI_ceq ],[a;b],_ -> 
       match strip_value a, strip_value b with
       | ConstValue(TConst_bool   a1,_),ConstValue(TConst_bool   a2,_)  -> mk_bool_value g (a1 = a2)
       | ConstValue(TConst_sbyte  a1,_),ConstValue(TConst_sbyte  a2,_) -> mk_bool_value g (a1 = a2)
       | ConstValue(TConst_int16  a1,_),ConstValue(TConst_int16  a2,_)  -> mk_bool_value g (a1 = a2)
       | ConstValue(TConst_int32  a1,_),ConstValue(TConst_int32  a2,_)  -> mk_bool_value g (a1 = a2)
       | ConstValue(TConst_int64  a1,_),ConstValue(TConst_int64  a2,_)  -> mk_bool_value g (a1 = a2)
       | ConstValue(TConst_char   a1,_),ConstValue(TConst_char   a2,_)  -> mk_bool_value g (a1 = a2)
       | ConstValue(TConst_byte   a1,_),ConstValue(TConst_byte   a2,_)   -> mk_bool_value g (a1 = a2)
       | ConstValue(TConst_uint16 a1,_),ConstValue(TConst_uint16 a2,_)  -> mk_bool_value g (a1 = a2)
       | ConstValue(TConst_uint32 a1,_),ConstValue(TConst_uint32 a2,_)  -> mk_bool_value g (a1 = a2)
       | ConstValue(TConst_uint64 a1,_),ConstValue(TConst_uint64 a2,_)  -> mk_bool_value g (a1 = a2)
       | _ -> UnknownValue
    | [ I_arith AI_clt ],[a;b],_ -> 
       match strip_value a,strip_value b with
       | ConstValue(TConst_bool  a1,_),ConstValue(TConst_bool  a2,_) -> mk_bool_value g (a1 < a2)
       | ConstValue(TConst_int32 a1,_),ConstValue(TConst_int32 a2,_) -> mk_bool_value g (a1 < a2)
       | ConstValue(TConst_int64 a1,_),ConstValue(TConst_int64 a2,_) -> mk_bool_value g (a1 < a2)
       | ConstValue(TConst_sbyte a1,_),ConstValue(TConst_sbyte a2,_) -> mk_bool_value g (a1 < a2)
       | ConstValue(TConst_int16 a1,_),ConstValue(TConst_int16 a2,_) -> mk_bool_value g (a1 < a2)
       | _ -> UnknownValue
    | [ I_arith (AI_conv(DT_U1))],[a],[ty] when type_equiv g ty g.byte_ty -> 
       match strip_value a with
       | ConstValue(TConst_sbyte  a,_) -> mk_uint8_value g (Unchecked.byte a)
       | ConstValue(TConst_int16  a,_) -> mk_uint8_value g (Unchecked.byte a)
       | ConstValue(TConst_int32  a,_) -> mk_uint8_value g (Unchecked.byte a)
       | ConstValue(TConst_int64  a,_) -> mk_uint8_value g (Unchecked.byte a)
       | ConstValue(TConst_byte   a,_) -> mk_uint8_value g (Unchecked.byte a)
       | ConstValue(TConst_uint16 a,_) -> mk_uint8_value g (Unchecked.byte a)
       | ConstValue(TConst_uint32 a,_) -> mk_uint8_value g (Unchecked.byte a)
       | ConstValue(TConst_uint64 a,_) -> mk_uint8_value g (Unchecked.byte a)
       | _ -> UnknownValue
    | [ I_arith (AI_conv(DT_U2))],[a],[ty] when type_equiv g ty g.uint16_ty -> 
       match strip_value a with
       | ConstValue(TConst_sbyte   a,_) -> mk_uint16_value g (Unchecked.uint16 a)
       | ConstValue(TConst_int16  a,_) -> mk_uint16_value g (Unchecked.uint16 a)
       | ConstValue(TConst_int32  a,_) -> mk_uint16_value g (Unchecked.uint16 a)
       | ConstValue(TConst_int64  a,_) -> mk_uint16_value g (Unchecked.uint16 a)
       | ConstValue(TConst_byte  a,_) -> mk_uint16_value g (Unchecked.uint16 a)
       | ConstValue(TConst_uint16 a,_) -> mk_uint16_value g (Unchecked.uint16 a)
       | ConstValue(TConst_uint32 a,_) -> mk_uint16_value g (Unchecked.uint16 a)
       | ConstValue(TConst_uint64 a,_) -> mk_uint16_value g (Unchecked.uint16 a)
       | _ -> UnknownValue
    | [ I_arith (AI_conv(DT_U4))],[a],[ty] when type_equiv g ty g.uint32_ty -> 
       match strip_value a with
       | ConstValue(TConst_sbyte   a,_) -> mk_uint32_value g (Unchecked.uint32 a)
       | ConstValue(TConst_int16  a,_) -> mk_uint32_value g (Unchecked.uint32 a)
       | ConstValue(TConst_int32  a,_) -> mk_uint32_value g (Unchecked.uint32 a)
       | ConstValue(TConst_int64  a,_) -> mk_uint32_value g (Unchecked.uint32 a)
       | ConstValue(TConst_byte  a,_) -> mk_uint32_value g (Unchecked.uint32 a)
       | ConstValue(TConst_uint16 a,_) -> mk_uint32_value g (Unchecked.uint32 a)
       | ConstValue(TConst_uint32 a,_) -> mk_uint32_value g (Unchecked.uint32 a)
       | ConstValue(TConst_uint64 a,_) -> mk_uint32_value g (Unchecked.uint32 a)
       | _ -> UnknownValue
    | [ I_arith (AI_conv(DT_U8))],[a],[ty] when type_equiv g ty g.uint64_ty  -> 
       match strip_value a with
       | ConstValue(TConst_sbyte   a,_) -> mk_uint64_value g (Unchecked.uint64 a)
       | ConstValue(TConst_int16  a,_) -> mk_uint64_value g (Unchecked.uint64 a)
       | ConstValue(TConst_int32  a,_) -> mk_uint64_value g (Unchecked.uint64 a)
       | ConstValue(TConst_int64  a,_) -> mk_uint64_value g (Unchecked.uint64 a)
       | ConstValue(TConst_byte  a,_) -> mk_uint64_value g (Unchecked.uint64 a)
       | ConstValue(TConst_uint16 a,_) -> mk_uint64_value g (Unchecked.uint64 a)
       | ConstValue(TConst_uint32 a,_) -> mk_uint64_value g (Unchecked.uint64 a)
       | ConstValue(TConst_uint64 a,_) -> mk_uint64_value g (Unchecked.uint64 a)
       | _ -> UnknownValue
    | [ I_arith (AI_conv(DT_I1))],[a],[ty] when type_equiv g ty g.sbyte_ty  -> 
       match strip_value a with
       | ConstValue(TConst_sbyte   a,_) -> mk_int8_value g (Unchecked.sbyte a)
       | ConstValue(TConst_int16  a,_) -> mk_int8_value g (Unchecked.sbyte a)
       | ConstValue(TConst_int32  a,_) -> mk_int8_value g (Unchecked.sbyte a)
       | ConstValue(TConst_int64  a,_) -> mk_int8_value g (Unchecked.sbyte a)
       | ConstValue(TConst_byte  a,_) -> mk_int8_value g (Unchecked.sbyte a)
       | ConstValue(TConst_uint16 a,_) -> mk_int8_value g (Unchecked.sbyte a)
       | ConstValue(TConst_uint32 a,_) -> mk_int8_value g (Unchecked.sbyte a)
       | ConstValue(TConst_uint64 a,_) -> mk_int8_value g (Unchecked.sbyte a)
       | _ -> UnknownValue
    | [ I_arith (AI_conv(DT_I2))],[a],[ty] when type_equiv g ty g.int16_ty  -> 
       match strip_value a with
       | ConstValue(TConst_int32  a,_) -> mk_int16_value g (Unchecked.int16 a)
       | ConstValue(TConst_int16  a,_) -> mk_int16_value g (Unchecked.int16 a)
       | ConstValue(TConst_sbyte   a,_) -> mk_int16_value g (Unchecked.int16 a)
       | ConstValue(TConst_int64  a,_) -> mk_int16_value g (Unchecked.int16 a)
       | ConstValue(TConst_uint32 a,_) -> mk_int16_value g (Unchecked.int16 a)
       | ConstValue(TConst_uint16 a,_) -> mk_int16_value g (Unchecked.int16 a)
       | ConstValue(TConst_byte  a,_) -> mk_int16_value g (Unchecked.int16 a)
       | ConstValue(TConst_uint64 a,_) -> mk_int16_value g (Unchecked.int16 a)
       | _ -> UnknownValue
    | [ I_arith (AI_conv(DT_I4))],[a],[ty] when type_equiv g ty g.int32_ty -> 
       match strip_value a with
       | ConstValue(TConst_int32  a,_) -> mk_int32_value g (Unchecked.int32 a)
       | ConstValue(TConst_int16  a,_) -> mk_int32_value g (Unchecked.int32 a)
       | ConstValue(TConst_sbyte   a,_) -> mk_int32_value g (Unchecked.int32 a)
       | ConstValue(TConst_int64  a,_) -> mk_int32_value g (Unchecked.int32 a)
       | ConstValue(TConst_uint32 a,_) -> mk_int32_value g (Unchecked.int32 a)
       | ConstValue(TConst_uint16 a,_) -> mk_int32_value g (Unchecked.int32 a)
       | ConstValue(TConst_byte  a,_) -> mk_int32_value g (Unchecked.int32 a)
       | ConstValue(TConst_uint64 a,_) -> mk_int32_value g (Unchecked.int32 a)
       | _ -> UnknownValue
    | [ I_arith (AI_conv(DT_I8))],[a],[ty] when type_equiv g ty g.int64_ty  -> 
       match strip_value a with
       | ConstValue(TConst_int32  a,_) -> mk_int64_value g (Unchecked.int64 a)
       | ConstValue(TConst_int16  a,_) -> mk_int64_value g (Unchecked.int64 a)
       | ConstValue(TConst_sbyte  a,_) -> mk_int64_value g (Unchecked.int64 a)
       | ConstValue(TConst_int64  a,_) -> mk_int64_value g (Unchecked.int64 a)
       | ConstValue(TConst_uint32 a,_) -> mk_int64_value g (Unchecked.int64 a)
       | ConstValue(TConst_uint16 a,_) -> mk_int64_value g (Unchecked.int64 a)
       | ConstValue(TConst_byte   a,_) -> mk_int64_value g (Unchecked.int64 a)
       | ConstValue(TConst_uint64 a,_) -> mk_int64_value g (Unchecked.int64 a)
       | _ -> UnknownValue
    | [ I_arith AI_clt_un ],[a;b],[ty] when type_equiv g ty g.bool_ty  -> 
       match strip_value a,strip_value b with
       | ConstValue(TConst_char   a1,_),ConstValue(TConst_char   a2,_) -> mk_bool_value g (a1 < a2)
       | ConstValue(TConst_byte   a1,_),ConstValue(TConst_byte  a2,_)  -> mk_bool_value g (a1 < a2)
       | ConstValue(TConst_uint16 a1,_),ConstValue(TConst_uint16 a2,_) -> mk_bool_value g (a1 < a2)
       | ConstValue(TConst_uint32 a1,_),ConstValue(TConst_uint32 a2,_) -> mk_bool_value g (a1 < a2)
       | ConstValue(TConst_uint64 a1,_),ConstValue(TConst_uint64 a2,_) -> mk_bool_value g (a1 < a2)
       | _ -> UnknownValue
    | [ I_arith AI_cgt ],[a;b],[ty] when type_equiv g ty g.bool_ty  -> 
       match strip_value a,strip_value b with
       | ConstValue(TConst_sbyte a1,_),ConstValue(TConst_sbyte  a2,_) -> mk_bool_value g (a1 > a2)
       | ConstValue(TConst_int16 a1,_),ConstValue(TConst_int16 a2,_)  -> mk_bool_value g (a1 > a2)
       | ConstValue(TConst_int32 a1,_),ConstValue(TConst_int32 a2,_)  -> mk_bool_value g (a1 > a2)
       | ConstValue(TConst_int64 a1,_),ConstValue(TConst_int64 a2,_)  -> mk_bool_value g (a1 > a2)
       | _ -> UnknownValue
    | [ I_arith AI_cgt_un ],[a;b],[ty] when type_equiv g ty g.bool_ty   -> 
       match strip_value a,strip_value b with
       | ConstValue(TConst_char   a1,_),ConstValue(TConst_char   a2,_) -> mk_bool_value g (a1 > a2)
       | ConstValue(TConst_byte   a1,_),ConstValue(TConst_byte  a2,_)  -> mk_bool_value g (a1 > a2)
       | ConstValue(TConst_uint16 a1,_),ConstValue(TConst_uint16 a2,_) -> mk_bool_value g (a1 > a2)
       | ConstValue(TConst_uint32 a1,_),ConstValue(TConst_uint32 a2,_) -> mk_bool_value g (a1 > a2)
       | ConstValue(TConst_uint64 a1,_),ConstValue(TConst_uint64 a2,_) -> mk_bool_value g (a1 > a2)
       | _ -> UnknownValue
    | [ I_arith AI_shl ],[a;n],_ -> 
       match strip_value a,strip_value n with
       | ConstValue(TConst_int64  a,_),ConstValue(TConst_int32 n,_) when n >= 0 && n <= 63 -> (mk_int64_value  g (a <<< n))
       | ConstValue(TConst_int32  a,_),ConstValue(TConst_int32 n,_) when n >= 0 && n <= 31 -> (mk_int32_value  g (a <<< n))
       | ConstValue(TConst_int16  a,_),ConstValue(TConst_int32 n,_) when n >= 0 && n <= 15 -> (mk_int16_value  g (a <<< n))
       | ConstValue(TConst_sbyte  a,_),ConstValue(TConst_int32 n,_) when n >= 0 && n <= 7  -> (mk_int8_value   g (a <<< n))
       | ConstValue(TConst_uint64 a,_),ConstValue(TConst_int32 n,_) when n >= 0 && n <= 63 -> (mk_uint64_value g (a <<< n))
       | ConstValue(TConst_uint32 a,_),ConstValue(TConst_int32 n,_) when n >= 0 && n <= 31 -> (mk_uint32_value g (a <<< n))
       | ConstValue(TConst_uint16 a,_),ConstValue(TConst_int32 n,_) when n >= 0 && n <= 15 -> (mk_uint16_value g (a <<< n))
       | ConstValue(TConst_byte   a,_),ConstValue(TConst_int32 n,_) when n >= 0 && n <= 7  -> (mk_uint8_value  g (a <<< n))
       | _ -> UnknownValue

    | [ I_arith AI_shr ],[a;n],_ -> 
       match strip_value a,strip_value n with
       | ConstValue(TConst_sbyte a,_),ConstValue(TConst_int32 n,_) when n >= 0 && n <= 7  -> (mk_int8_value  g (a >>> n))
       | ConstValue(TConst_int16 a,_),ConstValue(TConst_int32 n,_) when n >= 0 && n <= 15 -> (mk_int16_value g (a >>> n))
       | ConstValue(TConst_int32 a,_),ConstValue(TConst_int32 n,_) when n >= 0 && n <= 31 -> (mk_int32_value g (a >>> n))
       | ConstValue(TConst_int64 a,_),ConstValue(TConst_int32 n,_) when n >= 0 && n <= 63 -> (mk_int64_value g (a >>> n))
       | _ -> UnknownValue
    | [ I_arith AI_shr_un ],[a;n],_ -> 
       match strip_value a,strip_value n with
       | ConstValue(TConst_byte   a,_),ConstValue(TConst_int32 n,_) when n >= 0 && n <= 7  -> (mk_uint8_value g  (a >>> n))
       | ConstValue(TConst_uint16 a,_),ConstValue(TConst_int32 n,_) when n >= 0 && n <= 15 -> (mk_uint16_value g (a >>> n))
       | ConstValue(TConst_uint32 a,_),ConstValue(TConst_int32 n,_) when n >= 0 && n <= 31 -> (mk_uint32_value g (a >>> n))
       | ConstValue(TConst_uint64 a,_),ConstValue(TConst_int32 n,_) when n >= 0 && n <= 63 -> (mk_uint64_value g (a >>> n))
       | _ -> UnknownValue
       
    // Retypings using IL asm "" are quite common in prim-types.fs
    // Sometimes these are only to get the primitives to pass the type checker.
    // Here we check for retypings from know values to known types.
    // We're conservative not to apply any actual data-changing conversions here.
    | [ ],[v],[ty] -> 
       match strip_value v with
       | ConstValue(TConst_bool   a,_) ->
            if type_equiv g ty g.bool_ty then v
            elif type_equiv g ty g.sbyte_ty then mk_int8_value g (if a then 1y else 0y)
            elif type_equiv g ty g.int16_ty then mk_int16_value g (if a then 1s else 0s)
            elif type_equiv g ty g.int32_ty then mk_int32_value g (if a then 1 else 0)
            elif type_equiv g ty g.byte_ty then mk_uint8_value g (if a then 1uy else 0uy)
            elif type_equiv g ty g.uint16_ty then mk_uint16_value g (if a then 1us else 0us)
            elif type_equiv g ty g.uint32_ty then mk_uint32_value g (if a then 1u else 0u)
            else UnknownValue
       | ConstValue(TConst_sbyte   a,_) ->
            if type_equiv g ty g.sbyte_ty then v
            elif type_equiv g ty g.int16_ty then mk_int16_value g (Unchecked.int16 a)
            elif type_equiv g ty g.int32_ty then mk_int32_value g (Unchecked.int32 a)
            else UnknownValue
       | ConstValue(TConst_byte   a,_) ->
            if type_equiv g ty g.byte_ty then v
            elif type_equiv g ty g.uint16_ty then mk_uint16_value g (Unchecked.uint16 a)
            elif type_equiv g ty g.uint32_ty then mk_uint32_value g (Unchecked.uint32 a)
            else UnknownValue
       | ConstValue(TConst_int16   a,_) ->
            if type_equiv g ty g.int16_ty then v
            elif type_equiv g ty g.int32_ty then mk_int32_value g (Unchecked.int32 a)
            else UnknownValue
       | ConstValue(TConst_uint16   a,_) ->
            if type_equiv g ty g.uint16_ty then v
            elif type_equiv g ty g.uint32_ty then mk_uint32_value g (Unchecked.uint32 a)
            else UnknownValue
       | ConstValue(TConst_int32   a,_) ->
            if type_equiv g ty g.int32_ty then v
            elif type_equiv g ty g.uint32_ty then mk_uint32_value g (Unchecked.uint32 a)
            else UnknownValue
       | ConstValue(TConst_uint32   a,_) ->
            if type_equiv g ty g.uint32_ty then v
            elif type_equiv g ty g.int32_ty then mk_int32_value g (Unchecked.int32 a)
            else UnknownValue
       | ConstValue(TConst_int64   a,_) ->
            if type_equiv g ty g.int64_ty then v
            elif type_equiv g ty g.uint64_ty then mk_uint64_value g (Unchecked.uint64 a)
            else UnknownValue
       | ConstValue(TConst_uint64   a,_) ->
            if type_equiv g ty g.uint64_ty then v
            elif type_equiv g ty g.int64_ty then mk_int64_value g (Unchecked.int64 a)
            else UnknownValue
       | _ -> UnknownValue
    | _ -> UnknownValue


//-------------------------------------------------------------------------
// Size constants and combinators
//------------------------------------------------------------------------- 

let local_var_size = 1

let rec AddTotalSizesAux acc l = match l with [] -> acc | h::t -> AddTotalSizesAux (h.TotalSize + acc) t
let AddTotalSizes l = AddTotalSizesAux 0 l

let rec AddFunctionSizesAux acc l = match l with [] -> acc | h::t -> AddFunctionSizesAux (h.FunctionSize + acc) t
let AddFunctionSizes l = AddFunctionSizesAux 0 l

let AddTotalSizesFlat l = l |> FlatList.sum_by (fun x -> x.TotalSize) 
let AddFunctionSizesFlat l = l |> FlatList.sum_by (fun x -> x.FunctionSize) 

//-------------------------------------------------------------------------
// opt list/array combinators - zipping (_,_) return type
//------------------------------------------------------------------------- 
let rec or_effects l = match l with [] -> false | h::t -> h.HasEffect || or_effects t
let or_effects_Flat l = FlatList.exists (fun x -> x.HasEffect) l

let rec or_tailcalls l = match l with [] -> false | h::t -> h.MightMakeCriticalTailcall || or_tailcalls t
let or_tailcalls_Flat l = FlatList.exists (fun x -> x.MightMakeCriticalTailcall) l
        
let rec OptimizeListAux f l acc1 acc2 = 
    match l with 
    | [] -> List.rev acc1, List.rev acc2
    | (h ::t) -> 
        let (x1,x2)  = f h
        OptimizeListAux f t (x1::acc1) (x2::acc2) 

let OptimizeList f l = OptimizeListAux f l [] [] 

let OptimizeFlatList f l = l |> FlatList.map f |> FlatList.unzip 

(* let opt_array f l = let l1,l2 = OptimizeList f (Array.to_list l) in Array.of_list l1, l2 *)

let no_exprs : (expr list * list<summary<ExprValueInfo>>)= [],[]
let no_FlatExprs : (FlatExprs * FlatList<summary<ExprValueInfo>>) = FlatList.empty, FlatList.empty

//-------------------------------------------------------------------------
// Common ways of building new value_infos
//------------------------------------------------------------------------- 

let CombineValueInfos einfos res = 
      { TotalSize  = AddTotalSizes einfos;
        FunctionSize  = AddFunctionSizes einfos;
        HasEffect = or_effects einfos; 
        MightMakeCriticalTailcall = or_tailcalls einfos; 
        Info = res }

let CombineFlatValueInfos einfos res = 
      { TotalSize  = AddTotalSizesFlat einfos;
        FunctionSize  = AddFunctionSizesFlat einfos;
        HasEffect = or_effects_Flat einfos; 
        MightMakeCriticalTailcall = or_tailcalls_Flat einfos; 
        Info = res }

let CombineValueInfosUnknown einfos = CombineValueInfos einfos UnknownValue
let CombineFlatValueInfosUnknown einfos = CombineFlatValueInfos einfos UnknownValue

//-------------------------------------------------------------------------
// Hide information because of a signature
//------------------------------------------------------------------------- 

let AbstractLazyModulInfoByHiding isAssemblyBoundary m mhi =

(* Previously: "This code is not sound when abstracting at the assembly boundary. 
                1. The MHI is not looking at 'internal' access attributes".
   Now, the freevars and FreeTyvars can indicate if the non-public (hidden) items have been used.
   Under those checks, the further hidden* checks may be subsumed (meaning, not required anymore).
*)
    let hiddenTycon,hiddenTyconRepr,hiddenVal, hiddenRfield, hiddenUconstr = 
        Zset.mem_of mhi.mhiTycons, 
        Zset.mem_of mhi.mhiTyconReprs, 
        Zset.mem_of mhi.mhiVals, 
        Zset.mem_of mhi.mhiRecdFields, 
        Zset.mem_of mhi.mhiUnionCases

    let rec abstractExprInfo ivalue = 
        if verboseOptimizationInfo then dprintf "abstractExprInfo\n"; 
        match ivalue with 
        (* Check for escaping value. Revert to old info if possible  *)
        | ValValue (vref2,detail) ->
            let detail' = abstractExprInfo detail 
            let v2 = (deref_val vref2) 
            let tyvars = free_in_val CollectAll v2 
            if  
                (isAssemblyBoundary && not (free_tyvars_all_public tyvars)) || 
                Zset.exists hiddenTycon tyvars.FreeTycons || 
                hiddenVal v2
            then detail'
            else ValValue (vref2,detail')
        (* Check for escape in lambda *)
        | CurriedLambdaValue (_,_,_,expr,_) | ConstExprValue(_,expr) when            
            (let fvs = free_in_expr CollectAll expr
             (*dprintf "abstractModulInfoByHiding, #fvs.FreeLocals = %d, #fvs.FreeRecdFields = %d\n" (List.length (Zset.elements fvs.FreeLocals)) (List.length (Zset.elements fvs.FreeRecdFields)); *)
             (isAssemblyBoundary && not (freevars_all_public fvs))      || 
             Zset.exists hiddenVal       fvs.FreeLocals               ||
             Zset.exists hiddenTycon     fvs.FreeTyvars.FreeTycons ||
             Zset.exists hiddenTyconRepr fvs.FreeLocalTyconReprs        ||
             Zset.exists hiddenRfield    fvs.FreeRecdFields               ||
             Zset.exists hiddenUconstr   fvs.FreeUnionCases ) ->
                UnknownValue
        (* Check for escape in constant *)
        | ConstValue(_,ty) when 
            (let ftyvs = free_in_type CollectAll ty
             (isAssemblyBoundary && not (free_tyvars_all_public ftyvs)) || 
             Zset.exists hiddenTycon ftyvs.FreeTycons) ->
                UnknownValue
        | TupleValue vinfos         -> TupleValue (Array.map abstractExprInfo vinfos)
        | RecdValue (tcref,vinfos)  -> 
            if hiddenTyconRepr (deref_tycon tcref) || Array.exists (rfref_of_rfield tcref >> hiddenRfield) tcref.AllFieldsArray
            then UnknownValue 
            else RecdValue (tcref,Array.map abstractExprInfo vinfos)
        | UnionCaseValue(ucref,vinfos) -> 
            let tcref = ucref.TyconRef
            if hiddenTyconRepr ucref.Tycon || tcref.UnionCasesArray |> Array.exists (ucref_of_ucase tcref >> hiddenUconstr) 
            then UnknownValue 
            else UnionCaseValue (ucref,Array.map abstractExprInfo vinfos)
        | ModuleValue sinfo         -> ModuleValue (abstractModulInfo sinfo)
        | SizeValue(vdepth,vinfo)   -> MakeSizedValueInfo (abstractExprInfo vinfo)
        | UnknownValue  
        | ConstExprValue _   
        | CurriedLambdaValue _ 
        | ConstValue _  -> ivalue
    and abstractValInfo v = { ValExprInfo=abstractExprInfo v.ValExprInfo; ValMakesNoCriticalTailcalls=v.ValMakesNoCriticalTailcalls }
    and abstractModulInfo ss =
         if verboseOptimizationInfo then dprintf "abstractModulInfo\n"; 
         { ModuleOrNamespaceInfos = NameMap.map abstractLazyModulInfo ss.ModuleOrNamespaceInfos;
           ValInfos = 
               ss.ValInfos 
               |> NameMap.filterRange (fst >> deref_val >> hiddenVal >> not)
               |> NameMap.map (fun (vref,e) -> 
                check "its implementation uses a binding hidden by a signature" m vref (abstractValInfo e) )  } 
    and abstractLazyModulInfo (ss:LazyModuleInfo) = 
          ss.Force() |> abstractModulInfo |> notlazy

    abstractLazyModulInfo

/// Hide all information except what we need for "must inline". We always save this optimization information
let AbstractLazyModulInfoToEssentials =

    let rec abstractModulInfo (ss:ModuleInfo) =
         { ModuleOrNamespaceInfos = NameMap.map (Lazy.force >> abstractModulInfo >> notlazy) ss.ModuleOrNamespaceInfos;
           ValInfos =  ss.ValInfos  |> NameMap.filterRange (fun (v,_) -> v.MustInline) }
    and abstractLazyModulInfo ss = ss |> Lazy.force |> abstractModulInfo |> notlazy
      
    abstractLazyModulInfo


//-------------------------------------------------------------------------
// Hide information because of a "let ... in ..." or "let rec  ... in ... "
//------------------------------------------------------------------------- 

let AbstractExprInfoByVars m (boundVars:Val list,boundTyVars) ivalue =
  // Module and member bindings can be skipped when checking abstraction, since abstraction of these values has already been done when 
  // we hit the end of the module and called AbstractLazyModulInfoByHiding. If we don't skip these then we end up quadtratically retraversing  
  // the inferred optimization data, i.e. at each binding all the way up a sequences of 'lets' in a module. 
  let boundVars = boundVars |> List.filter (fun v -> not v.IsMemberOrModuleBinding)

  match boundVars,boundTyVars with 
  | [],[] -> ivalue
  | _ -> 
      //let dump() = 
      //            boundVars |> List.iter (fun v -> dprintf "  -- bv  %s @ %a\n" v.MangledName output_range v.Range);
      //            boundTyVars |> List.iter (fun v -> dprintf "  -- btyv  %s @ %a\n" v.Name output_range v.Range)

      let rec abstractExprInfo ivalue =
          match ivalue with 
        (* Check for escaping value. Revert to old info if possible  *)
          | ValValue (VRef_private v2,detail) when  
            (nonNil boundVars && List.exists (vspec_eq v2) boundVars) || 
            (nonNil boundTyVars &&
             let ftyvs = free_in_val CollectTypars v2
             List.exists (Zset.mem_of ftyvs.FreeTypars) boundTyVars) -> 

              if verboseOptimizationInfo then 
                  dprintf "hiding value '%s' when used in expression (see %a)\n" v2.MangledName output_range v2.Range;
                  let ftyvs = free_in_val CollectTypars v2
                  ftyvs.FreeTypars |> Zset.iter (fun v -> dprintf "  -- ftyv  %s @ %a\n" v.Name output_range v.Range);
                  boundVars |> List.iter (fun v -> dprintf "  -- bv  %s @ %a\n" v.MangledName output_range v.Range);
                  boundTyVars |> List.iter (fun v -> dprintf "  -- btyv  %s @ %a\n" v.Name output_range v.Range)

              abstractExprInfo detail
          | ValValue (v2,detail) -> 
              let detail' = abstractExprInfo detail
              ValValue (v2,detail')
        
          // Check for escape in lambda 
          | CurriedLambdaValue (_,_,_,expr,_) | ConstExprValue(_,expr)  when 
            (let fvs = free_in_expr (if isNil boundTyVars then CollectLocals else CollectTyparsAndLocals) expr
             (nonNil boundVars   && List.exists (Zset.mem_of fvs.FreeLocals) boundVars) or
             (nonNil boundTyVars && List.exists (Zset.mem_of fvs.FreeTyvars.FreeTypars) boundTyVars) or
             (fvs.UsesMethodLocalConstructs )) ->
              if verboseOptimizationInfo then 
                  let fvs = free_in_expr (if isNil boundTyVars then CollectLocals else CollectTyparsAndLocals) expr
                  dprintf "Trimming lambda @ %a, UsesMethodLocalConstructs = %b, ExprL = %s\n"  output_range (range_of_expr expr) fvs.UsesMethodLocalConstructs (showL (ExprL expr));
                  fvs.FreeLocals |> Zset.iter (fun v -> dprintf "fv  %s @ %a\n" v.MangledName output_range v.Range);
                  fvs.FreeTyvars.FreeTypars |> Zset.iter (fun v -> dprintf "ftyv  %s @ %a\n" v.Name output_range v.Range);
                  boundVars |> List.iter (fun v -> dprintf "bv  %s @ %a\n" v.MangledName output_range v.Range);
                  boundTyVars |> List.iter (fun v -> dprintf "btyv  %s @ %a\n" v.Name output_range v.Range)

              UnknownValue

          // Check for escape in generic constant
          | ConstValue(_,ty) when 
            (nonNil boundTyVars && 
             (let ftyvs = free_in_type CollectTypars ty
              List.exists (Zset.mem_of ftyvs.FreeTypars) boundTyVars)) ->
              UnknownValue

          // Otherwise check all sub-values 
          | TupleValue vinfos -> TupleValue (Array.map (abstractExprInfo) vinfos)
          | RecdValue (tcref,vinfos) -> RecdValue (tcref,Array.map (abstractExprInfo) vinfos)
          | UnionCaseValue (cspec,vinfos) -> UnionCaseValue(cspec,Array.map (abstractExprInfo) vinfos)
          | ModuleValue sinfo -> ModuleValue (abstractModulInfo sinfo)
          | CurriedLambdaValue _ 
          | ConstValue _ 
          | ConstExprValue _ 
          | UnknownValue -> ivalue
          | SizeValue (vdepth,vinfo) -> MakeSizedValueInfo (abstractExprInfo vinfo)

      and abstractValInfo v = 
          { ValExprInfo=abstractExprInfo v.ValExprInfo; 
            ValMakesNoCriticalTailcalls=v.ValMakesNoCriticalTailcalls }

      and abstractModulInfo ss =
         { ModuleOrNamespaceInfos  = ss.ModuleOrNamespaceInfos  |> NameMap.map (Lazy.force >> abstractModulInfo >> notlazy) ;
           ValInfos = ss.ValInfos |> NameMap.map (fun (vref,e) -> 
               if verboseOptimizationInfo then dprintf "checking %s @ %a\n" vref.MangledName output_range (vref.Range); 
               check "its implementation uses a private binding" m vref (abstractValInfo e) ) }

      abstractExprInfo ivalue

//-------------------------------------------------------------------------
// Rewrite optimization, e.g. to use public stable references so we can pickle it
// to disk.
//------------------------------------------------------------------------- 
let RemapLazyModulInfo g tmenv =

    let rec remapExprInfo ivalue = 
        if verboseOptimizationInfo then dprintf "remapExprInfo\n"; 
        match ivalue with 
        | ValValue (v,detail)      -> ValValue (remap_vref tmenv v,remapExprInfo detail)
        | TupleValue vinfos         -> TupleValue (Array.map remapExprInfo vinfos)
        | RecdValue (tcref,vinfos)  -> RecdValue (remap_tcref tmenv.tcref_remap tcref, Array.map remapExprInfo vinfos)
        | UnionCaseValue(cspec,vinfos) -> UnionCaseValue (remap_ucref tmenv.tcref_remap cspec,Array.map remapExprInfo vinfos)
        | ModuleValue sinfo         -> ModuleValue (remapModulInfo sinfo)
        | SizeValue(vdepth,vinfo) -> MakeSizedValueInfo (remapExprInfo vinfo)
        | UnknownValue              -> UnknownValue
        | CurriedLambdaValue (uniq,arity,sz,expr,typ)  -> CurriedLambdaValue (uniq,arity,sz,remap_expr g CloneAll tmenv expr,remap_possible_forall_typ g tmenv typ)  
        | ConstValue (c,ty)  -> ConstValue (c,remap_possible_forall_typ g tmenv ty)
        | ConstExprValue (sz,expr)  -> ConstExprValue (sz,remap_expr g CloneAll tmenv expr)

    and remapValInfo v = { ValExprInfo=remapExprInfo v.ValExprInfo; ValMakesNoCriticalTailcalls=v.ValMakesNoCriticalTailcalls }
    and remapModulInfo ss =
         if verboseOptimizationInfo then dprintf "remapModulInfo\n"; 
         { ModuleOrNamespaceInfos = ss.ModuleOrNamespaceInfos |> NameMap.map RemapLazyModulInfo;
           ValInfos = ss.ValInfos |> NameMap.map (fun (vref,vinfo) -> 
                                                     let vref' = remap_vref tmenv vref 
                                                     let vinfo = remapValInfo vinfo
                                                     // Propogate any inferred ValMakesNoCriticalTailcalls flag from implementation to signature information
                                                     if vinfo.ValMakesNoCriticalTailcalls then set_notailcall_hint_of_vflags vref'.Deref.Data true
                                                     (vref',vinfo)) } 

    and RemapLazyModulInfo ss =
         ss |> Lazy.force |> remapModulInfo |> notlazy
           
    RemapLazyModulInfo

//-------------------------------------------------------------------------
// Hide information when a value is no longer visible
//------------------------------------------------------------------------- 

let AbstractAndRemapModulInfo msg g m (repackage,hidden) info =
    let mrpi = mk_repackage_remapping repackage
    if verboseOptimizationInfo then dprintf "%s - %a - Optimization data prior to trim: \n%s\n" msg output_range m (Layout.showL (Layout.squashTo 192 (moduleInfoL info)));
    let info = info |> AbstractLazyModulInfoByHiding false m hidden
    if verboseOptimizationInfo then dprintf "%s - %a - Optimization data after trim:\n%s\n" msg output_range m (Layout.showL (Layout.squashTo 192 (moduleInfoL info)));
    let info = info |> RemapLazyModulInfo g mrpi
    if verboseOptimizationInfo then dprintf "%s - %a - Optimization data after remap:\n%s\n" msg output_range m (Layout.showL (Layout.squashTo 192 (moduleInfoL info)));
    info

//-------------------------------------------------------------------------
// Misc helerps
//------------------------------------------------------------------------- 

(* Mark some variables (the ones we introduce via abstractBigTargets) as don't-eliminate *)
let suffixForVariablesThatMayNotBeEliminated = "$cont"

/// Type applications of F# "type functions" may cause side effects, e.g. 
/// let x<'a> = printfn "hello"; typeof<'a> 
/// In this case do not treat them as constants. 
let IsTyFuncValRefExpr = function 
    | TExpr_val (fv,_,_) -> fv.IsTypeFunction
    | _ -> false

/// Type applications of existing functions are always simple constants, with the exception of F# 'type functions' 
/// REVIEW: we could also include any under-applied application here. 
let rec IsSmallConstExpr x =
    match x with
    | TExpr_val (v,_,m) -> not v.IsMutable
    | TExpr_app(fe,_,tyargs,args,_) -> isNil(args) && not (IsTyFuncValRefExpr fe) && IsSmallConstExpr fe
    | _ -> false

let ValueOfExpr expr = 
    if IsSmallConstExpr expr then 
      ConstExprValue(0,expr)
    else UnknownValue

//-------------------------------------------------------------------------
// Dead binding elimination 
//------------------------------------------------------------------------- 
 
let ValueIsUsedOrHasEffect cenv m fvs (b:Binding,binfo) =
    let v = b.Var
    not (cenv.settings.EliminateUnusedBindings()) ||
    isSome v.MemberInfo ||
    binfo.HasEffect || 
    Zset.mem v fvs

let rec SplitValuesByIsUsedOrHasEffect cenv m fvs x = 
    x |> FlatList.filter (ValueIsUsedOrHasEffect cenv m fvs) |> FlatList.unzip

//-------------------------------------------------------------------------
// 
//------------------------------------------------------------------------- 

let IlAssemblyCodeInstrHasEffect i = 
    match i with 
    | I_arith (  AI_nop | AI_ldc _ | AI_add | AI_sub | AI_mul | AI_xor | AI_and | AI_or 
               | AI_ceq | AI_cgt | AI_cgt_un | AI_clt | AI_clt_un | AI_conv _ | AI_shl 
               | AI_shr | AI_shr_un | AI_neg | AI_not | AI_ldnull )
    | I_ldstr _ | I_ldtoken _  -> false
    | _ -> true
  
let IlAssemblyCodeHasEffect instrs = List.exists IlAssemblyCodeInstrHasEffect instrs

//-------------------------------------------------------------------------
// Effects
//
// note: allocating an object with observable identity (i.e. a name) 
// or reading from a mutable field counts as an 'effect', i.e.
// this context 'effect' has it's usual meaning in the effect analysis literature of 
//   read-from-mutable 
//   write-to-mutable 
//   name-generation
//   arbitrary-side-effect (e.g. 'non-termination' or 'fire the missiles')
//------------------------------------------------------------------------- 

let rec ExprHasEffect g expr = 
    match expr with 
    | TExpr_val (vref,_,_) -> vref.IsTypeFunction or (vref.IsMutable)
    | TExpr_quote _ 
    | TExpr_lambda _
    | TExpr_tlambda _ 
    | TExpr_const _ -> false
    /// type applications do not have effects, with the exception of type functions
    | TExpr_app(f0,_,_,[],_) -> (IsTyFuncValRefExpr f0) or ExprHasEffect g f0
    | TExpr_op(op,_,args,_) -> ExprsHaveEffect g args || OpHasEffect g op
    | TExpr_letrec(binds,body,_,_) -> BindingsHaveEffect g binds || ExprHasEffect g body
    | TExpr_let(bind,body,_,_) -> BindingHasEffect g bind || ExprHasEffect g body
    // REVIEW: could add TExpr_obj on an interface type - these are similar to records of lambda expressions 
    | _ -> true
and ExprsHaveEffect g exprs = List.exists (ExprHasEffect g) exprs
and BindingsHaveEffect g binds = FlatList.exists (BindingHasEffect g) binds
and BindingHasEffect g bind = bind.Expr |> ExprHasEffect g
and OpHasEffect g op = 
    match op with 
    | TOp_tuple -> false
    | TOp_recd (ctor,tcref) -> 
        match ctor with 
        | RecdExprIsObjInit -> true
        | RecdExpr -> tcref_alloc_observable tcref
    | TOp_ucase ucref -> tcref_alloc_observable ucref.TyconRef
    | TOp_exnconstr ecref -> ecref_alloc_observable ecref
    | TOp_bytes _ | TOp_uint16s _ | TOp_array -> true (* alloc observable *)
    | TOp_ucase_tag_get _ -> false
    | TOp_ucase_proof _ -> false
    | TOp_ucase_field_get (ucref,n) -> ucref_rfield_mutable g ucref n 
    | TOp_asm(instrs,_) -> IlAssemblyCodeHasEffect instrs
    | TOp_tuple_field_get(_) -> false
    | TOp_exnconstr_field_get(ecref,n) -> ecref_rfield_mutable ecref n 
    | TOp_get_ref_lval -> false
    | TOp_rfield_get rfref  -> rfref.RecdField.IsMutable
    | TOp_field_get_addr rfref  -> true (* check *)
    | TOp_ucase_field_set _
    | TOp_exnconstr_field_set _
    | TOp_coerce
    | TOp_rethrow
    | TOp_for _ 
    | TOp_while  _
    | TOp_try_catch _
    | TOp_try_finally _ (* note: these really go through a different path anyway *)
    | TOp_trait_call _
    | TOp_goto _
    | TOp_label _
    | TOp_return
    | TOp_ilcall _ (* conservative *)
    | TOp_lval_op _  (* conservative *)
    | TOp_rfield_set _ -> true


let TryEliminateBinding cenv env (TBind(vspec1,e1,spBind)) e2 m  =
    // don't eliminate bindings if we're not optimizing AND the binding is not a compiler generated variable
    if not (cenv.optimizing && cenv.settings.EliminateImmediatelyConsumedLocals()) && 
       not vspec1.IsCompilerGenerated then 
       None 
    else
        // Peephole on immediate consumption of single bindings, e.g. "let x = e in x" --> "e" 
        // REVIEW: enhance this by general elimination of bindings to 
        // non-side-effecting expressions that are used only once. 
        // But note the cases below cover some instances of side-effecting expressions as well.... 
        let IsUniqueUse vspec2 args = 
              vspec_eq vspec1 vspec2  
           && (not (vspec2.MangledName.Contains(suffixForVariablesThatMayNotBeEliminated)))
           // REVIEW: this looks slow. Look only for one variable instead 
           && (let fvs = acc_free_in_exprs CollectLocals args empty_freevars
               not (Zset.mem vspec1 fvs.FreeLocals))

        // Immediate consumption of value as 2nd or subsequent argument to a construction or projection operation 
        let rec GetImmediateUseContext rargsl argsr = 
              match argsr with 
              | (TExpr_val(VRef_private vspec2,_,_)) :: argsr2
                 when vspec_eq vspec1 vspec2 && IsUniqueUse vspec2 (List.rev rargsl@argsr2) -> Some(List.rev rargsl,argsr2)
              | argsrh :: argsrt when not (ExprHasEffect cenv.g argsrh) -> GetImmediateUseContext (argsrh::rargsl) argsrt 
              | _ -> None

        match strip_expr e2 with 

         // Immediate consumption of value as itself 'let x = e in x'
         | TExpr_val(VRef_private vspec2,_,_) 
             when IsUniqueUse vspec2 [] -> 
               // if verbose then dprintf "Simplifying let x = e in x near %a\n" output_range m;
               Some e1

         // Immediate consumption of value by a pattern match 'let x = e in match x with ...'
         | TExpr_match(spMatch,exprm,TDSwitch(TExpr_val(VRef_private vspec2,_,_),cases,dflt,_),targets,m,ty2,_)
             when (vspec_eq vspec1 vspec2 && 
                   let fvs = acc_free_in_targets CollectLocals targets (acc_free_in_switch_cases CollectLocals cases dflt empty_freevars)
                   not (Zset.mem vspec1 fvs.FreeLocals)) -> 
                    (* if verbose then dprintf "Simplifying let x = e in match x with ... near %a\n" output_range m;*)
              let spMatch = spBind.Combine(spMatch)
              Some (TExpr_match(spMatch,range_of_expr e1,TDSwitch(e1,cases,dflt,m),targets,m,ty2,SkipFreeVarsCache()))
               
         // Immediate consumption of value as a function 'let f = e in f ...'
         // Note functions are evaluated before args 
         // Note: do not include functions with a single arg of unit type, introduced by abstractBigTargets 
         | TExpr_app(f,f0ty,tyargs,args,m) 
               when not (vspec1.MangledName.Contains(suffixForVariablesThatMayNotBeEliminated)) ->
             match GetImmediateUseContext [] (f::args) with 
             | Some([],rargs) -> Some (MakeApplicationAndBetaReduce cenv.g (e1,f0ty,[tyargs],rargs ,m))
             | Some(f::largs,rargs) -> Some (MakeApplicationAndBetaReduce cenv.g (f,f0ty,[tyargs],largs @ (e1::rargs),m))
             | None -> None

         // Immediate consumption of value as first non-effectful argument to a construction or projection operation 
         // 'let x = e in op[x;....]'
         | TExpr_op (c,tyargs,args,m) -> 
             match GetImmediateUseContext [] args with 
             | Some(largs,rargs) -> Some (TExpr_op (c,tyargs,(largs @ (e1:: rargs)),m))
             | None -> None

         | _ ->  
            None

let TryEliminateLet cenv env bind e2 m = 
    match TryEliminateBinding cenv env bind e2 m with 
    | Some e2' -> e2',-local_var_size  (* eliminated a let, hence reduce size estimate *)
    | None -> mk_let_bind m bind e2 ,0

//-------------------------------------------------------------------------

/// Detect the application of a value to an arbitrary number of arguments
let rec (|KnownValApp|_|) expr = 
    match strip_expr expr with
    | TExpr_val(vref,_,_) -> Some(vref,[],[])
    | TExpr_app(KnownValApp(vref,typeArgs1,otherArgs1),_,typeArgs2,otherArgs2,_) -> Some(vref,typeArgs1@typeArgs2,otherArgs1@otherArgs2)
    | _ -> None

//-------------------------------------------------------------------------
// ExpandStructuralBinding
//
// Expand bindings to tuple expressions by factoring sub-expression out as prior bindings.
// Similarly for other structural constructions, like records...
// If the item is only projected from then the construction (allocation) can be eliminated.
// This transform encourages that by allowing projections to be simplified.
//------------------------------------------------------------------------- 

let ExprIsValue = function TExpr_val _ -> true | _ -> false
let ExpandStructuralBinding cenv env expr =
    match expr with
    | TExpr_let (TBind(v,rhs,tgtSeqPtOpt),body,m,_) 
        when (is_tuple rhs &&  
              not v.IsCompiledAsTopLevel &&  
              not v.IsMember && 
              not v.IsTypeFunction &&
              not v.IsMutable) ->
          let args   = try_dest_tuple rhs
          if List.forall ExprIsValue args then
              expr (* avoid re-expanding when recursion hits original binding *)
          else
              let argTys = dest_tuple_typ cenv.g v.Type
              let argBind i arg argTy =
                  let name = v.MangledName ^ "_" ^ string i
                  let v,ve = mk_compgen_local (range_of_expr arg) name argTy
                  ve,mk_compgen_bind v arg
           
              let ves,binds = List.mapi2 argBind args argTys |> List.unzip
              let tuple = mk_tupled cenv.g m ves argTys
              mk_lets_bind m binds (mk_let tgtSeqPtOpt m v tuple body)
              (* REVIEW: other cases - records, explicit lists etc. *)
    | expr -> expr
    
//-------------------------------------------------------------------------
// The traversal
//------------------------------------------------------------------------- 

let rec OptimizeExpr cenv (env:IncrementalOptimizationEnv) expr =
    if verboseOptimizations then dprintf "OptimizeExpr@%a\n" output_range (range_of_expr expr);

    // Eliminate subsumption coercions for functions. This must be done post-typechecking because we need
    // complete inference types.
    let expr = NormalizeAndAdjustPossibleSubsumptionExprs cenv.g expr

    let expr = strip_expr expr
    

    match expr with
    (* treat the common linear cases to avoid stack overflows, using an explicit continutation *)
    | TExpr_seq _ | TExpr_let _ ->  OptimizeLinearExpr cenv env expr (fun x -> x)

    | TExpr_const (c,m,ty) -> OptimizeConst cenv env expr (c,m,ty)
    | TExpr_val (v,vFlags,m) -> OptimizeVal cenv env expr (v,m)
    | TExpr_quote(ast,conv,m,ty) -> 
          TExpr_quote(ast,conv,m,ty),
          { TotalSize = 10;
            FunctionSize = 1;
            HasEffect = false;  
            MightMakeCriticalTailcall=false;
            Info=UnknownValue }
    | TExpr_obj (_,typ,basev,expr,overrides,iimpls,m,_) -> OptimizeObjectExpr cenv env (typ,basev,expr,overrides,iimpls,m)
    | TExpr_op (c,tyargs,args,m) -> OptimizeExprOp cenv env (c,tyargs,args,m)
    | TExpr_app(f,fty,tyargs,argsl,m) -> OptimizeApplication cenv env (f,fty,tyargs,argsl,m) 
    (* REVIEW: fold the next two cases together *)
    | TExpr_lambda(lambdaId,_,argvs,body,m,rty,_) -> 
        let topValInfo = TopValInfo ([],[argvs |> List.map (fun _ -> TopValInfo.unnamedTopArg1)],TopValInfo.unnamedRetVal)
        let ty = mk_multi_lambda_ty m argvs rty
        OptimizeLambdas None cenv env topValInfo expr ty
    | TExpr_tlambda(lambdaId,tps,body,m,rty,_)  -> 
        let topValInfo = TopValInfo (TopValInfo.InferTyparInfo tps,[],TopValInfo.unnamedRetVal)
        let ty = try_mk_forall_ty tps rty
        OptimizeLambdas None cenv env topValInfo expr ty
    | TExpr_tchoose _  -> OptimizeExpr cenv env (Typrelns.choose_typar_solutions_for_tchoose cenv.g cenv.amap expr)
    | TExpr_match(spMatch,exprm,dtree,targets,m,ty,_) -> OptimizeMatch cenv env (spMatch,exprm,dtree,targets,m,ty)
    | TExpr_letrec (binds,e,m,_) ->  OptimizeLetRec cenv env (binds,e,m)
    | TExpr_static_optimization (constraints,e2,e3,m) ->
        let e2',e2info = OptimizeExpr cenv env e2
        let e3',e3info = OptimizeExpr cenv env e3
        TExpr_static_optimization(constraints,e2',e3',m), 
        { TotalSize = min e2info.TotalSize e3info.TotalSize;
          FunctionSize = min e2info.FunctionSize e3info.FunctionSize;
          HasEffect = e2info.HasEffect || e3info.HasEffect;
          MightMakeCriticalTailcall=e2info.MightMakeCriticalTailcall || e3info.MightMakeCriticalTailcall // seems conservative
          Info= UnknownValue }
    | TExpr_link eref -> 
        assert ("unexpected reclink" = "");
        failwith "Unexpected reclink"


//-------------------------------------------------------------------------
// Optimize/analyze an object expression
//------------------------------------------------------------------------- 

and OptimizeObjectExpr cenv env (typ,basevopt,basecall,overrides,iimpls,m) =
    if verboseOptimizations then dprintf "OptimizeObjectExpr\n";
    let basecall',basecallinfo = OptimizeExpr cenv env basecall
    let overrides',overrideinfos = OptimizeMethods cenv env basevopt overrides
    let iimpls',iimplsinfos = OptimizeInterfaceImpls cenv env basevopt iimpls
    let expr'=mk_obj_expr(typ,basevopt,basecall',overrides',iimpls',m)
    expr', { TotalSize=closureTotalSize + basecallinfo.TotalSize + AddTotalSizes overrideinfos + AddTotalSizes iimplsinfos;
             FunctionSize=1 (* a newobj *) ;
             HasEffect=true;
             MightMakeCriticalTailcall=false; // creating an object is not a useful tailcall
             Info=UnknownValue}

//-------------------------------------------------------------------------
// Optimize/analyze the methods that make up an object expression
//------------------------------------------------------------------------- 

and OptimizeMethods cenv env basevopt l = OptimizeList (OptimizeMethod cenv env basevopt) l
and OptimizeMethod cenv env basevopt (TObjExprMethod(slotsig,tps,vs,e,m) as tmethod) = 
    if verboseOptimizations then dprintf "OptimizeMethod\n";
    let env = {env with latestBoundId=Some tmethod.Id; functionVal = None}
    let env = BindTypeVarsToUnknown tps env
    let env = bind_internal_vspecs_to_unknown cenv vs env
    let env = Option.fold_right (bind_internal_vspec_to_unknown cenv) basevopt env
    let e',einfo = OptimizeExpr cenv env e
    (* REVIEW: if we ever change this from being UnknownValue then we should call AbstractExprInfoByVars *)
    TObjExprMethod(slotsig,tps,vs,e',m),
    { TotalSize = einfo.TotalSize;
      FunctionSize = 0;
      HasEffect = false;
      MightMakeCriticalTailcall=false;
      Info=UnknownValue}

//-------------------------------------------------------------------------
// Optimize/analyze the interface implementations that form part of an object expression
//------------------------------------------------------------------------- 

and OptimizeInterfaceImpls cenv env basevopt l = OptimizeList (OptimizeInterfaceImpl cenv env basevopt) l
and OptimizeInterfaceImpl cenv env basevopt (ty,overrides) = 
    if verboseOptimizations then dprintf "OptimizeInterfaceImpl\n";
    let overrides',overridesinfos = OptimizeMethods cenv env basevopt overrides
    (ty, overrides'), 
    { TotalSize = AddTotalSizes overridesinfos;
      FunctionSize = 1;
      HasEffect = false;
      MightMakeCriticalTailcall=false;
      Info=UnknownValue}

//-------------------------------------------------------------------------
// Optimize/analyze an application of an intrinsic operator to arguments
//------------------------------------------------------------------------- 

and OptimizeExprOp cenv env (op,tyargs,args,m) =

    if verboseOptimizations then dprintf "OptimizeExprOp\n";
    (* Special cases *)
    match op,tyargs,args with 
    | TOp_coerce,[toty;fromty],[e] -> 
        let e',einfo = OptimizeExpr cenv env e
        if type_equiv cenv.g toty fromty then e',einfo 
        else 
          mk_coerce(e',toty,m,fromty), 
          { TotalSize=einfo.TotalSize + 1;
            FunctionSize=einfo.FunctionSize + 1;
            HasEffect = true;  
            MightMakeCriticalTailcall=false;
            Info=UnknownValue }
    (* Handle these as special cases since mutables are allowed inside their bodies *)
    | TOp_while spWhile,_,[TExpr_lambda(_,_,[_],e1,_,_,_);TExpr_lambda(_,_,[_],e2,_,_,_)]  -> OptimizeWhileLoop cenv env (spWhile,e1,e2,m) 
    | TOp_for(spStart,dir),_,[TExpr_lambda(_,_,[_],e1,_,_,_);TExpr_lambda(_,_,[_],e2,_,_,_);TExpr_lambda(_,_,[v],e3,_,_,_)]  -> OptimizeFastIntegerForLoop cenv env (spStart,v,e1,dir,e2,e3,m) 
    | TOp_try_finally(spTry,spFinally),[resty],[TExpr_lambda(_,_,[_],e1,_,_,_); TExpr_lambda(_,_,[_],e2,_,_,_)] -> OptimizeTryFinally cenv env (spTry,spFinally,e1,e2,m,resty)
    | TOp_try_catch(spTry,spWith),[resty],[TExpr_lambda(_,_,[_],e1,_,_,_); TExpr_lambda(_,_,[vf],ef,_,_,_); TExpr_lambda(_,_,[vh],eh,_,_,_)] -> OptimizeTryCatch cenv env (e1,vf,ef,vh,eh,m,resty,spTry,spWith)
    | TOp_trait_call(traitInfo),[],args -> OptimizeTraitCall cenv env (traitInfo, args, m) 

   // This code hooks arr.Length. The idea is to ensure loops end up in the "same shape"as the forms of loops that the .NET JIT
   // guarantees to optimize.
  
    | TOp_ilcall ((virt,protect,valu,newobj,superInit,prop,isDllImport,boxthis,mref),enclTypeArgs,methTypeArgs,tys),_,[arg]
        when (mref.EnclosingTypeRef.Scope.IsAssemblyRef &&
              mref.EnclosingTypeRef.Scope.AssemblyRef.Name = "mscorlib" &&
              mref.EnclosingTypeRef.Name = "System.Array" &&
              mref.Name = "get_Length" &&
              is_il_arr1_typ cenv.g (type_of_expr cenv.g arg)) -> 
         OptimizeExpr cenv env (TExpr_op(TOp_asm(i_ldlen,[cenv.g.int_ty]),[],[arg],m))


    // Empty IL instruction lists are used as casts in prim_types.fs. But we can get rid of them 
    // if the types match up. 
    | TOp_asm([],[ty]),_,[a] when type_equiv cenv.g (type_of_expr cenv.g a) ty -> OptimizeExpr cenv env a

    | _ -> 
    (* Reductions *)
    let args',arginfos = OptimizeExprsThenConsiderSplits cenv env args
    let knownValue = 
        match op,arginfos with 
        | TOp_rfield_get (rf),[e1info] -> TryOptimizeRecordFieldGet cenv env (e1info,rf,tyargs,m) 
        | TOp_tuple_field_get n,[e1info] -> TryOptimizeTupleFieldGet cenv env (e1info,tyargs,n,m)
        | TOp_ucase_field_get (cspec,n),[e1info] -> TryOptimizeUnionCaseGet cenv env (e1info,cspec,tyargs,n,m)
        | _ -> None
    match knownValue with 
    | Some valu -> 
        match TryOptimizeVal cenv env (false,valu,m)  with 
        | Some res -> OptimizeExpr cenv env res  (* discard e1 since guard ensures it has no effects *)
        | None -> OptimizeExprOpFallback cenv env (op,tyargs,args',m) arginfos valu
    | None -> OptimizeExprOpFallback cenv env (op,tyargs,args',m) arginfos UnknownValue


and OptimizeExprOpFallback cenv env (op,tyargs,args',m) arginfos valu =
    if verboseOptimizations then dprintf "OptimizeExprOpFallback\n";
    (* The generic case - we may collect information, but the construction/projection doesn't disappear *)
    let args_tsize = AddTotalSizes arginfos
    let args_fsize = AddFunctionSizes arginfos
    let args_effect = or_effects arginfos
    let args_valus = List.map (fun x -> x.Info) arginfos
    let effect = OpHasEffect cenv.g op
    let cost,valu = 
      match op with
      | TOp_ucase c              -> 2,MakeValueInfoForUnionCase c (Array.of_list args_valus)
      | TOp_exnconstr _           -> 2,valu (* REVIEW: information collection possilbe here *)
      | TOp_tuple                 -> 1, MakeValueInfoForTuple (Array.of_list args_valus)
      | TOp_rfield_get _     
      | TOp_tuple_field_get _    
      | TOp_ucase_field_get _   
      | TOp_exnconstr_field_get _
      | TOp_ucase_tag_get _      -> 1,valu (* REVIEW: reduction possible here, and may be very effective *)
      | TOp_ucase_proof _        -> 
            // We count the proof as size 0
            // We maintain the value of the source of the proof-cast if it is known to be a UnionCaseValue
            let valu = (match args_valus.[0] with StripUnionCaseValue (uc,info) -> UnionCaseValue(uc,info) | _ ->  valu)  
            0,valu
      | TOp_asm(instrs,tys)         -> min (List.length instrs) 1, 
                                       MakeAssemblyCodeValueInfo cenv.g instrs args_valus tys
      | TOp_bytes bytes -> (Bytes.length bytes)/10 , valu
      | TOp_uint16s bytes -> bytes.Length/10 , valu
      | TOp_field_get_addr _     
      | TOp_array | TOp_for _ | TOp_while _ | TOp_try_catch _ | TOp_try_finally _
      | TOp_ilcall _
      | TOp_trait_call _          
      | TOp_lval_op _    
      | TOp_rfield_set _
      | TOp_ucase_field_set _
      | TOp_get_ref_lval 
      | TOp_coerce
      | TOp_rethrow
      | TOp_exnconstr_field_set _ -> 1,valu
      | TOp_recd (ctorInfo,tcref) ->
          let finfos = tcref.AllInstanceFieldsAsList
          (* REVIEW: this seems a little conservative: allocating a record with a mutable field *)
          (* is not an effect - only reading or writing the field is. *)
          let valu = 
              match ctorInfo with 
              | RecdExprIsObjInit -> UnknownValue
              | RecdExpr -> 
                   if args_valus.Length <> finfos.Length then valu 
                   else MakeValueInfoForRecord tcref tyargs (Array.of_list ((args_valus,finfos) ||> List.map2 (fun x f -> if f.IsMutable then UnknownValue else x) ))
          2,valu  
      | TOp_goto _ | TOp_label _ | TOp_return -> assert false; error(InternalError("unexpected goto/label/return in optimization",m))

    // Indirect calls to IL code are always taken as tailcalls
    let mayBeCriticalTailcall = 
      match op with
      | TOp_ilcall ((virt,_,_,newobj,_,_,_,_,_),_,_,_) -> not newobj && virt
      | _ -> false
    
    let vinfo = { TotalSize=args_tsize + cost;
                  FunctionSize=args_fsize + cost;
                  HasEffect=args_effect || effect;                  
                  MightMakeCriticalTailcall= mayBeCriticalTailcall; // discard tailcall info for args - these are not in tailcall position
                  Info=valu } 

    // Replace entire expression with known value? 
    match TryOptimizeValInfo cenv env m vinfo with 
    | Some res -> res,vinfo
    | None ->
          TExpr_op(op,tyargs,args',m),
          { TotalSize=args_tsize + cost;
            FunctionSize=args_fsize + cost;
            HasEffect=args_effect || effect;
            MightMakeCriticalTailcall= mayBeCriticalTailcall; // discard tailcall info for args - these are not in tailcall position
            Info=valu }

//-------------------------------------------------------------------------
// Optimize/analyze a constant node
//------------------------------------------------------------------------- 

and OptimizeConst cenv env expr (c,m,ty) = 
    match TryEliminateDesugaredConstants cenv.g m c with 
    | Some(e) -> 
        OptimizeExpr cenv env e
    | None ->
        if verboseOptimizations then dprintf "OptimizeConst\n";
        expr, { TotalSize=(match c with 
                           | TConst_string b -> b.Length/10 
                           | _ -> 0);
                FunctionSize=0;
                HasEffect=false;
                MightMakeCriticalTailcall=false;
                Info=MakeValueInfoForConst c ty}

//-------------------------------------------------------------------------
// Optimize/analyze a record lookup. 
//------------------------------------------------------------------------- 

and TryOptimizeRecordFieldGet cenv env (e1info,r,tinst,m) =
    match dest_recd_value e1info.Info with
    | Some finfos when cenv.settings.EliminateRecdFieldGet() && not e1info.HasEffect ->
        let n = (rfref_index r)
        if n >= finfos.Length then errorR(InternalError( "TryOptimizeRecordFieldGet: term argument out of range",m));
        Some finfos.[n]   (* Uses INVARIANT on record ValInfos that exprs are in defn order *)
    | _ -> None
  
and TryOptimizeTupleFieldGet cenv env (e1info,tys,n,m) =
    match dest_tuple_value e1info.Info with
    | Some tups when cenv.settings.EliminateTupleFieldGet() && not e1info.HasEffect ->
        let len = tups.Length 
        if len <> tys.Length then errorR(InternalError("error: tuple lengths don't match",m));
        if n >= len then errorR(InternalError("TryOptimizeTupleFieldGet: tuple index out of range",m));
        Some tups.[n]
    | _ -> None
      
and TryOptimizeUnionCaseGet cenv env (e1info,cspec,tys,n,m) =
    match e1info.Info with
    | StripUnionCaseValue(cspec2,args) when cenv.settings.EliminatUnionCaseFieldGet() && not e1info.HasEffect && cenv.g.ucref_eq cspec cspec2 ->
        if n >= args.Length then errorR(InternalError( "TryOptimizeUnionCaseGet: term argument out of range",m));
        Some args.[n]
    | _ -> None

//-------------------------------------------------------------------------
// Optimize/analyze a for-loop
//------------------------------------------------------------------------- 

and OptimizeFastIntegerForLoop cenv env (spStart,v,e1,dir,e2,e3,m) =
    if verboseOptimizations then dprintf "OptimizeFastIntegerForLoop\n";
    let e1',e1info = OptimizeExpr cenv env e1 
    let e2',e2info = OptimizeExpr cenv env e2 
    let env = bind_internal_vspec_to_unknown cenv v env 
    let e3', e3info = OptimizeExpr cenv env e3 
    // Try to replace F#-style loops with C# style loops that recompute their bounds but which are compiled more efficiently by the JITs, e.g.
    //  F#  "for x = 0 to arre.Length - 1 do ..." --> C# "for (int x = 0; x < arre.Length; x++) { ... }"
    //  F#  "for x = 0 to 10 do ..." --> C# "for (int x = 0; x < 11; x++) { ... }"
    let e2', dir = 
        match dir, e2' with 
        // detect upwards for loops with bounds of the form "arr.Length - 1" and convert them to a C#-style for loop
        | FSharpForLoopUp, TExpr_op(TOp_asm([ I_arith AI_sub ],_),_,[TExpr_op(TOp_asm([ I_ldlen; I_arith (AI_conv DT_I4)],_),_,[arre],_);
                                                                     TExpr_const(TConst_int32 1,_,_)],_) 
                  when not (snd(OptimizeExpr cenv env arre)).HasEffect -> 

            mk_ldlen cenv.g m arre, CSharpForLoopUp

        // detect upwards for loops with constant bounds, but not MaxValue!
        | FSharpForLoopUp, TExpr_const(TConst_int32 n,_,_) 
                  when n < System.Int32.MaxValue -> 

            mk_incr cenv.g m e2', CSharpForLoopUp

        | _ ->
            e2', dir

    let einfos = [e1info;e2info;e3info] 
    let eff = or_effects einfos 
    (* neither bounds nor body has an effect, and loops always terminate, hence eliminate the loop *)
    if not eff then 
        mk_unit cenv.g m , { TotalSize=0; FunctionSize=0; HasEffect=false; MightMakeCriticalTailcall=false; Info=UnknownValue }
    else
        let expr' = mk_for cenv.g (spStart,v,e1',dir,e2',e3',m) 
        expr', { TotalSize=AddTotalSizes einfos + forAndWhileLoopSize;
                 FunctionSize=AddFunctionSizes einfos + forAndWhileLoopSize;
                 HasEffect=eff;
                 MightMakeCriticalTailcall=false;
                 Info=UnknownValue }

//-------------------------------------------------------------------------
// Optimize/analyze a set of recursive bindings
//------------------------------------------------------------------------- 

and OptimizeLetRec cenv env (binds,bodyExpr,m) =
    if verboseOptimizations then dprintf "OptimizeLetRec\n";
    let vs = binds |> FlatList.map (fun v -> v.Var) in 
    let env = bind_internal_vspecs_to_unknown cenv vs env 
    let binds',env = OptimizeBindings cenv true env binds 
    let bodyExpr',einfo = OptimizeExpr cenv env bodyExpr 
    // REVIEW: graph analysis to determine which items are unused 
    // Eliminate any unused bindings, as in let case 
    let binds'',bindinfos = 
        let fvs0 = free_in_expr CollectLocals bodyExpr' 
        let fvsN = FlatList.map (fst >> free_in_rhs CollectLocals) binds' 
        let fvs  = FlatList.fold union_freevars fvs0 fvsN 
        SplitValuesByIsUsedOrHasEffect cenv m fvs.FreeLocals binds'
    // Trim out any optimization info that involves escaping values 
    let evalue' = AbstractExprInfoByVars m (FlatList.to_list vs,[]) einfo.Info 
    // REVIEW: size of constructing new closures - should probably add #freevars + #recfixups here 
    let bodyExpr' = TExpr_letrec(binds'',bodyExpr',m,NewFreeVarsCache()) 
    let info = CombineValueInfos (einfo :: FlatList.to_list bindinfos) evalue' 
    bodyExpr', info

//-------------------------------------------------------------------------
// Optimize/analyze a linear sequence of sequentioanl execution or 'let' bindings.
//------------------------------------------------------------------------- 

and OptimizeLinearExpr cenv env expr contf =
    if verboseOptimizations then dprintf "OptimizeLinearExpr\n";
    let expr = if cenv.settings.ExpandStructrualValues() then ExpandStructuralBinding cenv env expr else expr 
    match expr with 
    | TExpr_seq (e1,e2,flag,spSeq,m) -> 
      if verboseOptimizations then dprintf "OptimizeLinearExpr: seq\n";
      let e1',e1info = OptimizeExpr cenv env e1 
      OptimizeLinearExpr cenv env e2 (contf << (fun (e2',e2info) -> 
        if flag = NormalSeq && cenv.settings.EliminateSequential () && not e1info.HasEffect then 
            e2', e2info
        else 
            TExpr_seq(e1',e2',flag,spSeq,m),
            { TotalSize = e1info.TotalSize + e2info.TotalSize;
              FunctionSize = e1info.FunctionSize + e2info.FunctionSize;
              HasEffect = flag <> NormalSeq || e1info.HasEffect || e2info.HasEffect;
              MightMakeCriticalTailcall = (if flag = NormalSeq then e2info.MightMakeCriticalTailcall else  e1info.MightMakeCriticalTailcall || e2info.MightMakeCriticalTailcall)
              Info = UnknownValue (* can't propagate value: must access result of computation for its effects *) }))

    | TExpr_let (bind,body,m,_) ->  
      if verboseOptimizations then dprintf "OptimizeLinearExpr: let\n";
      let (bind',bindingInfo),env = OptimizeBinding cenv false env bind 
      OptimizeLinearExpr cenv env body (contf << (fun (body',bodyInfo) ->  
        if ValueIsUsedOrHasEffect cenv m (free_in_expr CollectLocals body').FreeLocals (bind',bindingInfo) then
            (* Eliminate let bindings on the way back up *)
            let expr',adjust = TryEliminateLet cenv env  bind' body' m 
            expr',
            { TotalSize = bindingInfo.TotalSize + bodyInfo.TotalSize + adjust; 
              FunctionSize = bindingInfo.FunctionSize + bodyInfo.FunctionSize + adjust; 
              HasEffect=bindingInfo.HasEffect || bodyInfo.HasEffect;
              MightMakeCriticalTailcall = bodyInfo.MightMakeCriticalTailcall; // discard tailcall info from binding - not in tailcall position
              Info = UnknownValue }
        else 
            (* On the way back up: Trim out any optimization info that involves escaping values on the way back up *)
            let evalue' = AbstractExprInfoByVars bind'.Var.Range ([bind'.Var],[]) bodyInfo.Info 
            body',
            { TotalSize = bindingInfo.TotalSize + bodyInfo.TotalSize - local_var_size (* eliminated a local var *); 
              FunctionSize = bindingInfo.FunctionSize + bodyInfo.FunctionSize - local_var_size (* eliminated a local var *); 
              HasEffect=bindingInfo.HasEffect || bodyInfo.HasEffect;
              MightMakeCriticalTailcall = bodyInfo.MightMakeCriticalTailcall; // discard tailcall info from binding - not in tailcall position
              Info = evalue' } ))

    | _ -> contf (OptimizeExpr cenv env expr)

//-------------------------------------------------------------------------
// Optimize/analyze a try/finally construct.
//------------------------------------------------------------------------- 
  
and OptimizeTryFinally cenv env (spTry,spFinally,e1,e2,m,ty) =
    if verboseOptimizations then dprintf "OptimizeTryFinally\n";
    let e1',e1info = OptimizeExpr cenv env e1 
    let e2',e2info = OptimizeExpr cenv env e2 
    let info = 
        { TotalSize = e1info.TotalSize + e2info.TotalSize + tryFinallySize;
          FunctionSize = e1info.FunctionSize + e2info.FunctionSize + tryFinallySize;
          HasEffect = e1info.HasEffect || e2info.HasEffect;
          MightMakeCriticalTailcall = false; // no tailcalls from inside in try/finally
          Info = UnknownValue } 
    (* try-finally, so no effect means no exception can be raised, so just sequence the finally *)
    if cenv.settings.EliminateTryCatchAndTryFinally () && not e1info.HasEffect then 
        let sp = 
            match spTry with 
            | SequencePointAtTry _ -> SequencePointsAtSeq 
            | NoSequencePointAtTry -> SuppressSequencePointOnExprOfSequential
        TExpr_seq(e1',e2',ThenDoSeq,sp,m),info 
    else
        mk_try_finally cenv.g (e1',e2',m,ty,spTry,spFinally), 
        info

//-------------------------------------------------------------------------
// Optimize/analyze a try/catch construct.
//------------------------------------------------------------------------- 
  
and OptimizeTryCatch cenv env (e1,vf,ef,vh,eh,m,ty,spTry,spWith) =
    if verboseOptimizations then dprintf "OptimizeTryCatch\n";
    let e1',e1info = OptimizeExpr cenv env e1    
    // try-catch, so no effect means no exception can be raised, so discard the catch 
    if cenv.settings.EliminateTryCatchAndTryFinally () && not e1info.HasEffect then 
        e1',e1info 
    else
        let envinner = bind_internal_vspec_to_unknown cenv vf (bind_internal_vspec_to_unknown cenv vh env)
        let ef',efinfo = OptimizeExpr cenv envinner ef 
        let eh',ehinfo = OptimizeExpr cenv envinner eh 
        let info = 
            { TotalSize = e1info.TotalSize + efinfo.TotalSize+ ehinfo.TotalSize  + tryCatchSize;
              FunctionSize = e1info.FunctionSize + efinfo.FunctionSize+ ehinfo.FunctionSize  + tryCatchSize;
              HasEffect = e1info.HasEffect || efinfo.HasEffect || ehinfo.HasEffect;
              MightMakeCriticalTailcall = false;
              Info = UnknownValue } 
        mk_try_catch cenv.g (e1',vf,ef',vh,eh',m,ty,spTry,spWith), 
        info

//-------------------------------------------------------------------------
// Optimize/analyze a while loop
//------------------------------------------------------------------------- 
  
and OptimizeWhileLoop cenv env  (spWhile,e1,e2,m) =
    if verboseOptimizations then dprintf "OptimizeWhileLoop\n";
    let e1',e1info = OptimizeExpr cenv env e1 
    let e2',e2info = OptimizeExpr cenv env e2 
    mk_while cenv.g (spWhile,e1',e2',m), 
    { TotalSize = e1info.TotalSize + e2info.TotalSize + forAndWhileLoopSize;
      FunctionSize = e1info.FunctionSize + e2info.FunctionSize + forAndWhileLoopSize;
      HasEffect = true; (* may not terminate *)
      MightMakeCriticalTailcall = false;
      Info = UnknownValue }

//-------------------------------------------------------------------------
// Optimize/analyze a call to a 'member' constraint. Try to resolve the call to 
// a witness (should always be possible due to compulsory inlining of any
// code that contains calls to member constraints, except when analyzing 
// not-yet-inlined generic code)
//------------------------------------------------------------------------- 
 

and OptimizeTraitCall cenv env   (traitInfo, args, m) =

    // Resolve the static overloading early (during the compulsory rewrite phase) so we can inline. 
    match ConstraintSolver.CodegenWitnessThatTypSupportsTraitConstraint cenv.g cenv.amap m traitInfo with
           
    | OkResult (_,Some(minfo,minst)) 
        // Limitation related to bug 1281:   If we resolve to an instance method on a struct and we haven't yet taken the address of the object 
        when not (Infos.minfo_is_struct cenv.g minfo && minfo.IsInstance) -> 
        
        let expr = Infos.MakeMethInfoCall cenv.amap m minfo minst args 
        OptimizeExpr cenv env expr

    // resolution fails when optimizing generic code 
    |  _ -> 
        let args',arginfos = OptimizeExprsThenConsiderSplits cenv env args 
        OptimizeExprOpFallback cenv env (TOp_trait_call(traitInfo),[],args,m) arginfos UnknownValue 

//-------------------------------------------------------------------------
// Make optimization decisions once we know the optimization information
// for a value
//------------------------------------------------------------------------- 

and TryOptimizeVal cenv env (mustInline,valInfoForVal,m) = 
    match valInfoForVal with 
    (* Inline constants immediately *)
    | ConstValue (c,ty) -> Some (TExpr_const (c,m,ty))
    | SizeValue (_,detail) -> TryOptimizeVal cenv env (mustInline,detail,m) 
    | ValValue (v',detail) -> 
         if verboseOptimizations then dprintf "TryOptimizeVal, ValValue, valInfoForVal = %s\n" (showL(exprValueInfoL valInfoForVal));
         (* Inline values bound to other values immediately *)
         (* if verbose then dprintf "Considering inlining value %a to value %a near %a\n" output_val_ref v output_locval_ref v' output_range m;  *)
         match  TryOptimizeVal cenv env (mustInline,detail,m) with 
          (* Prefer to inline using the more specific info if possible *)
          | Some e -> Some e
          (* If the more specific info didn't reveal an inline then use the value *)
          | None ->  Some(expr_for_vref m v')
    | ConstExprValue(size,expr) ->
        if verboseOptimizations then dprintf "Inlining constant expression value at %a\n"  output_range m;
        Some (RemarkExpr m (copy_expr cenv.g CloneAllAndMarkExprValsAsCompilerGenerated expr))
    | CurriedLambdaValue (_,_,_,expr,_) when mustInline ->
        if verboseOptimizations then dprintf "Inlining mustinline-lambda at %a\n"  output_range m;
        Some (RemarkExpr m (copy_expr cenv.g CloneAllAndMarkExprValsAsCompilerGenerated expr))
    | TupleValue _ | UnionCaseValue _ | RecdValue _ when mustInline -> failwith "tuple, union and record values cannot be marked 'inline'"
    | UnknownValue when mustInline -> warning(Error("a value marked as 'inline' has an unexpected value",m)); None
    | _ when mustInline -> warning(Error("a value marked as 'inline' could not be inlined",m)); None
    | _ -> None 
  
and TryOptimizeValInfo cenv env m vinfo = 
    if vinfo.HasEffect then None else TryOptimizeVal cenv env (false,vinfo.Info ,m)

//-------------------------------------------------------------------------
// Add 'v1 = v2' information into the information stored about a value
//------------------------------------------------------------------------- 
  
and AddValEqualityInfo g m (v:ValRef) info =
    if v.IsMutable then 
        /// the env assumes known-values do not change 
        info 
    else 
        {info with Info= MakeValueInfoForValue g m v info.Info}

//-------------------------------------------------------------------------
// Optimize/analyze a use of a value
//------------------------------------------------------------------------- 

and OptimizeVal cenv env expr (v:ValRef,m) =
    let valInfoForVal = GetInfoForVal cenv env m v 

    match TryOptimizeVal cenv env (v.MustInline,valInfoForVal.ValExprInfo ,m) with
    | Some e -> 
       // don't reoptimize inlined lambdas until they get applied to something
       match e with 
       | TExpr_tlambda _ 
       | TExpr_lambda _ ->
           e, (AddValEqualityInfo cenv.g m v 
                    { Info=valInfoForVal.ValExprInfo; 
                      HasEffect=false; 
                      MightMakeCriticalTailcall = false;
                      FunctionSize=10; 
                      TotalSize=10})
       | _ -> 
           let e,einfo = OptimizeExpr cenv env e 
           e,AddValEqualityInfo cenv.g m v einfo 

    | None -> 
       if v.MustInline  then error(Error("failed to inline the value '"^v.MangledName^"' marked 'inline', perhaps because a recursive value was marked 'inline'",m));
       expr,(AddValEqualityInfo cenv.g m v 
                    { Info=valInfoForVal.ValExprInfo; 
                      HasEffect=false; 
                      MightMakeCriticalTailcall = false;
                      FunctionSize=1; 
                      TotalSize=1})

//-------------------------------------------------------------------------
// Attempt to replace an application of a value by an alternative value.
//------------------------------------------------------------------------- 

and StripToNominalTyconRef cenv ty = 
    if is_stripped_tyapp_typ cenv.g ty then dest_stripped_tyapp_typ cenv.g ty 
    elif is_tuple_typ cenv.g ty then 
        let tyargs = dest_tuple_typ cenv.g ty
        compiled_tuple_tcref cenv.g tyargs, tyargs 
    else failwith "StripToNominalTyconRef: unreachable" 
      

and CanDevirtualizeApplication cenv v vref ty args  = 
     cenv.g.vref_eq v vref
     && not (is_unit_typ cenv.g ty)
     && is_stripped_tyapp_typ cenv.g ty // || (is_tuple_typ cenv.g ty && List.length (dest_tuple_typ cenv.g ty) < maxTuple)) 
     // Exclusion: Some unions have null as representations 
     && not (IsUnionTypeWithNullAsTrueValue cenv.g (deref_tycon (fst(StripToNominalTyconRef cenv ty))))  
     // If we de-virtualize an operation on structs then we have to take the address of the object argument
     // Hence we have to actually have the object argument available to us,
     && (not (is_struct_typ cenv.g ty) || nonNil args) 

and TakeAddressOfStructArgumentIfNeeded cenv (vref:ValRef) ty args m =
    if vref.IsInstanceMember && is_struct_typ cenv.g ty then 
        match args with 
        | objArg::rest -> 
            // REVIEW: we set NeverMutates. This is valid because we only ever use DevirtualizeApplication to transform 
            // known calls to known generated F# code for CompareTo, Equals and GetHashCode.
            // If we ever reuse DevirtualizeApplication to transform an arbitrary virtual call into a 
            // direct call then this assumption is not valid.
            let wrap,objArgAddress = mk_expra_of_expr cenv.g true NeverMutates objArg m 
            wrap, (objArgAddress::rest)
        | _ -> 
            // no wrapper, args stay the same 
            (fun x -> x), args
    else
        (fun x -> x), args

and DevirtualizeApplication cenv env (vref:ValRef) ty tyargs args m =
    let wrap,args = TakeAddressOfStructArgumentIfNeeded cenv vref ty args m
    let transformedExpr = wrap (MakeApplicationAndBetaReduce cenv.g (expr_for_vref m vref,vref.Type,(if isNil tyargs then [] else [tyargs]),args,m))
    OptimizeExpr cenv env transformedExpr

    
  
and TryDevirtualizeApplication cenv env (f,tyargs,args,m) =
    match f,tyargs,args with 

    // Optimize/analyze calls to LanguagePrimitives.HashCompare.GenericComparisonIntrinsic when type is known 
    // to be augmented with a visible comparison value. 
    //
    // e.g rewrite 
    //      'LanguagePrimitives.HashCompare.GenericComparisonIntrinsic (x:C) (y:C)' 
    //  --> 'x.CompareTo(y:C)' where this is a direct call to the implementation of CompareTo, i.e.
    //        C::CompareTo(C)
    //    not C::CompareTo(obj)
    //
    // If C is a struct type then we have to take the address of 'c'
    
    | TExpr_val(v,_,_),[ty],_ when CanDevirtualizeApplication cenv v cenv.g.generic_comparison_inner_vref ty args ->
         
        let tcref,tyargs = StripToNominalTyconRef cenv ty
        match tcref.TypeContents.tcaug_compare with 
        | Some (_,vref)  -> Some (DevirtualizeApplication cenv env vref ty tyargs args m)
        | _ -> None
        
    | TExpr_val(v,_,_),[ty],_ when CanDevirtualizeApplication cenv v cenv.g.generic_comparison_withc_inner_vref ty args ->
         
        let tcref,tyargs = StripToNominalTyconRef cenv ty
        match tcref.TypeContents.tcaug_compare_withc, args with 
        | Some vref, [comp; x; y]  -> 
            // the target takes a tupled argument, so we need to reorder the arg expressions in the
            // arg list, and create a tuple of y & comp
            // push the comparer to the end and box the argument
            let args2 = [x; mk_tupled_notypes cenv.g m [mk_coerce(y,cenv.g.obj_ty,m,ty) ; comp]]
            Some (DevirtualizeApplication cenv env vref ty tyargs args2 m)
        | _ -> None
        
    // Optimize/analyze calls to LanguagePrimitives.HashCompare.GenericEqualityIntrinsic when type is known 
    // to be augmented with a visible comparison value. 
    | TExpr_val(v,_,_),[ty],_ when CanDevirtualizeApplication cenv v cenv.g.generic_equality_inner_vref ty args ->
         
        let tcref,tyargs = StripToNominalTyconRef cenv ty 
        match tcref.TypeContents.tcaug_equals with 
        | Some (_,vref)  -> Some (DevirtualizeApplication cenv env vref ty tyargs args m)
        | _ -> None
        
    // Optimize/analyze calls to LanguagePrimitives.HashCompare.GenericEqualityWithComparerFast
    | TExpr_val(v,_,_),[ty],_ when CanDevirtualizeApplication cenv v cenv.g.generic_equality_withc_inner_vref ty args ->
        let tcref,tyargs = StripToNominalTyconRef cenv ty
        match tcref.TypeContents.tcaug_hash_and_equals_withc, args with
        | Some (_,vref), [comp; x; y]  -> 
            // push the comparer to the end and box the argument
            let args2 = [x; mk_tupled_notypes cenv.g m [mk_coerce(y,cenv.g.obj_ty,m,ty) ; comp]]
            Some (DevirtualizeApplication cenv env vref ty tyargs args2 m)
        | _ -> None 
        

    // Optimize/analyze calls to LanguagePrimitives.HashCompare.GenericHashIntrinsic
    | TExpr_val(v,_,_),[ty],_ when CanDevirtualizeApplication cenv v cenv.g.generic_hash_inner_vref ty args ->
        let tcref,tyargs = StripToNominalTyconRef cenv ty
        match tcref.TypeContents.tcaug_hash_and_equals_withc, args with
        | Some (vref,_), [x] -> 
            let args2 = [x; mk_call_get_generic_equality_comparer cenv.g m]
            Some (DevirtualizeApplication cenv env vref ty tyargs args2 m)
        | _ -> None 
        
    // Optimize/analyze calls to LanguagePrimitives.HashCompare.GenericHashWithComparerIntrinsic
    | TExpr_val(v,_,_),[ty],_ when  CanDevirtualizeApplication cenv v cenv.g.generic_hash_withc_inner_vref ty args ->
        let tcref,tyargs = StripToNominalTyconRef cenv ty
        match tcref.TypeContents.tcaug_hash_and_equals_withc, args with
        | Some (vref,_), [comp; x]  -> 
            let args2 = [x; comp]
            Some (DevirtualizeApplication cenv env vref ty tyargs args2 m)
        | _ -> None 

    // Optimize/analyze calls to LanguagePrimitives.HashCompare.GenericComparisonWithComparerIntrinsic for tuple types
    | TExpr_val(v,_,_),[ty],_ when  cenv.g.vref_eq v cenv.g.generic_comparison_inner_vref && is_tuple_typ cenv.g ty ->
        let tyargs = dest_tuple_typ cenv.g ty 
        let vref = 
            match tyargs.Length with 
            | 2 -> Some cenv.g.generic_compare_withc_tuple2_vref 
            | 3 -> Some cenv.g.generic_compare_withc_tuple3_vref 
            | 4 -> Some cenv.g.generic_compare_withc_tuple4_vref 
            | 5 -> Some cenv.g.generic_compare_withc_tuple5_vref 
            | _ -> None
        match vref with 
        | Some vref -> Some (DevirtualizeApplication cenv env vref ty tyargs (mk_call_get_generic_comparer cenv.g m :: args) m)            
        | None -> None
        
    // Optimize/analyze calls to LanguagePrimitives.HashCompare.GenericHashWithComparerIntrinsic for tuple types
    | TExpr_val(v,_,_),[ty],_ when  cenv.g.vref_eq v cenv.g.generic_hash_inner_vref && is_tuple_typ cenv.g ty ->
        let tyargs = dest_tuple_typ cenv.g ty 
        let vref = 
            match tyargs.Length with 
            | 2 -> Some cenv.g.generic_hash_withc_tuple2_vref 
            | 3 -> Some cenv.g.generic_hash_withc_tuple3_vref 
            | 4 -> Some cenv.g.generic_hash_withc_tuple4_vref 
            | 5 -> Some cenv.g.generic_hash_withc_tuple5_vref 
            | _ -> None
        match vref with 
        | Some vref -> Some (DevirtualizeApplication cenv env vref ty tyargs (mk_call_get_generic_equality_comparer cenv.g m :: args) m)            
        | None -> None
        
    // Optimize/analyze calls to LanguagePrimitives.HashCompare.GenericHashWithComparerIntrinsic for tuple types
    | TExpr_val(v,_,_),[ty],_ when  cenv.g.vref_eq v cenv.g.generic_equality_inner_vref && is_tuple_typ cenv.g ty ->
        let tyargs = dest_tuple_typ cenv.g ty 
        let vref = 
            match tyargs.Length with 
            | 2 -> Some cenv.g.generic_equals_withc_tuple2_vref 
            | 3 -> Some cenv.g.generic_equals_withc_tuple3_vref 
            | 4 -> Some cenv.g.generic_equals_withc_tuple4_vref 
            | 5 -> Some cenv.g.generic_equals_withc_tuple5_vref 
            | _ -> None
        match vref with 
        | Some vref -> Some (DevirtualizeApplication cenv env vref ty tyargs (mk_call_get_generic_equality_comparer cenv.g m :: args) m)            
        | None -> None
        
        
    // Optimize/analyze calls to LanguagePrimitives.HashCompare.GenericComparisonWithComparerIntrinsic for tuple types
    | TExpr_val(v,_,_),[ty],_ when  cenv.g.vref_eq v cenv.g.generic_comparison_withc_inner_vref && is_tuple_typ cenv.g ty ->
        let tyargs = dest_tuple_typ cenv.g ty 
        let vref = 
            match tyargs.Length with 
            | 2 -> Some cenv.g.generic_compare_withc_tuple2_vref 
            | 3 -> Some cenv.g.generic_compare_withc_tuple3_vref 
            | 4 -> Some cenv.g.generic_compare_withc_tuple4_vref 
            | 5 -> Some cenv.g.generic_compare_withc_tuple5_vref 
            | _ -> None
        match vref with 
        | Some vref -> Some (DevirtualizeApplication cenv env vref ty tyargs args m)            
        | None -> None
        
    // Optimize/analyze calls to LanguagePrimitives.HashCompare.GenericHashWithComparerIntrinsic for tuple types
    | TExpr_val(v,_,_),[ty],_ when  cenv.g.vref_eq v cenv.g.generic_hash_withc_inner_vref && is_tuple_typ cenv.g ty ->
        let tyargs = dest_tuple_typ cenv.g ty 
        let vref = 
            match tyargs.Length with 
            | 2 -> Some cenv.g.generic_hash_withc_tuple2_vref 
            | 3 -> Some cenv.g.generic_hash_withc_tuple3_vref 
            | 4 -> Some cenv.g.generic_hash_withc_tuple4_vref 
            | 5 -> Some cenv.g.generic_hash_withc_tuple5_vref 
            | _ -> None
        match vref with 
        | Some vref -> Some (DevirtualizeApplication cenv env vref ty tyargs args m)            
        | None -> None
        
    // Optimize/analyze calls to LanguagePrimitives.HashCompare.GenericEqualityWithComparerIntrinsic for tuple types
    | TExpr_val(v,_,_),[ty],_ when  cenv.g.vref_eq v cenv.g.generic_equality_withc_inner_vref && is_tuple_typ cenv.g ty ->
        let tyargs = dest_tuple_typ cenv.g ty 
        let vref = 
            match tyargs.Length with 
            | 2 -> Some cenv.g.generic_equals_withc_tuple2_vref 
            | 3 -> Some cenv.g.generic_equals_withc_tuple3_vref 
            | 4 -> Some cenv.g.generic_equals_withc_tuple4_vref 
            | 5 -> Some cenv.g.generic_equals_withc_tuple5_vref 
            | _ -> None
        match vref with 
        | Some vref -> Some (DevirtualizeApplication cenv env vref ty tyargs args m)            
        | None -> None
        

    // Calls to LanguagePrimitives.IntrinsicFunctions.UnboxGeneric can be optimized to calls to UnboxFast when we know that the 
    // target type isn't 'NullNotLiked', i.e. that the target type is not an F# union, record etc. 
    // Note UnboxFast is just the .NET IL 'unbox.any' instruction. 
    | TExpr_val(v,_,_),[ty],_ when cenv.g.vref_eq v cenv.g.unbox_vref && 
                                   can_use_unbox_fast cenv.g ty ->

        Some(DevirtualizeApplication cenv env cenv.g.unbox_fast_vref ty tyargs args m)
        
    // Calls to LanguagePrimitives.IntrinsicFunctions.TypeTestGeneric can be optimized to calls to TypeTestFast when we know that the 
    // target type isn't 'NullNotTrueValue', i.e. that the target type is not an F# union, record etc. 
    // Note TypeTestFast is just the .NET IL 'isinst' instruction followed by a non-null comparison 
    | TExpr_val(v,_,_),[ty],_ when cenv.g.vref_eq v cenv.g.istype_vref && 
                                   can_use_istype_fast cenv.g ty ->

        Some(DevirtualizeApplication cenv env cenv.g.istype_fast_vref ty tyargs args m)
        
    | _ -> None

/// Attempt to inline an application of a known value at callsites
and TryInlineApplication cenv env (f0',finfo) (tyargs,args,m) =
    if verboseOptimizations then dprintf "Considering inlining app near %a\n"  output_range m; 
    match finfo.Info with 
    | StripLambdaValue (lambdaId,arities,size,f2,f2ty) when

       (if verboseOptimizations then dprintf "Considering inlining lambda near %a, size = %d, finfo.HasEffect = %b\n"  output_range m size finfo.HasEffect;
        cenv.optimizing &&
        cenv.settings.InlineLambdas () &&
        not finfo.HasEffect &&
        (* Don't inline recursively! *)
        not (Zset.mem lambdaId env.dontInline) &&
        (if verboseOptimizations then dprintf "Recursion ok, #tyargs = %d, #args = %d, #arities=%d\n" (List.length tyargs) (List.length args) arities;
         (* Check the number of argument groups is enough to saturate the lambdas of the target. *)
         (if List.filter (fun t -> match t with TType_measure _ -> false | _ -> true) tyargs = [] then 0 else 1) + List.length args >= arities &&
          (if verboseOptimizations then dprintn "Enough args"; 
           (if size > cenv.settings.lambdaInlineThreshold + List.length args then
              if verboseOptimizations then dprintf "Not inlining lambda near %a because size = %d\n" output_range m size; 
              false
            else true)))) ->

        if verboseOptimizations then dprintf "Inlining lambda near %a\n"  output_range m;
  (* ----------       Printf.printf "Inlining lambda near %a = %s\n"  output_range m (showL (ExprL f2));  (* JAMES: *) ----------*)
        let f2' = RemarkExpr m (copy_expr cenv.g CloneAllAndMarkExprValsAsCompilerGenerated f2)
        if verboseOptimizations then dprintf "--- TryInlineApplication, optimizing arguments\n";

        // REVIEW: this is a cheapshot way of optimizing the arg expressions as well without the restriction of recursive  
        // inlining kicking into effect 
        let args' = args |> List.map (fun e -> let e',einfo = OptimizeExpr cenv env e in e') 
        // Beta reduce. MakeApplicationAndBetaReduce cenv.g does all the hard work. 
        if verboseOptimizations then dprintf "--- TryInlineApplication, beta reducing \n";
        let expr' = MakeApplicationAndBetaReduce cenv.g (f2',f2ty,[tyargs],args',m)
        if verboseOptimizations then dprintf "--- TryInlineApplication, reoptimizing\n";
        Some (OptimizeExpr cenv {env with dontInline= Zset.add lambdaId env.dontInline} expr')
          
    | _ -> None

//-------------------------------------------------------------------------
// Optimize/analyze an application of a function to type and term arguments
//------------------------------------------------------------------------- 

and OptimizeApplication cenv env (f0,f0ty,tyargs,args,m) =
    if verboseOptimizations then dprintf "--> OptimizeApplication\n";
    let f0',finfo = OptimizeExpr cenv env f0 
    if verboseOptimizations then dprintf "--- OptimizeApplication, trying to devirtualize\n";
    match TryDevirtualizeApplication cenv env (f0,tyargs,args,m) with 
    | Some res -> 
        if verboseOptimizations then dprintf "<-- OptimizeApplication, devirtualized\n";
        res
    | None -> 

    match TryInlineApplication cenv env (f0',finfo) (tyargs,args,m) with 
    | Some res -> 
        if verboseOptimizations then dprintf "<-- OptimizeApplication, inlined\n";
        res
    | None -> 

    let shapes = 
        match f0' with 
        | TExpr_val(vref,_,_) when isSome vref.TopValInfo -> 
            let (TopValInfo(kinds,detupArgsL,_)) = the vref.TopValInfo 
            let nargs = (args.Length) 
            let nDetupArgsL = detupArgsL.Length
            let nShapes = min nargs nDetupArgsL 
            let detupArgsShapesL = 
                List.take  nShapes detupArgsL |> List.map (fun detupArgs -> 
                    match detupArgs with 
                    | [] | [_] -> UnknownValue
                    | _ -> TupleValue(Array.of_list (List.map (fun _ -> UnknownValue) detupArgs))) 
            detupArgsShapesL @ List.replicate (nargs - nShapes) UnknownValue
            
        | _ -> args |> List.map (fun _ -> UnknownValue) 

    let args',arginfos = OptimizeExprsThenReshapeAndConsiderSplits cenv env (List.zip shapes args) 
    if verboseOptimizations then dprintf "<-- OptimizeApplication, beta reducing\n";
    let expr' = MakeApplicationAndBetaReduce cenv.g (f0',f0ty, [tyargs],args',m) 
    
    match f0' with 
    | TExpr_lambda _ | TExpr_tlambda _ -> 
       (* we beta-reduced, hence reoptimize *)
        if verboseOptimizations then dprintf "<-- OptimizeApplication, beta reduced\n";
        OptimizeExpr cenv env expr'
    | _ -> 
        if verboseOptimizations then dprintf "<-- OptimizeApplication, regular\n";

        // Determine if this application is a critical tailcall
        let mayBeCriticalTailcall = 
            match f0' with 
            | KnownValApp(vref,typeArgs,otherArgs)   ->

                 // Check if this is a call to a function of known arity that has been inferred to not be a critical tailcall when used as a direct call
                 // This includes recursive calls to the function being defined (in which case we get a non-critical, closed-world tailcall).
                 // Note we also have to check the argument count to ensure this is a direct call (or a partial application).
                 let doesNotMakeCriticalTailcall = 
                     vref.MakesNoCriticalTailcalls || 
                     (let valInfoForVal = GetInfoForVal cenv env m vref  in valInfoForVal.ValMakesNoCriticalTailcalls) ||
                     (match env.functionVal with | None -> false | Some (v,_) -> vspec_eq vref.Deref v)
                 if doesNotMakeCriticalTailcall then
                    let numArgs = otherArgs.Length + args'.Length
                    match vref.TopValInfo with 
                    | Some i -> numArgs > i.NumCurriedArgs 
                    | None -> 
                    match env.functionVal with 
                    | Some (v,i) ->  numArgs > i.NumCurriedArgs
                    | None -> true // over-applicaiton of a known function, which presumably returns a function. This counts as an indirect call
                 else
                    true // application of a function that may make a critical tailcall
                
            | _ -> 
                // All indirect calls (calls to unknown functions) are assumed to be critical tailcalls 
                true

        expr', { TotalSize=finfo.TotalSize + AddTotalSizes arginfos;
                 FunctionSize=finfo.FunctionSize + AddFunctionSizes arginfos;
                 HasEffect=true;
                 MightMakeCriticalTailcall = mayBeCriticalTailcall;
                 Info=ValueOfExpr expr' }

//-------------------------------------------------------------------------
// Optimize/analyze a lambda expression
//------------------------------------------------------------------------- 
        
and OptimizeLambdas (vspec: Val option) cenv env topValInfo e ety = 
    if verboseOptimizations then dprintf "OptimizeLambdas, #argsl = %d, %a\n" topValInfo.NumCurriedArgs output_range (range_of_expr e) ;
    match e with 
    | TExpr_lambda (lambdaId,_,_,_,m,_,_)  
    | TExpr_tlambda(lambdaId,_,_,m,_,_) ->
        let isTopLevel = isSome vspec && vspec.Value.IsCompiledAsTopLevel
        let tps,basevopt,vsl,body,bodyty = IteratedAdjustArityOfLambda cenv.g cenv.amap topValInfo e
        let env = { env with functionVal = (match vspec with None -> None | Some v -> Some (v,topValInfo)) }
        let env = Option.fold_right (bind_internal_vspec_to_unknown cenv) basevopt env
        let env = BindTypeVarsToUnknown tps env
        let env = List.foldBack (bind_internal_vspecs_to_unknown cenv) vsl env
        let env = bind_internal_vspecs_to_unknown cenv (Option.to_list basevopt) env
        let body',bodyinfo = OptimizeExpr cenv env body
        let expr' = mk_basev_multi_lambdas m tps basevopt vsl (body',bodyty)
        let arities = List.length vsl
        let arities = if tps = [] then arities else 1+arities
        let bsize = bodyinfo.TotalSize
        if verboseOptimizations then dprintf "lambda @ %a, bsize = %d\n" output_range m bsize;

        
        /// Set the flag on the value indicating that direct calls can avoid a tailcall (which are expensive on .NET x86)
        /// MightMakeCriticalTailcall is true whenever the body of the method may itself do a useful tailcall, e.g. has
        /// an application in the last position.
        match vspec with 
        | Some v -> 
            if not bodyinfo.MightMakeCriticalTailcall then 
                set_notailcall_hint_of_vflags v.Data true
            
            // UNIT TEST HOOK: report analysis results for the first optimization phase 
            if cenv.settings.reportingPhase && not v.IsCompilerGenerated then 
                if cenv.settings.reportNoNeedToTailcall then 
                    if bodyinfo.MightMakeCriticalTailcall then
                        printfn "value %s at line %d may make a critical tailcall" v.DisplayName (start_line_of_range v.Range) 
                    else 
                        printfn "value %s at line %d does not make a critical tailcall" v.DisplayName (start_line_of_range v.Range) 
                if cenv.settings.reportTotalSizes then 
                    printfn "value %s at line %d has total size %d" v.DisplayName (start_line_of_range v.Range)  bodyinfo.TotalSize 
                if cenv.settings.reportFunctionSizes then 
                    printfn "value %s at line %d has method size %d" v.DisplayName (start_line_of_range v.Range)  bodyinfo.FunctionSize
                if cenv.settings.reportHasEffect then 
                    if bodyinfo.HasEffect then
                        printfn "function %s at line %d causes side effects or may not terminate" v.DisplayName (start_line_of_range v.Range) 
                    else 
                        printfn "function %s at line %d causes no side effects" v.DisplayName (start_line_of_range v.Range) 
        | _ -> 
            () 

        // can't inline any values with semi-recursive object references to self or base 
        let valu =   
          match basevopt with 
          | None -> CurriedLambdaValue (lambdaId,arities,bsize,expr',ety) 
          | _ -> UnknownValue

        expr', { TotalSize=bsize + (if isTopLevel then methodDefnTotalSize else closureTotalSize); (* estimate size of new syntactic closure - expensive, in contrast to a method *)
                 FunctionSize=1; 
                 HasEffect=false;
                 MightMakeCriticalTailcall = false;
                 Info= valu; }
    | _ -> OptimizeExpr cenv env e 
      


//-------------------------------------------------------------------------
// Recursive calls that first try to make an expression "fit" the a shape
// where it is about to be consumed.
//------------------------------------------------------------------------- 

and OptimizeExprsThenReshapeAndConsiderSplits cenv env exprs = 
    match exprs with 
    | [] -> no_exprs 
    | _ -> OptimizeList (OptimizeExprThenReshapeAndConsiderSplit cenv env) exprs

and OptimizeExprsThenConsiderSplits cenv env exprs = 
    match exprs with 
    | [] -> no_exprs 
    | _ -> OptimizeList (OptimizeExprThenConsiderSplit cenv env) exprs

and OptimizeFlatExprsThenConsiderSplits cenv env (exprs:FlatExprs) = 
    if FlatList.isEmpty exprs then no_FlatExprs
    else OptimizeFlatList (OptimizeExprThenConsiderSplit cenv env) exprs

and OptimizeExprThenReshapeAndConsiderSplit cenv env (shape,e) = 
    OptimizeExprThenConsiderSplit cenv env (ReshapeExpr cenv (shape,e))

and OptimizeDecisionTreeTargets cenv env m ty targets = 
    OptimizeList (OptimizeDecisionTreeTarget cenv env m ty) (Array.to_list targets)

and ReshapeExpr cenv (shape,e) = 
  match shape,e with 
  | TupleValue(subshapes), TExpr_val(vref,vFlags,m) ->
      let tinst = dest_tuple_typ cenv.g (type_of_expr cenv.g e)
      mk_tupled cenv.g m (List.mapi (fun i subshape -> ReshapeExpr cenv (subshape,mk_tuple_field_get(e,tinst,i,m))) (Array.to_list subshapes)) tinst
  | _ ->  
      e

and OptimizeExprThenConsiderSplit cenv env e = 
  let e',einfo = OptimizeExpr cenv env e
  (* ALWAYS consider splits for enormous sub terms here - otherwise we will create invalid .NET programs  *)
  ConsiderSplitToMethod true cenv.settings.veryBigExprSize cenv env (e',einfo) 

//-------------------------------------------------------------------------
// Decide whether to List.unzip a sub-expression into a new method
//------------------------------------------------------------------------- 

and ComputeSplitToMethodCondition flag threshold cenv env (e,einfo) = 
    flag &&
    // don't mess with taking guaranteed tailcalls if used with --no-tailcalls! 
    !Msilxlib.tailcall_implementation <> Ilxsettings.NoTailcalls &&
    einfo.FunctionSize >= threshold &&
    (let fvs = free_in_expr CollectLocals e
     not fvs.UsesUnboundRethrow  &&
     // We can only split an expression out as a method if certain conditions are met. 
     // It can't use any protected or base calls 
     not fvs.UsesMethodLocalConstructs &&
     fvs.FreeLocals |> Zset.for_all (fun v -> 
          // no direct-self-recursive refrences
          not (vspec_map_mem v env.dontSplitVars) &&
          (v.TopValInfo.IsSome ||
            // All the free variables (apart from things with an arity, i.e. compiled as methods) should be normal, i.e. not base/this etc. 
            (v.BaseOrThisInfo = NormalVal && 
             // None of them should be byrefs 
             not (is_byref_typ cenv.g v.Type) && 
             //  None of them should be local polymorphic constrained values 
             not (IsGenericValWithGenericContraints cenv.g v) &&
             // None of them should be mutable 
             not v.IsMutable))))

and ConsiderSplitToMethod flag threshold cenv env (e,einfo) = 
    if ComputeSplitToMethodCondition flag threshold cenv env (e,einfo) then
        let m = (range_of_expr e)
        let uv,ue = mk_compgen_local m "unitVar" cenv.g.unit_ty
        let ty = type_of_expr cenv.g e
        let fv,fe = mk_compgen_local m (match env.latestBoundId with Some id -> id.idText^suffixForVariablesThatMayNotBeEliminated | None -> suffixForVariablesThatMayNotBeEliminated) (cenv.g.unit_ty --> ty)
        mk_invisible_let m fv (mk_lambda m uv (e,ty)) 
          (prim_mk_app (fe,(cenv.g.unit_ty --> ty)) [] [mk_unit cenv.g m] m),
        {einfo with FunctionSize=callSize }
    else
        e,einfo 

//-------------------------------------------------------------------------
// Optimize/analyze a pattern matching expression
//------------------------------------------------------------------------- 

and OptimizeMatch cenv env (spMatch,exprm,dtree,targets,m, ty) =
    if verboseOptimizations then dprintf "OptimizeMatch\n";
    // REVIEW: consider collecting, merging and using information flowing through each line of the decision tree to each target 
    let dtree',dinfo = OptimizeDecisionTree cenv env m dtree 
    let targets',tinfos = OptimizeDecisionTreeTargets cenv env m ty targets 
    let tinfo = CombineValueInfosUnknown tinfos
    let expr' = mk_and_optimize_match spMatch exprm m ty dtree' targets' 
    let einfo = 
        { TotalSize = dinfo.TotalSize + tinfo.TotalSize;
          FunctionSize = dinfo.FunctionSize + tinfo.FunctionSize;
          HasEffect = dinfo.HasEffect || tinfo.HasEffect;
          MightMakeCriticalTailcall=tinfo.MightMakeCriticalTailcall; // discard tailcall info from decision tree since it's not in tailcall position
          Info= UnknownValue }
    expr', einfo

//-------------------------------------------------------------------------
// Optimize/analyze a target of a decision tree
//------------------------------------------------------------------------- 

and OptimizeDecisionTreeTarget cenv env m ty (TTarget(vs,e,spTarget)) = 
    if verboseOptimizations then dprintf "OptimizeDecisionTreeTarget\n";
    (* REVIEW: this is where we should be using information collected for each target *)
    let env = bind_internal_vspecs_to_unknown cenv vs env 
    let e',einfo = OptimizeExpr cenv env e 
    let e',einfo = ConsiderSplitToMethod cenv.settings.abstractBigTargets cenv.settings.bigTargetSize cenv env (e',einfo) 
    let evalue' = AbstractExprInfoByVars m (FlatList.to_list vs,[]) einfo.Info 
    TTarget(vs,e',spTarget),
    { TotalSize=einfo.TotalSize; 
      FunctionSize=einfo.FunctionSize;
      HasEffect=einfo.HasEffect;
      MightMakeCriticalTailcall = einfo.MightMakeCriticalTailcall; 
      Info=evalue' }

//-------------------------------------------------------------------------
// Optimize/analyze a decision tree
//------------------------------------------------------------------------- 

and OptimizeDecisionTree cenv env m x =
    match x with 
    | TDSuccess (es,n) -> 
        let es', einfos = OptimizeFlatExprsThenConsiderSplits cenv env es 
        TDSuccess(es',n),CombineFlatValueInfosUnknown einfos
    | TDBind(bind,rest) -> 
        let (bind,binfo),envinner = OptimizeBinding cenv false env bind 
        let rest,rinfo = OptimizeDecisionTree cenv envinner m rest 

        if ValueIsUsedOrHasEffect cenv m (acc_free_in_dtree CollectLocals rest empty_freevars).FreeLocals (bind,binfo) then

            let info = CombineValueInfosUnknown [rinfo;binfo]
            // try to fold the let-binding into a single result expression
            match rest with 
            | TDSuccess([e],n) -> 
                let e,adjust = TryEliminateLet cenv env bind e m 
                TDSuccess([e],n),info
            | _ -> 
                TDBind(bind,rest),info

        else 
            rest,rinfo

    | TDSwitch (e,cases,dflt,m) -> 
        OptimizeSwitch cenv env (e,cases,dflt,m)

and TryOptimizeDecisionTreeTest cenv test vinfo = 
    match test,vinfo with 
    | TTest_unionconstr (c1,_), StripUnionCaseValue(c2,_) ->  Some(cenv.g.ucref_eq c1 c2)
    | TTest_array_length (n1,_),  _ -> None
    | TTest_const c1,StripConstValue(c2) -> if c1 = TConst_zero or c2 = TConst_zero then None else Some(c1=c2)
    | TTest_isnull,StripConstValue(c2) -> Some(c2=TConst_zero)
    | TTest_isinst (srcty1,tgty1), _ -> None
    // These should not occur in optimization
    | TTest_query (_,_,vrefOpt1,n1,apinfo1),_ -> None
    | _ -> None

/// Optimize/analyze a switch construct from pattern matching 
and OptimizeSwitch cenv env (e,cases,dflt,m) =
    if verboseOptimizations then dprintf "OptimizeSwitch\n";
    let e', einfo = OptimizeExpr cenv env e 

    let cases,dflt = 
        if cenv.settings.EliminateSwitch() && not einfo.HasEffect then
            // Attempt to find a definite success, i.e. the first case where there is definite success
            match (List.tryfind (function (TCase(d2,_)) when TryOptimizeDecisionTreeTest cenv d2 einfo.Info  = Some(true) -> true | _ -> false) cases) with 
            | Some(TCase(_,case)) -> [],Some(case)
            | _ -> 
                // Filter definite failures
                cases |> List.filter (function (TCase(d2,_)) when TryOptimizeDecisionTreeTest cenv d2 einfo.Info = Some(false) -> false | _ -> true), 
                dflt
        else
            cases,dflt 
    // OK, see what we're left with and continue
    match cases,dflt with 
    | [],Some case -> OptimizeDecisionTree cenv env m case
    | _ -> OptimizeSwitchFallback cenv env (e', einfo, cases,dflt,m)

and OptimizeSwitchFallback cenv env (e', einfo, cases,dflt,m) =
    let cases',cinfos = List.unzip (List.map (fun (TCase(discrim,e)) -> let e',einfo = OptimizeDecisionTree cenv env m e in TCase(discrim,e'),einfo) cases) 
    let dflt',dinfos = match dflt with None -> None,[] | Some df -> let df',einfo = OptimizeDecisionTree cenv env m df in Some df',[einfo] 
    let size = (List.length dinfos + List.length cinfos) * 2
    let info = CombineValueInfosUnknown (einfo :: cinfos @ dinfos)
    let info = { info with TotalSize = info.TotalSize + size; FunctionSize = info.FunctionSize + size;  }
    TDSwitch (e',cases',dflt',m),info

and OptimizeBinding cenv isRec env (TBind(v,e,spBind) as bind) =
    try 
        if verboseOptimizations then dprintf "OptimizeBinding\n";
        
        // The aim here is to stop method splitting for direct-self-tailcalls. We do more than that: if an expression
        // occurs in the body of recursively defined values RVS, then we refuse to split
        // any expression that contains a reference to any value in RVS.
        // This doesn't prevent splitting for mutually recursive references. See FSharp 1.0 bug 2892.
        let env = 
            if isRec then { env with dontSplitVars = vspec_map_add v () env.dontSplitVars } 
            else env
        
        let repr',einfo = 
            let env = if v.IsCompilerGenerated && isSome env.latestBoundId then env else {env with latestBoundId=Some v.Id} 
            let cenv = if v.InlineInfo = PseudoValue then { cenv with optimizing=false} else cenv 
            if verboseOptimizations then dprintf "OptimizeBinding --> OptimizeLambdas\n";
            let e',einfo = OptimizeLambdas (Some v) cenv env (InferArityOfExprBinding cenv.g v e)  e v.Type 
            let size = local_var_size 
            e',{einfo with FunctionSize=einfo.FunctionSize+size; TotalSize = einfo.TotalSize+size} 

        // Trim out optimization information for large lambdas we'll never inline
        // Trim out optimization information for expressions that call protected members 
        let rec cut ivalue = 
            match ivalue with
            | CurriedLambdaValue (_, arities, size, body,_) -> 
                if size > (cenv.settings.lambdaInlineThreshold + arities + 2) then 
                    if verboseOptimizations then dprintf "Discarding lambda for binding %s, size = %d, m = %a\n"  v.MangledName size output_range (range_of_expr body);
                    UnknownValue (* trim large *)
                else
                    let fvs = free_in_expr CollectLocals body
                    if fvs.UsesMethodLocalConstructs then
                        if verboseOptimizations then dprintf "Discarding lambda for binding %s because uses protected members, m = %a\n"  v.MangledName output_range (range_of_expr body);
                        UnknownValue (* trim protected *)
                    else
                        ivalue

            | ValValue(v,x) -> ValValue(v,cut x)
            | ModuleValue _ -> UnknownValue
            | TupleValue a -> TupleValue(Array.map cut a)
            | RecdValue (tcref,a) -> RecdValue(tcref,Array.map cut a)       
            | UnionCaseValue (a,b) -> UnionCaseValue (a,Array.map cut b)
            | UnknownValue | ConstValue _  | ConstExprValue _ -> ivalue
            | SizeValue(_,a) -> MakeSizedValueInfo (cut a) 
        let einfo = if v.MustInline  then einfo else {einfo with Info = cut einfo.Info } 
        let einfo = 
            if (not(v.MustInline ) && not (cenv.settings.KeepOptimizationValues())) ||
               (v.InlineInfo = NeverInline) ||
               // MarshalByRef methods may not be inlined
               (match v.ActualParent with 
                | Parent tcref -> 
                    // Check we can deref system_MarshalByRefObject_tcref. When compiling against the Silverlight mscorlib we can't
                    cenv.g.system_MarshalByRefObject_tcref.TryDeref.IsSome &&
                    // Check if this is a subtype of MarshalByRefObject
                    ExistsSameHeadTypeInHierarchy cenv.g cenv.amap v.Range (snd(generalize_tcref tcref)) cenv.g.system_MarshalByRefObject_typ 
                | ParentNone -> false) ||

               // These values are given a special going-over by the optimizer and 
               // ilxgen.ml, hence treat them as if no-inline 
               (let nvref = mk_local_vref v 
                cenv.g.vref_eq nvref cenv.g.seq_vref ||
                cenv.g.vref_eq nvref cenv.g.poly_eq_inner_vref ||
                cenv.g.vref_eq nvref cenv.g.generic_comparison_inner_vref ||
                cenv.g.vref_eq nvref cenv.g.generic_comparison_withc_inner_vref ||
                cenv.g.vref_eq nvref cenv.g.generic_equality_inner_vref ||
                cenv.g.vref_eq nvref cenv.g.generic_equality_withc_inner_vref ||
                cenv.g.vref_eq nvref cenv.g.generic_hash_inner_vref)
            then {einfo with Info=UnknownValue} 
            else einfo 
        if v.MustInline  && IsPartialExprVal einfo.Info then 
            errorR(InternalError("the mustinline value '"^v.MangledName^"' was not inferred to have a known value",v.Range));
        if verboseOptimizations then dprintf "val %s gets opt info %s\n" (showL(valL v)) (showL(exprValueInfoL einfo.Info));
        
        let env = bind_internal_local_vspec cenv v (MkValInfo einfo v) env 
        (TBind(v,repr',spBind), einfo), env
    with exn -> 
        errorRecovery exn v.Range; 
        raise ReportedError
          
and OptimizeBindings cenv isRec env xs = FlatList.mapfold (OptimizeBinding cenv isRec) env xs
    
and OptimizeModuleExpr cenv env x = 
    match x with   
    | TMTyped(mty,def,m) -> 
        // Optimize the module implementation
        let (def,info),(env,bindInfosColl) = OptimizeModuleDef cenv (env,[]) def  
        let bindInfosColl = List.concat bindInfosColl 
        
        // Compute the elements truly hidden by the module signature.
        // The hidden set here must contain NOT MORE THAN the set of values made inaccessible by 
        // the application of the signature. If it contains extra elements we'll accidentally eliminate
        // bindings.
         
        let (renaming, hidden) as rpi = mk_mdef_to_mtyp_remapping def mty 
        let def = 
            if not (cenv.settings.localOpt()) then def else 

            let fvs = free_in_mdef CollectLocals def 
            let dead = 
                bindInfosColl |> List.filter (fun (bind,binfo) -> 

                    // Check the expression has no side effect, e.g. is a lambda expression (a function definition)
                    not (ValueIsUsedOrHasEffect cenv m fvs.FreeLocals (bind,binfo)) &&

                    // Check the thing is hidden by the signature (if any)
                    hidden.mhiVals.Contains(bind.Var) && 

                    // Check the thing is not compiled as a static field
                    not (IsCompiledAsStaticValue cenv.g bind.Var))
            if verboseOptimizations then dead |> List.iter (fun (bind,_) -> dprintf "dead, hidden, buried, gone: %s\n" (showL (vspecAtBindL bind.Var)));
            let deadSet = Zset.addList (dead |> List.map (fun (bind,_) -> bind.Var)) (Zset.empty val_spec_order)

            // Eliminate dead private bindings from a module type by mutation. Note that the optimizer doesn't
            // actually copy the entire term - it copies the expression portions of the term and leaves the 
            // value_spec and entity_specs in place. However this means that the value_specs and entity specs 
            // need to be updated when a change is made that affects them, e.g. when a binding is eliminated. 
            // We'd have to do similar tricks if the type of variable is changed (as happens in TLR, which also
            // uses mutation), or if we eliminated a type constructor.
            //
            // It may be wise to move to a non-mutating implementation at some point here. Copying expressions is
            // probably more costly than copying specs anyway.
            let rec elim_mtyp (mtyp:ModuleOrNamespaceType) =                  
                let mty = 
                    new ModuleOrNamespaceType(kind=mtyp.ModuleOrNamespaceKind, 
                                  vals= (mtyp.AllValuesAndMembers |> NameMap.filterRange (Zset.mem_of deadSet >> not)),
                                  entities= mtyp.AllEntities)
                mtyp.ModuleAndNamespaceDefinitions |> List.iter (fun mspec -> elim_mspec  mspec)
                mty;
            and elim_mspec (mspec:ModuleOrNamespace) = 
                let mtyp = elim_mtyp mspec.ModuleOrNamespaceType 
                mspec.Data.entity_modul_contents <- notlazy mtyp

            let rec elim_mdef x =                  
                match x with 
                | TMDefRec(tycons,vbinds,mbinds,m) -> 
                    let vbinds = vbinds |> FlatList.filter (var_of_bind >> Zset.mem_of deadSet >> not) 
                    let mbinds = mbinds |> List.map elim_mbind
                    TMDefRec(tycons,vbinds,mbinds,m)
                | TMDefLet(bind,m)  -> 
                    if Zset.mem bind.Var deadSet then TMDefRec([],FlatList.empty,[],m) else x
                | TMDefDo _  -> x
                | TMDefs(defs) -> TMDefs(List.map elim_mdef defs) 
                | TMAbstract _ ->  x 
            and elim_mbind (TMBind(mspec, d)) =
                // Clean up the ModuleOrNamespaceType by mutation
                elim_mspec mspec;
                TMBind(mspec,elim_mdef d) 
            
            elim_mdef def 

        let info = AbstractAndRemapModulInfo "defs" cenv.g m rpi info

        TMTyped(mty,def,m),info 

and mk_var_bind (bind:Binding) info =
    let v = bind.Var
    (v.MangledName, (mk_local_vref v, info))

and OptimizeModuleDef cenv (env,bindInfosColl) x = 
    match x with 
    | TMDefRec(tycons,binds,mbinds,m) -> 
        let env = bind_internal_vspecs_to_unknown cenv (vars_of_Bindings binds) env
        let bindInfos,env = OptimizeBindings cenv true env binds
        let binds', binfos = FlatList.unzip bindInfos
        let mbindInfos,(env,bindInfosColl) = OptimizeModuleBindings cenv (env,bindInfosColl) mbinds
        let mbinds,minfos = List.unzip mbindInfos
        
          (* REVIEW: Eliminate let bindings on the way back up *)
        (TMDefRec(tycons,binds',mbinds,m),
         notlazy { ValInfos=NameMap.of_FlatList (FlatList.map2 (fun bind binfo -> mk_var_bind bind (MkValInfo binfo bind.Var)) binds binfos); 
                   ModuleOrNamespaceInfos = NameMap.of_list minfos}),
         (env,(FlatList.to_list bindInfos :: bindInfosColl))
    | TMAbstract(mexpr) -> 
        let mexpr,info = OptimizeModuleExpr cenv env mexpr
        let env = bind_module_vspecs cenv info env
        (TMAbstract(mexpr),info),(env,bindInfosColl)
    | TMDefLet(bind,m)  ->
        let ((bind',binfo) as bindInfo),env = OptimizeBinding cenv false env bind
          (* REVIEW: Eliminate unused let bindings from modules *)
        (TMDefLet(bind',m),
         notlazy { ValInfos=NameMap.of_list [mk_var_bind bind (MkValInfo binfo bind.Var)]; 
                   ModuleOrNamespaceInfos = NameMap.of_list []}),
        (env ,([bindInfo]::bindInfosColl))

    | TMDefDo(e,m)  ->
        let (e,einfo) = OptimizeExpr cenv env e
        (TMDefDo(e,m),EmptyModuleInfo),
        (env ,bindInfosColl)
    | TMDefs(defs) -> 
        let (defs,info),(env,bindInfosColl) = OptimizeModuleDefs cenv (env,bindInfosColl) defs 
        (TMDefs(defs), info), (env,bindInfosColl)

and OptimizeModuleBindings cenv (env,bindInfosColl) xs = List.mapfold (OptimizeModuleBinding cenv) (env,bindInfosColl) xs

and OptimizeModuleBinding cenv (env,bindInfosColl) (TMBind(mspec, def)) = 
    let id = mspec.Id
    let (def,info),(_,bindInfosColl) = OptimizeModuleDef cenv (env,bindInfosColl) def 
    let env = bind_module_vspecs cenv info env
    (TMBind(mspec,def),(id.idText, info)), 
    (env,bindInfosColl)

and OptimizeModuleDefs cenv (env,bindInfosColl) defs = 
    if verboseOptimizations then dprintf "OptimizeModuleDefs\n";
    let defs,(env,bindInfosColl) = List.mapfold (OptimizeModuleDef cenv) (env,bindInfosColl) defs
    let defs,minfos = List.unzip defs
    (defs,UnionModuleInfos minfos),(env,bindInfosColl)
   
and OptimizeImplFileInternal cenv env isIncrementalFragment (TImplFile(qname, pragmas, (TMTyped(mty,_,m) as mexpr))) =
    let env,mexpr',minfo  = 
        match mexpr with 
        // FSI: FSI compiles everything as if you're typing incrementally into one module 
        // This means the fragment is not truly a constrained module as later fragments will be typechecked 
        // against the internals of the module rather than the externals. Furthermore it would be wrong to apply 
        // optimizations that did lots of reorganizing stuff to the internals of a module should we ever implement that. 
        | TMTyped(mty,def,m) when isIncrementalFragment -> 
            let (def,minfo),(env,bindInfosColl) = OptimizeModuleDef cenv (env,[]) def 
            env, TMTyped(mty, def,m), minfo
        |  _ -> 
            let mexpr', minfo = OptimizeModuleExpr cenv env mexpr
            let env = bind_module_vspecs cenv minfo env
            env, mexpr', minfo

    let hidden = mk_assembly_boundary_mhi mty

    let minfo = AbstractLazyModulInfoByHiding true m hidden minfo
    env, TImplFile(qname,pragmas,mexpr'), minfo

//-------------------------------------------------------------------------
// Entry point
//------------------------------------------------------------------------- 

let OptimizeImplFile(settings,ccu,tcGlobals,importMap,optEnv,isIncrementalFragment,mimpls) =
    let cenv = mk_cenv settings ccu tcGlobals importMap
    OptimizeImplFileInternal cenv optEnv isIncrementalFragment mimpls 


//-------------------------------------------------------------------------
// Pickle to stable format for cross-module optimization data
//------------------------------------------------------------------------- 

open Pickle

let rec p_expr_info x st =
    match x with 
    | ConstValue (c,ty)   -> p_byte 0 st; p_tup2 p_const p_typ (c,ty) st 
    | UnknownValue   -> p_byte 1 st
    | ValValue (a,b) -> p_byte 2 st; p_tup2 (p_vref "optval") p_expr_info (a,b) st
    | ModuleValue a  -> p_byte 3 st; p_submodul_info a st
    | TupleValue a   -> p_byte 4 st; (p_array p_expr_info) a st
    | UnionCaseValue (a,b) -> p_byte 5 st; p_tup2 p_ucref (p_array p_expr_info) (a,b) st
    | CurriedLambdaValue (_,b,c,d,e) -> p_byte 6 st; p_tup4 p_int p_int p_expr p_typ (b,c,d,e) st
    | ConstExprValue (a,b) -> p_byte 7 st; p_tup2 p_int p_expr (a,b) st
    | RecdValue (tcref,a)  -> p_byte 10 st; p_tup2 (p_tcref "opt data") (p_array p_expr_info) (tcref,a) st
    | SizeValue (adepth,a) -> p_expr_info a st

and p_val_info (v:ValInfo) st = 
    p_tup2 p_expr_info p_bool (v.ValExprInfo, v.ValMakesNoCriticalTailcalls) st

and p_submodul_info x st = 
    p_tup2 (p_namemap (p_tup2 (p_vref "opttab") p_val_info)) (p_namemap p_lazy_submodul_info) (x.ValInfos, x.ModuleOrNamespaceInfos) st

and p_lazy_submodul_info x st = 
    p_lazy p_submodul_info x st

let rec u_expr_info st =
    let rec u_expr_info st =
        let tag = u_byte st
        match tag with
        | 0 -> u_tup2 u_const u_typ       st |> (fun (c,ty) -> ConstValue(c,ty))
        | 1 -> UnknownValue
        | 2 -> u_tup2 u_vref u_expr_info st |> (fun (a,b) -> ValValue (a,b))
        | 3 -> u_submodul_info          st |> (fun a -> ModuleValue a)
        | 4 -> u_array u_expr_info       st |> (fun a -> TupleValue a)
        | 5 -> u_tup2 u_ucref (u_array u_expr_info)  st |> (fun (a,b) -> UnionCaseValue (a,b))
        | 6 -> u_tup4 u_int u_int u_expr u_typ st |> (fun (b,c,d,e) -> CurriedLambdaValue (new_uniq(),b,c,d,e))
        | 7 -> u_tup2 u_int u_expr        st |> (fun (a,b) -> ConstExprValue (a,b))
        | 10 -> u_tup2 u_tcref (u_array u_expr_info)      st |> (fun (a,b) -> RecdValue (a,b))
        | _ -> failwith "u_expr_info"
    MakeSizedValueInfo (u_expr_info st) (* calc size of unpicked ExprValueInfo *)

and u_val_info st = 
    let a,b = u_tup2 u_expr_info u_bool st
    { ValExprInfo=a; ValMakesNoCriticalTailcalls = b } 

and u_submodul_info st = 
    let a,b = u_tup2 (u_namemap (u_tup2 u_vref u_val_info)) (u_namemap u_lazy_submodul_info) st
    { ValInfos=a; ModuleOrNamespaceInfos=b}

and u_lazy_submodul_info st = u_lazy u_submodul_info st

let p_lazy_modul_info info st = p_lazy_submodul_info info st
let u_lazy_modul_info st = u_lazy_submodul_info st
