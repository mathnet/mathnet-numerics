Platform Support
================

*We're currently planning what platforms we should support in the future.
Consider to [vote for the platforms you need to be supported](https://discuss.mathdotnet.com/t/poll-what-platforms-should-math-net-numerics-support/60),
especially if you need support for older or more exotic platforms.*

Supported Platforms:

- .Net 4.0, .Net 3.5 and Mono: Windows, Linux and Mac.
- PCL Portable Profiles 7, 47, 78, 259 and 328: Windows 8, Silverlight 5, Windows Phone/SL 8, Windows Phone 8.1.
- Xamarin: Android, iOS

The F# extensions support a slightly reduced platform set:

- .Net 4.0, .Net 3.5 and Mono: Windows, Linux and Mac.
- PCL Portable Profile 47: Windows 8, Silverlight 5
- Xamarin: Android, iOS

Configuration | Net35 | Net40 | Net45 | SL5  | Win8 | WP8/SL | WP8.1 | Xamarin
------------- | ----- | ----- | ----- | ---- | ---- | ------ | ----- | -------
.Net 4.0      | -     | Best  | Best  | -    | -    | -      | -     | -
.Net 3.5      | Best  | OK    | OK    | -    | -    | -      | -     | -
Portable 7    | -     | -     | OK    | -    | Best | -      | -     | OK
Portable 47   | -     | -     | OK    | Best | OK   | -      | -     | OK
Portable 78   | -     | -     | OK    | -    | OK   | Best   | -     | OK
Portable 259  | -     | -     | OK    | -    | OK   | OK     | Best  | OK
Portable 328  | -     | OK    | OK    | OK   | OK   | OK     | OK    | OK


Dependencies
------------

Package Dependencies:

- .Net 4.0 and higher, Mono, PCL Profiles: None
- .Net 3.5: [Task Parallel Library for .NET 3.5](https://www.nuget.org/packages/TaskParallelLibrary)
- F# on  .Net 4.0 an higher, Mono, PCL Profiles: additionally [FSharp.Core](https://www.nuget.org/packages/FSharp.Core)

Framework Dependencies (part of the .NET Framework):

- .Net 4.0 and higher, Mono, PCL profiles 7 and 47: System.Numerics
- .Net 3.5, PCL profiles 78, 259 and 328: None


Platform Discrepancies
----------------------

Compilation symbols used to deal with platform differences:

* **NET35** - Some framework attributes are not available and we provide our own Tuple types, generic comparer, LINQ Zip routine and thread partitioner. The crypto random source is not disposable.
* **PORTABLE** - Some framework attributes are not available and we provide our own parallelization routines and partitioning using TPL Tasks. Reduced globalization and serialization support. Work around some missing routines like `Math.DivRem`, `Array.FindIndex` and `BitConverter`. There is no `ICloneable`. The crypto random source is not available; simpler random seeding.
* **NOSYSNUMERICS** - The `System.Numerics` framework assembly is not available. We provide our own double-precision complex number type and disable all arbitrary precision numbers support (BigInteger, BigRational).
* **NET45REFLECTION** - we use the new .Net 4.5 reflection API where type information is split into `Type` and `TypeInfo`.
* **NATIVE** - we can support native providers like Intel MKL.

Configuration | Net35 | Portable | NoSysNumerics | Net45Reflection | Native
------------- | ----- | -------- | ------------- | --------------- | ------
.Net 4.0      | -     | -        | -             | -               | Yes
.Net 3.5      | Yes   | -        | Yes           | -               | -
Portable 7    | -     | Yes      | -             | Yes             | -
Portable 47   | -     | Yes      | -             | -               | -
Portable 78   | -     | Yes      | Yes           | Yes             | -
Portable 259  | -     | Yes      | Yes           | Yes             | -
Portable 328  | -     | Yes      | Yes           | -               | -
