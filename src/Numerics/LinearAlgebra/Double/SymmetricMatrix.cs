namespace MathNet.Numerics.LinearAlgebra.Double
{
    using System;

    using MathNet.Numerics.Distributions;
    using MathNet.Numerics.LinearAlgebra.Generic;
    using MathNet.Numerics.LinearAlgebra.Storage;

    /// <summary>
    /// Abstract class for symmetric matrices. 
    /// </summary>
    [Serializable]
    public abstract class SymmetricMatrix : SquareMatrix
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SymmetricMatrix"/> class.
        /// </summary>
        protected SymmetricMatrix(MatrixStorage<double> storage)
            : base(storage)
        {
        }

        /// <summary>
        /// Returns a value indicating whether the array is symmetric.
        /// </summary>
        /// <param name="array">
        /// The array to check for symmetry. 
        /// </param>
        /// <returns>
        /// True is array is symmetric, false if not symmetric. 
        /// </returns>
        public static bool CheckIfSymmetric(double[,] array)
        {
            var rows = array.GetLength(0);
            var columns = array.GetLength(1);

            if (rows != columns)
            {
                return false;
            }

            for (var row = 0; row < rows; row++)
            {
                for (var column = 0; column < columns; column++)
                {
                    if (column >= row)
                    {
                        continue;
                    }

                    if (!array[row, column].Equals(array[column, row]))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        ///   Gets a value indicating whether this matrix is symmetric.
        /// </summary>
        public override sealed bool IsSymmetric
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Returns the transpose of this matrix. The transpose is equal and this method returns a reference to this matrix.
        /// </summary>
        /// <returns>
        /// The transpose of this matrix.
        /// </returns>
        public override sealed Matrix<double> Transpose()
        {
            return this;
        }

        /// <summary>
        /// Adds another matrix to this matrix.
        /// </summary>
        /// <param name="other">
        /// The matrix to add to this matrix.
        /// </param>
        /// <param name="result">
        /// The matrix to store the result of the addition.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// If the other matrix is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If the two matrices don't have the same dimensions.
        /// </exception>
        protected override void DoAdd(Matrix<double> other, Matrix<double> result)
        {
            var symmetricOther = other as SymmetricMatrix;
            var symmetricResult = result as SymmetricMatrix;
            if (symmetricOther == null || symmetricResult == null)
            {
                base.DoAdd(other, result);
            }
            else
            {
                for (var row = 0; row < RowCount; row++)
                {
                    for (var column = row; column < ColumnCount; column++)
                    {
                        symmetricResult.At(row, column, At(row, column) + symmetricOther.At(row, column));
                    }
                }
            }
        }

        /// <summary>
        /// Subtracts another matrix from this matrix.
        /// </summary>
        /// <param name="other">
        /// The matrix to subtract to this matrix.
        /// </param>
        /// <param name="result">
        /// The matrix to store the result of subtraction.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// If the other matrix is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If the two matrices don't have the same dimensions.
        /// </exception>
        protected override void DoSubtract(Matrix<double> other, Matrix<double> result)
        {
            var symmetricOther = other as SymmetricMatrix;
            var symmetricResult = result as SymmetricMatrix;
            if (symmetricOther == null || symmetricResult == null)
            {
                base.DoSubtract(other, result);
            }
            else
            {
                for (var row = 0; row < RowCount; row++)
                {
                    for (var column = row; column < ColumnCount; column++)
                    {
                        symmetricResult.At(row, column, At(row, column) - symmetricOther.At(row, column));
                    }
                }
            }
        }

        /// <summary>
        /// Multiplies each element of the matrix by a scalar and places results into the result matrix.
        /// </summary>
        /// <param name="scalar">
        /// The scalar to multiply the matrix with.
        /// </param>
        /// <param name="result">
        /// The matrix to store the result of the multiplication.
        /// </param>
        protected override void DoMultiply(double scalar, Matrix<double> result)
        {
            var symmetricResult = result as SymmetricMatrix;

            if (symmetricResult == null)
            {
                base.DoMultiply(scalar, result);
            }
            else
            {
                for (var row = 0; row < RowCount; row++)
                {
                    for (var column = row; column < ColumnCount; column++)
                    {
                        symmetricResult.At(row, column, At(row, column) * scalar);
                    }
                }
            }
        }

        /// <summary>
        /// Multiplies the transpose of this matrix with another matrix and places the results into the result matrix.
        /// </summary>
        /// <param name="other">
        /// The matrix to multiply with.
        /// </param>
        /// <param name="result">
        /// The result of the multiplication.
        /// </param>
        protected override sealed void DoTransposeThisAndMultiply(Matrix<double> other, Matrix<double> result)
        {
            DoMultiply(other, result);
        }

        /// <summary>
        /// Multiplies the transpose of this matrix with a vector and places the results into the result vector.
        /// </summary>
        /// <param name="rightSide">
        /// The vector to multiply with.
        /// </param>
        /// <param name="result">
        /// The result of the multiplication.
        /// </param>
        protected override sealed void DoTransposeThisAndMultiply(Vector<double> rightSide, Vector<double> result)
        {
            DoMultiply(rightSide, result);
        }

        /// <summary>
        /// Negate each element of this matrix and place the results into the result matrix.
        /// </summary>
        /// <param name="result">
        /// The result of the negation.
        /// </param>
        protected override void DoNegate(Matrix<double> result)
        {
            var symmetricResult = result as SymmetricMatrix;

            if (symmetricResult == null)
            {
                base.DoNegate(result);
            }
            else
            {
                for (var row = 0; row < RowCount; row++)
                {
                    for (var column = row; column != ColumnCount; column++)
                    {
                        symmetricResult[row, column] = -At(row, column);
                    }
                }
            }
        }

        /// <summary>
        /// Pointwise multiplies this matrix with another matrix and stores the result into the result matrix.
        /// </summary>
        /// <param name="other">
        /// The matrix to pointwise multiply with this one.
        /// </param>
        /// <param name="result">
        /// The matrix to store the result of the pointwise multiplication.
        /// </param>
        protected override void DoPointwiseMultiply(Matrix<double> other, Matrix<double> result)
        {
            var symmetricOther = other as SymmetricMatrix;
            var symmetricResult = result as SymmetricMatrix;
            if (symmetricOther == null || symmetricResult == null)
            {
                base.DoPointwiseMultiply(other, result);
            }
            else
            {
                for (var row = 0; row < RowCount; row++)
                {
                    for (var column = row; column < ColumnCount; column++)
                    {
                        symmetricResult.At(row, column, At(row, column) * symmetricOther.At(row, column));
                    }
                }
            }
        }

        /// <summary>
        /// Pointwise divide this matrix by another matrix and stores the result into the result matrix.
        /// </summary>
        /// <param name="other">
        /// The matrix to pointwise divide this one by.
        /// </param>
        /// <param name="result">
        /// The matrix to store the result of the pointwise division.
        /// </param>
        protected override void DoPointwiseDivide(Matrix<double> other, Matrix<double> result)
        {
            var symmetricOther = other as SymmetricMatrix;
            var symmetricResult = result as SymmetricMatrix;
            if (symmetricOther == null || symmetricResult == null)
            {
                base.DoPointwiseDivide(other, result);
            }
            else
            {
                for (var row = 0; row < RowCount; row++)
                {
                    for (var column = row; column < ColumnCount; column++)
                    {
                        symmetricResult.At(row, column, At(row, column) / symmetricOther.At(row, column));
                    }
                }
            }
        }

        /// <summary>
        /// Computes the modulus for each element of the matrix.
        /// </summary>
        /// <param name="divisor">
        /// The divisor to use.
        /// </param>
        /// <param name="result">
        /// Matrix to store the results in.
        /// </param>
        protected override void DoModulus(double divisor, Matrix<double> result)
        {
            var symmetricResult = result as SymmetricMatrix;

            if (symmetricResult == null)
            {
                base.DoModulus(divisor, result);
            }
            else
            {
                for (var row = 0; row < RowCount; row++)
                {
                    for (var column = row; column < ColumnCount; column++)
                    {
                        symmetricResult.At(row, column, At(row, column) % divisor);
                    }
                }
            }
        }

        /// <summary>
        /// Populates a matrix with random elements.
        /// </summary>
        /// <param name="matrix">
        /// The matrix to populate.
        /// </param>
        /// <param name="distribution">
        /// Continuous Random Distribution to generate elements from.
        /// </param>
        protected override void DoRandom(Matrix<double> matrix, IContinuousDistribution distribution)
        {
            var symmetricMatrix = matrix as SymmetricMatrix;

            if (symmetricMatrix == null)
            {
                base.DoRandom(matrix, distribution);
            }
            else
            {
                for (var row = 0; row < matrix.RowCount; row++)
                {
                    for (var column = row; column < matrix.ColumnCount; column++)
                    {
                        symmetricMatrix.At(row, column, distribution.Sample());
                    }
                }
            }
        }

        /// <summary>
        /// Populates a matrix with random elements.
        /// </summary>
        /// <param name="matrix">
        /// The matrix to populate.
        /// </param>
        /// <param name="distribution">
        /// Continuous Random Distribution to generate elements from.
        /// </param>
        protected override void DoRandom(Matrix<double> matrix, IDiscreteDistribution distribution)
        {
            var symmetricMatrix = matrix as SymmetricMatrix;
            if (symmetricMatrix == null)
            {
                base.DoRandom(matrix, distribution);
            }
            else
            {
                for (var row = 0; row < matrix.RowCount; row++)
                {
                    for (var column = row; column < matrix.ColumnCount; column++)
                    {
                        symmetricMatrix.At(row, column, distribution.Sample());
                    }
                }
            }
        }
    }
}
