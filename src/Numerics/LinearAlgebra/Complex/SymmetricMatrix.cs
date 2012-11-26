// <copyright file="SymmetricMatrix.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
// Copyright (c) 2009-2010 Math.NET
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
// </copyright>

namespace MathNet.Numerics.LinearAlgebra.Complex
{
    using System;
    using System.Numerics;
    using Generic;
    using Distributions;
    using Properties;
    using Storage;


    /// <summary>
    /// Abstract class for symmetric matrices. 
    /// </summary>
    [Serializable]
    public abstract class SymmetricMatrix : SquareMatrix
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SymmetricMatrix"/> class.
        /// </summary>
        protected SymmetricMatrix(MatrixStorage<Complex> storage)
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
        public static bool CheckIfSymmetric(Complex[,] array)
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
        public override sealed Matrix<Complex> Transpose()
        {
            return this.Clone();
        }

        /// <summary>
        /// Adds another matrix to this matrix.
        /// </summary>
        /// <param name="other">The matrix to add to this matrix.</param>
        /// <returns>The result of the addition.</returns>
        /// <exception cref="ArgumentNullException">If the other matrix is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If the two matrices don't have the same dimensions.</exception>
        public override Matrix<Complex> Add(Matrix<Complex> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            if (other.RowCount != RowCount || other.ColumnCount != ColumnCount)
            {
                throw DimensionsDontMatch<ArgumentOutOfRangeException>(this, other);
            }

            Matrix<Complex> result;
            if (other is SymmetricMatrix)
            {
                result = CreateMatrix(RowCount, ColumnCount);
            }
            else
            {
                result = CreateMatrix(RowCount, ColumnCount, true);
            }

            DoAdd(other, result);
            return result;
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
        protected override void DoAdd(Matrix<Complex> other, Matrix<Complex> result)
        {
            var symmetricOther = other as SymmetricMatrix;
            var symmetricResult = result as SymmetricMatrix;

            if (symmetricResult != null && !other.IsSymmetric)
            {
                throw new InvalidOperationException("Symmetric + non-symmetric matrix cannot be a symmetric matrix");
            }

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
        /// <param name="other">The matrix to subtract.</param>
        /// <returns>The result of the subtraction.</returns>
        /// <exception cref="ArgumentNullException">If the other matrix is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If the two matrices don't have the same dimensions.</exception>
        public override Matrix<Complex> Subtract(Matrix<Complex> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            if (other.RowCount != RowCount || other.ColumnCount != ColumnCount)
            {
                throw DimensionsDontMatch<ArgumentOutOfRangeException>(this, other);
            }

            Matrix<Complex> result;
            if (other is SymmetricMatrix)
            {
                result = CreateMatrix(RowCount, ColumnCount);
            }
            else
            {
                result = CreateMatrix(RowCount, ColumnCount, true);
            }

            DoSubtract(other, result);
            return result;
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
        protected override void DoSubtract(Matrix<Complex> other, Matrix<Complex> result)
        {
            var symmetricOther = other as SymmetricMatrix;
            var symmetricResult = result as SymmetricMatrix;

            if (symmetricResult != null && !other.IsSymmetric)
            {
                throw new InvalidOperationException("Symmetric - non-symmetric matrix cannot be a symmetric matrix");
            }

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
        protected override void DoMultiply(Complex scalar, Matrix<Complex> result)
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
        protected override sealed void DoTransposeThisAndMultiply(Matrix<Complex> other, Matrix<Complex> result)
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
        protected override sealed void DoTransposeThisAndMultiply(Vector<Complex> rightSide, Vector<Complex> result)
        {
            DoMultiply(rightSide, result);
        }

        /// <summary>
        /// Negate each element of this matrix and place the results into the result matrix.
        /// </summary>
        /// <param name="result">
        /// The result of the negation.
        /// </param>
        protected override void DoNegate(Matrix<Complex> result)
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
        /// Pointwise multiplies this matrix with another matrix.
        /// </summary>
        /// <param name="other">The matrix to pointwise multiply with this one.</param>
        /// <exception cref="ArgumentNullException">If the other matrix is <see langword="null" />.</exception> 
        /// <exception cref="ArgumentException">If this matrix and <paramref name="other"/> are not the same size.</exception>
        /// <returns>A new matrix that is the pointwise multiplication of this matrix and <paramref name="other"/>.</returns>
        public override Matrix<Complex> PointwiseMultiply(Matrix<Complex> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            if (ColumnCount != other.ColumnCount || RowCount != other.RowCount)
            {
                throw DimensionsDontMatch<ArgumentException>(this, other, "other");
            }

            Matrix<Complex> result;
            if (other is SymmetricMatrix)
            {
                result = CreateMatrix(RowCount, ColumnCount);
            }
            else
            {
                result = CreateMatrix(RowCount, ColumnCount, true);
            }

            PointwiseMultiply(other, result);
            return result;
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
        protected override void DoPointwiseMultiply(Matrix<Complex> other, Matrix<Complex> result)
        {
            var symmetricOther = other as SymmetricMatrix;
            var symmetricResult = result as SymmetricMatrix;

            if (symmetricResult != null && !other.IsSymmetric)
            {
                throw new InvalidOperationException("Symmetric pointwise* non-symmetric matrix cannot be a symmetric matrix");
            }

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
        /// Pointwise divide this matrix by another matrix.
        /// </summary>
        /// <param name="other">The matrix to pointwise subtract this one by.</param>
        /// <exception cref="ArgumentNullException">If the other matrix is <see langword="null" />.</exception> 
        /// <exception cref="ArgumentException">If this matrix and <paramref name="other"/> are not the same size.</exception>
        /// <returns>A new matrix that is the pointwise division of this matrix and <paramref name="other"/>.</returns>
        public override Matrix<Complex> PointwiseDivide(Matrix<Complex> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            if (ColumnCount != other.ColumnCount || RowCount != other.RowCount)
            {
                throw DimensionsDontMatch<ArgumentException>(this, other);
            }

            Matrix<Complex> result;
            if (other is SymmetricMatrix)
            {
                result = CreateMatrix(RowCount, ColumnCount);
            }
            else
            {
                result = CreateMatrix(RowCount, ColumnCount, true);
            }

            PointwiseDivide(other, result);
            return result;
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
        protected override void DoPointwiseDivide(Matrix<Complex> other, Matrix<Complex> result)
        {
            var symmetricOther = other as SymmetricMatrix;
            var symmetricResult = result as SymmetricMatrix;

            if (symmetricResult != null && !other.IsSymmetric)
            {
                throw new InvalidOperationException("Symmetric pointwise/ non-symmetric matrix cannot be a symmetric matrix");
            }

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
        protected override void DoModulus(Complex divisor, Matrix<Complex> result)
        {
            throw new NotImplementedException();
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
        protected override void DoRandom(Matrix<Complex> matrix, IContinuousDistribution distribution)
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
        protected override void DoRandom(Matrix<Complex> matrix, IDiscreteDistribution distribution)
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
        /// Creates a new matrix and inserts the given column at the given index.
        /// </summary>
        /// <param name="columnIndex">The index of where to insert the column.</param>
        /// <param name="column">The column to insert.</param>
        /// <returns>A new matrix with the inserted column.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="column "/> is <see langword="null" />. </exception>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="columnIndex"/> is &lt; zero or &gt; the number of columns.</exception>
        /// <exception cref="ArgumentException">If the size of <paramref name="column"/> != the number of rows.</exception>
        public override Matrix<Complex> InsertColumn(int columnIndex, Vector<Complex> column)
        {
            throw new InvalidOperationException("Inserting a column is not supported on a symmetric matrix. Symmetric matrices are square");
        }

        /// <summary>
        /// Copies the values of the given array to the specified column. The changes retain the symmetry of the matrix. 
        /// </summary>
        /// <param name="columnIndex">The column to copy the values to.</param>
        /// <param name="column">The array to copy the values from.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="column"/> is <see langword="null" />.</exception>        
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="columnIndex"/> is less than zero,
        /// or greater than or equal to the number of columns.</exception>
        /// <exception cref="ArgumentException">If the size of <paramref name="column"/> does not
        /// equal the number of rows of this <strong>Matrix</strong>.</exception>
        /// <exception cref="ArgumentException">If the size of <paramref name="column"/> does not
        /// equal the number of rows of this <strong>Matrix</strong>.</exception>
        public override void SetColumn(int columnIndex, Complex[] column)
        {
            if (columnIndex < 0 || columnIndex >= ColumnCount)
            {
                throw new ArgumentOutOfRangeException("columnIndex");
            }

            if (column == null)
            {
                throw new ArgumentNullException("column");
            }

            if (column.Length != RowCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixSameRowDimension, "column");
            }

            for (var i = 0; i < columnIndex; i++)
            {
                At(i, columnIndex, column[i]);
            }
        }

        /// <summary>
        /// Copies the values of the given Vector to the specified column. The changes retain the symmetry of the matrix. 
        /// </summary>
        /// <param name="columnIndex">The column to copy the values to.</param>
        /// <param name="column">The vector to copy the values from.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="column"/> is <see langword="null" />.</exception>        
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="columnIndex"/> is less than zero,
        /// or greater than or equal to the number of columns.</exception>
        /// <exception cref="ArgumentException">If the size of <paramref name="column"/> does not
        /// equal the number of rows of this <strong>Matrix</strong>.</exception>
        public override void SetColumn(int columnIndex, Vector<Complex> column)
        {
            if (columnIndex < 0 || columnIndex >= ColumnCount)
            {
                throw new ArgumentOutOfRangeException("columnIndex");
            }

            if (column == null)
            {
                throw new ArgumentNullException("column");
            }

            if (column.Count != RowCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixSameRowDimension, "column");
            }

            for (var i = 0; i < columnIndex; i++)
            {
                At(i, columnIndex, column[i]);
            }
        }

        /// <summary>
        /// Creates a new matrix and inserts the given row at the given index.
        /// </summary>
        /// <param name="rowIndex">The index of where to insert the row.</param>
        /// <param name="row">The row to insert.</param>
        /// <returns>A new matrix with the inserted column.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="row"/> is <see langword="null" />. </exception>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="rowIndex"/> is &lt; zero or &gt; the number of rows.</exception>
        /// <exception cref="ArgumentException">If the size of <paramref name="row"/> != the number of columns.</exception>
        public override Matrix<Complex> InsertRow(int rowIndex, Vector<Complex> row)
        {
            throw new InvalidOperationException("Inserting a row is not supported on a symmetric matrix. Symmetric matrices are square");
        }

        /// <summary>
        /// Copies the values of the given Vector to the specified row.
        /// </summary>
        /// <param name="rowIndex">The row to copy the values to.</param>
        /// <param name="row">The vector to copy the values from.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="row"/> is <see langword="null" />.</exception>            
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="rowIndex"/> is less than zero,
        /// or greater than or equal to the number of rows.</exception>
        /// <exception cref="ArgumentException">If the size of <paramref name="row"/> does not
        /// equal the number of columns of this <strong>Matrix</strong>.</exception>
        public override void SetRow(int rowIndex, Vector<Complex> row)
        {
            if (rowIndex < 0 || rowIndex >= RowCount)
            {
                throw new ArgumentOutOfRangeException("rowIndex");
            }

            if (row == null)
            {
                throw new ArgumentNullException("row");
            }

            if (row.Count != ColumnCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixSameRowDimension, "row");
            }

            for (var i = rowIndex; i < ColumnCount; i++)
            {
                At(rowIndex, i, row[i]);
            }
        }

        /// <summary>
        /// Copies the values of the given array to the specified row.
        /// </summary>
        /// <param name="rowIndex">The row to copy the values to.</param>
        /// <param name="row">The array to copy the values from.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="row"/> is <see langword="null" />.</exception>  
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="rowIndex"/> is less than zero,
        /// or greater than or equal to the number of rows.</exception>
        /// <exception cref="ArgumentException">If the size of <paramref name="row"/> does not
        /// equal the number of columns of this <strong>Matrix</strong>.</exception>
        public override void SetRow(int rowIndex, Complex[] row)
        {
            if (rowIndex < 0 || rowIndex >= RowCount)
            {
                throw new ArgumentOutOfRangeException("rowIndex");
            }

            if (row == null)
            {
                throw new ArgumentNullException("row");
            }

            if (row.Length != ColumnCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixSameRowDimension, "row");
            }

            for (var i = rowIndex; i < ColumnCount; i++)
            {
                At(rowIndex, i, row[i]);
            }
        }
    }
}
