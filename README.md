Math.NET Numerics
=================

Math.NET Numerics is an opensource **numerical library for .Net, Silverlight and Mono**.

Math.NET Numerics is the numerical foundation of the Math.NET initiative, aiming to provide methods and algorithms for numerical computations in science, engineering and every day use. Covered topics include special functions, linear algebra, probability models, random numbers, statistics, interpolation, integration, regression, curve fitting, integral transforms (FFT) and more.

In addition to the core .NET package (which is written entirely in C#), Numerics specifically supports F# with idiomatic extension modules and maintains mathematical data structures like BigRational that originated in the F# PowerPack. If a performance boost is needed, the managed-code provider backing its linear algebra routines and decompositions can be exchanged with wrappers for optimized native implementations such as Intel MKL.

Math.NET Numerics is covered under the terms of the [MIT/X11](LICENSE.md) license. You may therefore link to it and use it in both opensource and proprietary software projects. We accept contributions!

* [**Project Website**](https://numerics.mathdotnet.com)
* [Source Code](https://github.com/mathnet/mathnet-numerics)
* [NuGet & Binaries](https://numerics.mathdotnet.com/Packages.html) | [Release Notes](https://numerics.mathdotnet.com/ReleaseNotes.html)
* [Documentation](https://numerics.mathdotnet.com) | [API Reference](https://numerics.mathdotnet.com/api/)
* [Issues & Bugs](https://github.com/mathnet/mathnet-numerics/issues) | [Ideas](http://feedback.mathdotnet.com/forums/2060-math-net-numerics)
* [Discussions](https://discuss.mathdotnet.com/c/numerics) | [Stack Overflow](https://stackoverflow.com/questions/tagged/mathdotnet) | [Twitter](https://twitter.com/MathDotNet)
* [Wikipedia](https://en.wikipedia.org/wiki/Math.NET_Numerics) | [OpenHUB](https://www.openhub.net/p/mathnet-numerics)

### Current Version

![Math.NET Numerics Version](https://buildstats.info/nuget/MathNet.Numerics) Math.NET Numerics  
![MKL Native Provider Version](https://buildstats.info/nuget/MathNet.Numerics.MKL.Win) MKL Native Provider  
![OpenBLAS Native Provider Version](https://buildstats.info/nuget/MathNet.Numerics.OpenBLAS.Win) OpenBLAS Native Provider  
![Data Extensions Version](https://buildstats.info/nuget/MathNet.Numerics.Data.Text) Data Extensions

Installation Instructions
-------------------------

The recommended way to get Math.NET Numerics is to use NuGet. The following packages are provided and maintained in the public [NuGet Gallery](https://nuget.org/profiles/mathnet/).

Core Package:

- **MathNet.Numerics**
- **MathNet.Numerics.FSharp** - optional extensions for a better F# experience. BigRational.

Alternative Provider Packages (optional):

- **MathNet.Numerics.MKL.Win** - Native Intel MKL Linear Algebra provider (Windows).
- **MathNet.Numerics.MKL.Win-x86** - Native Intel MKL Linear Algebra provider (Windows/32-bit only).
- **MathNet.Numerics.MKL.Win-x64** - Native Intel MKL Linear Algebra provider (Windows/64-bit only).

Data/IO Packages for reading and writing data (optional):

- **MathNet.Numerics.Data.Text** - Text-based matrix formats like CSV and MatrixMarket.
- **MathNet.Numerics.Data.Matlab** - MATLAB Level-5 matrix file format.

Platform Support and Dependencies
---------------------------------

Supported Platforms:

- .Net Framework 4.0 or higher and Mono (Package includes builds for 4.0 and 4.6.1)
- .Net Standard 1.3 or higher (Package includes builds for 1.3 and 2.0)

Supported Platforms for the F# extensions:

- .Net Framework 4.5 or higher (Package includes builds for 4.5)
- .Net Standard 1.6 or higher (Package includes builds for 1.6 and 2.0)

For full details, dependencies and platform discrepancies see [Platform Compatibility](https://numerics.mathdotnet.com/Compatibility.html).

Building Math.NET Numerics
--------------------------

Windows (.Net): [![AppVeyor build status](https://ci.appveyor.com/api/projects/status/79j22c061saisces/branch/master)](https://ci.appveyor.com/project/cdrnet/mathnet-numerics) [![Build Status](https://dev.azure.com/mathdotnet/Math.NET%20Build/_apis/build/status/Math.NET%20Numerics?branchName=master)](https://dev.azure.com/mathdotnet/Math.NET%20Build/_build/latest?definitionId=1&branchName=master)  
Linux (Mono): [![Travis Build Status](https://travis-ci.org/mathnet/mathnet-numerics.svg?branch=master)](https://travis-ci.org/mathnet/mathnet-numerics)

You can build Math.NET Numerics with an IDE like VisualStudio or JetBrains Rider, with MsBuild, .Net CLI tools or with FAKE (recommended).

FAKE:

    ./build.sh build (or build.cmd)
    ./build.sh test

.Net CLI:

    ./restore.sh (or restore.cmd)
    dotnet build MathNet.Numerics.sln

MsBuild/XBuild:

    ./restore.sh (or restore.cmd)
    msbuild MathNet.Numerics.sln

See [Build & Tools](https://numerics.mathdotnet.com/Build.html) for full details
on how to build, generate documentation or even create a full release.
