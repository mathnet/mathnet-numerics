﻿// <copyright file="DenseSvd.cs" company="Math.NET">
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
using MathNet.Numerics.Properties;

namespace MathNet.Numerics.LinearAlgebra.Single.Factorization
{
    /// <summary>
    /// <para>A class which encapsulates the functionality of the singular value decomposition (SVD) for <see cref="DenseMatrix"/>.</para>
    /// <para>Suppose M is an m-by-n matrix whose entries are real numbers. 
    /// Then there exists a factorization of the form M = UΣVT where:
    /// - U is an m-by-m unitary matrix;
    /// - Σ is m-by-n diagonal matrix with nonnegative real numbers on the diagonal;
    /// - VT denotes transpose of V, an n-by-n unitary matrix; 
    /// Such a factorization is called a singular-value decomposition of M. A common convention is to order the diagonal 
    /// entries Σ(i,i) in descending order. In this case, the diagonal matrix Σ is uniquely determined 
    /// by M (though the matrices U and V are not). The diagonal entries of Σ are known as the singular values of M.</para>
    /// </summary>
    /// <remarks>
    /// The computation of the singular value decomposition is done at construction time.
    /// </remarks>
    internal sealed class DenseSvd : Svd
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DenseSvd"/> class. This object will compute the
        /// the singular value decomposition when the constructor is called and cache it's decomposition.
        /// </summary>
        /// <param name="matrix">The matrix to factor.</param>
        /// <param name="computeVectors">Compute the singular U and VT vectors or not.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="matrix"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">If SVD algorithm failed to converge with matrix <paramref name="matrix"/>.</exception>
        public static DenseSvd Create(DenseMatrix matrix, bool computeVectors)
        {
            var nm = Math.Min(matrix.RowCount, matrix.ColumnCount);
            var s = new DenseVector(nm);
            var u = new DenseMatrix(matrix.RowCount);
            var vt = new DenseMatrix(matrix.ColumnCount);
            Control.LinearAlgebraProvider.SingularValueDecomposition(computeVectors, ((DenseMatrix) matrix.Clone()).Values, matrix.RowCount, matrix.ColumnCount, s.Values, u.Values, vt.Values);

            return new DenseSvd(s, u, vt, computeVectors);
        }

        DenseSvd(Vector<float> s, Matrix<float> u, Matrix<float> vt, bool vectorsComputed)
            : base(s, u, vt, vectorsComputed)
        {
        }

        /// <summary>
        /// Solves a system of linear equations, <b>AX = B</b>, with A SVD factorized.
        /// </summary>
        /// <param name="input">The right hand side <see cref="Matrix{T}"/>, <b>B</b>.</param>
        /// <param name="result">The left hand side <see cref="Matrix{T}"/>, <b>X</b>.</param>
        public override void Solve(Matrix<float> input, Matrix<float> result)
        {
            if (!VectorsComputed)
            {
                throw new InvalidOperationException(Resources.SingularVectorsNotComputed);
            }

            // The solution X should have the same number of columns as B
            if (input.ColumnCount != result.ColumnCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixSameColumnDimension);
            }

            // The dimension compatibility conditions for X = A\B require the two matrices A and B to have the same number of rows
            if (U.RowCount != input.RowCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixSameRowDimension);
            }

            // The solution X row dimension is equal to the column dimension of A
            if (VT.ColumnCount != result.RowCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixSameColumnDimension);
            }

            var dinput = input as DenseMatrix;
            if (dinput == null)
            {
                throw new NotSupportedException("Can only do SVD factorization for dense matrices at the moment.");
            }

            var dresult = result as DenseMatrix;
            if (dresult == null)
            {
                throw new NotSupportedException("Can only do SVD factorization for dense matrices at the moment.");
            }

            Control.LinearAlgebraProvider.SvdSolveFactored(U.RowCount, VT.ColumnCount, ((DenseVector) S).Values, ((DenseMatrix) U).Values, ((DenseMatrix) VT).Values, dinput.Values, input.ColumnCount, dresult.Values);
        }

        /// <summary>
        /// Solves a system of linear equations, <b>Ax = b</b>, with A SVD factorized.
        /// </summary>
        /// <param name="input">The right hand side vector, <b>b</b>.</param>
        /// <param name="result">The left hand side <see cref="Matrix{T}"/>, <b>x</b>.</param>
        public override void Solve(Vector<float> input, Vector<float> result)
        {
            if (!VectorsComputed)
            {
                throw new InvalidOperationException(Resources.SingularVectorsNotComputed);
            }

            // Ax=b where A is an m x n matrix
            // Check that b is a column vector with m entries
            if (U.RowCount != input.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength);
            }

            // Check that x is a column vector with n entries
            if (VT.ColumnCount != result.Count)
            {
                throw Matrix.DimensionsDontMatch<ArgumentException>(VT, result);
            }

            var dinput = input as DenseVector;
            if (dinput == null)
            {
                throw new NotSupportedException("Can only do SVD factorization for dense vectors at the moment.");
            }

            var dresult = result as DenseVector;
            if (dresult == null)
            {
                throw new NotSupportedException("Can only do SVD factorization for dense vectors at the moment.");
            }

            Control.LinearAlgebraProvider.SvdSolveFactored(U.RowCount, VT.ColumnCount, ((DenseVector) S).Values, ((DenseMatrix) U).Values, ((DenseMatrix) VT).Values, dinput.Values, 1, dresult.Values);
        }
    }
}
