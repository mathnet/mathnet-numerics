// <copyright file="DenseColumnMajorMatrixStorage.cs" company="Math.NET">
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
using System.Linq;
using MathNet.Numerics.Properties;
using MathNet.Numerics.Threading;

namespace MathNet.Numerics.LinearAlgebra.Storage
{
    [Serializable]
    public class DenseColumnMajorMatrixStorage<T> : MatrixStorage<T>
        where T : struct, IEquatable<T>, IFormattable
    {
        // [ruegg] public fields are OK here

        public readonly T[] Data;

        internal DenseColumnMajorMatrixStorage(int rows, int columns)
            : base(rows, columns)
        {
            Data = new T[rows*columns];
        }

        internal DenseColumnMajorMatrixStorage(int rows, int columns, T[] data)
            : base(rows, columns)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            if (data.Length != rows*columns)
            {
                throw new ArgumentOutOfRangeException("data", string.Format(Resources.ArgumentArrayWrongLength, rows*columns));
            }

            Data = data;
        }

        /// <summary>
        /// True if the matrix storage format is dense.
        /// </summary>
        public override bool IsDense
        {
            get { return true; }
        }

        /// <summary>
        /// True if all fields of this matrix can be set to any value.
        /// False if some fields are fixed, like on a diagonal matrix.
        /// </summary>
        public override bool IsFullyMutable
        {
            get { return true; }
        }

        /// <summary>
        /// True if the specified field can be set to any value.
        /// False if the field is fixed, like an off-diagonal field on a diagonal matrix.
        /// </summary>
        public override bool IsMutableAt(int row, int column)
        {
            return true;
        }

        /// <summary>
        /// Retrieves the requested element without range checking.
        /// </summary>
        public override T At(int row, int column)
        {
            return Data[(column*RowCount) + row];
        }

        /// <summary>
        /// Sets the element without range checking.
        /// </summary>
        public override void At(int row, int column, T value)
        {
            Data[(column*RowCount) + row] = value;
        }

        public override void Clear()
        {
            Array.Clear(Data, 0, Data.Length);
        }

        public override void Clear(int rowIndex, int rowCount, int columnIndex, int columnCount)
        {
            if (rowIndex == 0 && columnIndex == 0 && rowCount == RowCount && columnCount == ColumnCount)
            {
                Clear();
                return;
            }

            for (int j = columnIndex; j < columnIndex + columnCount; j++)
            {
                Array.Clear(Data, j*RowCount + rowIndex, rowCount);
            }
        }

        public override void ClearRows(int[] rowIndices)
        {
            for (var j = 0; j < ColumnCount; j++)
            {
                int offset = j*RowCount;
                for (var k = 0; k < rowIndices.Length; k++)
                {
                    Data[offset + rowIndices[k]] = Zero;
                }
            }
        }

        public override void ClearColumns(int[] columnIndices)
        {
            for (int k = 0; k < columnIndices.Length; k++)
            {
                Array.Clear(Data, columnIndices[k]*RowCount, RowCount);
            }
        }

        // INITIALIZATION

        public static DenseColumnMajorMatrixStorage<T> OfMatrix(MatrixStorage<T> matrix)
        {
            var storage = new DenseColumnMajorMatrixStorage<T>(matrix.RowCount, matrix.ColumnCount);
            matrix.CopyToUnchecked(storage, ExistingData.AssumeZeros);
            return storage;
        }

        public static DenseColumnMajorMatrixStorage<T> OfValue(int rows, int columns, T value)
        {
            var storage = new DenseColumnMajorMatrixStorage<T>(rows, columns);
            var data = storage.Data;
            CommonParallel.For(0, data.Length, 4096, (a, b) =>
            {
                for (int i = a; i < b; i++)
                {
                    data[i] = value;
                }
            });
            return storage;
        }

        public static DenseColumnMajorMatrixStorage<T> OfInit(int rows, int columns, Func<int, int, T> init)
        {
            var storage = new DenseColumnMajorMatrixStorage<T>(rows, columns);
            int index = 0;
            for (var j = 0; j < columns; j++)
            {
                for (var i = 0; i < rows; i++)
                {
                    storage.Data[index++] = init(i, j);
                }
            }
            return storage;
        }

        public static DenseColumnMajorMatrixStorage<T> OfDiagonalInit(int rows, int columns, Func<int, T> init)
        {
            var storage = new DenseColumnMajorMatrixStorage<T>(rows, columns);
            int index = 0;
            int stride = rows + 1;
            for (var i = 0; i < Math.Min(rows, columns); i++)
            {
                storage.Data[index] = init(i);
                index += stride;
            }
            return storage;
        }

        public static DenseColumnMajorMatrixStorage<T> OfArray(T[,] array)
        {
            var storage = new DenseColumnMajorMatrixStorage<T>(array.GetLength(0), array.GetLength(1));
            int index = 0;
            for (var j = 0; j < storage.ColumnCount; j++)
            {
                for (var i = 0; i < storage.RowCount; i++)
                {
                    storage.Data[index++] = array[i, j];
                }
            }
            return storage;
        }

        public static DenseColumnMajorMatrixStorage<T> OfColumnArrays(T[][] data)
        {
            int columns = data.Length;
            int rows = data[0].Length;
            var array = new T[rows*columns];
            for (int j = 0; j < data.Length; j++)
            {
                Array.Copy(data[j], 0, array, j*rows, rows);
            }
            return new DenseColumnMajorMatrixStorage<T>(rows, columns, array);
        }

        public static DenseColumnMajorMatrixStorage<T> OfRowArrays(T[][] data)
        {
            int rows = data.Length;
            int columns = data[0].Length;
            var array = new T[rows*columns];
            for (int j = 0; j < columns; j++)
            {
                int offset = j*rows;
                for (int i = 0; i < rows; i++)
                {
                    array[offset + i] = data[i][j];
                }
            }
            return new DenseColumnMajorMatrixStorage<T>(rows, columns, array);
        }

        public static DenseColumnMajorMatrixStorage<T> OfColumnVectors(VectorStorage<T>[] data)
        {
            int columns = data.Length;
            int rows = data[0].Length;
            var array = new T[rows*columns];
            for (int j = 0; j < data.Length; j++)
            {
                var column = data[j];
                var denseColumn = column as DenseVectorStorage<T>;
                if (denseColumn != null)
                {
                    Array.Copy(denseColumn.Data, 0, array, j*rows, rows);
                }
                else
                {
                    // FALL BACK
                    int offset = j*rows;
                    for (int i = 0; i < rows; i++)
                    {
                        array[offset + i] = column.At(i);
                    }
                }
            }
            return new DenseColumnMajorMatrixStorage<T>(rows, columns, array);
        }

        public static DenseColumnMajorMatrixStorage<T> OfRowVectors(VectorStorage<T>[] data)
        {
            int rows = data.Length;
            int columns = data[0].Length;
            var array = new T[rows*columns];
            for (int j = 0; j < columns; j++)
            {
                int offset = j*rows;
                for (int i = 0; i < rows; i++)
                {
                    array[offset + i] = data[i].At(j);
                }
            }
            return new DenseColumnMajorMatrixStorage<T>(rows, columns, array);
        }

        public static DenseColumnMajorMatrixStorage<T> OfIndexedEnumerable(int rows, int columns, IEnumerable<Tuple<int, int, T>> data)
        {
            var array = new T[rows*columns];
            foreach (var item in data)
            {
                array[(item.Item2*rows) + item.Item1] = item.Item3;
            }
            return new DenseColumnMajorMatrixStorage<T>(rows, columns, array);
        }

        public static DenseColumnMajorMatrixStorage<T> OfColumnMajorEnumerable(int rows, int columns, IEnumerable<T> data)
        {
            var arrayData = data as T[];
            if (arrayData != null)
            {
                var copy = new T[arrayData.Length];
                Array.Copy(arrayData, copy, arrayData.Length);
                return new DenseColumnMajorMatrixStorage<T>(rows, columns, copy);
            }

            return new DenseColumnMajorMatrixStorage<T>(rows, columns, data.ToArray());
        }

        public static DenseColumnMajorMatrixStorage<T> OfColumnEnumerables(int rows, int columns, IEnumerable<IEnumerable<T>> data)
        {
            var array = new T[rows*columns];
            using (var columnIterator = data.GetEnumerator())
            {
                for (int column = 0; column < columns; column++)
                {
                    if (!columnIterator.MoveNext()) throw new ArgumentOutOfRangeException("data", string.Format(Resources.ArgumentArrayWrongLength, columns));
                    var arrayColumn = columnIterator.Current as T[];
                    if (arrayColumn != null)
                    {
                        Array.Copy(arrayColumn, 0, array, column*rows, rows);
                    }
                    else
                    {
                        using (var rowIterator = columnIterator.Current.GetEnumerator())
                        {
                            var end = (column + 1)*rows;
                            for (int index = column*rows; index < end; index++)
                            {
                                if (!rowIterator.MoveNext()) throw new ArgumentOutOfRangeException("data", string.Format(Resources.ArgumentArrayWrongLength, rows));
                                array[index] = rowIterator.Current;
                            }
                            if (rowIterator.MoveNext()) throw new ArgumentOutOfRangeException("data", string.Format(Resources.ArgumentArrayWrongLength, rows));
                        }
                    }
                }
                if (columnIterator.MoveNext()) throw new ArgumentOutOfRangeException("data", string.Format(Resources.ArgumentArrayWrongLength, columns));
            }
            return new DenseColumnMajorMatrixStorage<T>(rows, columns, array);
        }

        public static DenseColumnMajorMatrixStorage<T> OfRowEnumerables(int rows, int columns, IEnumerable<IEnumerable<T>> data)
        {
            var array = new T[rows*columns];
            using (var rowIterator = data.GetEnumerator())
            {
                for (int row = 0; row < rows; row++)
                {
                    if (!rowIterator.MoveNext()) throw new ArgumentOutOfRangeException("data", string.Format(Resources.ArgumentArrayWrongLength, rows));
                    using (var columnIterator = rowIterator.Current.GetEnumerator())
                    {
                        for (int index = row; index < array.Length; index += rows)
                        {
                            if (!columnIterator.MoveNext()) throw new ArgumentOutOfRangeException("data", string.Format(Resources.ArgumentArrayWrongLength, columns));
                            array[index] = columnIterator.Current;
                        }
                        if (columnIterator.MoveNext()) throw new ArgumentOutOfRangeException("data", string.Format(Resources.ArgumentArrayWrongLength, columns));
                    }
                }
                if (rowIterator.MoveNext()) throw new ArgumentOutOfRangeException("data", string.Format(Resources.ArgumentArrayWrongLength, rows));
            }
            return new DenseColumnMajorMatrixStorage<T>(rows, columns, array);
        }

        // MATRIX COPY

        internal override void CopyToUnchecked(MatrixStorage<T> target, ExistingData existingData = ExistingData.Clear)
        {
            var denseTarget = target as DenseColumnMajorMatrixStorage<T>;
            if (denseTarget != null)
            {
                CopyToUnchecked(denseTarget);
                return;
            }

            // FALL BACK

            for (int j = 0, offset = 0; j < ColumnCount; j++, offset += RowCount)
            {
                for (int i = 0; i < RowCount; i++)
                {
                    target.At(i, j, Data[i + offset]);
                }
            }
        }

        void CopyToUnchecked(DenseColumnMajorMatrixStorage<T> target)
        {
            //Buffer.BlockCopy(Data, 0, target.Data, 0, Data.Length * System.Runtime.InteropServices.Marshal.SizeOf(typeof(T)));
            Array.Copy(Data, 0, target.Data, 0, Data.Length);
        }

        internal override void CopySubMatrixToUnchecked(MatrixStorage<T> target,
            int sourceRowIndex, int targetRowIndex, int rowCount,
            int sourceColumnIndex, int targetColumnIndex, int columnCount,
            ExistingData existingData = ExistingData.Clear)
        {
            var denseTarget = target as DenseColumnMajorMatrixStorage<T>;
            if (denseTarget != null)
            {
                CopySubMatrixToUnchecked(denseTarget, sourceRowIndex, targetRowIndex, rowCount, sourceColumnIndex, targetColumnIndex, columnCount);
                return;
            }

            // TODO: Proper Sparse Implementation

            // FALL BACK

            for (int j = sourceColumnIndex, jj = targetColumnIndex; j < sourceColumnIndex + columnCount; j++, jj++)
            {
                int index = sourceRowIndex + j*RowCount;
                for (int ii = targetRowIndex; ii < targetRowIndex + rowCount; ii++)
                {
                    target.At(ii, jj, Data[index++]);
                }
            }
        }

        void CopySubMatrixToUnchecked(DenseColumnMajorMatrixStorage<T> target,
            int sourceRowIndex, int targetRowIndex, int rowCount,
            int sourceColumnIndex, int targetColumnIndex, int columnCount)
        {
            for (int j = sourceColumnIndex, jj = targetColumnIndex; j < sourceColumnIndex + columnCount; j++, jj++)
            {
                //Buffer.BlockCopy(Data, j*RowCount + sourceRowIndex, target.Data, jj*target.RowCount + targetRowIndex, rowCount * System.Runtime.InteropServices.Marshal.SizeOf(typeof(T)));
                Array.Copy(Data, j*RowCount + sourceRowIndex, target.Data, jj*target.RowCount + targetRowIndex, rowCount);
            }
        }

        // ROW COPY

        internal override void CopySubRowToUnchecked(VectorStorage<T> target, int rowIndex, int sourceColumnIndex, int targetColumnIndex, int columnCount,
            ExistingData existingData = ExistingData.Clear)
        {
            var targetDense = target as DenseVectorStorage<T>;
            if (targetDense != null)
            {
                for (int j = 0; j < columnCount; j++)
                {
                    targetDense.Data[j + targetColumnIndex] = Data[(j + sourceColumnIndex)*RowCount + rowIndex];
                }
                return;
            }

            // FALL BACK

            for (int j = sourceColumnIndex, jj = targetColumnIndex; j < sourceColumnIndex + columnCount; j++, jj++)
            {
                target.At(jj, Data[(j*RowCount) + rowIndex]);
            }
        }

        // COLUMN COPY

        internal override void CopySubColumnToUnchecked(VectorStorage<T> target, int columnIndex, int sourceRowIndex, int targetRowIndex, int rowCount,
            ExistingData existingData = ExistingData.Clear)
        {
            var targetDense = target as DenseVectorStorage<T>;
            if (targetDense != null)
            {
                Array.Copy(Data, columnIndex*RowCount + sourceRowIndex, targetDense.Data, targetRowIndex, rowCount);
                return;
            }

            // FALL BACK

            var offset = columnIndex*RowCount;
            for (int i = sourceRowIndex, ii = targetRowIndex; i < sourceRowIndex + rowCount; i++, ii++)
            {
                target.At(ii, Data[offset + i]);
            }
        }

        // TRANSPOSE

        internal override void TransposeToUnchecked(MatrixStorage<T> target, ExistingData existingData = ExistingData.Clear)
        {
            var denseTarget = target as DenseColumnMajorMatrixStorage<T>;
            if (denseTarget != null)
            {
                TransposeToUnchecked(denseTarget);
                return;
            }

            var sparseTarget = target as SparseCompressedRowMatrixStorage<T>;
            if (sparseTarget != null)
            {
                TransposeToUnchecked(sparseTarget);
                return;
            }

            // FALL BACK

            for (int j = 0, offset = 0; j < ColumnCount; j++, offset += RowCount)
            {
                for (int i = 0; i < RowCount; i++)
                {
                    target.At(j, i, Data[i + offset]);
                }
            }
        }

        void TransposeToUnchecked(DenseColumnMajorMatrixStorage<T> target)
        {
            for (var j = 0; j < ColumnCount; j++)
            {
                var index = j * RowCount;
                for (var i = 0; i < RowCount; i++)
                {
                    target.Data[(i * ColumnCount) + j] = Data[index + i];
                }
            }
        }

        void TransposeToUnchecked(SparseCompressedRowMatrixStorage<T> target)
        {
            var rowPointers = target.RowPointers;
            var columnIndices = new List<int>();
            var values = new List<T>();

            for (int j = 0; j < ColumnCount; j++)
            {
                rowPointers[j] = values.Count;
                var index = j * RowCount;
                for (int i = 0; i < RowCount; i++)
                {
                    if (!Zero.Equals(Data[index + i]))
                    {
                        values.Add(Data[index + i]);
                        columnIndices.Add(i);
                    }
                }
            }

            rowPointers[ColumnCount] = values.Count;
            target.ColumnIndices = columnIndices.ToArray();
            target.Values = values.ToArray();
        }

        // EXTRACT

        public override T[] ToRowMajorArray()
        {
            var ret = new T[Data.Length];
            for (int i = 0; i < RowCount; i++)
            {
                var offset = i*ColumnCount;
                for (int j = 0; j < ColumnCount; j++)
                {
                    ret[offset + j] = Data[(j*RowCount) + i];
                }
            }
            return ret;
        }

        public override T[] ToColumnMajorArray()
        {
            var ret = new T[Data.Length];
            Array.Copy(Data, ret, Data.Length);
            return ret;
        }

        public override T[,] ToArray()
        {
            var ret = new T[RowCount, ColumnCount];
            for (int i = 0; i < RowCount; i++)
            {
                for (int j = 0; j < ColumnCount; j++)
                {
                    ret[i, j] = Data[(j*RowCount) + i];
                }
            }
            return ret;
        }

        // ENUMERATION

        public override IEnumerable<T> Enumerate()
        {
            return Data;
        }

        public override IEnumerable<Tuple<int, int, T>> EnumerateIndexed()
        {
            int index = 0;
            for (int j = 0; j < ColumnCount; j++)
            {
                for (int i = 0; i < RowCount; i++)
                {
                    yield return new Tuple<int, int, T>(i, j, Data[index]);
                    index++;
                }
            }
        }

        public override IEnumerable<T> EnumerateNonZero()
        {
            return Data.Where(x => !Zero.Equals(x));
        }

        public override IEnumerable<Tuple<int, int, T>> EnumerateNonZeroIndexed()
        {
            int index = 0;
            for (int j = 0; j < ColumnCount; j++)
            {
                for (int i = 0; i < RowCount; i++)
                {
                    var x = Data[index];
                    if (!Zero.Equals(x))
                    {
                        yield return new Tuple<int, int, T>(i, j, x);
                    }
                    index++;
                }
            }
        }

        // FUNCTIONAL COMBINATORS: MAP

        public override void MapInplace(Func<T, T> f, Zeros zeros = Zeros.AllowSkip)
        {
            CommonParallel.For(0, Data.Length, 4096, (a, b) =>
            {
                for (int i = a; i < b; i++)
                {
                    Data[i] = f(Data[i]);
                }
            });
        }

        public override void MapIndexedInplace(Func<int, int, T, T> f, Zeros zeros = Zeros.AllowSkip)
        {
            CommonParallel.For(0, ColumnCount, Math.Max(4096/RowCount, 32), (a, b) =>
            {
                int index = a*RowCount;
                for (int j = a; j < b; j++)
                {
                    for (int i = 0; i < RowCount; i++)
                    {
                        Data[index] = f(i, j, Data[index]);
                        index++;
                    }
                }
            });
        }

        internal override void MapToUnchecked<TU>(MatrixStorage<TU> target, Func<T, TU> f,
            Zeros zeros = Zeros.AllowSkip, ExistingData existingData = ExistingData.Clear)
        {
            var denseTarget = target as DenseColumnMajorMatrixStorage<TU>;
            if (denseTarget != null)
            {
                CommonParallel.For(0, Data.Length, 4096, (a, b) =>
                {
                    for (int i = a; i < b; i++)
                    {
                        denseTarget.Data[i] = f(Data[i]);
                    }
                });
                return;
            }

            // FALL BACK

            int index = 0;
            for (int j = 0; j < ColumnCount; j++)
            {
                for (int i = 0; i < RowCount; i++)
                {
                    target.At(i, j, f(Data[index++]));
                }
            }
        }

        internal override void MapIndexedToUnchecked<TU>(MatrixStorage<TU> target, Func<int, int, T, TU> f,
            Zeros zeros = Zeros.AllowSkip, ExistingData existingData = ExistingData.Clear)
        {
            var denseTarget = target as DenseColumnMajorMatrixStorage<TU>;
            if (denseTarget != null)
            {
                CommonParallel.For(0, ColumnCount, Math.Max(4096/RowCount, 32), (a, b) =>
                {
                    int index = a*RowCount;
                    for (int j = a; j < b; j++)
                    {
                        for (int i = 0; i < RowCount; i++)
                        {
                            denseTarget.Data[index] = f(i, j, Data[index]);
                            index++;
                        }
                    }
                });
                return;
            }

            // FALL BACK

            int index2 = 0;
            for (int j = 0; j < ColumnCount; j++)
            {
                for (int i = 0; i < RowCount; i++)
                {
                    target.At(i, j, f(i, j, Data[index2++]));
                }
            }
        }

        internal override void MapSubMatrixIndexedToUnchecked<TU>(MatrixStorage<TU> target, Func<int, int, T, TU> f,
            int sourceRowIndex, int targetRowIndex, int rowCount,
            int sourceColumnIndex, int targetColumnIndex, int columnCount,
            Zeros zeros = Zeros.AllowSkip, ExistingData existingData = ExistingData.Clear)
        {
            var denseTarget = target as DenseColumnMajorMatrixStorage<TU>;
            if (denseTarget != null)
            {
                CommonParallel.For(0, columnCount, Math.Max(4096/rowCount, 32), (a, b) =>
                {
                    for (int j = a; j < b; j++)
                    {
                        int sourceIndex = sourceRowIndex + (j + sourceColumnIndex)*RowCount;
                        int targetIndex = targetRowIndex + (j + targetColumnIndex)*target.RowCount;
                        for (int i = 0; i < rowCount; i++)
                        {
                            denseTarget.Data[targetIndex++] = f(targetRowIndex + i, targetColumnIndex + j, Data[sourceIndex++]);
                        }
                    }
                });
                return;
            }

            // TODO: Proper Sparse Implementation

            // FALL BACK

            for (int j = sourceColumnIndex, jj = targetColumnIndex; j < sourceColumnIndex + columnCount; j++, jj++)
            {
                int index = sourceRowIndex + j*RowCount;
                for (int ii = targetRowIndex; ii < targetRowIndex + rowCount; ii++)
                {
                    target.At(ii, jj, f(ii, jj, Data[index++]));
                }
            }
        }

        // FUNCTIONAL COMBINATORS: FOLD

        internal override void FoldByRowUnchecked<TU>(TU[] target, Func<TU, T, TU> f, Func<TU, int, TU> finalize, TU[] state, Zeros zeros = Zeros.AllowSkip)
        {
            for (int i = 0; i < RowCount; i++)
            {
                TU s = state[i];
                for (int j = 0; j < ColumnCount; j++)
                {
                    s = f(s, Data[j*RowCount + i]);
                }
                target[i] = finalize(s, ColumnCount);
            }
        }

        internal override void FoldByColumnUnchecked<TU>(TU[] target, Func<TU, T, TU> f, Func<TU, int, TU> finalize, TU[] state, Zeros zeros = Zeros.AllowSkip)
        {
            for (int j = 0; j < ColumnCount; j++)
            {
                int offset = j*RowCount;
                TU s = state[j];
                for (int i = 0; i < RowCount; i++)
                {
                    s = f(s, Data[offset + i]);
                }
                target[j] = finalize(s, RowCount);
            }
        }
    }
}
