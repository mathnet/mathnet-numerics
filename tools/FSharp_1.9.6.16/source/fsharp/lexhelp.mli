(*------------------------------------------------------------------------
 * (c) Microsoft Corporation. All rights reserved 
 *
 * Helper functions for the F# lexer lex.mll
 *-----------------------------------------------------------------------*)

#light

module Microsoft.FSharp.Compiler.Lexhelp

open Internal.Utilities
open Internal.Utilities.Text
open Internal.Utilities.Pervasives
open Microsoft.FSharp.Text
open Microsoft.FSharp.Compiler.ErrorLogger
open Microsoft.FSharp.Compiler.Ast
open Microsoft.FSharp.Compiler.AbstractIL
open Microsoft.FSharp.Compiler.AbstractIL.Internal
open Microsoft.FSharp.Compiler

[<Sealed>]
type LightSyntaxStatus =
    new : initial:bool * warn : bool -> LightSyntaxStatus
    member ExplicitlySet : bool
    member Status : bool
    member Status : bool with set
    member WarnOnMultipleTokens : bool

[<Sealed>]
type LexResourceManager =
    new : unit -> LexResourceManager
    member InternString : s:string -> string

type lexargs =
  {defines: string list;
   ifdefStack: ifdefStack;
   resourceManager: LexResourceManager;
   getSourceDirectory: unit -> string;
   lightSyntaxStatus: LightSyntaxStatus;
   errorLogger: ErrorLogger}
val resetLexbufPos : string -> UnicodeLexing.Lexbuf -> unit
val mkLexargs : (unit -> string) * 'a * string list * LightSyntaxStatus * LexResourceManager * ifdefStack * ErrorLogger -> lexargs
val reusingLexbufForParsing : UnicodeLexing.Lexbuf * ConcreteSyntaxSink option -> (unit -> 'a) -> 'a 

val internal  set_pos : UnicodeLexing.Lexbuf -> Lexing.Position -> unit
val internal usingLexbufForParsing : UnicodeLexing.Lexbuf * string * ConcreteSyntaxSink option -> (UnicodeLexing.Lexbuf -> 'a) -> 'a
val internal default_string_finish : 'a -> 'b -> byte[] -> Parser.token
val internal call_string_finish : ('a -> 'b -> byte[] -> 'c) -> AbstractIL.Internal.Bytes.Bytebuf.t -> 'a -> 'b -> 'c
val internal add_string : AbstractIL.Internal.Bytes.Bytebuf.t -> string -> unit
val internal add_int_char : AbstractIL.Internal.Bytes.Bytebuf.t -> int -> unit
val internal add_unichar : AbstractIL.Internal.Bytes.Bytebuf.t -> int -> unit
val internal add_byte_char : AbstractIL.Internal.Bytes.Bytebuf.t -> char -> unit
val internal stringbuf_as_bytes : AbstractIL.Internal.Bytes.Bytebuf.t -> byte[]
val internal stringbuf_is_bytes : AbstractIL.Internal.Bytes.Bytebuf.t -> bool
val internal newline : Lexing.LexBuffer<'a> -> unit
val internal trigraph : char -> char -> char -> char
val internal digit : char -> int32
val internal hexdigit : char -> int32
val internal unicodegraph_short : string -> uint16
val internal hexgraph_short : string -> uint16
val internal unicodegraph_long : string -> uint16 option * uint16
val internal escape : char -> char

val inline internal skipToken : bool -> Parser.token -> (bool -> UnicodeLexing.Lexbuf -> Parser.token) -> UnicodeLexing.Lexbuf -> Parser.token

exception internal ReservedKeyword of string * Range.range
exception internal IndentationProblem of string * Range.range

module Keywords = 
    val internal KeywordOrIdentifierToken : lexargs -> UnicodeLexing.Lexbuf -> string -> Parser.token
    val internal IdentifierToken : lexargs -> UnicodeLexing.Lexbuf -> string -> Parser.token
    val internal QuoteIdentifierIfNeeded : string -> string
    val internal permitFsharpKeywords : bool ref
    val keywordNames : string list
