// <copyright file="DenseColumnMajorMatrixStorage.cs" company="Math.NET">
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
    public class DenseColumnMajorMatrixStorage<T> : MatrixStorage<T>
        where T : struct, IEquatable<T>, IFormattable
    {
        // [ruegg] public fields are OK here

        [DataMember(Order = 1)]
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
                throw new ArgumentNullException(nameof(data));
            }

            if (data.Length != rows*columns)
            {
                throw new ArgumentOutOfRangeException(nameof(data), $"The given array has the wrong length. Should be {rows * columns}.");
            }

            Data = data;
        }

        /// <summary>
        /// True if the matrix storage format is dense.
        /// </summary>
        public override bool IsDense => true;

        /// <summary>
        /// True if all fields of this matrix can be set to any value.
        /// False if some fields are fixed, like on a diagonal matrix.
        /// </summary>
        public override bool IsFullyMutable => true;

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

        /// <summary>
        /// Evaluate the row and column at a specific data index.
        /// </summary>
        void RowColumnAtIndex(int index, out int row, out int column)
        {
            column = Math.DivRem(index, RowCount, out row);
        }

        // CLEARING

        public override void Clear()
        {
            Array.Clear(Data, 0, Data.Length);
        }

        internal override void ClearUnchecked(int rowIndex, int rowCount, int columnIndex, int columnCount)
        {
            if (rowIndex == 0 && columnIndex == 0 && rowCount == RowCount && columnCount == ColumnCount)
            {
                Array.Clear(Data, 0, Data.Length);
                return;
            }

            for (int j = columnIndex; j < columnIndex + columnCount; j++)
            {
                Array.Clear(Data, j*RowCount + rowIndex, rowCount);
            }
        }

        internal override void ClearRowsUnchecked(int[] rowIndices)
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

        internal override void ClearColumnsUnchecked(int[] columnIndices)
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
            if (data.Length <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(data), "Matrices can not be empty and must have at least one row and column.");
            }

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
            if (data.Length <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(data), "Matrices can not be empty and must have at least one row and column.");
            }

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

        public static DenseColumnMajorMatrixStorage<T> OfColumnMajorArray(int rows, int columns, T[] data)
        {
            T[] ret = new T[rows*columns];
            Array.Copy(data, 0, ret, 0, Math.Min(ret.Length, data.Length));
            return new DenseColumnMajorMatrixStorage<T>(rows, columns, ret);
        }

        public static DenseColumnMajorMatrixStorage<T> OfRowMajorArray(int rows, int columns, T[] data)
        {
            T[] ret = new T[rows*columns];
            for (int i = 0; i < rows; i++)
            {
                int offset = i*columns;
                for (int j = 0; j < columns; j++)
                {
                    ret[(j*rows) + i] = data[offset + j];
                }
            }
            return new DenseColumnMajorMatrixStorage<T>(rows, columns, ret);
        }

        public static DenseColumnMajorMatrixStorage<T> OfColumnVectors(VectorStorage<T>[] data)
        {
            if (data.Length <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(data), "Matrices can not be empty and must have at least one row and column.");
            }

            int columns = data.Length;
            int rows = data[0].Length;
            var array = new T[rows*columns];
            for (int j = 0; j < data.Length; j++)
            {
                var column = data[j];
                if (column is DenseVectorStorage<T> denseColumn)
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
            if (data.Length <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(data), "Matrices can not be empty and must have at least one row and column.");
            }

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
            foreach (var (i,j,x) in data)
            {
                array[j * rows + i] = x;
            }
            return new DenseColumnMajorMatrixStorage<T>(rows, columns, array);
        }

        public static DenseColumnMajorMatrixStorage<T> OfIndexedEnumerable(int rows, int columns, IEnumerable<(int, int, T)> data)
        {
            var array = new T[rows*columns];
            foreach (var (i,j,x) in data)
            {
                array[j * rows + i] = x;
            }
            return new DenseColumnMajorMatrixStorage<T>(rows, columns, array);
        }

        public static DenseColumnMajorMatrixStorage<T> OfColumnMajorEnumerable(int rows, int columns, IEnumerable<T> data)
        {
            if (data is T[] arrayData)
            {
                return OfColumnMajorArray(rows, columns, arrayData);
            }

            return new DenseColumnMajorMatrixStorage<T>(rows, columns, data.ToArray());
        }

        public static DenseColumnMajorMatrixStorage<T> OfRowMajorEnumerable(int rows, int columns, IEnumerable<T> data)
        {
            return OfRowMajorArray(rows, columns, data as T[] ?? data.ToArray());
        }

        public static DenseColumnMajorMatrixStorage<T> OfColumnEnumerables(int rows, int columns, IEnumerable<IEnumerable<T>> data)
        {
            var array = new T[rows*columns];
            using (var columnIterator = data.GetEnumerator())
            {
                for (int column = 0; column < columns; column++)
                {
                    if (!columnIterator.MoveNext()) throw new ArgumentOutOfRangeException(nameof(data), $"The given array has the wrong length. Should be {columns}.");
                    if (columnIterator.Current is T[] arrayColumn)
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
                                if (!rowIterator.MoveNext()) throw new ArgumentOutOfRangeException(nameof(data), $"The given array has the wrong length. Should be {rows}.");
                                array[index] = rowIterator.Current;
                            }
                            if (rowIterator.MoveNext()) throw new ArgumentOutOfRangeException(nameof(data), $"The given array has the wrong length. Should be {rows}.");
                        }
                    }
                }
                if (columnIterator.MoveNext()) throw new ArgumentOutOfRangeException(nameof(data), $"The given array has the wrong length. Should be {columns}.");
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
                    if (!rowIterator.MoveNext()) throw new ArgumentOutOfRangeException(nameof(data), $"The given array has the wrong length. Should be {rows}.");
                    using (var columnIterator = rowIterator.Current.GetEnumerator())
                    {
                        for (int index = row; index < array.Length; index += rows)
                        {
                            if (!columnIterator.MoveNext()) throw new ArgumentOutOfRangeException(nameof(data), $"The given array has the wrong length. Should be {columns}.");
                            array[index] = columnIterator.Current;
                        }
                        if (columnIterator.MoveNext()) throw new ArgumentOutOfRangeException(nameof(data), $"The given array has the wrong length. Should be {columns}.");
                    }
                }
                if (rowIterator.MoveNext()) throw new ArgumentOutOfRangeException(nameof(data), $"The given array has the wrong length. Should be {rows}.");
            }
            return new DenseColumnMajorMatrixStorage<T>(rows, columns, array);
        }

        // MATRIX COPY

        internal override void CopyToUnchecked(MatrixStorage<T> target, ExistingData existingData)
        {
            if (target is DenseColumnMajorMatrixStorage<T> denseTarget)
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
            ExistingData existingData)
        {
            if (target is DenseColumnMajorMatrixStorage<T> denseTarget)
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
            ExistingData existingData)
        {
            if (target is DenseVectorStorage<T> targetDense)
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
            ExistingData existingData)
        {
            if (target is DenseVectorStorage<T> targetDense)
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

        internal override void TransposeToUnchecked(MatrixStorage<T> target, ExistingData existingData)
        {
            if (target is DenseColumnMajorMatrixStorage<T> denseTarget)
            {
                TransposeToUnchecked(denseTarget);
                return;
            }

            if (target is SparseCompressedRowMatrixStorage<T> sparseTarget)
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

        internal override void TransposeSquareInplaceUnchecked()
        {
            for (var j = 0; j < ColumnCount; j++)
            {
                var index = j * RowCount;
                for (var i = 0; i < j; i++)
                {
                    T swap = Data[index + i];
                    Data[index + i] = Data[i*ColumnCount + j];
                    Data[i*ColumnCount + j] = swap;
                }
            }
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
            Array.Copy(Data, 0, ret, 0, Data.Length);
            return ret;
        }

        public override T[][] ToRowArrays()
        {
            var ret = new T[RowCount][];
            CommonParallel.For(0, RowCount, Math.Max(4096/ColumnCount, 32), (a, b) =>
            {
                for (int i = a; i < b; i++)
                {
                    var row = new T[ColumnCount];
                    for (int j = 0; j < ColumnCount; j++)
                    {
                        row[j] = Data[j*RowCount + i];
                    }
                    ret[i] = row;
                }
            });
            return ret;
        }

        public override T[][] ToColumnArrays()
        {
            var ret = new T[ColumnCount][];
            CommonParallel.For(0, ColumnCount, Math.Max(4096/RowCount, 32), (a, b) =>
            {
                for (int j = a; j < b; j++)
                {
                    var column = new T[RowCount];
                    Array.Copy(Data, j*RowCount, column, 0, RowCount);
                    ret[j] = column;
                }
            });
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

        public override T[] AsColumnMajorArray()
        {
            return Data;
        }

        // ENUMERATION

        public override IEnumerable<T> Enumerate()
        {
            return Data;
        }

        public override IEnumerable<(int, int, T)> EnumerateIndexed()
        {
            int index = 0;
            for (int j = 0; j < ColumnCount; j++)
            {
                for (int i = 0; i < RowCount; i++)
                {
                    yield return (i, j, Data[index]);
                    index++;
                }
            }
        }

        public override IEnumerable<T> EnumerateNonZero()
        {
            return Data.Where(x => !Zero.Equals(x));
        }

        public override IEnumerable<(int, int, T)> EnumerateNonZeroIndexed()
        {
            int index = 0;
            for (int j = 0; j < ColumnCount; j++)
            {
                for (int i = 0; i < RowCount; i++)
                {
                    var x = Data[index];
                    if (!Zero.Equals(x))
                    {
                        yield return (i, j, x);
                    }
                    index++;
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
                    RowColumnAtIndex(i, out int row, out int column);
                    return new Tuple<int, int, T>(row, column, Data[i]);
                }
            }
            return null;
        }

        internal override Tuple<int, int, T, TOther> Find2Unchecked<TOther>(MatrixStorage<TOther> other, Func<T, TOther, bool> predicate, Zeros zeros)
        {
            if (other is DenseColumnMajorMatrixStorage<TOther> denseOther)
            {
                TOther[] otherData = denseOther.Data;
                for (int i = 0; i < Data.Length; i++)
                {
                    if (predicate(Data[i], otherData[i]))
                    {
                        RowColumnAtIndex(i, out int row, out int column);
                        return new Tuple<int, int, T, TOther>(row, column, Data[i], otherData[i]);

                    }
                }
                return null;
            }

            if (other is DiagonalMatrixStorage<TOther> diagonalOther)
            {
                TOther[] otherData = diagonalOther.Data;
                TOther otherZero = BuilderInstance<TOther>.Matrix.Zero;
                int k = 0;
                for (int j = 0; j < ColumnCount; j++)
                {
                    for (int i = 0; i < RowCount; i++)
                    {
                        if (predicate(Data[k], i == j ? otherData[i] : otherZero))
                        {
                            return new Tuple<int, int, T, TOther>(i, j, Data[k], i == j ? otherData[i] : otherZero);
                        }
                        k++;
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
                int k = 0;
                for (int row = 0; row < RowCount; row++)
                {
                    for (int col = 0; col < ColumnCount; col++)
                    {
                        if (k < otherRowPointers[row + 1] && otherColumnIndices[k] == col)
                        {
                            if (predicate(Data[col*RowCount + row], otherValues[k]))
                            {
                                return new Tuple<int, int, T, TOther>(row, col, Data[col*RowCount + row], otherValues[k]);
                            }
                            k++;
                        }
                        else
                        {
                            if (predicate(Data[col*RowCount + row], otherZero))
                            {
                                return new Tuple<int, int, T, TOther>(row, col, Data[col*RowCount + row], otherValues[k]);
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
            Zeros zeros, ExistingData existingData)
        {
            if (target is DenseColumnMajorMatrixStorage<TU> denseTarget)
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
            Zeros zeros, ExistingData existingData)
        {
            if (target is DenseColumnMajorMatrixStorage<TU> denseTarget)
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
            Zeros zeros, ExistingData existingData)
        {
            if (target is DenseColumnMajorMatrixStorage<TU> denseTarget)
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

        internal override void FoldByRowUnchecked<TU>(TU[] target, Func<TU, T, TU> f, Func<TU, int, TU> finalize, TU[] state, Zeros zeros)
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

        internal override void FoldByColumnUnchecked<TU>(TU[] target, Func<TU, T, TU> f, Func<TU, int, TU> finalize, TU[] state, Zeros zeros)
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

        internal override TState Fold2Unchecked<TOther, TState>(MatrixStorage<TOther> other, Func<TState, T, TOther, TState> f, TState state, Zeros zeros)
        {
            if (other is DenseColumnMajorMatrixStorage<TOther> denseOther)
            {
                TOther[] otherData = denseOther.Data;
                for (int i = 0; i < Data.Length; i++)
                {
                    state = f(state, Data[i], otherData[i]);
                }
                return state;
            }

            if (other is DiagonalMatrixStorage<TOther> diagonalOther)
            {
                TOther[] otherData = diagonalOther.Data;
                TOther otherZero = BuilderInstance<TOther>.Matrix.Zero;
                int k = 0;
                for (int j = 0; j < ColumnCount; j++)
                {
                    for (int i = 0; i < RowCount; i++)
                    {
                        state = f(state, Data[k], i == j ? otherData[i] : otherZero);
                        k++;
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
                int k = 0;
                for (int row = 0; row < RowCount; row++)
                {
                    for (int col = 0; col < ColumnCount; col++)
                    {
                        if (k < otherRowPointers[row + 1] && otherColumnIndices[k] == col)
                        {
                            state = f(state, Data[col*RowCount + row], otherValues[k++]);
                        }
                        else
                        {
                            state = f(state, Data[col*RowCount + row], otherZero);
                        }
                    }
                }
                return state;
            }

            // FALL BACK

            return base.Fold2Unchecked(other, f, state, zeros);
        }
    }
}
