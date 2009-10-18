// <copyright file="Matrix.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://mathnet.opensourcedotnet.info
//
// Copyright (c) 2009 Math.NET
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

    using Properties;

    /// <summary>
    /// Defines the base class for <c>Matrix</c> classes.
    /// </summary>
    [Serializable]
    public abstract class Matrix : IFormattable, ICloneable, IEquatable<Matrix>
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

            RowCount = rows;
            ColumnCount = columns;
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

            RowCount = order;
            ColumnCount = order;
        }

        /// <summary>
        /// Gets the number of columns.
        /// </summary>
        /// <value>The number of columns.</value>
        public virtual int ColumnCount { get; private set; }

        /// <summary>
        /// Gets the number of rows.
        /// </summary>
        /// <value>The number of rows.</value>
        public virtual int RowCount { get; private set; }

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
                RangeCheck(row, column);
                return At(row, column);
            }

            set
            {
                RangeCheck(row, column);
                At(row, column, value);
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
            var result = CreateMatrix(RowCount, ColumnCount);
            CopyTo(result);
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

            if (RowCount != target.RowCount || ColumnCount != target.ColumnCount)
            {
                throw new ArgumentException("target", Resources.ArgumentMatrixSameDimensions);
            }

            var denseMatrix = target as DenseMatrix;
            if (denseMatrix != null)
            {
                // TODO this assumes that all entries matter; if "this" is a sparse matrix,
                // we might be able to optimize the copying a bit.
                for (int i = 0; i < RowCount; i++)
                {
                    for (int j = 0; j < ColumnCount; j++)
                    {
                        denseMatrix.At(i, j, this.At(i, j));
                    }
                }

                return;
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
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.ToString(null, null);
        }

        #region Implemented Interfaces

        #region ICloneable

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        object ICloneable.Clone()
        {
            return Clone();
        }

        #endregion

        #region IEquatable<Matrix>

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">
        /// An object to compare with this object.
        /// </param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        public bool Equals(Matrix other)
        {
            // Reject equality when the argument is null or has a different shape.
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

            // If all else fails, perform element wise comparison.
            for (var row = 0; row < RowCount; row++)
            {
                for (var column = 0; column < ColumnCount; column++)
                {
                    if (At(row, column) != other.At(row, column))
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
            if (row < 0 || row >= RowCount)
            {
                throw new ArgumentOutOfRangeException("row");
            }

            if (column < 0 || column >= ColumnCount)
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
            return Equals(obj as Matrix);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            var hashNum = Math.Min(RowCount * ColumnCount, 25);
            long hash = 0;
            for (var i = 0; i < hashNum; i++)
            {
                var col = i % ColumnCount;
                var row = (i - col) / RowCount;

                hash ^= BitConverter.DoubleToInt64Bits(this[row, col]);
            }

            return BitConverter.ToInt32(BitConverter.GetBytes(hash), 4);
        }

        #endregion
    }
}