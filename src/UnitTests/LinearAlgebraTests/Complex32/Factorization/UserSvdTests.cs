// <copyright file="UserSvdTests.cs" company="Math.NET">
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
using MathNet.Numerics.LinearAlgebra;
using NUnit.Framework;

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Complex32.Factorization
{
    using Numerics;

    /// <summary>
    /// Svd factorization tests for a user matrix.
    /// </summary>
    [TestFixture, Category("LAFactorization")]
    public class UserSvdTests
    {
        /// <summary>
        /// Can factorize identity matrix.
        /// </summary>
        /// <param name="order">Matrix order.</param>
        [TestCase(1)]
        [TestCase(10)]
        [TestCase(100)]
        public void CanFactorizeIdentity(int order)
        {
            var matrixI = UserDefinedMatrix.Identity(order);
            var factorSvd = matrixI.Svd();
            var u = factorSvd.U;
            var vt = factorSvd.VT;
            var w = factorSvd.W;

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
                    Assert.AreEqual(i == j ? Complex32.One : Complex32.Zero, w[i, j]);
                }
            }
        }

        /// <summary>
        /// Can factorize a random matrix.
        /// </summary>
        /// <param name="row">Matrix row number.</param>
        /// <param name="column">Matrix column number.</param>
        [TestCase(1, 1)]
        [TestCase(2, 2)]
        [TestCase(5, 5)]
        [TestCase(10, 6)]
        [TestCase(50, 48)]
        [TestCase(100, 98)]
        public void CanFactorizeRandomMatrix(int row, int column)
        {
            var matrixA = new UserDefinedMatrix(Matrix<Complex32>.Build.Random(row, column, 1).ToArray());
            var factorSvd = matrixA.Svd();
            var u = factorSvd.U;
            var vt = factorSvd.VT;
            var w = factorSvd.W;

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
            var matrix = u*w*vt;
            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    Assert.AreEqual(matrixA[i, j].Real, matrix[i, j].Real, 1e-3f);
                    Assert.AreEqual(matrixA[i, j].Imaginary, matrix[i, j].Imaginary, 1e-3f);
                }
            }
        }

        /// <summary>
        /// Can check rank of a non-square matrix.
        /// </summary>
        /// <param name="row">Matrix row number.</param>
        /// <param name="column">Matrix column number.</param>
        [TestCase(10, 8)]
        [TestCase(48, 52)]
        [TestCase(100, 93)]
        public void CanCheckRankOfNonSquare(int row, int column)
        {
            var matrixA = new UserDefinedMatrix(Matrix<Complex32>.Build.Random(row, column, 1).ToArray());
            var factorSvd = matrixA.Svd();

            var mn = Math.Min(row, column);
            Assert.AreEqual(factorSvd.Rank, mn);
        }

        /// <summary>
        /// Can check rank of a square matrix.
        /// </summary>
        /// <param name="order">Matrix order.</param>
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(5)]
        [TestCase(9)]
        [TestCase(50)]
        [TestCase(90)]
        public void CanCheckRankSquare(int order)
        {
            var matrixA = new UserDefinedMatrix(Matrix<Complex32>.Build.Random(order, order, 1).ToArray());
            var factorSvd = matrixA.Svd();

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
        [TestCase(10)]
        [TestCase(50)]
        [TestCase(100)]
        public void CanCheckRankOfSquareSingular(int order)
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

            var factorSvd = matrixA.Svd();

            Assert.AreEqual(factorSvd.Determinant, Complex32.Zero);
            Assert.AreEqual(factorSvd.Rank, order - 1);
        }

        /// <summary>
        /// Solve for matrix if vectors are not computed throws <c>InvalidOperationException</c>.
        /// </summary>
        [Test]
        public void SolveMatrixIfVectorsNotComputedThrowsInvalidOperationException()
        {
            var matrixA = new UserDefinedMatrix(Matrix<Complex32>.Build.Random(10, 3, 1).ToArray());
            var factorSvd = matrixA.Svd(false);

            var matrixB = new UserDefinedMatrix(Matrix<Complex32>.Build.Random(10, 3, 1).ToArray());
            Assert.That(() => factorSvd.Solve(matrixB), Throws.InvalidOperationException);
        }

        /// <summary>
        /// Solve for vector if vectors are not computed throws <c>InvalidOperationException</c>.
        /// </summary>
        [Test]
        public void SolveVectorIfVectorsNotComputedThrowsInvalidOperationException()
        {
            var matrixA = new UserDefinedMatrix(Matrix<Complex32>.Build.Random(10, 3, 1).ToArray());
            var factorSvd = matrixA.Svd(false);

            var vectorb = new UserDefinedVector(Vector<Complex32>.Build.Random(3, 1).ToArray());
            Assert.That(() => factorSvd.Solve(vectorb), Throws.InvalidOperationException);
        }

        /// <summary>
        /// Can solve a system of linear equations for a random vector (Ax=b).
        /// </summary>
        /// <param name="row">Matrix row number.</param>
        /// <param name="column">Matrix column number.</param>
        [TestCase(1, 1)]
        [TestCase(2, 2)]
        [TestCase(5, 5)]
        [TestCase(9, 10)]
        [TestCase(50, 50)]
        [TestCase(90, 100)]
        public void CanSolveForRandomVector(int row, int column)
        {
            var matrixA = new UserDefinedMatrix(Matrix<Complex32>.Build.Random(row, column, 1).ToArray());
            var matrixACopy = matrixA.Clone();
            var factorSvd = matrixA.Svd();

            var vectorb = new UserDefinedVector(Vector<Complex32>.Build.Random(row, 1).ToArray());
            var resultx = factorSvd.Solve(vectorb);

            Assert.AreEqual(matrixA.ColumnCount, resultx.Count);

            var matrixBReconstruct = matrixA*resultx;

            // Check the reconstruction.
            for (var i = 0; i < vectorb.Count; i++)
            {
                Assert.AreEqual(vectorb[i].Real, matrixBReconstruct[i].Real, 1e-3f);
                Assert.AreEqual(vectorb[i].Imaginary, matrixBReconstruct[i].Imaginary, 1e-3f);
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
        [TestCase(1, 1)]
        [TestCase(4, 4)]
        [TestCase(7, 8)]
        [TestCase(10, 10)]
        [TestCase(45, 50)]
        [TestCase(80, 100)]
        public void CanSolveForRandomMatrix(int row, int column)
        {
            var matrixA = new UserDefinedMatrix(Matrix<Complex32>.Build.Random(row, column, 1).ToArray());
            var matrixACopy = matrixA.Clone();
            var factorSvd = matrixA.Svd();

            var matrixB = new UserDefinedMatrix(Matrix<Complex32>.Build.Random(row, column, 1).ToArray());
            var matrixX = factorSvd.Solve(matrixB);

            // The solution X row dimension is equal to the column dimension of A
            Assert.AreEqual(matrixA.ColumnCount, matrixX.RowCount);

            // The solution X has the same number of columns as B
            Assert.AreEqual(matrixB.ColumnCount, matrixX.ColumnCount);

            var matrixBReconstruct = matrixA*matrixX;

            // Check the reconstruction.
            for (var i = 0; i < matrixB.RowCount; i++)
            {
                for (var j = 0; j < matrixB.ColumnCount; j++)
                {
                    Assert.AreEqual(matrixB[i, j].Real, matrixBReconstruct[i, j].Real, 1e-3f);
                    Assert.AreEqual(matrixB[i, j].Imaginary, matrixBReconstruct[i, j].Imaginary, 1e-3f);
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
        [TestCase(1, 1)]
        [TestCase(2, 2)]
        [TestCase(5, 5)]
        [TestCase(9, 10)]
        [TestCase(50, 50)]
        [TestCase(90, 100)]
        public void CanSolveForRandomVectorWhenResultVectorGiven(int row, int column)
        {
            var matrixA = new UserDefinedMatrix(Matrix<Complex32>.Build.Random(row, column, 1).ToArray());
            var matrixACopy = matrixA.Clone();
            var factorSvd = matrixA.Svd();
            var vectorb = new UserDefinedVector(Vector<Complex32>.Build.Random(row, 1).ToArray());
            var vectorbCopy = vectorb.Clone();
            var resultx = new UserDefinedVector(column);
            factorSvd.Solve(vectorb, resultx);

            var matrixBReconstruct = matrixA*resultx;

            // Check the reconstruction.
            for (var i = 0; i < vectorb.Count; i++)
            {
                Assert.AreEqual(vectorb[i].Real, matrixBReconstruct[i].Real, 1e-3f);
                Assert.AreEqual(vectorb[i].Imaginary, matrixBReconstruct[i].Imaginary, 1e-3f);
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
        [TestCase(1, 1)]
        [TestCase(4, 4)]
        [TestCase(7, 8)]
        [TestCase(10, 10)]
        [TestCase(45, 50)]
        [TestCase(80, 100)]
        public void CanSolveForRandomMatrixWhenResultMatrixGiven(int row, int column)
        {
            var matrixA = new UserDefinedMatrix(Matrix<Complex32>.Build.Random(row, column, 1).ToArray());
            var matrixACopy = matrixA.Clone();
            var factorSvd = matrixA.Svd();

            var matrixB = new UserDefinedMatrix(Matrix<Complex32>.Build.Random(row, column, 1).ToArray());
            var matrixBCopy = matrixB.Clone();

            var matrixX = new UserDefinedMatrix(column, column);
            factorSvd.Solve(matrixB, matrixX);

            // The solution X row dimension is equal to the column dimension of A
            Assert.AreEqual(matrixA.ColumnCount, matrixX.RowCount);

            // The solution X has the same number of columns as B
            Assert.AreEqual(matrixB.ColumnCount, matrixX.ColumnCount);

            var matrixBReconstruct = matrixA*matrixX;

            // Check the reconstruction.
            for (var i = 0; i < matrixB.RowCount; i++)
            {
                for (var j = 0; j < matrixB.ColumnCount; j++)
                {
                    Assert.AreEqual(matrixB[i, j].Real, matrixBReconstruct[i, j].Real, 1e-3f);
                    Assert.AreEqual(matrixB[i, j].Imaginary, matrixBReconstruct[i, j].Imaginary, 1e-3f);
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
