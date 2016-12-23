namespace MathNet.Numerics

open System.Reflection
open System.Resources;
open System.Runtime.CompilerServices
open System.Runtime.InteropServices

[<assembly: AssemblyTitle("Math.NET Numerics for F# Unit Tests")>]
[<assembly: AssemblyCompany("Math.NET Project")>]
[<assembly: AssemblyProduct("Math.NET Numerics")>]
[<assembly: AssemblyCopyright("Copyright (c) Math.NET Project")>]

[<assembly: AssemblyVersion("3.14.0.0")>]
[<assembly: AssemblyFileVersion("3.14.0.0")>]
[<assembly: AssemblyInformationalVersion("3.14.0-beta03")>]

#if PORTABLE
#else
[<assembly: ComVisible(false)>]
[<assembly: Guid("C9AA6156-F799-42E4-B50D-2E88AD7D1750")>]
#endif

()
