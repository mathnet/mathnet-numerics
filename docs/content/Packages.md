NuGet Packages & Binaries
=========================

The recommended way to get Math.NET Numerics is NuGet. The following packages are
provided and maintained in the public [NuGet Gallery](https://nuget.org/profiles/mathnet/).
The complete set of Zip and NuGet packages including symbol packages is also available in the
[release archive](https://1drv.ms/1NlUeDT).

*We're currently planning what platforms we should support in the future.
Consider to [vote for the platforms you need to be supported](https://discuss.mathdotnet.com/t/poll-what-platforms-should-math-net-numerics-support/60),
especially if you need support for older or more exotic platforms.*

Math.NET Numerics
-----------------

In most scenarios you'll only need the primary package named `MathNet.Numerics`.
If you are working with F# we recommend to also use the F# extension package
for a more natural and idiomatic experience.

[MathNet.NET Numerics Release Notes](ReleaseNotes.html)

- [**MathNet.Numerics**](https://www.nuget.org/packages/MathNet.Numerics/) - core package, including .Net 4, .Net 3.5 and portable/PCL builds.
- [**MathNet.Numerics.FSharp**](https://www.nuget.org/packages/MathNet.Numerics.FSharp/) - optional extensions for a better F# experience. BigRational.

Both packages above do not have a strong name. While we do not recommend it,
there are valid scenarios where strong named assemblies are required. That's why
we also provide strong-named variants with the `.Signed` suffix. Note that signed
packages do not contain portable builds.

- [**MathNet.Numerics.Signed**](https://www.nuget.org/packages/MathNet.Numerics.Signed/) - strong-named version of the core package.
- [**MathNet.Numerics.FSharp.Signed**](https://www.nuget.org/packages/MathNet.Numerics.FSharp.Signed/) - strong-named version of the F# package.

Intel MKL Native Provider
-------------------------

The new combined package includes both 32 and 64 bit binaries and can automatically
pick the right one at runtime. It is also MsBuild integrated, so there is no
more need to any manual file handling. But it is only supported by Math.NET Numerics
v3.6.0 and higher.

If you intend to [maintain the native binaries manually](MKL.html#Native-Binaries)
it may be easier to download the Zip file in the release archive.

[Intel MKL Native Provider Release Notes](ReleaseNotes-MKL.html)

- [**MathNet.Numerics.MKL.Win**](https://www.nuget.org/packages/MathNet.Numerics.MKL.Win/) - Windows (combined, MsBuild integrated).
- [**MathNet.Numerics.MKL.Win-x64**](https://www.nuget.org/packages/MathNet.Numerics.MKL.Win-x64/) - Windows 64-bit only.
- [**MathNet.Numerics.MKL.Win-x86**](https://www.nuget.org/packages/MathNet.Numerics.MKL.Win-x86/) - Windows 32-bit only.
- [**MathNet.Numerics.MKL.Linux-x64**](https://www.nuget.org/packages/MathNet.Numerics.MKL.Linux-x64/) - Linux 64-bit.
- [**MathNet.Numerics.MKL.Linux-x86**](https://www.nuget.org/packages/MathNet.Numerics.MKL.Linux-x86/) - Linux 32-bit.

Data Extensions
---------------

Data/IO Packages for reading and writing data.

[Data Extensions Release Notes](ReleaseNotes-Data.html)

- [**MathNet.Numerics.Data.Text**](https://www.nuget.org/packages/MathNet.Numerics.Data.Text/) - Text-based matrix formats like [CSV](CSV.html) and [MatrixMarket](MatrixMarket.html).
- [**MathNet.Numerics.Data.Matlab**](https://www.nuget.org/packages/MathNet.Numerics.Data.Matlab/) - [MATLAB Level-5](MatlabFiles.html) matrix file format.
