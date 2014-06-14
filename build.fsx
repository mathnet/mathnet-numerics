//  __  __       _   _       _   _ ______ _______
// |  \/  |     | | | |     | \ | |  ____|__   __|
// | \  / | __ _| |_| |__   |  \| | |__     | |
// | |\/| |/ _` | __| '_ \  | . ` |  __|    | |
// | |  | | (_| | |_| | | |_| |\  | |____   | |
// |_|  |_|\__,_|\__|_| |_(_)_| \_|______|  |_|
//
// Math.NET Numerics - http://numerics.mathdotnet.com
// Copyright (c) Math.NET - Open Source MIT/X11 License
//
// FAKE build script, see http://fsharp.github.io/FAKE
//

// --------------------------------------------------------------------------------------
// PRELUDE
// --------------------------------------------------------------------------------------

#I "packages/FAKE/tools"
#r "packages/FAKE/tools/FakeLib.dll"

open Fake
open Fake.DocuHelper
open Fake.AssemblyInfoFile
open Fake.ReleaseNotesHelper
open Fake.StringHelper
open System

Environment.CurrentDirectory <- __SOURCE_DIRECTORY__
let header = ReadFile(__SOURCE_DIRECTORY__ @@ "build.fsx") |> Seq.take 10 |> Seq.map (fun s -> s.Substring(2)) |> toLines
trace header


// --------------------------------------------------------------------------------------
// PROJECT INFO
// --------------------------------------------------------------------------------------

// CORE PACKAGES

type Package =
    { Id: string
      Version: string
      Title: string
      Summary: string
      Description: string
      ReleaseNotes: string
      Tags: string
      Authors: string list
      Dependencies: (string*string) list
      Files: (string * string option * string option) list }

let release = LoadReleaseNotes "RELEASENOTES.md"
let buildPart = "0"
let assemblyVersion = release.AssemblyVersion + "." + buildPart
let packageVersion = release.NugetVersion
let releaseNotes = release.Notes |> List.map (fun l -> l.Replace("*","").Replace("`","")) |> toLines
trace (sprintf " Math.NET Numerics                    v%s" release.NugetVersion)

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
      Version = packageVersion
      Title = "Math.NET Numerics"
      Summary = summary
      Description = description + support
      ReleaseNotes = releaseNotes
      Tags = tags
      Authors = [ "Christoph Ruegg"; "Marcus Cuda"; "Jurgen Van Gael" ]
      Dependencies = []
      Files = [ @"..\..\out\lib\Net35\MathNet.Numerics.*", Some libnet35, Some @"**\MathNet.Numerics.FSharp.*";
                @"..\..\out\lib\Net40\MathNet.Numerics.*", Some libnet40, Some @"**\MathNet.Numerics.FSharp.*";
                @"..\..\out\lib\Profile47\MathNet.Numerics.*", Some libpcl47, Some @"**\MathNet.Numerics.FSharp.*";
                @"..\..\out\lib\Profile344\MathNet.Numerics.*", Some libpcl344, Some @"**\MathNet.Numerics.FSharp.*";
                @"..\..\src\Numerics\**\*.cs", Some "src/Common", None ] }

let fsharpPack =
    { numericsPack with Id = "MathNet.Numerics.FSharp"
                        Title = "Math.NET Numerics for F#"
                        Summary = "F# Modules for " + summary
                        Description = description + supportFsharp
                        Tags = "fsharp F# " + tags
                        Dependencies = [ "MathNet.Numerics", packageVersion ]
                        Files = [ @"..\..\out\lib\Net40\MathNet.Numerics.FSharp.*", Some libnet40, None;
                                  @"..\..\out\lib\Profile47\MathNet.Numerics.FSharp.*", Some libpcl47, None;
                                  @"..\..\out\lib\Profile344\MathNet.Numerics.FSharp.*", Some libpcl344, None;
                                  @"..\..\src\FSharp\**\*.fs", Some "src/Common", None ] }

let numericsSignedPack =
    { numericsPack with Id = numericsPack.Id + ".Signed"
                        Title = numericsPack.Title + " - Signed Edition"
                        Description = description + supportSigned
                        Tags = numericsPack.Tags + " signed"
                        Files = [ @"..\..\out\lib-signed\Net40\MathNet.Numerics.*", Some libnet40, Some @"**\MathNet.Numerics.FSharp.*";
                                  @"..\..\src\Numerics\**\*.cs", Some "src/Common", None ] }

let fsharpSignedPack =
    { fsharpPack with Id = fsharpPack.Id + ".Signed"
                      Title = fsharpPack.Title + " - Signed Edition"
                      Description = description + supportSigned
                      Tags = fsharpPack.Tags + " signed"
                      Dependencies = [ "MathNet.Numerics.Signed", packageVersion ]
                      Files = [ @"..\..\out\lib-signed\Net40\MathNet.Numerics.FSharp.*", Some libnet40, None;
                                @"..\..\src\FSharp\**\*.fs", Some "src/Common", None ] }


// NATIVE PROVIDER PACKAGES

let nativeRelease = LoadReleaseNotes "RELEASENOTES-Native.md"
let nativeBuildPart = "0"
let nativeAssemblyVersion = nativeRelease.AssemblyVersion + "." + nativeBuildPart
let nativePackageVersion = nativeRelease.NugetVersion
let nativeReleaseNotes = nativeRelease.Notes |> List.map (fun l -> l.Replace("*","").Replace("`","")) |> toLines
trace (sprintf " Math.NET Numerics Native Providers   v%s" nativeRelease.NugetVersion)
trace ""

let nativeSummary = "Intel MKL native libraries for Math.NET Numerics. Requires an Intel MKL license if redistributed."

let nativeMKLWin32Pack =
    { Id = "MathNet.Numerics.MKL.Win-x86"
      Version = nativePackageVersion
      Title = "Math.NET Numerics - MKL Native Libraries (Windows 32-bit)"
      Summary = nativeSummary
      Description = nativeSummary
      ReleaseNotes = nativeReleaseNotes
      Tags = "math numeric statistics probability integration interpolation linear algebra matrix fft native mkl x86"
      Authors = [ "Christoph Ruegg"; "Marcus Cuda"; "Jurgen Van Gael" ]
      Dependencies = [ "MathNet.Numerics", "2.4.0" ]
      Files = [ @"..\..\out\MKL\Windows\x86\libiomp5md.dll", Some "content", None;
                @"..\..\out\MKL\Windows\x86\MathNet.Numerics.MKL.dll", Some "content", None ] }

let nativeMKLWin64Pack =
    { nativeMKLWin32Pack with Id = "MathNet.Numerics.MKL.Win-x64"
                              Title = "Math.NET Numerics - MKL Native Libraries (Windows 64-bit)"
                              Files = [ @"..\..\out\MKL\Windows\x64\libiomp5md.dll", Some "content", None;
                                        @"..\..\out\MKL\Windows\x64\MathNet.Numerics.MKL.dll", Some "content", None ] }


// --------------------------------------------------------------------------------------
// PREPARE
// --------------------------------------------------------------------------------------

Target "Start" DoNothing

Target "Clean" (fun _ ->
    CleanDirs [ "obj" ]
    CleanDirs [ "out/api"; "out/docs"; "out/packages" ]
    CleanDirs [ "out/lib/Net35"; "out/lib/Net40"; "out/lib/Profile47"; "out/lib/Profile344" ]
    CleanDirs [ "out/test/Net35"; "out/test/Net40"; "out/test/Profile47"; "out/test/Profile344" ]
    CleanDirs [ "out/lib-signed/Net40" ]
    CleanDirs [ "out/test-signed/Net40" ])

Target "NativeClean" (fun _ -> CleanDirs [ "out/MKL"; "out/ATLAS" ] )

Target "RestorePackages" RestorePackages

Target "ApplyVersion" (fun _ ->
    BulkReplaceAssemblyInfoVersions "src/" (fun f ->
        { f with
            AssemblyVersion = assemblyVersion
            AssemblyFileVersion = assemblyVersion
            AssemblyInformationalVersion = packageVersion })
    ReplaceInFile
        (regex_replace "\d+\.\d+\.\d+\.\d+" nativeAssemblyVersion
         >> regex_replace "\d+,\d+,\d+,\d+" (replace "." "," nativeAssemblyVersion))
        "src/NativeProviders/Common/resource.rc")

Target "Prepare" DoNothing
"Start"
  =?> ("Clean", not (hasBuildParam "incremental"))
  ==> "RestorePackages"
  ==> "ApplyVersion"
  ==> "Prepare"


// --------------------------------------------------------------------------------------
// BUILD
// --------------------------------------------------------------------------------------

let buildConfig config subject = MSBuild "" (if hasBuildParam "incremental" then "Build" else "Rebuild") [ "Configuration", config ] subject |> ignore
let build subject = buildConfig "Release" subject
let buildSigned subject = buildConfig "Release-Signed" subject
let native32Build subject = MSBuild "" (if hasBuildParam "incremental" then "Build" else "Rebuild") [("Configuration","Release"); ("Platform","Win32")] subject |> ignore
let native64Build subject = MSBuild "" (if hasBuildParam "incremental" then "Build" else "Rebuild") [("Configuration","Release"); ("Platform","x64")] subject |> ignore

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

Target "Native32Build" (fun _ -> native32Build !! "MathNet.Numerics.NativeProviders.sln")
Target "Native64Build" (fun _ -> native64Build !! "MathNet.Numerics.NativeProviders.sln")

Target "NativeBuild" DoNothing
"Prepare"
  =?> ("NativeClean", not (hasBuildParam "incremental"))
  ==> "Native32Build"
  ==> "NativeBuild"
"Prepare"
  =?> ("NativeClean", not (hasBuildParam "incremental"))
  ==> "NativeVersion"
  ==> "Native64Build"
  ==> "NativeBuild"


// --------------------------------------------------------------------------------------
// TEST
// --------------------------------------------------------------------------------------

let test target =
    let quick p = if hasBuildParam "quick" then { p with ExcludeCategory="LongRunning" } else p
    NUnit (fun p ->
        { p with
            DisableShadowCopy = true
            TimeOut = TimeSpan.FromMinutes 30.
            OutputFile = "TestResults.xml" } |> quick) target

Target "Test" (fun _ -> test !! "out/test/**/*UnitTests*.dll")
"Build" ==> "Test"

Target "Native32Test" (fun _ ->
    ActivateFinalTarget "CloseTestRunner"
    !! "out/MKL/Windows/x86/*UnitTests*.dll"
    |> NUnit (fun p ->
        { p with
            ToolName = "nunit-console-x86.exe"
            DisableShadowCopy = true
            TimeOut = TimeSpan.FromMinutes 30.
            OutputFile = "TestResults.xml" }))
Target "Native64Test" (fun _ ->
    ActivateFinalTarget "CloseTestRunner"
    !! "out/MKL/Windows/x64/*UnitTests*.dll"
    |> NUnit (fun p ->
        { p with
            ToolName = "nunit-console.exe"
            DisableShadowCopy = true
            TimeOut = TimeSpan.FromMinutes 30.
            OutputFile = "TestResults.xml" }))
Target "NativeTest" DoNothing

FinalTarget "CloseTestRunner" (fun _ ->
    ProcessHelper.killProcess "nunit-agent.exe"
    ProcessHelper.killProcess "nunit-agent-x86.exe"
)

"Native32Build" ==> "Native32Test" ==> "NativeTest"
"Native64Build" ==> "Native64Test" ==> "NativeTest"


// --------------------------------------------------------------------------------------
// PACKAGES
// --------------------------------------------------------------------------------------

let provideLicenseReadme license releasenotes path =
    let licensePath = path @@ "license.txt"
    let readmePath = path @@ "readme.txt"
    CopyFile licensePath license
    CopyFile readmePath releasenotes
    ConvertFileToWindowsLineBreaks licensePath
    header + Environment.NewLine + (ReadFileAsString readmePath)
    |> ConvertTextToWindowsLineBreaks 
    |> ReplaceFile readmePath

// ZIP

Target "Zip" (fun _ ->
    CleanDir "out/packages/Zip"
    CleanDir "obj/Zip"
    if not (hasBuildParam "signed") || hasBuildParam "release" then
        CopyDir "obj/Zip/MathNet.Numerics" "out/lib" (fun f -> f.Contains("MathNet.Numerics."))
        provideLicenseReadme "LICENSE.md" "RELEASENOTES.md" "obj/Zip/MathNet.Numerics"
        Zip "obj/Zip/" (sprintf "out/packages/Zip/MathNet.Numerics-%s.zip" packageVersion) !! "obj/Zip/MathNet.Numerics/**/*.*"
    if hasBuildParam "signed" || hasBuildParam "release" then
        CopyDir "obj/Zip/MathNet.Numerics.Signed" "out/lib-signed" (fun f -> f.Contains("MathNet.Numerics."))
        provideLicenseReadme "LICENSE.md" "RELEASENOTES.md" "obj/Zip/MathNet.Numerics.Signed"
        Zip "obj/Zip/" (sprintf "out/packages/Zip/MathNet.Numerics.Signed-%s.zip" packageVersion) !! "obj/Zip/MathNet.Numerics.Signed/**/*.*"
    CleanDir "obj/Zip")

"Build" ==> "Zip"

Target "NativeZip" (fun _ ->
    CleanDir "out/MKL/packages/Zip"
    CleanDir "obj/Zip"
    CopyDir "obj/Zip/MathNet.Numerics.MKL" "out/MKL" (fun f -> f.Contains("MathNet.Numerics.MKL.") || f.Contains("libiomp5md.dll"))
    provideLicenseReadme "LICENSE.md" "RELEASENOTES-Native.md" "obj/Zip/MathNet.Numerics.MKL"
    Zip "obj/Zip/" (sprintf "out/MKL/packages/Zip/MathNet.Numerics.MKL-%s.zip" nativePackageVersion) !! "obj/Zip/MathNet.Numerics.MKL/**/*.*"
    CleanDir "obj/Zip")

"NativeBuild" ==> "NativeZip"

// NUGET

let updateNuspec pack outPath symbols updateFiles spec =
    { spec with ToolPath = "tools/NuGet/NuGet.exe"
                OutputPath = outPath
                WorkingDir = "obj/NuGet"
                Version = pack.Version
                ReleaseNotes = pack.ReleaseNotes
                Project = pack.Id
                Title = pack.Title
                Summary = pack.Summary
                Description = pack.Description
                Tags = pack.Tags
                Authors = pack.Authors
                Dependencies = pack.Dependencies
                SymbolPackage = symbols
                Files = updateFiles pack.Files
                Publish = false }

let nugetPack pack outPath =
    CleanDir "obj/NuGet"
    provideLicenseReadme "LICENSE.md" "RELEASENOTES.md" "obj/NuGet"
    let withLicenseReadme f = [ "license.txt", None, None; "readme.txt", None, None; ] @ f
    let withoutSymbolsSources f =
        List.choose (function | (_, Some (target:string), _) when target.StartsWith("src") -> None
                              | (s, t, None) -> Some (s, t, Some ("**/*.pdb"))
                              | (s, t, Some e) -> Some (s, t, Some (e + ";**/*.pdb"))) f
    NuGet (updateNuspec pack outPath NugetSymbolPackage.Nuspec withLicenseReadme) "build/MathNet.Numerics.nuspec"
    NuGet (updateNuspec pack outPath NugetSymbolPackage.None (withLicenseReadme >> withoutSymbolsSources)) "build/MathNet.Numerics.nuspec"
    CleanDir "obj/NuGet"

Target "NuGet" (fun _ ->
    let outPath = "out/packages/NuGet"
    CleanDir outPath
    if hasBuildParam "signed" || hasBuildParam "release" then
        nugetPack numericsSignedPack outPath
        nugetPack fsharpSignedPack outPath
    if hasBuildParam "all" || hasBuildParam "release" then
        nugetPack numericsPack outPath
        nugetPack fsharpPack outPath
    CleanDir "obj/NuGet")

"Build" ==> "NuGet"

let nativeNugetPack pack outPath =
    CleanDir "obj/NuGet"
    provideLicenseReadme "LICENSE.md" "RELEASENOTES-Native.md" "obj/NuGet"
    let withLicenseReadme f = [ "license.txt", None, None; "readme.txt", None, None; ] @ f
    NuGet (updateNuspec pack outPath NugetSymbolPackage.None withLicenseReadme) "build/MathNet.Numerics.Native.nuspec"
    CleanDir "obj/NuGet"

Target "NativeNuGet" (fun _ ->
    let outPath = "out/MKL/packages/NuGet"
    CleanDir outPath
    nativeNugetPack nativeMKLWin32Pack outPath
    nativeNugetPack nativeMKLWin64Pack outPath
    CleanDir "obj/NuGet")

"NativeBuild" ==> "NativeNuGet"


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
// Release/Publishing
// Requires permissions; intended only for maintainers
// --------------------------------------------------------------------------------------

let pushGitTag tag =
    let _, remotes, _ = Git.CommandHelper.runGitCommand "." "remote -v"
    let main = remotes |> Seq.find (fun s -> s.Contains("(push)") && s.Contains("mathnet/mathnet-numerics"))
    let remoteName = main.Split('\t').[0]
    Git.Branches.pushTag "." remoteName tag

Target "PublishTag" (fun _ ->
    // inspired by Deedle/tpetricek
    let tagName = "v" + packageVersion
    let tagMessage = String.concat Environment.NewLine ["Math.NET Numerics v" + packageVersion; ""; releaseNotes ]
    let cmd = sprintf """tag -a %s -m "%s" """ tagName tagMessage
    Git.CommandHelper.runSimpleGitCommand "." cmd |> printfn "%s"
    pushGitTag tagName)

Target "NativePublishTag" (fun _ ->
    // create tag with release notes
    let tagName = "native-v" + nativePackageVersion
    let tagMessage = String.concat Environment.NewLine ["Math.NET Numerics Native Providers v" + nativePackageVersion; ""; nativeReleaseNotes ]
    let cmd = sprintf """tag -a %s -m "%s" """ tagName tagMessage
    Git.CommandHelper.runSimpleGitCommand "." cmd |> printfn "%s"
    pushGitTag tagName)

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

Target "NativePublish" DoNothing
"NativePublishTag" ==> "NativePublish"
"PublishDocs" ==> "NativePublish"
"PublishApi" ==> "NativePublish"


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

Target "NativeAll" DoNothing
"NativeBuild" ==> "NativeAll"
"NativeZip" ==> "NativeAll"
"NativeNuGet" ==> "NativeAll"
"NativeTest" ==> "NativeAll"

RunTargetOrDefault "Test"
