// (c) Microsoft Corporation. All rights reserved

/// Configurable AppDomain-global diagnostics channel for the Abstract IL library
///
/// REVIEW: review if we should just switch to System.Diagnostics
module Microsoft.FSharp.Compiler.AbstractIL.Diagnostics 

open Internal.Utilities
open Internal.Utilities.Pervasives

open Microsoft.FSharp.Compiler.AbstractIL 
open Microsoft.FSharp.Compiler.AbstractIL.Internal 

let diagnosticsLog = ref (Some stdout)
let dflushn () = match !diagnosticsLog with None -> () | Some d -> d.WriteLine(); flush d
let dflush () = match !diagnosticsLog with None -> () | Some d -> flush d
let dprintn s = 
  match !diagnosticsLog with None -> () | Some d -> output_string d s; output_string d "\n"; dflush()
let dprint s = 
  match !diagnosticsLog with None -> () | Some d -> output_string d s; dflush()

let dprintf (fmt: (_,_,_,_) format4) = 
    Printf.kfprintf dflush (match !diagnosticsLog with None -> System.IO.TextWriter.Null | Some d -> d) fmt

let dprintfn (fmt: (_,_,_,_) format4) = 
    Printf.kfprintf dflushn (match !diagnosticsLog with None -> System.IO.TextWriter.Null | Some d -> d) fmt

let setDiagnosticsChannel s = diagnosticsLog := s
