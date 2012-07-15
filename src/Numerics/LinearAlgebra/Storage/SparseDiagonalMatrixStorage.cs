using System;
using MathNet.Numerics.Properties;

namespace MathNet.Numerics.LinearAlgebra.Storage
{
    internal class SparseDiagonalMatrixStorage<T>
        where T : struct, IEquatable<T>, IFormattable
    {
        // [ruegg] public fields are OK here

        public readonly int RowCount;
        public readonly int ColumnCount;
        readonly T _zero;

        public readonly T[] Data;

        internal SparseDiagonalMatrixStorage(int rows, int columns, T zero)
        {
            RowCount = rows;
            ColumnCount = columns;
            _zero = zero;

            Data = new T[Math.Min(rows, columns)];
        }

        internal SparseDiagonalMatrixStorage(int rows, int columns, T zero, T[] data)
        {
            RowCount = rows;
            ColumnCount = columns;
            _zero = zero;

            Data = data;
        }

        public T this[int row, int column]
        {
            get
            {
                return row == column ? Data[row] : _zero;
            }
            set
            {
                if (row == column)
                {
                    Data[row] = value;
                }
                else if (!_zero.Equals(value))
                {
                    throw new IndexOutOfRangeException("Cannot set an off-diagonal element in a diagonal matrix.");
                }
            }
        }

        public void Clear()
        {
            Array.Clear(Data, 0, Data.Length);
        }

        public void CopyTo(SparseDiagonalMatrixStorage<T> target)
        {
            if (ReferenceEquals(this, target))
            {
                return;
            }

            if (target == null)
            {
                throw new ArgumentNullException("target");
            }

            if (RowCount != target.RowCount || ColumnCount != target.ColumnCount)
            {
                var message = string.Format(Resources.ArgumentMatrixDimensions2, RowCount + "x" + ColumnCount, target.RowCount + "x" + target.ColumnCount);
                throw new ArgumentException(message, "target");
            }

            //Buffer.BlockCopy(Data, 0, target.Data, 0, Data.Length * System.Runtime.InteropServices.Marshal.SizeOf(typeof(T)));
            Array.Copy(Data, 0, target.Data, 0, Data.Length);
        }

        public void CopyTo(DenseColumnMajorMatrixStorage<T> target, bool targetKnownClear = false)
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

            if (!targetKnownClear)
            {
                target.Clear();
            }

            for (int i = 0; i < Data.Length; i++)
            {
                target.Data[i*(target.RowCount + 1)] = Data[i];
            }
        }
    }
}
