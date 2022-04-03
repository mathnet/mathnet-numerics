module Packaging

open FSharp.Core
open Fake.Core
open Fake.DotNet
open Fake.DotNet.NuGet
open Fake.IO
open Fake.IO.FileSystemOperators
open Fake.IO.Globbing.Operators
open System

open Model

let collectBinaries (solution:Solution) =
    solution.Projects |> List.iter (function
        | VisualStudio project -> Shell.copyDir solution.OutputLibDir project.OutputDir (fun n -> n.Contains(project.AssemblyName + ".dll") || n.Contains(project.AssemblyName + ".pdb") || n.Contains(project.AssemblyName + ".xml"))
        | _ -> failwith "Project type not supported")

let collectBinariesSN (solution:Solution) =
    solution.Projects |> List.iter (function
        | VisualStudio project -> Shell.copyDir solution.OutputLibStrongNameDir project.OutputDir (fun n -> n.Contains(project.AssemblyName + ".dll") || n.Contains(project.AssemblyName + ".pdb") || n.Contains(project.AssemblyName + ".xml"))
        | _ -> failwith "Project type not supported")

let collectNuGetPackages (solution:Solution) =
    solution.Projects |> List.iter (function
        | VisualStudio project -> Shell.copyDir solution.OutputNuGetDir project.OutputDir (fun n -> n.EndsWith(".nupkg"))
        | _ -> failwith "Project type not supported")

let provideLicense licenseFile path =
    File.readAsString licenseFile
    |> String.convertTextToWindowsLineBreaks
    |> File.replaceContent (path </> "license.txt")

let private provideReadme header title (release:Release) path =
    String.concat Environment.NewLine [header; " " + title; ""; File.readAsString release.ReleaseNotesFile]
    |> String.convertTextToWindowsLineBreaks
    |> File.replaceContent (path </> "readme.txt")


// SIGN

let sign fingerprint timeserver (solution: Solution) =
    let files = solution.Projects |> Seq.collect (function
        | VisualStudio project -> !! (project.OutputDir + "/**/" + project.AssemblyName + ".dll")
        | _ -> failwith "Project type not supported")
    let fileArgs = files |> Seq.map (sprintf "\"%s\"") |> String.concat " "
    let optionsArgs = sprintf """/v /fd sha256 /sha1 "%s" /tr "%s" /td sha256""" fingerprint timeserver
    let arguments = sprintf """sign %s %s""" optionsArgs fileArgs
    let result =
        CreateProcess.fromRawCommandLine (ProcessUtils.findLocalTool "SIGNTOOL" "signtool.exe" ["""C:\Program Files (x86)\Windows Kits\10\bin\x64"""]) arguments
        |> CreateProcess.withTimeout (TimeSpan.FromMinutes 10.)
        |> Proc.run
    if result.ExitCode <> 0 then failwithf "Error during SignTool call "

let signNuGet fingerprint timeserver (solutions: Solution list) =
    Shell.cleanDir "obj/NuGet"
    solutions
    |> Seq.collect (fun solution -> !! (solution.OutputNuGetDir </> "*.nupkg"))
    |> Seq.distinct
    |> Seq.iter (fun file ->
        let args = sprintf """sign "%s" -HashAlgorithm SHA256 -TimestampHashAlgorithm SHA256 -CertificateFingerprint "%s" -Timestamper "%s""" (Path.getFullName file) fingerprint timeserver
        let result =
            CreateProcess.fromRawCommandLine "packages/build/NuGet.CommandLine/tools/NuGet.exe" args
            |> CreateProcess.withWorkingDirectory (Path.getFullName "obj/NuGet")
            |> CreateProcess.withTimeout (TimeSpan.FromMinutes 10.)
            |> Proc.run
        if result.ExitCode <> 0 then failwith "Error during NuGet sign.")
    Directory.delete "obj/NuGet"


let zip (package:ZipPackage) header zipDir filesDir filesFilter =
    Shell.cleanDir "obj/Zip"
    let workPath = "obj/Zip/" + package.Id
    Shell.copyDir workPath filesDir filesFilter
    provideLicense "LICENSE.md" workPath
    provideReadme header (sprintf "%s v%s" package.Title package.Release.PackageVersion) package.Release workPath
    Zip.zip "obj/Zip/" (zipDir </> sprintf "%s-%s.zip" package.Id package.Release.PackageVersion) !! (workPath + "/**/*.*")
    Directory.delete "obj/Zip"


let private updateNuspec (nuget:NuGetPackage) outPath dependencies (spec:NuGet.NuGet.NuGetParams) =
    { spec with ToolPath = "packages/build/NuGet.CommandLine/tools/NuGet.exe"
                OutputPath = outPath
                WorkingDir = "obj/NuGet"
                Version = nuget.Release.PackageVersion
                Dependencies = dependencies
                ReleaseNotes = nuget.Release.ReleaseNotes
                Publish = false }

let nugetPackManually (solution:Solution) (packages:NuGetSpecification list) licenseFile header =
    Shell.cleanDir "obj/NuGet"
    for pack in packages do
        provideLicense licenseFile "obj/NuGet"
        provideReadme header (sprintf "%s v%s" pack.Title pack.NuGet.Release.PackageVersion) pack.NuGet.Release "obj/NuGet"
        NuGet.NuGet (updateNuspec pack.NuGet solution.OutputNuGetDir pack.Dependencies) pack.NuSpecFile
        Shell.cleanDir "obj/NuGet"
    Directory.delete "obj/NuGet"
