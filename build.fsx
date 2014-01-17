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
Target "BuildAll" (fun _ -> !! "MathNet.Numerics.Portable.sln" |> MSBuildRelease "" "Rebuild" |> ignore)

Target "Main" DoNothing
Target "Net35" DoNothing
Target "All" DoNothing

"Prepare" ==> "BuildMain" ==> "Main"
"Prepare" ==> "BuildNet35" ==> "Net35"
"Prepare" ==> "BuildAll" ==> "All"


// TEST

Target "RunTests" (fun _ ->
    !! "out/test/*/*UnitTests*.dll"
    |> NUnit (fun p ->
        { p with
            DisableShadowCopy = true
            TimeOut = TimeSpan.FromMinutes 20.
            OutputFile = "TestResults.xml" })
)

"RunTests" ==> "Main"
"RunTests" ==> "Net35"
"RunTests" ==> "All"


// DOCUMENTATION

Target "BuildDocs" DoNothing


// RELEASE

Target "Release" DoNothing
"All"  ==> "BuildDocs" ==> "Release"


RunTargetOrDefault "Main"
