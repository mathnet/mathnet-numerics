// <copyright file="LinearAlgebraProviderTests.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
//
// Copyright (c) 2009-2013 Math.NET
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
// </copyright>

using System;
using System.Collections.Generic;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Factorization;
using MathNet.Numerics.LinearAlgebra.Integer;
using MathNet.Numerics.Providers.LinearAlgebra;
using NUnit.Framework;

namespace MathNet.Numerics.UnitTests.LinearAlgebraProviderTests.Integer
{
    /// <summary>
    /// Base class for linear algebra provider tests.
    /// </summary>
    [TestFixture, Category("LAProvider")]
    public class LinearAlgebraProviderTests
    {
        /// <summary>
        /// The Y int test vector.
        /// </summary>
        readonly int[] _y = {11, 22, 33, 44, 55};

        /// <summary>
        /// The X int test vector.
        /// </summary>
        readonly int[] _x = {66, 77, 88, 99, 101};

        static readonly IContinuousDistribution Dist = new Normal(0.0, 100.0);      // so values can be non-zeros when truncated to int

        /// <summary>
        /// Test matrix to use.
        /// </summary>
        readonly IDictionary<string, DenseMatrix> _matrices = new Dictionary<string, DenseMatrix>
            {
                {"Singular3x3", DenseMatrix.OfArray(new[,] {{1, 1, 2}, {1, 1, 2}, {1, 1, 2}})},
                {"Square3x3", DenseMatrix.OfArray(new[,] {{-11, -22, -33}, {00, 11, 22}, {-44, 55, 66}})},
                {"Square4x4", DenseMatrix.OfArray(new[,] {{-11, -22, -33, -44}, {00, 11, 22, 33}, {10, 21, 62, 43}, {-44, 55, 66, -77}})},
                {"Singular4x4", DenseMatrix.OfArray(new[,] {{-11, -22, -33, -44}, {-11, -22, -33, -44}, {-11, -22, -33, -44}, {-11, -22, -33, -44}})},
                {"Tall3x2", DenseMatrix.OfArray(new[,] {{-11, -22}, {00, 11}, {-44, 55}})},
                {"Wide2x3", DenseMatrix.OfArray(new[,] {{-11, -22, -33}, {00, 11, 22}})},
                {"Tall50000x10", DenseMatrix.CreateRandom(50000, 10, Dist)},
                {"Wide10x50000", DenseMatrix.CreateRandom(10, 50000, Dist)},
                {"Square1000x1000", DenseMatrix.CreateRandom(1000, 1000, Dist)}
            };

        static readonly int _scale = 3;    // Math.PI was used in Single and Double (as an inline value)

        /// <summary>
        /// Can add a vector to scaled vector
        /// </summary>
        [Test]
        public void CanAddVectorToScaledVectorSingle()
        {
            var result = new int[_y.Length];

            Control.LinearAlgebraProvider.AddVectorToScaledVector(_y, 0, _x, result);
            for (var i = 0; i < _y.Length; i++)
            {
                Assert.AreEqual(_y[i], result[i]);
            }

            Array.Copy(_y, result, _y.Length);
            Control.LinearAlgebraProvider.AddVectorToScaledVector(result, 1, _x, result);
            for (var i = 0; i < _y.Length; i++)
            {
                Assert.AreEqual(_y[i] + _x[i], result[i]);
            }

            Array.Copy(_y, result, _y.Length);
            Control.LinearAlgebraProvider.AddVectorToScaledVector(result, _scale, _x, result);
            for (var i = 0; i < _y.Length; i++)
            {
                AssertHelpers.AlmostEqualRelative(_y[i] + (_scale*_x[i]), result[i], 5);
            }
        }

        /// <summary>
        /// Can scale an array.
        /// </summary>
        [Test]
        public void CanScaleArray()
        {
            var result = new int[_y.Length];

            Control.LinearAlgebraProvider.ScaleArray(1, _y, result);
            for (var i = 0; i < _y.Length; i++)
            {
                Assert.AreEqual(_y[i], result[i]);
            }

            Array.Copy(_y, result, _y.Length);
            Control.LinearAlgebraProvider.ScaleArray(_scale, result, result);
            for (var i = 0; i < _y.Length; i++)
            {
              AssertHelpers.AlmostEqualRelative(_y[i] * _scale, result[i], 5);
            }
        }

        /// <summary>
        /// Can compute the dot product.
        /// </summary>
        [Test]
        public void CanComputeDotProduct()
        {
            var result = Control.LinearAlgebraProvider.DotProduct(_x, _y);
            Assert.AreEqual(15235, result);
        }

        /// <summary>
        /// Can add two arrays.
        /// </summary>
        [Test]
        public void CanAddArrays()
        {
            var result = new int[_y.Length];
            Control.LinearAlgebraProvider.AddArrays(_x, _y, result);
            for (var i = 0; i < result.Length; i++)
            {
                Assert.AreEqual(_x[i] + _y[i], result[i]);
            }
        }

        /// <summary>
        /// Can subtract two arrays.
        /// </summary>
        [Test]
        public void CanSubtractArrays()
        {
            var result = new int[_y.Length];
            Control.LinearAlgebraProvider.SubtractArrays(_x, _y, result);
            for (var i = 0; i < result.Length; i++)
            {
                Assert.AreEqual(_x[i] - _y[i], result[i]);
            }
        }

        /// <summary>
        /// Can pointwise multiply two arrays.
        /// </summary>
        [Test]
        public void CanPointWiseMultiplyArrays()
        {
            var result = new int[_y.Length];
            Control.LinearAlgebraProvider.PointWiseMultiplyArrays(_x, _y, result);
            for (var i = 0; i < result.Length; i++)
            {
                Assert.AreEqual(_x[i]*_y[i], result[i]);
            }
        }

        /// <summary>
        /// Can pointwise divide two arrays.
        /// </summary>
        [Test]
        public void CanPointWiseDivideArrays()
        {
            var result = new int[_y.Length];
            //
            Control.LinearAlgebraProvider.PointWiseDivideArrays(_y, _x, result);
            for (var i = 0; i < result.Length; i++)
            {
                Assert.AreEqual(_y[i]/_x[i], result[i]);
            }
        }

        /// <summary>
        /// Can compute L1 norm.
        /// </summary>
        [Test]
        public void CanComputeMatrixL1Norm()
        {
            var matrix = _matrices["Square3x3"];
            var norm = Control.LinearAlgebraProvider.MatrixNorm(Norm.OneNorm, matrix.RowCount, matrix.ColumnCount, matrix.Values);
            Assert.AreEqual(121, norm);
        }

        /// <summary>
        /// Can compute Frobenius norm.
        /// </summary>
        [Test]
        public void CanComputeMatrixFrobeniusNorm()
        {
            var matrix = _matrices["Square3x3"];
            var norm = Control.LinearAlgebraProvider.MatrixNorm(Norm.FrobeniusNorm, matrix.RowCount, matrix.ColumnCount, matrix.Values);
            AssertHelpers.AlmostEqual(107.77754868246, norm, 5);
        }

        /// <summary>
        /// Can compute Infinity norm.
        /// </summary>
        [Test]
        public void CanComputeMatrixInfinityNorm()
        {
            var matrix = _matrices["Square3x3"];
            var norm = Control.LinearAlgebraProvider.MatrixNorm(Norm.InfinityNorm, matrix.RowCount, matrix.ColumnCount, matrix.Values);
            AssertHelpers.AlmostEqual(165, norm, 7);
        }

        /// <summary>
        /// Can multiply two square matrices.
        /// </summary>
        [Test]
        public void CanMultiplySquareMatrices()
        {
            var x = _matrices["Singular3x3"];
            var y = _matrices["Square3x3"];
            var c = new DenseMatrix(x.RowCount, y.ColumnCount);

            Control.LinearAlgebraProvider.MatrixMultiply(x.Values, x.RowCount, x.ColumnCount, y.Values, y.RowCount, y.ColumnCount, c.Values);

            for (var i = 0; i < c.RowCount; i++)
            {
                for (var j = 0; j < c.ColumnCount; j++)
                {
                  Assert.AreEqual(x.Row(i) * y.Column(j), c[i, j]);
                }
            }
        }

        /// <summary>
        /// Can multiply a wide and tall matrix.
        /// </summary>
        [Test]
        public void CanMultiplyWideAndTallMatrices()
        {
            var x = _matrices["Wide2x3"];
            var y = _matrices["Tall3x2"];
            var c = new DenseMatrix(x.RowCount, y.ColumnCount);

            Control.LinearAlgebraProvider.MatrixMultiply(x.Values, x.RowCount, x.ColumnCount, y.Values, y.RowCount, y.ColumnCount, c.Values);

            for (var i = 0; i < c.RowCount; i++)
            {
                for (var j = 0; j < c.ColumnCount; j++)
                {
                  Assert.AreEqual(x.Row(i) * y.Column(j), c[i, j]);
                }
            }
        }

        /// <summary>
        /// Can multiply a tall and wide matrix.
        /// </summary>
        [Test]
        public void CanMultiplyTallAndWideMatrices()
        {
            var x = _matrices["Tall3x2"];
            var y = _matrices["Wide2x3"];
            var c = new DenseMatrix(x.RowCount, y.ColumnCount);

            Control.LinearAlgebraProvider.MatrixMultiply(x.Values, x.RowCount, x.ColumnCount, y.Values, y.RowCount, y.ColumnCount, c.Values);

            for (var i = 0; i < c.RowCount; i++)
            {
                for (var j = 0; j < c.ColumnCount; j++)
                {
                  Assert.AreEqual(x.Row(i) * y.Column(j), c[i, j]);
                }
            }
        }

        /// <summary>
        /// Can multiply two square matrices.
        /// </summary>
        [Test]
        public void CanMultiplySquareMatricesWithUpdate()
        {
            var x = _matrices["Singular3x3"];
            var y = _matrices["Square3x3"];
            var c = new DenseMatrix(x.RowCount, y.ColumnCount);

            Control.LinearAlgebraProvider.MatrixMultiplyWithUpdate(Transpose.DontTranspose, Transpose.DontTranspose, 22, x.Values, x.RowCount, x.ColumnCount, y.Values, y.RowCount, y.ColumnCount, 1, c.Values);

            for (var i = 0; i < c.RowCount; i++)
            {
                for (var j = 0; j < c.ColumnCount; j++)
                {
                  Assert.AreEqual(22 * x.Row(i) * y.Column(j), c[i, j]);
                }
            }
        }

        /// <summary>
        /// Can multiply a wide and tall matrix.
        /// </summary>
        [Test]
        public void CanMultiplyWideAndTallMatricesWithUpdate()
        {
            var x = _matrices["Wide2x3"];
            var y = _matrices["Tall3x2"];
            var c = new DenseMatrix(x.RowCount, y.ColumnCount);

            Control.LinearAlgebraProvider.MatrixMultiplyWithUpdate(Transpose.DontTranspose, Transpose.DontTranspose, 22, x.Values, x.RowCount, x.ColumnCount, y.Values, y.RowCount, y.ColumnCount, 1, c.Values);

            for (var i = 0; i < c.RowCount; i++)
            {
                for (var j = 0; j < c.ColumnCount; j++)
                {
                  Assert.AreEqual(22 * x.Row(i) * y.Column(j), c[i, j]);
                }
            }
        }

        /// <summary>
        /// Can multiply a tall and wide matrix.
        /// </summary>
        [Test]
        public void CanMultiplyTallAndWideMatricesWithUpdate()
        {
            var x = _matrices["Tall3x2"];
            var y = _matrices["Wide2x3"];
            var c = new DenseMatrix(x.RowCount, y.ColumnCount);

            Control.LinearAlgebraProvider.MatrixMultiplyWithUpdate(Transpose.DontTranspose, Transpose.DontTranspose, 22, x.Values, x.RowCount, x.ColumnCount, y.Values, y.RowCount, y.ColumnCount, 1, c.Values);

            for (var i = 0; i < c.RowCount; i++)
            {
                for (var j = 0; j < c.ColumnCount; j++)
                {
                    var test = 22*x.Row(i)*y.Column(j);

                    ////// if they are both close to zero, skip
                    ////if (Math.Abs(test) < 1e-7 && Math.Abs(c[i, j]) < 1e-7)
                    ////{
                    ////    continue;
                    ////}

                    Assert.AreEqual(22 * x.Row(i) * y.Column(j), c[i, j]);
                }
            }
        }

        /// <summary>
		/// Compute the LU factor of a matrix always throws <c>InvalidOperationException</c>
        /// </summary>
        [Test]
		public void ComputeLuFactorThrowsInvalidOperationException()
        {
            var matrix = _matrices["Square3x3"];
            var a = new int[matrix.RowCount*matrix.RowCount];
            Array.Copy(matrix.Values, a, a.Length);

            var ipiv = new int[matrix.RowCount];

            Assert.That(() => Control.LinearAlgebraProvider.LUFactor(a, matrix.RowCount, ipiv), Throws.InvalidOperationException);
        }

        /// <summary>
        /// Compute the inverse of a matrix using LU factorization always throws <c>InvalidOperationException</c>.
        /// </summary>
        [Test]
		public void ComputeLuInverseThrowsInvalidOperationException()
        {
            var matrix = _matrices["Square3x3"];
            var a = new int[matrix.RowCount*matrix.RowCount];
            Array.Copy(matrix.Values, a, a.Length);

            Assert.That(() => Control.LinearAlgebraProvider.LUInverse(a, matrix.RowCount), Throws.InvalidOperationException);
        }

        /// <summary>
        /// Compute the inverse of a matrix using LU factorization
        /// using a previously factored matrix always throws <c>InvalidOperationException</c>.
        /// </summary>
        [Test]
		public void ComputeLuInverseOnFactoredMatrixThrowsInvalidOperationException()
        {
            var matrix = _matrices["Square3x3"];
            var a = new int[matrix.RowCount*matrix.RowCount];
            Array.Copy(matrix.Values, a, a.Length);

            var ipiv = new int[matrix.RowCount];

            // The first of these is irrelevant since LUInverseFactored should immediately throw the exception
            Assert.That(() => Control.LinearAlgebraProvider.LUFactor(a, matrix.RowCount, ipiv), Throws.InvalidOperationException);
            Assert.That(() => Control.LinearAlgebraProvider.LUInverseFactored(a, matrix.RowCount, ipiv), Throws.InvalidOperationException);
        }

        /// <summary>
        /// Compute the inverse of a matrix using LU factorization
        /// with a work array always throws <c>InvalidOperationException</c>.
        /// </summary>
        [Test]
		public void ComputeLuInverseWithWorkArrayThrowsInvalidOperationException()
        {
            var matrix = _matrices["Square3x3"];
            var a = new int[matrix.RowCount*matrix.RowCount];
            Array.Copy(matrix.Values, a, a.Length);

            var work = new int[matrix.RowCount];
            Assert.That(() => Control.LinearAlgebraProvider.LUFactor(a, matrix.RowCount, work), Throws.InvalidOperationException);
        }

        /// <summary>
        /// Compute the inverse of a matrix using LU factorization
        /// using a previously factored matrix with a work array always throws <c>InvalidOperationException</c>.
        /// </summary>
        [Test]
		public void ComputeLuInverseOnFactoredMatrixWithWorkArrayThrowsInvalidOperationException()
        {
            var matrix = _matrices["Square3x3"];
            var a = new int[matrix.RowCount*matrix.RowCount];
            Array.Copy(matrix.Values, a, a.Length);

            var ipiv = new int[matrix.RowCount];

            // The first of these is irrelevant since LUInverseFactored should immediately throw the exception
            Assert.That(() => Control.LinearAlgebraProvider.LUFactor(a, matrix.RowCount, ipiv), Throws.InvalidOperationException);

            var work = new int[matrix.RowCount];
            Assert.That(() => Control.LinearAlgebraProvider.LUInverseFactored(a, matrix.RowCount, ipiv, work), Throws.InvalidOperationException);
        }

        /// <summary>
        /// Solve Ax=b using LU factorization always throws <c>InvalidOperationException</c>.
        /// </summary>
        [Test]
		public void SolveUsingLUThrowsInvalidOperationException()
        {
            var matrix = _matrices["Square3x3"];
            var a = new int[matrix.RowCount*matrix.RowCount];
            Array.Copy(matrix.Values, a, a.Length);

            var b = new[] {1, 2, 3, 4, 5, 6};
            Assert.That(() => Control.LinearAlgebraProvider.LUSolve(2, a, matrix.RowCount, b), Throws.InvalidOperationException);
        }

        /// <summary>
        /// Solve Ax=b using LU factorization using a factored matrix always throws <c>InvalidOperationException</c>.
        /// </summary>
        [Test]
		public void SolveUsingLUOnFactoredMatrixThrowsInvalidOperationException()
        {
            var matrix = _matrices["Square3x3"];
            var a = new int[matrix.RowCount*matrix.RowCount];
            Array.Copy(matrix.Values, a, a.Length);

            var ipiv = new int[matrix.RowCount];
            Assert.That(() => Control.LinearAlgebraProvider.LUFactor(a, matrix.RowCount, ipiv), Throws.InvalidOperationException);

            var b = new[] {1, 2, 3, 4, 5, 6};
            Assert.That(() => Control.LinearAlgebraProvider.LUSolveFactored(2, a, matrix.RowCount, ipiv, b), Throws.InvalidOperationException);
        }

        /// <summary>
        /// Compute the <c>Cholesky</c> factorization always throws <c>InvalidOperationException</c>.
        /// </summary>
        [Test]
		public void ComputeCholeskyFactorThrowsInvalidOperationException()
        {
            var matrix = new int[] {1, 1, 1, 1, 1, 5, 5, 5, 1, 5, 14, 14, 1, 5, 14, 15};
            Assert.That(() => Control.LinearAlgebraProvider.CholeskyFactor(matrix, 4), Throws.InvalidOperationException);
        }

        /// <summary>
        /// Solve Ax=b using Cholesky factorization always throws <c>InvalidOperationException</c>.
        /// </summary>
        [Test]
		public void SolveUsingCholeskyThrowsInvalidOperationException()
        {
            var matrix = new DenseMatrix(3, 3, new int[] {1, 1, 1, 1, 2, 3, 1, 3, 6});
            var a = new int[] {1, 1, 1, 1, 2, 3, 1, 3, 6};

            var b = new[] {1, 2, 3, 4, 5, 6};
            Assert.That(() => Control.LinearAlgebraProvider.CholeskySolve(a, 3, b, 2), Throws.InvalidOperationException);
        }

        /// <summary>
        /// Solve Ax=b using LU factorization using a factored matrix always throws <c>InvalidOperationException</c>.
        /// </summary>
        [Test]
		public void SolveUsingCholeskyOnFactoredMatrixThrowsInvalidOperationException()
        {
            var a = new int[] {1, 1, 1, 1, 2, 3, 1, 3, 6};

            Control.LinearAlgebraProvider.CholeskyFactor(a, 3);

            var b = new[] {1, 2, 3, 4, 5, 6};
            Assert.That(() => Control.LinearAlgebraProvider.CholeskySolveFactored(a, 3, b, 2), Throws.InvalidOperationException);
        }

        /// <summary>
        /// Compute QR factorization of a square matrix always throws <c>InvalidOperationException</c>.
        /// </summary>
        [Test]
		public void ComputeQRFactorSquareMatrixThrowsInvalidOperationException()
        {
            var matrix = _matrices["Square3x3"];
            var r = new int[matrix.RowCount*matrix.ColumnCount];
            Array.Copy(matrix.Values, r, r.Length);

            var tau = new int[3];
            var q = new int[matrix.RowCount*matrix.RowCount];
            Assert.That(() => Control.LinearAlgebraProvider.QRFactor(r, matrix.RowCount, matrix.ColumnCount, q, tau), Throws.InvalidOperationException);
        }

        /// <summary>
		/// Compute QR factorization of a tall matrix always throws <c>InvalidOperationException</c>.
        /// </summary>
        [Test]
        public void ComputeQRFactorTallMatrixThrowsInvalidOperationException()
        {
            var matrix = _matrices["Tall3x2"];
            var r = new int[matrix.RowCount*matrix.ColumnCount];
            Array.Copy(matrix.Values, r, r.Length);

            var tau = new int[3];
            var q = new int[matrix.RowCount*matrix.RowCount];
            Assert.That(() => Control.LinearAlgebraProvider.QRFactor(r, matrix.RowCount, matrix.ColumnCount, q, tau), Throws.InvalidOperationException);
        }

        /// <summary>
		/// Compute QR factorization of a wide matrix always throws <c>InvalidOperationException</c>.
        /// </summary>
        [Test]
        public void ComputeQRFactorWideMatrixThrowsInvalidOperationException()
        {
            var matrix = _matrices["Wide2x3"];
            var r = new int[matrix.RowCount*matrix.ColumnCount];
            Array.Copy(matrix.Values, r, r.Length);

            var tau = new int[3];
            var q = new int[matrix.RowCount*matrix.RowCount];
            Assert.That(() => Control.LinearAlgebraProvider.QRFactor(r, matrix.RowCount, matrix.ColumnCount, q, tau), Throws.InvalidOperationException);
        }

        /// <summary>
		/// Compute QR factorization of a square matrix using a work array always throws <c>InvalidOperationException</c>.
        /// </summary>
        [Test]
        public void ComputeQRFactorSquareMatrixWithWorkArrayThrowsInvalidOperationException()
        {
            var matrix = _matrices["Square3x3"];
            var r = new int[matrix.RowCount*matrix.ColumnCount];
            Array.Copy(matrix.Values, r, r.Length);

            var tau = new int[3];
            var q = new int[matrix.RowCount*matrix.RowCount];
            var work = new int[matrix.ColumnCount*Control.BlockSize];
            Assert.That(() => Control.LinearAlgebraProvider.QRFactor(r, matrix.RowCount, matrix.ColumnCount, q, tau, work), Throws.InvalidOperationException);
        }

        /// <summary>
		/// Compute QR factorization of a tall matrix using a work matrix always throws <c>InvalidOperationException</c>.
        /// </summary>
        [Test]
        public void CanComputeQRFactorTallMatrixWithWorkArrayThrowsInvalidOperationException()
        {
            var matrix = _matrices["Tall3x2"];
            var r = new int[matrix.RowCount*matrix.ColumnCount];
            Array.Copy(matrix.Values, r, r.Length);

            var tau = new int[3];
            var q = new int[matrix.RowCount*matrix.RowCount];
            var work = new int[matrix.ColumnCount*Control.BlockSize];
            Assert.That(() => Control.LinearAlgebraProvider.QRFactor(r, matrix.RowCount, matrix.ColumnCount, q, tau, work), Throws.InvalidOperationException);
        }

        /// <summary>
		/// Compute QR factorization of a wide matrix using a work matrix always throws <c>InvalidOperationException</c>.
        /// </summary>
        [Test]
        public void ComputeQRFactorWideMatrixWithWorkArrayThrowsInvalidOperationException()
        {
            var matrix = _matrices["Wide2x3"];
            var r = new int[matrix.RowCount*matrix.ColumnCount];
            Array.Copy(matrix.Values, r, r.Length);

            var tau = new int[3];
            var q = new int[matrix.RowCount*matrix.RowCount];
            var work = new int[matrix.ColumnCount*Control.BlockSize];
            Assert.That(() => Control.LinearAlgebraProvider.QRFactor(r, matrix.RowCount, matrix.ColumnCount, q, tau, work), Throws.InvalidOperationException);
        }

        /// <summary>
		/// Compute thin QR factorization of a square matrix always throws <c>InvalidOperationException</c>.
        /// </summary>
        [Test]
        public void ComputeThinQRFactorSquareMatrixThrowsInvalidOperationException()
        {
            var matrix = _matrices["Square3x3"];
            var r = new int[matrix.ColumnCount*matrix.ColumnCount];
            var tau = new int[3];
            var q = new int[matrix.RowCount*matrix.ColumnCount];
            Array.Copy(matrix.Values, q, q.Length);

            Assert.That(() => Control.LinearAlgebraProvider.ThinQRFactor(q, matrix.RowCount, matrix.ColumnCount, r, tau), Throws.InvalidOperationException);
        }

        /// <summary>
		/// Compute thin QR factorization of a tall matrix always throws <c>InvalidOperationException</c>.
        /// </summary>
        [Test]
        public void ComputeThinQRFactorTallMatrixThrowsInvalidOperationException()
        {
            var matrix = _matrices["Tall3x2"];
            var r = new int[matrix.ColumnCount*matrix.ColumnCount];
            var tau = new int[3];
            var q = new int[matrix.RowCount*matrix.ColumnCount];
            Array.Copy(matrix.Values, q, q.Length);

            Assert.That(() => Control.LinearAlgebraProvider.ThinQRFactor(q, matrix.RowCount, matrix.ColumnCount, r, tau), Throws.InvalidOperationException);
        }

        /// <summary>
		/// Compute thin QR factorization of a square matrix using a work array always throws <c>InvalidOperationException</c>.
        /// </summary>
        [Test]
        public void ComputeThinQRFactorSquareMatrixWithWorkArrayThrowsInvalidOperationException()
        {
            var matrix = _matrices["Square3x3"];
            var r = new int[matrix.ColumnCount*matrix.ColumnCount];
            var tau = new int[3];
            var q = new int[matrix.RowCount*matrix.ColumnCount];
            Array.Copy(matrix.Values, q, q.Length);

            var work = new int[matrix.ColumnCount*Control.BlockSize];
            Assert.That(() => Control.LinearAlgebraProvider.ThinQRFactor(q, matrix.RowCount, matrix.ColumnCount, r, tau, work), Throws.InvalidOperationException);
        }

        /// <summary>
		/// Compute thin QR factorization of a tall matrix using a work matrix always throws <c>InvalidOperationException</c>.
        /// </summary>
        [Test]
        public void ComputeThinQRFactorTallMatrixWithWorkArrayThrowsInvalidOperationException()
        {
            var matrix = _matrices["Tall3x2"];
            var r = new int[matrix.ColumnCount*matrix.ColumnCount];
            var tau = new int[3];
            var q = new int[matrix.RowCount*matrix.ColumnCount];
            Array.Copy(matrix.Values, q, q.Length);

            var work = new int[matrix.ColumnCount*Control.BlockSize];
            Assert.That(() => Control.LinearAlgebraProvider.ThinQRFactor(q, matrix.RowCount, matrix.ColumnCount, r, tau, work), Throws.InvalidOperationException);
        }

        /// <summary>
		/// Solve Ax=b using QR factorization with a square A matrix always throws <c>InvalidOperationException</c>.
        /// </summary>
        [Test]
        public void SolveUsingQRSquareMatrixThrowsInvalidOperationException()
        {
            var matrix = _matrices["Square3x3"];
            var a = new int[matrix.RowCount*matrix.ColumnCount];
            Array.Copy(matrix.Values, a, a.Length);

            var b = new[] {1, 2, 3, 4, 5, 6};
            var x = new int[matrix.ColumnCount*2];
            Assert.That(() => Control.LinearAlgebraProvider.QRSolve(a, matrix.RowCount, matrix.ColumnCount, b, 2, x), Throws.InvalidOperationException);
        }

        /// <summary>
		/// Solve Ax=b using QR factorization with a tall A matrix always throws <c>InvalidOperationException</c>.
        /// </summary>
        [Test]
        public void SolveUsingQRTallMatrixThrowsInvalidOperationException()
        {
            var matrix = _matrices["Tall3x2"];
            var a = new int[matrix.RowCount*matrix.ColumnCount];
            Array.Copy(matrix.Values, a, a.Length);

            var b = new[] {1, 2, 3, 4, 5, 6};
            var x = new int[matrix.ColumnCount*2];
            Assert.That(() => Control.LinearAlgebraProvider.QRSolve(a, matrix.RowCount, matrix.ColumnCount, b, 2, x), Throws.InvalidOperationException);
        }

        /// <summary>
        /// Solve Ax=b using QR factorization with a square A matrix
		/// using a work array always throws <c>InvalidOperationException</c>.
        /// </summary>
        [Test]
        public void SolveUsingQRSquareMatrixUsingWorkArrayThrowsInvalidOperationException()
        {
            var matrix = _matrices["Square3x3"];
            var a = new int[matrix.RowCount*matrix.ColumnCount];
            Array.Copy(matrix.Values, a, a.Length);

            var b = new[] {1, 2, 3, 4, 5, 6};
            var x = new int[matrix.ColumnCount*2];
            var work = new int[matrix.RowCount*matrix.RowCount];
            Assert.That(() => Control.LinearAlgebraProvider.QRSolve(a, matrix.RowCount, matrix.ColumnCount, b, 2, x, work), Throws.InvalidOperationException);
        }

        /// <summary>
        /// Solve Ax=b using QR factorization with a tall A matrix
		/// using a work array always throws <c>InvalidOperationException</c>.
        /// </summary>
        [Test]
        public void SolveUsingQRTallMatrixUsingWorkArrayThrowsInvalidOperationException()
        {
            var matrix = _matrices["Tall3x2"];
            var a = new int[matrix.RowCount*matrix.ColumnCount];
            Array.Copy(matrix.Values, a, a.Length);

            var b = new[] {1, 2, 3, 4, 5, 6};
            var x = new int[matrix.ColumnCount*2];
            var work = new int[matrix.RowCount*matrix.RowCount];
            Assert.That(() => Control.LinearAlgebraProvider.QRSolve(a, matrix.RowCount, matrix.ColumnCount, b, 2, x, work), Throws.InvalidOperationException);
        }

        /// <summary>
        /// Solve Ax=b using QR factorization with a square A matrix
		/// using a factored A matrix always throws <c>InvalidOperationException</c>.
        /// </summary>
        [Test]
        public void SolveUsingQRSquareMatrixOnFactoredMatrixThrowsInvalidOperationException()
        {
            var matrix = _matrices["Square3x3"];
            var a = new int[matrix.RowCount*matrix.RowCount];
            Array.Copy(matrix.Values, a, a.Length);

            var tau = new int[matrix.ColumnCount];
            var q = new int[matrix.ColumnCount*matrix.ColumnCount];
            Assert.That(() => Control.LinearAlgebraProvider.QRFactor(a, matrix.RowCount, matrix.ColumnCount, q, tau), Throws.InvalidOperationException);

            var b = new[] {1, 2, 3, 4, 5, 6};
            var x = new int[matrix.ColumnCount*2];
            Assert.That(() => Control.LinearAlgebraProvider.QRSolveFactored(q, a, matrix.RowCount, matrix.ColumnCount, tau, b, 2, x), Throws.InvalidOperationException);
        }

        /// <summary>
        /// Solve Ax=b using QR factorization with a tall A matrix
		/// using a factored A matrix always throws <c>InvalidOperationException</c>.
        /// </summary>
        [Test]
        public void SolveUsingQRTallMatrixOnFactoredMatrixThrowsInvalidOperationException()
        {
            var matrix = _matrices["Tall3x2"];
            var a = new int[matrix.RowCount*matrix.ColumnCount];
            Array.Copy(matrix.Values, a, a.Length);

            var tau = new int[matrix.ColumnCount];
            var q = new int[matrix.RowCount*matrix.RowCount];
            Assert.That(() => Control.LinearAlgebraProvider.QRFactor(a, matrix.RowCount, matrix.ColumnCount, q, tau), Throws.InvalidOperationException);

            var b = new[] {1, 2, 3, 4, 5, 6};
            var x = new int[matrix.ColumnCount*2];
            Assert.That(() => Control.LinearAlgebraProvider.QRSolveFactored(q, a, matrix.RowCount, matrix.ColumnCount, tau, b, 2, x), Throws.InvalidOperationException);
        }

        /// <summary>
        /// Solve Ax=b using QR factorization with a square A matrix
		/// using a factored A matrix with a work array always throws <c>InvalidOperationException</c>.
        /// </summary>
        [Test]
        public void SolveUsingQRSquareMatrixOnFactoredMatrixWithWorkArrayThrowsInvalidOperationException()
        {
            var matrix = _matrices["Square3x3"];
            var a = new int[matrix.RowCount*matrix.RowCount];
            Array.Copy(matrix.Values, a, a.Length);

            var tau = new int[matrix.ColumnCount];
            var q = new int[matrix.ColumnCount*matrix.ColumnCount];
            var work = new int[2048];
            Assert.That(() => Control.LinearAlgebraProvider.QRFactor(a, matrix.RowCount, matrix.ColumnCount, q, tau, work), Throws.InvalidOperationException);

            var b = new[] {1, 2, 3, 4, 5, 6};
            var x = new int[matrix.ColumnCount*2];
            Assert.That(() => Control.LinearAlgebraProvider.QRSolveFactored(q, a, matrix.RowCount, matrix.ColumnCount, tau, b, 2, x, work), Throws.InvalidOperationException);
        }

        /// <summary>
        /// Solve Ax=b using QR factorization with a tall A matrix
		/// using a factored A matrix with a work array always throws <c>InvalidOperationException</c>.
        /// </summary>
        [Test]
        public void SolveUsingQRTallMatrixOnFactoredMatrixWithWorkArray()
        {
            var matrix = _matrices["Tall3x2"];
            var a = new int[matrix.RowCount*matrix.ColumnCount];
            Array.Copy(matrix.Values, a, a.Length);

            var tau = new int[matrix.ColumnCount];
            var q = new int[matrix.RowCount*matrix.RowCount];
            var work = new int[2048];
            Assert.That(() => Control.LinearAlgebraProvider.QRFactor(a, matrix.RowCount, matrix.ColumnCount, q, tau, work), Throws.InvalidOperationException);

            var b = new[] {1, 2, 3, 4, 5, 6};
            var x = new int[matrix.ColumnCount*2];
            Assert.That(() => Control.LinearAlgebraProvider.QRSolveFactored(q, a, matrix.RowCount, matrix.ColumnCount, tau, b, 2, x, work), Throws.InvalidOperationException);
        }

        /// <summary>
		/// Solve Ax=b using thin QR factorization with a square A matrix always throws <c>InvalidOperationException</c>.
        /// </summary>
        [Test]
        public void SolveUsingThinQRSquareMatrixThrowsInvalidOperationException()
        {
            var matrix = _matrices["Square3x3"];
            var a = new int[matrix.RowCount*matrix.ColumnCount];
            Array.Copy(matrix.Values, a, a.Length);

            var b = new[] {1, 2, 3, 4, 5, 6};
            var x = new int[matrix.ColumnCount*2];
            Assert.That(() => Control.LinearAlgebraProvider.QRSolve(a, matrix.RowCount, matrix.ColumnCount, b, 2, x, QRMethod.Thin), Throws.InvalidOperationException);
        }

        /// <summary>
		/// Solve Ax=b using thin QR factorization with a tall A matrix always throws <c>InvalidOperationException</c>.
        /// </summary>
        [Test]
        public void SolveUsingThinQRTallMatrixThrowsInvalidOperationException()
        {
            var matrix = _matrices["Tall3x2"];
            var a = new int[matrix.RowCount*matrix.ColumnCount];
            Array.Copy(matrix.Values, a, a.Length);

            var b = new[] {1, 2, 3, 4, 5, 6};
            var x = new int[matrix.ColumnCount*2];
            Assert.That(() => Control.LinearAlgebraProvider.QRSolve(a, matrix.RowCount, matrix.ColumnCount, b, 2, x, QRMethod.Thin), Throws.InvalidOperationException);
        }

        /// <summary>
        /// Solve Ax=b using thin QR factorization with a square A matrix
		/// using a work array always throws <c>InvalidOperationException</c>.
        /// </summary>
        [Test]
        public void SolveUsingThinQRSquareMatrixUsingWorkArrayThrowsInvalidOperationException()
        {
            var matrix = _matrices["Square3x3"];
            var a = new int[matrix.RowCount*matrix.ColumnCount];
            Array.Copy(matrix.Values, a, a.Length);

            var b = new[] {1, 2, 3, 4, 5, 6};
            var x = new int[matrix.ColumnCount*2];
            var work = new int[matrix.RowCount*matrix.ColumnCount];
            Assert.That(() => Control.LinearAlgebraProvider.QRSolve(a, matrix.RowCount, matrix.ColumnCount, b, 2, x, work, QRMethod.Thin), Throws.InvalidOperationException);
        }

        /// <summary>
        /// Solve Ax=b using thin QR factorization with a tall A matrix
		/// using a work array always throws <c>InvalidOperationException</c>.
        /// </summary>
        [Test]
        public void SolveUsingThinQRTallMatrixUsingWorkArrayThrowsInvalidOperationException()
        {
            var matrix = _matrices["Tall3x2"];
            var a = new int[matrix.RowCount*matrix.ColumnCount];
            Array.Copy(matrix.Values, a, a.Length);

            var b = new[] {1, 2, 3, 4, 5, 6};
            var x = new int[matrix.ColumnCount*2];
            var work = new int[matrix.RowCount*matrix.ColumnCount];
            Assert.That(() => Control.LinearAlgebraProvider.QRSolve(a, matrix.RowCount, matrix.ColumnCount, b, 2, x, work, QRMethod.Thin), Throws.InvalidOperationException);
        }

        /// <summary>
        /// Solve Ax=b using thin QR factorization with a square A matrix
		/// using a factored A matrix always throws <c>InvalidOperationException</c>.
        /// </summary>
        [Test]
        public void SolveUsingThinQRSquareMatrixOnFactoredMatrixThrowsInvalidOperationException()
        {
            var matrix = _matrices["Square3x3"];
            var a = new int[matrix.RowCount*matrix.ColumnCount];
            Array.Copy(matrix.Values, a, a.Length);

            var tau = new int[matrix.ColumnCount];
            var r = new int[matrix.ColumnCount*matrix.ColumnCount];
            Assert.That(() => Control.LinearAlgebraProvider.ThinQRFactor(a, matrix.RowCount, matrix.ColumnCount, r, tau), Throws.InvalidOperationException);

            var b = new[] {1, 2, 3, 4, 5, 6};
            var x = new int[matrix.ColumnCount*2];
            Assert.That(() => Control.LinearAlgebraProvider.QRSolveFactored(a, r, matrix.RowCount, matrix.ColumnCount, tau, b, 2, x, QRMethod.Thin), Throws.InvalidOperationException);
        }

        /// <summary>
        /// Solve Ax=b using thin QR factorization with a tall A matrix
		/// using a factored A matrix always throws <c>InvalidOperationException</c>.
        /// </summary>
        [Test]
        public void SolveUsingThinQRTallMatrixOnFactoredMatrixThrowsInvalidOperationException()
        {
            var matrix = _matrices["Tall3x2"];
            var a = new int[matrix.RowCount*matrix.ColumnCount];
            Array.Copy(matrix.Values, a, a.Length);

            var tau = new int[matrix.ColumnCount];
            var r = new int[matrix.ColumnCount*matrix.ColumnCount];
            Assert.That(() => Control.LinearAlgebraProvider.ThinQRFactor(a, matrix.RowCount, matrix.ColumnCount, r, tau), Throws.InvalidOperationException);

            var b = new[] {1, 2, 3, 4, 5, 6};
            var x = new int[matrix.ColumnCount*2];
            Assert.That(() => Control.LinearAlgebraProvider.QRSolveFactored(a, r, matrix.RowCount, matrix.ColumnCount, tau, b, 2, x, QRMethod.Thin), Throws.InvalidOperationException);
        }

        /// <summary>
        /// Solve Ax=b using thin QR factorization with a square A matrix
		/// using a factored A matrix with a work array always throws <c>InvalidOperationException</c>.
        /// </summary>
        [Test]
        public void SolveUsingThinQRSquareMatrixOnFactoredMatrixWithWorkArrayThrowsInvalidOperationException()
        {
            var matrix = _matrices["Square3x3"];
            var a = new int[matrix.RowCount*matrix.ColumnCount];
            Array.Copy(matrix.Values, a, a.Length);

            var tau = new int[matrix.ColumnCount];
            var r = new int[matrix.ColumnCount*matrix.ColumnCount];
            var work = new int[2048];
            Assert.That(() => Control.LinearAlgebraProvider.ThinQRFactor(a, matrix.RowCount, matrix.ColumnCount, r, tau, work), Throws.InvalidOperationException);

            var b = new[] {1, 2, 3, 4, 5, 6};
            var x = new int[matrix.ColumnCount*2];
            Assert.That(() => Control.LinearAlgebraProvider.QRSolveFactored(a, r, matrix.RowCount, matrix.ColumnCount, tau, b, 2, x, work, QRMethod.Thin), Throws.InvalidOperationException);
        }

        /// <summary>
        /// Solve Ax=b using thin QR factorization with a tall A matrix
		/// using a factored A matrix with a work array always throws <c>InvalidOperationException</c>.
        /// </summary>
        [Test]
        public void SolveUsingThinQRTallMatrixOnFactoredMatrixWithWorkArrayThrowsInvalidOperationException()
        {
            var matrix = _matrices["Tall3x2"];
            var a = new int[matrix.RowCount*matrix.ColumnCount];
            Array.Copy(matrix.Values, a, a.Length);

            var tau = new int[matrix.ColumnCount];
            var r = new int[matrix.ColumnCount*matrix.ColumnCount];
            var work = new int[2048];
            Assert.That(() => Control.LinearAlgebraProvider.ThinQRFactor(a, matrix.RowCount, matrix.ColumnCount, r, tau, work), Throws.InvalidOperationException);

            var b = new[] {1, 2, 3, 4, 5, 6};
            var x = new int[matrix.ColumnCount*2];
            Assert.That(() => Control.LinearAlgebraProvider.QRSolveFactored(a, r, matrix.RowCount, matrix.ColumnCount, tau, b, 2, x, work, QRMethod.Thin), Throws.InvalidOperationException);
        }

        /// <summary>
		/// Compute the SVD factorization of a square matrix always throws <c>InvalidOperationException</c>.
        /// </summary>
        [Test]
        public void ComputeSVDFactorizationOfSquareMatrixThrowsInvalidOperationException()
        {
            var matrix = _matrices["Square3x3"];
            var a = new int[matrix.RowCount*matrix.ColumnCount];
            Array.Copy(matrix.Values, a, a.Length);

            var s = new int[matrix.RowCount];
            var u = new int[matrix.RowCount*matrix.RowCount];
            var vt = new int[matrix.ColumnCount*matrix.ColumnCount];

            Assert.That(() => Control.LinearAlgebraProvider.SingularValueDecomposition(true, a, matrix.RowCount, matrix.ColumnCount, s, u, vt), Throws.InvalidOperationException);
        }

        /// <summary>
		/// Compute the SVD factorization of a tall matrix always throws <c>InvalidOperationException</c>.
        /// </summary>
        [Test]
        public void ComputeSVDFactorizationOfTallMatrixThrowsInvalidOperationException()
        {
            var matrix = _matrices["Tall3x2"];
            var a = new int[matrix.RowCount*matrix.ColumnCount];
            Array.Copy(matrix.Values, a, a.Length);

            var s = new int[matrix.ColumnCount];
            var u = new int[matrix.RowCount*matrix.RowCount];
            var vt = new int[matrix.ColumnCount*matrix.ColumnCount];

            Assert.That(() => Control.LinearAlgebraProvider.SingularValueDecomposition(true, a, matrix.RowCount, matrix.ColumnCount, s, u, vt), Throws.InvalidOperationException);
        }

        /// <summary>
		/// Compute the SVD factorization of a wide matrix always throws <c>InvalidOperationException</c>.
        /// </summary>
        [Test]
        public void ComputeSVDFactorizationOfWideMatrixThrowsInvalidOperationException()
        {
            var matrix = _matrices["Wide2x3"];
            var a = new int[matrix.RowCount*matrix.ColumnCount];
            Array.Copy(matrix.Values, a, a.Length);

            var s = new int[matrix.RowCount];
            var u = new int[matrix.RowCount*matrix.RowCount];
            var vt = new int[matrix.ColumnCount*matrix.ColumnCount];

            Assert.That(() => Control.LinearAlgebraProvider.SingularValueDecomposition(true, a, matrix.RowCount, matrix.ColumnCount, s, u, vt), Throws.InvalidOperationException);
        }

        /// <summary>
        /// Compute the SVD factorization of a square matrix using
		/// a work array always throws <c>InvalidOperationException</c>.
        /// </summary>
        [Test]
        public void ComputeSVDFactorizationOfSquareMatrixWithWorkArrayThrowsInvalidOperationException()
        {
            var matrix = _matrices["Square3x3"];
            var a = new int[matrix.RowCount*matrix.ColumnCount];
            Array.Copy(matrix.Values, a, a.Length);

            var s = new int[matrix.RowCount];
            var u = new int[matrix.RowCount*matrix.RowCount];
            var vt = new int[matrix.ColumnCount*matrix.ColumnCount];
            var work = new int[100];

            Assert.That(() => Control.LinearAlgebraProvider.SingularValueDecomposition(true, a, matrix.RowCount, matrix.ColumnCount, s, u, vt, work), Throws.InvalidOperationException);
        }

        /// <summary>
        /// Compute the SVD factorization of a tall matrix using
		/// a work array always throws <c>InvalidOperationException</c>.
        /// </summary>
        [Test]
        public void ComputeSVDFactorizationOfTallMatrixWithWorkArrayThrowsInvalidOperationException()
        {
            var matrix = _matrices["Tall3x2"];
            var a = new int[matrix.RowCount*matrix.ColumnCount];
            Array.Copy(matrix.Values, a, a.Length);

            var s = new int[matrix.ColumnCount];
            var u = new int[matrix.RowCount*matrix.RowCount];
            var vt = new int[matrix.ColumnCount*matrix.ColumnCount];
            var work = new int[100];

            Assert.That(() => Control.LinearAlgebraProvider.SingularValueDecomposition(true, a, matrix.RowCount, matrix.ColumnCount, s, u, vt, work), Throws.InvalidOperationException);
        }

        /// <summary>
        /// Compute the SVD factorization of a wide matrix using
		/// a work array always throws <c>InvalidOperationException</c>.
        /// </summary>
        [Test]
        public void ComputeSVDFactorizationOfWideMatrixWithWorkArrayThrowsInvalidOperationException()
        {
            var matrix = _matrices["Wide2x3"];
            var a = new int[matrix.RowCount*matrix.ColumnCount];
            Array.Copy(matrix.Values, a, a.Length);

            var s = new int[matrix.RowCount];
            var u = new int[matrix.RowCount*matrix.RowCount];
            var vt = new int[matrix.ColumnCount*matrix.ColumnCount];
            var work = new int[100];

            Assert.That(() => Control.LinearAlgebraProvider.SingularValueDecomposition(true, a, matrix.RowCount, matrix.ColumnCount, s, u, vt, work), Throws.InvalidOperationException);
        }

        /// <summary>
		/// Solve Ax=b using SVD factorization with a square A matrix always throws <c>InvalidOperationException</c>.
        /// </summary>
        [Test]
        public void SolveUsingSVDSquareMatrixThrowsInvalidOperationException()
        {
            var matrix = _matrices["Square3x3"];
            var a = new int[matrix.RowCount*matrix.ColumnCount];
            Array.Copy(matrix.Values, a, a.Length);

            var b = new[] {1, 2, 3, 4, 5, 6};
            var x = new int[matrix.ColumnCount*2];
            Assert.That(() => Control.LinearAlgebraProvider.SvdSolve(a, matrix.RowCount, matrix.ColumnCount, b, 2, x), Throws.InvalidOperationException);
        }

        /// <summary>
		/// Solve Ax=b using SVD factorization with a tall A matrix always throws <c>InvalidOperationException</c>.
        /// </summary>
        [Test]
        public void SolveUsingSVDTallMatrixThrowsInvalidOperationException()
        {
            var matrix = _matrices["Tall3x2"];
            var a = new int[matrix.RowCount*matrix.ColumnCount];
            Array.Copy(matrix.Values, a, a.Length);

            var b = new[] {1, 2, 3, 4, 5, 6};
            var x = new int[matrix.ColumnCount*2];
            Assert.That(() => Control.LinearAlgebraProvider.SvdSolve(a, matrix.RowCount, matrix.ColumnCount, b, 2, x), Throws.InvalidOperationException);
        }

        /// <summary>
        /// Solve Ax=b using SVD factorization with a square A matrix
		/// using a factored matrix always throws <c>InvalidOperationException</c>.
        /// </summary>
        [Test]
        public void SolveUsingSVDSquareMatrixOnFactoredMatrixThrowsInvalidOperationException()
        {
            var matrix = _matrices["Square3x3"];
            var a = new int[matrix.RowCount*matrix.ColumnCount];
            Array.Copy(matrix.Values, a, a.Length);

            var s = new int[matrix.RowCount];
            var u = new int[matrix.RowCount*matrix.RowCount];
            var vt = new int[matrix.ColumnCount*matrix.ColumnCount];

            Assert.That(() => Control.LinearAlgebraProvider.SingularValueDecomposition(true, a, matrix.RowCount, matrix.ColumnCount, s, u, vt), Throws.InvalidOperationException);

            var b = new[] {1, 2, 3, 4, 5, 6};
            var x = new int[matrix.ColumnCount*2];
            Assert.That(() => Control.LinearAlgebraProvider.SvdSolveFactored(matrix.RowCount, matrix.ColumnCount, s, u, vt, b, 2, x), Throws.InvalidOperationException);
        }

        /// <summary>
        /// Solve Ax=b using SVD factorization with a tall A matrix
		/// using a factored matrix always throws <c>InvalidOperationException</c>.
        /// </summary>
        [Test]
        public void SolveUsingSVDTallMatrixOnFactoredMatrixThrowsInvalidOperationException()
        {
            var matrix = _matrices["Tall3x2"];
            var a = new int[matrix.RowCount*matrix.ColumnCount];
            Array.Copy(matrix.Values, a, a.Length);

            var s = new int[matrix.ColumnCount];
            var u = new int[matrix.RowCount*matrix.RowCount];
            var vt = new int[matrix.ColumnCount*matrix.ColumnCount];

            Assert.That(() => Control.LinearAlgebraProvider.SingularValueDecomposition(true, a, matrix.RowCount, matrix.ColumnCount, s, u, vt), Throws.InvalidOperationException);

            var b = new[] {1, 2, 3, 4, 5, 6};
            var x = new int[matrix.ColumnCount*2];
            Assert.That(() => Control.LinearAlgebraProvider.SvdSolveFactored(matrix.RowCount, matrix.ColumnCount, s, u, vt, b, 2, x), Throws.InvalidOperationException);
        }

        [TestCase("Wide10x50000", "Tall50000x10")]
        [TestCase("Square1000x1000", "Square1000x1000")]
        [Explicit, Timeout(1000*5)]
        public void IsMatrixMultiplicationPerformant(string leftMatrixKey, string rightMatrixKey)
        {
            var leftMatrix = _matrices[leftMatrixKey];
            var rightMatrix = _matrices[rightMatrixKey];
            var result = leftMatrix*rightMatrix;
            Assert.That(result, Is.Not.Null);
        }

        /// <summary>
        /// Checks to see if a matrix and array contain the same values.
        /// </summary>
        /// <param name="rows">number of rows.</param>
        /// <param name="columns">number of columns.</param>
        /// <param name="array">array to check.</param>
        /// <param name="matrix">matrix to check against.</param>
        static void NotModified(int rows, int columns, IList<int> array, Matrix<int> matrix)
        {
            var index = 0;
            for (var col = 0; col < columns; col++)
            {
                for (var row = 0; row < rows; row++)
                {
                    Assert.AreEqual(array[index++], matrix[row, col]);
                }
            }
        }
    }
}
