using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Storage;
using MathNet.Numerics.Properties;
using MathNet.Numerics.Providers.SparseSolver;
using System;

namespace MathNet.Numerics
{
    using Complex = System.Numerics.Complex;

    public static class Experimental
    {
        public static DssStatus Solve(DssMatrixStructure matrixStructure, DssMatrixType matrixType, DssSystemType systemType,
            int rowCount, int columnCount, int nonZerosCount, int[] rowPointers, int[] columnIndices, float[] values,
            int nRhs, float[] rhs, float[] solution)
        {
            return SparseSolverControl.Provider.Solve(matrixStructure, matrixType, systemType,
                rowCount, columnCount, nonZerosCount, rowPointers, columnIndices, values,
                nRhs, rhs, solution);
        }

        public static DssStatus Solve(DssMatrixStructure matrixStructure, DssMatrixType matrixType, DssSystemType systemType,
            int rowCount, int columnCount, int nonZerosCount, int[] rowPointers, int[] columnIndices, double[] values,
            int nRhs, double[] rhs, double[] solution)
        {
            return SparseSolverControl.Provider.Solve(matrixStructure, matrixType, systemType,
                rowCount, columnCount, nonZerosCount, rowPointers, columnIndices, values,
                nRhs, rhs, solution);
        }

        public static DssStatus Solve(DssMatrixStructure matrixStructure, DssMatrixType matrixType, DssSystemType systemType,
            int rowCount, int columnCount, int nonZerosCount, int[] rowPointers, int[] columnIndices, Complex32[] values,
            int nRhs, Complex32[] rhs, Complex32[] solution)
        {
            return SparseSolverControl.Provider.Solve(matrixStructure, matrixType, systemType,
                rowCount, columnCount, nonZerosCount, rowPointers, columnIndices, values,
                nRhs, rhs, solution);
        }

        public static DssStatus Solve(DssMatrixStructure matrixStructure, DssMatrixType matrixType, DssSystemType systemType,
            int rowCount, int columnCount, int nonZerosCount, int[] rowPointers, int[] columnIndices, Complex[] values,
            int nRhs, Complex[] rhs, Complex[] solution)
        {
            return SparseSolverControl.Provider.Solve(matrixStructure, matrixType, systemType,
                rowCount, columnCount, nonZerosCount, rowPointers, columnIndices, values,
                nRhs, rhs, solution);
        }

        // solve A x = b
        // The symmetricity or definiteness of A is not checked.

        public static DssStatus Solve(this Matrix<float> matrix, Vector<float> input, Vector<float> result)
        {
            if (matrix.RowCount != matrix.ColumnCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixSquare, nameof(matrix));
            }
            if (result.Count != input.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength);
            }
            if (input.Count != matrix.RowCount)
            {
                throw LinearAlgebra.Single.Matrix.DimensionsDontMatch<ArgumentException>(input, matrix);
            }
            var csr = matrix.Storage as SparseCompressedRowMatrixStorage<float>;
            if (csr == null)
            {
                throw new ArgumentException(Resources.MatrixMustBeSparse, nameof(matrix));
            }

            // No diagonal element can be omitted from the values array.
            // If there is a zero value on the diagonal, for example, that element nonetheless must be explicitly represented.
            csr.PopulateExplicitZerosOnDiagonal();

            var rowCount = csr.RowCount;
            var columnCount = csr.ColumnCount;
            var valueCount = csr.ValueCount;

            var values = csr.Values;
            var rowPointers = csr.RowPointers;
            var columnIndices = csr.ColumnIndices;

            var rhs = input.ToArray();
            var solution = new float[rowCount];

            var error = SparseSolverControl.Provider.Solve(DssMatrixStructure.Nonsymmetric, DssMatrixType.Indefinite, DssSystemType.NonTransposed,
                rowCount, columnCount, valueCount, rowPointers, columnIndices, values,
                1, rhs, solution);

            if (error == DssStatus.MKL_DSS_SUCCESS)
                result.SetValues(solution);

            return error;
        }

        public static DssStatus Solve(this Matrix<double> matrix, Vector<double> input, Vector<double> result)
        {
            if (matrix.RowCount != matrix.ColumnCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixSquare, nameof(matrix));
            }

            if (result.Count != input.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength);
            }

            if (input.Count != matrix.RowCount)
            {
                throw LinearAlgebra.Double.Matrix.DimensionsDontMatch<ArgumentException>(input, matrix);
            }

            var csr = matrix.Storage as SparseCompressedRowMatrixStorage<double>;
            if (csr == null)
            {
                throw new ArgumentException(Resources.MatrixMustBeSparse, nameof(matrix));
            }

            // No diagonal element can be omitted from the values array.
            // If there is a zero value on the diagonal, that element nonetheless must be explicitly represented.
            csr.PopulateExplicitZerosOnDiagonal();

            var rowCount = csr.RowCount;
            var columnCount = csr.ColumnCount;
            var valueCount = csr.ValueCount;

            var values = csr.Values; 
            var rowPointers = csr.RowPointers; 
            var columnIndices = csr.ColumnIndices;

            var rhs = input.ToArray();
            var solution = new double[rowCount];

            var error = SparseSolverControl.Provider.Solve(DssMatrixStructure.Nonsymmetric, DssMatrixType.Indefinite, DssSystemType.NonTransposed,
                rowCount, columnCount, valueCount, rowPointers, columnIndices, values,
                1, rhs, solution);

            if (error == DssStatus.MKL_DSS_SUCCESS)
                result.SetValues(solution);

            return error;
        }

        public static DssStatus Solve(this Matrix<Complex32> matrix, Vector<Complex32> input, Vector<Complex32> result)
        {
            if (matrix.RowCount != matrix.ColumnCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixSquare, nameof(matrix));
            }

            if (result.Count != input.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength);
            }

            if (input.Count != matrix.RowCount)
            {
                throw MathNet.Numerics.LinearAlgebra.Complex32.Matrix.DimensionsDontMatch<ArgumentException>(input, matrix);
            }

            var csr = matrix.Storage as SparseCompressedRowMatrixStorage<Complex32>;
            if (csr == null)
            {
                throw new ArgumentException(Resources.MatrixMustBeSparse, nameof(matrix));
            }

            // No diagonal element can be omitted from the values array.
            // If there is a zero value on the diagonal, that element nonetheless must be explicitly represented.
            csr.PopulateExplicitZerosOnDiagonal();

            var rowCount = csr.RowCount;
            var columnCount = csr.ColumnCount;
            var valueCount = csr.ValueCount;

            var values = csr.Values;
            var rowPointers = csr.RowPointers;
            var columnIndices = csr.ColumnIndices;

            var rhs = input.ToArray();
            var solution = new Complex32[rowCount];

            var error = SparseSolverControl.Provider.Solve(DssMatrixStructure.Nonsymmetric, DssMatrixType.Indefinite, DssSystemType.NonTransposed,
                rowCount, columnCount, valueCount, rowPointers, columnIndices, values,
                1, rhs, solution);

            if (error == DssStatus.MKL_DSS_SUCCESS)
                result.SetValues(solution);

            return error;
        }
        
        public static DssStatus Solve(this Matrix<Complex> matrix, Vector<Complex> input, Vector<Complex> result)
        {
            if (matrix.RowCount != matrix.ColumnCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixSquare, nameof(matrix));
            }

            if (result.Count != input.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength);
            }

            if (input.Count != matrix.RowCount)
            {
                throw LinearAlgebra.Complex.Matrix.DimensionsDontMatch<ArgumentException>(input, matrix);
            }

            var csr = matrix.Storage as SparseCompressedRowMatrixStorage<Complex>;
            if (csr == null)
            {
                throw new ArgumentException(Resources.MatrixMustBeSparse, nameof(matrix));
            }

            // No diagonal element can be omitted from the values array.
            // If there is a zero value on the diagonal, that element nonetheless must be explicitly represented.
            csr.PopulateExplicitZerosOnDiagonal();

            var rowCount = csr.RowCount;
            var columnCount = csr.ColumnCount;
            var valueCount = csr.ValueCount;

            var values = csr.Values;
            var rowPointers = csr.RowPointers;
            var columnIndices = csr.ColumnIndices;

            var rhs = input.ToArray();
            var solution = new Complex[rowCount];

            var error = SparseSolverControl.Provider.Solve(DssMatrixStructure.Nonsymmetric, DssMatrixType.Indefinite, DssSystemType.NonTransposed,
                rowCount, columnCount, valueCount, rowPointers, columnIndices, values,
                1, rhs, solution);

            if (error == DssStatus.MKL_DSS_SUCCESS)
                result.SetValues(solution);

            return error;
        }

        // Solve A X = B
        // The symmetricity or definiteness of A is not checked.

        public static DssStatus Solve(this Matrix<float> matrix, Matrix<float> input, Matrix<float> result)
        {
            if (matrix.RowCount != matrix.ColumnCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixSquare, nameof(matrix));
            }
            if (result.ColumnCount != input.ColumnCount || result.RowCount != input.RowCount)
            {
                throw LinearAlgebra.Single.Matrix.DimensionsDontMatch<ArgumentException>(input, result);
            }
            if (input.RowCount != matrix.RowCount)
            {
                throw LinearAlgebra.Single.Matrix.DimensionsDontMatch<ArgumentException>(input, matrix);
            }
            var csr = matrix.Storage as SparseCompressedRowMatrixStorage<float>;
            if (csr == null)
            {
                throw new ArgumentException(Resources.MatrixMustBeSparse, nameof(matrix));
            }

            // No diagonal element can be omitted from the values array.
            // If there is a zero value on the diagonal, for example, that element nonetheless must be explicitly represented.
            csr.PopulateExplicitZerosOnDiagonal();

            var rowCount = csr.RowCount;
            var columnCount = csr.ColumnCount;
            var valueCount = csr.ValueCount;

            var values = csr.Values;
            var rowPointers = csr.RowPointers;
            var columnIndices = csr.ColumnIndices;

            var nRhs = input.ColumnCount;
            var rhs = new float[rowCount * nRhs];
            Array.Copy(input.ToColumnMajorArray(), rhs, rhs.Length);

            var solution = new float[rowCount * nRhs];

            var error = SparseSolverControl.Provider.Solve(DssMatrixStructure.Nonsymmetric, DssMatrixType.Indefinite, DssSystemType.NonTransposed,
                rowCount, columnCount, valueCount, rowPointers, columnIndices, values,
                nRhs, rhs, solution);

            if (error == DssStatus.MKL_DSS_SUCCESS)
                result = Matrix<float>.Build.DenseOfColumnMajor(rowCount, nRhs, solution);

            return error;
        }

        public static DssStatus Solve(this Matrix<double> matrix, Matrix<double> input, Matrix<double> result)
        {
            if (matrix.RowCount != matrix.ColumnCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixSquare, nameof(matrix));
            }
            if (result.ColumnCount != input.ColumnCount || result.RowCount != input.RowCount)
            {
                throw LinearAlgebra.Double.Matrix.DimensionsDontMatch<ArgumentException>(input, result);
            }
            if (input.RowCount != matrix.RowCount)
            {
                throw LinearAlgebra.Double.Matrix.DimensionsDontMatch<ArgumentException>(input, matrix);
            }

            var csr = matrix.Storage as SparseCompressedRowMatrixStorage<double>;
            if (csr == null)
            {
                throw new ArgumentException(Resources.MatrixMustBeSparse, nameof(matrix));
            }

            // No diagonal element can be omitted from the values array.
            // If there is a zero value on the diagonal, that element nonetheless must be explicitly represented.
            csr.PopulateExplicitZerosOnDiagonal();

            var rowCount = csr.RowCount;
            var columnCount = csr.ColumnCount;
            var valueCount = csr.ValueCount;

            var values = csr.Values;
            var rowPointers = csr.RowPointers;
            var columnIndices = csr.ColumnIndices;

            var nRhs = input.ColumnCount;
            var rhs = new double[rowCount * nRhs];
            Array.Copy(input.ToColumnMajorArray(), rhs, rhs.Length);

            var solution = new double[rowCount * nRhs];

            var error = SparseSolverControl.Provider.Solve(DssMatrixStructure.Nonsymmetric, DssMatrixType.Indefinite, DssSystemType.NonTransposed,
                rowCount, columnCount, valueCount, rowPointers, columnIndices, values,
                nRhs, rhs, solution);

            if (error == DssStatus.MKL_DSS_SUCCESS)
                result = Matrix<double>.Build.DenseOfColumnMajor(rowCount, nRhs, solution);

            return error;
        }

        public static DssStatus Solve(this Matrix<Complex32> matrix, Matrix<Complex32> input, Matrix<Complex32> result)
        {
            if (matrix.RowCount != matrix.ColumnCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixSquare, nameof(matrix));
            }
            if (result.ColumnCount != input.ColumnCount || result.RowCount != input.RowCount)
            {
                throw LinearAlgebra.Complex32.Matrix.DimensionsDontMatch<ArgumentException>(input, result);
            }
            if (input.RowCount != matrix.RowCount)
            {
                throw LinearAlgebra.Complex32.Matrix.DimensionsDontMatch<ArgumentException>(input, matrix);
            }

            var csr = matrix.Storage as SparseCompressedRowMatrixStorage<Complex32>;
            if (csr == null)
            {
                throw new ArgumentException(Resources.MatrixMustBeSparse, nameof(matrix));
            }

            // No diagonal element can be omitted from the values array.
            // If there is a zero value on the diagonal, that element nonetheless must be explicitly represented.
            csr.PopulateExplicitZerosOnDiagonal();

            var rowCount = csr.RowCount;
            var columnCount = csr.ColumnCount;
            var valueCount = csr.ValueCount;

            var values = csr.Values;
            var rowPointers = csr.RowPointers;
            var columnIndices = csr.ColumnIndices;

            var nRhs = input.ColumnCount;
            var rhs = new Complex32[rowCount * nRhs];
            Array.Copy(input.ToColumnMajorArray(), rhs, rhs.Length);

            var solution = new Complex32[rowCount * nRhs];

            var error = SparseSolverControl.Provider.Solve(DssMatrixStructure.Nonsymmetric, DssMatrixType.Indefinite, DssSystemType.NonTransposed,
                rowCount, columnCount, valueCount, rowPointers, columnIndices, values,
                nRhs, rhs, solution);

            if (error == DssStatus.MKL_DSS_SUCCESS)
                result = Matrix<Complex32>.Build.DenseOfColumnMajor(rowCount, nRhs, solution);

            return error;
        }

        public static DssStatus Solve(this Matrix<Complex> matrix, Matrix<Complex> input, Matrix<Complex> result)
        {
            if (matrix.RowCount != matrix.ColumnCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixSquare, nameof(matrix));
            }
            if (result.ColumnCount != input.ColumnCount || result.RowCount != input.RowCount)
            {
                throw LinearAlgebra.Complex.Matrix.DimensionsDontMatch<ArgumentException>(input, result);
            }
            if (input.RowCount != matrix.RowCount)
            {
                throw LinearAlgebra.Complex.Matrix.DimensionsDontMatch<ArgumentException>(input, matrix);
            }

            var csr = matrix.Storage as SparseCompressedRowMatrixStorage<Complex>;
            if (csr == null)
            {
                throw new ArgumentException(Resources.MatrixMustBeSparse, nameof(matrix));
            }

            // No diagonal element can be omitted from the values array.
            // If there is a zero value on the diagonal, that element nonetheless must be explicitly represented.
            csr.PopulateExplicitZerosOnDiagonal();

            var rowCount = csr.RowCount;
            var columnCount = csr.ColumnCount;
            var valueCount = csr.ValueCount;

            var values = csr.Values;
            var rowPointers = csr.RowPointers;
            var columnIndices = csr.ColumnIndices;

            var nRhs = input.ColumnCount;
            var rhs = new Complex[rowCount * nRhs];
            Array.Copy(input.ToColumnMajorArray(), rhs, rhs.Length);

            var solution = new Complex[rowCount * nRhs];

            var error = SparseSolverControl.Provider.Solve(DssMatrixStructure.Nonsymmetric, DssMatrixType.Indefinite, DssSystemType.NonTransposed,
                rowCount, columnCount, valueCount, rowPointers, columnIndices, values,
                nRhs, rhs, solution);

            if (error == DssStatus.MKL_DSS_SUCCESS)
                result = Matrix<Complex>.Build.DenseOfColumnMajor(rowCount, nRhs, solution);

            return error;
        }
    }
}
