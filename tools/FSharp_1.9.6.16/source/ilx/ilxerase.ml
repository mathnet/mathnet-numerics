// (c) Microsoft Corporation. All rights reserved
#light

module Microsoft.FSharp.Compiler.AbstractIL.Extensions.ILX.Ilxerase

open Internal.Utilities

open Microsoft.FSharp.Compiler.AbstractIL 
open Microsoft.FSharp.Compiler.AbstractIL.Internal 
open Microsoft.FSharp.Compiler.AbstractIL.Extensions.ILX
module Ilprint = Microsoft.FSharp.Compiler.AbstractIL.AsciiWriter 
module Il = Microsoft.FSharp.Compiler.AbstractIL.IL 

open Il
open Msilxlib
open Ilxsettings

let SetNoTailCalls () = 
   Msilxlib.tailcall_implementation := Ilxsettings.NoTailcalls; 
   Ilprint.emit_tailcalls := false
   
let SetTailCalls () = 
   Msilxlib.tailcall_implementation := Ilxsettings.AllTailcalls; 
   Ilprint.emit_tailcalls := true

let ConvModuleFragment ilg modFragName modul = 
    let modul = EraseIlxClassunions.ConvModule ilg modul 
    let modul = Pubclo.ConvModule ilg modul 
    modul

let ConvModule ilg modul = ConvModuleFragment ilg modul.modulName modul
