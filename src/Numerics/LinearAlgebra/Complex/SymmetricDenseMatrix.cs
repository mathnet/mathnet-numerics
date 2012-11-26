// <copyright file="SymmetricDenseMatrix.cs" company="Math.NET">
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
    using MathNet.Numerics.Distributions;
    using MathNet.Numerics.LinearAlgebra.Storage.Indexers.Static;
    using Properties;
    using Storage;

    /// <summary>
    /// A Symmetric Matrix class with dense storage. 
    /// </summary>
    /// <remarks> The underlying storage is a one dimensional array in column-major order.
    /// The Upper Triangle is stored(it is equal to the Lower Triangle) </remarks>
    [Serializable]
    public class SymmetricDenseMatrix : SymmetricMatrix
    {
        readonly DenseColumnMajorSymmetricMatrixStorage<Complex> _storage;

        /// <summary>
        /// Gets the matrix's data.
        /// </summary>
        /// <value>The matrix's data.</value>
        readonly Complex[] _data;

        internal SymmetricDenseMatrix(DenseColumnMajorSymmetricMatrixStorage<Complex> storage)
            : base(storage)
        {
            _storage = storage;
            _data = _storage.Data;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SymmetricDenseMatrix"/> class. This matrix is square with a given size.
        /// </summary>
        /// <param name="order">The order of the matrix.</param>
        /// <exception cref="ArgumentException">
        /// If <paramref name="order"/> is less than one.
        /// </exception>
        public SymmetricDenseMatrix(int order)
            : this(new DenseColumnMajorSymmetricMatrixStorage<Complex>(order))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SymmetricDenseMatrix"/> class with all entries set to a particular value.
        /// </summary>
        /// <param name="order">
        /// The order of the matrix.
        /// </param>
        /// <param name="value">The value which we assign to each element of the matrix.</param>
        public SymmetricDenseMatrix(int order, Complex value)
            : this(order)
        {
            for (var i = 0; i < _data.Length; i++)
            {
                _data[i] = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SymmetricDenseMatrix"/> class from a one dimensional array. This constructor
        /// will reference the one dimensional array and not copy it.
        /// </summary>
        /// <param name="order">The size of the square matrix.</param>
        /// <param name="array">
        /// The one dimensional array to create this matrix from. Column-major and row-major order is identical on a symmetric matrix: http://en.wikipedia.org/wiki/Row-major_order 
        /// </param>
        /// <exception cref="ArgumentException">
        /// If <paramref name="array"/> does not represent a packed array.
        /// </exception>
        public SymmetricDenseMatrix(int order, Complex[] array)
            : this(new DenseColumnMajorSymmetricMatrixStorage<Complex>(order, array))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SymmetricDenseMatrix"/> class from a 2D array. This constructor
        /// will allocate a completely new memory block for storing the symmetric dense matrix.
        /// </summary>
        /// <param name="array">The 2D array to create this matrix from.</param>
        /// <exception cref="ArgumentException">
        /// If <paramref name="array"/> is not a square array.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="array"/> is not a symmetric array.
        /// </exception>
        public SymmetricDenseMatrix(Complex[,] array)
            : this(array.GetLength(0))
        {
            if (!CheckIfSymmetric(array))
            {
                throw new ArgumentException(Resources.ArgumentMatrixSymmetric);
            }

            var indexer = new PackedStorageIndexerUpper(Order);
            for (var row = 0; row < Order; row++)
            {
                for (var column = row; column < Order; column++)
                {
                    _data[indexer.Of(row, column)] = array[row, column];
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SymmetricDenseMatrix"/> class, copying
        /// the values from the given matrix. Matrix must be Symmetric.
        /// </summary>
        /// <param name="matrix">The matrix to copy.</param>
        /// <exception cref="ArgumentException">
        /// If <paramref name="matrix"/> is not a square matrix.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="matrix"/> is not a symmetric matrix.
        /// </exception>
        public SymmetricDenseMatrix(Matrix<Complex> matrix)
            : this(matrix.RowCount)
        {
            var symmetricMatrix = matrix as SymmetricDenseMatrix;

            if (!matrix.IsSymmetric)
            {
                throw new ArgumentException(Resources.ArgumentMatrixSymmetric);
            }

            if (symmetricMatrix == null)
            {
                var indexer = new PackedStorageIndexerUpper(Order);
                for (var row = 0; row < Order; row++)
                {
                    for (var column = row; column < Order; column++)
                    {
                        _data[indexer.Of(row, column)] = matrix[row, column];
                    }
                }
            }
            else
            {
                matrix.CopyTo(this);
            }
        }

        /// <summary>
        /// Gets the matrix's data.
        /// </summary>
        /// <value>The matrix's data.</value>
        public Complex[] Data
        {
            get { return _data; }
        }

        /// <summary>
        /// Creates a <c>SymmetricDenseMatrix</c> for the given number of rows and columns. 
        /// If rows and columns are not equal, returns a <c>DenseMatrix</c> instead. 
        /// </summary>
        /// <param name="numberOfRows">
        /// The number of rows.
        /// </param>
        /// <param name="numberOfColumns">
        /// The number of columns.
        /// </param>
        /// <param name="fullyMutable">True if all fields must be mutable (e.g. not a diagonal matrix).</param>
        /// <returns>
        /// A <c>DenseMatrix</c> or <c>SymmetricDenseMatrix</c> with the given dimensions.
        /// </returns>
        /// /// <exception cref="ArgumentException">
        /// If <paramref name="numberOfRows"/> is not equal to <paramref name="numberOfColumns"/>. 
        /// Symmetric arrays are always square
        /// </exception>
        public override Matrix<Complex> CreateMatrix(int numberOfRows, int numberOfColumns, bool fullyMutable = false)
        {
            if (numberOfRows != numberOfColumns || fullyMutable)
            {
                return new DenseMatrix(numberOfRows, numberOfColumns);
            }

            return new SymmetricDenseMatrix(numberOfRows);
        }

        /// <summary>
        /// Creates a <see cref="Vector{T}"/> with a the given dimension.
        /// </summary>
        /// <param name="size">The size of the vector.</param>
        /// <param name="fullyMutable">True if all fields must be mutable.</param>
        /// <returns>
        /// A <see cref="Vector{T}"/> with the given dimension.
        /// </returns>
        public override Vector<Complex> CreateVector(int size, bool fullyMutable = false)
        {
            return new DenseVector(size);
        }

        #region Static constructors for special matrices.

        /// <summary>
        /// Initializes a square <see cref="SymmetricDenseMatrix"/> with all zero's except for ones on the diagonal.
        /// </summary>
        /// <param name="order">the size of the square matrix.</param>
        /// <returns>A symmetric dense identity matrix.</returns>
        /// <exception cref="ArgumentException">
        /// If <paramref name="order"/> is less than one.
        /// </exception>
        public static SymmetricDenseMatrix Identity(int order)
        {
            var m = new SymmetricDenseMatrix(order);
            for (var i = 0; i < order; i++)
            {
                m.At(i, i, 1.0);
            }

            return m;
        }

        #endregion

        /// <summary>
        /// Adds another matrix to this matrix.
        /// </summary>
        /// <param name="other">The matrix to add to this matrix.</param>
        /// <param name="result">The matrix to store the result of add</param>
        /// <exception cref="ArgumentNullException">If the other matrix is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If the two matrices don't have the same dimensions.</exception>
        protected override void DoAdd(Matrix<Complex> other, Matrix<Complex> result)
        {
            var denseOther = other as SymmetricDenseMatrix;
            var denseResult = result as SymmetricDenseMatrix;
            if (denseOther == null || denseResult == null)
            {
                base.DoAdd(other, result);
            }
            else
            {
                Control.LinearAlgebraProvider.AddArrays(_data, denseOther._data, denseResult._data);
            }
        }

        /// <summary>
        /// Subtracts another matrix from this matrix.
        /// </summary>
        /// <param name="other">The matrix to subtract.</param>
        /// <param name="result">The matrix to store the result of the subtraction.</param>
        protected override void DoSubtract(Matrix<Complex> other, Matrix<Complex> result)
        {
            var denseOther = other as SymmetricDenseMatrix;
            var denseResult = result as SymmetricDenseMatrix;
            if (denseOther == null || denseResult == null)
            {
                base.DoSubtract(other, result);
            }
            else
            {
                Control.LinearAlgebraProvider.SubtractArrays(_data, denseOther._data, denseResult._data);
            }
        }

        /// <summary>
        /// Multiplies each element of the matrix by a scalar and places results into the result matrix.
        /// </summary>
        /// <param name="scalar">The scalar to multiply the matrix with.</param>
        /// <param name="result">The matrix to store the result of the multiplication.</param>
        protected override void DoMultiply(Complex scalar, Matrix<Complex> result)
        {
            var denseResult = result as SymmetricDenseMatrix;
            if (denseResult == null)
            {
                base.DoMultiply(scalar, result);
            }
            else
            {
                Control.LinearAlgebraProvider.ScaleArray(scalar, _data, denseResult._data);
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
                // TODO: Change this when symmetric methods are implemented in the Linear Algebra Providers. 
                base.DoMultiply(rightSide, result);
            }
        }

        /// <summary>
        /// Multiplies this matrix with another matrix and places the results into the result matrix.
        /// </summary>
        /// <param name="other">The matrix to multiply with.</param>
        /// <param name="result">The result of the multiplication.</param>
        protected override void DoMultiply(Matrix<Complex> other, Matrix<Complex> result)
        {
            var denseOther = other as SymmetricDenseMatrix;
            var denseResult = result as SymmetricDenseMatrix;

            if (denseOther == null || denseResult == null)
            {
                base.DoMultiply(other, result);
            }
            else
            {
                // TODO: Change this when symmetric methods are implemented in the Linear Algebra Providers.
                base.DoMultiply(other, result);
            }
        }

        /// <summary>
        /// Multiplies this matrix with transpose of another matrix and places the results into the result matrix.
        /// </summary>
        /// <param name="other">The matrix to multiply with.</param>
        /// <param name="result">The result of the multiplication.</param>
        protected override void DoTransposeAndMultiply(Matrix<Complex> other, Matrix<Complex> result)
        {
            var denseOther = other as SymmetricDenseMatrix;
            var denseResult = result as SymmetricDenseMatrix;

            if (denseOther == null || denseResult == null)
            {
                base.DoTransposeAndMultiply(other, result);
            }
            else
            {
                // TODO: Change this when symmetric methods are implemented in the Linear Algebra Providers.
                base.DoTransposeAndMultiply(other, result);
            }
        }

        /// <summary>
        /// Negate each element of this matrix and place the results into the result matrix.
        /// </summary>
        /// <param name="result">The result of the negation.</param>
        protected override void DoNegate(Matrix<Complex> result)
        {
            var denseResult = result as SymmetricDenseMatrix;

            if (denseResult == null)
            {
                base.DoNegate(result);
            }
            else
            {
                Control.LinearAlgebraProvider.ScaleArray(-1, _data, denseResult._data);
            }
        }

        /// <summary>
        /// Pointwise multiplies this matrix with another matrix and stores the result into the result matrix.
        /// </summary>
        /// <param name="other">The matrix to pointwise multiply with this one.</param>
        /// <param name="result">The matrix to store the result of the pointwise multiplication.</param>
        protected override void DoPointwiseMultiply(Matrix<Complex> other, Matrix<Complex> result)
        {
            var denseOther = other as SymmetricDenseMatrix;
            var denseResult = result as SymmetricDenseMatrix;

            if (denseOther == null || denseResult == null)
            {
                base.DoPointwiseMultiply(other, result);
            }
            else
            {
                Control.LinearAlgebraProvider.PointWiseMultiplyArrays(_data, denseOther._data, denseResult._data);
            }
        }

        /// <summary>
        /// Pointwise divide this matrix by another matrix and stores the result into the result matrix.
        /// </summary>
        /// <param name="other">The matrix to pointwise divide this one by.</param>
        /// <param name="result">The matrix to store the result of the pointwise division.</param>
        protected override void DoPointwiseDivide(Matrix<Complex> other, Matrix<Complex> result)
        {
            var denseOther = other as SymmetricDenseMatrix;
            var denseResult = result as SymmetricDenseMatrix;

            if (denseOther == null || denseResult == null)
            {
                base.DoPointwiseDivide(other, result);
            }
            else
            {
                Control.LinearAlgebraProvider.PointWiseDivideArrays(_data, denseOther._data, denseResult._data);
            }
        }

        /// <summary>
        /// Returns a new matrix containing the lower triangle of this matrix.
        /// </summary>
        /// <returns>The lower triangle of this matrix.</returns>  
        public override Matrix<Complex> LowerTriangle()
        {
            var ret = new DenseMatrix(Order);
            for (var row = 0; row < Order; row++)
            {
                for (var column = 0; column <= row; column++)
                {
                    ret[row, column] = At(row, column);
                }
            }

            return ret;
        }

        /// <summary>
        /// Returns a new matrix containing the lower triangle of this matrix. The new matrix
        /// does not contain the diagonal elements of this matrix.
        /// </summary>
        /// <returns>The lower triangle of this matrix.</returns>
        public override Matrix<Complex> StrictlyLowerTriangle()
        {
            var ret = new DenseMatrix(Order);
            for (var row = 0; row < Order; row++)
            {
                for (var column = 0; column < row; column++)
                {
                    ret[row, column] = At(row, column);
                }
            }

            return ret;
        }

        /// <summary>
        /// Returns a new matrix containing the upper triangle of this matrix.
        /// </summary>
        /// <returns>The upper triangle of this matrix.</returns>   
        public override Matrix<Complex> UpperTriangle()
        {
            var ret = new DenseMatrix(Order);
            for (var row = 0; row < Order; row++)
            {
                for (var column = row; column < Order; column++)
                {
                    ret[row, column] = At(row, column);
                }
            }

            return ret;
        }

        /// <summary>
        /// Returns a new matrix containing the upper triangle of this matrix. The new matrix
        /// does not contain the diagonal elements of this matrix.
        /// </summary>
        /// <returns>The upper triangle of this matrix.</returns>
        public override Matrix<Complex> StrictlyUpperTriangle()
        {
            var ret = new DenseMatrix(Order);
            for (var row = 0; row < Order; row++)
            {
                for (var column = row + 1; column < Order; column++)
                {
                    ret[row, column] = At(row, column);
                }
            }

            return ret;
        }

        /// <summary>
        /// Computes the trace of this matrix.
        /// </summary>
        /// <returns>The trace of this matrix</returns>
        public override Complex Trace()
        {
            // Matrix is always square.
            var sum = Complex.Zero;
            for (var i = 0; i < RowCount; i++)
            {
                sum += At(i, i);
            }

            return sum;
        }

        /// <summary>
        /// Populates a symmetric matrix with random elements.
        /// </summary>
        /// <param name="matrix">The symmetric matrix to populate.</param>
        /// <param name="distribution">Continuous Random Distribution to generate elements from.</param>
        protected override void DoRandom(Matrix<Complex> matrix, IContinuousDistribution distribution)
        {
            var denseMatrix = matrix as SymmetricDenseMatrix;

            if (denseMatrix == null)
            {
                base.DoRandom(matrix, distribution);
            }
            else
            {
                for (var i = 0; i < denseMatrix._data.Length; i++)
                {
                    denseMatrix._data[i] = distribution.Sample();
                }
            }
        }

        /// <summary>
        /// Populates a symmetric matrix with random elements.
        /// </summary>
        /// <param name="matrix">The symmetric matrix to populate.</param>
        /// <param name="distribution">Continuous Random Distribution to generate elements from.</param>
        protected override void DoRandom(Matrix<Complex> matrix, IDiscreteDistribution distribution)
        {
            var denseMatrix = matrix as SymmetricDenseMatrix;

            if (denseMatrix == null)
            {
                base.DoRandom(matrix, distribution);
            }
            else
            {
                for (var i = 0; i < denseMatrix._data.Length; i++)
                {
                    denseMatrix._data[i] = distribution.Sample();
                }
            }
        }

        /// <summary>
        /// Adds two matrices together and returns the results.
        /// </summary>
        /// <remarks>This operator will allocate new memory for the result. It will
        /// choose the representation of either <paramref name="leftSide"/> or <paramref name="rightSide"/> depending on which
        /// is denser.</remarks>
        /// <param name="leftSide">The left matrix to add.</param>
        /// <param name="rightSide">The right matrix to add.</param>
        /// <returns>The result of the addition.</returns>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="leftSide"/> and <paramref name="rightSide"/> don't have the same dimensions.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="leftSide"/> or <paramref name="rightSide"/> is <see langword="null" />.</exception>
        public static SymmetricDenseMatrix operator +(SymmetricDenseMatrix leftSide, SymmetricDenseMatrix rightSide)
        {
            if (rightSide == null)
            {
                throw new ArgumentNullException("rightSide");
            }

            if (leftSide == null)
            {
                throw new ArgumentNullException("leftSide");
            }

            if (leftSide.RowCount != rightSide.RowCount)
            {
                throw new ArgumentOutOfRangeException(Resources.ArgumentMatrixDimensions);
            }

            return (SymmetricDenseMatrix)leftSide.Add(rightSide);
        }

        /// <summary>
        /// Returns a <strong>Matrix</strong> containing the same values of <paramref name="rightSide"/>. 
        /// </summary>
        /// <param name="rightSide">The matrix to get the values from.</param>
        /// <returns>A matrix containing a the same values as <paramref name="rightSide"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="rightSide"/> is <see langword="null" />.</exception>
        public static SymmetricDenseMatrix operator +(SymmetricDenseMatrix rightSide)
        {
            if (rightSide == null)
            {
                throw new ArgumentNullException("rightSide");
            }

            return (SymmetricDenseMatrix)rightSide.Clone();
        }

        /// <summary>
        /// Subtracts two matrices together and returns the results.
        /// </summary>
        /// <remarks>This operator will allocate new memory for the result. It will
        /// choose the representation of either <paramref name="leftSide"/> or <paramref name="rightSide"/> depending on which
        /// is denser.</remarks>
        /// <param name="leftSide">The left matrix to subtract.</param>
        /// <param name="rightSide">The right matrix to subtract.</param>
        /// <returns>The result of the addition.</returns>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="leftSide"/> and <paramref name="rightSide"/> don't have the same dimensions.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="leftSide"/> or <paramref name="rightSide"/> is <see langword="null" />.</exception>
        public static SymmetricDenseMatrix operator -(SymmetricDenseMatrix leftSide, SymmetricDenseMatrix rightSide)
        {
            if (rightSide == null)
            {
                throw new ArgumentNullException("rightSide");
            }

            if (leftSide == null)
            {
                throw new ArgumentNullException("leftSide");
            }

            if (leftSide.RowCount != rightSide.RowCount)
            {
                throw new ArgumentOutOfRangeException(Resources.ArgumentMatrixDimensions);
            }

            return (SymmetricDenseMatrix)leftSide.Subtract(rightSide);
        }

        /// <summary>
        /// Negates each element of the matrix.
        /// </summary>
        /// <param name="rightSide">The matrix to negate.</param>
        /// <returns>A matrix containing the negated values.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="rightSide"/> is <see langword="null" />.</exception>
        public static SymmetricDenseMatrix operator -(SymmetricDenseMatrix rightSide)
        {
            if (rightSide == null)
            {
                throw new ArgumentNullException("rightSide");
            }

            return (SymmetricDenseMatrix)rightSide.Negate();
        }

        /// <summary>
        /// Multiplies a <strong>Matrix</strong> by a constant and returns the result.
        /// </summary>
        /// <param name="leftSide">The matrix to multiply.</param>
        /// <param name="rightSide">The constant to multiply the matrix by.</param>
        /// <returns>The result of the multiplication.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="leftSide"/> is <see langword="null" />.</exception>
        public static SymmetricDenseMatrix operator *(SymmetricDenseMatrix leftSide, Complex rightSide)
        {
            if (leftSide == null)
            {
                throw new ArgumentNullException("leftSide");
            }

            return (SymmetricDenseMatrix)leftSide.Multiply(rightSide);
        }

        /// <summary>
        /// Multiplies a <strong>Matrix</strong> by a constant and returns the result.
        /// </summary>
        /// <param name="leftSide">The matrix to multiply.</param>
        /// <param name="rightSide">The constant to multiply the matrix by.</param>
        /// <returns>The result of the multiplication.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="rightSide"/> is <see langword="null" />.</exception>
        public static SymmetricDenseMatrix operator *(Complex leftSide, SymmetricDenseMatrix rightSide)
        {
            if (rightSide == null)
            {
                throw new ArgumentNullException("rightSide");
            }

            return (SymmetricDenseMatrix)rightSide.Multiply(leftSide);
        }

        /// <summary>
        /// Multiplies two matrices.
        /// </summary>
        /// <remarks>This operator will allocate new memory for the result. It will
        /// choose the representation of either <paramref name="leftSide"/> or <paramref name="rightSide"/> depending on which
        /// is denser.</remarks>
        /// <param name="leftSide">The left matrix to multiply.</param>
        /// <param name="rightSide">The right matrix to multiply.</param>
        /// <returns>The result of multiplication.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="leftSide"/> or <paramref name="rightSide"/> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException">If the dimensions of <paramref name="leftSide"/> or <paramref name="rightSide"/> don't conform.</exception>
        public static SymmetricDenseMatrix operator *(SymmetricDenseMatrix leftSide, SymmetricDenseMatrix rightSide)
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

            return (SymmetricDenseMatrix)leftSide.Multiply(rightSide);
        }

        /// <summary>
        /// Multiplies a <strong>Matrix</strong> and a Vector.
        /// </summary>
        /// <param name="leftSide">The matrix to multiply.</param>
        /// <param name="rightSide">The vector to multiply.</param>
        /// <returns>The result of multiplication.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="leftSide"/> or <paramref name="rightSide"/> is <see langword="null" />.</exception>
        public static DenseVector operator *(SymmetricDenseMatrix leftSide, DenseVector rightSide)
        {
            if (leftSide == null)
            {
                throw new ArgumentNullException("leftSide");
            }

            return (DenseVector)leftSide.Multiply(rightSide);
        }

        /// <summary>
        /// Multiplies a Vector and a <strong>Matrix</strong>.
        /// </summary>
        /// <param name="leftSide">The vector to multiply.</param>
        /// <param name="rightSide">The matrix to multiply.</param>
        /// <returns>The result of multiplication.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="leftSide"/> or <paramref name="rightSide"/> is <see langword="null" />.</exception>
        public static DenseVector operator *(DenseVector leftSide, SymmetricDenseMatrix rightSide)
        {
            if (rightSide == null)
            {
                throw new ArgumentNullException("rightSide");
            }

            return (DenseVector)rightSide.LeftMultiply(leftSide);
        }

        /// <summary>
        /// Multiplies a <strong>Matrix</strong> by a constant and returns the result.
        /// </summary>
        /// <param name="leftSide">The matrix to multiply.</param>
        /// <param name="rightSide">The constant to multiply the matrix by.</param>
        /// <returns>The result of the multiplication.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="leftSide"/> is <see langword="null" />.</exception>
        public static SymmetricDenseMatrix operator %(SymmetricDenseMatrix leftSide, Complex rightSide)
        {
            if (leftSide == null)
            {
                throw new ArgumentNullException("leftSide");
            }

            return (SymmetricDenseMatrix)leftSide.Modulus(rightSide);
        }
    }
}