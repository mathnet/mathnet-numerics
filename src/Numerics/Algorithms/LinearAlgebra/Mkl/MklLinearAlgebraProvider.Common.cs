// <copyright file="MklLinearAlgebraProvider.Common.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
//
// Copyright (c) 2009-2013 Math.NET
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

namespace MathNet.Numerics.Algorithms.LinearAlgebra.Mkl
{
    using System;
    using System.Numerics;
    using System.Security;
    using Properties;
   
    /// <summary>
    /// Intel's Math Kernel Library (MKL) linear algebra provider.
    /// </summary>
    public partial class MklLinearAlgebraProvider : ManagedLinearAlgebraProvider
    {
        /// <summary>
        /// Computes the requested <see cref="Norm"/> of the matrix.
        /// </summary>
        /// <param name="norm">The type of norm to compute.</param>
        /// <param name="rows">The number of rows in the matrix.</param>
        /// <param name="columns">The number of columns in the matrix.</param>
        /// <param name="matrix">The matrix to compute the norm from.</param>
        /// <returns>
        /// The requested <see cref="Norm"/> of the matrix.
        /// </returns>
        [SecuritySafeCritical]
        public override float MatrixNorm(Norm norm, int rows, int columns, float[] matrix)
        {
            if (matrix == null)
            {
                throw new ArgumentNullException("matrix");
            }

            if (rows <= 0)
            {
                throw new ArgumentException(Resources.ArgumentMustBePositive, "rows");
            }

            if (columns <= 0)
            {
                throw new ArgumentException(Resources.ArgumentMustBePositive, "columns");
            }

            if (matrix.Length < rows * columns)
            {
                throw new ArgumentException(string.Format(Resources.ArrayTooSmall, rows * columns), "matrix");
            }

            var work = new float[rows];
            return MatrixNorm(norm, rows, columns, matrix, work);
        }

        /// <summary>
        /// Computes the requested <see cref="Norm"/> of the matrix.
        /// </summary>
        /// <param name="norm">The type of norm to compute.</param>
        /// <param name="rows">The number of rows in the matrix.</param>
        /// <param name="columns">The number of columns in the matrix.</param>
        /// <param name="matrix">The matrix to compute the norm from.</param>
        /// <param name="work">The work array. Only used when <see cref="Norm.InfinityNorm"/>
        /// and needs to be have a length of at least M (number of rows of <paramref name="matrix"/>.</param>
        /// <returns>
        /// The requested <see cref="Norm"/> of the matrix.
        /// </returns>
        [SecuritySafeCritical]
        public override float MatrixNorm(Norm norm, int rows, int columns, float[] matrix, float[] work)
        {
            if (matrix == null)
            {
                throw new ArgumentNullException("matrix");
            }

            if (rows <= 0)
            {
                throw new ArgumentException(Resources.ArgumentMustBePositive, "rows");
            }

            if (columns <= 0)
            {
                throw new ArgumentException(Resources.ArgumentMustBePositive, "columns");
            }

            if (matrix.Length < rows * columns)
            {
                throw new ArgumentException(string.Format(Resources.ArrayTooSmall, rows * columns), "matrix");
            }

            if (work.Length < rows)
            {
                throw new ArgumentException(string.Format(Resources.ArrayTooSmall, rows), "work");
            }

            return SafeNativeMethods.s_matrix_norm((byte)norm, rows, columns, matrix, work);
        }

        /// <summary>
        /// Computes the requested <see cref="Norm"/> of the matrix.
        /// </summary>
        /// <param name="norm">The type of norm to compute.</param>
        /// <param name="rows">The number of rows in the matrix.</param>
        /// <param name="columns">The number of columns in the matrix.</param>
        /// <param name="matrix">The matrix to compute the norm from.</param>
        /// <returns>
        /// The requested <see cref="Norm"/> of the matrix.
        /// </returns>
        [SecuritySafeCritical]
        public override double MatrixNorm(Norm norm, int rows, int columns, double[] matrix)
        {
            if (matrix == null)
            {
                throw new ArgumentNullException("matrix");
            }

            if (rows <= 0)
            {
                throw new ArgumentException(Resources.ArgumentMustBePositive, "rows");
            }

            if (columns <= 0)
            {
                throw new ArgumentException(Resources.ArgumentMustBePositive, "columns");
            }

            if (matrix.Length < rows * columns)
            {
                throw new ArgumentException(string.Format(Resources.ArrayTooSmall, rows * columns), "matrix");
            }

            var work = new double[rows];
            return MatrixNorm(norm, rows, columns, matrix, work);
        }

        /// <summary>
        /// Computes the requested <see cref="Norm"/> of the matrix.
        /// </summary>
        /// <param name="norm">The type of norm to compute.</param>
        /// <param name="rows">The number of rows in the matrix.</param>
        /// <param name="columns">The number of columns in the matrix.</param>
        /// <param name="matrix">The matrix to compute the norm from.</param>
        /// <param name="work">The work array. Only used when <see cref="Norm.InfinityNorm"/>
        /// and needs to be have a length of at least M (number of rows of <paramref name="matrix"/>.</param>
        /// <returns>
        /// The requested <see cref="Norm"/> of the matrix.
        /// </returns>
        [SecuritySafeCritical]
        public override double MatrixNorm(Norm norm, int rows, int columns, double[] matrix, double[] work)
        {
            if (matrix == null)
            {
                throw new ArgumentNullException("matrix");
            }

            if (rows <= 0)
            {
                throw new ArgumentException(Resources.ArgumentMustBePositive, "rows");
            }

            if (columns <= 0)
            {
                throw new ArgumentException(Resources.ArgumentMustBePositive, "columns");
            }

            if (matrix.Length < rows * columns)
            {
                throw new ArgumentException(string.Format(Resources.ArrayTooSmall, rows * columns), "matrix");
            }

            if (work.Length < rows)
            {
                throw new ArgumentException(string.Format(Resources.ArrayTooSmall, rows), "work");
            }

            return SafeNativeMethods.d_matrix_norm((byte)norm, rows, columns, matrix, work);
        }

        /// <summary>
        /// Computes the requested <see cref="Norm"/> of the matrix.
        /// </summary>
        /// <param name="norm">The type of norm to compute.</param>
        /// <param name="rows">The number of rows in the matrix.</param>
        /// <param name="columns">The number of columns in the matrix.</param>
        /// <param name="matrix">The matrix to compute the norm from.</param>
        /// <returns>
        /// The requested <see cref="Norm"/> of the matrix.
        /// </returns>
        [SecuritySafeCritical]
        public override Complex32 MatrixNorm(Norm norm, int rows, int columns, Complex32[] matrix)
        {
            if (matrix == null)
            {
                throw new ArgumentNullException("matrix");
            }

            if (rows <= 0)
            {
                throw new ArgumentException(Resources.ArgumentMustBePositive, "rows");
            }

            if (columns <= 0)
            {
                throw new ArgumentException(Resources.ArgumentMustBePositive, "columns");
            }

            if (matrix.Length < rows * columns)
            {
                throw new ArgumentException(string.Format(Resources.ArrayTooSmall, rows * columns), "matrix");
            }

            var work = new float[rows];
            return MatrixNorm(norm, rows, columns, matrix, work);
        }

        /// <summary>
        /// Computes the requested <see cref="Norm"/> of the matrix.
        /// </summary>
        /// <param name="norm">The type of norm to compute.</param>
        /// <param name="rows">The number of rows in the matrix.</param>
        /// <param name="columns">The number of columns in the matrix.</param>
        /// <param name="matrix">The matrix to compute the norm from.</param>
        /// <param name="work">The work array. Only used when <see cref="Norm.InfinityNorm"/>
        /// and needs to be have a length of at least M (number of rows of <paramref name="matrix"/>.</param>
        /// <returns>
        /// The requested <see cref="Norm"/> of the matrix.
        /// </returns>
        [SecuritySafeCritical]
        public override Complex32 MatrixNorm(Norm norm, int rows, int columns, Complex32[] matrix, float[] work)
        {
            if (matrix == null)
            {
                throw new ArgumentNullException("matrix");
            }

            if (rows <= 0)
            {
                throw new ArgumentException(Resources.ArgumentMustBePositive, "rows");
            }

            if (columns <= 0)
            {
                throw new ArgumentException(Resources.ArgumentMustBePositive, "columns");
            }

            if (matrix.Length < rows * columns)
            {
                throw new ArgumentException(string.Format(Resources.ArrayTooSmall, rows * columns), "matrix");
            }

            if (work.Length < rows)
            {
                throw new ArgumentException(string.Format(Resources.ArrayTooSmall, rows), "work");
            }

            return SafeNativeMethods.c_matrix_norm((byte)norm, rows, columns, matrix, work);
        }

        /// <summary>
        /// Computes the requested <see cref="Norm"/> of the matrix.
        /// </summary>
        /// <param name="norm">The type of norm to compute.</param>
        /// <param name="rows">The number of rows in the matrix.</param>
        /// <param name="columns">The number of columns in the matrix.</param>
        /// <param name="matrix">The matrix to compute the norm from.</param>
        /// <returns>
        /// The requested <see cref="Norm"/> of the matrix.
        /// </returns>
        [SecuritySafeCritical]
        public override Complex MatrixNorm(Norm norm, int rows, int columns, Complex[] matrix)
        {
            if (matrix == null)
            {
                throw new ArgumentNullException("matrix");
            }

            if (rows <= 0)
            {
                throw new ArgumentException(Resources.ArgumentMustBePositive, "rows");
            }

            if (columns <= 0)
            {
                throw new ArgumentException(Resources.ArgumentMustBePositive, "columns");
            }

            if (matrix.Length < rows * columns)
            {
                throw new ArgumentException(string.Format(Resources.ArrayTooSmall, rows * columns), "matrix");
            }

            var work = new double[rows];
            return MatrixNorm(norm, rows, columns, matrix, work);
        }

        /// <summary>
        /// Computes the requested <see cref="Norm"/> of the matrix.
        /// </summary>
        /// <param name="norm">The type of norm to compute.</param>
        /// <param name="rows">The number of rows in the matrix.</param>
        /// <param name="columns">The number of columns in the matrix.</param>
        /// <param name="matrix">The matrix to compute the norm from.</param>
        /// <param name="work">The work array. Only used when <see cref="Norm.InfinityNorm"/>
        /// and needs to be have a length of at least M (number of rows of <paramref name="matrix"/>.</param>
        /// <returns>
        /// The requested <see cref="Norm"/> of the matrix.
        /// </returns>
        [SecuritySafeCritical]
        public override Complex MatrixNorm(Norm norm, int rows, int columns, Complex[] matrix, double[] work)
        {
            if (matrix == null)
            {
                throw new ArgumentNullException("matrix");
            }

            if (rows <= 0)
            {
                throw new ArgumentException(Resources.ArgumentMustBePositive, "rows");
            }

            if (columns <= 0)
            {
                throw new ArgumentException(Resources.ArgumentMustBePositive, "columns");
            }

            if (matrix.Length < rows * columns)
            {
                throw new ArgumentException(string.Format(Resources.ArrayTooSmall, rows * columns), "matrix");
            }

            if (work.Length < rows)
            {
                throw new ArgumentException(string.Format(Resources.ArrayTooSmall, rows), "work");
            }

            return SafeNativeMethods.z_matrix_norm((byte)norm, rows, columns, matrix, work);
        }
    }
}
