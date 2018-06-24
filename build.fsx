//  __  __       _   _       _   _ ______ _______
// |  \/  |     | | | |     | \ | |  ____|__   __|
// | \  / | __ _| |_| |__   |  \| | |__     | |
// | |\/| |/ _` | __| '_ \  | . ` |  __|    | |
// | |  | | (_| | |_| | | |_| |\  | |____   | |
// |_|  |_|\__,_|\__|_| |_(_)_| \_|______|  |_|
//
// Math.NET Numerics - https://numerics.mathdotnet.com
// Copyright (c) Math.NET - Open Source MIT/X11 License
//
// FAKE build script, see http://fsharp.github.io/FAKE
//

// --------------------------------------------------------------------------------------
// PRELUDE
// --------------------------------------------------------------------------------------

#I "packages/build/FAKE/tools"
#r "packages/build/FAKE/tools/FakeLib.dll"

open Fake
open Fake.DocuHelper
open System
open System.IO

#load "build/build-framework.fsx"
open BuildFramework


// --------------------------------------------------------------------------------------
// PROJECT INFO
// --------------------------------------------------------------------------------------

// VERSION OVERVIEW

let numericsRelease = release "Math.NET Numerics" "RELEASENOTES.md"
let mklRelease = release "MKL Provider" "RELEASENOTES-MKL.md"
let cudaRelease = release "CUDA Provider" "RELEASENOTES-CUDA.md"
let openBlasRelease = release "OpenBLAS Provider" "RELEASENOTES-OpenBLAS.md"
let dataRelease = release "Data Extensions" "RELEASENOTES-Data.md"
let releases = [ numericsRelease; mklRelease; openBlasRelease; dataRelease ] // skip cuda
traceHeader releases


// NUMERICS PACKAGES

let numericsZipPackage = zipPackage "MathNet.Numerics" "Math.NET Numerics" numericsRelease true
let numericsStrongNameZipPackage = zipPackage "MathNet.Numerics.Signed" "Math.NET Numerics" numericsRelease true

let numericsNuGetPackage = nugetPackage "MathNet.Numerics" numericsRelease
let numericsFSharpNuGetPackage = nugetPackage "MathNet.Numerics.FSharp" numericsRelease
let numericsStrongNameNuGetPackage = nugetPackage "MathNet.Numerics.Signed" numericsRelease
let numericsFSharpStrongNameNuGetPackage = nugetPackage "MathNet.Numerics.FSharp.Signed" numericsRelease

let numericsProject = project "MathNet.Numerics" "src/Numerics/Numerics.csproj" [numericsNuGetPackage; numericsStrongNameNuGetPackage]
let numericsFsharpProject = project "MathNet.Numerics.FSharp" "src/FSharp/FSharp.fsproj" [numericsFSharpNuGetPackage; numericsFSharpStrongNameNuGetPackage]
let numericsSolution = solution "Numerics" "MathNet.Numerics.sln" [numericsProject; numericsFsharpProject] [numericsZipPackage; numericsStrongNameZipPackage]


// DATA EXTENSION PACKAGES

let dataZipPackage = zipPackage "MathNet.Numerics.Data" "Math.NET Numerics Data Extensions" dataRelease false
let dataStrongNameZipPackage = zipPackage "MathNet.Numerics.Data.Signed" "Math.NET Numerics Data Extensions" dataRelease false

let dataTextNuGetPackage = nugetPackage "MathNet.Numerics.Data.Text" dataRelease
let dataMatlabNuGetPackage = nugetPackage "MathNet.Numerics.Data.Matlab" dataRelease
let dataTextStrongNameNuGetPackage = nugetPackage "MathNet.Numerics.Data.Text.Signed" dataRelease
let dataMatlabStrongNameNuGetPackage = nugetPackage "MathNet.Numerics.Data.Matlab.Signed" dataRelease

let dataTextProject = project "MathNet.Numerics.Data.Text" "src/Data/Text/Text.csproj" [dataTextNuGetPackage; dataTextStrongNameNuGetPackage]
let dataMatlabProject = project "MathNet.Numerics.Data.Matlab" "src/Data/Matlab/Matlab.csproj" [dataMatlabNuGetPackage; dataMatlabStrongNameNuGetPackage]
let dataSolution = solution "Data" "MathNet.Numerics.Data.sln" [dataTextProject; dataMatlabProject] [dataZipPackage; dataStrongNameZipPackage]


// MKL NATIVE PROVIDER PACKAGES

let mklWinZipPackage = zipPackage "MathNet.Numerics.MKL.Win" "Math.NET Numerics MKL Native Provider for Windows" mklRelease false
let mklLinuxZipPackage = zipPackage "MathNet.Numerics.MKL.Linux" "Math.NET Numerics MKL Native Provider for Linux" mklRelease false

let mklWinNuGetPackage = nugetPackage "MathNet.Numerics.MKL.Win" mklRelease
let mklWin32NuGetPackage = nugetPackage "MathNet.Numerics.MKL.Win-x86" mklRelease
let mklWin64NuGetPackage = nugetPackage "MathNet.Numerics.MKL.Win-x64" mklRelease
let mklLinuxNuGetPackage = nugetPackage "MathNet.Numerics.MKL.Linux" mklRelease
let mklLinux32NuGetPackage = nugetPackage "MathNet.Numerics.MKL.Linux-x86" mklRelease
let mklLinux64NuGetPackage = nugetPackage "MathNet.Numerics.MKL.Linux-x64" mklRelease

let mklWinProject = nativeProject "MathNet.Numerics.MKL" "src/NativeProviders/Windows/MKL/MKLWrapper.vcxproj" [mklWinNuGetPackage; mklWin32NuGetPackage; mklWin64NuGetPackage]
let mklLinuxProject = nativeBashScriptProject "MathNet.Numerics.MKL" "src/NativeProviders/Linux/mkl_build.sh" [mklLinuxNuGetPackage; mklLinux32NuGetPackage; mklLinux64NuGetPackage]
let mklSolution = solution "MKL" "MathNet.Numerics.MKL.sln" [mklWinProject; mklLinuxProject] [mklWinZipPackage; mklLinuxZipPackage]

let mklWinPack =
    { NuGet = mklWinNuGetPackage
      NuSpecFile = "build/MathNet.Numerics.MKL.Win.nuspec"
      Title = "Math.NET Numerics - MKL Native Provider for Windows (x64 and x86)" }

let mklWin32Pack =
    { NuGet = mklWin32NuGetPackage
      NuSpecFile = "build/MathNet.Numerics.MKL.Win-x86.nuspec"
      Title = "Math.NET Numerics - MKL Native Provider for Windows (x86)" }

let mklWin64Pack =
    { NuGet = mklWin64NuGetPackage
      NuSpecFile = "build/MathNet.Numerics.MKL.Win-x64.nuspec"
      Title = "Math.NET Numerics - MKL Native Provider for Windows (x64)" }

let mklLinuxPack =
    { NuGet = mklLinuxNuGetPackage
      NuSpecFile = "build/MathNet.Numerics.MKL.Linux.nuspec"
      Title = "Math.NET Numerics - MKL Native Provider for Linux (x64 and x86)" }

let mklLinux32Pack =
    { NuGet = mklLinux32NuGetPackage
      NuSpecFile = "build/MathNet.Numerics.MKL.Linux-x86.nuspec"
      Title = "Math.NET Numerics - MKL Native Provider for Linux (x86)" }

let mklLinux64Pack =
    { NuGet = mklLinux64NuGetPackage
      NuSpecFile = "build/MathNet.Numerics.MKL.Linux-x64.nuspec"
      Title = "Math.NET Numerics - MKL Native Provider for Linux (x64)" }


// CUDA NATIVE PROVIDER PACKAGES

let cudaWinZipPackage = zipPackage "MathNet.Numerics.CUDA.Win" "Math.NET Numerics CUDA Native Provider for Windows" cudaRelease false
let cudaWinNuGetPackage = nugetPackage "MathNet.Numerics.CUDA.Win" cudaRelease

let cudaWinProject = nativeProject "MathNet.Numerics.CUDA" "src/NativeProviders/Windows/CUDA/CUDAWrapper.vcxproj" [cudaWinNuGetPackage]
let cudaSolution = solution "CUDA" "MathNet.Numerics.CUDA.sln" [cudaWinProject] [cudaWinZipPackage]

let cudaWinPack =
    { NuGet = cudaWinNuGetPackage
      NuSpecFile = "build/MathNet.Numerics.CUDA.Win.nuspec"
      Title = "Math.NET Numerics - CUDA Native Provider for Windows (x64)" }


// OpenBLAS NATIVE PROVIDER PACKAGES

let openBlasWinZipPackage = zipPackage "MathNet.Numerics.OpenBLAS.Win" "Math.NET Numerics OpenBLAS Native Provider for Windows" openBlasRelease false
let openBlasWinNuGetPackage = nugetPackage "MathNet.Numerics.OpenBLAS.Win" openBlasRelease

let openBlasWinProject = nativeProject "MathNet.Numerics.OpenBLAS" "src/NativeProviders/Windows/OpenBLAS/OpenBLASWrapper.vcxproj" [openBlasWinNuGetPackage]
let openBlasSolution = solution "OpenBLAS" "MathNet.Numerics.OpenBLAS.sln" [openBlasWinProject] [openBlasWinZipPackage]

let openBlasWinPack =
    { NuGet = openBlasWinNuGetPackage
      NuSpecFile = "build/MathNet.Numerics.OpenBLAS.Win.nuspec"
      Title = "Math.NET Numerics - OpenBLAS Native Provider for Windows (x64 and x86)" }


// ALL

let allSolutions = [numericsSolution; dataSolution]
let allProjects = allSolutions |> List.collect (fun s -> s.Projects) |> List.distinct


// --------------------------------------------------------------------------------------
// PREPARE
// --------------------------------------------------------------------------------------

Target "Start" DoNothing

Target "Clean" (fun _ ->
    DeleteDirs (!! "src/**/obj/" ++ "src/**/bin/" )
    CleanDirs [ "out/api"; "out/docs" ]
    CleanDirs [ "out/MKL"; "out/ATLAS"; "out/CUDA"; "out/OpenBLAS" ] // Native Providers
    allSolutions |> List.iter (fun solution -> CleanDirs [ solution.OutputZipDir; solution.OutputNuGetDir; solution.OutputLibDir; solution.OutputLibStrongNameDir ])
    allSolutions |> List.iter clean)

Target "ApplyVersion" (fun _ ->
    allProjects |> List.iter patchVersionInProjectFile
    patchVersionInAssemblyInfo "src/FSharp" numericsRelease
    patchVersionInAssemblyInfo "src/TestData" numericsRelease
    patchVersionInAssemblyInfo "src/Numerics.Tests" numericsRelease
    patchVersionInAssemblyInfo "src/FSharp.Tests" numericsRelease
    patchVersionInAssemblyInfo "src/Data.Tests" dataRelease
    patchVersionInResource "src/NativeProviders/MKL/resource.rc" mklRelease
    patchVersionInResource "src/NativeProviders/CUDA/resource.rc" cudaRelease
    patchVersionInResource "src/NativeProviders/OpenBLAS/resource.rc" openBlasRelease)

Target "Restore" (fun _ -> allSolutions |> List.iter restore)
"Start"
  =?> ("Clean", not (hasBuildParam "incremental"))
  ==> "Restore"

Target "Prepare" DoNothing
"Start"
  =?> ("Clean", not (hasBuildParam "incremental"))
  ==> "ApplyVersion"
  ==> "Prepare"


// --------------------------------------------------------------------------------------
// BUILD, SIGN, COLLECT
// --------------------------------------------------------------------------------------

let fingerprint = "5dbea70701b40cab1b2ca62c75401342b4f0f03a"
let timeserver = "http://time.certum.pl/"

Target "Build" (fun _ ->

    // Strong Name Build (with strong name, without certificate signature)
    if hasBuildParam "strongname" then
        CleanDirs (!! "src/**/obj/" ++ "src/**/bin/" )
        restoreSN numericsSolution
        buildSN numericsSolution
        if isWindows && hasBuildParam "sign" then sign fingerprint timeserver numericsSolution
        collectBinariesSN numericsSolution
        zip numericsStrongNameZipPackage numericsSolution.OutputZipDir numericsSolution.OutputLibStrongNameDir (fun f -> f.Contains("MathNet.Numerics.") || f.Contains("System.Threading.") || f.Contains("FSharp.Core."))
        if isWindows then
            packSN numericsSolution
            collectNuGetPackages numericsSolution

    // Normal Build (without strong name, with certificate signature)
    CleanDirs (!! "src/**/obj/" ++ "src/**/bin/" )
    restore numericsSolution
    build numericsSolution
    if isWindows && hasBuildParam "sign" then sign fingerprint timeserver numericsSolution
    collectBinaries numericsSolution
    zip numericsZipPackage numericsSolution.OutputZipDir numericsSolution.OutputLibDir (fun f -> f.Contains("MathNet.Numerics.") || f.Contains("System.Threading.") || f.Contains("FSharp.Core."))
    if isWindows then
        pack numericsSolution
        collectNuGetPackages numericsSolution

    // NuGet Sign (all or nothing)
    if isWindows && hasBuildParam "sign" then signNuGet fingerprint timeserver numericsSolution

    )
"Prepare" ==> "Build"

Target "DataBuild" (fun _ ->

    // Strong Name Build (with strong name, without certificate signature)
    if hasBuildParam "strongname" then
        CleanDirs (!! "src/**/obj/" ++ "src/**/bin/" )
        restoreSN dataSolution
        buildSN dataSolution
        if isWindows && hasBuildParam "sign" then sign fingerprint timeserver dataSolution
        collectBinariesSN dataSolution
        zip dataStrongNameZipPackage dataSolution.OutputZipDir dataSolution.OutputLibStrongNameDir (fun f -> f.Contains("MathNet.Numerics.Data."))
        if isWindows then
            packProjectSN dataTextProject
            packProjectSN dataMatlabProject
            collectNuGetPackages dataSolution

    // Normal Build (without strong name, with certificate signature)
    CleanDirs (!! "src/**/obj/" ++ "src/**/bin/" )
    restore dataSolution
    build dataSolution
    if isWindows && hasBuildParam "sign" then sign fingerprint timeserver dataSolution
    collectBinaries dataSolution
    zip dataZipPackage dataSolution.OutputZipDir dataSolution.OutputLibDir (fun f -> f.Contains("MathNet.Numerics.Data."))
    if isWindows then
        packProject dataTextProject
        packProject dataMatlabProject
        collectNuGetPackages dataSolution

    // NuGet Sign (all or nothing)
    if isWindows && hasBuildParam "sign" then signNuGet fingerprint timeserver dataSolution

    )
"Prepare" ==> "DataBuild"

Target "MklWinBuild" (fun _ ->

    restore mklSolution
    buildConfig32 "Release-MKL" !! "MathNet.Numerics.MKL.sln"
    buildConfig64 "Release-MKL" !! "MathNet.Numerics.MKL.sln"
    CreateDir mklSolution.OutputZipDir
    zip mklWinZipPackage mklSolution.OutputZipDir "out/MKL/Windows" (fun f -> f.Contains("MathNet.Numerics.MKL.") || f.Contains("libiomp5md.dll"))
    CreateDir mklSolution.OutputNuGetDir
    nugetPackManually mklSolution [ mklWinPack; mklWin32Pack; mklWin64Pack ]

    // NuGet Sign (all or nothing)
    if isWindows && hasBuildParam "sign" then signNuGet fingerprint timeserver mklSolution

    )
"Prepare" ==> "MklWinBuild"

Target "CudaWinBuild" (fun _ ->

    restore cudaSolution
    buildConfig64 "Release-CUDA" !! "MathNet.Numerics.CUDA.sln"
    CreateDir cudaSolution.OutputZipDir
    zip cudaWinZipPackage cudaSolution.OutputZipDir "out/CUDA/Windows" (fun f -> f.Contains("MathNet.Numerics.CUDA.") || f.Contains("cublas") || f.Contains("cudart") || f.Contains("cusolver"))
    CreateDir cudaSolution.OutputNuGetDir
    nugetPackManually cudaSolution [ cudaWinPack ]

    // NuGet Sign (all or nothing)
    if isWindows && hasBuildParam "sign" then signNuGet fingerprint timeserver cudaSolution

    )
"Prepare" ==> "CudaWinBuild"

Target "OpenBlasWinBuild" (fun _ ->

    restore openBlasSolution
    buildConfig32 "Release-OpenBLAS" !! "MathNet.Numerics.OpenBLAS.sln"
    buildConfig64 "Release-OpenBLAS" !! "MathNet.Numerics.OpenBLAS.sln"
    CreateDir openBlasSolution.OutputZipDir
    zip openBlasWinZipPackage openBlasSolution.OutputZipDir "out/OpenBLAS/Windows" (fun f -> f.Contains("MathNet.Numerics.OpenBLAS.") || f.Contains("libgcc") || f.Contains("libgfortran") || f.Contains("libopenblas") || f.Contains("libquadmath"))
    CreateDir openBlasSolution.OutputNuGetDir
    nugetPackManually openBlasSolution [ openBlasWinPack ]

    // NuGet Sign (all or nothing)
    if isWindows && hasBuildParam "sign" then signNuGet fingerprint timeserver openBlasSolution

    )
"Prepare" ==> "OpenBlasWinBuild"


// --------------------------------------------------------------------------------------
// TEST
// --------------------------------------------------------------------------------------

let testNumerics framework = test "src/Numerics.Tests" "Numerics.Tests.csproj" framework
Target "TestNumerics" DoNothing
Target "TestNumericsCore1.1" (fun _ -> testNumerics "netcoreapp1.1")
Target "TestNumericsCore2.0" (fun _ -> testNumerics "netcoreapp2.0")
Target "TestNumericsNET40" (fun _ -> testNumerics "net40")
Target "TestNumericsNET45" (fun _ -> testNumerics "net45")
Target "TestNumericsNET461" (fun _ -> testNumerics "net461")
Target "TestNumericsNET47"  (fun _ -> testNumerics "net47")
"Build" ==> "TestNumericsCore1.1"
"Build" ==> "TestNumericsCore2.0" ==> "TestNumerics"
"Build" =?> ("TestNumericsNET40", isWindows)
"Build" =?> ("TestNumericsNET45", isWindows)
"Build" =?> ("TestNumericsNET461", isWindows) ==> "TestNumerics"
"Build" =?> ("TestNumericsNET47", isWindows)
let testFsharp framework = test "src/FSharp.Tests" "FSharp.Tests.fsproj" framework
Target "TestFsharp" DoNothing
Target "TestFsharpCore1.1" (fun _ -> testFsharp "netcoreapp1.1")
Target "TestFsharpCore2.0" (fun _ -> testFsharp "netcoreapp2.0")
Target "TestFsharpNET45" (fun _ -> testFsharp "net45")
Target "TestFsharpNET461" (fun _ -> testFsharp "net461")
Target "TestFsharpNET47" (fun _ -> testFsharp "net47")
"Build" ==> "TestFsharpCore1.1"
"Build" ==> "TestFsharpCore2.0" ==> "TestFsharp"
"Build" =?> ("TestFsharpNET45", isWindows)
"Build" =?> ("TestFsharpNET461", isWindows) ==> "TestFsharp"
"Build" =?> ("TestFsharpNET47", isWindows)
Target "Test" DoNothing
"TestNumerics" ==> "Test"
"TestFsharp" ==> "Test"

let testMKL framework = test "src/Numerics.Tests" "Numerics.Tests.MKL.csproj" framework
Target "MklTest" DoNothing
Target "MklTestCore2.0" (fun _ -> testMKL "netcoreapp2.0")
Target "MklTestNET40" (fun _ -> testMKL "net40")
"MklWinBuild" ==> "MklTestCore2.0" ==> "MklTest"
"MklWinBuild" =?> ("MklTestNET40", isWindows) ==> "MklTest"

let testOpenBLAS framework = test "src/Numerics.Tests" "Numerics.Tests.OpenBLAS.csproj" framework
Target "OpenBlasTest" DoNothing
Target "OpenBlasTestCore2.0" (fun _ -> testOpenBLAS "netcoreapp2.0")
Target "OpenBlasTestNET40" (fun _ -> testOpenBLAS "net40")
"OpenBlasWinBuild" ==> "OpenBlasTestCore2.0" ==> "OpenBlasTest"
"OpenBlasWinBuild" =?> ("OpenBlasTestNET40", isWindows) ==> "OpenBlasTest"

let testCUDA framework = test "src/Numerics.Tests" "Numerics.Tests.CUDA.csproj" framework
Target "CudaTest" DoNothing
Target "CudaTestCore2.0" (fun _ -> testCUDA "netcoreapp2.0")
Target "CudaTestNET40" (fun _ -> testCUDA "net40")
"CudaWinBuild" ==> "CudaTestCore2.0" ==> "CudaTest"
"CudaWinBuild" =?> ("CudaTestNET40", isWindows) ==> "CudaTest"

let testData framework = test "src/Data.Tests" "Data.Tests.csproj" framework
Target "DataTest" DoNothing
Target "DataTestCore1.1" (fun _ -> testData "netcoreapp1.1")
Target "DataTestCore2.0" (fun _ -> testData "netcoreapp2.0")
Target "DataTestNET45" (fun _ -> testData "net45")
"DataBuild" ==> "DataTestCore1.1"
"DataBuild" ==> "DataTestCore2.0" ==> "DataTest"
"DataBuild" =?> ("DataTestNET45", isWindows) ==> "DataTest"


// --------------------------------------------------------------------------------------
// LINUX PACKAGES
// --------------------------------------------------------------------------------------

Target "MklLinuxPack" DoNothing

Target "MklLinuxZip" (fun _ ->
    CreateDir mklSolution.OutputZipDir
    zip mklLinuxZipPackage mklSolution.OutputZipDir "out/MKL/Linux" (fun f -> f.Contains("MathNet.Numerics.MKL.") || f.Contains("libiomp5.so")))
"MklLinuxZip" ==> "MklLinuxPack"

Target "MklLinuxNuGet" (fun _ ->
    CreateDir mklSolution.OutputNuGetDir
    nugetPackManually mklSolution [ mklLinuxPack; mklLinux32Pack; mklLinux64Pack ])
"MklLinuxNuGet" ==> "MklLinuxPack"


// --------------------------------------------------------------------------------------
// Documentation
// --------------------------------------------------------------------------------------

// DOCS

Target "CleanDocs" (fun _ -> CleanDirs ["out/docs"])

let extraDocs =
    [ "LICENSE.md", "License.md"
      "CONTRIBUTING.md", "Contributing.md"
      "CONTRIBUTORS.md", "Contributors.md" ]

Target "Docs" (fun _ ->
    provideDocExtraFiles extraDocs releases
    generateDocs true false)
Target "DocsDev" (fun _ ->
    provideDocExtraFiles  extraDocs releases
    generateDocs true true)
Target "DocsWatch" (fun _ ->
    provideDocExtraFiles  extraDocs releases
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

"Build" ==> "CleanDocs" ==> "Docs"

"Start"
  =?> ("CleanDocs", not (hasBuildParam "incremental"))
  ==> "DocsDev"
  ==> "DocsWatch"


// API REFERENCE

Target "CleanApi" (fun _ -> CleanDirs ["out/api"])

Target "Api" (fun _ ->
    !! "src/Numerics/bin/Release/net40/MathNet.Numerics.dll"
    |> Docu (fun p ->
        { p with
            ToolPath = "tools/docu/docu.exe"
            TemplatesPath = "tools/docu/templates/"
            TimeOut = TimeSpan.FromMinutes 10.
            OutputPath = "out/api/" }))

"Build" ==> "CleanApi" ==> "Api"


// --------------------------------------------------------------------------------------
// Publishing
// Requires permissions; intended only for maintainers
// --------------------------------------------------------------------------------------

Target "PublishTag" (fun _ -> publishReleaseTag "Math.NET Numerics" "" numericsRelease)
Target "DataPublishTag" (fun _ -> publishReleaseTag "Math.NET Numerics Data Extensions" "data-" dataRelease)
Target "MklPublishTag" (fun _ -> publishReleaseTag "Math.NET Numerics MKL Provider" "mkl-" mklRelease)
Target "CudaPublishTag" (fun _ -> publishReleaseTag "Math.NET Numerics CUDA Provider" "cuda-" cudaRelease)
Target "OpenBlasPublishTag" (fun _ -> publishReleaseTag "Math.NET Numerics OpenBLAS Provider" "openblas-" openBlasRelease)

Target "PublishMirrors" (fun _ -> publishMirrors ())
Target "PublishDocs" (fun _ -> publishDocs numericsRelease)
Target "PublishApi" (fun _ -> publishApi numericsRelease)

Target "PublishArchive" (fun _ -> publishArchive numericsSolution)
Target "DataPublishArchive" (fun _ -> publishArchive dataSolution)
Target "MklPublishArchive" (fun _ -> publishArchive mklSolution)
Target "CudaPublishArchive" (fun _ -> publishArchive cudaSolution)
Target "OpenBlasPublishArchive" (fun _ -> publishArchive openBlasSolution)

Target "PublishNuGet" (fun _ -> publishNuGet !! (numericsSolution.OutputNuGetDir </> "/*.nupkg"))
Target "DataPublishNuGet" (fun _ -> publishNuGet !! (dataSolution.OutputNuGetDir </> "/*.nupkg"))
Target "MklPublishNuGet" (fun _ -> publishNuGet !! (mklSolution.OutputNuGetDir </> "/*.nupkg"))
Target "CudaPublishNuGet" (fun _ -> publishNuGet !! (cudaSolution.OutputNuGetDir </> "/*.nupkg"))
Target "OpenBlasPublishNuGet" (fun _ -> publishNuGet !! (openBlasSolution.OutputNuGetDir </> "/*.nupkg"))

Target "Publish" DoNothing
Dependencies "Publish" [ "PublishTag"; "PublishDocs"; "PublishApi"; "PublishArchive"; "PublishNuGet" ]

Target "DataPublish" DoNothing
Dependencies "DataPublish" [ "DataPublishTag"; "DataPublishArchive"; "DataPublishNuGet" ]

Target "MklPublish" DoNothing
Dependencies "MklPublish" [ "MklPublishTag"; "PublishDocs"; "MklPublishArchive"; "MklPublishNuGet" ]

Target "CudaPublish" DoNothing
Dependencies "CudaPublish" [ "CudaPublishTag"; "PublishDocs"; "CudaPublishArchive"; "CudaPublishNuGet" ]

Target "OpenBlasPublish" DoNothing
Dependencies "OpenBlasPublish" [ "OpenBlasPublishTag"; "PublishDocs"; "OpenBlasPublishArchive"; "OpenBlasPublishNuGet" ]


// --------------------------------------------------------------------------------------
// Default Targets
// --------------------------------------------------------------------------------------

Target "All" DoNothing
Dependencies "All" [ "Build"; "Docs"; "Api"; "Test" ]

Target "DataAll" DoNothing
Dependencies "DataAll" [ "DataBuild"; "DataTest" ]

Target "MklWinAll" DoNothing
Dependencies "MklWinAll" [ "MklWinBuild"; "MklTest" ]

Target "CudaWinAll" DoNothing
Dependencies "CudaWinAll" [ "CudaWinBuild"; "CudaTest" ]

Target "OpenBlasWinAll" DoNothing
Dependencies "OpenBlasWinAll" [ "OpenBlasWinBuild"; "OpenBlasTest" ]

RunTargetOrDefault "Test"
