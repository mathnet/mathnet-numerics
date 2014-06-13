//  __  __       _   _       _   _ ______ _______
// |  \/  |     | | | |     | \ | |  ____|__   __|
// | \  / | __ _| |_| |__   |  \| | |__     | |
// | |\/| |/ _` | __| '_ \  | . ` |  __|    | |
// | |  | | (_| | |_| | | |_| |\  | |____   | |
// |_|  |_|\__,_|\__|_| |_(_)_| \_|______|  |_|
//
// Math.NET Numerics - http://numerics.mathdotnet.com
// Copyright (c) Math.NET - Open Source MIT/X11 License
// FAKE build script, see http://fsharp.github.io/FAKE
//


// --------------------------------------------------------------------------------------
// Prelude
// --------------------------------------------------------------------------------------

#I "packages/FAKE/tools"
#r "packages/FAKE/tools/FakeLib.dll"

open Fake
open Fake.DocuHelper
open Fake.AssemblyInfoFile
open Fake.ReleaseNotesHelper
open System

Environment.CurrentDirectory <- __SOURCE_DIRECTORY__

Target "Start" DoNothing
Target "Clean" (fun _ ->
    CleanDirs [ "obj" ]
    CleanDirs [ "out/api"; "out/docs"; "out/packages" ]
    CleanDirs [ "out/lib/Net35"; "out/lib/Net40"; "out/lib/Profile47"; "out/lib/Profile344" ]
    CleanDirs [ "out/test/Net35"; "out/test/Net40"; "out/test/Profile47"; "out/test/Profile344" ]
    CleanDirs [ "out/lib-signed/Net40" ]
    CleanDirs [ "out/test-signed/Net40" ])
Target "RestorePackages" RestorePackages
Target "Prepare" DoNothing

"Start"
  =?> ("Clean", not (hasBuildParam "incremental"))
  ==> "RestorePackages"
  ==> "Prepare"


// --------------------------------------------------------------------------------------
// Math.NET Numerics Core
// --------------------------------------------------------------------------------------

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
                "..\..\out\lib\Profile344\MathNet.Numerics.*", Some libpcl344, Some "**\MathNet.Numerics.FSharp.*";
                "..\..\src\Numerics\**\*.cs", Some "src/Common", None ] }

let fsharpPack =
    { numericsPack with Id = "MathNet.Numerics.FSharp"
                        Title = "Math.NET Numerics for F#"
                        Summary = "F# Modules for " + summary
                        Description = description + supportFsharp
                        Tags = "fsharp F# " + tags
                        Dependencies = [ "MathNet.Numerics" ]
                        Files = [ "..\..\out\lib\Net40\MathNet.Numerics.FSharp.*", Some libnet40, None;
                                  "..\..\out\lib\Profile47\MathNet.Numerics.FSharp.*", Some libpcl47, None;
                                  "..\..\out\lib\Profile344\MathNet.Numerics.FSharp.*", Some libpcl344, None;
                                  "..\..\src\FSharp\**\*.fs", Some "src/Common", None ] }

let numericsSignedPack =
    { numericsPack with Id = numericsPack.Id + ".Signed"
                        Title = numericsPack.Title + " - Signed Edition"
                        Description = description + supportSigned
                        Tags = numericsPack.Tags + " signed"
                        Files = [ "..\..\out\lib-signed\Net40\MathNet.Numerics.*", Some libnet40, Some "**\MathNet.Numerics.FSharp.*";
                                  "..\..\src\Numerics\**\*.cs", Some "src/Common", None ] }

let fsharpSignedPack =
    { fsharpPack with Id = fsharpPack.Id + ".Signed"
                      Title = fsharpPack.Title + " - Signed Edition"
                      Description = description + supportSigned
                      Tags = fsharpPack.Tags + " signed"
                      Dependencies = [ "MathNet.Numerics.Signed" ]
                      Files = [ "..\..\out\lib-signed\Net40\MathNet.Numerics.FSharp.*", Some libnet40, None;
                                "..\..\src\FSharp\**\*.fs", Some "src/Common", None ] }


// VERSION

Target "AssemblyInfo" (fun _ ->
    BulkReplaceAssemblyInfoVersions "src/" (fun f ->
        { f with
            AssemblyVersion = assemblyVersion
            AssemblyFileVersion = assemblyVersion
            AssemblyInformationalVersion = packageVersion }))

"Prepare" ==> "AssemblyInfo"


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
  ==> "AssemblyInfo"
  =?> ("BuildNet35", hasBuildParam "net35")
  =?> ("BuildSigned", hasBuildParam "signed" || hasBuildParam "release")
  =?> ("BuildAll", hasBuildParam "all" || hasBuildParam "release")
  =?> ("BuildMain", not (hasBuildParam "all" || hasBuildParam "release" || hasBuildParam "net35" || hasBuildParam "signed"))
  ==> "Build"


// TEST

let test target =
    let quick p = if hasBuildParam "quick" then { p with ExcludeCategory="LongRunning" } else p
    NUnit (fun p ->
        { p with
            DisableShadowCopy = true
            TimeOut = TimeSpan.FromMinutes 30.
            OutputFile = "TestResults.xml" } |> quick) target

Target "Test" (fun _ -> test !! "out/test/**/*UnitTests*.dll")
"Build" ==> "Test"


// --------------------------------------------------------------------------------------
// Math.NET Numerics Native Providers
// --------------------------------------------------------------------------------------

// BUILD

let buildx86 subject = MSBuild "" (if hasBuildParam "incremental" then "Build" else "Rebuild") [("Configuration","Release"); ("Platform","Win32")] subject |> ignore
let buildx64 subject = MSBuild "" (if hasBuildParam "incremental" then "Build" else "Rebuild") [("Configuration","Release"); ("Platform","x64")] subject |> ignore

Target "BuildNativex86" (fun _ -> buildx86 !! "MathNet.Numerics.NativeProviders.sln")
Target "BuildNativex64" (fun _ -> buildx64 !! "MathNet.Numerics.NativeProviders.sln")
Target "BuildNative" DoNothing

"Prepare" ==> "BuildNativex86" ==> "BuildNative"
"Prepare" ==> "BuildNativex64" ==> "BuildNative"


// TEST

Target "TestNativex86" (fun _ ->
    ActivateFinalTarget "CloseTestRunner"
    !! "out/MKL/Windows/x86/*UnitTests*.dll"
    |> NUnit (fun p ->
        { p with
            ToolName = "nunit-console-x86.exe"
            DisableShadowCopy = true
            TimeOut = TimeSpan.FromMinutes 30.
            OutputFile = "TestResults.xml" }))
Target "TestNativex64" (fun _ ->
    ActivateFinalTarget "CloseTestRunner"
    !! "out/MKL/Windows/x64/*UnitTests*.dll"
    |> NUnit (fun p ->
        { p with
            ToolName = "nunit-console.exe"
            DisableShadowCopy = true
            TimeOut = TimeSpan.FromMinutes 30.
            OutputFile = "TestResults.xml" }))
Target "TestNative" DoNothing

FinalTarget "CloseTestRunner" (fun _ ->
    ProcessHelper.killProcess "nunit-agent.exe"
    ProcessHelper.killProcess "nunit-agent-x86.exe"
)

"BuildNativex86" ==> "TestNativex86" ==> "TestNative"
"BuildNativex64" ==> "TestNativex64" ==> "TestNative"


// --------------------------------------------------------------------------------------
// Documentation
// --------------------------------------------------------------------------------------

// DOCS

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


// --------------------------------------------------------------------------------------
// Packages
// --------------------------------------------------------------------------------------

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

    let update p =
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
                 SymbolPackage = NugetSymbolPackage.Nuspec
                 Files = [ "license.txt", None, None; "readme.txt", None, None; ] @ pack.Files
                 Publish = false }

    NuGet (fun p -> update p) "build/MathNet.Numerics.nuspec"

    NuGet (fun p ->
        let p' = update p in
        { p' with Files = p'.Files |> List.choose (function
                                                   | (_, Some target, _) when target.StartsWith("src") -> None
                                                   | (s, t, None) -> Some (s, t, Some ("**/*.pdb"))
                                                   | (s, t, Some e) -> Some (s, t, Some (e + ";**/*.pdb")))
                  SymbolPackage = NugetSymbolPackage.None })
        "build/MathNet.Numerics.nuspec"


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


// --------------------------------------------------------------------------------------
// Release/Publishing
// These targets are interesting only for maintainers and require write permissions
// --------------------------------------------------------------------------------------

Target "PublishTag" (fun _ ->
    // inspired by Deedle/tpetricek
    // create tag with release notes
    let tagName = "v" + packageVersion
    let cmd = sprintf """tag -a %s -m "%s" """ tagName releaseNotes
    Git.CommandHelper.runSimpleGitCommand "." cmd |> printfn "%s"
    // push tag
    let _, remotes, _ = Git.CommandHelper.runGitCommand "." "remote -v"
    let main = remotes |> Seq.find (fun s -> s.Contains("(push)") && s.Contains("mathnet/mathnet-numerics"))
    let remoteName = main.Split('\t').[0]
    Git.Branches.pushTag "." remoteName tagName)

Target "PublishDocs" (fun _ ->
    let repo = "../mathnet-websites"
    Git.Branches.pull repo "origin" "master"
    CleanDir "../mathnet-websites/numerics/docs"
    CopyRecursive "out/docs" "../mathnet-websites/numerics/docs" true |> printfn "%A"
    Git.Staging.StageAll repo
    Git.Commit.Commit repo (sprintf "Numerics: %s docs update" packageVersion)
    Git.Branches.pushBranch repo "origin" "master")

Target "PublishApi" (fun _ ->
    let repo = "../mathnet-websites"
    Git.Branches.pull repo "origin" "master"
    CleanDir "../mathnet-websites/api-numerics"
    CopyRecursive "out/api" "../mathnet-websites/api-numerics" true |> printfn "%A"
    Git.Staging.StageAll repo
    Git.Commit.Commit repo (sprintf "Numerics: %s api update" packageVersion)
    Git.Branches.pushBranch repo "origin" "master")

Target "Publish" DoNothing
"PublishTag" ==> "Publish"
"PublishDocs" ==> "Publish"
"PublishApi" ==> "Publish"


// --------------------------------------------------------------------------------------
// Default Targets
// --------------------------------------------------------------------------------------

Target "All" DoNothing

"Build" ==> "All"
"Zip" ==> "All"
"NuGet" ==> "All"
"Docs" ==> "All"
"Api" ==> "All"
"Test" ==> "All"

RunTargetOrDefault "Test"
