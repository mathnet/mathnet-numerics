// <copyright file="Matrix.cs" company="Math.NET">
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

using MathNet.Numerics.LinearAlgebra.Storage;
using MathNet.Numerics.Threading;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime;
using System.Runtime.CompilerServices;

namespace MathNet.Numerics.LinearAlgebra
{
    /// <summary>
    /// Defines the base class for <c>Matrix</c> classes.
    /// </summary>
    /// <typeparam name="T">Supported data types are <c>double</c>, <c>single</c>, <see cref="Complex"/>, and <see cref="Complex32"/>.</typeparam>
    [Serializable]
    [DebuggerTypeProxy(typeof(MatrixDebuggingView<>))]
    public abstract partial class Matrix<T> : IFormattable, IEquatable<Matrix<T>>, ICloneable
        where T : struct, IEquatable<T>, IFormattable
    {
        /// <summary>
        /// Initializes a new instance of the Matrix class.
        /// </summary>
        protected Matrix(MatrixStorage<T> storage)
        {
            Storage = storage;
            RowCount = storage.RowCount;
            ColumnCount = storage.ColumnCount;
        }

        public static readonly MatrixBuilder<T> Build = BuilderInstance<T>.Matrix;

        /// <summary>
        /// Gets the raw matrix data storage.
        /// </summary>
        public MatrixStorage<T> Storage { get; private set; }

        /// <summary>
        /// Gets the number of columns.
        /// </summary>
        /// <value>The number of columns.</value>
        public int ColumnCount { get; private set; }

        /// <summary>
        /// Gets the number of rows.
        /// </summary>
        /// <value>The number of rows.</value>
        public int RowCount { get; private set; }

        /// <summary>
        /// Gets or sets the value at the given row and column, with range checking.
        /// </summary>
        /// <param name="row">
        /// The row of the element.
        /// </param>
        /// <param name="column">
        /// The column of the element.
        /// </param>
        /// <value>The value to get or set.</value>
        /// <remarks>This method is ranged checked. <see cref="At(int,int)"/> and <see cref="At(int,int,T)"/>
        /// to get and set values without range checking.</remarks>
        public T this[int row, int column]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get { return Storage[row, column]; }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            set { Storage[row, column] = value; }
        }

        /// <summary>
        /// Retrieves the requested element without range checking.
        /// </summary>
        /// <param name="row">
        /// The row of the element.
        /// </param>
        /// <param name="column">
        /// The column of the element.
        /// </param>
        /// <returns>
        /// The requested element.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public T At(int row, int column)
        {
            return Storage.At(row, column);
        }

        /// <summary>
        /// Sets the value of the given element without range checking.
        /// </summary>
        /// <param name="row">
        /// The row of the element.
        /// </param>
        /// <param name="column">
        /// The column of the element.
        /// </param>
        /// <param name="value">
        /// The value to set the element to.
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public void At(int row, int column, T value)
        {
            Storage.At(row, column, value);
        }

        /// <summary>
        /// Sets all values to zero.
        /// </summary>
        public void Clear()
        {
            Storage.Clear();
        }

        /// <summary>
        /// Sets all values of a row to zero.
        /// </summary>
        public void ClearRow(int rowIndex)
        {
            if ((uint)rowIndex >= (uint)RowCount)
            {
                throw new ArgumentOutOfRangeException(nameof(rowIndex));
            }

            Storage.ClearUnchecked(rowIndex, 1, 0, ColumnCount);
        }

        /// <summary>
        /// Sets all values of a column to zero.
        /// </summary>
        public void ClearColumn(int columnIndex)
        {
            if ((uint)columnIndex >= (uint)ColumnCount)
            {
                throw new ArgumentOutOfRangeException(nameof(columnIndex));
            }

            Storage.ClearUnchecked(0, RowCount, columnIndex, 1);
        }

        /// <summary>
        /// Sets all values for all of the chosen rows to zero.
        /// </summary>
        public void ClearRows(params int[] rowIndices)
        {
            Storage.ClearRows(rowIndices);
        }

        /// <summary>
        /// Sets all values for all of the chosen columns to zero.
        /// </summary>
        public void ClearColumns(params int[] columnIndices)
        {
            Storage.ClearColumns(columnIndices);
        }

        /// <summary>
        /// Sets all values of a sub-matrix to zero.
        /// </summary>
        public void ClearSubMatrix(int rowIndex, int rowCount, int columnIndex, int columnCount)
        {
            Storage.Clear(rowIndex, rowCount, columnIndex, columnCount);
        }

        /// <summary>
        /// Set all values whose absolute value is smaller than the threshold to zero, in-place.
        /// </summary>
        public abstract void CoerceZero(double threshold);

        /// <summary>
        /// Set all values that meet the predicate to zero, in-place.
        /// </summary>
        public void CoerceZero(Func<T, bool> zeroPredicate)
        {
            MapInplace(x => zeroPredicate(x) ? Zero : x, Zeros.AllowSkip);
        }

        /// <summary>
        /// Creates a clone of this instance.
        /// </summary>
        /// <returns>
        /// A clone of the instance.
        /// </returns>
        public Matrix<T> Clone()
        {
            var result = Build.SameAs(this);
            Storage.CopyToUnchecked(result.Storage, ExistingData.AssumeZeros);
            return result;
        }

        /// <summary>
        /// Copies the elements of this matrix to the given matrix.
        /// </summary>
        /// <param name="target">
        /// The matrix to copy values into.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// If target is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If this and the target matrix do not have the same dimensions..
        /// </exception>
        public void CopyTo(Matrix<T> target)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            Storage.CopyTo(target.Storage);
        }

        /// <summary>
        /// Copies a row into an Vector.
        /// </summary>
        /// <param name="index">The row to copy.</param>
        /// <returns>A Vector containing the copied elements.</returns>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="index"/> is negative,
        /// or greater than or equal to the number of rows.</exception>
        public Vector<T> Row(int index)
        {
            if ((uint)index >= (uint)RowCount)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            var ret = Vector<T>.Build.SameAs(this, ColumnCount);
            Storage.CopySubRowToUnchecked(ret.Storage, index, 0, 0, ColumnCount, ExistingData.AssumeZeros);
            return ret;
        }

        /// <summary>
        /// Copies a row into to the given Vector.
        /// </summary>
        /// <param name="index">The row to copy.</param>
        /// <param name="result">The Vector to copy the row into.</param>
        /// <exception cref="ArgumentNullException">If the result vector is <see langword="null" />.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="index"/> is negative,
        /// or greater than or equal to the number of rows.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If <b>this.Columns != result.Count</b>.</exception>
        public void Row(int index, Vector<T> result)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            Storage.CopyRowTo(result.Storage, index);
        }

        /// <summary>
        /// Copies the requested row elements into a new Vector.
        /// </summary>
        /// <param name="rowIndex">The row to copy elements from.</param>
        /// <param name="columnIndex">The column to start copying from.</param>
        /// <param name="length">The number of elements to copy.</param>
        /// <returns>A Vector containing the requested elements.</returns>
        /// <exception cref="ArgumentOutOfRangeException">If:
        /// <list><item><paramref name="rowIndex"/> is negative,
        /// or greater than or equal to the number of rows.</item>
        /// <item><paramref name="columnIndex"/> is negative,
        /// or greater than or equal to the number of columns.</item>
        /// <item><c>(columnIndex + length) &gt;= Columns.</c></item></list></exception>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="length"/> is not positive.</exception>
        public Vector<T> Row(int rowIndex, int columnIndex, int length)
        {
            var ret = Vector<T>.Build.SameAs(this, length);
            Storage.CopySubRowTo(ret.Storage, rowIndex, columnIndex, 0, length);
            return ret;
        }

        /// <summary>
        /// Copies the requested row elements into a new Vector.
        /// </summary>
        /// <param name="rowIndex">The row to copy elements from.</param>
        /// <param name="columnIndex">The column to start copying from.</param>
        /// <param name="length">The number of elements to copy.</param>
        /// <param name="result">The Vector to copy the column into.</param>
        /// <exception cref="ArgumentNullException">If the result Vector is <see langword="null" />.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="rowIndex"/> is negative,
        /// or greater than or equal to the number of columns.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="columnIndex"/> is negative,
        /// or greater than or equal to the number of rows.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="columnIndex"/> + <paramref name="length"/>
        /// is greater than or equal to the number of rows.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="length"/> is not positive.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If <strong>result.Count &lt; length</strong>.</exception>
        public void Row(int rowIndex, int columnIndex, int length, Vector<T> result)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            Storage.CopySubRowTo(result.Storage, rowIndex, columnIndex, 0, length);
        }

        /// <summary>
        /// Copies a column into a new Vector>.
        /// </summary>
        /// <param name="index">The column to copy.</param>
        /// <returns>A Vector containing the copied elements.</returns>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="index"/> is negative,
        /// or greater than or equal to the number of columns.</exception>
        public Vector<T> Column(int index)
        {
            if ((uint)index >= (uint)ColumnCount)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            var ret = Vector<T>.Build.SameAs(this, RowCount);
            Storage.CopySubColumnToUnchecked(ret.Storage, index, 0, 0, RowCount, ExistingData.AssumeZeros);
            return ret;
        }

        /// <summary>
        /// Copies a column into to the given Vector.
        /// </summary>
        /// <param name="index">The column to copy.</param>
        /// <param name="result">The Vector to copy the column into.</param>
        /// <exception cref="ArgumentNullException">If the result Vector is <see langword="null" />.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="index"/> is negative,
        /// or greater than or equal to the number of columns.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If <b>this.Rows != result.Count</b>.</exception>
        public void Column(int index, Vector<T> result)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            Storage.CopyColumnTo(result.Storage, index);
        }

        /// <summary>
        /// Copies the requested column elements into a new Vector.
        /// </summary>
        /// <param name="columnIndex">The column to copy elements from.</param>
        /// <param name="rowIndex">The row to start copying from.</param>
        /// <param name="length">The number of elements to copy.</param>
        /// <returns>A Vector containing the requested elements.</returns>
        /// <exception cref="ArgumentOutOfRangeException">If:
        /// <list><item><paramref name="columnIndex"/> is negative,
        /// or greater than or equal to the number of columns.</item>
        /// <item><paramref name="rowIndex"/> is negative,
        /// or greater than or equal to the number of rows.</item>
        /// <item><c>(rowIndex + length) &gt;= Rows.</c></item></list>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="length"/> is not positive.</exception>
        public Vector<T> Column(int columnIndex, int rowIndex, int length)
        {
            var ret = Vector<T>.Build.SameAs(this, length);
            Storage.CopySubColumnTo(ret.Storage, columnIndex, rowIndex, 0, length);
            return ret;
        }

        /// <summary>
        /// Copies the requested column elements into the given vector.
        /// </summary>
        /// <param name="columnIndex">The column to copy elements from.</param>
        /// <param name="rowIndex">The row to start copying from.</param>
        /// <param name="length">The number of elements to copy.</param>
        /// <param name="result">The Vector to copy the column into.</param>
        /// <exception cref="ArgumentNullException">If the result Vector is <see langword="null" />.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="columnIndex"/> is negative,
        /// or greater than or equal to the number of columns.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="rowIndex"/> is negative,
        /// or greater than or equal to the number of rows.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="rowIndex"/> + <paramref name="length"/>
        /// is greater than or equal to the number of rows.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="length"/> is not positive.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If <strong>result.Count &lt; length</strong>.</exception>
        public void Column(int columnIndex, int rowIndex, int length, Vector<T> result)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            Storage.CopySubColumnTo(result.Storage, columnIndex, rowIndex, 0, length);
        }

        /// <summary>
        /// Returns a new matrix containing the upper triangle of this matrix.
        /// </summary>
        /// <returns>The upper triangle of this matrix.</returns>
        public virtual Matrix<T> UpperTriangle()
        {
            var result = Build.SameAs(this);
            for (var row = 0; row < RowCount; row++)
            {
                for (var column = row; column < ColumnCount; column++)
                {
                    result.At(row, column, At(row, column));
                }
            }
            return result;
        }

        /// <summary>
        /// Returns a new matrix containing the lower triangle of this matrix.
        /// </summary>
        /// <returns>The lower triangle of this matrix.</returns>
        public virtual Matrix<T> LowerTriangle()
        {
            var result = Build.SameAs(this);
            for (var row = 0; row < RowCount; row++)
            {
                for (var column = 0; column <= row && column < ColumnCount; column++)
                {
                    result.At(row, column, At(row, column));
                }
            }
            return result;
        }

        /// <summary>
        /// Puts the lower triangle of this matrix into the result matrix.
        /// </summary>
        /// <param name="result">Where to store the lower triangle.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="result"/> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException">If the result matrix's dimensions are not the same as this matrix.</exception>
        public virtual void LowerTriangle(Matrix<T> result)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            if (result.RowCount != RowCount || result.ColumnCount != ColumnCount)
            {
                throw DimensionsDontMatch<ArgumentException>(this, result, "result");
            }

            for (var row = 0; row < RowCount; row++)
            {
                for (var column = 0; column < ColumnCount; column++)
                {
                    result.At(row, column, row >= column ? At(row, column) : Zero);
                }
            }
        }

        /// <summary>
        /// Puts the upper triangle of this matrix into the result matrix.
        /// </summary>
        /// <param name="result">Where to store the lower triangle.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="result"/> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException">If the result matrix's dimensions are not the same as this matrix.</exception>
        public virtual void UpperTriangle(Matrix<T> result)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            if (result.RowCount != RowCount || result.ColumnCount != ColumnCount)
            {
                throw DimensionsDontMatch<ArgumentException>(this, result, "result");
            }

            for (var row = 0; row < RowCount; row++)
            {
                for (var column = 0; column < ColumnCount; column++)
                {
                    result.At(row, column, row <= column ? At(row, column) : Zero);
                }
            }
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
        public virtual Matrix<T> SubMatrix(int rowIndex, int rowCount, int columnIndex, int columnCount)
        {
            var result = Build.SameAs(this, rowCount, columnCount);
            Storage.CopySubMatrixTo(result.Storage, rowIndex, 0, rowCount, columnIndex, 0, columnCount, ExistingData.AssumeZeros);
            return result;
        }

        /// <summary>
        /// Returns the elements of the diagonal in a Vector.
        /// </summary>
        /// <returns>The elements of the diagonal.</returns>
        /// <remarks>For non-square matrices, the method returns Min(Rows, Columns) elements where
        /// i == j (i is the row index, and j is the column index).</remarks>
        public virtual Vector<T> Diagonal()
        {
            var min = Math.Min(RowCount, ColumnCount);
            var diagonal = Vector<T>.Build.SameAs(this, min);

            for (var i = 0; i < min; i++)
            {
                diagonal.At(i, At(i, i));
            }

            return diagonal;
        }

        /// <summary>
        /// Returns a new matrix containing the lower triangle of this matrix. The new matrix
        /// does not contain the diagonal elements of this matrix.
        /// </summary>
        /// <returns>The lower triangle of this matrix.</returns>
        public virtual Matrix<T> StrictlyLowerTriangle()
        {
            var result = Build.SameAs(this);
            for (var row = 0; row < RowCount; row++)
            {
                var columns = Math.Min(row, ColumnCount);
                for (var column = 0; column < columns; column++)
                {
                    result.At(row, column, At(row, column));
                }
            }
            return result;
        }

        /// <summary>
        /// Puts the strictly lower triangle of this matrix into the result matrix.
        /// </summary>
        /// <param name="result">Where to store the lower triangle.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="result"/> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException">If the result matrix's dimensions are not the same as this matrix.</exception>
        public virtual void StrictlyLowerTriangle(Matrix<T> result)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            if (result.RowCount != RowCount || result.ColumnCount != ColumnCount)
            {
                throw DimensionsDontMatch<ArgumentException>(this, result, "result");
            }

            for (var row = 0; row < RowCount; row++)
            {
                for (var column = 0; column < ColumnCount; column++)
                {
                    result.At(row, column, row > column ? At(row, column) : Zero);
                }
            }
        }

        /// <summary>
        /// Returns a new matrix containing the upper triangle of this matrix. The new matrix
        /// does not contain the diagonal elements of this matrix.
        /// </summary>
        /// <returns>The upper triangle of this matrix.</returns>
        public virtual Matrix<T> StrictlyUpperTriangle()
        {
            var result = Build.SameAs(this);
            for (var row = 0; row < RowCount; row++)
            {
                for (var column = row + 1; column < ColumnCount; column++)
                {
                    result.At(row, column, At(row, column));
                }
            }
            return result;
        }

        /// <summary>
        /// Puts the strictly upper triangle of this matrix into the result matrix.
        /// </summary>
        /// <param name="result">Where to store the lower triangle.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="result"/> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException">If the result matrix's dimensions are not the same as this matrix.</exception>
        public virtual void StrictlyUpperTriangle(Matrix<T> result)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            if (result.RowCount != RowCount || result.ColumnCount != ColumnCount)
            {
                throw DimensionsDontMatch<ArgumentException>(this, result, "result");
            }

            for (var row = 0; row < RowCount; row++)
            {
                for (var column = 0; column < ColumnCount; column++)
                {
                    result.At(row, column, row < column ? At(row, column) : Zero);
                }
            }
        }

        /// <summary>
        /// Creates a new matrix and inserts the given column at the given index.
        /// </summary>
        /// <param name="columnIndex">The index of where to insert the column.</param>
        /// <param name="column">The column to insert.</param>
        /// <returns>A new matrix with the inserted column.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="column "/> is <see langword="null" />. </exception>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="columnIndex"/> is &lt; zero or &gt; the number of columns.</exception>
        /// <exception cref="ArgumentException">If the size of <paramref name="column"/> != the number of rows.</exception>
        public Matrix<T> InsertColumn(int columnIndex, Vector<T> column)
        {
            if (column == null)
            {
                throw new ArgumentNullException(nameof(column));
            }

            if ((uint)columnIndex > (uint)ColumnCount)
            {
                throw new ArgumentOutOfRangeException(nameof(columnIndex));
            }

            if (column.Count != RowCount)
            {
                throw new ArgumentException("Matrix row dimensions must agree.", nameof(column));
            }

            var result = Build.SameAs(this, RowCount, ColumnCount + 1, fullyMutable: true);
            Storage.CopySubMatrixTo(result.Storage, 0, 0, RowCount, 0, 0, columnIndex, ExistingData.AssumeZeros);
            result.SetColumn(columnIndex, column);
            Storage.CopySubMatrixTo(result.Storage, 0, 0, RowCount, columnIndex, columnIndex + 1, ColumnCount - columnIndex, ExistingData.AssumeZeros);
            return result;
        }

        /// <summary>
        /// Creates a new matrix with the given column removed.
        /// </summary>
        /// <param name="columnIndex">The index of the column to remove.</param>
        /// <returns>A new matrix without the chosen column.</returns>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="columnIndex"/> is &lt; zero or &gt;= the number of columns.</exception>
        public Matrix<T> RemoveColumn(int columnIndex)
        {
            if ((uint)columnIndex >= (uint)ColumnCount)
            {
                throw new ArgumentOutOfRangeException(nameof(columnIndex));
            }

            var result = Build.SameAs(this, RowCount, ColumnCount - 1, fullyMutable: true);
            Storage.CopySubMatrixTo(result.Storage, 0, 0, RowCount, 0, 0, columnIndex, ExistingData.AssumeZeros);
            Storage.CopySubMatrixTo(result.Storage, 0, 0, RowCount, columnIndex + 1, columnIndex, ColumnCount - columnIndex - 1, ExistingData.AssumeZeros);
            return result;
        }

        /// <summary>
        /// Copies the values of the given Vector to the specified column.
        /// </summary>
        /// <param name="columnIndex">The column to copy the values to.</param>
        /// <param name="column">The vector to copy the values from.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="column"/> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="columnIndex"/> is less than zero,
        /// or greater than or equal to the number of columns.</exception>
        /// <exception cref="ArgumentException">If the size of <paramref name="column"/> does not
        /// equal the number of rows of this <strong>Matrix</strong>.</exception>
        public void SetColumn(int columnIndex, Vector<T> column)
        {
            if (column == null)
            {
                throw new ArgumentNullException(nameof(column));
            }

            column.Storage.CopyToColumn(Storage, columnIndex);
        }

        /// <summary>
        /// Copies the values of the given Vector to the specified sub-column.
        /// </summary>
        /// <param name="columnIndex">The column to copy the values to.</param>
        /// <param name="rowIndex">The row to start copying to.</param>
        /// <param name="length">The number of elements to copy.</param>
        /// <param name="column">The vector to copy the values from.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="column"/> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="columnIndex"/> is less than zero,
        /// or greater than or equal to the number of columns.</exception>
        /// <exception cref="ArgumentException">If the size of <paramref name="column"/> does not
        /// equal the number of rows of this <strong>Matrix</strong>.</exception>
        public void SetColumn(int columnIndex, int rowIndex, int length, Vector<T> column)
        {
            if (column == null)
            {
                throw new ArgumentNullException(nameof(column));
            }

            column.Storage.CopyToSubColumn(Storage, columnIndex, 0, rowIndex, length);
        }

        /// <summary>
        /// Copies the values of the given array to the specified column.
        /// </summary>
        /// <param name="columnIndex">The column to copy the values to.</param>
        /// <param name="column">The array to copy the values from.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="column"/> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="columnIndex"/> is less than zero,
        /// or greater than or equal to the number of columns.</exception>
        /// <exception cref="ArgumentException">If the size of <paramref name="column"/> does not
        /// equal the number of rows of this <strong>Matrix</strong>.</exception>
        /// <exception cref="ArgumentException">If the size of <paramref name="column"/> does not
        /// equal the number of rows of this <strong>Matrix</strong>.</exception>
        public void SetColumn(int columnIndex, T[] column)
        {
            if (column == null)
            {
                throw new ArgumentNullException(nameof(column));
            }

            new DenseVectorStorage<T>(column.Length, column).CopyToColumn(Storage, columnIndex);
        }

        /// <summary>
        /// Creates a new matrix and inserts the given row at the given index.
        /// </summary>
        /// <param name="rowIndex">The index of where to insert the row.</param>
        /// <param name="row">The row to insert.</param>
        /// <returns>A new matrix with the inserted column.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="row"/> is <see langword="null" />. </exception>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="rowIndex"/> is &lt; zero or &gt; the number of rows.</exception>
        /// <exception cref="ArgumentException">If the size of <paramref name="row"/> != the number of columns.</exception>
        public Matrix<T> InsertRow(int rowIndex, Vector<T> row)
        {
            if (row == null)
            {
                throw new ArgumentNullException(nameof(row));
            }

            if ((uint)rowIndex > (uint)RowCount)
            {
                throw new ArgumentOutOfRangeException(nameof(rowIndex));
            }

            if (row.Count != ColumnCount)
            {
                throw new ArgumentException("Matrix row dimensions must agree.", nameof(row));
            }

            var result = Build.SameAs(this, RowCount + 1, ColumnCount, fullyMutable: true);
            Storage.CopySubMatrixTo(result.Storage, 0, 0, rowIndex, 0, 0, ColumnCount, ExistingData.AssumeZeros);
            result.SetRow(rowIndex, row);
            Storage.CopySubMatrixTo(result.Storage, rowIndex, rowIndex+1, RowCount - rowIndex, 0, 0, ColumnCount, ExistingData.AssumeZeros);
            return result;
        }

        /// <summary>
        /// Creates a new matrix with the given row removed.
        /// </summary>
        /// <param name="rowIndex">The index of the row to remove.</param>
        /// <returns>A new matrix without the chosen row.</returns>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="rowIndex"/> is &lt; zero or &gt;= the number of rows.</exception>
        public Matrix<T> RemoveRow(int rowIndex)
        {
            if ((uint)rowIndex >= (uint)RowCount)
            {
                throw new ArgumentOutOfRangeException(nameof(rowIndex));
            }

            var result = Build.SameAs(this, RowCount - 1, ColumnCount, fullyMutable: true);
            Storage.CopySubMatrixTo(result.Storage, 0, 0, rowIndex, 0, 0, ColumnCount, ExistingData.AssumeZeros);
            Storage.CopySubMatrixTo(result.Storage, rowIndex + 1, rowIndex, RowCount - rowIndex - 1, 0, 0, ColumnCount, ExistingData.AssumeZeros);
            return result;
        }

        /// <summary>
        /// Copies the values of the given Vector to the specified row.
        /// </summary>
        /// <param name="rowIndex">The row to copy the values to.</param>
        /// <param name="row">The vector to copy the values from.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="row"/> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="rowIndex"/> is less than zero,
        /// or greater than or equal to the number of rows.</exception>
        /// <exception cref="ArgumentException">If the size of <paramref name="row"/> does not
        /// equal the number of columns of this <strong>Matrix</strong>.</exception>
        public void SetRow(int rowIndex, Vector<T> row)
        {
            if (row == null)
            {
                throw new ArgumentNullException(nameof(row));
            }

            row.Storage.CopyToRow(Storage, rowIndex);
        }

        /// <summary>
        /// Copies the values of the given Vector to the specified sub-row.
        /// </summary>
        /// <param name="rowIndex">The row to copy the values to.</param>
        /// <param name="columnIndex">The column to start copying to.</param>
        /// <param name="length">The number of elements to copy.</param>
        /// <param name="row">The vector to copy the values from.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="row"/> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="rowIndex"/> is less than zero,
        /// or greater than or equal to the number of rows.</exception>
        /// <exception cref="ArgumentException">If the size of <paramref name="row"/> does not
        /// equal the number of columns of this <strong>Matrix</strong>.</exception>
        public void SetRow(int rowIndex, int columnIndex, int length, Vector<T> row)
        {
            if (row == null)
            {
                throw new ArgumentNullException(nameof(row));
            }

            row.Storage.CopyToSubRow(Storage, rowIndex, 0, columnIndex, length);
        }


        /// <summary>
        /// Copies the values of the given array to the specified row.
        /// </summary>
        /// <param name="rowIndex">The row to copy the values to.</param>
        /// <param name="row">The array to copy the values from.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="row"/> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="rowIndex"/> is less than zero,
        /// or greater than or equal to the number of rows.</exception>
        /// <exception cref="ArgumentException">If the size of <paramref name="row"/> does not
        /// equal the number of columns of this <strong>Matrix</strong>.</exception>
        public void SetRow(int rowIndex, T[] row)
        {
            if (row == null)
            {
                throw new ArgumentNullException(nameof(row));
            }

            new DenseVectorStorage<T>(row.Length, row).CopyToRow(Storage, rowIndex);
        }

        /// <summary>
        /// Copies the values of a given matrix into a region in this matrix.
        /// </summary>
        /// <param name="rowIndex">The row to start copying to.</param>
        /// <param name="columnIndex">The column to start copying to.</param>
        /// <param name="subMatrix">The sub-matrix to copy from.</param>
        /// <exception cref="ArgumentOutOfRangeException">If: <list><item><paramref name="rowIndex"/> is
        /// negative, or greater than or equal to the number of rows.</item>
        /// <item><paramref name="columnIndex"/> is negative, or greater than or equal to the number
        /// of columns.</item>
        /// <item><c>(columnIndex + columnLength) &gt;= Columns</c></item>
        /// <item><c>(rowIndex + rowLength) &gt;= Rows</c></item></list></exception>
        public void SetSubMatrix(int rowIndex, int columnIndex, Matrix<T> subMatrix)
        {
            subMatrix.Storage.CopySubMatrixTo(Storage, 0, rowIndex, subMatrix.RowCount, 0, columnIndex, subMatrix.ColumnCount);
        }

        /// <summary>
        /// Copies the values of a given matrix into a region in this matrix.
        /// </summary>
        /// <param name="rowIndex">The row to start copying to.</param>
        /// <param name="rowCount">The number of rows to copy. Must be positive.</param>
        /// <param name="columnIndex">The column to start copying to.</param>
        /// <param name="columnCount">The number of columns to copy. Must be positive.</param>
        /// <param name="subMatrix">The sub-matrix to copy from.</param>
        /// <exception cref="ArgumentOutOfRangeException">If: <list><item><paramref name="rowIndex"/> is
        /// negative, or greater than or equal to the number of rows.</item>
        /// <item><paramref name="columnIndex"/> is negative, or greater than or equal to the number
        /// of columns.</item>
        /// <item><c>(columnIndex + columnLength) &gt;= Columns</c></item>
        /// <item><c>(rowIndex + rowLength) &gt;= Rows</c></item></list></exception>
        /// <item>the size of <paramref name="subMatrix"/> is not at least <paramref name="rowCount"/> x <paramref name="columnCount"/>.</item>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="rowCount"/> or <paramref name="columnCount"/>
        /// is not positive.</exception>
        public void SetSubMatrix(int rowIndex, int rowCount, int columnIndex, int columnCount, Matrix<T> subMatrix)
        {
            subMatrix.Storage.CopySubMatrixTo(Storage, 0, rowIndex, rowCount, 0, columnIndex, columnCount);
        }

        /// <summary>
        /// Copies the values of a given matrix into a region in this matrix.
        /// </summary>
        /// <param name="rowIndex">The row to start copying to.</param>
        /// <param name="sorceRowIndex">The row of the sub-matrix to start copying from.</param>
        /// <param name="rowCount">The number of rows to copy. Must be positive.</param>
        /// <param name="columnIndex">The column to start copying to.</param>
        /// <param name="sourceColumnIndex">The column of the sub-matrix to start copying from.</param>
        /// <param name="columnCount">The number of columns to copy. Must be positive.</param>
        /// <param name="subMatrix">The sub-matrix to copy from.</param>
        /// <exception cref="ArgumentOutOfRangeException">If: <list><item><paramref name="rowIndex"/> is
        /// negative, or greater than or equal to the number of rows.</item>
        /// <item><paramref name="columnIndex"/> is negative, or greater than or equal to the number
        /// of columns.</item>
        /// <item><c>(columnIndex + columnLength) &gt;= Columns</c></item>
        /// <item><c>(rowIndex + rowLength) &gt;= Rows</c></item></list></exception>
        /// <item>the size of <paramref name="subMatrix"/> is not at least <paramref name="rowCount"/> x <paramref name="columnCount"/>.</item>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="rowCount"/> or <paramref name="columnCount"/>
        /// is not positive.</exception>
        public void SetSubMatrix(int rowIndex, int sorceRowIndex, int rowCount, int columnIndex, int sourceColumnIndex, int columnCount, Matrix<T> subMatrix)
        {
            subMatrix.Storage.CopySubMatrixTo(Storage, sorceRowIndex, rowIndex, rowCount, sourceColumnIndex, columnIndex, columnCount);
        }

        /// <summary>
        /// Copies the values of the given Vector to the diagonal.
        /// </summary>
        /// <param name="source">The vector to copy the values from. The length of the vector should be
        /// Min(Rows, Columns).</param>
        /// <exception cref="ArgumentNullException">If <paramref name="source"/> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException">If the length of <paramref name="source"/> does not
        /// equal Min(Rows, Columns).</exception>
        /// <remarks>For non-square matrices, the elements of <paramref name="source"/> are copied to
        /// this[i,i].</remarks>
        public virtual void SetDiagonal(Vector<T> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            var min = Math.Min(RowCount, ColumnCount);

            if (source.Count != min)
            {
                throw new ArgumentException("All vectors must have the same dimensionality.", nameof(source));
            }

            for (var i = 0; i < min; i++)
            {
                At(i, i, source.At(i));
            }
        }

        /// <summary>
        /// Copies the values of the given array to the diagonal.
        /// </summary>
        /// <param name="source">The array to copy the values from. The length of the vector should be
        /// Min(Rows, Columns).</param>
        /// <exception cref="ArgumentNullException">If <paramref name="source"/> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException">If the length of <paramref name="source"/> does not
        /// equal Min(Rows, Columns).</exception>
        /// <remarks>For non-square matrices, the elements of <paramref name="source"/> are copied to
        /// this[i,i].</remarks>
        public virtual void SetDiagonal(T[] source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            var min = Math.Min(RowCount, ColumnCount);

            if (source.Length != min)
            {
                throw new ArgumentException("The array arguments must have the same length.", nameof(source));
            }

            for (var i = 0; i < min; i++)
            {
                At(i, i, source[i]);
            }
        }

        /// <summary>
        /// Creates a new matrix with the desired size and copies this matrix to it.
        /// Values which no longer exist in the new matrix are ignored, new values are set to zero.
        /// </summary>
        /// <param name="rowCount">The number of rows of the new matrix.</param>
        /// <param name="columnCount">The number of columns of the new matrix.</param>
        /// <returns>A new matrix with the desired rows and columns.</returns>
        public Matrix<T> Resize(int rowCount, int columnCount)
        {
            var result = Build.SameAs(this, rowCount, columnCount, fullyMutable: true);
            Storage.CopySubMatrixTo(result.Storage, 0, 0, Math.Min(RowCount, rowCount), 0, 0, Math.Min(ColumnCount, columnCount), ExistingData.AssumeZeros);
            return result;
        }

        /// <summary>
        /// Returns the transpose of this matrix.
        /// </summary>
        /// <returns>The transpose of this matrix.</returns>
        public Matrix<T> Transpose()
        {
            var result = Build.SameAs(this, ColumnCount, RowCount);
            Storage.TransposeToUnchecked(result.Storage, ExistingData.AssumeZeros);
            return result;
        }

        /// <summary>
        /// Puts the transpose of this matrix into the result matrix.
        /// </summary>
        public void Transpose(Matrix<T> result)
        {
            Storage.TransposeTo(result.Storage, ExistingData.Clear);
        }

        /// <summary>
        /// Returns the conjugate transpose of this matrix.
        /// </summary>
        /// <returns>The conjugate transpose of this matrix.</returns>
        public abstract Matrix<T> ConjugateTranspose();

        /// <summary>
        /// Puts the conjugate transpose of this matrix into the result matrix.
        /// </summary>
        public abstract void ConjugateTranspose(Matrix<T> result);

        /// <summary>
        /// Permute the rows of a matrix according to a permutation.
        /// </summary>
        /// <param name="p">The row permutation to apply to this matrix.</param>
        public virtual void PermuteRows(Permutation p)
        {
            if (p.Dimension != RowCount)
            {
                throw new ArgumentException("The array arguments must have the same length.", nameof(p));
            }

            // Get a sequence of inversions from the permutation.
            var inv = p.ToInversions();

            for (var i = 0; i < inv.Length; i++)
            {
                if (inv[i] != i)
                {
                    var q = inv[i];
                    for (var j = 0; j < ColumnCount; j++)
                    {
                        var temp = At(q, j);
                        At(q, j, At(i, j));
                        At(i, j, temp);
                    }
                }
            }
        }

        /// <summary>
        /// Permute the columns of a matrix according to a permutation.
        /// </summary>
        /// <param name="p">The column permutation to apply to this matrix.</param>
        public virtual void PermuteColumns(Permutation p)
        {
            if (p.Dimension != ColumnCount)
            {
                throw new ArgumentException("The array arguments must have the same length.", nameof(p));
            }

            // Get a sequence of inversions from the permutation.
            var inv = p.ToInversions();

            for (var i = 0; i < inv.Length; i++)
            {
                if (inv[i] != i)
                {
                    var q = inv[i];
                    for (var j = 0; j < RowCount; j++)
                    {
                        var temp = At(j, q);
                        At(j, q, At(j, i));
                        At(j, i, temp);
                    }
                }
            }
        }

        /// <summary>
        ///  Concatenates this matrix with the given matrix.
        /// </summary>
        /// <param name="right">The matrix to concatenate.</param>
        /// <returns>The combined matrix.</returns>
        /// <seealso cref="Stack(Matrix{T})"/>
        /// <seealso cref="DiagonalStack(Matrix{T})"/>
        public Matrix<T> Append(Matrix<T> right)
        {
            if (right == null)
            {
                throw new ArgumentNullException(nameof(right));
            }

            if (right.RowCount != RowCount)
            {
                throw new ArgumentException("Matrix row dimensions must agree.");
            }

            var result = Build.SameAs(this, right, RowCount, ColumnCount + right.ColumnCount, fullyMutable: true);
            Storage.CopySubMatrixToUnchecked(result.Storage, 0, 0, RowCount, 0, 0, ColumnCount, ExistingData.AssumeZeros);
            right.Storage.CopySubMatrixToUnchecked(result.Storage, 0, 0, right.RowCount, 0, ColumnCount, right.ColumnCount, ExistingData.AssumeZeros);
            return result;
        }

        /// <summary>
        /// Concatenates this matrix with the given matrix and places the result into the result matrix.
        /// </summary>
        /// <param name="right">The matrix to concatenate.</param>
        /// <param name="result">The combined matrix.</param>
        /// <seealso cref="Stack(Matrix{T}, Matrix{T})"/>
        /// <seealso cref="DiagonalStack(Matrix{T}, Matrix{T})"/>
        public void Append(Matrix<T> right, Matrix<T> result)
        {
            if (right == null)
            {
                throw new ArgumentNullException(nameof(right));
            }

            if (right.RowCount != RowCount)
            {
                throw new ArgumentException("Matrix row dimensions must agree.");
            }

            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            if (result.ColumnCount != (ColumnCount + right.ColumnCount) || result.RowCount != RowCount)
            {
                throw new ArgumentException("Matrix column dimensions must agree.");
            }

            Storage.CopySubMatrixToUnchecked(result.Storage, 0, 0, RowCount, 0, 0, ColumnCount, ExistingData.Clear);
            right.Storage.CopySubMatrixToUnchecked(result.Storage, 0, 0, right.RowCount, 0, ColumnCount, right.ColumnCount, ExistingData.Clear);
        }

        /// <summary>
        /// Stacks this matrix on top of the given matrix and places the result into the result matrix.
        /// </summary>
        /// <param name="lower">The matrix to stack this matrix upon.</param>
        /// <returns>The combined matrix.</returns>
        /// <exception cref="ArgumentNullException">If lower is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException">If <strong>upper.Columns != lower.Columns</strong>.</exception>
        /// <seealso cref="Append(Matrix{T})"/>
        /// <seealso cref="DiagonalStack(Matrix{T})"/>
        public Matrix<T> Stack(Matrix<T> lower)
        {
            if (lower == null)
            {
                throw new ArgumentNullException(nameof(lower));
            }

            if (lower.ColumnCount != ColumnCount)
            {
                throw new ArgumentException("Matrix column dimensions must agree.", nameof(lower));
            }

            var result = Build.SameAs(this, lower, RowCount + lower.RowCount, ColumnCount, fullyMutable: true);
            Storage.CopySubMatrixToUnchecked(result.Storage, 0, 0, RowCount, 0, 0, ColumnCount, ExistingData.AssumeZeros);
            lower.Storage.CopySubMatrixToUnchecked(result.Storage, 0, RowCount, lower.RowCount, 0, 0, lower.ColumnCount, ExistingData.AssumeZeros);
            return result;
        }

        /// <summary>
        /// Stacks this matrix on top of the given matrix and places the result into the result matrix.
        /// </summary>
        /// <param name="lower">The matrix to stack this matrix upon.</param>
        /// <param name="result">The combined matrix.</param>
        /// <exception cref="ArgumentNullException">If lower is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException">If <strong>upper.Columns != lower.Columns</strong>.</exception>
        /// <seealso cref="Append(Matrix{T}, Matrix{T})"/>
        /// <seealso cref="DiagonalStack(Matrix{T}, Matrix{T})"/>
        public void Stack(Matrix<T> lower, Matrix<T> result)
        {
            if (lower == null)
            {
                throw new ArgumentNullException(nameof(lower));
            }

            if (lower.ColumnCount != ColumnCount)
            {
                throw new ArgumentException("Matrix column dimensions must agree.", nameof(lower));
            }

            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            if (result.RowCount != (RowCount + lower.RowCount) || result.ColumnCount != ColumnCount)
            {
                throw DimensionsDontMatch<ArgumentException>(this, result, "result");
            }

            Storage.CopySubMatrixToUnchecked(result.Storage, 0, 0, RowCount, 0, 0, ColumnCount, ExistingData.Clear);
            lower.Storage.CopySubMatrixToUnchecked(result.Storage, 0, RowCount, lower.RowCount, 0, 0, lower.ColumnCount, ExistingData.Clear);
        }

        /// <summary>
        /// Diagonally stacks his matrix on top of the given matrix. The new matrix is a M-by-N matrix,
        /// where M = this.Rows + lower.Rows and N = this.Columns + lower.Columns.
        /// The values of off the off diagonal matrices/blocks are set to zero.
        /// </summary>
        /// <param name="lower">The lower, right matrix.</param>
        /// <exception cref="ArgumentNullException">If lower is <see langword="null" />.</exception>
        /// <returns>the combined matrix</returns>
        /// <seealso cref="Stack(Matrix{T})"/>
        /// <seealso cref="Append(Matrix{T})"/>
        public Matrix<T> DiagonalStack(Matrix<T> lower)
        {
            if (lower == null)
            {
                throw new ArgumentNullException(nameof(lower));
            }

            var result = Build.SameAs(this, lower, RowCount + lower.RowCount, ColumnCount + lower.ColumnCount, RowCount != ColumnCount);
            Storage.CopySubMatrixToUnchecked(result.Storage, 0, 0, RowCount, 0, 0, ColumnCount, ExistingData.AssumeZeros);
            lower.Storage.CopySubMatrixToUnchecked(result.Storage, 0, RowCount, lower.RowCount, 0, ColumnCount, lower.ColumnCount, ExistingData.AssumeZeros);
            return result;
        }

        /// <summary>
        /// Diagonally stacks his matrix on top of the given matrix and places the combined matrix into the result matrix.
        /// </summary>
        /// <param name="lower">The lower, right matrix.</param>
        /// <param name="result">The combined matrix</param>
        /// <exception cref="ArgumentNullException">If lower is <see langword="null" />.</exception>
        /// <exception cref="ArgumentNullException">If the result matrix is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException">If the result matrix's dimensions are not (this.Rows + lower.rows) x (this.Columns + lower.Columns).</exception>
        /// <seealso cref="Stack(Matrix{T}, Matrix{T})"/>
        /// <seealso cref="Append(Matrix{T}, Matrix{T})"/>
        public void DiagonalStack(Matrix<T> lower, Matrix<T> result)
        {
            if (lower == null)
            {
                throw new ArgumentNullException(nameof(lower));
            }

            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            if (result.RowCount != RowCount + lower.RowCount || result.ColumnCount != ColumnCount + lower.ColumnCount)
            {
                throw DimensionsDontMatch<ArgumentException>(this, result, "result");
            }

            Storage.CopySubMatrixToUnchecked(result.Storage, 0, 0, RowCount, 0, 0, ColumnCount, ExistingData.Clear);
            lower.Storage.CopySubMatrixToUnchecked(result.Storage, 0, RowCount, lower.RowCount, 0, ColumnCount, lower.ColumnCount, ExistingData.Clear);
        }

        /// <summary>
        /// Evaluates whether this matrix is symmetric.
        /// </summary>
        public virtual bool IsSymmetric()
        {
            if (RowCount != ColumnCount)
            {
                return false;
            }

            for (var row = 0; row < RowCount; row++)
            {
                for (var column = row + 1; column < ColumnCount; column++)
                {
                    if (!At(row, column).Equals(At(column, row)))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Evaluates whether this matrix is Hermitian (conjugate symmetric).
        /// </summary>
        public abstract bool IsHermitian();

        /// <summary>
        /// Returns this matrix as a multidimensional array.
        /// The returned array will be independent from this matrix.
        /// A new memory block will be allocated for the array.
        /// </summary>
        /// <returns>A multidimensional containing the values of this matrix.</returns>
        public T[,] ToArray()
        {
            return Storage.ToArray();
        }

        /// <summary>
        /// Returns the matrix's elements as an array with the data laid out column by column (column major).
        /// The returned array will be independent from this matrix.
        /// A new memory block will be allocated for the array.
        /// </summary>
        /// <example><pre>
        /// 1, 2, 3
        /// 4, 5, 6  will be returned as  1, 4, 7, 2, 5, 8, 3, 6, 9
        /// 7, 8, 9
        /// </pre></example>
        /// <returns>An array containing the matrix's elements.</returns>
        /// <seealso cref="ToRowMajorArray"/>
        /// <seealso cref="Enumerate(Zeros)"/>
        public T[] ToColumnMajorArray()
        {
            return Storage.ToColumnMajorArray();
        }

        /// <summary>
        /// Returns the matrix's elements as an array with the data laid row by row (row major).
        /// The returned array will be independent from this matrix.
        /// A new memory block will be allocated for the array.
        /// </summary>
        /// <example><pre>
        /// 1, 2, 3
        /// 4, 5, 6  will be returned as  1, 2, 3, 4, 5, 6, 7, 8, 9
        /// 7, 8, 9
        /// </pre></example>
        /// <returns>An array containing the matrix's elements.</returns>
        /// <seealso cref="ToColumnMajorArray"/>
        /// <seealso cref="Enumerate(Zeros)"/>
        public T[] ToRowMajorArray()
        {
            return Storage.ToRowMajorArray();
        }

        /// <summary>
        /// Returns this matrix as array of row arrays.
        /// The returned arrays will be independent from this matrix.
        /// A new memory block will be allocated for the arrays.
        /// </summary>
        public T[][] ToRowArrays()
        {
            return Storage.ToRowArrays();
        }

        /// <summary>
        /// Returns this matrix as array of column arrays.
        /// The returned arrays will be independent from this matrix.
        /// A new memory block will be allocated for the arrays.
        /// </summary>
        public T[][] ToColumnArrays()
        {
            return Storage.ToColumnArrays();
        }

        /// <summary>
        /// Returns the internal multidimensional array of this matrix if, and only if, this matrix is stored by such an array internally.
        /// Otherwise returns null. Changes to the returned array and the matrix will affect each other.
        /// Use ToArray instead if you always need an independent array.
        /// </summary>
        public T[,] AsArray()
        {
            return Storage.AsArray();
        }

        /// <summary>
        /// Returns the internal column by column (column major) array of this matrix if, and only if, this matrix is stored by such arrays internally.
        /// Otherwise returns null. Changes to the returned arrays and the matrix will affect each other.
        /// Use ToColumnMajorArray instead if you always need an independent array.
        /// </summary>
        /// <example><pre>
        /// 1, 2, 3
        /// 4, 5, 6  will be returned as  1, 4, 7, 2, 5, 8, 3, 6, 9
        /// 7, 8, 9
        /// </pre></example>
        /// <returns>An array containing the matrix's elements.</returns>
        /// <seealso cref="ToRowMajorArray"/>
        /// <seealso cref="Enumerate(Zeros)"/>
        public T[] AsColumnMajorArray()
        {
            return Storage.AsColumnMajorArray();
        }

        /// <summary>
        /// Returns the internal row by row (row major) array of this matrix if, and only if, this matrix is stored by such arrays internally.
        /// Otherwise returns null. Changes to the returned arrays and the matrix will affect each other.
        /// Use ToRowMajorArray instead if you always need an independent array.
        /// </summary>
        /// <example><pre>
        /// 1, 2, 3
        /// 4, 5, 6  will be returned as  1, 2, 3, 4, 5, 6, 7, 8, 9
        /// 7, 8, 9
        /// </pre></example>
        /// <returns>An array containing the matrix's elements.</returns>
        /// <seealso cref="ToColumnMajorArray"/>
        /// <seealso cref="Enumerate(Zeros)"/>
        public T[] AsRowMajorArray()
        {
            return Storage.AsRowMajorArray();
        }

        /// <summary>
        /// Returns the internal row arrays of this matrix if, and only if, this matrix is stored by such arrays internally.
        /// Otherwise returns null. Changes to the returned arrays and the matrix will affect each other.
        /// Use ToRowArrays instead if you always need an independent array.
        /// </summary>
        public T[][] AsRowArrays()
        {
            return Storage.AsRowArrays();
        }

        /// <summary>
        /// Returns the internal column arrays of this matrix if, and only if, this matrix is stored by such arrays internally.
        /// Otherwise returns null. Changes to the returned arrays and the matrix will affect each other.
        /// Use ToColumnArrays instead if you always need an independent array.
        /// </summary>
        public T[][] AsColumnArrays()
        {
            return Storage.AsColumnArrays();
        }

        /// <summary>
        /// Returns an IEnumerable that can be used to iterate through all values of the matrix.
        /// </summary>
        /// <remarks>
        /// The enumerator will include all values, even if they are zero.
        /// The ordering of the values is unspecified (not necessarily column-wise or row-wise).
        /// </remarks>
        public IEnumerable<T> Enumerate()
        {
            return Storage.Enumerate();
        }

        /// <summary>
        /// Returns an IEnumerable that can be used to iterate through all values of the matrix.
        /// </summary>
        /// <remarks>
        /// The enumerator will include all values, even if they are zero.
        /// The ordering of the values is unspecified (not necessarily column-wise or row-wise).
        /// </remarks>
        public IEnumerable<T> Enumerate(Zeros zeros)
        {
            switch (zeros)
            {
                case Zeros.AllowSkip:
                    return Storage.EnumerateNonZero();
                default:
                    return Storage.Enumerate();
            }
        }

        /// <summary>
        /// Returns an IEnumerable that can be used to iterate through all values of the matrix and their index.
        /// </summary>
        /// <remarks>
        /// The enumerator returns a Tuple with the first two values being the row and column index
        /// and the third value being the value of the element at that index.
        /// The enumerator will include all values, even if they are zero.
        /// </remarks>
        public IEnumerable<(int, int, T)> EnumerateIndexed()
        {
            return Storage.EnumerateIndexed();
        }

        /// <summary>
        /// Returns an IEnumerable that can be used to iterate through all values of the matrix and their index.
        /// </summary>
        /// <remarks>
        /// The enumerator returns a Tuple with the first two values being the row and column index
        /// and the third value being the value of the element at that index.
        /// The enumerator will include all values, even if they are zero.
        /// </remarks>
        public IEnumerable<(int, int, T)> EnumerateIndexed(Zeros zeros)
        {
            switch (zeros)
            {
                case Zeros.AllowSkip:
                    return Storage.EnumerateNonZeroIndexed();
                default:
                    return Storage.EnumerateIndexed();
            }
        }

        /// <summary>
        /// Returns an IEnumerable that can be used to iterate through all columns of the matrix.
        /// </summary>
        public IEnumerable<Vector<T>> EnumerateColumns()
        {
            for (var i = 0; i < ColumnCount; i++)
            {
                yield return Column(i);
            }
        }

        /// <summary>
        /// Returns an IEnumerable that can be used to iterate through a subset of all columns of the matrix.
        /// </summary>
        /// <param name="index">The column to start enumerating over.</param>
        /// <param name="length">The number of columns to enumerating over.</param>
        public IEnumerable<Vector<T>> EnumerateColumns(int index, int length)
        {
            var maxIndex = Math.Min(index + length, ColumnCount);
            for (var i = Math.Max(index, 0); i < maxIndex; i++)
            {
                yield return Column(i);
            }
        }

        /// <summary>
        /// Returns an IEnumerable that can be used to iterate through all columns of the matrix and their index.
        /// </summary>
        /// <remarks>
        /// The enumerator returns a Tuple with the first value being the column index
        /// and the second value being the value of the column at that index.
        /// </remarks>
        public IEnumerable<(int, Vector<T>)> EnumerateColumnsIndexed()
        {
            for (var i = 0; i < ColumnCount; i++)
            {
                yield return (i, Column(i));
            }
        }

        /// <summary>
        /// Returns an IEnumerable that can be used to iterate through a subset of all columns of the matrix and their index.
        /// </summary>
        /// <param name="index">The column to start enumerating over.</param>
        /// <param name="length">The number of columns to enumerating over.</param>
        /// <remarks>
        /// The enumerator returns a Tuple with the first value being the column index
        /// and the second value being the value of the column at that index.
        /// </remarks>
        public IEnumerable<(int, Vector<T>)> EnumerateColumnsIndexed(int index, int length)
        {
            var maxIndex = Math.Min(index + length, ColumnCount);
            for (var i = Math.Max(index, 0); i < maxIndex; i++)
            {
                yield return (i, Column(i));
            }
        }

        /// <summary>
        /// Returns an IEnumerable that can be used to iterate through all rows of the matrix.
        /// </summary>
        public IEnumerable<Vector<T>> EnumerateRows()
        {
            for (var i = 0; i < RowCount; i++)
            {
                yield return Row(i);
            }
        }

        /// <summary>
        /// Returns an IEnumerable that can be used to iterate through a subset of all rows of the matrix.
        /// </summary>
        /// <param name="index">The row to start enumerating over.</param>
        /// <param name="length">The number of rows to enumerating over.</param>
        public IEnumerable<Vector<T>> EnumerateRows(int index, int length)
        {
            var maxIndex = Math.Min(index + length, RowCount);
            for (var i = Math.Max(index, 0); i < maxIndex; i++)
            {
                yield return Row(i);
            }
        }

        /// <summary>
        /// Returns an IEnumerable that can be used to iterate through all rows of the matrix and their index.
        /// </summary>
        /// <remarks>
        /// The enumerator returns a Tuple with the first value being the row index
        /// and the second value being the value of the row at that index.
        /// </remarks>
        public IEnumerable<(int, Vector<T>)> EnumerateRowsIndexed()
        {
            for (var i = 0; i < RowCount; i++)
            {
                yield return (i, Row(i));
            }
        }

        /// <summary>
        /// Returns an IEnumerable that can be used to iterate through a subset of all rows of the matrix and their index.
        /// </summary>
        /// <param name="index">The row to start enumerating over.</param>
        /// <param name="length">The number of rows to enumerating over.</param>
        /// <remarks>
        /// The enumerator returns a Tuple with the first value being the row index
        /// and the second value being the value of the row at that index.
        /// </remarks>
        public IEnumerable<(int, Vector<T>)> EnumerateRowsIndexed(int index, int length)
        {
            var maxIndex = Math.Min(index + length, RowCount);
            for (var i = Math.Max(index, 0); i < maxIndex; i++)
            {
                yield return (i, Row(i));
            }
        }

        /// <summary>
        /// Applies a function to each value of this matrix and replaces the value with its result.
        /// If forceMapZero is not set to true, zero values may or may not be skipped depending
        /// on the actual data storage implementation (relevant mostly for sparse matrices).
        /// </summary>
        public void MapInplace(Func<T, T> f, Zeros zeros = Zeros.AllowSkip)
        {
            Storage.MapInplace(f, zeros);
        }

        /// <summary>
        /// Applies a function to each value of this matrix and replaces the value with its result.
        /// The row and column indices of each value (zero-based) are passed as first arguments to the function.
        /// If forceMapZero is not set to true, zero values may or may not be skipped depending
        /// on the actual data storage implementation (relevant mostly for sparse matrices).
        /// </summary>
        public void MapIndexedInplace(Func<int, int, T, T> f, Zeros zeros = Zeros.AllowSkip)
        {
            Storage.MapIndexedInplace(f, zeros);
        }

        /// <summary>
        /// Applies a function to each value of this matrix and replaces the value in the result matrix.
        /// If forceMapZero is not set to true, zero values may or may not be skipped depending
        /// on the actual data storage implementation (relevant mostly for sparse matrices).
        /// </summary>
        public void Map(Func<T, T> f, Matrix<T> result, Zeros zeros = Zeros.AllowSkip)
        {
            if (ReferenceEquals(this, result))
            {
                Storage.MapInplace(f, zeros);
            }
            else
            {
                Storage.MapTo(result.Storage, f, zeros, zeros == Zeros.Include ? ExistingData.AssumeZeros : ExistingData.Clear);
            }
        }

        /// <summary>
        /// Applies a function to each value of this matrix and replaces the value in the result matrix.
        /// The index of each value (zero-based) is passed as first argument to the function.
        /// If forceMapZero is not set to true, zero values may or may not be skipped depending
        /// on the actual data storage implementation (relevant mostly for sparse matrices).
        /// </summary>
        public void MapIndexed(Func<int, int, T, T> f, Matrix<T> result, Zeros zeros = Zeros.AllowSkip)
        {
            if (ReferenceEquals(this, result))
            {
                Storage.MapIndexedInplace(f, zeros);
            }
            else
            {
                Storage.MapIndexedTo(result.Storage, f, zeros, zeros == Zeros.Include ? ExistingData.AssumeZeros : ExistingData.Clear);
            }
        }

        /// <summary>
        /// Applies a function to each value of this matrix and replaces the value in the result matrix.
        /// If forceMapZero is not set to true, zero values may or may not be skipped depending
        /// on the actual data storage implementation (relevant mostly for sparse matrices).
        /// </summary>
        public void MapConvert<TU>(Func<T, TU> f, Matrix<TU> result, Zeros zeros = Zeros.AllowSkip)
            where TU : struct, IEquatable<TU>, IFormattable
        {
            Storage.MapTo(result.Storage, f, zeros, zeros == Zeros.Include ? ExistingData.AssumeZeros : ExistingData.Clear);
        }

        /// <summary>
        /// Applies a function to each value of this matrix and replaces the value in the result matrix.
        /// The index of each value (zero-based) is passed as first argument to the function.
        /// If forceMapZero is not set to true, zero values may or may not be skipped depending
        /// on the actual data storage implementation (relevant mostly for sparse matrices).
        /// </summary>
        public void MapIndexedConvert<TU>(Func<int, int, T, TU> f, Matrix<TU> result, Zeros zeros = Zeros.AllowSkip)
            where TU : struct, IEquatable<TU>, IFormattable
        {
            Storage.MapIndexedTo(result.Storage, f, zeros, zeros == Zeros.Include ? ExistingData.AssumeZeros : ExistingData.Clear);
        }

        /// <summary>
        /// Applies a function to each value of this matrix and returns the results as a new matrix.
        /// If forceMapZero is not set to true, zero values may or may not be skipped depending
        /// on the actual data storage implementation (relevant mostly for sparse matrices).
        /// </summary>
        public Matrix<TU> Map<TU>(Func<T, TU> f, Zeros zeros = Zeros.AllowSkip)
            where TU : struct, IEquatable<TU>, IFormattable
        {
            var result = Matrix<TU>.Build.SameAs(this, RowCount, ColumnCount, fullyMutable: zeros == Zeros.Include);
            Storage.MapToUnchecked(result.Storage, f, zeros, ExistingData.AssumeZeros);
            return result;
        }

        /// <summary>
        /// Applies a function to each value of this matrix and returns the results as a new matrix.
        /// The index of each value (zero-based) is passed as first argument to the function.
        /// If forceMapZero is not set to true, zero values may or may not be skipped depending
        /// on the actual data storage implementation (relevant mostly for sparse matrices).
        /// </summary>
        public Matrix<TU> MapIndexed<TU>(Func<int, int, T, TU> f, Zeros zeros = Zeros.AllowSkip)
            where TU : struct, IEquatable<TU>, IFormattable
        {
            var result = Matrix<TU>.Build.SameAs(this, RowCount, ColumnCount, fullyMutable: zeros == Zeros.Include);
            Storage.MapIndexedToUnchecked(result.Storage, f, zeros, ExistingData.AssumeZeros);
            return result;
        }

        /// <summary>
        /// For each row, applies a function f to each element of the row, threading an accumulator argument through the computation.
        /// Returns an array with the resulting accumulator states for each row.
        /// </summary>
        public TU[] FoldByRow<TU>(Func<TU, T, TU> f, TU state, Zeros zeros = Zeros.AllowSkip)
        {
            var result = new TU[RowCount];
            if (!EqualityComparer<TU>.Default.Equals(state, default(TU)))
            {
                CommonParallel.For(0, result.Length, 4096, (a, b) =>
                {
                    for (int i = a; i < b; i++)
                    {
                        result[i] = state;
                    }
                });
            }
            Storage.FoldByRowUnchecked(result, f, (x, _) => x, result, zeros);
            return result;
        }

        /// <summary>
        /// For each column, applies a function f to each element of the column, threading an accumulator argument through the computation.
        /// Returns an array with the resulting accumulator states for each column.
        /// </summary>
        public TU[] FoldByColumn<TU>(Func<TU, T, TU> f, TU state, Zeros zeros = Zeros.AllowSkip)
        {
            var result = new TU[ColumnCount];
            if (!EqualityComparer<TU>.Default.Equals(state, default(TU)))
            {
                CommonParallel.For(0, result.Length, 4096, (a, b) =>
                {
                    for (int i = a; i < b; i++)
                    {
                        result[i] = state;
                    }
                });
            }
            Storage.FoldByColumnUnchecked(result, f, (x, _) => x, result, zeros);
            return result;
        }

        /// <summary>
        /// Applies a function f to each row vector, threading an accumulator vector argument through the computation.
        /// Returns the resulting accumulator vector.
        /// </summary>
        public Vector<TU> FoldRows<TU>(Func<Vector<TU>, Vector<T>, Vector<TU>> f, Vector<TU> state)
            where TU : struct, IEquatable<TU>, IFormattable
        {
            foreach (var vector in EnumerateRows())
            {
                state = f(state, vector);
            }
            return state;
        }

        /// <summary>
        /// Applies a function f to each column vector, threading an accumulator vector argument through the computation.
        /// Returns the resulting accumulator vector.
        /// </summary>
        public Vector<TU> FoldColumns<TU>(Func<Vector<TU>, Vector<T>, Vector<TU>> f, Vector<TU> state)
            where TU : struct, IEquatable<TU>, IFormattable
        {
            foreach (var vector in EnumerateColumns())
            {
                state = f(state, vector);
            }
            return state;
        }

        /// <summary>
        /// Reduces all row vectors by applying a function between two of them, until only a single vector is left.
        /// </summary>
        public Vector<T> ReduceRows(Func<Vector<T>, Vector<T>, Vector<T>> f)
        {
            return EnumerateRows().Aggregate(f);
        }

        /// <summary>
        /// Reduces all column vectors by applying a function between two of them, until only a single vector is left.
        /// </summary>
        public Vector<T> ReduceColumns(Func<Vector<T>, Vector<T>, Vector<T>> f)
        {
            return EnumerateColumns().Aggregate(f);
        }

        /// <summary>
        /// Applies a function to each value pair of two matrices and replaces the value in the result vector.
        /// </summary>
        public void Map2(Func<T, T, T> f, Matrix<T> other, Matrix<T> result, Zeros zeros = Zeros.AllowSkip)
        {
            Storage.Map2To(result.Storage, other.Storage, f, zeros, ExistingData.Clear);
        }

        /// <summary>
        /// Applies a function to each value pair of two matrices and returns the results as a new vector.
        /// </summary>
        public Matrix<T> Map2(Func<T, T, T> f, Matrix<T> other, Zeros zeros = Zeros.AllowSkip)
        {
            var result = Build.SameAs(this);
            Storage.Map2To(result.Storage, other.Storage, f, zeros, ExistingData.AssumeZeros);
            return result;
        }

        /// <summary>
        /// Applies a function to update the status with each value pair of two matrices and returns the resulting status.
        /// </summary>
        public TState Fold2<TOther, TState>(Func<TState, T, TOther, TState> f, TState state, Matrix<TOther> other, Zeros zeros = Zeros.AllowSkip)
            where TOther : struct, IEquatable<TOther>, IFormattable
        {
            return Storage.Fold2(other.Storage, f, state, zeros);
        }

        /// <summary>
        /// Returns a tuple with the index and value of the first element satisfying a predicate, or null if none is found.
        /// Zero elements may be skipped on sparse data structures if allowed (default).
        /// </summary>
        public Tuple<int, int, T> Find(Func<T, bool> predicate, Zeros zeros = Zeros.AllowSkip)
        {
            return Storage.Find(predicate, zeros);
        }

        /// <summary>
        /// Returns a tuple with the index and values of the first element pair of two matrices of the same size satisfying a predicate, or null if none is found.
        /// Zero elements may be skipped on sparse data structures if allowed (default).
        /// </summary>
        public Tuple<int, int, T, TOther> Find2<TOther>(Func<T, TOther, bool> predicate, Matrix<TOther> other, Zeros zeros = Zeros.AllowSkip)
            where TOther : struct, IEquatable<TOther>, IFormattable
        {
            return Storage.Find2(other.Storage, predicate, zeros);
        }

        /// <summary>
        /// Returns true if at least one element satisfies a predicate.
        /// Zero elements may be skipped on sparse data structures if allowed (default).
        /// </summary>
        public bool Exists(Func<T, bool> predicate, Zeros zeros = Zeros.AllowSkip)
        {
            return Storage.Find(predicate, zeros) != null;
        }

        /// <summary>
        /// Returns true if at least one element pairs of two matrices of the same size satisfies a predicate.
        /// Zero elements may be skipped on sparse data structures if allowed (default).
        /// </summary>
        public bool Exists2<TOther>(Func<T, TOther, bool> predicate, Matrix<TOther> other, Zeros zeros = Zeros.AllowSkip)
            where TOther : struct, IEquatable<TOther>, IFormattable
        {
            return Storage.Find2(other.Storage, predicate, zeros) != null;
        }

        /// <summary>
        /// Returns true if all elements satisfy a predicate.
        /// Zero elements may be skipped on sparse data structures if allowed (default).
        /// </summary>
        public bool ForAll(Func<T, bool> predicate, Zeros zeros = Zeros.AllowSkip)
        {
            return Storage.Find(x => !predicate(x), zeros) == null;
        }

        /// <summary>
        /// Returns true if all element pairs of two matrices of the same size satisfy a predicate.
        /// Zero elements may be skipped on sparse data structures if allowed (default).
        /// </summary>
        public bool ForAll2<TOther>(Func<T, TOther, bool> predicate, Matrix<TOther> other, Zeros zeros = Zeros.AllowSkip)
            where TOther : struct, IEquatable<TOther>, IFormattable
        {
            return Storage.Find2(other.Storage, (x, y) => !predicate(x, y), zeros) == null;
        }
    }

    internal class MatrixDebuggingView<T>
        where T : struct, IEquatable<T>, IFormattable
    {
        private readonly Matrix<T> _matrix;

        public MatrixDebuggingView(Matrix<T> matrix)
        {
            _matrix = matrix;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public T[,] Items => _matrix.ToArray();
    }
}
