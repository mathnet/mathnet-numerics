// (c) Microsoft Corporation 2005-2009. 
/// Defines an extension of the IL algebra
module (* internal *) Microsoft.FSharp.Compiler.AbstractIL.Extensions.ILX.Types

open Internal.Utilities
open Microsoft.FSharp.Compiler.AbstractIL 
open Microsoft.FSharp.Compiler.AbstractIL.Internal 
module Il = Microsoft.FSharp.Compiler.AbstractIL.IL 
module Illib = Microsoft.FSharp.Compiler.AbstractIL.Internal.Library 

open Illib
open Il

(* --------------------------------------------------------------------
 * Define an extension of the IL instruction algebra
 * -------------------------------------------------------------------- *)

type IlxUnionAlternative = 
    { altName: string;
      altFields: ILFieldDef array;
      altCustomAttrs: ILAttributes }

and IlxUnionRef = IlxUnionRef of ILTypeRef * IlxUnionAlternative array * bool
and IlxUnionSpec = IlxUnionSpec of IlxUnionRef * ILGenericArgs

and IlxClosureLambdas = 
  | Lambdas_forall of ILGenericParameterDef * IlxClosureLambdas
  | Lambdas_lambda of ILParameter * IlxClosureLambdas
  | Lambdas_return of ILType
and IlxClosureRef = IlxClosureRef of ILTypeRef * IlxClosureLambdas * IlxClosureFreeVar list 
and IlxClosureSpec = IlxClosureSpec of IlxClosureRef * ILGenericArgs

and IlxClosureFreeVar = 
    { fvName: string ; fvCompilerGenerated:bool; fvType: ILType }
let mk_freevar (name,compgen,ty) = 
    { fvName=name;
      fvCompilerGenerated=compgen;
      fvType=ty; }
let typ_of_freevar p = p.fvType
let name_of_freevar p = p.fvName

type IlxClosureApps = 
  | Apps_tyapp of ILType * IlxClosureApps 
  | Apps_app of ILType * IlxClosureApps 
  | Apps_done of ILType

type IlxInstr = 
  // Discriminated unions
  | EI_lddata of IlxUnionSpec * int * int
  | EI_isdata of IlxUnionSpec * int
  | EI_brisdata of IlxUnionSpec * int * ILCodeLabel * ILCodeLabel
  | EI_castdata of bool * IlxUnionSpec * int
  | EI_stdata of IlxUnionSpec * int * int
  | EI_datacase of (bool * IlxUnionSpec * (int * ILCodeLabel) list * ILCodeLabel) (* last label is fallthrough, bool is whether to leave value on the stack for each case *)
  | EI_lddatatag of IlxUnionSpec
  | EI_newdata of IlxUnionSpec * int
  
  // Closures
  | EI_newclo of IlxClosureSpec
  | EI_castclo of IlxClosureSpec
  | EI_isclo of IlxClosureSpec
  | EI_callclo of Tailcall * IlxClosureSpec * IlxClosureApps
  | EI_stclofld  of (IlxClosureSpec * int)  (* nb. leave these brackets *)
  | EI_stenv  of int
  | EI_ldenv  of int
  | EI_ldenva  of int
  | EI_callfunc of Tailcall * IlxClosureApps
  | EI_ldftn_then_call of ILMethodSpec * (Tailcall * ILMethodSpec * varargs)  (* special: for internal use only *)
  | EI_ld_instance_ftn_then_newobj of ILMethodSpec * ILCallingSignature * (ILMethodSpec * varargs)  (* special: for internal use only *)

let destinations_of_IlxInstr i =
  match i with 
  |  (EI_brisdata (_,_,l1,l2)) ->  [l1; l2]
  |  (EI_callfunc (Tailcall,_)) |  (EI_callclo (Tailcall,_,_)) ->   []
  |  (EI_datacase (_,_,ls,l)) -> l:: (List.foldBack (fun (_,l) acc -> ListSet.insert l acc) ls [])
  | _ -> []

let fallthrough_of_IlxInstr i = 
  match i with 
  |  (EI_brisdata (_,_,_,l)) 
  |  (EI_datacase (_,_,_,l)) -> Some l
  | _ -> None

let IlxInstr_is_tailcall i = 
  match i with 
  |  (EI_callfunc (Tailcall,_)) |  (EI_callclo (Tailcall,_,_)) -> true
  | _ -> false

let remap_ilx_labels lab2cl i = 
  match i with 
    | EI_brisdata (a,b,l1,l2) -> EI_brisdata (a,b,lab2cl l1,lab2cl l2)
    | EI_datacase (b,x,ls,l) -> EI_datacase (b,x,List.map (fun (y,l) -> (y,lab2cl l)) ls, lab2cl l)
    | _ -> i

let (mk_ilx_ext_instr,is_ilx_ext_instr,dest_ilx_ext_instr) = 
  define_instr_extension  
    { instrExtDests=destinations_of_IlxInstr;
      instrExtFallthrough=fallthrough_of_IlxInstr;
      instrExtIsTailcall=IlxInstr_is_tailcall;
      instrExtRelabel=remap_ilx_labels; }

let mk_IlxInstr i = I_other (mk_ilx_ext_instr i)

// Define an extension of the IL algebra of type definitions
type IlxClosureInfo = 
    { cloStructure: IlxClosureLambdas;
      cloFreeVars: IlxClosureFreeVar list;  
      cloCode: (ILMethodBody Lazy.t);
      cloSource: ILSourceMarker option}

and IlxUnionInfo = 
    { cudReprAccess: ILMemberAccess; (* is the representation public? *)
      cudHelpersAccess: ILMemberAccess; (* are the representation public? *)
      cudHelpers: bool; (* generate the helpers? *)
      cudDebugProxies: bool; (* generate the helpers? *)
      cudDebugDisplayAttributes: ILAttribute list;
      cudAlternatives: IlxUnionAlternative array;
      cudNullPermitted: bool;
      (* debug info for generated code for classunions *) 
      cudWhere: ILSourceMarker option; }

type IlxTypeDefKind = 
 | ETypeDef_closure of IlxClosureInfo
 | ETypeDef_classunion of IlxUnionInfo


let (mk_ilx_ext_type_def_kind,is_ilx_ext_type_def_kind,dest_ilx_ext_type_def_kind) = 
  (define_type_def_kind_extension Type_def_kind_extension : (IlxTypeDefKind -> IlxExtensionTypeKind) * (IlxExtensionTypeKind -> bool) * (IlxExtensionTypeKind -> IlxTypeDefKind) )

let mk_IlxTypeDefKind i = TypeDef_other (mk_ilx_ext_type_def_kind i)

(* --------------------------------------------------------------------
 * Define these as extensions of the IL types
 * -------------------------------------------------------------------- *)

let dest_func_app = function Apps_app (d,r) -> d,r | _ -> failwith "dest_func_app"
let dest_tyfunc_app = function Apps_tyapp (b,c) -> b,c | _ -> failwith "dest_tyfunc_app"

let gen_mk_array_ty (shape,ty) = Type_array(shape,ty)

let gen_is_array_ty ty = 
  match ty with 
    Type_array _ -> true
  | _ -> false

let gen_dest_array_ty ty =
  match ty with 
  | Type_array(shape,ty) -> (shape,ty)
  | _ -> failwith "gen_dest_array_ty"


let mk_array_ty_old (shape,ty) = gen_mk_array_ty (shape,ty)


(* --------------------------------------------------------------------
 * MS-ILX: Closures
 * -------------------------------------------------------------------- *)

let rec inst_apps_aux n inst = function
    Apps_tyapp (ty,rty) -> Apps_tyapp(inst_typ_aux n inst ty, inst_apps_aux n inst rty)
  | Apps_app (dty,rty) ->  Apps_app(inst_typ_aux n inst dty, inst_apps_aux n inst rty)
  | Apps_done rty ->  Apps_done(inst_typ_aux n inst rty)

let rec inst_lambdas_aux n inst = function
  | Lambdas_forall (b,rty) -> 
      Lambdas_forall(b, inst_lambdas_aux n inst rty)
  | Lambdas_lambda (p,rty) ->  
      Lambdas_lambda({ p with paramType=inst_typ_aux n inst p.paramType},inst_lambdas_aux n inst rty)
  | Lambdas_return rty ->  Lambdas_return(inst_typ_aux n inst rty)

let inst_lambdas i t = inst_lambdas_aux 0 i t

let cloref_of_clospec (IlxClosureSpec(cloref,_)) = cloref

let tref_of_clospec (IlxClosureSpec(IlxClosureRef(tref,_,_),_)) = tref
let formal_freevar_type_of_cloref (IlxClosureRef(_,_,fvs)) n = 
  typ_of_freevar (List.nth fvs n)
let formal_freevar_type_of_clospec x n = formal_freevar_type_of_cloref (cloref_of_clospec x) n
let actual_freevar_type_of_clospec (IlxClosureSpec(cloref,inst)) n = 
  inst_typ inst (formal_freevar_type_of_cloref cloref n)
let actual_freevars_of_clospec (IlxClosureSpec(IlxClosureRef(_,_,fvs),inst)) = 
  List.map (fun fv -> {fv with fvType = inst_typ inst fv.fvType}) fvs
let formal_freevars_of_clospec (IlxClosureSpec(IlxClosureRef(_,_,fvs),inst)) = fvs
let actual_lambdas_of_clospec (IlxClosureSpec(IlxClosureRef(_,lambdas,_),inst)) = inst_lambdas inst lambdas
let formal_lambdas_of_clospec (IlxClosureSpec(IlxClosureRef(_,lambdas,_),_)) = lambdas
let inst_of_clospec (IlxClosureSpec(_,inst)) = inst
let generalize_cloref gparams csig = IlxClosureSpec(csig, generalize_gparams gparams)



(* --------------------------------------------------------------------
 * MS-ILX: Unions
 * -------------------------------------------------------------------- *)

let objtype_of_cuspec (IlxUnionSpec(IlxUnionRef(tref,_,_),inst)) = mk_boxed_typ tref inst
let tref_of_cuspec (IlxUnionSpec(IlxUnionRef(tref,_,_),inst)) = tref
let inst_of_cuspec (IlxUnionSpec(_,inst)) = inst
let altsarray_of_cuspec (IlxUnionSpec(IlxUnionRef(tref,alts,_),inst)) = alts
let nullPermitted_of_cuspec (IlxUnionSpec(IlxUnionRef(_,_,np),inst)) = np
let alts_of_cuspec cuspec = Array.to_list (altsarray_of_cuspec cuspec)
let alt_of_cuspec cuspec n = (altsarray_of_cuspec cuspec).[n]

let alt_is_nullary alt = (alt.altFields.Length = 0)
let fdefs_of_alt alt = alt.altFields
let name_of_alt alt = alt.altName
let fdef_of_alt alt fidx = (fdefs_of_alt alt).[fidx]

let fdef_of_cuspec cuspec idx fidx = fdef_of_alt (alt_of_cuspec cuspec idx) fidx

let actual_typ_of_cuspec_field cuspec idx fidx =
  inst_typ (inst_of_cuspec cuspec) (fdef_of_cuspec cuspec idx fidx).fdType

