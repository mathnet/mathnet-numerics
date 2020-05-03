using Complex = System.Numerics.Complex;

namespace MathNet.Numerics.Providers.SparseSolver
{
    /// <summary>
    /// Structure option.
    /// </summary>
    public enum DssMatrixStructure : int
    {
        Symmetric = 536870976,
        SymmetricStructure = 536871040,
        Nonsymmetric = 536871104,
        SymmetricComplex = 536871168,
        SymmetricStructureComplex = 536871232,
        NonsymmetricComplex = 536871296,
    }

    /// <summary>
    /// Factorization option.
    /// </summary>
    public enum DssMatrixType : int
    {
        PositiveDefinite = 134217792,
        Indefinite = 134217856,
        HermitianPositiveDefinite = 134217920,
        HermitianIndefinite = 134217984
    }

    /// <summary>
    /// Solver step's substitution.
    /// </summary>
    public enum DssSystemType : int
    {
        /// <summary>
        /// Solve a system, Ax = b.
        /// </summary>
        DontTranspose = 0,
        /// <summary>
        /// Solve a transposed system, A'x = b
        /// </summary>
        Transpose = 262144,
        /// <summary>
        /// Solve a conjugate transposed system, A†x = b
        /// </summary>
        ConjugateTranspose = 524288,
    }

    /// <summary>
    /// Status values
    /// </summary>
    public enum DssStatus : int
    {
        /// <summary>
        /// The operation was successful.
        /// </summary>
        MKL_DSS_SUCCESS = 0, 
        MKL_DSS_ZERO_PIVOT = -1,
        MKL_DSS_OUT_OF_MEMORY = -2,
        MKL_DSS_FAILURE = -3,
        MKL_DSS_ROW_ERR = -4,
        MKL_DSS_COL_ERR = -5,
        MKL_DSS_TOO_FEW_VALUES = -6,
        MKL_DSS_TOO_MANY_VALUES = -7,
        MKL_DSS_NOT_SQUARE = -8,
        MKL_DSS_STATE_ERR = -9,
        MKL_DSS_INVALID_OPTION = -10,
        MKL_DSS_OPTION_CONFLICT = -11,
        MKL_DSS_MSG_LVL_ERR = -12,
        MKL_DSS_TERM_LVL_ERR = -13,
        MKL_DSS_STRUCTURE_ERR = -14,
        MKL_DSS_REORDER_ERR = -15,
        MKL_DSS_VALUES_ERR = -16,
        MKL_DSS_STATISTICS_INVALID_MATRIX = -17,
        MKL_DSS_STATISTICS_INVALID_STATE = -18,
        MKL_DSS_STATISTICS_INVALID_STRING = -19,
        MKL_DSS_REORDER1_ERR = -20,
        MKL_DSS_PREORDER_ERR = -21,
        MKL_DSS_DIAG_ERR = -22,
        MKL_DSS_I32BIT_ERR = -23,
        MKL_DSS_OOC_MEM_ERR = -24,
        MKL_DSS_OOC_OC_ERR = -25,
        MKL_DSS_OOC_RW_ERR = -26,
    }

    public interface ISparseSolverProvider :
        ISparseSolverProvider<double>,
        ISparseSolverProvider<float>,
        ISparseSolverProvider<Complex>,
        ISparseSolverProvider<Complex32>
    {
        /// <summary>
        /// Try to find out whether the provider is available, at least in principle.
        /// Verification may still fail if available, but it will certainly fail if unavailable.
        /// </summary>
        bool IsAvailable();

        /// <summary>
        /// Initialize and verify that the provided is indeed available. If not, fall back to alternatives like the managed provider
        /// </summary>
        void InitializeVerify();

        /// <summary>
        /// Frees memory buffers, caches and handles allocated in or to the provider.
        /// Does not unload the provider itself, it is still usable afterwards.
        /// </summary>
        void FreeResources();
    }

    public interface ISparseSolverProvider<T>
        where T : struct
    {
        DssStatus Solve(DssMatrixStructure matrixStructure, DssMatrixType matrixType, DssSystemType systemType, int rows, int cols, int nnz, int[] rowIdx, int[] colPtr, T[] values, int nRhs, T[] rhs, T[] solution);
    }
}

