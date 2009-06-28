// (c) Microsoft Corporation. All rights reserved
#light

module (* internal *) Microsoft.FSharp.Compiler.Patcompile

open System.Collections.Generic
open Internal.Utilities
open Internal.Utilities.Pervasives
open Microsoft.FSharp.Compiler 
open Microsoft.FSharp.Compiler.AbstractIL 
open Microsoft.FSharp.Compiler.AbstractIL.Internal 
open Microsoft.FSharp.Compiler.AbstractIL.Internal.Library
open Microsoft.FSharp.Compiler.AbstractIL.Diagnostics 

open Microsoft.FSharp.Compiler.Range
open Microsoft.FSharp.Compiler.Ast
open Microsoft.FSharp.Compiler.ErrorLogger
open Microsoft.FSharp.Compiler.Tast
open Microsoft.FSharp.Compiler.Tastops
open Microsoft.FSharp.Compiler.PrettyNaming
open Microsoft.FSharp.Compiler.Typrelns
open Microsoft.FSharp.Compiler.Env
open Microsoft.FSharp.Compiler.Lib

exception MatchIncomplete of bool * (string * bool) option * range
exception RuleNeverMatched of range

type ActionOnFailure = 
  | Incomplete 
  | Throw 
  | Rethrow 
  | FailFilter

[<StructuralEquality(false); StructuralComparison(false)>]
type pat =
  | TPat_const of Constant * range
  | TPat_wild of range  (* note = TPat_disjs([],m), but we haven't yet removed that duplication *)
  | TPat_as of  pat * pbind * range (* note: can be replaced by TPat_var, i.e. equals TPat_conjs([TPat_var; pat]) *)
  | TPat_disjs of  pat list * range
  | TPat_conjs of  pat list * range
  | TPat_query of (expr * typ list * ValRef option * int * ActivePatternInfo) * pat * range
  | TPat_unioncase of UnionCaseRef * tinst * pat list * range
  | TPat_exnconstr of TyconRef * pat list * range
  | TPat_tuple of  pat list * typ list * range
  | TPat_array of  pat list * typ * range
  | TPat_recd of TyconRef * tinst * (tinst * pat) list * range
  | TPat_range of char * char * range
  | TPat_null of range
  | TPat_isinst of typ * typ * pbind option * range
and pbind = PBind of Val * TypeScheme

and tclause =  TClause of pat * expr option * DecisionTreeTarget * range

let debug = false

(*---------------------------------------------------------------------------
 * Nasty stuff to permit obscure polymorphic bindings.
 *
 * [bind_subexpr] actually produces the binding
 * e.g. let v2 = \Gamma ['a,'b]. ([] : 'a ,[] : 'b)
 *      let (x,y) = p.  
 * When v = x, gtvs = 'a,'b.  We must bind:
 *     x --> \Gamma A. fst (v2[A,<dummy>]) 
 *     y --> \Gamma A. snd (v2[<dummy>,A]).  
 * 
 * [GetSubExprOfInput] is just used to get a concrete value from a type
 * function in the middle of the "test" part of pattern matching.
 * For example, e.g.  let [x; y] = [ (\x.x); (\x.x) ] 
 * Here the constructor test needs a real list, even though the
 * r.h.s. is actually a polymorphic type function.  To do the
 * test, we apply the r.h.s. to a dummy type - it doesn't matter
 * which (unless the r.h.s. actually looks at it's type argument...)
 *------------------------------------------------------------------------- *)

type SubExprOfInput = SubExpr of (TyparInst -> expr -> expr) * (expr * Val)

let bind_subexpr g amap gtps (PBind(v,tyscheme)) m (SubExpr(accessf,(ve2,v2))) =
    let e' = 
        if isNil gtps then accessf [] ve2 else 
        let freeze_var gtp = 
             if is_being_generalized gtp tyscheme then mk_typar_ty gtp 
             else Typrelns.choose_typar_solution g amap gtp
             
        let tyargs = List.map freeze_var gtps
        let tinst = mk_typar_inst gtps tyargs
        accessf tinst (mk_appl g ((ve2,v2.Type),[tyargs],[],v2.Range))
    v,mk_poly_bind_rhs m tyscheme e'

let GetSubExprOfInput g (gtps,tyargs,tinst) (SubExpr(accessf,(ve2,v2))) =
    if isNil gtps then accessf [] ve2 else
    accessf tinst (mk_appl g ((ve2,v2.Type),[tyargs],[],v2.Range))

(*---------------------------------------------------------------------------
 * path, frontier
 *------------------------------------------------------------------------- *)

// A path reaches into a pattern.
// The ints record which choices taken, e.g. tuple/record fields.
// [it may be enough that subpatterns have unique paths].
type path = 
    | PathQuery of path * uniq
    | PathConj of path * int
    | PathTuple of path * tinst * int
    | PathRecd of path * TyconRef * tinst * tinst  * int
    | PathUnionConstr of path * UnionCaseRef * tinst * int
    | PathArray of path * typ * int * int
    | PathExnConstr of path * TyconRef * int
    | PathEmpty of typ 

let rec path_eq p1 p2 = 
    match p1,p2 with
    | PathQuery(p1,n1), PathQuery(p2,n2) -> (n1 = n2) && path_eq p1 p2
    | PathConj(p1,n1), PathConj(p2,n2) -> (n1 = n2) && path_eq p1 p2
    | PathTuple(p1,_,n1), PathTuple(p2,_,n2) -> (n1 = n2) && path_eq p1 p2
    | PathRecd(p1,_,_,_,n1), PathRecd(p2,_,_,_,n2) -> (n1 = n2) && path_eq p1 p2
    | PathUnionConstr(p1,_,_,n1), PathUnionConstr(p2,_,_,n2) -> (n1 = n2) && path_eq p1 p2
    | PathArray(p1,_,_,n1), PathArray(p2,_,_,n2) -> (n1 = n2) && path_eq p1 p2
    | PathExnConstr(p1,_,n1), PathExnConstr(p2,_,n2) -> (n1 = n2) && path_eq p1 p2
    | PathEmpty(_), PathEmpty(_) -> true
    | _ -> false


(*---------------------------------------------------------------------------
 * Counter example generation 
 *------------------------------------------------------------------------- *)

type refutedSet = 
    /// A value RefutedInvestigation(path,discrim) indicates that the value at the given path is known 
    /// to NOT be matched by the given discriminator
    | RefutedInvestigation of path * DecisionTreeDiscriminator list
    /// A value RefutedWhenClause indicates that a 'when' clause failed
    | RefutedWhenClause

let notNullText = "some-non-null-value"
let otherSubtypeText = "some-other-subtype"

exception CannotRefute
let RefuteDiscrimSet g m path discrims = 
    let mk_unknown ty = snd(mk_compgen_local m "_" ty)
    let rec go path tm = 
        match path with 
        | PathQuery _ -> raise CannotRefute
        | PathConj (p,j) -> 
             go p tm
        | PathTuple (p,tys,j) -> 
             go p (fun _ -> mk_tupled g m (mk_one_known tm j tys) tys)
        | PathRecd (p,tcref,tinst,finst,j) -> 
             let flds = tcref |> typs_of_instance_rfields_of_tcref (mk_tcref_inst tcref tinst) |> mk_one_known tm j
             go p (fun _ -> TExpr_op(TOp_recd(RecdExpr, tcref),tinst, flds,m))

        | PathUnionConstr (p,ucref,tinst,j) -> 
             let flds = ucref |> typs_of_ucref_rfields (mk_tcref_inst ucref.TyconRef tinst)|> mk_one_known tm j
             go p (fun _ -> TExpr_op(TOp_ucase(ucref),tinst, flds,m))

        | PathArray (p,ty,len,n) -> 
             go p (fun _ -> TExpr_op(TOp_array,[ty], mk_one_known tm n (List.replicate len ty) ,m))

        | PathExnConstr (p,ecref,n) -> 
             let flds = ecref |> typs_of_ecref_rfields |> mk_one_known tm n
             go p (fun _ -> TExpr_op(TOp_exnconstr(ecref),[], flds,m))

        | PathEmpty(ty) -> tm ty
        
    and mk_one_known tm n tys = List.mapi (fun i ty -> if i = n then tm ty else mk_unknown ty) tys 
    and mk_unknowns tys = List.map mk_unknown tys

    let tm ty = 
        match discrims with 
        | [TTest_isnull] -> 
            snd(mk_compgen_local m notNullText ty)
        | [TTest_isinst (srcty,tgty)] -> 
            snd(mk_compgen_local m otherSubtypeText ty)
        | (TTest_const c :: rest) -> 
            let consts = Set.of_list (c :: List.choose (function TTest_const(c) -> Some c | _ -> None) rest)
            let c' = 
                Seq.tryfind (fun c -> not (consts.Contains(c)))
                     (match c with 
                      | TConst_bool(b) -> [ true; false ] |> List.to_seq |> Seq.map (fun v -> TConst_bool(v))
                      | TConst_sbyte(i) ->  Seq.append (seq { 0y .. System.SByte.MaxValue }) (seq { System.SByte.MinValue .. 0y })|> Seq.map (fun v -> TConst_sbyte(v))
                      | TConst_int16(i) -> Seq.append (seq { 0s .. System.Int16.MaxValue }) (seq { System.Int16.MinValue .. 0s }) |> Seq.map (fun v -> TConst_int16(v))
                      | TConst_int32(i) ->  Seq.append (seq { 0 .. System.Int32.MaxValue }) (seq { System.Int32.MinValue .. 0 })|> Seq.map (fun v -> TConst_int32(v))
                      | TConst_int64(i) ->  Seq.append (seq { 0L .. System.Int64.MaxValue }) (seq { System.Int64.MinValue .. 0L })|> Seq.map (fun v -> TConst_int64(v))
                      | TConst_nativeint(i) ->  Seq.append (seq { 0L .. System.Int64.MaxValue }) (seq { System.Int64.MinValue .. 0L })|> Seq.map (fun v -> TConst_nativeint(v))
                      | TConst_byte(i) -> seq { 0uy .. System.Byte.MaxValue } |> Seq.map (fun v -> TConst_byte(v))
                      | TConst_uint16(i) -> seq { 0us .. System.UInt16.MaxValue } |> Seq.map (fun v -> TConst_uint16(v))
                      | TConst_uint32(i) -> seq { 0u .. System.UInt32.MaxValue } |> Seq.map (fun v -> TConst_uint32(v))
                      | TConst_uint64(i) -> seq { 0UL .. System.UInt64.MaxValue } |> Seq.map (fun v -> TConst_uint64(v))
                      | TConst_unativeint(i) -> seq { 0UL .. System.UInt64.MaxValue } |> Seq.map (fun v -> TConst_unativeint(v))
                      | TConst_float(i) -> seq { 0 .. System.Int32.MaxValue } |> Seq.map (fun v -> TConst_float(float v))
                      | TConst_float32(i) -> seq { 0 .. System.Int32.MaxValue } |> Seq.map (fun v -> TConst_float32(float32 v))
                      | TConst_char(i) -> seq { 32us .. System.UInt16.MaxValue } |> Seq.map (fun v -> TConst_char(char v))
                      | TConst_string(s) -> seq { 1 .. System.Int32.MaxValue } |> Seq.map (fun v -> TConst_string(new System.String('a',v)))
                      | TConst_decimal(s) -> seq { 1 .. System.Int32.MaxValue } |> Seq.map (fun v -> TConst_decimal(decimal v))
                      | _ -> 
                          raise CannotRefute) 

            (* REVIEW: we could return a better enumeration literal field here if a field matches one of the enumeration cases *)

            match c' with 
            | None -> raise CannotRefute
            | Some c -> TExpr_const(c,m,ty)
            
        | (TTest_unionconstr (ucref1,tinst) :: rest) -> 
             let ucrefs = ucref1 :: List.choose (function TTest_unionconstr(ucref,_) -> Some ucref | _ -> None) rest
             let tcref = ucref1.TyconRef
             (* Choose the first ucref based on ordering of names *)
             let others = 
                 tcref 
                 |> ucrefs_of_tcref 
                 |> List.filter (fun ucref -> not (List.exists (g.ucref_eq ucref) ucrefs)) 
                 |> List.sortBy (fun ucref -> ucref.CaseName)
             match others with 
             | [] -> raise CannotRefute
             | ucref2 :: _ -> 
               let flds = ucref2 |> typs_of_ucref_rfields (mk_tcref_inst tcref tinst) |> mk_unknowns
               TExpr_op(TOp_ucase(ucref2),tinst, flds,m)
               
        | [TTest_array_length (n,ty)] -> 
             TExpr_op(TOp_array,[ty], mk_unknowns (List.replicate (n+1) ty) ,m)
             
        | _ -> 
            raise CannotRefute
    go path tm

let rec CombineRefutations g r1 r2 =
   match r1,r2 with
   | TExpr_val(vref,_,_), other | other, TExpr_val(vref,_,_) when vref.MangledName = "_" -> other 
   | TExpr_val(vref,_,_), other | other, TExpr_val(vref,_,_) when vref.MangledName = notNullText -> other 
   | TExpr_val(vref,_,_), other | other, TExpr_val(vref,_,_) when vref.MangledName = otherSubtypeText -> other 

   | TExpr_op((TOp_exnconstr(ecref1) as op1), tinst1,flds1,m1), TExpr_op(TOp_exnconstr(ecref2), _,flds2,_) when tcref_eq g ecref1 ecref2 -> 
        TExpr_op(op1, tinst1,List.map2 (CombineRefutations g) flds1 flds2,m1)

   | TExpr_op((TOp_ucase(ucref1) as op1), tinst1,flds1,m1), 
     TExpr_op(TOp_ucase(ucref2), _,flds2,_)  -> 
       if g.ucref_eq ucref1 ucref2 then 
           TExpr_op(op1, tinst1,List.map2 (CombineRefutations g) flds1 flds2,m1)
       (* Choose the greater of the two ucrefs based on name ordering *)
       elif ucref1.CaseName < ucref2.CaseName then 
           r2
       else 
           r1
        
   | TExpr_op(op1, tinst1,flds1,m1), TExpr_op(_, _,flds2,_) -> 
        TExpr_op(op1, tinst1,List.map2 (CombineRefutations g) flds1 flds2,m1)
        
   | TExpr_const(c1, m1, ty1), TExpr_const(c2,_,_) -> 
       let c12 = 

           (* Make sure longer strings are greater, not the case in the default ordinal comparison *)
           (* This is needed because the individual counter examples make longer strings *)
           let MaxStrings s1 s2 = 
               let c = compare (String.length s1) (String.length s2)
               if c < 0 then s2 
               elif c > 0 then s1 
               elif s1 < s2 then s2 
               else s1
               
           match c1,c2 with 
           | TConst_string(s1), TConst_string(s2) -> TConst_string(MaxStrings s1 s2)
           | TConst_decimal(s1), TConst_decimal(s2) -> TConst_decimal(max s1 s2)
           | _ -> max c1 c2 
           
       (* REVIEW: we couldd return a better enumeration literal field here if a field matches one of the enumeration cases *)
       TExpr_const(c12, m1, ty1)

   | _ -> r1 

let ShowCounterExample g denv m refuted = 
   try
      let refutations = refuted |> List.collect (function RefutedWhenClause -> [] | (RefutedInvestigation(path,discrim)) -> [RefuteDiscrimSet g m path discrim])
      let counterExample = 
          match refutations with 
          | [] -> raise CannotRefute
          | h :: t -> 
              if verbose then dprintf "h = %s\n" (Layout.showL (ExprL h));
              List.fold (CombineRefutations g) h t
      let text = Layout.showL (NicePrint.dataExprL denv counterExample)
      let failingWhenClause = refuted |> List.exists (function RefutedWhenClause -> true | _ -> false)
      Some(text,failingWhenClause)
      
    with 
        | CannotRefute ->    
          None 
        | e -> 
          warning(InternalError(Printf.sprintf "<failure during counter example generation: %s>" (e.ToString()),m));
          None
       
(*---------------------------------------------------------------------------
 * Basic problem specification
 *------------------------------------------------------------------------- *)
    
type RuleNumber = int

type Active = Active of path * SubExprOfInput * pat

type Actives = Active list

type Frontier = Frontier of RuleNumber * Actives * ValMap<expr>

type InvestigationPoint = Investigation of RuleNumber * DecisionTreeDiscriminator * path

(* Note: actives must be a SortedDictionary *)
(* REVIEW: improve these data structures, though surprisingly these functions don't tend to show up *)
(* on profiling runs *)
let rec IsMemOfActives p1 actives = 
    match actives with 
    | [] -> false 
    | (Active(p2,_,_)) :: rest -> path_eq p1 p2 or IsMemOfActives p1 rest

let rec LookupActive x l = 
    match l with 
    | [] -> raise (KeyNotFoundException())
    | (Active(h,r1,r2)::t) -> if path_eq x h then (r1,r2) else LookupActive x t

let rec RemoveActive x l = 
    match l with 
    | [] -> []
    | ((Active(h,_,_) as p) ::t) -> if path_eq x h then t else p:: RemoveActive x t

(*---------------------------------------------------------------------------
 * Utilities
 *------------------------------------------------------------------------- *)

let RangeOfPat t = 
    match t with 
    | TPat_null m | TPat_isinst (_,_,_,m) | TPat_const (_,m) | TPat_unioncase (_ ,_,_,m) 
    | TPat_exnconstr (_,_,m) | TPat_query (_,_,m) | TPat_range(_,_,m) 
    | TPat_recd(_,_,_,m) | TPat_tuple(_,_,m) | TPat_array(_,_,m) | TPat_disjs(_,m) | TPat_conjs(_,m) | TPat_as(_,_,m) | TPat_wild(m) -> m
  
// tpinst is required because the pattern is specified w.r.t. generalized type variables. 
let GetDiscrimOfPattern g tpinst t = 
    match t with 
    | TPat_null m -> 
        Some(TTest_isnull)
    | TPat_isinst (srcty,tgty,_,m) -> 
        Some(TTest_isinst (InstType tpinst srcty,InstType tpinst tgty))
    | TPat_exnconstr(tcref,_,m) -> 
        Some(TTest_isinst (g.exn_ty,mk_tyapp_ty tcref []))
    | TPat_const (c,m) -> 
        Some(TTest_const c)
    | TPat_unioncase (c,tyargs',_,m) -> 
        Some(TTest_unionconstr (c,inst_types tpinst tyargs'))
    | TPat_array (args,ty,m) -> 
        Some(TTest_array_length (List.length args,ty))
    | TPat_query ((pexp,restys,apatVrefOpt,idx,apinfo),_,m) -> 
        Some(TTest_query (pexp, inst_types tpinst restys, apatVrefOpt,idx,apinfo))
    | _ -> None

let ConstOfDiscrim discrim =
    match discrim with 
    | TTest_const x -> x
    | _ -> failwith "not a const case"

let ConstOfCase c = ConstOfDiscrim(discrim_of_case c)

let GetDiscrimOfCase (TCase(discrim,_)) = discrim

/// Compute pattern identity
let DiscrimsEq g d1 d2 =
  match d1,d2 with 
  | TTest_unionconstr (c1,_),    TTest_unionconstr(c2,_) -> g.ucref_eq c1 c2
  | TTest_array_length (n1,_),   TTest_array_length(n2,_) -> (n1=n2)
  | TTest_const c1,              TTest_const c2 -> (c1=c2)
  | TTest_isnull ,               TTest_isnull -> true
  | TTest_isinst (srcty1,tgty1), TTest_isinst (srcty2,tgty2) -> type_equiv g srcty1 srcty2 && type_equiv g tgty1 tgty2
  | TTest_query (_,_,vrefOpt1,n1,apinfo1),        TTest_query (_,_,vrefOpt2,n2,apinfo2) -> 
      match vrefOpt1, vrefOpt2 with 
      | Some vref1, Some vref2 -> g.vref_eq vref1 vref2 && n1 = n2 
      | _ -> false (* for equality purposes these are considered unequal! This is because adhoc computed patterns have no identity. *)

  | _ -> false
    
/// Redundancy of 'isinst' patterns 
let IsDiscrimSubsumedBy g amap m d1 d2 =
  (DiscrimsEq g d1 d2) 
  ||
  (match d1,d2 with 
   | TTest_isinst (srcty1,tgty1), TTest_isinst (srcty2,tgty2) -> 
      type_definitely_subsumes_type_no_coercion 0 g amap m tgty2 tgty1
   | _ -> false)
    
/// Choose a set of investigations that can be performed simultaneously 
let rec ChooseSimultaneousEdgeSet prevOpt f l =
    match l with 
    | [] -> [],[]
    | h::t -> 
        match f prevOpt h with 
        | Some x,_ ->         
             let l,r = ChooseSimultaneousEdgeSet (Some x) f t
             x :: l, r
        | None,cont -> 
             let l,r = ChooseSimultaneousEdgeSet prevOpt f t
             l, h :: r

/// Can we represent a integer discrimination as a 'switch'
let CanCompactConstantClass c = 
    match c with 
    | TConst_sbyte _ | TConst_int16 _ | TConst_int32 _ 
    | TConst_byte _ | TConst_uint16 _ | TConst_uint32 _ 
    | TConst_char _ -> true
    | _ -> false
                         
/// Can two discriminators in a 'column' be decided simultaneously?
let DiscrimsHaveSameSimultaneousClass g d1 d2 =
  match d1,d2 with 
  | TTest_const _,              TTest_const _ 
  | TTest_isnull ,               TTest_isnull 
  | TTest_array_length _,   TTest_array_length _
  | TTest_unionconstr _,    TTest_unionconstr _  -> true

  | TTest_isinst _, TTest_isinst _ -> false
  | TTest_query (_,_,apatVrefOpt1,n1,apinfo1),        TTest_query (_,_,apatVrefOpt2,n2,apinfo2) -> 
      match apatVrefOpt1, apatVrefOpt2 with 
      | Some vref1, Some vref2 -> g.vref_eq vref1 vref2 
      | _ -> false (* for equality purposes these are considered different classes of discriminators! This is because adhoc computed patterns have no identity! *)

  | _ -> false


/// Decide the next pattern to investigate
let ChooseInvestigationPointLeftToRight frontiers =
    match frontiers with 
    | Frontier (i,actives,_) ::t -> 
        let rec choose l = 
            match l with 
            | [] -> failwith "ChooseInvestigationPointLeftToRight: no non-immediate patterns in first rule"
            | (Active(path,_,(TPat_null _ | TPat_isinst _ | TPat_exnconstr _ | TPat_unioncase _ | TPat_array _ | TPat_const _ | TPat_query _ | TPat_range _)) as active)
                ::t -> active
            | _ :: t -> choose t
        choose actives
    | [] -> failwith "ChooseInvestigationPointLeftToRight: no frontiers!"


/// Build a dtree, equivalent to: TDSwitch("expr",edges,default,m) 
///
/// Once we've chosen a particular active to investigate, we compile the
/// set of edges affected by this investigation into a switch.  
///
///   - For TTest_query(...,None,...) there is only one edge
///
///   - For TTest_isinst there are multiple edges, which we can't deal with
///     one switch, so we make an iterated if-then-else to cover the cases. We
///     should probably adjust the code to only choose one edge in this case.
///
///   - Compact integer switches become a single switch.  Non-compact integer
///     switches, string switches and floating point switches are treated in the
///     same way as TTest_isinst.
let rec BuildSwitch resVarOpt g expr edges dflt m =
    if verbose then dprintf "--> BuildSwitch@%a, #edges = %A, dflt.IsSome = %A\n" output_range m (List.length edges) (Option.is_some dflt); 
    match edges,dflt with 
    | [], None      -> failwith "internal error: no edges and no default"
    | [], Some dflt -> dflt      (* NOTE: first time around, edges<>[] *)
    (* Optimize the case where the match always succeeds *)
    | [TCase(_,tree)], None -> tree

    (* 'isinst' tests where we have stored the result of the 'isinst' in a variable *)
    (* In this case the 'expr' already holds the result of the 'isinst' test. *)
    
    | (TCase(TTest_isinst _,success) as edge):: edges, dflt  when isSome(resVarOpt) -> 
        TDSwitch(expr,[TCase(TTest_isnull,(BuildSwitch None g expr edges dflt) m)],Some success,m)    
        
    | (TCase((TTest_isnull | TTest_isinst _),_) as edge):: edges, dflt  -> 
        TDSwitch(expr,[edge],Some (BuildSwitch resVarOpt g expr edges dflt m),m)    
        (*begin match dflt with 
        | None      -> error(InternalError("exception/null/isinst matches need default cases!",m))
        | Some dflt -> TDSwitch(GetSubExprOfInput  g subexpr,[edge],Some (BuildSwitch resVarOpt g subexpr edges (Some dflt) m),m)
        end*)

    (* All these should also always have default cases *)
    | TCase(TTest_const (TConst_decimal _ | TConst_string _ | TConst_float32 _ |  TConst_float _ | TConst_sbyte _ | TConst_byte _| TConst_int16 _ | TConst_uint16 _ | TConst_int32 _ | TConst_uint32 _ | TConst_int64 _ | TConst_uint64 _ | TConst_nativeint _ | TConst_unativeint _ | TConst_char _ ),_) :: _, None -> 
        error(InternalError("inexhaustive match - need a default cases!",m))

    (* Split string, float, uint64, int64, unativeint, nativeint matches into serial equality tests *)
    | TCase((TTest_array_length _ | TTest_const (TConst_float32 _ | TConst_float _ | TConst_string _ | TConst_decimal _ | TConst_int64 _ | TConst_uint64 _ | TConst_nativeint _ | TConst_unativeint _)),_) :: _, Some dflt -> 
        List.foldBack 
            (fun (TCase(discrim,tree)) sofar -> 
                let testexpr = expr
                let testexpr = 
                    match discrim with 
                    | TTest_array_length(n,ty)       -> 
                        let v,vexp,bind = mk_compgen_local_and_invisible_bind g "testExpr" m testexpr
                        mk_let_bind m bind (mk_lazy_and g m (mk_nonnull_test g m vexp) (mk_ceq g m (mk_ldlen g m vexp) (mk_int g m n)))
                    | TTest_const (TConst_string _ as c)  -> 
                        mk_call_generic_equality_outer g m g.string_ty testexpr (TExpr_const(c,m,g.string_ty))
                    | TTest_const (TConst_decimal _ as c)  -> 
                        mk_call_generic_equality_outer g m g.decimal_ty testexpr (TExpr_const(c,m,g.decimal_ty))
                    | TTest_const ((TConst_float _ | TConst_float32 _ | TConst_int64 _ | TConst_uint64 _ | TConst_nativeint _ | TConst_unativeint _) as c)   -> 
                        mk_ceq g m testexpr (TExpr_const(c,m,type_of_expr g testexpr))
                    | _ -> error(InternalError("strange switch",m))
                mk_bool_switch m testexpr tree sofar)
          edges
          dflt

    (* Split integer and char matches into compact fragments which will themselves become switch *)
    (* statements. *)
    | TCase(TTest_const c,_) :: _, Some dflt when CanCompactConstantClass c -> 
        let edge_compare c1 c2 = 
            match ConstOfCase c1,ConstOfCase c2 with 
            | (TConst_sbyte i1),(TConst_sbyte i2) -> compare i1 i2
            | (TConst_int16 i1),(TConst_int16 i2) -> compare i1 i2
            | (TConst_int32 i1),(TConst_int32 i2) -> compare i1 i2
            | (TConst_byte i1),(TConst_byte i2) -> compare i1 i2
            | (TConst_uint16 i1),(TConst_uint16 i2) -> compare i1 i2
            | (TConst_uint32 i1),(TConst_uint32 i2) -> compare i1 i2
            | (TConst_char c1),(TConst_char c2) -> compare c1 c2
            | _ -> failwith "illtyped term during pattern compilation" 
        let edges' = List.sortWith edge_compare edges
        let rec compactify curr edges = 
            if debug then  dprintf "--> compactify@%a\n" output_range m; 
            match curr,edges with 
            | None,[] -> []
            | Some last,[] -> [List.rev last]
            | None,h::t -> compactify (Some [h]) t
            | Some (prev::moreprev),h::t -> 
                match ConstOfCase prev,ConstOfCase h with 
                | TConst_sbyte iprev,TConst_sbyte inext when int32(iprev) + 1 = int32 inext -> 
                    compactify (Some (h::prev::moreprev)) t
                | TConst_int16 iprev,TConst_int16 inext when int32(iprev) + 1 = int32 inext -> 
                    compactify (Some (h::prev::moreprev)) t
                | TConst_int32 iprev,TConst_int32 inext when iprev+1 = inext -> 
                    compactify (Some (h::prev::moreprev)) t
                | TConst_byte iprev,TConst_byte inext when int32(iprev) + 1 = int32 inext -> 
                    compactify (Some (h::prev::moreprev)) t
                | TConst_uint16 iprev,TConst_uint16 inext when int32(iprev)+1 = int32 inext -> 
                    compactify (Some (h::prev::moreprev)) t
                | TConst_uint32 iprev,TConst_uint32 inext when int32(iprev)+1 = int32 inext -> 
                    compactify (Some (h::prev::moreprev)) t
                | TConst_char cprev,TConst_char cnext when (int32 cprev + 1 = int32 cnext) -> 
                    compactify (Some (h::prev::moreprev)) t
                |       _ ->  (List.rev (prev::moreprev)) :: compactify None edges

            | _ -> failwith "internal error: compactify"
        let edge_groups = compactify None edges'
        List.foldBack 
            (fun edge_group sofar ->  TDSwitch(expr,edge_group,Some sofar,m))
            edge_groups
            dflt

    // For a total pattern match, run the active pattern, bind the result and 
    // recursively build a switch in the choice type 
    | (TCase(TTest_query _,_)::rest), dflt -> 
       error(InternalError("TTest_query should have been eliminated",m));

    // For a complete match, optimize one test to be the default 
    | (TCase(test,tree)::rest), None -> TDSwitch (expr,rest,Some tree,m)

    // Otherwise let codegen make the choices 
    | _ -> TDSwitch (expr,edges,dflt,m)

let rec patL pat = 
    if debug then  dprintf "--> patL\n"; 
    match pat with
    | TPat_query (_,pat,_) -> Layout.(--) (Layout.wordL "query") (patL pat)
    | TPat_wild _ -> Layout.wordL "wild"
    | TPat_as _ -> Layout.wordL "var"
    | TPat_tuple (pats, _, _) 
    | TPat_array (pats, _, _) -> Layout.bracketL (Layout.tupleL (List.map patL pats))
    | _ -> Layout.wordL "?" 
  
let rec pathL p = Layout.wordL "<path>"
     
let activeL (Active (path, subexpr, pat)) =
    Layout.(--) (Layout.wordL "Active") (Layout.tupleL [pathL path; patL pat]) 
     
let frontierL (Frontier (i,actives,_)) =
    Layout.(--) (Layout.wordL "Frontier") (Layout.tupleL [intL i; Layout.listL activeL actives]) 

let mk_frontiers investigations i = 
    List.map (fun (actives,valMap) -> Frontier(i,actives,valMap)) investigations

let GetRuleIndex (Frontier (i,active,valMap)) = i

/// Is a pattern a partial pattern?
let rec IsPatternPartial p = 
    match p with 
    | TPat_query ((_,restys,apatVrefOpt,idx,apinfo),p,m) -> not (total_of_apinfo apinfo) or IsPatternPartial p
    | TPat_const _ -> false
    | TPat_wild _ -> false
    | TPat_as (p,_,_) -> IsPatternPartial p
    | TPat_disjs (ps,_) | TPat_conjs(ps,_) 
    | TPat_tuple (ps,_,_) | TPat_exnconstr(_,ps,_) 
    | TPat_array (ps,_,_) | TPat_unioncase (_,_,ps,_)-> List.exists IsPatternPartial ps
    | TPat_recd (_,_,psl,_) -> List.exists (snd >> IsPatternPartial) psl
    | TPat_range _ -> false
    | TPat_null _ -> false
    | TPat_isinst _ -> false

let rec ErasePartialPatterns inpp = 
    match inpp with 
    | TPat_query ((expr,restys,apatVrefOpt,idx,apinfo),p,m) -> 
         if (total_of_apinfo apinfo) then TPat_query ((expr,restys,apatVrefOpt,idx,apinfo),ErasePartialPatterns p,m)
         else TPat_disjs ([],m) (* always fail *)
    | TPat_as (p,x,m) -> TPat_as (ErasePartialPatterns p,x,m)
    | TPat_disjs (ps,m) -> TPat_disjs(erase_partials ps, m)
    | TPat_conjs(ps,m) -> TPat_conjs(erase_partials ps, m)
    | TPat_tuple (ps,x,m) -> TPat_tuple(erase_partials ps, x, m)
    | TPat_exnconstr(x,ps,m) -> TPat_exnconstr(x,erase_partials ps,m) 
    | TPat_array (ps,x,m) -> TPat_array (erase_partials ps,x,m)
    | TPat_unioncase (x,y,ps,m) -> TPat_unioncase (x,y,erase_partials ps,m)
    | TPat_recd (x,y,ps,m) -> TPat_recd (x,y,List.map (map2'2 ErasePartialPatterns) ps,m)
    | TPat_const _ 
    | TPat_wild _ 
    | TPat_range _ 
    | TPat_null _ 
    | TPat_isinst _ -> inpp
and erase_partials inps = List.map ErasePartialPatterns inps




(*---------------------------------------------------------------------------
 * The algorithm
 *------------------------------------------------------------------------- *)

type EdgeDiscrim = EdgeDiscrim of int * DecisionTreeDiscriminator * range
let get_discrim (EdgeDiscrim(_,discrim,_)) = discrim

let when_of_clause (TClause(p,whenOpt,_,_)) = whenOpt
let pat_of_clause (TClause(p,whenOpt,_,_)) = p
let range_of_clause (TClause(p,whenOpt,_,m)) = m
let vs_of_clause (TClause(p,whenOpt,TTarget(vs,_,_),m)) = vs

let CompilePatternBasic 
        g denv amap exprm matchm 
        warnOnUnused 
        warnOnIncomplete 
        actionOnFailure 
        (topv,topgtvs) 
        (clausesL: tclause list) 
        ty =
    // Add the targets to a match builder 
    // Note the input expression has already been evaluated and saved into a variable.
    // Hence no need for a new sequence point.
    let mbuilder = new MatchBuilder(NoSequencePointAtInvisibleBinding,exprm)
    List.iteri (fun i (TClause(_,_,tg,_)) -> mbuilder.AddTarget(tg) |> ignore) clausesL;
    
    (* Add the incomplete or rethrow match clause on demand, printing a *)
    (* warning if necessary (only if it is ever exercised) *)
    let incompleteMatchClauseOnce = ref None
    let getIncompleteMatchClause (refuted) = 
        // This is lazy because emit a 
        // warning when the lazy thunk gets evaluated 
        match !incompleteMatchClauseOnce with 
        | None -> 
                (* Emit the incomplete match warning *)               
                if (actionOnFailure = Incomplete) && warnOnIncomplete then 
                    warning (MatchIncomplete (false,ShowCounterExample g denv exprm refuted, exprm));
                let throwExpr =
                    match actionOnFailure with
                      | FailFilter  -> 
                          // Return 0 from the .NET exception filter
                          mk_int g matchm 0

                      | Rethrow     -> 
                          // Rethrow unmatched try-catch exn. No sequence point at the target since its not
                          // real code.
                          mk_rethrow matchm ty 
                      
                      | Throw       -> 
                          // We throw instead of rethrow on unmatched try-catch in a computation expression. But why?
                          // Because this isn't a real .NET exception filter/handler but just a function we're passing
                          // to a computation expression builder to simulate one.
                          mk_throw   matchm ty (expr_for_val matchm topv) 
                          
                      | Incomplete  -> 
                          mk_throw   matchm ty 
                              (mk_exnconstr(mk_MFCore_tcref g.fslibCcu "MatchFailureException", 
                                            // REVIEW: get rid of this use of Bytes.string_as_unicode_bytes 
                                            [ mk_string g matchm (file_of_range matchm); 
                                              mk_int g matchm (matchm |> start_of_range |> line_of_pos); 
                                              mk_int g matchm (matchm |> start_of_range |> col_of_pos)],matchm))

                // We don't emit a sequence point at any of the above cases because they don't correspond to 
                // user code. 
                //
                // Note we don't emit sequence points at either the succeeding or failing
                // targets of filters since if the exception is filtered successfully then we 
                // will run the handler and hit the sequence point there.
                // That sequence point will have the pattern variables bound, which is exactly what we want.
                let tg = TTarget(FlatList.empty,throwExpr,SuppressSequencePointAtTarget  )
                mbuilder.AddTarget tg |> ignore;
                let clause = TClause(TPat_wild matchm,None,tg,matchm)
                incompleteMatchClauseOnce := Some(clause);
                clause
                
        | Some c -> c

    (* Helpers to get the variables bound at a target. We conceptually add a dummy clause that will always succeed with a "throw" *)
    let clausesA = Array.of_list clausesL
    let nclauses = Array.length clausesA
    let lastMatchClauseWarnngGiven = ref false
    let GetClause i refuted = 
        if i < nclauses then 
            clausesA.[i]  
        elif i = nclauses then getIncompleteMatchClause(refuted)
        else failwith "GetClause"
    let GetValsBoundByClause i refuted = vs_of_clause (GetClause i refuted)
    let GetWhenGuardOfClause i refuted = when_of_clause (GetClause i refuted)
    
    // Different uses of parameterized active patterns have different identities as far as paths 
    // are concerned. Here we generate unique numbers that are completely different to any stamp
    // by usig negative numbers.
    let genUniquePathId() = - (new_uniq())

    // Build versions of these functions which apply a dummy instantiation to the overall type arguments 
    let GetSubExprOfInput,GetDiscrimOfPattern = 
        let tyargs = List.map (fun _ -> g.unit_ty) topgtvs
        let unit_tpinst = mk_typar_inst topgtvs tyargs
        GetSubExprOfInput g (topgtvs,tyargs,unit_tpinst),
        GetDiscrimOfPattern g unit_tpinst


     (* The main recursive loop of the pattern match compiler *)
    let rec InvestigateFrontiers refuted frontiers = 
        if debug then dprintf "frontiers = %s\n" (String.concat ";" (List.map (GetRuleIndex >> string) frontiers));
        match frontiers with
        | [] -> failwith "CompilePattern:compile - empty clauses: at least the final clause should always succeed"
        | (Frontier (i,active,valMap)) :: rest ->

            (* Check to see if we've got a succeeding clause.  There may still be a 'when' condition for the clause *)
            match active with
            | [] -> CompileSuccessPointAndGuard i refuted valMap rest 

            | _ -> 
                if debug then dprintf "Investigating based on rule %d, #active = %d\n" i (List.length active);
                (* Otherwise choose a point (i.e. a path) to investigate. *)
                let (Active(path,subexpr,pat))  = ChooseInvestigationPointLeftToRight frontiers
                match pat with
                // All these constructs should have been eliminated in BindProjectionPattern 
                | TPat_as _   | TPat_tuple _  | TPat_wild _      | TPat_disjs _  | TPat_conjs _  | TPat_recd _ -> failwith "Unexpected pattern"

                // Leaving the ones where we have real work to do 
                | _ -> 

                    if debug then dprintf "ChooseSimultaneousEdgeSet\n";
                    let simulSetOfEdgeDiscrims,fallthroughPathFrontiers = ChooseSimultaneousEdges frontiers path

                    let resPreBindOpt,bindOpt =     ChoosePreBinder simulSetOfEdgeDiscrims subexpr    
                            
                    // For each case, recursively compile the residue decision trees that result if that case successfully matches 
                    let simulSetOfCases, _ = CompileSimultaneousSet frontiers path refuted subexpr simulSetOfEdgeDiscrims (resPreBindOpt) 
                          
                    assert (nonNil(simulSetOfCases));

                    if debug then 
                        dprintf "#fallthroughPathFrontiers = %d, #simulSetOfEdgeDiscrims = %d\n"  (List.length fallthroughPathFrontiers) (List.length simulSetOfEdgeDiscrims);
                        dprintf "Making cases for each discriminator...\n";
                        dprintf "#edges = %d\n" (List.length simulSetOfCases);
                        dprintf "Checking for completeness of edge set from earlier investigation of rule %d, #active = %d\n" i (List.length active);

                    // Work out what the default/fall-through tree looks like, is any 
                    // Check if match is complete, if so optimize the default case away. 
                
                    let defaultTreeOpt  : DecisionTree option = CompileFallThroughTree fallthroughPathFrontiers path refuted  simulSetOfCases

                    // OK, build the whole tree and whack on the binding if any 
                    let finalDecisionTree = 
                        let inpExprToSwitch = (match resPreBindOpt with Some vexp -> vexp | None -> GetSubExprOfInput subexpr)
                        let tree = BuildSwitch resPreBindOpt g inpExprToSwitch simulSetOfCases defaultTreeOpt matchm
                        match bindOpt with 
                        | None -> tree
                        | Some bind -> TDBind (bind,tree)
                        
                    finalDecisionTree

    and CompileSuccessPointAndGuard i refuted valMap rest =

        if debug then dprintf "generating success node for rule %d\n" i;
        let vs2 = GetValsBoundByClause i refuted
        let es2 = 
            vs2 |> FlatList.map (fun v -> 
                match (vspec_map_tryfind v valMap) with 
                | None -> error(Error("missing variable "^v.DisplayName,v.Range)) 
                | Some res -> res)
        let rhs' = TDSuccess(es2, i)
        match GetWhenGuardOfClause i refuted with 
        | Some whenExpr -> 
            if debug then dprintf "generating success node for rule %d, with 'when' clause\n" i;
            // We duplicate both the bindings and the guard expression to ensure uniqueness of bound variables: the same vars are boun in the guard and the targets 
            // REVIEW: this is also duplicating the guard when "or" patterns are used, leading to code explosion for large guards with many "or" patterns 
            let m = (range_of_expr whenExpr)
            
            // SEQUENCE POINTS: REVIEW: Build a sequence point at 'when' 
            let whenExpr = copy_expr g CloneAll (mk_lets_from_Bindings m (mk_invisible_FlatBindings vs2 es2) whenExpr)
            mk_bool_switch m whenExpr rhs' (InvestigateFrontiers (RefutedWhenClause::refuted) rest)
        | None -> rhs' 

    /// Select the set of discriminators which we can handle in one test, or as a series of 
    /// iterated tests, e.g. in the case of TPat_isinst.  Ensure we only take at most one class of TPat_query(_) at a time. 
    /// Record the rule numbers so we know which rule the TPat_query cam from, so that when we project through 
    /// the frontier we only project the right rule. 
    and ChooseSimultaneousEdges frontiers path =
        if debug then dprintf "ChooseSimultaneousEdgeSet\n";
        frontiers |> ChooseSimultaneousEdgeSet None (fun prevOpt (Frontier (i',active',_)) -> 
              if IsMemOfActives path active' then 
                  let p = LookupActive path active' |> snd
                  match GetDiscrimOfPattern p with
                  | Some discrim -> 
                      if (match prevOpt with None -> true | Some (EdgeDiscrim(_,discrimPrev,_)) -> DiscrimsHaveSameSimultaneousClass g discrim discrimPrev) then (
                          if debug then dprintf "taking rule %d\n" i';
                          Some (EdgeDiscrim(i',discrim,RangeOfPat p)),true
                      ) else 
                          None,false
                                                        
                  | None -> 
                      None,true
              else 
                  None,true)

    and ChoosePreBinder simulSetOfEdgeDiscrims subexpr =
         match simulSetOfEdgeDiscrims with 
          // Very simple 'isinst' tests: put the result of 'isinst' in a local variable 
          //
          // That is, transform 
          //     'if istype e then ...unbox e .... ' 
          // into
          //     'let v = isinst e in .... if nonnull v then ...v .... ' 
          //
          // This is really an optimization that could be done more effectively in opt.ml
          // if we flowed a bit of information through 

          
         | EdgeDiscrim(i',(TTest_isinst (srcty,tgty)),m) :: rest 
                    (* check we can use a simple 'isinst' instruction *)
                    when can_use_istype_fast g tgty && isNil topgtvs ->

             let v,vexp = mk_compgen_local m "typeTestResult" tgty
             if topv.IsMemberOrModuleBinding then 
                 AdjustValToTopVal v topv.ActualParent TopValInfo.emptyValData;
             let argexp = GetSubExprOfInput subexpr
             let appexp = mk_isinst tgty argexp matchm
             Some(vexp),Some(mk_invisible_bind v appexp)

         // Active pattern matches: create a variable to hold the results of executing the active pattern. 
         | (EdgeDiscrim(i',(TTest_query(pexp,restys,resPreBindOpt,_,apinfo)),m) :: rest) ->
             if debug then dprintf "Building result var for active pattern...\n";
             
             if nonNil topgtvs then error(InternalError("Unexpected generalized type variables when compiling an active pattern",m));
             let rty = mk_apinfo_result_typ g m apinfo restys
             let v,vexp = mk_compgen_local m "activePatternResult" rty
             if topv.IsMemberOrModuleBinding then 
                 AdjustValToTopVal v topv.ActualParent TopValInfo.emptyValData;
             let argexp = GetSubExprOfInput subexpr
             let appexp = mk_appl g ((pexp,type_of_expr g pexp), [], [argexp],m)
             
             Some(vexp),Some(mk_invisible_bind v appexp)
          | _ -> None,None
                            

    and CompileSimultaneousSet frontiers path refuted subexpr simulSetOfEdgeDiscrims (resPreBindOpt:expr option) =

        ([],simulSetOfEdgeDiscrims) ||> List.collectFold (fun taken (EdgeDiscrim(i',discrim,m)) -> 
             // Check to see if we've already collected the edge for this case, in which case skip it. 
             if List.exists (IsDiscrimSubsumedBy g amap m discrim) taken  then 
                 // Skip this edge: it is refuted 
                 ([],taken) 
             else 
                 // Make a resVar to hold the results of the successful "proof" that a union value is
                 // a successful union case. That is, transform 
                 //     'match v with 
                 //        | A _ -> ... 
                 //        | B _ -> ...' 
                 // into
                 //     'match v with 
                 //        | A _ -> let vA = (v ~~> A)  in .... 
                 //        | B _ -> let vB = (v ~~> B)  in .... ' 
                 //
                 // Only do this for union cases that actually have some fields and with more than one case
                 let resPostBindOpt,ucaseBindOpt =
                     match discrim with 
                     | TTest_unionconstr (ucref, tinst) when isNil topgtvs && not topv.IsMemberOrModuleBinding && ucref.UnionCase.RecdFields.Length >= 1 && ucref.Tycon.UnionCasesArray.Length > 1 ->
                       let v,vexp = mk_compgen_local m "unionCase" (mk_proven_ucase_typ ucref tinst)
                       let argexp = GetSubExprOfInput subexpr
                       let appexp = mk_ucase_proof(argexp, ucref,tinst,m)
                       Some(vexp),Some(mk_invisible_bind v appexp)
                     | _ -> 
                       None,None
                 
                 // Convert active pattern edges to tests on results data 
                 let discrim' = 
                     match discrim with 
                     | TTest_query(pexp,restys,apatVrefOpt,idx,apinfo) -> 
                         let aparity = List.length (names_of_apinfo apinfo)
                         let total = total_of_apinfo apinfo
                         if not total && aparity > 1 then 
                             error(Error("partial active patterns may only generate one result",m));
                         
                         if not total then TTest_unionconstr(mk_some_ucref g,restys)
                         elif aparity <= 1 then TTest_const(TConst_unit) 
                         else TTest_unionconstr(mk_choices_ucref g m aparity idx,restys) 
                     | _ -> discrim
                     
                 // Project a successful edge through the frontiers. 
                 let investigation = Investigation(i',discrim,path)

                 let frontiers = frontiers |> List.collect (GenerateNewFrontiersAfterSucccessfulInvestigation resPreBindOpt resPostBindOpt investigation) 
                 let tree = InvestigateFrontiers refuted frontiers
                 // Bind the resVar for the union case, if we have one
                 let tree = 
                     match ucaseBindOpt with 
                     | None -> tree
                     | Some bind -> TDBind (bind,tree)
                 // Return the edge 
                 let edge = TCase(discrim',tree)
                 [edge], (discrim :: taken) )

    and CompileFallThroughTree fallthroughPathFrontiers path refuted  simulSetOfCases =

        let simulSetOfDiscrims = List.map GetDiscrimOfCase simulSetOfCases

        let IsRefuted (Frontier (i',active',_)) = 
            IsMemOfActives path active' &&
            let p = LookupActive path active' |> snd
            match GetDiscrimOfPattern p with 
            | Some(discrim) -> List.exists (IsDiscrimSubsumedBy g amap exprm discrim) simulSetOfDiscrims 
            | None -> false

        match simulSetOfDiscrims with 
        | TTest_const (TConst_bool b) :: _ when simulSetOfCases.Length = 2 ->  None
        | TTest_const (TConst_unit) :: _  ->  None
        | TTest_unionconstr (ucref,_) :: _ when  simulSetOfCases.Length = ucref.TyconRef.UnionCasesArray.Length -> None                      
        | TTest_query _ :: _ -> error(InternalError("TTest_query should have been eliminated",matchm))
        | _ -> 
            let fallthroughPathFrontiers = List.filter (IsRefuted >> not) fallthroughPathFrontiers
            
            (* Add to the refuted set *)
            let refuted = (RefutedInvestigation(path,simulSetOfDiscrims)) :: refuted
        
            if debug then dprintf "Edge set was incomplete. Compiling remaining cases\n";
            match fallthroughPathFrontiers with
            | [] -> 
                None
            | _ -> 
                Some(InvestigateFrontiers refuted fallthroughPathFrontiers)
          
    // Build a new frontire that represents the result of a successful investigation 
    // at rule point (i',discrim,path) 
    and GenerateNewFrontiersAfterSucccessfulInvestigation resPreBindOpt resPostBindOpt (Investigation(i',discrim,path)) (Frontier (i, active,valMap) as frontier) =
        if debug then dprintf "projecting success of investigation encompassing rule %d through rule %d \n" i' i;

        if (IsMemOfActives path active) then
            let (SubExpr(accessf,ve) as e),pat = LookupActive path active
            if debug then dprintf "active...\n";

            let mk_sub_frontiers path accessf' active' argpats pathBuilder = 
                let mk_sub_active j p = 
                    let newSubExpr = SubExpr(accessf' j, ve)
                    let newPath = pathBuilder path j
                    Active(newPath, newSubExpr, p)
                let newActives = List.mapi mk_sub_active argpats
                let investigations = BindProjectionPatterns newActives (active', valMap)
                mk_frontiers investigations i

            let active' = RemoveActive path active
            match pat with 
            | TPat_wild _ | TPat_as _ | TPat_tuple _ | TPat_disjs _ | TPat_conjs _ | TPat_recd _ -> failwith "Unexpected projection pattern"
            | TPat_query ((_,restys,apatVrefOpt,idx,apinfo),p,m) -> 
            
                if total_of_apinfo apinfo then
            
                    if (isNone apatVrefOpt && i = i') 
                       || (DiscrimsEq g discrim (the (GetDiscrimOfPattern pat))) then
                        let aparity = List.length (names_of_apinfo apinfo)
                        let accessf' j tpinst e' = 
                            if aparity <= 1 then the resPreBindOpt 
                            else
                                let ucref = mk_choices_ucref g m aparity idx
                                mk_ucase_field_get_unproven(the resPreBindOpt,ucref,inst_types tpinst restys,j,exprm)
                        mk_sub_frontiers path accessf' active' [p] (fun path j -> PathQuery(path,int64 j))

                    elif isNone apatVrefOpt then

                        // Successful active patterns  don't refute other patterns
                        [frontier] 
                    else
                        []
                else 
                    if i = i' then
                            let accessf' j tpinst _ =  
                                mk_ucase_field_get_unproven(the resPreBindOpt, mk_some_ucref g, inst_types tpinst restys, 0, exprm)
                            mk_sub_frontiers path accessf' active' [p] (fun path j -> PathQuery(path,int64 j))
                    else 
                        // Successful active patterns  don't refute other patterns
                        [frontier]  

            | TPat_unioncase (ucref1, tyargs, argpats,_) -> 
                match discrim with 
                | TTest_unionconstr (ucref2, tinst) when g.ucref_eq ucref1 ucref2 ->
                    let accessf' j tpinst e' = 
                        match resPostBindOpt with 
                        | Some e -> mk_ucase_field_get_proven(e,ucref1,tinst,j,exprm)
                        | None -> mk_ucase_field_get_unproven(accessf tpinst e',ucref1,inst_types tpinst tyargs,j,exprm)
                        
                    mk_sub_frontiers path accessf' active' argpats (fun path j -> PathUnionConstr(path,ucref1,tyargs,j))
                | TTest_unionconstr _ ->
                    // Successful union case tests DO refute all other union case tests (no overlapping union cases)
                    []
                | _ -> 
                    // Successful union case tests don't refute any other patterns
                    [frontier]

            | TPat_array (argpats,ty,_) -> 
                match discrim with
                | TTest_array_length (n,_) when List.length argpats = n ->
                    let accessf' j tpinst e' = mk_call_array_get g exprm ty (accessf tpinst e') (mk_int g exprm j)
                    mk_sub_frontiers path accessf' active' argpats (fun path j -> PathArray(path,ty,List.length argpats,j))
                | TTest_array_length _ -> 
                    []
                | _ -> 
                    [frontier]

            | TPat_exnconstr (ecref, argpats,_) -> 
                match discrim with 
                | TTest_isinst (srcTy,tgtTy) when type_equiv g (mk_tyapp_ty ecref []) tgtTy ->
                    let accessf' j tpinst e' = mk_exnconstr_field_get(accessf tpinst e',ecref,j,exprm)
                    mk_sub_frontiers path accessf' active' argpats (fun path j -> PathExnConstr(path,ecref,j))
                | _ -> 
                    // Successful type tests against other types don't refute anything
                    [frontier]

            | TPat_isinst (srcty,tgtTy1,pbindOpt,_) -> 
                match discrim with 
                | TTest_isinst (srcTy,tgtTy2) when type_equiv g tgtTy1 tgtTy2  ->
                    match pbindOpt with 
                    | Some pbind -> 
                        let accessf' tpinst e' = 
                            // Fetch the result from the place where we saved it, if possible
                            match resPreBindOpt with 
                            | Some e -> e 
                            | _ -> 
                                // Otherwise call the helper
                               mk_call_unbox_fast g exprm (InstType tpinst tgtTy1) (accessf tpinst e')

                        let (v,e') =  bind_subexpr g amap topgtvs pbind exprm (SubExpr(accessf',ve))
                        [Frontier (i, active', vspec_map_add v e' valMap)]
                    | None -> 
                        [Frontier (i, active', valMap)]
                    
                | _ ->
                    // Successful type tests against other types don't refute anything
                    [frontier]

            | TPat_null _ -> 
                match discrim with 
                | TTest_isnull -> 
                    [Frontier (i, active',valMap)]
                | _ ->
                    // Successful null tests don't refute any other patterns 
                    [frontier]

            | TPat_const (c1,_) -> 
                match discrim with 
                | TTest_const c2 when (c1=c2) -> 
                    [Frontier (i, active',valMap)]
                | TTest_const _ -> 
                    // All constants refute all other constants (no overlapping between constants!)
                    []
                | _ ->
                    [frontier]

            | _ -> failwith "pattern compilation: GenerateNewFrontiersAfterSucccessfulInvestigation"
        else [frontier] 
        
    and BindProjectionPattern (Active(path,subExpr,p) as inp) ((acc_active,acc_vmap) as s) = 
        let (SubExpr(accessf,ve)) = subExpr 
        let mk_sub_active pathBuilder accessf'  j p'  = 
            Active(pathBuilder path j,SubExpr(accessf' j,ve),p')
            
        match p with 
        | TPat_wild _ -> 
            BindProjectionPatterns [] s 
        | TPat_as(p',pbind,m) -> 
            let (v,e') =  bind_subexpr g amap topgtvs pbind m subExpr
            BindProjectionPattern (Active(path,subExpr,p')) (acc_active,vspec_map_add v e' acc_vmap)
        | TPat_tuple(ps,tyargs,m) ->
            let accessf' j tpinst e' = mk_tuple_field_get(accessf tpinst e',inst_types tpinst tyargs,j,exprm)
            let pathBuilder path j = PathTuple(path,tyargs,j)
            let newActives = List.mapi (mk_sub_active pathBuilder accessf') ps
            BindProjectionPatterns newActives s 
        | TPat_recd(tcref,tinst,ps,m) -> 
            let newActives = 
                (ps,instance_rfrefs_of_tcref tcref) ||> List.mapi2 (fun j (finst,p) fref -> 
                    let accessf' fref j tpinst e' = mk_recd_field_get g (accessf tpinst e',fref,inst_types tpinst tinst,finst,exprm)
                    let pathBuilder path j = PathRecd(path,tcref,tinst,finst,j)
                    mk_sub_active pathBuilder (accessf' fref) j p) 
            BindProjectionPatterns newActives s 
        | TPat_disjs(ps,m) -> 
            List.collect (fun p -> BindProjectionPattern (Active(path,subExpr,p)) s)  ps
        | TPat_conjs(ps,m) -> 
            let newActives = List.mapi (mk_sub_active (fun path j -> PathConj(path,j)) (fun j -> accessf)) ps
            BindProjectionPatterns newActives s 
        
        | TPat_range (c1,c2,m) ->
            let res = ref []
            for i = int c1 to int c2 do
                res :=  BindProjectionPattern (Active(path,subExpr,TPat_const(TConst_char(char i),m))) s @ !res
            done;
            !res
        (* Assign an identifier to each TPat_query based on our knowledge of the 'identity' of the active pattern, if any *)
        | TPat_query ((_,_,apatVrefOpt,_,_),_,_) -> 
            let uniqId = match apatVrefOpt with None -> genUniquePathId() | Some vref -> vref.Stamp
            let inp = Active(PathQuery(path,uniqId),subExpr,p) 
            [(inp::acc_active, acc_vmap)] 
        | _ -> 
            [(inp::acc_active, acc_vmap)] 
    and BindProjectionPatterns ps s =
        List.foldBack (fun p sofar -> List.collect (BindProjectionPattern p) sofar) ps [s] 

    (* The setup routine of the match compiler *)
    let dtree = 
      InvestigateFrontiers
        []
        (List.concat 
           (List.mapi 
              (fun i (TClause(p,opt_when,rhs,_)) -> 
                let initialSubExpr = SubExpr((fun tpinst x -> x),(expr_for_val topv.Range topv,topv))
                let investigations = BindProjectionPattern (Active(PathEmpty(ty),initialSubExpr,p)) ([],vspec_map_empty())
                mk_frontiers investigations i)
              clausesL)
          @ 
          mk_frontiers [([],vspec_map_empty())] nclauses)

    let targets = mbuilder.CloseTargets()

    (* Report unused targets *)
    let used = acc_targets_of_dtree dtree []
    let _ = if warnOnUnused then List.iteri (fun i (TClause(_,_,_,patm)) ->  if not (List.mem i used) then warning (RuleNeverMatched patm)) clausesL
    dtree,targets
  
let isPartialOrWhenClause c = IsPatternPartial (pat_of_clause c) or (isSome (when_of_clause c))


let rec CompilePattern  g denv amap exprm matchm warnOnUnused actionOnFailure (topv,topgtvs) (clausesL: tclause list) ty =
  match clausesL with 
  | _ when List.exists isPartialOrWhenClause clausesL ->
        // Partial clauses cause major code explosion if treated naively 
        // Hence treat any pattern matches with any partial clauses clause-by-clause 
        
        // First make sure we generate at least some of the obvious incomplete match warnings. 
        let warnOnUnused = false in (* we can't turn this on since we're pretending all partial's fail in order to control the complexity of this. *)
        let warnOnIncomplete = true
        let clausesPretendAllPartialFail = List.collect (fun (TClause(p,whenOpt,tg,m)) -> [TClause(ErasePartialPatterns p,whenOpt,tg,m)]) clausesL
        let _ = CompilePatternBasic g denv amap exprm matchm warnOnUnused warnOnIncomplete actionOnFailure (topv,topgtvs) clausesPretendAllPartialFail ty
        let warnOnIncomplete = false
        
        let rec atMostOnePartialAtATime clauses = 
            if debug then dprintf "atMostOnePartialAtATime: #clauses = %A\n" clauses;
            match List.takeUntil isPartialOrWhenClause clauses with 
            | l,[]       -> CompilePatternBasic g denv amap exprm matchm warnOnUnused warnOnIncomplete actionOnFailure (topv,topgtvs) l ty
            | l,(h :: t) -> doGroupWithAtMostOnePartial (l @ [h]) t
        and doGroupWithAtMostOnePartial group rest = 
            if debug then dprintf "doGroupWithAtMostOnePartial: #group = %A\n" group;
            let dtree,targets = atMostOnePartialAtATime rest
            let expr = mk_and_optimize_match NoSequencePointAtInvisibleBinding exprm matchm ty dtree targets
            CompilePatternBasic 
                 g denv amap exprm matchm warnOnUnused warnOnIncomplete actionOnFailure (topv,topgtvs) 
                 (group @ [TClause(TPat_wild matchm,None,TTarget(FlatList.empty,expr,SuppressSequencePointAtTarget),matchm)]) ty
        

        atMostOnePartialAtATime clausesL
      
  | _ -> 
      CompilePatternBasic g denv amap exprm matchm warnOnUnused true actionOnFailure (topv,topgtvs) (clausesL: tclause list) ty
