module Testing

open FSharp.Core
open Fake.Core
open Fake.DotNet
//open Fake.DotNet.DotNet.Options

let test testsDir testsProj framework =
    //DotNet.test (withWorkingDirectory testsDir >> (fun o -> { o with Framework = Some framework; NoRestore = true; NoBuild = true; Configuration = DotNet.BuildConfiguration.Release })) testsProj
    let result =
        DotNet.exec
            (fun c -> { c with WorkingDirectory = testsDir })
            (sprintf "run --project %s --configuration Release --framework %s --no-restore --no-build" testsProj framework)
            ""
    if not result.OK then failwithf "Tests Failed: %A" result

