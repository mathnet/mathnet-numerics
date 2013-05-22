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

namespace MathNet.Numerics.UnitTests.LinearAlgebraProviderTests.Complex32
{
    using Algorithms.LinearAlgebra;
    using Distributions;
    using LinearAlgebra.Complex32;
    using LinearAlgebra.Generic;
    using LinearAlgebra.Generic.Factorization;
    using Numerics;
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Base class for linear algebra provider tests.
    /// </summary>
    [TestFixture]
    public class LinearAlgebraProviderTests
    {
        /// <summary>
        /// The Y Complex32 test vector.
        /// </summary>
        readonly Complex32[] _y = new[] {new Complex32(1.1f, 0f), 2.2f, 3.3f, 4.4f, 5.5f};

        /// <summary>
        /// The X Complex32 test vector.
        /// </summary>
        readonly Complex32[] _x = new[] {new Complex32(6.6f, 0f), 7.7f, 8.8f, 9.9f, 10.1f};

        static readonly IContinuousDistribution Dist = new Normal();

        /// <summary>
        /// Test matrix to use.
        /// </summary>
        readonly IDictionary<string, DenseMatrix> _matrices = new Dictionary<string, DenseMatrix>
            {
                {"Singular3x3", DenseMatrix.OfArray(new[,] {{new Complex32(1.0f, 0.0f), 1.0f, 2.0f}, {1.0f, 1.0f, 2.0f}, {1.0f, 1.0f, 2.0f}})},
                {"Square3x3", DenseMatrix.OfArray(new[,] {{new Complex32(-1.1f, 0.0f), -2.2f, -3.3f}, {0.0f, 1.1f, 2.2f}, {-4.4f, 5.5f, 6.6f}})},
                {"Square4x4", DenseMatrix.OfArray(new[,] {{new Complex32(-1.1f, 0.0f), -2.2f, -3.3f, -4.4f}, {0.0f, 1.1f, 2.2f, 3.3f}, {1.0f, 2.1f, 6.2f, 4.3f}, {-4.4f, 5.5f, 6.6f, -7.7f}})},
                {"Singular4x4", DenseMatrix.OfArray(new[,] {{new Complex32(-1.1f, 0.0f), -2.2f, -3.3f, -4.4f}, {-1.1f, -2.2f, -3.3f, -4.4f}, {-1.1f, -2.2f, -3.3f, -4.4f}, {-1.1f, -2.2f, -3.3f, -4.4f}})},
                {"Tall3x2", DenseMatrix.OfArray(new[,] {{new Complex32(-1.1f, 0.0f), -2.2f}, {0.0f, 1.1f}, {-4.4f, 5.5f}})},
                {"Wide2x3", DenseMatrix.OfArray(new[,] {{new Complex32(-1.1f, 0.0f), -2.2f, -3.3f}, {0.0f, 1.1f, 2.2f}})},
                {"Tall50000x10", DenseMatrix.CreateRandom(50000, 10, Dist)},
                {"Wide10x50000", DenseMatrix.CreateRandom(10, 50000, Dist)},
                {"Square1000x1000", DenseMatrix.CreateRandom(1000, 1000, Dist)}
            };

        /// <summary>
        /// Can add a vector to scaled vector
        /// </summary>
        [Test]
        public void CanAddVectorToScaledVectorComplex32()
        {
            var result = new Complex32[_y.Length];

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
            Control.LinearAlgebraProvider.AddVectorToScaledVector(result, (Complex32) Math.PI, _x, result);
            for (var i = 0; i < _y.Length; i++)
            {
                AssertHelpers.AlmostEqual(_y[i] + ((Complex32) Math.PI*_x[i]), result[i], 6);
            }
        }

        /// <summary>
        /// Can scale an array.
        /// </summary>
        [Test]
        public void CanScaleArray()
        {
            var result = new Complex32[_y.Length];

            Control.LinearAlgebraProvider.ScaleArray(1, _y, result);
            for (var i = 0; i < _y.Length; i++)
            {
                Assert.AreEqual(_y[i], result[i]);
            }

            Array.Copy(_y, result, _y.Length);
            Control.LinearAlgebraProvider.ScaleArray((Complex32) Math.PI, result, result);
            for (var i = 0; i < _y.Length; i++)
            {
                AssertHelpers.AlmostEqual(_y[i]*(Complex32) Math.PI, result[i], 6);
            }
        }

        /// <summary>
        /// Can compute the dot product.
        /// </summary>
        [Test]
        public void CanComputeDotProduct()
        {
            var result = Control.LinearAlgebraProvider.DotProduct(_x, _y);
            AssertHelpers.AlmostEqual(152.35f, result, 6);
        }

        /// <summary>
        /// Can add two arrays.
        /// </summary>
        [Test]
        public void CanAddArrays()
        {
            var result = new Complex32[_y.Length];
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
            var result = new Complex32[_y.Length];
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
            var result = new Complex32[_y.Length];
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
            var result = new Complex32[_y.Length];
            Control.LinearAlgebraProvider.PointWiseDivideArrays(_x, _y, result);
            for (var i = 0; i < result.Length; i++)
            {
                Assert.AreEqual(_x[i]/_y[i], result[i]);
            }
        }

        /// <summary>
        /// Can compute L1 norm.
        /// </summary>
        [Test]
        public void CanComputeMatrixL1Norm()
        {
            var matrix = _matrices["Square3x3"];
            var work = new float[matrix.RowCount];
            var norm = Control.LinearAlgebraProvider.MatrixNorm(Norm.OneNorm, matrix.RowCount, matrix.ColumnCount, matrix.Values, work);
            AssertHelpers.AlmostEqual(12.1f, norm, 6);
        }

        /// <summary>
        /// Can compute Frobenius norm.
        /// </summary>
        [Test]
        public void CanComputeMatrixFrobeniusNorm()
        {
            var matrix = _matrices["Square3x3"];
            var work = new float[matrix.RowCount];
            var norm = Control.LinearAlgebraProvider.MatrixNorm(Norm.FrobeniusNorm, matrix.RowCount, matrix.ColumnCount, matrix.Values, work);
            AssertHelpers.AlmostEqual(10.777754868246f, norm, 6);
        }

        /// <summary>
        /// Can compute Infinity norm.
        /// </summary>
        [Test]
        public void CanComputeMatrixInfinityNorm()
        {
            var matrix = _matrices["Square3x3"];
            var work = new float[matrix.RowCount];
            var norm = Control.LinearAlgebraProvider.MatrixNorm(Norm.InfinityNorm, matrix.RowCount, matrix.ColumnCount, matrix.Values, work);
            Assert.AreEqual(16.5, norm.Real);
        }

        /// <summary>
        /// Can compute L1 norm using a work array.
        /// </summary>
        [Test]
        public void CanComputeMatrixL1NormWithWorkArray()
        {
            var matrix = _matrices["Square3x3"];
            var work = new float[18];
            var norm = Control.LinearAlgebraProvider.MatrixNorm(Norm.OneNorm, matrix.RowCount, matrix.ColumnCount, matrix.Values, work);
            AssertHelpers.AlmostEqual(12.1f, norm, 6);
        }

        /// <summary>
        /// Can compute Frobenius norm using a work array.
        /// </summary>
        [Test]
        public void CanComputeMatrixFrobeniusNormWithWorkArray()
        {
            var matrix = _matrices["Square3x3"];
            var work = new float[18];
            var norm = Control.LinearAlgebraProvider.MatrixNorm(Norm.FrobeniusNorm, matrix.RowCount, matrix.ColumnCount, matrix.Values, work);
            AssertHelpers.AlmostEqual(10.777754868246f, norm, 6);
        }

        /// <summary>
        /// Can compute Infinity norm using a work array.
        /// </summary>
        [Test]
        public void CanComputeMatrixInfinityNormWithWorkArray()
        {
            var matrix = _matrices["Square3x3"];
            var work = new float[18];
            var norm = Control.LinearAlgebraProvider.MatrixNorm(Norm.InfinityNorm, matrix.RowCount, matrix.ColumnCount, matrix.Values);
            Assert.AreEqual(16.5, norm.Real);
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
                    AssertHelpers.AlmostEqual(x.Row(i)*y.Column(j), c[i, j], 6);
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
                    AssertHelpers.AlmostEqual(x.Row(i)*y.Column(j), c[i, j], 6);
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
                    AssertHelpers.AlmostEqual(x.Row(i)*y.Column(j), c[i, j], 6);
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

            Control.LinearAlgebraProvider.MatrixMultiplyWithUpdate(Transpose.DontTranspose, Transpose.DontTranspose, 2.2f, x.Values, x.RowCount, x.ColumnCount, y.Values, y.RowCount, y.ColumnCount, 1.0f, c.Values);

            for (var i = 0; i < c.RowCount; i++)
            {
                for (var j = 0; j < c.ColumnCount; j++)
                {
                    AssertHelpers.AlmostEqual(2.2f*x.Row(i)*y.Column(j), c[i, j], 6);
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

            Control.LinearAlgebraProvider.MatrixMultiplyWithUpdate(Transpose.DontTranspose, Transpose.DontTranspose, 2.2f, x.Values, x.RowCount, x.ColumnCount, y.Values, y.RowCount, y.ColumnCount, 1.0f, c.Values);

            for (var i = 0; i < c.RowCount; i++)
            {
                for (var j = 0; j < c.ColumnCount; j++)
                {
                    AssertHelpers.AlmostEqual(2.2f*x.Row(i)*y.Column(j), c[i, j], 6);
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

            Control.LinearAlgebraProvider.MatrixMultiplyWithUpdate(Transpose.DontTranspose, Transpose.DontTranspose, 2.2f, x.Values, x.RowCount, x.ColumnCount, y.Values, y.RowCount, y.ColumnCount, 1.0f, c.Values);

            for (var i = 0; i < c.RowCount; i++)
            {
                for (var j = 0; j < c.ColumnCount; j++)
                {
                    var test = 2.2f*x.Row(i)*y.Column(j);

                    // if they are both close to zero, skip
                    if (Math.Abs(test.Real) < 1e-7 && Math.Abs(c[i, j].Real) < 1e-7)
                    {
                        continue;
                    }

                    AssertHelpers.AlmostEqual(2.2f*x.Row(i)*y.Column(j), c[i, j], 6);
                }
            }
        }

        /// <summary>
        /// Can compute the LU factor of a matrix.
        /// </summary>
        [Test]
        public void CanComputeLuFactor()
        {
            var matrix = _matrices["Square3x3"];
            var a = new Complex32[matrix.RowCount*matrix.RowCount];
            Array.Copy(matrix.Values, a, a.Length);

            var ipiv = new int[matrix.RowCount];

            Control.LinearAlgebraProvider.LUFactor(a, matrix.RowCount, ipiv);

            AssertHelpers.AlmostEqual(a[0], -4.4f, 6);
            AssertHelpers.AlmostEqual(a[1], 0.25f, 6);
            AssertHelpers.AlmostEqual(a[2], 0f, 6);
            AssertHelpers.AlmostEqual(a[3], 5.5f, 6);
            AssertHelpers.AlmostEqual(a[4], -3.575f, 6);
            AssertHelpers.AlmostEqual(a[5], -0.307692307692308f, 6);
            AssertHelpers.AlmostEqual(a[6], 6.6f, 6);
            AssertHelpers.AlmostEqual(a[7], -4.95f, 6);
            AssertHelpers.AlmostEqual(a[8], 0.676923076923077f, 6);
            Assert.AreEqual(ipiv[0], 2);
            Assert.AreEqual(ipiv[1], 2);
            Assert.AreEqual(ipiv[2], 2);
        }

        /// <summary>
        /// Can compute the inverse of a matrix using LU factorization.
        /// </summary>
        [Test]
        public void CanComputeLuInverse()
        {
            var matrix = _matrices["Square3x3"];
            var a = new Complex32[matrix.RowCount*matrix.RowCount];
            Array.Copy(matrix.Values, a, a.Length);

            Control.LinearAlgebraProvider.LUInverse(a, matrix.RowCount);

            AssertHelpers.AlmostEqual(a[0], -0.454545454545454f, 6);
            AssertHelpers.AlmostEqual(a[1], -0.909090909090908f, 6);
            AssertHelpers.AlmostEqual(a[2], 0.454545454545454f, 6);
            AssertHelpers.AlmostEqual(a[3], -0.340909090909090f, 6);
            AssertHelpers.AlmostEqual(a[4], -2.045454545454543f, 6);
            AssertHelpers.AlmostEqual(a[5], 1.477272727272726f, 6);
            AssertHelpers.AlmostEqual(a[6], -0.113636363636364f, 6);
            AssertHelpers.AlmostEqual(a[7], 0.227272727272727f, 6);
            AssertHelpers.AlmostEqual(a[8], -0.113636363636364f, 6);
        }

        /// <summary>
        /// Can compute the inverse of a matrix using LU factorization
        /// using a previously factored matrix.
        /// </summary>
        [Test]
        public void CanComputeLuInverseOnFactoredMatrix()
        {
            var matrix = _matrices["Square3x3"];
            var a = new Complex32[matrix.RowCount*matrix.RowCount];
            Array.Copy(matrix.Values, a, a.Length);

            var ipiv = new int[matrix.RowCount];

            Control.LinearAlgebraProvider.LUFactor(a, matrix.RowCount, ipiv);
            Control.LinearAlgebraProvider.LUInverseFactored(a, matrix.RowCount, ipiv);

            AssertHelpers.AlmostEqual(a[0], -0.454545454545454f, 6);
            AssertHelpers.AlmostEqual(a[1], -0.909090909090908f, 6);
            AssertHelpers.AlmostEqual(a[2], 0.454545454545454f, 6);
            AssertHelpers.AlmostEqual(a[3], -0.340909090909090f, 6);
            AssertHelpers.AlmostEqual(a[4], -2.045454545454543f, 6);
            AssertHelpers.AlmostEqual(a[5], 1.477272727272726f, 6);
            AssertHelpers.AlmostEqual(a[6], -0.113636363636364f, 6);
            AssertHelpers.AlmostEqual(a[7], 0.227272727272727f, 6);
            AssertHelpers.AlmostEqual(a[8], -0.113636363636364f, 6);
        }

        /// <summary>
        /// Can compute the inverse of a matrix using LU factorization
        /// with a work array.
        /// </summary>
        [Test]
        public void CanComputeLuInverseWithWorkArray()
        {
            var matrix = _matrices["Square3x3"];
            var a = new Complex32[matrix.RowCount*matrix.RowCount];
            Array.Copy(matrix.Values, a, a.Length);

            var work = new Complex32[matrix.RowCount];
            Control.LinearAlgebraProvider.LUInverse(a, matrix.RowCount, work);

            AssertHelpers.AlmostEqual(a[0], -0.454545454545454f, 6);
            AssertHelpers.AlmostEqual(a[1], -0.909090909090908f, 6);
            AssertHelpers.AlmostEqual(a[2], 0.454545454545454f, 6);
            AssertHelpers.AlmostEqual(a[3], -0.340909090909090f, 6);
            AssertHelpers.AlmostEqual(a[4], -2.045454545454543f, 6);
            AssertHelpers.AlmostEqual(a[5], 1.477272727272726f, 6);
            AssertHelpers.AlmostEqual(a[6], -0.113636363636364f, 6);
            AssertHelpers.AlmostEqual(a[7], 0.227272727272727f, 6);
            AssertHelpers.AlmostEqual(a[8], -0.113636363636364f, 6);
        }

        /// <summary>
        /// Can compute the inverse of a matrix using LU factorization
        /// using a previously factored matrix with a work array.
        /// </summary>
        [Test]
        public void CanComputeLuInverseOnFactoredMatrixWithWorkArray()
        {
            var matrix = _matrices["Square3x3"];
            var a = new Complex32[matrix.RowCount*matrix.RowCount];
            Array.Copy(matrix.Values, a, a.Length);

            var ipiv = new int[matrix.RowCount];

            Control.LinearAlgebraProvider.LUFactor(a, matrix.RowCount, ipiv);

            var work = new Complex32[matrix.RowCount];
            Control.LinearAlgebraProvider.LUInverseFactored(a, matrix.RowCount, ipiv, work);

            AssertHelpers.AlmostEqual(a[0], -0.454545454545454f, 6);
            AssertHelpers.AlmostEqual(a[1], -0.909090909090908f, 6);
            AssertHelpers.AlmostEqual(a[2], 0.454545454545454f, 6);
            AssertHelpers.AlmostEqual(a[3], -0.340909090909090f, 6);
            AssertHelpers.AlmostEqual(a[4], -2.045454545454543f, 6);
            AssertHelpers.AlmostEqual(a[5], 1.477272727272726f, 6);
            AssertHelpers.AlmostEqual(a[6], -0.113636363636364f, 6);
            AssertHelpers.AlmostEqual(a[7], 0.227272727272727f, 6);
            AssertHelpers.AlmostEqual(a[8], -0.113636363636364f, 6);
        }

        /// <summary>
        /// Can solve Ax=b using LU factorization.
        /// </summary>
        [Test]
        public void CanSolveUsingLU()
        {
            var matrix = _matrices["Square3x3"];
            var a = new Complex32[matrix.RowCount*matrix.RowCount];
            Array.Copy(matrix.Values, a, a.Length);

            var b = new[] {new Complex32(1.0f, 0.0f), 2.0f, 3.0f, 4.0f, 5.0f, 6.0f};
            Control.LinearAlgebraProvider.LUSolve(2, a, matrix.RowCount, b);

            AssertHelpers.AlmostEqual(b[0], -1.477272727272726f, 6);
            AssertHelpers.AlmostEqual(b[1], -4.318181818181815f, 6);
            AssertHelpers.AlmostEqual(b[2], 3.068181818181816f, 6);
            AssertHelpers.AlmostEqual(b[3], -4.204545454545451f, 6);
            AssertHelpers.AlmostEqual(b[4], -12.499999999999989f, 6);
            AssertHelpers.AlmostEqual(b[5], 8.522727272727266f, 6);

            NotModified(matrix.RowCount, matrix.ColumnCount, a, matrix);
        }

        /// <summary>
        /// Can solve Ax=b using LU factorization using a factored matrix.
        /// </summary>
        [Test]
        public void CanSolveUsingLUOnFactoredMatrix()
        {
            var matrix = _matrices["Square3x3"];
            var a = new Complex32[matrix.RowCount*matrix.RowCount];
            Array.Copy(matrix.Values, a, a.Length);

            var ipiv = new int[matrix.RowCount];
            Control.LinearAlgebraProvider.LUFactor(a, matrix.RowCount, ipiv);

            var b = new[] {new Complex32(1.0f, 0.0f), 2.0f, 3.0f, 4.0f, 5.0f, 6.0f};
            Control.LinearAlgebraProvider.LUSolveFactored(2, a, matrix.RowCount, ipiv, b);

            AssertHelpers.AlmostEqual(b[0], -1.477272727272726f, 6);
            AssertHelpers.AlmostEqual(b[1], -4.318181818181815f, 6);
            AssertHelpers.AlmostEqual(b[2], 3.068181818181816f, 6);
            AssertHelpers.AlmostEqual(b[3], -4.204545454545451f, 6);
            AssertHelpers.AlmostEqual(b[4], -12.499999999999989f, 6);
            AssertHelpers.AlmostEqual(b[5], 8.522727272727266f, 6);
        }

        /// <summary>
        /// Can compute the <c>Cholesky</c> factorization.
        /// </summary>
        [Test]
        public void CanComputeCholeskyFactor()
        {
            var matrix = new Complex32[] {1, 1, 1, 1, 1, 5, 5, 5, 1, 5, 14, 14, 1, 5, 14, 15};
            Control.LinearAlgebraProvider.CholeskyFactor(matrix, 4);
            Assert.AreEqual(matrix[0].Real, 1);
            Assert.AreEqual(matrix[1].Real, 1);
            Assert.AreEqual(matrix[2].Real, 1);
            Assert.AreEqual(matrix[3].Real, 1);
            Assert.AreEqual(matrix[4].Real, 0);
            Assert.AreEqual(matrix[5].Real, 2);
            Assert.AreEqual(matrix[6].Real, 2);
            Assert.AreEqual(matrix[7].Real, 2);
            Assert.AreEqual(matrix[8].Real, 0);
            Assert.AreEqual(matrix[9].Real, 0);
            Assert.AreEqual(matrix[10].Real, 3);
            Assert.AreEqual(matrix[11].Real, 3);
            Assert.AreEqual(matrix[12].Real, 0);
            Assert.AreEqual(matrix[13].Real, 0);
            Assert.AreEqual(matrix[14].Real, 0);
            Assert.AreEqual(matrix[15].Real, 1);
        }

        /// <summary>
        /// Can solve Ax=b using Cholesky factorization.
        /// </summary>
        [Test]
        public void CanSolveUsingCholesky()
        {
            var matrix = new DenseMatrix(3, 3, new Complex32[] {1, 1, 1, 1, 2, 3, 1, 3, 6});
            var a = new Complex32[] {1, 1, 1, 1, 2, 3, 1, 3, 6};

            var b = new[] {new Complex32(1.0f, 0.0f), 2.0f, 3.0f, 4.0f, 5.0f, 6.0f};
            Control.LinearAlgebraProvider.CholeskySolve(a, 3, b, 2);

            AssertHelpers.AlmostEqual(b[0], 0, 6);
            AssertHelpers.AlmostEqual(b[1], 1, 6);
            AssertHelpers.AlmostEqual(b[2], 0, 6);
            AssertHelpers.AlmostEqual(b[3], 3, 6);
            AssertHelpers.AlmostEqual(b[4], 1, 6);
            AssertHelpers.AlmostEqual(b[5], 0, 6);

            NotModified(3, 3, a, matrix);
        }

        /// <summary>
        /// Can solve Ax=b using LU factorization using a factored matrix.
        /// </summary>
        [Test]
        public void CanSolveUsingCholeskyOnFactoredMatrix()
        {
            var a = new Complex32[] {1, 1, 1, 1, 2, 3, 1, 3, 6};

            Control.LinearAlgebraProvider.CholeskyFactor(a, 3);

            var b = new[] {new Complex32(1.0f, 0.0f), 2.0f, 3.0f, 4.0f, 5.0f, 6.0f};
            Control.LinearAlgebraProvider.CholeskySolveFactored(a, 3, b, 2);

            AssertHelpers.AlmostEqual(b[0], 0, 6);
            AssertHelpers.AlmostEqual(b[1], 1, 6);
            AssertHelpers.AlmostEqual(b[2], 0, 6);
            AssertHelpers.AlmostEqual(b[3], 3, 6);
            AssertHelpers.AlmostEqual(b[4], 1, 6);
            AssertHelpers.AlmostEqual(b[5], 0, 6);
        }

        /// <summary>
        /// Can compute QR factorization of a square matrix.
        /// </summary>
        [Test]
        public void CanComputeQRFactorSquareMatrix()
        {
            var matrix = _matrices["Square3x3"];
            var r = new Complex32[matrix.RowCount*matrix.ColumnCount];
            Array.Copy(matrix.Values, r, r.Length);

            var tau = new Complex32[3];
            var q = new Complex32[matrix.RowCount*matrix.RowCount];
            Control.LinearAlgebraProvider.QRFactor(r, matrix.RowCount, matrix.ColumnCount, q, tau);

            var mq = new DenseMatrix(matrix.RowCount, matrix.RowCount, q);
            var mr = new DenseMatrix(matrix.RowCount, matrix.ColumnCount, r).UpperTriangle();
            var a = mq*mr;

            for (var row = 0; row < matrix.RowCount; row++)
            {
                for (var col = 0; col < matrix.ColumnCount; col++)
                {
                    AssertHelpers.AlmostEqual(matrix[row, col], a[row, col], 6);
                }
            }
        }

        /// <summary>
        /// Can compute QR factorization of a tall matrix.
        /// </summary>
        [Test]
        public void CanComputeQRFactorTallMatrix()
        {
            var matrix = _matrices["Tall3x2"];
            var r = new Complex32[matrix.RowCount*matrix.ColumnCount];
            Array.Copy(matrix.Values, r, r.Length);

            var tau = new Complex32[3];
            var q = new Complex32[matrix.RowCount*matrix.RowCount];
            Control.LinearAlgebraProvider.QRFactor(r, matrix.RowCount, matrix.ColumnCount, q, tau);

            var mr = new DenseMatrix(matrix.RowCount, matrix.ColumnCount, r).UpperTriangle();
            var mq = new DenseMatrix(matrix.RowCount, matrix.RowCount, q);
            var a = mq*mr;

            for (var row = 0; row < matrix.RowCount; row++)
            {
                for (var col = 0; col < matrix.ColumnCount; col++)
                {
                    AssertHelpers.AlmostEqual(matrix[row, col], a[row, col], 6);
                }
            }
        }

        /// <summary>
        /// Can compute QR factorization of a wide matrix.
        /// </summary>
        [Test]
        public void CanComputeQRFactorWideMatrix()
        {
            var matrix = _matrices["Wide2x3"];
            var r = new Complex32[matrix.RowCount*matrix.ColumnCount];
            Array.Copy(matrix.Values, r, r.Length);

            var tau = new Complex32[3];
            var q = new Complex32[matrix.RowCount*matrix.RowCount];
            Control.LinearAlgebraProvider.QRFactor(r, matrix.RowCount, matrix.ColumnCount, q, tau);

            var mr = new DenseMatrix(matrix.RowCount, matrix.ColumnCount, r).UpperTriangle();
            var mq = new DenseMatrix(matrix.RowCount, matrix.RowCount, q);
            var a = mq*mr;

            for (var row = 0; row < matrix.RowCount; row++)
            {
                for (var col = 0; col < matrix.ColumnCount; col++)
                {
                    AssertHelpers.AlmostEqual(matrix[row, col], a[row, col], 6);
                }
            }
        }

        /// <summary>
        /// Can compute QR factorization of a square matrix using a work array.
        /// </summary>
        [Test]
        public void CanComputeQRFactorSquareMatrixWithWorkArray()
        {
            var matrix = _matrices["Square3x3"];
            var r = new Complex32[matrix.RowCount*matrix.ColumnCount];
            Array.Copy(matrix.Values, r, r.Length);

            var tau = new Complex32[3];
            var q = new Complex32[matrix.RowCount*matrix.RowCount];
            var work = new Complex32[matrix.ColumnCount*Control.BlockSize];
            Control.LinearAlgebraProvider.QRFactor(r, matrix.RowCount, matrix.ColumnCount, q, tau, work);

            var mq = new DenseMatrix(matrix.RowCount, matrix.RowCount, q);
            var mr = new DenseMatrix(matrix.RowCount, matrix.ColumnCount, r).UpperTriangle();
            var a = mq*mr;

            for (var row = 0; row < matrix.RowCount; row++)
            {
                for (var col = 0; col < matrix.ColumnCount; col++)
                {
                    AssertHelpers.AlmostEqual(matrix[row, col], a[row, col], 6);
                }
            }
        }

        /// <summary>
        /// Can compute QR factorization of a tall matrix using a work matrix.
        /// </summary>
        [Test]
        public void CanComputeQRFactorTallMatrixWithWorkArray()
        {
            var matrix = _matrices["Tall3x2"];
            var r = new Complex32[matrix.RowCount*matrix.ColumnCount];
            Array.Copy(matrix.Values, r, r.Length);

            var tau = new Complex32[3];
            var q = new Complex32[matrix.RowCount*matrix.RowCount];
            var work = new Complex32[matrix.ColumnCount*Control.BlockSize];
            Control.LinearAlgebraProvider.QRFactor(r, matrix.RowCount, matrix.ColumnCount, q, tau, work);

            var mr = new DenseMatrix(matrix.RowCount, matrix.ColumnCount, r).UpperTriangle();
            var mq = new DenseMatrix(matrix.RowCount, matrix.RowCount, q);
            var a = mq*mr;

            for (var row = 0; row < matrix.RowCount; row++)
            {
                for (var col = 0; col < matrix.ColumnCount; col++)
                {
                    AssertHelpers.AlmostEqual(matrix[row, col], a[row, col], 6);
                }
            }
        }

        /// <summary>
        /// Can compute QR factorization of a wide matrix using a work matrix.
        /// </summary>
        [Test]
        public void CanComputeQRFactorWideMatrixWithWorkArray()
        {
            var matrix = _matrices["Wide2x3"];
            var r = new Complex32[matrix.RowCount*matrix.ColumnCount];
            Array.Copy(matrix.Values, r, r.Length);

            var tau = new Complex32[3];
            var q = new Complex32[matrix.RowCount*matrix.RowCount];
            var work = new Complex32[matrix.ColumnCount*Control.BlockSize];
            Control.LinearAlgebraProvider.QRFactor(r, matrix.RowCount, matrix.ColumnCount, q, tau, work);

            var mr = new DenseMatrix(matrix.RowCount, matrix.ColumnCount, r).UpperTriangle();
            var mq = new DenseMatrix(matrix.RowCount, matrix.RowCount, q);
            var a = mq*mr;

            for (var row = 0; row < matrix.RowCount; row++)
            {
                for (var col = 0; col < matrix.ColumnCount; col++)
                {
                    AssertHelpers.AlmostEqual(matrix[row, col], a[row, col], 6);
                }
            }
        }

        /// <summary>
        /// Can compute thin QR factorization of a square matrix.
        /// </summary>
        [Test]
        public void CanComputeThinQRFactorSquareMatrix()
        {
            var matrix = _matrices["Square3x3"];
            var r = new Complex32[matrix.ColumnCount*matrix.ColumnCount];
            var tau = new Complex32[3];
            var q = new Complex32[matrix.RowCount*matrix.ColumnCount];
            Array.Copy(matrix.Values, q, q.Length);

            Control.LinearAlgebraProvider.ThinQRFactor(q, matrix.RowCount, matrix.ColumnCount, r, tau);

            var mq = new DenseMatrix(matrix.RowCount, matrix.ColumnCount, q);
            var mr = new DenseMatrix(matrix.ColumnCount, matrix.ColumnCount, r);
            var a = mq*mr;

            for (var row = 0; row < matrix.RowCount; row++)
            {
                for (var col = 0; col < matrix.ColumnCount; col++)
                {
                    AssertHelpers.AlmostEqual(matrix[row, col], a[row, col], 6);
                }
            }
        }

        /// <summary>
        /// Can compute thin QR factorization of a tall matrix.
        /// </summary>
        [Test]
        public void CanComputeThinQRFactorTallMatrix()
        {
            var matrix = _matrices["Tall3x2"];
            var r = new Complex32[matrix.ColumnCount*matrix.ColumnCount];
            var tau = new Complex32[3];
            var q = new Complex32[matrix.RowCount*matrix.ColumnCount];
            Array.Copy(matrix.Values, q, q.Length);

            Control.LinearAlgebraProvider.ThinQRFactor(q, matrix.RowCount, matrix.ColumnCount, r, tau);

            var mq = new DenseMatrix(matrix.RowCount, matrix.ColumnCount, q);
            var mr = new DenseMatrix(matrix.ColumnCount, matrix.ColumnCount, r);
            var a = mq*mr;

            for (var row = 0; row < matrix.RowCount; row++)
            {
                for (var col = 0; col < matrix.ColumnCount; col++)
                {
                    AssertHelpers.AlmostEqual(matrix[row, col], a[row, col], 6);
                }
            }
        }

        /// <summary>
        /// Can compute thin QR factorization of a square matrix using a work array.
        /// </summary>
        [Test]
        public void CanComputeThinQRFactorSquareMatrixWithWorkArray()
        {
            var matrix = _matrices["Square3x3"];
            var r = new Complex32[matrix.ColumnCount*matrix.ColumnCount];
            var tau = new Complex32[3];
            var q = new Complex32[matrix.RowCount*matrix.ColumnCount];
            Array.Copy(matrix.Values, q, q.Length);

            var work = new Complex32[matrix.RowCount*matrix.ColumnCount];
            Control.LinearAlgebraProvider.ThinQRFactor(q, matrix.RowCount, matrix.ColumnCount, r, tau, work);

            var mq = new DenseMatrix(matrix.RowCount, matrix.ColumnCount, q);
            var mr = new DenseMatrix(matrix.ColumnCount, matrix.ColumnCount, r);
            var a = mq*mr;

            for (var row = 0; row < matrix.RowCount; row++)
            {
                for (var col = 0; col < matrix.ColumnCount; col++)
                {
                    AssertHelpers.AlmostEqual(matrix[row, col], a[row, col], 6);
                }
            }
        }

        /// <summary>
        /// Can compute thin QR factorization of a tall matrix using a work matrix.
        /// </summary>
        [Test]
        public void CanComputeThinQRFactorTallMatrixWithWorkArray()
        {
            var matrix = _matrices["Tall3x2"];
            var r = new Complex32[matrix.ColumnCount*matrix.ColumnCount];
            var tau = new Complex32[3];
            var q = new Complex32[matrix.RowCount*matrix.ColumnCount];
            Array.Copy(matrix.Values, q, q.Length);

            var work = new Complex32[matrix.RowCount*matrix.ColumnCount];
            Control.LinearAlgebraProvider.ThinQRFactor(q, matrix.RowCount, matrix.ColumnCount, r, tau, work);

            var mq = new DenseMatrix(matrix.RowCount, matrix.ColumnCount, q);
            var mr = new DenseMatrix(matrix.ColumnCount, matrix.ColumnCount, r);
            var a = mq*mr;
            for (var row = 0; row < matrix.RowCount; row++)
            {
                for (var col = 0; col < matrix.ColumnCount; col++)
                {
                    AssertHelpers.AlmostEqual(matrix[row, col], a[row, col], 6);
                }
            }
        }

        /// <summary>
        /// Can solve Ax=b using QR factorization with a square A matrix.
        /// </summary>
        [Test]
        public void CanSolveUsingQRSquareMatrix()
        {
            var matrix = _matrices["Square3x3"];
            var a = new Complex32[matrix.RowCount*matrix.ColumnCount];
            Array.Copy(matrix.Values, a, a.Length);

            var b = new[] {new Complex32(1.0f, 0.0f), 2.0f, 3.0f, 4.0f, 5.0f, 6.0f};
            var x = new Complex32[matrix.ColumnCount*2];
            Control.LinearAlgebraProvider.QRSolve(a, matrix.RowCount, matrix.ColumnCount, b, 2, x);

            NotModified(3, 3, a, matrix);

            var mx = new DenseMatrix(matrix.ColumnCount, 2, x);
            var mb = matrix*mx;

            AssertHelpers.AlmostEqual(mb[0, 0], b[0], 5);
            AssertHelpers.AlmostEqual(mb[1, 0], b[1], 5);
            AssertHelpers.AlmostEqual(mb[2, 0], b[2], 5);
            AssertHelpers.AlmostEqual(mb[0, 1], b[3], 5);
            AssertHelpers.AlmostEqual(mb[1, 1], b[4], 5);
            AssertHelpers.AlmostEqual(mb[2, 1], b[5], 5);
        }

        /// <summary>
        /// Can solve Ax=b using QR factorization with a tall A matrix.
        /// </summary>
        [Test]
        public void CanSolveUsingQRTallMatrix()
        {
            var matrix = _matrices["Tall3x2"];
            var a = new Complex32[matrix.RowCount*matrix.ColumnCount];
            Array.Copy(matrix.Values, a, a.Length);

            var b = new[] {new Complex32(1.0f, 0.0f), 2.0f, 3.0f, 4.0f, 5.0f, 6.0f};
            var x = new Complex32[matrix.ColumnCount*2];
            Control.LinearAlgebraProvider.QRSolve(a, matrix.RowCount, matrix.ColumnCount, b, 2, x);

            NotModified(3, 2, a, matrix);

            var mb = new DenseMatrix(matrix.RowCount, 2, b);
            var test = (matrix.Transpose()*matrix).Inverse()*matrix.Transpose()*mb;

            AssertHelpers.AlmostEqual(test[0, 0], x[0], 6);
            AssertHelpers.AlmostEqual(test[1, 0], x[1], 6);
            AssertHelpers.AlmostEqual(test[0, 1], x[2], 6);
            AssertHelpers.AlmostEqual(test[1, 1], x[3], 6);
        }

        /// <summary>
        /// Can solve Ax=b using QR factorization with a square A matrix
        /// using a work array.
        /// </summary>
        [Test]
        public void CanSolveUsingQRSquareMatrixUsingWorkArray()
        {
            var matrix = _matrices["Square3x3"];
            var a = new Complex32[matrix.RowCount*matrix.ColumnCount];
            Array.Copy(matrix.Values, a, a.Length);

            var b = new[] {new Complex32(1.0f, 0.0f), 2.0f, 3.0f, 4.0f, 5.0f, 6.0f};
            var x = new Complex32[matrix.ColumnCount*2];
            var work = new Complex32[matrix.RowCount*matrix.RowCount];
            Control.LinearAlgebraProvider.QRSolve(a, matrix.RowCount, matrix.ColumnCount, b, 2, x, work);

            NotModified(3, 3, a, matrix);

            var mx = new DenseMatrix(matrix.ColumnCount, 2, x);
            var mb = matrix*mx;

            AssertHelpers.AlmostEqual(mb[0, 0], b[0], 5);
            AssertHelpers.AlmostEqual(mb[1, 0], b[1], 5);
            AssertHelpers.AlmostEqual(mb[2, 0], b[2], 5);
            AssertHelpers.AlmostEqual(mb[0, 1], b[3], 5);
            AssertHelpers.AlmostEqual(mb[1, 1], b[4], 5);
            AssertHelpers.AlmostEqual(mb[2, 1], b[5], 5);
        }

        /// <summary>
        /// Can solve Ax=b using QR factorization with a tall A matrix
        /// using a work array.
        /// </summary>
        [Test]
        public void CanSolveUsingQRTallMatrixUsingWorkArray()
        {
            var matrix = _matrices["Tall3x2"];
            var a = new Complex32[matrix.RowCount*matrix.ColumnCount];
            Array.Copy(matrix.Values, a, a.Length);

            var b = new[] {new Complex32(1.0f, 0.0f), 2.0f, 3.0f, 4.0f, 5.0f, 6.0f};
            var x = new Complex32[matrix.ColumnCount*2];
            var work = new Complex32[matrix.RowCount*matrix.RowCount];
            Control.LinearAlgebraProvider.QRSolve(a, matrix.RowCount, matrix.ColumnCount, b, 2, x, work);

            NotModified(3, 2, a, matrix);

            var mb = new DenseMatrix(matrix.RowCount, 2, b);
            var test = (matrix.Transpose()*matrix).Inverse()*matrix.Transpose()*mb;

            AssertHelpers.AlmostEqual(test[0, 0], x[0], 6);
            AssertHelpers.AlmostEqual(test[1, 0], x[1], 6);
            AssertHelpers.AlmostEqual(test[0, 1], x[2], 6);
            AssertHelpers.AlmostEqual(test[1, 1], x[3], 6);
        }

        /// <summary>
        /// Can solve Ax=b using QR factorization with a square A matrix
        /// using a factored A matrix.
        /// </summary>
        [Test]
        public void CanSolveUsingQRSquareMatrixOnFactoredMatrix()
        {
            var matrix = _matrices["Square3x3"];
            var a = new Complex32[matrix.RowCount*matrix.RowCount];
            Array.Copy(matrix.Values, a, a.Length);

            var tau = new Complex32[matrix.ColumnCount];
            var q = new Complex32[matrix.ColumnCount*matrix.ColumnCount];
            Control.LinearAlgebraProvider.QRFactor(a, matrix.RowCount, matrix.ColumnCount, q, tau);

            var b = new[] {new Complex32(1.0f, 0.0f), 2.0f, 3.0f, 4.0f, 5.0f, 6.0f};
            var x = new Complex32[matrix.ColumnCount*2];
            Control.LinearAlgebraProvider.QRSolveFactored(q, a, matrix.RowCount, matrix.ColumnCount, tau, b, 2, x);

            var mx = new DenseMatrix(matrix.ColumnCount, 2, x);
            var mb = matrix*mx;

            AssertHelpers.AlmostEqual(mb[0, 0], b[0], 5);
            AssertHelpers.AlmostEqual(mb[1, 0], b[1], 5);
            AssertHelpers.AlmostEqual(mb[2, 0], b[2], 5);
            AssertHelpers.AlmostEqual(mb[0, 1], b[3], 5);
            AssertHelpers.AlmostEqual(mb[1, 1], b[4], 5);
            AssertHelpers.AlmostEqual(mb[2, 1], b[5], 5);
        }

        /// <summary>
        /// Can solve Ax=b using QR factorization with a tall A matrix
        /// using a factored A matrix.
        /// </summary>
        [Test]
        public void CanSolveUsingQRTallMatrixOnFactoredMatrix()
        {
            var matrix = _matrices["Tall3x2"];
            var a = new Complex32[matrix.RowCount*matrix.ColumnCount];
            Array.Copy(matrix.Values, a, a.Length);

            var tau = new Complex32[matrix.ColumnCount];
            var q = new Complex32[matrix.RowCount*matrix.RowCount];
            Control.LinearAlgebraProvider.QRFactor(a, matrix.RowCount, matrix.ColumnCount, q, tau);

            var b = new[] {new Complex32(1.0f, 0.0f), 2.0f, 3.0f, 4.0f, 5.0f, 6.0f};
            var x = new Complex32[matrix.ColumnCount*2];
            Control.LinearAlgebraProvider.QRSolveFactored(q, a, matrix.RowCount, matrix.ColumnCount, tau, b, 2, x);

            var mb = new DenseMatrix(matrix.RowCount, 2, b);
            var test = (matrix.Transpose()*matrix).Inverse()*matrix.Transpose()*mb;

            AssertHelpers.AlmostEqual(test[0, 0], x[0], 6);
            AssertHelpers.AlmostEqual(test[1, 0], x[1], 6);
            AssertHelpers.AlmostEqual(test[0, 1], x[2], 6);
            AssertHelpers.AlmostEqual(test[1, 1], x[3], 6);
        }

        /// <summary>
        /// Can solve Ax=b using QR factorization with a square A matrix
        /// using a factored A matrix with a work array.
        /// </summary>
        [Test]
        public void CanSolveUsingQRSquareMatrixOnFactoredMatrixWithWorkArray()
        {
            var matrix = _matrices["Square3x3"];
            var a = new Complex32[matrix.RowCount*matrix.RowCount];
            Array.Copy(matrix.Values, a, a.Length);

            var tau = new Complex32[matrix.ColumnCount];
            var q = new Complex32[matrix.ColumnCount*matrix.ColumnCount];
            var work = new Complex32[2048];
            Control.LinearAlgebraProvider.QRFactor(a, matrix.RowCount, matrix.ColumnCount, q, tau, work);

            var b = new[] {new Complex32(1.0f, 0.0f), 2.0f, 3.0f, 4.0f, 5.0f, 6.0f};
            var x = new Complex32[matrix.ColumnCount*2];
            Control.LinearAlgebraProvider.QRSolveFactored(q, a, matrix.RowCount, matrix.ColumnCount, tau, b, 2, x, work);

            var mx = new DenseMatrix(matrix.ColumnCount, 2, x);
            var mb = matrix*mx;

            AssertHelpers.AlmostEqual(mb[0, 0], b[0], 5);
            AssertHelpers.AlmostEqual(mb[1, 0], b[1], 5);
            AssertHelpers.AlmostEqual(mb[2, 0], b[2], 5);
            AssertHelpers.AlmostEqual(mb[0, 1], b[3], 5);
            AssertHelpers.AlmostEqual(mb[1, 1], b[4], 5);
            AssertHelpers.AlmostEqual(mb[2, 1], b[5], 5);
        }

        /// <summary>
        /// Can solve Ax=b using QR factorization with a tall A matrix
        /// using a factored A matrix with a work array.
        /// </summary>
        [Test]
        public void CanSolveUsingQRTallMatrixOnFactoredMatrixWithWorkArray()
        {
            var matrix = _matrices["Tall3x2"];
            var a = new Complex32[matrix.RowCount*matrix.ColumnCount];
            Array.Copy(matrix.Values, a, a.Length);

            var tau = new Complex32[matrix.ColumnCount];
            var q = new Complex32[matrix.RowCount*matrix.RowCount];
            var work = new Complex32[2048];
            Control.LinearAlgebraProvider.QRFactor(a, matrix.RowCount, matrix.ColumnCount, q, tau, work);

            var b = new[] {new Complex32(1.0f, 0.0f), 2.0f, 3.0f, 4.0f, 5.0f, 6.0f};
            var x = new Complex32[matrix.ColumnCount*2];
            Control.LinearAlgebraProvider.QRSolveFactored(q, a, matrix.RowCount, matrix.ColumnCount, tau, b, 2, x, work);

            var mb = new DenseMatrix(matrix.RowCount, 2, b);
            var test = (matrix.Transpose()*matrix).Inverse()*matrix.Transpose()*mb;

            AssertHelpers.AlmostEqual(test[0, 0], x[0], 6);
            AssertHelpers.AlmostEqual(test[1, 0], x[1], 6);
            AssertHelpers.AlmostEqual(test[0, 1], x[2], 6);
            AssertHelpers.AlmostEqual(test[1, 1], x[3], 6);
        }

        /// <summary>
        /// Can solve Ax=b using thin QR factorization with a square A matrix.
        /// </summary>
        [Test]
        public void CanSolveUsingThinQRSquareMatrix()
        {
            var matrix = _matrices["Square3x3"];
            var a = new Complex32[matrix.RowCount*matrix.ColumnCount];
            Array.Copy(matrix.Values, a, a.Length);

            var b = new[] {new Complex32(1.0f, 0.0f), 2.0f, 3.0f, 4.0f, 5.0f, 6.0f};
            var x = new Complex32[matrix.ColumnCount*2];
            Control.LinearAlgebraProvider.QRSolve(a, matrix.RowCount, matrix.ColumnCount, b, 2, x, QRMethod.Thin);

            NotModified(3, 3, a, matrix);

            var mx = new DenseMatrix(matrix.ColumnCount, 2, x);
            var mb = matrix*mx;

            AssertHelpers.AlmostEqual(mb[0, 0], b[0], 5);
            AssertHelpers.AlmostEqual(mb[1, 0], b[1], 5);
            AssertHelpers.AlmostEqual(mb[2, 0], b[2], 5);
            AssertHelpers.AlmostEqual(mb[0, 1], b[3], 5);
            AssertHelpers.AlmostEqual(mb[1, 1], b[4], 5);
            AssertHelpers.AlmostEqual(mb[2, 1], b[5], 5);
        }

        /// <summary>
        /// Can solve Ax=b using thin QR factorization with a tall A matrix.
        /// </summary>
        [Test]
        public void CanSolveUsingThinQRTallMatrix()
        {
            var matrix = _matrices["Tall3x2"];
            var a = new Complex32[matrix.RowCount*matrix.ColumnCount];
            Array.Copy(matrix.Values, a, a.Length);

            var b = new[] {new Complex32(1.0f, 0.0f), 2.0f, 3.0f, 4.0f, 5.0f, 6.0f};
            var x = new Complex32[matrix.ColumnCount*2];
            Control.LinearAlgebraProvider.QRSolve(a, matrix.RowCount, matrix.ColumnCount, b, 2, x, QRMethod.Thin);

            NotModified(3, 2, a, matrix);

            var mb = new DenseMatrix(matrix.RowCount, 2, b);
            var test = (matrix.Transpose()*matrix).Inverse()*matrix.Transpose()*mb;

            AssertHelpers.AlmostEqual(test[0, 0], x[0], 6);
            AssertHelpers.AlmostEqual(test[1, 0], x[1], 6);
            AssertHelpers.AlmostEqual(test[0, 1], x[2], 6);
            AssertHelpers.AlmostEqual(test[1, 1], x[3], 6);
        }

        /// <summary>
        /// Can solve Ax=b using thin QR factorization with a square A matrix
        /// using a work array.
        /// </summary>
        [Test]
        public void CanSolveUsingThinQRSquareMatrixUsingWorkArray()
        {
            var matrix = _matrices["Square3x3"];
            var a = new Complex32[matrix.RowCount*matrix.ColumnCount];
            Array.Copy(matrix.Values, a, a.Length);

            var b = new[] {new Complex32(1.0f, 0.0f), 2.0f, 3.0f, 4.0f, 5.0f, 6.0f};
            var x = new Complex32[matrix.ColumnCount*2];
            var work = new Complex32[matrix.RowCount*matrix.ColumnCount];
            Control.LinearAlgebraProvider.QRSolve(a, matrix.RowCount, matrix.ColumnCount, b, 2, x, work, QRMethod.Thin);

            NotModified(3, 3, a, matrix);

            var mx = new DenseMatrix(matrix.ColumnCount, 2, x);
            var mb = matrix*mx;

            AssertHelpers.AlmostEqual(mb[0, 0], b[0], 5);
            AssertHelpers.AlmostEqual(mb[1, 0], b[1], 5);
            AssertHelpers.AlmostEqual(mb[2, 0], b[2], 5);
            AssertHelpers.AlmostEqual(mb[0, 1], b[3], 5);
            AssertHelpers.AlmostEqual(mb[1, 1], b[4], 5);
            AssertHelpers.AlmostEqual(mb[2, 1], b[5], 5);
        }

        /// <summary>
        /// Can solve Ax=b using thin QR factorization with a tall A matrix
        /// using a work array.
        /// </summary>
        [Test]
        public void CanSolveUsingThinQRTallMatrixUsingWorkArray()
        {
            var matrix = _matrices["Tall3x2"];
            var a = new Complex32[matrix.RowCount*matrix.ColumnCount];
            Array.Copy(matrix.Values, a, a.Length);

            var b = new[] {new Complex32(1.0f, 0.0f), 2.0f, 3.0f, 4.0f, 5.0f, 6.0f};
            var x = new Complex32[matrix.ColumnCount*2];
            var work = new Complex32[matrix.RowCount*matrix.ColumnCount];
            Control.LinearAlgebraProvider.QRSolve(a, matrix.RowCount, matrix.ColumnCount, b, 2, x, work, QRMethod.Thin);

            NotModified(3, 2, a, matrix);

            var mb = new DenseMatrix(matrix.RowCount, 2, b);
            var test = (matrix.Transpose()*matrix).Inverse()*matrix.Transpose()*mb;

            AssertHelpers.AlmostEqual(test[0, 0], x[0], 6);
            AssertHelpers.AlmostEqual(test[1, 0], x[1], 6);
            AssertHelpers.AlmostEqual(test[0, 1], x[2], 6);
            AssertHelpers.AlmostEqual(test[1, 1], x[3], 6);
        }

        /// <summary>
        /// Can solve Ax=b using thin QR factorization with a square A matrix
        /// using a factored A matrix.
        /// </summary>
        [Test]
        public void CanSolveUsingThinQRSquareMatrixOnFactoredMatrix()
        {
            var matrix = _matrices["Square3x3"];
            var a = new Complex32[matrix.RowCount*matrix.ColumnCount];
            Array.Copy(matrix.Values, a, a.Length);

            var tau = new Complex32[matrix.ColumnCount];
            var r = new Complex32[matrix.ColumnCount*matrix.ColumnCount];
            Control.LinearAlgebraProvider.ThinQRFactor(a, matrix.RowCount, matrix.ColumnCount, r, tau);

            var b = new[] {new Complex32(1.0f, 0.0f), 2.0f, 3.0f, 4.0f, 5.0f, 6.0f};
            var x = new Complex32[matrix.ColumnCount*2];
            Control.LinearAlgebraProvider.QRSolveFactored(a, r, matrix.RowCount, matrix.ColumnCount, tau, b, 2, x, QRMethod.Thin);

            var mx = new DenseMatrix(matrix.ColumnCount, 2, x);
            var mb = matrix*mx;

            AssertHelpers.AlmostEqual(mb[0, 0], b[0], 5);
            AssertHelpers.AlmostEqual(mb[1, 0], b[1], 5);
            AssertHelpers.AlmostEqual(mb[2, 0], b[2], 5);
            AssertHelpers.AlmostEqual(mb[0, 1], b[3], 5);
            AssertHelpers.AlmostEqual(mb[1, 1], b[4], 5);
            AssertHelpers.AlmostEqual(mb[2, 1], b[5], 5);
        }

        /// <summary>
        /// Can solve Ax=b using thin QR factorization with a tall A matrix
        /// using a factored A matrix.
        /// </summary>
        [Test]
        public void CanSolveUsingThinQRTallMatrixOnFactoredMatrix()
        {
            var matrix = _matrices["Tall3x2"];
            var a = new Complex32[matrix.RowCount*matrix.ColumnCount];
            Array.Copy(matrix.Values, a, a.Length);

            var tau = new Complex32[matrix.ColumnCount];
            var r = new Complex32[matrix.ColumnCount*matrix.ColumnCount];
            Control.LinearAlgebraProvider.ThinQRFactor(a, matrix.RowCount, matrix.ColumnCount, r, tau);

            var b = new[] {new Complex32(1.0f, 0.0f), 2.0f, 3.0f, 4.0f, 5.0f, 6.0f};
            var x = new Complex32[matrix.ColumnCount*2];
            Control.LinearAlgebraProvider.QRSolveFactored(a, r, matrix.RowCount, matrix.ColumnCount, tau, b, 2, x, QRMethod.Thin);

            var mb = new DenseMatrix(matrix.RowCount, 2, b);
            var test = (matrix.Transpose()*matrix).Inverse()*matrix.Transpose()*mb;

            AssertHelpers.AlmostEqual(test[0, 0], x[0], 6);
            AssertHelpers.AlmostEqual(test[1, 0], x[1], 6);
            AssertHelpers.AlmostEqual(test[0, 1], x[2], 6);
            AssertHelpers.AlmostEqual(test[1, 1], x[3], 6);
        }

        /// <summary>
        /// Can solve Ax=b using thin QR factorization with a square A matrix
        /// using a factored A matrix with a work array.
        /// </summary>
        [Test]
        public void CanSolveUsingThinQRSquareMatrixOnFactoredMatrixWithWorkArray()
        {
            var matrix = _matrices["Square3x3"];
            var a = new Complex32[matrix.RowCount*matrix.ColumnCount];
            Array.Copy(matrix.Values, a, a.Length);

            var tau = new Complex32[matrix.ColumnCount];
            var r = new Complex32[matrix.ColumnCount*matrix.ColumnCount];
            var work = new Complex32[2048];
            Control.LinearAlgebraProvider.ThinQRFactor(a, matrix.RowCount, matrix.ColumnCount, r, tau, work);

            var b = new[] {new Complex32(1.0f, 0.0f), 2.0f, 3.0f, 4.0f, 5.0f, 6.0f};
            var x = new Complex32[matrix.ColumnCount*2];
            Control.LinearAlgebraProvider.QRSolveFactored(a, r, matrix.RowCount, matrix.ColumnCount, tau, b, 2, x, work, QRMethod.Thin);

            var mx = new DenseMatrix(matrix.ColumnCount, 2, x);
            var mb = matrix*mx;

            AssertHelpers.AlmostEqual(mb[0, 0], b[0], 5);
            AssertHelpers.AlmostEqual(mb[1, 0], b[1], 5);
            AssertHelpers.AlmostEqual(mb[2, 0], b[2], 5);
            AssertHelpers.AlmostEqual(mb[0, 1], b[3], 5);
            AssertHelpers.AlmostEqual(mb[1, 1], b[4], 5);
            AssertHelpers.AlmostEqual(mb[2, 1], b[5], 5);
        }

        /// <summary>
        /// Can solve Ax=b using thin QR factorization with a tall A matrix
        /// using a factored A matrix with a work array.
        /// </summary>
        [Test]
        public void CanSolveUsingThinQRTallMatrixOnFactoredMatrixWithWorkArray()
        {
            var matrix = _matrices["Tall3x2"];
            var a = new Complex32[matrix.RowCount*matrix.ColumnCount];
            Array.Copy(matrix.Values, a, a.Length);

            var tau = new Complex32[matrix.ColumnCount];
            var r = new Complex32[matrix.ColumnCount*matrix.ColumnCount];
            var work = new Complex32[2048];
            Control.LinearAlgebraProvider.ThinQRFactor(a, matrix.RowCount, matrix.ColumnCount, r, tau, work);

            var b = new[] {new Complex32(1.0f, 0.0f), 2.0f, 3.0f, 4.0f, 5.0f, 6.0f};
            var x = new Complex32[matrix.ColumnCount*2];
            Control.LinearAlgebraProvider.QRSolveFactored(a, r, matrix.RowCount, matrix.ColumnCount, tau, b, 2, x, work, QRMethod.Thin);

            var mb = new DenseMatrix(matrix.RowCount, 2, b);
            var test = (matrix.Transpose()*matrix).Inverse()*matrix.Transpose()*mb;

            AssertHelpers.AlmostEqual(test[0, 0], x[0], 6);
            AssertHelpers.AlmostEqual(test[1, 0], x[1], 6);
            AssertHelpers.AlmostEqual(test[0, 1], x[2], 6);
            AssertHelpers.AlmostEqual(test[1, 1], x[3], 6);
        }

        /// <summary>
        /// Can compute the SVD factorization of a square matrix.
        /// </summary>
        [Test]
        public void CanComputeSVDFactorizationOfSquareMatrix()
        {
            var matrix = _matrices["Square3x3"];
            var a = new Complex32[matrix.RowCount*matrix.ColumnCount];
            Array.Copy(matrix.Values, a, a.Length);

            var s = new Complex32[matrix.RowCount];
            var u = new Complex32[matrix.RowCount*matrix.RowCount];
            var vt = new Complex32[matrix.ColumnCount*matrix.ColumnCount];

            Control.LinearAlgebraProvider.SingularValueDecomposition(true, a, matrix.RowCount, matrix.ColumnCount, s, u, vt);

            var w = new DenseMatrix(matrix.RowCount, matrix.ColumnCount);
            for (var index = 0; index < s.Length; index++)
            {
                w[index, index] = s[index];
            }

            var mU = new DenseMatrix(matrix.RowCount, matrix.RowCount, u);
            var mV = new DenseMatrix(matrix.ColumnCount, matrix.ColumnCount, vt);
            var result = mU*w*mV;

            AssertHelpers.AlmostEqual(matrix[0, 0], result[0, 0], 6);
            AssertHelpers.AlmostEqual(matrix[1, 0], result[1, 0], 6);
            AssertHelpers.AlmostEqual(matrix[2, 0], result[2, 0], 6);
            AssertHelpers.AlmostEqual(matrix[0, 1], result[0, 1], 6);
            AssertHelpers.AlmostEqual(matrix[1, 1], result[1, 1], 6);
            AssertHelpers.AlmostEqual(matrix[2, 1], result[2, 1], 6);
            AssertHelpers.AlmostEqual(matrix[0, 2], result[0, 2], 6);
            AssertHelpers.AlmostEqual(matrix[1, 2], result[1, 2], 6);
            AssertHelpers.AlmostEqual(matrix[2, 2], result[2, 2], 6);
        }

        /// <summary>
        /// Can compute the SVD factorization of a tall matrix.
        /// </summary>
        [Test]
        public void CanComputeSVDFactorizationOfTallMatrix()
        {
            var matrix = _matrices["Tall3x2"];
            var a = new Complex32[matrix.RowCount*matrix.ColumnCount];
            Array.Copy(matrix.Values, a, a.Length);

            var s = new Complex32[matrix.ColumnCount];
            var u = new Complex32[matrix.RowCount*matrix.RowCount];
            var vt = new Complex32[matrix.ColumnCount*matrix.ColumnCount];

            Control.LinearAlgebraProvider.SingularValueDecomposition(true, a, matrix.RowCount, matrix.ColumnCount, s, u, vt);

            var w = new DenseMatrix(matrix.RowCount, matrix.ColumnCount);
            for (var index = 0; index < s.Length; index++)
            {
                w[index, index] = s[index];
            }

            var mU = new DenseMatrix(matrix.RowCount, matrix.RowCount, u);
            var mV = new DenseMatrix(matrix.ColumnCount, matrix.ColumnCount, vt);
            var result = mU*w*mV;

            AssertHelpers.AlmostEqual(matrix[0, 0], result[0, 0], 6);
            AssertHelpers.AlmostEqual(matrix[1, 0], result[1, 0], 6);
            AssertHelpers.AlmostEqual(matrix[2, 0], result[2, 0], 6);
            AssertHelpers.AlmostEqual(matrix[0, 1], result[0, 1], 6);
            AssertHelpers.AlmostEqual(matrix[1, 1], result[1, 1], 6);
            AssertHelpers.AlmostEqual(matrix[2, 1], result[2, 1], 5);
        }

        /// <summary>
        /// Can compute the SVD factorization of a wide matrix.
        /// </summary>
        [Test]
        public void CanComputeSVDFactorizationOfWideMatrix()
        {
            var matrix = _matrices["Wide2x3"];
            var a = new Complex32[matrix.RowCount*matrix.ColumnCount];
            Array.Copy(matrix.Values, a, a.Length);

            var s = new Complex32[matrix.RowCount];
            var u = new Complex32[matrix.RowCount*matrix.RowCount];
            var vt = new Complex32[matrix.ColumnCount*matrix.ColumnCount];

            Control.LinearAlgebraProvider.SingularValueDecomposition(true, a, matrix.RowCount, matrix.ColumnCount, s, u, vt);

            var w = new DenseMatrix(matrix.RowCount, matrix.ColumnCount);
            for (var index = 0; index < s.Length; index++)
            {
                w[index, index] = s[index];
            }

            var mU = new DenseMatrix(matrix.RowCount, matrix.RowCount, u);
            var mV = new DenseMatrix(matrix.ColumnCount, matrix.ColumnCount, vt);
            var result = mU*w*mV;

            AssertHelpers.AlmostEqual(matrix[0, 0], result[0, 0], 6);
            AssertHelpers.AlmostEqual(matrix[1, 0], result[1, 0], 6);
            AssertHelpers.AlmostEqual(matrix[0, 1], result[0, 1], 6);
            AssertHelpers.AlmostEqual(matrix[1, 1], result[1, 1], 6);
            AssertHelpers.AlmostEqual(matrix[0, 2], result[0, 2], 6);
            AssertHelpers.AlmostEqual(matrix[1, 2], result[1, 2], 5);
        }

        /// <summary>
        /// Can compute the SVD factorization of a square matrix using
        /// a work array.
        /// </summary>
        [Test]
        public void CanComputeSVDFactorizationOfSquareMatrixWithWorkArray()
        {
            var matrix = _matrices["Square3x3"];
            var a = new Complex32[matrix.RowCount*matrix.ColumnCount];
            Array.Copy(matrix.Values, a, a.Length);

            var s = new Complex32[matrix.RowCount];
            var u = new Complex32[matrix.RowCount*matrix.RowCount];
            var vt = new Complex32[matrix.ColumnCount*matrix.ColumnCount];
            var work = new Complex32[100];

            Control.LinearAlgebraProvider.SingularValueDecomposition(true, a, matrix.RowCount, matrix.ColumnCount, s, u, vt, work);

            var w = new DenseMatrix(matrix.RowCount, matrix.ColumnCount);
            for (var index = 0; index < s.Length; index++)
            {
                w[index, index] = s[index];
            }

            var mU = new DenseMatrix(matrix.RowCount, matrix.RowCount, u);
            var mV = new DenseMatrix(matrix.ColumnCount, matrix.ColumnCount, vt);
            var result = mU*w*mV;

            AssertHelpers.AlmostEqual(matrix[0, 0], result[0, 0], 6);
            AssertHelpers.AlmostEqual(matrix[1, 0], result[1, 0], 6);
            AssertHelpers.AlmostEqual(matrix[2, 0], result[2, 0], 6);
            AssertHelpers.AlmostEqual(matrix[0, 1], result[0, 1], 6);
            AssertHelpers.AlmostEqual(matrix[1, 1], result[1, 1], 6);
            AssertHelpers.AlmostEqual(matrix[2, 1], result[2, 1], 6);
            AssertHelpers.AlmostEqual(matrix[0, 2], result[0, 2], 6);
            AssertHelpers.AlmostEqual(matrix[1, 2], result[1, 2], 6);
            AssertHelpers.AlmostEqual(matrix[2, 2], result[2, 2], 6);
        }

        /// <summary>
        /// Can compute the SVD factorization of a tall matrix using
        /// a work array.
        /// </summary>
        [Test]
        public void CanComputeSVDFactorizationOfTallMatrixWithWorkArray()
        {
            var matrix = _matrices["Tall3x2"];
            var a = new Complex32[matrix.RowCount*matrix.ColumnCount];
            Array.Copy(matrix.Values, a, a.Length);

            var s = new Complex32[matrix.ColumnCount];
            var u = new Complex32[matrix.RowCount*matrix.RowCount];
            var vt = new Complex32[matrix.ColumnCount*matrix.ColumnCount];
            var work = new Complex32[100];

            Control.LinearAlgebraProvider.SingularValueDecomposition(true, a, matrix.RowCount, matrix.ColumnCount, s, u, vt, work);

            var w = new DenseMatrix(matrix.RowCount, matrix.ColumnCount);
            for (var index = 0; index < s.Length; index++)
            {
                w[index, index] = s[index];
            }

            var mU = new DenseMatrix(matrix.RowCount, matrix.RowCount, u);
            var mV = new DenseMatrix(matrix.ColumnCount, matrix.ColumnCount, vt);
            var result = mU*w*mV;

            AssertHelpers.AlmostEqual(matrix[0, 0], result[0, 0], 6);
            AssertHelpers.AlmostEqual(matrix[1, 0], result[1, 0], 6);
            AssertHelpers.AlmostEqual(matrix[2, 0], result[2, 0], 6);
            AssertHelpers.AlmostEqual(matrix[0, 1], result[0, 1], 6);
            AssertHelpers.AlmostEqual(matrix[1, 1], result[1, 1], 6);
            AssertHelpers.AlmostEqual(matrix[2, 1], result[2, 1], 5);
        }

        /// <summary>
        /// Can compute the SVD factorization of a wide matrix using
        /// a work array.
        /// </summary>
        [Test]
        public void CanComputeSVDFactorizationOfWideMatrixWithWorkArray()
        {
            var matrix = _matrices["Wide2x3"];
            var a = new Complex32[matrix.RowCount*matrix.ColumnCount];
            Array.Copy(matrix.Values, a, a.Length);

            var s = new Complex32[matrix.RowCount];
            var u = new Complex32[matrix.RowCount*matrix.RowCount];
            var vt = new Complex32[matrix.ColumnCount*matrix.ColumnCount];
            var work = new Complex32[100];

            Control.LinearAlgebraProvider.SingularValueDecomposition(true, a, matrix.RowCount, matrix.ColumnCount, s, u, vt, work);

            var w = new DenseMatrix(matrix.RowCount, matrix.ColumnCount);
            for (var index = 0; index < s.Length; index++)
            {
                w[index, index] = s[index];
            }

            var mU = new DenseMatrix(matrix.RowCount, matrix.RowCount, u);
            var mV = new DenseMatrix(matrix.ColumnCount, matrix.ColumnCount, vt);
            var result = mU*w*mV;

            AssertHelpers.AlmostEqual(matrix[0, 0], result[0, 0], 6);
            AssertHelpers.AlmostEqual(matrix[1, 0], result[1, 0], 6);
            AssertHelpers.AlmostEqual(matrix[0, 1], result[0, 1], 6);
            AssertHelpers.AlmostEqual(matrix[1, 1], result[1, 1], 6);
            AssertHelpers.AlmostEqual(matrix[0, 2], result[0, 2], 6);
            AssertHelpers.AlmostEqual(matrix[1, 2], result[1, 2], 5);
        }

        /// <summary>
        /// Can solve Ax=b using SVD factorization with a square A matrix.
        /// </summary>
        [Test]
        public void CanSolveUsingSVDSquareMatrix()
        {
            var matrix = _matrices["Square3x3"];
            var a = new Complex32[matrix.RowCount*matrix.ColumnCount];
            Array.Copy(matrix.Values, a, a.Length);

            var b = new[] {new Complex32(1.0f, 0.0f), 2.0f, 3.0f, 4.0f, 5.0f, 6.0f};
            var x = new Complex32[matrix.ColumnCount*2];
            Control.LinearAlgebraProvider.SvdSolve(a, matrix.RowCount, matrix.ColumnCount, b, 2, x);

            NotModified(3, 3, a, matrix);

            var mx = new DenseMatrix(matrix.ColumnCount, 2, x);
            var mb = matrix*mx;

            AssertHelpers.AlmostEqual(mb[0, 0], b[0], 6);
            AssertHelpers.AlmostEqual(mb[1, 0], b[1], 6);
            AssertHelpers.AlmostEqual(mb[2, 0], b[2], 5);
            AssertHelpers.AlmostEqual(mb[0, 1], b[3], 5);
            AssertHelpers.AlmostEqual(mb[1, 1], b[4], 5);
            AssertHelpers.AlmostEqual(mb[2, 1], b[5], 5);
        }

        /// <summary>
        /// Can solve Ax=b using SVD factorization with a tall A matrix.
        /// </summary>
        [Test]
        public void CanSolveUsingSvdTallMatrix()
        {
            var matrix = _matrices["Tall3x2"];
            var a = new Complex32[matrix.RowCount*matrix.ColumnCount];
            Array.Copy(matrix.Values, a, a.Length);

            var b = new[] {new Complex32(1.0f, 0.0f), 2.0f, 3.0f, 4.0f, 5.0f, 6.0f};
            var x = new Complex32[matrix.ColumnCount*2];
            Control.LinearAlgebraProvider.SvdSolve(a, matrix.RowCount, matrix.ColumnCount, b, 2, x);

            NotModified(3, 2, a, matrix);

            var mb = new DenseMatrix(matrix.RowCount, 2, b);
            var test = (matrix.Transpose()*matrix).Inverse()*matrix.Transpose()*mb;

            AssertHelpers.AlmostEqual(test[0, 0], x[0], 5);
            AssertHelpers.AlmostEqual(test[1, 0], x[1], 5);
            AssertHelpers.AlmostEqual(test[0, 1], x[2], 5);
            AssertHelpers.AlmostEqual(test[1, 1], x[3], 5);
        }

        /// <summary>
        /// Can solve Ax=b using SVD factorization with a square A matrix
        /// using a factored matrix.
        /// </summary>
        [Test]
        public void CanSolveUsingSvdSquareMatrixOnFactoredMatrix()
        {
            var matrix = _matrices["Square3x3"];
            var a = new Complex32[matrix.RowCount*matrix.ColumnCount];
            Array.Copy(matrix.Values, a, a.Length);

            var s = new Complex32[matrix.RowCount];
            var u = new Complex32[matrix.RowCount*matrix.RowCount];
            var vt = new Complex32[matrix.ColumnCount*matrix.ColumnCount];

            Control.LinearAlgebraProvider.SingularValueDecomposition(true, a, matrix.RowCount, matrix.ColumnCount, s, u, vt);

            var b = new[] {new Complex32(1.0f, 0.0f), 2.0f, 3.0f, 4.0f, 5.0f, 6.0f};
            var x = new Complex32[matrix.ColumnCount*2];
            Control.LinearAlgebraProvider.SvdSolveFactored(matrix.RowCount, matrix.ColumnCount, s, u, vt, b, 2, x);

            var mx = new DenseMatrix(matrix.ColumnCount, 2, x);
            var mb = matrix*mx;

            AssertHelpers.AlmostEqual(mb[0, 0], b[0], 5);
            AssertHelpers.AlmostEqual(mb[1, 0], b[1], 5);
            AssertHelpers.AlmostEqual(mb[2, 0], b[2], 5);
            AssertHelpers.AlmostEqual(mb[0, 1], b[3], 5);
            AssertHelpers.AlmostEqual(mb[1, 1], b[4], 5);
            AssertHelpers.AlmostEqual(mb[2, 1], b[5], 5);
        }

        /// <summary>
        /// Can solve Ax=b using SVD factorization with a tall A matrix
        /// using a factored matrix.
        /// </summary>
        [Test]
        public void CanSolveUsingSvdTallMatrixOnFactoredMatrix()
        {
            var matrix = _matrices["Tall3x2"];
            var a = new Complex32[matrix.RowCount*matrix.ColumnCount];
            Array.Copy(matrix.Values, a, a.Length);

            var s = new Complex32[matrix.ColumnCount];
            var u = new Complex32[matrix.RowCount*matrix.RowCount];
            var vt = new Complex32[matrix.ColumnCount*matrix.ColumnCount];

            Control.LinearAlgebraProvider.SingularValueDecomposition(true, a, matrix.RowCount, matrix.ColumnCount, s, u, vt);

            var b = new[] {new Complex32(1.0f, 0.0f), 2.0f, 3.0f, 4.0f, 5.0f, 6.0f};
            var x = new Complex32[matrix.ColumnCount*2];
            Control.LinearAlgebraProvider.SvdSolveFactored(matrix.RowCount, matrix.ColumnCount, s, u, vt, b, 2, x);

            var mb = new DenseMatrix(matrix.RowCount, 2, b);
            var test = (matrix.Transpose()*matrix).Inverse()*matrix.Transpose()*mb;

            AssertHelpers.AlmostEqual(test[0, 0], x[0], 5);
            AssertHelpers.AlmostEqual(test[1, 0], x[1], 5);
            AssertHelpers.AlmostEqual(test[0, 1], x[2], 5);
            AssertHelpers.AlmostEqual(test[1, 1], x[3], 5);
        }

        [TestCase("Wide10x50000", "Tall50000x10")]
        [TestCase("Square1000x1000", "Square1000x1000")]
        [Explicit, Timeout(1000*10)]
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
        static void NotModified(int rows, int columns, IList<Complex32> array, Matrix<Complex32> matrix)
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
