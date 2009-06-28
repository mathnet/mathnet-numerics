{

module Microsoft.FSharp.Compiler.Lexer

(*------------------------------------------------------------------------
 * The Lexer.  Some of the complication arises from the fact it is 
 * reused by the Visual Studio mode to do partial lexing reporting 
 * whitespace etc.
 * (c) Microsoft Corporation. All rights reserved 
 *-----------------------------------------------------------------------*)

open System.Text
open Internal.Utilities
open Internal.Utilities.Pervasives

open Microsoft.FSharp.Compiler.AbstractIL
open Microsoft.FSharp.Compiler.AbstractIL.Internal
open Microsoft.FSharp.Compiler

open Microsoft.FSharp.Compiler.Range
open Microsoft.FSharp.Compiler.Ast
open Microsoft.FSharp.Compiler.ErrorLogger
open Microsoft.FSharp.Compiler.Parser
open Microsoft.FSharp.Compiler.Lexhelp
open Microsoft.FSharp.Compiler.Lib
open Internal.Utilities.Text.Lexing

let lexeme (lexbuf : UnicodeLexing.Lexbuf) = 
    UnicodeLexing.Lexbuf.LexemeString(lexbuf)
let ulexeme lexbuf = lexeme lexbuf


let adjust_lexbuf_start_pos (lexbuf:UnicodeLexing.Lexbuf) p =  lexbuf.StartPos <- p 

let string_trim_both s n m = String.sub s n (String.length s - (n+m))
let trim_both   lexbuf n m = string_trim_both (ulexeme lexbuf) n m
let trim_right  lexbuf n = trim_both lexbuf 0 n
let trim_left   lexbuf n = trim_both lexbuf n 0

let fail args lexbuf msg dflt =
     let m = GetLexerRange  lexbuf in 
     args.errorLogger.ErrorR(Error(msg,m));
     dflt        
     
let trim_to_i32 args lexbuf n = 
    try int32 (trim_right lexbuf n)
    with _ -> fail args lexbuf "This number is outside the allowable range for this integer type" 0

let CheckExprOp lexbuf = 
    if String.contains (ulexeme lexbuf)  ':' then 
        deprecated "':' is no longer permitted as a character in operator names" (GetLexerRange lexbuf) 

let quote_op_is_raw s = String.length s >= 1 && s.[0] = '@'

let rev_string s = 
  let n = String.length s in 
  let res = Buffer.create n in 
  for i = n - 1 downto 0 do Buffer.add_char res s.[i] done;
  Buffer.contents res
  
let unexpected_char lexbuf =
    LEX_FAILURE ("Unexpected character '"^(lexeme lexbuf)^"'")

let start_string args (lexbuf: UnicodeLexing.Lexbuf) =
    let buf = Bytes.Bytebuf.create 100 in 
    let m = GetLexerRange  lexbuf in
    let startp = lexbuf.StartPos in  
    let fin = (fun m2 b s -> 
                     let endp = lexbuf.EndPos in  
                     (* Adjust the start-of-token mark back to the true start of the token *)
                     adjust_lexbuf_start_pos lexbuf startp;
                     if b then
                        if Lexhelp.stringbuf_is_bytes buf then
                            BYTEARRAY (Lexhelp.stringbuf_as_bytes buf)
                        else (
                            fail args lexbuf "This byte array literal contains characters that do not encode as a single byte" ();
                            BYTEARRAY (Lexhelp.stringbuf_as_bytes buf)
                        )
                     else
                        STRING (Bytes.unicode_bytes_as_string s))  in
    buf,fin,m
             
(* Utility functions for processing XML documentation *)

let trySaveXmlDoc lexbuf (buff:option<System.Text.StringBuilder>) =
    match buff with 
    | None -> () 
    | Some(sb) -> LexbufLocalXmlDocStore.SaveXmlDoc lexbuf (sb.ToString(), pos_of_lexpos lexbuf.StartPos)
  
let tryAppendXmlDoc (buff:option<System.Text.StringBuilder>) (s:string) =
    match buff with 
    | None -> ()
    | Some(sb) -> ignore(sb.Append(s))

(* Utilities for parsing #if/#else/#endif *)

let shouldStartLine args lexbuf m err tok = 
    if (start_col_of_range m <> 0) then fail args lexbuf err tok
    else tok
    
let extractIdentFromHashIf (lexed:string) =
    // Skip the '#if' token, then trim whitespace, then find the end of the identifier
    let lexed = lexed.Trim() in     
    let trimIf = lexed.Substring(3).Trim() in
    let identEnd = trimIf.IndexOfAny([| ' '; '\t'; '/' |]) in
    let identEnd = (if identEnd = -1 then trimIf.Length else identEnd) in
    trimIf.Substring(0, identEnd)
} 

let letter = '\Lu' | '\Ll' | '\Lt' | '\Lm' | '\Lo' | '\Nl'
let digit = '\Nd'
let hex = ['0'-'9'] | ['A'-'F'] | ['a'-'f']
let truewhite = [' ']
let offwhite = ['\t']
let anywhite = truewhite | offwhite
let op_char = '!'|'$'|'%'|'&'|'*'|'+'|'-'|'.'|'/'|'<'|'='|'>'|'?'|'@'|'^'|'|'|'~'|':'
let ignored_op_char = '.' | '$'
let xinteger = 
  (  '0' ('x'| 'X')  hex + 
   | '0' ('o'| 'O')  (['0'-'7']) + 
   | '0' ('b'| 'B')  (['0'-'1']) + )
let integer = digit+
let int8 = integer 'y'
let uint8 = (xinteger | integer) 'u' 'y' 
let int16 = integer 's'
let uint16 = (xinteger | integer) 'u' 's'
let int = integer 
let int32 = integer 'l'
let uint32 = (xinteger | integer) 'u' 
let uint32l = (xinteger | integer) 'u' 'l'
let nativeint = (xinteger | integer) 'n'
let unativeint = (xinteger | integer) 'u' 'n'
let int64 = (xinteger | integer) 'L' 
let uint64 = (xinteger | integer) ('u' | 'U') 'L' 
let xint8 = xinteger 'y'
let xint16 = xinteger 's'
let xint = xinteger 
let xint32 = xinteger 'l'
let floatp = digit+ '.' digit*  
let floate = digit+ ('.' digit* )? ('e'| 'E') ['+' '-']? digit+
let float = floatp | floate 
let bignum =  integer ('I'  | 'N' | 'Z' | 'Q' | 'R' | 'G')
let ieee64 = float
(* let ieee64d = (float | integer) ('d' | 'D')  *)
let ieee32 = float ('f' | 'F') 
let decimal = (float | integer) ('m' | 'M') 
let xieee32 = xinteger 'l' 'f'
let xieee64 = xinteger 'L' 'F'
let escape_char = ('\\' ( '\\' | "\"" | '\'' | 'a' | 'f' | 'v' | 'n' | 't' | 'b' | 'r'))
let char = '\'' ( [^'\\''\n''\r''\t''\b'] | escape_char) '\''
let trigraph = '\\' digit digit digit
let hexgraph_short = '\\' 'x' hex hex 
let unicodegraph_short = '\\' 'u' hex hex hex hex
let unicodegraph_long =  '\\' 'U' hex hex hex hex hex hex hex hex
let newline = ('\n' | '\r' '\n')

let connecting_char = '\Pc'
let combining_char = '\Mn' | '\Mc'
let formatting_char = '\Cf' 

let ident_start_char = 
    letter | '_'

let ident_char = 
    letter
  | connecting_char 
  | combining_char 
  | formatting_char 
  | digit 
  | ['\'']
  
let ident = ident_start_char ident_char*

rule token args skip = parse
 | ident 
     { Keywords.KeywordOrIdentifierToken args lexbuf (ulexeme lexbuf) }
 | "do!" 
     { DO_BANG } 
 | "yield!" 
     { YIELD_BANG(true)  } 
 | "return!" 
     { YIELD_BANG(false) } 
 | ident '!' 
     { let tok = Keywords.KeywordOrIdentifierToken args lexbuf (trim_right lexbuf 1)  in
       match tok with 
       | LET _ -> BINDER (trim_right lexbuf 1) 
       | _ -> fail args lexbuf "Identifiers followed by '!' are reserved for future use" (Keywords.KeywordOrIdentifierToken args lexbuf (ulexeme lexbuf)) } 
 | ident ('#')  
     { fail args lexbuf "Identifiers followed by '?' or '#' are reserved for future use" (Keywords.KeywordOrIdentifierToken args lexbuf (ulexeme lexbuf)) }
 | int8 
     { let n = trim_to_i32 args lexbuf 1 in 
       if n > 0x80 or n < -0x80 then fail args lexbuf "This number is outside the allowable range for 8-bit signed integers" (INT8(0y,false))
    (* Allow <max_int+1> to parse as min_int.  Allowed only because we parse '-' as an operator. *)
       else if n = 0x80 then INT8(sbyte(-0x80), true (* 'true' = 'bad'*) )
       else INT8(sbyte n,false) }
 | xint8 
     { let n = trim_to_i32 args lexbuf 1 in 
       if n > 0xFF or n < 0 then fail args lexbuf "This number is outside the allowable range for hexadecimal 8-bit signed integers" (INT8(0y,false))
       else INT8(sbyte(byte(n)),false) }
 | uint8
     { let n = trim_to_i32 args lexbuf 2 in 
       if n > 0xFF or n < 0 then fail args lexbuf "This number is outside the allowable range for 8-bit unsigned integers" (UINT8(0uy))
       else UINT8(byte n) }
 | int16 
     { let n = trim_to_i32 args lexbuf 1 in 
       if n > 0x8000 or n < -0x8000 then fail args lexbuf "This number is outside the allowable range for 16-bit signed integers" (INT16(0s,false))
    (* Allow <max_int+1> to parse as min_int.  Allowed only because we parse '-' as an operator. *)
       else if n = 0x8000 then INT16(-0x8000s,true)
       else INT16(int16 n,false) }
 | xint16 
     { let n = trim_to_i32 args lexbuf 1 in 
       if n > 0xFFFF or n < 0 then fail args lexbuf "This number is outside the allowable range for 16-bit signed integers" (INT16(0s,false))
       else INT16(int16(uint16(n)),false) }
 | uint16 
     { let n = trim_to_i32 args lexbuf 2 in 
       if n > 0xFFFF or n < 0 then fail args lexbuf "This number is outside the allowable range for 16-bit unsigned integers" (UINT16(0us))
       else UINT16(uint16 n) }
 | int '.' '.' 
     { let s = trim_right lexbuf 2 in 
       (* Allow <max_int+1> to parse as min_int.  Allowed only because we parse '-' as an operator. *)
       if s = "2147483648" then INT32_DOT_DOT(-2147483648,true) else
       let n = try int32 s with _ ->  fail args lexbuf "This number is outside the allowable range for 32-bit signed integers" 0 in
       INT32_DOT_DOT(n,false)
     } 
 | xint 
 | int 
     { let s = ulexeme lexbuf in 
       (* Allow <max_int+1> to parse as min_int.  Allowed only because we parse '-' as an operator. *)
       if s = "2147483648" then INT32(-2147483648,true) else
       let n =
           try int32 s with _ ->  fail args lexbuf "This number is outside the allowable range for 32-bit signed integers" 0
       in 
       INT32(n,false)
     } 
 | xint32 
 | int32 
     { let s = trim_right lexbuf 1 in 
       (* Allow <max_int+1> to parse as min_int.  Allowed only because we parse '-' as an operator. *)
       if s = "2147483648" then INT32(-2147483648,true) else
       let n = 
           try int32 s with _ ->  fail args lexbuf "This number is outside the allowable range for 32-bit signed integers" 0
       in
       INT32(n,false)
     } 

 | uint32
     { 
       let s = trim_right lexbuf 1 in 
       let n = 
           try int64 s with _ ->  fail args lexbuf "This number is outside the allowable range for 32-bit unsigned integers" 0L
       in
       if n > 0xFFFFFFFFL or n < 0L then fail args lexbuf "This number is outside the allowable range for 32-bit unsigned integers" (UINT32(0u)) else
       UINT32(uint32 (uint64 n)) } 

 | uint32l
     { 
       let s = trim_right lexbuf 2 in 
       let n = 
           try int64 s with _ ->  fail args lexbuf "This number is outside the allowable range for 32-bit unsigned integers" 0L
       in
       if n > 0xFFFFFFFFL or n < 0L then fail args lexbuf "This number is outside the allowable range for 32-bit unsigned integers" (UINT32(0u)) else
       UINT32(uint32 (uint64 n)) } 

 | int64 
     { let s = trim_right lexbuf 1 in 
       (* Allow <max_int+1> to parse as min_int.  Stupid but allowed because we parse '-' as an operator. *)
       if s = "9223372036854775808" then INT64(-9223372036854775808L,true) else
       let n = 
          try int64 s with _ ->  fail args lexbuf "This number is outside the allowable range for 64-bit signed integers" 0L
       in 
       INT64(n,false)
     }

 | uint64     
     { let s = trim_right lexbuf 2 in 
       let n = 
         try uint64 s with _ -> fail args lexbuf "This number is outside the allowable range for 64-bit unsigned integers" 0UL
       in 
       UINT64(n) } 

 | nativeint  
     { try 
           NATIVEINT(int64 (trim_right lexbuf 1)) 
       with _ ->  fail args lexbuf "This number is outside the allowable range for native integers" (NATIVEINT(0L)) } 

 | unativeint 
     { try 
           UNATIVEINT(uint64 (trim_right lexbuf 2)) 
       with _ ->  fail args lexbuf "This number is outside the allowable range for unsigned native integers"  (UNATIVEINT(0UL)) }

 | ieee32     
     { IEEE32 (try float32(trim_right lexbuf 1) with _ -> fail args lexbuf "Invalid floating point number" 0.0f) }
 | ieee64     
     { IEEE64 (try float(ulexeme lexbuf) with _ -> fail args lexbuf "Invalid floating point number" 0.0) }

(*  | ieee64d    { IEEE64 (float (trim_right lexbuf 1)) } *)
 | decimal    
     { try 
          let s = trim_right lexbuf 1 in
          (* This implements a range check for decimal literals *)
          let d = System.Decimal.Parse(s,System.Globalization.NumberStyles.AllowExponent ||| System.Globalization.NumberStyles.Number,System.Globalization.CultureInfo.InvariantCulture) in
          DECIMAL d 
       with 
          e -> fail args lexbuf "This number is outside the allowable range for decimal literals" (DECIMAL (decimal 0))
     }
 | xieee32     
     { 
       let s = trim_right lexbuf 2 in
       let n64 = int64 s in 
       if n64 > 0xFFFFFFFFL or n64 < 0L then fail args lexbuf "This number is outside the allowable range for 32-bit floats" (IEEE32 0.0f) else
       IEEE32 (System.BitConverter.ToSingle(System.BitConverter.GetBytes(int32 (uint32 (uint64 n64))),0)) }

 | xieee64     
     { 
       let n64 = int64 (trim_right lexbuf 2) in 
       IEEE64 (System.BitConverter.Int64BitsToDouble(n64)) }
       
 | bignum     
       { let s = ulexeme lexbuf in 
         BIGNUM (trim_right lexbuf 1, s.[s.Length-1..s.Length-1]) }

 | (int | xint | float) ident_char+
       { fail args lexbuf "This is not a valid numeric literal. Sample formats include 4, 0x4, 0b0100, 4L, 4UL, 4u, 4s, 4us, 4y, 4uy, 4.0, 4.0f, 4I" (INT32(0,false)) }
 
 | char
     { let s = ulexeme lexbuf in 
       CHAR (if s.[1] = '\\' then escape s.[2] else s.[1])  }

 | char 'B' 
     { let s = ulexeme lexbuf in 
       let x = int32 (if s.[1] = '\\' then escape s.[2] else s.[1]) in
       if x < 0 || x > 127 then 
           fail args lexbuf "This is not a valid byte literal" (UINT8(byte 0))
       else
           UINT8 (byte(x))  }
     
 | '\'' trigraph '\''
     { let s = ulexeme lexbuf in 
       let c = trigraph s.[2] s.[3] s.[4] in 
       let x = int32 c in
       if x < 0 || x > 255 then 
           fail args lexbuf "This is not a valid character literal" (CHAR c)
       else
           CHAR c }

 | '\'' trigraph '\'' 'B'
     { let s = ulexeme lexbuf in 
       let x = int32 (trigraph s.[2] s.[3] s.[4]) in
       if x < 0 || x > 255 then 
           fail args lexbuf "This is not a valid byte literal" (UINT8(byte 0))
       else
           UINT8 (byte(x))  }

 | '\'' unicodegraph_short '\''  'B'
     { let x = int32 (unicodegraph_short (trim_both lexbuf 3 2)) in
       if x < 0 || x > 127 then 
           fail args lexbuf "This is not a valid byte literal" (UINT8(byte 0))
       else
           UINT8 (byte(x))  }
     
 | '\'' hexgraph_short '\'' { CHAR (char (int32 (hexgraph_short (trim_both lexbuf 3 1)))) }
 | '\'' unicodegraph_short '\'' { CHAR (char (int32 (unicodegraph_short (trim_both lexbuf 3 1)))) }
 | '\'' unicodegraph_long '\''  
     { let hi,lo = unicodegraph_long (trim_both lexbuf 3 1) in 
       match hi with 
       | None -> CHAR (char lo)
       | Some _ -> fail args lexbuf  "This unicode encoding is only valid in string literals" (CHAR (char lo)) }
 | "(*IF-FSHARP"    { skipToken skip (COMMENT (AT_token !args.ifdefStack)) (token args) lexbuf }
 | "(*F#"           { skipToken skip (COMMENT (AT_token !args.ifdefStack)) (token args) lexbuf }
 | "ENDIF-FSHARP*)" { skipToken skip (COMMENT (AT_token !args.ifdefStack)) (token args) lexbuf  }
 | "F#*)"           { skipToken skip (COMMENT (AT_token !args.ifdefStack)) (token args) lexbuf }

 | "(*)"            { LPAREN_STAR_RPAREN }

 | "(*"
     { let m = GetLexerRange  lexbuf in 
       skipToken skip (COMMENT (AT_comment(!args.ifdefStack,1,m))) (comment(1,m,args)) lexbuf }

 | "(*IF-CAML*)" |  "(*IF-OCAML*)" 
     { let m = GetLexerRange  lexbuf in 
       skipToken skip (COMMENT (AT_camlonly(!args.ifdefStack,m))) (camlonly m args) lexbuf }

 | '"' 
     { let buf,fin,m = start_string args lexbuf in 
       skipToken skip (STRING_TEXT (AT_string(!args.ifdefStack,m))) (string (buf,fin,m,args)) lexbuf }
      
 | '@' '"' 
     { let buf,fin,m = start_string args lexbuf in 
       skipToken skip (STRING_TEXT (AT_vstring(!args.ifdefStack,m))) (vstring (buf,fin,m,args)) lexbuf }

 | truewhite+  
     { if skip then token args skip lexbuf
       else WHITESPACE (AT_token !args.ifdefStack) }

 | offwhite+  
     { if args.lightSyntaxStatus.Status then errorR(Error("TABs are not allowed in #light code",GetLexerRange lexbuf));
       skipToken skip (WHITESPACE (AT_token !args.ifdefStack)) (token args) lexbuf }

 | "////" op_char* 
     { (* 4+ slash are 1-line comments, online 3 slash are XmlDoc *)
       let m = GetLexerRange lexbuf in 
       skipToken skip (LINE_COMMENT (AT_tokenized_comment(!args.ifdefStack,1,m))) (tokenized_comment(None,1,m,args)) lexbuf }

 | "///" op_char*
     { (* Match exactly 3 slash, 4+ slash caught by preceding rule *)
       let m = GetLexerRange lexbuf in 
       let doc = trim_left lexbuf 3 in 
       let sb = (new StringBuilder(100)).Append(doc) in
       skipToken skip (LINE_COMMENT (AT_tokenized_comment(!args.ifdefStack,1,m))) (tokenized_comment(Some(sb),1,m,args)) lexbuf }

 | "//" op_char*
     { (* Need to read all operator symbols too, otherwise it might be parsed by a rule below *)
       let m = GetLexerRange lexbuf in 
       skipToken skip (LINE_COMMENT (AT_tokenized_comment(!args.ifdefStack,1,m))) (tokenized_comment(None,1,m,args)) lexbuf }

 | newline 
     { newline lexbuf; skipToken skip (WHITESPACE (AT_token !args.ifdefStack)) (token args) lexbuf }

 | '`' '`' ([^'`' '\n' '\r' '\t'] | '`' [^'`''\n' '\r' '\t']) + '`' '`' 
     { Keywords.IdentifierToken args lexbuf (trim_both lexbuf 2 2) }

 | ('#' anywhite* | "#line" anywhite+ ) digit+ anywhite* ('@'? "\"" [^'\n''\r''"']+ '"')? anywhite* newline
     {  let pos = lexbuf.EndPos in 
        let lnum = pos.Line in 
        if skip then 
          let s = ulexeme lexbuf in 
          let rec parseLeadingDirective n = 
            match s.[n] with 
            | c when c >= 'a' && c <= 'z' -> parseLeadingDirective (n+1) 
            | _ -> parseLeadingWhitespace n // goto the next state
          
          and parseLeadingWhitespace n = 
            match s.[n] with 
            | ' ' | '\t' -> parseLeadingWhitespace (n+1) 
            | _ -> parseLineNumber n n // goto the next state
          
          and parseLineNumber start n = 
            match s.[n] with 
            | c when c >= '0' && c <= '9' -> parseLineNumber start (n+1)
            | _ -> let text =  (String.sub s start (n-start)) in 
                   let lineNumber = 
                       try int32 text
                       with err -> errorR(Error("invalid line number: '"^text^"'",GetLexerRange lexbuf)); 0 in
                   lineNumber, parseWhitespaceBeforeFile n // goto the next state
          
          and parseWhitespaceBeforeFile n =  
            match s.[n] with 
            | ' ' | '\t' | '@' -> parseWhitespaceBeforeFile (n+1) 
            | '"' -> Some (parseFile (n+1) (n+1))
            | _ -> None
          
          and parseFile start n =   
            match s.[n] with 
            | '"' -> String.sub s start (n-start)  
            | _ -> parseFile start (n+1) in 

          // Call the parser
          let line,file = parseLeadingDirective 1 in 

          // Construct the new position
          set_pos lexbuf {pos with
                pos_fname = (match file with Some f -> encode_file f | None -> pos.pos_fname); 
                pos_bol= pos.pos_cnum;
                pos_lnum=line };
          token args skip lexbuf 
        else 
          skipToken skip (HASH_LINE (AT_token !args.ifdefStack)) (token args) lexbuf }
            
 | "<@" op_char* { CheckExprOp lexbuf; let s = trim_left lexbuf 2 in LQUOTE (Printf.sprintf "<@%s %s@>" s (rev_string s), quote_op_is_raw s) }
 | op_char* "@>" { CheckExprOp lexbuf; let s = trim_right lexbuf 2 in RQUOTE (Printf.sprintf "<@%s %s@>" (rev_string s) s, quote_op_is_raw (rev_string s)) }
 | '#' { HASH }
 | '&' { AMP }
 | "&&" { AMP_AMP }
 | "||" { BAR_BAR }
 | '\'' { QUOTE }
 | '(' { LPAREN }
 | ')' { RPAREN }
 | '*' { STAR }
 | ',' { COMMA }
 | "->" { RARROW }
 | "->>" { RARROW2 }
 | "?" { QMARK }
 | "??" { QMARK_QMARK }
 | ".." { DOT_DOT }
 | "." { DOT }
 | ":" { COLON }
 | "::" { COLON_COLON }
 | ":>" { COLON_GREATER }
 | ">." { GREATER_DOT }
 | "@>." { RQUOTE_DOT ("<@ @>",false) }
 | "@@>." { RQUOTE_DOT ("<@@ @@>",true) }
 | ">|]" { GREATER_BAR_RBRACK }
 | ":?>" { COLON_QMARK_GREATER }
 | ":?" { COLON_QMARK }
 | ":=" { COLON_EQUALS }
 | ";;" { SEMICOLON_SEMICOLON }
 | ";" { SEMICOLON }
 | "<-" { LARROW }
 | "=" { EQUALS }
 | "[" { LBRACK }
 | "[|" { LBRACK_BAR }
 | "<" { LESS }
 | ">" { GREATER }
 | "[<" { LBRACK_LESS }
 | "]" { RBRACK }
 | "|]" { BAR_RBRACK }
 | ">]" { GREATER_RBRACK }
 | "{" { LBRACE }
 | "|" { BAR }
 | "}" { RBRACE }
 | "$" { DOLLAR }
 | "%" { PERCENT_OP("%") }
 | "%%" { PERCENT_OP("%%") }
 | "-" { MINUS }
 | "~" { RESERVED }
 | "`" { RESERVED }
 | ignored_op_char* '*' '*'                    op_char* { CheckExprOp lexbuf; INFIX_STAR_STAR_OP(ulexeme lexbuf) }
 | ignored_op_char* ('*' | '/'|'%')            op_char* { CheckExprOp lexbuf; INFIX_STAR_DIV_MOD_OP(ulexeme lexbuf) }
 | ignored_op_char* ('+'|'-')                  op_char* { CheckExprOp lexbuf; PLUS_MINUS_OP(ulexeme lexbuf) }
 | ignored_op_char* ('@'|'^')                  op_char* { CheckExprOp lexbuf; INFIX_AT_HAT_OP(ulexeme lexbuf) }
 | ignored_op_char* ('=' | "!=" | '<' | '>' | '$')  op_char* { CheckExprOp lexbuf; INFIX_COMPARE_OP(ulexeme lexbuf) }
 | ignored_op_char* ('&')                      op_char* { CheckExprOp lexbuf; INFIX_AMP_OP(ulexeme lexbuf) }
 | ignored_op_char* '|'                        op_char* { CheckExprOp lexbuf; INFIX_BAR_OP(ulexeme lexbuf) }
 | ignored_op_char* ('!' | '?' | '~' )         op_char* { CheckExprOp lexbuf; PREFIX_OP(ulexeme lexbuf) }
 | ".[]"  | ".[]<-" | ".[,]<-" | ".[,,]<-" | ".[,,,]<-" | ".[,,,]" | ".[,,]" | ".[,]" | ".[..]" | ".[..,..]" | ".[..,..,..]" | ".[..,..,..,..]"
 | ".()"  | ".()<-"  { FUNKY_OPERATOR_NAME(ulexeme lexbuf) }

 | "#light" anywhite* 
   { if args.lightSyntaxStatus.ExplicitlySet && args.lightSyntaxStatus.WarnOnMultipleTokens then 
         warning(Error("#light should only occur as the first non-comment text in an F# source file",GetLexerRange lexbuf));
     args.lightSyntaxStatus.Status <- true; 
     skipToken skip (HASH_LIGHT (AT_token !args.ifdefStack)) (token args) lexbuf } 

 | ("#indent" | "#light") anywhite+ "\"off\"" 
   { args.lightSyntaxStatus.Status <- false; 
     skipToken skip (HASH_LIGHT (AT_token !args.ifdefStack)) (token args) lexbuf } 
   
 | ("#indent" | "#light") anywhite+ "\"on\""
   { args.lightSyntaxStatus.Status <- true; 
     skipToken skip (HASH_LIGHT (AT_token !args.ifdefStack)) (token args) lexbuf } 
   
 | anywhite* "#if" anywhite+ ident anywhite* ("//" [^'\n''\r']*)?
   { let m = GetLexerRange lexbuf in    
     let lexed = (ulexeme lexbuf) in
     let id = extractIdentFromHashIf lexed in
     args.ifdefStack := (IfDefIf,m) :: !(args.ifdefStack);
     
     // Get the token; make sure it starts at zero position & return
     let cont, f = 
       ( if List.mem id args.defines then (AT_endline(ENDL_token(!args.ifdefStack)), endline (ENDL_token !args.ifdefStack) args)
         else (AT_endline(ENDL_skip(!args.ifdefStack,0,m)), endline (ENDL_skip(!args.ifdefStack,0,m)) args)  ) in         
     let tok = shouldStartLine args lexbuf m "#if directive must appear as the first non-whitespace character on a line" (HASH_IF(m,lexed,cont)) in
     skipToken skip tok f lexbuf }

 | anywhite* "#else" anywhite* ("//" [^'\n''\r']*)?
   { let lexed = (ulexeme lexbuf) in
     let m = GetLexerRange lexbuf in 
     match !(args.ifdefStack) with
     | [] ->  LEX_FAILURE "#else has no matching #if" 
     | (IfDefElse,_) :: rest -> LEX_FAILURE "#endif required for #else" 
     | (IfDefIf,_) :: rest -> 
       let m = GetLexerRange  lexbuf in 
       args.ifdefStack := (IfDefElse,m) :: rest;
       let tok = HASH_ELSE(m,lexed, AT_endline(ENDL_skip(!args.ifdefStack,0,m))) in
       let tok = shouldStartLine args lexbuf m "#else directive must appear as the first non-whitespace character on a line" tok in
       skipToken skip tok (endline (ENDL_skip(!args.ifdefStack,0,m)) args) lexbuf }

 | anywhite* "#endif" anywhite* ("//" [^'\n''\r']*)?
   { let lexed = (ulexeme lexbuf) in
     let m = GetLexerRange lexbuf in 
     match !(args.ifdefStack) with
     | []->  LEX_FAILURE "#endif has no matching #if" 
     | _ :: rest ->  
        args.ifdefStack := rest;          
        let tok = HASH_ENDIF(m,lexed,AT_endline(ENDL_token(!args.ifdefStack))) in
        let tok = shouldStartLine args lexbuf m "#endif directive must appear as the first non-whitespace character on a line" tok in
        skipToken skip tok (endline (ENDL_token(!args.ifdefStack)) args) lexbuf }

 | "#if" 
   { let tok = fail args lexbuf "#if directive should be immediately followed by an identifier" (WHITESPACE (AT_token !args.ifdefStack)) in
     skipToken skip tok (token args) lexbuf }

 | _ 
   { unexpected_char lexbuf }     
 | eof 
   { EOF (AT_token !args.ifdefStack) }

(* Skips INACTIVE code until if finds #else / #endif matching with the #if or #else *)

and ifdef_skip n m args skip = parse               
 | anywhite* "#if" anywhite+ ident anywhite* ("//" [^'\n''\r']*)?
   { let m = GetLexerRange lexbuf in    
     let id = extractIdentFromHashIf (ulexeme lexbuf) in
     
     // If #if is the first thing on the line then increase depth, otherwise skip, because it is invalid (e.g. "(**) #if ...")
     if (start_col_of_range m <> 0) then
       skipToken skip (INACTIVECODE (AT_ifdef_skip(!args.ifdefStack,n,m))) (ifdef_skip n m args) lexbuf
     else
       let tok = INACTIVECODE(AT_endline(ENDL_skip(!args.ifdefStack,n+1,m))) in
       skipToken skip tok (endline (ENDL_skip(!args.ifdefStack,n+1,m)) args) lexbuf }

  | anywhite* "#else" anywhite* ("//" [^'\n''\r']*)?
    { let lexed = (ulexeme lexbuf) in
      let m = GetLexerRange  lexbuf in 
           
      // If #else is the first thing on the line then process it, otherwise ignore, because it is invalid (e.g. "(**) #else ...")
      if (start_col_of_range m <> 0) then
        skipToken skip (INACTIVECODE (AT_ifdef_skip(!args.ifdefStack,n,m))) (ifdef_skip n m args) lexbuf
      elif n = 0 then 
         match !(args.ifdefStack) with
         | []->  LEX_FAILURE "#else has no matching #if" 
         | (IfDefElse,_) :: rest -> LEX_FAILURE "#endif required for #else" 
         | (IfDefIf,_) :: rest -> 
           let m = GetLexerRange  lexbuf in 
           args.ifdefStack := (IfDefElse,m) :: rest;
           skipToken skip (HASH_ELSE(m,lexed,AT_endline(ENDL_token(!args.ifdefStack)))) (endline (ENDL_token(!args.ifdefStack)) args) lexbuf 
       else
         skipToken skip (INACTIVECODE(AT_endline(ENDL_skip(!args.ifdefStack,n,m)))) (endline (ENDL_skip(!args.ifdefStack,n,m)) args) lexbuf }
          
  | anywhite* "#endif" anywhite* ("//" [^'\n''\r']*)?
    { let lexed = (ulexeme lexbuf) in
      let m = GetLexerRange  lexbuf in 
      
      // If #endif is the first thing on the line then process it, otherwise ignore, because it is invalid (e.g. "(**) #endif ...")
      if (start_col_of_range m <> 0) then
          skipToken skip (INACTIVECODE (AT_ifdef_skip(!args.ifdefStack,n,m))) (ifdef_skip n m args) lexbuf
      elif n = 0 then 
          match !(args.ifdefStack) with
          | [] ->  LEX_FAILURE "#endif has no matching #if" 
          | _ :: rest -> 
              args.ifdefStack := rest;
              skipToken skip (HASH_ENDIF(m,lexed,AT_endline(ENDL_token(!args.ifdefStack)))) (endline (ENDL_token(!args.ifdefStack)) args) lexbuf 
       else
           let tok = INACTIVECODE(AT_endline(ENDL_skip(!args.ifdefStack,n-1,m))) in
           let tok = shouldStartLine args lexbuf m "Syntax error. wrong nested #endif, unexpected tokens before it" tok in
           skipToken skip tok (endline (ENDL_skip(!args.ifdefStack,(n-1),m)) args) lexbuf }
           
  | newline 
    { newline lexbuf; ifdef_skip n m args skip lexbuf }
    
  | [^ ' ' '\n' '\r' ]+
  | anywhite+
  | _    
    { // This tries to be nice and get tokens as 'words' because VS uses this when selecting stuff
      skipToken skip (INACTIVECODE (AT_ifdef_skip(!args.ifdefStack,n,m))) (ifdef_skip n m args) lexbuf }
  | eof  
    { EOF (AT_ifdef_skip(!args.ifdefStack,n,m)) }

(* Called after lexing #if IDENT/#else/#endif - this checks whether there is nothing except end of line *)
(* or end of file and then calls the lexing function specified by 'cont' - either token or ifdef_skip *)
and endline cont args skip = parse
 | newline
   { newline lexbuf; 
     match cont with
     | ENDL_token(ifdefStack) -> skipToken skip (WHITESPACE(AT_token ifdefStack)) (token args) lexbuf
     | ENDL_skip(ifdefStack, n, m) -> skipToken skip (INACTIVECODE (AT_ifdef_skip(ifdefStack,n,m))) (ifdef_skip n m args) lexbuf
   }
 | eof
   { match cont with
     | ENDL_token(ifdefStack) -> (EOF(AT_token ifdefStack))
     | ENDL_skip(ifdefStack, n, m) -> (EOF(AT_ifdef_skip(ifdefStack,n,m)))
   }
 | [^'\r' '\n']+
 | _
   { let tok = fail args lexbuf "Expected single line comment or end of line" (WHITESPACE (AT_token !args.ifdefStack)) in
     skipToken skip tok (token args) lexbuf }     

(* NOTE : OCaml doesn't take tailcalls for functions > ~5 arguments.  Sheesh. *)
(* Hence we have to wrap up arguments for deeply nested *)
(* recursive call targets such as this one *)
and string sargs skip = parse
 |  '\\' newline anywhite* 
    { let (buf,fin,m,args) = sargs in 
      newline lexbuf; 
      skipToken skip (STRING_TEXT (AT_string(!args.ifdefStack,m)))  (string sargs) lexbuf }

 |  escape_char
    { let (buf,fin,m,args) = sargs in 
      add_byte_char buf (escape (lexeme lexbuf).[1]);
      skipToken skip (STRING_TEXT (AT_string(!args.ifdefStack,m)))  (string sargs) lexbuf } 

 | trigraph
    (* REVIEW: Disallow these in string sargs constants, at least if > 127, since then *)
    (* they have no established meaning *)
    { let (buf,fin,m,args) = sargs in 
      let s = ulexeme lexbuf in 
      add_byte_char buf (trigraph s.[1] s.[2] s.[3]);
      skipToken skip (STRING_TEXT (AT_string(!args.ifdefStack,m)))  (string sargs) lexbuf }

 | hexgraph_short
    { let (buf,fin,m,args) = sargs in 
      add_unichar buf (int (hexgraph_short (trim_left lexbuf 2)));
      skipToken skip (STRING_TEXT (AT_string(!args.ifdefStack,m)))  (string sargs) lexbuf  }
      
 | unicodegraph_short
    (* REVIEW: Disallow these in bytearray constants *)
    { let (buf,fin,m,args) = sargs in 
      add_unichar buf (int (unicodegraph_short (trim_left lexbuf 2)));
      skipToken skip (STRING_TEXT (AT_string(!args.ifdefStack,m)))  (string sargs) lexbuf  }
     
 | unicodegraph_long
    { let (buf,fin,m,args) = sargs in 
      let hi,lo = unicodegraph_long (trim_left lexbuf 2) in 
      (match hi with | None -> () | Some c -> add_unichar buf (int c));
      add_unichar buf (int lo);
      skipToken skip (STRING_TEXT (AT_string(!args.ifdefStack,m))) (string sargs) lexbuf  }
     
 |  '"' 
    { let (buf,fin,m,args) = sargs in 
      let m2 = GetLexerRange lexbuf in 
      call_string_finish fin buf m2 false }

 |  '"''B' 
    { let (buf,fin,m,args) = sargs in 
      let m2 = GetLexerRange lexbuf in 
      call_string_finish fin buf m2 true }

 | newline
    { let (buf,fin,m,args) = sargs in 
      newline lexbuf; 
      add_string buf (ulexeme lexbuf); 
      skipToken skip (STRING_TEXT (AT_string(!args.ifdefStack,m)))  (string sargs) lexbuf }

 | ident  
    { let (buf,fin,m,args) = sargs in 
      add_string buf (ulexeme lexbuf); 
      skipToken skip (STRING_TEXT (AT_string(!args.ifdefStack,m)))  (string sargs) lexbuf }

 | integer 
 | xinteger
    { let (buf,fin,m,args) = sargs in 
      add_string buf (ulexeme lexbuf); 
      skipToken skip (STRING_TEXT (AT_string(!args.ifdefStack,m)))  (string sargs) lexbuf }

 | anywhite +  
    { let (buf,fin,m,args) = sargs in 
      add_string buf (ulexeme lexbuf); 
      skipToken skip (STRING_TEXT (AT_string(!args.ifdefStack,m)))  (string sargs) lexbuf }

 | eof  
    { let (buf,fin,m,args) = sargs in 
      EOF (AT_string(!args.ifdefStack,m)) }
 | _ 
    { let (buf,fin,m,args) = sargs in 
      add_string buf (lexeme lexbuf); 
      skipToken skip (STRING_TEXT (AT_string(!args.ifdefStack,m))) (string sargs) lexbuf }

(* REVIEW: consider sharing this code with the 'string' lexer state *)
and vstring sargs skip = parse
 |  '"' '"'
   { let (buf,fin,m,args) = sargs in 
     add_byte_char buf '\"';
     skipToken skip (STRING_TEXT (AT_vstring(!args.ifdefStack,m)))  (vstring sargs) lexbuf } 

 |  '"' 
    { let (buf,fin,m,args) = sargs in 
      let m2 = GetLexerRange lexbuf in 
      call_string_finish fin buf m2 false }

 |  '"''B' 
    { let (buf,fin,m,args) = sargs in 
      let m2 = GetLexerRange lexbuf in 
      call_string_finish fin buf m2 true }

 | newline 
    { let (buf,fin,m,args) = sargs in 
      newline lexbuf; 
      add_string buf (ulexeme lexbuf); 
      skipToken skip (STRING_TEXT (AT_vstring(!args.ifdefStack,m)))  (vstring sargs) lexbuf }

 | ident  
    { let (buf,fin,m,args) = sargs in 
      add_string buf (ulexeme lexbuf); 
      skipToken skip (STRING_TEXT (AT_vstring(!args.ifdefStack,m)))  (vstring sargs) lexbuf }

 | integer 
 | xinteger
    { let (buf,fin,m,args) = sargs in 
      add_string buf (ulexeme lexbuf); 
      skipToken skip (STRING_TEXT (AT_vstring(!args.ifdefStack,m)))  (vstring sargs) lexbuf }

 | anywhite +  
    { let (buf,fin,m,args) = sargs in 
      add_string buf (ulexeme lexbuf); 
      skipToken skip (STRING_TEXT (AT_vstring(!args.ifdefStack,m)))  (vstring sargs) lexbuf }

 | eof 
    { let (buf,fin,m,args) = sargs in 
      EOF (AT_vstring(!args.ifdefStack,m)) }
 | _ 
    { let (buf,fin,m,args) = sargs in 
      add_string buf (lexeme lexbuf); 
      skipToken skip (STRING_TEXT (AT_vstring(!args.ifdefStack,m))) (vstring sargs) lexbuf }

(* Parsing single-line comment - we need to split it into words for Visual Studio IDE *)
and tokenized_comment cargs skip = parse 
 | newline
     { let buff,n,m,args = cargs in 
       trySaveXmlDoc lexbuf buff;
       newline lexbuf; 
       (* saves the documentation (if we're collecting any) into a buffer-local variable *)
       skipToken skip (LINE_COMMENT (AT_token !args.ifdefStack)) (token args) lexbuf }
 
 | eof 
     { let _, n,m,args = cargs in 
       EOF (AT_token !args.ifdefStack) } (* NOTE: it is legal to end a file with this comment, so we'll return EOF as a token *)
      
 | [^ ' ' '\n' '\r' ]+
 | anywhite+
     { let buff,n,m,args = cargs in 
       (* Append the current token to the XML documentation if we're collecting it *)
       tryAppendXmlDoc buff (ulexeme lexbuf);
       skipToken skip (LINE_COMMENT (AT_tokenized_comment(!args.ifdefStack,n,m))) (tokenized_comment(buff,n,m,args)) lexbuf  } 
       
 | _ { let _, n,m,args = cargs in 
      skipToken skip (LINE_COMMENT (AT_token !args.ifdefStack)) (token args) lexbuf }

       
(* WARNING: taking sargs as a single parameter seems to make a difference as to whether *)
(* OCaml takes tailcalls. *)
and comment cargs skip = parse
 |  char
    { let n,m,args = cargs in 
      skipToken skip (COMMENT (AT_comment(!args.ifdefStack,n,m))) (comment(n,m,args)) lexbuf  } 
    
 | '"'   
    { let n,m,args = cargs in 
      skipToken skip (COMMENT (AT_comment_string(!args.ifdefStack,n,m))) (comment_string n m args) lexbuf }

 | '@' '"'
    { let n,m,args = cargs in 
      skipToken skip (COMMENT (AT_comment_vstring(!args.ifdefStack,n,m))) (comment_vstring n m args) lexbuf }

 | '(' '*'
    { let n,m,args = cargs in 
      skipToken skip (COMMENT (AT_comment(!args.ifdefStack,n+1,m))) (comment (n+1,m,args)) lexbuf }
     
 | newline
    { let n,m,args = cargs in 
      newline lexbuf; 
      skipToken skip (COMMENT (AT_comment(!args.ifdefStack,n,m))) (comment cargs) lexbuf }
 | "*)" 
    { 
      let n,m,args = cargs in 
      if n > 1 then skipToken skip (COMMENT (AT_comment(!args.ifdefStack,n-1,m))) (comment (n-1,m,args)) lexbuf 
      else skipToken skip (COMMENT (AT_token !args.ifdefStack)) (token args) lexbuf }
      
 | anywhite+
 | [^ '\'' '(' '*' '\n' '\r' '"' ')' '@' ' ' '\t' ]+  
    { 
      let n,m,args = cargs in 
      skipToken skip (COMMENT (AT_comment(!args.ifdefStack,n,m))) (comment cargs) lexbuf }
    
 | eof 
     { let n,m,args = cargs in 
       EOF (AT_comment(!args.ifdefStack,n,m)) }
     
 | _ { let n,m,args = cargs in 
       skipToken skip (COMMENT (AT_comment(!args.ifdefStack,n,m))) (comment(n,m,args)) lexbuf }

and comment_string n m args skip = parse
 (* Follow string lexing, skipping tokens until it finishes *)
 |  '\\' newline anywhite* 
     { newline lexbuf; 
       skipToken skip (COMMENT (AT_comment_string(!args.ifdefStack,n,m))) (comment_string n m args) lexbuf }

 | escape_char
 | trigraph
 | hexgraph_short
 | unicodegraph_short
 | unicodegraph_long
 | ident  
 | integer
 | xinteger
 | anywhite +  
     { skipToken skip (COMMENT (AT_comment_string(!args.ifdefStack,n,m))) (comment_string n m args) lexbuf }


 | '"' 
     { skipToken skip (COMMENT (AT_comment(!args.ifdefStack,n,m))) (comment(n,m,args)) lexbuf }
     
 | newline 
     { newline lexbuf;  
       skipToken skip (COMMENT (AT_comment_string(!args.ifdefStack,n,m))) (comment_string n m args) lexbuf }
     
 | eof 
     { EOF (AT_comment_string(!args.ifdefStack,n,m)) }
     
 | _  
     { skipToken skip (COMMENT (AT_comment_string(!args.ifdefStack,n,m))) (comment_string n m args) lexbuf }

and comment_vstring n m args skip = parse
 (* Follow vstring lexing, in short, skip double-quotes and other chars until we hit a single quote *)
 | '"' '"'
     { skipToken skip (COMMENT (AT_comment_vstring(!args.ifdefStack,n,m))) (comment_vstring n m args) lexbuf }

 | '"' 
     { skipToken skip (COMMENT (AT_comment(!args.ifdefStack,n,m))) (comment(n,m,args)) lexbuf }

 | ident  
 | integer 
 | xinteger
 | anywhite +  
     { skipToken skip (COMMENT (AT_comment_vstring(!args.ifdefStack,n,m))) (comment_vstring n m args) lexbuf }
     
 | newline 
     { newline lexbuf;
       skipToken skip (COMMENT (AT_comment_vstring(!args.ifdefStack,n,m))) (comment_vstring n m args) lexbuf }
     
 | eof 
     { EOF (AT_comment_vstring(!args.ifdefStack,n,m)) }
     
 | _  
     { skipToken skip (COMMENT (AT_comment_vstring(!args.ifdefStack,n,m))) (comment_vstring n m args) lexbuf }
     
and camlonly m args skip = parse
 | "\""
     { let buf = Bytes.Bytebuf.create 100 in 
       let m2 = GetLexerRange  lexbuf in 
       let _ = string (buf,default_string_finish,m2,args) skip lexbuf in  
       skipToken skip (COMMENT (AT_camlonly(!args.ifdefStack,m))) (camlonly m args) lexbuf }
 | newline { newline lexbuf;  skipToken skip (COMMENT (AT_camlonly(!args.ifdefStack,m))) (camlonly m args) lexbuf }
 | "(*ENDIF-CAML*)"  {  skipToken skip (COMMENT (AT_token !args.ifdefStack)) (token args) lexbuf }
 | "(*ENDIF-OCAML*)" {  skipToken skip (COMMENT (AT_token !args.ifdefStack)) (token args) lexbuf }
 | [^ '(' '"' '\n' '\r' ]+  { skipToken skip (COMMENT (AT_camlonly(!args.ifdefStack,m))) (camlonly m args) lexbuf }
 | eof { EOF (AT_camlonly(!args.ifdefStack,m)) }
 | _ {  skipToken skip (COMMENT (AT_camlonly(!args.ifdefStack,m))) (camlonly m args) lexbuf }
