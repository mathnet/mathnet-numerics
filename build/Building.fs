module Building

open FSharp.Core
open Fake.Core
open Fake.DotNet
open Fake.IO.FileSystemOperators

open Model


let private dotnet command =
    DotNet.exec id command "" |> ignore<ProcessResult>

let private dotnetWeak command =
    let properties = [ ("StrongName", "False") ]
    let suffix = properties |> List.map (fun (name, value) -> sprintf """ /p:%s="%s" /nr:false """ name value) |> String.concat ""
    DotNet.exec id command suffix |> ignore<ProcessResult>

let private dotnetStrong command =
    let properties = [ ("StrongName", "True") ]
    let suffix = properties |> List.map (fun (name, value) -> sprintf """ /p:%s="%s" /nr:false """ name value) |> String.concat ""
    DotNet.exec id command suffix |> ignore<ProcessResult>


let clean (solution:Solution) = dotnet (sprintf "clean %s --configuration Release --verbosity minimal" solution.SolutionFile)

let restoreWeak (solution:Solution) = dotnetWeak (sprintf "restore %s --verbosity minimal" solution.SolutionFile)
let restoreStrong (solution:Solution) = dotnetStrong (sprintf "restore %s --verbosity minimal" solution.SolutionFile)

let buildWeak (solution:Solution) = dotnetWeak (sprintf "build %s --configuration Release --no-incremental --no-restore --verbosity minimal" solution.SolutionFile)
let buildStrong (solution:Solution) = dotnetStrong (sprintf "build %s --configuration Release --no-incremental --no-restore --verbosity minimal" solution.SolutionFile)

let packWeak (solution:Solution) = dotnetWeak (sprintf "pack %s --configuration Release --no-restore --verbosity minimal" solution.SolutionFile)
let packStrong (solution:Solution) = dotnetStrong (sprintf "pack %s --configuration Release --no-restore --verbosity minimal" solution.SolutionFile)

let packProjectWeak = function
    | VisualStudio p -> dotnetWeak (sprintf "pack %s --configuration Release --no-restore --no-build" p.ProjectFile)
    | _ -> failwith "Project type not supported"
let packProjectStrong = function
    | VisualStudio p -> dotnetStrong (sprintf "pack %s --configuration Release --no-restore --no-build" p.ProjectFile)
    | _ -> failwith "Project type not supported"

let buildVS2019x86 config isIncremental subject =
    MSBuild.run
        (fun p -> {p with ToolPath = Environment.environVar "VS2019INSTALLDIR" </> @"MSBuild\Current\Bin\MSBuild.exe"})
        ""
        (if isIncremental then "Build" else "Rebuild")
        [("Configuration", config); ("Platform","Win32")]
        subject
        |> ignore<string list>
let buildVS2019x64 config isIncremental subject =
    MSBuild.run
        (fun p -> {p with ToolPath = Environment.environVar "VS2019INSTALLDIR" </> @"MSBuild\Current\Bin\MSBuild.exe"})
        ""
        (if isIncremental then "Build" else "Rebuild")
        [("Configuration", config); ("Platform","x64")]
        subject
        |> ignore<string list>
