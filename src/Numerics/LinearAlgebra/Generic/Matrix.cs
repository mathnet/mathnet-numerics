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

namespace MathNet.Numerics.LinearAlgebra.Generic
{
    using Factorization;
    using Numerics;
    using Properties;
    using Storage;
    using System;
    using System.Collections.Generic;
    using System.Numerics;
    using System.Runtime;

    /// <summary>
    /// Defines the base class for <c>Matrix</c> classes.
    /// </summary>
    /// <typeparam name="T">Supported data types are <c>double</c>, <c>single</c>, <see cref="Complex"/>, and <see cref="Complex32"/>.</typeparam>
    [Serializable]
    public abstract partial class Matrix<T> :
        IFormattable, IEquatable<Matrix<T>>
#if !PORTABLE
        , ICloneable
#endif
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
        /// Constructs matrix from a list of column vectors.
        /// </summary>
        /// <param name="columnVectors">The vectors to construct the matrix from.</param>
        /// <returns>The matrix constructed from the list of column vectors.</returns>
        /// <remarks>Creates a matrix of size Max(<paramref name="columnVectors"/>[i].Count) x <paramref name="columnVectors"/>.Count</remarks>
        [Obsolete("Use DenseMatrix.OfColumns or SparseMatrix.OfColumns instead. Scheduled for removal in v3.0.")]
        public static Matrix<T> CreateFromColumns(IList<Vector<T>> columnVectors)
        {
            if (columnVectors == null)
            {
                throw new ArgumentNullException("columnVectors");
            }

            if (columnVectors.Count == 0)
            {
                throw new ArgumentOutOfRangeException("columnVectors");
            }

            var rows = columnVectors[0].Count;
            var columns = columnVectors.Count;

            for (var column = 1; column < columns; column++)
            {
                rows = Math.Max(rows, columnVectors[column].Count);
            }

            var matrix = columnVectors[0].CreateMatrix(rows, columns);
            for (var j = 0; j < columns; j++)
            {
                for (var i = 0; i < columnVectors[j].Count; i++)
                {
                    matrix.At(i, j, columnVectors[j].At(i));
                }
            }

            return matrix;
        }

        /// <summary>
        /// Constructs matrix from a list of  row vectors.
        /// </summary>
        /// <param name="rowVectors">The vectors to construct the matrix from.</param>
        /// <returns>The matrix constructed from the list of row vectors.</returns>
        /// <remarks>Creates a matrix of size Max(<paramref name="rowVectors"/>.Count) x <paramref name="rowVectors"/>[i].Count</remarks>
        [Obsolete("Use DenseMatrix.OfRows or SparseMatrix.OfRows instead. Scheduled for removal in v3.0.")]
        public static Matrix<T> CreateFromRows(IList<Vector<T>> rowVectors)
        {
            if (rowVectors == null)
            {
                throw new ArgumentNullException("rowVectors");
            }

            if (rowVectors.Count == 0)
            {
                throw new ArgumentOutOfRangeException("rowVectors");
            }

            var rows = rowVectors.Count;
            var columns = rowVectors[0].Count;

            for (var row = 1; row < rows; row++)
            {
                columns = Math.Max(columns, rowVectors[row].Count);
            }

            var matrix = rowVectors[0].CreateMatrix(rows, columns);
            for (var i = 0; i < rows; i++)
            {
                for (var j = 0; j < rowVectors[i].Count; j++)
                {
                    matrix.At(i, j, rowVectors[i].At(j));
                }
            }

            return matrix;
        }

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
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            //[MethodImpl(MethodImplOptions.AggressiveInlining)] .Net 4.5 only
            get { return Storage[row, column]; }

            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            //[MethodImpl(MethodImplOptions.AggressiveInlining)] .Net 4.5 only
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
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        //[MethodImpl(MethodImplOptions.AggressiveInlining)] .Net 4.5 only
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
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        //[MethodImpl(MethodImplOptions.AggressiveInlining)] .Net 4.5 only
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
        /// Sets all values of a column to zero.
        /// </summary>
        public void ClearColumn(int columnIndex)
        {
            if (columnIndex < 0 || columnIndex >= ColumnCount)
            {
                throw new ArgumentOutOfRangeException("columnIndex");
            }

            Storage.Clear(0,RowCount,columnIndex,1);
        }

        /// <summary>
        /// Sets all values of a row to zero.
        /// </summary>
        public void ClearRow(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= RowCount)
            {
                throw new ArgumentOutOfRangeException("rowIndex");
            }

            Storage.Clear(rowIndex, 1, 0, ColumnCount);
        }

        /// <summary>
        /// Sets all values of a submatrix to zero.
        /// </summary>
        public void ClearSubMatrix(int rowIndex, int rowCount, int columnIndex, int columnCount)
        {
            if (rowCount < 1)
            {
                throw new ArgumentOutOfRangeException("rowCount", Resources.ArgumentMustBePositive);
            }

            if (columnCount < 1)
            {
                throw new ArgumentOutOfRangeException("columnCount", Resources.ArgumentMustBePositive);
            }

            if (rowIndex + rowCount > RowCount || rowIndex < 0)
            {
                throw new ArgumentOutOfRangeException("rowIndex");
            }

            if (columnIndex + columnCount > ColumnCount || columnIndex < 0)
            {
                throw new ArgumentOutOfRangeException("columnIndex");
            }

            Storage.Clear(rowIndex, rowCount, columnIndex, columnCount);
        }

        /// <summary>
        /// Creates a clone of this instance.
        /// </summary>
        /// <returns>
        /// A clone of the instance.
        /// </returns>
        public Matrix<T> Clone()
        {
            var result = CreateMatrix(RowCount, ColumnCount);
            Storage.CopyToUnchecked(result.Storage, skipClearing: true);
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
                throw new ArgumentNullException("target");
            }

            Storage.CopyTo(target.Storage);
        }

        /// <summary>
        /// Creates a <strong>Matrix</strong> for the given number of rows and columns.
        /// </summary>
        /// <param name="numberOfRows">The number of rows.</param>
        /// <param name="numberOfColumns">The number of columns.</param>
        /// <param name="fullyMutable">True if all fields must be mutable (e.g. not a diagonal matrix).</param>
        /// <returns>
        /// A <strong>Matrix</strong> with the given dimensions.
        /// </returns>
        /// <remarks>
        /// Creates a matrix of the same matrix type as the current matrix.
        /// </remarks>
        public abstract Matrix<T> CreateMatrix(int numberOfRows, int numberOfColumns, bool fullyMutable = false);

        /// <summary>
        /// Creates a Vector with a the given dimension.
        /// </summary>
        /// <param name="size">The size of the vector.</param>
        /// <param name="fullyMutable">True if all fields must be mutable.</param>
        /// <returns>
        /// A Vector with the given dimension.
        /// </returns>
        /// <remarks>
        /// Creates a vector of the same type as the current matrix.
        /// </remarks>
        public abstract Vector<T> CreateVector(int size, bool fullyMutable = false);

        /// <summary>
        /// Copies a row into an Vector.
        /// </summary>
        /// <param name="index">The row to copy.</param>
        /// <returns>A Vector containing the copied elements.</returns>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="index"/> is negative,
        /// or greater than or equal to the number of rows.</exception>
        public Vector<T> Row(int index)
        {
            if (index >= RowCount || index < 0)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            var ret = CreateVector(ColumnCount);
            Storage.CopySubRowToUnchecked(ret.Storage, index, 0, 0, ColumnCount);
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
                throw new ArgumentNullException("result");
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
            var ret = CreateVector(length);
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
                throw new ArgumentNullException("result");
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
            if (index >= ColumnCount || index < 0)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            var ret = CreateVector(RowCount);
            Storage.CopySubColumnToUnchecked(ret.Storage, index, 0, 0, RowCount);
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
                throw new ArgumentNullException("result");
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
            var ret = CreateVector(length);
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
                throw new ArgumentNullException("result");
            }

            Storage.CopySubColumnTo(result.Storage, columnIndex, rowIndex, 0, length);
        }

        /// <summary>
        /// Returns a new matrix containing the upper triangle of this matrix.
        /// </summary>
        /// <returns>The upper triangle of this matrix.</returns>
        public virtual Matrix<T> UpperTriangle()
        {
            var ret = CreateMatrix(RowCount, ColumnCount);

            for (var row = 0; row < RowCount; row++)
            {
                for (var column = row; column < ColumnCount; column++)
                {
                    ret.At(row, column, At(row, column));
                }
            }

            return ret;
        }

        /// <summary>
        /// Returns a new matrix containing the lower triangle of this matrix.
        /// </summary>
        /// <returns>The lower triangle of this matrix.</returns>
        public virtual Matrix<T> LowerTriangle()
        {
            var ret = CreateMatrix(RowCount, ColumnCount);

            for (var row = 0; row < RowCount; row++)
            {
                for (var column = 0; column <= row && column < ColumnCount; column++)
                {
                    ret.At(row, column, At(row, column));
                }
            }

            return ret;
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
                throw new ArgumentNullException("result");
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
                throw new ArgumentNullException("result");
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
            var target = CreateMatrix(rowCount, columnCount);
            Storage.CopySubMatrixTo(target.Storage, rowIndex, 0, rowCount, columnIndex, 0, columnCount, skipClearing: true);
            return target;
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
            var diagonal = CreateVector(min);

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
            var result = CreateMatrix(RowCount, ColumnCount);

            for (var row = 0; row < RowCount; row++)
            {
                for (var column = 0; column < row; column++)
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
                throw new ArgumentNullException("result");
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
            var result = CreateMatrix(RowCount, ColumnCount);

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
                throw new ArgumentNullException("result");
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
        public virtual Matrix<T> InsertColumn(int columnIndex, Vector<T> column)
        {
            if (column == null)
            {
                throw new ArgumentNullException("column");
            }

            if (columnIndex < 0 || columnIndex > ColumnCount)
            {
                throw new ArgumentOutOfRangeException("columnIndex");
            }

            if (column.Count != RowCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixSameRowDimension, "column");
            }

            var result = CreateMatrix(RowCount, ColumnCount + 1);

            for (var i = 0; i < columnIndex; i++)
            {
                result.SetColumn(i, Column(i));
            }

            result.SetColumn(columnIndex, column);

            for (var i = columnIndex + 1; i < ColumnCount + 1; i++)
            {
                result.SetColumn(i, Column(i - 1));
            }

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
                throw new ArgumentNullException("column");
            }

            column.Storage.CopyToColumn(Storage, columnIndex);
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
                throw new ArgumentNullException("column");
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
        public virtual Matrix<T> InsertRow(int rowIndex, Vector<T> row)
        {
            if (row == null)
            {
                throw new ArgumentNullException("row");
            }

            if (rowIndex < 0 || rowIndex > RowCount)
            {
                throw new ArgumentOutOfRangeException("rowIndex");
            }

            if (row.Count != ColumnCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixSameRowDimension, "row");
            }

            var result = CreateMatrix(RowCount + 1, ColumnCount);

            for (var i = 0; i < rowIndex; i++)
            {
                result.SetRow(i, Row(i));
            }

            result.SetRow(rowIndex, row);

            for (var i = rowIndex + 1; i < RowCount + 1; i++)
            {
                result.SetRow(i, Row(i - 1));
            }

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
                throw new ArgumentNullException("row");
            }

            row.Storage.CopyToRow(Storage, rowIndex);
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
                throw new ArgumentNullException("row");
            }

            new DenseVectorStorage<T>(row.Length, row).CopyToRow(Storage, rowIndex);
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
        /// <exception cref="ArgumentNullException">If <paramref name="subMatrix"/> is <see langword="null" /></exception>
        /// <item>the size of <paramref name="subMatrix"/> is not at least <paramref name="rowCount"/> x <paramref name="columnCount"/>.</item>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="rowCount"/> or <paramref name="columnCount"/>
        /// is not positive.</exception>
        public void SetSubMatrix(int rowIndex, int rowCount, int columnIndex, int columnCount, Matrix<T> subMatrix)
        {
            if (subMatrix == null)
            {
                throw new ArgumentNullException("subMatrix");
            }

            subMatrix.Storage.CopySubMatrixTo(Storage, 0, rowIndex, rowCount, 0, columnIndex, columnCount);
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
                throw new ArgumentNullException("source");
            }

            var min = Math.Min(RowCount, ColumnCount);

            if (source.Count != min)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "source");
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
                throw new ArgumentNullException("source");
            }

            var min = Math.Min(RowCount, ColumnCount);

            if (source.Length != min)
            {
                throw new ArgumentException(Resources.ArgumentArraysSameLength, "source");
            }

            for (var i = 0; i < min; i++)
            {
                At(i, i, source[i]);
            }
        }

        /// <summary>
        /// Returns the transpose of this matrix.
        /// </summary>
        /// <returns>The transpose of this matrix.</returns>
        public virtual Matrix<T> Transpose()
        {
            var ret = CreateMatrix(ColumnCount, RowCount);
            for (var j = 0; j < ColumnCount; j++)
            {
                for (var i = 0; i < RowCount; i++)
                {
                    ret.At(j, i, At(i, j));
                }
            }

            return ret;
        }

        /// <summary>
        /// Returns the conjugate transpose of this matrix.
        /// </summary>
        /// <returns>The conjugate transpose of this matrix.</returns>
        public abstract Matrix<T> ConjugateTranspose();

        /// <summary>
        /// Permute the rows of a matrix according to a permutation.
        /// </summary>
        /// <param name="p">The row permutation to apply to this matrix.</param>
        public virtual void PermuteRows(Permutation p)
        {
            if (p.Dimension != RowCount)
            {
                throw new ArgumentException(Resources.ArgumentArraysSameLength, "p");
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
                throw new ArgumentException(Resources.ArgumentArraysSameLength, "p");
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
        public Matrix<T> Append(Matrix<T> right)
        {
            if (right == null)
            {
                throw new ArgumentNullException("right");
            }

            if (right.RowCount != RowCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixSameRowDimension);
            }

            var result = CreateMatrix(RowCount, ColumnCount + right.ColumnCount, fullyMutable: true);
            Storage.CopySubMatrixToUnchecked(result.Storage, 0, 0, RowCount, 0, 0, ColumnCount, skipClearing: true);
            right.Storage.CopySubMatrixToUnchecked(result.Storage, 0, 0, right.RowCount, 0, ColumnCount, right.ColumnCount, skipClearing: true);
            return result;
        }

        /// <summary>
        /// Concatenates this matrix with the given matrix and places the result into the result matrix.
        /// </summary>
        /// <param name="right">The matrix to concatenate.</param>
        /// <param name="result">The combined matrix.</param>
        public void Append(Matrix<T> right, Matrix<T> result)
        {
            if (right == null)
            {
                throw new ArgumentNullException("right");
            }

            if (right.RowCount != RowCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixSameRowDimension);
            }

            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            if (result.ColumnCount != (ColumnCount + right.ColumnCount) || result.RowCount != RowCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixSameColumnDimension);
            }

            Storage.CopySubMatrixToUnchecked(result.Storage, 0, 0, RowCount, 0, 0, ColumnCount);
            right.Storage.CopySubMatrixToUnchecked(result.Storage, 0, 0, right.RowCount, 0, ColumnCount, right.ColumnCount);
        }

        /// <summary>
        /// Stacks this matrix on top of the given matrix and places the result into the result matrix.
        /// </summary>
        /// <param name="lower">The matrix to stack this matrix upon.</param>
        /// <returns>The combined matrix.</returns>
        /// <exception cref="ArgumentNullException">If lower is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException">If <strong>upper.Columns != lower.Columns</strong>.</exception>
        public Matrix<T> Stack(Matrix<T> lower)
        {
            if (lower == null)
            {
                throw new ArgumentNullException("lower");
            }

            if (lower.ColumnCount != ColumnCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixSameColumnDimension, "lower");
            }

            var result = CreateMatrix(RowCount + lower.RowCount, ColumnCount, fullyMutable: true);
            Storage.CopySubMatrixToUnchecked(result.Storage, 0, 0, RowCount, 0, 0, ColumnCount, skipClearing: true);
            lower.Storage.CopySubMatrixToUnchecked(result.Storage, 0, RowCount, lower.RowCount, 0, 0, lower.ColumnCount, skipClearing: true);
            return result;
        }

        /// <summary>
        /// Stacks this matrix on top of the given matrix and places the result into the result matrix.
        /// </summary>
        /// <param name="lower">The matrix to stack this matrix upon.</param>
        /// <param name="result">The combined matrix.</param>
        /// <exception cref="ArgumentNullException">If lower is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException">If <strong>upper.Columns != lower.Columns</strong>.</exception>
        public void Stack(Matrix<T> lower, Matrix<T> result)
        {
            if (lower == null)
            {
                throw new ArgumentNullException("lower");
            }

            if (lower.ColumnCount != ColumnCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixSameColumnDimension, "lower");
            }

            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            if (result.RowCount != (RowCount + lower.RowCount) || result.ColumnCount != ColumnCount)
            {
                throw DimensionsDontMatch<ArgumentException>(this, result, "result");
            }

            Storage.CopySubMatrixToUnchecked(result.Storage, 0, 0, RowCount, 0, 0, ColumnCount);
            lower.Storage.CopySubMatrixToUnchecked(result.Storage, 0, RowCount, lower.RowCount, 0, 0, lower.ColumnCount);
        }

        /// <summary>
        /// Diagonally stacks his matrix on top of the given matrix. The new matrix is a M-by-N matrix,
        /// where M = this.Rows + lower.Rows and N = this.Columns + lower.Columns.
        /// The values of off the off diagonal matrices/blocks are set to zero.
        /// </summary>
        /// <param name="lower">The lower, right matrix.</param>
        /// <exception cref="ArgumentNullException">If lower is <see langword="null" />.</exception>
        /// <returns>the combined matrix</returns>
        public Matrix<T> DiagonalStack(Matrix<T> lower)
        {
            if (lower == null)
            {
                throw new ArgumentNullException("lower");
            }

            var result = CreateMatrix(RowCount + lower.RowCount, ColumnCount + lower.ColumnCount, fullyMutable: true);
            Storage.CopySubMatrixToUnchecked(result.Storage, 0, 0, RowCount, 0, 0, ColumnCount);
            lower.Storage.CopySubMatrixToUnchecked(result.Storage, 0, RowCount, lower.RowCount, 0, ColumnCount, lower.ColumnCount);
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
        public void DiagonalStack(Matrix<T> lower, Matrix<T> result)
        {
            if (lower == null)
            {
                throw new ArgumentNullException("lower");
            }

            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            if (result.RowCount != RowCount + lower.RowCount || result.ColumnCount != ColumnCount + lower.ColumnCount)
            {
                throw DimensionsDontMatch<ArgumentException>(this, result, "result");
            }

            Storage.CopySubMatrixToUnchecked(result.Storage, 0, 0, RowCount, 0, 0, ColumnCount);
            lower.Storage.CopySubMatrixToUnchecked(result.Storage, 0, RowCount, lower.RowCount, 0, ColumnCount, lower.ColumnCount);
        }

        /// <summary>Calculates the L1 norm.</summary>
        /// <returns>The L1 norm of the matrix.</returns>
        public abstract T L1Norm();

        /// <summary>Calculates the L2 norm.</summary>
        /// <returns>The L2 norm of the matrix.</returns>
        /// <remarks>For sparse matrices, the L2 norm is computed using a dense implementation of singular value decomposition.
        /// In a later release, it will be replaced with a sparse implementation.</remarks>
        public virtual T L2Norm()
        {
            return Svd<T>.Create(this, false).Norm2;
        }

        /// <summary>Calculates the Frobenius norm of this matrix.</summary>
        /// <returns>The Frobenius norm of this matrix.</returns>
        public abstract T FrobeniusNorm();

        /// <summary>Calculates the infinity norm of this matrix.</summary>
        /// <returns>The infinity norm of this matrix.</returns>
        public abstract T InfinityNorm();

        /// <summary>
        /// Gets a value indicating whether this matrix is symmetric.
        /// </summary>
        public virtual bool IsSymmetric
        {
            get
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
        }

        /// <summary>
        /// Returns an <see cref="IEnumerator{T}"/> that enumerates over the matrix columns.
        /// </summary>
        /// <returns>An <see cref="IEnumerator{T}"/> that enumerates over the matrix columns</returns>
        /// <seealso cref="IEnumerator{T}"/>
        public virtual IEnumerable<Tuple<int, Vector<T>>> ColumnEnumerator()
        {
            for (var i = 0; i < ColumnCount; i++)
            {
                yield return new Tuple<int, Vector<T>>(i, Column(i));
            }
        }

        /// <summary>
        /// Returns an <see cref="IEnumerator{T}"/> that enumerates the requested matrix columns.
        /// </summary>
        /// <param name="index">The column to start enumerating over.</param>
        /// <param name="length">The number of columns to enumerating over.</param>
        /// <returns>An <see cref="IEnumerator{T}"/> that enumerates over requested matrix columns.</returns>
        /// <seealso cref="IEnumerator{T}"/>
        /// <exception cref="ArgumentOutOfRangeException">If:
        /// <list><item><paramref name="index"/> is negative,
        /// or greater than or equal to the number of columns.</item>
        /// <item><c>(index + length) &gt;= Columns.</c></item></list>
        /// </exception>
        /// <exception cref="ArgumentException">If <paramref name="length"/> is not positive.</exception>
        public virtual IEnumerable<Tuple<int, Vector<T>>> ColumnEnumerator(int index, int length)
        {
            if (index >= ColumnCount || index < 0)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            if (index + length > ColumnCount)
            {
                throw new ArgumentOutOfRangeException("length");
            }

            if (length < 1)
            {
                throw new ArgumentException(Resources.ArgumentMustBePositive, "length");
            }

            var maxIndex = index + length;
            for (var i = index; i < maxIndex; i++)
            {
                yield return new Tuple<int, Vector<T>>(i, Column(i));
            }
        }

        /// <summary>
        /// Returns an <see cref="IEnumerator{T}"/> that enumerates the requested matrix rows.
        /// </summary>
        /// <param name="index">The row to start enumerating over.</param>
        /// <param name="length">The number of rows to enumerating over.</param>
        /// <returns>An <see cref="IEnumerator{T}"/> that enumerates over requested matrix rows.</returns>
        /// <seealso cref="IEnumerator{T}"/>
        /// <exception cref="ArgumentOutOfRangeException">If:
        /// <list><item><paramref name="index"/> is negative,
        /// or greater than or equal to the number of rows.</item>
        /// <item><c>(index + length) &gt;= Rows.</c></item></list></exception>
        /// <exception cref="ArgumentException">If <paramref name="length"/> is not positive.</exception>
        public virtual IEnumerable<Tuple<int, Vector<T>>> RowEnumerator(int index, int length)
        {
            if (index >= RowCount || index < 0)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            if (index + length > RowCount)
            {
                throw new ArgumentOutOfRangeException("length");
            }

            if (length < 1)
            {
                throw new ArgumentException(Resources.ArgumentMustBePositive, "length");
            }

            var maxi = index + length;
            for (var i = index; i < maxi; i++)
            {
                yield return new Tuple<int, Vector<T>>(i, Row(i));
            }
        }

        /// <summary>
        /// Returns an <see cref="IEnumerator{T}"/> that enumerates over the matrix rows.
        /// </summary>
        /// <returns>An <see cref="IEnumerator{T}"/> that enumerates over the matrix rows</returns>
        /// <seealso cref="IEnumerator{T}"/>
        public virtual IEnumerable<Tuple<int, Vector<T>>> RowEnumerator()
        {
            for (var i = 0; i < RowCount; i++)
            {
                yield return new Tuple<int, Vector<T>>(i, Row(i));
            }
        }

        /// <summary>
        /// Iterates throw each element in the matrix (row-wise).
        /// </summary>
        /// <returns>The value at the current iteration along with its position (row, column, value).</returns>
        public virtual IEnumerable<Tuple<int, int, T>> IndexedEnumerator()
        {
            for (var row = 0; row < RowCount; row++)
            {
                for (var column = 0; column < ColumnCount; column++)
                {
                    yield return new Tuple<int, int, T>(row, column, At(row, column));
                }
            }
        }

        /// <summary>
        /// Returns this matrix as a multidimensional array.
        /// </summary>
        /// <returns>A multidimensional containing the values of this matrix.</returns>
        public T[,] ToArray()
        {
            return Storage.ToArray();
        }

        /// <summary>
        /// Returns the matrix's elements as an array with the data laid out column-wise.
        /// </summary>
        /// <example><pre>
        /// 1, 2, 3
        /// 4, 5, 6  will be returned as  1, 4, 7, 2, 5, 8, 3, 6, 9
        /// 7, 8, 9
        /// </pre></example>
        /// <returns>An array containing the matrix's elements.</returns>
        public T[] ToColumnWiseArray()
        {
            return Storage.ToColumnMajorArray();
        }

        /// <summary>
        /// Returns the matrix's elements as an array with the data laid row-wise.
        /// </summary>
        /// <example><pre>
        /// 1, 2, 3
        /// 4, 5, 6  will be returned as  1, 2, 3, 4, 5, 6, 7, 8, 9
        /// 7, 8, 9
        /// </pre></example>
        /// <returns>An array containing the matrix's elements.</returns>
        public T[] ToRowWiseArray()
        {
            return Storage.ToRowMajorArray();
        }

        /// <summary>
        /// Applies a function to each value of this matrix and replaces the value with its result.
        /// If forceMapZero is not set to true, zero values may or may not be skipped depending
        /// on the actual data storage implementation (relevant mostly for sparse matrices).
        /// </summary>
        public void MapInplace(Func<T, T> f, bool forceMapZeros = false)
        {
            Storage.MapInplace(f, forceMapZeros);
        }

        /// <summary>
        /// Applies a function to each value of this matrix and replaces the value with its result.
        /// The row and column indices of each value (zero-based) are passed as first arguments to the function.
        /// If forceMapZero is not set to true, zero values may or may not be skipped depending
        /// on the actual data storage implementation (relevant mostly for sparse matrices).
        /// </summary>
        public void MapIndexedInplace(Func<int, int, T, T> f, bool forceMapZeros = false)
        {
            Storage.MapIndexedInplace(f, forceMapZeros);
        }
    }
}
