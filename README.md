Math.NET Numerics
=================

Math.NET Numerics is an opensource **numerical library for .Net, Silverlight and Mono**.

Math.NET Numerics is the numerical foundation of the Math.NET initiative, aiming to provide methods and algorithms for numerical computations in science, engineering and every day use. Covered topics include special functions, linear algebra, probability models, random numbers, statistics, interpolation, integration, regression, curve fitting, integral transforms (FFT) and more.

In addition to the core .NET package (which is written entirely in C#), Numerics specifically supports F# 3.0 and 3.1 with idiomatic extension modules and maintains mathematical data structures like BigRational that originated in the F# PowerPack. If a performance boost is needed, the managed-code provider backing its linear algebra routines and decompositions can be exchanged with wrappers for optimized native implementations such as Intel MKL.

Supports Mono and .NET 4.0 and 3.5 on Linux, Mac and Windows, the portable build (PCL) also Silverlight 5, Windows Phone 8, .NET for Windows Store apps and Xamarin Android/iOS.

Math.NET Numerics is covered under the terms of the [MIT/X11](http://mathnetnumerics.codeplex.com/license) license. You may therefore link to it and use it in both opensource and proprietary software projects. See also the [license](LICENSE.md) file in the root folder.

Maintained by [Christoph Rüegg](http://christoph.ruegg.name/) but brought to you by all our awesome [contributors](CONTRIBUTORS.md) of Math.NET Numerics and its predecessors [dnAnalytics](http://dnanalytics.codeplex.com/) and [Math.NET Iridium](http://www.mathdotnet.com/Iridium.aspx). We accept contributions!

**[Release Notes & Changes](RELEASENOTES.md)**

Installation Instructions
-------------------------

The recommended way to get Math.NET Numerics is to use NuGet. The following packages are provided and maintained in the public [NuGet Gallery](https://nuget.org/profiles/mathnet/):

- **MathNet.Numerics** - core package, including .Net 4, .Net 3.5 and portable/PCL builds.
- **MathNet.Numerics.FSharp** - optional extensions for a better F# experience. BigRational.
- **MathNet.Numerics.Data.Text** - optional extensions for text-based matrix input/output.
- **MathNet.Numerics.Data.Matlab** - optional extensions for MATLAB matrix file input/output.
- **MathNet.Numerics.MKL.Win-x86** - optional Linear Algebra MKL native provider.
- **MathNet.Numerics.MKL.Win-x64** - optional Linear Algebra MKL native provider.
- **MathNet.Numerics.Signed** - strong-named version of the core package *(not recommended)*.
- **MathNet.Numerics.FSharp.Signed** - strong-named version of the F# package *(not recommended)*.

Alternatively you can also download the binaries in Zip packages, available on [CodePlex](http://mathnetnumerics.codeplex.com/releases):

- Binaries - core package and F# extensions, including .Net 4, .Net 3.5 and portable/PCL builds.
- Signed Binaries - strong-named version of the core package *(not recommended)*.

Supported Platforms:

- .Net 4.0, .Net 3.5 and Mono: Windows, Linux and Mac.
- PCL Portable Profiles 47 and 136: Silverlight 5, Windows Phone 8, .NET for Windows Store apps (Metro).
- PCL/Xamarin: Android, iOS  *(not verified due to lack of license and devices)*

Building Math.NET Numerics
--------------------------

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

    build.cmd   # normal build (.Net 4.0), run unit tests
    ./build.sh  # normal build (.Net 4.0), run unit tests - on Linux or Mac
    
    build.cmd Build              # normal build (.Net 4.0)
    build.cmd Build incremental  # normal build, incremental (.Net 4.0)
    build.cmd Build all          # full build (.Net 4.0, 3.5, PCL)
    build.cmd Build net35        # compatibility build (.Net 3.5)
    build.cmd Build signed       # normal build, signed/strong named (.Net 4.0)
    
    build.cmd Test        # normal build (.Net 4.0), run unit tests
    build.cmd Test all    # full build (.Net 4.0, 3.5, PCL), run all unit tests
    build.cmd Test net35  # compatibility build (.Net 3.5), run unit tests
    
    build.cmd Clean  # cleanup build artifacts
    build.cmd Docs   # generate documentation (.Net 4.0)
    
    build.cmd NuGet         # generate NuGet packages (.Net 4.0, 3.5, PCL)
    build.cmd NuGet signed  # generate signed/strong named NuGet packages (.Net 4.0)
    
    build.cmd BuildNativex86   # build native providers 32bit/x86
    build.cmd BuildNativex64   # build native providers 64bit/x64
    build.cmd BuildNative      # build native providers for all platforms
    build.cmd TestNativex86    # test native providers 32bit/x86
    build.cmd TestNativex64    # test native providers 64bit/x64
    build.cmd TestNative       # test native providers for all platforms

FAKE itself is not included in the repository but it will download and bootstrap itself automatically when build.cmd is run the first time. Note that this step is *not* required when using Visual Studio or `msbuild` directly.

Quick Links
-----------

* [**Project Website**](http://numerics.mathdotnet.com)
* [Source Code](http://github.com/mathnet/mathnet-numerics)
* [Downloads](http://mathnetnumerics.codeplex.com/releases)
* [Documentation](http://numerics.mathdotnet.com/docs/)
* [API Reference](http://numerics.mathdotnet.com/api/)
* [Code Samples](http://github.com/mathnet/mathnet-numerics/tree/master/src/Examples)
* [Discussions](http://mathnetnumerics.codeplex.com/discussions)
* [Work Items and Bug Tracker](http://github.com/mathnet/mathnet-numerics/issues)
* [Ideas & Feedback](http://feedback.mathdotnet.com/forums/2060-math-net-numerics)

Feeds:

* [Blog Feed](http://christoph.ruegg.name/blog/categories/math-net-numerics/atom.xml)
* [Activity Feed](http://feeds.mathdotnet.com/MathNetNumericsActivity)

Math.NET Numerics on other sites:

* [Twitter @MathDotNet](http://twitter.com/MathDotNet)
* [Google+](https://plus.google.com/112484567926928665204)
* [Ohloh](https://www.ohloh.net/p/mathnet)
* [Stack Overflow](http://stackoverflow.com/questions/tagged/mathdotnet)
* [Wikipedia](http://en.wikipedia.org/wiki/Math.NET_Numerics)
