let header ="""
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
open Fake.IO
open Fake.IO.FileSystemOperators
open Fake.IO.Globbing.Operators
open System

open Model
open Dotnet
open Preparing
open Building
open Packaging
open Documentation
open Publishing

let rootDir = Environment.CurrentDirectory
Trace.log rootDir


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


let initTargets () =

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

    dotnet rootDir "--info"
    Trace.log ""

    // --------------------------------------------------------------------------------------
    // PREPARE
    // --------------------------------------------------------------------------------------
    
    Target.create "Start" ignore
    
    Target.create "Clean" (fun _ ->
        Shell.deleteDirs (!! "src/**/obj/" ++ "src/**/bin/" )
        Shell.cleanDirs [ "out/api"; "out/docs" ]
        Shell.cleanDirs [ "out/MKL"; "out/ATLAS"; "out/CUDA"; "out/OpenBLAS" ] // Native Providers
        allSolutions |> List.iter (fun solution -> Shell.cleanDirs [ solution.OutputZipDir; solution.OutputNuGetDir; solution.OutputLibDir; solution.OutputLibStrongNameDir ]))
    
    Target.create "ApplyVersion" (fun _ ->
        allProjects |> List.iter patchVersionInProjectFile
        patchVersionInResource "src/NativeProviders/MKL/resource.rc" mklRelease
        patchVersionInResource "src/NativeProviders/CUDA/resource.rc" cudaRelease
        patchVersionInResource "src/NativeProviders/OpenBLAS/resource.rc" openBlasRelease)
    
    Target.create "Restore" (fun _ -> allSolutions |> List.iter restoreWeak)
    "Start"
      =?> ("Clean", not isIncremental)
      ==> "Restore"
      |> ignore
    
    Target.create "Prepare" ignore
    "Start"
      =?> ("Clean", not isIncremental)
      ==> "ApplyVersion"
      ==> "Prepare"
      |> ignore
    
    
    // --------------------------------------------------------------------------------------
    // BUILD, SIGN, COLLECT
    // --------------------------------------------------------------------------------------
    
    let fingerprint = "490408de3618bed0a28e68dc5face46e5a3a97dd"
    let timeserver = "http://time.certum.pl/"
    
    Target.create "Build" (fun _ ->
    
        // Strong Name Build (with strong name, without certificate signature)
        if isStrongname then
            Shell.cleanDirs (!! "src/**/obj/" ++ "src/**/bin/" )
            restoreStrong numericsSolution
            buildStrong numericsSolution
            if isSign then sign fingerprint timeserver numericsSolution
            collectBinariesSN numericsSolution
            zip numericsStrongNameZipPackage header numericsSolution.OutputZipDir numericsSolution.OutputLibStrongNameDir (fun f -> f.Contains("MathNet.Numerics.") || f.Contains("System.Threading.") || f.Contains("FSharp.Core."))
            packStrong numericsSolution
            collectNuGetPackages numericsSolution
    
        // Normal Build (without strong name, with certificate signature)
        Shell.cleanDirs (!! "src/**/obj/" ++ "src/**/bin/" )
        restoreWeak numericsSolution
        buildWeak numericsSolution
        if isSign then sign fingerprint timeserver numericsSolution
        collectBinaries numericsSolution
        zip numericsZipPackage header numericsSolution.OutputZipDir numericsSolution.OutputLibDir (fun f -> f.Contains("MathNet.Numerics.") || f.Contains("System.Threading.") || f.Contains("FSharp.Core."))
        packWeak numericsSolution
        collectNuGetPackages numericsSolution
    
        // NuGet Sign (all or nothing)
        if isSign then signNuGet fingerprint timeserver [numericsSolution]
    
        )
    "Prepare" ==> "Build" |> ignore
    
    Target.create "MklWinBuild" (fun _ ->
    
        //let result =
        //    CreateProcess.fromRawCommandLine "cmd.exe" "/c setvars.bat"
        //    |> CreateProcess.withWorkingDirectory (Environment.GetEnvironmentVariable("ONEAPI_ROOT"))
        //    |> CreateProcess.withTimeout (TimeSpan.FromMinutes 10.)
        //    |> Proc.run
        //if result.ExitCode <> 0 then failwith "Error while setting oneAPI environment variables."
    
        restoreWeak mklSolution
        buildVS2019x86 "Release-MKL" isIncremental !! "MathNet.Numerics.MKL.sln"
        buildVS2019x64 "Release-MKL" isIncremental !! "MathNet.Numerics.MKL.sln"
        Directory.create mklSolution.OutputZipDir
        zip mklWinZipPackage header mklSolution.OutputZipDir "out/MKL/Windows" (fun f -> f.Contains("MathNet.Numerics.Providers.MKL.") || f.Contains("MathNet.Numerics.MKL.") || f.Contains("libiomp5md.dll"))
        Directory.create mklSolution.OutputNuGetDir
        nugetPackManually mklSolution [ mklWinPack; mklWin32Pack; mklWin64Pack ] header
    
        // NuGet Sign (all or nothing)
        if isSign then signNuGet fingerprint timeserver [mklSolution]
    
        )
    "Prepare" ==> "MklWinBuild" |> ignore
    
    Target.create "CudaWinBuild" (fun _ ->
    
        restoreWeak cudaSolution
        buildVS2019x64 "Release-CUDA" isIncremental !! "MathNet.Numerics.CUDA.sln"
        Directory.create cudaSolution.OutputZipDir
        zip cudaWinZipPackage header cudaSolution.OutputZipDir "out/CUDA/Windows" (fun f -> f.Contains("MathNet.Numerics.Providers.CUDA.") || f.Contains("MathNet.Numerics.CUDA.") || f.Contains("cublas") || f.Contains("cudart") || f.Contains("cusolver"))
        Directory.create cudaSolution.OutputNuGetDir
        nugetPackManually cudaSolution [ cudaWinPack ] header
    
        // NuGet Sign (all or nothing)
        if isSign then signNuGet fingerprint timeserver [cudaSolution]
    
        )
    "Prepare" ==> "CudaWinBuild" |> ignore
    
    Target.create "OpenBlasWinBuild" (fun _ ->
    
        restoreWeak openBlasSolution
        buildVS2019x86 "Release-OpenBLAS" isIncremental !! "MathNet.Numerics.OpenBLAS.sln"
        buildVS2019x64 "Release-OpenBLAS" isIncremental !! "MathNet.Numerics.OpenBLAS.sln"
        Directory.create openBlasSolution.OutputZipDir
        zip openBlasWinZipPackage header openBlasSolution.OutputZipDir "out/OpenBLAS/Windows" (fun f -> f.Contains("MathNet.Numerics.Providers.OpenBLAS.") || f.Contains("MathNet.Numerics.OpenBLAS.") || f.Contains("libgcc") || f.Contains("libgfortran") || f.Contains("libopenblas") || f.Contains("libquadmath"))
        Directory.create openBlasSolution.OutputNuGetDir
        nugetPackManually openBlasSolution [ openBlasWinPack ] header
    
        // NuGet Sign (all or nothing)
        if isSign then signNuGet fingerprint timeserver [openBlasSolution]
    
        )
    "Prepare" ==> "OpenBlasWinBuild" |> ignore
    
    
    // --------------------------------------------------------------------------------------
    // TEST
    // --------------------------------------------------------------------------------------
    
    let testNumerics framework = test "src/Numerics.Tests" "Numerics.Tests.csproj" framework
    Target.create "TestNumerics" ignore
    Target.create "TestNumericsNET50"  (fun _ -> testNumerics "net5.0")
    Target.create "TestNumericsNET48" (fun _ -> testNumerics "net48")
    "Build" ==> "TestNumericsNET50" ==> "TestNumerics" |> ignore
    "Build" =?> ("TestNumericsNET48", Environment.isWindows) ==> "TestNumerics" |> ignore
    let testFsharp framework = test "src/FSharp.Tests" "FSharp.Tests.fsproj" framework
    Target.create "TestFsharp" ignore
    Target.create "TestFsharpNET50" (fun _ -> testFsharp "net5.0")
    Target.create "TestFsharpNET48" (fun _ -> testFsharp "net48")
    "Build" ==> "TestFsharpNET50" ==> "TestFsharp" |> ignore
    "Build" =?> ("TestFsharpNET48", Environment.isWindows) ==> "TestFsharp" |> ignore
    let testData framework = test "src/Data.Tests" "Data.Tests.csproj" framework
    Target.create "TestData" ignore
    Target.create "TestDataNET50" (fun _ -> testData "net5.0")
    Target.create "TestDataNET48" (fun _ -> testData "net48")
    "Build" ==> "TestDataNET50" ==> "TestData" |> ignore
    "Build" =?> ("TestDataNET48", Environment.isWindows) ==> "TestData" |> ignore
    Target.create "Test" ignore
    "TestNumerics" ==> "Test" |> ignore
    "TestFsharp" ==> "Test" |> ignore
    "TestData" ==> "Test" |> ignore
    
    let testMKL framework = test "src/Numerics.Tests" "Numerics.Tests.MKL.csproj" framework
    Target.create "MklTest" ignore
    Target.create "MklTestNET50" (fun _ -> testMKL "net5.0")
    Target.create "MklTestNET48" (fun _ -> testMKL "net48")
    "MklWinBuild" ==> "MklTestNET50" ==> "MklTest" |> ignore
    "MklWinBuild" =?> ("MklTestNET48", Environment.isWindows) ==> "MklTest" |> ignore
    
    let testOpenBLAS framework = test "src/Numerics.Tests" "Numerics.Tests.OpenBLAS.csproj" framework
    Target.create "OpenBlasTest" ignore
    Target.create "OpenBlasTestNET50" (fun _ -> testOpenBLAS "net5.0")
    Target.create "OpenBlasTestNET48" (fun _ -> testOpenBLAS "net48")
    "OpenBlasWinBuild" ==> "OpenBlasTestNET50" ==> "OpenBlasTest" |> ignore
    "OpenBlasWinBuild" =?> ("OpenBlasTestNET48", Environment.isWindows) ==> "OpenBlasTest" |> ignore
    
    let testCUDA framework = test "src/Numerics.Tests" "Numerics.Tests.CUDA.csproj" framework
    Target.create "CudaTest" ignore
    Target.create "CudaTestNET50" (fun _ -> testCUDA "net5.0")
    Target.create "CudaTestNET48" (fun _ -> testCUDA "net48")
    "CudaWinBuild" ==> "CudaTestNET50" ==> "CudaTest" |> ignore
    "CudaWinBuild" =?> ("CudaTestNET48", Environment.isWindows) ==> "CudaTest" |> ignore


    // --------------------------------------------------------------------------------------
    // LINUX PACKAGES
    // --------------------------------------------------------------------------------------
    
    Target.create "MklLinuxPack" ignore
    
    Target.create "MklLinuxZip" (fun _ ->
        Directory.create mklSolution.OutputZipDir
        zip mklLinuxZipPackage header mklSolution.OutputZipDir "out/MKL/Linux" (fun f -> f.Contains("MathNet.Numerics.Providers.MKL.") || f.Contains("MathNet.Numerics.MKL.") || f.Contains("libiomp5.so")))
    "MklLinuxZip" ==> "MklLinuxPack" |> ignore
    
    Target.create "MklLinuxNuGet" (fun _ ->
        Directory.create mklSolution.OutputNuGetDir
        nugetPackManually mklSolution [ mklLinuxPack; mklLinux32Pack; mklLinux64Pack ] header)
    "MklLinuxNuGet" ==> "MklLinuxPack" |> ignore
    


    // --------------------------------------------------------------------------------------
    // Documentation
    // --------------------------------------------------------------------------------------

    // DOCS

    Target.create "CleanDocs" (fun _ -> Shell.cleanDirs ["out/docs"])

    let extraDocs =
        [ "LICENSE.md", "License.md"
          "CONTRIBUTING.md", "Contributing.md"
          "CONTRIBUTORS.md", "Contributors.md" ]

    Target.create "Docs" (fun _ ->
        provideDocExtraFiles extraDocs releases
        dotnet rootDir "fsdocs build --noapidocs --output out/docs")
    Target.create "DocsDev" (fun _ ->
        provideDocExtraFiles extraDocs releases
        dotnet rootDir "fsdocs build --noapidocs --output out/docs")
    Target.create "DocsWatch" (fun _ ->
        provideDocExtraFiles extraDocs releases
        dotnet rootDir "fsdocs build --noapidocs --output out/docs"
        dotnet rootDir "fsdocs watch --noapidocs --output out/docs")

    "Build" ==> "CleanDocs" ==> "Docs" |> ignore

    "Start"
      =?> ("CleanDocs", not isIncremental)
      ==> "DocsDev"
      ==> "DocsWatch"
      |> ignore


    // API REFERENCE

    Target.create "CleanApi" (fun _ -> Shell.cleanDirs ["out/api"])

    Target.create "Api" (fun _ ->
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
        if result.ExitCode <> 0 then failwith "Error during API reference generation."    )

    "Build" ==> "CleanApi" ==> "Api" |> ignore


    // --------------------------------------------------------------------------------------
    // Publishing
    // Requires permissions; intended only for maintainers
    // --------------------------------------------------------------------------------------
    
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
    
    Target.create "Publish" ignore
    "Publish" <== [ "PublishTag"; "PublishDocs"; "PublishApi"; "PublishArchive"; "PublishNuGet" ]
    
    Target.create "MklPublish" ignore
    "MklPublish" <== [ "MklPublishTag"; "PublishDocs"; "MklPublishArchive"; "MklPublishNuGet" ]
    
    Target.create "CudaPublish" ignore
    "CudaPublish" <== [ "CudaPublishTag"; "PublishDocs"; "CudaPublishArchive"; "CudaPublishNuGet" ]
    
    Target.create "OpenBlasPublish" ignore
    "OpenBlasPublish" <== [ "OpenBlasPublishTag"; "PublishDocs"; "OpenBlasPublishArchive"; "OpenBlasPublishNuGet" ]


    // --------------------------------------------------------------------------------------
    // Default Targets
    // --------------------------------------------------------------------------------------

    Target.create "All" ignore
    "All" <== [ "Build"; "Docs"; "Api"; "Test" ]
    
    Target.create "MklWinAll" ignore
    "MklWinAll" <== [ "MklWinBuild"; "MklTest" ]
    
    Target.create "CudaWinAll" ignore
    "CudaWinAll" <== [ "CudaWinBuild"; "CudaTest" ]
    
    Target.create "OpenBlasWinAll" ignore
    "OpenBlasWinAll" <== [ "OpenBlasWinBuild"; "OpenBlasTest" ]


[<EntryPoint>]
let main argv =

    argv
    |> Array.toList
    |> Context.FakeExecutionContext.Create false "build.fsx"
    |> Context.RuntimeContext.Fake
    |> Context.setExecutionContext

    initTargets ()

    Target.runOrDefaultWithArguments "Test"

    0
