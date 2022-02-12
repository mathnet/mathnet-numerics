module Building

open FSharp.Core
open Fake.Core
open Fake.DotNet
open Fake.DotNet.DotNet.Options
open Fake.IO.FileSystemOperators

open Model

let inline private minimal p = withVerbosity (Some DotNet.Verbosity.Minimal) p
let inline private buildOptions args (p:DotNet.BuildOptions) = { p with NoRestore = true; MSBuildParams = args }
let inline private packOptions args (p:DotNet.PackOptions) = { p with NoRestore = true; MSBuildParams = args }

let private normal = { MSBuild.CliArguments.Create() with NodeReuse = false; Properties = [ ("StrongName", "False") ] }
let private strongNamed = { MSBuild.CliArguments.Create() with NodeReuse = false; Properties = [ ("StrongName", "True") ] }

let restore (solution:Solution) = DotNet.restore minimal solution.SolutionFile

let build (solution:Solution) = DotNet.build (minimal >> buildOptions normal) solution.SolutionFile
let buildStrongNamed (solution:Solution) = DotNet.build (minimal >> buildOptions strongNamed) solution.SolutionFile

let pack (solution:Solution) = DotNet.pack (minimal >> packOptions normal) solution.SolutionFile
let packStrongNamed (solution:Solution) = DotNet.pack (minimal >> packOptions strongNamed) solution.SolutionFile

let buildVS2022x86 config isIncremental subject =
    MSBuild.run
        (fun p -> {p with ToolPath = Environment.environVar "VS2022INSTALLDIR" </> @"MSBuild\Current\Bin\MSBuild.exe"})
        ""
        (if isIncremental then "Build" else "Rebuild")
        [("Configuration", config); ("Platform","Win32")]
        subject
        |> ignore<string list>
let buildVS2022x64 config isIncremental subject =
    MSBuild.run
        (fun p -> {p with ToolPath = Environment.environVar "VS2022INSTALLDIR" </> @"MSBuild\Current\Bin\MSBuild.exe"})
        ""
        (if isIncremental then "Build" else "Rebuild")
        [("Configuration", config); ("Platform","x64")]
        subject
        |> ignore<string list>
