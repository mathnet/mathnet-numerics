Math.NET Numerics
=================

Math.NET Numerics is an opensource **numerical library for .Net, Silverlight and Mono**.

Math.NET Numerics is the numerical foundation of the Math.NET project,
aiming to provide methods and algorithms for numerical computations in science,
engineering and every day use. Covered topics include special functions,
linear algebra, probability models, random numbers, statistics, interpolation,
integral transforms (FFT) and more.

In addition to the core .NET package (which is written entirely in C#),
Numerics specifically supports F# 3.0 with idiomatic extension modules and
maintains mathematical data structures like BigRational that originated in the F# PowerPack.
If a performance boost is needed, the managed-code provider backing its linear algebra
routines and decompositions can be exchanged with wrappers for optimized native
implementations such as Intel MKL.

Supports Mono and .NET 4.0 on Linux, Mac and Windows, the portable version also
SL5 and .NET for Windows Store apps.

Math.NET Numerics is covered under the terms of the [MIT/X11](http://mathnetnumerics.codeplex.com/license)
license. You may therefore link to it and use it in both opensource and proprietary
software projects. See also the [COPYRIGHT](COPYRIGHT.markdown) file in the root folder.

Math.NET Numerics is the result of merging [dnAnalytics](http://dnanalytics.codeplex.com/)
with [Math.NET Iridium](http://www.mathdotnet.com/Iridium.aspx) and replaces both.

Quick Links
-----------

* [**Project Website**](http://numerics.mathdotnet.com)
* [Source Code](http://github.com/mathnet/mathnet-numerics)
* [Downloads](http://mathnetnumerics.codeplex.com/releases)
* [Documentation](http://mathnetnumerics.codeplex.com/documentation)
* [API Reference](http://numerics.mathdotnet.com/api/)
* [Code Samples](http://github.com/mathnet/mathnet-numerics/tree/master/src/Examples)
* [Discussions](http://mathnetnumerics.codeplex.com/discussions)
* [Work Items and Bug Tracker](http://github.com/mathnet/mathnet-numerics/issues)
* [Ideas & Feedback](http://feedback.mathdotnet.com/forums/2060-math-net-numerics)

Feeds:

* [Blog Feed](http://feeds.mathdotnet.com/MathNetNumerics)
* [Activity Feed](http://feeds.mathdotnet.com/MathNetNumericsActivity)

Math.NET Numerics on other sites:

* [@MathDotNet](http://twitter.com/MathDotNet)
* [Google+](https://plus.google.com/112484567926928665204)
* [Ohloh](https://www.ohloh.net/p/mathnet)
* [Stack Overflow](http://stackoverflow.com/questions/tagged/mathdotnet)

Installation Instructions
-------------------------

Download the *MathNet.Numerics.dll* assembly, add a reference to it to your project and you're done. To make this even simpler we publish binary releases to the [**NuGet Gallery**](http://nuget.org/) as package *MathNet.Numerics* (or *MathNet.Numerics.FSharp* for F# integration). Altenatively we also publish binary releases including documentation on [CodePlex](http://mathnetnumerics.codeplex.com/releases).

Team & Contributors
-------------------

**Primary Authors**:
[Marcus Cuda](http://marcuscuda.com/),
[Jurgen Van Gael](http://mlg.eng.cam.ac.uk/jurgen/),
[Christoph Rüegg](http://christoph.ruegg.name/),
[Andriy Bratiychuk](http://ua.linkedin.com/pub/andriy-bratiychuk/4/6b/920)  
**Contributors**:
Alexander Karatarakis, Patrick van der Velde, Joannès Vermorel,
Matthew Kitchin, Rana Ian, Andrew Kurochka,
Thaddaeus Parker, Sergey Bochkanov (ALGLIB), John Maddock (Boost),
Stephen L. Moshier (Cephes Math Library)

See also the [AUTHORS](AUTHORS.markdown) file for a full list of all contributors.
