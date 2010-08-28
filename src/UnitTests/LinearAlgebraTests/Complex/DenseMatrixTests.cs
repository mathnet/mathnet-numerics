// <copyright file="DenseMatrixTests.cs" company="Math.NET">
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

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Complex
{
	using System.Collections.Generic;
    using System.Numerics;
	using LinearAlgebra.Generic;
	using MbUnit.Framework;
    using LinearAlgebra.Complex;

    public class DenseMatrixTests : MatrixTests
    {
        protected override Matrix<Complex> CreateMatrix(int rows, int columns)
        {
            return new DenseMatrix(rows, columns);
        }

        protected override Matrix<Complex> CreateMatrix(Complex[,] data)
        {
            return new DenseMatrix(data);
        }

        protected override Vector<Complex> CreateVector(int size)
        {
            return new DenseVector(size);
        }

        protected override Vector<Complex> CreateVector(Complex[] data)
        {
            return new DenseVector(data);
        }

        [Test]
        public void CanCreateMatrixFrom1DArray()
        {
            Dictionary<string, Matrix<Complex>> testData = new Dictionary<string, Matrix<Complex>>
                                                  {
                                                      { "Singular3x3",  new DenseMatrix(3, 3, new[] { new Complex(1.0, 1), new Complex(1.0, 1), new Complex(1.0, 1), new Complex(1.0, 1), new Complex(1.0, 1), new Complex(1.0, 1), new Complex(2.0, 1), new Complex(2.0, 1), new Complex(2.0, 1) }) },
                                                      { "Square3x3",    new DenseMatrix(3, 3, new[] { new Complex(-1.1, 1), Complex.Zero, new Complex(-4.4, 1), new Complex(-2.2, 1), new Complex(1.1, 1), new Complex(5.5, 1), new Complex(-3.3, 1), new Complex(2.2, 1), new Complex(6.6, 1) }) },
                                                      { "Square4x4",    new DenseMatrix(4, 4, new[] { new Complex(-1.1, 1), Complex.Zero, new Complex(1.0, 1), new Complex(-4.4, 1), new Complex(-2.2, 1), new Complex(1.1, 1), new Complex(2.1, 1), new Complex(5.5, 1), new Complex(-3.3, 1), new Complex(2.2, 1), new Complex(6.2, 1), new Complex(6.6, 1), new Complex(-4.4, 1), new Complex(3.3, 1), new Complex(4.3, 1), new Complex(-7.7, 1) }) },
                                                      { "Tall3x2",      new DenseMatrix(3, 2, new[] { new Complex(-1.1, 1), Complex.Zero, new Complex(-4.4, 1), new Complex(-2.2, 1), new Complex(1.1, 1), new Complex(5.5, 1) }) },
                                                      { "Wide2x3",      new DenseMatrix(2, 3, new[] { new Complex(-1.1, 1), Complex.Zero, new Complex(-2.2, 1), new Complex(1.1, 1), new Complex(-3.3, 1), new Complex(2.2, 1) }) }
                                                  };

            foreach (var name in testData.Keys)
            {
                Assert.AreEqual(TestMatrices[name], testData[name]);
            }
        }

        [Test]
        public void MatrixFrom1DArrayIsReference()
        {
            var data = new[] { new Complex(1.0, 1), new Complex(1.0, 1), new Complex(1.0, 1), new Complex(1.0, 1), new Complex(1.0, 1), new Complex(1.0, 1), new Complex(2.0, 1), new Complex(2.0, 1), new Complex(2.0, 1) };
            var matrix = new DenseMatrix(3, 3, data);
            matrix[0, 0] = 10.0;
            AssertHelpers.AreEqual(10.0, data[0]);
        }

        [Test]
        public void MatrixFrom2DArrayIsCopy()
        {
            var matrix = new DenseMatrix(TestData2D["Singular3x3"]);
            matrix[0, 0] = 10.0;
            AssertHelpers.AreEqual(new Complex(1.0, 1), TestData2D["Singular3x3"][0, 0]);
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
            var matrix = new DenseMatrix(TestData2D[name]);
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
            var matrix = new DenseMatrix(10, 10, new Complex(10.0, 1));
            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    AssertHelpers.AreEqual(matrix[i, j], new Complex(10.0, 1));
                }
            }
        }

        [Test]
        public void CanCreateIdentity()
        {
            var matrix = DenseMatrix.Identity(5);
            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    if (i == j)
                    {
                        AssertHelpers.AreEqual(Complex.One, matrix[i, j]);
                    }
                    else
                    {
                        AssertHelpers.AreEqual(Complex.Zero, matrix[i, j]);
                    }
                }
            }
        }

        [Test]
        [Row(0)]
        [Row(-1)]
        [ExpectedArgumentException]
        public void IdentityFailsWithZeroOrNegativeOrder(int order)
        {
            DenseMatrix.Identity(order);
        }
    }
}
