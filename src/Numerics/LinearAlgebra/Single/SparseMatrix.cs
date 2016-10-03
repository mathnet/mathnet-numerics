﻿// <copyright file="SparseMatrix.cs" company="Math.NET">
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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MathNet.Numerics.LinearAlgebra.Storage;

namespace MathNet.Numerics.LinearAlgebra.Single
{
    /// <summary>
    /// A Matrix with sparse storage, intended for very large matrices where most of the cells are zero.
    /// The underlying storage scheme is 3-array compressed-sparse-row (CSR) Format.
    /// <a href="http://en.wikipedia.org/wiki/Sparse_matrix#Compressed_sparse_row_.28CSR_or_CRS.29">Wikipedia - CSR</a>.
    /// </summary>
    [Serializable]
    [DebuggerDisplay("SparseMatrix {RowCount}x{ColumnCount}-Single {NonZerosCount}-NonZero")]
    public class SparseMatrix : Matrix
    {
        readonly SparseCompressedRowMatrixStorage<float> _storage;

        /// <summary>
        /// Gets the number of non zero elements in the matrix.
        /// </summary>
        /// <value>The number of non zero elements.</value>
        public int NonZerosCount
        {
            get { return _storage.ValueCount; }
        }

        /// <summary>
        /// Create a new sparse matrix straight from an initialized matrix storage instance.
        /// The storage is used directly without copying.
        /// Intended for advanced scenarios where you're working directly with
        /// storage for performance or interop reasons.
        /// </summary>
        public SparseMatrix(SparseCompressedRowMatrixStorage<float> storage)
            : base(storage)
        {
            _storage = storage;
        }

        /// <summary>
        /// Create a new square sparse matrix with the given number of rows and columns.
        /// All cells of the matrix will be initialized to zero.
        /// Zero-length matrices are not supported.
        /// </summary>
        /// <exception cref="ArgumentException">If the order is less than one.</exception>
        public SparseMatrix(int order)
            : this(order, order)
        {
        }

        /// <summary>
        /// Create a new sparse matrix with the given number of rows and columns.
        /// All cells of the matrix will be initialized to zero.
        /// Zero-length matrices are not supported.
        /// </summary>
        /// <exception cref="ArgumentException">If the row or column count is less than one.</exception>
        public SparseMatrix(int rows, int columns)
            : this(new SparseCompressedRowMatrixStorage<float>(rows, columns))
        {
        }

        /// <summary>
        /// Create a new sparse matrix as a copy of the given other matrix.
        /// This new matrix will be independent from the other matrix.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        public static SparseMatrix OfMatrix(Matrix<float> matrix)
        {
            return new SparseMatrix(SparseCompressedRowMatrixStorage<float>.OfMatrix(matrix.Storage));
        }

        /// <summary>
        /// Create a new sparse matrix as a copy of the given two-dimensional array.
        /// This new matrix will be independent from the provided array.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        public static SparseMatrix OfArray(float[,] array)
        {
            return new SparseMatrix(SparseCompressedRowMatrixStorage<float>.OfArray(array));
        }

        /// <summary>
        /// Create a new sparse matrix as a copy of the given indexed enumerable.
        /// Keys must be provided at most once, zero is assumed if a key is omitted.
        /// This new matrix will be independent from the enumerable.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        public static SparseMatrix OfIndexed(int rows, int columns, IEnumerable<Tuple<int, int, float>> enumerable)
        {
            return new SparseMatrix(SparseCompressedRowMatrixStorage<float>.OfIndexedEnumerable(rows, columns, enumerable));
        }

        /// <summary>
        /// Create a new sparse matrix as a copy of the given enumerable.
        /// The enumerable is assumed to be in row-major order (row by row).
        /// This new matrix will be independent from the enumerable.
        /// A new memory block will be allocated for storing the vector.
        /// </summary>
        /// <seealso href="http://en.wikipedia.org/wiki/Row-major_order"/>
        public static SparseMatrix OfRowMajor(int rows, int columns, IEnumerable<float> rowMajor)
        {
            return new SparseMatrix(SparseCompressedRowMatrixStorage<float>.OfRowMajorEnumerable(rows, columns, rowMajor));
        }

        /// <summary>
        /// Create a new sparse matrix with the given number of rows and columns as a copy of the given array.
        /// The array is assumed to be in column-major order (column by column).
        /// This new matrix will be independent from the provided array.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        /// <seealso href="http://en.wikipedia.org/wiki/Row-major_order"/>
        public static SparseMatrix OfColumnMajor(int rows, int columns, IList<float> columnMajor)
        {
            return new SparseMatrix(SparseCompressedRowMatrixStorage<float>.OfColumnMajorList(rows, columns, columnMajor));
        }

        /// <summary>
        /// Create a new sparse matrix as a copy of the given enumerable of enumerable columns.
        /// Each enumerable in the master enumerable specifies a column.
        /// This new matrix will be independent from the enumerables.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        public static SparseMatrix OfColumns(IEnumerable<IEnumerable<float>> data)
        {
            return OfColumnArrays(data.Select(v => v.ToArray()).ToArray());
        }

        /// <summary>
        /// Create a new sparse matrix as a copy of the given enumerable of enumerable columns.
        /// Each enumerable in the master enumerable specifies a column.
        /// This new matrix will be independent from the enumerables.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        public static SparseMatrix OfColumns(int rows, int columns, IEnumerable<IEnumerable<float>> data)
        {
            return new SparseMatrix(SparseCompressedRowMatrixStorage<float>.OfColumnEnumerables(rows, columns, data));
        }

        /// <summary>
        /// Create a new sparse matrix as a copy of the given column arrays.
        /// This new matrix will be independent from the arrays.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        public static SparseMatrix OfColumnArrays(params float[][] columns)
        {
            return new SparseMatrix(SparseCompressedRowMatrixStorage<float>.OfColumnArrays(columns));
        }

        /// <summary>
        /// Create a new sparse matrix as a copy of the given column arrays.
        /// This new matrix will be independent from the arrays.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        public static SparseMatrix OfColumnArrays(IEnumerable<float[]> columns)
        {
            return new SparseMatrix(SparseCompressedRowMatrixStorage<float>.OfColumnArrays((columns as float[][]) ?? columns.ToArray()));
        }

        /// <summary>
        /// Create a new sparse matrix as a copy of the given column vectors.
        /// This new matrix will be independent from the vectors.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        public static SparseMatrix OfColumnVectors(params Vector<float>[] columns)
        {
            var storage = new VectorStorage<float>[columns.Length];
            for (int i = 0; i < columns.Length; i++)
            {
                storage[i] = columns[i].Storage;
            }
            return new SparseMatrix(SparseCompressedRowMatrixStorage<float>.OfColumnVectors(storage));
        }

        /// <summary>
        /// Create a new sparse matrix as a copy of the given column vectors.
        /// This new matrix will be independent from the vectors.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        public static SparseMatrix OfColumnVectors(IEnumerable<Vector<float>> columns)
        {
            return new SparseMatrix(SparseCompressedRowMatrixStorage<float>.OfColumnVectors(columns.Select(c => c.Storage).ToArray()));
        }

        /// <summary>
        /// Create a new sparse matrix as a copy of the given enumerable of enumerable rows.
        /// Each enumerable in the master enumerable specifies a row.
        /// This new matrix will be independent from the enumerables.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        public static SparseMatrix OfRows(IEnumerable<IEnumerable<float>> data)
        {
            return OfRowArrays(data.Select(v => v.ToArray()).ToArray());
        }

        /// <summary>
        /// Create a new sparse matrix as a copy of the given enumerable of enumerable rows.
        /// Each enumerable in the master enumerable specifies a row.
        /// This new matrix will be independent from the enumerables.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        public static SparseMatrix OfRows(int rows, int columns, IEnumerable<IEnumerable<float>> data)
        {
            return new SparseMatrix(SparseCompressedRowMatrixStorage<float>.OfRowEnumerables(rows, columns, data));
        }

        /// <summary>
        /// Create a new sparse matrix as a copy of the given row arrays.
        /// This new matrix will be independent from the arrays.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        public static SparseMatrix OfRowArrays(params float[][] rows)
        {
            return new SparseMatrix(SparseCompressedRowMatrixStorage<float>.OfRowArrays(rows));
        }

        /// <summary>
        /// Create a new sparse matrix as a copy of the given row arrays.
        /// This new matrix will be independent from the arrays.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        public static SparseMatrix OfRowArrays(IEnumerable<float[]> rows)
        {
            return new SparseMatrix(SparseCompressedRowMatrixStorage<float>.OfRowArrays((rows as float[][]) ?? rows.ToArray()));
        }

        /// <summary>
        /// Create a new sparse matrix as a copy of the given row vectors.
        /// This new matrix will be independent from the vectors.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        public static SparseMatrix OfRowVectors(params Vector<float>[] rows)
        {
            var storage = new VectorStorage<float>[rows.Length];
            for (int i = 0; i < rows.Length; i++)
            {
                storage[i] = rows[i].Storage;
            }
            return new SparseMatrix(SparseCompressedRowMatrixStorage<float>.OfRowVectors(storage));
        }

        /// <summary>
        /// Create a new sparse matrix as a copy of the given row vectors.
        /// This new matrix will be independent from the vectors.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        public static SparseMatrix OfRowVectors(IEnumerable<Vector<float>> rows)
        {
            return new SparseMatrix(SparseCompressedRowMatrixStorage<float>.OfRowVectors(rows.Select(r => r.Storage).ToArray()));
        }

        /// <summary>
        /// Create a new sparse matrix with the diagonal as a copy of the given vector.
        /// This new matrix will be independent from the vector.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        public static SparseMatrix OfDiagonalVector(Vector<float> diagonal)
        {
            var m = new SparseMatrix(diagonal.Count, diagonal.Count);
            m.SetDiagonal(diagonal);
            return m;
        }

        /// <summary>
        /// Create a new sparse matrix with the diagonal as a copy of the given vector.
        /// This new matrix will be independent from the vector.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        public static SparseMatrix OfDiagonalVector(int rows, int columns, Vector<float> diagonal)
        {
            var m = new SparseMatrix(rows, columns);
            m.SetDiagonal(diagonal);
            return m;
        }

        /// <summary>
        /// Create a new sparse matrix with the diagonal as a copy of the given array.
        /// This new matrix will be independent from the array.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        public static SparseMatrix OfDiagonalArray(float[] diagonal)
        {
            var m = new SparseMatrix(diagonal.Length, diagonal.Length);
            m.SetDiagonal(diagonal);
            return m;
        }

        /// <summary>
        /// Create a new sparse matrix with the diagonal as a copy of the given array.
        /// This new matrix will be independent from the array.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        public static SparseMatrix OfDiagonalArray(int rows, int columns, float[] diagonal)
        {
            var m = new SparseMatrix(rows, columns);
            m.SetDiagonal(diagonal);
            return m;
        }

        /// <summary>
        /// Create a new sparse matrix and initialize each value to the same provided value.
        /// </summary>
        public static SparseMatrix Create(int rows, int columns, float value)
        {
            if (value == 0f) return new SparseMatrix(rows, columns);
            return new SparseMatrix(SparseCompressedRowMatrixStorage<float>.OfValue(rows, columns, value));
        }

        /// <summary>
        /// Create a new sparse matrix and initialize each value using the provided init function.
        /// </summary>
        public static SparseMatrix Create(int rows, int columns, Func<int, int, float> init)
        {
            return new SparseMatrix(SparseCompressedRowMatrixStorage<float>.OfInit(rows, columns, init));
        }

        /// <summary>
        /// Create a new diagonal sparse matrix and initialize each diagonal value to the same provided value.
        /// </summary>
        public static SparseMatrix CreateDiagonal(int rows, int columns, float value)
        {
            if (value == 0f) return new SparseMatrix(rows, columns);
            return new SparseMatrix(SparseCompressedRowMatrixStorage<float>.OfDiagonalInit(rows, columns, i => value));
        }

        /// <summary>
        /// Create a new diagonal sparse matrix and initialize each diagonal value using the provided init function.
        /// </summary>
        public static SparseMatrix CreateDiagonal(int rows, int columns, Func<int, float> init)
        {
            return new SparseMatrix(SparseCompressedRowMatrixStorage<float>.OfDiagonalInit(rows, columns, init));
        }

        /// <summary>
        /// Create a new square sparse identity matrix where each diagonal value is set to One.
        /// </summary>
        public static SparseMatrix CreateIdentity(int order)
        {
            return new SparseMatrix(SparseCompressedRowMatrixStorage<float>.OfDiagonalInit(order, order, i => One));
        }

        /// <summary>
        /// Returns a new matrix containing the lower triangle of this matrix.
        /// </summary>
        /// <returns>The lower triangle of this matrix.</returns>
        public override Matrix<float> LowerTriangle()
        {
            var result = Build.SameAs(this);
            LowerTriangleImpl(result);
            return result;
        }

        /// <summary>
        /// Puts the lower triangle of this matrix into the result matrix.
        /// </summary>
        /// <param name="result">Where to store the lower triangle.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="result"/> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException">If the result matrix's dimensions are not the same as this matrix.</exception>
        public override void LowerTriangle(Matrix<float> result)
        {
            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            if (result.RowCount != RowCount || result.ColumnCount != ColumnCount)
            {
                throw DimensionsDontMatch<ArgumentException>(this, result, "result");
            }

            if (ReferenceEquals(this, result))
            {
                var tmp = Build.SameAs(result);
                LowerTriangle(tmp);
                tmp.CopyTo(result);
            }
            else
            {
                result.Clear();
                LowerTriangleImpl(result);
            }
        }

        /// <summary>
        /// Puts the lower triangle of this matrix into the result matrix.
        /// </summary>
        /// <param name="result">Where to store the lower triangle.</param>
        private void LowerTriangleImpl(Matrix<float> result)
        {
            var rowPointers = _storage.RowPointers;
            var columnIndices = _storage.ColumnIndices;
            var values = _storage.Values;

            for (var row = 0; row < result.RowCount; row++)
            {
                var endIndex = rowPointers[row + 1];
                for (var j = rowPointers[row]; j < endIndex; j++)
                {
                    if (row >= columnIndices[j])
                    {
                        result.At(row, columnIndices[j], values[j]);
                    }
                }
            }
        }

        /// <summary>
        /// Returns a new matrix containing the upper triangle of this matrix.
        /// </summary>
        /// <returns>The upper triangle of this matrix.</returns>
        public override Matrix<float> UpperTriangle()
        {
            var result = Build.SameAs(this);
            UpperTriangleImpl(result);
            return result;
        }

        /// <summary>
        /// Puts the upper triangle of this matrix into the result matrix.
        /// </summary>
        /// <param name="result">Where to store the lower triangle.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="result"/> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException">If the result matrix's dimensions are not the same as this matrix.</exception>
        public override void UpperTriangle(Matrix<float> result)
        {
            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            if (result.RowCount != RowCount || result.ColumnCount != ColumnCount)
            {
                throw DimensionsDontMatch<ArgumentException>(this, result, "result");
            }

            if (ReferenceEquals(this, result))
            {
                var tmp = Build.SameAs(result);
                UpperTriangle(tmp);
                tmp.CopyTo(result);
            }
            else
            {
                result.Clear();
                UpperTriangleImpl(result);
            }
        }

        /// <summary>
        /// Puts the upper triangle of this matrix into the result matrix.
        /// </summary>
        /// <param name="result">Where to store the lower triangle.</param>
        private void UpperTriangleImpl(Matrix<float> result)
        {
            var rowPointers = _storage.RowPointers;
            var columnIndices = _storage.ColumnIndices;
            var values = _storage.Values;

            for (var row = 0; row < result.RowCount; row++)
            {
                var endIndex = rowPointers[row + 1];
                for (var j = rowPointers[row]; j < endIndex; j++)
                {
                    if (row <= columnIndices[j])
                    {
                        result.At(row, columnIndices[j], values[j]);
                    }
                }
            }
        }

        /// <summary>
        /// Returns a new matrix containing the lower triangle of this matrix. The new matrix
        /// does not contain the diagonal elements of this matrix.
        /// </summary>
        /// <returns>The lower triangle of this matrix.</returns>
        public override Matrix<float> StrictlyLowerTriangle()
        {
            var result = Build.SameAs(this);
            StrictlyLowerTriangleImpl(result);
            return result;
        }

        /// <summary>
        /// Puts the strictly lower triangle of this matrix into the result matrix.
        /// </summary>
        /// <param name="result">Where to store the lower triangle.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="result"/> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException">If the result matrix's dimensions are not the same as this matrix.</exception>
        public override void StrictlyLowerTriangle(Matrix<float> result)
        {
            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            if (result.RowCount != RowCount || result.ColumnCount != ColumnCount)
            {
                throw DimensionsDontMatch<ArgumentException>(this, result, "result");
            }

            if (ReferenceEquals(this, result))
            {
                var tmp = Build.SameAs(result);
                StrictlyLowerTriangle(tmp);
                tmp.CopyTo(result);
            }
            else
            {
                result.Clear();
                StrictlyLowerTriangleImpl(result);
            }
        }

        /// <summary>
        /// Puts the strictly lower triangle of this matrix into the result matrix.
        /// </summary>
        /// <param name="result">Where to store the lower triangle.</param>
        private void StrictlyLowerTriangleImpl(Matrix<float> result)
        {
            var rowPointers = _storage.RowPointers;
            var columnIndices = _storage.ColumnIndices;
            var values = _storage.Values;

            for (var row = 0; row < result.RowCount; row++)
            {
                var endIndex = rowPointers[row + 1];
                for (var j = rowPointers[row]; j < endIndex; j++)
                {
                    if (row > columnIndices[j])
                    {
                        result.At(row, columnIndices[j], values[j]);
                    }
                }
            }
        }

        /// <summary>
        /// Returns a new matrix containing the upper triangle of this matrix. The new matrix
        /// does not contain the diagonal elements of this matrix.
        /// </summary>
        /// <returns>The upper triangle of this matrix.</returns>
        public override Matrix<float> StrictlyUpperTriangle()
        {
            var result = Build.SameAs(this);
            StrictlyUpperTriangleImpl(result);
            return result;
        }

        /// <summary>
        /// Puts the strictly upper triangle of this matrix into the result matrix.
        /// </summary>
        /// <param name="result">Where to store the lower triangle.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="result"/> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException">If the result matrix's dimensions are not the same as this matrix.</exception>
        public override void StrictlyUpperTriangle(Matrix<float> result)
        {
            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            if (result.RowCount != RowCount || result.ColumnCount != ColumnCount)
            {
                throw DimensionsDontMatch<ArgumentException>(this, result, "result");
            }

            if (ReferenceEquals(this, result))
            {
                var tmp = Build.SameAs(result);
                StrictlyUpperTriangle(tmp);
                tmp.CopyTo(result);
            }
            else
            {
                result.Clear();
                StrictlyUpperTriangleImpl(result);
            }
        }

        /// <summary>
        /// Puts the strictly upper triangle of this matrix into the result matrix.
        /// </summary>
        /// <param name="result">Where to store the lower triangle.</param>
        private void StrictlyUpperTriangleImpl(Matrix<float> result)
        {
            var rowPointers = _storage.RowPointers;
            var columnIndices = _storage.ColumnIndices;
            var values = _storage.Values;

            for (var row = 0; row < result.RowCount; row++)
            {
                var endIndex = rowPointers[row + 1];
                for (var j = rowPointers[row]; j < endIndex; j++)
                {
                    if (row < columnIndices[j])
                    {
                        result.At(row, columnIndices[j], values[j]);
                    }
                }
            }
        }

        /// <summary>
        /// Negate each element of this matrix and place the results into the result matrix.
        /// </summary>
        /// <param name="result">The result of the negation.</param>
        protected override void DoNegate(Matrix<float> result)
        {
            CopyTo(result);
            DoMultiply(-1, result);
        }

        /// <summary>Calculates the induced infinity norm of this matrix.</summary>
        /// <returns>The maximum absolute row sum of the matrix.</returns>
        public override double InfinityNorm()
        {
            var rowPointers = _storage.RowPointers;
            var values = _storage.Values;
            var norm = 0d;
            for (var i = 0; i < RowCount; i++)
            {
                // Get the begin / end index for the current row
                var startIndex = rowPointers[i];
                var endIndex = rowPointers[i + 1];

                // Get the values for the current row
                if (startIndex == endIndex)
                {
                    // Begin and end are equal. There are no values in the row, Move to the next row
                    continue;
                }

                var s = 0d;
                for (var j = startIndex; j < endIndex; j++)
                {
                    s += Math.Abs(values[j]);
                }
                norm = Math.Max(norm, s);
            }
            return norm;
        }

        /// <summary>Calculates the entry-wise Frobenius norm of this matrix.</summary>
        /// <returns>The square root of the sum of the squared values.</returns>
        public override double FrobeniusNorm()
        {
            var aat = (SparseCompressedRowMatrixStorage<float>) (this*Transpose()).Storage;
            var norm = 0d;
            for (var i = 0; i < aat.RowCount; i++)
            {
                // Get the begin / end index for the current row
                var startIndex = aat.RowPointers[i];
                var endIndex = aat.RowPointers[i + 1];

                // Get the values for the current row
                if (startIndex == endIndex)
                {
                    // Begin and end are equal. There are no values in the row, Move to the next row
                    continue;
                }

                for (var j = startIndex; j < endIndex; j++)
                {
                    if (i == aat.ColumnIndices[j])
                    {
                        norm += Math.Abs(aat.Values[j]);
                    }
                }
            }
            return Math.Sqrt(norm);
        }

        /// <summary>
        /// Adds another matrix to this matrix.
        /// </summary>
        /// <param name="other">The matrix to add to this matrix.</param>
        /// <param name="result">The matrix to store the result of the addition.</param>
        /// <exception cref="ArgumentNullException">If the other matrix is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If the two matrices don't have the same dimensions.</exception>
        protected override void DoAdd(Matrix<float> other, Matrix<float> result)
        {
            var sparseOther = other as SparseMatrix;
            var sparseResult = result as SparseMatrix;
            if (sparseOther == null || sparseResult == null)
            {
                base.DoAdd(other, result);
                return;
            }

            if (ReferenceEquals(this, other))
            {
                if (!ReferenceEquals(this, result))
                {
                    CopyTo(result);
                }

                Control.LinearAlgebraProvider.ScaleArray(2.0f, sparseResult._storage.Values, sparseResult._storage.Values);
                return;
            }

            SparseMatrix left;

            if (ReferenceEquals(sparseOther, sparseResult))
            {
                left = this;
            }
            else if (ReferenceEquals(this, sparseResult))
            {
                left = sparseOther;
            }
            else
            {
                CopyTo(sparseResult);
                left = sparseOther;
            }

            var leftStorage = left._storage;
            for (var i = 0; i < leftStorage.RowCount; i++)
            {
                var endIndex = leftStorage.RowPointers[i + 1];
                for (var j = leftStorage.RowPointers[i]; j < endIndex; j++)
                {
                    var columnIndex = leftStorage.ColumnIndices[j];
                    var resVal = leftStorage.Values[j] + result.At(i, columnIndex);
                    result.At(i, columnIndex, resVal);
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
        protected override void DoSubtract(Matrix<float> other, Matrix<float> result)
        {
            var sparseOther = other as SparseMatrix;
            var sparseResult = result as SparseMatrix;
            if (sparseOther == null || sparseResult == null)
            {
                base.DoSubtract(other, result);
                return;
            }

            if (ReferenceEquals(this, other))
            {
                result.Clear();
                return;
            }

            var otherStorage = sparseOther._storage;

            if (ReferenceEquals(this, sparseResult))
            {
                for (var i = 0; i < otherStorage.RowCount; i++)
                {
                    var endIndex = otherStorage.RowPointers[i + 1];
                    for (var j = otherStorage.RowPointers[i]; j < endIndex; j++)
                    {
                        var columnIndex = otherStorage.ColumnIndices[j];
                        var resVal = sparseResult.At(i, columnIndex) - otherStorage.Values[j];
                        result.At(i, columnIndex, resVal);
                    }
                }
            }
            else
            {
                if (!ReferenceEquals(sparseOther, sparseResult))
                {
                    sparseOther.CopyTo(sparseResult);
                }

                sparseResult.Negate(sparseResult);

                var rowPointers = _storage.RowPointers;
                var columnIndices = _storage.ColumnIndices;
                var values = _storage.Values;

                for (var i = 0; i < RowCount; i++)
                {
                    var endIndex = rowPointers[i + 1];
                    for (var j = rowPointers[i]; j < endIndex; j++)
                    {
                        var columnIndex = columnIndices[j];
                        var resVal = sparseResult.At(i, columnIndex) + values[j];
                        result.At(i, columnIndex, resVal);
                    }
                }
            }
        }

        /// <summary>
        /// Multiplies each element of the matrix by a scalar and places results into the result matrix.
        /// </summary>
        /// <param name="scalar">The scalar to multiply the matrix with.</param>
        /// <param name="result">The matrix to store the result of the multiplication.</param>
        protected override void DoMultiply(float scalar, Matrix<float> result)
        {
            if (scalar == 1.0)
            {
                CopyTo(result);
                return;
            }

            if (scalar == 0.0 || NonZerosCount == 0)
            {
                result.Clear();
                return;
            }

            var sparseResult = result as SparseMatrix;
            if (sparseResult == null)
            {
                result.Clear();

                var rowPointers = _storage.RowPointers;
                var columnIndices = _storage.ColumnIndices;
                var values = _storage.Values;

                for (var row = 0; row < RowCount; row++)
                {
                    var start = rowPointers[row];
                    var end = rowPointers[row + 1];

                    if (start == end)
                    {
                        continue;
                    }

                    for (var index = start; index < end; index++)
                    {
                        var column = columnIndices[index];
                        result.At(row, column, values[index] * scalar);
                    }
                }
            }
            else
            {
                if (!ReferenceEquals(this, result))
                {
                    CopyTo(sparseResult);
                }

                Control.LinearAlgebraProvider.ScaleArray(scalar, sparseResult._storage.Values, sparseResult._storage.Values);
            }
        }

        /// <summary>
        /// Multiplies this matrix with another matrix and places the results into the result matrix.
        /// </summary>
        /// <param name="other">The matrix to multiply with.</param>
        /// <param name="result">The result of the multiplication.</param>
        protected override void DoMultiply(Matrix<float> other, Matrix<float> result)
        {
            var sparseOther = other as SparseMatrix;
            var sparseResult = result as SparseMatrix;
            if (sparseOther != null && sparseResult != null)
            {
                DoMultiplySparse(sparseOther, sparseResult);
                return;
            }

            var diagonalOther = other.Storage as DiagonalMatrixStorage<float>;
            if (diagonalOther != null && sparseResult != null)
            {
                var diagonal = diagonalOther.Data;
                if (other.ColumnCount == other.RowCount)
                {
                    Storage.MapIndexedTo(result.Storage, (i, j, x) => x*diagonal[j], Zeros.AllowSkip, ExistingData.Clear);
                }
                else
                {
                    result.Storage.Clear();
                    Storage.MapSubMatrixIndexedTo(result.Storage, (i, j, x) => x*diagonal[j], 0, 0, RowCount, 0, 0, ColumnCount, Zeros.AllowSkip, ExistingData.AssumeZeros);
                }
                return;
            }

            result.Clear();
            var rowPointers = _storage.RowPointers;
            var columnIndices = _storage.ColumnIndices;
            var values = _storage.Values;
            var denseOther = other.Storage as DenseColumnMajorMatrixStorage<float>;
            if (denseOther != null)
            {
                // in this case we can directly address the underlying data-array
                for (var row = 0; row < RowCount; row++)
                {
                    var startIndex = rowPointers[row];
                    var endIndex = rowPointers[row + 1];

                    if (startIndex == endIndex)
                    {
                        continue;
                    }

                    for (var column = 0; column < other.ColumnCount; column++)
                    {
                        int otherColumnStartPosition = column * other.RowCount;
                        var sum = 0f;
                        for (var index = startIndex; index < endIndex; index++)
                        {
                            sum += values[index] * denseOther.Data[otherColumnStartPosition + columnIndices[index]];
                        }

                        result.At(row, column, sum);
                    }
                }
                return;
            }

            var columnVector = new DenseVector(other.RowCount);
            for (var row = 0; row < RowCount; row++)
            {
                var startIndex = rowPointers[row];
                var endIndex = rowPointers[row + 1];

                if (startIndex == endIndex)
                {
                    continue;
                }

                for (var column = 0; column < other.ColumnCount; column++)
                {
                    // Multiply row of matrix A on column of matrix B
                    other.Column(column, columnVector);

                    var sum = 0f;
                    for (var index = startIndex; index < endIndex; index++)
                    {
                        sum += values[index] * columnVector[columnIndices[index]];
                    }

                    result.At(row, column, sum);
                }
            }
        }

        void DoMultiplySparse(SparseMatrix other, SparseMatrix result)
        {
            result.Clear();

            var ax = _storage.Values;
            var ap = _storage.RowPointers;
            var ai = _storage.ColumnIndices;

            var bx = other._storage.Values;
            var bp = other._storage.RowPointers;
            var bi = other._storage.ColumnIndices;

            int rows = RowCount;
            int cols = other.ColumnCount;

            int[] cp = result._storage.RowPointers;

            var marker = new int[cols];
            for (int ib = 0; ib < cols; ib++)
            {
                marker[ib] = -1;
            }

            int count = 0;
            for (int i = 0; i < rows; i++)
            {
                // For each row of A
                for (int j = ap[i]; j < ap[i + 1]; j++)
                {
                    // Row number to be added
                    int a = ai[j];
                    for (int k = bp[a]; k < bp[a + 1]; k++)
                    {
                        int b = bi[k];
                        if (marker[b] != i)
                        {
                            marker[b] = i;
                            count++;
                        }
                    }
                }

                // Record non-zero count.
                cp[i + 1] = count;
            }

            var ci = new int[count];
            var cx = new float[count];

            for (int ib = 0; ib < cols; ib++)
            {
                marker[ib] = -1;
            }

            count = 0;
            for (int i = 0; i < rows; i++)
            {
                int rowStart = cp[i];
                for (int j = ap[i]; j < ap[i + 1]; j++)
                {
                    int a = ai[j];
                    float aEntry = ax[j];
                    for (int k = bp[a]; k < bp[a + 1]; k++)
                    {
                        int b = bi[k];
                        float bEntry = bx[k];
                        if (marker[b] < rowStart)
                        {
                            marker[b] = count;
                            ci[marker[b]] = b;
                            cx[marker[b]] = aEntry * bEntry;
                            count++;
                        }
                        else
                        {
                            cx[marker[b]] += aEntry * bEntry;
                        }
                    }
                }
            }

            result._storage.Values = cx;
            result._storage.ColumnIndices = ci;
            result._storage.Normalize();
        }

        /// <summary>
        /// Multiplies this matrix with a vector and places the results into the result vector.
        /// </summary>
        /// <param name="rightSide">The vector to multiply with.</param>
        /// <param name="result">The result of the multiplication.</param>
        protected override void DoMultiply(Vector<float> rightSide, Vector<float> result)
        {
            var rowPointers = _storage.RowPointers;
            var columnIndices = _storage.ColumnIndices;
            var values = _storage.Values;

            for (var row = 0; row < RowCount; row++)
            {
                var startIndex = rowPointers[row];
                var endIndex = rowPointers[row + 1];

                if (startIndex == endIndex)
                {
                    continue;
                }

                var sum = 0f;
                for (var index = startIndex; index < endIndex; index++)
                {
                    sum += values[index] * rightSide[columnIndices[index]];
                }

                result[row] = sum;
            }
        }

        /// <summary>
        /// Multiplies this matrix with transpose of another matrix and places the results into the result matrix.
        /// </summary>
        /// <param name="other">The matrix to multiply with.</param>
        /// <param name="result">The result of the multiplication.</param>
        protected override void DoTransposeAndMultiply(Matrix<float> other, Matrix<float> result)
        {
            var otherSparse = other as SparseMatrix;
            var resultSparse = result as SparseMatrix;

            if (otherSparse == null || resultSparse == null)
            {
                base.DoTransposeAndMultiply(other, result);
                return;
            }

            resultSparse.Clear();

            var rowPointers = _storage.RowPointers;
            var values = _storage.Values;

            var otherStorage = otherSparse._storage;

            for (var j = 0; j < RowCount; j++)
            {
                var startIndexOther = otherStorage.RowPointers[j];
                var endIndexOther = otherStorage.RowPointers[j + 1];

                if (startIndexOther == endIndexOther)
                {
                    continue;
                }

                for (var i = 0; i < RowCount; i++)
                {
                    // Multiply row of matrix A on row of matrix B

                    var startIndexThis = rowPointers[i];
                    var endIndexThis = rowPointers[i + 1];

                    if (startIndexThis == endIndexThis)
                    {
                        continue;
                    }

                    var sum = 0f;
                    for (var index = startIndexOther; index < endIndexOther; index++)
                    {
                        var ind = _storage.FindItem(i, otherStorage.ColumnIndices[index]);
                        if (ind >= 0)
                        {
                            sum += otherStorage.Values[index]*values[ind];
                        }
                    }

                    resultSparse._storage.At(i, j, sum + result.At(i, j));
                }
            }
        }

        /// <summary>
        /// Pointwise multiplies this matrix with another matrix and stores the result into the result matrix.
        /// </summary>
        /// <param name="other">The matrix to pointwise multiply with this one.</param>
        /// <param name="result">The matrix to store the result of the pointwise multiplication.</param>
        protected override void DoPointwiseMultiply(Matrix<float> other, Matrix<float> result)
        {
            result.Clear();

            var rowPointers = _storage.RowPointers;
            var columnIndices = _storage.ColumnIndices;
            var values = _storage.Values;

            for (var i = 0; i < RowCount; i++)
            {
                var endIndex = rowPointers[i + 1];
                for (var j = rowPointers[i]; j < endIndex; j++)
                {
                    var resVal = values[j]*other.At(i, columnIndices[j]);
                    if (resVal != 0f)
                    {
                        result.At(i, columnIndices[j], resVal);
                    }
                }
            }
        }

        /// <summary>
        /// Pointwise divide this matrix by another matrix and stores the result into the result matrix.
        /// </summary>
        /// <param name="divisor">The matrix to pointwise divide this one by.</param>
        /// <param name="result">The matrix to store the result of the pointwise division.</param>
        protected override void DoPointwiseDivide(Matrix<float> divisor, Matrix<float> result)
        {
            result.Clear();

            var rowPointers = _storage.RowPointers;
            var columnIndices = _storage.ColumnIndices;
            var values = _storage.Values;

            for (var i = 0; i < RowCount; i++)
            {
                var endIndex = rowPointers[i + 1];
                for (var j = rowPointers[i]; j < endIndex; j++)
                {
                    if (values[j] != 0f)
                    {
                        result.At(i, columnIndices[j], values[j]/divisor.At(i, columnIndices[j]));
                    }
                }
            }
        }

        public override void KroneckerProduct(Matrix<float> other, Matrix<float> result)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            if (result.RowCount != (RowCount*other.RowCount) || result.ColumnCount != (ColumnCount*other.ColumnCount))
            {
                throw DimensionsDontMatch<ArgumentOutOfRangeException>(this, other, result);
            }

            var rowPointers = _storage.RowPointers;
            var columnIndices = _storage.ColumnIndices;
            var values = _storage.Values;

            for (var i = 0; i < RowCount; i++)
            {
                var endIndex = rowPointers[i + 1];
                for (var j = rowPointers[i]; j < endIndex; j++)
                {
                    if (values[j] != 0f)
                    {
                        result.SetSubMatrix(i*other.RowCount, other.RowCount, columnIndices[j]*other.ColumnCount, other.ColumnCount, values[j]*other);
                    }
                }
            }
        }

        /// <summary>
        /// Computes the canonical modulus, where the result has the sign of the divisor,
        /// for the given divisor each element of the matrix.
        /// </summary>
        /// <param name="divisor">The scalar denominator to use.</param>
        /// <param name="result">Matrix to store the results in.</param>
        protected override void DoModulus(float divisor, Matrix<float> result)
        {
            var sparseResult = result as SparseMatrix;
            if (sparseResult == null)
            {
                base.DoModulus(divisor, result);
                return;
            }

            if (!ReferenceEquals(this, result))
            {
                CopyTo(result);
            }

            var resultStorage = sparseResult._storage;
            for (var index = 0; index < resultStorage.Values.Length; index++)
            {
                resultStorage.Values[index] = Euclid.Modulus(resultStorage.Values[index], divisor);
            }
        }

        /// <summary>
        /// Computes the remainder (% operator), where the result has the sign of the dividend,
        /// for the given divisor each element of the matrix.
        /// </summary>
        /// <param name="divisor">The scalar denominator to use.</param>
        /// <param name="result">Matrix to store the results in.</param>
        protected override void DoRemainder(float divisor, Matrix<float> result)
        {
            var sparseResult = result as SparseMatrix;
            if (sparseResult == null)
            {
                base.DoRemainder(divisor, result);
                return;
            }

            if (!ReferenceEquals(this, result))
            {
                CopyTo(result);
            }

            var resultStorage = sparseResult._storage;
            for (var index = 0; index < resultStorage.Values.Length; index++)
            {
                resultStorage.Values[index] %= divisor;
            }
        }

        /// <summary>
        /// Evaluates whether this matrix is symmetric.
        /// </summary>
        public override bool IsSymmetric()
        {
            if (RowCount != ColumnCount)
            {
                return false;
            }

            // todo: we might be able to speed this up by caching one half of the matrix
            var rowPointers = _storage.RowPointers;
            var columnIndices = _storage.ColumnIndices;
            var values = _storage.Values;

            for (var row = 0; row < RowCount; row++)
            {
                var start = rowPointers[row];
                var end = rowPointers[row + 1];

                if (start == end)
                {
                    continue;
                }

                for (var index = start; index < end; index++)
                {
                    var column = columnIndices[index];
                    var opposite = At(column, row);
                    if (!values[index].Equals(opposite))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Adds two matrices together and returns the results.
        /// </summary>
        /// <remarks>This operator will allocate new memory for the result. It will
        /// choose the representation of either <paramref name="leftSide"/> or <paramref name="rightSide"/> depending on which
        /// is denser.</remarks>
        /// <param name="leftSide">The left matrix to add.</param>
        /// <param name="rightSide">The right matrix to add.</param>
        /// <returns>The result of the addition.</returns>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="leftSide"/> and <paramref name="rightSide"/> don't have the same dimensions.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="leftSide"/> or <paramref name="rightSide"/> is <see langword="null" />.</exception>
        public static SparseMatrix operator +(SparseMatrix leftSide, SparseMatrix rightSide)
        {
            if (rightSide == null)
            {
                throw new ArgumentNullException("rightSide");
            }

            if (leftSide == null)
            {
                throw new ArgumentNullException("leftSide");
            }

            if (leftSide.RowCount != rightSide.RowCount || leftSide.ColumnCount != rightSide.ColumnCount)
            {
                throw DimensionsDontMatch<ArgumentException>(leftSide, rightSide);
            }

            return (SparseMatrix)leftSide.Add(rightSide);
        }

        /// <summary>
        /// Returns a <strong>Matrix</strong> containing the same values of <paramref name="rightSide"/>.
        /// </summary>
        /// <param name="rightSide">The matrix to get the values from.</param>
        /// <returns>A matrix containing a the same values as <paramref name="rightSide"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="rightSide"/> is <see langword="null" />.</exception>
        public static SparseMatrix operator +(SparseMatrix rightSide)
        {
            if (rightSide == null)
            {
                throw new ArgumentNullException("rightSide");
            }

            return (SparseMatrix)rightSide.Clone();
        }

        /// <summary>
        /// Subtracts two matrices together and returns the results.
        /// </summary>
        /// <remarks>This operator will allocate new memory for the result. It will
        /// choose the representation of either <paramref name="leftSide"/> or <paramref name="rightSide"/> depending on which
        /// is denser.</remarks>
        /// <param name="leftSide">The left matrix to subtract.</param>
        /// <param name="rightSide">The right matrix to subtract.</param>
        /// <returns>The result of the addition.</returns>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="leftSide"/> and <paramref name="rightSide"/> don't have the same dimensions.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="leftSide"/> or <paramref name="rightSide"/> is <see langword="null" />.</exception>
        public static SparseMatrix operator -(SparseMatrix leftSide, SparseMatrix rightSide)
        {
            if (rightSide == null)
            {
                throw new ArgumentNullException("rightSide");
            }

            if (leftSide == null)
            {
                throw new ArgumentNullException("leftSide");
            }

            if (leftSide.RowCount != rightSide.RowCount || leftSide.ColumnCount != rightSide.ColumnCount)
            {
                throw DimensionsDontMatch<ArgumentException>(leftSide, rightSide);
            }

            return (SparseMatrix)leftSide.Subtract(rightSide);
        }

        /// <summary>
        /// Negates each element of the matrix.
        /// </summary>
        /// <param name="rightSide">The matrix to negate.</param>
        /// <returns>A matrix containing the negated values.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="rightSide"/> is <see langword="null" />.</exception>
        public static SparseMatrix operator -(SparseMatrix rightSide)
        {
            if (rightSide == null)
            {
                throw new ArgumentNullException("rightSide");
            }

            return (SparseMatrix)rightSide.Negate();
        }

        /// <summary>
        /// Multiplies a <strong>Matrix</strong> by a constant and returns the result.
        /// </summary>
        /// <param name="leftSide">The matrix to multiply.</param>
        /// <param name="rightSide">The constant to multiply the matrix by.</param>
        /// <returns>The result of the multiplication.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="leftSide"/> is <see langword="null" />.</exception>
        public static SparseMatrix operator *(SparseMatrix leftSide, float rightSide)
        {
            if (leftSide == null)
            {
                throw new ArgumentNullException("leftSide");
            }

            return (SparseMatrix)leftSide.Multiply(rightSide);
        }

        /// <summary>
        /// Multiplies a <strong>Matrix</strong> by a constant and returns the result.
        /// </summary>
        /// <param name="leftSide">The matrix to multiply.</param>
        /// <param name="rightSide">The constant to multiply the matrix by.</param>
        /// <returns>The result of the multiplication.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="rightSide"/> is <see langword="null" />.</exception>
        public static SparseMatrix operator *(float leftSide, SparseMatrix rightSide)
        {
            if (rightSide == null)
            {
                throw new ArgumentNullException("rightSide");
            }

            return (SparseMatrix)rightSide.Multiply(leftSide);
        }

        /// <summary>
        /// Multiplies two matrices.
        /// </summary>
        /// <remarks>This operator will allocate new memory for the result. It will
        /// choose the representation of either <paramref name="leftSide"/> or <paramref name="rightSide"/> depending on which
        /// is denser.</remarks>
        /// <param name="leftSide">The left matrix to multiply.</param>
        /// <param name="rightSide">The right matrix to multiply.</param>
        /// <returns>The result of multiplication.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="leftSide"/> or <paramref name="rightSide"/> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException">If the dimensions of <paramref name="leftSide"/> or <paramref name="rightSide"/> don't conform.</exception>
        public static SparseMatrix operator *(SparseMatrix leftSide, SparseMatrix rightSide)
        {
            if (leftSide == null)
            {
                throw new ArgumentNullException("leftSide");
            }

            if (rightSide == null)
            {
                throw new ArgumentNullException("rightSide");
            }

            if (leftSide.ColumnCount != rightSide.RowCount)
            {
                throw DimensionsDontMatch<ArgumentException>(leftSide, rightSide);
            }

            return (SparseMatrix)leftSide.Multiply(rightSide);
        }

        /// <summary>
        /// Multiplies a <strong>Matrix</strong> and a Vector.
        /// </summary>
        /// <param name="leftSide">The matrix to multiply.</param>
        /// <param name="rightSide">The vector to multiply.</param>
        /// <returns>The result of multiplication.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="leftSide"/> or <paramref name="rightSide"/> is <see langword="null" />.</exception>
        public static SparseVector operator *(SparseMatrix leftSide, SparseVector rightSide)
        {
            if (leftSide == null)
            {
                throw new ArgumentNullException("leftSide");
            }

            return (SparseVector)leftSide.Multiply(rightSide);
        }

        /// <summary>
        /// Multiplies a Vector and a <strong>Matrix</strong>.
        /// </summary>
        /// <param name="leftSide">The vector to multiply.</param>
        /// <param name="rightSide">The matrix to multiply.</param>
        /// <returns>The result of multiplication.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="leftSide"/> or <paramref name="rightSide"/> is <see langword="null" />.</exception>
        public static SparseVector operator *(SparseVector leftSide, SparseMatrix rightSide)
        {
            if (rightSide == null)
            {
                throw new ArgumentNullException("rightSide");
            }

            return (SparseVector)rightSide.LeftMultiply(leftSide);
        }

        /// <summary>
        /// Multiplies a <strong>Matrix</strong> by a constant and returns the result.
        /// </summary>
        /// <param name="leftSide">The matrix to multiply.</param>
        /// <param name="rightSide">The constant to multiply the matrix by.</param>
        /// <returns>The result of the multiplication.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="leftSide"/> is <see langword="null" />.</exception>
        public static SparseMatrix operator %(SparseMatrix leftSide, float rightSide)
        {
            if (leftSide == null)
            {
                throw new ArgumentNullException("leftSide");
            }

            return (SparseMatrix)leftSide.Remainder(rightSide);
        }

        public override string ToTypeString()
        {
            return string.Format("SparseMatrix {0}x{1}-Single {2:P2} Filled", RowCount, ColumnCount, NonZerosCount / (RowCount * (double)ColumnCount));
        }
    }
}
