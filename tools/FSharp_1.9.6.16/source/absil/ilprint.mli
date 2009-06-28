// (c) Microsoft Corporation. All rights reserved

/// Printer for the abstract syntax.
module  Microsoft.FSharp.Compiler.AbstractIL.AsciiWriter 

open Microsoft.FSharp.Compiler.AbstractIL 
open Microsoft.FSharp.Compiler.AbstractIL.IL 
open Microsoft.FSharp.Compiler.AbstractIL.Internal 
open System.IO

#if DEBUG
val public output_module      : TextWriter -> ILModuleDef -> unit
#endif

val public emit_tailcalls : bool ref
