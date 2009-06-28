// (c) Microsoft Corporation. All rights reserved

/// Parse "printf-style" format specifiers at compile time, producing
/// a list of items that specify the types of the things that follow.
///
/// Must be updated if the Printf runtime component is updated.

module internal Microsoft.FSharp.Compiler.Formats

open Internal.Utilities
open Microsoft.FSharp.Compiler 
open Microsoft.FSharp.Compiler.AbstractIL.Internal 

val ParseFormatString : Range.range -> Env.TcGlobals -> string -> Tast.typ -> Tast.typ -> Tast.typ -> Tast.typ * Tast.typ
