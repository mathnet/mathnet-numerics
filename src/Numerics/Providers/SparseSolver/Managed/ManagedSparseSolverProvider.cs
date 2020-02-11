using System;
using System.Numerics;

namespace MathNet.Numerics.Providers.SparseSolver.Managed
{
    /// <summary>
    /// The managed sparse solver provider
    /// </summary>
    internal partial class ManagedSparseSolverProvider : ISparseSolverProvider
    {
        /// <summary>
        /// Try to find out whether the provider is available, at least in principle.
        /// Verification may still fail if available, but it will certainly fail if unavailable.
        /// </summary>
        public virtual bool IsAvailable()
        {
            return true;
        }

        /// <summary>
        /// Initialize and verify that the provided is indeed available. If not, fall back to alternatives like the managed provider
        /// </summary>
        public virtual void InitializeVerify()
        {
        }

        /// <summary>
        /// Frees memory buffers, caches and handles allocated in or to the provider.
        /// Does not unload the provider itself, it is still usable afterwards.
        /// </summary>
        public virtual void FreeResources()
        {
        }

        public override string ToString()
        {
            return "Managed";
        }

        public virtual DssStatus Solve(DssMatrixStructure matrixStructure, DssMatrixType matrixType, DssSystemType systemType,
            int rowCount, int columnCount, int nonZerosCount, int[] rowPointers, int[] ColumnIndices, float[] values,
            int nRhs, float[] rhs, float[] solution)
        {
            throw new NotImplementedException();
        }

        public virtual DssStatus Solve(DssMatrixStructure matrixStructure, DssMatrixType matrixType, DssSystemType systemType,
            int rowCount, int columnCount, int nonZerosCount, int[] rowPointers, int[] ColumnIndices, double[] values,
            int nRhs, double[] rhs, double[] solution)
        {
            throw new NotImplementedException();
        }

        public virtual DssStatus Solve(DssMatrixStructure matrixStructure, DssMatrixType matrixType, DssSystemType systemType,
            int rowCount, int columnCount, int nonZerosCount, int[] rowPointers, int[] ColumnIndices, Complex32[] values,
            int nRhs, Complex32[] rhs, Complex32[] solution)
        {
            throw new NotImplementedException();
        }

        public virtual DssStatus Solve(DssMatrixStructure matrixStructure, DssMatrixType matrixType, DssSystemType systemType,
            int rowCount, int columnCount, int nonZerosCount, int[] rowPointers, int[] ColumnIndices, Complex[] values,
            int nRhs, Complex[] rhs, Complex[] solution)
        {
            throw new NotImplementedException();
        }
    }
}
