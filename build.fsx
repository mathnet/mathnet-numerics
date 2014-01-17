// --------------------------------------------------------------------------------------
// FAKE build script, see http://fsharp.github.io/FAKE
// --------------------------------------------------------------------------------------

#r @"packages/FAKE/tools/FakeLib.dll"
open Fake 
open Fake.Git
open Fake.AssemblyInfoFile
open Fake.ReleaseNotesHelper
open System

Environment.CurrentDirectory <- __SOURCE_DIRECTORY__


// PREPARE

Target "Clean" (fun _ -> CleanDirs ["out"; "obj"; "temp"])
Target "RestorePackages" RestorePackages
Target "AssemblyInfo" DoNothing
Target "Prepare" DoNothing

"Clean" ==> "Restorepackages" ==> "AssemblyInfo" ==> "Prepare"


// BUILD

Target "BuildMain" (fun _ -> !! "MathNet.Numerics.sln" |> MSBuildRelease "" "Rebuild" |> ignore)
Target "BuildNet35" (fun _ -> !! "MathNet.Numerics.Net35Only.sln" |> MSBuildRelease "" "Rebuild" |> ignore)
Target "BuildFull" (fun _ -> !! "MathNet.Numerics.Portable.sln" |> MSBuildRelease "" "Rebuild" |> ignore)
Target "Build" DoNothing

"Prepare"
  =?> ("BuildNet35", hasBuildParam "net35")
  =?> ("BuildFull", hasBuildParam "full")
  =?> ("BuildMain", not (hasBuildParam "full" || hasBuildParam "net35"))
  ==> "Build"


// TEST

Target "Test" (fun _ ->
    !! "out/test/*/*UnitTests*.dll"
    |> NUnit (fun p ->
        { p with
            DisableShadowCopy = true
            TimeOut = TimeSpan.FromMinutes 30.
            OutputFile = "TestResults.xml" })
)

"Build" ==> "Test"


// DOCUMENTATION

Target "Docs" DoNothing

"Build"  ==> "Docs"


// NUGET

Target "NuGetPackage" DoNothing

"BuildFull" ==> "NuGetPackage"


// RUN

Target "Release" DoNothing
"Test" ==> "Release"
"Docs" ==> "Release"
"NuGetPackage" ==> "Release"

Target "All" DoNothing
"Build" ==> "Test" ==> "All"

RunTargetOrDefault "All"


// EXAMPLES

// * build.cmd: normal build (.Net 4.0), run unit tests

// * build.cmd All: normal build (.Net 4.0), run unit tests
// * build.cmd All full: full build (.Net 3.5, 4.0, PCL), run all unit tests
// * build.cmd All net35: build (.Net 3.5), run unit tests

// * build.cmd Build: normal build (.Net 4.0)
// * build.cmd Build full: full build (.Net 3.5, 4.0, PCL)
// * build.cmd Build net35: build (.Net 3.5)

// * build.cmd Clean: cleanup build artifacts
// * build.cmd Docs: generate documentation, normal build
// * build.cmd NuGetPackage: generate NuGet packages, full build

