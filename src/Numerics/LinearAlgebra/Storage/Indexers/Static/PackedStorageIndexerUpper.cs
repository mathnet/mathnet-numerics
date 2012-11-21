namespace MathNet.Numerics.LinearAlgebra.Storage.Indexers.Static
{
    using System;

    /// <summary>
    ///  A class for managing indexing when using Packed Storage scheme, which is a column-Wise packing scheme for Symmetric, Hermitian or Triangular square matrices. 
    ///  This variation provides indexes for storing the upper triangle of a matrix (row less than or equal to column).
    /// </summary>
    public class PackedStorageIndexerUpper : PackedStorageIndexer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PackedStorageIndexerUpper"/> class.
        /// </summary>
        /// <param name="order">
        /// The order of the matrix.
        /// </param>
        internal PackedStorageIndexerUpper(int order)
            : base(order)
        {
        }

        /// <summary>
        ///   Gets the index of the given element.
        /// </summary>
        /// <param name = "row">
        ///   The row of the element.
        /// </param>
        /// <param name = "column">
        ///   The column of the element.
        /// </param>
        /// <remarks>
        ///   This method is parameter checked. <see cref = "IndexOf(int,int)" /> and <see cref = "IndexOfDiagonal(int)" /> to get values without parameter checking.
        /// </remarks>
        public override int this[int row, int column]
        {
            get
            {
                if (row < 0 || row >= Order)
                {
                    throw new ArgumentOutOfRangeException("row");
                }

                if (column < 0 || column >= Order)
                {
                    throw new ArgumentOutOfRangeException("column");
                }

                if (row > column)
                {
                    throw new ArgumentException("Row must be less than or equal to column");
                }

                return IndexOf(row, column);
            }
        }

        /// <summary>
        /// Retrieves the index of the requested element without parameter checking. Row must be less than or equal to column.
        /// </summary>
        /// <param name="row">
        /// The row of the element. 
        /// </param>
        /// <param name="column">
        /// The column of the element. 
        /// </param>
        /// <returns>
        /// The requested index. 
        /// </returns>
        public override int IndexOf(int row, int column)
        {
            return row + ((column * (column + 1)) / 2);
        }

        /// <summary>
        /// Retrieves the index of the requested diagonal element without parameter checking.
        /// </summary>
        /// <param name="row">
        /// The row=column of the diagonal element. 
        /// </param>
        /// <returns>
        /// The requested index. 
        /// </returns>
        public override int IndexOfDiagonal(int row)
        {
            return (row * (row + 3)) / 2;
        }
    }
}
