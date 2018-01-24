// <copyright file="MatrixExtensions.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2015 Math.NET
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

namespace MathNet.Numerics.LinearAlgebra
{
    using Complex64 = System.Numerics.Complex;

    public static class MatrixExtensions
    {
        /// <summary>
        /// Converts a matrix to single precision.
        /// </summary>
        public static Matrix<float> ToSingle(this Matrix<double> matrix)
        {
            return matrix.Map(x => (float)x, Zeros.AllowSkip);
        }

        /// <summary>
        /// Converts a matrix to double precision.
        /// </summary>
        public static Matrix<double> ToDouble(this Matrix<float> matrix)
        {
            return matrix.Map(x => (double)x, Zeros.AllowSkip);
        }

        /// <summary>
        /// Converts a matrix to single precision complex numbers.
        /// </summary>
        public static Matrix<Numerics.Complex32> ToComplex32(this Matrix<Complex64> matrix)
        {
            return matrix.Map(x => new Numerics.Complex32((float)x.Real, (float)x.Imaginary), Zeros.AllowSkip);
        }

        /// <summary>
        /// Converts a matrix to double precision complex numbers.
        /// </summary>
        public static Matrix<Complex64> ToComplex(this Matrix<Numerics.Complex32> matrix)
        {
            return matrix.Map(x => new Complex64(x.Real, x.Imaginary), Zeros.AllowSkip);
        }

        /// <summary>
        /// Gets a single precision complex matrix with the real parts from the given matrix.
        /// </summary>
        public static Matrix<Numerics.Complex32> ToComplex32(this Matrix<float> matrix)
        {
            return matrix.Map(x => new Numerics.Complex32(x, 0f), Zeros.AllowSkip);
        }

        /// <summary>
        /// Gets a double precision complex matrix with the real parts from the given matrix.
        /// </summary>
        public static Matrix<Complex64> ToComplex(this Matrix<double> matrix)
        {
            return matrix.Map(x => new Complex64(x, 0d), Zeros.AllowSkip);
        }

        /// <summary>
        /// Gets a real matrix representing the real parts of a complex matrix.
        /// </summary>
        public static Matrix<double> Real(this Matrix<Complex64> matrix)
        {
            return matrix.Map(x => x.Real, Zeros.AllowSkip);
        }

        /// <summary>
        /// Gets a real matrix representing the real parts of a complex matrix.
        /// </summary>
        public static Matrix<float> Real(this Matrix<Numerics.Complex32> matrix)
        {
            return matrix.Map(x => x.Real, Zeros.AllowSkip);
        }

        /// <summary>
        /// Gets a real matrix representing the imaginary parts of a complex matrix.
        /// </summary>
        public static Matrix<double> Imaginary(this Matrix<Complex64> matrix)
        {
            return matrix.Map(x => x.Imaginary, Zeros.AllowSkip);
        }

        /// <summary>
        /// Gets a real matrix representing the imaginary parts of a complex matrix.
        /// </summary>
        public static Matrix<float> Imaginary(this Matrix<Numerics.Complex32> matrix)
        {
            return matrix.Map(x => x.Imaginary, Zeros.AllowSkip);
        }
    }
}
