// (c) Microsoft Corporation. All rights reserved

#light

module internal Microsoft.FSharp.Compiler.CommandLineMain

open Microsoft.FSharp.Compiler
open Microsoft.FSharp.Compiler.ErrorLogger
open Microsoft.FSharp.Compiler.Driver
open System.Runtime.CompilerServices


[<Dependency("FSharp.Compiler",LoadHint.Always)>] 
do ()

[<EntryPoint>]
let main(argv) = 
    try 
        Driver.main(Array.append [| "fsc.exe" |] argv); 
        0 
    with e -> 
        errorRecovery e Microsoft.FSharp.Compiler.Range.range0; 
        1

