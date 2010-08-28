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

namespace MathNet.Numerics.LinearAlgebra.Double
{
    using System;
    using System.Text;
    using Distributions;
    using Generic;
    using Properties;
    using Threading;
    
    /// <summary>
    /// A Matrix class with sparse storage. The underlying storage scheme is 3-array CSR Format.
    /// </summary>
    public class SparseMatrix : Matrix<double> 
    {
        /// <summary>
        /// Object for use in "lock"
        /// </summary>
        private readonly object _lockObject = new object();

        /// <summary>
        /// The array containing the row indices of the existing rows. Element "j" of the array gives the index of the 
        /// element in the <see cref="_nonZeroValues"/> array that is first non-zero element in a row "j"
        /// </summary>
        private readonly int[] _rowIndex = new int[0];

        /// <summary>
        /// Array that contains the non-zero elements of matrix. Values of the non-zero elements of matrix are mapped into the values 
        /// array using the row-major storage mapping described in a compressed sparse row (CSR) format.
        /// </summary>
        private double[] _nonZeroValues = new double[0];
        
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
        public SparseMatrix(int rows, int columns, double value) : this(rows, columns)
        {
            if (value == 0.0)
            {
                return;
            }

            NonZerosCount = rows * columns;
            _nonZeroValues = new double[NonZerosCount];
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
        /// <param name="array">The one dimensional array to create this matrix from. This array should store the matrix in column-major order. <seealso cref="http://en.wikipedia.org/wiki/Column-major_order"/></param>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="array"/> length is less than <paramref name="rows"/> * <paramref name="columns"/>.
        /// </exception>
        public SparseMatrix(int rows, int columns, double[] array) : this(rows, columns)
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
        public SparseMatrix(double[,] array) : this(array.GetLength(0), array.GetLength(1))
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
        public override Matrix<double> CreateMatrix(int numberOfRows, int numberOfColumns)
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
        public override Vector<double> CreateVector(int size)
        {
            return new SparseVector(size);
        }

        /// <summary>
        /// Returns a new matrix containing the lower triangle of this matrix.
        /// </summary>
        /// <returns>The lower triangle of this matrix.</returns>        
        public override Matrix<double> LowerTriangle()
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
        public override void LowerTriangle(Matrix<double> result)
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
        private void LowerTriangleImpl(Matrix<double> result)
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
        public override Matrix<double> UpperTriangle()
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
        public override void UpperTriangle(Matrix<double> result)
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
        private void UpperTriangleImpl(Matrix<double> result)
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
        public override Matrix<double> SubMatrix(int rowIndex, int rowLength, int columnIndex, int columnLength)
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
                var endIndex = row < _rowIndex.Length - 1 ? _rowIndex[i + 1] : NonZerosCount;

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
        public override Matrix<double> StrictlyLowerTriangle()
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
        public override void StrictlyLowerTriangle(Matrix<double> result)
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
        private void StrictlyLowerTriangleImpl(Matrix<double> result)
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
        public override Matrix<double> StrictlyUpperTriangle()
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
        public override void StrictlyUpperTriangle(Matrix<double> result)
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
        private void StrictlyUpperTriangleImpl(Matrix<double> result)
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
        public override double[] ToColumnWiseArray()
        {
            var ret = new double[RowCount * ColumnCount];
            for (var j = 0; j < ColumnCount; j++)
            {
                for (var i = 0; i < RowCount; i++)
                {
                    var index = FindItem(i, j);
                    ret[(j * RowCount) + i] = index >= 0 ? _nonZeroValues[index] : 0.0;
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
        public override double At(int row, int column)
        {
            lock (_lockObject)
            {
                var index = FindItem(row, column);
                return index >= 0 ? _nonZeroValues[index] : 0.0;
            }
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
        public override void At(int row, int column, double value)
        {
            lock (_lockObject)
            {
                SetValueAt(row, column, value);
            }
        }

        #region Internal methods - CRS storage implementation
        /// <summary>
        /// Created this method because we cannot call "virtual At" in constructor of the class, but we need to do it
        /// </summary>
        /// <param name="row"> The row of the element. </param>
        /// <param name="column"> The column of the element. </param>
        /// <param name="value"> The value to set the element to. </param>
        /// <remarks>WARNING: This method is not thread safe. Use "lock" with it and be sure to avoid deadlocks</remarks>
        private void SetValueAt(int row, int column, double value)
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
        public override void CopyTo(Matrix<double> target)
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
                sparseTarget._nonZeroValues = new double[NonZerosCount];
                sparseTarget._columnIndices = new int[NonZerosCount];
                sparseTarget.NonZerosCount = NonZerosCount;

                Buffer.BlockCopy(_nonZeroValues, 0, sparseTarget._nonZeroValues, 0, NonZerosCount * Constants.SizeOfDouble);
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
        public override Matrix<double> Transpose()
        {
            var ret = new SparseMatrix(ColumnCount, RowCount)
            {
                _columnIndices = new int[NonZerosCount],
                _nonZeroValues = new double[NonZerosCount]
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
        public override double FrobeniusNorm()
        {
            var transpose = (SparseMatrix)Transpose();
            var aat = this * transpose;

            var norm = 0.0;

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

            norm = Math.Sqrt(norm);
            return norm;
        }

        /// <summary>Calculates the infinity norm of this matrix.</summary>
        /// <returns>The infinity norm of this matrix.</returns>   
        public override double InfinityNorm()
        {
            var norm = 0.0;
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

                var s = 0.0;
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
        public override void Row(int rowIndex, int columnIndex, int length, Vector<double> result)
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
                    result[j] = index >= 0 ? _nonZeroValues[index] : 0.0;
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
        public override void DiagonalStack(Matrix<double> lower, Matrix<double> result)
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
                resultSparseMatrix._nonZeroValues = new double[resultSparseMatrix.NonZerosCount];
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

        #region Elementary operations

        /// <summary>
        /// Adds another matrix to this matrix. The result will be written into this matrix.
        /// </summary>
        /// <param name="other">The matrix to add to this matrix.</param>
        /// <exception cref="ArgumentNullException">If the other matrix is <see langword="null" />.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If the two matrices don't have the same dimensions.</exception>
        public override void Add(Matrix<double> other)
        {
            if (ReferenceEquals(this, other))
            {
                Multiply(2);
                return;
            }

            var m = other as SparseMatrix;
            if (m == null)
            {
                base.Add(other);
            }
            else
            {
                Add(m);
            }
        }

        /// <summary>
        /// Adds another <see cref="SparseMatrix"/> to this matrix. The result will be written into this matrix.
        /// </summary>
        /// <param name="other">The <see cref="SparseMatrix"/> to add to this matrix.</param>
        /// <exception cref="ArgumentNullException">If the other matrix is <see langword="null" />.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If the two matrices don't have the same dimensions.</exception>
        public void Add(SparseMatrix other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            if (other.RowCount != RowCount || other.ColumnCount != ColumnCount)
            {
                throw new ArgumentOutOfRangeException(Resources.ArgumentMatrixDimensions);
            }

            for (var i = 0; i < other.RowCount; i++)
            {
                // Get the begin / end index for the current row
                var startIndex = other._rowIndex[i];
                var endIndex = i < other._rowIndex.Length - 1 ? other._rowIndex[i + 1] : other.NonZerosCount;

                for (var j = startIndex; j < endIndex; j++)
                {
                    var index = FindItem(i, other._columnIndices[j]);
                    if (index >= 0)
                    {
                        if (_nonZeroValues[index] + other._nonZeroValues[j] == 0.0)
                        {
                            DeleteItemByIndex(index, i);
                        }
                        else
                        {
                            _nonZeroValues[index] += other._nonZeroValues[j];
                        }
                    }
                    else
                    {
                        SetValueAt(i, other._columnIndices[j], other._nonZeroValues[j]);
                    }
                }
            }
        }

        /// <summary>
        /// Subtracts another matrix from this matrix. The result will be written into this matrix.
        /// </summary>
        /// <param name="other">The matrix to subtract.</param>
        /// <exception cref="ArgumentNullException">If the other matrix is <see langword="null" />.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If the two matrices don't have the same dimensions.</exception>
        public override void Subtract(Matrix<double> other)
        {
            // We are substracting Matrix form itself
            if (ReferenceEquals(this, other))
            {
                Clear();
                return;
            }

            var m = other as SparseMatrix;
            if (m == null)
            {
                base.Subtract(other);
            }
            else
            {
                Subtract(m);
            }
        }

        /// <summary>
        /// Subtracts another <see cref="SparseMatrix"/> from this matrix. The result will be written into this matrix.
        /// </summary>
        /// <param name="other">The <see cref="SparseMatrix"/> to subtract.</param>
        /// <exception cref="ArgumentNullException">If the other matrix is <see langword="null" />.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If the two matrices don't have the same dimensions.</exception>
        public void Subtract(SparseMatrix other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            if (other.RowCount != RowCount || other.ColumnCount != ColumnCount)
            {
                throw new ArgumentOutOfRangeException(Resources.ArgumentMatrixDimensions);
            }

            for (var i = 0; i < other.RowCount; i++)
            {
                // Get the begin / end index for the current row
                var startIndex = other._rowIndex[i];
                var endIndex = i < other._rowIndex.Length - 1 ? other._rowIndex[i + 1] : other.NonZerosCount;

                for (var j = startIndex; j < endIndex; j++)
                {
                    var index = FindItem(i, other._columnIndices[j]);
                    if (index >= 0)
                    {
                        if (_nonZeroValues[index] - other._nonZeroValues[j] == 0.0)
                        {
                            DeleteItemByIndex(index, i);
                        }
                        else
                        {
                            _nonZeroValues[index] -= other._nonZeroValues[j];
                        }
                    }
                    else
                    {
                        SetValueAt(i, other._columnIndices[j], -other._nonZeroValues[j]);
                    }
                }
            }
        }

        /// <summary>
        /// Multiplies each element of this matrix with a scalar.
        /// </summary>
        /// <param name="scalar">The scalar to multiply with.</param>
        public override void Multiply(double scalar)
        {
            if (1.0.AlmostEqualInDecimalPlaces(scalar, 15))
            {
                return;
            }

            if (0.0.AlmostEqualInDecimalPlaces(scalar, 15))
            {
                Clear();
                return;
            }

            Control.LinearAlgebraProvider.ScaleArray(scalar, _nonZeroValues);
        }

        /// <summary>
        /// Multiplies this sparse matrix with another sparse matrix and places the results into the result sparse matrix.
        /// </summary>
        /// <param name="other">The matrix to multiply with.</param>
        /// <param name="result">The result of the multiplication.</param>
        /// <exception cref="ArgumentNullException">If the other matrix is <see langword="null" />.</exception>
        /// <exception cref="ArgumentNullException">If the result matrix is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException">If <strong>Columns != other.Rows</strong>.</exception>
        /// <exception cref="ArgumentException">If the result matrix's dimensions are not the Rows x other.Columns.</exception>
        public override void Multiply(Matrix<double> other, Matrix<double> result)
        {
            var otherSparseMatrix = other as SparseMatrix;
            var resultSparseMatrix = result as SparseMatrix;
            if (otherSparseMatrix == null || resultSparseMatrix == null)
            {
                base.Multiply(other, result);
                return;
            }

            if (ColumnCount != otherSparseMatrix.RowCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixDimensions);
            }

            if (resultSparseMatrix.RowCount != RowCount || resultSparseMatrix.ColumnCount != otherSparseMatrix.ColumnCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixDimensions);
            }

            resultSparseMatrix.Clear();
            var columnVector = new DenseVector(otherSparseMatrix.RowCount);
            for (var row = 0; row < RowCount; row++)
            {
                // Get the begin / end index for the current row
                var startIndex = _rowIndex[row];
                var endIndex = row < _rowIndex.Length - 1 ? _rowIndex[row + 1] : NonZerosCount;
                if (startIndex == endIndex)
                {
                    continue;
                }

                for (var column = 0; column < otherSparseMatrix.ColumnCount; column++)
                {
                    // Multiply row of matrix A on column of matrix B
                    otherSparseMatrix.Column(column, columnVector);
                    var sum = CommonParallel.Aggregate(
                        startIndex,
                        endIndex,
                        index => _nonZeroValues[index] * columnVector[_columnIndices[index]]);
                    resultSparseMatrix.SetValueAt(row, column, sum);
                }
            }
        }

        /// <summary>
        /// Multiplies this matrix with another matrix and returns the result.
        /// </summary>
        /// <param name="other">The matrix to multiply with.</param>
        /// <exception cref="ArgumentException">If <strong>Columns != other.Rows</strong>.</exception>        
        /// <exception cref="ArgumentNullException">If the other matrix is <see langword="null" />.</exception>
        /// <returns>The result of multiplication.</returns>
        public override Matrix<double> Multiply(Matrix<double> other)
        {
            var matrix = other as SparseMatrix;
            if (matrix == null)
            {
                return base.Multiply(other);
            }

            if (ColumnCount != matrix.RowCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixDimensions);
            }

            var result = (SparseMatrix)CreateMatrix(RowCount, matrix.ColumnCount);
            Multiply(matrix, result);
            return result;
        }

        /// <summary>
        /// Multiplies this dense matrix with transpose of another dense matrix and places the results into the result dense matrix.
        /// </summary>
        /// <param name="other">The matrix to multiply with.</param>
        /// <param name="result">The result of the multiplication.</param>
        /// <exception cref="ArgumentNullException">If the other matrix is <see langword="null" />.</exception>
        /// <exception cref="ArgumentNullException">If the result matrix is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException">If <strong>this.Columns != other.Rows</strong>.</exception>
        /// <exception cref="ArgumentException">If the result matrix's dimensions are not the this.Rows x other.Columns.</exception>
        public override void TransposeAndMultiply(Matrix<double> other, Matrix<double> result)
        {
            var otherSparse = other as SparseMatrix;
            var resultSparse = result as SparseMatrix;

            if (otherSparse == null || resultSparse == null)
            {
                base.TransposeAndMultiply(other, result);
                return;
            }

            if (ColumnCount != otherSparse.ColumnCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixDimensions);
            }

            if ((resultSparse.RowCount != RowCount) || (resultSparse.ColumnCount != otherSparse.RowCount))
            {
                throw new ArgumentException(Resources.ArgumentMatrixDimensions);
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

                    var i1 = i;
                    var sum = CommonParallel.Aggregate(
                        startIndexOther,
                        endIndexOther,
                        index =>
                        {
                            var ind = FindItem(i1, otherSparse._columnIndices[index]);
                            return ind >= 0 ? otherSparse._nonZeroValues[index] * _nonZeroValues[ind] : 0.0;
                        });

                    resultSparse.SetValueAt(i, j, sum + result.At(i, j));
                }
            }
        }
    
        /// <summary>
        /// Multiplies this matrix with transpose of another matrix and returns the result.
        /// </summary>
        /// <param name="other">The matrix to multiply with.</param>
        /// <exception cref="ArgumentException">If <strong>this.Columns != other.Rows</strong>.</exception>        
        /// <exception cref="ArgumentNullException">If the other matrix is <see langword="null" />.</exception>
        /// <returns>The result of multiplication.</returns>
        public override Matrix<double> TransposeAndMultiply(Matrix<double> other)
        {
            var otherSparse = other as SparseMatrix;
            if (otherSparse == null)
            {
                return base.TransposeAndMultiply(other);
            }

            if (ColumnCount != otherSparse.ColumnCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixDimensions);
            }

            var result = (SparseMatrix)CreateMatrix(RowCount, other.RowCount);
            TransposeAndMultiply(other, result);
            return result;
        }

        /// <summary>
        /// Multiplies two sparse matrices.
        /// </summary>
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
        /// Pointwise multiplies this matrix with another matrix and stores the result into the result matrix.
        /// </summary>
        /// <param name="other">The matrix to pointwise multiply with this one.</param>
        /// <param name="result">The matrix to store the result of the pointwise multiplication.</param>
        /// <exception cref="ArgumentNullException">If the other matrix is <see langword="null" />.</exception> 
        /// <exception cref="ArgumentNullException">If the result matrix is <see langword="null" />.</exception> 
        /// <exception cref="ArgumentException">If this matrix and <paramref name="other"/> are not the same size.</exception>
        /// <exception cref="ArgumentException">If this matrix and <paramref name="result"/> are not the same size.</exception>
        public override void PointwiseMultiply(Matrix<double> other, Matrix<double> result)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            if (ColumnCount != other.ColumnCount || RowCount != other.RowCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixDimensions, "result");
            }

            if (ColumnCount != result.ColumnCount || RowCount != result.RowCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixDimensions, "result");
            }

            result.Clear();
            for (var i = 0; i < other.RowCount; i++)
            {
                // Get the begin / end index for the current row
                var startIndex = _rowIndex[i];
                var endIndex = i < _rowIndex.Length - 1 ? _rowIndex[i + 1] : NonZerosCount;

                for (var j = startIndex; j < endIndex; j++)
                {
                    var resVal = _nonZeroValues[j] * other[i, _columnIndices[j]];
                    if (resVal != 0.0)
                    {
                        result[i, _columnIndices[j]] = resVal;
                    }
                }
            }
        }

        /// <summary>
        /// Generates matrix with random elements.
        /// </summary>
        /// <param name="numberOfRows">Number of rows.</param>
        /// <param name="numberOfColumns">Number of columns.</param>
        /// <param name="distribution">Continuous Random Distribution or Source</param>
        /// <returns>
        /// An <c>numberOfRows</c>-by-<c>numberOfColumns</c> matrix with elements distributed according to the provided distribution.
        /// </returns>
        /// <exception cref="ArgumentException">If the parameter <paramref name="numberOfRows"/> is not positive.</exception>
        /// <exception cref="ArgumentException">If the parameter <paramref name="numberOfColumns"/> is not positive.</exception>
        public override Matrix<double> Random(int numberOfRows, int numberOfColumns, IContinuousDistribution distribution)
        {
            if (numberOfRows < 1)
            {
                throw new ArgumentException(Resources.ArgumentMustBePositive, "numberOfRows");
            }

            if (numberOfColumns < 1)
            {
                throw new ArgumentException(Resources.ArgumentMustBePositive, "numberOfColumns");
            }

            var matrix = (SparseMatrix)CreateMatrix(numberOfRows, numberOfColumns);
            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    var value = distribution.Sample();
                    if (value != 0.0)
                    {
                        matrix.SetValueAt(i, j, value);
                    }
                }
            }

            return matrix;
        }

        /// <summary>
        /// Generates matrix with random elements.
        /// </summary>
        /// <param name="numberOfRows">Number of rows.</param>
        /// <param name="numberOfColumns">Number of columns.</param>
        /// <param name="distribution">Continuous Random Distribution or Source</param>
        /// <returns>
        /// An <c>numberOfRows</c>-by-<c>numberOfColumns</c> matrix with elements distributed according to the provided distribution.
        /// </returns>
        /// <exception cref="ArgumentException">If the parameter <paramref name="numberOfRows"/> is not positive.</exception>
        /// <exception cref="ArgumentException">If the parameter <paramref name="numberOfColumns"/> is not positive.</exception>
        public override Matrix<double> Random(int numberOfRows, int numberOfColumns, IDiscreteDistribution distribution)
        {
            if (numberOfRows < 1)
            {
                throw new ArgumentException(Resources.ArgumentMustBePositive, "numberOfRows");
            }

            if (numberOfColumns < 1)
            {
                throw new ArgumentException(Resources.ArgumentMustBePositive, "numberOfColumns");
            }

            var matrix = (SparseMatrix)CreateMatrix(numberOfRows, numberOfColumns);
            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    var value = distribution.Sample();
                    if (value != 0.0)
                    {
                        matrix.SetValueAt(i, j, value);
                    }
                }
            }

            return matrix;
        }

        #endregion

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
                        _nonZeroValues = new double[order],
                        _columnIndices = new int[order]
                    };

            for (var i = 0; i < order; i++)
            {
                m._nonZeroValues[i] = 1.0;
                m._columnIndices[i] = i;
                m._rowIndex[i] = i;
            }

            return m;
        }
        #endregion

        /// <summary>
        /// Negates each element of this matrix.
        /// </summary>        
        public override void Negate()
        {
            Multiply(-1);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">
        /// An object to compare with this object.
        /// </param>
        /// <returns>
        /// <c>true</c> if the current object is equal to the <paramref name="other"/> parameter; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(Matrix<double> other)
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
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <param name="format">
        /// The format to use.
        /// </param>
        /// <param name="formatProvider">
        /// The format provider to use.
        /// </param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString(string format, IFormatProvider formatProvider)
        {
            var stringBuilder = new StringBuilder();
            for (var row = 0; row < RowCount; row++)
            {
                for (var column = 0; column < ColumnCount; column++)
                {
                    stringBuilder.Append(At(row, column).ToString(format, formatProvider));
                    if (column != ColumnCount - 1)
                    {
                        stringBuilder.Append(formatProvider.GetTextInfo().ListSeparator);
                    }
                }

                if (row != RowCount - 1)
                {
                    stringBuilder.Append(Environment.NewLine);
                }
            }

            return stringBuilder.ToString();
        }

        #region Simple arithmetic of type T
        /// <summary>
        /// Add two values T+T
        /// </summary>
        /// <param name="val1">Left operand value</param>
        /// <param name="val2">Right operand value</param>
        /// <returns>Result of addition</returns>
        protected sealed override double AddT(double val1, double val2)
        {
            return val1 + val2;
        }

        /// <summary>
        /// Subtract two values T-T
        /// </summary>
        /// <param name="val1">Left operand value</param>
        /// <param name="val2">Right operand value</param>
        /// <returns>Result of subtract</returns>
        protected sealed override double SubtractT(double val1, double val2)
        {
            return val1 - val2;
        }

        /// <summary>
        /// Multiply two values T*T
        /// </summary>
        /// <param name="val1">Left operand value</param>
        /// <param name="val2">Right operand value</param>
        /// <returns>Result of multiplication</returns>
        protected sealed override double MultiplyT(double val1, double val2)
        {
            return val1 * val2;
        }

        /// <summary>
        /// Divide two values T/T
        /// </summary>
        /// <param name="val1">Left operand value</param>
        /// <param name="val2">Right operand value</param>
        /// <returns>Result of divide</returns>
        protected sealed override double DivideT(double val1, double val2)
        {
            return val1 / val2;
        }

        /// <summary>
        /// Is equal to one?
        /// </summary>
        /// <param name="val1">Value to check</param>
        /// <returns>True if one; otherwise false</returns>
        protected sealed override bool IsOneT(double val1)
        {
            return 1.0.AlmostEqualInDecimalPlaces(val1, 15);
        }

        /// <summary>
        /// Take absolute value
        /// </summary>
        /// <param name="val1">Source alue</param>
        /// <returns>True if one; otherwise false</returns>
        protected sealed override double AbsoluteT(double val1)
        {
            return Math.Abs(val1);
        }
        #endregion  
    }
}
