// <copyright file="MatrixStorage.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
//
// Copyright (c) 2009-2014 Math.NET
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
using MathNet.Numerics.Properties;

namespace MathNet.Numerics.LinearAlgebra.Storage
{
    [Serializable]
    public abstract partial class MatrixStorage<T> : IEquatable<MatrixStorage<T>>
        where T : struct, IEquatable<T>, IFormattable
    {
        // [ruegg] public fields are OK here

        protected static readonly T Zero = BuilderInstance<T>.Matrix.Zero;
        public readonly int RowCount;
        public readonly int ColumnCount;

        protected MatrixStorage(int rowCount, int columnCount)
        {
            if (rowCount <= 0)
            {
                throw new ArgumentOutOfRangeException("rowCount", Resources.MatrixRowsMustBePositive);
            }

            if (columnCount <= 0)
            {
                throw new ArgumentOutOfRangeException("columnCount", Resources.MatrixColumnsMustBePositive);
            }

            RowCount = rowCount;
            ColumnCount = columnCount;
        }

        /// <summary>
        /// True if the matrix storage format is dense.
        /// </summary>
        public abstract bool IsDense { get; }

        /// <summary>
        /// True if all fields of this matrix can be set to any value.
        /// False if some fields are fixed, like on a diagonal matrix.
        /// </summary>
        public abstract bool IsFullyMutable { get; }

        /// <summary>
        /// True if the specified field can be set to any value.
        /// False if the field is fixed, like an off-diagonal field on a diagonal matrix.
        /// </summary>
        public abstract bool IsMutableAt(int row, int column);

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

        public virtual void ClearRows(int[] rowIndices)
        {
            for (var k = 0; k < rowIndices.Length; k++)
            {
                int row = rowIndices[k];
                for (var j = 0; j < ColumnCount; j++)
                {
                    At(row, j, Zero);
                }
            }
        }

        public virtual void ClearColumns(int[] columnIndices)
        {
            for (var k = 0; k < ColumnCount; k++)
            {
                int column = columnIndices[k];
                for (var i = 0; i < RowCount; i++)
                {
                    At(i, column, Zero);
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

        public void CopyTo(MatrixStorage<T> target, ExistingData existingData = ExistingData.Clear)
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

            CopyToUnchecked(target, existingData);
        }

        internal virtual void CopyToUnchecked(MatrixStorage<T> target, ExistingData existingData = ExistingData.Clear)
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
            ExistingData existingData = ExistingData.Clear)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }

            if (rowCount == 0 || columnCount == 0)
            {
                return;
            }

            if (ReferenceEquals(this, target))
            {
                throw new NotSupportedException();
            }

            ValidateSubMatrixRange(target,
                sourceRowIndex, targetRowIndex, rowCount,
                sourceColumnIndex, targetColumnIndex, columnCount);

            CopySubMatrixToUnchecked(target, sourceRowIndex, targetRowIndex, rowCount,
                sourceColumnIndex, targetColumnIndex, columnCount, existingData);
        }

        internal virtual void CopySubMatrixToUnchecked(MatrixStorage<T> target,
            int sourceRowIndex, int targetRowIndex, int rowCount,
            int sourceColumnIndex, int targetColumnIndex, int columnCount,
            ExistingData existingData = ExistingData.Clear)
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

        public void CopyRowTo(VectorStorage<T> target, int rowIndex, ExistingData existingData = ExistingData.Clear)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }

            ValidateRowRange(target, rowIndex);
            CopySubRowToUnchecked(target, rowIndex, 0, 0, ColumnCount, existingData);
        }

        public void CopySubRowTo(VectorStorage<T> target, int rowIndex,
            int sourceColumnIndex, int targetColumnIndex, int columnCount,
            ExistingData existingData = ExistingData.Clear)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }

            if (columnCount == 0)
            {
                return;
            }

            ValidateSubRowRange(target, rowIndex, sourceColumnIndex, targetColumnIndex, columnCount);
            CopySubRowToUnchecked(target, rowIndex, sourceColumnIndex, targetColumnIndex, columnCount, existingData);
        }

        internal virtual void CopySubRowToUnchecked(VectorStorage<T> target, int rowIndex,
            int sourceColumnIndex, int targetColumnIndex, int columnCount,
            ExistingData existingData = ExistingData.Clear)
        {
            for (int j = sourceColumnIndex, jj = targetColumnIndex; j < sourceColumnIndex + columnCount; j++, jj++)
            {
                target.At(jj, At(rowIndex, j));
            }
        }

        // COLUMN COPY

        public void CopyColumnTo(VectorStorage<T> target, int columnIndex, ExistingData existingData = ExistingData.Clear)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }

            ValidateColumnRange(target, columnIndex);
            CopySubColumnToUnchecked(target, columnIndex, 0, 0, RowCount, existingData);
        }

        public void CopySubColumnTo(VectorStorage<T> target, int columnIndex,
            int sourceRowIndex, int targetRowIndex, int rowCount,
            ExistingData existingData = ExistingData.Clear)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }

            if (rowCount == 0)
            {
                return;
            }

            ValidateSubColumnRange(target, columnIndex, sourceRowIndex, targetRowIndex, rowCount);
            CopySubColumnToUnchecked(target, columnIndex, sourceRowIndex, targetRowIndex, rowCount, existingData);
        }

        internal virtual void CopySubColumnToUnchecked(VectorStorage<T> target, int columnIndex,
            int sourceRowIndex, int targetRowIndex, int rowCount,
            ExistingData existingData = ExistingData.Clear)
        {
            for (int i = sourceRowIndex, ii = targetRowIndex; i < sourceRowIndex + rowCount; i++, ii++)
            {
                target.At(ii, At(i, columnIndex));
            }
        }

        // TRANSPOSE

        public void TransposeTo(MatrixStorage<T> target, ExistingData existingData = ExistingData.Clear)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }

            if (ReferenceEquals(this, target))
            {
                throw new NotSupportedException("In-place transpose is not supported.");
            }

            if (RowCount != target.ColumnCount || ColumnCount != target.RowCount)
            {
                var message = string.Format(Resources.ArgumentMatrixDimensions2, RowCount + "x" + ColumnCount, target.RowCount + "x" + target.ColumnCount);
                throw new ArgumentException(message, "target");
            }

            TransposeToUnchecked(target, existingData);
        }

        internal virtual void TransposeToUnchecked(MatrixStorage<T> target, ExistingData existingData = ExistingData.Clear)
        {
            for (int j = 0; j < ColumnCount; j++)
            {
                for (int i = 0; i < RowCount; i++)
                {
                    target.At(j, i, At(i, j));
                }
            }
        }

        // EXTRACT

        public virtual T[] ToRowMajorArray()
        {
            var ret = new T[RowCount*ColumnCount];
            for (int i = 0; i < RowCount; i++)
            {
                var offset = i*ColumnCount;
                for (int j = 0; j < ColumnCount; j++)
                {
                    ret[offset + j] = At(i, j);
                }
            }
            return ret;
        }

        public virtual T[] ToColumnMajorArray()
        {
            var ret = new T[RowCount*ColumnCount];
            for (int j = 0; j < ColumnCount; j++)
            {
                var offset = j*RowCount;
                for (int i = 0; i < RowCount; i++)
                {
                    ret[offset + i] = At(i, j);
                }
            }
            return ret;
        }

        public virtual T[,] ToArray()
        {
            var ret = new T[RowCount, ColumnCount];
            for (int i = 0; i < RowCount; i++)
            {
                for (int j = 0; j < ColumnCount; j++)
                {
                    ret[i, j] = At(i, j);
                }
            }
            return ret;
        }

        // ENUMERATION

        public virtual IEnumerable<T> Enumerate()
        {
            for (int i = 0; i < RowCount; i++)
            {
                for (int j = 0; j < ColumnCount; j++)
                {
                    yield return At(i, j);
                }
            }
        }

        public virtual IEnumerable<Tuple<int, int, T>> EnumerateIndexed()
        {
            for (int i = 0; i < RowCount; i++)
            {
                for (int j = 0; j < ColumnCount; j++)
                {
                    yield return new Tuple<int, int, T>(i, j, At(i, j));
                }
            }
        }

        public virtual IEnumerable<T> EnumerateNonZero()
        {
            for (int i = 0; i < RowCount; i++)
            {
                for (int j = 0; j < ColumnCount; j++)
                {
                    var x = At(i, j);
                    if (!Zero.Equals(x))
                    {
                        yield return x;
                    }
                }
            }
        }

        public virtual IEnumerable<Tuple<int, int, T>> EnumerateNonZeroIndexed()
        {
            for (int i = 0; i < RowCount; i++)
            {
                for (int j = 0; j < ColumnCount; j++)
                {
                    var x = At(i, j);
                    if (!Zero.Equals(x))
                    {
                        yield return new Tuple<int, int, T>(i, j, x);
                    }
                }
            }
        }

        // FUNCTIONAL COMBINATORS: MAP

        public virtual void MapInplace(Func<T, T> f, Zeros zeros = Zeros.AllowSkip)
        {
            for (int i = 0; i < RowCount; i++)
            {
                for (int j = 0; j < ColumnCount; j++)
                {
                    At(i, j, f(At(i, j)));
                }
            }
        }

        public virtual void MapIndexedInplace(Func<int, int, T, T> f, Zeros zeros = Zeros.AllowSkip)
        {
            for (int i = 0; i < RowCount; i++)
            {
                for (int j = 0; j < ColumnCount; j++)
                {
                    At(i, j, f(i, j, At(i, j)));
                }
            }
        }

        public void MapTo<TU>(MatrixStorage<TU> target, Func<T, TU> f,
            Zeros zeros = Zeros.AllowSkip, ExistingData existingData = ExistingData.Clear)
            where TU : struct, IEquatable<TU>, IFormattable
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }

            if (RowCount != target.RowCount || ColumnCount != target.ColumnCount)
            {
                var message = string.Format(Resources.ArgumentMatrixDimensions2, RowCount + "x" + ColumnCount, target.RowCount + "x" + target.ColumnCount);
                throw new ArgumentException(message, "target");
            }

            MapToUnchecked(target, f, zeros, existingData);
        }

        internal virtual void MapToUnchecked<TU>(MatrixStorage<TU> target, Func<T, TU> f,
            Zeros zeros = Zeros.AllowSkip, ExistingData existingData = ExistingData.Clear)
            where TU : struct, IEquatable<TU>, IFormattable
        {
            for (int i = 0; i < RowCount; i++)
            {
                for (int j = 0; j < ColumnCount; j++)
                {
                    target.At(i, j, f(At(i, j)));
                }
            }
        }

        public void MapIndexedTo<TU>(MatrixStorage<TU> target, Func<int, int, T, TU> f,
            Zeros zeros = Zeros.AllowSkip, ExistingData existingData = ExistingData.Clear)
            where TU : struct, IEquatable<TU>, IFormattable
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }

            if (RowCount != target.RowCount || ColumnCount != target.ColumnCount)
            {
                var message = string.Format(Resources.ArgumentMatrixDimensions2, RowCount + "x" + ColumnCount, target.RowCount + "x" + target.ColumnCount);
                throw new ArgumentException(message, "target");
            }

            MapIndexedToUnchecked(target, f, zeros, existingData);
        }

        internal virtual void MapIndexedToUnchecked<TU>(MatrixStorage<TU> target, Func<int, int, T, TU> f,
            Zeros zeros = Zeros.AllowSkip, ExistingData existingData = ExistingData.Clear)
            where TU : struct, IEquatable<TU>, IFormattable
        {
            for (int j = 0; j < ColumnCount; j++)
            {
                for (int i = 0; i < RowCount; i++)
                {
                    target.At(i, j, f(i, j, At(i, j)));
                }
            }
        }

        public void MapSubMatrixIndexedTo<TU>(MatrixStorage<TU> target, Func<int, int, T, TU> f,
            int sourceRowIndex, int targetRowIndex, int rowCount,
            int sourceColumnIndex, int targetColumnIndex, int columnCount,
            Zeros zeros = Zeros.AllowSkip, ExistingData existingData = ExistingData.Clear)
            where TU : struct, IEquatable<TU>, IFormattable
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }

            if (rowCount == 0 || columnCount == 0)
            {
                return;
            }

            if (ReferenceEquals(this, target))
            {
                throw new NotSupportedException();
            }

            ValidateSubMatrixRange(target,
                sourceRowIndex, targetRowIndex, rowCount,
                sourceColumnIndex, targetColumnIndex, columnCount);

            MapSubMatrixIndexedToUnchecked(target, f, sourceRowIndex, targetRowIndex, rowCount, sourceColumnIndex, targetColumnIndex, columnCount, zeros, existingData);
        }

        internal virtual void MapSubMatrixIndexedToUnchecked<TU>(MatrixStorage<TU> target, Func<int, int, T, TU> f,
            int sourceRowIndex, int targetRowIndex, int rowCount,
            int sourceColumnIndex, int targetColumnIndex, int columnCount,
            Zeros zeros = Zeros.AllowSkip, ExistingData existingData = ExistingData.Clear)
            where TU : struct, IEquatable<TU>, IFormattable
        {
            for (int j = sourceColumnIndex, jj = targetColumnIndex; j < sourceColumnIndex + columnCount; j++, jj++)
            {
                for (int i = sourceRowIndex, ii = targetRowIndex; i < sourceRowIndex + rowCount; i++, ii++)
                {
                    target.At(ii, jj, f(ii, jj, At(i, j)));
                }
            }
        }

        // FUNCTIONAL COMBINATORS: FOLD

        /// <remarks>The state array will not be modified, unless it is the same instance as the target array (which is allowed).</remarks>
        public void FoldByRow<TU>(TU[] target, Func<TU, T, TU> f, Func<TU, int, TU> finalize, TU[] state, Zeros zeros = Zeros.AllowSkip)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }
            if (target.Length != RowCount)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "target");
            }

            if (state == null)
            {
                throw new ArgumentNullException("state");
            }
            if (state.Length != RowCount)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "state");
            }

            FoldByRowUnchecked(target, f, finalize, state, zeros);
        }

        /// <remarks>The state array will not be modified, unless it is the same instance as the target array (which is allowed).</remarks>
        internal virtual void FoldByRowUnchecked<TU>(TU[] target, Func<TU, T, TU> f, Func<TU, int, TU> finalize, TU[] state, Zeros zeros = Zeros.AllowSkip)
        {
            for (int i = 0; i < RowCount; i++)
            {
                TU s = state[i];
                for (int j = 0; j < ColumnCount; j++)
                {
                    s = f(s, At(i, j));
                }
                target[i] = finalize(s, ColumnCount);
            }
        }

        /// <remarks>The state array will not be modified, unless it is the same instance as the target array (which is allowed).</remarks>
        public void FoldByColumn<TU>(TU[] target, Func<TU, T, TU> f, Func<TU, int, TU> finalize, TU[] state, Zeros zeros = Zeros.AllowSkip)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }
            if (target.Length != ColumnCount)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "target");
            }

            if (state == null)
            {
                throw new ArgumentNullException("state");
            }
            if (state.Length != ColumnCount)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "state");
            }

            FoldByColumnUnchecked(target, f, finalize, state, zeros);
        }

        /// <remarks>The state array will not be modified, unless it is the same instance as the target array (which is allowed).</remarks>
        internal virtual void FoldByColumnUnchecked<TU>(TU[] target, Func<TU, T, TU> f, Func<TU, int, TU> finalize, TU[] state, Zeros zeros = Zeros.AllowSkip)
        {
            for (int j = 0; j < ColumnCount; j++)
            {
                TU s = state[j];
                for (int i = 0; i < RowCount; i++)
                {
                    s = f(s, At(i, j));
                }
                target[j] = finalize(s, RowCount);
            }
        }
    }
}
