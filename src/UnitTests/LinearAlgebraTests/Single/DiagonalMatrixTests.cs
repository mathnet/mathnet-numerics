// <copyright file="DiagonalMatrixTests.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
// Copyright (c) 2009-2010 Math.NET
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
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
    using LinearAlgebra.Single;
    using NUnit.Framework;

#if PORTABLE
    using Threading;
#endif

    /// <summary>
    /// Diagonal matrix tests.
    /// </summary>
    public class DiagonalMatrixTests : MatrixTests
    {
        /// <summary>
        /// Setup test matrices.
        /// </summary>
        [SetUp]
        public override void SetupMatrices()
        {
            TestData2D = new Dictionary<string, float[,]>
                         {
                             { "Singular3x3", new[,] { { 1.0f, 0.0f, 0.0f }, { 0.0f, 0.0f, 0.0f }, { 0.0f, 0.0f, 3.0f } } },
                             { "Square3x3", new[,] { { -1.1f, 0.0f, 0.0f }, { 0.0f, 1.1f, 0.0f }, { 0.0f, 0.0f, 6.6f } } },
                             { "Square4x4", new[,] { { -1.1f, 0.0f, 0.0f, 0.0f }, { 0.0f, 1.1f, 0.0f, 0.0f }, { 0.0f, 0.0f, 6.2f, 0.0f }, { 0.0f, 0.0f, 0.0f, -7.7f } } },
                             { "Singular4x4", new[,] { { -1.1f, 0.0f, 0.0f, 0.0f }, { 0.0f, -2.2f, 0.0f, 0.0f }, { 0.0f, 0.0f, 0.0f, 0.0f }, { 0.0f, 0.0f, 0.0f, -4.4f } } },
                             { "Tall3x2", new[,] { { -1.1f, 0.0f }, { 0.0f, 1.1f }, { 0.0f, 0.0f } } },
                             { "Wide2x3", new[,] { { -1.1f, 0.0f, 0.0f }, { 0.0f, 1.1f, 0.0f } } }
                         };

            TestMatrices = new Dictionary<string, Matrix>();
            foreach (var name in TestData2D.Keys)
            {
                TestMatrices.Add(name, CreateMatrix(TestData2D[name]));
            }
        }

        /// <summary>
        /// Creates a matrix for the given number of rows and columns.
        /// </summary>
        /// <param name="rows">The number of rows.</param>
        /// <param name="columns">The number of columns.</param>
        /// <returns>A matrix with the given dimensions.</returns>
        protected override Matrix CreateMatrix(int rows, int columns)
        {
            return new DiagonalMatrix(rows, columns);
        }

        /// <summary>
        /// Creates a matrix from a 2D array.
        /// </summary>
        /// <param name="data">The 2D array to create this matrix from.</param>
        /// <returns>A matrix with the given values.</returns>
        protected override Matrix CreateMatrix(float[,] data)
        {
            return new DiagonalMatrix(data);
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
        protected override Vector CreateVector(float[] data)
        {
            return new DenseVector(data);
        }

        /// <summary>
        /// Can create a matrix from a diagonal array.
        /// </summary>
        [Test]
        public void CanCreateMatrixFromDiagonalArray()
        {
            var testData = new Dictionary<string, Matrix>
                           {
                               { "Singular3x3", new DiagonalMatrix(3, 3, new[] { 1.0f, 0.0f, 3.0f }) },
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

        /// <summary>
        /// Matrix from array is a reference.
        /// </summary>
        [Test]
        public void MatrixFrom1DArrayIsReference()
        {
            var data = new float[] { 1, 2, 3, 4, 5 };
            var matrix = new DiagonalMatrix(5, 5, data);
            matrix[0, 0] = 10.0f;
            Assert.AreEqual(10.0f, data[0]);
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
            var matrix = new DiagonalMatrix(TestData2D[name]);
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
            var matrix = new DiagonalMatrix(10, 10, 10.0f);
            for (var i = 0; i < matrix.RowCount; i++)
            {
                Assert.AreEqual(matrix[i, i], 10.0f);
            }
        }

        /// <summary>
        /// Can create an identity matrix.
        /// </summary>
        [Test]
        public void CanCreateIdentity()
        {
            var matrix = DiagonalMatrix.Identity(5);
            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    Assert.AreEqual(i == j ? 1.0f : 0.0f, matrix[i, j]);
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
            Assert.Throws<ArgumentOutOfRangeException>(() => DiagonalMatrix.Identity(order));
        }

        /// <summary>
        /// Can diagonally stack matrices into a result matrix.
        /// </summary>
        public override void CanDiagonallyStackMatricesIntoResult()
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

        /// <summary>
        /// Can multiply a matrix with matrix.
        /// </summary>
        /// <param name="nameA">Matrix A name.</param>
        /// <param name="nameB">Matrix B name.</param>
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

        /// <summary>
        /// Permute matrix rows throws <c>InvalidOperationException</c>.
        /// </summary>
        [Test]
        public void PermuteMatrixRowsThrowsInvalidOperationException()
        {
            var matrixp = CreateMatrix(TestData2D["Singular3x3"]);
            var permutation = new Permutation(new[] { 2, 0, 1 });
            Assert.Throws<InvalidOperationException>(() => matrixp.PermuteRows(permutation));
        }

        /// <summary>
        /// Permute matrix columns throws <c>InvalidOperationException</c>.
        /// </summary>
        [Test]
        public void PermuteMatrixColumnsThrowsInvalidOperationException()
        {
            var matrixp = CreateMatrix(TestData2D["Singular3x3"]);
            var permutation = new Permutation(new[] { 2, 0, 1 });
            Assert.Throws<InvalidOperationException>(() => matrixp.PermuteColumns(permutation));
        }

        /// <summary>
        /// Can permute matrix rows.
        /// </summary>
        /// <param name="name">Matrix name.</param>
        public override void CanPermuteMatrixRows(string name)
        {
        }

        /// <summary>
        /// Can permute matrix columns.
        /// </summary>
        /// <param name="name">Matrix name.</param>
        public override void CanPermuteMatrixColumns(string name)
        {
        }

        /// <summary>
        /// Can pointwise divide matrices into a result matrix.
        /// </summary>
        public override void CanPointwiseDivideIntoResult()
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

        /// <summary>
        /// Can set a column with an array.
        /// </summary>
        /// <param name="name">Matrix name.</param>
        /// <param name="column">Column array.</param>
        public override void CanSetColumnWithArray(string name, float[] column)
        {
            try
            {
                // Pass all invoke to base
                base.CanSetColumnWithArray(name, column);
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

        /// <summary>
        /// Can set a column with a vector.
        /// </summary>
        /// <param name="name">Matrix name.</param>
        /// <param name="column">Column values.</param>
        public override void CanSetColumnWithVector(string name, float[] column)
        {
            try
            {
                // Pass all invoke to base
                base.CanSetColumnWithVector(name, column);
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

        /// <summary>
        /// Can set a row with an array.
        /// </summary>
        /// <param name="name">Matrix name.</param>
        /// <param name="row">Row index.</param>
        public override void CanSetRowWithArray(string name, float[] row)
        {
            try
            {
                // Pass all invoke to base
                base.CanSetRowWithArray(name, row);
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

        /// <summary>
        /// Can set a row with a vector.
        /// </summary>
        /// <param name="name">Matrix name.</param>
        /// <param name="row">Row index.</param>
        public override void CanSetRowWithVector(string name, float[] row)
        {
            try
            {
                // Pass all invoke to base
                base.CanSetRowWithVector(name, row);
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

        /// <summary>
        /// Can set a submatrix.
        /// </summary>
        /// <param name="rowStart">The row to start copying to.</param>
        /// <param name="rowLength">The number of rows to copy.</param>
        /// <param name="colStart">The column to start copying to.</param>
        /// <param name="colLength">The number of columns to copy.</param>
        public override void CanSetSubMatrix(int rowStart, int rowLength, int colStart, int colLength)
        {
            try
            {
                // Pass all invoke to base
                base.CanSetSubMatrix(rowStart, rowLength, colStart, colLength);
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

        /// <summary>
        /// Can compute Frobenius norm.
        /// </summary>
        public override void CanComputeFrobeniusNorm()
        {
            var matrix = TestMatrices["Square3x3"];
            var denseMatrix = new DenseMatrix(TestData2D["Square3x3"]);
            AssertHelpers.AlmostEqual(denseMatrix.FrobeniusNorm(), matrix.FrobeniusNorm(), 7);

            matrix = TestMatrices["Wide2x3"];
            denseMatrix = new DenseMatrix(TestData2D["Wide2x3"]);
            AssertHelpers.AlmostEqual(denseMatrix.FrobeniusNorm(), matrix.FrobeniusNorm(), 7);

            matrix = TestMatrices["Tall3x2"];
            denseMatrix = new DenseMatrix(TestData2D["Tall3x2"]);
            AssertHelpers.AlmostEqual(denseMatrix.FrobeniusNorm(), matrix.FrobeniusNorm(), 7);
        }

        /// <summary>
        /// Can compute Infinity norm.
        /// </summary>
        public override void CanComputeInfinityNorm()
        {
            var matrix = TestMatrices["Square3x3"];
            var denseMatrix = new DenseMatrix(TestData2D["Square3x3"]);
            AssertHelpers.AlmostEqual(denseMatrix.InfinityNorm(), matrix.InfinityNorm(), 7);

            matrix = TestMatrices["Wide2x3"];
            denseMatrix = new DenseMatrix(TestData2D["Wide2x3"]);
            AssertHelpers.AlmostEqual(denseMatrix.InfinityNorm(), matrix.InfinityNorm(), 7);

            matrix = TestMatrices["Tall3x2"];
            denseMatrix = new DenseMatrix(TestData2D["Tall3x2"]);
            AssertHelpers.AlmostEqual(denseMatrix.InfinityNorm(), matrix.InfinityNorm(), 7);
        }

        /// <summary>
        /// Can compute L1 norm.
        /// </summary>
        public override void CanComputeL1Norm()
        {
            var matrix = TestMatrices["Square3x3"];
            var denseMatrix = new DenseMatrix(TestData2D["Square3x3"]);
            AssertHelpers.AlmostEqual(denseMatrix.L1Norm(), matrix.L1Norm(), 7);

            matrix = TestMatrices["Wide2x3"];
            denseMatrix = new DenseMatrix(TestData2D["Wide2x3"]);
            AssertHelpers.AlmostEqual(denseMatrix.L1Norm(), matrix.L1Norm(), 7);

            matrix = TestMatrices["Tall3x2"];
            denseMatrix = new DenseMatrix(TestData2D["Tall3x2"]);
            AssertHelpers.AlmostEqual(denseMatrix.L1Norm(), matrix.L1Norm(), 7);
        }

        /// <summary>
        /// Can compute L2 norm.
        /// </summary>
        public override void CanComputeL2Norm()
        {
            var matrix = TestMatrices["Square3x3"];
            var denseMatrix = new DenseMatrix(TestData2D["Square3x3"]);
            AssertHelpers.AlmostEqual(denseMatrix.L2Norm(), matrix.L2Norm(), 7);

            matrix = TestMatrices["Wide2x3"];
            denseMatrix = new DenseMatrix(TestData2D["Wide2x3"]);
            AssertHelpers.AlmostEqual(denseMatrix.L2Norm(), matrix.L2Norm(), 7);

            matrix = TestMatrices["Tall3x2"];
            denseMatrix = new DenseMatrix(TestData2D["Tall3x2"]);
            AssertHelpers.AlmostEqual(denseMatrix.L2Norm(), matrix.L2Norm(), 7);
        }

        /// <summary>
        /// Can compute determinant.
        /// </summary>
        [Test]
        public void CanComputeDeterminant()
        {
            var matrix = TestMatrices["Square3x3"];
            var denseMatrix = new DenseMatrix(TestData2D["Square3x3"]);
            AssertHelpers.AlmostEqual(denseMatrix.Determinant(), matrix.Determinant(), 7);

            matrix = TestMatrices["Square4x4"];
            denseMatrix = new DenseMatrix(TestData2D["Square4x4"]);
            AssertHelpers.AlmostEqual(denseMatrix.Determinant(), matrix.Determinant(), 7);
        }

        /// <summary>
        /// Determinant of  non-square matrix throws <c>ArgumentException</c>.
        /// </summary>
        [Test]
        public void DeterminantNotSquareMatrixThrowsArgumentException()
        {
            var matrix = TestMatrices["Tall3x2"];
            Assert.Throws<ArgumentException>(() => matrix.Determinant());
        }

        /// <summary>
        /// Test whether the index enumerator returns the correct values.
        /// </summary>
        [Test]
        public override void CanUseIndexedEnumerator()
        {
            var matrix = TestMatrices["Singular3x3"];
            using (var enumerator = matrix.IndexedEnumerator().GetEnumerator())
            {
                enumerator.MoveNext();
                var item = enumerator.Current;
                Assert.AreEqual(0, item.Item1);
                Assert.AreEqual(0, item.Item2);
                Assert.AreEqual(1.0f, item.Item3);

                enumerator.MoveNext();
                item = enumerator.Current;
                Assert.AreEqual(0, item.Item1);
                Assert.AreEqual(1, item.Item2);
                Assert.AreEqual(0.0f, item.Item3);

                enumerator.MoveNext();
                item = enumerator.Current;
                Assert.AreEqual(0, item.Item1);
                Assert.AreEqual(2, item.Item2);
                Assert.AreEqual(0.0f, item.Item3);

                enumerator.MoveNext();
                item = enumerator.Current;
                Assert.AreEqual(1, item.Item1);
                Assert.AreEqual(0, item.Item2);
                Assert.AreEqual(0.0f, item.Item3);

                enumerator.MoveNext();
                item = enumerator.Current;
                Assert.AreEqual(1, item.Item1);
                Assert.AreEqual(1, item.Item2);
                Assert.AreEqual(0.0f, item.Item3);

                enumerator.MoveNext();
                item = enumerator.Current;
                Assert.AreEqual(1, item.Item1);
                Assert.AreEqual(2, item.Item2);
                Assert.AreEqual(0.0f, item.Item3);

                enumerator.MoveNext();
                item = enumerator.Current;
                Assert.AreEqual(2, item.Item1);
                Assert.AreEqual(0, item.Item2);
                Assert.AreEqual(0.0f, item.Item3);

                enumerator.MoveNext();
                item = enumerator.Current;
                Assert.AreEqual(2, item.Item1);
                Assert.AreEqual(1, item.Item2);
                Assert.AreEqual(0.0f, item.Item3);

                enumerator.MoveNext();
                item = enumerator.Current;
            
                Assert.AreEqual(2, item.Item1);
                Assert.AreEqual(2, item.Item2);
                Assert.AreEqual(3.0f, item.Item3);
            }
        }

        /// <summary>
        /// Can check if a matrix is symmetric.
        /// </summary>
        [Test]
        public override void CanCheckIfMatrixIsSymmetric()
        {
            var matrix = TestMatrices["Square3x3"];
            Assert.IsTrue(matrix.IsSymmetric);
        }

        /// <summary>
        /// Can get a sub-matrix.
        /// </summary>
        [Test]
        public override void CanGetASubMatrix()
        {
            var matrix = CreateMatrix(10, 10);
            for (var row = 0; row < matrix.RowCount; row++)
            {
                for (var column = 0; column < matrix.ColumnCount; column++)
                {
                    if (row == column)
                    {
                        matrix[row, column] = 1.0f;
                    }
                }
            }

            var submatrix = matrix.SubMatrix(8, 2, 0, 2);
            Assert.AreEqual(2, submatrix.RowCount);
            Assert.AreEqual(2, submatrix.ColumnCount);

            for (var row = 0; row < submatrix.RowCount; row++)
            {
                for (var column = 0; column < submatrix.ColumnCount; column++)
                {
                    if (row == column)
                    {
                        Assert.AreEqual(1.0f, submatrix[row, column]);
                    }
                    else
                    {
                        Assert.AreEqual(0.0f, submatrix[row, column]);
                    }
                }
            }
        }
    }
}
