// <copyright file="Svd.cs" company="Math.NET">
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

namespace MathNet.Numerics.LinearAlgebra.Generic.Factorization
{
    using System;
    using System.Numerics;
    using Generic;
    using Numerics;
    using Properties;

    /// <summary>
    /// <para>A class which encapsulates the functionality of the singular value decomposition (SVD).</para>
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
    /// <typeparam name="T">Supported data types are double, single, <see cref="Complex"/>, and <see cref="Complex32"/>.</typeparam>
    public abstract class Svd<T> : ISolver<T>
    where T : struct, IEquatable<T>, IFormattable
    {
        /// <summary>
        /// Gets or sets a value indicating whether to compute U and VT matrices during SVD factorization or not
        /// </summary>
        protected bool ComputeVectors
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the singular values (Σ) of matrix in ascending value.
        /// </summary>
        protected Vector<T> VectorS
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets left singular vectors (U - m-by-m unitary matrix)
        /// </summary>
        protected Matrix<T> MatrixU
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets transpose right singular vectors (transpose of V, an n-by-n unitary matrix
        /// </summary>
        protected Matrix<T> MatrixVT
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the effective numerical matrix rank.
        /// </summary>
        /// <value>The number of non-negligible singular values.</value>
        public abstract int Rank
        {
            get;
        }

        /// <summary>
        /// Internal method which routes the call to perform the singular value decomposition to the appropriate class.
        /// </summary>
        /// <param name="matrix">The matrix to factor.</param>
        /// <param name="computeVectors">Compute the singular U and VT vectors or not.</param>
        /// <returns>An SVD object.</returns>
        internal static Svd<T> Create(Matrix<T> matrix, bool computeVectors)
        {
            if (typeof(T) == typeof(double))
            {
                var dense = matrix as LinearAlgebra.Double.DenseMatrix;
                if (dense != null)
                {
                    return new LinearAlgebra.Double.Factorization.DenseSvd(dense, computeVectors) as Svd<T>;
                }

                return new LinearAlgebra.Double.Factorization.UserSvd(matrix as Matrix<double>, computeVectors) as Svd<T>;
            }

            if (typeof(T) == typeof(float))
            {
                var dense = matrix as LinearAlgebra.Single.DenseMatrix;
                if (dense != null)
                {
                    return new LinearAlgebra.Single.Factorization.DenseSvd(dense, computeVectors) as Svd<T>;
                }

                return new LinearAlgebra.Single.Factorization.UserSvd(matrix as Matrix<float>, computeVectors) as Svd<T>;
            }

            if (typeof(T) == typeof(Complex))
            {
                var dense = matrix as LinearAlgebra.Complex.DenseMatrix;
                if (dense != null)
                {
                    return new LinearAlgebra.Complex.Factorization.DenseSvd(dense, computeVectors) as Svd<T>;
                }

                return new LinearAlgebra.Complex.Factorization.UserSvd(matrix as Matrix<Complex>, computeVectors) as Svd<T>;
            }

            if (typeof(T) == typeof(Complex32))
            {
                var dense = matrix as LinearAlgebra.Complex32.DenseMatrix;
                if (dense != null)
                {
                    return new LinearAlgebra.Complex32.Factorization.DenseSvd(dense, computeVectors) as Svd<T>;
                }

                return new LinearAlgebra.Complex32.Factorization.UserSvd(matrix as Matrix<Complex32>, computeVectors) as Svd<T>;
            }

            throw new NotSupportedException();
        }

        /// <summary>
        /// Gets the two norm of the <see cref="Matrix{T}"/>.
        /// </summary>
        /// <returns>The 2-norm of the <see cref="Matrix{T}"/>.</returns>
        public abstract T Norm2
        {
            get;
        }

        /// <summary>
        /// Gets the condition number <b>max(S) / min(S)</b>
        /// </summary>
        /// <returns>The condition number.</returns>
        public abstract T ConditionNumber
        {
            get;
        }

        /// <summary>
        /// Gets the determinant of the square matrix for which the SVD was computed.
        /// </summary>
        public abstract T Determinant
        {
            get;
        }

        /// <summary>Returns the left singular vectors as a <see cref="Matrix{T}"/>.</summary>
        /// <returns>The left singular vectors. The matrix will be <c>null</c>, if <b>computeVectors</b> in the constructor is set to <c>false</c>.</returns>
        public Matrix<T> U()
        {
            return ComputeVectors ? MatrixU.Clone() : null;
        }

        /// <summary>Returns the right singular vectors as a <see cref="Matrix{T}"/>.</summary>
        /// <returns>The right singular vectors. The matrix will be <c>null</c>, if <b>computeVectors</b> in the constructor is set to <c>false</c>.</returns>
        /// <remarks>This is the transpose of the V matrix.</remarks>
        public Matrix<T> VT()
        {
            return ComputeVectors ? MatrixVT.Clone() : null;
        }

        /// <summary>Returns the singular values as a diagonal <see cref="Matrix{T}"/>.</summary>
        /// <returns>The singular values as a diagonal <see cref="Matrix{T}"/>.</returns>        
        public Matrix<T> W()
        {
            var rows = MatrixU.RowCount;
            var columns = MatrixVT.ColumnCount;
            var result = MatrixU.CreateMatrix(rows, columns);
            for (var i = 0; i < rows; i++)
            {
                for (var j = 0; j < columns; j++)
                {
                    if (i == j)
                    {
                        result.At(i, i, VectorS[i]);
                    }
                }
            }

            return result;
        }

        /// <summary>Returns the singular values as a <see cref="Vector{T}"/>.</summary>
        /// <returns>the singular values as a <see cref="Vector{T}"/>.</returns>
        public Vector<T> S()
        {
            return VectorS.Clone();
        }

        /// <summary>
        /// Solves a system of linear equations, <b>AX = B</b>, with A SVD factorized.
        /// </summary>
        /// <param name="input">The right hand side <see cref="Matrix{T}"/>, <b>B</b>.</param>
        /// <returns>The left hand side <see cref="Matrix{T}"/>, <b>X</b>.</returns>
        public virtual Matrix<T> Solve(Matrix<T> input)
        {
            // Check for proper arguments.
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            if (!ComputeVectors)
            {
                throw new InvalidOperationException(Resources.SingularVectorsNotComputed);
            }

            var result = MatrixU.CreateMatrix(MatrixVT.ColumnCount, input.ColumnCount);
            Solve(input, result);
            return result;
        }

        /// <summary>
        /// Solves a system of linear equations, <b>AX = B</b>, with A SVD factorized.
        /// </summary>
        /// <param name="input">The right hand side <see cref="Matrix{T}"/>, <b>B</b>.</param>
        /// <param name="result">The left hand side <see cref="Matrix{T}"/>, <b>X</b>.</param>
        public abstract void Solve(Matrix<T> input, Matrix<T> result);

        /// <summary>
        /// Solves a system of linear equations, <b>Ax = b</b>, with A SVD factorized.
        /// </summary>
        /// <param name="input">The right hand side vector, <b>b</b>.</param>
        /// <returns>The left hand side <see cref="Vector{T}"/>, <b>x</b>.</returns>
        public virtual Vector<T> Solve(Vector<T> input)
        {
            // Check for proper arguments.
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            if (!ComputeVectors)
            {
                throw new InvalidOperationException(Resources.SingularVectorsNotComputed);
            }

            var x = MatrixU.CreateVector(MatrixVT.ColumnCount);
            Solve(input, x);
            return x;
        }

        /// <summary>
        /// Solves a system of linear equations, <b>Ax = b</b>, with A SVD factorized.
        /// </summary>
        /// <param name="input">The right hand side vector, <b>b</b>.</param>
        /// <param name="result">The left hand side <see cref="Matrix{T}"/>, <b>x</b>.</param>
        public abstract void Solve(Vector<T> input, Vector<T> result);
    }
}
