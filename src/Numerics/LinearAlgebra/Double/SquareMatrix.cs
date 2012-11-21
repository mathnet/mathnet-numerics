namespace MathNet.Numerics.LinearAlgebra.Double
{
    using System;

    using MathNet.Numerics.LinearAlgebra.Storage;
    using MathNet.Numerics.Properties;

    /// <summary>
    /// Abstract class for square matrices. 
    /// </summary>
    [Serializable]
    public abstract class SquareMatrix : Matrix
    {
        /// <summary>
        ///   Number of rows or columns.
        /// </summary>
        protected readonly int Order;

        /// <summary>
        /// Initializes a new instance of the <see cref="SquareMatrix"/> class. 
        /// </summary>
        /// <exception cref="ArgumentException">
        /// If the matrix is not square.
        /// </exception>
        protected SquareMatrix(MatrixStorage<double> storage)
            : base(storage)
        {
            if (storage.RowCount != storage.ColumnCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixSquare);
            }

            Order = storage.RowCount;
        }
    }
}
