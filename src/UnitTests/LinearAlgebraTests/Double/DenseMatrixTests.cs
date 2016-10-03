// <copyright file="DenseMatrixTests.cs" company="Math.NET">
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
    /// Dense matrix tests.
    /// </summary>
    public class DenseMatrixTests : MatrixTests
    {
        /// <summary>
        /// Creates a matrix for the given number of rows and columns.
        /// </summary>
        /// <param name="rows">The number of rows.</param>
        /// <param name="columns">The number of columns.</param>
        /// <returns>A matrix with the given dimensions.</returns>
        protected override Matrix<double> CreateMatrix(int rows, int columns)
        {
            return Matrix<double>.Build.Dense(rows, columns);
        }

        /// <summary>
        /// Creates a matrix from a 2D array.
        /// </summary>
        /// <param name="data">The 2D array to create this matrix from.</param>
        /// <returns>A matrix with the given values.</returns>
        protected override Matrix<double> CreateMatrix(double[,] data)
        {
            return Matrix<double>.Build.DenseOfArray(data);
        }

        /// <summary>
        /// Can create a matrix form array.
        /// </summary>
        [Test]
        public void CanCreateMatrixFrom1DArray()
        {
            var testData = new Dictionary<string, Matrix<double>>
                {
                    {"Singular3x3", Matrix<double>.Build.Dense(3, 3, new[] {1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 2.0, 2.0, 2.0})},
                    {"Square3x3", Matrix<double>.Build.Dense(3, 3, new[] {-1.1, 0.0, -4.4, -2.2, 1.1, 5.5, -3.3, 2.2, 6.6})},
                    {"Square4x4", Matrix<double>.Build.Dense(4, 4, new[] {-1.1, 0.0, 1.0, -4.4, -2.2, 1.1, 2.1, 5.5, -3.3, 2.2, 6.2, 6.6, -4.4, 3.3, 4.3, -7.7})},
                    {"Tall3x2", Matrix<double>.Build.Dense(3, 2, new[] {-1.1, 0.0, -4.4, -2.2, 1.1, 5.5})},
                    {"Wide2x3", Matrix<double>.Build.Dense(2, 3, new[] {-1.1, 0.0, -2.2, 1.1, -3.3, 2.2})}
                };

            foreach (var name in testData.Keys)
            {
                Assert.AreEqual(TestMatrices[name], testData[name]);
            }
        }

        /// <summary>
        /// Matrix from array is a reference.
        /// </summary>
        [Test]
        public void MatrixFrom1DArrayIsReference()
        {
            var data = new double[] {1, 1, 1, 1, 1, 1, 2, 2, 2};
            var matrix = Matrix<double>.Build.Dense(3, 3, data);
            matrix[0, 0] = 10.0;
            Assert.AreEqual(10.0, data[0]);
        }

        /// <summary>
        /// Matrix from two-dimensional array is a copy.
        /// </summary>
        [Test]
        public void MatrixFrom2DArrayIsCopy()
        {
            var matrix = Matrix<double>.Build.DenseOfArray(TestData2D["Singular3x3"]);
            Assert.That(matrix, Is.TypeOf<DenseMatrix>());
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
            var matrix = Matrix<double>.Build.DenseOfArray(TestData2D[name]);
            Assert.That(matrix, Is.TypeOf<DenseMatrix>());
            for (var i = 0; i < TestData2D[name].GetLength(0); i++)
            {
                for (var j = 0; j < TestData2D[name].GetLength(1); j++)
                {
                    Assert.AreEqual(TestData2D[name][i, j], matrix[i, j]);
                }
            }
        }

        /// <summary>
        /// Can create a matrix with uniform values.
        /// </summary>
        [Test]
        public void CanCreateMatrixWithUniformValues()
        {
            var matrix = Matrix<double>.Build.Dense(10, 10, 10.0);
            Assert.That(matrix, Is.TypeOf<DenseMatrix>());
            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    Assert.AreEqual(matrix[i, j], 10.0);
                }
            }
        }

        /// <summary>
        /// Can create an identity matrix.
        /// </summary>
        [Test]
        public void CanCreateIdentity()
        {
            var matrix = Matrix<double>.Build.DenseIdentity(5);
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
            Assert.That(() => Matrix<double>.Build.DenseIdentity(order), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void MatrixPower()
        {
            var d = Matrix<double>.Build.Random(3, 3, 1);
            var d2 = d*d;
            var d3 = d2*d;
            var d4 = d3*d;
            var d5 = d4*d;
            var d6 = d5*d;
            var d7 = d6*d;
            var d8 = d7*d;

            AssertHelpers.AlmostEqual(Matrix<double>.Build.DiagonalIdentity(3), d.Power(0), 10);
            AssertHelpers.AlmostEqual(d, d.Power(1), 10);
            AssertHelpers.AlmostEqual(d2, d.Power(2), 10);
            AssertHelpers.AlmostEqual(d3, d.Power(3), 10);
            AssertHelpers.AlmostEqual(d4, d.Power(4), 10);
            AssertHelpers.AlmostEqual(d5, d.Power(5), 10);
            AssertHelpers.AlmostEqual(d6, d.Power(6), 10);
            AssertHelpers.AlmostEqual(d7, d.Power(7), 10);
            AssertHelpers.AlmostEqual(d8, d.Power(8), 10);
        }

        [Test]
        public void MatrixToMatrixString()
        {
            var m = Matrix<double>.Build.Dense(20, 10);
            for (int i = 1; i < 25; i++)
            {
                for (int j = 1; j < 25; j++)
                {
                    GC.KeepAlive(m.ToMatrixString(i, j));
                }
            }
        }
    }
}
