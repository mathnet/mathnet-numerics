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
        [ { FrameworkVersion="net35"
            Dependencies=[ "TaskParallelLibrary", GetPackageVersion "./packages/" "TaskParallelLibrary" ] }
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
        FsLoader = true
        Dependencies =
          [ { FrameworkVersion=""
              Dependencies=[ "MathNet.Numerics", RequireExactly numericsRelease.PackageVersion
                             "FSharp.Core", GetPackageVersion "./packages/" "FSharp.Core" ] } ]
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
            Dependencies=[ "MathNet.Numerics", GetPackageVersion "./packages/data/" "MathNet.Numerics" ] } ]
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
            Dependencies=[ "MathNet.Numerics", GetPackageVersion "./packages/data/" "MathNet.Numerics" ] } ]
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
    CleanDirs [ "obj" ]
    CleanDirs [ "out/api"; "out/docs"; "out/packages" ]
    CleanDirs [ "out/lib/Net35"; "out/lib/Net40"; "out/lib/Profile7"; "out/lib/Profile47"; "out/lib/Profile78"; "out/lib/Profile259"; "out/lib/Profile328" ]
    CleanDirs [ "out/test/Net35"; "out/test/Net40"; "out/test/Profile7"; "out/test/Profile47"; "out/test/Profile78"; "out/test/Profile259"; "out/test/Profile328" ]
    CleanDirs [ "out/lib-debug/Net35"; "out/lib-debug/Net40"; "out/lib-debug/Profile7"; "out/lib-debug/Profile47"; "out/lib-debug/Profile78"; "out/lib-debug/Profile259"; "out/lib-debug/Profile328" ]
    CleanDirs [ "out/test-debug/Net35"; "out/test-debug/Net40"; "out/test-debug/Profile7"; "out/test-debug/Profile47"; "out/test-debug/Profile78"; "out/test-debug/Profile259"; "out/test-debug/Profile328" ]
    CleanDirs [ "out/lib-signed/Net40"; "out/test-signed/Net40" ] // Signed Build
    CleanDirs [ "out/MKL"; "out/ATLAS"; "out/CUDA"; "out/OpenBLAS" ] // Native Providers
    CleanDirs [ "out/Data" ]) // Data Extensions

Target "ApplyVersion" (fun _ ->
    patchVersionInAssemblyInfo "src/Numerics" numericsRelease
    patchVersionInAssemblyInfo "src/FSharp" numericsRelease
    patchVersionInAssemblyInfo "src/UnitTests" numericsRelease
    patchVersionInAssemblyInfo "src/FSharpUnitTests" numericsRelease
    patchVersionInAssemblyInfo "src/Data" dataRelease
    patchVersionInAssemblyInfo "src/DataUnitTests" dataRelease
    patchVersionInResource "src/NativeProviders/MKL/resource.rc" mklRelease
    patchVersionInResource "src/NativeProviders/CUDA/resource.rc" cudaRelease
    patchVersionInResource "src/NativeProviders/OpenBLAS/resource.rc" openBlasRelease)

Target "Prepare" DoNothing
"Start"
  =?> ("Clean", not (hasBuildParam "incremental"))
  ==> "ApplyVersion"
  ==> "Prepare"


// --------------------------------------------------------------------------------------
// BUILD
// --------------------------------------------------------------------------------------

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

Target "DataBuild" (fun _ -> build !! "MathNet.Numerics.Data.sln")
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

Target "Test" (fun _ -> test !! "out/test/**/*UnitTests*.dll")
"Build" ==> "Test"

Target "DataTest" (fun _ -> test !! "out/Data/test/**/*UnitTests*.dll")
"DataBuild" ==> "DataTest"

Target "MklWin32Test" (fun _ -> test32 !! "out/MKL/Windows/*UnitTests*.dll")
Target "MklWin64Test" (fun _ -> test !! "out/MKL/Windows/*UnitTests*.dll")
Target "MklWinTest" DoNothing
"MklWin32Build" ==> "MklWin32Test" ==> "MklWinTest"
"MklWin64Build" ==> "MklWin64Test" ==> "MklWinTest"

Target "CudaWin64Test" (fun _ -> test !! "out/CUDA/Windows/*UnitTests*.dll")
Target "CudaWinTest" DoNothing
"CudaWin64Build" ==> "CudaWin64Test" ==> "CudaWinTest"

Target "OpenBlasWin32Test" (fun _ -> test32 !! "out/OpenBLAS/Windows/*UnitTests*.dll")
Target "OpenBlasWin64Test" (fun _ -> test !! "out/OpenBLAS/Windows/*UnitTests*.dll")
Target "OpenBlasWinTest" DoNothing
"OpenBlasWin32Build" ==> "OpenBlasWin32Test" ==> "OpenBlasWinTest"
"OpenBlasWin64Build" ==> "OpenBlasWin64Test" ==> "OpenBlasWinTest"


// --------------------------------------------------------------------------------------
// PACKAGES
// --------------------------------------------------------------------------------------

Target "Pack" DoNothing
Target "DataPack" DoNothing
Target "MklWinPack" DoNothing
Target "MklLinuxPack" DoNothing
Target "CudaWinPack" DoNothing
Target "OpenBlasWinPack" DoNothing

// ZIP

Target "Zip" (fun _ ->
    CleanDir "out/packages/Zip"
    if not (hasBuildParam "signed") || hasBuildParam "release" then
        coreBundle |> zip "out/packages/Zip" "out/lib" (fun f -> f.Contains("MathNet.Numerics.") || f.Contains("System.Threading.") || f.Contains("FSharp.Core."))
    if hasBuildParam "signed" || hasBuildParam "release" then
        coreSignedBundle |> zip "out/packages/Zip" "out/lib-signed" (fun f -> f.Contains("MathNet.Numerics.")))
"Build" ==> "Zip" ==> "Pack"

Target "DataZip" (fun _ ->
    CleanDir "out/Data/packages/Zip"
    dataBundle |> zip "out/Data/packages/Zip" "out/Data/lib" (fun f -> f.Contains("MathNet.Numerics.Data.")))
"DataBuild" ==> "DataZip" ==> "DataPack"

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

Target "NuGet" (fun _ ->
    CleanDir "out/packages/NuGet"
    if hasBuildParam "signed" || hasBuildParam "release" then
        nugetPack coreSignedBundle "out/packages/NuGet"
    if hasBuildParam "all" || hasBuildParam "release" then
        nugetPack coreBundle "out/packages/NuGet")
"Build" ==> "NuGet" ==> "Pack"

Target "DataNuGet" (fun _ ->
    CleanDir "out/Data/packages/NuGet"
    nugetPackExtension dataBundle "out/Data/packages/NuGet")
"DataBuild" ==> "DataNuGet" ==> "DataPack"

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
    !! "out/lib/Net40/MathNet.Numerics.dll"
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
Dependencies "MklWinAll" [ "MklWinPack"; "MklWinTest" ]

Target "CudaWinAll" DoNothing
Dependencies "CudaWinAll" [ "CudaWinPack"; "CudaWinTest" ]

Target "OpenBlasWinAll" DoNothing
Dependencies "OpenBlasWinAll" [ "OpenBlasWinPack"; "OpenBlasWinTest" ]

Target "DataAll" DoNothing
Dependencies "DataAll" [ "DataPack"; "DataTest" ]

RunTargetOrDefault "Test"
