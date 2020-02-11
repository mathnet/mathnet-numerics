#if NATIVE

using MathNet.Numerics.Properties;
using MathNet.Numerics.Providers.Common.Mkl;
using System;
using System.Security;

namespace MathNet.Numerics.Providers.SparseSolver.Mkl
{
    /// <summary>
    /// Intel's Math Kernel Library (MKL) direct sparse solver provider.
    /// </summary>
    internal partial class MklSparseSolverProvider
    {
        [SecuritySafeCritical]
        public override DssStatus Solve(DssMatrixStructure matrixStructure, DssMatrixType matrixType, DssSystemType systemType,
            int rowCount, int columnCount, int nonZerosCount, int[] rowPointers, int[] columnIndices, Complex32[] values,
            int nRhs, Complex32[] rhs, Complex32[] solution)
        {
            if (rowCount != columnCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixSymmetric);
            }

            if (rowPointers == null)
            {
                throw new ArgumentNullException(nameof(rowPointers));
            }

            if (columnIndices == null)
            {
                throw new ArgumentNullException(nameof(columnIndices));
            }

            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            if (rhs == null)
            {
                throw new ArgumentNullException(nameof(rhs));
            }

            if (solution == null)
            {
                throw new ArgumentNullException(nameof(solution));
            }

            if (rowCount * nRhs != rhs.Length)
            {
                throw new ArgumentException(Resources.ArgumentArraysSameLength, nameof(rhs));
            }

            if (columnCount * nRhs != solution.Length)
            {
                throw new ArgumentException(Resources.ArgumentArraysSameLength, nameof(solution));
            }

            var error = SafeNativeMethods.c_dss_solve((int)matrixStructure, (int)matrixType, (int)systemType,
                rowCount, columnCount, nonZerosCount, rowPointers, columnIndices, values,
                nRhs, rhs, solution);
            return (DssStatus)error;
        }
    }
}

#endif
