﻿// <copyright file="DenseQR.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
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

using System;
using MathNet.Numerics.LinearAlgebra.Factorization;
using MathNet.Numerics.Properties;

namespace MathNet.Numerics.LinearAlgebra.Complex.Factorization
{

#if NOSYSNUMERICS
    using Numerics;
#else
    using System.Numerics;
#endif

    /// <summary>
    /// <para>A class which encapsulates the functionality of the QR decomposition.</para>
    /// <para>Any real square matrix A may be decomposed as A = QR where Q is an orthogonal matrix 
    /// (its columns are orthogonal unit vectors meaning QTQ = I) and R is an upper triangular matrix 
    /// (also called right triangular matrix).</para>
    /// </summary>
    /// <remarks>
    /// The computation of the QR decomposition is done at construction time by Householder transformation.
    /// </remarks>
    internal sealed class DenseQR : QR
    {
        /// <summary>
        ///  Gets or sets Tau vector. Contains additional information on Q - used for native solver.
        /// </summary>
        Complex[] Tau { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DenseQR"/> class. This object will compute the
        /// QR factorization when the constructor is called and cache it's factorization.
        /// </summary>
        /// <param name="matrix">The matrix to factor.</param>
        /// <param name="method">The type of QR factorization to perform.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="matrix"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="matrix"/> row count is less then column count</exception>
        public static DenseQR Create(DenseMatrix matrix, QRMethod method = QRMethod.Full)
        {
            if (matrix.RowCount < matrix.ColumnCount)
            {
                throw Matrix.DimensionsDontMatch<ArgumentException>(matrix);
            }

            var tau = new Complex[Math.Min(matrix.RowCount, matrix.ColumnCount)];
            Matrix<Complex> q;
            Matrix<Complex> r;

            if (method == QRMethod.Full)
            {
                r = matrix.Clone();
                q = new DenseMatrix(matrix.RowCount);
                Control.LinearAlgebraProvider.QRFactor(((DenseMatrix) r).Values, matrix.RowCount, matrix.ColumnCount, ((DenseMatrix) q).Values, tau);
            }
            else
            {
                q = matrix.Clone();
                r = new DenseMatrix(matrix.ColumnCount);
                Control.LinearAlgebraProvider.ThinQRFactor(((DenseMatrix) q).Values, matrix.RowCount, matrix.ColumnCount, ((DenseMatrix) r).Values, tau);
            }

            return new DenseQR(q, r, method, tau);
        }

        DenseQR(Matrix<Complex> q, Matrix<Complex> rFull, QRMethod method, Complex[] tau)
            : base(q, rFull, method)
        {
            Tau = tau;
        }

        /// <summary>
        /// Solves a system of linear equations, <b>AX = B</b>, with A QR factorized.
        /// </summary>
        /// <param name="input">The right hand side <see cref="Matrix{T}"/>, <b>B</b>.</param>
        /// <param name="result">The left hand side <see cref="Matrix{T}"/>, <b>X</b>.</param>
        public override void Solve(Matrix<Complex> input, Matrix<Complex> result)
        {
            // The solution X should have the same number of columns as B
            if (input.ColumnCount != result.ColumnCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixSameColumnDimension);
            }

            // The dimension compatibility conditions for X = A\B require the two matrices A and B to have the same number of rows
            if (Q.RowCount != input.RowCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixSameRowDimension);
            }

            // The solution X row dimension is equal to the column dimension of A
            if (FullR.ColumnCount != result.RowCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixSameColumnDimension);
            }

            var dinput = input as DenseMatrix;
            if (dinput == null)
            {
                throw new NotSupportedException("Can only do QR factorization for dense matrices at the moment.");
            }

            var dresult = result as DenseMatrix;
            if (dresult == null)
            {
                throw new NotSupportedException("Can only do QR factorization for dense matrices at the moment.");
            }

            Control.LinearAlgebraProvider.QRSolveFactored(((DenseMatrix) Q).Values, ((DenseMatrix) FullR).Values, Q.RowCount, FullR.ColumnCount, Tau, dinput.Values, input.ColumnCount, dresult.Values, Method);
        }

        /// <summary>
        /// Solves a system of linear equations, <b>Ax = b</b>, with A QR factorized.
        /// </summary>
        /// <param name="input">The right hand side vector, <b>b</b>.</param>
        /// <param name="result">The left hand side <see cref="Matrix{T}"/>, <b>x</b>.</param>
        public override void Solve(Vector<Complex> input, Vector<Complex> result)
        {
            // Ax=b where A is an m x n matrix
            // Check that b is a column vector with m entries
            if (Q.RowCount != input.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength);
            }

            // Check that x is a column vector with n entries
            if (FullR.ColumnCount != result.Count)
            {
                throw Matrix.DimensionsDontMatch<ArgumentException>(FullR, result);
            }

            var dinput = input as DenseVector;
            if (dinput == null)
            {
                throw new NotSupportedException("Can only do QR factorization for dense vectors at the moment.");
            }

            var dresult = result as DenseVector;
            if (dresult == null)
            {
                throw new NotSupportedException("Can only do QR factorization for dense vectors at the moment.");
            }

            Control.LinearAlgebraProvider.QRSolveFactored(((DenseMatrix) Q).Values, ((DenseMatrix) FullR).Values, Q.RowCount, FullR.ColumnCount, Tau, dinput.Values, 1, dresult.Values, Method);
        }
    }
}
