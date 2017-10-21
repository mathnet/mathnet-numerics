// <copyright file="Vector.Conversions.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2017 Math.NET
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
using System.Runtime.CompilerServices;

namespace MathNet.Numerics.LinearAlgebra
{
#if NOSYSNUMERICS
    using Complex64 = Numerics.Complex;
#else
    using Complex64 = System.Numerics.Complex;
#endif
    public abstract partial class Vector<T>
    {
        /// <summary>
        /// Implicitly converts a matrix to double precision complex numbers.
        /// </summary>
        public static implicit operator Vector<Complex64>(Vector<T> vector)
        {
            var vectorDouble = vector as Vector<double>;
            if (vectorDouble != null)
                return vectorDouble.ToComplex();

            var vectorFloat = vector as Vector<float>;
            if (vectorFloat != null)
                return vectorFloat.ToComplex();

            var vectorComplex32 = vector as Vector<Numerics.Complex32>;
            if (vectorComplex32 != null)
                return vectorComplex32.ToComplex();

            return vector.Map(x => new Complex64(Convert.ToDouble(x), 0), Zeros.AllowSkip);
        }
    }
}
