Math.NET Numerics
=================

Math.NET Numerics is an opensource **numerical library for .Net, Silverlight and Mono**.

Math.NET Numerics is the numerical foundation of the Math.NET initiative, aiming to provide methods and algorithms for numerical computations in science, engineering and every day use. Covered topics include special functions, linear algebra, probability models, random numbers, statistics, interpolation, integration, regression, curve fitting, integral transforms (FFT) and more.

In addition to the core .NET package (which is written entirely in C#), Numerics specifically supports F# with idiomatic extension modules and maintains mathematical data structures like BigRational that originated in the F# PowerPack. If a performance boost is needed, the managed-code provider backing its linear algebra routines and decompositions can be exchanged with wrappers for optimized native implementations such as Intel MKL.

Math.NET Numerics is covered under the terms of the [MIT/X11](LICENSE.md) license. You may therefore link to it and use it in both opensource and proprietary software projects. We accept contributions!

* [**Project Website**](http://numerics.mathdotnet.com)
* [Source Code](http://github.com/mathnet/mathnet-numerics)
* [NuGet Packages](https://www.nuget.org/profiles/mathnet/) | [Downloads](http://mathnetnumerics.codeplex.com/releases) | [Release Notes](http://numerics.mathdotnet.com/docs/ReleaseNotes.html)
* [Documentation](http://numerics.mathdotnet.com/docs/) | [API Reference](http://numerics.mathdotnet.com/api/)
* [Issues & Bugs](http://github.com/mathnet/mathnet-numerics/issues) | [Ideas](http://feedback.mathdotnet.com/forums/2060-math-net-numerics)
* [Discussions](http://mathnetnumerics.codeplex.com/discussions) | [Stack Overflow](http://stackoverflow.com/questions/tagged/mathdotnet) | [Chat](https://gitter.im/mathnet/mathnet-numerics) | [Twitter](http://twitter.com/MathDotNet) | [Google+](https://plus.google.com/112484567926928665204)
* [Wikipedia](http://en.wikipedia.org/wiki/Math.NET_Numerics) | [OpenHUB](https://www.ohloh.net/p/mathnet)

### Current Version

![Math.NET Numerics Version](http://img.shields.io/nuget/v/MathNet.Numerics.svg?style=flat) Math.NET Numerics  
![Native Providers Version](http://img.shields.io/nuget/v/MathNet.Numerics.MKL.Win-x64.svg?style=flat) Native Providers  
![Data Extensions Version](http://img.shields.io/nuget/v/MathNet.Numerics.Data.Text.svg?style=flat) Data Extensions

Installation Instructions
-------------------------

The recommended way to get Math.NET Numerics is to use NuGet. The following packages are provided and maintained in the public [NuGet Gallery](https://nuget.org/profiles/mathnet/). Alternatively you can also download the binaries in Zip packages, available on [CodePlex](http://mathnetnumerics.codeplex.com/releases).

Core Package:

- **MathNet.Numerics** - core package, including .Net 4, .Net 3.5 and portable/PCL builds.
- **MathNet.Numerics.FSharp** - optional extensions for a better F# experience. BigRational.
- **MathNet.Numerics.Signed** - strong-named version of the core package *(not recommended)*.
- **MathNet.Numerics.FSharp.Signed** - strong-named version of the F# package *(not recommended)*.

Alternative Provider Packages (optional):

- **MathNet.Numerics.MKL.Win-x86** - Native Intel MKL Linear Algebra provider (Windows/32-bit).
- **MathNet.Numerics.MKL.Win-x64** - Native Intel MKL Linear Algebra provider (Windows/64-bit).

Data/IO Packages for reading and writing data (optional):

- **MathNet.Numerics.Data.Text** - Text-based matrix formats like CSV and MatrixMarket.
- **MathNet.Numerics.Data.Matlab** - MATLAB Level-5 matrix file format.

### Platform Support and Dependencies

- .Net 4.0, .Net 3.5 and Mono: Windows, Linux and Mac.
- PCL Portable Profiles 47, 259 and 328: Windows 8, Silverlight 5, Windows Phone/SL 8, Windows Phone 8.1.
- Xamarin: Android, iOS

Package Dependencies:

- .Net 3.5: [Task Parallel Library for .NET 3.5](http://www.nuget.org/packages/TaskParallelLibrary)
- .Net 4.0 and higher, Mono, PCL Profiles: None
- F# .Net 3.5: additionally [FSharp.Core.4.3.0.0.Microsoft.Signed](http://www.nuget.org/packages/FSharp.Core.4.3.0.0.Microsoft.Signed
- F# .Net 4.0 an higher, Mono, PCL Profiles: additionally [FSharp.Core.Microsoft.Signed](http://www.nuget.org/packages/FSharp.Core.Microsoft.Signed)

Framework Dependencies (part of the .NET Framework):

- .Net 4.0 and higher, Mono, PCL Profile 47: System.Numerics
- .Net 3.5, PCL Profile 328: None


Building Math.NET Numerics
--------------------------

Windows: [![AppVeyor build status](https://ci.appveyor.com/api/projects/status/79j22c061saisces/branch/master)](https://ci.appveyor.com/project/cdrnet/mathnet-numerics)  
Mono: [![Travis Build Status](https://travis-ci.org/mathnet/mathnet-numerics.svg?branch=master)](https://travis-ci.org/mathnet/mathnet-numerics)

If you do not want to use the official binaries, or if you like to modify, debug or contribute, you can compile Math.NET Numerics locally either using Visual Studio or manually with the build scripts.

* The Visual Studio solutions should build out of the box, without any preparation steps or package restores.
* Instead of a compatible IDE you can also build the solutions with `msbuild`, or on Mono with `xbuild`.
* The full build including unit tests, docs, NuGet and Zip packages is using [FAKE](http://fsharp.github.io/FAKE/).

### How to build with MSBuild/XBuild

    msbuild MathNet.Numerics.sln            # only build for .Net 4 (main solution)
    msbuild MathNet.Numerics.Net35Only.sln  # only build for .Net 3.5
    msbuild MathNet.Numerics.All.sln        # full build with .Net 4, 3.5 and PCL profiles
    xbuild MathNet.Numerics.sln             # build with Mono, e.g. on Linux or Mac

### How to build with FAKE

    build.cmd    # normal build (.Net 4.0), run unit tests (.Net on Windows)
    ./build.sh   # normal build (.Net 4.0), run unit tests (Mono on Linux/Mac, .Net on Windows)
    
    build.cmd Build              # normal build (.Net 4.0)
    build.cmd Build incremental  # normal build, incremental (.Net 4.0)
    build.cmd Build all          # full build (.Net 4.0, 3.5, PCL)
    build.cmd Build net35        # compatibility build (.Net 3.5)
    build.cmd Build signed       # normal build, signed/strong named (.Net 4.0)
    
    build.cmd Test        # normal build (.Net 4.0), run unit tests
    build.cmd Test quick  # normal build (.Net 4.0), run unit tests except long running ones
    build.cmd Test all    # full build (.Net 4.0, 3.5, PCL), run all unit tests
    build.cmd Test net35  # compatibility build (.Net 3.5), run unit testss
    
    build.cmd Clean  # cleanup build artifacts
    build.cmd Docs   # generate documentation
    build.cmd Api    # generate api reference
    
    build.cmd NuGet all     # generate normal NuGet packages (.Net 4.0, 3.5, PCL)
    build.cmd NuGet signed  # generate signed/strong named NuGet packages (.Net 4.0)
    
    build.cmd NativeBuild      # build native providers for all platforms
    build.cmd NativeTest       # test native providers for all platforms
    
    build.cmd All          # build, test, docs, api reference (.Net 4.0)
    build.cmd All release  # release build

FAKE itself is not included in the repository but it will download and bootstrap itself automatically when build.cmd is run the first time. Note that this step is *not* required when using Visual Studio or `msbuild` directly.

