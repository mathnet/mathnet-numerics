// (c) Microsoft Corporation 2005-2009.
module Microsoft.FSharp.Compiler.AbstractIL.Extensions.ILX.Ilxconfig
open Microsoft.FSharp.Compiler.AbstractIL
open Microsoft.FSharp.Compiler.AbstractIL.Extensions.ILX
let version = "1.9.6.16"
let default_tailcall_implementation = Ilxsettings.AllTailcalls  
let default_call_implementation = Ilxsettings.VirtEntriesVirtCode 
let default_entrypoint_implementation = Ilxsettings.MultiEntryPoints
