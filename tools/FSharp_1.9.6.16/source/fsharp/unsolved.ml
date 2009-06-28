(*-------------------------------------------------------------------------
 * Apply default values to unresolved type variables throughout an expression
 *------------------------------------------------------------------------- *)

#light

module internal Microsoft.FSharp.Compiler.FindUnsolved

open Internal.Utilities
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
open Microsoft.FSharp.Compiler.Lib
open Microsoft.FSharp.Compiler.Layout
open Microsoft.FSharp.Compiler.AbstractIL.IL
open Microsoft.FSharp.Compiler.Typrelns
open Microsoft.FSharp.Compiler.Infos

type env = Nix

type cenv = { g: TcGlobals; amap: Import.ImportMap; denv: DisplayEnv; mutable unsolved: typars }
let mk_cenv  g amap denv =  { g =g ; amap=amap; denv=denv; unsolved = [] }

(*--------------------------------------------------------------------------
!* eliminate internal uninstantiated type variables
 *--------------------------------------------------------------------------*)

let acc_ty cenv env ty =
    (free_in_type CollectTyparsNoCaching ty).FreeTypars |> Zset.iter (fun tp -> 
            if (tp.Rigidity <> TyparRigid) then 
                cenv.unsolved <- tp :: cenv.unsolved) 

let acc_tinst cenv env tyargs =
  tyargs |> List.iter (acc_ty cenv env)

(*--------------------------------------------------------------------------
!* check exprs etc
 *--------------------------------------------------------------------------*)
  
let rec acc_expr   (cenv:cenv) (env:env) expr =     
    let expr = strip_expr expr 
    match expr with
    | TExpr_seq (e1,e2,_,_,_) -> 
        acc_expr cenv env e1; 
        acc_expr cenv env e2
    | TExpr_let (bind,body,_,_) ->  
        acc_bind cenv env bind ; 
        acc_expr cenv env body
    | TExpr_const (_,_,ty) -> 
        acc_ty cenv env ty 
    
    | TExpr_val (v,vFlags,m) -> ()
    | TExpr_quote(ast,_,m,ty) -> 
          acc_expr cenv env ast;
          acc_ty cenv env ty;
    | TExpr_obj (_,typ,basev,basecall,overrides,iimpls,m,_) -> 
          acc_expr cenv env basecall;
          acc_methods cenv env basev overrides ;
          acc_iimpls cenv env basev iimpls;
    | TExpr_op (c,tyargs,args,m) ->
          acc_op cenv env (c,tyargs,args,m) 
    | TExpr_app(f,fty,tyargs,argsl,m) ->
          acc_ty cenv env fty;
          acc_tinst cenv env tyargs;
          acc_expr cenv env f;
          acc_exprs cenv env argsl
    (* REVIEW: fold the next two cases together *)
    | TExpr_lambda(lambda_id,basevopt,argvs,body,m,rty,_) -> 
        let topValInfo = TopValInfo ([],[argvs |> List.map (fun _ -> TopValInfo.unnamedTopArg1)],TopValInfo.unnamedRetVal) in 
        let ty = mk_multi_lambda_ty m argvs rty in 
        acc_lambdas cenv env topValInfo expr ty
    | TExpr_tlambda(lambda_id,tps,body,m,rty,_)  -> 
        let topValInfo = TopValInfo (TopValInfo.InferTyparInfo tps,[],TopValInfo.unnamedRetVal) in
        acc_ty cenv env rty;
        let ty = try_mk_forall_ty tps rty in 
        acc_lambdas cenv env topValInfo expr ty
    | TExpr_tchoose(tps,e1,m)  -> 
        acc_expr cenv env e1 
    | TExpr_match(_,exprm,dtree,targets,m,ty,_) -> 
        acc_ty cenv env ty;
        acc_dtree cenv env dtree;
        acc_targets cenv env m ty targets;
    | TExpr_letrec (binds,e,m,_) ->  
        acc_binds cenv env binds;
        acc_expr cenv env e
    | TExpr_static_optimization (constraints,e2,e3,m) -> 
        acc_expr cenv env e2;
        acc_expr cenv env e3;
        constraints |> List.iter (fun (TTyconEqualsTycon(ty1,ty2)) -> 
            acc_ty cenv env ty1;
            acc_ty cenv env ty2)
    | TExpr_link eref -> failwith "Unexpected reclink"

and acc_methods cenv env basevopt l = List.iter (acc_method cenv env basevopt) l
and acc_method cenv env basevopt (TObjExprMethod(slotsig,tps,vs,e,m)) = 
    vs |> List.iterSquared (acc_val cenv env);
    acc_expr cenv env e

and acc_iimpls cenv env basevopt l = List.iter (acc_iimpl cenv env basevopt) l
and acc_iimpl cenv env basevopt (ty,overrides) = acc_methods cenv env basevopt overrides 

and acc_op cenv env (op,tyargs,args,m) =
    (* Special cases *)
    acc_tinst cenv env tyargs;
    acc_exprs cenv env args;
    match op with 
    (* Handle these as special cases since mutables are allowed inside their bodies *)
    | TOp_ilcall ((virt,protect,valu,newobj,superInit,prop,isDllImport,boxthis,mref),enclTypeArgs,methTypeArgs,tys) ->
        acc_tinst cenv env enclTypeArgs;
        acc_tinst cenv env methTypeArgs;
        acc_tinst cenv env tys
    | TOp_trait_call(TTrait(tys,nm,_,argtys,rty,sln)) -> 
        argtys |> acc_tinst cenv env ;
        rty |> Option.iter (acc_ty cenv env)
        tys |> List.iter (acc_ty cenv env)
        
    | TOp_asm (_,tys) ->
        acc_tinst cenv env tys
    | _ ->    ()

and acc_lambdas cenv env topValInfo e ety =
    match e with
    | TExpr_tchoose(tps,e1,m)  -> acc_lambdas cenv env topValInfo e1 ety      
    | TExpr_lambda (lambda_id,_,_,_,m,_,_)  
    | TExpr_tlambda(lambda_id,_,_,m,_,_) ->
        let tps,basevopt,vsl,body,bodyty = dest_top_lambda_upto cenv.g cenv.amap topValInfo (e, ety) in
        acc_ty cenv env bodyty;
        vsl |> List.iterSquared (acc_val cenv env);
        basevopt |> Option.iter (acc_val cenv env);
        acc_expr cenv env body;
    | _ -> 
        acc_expr cenv env e

and acc_exprs            cenv env exprs = exprs |> List.iter (acc_expr cenv env) 
and acc_FlatExprs        cenv env exprs = exprs |> FlatList.iter (acc_expr cenv env) 
and acc_targets cenv env m ty targets = Array.iter (acc_target cenv env m ty) targets

and acc_target cenv env m ty (TTarget(vs,e,_)) = acc_expr cenv env e;

and acc_dtree cenv env x =
    match x with 
    | TDSuccess (es,n) -> acc_FlatExprs cenv env es;
    | TDBind(bind,rest) -> acc_bind cenv env bind; acc_dtree cenv env rest 
    | TDSwitch (e,cases,dflt,m) -> acc_switch cenv env (e,cases,dflt,m)

and acc_switch cenv env (e,cases,dflt,m) =
    acc_expr cenv env e;
    List.iter (fun (TCase(discrim,e)) -> acc_discrim cenv env discrim; acc_dtree cenv env e) cases;
    Option.iter (acc_dtree cenv env) dflt

and acc_discrim cenv env d =
    match d with 
    | TTest_unionconstr(ucref,tinst) -> acc_tinst cenv env tinst 
    | TTest_array_length(_,ty) -> acc_ty cenv env ty
    | TTest_const _
    | TTest_isnull -> ()
    | TTest_isinst (srcty,tgty) -> acc_ty cenv env srcty; acc_ty cenv env tgty
    | TTest_query (exp, tys, vref, idx, apinfo) -> 
        acc_expr cenv env exp;
        acc_tinst cenv env tys

and acc_attrib cenv env (Attrib(_,k,args,props,m)) = 
    args |> List.iter (fun (AttribExpr(e1,_)) -> acc_expr cenv env e1);
    props |> List.iter (fun (AttribNamedArg(nm,ty,flg,AttribExpr(expr,_))) -> acc_expr cenv env expr)
  
and acc_attribs cenv env attribs = List.iter (acc_attrib cenv env) attribs

and acc_topValInfo cenv env (TopValInfo(_,args,ret)) =
    args |> List.iterSquared (acc_topArgInfo cenv env);
    ret |> acc_topArgInfo cenv env;

and acc_topArgInfo cenv env (TopArgInfo(attribs,_)) = 
    acc_attribs cenv env attribs

and acc_val cenv env v =
    v.Attribs |> acc_attribs cenv env;
    v.TopValInfo |> Option.iter (acc_topValInfo cenv env);
    v.Type |> acc_ty cenv env 

and acc_bind cenv env (TBind(v,e,_) as bind) =
    acc_val cenv env v;    
    let topValInfo  = match chosen_arity_of_bind bind with Some info -> info | _ -> TopValInfo.emptyValData in
    acc_lambdas cenv env topValInfo e v.Type;

and acc_binds cenv env xs = xs |> FlatList.iter (acc_bind cenv env) 

let modul_rights cpath = Infos.AccessibleFrom ([cpath],None) // review:

(*--------------------------------------------------------------------------
!* check tycons
 *--------------------------------------------------------------------------*)
  
let acc_tycon_rfield cenv env tycon (rfield:RecdField) = 
    acc_attribs cenv env rfield.PropertyAttribs;
    acc_attribs cenv env rfield.FieldAttribs

let acc_tycon cenv env (tycon:Tycon) =
    acc_attribs cenv env tycon.Attribs;
    tycon.AllFieldsArray |> Array.iter (acc_tycon_rfield cenv env tycon);
    if tycon.IsUnionTycon then                             (* This covers finite unions. *)
      tycon.UnionCasesAsList |> List.iter (fun uc ->
          acc_attribs cenv env uc.Attribs;
          uc.RecdFields |> List.iter (acc_tycon_rfield cenv env tycon))
  

let acc_tycons cenv env tycons = List.iter (acc_tycon cenv env) tycons

(*--------------------------------------------------------------------------
!* check modules
 *--------------------------------------------------------------------------*)

let rec acc_mexpr cenv env x = 
    match x with  
    | TMTyped(mty,def,m) -> acc_mdef cenv env def
    
and acc_mdefs cenv env x = List.iter (acc_mdef cenv env) x

and acc_mdef cenv env x = 
    match x with 
    | TMDefRec(tycons,binds,mbinds,m) -> 
        acc_tycons cenv env tycons; 
        acc_binds cenv env binds;
        acc_mbinds cenv env mbinds 
    | TMDefLet(bind,m)  -> acc_bind cenv env bind 
    | TMDefDo(e,m)  -> acc_expr cenv env e
    | TMAbstract(def)  -> acc_mexpr cenv env def
    | TMDefs(defs) -> acc_mdefs cenv env defs 
and acc_mbinds cenv env xs = List.iter (acc_mbind cenv env) xs
and acc_mbind cenv env (TMBind(mspec, rhs)) = acc_tycon cenv env mspec; acc_mdef cenv env rhs 

let unsolved_typars_of_mdef g amap denv (mdef,extraAttribs) =
   let cenv = mk_cenv  g amap denv in 
   acc_mdef cenv Nix mdef;
   acc_attribs cenv Nix extraAttribs;
   List.rev cenv.unsolved


