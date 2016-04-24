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
using MathNet.Numerics.LinearAlgebra.Complex;
using NUnit.Framework;

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Complex
{
#if NOSYSNUMERICS
    using Complex = Numerics.Complex;
#else
    using Complex = System.Numerics.Complex;
#endif

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
        protected override Matrix<Complex> CreateMatrix(int rows, int columns)
        {
            return new DenseMatrix(rows, columns);
        }

        /// <summary>
        /// Creates a matrix from a 2D array.
        /// </summary>
        /// <param name="data">The 2D array to create this matrix from.</param>
        /// <returns>A matrix with the given values.</returns>
        protected override Matrix<Complex> CreateMatrix(Complex[,] data)
        {
            return DenseMatrix.OfArray(data);
        }

        /// <summary>
        /// Can create a matrix form array.
        /// </summary>
        [Test]
        public void CanCreateMatrixFrom1DArray()
        {
            var testData = new Dictionary<string, Matrix<Complex>>
                {
                    {"Singular3x3", new DenseMatrix(3, 3, new[] {new Complex(1.0, 1), new Complex(1.0, 1), new Complex(1.0, 1), new Complex(1.0, 1), new Complex(1.0, 1), new Complex(1.0, 1), new Complex(2.0, 1), new Complex(2.0, 1), new Complex(2.0, 1)})},
                    {"Square3x3", new DenseMatrix(3, 3, new[] {new Complex(-1.1, 1), Complex.Zero, new Complex(-4.4, 1), new Complex(-2.2, 1), new Complex(1.1, 1), new Complex(5.5, 1), new Complex(-3.3, 1), new Complex(2.2, 1), new Complex(6.6, 1)})},
                    {"Square4x4", new DenseMatrix(4, 4, new[] {new Complex(-1.1, 1), Complex.Zero, new Complex(1.0, 1), new Complex(-4.4, 1), new Complex(-2.2, 1), new Complex(1.1, 1), new Complex(2.1, 1), new Complex(5.5, 1), new Complex(-3.3, 1), new Complex(2.2, 1), new Complex(6.2, 1), new Complex(6.6, 1), new Complex(-4.4, 1), new Complex(3.3, 1), new Complex(4.3, 1), new Complex(-7.7, 1)})},
                    {"Tall3x2", new DenseMatrix(3, 2, new[] {new Complex(-1.1, 1), Complex.Zero, new Complex(-4.4, 1), new Complex(-2.2, 1), new Complex(1.1, 1), new Complex(5.5, 1)})},
                    {"Wide2x3", new DenseMatrix(2, 3, new[] {new Complex(-1.1, 1), Complex.Zero, new Complex(-2.2, 1), new Complex(1.1, 1), new Complex(-3.3, 1), new Complex(2.2, 1)})}
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
            var data = new[] {new Complex(1.0, 1), new Complex(1.0, 1), new Complex(1.0, 1), new Complex(1.0, 1), new Complex(1.0, 1), new Complex(1.0, 1), new Complex(2.0, 1), new Complex(2.0, 1), new Complex(2.0, 1)};
            var matrix = new DenseMatrix(3, 3, data);
            matrix[0, 0] = new Complex(10.0, 1);
            Assert.AreEqual(new Complex(10.0, 1), data[0]);
        }

        /// <summary>
        /// Matrix from two-dimensional array is a copy.
        /// </summary>
        [Test]
        public void MatrixFrom2DArrayIsCopy()
        {
            var matrix = DenseMatrix.OfArray(TestData2D["Singular3x3"]);
            matrix[0, 0] = 10.0;
            Assert.AreEqual(new Complex(1.0, 1), TestData2D["Singular3x3"][0, 0]);
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
            var matrix = DenseMatrix.OfArray(TestData2D[name]);
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
            var matrix = DenseMatrix.Create(10, 10, new Complex(10.0, 1));
            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    Assert.AreEqual(matrix[i, j], new Complex(10.0, 1));
                }
            }
        }

        /// <summary>
        /// Can create an identity matrix.
        /// </summary>
        [Test]
        public void CanCreateIdentity()
        {
            var matrix = DenseMatrix.CreateIdentity(5);
            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    Assert.AreEqual(i == j ? Complex.One : Complex.Zero, matrix[i, j]);
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
            Assert.That(() => DenseMatrix.CreateIdentity(order), Throws.TypeOf<ArgumentOutOfRangeException>());
        }
    }
}
