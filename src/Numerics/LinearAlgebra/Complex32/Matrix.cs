// <copyright file="Matrix.cs" company="Math.NET">
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

namespace MathNet.Numerics.LinearAlgebra.Complex32
{
    using System;
    using Generic;
    using Numerics;
    using Properties;
    using Storage;

    /// <summary>
    /// <c>Complex32</c> version of the <see cref="Matrix{T}"/> class.
    /// </summary>
    [Serializable]
    public abstract class Matrix : Matrix<Complex32>
    {
        /// <summary>
        /// Initializes a new instance of the Matrix class.
        /// </summary>
        protected Matrix(MatrixStorage<Complex32> storage)
            : base(storage)
        {
        }

        /// <summary>Calculates the L1 norm.</summary>
        /// <returns>The L1 norm of the matrix.</returns>
        public override Complex32 L1Norm()
        {
            var norm = 0.0f;
            for (var j = 0; j < ColumnCount; j++)
            {
                var s = 0.0f;
                for (var i = 0; i < RowCount; i++)
                {
                    s += At(i, j).Magnitude;
                }

                norm = Math.Max(norm, s);
            }

            return norm;
        }

        /// <summary>
        /// Returns the conjugate transpose of this matrix.
        /// </summary>
        /// <returns>The conjugate transpose of this matrix.</returns>
        public override Matrix<Complex32> ConjugateTranspose()
        {
            var ret = CreateMatrix(ColumnCount, RowCount);
            for (var j = 0; j < ColumnCount; j++)
            {
                for (var i = 0; i < RowCount; i++)
                {
                    ret.At(j, i, At(i, j).Conjugate());
                }
            }

            return ret;
        }

        /// <summary>Calculates the Frobenius norm of this matrix.</summary>
        /// <returns>The Frobenius norm of this matrix.</returns>
        public override Complex32 FrobeniusNorm()
        {
            var transpose = ConjugateTranspose();
            var aat = this * transpose;

            var norm = 0.0f;
            for (var i = 0; i < RowCount; i++)
            {
                norm += aat.At(i, i).Magnitude;
            }

            norm = Convert.ToSingle(Math.Sqrt(norm));

            return norm;
        }

        /// <summary>Calculates the infinity norm of this matrix.</summary>
        /// <returns>The infinity norm of this matrix.</returns>
        public override Complex32 InfinityNorm()
        {
            var norm = 0.0f;
            for (var i = 0; i < RowCount; i++)
            {
                var s = 0.0f;
                for (var j = 0; j < ColumnCount; j++)
                {
                    s += At(i, j).Magnitude;
                }

                norm = Math.Max(norm, s);
            }

            return norm;
        }

        /// <summary>
        /// Adds another matrix to this matrix.
        /// </summary>
        /// <param name="other">The matrix to add to this matrix.</param>
        /// <param name="result">The matrix to store the result of the addition.</param>
        /// <exception cref="ArgumentNullException">If the other matrix is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If the two matrices don't have the same dimensions.</exception>
        protected override void DoAdd(Matrix<Complex32> other, Matrix<Complex32> result)
        {
            for (var i = 0; i < RowCount; i++)
            {
                for (var j = 0; j < ColumnCount; j++)
                {
                    result.At(i, j, At(i, j) + other.At(i, j));
                }
            }
        }

        /// <summary>
        /// Subtracts another matrix from this matrix.
        /// </summary>
        /// <param name="other">The matrix to subtract to this matrix.</param>
        /// <param name="result">The matrix to store the result of subtraction.</param>
        /// <exception cref="ArgumentNullException">If the other matrix is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If the two matrices don't have the same dimensions.</exception>
        protected override void DoSubtract(Matrix<Complex32> other, Matrix<Complex32> result)
        {
            for (var i = 0; i < RowCount; i++)
            {
                for (var j = 0; j < ColumnCount; j++)
                {
                    result.At(i, j, At(i, j) - other.At(i, j));
                }
            }
        }

        /// <summary>
        /// Multiplies each element of the matrix by a scalar and places results into the result matrix.
        /// </summary>
        /// <param name="scalar">The scalar to multiply the matrix with.</param>
        /// <param name="result">The matrix to store the result of the multiplication.</param>
        protected override void DoMultiply(Complex32 scalar, Matrix<Complex32> result)
        {
            for (var i = 0; i < RowCount; i++)
            {
                for (var j = 0; j < ColumnCount; j++)
                {
                    result.At(i, j, At(i, j) * scalar);
                }
            }
        }

         /// <summary>
        /// Multiplies this matrix with a vector and places the results into the result vector.
        /// </summary>
        /// <param name="rightSide">The vector to multiply with.</param>
        /// <param name="result">The result of the multiplication.</param>
        protected override void DoMultiply(Vector<Complex32> rightSide, Vector<Complex32> result)
         {
            for (var i = 0; i < RowCount; i++)
            {
                var s = Complex32.Zero;
                for (var j = 0; j != ColumnCount; j++)
                {
                    s += At(i, j) * rightSide[j];
                }

                result[i] = s;
            }
         }

        /// <summary>
        /// Divides each element of the matrix by a scalar and places results into the result matrix.
        /// </summary>
        /// <param name="scalar">The scalar to divide the matrix with.</param>
        /// <param name="result">The matrix to store the result of the division.</param>
        protected override void DoDivide(Complex32 scalar, Matrix<Complex32> result)
        {
            DoMultiply(1.0f / scalar, result);
        }

        /// <summary>
        /// Multiplies this matrix with another matrix and places the results into the result matrix.
        /// </summary>
        /// <param name="other">The matrix to multiply with.</param>
        /// <param name="result">The result of the multiplication.</param>
        protected override void DoMultiply(Matrix<Complex32> other, Matrix<Complex32> result)
        {
            for (var j = 0; j < RowCount; j++)
            {
                for (var i = 0; i != other.ColumnCount; i++)
                {
                    var s = Complex32.Zero;
                    for (var l = 0; l < ColumnCount; l++)
                    {
                        s += At(j, l) * other.At(l, i);
                    }

                    result.At(j, i, s);
                }
            }
        }

        /// <summary>
        /// Multiplies this matrix with transpose of another matrix and places the results into the result matrix.
        /// </summary>
        /// <param name="other">The matrix to multiply with.</param>
        /// <param name="result">The result of the multiplication.</param>
        protected override void DoTransposeAndMultiply(Matrix<Complex32> other, Matrix<Complex32> result)
        {
            for (var j = 0; j < other.RowCount; j++)
            {
                for (var i = 0; i < RowCount; i++)
                {
                    var s = Complex32.Zero;
                    for (var l = 0; l < ColumnCount; l++)
                    {
                        s += At(i, l) * other.At(j, l);
                    }

                    result.At(i, j, s);
                }
            }
        }

        /// <summary>
        /// Multiplies the transpose of this matrix with another matrix and places the results into the result matrix.
        /// </summary>
        /// <param name="other">The matrix to multiply with.</param>
        /// <param name="result">The result of the multiplication.</param>
        protected override void DoTransposeThisAndMultiply(Matrix<Complex32> other, Matrix<Complex32> result)
        {
            for (var j = 0; j < other.ColumnCount; j++)
            {
                for (var i = 0; i < ColumnCount; i++)
                {
                    var s = Complex32.Zero;
                    for (var l = 0; l < RowCount; l++)
                    {
                        s += At(l, i) * other.At(l, j);
                    }

                    result.At(i, j, s);
                }
            }
        }

        /// <summary>
        /// Multiplies the transpose of this matrix with a vector and places the results into the result vector.
        /// </summary>
        /// <param name="rightSide">The vector to multiply with.</param>
        /// <param name="result">The result of the multiplication.</param>
        protected override void DoTransposeThisAndMultiply(Vector<Complex32> rightSide, Vector<Complex32> result)
        {
            for (var i = 0; i < ColumnCount; i++)
            {
                var s = Complex32.Zero;
                for (var j = 0; j != RowCount; j++)
                {
                    s += At(j, i) * rightSide[j];
                }

                result[i] = s;
            }
        }

        /// <summary>
        /// Negate each element of this matrix and place the results into the result matrix.
        /// </summary>
        /// <param name="result">The result of the negation.</param>
        protected override void DoNegate(Matrix<Complex32> result)
        {
            for (var i = 0; i < RowCount; i++)
            {
                for (var j = 0; j != ColumnCount; j++)
                {
                    result.At(i, j, -At(i, j));
                }
            }
        }

        /// <summary>
        /// Complex conjugates each element of this matrix and place the results into the result matrix.
        /// </summary>
        /// <param name="result">The result of the conjugation.</param>
        protected override void DoConjugate(Matrix<Complex32> result)
        {
            for (var i = 0; i < RowCount; i++)
            {
                for (var j = 0; j != ColumnCount; j++)
                {
                    result.At(i, j, At(i, j).Conjugate());
                }
            }
        }

        /// <summary>
        /// Pointwise multiplies this matrix with another matrix and stores the result into the result matrix.
        /// </summary>
        /// <param name="other">The matrix to pointwise multiply with this one.</param>
        /// <param name="result">The matrix to store the result of the pointwise multiplication.</param>
        protected override void DoPointwiseMultiply(Matrix<Complex32> other, Matrix<Complex32> result)
        {
            for (var j = 0; j < ColumnCount; j++)
            {
                for (var i = 0; i < RowCount; i++)
                {
                    result.At(i, j, At(i, j) * other.At(i, j));
                }
            }
        }

        /// <summary>
        /// Pointwise divide this matrix by another matrix and stores the result into the result matrix.
        /// </summary>
        /// <param name="other">The matrix to pointwise divide this one by.</param>
        /// <param name="result">The matrix to store the result of the pointwise division.</param>
        protected override void DoPointwiseDivide(Matrix<Complex32> other, Matrix<Complex32> result)
        {
            for (var j = 0; j < ColumnCount; j++)
            {
                for (var i = 0; i < RowCount; i++)
                {
                    result.At(i, j, At(i, j) / other.At(i, j));
                }
            }
        }

        /// <summary>
        /// Computes the modulus for each element of the matrix.
        /// </summary>
        /// <param name="divisor">The divisor to use.</param>
        /// <param name="result">Matrix to store the results in.</param>
        protected override void DoModulus(Complex32 divisor, Matrix<Complex32> result)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Computes the trace of this matrix.
        /// </summary>
        /// <returns>The trace of this matrix</returns>
        /// <exception cref="ArgumentException">If the matrix is not square</exception>
        public override Complex32 Trace()
        {
            if (RowCount != ColumnCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixSquare);
            }

            var sum = Complex32.Zero;
            for (var i = 0; i < RowCount; i++)
            {
                sum += At(i, i);
            }

            return sum;
        }
    }
}
