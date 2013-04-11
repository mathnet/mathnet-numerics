// <copyright file="DiagonalMatrixStorage.cs" company="Math.NET">
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
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.Properties;
using MathNet.Numerics.Threading;

namespace MathNet.Numerics.LinearAlgebra.Storage
{
    [Serializable]
    public class DiagonalMatrixStorage<T> : MatrixStorage<T>
        where T : struct, IEquatable<T>, IFormattable
    {
        // [ruegg] public fields are OK here

        public readonly T[] Data;

        internal DiagonalMatrixStorage(int rows, int columns)
            : base(rows, columns)
        {
            Data = new T[Math.Min(rows, columns)];
        }

        internal DiagonalMatrixStorage(int rows, int columns, T[] data)
            : base(rows, columns)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            if (data.Length != Math.Min(rows, columns))
            {
                throw new ArgumentOutOfRangeException("data", string.Format(Resources.ArgumentArrayWrongLength, Math.Min(rows, columns)));
            }

            Data = data;
        }

        /// <summary>
        /// Retrieves the requested element without range checking.
        /// </summary>
        public override T At(int row, int column)
        {
            return row == column ? Data[row] : Zero;
        }

        /// <summary>
        /// Sets the element without range checking.
        /// </summary>
        public override void At(int row, int column, T value)
        {
            if (row == column)
            {
                Data[row] = value;
            }
            else if (!Zero.Equals(value))
            {
                throw new IndexOutOfRangeException("Cannot set an off-diagonal element in a diagonal matrix.");
            }
        }

        public override bool IsFullyMutable
        {
            get { return false; }
        }

        public override bool IsMutable(int row, int column)
        {
            return row == column;
        }

        public override void Clear()
        {
            Array.Clear(Data, 0, Data.Length);
        }

        public override void Clear(int rowIndex, int rowCount, int columnIndex, int columnCount)
        {
            var beginInclusive = Math.Max(rowIndex, columnIndex);
            var endExclusive = Math.Min(rowIndex + rowCount, columnIndex + columnCount);
            if (endExclusive > beginInclusive)
            {
                Array.Clear(Data, beginInclusive, endExclusive - beginInclusive);
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
        public override bool Equals(MatrixStorage<T> other)
        {
            var diagonal = other as DiagonalMatrixStorage<T>;
            if (diagonal == null)
            {
                return base.Equals(other);
            }

            // Reject equality when the argument is null or has a different shape.
            if (ColumnCount != other.ColumnCount || RowCount != other.RowCount)
            {
                return false;
            }

            // Accept if the argument is the same object as this.
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            if (diagonal.Data.Length != Data.Length)
            {
                return false;
            }

            // If all else fails, perform element wise comparison.
            return !Data.Where((t, i) => !t.Equals(diagonal.Data[i])).Any();
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode()
        {
            var hashNum = Math.Min(Data.Length, 25);
            int hash = 17;
            unchecked
            {
                for (var i = 0; i < hashNum; i++)
                {
                    hash = hash * 31 + Data[i].GetHashCode();
                }
            }
            return hash;
        }

        // INITIALIZATION

        public static DiagonalMatrixStorage<T> OfMatrix(MatrixStorage<T> matrix)
        {
            var storage = new DiagonalMatrixStorage<T>(matrix.RowCount, matrix.ColumnCount);
            matrix.CopyToUnchecked(storage, skipClearing: true);
            return storage;
        }

        public static DiagonalMatrixStorage<T> OfArray(T[,] array)
        {
            var storage = new DiagonalMatrixStorage<T>(array.GetLength(0), array.GetLength(1));
            for (var i = 0; i < storage.RowCount; i++)
            {
                for (var j = 0; j < storage.ColumnCount; j++)
                {
                    if (i == j)
                    {
                        storage.Data[i] = array[i, j];
                    }
                    else if (!Zero.Equals(array[i, j]))
                    {
                        throw new IndexOutOfRangeException("Cannot set an off-diagonal element in a diagonal matrix.");
                    }
                }
            }
            return storage;
        }

        public static DiagonalMatrixStorage<T> OfInit(int rows, int columns, Func<int, T> init)
        {
            var storage = new DiagonalMatrixStorage<T>(rows, columns);
            for (var i = 0; i < storage.Data.Length; i++)
            {
                storage.Data[i] = init(i);
            }
            return storage;
        }

        public static DiagonalMatrixStorage<T> OfEnumerable(int rows, int columns, IEnumerable<T> data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            var arrayData = data as T[];
            if (arrayData != null)
            {
                var copy = new T[arrayData.Length];
                Array.Copy(arrayData, copy, arrayData.Length);
                return new DiagonalMatrixStorage<T>(rows, columns, copy);
            }

            return new DiagonalMatrixStorage<T>(rows, columns, data.ToArray());
        }

        public static DiagonalMatrixStorage<T> OfIndexedEnumerable(int rows, int columns, IEnumerable<Tuple<int, T>> data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            var storage = new DiagonalMatrixStorage<T>(rows, columns);
            foreach(var item in data)
            {
                storage.Data[item.Item1] = item.Item2;
            }
            return storage;
        }

        // MATRIX COPY

        internal override void CopyToUnchecked(MatrixStorage<T> target, bool skipClearing = false)
        {
            var diagonalTarget = target as DiagonalMatrixStorage<T>;
            if (diagonalTarget != null)
            {
                CopyToUnchecked(diagonalTarget);
                return;
            }

            var denseTarget = target as DenseColumnMajorMatrixStorage<T>;
            if (denseTarget != null)
            {
                CopyToUnchecked(denseTarget, skipClearing);
                return;
            }

            var sparseTarget = target as SparseCompressedRowMatrixStorage<T>;
            if (sparseTarget != null)
            {
                CopyToUnchecked(sparseTarget, skipClearing);
                return;
            }

            // FALL BACK

            if (!skipClearing)
            {
                target.Clear();
            }

            for (int i = 0; i < Data.Length; i++)
            {
                target.At(i, i, Data[i]);
            }
        }

        void CopyToUnchecked(DiagonalMatrixStorage<T> target)
        {
            //Buffer.BlockCopy(Data, 0, target.Data, 0, Data.Length * System.Runtime.InteropServices.Marshal.SizeOf(typeof(T)));
            Array.Copy(Data, 0, target.Data, 0, Data.Length);
        }

        void CopyToUnchecked(SparseCompressedRowMatrixStorage<T> target, bool skipClearing)
        {
            if (!skipClearing)
            {
                target.Clear();
            }

            for (int i = 0; i < Data.Length; i++)
            {
                target.At(i, i, Data[i]);
            }
        }

        void CopyToUnchecked(DenseColumnMajorMatrixStorage<T> target, bool skipClearing)
        {
            if (!skipClearing)
            {
                target.Clear();
            }

            for (int i = 0; i < Data.Length; i++)
            {
                target.Data[i*(target.RowCount + 1)] = Data[i];
            }
        }

        internal override void CopySubMatrixToUnchecked(MatrixStorage<T> target,
            int sourceRowIndex, int targetRowIndex, int rowCount,
            int sourceColumnIndex, int targetColumnIndex, int columnCount,
            bool skipClearing = false)
        {
            var denseTarget = target as DenseColumnMajorMatrixStorage<T>;
            if (denseTarget != null)
            {
                CopySubMatrixToUnchecked(denseTarget, sourceRowIndex, targetRowIndex, rowCount, sourceColumnIndex, targetColumnIndex, columnCount, skipClearing);
                return;
            }

            var diagonalTarget = target as DiagonalMatrixStorage<T>;
            if (diagonalTarget != null)
            {
                CopySubMatrixToUnchecked(diagonalTarget, sourceRowIndex, targetRowIndex, rowCount, sourceColumnIndex, targetColumnIndex, columnCount);
                return;
            }

            var sparseTarget = target as SparseCompressedRowMatrixStorage<T>;
            if (sparseTarget != null)
            {
                CopySubMatrixToUnchecked(sparseTarget, sourceRowIndex, targetRowIndex, rowCount, sourceColumnIndex, targetColumnIndex, columnCount, skipClearing);
                return;
            }

            // FALL BACK

            base.CopySubMatrixToUnchecked(target, sourceRowIndex, targetRowIndex, rowCount, sourceColumnIndex, targetColumnIndex, columnCount, skipClearing);
        }

        void CopySubMatrixToUnchecked(DiagonalMatrixStorage<T> target,
            int sourceRowIndex, int targetRowIndex, int rowCount,
            int sourceColumnIndex, int targetColumnIndex, int columnCount)
        {
            if (sourceRowIndex - sourceColumnIndex != targetRowIndex - targetColumnIndex)
            {
                if (Data.Any(x => !Zero.Equals(x)))
                {
                    throw new NotSupportedException();
                }

                target.Clear(targetRowIndex, rowCount, targetColumnIndex, columnCount);
                return;
            }

            var beginInclusive = Math.Max(sourceRowIndex, sourceColumnIndex);
            var endExclusive = Math.Min(sourceRowIndex + rowCount, sourceColumnIndex + columnCount);
            if (endExclusive > beginInclusive)
            {
                var beginTarget = Math.Max(targetRowIndex, targetColumnIndex);
                Array.Copy(Data, beginInclusive, target.Data, beginTarget, endExclusive - beginInclusive);
            }
        }

        void CopySubMatrixToUnchecked(DenseColumnMajorMatrixStorage<T> target,
            int sourceRowIndex, int targetRowIndex, int rowCount,
            int sourceColumnIndex, int targetColumnIndex, int columnCount,
            bool skipClearing)
        {
            if (!skipClearing)
            {
                target.Clear(targetRowIndex, rowCount, targetColumnIndex, columnCount);
            }

            if (sourceRowIndex > sourceColumnIndex && sourceColumnIndex + columnCount > sourceRowIndex)
            {
                // column by column, but skip resulting zero columns at the beginning

                int columnInit = sourceRowIndex - sourceColumnIndex;
                int offset = (columnInit + targetColumnIndex) * target.RowCount + targetRowIndex;
                int step = target.RowCount + 1;
                int end = Math.Min(columnCount - columnInit, rowCount) + sourceRowIndex;

                for (int i = sourceRowIndex, j = offset; i < end; i++, j += step)
                {
                    target.Data[j] = Data[i];
                }
            }
            else if (sourceRowIndex < sourceColumnIndex && sourceRowIndex + rowCount > sourceColumnIndex)
            {
                // row by row, but skip resulting zero rows at the beginning

                int rowInit = sourceColumnIndex - sourceRowIndex;
                int offset = targetColumnIndex*target.RowCount + rowInit + targetRowIndex;
                int step = target.RowCount + 1;
                int end = Math.Min(columnCount, rowCount - rowInit) + sourceColumnIndex;

                for (int i = sourceColumnIndex, j = offset; i < end; i++, j += step)
                {
                    target.Data[j] = Data[i];
                }
            }
            else
            {
                int offset = targetColumnIndex*target.RowCount + targetRowIndex;
                int step = target.RowCount + 1;
                var end = Math.Min(columnCount, rowCount) + sourceRowIndex;

                for (int i = sourceRowIndex, j = offset; i < end; i++, j += step)
                {
                    target.Data[j] = Data[i];
                }
            }
        }

        void CopySubMatrixToUnchecked(SparseCompressedRowMatrixStorage<T> target,
            int sourceRowIndex, int targetRowIndex, int rowCount,
            int sourceColumnIndex, int targetColumnIndex, int columnCount,
            bool skipClearing)
        {
            if (!skipClearing)
            {
                target.Clear(targetRowIndex, rowCount, targetColumnIndex, columnCount);
            }

            if (sourceRowIndex == sourceColumnIndex)
            {
                for (var i = 0; i < Math.Min(columnCount, rowCount); i++)
                {
                    target.At(i + targetRowIndex, i + targetColumnIndex, Data[sourceRowIndex + i]);
                }
            }
            else if (sourceRowIndex > sourceColumnIndex && sourceColumnIndex + columnCount > sourceRowIndex)
            {
                // column by column, but skip resulting zero columns at the beginning
                int columnInit = sourceRowIndex - sourceColumnIndex;
                for (var i = 0; i < Math.Min(columnCount - columnInit, rowCount); i++)
                {
                    target.At(i + targetRowIndex, columnInit + i + targetColumnIndex, Data[sourceRowIndex + i]);
                }
            }
            else if (sourceRowIndex < sourceColumnIndex && sourceRowIndex + rowCount > sourceColumnIndex)
            {
                // row by row, but skip resulting zero rows at the beginning
                int rowInit = sourceColumnIndex - sourceRowIndex;
                for (var i = 0; i < Math.Min(columnCount, rowCount - rowInit); i++)
                {
                    target.At(rowInit + i + targetRowIndex, i + targetColumnIndex, Data[sourceColumnIndex + i]);
                }
            }

            // else: all zero, nop
        }

        // ROW COPY

        internal override void CopySubRowToUnchecked(VectorStorage<T> target, int rowIndex,
            int sourceColumnIndex, int targetColumnIndex, int columnCount,
            bool skipClearing = false)
        {
            if (!skipClearing)
            {
                target.Clear(targetColumnIndex, columnCount);
            }

            if (rowIndex >= sourceColumnIndex && rowIndex < sourceColumnIndex + columnCount && rowIndex < Data.Length)
            {
                target.At(rowIndex - sourceColumnIndex + targetColumnIndex, Data[rowIndex]);
            }
        }

        // COLUMN COPY

        internal override void CopySubColumnToUnchecked(VectorStorage<T> target, int columnIndex,
            int sourceRowIndex, int targetRowIndex, int rowCount,
            bool skipClearing = false)
        {
            if (!skipClearing)
            {
                target.Clear(targetRowIndex, rowCount);
            }

            if (columnIndex >= sourceRowIndex && columnIndex < sourceRowIndex + rowCount && columnIndex < Data.Length)
            {
                target.At(columnIndex - sourceRowIndex + targetRowIndex, Data[columnIndex]);
            }
        }

        // EXTRACT

        public override T[] ToRowMajorArray()
        {
            var ret = new T[RowCount * ColumnCount];
            var stride = ColumnCount + 1;
            for (int i = 0; i < Data.Length; i++)
            {
                ret[i * stride] = Data[i];
            }
            return ret;
        }

        public override T[] ToColumnMajorArray()
        {
            var ret = new T[RowCount * ColumnCount];
            var stride = RowCount + 1;
            for (int i = 0; i < Data.Length; i++)
            {
                ret[i * stride] = Data[i];
            }
            return ret;
        }

        public override T[,] ToArray()
        {
            var ret = new T[RowCount, ColumnCount];
            for (int i = 0; i < Data.Length; i++)
            {
                ret[i, i] = Data[i];
            }
            return ret;
        }

        // FUNCTIONAL COMBINATORS

        public override void MapInplace(Func<T, T> f, bool forceMapZeros = false)
        {
            // we deliberately ignore forceMapZeros since we would not actually
            // support any non-zero results outside of the diagonal anyway
            CommonParallel.For(0, Data.Length, 4096, (a, b) =>
                {
                    for (int i = a; i < b; i++)
                    {
                        Data[i] = f(Data[i]);
                    }
                });
        }

        public override void MapIndexedInplace(Func<int, int, T, T> f, bool forceMapZeros = false)
        {
            // we deliberately ignore forceMapZeros since we would not actually
            // support any non-zero results outside of the diagonal anyway
            CommonParallel.For(0, Data.Length, 4096, (a, b) =>
                {
                    for (int i = a; i < b; i++)
                    {
                        Data[i] = f(i, i, Data[i]);
                    }
                });
        }
    }
}
