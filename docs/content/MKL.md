Intel Math Kernel Library (MKL)
===============================

Math.NET Numerics is designed such that performance-sensitive algorithms
can be swapped with alternative implementations by the concept of providers.
There is currently only a provider for [linear algebra related routines](https://numerics.mathdotnet.com/api/MathNet.Numerics.Providers.LinearAlgebra.Mkl/MklLinearAlgebraProvider.htm), but there
are plans to add additional more e.g. related to nonlinear optimization problems or signal processing.

Providers become interesting when they can leverage a platform-native high performance library
like Intel MKL instead of the default purely managed provider. Math.NET Numerics
provides such a provider as NuGet packages:

* MathNet.Numerics.MKL.Win
* MathNet.Numerics.MKL.Linux

Since these native libraries can become very big, there are also variants supporting
only a single platform, for example:

* MathNet.Numerics.MKL.Win-x86
* MathNet.Numerics.MKL.Win-x64

In order to leverage the MKL linear algebra provider, we need to make sure the .NET
runtime can find the native libraries (see below) and then enable it by calling:

    [lang=csharp]
    Control.UseNativeMKL();

Alternatively you can also enable it by setting the environment variable `MathNetNumericsLAProvider=MKL`.

You can also explicitly disable the MKL provider by forcing it to use the managed provider by calling:

    [lang=csharp]
    Control.UseManaged();

You can tell what provider is effectively loaded by calling `Control.LinearAlgebraProvider.ToString()`,
which will return something along the lines of `Intel MKL (x86; revision 7)`.


Native Binaries
---------------

In .Net, the fusion engine is responsible for finding referenced
assemblies in the file system and loading them into the executing process.
However, native binaries like our MKL provider are platform specific,
so we need to load them with services of the platform instead of the .Net runtime.
We use P/Invoke to talk to the binaries, but for this to work they must
have already been loaded or the platform service needs to be able to find and
load them on its own.

In order to make providers easier to use, since v3.6.0 Math.NET Numerics
first tries to load native providers from a set of known directories before
falling back to the platform's default behavior. In each of these directories
it first looks for a processor-architecture specific folder within the directory,
before looking at the directory itself:

1. If `Control.NativeProviderPath` is set: `{NativeProviderPath}/{Platform}/`
2. If `Control.NativeProviderPath` is set: `{NativeProviderPath}/`
3. `{AppDomain.BaseDirectory}/{Platform}/`
4. `{AppDomain.BaseDirectory}/`
5. `{ExecutingAssemblyPath}/{Platform}/`
6. `{ExecutingAssemblyPath}/`
7. Fall back to the platform's default behavior (see below)

Where `{Platform}` can be one of the following: `x86`, `x64`, `ia64`, `arm` or `arm64`.

This means that you can, for example, place the 32 bit MKL provider binaries into `C:\MKL\x86`
and the 64 bit ones into `C:\MKL\x64`, and then set `Control.NativeProviderPath = @"C:\MKL";`.
Numerics will automatically choose the right one depending on whether your process is
running in 32 or 64 bit mode, and there is no more need to copy the large binaries to the
output folder of every script or project.


Default Behavior on Windows
---------------------------

On Windows it is usually enough to make sure the native libraries are in the
same folder as the executable. Reference the appropriate NuGet package and set
"Copy to Output Directory" for both MathNet.Numerics.MKL.dll and libiomp5md.dll
to "Copy always", or place the two native DLLs manually into the same directory
as your application's executable. There is no need to set the native provider
path explicitly.

For more details how the platform default behavior works and what influences it,
see [Dynamic-Link Library Search Order](https://msdn.microsoft.com/en-us/library/windows/desktop/ms682586.aspx).


Default Behavior on Linux
-------------------------

Native assembly resolving is very different on Linux than on Windows, simply putting the native
libraries into the same folder as the executable is not enough. The safe way is to edit `/etc/ld.so.conf`
and use `ldconfig` to tell where to look for the libraries. Alternatively you could add the path
to `LD_LIBRARY_PATH` or even just copy them to `/usr/lib`.

For details see Mono's [Interop with Native Libraries](http://www.mono-project.com/docs/advanced/pinvoke/#linux-shared-library-search-path).


Default Behavior on Mac OS X
----------------------------

You can configure the search path on one of the environment variables like `DYLD_LIBRARY_PATH`
or just copy them e.g. to `/usr/lib`.

For details see Mono's [Interop with Native Libraries](http://www.mono-project.com/docs/advanced/pinvoke/#mac-os-x-framework-and-dylib-search-path).

To build the MKL native provider for OSX:

1. Make sure you've a valid [Intel MKL](https://software.intel.com/en-us/qualify-for-free-software/academicresearcher) licence installed on your mac (look at opt/intel).
   If not, you can get a free trial on intel's web site.
2. Open the terminal
3. cd to the folder mathnet-numerics/src/NativeProviders/OSX
4. Run the .sh script by typping sh mkl_build.sh
5. ... wait for the build


    [lang=sh]
    lionel:~ Lionel$ cd /Users/Lionel/Public/Git/GitHub/mathnet-numerics/src/NativeProviders/OSX
    lionel:OSX Lionel$ ls
    mkl_build.sh
    lionel:OSX Lionel$ sh mkl_build.sh

Check the /x86 and /x64 folders in mathnet-numerics/out/MKL: you should now find the `libiomp5.dylib` and `MathNet.Numerics.MKL.dll` libaries.
You need to add the path to the generated libraies in your `DYLD_LIBRARY_PATH` environnement variable (which you can move to the folder of you choice before).
To do that, open your /Users/Lionel/.bas_profile.sh file with a text editor and add the following statements.

	[lang=sh]
    export DYLD_LIBRARY_PATH=$DYLD_LIBRARY_PATH:/Users/Lionel/../mathnet-numerics/out/MKL/OSX/x64
    export DYLD_LIBRARY_PATH=$DYLD_LIBRARY_PATH:/Users/Lionel/../mathnet-numerics/out/MKL/OSX/x86

Of course replace `Lionel` by your account login.

Have a look a the example down this page to compare MKL-provider vs. managed-provider performances.


F# Interactive
--------------

In F# Interactive, the easiest way to use native providers is to copy them to a shared
directory somewhere and use them directly from there:

    [lang=fsharp]
    Control.NativeProviderPath <- @"C:\MKL"
    Control.UseNativeMKL()

If you are using the F# Power Tools in VisualStudio, you can also let it generate "Reference
scripts for F# Interactive" right from the context menu. This will generate a script called
`load-references.fsx` in a `Scripts` folder, which you can extend as follows to load the
MKL provider automatically.

    [lang=fsharp]
    open System.IO
    open MathNet.Numerics

    Control.NativeProviderPath <- Path.Combine(__SOURCE_DIRECTORY__,"../")
    Control.UseNativeMKL()

This script assumes that the MKL binaries have been copied to the project directory,
which is also where the NuGet packages place them by default. If you place them somewhere
else, adapt the path accordingly.

See also [Loading Native DLLs in F# Interactive](http://christoph.ruegg.name/blog/loading-native-dlls-in-fsharp-interactive.html)
for more alternatives.

LINQPad and assembly shadowing
------------------------------

The automatic strategy may still work if assembly shadowing is involved,
but it often simpler and more reliable to provide the folder explicitly.
This also works well in LINQPad, with and without assembly shadowing:

    [lang=csharp]
    Control.NativeProviderPath = @"C:\MKL";
    Control.UseNativeMKL();


Example: Intel MKL on Linux with Mono
-------------------------------------

We also provide MKL NuGet package for Linux if you do not want to build them yourself. Assuming you have
Mono and NuGet installed (here v3.2.8), you can fetch the MKL package of the right architecture
(x64 or x86, `uname -m` if you don't know) as usual:

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
