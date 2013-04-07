// <copyright file="DenseColumnMajorMatrixStorage.cs" company="Math.NET">
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
using MathNet.Numerics.Properties;

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
            Data = new T[rows * columns];
        }

        internal DenseColumnMajorMatrixStorage(int rows, int columns, T[] data)
            : base(rows, columns)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            if (data.Length != rows * columns)
            {
                throw new ArgumentOutOfRangeException("data", string.Format(Resources.ArgumentArrayWrongLength, rows * columns));
            }

            Data = data;
        }

        /// <summary>
        /// Retrieves the requested element without range checking.
        /// </summary>
        public override T At(int row, int column)
        {
            return Data[(column * RowCount) + row];
        }

        /// <summary>
        /// Sets the element without range checking.
        /// </summary>
        public override void At(int row, int column, T value)
        {
            Data[(column * RowCount) + row] = value;
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

        // INITIALIZATION

        public static DenseColumnMajorMatrixStorage<T> OfMatrix(MatrixStorage<T> matrix)
        {
            var storage = new DenseColumnMajorMatrixStorage<T>(matrix.RowCount, matrix.ColumnCount);
            matrix.CopyToUnchecked(storage, skipClearing: true);
            return storage;
        }

        public static DenseColumnMajorMatrixStorage<T> OfColumnMajorEnumerable(int rows, int columns, IEnumerable<T> data)
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
                return new DenseColumnMajorMatrixStorage<T>(rows, columns, copy);
            }

            var array = System.Linq.Enumerable.ToArray(data);
            return new DenseColumnMajorMatrixStorage<T>(rows, columns, array);
        }

        // MATRIX COPY

        internal override void CopyToUnchecked(MatrixStorage<T> target, bool skipClearing = false)
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
            bool skipClearing = false)
        {
            var denseTarget = target as DenseColumnMajorMatrixStorage<T>;
            if (denseTarget != null)
            {
                CopySubMatrixToUnchecked(denseTarget, sourceRowIndex, targetRowIndex, rowCount, sourceColumnIndex, targetColumnIndex, columnCount);
                return;
            }

            // FALL BACK

            base.CopySubMatrixToUnchecked(target, sourceRowIndex, targetRowIndex, rowCount, sourceColumnIndex, targetColumnIndex, columnCount, skipClearing);
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

        internal override void CopySubRowToUnchecked(VectorStorage<T> target, int rowIndex, int sourceColumnIndex, int targetColumnIndex, int columnCount, bool skipClearing = false)
        {
            var targetDense = target as DenseVectorStorage<T>;
            if (targetDense != null)
            {
                for (int j = 0; j<columnCount; j++)
                {
                    targetDense.Data[j + targetColumnIndex] = Data[(j + sourceColumnIndex) * RowCount + rowIndex];
                }
                return;
            }

            // FALL BACK

            for (int j = sourceColumnIndex, jj = targetColumnIndex; j < sourceColumnIndex + columnCount; j++, jj++)
            {
                target.At(jj, Data[(j * RowCount) + rowIndex]);
            }
        }

        // COLUMN COPY

        internal override void CopySubColumnToUnchecked(VectorStorage<T> target, int columnIndex, int sourceRowIndex, int targetRowIndex, int rowCount, bool skipClearing = false)
        {
            var targetDense = target as DenseVectorStorage<T>;
            if (targetDense != null)
            {
                Array.Copy(Data, columnIndex*RowCount + sourceRowIndex, targetDense.Data, targetRowIndex, rowCount);
                return;
            }

            // FALL BACK

            var offset = columnIndex * RowCount;
            for (int i = sourceRowIndex, ii = targetRowIndex; i < sourceRowIndex + rowCount; i++, ii++)
            {
                target.At(ii, Data[offset + i]);
            }
        }

        // EXTRACT

        public override T[] ToRowMajorArray()
        {
            var ret = new T[Data.Length];
            for (int i = 0; i < RowCount; i++)
            {
                var offset = i * ColumnCount;
                for (int j = 0; j < ColumnCount; j++)
                {
                    ret[offset + j] = Data[(j * RowCount) + i];
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
                    ret[i, j] = Data[(j * RowCount) + i];
                }
            }
            return ret;
        }
    }
}
