// (c) Microsoft Corporation. All rights reserved

/// Various constants and utilities used when parsing the ILASM format for IL
module (* internal *) Microsoft.FSharp.Compiler.AbstractIL.Internal.AsciiConstants

open Internal.Utilities

open Microsoft.FSharp.Compiler.AbstractIL
open Microsoft.FSharp.Compiler.AbstractIL.Internal

module Ilx = Microsoft.FSharp.Compiler.AbstractIL.Extensions.ILX.Types 
module Il = Microsoft.FSharp.Compiler.AbstractIL.IL 

open Ilx
open Il

// -------------------------------------------------------------------- 
// IL Parser state - must be initialized before parsing a module
// -------------------------------------------------------------------- 

val parse_ilGlobals: ILGlobals ref

// -------------------------------------------------------------------- 
// IL Lexer and pretty-printer tables
// -------------------------------------------------------------------- 

type instr_name = string list
type prefix = 
  | Prefix_Tail
  | Prefix_Volatile
  | Prefix_Unaligned of int
  | Prefix_Readonly
  | Prefix_Constrained of ILType
type prefixes = prefix list

type CoreInstr = prefixes -> ILInstr

type none_instr = unit -> CoreInstr
type i32_instr = int32 -> CoreInstr
type i32_i32_instr = int32 * int32 -> CoreInstr
type arg_instr = uint16 -> CoreInstr
type loc_instr = uint16 -> CoreInstr
type env_instr = int -> CoreInstr
type arg_typ_instr = uint16 * ILType -> CoreInstr
type i64_instr = int64 -> CoreInstr
type real_instr = ILConstSpec -> CoreInstr
//type field_instr = ILFieldSpec -> CoreInstr
type method_instr = ILMethodSpec * varargs -> CoreInstr
type unconditional_instr = ILCodeLabel -> CoreInstr
type conditional_instr = ILCodeLabel * ILCodeLabel -> CoreInstr
type type_instr = ILType -> CoreInstr
type int_type_instr = int * ILType -> CoreInstr
type valuetype_instr = ILType -> CoreInstr
type string_instr = string -> CoreInstr
//type sig_instr = ILCallingSignature * varargs -> CoreInstr
type tok_instr = ILTokenSpec -> CoreInstr
type switch_instr = ILCodeLabel list * ILCodeLabel -> CoreInstr

type 'a instr_table = (string list * 'a) list
type 'a lazy_instr_table = 'a instr_table Lazy.t

val loc_instrs: loc_instr lazy_instr_table
val arg_instrs: arg_instr lazy_instr_table
val none_instrs: none_instr lazy_instr_table
val i64_instrs: i64_instr lazy_instr_table
val i32_instrs: i32_instr lazy_instr_table
val i32_i32_instrs: i32_i32_instr lazy_instr_table
val real_instrs: real_instr lazy_instr_table
//val field_instrs: field_instr lazy_instr_table
val method_instrs: method_instr lazy_instr_table
val string_instrs: string_instr lazy_instr_table
val switch_instrs: switch_instr lazy_instr_table
val tok_instrs: tok_instr lazy_instr_table
//val sig_instrs: sig_instr lazy_instr_table
val type_instrs: type_instr lazy_instr_table
val int_type_instrs: int_type_instr lazy_instr_table
val valuetype_instrs: valuetype_instr lazy_instr_table

val words_of_noarg_instr : (ILInstr -> string list)
val is_noarg_instr : (ILInstr -> bool)

(* -------------------------------------------------------------------- 
 * Lexer state
 * -------------------------------------------------------------------- *)

val lexing_bytearray: bool ref



