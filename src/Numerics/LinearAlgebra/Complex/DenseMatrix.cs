// <copyright file="DenseMatrix.cs" company="Math.NET">
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
    using Distributions;
    using Generic;
    using Properties;
    using Threading;

    /// <summary>
    /// A Matrix class with dense storage. The underlying storage is a one dimensional array in column-major order.
    /// </summary>
    public class DenseMatrix : Matrix<Complex>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DenseMatrix"/> class. This matrix is square with a given size.
        /// </summary>
        /// <param name="order">the size of the square matrix.</param>
        /// <exception cref="ArgumentException">
        /// If <paramref name="order"/> is less than one.
        /// </exception>
        public DenseMatrix(int order)
            : base(order)
        {
            Data = new Complex[order * order];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DenseMatrix"/> class.
        /// </summary>
        /// <param name="rows">
        /// The number of rows.
        /// </param>
        /// <param name="columns">
        /// The number of columns.
        /// </param>
        public DenseMatrix(int rows, int columns)
            : base(rows, columns)
        {
            Data = new Complex[rows * columns];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DenseMatrix"/> class with all entries set to a particular value.
        /// </summary>
        /// <param name="rows">
        /// The number of rows.
        /// </param>
        /// <param name="columns">
        /// The number of columns.
        /// </param>
        /// <param name="value">The value which we assign to each element of the matrix.</param>
        public DenseMatrix(int rows, int columns, Complex value)
            : base(rows, columns)
        {
            Data = new Complex[rows * columns];
            for (var i = 0; i < Data.Length; i++)
            {
                Data[i] = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DenseMatrix"/> class from a one dimensional array. This constructor
        /// will reference the one dimensional array and not copy it.
        /// </summary>
        /// <param name="rows">The number of rows.</param>
        /// <param name="columns">The number of columns.</param>
        /// <param name="array">The one dimensional array to create this matrix from. This array should store the matrix in column-major order. <seealso cref="http://en.wikipedia.org/wiki/Row-major_order"/></param>
        public DenseMatrix(int rows, int columns, Complex[] array)
            : base(rows, columns)
        {
            Data = array;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DenseMatrix"/> class from a 2D array. This constructor
        /// will allocate a completely new memory block for storing the dense matrix.
        /// </summary>
        /// <param name="array">The 2D array to create this matrix from.</param>
        public DenseMatrix(Complex[,] array)
            : base(array.GetLength(0), array.GetLength(1))
        {
            var rows = array.GetLength(0);
            var columns = array.GetLength(1);
            Data = new Complex[rows * columns];
            for (var i = 0; i < rows; i++)
            {
                for (var j = 0; j < columns; j++)
                {
                    Data[(j * rows) + i] = array[i, j];
                }
            }
        }

        /// <summary>
        /// Gets the matrix's data.
        /// </summary>
        /// <value>The matrix's data.</value>
        internal Complex[] Data
        {
            get;
            private set;
        }

        /// <summary>
        /// Creates a <c>DenseMatrix</c> for the given number of rows and columns.
        /// </summary>
        /// <param name="numberOfRows">
        /// The number of rows.
        /// </param>
        /// <param name="numberOfColumns">
        /// The number of columns.
        /// </param>
        /// <returns>
        /// A <c>DenseMatrix</c> with the given dimensions.
        /// </returns>
        public override Matrix<Complex> CreateMatrix(int numberOfRows, int numberOfColumns)
        {
            return new DenseMatrix(numberOfRows, numberOfColumns);
        }

        /// <summary>
        /// Creates a <see cref="Vector{T}"/> with a the given dimension.
        /// </summary>
        /// <param name="size">The size of the vector.</param>
        /// <returns>
        /// A <see cref="Vector{T}"/> with the given dimension.
        /// </returns>
        public override Vector<Complex> CreateVector(int size)
        {
            return new DenseVector(size);
        }

        /// <summary>
        /// Retrieves the requested element without range checking.
        /// </summary>
        /// <param name="row">
        /// The row of the element.
        /// </param>
        /// <param name="column">
        /// The column of the element.
        /// </param>
        /// <returns>
        /// The requested element.
        /// </returns>
        public override Complex At(int row, int column)
        {
            return Data[(column * RowCount) + row];
        }

        /// <summary>
        /// Sets the value of the given element.
        /// </summary>
        /// <param name="row">
        /// The row of the element.
        /// </param>
        /// <param name="column">
        /// The column of the element.
        /// </param>
        /// <param name="value">
        /// The value to set the element to.
        /// </param>
        public override void At(int row, int column, Complex value)
        {
            Data[(column * RowCount) + row] = value;
        }

        /// <summary>
        /// Sets all values to zero.
        /// </summary>
        public override void Clear()
        {
            Array.Clear(Data, 0, Data.Length);
        }

        /// <summary>
        /// Returns the transpose of this matrix.
        /// </summary>        
        /// <returns>The transpose of this matrix.</returns>
        public override Matrix<Complex> Transpose()
        {
            var ret = new DenseMatrix(ColumnCount, RowCount);
            for (var j = 0; j < ColumnCount; j++)
            {
                var index = j * RowCount;
                for (var i = 0; i < RowCount; i++)
                {
                    ret.Data[(i * ColumnCount) + j] = Data[index + i];
                }
            }

            return ret;
        }

        /// <summary>
        /// Returns the conjugate transpose of this matrix.
        /// </summary>        
        /// <returns>The conjugate transpose of this matrix.</returns>
        public override Matrix<Complex> ConjugateTranspose()
        {
            var ret = new DenseMatrix(ColumnCount, RowCount);
            for (var j = 0; j < ColumnCount; j++)
            {
                var index = j * RowCount;
                for (var i = 0; i < RowCount; i++)
                {
                    ret.Data[(i * ColumnCount) + j] = Data[index + i].Conjugate();
                }
            }

            return ret;
        }

        /// <summary>Calculates the L1 norm.</summary>
        /// <returns>The L1 norm of the matrix.</returns>
        public override double L1Norm()
        {
            var norm = 0.0;
            for (var j = 0; j < ColumnCount; j++)
            {
                var s = 0.0;
                for (var i = 0; i < RowCount; i++)
                {
                    s += Data[(j * RowCount) + i].Magnitude;
                }

                norm = Math.Max(norm, s);
            }

            return norm;
        }

        /// <summary>Calculates the Frobenius norm of this matrix.</summary>
        /// <returns>The Frobenius norm of this matrix.</returns>
        public override double FrobeniusNorm()
        {
            var transpose = (DenseMatrix)Transpose();
            var aat = this * transpose;

            var norm = 0.0;
            for (var i = 0; i < RowCount; i++)
            {
                norm += aat.Data[(i * RowCount) + i].Magnitude;
            }

            norm = Math.Sqrt(norm);
            return norm;
        }

        /// <summary>Calculates the infinity norm of this matrix.</summary>
        /// <returns>The infinity norm of this matrix.</returns>  
        public override double InfinityNorm()
        {
            var norm = 0.0;
            for (var i = 0; i < RowCount; i++)
            {
                var s = 0.0;
                for (var j = 0; j < ColumnCount; j++)
                {
                    s += Data[(j * RowCount) + i].Magnitude;
                }

                norm = Math.Max(norm, s);
            }

            return norm;
        }

        #region Elementary operations

        /// <summary>
        /// Adds another matrix to this matrix. The result will be written into this matrix.
        /// </summary>
        /// <param name="other">The matrix to add to this matrix.</param>
        /// <exception cref="ArgumentNullException">If the other matrix is <see langword="null" />.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If the two matrices don't have the same dimensions.</exception>
        public override void Add(Matrix<Complex> other)
        {
            var m = other as DenseMatrix;
            if (m == null)
            {
                base.Add(other);
            }
            else
            {
                Add(m);
            }
        }

        /// <summary>
        /// Adds another <see cref="DenseMatrix"/> to this matrix. The result will be written into this matrix.
        /// </summary>
        /// <param name="other">The <see cref="DenseMatrix"/> to add to this matrix.</param>
        /// <exception cref="ArgumentNullException">If the other matrix is <see langword="null" />.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If the two matrices don't have the same dimensions.</exception>
        public void Add(DenseMatrix other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            if (other.RowCount != RowCount || other.ColumnCount != ColumnCount)
            {
                throw new ArgumentOutOfRangeException(Resources.ArgumentMatrixDimensions);
            }

            Control.LinearAlgebraProvider.AddArrays(Data, other.Data, Data);
        }

        /// <summary>
        /// Subtracts another matrix from this matrix. The result will be written into this matrix.
        /// </summary>
        /// <param name="other">The matrix to subtract.</param>
        /// <exception cref="ArgumentNullException">If the other matrix is <see langword="null" />.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If the two matrices don't have the same dimensions.</exception>
        public override void Subtract(Matrix<Complex> other)
        {
            var m = other as DenseMatrix;
            if (m == null)
            {
                base.Subtract(other);
            }
            else
            {
                Subtract(m);
            }
        }

        /// <summary>
        /// Subtracts another <see cref="DenseMatrix"/> from this matrix. The result will be written into this matrix.
        /// </summary>
        /// <param name="other">The <see cref="DenseMatrix"/> to subtract.</param>
        /// <exception cref="ArgumentNullException">If the other matrix is <see langword="null" />.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If the two matrices don't have the same dimensions.</exception>
        public void Subtract(DenseMatrix other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            if (other.RowCount != RowCount || other.ColumnCount != ColumnCount)
            {
                throw new ArgumentOutOfRangeException(Resources.ArgumentMatrixDimensions);
            }

            Control.LinearAlgebraProvider.SubtractArrays(Data, other.Data, Data);
        }

        /// <summary>
        /// Multiplies each element of this matrix with a complex.
        /// </summary>
        /// <param name="complex">The complex to multiply with.</param>
        public override void Multiply(Complex complex)
        {
            Control.LinearAlgebraProvider.ScaleArray(complex, Data);
        }

        /// <summary>
        /// Multiplies this dense matrix with another dense matrix and places the results into the result dense matrix.
        /// </summary>
        /// <param name="other">The matrix to multiply with.</param>
        /// <param name="result">The result of the multiplication.</param>
        /// <exception cref="ArgumentNullException">If the other matrix is <see langword="null" />.</exception>
        /// <exception cref="ArgumentNullException">If the result matrix is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException">If <strong>this.Columns != other.Rows</strong>.</exception>
        /// <exception cref="ArgumentException">If the result matrix's dimensions are not the this.Rows x other.Columns.</exception>
        public override void Multiply(Matrix<Complex> other, Matrix<Complex> result)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            if (ColumnCount != other.RowCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixDimensions);
            }

            if (result.RowCount != RowCount || result.ColumnCount != other.ColumnCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixDimensions);
            }

            var m = other as DenseMatrix;
            var r = result as DenseMatrix;

            if (m == null || r == null)
            {
                base.Multiply(other, result);
            }
            else
            {
                Control.LinearAlgebraProvider.MatrixMultiply(
                    Data, 
                    RowCount, 
                    ColumnCount, 
                    m.Data, 
                    m.RowCount, 
                    m.ColumnCount, 
                    r.Data);
            }
        }

        /// <summary>
        /// Multiplies this matrix with another matrix and returns the result.
        /// </summary>
        /// <param name="other">The matrix to multiply with.</param>
        /// <exception cref="ArgumentException">If <strong>this.Columns != other.Rows</strong>.</exception>        
        /// <exception cref="ArgumentNullException">If the other matrix is <see langword="null" />.</exception>
        /// <returns>The result of multiplication.</returns>
        public override Matrix<Complex> Multiply(Matrix<Complex> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            if (ColumnCount != other.RowCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixDimensions);
            }

            var m = other as DenseMatrix;
            if (m == null)
            {
                return base.Multiply(other);
            }

            var result = (DenseMatrix)CreateMatrix(RowCount, other.ColumnCount);
            Multiply(other, result);
            return result;
        }

        /// <summary>
        /// Multiplies this dense matrix with transpose of another dense matrix and places the results into the result dense matrix.
        /// </summary>
        /// <param name="other">The matrix to multiply with.</param>
        /// <param name="result">The result of the multiplication.</param>
        /// <exception cref="ArgumentNullException">If the other matrix is <see langword="null" />.</exception>
        /// <exception cref="ArgumentNullException">If the result matrix is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException">If <strong>this.Columns != other.Rows</strong>.</exception>
        /// <exception cref="ArgumentException">If the result matrix's dimensions are not the this.Rows x other.Columns.</exception>
        public override void TransposeAndMultiply(Matrix<Complex> other, Matrix<Complex> result)
        {
            var otherDense = other as DenseMatrix;
            var resultDense = result as DenseMatrix;

            if (otherDense == null || resultDense == null)
            {
                base.TransposeAndMultiply(other, result);
                return;
            }

            if (ColumnCount != otherDense.ColumnCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixDimensions);
            }

            if ((resultDense.RowCount != RowCount) || (resultDense.ColumnCount != otherDense.RowCount))
            {
                throw new ArgumentException(Resources.ArgumentMatrixDimensions);
            }

            Control.LinearAlgebraProvider.MatrixMultiplyWithUpdate(
                Algorithms.LinearAlgebra.Transpose.DontTranspose,
                Algorithms.LinearAlgebra.Transpose.Transpose,
                1.0,
                Data,
                RowCount,
                ColumnCount,
                otherDense.Data,
                otherDense.RowCount,
                otherDense.ColumnCount,
                1.0,
                resultDense.Data);
        }

        /// <summary>
        /// Multiplies this matrix with transpose of another matrix and returns the result.
        /// </summary>
        /// <param name="other">The matrix to multiply with.</param>
        /// <exception cref="ArgumentException">If <strong>this.Columns != other.Rows</strong>.</exception>        
        /// <exception cref="ArgumentNullException">If the other matrix is <see langword="null" />.</exception>
        /// <returns>The result of multiplication.</returns>
        public override Matrix<Complex> TransposeAndMultiply(Matrix<Complex> other)
        {
            var otherDense = other as DenseMatrix;
            if (otherDense == null)
            {
                return base.TransposeAndMultiply(other);
            }

            if (ColumnCount != otherDense.ColumnCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixDimensions);
            }

            var result = (DenseMatrix)CreateMatrix(RowCount, other.RowCount);
            TransposeAndMultiply(other, result);
            return result;
        }

        /// <summary>
        /// Multiplies two dense matrices.
        /// </summary>
        /// <param name="leftSide">The left matrix to multiply.</param>
        /// <param name="rightSide">The right matrix to multiply.</param>
        /// <returns>The result of multiplication.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="leftSide"/> or <paramref name="rightSide"/> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException">If the dimensions of <paramref name="leftSide"/> or <paramref name="rightSide"/> don't conform.</exception>
        public static DenseMatrix operator *(DenseMatrix leftSide, DenseMatrix rightSide)
        {
            if (leftSide == null)
            {
                throw new ArgumentNullException("leftSide");
            }

            if (rightSide == null)
            {
                throw new ArgumentNullException("rightSide");
            }

            if (leftSide.ColumnCount != rightSide.RowCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixDimensions);
            }

            return (DenseMatrix)leftSide.Multiply(rightSide);
        }

        #endregion

        #region Static constructors for special matrices.

        /// <summary>
        /// Initializes a square <see cref="DenseMatrix"/> with all zero's except for ones on the diagonal.
        /// </summary>
        /// <param name="order">the size of the square matrix.</param>
        /// <returns>A dense identity matrix.</returns>
        /// <exception cref="ArgumentException">
        /// If <paramref name="order"/> is less than one.
        /// </exception>
        public static DenseMatrix Identity(int order)
        {
            var m = new DenseMatrix(order);
            for (var i = 0; i < order; i++)
            {
                m[i, i] = Complex.One;
            }

            return m;
        }

        #endregion

        /// <summary>
        /// Negate each element of this matrix.
        /// </summary>
        /// <exception cref="ArgumentNullException">If the result matrix is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException">if the result matrix's dimensions are not the same as this matrix.</exception>
        public override void Negate()
        {
            Multiply(-1);
        }

        /// <summary>
        /// Generates matrix with random elements.
        /// </summary>
        /// <param name="numberOfRows">Number of rows.</param>
        /// <param name="numberOfColumns">Number of columns.</param>
        /// <param name="distribution">Continuous Random Distribution or Source</param>
        /// <returns>
        /// An <c>numberOfRows</c>-by-<c>numberOfColumns</c> matrix with elements distributed according to the provided distribution.
        /// </returns>
        /// <exception cref="ArgumentException">If the parameter <paramref name="numberOfRows"/> is not positive.</exception>
        /// <exception cref="ArgumentException">If the parameter <paramref name="numberOfColumns"/> is not positive.</exception>
        public override Matrix<Complex> Random(int numberOfRows, int numberOfColumns, IContinuousDistribution distribution)
        {
            if (numberOfRows < 1)
            {
                throw new ArgumentException(Resources.ArgumentMustBePositive, "numberOfRows");
            }

            if (numberOfColumns < 1)
            {
                throw new ArgumentException(Resources.ArgumentMustBePositive, "numberOfColumns");
            }

            var matrix = CreateMatrix(numberOfRows, numberOfColumns);
            CommonParallel.For(
                0,
                ColumnCount,
                j =>
                {
                    for (var i = 0; i < matrix.RowCount; i++)
                    {
                        matrix[i, j] = new Complex(distribution.Sample(), distribution.Sample());
                    }
                });

            return matrix;
        }

        /// <summary>
        /// Generates matrix with random elements.
        /// </summary>
        /// <param name="numberOfRows">Number of rows.</param>
        /// <param name="numberOfColumns">Number of columns.</param>
        /// <param name="distribution">Continuous Random Distribution or Source</param>
        /// <returns>
        /// An <c>numberOfRows</c>-by-<c>numberOfColumns</c> matrix with elements distributed according to the provided distribution.
        /// </returns>
        /// <exception cref="ArgumentException">If the parameter <paramref name="numberOfRows"/> is not positive.</exception>
        /// <exception cref="ArgumentException">If the parameter <paramref name="numberOfColumns"/> is not positive.</exception>
        public override Matrix<Complex> Random(int numberOfRows, int numberOfColumns, IDiscreteDistribution distribution)
        {
            if (numberOfRows < 1)
            {
                throw new ArgumentException(Resources.ArgumentMustBePositive, "numberOfRows");
            }

            if (numberOfColumns < 1)
            {
                throw new ArgumentException(Resources.ArgumentMustBePositive, "numberOfColumns");
            }

            var matrix = CreateMatrix(numberOfRows, numberOfColumns);
            CommonParallel.For(
                0,
                ColumnCount,
                j =>
                {
                    for (var i = 0; i < matrix.RowCount; i++)
                    {
                        matrix[i, j] = new Complex(distribution.Sample(), distribution.Sample());
                    }
                });

            return matrix;
        }

        #region Simple arithmetic of type T
        /// <summary>
        /// Add two values T+T
        /// </summary>
        /// <param name="val1">Left operand value</param>
        /// <param name="val2">Right operand value</param>
        /// <returns>Result of addition</returns>
        protected sealed override Complex AddT(Complex val1, Complex val2)
        {
            return val1 + val2;
        }

        /// <summary>
        /// Subtract two values T-T
        /// </summary>
        /// <param name="val1">Left operand value</param>
        /// <param name="val2">Right operand value</param>
        /// <returns>Result of subtract</returns>
        protected sealed override Complex SubtractT(Complex val1, Complex val2)
        {
            return val1 - val2;
        }

        /// <summary>
        /// Multiply two values T*T
        /// </summary>
        /// <param name="val1">Left operand value</param>
        /// <param name="val2">Right operand value</param>
        /// <returns>Result of multiplication</returns>
        protected sealed override Complex MultiplyT(Complex val1, Complex val2)
        {
            return val1 * val2;
        }

        /// <summary>
        /// Divide two values T/T
        /// </summary>
        /// <param name="val1">Left operand value</param>
        /// <param name="val2">Right operand value</param>
        /// <returns>Result of divide</returns>
        protected sealed override Complex DivideT(Complex val1, Complex val2)
        {
            return val1 / val2;
        }

        /// <summary>
        /// Take absolute value
        /// </summary>
        /// <param name="val1">Source alue</param>
        /// <returns>True if one; otherwise false</returns>
        protected sealed override double AbsoluteT(Complex val1)
        {
            return val1.Magnitude;
        }
        #endregion  
    }
}
