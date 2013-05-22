// <copyright file="DenseMatrixTests.cs" company="Math.NET">
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

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Complex32
{
    using LinearAlgebra.Complex32;
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;
    using Complex32 = Numerics.Complex32;

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
        protected override Matrix CreateMatrix(int rows, int columns)
        {
            return new DenseMatrix(rows, columns);
        }

        /// <summary>
        /// Creates a matrix from a 2D array.
        /// </summary>
        /// <param name="data">The 2D array to create this matrix from.</param>
        /// <returns>A matrix with the given values.</returns>
        protected override Matrix CreateMatrix(Complex32[,] data)
        {
            return DenseMatrix.OfArray(data);
        }

        /// <summary>
        /// Creates a vector of the given size.
        /// </summary>
        /// <param name="size">The size of the vector to create.
        /// </param>
        /// <returns>The new vector. </returns>
        protected override Vector CreateVector(int size)
        {
            return new DenseVector(size);
        }

        /// <summary>
        /// Creates a vector from an array.
        /// </summary>
        /// <param name="data">The array to create this vector from.</param>
        /// <returns>The new vector. </returns>
        protected override Vector CreateVector(Complex32[] data)
        {
            return new DenseVector(data);
        }

        /// <summary>
        /// Can create a matrix form array.
        /// </summary>
        [Test]
        public void CanCreateMatrixFrom1DArray()
        {
            var testData = new Dictionary<string, Matrix>
                {
                    {"Singular3x3", new DenseMatrix(3, 3, new[] {new Complex32(1.0f, 1), new Complex32(1.0f, 1), new Complex32(1.0f, 1), new Complex32(1.0f, 1), new Complex32(1.0f, 1), new Complex32(1.0f, 1), new Complex32(2.0f, 1), new Complex32(2.0f, 1), new Complex32(2.0f, 1)})},
                    {"Square3x3", new DenseMatrix(3, 3, new[] {new Complex32(-1.1f, 1), Complex32.Zero, new Complex32(-4.4f, 1), new Complex32(-2.2f, 1), new Complex32(1.1f, 1), new Complex32(5.5f, 1), new Complex32(-3.3f, 1), new Complex32(2.2f, 1), new Complex32(6.6f, 1)})},
                    {"Square4x4", new DenseMatrix(4, 4, new[] {new Complex32(-1.1f, 1), Complex32.Zero, new Complex32(1.0f, 1), new Complex32(-4.4f, 1), new Complex32(-2.2f, 1), new Complex32(1.1f, 1), new Complex32(2.1f, 1), new Complex32(5.5f, 1), new Complex32(-3.3f, 1), new Complex32(2.2f, 1), new Complex32(6.2f, 1), new Complex32(6.6f, 1), new Complex32(-4.4f, 1), new Complex32(3.3f, 1), new Complex32(4.3f, 1), new Complex32(-7.7f, 1)})},
                    {"Tall3x2", new DenseMatrix(3, 2, new[] {new Complex32(-1.1f, 1), Complex32.Zero, new Complex32(-4.4f, 1), new Complex32(-2.2f, 1), new Complex32(1.1f, 1), new Complex32(5.5f, 1)})},
                    {"Wide2x3", new DenseMatrix(2, 3, new[] {new Complex32(-1.1f, 1), Complex32.Zero, new Complex32(-2.2f, 1), new Complex32(1.1f, 1), new Complex32(-3.3f, 1), new Complex32(2.2f, 1)})}
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
            var data = new[] {new Complex32(1.0f, 1), new Complex32(1.0f, 1), new Complex32(1.0f, 1), new Complex32(1.0f, 1), new Complex32(1.0f, 1), new Complex32(1.0f, 1), new Complex32(2.0f, 1), new Complex32(2.0f, 1), new Complex32(2.0f, 1)};
            var matrix = new DenseMatrix(3, 3, data);
            matrix[0, 0] = new Complex32(10.0f, 1);
            Assert.AreEqual(new Complex32(10.0f, 1), data[0]);
        }

        /// <summary>
        /// Matrix from two-dimensional array is a copy.
        /// </summary>
        [Test]
        public void MatrixFrom2DArrayIsCopy()
        {
            var matrix = DenseMatrix.OfArray(TestData2D["Singular3x3"]);
            matrix[0, 0] = 10.0f;
            Assert.AreEqual(new Complex32(1.0f, 1), TestData2D["Singular3x3"][0, 0]);
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
            var matrix = DenseMatrix.Create(10, 10, (i, j) => new Complex32(10.0f, 1));
            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    Assert.AreEqual(matrix[i, j], new Complex32(10.0f, 1));
                }
            }
        }

        /// <summary>
        /// Can create an identity matrix.
        /// </summary>
        [Test]
        public void CanCreateIdentity()
        {
            var matrix = DenseMatrix.Identity(5);
            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    Assert.AreEqual(i == j ? Complex32.One : Complex32.Zero, matrix[i, j]);
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
            Assert.Throws<ArgumentOutOfRangeException>(() => DenseMatrix.Identity(order));
        }
    }
}
