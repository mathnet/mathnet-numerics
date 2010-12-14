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
    using Algorithms.LinearAlgebra;
    using Generic;
    using Properties;
    using Threading;

    /// <summary>
    /// A Matrix class with dense storage. The underlying storage is a one dimensional array in column-major order.
    /// </summary>
    public class DenseMatrix : Matrix
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
        /// <param name="array">The one dimensional array to create this matrix from. This array should store the matrix in column-major order. see: http://en.wikipedia.org/wiki/Row-major_order </param>
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
        public Complex[] Data
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

        /// <summary>Calculates the L1 norm.</summary>
        /// <returns>The L1 norm of the matrix.</returns>
        public override Complex L1Norm()
        {
            return Control.LinearAlgebraProvider.MatrixNorm(Norm.OneNorm, RowCount, ColumnCount, Data);
        }

        /// <summary>Calculates the Frobenius norm of this matrix.</summary>
        /// <returns>The Frobenius norm of this matrix.</returns>
        public override Complex FrobeniusNorm()
        {
            return Control.LinearAlgebraProvider.MatrixNorm(Norm.FrobeniusNorm, RowCount, ColumnCount, Data);
        }

        /// <summary>Calculates the infinity norm of this matrix.</summary>
        /// <returns>The infinity norm of this matrix.</returns>  
        public override Complex InfinityNorm()
        {
            return Control.LinearAlgebraProvider.MatrixNorm(Norm.InfinityNorm, RowCount, ColumnCount, Data);
        }

        #region Elementary operations

        /// <summary>
        /// Adds another matrix to this matrix.
        /// </summary>
        /// <param name="other">The matrix to add to this matrix.</param>
        /// <param name="result">The matrix to store the result of add</param>
        /// <exception cref="ArgumentNullException">If the other matrix is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If the two matrices don't have the same dimensions.</exception>
        protected override void DoAdd(Matrix<Complex> other, Matrix<Complex> result)
        {
            var denseOther = other as DenseMatrix;
            var denseResult = result as DenseMatrix;
            if (denseOther == null || denseResult == null)
            {
                base.DoAdd(other, result);
            }
            else
            {
                Control.LinearAlgebraProvider.AddArrays(Data, denseOther.Data, denseResult.Data);
            }
        }

        /// <summary>
        /// Subtracts another matrix from this matrix.
        /// </summary>
        /// <param name="other">The matrix to subtract.</param>
        /// <param name="result">The matrix to store the result of the subtraction.</param>
        protected override void DoSubtract(Matrix<Complex> other, Matrix<Complex> result)
        {
            var denseOther = other as DenseMatrix;
            var denseResult = result as DenseMatrix;
            if (denseOther == null || denseResult == null)
            {
                base.DoSubtract(other, result);
            }
            else
            {
                Control.LinearAlgebraProvider.SubtractArrays(Data, denseOther.Data, denseResult.Data);
            }
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
                m.Data[(i * order) + i] = 1.0;
            }

            return m;
        }

        #endregion

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

        /// <summary>
        /// Multiplies each element of the matrix by a scalar and places results into the result matrix.
        /// </summary>
        /// <param name="scalar">The scalar to multiply the matrix with.</param>
        /// <param name="result">The matrix to store the result of the multiplication.</param>
        protected override void DoMultiply(Complex scalar, Matrix<Complex> result)
        {
            var denseResult = result as DenseMatrix;
            if (denseResult == null)
            {
                base.DoMultiply(scalar, result);
            }
            else
            {
                CopyTo(result);
                Control.LinearAlgebraProvider.ScaleArray(scalar, denseResult.Data);
            }
        }

        /// <summary>
        /// Multiplies this matrix with a vector and places the results into the result vector.
        /// </summary>
        /// <param name="rightSide">The vector to multiply with.</param>
        /// <param name="result">The result of the multiplication.</param>
        protected override void DoMultiply(Vector<Complex> rightSide, Vector<Complex> result)
        {
            var denseRight = rightSide as DenseVector;
            var denseResult = result as DenseVector;

            if (denseRight == null || denseResult == null)
            {
                base.DoMultiply(rightSide, result);
            }
            else
            {
                Control.LinearAlgebraProvider.MatrixMultiplyWithUpdate(
                    Algorithms.LinearAlgebra.Transpose.DontTranspose,
                    Algorithms.LinearAlgebra.Transpose.DontTranspose,
                    1.0,
                    Data,
                    RowCount,
                    ColumnCount,
                    denseRight.Data,
                    denseRight.Count,
                    1,
                    0.0,
                    denseResult.Data);
            }
        }

        /// <summary>
        /// Left multiply a matrix with a vector ( = vector * matrix ) and place the result in the result vector.
        /// </summary>
        /// <param name="leftSide">The vector to multiply with.</param>
        /// <param name="result">The result of the multiplication.</param>
        protected override void DoLeftMultiply(Vector<Complex> leftSide, Vector<Complex> result)
        {
            var denseLeft = leftSide as DenseVector;
            var denseResult = result as DenseVector;

            if (denseLeft == null || denseResult == null)
            {
                base.DoLeftMultiply(leftSide, result);
            }
            else
            {
                Control.LinearAlgebraProvider.MatrixMultiplyWithUpdate(
                    Algorithms.LinearAlgebra.Transpose.DontTranspose,
                    Algorithms.LinearAlgebra.Transpose.DontTranspose,
                    1.0,
                    denseLeft.Data,
                    1,
                    denseLeft.Count,
                    Data,
                    RowCount,
                    ColumnCount,
                    0.0,
                    denseResult.Data);
            }
        }
   
        /// <summary>
        /// Multiplies this matrix with another matrix and places the results into the result matrix.
        /// </summary>
        /// <param name="other">The matrix to multiply with.</param>
        /// <param name="result">The result of the multiplication.</param>
        protected override void DoMultiply(Matrix<Complex> other, Matrix<Complex> result)
        {
            var denseOther = other as DenseMatrix;
            var denseResult = result as DenseMatrix;

            if (denseOther == null || denseResult == null)
            {
                base.DoMultiply(other, result);
            }
            else
            {
                Control.LinearAlgebraProvider.MatrixMultiplyWithUpdate(
                                  Algorithms.LinearAlgebra.Transpose.DontTranspose,
                                  Algorithms.LinearAlgebra.Transpose.DontTranspose,
                                  1.0,
                                  Data,
                                  RowCount,
                                  ColumnCount,
                                  denseOther.Data,
                                  denseOther.RowCount,
                                  denseOther.ColumnCount,
                                  0.0,
                                  denseResult.Data);
            }
        }

        /// <summary>
        /// Multiplies this matrix with transpose of another matrix and places the results into the result matrix.
        /// </summary>
        /// <param name="other">The matrix to multiply with.</param>
        /// <param name="result">The result of the multiplication.</param>
        protected override void DoTransposeAndMultiply(Matrix<Complex> other, Matrix<Complex> result)
        {
             var denseOther = other as DenseMatrix;
            var denseResult = result as DenseMatrix;

            if (denseOther == null || denseResult == null)
            {
                base.DoTransposeAndMultiply(other, result);
            }
            else
            {
                Control.LinearAlgebraProvider.MatrixMultiplyWithUpdate(
                                  Algorithms.LinearAlgebra.Transpose.DontTranspose,
                                  Algorithms.LinearAlgebra.Transpose.Transpose,
                                  1.0,
                                  Data,
                                  RowCount,
                                  ColumnCount,
                                  denseOther.Data,
                                  denseOther.RowCount,
                                  denseOther.ColumnCount,
                                  0.0,
                                  denseResult.Data);
            }
        }

        /// <summary>
        /// Negate each element of this matrix and place the results into the result matrix.
        /// </summary>
        /// <param name="result">The result of the negation.</param>
        protected override void DoNegate(Matrix<Complex> result)
        {
            var denseResult = result as DenseMatrix;

            if (denseResult == null)
            {
                base.DoNegate(result);
            }
            else
            {
                Array.Copy(Data, denseResult.Data, Data.Length);
                Control.LinearAlgebraProvider.ScaleArray(-1, denseResult.Data);
            }
        }

        /// <summary>
        /// Pointwise multiplies this matrix with another matrix and stores the result into the result matrix.
        /// </summary>
        /// <param name="other">The matrix to pointwise multiply with this one.</param>
        /// <param name="result">The matrix to store the result of the pointwise multiplication.</param>
        protected override void DoPointwiseMultiply(Matrix<Complex> other, Matrix<Complex> result)
        {
            var denseOther = other as DenseMatrix;
            var denseResult = result as DenseMatrix;

            if (denseOther == null || denseResult == null)
            {
                base.DoPointwiseMultiply(other, result);
            }
            else
            {
                Control.LinearAlgebraProvider.PointWiseMultiplyArrays(Data, denseOther.Data, denseResult.Data);
            }
        }

        /// <summary>
        /// Pointwise divide this matrix by another matrix and stores the result into the result matrix.
        /// </summary>
        /// <param name="other">The matrix to pointwise divide this one by.</param>
        /// <param name="result">The matrix to store the result of the pointwise division.</param>
        protected override void DoPointwiseDivide(Matrix<Complex> other, Matrix<Complex> result)
        {
            var denseOther = other as DenseMatrix;
            var denseResult = result as DenseMatrix;

            if (denseOther == null || denseResult == null)
            {
                base.DoPointwiseDivide(other, result);
            }
            else
            {
                Control.LinearAlgebraProvider.PointWiseDivideArrays(Data, denseOther.Data, denseResult.Data);
            }
        }

        /// <summary>
        /// Computes the trace of this matrix.
        /// </summary>
        /// <returns>The trace of this matrix</returns>
        /// <exception cref="ArgumentException">If the matrix is not square</exception>
        public override Complex Trace()
        {
            if (RowCount != ColumnCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixSquare);
            }

            return CommonParallel.Aggregate(0, RowCount, i => Data[(i * RowCount) + i]);
        }
    }
}
