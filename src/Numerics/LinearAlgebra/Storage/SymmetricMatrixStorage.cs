using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MathNet.Numerics.LinearAlgebra.Storage
{
    using MathNet.Numerics.Properties;

    public abstract class SymmetricMatrixStorage<T> : MatrixStorage<T> 
        where T : struct, IEquatable<T>, IFormattable
    {
        // [ruegg] public fields are OK here

        protected SymmetricMatrixStorage(int order)
         : base(order, order)
        {
        }

        public override bool IsFullyMutable
        {
            get { return false; }
        }

        public override bool IsMutable(int row, int column)
        {
            return row <= column;
        }

        public override void Clear()
        {
            for (var i = 0; i < RowCount; i++)
            {
                for (var j = i; j < ColumnCount; j++)
                {
                    At(i, j, default(T));
                }
            }
        }
         
        public override void Clear(int rowIndex, int rowCount, int columnIndex, int columnCount)
        {
            for (var i = rowIndex; i < rowIndex + rowCount; i++)
            {
                for (var j = Math.Max(columnIndex, i); j < columnIndex + columnCount; j++)
                {
                    At(i, j, default(T));
                }
            }
        }

        /// <remarks>Parameters assumed to be validated already.</remarks>
        public override void CopyTo(MatrixStorage<T> target, bool skipClearing = false)
        {
            for (int j = 0; j < ColumnCount; j++)
            {
                for (int i = 0; i <= j; i++)
                {
                    target.At(i, j, At(i, j));
                }
            }
        }
    }
}
