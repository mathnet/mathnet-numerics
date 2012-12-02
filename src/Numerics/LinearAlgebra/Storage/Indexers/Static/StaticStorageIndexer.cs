namespace MathNet.Numerics.LinearAlgebra.Storage.Indexers.Static
{
    /// <summary>
    ///   Classes that contain indexing information of a static storage scheme.
    /// </summary>
    /// <remarks>
    ///   A static storage scheme is always the same and only depends on the size of the matrix. 
    /// </remarks>
    public abstract class StaticStorageIndexer : IStorageIndexer
    {
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
        ///   This method is parameter checked. <see cref = "IStorageIndexer.Of" /> and <see cref = "IStorageIndexer.OfDiagonal" /> to get values without parameter checking.
        /// </remarks>
        public abstract int this[int row, int column]
        {
            get;
        }

        /// <summary>
        ///    Gets the length of the stored data.
        /// </summary>
        public abstract int DataLength
        {
            get;
        }

        /// <summary>
        ///  Retrieves the index of the requested element without parameter checking.
        ///  </summary><param name="row">
        ///  The row of the element. 
        ///  </param><param name="column">
        ///  The column of the element. 
        ///  </param><returns>
        ///  The requested index. 
        /// </returns>
        public abstract int Of(int row, int column);

        /// <summary>
        ///  Retrieves the index of the requested diagonal element without parameter checking.
        ///  </summary><param name="row">
        ///  The row=column of the diagonal element. 
        ///  </param><returns>
        ///  The requested index. 
        /// </returns>
        public abstract int OfDiagonal(int row);
    }
}
