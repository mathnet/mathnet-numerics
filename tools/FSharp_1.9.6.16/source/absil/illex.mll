
{
// (c) Microsoft Corporation 2005-2009. 
  
module Microsoft.FSharp.Compiler.AbstractIL.Internal.AsciiLexer 

open Internal.Utilities
open Internal.Utilities.Text
open Internal.Utilities.Text.Lexing
open Microsoft.FSharp.Compiler.AbstractIL 
open Microsoft.FSharp.Compiler.AbstractIL.Internal 

module Ildiag = Microsoft.FSharp.Compiler.AbstractIL.Diagnostics 
module Ilpars = Microsoft.FSharp.Compiler.AbstractIL.Internal.AsciiParser 
module Ilascii = Microsoft.FSharp.Compiler.AbstractIL.Internal.AsciiConstants 


open Ildiag
open Ilpars
open Ilascii

let lexeme (lexbuf : LexBuffer<char>) = new System.String(lexbuf.Lexeme)

let unexpected_char lexbuf =
  dprintf "Unexpected character '%s'" (lexeme lexbuf);
  raise Parsing.RecoverableParseError ;;

let unquote n m s =
   String.sub s n (String.length s-(n+m));;

(* -------------------------------------------------------------------- 
 * STRING LITERALS 
 * -------------------------------------------------------------------- *)

let string_buffer = Buffer.create 256
let reset_string_buffer () = Buffer.clear string_buffer
let store_string_char c = Buffer.add_char string_buffer c
let get_stored_string () =Buffer.contents string_buffer

let escape = function
  | 'n' -> '\010'
  | 'r' -> '\013'
  | 'b' -> '\008'
  | 't' -> '\009'
  | c   -> c

(* -------------------------------------------------------------------- 
 * Keywords
 * -------------------------------------------------------------------- *)

let keywords = lazy

[
  "void",VOID
; "bool",BOOL
; "bytearray",BYTEARRAY
; "cdecl",CDECL
; "char",CHAR
; "class",CLASS
; "default",DEFAULT
; "explicit",EXPLICIT
; "fastcall",FASTCALL
; "float32",FLOAT32
; "float64",FLOAT64
; "instance",INSTANCE
; "int",INT
; "int16",INT16
; "int32",INT32
; "int64",INT64
; "int8",INT8
; "method",METHOD
; "native",NATIVE
; "object", OBJECT
; "stdcall",STDCALL
; "string",STRING
; "thiscall",THISCALL
; "typedref",TYPEDREF
; "uint",UINT
; "uint16",UINT16
; "uint32",UINT32
; "uint64",UINT64
; "uint8",UINT8
; "unmanaged",UNMANAGED
; "unsigned",UNSIGNED
; "value",VALUE
; "valuetype",VALUETYPE
; "vararg",VARARG

] 

(* -------------------------------------------------------------------- 
 * Instructions
 * -------------------------------------------------------------------- *)

let addTable t f l = List.iter (fun (x,i) -> Hashtbl.add t (String.concat "." x) (f i)) (Lazy.force l)
    
let kwd_instr_table = 
  lazy begin
    let t = Hashtbl.create 1000 in 
    List.iter (fun (x,y) -> Hashtbl.add t x y) (Lazy.force keywords);
    addTable t (fun i -> INSTR_NONE i) none_instrs;
    addTable t (fun i -> INSTR_ARG i) arg_instrs;
    addTable t (fun i -> INSTR_LOC i) loc_instrs;
    addTable t (fun i -> INSTR_I i) i32_instrs;
    addTable t (fun i -> INSTR_I32_I32 i) i32_i32_instrs;
    addTable t (fun i -> INSTR_I8 i) i64_instrs;
    addTable t (fun i -> INSTR_R i) real_instrs;
    addTable t (fun i -> INSTR_METHOD i) method_instrs;
    //addTable t (fun i -> INSTR_FIELD i) field_instrs;
    addTable t (fun i -> INSTR_TYPE i) type_instrs;
    addTable t (fun i -> INSTR_INT_TYPE i) int_type_instrs;
    addTable t (fun i -> INSTR_VALUETYPE i) valuetype_instrs;
    addTable t (fun i -> INSTR_STRING i) string_instrs;
    //addTable t (fun i -> INSTR_SIG i) sig_instrs;
    addTable t (fun i -> INSTR_TOK i) tok_instrs;
    //addTable t (fun i -> INSTR_SWITCH i) switch_instrs;
    t
 end
  
let kwd_or_instr s = Hashtbl.find (Lazy.force kwd_instr_table) s (* words *)

let eval = function 
  | '0' -> 0  | '1' -> 1 | '2' -> 2  | '3' -> 3  | '4' -> 4  | '5' -> 5 
  | '6' -> 6  | '7' -> 7  | '8' -> 8  | '9' -> 9 
  | 'A' -> 10 | 'B' -> 11 | 'C' -> 12 | 'D' -> 13 | 'E' -> 14 | 'F' -> 15
  | 'a' -> 10 | 'b' -> 11 | 'c' -> 12 | 'd' -> 13 | 'e' -> 14 | 'f' -> 15
  | _ -> failwith "bad hexbyte"  

let kwd_or_instr_or_id s = if Hashtbl.mem  (Lazy.force kwd_instr_table) s  then kwd_or_instr s else VAL_ID s
        
}

(* -------------------------------------------------------------------- 
 * The Rules
 * -------------------------------------------------------------------- *)
rule token = parse
  | "," { COMMA }
  | "." { DOT }
  | "*" { STAR }
  | "!" { BANG }
  | "&" { AMP }
  | "(" { LPAREN }
  | ")" { RPAREN }
  | "[" { LBRACK }
  | "]" { RBRACK }
  | "/" { SLASH }
  | "<" { LESS }
  | ">" { GREATER }
  | "..." { ELIPSES }
  | "::" { DCOLON }
  | "+" { PLUS }
  | (['0'-'9']) | (['0'-'9']['0'-'9']['0'-'9']+)
      {  VAL_INT64(int64(lexeme lexbuf)) }

  (* We need to be able to parse all of *)
  (* ldc.r8     0. *)
  (* float64(-657435.)     *)
  (* and int32[0...,0...] *)
  (* The problem is telling an integer-followed-by-ellipses from a floating-point-nubmer-followed-by-dots *)

  | ((['0'-'9']) | (['0'-'9']['0'-'9']['0'-'9']+)) "..."
      {  let b = lexeme lexbuf in 
         VAL_INT32_ELIPSES(int32(String.sub b  0 (String.length b - 3))) }
  | ['0'-'9' 'A'-'F' 'a'-'f' ] ['0'-'9' 'A'-'F' 'a'-'f' ] 
      { let c1 = String.get (lexeme lexbuf) 0 in 
        let c2 = String.get (lexeme lexbuf) 1 in 
        if !lexing_bytearray then 
           VAL_HEXBYTE (16 * eval c1 + eval c2)  
        else if c1 >= '0' & c1 <= '9' & c2 >= '0' & c2 <= '9' then 
          VAL_INT64(int64 (10*eval c1 + eval c2) )
        else VAL_ID(lexeme lexbuf) }
  | '0' 'x' ['0'-'9' 'a'-'f' 'A'-'F']+ 
      { VAL_INT64(int64(lexeme lexbuf)) }
  | "FFFFFF"  ['0'-'9' 'A'-'F' 'a'-'f' ] ['0'-'9' 'A'-'F' 'a'-'f' ] 
      { let c1 = (lexeme lexbuf).[6] in 
        let c2 = (lexeme lexbuf).[7] in 
        if !lexing_bytearray then 
           VAL_HEXBYTE (16 * eval c1 + eval c2)
        else if c1 >= '0' & c1 <= '9' & c2 >= '0' & c2 <= '9' then 
          VAL_INT64(int64 (10*eval c1 + eval c2)) 
        else VAL_ID(lexeme lexbuf) }

  | '-' ['0'-'9']+ 
      { VAL_INT64(int64(lexeme lexbuf)) }
  | ('+'|'-')? ['0'-'9']+ ('.' ['0' - '9']*)? (('E'|'e') ('-'|'+')? ['0' - '9']+)?
      { VAL_FLOAT64( (float (lexeme lexbuf)) ) }

  | '\'' 
      { reset_string_buffer();
        singleQuoteString lexbuf;
        VAL_SQSTRING (get_stored_string()) }
  | "\""
      { reset_string_buffer();
        stringToken lexbuf;
        VAL_QSTRING (get_stored_string()) }

  | ("ldarg"|"ldc"|"ldloc"|"stloc"|"bne"|"conv"|"ble"|"bgt"|"bge"|"blt"|"cle"|"cgt"|"cge"|"clt"|"ceq"|"brtrue"|"brfalse"|"br"|"add"|"sub"|"div"|"rem"|"mul"|"beq"|"bne"|"cne"|"ldarga"|"ldloca"|"ldind"|"leave"|"newarr"|"shr"|"starg"|"stind"|"ldelem"|"ldelema"|"ldlen"|"stelem"|"unbox"|"box"|"initobj") '.' ['a'-'z' 'A'-'Z' '0'-'9' '.']+
      { let s = (lexeme lexbuf) in kwd_or_instr s }
  | [ '`'  '\128'-'\255' '@' '?' '$' 'a'-'z' 'A'-'Z' '_'] [  '`' '\128'-'\255' '$' 'a'-'z' 'A'-'Z' '0'-'9' '-' '_' '@' '$' ] *
      { kwd_or_instr_or_id (lexeme lexbuf) }
  | [ '`'  '\128'-'\255' '@' '?' '$' 'a'-'z' 'A'-'Z' '_'] [  '`' '\128'-'\255' '$' 'a'-'z' 'A'-'Z' '0'-'9' '-' '_' '@' '$' ]+
        ('.' [ '`'  '\128'-'\255' '@' '?' '$' 'a'-'z' 'A'-'Z' '_'] [  '`' '\128'-'\255' '$' 'a'-'z' 'A'-'Z' '0'-'9' '-' '_' '@' '$' ] +)+
      { VAL_DOTTEDNAME(lexeme lexbuf) } 
  | ".cctor" {DOT_CCTOR}                      
  | ".ctor" {DOT_CTOR}                
                                            
  |   [' ' '\t' '\r' '\n']                  
      { token lexbuf }                      
  | _ 
      { unexpected_char lexbuf }            
  | eof                                     
      { EOF }                                     

and singleQuoteString = parse
    '\''
      { () }
  | '\\' ("\010" | "\013" | "\013\010") [' ' '\009'] *
      { singleQuoteString lexbuf }
  | '\\' ['\\' '\'' 'n' 't' 'b' 'r']
      { store_string_char(escape(String.get (lexeme lexbuf) 1));
        singleQuoteString lexbuf }
  | eof
      { failwith "unterminated string" }
  | _
      { store_string_char(String.get (lexeme lexbuf) 0);
        singleQuoteString lexbuf }

and stringToken = parse
    '"'
      { () }
  | '\\' ("\010" | "\013" | "\013\010") [' ' '\009'] *
      { stringToken lexbuf }
  | '\\' ['\\' '"' 'n' 't' 'b' 'r']
      { store_string_char(escape(String.get (lexeme lexbuf) 1));
        stringToken lexbuf }
  | eof
      { failwith "unterminated string" }
  | _
      { store_string_char(String.get (lexeme lexbuf) 0);
        stringToken lexbuf }
