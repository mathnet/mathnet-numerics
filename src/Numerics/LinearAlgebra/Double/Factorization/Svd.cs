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

namespace MathNet.Numerics.LinearAlgebra.Double.Factorization
{
    using System;
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
    public abstract class Svd : ISolver
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
        protected Vector VectorS
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets left singular vectors (U - m-by-m unitary matrix)
        /// </summary>
        protected Matrix MatrixU
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets transpose right singular vectors (transpose of V, an n-by-n unitary matrix
        /// </summary>
        protected Matrix MatrixVT
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the effective numerical matrix rank.
        /// </summary>
        /// <value>The number of non-negligible singular values.</value>
        public virtual int Rank
        {
            get
            {
                var eps = Math.Pow(2.0, -52.0);
                var tol = Math.Max(MatrixU.RowCount, MatrixVT.ColumnCount) * VectorS[0] * eps;
                var nm = Math.Min(MatrixU.RowCount, MatrixVT.ColumnCount);
                var rank = 0;
                for (var h = 0; h < nm; h++)
                {
                    if (VectorS[h] > tol)
                    {
                        rank++;
                    }
                }

                return rank;
            }
        }

        /// <summary>
        /// Internal method which routes the call to perform the singular value decomposition to the appropriate class.
        /// </summary>
        /// <param name="matrix">The matrix to factor.</param>
        /// <param name="computeVectors">Compute the singular U and VT vectors or not.</param>
        /// <returns>An SVD object.</returns>
        internal static Svd Create(Matrix matrix, bool computeVectors)
        {
            var dense = matrix as DenseMatrix;
            if (dense != null)
            {
                return new DenseSvd(dense, computeVectors);
            }

            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the two norm of the <see cref="Matrix"/>.
        /// </summary>
        /// <returns>The 2-norm of the <see cref="Matrix"/>.</returns>
        public virtual double Norm2
        {
            get
            {
                return VectorS[0];
            }
        }

        /// <summary>
        /// Gets the condition number <b>max(S) / min(S)</b>
        /// </summary>
        /// <returns>The condition number.</returns>
        public virtual double ConditionNumber
        {
            get
            {
                var tmp = Math.Min(MatrixU.RowCount, MatrixVT.ColumnCount) - 1;
                return VectorS[0] / VectorS[tmp];
            }
        }

        /// <summary>
        /// Gets the determinant of the square matrix for which the SVD was computed.
        /// </summary>
        public virtual double Determinant
        {
            get
            {
                if (MatrixU.RowCount != MatrixVT.ColumnCount)
                {
                    throw new ArgumentException(Resources.ArgumentMatrixSquare);
                }

                var det = 1.0;
                for (var i = 0; i < VectorS.Count; i++)
                {
                    det *= VectorS[i];
                    if (Math.Abs(VectorS[i]).AlmostEqualInDecimalPlaces(0.0, 15))
                    {
                        return 0;
                    }
                }

                return Math.Abs(det);
            }
        }

        /// <summary>Returns the left singular vectors as a <see cref="Matrix"/>.</summary>
        /// <returns>The left singular vectors. The matrix will be <c>null</c>, if <b>computeVectors</b> in the constructor is set to <c>false</c>.</returns>
        public Matrix U()
        {
            return ComputeVectors ? MatrixU.Clone() : null;
        }

        /// <summary>Returns the right singular vectors as a <see cref="Matrix"/>.</summary>
        /// <returns>The right singular vectors. The matrix will be <c>null</c>, if <b>computeVectors</b> in the constructor is set to <c>false</c>.</returns>
        /// <remarks>This is the transpose of the V matrix.</remarks>
        public Matrix VT()
        {
            return ComputeVectors ? MatrixVT.Clone() : null;
        }

        /// <summary>Returns the singular values as a diagonal <see cref="Matrix"/>.</summary>
        /// <returns>The singular values as a diagonal <see cref="Matrix"/>.</returns>        
        public Matrix W()
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

        /// <summary>Returns the singular values as a <see cref="Vector"/>.</summary>
        /// <returns>the singular values as a <see cref="Vector"/>.</returns>
        public Vector S()
        {
            return VectorS.Clone();
        }

        /// <summary>
        /// Solves a system of linear equations, <b>AX = B</b>, with A SVD factorized.
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

            if (!ComputeVectors)
            {
                throw new InvalidOperationException(Resources.SingularVectorsNotComputed);
            }

            Matrix result = MatrixU.CreateMatrix(MatrixVT.ColumnCount, input.ColumnCount);
            Solve(input, result);
            return result;
        }

        /// <summary>
        /// Solves a system of linear equations, <b>AX = B</b>, with A SVD factorized.
        /// </summary>
        /// <param name="input">The right hand side <see cref="Matrix"/>, <b>B</b>.</param>
        /// <param name="result">The left hand side <see cref="Matrix"/>, <b>X</b>.</param>
        public abstract void Solve(Matrix input, Matrix result);

        /// <summary>
        /// Solves a system of linear equations, <b>Ax = b</b>, with A SVD factorized.
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
        /// <param name="result">The left hand side <see cref="Matrix"/>, <b>x</b>.</param>
        public abstract void Solve(Vector input, Vector result);
    }
}
