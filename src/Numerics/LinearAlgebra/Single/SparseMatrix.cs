// <copyright file="SparseMatrix.cs" company="Math.NET">
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

namespace MathNet.Numerics.LinearAlgebra.Single
{
    using System;
    using System.Collections.Generic;
    using Generic;
    using Properties;
    using Threading;
    
    /// <summary>
    /// A Matrix class with sparse storage. The underlying storage scheme is 3-array compressed-sparse-row (CSR) Format.
    /// <a href="http://en.wikipedia.org/wiki/Sparse_matrix#Compressed_sparse_row_.28CSR_or_CRS.29">Wikipedia - CSR</a>.
    /// </summary>
    [Serializable]
    public class SparseMatrix : Matrix 
    {
        /// <summary>
        /// The array containing the row indices of the existing rows. Element "j" of the array gives the index of the 
        /// element in the <see cref="_nonZeroValues"/> array that is first non-zero element in a row "j"
        /// </summary>
        private readonly int[] _rowIndex = new int[0];

        /// <summary>
        /// Array that contains the non-zero elements of matrix. Values of the non-zero elements of matrix are mapped into the values 
        /// array using the row-major storage mapping described in a compressed sparse row (CSR) format.
        /// </summary>
        private float[] _nonZeroValues = new float[0];
        
        /// <summary>
        /// Gets the number of non zero elements in the matrix.
        /// </summary>
        /// <value>The number of non zero elements.</value>
        public int NonZerosCount
        {
            get;
            private set;
        }
        
        /// <summary>
        /// An array containing the column indices of the non-zero values. Element "I" of the array 
        /// is the number of the column in matrix that contains the I-th value in the <see cref="_nonZeroValues"/> array.
        /// </summary>
        private int[] _columnIndices = new int[0];

        /// <summary>
        /// Initializes a new instance of the <see cref="SparseMatrix"/> class.
        /// </summary>
        /// <param name="rows">
        /// The number of rows.
        /// </param>
        /// <param name="columns">
        /// The number of columns.
        /// </param>
        public SparseMatrix(int rows, int columns) : base(rows, columns)
        {
            _rowIndex = new int[rows];
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="SparseMatrix"/> class. This matrix is square with a given size.
        /// </summary>
        /// <param name="order">the size of the square matrix.</param>
        /// <exception cref="ArgumentException">
        /// If <paramref name="order"/> is less than one.
        /// </exception>
        public SparseMatrix(int order) : this(order, order)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SparseMatrix"/> class with all entries set to a particular value.
        /// </summary>
        /// <param name="rows">
        /// The number of rows.
        /// </param>
        /// <param name="columns">
        /// The number of columns.
        /// </param>
        /// <param name="value">The value which we assign to each element of the matrix.</param>
        public SparseMatrix(int rows, int columns, float value) : this(rows, columns)
        {
            if (value == 0.0)
            {
                return;
            }

            NonZerosCount = rows * columns;
            _nonZeroValues = new float[NonZerosCount];
            _columnIndices = new int[NonZerosCount];

            for (int i = 0, j = 0; i < _nonZeroValues.Length; i++, j++)
            {
                // Reset column position to "0"
                if (j == columns)
                {
                    j = 0;
                }

                _nonZeroValues[i] = value;
                _columnIndices[i] = j;
            }
            
            // Set proper row pointers
            for (var i = 0; i < _rowIndex.Length; i++)
            {
                _rowIndex[i] = ((i + 1) * columns) - columns;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SparseMatrix"/> class from a one dimensional array. 
        /// </summary>
        /// <param name="rows">The number of rows.</param>
        /// <param name="columns">The number of columns.</param>
        /// <param name="array">The one dimensional array to create this matrix from. This array should store the matrix in column-major order. see: http://en.wikipedia.org/wiki/Column-major_order </param>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="array"/> length is less than <paramref name="rows"/> * <paramref name="columns"/>.
        /// </exception>
        public SparseMatrix(int rows, int columns, float[] array) : this(rows, columns)
        {
            if (rows * columns > array.Length)
            {
                throw new ArgumentOutOfRangeException(Resources.ArgumentMatrixDimensions);
            }

            for (var i = 0; i < rows; i++)
            {
                for (var j = 0; j < columns; j++)
                {
                    SetValueAt(i, j, array[i + (j * rows)]);
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SparseMatrix"/> class from a 2D array. 
        /// </summary>
        /// <param name="array">The 2D array to create this matrix from.</param>
        public SparseMatrix(float[,] array) : this(array.GetLength(0), array.GetLength(1))
        {
            var rows = array.GetLength(0);
            var columns = array.GetLength(1);

            for (var i = 0; i < rows; i++)
            {
                for (var j = 0; j < columns; j++)
                {
                    SetValueAt(i, j, array[i, j]);
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SparseMatrix"/> class, copying
        /// the values from the given matrix.
        /// </summary>
        /// <param name="matrix">The matrix to copy.</param>
        public SparseMatrix(Matrix<float> matrix)
            : this(matrix.RowCount, matrix.ColumnCount)
        {
            var sparseMatrix = matrix as SparseMatrix;

            var rows = matrix.RowCount;
            var columns = matrix.ColumnCount;

            if (sparseMatrix == null)
            {
                for (var i = 0; i < rows; i++)
                {
                    for (var j = 0; j < columns; j++)
                    {
                        SetValueAt(i, j, matrix.At(i, j));
                    }
                }
            }
            else
            {
                NonZerosCount = sparseMatrix.NonZerosCount;
                _rowIndex = new int[rows];
                _columnIndices = new int[NonZerosCount];
                _nonZeroValues = new float[NonZerosCount];

                Buffer.BlockCopy(sparseMatrix._nonZeroValues, 0, _nonZeroValues, 0, NonZerosCount * Constants.SizeOfFloat);
                Buffer.BlockCopy(sparseMatrix._columnIndices, 0, _columnIndices, 0, NonZerosCount * Constants.SizeOfInt);
                Buffer.BlockCopy(sparseMatrix._rowIndex, 0, _rowIndex, 0, rows * Constants.SizeOfInt);
            }
        }

        /// <summary>
        /// Creates a <c>SparseMatrix</c> for the given number of rows and columns.
        /// </summary>
        /// <param name="numberOfRows">
        /// The number of rows.
        /// </param>
        /// <param name="numberOfColumns">
        /// The number of columns.
        /// </param>
        /// <returns>
        /// A <c>SparseMatrix</c> with the given dimensions.
        /// </returns>
        public override Matrix<float> CreateMatrix(int numberOfRows, int numberOfColumns)
        {
            return new SparseMatrix(numberOfRows, numberOfColumns);
        }

        /// <summary>
        /// Creates a <see cref="SparseVector"/> with a the given dimension.
        /// </summary>
        /// <param name="size">The size of the vector.</param>
        /// <returns>
        /// A <see cref="SparseVector"/> with the given dimension.
        /// </returns>
        public override Vector<float> CreateVector(int size)
        {
            return new SparseVector(size);
        }

        /// <summary>
        /// Returns a new matrix containing the lower triangle of this matrix.
        /// </summary>
        /// <returns>The lower triangle of this matrix.</returns>        
        public override Matrix<float> LowerTriangle()
        {
            var result = CreateMatrix(RowCount, ColumnCount);
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
                throw new ArgumentException(Resources.ArgumentMatrixDimensions, "result");
            }

            if (ReferenceEquals(this, result))
            {
                var tmp = result.CreateMatrix(result.RowCount, result.ColumnCount);
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
            for (var row = 0; row < result.RowCount; row++)
            {
                var startIndex = _rowIndex[row];
                var endIndex = row < _rowIndex.Length - 1 ? _rowIndex[row + 1] : NonZerosCount;
                for (var j = startIndex; j < endIndex; j++)
                {
                    if (row >= _columnIndices[j])
                    {
                        result.At(row, _columnIndices[j], _nonZeroValues[j]);
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
            var result = CreateMatrix(RowCount, ColumnCount);
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
                throw new ArgumentException(Resources.ArgumentMatrixDimensions, "result");
            }

            if (ReferenceEquals(this, result))
            {
                var tmp = result.CreateMatrix(result.RowCount, result.ColumnCount);
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
            for (var row = 0; row < result.RowCount; row++)
            {
                var startIndex = _rowIndex[row];
                var endIndex = row < _rowIndex.Length - 1 ? _rowIndex[row + 1] : NonZerosCount;
                for (var j = startIndex; j < endIndex; j++)
                {
                    if (row <= _columnIndices[j])
                    {
                        result.At(row, _columnIndices[j], _nonZeroValues[j]);
                    }
                }
            }
        }

        /// <summary>
        /// Creates a matrix that contains the values from the requested sub-matrix.
        /// </summary>
        /// <param name="rowIndex">The row to start copying from.</param>
        /// <param name="rowLength">The number of rows to copy. Must be positive.</param>
        /// <param name="columnIndex">The column to start copying from.</param>
        /// <param name="columnLength">The number of columns to copy. Must be positive.</param>
        /// <returns>The requested sub-matrix.</returns>
        /// <exception cref="ArgumentOutOfRangeException">If: <list><item><paramref name="rowIndex"/> is
        /// negative, or greater than or equal to the number of rows.</item>
        /// <item><paramref name="columnIndex"/> is negative, or greater than or equal to the number 
        /// of columns.</item>
        /// <item><c>(columnIndex + columnLength) &gt;= Columns</c></item>
        /// <item><c>(rowIndex + rowLength) &gt;= Rows</c></item></list></exception>        
        /// <exception cref="ArgumentException">If <paramref name="rowLength"/> or <paramref name="columnLength"/>
        /// is not positive.</exception>
        public override Matrix<float> SubMatrix(int rowIndex, int rowLength, int columnIndex, int columnLength)
        {
            if (rowIndex >= RowCount || rowIndex < 0)
            {
                throw new ArgumentOutOfRangeException("rowIndex");
            }

            if (columnIndex >= ColumnCount || columnIndex < 0)
            {
                throw new ArgumentOutOfRangeException("columnIndex");
            }

            if (rowLength < 1)
            {
                throw new ArgumentException(Resources.ArgumentMustBePositive, "rowLength");
            }

            if (columnLength < 1)
            {
                throw new ArgumentException(Resources.ArgumentMustBePositive, "columnLength");
            }

            var colMax = columnIndex + columnLength;
            var rowMax = rowIndex + rowLength;

            if (rowMax > RowCount)
            {
                throw new ArgumentOutOfRangeException("rowLength");
            }

            if (colMax > ColumnCount)
            {
                throw new ArgumentOutOfRangeException("columnLength");
            }

            var result = (SparseMatrix)CreateMatrix(rowLength, columnLength);

            for (int i = rowIndex, row = 0; i < rowMax; i++, row++)
            {
                var startIndex = _rowIndex[i];
                var endIndex = i < _rowIndex.Length - 1 ? _rowIndex[i + 1] : NonZerosCount;

                for (int j = startIndex; j < endIndex; j++)
                {
                    // check if the column index is in the range
                    if ((_columnIndices[j] >= columnIndex) && (_columnIndices[j] < columnIndex + columnLength))
                    {
                        var column = _columnIndices[j] - columnIndex;
                        result.SetValueAt(row, column, _nonZeroValues[j]);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Returns a new matrix containing the lower triangle of this matrix. The new matrix
        /// does not contain the diagonal elements of this matrix.
        /// </summary>
        /// <returns>The lower triangle of this matrix.</returns>
        public override Matrix<float> StrictlyLowerTriangle()
        {
            var result = CreateMatrix(RowCount, ColumnCount);
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
                throw new ArgumentException(Resources.ArgumentMatrixDimensions, "result");
            }

            if (ReferenceEquals(this, result))
            {
                var tmp = result.CreateMatrix(result.RowCount, result.ColumnCount);
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
            for (var row = 0; row < result.RowCount; row++)
            {
                var startIndex = _rowIndex[row];
                var endIndex = row < _rowIndex.Length - 1 ? _rowIndex[row + 1] : NonZerosCount;
                for (var j = startIndex; j < endIndex; j++)
                {
                    if (row > _columnIndices[j])
                    {
                        result.At(row, _columnIndices[j], _nonZeroValues[j]);
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
            var result = CreateMatrix(RowCount, ColumnCount);
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
                throw new ArgumentException(Resources.ArgumentMatrixDimensions, "result");
            }

            if (ReferenceEquals(this, result))
            {
                var tmp = result.CreateMatrix(result.RowCount, result.ColumnCount);
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
            for (var row = 0; row < result.RowCount; row++)
            {
                var startIndex = _rowIndex[row];
                var endIndex = row < _rowIndex.Length - 1 ? _rowIndex[row + 1] : NonZerosCount;
                for (var j = startIndex; j < endIndex; j++)
                {
                    if (row < _columnIndices[j])
                    {
                        result.At(row, _columnIndices[j], _nonZeroValues[j]);
                    }
                }
            }
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
        public override float[] ToColumnWiseArray()
        {
            var ret = new float[RowCount * ColumnCount];
            for (var j = 0; j < ColumnCount; j++)
            {
                for (var i = 0; i < RowCount; i++)
                {
                    var index = FindItem(i, j);
                    ret[(j * RowCount) + i] = index >= 0 ? _nonZeroValues[index] : 0.0f;
                }
            }

            return ret;
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
        public override float At(int row, int column)
        {
            var index = FindItem(row, column);
            return index >= 0 ? _nonZeroValues[index] : 0.0f;
        }

        /// <summary>
        /// Sets the value of the given element.
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
        public override void At(int row, int column, float value)
        {
            SetValueAt(row, column, value);
        }

        #region Internal methods - CRS storage implementation
        /// <summary>
        /// Created this method because we cannot call "virtual At" in constructor of the class, but we need to do it
        /// </summary>
        /// <param name="row"> The row of the element. </param>
        /// <param name="column"> The column of the element. </param>
        /// <param name="value"> The value to set the element to. </param>
        /// <remarks>WARNING: This method is not thread safe. Use "lock" with it and be sure to avoid deadlocks</remarks>
        private void SetValueAt(int row, int column, float value)
        {
            var index = FindItem(row, column);
            if (index >= 0)
            {
                // Non-zero item found in matrix
                if (value == 0.0)
                {
                    // Delete existing item
                    DeleteItemByIndex(index, row);
                }
                else
                {
                    // Update item
                    _nonZeroValues[index] = value;
                }
            }
            else
            {
                // Item not found. Add new value
                if (value == 0.0)
                {
                    return;
                }

                index = ~index;

                // Check if the storage needs to be increased
                if ((NonZerosCount == _nonZeroValues.Length) && (NonZerosCount < (RowCount * ColumnCount)))
                {
                    // Value array is completely full so we increase the size
                    // Determine the increase in size. We will not grow beyond the size of the matrix
                    var size = Math.Min(_nonZeroValues.Length + GrowthSize(), RowCount * ColumnCount);
                    Array.Resize(ref _nonZeroValues, size);
                    Array.Resize(ref _columnIndices, size);
                }

                // Move all values (with an position larger than index) in the value array to the next position
                // move all values (with an position larger than index) in the columIndices array to the next position
                for (var i = NonZerosCount - 1; i > index - 1; i--)
                {
                    _nonZeroValues[i + 1] = _nonZeroValues[i];
                    _columnIndices[i + 1] = _columnIndices[i];
                }

                // Add the value and the column index
                _nonZeroValues[index] = value;
                _columnIndices[index] = column;

                // increase the number of non-zero numbers by one
                NonZerosCount += 1;

                // add 1 to all the row indices for rows bigger than rowIndex
                // so that they point to the correct part of the value array again.
                for (var i = row + 1; i < _rowIndex.Length; i++)
                {
                    _rowIndex[i] += 1;
                }
            }
        }

        /// <summary>
        /// Delete value from internal storage
        /// </summary>
        /// <param name="itemIndex">Index of value in nonZeroValues array</param>
        /// <param name="row">Row number of matrix</param>
        /// <remarks>WARNING: This method is not thread safe. Use "lock" with it and be sure to avoid deadlocks</remarks>
        private void DeleteItemByIndex(int itemIndex, int row)
        {
            // Move all values (with an position larger than index) in the value array to the previous position
            // move all values (with an position larger than index) in the columIndices array to the previous position
            for (var i = itemIndex + 1; i < NonZerosCount; i++)
            {
                _nonZeroValues[i - 1] = _nonZeroValues[i];
                _columnIndices[i - 1] = _columnIndices[i];
            }
            
            // Decrease value in Row
            for (var i = row + 1; i < _rowIndex.Length; i++)
            {
                _rowIndex[i] -= 1;
            }

            NonZerosCount -= 1;

            // Check if the storage needs to be shrink. This is reasonable to do if 
            // there are a lot of non-zero elements and storage is two times bigger
            if ((NonZerosCount > 1024) && (NonZerosCount < _nonZeroValues.Length / 2))
            {
                Array.Resize(ref _nonZeroValues, NonZerosCount);
                Array.Resize(ref _columnIndices, NonZerosCount);
            }
        }
        
        /// <summary>
        /// Find item Index in nonZeroValues array
        /// </summary>
        /// <param name="row">Matrix row index</param>
        /// <param name="column">Matrix column index</param>
        /// <returns>Item index</returns>
        /// <remarks>WARNING: This method is not thread safe. Use "lock" with it and be sure to avoid deadlocks</remarks>
        private int FindItem(int row, int column)
        {
            // Determin bounds in columnIndices array where this item should be searched (using rowIndex)
            var startIndex = _rowIndex[row];
            var endIndex = row < _rowIndex.Length - 1 ? _rowIndex[row + 1] : NonZerosCount;
            return Array.BinarySearch(_columnIndices, startIndex, endIndex - startIndex, column);
        }
        
        /// <summary>
        /// Calculates the amount with which to grow the storage array's if they need to be
        /// increased in size.
        /// </summary>
        /// <returns>The amount grown.</returns>
        private int GrowthSize()
        {
            int delta;
            if (_nonZeroValues.Length > 1024)
            {
                delta = _nonZeroValues.Length / 4;
            }
            else
            {
                if (_nonZeroValues.Length > 256)
                {
                    delta = 512;
                }
                else
                {
                    delta = _nonZeroValues.Length > 64 ? 128 : 32;
                }
            }

            return delta;
        }
        #endregion

        /// <summary>
        /// Sets all values to zero.
        /// </summary>
        public override void Clear()
        {
            NonZerosCount = 0;
            Array.Clear(_rowIndex, 0, _rowIndex.Length);
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
        public override void CopyTo(Matrix<float> target)
        {
            var sparseTarget = target as SparseMatrix;

            if (sparseTarget == null)
            {
                base.CopyTo(target);
            }
            else
            {
                if (ReferenceEquals(this, target))
                {
                    return;
                }

                if (RowCount != target.RowCount || ColumnCount != target.ColumnCount)
                {
                    throw new ArgumentException(Resources.ArgumentMatrixDimensions, "target");
                }

                // Lets copy only needed data. Portion of needed data is determined by NonZerosCount value
                sparseTarget._nonZeroValues = new float[NonZerosCount];
                sparseTarget._columnIndices = new int[NonZerosCount];
                sparseTarget.NonZerosCount = NonZerosCount;

                Buffer.BlockCopy(_nonZeroValues, 0, sparseTarget._nonZeroValues, 0, NonZerosCount * Constants.SizeOfFloat);
                Buffer.BlockCopy(_columnIndices, 0, sparseTarget._columnIndices, 0, NonZerosCount * Constants.SizeOfInt);
                Buffer.BlockCopy(_rowIndex, 0, sparseTarget._rowIndex, 0, RowCount * Constants.SizeOfInt);
            }
        }
        
        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            var hashNum = Math.Min(NonZerosCount, 25);
            long hash = 0;
            for (var i = 0; i < hashNum; i++)
            {
#if SILVERLIGHT
                hash ^= Precision.DoubleToInt64Bits(_nonZeroValues[i]);
#else
                hash ^= BitConverter.DoubleToInt64Bits(_nonZeroValues[i]);
#endif
            }

            return BitConverter.ToInt32(BitConverter.GetBytes(hash), 4);
        }

        /// <summary>
        /// Returns the transpose of this matrix.
        /// </summary>        
        /// <returns>The transpose of this matrix.</returns>
        public override Matrix<float> Transpose()
        {
            var ret = new SparseMatrix(ColumnCount, RowCount)
            {
                _columnIndices = new int[NonZerosCount],
                _nonZeroValues = new float[NonZerosCount]
            };

            // Do an 'inverse' CopyTo iterate over the rows
            for (var i = 0; i < _rowIndex.Length; i++)
            {
                // Get the begin / end index for the current row
                var startIndex = _rowIndex[i];
                var endIndex = i < _rowIndex.Length - 1 ? _rowIndex[i + 1] : NonZerosCount;

                // Get the values for the current row
                if (startIndex == endIndex)
                {
                    // Begin and end are equal. There are no values in the row, Move to the next row
                    continue;
                }

                for (var j = startIndex; j < endIndex; j++)
                {
                    ret.SetValueAt(_columnIndices[j], i, _nonZeroValues[j]);
                }
            }

            return ret;
        }

        /// <summary>Calculates the Frobenius norm of this matrix.</summary>
        /// <returns>The Frobenius norm of this matrix.</returns>
        public override float FrobeniusNorm()
        {
            var transpose = (SparseMatrix)Transpose();
            var aat = this * transpose;

            var norm = 0.0f;

            for (var i = 0; i < aat._rowIndex.Length; i++)
            {
                // Get the begin / end index for the current row
                var startIndex = aat._rowIndex[i];
                var endIndex = i < aat._rowIndex.Length - 1 ? aat._rowIndex[i + 1] : aat.NonZerosCount;

                // Get the values for the current row
                if (startIndex == endIndex)
                {
                    // Begin and end are equal. There are no values in the row, Move to the next row
                    continue;
                }

                for (var j = startIndex; j < endIndex; j++)
                {
                    if (i == aat._columnIndices[j])
                    {
                        norm += Math.Abs(aat._nonZeroValues[j]);
                    }
                }
            }

            norm = Convert.ToSingle(Math.Sqrt(norm));
            return norm;
        }

        /// <summary>Calculates the infinity norm of this matrix.</summary>
        /// <returns>The infinity norm of this matrix.</returns>   
        public override float InfinityNorm()
        {
            var norm = 0.0f;
            for (var i = 0; i < _rowIndex.Length; i++)
            {
                // Get the begin / end index for the current row
                var startIndex = _rowIndex[i];
                var endIndex = i < _rowIndex.Length - 1 ? _rowIndex[i + 1] : NonZerosCount;

                // Get the values for the current row
                if (startIndex == endIndex)
                {
                    // Begin and end are equal. There are no values in the row, Move to the next row
                    continue;
                }

                var s = 0.0f;
                for (var j = startIndex; j < endIndex; j++)
                {
                    s += Math.Abs(_nonZeroValues[j]);
                }

                norm = Math.Max(norm, s);
            }

            return norm;
        }

        /// <summary>
        /// Copies the requested row elements into a new <see cref="Vector{T}"/>.
        /// </summary>
        /// <param name="rowIndex">The row to copy elements from.</param>
        /// <param name="columnIndex">The column to start copying from.</param>
        /// <param name="length">The number of elements to copy.</param>
        /// <param name="result">The <see cref="Vector{T}"/> to copy the column into.</param>
        /// <exception cref="ArgumentNullException">If the result <see cref="Vector{T}"/> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="rowIndex"/> is negative,
        /// or greater than or equal to the number of columns.</exception>        
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="columnIndex"/> is negative,
        /// or greater than or equal to the number of rows.</exception>        
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="columnIndex"/> + <paramref name="length"/>  
        /// is greater than or equal to the number of rows.</exception>
        /// <exception cref="ArgumentException">If <paramref name="length"/> is not positive.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If <strong>result.Count &lt; length</strong>.</exception>
        public override void Row(int rowIndex, int columnIndex, int length, Vector<float> result)
        {
            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            if (rowIndex >= RowCount || rowIndex < 0)
            {
                throw new ArgumentOutOfRangeException("rowIndex");
            }

            if (columnIndex >= ColumnCount || columnIndex < 0)
            {
                throw new ArgumentOutOfRangeException("columnIndex");
            }

            if (columnIndex + length > ColumnCount)
            {
                throw new ArgumentOutOfRangeException("length");
            }

            if (length < 1)
            {
                throw new ArgumentException(Resources.ArgumentMustBePositive, "length");
            }

            if (result.Count < length)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "result");
            }

            // Determine bounds in columnIndices array where this item should be searched (using rowIndex)
            var startIndex = _rowIndex[rowIndex];
            var endIndex = rowIndex < _rowIndex.Length - 1 ? _rowIndex[rowIndex + 1] : NonZerosCount;

            if (startIndex == endIndex)
            {
                result.Clear();
            }
            else
            {
                // If there are non-zero elements use base class implementation
                for (int i = columnIndex, j = 0; i < columnIndex + length; i++, j++)
                {
                    // Copy code from At(row, column) to avoid unnecessary lock
                    var index = FindItem(rowIndex, i);
                    result[j] = index >= 0 ? _nonZeroValues[index] : 0.0f;
                }
            }
        }

        /// <summary>
        /// Diagonally stacks this matrix on top of the given matrix and places the combined matrix into the result matrix.
        /// </summary>
        /// <param name="lower">The lower, right matrix.</param>
        /// <param name="result">The combined matrix</param>
        /// <exception cref="ArgumentNullException">If lower is <see langword="null" />.</exception>
        /// <exception cref="ArgumentNullException">If the result matrix is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException">If the result matrix's dimensions are not (Rows + lower.rows) x (Columns + lower.Columns).</exception>
        public override void DiagonalStack(Matrix<float> lower, Matrix<float> result)
        {
            var lowerSparseMatrix = lower as SparseMatrix;
            var resultSparseMatrix = result as SparseMatrix;

            if ((lowerSparseMatrix == null) || (resultSparseMatrix == null))
            {
                base.DiagonalStack(lower, result);
            }
            else
            {
                if (resultSparseMatrix.RowCount != RowCount + lowerSparseMatrix.RowCount || resultSparseMatrix.ColumnCount != ColumnCount + lowerSparseMatrix.ColumnCount)
                {
                    throw new ArgumentException(Resources.ArgumentMatrixDimensions, "result");
                }

                resultSparseMatrix.NonZerosCount = NonZerosCount + lowerSparseMatrix.NonZerosCount;
                resultSparseMatrix._nonZeroValues = new float[resultSparseMatrix.NonZerosCount];
                resultSparseMatrix._columnIndices = new int[resultSparseMatrix.NonZerosCount];
                
                Array.Copy(_nonZeroValues, 0, resultSparseMatrix._nonZeroValues, 0, NonZerosCount);
                Array.Copy(lowerSparseMatrix._nonZeroValues, 0, resultSparseMatrix._nonZeroValues, NonZerosCount, lowerSparseMatrix.NonZerosCount);

                Array.Copy(_columnIndices, 0, resultSparseMatrix._columnIndices, 0, NonZerosCount);
                Array.Copy(_rowIndex, 0, resultSparseMatrix._rowIndex, 0, RowCount);

                // Copy and adjust lower column indices and rowIndex
                for (int i = NonZerosCount, j = 0; i < resultSparseMatrix.NonZerosCount; i++, j++)
                {
                    resultSparseMatrix._columnIndices[i] = lowerSparseMatrix._columnIndices[j] + ColumnCount;
                }

                for (int i = RowCount, j = 0; i < resultSparseMatrix.RowCount; i++, j++)
                {
                    resultSparseMatrix._rowIndex[i] = lowerSparseMatrix._rowIndex[j] + NonZerosCount;
                }
            }
        }

        #region Static constructors for special matrices.
        /// <summary>
        /// Initializes a square <see cref="SparseMatrix"/> with all zero's except for ones on the diagonal.
        /// </summary>
        /// <param name="order">the size of the square matrix.</param>
        /// <returns>Identity <c>SparseMatrix</c></returns>
        /// <exception cref="ArgumentException">
        /// If <paramref name="order"/> is less than one.
        /// </exception>
        public static SparseMatrix Identity(int order)
        {
            var m = new SparseMatrix(order)
                    {
                        NonZerosCount = order,
                        _nonZeroValues = new float[order],
                        _columnIndices = new int[order]
                    };

            for (var i = 0; i < order; i++)
            {
                m._nonZeroValues[i] = 1.0f;
                m._columnIndices[i] = i;
                m._rowIndex[i] = i;
            }

            return m;
        }
        #endregion

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">
        /// An object to compare with this object.
        /// </param>
        /// <returns>
        /// <c>true</c> if the current object is equal to the <paramref name="other"/> parameter; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(Matrix<float> other)
        {
            if (other == null)
            {
                return false;
            }

            if (ColumnCount != other.ColumnCount || RowCount != other.RowCount)
            {
                return false;
            }

            // Accept if the argument is the same object as this.
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            var sparseMatrix = other as SparseMatrix;

            if (sparseMatrix == null)
            {
                return base.Equals(other);
            }

            if (NonZerosCount != sparseMatrix.NonZerosCount)
            {
                return false;
            }

            // If all else fails, perform element wise comparison.
            for (var index = 0; index < NonZerosCount; index++)
            {
                if (!_nonZeroValues[index].AlmostEqual(sparseMatrix._nonZeroValues[index]) || _columnIndices[index] != sparseMatrix._columnIndices[index])
                {
                    return false;
                }
            }

            return true;
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

                Control.LinearAlgebraProvider.ScaleArray(2.0f, _nonZeroValues, _nonZeroValues);
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

            for (var i = 0; i < left.RowCount; i++)
            {
                // Get the begin / end index for the current row
                var startIndex = left._rowIndex[i];
                var endIndex = i < left._rowIndex.Length - 1 ? left._rowIndex[i + 1] : left.NonZerosCount;

                for (var j = startIndex; j < endIndex; j++)
                {
                    var columnIndex = left._columnIndices[j];
                    var resVal = left._nonZeroValues[j] + result.At(i, columnIndex);
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

            if (ReferenceEquals(this, sparseResult))
            {
                for (var i = 0; i < sparseOther.RowCount; i++)
                {
                    // Get the begin / end index for the current row
                    var startIndex = sparseOther._rowIndex[i];
                    var endIndex = i < sparseOther._rowIndex.Length - 1 ? sparseOther._rowIndex[i + 1] : sparseOther.NonZerosCount;

                    for (var j = startIndex; j < endIndex; j++)
                    {
                        var columnIndex = sparseOther._columnIndices[j];
                        var resVal = sparseResult.At(i, columnIndex) - sparseOther._nonZeroValues[j];
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

                for (var i = 0; i < RowCount; i++)
                {
                    // Get the begin / end index for the current row
                    var startIndex = _rowIndex[i];
                    var endIndex = i < _rowIndex.Length - 1 ? _rowIndex[i + 1] : NonZerosCount;

                    for (var j = startIndex; j < endIndex; j++)
                    {
                        var columnIndex = _columnIndices[j];
                        var resVal = sparseResult.At(i, columnIndex) + _nonZeroValues[j];
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

                for (var row = 0; row < RowCount; row++)
                {
                    var start = _rowIndex[row];
                    var end = _rowIndex[row + 1];

                    if (start == end)
                    {
                        continue;
                    }

                    for (var index = start; index < end; index++)
                    {
                        var column = _columnIndices[index];
                        result.At(row, column, _nonZeroValues[index] * scalar);
                    }
                }
            }
            else
            {
                if (!ReferenceEquals(this, result))
                {
                    CopyTo(sparseResult);
                }

                CommonParallel.For(0, NonZerosCount, index => sparseResult._nonZeroValues[index] *= scalar);
            }
        }

        /// <summary>
        /// Multiplies this matrix with another matrix and places the results into the result matrix.
        /// </summary>
        /// <param name="other">The matrix to multiply with.</param>
        /// <param name="result">The result of the multiplication.</param>
        protected override void DoMultiply(Matrix<float> other, Matrix<float> result)
        {
            result.Clear();
            var columnVector = new DenseVector(other.RowCount);
            for (var row = 0; row < RowCount; row++)
            {
                // Get the begin / end index for the current row
                var startIndex = _rowIndex[row];
                var endIndex = row < _rowIndex.Length - 1 ? _rowIndex[row + 1] : NonZerosCount;
                if (startIndex == endIndex)
                {
                    continue;
                }

                for (var column = 0; column < other.ColumnCount; column++)
                {
                    // Multiply row of matrix A on column of matrix B
                    other.Column(column, columnVector);
                    
                    var sum = 0.0f;
                    for (var index = startIndex; index < endIndex; index++)
                    {
                        sum += _nonZeroValues[index] * columnVector[_columnIndices[index]];
                    }

                    result.At(row, column, sum);
                }
            }
        }

        /// <summary>
        /// Multiplies this matrix with a vector and places the results into the result vector.
        /// </summary>
        /// <param name="rightSide">The vector to multiply with.</param>
        /// <param name="result">The result of the multiplication.</param>
        protected override void DoMultiply(Vector<float> rightSide, Vector<float> result)
        {
            for (var row = 0; row < RowCount; row++)
            {
                // Get the begin / end index for the current row
                var startIndex = _rowIndex[row];
                var endIndex = row < _rowIndex.Length - 1 ? _rowIndex[row + 1] : NonZerosCount;
                if (startIndex == endIndex)
                {
                    continue;
                }

                var sum = 0.0f;
                for (var index = startIndex; index < endIndex; index++)
                {
                    sum += _nonZeroValues[index] * rightSide[_columnIndices[index]];
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
            for (var j = 0; j < RowCount; j++)
            {
                // Get the begin / end index for the row
                var startIndexOther = otherSparse._rowIndex[j];
                var endIndexOther = j < otherSparse._rowIndex.Length - 1 ? otherSparse._rowIndex[j + 1] : otherSparse.NonZerosCount;
                if (startIndexOther == endIndexOther)
                {
                    continue;
                }

                for (var i = 0; i < RowCount; i++)
                {
                    // Multiply row of matrix A on row of matrix B
                    // Get the begin / end index for the row
                    var startIndexThis = _rowIndex[i];
                    var endIndexThis = i < _rowIndex.Length - 1 ? _rowIndex[i + 1] : NonZerosCount;
                    if (startIndexThis == endIndexThis)
                    {
                        continue;
                    }

                    var sum = 0.0f;
                    for (var index = startIndexOther; index < endIndexOther; index++)
                    {
                        var ind = FindItem(i, otherSparse._columnIndices[index]);
                        if (ind >= 0)
                        {
                            sum += otherSparse._nonZeroValues[index] * _nonZeroValues[ind];
                        }
                    }

                    resultSparse.SetValueAt(i, j, sum + result.At(i, j));
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

        /// <summary>
        /// Pointwise multiplies this matrix with another matrix and stores the result into the result matrix.
        /// </summary>
        /// <param name="other">The matrix to pointwise multiply with this one.</param>
        /// <param name="result">The matrix to store the result of the pointwise multiplication.</param>
        protected override void DoPointwiseMultiply(Matrix<float> other, Matrix<float> result)
        {
            result.Clear();

            for (var i = 0; i < other.RowCount; i++)
            {
                // Get the begin / end index for the current row
                var startIndex = _rowIndex[i];
                var endIndex = i < _rowIndex.Length - 1 ? _rowIndex[i + 1] : NonZerosCount;

                for (var j = startIndex; j < endIndex; j++)
                {
                    var resVal = _nonZeroValues[j] * other.At(i, _columnIndices[j]);
                    if (resVal != 0.0)
                    {
                        result.At(i, _columnIndices[j], resVal);
                    }
                }
            }
        }

        /// <summary>
        /// Pointwise divide this matrix by another matrix and stores the result into the result matrix.
        /// </summary>
        /// <param name="other">The matrix to pointwise divide this one by.</param>
        /// <param name="result">The matrix to store the result of the pointwise division.</param>
        protected override void DoPointwiseDivide(Matrix<float> other, Matrix<float> result)
        {
            result.Clear();

            for (var i = 0; i < other.RowCount; i++)
            {
                // Get the begin / end index for the current row
                var startIndex = _rowIndex[i];
                var endIndex = i < _rowIndex.Length - 1 ? _rowIndex[i + 1] : NonZerosCount;

                for (var j = startIndex; j < endIndex; j++)
                {
                    var resVal = _nonZeroValues[j] / other.At(i, _columnIndices[j]);
                    if (resVal != 0.0)
                    {
                        result.At(i, _columnIndices[j], resVal);
                    }
                }
            }
        }

        /// <summary>
        /// Computes the modulus for each element of the matrix.
        /// </summary>
        /// <param name="divisor">The divisor to use.</param>
        /// <param name="result">Matrix to store the results in.</param>
        protected override void DoModulus(float divisor, Matrix<float> result)
        {
            var denseResult = result as SparseMatrix;

            if (denseResult == null)
            {
                base.DoModulus(divisor, result);
            }
            else
            {
                if (!ReferenceEquals(this, result))
                {
                    CopyTo(result);
                }

                for (var index = 0; index < denseResult._nonZeroValues.Length; index++)
                {
                    denseResult._nonZeroValues[index] %= divisor;
                }
            }
        }

        /// <summary>
        /// Iterates throw each element in the matrix (row-wise).
        /// </summary>
        /// <returns>The value at the current iteration along with its position (row, column, value).</returns>
        public override IEnumerable<Tuple<int, int, float>> IndexedEnumerator()
        {
            for (var row = 0; row < RowCount - 1; row++)
            {
                var start = _rowIndex[row];
                var end = _rowIndex[row + 1];

                if (start == end)
                {
                    continue;
                }

                for (var index = start; index < end; index++)
                {
                    yield return new Tuple<int, int, float>(row, _columnIndices[index], _nonZeroValues[index]);
                }
            }

            var lastRow = _rowIndex.Length - 1;

            if (_rowIndex[lastRow] < NonZerosCount)
            {
                for (var index = _rowIndex[lastRow]; index < NonZerosCount; index++)
                {
                    yield return new Tuple<int, int, float>(lastRow, _columnIndices[index], _nonZeroValues[index]);
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether this matrix is symmetric.
        /// </summary>
        public override bool IsSymmetric
        {
            get
            {
                if (RowCount != ColumnCount)
                {
                    return false;
                }

                // todo: we might be able to speed this up by caching one half of the matrix
                for (var row = 0; row < RowCount - 1; row++)
                {
                    var start = _rowIndex[row];
                    var end = _rowIndex[row + 1];

                    if (start == end)
                    {
                        continue;
                    }

                    if (!CheckIfOppositesAreEqual(start, end, row))
                    {
                        return false;
                    }
                }

                var lastRow = _rowIndex.Length - 1;

                if (_rowIndex[lastRow] < NonZerosCount)
                {
                    if (!CheckIfOppositesAreEqual(_rowIndex[lastRow], NonZerosCount, lastRow))
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        /// <summary>
        /// Checks if opposites in a range are equal.
        /// </summary>
        /// <param name="start">The start of the range.</param>
        /// <param name="end">The end of the range.</param>
        /// <param name="row">The row the row to check.</param>
        /// <returns>If the values are equal or not.</returns>
        private bool CheckIfOppositesAreEqual(int start, int end, int row)
        {
            for (var index = start; index < end; index++)
            {
                var column = _columnIndices[index];
                var opposite = At(column, row);
                if (!_nonZeroValues[index].Equals(opposite))
                {
                    return false;
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
                throw new ArgumentOutOfRangeException(Resources.ArgumentMatrixDimensions);
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
                throw new ArgumentOutOfRangeException(Resources.ArgumentMatrixDimensions);
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
                throw new ArgumentException(Resources.ArgumentMatrixDimensions);
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

            return (SparseMatrix)leftSide.Modulus(rightSide);
        }
    }
}
