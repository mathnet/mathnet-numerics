// <copyright file="SparseMatrixTests.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2016 Math.NET
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
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using NUnit.Framework;

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Double
{
    /// <summary>
    /// Sparse matrix tests.
    /// </summary>
    public class SparseMatrixTests : MatrixTests
    {
        /// <summary>
        /// Creates a matrix for the given number of rows and columns.
        /// </summary>
        /// <param name="rows">The number of rows.</param>
        /// <param name="columns">The number of columns.</param>
        /// <returns>A matrix with the given dimensions.</returns>
        protected override Matrix<double> CreateMatrix(int rows, int columns)
        {
            return Matrix<double>.Build.Sparse(rows, columns);
        }

        /// <summary>
        /// Creates a matrix from a 2D array.
        /// </summary>
        /// <param name="data">The 2D array to create this matrix from.</param>
        /// <returns>A matrix with the given values.</returns>
        protected override Matrix<double> CreateMatrix(double[,] data)
        {
            return Matrix<double>.Build.SparseOfArray(data);
        }

        /// <summary>
        /// Can create a matrix form array.
        /// </summary>
        [Test]
        public void CanCreateMatrixFrom1DArray()
        {
            var testData = new Dictionary<string, Matrix<double>>
                {
                    {"Singular3x3", Matrix<double>.Build.SparseOfColumnMajor(3, 3, new double[] {1, 1, 1, 1, 1, 1, 2, 2, 2})},
                    {"Square3x3", Matrix<double>.Build.SparseOfColumnMajor(3, 3, new[] {-1.1, 0.0, -4.4, -2.2, 1.1, 5.5, -3.3, 2.2, 6.6})},
                    {"Square4x4", Matrix<double>.Build.SparseOfColumnMajor(4, 4, new[] {-1.1, 0.0, 1.0, -4.4, -2.2, 1.1, 2.1, 5.5, -3.3, 2.2, 6.2, 6.6, -4.4, 3.3, 4.3, -7.7})},
                    {"Tall3x2", Matrix<double>.Build.SparseOfColumnMajor(3, 2, new[] {-1.1, 0.0, -4.4, -2.2, 1.1, 5.5})},
                    {"Wide2x3", Matrix<double>.Build.SparseOfColumnMajor(2, 3, new[] {-1.1, 0.0, -2.2, 1.1, -3.3, 2.2})}
                };

            foreach (var name in testData.Keys)
            {
                Assert.AreEqual(TestMatrices[name], testData[name]);
            }
        }

        /// <summary>
        /// Matrix from array is a copy.
        /// </summary>
        [Test]
        public void MatrixFrom1DArrayIsCopy()
        {
            // Sparse Matrix copies values from double[], but no remember reference.
            var data = new double[] {1, 1, 1, 1, 1, 1, 2, 2, 2};
            var matrix = Matrix<double>.Build.SparseOfColumnMajor(3, 3, data);
            matrix[0, 0] = 10.0;
            Assert.AreNotEqual(10.0, data[0]);
        }

        /// <summary>
        /// Matrix from two-dimensional array is a copy.
        /// </summary>
        [Test]
        public void MatrixFrom2DArrayIsCopy()
        {
            var matrix = Matrix<double>.Build.SparseOfArray(TestData2D["Singular3x3"]);
            matrix[0, 0] = 10.0;
            Assert.AreEqual(1.0, TestData2D["Singular3x3"][0, 0]);
        }

        /// <summary>
        /// Can create a matrix from two-dimensional array.
        /// </summary>
        /// <param name="name">Matrix name.</param>
        [TestCase("Singular3x3")]
        [TestCase("Singular4x4")]
        [TestCase("Square3x3")]
        [TestCase("Square4x4")]
        [TestCase("Tall3x2")]
        [TestCase("Wide2x3")]
        public void CanCreateMatrixFrom2DArray(string name)
        {
            var matrix = Matrix<double>.Build.SparseOfArray(TestData2D[name]);
            for (var i = 0; i < TestData2D[name].GetLength(0); i++)
            {
                for (var j = 0; j < TestData2D[name].GetLength(1); j++)
                {
                    Assert.AreEqual(TestData2D[name][i, j], matrix[i, j]);
                }
            }
        }

        /// <summary>
        /// Can create an identity matrix.
        /// </summary>
        [Test]
        public void CanCreateIdentity()
        {
            var matrix = Matrix<double>.Build.SparseIdentity(5);
            Assert.That(matrix, Is.TypeOf<SparseMatrix>());
            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    Assert.AreEqual(i == j ? 1.0 : 0.0, matrix[i, j]);
                }
            }
        }

        /// <summary>
        /// Identity with wrong order throws <c>ArgumentOutOfRangeException</c>.
        /// </summary>
        /// <param name="order">The size of the square matrix</param>
        [TestCase(0)]
        [TestCase(-1)]
        public void IdentityWithWrongOrderThrowsArgumentOutOfRangeException(int order)
        {
            Assert.That(() => Matrix<double>.Build.SparseIdentity(order), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        /// <summary>
        /// Can create a large sparse matrix
        /// </summary>
        [Test]
        public void CanCreateLargeSparseMatrix()
        {
            var matrix = new SparseMatrix(500, 1000);
            var nonzero = 0;
            var rnd = new System.Random(0);

            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    var value = rnd.Next(10)*rnd.Next(10)*rnd.Next(10)*rnd.Next(10)*rnd.Next(10);
                    if (value != 0)
                    {
                        nonzero++;
                    }

                    matrix[i, j] = value;
                }
            }

            Assert.AreEqual(matrix.NonZerosCount, nonzero);
        }

        /// <summary>
        /// Test whether order matters when adding sparse matrices.
        /// </summary>
        [Test]
        public void CanAddSparseMatricesBothWays()
        {
            var m1 = new SparseMatrix(1, 3);
            var m2 = Matrix<double>.Build.SparseOfArray(new double[,] { { 0, 1, 1 } });
            var sum1 = m1 + m2;
            var sum2 = m2 + m1;
            Assert.IsTrue(sum1.Equals(m2));
            Assert.IsTrue(sum1.Equals(sum2));

            Matrix<double> sparseResult = new SparseMatrix(1, 3);
            sparseResult.Add(m2, sparseResult);
            Assert.IsTrue(sparseResult.Equals(sum1));

            sparseResult = Matrix<double>.Build.SparseOfArray(new double[,] { { 0, 1, 1 } });
            sparseResult.Add(m1, sparseResult);
            Assert.IsTrue(sparseResult.Equals(sum1));

            sparseResult = Matrix<double>.Build.SparseOfArray(new double[,] { { 0, 1, 1 } });
            m1.Add(sparseResult, sparseResult);
            Assert.IsTrue(sparseResult.Equals(sum1));

            sparseResult = Matrix<double>.Build.SparseOfArray(new double[,] { { 0, 1, 1 } });
            sparseResult.Add(sparseResult, sparseResult);
            Assert.IsTrue(sparseResult.Equals(2*sum1));

            Matrix<double> denseResult = Matrix<double>.Build.Dense(1, 3);
            denseResult.Add(m2, denseResult);
            Assert.IsTrue(denseResult.Equals(sum1));

            denseResult = Matrix<double>.Build.DenseOfArray(new double[,] { { 0, 1, 1 } });
            denseResult.Add(m1, denseResult);
            Assert.IsTrue(denseResult.Equals(sum1));

            var m3 = Matrix<double>.Build.DenseOfArray(new double[,] { { 0, 1, 1 } });
            var sum3 = m1 + m3;
            var sum4 = m3 + m1;
            Assert.IsTrue(sum3.Equals(m3));
            Assert.IsTrue(sum3.Equals(sum4));
        }

        /// <summary>
        /// Test whether order matters when subtracting sparse matrices.
        /// </summary>
        [Test]
        public void CanSubtractSparseMatricesBothWays()
        {
            var m1 = new SparseMatrix(1, 3);
            var m2 = Matrix<double>.Build.SparseOfArray(new double[,] { { 0, 1, 1 } });
            var diff1 = m1 - m2;
            var diff2 = m2 - m1;
            Assert.IsTrue(diff1.Equals(m2.Negate()));
            Assert.IsTrue(diff1.Equals(diff2.Negate()));

            Matrix<double> sparseResult = new SparseMatrix(1, 3);
            sparseResult.Subtract(m2, sparseResult);
            Assert.IsTrue(sparseResult.Equals(diff1));

            sparseResult = Matrix<double>.Build.SparseOfArray(new double[,] { { 0, 1, 1 } });
            sparseResult.Subtract(m1, sparseResult);
            Assert.IsTrue(sparseResult.Equals(diff2));

            sparseResult = Matrix<double>.Build.SparseOfArray(new double[,] { { 0, 1, 1 } });
            m1.Subtract(sparseResult, sparseResult);
            Assert.IsTrue(sparseResult.Equals(diff1));

            sparseResult = Matrix<double>.Build.SparseOfArray(new double[,] { { 0, 1, 1 } });
            sparseResult.Subtract(sparseResult, sparseResult);
            Assert.IsTrue(sparseResult.Equals(0*diff1));

            Matrix<double> denseResult = Matrix<double>.Build.Dense(1, 3);
            denseResult.Subtract(m2, denseResult);
            Assert.IsTrue(denseResult.Equals(diff1));

            denseResult = Matrix<double>.Build.DenseOfArray(new double[,] { { 0, 1, 1 } });
            denseResult.Subtract(m1, denseResult);
            Assert.IsTrue(denseResult.Equals(diff2));

            var m3 = Matrix<double>.Build.DenseOfArray(new double[,] { { 0, 1, 1 } });
            var diff3 = m1 - m3;
            var diff4 = m3 - m1;
            Assert.IsTrue(diff3.Equals(m3.Negate()));
            Assert.IsTrue(diff3.Equals(diff4.Negate()));
        }

        /// <summary>
        /// Test whether we can create a large sparse matrix
        /// </summary>
        [Test]
        public void CanCreateLargeMatrix()
        {
            const int Order = 1000000;
            var matrix = Matrix<double>.Build.Sparse(Order, Order);
            Assert.AreEqual(Order, matrix.RowCount);
            Assert.AreEqual(Order, matrix.ColumnCount);
            Assert.DoesNotThrow(() => matrix[0, 0] = 1);
        }
    }
}
