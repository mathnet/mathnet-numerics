namespace MathNet.Numerics.LinearAlgebra.Generic
{
    using System;

    using MathNet.Numerics.LinearAlgebra.Storage;

    using Properties;

    /// <summary>
    /// Abstract class for square matrices. 
    /// </summary>
    /// <typeparam name="T">Supported data types are <c>double</c>, <c>single</c>, <see cref="Complex"/>, and <see cref="Complex32"/>.</typeparam>
    [Serializable]
    public abstract class SquareMatrix<T> : Matrix<T>
         where T : struct, IEquatable<T>, IFormattable
    {
        /// <summary>
        ///   Number of rows or columns.
        /// </summary>
        protected readonly int Order;

        /// <summary>
        /// Initializes a new instance of the <see cref="SquareMatrix{T}"/> class. 
        /// </summary>
        /// <exception cref="ArgumentException">
        /// If the matrix is not square.
        /// </exception>
        protected SquareMatrix(MatrixStorage<T> storage)
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
