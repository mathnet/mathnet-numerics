(*** hide ***)
#I "../../out/lib/net40"
#r "MathNet.Numerics.dll"
#r "MathNet.Numerics.FSharp.dll"

(**
Intel Math Kernel Library (MKL)
===============================

Math.NET Numerics is designed such that performance-sensitive algorithms
can be swapped with alternative implementations by the concept of providers.
There is currently only a provider for linear algebra related routines, but there
are plans to add additional ones e.g. related to nonlinear optimization problems or signal processing.

Providers become interesting when they can leverage a platform-native high performance library
like Intel MKL instead of the default purely managed provider. Math.NET Numerics
provides such a provider in the form of two NuGet packages:

* MathNet.Numerics.MKL.Win-x86
* MathNet.Numerics.MKL.Win-x64

Equivalent providers for Linux can be built manually from source.

In order to leverage the MKL linear algebra provider, reference the appropriate NuGet package
in your project. Set "Copy to Output Directory" for both MathNet.Numerics.MKL.dll and libiomp5md.dll
to "Copy always", or place the two native DLLs manually into the same directory as your application's executable.
Then enable it by calling:

    [lang=csharp]
    Control.UseNativeMKL();

Alternatively you can also enable it by setting the environment variable `MathNetNumericsLAProvider=MKL`.

You can also explicitly disable the MKL provider by forcing it to use the managed provider by calling:

    [lang=csharp]
    Control.UseManaged();


Using Intel MKL on Linux with Mono
----------------------------------

We no longer provide new MKL NuGet packages for Linux since we no longer have an license to do so
(only Windows for now), but it is still possible to use the older Linux MKL NuGet packages if you
don't want to build it yourself. Assuming you have Mono and NuGet installed (here v3.2.8), you can
fetch the MKL package of the right architecture (x64 or x86, `uname -m` if you don't know) as usual:

    [lang=sh]
    mono nuget.exe install MathNet.Numerics -Pre -OutputDirectory packages
    mono nuget.exe install MathNet.Numerics.MKL.Linux-x64 -Pre -OutputDirectory packages

Native assembly resolving is very different on Linux than on Windows, simply putting the native
libraries into the same folder as the executable is not enough. The safe way is to edit `/etc/ld.so.conf`
and use `ldconfig` to tell where to look for the libraries, but for now we'll just copy them to `/usr/lib`:

    [lang=sh]
    sudo cp packages/MathNet.Numerics.MKL.Linux-x64.1.3.0/content/libiomp5.so /usr/lib/
    sudo cp packages/MathNet.Numerics.MKL.Linux-x64.1.3.0/content/MathNet.Numerics.MKL.dll /usr/lib/

Then we're all set and can just call `Control.UseNativeMKL()` if we want to use the native provider.
Let's create the following C# file `Example.cs`:

    [lang=csharp]
    using System;
    using System.Diagnostics;
    using MathNet.Numerics;
    using MathNet.Numerics.LinearAlgebra;

    class Program
    {
        static void Main(string[] args)
        {
            // Using managed code only
            Control.UseManaged();
            Console.WriteLine(Control.LinearAlgebraProvider);

            var m = Matrix<double>.Build.Random(500, 500);
            var v = Vector<double>.Build.Random(500);

            var w = Stopwatch.StartNew();
            var y1 = m.Solve(v);
            Console.WriteLine(w.Elapsed);
            Console.WriteLine(y1);

            // Using the Intel MKL native provider
            Control.UseNativeMKL();
            Console.WriteLine(Control.LinearAlgebraProvider);

            w.Restart();
            var y2 = m.Solve(v);
            Console.WriteLine(w.Elapsed);
            Console.WriteLine(y2);
        }
    }

Compile and run:

    [lang=sh]
    # single line:
    mcs -optimize -lib:packages/MathNet.Numerics.3.0.0-alpha8/lib/net40/
                  -r:MathNet.Numerics.dll Example.cs -out:Example
    # launch:
    mono Example


Licensing Restrictions
----------------------

Be aware that unlike the core of Math.NET Numerics including the native wrapper, which are both
open source under the terms of the MIT/X11 license, the Intel MKL binaries themselves are closed
source and non-free.

The Math.NET Numerics project does own an Intel MKL license (for Windows, no longer for Linux) and
thus does have the right to distribute it along Math.NET Numerics. You can therefore use the Math.NET
Numerics MKL native provider for free for your own use. However, it does *not* give you any right to
redistribute it again yourself to customers of your own product. **If you need to redistribute,
buy a license from Intel. If unsure, contact the Intel sales team to clarify.**
The Math.NET team absolutely loves open source and free access, but we also respect commercial
vendors and their restricted licensing terms, and ask you to do the same. Thanks.

*)
