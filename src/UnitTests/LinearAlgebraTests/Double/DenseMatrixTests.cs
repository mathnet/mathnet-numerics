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

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Double
{
	using System.Collections.Generic;
	using MbUnit.Framework;
    using LinearAlgebra.Double;

    public class DenseMatrixTests : MatrixTests
    {
        protected override Matrix CreateMatrix(int rows, int columns)
        {
            return new DenseMatrix(rows, columns);
        }

        protected override Matrix CreateMatrix(double[,] data)
        {
            return new DenseMatrix(data);
        }

        protected override Vector CreateVector(int size)
        {
            return new DenseVector(size);
        }

        protected override Vector CreateVector(double[] data)
        {
            return new DenseVector(data);
        }

        [Test]
        public void CanCreateMatrixFrom1DArray()
        {
            Dictionary<string, Matrix> testData = new Dictionary<string, Matrix>();
            testData.Add("Singular3x3", new DenseMatrix(3, 3, new double[] { 1, 1, 1, 1, 1, 1, 2, 2, 2 }));
            testData.Add("Square3x3", new DenseMatrix(3, 3, new double[] { -1.1, 0.0, -4.4, -2.2, 1.1, 5.5, -3.3, 2.2, 6.6 }));
            testData.Add("Square4x4", new DenseMatrix(4, 4, new double[] { -1.1, 0.0, 1.0, -4.4, -2.2, 1.1, 2.1, 5.5, -3.3, 2.2, 6.2, 6.6, -4.4, 3.3, 4.3, -7.7 }));
            testData.Add("Tall3x2", new DenseMatrix(3, 2, new double[] { -1.1, 0.0, -4.4, -2.2, 1.1, 5.5 }));
            testData.Add("Wide2x3", new DenseMatrix(2, 3, new double[] { -1.1, 0.0, -2.2, 1.1, -3.3, 2.2 }));

            foreach (var name in testData.Keys)
            {
                Assert.AreEqual(testMatrices[name], testData[name]);
            }
        }

        [Test]
        public void MatrixFrom1DArrayIsReference()
        {
            var data = new double[] { 1, 1, 1, 1, 1, 1, 2, 2, 2 };
            var matrix = new DenseMatrix(3, 3, data);
            matrix[0, 0] = 10.0;
            Assert.AreEqual(10.0, data[0]);
        }

        [Test]
        public void MatrixFrom2DArrayIsCopy()
        {
            var matrix = new DenseMatrix(testData2D["Singular3x3"]);
            matrix[0, 0] = 10.0;
            Assert.AreEqual(1.0, testData2D["Singular3x3"][0, 0]);
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
            var matrix = new DenseMatrix(testData2D[name]);
            for (var i = 0; i < testData2D[name].GetLength(0); i++)
            {
                for (var j = 0; j < testData2D[name].GetLength(1); j++)
                {
                    Assert.AreEqual(testData2D[name][i, j], matrix[i, j]);
                }
            }
        }

        [Test]
        public void CanCreateMatrixWithUniformValues()
        {
            var matrix = new DenseMatrix(10, 10, 10.0);
            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    Assert.AreEqual(matrix[i, j], 10.0);
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
                        Assert.AreEqual(1.0, matrix[i, j]);
                    }
                    else
                    {
                        Assert.AreEqual(0.0, matrix[i, j]);
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
            var matrix = DenseMatrix.Identity(order);
        }
    }
}
