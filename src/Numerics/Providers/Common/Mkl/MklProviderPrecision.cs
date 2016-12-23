// <copyright file="MklProviderPrecision.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2016 Math.NET
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

namespace MathNet.Numerics.Providers.Common.Mkl
{
    /// <summary>
    /// Consistency vs. performance trade-off between runs on different machines.
    /// </summary>
    public enum MklConsistency : int
    {
        /// <summary>Consistent on the same CPU only (maximum performance)</summary>
        Auto = 2,
        /// <summary>Consistent on Intel and compatible CPUs with SSE2 support (maximum compatibility)</summary>
        Compatible = 3,
        /// <summary>Consistent on Intel CPUs supporting SSE2 or later</summary>
        SSE2 = 4,
        /// <summary>Consistent on Intel CPUs supporting SSE4.2 or later</summary>
        SSE4_2 = 8,
        /// <summary>Consistent on Intel CPUs supporting AVX or later</summary>
        AVX = 9,
        /// <summary>Consistent on Intel CPUs supporting AVX2 or later</summary>
        AVX2 = 10
    }

    [CLSCompliant(false)]
    public enum MklAccuracy : uint
    {
        Low = 0x1,
        High = 0x2
    }

    [CLSCompliant(false)]
    public enum MklPrecision : uint
    {
        Single = 0x10,
        Double = 0x20
    }
}
