// <copyright file="QR.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
// Copyright (c) 2009-2010 Math.NET
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
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
    /// <para>A class which encapsulates the functionality of the QR decomposition.</para>
    /// <para>Any real square matrix A (m x n) may be decomposed as A = QR where Q is an orthogonal matrix (m x m)
    /// (its columns are orthogonal unit vectors meaning QTQ = I) and R (m x n) is an upper triangular matrix 
    /// (also called right triangular matrix).</para>
    /// </summary>
    /// <remarks>
    /// The computation of the QR decomposition is done at construction time by Householder transformation.
    /// </remarks>
    public abstract class QR : ISolver
    {
        /// <summary>
        /// Gets or sets orthogonal Q matrix
        /// </summary>
        protected Matrix MatrixQ
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets upper triangular factor R
        /// </summary>
        protected Matrix MatrixR
        {
            get;
            set;
        }

        /// <summary>
        /// Internal method which routes the call to perform the QR factorization to the appropriate class.
        /// </summary>
        /// <param name="matrix">The matrix to factor.</param>
        /// <returns>A QR factorization object.</returns>
        internal static QR Create(Matrix matrix)
        {
            var dense = matrix as DenseMatrix;
            if (dense != null)
            {
                return new DenseQR(dense);
            }

            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets orthogonal Q matrix
        /// </summary>
        public virtual Matrix Q 
        {
            get
            {
                return MatrixQ.Clone();
            }
        }

        /// <summary>
        /// Gets the upper triangular factor R.
        /// </summary>
        public virtual Matrix R
        {
            get
            {
                return MatrixR.UpperTriangle();
            }
        }

        /// <summary>
        /// Gets the determinant of the matrix for which the QR matrix was computed.
        /// </summary>
        public virtual double Determinant
        {
            get
            {
                if (MatrixR.RowCount != MatrixR.ColumnCount)
                {
                    throw new ArgumentException(Resources.ArgumentMatrixSquare);
                }

                var det = 1.0;
                for (var i = 0; i < MatrixR.ColumnCount; i++)
                {
                    det *= MatrixR.At(i, i);
                    if (Math.Abs(MatrixR.At(i, i)).AlmostEqualInDecimalPlaces(0.0, 15))
                    {
                        return 0;
                    }
                }

                return Math.Abs(det);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the matrix is full rank or not.
        /// </summary>
        /// <value><c>true</c> if the matrix is full rank; otherwise <c>false</c>.</value>
        public virtual bool IsFullRank
        {
            get
            {
                for (var i = 0; i < MatrixR.ColumnCount; i++)
                {
                    if (Math.Abs(MatrixR.At(i, i)).AlmostEqualInDecimalPlaces(0.0, 15))
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        /// <summary>
        /// Solves a system of linear equations, <b>AX = B</b>, with A QR factorized.
        /// </summary>
        /// <param name="input">The right hand side <see cref="Matrix"/>, <b>B</b>.</param>
        /// <returns>The left hand side <see cref="Matrix"/>, <b>X</b>.</returns>
        public virtual Matrix Solve(Matrix input)
        {
            // Check for proper arguments.
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            var matrixX = input.CreateMatrix(MatrixR.ColumnCount, input.ColumnCount);
            Solve(input, matrixX);
            return matrixX;
        }

        /// <summary>
        /// Solves a system of linear equations, <b>AX = B</b>, with A QR factorized.
        /// </summary>
        /// <param name="input">The right hand side <see cref="Matrix"/>, <b>B</b>.</param>
        /// <param name="result">The left hand side <see cref="Matrix"/>, <b>X</b>.</param>
        public abstract void Solve(Matrix input, Matrix result);

        /// <summary>
        /// Solves a system of linear equations, <b>Ax = b</b>, with A QR factorized.
        /// </summary>
        /// <param name="input">The right hand side vector, <b>b</b>.</param>
        /// <returns>The left hand side <see cref="Vector"/>, <b>x</b>.</returns>
        public virtual Vector Solve(Vector input)
        {
            // Check for proper arguments.
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            var x = input.CreateVector(MatrixR.ColumnCount);
            Solve(input, x);
            return x;
        }

        /// <summary>
        /// Solves a system of linear equations, <b>Ax = b</b>, with A QR factorized.
        /// </summary>
        /// <param name="input">The right hand side vector, <b>b</b>.</param>
        /// <param name="result">The left hand side <see cref="Matrix"/>, <b>x</b>.</param>
        public abstract void Solve(Vector input, Vector result);
    }
}
