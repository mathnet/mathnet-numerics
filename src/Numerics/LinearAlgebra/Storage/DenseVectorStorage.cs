// <copyright file="DenseVectorStorage.cs" company="Math.NET">
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
    public class DenseVectorStorage<T> : VectorStorage<T>
        where T : struct, IEquatable<T>, IFormattable
    {
        // [ruegg] public fields are OK here

        public readonly T[] Data;

        internal DenseVectorStorage(int length)
            : base(length)
        {
            Data = new T[length];
        }

        internal DenseVectorStorage(int length, T[] data)
            : base(length)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            if (data.Length != length)
            {
                throw new ArgumentOutOfRangeException("data", string.Format(Resources.ArgumentArrayWrongLength, length));
            }

            Data = data;
        }

        /// <summary>
        /// Retrieves the requested element without range checking.
        /// </summary>
        public override T At(int index)
        {
            return Data[index];
        }

        /// <summary>
        /// Sets the element without range checking.
        /// </summary>
        public override void At(int index, T value)
        {
            Data[index] = value;
        }

        public override void Clear()
        {
            Array.Clear(Data, 0, Data.Length);
        }

        public override void Clear(int index, int count)
        {
            Array.Clear(Data, index, count);
        }

        // VECTOR COPY

        internal override void CopyToUnchecked(VectorStorage<T> target, bool skipClearing = false)
        {
            var denseTarget = target as DenseVectorStorage<T>;
            if (denseTarget != null)
            {
                if (!ReferenceEquals(this, denseTarget))
                {
                    Array.Copy(Data, 0, denseTarget.Data, 0, Data.Length);
                }
                return;
            }

            // FALL BACK

            for (int i = 0; i < Data.Length; i++)
            {
                target.At(i, Data[i]);
            }
        }

        // ROW COPY

        internal override void CopyToRowUnchecked(MatrixStorage<T> target, int rowIndex, bool skipClearing = false)
        {
            var denseTarget = target as DenseColumnMajorMatrixStorage<T>;
            if (denseTarget != null)
            {
                for (int j = 0; j < Data.Length; j++)
                {
                    denseTarget.Data[j*target.RowCount + rowIndex] = Data[j];
                }
                return;
            }

            // FALL BACK

            for (int j = 0; j < Length; j++)
            {
                target.At(rowIndex, j, Data[j]);
            }
        }

        // COLUMN COPY

        internal override void CopyToColumnUnchecked(MatrixStorage<T> target, int columnIndex, bool skipClearing = false)
        {
            var denseTarget = target as DenseColumnMajorMatrixStorage<T>;
            if (denseTarget != null)
            {
                Array.Copy(Data, 0, denseTarget.Data, columnIndex * denseTarget.RowCount, Data.Length);
                return;
            }

            // FALL BACK

            for (int i = 0; i < Length; i++)
            {
                target.At(i, columnIndex, Data[i]);
            }
        }

        // SUB-VECTOR COPY

        internal override void CopySubVectorToUnchecked(VectorStorage<T> target,
            int sourceIndex, int targetIndex, int count,
            bool skipClearing = false)
        {
            var denseTarget = target as DenseVectorStorage<T>;
            if (denseTarget != null)
            {
                Array.Copy(Data, sourceIndex, denseTarget.Data, targetIndex, count);
                return;
            }

            // FALL BACK

            base.CopySubVectorToUnchecked(target, sourceIndex, targetIndex, count, skipClearing);
        }

        // SUB-ROW COPY

        internal override void CopyToSubRowUnchecked(MatrixStorage<T> target, int rowIndex,
            int sourceColumnIndex, int targetColumnIndex, int columnCount,
            bool skipClearing = false)
        {
            var denseTarget = target as DenseColumnMajorMatrixStorage<T>;
            if (denseTarget != null)
            {
                for (int j = 0; j < Data.Length; j++)
                {
                    denseTarget.Data[(j + targetColumnIndex) * target.RowCount + rowIndex] = Data[j + sourceColumnIndex];
                }
                return;
            }

            // FALL BACK

            for (int j = sourceColumnIndex, jj = targetColumnIndex; j < sourceColumnIndex + columnCount; j++, jj++)
            {
                target.At(rowIndex, jj, Data[j]);
            }
        }

        // SUB-COLUMN COPY

        internal override void CopyToSubColumnUnchecked(MatrixStorage<T> target, int columnIndex,
            int sourceRowIndex, int targetRowIndex, int rowCount,
            bool skipClearing = false)
        {
            var denseTarget = target as DenseColumnMajorMatrixStorage<T>;
            if (denseTarget != null)
            {
                Array.Copy(Data, sourceRowIndex, denseTarget.Data, columnIndex*denseTarget.RowCount + targetRowIndex, rowCount);
                return;
            }

            // FALL BACK

            for (int i = sourceRowIndex, ii = targetRowIndex; i < sourceRowIndex + rowCount; i++, ii++)
            {
                target.At(ii, columnIndex, Data[i]);
            }
        }
    }
}
