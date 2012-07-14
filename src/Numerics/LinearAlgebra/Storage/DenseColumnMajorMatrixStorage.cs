using System;
using MathNet.Numerics.Properties;

namespace MathNet.Numerics.LinearAlgebra.Storage
{
    internal class DenseColumnMajorMatrixStorage<T>
        where T : struct, IEquatable<T>, IFormattable
    {
        public int RowCount { get; private set; }
        public int ColumnCount { get; private set; }
        public T[] Data { get; private set; }

        internal DenseColumnMajorMatrixStorage(int rows, int columns)
        {
            RowCount = rows;
            ColumnCount = columns;
            Data = new T[rows * columns];
        }

        internal DenseColumnMajorMatrixStorage(int rows, int columns, T[] data)
        {
            RowCount = rows;
            ColumnCount = columns;
            Data = data;
        }

        public void CopyTo(DenseColumnMajorMatrixStorage<T> target)
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
    }
}
