// <copyright file="DiagonalMatrixTests.cs" company="Math.NET">
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

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Single
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using LinearAlgebra.Generic;
    using MbUnit.Framework;
    using LinearAlgebra.Single;

    public class DiagonalMatrixTests : MatrixTests
    {
        [SetUp]
        public override void SetupMatrices()
        {
            TestData2D = new Dictionary<string, float[,]>
                         {
                             { "Singular3x3", new [,] { { 1.0f, 0.0f, 0.0f }, { 0.0f, 0.0f, 0.0f }, { 0.0f, 0.0f, 3.0f } } },
                             { "Square3x3", new [,] { { -1.1f, 0.0f, 0.0f }, { 0.0f, 1.1f, 0.0f }, { 0.0f, 0.0f, 6.6f } } },
                             { "Square4x4", new [,] { { -1.1f, 0.0f, 0.0f, 0.0f }, { 0.0f, 1.1f, 0.0f, 0.0f }, { 0.0f, 0.0f, 6.2f, 0.0f}, { 0.0f, 0.0f, 0.0f, -7.7f} } },
                             { "Singular4x4", new [,] { { -1.1f, 0.0f, 0.0f, 0.0f }, { 0.0f, -2.2f, 0.0f, 0.0f }, { 0.0f, 0.0f, 0.0f, 0.0f}, { 0.0f, 0.0f, 0.0f, -4.4f } } },
                             { "Tall3x2", new [,] { { -1.1f, 0.0f }, { 0.0f, 1.1f }, { 0.0f, 0.0f } } },
                             { "Wide2x3", new [,] { { -1.1f, 0.0f, 0.0f }, { 0.0f, 1.1f, 0.0f } } }
                         };

            TestMatrices = new Dictionary<string, Matrix<float>>();
            foreach (var name in TestData2D.Keys)
            {
                TestMatrices.Add(name, CreateMatrix(TestData2D[name]));
            }
        }

        protected override Matrix<float> CreateMatrix(int rows, int columns)
        {
            return new DiagonalMatrix(rows, columns);
        }

        protected override Matrix<float> CreateMatrix(float[,] data)
        {
            return new DiagonalMatrix(data);
        }

        protected override Vector<float> CreateVector(int size)
        {
            return new DenseVector(size);
        }

        protected override Vector<float> CreateVector(float[] data)
        {
            return new DenseVector(data);
        }

        [Test]
        public void CanCreateMatrixFromDiagonalArray()
        {
            var testData = new Dictionary<string, Matrix<float>>
                           {
                               { "Singular3x3", new DiagonalMatrix(3, 3, new[] { 1.0f, 0.0f, 3.0f}) },
                               { "Square3x3", new DiagonalMatrix(4, 4, new[] { -1.1f, 1.1f, 6.6f }) },
                               { "Square4x4", new DiagonalMatrix(4, 4, new[] { -1.1f, 1.1f, 6.2f, -7.7f }) },
                               { "Tall3x2", new DiagonalMatrix(3, 2, new[] { -1.1f, 1.1f }) },
                               { "Wide2x3", new DiagonalMatrix(2, 3, new[] { -1.1f, 1.1f }) },
                           };

            foreach (var name in testData.Keys)
            {
                Assert.AreEqual(TestMatrices[name], testData[name]);
            }
        }

        [Test]
        public void MatrixFrom1DArrayIsReference()
        {
            var data = new float[] { 1, 2, 3, 4, 5};
            var matrix = new DiagonalMatrix(5, 5, data);
            matrix[0, 0] = 10.0f;
            Assert.AreEqual(10.0, data[0]);
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
            var matrix = new DiagonalMatrix(TestData2D[name]);
            for (var i = 0; i < TestData2D[name].GetLength(0); i++)
            {
                for (var j = 0; j < TestData2D[name].GetLength(1); j++)
                {
                    Assert.AreEqual(TestData2D[name][i, j], matrix[i, j]);
                }
            }
        }

        [Test]
        public void CanCreateMatrixWithUniformValues()
        {
            var matrix = new DiagonalMatrix(10, 10, 10.0f);
            for (var i = 0; i < matrix.RowCount; i++)
            {
                Assert.AreEqual(matrix[i, i], 10.0);
            }
        }

        [Test]
        public void CanCreateIdentity()
        {
            var matrix = DiagonalMatrix.Identity(5);
            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    Assert.AreEqual(i == j ? 1.0 : 0.0, matrix[i, j]);
                }
            }
        }

        [Test]
        [Row(0)]
        [Row(-1)]
        [ExpectedArgumentException]
        public void IdentityFailsWithZeroOrNegativeOrder(int order)
        {
            DiagonalMatrix.Identity(order);
        }

        [Test]
        public override void CanDiagonallyStackMatricesWithPassingResult()
        {
            var top = TestMatrices["Tall3x2"];
            var bottom = TestMatrices["Wide2x3"];
            var result = new SparseMatrix(top.RowCount + bottom.RowCount, top.ColumnCount + bottom.ColumnCount);
            top.DiagonalStack(bottom, result);
            Assert.AreEqual(top.RowCount + bottom.RowCount, result.RowCount);
            Assert.AreEqual(top.ColumnCount + bottom.ColumnCount, result.ColumnCount);

            for (var i = 0; i < result.RowCount; i++)
            {
                for (var j = 0; j < result.ColumnCount; j++)
                {
                    if (i < top.RowCount && j < top.ColumnCount)
                    {
                        Assert.AreEqual(top[i, j], result[i, j]);
                    }
                    else if (i >= top.RowCount && j >= top.ColumnCount)
                    {
                        Assert.AreEqual(bottom[i - top.RowCount, j - top.ColumnCount], result[i, j]);
                    }
                    else
                    {
                        Assert.AreEqual(0, result[i, j]);
                    }
                }
            }
        }

        public override void CanMultiplyMatrixWithMatrixIntoResult(string nameA, string nameB)
        {
            var matrixA = TestMatrices[nameA];
            var matrixB = TestMatrices[nameB];
            var matrixC = new SparseMatrix(matrixA.RowCount, matrixB.ColumnCount);
            matrixA.Multiply(matrixB, matrixC);

            Assert.AreEqual(matrixC.RowCount, matrixA.RowCount);
            Assert.AreEqual(matrixC.ColumnCount, matrixB.ColumnCount);

            for (var i = 0; i < matrixC.RowCount; i++)
            {
                for (var j = 0; j < matrixC.ColumnCount; j++)
                {
                    AssertHelpers.AlmostEqual(matrixA.Row(i) * matrixB.Column(j), matrixC[i, j], 15);
                }
            }
        }

        [Test]
        [Row("Singular3x3")]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CanPermuteMatrixRowsThrowException(string name)
        {
            var matrix = CreateMatrix(TestData2D[name]);
            var matrixp = CreateMatrix(TestData2D[name]);

            var permutation = new Permutation(new[] { 2, 0, 1 });
            matrixp.PermuteRows(permutation);

            Assert.AreNotSame(matrix, matrixp);
            Assert.AreEqual(matrix.RowCount, matrixp.RowCount);
            Assert.AreEqual(matrix.ColumnCount, matrixp.ColumnCount);
            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    Assert.AreEqual(matrix[i, j], matrixp[permutation[i], j]);
                }
            }
        }

        [Test]
        [Row("Singular3x3")]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CanPermuteMatrixColumnsThrowException(string name)
        {
            var matrix = CreateMatrix(TestData2D[name]);
            var matrixp = CreateMatrix(TestData2D[name]);

            var permutation = new Permutation(new[] { 2, 0, 1 });
            matrixp.PermuteColumns(permutation);

            Assert.AreNotSame(matrix, matrixp);
            Assert.AreEqual(matrix.RowCount, matrixp.RowCount);
            Assert.AreEqual(matrix.ColumnCount, matrixp.ColumnCount);
            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    Assert.AreEqual(matrix[i, j], matrixp[i, permutation[j]]);
                }
            }
        }

        public override void CanPermuteMatrixRows(string name)
        {
        }

        public override void CanPermuteMatrixColumns(string name)
        {
        }

        public override void PointwiseDivideResult()
        {
            foreach (var data in TestMatrices.Values)
            {
                var other = data.Clone();
                var result = data.Clone();
                data.PointwiseDivide(other, result);
                var min = Math.Min(data.RowCount, data.ColumnCount);
                for (var i = 0; i < min; i++)
                {
                    Assert.AreEqual(data[i, i] / other[i, i], result[i, i]);
                }

                result = data.PointwiseDivide(other);
                for (var i = 0; i < min; i++)
                {
                    Assert.AreEqual(data[i, i] / other[i, i], result[i, i]);
                }
            }
        }

        public override void SetColumnWithArray(string name, float[] column)
        {
            try
            {
                // Pass all invoke to base
                base.SetColumnWithArray(name, column);
            }
            catch(AggregateException ex)
            {
                // Supress only IndexOutOfRangeException exceptions due to Diagonal matrix nature
                if (ex.InnerExceptions.Any(innerException => !(innerException is IndexOutOfRangeException)))
                {
                    throw;
                }
            }
        }

        public override void SetColumnWithVector(string name, float[] column)
        {
            try
            {
                // Pass all invoke to base
                base.SetColumnWithVector(name, column);
            }
            catch (AggregateException ex)
            {
                // Supress only IndexOutOfRangeException exceptions due to Diagonal matrix nature
                if (ex.InnerExceptions.Any(innerException => !(innerException is IndexOutOfRangeException)))
                {
                    throw;
                }
            }
        }

        public override void SetRowWithArray(string name, float[] row)
        {
            try
            {
                // Pass all invoke to base
                base.SetRowWithArray(name, row);
            }
            catch (AggregateException ex)
            {
                // Supress only IndexOutOfRangeException exceptions due to Diagonal matrix nature
                if (ex.InnerExceptions.Any(innerException => !(innerException is IndexOutOfRangeException)))
                {
                    throw;
                }
            }
        }

        public override void SetRowWithVector(string name, float[] row)
        {
            try
            {
                // Pass all invoke to base
                base.SetRowWithVector(name, row);
            }
            catch (AggregateException ex)
            {
                // Supress only IndexOutOfRangeException exceptions due to Diagonal matrix nature
                if (ex.InnerExceptions.Any(innerException => !(innerException is IndexOutOfRangeException)))
                {
                    throw;
                }
            }
        }

        public override void SetSubMatrix(int rowStart, int rowLength, int colStart, int colLength)
        {
            try
            {
                // Pass all invoke to base
                base.SetSubMatrix(rowStart, rowLength, colStart, colLength);
            }
            catch (AggregateException ex)
            {
                // Supress only IndexOutOfRangeException exceptions due to Diagonal matrix nature
                if (ex.InnerExceptions.Any(innerException => !(innerException is IndexOutOfRangeException)))
                {
                    throw;
                }
            }
        }

        public override void FrobeniusNorm()
        {
            var matrix = TestMatrices["Square3x3"];
            var denseMatrix = new DenseMatrix(TestData2D["Square3x3"]);
            AssertHelpers.AlmostEqual((float)denseMatrix.FrobeniusNorm(), (float)matrix.FrobeniusNorm(), 7);

            matrix = TestMatrices["Wide2x3"];
            denseMatrix = new DenseMatrix(TestData2D["Wide2x3"]);
            AssertHelpers.AlmostEqual((float)denseMatrix.FrobeniusNorm(), (float)matrix.FrobeniusNorm(), 7);

            matrix = TestMatrices["Tall3x2"];
            denseMatrix = new DenseMatrix(TestData2D["Tall3x2"]);
            AssertHelpers.AlmostEqual((float)denseMatrix.FrobeniusNorm(), (float)matrix.FrobeniusNorm(), 7);
        }

        public override void InfinityNorm()
        {
            var matrix = TestMatrices["Square3x3"];
            var denseMatrix = new DenseMatrix(TestData2D["Square3x3"]);
            AssertHelpers.AlmostEqual((float)denseMatrix.InfinityNorm(), (float)matrix.InfinityNorm(), 7);

            matrix = TestMatrices["Wide2x3"];
            denseMatrix = new DenseMatrix(TestData2D["Wide2x3"]);
            AssertHelpers.AlmostEqual((float)denseMatrix.InfinityNorm(), (float)matrix.InfinityNorm(), 7);

            matrix = TestMatrices["Tall3x2"];
            denseMatrix = new DenseMatrix(TestData2D["Tall3x2"]);
            AssertHelpers.AlmostEqual((float)denseMatrix.InfinityNorm(), (float)matrix.InfinityNorm(), 7);
        }

        public override void L1Norm()
        {
            var matrix = TestMatrices["Square3x3"];
            var denseMatrix = new DenseMatrix(TestData2D["Square3x3"]);
            AssertHelpers.AlmostEqual((float)denseMatrix.L1Norm(), (float)matrix.L1Norm(), 7);

            matrix = TestMatrices["Wide2x3"];
            denseMatrix = new DenseMatrix(TestData2D["Wide2x3"]);
            AssertHelpers.AlmostEqual((float)denseMatrix.L1Norm(), (float)matrix.L1Norm(), 7);

            matrix = TestMatrices["Tall3x2"];
            denseMatrix = new DenseMatrix(TestData2D["Tall3x2"]);
            AssertHelpers.AlmostEqual((float)denseMatrix.L1Norm(), (float)matrix.L1Norm(), 7);
        }

        public override void L2Norm()
        {
            var matrix = TestMatrices["Square3x3"];
            var denseMatrix = new DenseMatrix(TestData2D["Square3x3"]);
            AssertHelpers.AlmostEqual((float)denseMatrix.L2Norm(), (float)matrix.L2Norm(), 7);

            matrix = TestMatrices["Wide2x3"];
            denseMatrix = new DenseMatrix(TestData2D["Wide2x3"]);
            AssertHelpers.AlmostEqual((float)denseMatrix.L2Norm(), (float)matrix.L2Norm(), 7);

            matrix = TestMatrices["Tall3x2"];
            denseMatrix = new DenseMatrix(TestData2D["Tall3x2"]);
            AssertHelpers.AlmostEqual((float)denseMatrix.L2Norm(), (float)matrix.L2Norm(), 7);
        }

        [Test]
        [MultipleAsserts]
        public void Determinant()
        {
            var matrix = TestMatrices["Square3x3"];
            var denseMatrix = new DenseMatrix(TestData2D["Square3x3"]);
            AssertHelpers.AlmostEqual(denseMatrix.Determinant(), matrix.Determinant(), 7);

            matrix = TestMatrices["Square4x4"];
            denseMatrix = new DenseMatrix(TestData2D["Square4x4"]);
            AssertHelpers.AlmostEqual(denseMatrix.Determinant(), matrix.Determinant(), 7);
        }

        [Test]
        [ExpectedArgumentException]
        public void DeterminantNotSquareMatrixThrowException()
        {
            var matrix = TestMatrices["Tall3x2"];
            matrix.Determinant();
        }
    }
}
