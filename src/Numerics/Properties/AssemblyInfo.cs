﻿// <copyright file="AssemblyInfo.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// https://numerics.mathdotnet.com
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

using System;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;

#if !PORTABLE
using System.Runtime.InteropServices;
#endif

[assembly: AssemblyDescription("Math.NET Numerics, providing methods and algorithms for numerical computations in science, engineering and every day use.")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Math.NET Project")]
[assembly: AssemblyProduct("Math.NET Numerics")]
[assembly: AssemblyCopyright("Copyright (c) Math.NET Project")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: CLSCompliant(true)]
[assembly: NeutralResourcesLanguage("en")]

[assembly: AssemblyVersion("3.20.0.0")]
[assembly: AssemblyFileVersion("3.20.0.0")]
[assembly: AssemblyInformationalVersion("3.20.0")]

#if PORTABLE || NETSTANDARD1_6

[assembly: AssemblyTitle("Math.NET Numerics - Portable Edition")]
[assembly: InternalsVisibleTo("MathNet.Numerics.UnitTests")]
[assembly: InternalsVisibleTo("MathNet.Numerics.UnitTests7")]
[assembly: InternalsVisibleTo("MathNet.Numerics.UnitTests47")]
[assembly: InternalsVisibleTo("MathNet.Numerics.UnitTests78")]
[assembly: InternalsVisibleTo("MathNet.Numerics.UnitTests259")]
[assembly: InternalsVisibleTo("MathNet.Numerics.UnitTests328")]

#elif NET35

[assembly: AssemblyTitle("Math.NET Numerics - .Net 3.5 Edition")]
[assembly: InternalsVisibleTo("MathNet.Numerics.UnitTests")]
[assembly: InternalsVisibleTo("MathNet.Numerics.UnitTestsNet35")]

#else

[assembly: AssemblyTitle("Math.NET Numerics")]
[assembly: ComVisible(false)]
[assembly: Guid("7b66646f-f0ee-425d-9065-910d1937a2df")]

#if STRONGNAME
[assembly: InternalsVisibleTo("MathNet.Numerics.UnitTests, PublicKey=0024000004800000940000000602000000240000525341310004000001000100ed2314a577643d859571b8b9307c6ff2670525c4598fbb307e57ea65ebf5d4417284cb3da9181636480b623f4db8cc3c1947244ba069df0df86e2431621f51a488f9929519a1c5d0ae595f6e2d0e4094685f0c1229ff658360acbb9f63f1a0258e984dda00dc7ad4fd16dbb550ec1ef8a11df138402b7c1998ee224e652c839b")]
#else
[assembly: InternalsVisibleTo("MathNet.Numerics.UnitTests")]
[assembly: InternalsVisibleTo("MathNet.Numerics.UnitTestsMKL")]
[assembly: InternalsVisibleTo("MathNet.Numerics.UnitTestsCUDA")]
[assembly: InternalsVisibleTo("MathNet.Numerics.UnitTestsOpenBLAS")]
[assembly: InternalsVisibleTo("Benchmark")]
#endif

#endif
