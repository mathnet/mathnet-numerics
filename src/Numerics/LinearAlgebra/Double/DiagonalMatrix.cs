// <copyright file="DiagonalMatrix.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2015 Math.NET
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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.LinearAlgebra.Storage;
using MathNet.Numerics.Providers.LinearAlgebra;
using MathNet.Numerics.Threading;

namespace MathNet.Numerics.LinearAlgebra.Double
{
    /// <summary>
    /// A matrix type for diagonal matrices.
    /// </summary>
    /// <remarks>
    /// Diagonal matrices can be non-square matrices but the diagonal always starts
    /// at element 0,0. A diagonal matrix will throw an exception if non diagonal
    /// entries are set. The exception to this is when the off diagonal elements are
    /// 0.0 or NaN; these settings will cause no change to the diagonal matrix.
    /// </remarks>
    [Serializable]
    [DebuggerDisplay("DiagonalMatrix {RowCount}x{ColumnCount}-Double")]
    public class DiagonalMatrix : Matrix
    {
        /// <summary>
        /// Gets the matrix's data.
        /// </summary>
        /// <value>The matrix's data.</value>
        readonly double[] _data;

        /// <summary>
        /// Create a new diagonal matrix straight from an initialized matrix storage instance.
        /// The storage is used directly without copying.
        /// Intended for advanced scenarios where you're working directly with
        /// storage for performance or interop reasons.
        /// </summary>
        public DiagonalMatrix(DiagonalMatrixStorage<double> storage)
            : base(storage)
        {
            _data = storage.Data;
        }

        /// <summary>
        /// Create a new square diagonal matrix with the given number of rows and columns.
        /// All cells of the matrix will be initialized to zero.
        /// </summary>
        /// <exception cref="ArgumentException">If the order is less than one.</exception>
        public DiagonalMatrix(int order)
            : this(new DiagonalMatrixStorage<double>(order, order))
         {
         }

        /// <summary>
        /// Create a new diagonal matrix with the given number of rows and columns.
        /// All cells of the matrix will be initialized to zero.
        /// </summary>
        /// <exception cref="ArgumentException">If the row or column count is less than one.</exception>
        public DiagonalMatrix(int rows, int columns)
            : this(new DiagonalMatrixStorage<double>(rows, columns))
        {
        }

        /// <summary>
        /// Create a new diagonal matrix with the given number of rows and columns.
        /// All diagonal cells of the matrix will be initialized to the provided value, all non-diagonal ones to zero.
        /// </summary>
        /// <exception cref="ArgumentException">If the row or column count is less than one.</exception>
        public DiagonalMatrix(int rows, int columns, double diagonalValue)
            : this(rows, columns)
        {
            for (var i = 0; i < _data.Length; i++)
            {
                _data[i] = diagonalValue;
            }
        }

        /// <summary>
        /// Create a new diagonal matrix with the given number of rows and columns directly binding to a raw array.
        /// The array is assumed to contain the diagonal elements only and is used directly without copying.
        /// Very efficient, but changes to the array and the matrix will affect each other.
        /// </summary>
        public DiagonalMatrix(int rows, int columns, double[] diagonalStorage)
            : this(new DiagonalMatrixStorage<double>(rows, columns, diagonalStorage))
        {
        }

        /// <summary>
        /// Create a new diagonal matrix as a copy of the given other matrix.
        /// This new matrix will be independent from the other matrix.
        /// The matrix to copy from must be diagonal as well.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        public static DiagonalMatrix OfMatrix(Matrix<double> matrix)
        {
            return new DiagonalMatrix(DiagonalMatrixStorage<double>.OfMatrix(matrix.Storage));
        }

        /// <summary>
        /// Create a new diagonal matrix as a copy of the given two-dimensional array.
        /// This new matrix will be independent from the provided array.
        /// The array to copy from must be diagonal as well.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        public static DiagonalMatrix OfArray(double[,] array)
        {
            return new DiagonalMatrix(DiagonalMatrixStorage<double>.OfArray(array));
        }

        /// <summary>
        /// Create a new diagonal matrix and initialize each diagonal value from the provided indexed enumerable.
        /// Keys must be provided at most once, zero is assumed if a key is omitted.
        /// This new matrix will be independent from the enumerable.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        public static DiagonalMatrix OfIndexedDiagonal(int rows, int columns, IEnumerable<Tuple<int, double>> diagonal)
        {
            return new DiagonalMatrix(DiagonalMatrixStorage<double>.OfIndexedEnumerable(rows, columns, diagonal));
        }

        /// <summary>
        /// Create a new diagonal matrix and initialize each diagonal value from the provided indexed enumerable.
        /// Keys must be provided at most once, zero is assumed if a key is omitted.
        /// This new matrix will be independent from the enumerable.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        public static DiagonalMatrix OfIndexedDiagonal(int rows, int columns, IEnumerable<(int, double)> diagonal)
        {
            return new DiagonalMatrix(DiagonalMatrixStorage<double>.OfIndexedEnumerable(rows, columns, diagonal));
        }

        /// <summary>
        /// Create a new diagonal matrix and initialize each diagonal value from the provided enumerable.
        /// This new matrix will be independent from the enumerable.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        public static DiagonalMatrix OfDiagonal(int rows, int columns, IEnumerable<double> diagonal)
        {
            return new DiagonalMatrix(DiagonalMatrixStorage<double>.OfEnumerable(rows, columns, diagonal));
        }

        /// <summary>
        /// Create a new diagonal matrix and initialize each diagonal value using the provided init function.
        /// </summary>
        public static DiagonalMatrix Create(int rows, int columns, Func<int, double> init)
        {
            return new DiagonalMatrix(DiagonalMatrixStorage<double>.OfInit(rows, columns, init));
        }

        /// <summary>
        /// Create a new square sparse identity matrix where each diagonal value is set to One.
        /// </summary>
        public static DiagonalMatrix CreateIdentity(int order)
        {
            return new DiagonalMatrix(DiagonalMatrixStorage<double>.OfValue(order, order, One));
        }

        /// <summary>
        /// Create a new diagonal matrix with diagonal values sampled from the provided random distribution.
        /// </summary>
        public static DiagonalMatrix CreateRandom(int rows, int columns, IContinuousDistribution distribution)
        {
            return new DiagonalMatrix(new DiagonalMatrixStorage<double>(rows, columns, Generate.Random(Math.Min(rows, columns), distribution)));
        }

        /// <summary>
        /// Negate each element of this matrix and place the results into the result matrix.
        /// </summary>
        /// <param name="result">The result of the negation.</param>
        protected override void DoNegate(Matrix<double> result)
        {
            if (result is DiagonalMatrix diagResult)
            {
                LinearAlgebraControl.Provider.ScaleArray(-1, _data, diagResult._data);
                return;
            }

            result.Clear();
            for (var i = 0; i < _data.Length; i++)
            {
                result.At(i, i, -_data[i]);
            }
        }

        /// <summary>
        /// Adds another matrix to this matrix.
        /// </summary>
        /// <param name="other">The matrix to add to this matrix.</param>
        /// <param name="result">The matrix to store the result of the addition.</param>
        /// <exception cref="ArgumentOutOfRangeException">If the two matrices don't have the same dimensions.</exception>
        protected override void DoAdd(Matrix<double> other, Matrix<double> result)
        {
            // diagonal + diagonal = diagonal
            if (other is DiagonalMatrix diagOther && result is DiagonalMatrix diagResult)
            {
                LinearAlgebraControl.Provider.AddArrays(_data, diagOther._data, diagResult._data);
                return;
            }

            other.CopyTo(result);
            for (int i = 0; i < _data.Length; i++)
            {
                result.At(i, i, result.At(i, i) + _data[i]);
            }
        }

        /// <summary>
        /// Subtracts another matrix from this matrix.
        /// </summary>
        /// <param name="other">The matrix to subtract.</param>
        /// <param name="result">The matrix to store the result of the subtraction.</param>
        /// <exception cref="ArgumentOutOfRangeException">If the two matrices don't have the same dimensions.</exception>
        protected override void DoSubtract(Matrix<double> other, Matrix<double> result)
        {
            // diagonal - diagonal = diagonal
            if (other is DiagonalMatrix diagOther && result is DiagonalMatrix diagResult)
            {
                LinearAlgebraControl.Provider.SubtractArrays(_data, diagOther._data, diagResult._data);
                return;
            }

            other.Negate(result);
            for (int i = 0; i < _data.Length; i++)
            {
                result.At(i, i, result.At(i, i) + _data[i]);
            }
        }

        /// <summary>
        /// Multiplies each element of the matrix by a scalar and places results into the result matrix.
        /// </summary>
        /// <param name="scalar">The scalar to multiply the matrix with.</param>
        /// <param name="result">The matrix to store the result of the multiplication.</param>
        /// <exception cref="ArgumentException">If the result matrix's dimensions are not the same as this matrix.</exception>
        protected override void DoMultiply(double scalar, Matrix<double> result)
        {
            if (scalar == 0.0)
            {
                result.Clear();
                return;
            }

            if (scalar == 1.0)
            {
                CopyTo(result);
                return;
            }

            if (result is DiagonalMatrix diagResult)
            {
                LinearAlgebraControl.Provider.ScaleArray(scalar, _data, diagResult._data);
            }
            else
            {
                base.DoMultiply(scalar, result);
            }
        }

        /// <summary>
        /// Multiplies this matrix with a vector and places the results into the result vector.
        /// </summary>
        /// <param name="rightSide">The vector to multiply with.</param>
        /// <param name="result">The result of the multiplication.</param>
        protected override void DoMultiply(Vector<double> rightSide, Vector<double> result)
        {
            var d = Math.Min(ColumnCount, RowCount);
            if (d < RowCount)
            {
                result.ClearSubVector(ColumnCount, RowCount - ColumnCount);
            }

            if (d == ColumnCount)
            {
                if (rightSide.Storage is DenseVectorStorage<double> denseOther && result.Storage is DenseVectorStorage<double> denseResult)
                {
                    LinearAlgebraControl.Provider.PointWiseMultiplyArrays(_data, denseOther.Data, denseResult.Data);
                    return;
                }
            }

            for (var i = 0; i < d; i++)
            {
                result.At(i, _data[i]*rightSide.At(i));
            }
        }

        /// <summary>
        /// Multiplies this matrix with another matrix and places the results into the result matrix.
        /// </summary>
        /// <param name="other">The matrix to multiply with.</param>
        /// <param name="result">The result of the multiplication.</param>
        protected override void DoMultiply(Matrix<double> other, Matrix<double> result)
        {
            if (other is DiagonalMatrix diagonalOther && result is DiagonalMatrix diagonalResult)
            {
                var thisDataCopy = new double[diagonalResult._data.Length];
                var otherDataCopy = new double[diagonalResult._data.Length];
                Array.Copy(_data, 0, thisDataCopy, 0, (diagonalResult._data.Length > _data.Length) ? _data.Length : diagonalResult._data.Length);
                Array.Copy(diagonalOther._data, 0, otherDataCopy, 0, (diagonalResult._data.Length > diagonalOther._data.Length) ? diagonalOther._data.Length : diagonalResult._data.Length);
                LinearAlgebraControl.Provider.PointWiseMultiplyArrays(thisDataCopy, otherDataCopy, diagonalResult._data);
                return;
            }

            if (other.Storage is DenseColumnMajorMatrixStorage<double> denseOther)
            {
                var dense = denseOther.Data;
                var diagonal = _data;
                var d = Math.Min(denseOther.RowCount, RowCount);
                if (d < RowCount)
                {
                    result.ClearSubMatrix(denseOther.RowCount, RowCount - denseOther.RowCount, 0, denseOther.ColumnCount);
                }
                int index = 0;
                for (int i = 0; i < denseOther.ColumnCount; i++)
                {
                    for (int j = 0; j < d; j++)
                    {
                        result.At(j, i, dense[index]*diagonal[j]);
                        index++;
                    }
                    index += (denseOther.RowCount - d);
                }
                return;
            }

            if (ColumnCount == RowCount)
            {
                other.Storage.MapIndexedTo(result.Storage, (i, _, x) => x*_data[i], Zeros.AllowSkip, ExistingData.Clear);
            }
            else
            {
                result.Clear();
                other.Storage.MapSubMatrixIndexedTo(result.Storage, (i, _, x) => x*_data[i], 0, 0, Math.Min(RowCount, other.RowCount), 0, 0, other.ColumnCount, Zeros.AllowSkip, ExistingData.AssumeZeros);
            }
        }

        /// <summary>
        /// Multiplies this matrix with transpose of another matrix and places the results into the result matrix.
        /// </summary>
        /// <param name="other">The matrix to multiply with.</param>
        /// <param name="result">The result of the multiplication.</param>
        protected override void DoTransposeAndMultiply(Matrix<double> other, Matrix<double> result)
        {
            if (other is DiagonalMatrix diagonalOther && result is DiagonalMatrix diagonalResult)
            {
                var thisDataCopy = new double[diagonalResult._data.Length];
                var otherDataCopy = new double[diagonalResult._data.Length];
                Array.Copy(_data, 0, thisDataCopy, 0, (diagonalResult._data.Length > _data.Length) ? _data.Length : diagonalResult._data.Length);
                Array.Copy(diagonalOther._data, 0, otherDataCopy, 0, (diagonalResult._data.Length > diagonalOther._data.Length) ? diagonalOther._data.Length : diagonalResult._data.Length);
                LinearAlgebraControl.Provider.PointWiseMultiplyArrays(thisDataCopy, otherDataCopy, diagonalResult._data);
                return;
            }

            if (other.Storage is DenseColumnMajorMatrixStorage<double> denseOther)
            {
                var dense = denseOther.Data;
                var diagonal = _data;
                var d = Math.Min(denseOther.ColumnCount, RowCount);
                if (d < RowCount)
                {
                    result.ClearSubMatrix(denseOther.ColumnCount, RowCount - denseOther.ColumnCount, 0, denseOther.RowCount);
                }
                int index = 0;
                for (int j = 0; j < d; j++)
                {
                    for (int i = 0; i < denseOther.RowCount; i++)
                    {
                        result.At(j, i, dense[index]*diagonal[j]);
                        index++;
                    }
                }
                return;
            }

            base.DoTransposeAndMultiply(other, result);
        }

        /// <summary>
        /// Multiplies the transpose of this matrix with another matrix and places the results into the result matrix.
        /// </summary>
        /// <param name="other">The matrix to multiply with.</param>
        /// <param name="result">The result of the multiplication.</param>
        protected override void DoTransposeThisAndMultiply(Matrix<double> other, Matrix<double> result)
        {
            if (other is DiagonalMatrix diagonalOther && result is DiagonalMatrix diagonalResult)
            {
                var thisDataCopy = new double[diagonalResult._data.Length];
                var otherDataCopy = new double[diagonalResult._data.Length];
                Array.Copy(_data, 0, thisDataCopy, 0, (diagonalResult._data.Length > _data.Length) ? _data.Length : diagonalResult._data.Length);
                Array.Copy(diagonalOther._data, 0, otherDataCopy, 0, (diagonalResult._data.Length > diagonalOther._data.Length) ? diagonalOther._data.Length : diagonalResult._data.Length);
                LinearAlgebraControl.Provider.PointWiseMultiplyArrays(thisDataCopy, otherDataCopy, diagonalResult._data);
                return;
            }

            if (other.Storage is DenseColumnMajorMatrixStorage<double> denseOther)
            {
                var dense = denseOther.Data;
                var diagonal = _data;
                var d = Math.Min(denseOther.RowCount, ColumnCount);
                if (d < ColumnCount)
                {
                    result.ClearSubMatrix(denseOther.RowCount, ColumnCount - denseOther.RowCount, 0, denseOther.ColumnCount);
                }
                int index = 0;
                for (int i = 0; i < denseOther.ColumnCount; i++)
                {
                    for (int j = 0; j < d; j++)
                    {
                        result.At(j, i, dense[index]*diagonal[j]);
                        index++;
                    }
                    index += (denseOther.RowCount - d);
                }
                return;
            }

            if (ColumnCount == RowCount)
            {
                other.Storage.MapIndexedTo(result.Storage, (i, _, x) => x*_data[i], Zeros.AllowSkip, ExistingData.Clear);
            }
            else
            {
                result.Clear();
                other.Storage.MapSubMatrixIndexedTo(result.Storage, (i, _, x) => x*_data[i], 0, 0, other.RowCount, 0, 0, other.ColumnCount, Zeros.AllowSkip, ExistingData.AssumeZeros);
            }
        }

        /// <summary>
        /// Multiplies the transpose of this matrix with a vector and places the results into the result vector.
        /// </summary>
        /// <param name="rightSide">The vector to multiply with.</param>
        /// <param name="result">The result of the multiplication.</param>
        protected override void DoTransposeThisAndMultiply(Vector<double> rightSide, Vector<double> result)
        {
            var d = Math.Min(ColumnCount, RowCount);
            if (d < ColumnCount)
            {
                result.ClearSubVector(RowCount, ColumnCount - RowCount);
            }

            if (d == RowCount)
            {
                if (rightSide.Storage is DenseVectorStorage<double> denseOther && result.Storage is DenseVectorStorage<double> denseResult)
                {
                    LinearAlgebraControl.Provider.PointWiseMultiplyArrays(_data, denseOther.Data, denseResult.Data);
                    return;
                }
            }

            for (var i = 0; i < d; i++)
            {
                result.At(i, _data[i]*rightSide.At(i));
            }
        }

        /// <summary>
        /// Divides each element of the matrix by a scalar and places results into the result matrix.
        /// </summary>
        /// <param name="divisor">The scalar to divide the matrix with.</param>
        /// <param name="result">The matrix to store the result of the division.</param>
        protected override void DoDivide(double divisor, Matrix<double> result)
        {
            if (divisor == 1.0)
            {
                CopyTo(result);
                return;
            }

            if (result is DiagonalMatrix diagResult)
            {
                LinearAlgebraControl.Provider.ScaleArray(1.0/divisor, _data, diagResult._data);
                return;
            }

            result.Clear();
            for (int i = 0; i < _data.Length; i++)
            {
                result.At(i, i, _data[i]/divisor);
            }
        }

        /// <summary>
        /// Divides a scalar by each element of the matrix and stores the result in the result matrix.
        /// </summary>
        /// <param name="dividend">The scalar to add.</param>
        /// <param name="result">The matrix to store the result of the division.</param>
        protected override void DoDivideByThis(double dividend, Matrix<double> result)
        {
            if (result is DiagonalMatrix diagResult)
            {
                var resultData = diagResult._data;
                CommonParallel.For(0, _data.Length, 4096, (a, b) =>
                {
                    for (int i = a; i < b; i++)
                    {
                        resultData[i] = dividend/_data[i];
                    }
                });
                return;
            }

            result.Clear();
            for (int i = 0; i < _data.Length; i++)
            {
                result.At(i, i, dividend/_data[i]);
            }
        }

        /// <summary>
        /// Computes the determinant of this matrix.
        /// </summary>
        /// <returns>The determinant of this matrix.</returns>
        public override double Determinant()
        {
            if (RowCount != ColumnCount)
            {
                throw new ArgumentException("Matrix must be square.");
            }

            return _data.Aggregate(1.0, (current, t) => current * t);
        }

        /// <summary>
        /// Returns the elements of the diagonal in a <see cref="DenseVector"/>.
        /// </summary>
        /// <returns>The elements of the diagonal.</returns>
        /// <remarks>For non-square matrices, the method returns Min(Rows, Columns) elements where
        /// i == j (i is the row index, and j is the column index).</remarks>
        public override Vector<double> Diagonal()
        {
            return new DenseVector(_data).Clone();
        }

        /// <summary>
        /// Copies the values of the given array to the diagonal.
        /// </summary>
        /// <param name="source">The array to copy the values from. The length of the vector should be
        /// Min(Rows, Columns).</param>
        /// <exception cref="ArgumentException">If the length of <paramref name="source"/> does not
        /// equal Min(Rows, Columns).</exception>
        /// <remarks>For non-square matrices, the elements of <paramref name="source"/> are copied to
        /// this[i,i].</remarks>
        public override void SetDiagonal(double[] source)
        {
            if (source.Length != _data.Length)
            {
                throw new ArgumentException("The array arguments must have the same length.", nameof(source));
            }

            Buffer.BlockCopy(source, 0, _data, 0, source.Length * Constants.SizeOfDouble);
        }

        /// <summary>
        /// Copies the values of the given <see cref="Vector{T}"/> to the diagonal.
        /// </summary>
        /// <param name="source">The vector to copy the values from. The length of the vector should be
        /// Min(Rows, Columns).</param>
        /// <exception cref="ArgumentException">If the length of <paramref name="source"/> does not
        /// equal Min(Rows, Columns).</exception>
        /// <remarks>For non-square matrices, the elements of <paramref name="source"/> are copied to
        /// this[i,i].</remarks>
        public override void SetDiagonal(Vector<double> source)
        {
            if (source is DenseVector denseSource)
            {
                if (_data.Length != denseSource.Values.Length)
                {
                    throw new ArgumentException("All vectors must have the same dimensionality.", nameof(source));
                }

                Buffer.BlockCopy(denseSource.Values, 0, _data, 0, denseSource.Values.Length * Constants.SizeOfDouble);
            }
            else
            {
                base.SetDiagonal(source);
            }
        }

        /// <summary>Calculates the induced L1 norm of this matrix.</summary>
        /// <returns>The maximum absolute column sum of the matrix.</returns>
        public override double L1Norm()
        {
            return _data.Aggregate(0d, (current, t) => Math.Max(current, Math.Abs(t)));
        }

        /// <summary>Calculates the induced L2 norm of the matrix.</summary>
        /// <returns>The largest singular value of the matrix.</returns>
        public override double L2Norm()
        {
            return _data.Aggregate(0d, (current, t) => Math.Max(current, Math.Abs(t)));
        }

        /// <summary>Calculates the induced infinity norm of this matrix.</summary>
        /// <returns>The maximum absolute row sum of the matrix.</returns>
        public override double InfinityNorm()
        {
            return L1Norm();
        }

        /// <summary>Calculates the entry-wise Frobenius norm of this matrix.</summary>
        /// <returns>The square root of the sum of the squared values.</returns>
        public override double FrobeniusNorm()
        {
            return Math.Sqrt(_data.Sum(t => t * t));
        }

        /// <summary>Calculates the condition number of this matrix.</summary>
        /// <returns>The condition number of the matrix.</returns>
        public override double ConditionNumber()
        {
            var maxSv = double.NegativeInfinity;
            var minSv = double.PositiveInfinity;
            foreach (var t in _data)
            {
                maxSv = Math.Max(maxSv, Math.Abs(t));
                minSv = Math.Min(minSv, Math.Abs(t));
            }

            return maxSv / minSv;
        }

        /// <summary>Computes the inverse of this matrix.</summary>
        /// <exception cref="ArgumentException">If <see cref="DiagonalMatrix"/> is not a square matrix.</exception>
        /// <exception cref="ArgumentException">If <see cref="DiagonalMatrix"/> is singular.</exception>
        /// <returns>The inverse of this matrix.</returns>
        public override Matrix<double> Inverse()
        {
            if (RowCount != ColumnCount)
            {
                throw new ArgumentException("Matrix must be square.");
            }

            var inverse = (DiagonalMatrix)Clone();
            var inverseData = inverse._data;

            for (var i = 0; i < _data.Length; i++)
            {
                if (_data[i] != 0.0)
                {
                    inverseData[i] = 1.0 / _data[i];
                }
                else
                {
                    throw new ArgumentException("Matrix must not be singular.");
                }
            }

            return inverse;
        }

        /// <summary>
        /// Returns a new matrix containing the lower triangle of this matrix.
        /// </summary>
        /// <returns>The lower triangle of this matrix.</returns>
        public override Matrix<double> LowerTriangle()
        {
            return Clone();
        }

        /// <summary>
        /// Puts the lower triangle of this matrix into the result matrix.
        /// </summary>
        /// <param name="result">Where to store the lower triangle.</param>
        /// <exception cref="ArgumentException">If the result matrix's dimensions are not the same as this matrix.</exception>
        public override void LowerTriangle(Matrix<double> result)
        {
            if (result.RowCount != RowCount || result.ColumnCount != ColumnCount)
            {
                throw DimensionsDontMatch<ArgumentException>(this, result, "result");
            }

            if (ReferenceEquals(this, result))
            {
                return;
            }

            result.Clear();
            for (var i = 0; i < _data.Length; i++)
            {
                result.At(i, i, _data[i]);
            }
        }

        /// <summary>
        /// Returns a new matrix containing the lower triangle of this matrix. The new matrix
        /// does not contain the diagonal elements of this matrix.
        /// </summary>
        /// <returns>The lower triangle of this matrix.</returns>
        public override Matrix<double> StrictlyLowerTriangle()
        {
            return new DiagonalMatrix(RowCount, ColumnCount);
        }

        /// <summary>
        /// Puts the strictly lower triangle of this matrix into the result matrix.
        /// </summary>
        /// <param name="result">Where to store the lower triangle.</param>
        /// <exception cref="ArgumentException">If the result matrix's dimensions are not the same as this matrix.</exception>
        public override void StrictlyLowerTriangle(Matrix<double> result)
        {
            if (result.RowCount != RowCount || result.ColumnCount != ColumnCount)
            {
                throw DimensionsDontMatch<ArgumentException>(this, result, "result");
            }

            result.Clear();
        }

        /// <summary>
        /// Returns a new matrix containing the upper triangle of this matrix.
        /// </summary>
        /// <returns>The upper triangle of this matrix.</returns>
        public override Matrix<double> UpperTriangle()
        {
            return Clone();
        }

        /// <summary>
        /// Puts the upper triangle of this matrix into the result matrix.
        /// </summary>
        /// <param name="result">Where to store the lower triangle.</param>
        /// <exception cref="ArgumentException">If the result matrix's dimensions are not the same as this matrix.</exception>
        public override void UpperTriangle(Matrix<double> result)
        {
            if (result.RowCount != RowCount || result.ColumnCount != ColumnCount)
            {
                throw DimensionsDontMatch<ArgumentException>(this, result, "result");
            }

            result.Clear();
            for (var i = 0; i < _data.Length; i++)
            {
                result.At(i, i, _data[i]);
            }
        }

        /// <summary>
        /// Returns a new matrix containing the upper triangle of this matrix. The new matrix
        /// does not contain the diagonal elements of this matrix.
        /// </summary>
        /// <returns>The upper triangle of this matrix.</returns>
        public override Matrix<double> StrictlyUpperTriangle()
        {
            return new DiagonalMatrix(RowCount, ColumnCount);
        }

        /// <summary>
        /// Puts the strictly upper triangle of this matrix into the result matrix.
        /// </summary>
        /// <param name="result">Where to store the lower triangle.</param>
        /// <exception cref="ArgumentException">If the result matrix's dimensions are not the same as this matrix.</exception>
        public override void StrictlyUpperTriangle(Matrix<double> result)
        {
            if (result.RowCount != RowCount || result.ColumnCount != ColumnCount)
            {
                throw DimensionsDontMatch<ArgumentException>(this, result, "result");
            }

            result.Clear();
        }

        /// <summary>
        /// Creates a matrix that contains the values from the requested sub-matrix.
        /// </summary>
        /// <param name="rowIndex">The row to start copying from.</param>
        /// <param name="rowCount">The number of rows to copy. Must be positive.</param>
        /// <param name="columnIndex">The column to start copying from.</param>
        /// <param name="columnCount">The number of columns to copy. Must be positive.</param>
        /// <returns>The requested sub-matrix.</returns>
        /// <exception cref="ArgumentOutOfRangeException">If: <list><item><paramref name="rowIndex"/> is
        /// negative, or greater than or equal to the number of rows.</item>
        /// <item><paramref name="columnIndex"/> is negative, or greater than or equal to the number
        /// of columns.</item>
        /// <item><c>(columnIndex + columnLength) &gt;= Columns</c></item>
        /// <item><c>(rowIndex + rowLength) &gt;= Rows</c></item></list></exception>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="rowCount"/> or <paramref name="columnCount"/>
        /// is not positive.</exception>
        public override Matrix<double> SubMatrix(int rowIndex, int rowCount, int columnIndex, int columnCount)
        {
            var target = rowIndex == columnIndex
                ? (Matrix<double>)new DiagonalMatrix(rowCount, columnCount)
                : new SparseMatrix(rowCount, columnCount);

            Storage.CopySubMatrixTo(target.Storage, rowIndex, 0, rowCount, columnIndex, 0, columnCount, ExistingData.AssumeZeros);
            return target;
        }

        /// <summary>
        /// Permute the columns of a matrix according to a permutation.
        /// </summary>
        /// <param name="p">The column permutation to apply to this matrix.</param>
        /// <exception cref="InvalidOperationException">Always thrown</exception>
        /// <remarks>Permutation in diagonal matrix are senseless, because of matrix nature</remarks>
        public override void PermuteColumns(Permutation p)
        {
            throw new InvalidOperationException("Permutations in diagonal matrix are not allowed");
        }

        /// <summary>
        /// Permute the rows of a matrix according to a permutation.
        /// </summary>
        /// <param name="p">The row permutation to apply to this matrix.</param>
        /// <exception cref="InvalidOperationException">Always thrown</exception>
        /// <remarks>Permutation in diagonal matrix are senseless, because of matrix nature</remarks>
        public override void PermuteRows(Permutation p)
        {
            throw new InvalidOperationException("Permutations in diagonal matrix are not allowed");
        }

        /// <summary>
        /// Evaluates whether this matrix is symmetric.
        /// </summary>
        public sealed override bool IsSymmetric()
        {
            return true;
        }

        /// <summary>
        /// Computes the canonical modulus, where the result has the sign of the divisor,
        /// for the given divisor each element of the matrix.
        /// </summary>
        /// <param name="divisor">The scalar denominator to use.</param>
        /// <param name="result">Matrix to store the results in.</param>
        protected override void DoModulus(double divisor, Matrix<double> result)
        {
            if (result is DiagonalMatrix diagonalResult)
            {
                CommonParallel.For(0, _data.Length, 4096, (a, b) =>
                {
                    var r = diagonalResult._data;
                    for (var i = a; i < b; i++)
                    {
                        r[i] = Euclid.Modulus(_data[i], divisor);
                    }
                });
            }
            else
            {
                base.DoModulus(divisor, result);
            }
        }

        /// <summary>
        /// Computes the canonical modulus, where the result has the sign of the divisor,
        /// for the given dividend for each element of the matrix.
        /// </summary>
        /// <param name="dividend">The scalar numerator to use.</param>
        /// <param name="result">A vector to store the results in.</param>
        protected override void DoModulusByThis(double dividend, Matrix<double> result)
        {
            if (result is DiagonalMatrix diagonalResult)
            {
                CommonParallel.For(0, _data.Length, 4096, (a, b) =>
                {
                    var r = diagonalResult._data;
                    for (var i = a; i < b; i++)
                    {
                        r[i] = Euclid.Modulus(dividend, _data[i]);
                    }
                });
            }
            else
            {
                base.DoModulusByThis(dividend, result);
            }
        }

        /// <summary>
        /// Computes the remainder (% operator), where the result has the sign of the dividend,
        /// for the given divisor each element of the matrix.
        /// </summary>
        /// <param name="divisor">The scalar denominator to use.</param>
        /// <param name="result">Matrix to store the results in.</param>
        protected override void DoRemainder(double divisor, Matrix<double> result)
        {
            if (result is DiagonalMatrix diagonalResult)
            {
                CommonParallel.For(0, _data.Length, 4096, (a, b) =>
                {
                    var r = diagonalResult._data;
                    for (var i = a; i < b; i++)
                    {
                        r[i] = _data[i] % divisor;
                    }
                });
            }
            else
            {
                base.DoRemainder(divisor, result);
            }
        }

        /// <summary>
        /// Computes the remainder (% operator), where the result has the sign of the dividend,
        /// for the given dividend for each element of the matrix.
        /// </summary>
        /// <param name="dividend">The scalar numerator to use.</param>
        /// <param name="result">A vector to store the results in.</param>
        protected override void DoRemainderByThis(double dividend, Matrix<double> result)
        {
            if (result is DiagonalMatrix diagonalResult)
            {
                CommonParallel.For(0, _data.Length, 4096, (a, b) =>
                {
                    var r = diagonalResult._data;
                    for (var i = a; i < b; i++)
                    {
                        r[i] = dividend % _data[i];
                    }
                });
            }
            else
            {
                base.DoRemainderByThis(dividend, result);
            }
        }
    }
}
