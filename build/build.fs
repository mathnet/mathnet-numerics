let header = """
  __  __       _   _       _   _ ______ _______
 |  \/  |     | | | |     | \ | |  ____|__   __|
 | \  / | __ _| |_| |__   |  \| | |__     | |
 | |\/| |/ _` | __| '_ \  | . ` |  __|    | |
 | |  | | (_| | |_| | | |_| |\  | |____   | |
 |_|  |_|\__,_|\__|_| |_(_)_| \_|______|  |_|

 Math.NET Numerics - https://numerics.mathdotnet.com
 Copyright (c) Math.NET - Open Source MIT/X11 License

 FAKE build script, see https://fake.build/
"""

open FSharp.Core
open Fake.Core
open Fake.Core.TargetOperators
open Fake.DotNet
open Fake.IO
open Fake.IO.FileSystemOperators
open Fake.IO.Globbing.Operators
open System

open Model
open Building
open Testing
open Packaging
open Documentation
open Publishing


// --------------------------------------------------------------------------------------
// PRODUCT DEFINITION
// --------------------------------------------------------------------------------------

// VERSION OVERVIEW

let numericsRelease = release "numerics" "Math.NET Numerics" "RELEASENOTES.md"
let mklRelease = release "numerics" "MKL Provider" "RELEASENOTES-MKL.md"
let cudaRelease = release "numerics" "CUDA Provider" "RELEASENOTES-CUDA.md"
let openBlasRelease = release "numerics" "OpenBLAS Provider" "RELEASENOTES-OpenBLAS.md"
let releases = [ numericsRelease; mklRelease; openBlasRelease ] // skip cuda


// NUMERICS PACKAGES

let numericsZipPackage = zipPackage "MathNet.Numerics" "Math.NET Numerics" numericsRelease
let numericsStrongNameZipPackage = zipPackage "MathNet.Numerics.Signed" "Math.NET Numerics" numericsRelease

let numericsNuGetPackage = nugetPackage "MathNet.Numerics" numericsRelease
let numericsFSharpNuGetPackage = nugetPackage "MathNet.Numerics.FSharp" numericsRelease
let numericsProvidersMklNuGetPackage = nugetPackage "MathNet.Numerics.Providers.MKL" numericsRelease
let numericsProvidersOpenBlasNuGetPackage = nugetPackage "MathNet.Numerics.Providers.OpenBLAS" numericsRelease
let numericsProvidersCudaNuGetPackage = nugetPackage "MathNet.Numerics.Providers.CUDA" numericsRelease
let numericsDataTextNuGetPackage = nugetPackage "MathNet.Numerics.Data.Text" numericsRelease
let numericsDataMatlabNuGetPackage = nugetPackage "MathNet.Numerics.Data.Matlab" numericsRelease

let numericsStrongNameNuGetPackage = nugetPackage "MathNet.Numerics.Signed" numericsRelease
let numericsFSharpStrongNameNuGetPackage = nugetPackage "MathNet.Numerics.FSharp.Signed" numericsRelease
let numericsProvidersMklStrongNameNuGetPackage = nugetPackage "MathNet.Numerics.Providers.MKL.Signed" numericsRelease
let numericsProvidersOpenBlasStrongNameNuGetPackage = nugetPackage "MathNet.Numerics.Providers.OpenBLAS.Signed" numericsRelease
let numericsProvidersCudaStrongNameNuGetPackage = nugetPackage "MathNet.Numerics.Providers.CUDA.Signed" numericsRelease
let numericsDataTextStrongNameNuGetPackage = nugetPackage "MathNet.Numerics.Data.Text.Signed" numericsRelease
let numericsDataMatlabStrongNameNuGetPackage = nugetPackage "MathNet.Numerics.Data.Matlab.Signed" numericsRelease

let numericsProject = project "MathNet.Numerics" "src/Numerics/Numerics.csproj" [numericsNuGetPackage; numericsStrongNameNuGetPackage]
let numericsFsharpProject = project "MathNet.Numerics.FSharp" "src/FSharp/FSharp.fsproj" [numericsFSharpNuGetPackage; numericsFSharpStrongNameNuGetPackage]
let numericsProvidersMklProject = project "MathNet.Numerics.Providers.MKL" "src/Providers.MKL/Providers.MKL.csproj" [numericsProvidersMklNuGetPackage; numericsProvidersMklStrongNameNuGetPackage]
let numericsProvidersOpenBlasProject = project "MathNet.Numerics.Providers.OpenBLAS" "src/Providers.OpenBLAS/Providers.OpenBLAS.csproj" [numericsProvidersOpenBlasNuGetPackage; numericsProvidersOpenBlasStrongNameNuGetPackage]
let numericsProvidersCudaProject = project "MathNet.Numerics.Providers.CUDA" "src/Providers.CUDA/Providers.CUDA.csproj" [numericsProvidersCudaNuGetPackage; numericsProvidersCudaStrongNameNuGetPackage]
let numericsDataTextProject = project "MathNet.Numerics.Data.Text" "src/Data.Text/Data.Text.csproj" [numericsDataTextNuGetPackage; numericsDataTextStrongNameNuGetPackage]
let numericsDataMatlabProject = project "MathNet.Numerics.Data.Matlab" "src/Data.Matlab/Data.Matlab.csproj" [numericsDataMatlabNuGetPackage; numericsDataMatlabStrongNameNuGetPackage]
let numericsSolution = solution "Numerics" "MathNet.Numerics.sln" [numericsProject; numericsFsharpProject; numericsProvidersMklProject; numericsProvidersOpenBlasProject; numericsProvidersCudaProject; numericsDataTextProject; numericsDataMatlabProject] [numericsZipPackage; numericsStrongNameZipPackage]


// MKL NATIVE PROVIDER PACKAGES

let mklWinZipPackage = zipPackage "MathNet.Numerics.MKL.Win" "Math.NET Numerics MKL Native Provider for Windows" mklRelease
let mklLinuxZipPackage = zipPackage "MathNet.Numerics.MKL.Linux" "Math.NET Numerics MKL Native Provider for Linux" mklRelease

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
      Dependencies = [ numericsProvidersMklNuGetPackage.Id, numericsProvidersMklNuGetPackage.Release.PackageVersion ]
      Title = "Math.NET Numerics - MKL Native Provider for Windows (x64 and x86)" }

let mklWin32Pack =
    { NuGet = mklWin32NuGetPackage
      NuSpecFile = "build/MathNet.Numerics.MKL.Win-x86.nuspec"
      Dependencies = [ numericsProvidersMklNuGetPackage.Id, numericsProvidersMklNuGetPackage.Release.PackageVersion ]
      Title = "Math.NET Numerics - MKL Native Provider for Windows (x86)" }

let mklWin64Pack =
    { NuGet = mklWin64NuGetPackage
      NuSpecFile = "build/MathNet.Numerics.MKL.Win-x64.nuspec"
      Dependencies = [ numericsProvidersMklNuGetPackage.Id, numericsProvidersMklNuGetPackage.Release.PackageVersion ]
      Title = "Math.NET Numerics - MKL Native Provider for Windows (x64)" }

let mklLinuxPack =
    { NuGet = mklLinuxNuGetPackage
      NuSpecFile = "build/MathNet.Numerics.MKL.Linux.nuspec"
      Dependencies = [ numericsProvidersMklNuGetPackage.Id, numericsProvidersMklNuGetPackage.Release.PackageVersion ]
      Title = "Math.NET Numerics - MKL Native Provider for Linux (x64 and x86)" }

let mklLinux32Pack =
    { NuGet = mklLinux32NuGetPackage
      NuSpecFile = "build/MathNet.Numerics.MKL.Linux-x86.nuspec"
      Dependencies = [ numericsProvidersMklNuGetPackage.Id, numericsProvidersMklNuGetPackage.Release.PackageVersion ]
      Title = "Math.NET Numerics - MKL Native Provider for Linux (x86)" }

let mklLinux64Pack =
    { NuGet = mklLinux64NuGetPackage
      NuSpecFile = "build/MathNet.Numerics.MKL.Linux-x64.nuspec"
      Dependencies = [ numericsProvidersMklNuGetPackage.Id, numericsProvidersMklNuGetPackage.Release.PackageVersion ]
      Title = "Math.NET Numerics - MKL Native Provider for Linux (x64)" }


// CUDA NATIVE PROVIDER PACKAGES

let cudaWinZipPackage = zipPackage "MathNet.Numerics.CUDA.Win" "Math.NET Numerics CUDA Native Provider for Windows" cudaRelease
let cudaWinNuGetPackage = nugetPackage "MathNet.Numerics.CUDA.Win" cudaRelease

let cudaWinProject = nativeProject "MathNet.Numerics.CUDA" "src/NativeProviders/Windows/CUDA/CUDAWrapper.vcxproj" [cudaWinNuGetPackage]
let cudaSolution = solution "CUDA" "MathNet.Numerics.CUDA.sln" [cudaWinProject] [cudaWinZipPackage]

let cudaWinPack =
    { NuGet = cudaWinNuGetPackage
      NuSpecFile = "build/MathNet.Numerics.CUDA.Win.nuspec"
      Dependencies = [ numericsProvidersCudaNuGetPackage.Id, numericsProvidersCudaNuGetPackage.Release.PackageVersion ]
      Title = "Math.NET Numerics - CUDA Native Provider for Windows (x64)" }


// OpenBLAS NATIVE PROVIDER PACKAGES

let openBlasWinZipPackage = zipPackage "MathNet.Numerics.OpenBLAS.Win" "Math.NET Numerics OpenBLAS Native Provider for Windows" openBlasRelease
let openBlasWinNuGetPackage = nugetPackage "MathNet.Numerics.OpenBLAS.Win" openBlasRelease

let openBlasWinProject = nativeProject "MathNet.Numerics.OpenBLAS" "src/NativeProviders/Windows/OpenBLAS/OpenBLASWrapper.vcxproj" [openBlasWinNuGetPackage]
let openBlasSolution = solution "OpenBLAS" "MathNet.Numerics.OpenBLAS.sln" [openBlasWinProject] [openBlasWinZipPackage]

let openBlasWinPack =
    { NuGet = openBlasWinNuGetPackage
      NuSpecFile = "build/MathNet.Numerics.OpenBLAS.Win.nuspec"
      Dependencies = [ numericsProvidersOpenBlasNuGetPackage.Id, numericsProvidersOpenBlasNuGetPackage.Release.PackageVersion ]
      Title = "Math.NET Numerics - OpenBLAS Native Provider for Windows (x64 and x86)" }


// ALL

let allSolutions = [numericsSolution]
let allProjects = allSolutions |> List.collect (fun s -> s.Projects) |> List.distinct


// --------------------------------------------------------------------------------------
// BUILD STEPS FOR DEFINED PRODUCTS
// --------------------------------------------------------------------------------------

let ``Clean`` _ =
    Shell.deleteDirs (!! "src/**/obj/" ++ "src/**/bin/" )
    Shell.cleanDirs [ "out/api"; "out/docs" ]
    Shell.cleanDirs [ "out/MKL"; "out/ATLAS"; "out/CUDA"; "out/OpenBLAS" ] // Native Providers
    allSolutions |> List.iter (fun solution -> Shell.cleanDirs [ solution.OutputZipDir; solution.OutputNuGetDir; solution.OutputLibDir; solution.OutputLibStrongNameDir ])

let ``Apply Version`` _ =
    allProjects |> List.iter Versioning.updateProject
    Versioning.updateNativeResource "src/NativeProviders/MKL/resource.rc" mklRelease
    Versioning.updateNativeResource "src/NativeProviders/CUDA/resource.rc" cudaRelease
    Versioning.updateNativeResource "src/NativeProviders/OpenBLAS/resource.rc" openBlasRelease

let ``Restore`` _ =
    allSolutions |> List.iter restore

let fingerprint = "490408de3618bed0a28e68dc5face46e5a3a97dd"
let timeserver = "http://time.certum.pl/"

let ``Build`` isStrongname isSign _ =

    // Strong Name Build (with strong name, without certificate signature)
    if isStrongname then
        Shell.cleanDirs (!! "src/**/obj/" ++ "src/**/bin/" )
        restore numericsSolution
        buildStrongNamed numericsSolution
        if isSign then sign fingerprint timeserver numericsSolution
        collectBinariesSN numericsSolution
        zip numericsStrongNameZipPackage header numericsSolution.OutputZipDir numericsSolution.OutputLibStrongNameDir (fun f -> f.Contains("MathNet.Numerics.") || f.Contains("System.Threading.") || f.Contains("FSharp.Core."))
        packStrongNamed numericsSolution
        collectNuGetPackages numericsSolution

    // Normal Build (without strong name, with certificate signature)
    Shell.cleanDirs (!! "src/**/obj/" ++ "src/**/bin/" )
    restore numericsSolution
    build numericsSolution
    if isSign then sign fingerprint timeserver numericsSolution
    collectBinaries numericsSolution
    zip numericsZipPackage header numericsSolution.OutputZipDir numericsSolution.OutputLibDir (fun f -> f.Contains("MathNet.Numerics.") || f.Contains("System.Threading.") || f.Contains("FSharp.Core."))
    pack numericsSolution
    collectNuGetPackages numericsSolution

    // NuGet Sign (all or nothing)
    if isSign then signNuGet fingerprint timeserver [numericsSolution]

let ``Build MKL Windows`` isIncremental isSign _ =

    //let result =
    //    CreateProcess.fromRawCommandLine "cmd.exe" "/c setvars.bat"
    //    |> CreateProcess.withWorkingDirectory (Environment.GetEnvironmentVariable("ONEAPI_ROOT"))
    //    |> CreateProcess.withTimeout (TimeSpan.FromMinutes 10.)
    //    |> Proc.run
    //if result.ExitCode <> 0 then failwith "Error while setting oneAPI environment variables."

    restore mklSolution
    buildVS2022x86 "Release-MKL" isIncremental !! "MathNet.Numerics.MKL.sln"
    buildVS2022x64 "Release-MKL" isIncremental !! "MathNet.Numerics.MKL.sln"
    Directory.create mklSolution.OutputZipDir
    zip mklWinZipPackage header mklSolution.OutputZipDir "out/MKL/Windows" (fun f -> f.Contains("MathNet.Numerics.Providers.MKL.") || f.Contains("libMathNetNumerics") || f.Contains("libiomp5md.dll"))
    Directory.create mklSolution.OutputNuGetDir
    nugetPackManually mklSolution [ mklWinPack; mklWin32Pack; mklWin64Pack ] "LICENSE-MKL.md" header

    // NuGet Sign (all or nothing)
    if isSign then signNuGet fingerprint timeserver [mklSolution]

let ``Build CUDA Windows`` isIncremental isSign _ =

    restore cudaSolution
    buildVS2022x64 "Release-CUDA" isIncremental !! "MathNet.Numerics.CUDA.sln"
    Directory.create cudaSolution.OutputZipDir
    zip cudaWinZipPackage header cudaSolution.OutputZipDir "out/CUDA/Windows" (fun f -> f.Contains("MathNet.Numerics.Providers.CUDA.") || f.Contains("libMathNetNumerics") || f.Contains("cublas") || f.Contains("cudart") || f.Contains("cusolver"))
    Directory.create cudaSolution.OutputNuGetDir
    nugetPackManually cudaSolution [ cudaWinPack ] "LICENSE.md" header

    // NuGet Sign (all or nothing)
    if isSign then signNuGet fingerprint timeserver [cudaSolution]

let ``Build OpenBLAS Windows`` isIncremental isSign _ =

    restore openBlasSolution
    buildVS2022x86 "Release-OpenBLAS" isIncremental !! "MathNet.Numerics.OpenBLAS.sln"
    buildVS2022x64 "Release-OpenBLAS" isIncremental !! "MathNet.Numerics.OpenBLAS.sln"
    Directory.create openBlasSolution.OutputZipDir
    zip openBlasWinZipPackage header openBlasSolution.OutputZipDir "out/OpenBLAS/Windows" (fun f -> f.Contains("MathNet.Numerics.Providers.OpenBLAS.") || f.Contains("libMathNetNumerics") || f.Contains("libgcc") || f.Contains("libgfortran") || f.Contains("libopenblas") || f.Contains("libquadmath"))
    Directory.create openBlasSolution.OutputNuGetDir
    nugetPackManually openBlasSolution [ openBlasWinPack ] "LICENSE.md" header

    // NuGet Sign (all or nothing)
    if isSign then signNuGet fingerprint timeserver [openBlasSolution]

let ``Test Numerics`` framework _ = test "src/Numerics.Tests" "Numerics.Tests.csproj" framework
let ``Test FSharp`` framework _ = test "src/FSharp.Tests" "FSharp.Tests.fsproj" framework
let ``Test Data`` framework _ = test "src/Data.Tests" "Data.Tests.csproj" framework
let ``Test MKL`` framework _ = test "src/Numerics.Tests" "Numerics.Tests.MKL.csproj" framework
let ``Test OpenBLAS`` framework _ = test "src/Numerics.Tests" "Numerics.Tests.OpenBLAS.csproj" framework
let ``Test CUDA`` framework _ = test "src/Numerics.Tests" "Numerics.Tests.CUDA.csproj" framework

let ``Pack MKL Linux Zip`` _ =
    Directory.create mklSolution.OutputZipDir
    zip mklLinuxZipPackage header mklSolution.OutputZipDir "out/MKL/Linux" (fun f -> f.Contains("MathNet.Numerics.Providers.MKL.") || f.Contains("libMathNetNumerics") || f.Contains("libiomp5.so"))

let ``Pack MKL Linux NuGet`` _ =
    Directory.create mklSolution.OutputNuGetDir
    nugetPackManually mklSolution [ mklLinuxPack; mklLinux32Pack; mklLinux64Pack ] "LICENSE-MKL.md" header

let ``Pack MKL Windows`` _ =
    Directory.create mklSolution.OutputZipDir
    zip mklWinZipPackage header mklSolution.OutputZipDir "out/MKL/Windows" (fun f -> f.Contains("MathNet.Numerics.Providers.MKL.") || f.Contains("libMathNetNumerics") || f.Contains("libiomp5md.dll"))
    Directory.create mklSolution.OutputNuGetDir
    nugetPackManually mklSolution [ mklWinPack; mklWin32Pack; mklWin64Pack ] "LICENSE-MKL.md" header

let ``Pack OpenBLAS Windows`` _ =
    Directory.create openBlasSolution.OutputZipDir
    zip openBlasWinZipPackage header openBlasSolution.OutputZipDir "out/OpenBLAS/Windows" (fun f -> f.Contains("MathNet.Numerics.Providers.OpenBLAS.") || f.Contains("libMathNetNumerics") || f.Contains("libgcc") || f.Contains("libgfortran") || f.Contains("libopenblas") || f.Contains("libquadmath"))
    Directory.create openBlasSolution.OutputNuGetDir
    nugetPackManually openBlasSolution [ openBlasWinPack ] "LICENSE.md" header

let extraDocs =
    [ "LICENSE.md", "License.md"
      "LICENSE-MKL.md", "License-MKL.md"
      "CONTRIBUTING.md", "Contributing.md"
      "CONTRIBUTORS.md", "Contributors.md" ]

let ``Docs Clean`` _ =
    Shell.cleanDirs ["out/docs"]

let ``Docs Build`` _ =
    provideDocExtraFiles extraDocs releases
    buildDocs "out/docs"

let ``Docs Build and Watch`` _ =
    provideDocExtraFiles extraDocs releases
    buildDocs "out/docs"
    watchDocs "out/docs"

let ``API Clean`` _ =
    Shell.cleanDirs ["out/api"]

let ``API Build`` _ =
    let rootDir = Environment.CurrentDirectory
    let result =
        CreateProcess.fromRawCommandLine
            "tools/docu/docu.exe"
            ([
                rootDir </> "src/Numerics/bin/Release/net461/MathNet.Numerics.dll" |> Path.getFullName
                "--output=" + (rootDir </> "out/api/" |> Path.getFullName)
                "--templates=" + (rootDir </> "tools/docu/templates/" |> Path.getFullName)
             ] |> String.concat " ")
        |> CreateProcess.withWorkingDirectory rootDir
        |> CreateProcess.withTimeout (TimeSpan.FromMinutes 10.)
        |> Proc.run
    if result.ExitCode <> 0 then failwith "Error during API reference generation."


// --------------------------------------------------------------------------------------
// BUILD TARGETS
// --------------------------------------------------------------------------------------

let initTargets strongname sign incremental =

    // PREPARE
    Target.create "Start" ignore
    Target.create "Clean" ``Clean``
    Target.create "ApplyVersion" ``Apply Version``
    Target.create "Restore" ``Restore``
    "Start" =?> ("Clean", not incremental) ==> "Restore" |> ignore
    Target.create "Prepare" ignore
    "Start" =?> ("Clean", not incremental) ==> "ApplyVersion" ==> "Prepare" |> ignore

    // BUILD, SIGN, COLLECT
    Target.create "Build" (``Build`` strongname sign)
    "Prepare" ==> "Build" |> ignore
    Target.create "MklWinBuild" (``Build MKL Windows`` incremental sign)
    "Prepare" ==> "MklWinBuild" |> ignore
    Target.create "CudaWinBuild" (``Build CUDA Windows`` incremental sign)
    "Prepare" ==> "CudaWinBuild" |> ignore
    Target.create "OpenBlasWinBuild" (``Build OpenBLAS Windows`` incremental sign)
    "Prepare" ==> "OpenBlasWinBuild" |> ignore

    // TEST
    Target.create "TestNumerics" ignore
    Target.create "TestNumericsNET60" (``Test Numerics`` "net6.0")
    Target.create "TestNumericsNET48" (``Test Numerics`` "net48")
    "Build" ==> "TestNumericsNET60" ==> "TestNumerics" |> ignore
    "Build" =?> ("TestNumericsNET48", Environment.isWindows) ==> "TestNumerics" |> ignore
    Target.create "TestFsharp" ignore
    Target.create "TestFsharpNET60" (``Test FSharp`` "net6.0")
    Target.create "TestFsharpNET48" (``Test FSharp`` "net48")
    "Build" ==> "TestFsharpNET60" ==> "TestFsharp" |> ignore
    "Build" =?> ("TestFsharpNET48", Environment.isWindows) ==> "TestFsharp" |> ignore
    Target.create "TestData" ignore
    Target.create "TestDataNET60" (``Test Data`` "net6.0")
    Target.create "TestDataNET48" (``Test Data`` "net48")
    "Build" ==> "TestDataNET60" ==> "TestData" |> ignore
    "Build" =?> ("TestDataNET48", Environment.isWindows) ==> "TestData" |> ignore
    Target.create "Test" ignore
    "TestNumerics" ==> "Test" |> ignore
    "TestFsharp" ==> "Test" |> ignore
    "TestData" ==> "Test" |> ignore
    Target.create "MklTest" ignore
    Target.create "MklTestNET60" (``Test MKL`` "net6.0")
    Target.create "MklTestNET48" (``Test MKL`` "net48")
    "MklWinBuild" ==> "MklTestNET60" ==> "MklTest" |> ignore
    "MklWinBuild" =?> ("MklTestNET48", Environment.isWindows) ==> "MklTest" |> ignore
    Target.create "OpenBlasTest" ignore
    Target.create "OpenBlasTestNET60" (``Test OpenBLAS`` "net6.0")
    Target.create "OpenBlasTestNET48" (``Test OpenBLAS`` "net48")
    "OpenBlasWinBuild" ==> "OpenBlasTestNET60" ==> "OpenBlasTest" |> ignore
    "OpenBlasWinBuild" =?> ("OpenBlasTestNET48", Environment.isWindows) ==> "OpenBlasTest" |> ignore
    Target.create "CudaTest" ignore
    Target.create "CudaTestNET60" (``Test CUDA`` "net6.0")
    Target.create "CudaTestNET48" (``Test CUDA`` "net48")
    "CudaWinBuild" ==> "CudaTestNET60" ==> "CudaTest" |> ignore
    "CudaWinBuild" =?> ("CudaTestNET48", Environment.isWindows) ==> "CudaTest" |> ignore

    // PACKAGING ONLY WITHOUT BUILD
    Target.create "MklLinuxPack" ignore
    Target.create "MklLinuxZip" ``Pack MKL Linux Zip``
    "MklLinuxZip" ==> "MklLinuxPack" |> ignore
    Target.create "MklLinuxNuGet" ``Pack MKL Linux NuGet``
    "MklLinuxNuGet" ==> "MklLinuxPack" |> ignore
    Target.create "MklWinPack" ``Pack MKL Windows``
    Target.create "OpenBlasWinPack" ``Pack OpenBLAS Windows``

    // DOCS
    Target.create "CleanDocs" ``Docs Clean``
    Target.create "Docs" ``Docs Build``
    Target.create "DocsDev" ``Docs Build``
    Target.create "DocsWatch" ``Docs Build and Watch``
    "Build" ==> "CleanDocs" ==> "Docs" |> ignore
    "Start" =?> ("CleanDocs", not incremental) ==> "DocsDev" ==> "DocsWatch" |> ignore

    // API REFERENCE
    Target.create "CleanApi" ``API Clean``
    Target.create "Api" ``API Build``
    "Build" ==> "CleanApi" ==> "Api" |> ignore

    // PUBLISHING
    // Requires permissions; intended only for maintainers
    Target.create "PublishTag" (fun _ -> publishReleaseTag "Math.NET Numerics" "" numericsRelease)
    Target.create "MklPublishTag" (fun _ -> publishReleaseTag "Math.NET Numerics MKL Provider" "mkl-" mklRelease)
    Target.create "CudaPublishTag" (fun _ -> publishReleaseTag "Math.NET Numerics CUDA Provider" "cuda-" cudaRelease)
    Target.create "OpenBlasPublishTag" (fun _ -> publishReleaseTag "Math.NET Numerics OpenBLAS Provider" "openblas-" openBlasRelease)
    Target.create "PublishDocs" (fun _ -> publishDocs numericsRelease)
    Target.create "PublishApi" (fun _ -> publishApi numericsRelease)
    Target.create "PublishArchive" (fun _ -> publishArchives [numericsSolution])
    Target.create "MklPublishArchive" (fun _ -> publishArchives [mklSolution])
    Target.create "CudaPublishArchive" (fun _ -> publishArchives [cudaSolution])
    Target.create "OpenBlasPublishArchive" (fun _ -> publishArchives [openBlasSolution])
    Target.create "PublishNuGet" (fun _ -> publishNuGet [numericsSolution])
    Target.create "MklPublishNuGet" (fun _ -> publishNuGet [mklSolution])
    Target.create "CudaPublishNuGet" (fun _ -> publishNuGet [cudaSolution])
    Target.create "OpenBlasPublishNuGet" (fun _ -> publishNuGet [openBlasSolution])

    // COMPOSITE TARGETS
    Target.create "Publish" ignore
    "Publish" <== [ "PublishTag"; "PublishDocs"; "PublishApi"; "PublishArchive"; "PublishNuGet" ]

    Target.create "MklPublish" ignore
    "MklPublish" <== [ "MklPublishTag"; "PublishDocs"; "MklPublishArchive"; "MklPublishNuGet" ]

    Target.create "CudaPublish" ignore
    "CudaPublish" <== [ "CudaPublishTag"; "PublishDocs"; "CudaPublishArchive"; "CudaPublishNuGet" ]

    Target.create "OpenBlasPublish" ignore
    "OpenBlasPublish" <== [ "OpenBlasPublishTag"; "PublishDocs"; "OpenBlasPublishArchive"; "OpenBlasPublishNuGet" ]

    Target.create "All" ignore
    "All" <== [ "Build"; "Docs"; "Api"; "Test" ]

    Target.create "MklWinAll" ignore
    "MklWinAll" <== [ "MklWinBuild"; "MklTest" ]

    Target.create "CudaWinAll" ignore
    "CudaWinAll" <== [ "CudaWinBuild"; "CudaTest" ]

    Target.create "OpenBlasWinAll" ignore
    "OpenBlasWinAll" <== [ "OpenBlasWinBuild"; "OpenBlasTest" ]


// --------------------------------------------------------------------------------------
// MAIN PROGRAM
// --------------------------------------------------------------------------------------

[<EntryPoint>]
let main argv =

    Environment.CurrentDirectory <- Path.getFullName (__SOURCE_DIRECTORY__ </> "..")
    Trace.log Environment.CurrentDirectory

    argv
    |> Array.toList
    |> Context.FakeExecutionContext.Create false "build.fsx"
    |> Context.RuntimeContext.Fake
    |> Context.setExecutionContext

    Trace.log header
    let titleLength = releases |> List.map (fun r -> r.Title.Length) |> List.max
    for release in releases do
        Trace.log ([ " "; release.Title.PadRight titleLength; "  v"; release.PackageVersion ] |> String.concat "")
    Trace.log ""

    let args = Target.getArguments()
    let isStrongname, isSign, isIncremental =
        match args with
        | Some args ->
            args |> Seq.contains "--strongname",
            args |> Seq.contains "--sign" && Environment.isWindows,
            args |> Seq.contains "--incremental"
        | None -> false, false, false

    if isStrongname then Trace.log " Option: Strongnamed"
    if isSign then Trace.log " Option: Signed"
    if isIncremental then Trace.log " Option: Incremental"
    Trace.log ""

    DotNet.exec id "--info" "" |> ignore<ProcessResult>
    Trace.log ""

    initTargets isStrongname isSign isIncremental

    Target.runOrDefaultWithArguments "Test"

    0
