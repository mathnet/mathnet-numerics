// <copyright file="UserSvdTests.cs" company="Math.NET">
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

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Complex.Factorization
{
    using System;
    using System.Numerics;
    using LinearAlgebra.Complex;
    using LinearAlgebra.Complex.Factorization;
    using NUnit.Framework;

    /// <summary>
    /// Svd factorization tests for a user matrix.
    /// </summary>
    public class UserSvdTests
    {
        /// <summary>
        /// Constructor with <c>null</c> throws <c>ArgumentNullException</c>.
        /// </summary>
        [Test]
        public void ConstructorNull()
        {
            Assert.Throws<ArgumentNullException>(() => new UserSvd(null, true));
        }

        /// <summary>
        /// Can factorize identity matrix.
        /// </summary>
        /// <param name="order">Matrix order.</param>
        [Test]
        public void CanFactorizeIdentity([Values(1, 10, 100)] int order)
        {
            var matrixI = UserDefinedMatrix.Identity(order);
            var factorSvd = matrixI.Svd(true);
            var u = factorSvd.U();
            var vt = factorSvd.VT();
            var w = factorSvd.W();

            Assert.AreEqual(matrixI.RowCount, u.RowCount);
            Assert.AreEqual(matrixI.RowCount, u.ColumnCount);

            Assert.AreEqual(matrixI.ColumnCount, vt.RowCount);
            Assert.AreEqual(matrixI.ColumnCount, vt.ColumnCount);

            Assert.AreEqual(matrixI.RowCount, w.RowCount);
            Assert.AreEqual(matrixI.ColumnCount, w.ColumnCount);

            for (var i = 0; i < w.RowCount; i++)
            {
                for (var j = 0; j < w.ColumnCount; j++)
                {
                    Assert.AreEqual(i == j ? Complex.One : Complex.Zero, w[i, j]);
                }
            }
        }

        /// <summary>
        /// Can factorize a random matrix.
        /// </summary>
        /// <param name="row">Matrix row number.</param>
        /// <param name="column">Matrix column number.</param>
        [Test, Sequential]
        public void CanFactorizeRandomMatrix([Values(1, 2, 5, 10, 50, 100)] int row, [Values(1, 2, 5, 6, 48, 98)] int column)
        {
            var matrixA = MatrixLoader.GenerateRandomUserDefinedMatrix(row, column);
            var factorSvd = matrixA.Svd(true);
            var u = factorSvd.U();
            var vt = factorSvd.VT();
            var w = factorSvd.W();

            // Make sure the U has the right dimensions.
            Assert.AreEqual(row, u.RowCount);
            Assert.AreEqual(row, u.ColumnCount);

            // Make sure the VT has the right dimensions.
            Assert.AreEqual(column, vt.RowCount);
            Assert.AreEqual(column, vt.ColumnCount);

            // Make sure the W has the right dimensions.
            Assert.AreEqual(row, w.RowCount);
            Assert.AreEqual(column, w.ColumnCount);

            // Make sure the U*W*VT is the original matrix.
            var matrix = u * w * vt;
            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    AssertHelpers.AlmostEqual(matrixA[i, j], matrix[i, j], 9);
                }
            }
        }

        /// <summary>
        /// Can check rank of a non-square matrix.
        /// </summary>
        /// <param name="row">Matrix row number.</param>
        /// <param name="column">Matrix column number.</param>
        [Test, Sequential]
        public void CanCheckRankOfNonSquare([Values(10, 48, 100)] int row, [Values(8, 52, 93)] int column)
        {
            var matrixA = MatrixLoader.GenerateRandomUserDefinedMatrix(row, column);
            var factorSvd = matrixA.Svd(true);

            var mn = Math.Min(row, column);
            Assert.AreEqual(factorSvd.Rank, mn);
        }

        /// <summary>
        /// Can check rank of a square matrix.
        /// </summary>
        /// <param name="order">Matrix order.</param>
        [Test]
        public void CanCheckRankSquare([Values(1, 2, 5, 9, 50, 90)] int order)
        {
            var matrixA = MatrixLoader.GenerateRandomUserDefinedMatrix(order, order);
            var factorSvd = matrixA.Svd(true);

            if (factorSvd.Determinant != 0)
            {
                Assert.AreEqual(factorSvd.Rank, order);
            }
            else
            {
                Assert.AreEqual(factorSvd.Rank, order - 1);
            }
        }

        /// <summary>
        /// Can check rank of a square singular matrix.
        /// </summary>
        /// <param name="order">Matrix order.</param>
        [Test]
        public void CanCheckRankOfSquareSingular([Values(10, 50, 100)] int order)
        {
            var matrixA = new UserDefinedMatrix(order, order);
            matrixA[0, 0] = 1;
            matrixA[order - 1, order - 1] = 1;
            for (var i = 1; i < order - 1; i++)
            {
                matrixA[i, i - 1] = 1;
                matrixA[i, i + 1] = 1;
                matrixA[i - 1, i] = 1;
                matrixA[i + 1, i] = 1;
            }

            var factorSvd = matrixA.Svd(true);

            Assert.AreEqual(factorSvd.Determinant, Complex.Zero);
            Assert.AreEqual(factorSvd.Rank, order - 1);
        }

        /// <summary>
        /// Solve for matrix if vectors are not computed throws <c>InvalidOperationException</c>.
        /// </summary>
        [Test]
        public void SolveMatrixIfVectorsNotComputedThrowsInvalidOperationException()
        {
            var matrixA = MatrixLoader.GenerateRandomUserDefinedMatrix(10, 9);
            var factorSvd = matrixA.Svd(false);

            var matrixB = MatrixLoader.GenerateRandomUserDefinedMatrix(10, 9);
            Assert.Throws<InvalidOperationException>(() => factorSvd.Solve(matrixB));
        }

        /// <summary>
        /// Solve for vector if vectors are not computed throws <c>InvalidOperationException</c>.
        /// </summary>
        [Test]
        public void SolveVectorIfVectorsNotComputedThrowsInvalidOperationException()
        {
            var matrixA = MatrixLoader.GenerateRandomUserDefinedMatrix(10, 9);
            var factorSvd = matrixA.Svd(false);

            var vectorb = MatrixLoader.GenerateRandomUserDefinedVector(9);
            Assert.Throws<InvalidOperationException>(() => factorSvd.Solve(vectorb));
        }

        /// <summary>
        /// Can solve a system of linear equations for a random vector (Ax=b).
        /// </summary>
        /// <param name="row">Matrix row number.</param>
        /// <param name="column">Matrix column number.</param>
        [Test, Sequential]
        public void CanSolveForRandomVector([Values(1, 2, 5, 9, 50, 90)] int row, [Values(1, 2, 5, 10, 50, 100)] int column)
        {
            var matrixA = MatrixLoader.GenerateRandomUserDefinedMatrix(row, column);
            var matrixACopy = matrixA.Clone();
            var factorSvd = matrixA.Svd(true);

            var vectorb = MatrixLoader.GenerateRandomUserDefinedVector(row);
            var resultx = factorSvd.Solve(vectorb);

            Assert.AreEqual(matrixA.ColumnCount, resultx.Count);

            var matrixBReconstruct = matrixA * resultx;

            // Check the reconstruction.
            for (var i = 0; i < vectorb.Count; i++)
            {
                AssertHelpers.AlmostEqual(vectorb[i], matrixBReconstruct[i], 9);
            }

            // Make sure A didn't change.
            for (var i = 0; i < matrixA.RowCount; i++)
            {
                for (var j = 0; j < matrixA.ColumnCount; j++)
                {
                    Assert.AreEqual(matrixACopy[i, j], matrixA[i, j]);
                }
            }
        }

        /// <summary>
        /// Can solve a system of linear equations for a random matrix (AX=B).
        /// </summary>
        /// <param name="row">Matrix row number.</param>
        /// <param name="column">Matrix column number.</param>
        [Test, Sequential]
        public void CanSolveForRandomMatrix([Values(1, 4, 7, 10, 45, 80)] int row, [Values(1, 4, 8, 10, 50, 100)] int column)
        {
            var matrixA = MatrixLoader.GenerateRandomUserDefinedMatrix(row, column);
            var matrixACopy = matrixA.Clone();
            var factorSvd = matrixA.Svd(true);

            var matrixB = MatrixLoader.GenerateRandomUserDefinedMatrix(row, column);
            var matrixX = factorSvd.Solve(matrixB);

            // The solution X row dimension is equal to the column dimension of A
            Assert.AreEqual(matrixA.ColumnCount, matrixX.RowCount);

            // The solution X has the same number of columns as B
            Assert.AreEqual(matrixB.ColumnCount, matrixX.ColumnCount);

            var matrixBReconstruct = matrixA * matrixX;

            // Check the reconstruction.
            for (var i = 0; i < matrixB.RowCount; i++)
            {
                for (var j = 0; j < matrixB.ColumnCount; j++)
                {
                    AssertHelpers.AlmostEqual(matrixB[i, j], matrixBReconstruct[i, j], 9);
                }
            }

            // Make sure A didn't change.
            for (var i = 0; i < matrixA.RowCount; i++)
            {
                for (var j = 0; j < matrixA.ColumnCount; j++)
                {
                    Assert.AreEqual(matrixACopy[i, j], matrixA[i, j]);
                }
            }
        }

        /// <summary>
        /// Can solve for a random vector into a result vector.
        /// </summary>
        /// <param name="row">Matrix row number.</param>
        /// <param name="column">Matrix column number.</param>
        [Test, Sequential]
        public void CanSolveForRandomVectorWhenResultVectorGiven([Values(1, 2, 5, 9, 50, 90)] int row, [Values(1, 2, 5, 10, 50, 100)] int column)
        {
            var matrixA = MatrixLoader.GenerateRandomUserDefinedMatrix(row, column);
            var matrixACopy = matrixA.Clone();
            var factorSvd = matrixA.Svd(true);
            var vectorb = MatrixLoader.GenerateRandomUserDefinedVector(row);
            var vectorbCopy = vectorb.Clone();
            var resultx = new UserDefinedVector(column);
            factorSvd.Solve(vectorb, resultx);

            var matrixBReconstruct = matrixA * resultx;

            // Check the reconstruction.
            for (var i = 0; i < vectorb.Count; i++)
            {
                AssertHelpers.AlmostEqual(vectorb[i], matrixBReconstruct[i], 9);
            }

            // Make sure A didn't change.
            for (var i = 0; i < matrixA.RowCount; i++)
            {
                for (var j = 0; j < matrixA.ColumnCount; j++)
                {
                    Assert.AreEqual(matrixACopy[i, j], matrixA[i, j]);
                }
            }

            // Make sure b didn't change.
            for (var i = 0; i < vectorb.Count; i++)
            {
                Assert.AreEqual(vectorbCopy[i], vectorb[i]);
            }
        }

        /// <summary>
        /// Can solve a system of linear equations for a random matrix (AX=B) into a result matrix.
        /// </summary>
        /// <param name="row">Matrix row number.</param>
        /// <param name="column">Matrix column number.</param>
        [Test, Sequential]
        public void CanSolveForRandomMatrixWhenResultMatrixGiven([Values(1, 4, 7, 10, 45, 80)] int row, [Values(1, 4, 8, 10, 50, 100)] int column)
        {
            var matrixA = MatrixLoader.GenerateRandomUserDefinedMatrix(row, column);
            var matrixACopy = matrixA.Clone();
            var factorSvd = matrixA.Svd(true);

            var matrixB = MatrixLoader.GenerateRandomUserDefinedMatrix(row, column);
            var matrixBCopy = matrixB.Clone();

            var matrixX = new UserDefinedMatrix(column, column);
            factorSvd.Solve(matrixB, matrixX);

            // The solution X row dimension is equal to the column dimension of A
            Assert.AreEqual(matrixA.ColumnCount, matrixX.RowCount);

            // The solution X has the same number of columns as B
            Assert.AreEqual(matrixB.ColumnCount, matrixX.ColumnCount);

            var matrixBReconstruct = matrixA * matrixX;

            // Check the reconstruction.
            for (var i = 0; i < matrixB.RowCount; i++)
            {
                for (var j = 0; j < matrixB.ColumnCount; j++)
                {
                    AssertHelpers.AlmostEqual(matrixB[i, j], matrixBReconstruct[i, j], 9);
                }
            }

            // Make sure A didn't change.
            for (var i = 0; i < matrixA.RowCount; i++)
            {
                for (var j = 0; j < matrixA.ColumnCount; j++)
                {
                    Assert.AreEqual(matrixACopy[i, j], matrixA[i, j]);
                }
            }

            // Make sure B didn't change.
            for (var i = 0; i < matrixB.RowCount; i++)
            {
                for (var j = 0; j < matrixB.ColumnCount; j++)
                {
                    Assert.AreEqual(matrixBCopy[i, j], matrixB[i, j]);
                }
            }
        }
    }
}
