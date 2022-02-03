module Testing

open FSharp.Core
open Fake.Core
open Fake.DotNet

let test testsDir testsProj framework =
    DotNet.exec
        (fun c -> { c with WorkingDirectory = testsDir })
        (sprintf "run --project %s --configuration Release --framework %s --no-restore --no-build" testsProj framework)
        ""
        |> ignore<ProcessResult>
