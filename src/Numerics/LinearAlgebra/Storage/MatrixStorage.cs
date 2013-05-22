// <copyright file="MatrixStorage.cs" company="Math.NET">
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

using System;
using MathNet.Numerics.Properties;

namespace MathNet.Numerics.LinearAlgebra.Storage
{
    [Serializable]
    public abstract partial class MatrixStorage<T> : IEquatable<MatrixStorage<T>>
        where T : struct, IEquatable<T>, IFormattable
    {
        // [ruegg] public fields are OK here

        protected static readonly T Zero = Common.ZeroOf<T>();
        public readonly int RowCount;
        public readonly int ColumnCount;

        protected MatrixStorage(int rowCount, int columnCount)
        {
            if (rowCount <= 0)
            {
                throw new ArgumentOutOfRangeException(Resources.MatrixRowsMustBePositive);
            }

            if (columnCount <= 0)
            {
                throw new ArgumentOutOfRangeException(Resources.MatrixColumnsMustBePositive);
            }

            RowCount = rowCount;
            ColumnCount = columnCount;
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
            get
            {
                ValidateRange(row, column);
                return At(row, column);
            }

            set
            {
                ValidateRange(row, column);
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
        /// <remarks>Not range-checked.</remarks>
        public abstract T At(int row, int column);

        /// <summary>
        /// Sets the element without range checking.
        /// </summary>
        /// <param name="row"> The row of the element. </param>
        /// <param name="column"> The column of the element. </param>
        /// <param name="value"> The value to set the element to. </param>
        /// <remarks>WARNING: This method is not thread safe. Use "lock" with it and be sure to avoid deadlocks.</remarks>
        public abstract void At(int row, int column, T value);

        /// <summary>
        /// True if all fields of this matrix can be set to any value.
        /// False if some fields are fixed, like on a diagonal matrix.
        /// </summary>
        public virtual bool IsFullyMutable
        {
            get { return true; }
        }

        /// <summary>
        /// True if the specified field can be set to any value.
        /// False if the field is fixed, like an off-diagonal field on a diagonal matrix.
        /// </summary>
        public virtual bool IsMutable(int row, int column)
        {
            return true;
        }

        public virtual void Clear()
        {
            for (var i = 0; i < RowCount; i++)
            {
                for (var j = 0; j < ColumnCount; j++)
                {
                    At(i, j, Zero);
                }
            }
        }

        public virtual void Clear(int rowIndex, int rowCount, int columnIndex, int columnCount)
        {
            for (var i = rowIndex; i < rowIndex + rowCount; i++)
            {
                for (var j = columnIndex; j < columnIndex + columnCount; j++)
                {
                    At(i, j, Zero);
                }
            }
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
        public virtual bool Equals(MatrixStorage<T> other)
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
                    if (!At(row, column).Equals(other.At(row, column)))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
        /// </returns>
        /// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>. </param>
        public override sealed bool Equals(object obj)
        {
            return Equals(obj as MatrixStorage<T>);
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        public override int GetHashCode()
        {
            var hashNum = Math.Min(RowCount*ColumnCount, 25);
            int hash = 17;
            unchecked
            {
                for (var i = 0; i < hashNum; i++)
                {
                    var col = i%ColumnCount;
                    var row = (i - col)/RowCount;
                    hash = hash*31 + At(row, col).GetHashCode();
                }
            }
            return hash;
        }

        // MATRIX COPY

        public void CopyTo(MatrixStorage<T> target, bool skipClearing = false)
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
                var message = string.Format(Resources.ArgumentMatrixDimensions2, RowCount + "x" + ColumnCount, target.RowCount + "x" + target.ColumnCount);
                throw new ArgumentException(message, "target");
            }

            CopyToUnchecked(target, skipClearing);
        }

        internal virtual void CopyToUnchecked(MatrixStorage<T> target, bool skipClearing = false)
        {
            for (int j = 0; j < ColumnCount; j++)
            {
                for (int i = 0; i < RowCount; i++)
                {
                    target.At(i, j, At(i, j));
                }
            }
        }

        public void CopySubMatrixTo(MatrixStorage<T> target,
            int sourceRowIndex, int targetRowIndex, int rowCount,
            int sourceColumnIndex, int targetColumnIndex, int columnCount,
            bool skipClearing = false)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }

            if (ReferenceEquals(this, target))
            {
                throw new NotSupportedException();
            }

            ValidateSubMatrixRange(target,
                sourceRowIndex, targetRowIndex, rowCount,
                sourceColumnIndex, targetColumnIndex, columnCount);

            CopySubMatrixToUnchecked(target, sourceRowIndex, targetRowIndex, rowCount,
                sourceColumnIndex, targetColumnIndex, columnCount, skipClearing);
        }

        internal virtual void CopySubMatrixToUnchecked(MatrixStorage<T> target,
            int sourceRowIndex, int targetRowIndex, int rowCount,
            int sourceColumnIndex, int targetColumnIndex, int columnCount,
            bool skipClearing = false)
        {
            for (int j = sourceColumnIndex, jj = targetColumnIndex; j < sourceColumnIndex + columnCount; j++, jj++)
            {
                for (int i = sourceRowIndex, ii = targetRowIndex; i < sourceRowIndex + rowCount; i++, ii++)
                {
                    target.At(ii, jj, At(i, j));
                }
            }
        }

        // ROW COPY

        public void CopyRowTo(VectorStorage<T> target, int rowIndex, bool skipClearing = false)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }

            ValidateRowRange(target, rowIndex);
            CopySubRowToUnchecked(target, rowIndex, 0, 0, ColumnCount, skipClearing);
        }

        public void CopySubRowTo(VectorStorage<T> target, int rowIndex,
            int sourceColumnIndex, int targetColumnIndex, int columnCount,
            bool skipClearing = false)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }

            ValidateSubRowRange(target, rowIndex, sourceColumnIndex, targetColumnIndex, columnCount);
            CopySubRowToUnchecked(target, rowIndex, sourceColumnIndex, targetColumnIndex, columnCount, skipClearing);
        }

        internal virtual void CopySubRowToUnchecked(VectorStorage<T> target, int rowIndex,
            int sourceColumnIndex, int targetColumnIndex, int columnCount,
            bool skipClearing = false)
        {
            for (int j = sourceColumnIndex, jj = targetColumnIndex; j < sourceColumnIndex + columnCount; j++, jj++)
            {
                target.At(jj, At(rowIndex, j));
            }
        }

        // COLUMN COPY

        public void CopyColumnTo(VectorStorage<T> target, int columnIndex, bool skipClearing = false)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }

            ValidateColumnRange(target, columnIndex);
            CopySubColumnToUnchecked(target, columnIndex, 0, 0, RowCount, skipClearing);
        }

        public void CopySubColumnTo(VectorStorage<T> target, int columnIndex,
            int sourceRowIndex, int targetRowIndex, int rowCount,
            bool skipClearing = false)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }

            ValidateSubColumnRange(target, columnIndex, sourceRowIndex, targetRowIndex, rowCount);
            CopySubColumnToUnchecked(target, columnIndex, sourceRowIndex, targetRowIndex, rowCount, skipClearing);
        }

        internal virtual void CopySubColumnToUnchecked(VectorStorage<T> target, int columnIndex,
            int sourceRowIndex, int targetRowIndex, int rowCount,
            bool skipClearing = false)
        {
            for (int i = sourceRowIndex, ii = targetRowIndex; i < sourceRowIndex + rowCount; i++, ii++)
            {
                target.At(ii, At(i, columnIndex));
            }
        }

        // EXTRACT

        public virtual T[] ToRowMajorArray()
        {
            var ret = new T[RowCount * ColumnCount];
            for (int i = 0; i < RowCount; i++)
            {
                var offset = i * ColumnCount;
                for (int j = 0; j < ColumnCount; j++)
                {
                    ret[offset + j] = At(i, j);
                }
            }
            return ret;
        }

        public virtual T[] ToColumnMajorArray()
        {
            var ret = new T[RowCount * ColumnCount];
            for (int j = 0; j < ColumnCount; j++)
            {
                var offset = j * RowCount;
                for (int i = 0; i < RowCount; i++)
                {
                    ret[offset + i] = At(i, j);
                }
            }
            return ret;
        }

        public virtual T[,] ToArray()
        {
            var ret = new T[RowCount,ColumnCount];
            for (int i = 0; i < RowCount; i++)
            {
                for (int j = 0; j < ColumnCount; j++)
                {
                    ret[i, j] = At(i, j);
                }
            }
            return ret;
        }

        // FUNCTIONAL COMBINATORS

        public virtual void MapInplace(Func<T, T> f, bool forceMapZeros = false)
        {
            for (int i = 0; i < RowCount; i++)
            {
                for (int j = 0; j < ColumnCount; j++)
                {
                    At(i, j, f(At(i, j)));
                }
            }
        }

        public virtual void MapIndexedInplace(Func<int, int, T, T> f, bool forceMapZeros = false)
        {
            for (int i = 0; i < RowCount; i++)
            {
                for (int j = 0; j < ColumnCount; j++)
                {
                    At(i, j, f(i, j, At(i, j)));
                }
            }
        }
    }
}
