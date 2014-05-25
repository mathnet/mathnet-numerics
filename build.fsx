// --------------------------------------------------------------------------------------
// FAKE build script, see http://fsharp.github.io/FAKE
// --------------------------------------------------------------------------------------

#I "packages/FAKE/tools"
#r "packages/FAKE/tools/FakeLib.dll"

open Fake
open Fake.DocuHelper
open Fake.AssemblyInfoFile
open Fake.ReleaseNotesHelper
open System

Environment.CurrentDirectory <- __SOURCE_DIRECTORY__


// PROJECT INFO

type Package =
    { Id : string
      Title : string
      Summary : string
      Description : string
      Tags : string
      Authors : string list
      Dependencies : string list
      Files : (string * string option * string option) list }

let release = LoadReleaseNotes "RELEASENOTES.md"
let buildPart = "0" // TODO: Fetch from TC
let assemblyVersion = release.AssemblyVersion + "." + buildPart
let packageVersion = release.NugetVersion
let releaseNotes = release.Notes |> List.map (fun l -> l.Replace("*","").Replace("`","")) |> toLines
trace (sprintf "Building Math.NET Numerics %s" release.NugetVersion)

let summary = "Math.NET Numerics, providing methods and algorithms for numerical computations in science, engineering and every day use."
let description = "Math.NET Numerics is the numerical foundation of the Math.NET project, aiming to provide methods and algorithms for numerical computations in science, engineering and every day use. "
let support = "Supports .Net 4.0, .Net 3.5 and Mono on Windows, Linux and Mac; Silverlight 5, WindowsPhone/SL 8, WindowsPhone 8.1 and Windows 8 with PCL Portable Profiles 47 and 344; Android/iOS with Xamarin."
let supportFsharp = "Supports F# 3.0 on .Net 4.0 and Mono on Windows, Linux and Mac; Silverlight 5, WindowsPhone/SL 8, WindowsPhone 8.1 and Windows 8 with PCL Portable Profiles 47 and 344; Android/iOS with Xamarin."
let supportSigned = "Supports .Net 4.0."
let tags = "math numeric statistics probability integration interpolation regression solve fit linear algebra matrix fft"

let libnet35 = "lib/net35"
let libnet40 = "lib/net40"
let libpcl47 = "lib/portable-net45+sl5+netcore45+MonoAndroid1+MonoTouch1"
let libpcl344 = "lib/portable-net45+sl5+netcore45+wpa81+wp8+MonoAndroid1+MonoTouch1"

let numericsPack =
    { Id = "MathNet.Numerics"
      Title = "Math.NET Numerics"
      Summary = summary
      Description = description + support
      Tags = tags
      Authors = [ "Christoph Ruegg"; "Marcus Cuda"; "Jurgen Van Gael" ]
      Dependencies = []
      Files = [ "..\..\out\lib\Net35\MathNet.Numerics.*", Some libnet35, Some "**\MathNet.Numerics.FSharp.*";
                "..\..\out\lib\Net40\MathNet.Numerics.*", Some libnet40, Some "**\MathNet.Numerics.FSharp.*";
                "..\..\out\lib\Profile47\MathNet.Numerics.*", Some libpcl47, Some "**\MathNet.Numerics.FSharp.*";
                "..\..\out\lib\Profile344\MathNet.Numerics.*", Some libpcl344, Some "**\MathNet.Numerics.FSharp.*" ] }

let fsharpPack =
    { numericsPack with Id = "MathNet.Numerics.FSharp"
                        Title = "Math.NET Numerics for F#"
                        Summary = "F# Modules for " + summary
                        Description = description + supportFsharp
                        Tags = "fsharp F# " + tags
                        Dependencies = [ "MathNet.Numerics" ]
                        Files = [ "..\..\out\lib\Net40\MathNet.Numerics.FSharp.*", Some libnet40, None;
                                  "..\..\out\lib\Profile47\MathNet.Numerics.FSharp.*", Some libpcl47, None;
                                  "..\..\out\lib\Profile344\MathNet.Numerics.FSharp.*", Some libpcl344, None ] }

let numericsSignedPack =
    { numericsPack with Id = numericsPack.Id + ".Signed"
                        Title = numericsPack.Title + " - Signed Edition"
                        Description = description + supportSigned
                        Tags = numericsPack.Tags + " signed"
                        Files = [ "..\..\out\lib-signed\Net40\MathNet.Numerics.*", Some libnet40, Some "**\MathNet.Numerics.FSharp.*" ] }

let fsharpSignedPack =
    { fsharpPack with Id = fsharpPack.Id + ".Signed"
                      Title = fsharpPack.Title + " - Signed Edition"
                      Description = description + supportSigned
                      Tags = fsharpPack.Tags + " signed"
                      Dependencies = [ "MathNet.Numerics.Signed" ]
                      Files = [ "..\..\out\lib-signed\Net40\MathNet.Numerics.FSharp.*", Some libnet40, None ] }


// PREPARE

Target "Start" DoNothing
Target "Clean" (fun _ -> CleanDirs ["out"; "obj"; "temp"])
Target "RestorePackages" RestorePackages

Target "AssemblyInfo" (fun _ ->
    BulkReplaceAssemblyInfoVersions "src/" (fun f ->
        { f with
            AssemblyVersion = assemblyVersion
            AssemblyFileVersion = assemblyVersion
            AssemblyInformationalVersion = packageVersion }))

Target "Prepare" DoNothing

"Start"
  =?> ("Clean", not (hasBuildParam "incremental"))
  ==> "RestorePackages"
  ==> "AssemblyInfo"
  ==> "Prepare"


// BUILD

let buildConfig config subject = MSBuild "" (if hasBuildParam "incremental" then "Build" else "Rebuild") [ "Configuration", config ] subject |> ignore
let build subject = buildConfig "Release" subject
let buildSigned subject = buildConfig "Release-Signed" subject

Target "BuildMain" (fun _ -> build !! "MathNet.Numerics.sln")
Target "BuildNet35" (fun _ -> build !! "MathNet.Numerics.Net35Only.sln")
Target "BuildAll" (fun _ -> build !! "MathNet.Numerics.All.sln")
Target "BuildSigned" (fun _ -> buildSigned !! "MathNet.Numerics.sln")

Target "Build" DoNothing
"Prepare"
  =?> ("BuildNet35", hasBuildParam "net35")
  =?> ("BuildSigned", hasBuildParam "signed" || hasBuildParam "release")
  =?> ("BuildAll", hasBuildParam "all" || hasBuildParam "release")
  =?> ("BuildMain", not (hasBuildParam "all" || hasBuildParam "release" || hasBuildParam "net35" || hasBuildParam "signed"))
  ==> "Build"


// NATIVE BUILD

let buildx86 subject = MSBuild "" (if hasBuildParam "incremental" then "Build" else "Rebuild") [("Configuration","Release"); ("Platform","Win32")] subject |> ignore
let buildx64 subject = MSBuild "" (if hasBuildParam "incremental" then "Build" else "Rebuild") [("Configuration","Release"); ("Platform","x64")] subject |> ignore

Target "BuildNativex86" (fun _ -> buildx86 !! "MathNet.Numerics.NativeProviders.sln")
Target "BuildNativex64" (fun _ -> buildx64 !! "MathNet.Numerics.NativeProviders.sln")
Target "BuildNative" DoNothing

"Prepare" ==> "BuildNativex86" ==> "BuildNative"
"Prepare" ==> "BuildNativex64" ==> "BuildNative"


// TEST

let test target =
    NUnit (fun p ->
        { p with
            DisableShadowCopy = true
            TimeOut = TimeSpan.FromMinutes 30.
            OutputFile = "TestResults.xml" }) target

Target "Test" (fun _ -> test !! "out/test/**/*UnitTests*.dll")
"Build" ==> "Test"


// NATIVE TEST

Target "TestNativex86" (fun _ ->
    !! "out/MKL/Windows/x86/*UnitTests*.dll"
    |> NUnit (fun p ->
        { p with
            ToolName = "nunit-console-x86.exe"
            DisableShadowCopy = true
            TimeOut = TimeSpan.FromMinutes 30.
            OutputFile = "TestResults.xml" }))
Target "TestNativex64" (fun _ ->
    !! "out/MKL/Windows/x64/*UnitTests*.dll"
    |> NUnit (fun p ->
        { p with
            DisableShadowCopy = true
            TimeOut = TimeSpan.FromMinutes 30.
            OutputFile = "TestResults.xml" }))
Target "TestNative" DoNothing

"BuildNativex86" ==> "TestNativex86" ==> "TestNative"
"BuildNativex64" ==> "TestNativex64" ==> "TestNative"


// DOCUMENTATION

Target "CleanDocs" (fun _ -> CleanDirs ["out/docs"])

Target "Docs" (fun _ ->
    executeFSIWithArgs "docs/tools" "build-docs.fsx" ["--define:RELEASE"] [] |> ignore
)

Target "DocsDev" (fun _ ->
    executeFSIWithArgs "docs/tools" "build-docs.fsx" [] [] |> ignore
)

"Build" ==> "CleanDocs" ==> "Docs"

"Start"
  =?> ("CleanDocs", not (hasBuildParam "incremental"))
  ==> "DocsDev"


// API REFERENCE

Target "CleanApi" (fun _ -> CleanDirs ["out/api"])

Target "Api" (fun _ ->
    !! "out/lib/Net40/MathNet.Numerics.dll"
    |> Docu (fun p ->
        { p with
            ToolPath = "tools/docu/docu.exe"
            TemplatesPath = "tools/docu/templates/"
            TimeOut = TimeSpan.FromMinutes 10.
            OutputPath = "out/api/" }))

"Build" ==> "CleanApi" ==> "Api"


// ZIP

Target "Zip" (fun _ ->
    CleanDir "out/packages/Zip"
    CleanDir "obj/Zip"
    if not (hasBuildParam "signed") || hasBuildParam "release" then
        CopyDir "obj/Zip/MathNet.Numerics" "out/lib" (fun f -> f.Contains("MathNet.Numerics."))
        Zip "obj/Zip/" (sprintf "out/packages/Zip/MathNet.Numerics-%s.zip" packageVersion) !! "obj/Zip/MathNet.Numerics/**/*.*"
    if hasBuildParam "signed" || hasBuildParam "release" then
        CopyDir "obj/Zip/MathNet.Numerics.Signed" "out/lib-signed" (fun f -> f.Contains("MathNet.Numerics."))
        Zip "obj/Zip/" (sprintf "out/packages/Zip/MathNet.Numerics.Signed-%s.zip" packageVersion) !! "obj/Zip/MathNet.Numerics.Signed/**/*.*"
    CleanDir "obj/Zip")

"Build" ==> "Zip"


// NUGET

let nugetPack pack =
    CleanDir "obj/NuGet"
    CopyFile "obj/NuGet/license.txt" "LICENSE.md"
    CopyFile "obj/NuGet/readme.txt" "RELEASENOTES.md"
    NuGet (fun p ->
        { p with ToolPath = "tools/NuGet/NuGet.exe"
                 OutputPath = "out/packages/NuGet"
                 WorkingDir = "obj/NuGet"
                 Version = packageVersion
                 ReleaseNotes = releaseNotes
                 Project = pack.Id
                 Title = pack.Title
                 Summary = pack.Summary
                 Description = pack.Description
                 Tags = pack.Tags
                 Authors = pack.Authors
                 Dependencies = pack.Dependencies |> List.map (fun p -> (p, packageVersion))
                 Files = [ "license.txt", None, None; "readme.txt", None, None; ] @ pack.Files
                 Publish = false }) "build/NuGet/MathNet.Numerics.nuspec"

Target "NuGet" (fun _ ->
    CleanDir "out/packages/NuGet"
    if hasBuildParam "signed" || hasBuildParam "release" then
        nugetPack numericsSignedPack
        nugetPack fsharpSignedPack
    if hasBuildParam "all" || hasBuildParam "release" then
        nugetPack numericsPack
        nugetPack fsharpPack
    CleanDir "obj/NuGet")

"Build" ==> "NuGet"


// RUN

Target "All" DoNothing

"Build" ==> "All"
"Zip" ==> "All"
"NuGet" ==> "All"
"Docs" ==> "All"
"Api" ==> "All"
"Test" ==> "All"

RunTargetOrDefault "Test"
