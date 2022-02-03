module Building

open FSharp.Core
open Fake.Core
open Fake.DotNet
open Fake.IO.FileSystemOperators

open Model
open Dotnet

let rootDir = System.Environment.CurrentDirectory

let clean (solution:Solution) = dotnet rootDir (sprintf "clean %s --configuration Release --verbosity minimal" solution.SolutionFile)

let restoreWeak (solution:Solution) = dotnetWeak rootDir (sprintf "restore %s --verbosity minimal" solution.SolutionFile)
let restoreStrong (solution:Solution) = dotnetStrong rootDir (sprintf "restore %s --verbosity minimal" solution.SolutionFile)

let buildWeak (solution:Solution) = dotnetWeak rootDir (sprintf "build %s --configuration Release --no-incremental --no-restore --verbosity minimal" solution.SolutionFile)
let buildStrong (solution:Solution) = dotnetStrong rootDir (sprintf "build %s --configuration Release --no-incremental --no-restore --verbosity minimal" solution.SolutionFile)

let packWeak (solution:Solution) = dotnetWeak rootDir (sprintf "pack %s --configuration Release --no-restore --verbosity minimal" solution.SolutionFile)
let packStrong (solution:Solution) = dotnetStrong rootDir (sprintf "pack %s --configuration Release --no-restore --verbosity minimal" solution.SolutionFile)

let packProjectWeak = function
    | VisualStudio p -> dotnetWeak rootDir (sprintf "pack %s --configuration Release --no-restore --no-build" p.ProjectFile)
    | _ -> failwith "Project type not supported"
let packProjectStrong = function
    | VisualStudio p -> dotnetStrong rootDir (sprintf "pack %s --configuration Release --no-restore --no-build" p.ProjectFile)
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

let test testsDir testsProj framework =
    dotnet testsDir (sprintf "run --project %s --configuration Release --framework %s --no-restore --no-build" testsProj framework)
