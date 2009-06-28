// (c) Microsoft Corporation 2005-2009. 
#light

module  Microsoft.FSharp.Compiler.UnicodeLexing

open Microsoft.FSharp.Text
open Internal.Utilities.Text.Lexing

type Lexbuf = LexBuffer<char>
val internal StringAsLexbuf : string -> Lexbuf
val public FunctionAsLexbuf : (char [] * int * int -> int) -> Lexbuf
val public UnicodeFileAsLexbuf :string * int option -> System.IO.FileStream * System.IO.StreamReader * Lexbuf
