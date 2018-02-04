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


// CORE PACKAGES

let summary = "Math.NET Numerics, providing methods and algorithms for numerical computations in science, engineering and every day use."
let description = "Math.NET Numerics is the numerical foundation of the Math.NET project, aiming to provide methods and algorithms for numerical computations in science, engineering and every day use. "
let support = "Supports .Net 4.0, .Net 3.5 and Mono on Windows, Linux and Mac; Silverlight 5, WindowsPhone/SL 8, WindowsPhone 8.1 and Windows 8 with PCL portable profiles 7, 47, 78, 259 and 328; Android/iOS with Xamarin."
let supportFsharp = "Supports F# 3.0 on .Net 4.0, .Net 3.5 and Mono on Windows, Linux and Mac; Silverlight 5 and Windows 8 with PCL portable profile 47; Android/iOS with Xamarin."
let supportSigned = "Supports .Net 4.0. This package contains strong-named assemblies for legacy use cases."
let tags = "math numeric statistics probability integration interpolation regression solve fit linear algebra matrix fft"

let numericsPack =
    { Id = "MathNet.Numerics"
      Release = numericsRelease
      Title = "Math.NET Numerics"
      Summary = summary
      Description = description + support
      Tags = tags
      Authors = [ "Christoph Ruegg"; "Marcus Cuda"; "Jurgen Van Gael" ]
      FsLoader = false
      Dependencies =
        [ { FrameworkVersion="net40"
            Dependencies=[] } ]
      Files =
        [ @"..\..\out\lib\Net40\MathNet.Numerics.*", Some libnet40, Some @"**\MathNet.Numerics.FSharp.*";
          @"..\..\src\Numerics\**\*.cs", Some "src/Common", None ] }

let fsharpPack =
    { numericsPack with
        Id = "MathNet.Numerics.FSharp"
        Title = "Math.NET Numerics for F#"
        Summary = "F# Modules for " + summary
        Description = description + supportFsharp
        Tags = "fsharp F# " + tags
        FsLoader = true
        Dependencies =
          [ { FrameworkVersion=""
              Dependencies=[ "MathNet.Numerics", RequireExactly numericsRelease.PackageVersion
                             "FSharp.Core", GetPackageVersion "./packages/" "FSharp.Core" ] } ]
        Files =
          [ @"..\..\out\lib\Net40\MathNet.Numerics.FSharp.*", Some libnet40, None;
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
              Dependencies=[ "MathNet.Numerics.Signed", RequireExactly numericsRelease.PackageVersion
                             "FSharp.Core", GetPackageVersion "./packages/" "FSharp.Core" ] } ]
        Files =
          [ @"..\..\out\lib-signed\Net40\MathNet.Numerics.FSharp.*", Some libnet40, None;
            @"MathNet.Numerics.fsx", None, None;
            @"MathNet.Numerics.IfSharp.fsx", None, None;
            @"..\..\src\FSharp\**\*.fs", Some "src/Common", None ] }

let coreBundle =
    { Id = numericsPack.Id
      Release = numericsRelease
      Title = numericsPack.Title
      Packages = [ numericsPack; fsharpPack ] }

let coreSignedBundle =
    { Id = numericsSignedPack.Id
      Release = numericsRelease
      Title = numericsSignedPack.Title
      Packages = [ numericsSignedPack; fsharpSignedPack ] }


// MKL NATIVE PROVIDER PACKAGES

let mklWinPack =
    { Id = "MathNet.Numerics.MKL.Win"
      Release = mklRelease
      Title = "Math.NET Numerics - MKL Native Provider for Windows (x64 and x86)"
      Summary = ""
      Description = "Intel MKL native libraries for Math.NET Numerics on Windows."
      Tags = "math numeric statistics probability integration interpolation linear algebra matrix fft native mkl"
      Authors = [ "Christoph Ruegg"; "Marcus Cuda"; "Jurgen Van Gael" ]
      FsLoader = false
      Dependencies = []
      Files =
        [ @"..\..\build\NativeProvider.targets", Some "build\MathNet.Numerics.MKL.Win.targets", None;
          @"..\..\out\MKL\Windows\x64\libiomp5md.dll", Some @"build\x64", None;
          @"..\..\out\MKL\Windows\x64\MathNet.Numerics.MKL.dll", Some @"build\x64", None;
          @"..\..\out\MKL\Windows\x86\libiomp5md.dll", Some @"build\x86", None;
          @"..\..\out\MKL\Windows\x86\MathNet.Numerics.MKL.dll", Some @"build\x86", None ] }

let mklWin32Pack =
    { mklWinPack with
        Id = "MathNet.Numerics.MKL.Win-x86"
        Title = "Math.NET Numerics - MKL Native Provider for Windows (x86)"
        Files =
          [ @"..\..\build\NativeProvider.targets", Some "build\MathNet.Numerics.MKL.Win-x86.targets", None;
            @"..\..\out\MKL\Windows\x86\libiomp5md.dll", Some @"build\x86", None;
            @"..\..\out\MKL\Windows\x86\MathNet.Numerics.MKL.dll", Some @"build\x86", None ] }

let mklWin64Pack =
    { mklWinPack with
        Id = "MathNet.Numerics.MKL.Win-x64"
        Title = "Math.NET Numerics - MKL Native Provider for Windows (x64)"
        Files =
          [ @"..\..\build\NativeProvider.targets", Some "build\MathNet.Numerics.MKL.Win-x64.targets", None;
            @"..\..\out\MKL\Windows\x64\libiomp5md.dll", Some @"build\x64", None;
            @"..\..\out\MKL\Windows\x64\MathNet.Numerics.MKL.dll", Some @"build\x64", None ] }

let mklLinuxPack =
    { Id = "MathNet.Numerics.MKL.Linux"
      Release = mklRelease
      Title = "Math.NET Numerics - MKL Native Provider for Linux (x64 and x86)"
      Summary = ""
      Description = "Intel MKL native libraries for Math.NET Numerics on Linux."
      Tags = "math numeric statistics probability integration interpolation linear algebra matrix fft native mkl"
      Authors = [ "Christoph Ruegg"; "Marcus Cuda"; "Jurgen Van Gael" ]
      FsLoader = false
      Dependencies = []
      Files =
        [ @"..\..\build\NativeProvider.targets", Some "build\MathNet.Numerics.MKL.Linux.targets", None;
          @"..\..\out\MKL\Linux\x64\libiomp5.so", Some @"build\x64", None;
          @"..\..\out\MKL\Linux\x64\MathNet.Numerics.MKL.dll", Some @"build\x64", None;
          @"..\..\out\MKL\Linux\x86\libiomp5.so", Some @"build\x86", None;
          @"..\..\out\MKL\Linux\x86\MathNet.Numerics.MKL.dll", Some @"build\x86", None ] }

let mklLinux32Pack =
    { mklLinuxPack with
        Id = "MathNet.Numerics.MKL.Linux-x86"
        Title = "Math.NET Numerics - MKL Native Provider for Linux (x86)"
        Files =
          [ @"..\..\build\NativeProvider.targets", Some "build\MathNet.Numerics.MKL.Linux-x86.targets", None;
            @"..\..\out\MKL\Linux\x86\libiomp5.so", Some @"build\x86", None;
            @"..\..\out\MKL\Linux\x86\MathNet.Numerics.MKL.dll", Some @"build\x86", None ] }

let mklLinux64Pack =
    { mklLinuxPack with
        Id = "MathNet.Numerics.MKL.Linux-x64"
        Title = "Math.NET Numerics - MKL Native Provider for Linux (x64)"
        Files =
          [ @"..\..\build\NativeProvider.targets", Some "build\MathNet.Numerics.MKL.Linux-x64.targets", None;
            @"..\..\out\MKL\Linux\x64\libiomp5.so", Some @"build\x64", None;
            @"..\..\out\MKL\Linux\x64\MathNet.Numerics.MKL.dll", Some @"build\x64", None ] }

let mklWinBundle =
    { Id = "MathNet.Numerics.MKL.Win"
      Release = mklRelease
      Title = "Math.NET Numerics MKL Native Provider for Windows"
      Packages = [ mklWinPack; mklWin32Pack; mklWin64Pack ] }

let mklLinuxBundle =
    { Id = "MathNet.Numerics.MKL.Linux"
      Release = mklRelease
      Title = "Math.NET Numerics MKL Native Provider for Linux"
      Packages = [ mklLinuxPack; mklLinux32Pack; mklLinux64Pack ] }


// CUDA NATIVE PROVIDER PACKAGES

let cudaWinPack =
    { Id = "MathNet.Numerics.CUDA.Win"
      Release = cudaRelease
      Title = "Math.NET Numerics - CUDA Native Provider for Windows (x64)"
      Summary = ""
      Description = "Nvidia CUDA native libraries for Math.NET Numerics."
      Tags = "math numeric statistics probability integration interpolation linear algebra matrix fft native cuda gpu"
      Authors = [ "Matthew A Johnson"; "Christoph Ruegg" ]
      FsLoader = false
      Dependencies = []
      Files =
        [ @"..\..\build\NativeProvider.targets", Some "build\MathNet.Numerics.CUDA.Win.targets", None;
          @"..\..\out\CUDA\Windows\x64\cublas64_70.dll", Some "content", None;
          @"..\..\out\CUDA\Windows\x64\cudart64_70.dll", Some "content", None;
          @"..\..\out\CUDA\Windows\x64\cusolver64_70.dll", Some "content", None;
          @"..\..\out\CUDA\Windows\x64\MathNet.Numerics.CUDA.dll", Some "content", None ] }

let cudaWinBundle =
    { Id = "MathNet.Numerics.CUDA.Win"
      Release = cudaRelease
      Title = "Math.NET Numerics CUDA Native Provider for Windows"
      Packages = [ cudaWinPack ] }


// OpenBLAS NATIVE PROVIDER PACKAGES

let openBlasWinPack =
    { Id = "MathNet.Numerics.OpenBLAS.Win"
      Release = openBlasRelease
      Title = "Math.NET Numerics - OpenBLAS Native Provider for Windows (x64 and x86)"
      Summary = ""
      Description = "OpenBLAS native libraries for Math.NET Numerics."
      Tags = "math numeric statistics probability integration interpolation linear algebra matrix fft native openblas"
      Authors = [ "Kuan Bartel"; "Christoph Ruegg"; "Marcus Cuda" ]
      FsLoader = false
      Dependencies = []
      Files =
        [ @"..\..\build\NativeProvider.targets", Some "build\MathNet.Numerics.OpenBLAS.Win.targets", None;
          @"..\..\out\OpenBLAS\Windows\x64\libgcc_s_seh-1.dll", Some @"build\x64", None;
          @"..\..\out\OpenBLAS\Windows\x64\libgfortran-3.dll", Some @"build\x64", None;
          @"..\..\out\OpenBLAS\Windows\x64\libopenblas.dll", Some @"build\x64", None;
          @"..\..\out\OpenBLAS\Windows\x64\libquadmath-0.dll", Some @"build\x64", None;
          @"..\..\out\OpenBLAS\Windows\x64\MathNet.Numerics.OpenBLAS.dll", Some @"build\x64", None;
          @"..\..\out\OpenBLAS\Windows\x86\libgcc_s_sjlj-1.dll", Some @"build\x86", None;
          @"..\..\out\OpenBLAS\Windows\x86\libgfortran-3.dll", Some @"build\x86", None;
          @"..\..\out\OpenBLAS\Windows\x86\libopenblas.dll", Some @"build\x86", None;
          @"..\..\out\OpenBLAS\Windows\x86\libquadmath-0.dll", Some @"build\x86", None;
          @"..\..\out\OpenBLAS\Windows\x86\MathNet.Numerics.OpenBLAS.dll", Some @"build\x86", None ] }

let openBlasWinBundle =
    { Id = "MathNet.Numerics.OpenBLAS.Win"
      Release = openBlasRelease
      Title = "Math.NET Numerics OpenBLAS Native Provider for Windows"
      Packages = [ openBlasWinPack ] }


// DATA EXTENSION PACKAGES

let dataTextPack =
    { Id = "MathNet.Numerics.Data.Text"
      Release = dataRelease
      Title = "Math.NET Numerics - Text Data I/O Extensions"
      Summary = ""
      Description = "Text Data Input/Output Extensions for Math.NET Numerics, the numerical foundation of the Math.NET project, aiming to provide methods and algorithms for numerical computations in science, engineering and every day use."
      Tags = "math numeric data text csv tsv json xml"
      Authors = [ "Christoph Ruegg"; "Marcus Cuda" ]
      FsLoader = false
      Dependencies =
        [ { FrameworkVersion=""
            Dependencies=[ "MathNet.Numerics", "4.0.0" ] } ]
      Files =
        [ @"..\..\out\Data\lib\Net40\MathNet.Numerics.Data.Text.dll", Some libnet40, None;
          @"..\..\out\Data\lib\Net40\MathNet.Numerics.Data.Text.xml", Some libnet40, None ] }

let dataMatlabPack =
    { Id = "MathNet.Numerics.Data.Matlab"
      Release = dataRelease
      Title = "Math.NET Numerics - MATLAB Data I/O Extensions"
      Summary = ""
      Description = "MathWorks MATLAB Data Input/Output Extensions for Math.NET Numerics, the numerical foundation of the Math.NET project, aiming to provide methods and algorithms for numerical computations in science, engineering and every day use."
      Tags = "math numeric data matlab"
      Authors = [ "Christoph Ruegg"; "Marcus Cuda" ]
      FsLoader = false
      Dependencies =
        [ { FrameworkVersion=""
            Dependencies=[ "MathNet.Numerics", "4.0.0" ] } ]
      Files =
        [ @"..\..\out\Data\lib\Net40\MathNet.Numerics.Data.Matlab.dll", Some libnet40, None;
          @"..\..\out\Data\lib\Net40\MathNet.Numerics.Data.Matlab.xml", Some libnet40, None ] }

let dataBundle =
    { Id = "MathNet.Numerics.Data"
      Release = dataRelease
      Title = "Math.NET Numerics Data Extensions"
      Packages = [ dataTextPack; dataMatlabPack ] }


// --------------------------------------------------------------------------------------
// PREPARE
// --------------------------------------------------------------------------------------

Target "Start" DoNothing

Target "Clean" (fun _ ->
    // Force delete the obj folder first (dotnet SDK has a habbit of fucking this folder up to a state where not even clean works...)
    CleanDirs [ "src/Numerics/bin"; "src/FSharp/bin"; "src/TestData/bin"; "src/Numerics.Tests/bin"; "src/FSharp.Tests/bin"; "src/Data/Text/bin"; "src/Data/Matlab/bin"; "src/Data.Tests/bin" ]
    CleanDirs [ "src/Numerics/obj"; "src/FSharp/obj"; "src/TestData/obj"; "src/Numerics.Tests/obj"; "src/FSharp.Tests/obj"; "src/Data/Text/obj"; "src/Data/Matlab/obj"; "src/Data.Tests/obj" ]
    CleanDirs [ "obj" ]
    CleanDirs [ "out/api"; "out/docs"; "out/packages" ]
    CleanDirs [ "out/lib" ]
    //CleanDirs [ "out/lib-signed/Net40"; "out/test-signed/Net40" ] // Signed Build
    CleanDirs [ "out/MKL"; "out/ATLAS"; "out/CUDA"; "out/OpenBLAS" ] // Native Providers
    CleanDirs [ "out/Data" ] // Data Extensions
    DotNetCli.RunCommand id "clean MathNet.Numerics.sln"
    DotNetCli.RunCommand id "clean MathNet.Numerics.Data.sln")

Target "ApplyVersion" (fun _ ->
    patchVersionInAssemblyInfo "src/FSharp" numericsRelease
    patchVersionInAssemblyInfo "src/TestData" numericsRelease
    patchVersionInAssemblyInfo "src/Numerics.Tests" numericsRelease
    patchVersionInAssemblyInfo "src/FSharp.Tests" numericsRelease
    patchVersionInAssemblyInfo "src/Data.Tests" dataRelease
    patchVersionInProjectFile "src/Numerics/Numerics.csproj" numericsRelease
    patchVersionInProjectFile "src/FSharp/FSharp.fsproj" numericsRelease
    patchVersionInProjectFile "src/Data/Text/Text.csproj" dataRelease
    patchVersionInProjectFile "src/Data/Matlab/Matlab.csproj" dataRelease
    patchVersionInResource "src/NativeProviders/MKL/resource.rc" mklRelease
    patchVersionInResource "src/NativeProviders/CUDA/resource.rc" cudaRelease
    patchVersionInResource "src/NativeProviders/OpenBLAS/resource.rc" openBlasRelease)

Target "Restore" (fun _ ->
    restore "MathNet.Numerics.sln"
    restore "MathNet.Numerics.Data.sln")

Target "Prepare" DoNothing
"Start"
  =?> ("Clean", not (hasBuildParam "incremental"))
  ==> "Restore"
  ==> "ApplyVersion"
  ==> "Prepare"


// --------------------------------------------------------------------------------------
// BUILD
// --------------------------------------------------------------------------------------

Target "Build" (fun _ -> build "MathNet.Numerics.sln")
"Prepare" ==> "Build"

Target "DataBuild" (fun _ -> build "MathNet.Numerics.Data.sln")
"Prepare" ==> "DataBuild"

Target "MklWin32Build" (fun _ -> buildConfig32 "Release-MKL" !! "MathNet.Numerics.NativeProviders.sln")
Target "MklWin64Build" (fun _ -> buildConfig64 "Release-MKL" !! "MathNet.Numerics.NativeProviders.sln")
Target "MklWinBuild" DoNothing
"Prepare" ==> "MklWin32Build" ==> "MklWinBuild"
"Prepare" ==> "MklWin64Build" ==> "MklWinBuild"

Target "CudaWin64Build" (fun _ -> buildConfig64 "Release-CUDA" !! "MathNet.Numerics.NativeProviders.sln")
Target "CudaWinBuild" DoNothing
"Prepare" ==> "CudaWin64Build" ==> "CudaWinBuild"

Target "OpenBlasWin32Build" (fun _ -> buildConfig32 "Release-OpenBLAS" !! "MathNet.Numerics.NativeProviders.sln")
Target "OpenBlasWin64Build" (fun _ -> buildConfig64 "Release-OpenBLAS" !! "MathNet.Numerics.NativeProviders.sln")
Target "OpenBlasWinBuild" DoNothing
"Prepare" ==> "OpenBlasWin32Build" ==> "OpenBlasWinBuild"
"Prepare" ==> "OpenBlasWin64Build" ==> "OpenBlasWinBuild"


// --------------------------------------------------------------------------------------
// TEST
// --------------------------------------------------------------------------------------

let testLibrary testsDir testsProj framework =
    DotNetCli.RunCommand
        (fun c -> { c with WorkingDir = testsDir})
        (sprintf "run -p %s --configuration Release --framework %s --no-restore --no-build"
            testsProj
            framework)

let testNumerics framework = testLibrary "src/Numerics.Tests" "Numerics.Tests.csproj" framework
Target "TestNumerics" DoNothing
Target "TestNumericsCore1.1" (fun _ -> testNumerics "netcoreapp1.1")
Target "TestNumericsCore2.0" (fun _ -> testNumerics "netcoreapp2.0")
Target "TestNumericsNET40" (fun _ -> testNumerics "net40")
Target "TestNumericsNET45" (fun _ -> testNumerics "net45")
Target "TestNumericsNET46" (fun _ -> testNumerics "net46")
Target "TestNumericsNET47"  (fun _ -> testNumerics "net47")
"Build" ==> "TestNumericsCore1.1" ==> "TestNumerics"
"Build" ==> "TestNumericsCore2.0"
"Build" =?> ("TestNumericsNET40", isWindows)
"Build" =?> ("TestNumericsNET45", isWindows) ==> "TestNumerics"
"Build" =?> ("TestNumericsNET46", isWindows)
"Build" =?> ("TestNumericsNET47", isWindows)
let testFsharp framework = testLibrary "src/FSharp.Tests" "FSharp.Tests.fsproj" framework
Target "TestFsharp" DoNothing
Target "TestFsharpCore1.1" (fun _ -> testFsharp "netcoreapp1.1")
Target "TestFsharpCore2.0" (fun _ -> testFsharp "netcoreapp2.0")
Target "TestFsharpNET45" (fun _ -> testFsharp "net45")
Target "TestFsharpNET46" (fun _ -> testFsharp "net46")
Target "TestFsharpNET47" (fun _ -> testFsharp "net47")
"Build" ==> "TestFsharpCore1.1" ==> "TestFsharp"
"Build" ==> "TestFsharpCore2.0"
"Build" =?> ("TestFsharpNET45", isWindows) ==> "TestFsharp"
"Build" =?> ("TestFsharpNET46", isWindows)
"Build" =?> ("TestFsharpNET47", isWindows)
Target "Test" DoNothing
"TestNumerics" ==> "Test"
"TestFsharp" ==> "Test"

let testMKL framework = testLibrary "src/Numerics.Tests" "Numerics.Tests.MKL.csproj" framework
Target "MklTest" DoNothing
Target "MklTestCore2.0" (fun _ -> testMKL "netcoreapp2.0")
Target "MklTestNET40" (fun _ -> testMKL "net40")
"Build" ==> "MklTestCore2.0" ==> "MklTest"
"Build" =?> ("MklTestNET40", isWindows) ==> "MklTest"

let testOpenBLAS framework = testLibrary "src/Numerics.Tests" "Numerics.Tests.OpenBLAS.csproj" framework
Target "OpenBlasTest" DoNothing
Target "OpenBlasTestCore2.0" (fun _ -> testOpenBLAS "netcoreapp2.0")
Target "OpenBlasTestNET40" (fun _ -> testOpenBLAS "net40")
"Build" ==> "OpenBlasTestCore2.0" ==> "OpenBlasTest"
"Build" =?> ("OpenBlasTestNET40", isWindows) ==> "OpenBlasTest"

let testCUDA framework = testLibrary "src/Numerics.Tests" "Numerics.Tests.CUDA.csproj" framework
Target "CudaTest" DoNothing
Target "CudaTestCore2.0" (fun _ -> testCUDA "netcoreapp2.0")
Target "CudaTestNET40" (fun _ -> testCUDA "net40")
"Build" ==> "CudaTestCore2.0" ==> "CudaTest"
"Build" =?> ("CudaTestNET40", isWindows) ==> "CudaTest"

let testData framework = testLibrary "src/Data.Tests" "Data.Tests.csproj" framework
Target "DataTest" DoNothing
Target "DataTestCore1.1" (fun _ -> testData "netcoreapp1.1")
Target "DataTestCore2.0" (fun _ -> testData "netcoreapp2.0")
Target "DataTestNET45" (fun _ -> testData "net45")
"DataBuild" ==> "DataTestCore1.1" ==> "DataTest"
"DataBuild" ==> "DataTestCore2.0"
"DataBuild" =?> ("DataTestNET45", isWindows) ==> "DataTest"


// --------------------------------------------------------------------------------------
// CODE SIGN
// --------------------------------------------------------------------------------------

Target "Sign" (fun _ ->
    let fingerprint = "5dbea70701b40cab1b2ca62c75401342b4f0f03a"
    let timeserver = "http://time.certum.pl/"
    sign fingerprint timeserver (!! "src/Numerics/bin/Release/**/MathNet.Numerics.dll" ++ "src/FSharp/bin/Release/**/MathNet.Numerics.FSharp.dll" ))
    
Target "DataSign" (fun _ ->
    let fingerprint = "5dbea70701b40cab1b2ca62c75401342b4f0f03a"
    let timeserver = "http://time.certum.pl/"
    sign fingerprint timeserver (!! "src/Data/Text/bin/Release/**/MathNet.Numerics.Data.Text.dll" ++ "src/Data/Matlab/bin/Release/**/MathNet.Numerics.Data.Matlab.dll" ))

// --------------------------------------------------------------------------------------
// PACKAGES
// --------------------------------------------------------------------------------------

Target "Pack" DoNothing
Target "DataPack" DoNothing
Target "MklWinPack" DoNothing
Target "MklLinuxPack" DoNothing
Target "CudaWinPack" DoNothing
Target "OpenBlasWinPack" DoNothing

// COLLECT

Target "Collect" (fun _ ->
    // It is important that the libs have been signed before we collect them (that's why we cannot copy them right after the build)
    CopyDir "out/lib" "src/Numerics/bin/Release" (fun n -> n.Contains("MathNet.Numerics.dll") || n.Contains("MathNet.Numerics.pdb") || n.Contains("MathNet.Numerics.xml"))
    CopyDir "out/lib" "src/FSharp/bin/Release" (fun n -> n.Contains("MathNet.Numerics.FSharp.dll") || n.Contains("MathNet.Numerics.FSharp.pdb") || n.Contains("MathNet.Numerics.FSharp.xml")))
"Build" =?> ("Sign", hasBuildParam "sign") ==> "Collect"

Target "DataCollect" (fun _ ->
    // It is important that the libs have been signed before we collect them (that's why we cannot copy them right after the build)
    CopyDir "out/Data/lib" "src/Data/Text/bin/Release" (fun n -> n.Contains("MathNet.Numerics.Data.Text.dll") || n.Contains("MathNet.Numerics.Data.Text.pdb") || n.Contains("MathNet.Numerics.Data.Text.xml"))
    CopyDir "out/Data/lib" "src/Data/Matlab/bin/Release" (fun n -> n.Contains("MathNet.Numerics.Data.Matlab.dll") || n.Contains("MathNet.Numerics.Data.Matlab.pdb") || n.Contains("MathNet.Numerics.Data.Matlab.xml")))
"DataBuild" =?> ("DataSign", hasBuildParam "sign") ==> "DataCollect"

// ZIP

Target "Zip" (fun _ ->
    CleanDir "out/packages/Zip"
    coreBundle |> zip "out/packages/Zip" "out/lib" (fun f -> f.Contains("MathNet.Numerics.") || f.Contains("System.Threading.") || f.Contains("FSharp.Core.")))
"Collect" ==> "Zip" ==> "Pack"

Target "DataZip" (fun _ ->
    CleanDir "out/Data/packages/Zip"
    dataBundle |> zip "out/Data/packages/Zip" "out/Data/lib" (fun f -> f.Contains("MathNet.Numerics.Data.")))
"DataCollect" ==> "DataZip" ==> "DataPack"

Target "MklWinZip" (fun _ ->
    CreateDir "out/MKL/packages/Zip"
    mklWinBundle |> zip "out/MKL/packages/Zip" "out/MKL/Windows" (fun f -> f.Contains("MathNet.Numerics.MKL.") || f.Contains("libiomp5md.dll")))
"MklWinBuild" ==> "MklWinZip" ==> "MklWinPack"

Target "MklLinuxZip" (fun _ ->
    CreateDir "out/MKL/packages/Zip"
    mklLinuxBundle |> zip "out/MKL/packages/Zip" "out/MKL/Linux" (fun f -> f.Contains("MathNet.Numerics.MKL.") || f.Contains("libiomp5.so")))
// "MklLinuxBuild" ==> "MklLinuxZip" ==> "MklLinuxPack"
"MklLinuxZip" ==> "MklLinuxPack"

Target "CudaWinZip" (fun _ ->
    CreateDir "out/CUDA/packages/Zip"
    cudaWinBundle |> zip "out/CUDA/packages/Zip" "out/CUDA/Windows" (fun f -> f.Contains("MathNet.Numerics.CUDA.") || f.Contains("cublas") || f.Contains("cudart") || f.Contains("cusolver")))
"CudaWinBuild" ==> "CudaWinZip" ==> "CudaWinPack"

Target "OpenBlasWinZip" (fun _ ->
    CreateDir "out/OpenBLAS/packages/Zip"
    openBlasWinBundle |> zip "out/OpenBLAS/packages/Zip" "out/OpenBLAS/Windows" (fun f -> f.Contains("MathNet.Numerics.OpenBLAS.") || f.Contains("libgcc") || f.Contains("libgfortran") || f.Contains("libopenblas") || f.Contains("libquadmath")))
"OpenBlasWinBuild" ==> "OpenBlasWinZip" ==> "OpenBlasWinPack"

// NUGET

let dotnetPack solution = DotNetCli.Pack (fun p ->
    { p with
        Project = solution
        Configuration = "Release"
        AdditionalArgs = ["--no-restore"; "--no-build" ]})

Target "NuGet" (fun _ ->
    pack "MathNet.Numerics.sln"
    CopyDir "out/packages/NuGet" "src/Numerics/bin/Release/" (fun n -> n.EndsWith(".nupkg"))
    CopyDir "out/packages/NuGet" "src/FSharp/bin/Release/" (fun n -> n.EndsWith(".nupkg")))
"Collect" ==> "NuGet" ==> "Pack"

Target "DataNuGet" (fun _ ->
    pack "src/Data/Text/Text.csproj"
    pack "src/Data/Matlab/Matlab.csproj"
    CopyDir "out/Data/packages/NuGet" "src/Data/Text/bin/Release/" (fun n -> n.EndsWith(".nupkg"))
    CopyDir "out/Data/packages/NuGet" "src/Data/Matlab/bin/Release/" (fun n -> n.EndsWith(".nupkg")))
"DataCollect" ==> "DataNuGet" ==> "DataPack"

Target "MklWinNuGet" (fun _ ->
    CreateDir "out/MKL/packages/NuGet"
    nugetPackExtension mklWinBundle "out/MKL/packages/NuGet")
"MklWinBuild" ==> "MklWinNuGet" ==> "MklWinPack"

Target "MklLinuxNuGet" (fun _ ->
    CreateDir "out/MKL/packages/NuGet"
    nugetPackExtension mklLinuxBundle "out/MKL/packages/NuGet")
// "MklLinuxBuild" ==> "MklLinuxNuGet" ==> "MklLinuxPack"
"MklLinuxNuGet" ==> "MklLinuxPack"

Target "CudaWinNuGet" (fun _ ->
    CreateDir "out/CUDA/packages/NuGet"
    nugetPackExtension cudaWinBundle "out/CUDA/packages/NuGet")
"CudaWinBuild" ==> "CudaWinNuGet" ==> "CudaWinPack"

Target "OpenBlasWinNuGet" (fun _ ->
    CreateDir "out/OpenBLAS/packages/NuGet"
    nugetPackExtension openBlasWinBundle "out/OpenBLAS/packages/NuGet")
"OpenBlasWinBuild" ==> "OpenBlasWinNuGet" ==> "OpenBlasWinPack"


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
Target "MklPublishTag" (fun _ -> publishReleaseTag "Math.NET Numerics MKL Provider" "mkl-" mklRelease)
Target "CudaPublishTag" (fun _ -> publishReleaseTag "Math.NET Numerics CUDA Provider" "cuda-" cudaRelease)
Target "OpenBlasPublishTag" (fun _ -> publishReleaseTag "Math.NET Numerics OpenBLAS Provider" "openblas-" openBlasRelease)
Target "DataPublishTag" (fun _ -> publishReleaseTag "Math.NET Numerics Data Extensions" "data-" dataRelease)

Target "PublishMirrors" (fun _ -> publishMirrors ())
Target "PublishDocs" (fun _ -> publishDocs numericsRelease)
Target "PublishApi" (fun _ -> publishApi numericsRelease)

Target "PublishArchive" (fun _ -> publishArchive "out/packages/Zip" "out/packages/NuGet" [coreBundle; coreSignedBundle])
Target "MklPublishArchive" (fun _ -> publishArchive "out/MKL/packages/Zip" "out/MKL/packages/NuGet" [mklWinBundle; mklLinuxBundle])
Target "CudaPublishArchive" (fun _ -> publishArchive "out/CUDA/packages/Zip" "out/CUDA/packages/NuGet" [cudaWinBundle])
Target "OpenBlasPublishArchive" (fun _ -> publishArchive "out/OpenBLAS/packages/Zip" "out/OpenBLAS/packages/NuGet" [openBlasWinBundle])
Target "DataPublishArchive" (fun _ -> publishArchive "out/Data/packages/Zip" "out/Data/packages/NuGet" [dataBundle])

Target "PublishNuGet" (fun _ -> !! "out/packages/NuGet/*.nupkg" -- "out/packages/NuGet/*.symbols.nupkg" |> publishNuGet)
Target "MklPublishNuGet" (fun _ -> !! "out/MKL/packages/NuGet/*.nupkg" |> publishNuGet)
Target "CudaPublishNuGet" (fun _ -> !! "out/CUDA/packages/NuGet/*.nupkg" |> publishNuGet)
Target "OpenBlasPublishNuGet" (fun _ -> !! "out/OpenBLAS/packages/NuGet/*.nupkg" |> publishNuGet)
Target "DataPublishNuGet" (fun _ -> !! "out/Data/packages/NuGet/*.nupkg" |> publishNuGet)

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
Dependencies "All" [ "Pack"; "Docs"; "Api"; "Test" ]

Target "MklWinAll" DoNothing
Dependencies "MklWinAll" [ "MklWinPack"; "MklTest" ]

Target "CudaWinAll" DoNothing
Dependencies "CudaWinAll" [ "CudaWinPack"; "CudaTest" ]

Target "OpenBlasWinAll" DoNothing
Dependencies "OpenBlasWinAll" [ "OpenBlasWinPack"; "OpenBlasTest" ]

Target "DataAll" DoNothing
Dependencies "DataAll" [ "DataPack"; "DataTest" ]

RunTargetOrDefault "Test"
