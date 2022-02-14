module Model

open FSharp.Core
open Fake.Core
open Fake.IO
open Fake.IO.FileSystemOperators
open System.IO

type Release =
    { RepoKey: string
      Title: string
      AssemblyVersion: string
      PackageVersion: string
      ReleaseNotes: string
      ReleaseNotesFile: string }

type ZipPackage =
    { Id: string
      Release: Release
      Title: string }

type NuGetPackage =
    { Id: string
      Release: Release }

type VisualStudioProject =
    { AssemblyName: string
      ProjectFile: string
      OutputDir: string
      Release: Release
      NuGetPackages: NuGetPackage list }

type NativeVisualStudioProject =
    { BinaryName: string
      ProjectFile: string
      OutputDir: string
      Release: Release
      NuGetPackages: NuGetPackage list }

type NativeBashScriptProject =
    { BinaryName: string
      BashScriptFile: string
      OutputDir: string
      Release: Release
      NuGetPackages: NuGetPackage list }

type Project =
    | VisualStudio of VisualStudioProject
    | NativeVisualStudio of NativeVisualStudioProject
    | NativeBashScript of NativeBashScriptProject

type Solution =
    { Key: string
      SolutionFile: string
      Projects: Project list
      Release: Release
      ZipPackages: ZipPackage list
      OutputDir: string
      OutputLibDir: string
      OutputLibStrongNameDir: string
      OutputZipDir: string
      OutputNuGetDir: string }

type NuGetSpecification =
    { NuGet: NuGetPackage
      NuSpecFile: string
      Dependencies: (string * string) list
      Title: string }

let release repoKey title releaseNotesFile : Release =
    let info = ReleaseNotes.load releaseNotesFile
    let buildPart = "0"
    let assemblyVersion = info.AssemblyVersion + "." + buildPart
    let packageVersion = info.NugetVersion
    let notes = info.Notes |> List.map (fun l -> l.Replace("*","").Replace("`","")) |> String.toLines
    { Release.RepoKey = repoKey
      Title = title
      AssemblyVersion = assemblyVersion
      PackageVersion = packageVersion
      ReleaseNotes = notes
      ReleaseNotesFile = releaseNotesFile }

let zipPackage packageId title release =
    { ZipPackage.Id = packageId
      Title = title
      Release = release }

let nugetPackage packageId release =
    { NuGetPackage.Id = packageId
      Release = release }

let project assemblyName projectFile nuGetPackages =
    { VisualStudioProject.AssemblyName = assemblyName
      ProjectFile = projectFile
      OutputDir = (Path.GetDirectoryName projectFile) </> "bin" </> "Release"
      NuGetPackages = nuGetPackages
      Release = nuGetPackages |> List.map (fun p -> p.Release) |> List.distinct |> List.exactlyOne }
    |> Project.VisualStudio

let nativeProject binaryName projectFile nuGetPackages =
    { NativeVisualStudioProject.BinaryName = binaryName
      ProjectFile = projectFile
      OutputDir = (Path.GetDirectoryName projectFile) </> "bin" </> "Release"
      NuGetPackages = nuGetPackages
      Release = nuGetPackages |> List.map (fun p -> p.Release) |> List.distinct |> List.exactlyOne }
    |> Project.NativeVisualStudio

let nativeBashScriptProject binaryName bashScriptFile nuGetPackages =
    { NativeBashScriptProject.BinaryName = binaryName
      BashScriptFile = bashScriptFile
      OutputDir = (Path.GetDirectoryName bashScriptFile) </> "bin" </> "Release"
      NuGetPackages = nuGetPackages
      Release = nuGetPackages |> List.map (fun p -> p.Release) |> List.distinct |> List.exactlyOne }
    |> Project.NativeBashScript

let projectRelease = function
    | VisualStudio p -> p.Release
    | NativeVisualStudio p -> p.Release
    | NativeBashScript p -> p.Release

let projectNuGetPackages = function
    | VisualStudio p -> p.NuGetPackages
    | NativeVisualStudio p -> p.NuGetPackages
    | NativeBashScript p -> p.NuGetPackages

let solution key solutionFile projects zipPackages =
    { Solution.Key = key
      SolutionFile = solutionFile
      Projects = projects
      ZipPackages = zipPackages
      Release = List.concat [ projects |> List.map projectRelease; zipPackages |> List.map (fun p -> p.Release) ] |> List.distinct |> List.exactlyOne
      OutputDir = "out" </> key
      OutputLibDir = "out" </> key </> "Lib"
      OutputLibStrongNameDir = "out" </> key </> "Lib-StrongName"
      OutputZipDir = "out" </> key </> "Zip"
      OutputNuGetDir = "out" </> key </> "NuGet" }
