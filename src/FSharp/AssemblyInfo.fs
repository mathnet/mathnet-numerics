// <copyright file="AssemblyInfo.fs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009 Math.NET
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
// </copyright>

namespace MathNet.Numerics

open System.Reflection
open System.Resources;
open System.Runtime.CompilerServices
open System.Runtime.InteropServices

[<assembly: AssemblyTitle("Math.NET Numerics for F#")>]
[<assembly: AssemblyDescription("F# Modules for Math.NET Numerics, providing methods and algorithms for numerical computations in science, engineering and every day use.")>]
[<assembly: AssemblyConfiguration("")>]
[<assembly: AssemblyCompany("Math.NET Project")>]
[<assembly: AssemblyProduct("Math.NET Numerics")>]
[<assembly: AssemblyCopyright("Copyright (c) Math.NET Project")>]
[<assembly: AssemblyTrademark("")>]
[<assembly: AssemblyCulture("")>]
[<assembly: NeutralResourcesLanguage("en")>]

[<assembly: AssemblyVersion("3.20.0.0")>]
[<assembly: AssemblyFileVersion("3.20.0.0")>]
[<assembly: AssemblyInformationalVersion("3.20.0-beta01")>]

#if PORTABLE
#else
[<assembly: ComVisible(false)>]
[<assembly: Guid("048BC4EB-CE2B-4040-9967-4784F5405B0F")>]
#endif

()
