// (c) Microsoft Corporation. All rights reserved

module (* internal *) Microsoft.FSharp.Compiler.AbstractIL.Internal.AsciiConstants 

open Internal.Utilities

open Microsoft.FSharp.Compiler.AbstractIL 
open Microsoft.FSharp.Compiler.AbstractIL.Internal 
module Ildiag = Microsoft.FSharp.Compiler.AbstractIL.Diagnostics 
module Ilx = Microsoft.FSharp.Compiler.AbstractIL.Extensions.ILX.Types 
module Il = Microsoft.FSharp.Compiler.AbstractIL.IL 

open Ildiag
open Il
open Ilx

(* The hasthis i.e. static/nonstatic part of the calling convention *)
(* participates in binding, i.e. constructors have the flag set. *)
(* However, when quoting a constructor at the callsite of a newobj *)
(* instruction the flag does not have to be given, even though that *)
(* results in an inadequate specification of the method to call.  *)
(* Thus we perform a fix-up on every method quoted at a newobj instruction, *)
(* forcing the flag to be set.  This is only for naming/linking purposes, *)
(* and the semantic meaning of the flag, as stored in the method callsig, is *)
(* not effected.  *)
let set_hasthis_in_callconv hasthis (Callconv (_,bcc)) = Callconv (hasthis,bcc)
let set_hasthis_in_callsig hasthis x = {x with callsigCallconv= set_hasthis_in_callconv hasthis x.callsigCallconv}
let set_hasthis_in_mref hasthis (x:ILMethodRef) = 
  ILMethodRef.Create(enclosingTypeRef=x.EnclosingTypeRef,
                     callingConv=set_hasthis_in_callconv hasthis x.CallingConv,
                     name=x.Name,
                     genericArity=x.GenericArity,
                     argTypes= x.ArgTypes,
                     returnType= x.ReturnType)

let set_hasthis_in_mspec hasthis x = let x1,x2,x3 = dest_mspec x in mk_mref_mspec_in_typ(set_hasthis_in_mref hasthis x1,x2,x3)

let pretty () = true
let parse_ilGlobals = ref (ecmaILGlobals) 

(* -------------------------------------------------------------------- 
 * Table of parsing and pretty printing data for instructions.
 *   - PP data is only used for instructions with no arguments
 * -------------------------------------------------------------------- *)

let noarg_instrs = 
 lazy [
  ["ldc";"i4";"0"],           (((mk_ldc_i32 (0))));
  ["ldc";"i4";"1"],           (((mk_ldc_i32 (1))));
  ["ldc";"i4";"2"],           (((mk_ldc_i32 (2))));
  ["ldc";"i4";"3"],           (((mk_ldc_i32 (3))));
  ["ldc";"i4";"4"],           (((mk_ldc_i32 (4))));
  ["ldc";"i4";"5"],           (((mk_ldc_i32 (5))));
  ["ldc";"i4";"6"],           (((mk_ldc_i32 (6))));
  ["ldc";"i4";"7"],           (((mk_ldc_i32 (7))));
  ["ldc";"i4";"8"],           (((mk_ldc_i32 (8))));
  ["ldc";"i4";"M1"],           (((mk_ldc_i32 ((0-1)))));
  ["ldc";"i4";"m1"],           (((mk_ldc_i32 ((0-1)))));
  ["stloc";"0"],            (I_stloc (uint16 ( 0)));
  ["stloc";"1"],            (I_stloc (uint16 ( 1)));
  ["stloc";"2"],            (I_stloc (uint16 ( 2)));
  ["stloc";"3"],            (I_stloc (uint16 ( 3)));
  ["ldloc";"0"],            (I_ldloc (uint16 ( 0)));
  ["ldloc";"1"],            (I_ldloc (uint16 ( 1)));
  ["ldloc";"2"],            (I_ldloc (uint16 ( 2)));
  ["ldloc";"3"],            (I_ldloc (uint16 ( 3)));
  ["ldarg";"0"],            (I_ldarg (uint16 ( 0)));
  ["ldarg";"1"],            (I_ldarg (uint16 ( 1)));
  ["ldarg";"2"],            (I_ldarg (uint16 ( 2)));
  ["ldarg";"3"],            (I_ldarg (uint16 ( 3)));
  ["ret"],              (I_ret);
  ["add"],              (I_arith AI_add);
  ["add";"ovf"],        (I_arith AI_add_ovf);
  ["add";"ovf";"un"],   (I_arith AI_add_ovf_un);
  ["and"],              (I_arith AI_and);  
  ["div"],              (I_arith AI_div); 
  ["div";"un"],         (I_arith AI_div_un);
  ["ceq"],              (I_arith AI_ceq);  
  ["cgt"],              (I_arith AI_cgt );
  ["cgt";"un"],         (I_arith AI_cgt_un);
  ["clt"],              (I_arith AI_clt);
  ["clt";"un"],         (I_arith AI_clt_un);
  ["conv";"i1"],        (I_arith (AI_conv DT_I1));  
  ["conv";"i2"],   (I_arith (AI_conv DT_I2));  
  ["conv";"i4"],   (I_arith (AI_conv DT_I4));  
  ["conv";"i8"],   (I_arith (AI_conv DT_I8));  
  ["conv";"i"],   (I_arith (AI_conv DT_I));  
  ["conv";"r4"],   (I_arith (AI_conv DT_R4));  
  ["conv";"r8"],   (I_arith (AI_conv DT_R8));  
  ["conv";"u1"],   (I_arith (AI_conv DT_U1));  
  ["conv";"u2"],   (I_arith (AI_conv DT_U2));  
  ["conv";"u4"],   (I_arith (AI_conv DT_U4));  
  ["conv";"u8"],   (I_arith (AI_conv DT_U8));  
  ["conv";"u"],   (I_arith (AI_conv DT_U));  
  ["conv";"r"; "un"],   (I_arith (AI_conv DT_R));  
  ["conv";"ovf";"i1"],   (I_arith (AI_conv_ovf DT_I1));  
  ["conv";"ovf";"i2"],   (I_arith (AI_conv_ovf DT_I2));  
  ["conv";"ovf";"i4"],   (I_arith (AI_conv_ovf DT_I4));  
  ["conv";"ovf";"i8"],   (I_arith (AI_conv_ovf DT_I8));  
  ["conv";"ovf";"i"],   (I_arith (AI_conv_ovf DT_I));  
  ["conv";"ovf";"u1"],   (I_arith (AI_conv_ovf DT_U1));  
  ["conv";"ovf";"u2"],   (I_arith (AI_conv_ovf DT_U2));  
  ["conv";"ovf";"u4"],   (I_arith (AI_conv_ovf DT_U4));  
  ["conv";"ovf";"u8"],   (I_arith (AI_conv_ovf DT_U8));  
  ["conv";"ovf";"u"],   (I_arith (AI_conv_ovf DT_U));  
  ["conv";"ovf";"i1"; "un"],   (I_arith (AI_conv_ovf_un DT_I1));  
  ["conv";"ovf";"i2"; "un"],   (I_arith (AI_conv_ovf_un DT_I2));  
  ["conv";"ovf";"i4"; "un"],   (I_arith (AI_conv_ovf_un DT_I4));  
  ["conv";"ovf";"i8"; "un"],   (I_arith (AI_conv_ovf_un DT_I8));  
  ["conv";"ovf";"i"; "un"],   (I_arith (AI_conv_ovf_un DT_I));  
  ["conv";"ovf";"u1"; "un"],   (I_arith (AI_conv_ovf_un DT_U1));  
  ["conv";"ovf";"u2"; "un"],   (I_arith (AI_conv_ovf_un DT_U2));  
  ["conv";"ovf";"u4"; "un"],   (I_arith (AI_conv_ovf_un DT_U4));  
  ["conv";"ovf";"u8"; "un"],   (I_arith (AI_conv_ovf_un DT_U8));  
  ["conv";"ovf";"u"; "un"],   (I_arith (AI_conv_ovf_un DT_U));  
  ["stelem";"i1"],   (I_stelem DT_I1);  
  ["stelem";"i2"],   (I_stelem DT_I2);  
  ["stelem";"i4"],   (I_stelem DT_I4);  
  ["stelem";"i8"],   (I_stelem DT_I8);  
  ["stelem";"r4"],   (I_stelem DT_R4);  
  ["stelem";"r8"],   (I_stelem DT_R8);  
  ["stelem";"i"],   (I_stelem DT_I);  
  ["stelem";"u"],   (I_stelem DT_I);  
  ["stelem";"u8"],   (I_stelem DT_I8);  
  ["stelem";"ref"],   (I_stelem DT_REF);  
  ["ldelem";"i1"],   (I_ldelem DT_I1);  
  ["ldelem";"i2"],   (I_ldelem DT_I2);  
  ["ldelem";"i4"],   (I_ldelem DT_I4);  
  ["ldelem";"i8"],   (I_ldelem DT_I8);  
  ["ldelem";"u8"],   (I_ldelem DT_I8);  
  ["ldelem";"u1"],   (I_ldelem DT_U1);  
  ["ldelem";"u2"],   (I_ldelem DT_U2);  
  ["ldelem";"u4"],   (I_ldelem DT_U4);  
  ["ldelem";"r4"],   (I_ldelem DT_R4);  
  ["ldelem";"r8"],   (I_ldelem DT_R8);  
  ["ldelem";"u"],   (I_ldelem DT_I);  (* EQUIV *)
  ["ldelem";"i"],   (I_ldelem DT_I);  
  ["ldelem";"ref"],   (I_ldelem DT_REF);  
  ["mul"],   (I_arith AI_mul  );
  ["mul";"ovf"],   (I_arith AI_mul_ovf);
  ["mul";"ovf";"un"],   (I_arith AI_mul_ovf_un);
  ["rem"],   (I_arith AI_rem  );
  ["rem";"un"],   (I_arith AI_rem_un ); 
  ["shl"],   (I_arith AI_shl ); 
  ["shr"],   (I_arith AI_shr ); 
  ["shr";"un"],   (I_arith AI_shr_un);
  ["sub"],   (I_arith AI_sub  );
  ["sub";"ovf"],   (I_arith AI_sub_ovf);
  ["sub";"ovf";"un"],   (I_arith AI_sub_ovf_un); 
  ["xor"],   (I_arith AI_xor);  
  ["or"],   (I_arith AI_or);     
  ["neg"],   (I_arith AI_neg);     
  ["not"],   (I_arith AI_not);     
  ["ldnull"],   (I_arith AI_ldnull);   
  ["dup"],   (I_arith AI_dup);   
  ["pop"],   (I_arith AI_pop);
  ["ckfinite"],   (I_arith AI_ckfinite);
  ["nop"],   (I_arith AI_nop);
  ["break"],   (I_break);
  ["arglist"],   (I_arglist);
  ["endfilter"],   (I_endfilter);
  ["endfinally"],   I_endfinally;
  ["refanytype"],   (I_refanytype);
  ["localloc"],   (I_localloc);
  ["throw"],   (I_throw);
  ["ldlen"],   (I_ldlen);
  ["rethrow"],       (I_rethrow);
];;


let words_of_noarg_instr, is_noarg_instr = 
  let t = 
    lazy begin 
      let t = Hashtbl.create 300 in 
      List.iter (fun (x,mk) -> Hashtbl.add t mk x) (Lazy.force noarg_instrs);
      t
    end in 
  (fun s -> Hashtbl.find (Lazy.force t) s),
  (fun s -> Hashtbl.mem (Lazy.force t) s)

(* -------------------------------------------------------------------- 
 * Instructions are preceded by prefixes, e.g. ".tail" etc.
 * -------------------------------------------------------------------- *)

type instr_name = string list
type 'a named =  instr_name * 'a

type prefix = 
  | Prefix_Tail
  | Prefix_Volatile
  | Prefix_Unaligned of int
  | Prefix_Readonly
  | Prefix_Constrained of ILType
type prefixes = prefix list

let prefix_processor nm mk = (nm, fun x (prefixes:prefixes) -> mk prefixes x)

let no_prefixes (nm,mk) =
  prefix_processor nm
     (function 
        [] -> mk
      | h::t -> failwith ("no prefixes are not allowed for instruction"^String.concat "." nm))

let int_to_unaligned n = 
  if n = 1 then Unaligned_1 
  else if n = 2 then Unaligned_2
  else if n = 4 then Unaligned_4
  else failwith "int_to_unaligned"

let volatile_unaligned_prefix (nm,mk) =
  prefix_processor nm
     (function
        [] -> mk(Aligned, Nonvolatile)
      | [Prefix_Unaligned n] -> mk(int_to_unaligned n, Nonvolatile)
      | [Prefix_Volatile] -> mk(Aligned, Volatile)
      | [Prefix_Unaligned n;Prefix_Volatile]
      | [Prefix_Volatile;Prefix_Unaligned n] -> mk(int_to_unaligned n, Volatile)
      | _ -> failwith ("bad prefix for instruction"^String.concat "." nm))

let volatile_prefix (nm,mk) =
  prefix_processor nm 
     (function  
        [] -> mk(Nonvolatile)
      | [Prefix_Volatile] -> mk(Volatile)
      | _ -> failwith ("bad prefix for instruction"^String.concat "." nm))

let tail_prefix (nm,mk) =
  prefix_processor nm 
     (function 
       [] -> mk Normalcall
      | [Prefix_Tail] -> mk Tailcall
      | _ -> failwith ("bad prefix for instruction"^String.concat "." nm))

let readonly_prefix (nm,mk) =
  prefix_processor nm 
     (function 
       [] -> mk NormalAddress
      | [Prefix_Readonly] -> mk ReadonlyAddress
      | _ -> failwith ("bad prefix for instruction"^String.concat "." nm))

let constraint_tail_prefix (nm,mk) =
  prefix_processor nm 
     (function 
       [] -> mk (None, Normalcall)
      | [Prefix_Tail] -> mk (None,Tailcall)
      | [Prefix_Constrained ty; Prefix_Tail]
      | [ Prefix_Tail; Prefix_Constrained ty] -> mk (Some ty,Tailcall)
      | [ Prefix_Constrained ty] -> mk (Some ty,Normalcall)
      | _ -> failwith ("bad prefix for instruction"^String.concat "." nm))

let mk_stind (nm,dt) = volatile_unaligned_prefix (nm, (fun (x,y) () -> I_stind(x,y,dt)))
let mk_ldind (nm,dt) = volatile_unaligned_prefix (nm, (fun (x,y) () -> I_ldind(x,y,dt)))

(* -------------------------------------------------------------------- 
 * Parsing only...  Tables of different types of instructions.
 *  First the different kinds of instructions.
 * -------------------------------------------------------------------- *)

type CoreInstr = prefixes -> ILInstr

type none_instr = (unit -> CoreInstr)
type i32_instr = (int32 ->  CoreInstr)
type i32_i32_instr = (int32 * int32 ->  CoreInstr)
type arg_instr = (uint16 ->  CoreInstr)
type env_instr = (int ->  CoreInstr)
type loc_instr = (uint16 ->  CoreInstr)
type arg_typ_instr = (uint16 * ILType ->  CoreInstr)
type i64_instr = (int64 ->  CoreInstr)
type real_instr = (ILConstSpec ->  CoreInstr)
//type field_instr = (ILFieldSpec ->  CoreInstr)
type method_instr = (ILMethodSpec * varargs ->  CoreInstr)
type unconditional_instr = (ILCodeLabel ->  CoreInstr)
type conditional_instr = (ILCodeLabel * ILCodeLabel ->  CoreInstr)
type type_instr = (ILType ->  CoreInstr)
type int_type_instr = (int * ILType ->  CoreInstr)
type valuetype_instr = (ILType ->  CoreInstr)  (* nb. diff. interp of types to type_instr *)
type string_instr = (string ->  CoreInstr)
//type sig_instr = (ILCallingSignature * varargs ->  CoreInstr)
type tok_instr = (ILTokenSpec ->  CoreInstr)
type switch_instr = (ILCodeLabel list * ILCodeLabel ->  CoreInstr)

(* -------------------------------------------------------------------- 
 * Now the generic code to make a table of instructions
 * -------------------------------------------------------------------- *)

type 'a instr_table = (string list * 'a) list
type 'a lazy_instr_table = 'a instr_table Lazy.t
let mk_table l =  l 
(*  let tab = Hashtbl.create 100 in  
  List.iter (fun (x,mk) -> Hashtbl.add tab x mk) l;
  (tab : 'a instr_table);;
*)
(* -------------------------------------------------------------------- 
 * Now the tables of instructions
 * -------------------------------------------------------------------- *)

let arg_instrs = 
lazy (mk_table [
  no_prefixes (["ldarg"],           (fun x -> I_ldarg x));
  no_prefixes (["ldarg";"s"],       (fun x -> I_ldarg x));
  no_prefixes (["starg"],           (fun x -> I_starg x));
  no_prefixes (["starg";"s"],       (fun x -> I_starg x));
  no_prefixes (["ldarga"],          (fun x -> I_ldarga x));
  no_prefixes (["ldarga";"s"],      (fun x -> I_ldarga x));
]  : arg_instr instr_table)

let loc_instrs = 
lazy (mk_table [
  no_prefixes (["stloc"],           (fun x -> I_stloc x));
  no_prefixes (["stloc";"s"],       (fun x -> I_stloc x));
  no_prefixes (["ldloc"],           (fun x -> I_ldloc x));
  no_prefixes (["ldloc";"s"],       (fun x -> I_ldloc x));
  no_prefixes (["ldloca"],          (fun x -> I_ldloca x));
  no_prefixes (["ldloca";"s"],      (fun x -> I_ldloca x));
]  : loc_instr instr_table)

let none_instrs =  
 lazy (mk_table 
  (List.map (fun (nm,i) -> no_prefixes (nm,(fun () -> i))) (Lazy.force noarg_instrs) @
   [  (mk_stind (["stind";"u"],            DT_I));
      (mk_stind (["stind";"i"],            DT_I));
      (mk_stind (["stind";"u1"],           DT_I1));(* ILX EQUIVALENT *)
      (mk_stind (["stind";"i1"],           DT_I1));
      (mk_stind (["stind";"u2"],           DT_I2));
      (mk_stind (["stind";"i2"],           DT_I2));
      (mk_stind (["stind";"u4"],           DT_I4));  (* ILX EQUIVALENT *)
      (mk_stind (["stind";"i4"],           DT_I4));
      (mk_stind (["stind";"u8"],           DT_I8));   (* ILX EQUIVALENT *)
      (mk_stind (["stind";"i8"],           DT_I8));
      (mk_stind (["stind";"r4"],           DT_R4));
      (mk_stind (["stind";"r8"],           DT_R8));
      (mk_stind (["stind";"ref"],          DT_REF));
      (mk_ldind (["ldind";"i"],            DT_I));
      (mk_ldind (["ldind";"i1"],           DT_I1));
      (mk_ldind (["ldind";"i2"],           DT_I2));
      (mk_ldind (["ldind";"i4"],           DT_I4));
      (mk_ldind (["ldind";"i8"],           DT_I8));
      (mk_ldind (["ldind";"u1"],           DT_U1));
      (mk_ldind (["ldind";"u2"],           DT_U2));
      (mk_ldind (["ldind";"u4"],           DT_U4));
      (mk_ldind (["ldind";"u8"],           DT_I8));
      (mk_ldind (["ldind";"r4"],           DT_R4));
      (mk_ldind (["ldind";"r8"],           DT_R8));
      (mk_ldind (["ldind";"ref"],          DT_REF));
     volatile_unaligned_prefix (["cpblk"], (fun (x,y) () -> I_cpblk(x,y)));
     volatile_unaligned_prefix (["initblk"], (fun (x,y) () -> I_initblk(x,y)));
   ]) : none_instr instr_table);;

let i64_instrs = 
 lazy (mk_table [
   no_prefixes (["ldc";"i8"], (fun x ->I_arith (AI_ldc (DT_I8, NUM_I8 x))));
  ] : i64_instr instr_table)

let i32_instrs = 
 lazy (mk_table [
   no_prefixes (["ldc";"i4"],     (fun x -> ((mk_ldc_i32 x))));
   no_prefixes (["ldc";"i4";"s"], (fun x -> ((mk_ldc_i32 x))));
  ] : i32_instr instr_table)

let i32_i32_instrs = 
 lazy (mk_table [
   no_prefixes (["ldlen";"multi"],     (fun (x,y) -> EI_ldlen_multi (x, y)));
  ] : i32_i32_instr instr_table)

let real_instrs = 
 lazy (mk_table [
   no_prefixes (["ldc";"r4"],     (fun x -> I_arith (AI_ldc (DT_R4, x))));
   no_prefixes (["ldc";"r8"],     (fun x -> I_arith (AI_ldc (DT_R8, x))));
  ]  : real_instr instr_table)

//let field_instrs = 
// lazy (mk_table [
//    (volatile_unaligned_prefix (["ldfld"], (fun (x,y) fspec -> I_ldfld(x,y,fspec))));
//    (volatile_unaligned_prefix (["stfld"], (fun  (x,y) fspec -> I_stfld(x,y,fspec))));
//    (volatile_prefix (["ldsfld"], (fun x fspec -> I_ldsfld (x, fspec))));
//    (volatile_prefix (["stsfld"], (fun x fspec -> I_stsfld (x, fspec))));
//    (no_prefixes (["ldflda"], (fun fspec -> I_ldflda fspec)));
//    (no_prefixes (["ldsflda"],(fun fspec -> I_ldsflda fspec)));
//  ]  : field_instr instr_table)

let method_instrs = 
 lazy (mk_table [
    (tail_prefix (["call"],      (fun tl (mspec,y) -> I_call (tl,mspec,y))));
    (no_prefixes (["ldftn"],     (fun (mspec,y) -> I_ldftn mspec)));
    (no_prefixes (["ldvirtftn"], (fun (mspec,y) -> I_ldvirtftn mspec)));
(* nb. set "instance" bit in method id for newobj and callvirt *)
(* This is the behaviour of ILDASM.  You'd think it would also do it for *)
(* ldvirtftn but it doesn't.  *)
    (no_prefixes (["newobj"],  (fun (mspec,y) -> I_newobj (set_hasthis_in_mspec CC_instance mspec,y))));
    (constraint_tail_prefix (["callvirt"], 
                             (fun (cons,tl) (mspec,y) -> 
                               let mspec2 = set_hasthis_in_mspec CC_instance mspec in 
                               match cons with 
                               | Some ty -> I_callconstraint (tl,ty,mspec2,y)
                               | None -> I_callvirt (tl,set_hasthis_in_mspec CC_instance mspec,y))));
  ]  : method_instr instr_table)

let string_instrs = 
 lazy (mk_table [
   no_prefixes (["ldstr"],    (fun x -> I_ldstr x));
  ]  : string_instr instr_table)

let switch_instrs = 
 lazy (mk_table [
   no_prefixes (["switch"],   (fun x -> I_switch x));
  ]  : switch_instr instr_table)

let tok_instrs = 
 lazy (mk_table [
   no_prefixes (["ldtoken"],   (fun x -> I_ldtoken x));
  ]  : tok_instr instr_table)


//let sig_instrs = 
// lazy (mk_table [
//    (tail_prefix (["calli"],   (fun tl (x,y) -> I_calli (tl, x, y))));
//  ]  : sig_instr instr_table)

let type_instrs = 
 lazy (mk_table [
  no_prefixes (["mkrefany"],  (fun x -> I_mkrefany x));
  no_prefixes (["refanyval"],  (fun x -> I_refanyval x));
  readonly_prefix (["ldelema"],   (fun ro x -> I_ldelema (ro,Rank1ArrayShape,x)));
  no_prefixes (["ldelem";"any"], (fun x -> I_ldelem_any (Rank1ArrayShape,x)));
  no_prefixes (["stelem";"any"], (fun x -> I_stelem_any (Rank1ArrayShape, x)));
  no_prefixes (["newarr"],    (fun x -> I_newarr (Rank1ArrayShape,x)));  
  no_prefixes (["castclass"], (fun x -> I_castclass x));
  no_prefixes (["ilzero"], (fun x -> EI_ilzero x));
  no_prefixes (["isinst"],    (fun x -> I_isinst x));
  no_prefixes (["initobj";"any"],   (fun x -> I_initobj x));
  no_prefixes (["unbox";"any"],    (fun x -> I_unbox_any x));
]  : type_instr instr_table)

let mk_shape n = ILArrayShape(Array.to_list (Array.create n (None,None)))
let int_type_instrs = 
 lazy (mk_table [
  no_prefixes (["ldelem";"multi"], (fun (x,y) -> (I_ldelem_any (mk_shape x,y))));
  no_prefixes (["stelem";"multi"], (fun (x,y) -> (I_stelem_any (mk_shape x,y))));
  no_prefixes (["newarr";"multi"], (fun (x,y) -> (I_newarr (mk_shape x,y))));  
  readonly_prefix (["ldelema";"multi"], (fun ro (x,y) -> (I_ldelema (ro,mk_shape x,y))));  
]  : int_type_instr instr_table)

let valuetype_instrs = 
 lazy (mk_table [
  no_prefixes (["cpobj"],     (fun x -> I_cpobj x));
  no_prefixes (["initobj"],   (fun x -> I_initobj x));
  volatile_unaligned_prefix (["ldobj"], (fun (x,y) z -> I_ldobj (x,y,z)));
  volatile_unaligned_prefix (["stobj"], (fun (x,y) z -> I_stobj (x,y,z)));
  no_prefixes (["sizeof"],    (fun x -> I_sizeof x));
  no_prefixes (["box"],       (fun x -> I_box x));
  no_prefixes (["unbox"],     (fun x -> I_unbox x));
]  : valuetype_instr instr_table)

(* -------------------------------------------------------------------- 
 * Parser/lexer state.
 * -------------------------------------------------------------------- *)

let lexing_bytearray = ref false;;
