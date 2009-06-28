//=========================================================================
// (c) Microsoft Corporation 2005-2009. 
//=========================================================================

///Pervasives: Additional bindings available at the top level 
module Microsoft.FSharp.Core.ExtraTopLevelOperators

open System
open Microsoft.FSharp.Core
open Microsoft.FSharp.Control
open Microsoft.FSharp.Collections
open Microsoft.FSharp.Text
open Microsoft.FSharp.Math

#if DONT_INCLUDE_DEPRECATED
#else
[<Obsolete("Use 'sprintf' with a '%A' pattern instead")>]
val any_to_string: 'T -> string

[<Obsolete("Use 'fprintf' with a '%A' pattern instead")>]
val output_any: System.IO.TextWriter -> 'T -> unit

[<Obsolete("Use 'printf' with a '%A' pattern instead")>]
val print_any : 'T -> unit
#endif

/// Print to <c>stdout</c> using the given format
val printf  :                format:Printf.TextWriterFormat<'T> -> 'T
/// Print to <c>stdout</c> using the given format, and add a newline
val printfn  :                format:Printf.TextWriterFormat<'T> -> 'T
/// Print to <c>stderr</c> using the given format
val eprintf  :               format:Printf.TextWriterFormat<'T> -> 'T
/// Print to <c>stderr</c> using the given format, and add a newline
val eprintfn  :               format:Printf.TextWriterFormat<'T> -> 'T
/// Print to a string using the given format
val sprintf :                format:Printf.StringFormat<'T> -> 'T
/// Print to a string buffer and raise an exception with the given
/// result.   Helper printers must return strings.
val failwithf: format:Printf.StringFormat<'T,'d> -> 'T
/// Print to a file using the given format
val fprintf : textWriter:System.IO.TextWriter -> format:Printf.TextWriterFormat<'T> -> 'T
/// Print to a file using the given format, and add a newline
val fprintfn : textWriter:System.IO.TextWriter -> format:Printf.TextWriterFormat<'T> -> 'T

/// Builds a set from a sequence of objects. The key objects are indexed using generic comparison
val set : elements:seq<'T> -> Set<'T>

/// Build an aysnchronous workflow using computation expression syntax
val async : Microsoft.FSharp.Control.AsyncBuilder  

/// Builds a lookup table from a sequence of key/value pairs. The key objects are indexed using generic hashing and equality.
val dict : keyValuePairs:seq<'K * 'V> -> System.Collections.Generic.IDictionary <'K,'V>

/// Builds a sequence using sequence expression syntax
val seq : sequence:seq<'T> -> seq<'T>

#if FX_MINIMAL_REFLECTION // not on Compact Framework 
#else
/// Special prefix operator for splicing typed expressions into quotation holes
val (~%) : expression:Microsoft.FSharp.Quotations.Expr<'T> -> 'T

/// Special prefix operator for splicing untyped expressions into quotation holes
val (~%%) : expression:Microsoft.FSharp.Quotations.Expr -> 'T
#endif

/// An active pattern to force the execution of values of type <c>Lazy&lt;_&gt;</c>
val (|Lazy|) : input:Lazy<'T> -> 'T

#if DONT_INCLUDE_DEPRECATED
#else
[<Obsolete("The 'Array2' module has been renamed to 'Array2D'", true)>]
val Array2 : int

[<Obsolete("The 'Array3' module has been renamed to 'Array3D'", true)>]
val Array3 : int

[<Obsolete("The 'IEvent' module has been renamed to 'Event'", true)>]
val IEvent : int
#endif
