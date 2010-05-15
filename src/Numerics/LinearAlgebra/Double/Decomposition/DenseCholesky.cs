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

namespace MathNet.Numerics.LinearAlgebra.Double.Decomposition
{
    using System;
    using Properties;

    /// <summary>
    /// <para>A class which encapsulates the functionality of a Cholesky decomposition for dense matrices.</para>
    /// <para>For a symmetric, positive definite matrix A, the Cholesky decomposition
    /// is an lower triangular matrix L so that A = L*L'.</para>
    /// </summary>
    /// <remarks>
    /// The computation of the Cholesky decomposition is done at construction time. If the matrix is not symmetric
    /// or positive definite, the constructor will throw an exception.
    /// </remarks>
    public class DenseCholesky : Cholesky
    {
        /// <summary>
        /// Stores the Cholesky factor for the decomposition.
        /// </summary>
        private DenseMatrix _factor;

        /// <summary>
        /// Initializes a new instance of the <see cref="DenseCholesky"/> class. This object will compute the
        /// Cholesky decomposition when the constructor is called and cache it's factorization.
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
            _factor = (DenseMatrix)matrix.Clone();
            Control.LinearAlgebraProvider.CholeskyFactor(_factor.Data, _factor.RowCount);
        }

        /// <summary>
        /// Returns the lower triangular form of the Cholesky matrix.
        /// </summary>
        public override Matrix Factor
        {
            get { return _factor; }
        }

        /// <summary>
        /// The determinant of the matrix for which the Cholesky matrix was computed.
        /// </summary>
        public override double Determinant
        {
            get
            {
                double det = 1.0;
                for (int j = 0; j < _factor.RowCount; j++)
                {
                    det *= (_factor[j, j] * _factor[j, j]);
                }
                return det;
            }
        }

        /// <summary>
        /// The log determinant of the matrix for which the Cholesky matrix was computed.
        /// </summary>
        public override double DeterminantLn
        {
            get
            {
                double det = 0.0;
                for (int j = 0; j < _factor.RowCount; j++)
                {
                    det += (2.0 * Math.Log(_factor[j, j]));
                }
                return det;
            }
        }
    }
}
