// (c) Microsoft Corporation. All rights reserved

/// Diagnostics from the AbsIL toolkit. You can reset the diagnostics 
/// stream to point elsewhere, or turn it
/// off altogether by setting it to 'None'.  The logging channel initally
/// points to stderr.  All functions call flush() automatically.
///
/// REVIEW: review if we should just switch to System.Diagnostics
module Microsoft.FSharp.Compiler.AbstractIL.Diagnostics

open System.IO
open Microsoft.FSharp.Text.Printf
open Microsoft.FSharp.Compiler.AbstractIL
open Microsoft.FSharp.Compiler.AbstractIL.Internal 

val public setDiagnosticsChannel: TextWriter option -> unit

val public dprintfn: TextWriterFormat<'a> -> 'a 
val public dprintf: TextWriterFormat<'a> -> 'a 

val public dprintn: string -> unit
val public dprint: string -> unit

