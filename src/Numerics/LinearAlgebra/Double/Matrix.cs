// <copyright file="Matrix.cs" company="Math.NET">
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
    using System.Collections.Generic;
    using System.Text;
    using Properties;
    using Threading;

    /// <summary>
    /// Defines the base class for <c>Matrix</c> classes.
    /// </summary>
#if !SILVERLIGHT
    [Serializable]
#endif
    public abstract partial class Matrix :
#if SILVERLIGHT
   IFormattable, IEquatable<Matrix>
#else
        IFormattable, IEquatable<Matrix>, ICloneable
#endif
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Matrix"/> class.
        /// </summary>
        /// <param name="rows">
        /// The number of rows.
        /// </param>
        /// <param name="columns">
        /// The number of columns.
        /// </param>
        protected Matrix(int rows, int columns)
        {
            if (rows <= 0)
            {
                throw new ArgumentOutOfRangeException(Resources.MatrixRowsMustBePositive);
            }

            if (columns <= 0)
            {
                throw new ArgumentOutOfRangeException(Resources.MatrixColumnsMustBePositive);
            }

            this.RowCount = rows;
            this.ColumnCount = columns;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Matrix"/> class.
        /// </summary>
        /// <param name="order">
        /// The order of the matrix.
        /// </param>
        protected Matrix(int order)
        {
            if (order <= 0)
            {
                throw new ArgumentOutOfRangeException(Resources.MatrixRowsOrColumnsMustBePositive);
            }

            this.RowCount = order;
            this.ColumnCount = order;
        }

        /// <summary>
        /// Gets the number of columns.
        /// </summary>
        /// <value>The number of columns.</value>
        public virtual int ColumnCount
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the number of rows.
        /// </summary>
        /// <value>The number of rows.</value>
        public virtual int RowCount
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the value at the given row and column.
        /// </summary>
        /// <param name="row">
        /// The row of the element.
        /// </param>
        /// <param name="column">
        /// The column of the element.
        /// </param>
        /// <value>The double value to get or set.</value>
        /// <remarks>This method is ranged checked. <see cref="At(int,int)"/> and <see cref="At(int,int,double)"/>
        /// to get and set values without range checking.</remarks>
        public virtual double this[int row, int column]
        {
            get
            {
                this.RangeCheck(row, column);
                return this.At(row, column);
            }

            set
            {
                this.RangeCheck(row, column);
                this.At(row, column, value);
            }
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
        public abstract double At(int row, int column);

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
        public abstract void At(int row, int column, double value);

        /// <summary>
        /// Creates a clone of this instance.
        /// </summary>
        /// <returns>
        /// A clone of the instance.
        /// </returns>
        public virtual Matrix Clone()
        {
            var result = this.CreateMatrix(this.RowCount, this.ColumnCount);
            this.CopyTo(result);
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
        public virtual void CopyTo(Matrix target)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }

            if (ReferenceEquals(this, target))
            {
                return;
            }

            if (this.RowCount != target.RowCount || this.ColumnCount != target.ColumnCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixDimensions, "target");
            }

            for (var i = 0; i < this.RowCount; i++)
            {
                for (var j = 0; j < this.ColumnCount; j++)
                {
                    target.At(i, j, this.At(i, j));
                }
            }
        }

        /// <summary>
        /// Creates a <strong>Matrix</strong> for the given number of rows and columns.
        /// </summary>
        /// <param name="numberOfRows">
        /// The number of rows.
        /// </param>
        /// <param name="numberOfColumns">
        /// The number of columns.
        /// </param>
        /// <returns>
        /// A <strong>Matrix</strong> with the given dimensions.
        /// </returns>
        /// <remarks>
        /// Creates a matrix of the same matrix type as the current matrix.
        /// </remarks>
        public abstract Matrix CreateMatrix(int numberOfRows, int numberOfColumns);

        /// <summary>
        /// Creates a <see cref="Vector"/> with a the given dimension.
        /// </summary>
        /// <param name="size">The size of the vector.</param>
        /// <returns>
        /// A <see cref="Vector"/> with the given dimension.
        /// </returns>
        /// <remarks>
        /// Creates a vector of the same type as the current matrix.
        /// </remarks>
        public abstract Vector CreateVector(int size);

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.ToString(null, null);
        }

        /// <summary>
        /// Copies a row into an <see cref="Vector"/>.
        /// </summary>
        /// <param name="index">The row to copy.</param>
        /// <returns>A <see cref="Vector"/> containing the copied elements.</returns>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="index"/> is negative,
        /// or greater than or equal to the number of rows.</exception>
        public virtual Vector Row(int index)
        {
            var ret = this.CreateVector(this.ColumnCount);
            this.Row(index, 0, this.ColumnCount, ret);
            return ret;
        }

        /// <summary>
        /// Copies a row into to the given <see cref="Vector"/>.
        /// </summary>
        /// <param name="index">The row to copy.</param>
        /// <param name="result">The <see cref="Vector"/> to copy the row into.</param>
        /// <exception cref="ArgumentNullException">If the result vector is <see langword="null" />.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="index"/> is negative,
        /// or greater than or equal to the number of rows.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If <b>this.Columns != result.Count</b>.</exception>
        public virtual void Row(int index, Vector result)
        {
            this.Row(index, 0, this.ColumnCount, result);
        }

        /// <summary>
        /// Copies the requested row elements into a new <see cref="Vector"/>.
        /// </summary>
        /// <param name="rowIndex">The row to copy elements from.</param>
        /// <param name="columnIndex">The column to start copying from.</param>
        /// <param name="length">The number of elements to copy.</param>
        /// <returns>A <see cref="Vector"/> containing the requested elements.</returns>
        /// <exception cref="ArgumentOutOfRangeException">If:
        /// <list><item><paramref name="rowIndex"/> is negative,
        /// or greater than or equal to the number of rows.</item>
        /// <item><paramref name="columnIndex"/> is negative,
        /// or greater than or equal to the number of columns.</item>
        /// <item><c>(columnIndex + length) &gt;= Columns.</c></item></list></exception>        
        /// <exception cref="ArgumentException">If <paramref name="length"/> is not positive.</exception>
        public virtual Vector Row(int rowIndex, int columnIndex, int length)
        {
            var ret = this.CreateVector(length);
            this.Row(rowIndex, columnIndex, length, ret);
            return ret;
        }

        /// <summary>
        /// Copies the requested row elements into a new <see cref="Vector"/>.
        /// </summary>
        /// <param name="rowIndex">The row to copy elements from.</param>
        /// <param name="columnIndex">The column to start copying from.</param>
        /// <param name="length">The number of elements to copy.</param>
        /// <param name="result">The <see cref="Vector"/> to copy the column into.</param>
        /// <exception cref="ArgumentNullException">If the result <see cref="Vector"/> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="rowIndex"/> is negative,
        /// or greater than or equal to the number of columns.</exception>        
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="columnIndex"/> is negative,
        /// or greater than or equal to the number of rows.</exception>        
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="columnIndex"/> + <paramref name="length"/>  
        /// is greater than or equal to the number of rows.</exception>
        /// <exception cref="ArgumentException">If <paramref name="length"/> is not positive.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If <strong>result.Count &lt; length</strong>.</exception>
        public virtual void Row(int rowIndex, int columnIndex, int length, Vector result)
        {
            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            if (rowIndex >= this.RowCount || rowIndex < 0)
            {
                throw new ArgumentOutOfRangeException("rowIndex");
            }

            if (columnIndex >= this.ColumnCount || columnIndex < 0)
            {
                throw new ArgumentOutOfRangeException("columnIndex");
            }

            if (columnIndex + length > this.ColumnCount)
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

            for (int i = columnIndex, j = 0; i < columnIndex + length; i++, j++)
            {
                result[j] = this.At(rowIndex, i);
            }
        }

        /// <summary>
        /// Copies a column into a new <see cref="Vector"/>.
        /// </summary>
        /// <param name="index">The column to copy.</param>
        /// <returns>A <see cref="Vector"/> containing the copied elements.</returns>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="index"/> is negative,
        /// or greater than or equal to the number of columns.</exception>
        public virtual Vector Column(int index)
        {
            var result = this.CreateVector(this.RowCount);
            this.Column(index, 0, this.RowCount, result);
            return result;
        }

        /// <summary>
        /// Copies a column into to the given <see cref="Vector"/>.
        /// </summary>
        /// <param name="index">The column to copy.</param>
        /// <param name="result">The <see cref="Vector"/> to copy the column into.</param>
        /// <exception cref="ArgumentNullException">If the result <see cref="Vector"/> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="index"/> is negative,
        /// or greater than or equal to the number of columns.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If <b>this.Rows != result.Count</b>.</exception>
        public virtual void Column(int index, Vector result)
        {
            this.Column(index, 0, this.RowCount, result);
        }

        /// <summary>
        /// Copies the requested column elements into a new <see cref="Vector"/>.
        /// </summary>
        /// <param name="columnIndex">The column to copy elements from.</param>
        /// <param name="rowIndex">The row to start copying from.</param>
        /// <param name="length">The number of elements to copy.</param>
        /// <returns>A <see cref="Vector"/> containing the requested elements.</returns>
        /// <exception cref="ArgumentOutOfRangeException">If:
        /// <list><item><paramref name="columnIndex"/> is negative,
        /// or greater than or equal to the number of columns.</item>
        /// <item><paramref name="rowIndex"/> is negative,
        /// or greater than or equal to the number of rows.</item>
        /// <item><c>(rowIndex + length) &gt;= Rows.</c></item></list>
        /// </exception>        
        /// <exception cref="ArgumentException">If <paramref name="length"/> is not positive.</exception>
        public virtual Vector Column(int columnIndex, int rowIndex, int length)
        {
            var result = this.CreateVector(length);
            this.Column(columnIndex, rowIndex, length, result);
            return result;
        }

        /// <summary>
        /// Copies the requested column elements into the given vector.
        /// </summary>
        /// <param name="columnIndex">The column to copy elements from.</param>
        /// <param name="rowIndex">The row to start copying from.</param>
        /// <param name="length">The number of elements to copy.</param>
        /// <param name="result">The <see cref="Vector"/> to copy the column into.</param>
        /// <exception cref="ArgumentNullException">If the result <see cref="Vector"/> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="columnIndex"/> is negative,
        /// or greater than or equal to the number of columns.</exception>        
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="rowIndex"/> is negative,
        /// or greater than or equal to the number of rows.</exception>        
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="rowIndex"/> + <paramref name="length"/>  
        /// is greater than or equal to the number of rows.</exception>
        /// <exception cref="ArgumentException">If <paramref name="length"/> is not positive.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If <strong>result.Count &lt; length</strong>.</exception>
        public virtual void Column(int columnIndex, int rowIndex, int length, Vector result)
        {
            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            if (columnIndex >= this.ColumnCount || columnIndex < 0)
            {
                throw new ArgumentOutOfRangeException("columnIndex");
            }

            if (rowIndex >= this.RowCount || rowIndex < 0)
            {
                throw new ArgumentOutOfRangeException("rowIndex");
            }

            if (rowIndex + length > this.RowCount)
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

            for (int i = rowIndex, j = 0; i < rowIndex + length; i++, j++)
            {
                result[j] = this.At(i, columnIndex);
            }
        }

        /// <summary>
        /// Returns a new matrix containing the lower triangle of this matrix.
        /// </summary>
        /// <returns>The lower triangle of this matrix.</returns>        
        public virtual Matrix LowerTriangle()
        {
            var ret = this.CreateMatrix(this.RowCount, this.ColumnCount);
            CommonParallel.For(
                0, 
                this.ColumnCount, 
                j =>
                {
                    for (var i = j; i < this.RowCount; i++)
                    {
                        ret.At(i, j, this.At(i, j));
                    }
                });
            return ret;
        }

        /// <summary>
        /// Puts the lower triangle of this matrix into the result matrix.
        /// </summary>
        /// <param name="result">Where to store the lower triangle.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="result"/> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException">If the result matrix's dimensions are not the same as this matrix.</exception>
        public virtual void LowerTriangle(Matrix result)
        {
            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            if (result.RowCount != this.RowCount || result.ColumnCount != this.ColumnCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixDimensions, "result");
            }

            CommonParallel.For(
                0, 
                this.ColumnCount, 
                j =>
                {
                    for (var i = 0; i < this.RowCount; i++)
                    {
                        result.At(i, j, i >= j ? this.At(i, j) : 0);
                    }
                });
        }

        /// <summary>
        /// Returns a new matrix containing the upper triangle of this matrix.
        /// </summary>
        /// <returns>The upper triangle of this matrix.</returns>   
        public virtual Matrix UpperTriangle()
        {
            var ret = this.CreateMatrix(this.RowCount, this.ColumnCount);
            CommonParallel.For(
                0, 
                this.ColumnCount, 
                j =>
                {
                    for (var i = 0; i < this.RowCount; i++)
                    {
                        if (i <= j)
                        {
                            ret.At(i, j, this.At(i, j));
                        }
                    }
                });
            return ret;
        }

        /// <summary>
        /// Puts the upper triangle of this matrix into the result matrix.
        /// </summary>
        /// <param name="result">Where to store the lower triangle.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="result"/> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException">If the result matrix's dimensions are not the same as this matrix.</exception>
        public virtual void UpperTriangle(Matrix result)
        {
            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            if (result.RowCount != this.RowCount || result.ColumnCount != this.ColumnCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixDimensions, "result");
            }

            CommonParallel.For(
                0, 
                this.ColumnCount, 
                j =>
                {
                    for (var i = 0; i < this.RowCount; i++)
                    {
                        result.At(i, j, i <= j ? this.At(i, j) : 0);
                    }
                });
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
        public virtual Matrix SubMatrix(int rowIndex, int rowLength, int columnIndex, int columnLength)
        {
            if (rowIndex >= this.RowCount || rowIndex < 0)
            {
                throw new ArgumentOutOfRangeException("rowIndex");
            }

            if (columnIndex >= this.ColumnCount || columnIndex < 0)
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

            if (rowMax > this.RowCount)
            {
                throw new ArgumentOutOfRangeException("rowLength");
            }

            if (colMax > this.ColumnCount)
            {
                throw new ArgumentOutOfRangeException("columnLength");
            }

            var result = this.CreateMatrix(rowLength, columnLength);

            CommonParallel.For(
                columnIndex, 
                colMax, 
                j =>
                {
                    for (int i = rowIndex, ii = 0; i < rowMax; i++, ii++)
                    {
                        result.At(ii, j - columnIndex, this.At(i, j));
                    }
                });
            return result;
        }

        /// <summary>
        /// Returns an <see cref="IEnumerator{T}"/> that enumerates over the matrix columns.
        /// </summary>
        /// <returns>An <see cref="IEnumerator{T}"/> that enumerates over the matrix columns</returns>
        /// <seealso cref="IEnumerator{T}"/>
        public virtual IEnumerable<KeyValuePair<int, Vector>> ColumnEnumerator()
        {
            for (var i = 0; i < this.ColumnCount; i++)
            {
                yield return new KeyValuePair<int, Vector>(i, this.Column(i));
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
        public virtual IEnumerable<KeyValuePair<int, Vector>> ColumnEnumerator(int index, int length)
        {
            if (index >= this.ColumnCount || index < 0)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            if (index + length > this.ColumnCount)
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
                yield return new KeyValuePair<int, Vector>(i, this.Column(i));
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
        public virtual IEnumerable<KeyValuePair<int, Vector>> RowEnumerator(int index, int length)
        {
            if (index >= this.RowCount || index < 0)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            if (index + length > this.RowCount)
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
                yield return new KeyValuePair<int, Vector>(i, this.Row(i));
            }
        }

        /// <summary>
        /// Returns an <see cref="IEnumerator{T}"/> that enumerates over the matrix rows.
        /// </summary>
        /// <returns>An <see cref="IEnumerator{T}"/> that enumerates over the matrix rows</returns>
        /// <seealso cref="IEnumerator{T}"/>        
        public virtual IEnumerable<KeyValuePair<int, Vector>> RowEnumerator()
        {
            for (var i = 0; i < this.RowCount; i++)
            {
                yield return new KeyValuePair<int, Vector>(i, this.Row(i));
            }
        }

        /// <summary>
        /// Returns the elements of the diagonal in a <see cref="Vector"/>.
        /// </summary>
        /// <returns>The elements of the diagonal.</returns>
        /// <remarks>For non-square matrices, the method returns Min(Rows, Columns) elements where
        /// i == j (i is the row index, and j is the column index).</remarks>
        public virtual Vector Diagonal()
        {
            var min = Math.Min(this.RowCount, this.ColumnCount);
            var diagonal = this.CreateVector(min);
            CommonParallel.For(
                0, 
                min, 
                i => { diagonal[i] = this.At(i, i); });
            return diagonal;
        }

        /// <summary>
        /// Returns a new matrix containing the lower triangle of this matrix. The new matrix
        /// does not contain the diagonal elements of this matrix.
        /// </summary>
        /// <returns>The lower triangle of this matrix.</returns>
        public virtual Matrix StrictlyLowerTriangle()
        {
            var result = this.CreateMatrix(this.RowCount, this.ColumnCount);
            CommonParallel.For(
                0, 
                this.RowCount, 
                i =>
                {
                    for (var j = 0; j < this.ColumnCount; j++)
                    {
                        if (i > j)
                        {
                            result.At(i, j, this.At(i, j));
                        }
                    }
                });
            return result;
        }

        /// <summary>
        /// Puts the strictly lower triangle of this matrix into the result matrix.
        /// </summary>
        /// <param name="result">Where to store the lower triangle.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="result"/> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException">If the result matrix's dimensions are not the same as this matrix.</exception>
        public virtual void StrictlyLowerTriangle(Matrix result)
        {
            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            if (result.RowCount != this.RowCount || result.ColumnCount != this.ColumnCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixDimensions, "result");
            }

            CommonParallel.For(
                0, 
                this.RowCount, 
                i =>
                {
                    for (var j = 0; j < this.ColumnCount; j++)
                    {
                        result.At(i, j, i > j ? this.At(i, j) : 0);
                    }
                });
        }

        /// <summary>
        /// Returns a new matrix containing the upper triangle of this matrix. The new matrix
        /// does not contain the diagonal elements of this matrix.
        /// </summary>
        /// <returns>The upper triangle of this matrix.</returns>
        public virtual Matrix StrictlyUpperTriangle()
        {
            var result = this.CreateMatrix(this.RowCount, this.ColumnCount);
            CommonParallel.For(
                0, 
                this.RowCount, 
                i =>
                {
                    for (var j = 0; j < this.ColumnCount; j++)
                    {
                        if (i < j)
                        {
                            result.At(i, j, this.At(i, j));
                        }
                    }
                });
            return result;
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
        public virtual Matrix InsertColumn(int columnIndex, Vector column)
        {
            if (column == null)
            {
                throw new ArgumentNullException("column");
            }

            if (columnIndex < 0 || columnIndex > this.ColumnCount)
            {
                throw new ArgumentOutOfRangeException("columnIndex");
            }

            if (column.Count != this.RowCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixSameRowDimension, "column");
            }

            var result = this.CreateMatrix(this.RowCount, this.ColumnCount + 1);

            for (var i = 0; i < columnIndex; i++)
            {
                result.SetColumn(i, this.Column(i));
            }

            result.SetColumn(columnIndex, column);

            for (var i = columnIndex + 1; i < this.ColumnCount + 1; i++)
            {
                result.SetColumn(i, this.Column(i - 1));
            }

            return result;
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
        public virtual void SetColumn(int columnIndex, double[] column)
        {
            if (columnIndex < 0 || columnIndex >= this.ColumnCount)
            {
                throw new ArgumentOutOfRangeException("columnIndex");
            }

            if (column == null)
            {
                throw new ArgumentNullException("column");
            }

            if (column.Length != this.RowCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixSameRowDimension, "column");
            }

            CommonParallel.For(
                0, 
                this.RowCount, 
                i => this.At(i, columnIndex, column[i]));
        }

        /// <summary>
        /// Copies the values of the given <see cref="Vector"/> to the specified column.
        /// </summary>
        /// <param name="columnIndex">The column to copy the values to.</param>
        /// <param name="column">The vector to copy the values from.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="column"/> is <see langword="null" />.</exception>        
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="columnIndex"/> is less than zero,
        /// or greater than or equal to the number of columns.</exception>
        /// <exception cref="ArgumentException">If the size of <paramref name="column"/> does not
        /// equal the number of rows of this <strong>Matrix</strong>.</exception>
        public virtual void SetColumn(int columnIndex, Vector column)
        {
            if (columnIndex < 0 || columnIndex >= this.ColumnCount)
            {
                throw new ArgumentOutOfRangeException("columnIndex");
            }

            if (column == null)
            {
                throw new ArgumentNullException("column");
            }

            if (column.Count != this.RowCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixSameRowDimension, "column");
            }

            CommonParallel.For(
                0, 
                this.RowCount, 
                i => this.At(i, columnIndex, column[i]));
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
        public virtual Matrix InsertRow(int rowIndex, Vector row)
        {
            if (row == null)
            {
                throw new ArgumentNullException("row");
            }

            if (rowIndex < 0 || rowIndex > this.RowCount)
            {
                throw new ArgumentOutOfRangeException("rowIndex");
            }

            if (row.Count != this.ColumnCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixSameRowDimension, "row");
            }

            var result = this.CreateMatrix(this.RowCount + 1, this.ColumnCount);

            for (var i = 0; i < rowIndex; i++)
            {
                result.SetRow(i, this.Row(i));
            }

            result.SetRow(rowIndex, row);

            for (var i = rowIndex + 1; i < this.RowCount; i++)
            {
                result.SetRow(i, this.Row(i - 1));
            }

            return result;
        }

        /// <summary>
        /// Copies the values of the given <see cref="Vector"/> to the specified row.
        /// </summary>
        /// <param name="rowIndex">The row to copy the values to.</param>
        /// <param name="row">The vector to copy the values from.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="row"/> is <see langword="null" />.</exception>            
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="rowIndex"/> is less than zero,
        /// or greater than or equal to the number of rows.</exception>
        /// <exception cref="ArgumentException">If the size of <paramref name="row"/> does not
        /// equal the number of columns of this <strong>Matrix</strong>.</exception>
        public virtual void SetRow(int rowIndex, Vector row)
        {
            if (rowIndex < 0 || rowIndex >= this.RowCount)
            {
                throw new ArgumentOutOfRangeException("rowIndex");
            }

            if (row == null)
            {
                throw new ArgumentNullException("row");
            }

            if (row.Count != this.ColumnCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixSameRowDimension, "row");
            }

            CommonParallel.For(
                0, 
                this.ColumnCount, 
                i => this.At(rowIndex, i, row[i]));
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
        public virtual void SetRow(int rowIndex, double[] row)
        {
            if (rowIndex < 0 || rowIndex >= this.RowCount)
            {
                throw new ArgumentOutOfRangeException("rowIndex");
            }

            if (row == null)
            {
                throw new ArgumentNullException("row");
            }

            if (row.Length != this.ColumnCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixSameRowDimension, "row");
            }

            CommonParallel.For(
                0, 
                this.ColumnCount, 
                i => this.At(rowIndex, i, row[i]));
        }

        /// <summary>
        /// Copies the values of a given matrix into a region in this matrix.
        /// </summary>
        /// <param name="rowIndex">The row to start copying to.</param>
        /// <param name="rowLength">The number of rows to copy. Must be positive.</param>
        /// <param name="columnIndex">The column to start copying to.</param>
        /// <param name="columnLength">The number of columns to copy. Must be positive.</param>
        /// <param name="subMatrix">The sub-matrix to copy from.</param>
        /// <exception cref="ArgumentOutOfRangeException">If: <list><item><paramref name="rowIndex"/> is
        /// negative, or greater than or equal to the number of rows.</item>
        /// <item><paramref name="columnIndex"/> is negative, or greater than or equal to the number 
        /// of columns.</item>
        /// <item><c>(columnIndex + columnLength) &gt;= Columns</c></item>
        /// <item><c>(rowIndex + rowLength) &gt;= Rows</c></item></list></exception> 
        /// <exception cref="ArgumentNullException">If <paramref name="subMatrix"/> is <see langword="null" /></exception>
        /// <item>the size of <paramref name="subMatrix"/> is not at least <paramref name="rowLength"/> x <paramref name="columnLength"/>.</item>
        /// <exception cref="ArgumentException">If <paramref name="rowLength"/> or <paramref name="columnLength"/>
        /// is not positive.</exception>
        public virtual void SetSubMatrix(int rowIndex, int rowLength, int columnIndex, int columnLength, Matrix subMatrix)
        {
            if (rowIndex >= this.RowCount || rowIndex < 0)
            {
                throw new ArgumentOutOfRangeException("rowIndex");
            }

            if (columnIndex >= this.ColumnCount || columnIndex < 0)
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

            if (subMatrix == null)
            {
                throw new ArgumentNullException("subMatrix");
            }

            if (columnLength > subMatrix.ColumnCount)
            {
                throw new ArgumentOutOfRangeException("columnLength", "columnLength can be at most the number of columns in subMatrix.");
            }

            if (rowLength > subMatrix.RowCount)
            {
                throw new ArgumentOutOfRangeException("rowLength", "rowLength can be at most the number of rows in subMatrix.");
            }

            var colMax = columnIndex + columnLength;
            var rowMax = rowIndex + rowLength;

            if (rowMax > this.RowCount)
            {
                throw new ArgumentOutOfRangeException("rowLength");
            }

            if (colMax > this.ColumnCount)
            {
                throw new ArgumentOutOfRangeException("columnLength");
            }

            CommonParallel.For(
                columnIndex, 
                colMax, 
                j =>
                {
                    for (int i = rowIndex, ii = 0; i < rowMax; i++, ii++)
                    {
                        this.At(i, j, subMatrix[ii, j - columnIndex]);
                    }
                });
        }

        /// <summary>
        /// Copies the values of the given <see cref="Vector"/> to the diagonal.
        /// </summary>
        /// <param name="source">The vector to copy the values from. The length of the vector should be
        /// Min(Rows, Columns).</param>
        /// <exception cref="ArgumentNullException">If <paramref name="source"/> is <see langword="null" />.</exception>   
        /// <exception cref="ArgumentException">If the length of <paramref name="source"/> does not
        /// equal Min(Rows, Columns).</exception>
        /// <remarks>For non-square matrices, the elements of <paramref name="source"/> are copied to
        /// this[i,i].</remarks>
        public virtual void SetDiagonal(Vector source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            var min = Math.Min(this.RowCount, this.ColumnCount);

            if (source.Count != min)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "source");
            }

            CommonParallel.For(
                0, 
                min, 
                i => this.At(i, i, source[i]));
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
        public virtual void SetDiagonal(double[] source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            var min = Math.Min(this.RowCount, this.ColumnCount);

            if (source.Length != min)
            {
                throw new ArgumentException(Resources.ArgumentArraysSameLength, "source");
            }

            CommonParallel.For(
                0, 
                min, 
                i => this.At(i, i, source[i]));
        }

        /// <summary>
        /// Puts the strictly upper triangle of this matrix into the result matrix.
        /// </summary>
        /// <param name="result">Where to store the lower triangle.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="result"/> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException">If the result matrix's dimensions are not the same as this matrix.</exception>
        public virtual void StrictlyUpperTriangle(Matrix result)
        {
            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            if (result.RowCount != this.RowCount || result.ColumnCount != this.ColumnCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixDimensions, "result");
            }

            CommonParallel.For(
                0, 
                this.RowCount, 
                i =>
                {
                    for (var j = 0; j < this.ColumnCount; j++)
                    {
                        result.At(i, j, i < j ? this.At(i, j) : 0);
                    }
                });
        }

        /// <summary>
        /// Returns this matrix as a multidimensional array.
        /// </summary>
        /// <returns>A multidimensional containing the values of this matrix.</returns>        
        public virtual double[,] ToArray()
        {
            var ret = new double[this.RowCount, this.ColumnCount];
            CommonParallel.For(
                0, 
                this.ColumnCount, 
                j =>
                {
                    for (var i = 0; i < this.RowCount; i++)
                    {
                        ret[i, j] = this.At(i, j);
                    }
                });
            return ret;
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
        public virtual double[] ToColumnWiseArray()
        {
            var ret = new double[this.RowCount * this.ColumnCount];
            foreach (var column in this.ColumnEnumerator())
            {
                var columnIndex = column.Key * this.RowCount;
                foreach (var element in column.Value.GetIndexedEnumerator())
                {
                    ret[columnIndex + element.Key] = element.Value;
                }
            }

            return ret;
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
        public virtual double[] ToRowWiseArray()
        {
            var ret = new double[this.RowCount * this.ColumnCount];

            foreach (var row in this.RowEnumerator())
            {
                var rowIndex = row.Key * this.ColumnCount;
                foreach (var element in row.Value.GetIndexedEnumerator())
                {
                    ret[rowIndex + element.Key] = element.Value;
                }
            }

            return ret;
        }

        #region Implemented Interfaces

#if !SILVERLIGHT

        #region ICloneable

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        object ICloneable.Clone()
        {
            return this.Clone();
        }

        #endregion

#endif

        #region IEquatable<Matrix>

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">
        /// An object to compare with this object.
        /// </param>
        /// <returns>
        /// <c>true</c> if the current object is equal to the <paramref name="other"/> parameter; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(Matrix other)
        {
            // Reject equality when the argument is null or has a different shape.
            if (other == null)
            {
                return false;
            }

            if (this.ColumnCount != other.ColumnCount || this.RowCount != other.RowCount)
            {
                return false;
            }

            // Accept if the argument is the same object as this.
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            // If all else fails, perform element wise comparison.
            for (var row = 0; row < this.RowCount; row++)
            {
                for (var column = 0; column < this.ColumnCount; column++)
                {
                    if (this.At(row, column) != other.At(row, column))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        #endregion

        #region IFormattable

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
        public string ToString(string format, IFormatProvider formatProvider)
        {
            var stringBuilder = new StringBuilder();
            for (var row = 0; row < this.RowCount; row++)
            {
                for (var column = 0; column < this.ColumnCount; column++)
                {
                    stringBuilder.Append(this.At(row, column).ToString(format, formatProvider));
                    if (column != this.ColumnCount - 1)
                    {
                        stringBuilder.Append(formatProvider.GetTextInfo().ListSeparator);
                    }
                }

                if (row != this.RowCount - 1)
                {
                    stringBuilder.Append(Environment.NewLine);
                }
            }

            return stringBuilder.ToString();
        }

        #endregion

        #endregion

        /// <summary>
        /// Ranges the check.
        /// </summary>
        /// <param name="row">
        /// The row of the element.
        /// </param>
        /// <param name="column">
        /// The column of the element.
        /// </param>
        private void RangeCheck(int row, int column)
        {
            if (row < 0 || row >= this.RowCount)
            {
                throw new ArgumentOutOfRangeException("row");
            }

            if (column < 0 || column >= this.ColumnCount)
            {
                throw new ArgumentOutOfRangeException("column");
            }
        }

        #region System.Object overrides

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns>
        ///     <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            return this.Equals(obj as Matrix);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            var hashNum = Math.Min(this.RowCount * this.ColumnCount, 25);
            long hash = 0;
            for (var i = 0; i < hashNum; i++)
            {
                var col = i % this.ColumnCount;
                var row = (i - col) / this.RowCount;

#if SILVERLIGHT
                hash ^= Precision.DoubleToInt64Bits(this[row, col]);
#else
                hash ^= BitConverter.DoubleToInt64Bits(this[row, col]);
#endif
            }

            return BitConverter.ToInt32(BitConverter.GetBytes(hash), 4);
        }

        #endregion

        /// <summary>
        /// Sets all values to zero.
        /// </summary>
        public virtual void Clear()
        {
            for (var i = 0; i < this.RowCount; i++)
            {
                for (var j = 0; j < this.ColumnCount; j++)
                {
                    this.At(i, j, 0);
                }
            }
        }

        /// <summary>
        /// Returns the transpose of this matrix.
        /// </summary>        
        /// <returns>The transpose of this matrix.</returns>
        public virtual Matrix Transpose()
        {
            var ret = this.CreateMatrix(this.ColumnCount, this.RowCount);
            for (var j = 0; j < this.ColumnCount; j++)
            {
                for (var i = 0; i < this.RowCount; i++)
                {
                    ret.At(j, i, this.At(i, j));
                }
            }

            return ret;
        }

        /// <summary>
        /// Permute the rows of a matrix according to a permutation.
        /// </summary>
        /// <param name="p">The row permutation to apply to this matrix.</param>
        public virtual void PermuteRows(Permutation p)
        {
            if (p.Dimension != this.RowCount)
            {
                throw new ArgumentException(Resources.ArgumentArraysSameLength, "p");
            }

            // Get a sequence of inversions from the permutation.
            var inv = p.ToInversions();

            for (var i = 0; i < p.Dimension; i++)
            {
                if (inv[i] != i)
                {
                    var q = inv[i];
                    for (var j = 0; j < this.ColumnCount; j++)
                    {
                        var temp = this.At(q, j);
                        this.At(q, j, this.At(i, j));
                        this.At(i, j, temp);
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
            if (p.Dimension != this.ColumnCount)
            {
                throw new ArgumentException(Resources.ArgumentArraysSameLength, "p");
            }

            // Get a sequence of inversions from the permutation.
            var inv = p.ToInversions();

            for (var i = 0; i < p.Dimension; i++)
            {
                if (inv[i] != i)
                {
                    var q = inv[i];
                    for (var j = 0; j < this.RowCount; j++)
                    {
                        var temp = this.At(j, q);
                        this.At(j, q, this.At(j, i));
                        this.At(j, i, temp);
                    }
                }
            }
        }

        /// <summary>
        ///  Concatenates this matrix with the given matrix.
        /// </summary>
        /// <param name="right">The matrix to concatenate.</param>
        /// <returns>The combined matrix.</returns>
        public virtual Matrix Append(Matrix right)
        {
            if (right == null)
            {
                throw new ArgumentNullException("right");
            }

            if (right.RowCount != this.RowCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixSameRowDimension);
            }

            var result = this.CreateMatrix(this.RowCount, this.ColumnCount + right.ColumnCount);
            this.Append(right, result);
            return result;
        }

        /// <summary>
        /// Concatenates this matrix with the given matrix and places the result into the result matrix.
        /// </summary>
        /// <param name="right">The matrix to concatenate.</param>
        /// <param name="result">The combined matrix.</param>
        public virtual void Append(Matrix right, Matrix result)
        {
            if (right == null)
            {
                throw new ArgumentNullException("right");
            }

            if (right.RowCount != this.RowCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixSameRowDimension);
            }

            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            if (result.ColumnCount != (this.ColumnCount + right.ColumnCount) || result.RowCount != this.RowCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixSameColumnDimension);
            }

            CommonParallel.Invoke(
                () =>
                {
                    for (var i = 0; i < this.RowCount; i++)
                    {
                        for (var j = 0; j < this.ColumnCount; j++)
                        {
                            result.At(i, j, this.At(i, j));
                        }
                    }
                }, 
                () =>
                {
                    for (var i = 0; i < this.RowCount; i++)
                    {
                        for (var j = 0; j < right.ColumnCount; j++)
                        {
                            result.At(i, j + this.ColumnCount, right.At(i, j));
                        }
                    }
                });
        }

        /// <summary>
        /// Stacks this matrix on top of the given matrix and places the result into the result matrix.
        /// </summary>
        /// <param name="lower">The matrix to stack this matrix upon.</param>
        /// <returns>The combined matrix.</returns>
        /// <exception cref="ArgumentNullException">If lower is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException">If <strong>upper.Columns != lower.Columns</strong>.</exception>
        public virtual Matrix Stack(Matrix lower)
        {
            if (lower == null)
            {
                throw new ArgumentNullException("lower");
            }

            if (lower.ColumnCount != this.ColumnCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixSameColumnDimension, "lower");
            }

            var result = this.CreateMatrix(this.RowCount + lower.RowCount, this.ColumnCount);
            this.Stack(lower, result);
            return result;
        }

        /// <summary>
        /// Stacks this matrix on top of the given matrix and places the result into the result matrix.
        /// </summary>
        /// <param name="lower">The matrix to stack this matrix upon.</param>
        /// <param name="result">The combined matrix.</param>
        /// <exception cref="ArgumentNullException">If lower is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException">If <strong>upper.Columns != lower.Columns</strong>.</exception>
        public virtual void Stack(Matrix lower, Matrix result)
        {
            if (lower == null)
            {
                throw new ArgumentNullException("lower");
            }

            if (lower.ColumnCount != this.ColumnCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixSameColumnDimension, "lower");
            }

            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            if (result.RowCount != (this.RowCount + lower.RowCount) || result.ColumnCount != this.ColumnCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixDimensions, "result");
            }

            CommonParallel.Invoke(
                () =>
                {
                    for (var i = 0; i < this.RowCount; i++)
                    {
                        for (var j = 0; j < this.ColumnCount; j++)
                        {
                            result.At(i, j, this.At(i, j));
                        }
                    }
                }, 
                () =>
                {
                    for (var i = 0; i < lower.RowCount; i++)
                    {
                        for (var j = 0; j < this.ColumnCount; j++)
                        {
                            result.At(i + this.RowCount, j, lower.At(i, j));
                        }
                    }
                });
        }

        /// <summary>
        /// Diagonally stacks his matrix on top of the given matrix. The new matrix is a M-by-N matrix, 
        /// where M = this.Rows + lower.Rows and N = this.Columns + lower.Columns.
        /// The values of off the off diagonal matrices/blocks are set to zero.
        /// </summary>
        /// <param name="lower">The lower, right matrix.</param>
        /// <exception cref="ArgumentNullException">If lower is <see langword="null" />.</exception>
        /// <returns>the combined matrix</returns>
        public virtual Matrix DiagonalStack(Matrix lower)
        {
            if (lower == null)
            {
                throw new ArgumentNullException("lower");
            }

            var result = this.CreateMatrix(this.RowCount + lower.RowCount, this.ColumnCount + lower.ColumnCount);
            this.DiagonalStack(lower, result);
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
        public virtual void DiagonalStack(Matrix lower, Matrix result)
        {
            if (lower == null)
            {
                throw new ArgumentNullException("lower");
            }

            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            if (result.RowCount != this.RowCount + lower.RowCount || result.ColumnCount != this.ColumnCount + lower.ColumnCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixDimensions, "result");
            }

            CommonParallel.Invoke(
                () =>
                {
                    for (var i = 0; i < this.RowCount; i++)
                    {
                        for (var j = 0; j < this.ColumnCount; j++)
                        {
                            result.At(i, j, this.At(i, j));
                        }
                    }
                }, 
                () =>
                {
                    for (var i = 0; i < lower.RowCount; i++)
                    {
                        for (var j = 0; j < lower.ColumnCount; j++)
                        {
                            result.At(i + this.RowCount, j + this.ColumnCount, lower.At(i, j));
                        }
                    }
                });
        }
    }
}