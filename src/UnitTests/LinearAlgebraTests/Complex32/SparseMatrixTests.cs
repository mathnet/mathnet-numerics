// <copyright file="SparseMatrixTests.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
//
// Copyright (c) 2009-2010 Math.NET
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
	using System.Collections.Generic;
    using Numerics;
	using LinearAlgebra.Generic;
	using MbUnit.Framework;
    using LinearAlgebra.Complex32;

    public class SparseMatrixTests : MatrixTests
    {
        protected override Matrix<Complex32> CreateMatrix(int rows, int columns)
        {
            return new SparseMatrix(rows, columns);
        }

        protected override Matrix<Complex32> CreateMatrix(Complex32[,] data)
        {
            return new SparseMatrix(data);
        }

        protected override Vector<Complex32> CreateVector(int size)
        {
            return new SparseVector(size);
        }

        protected override Vector<Complex32> CreateVector(Complex32[] data)
        {
            return new SparseVector(data);
        }

        [Test]
        public void CanCreateMatrixFrom1DArray()
        {
            var testData = new Dictionary<string, Matrix<Complex32>>
                                                  {
                                                      { "Singular3x3",  new SparseMatrix(3, 3, new[] { new Complex32(1.0f, 1), new Complex32(1.0f, 1), new Complex32(1.0f, 1), new Complex32(1.0f, 1), new Complex32(1.0f, 1), new Complex32(1.0f, 1), new Complex32(2.0f, 1), new Complex32(2.0f, 1), new Complex32(2.0f, 1) }) },
                                                      { "Square3x3",    new SparseMatrix(3, 3, new[] { new Complex32(-1.1f, 1), Complex32.Zero, new Complex32(-4.4f, 1), new Complex32(-2.2f, 1), new Complex32(1.1f, 1), new Complex32(5.5f, 1), new Complex32(-3.3f, 1), new Complex32(2.2f, 1), new Complex32(6.6f, 1) }) },
                                                      { "Square4x4",    new SparseMatrix(4, 4, new[] { new Complex32(-1.1f, 1), Complex32.Zero, new Complex32(1.0f, 1), new Complex32(-4.4f, 1), new Complex32(-2.2f, 1), new Complex32(1.1f, 1), new Complex32(2.1f, 1), new Complex32(5.5f, 1), new Complex32(-3.3f, 1), new Complex32(2.2f, 1), new Complex32(6.2f, 1), new Complex32(6.6f, 1), new Complex32(-4.4f, 1), new Complex32(3.3f, 1), new Complex32(4.3f, 1), new Complex32(-7.7f, 1) }) },
                                                      { "Tall3x2",      new SparseMatrix(3, 2, new[] { new Complex32(-1.1f, 1), Complex32.Zero, new Complex32(-4.4f, 1), new Complex32(-2.2f, 1), new Complex32(1.1f, 1), new Complex32(5.5f, 1) }) },
                                                      { "Wide2x3",      new SparseMatrix(2, 3, new[] { new Complex32(-1.1f, 1), Complex32.Zero, new Complex32(-2.2f, 1), new Complex32(1.1f, 1), new Complex32(-3.3f, 1), new Complex32(2.2f, 1) }) }
                                                  };

            foreach (var name in testData.Keys)
            {
                Assert.AreEqual(TestMatrices[name], testData[name]);
            }
        }

        [Test]
        public void MatrixFrom2DArrayIsCopy()
        {
            var matrix = new SparseMatrix(TestData2D["Singular3x3"]);
            matrix[0, 0] = 10.0f;
            AssertHelpers.AreEqual(new Complex32(1.0f, 1), TestData2D["Singular3x3"][0, 0]);
        }

        [Test]
        [Row("Singular3x3")]
        [Row("Singular3x3")]
        [Row("Square3x3")]
        [Row("Square4x4")]
        [Row("Tall3x2")]
        [Row("Wide2x3")]
        public void CanCreateMatrixFrom2DArray(string name)
        {
            var matrix = new SparseMatrix(TestData2D[name]);
            for (var i = 0; i < TestData2D[name].GetLength(0); i++)
            {
                for (var j = 0; j < TestData2D[name].GetLength(1); j++)
                {
                    AssertHelpers.AreEqual(TestData2D[name][i, j], matrix[i, j]);
                }
            }
        }

        [Test]
        public void CanCreateMatrixWithUniformValues()
        {
            var matrix = new SparseMatrix(10, 10, new Complex32(10.0f, 1));
            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    AssertHelpers.AreEqual(matrix[i, j], new Complex32(10.0f, 1));
                }
            }
        }

        [Test]
        public void CanCreateIdentity()
        {
            var matrix = SparseMatrix.Identity(5);
            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    AssertHelpers.AreEqual(i == j ? Complex32.One : Complex32.Zero, matrix[i, j]);
                }
            }
        }

        [Test]
        [Row(0)]
        [Row(-1)]
        [ExpectedArgumentException]
        public void IdentityFailsWithZeroOrNegativeOrder(int order)
        {
            SparseMatrix.Identity(order);
        }

        [Test]
        public void CanCreateHeavySparseMatrix()
        {
            var matrix = new SparseMatrix(500, 1000);
            var nonzero = 0;
            var rnd = new System.Random();

            for (int i = 0; i < matrix.RowCount; i++)
            {
                for (int j = 0; j < matrix.ColumnCount; j++ )
                {
                    var value = rnd.Next(10) * rnd.Next(10) * rnd.Next(10) * rnd.Next(10) * rnd.Next(10);
                    if (value != 0)
                    {
                        nonzero++;
                    }
                    matrix[i, j] = value;
                }
            }

            Assert.AreEqual(matrix.NonZerosCount, nonzero);
        }
    }
}
