// <copyright file="SparseMatrix.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
// Copyright (c) 2009-2010 Math.NET
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
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
    using Properties;
    using Threading;

    /// <summary>
    /// Sparse Matrix implementation
    /// </summary>
    public class SparseMatrix : Matrix
    {
        /// <summary>
        /// Object for use in "lock"
        /// </summary>
        private readonly object lockObject = new object();

        /// <summary>
        /// The array containing the row indices of the existing rows. Element "j" of the array gives the index of the 
        /// element in the <see cref="nonZeroValues"/> array that is first non-zero element in a row "j"
        /// </summary>
        private readonly int[] rowIndex = new int[0];

        /// <summary>
        /// Array that contains the non-zero elements of matrix. Values of the non-zero elements of matrix are mapped into the values 
        /// array using the row-major storage mapping described in a compressed sparse row (CSR) format.
        /// </summary>
        private double[] nonZeroValues = new double[0];

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
        /// is the number of the column in matrix that contains the I-th value in the <see cref="nonZeroValues"/> array.
        /// </summary>
        private int[] columnIndices = new int[0];

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
            this.rowIndex = new int[rows];
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

            this.NonZerosCount = rows * columns;
            this.nonZeroValues = new double[this.NonZerosCount];
            this.columnIndices = new int[this.NonZerosCount];

            for (int i = 0, j = 0; i < this.nonZeroValues.Length; i++, j++)
            {
                // Reset column position to "0"
                if (j == columns)
                {
                    j = 0;
                }

                this.nonZeroValues[i] = value;
                this.columnIndices[i] = j;
            }

            // Set proper row pointers
            for (var i = 0; i < this.rowIndex.Length; i++)
            {
                this.rowIndex[i] = ((i + 1) * columns) - columns;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SparseMatrix"/> class from a one dimensional array. 
        /// </summary>
        /// <param name="rows">The number of rows.</param>
        /// <param name="columns">The number of columns.</param>
        /// <param name="array">The one dimensional array to create this matrix from. This array should store the matrix in column-major order. <seealso cref="http://en.wikipedia.org/wiki/Row-major_order"/></param>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="array"/> lenght is less than <paramref name="rows"/> * <paramref name="columns"/>.
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
                    this.SetValueAt(i, j, array[i + (j * rows)]);
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
                    this.SetValueAt(i, j, array[i, j]);
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
        public override Matrix CreateMatrix(int numberOfRows, int numberOfColumns)
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
        public override Vector CreateVector(int size)
        {
            return new SparseVector(size);
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
            lock (this.lockObject)
            {
                var index = this.FindItem(row, column);
                return index >= 0 ? this.nonZeroValues[index] : 0.0;
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
            lock (this.lockObject)
            {
                this.SetValueAt(row, column, value);
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
            var index = this.FindItem(row, column);
            if (index >= 0)
            {
                // Non-zero item found in matrix
                if (value == 0.0)
                {
                    // Delete existing item
                    this.DeleteItemByIndex(index, row);
                }
                else
                {
                    // Update item
                    this.nonZeroValues[index] = value;
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
                if ((this.NonZerosCount == this.nonZeroValues.Length) && (this.NonZerosCount < (this.RowCount * this.ColumnCount)))
                {
                    // Value array is completely full so we increase the size
                    // Determine the increase in size. We will not grow beyond the size of the matrix
                    var size = Math.Min(this.nonZeroValues.Length + this.GrowthSize(), this.RowCount * this.ColumnCount);
                    Array.Resize(ref this.nonZeroValues, size);
                    Array.Resize(ref this.columnIndices, size);
                }

                // Move all values (with an position larger than index) in the value array to the next position
                // move all values (with an position larger than index) in the columIndices array to the next position
                for (var i = this.NonZerosCount - 1; i > index - 1; i--)
                {
                    this.nonZeroValues[i + 1] = this.nonZeroValues[i];
                    this.columnIndices[i + 1] = this.columnIndices[i];
                }

                // Add the value and the column index
                this.nonZeroValues[index] = value;
                this.columnIndices[index] = column;

                // increase the number of non-zero numbers by one
                this.NonZerosCount += 1;

                // add 1 to all the row indices for rows bigger than rowIndex
                // so that they point to the correct part of the value array again.
                for (var i = row + 1; i < this.rowIndex.Length; i++)
                {
                    this.rowIndex[i] += 1;
                }
            }
        }

        /// <summary>
        /// Delete value from internal storage
        /// </summary>
        /// <param name="itemIndex">Index of value in <c>nonZeroValues</c> array</param>
        /// <param name="row">Row number of matrix</param>
        /// <remarks>WARNING: This method is not thread safe. Use "lock" with it and be sure to avoid deadlocks</remarks>
        private void DeleteItemByIndex(int itemIndex, int row)
        {
            // Move all values (with an position larger than index) in the value array to the previous position
            // move all values (with an position larger than index) in the columIndices array to the previous position
            for (var i = itemIndex + 1; i < this.NonZerosCount; i++)
            {
                this.nonZeroValues[i - 1] = this.nonZeroValues[i];
                this.columnIndices[i - 1] = this.columnIndices[i];
            }

            // Decrease value in Row
            for (var i = row + 1; i < this.rowIndex.Length; i++)
            {
                this.rowIndex[i] -= 1;
            }

            this.NonZerosCount -= 1;

            // Check if the storage needs to be shrink. This is reasonable to do if 
            // there are a lot of non-zero elements and storage is two times bigger
            if ((this.NonZerosCount > 1024) && (this.NonZerosCount < this.nonZeroValues.Length / 2))
            {
                Array.Resize(ref this.nonZeroValues, this.NonZerosCount);
                Array.Resize(ref this.columnIndices, this.NonZerosCount);
            }
        }

        /// <summary>
        /// Find item Index in <c>nonZeroValues</c> array
        /// </summary>
        /// <param name="row">Matrix row index</param>
        /// <param name="column">Matrix column index</param>
        /// <returns>Item index</returns>
        /// <remarks>WARNING: This method is not thread safe. Use "lock" with it and be sure to avoid deadlocks</remarks>
        private int FindItem(int row, int column)
        {
            // Determin bounds in columnIndices array where this item should be searched (using rowIndex)
            var startIndex = this.rowIndex[row];
            var endIndex = row < this.rowIndex.Length - 1 ? this.rowIndex[row + 1] : this.NonZerosCount;
            return Array.BinarySearch(this.columnIndices, startIndex, endIndex - startIndex, column);
        }

        /// <summary>
        /// Calculate grows size
        /// </summary>
        /// <returns>Proposed new size</returns>
        private int GrowthSize()
        {
            int delta;
            if (this.nonZeroValues.Length > 1024)
            {
                delta = this.nonZeroValues.Length / 4;
            }
            else
            {
                if (this.nonZeroValues.Length > 256)
                {
                    delta = 512;
                }
                else
                {
                    delta = this.nonZeroValues.Length > 64 ? 128 : 32;
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
            this.NonZerosCount = 0;
            Array.Clear(this.rowIndex, 0, this.rowIndex.Length);
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
        public override void CopyTo(Matrix target)
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

                if (this.RowCount != target.RowCount || this.ColumnCount != target.ColumnCount)
                {
                    throw new ArgumentException(Resources.ArgumentMatrixDimensions, "target");
                }

                // Lets copy only needed data. Portion of needed data is determined by NonZerosCount value
                sparseTarget.nonZeroValues = new double[this.NonZerosCount];
                sparseTarget.columnIndices = new int[this.NonZerosCount];
                sparseTarget.NonZerosCount = this.NonZerosCount;

                Buffer.BlockCopy(this.nonZeroValues, 0, sparseTarget.nonZeroValues, 0, this.NonZerosCount * Constants.SizeOfDouble);
                Buffer.BlockCopy(this.columnIndices, 0, sparseTarget.columnIndices, 0, this.NonZerosCount * Constants.SizeOfInt);
                Buffer.BlockCopy(this.rowIndex, 0, sparseTarget.rowIndex, 0, this.RowCount * Constants.SizeOfInt);
            }
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="obj">
        /// An object to compare with this object.
        /// </param>
        /// <returns>
        /// <c>true</c> if the current object is equal to the <paramref name="obj"/> parameter; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            var sparseMatrix = obj as SparseMatrix;

            if (sparseMatrix == null)
            {
                return base.Equals(obj);
            }

            // Accept if the argument is the same object as this.
            if (ReferenceEquals(this, sparseMatrix))
            {
                return true;
            }

            if (this.ColumnCount != sparseMatrix.ColumnCount || this.RowCount != sparseMatrix.RowCount || this.NonZerosCount != sparseMatrix.NonZerosCount)
            {
                return false;
            }

            // If all else fails, perform element wise comparison.
            for (var index = 0; index < this.NonZerosCount; index++)
            {
                if (!this.nonZeroValues[index].AlmostEqual(sparseMatrix.nonZeroValues[index]) || this.columnIndices[index] != sparseMatrix.columnIndices[index])
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            var hashNum = Math.Min(this.NonZerosCount, 25);
            long hash = 0;
            for (var i = 0; i < hashNum; i++)
            {
#if SILVERLIGHT
                hash ^= Precision.DoubleToInt64Bits(this.nonZeroValues[i]);
#else
                hash ^= BitConverter.DoubleToInt64Bits(this.nonZeroValues[i]);
#endif
            }

            return BitConverter.ToInt32(BitConverter.GetBytes(hash), 4);
        }

        /// <summary>
        /// Returns the transpose of this matrix.
        /// </summary>        
        /// <returns>The transpose of this matrix.</returns>
        public override Matrix Transpose()
        {
            var ret = new SparseMatrix(this.ColumnCount, this.RowCount);

            // Do an 'inverse' CopyTo iterate over the rows
            for (var i = 0; i < this.rowIndex.Length; i++)
            {
                // Get the begin / end index for the current row
                var startIndex = this.rowIndex[i];
                var endIndex = i < this.rowIndex.Length - 1 ? this.rowIndex[i + 1] : this.NonZerosCount;

                // Get the values for the current row
                if (startIndex == endIndex)
                {
                    // Begin and end are equal. There are no values in the row, Move to the next row
                    continue;
                }

                for (var j = startIndex; j < endIndex; j++)
                {
                    ret[this.columnIndices[j], i] = this.nonZeroValues[j];
                }
            }

            return ret;
        }

        #region Elementary operations

        /// <summary>
        /// Adds another matrix to this matrix. The result will be written into this matrix.
        /// </summary>
        /// <param name="other">The matrix to add to this matrix.</param>
        /// <exception cref="ArgumentNullException">If the other matrix is <see langword="null" />.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If the two matrices don't have the same dimensions.</exception>
        public override void Add(Matrix other)
        {
            if (ReferenceEquals(this, other))
            {
                this.Multiply(2);
                return;
            }

            var m = other as SparseMatrix;
            if (m == null)
            {
                base.Add(other);
            }
            else
            {
                this.Add(m);
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

            if (other.RowCount != this.RowCount || other.ColumnCount != this.ColumnCount)
            {
                throw new ArgumentOutOfRangeException(Resources.ArgumentMatrixDimensions);
            }

            for (var i = 0; i < other.RowCount; i++)
            {
                // Get the begin / end index for the current row
                var startIndex = other.rowIndex[i];
                var endIndex = i < other.rowIndex.Length - 1 ? other.rowIndex[i + 1] : other.NonZerosCount;

                for (var j = startIndex; j < endIndex; j++)
                {
                    var index = this.FindItem(i, other.columnIndices[j]);
                    if (index >= 0)
                    {
                        if (this.nonZeroValues[index] + other.nonZeroValues[j] == 0.0)
                        {
                            this.DeleteItemByIndex(index, i);
                        }
                        else
                        {
                            this.nonZeroValues[index] += other.nonZeroValues[j];
                        }
                    }
                    else
                    {
                        this.SetValueAt(i, other.columnIndices[j], other.nonZeroValues[j]);
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
        public override void Subtract(Matrix other)
        {
            // We are substracting Matrix form itself
            if (ReferenceEquals(this, other))
            {
                this.Clear();
                return;
            }

            var m = other as SparseMatrix;
            if (m == null)
            {
                base.Subtract(other);
            }
            else
            {
                this.Subtract(m);
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

            if (other.RowCount != this.RowCount || other.ColumnCount != this.ColumnCount)
            {
                throw new ArgumentOutOfRangeException(Resources.ArgumentMatrixDimensions);
            }

            for (var i = 0; i < other.RowCount; i++)
            {
                // Get the begin / end index for the current row
                var startIndex = other.rowIndex[i];
                var endIndex = i < other.rowIndex.Length - 1 ? other.rowIndex[i + 1] : other.NonZerosCount;

                for (var j = startIndex; j < endIndex; j++)
                {
                    var index = this.FindItem(i, other.columnIndices[j]);
                    if (index >= 0)
                    {
                        if (this.nonZeroValues[index] - other.nonZeroValues[j] == 0.0)
                        {
                            this.DeleteItemByIndex(index, i);
                        }
                        else
                        {
                            this.nonZeroValues[index] -= other.nonZeroValues[j];
                        }
                    }
                    else
                    {
                        this.SetValueAt(i, other.columnIndices[j], -other.nonZeroValues[j]);
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
                this.Clear();
                return;
            }

            Control.LinearAlgebraProvider.ScaleArray(scalar, this.nonZeroValues);
        }

        /// <summary>
        /// Multiplies this sparse matrix with another sparse matrix and places the results into the result sparse matrix.
        /// </summary>
        /// <param name="other">The matrix to multiply with.</param>
        /// <param name="result">The result of the multiplication.</param>
        /// <exception cref="ArgumentNullException">If the other matrix is <see langword="null" />.</exception>
        /// <exception cref="ArgumentNullException">If the result matrix is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException">If <strong>this.Columns != other.Rows</strong>.</exception>
        /// <exception cref="ArgumentException">If the result matrix's dimensions are not the this.Rows x other.Columns.</exception>
        public override void Multiply(Matrix other, Matrix result)
        {
            var otherSparseMatrix = other as SparseMatrix;
            var resultSparseMatrix = result as SparseMatrix;

            if (otherSparseMatrix == null || resultSparseMatrix == null)
            {
                base.Multiply(other, result);
            }
            else
            {
                if (this.ColumnCount != otherSparseMatrix.RowCount)
                {
                    throw new ArgumentException(Resources.ArgumentMatrixDimensions);
                }

                if (resultSparseMatrix.RowCount != this.RowCount || resultSparseMatrix.ColumnCount != otherSparseMatrix.ColumnCount)
                {
                    throw new ArgumentException(Resources.ArgumentMatrixDimensions);
                }

                resultSparseMatrix.Clear();

                var columnVector = new SparseVector(otherSparseMatrix.RowCount);
                for (var row = 0; row < this.RowCount; row++)
                {
                    // Get the begin / end index for the current row
                    var startIndex = this.rowIndex[row];
                    var endIndex = row < this.rowIndex.Length - 1 ? this.rowIndex[row + 1] : this.NonZerosCount;
                    for (var column = 0; column < otherSparseMatrix.ColumnCount; column++)
                    {
                        columnVector.Clear();
                        otherSparseMatrix.Column(column, columnVector);

                        // Multiply row of matrix A on column of matrix B
                        var sum = CommonParallel.Aggregate(
                            startIndex, 
                            endIndex, 
                            index => this.nonZeroValues[index] * columnVector[this.columnIndices[index]]);
                        resultSparseMatrix.SetValueAt(row, column, sum);
                    }
                }
            }
        }

        /// <summary>
        /// Multiplies this matrix with another matrix and returns the result.
        /// </summary>
        /// <param name="other">The matrix to multiply with.</param>
        /// <exception cref="ArgumentException">If <strong>this.Columns != other.Rows</strong>.</exception>        
        /// <exception cref="ArgumentNullException">If the other matrix is <see langword="null" />.</exception>
        /// <returns>The result of multiplication.</returns>
        public override Matrix Multiply(Matrix other)
        {
            var matrix = other as SparseMatrix;
            if (matrix == null)
            {
                return base.Multiply(other);
            }

            if (this.ColumnCount != matrix.RowCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixDimensions);
            }

            var result = (SparseMatrix)this.CreateMatrix(this.RowCount, matrix.ColumnCount);
            Multiply(matrix, result);
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
                        nonZeroValues = new double[order], 
                        columnIndices = new int[order]
                    };

            for (var i = 0; i < order; i++)
            {
                m.nonZeroValues[i] = 1.0;
                m.columnIndices[i] = i;
                m.rowIndex[i] = i;
            }

            return m;
        }

        #endregion
    }
}