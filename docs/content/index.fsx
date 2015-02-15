(*** hide ***)
#I "../../out/lib/net40"
#r "MathNet.Numerics.dll"
#r "MathNet.Numerics.FSharp.dll"

(**
Math.NET Numerics
=================

Math.NET Numerics aims to provide methods and algorithms for numerical computations
in science, engineering and every day use. Covered topics include special functions,
linear algebra, probability models, random numbers, interpolation, integration,
regression, optimization problems and more.

Math.NET Numerics is part of the [Math.NET initiative](http://www.mathdotnet.com/)
and is the result of merging dnAnalytics with Math.NET Iridium, replacing both.
It targets Microsoft .Net 4, .Net 3.5 and Mono (Windows, Linux and Mac),
Silverlight 5, WindowsPhone 8, Windows 8/Store (PCL 47, 136) and Android/iOS (Xamarin).
In addition to a purely managed implementation it also supports native hardware
optimization. Available for free under the [MIT/X11 License](License.html).

NuGet Packages
--------------

The recommended way to get Math.NET Numerics is to use NuGet. The following
packages are provided and maintained in the public
[NuGet Gallery](https://nuget.org/profiles/mathnet/).
Alternatively you can also download the binaries in Zip packages, available on
[CodePlex](http://mathnetnumerics.codeplex.com/releases).

Core Package:

- [**MathNet.Numerics**](https://www.nuget.org/packages/MathNet.Numerics/) - core package, including .Net 4, .Net 3.5 and portable/PCL builds.
- [**MathNet.Numerics.FSharp**](https://www.nuget.org/packages/MathNet.Numerics.FSharp/) - optional extensions for a better F# experience. BigRational.
- [**MathNet.Numerics.Signed**](https://www.nuget.org/packages/MathNet.Numerics.Signed/) - strong-named version of the core package *(not recommended)*.
- [**MathNet.Numerics.FSharp.Signed**](https://www.nuget.org/packages/MathNet.Numerics.FSharp.Signed/) - strong-named version of the F# package *(not recommended)*.

Alternative Provider Packages (optional):

- [**MathNet.Numerics.MKL.Win-x86**](https://www.nuget.org/packages/MathNet.Numerics.MKL.Win-x86/) - Native Intel MKL Linear Algebra provider (Windows/32-bit).
- [**MathNet.Numerics.MKL.Win-x64**](https://www.nuget.org/packages/MathNet.Numerics.MKL.Win-x64/) - Native Intel MKL Linear Algebra provider (Windows/64-bit).

Data/IO Packages for reading and writing data (optional):

- [**MathNet.Numerics.Data.Text**](https://www.nuget.org/packages/MathNet.Numerics.Data.Text/) - Text-based matrix formats like CSV and MatrixMarket.
- [**MathNet.Numerics.Data.Matlab**](https://www.nuget.org/packages/MathNet.Numerics.Data.Matlab/) - MATLAB Level-5 matrix file format.


Platform Support and Dependencies
---------------------------------

- .Net 4.0, .Net 3.5 and Mono: Windows, Linux and Mac.
- PCL Portable Profiles 7, 47, 78, 259 and 328: Windows 8, Silverlight 5, Windows Phone/SL 8, Windows Phone 8.1.
- Xamarin: Android, iOS

The F# extensions support a slightly reduced platform set:

- .Net 4.0, .Net 3.5 and Mono: Windows, Linux and Mac.
- PCL Portable Profile 47: Windows 8, Silverlight 5
- Xamarin: Android, iOS

Package Dependencies:

- .Net 4.0 and higher, Mono, PCL Profiles: None
- .Net 3.5: [Task Parallel Library for .NET 3.5](http://www.nuget.org/packages/TaskParallelLibrary)
- F# on  .Net 4.0 an higher, Mono, PCL Profiles: additionally [FSharp.Core](http://www.nuget.org/packages/FSharp.Core)

Framework Dependencies (part of the .NET Framework):

- .Net 4.0 and higher, Mono, PCL profiles 7 and 47: System.Numerics
- .Net 3.5, PCL profiles 78, 259 and 328: None


Using Math.NET Numerics with C#
-------------------------------

Being written in it, Math.NET Numerics works very well with C# and related .Net languages.
When using Visual Studio or another IDE with built-in NuGet support, you can get started
quickly by adding a reference to the `MathNet.Numerics` NuGet package. Alternatively you can grab
that package with the command line tool with `nuget.exe install MathNet.Numerics -Pre`
or simply download the Zip package.

let's say we have a matrix $\mathrm{A}$ and want to find an orthonormal basis of the kernel or null-space
of that matrix, such that $\mathrm{A}x = 0$ for all $x$ in that subspace.

    [lang=csharp]
    using MathNet.Numerics.LinearAlgebra;
    using MathNet.Numerics.LinearAlgebra.Double;

    Matrix<double> A = DenseMatrix.OfArray(new double[,] {
            {1,1,1,1},
            {1,2,3,4},
            {4,3,2,1}});
    Vector<double>[] nullspace = A.Kernel();

    // verify: the following should be approximately (0,0,0)
    (A * (2*nullspace[0] - 3*nullspace[1]))


F# and F# Interactive
---------------------

Even though the core of Math.NET Numerics is written in C#, it aims to support F#
just as well. In order to achieve this we recommend to reference the `MathNet.Numerics.FSharp`
package as well (in addition to `MathNet.Numerics`) which adds a few modules to make it more
idiomatic and includes arbitrary precision types (BigInteger, BigRational).

It also works well in the interactive F# environment (REPL) which can be launched with
`fsharpi` on all platforms (including Linux). As a start let's enter the following lines
into F# interactive. Append `;;` to the end of a line to run all code up to there
immediately and print the result to the output. Use the tab key for auto-completion or `#help;;` for help.
For convenience our F# packages include a small script that sets everything up properly:

    [lang=fsharp]
    #load "../packages/MathNet.Numerics.FSharp.3.0.0/MathNet.Numerics.fsx"

    open MathNet.Numerics
    SpecialFunctions.Gamma(0.5)

    open MathNet.Numerics.LinearAlgebra
    let m : Matrix<float> = DenseMatrix.randomStandard 50 50
    (m * m.Transpose()).Determinant()


Visual Basic
------------

Let's use Visual Basic to find the polynomial roots $x$ such that $2x^2 - 2x - 2 = 0$
numerically. We already know there are two roots, one between -2 and 0, the other between 0 and 2:

    [lang=visualbasic]
    Imports MathNet.Numerics.RootFinding

    Dim f As Func(Of Double, Double) = Function(x) 2*x^2 - 2*x - 2

    Bisection.FindRoot(f, 0, 2) ' returns 1.61803398874989
    Bisection.FindRoot(f, -2, 0) ' returns -0.618033988749895

    ' Alternative to directly compute the roots for this special case:
    FindRoots.Quadratic(-2, -2, 2)


Linux with Mono
---------------

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

Compile and run:

    [lang=sh]
    # single line:
    mcs -optimize -lib:packages/MathNet.Numerics.3.0.0-alpha8/lib/net40/
                  -r:MathNet.Numerics.dll Start.cs -out:Start
    # launch:
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

*)
