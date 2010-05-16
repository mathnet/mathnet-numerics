// <copyright file="DenseCholesky.cs" company="Math.NET">
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

namespace MathNet.Numerics.LinearAlgebra.Double.Factorization
{
    using System;
    using Properties;

    /// <summary>
    /// <para>A class which encapsulates the functionality of a Cholesky factorization for dense matrices.</para>
    /// <para>For a symmetric, positive definite matrix A, the Cholesky factorization
    /// is an lower triangular matrix L so that A = L*L'.</para>
    /// </summary>
    /// <remarks>
    /// The computation of the Cholesky factorization is done at construction time. If the matrix is not symmetric
    /// or positive definite, the constructor will throw an exception.
    /// </remarks>
    public class DenseCholesky : Cholesky
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DenseCholesky"/> class. This object will compute the
        /// Cholesky factorization when the constructor is called and cache it's factorization.
        /// </summary>
        /// <param name="matrix">The matrix to factor.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="matrix"/> is <b>null</b>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="matrix"/> is not a square matrix.</exception>
        /// <exception cref="ArgumentException">If <paramref name="matrix"/> is not positive definite.</exception>
        public DenseCholesky(DenseMatrix matrix)
        {
            if (matrix == null)
            {
                throw new ArgumentNullException("matrix");
            }

            if (matrix.RowCount != matrix.ColumnCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixSquare);
            }

            // Create a new matrix for the Cholesky factor, then perform factorization (while overwriting).
            var factor = (DenseMatrix)matrix.Clone();
            Control.LinearAlgebraProvider.CholeskyFactor(factor.Data, factor.RowCount);
            mFactor = factor;
        }

        /// <summary>
        /// Solves a system of linear equations, <b>AX = B</b>, with A Cholesky factorized.
        /// </summary>
        /// <param name="input">The right hand side <see cref="Matrix"/>, <b>B</b>.</param>
        /// <returns>The left hand side <see cref="Matrix"/>, <b>X</b>.</returns>
        public override Matrix Solve(Matrix input)
        {
            // Check for proper arguments.
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            var X = new DenseMatrix(input.RowCount, input.ColumnCount);
            Solve(input, X);
            return X;
        }

        /// <summary>
        /// Solves a system of linear equations, <b>AX = B</b>, with A Cholesky factorized.
        /// </summary>
        /// <param name="input">The right hand side <see cref="Matrix"/>, <b>B</b>.</param>
        /// <param name="result">The left hand side <see cref="Matrix"/>, <b>X</b>.</param>
        public override void Solve(Matrix input, Matrix result)
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

            if (input.RowCount != mFactor.RowCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixDimensions);
            }

            var dinput = input as DenseMatrix;
            if (dinput == null)
            {
                throw new NotImplementedException("Can only do Cholesky factorization for dense matrices at the moment.");
            }

            var dresult = input as DenseMatrix;
            if (dresult == null)
            {
                throw new NotImplementedException("Can only do Cholesky factorization for dense matrices at the moment.");
            }

            // Copy the contents of input to result.
            Buffer.BlockCopy(dinput.Data, 0, dresult.Data, 0, dinput.Data.Length * Constants.SizeOfDouble);

            // Cholesky solve by overwriting result.
            var dfactor = mFactor as DenseMatrix;
            Control.LinearAlgebraProvider.CholeskySolveFactored(dfactor.Data, dfactor.RowCount, dresult.Data, dresult.RowCount, dresult.ColumnCount);
        }

        /// <summary>
        /// Solves a system of linear equations, <b>Ax = b</b>, with A Cholesky factorized.
        /// </summary>
        /// <param name="input">The right hand side vector, <b>b</b>.</param>
        /// <returns>The left hand side <see cref="DenseVector"/>, <b>x</b>.</returns>
        public override Vector Solve(Vector input)
        {
            // Check for proper arguments.
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            var x = new DenseVector(input.Count);
            Solve(input, x);
            return x;
        }

        /// <summary>
        /// Solves a system of linear equations, <b>Ax = b</b>, with A Cholesky factorized.
        /// </summary>
        /// <param name="input">The right hand side vector, <b>b</b>.</param>
        /// <param name="result">The left hand side <see cref="Matrix"/>, <b>x</b>.</param>
        public override void Solve(Vector input, Vector result)
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

            if (input.Count != mFactor.RowCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixDimensions);
            }

            var dinput = input as DenseVector;
            if (dinput == null)
            {
                throw new NotImplementedException("Can only do Cholesky factorization for dense vectors at the moment.");
            }

            var dresult = input as DenseVector;
            if (dresult == null)
            {
                throw new NotImplementedException("Can only do Cholesky factorization for dense vectors at the moment.");
            }

            // Copy the contents of input to result.
            Buffer.BlockCopy(dinput.Data, 0, dresult.Data, 0, dinput.Data.Length * Constants.SizeOfDouble);

            // Cholesky solve by overwriting result.
            var dfactor = mFactor as DenseMatrix;
            Control.LinearAlgebraProvider.CholeskySolveFactored(dfactor.Data, dfactor.RowCount, dresult.Data, dresult.Count, 1);
        }
    }
}
