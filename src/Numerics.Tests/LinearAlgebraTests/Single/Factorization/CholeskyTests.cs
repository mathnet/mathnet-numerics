// <copyright file="CholeskyTests.cs" company="Math.NET">
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

using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Single;
using NUnit.Framework;

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Single.Factorization
{
    /// <summary>
    /// Cholesky factorization tests for a dense matrix.
    /// </summary>
    [TestFixture, Category("LAFactorization")]
    public class CholeskyTests
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
            var matrixI = DenseMatrix.CreateIdentity(order);
            var factorC = matrixI.Cholesky().Factor;

            Assert.AreEqual(matrixI.RowCount, factorC.RowCount);
            Assert.AreEqual(matrixI.ColumnCount, factorC.ColumnCount);

            for (var i = 0; i < factorC.RowCount; i++)
            {
                for (var j = 0; j < factorC.ColumnCount; j++)
                {
                    Assert.AreEqual(i == j ? 1.0 : 0.0, factorC[i, j]);
                }
            }
        }

        /// <summary>
        /// Cholesky factorization fails with diagonal a non-positive definite matrix.
        /// </summary>
        [Test]
        public void CholeskyFailsWithDiagonalNonPositiveDefiniteMatrix()
        {
            var matrixI = DenseMatrix.CreateIdentity(10);
            matrixI[3, 3] = -4.0f;
            Assert.That(() => matrixI.Cholesky(), Throws.ArgumentException);
        }

        /// <summary>
        /// Cholesky factorization fails with a non-square matrix.
        /// </summary>
        [Test]
        public void CholeskyFailsWithNonSquareMatrix()
        {
            var matrix = new DenseMatrix(3, 2);
            Assert.That(() => matrix.Cholesky(), Throws.ArgumentException);
        }

        /// <summary>
        /// Identity determinant is one.
        /// </summary>
        /// <param name="order">Matrix order.</param>
        [TestCase(1)]
        [TestCase(10)]
        [TestCase(100)]
        public void IdentityDeterminantIsOne(int order)
        {
            var matrixI = DenseMatrix.CreateIdentity(order);
            var factorC = matrixI.Cholesky();
            Assert.AreEqual(1.0, factorC.Determinant);
            Assert.AreEqual(0.0, factorC.DeterminantLn);
        }

        /// <summary>
        /// Can factorize a random square matrix.
        /// </summary>
        /// <param name="order">Matrix order.</param>
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(5)]
        [TestCase(10)]
        [TestCase(50)]
        [TestCase(100)]
        public void CanFactorizeRandomMatrix(int order)
        {
            var matrixX = Matrix<float>.Build.RandomPositiveDefinite(order, 1);
            var chol = matrixX.Cholesky();
            var factorC = chol.Factor;

            // Make sure the Cholesky factor has the right dimensions.
            Assert.AreEqual(order, factorC.RowCount);
            Assert.AreEqual(order, factorC.ColumnCount);

            // Make sure the Cholesky factor is lower triangular.
            for (var i = 0; i < factorC.RowCount; i++)
            {
                for (var j = i + 1; j < factorC.ColumnCount; j++)
                {
                    Assert.AreEqual(0.0, factorC[i, j]);
                }
            }

            // Make sure the cholesky factor times it's transpose is the original matrix.
            var matrixXfromC = factorC * factorC.Transpose();
            for (var i = 0; i < matrixXfromC.RowCount; i++)
            {
                for (var j = 0; j < matrixXfromC.ColumnCount; j++)
                {
                    Assert.AreEqual(matrixX[i, j], matrixXfromC[i, j], 1e-3);
                }
            }

            // Check update
            var matrixC = Matrix<float>.Build.RandomPositiveDefinite(order, 1);
            var cholC = matrixC.Cholesky();
            chol.Factorize(matrixC);
            for (var i = 0; i < matrixC.RowCount; i++)
            {
                for (var j = 0; j < matrixC.ColumnCount; j++)
                {
                    Assert.AreEqual(cholC.Factor[i, j], chol.Factor[i, j]);
                }
            }

            // Check size mismatch
            var matrixD = Matrix<float>.Build.DenseIdentity(order + 1);
            Assert.That(() => chol.Factorize(matrixD), Throws.ArgumentException);
        }

        /// <summary>
        /// Can solve a system of linear equations for a random vector (Ax=b).
        /// </summary>
        /// <param name="order">Matrix order.</param>
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(5)]
        [TestCase(10)]
        [TestCase(50)]
        [TestCase(100)]
        public void CanSolveForRandomVector(int order)
        {
            var matrixA = Matrix<float>.Build.RandomPositiveDefinite(order, 1);
            var matrixACopy = matrixA.Clone();
            var chol = matrixA.Cholesky();
            var matrixB = Vector<float>.Build.Random(order, 1);
            var x = chol.Solve(matrixB);

            Assert.AreEqual(matrixB.Count, x.Count);

            var matrixBReconstruct = matrixA * x;

            // Check the reconstruction.
            for (var i = 0; i < order; i++)
            {
                Assert.AreEqual(matrixB[i], matrixBReconstruct[i], 0.5f);
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
        /// <param name="col">Matrix column number.</param>
        [TestCase(1, 1)]
        [TestCase(2, 4)]
        [TestCase(5, 8)]
        [TestCase(10, 3)]
        [TestCase(50, 10)]
        [TestCase(100, 100)]
        public void CanSolveForRandomMatrix(int row, int col)
        {
            var matrixA = Matrix<float>.Build.RandomPositiveDefinite(row, 1);
            var matrixACopy = matrixA.Clone();
            var chol = matrixA.Cholesky();
            var matrixB = Matrix<float>.Build.Random(row, col, 1);
            var matrixX = chol.Solve(matrixB);

            Assert.AreEqual(matrixB.RowCount, matrixX.RowCount);
            Assert.AreEqual(matrixB.ColumnCount, matrixX.ColumnCount);

            var matrixBReconstruct = matrixA * matrixX;

            // Check the reconstruction.
            for (var i = 0; i < matrixB.RowCount; i++)
            {
                for (var j = 0; j < matrixB.ColumnCount; j++)
                {
                    Assert.AreEqual(matrixB[i, j], matrixBReconstruct[i, j], 1f);
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
        /// <param name="order">Matrix order.</param>
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(5)]
        [TestCase(10)]
        [TestCase(50)]
        [TestCase(100)]
        public void CanSolveForRandomVectorWhenResultVectorGiven(int order)
        {
            var matrixA = Matrix<float>.Build.RandomPositiveDefinite(order, 1);
            var matrixACopy = matrixA.Clone();
            var chol = matrixA.Cholesky();
            var matrixB = Vector<float>.Build.Random(order, 1);
            var matrixBCopy = matrixB.Clone();
            var x = new DenseVector(order);
            chol.Solve(matrixB, x);

            Assert.AreEqual(matrixB.Count, x.Count);

            var matrixBReconstruct = matrixA * x;

            // Check the reconstruction.
            for (var i = 0; i < order; i++)
            {
                Assert.AreEqual(matrixB[i], matrixBReconstruct[i], 0.5f);
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
            for (var i = 0; i < order; i++)
            {
                Assert.AreEqual(matrixBCopy[i], matrixB[i]);
            }
        }

        /// <summary>
        /// Can solve a system of linear equations for a random matrix (AX=B) into a result matrix.
        /// </summary>
        /// <param name="row">Matrix row number.</param>
        /// <param name="col">Matrix column number.</param>
        [TestCase(1, 1)]
        [TestCase(2, 4)]
        [TestCase(5, 8)]
        [TestCase(10, 3)]
        [TestCase(50, 10)]
        [TestCase(100, 100)]
        public void CanSolveForRandomMatrixWhenResultMatrixGiven(int row, int col)
        {
            var matrixA = Matrix<float>.Build.RandomPositiveDefinite(row, 1);
            var matrixACopy = matrixA.Clone();
            var chol = matrixA.Cholesky();
            var matrixB = Matrix<float>.Build.Random(row, col, 1);
            var matrixBCopy = matrixB.Clone();
            var matrixX = new DenseMatrix(row, col);
            chol.Solve(matrixB, matrixX);

            Assert.AreEqual(matrixB.RowCount, matrixX.RowCount);
            Assert.AreEqual(matrixB.ColumnCount, matrixX.ColumnCount);

            var matrixBReconstruct = matrixA * matrixX;

            // Check the reconstruction.
            for (var i = 0; i < matrixB.RowCount; i++)
            {
                for (var j = 0; j < matrixB.ColumnCount; j++)
                {
                    Assert.AreEqual(matrixB[i, j], matrixBReconstruct[i, j], 1f);
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
