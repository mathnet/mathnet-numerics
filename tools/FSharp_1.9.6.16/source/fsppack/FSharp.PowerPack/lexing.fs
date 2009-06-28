//==========================================================================
// (c) Microsoft Corporation 2005-2009.  
//
//=========================================================================

#if INTERNALIZED_POWER_PACK
module (* internal *) Internal.Utilities.Compatibility.OCaml.Lexing
open Internal.Utilities.Text.Lexing
#else
module Microsoft.FSharp.Compatibility.OCaml.Lexing
open Microsoft.FSharp.Text.Lexing
#endif

open Microsoft.FSharp.Core
open System.IO
open System.Text

type position = Position  

type lexbuf =  LexBuffer<byte>

let from_function (f: byte[] -> int -> int)  = 
    LexBuffer<byte>.FromByteFunction f

let from_text_reader (enc: System.Text.Encoding) (tr: TextReader) =
  from_function (fun bytebuf len ->
    /// Don't read too many characters!
    let lenc = (len * 99) / enc.GetMaxByteCount(100) in 
    let charbuf : char[] = Array.zeroCreate lenc in
    let nRead = tr.Read(charbuf,0,lenc) in
    if nRead = 0 then 0 
                 else enc.GetBytes(charbuf,0,nRead,bytebuf,0))
                 
let defaultEncoding =
#if FX_NO_DEFAULT_ENCODING
        Encoding.UTF8
#else
        Encoding.Default
#endif

let from_channel (is:TextReader)  = from_text_reader defaultEncoding is

let from_bytearray s  = 
    LexBuffer<byte>.FromBytes(s)

#if FX_NO_ASCII_ENCODING
let from_string s  = from_channel (new StringReader(s))
#else
let from_string s  = from_bytearray (System.Text.Encoding.ASCII.GetBytes(s:string))
#endif

let from_binary_reader (sr: BinaryReader)  = from_function (fun s max -> sr.Read(s,0,max))

let lexeme_char (lb:lexbuf) n =  char (int32 (lb.LexemeChar n))
let lexeme_start_p (lb:lexbuf) = lb.StartPos
let lexeme_end_p (lb:lexbuf) = lb.EndPos
let lexeme_start (lb:lexbuf) = (lexeme_start_p lb).pos_cnum
let lexeme_end (lb:lexbuf) = (lexeme_end_p lb).pos_cnum
#if FX_NO_ASCII_ENCODING
let lexeme_utf8 (lb:lexbuf) = System.Text.Encoding.UTF8.GetString(lb.Lexeme, 0, lb.Lexeme.Length)
#else
let lexeme (lb:lexbuf) = System.Text.Encoding.ASCII.GetString(lb.Lexeme, 0, lb.Lexeme.Length)
let lexeme_utf8 (lb:lexbuf) = System.Text.Encoding.UTF8.GetString(lb.Lexeme, 0, lb.Lexeme.Length)
#endif

let lexeme_bytes (lb:lexbuf) = lb.Lexeme
let flush_input (lb: lexbuf) = lb.DiscardInput ()

let lexbuf_curr_p lb = lexeme_end_p lb
let lexbuf_set_curr_p (lb:lexbuf) (p : position) = lb.EndPos  <- p
let lexbuf_set_start_p (lb:lexbuf) (p : position) = lb.StartPos <- p

