// <copyright file="DiagonalMatrixTests.cs" company="Math.NET">
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
using MathNet.Numerics.Distributions;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.Random;
using NUnit.Framework;

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Double
{
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
            TestData2D = new Dictionary<string, double[,]>
                {
                    {"Singular3x3", new[,] {{1.0, 0.0, 0.0}, {0.0, 0.0, 0.0}, {0.0, 0.0, 3.0}}},
                    {"Square3x3", new[,] {{-1.1, 0.0, 0.0}, {0.0, 1.1, 0.0}, {0.0, 0.0, 6.6}}},
                    {"Square4x4", new[,] {{-1.1, 0.0, 0.0, 0.0}, {0.0, 1.1, 0.0, 0.0}, {0.0, 0.0, 6.2, 0.0}, {0.0, 0.0, 0.0, -7.7}}},
                    {"Singular4x4", new[,] {{-1.1, 0.0, 0.0, 0.0}, {0.0, -2.2, 0.0, 0.0}, {0.0, 0.0, 0.0, 0.0}, {0.0, 0.0, 0.0, -4.4}}},
                    {"Tall3x2", new[,] {{-1.1, 0.0}, {0.0, 1.1}, {0.0, 0.0}}},
                    {"Wide2x3", new[,] {{-1.1, 0.0, 0.0}, {0.0, 1.1, 0.0}}}
                };

            TestMatrices = new Dictionary<string, Matrix<double>>();
            foreach (var name in TestData2D.Keys)
            {
                TestMatrices.Add(name, DiagonalMatrix.OfArray(TestData2D[name]));
            }
        }

        /// <summary>
        /// Creates a matrix for the given number of rows and columns.
        /// </summary>
        /// <param name="rows">The number of rows.</param>
        /// <param name="columns">The number of columns.</param>
        /// <returns>A matrix with the given dimensions.</returns>
        protected override Matrix<double> CreateMatrix(int rows, int columns)
        {
            return Matrix<double>.Build.Diagonal(rows, columns);
        }

        /// <summary>
        /// Creates a matrix from a 2D array.
        /// </summary>
        /// <param name="data">The 2D array to create this matrix from.</param>
        /// <returns>A matrix with the given values.</returns>
        protected override Matrix<double> CreateMatrix(double[,] data)
        {
            return DiagonalMatrix.OfArray(data);
        }

        /// <summary>
        /// Can create a matrix from a diagonal array.
        /// </summary>
        [Test]
        public void CanCreateMatrixFromDiagonalArray()
        {
            var testData = new Dictionary<string, Matrix<double>>
                {
                    {"Singular3x3", new DiagonalMatrix(3, 3, new[] {1.0, 0.0, 3.0})},
                    {"Square3x3", new DiagonalMatrix(3, 3, new[] {-1.1, 1.1, 6.6})},
                    {"Square4x4", new DiagonalMatrix(4, 4, new[] {-1.1, 1.1, 6.2, -7.7})},
                    {"Tall3x2", new DiagonalMatrix(3, 2, new[] {-1.1, 1.1})},
                    {"Wide2x3", new DiagonalMatrix(2, 3, new[] {-1.1, 1.1})},
                };

            foreach (var name in testData.Keys)
            {
                Assert.That(testData[name], Is.EqualTo(TestMatrices[name]));
            }
        }

        /// <summary>
        /// Matrix from array is a reference.
        /// </summary>
        [Test]
        public void MatrixFrom1DArrayIsReference()
        {
            var data = new double[] {1, 2, 3, 4, 5};
            var matrix = new DiagonalMatrix(5, 5, data);
            matrix[0, 0] = 10.0;
            Assert.AreEqual(10.0, data[0]);
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
            var matrix = DiagonalMatrix.OfArray(TestData2D[name]);
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
            var matrix = new DiagonalMatrix(10, 10, 10.0);
            for (var i = 0; i < matrix.RowCount; i++)
            {
                Assert.AreEqual(matrix[i, i], 10.0);
            }
        }

        /// <summary>
        /// Can create an identity matrix.
        /// </summary>
        [Test]
        public void CanCreateIdentity()
        {
            var matrix = Matrix<double>.Build.DiagonalIdentity(5);
            Assert.That(matrix, Is.TypeOf<DiagonalMatrix>());
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
            Assert.That(() => Matrix<double>.Build.DiagonalIdentity(order), Throws.TypeOf<ArgumentOutOfRangeException>());
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
            var matrixC = Matrix<double>.Build.Sparse(matrixA.RowCount, matrixB.ColumnCount);
            matrixA.Multiply(matrixB, matrixC);

            Assert.AreEqual(matrixC.RowCount, matrixA.RowCount);
            Assert.AreEqual(matrixC.ColumnCount, matrixB.ColumnCount);

            for (var i = 0; i < matrixC.RowCount; i++)
            {
                for (var j = 0; j < matrixC.ColumnCount; j++)
                {
                    AssertHelpers.AlmostEqualRelative(matrixA.Row(i)*matrixB.Column(j), matrixC[i, j], 15);
                }
            }
        }

        /// <summary>
        /// Permute matrix rows throws <c>InvalidOperationException</c>.
        /// </summary>
        [Test]
        public void PermuteMatrixRowsThrowsInvalidOperationException()
        {
            var matrixp = DiagonalMatrix.OfArray(TestData2D["Singular3x3"]);
            var permutation = new Permutation(new[] {2, 0, 1});
            Assert.That(() => matrixp.PermuteRows(permutation), Throws.InvalidOperationException);
        }

        /// <summary>
        /// Permute matrix columns throws <c>InvalidOperationException</c>.
        /// </summary>
        [Test]
        public void PermuteMatrixColumnsThrowsInvalidOperationException()
        {
            var matrixp = DiagonalMatrix.OfArray(TestData2D["Singular3x3"]);
            var permutation = new Permutation(new[] {2, 0, 1});
            Assert.That(() => matrixp.PermuteColumns(permutation), Throws.InvalidOperationException);
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
                    Assert.AreEqual(data[i, i]/other[i, i], result[i, i]);
                }

                result = data.PointwiseDivide(other);
                for (var i = 0; i < min; i++)
                {
                    Assert.AreEqual(data[i, i]/other[i, i], result[i, i]);
                }
            }
        }

        /// <summary>
        /// Can compute Frobenius norm.
        /// </summary>
        public override void CanComputeFrobeniusNorm()
        {
            var matrix = TestMatrices["Square3x3"];
            var denseMatrix = Matrix<double>.Build.DenseOfArray(TestData2D["Square3x3"]);
            AssertHelpers.AlmostEqualRelative(denseMatrix.FrobeniusNorm(), matrix.FrobeniusNorm(), 14);

            matrix = TestMatrices["Wide2x3"];
            denseMatrix = Matrix<double>.Build.DenseOfArray(TestData2D["Wide2x3"]);
            AssertHelpers.AlmostEqualRelative(denseMatrix.FrobeniusNorm(), matrix.FrobeniusNorm(), 14);

            matrix = TestMatrices["Tall3x2"];
            denseMatrix = Matrix<double>.Build.DenseOfArray(TestData2D["Tall3x2"]);
            AssertHelpers.AlmostEqualRelative(denseMatrix.FrobeniusNorm(), matrix.FrobeniusNorm(), 14);
        }

        /// <summary>
        /// Can compute Infinity norm.
        /// </summary>
        public override void CanComputeInfinityNorm()
        {
            var matrix = TestMatrices["Square3x3"];
            var denseMatrix = Matrix<double>.Build.DenseOfArray(TestData2D["Square3x3"]);
            AssertHelpers.AlmostEqualRelative(denseMatrix.InfinityNorm(), matrix.InfinityNorm(), 14);

            matrix = TestMatrices["Wide2x3"];
            denseMatrix = Matrix<double>.Build.DenseOfArray(TestData2D["Wide2x3"]);
            AssertHelpers.AlmostEqualRelative(denseMatrix.InfinityNorm(), matrix.InfinityNorm(), 14);

            matrix = TestMatrices["Tall3x2"];
            denseMatrix = Matrix<double>.Build.DenseOfArray(TestData2D["Tall3x2"]);
            AssertHelpers.AlmostEqualRelative(denseMatrix.InfinityNorm(), matrix.InfinityNorm(), 14);
        }

        /// <summary>
        /// Can compute L1 norm.
        /// </summary>
        public override void CanComputeL1Norm()
        {
            var matrix = TestMatrices["Square3x3"];
            var denseMatrix = Matrix<double>.Build.DenseOfArray(TestData2D["Square3x3"]);
            AssertHelpers.AlmostEqualRelative(denseMatrix.L1Norm(), matrix.L1Norm(), 14);

            matrix = TestMatrices["Wide2x3"];
            denseMatrix = Matrix<double>.Build.DenseOfArray(TestData2D["Wide2x3"]);
            AssertHelpers.AlmostEqualRelative(denseMatrix.L1Norm(), matrix.L1Norm(), 14);

            matrix = TestMatrices["Tall3x2"];
            denseMatrix = Matrix<double>.Build.DenseOfArray(TestData2D["Tall3x2"]);
            AssertHelpers.AlmostEqualRelative(denseMatrix.L1Norm(), matrix.L1Norm(), 14);
        }

        /// <summary>
        /// Can compute L2 norm.
        /// </summary>
        public override void CanComputeL2Norm()
        {
            var matrix = TestMatrices["Square3x3"];
            var denseMatrix = Matrix<double>.Build.DenseOfArray(TestData2D["Square3x3"]);
            AssertHelpers.AlmostEqualRelative(denseMatrix.L2Norm(), matrix.L2Norm(), 14);

            matrix = TestMatrices["Wide2x3"];
            denseMatrix = Matrix<double>.Build.DenseOfArray(TestData2D["Wide2x3"]);
            AssertHelpers.AlmostEqualRelative(denseMatrix.L2Norm(), matrix.L2Norm(), 14);

            matrix = TestMatrices["Tall3x2"];
            denseMatrix = Matrix<double>.Build.DenseOfArray(TestData2D["Tall3x2"]);
            AssertHelpers.AlmostEqualRelative(denseMatrix.L2Norm(), matrix.L2Norm(), 14);
        }

        /// <summary>
        /// Can compute determinant.
        /// </summary>
        [Test]
        public void CanComputeDeterminant()
        {
            var matrix = TestMatrices["Square3x3"];
            var denseMatrix = Matrix<double>.Build.DenseOfArray(TestData2D["Square3x3"]);
            AssertHelpers.AlmostEqualRelative(denseMatrix.Determinant(), matrix.Determinant(), 14);

            matrix = TestMatrices["Square4x4"];
            denseMatrix = Matrix<double>.Build.DenseOfArray(TestData2D["Square4x4"]);
            AssertHelpers.AlmostEqualRelative(denseMatrix.Determinant(), matrix.Determinant(), 14);
        }

        /// <summary>
        /// Determinant of  non-square matrix throws <c>ArgumentException</c>.
        /// </summary>
        [Test]
        public void DeterminantNotSquareMatrixThrowsArgumentException()
        {
            var matrix = TestMatrices["Tall3x2"];
            Assert.That(() => matrix.Determinant(), Throws.ArgumentException);
        }

        /// <summary>
        /// Can check if a matrix is symmetric.
        /// </summary>
        [Test]
        public override void CanCheckIfMatrixIsSymmetric()
        {
            var matrix = TestMatrices["Square3x3"];
            Assert.IsTrue(matrix.IsSymmetric());
        }

        [Test]
        public void CanGetSubMatrix_Issue35()
        {
            // [ 1 0 0 ]
            // [ 0 2 0 ]
            // [ 0 0 3 ]
            var diagMatrix = new DiagonalMatrix(3);
            for (int i = 0; i < 3; i++)
            {
                diagMatrix[i, i] = i + 1;
            }

            // [ 0 0 ]
            // [ 2 0 ]
            var subM2 = diagMatrix.SubMatrix(0, 2, 1, 2);
            Assert.IsTrue(subM2.Equals(Matrix<double>.Build.Dense(2, 2, new[] { 0d, 2d, 0d, 0d })));

            // [ 0 0 ]
            // [ 2 0 ]
            // [ 0 3 ]
            var subM3 = diagMatrix.SubMatrix(0, 3, 1, 2);
            Assert.IsTrue(subM3.Equals(Matrix<double>.Build.Dense(3, 2, new[] { 0d, 2d, 0d, 0d, 0d, 3d })));
        }

        [Test]
        public void DiagonalDenseMatrixMultiplication_IssueCP5706()
        {
            Matrix<double> diagonal = Matrix<double>.Build.DiagonalIdentity(3);
            Matrix<double> dense = Matrix<double>.Build.DenseOfArray(new double[,] { { 1, 2, 3 }, { 1, 2, 3 }, { 1, 2, 3 } });
            var test = diagonal*dense;
            var test2 = dense*diagonal;
        }

        [Test]
        public void DenseDiagonalMatrixMultiply()
        {
            var dist = new ContinuousUniform(-1.0, 1.0, new SystemRandomSource(1));
            Assert.IsInstanceOf<DiagonalMatrix>(Matrix<double>.Build.DiagonalIdentity(3, 3));

            var tall = Matrix<double>.Build.Random(8, 3, dist);
            Assert.IsTrue((tall*Matrix<double>.Build.DiagonalIdentity(3).Multiply(2d)).Equals(tall.Multiply(2d)));
            Assert.IsTrue((tall*Matrix<double>.Build.Diagonal(3, 5, 2d)).Equals(tall.Multiply(2d).Append(Matrix<double>.Build.Dense(8, 2))));
            Assert.IsTrue((tall*Matrix<double>.Build.Diagonal(3, 2, 2d)).Equals(tall.Multiply(2d).SubMatrix(0, 8, 0, 2)));

            var wide = Matrix<double>.Build.Random(3, 8, dist);
            Assert.IsTrue((wide*Matrix<double>.Build.DiagonalIdentity(8).Multiply(2d)).Equals(wide.Multiply(2d)));
            Assert.IsTrue((wide*Matrix<double>.Build.Diagonal(8, 10, 2d)).Equals(wide.Multiply(2d).Append(Matrix<double>.Build.Dense(3, 2))));
            Assert.IsTrue((wide*Matrix<double>.Build.Diagonal(8, 2, 2d)).Equals(wide.Multiply(2d).SubMatrix(0, 3, 0, 2)));
        }

        [Test]
        public void DenseDiagonalMatrixTransposeAndMultiply()
        {
            var dist = new ContinuousUniform(-1.0, 1.0, new SystemRandomSource(1));
            Assert.IsInstanceOf<DiagonalMatrix>(Matrix<double>.Build.DiagonalIdentity(3, 3));

            var tall = Matrix<double>.Build.Random(8, 3, dist);
            Assert.IsTrue(tall.TransposeAndMultiply(Matrix<double>.Build.DiagonalIdentity(3).Multiply(2d)).Equals(tall.Multiply(2d)));
            Assert.IsTrue(tall.TransposeAndMultiply(Matrix<double>.Build.Diagonal(5, 3, 2d)).Equals(tall.Multiply(2d).Append(Matrix<double>.Build.Dense(8, 2))));
            Assert.IsTrue(tall.TransposeAndMultiply(Matrix<double>.Build.Diagonal(2, 3, 2d)).Equals(tall.Multiply(2d).SubMatrix(0, 8, 0, 2)));

            var wide = Matrix<double>.Build.Random(3, 8, dist);
            Assert.IsTrue(wide.TransposeAndMultiply(Matrix<double>.Build.DiagonalIdentity(8).Multiply(2d)).Equals(wide.Multiply(2d)));
            Assert.IsTrue(wide.TransposeAndMultiply(Matrix<double>.Build.Diagonal(10, 8, 2d)).Equals(wide.Multiply(2d).Append(Matrix<double>.Build.Dense(3, 2))));
            Assert.IsTrue(wide.TransposeAndMultiply(Matrix<double>.Build.Diagonal(2, 8, 2d)).Equals(wide.Multiply(2d).SubMatrix(0, 3, 0, 2)));
        }

        [Test]
        public void DenseDiagonalMatrixTransposeThisAndMultiply()
        {
            var dist = new ContinuousUniform(-1.0, 1.0, new SystemRandomSource(1));
            Assert.IsInstanceOf<DiagonalMatrix>(Matrix<double>.Build.DiagonalIdentity(3, 3));

            var wide = Matrix<double>.Build.Random(3, 8, dist);
            Assert.IsTrue(wide.TransposeThisAndMultiply(Matrix<double>.Build.DiagonalIdentity(3).Multiply(2d)).Equals(wide.Transpose().Multiply(2d)));
            Assert.IsTrue(wide.TransposeThisAndMultiply(Matrix<double>.Build.Diagonal(3, 5, 2d)).Equals(wide.Transpose().Multiply(2d).Append(Matrix<double>.Build.Dense(8, 2))));
            Assert.IsTrue(wide.TransposeThisAndMultiply(Matrix<double>.Build.Diagonal(3, 2, 2d)).Equals(wide.Transpose().Multiply(2d).SubMatrix(0, 8, 0, 2)));

            var tall = Matrix<double>.Build.Random(8, 3, dist);
            Assert.IsTrue(tall.TransposeThisAndMultiply(Matrix<double>.Build.DiagonalIdentity(8).Multiply(2d)).Equals(tall.Transpose().Multiply(2d)));
            Assert.IsTrue(tall.TransposeThisAndMultiply(Matrix<double>.Build.Diagonal(8, 10, 2d)).Equals(tall.Transpose().Multiply(2d).Append(Matrix<double>.Build.Dense(3, 2))));
            Assert.IsTrue(tall.TransposeThisAndMultiply(Matrix<double>.Build.Diagonal(8, 2, 2d)).Equals(tall.Transpose().Multiply(2d).SubMatrix(0, 3, 0, 2)));
        }

        [Test]
        public void DiagonalDenseMatrixMultiply()
        {
            var dist = new ContinuousUniform(-1.0, 1.0, new SystemRandomSource(1));
            Assert.IsInstanceOf<DiagonalMatrix>(Matrix<double>.Build.DiagonalIdentity(3, 3));

            var wide = Matrix<double>.Build.Random(3, 8, dist);
            Assert.IsTrue((Matrix<double>.Build.DiagonalIdentity(3).Multiply(2d)*wide).Equals(wide.Multiply(2d)));
            Assert.IsTrue((Matrix<double>.Build.Diagonal(5, 3, 2d)*wide).Equals(wide.Multiply(2d).Stack(Matrix<double>.Build.Dense(2, 8))));
            Assert.IsTrue((Matrix<double>.Build.Diagonal(2, 3, 2d)*wide).Equals(wide.Multiply(2d).SubMatrix(0, 2, 0, 8)));

            var tall = Matrix<double>.Build.Random(8, 3, dist);
            Assert.IsTrue((Matrix<double>.Build.DiagonalIdentity(8).Multiply(2d)*tall).Equals(tall.Multiply(2d)));
            Assert.IsTrue((Matrix<double>.Build.Diagonal(10, 8, 2d)*tall).Equals(tall.Multiply(2d).Stack(Matrix<double>.Build.Dense(2, 3))));
            Assert.IsTrue((Matrix<double>.Build.Diagonal(2, 8, 2d)*tall).Equals(tall.Multiply(2d).SubMatrix(0, 2, 0, 3)));
        }

        [Test]
        public void DiagonalDenseMatrixTransposeAndMultiply()
        {
            var dist = new ContinuousUniform(-1.0, 1.0, new SystemRandomSource(1));
            Assert.IsInstanceOf<DiagonalMatrix>(Matrix<double>.Build.DiagonalIdentity(3, 3));

            var tall = Matrix<double>.Build.Random(8, 3, dist);
            Assert.IsTrue(Matrix<double>.Build.DiagonalIdentity(3).Multiply(2d).TransposeAndMultiply(tall).Equals(tall.Multiply(2d).Transpose()));
            Assert.IsTrue(Matrix<double>.Build.Diagonal(5, 3, 2d).TransposeAndMultiply(tall).Equals(tall.Multiply(2d).Append(Matrix<double>.Build.Dense(8, 2)).Transpose()));
            Assert.IsTrue(Matrix<double>.Build.Diagonal(2, 3, 2d).TransposeAndMultiply(tall).Equals(tall.Multiply(2d).SubMatrix(0, 8, 0, 2).Transpose()));

            var wide = Matrix<double>.Build.Random(3, 8, dist);
            Assert.IsTrue(Matrix<double>.Build.DiagonalIdentity(8).Multiply(2d).TransposeAndMultiply(wide).Equals(wide.Multiply(2d).Transpose()));
            Assert.IsTrue(Matrix<double>.Build.Diagonal(10, 8, 2d).TransposeAndMultiply(wide).Equals(wide.Multiply(2d).Append(Matrix<double>.Build.Dense(3, 2)).Transpose()));
            Assert.IsTrue(Matrix<double>.Build.Diagonal(2, 8, 2d).TransposeAndMultiply(wide).Equals(wide.Multiply(2d).SubMatrix(0, 3, 0, 2).Transpose()));
        }

        [Test]
        public void DiagonalDenseMatrixTransposeThisAndMultiply()
        {
            var dist = new ContinuousUniform(-1.0, 1.0, new SystemRandomSource(1));
            Assert.IsInstanceOf<DiagonalMatrix>(Matrix<double>.Build.DiagonalIdentity(3, 3));

            var wide = Matrix<double>.Build.Random(3, 8, dist);
            Assert.IsTrue((Matrix<double>.Build.DiagonalIdentity(3).Multiply(2d).TransposeThisAndMultiply(wide)).Equals(wide.Multiply(2d)));
            Assert.IsTrue((Matrix<double>.Build.Diagonal(3, 5, 2d).TransposeThisAndMultiply(wide)).Equals(wide.Multiply(2d).Stack(Matrix<double>.Build.Dense(2, 8))));
            Assert.IsTrue((Matrix<double>.Build.Diagonal(3, 2, 2d).TransposeThisAndMultiply(wide)).Equals(wide.Multiply(2d).SubMatrix(0, 2, 0, 8)));

            var tall = Matrix<double>.Build.Random(8, 3, dist);
            Assert.IsTrue((Matrix<double>.Build.DiagonalIdentity(8).Multiply(2d).TransposeThisAndMultiply(tall)).Equals(tall.Multiply(2d)));
            Assert.IsTrue((Matrix<double>.Build.Diagonal(8, 10, 2d).TransposeThisAndMultiply(tall)).Equals(tall.Multiply(2d).Stack(Matrix<double>.Build.Dense(2, 3))));
            Assert.IsTrue((Matrix<double>.Build.Diagonal(8, 2, 2d).TransposeThisAndMultiply(tall)).Equals(tall.Multiply(2d).SubMatrix(0, 2, 0, 3)));
        }
    }
}
