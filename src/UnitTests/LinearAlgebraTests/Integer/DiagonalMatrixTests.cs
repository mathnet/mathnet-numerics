// <copyright file="DiagonalMatrixTests.cs" company="Math.NET">
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

using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Integer;
using MathNet.Numerics.Random;
using NUnit.Framework;

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Integer
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
            TestData2D = new Dictionary<string, int[,]>
                {
                    {"Singular3x3", new[,] {{10, 00, 00}, {00, 00, 00}, {00, 00, 30}}},
                    {"Square3x3", new[,] {{-11, 00, 00}, {00, 11, 00}, {00, 00, 66}}},
                    {"Square4x4", new[,] {{-11, 00, 00, 00}, {00, 11, 00, 00}, {00, 00, 62, 00}, {00, 00, 00, -77}}},
                    {"Singular4x4", new[,] {{-11, 00, 00, 00}, {00, -22, 00, 00}, {00, 00, 00, 00}, {00, 00, 00, -44}}},
                    {"Tall3x2", new[,] {{-11, 00}, {00, 11}, {00, 00}}},
                    {"Wide2x3", new[,] {{-11, 00, 00}, {00, 11, 00}}}
                };

            TestMatrices = new Dictionary<string, Matrix<int>>();
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
        protected override Matrix<int> CreateMatrix(int rows, int columns)
        {
            return new DiagonalMatrix(rows, columns);
        }

        /// <summary>
        /// Creates a matrix from a 2D array.
        /// </summary>
        /// <param name="data">The 2D array to create this matrix from.</param>
        /// <returns>A matrix with the given values.</returns>
        protected override Matrix<int> CreateMatrix(int[,] data)
        {
            return DiagonalMatrix.OfArray(data);
        }

        /// <summary>
        /// Can create a matrix from a diagonal array.
        /// </summary>
        [Test]
        public void CanCreateMatrixFromDiagonalArray()
        {
            var testData = new Dictionary<string, Matrix<int>>
                {
                    {"Singular3x3", new DiagonalMatrix(3, 3, new[] {10, 00, 30})},
                    {"Square3x3", new DiagonalMatrix(3, 3, new[] {-11, 11, 66})},
                    {"Square4x4", new DiagonalMatrix(4, 4, new[] {-11, 11, 62, -77})},
                    {"Tall3x2", new DiagonalMatrix(3, 2, new[] {-11, 11})},
                    {"Wide2x3", new DiagonalMatrix(2, 3, new[] {-11, 11})},
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
            var data = new int[] { 1, 2, 3, 4, 5 };
            var matrix = new DiagonalMatrix(5, 5, data);
            matrix[0, 0] = 10;
            Assert.AreEqual(10, data[0]);
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
            var matrix = new DiagonalMatrix(10, 10, 10);
            for (var i = 0; i < matrix.RowCount; i++)
            {
                Assert.AreEqual(matrix[i, i], 10);
            }
        }

        /// <summary>
        /// Can create an identity matrix.
        /// </summary>
        [Test]
        public void CanCreateIdentity()
        {
            var matrix = DiagonalMatrix.CreateIdentity(5);
            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    Assert.AreEqual(i == j ? 1 : 0, matrix[i, j]);
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
            Assert.That(() => DiagonalMatrix.CreateIdentity(order), Throws.TypeOf<ArgumentOutOfRangeException>());
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
                    Assert.AreEqual(matrixA.Row(i) * matrixB.Column(j), matrixC[i, j]);
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
            var permutation = new Permutation(new[] { 2, 0, 1 });
            Assert.That(() => matrixp.PermuteRows(permutation), Throws.InvalidOperationException);
        }

        /// <summary>
        /// Permute matrix columns throws <c>InvalidOperationException</c>.
        /// </summary>
        [Test]
        public void PermuteMatrixColumnsThrowsInvalidOperationException()
        {
            var matrixp = DiagonalMatrix.OfArray(TestData2D["Singular3x3"]);
            var permutation = new Permutation(new[] { 2, 0, 1 });
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
                data.PointwiseDivide(other, result);    // CONSIDER: divide by zero here?? (NaNs with float/double?)
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
        /// Can compute Frobenius norm.
        /// </summary>
        public override void CanComputeFrobeniusNorm()
        {
            var matrix = TestMatrices["Square3x3"];
            var denseMatrix = DenseMatrix.OfArray(TestData2D["Square3x3"]);
            AssertHelpers.AlmostEqualRelative(denseMatrix.FrobeniusNorm(), matrix.FrobeniusNorm(), 7);

            matrix = TestMatrices["Wide2x3"];
            denseMatrix = DenseMatrix.OfArray(TestData2D["Wide2x3"]);
            AssertHelpers.AlmostEqualRelative(denseMatrix.FrobeniusNorm(), matrix.FrobeniusNorm(), 7);

            matrix = TestMatrices["Tall3x2"];
            denseMatrix = DenseMatrix.OfArray(TestData2D["Tall3x2"]);
            AssertHelpers.AlmostEqualRelative(denseMatrix.FrobeniusNorm(), matrix.FrobeniusNorm(), 7);
        }

        /// <summary>
        /// Can compute Infinity norm.
        /// </summary>
        public override void CanComputeInfinityNorm()
        {
            var matrix = TestMatrices["Square3x3"];
            var denseMatrix = DenseMatrix.OfArray(TestData2D["Square3x3"]);
            AssertHelpers.AlmostEqualRelative(denseMatrix.InfinityNorm(), matrix.InfinityNorm(), 7);

            matrix = TestMatrices["Wide2x3"];
            denseMatrix = DenseMatrix.OfArray(TestData2D["Wide2x3"]);
            AssertHelpers.AlmostEqualRelative(denseMatrix.InfinityNorm(), matrix.InfinityNorm(), 7);

            matrix = TestMatrices["Tall3x2"];
            denseMatrix = DenseMatrix.OfArray(TestData2D["Tall3x2"]);
            AssertHelpers.AlmostEqualRelative(denseMatrix.InfinityNorm(), matrix.InfinityNorm(), 7);
        }

        /// <summary>
        /// Can compute L1 norm.
        /// </summary>
        public override void CanComputeL1Norm()
        {
            var matrix = TestMatrices["Square3x3"];
            var denseMatrix = DenseMatrix.OfArray(TestData2D["Square3x3"]);
            AssertHelpers.AlmostEqualRelative(denseMatrix.L1Norm(), matrix.L1Norm(), 7);

            matrix = TestMatrices["Wide2x3"];
            denseMatrix = DenseMatrix.OfArray(TestData2D["Wide2x3"]);
            AssertHelpers.AlmostEqualRelative(denseMatrix.L1Norm(), matrix.L1Norm(), 7);

            matrix = TestMatrices["Tall3x2"];
            denseMatrix = DenseMatrix.OfArray(TestData2D["Tall3x2"]);
            AssertHelpers.AlmostEqualRelative(denseMatrix.L1Norm(), matrix.L1Norm(), 7);
        }

        /// <summary>
        /// Can compute L2 norm.
        /// </summary>
        [Test]
        public void CanComputeL2Norm()
        {
            Func<int[,], double> calcDiagL2 = _data => {
                int l2 = 0;
                int dim = Math.Min(_data.GetLength(0), _data.GetLength(1));
                for (int rc = 0; rc < dim; rc++)
                {
                    l2 = Math.Max(l2, Math.Abs(_data[rc, rc]));
                }
                return (double)l2;
            };
            var matrix = TestMatrices["Square3x3"];
            var data = TestData2D["Square3x3"];
            AssertHelpers.AlmostEqualRelative(calcDiagL2(data), matrix.L2Norm(), 14);

            matrix = TestMatrices["Wide2x3"];
            data = TestData2D["Wide2x3"];
            AssertHelpers.AlmostEqualRelative(calcDiagL2(data), matrix.L2Norm(), 14);

            matrix = TestMatrices["Tall3x2"];
            data = TestData2D["Tall3x2"];
            AssertHelpers.AlmostEqualRelative(calcDiagL2(data), matrix.L2Norm(), 14);
        }

        public override void ComputeL2NormThrowsNotSupportedException()
        {
            Assert.Ignore("DiagonalMatrices CAN safely compute L2Norm");
        }

        /// <summary>
        /// Can compute determinant.
        /// </summary>
        [Test]
        public void CanComputeDeterminant()
        {
            Func<int[,], int> calcDiagDet = _data => {
                int det = 1;
                for (int rc = 0; rc < _data.GetLength(0); rc++)
                {
                    det *= _data[rc, rc];
                }
                return det;
            };
            var matrix = TestMatrices["Square3x3"];
            var data = TestData2D["Square3x3"];
            Assert.AreEqual(calcDiagDet(data), matrix.Determinant());

            matrix = TestMatrices["Square4x4"];
            data = TestData2D["Square4x4"];
            Assert.AreEqual(calcDiagDet(data), matrix.Determinant());
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
        public void DenseDiagonalMatrixMultiply()
        {
            var dist = new ContinuousUniform(-1.0, 1.0, new SystemRandomSource(1));
            Assert.IsInstanceOf<DiagonalMatrix>(Matrix<int>.Build.DiagonalIdentity(3, 3));

            var tall = Matrix<int>.Build.Random(8, 3, dist);
            Assert.IsTrue((tall * Matrix<int>.Build.DiagonalIdentity(3).Multiply(2)).Equals(tall.Multiply(2)));
            Assert.IsTrue((tall * Matrix<int>.Build.Diagonal(3, 5, 2)).Equals(tall.Multiply(2).Append(Matrix<int>.Build.Dense(8, 2))));
            Assert.IsTrue((tall * Matrix<int>.Build.Diagonal(3, 2, 2)).Equals(tall.Multiply(2).SubMatrix(0, 8, 0, 2)));

            var wide = Matrix<int>.Build.Random(3, 8, dist);
            Assert.IsTrue((wide * Matrix<int>.Build.DiagonalIdentity(8).Multiply(2)).Equals(wide.Multiply(2)));
            Assert.IsTrue((wide * Matrix<int>.Build.Diagonal(8, 10, 2)).Equals(wide.Multiply(2).Append(Matrix<int>.Build.Dense(3, 2))));
            Assert.IsTrue((wide * Matrix<int>.Build.Diagonal(8, 2, 2)).Equals(wide.Multiply(2).SubMatrix(0, 3, 0, 2)));
        }

        [Test]
        public void DenseDiagonalMatrixTransposeAndMultiply()
        {
            var dist = new ContinuousUniform(-1.0, 1.0, new SystemRandomSource(1));
            Assert.IsInstanceOf<DiagonalMatrix>(Matrix<int>.Build.DiagonalIdentity(3, 3));

            var tall = Matrix<int>.Build.Random(8, 3, dist);
            Assert.IsTrue(tall.TransposeAndMultiply(Matrix<int>.Build.DiagonalIdentity(3).Multiply(2)).Equals(tall.Multiply(2)));
            Assert.IsTrue(tall.TransposeAndMultiply(Matrix<int>.Build.Diagonal(5, 3, 2)).Equals(tall.Multiply(2).Append(Matrix<int>.Build.Dense(8, 2))));
            Assert.IsTrue(tall.TransposeAndMultiply(Matrix<int>.Build.Diagonal(2, 3, 2)).Equals(tall.Multiply(2).SubMatrix(0, 8, 0, 2)));

            var wide = Matrix<int>.Build.Random(3, 8, dist);
            Assert.IsTrue(wide.TransposeAndMultiply(Matrix<int>.Build.DiagonalIdentity(8).Multiply(2)).Equals(wide.Multiply(2)));
            Assert.IsTrue(wide.TransposeAndMultiply(Matrix<int>.Build.Diagonal(10, 8, 2)).Equals(wide.Multiply(2).Append(Matrix<int>.Build.Dense(3, 2))));
            Assert.IsTrue(wide.TransposeAndMultiply(Matrix<int>.Build.Diagonal(2, 8, 2)).Equals(wide.Multiply(2).SubMatrix(0, 3, 0, 2)));
        }

        [Test]
        public void DenseDiagonalMatrixTransposeThisAndMultiply()
        {
            var dist = new ContinuousUniform(-1.0, 1.0, new SystemRandomSource(1));
            Assert.IsInstanceOf<DiagonalMatrix>(Matrix<int>.Build.DiagonalIdentity(3, 3));

            var wide = Matrix<int>.Build.Random(3, 8, dist);
            Assert.IsTrue(wide.TransposeThisAndMultiply(Matrix<int>.Build.DiagonalIdentity(3).Multiply(2)).Equals(wide.Transpose().Multiply(2)));
            Assert.IsTrue(wide.TransposeThisAndMultiply(Matrix<int>.Build.Diagonal(3, 5, 2)).Equals(wide.Transpose().Multiply(2).Append(Matrix<int>.Build.Dense(8, 2))));
            Assert.IsTrue(wide.TransposeThisAndMultiply(Matrix<int>.Build.Diagonal(3, 2, 2)).Equals(wide.Transpose().Multiply(2).SubMatrix(0, 8, 0, 2)));

            var tall = Matrix<int>.Build.Random(8, 3, dist);
            Assert.IsTrue(tall.TransposeThisAndMultiply(Matrix<int>.Build.DiagonalIdentity(8).Multiply(2)).Equals(tall.Transpose().Multiply(2)));
            Assert.IsTrue(tall.TransposeThisAndMultiply(Matrix<int>.Build.Diagonal(8, 10, 2)).Equals(tall.Transpose().Multiply(2).Append(Matrix<int>.Build.Dense(3, 2))));
            Assert.IsTrue(tall.TransposeThisAndMultiply(Matrix<int>.Build.Diagonal(8, 2, 2)).Equals(tall.Transpose().Multiply(2).SubMatrix(0, 3, 0, 2)));
        }

        [Test]
        public void DiagonalDenseMatrixMultiply()
        {
            var dist = new ContinuousUniform(-1.0, 1.0, new SystemRandomSource(1));
            Assert.IsInstanceOf<DiagonalMatrix>(Matrix<int>.Build.DiagonalIdentity(3, 3));

            var wide = Matrix<int>.Build.Random(3, 8, dist);
            Assert.IsTrue((Matrix<int>.Build.DiagonalIdentity(3).Multiply(2) * wide).Equals(wide.Multiply(2)));
            Assert.IsTrue((Matrix<int>.Build.Diagonal(5, 3, 2) * wide).Equals(wide.Multiply(2).Stack(Matrix<int>.Build.Dense(2, 8))));
            Assert.IsTrue((Matrix<int>.Build.Diagonal(2, 3, 2) * wide).Equals(wide.Multiply(2).SubMatrix(0, 2, 0, 8)));

            var tall = Matrix<int>.Build.Random(8, 3, dist);
            Assert.IsTrue((Matrix<int>.Build.DiagonalIdentity(8).Multiply(2) * tall).Equals(tall.Multiply(2)));
            Assert.IsTrue((Matrix<int>.Build.Diagonal(10, 8, 2) * tall).Equals(tall.Multiply(2).Stack(Matrix<int>.Build.Dense(2, 3))));
            Assert.IsTrue((Matrix<int>.Build.Diagonal(2, 8, 2) * tall).Equals(tall.Multiply(2).SubMatrix(0, 2, 0, 3)));
        }

        [Test]
        public void DiagonalDenseMatrixTransposeAndMultiply()
        {
            var dist = new ContinuousUniform(-1.0, 1.0, new SystemRandomSource(1));
            Assert.IsInstanceOf<DiagonalMatrix>(Matrix<int>.Build.DiagonalIdentity(3, 3));

            var tall = Matrix<int>.Build.Random(8, 3, dist);
            Assert.IsTrue(Matrix<int>.Build.DiagonalIdentity(3).Multiply(2).TransposeAndMultiply(tall).Equals(tall.Multiply(2).Transpose()));
            Assert.IsTrue(Matrix<int>.Build.Diagonal(5, 3, 2).TransposeAndMultiply(tall).Equals(tall.Multiply(2).Append(Matrix<int>.Build.Dense(8, 2)).Transpose()));
            Assert.IsTrue(Matrix<int>.Build.Diagonal(2, 3, 2).TransposeAndMultiply(tall).Equals(tall.Multiply(2).SubMatrix(0, 8, 0, 2).Transpose()));

            var wide = Matrix<int>.Build.Random(3, 8, dist);
            Assert.IsTrue(Matrix<int>.Build.DiagonalIdentity(8).Multiply(2).TransposeAndMultiply(wide).Equals(wide.Multiply(2).Transpose()));
            Assert.IsTrue(Matrix<int>.Build.Diagonal(10, 8, 2).TransposeAndMultiply(wide).Equals(wide.Multiply(2).Append(Matrix<int>.Build.Dense(3, 2)).Transpose()));
            Assert.IsTrue(Matrix<int>.Build.Diagonal(2, 8, 2).TransposeAndMultiply(wide).Equals(wide.Multiply(2).SubMatrix(0, 3, 0, 2).Transpose()));
        }

        [Test]
        public void DiagonalDenseMatrixTransposeThisAndMultiply()
        {
            var dist = new ContinuousUniform(-1.0, 1.0, new SystemRandomSource(1));
            Assert.IsInstanceOf<DiagonalMatrix>(Matrix<int>.Build.DiagonalIdentity(3, 3));

            var wide = Matrix<int>.Build.Random(3, 8, dist);
            Assert.IsTrue((Matrix<int>.Build.DiagonalIdentity(3).Multiply(2).TransposeThisAndMultiply(wide)).Equals(wide.Multiply(2)));
            Assert.IsTrue((Matrix<int>.Build.Diagonal(3, 5, 2).TransposeThisAndMultiply(wide)).Equals(wide.Multiply(2).Stack(Matrix<int>.Build.Dense(2, 8))));
            Assert.IsTrue((Matrix<int>.Build.Diagonal(3, 2, 2).TransposeThisAndMultiply(wide)).Equals(wide.Multiply(2).SubMatrix(0, 2, 0, 8)));

            var tall = Matrix<int>.Build.Random(8, 3, dist);
            Assert.IsTrue((Matrix<int>.Build.DiagonalIdentity(8).Multiply(2).TransposeThisAndMultiply(tall)).Equals(tall.Multiply(2)));
            Assert.IsTrue((Matrix<int>.Build.Diagonal(8, 10, 2).TransposeThisAndMultiply(tall)).Equals(tall.Multiply(2).Stack(Matrix<int>.Build.Dense(2, 3))));
            Assert.IsTrue((Matrix<int>.Build.Diagonal(8, 2, 2).TransposeThisAndMultiply(tall)).Equals(tall.Multiply(2).SubMatrix(0, 2, 0, 3)));
        }
    }
}
