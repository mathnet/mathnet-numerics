using System;
using System.Security;
using MathNet.Numerics.Providers.SparseSolver;

namespace MathNet.Numerics.Providers.MKL.SparseSolver
{
    /// <summary>
    /// Intel's Math Kernel Library (MKL) direct sparse solver provider.
    /// </summary>
    internal partial class MklSparseSolverProvider
    {
        /// <summary>
        /// Solves sparse linear systems of equations, <b>AX = B</b>.
        /// </summary>
        /// <param name="matrixStructure">The symmetricity of the matrix. For a symmetric matrix, only upper ot lower triangular matrix is used.</param>
        /// <param name="matrixType">The definiteness of the matrix.</param>
        /// <param name="systemType">The type of the systems.</param>
        /// <param name="rowCount">The number of rows of matrix.</param>
        /// <param name="columnCount">The number of columns of matrix.</param>
        /// <param name="nonZerosCount">The number of non zero elements of matrix.</param>
        /// <param name="rowPointers">The array containing the row indices of the existing rows.</param>
        /// <param name="columnIndices">The array containing the column indices of the non-zero values</param>
        /// <param name="values">The array that contains the non-zero elements of matrix. No diagonal element can be ommitted. </param>
        /// <param name="nRhs">The number of columns of the right hand side matrix.</param>
        /// <param name="rhs">The right hand side matrix</param>
        /// <param name="solution">The left hand side matrix</param>
        /// <returns>The status of the solver.</returns>
        [SecuritySafeCritical]
        public DssStatus Solve(DssMatrixStructure matrixStructure, DssMatrixType matrixType, DssSystemType systemType,
            int rowCount, int columnCount, int nonZerosCount, int[] rowPointers, int[] columnIndices, float[] values,
            int nRhs, float[] rhs, float[] solution)
        {
            if (rowCount != columnCount)
            {
                throw new ArgumentException("Matrix must be symmetric.");
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
                throw new ArgumentException("The array arguments must have the same length.", nameof(rhs));
            }

            if (columnCount * nRhs != solution.Length)
            {
                throw new ArgumentException("The array arguments must have the same length.", nameof(solution));
            }

            var error = SafeNativeMethods.s_dss_solve((int)matrixStructure, (int)matrixType, (int)systemType,
                rowCount, columnCount, nonZerosCount, rowPointers, columnIndices, values,
                nRhs, rhs, solution);
            return (DssStatus)error;
        }
    }
}
