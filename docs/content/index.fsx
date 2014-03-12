(*** hide ***)
#I "../../out/lib/net40"
#r "MathNet.Numerics.dll"
#r "MathNet.Numerics.FSharp.dll"

(**
Getting Started
===============

NuGet Packages
--------------

The recommended way to get Math.NET Numerics is to use NuGet. The following packages are provided and maintained in the public [NuGet Gallery](https://nuget.org/profiles/mathnet/):

- **MathNet.Numerics** - core package, including .Net 4, .Net 3.5 and portable/PCL builds.
- **MathNet.Numerics.FSharp** - optional extensions for a better F# experience. BigRational.
- **MathNet.Numerics.Data.Text** - optional extensions for text-based matrix input/output.
- **MathNet.Numerics.Data.Matlab** - optional extensions for MATLAB matrix file input/output.
- **MathNet.Numerics.MKL.Win-x86** - optional Linear Algebra MKL native provider.
- **MathNet.Numerics.MKL.Win-x64** - optional Linear Algebra MKL native provider.
- **MathNet.Numerics.Signed** - strong-named version of the core package *(not recommended)*.
- **MathNet.Numerics.FSharp.Signed** - strong-named version of the F# package *(not recommended)*.

Supported Platforms:

- .Net 4.0, .Net 3.5 and Mono: Windows, Linux and Mac.
- PCL Portable Profiles 47 and 136: Silverlight 5, Windows Phone 8, .NET for Windows Store apps (Metro).
- PCL/Xamarin: Android, iOS  *(not verified due to lack of license and devices)*

Alternatively you can also download the binaries in Zip packages, available on [CodePlex](http://mathnetnumerics.codeplex.com/releases):

- Binaries - core package and F# extensions, including .Net 4, .Net 3.5 and portable/PCL builds.
- Signed Binaries - strong-named version of the core package *(not recommended)*.


Using Math.NET Numerics with F#
-------------------------------

Even though the core of Math.NET Numerics is written in C#, it aims to support F#
just as well. In order to achieve this we recommend to reference the `MathNet.Numerics.FSharp`
package as well (in addition to `MathNet.Numerics`) which adds a few modules to make it more
idiomatic and includes arbitrary precision types (BigInteger, BigRational).

It also works well in the interactive F# environment (REPL) which can be launched with
`fsharpi` on all platforms (including Linux). As a start let's enter the following lines
into F# interactive. Each `;;` will cause the preceding lines to be executed immediately,
use the `Tab` key for auto-completion or `#help;;` for help.

    [lang=fsharp]
    #r "MathNet.Numerics.dll"
    #r "MathNet.Numerics.FSharp.dll";;

    open MathNet.Numerics;;
    SpecialFunctions.Gamma(0.5);;

    open MathNet.Numerics.LinearAlgebra;;
    let m : Matrix<float> = DenseMatrix.randomStandard 50 50;;
    (m * m.Transpose()).Determinant();;


Using Math.NET Numerics on Linux with Mono
------------------------------------------

You need a recent version of Mono in order to use Math.NET Numerics on anything other than Windows.
Luckily there has been great progress lately to make both Mono and F# available as proper Debian packages.
In Debian *testing* and Ubuntu *14.04 (trusty/universe)* you can install both of them with APT:

    [lang=sh]
    sudo apt-get update
    sudo apt-get install mono-complete
    sudo apt-get install fsharp

If you don't have NuGet yet:

    [lang=sh]
    sudo mozroots --import --sync
    curl -L http://nuget.org/nuget.exe -o nuget.exe

Then you can use NuGet to fetch the latest binaries in your working directory.
The `-Pre` argument causes it to include pre-releases, omit it if you want stable releases only.

    [lang=sh]
    mono nuget.exe install MathNet.Numerics -Pre -OutputDirectory packages
    # or if you intend to use F#:
    mono nuget.exe install MathNet.Numerics.FSharp -Pre -OutputDirectory packages

In practice you'd probably use the Monodevelop IDE instead which can take care of fetching and updating
NuGet packages and maintain assembly references. But for completeness let's use the compiler directly this time.
Let's create a C# file `Start.cs`:

    [lang=csharp]
    using System;
    using MathNet.Numerics;
    using MathNet.Numerics.LinearAlgebra;

    class Program
    {
        static void Main(string[] args)
        {
            // Evaluate a special function
            Console.WriteLine(SpecialFunctions.Erf(0.5));

            // Solve a random linear equation system with 500 unknowns
            var m = Matrix<double>.Build.Random(500, 500);
            var v = Vector<double>.Build.Random(500);
            var y = m.Solve(v);
            Console.WriteLine(y);
        }
    }

Since we want to use the compiler directly, let's copy all references to the working directory
as well to keep the command line more compact (normally you'd use the -lib argument instead) and compile:

    [lang=sh]
    cp packages/MathNet.Numerics.3.0.0-alpha8/lib/net40/* .
    mcs -optimize -r:MathNet.Numerics.dll Start.cs -out:Start

Run:

    [lang=sh]
    mono Start

Which will print something like the following to the output:

    [lang=text]
    0.520499877813047
    DenseVector 500-Double
       -0.181414     -1.25024    -0.607136      1.12975     -3.31201     0.344146
        0.934095     -2.96364      1.84499      1.20752     0.753055      1.56942
        0.472414      6.10418    -0.359401     0.613927    -0.140105       2.6079
        0.163564     -3.04402    -0.350791      2.37228     -1.65218     -0.84056
         1.51311     -2.17326    -0.220243   -0.0368934    -0.970052     0.580543
        0.755483     -1.01755    -0.904162     -1.21824     -2.24888      1.42923
       -0.971345     -3.16723    -0.822723      1.85148     -1.12235    -0.547885
        -2.01044      4.06481    -0.128382      0.51167     -1.70276          ...

See [Intel MKL](MKL.html) for details how to use native providers on Linux.


Building Math.NET Numerics
--------------------------

If you do not want to use the official binaries, or if you like to modify, debug or contribute, you can compile Math.NET Numerics locally either using Visual Studio or manually with the build scripts.

* The Visual Studio solutions should build out of the box, without any preparation steps or package restores.
* Instead of a compatible IDE you can also build the solutions with `msbuild`, or on Mono with `xbuild`.
* The full build including unit tests, docs, NuGet and Zip packages is using [FAKE](http://fsharp.github.io/FAKE/).

### How to build with MSBuild/XBuild

    [lang=sh]
    msbuild MathNet.Numerics.sln            # only build for .Net 4 (main solution)
    msbuild MathNet.Numerics.Net35Only.sln  # only build for .Net 3.5
    msbuild MathNet.Numerics.Portable.sln   # full build with .Net 4, 3.5 and PCL profiles
    xbuild MathNet.Numerics.sln             # build with Mono, e.g. on Linux or Mac

### How to build with FAKE

    [lang=sh]
    build.cmd   # normal build (.Net 4.0), run unit tests
    ./build.sh  # normal build (.Net 4.0), run unit tests - on Linux or Mac
    
    build.cmd Build              # normal build (.Net 4.0)
    build.cmd Build incremental  # normal build, incremental (.Net 4.0)
    build.cmd Build full         # full build (.Net 3.5, 4.0, PCL)
    build.cmd Build net35        # compatibility build (.Net 3.5)
    
    build.cmd Test        # normal build (.Net 4.0), run unit tests
    build.cmd Test full   # full build (.Net 3.5, 4.0, PCL), run all unit tests
    build.cmd Test net35  # compatibility build (.Net 3.5), run unit tests
    
    build.cmd Clean  # cleanup build artifacts
    build.cmd Docs   # generate documentation, normal build
    build.cmd NuGet  # generate NuGet packages, full build

    build.cmd BuildNativex86   # build native providers 32bit/x86
    build.cmd BuildNativex64   # build native providers 64bit/x64
    build.cmd BuildNative      # build native providers for all platforms
    build.cmd TestNativex86    # test native providers 32bit/x86
    build.cmd TestNativex64    # test native providers 64bit/x64
    build.cmd TestNative       # test native providers for all platforms

FAKE itself is not included in the repository but it will download and bootstrap itself automatically when build.cmd is run the first time. Note that this step is *not* required when using Visual Studio or `msbuild` directly.
*)
