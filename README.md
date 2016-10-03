Math.NET Numerics
=================

Math.NET Numerics is an opensource **numerical library for .Net, Silverlight and Mono**.

Math.NET Numerics is the numerical foundation of the Math.NET initiative, aiming to provide methods and algorithms for numerical computations in science, engineering and every day use. Covered topics include special functions, linear algebra, probability models, random numbers, statistics, interpolation, integration, regression, curve fitting, integral transforms (FFT) and more.

In addition to the core .NET package (which is written entirely in C#), Numerics specifically supports F# with idiomatic extension modules and maintains mathematical data structures like BigRational that originated in the F# PowerPack. If a performance boost is needed, the managed-code provider backing its linear algebra routines and decompositions can be exchanged with wrappers for optimized native implementations such as Intel MKL.

Math.NET Numerics is covered under the terms of the [MIT/X11](LICENSE.md) license. You may therefore link to it and use it in both opensource and proprietary software projects. We accept contributions!

* [**Project Website**](http://numerics.mathdotnet.com)
* [Source Code](http://github.com/mathnet/mathnet-numerics)
* [NuGet & Binaries](http://numerics.mathdotnet.com/Packages.html) | [Release Notes](http://numerics.mathdotnet.com/ReleaseNotes.html)
* [Documentation](http://numerics.mathdotnet.com) | [API Reference](http://numerics.mathdotnet.com/api/)
* [Issues & Bugs](http://github.com/mathnet/mathnet-numerics/issues) | [Ideas](http://feedback.mathdotnet.com/forums/2060-math-net-numerics)
* [Discussions](http://discuss.mathdotnet.com/c/numerics) | [Stack Overflow](http://stackoverflow.com/questions/tagged/mathdotnet) | [Twitter](http://twitter.com/MathDotNet)
* [Wikipedia](http://en.wikipedia.org/wiki/Math.NET_Numerics) | [OpenHUB](https://www.ohloh.net/p/mathnet)

### Current Version

![Math.NET Numerics Version](https://buildstats.info/nuget/MathNet.Numerics) Math.NET Numerics  
![MKL Native Provider Version](https://buildstats.info/nuget/MathNet.Numerics.MKL.Win) MKL Native Provider  
![OpenBLAS Native Provider Version](https://buildstats.info/nuget/MathNet.Numerics.OpenBLAS.Win) OpenBLAS Native Provider  
![Data Extensions Version](https://buildstats.info/nuget/MathNet.Numerics.Data.Text) Data Extensions

Installation Instructions
-------------------------

The recommended way to get Math.NET Numerics is to use NuGet. The following packages are provided and maintained in the public [NuGet Gallery](https://nuget.org/profiles/mathnet/).

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

Platform Support and Dependencies
---------------------------------

Supported Platforms:

- .Net 4.0, .Net 3.5 and Mono: Windows, Linux and Mac.
- PCL Portable Profiles 7, 47, 78, 259 and 328: Windows 8, Silverlight 5, Windows Phone/SL 8, Windows Phone 8.1.
- Xamarin: Android, iOS

For full details, dependencies and platform discrepancies see [Platform Compatibility](http://numerics.mathdotnet.com/Compatibility.html).

Building Math.NET Numerics
--------------------------

Windows (.Net): [![AppVeyor build status](https://ci.appveyor.com/api/projects/status/79j22c061saisces/branch/master)](https://ci.appveyor.com/project/cdrnet/mathnet-numerics)  
Linux (Mono): [![Travis Build Status](https://travis-ci.org/mathnet/mathnet-numerics.svg?branch=master)](https://travis-ci.org/mathnet/mathnet-numerics)

You can build Math.NET Numerics with an IDE like VisualStudio or Xamarin,
with MsBuild or with FAKE.

MsBuild/XBuild:

    restore.cmd (or restore.sh)
    msbuild MathNet.Numerics.sln

FAKE:

    build.cmd Build    # build from the Windows console with .Net
    ./build.sh Build   # build from Bash, with Mono on Linux/Mac or .Net on Windows
    ./build.sh Test    # build and run unit tests

See [Build & Tools](http://numerics.mathdotnet.com/Build.html) for full details
on how to build, generate documentation or even create a full release.
