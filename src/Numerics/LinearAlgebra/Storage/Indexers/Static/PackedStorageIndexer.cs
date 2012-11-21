namespace MathNet.Numerics.LinearAlgebra.Storage.Indexers.Static
{
    using System;
    using Properties;

    /// <summary>
    /// A class for managing indexing when using Packed Storage, which is a column-major packing scheme for dense Symmetric, Hermitian or Triangular square matrices.
    /// </summary>
    public abstract class PackedStorageIndexer : StaticStorageIndexer
    {
        /// <summary>
        ///   Number of rows or columns.
        /// </summary>
        protected readonly int Order;

        /// <summary>
        ///   Length of the stored data. 
        /// </summary>
        private readonly int _dataLength;

        /// <summary>
        /// Initializes a new instance of the <see cref="PackedStorageIndexer"/> class.
        /// </summary>
        /// <param name="order">
        /// The order of the matrix.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException"><c></c> is out of range.</exception>
        protected PackedStorageIndexer(int order)
        {
            if (order <= 0)
            {
                throw new ArgumentOutOfRangeException(Resources.MatrixRowsOrColumnsMustBePositive);
            }

            Order = order;
            _dataLength = order * (order + 1) / 2;
        }

        /// <summary>
        /// Gets the length of the stored data. 
        /// </summary>
        public override int DataLength
        {
            get
            {
                return _dataLength;
            }
        }
    }
}
