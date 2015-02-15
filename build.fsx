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
open System.IO

Environment.CurrentDirectory <- __SOURCE_DIRECTORY__
let header = ReadFile(__SOURCE_DIRECTORY__ @@ "build.fsx") |> Seq.take 10 |> Seq.map (fun s -> s.Substring(2)) |> toLines
trace header


// --------------------------------------------------------------------------------------
// PROJECT INFO
// --------------------------------------------------------------------------------------

// VERSION OVERVIEW

let release = LoadReleaseNotes "RELEASENOTES.md"
let buildPart = "0"
let assemblyVersion = release.AssemblyVersion + "." + buildPart
let packageVersion = release.NugetVersion
let releaseNotes = release.Notes |> List.map (fun l -> l.Replace("*","").Replace("`","")) |> toLines
trace (sprintf " Math.NET Numerics                    v%s" packageVersion)

let mklRelease = LoadReleaseNotes "RELEASENOTES-MKL.md"
let mklBuildPart = "0"
let mklAssemblyVersion = mklRelease.AssemblyVersion + "." + mklBuildPart
let mklPackageVersion = mklRelease.NugetVersion
let mklReleaseNotes = mklRelease.Notes |> List.map (fun l -> l.Replace("*","").Replace("`","")) |> toLines
trace (sprintf " Math.NET Numerics MKL Provider       v%s" mklPackageVersion)

let dataRelease = LoadReleaseNotes "RELEASENOTES-Data.md"
let dataBuildPart = "0"
let dataAssemblyVersion = dataRelease.AssemblyVersion + "." + dataBuildPart
let dataPackageVersion = dataRelease.NugetVersion
let dataReleaseNotes = dataRelease.Notes |> List.map (fun l -> l.Replace("*","").Replace("`","")) |> toLines
trace (sprintf " Math.NET Numerics Data Extensions    v%s" dataPackageVersion)
trace ""


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
      Dependencies: NugetFrameworkDependencies list
      Files: (string * string option * string option) list }

type Bundle =
    { Id: string
      Version: string
      Title: string
      ReleaseNotesFile: string
      FsLoader: bool
      Packages: Package list }

let summary = "Math.NET Numerics, providing methods and algorithms for numerical computations in science, engineering and every day use."
let description = "Math.NET Numerics is the numerical foundation of the Math.NET project, aiming to provide methods and algorithms for numerical computations in science, engineering and every day use. "
let support = "Supports .Net 4.0, .Net 3.5 and Mono on Windows, Linux and Mac; Silverlight 5, WindowsPhone/SL 8, WindowsPhone 8.1 and Windows 8 with PCL portable profiles 7, 47, 78, 259 and 328; Android/iOS with Xamarin."
let supportFsharp = "Supports F# 3.0 on .Net 4.0, .Net 3.5 and Mono on Windows, Linux and Mac; Silverlight 5 and Windows 8 with PCL portable profile 47; Android/iOS with Xamarin."
let supportSigned = "Supports .Net 4.0."
let tags = "math numeric statistics probability integration interpolation regression solve fit linear algebra matrix fft"

let libnet35 = "lib/net35"
let libnet40 = "lib/net40"
let libnet45 = "lib/net45"
let libpcl7 = "lib/portable-net45+netcore45+MonoAndroid1+MonoTouch1"
let libpcl47 = "lib/portable-net45+sl5+netcore45+MonoAndroid1+MonoTouch1"
let libpcl78 = "lib/portable-net45+netcore45+wp8+MonoAndroid1+MonoTouch1"
let libpcl259 = "lib/portable-net45+netcore45+wpa81+wp8+MonoAndroid1+MonoTouch1"
let libpcl328 = "lib/portable-net4+sl5+netcore45+wpa81+wp8+MonoAndroid1+MonoTouch1"

let numericsPack =
    { Id = "MathNet.Numerics"
      Version = packageVersion
      Title = "Math.NET Numerics"
      Summary = summary
      Description = description + support
      ReleaseNotes = releaseNotes
      Tags = tags
      Authors = [ "Christoph Ruegg"; "Marcus Cuda"; "Jurgen Van Gael" ]
      Dependencies =
        [ { FrameworkVersion="net35"
            Dependencies=[ "TaskParallelLibrary", GetPackageVersion "packages" "TaskParallelLibrary" ] }
          { FrameworkVersion="net40"
            Dependencies=[] } ]
      Files =
        [ @"..\..\out\lib\Net35\MathNet.Numerics.*", Some libnet35, Some @"**\MathNet.Numerics.FSharp.*";
          @"..\..\out\lib\Net40\MathNet.Numerics.*", Some libnet40, Some @"**\MathNet.Numerics.FSharp.*";
          @"..\..\out\lib\Profile7\MathNet.Numerics.*", Some libpcl7, Some @"**\MathNet.Numerics.FSharp.*";
          @"..\..\out\lib\Profile47\MathNet.Numerics.*", Some libpcl47, Some @"**\MathNet.Numerics.FSharp.*";
          @"..\..\out\lib\Profile78\MathNet.Numerics.*", Some libpcl78, Some @"**\MathNet.Numerics.FSharp.*";
          @"..\..\out\lib\Profile259\MathNet.Numerics.*", Some libpcl259, Some @"**\MathNet.Numerics.FSharp.*";
          @"..\..\out\lib\Profile328\MathNet.Numerics.*", Some libpcl328, Some @"**\MathNet.Numerics.FSharp.*";
          @"..\..\src\Numerics\**\*.cs", Some "src/Common", None ] }

let fsharpPack =
    { numericsPack with
        Id = "MathNet.Numerics.FSharp"
        Title = "Math.NET Numerics for F#"
        Summary = "F# Modules for " + summary
        Description = description + supportFsharp
        Tags = "fsharp F# " + tags
        Dependencies =
          [ { FrameworkVersion=""
              Dependencies=[ "MathNet.Numerics", RequireExactly packageVersion
                             "FSharp.Core", GetPackageVersion "packages" "FSharp.Core" ] } ]
        Files =
          [ @"..\..\out\lib\Net35\MathNet.Numerics.FSharp.*", Some libnet35, None;
            @"..\..\out\lib\Net40\MathNet.Numerics.FSharp.*", Some libnet40, None;
            @"..\..\out\lib\Profile47\MathNet.Numerics.FSharp.*", Some libpcl47, None;
            @"MathNet.Numerics.fsx", None, None;
            @"MathNet.Numerics.IfSharp.fsx", None, None;
            @"..\..\src\FSharp\**\*.fs", Some "src/Common", None ] }

let numericsSignedPack =
    { numericsPack with
        Id = numericsPack.Id + ".Signed"
        Title = numericsPack.Title + " - Signed Edition"
        Description = description + supportSigned
        Tags = numericsPack.Tags + " signed"
        Dependencies = []
        Files =
          [ @"..\..\out\lib-signed\Net40\MathNet.Numerics.*", Some libnet40, Some @"**\MathNet.Numerics.FSharp.*";
            @"..\..\src\Numerics\**\*.cs", Some "src/Common", None ] }

let fsharpSignedPack =
    { fsharpPack with
        Id = fsharpPack.Id + ".Signed"
        Title = fsharpPack.Title + " - Signed Edition"
        Description = description + supportSigned
        Tags = fsharpPack.Tags + " signed"
        Dependencies =
          [ { FrameworkVersion=""
              Dependencies=[ "MathNet.Numerics.Signed", RequireExactly packageVersion
                             "FSharp.Core", GetPackageVersion "packages" "FSharp.Core" ] } ]
        Files =
          [ @"..\..\out\lib-signed\Net40\MathNet.Numerics.FSharp.*", Some libnet40, None;
            @"MathNet.Numerics.fsx", None, None;
            @"MathNet.Numerics.IfSharp.fsx", None, None;
            @"..\..\src\FSharp\**\*.fs", Some "src/Common", None ] }

let coreBundle =
    { Id = numericsPack.Id
      Version = packageVersion
      Title = numericsPack.Title
      ReleaseNotesFile = "RELEASENOTES.md"
      FsLoader = true
      Packages = [ numericsPack; fsharpPack ] }

let coreSignedBundle =
    { Id = numericsSignedPack.Id
      Version = packageVersion
      Title = numericsSignedPack.Title
      ReleaseNotesFile = "RELEASENOTES.md"
      FsLoader = true
      Packages = [ numericsSignedPack; fsharpSignedPack ] }


// NATIVE PROVIDER PACKAGES

let mklWin32Pack =
    { Id = "MathNet.Numerics.MKL.Win-x86"
      Version = mklPackageVersion
      Title = "Math.NET Numerics - MKL Native Provider for Windows (x86)"
      Summary = ""
      Description = "Intel MKL native libraries for Math.NET Numerics. Requires an Intel MKL license if redistributed."
      ReleaseNotes = mklReleaseNotes
      Tags = "math numeric statistics probability integration interpolation linear algebra matrix fft native mkl"
      Authors = [ "Christoph Ruegg"; "Marcus Cuda"; "Jurgen Van Gael" ]
      Dependencies =
        [ { FrameworkVersion=""
            Dependencies=[ "MathNet.Numerics", "2.4.0" ] } ]
      Files =
        [ @"..\..\out\MKL\Windows\x86\libiomp5md.dll", Some "content", None;
          @"..\..\out\MKL\Windows\x86\MathNet.Numerics.MKL.dll", Some "content", None ] }

let mklWin64Pack =
    { mklWin32Pack with
        Id = "MathNet.Numerics.MKL.Win-x64"
        Title = "Math.NET Numerics - MKL Native Provider for Windows (x64)"
        Files =
          [ @"..\..\out\MKL\Windows\x64\libiomp5md.dll", Some "content", None;
            @"..\..\out\MKL\Windows\x64\MathNet.Numerics.MKL.dll", Some "content", None ] }

let mklLinux32Pack =
    { Id = "MathNet.Numerics.MKL.Linux-x86"
      Version = mklPackageVersion
      Title = "Math.NET Numerics - MKL Native Provider for Linux (x86)"
      Summary = ""
      Description = "Intel MKL native libraries for Math.NET Numerics. Requires an Intel MKL license if redistributed."
      ReleaseNotes = mklReleaseNotes
      Tags = "math numeric statistics probability integration interpolation linear algebra matrix fft native mkl"
      Authors = [ "Christoph Ruegg"; "Marcus Cuda"; "Jurgen Van Gael" ]
      Dependencies =
        [ { FrameworkVersion=""
            Dependencies=[ "MathNet.Numerics", "2.4.0" ] } ]
      Files =
        [ @"..\..\out\MKL\Linux\x86\libiomp5.so", Some "content", None;
          @"..\..\out\MKL\Linux\x86\MathNet.Numerics.MKL.dll", Some "content", None ] }

let mklLinux64Pack =
    { mklLinux32Pack with
        Id = "MathNet.Numerics.MKL.Linux-x64"
        Title = "Math.NET Numerics - MKL Native Provider for Linux (x64)"
        Files =
          [ @"..\..\out\MKL\Linux\x64\libiomp5.so", Some "content", None;
            @"..\..\out\MKL\Linux\x64\MathNet.Numerics.MKL.dll", Some "content", None ] }

let mklWinBundle =
    { Id = "MathNet.Numerics.MKL.Win"
      Version = mklPackageVersion
      Title = "Math.NET Numerics MKL Native Provider for Windows"
      ReleaseNotesFile = "RELEASENOTES-MKL.md"
      FsLoader = false
      Packages = [ mklWin32Pack; mklWin64Pack ] }

let mklLinuxBundle =
    { Id = "MathNet.Numerics.MKL.Linux"
      Version = mklPackageVersion
      Title = "Math.NET Numerics MKL Native Provider for Linux"
      ReleaseNotesFile = "RELEASENOTES-MKL.md"
      FsLoader = false
      Packages = [ mklLinux32Pack; mklLinux64Pack ] }


// DATA EXTENSION PACKAGES

let dataTextPack =
    { Id = "MathNet.Numerics.Data.Text"
      Version = dataPackageVersion
      Title = "Math.NET Numerics - Text Data I/O Extensions"
      Summary = ""
      Description = "Text Data Input/Output Extensions for Math.NET Numerics, the numerical foundation of the Math.NET project, aiming to provide methods and algorithms for numerical computations in science, engineering and every day use."
      ReleaseNotes = dataReleaseNotes
      Tags = "math numeric data text csv tsv json xml"
      Authors = [ "Christoph Ruegg"; "Marcus Cuda" ]
      Dependencies =
        [ { FrameworkVersion=""
            Dependencies=[ "MathNet.Numerics", GetPackageVersion "packages" "MathNet.Numerics" ] } ]
      Files =
        [ @"..\..\out\Data\lib\Net40\MathNet.Numerics.Data.Text.dll", Some libnet40, None;
          @"..\..\out\Data\lib\Net40\MathNet.Numerics.Data.Text.xml", Some libnet40, None ] }

let dataMatlabPack =
    { Id = "MathNet.Numerics.Data.Matlab"
      Version = dataPackageVersion
      Title = "Math.NET Numerics - MATLAB Data I/O Extensions"
      Summary = ""
      Description = "MathWorks MATLAB Data Input/Output Extensions for Math.NET Numerics, the numerical foundation of the Math.NET project, aiming to provide methods and algorithms for numerical computations in science, engineering and every day use."
      ReleaseNotes = dataReleaseNotes
      Tags = "math numeric data matlab"
      Authors = [ "Christoph Ruegg"; "Marcus Cuda" ]
      Dependencies =
        [ { FrameworkVersion=""
            Dependencies=[ "MathNet.Numerics", GetPackageVersion "packages" "MathNet.Numerics" ] } ]
      Files =
        [ @"..\..\out\Data\lib\Net40\MathNet.Numerics.Data.Matlab.dll", Some libnet40, None;
          @"..\..\out\Data\lib\Net40\MathNet.Numerics.Data.Matlab.xml", Some libnet40, None ] }

let dataBundle =
    { Id = "MathNet.Numerics.Data"
      Version = dataPackageVersion
      Title = "Math.NET Numerics Data Extensions"
      ReleaseNotesFile = "RELEASENOTES-Data.md"
      FsLoader = false
      Packages = [ dataTextPack; dataMatlabPack ] }


// --------------------------------------------------------------------------------------
// PREPARE
// --------------------------------------------------------------------------------------

Target "Start" DoNothing

Target "Clean" (fun _ ->
    CleanDirs [ "obj" ]
    CleanDirs [ "out/api"; "out/docs"; "out/packages" ]
    CleanDirs [ "out/lib/Net35"; "out/lib/Net40"; "out/lib/Profile7"; "out/lib/Profile47"; "out/lib/Profile78"; "out/lib/Profile259"; "out/lib/Profile328" ]
    CleanDirs [ "out/test/Net35"; "out/test/Net40"; "out/test/Profile7"; "out/test/Profile47"; "out/test/Profile78"; "out/test/Profile259"; "out/test/Profile328" ]
    CleanDirs [ "out/lib-debug/Net35"; "out/lib-debug/Net40"; "out/lib-debug/Profile7"; "out/lib-debug/Profile47"; "out/lib-debug/Profile78"; "out/lib-debug/Profile259"; "out/lib-debug/Profile328" ]
    CleanDirs [ "out/test-debug/Net35"; "out/test-debug/Net40"; "out/test-debug/Profile7"; "out/test-debug/Profile47"; "out/test-debug/Profile78"; "out/test-debug/Profile259"; "out/test-debug/Profile328" ]
    CleanDirs [ "out/lib-signed/Net40"; "out/test-signed/Net40" ] // Signed Build
    CleanDirs [ "out/MKL"; "out/ATLAS" ] // Native Providers
    CleanDirs [ "out/Data" ]) // Data Extensions

Target "ApplyVersion" (fun _ ->
    let patchAssemblyInfo path assemblyVersion packageVersion =
        BulkReplaceAssemblyInfoVersions path (fun f ->
            { f with
                AssemblyVersion = assemblyVersion
                AssemblyFileVersion = assemblyVersion
                AssemblyInformationalVersion = packageVersion })
    patchAssemblyInfo "src/Numerics" assemblyVersion packageVersion
    patchAssemblyInfo "src/FSharp" assemblyVersion packageVersion
    patchAssemblyInfo "src/UnitTests" assemblyVersion packageVersion
    patchAssemblyInfo "src/FSharpUnitTests" assemblyVersion packageVersion
    patchAssemblyInfo "src/Data" dataAssemblyVersion dataPackageVersion
    patchAssemblyInfo "src/DataUnitTests" dataAssemblyVersion dataPackageVersion
    ReplaceInFile
        (regex_replace @"\d+\.\d+\.\d+\.\d+" mklAssemblyVersion
         >> regex_replace @"\d+,\d+,\d+,\d+" (replace "." "," mklAssemblyVersion))
        "src/NativeProviders/Common/resource.rc")

Target "Prepare" DoNothing
"Start"
  =?> ("Clean", not (hasBuildParam "incremental"))
  ==> "ApplyVersion"
  ==> "Prepare"


// --------------------------------------------------------------------------------------
// BUILD
// --------------------------------------------------------------------------------------

let buildConfig config subject = MSBuild "" (if hasBuildParam "incremental" then "Build" else "Rebuild") [ "Configuration", config ] subject |> ignore
let build subject = buildConfig "Release" subject
let buildSigned subject = buildConfig "Release-Signed" subject
let nativeWin32Build subject = MSBuild "" (if hasBuildParam "incremental" then "Build" else "Rebuild") [("Configuration","Release"); ("Platform","Win32")] subject |> ignore
let nativeWin64Build subject = MSBuild "" (if hasBuildParam "incremental" then "Build" else "Rebuild") [("Configuration","Release"); ("Platform","x64")] subject |> ignore

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

Target "MklWin32Build" (fun _ -> nativeWin32Build !! "MathNet.Numerics.NativeProviders.sln")
Target "MklWin64Build" (fun _ -> nativeWin64Build !! "MathNet.Numerics.NativeProviders.sln")

Target "MklWinBuild" DoNothing
"Prepare" ==> "MklWin32Build" ==> "MklWinBuild"
"Prepare" ==> "MklWin64Build" ==> "MklWinBuild"

Target "DataBuild" (fun _ -> build !! "MathNet.Numerics.Data.sln")
"Prepare" ==> "DataBuild"


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

Target "MklWin32Test" (fun _ ->
    ActivateFinalTarget "CloseTestRunner"
    !! "out/MKL/Windows/x86/*UnitTests*.dll"
    |> NUnit (fun p ->
        { p with
            ToolName = "nunit-console-x86.exe"
            DisableShadowCopy = true
            TimeOut = TimeSpan.FromMinutes 30.
            OutputFile = "TestResults.xml" }))
Target "MklWin64Test" (fun _ ->
    ActivateFinalTarget "CloseTestRunner"
    !! "out/MKL/Windows/x64/*UnitTests*.dll"
    |> NUnit (fun p ->
        { p with
            ToolName = "nunit-console.exe"
            DisableShadowCopy = true
            TimeOut = TimeSpan.FromMinutes 30.
            OutputFile = "TestResults.xml" }))
Target "MklWinTest" DoNothing

FinalTarget "CloseTestRunner" (fun _ ->
    ProcessHelper.killProcess "nunit-agent.exe"
    ProcessHelper.killProcess "nunit-agent-x86.exe"
)

"MklWin32Test" ==> "MklWinTest"
"MklWin64Test" ==> "MklWinTest"

Target "DataTest" (fun _ -> test !! "out/Data/test/**/*UnitTests*.dll")


// --------------------------------------------------------------------------------------
// PACKAGES
// --------------------------------------------------------------------------------------

let provideLicense path =
    ReadFileAsString "LICENSE.md"
    |> ConvertTextToWindowsLineBreaks
    |> ReplaceFile (path @@ "license.txt")

let provideReadme title releasenotes path =
    String.concat Environment.NewLine [header; " " + title; ""; ReadFileAsString releasenotes]
    |> ConvertTextToWindowsLineBreaks
    |> ReplaceFile (path @@ "readme.txt")

let provideFsLoader includes path =
    // inspired by FsLab/tpetricek
    let fullScript = ReadFile "src/FSharp/MathNet.Numerics.fsx" |> Array.ofSeq
    let startIndex = fullScript |> Seq.findIndex (fun s -> s.Contains "***MathNet.Numerics.fsx***")
    let extraScript = fullScript .[startIndex + 1 ..] |> List.ofSeq
    let assemblies = [ "MathNet.Numerics.dll"; "MathNet.Numerics.FSharp.dll" ]
    let nowarn = ["#nowarn \"211\""]
    let references = [ for assembly in assemblies -> sprintf "#r \"%s\"" assembly ]
    ReplaceFile (path @@ "MathNet.Numerics.fsx") (nowarn @ includes @ references @ extraScript |> toLines)

let provideFsIfSharpLoader path =
    let fullScript = ReadFile "src/FSharp/MathNet.Numerics.IfSharp.fsx" |> Array.ofSeq
    let startIndex = fullScript |> Seq.findIndex (fun s -> s.Contains "***MathNet.Numerics.IfSharp.fsx***")
    ReplaceFile (path @@ "MathNet.Numerics.IfSharp.fsx") (fullScript .[startIndex + 1 ..] |> toLines)

let provideZipExtraFiles path (bundle:Bundle) =
    provideLicense path
    provideReadme (sprintf "%s v%s" bundle.Title bundle.Version) bundle.ReleaseNotesFile path
    if bundle.FsLoader then
        let includes = [ for root in [ ""; "../"; "../../" ] -> sprintf "#I \"%sNet40\"" root ]
        provideFsLoader includes path
        provideFsIfSharpLoader path

let provideNuGetExtraFiles path (bundle:Bundle) (pack:Package) =
    provideLicense path
    provideReadme (sprintf "%s v%s" pack.Title pack.Version) bundle.ReleaseNotesFile path
    if pack = fsharpPack || pack = fsharpSignedPack then
        let includes = [ for root in [ ""; "../"; "../../"; "../../../" ] do
                         for package in bundle.Packages do
                         yield sprintf "#I \"%spackages/%s/lib/net40/\"" root package.Id
                         yield sprintf "#I \"%spackages/%s.%s/lib/net40/\"" root package.Id package.Version ]
        provideFsLoader includes path
        provideFsIfSharpLoader path

// ZIP

let zip zipDir filesDir filesFilter bundle =
    CleanDir "obj/Zip"
    let workPath = "obj/Zip/" + bundle.Id
    CopyDir workPath filesDir filesFilter
    provideZipExtraFiles workPath bundle
    Zip "obj/Zip/" (zipDir @@ sprintf "%s-%s.zip" bundle.Id bundle.Version) !! (workPath + "/**/*.*")
    CleanDir "obj/Zip"

Target "Zip" (fun _ ->
    CleanDir "out/packages/Zip"
    if not (hasBuildParam "signed") || hasBuildParam "release" then
        coreBundle |> zip "out/packages/Zip" "out/lib" (fun f -> f.Contains("MathNet.Numerics.") || f.Contains("System.Threading.") || f.Contains("FSharp.Core."))
    if hasBuildParam "signed" || hasBuildParam "release" then
        coreSignedBundle |> zip "out/packages/Zip" "out/lib-signed" (fun f -> f.Contains("MathNet.Numerics.")))

Target "MklWinZip" (fun _ ->
    CreateDir "out/MKL/packages/Zip"
    mklWinBundle |> zip "out/MKL/packages/Zip" "out/MKL/Windows" (fun f -> f.Contains("MathNet.Numerics.MKL.") || f.Contains("libiomp5md.dll")))

Target "MklLinuxZip" (fun _ ->
    CreateDir "out/MKL/packages/Zip"
    mklLinuxBundle |> zip "out/MKL/packages/Zip" "out/MKL/Linux" (fun f -> f.Contains("MathNet.Numerics.MKL.") || f.Contains("libiomp5.so")))

Target "DataZip" (fun _ ->
    CleanDir "out/Data/packages/Zip"
    dataBundle |> zip "out/Data/packages/Zip" "out/Data/lib" (fun f -> f.Contains("MathNet.Numerics.Data.")))


// NUGET

let updateNuspec (pack:Package) outPath symbols updateFiles spec =
    { spec with ToolPath = "packages/NuGet.CommandLine/tools/NuGet.exe"
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
                DependenciesByFramework = pack.Dependencies
                SymbolPackage = symbols
                Files = updateFiles pack.Files
                Publish = false }

let nugetPack bundle outPath =
    CleanDir "obj/NuGet"
    for pack in bundle.Packages do
        provideNuGetExtraFiles "obj/NuGet" bundle pack
        let withLicenseReadme f = [ "license.txt", None, None; "readme.txt", None, None; ] @ f
        let withoutSymbolsSources f =
            List.choose (function | (_, Some (target:string), _) when target.StartsWith("src") -> None
                                  | (s, t, None) -> Some (s, t, Some ("**/*.pdb"))
                                  | (s, t, Some e) -> Some (s, t, Some (e + ";**/*.pdb"))) f
        // first pass - generates symbol + normal package. NuGet does drop the symbols from the normal package, but unfortunately not the sources.
        NuGet (updateNuspec pack outPath NugetSymbolPackage.Nuspec withLicenseReadme) "build/MathNet.Numerics.nuspec"
        // second pass - generate only normal package, again, but this time explicitly drop the sources (and the debug symbols)
        NuGet (updateNuspec pack outPath NugetSymbolPackage.None (withLicenseReadme >> withoutSymbolsSources)) "build/MathNet.Numerics.nuspec"
        CleanDir "obj/NuGet"

let nugetPackExtension bundle outPath =
    CleanDir "obj/NuGet"
    for pack in bundle.Packages do
        provideNuGetExtraFiles "obj/NuGet" bundle pack
        let withLicenseReadme f = [ "license.txt", None, None; "readme.txt", None, None; ] @ f
        NuGet (updateNuspec pack outPath NugetSymbolPackage.None withLicenseReadme) "build/MathNet.Numerics.Extension.nuspec"
        CleanDir "obj/NuGet"

Target "NuGet" (fun _ ->
    CleanDir "out/packages/NuGet"
    if hasBuildParam "signed" || hasBuildParam "release" then
        nugetPack coreSignedBundle "out/packages/NuGet"
    if hasBuildParam "all" || hasBuildParam "release" then
        nugetPack coreBundle "out/packages/NuGet")

Target "MklWinNuGet" (fun _ ->
    CreateDir "out/MKL/packages/NuGet"
    nugetPackExtension mklWinBundle "out/MKL/packages/NuGet")

Target "MklLinuxNuGet" (fun _ ->
    CreateDir "out/MKL/packages/NuGet"
    nugetPackExtension mklLinuxBundle "out/MKL/packages/NuGet")

Target "DataNuGet" (fun _ ->
    CleanDir "out/Data/packages/NuGet"
    nugetPackExtension dataBundle "out/Data/packages/NuGet")


// --------------------------------------------------------------------------------------
// Documentation
// --------------------------------------------------------------------------------------

// DOCS

Target "CleanDocs" (fun _ -> CleanDirs ["out/docs"])

let extraDocs =
    [ "LICENSE.md", "License.md"
      "CONTRIBUTING.md", "Contributing.md"
      "CONTRIBUTORS.md", "Contributors.md" ]

let releaseNotesDocs =
    [ "RELEASENOTES.md", "ReleaseNotes.md", "Release Notes"
      "RELEASENOTES-Data.md", "ReleaseNotes-Data.md", "Data Extensions Release Notes"
      "RELEASENOTES-MKL.md", "ReleaseNotes-MKL.md", "MKL Native Provider Release Notes" ]

let provideDocExtraFiles() =
    for (fileName, docName) in extraDocs do CopyFile ("docs/content" @@ docName) fileName
    for (fileName, docName, title) in releaseNotesDocs do
        String.concat Environment.NewLine
          [ "# " + title
            "[Math.NET Numerics](ReleaseNotes.html) | [Data Extensions](ReleaseNotes-Data.html) | [MKL Native Provider](ReleaseNotes-MKL.html)"
            ""
            ReadFileAsString fileName ]
        |> ReplaceFile ("docs/content" @@ docName)

let generateDocs fail local =
    let args = if local then [] else ["--define:RELEASE"]
    if executeFSIWithArgs "docs/tools" "build-docs.fsx" args [] then
         traceImportant "Docs generated"
    else
        if fail then
            failwith "Generating documentation failed"
        else
            traceImportant "generating documentation failed"

Target "Docs" (fun _ ->
    provideDocExtraFiles ()
    generateDocs true false)
Target "DocsDev" (fun _ ->
    provideDocExtraFiles ()
    generateDocs true true)
Target "DocsWatch" (fun _ ->
    provideDocExtraFiles ()
    use watcher = new FileSystemWatcher(DirectoryInfo("docs/content").FullName, "*.*")
    watcher.EnableRaisingEvents <- true
    watcher.Changed.Add(fun e -> generateDocs false true)
    watcher.Created.Add(fun e -> generateDocs false true)
    watcher.Renamed.Add(fun e -> generateDocs false true)
    watcher.Deleted.Add(fun e -> generateDocs false true)
    traceImportant "Waiting for docs edits. Press any key to stop."
    System.Console.ReadKey() |> ignore
    watcher.EnableRaisingEvents <- false
    watcher.Dispose())

"CleanDocs" ==> "Docs"

"Start"
  =?> ("CleanDocs", not (hasBuildParam "incremental"))
  ==> "DocsDev"
  ==> "DocsWatch"


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

"CleanApi" ==> "Api"


// --------------------------------------------------------------------------------------
// Publishing
// Requires permissions; intended only for maintainers
// --------------------------------------------------------------------------------------

let publishReleaseTag title prefix version notes =
    // inspired by Deedle/tpetricek
    let tagName = prefix + "v" + version
    let tagMessage = String.concat Environment.NewLine [title + " v" + version; ""; notes ]
    let cmd = sprintf """tag -a %s -m "%s" """ tagName tagMessage
    Git.CommandHelper.runSimpleGitCommand "." cmd |> printfn "%s"
    let _, remotes, _ = Git.CommandHelper.runGitCommand "." "remote -v"
    let main = remotes |> Seq.find (fun s -> s.Contains("(push)") && s.Contains("mathnet/mathnet-numerics"))
    let remoteName = main.Split('\t').[0]
    Git.Branches.pushTag "." remoteName tagName

Target "PublishTag" (fun _ -> publishReleaseTag "Math.NET Numerics" "" packageVersion releaseNotes)
Target "MklPublishTag" (fun _ -> publishReleaseTag "Math.NET Numerics MKL Provider" "mkl-" mklPackageVersion mklReleaseNotes)
Target "DataPublishTag" (fun _ -> publishReleaseTag "Math.NET Numerics Data Extensions" "data-" dataPackageVersion dataReleaseNotes)

Target "PublishMirrors" (fun _ ->
    let repo = "../mirror-numerics"
    Git.CommandHelper.runSimpleGitCommand repo "remote update" |> printfn "%s"
    Git.CommandHelper.runSimpleGitCommand repo "push mirrors" |> printfn "%s")

Target "PublishDocs" (fun _ ->
    let repo = "../mathnet-websites"
    Git.Branches.pull repo "origin" "master"
    CopyRecursive "out/docs" "../mathnet-websites/numerics" true |> printfn "%A"
    Git.Staging.StageAll repo
    Git.Commit.Commit repo (sprintf "Numerics: %s docs update" packageVersion)
    Git.Branches.pushBranch repo "origin" "master")

Target "PublishApi" (fun _ ->
    let repo = "../mathnet-websites"
    Git.Branches.pull repo "origin" "master"
    CleanDir "../mathnet-websites/numerics/api"
    CopyRecursive "out/api" "../mathnet-websites/numerics/api" true |> printfn "%A"
    Git.Staging.StageAll repo
    Git.Commit.Commit repo (sprintf "Numerics: %s api update" packageVersion)
    Git.Branches.pushBranch repo "origin" "master")

let publishNuGet packageFiles =
    // TODO: Migrate to NuGet helper once it supports direct (non-integrated) operations
    let rec impl trials file =
        trace ("NuGet Push: " + System.IO.Path.GetFileName(file) + ".")
        try
            let args = sprintf "push \"%s\"" (FullName file)
            let result =
                ExecProcess (fun info ->
                    info.FileName <- "packages/NuGet.CommandLine/tools/NuGet.exe"
                    info.WorkingDirectory <- FullName "obj/NuGet"
                    info.Arguments <- args) (TimeSpan.FromMinutes 10.)
            if result <> 0 then failwith "Error during NuGet push."
        with exn ->
            if trials > 0 then impl (trials-1) file
            else ()
    Seq.iter (impl 3) packageFiles

Target "PublishNuGet" (fun _ -> !! "out/packages/NuGet/*.nupkg" -- "out/packages/NuGet/*.symbols.nupkg" |> publishNuGet)
Target "MklPublishNuGet" (fun _ -> !! "out/MKL/packages/NuGet/*.nupkg" |> publishNuGet)
Target "DataPublishNuGet" (fun _ -> !! "out/Data/packages/NuGet/*.nupkg" |> publishNuGet)

Target "Publish" DoNothing
"PublishTag" ==> "Publish"
"PublishNuGet" ==> "Publish"
"PublishDocs" ==> "Publish"
"PublishApi" ==> "Publish"

Target "MklPublish" DoNothing
"MklPublishTag" ==> "MklPublish"
"MklPublishNuGet" ==> "MklPublish"

Target "DataPublish" DoNothing
"DataPublishTag" ==> "DataPublish"
"DataPublishNuGet" ==> "DataPublish"


// --------------------------------------------------------------------------------------
// ENVIRONMENT DEPENDENCIES
// --------------------------------------------------------------------------------------

match buildServer with

| AppVeyor ->
    trace "AppVeyor Continuous Integration Build"
    // In AppVeyor we let its engine managed task dependencies
    // an let it call into this script multiple times, incrementally.

    // build --> test: do not enforce
    // build --> package: do not enforce
    // build --> docs: do not enforce
    ()

| _ ->
    trace "Normal Build"
    // In normal builds we need to set up proper dependencies between
    // the targets so FAKE can build up and order the full work-flow properly

    // build --> test
    "Build" ==> "Test" |> ignore
    "MklWin32Build" ==> "MklWin32Test" |> ignore
    "MklWin64Build" ==> "MklWin64Test" |> ignore
    "DataBuild" ==> "DataTest" |> ignore

    // build --> package
    "Build" ==> "Zip" |> ignore
    "MklWinBuild" ==> "MklWinZip" |> ignore
    "DataBuild" ==> "DataZip" |> ignore
    "Build" ==> "NuGet" |> ignore
    "MklWinBuild" ==> "MklWinNuGet" |> ignore
    "DataBuild" ==> "DataNuGet" |> ignore

    // build --> docs
    "Build" ==> "CleanDocs" |> ignore
    "Build" ==> "Docs" |> ignore
    "Build" ==> "CleanApi" |> ignore
    "Build" ==> "Api" |> ignore
    ()


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

Target "MklWinAll" DoNothing
"MklWinBuild" ==> "MklWinAll"
"MklWinZip" ==> "MklWinAll"
"MklWinNuGet" ==> "MklWinAll"
"MklWinTest" ==> "MklWinAll"

Target "DataAll" DoNothing
"DataBuild" ==> "DataAll"
"DataZip" ==> "DataAll"
"DataNuGet" ==> "DataAll"
"DataTest" ==> "DataAll"

RunTargetOrDefault "Test"
