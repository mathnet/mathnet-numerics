// <copyright file="DenseLU.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
//
// Copyright (c) 2009-2010 Math.NET
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

namespace MathNet.Numerics.LinearAlgebra.Complex32.Factorization
{
    using System;
    using Generic;
    using Numerics;
    using Properties;
    using Threading;

    /// <summary>
    /// <para>A class which encapsulates the functionality of an LU factorization.</para>
    /// <para>For a matrix A, the LU factorization is a pair of lower triangular matrix L and
    /// upper triangular matrix U so that A = L*U.</para>
    /// </summary>
    /// <remarks>
    /// The computation of the LU factorization is done at construction time.
    /// </remarks>
    public class DenseLU : LU
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DenseLU"/> class. This object will compute the
        /// LU factorization when the constructor is called and cache it's factorization.
        /// </summary>
        /// <param name="matrix">The matrix to factor.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="matrix"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="matrix"/> is not a square matrix.</exception>
        public DenseLU(DenseMatrix matrix)
        {
            if (matrix == null)
            {
                throw new ArgumentNullException("matrix");
            }

            if (matrix.RowCount != matrix.ColumnCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixSquare);
            }

            // Create an array for the pivot indices.
            Pivots = new int[matrix.RowCount];

            // Create a new matrix for the LU factors, then perform factorization (while overwriting).
            var factors = (DenseMatrix)matrix.Clone();
            Control.LinearAlgebraProvider.LUFactor(factors.Values, factors.RowCount, Pivots);
            Factors = factors;
        }

        /// <summary>
        /// Solves a system of linear equations, <c>AX = B</c>, with A LU factorized.
        /// </summary>
        /// <param name="input">The right hand side <see cref="Matrix{T}"/>, <c>B</c>.</param>
        /// <param name="result">The left hand side <see cref="Matrix{T}"/>, <c>X</c>.</param>
        public override void Solve(Matrix<Complex32> input, Matrix<Complex32> result)
        {
            // Check for proper arguments.
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            // Check for proper dimensions.
            if (result.RowCount != input.RowCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixSameRowDimension);
            }

            if (result.ColumnCount != input.ColumnCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixSameColumnDimension);
            }

            if (input.RowCount != Factors.RowCount)
            {
                throw Matrix.DimensionsDontMatch<ArgumentException>(input, Factors);
            }

            var dinput = input as DenseMatrix;
            if (dinput == null)
            {
                throw new NotSupportedException("Can only do LU factorization for dense matrices at the moment.");
            }

            var dresult = result as DenseMatrix;
            if (dresult == null)
            {
                throw new NotSupportedException("Can only do LU factorization for dense matrices at the moment.");
            }

            // Copy the contents of input to result.
            Array.Copy(dinput.Values, dresult.Values, dinput.Values.Length);

            // LU solve by overwriting result.
            var dfactors = (DenseMatrix)Factors;
            Control.LinearAlgebraProvider.LUSolveFactored(input.ColumnCount, dfactors.Values, dfactors.RowCount, Pivots, dresult.Values);
        }

        /// <summary>
        /// Solves a system of linear equations, <c>Ax = b</c>, with A LU factorized.
        /// </summary>
        /// <param name="input">The right hand side vector, <c>b</c>.</param>
        /// <param name="result">The left hand side <see cref="Matrix{T}"/>, <c>x</c>.</param>
        public override void Solve(Vector<Complex32> input, Vector<Complex32> result)
        {
            // Check for proper arguments.
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            // Check for proper dimensions.
            if (input.Count != result.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength);
            }

            if (input.Count != Factors.RowCount)
            {
                throw Matrix.DimensionsDontMatch<ArgumentException>(input, Factors);
            }

            var dinput = input as DenseVector;
            if (dinput == null)
            {
                throw new NotSupportedException("Can only do LU factorization for dense vectors at the moment.");
            }

            var dresult = result as DenseVector;
            if (dresult == null)
            {
                throw new NotSupportedException("Can only do LU factorization for dense vectors at the moment.");
            }

            // Copy the contents of input to result.
            Array.Copy(dinput.Values, dresult.Values, dinput.Values.Length);

            // LU solve by overwriting result.
            var dfactors = (DenseMatrix)Factors;
            Control.LinearAlgebraProvider.LUSolveFactored(1, dfactors.Values, dfactors.RowCount, Pivots, dresult.Values);
        }

        /// <summary>
        /// Returns the inverse of this matrix. The inverse is calculated using LU decomposition.
        /// </summary>
        /// <returns>The inverse of this matrix.</returns>
        public override Matrix<Complex32> Inverse()
        {
            var result = (DenseMatrix)Factors.Clone();
            Control.LinearAlgebraProvider.LUInverseFactored(result.Values, result.RowCount, Pivots);
            return result;
        }
    }
}
