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
