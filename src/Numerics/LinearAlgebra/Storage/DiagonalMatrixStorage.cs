// <copyright file="DiagonalMatrixStorage.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2015 Math.NET
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
using System.Runtime.Serialization;
using MathNet.Numerics.Threading;

namespace MathNet.Numerics.LinearAlgebra.Storage
{
    [Serializable]
    [DataContract(Namespace = "urn:MathNet/Numerics/LinearAlgebra")]
    public class DiagonalMatrixStorage<T> : MatrixStorage<T>
        where T : struct, IEquatable<T>, IFormattable
    {
        // [ruegg] public fields are OK here

        [DataMember(Order = 1)]
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
                throw new ArgumentNullException(nameof(data));
            }

            if (data.Length != Math.Min(rows, columns))
            {
                throw new ArgumentOutOfRangeException(nameof(data), $"The given array has the wrong length. Should be {Math.Min(rows, columns)}.");
            }

            Data = data;
        }

        /// <summary>
        /// True if the matrix storage format is dense.
        /// </summary>
        public override bool IsDense => false;

        /// <summary>
        /// True if all fields of this matrix can be set to any value.
        /// False if some fields are fixed, like on a diagonal matrix.
        /// </summary>
        public override bool IsFullyMutable => false;

        /// <summary>
        /// True if the specified field can be set to any value.
        /// False if the field is fixed, like an off-diagonal field on a diagonal matrix.
        /// </summary>
        public override bool IsMutableAt(int row, int column)
        {
            return row == column;
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
                    hash = hash*31 + Data[i].GetHashCode();
                }
            }
            return hash;
        }

        // CLEARING

        public override void Clear()
        {
            Array.Clear(Data, 0, Data.Length);
        }

        internal override void ClearUnchecked(int rowIndex, int rowCount, int columnIndex, int columnCount)
        {
            var beginInclusive = Math.Max(rowIndex, columnIndex);
            var endExclusive = Math.Min(rowIndex + rowCount, columnIndex + columnCount);
            if (endExclusive > beginInclusive)
            {
                Array.Clear(Data, beginInclusive, endExclusive - beginInclusive);
            }
        }

        internal override void ClearRowsUnchecked(int[] rowIndices)
        {
            for (int i = 0; i < rowIndices.Length; i++)
            {
                Data[rowIndices[i]] = Zero;
            }
        }

        internal override void ClearColumnsUnchecked(int[] columnIndices)
        {
            for (int i = 0; i < columnIndices.Length; i++)
            {
                Data[columnIndices[i]] = Zero;
            }
        }

        // INITIALIZATION

        public static DiagonalMatrixStorage<T> OfMatrix(MatrixStorage<T> matrix)
        {
            var storage = new DiagonalMatrixStorage<T>(matrix.RowCount, matrix.ColumnCount);
            matrix.CopyToUnchecked(storage, ExistingData.AssumeZeros);
            return storage;
        }

        public static DiagonalMatrixStorage<T> OfArray(T[,] array)
        {
            var storage = new DiagonalMatrixStorage<T>(array.GetLength(0), array.GetLength(1));
            var storageData = storage.Data;

            for (var i = 0; i < storage.RowCount; i++)
            {
                for (var j = 0; j < storage.ColumnCount; j++)
                {
                    if (i == j)
                    {
                        storageData[i] = array[i, j];
                    }
                    else if (!Zero.Equals(array[i, j]))
                    {
                        throw new ArgumentException("Cannot set an off-diagonal element in a diagonal matrix.");
                    }
                }
            }
            return storage;
        }

        public static DiagonalMatrixStorage<T> OfValue(int rows, int columns, T diagonalValue)
        {
            var storage = new DiagonalMatrixStorage<T>(rows, columns);
            var storageData = storage.Data;

            for (var i = 0; i < storage.Data.Length; i++)
            {
                storageData[i] = diagonalValue;
            }
            return storage;
        }

        public static DiagonalMatrixStorage<T> OfInit(int rows, int columns, Func<int, T> init)
        {
            var storage = new DiagonalMatrixStorage<T>(rows, columns);
            var storageData = storage.Data;

            for (var i = 0; i < storage.Data.Length; i++)
            {
                storageData[i] = init(i);
            }
            return storage;
        }

        public static DiagonalMatrixStorage<T> OfEnumerable(int rows, int columns, IEnumerable<T> data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (data is T[] arrayData)
            {
                var copy = new T[arrayData.Length];
                Array.Copy(arrayData, 0, copy, 0, arrayData.Length);
                return new DiagonalMatrixStorage<T>(rows, columns, copy);
            }

            return new DiagonalMatrixStorage<T>(rows, columns, data.ToArray());
        }

        public static DiagonalMatrixStorage<T> OfIndexedEnumerable(int rows, int columns, IEnumerable<Tuple<int, T>> data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            var storage = new DiagonalMatrixStorage<T>(rows, columns);
            var storageData = storage.Data;

            foreach (var (i,x) in data)
            {
                storageData[i] = x;
            }
            return storage;
        }

        public static DiagonalMatrixStorage<T> OfIndexedEnumerable(int rows, int columns, IEnumerable<(int, T)> data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            var storage = new DiagonalMatrixStorage<T>(rows, columns);
            var storageData = storage.Data;

            foreach (var (i,x) in data)
            {
                storageData[i] = x;
            }
            return storage;
        }

        // MATRIX COPY

        internal override void CopyToUnchecked(MatrixStorage<T> target, ExistingData existingData)
        {
            if (target is DiagonalMatrixStorage<T> diagonalTarget)
            {
                CopyToUnchecked(diagonalTarget);
                return;
            }

            if (target is DenseColumnMajorMatrixStorage<T> denseTarget)
            {
                CopyToUnchecked(denseTarget, existingData);
                return;
            }

            if (target is SparseCompressedRowMatrixStorage<T> sparseTarget)
            {
                CopyToUnchecked(sparseTarget, existingData);
                return;
            }

            // FALL BACK

            if (existingData == ExistingData.Clear)
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

        void CopyToUnchecked(SparseCompressedRowMatrixStorage<T> target, ExistingData existingData)
        {
            if (existingData == ExistingData.Clear)
            {
                target.Clear();
            }

            for (int i = 0; i < Data.Length; i++)
            {
                target.At(i, i, Data[i]);
            }
        }

        void CopyToUnchecked(DenseColumnMajorMatrixStorage<T> target, ExistingData existingData)
        {
            if (existingData == ExistingData.Clear)
            {
                target.Clear();
            }

            var targetData = target.Data;
            for (int i = 0; i < Data.Length; i++)
            {
                targetData[i*(target.RowCount + 1)] = Data[i];
            }
        }

        internal override void CopySubMatrixToUnchecked(MatrixStorage<T> target,
            int sourceRowIndex, int targetRowIndex, int rowCount,
            int sourceColumnIndex, int targetColumnIndex, int columnCount,
            ExistingData existingData)
        {
            if (target is DenseColumnMajorMatrixStorage<T> denseTarget)
            {
                CopySubMatrixToUnchecked(denseTarget, sourceRowIndex, targetRowIndex, rowCount, sourceColumnIndex, targetColumnIndex, columnCount, existingData);
                return;
            }

            if (target is DiagonalMatrixStorage<T> diagonalTarget)
            {
                CopySubMatrixToUnchecked(diagonalTarget, sourceRowIndex, targetRowIndex, rowCount, sourceColumnIndex, targetColumnIndex, columnCount);
                return;
            }

            // TODO: Proper Sparse Implementation

            // FALL BACK

            if (existingData == ExistingData.Clear)
            {
                target.ClearUnchecked(targetRowIndex, rowCount, targetColumnIndex, columnCount);
            }

            if (sourceRowIndex == sourceColumnIndex)
            {
                for (var i = 0; i < Math.Min(columnCount, rowCount); i++)
                {
                    target.At(targetRowIndex + i, targetColumnIndex + i, Data[sourceRowIndex + i]);
                }
            }
            else if (sourceRowIndex > sourceColumnIndex && sourceColumnIndex + columnCount > sourceRowIndex)
            {
                // column by column, but skip resulting zero columns at the beginning
                int columnInit = sourceRowIndex - sourceColumnIndex;
                for (var i = 0; i < Math.Min(columnCount - columnInit, rowCount); i++)
                {
                    target.At(targetRowIndex + i, columnInit + targetColumnIndex + i, Data[sourceRowIndex + i]);
                }
            }
            else if (sourceRowIndex < sourceColumnIndex && sourceRowIndex + rowCount > sourceColumnIndex)
            {
                // row by row, but skip resulting zero rows at the beginning
                int rowInit = sourceColumnIndex - sourceRowIndex;
                for (var i = 0; i < Math.Min(columnCount, rowCount - rowInit); i++)
                {
                    target.At(rowInit + targetRowIndex + i, targetColumnIndex + i, Data[sourceColumnIndex + i]);
                }
            }
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

                target.ClearUnchecked(targetRowIndex, rowCount, targetColumnIndex, columnCount);
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
            ExistingData existingData)
        {
            if (existingData == ExistingData.Clear)
            {
                target.ClearUnchecked(targetRowIndex, rowCount, targetColumnIndex, columnCount);
            }

            if (sourceRowIndex > sourceColumnIndex && sourceColumnIndex + columnCount > sourceRowIndex)
            {
                // column by column, but skip resulting zero columns at the beginning

                int columnInit = sourceRowIndex - sourceColumnIndex;
                int offset = (columnInit + targetColumnIndex)*target.RowCount + targetRowIndex;
                int step = target.RowCount + 1;
                int end = Math.Min(columnCount - columnInit, rowCount) + sourceRowIndex;

                var targetData = target.Data;
                for (int i = sourceRowIndex, j = offset; i < end; i++, j += step)
                {
                    targetData[j] = Data[i];
                }
            }
            else if (sourceRowIndex < sourceColumnIndex && sourceRowIndex + rowCount > sourceColumnIndex)
            {
                // row by row, but skip resulting zero rows at the beginning

                int rowInit = sourceColumnIndex - sourceRowIndex;
                int offset = targetColumnIndex*target.RowCount + rowInit + targetRowIndex;
                int step = target.RowCount + 1;
                int end = Math.Min(columnCount, rowCount - rowInit) + sourceColumnIndex;

                var targetData = target.Data;
                for (int i = sourceColumnIndex, j = offset; i < end; i++, j += step)
                {
                    targetData[j] = Data[i];
                }
            }
            else
            {
                int offset = targetColumnIndex*target.RowCount + targetRowIndex;
                int step = target.RowCount + 1;
                var end = Math.Min(columnCount, rowCount) + sourceRowIndex;

                var targetData = target.Data;
                for (int i = sourceRowIndex, j = offset; i < end; i++, j += step)
                {
                    targetData[j] = Data[i];
                }
            }
        }

        // ROW COPY

        internal override void CopySubRowToUnchecked(VectorStorage<T> target, int rowIndex,
            int sourceColumnIndex, int targetColumnIndex, int columnCount, ExistingData existingData)
        {
            if (existingData == ExistingData.Clear)
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
            int sourceRowIndex, int targetRowIndex, int rowCount, ExistingData existingData)
        {
            if (existingData == ExistingData.Clear)
            {
                target.Clear(targetRowIndex, rowCount);
            }

            if (columnIndex >= sourceRowIndex && columnIndex < sourceRowIndex + rowCount && columnIndex < Data.Length)
            {
                target.At(columnIndex - sourceRowIndex + targetRowIndex, Data[columnIndex]);
            }
        }

        // TRANSPOSE

        internal override void TransposeToUnchecked(MatrixStorage<T> target, ExistingData existingData)
        {
            CopyToUnchecked(target, existingData);
        }

        internal override void TransposeSquareInplaceUnchecked()
        {
            // nothing to do
        }

        // EXTRACT

        public override T[] ToRowMajorArray()
        {
            var ret = new T[RowCount*ColumnCount];
            var stride = ColumnCount + 1;
            for (int i = 0; i < Data.Length; i++)
            {
                ret[i*stride] = Data[i];
            }
            return ret;
        }

        public override T[] ToColumnMajorArray()
        {
            var ret = new T[RowCount*ColumnCount];
            var stride = RowCount + 1;
            for (int i = 0; i < Data.Length; i++)
            {
                ret[i*stride] = Data[i];
            }
            return ret;
        }

        public override T[][] ToRowArrays()
        {
            var ret = new T[RowCount][];
            for (int i = 0; i < RowCount; i++)
            {
                ret[i] = new T[ColumnCount];
            }
            for (int i = 0; i < Data.Length; i++)
            {
                ret[i][i] = Data[i];
            }
            return ret;
        }

        public override T[][] ToColumnArrays()
        {
            var ret = new T[ColumnCount][];
            for (int j = 0; j < ColumnCount; j++)
            {
                ret[j] = new T[RowCount];
            }
            for (int i = 0; i < Data.Length; i++)
            {
                ret[i][i] = Data[i];
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

        // ENUMERATION

        public override IEnumerable<T> Enumerate()
        {
            for (int j = 0; j < ColumnCount; j++)
            {
                for (int i = 0; i < RowCount; i++)
                {
                    // PERF: consider to break up loop to avoid branching
                    yield return i == j ? Data[i] : Zero;
                }
            }
        }

        public override IEnumerable<(int, int, T)> EnumerateIndexed()
        {
            for (int j = 0; j < ColumnCount; j++)
            {
                for (int i = 0; i < RowCount; i++)
                {
                    // PERF: consider to break up loop to avoid branching
                    yield return (i, j, i == j ? Data[i] : Zero);
                }
            }
        }

        public override IEnumerable<T> EnumerateNonZero()
        {
            return Data.Where(x => !Zero.Equals(x));
        }

        public override IEnumerable<(int, int, T)> EnumerateNonZeroIndexed()
        {
            for (int i = 0; i < Data.Length; i++)
            {
                if (!Zero.Equals(Data[i]))
                {
                    yield return (i, i, Data[i]);
                }
            }
        }

        // FIND

        public override Tuple<int, int, T> Find(Func<T, bool> predicate, Zeros zeros)
        {
            for (int i = 0; i < Data.Length; i++)
            {
                if (predicate(Data[i]))
                {
                    return new Tuple<int, int, T>(i, i, Data[i]);
                }
            }
            if (zeros == Zeros.Include && (RowCount > 1 || ColumnCount > 1))
            {
                if (predicate(Zero))
                {
                    return new Tuple<int, int, T>(RowCount > 1 ? 1 : 0, RowCount > 1 ? 0 : 1, Zero);
                }
            }
            return null;
        }

        internal override Tuple<int, int, T, TOther> Find2Unchecked<TOther>(MatrixStorage<TOther> other, Func<T, TOther, bool> predicate, Zeros zeros)
        {
            if (other is DenseColumnMajorMatrixStorage<TOther> denseOther)
            {
                TOther[] otherData = denseOther.Data;
                int k = 0;
                for (int j = 0; j < ColumnCount; j++)
                {
                    for (int i = 0; i < RowCount; i++)
                    {
                        if (predicate(i == j ? Data[i] : Zero, otherData[k]))
                        {
                            return new Tuple<int, int, T, TOther>(i, j, i == j ? Data[i] : Zero, otherData[k]);
                        }
                        k++;
                    }
                }
                return null;
            }

            if (other is DiagonalMatrixStorage<TOther> diagonalOther)
            {
                TOther[] otherData = diagonalOther.Data;
                for (int i = 0; i < Data.Length; i++)
                {
                    if (predicate(Data[i], otherData[i]))
                    {
                        return new Tuple<int, int, T, TOther>(i, i, Data[i], otherData[i]);
                    }
                }
                if (zeros == Zeros.Include && (RowCount > 1 || ColumnCount > 1))
                {
                    TOther otherZero = BuilderInstance<TOther>.Matrix.Zero;
                    if (predicate(Zero, otherZero))
                    {
                        return new Tuple<int, int, T, TOther>(RowCount > 1 ? 1 : 0, RowCount > 1 ? 0 : 1, Zero, otherZero);
                    }
                }
                return null;
            }

            if (other is SparseCompressedRowMatrixStorage<TOther> sparseOther)
            {
                int[] otherRowPointers = sparseOther.RowPointers;
                int[] otherColumnIndices = sparseOther.ColumnIndices;
                TOther[] otherValues = sparseOther.Values;
                TOther otherZero = BuilderInstance<TOther>.Matrix.Zero;
                for (int row = 0; row < RowCount; row++)
                {
                    bool diagonal = false;
                    var startIndex = otherRowPointers[row];
                    var endIndex = otherRowPointers[row + 1];
                    for (var j = startIndex; j < endIndex; j++)
                    {
                        if (otherColumnIndices[j] == row)
                        {
                            diagonal = true;
                            if (predicate(Data[row], otherValues[j]))
                            {
                                return new Tuple<int, int, T, TOther>(row, row, Data[row], otherValues[j]);
                            }
                        }
                        else
                        {
                            if (predicate(Zero, otherValues[j]))
                            {
                                return new Tuple<int, int, T, TOther>(row, otherColumnIndices[j], Zero, otherValues[j]);
                            }
                        }
                    }
                    if (!diagonal && row < ColumnCount)
                    {
                        if (predicate(Data[row], otherZero))
                        {
                            return new Tuple<int, int, T, TOther>(row, row, Data[row], otherZero);
                        }
                    }
                }
                if (zeros == Zeros.Include && sparseOther.ValueCount < (RowCount * ColumnCount))
                {
                    if (predicate(Zero, otherZero))
                    {
                        int k = 0;
                        for (int row = 0; row < RowCount; row++)
                        {
                            for (int col = 0; col < ColumnCount; col++)
                            {
                                if (k < otherRowPointers[row + 1] && otherColumnIndices[k] == col)
                                {
                                    k++;
                                }
                                else if (row != col)
                                {
                                    return new Tuple<int, int, T, TOther>(row, col, Zero, otherZero);
                                }
                            }
                        }
                    }
                }
                return null;
            }

            // FALL BACK

            return base.Find2Unchecked(other, predicate, zeros);
        }

        // FUNCTIONAL COMBINATORS: MAP

        public override void MapInplace(Func<T, T> f, Zeros zeros)
        {
            if (zeros == Zeros.Include)
            {
                throw new NotSupportedException("Cannot map non-zero off-diagonal values into a diagonal matrix");
            }

            CommonParallel.For(0, Data.Length, 4096, (a, b) =>
            {
                for (int i = a; i < b; i++)
                {
                    Data[i] = f(Data[i]);
                }
            });
        }

        public override void MapIndexedInplace(Func<int, int, T, T> f, Zeros zeros)
        {
            if (zeros == Zeros.Include)
            {
                throw new NotSupportedException("Cannot map non-zero off-diagonal values into a diagonal matrix");
            }

            CommonParallel.For(0, Data.Length, 4096, (a, b) =>
            {
                for (int i = a; i < b; i++)
                {
                    Data[i] = f(i, i, Data[i]);
                }
            });
        }

        internal override void MapToUnchecked<TU>(MatrixStorage<TU> target, Func<T, TU> f,
            Zeros zeros, ExistingData existingData)
        {
            var processZeros = zeros == Zeros.Include || !Zero.Equals(f(Zero));

            if (target is DiagonalMatrixStorage<TU> diagonalTarget)
            {
                if (processZeros)
                {
                    throw new NotSupportedException("Cannot map non-zero off-diagonal values into a diagonal matrix");
                }

                var diagonalTargetData = diagonalTarget.Data;

                CommonParallel.For(0, Data.Length, 4096, (a, b) =>
                {
                    for (int i = a; i < b; i++)
                    {
                        diagonalTargetData[i] = f(Data[i]);
                    }
                });
                return;
            }

            // FALL BACK

            if (existingData == ExistingData.Clear && !processZeros)
            {
                target.Clear();
            }

            if (processZeros)
            {
                for (int j = 0; j < ColumnCount; j++)
                {
                    for (int i = 0; i < RowCount; i++)
                    {
                        target.At(i, j, f(i == j ? Data[i] : Zero));
                    }
                }
            }
            else
            {
                for (int i = 0; i < Data.Length; i++)
                {
                    target.At(i, i, f(Data[i]));
                }
            }
        }

        internal override void MapIndexedToUnchecked<TU>(MatrixStorage<TU> target, Func<int, int, T, TU> f,
            Zeros zeros, ExistingData existingData)
        {
            var processZeros = zeros == Zeros.Include || !Zero.Equals(f(0, 1, Zero));

            if (target is DiagonalMatrixStorage<TU> diagonalTarget)
            {
                if (processZeros)
                {
                    throw new NotSupportedException("Cannot map non-zero off-diagonal values into a diagonal matrix");
                }

                var diagonalTargetData = diagonalTarget.Data;

                CommonParallel.For(0, Data.Length, 4096, (a, b) =>
                {
                    for (int i = a; i < b; i++)
                    {
                        diagonalTargetData[i] = f(i, i, Data[i]);
                    }
                });
                return;
            }

            // FALL BACK

            if (existingData == ExistingData.Clear && !processZeros)
            {
                target.Clear();
            }

            if (processZeros)
            {
                for (int j = 0; j < ColumnCount; j++)
                {
                    for (int i = 0; i < RowCount; i++)
                    {
                        target.At(i, j, f(i, j, i == j ? Data[i] : Zero));
                    }
                }
            }
            else
            {
                for (int i = 0; i < Data.Length; i++)
                {
                    target.At(i, i, f(i, i, Data[i]));
                }
            }
        }

        internal override void MapSubMatrixIndexedToUnchecked<TU>(MatrixStorage<TU> target, Func<int, int, T, TU> f,
            int sourceRowIndex, int targetRowIndex, int rowCount,
            int sourceColumnIndex, int targetColumnIndex, int columnCount,
            Zeros zeros, ExistingData existingData)
        {
            if (target is DiagonalMatrixStorage<TU> diagonalTarget)
            {
                MapSubMatrixIndexedToUnchecked(diagonalTarget, f, sourceRowIndex, targetRowIndex, rowCount, sourceColumnIndex, targetColumnIndex, columnCount, zeros);
                return;
            }

            if (target is DenseColumnMajorMatrixStorage<TU> denseTarget)
            {
                MapSubMatrixIndexedToUnchecked(denseTarget, f, sourceRowIndex, targetRowIndex, rowCount, sourceColumnIndex, targetColumnIndex, columnCount, zeros, existingData);
                return;
            }

            // TODO: Proper Sparse Implementation

            // FALL BACK

            if (existingData == ExistingData.Clear)
            {
                target.ClearUnchecked(targetRowIndex, rowCount, targetColumnIndex, columnCount);
            }

            if (sourceRowIndex == sourceColumnIndex)
            {
                int targetRow = targetRowIndex;
                int targetColumn = targetColumnIndex;
                for (var i = 0; i < Math.Min(columnCount, rowCount); i++)
                {
                    target.At(targetRow, targetColumn, f(targetRow, targetColumn, Data[sourceRowIndex + i]));
                    targetRow++;
                    targetColumn++;
                }
            }
            else if (sourceRowIndex > sourceColumnIndex && sourceColumnIndex + columnCount > sourceRowIndex)
            {
                // column by column, but skip resulting zero columns at the beginning
                int columnInit = sourceRowIndex - sourceColumnIndex;
                int targetRow = targetRowIndex;
                int targetColumn = targetColumnIndex + columnInit;
                for (var i = 0; i < Math.Min(columnCount - columnInit, rowCount); i++)
                {
                    target.At(targetRow, targetColumn, f(targetRow, targetColumn, Data[sourceRowIndex + i]));
                    targetRow++;
                    targetColumn++;
                }
            }
            else if (sourceRowIndex < sourceColumnIndex && sourceRowIndex + rowCount > sourceColumnIndex)
            {
                // row by row, but skip resulting zero rows at the beginning
                int rowInit = sourceColumnIndex - sourceRowIndex;
                int targetRow = targetRowIndex + rowInit;
                int targetColumn = targetColumnIndex;
                for (var i = 0; i < Math.Min(columnCount, rowCount - rowInit); i++)
                {
                    target.At(targetRow, targetColumn, f(targetRow, targetColumn, Data[sourceColumnIndex + i]));
                    targetRow++;
                    targetColumn++;
                }
            }
        }

        void MapSubMatrixIndexedToUnchecked<TU>(DiagonalMatrixStorage<TU> target, Func<int, int, T, TU> f,
            int sourceRowIndex, int targetRowIndex, int rowCount,
            int sourceColumnIndex, int targetColumnIndex, int columnCount,
            Zeros zeros)
            where TU : struct, IEquatable<TU>, IFormattable
        {
            var processZeros = zeros == Zeros.Include || !Zero.Equals(f(0, 1, Zero));
            if (processZeros || sourceRowIndex - sourceColumnIndex != targetRowIndex - targetColumnIndex)
            {
                throw new NotSupportedException("Cannot map non-zero off-diagonal values into a diagonal matrix");
            }

            var beginInclusive = Math.Max(sourceRowIndex, sourceColumnIndex);
            var count = Math.Min(sourceRowIndex + rowCount, sourceColumnIndex + columnCount) - beginInclusive;
            if (count > 0)
            {
                var targetData = target.Data;
                var beginTarget = Math.Max(targetRowIndex, targetColumnIndex);
                CommonParallel.For(0, count, 4096, (a, b) =>
                {
                    int targetIndex = beginTarget + a;
                    for (int i = a; i < b; i++)
                    {
                        targetData[targetIndex] = f(targetIndex, targetIndex, Data[beginInclusive + i]);
                        targetIndex++;
                    }
                });
            }
        }

        void MapSubMatrixIndexedToUnchecked<TU>(DenseColumnMajorMatrixStorage<TU> target, Func<int, int, T, TU> f,
            int sourceRowIndex, int targetRowIndex, int rowCount,
            int sourceColumnIndex, int targetColumnIndex, int columnCount,
            Zeros zeros, ExistingData existingData)
            where TU : struct, IEquatable<TU>, IFormattable
        {
            var processZeros = zeros == Zeros.Include || !Zero.Equals(f(0, 1, Zero));
            if (existingData == ExistingData.Clear && !processZeros)
            {
                target.ClearUnchecked(targetRowIndex, rowCount, targetColumnIndex, columnCount);
            }

            if (processZeros)
            {
                var targetData = target.Data;
                CommonParallel.For(0, columnCount, Math.Max(4096/rowCount, 32), (a, b) =>
                {
                    int sourceColumn = sourceColumnIndex + a;
                    int targetColumn = targetColumnIndex + a;
                    for (int j = a; j < b; j++)
                    {
                        int targetIndex = targetRowIndex + (j + targetColumnIndex)*target.RowCount;
                        int sourceRow = sourceRowIndex;
                        int targetRow = targetRowIndex;
                        for (int i = 0; i < rowCount; i++)
                        {
                            targetData[targetIndex++] = f(targetRow++, targetColumn, sourceRow++ == sourceColumn ? Data[sourceColumn] : Zero);
                        }
                        sourceColumn++;
                        targetColumn++;
                    }
                });
            }
            else
            {
                if (sourceRowIndex > sourceColumnIndex && sourceColumnIndex + columnCount > sourceRowIndex)
                {
                    // column by column, but skip resulting zero columns at the beginning

                    int columnInit = sourceRowIndex - sourceColumnIndex;
                    int offset = (columnInit + targetColumnIndex)*target.RowCount + targetRowIndex;
                    int step = target.RowCount + 1;
                    int count = Math.Min(columnCount - columnInit, rowCount);

                    var targetData = target.Data;
                    for (int k = 0, j = offset; k < count; j += step, k++)
                    {
                        targetData[j] = f(targetRowIndex + k, targetColumnIndex + columnInit + k, Data[sourceRowIndex + k]);
                    }
                }
                else if (sourceRowIndex < sourceColumnIndex && sourceRowIndex + rowCount > sourceColumnIndex)
                {
                    // row by row, but skip resulting zero rows at the beginning

                    int rowInit = sourceColumnIndex - sourceRowIndex;
                    int offset = targetColumnIndex*target.RowCount + rowInit + targetRowIndex;
                    int step = target.RowCount + 1;
                    int count = Math.Min(columnCount, rowCount - rowInit);

                    var targetData = target.Data;
                    for (int k = 0, j = offset; k < count; j += step, k++)
                    {
                        targetData[j] = f(targetRowIndex + rowInit + k, targetColumnIndex + k, Data[sourceColumnIndex + k]);
                    }
                }
                else
                {
                    int offset = targetColumnIndex*target.RowCount + targetRowIndex;
                    int step = target.RowCount + 1;
                    var count = Math.Min(columnCount, rowCount);

                    var targetData = target.Data;
                    for (int k = 0, j = offset; k < count; j += step, k++)
                    {;
                        targetData[j] = f(targetRowIndex + k, targetColumnIndex + k, Data[sourceRowIndex + k]);
                    }
                }
            }
        }

        // FUNCTIONAL COMBINATORS: FOLD

        internal override void FoldByRowUnchecked<TU>(TU[] target, Func<TU, T, TU> f, Func<TU, int, TU> finalize, TU[] state, Zeros zeros)
        {
            if (zeros == Zeros.AllowSkip)
            {
                for (int k = 0; k < Data.Length; k++)
                {
                    target[k] = finalize(f(state[k], Data[k]), 1);
                }

                for (int k = Data.Length; k < RowCount; k++)
                {
                    target[k] = finalize(state[k], 0);
                }
            }
            else
            {
                for (int i = 0; i < RowCount; i++)
                {
                    TU s = state[i];
                    for (int j = 0; j < ColumnCount; j++)
                    {
                        s = f(s, i == j ? Data[i] : Zero);
                    }
                    target[i] = finalize(s, ColumnCount);
                }
            }
        }

        internal override void FoldByColumnUnchecked<TU>(TU[] target, Func<TU, T, TU> f, Func<TU, int, TU> finalize, TU[] state, Zeros zeros)
        {
            if (zeros == Zeros.AllowSkip)
            {
                for (int k = 0; k < Data.Length; k++)
                {
                    target[k] = finalize(f(state[k], Data[k]), 1);
                }

                for (int k = Data.Length; k < ColumnCount; k++)
                {
                    target[k] = finalize(state[k], 0);
                }
            }
            else
            {
                for (int j = 0; j < ColumnCount; j++)
                {
                    TU s = state[j];
                    for (int i = 0; i < RowCount; i++)
                    {
                        s = f(s, i == j ? Data[i] : Zero);
                    }
                    target[j] = finalize(s, RowCount);
                }
            }
        }

        internal override TState Fold2Unchecked<TOther, TState>(MatrixStorage<TOther> other, Func<TState, T, TOther, TState> f, TState state, Zeros zeros)
        {
            if (other is DenseColumnMajorMatrixStorage<TOther> denseOther)
            {
                TOther[] otherData = denseOther.Data;
                int k = 0;
                for (int j = 0; j < ColumnCount; j++)
                {
                    for (int i = 0; i < RowCount; i++)
                    {
                        state = f(state, i == j ? Data[i] : Zero, otherData[k]);
                        k++;
                    }
                }
                return state;
            }

            if (other is DiagonalMatrixStorage<TOther> diagonalOther)
            {
                TOther[] otherData = diagonalOther.Data;
                for (int i = 0; i < Data.Length; i++)
                {
                    state = f(state, Data[i], otherData[i]);
                }

                // Do we really need to do this?
                if (zeros == Zeros.Include)
                {
                    TOther otherZero = BuilderInstance<TOther>.Matrix.Zero;
                    int count = RowCount*ColumnCount - Data.Length;
                    for (int i = 0; i < count; i++)
                    {
                        state = f(state, Zero, otherZero);
                    }
                }

                return state;
            }

            if (other is SparseCompressedRowMatrixStorage<TOther> sparseOther)
            {
                int[] otherRowPointers = sparseOther.RowPointers;
                int[] otherColumnIndices = sparseOther.ColumnIndices;
                TOther[] otherValues = sparseOther.Values;
                TOther otherZero = BuilderInstance<TOther>.Matrix.Zero;

                if (zeros == Zeros.Include)
                {
                    int k = 0;
                    for (int row = 0; row < RowCount; row++)
                    {
                        for (int col = 0; col < ColumnCount; col++)
                        {
                            if (k < otherRowPointers[row + 1] && otherColumnIndices[k] == col)
                            {
                                state = f(state, row == col ? Data[row] : Zero, otherValues[k++]);
                            }
                            else
                            {
                                state = f(state, row == col ? Data[row] : Zero, otherZero);
                            }
                        }
                    }
                    return state;
                }

                for (int row = 0; row < RowCount; row++)
                {
                    bool diagonal = false;

                    var startIndex = otherRowPointers[row];
                    var endIndex = otherRowPointers[row + 1];
                    for (var j = startIndex; j < endIndex; j++)
                    {
                        if (otherColumnIndices[j] == row)
                        {
                            diagonal = true;
                            state = f(state, Data[row], otherValues[j]);
                        }
                        else
                        {
                            state = f(state, Zero, otherValues[j]);
                        }
                    }

                    if (!diagonal && row < ColumnCount)
                    {
                        state = f(state, Data[row], otherZero);
                    }
                }

                return state;
            }

            // FALL BACK

            return base.Fold2Unchecked(other, f, state, zeros);
        }
    }
}
