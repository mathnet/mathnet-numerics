namespace MathNet.Numerics.LinearAlgebra.Storage.Indexers
{
    /// <summary>
    ///  Abstract class that defines common features for all storage schemes. 
    /// </summary>
    public interface IStorageIndexer
    {
        /// <summary>
        ///  Retrieves the index of the requested element without parameter checking.
        ///  </summary><param name="row">
        ///  The row of the element. 
        ///  </param><param name="column">
        ///  The column of the element. 
        ///  </param><returns>
        ///  The requested index. 
        /// </returns>
        int Of(int row, int column);

        /// <summary>
        ///  Retrieves the index of the requested diagonal element without parameter checking.
        ///  </summary><param name="row">
        ///  The row=column of the diagonal element. 
        ///  </param><returns>
        ///  The requested index. 
        /// </returns>
        int OfDiagonal(int row);
    }
}
