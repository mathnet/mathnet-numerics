// <copyright file="MklProviderCapabilities.cs" company="Math.NET">
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

#if NATIVE

namespace MathNet.Numerics.Providers.Common.Mkl
{
    internal enum ProviderPlatform : int
    {
        x86 = 8,
        x64 = 9,
        ia64 = 10,
    }

    internal enum ProviderConfig : int
    {
        MklMajorVersion = 32,
        MklMinorVersion = 33,
        MklUpdateVersion = 34,
        Revision = 64,
        Precision = 65,
        Threading = 66,
        Memory = 67,
    }

    internal enum ProviderCapability : int
    {
        LinearAlgebraMajor = 128,
        LinearAlgebraMinor = 129,
        VectorFunctionsMajor = 130,
        VectorFunctionsMinor = 131,
        FourierTransformMajor = 384,
        FourierTransformMinor = 385
    }
}

#endif
