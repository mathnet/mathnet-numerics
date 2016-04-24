// <copyright file="EvdTests.cs" company="Math.NET">
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
#if NOSYSNUMERICS
    using Complex = Numerics.Complex;
#else
    using Complex = System.Numerics.Complex;
#endif

    /// <summary>
    /// Eigenvalues factorization tests for a dense matrix.
    /// </summary>
    [TestFixture, Category("LAFactorization")]
    public class EvdTests
    {
        [Test]
        public void CanFactorizeIdentityMatrix([Values(1, 10, 100)] int order)
        {
            var matrix = Matrix<float>.Build.DenseIdentity(order);
            var factorEvd = matrix.Evd();
            var eigenValues = factorEvd.EigenValues;
            var eigenVectors = factorEvd.EigenVectors;
            var d = factorEvd.D;

            Assert.AreEqual(matrix.RowCount, eigenVectors.RowCount);
            Assert.AreEqual(matrix.RowCount, eigenVectors.ColumnCount);
            Assert.AreEqual(matrix.ColumnCount, d.RowCount);
            Assert.AreEqual(matrix.ColumnCount, d.ColumnCount);

            for (var i = 0; i < eigenValues.Count; i++)
            {
                Assert.AreEqual(Complex.One, eigenValues[i]);
            }
        }

        [Test]
        public void CanFactorizeRandomSquareMatrix([Values(1, 2, 5, 10, 50, 100)] int order)
        {
            var A = Matrix<float>.Build.Random(order, order, 1);
            var factorEvd = A.Evd();
            var V = factorEvd.EigenVectors;
            var λ = factorEvd.D;

            Assert.AreEqual(order, V.RowCount);
            Assert.AreEqual(order, V.ColumnCount);
            Assert.AreEqual(order, λ.RowCount);
            Assert.AreEqual(order, λ.ColumnCount);

            // Verify A*V = λ*V
            var Av = A * V;
            var Lv = V * λ;
            AssertHelpers.AlmostEqual(Av, Lv, 3);
        }

        [Test]
        public void CanFactorizeRandomSymmetricMatrix([Values(1, 2, 5, 10, 50, 100)] int order)
        {
            var A = Matrix<float>.Build.RandomPositiveDefinite(order, 1);
            MatrixHelpers.ForceSymmetric(A);
            var factorEvd = A.Evd();
            var V = factorEvd.EigenVectors;
            var λ = factorEvd.D;

            Assert.AreEqual(order, V.RowCount);
            Assert.AreEqual(order, V.ColumnCount);
            Assert.AreEqual(order, λ.RowCount);
            Assert.AreEqual(order, λ.ColumnCount);

            // Verify A = V*λ*VT
            var matrix = V * λ * V.Transpose();
            AssertHelpers.AlmostEqual(matrix, A, 2);
            AssertHelpers.AlmostEqualRelative(matrix, A, 0);
        }

        [Test]
        public void CanCheckRankSquare([Values(10, 50, 100)] int order)
        {
            var A = Matrix<float>.Build.Random(order, order, 1);
            Assert.AreEqual(A.Evd().Rank, order);
        }

        [Test]
        public void CanCheckRankOfSquareSingular([Values(10, 50, 100)] int order)
        {
            var A = new DenseMatrix(order, order);
            A[0, 0] = 1;
            A[order - 1, order - 1] = 1;
            for (var i = 1; i < order - 1; i++)
            {
                A[i, i - 1] = 1;
                A[i, i + 1] = 1;
                A[i - 1, i] = 1;
                A[i + 1, i] = 1;
            }
            var factorEvd = A.Evd();

            Assert.AreEqual(factorEvd.Determinant, 0);
            Assert.AreEqual(factorEvd.Rank, order - 1);
        }

        [Test]
        public void IdentityDeterminantIsOne([Values(1, 10, 100)] int order)
        {
            var matrixI = DenseMatrix.CreateIdentity(order);
            var factorEvd = matrixI.Evd();
            Assert.AreEqual(1.0, factorEvd.Determinant);
        }

        /// <summary>
        /// Can solve a system of linear equations for a random vector and symmetric matrix (Ax=b).
        /// </summary>
        /// <param name="order">Matrix order.</param>
        [Test]
        public void CanSolveForRandomVectorAndSymmetricMatrix([Values(1, 2, 5, 10, 50, 100)] int order)
        {
            var A = Matrix<float>.Build.RandomPositiveDefinite(order, 1);
            MatrixHelpers.ForceSymmetric(A);
            var ACopy = A.Clone();
            var evd = A.Evd();

            var b = Vector<float>.Build.Random(order, 2);
            var bCopy = b.Clone();

            var x = evd.Solve(b);

            var bReconstruct = A * x;

            // Check the reconstruction.
            AssertHelpers.AlmostEqual(b, bReconstruct, -1);

            // Make sure A/B didn't change.
            AssertHelpers.AlmostEqual(ACopy, A, 14);
            AssertHelpers.AlmostEqual(bCopy, b, 14);
        }

        /// <summary>
        /// Can solve a system of linear equations for a random matrix and symmetric matrix (AX=B).
        /// </summary>
        /// <param name="order">Matrix order.</param>
        [Test]
        public void CanSolveForRandomMatrixAndSymmetricMatrix([Values(1, 2, 5, 10, 50, 100)] int order)
        {
            var A = Matrix<float>.Build.RandomPositiveDefinite(order, 1);
            MatrixHelpers.ForceSymmetric(A);
            var ACopy = A.Clone();
            var evd = A.Evd();

            var B = Matrix<float>.Build.Random(order, order, 2);
            var BCopy = B.Clone();

            var X = evd.Solve(B);

            // The solution X row dimension is equal to the column dimension of A
            Assert.AreEqual(A.ColumnCount, X.RowCount);

            // The solution X has the same number of columns as B
            Assert.AreEqual(B.ColumnCount, X.ColumnCount);

            var BReconstruct = A * X;

            // Check the reconstruction.
            AssertHelpers.AlmostEqual(B, BReconstruct, -1);

            // Make sure A/B didn't change.
            AssertHelpers.AlmostEqual(ACopy, A, 14);
            AssertHelpers.AlmostEqual(BCopy, B, 14);
        }

        /// <summary>
        /// Can solve a system of linear equations for a random vector and symmetric matrix (Ax=b) into a result vector.
        /// </summary>
        /// <param name="order">Matrix order.</param>
        [Test]
        public void CanSolveForRandomVectorAndSymmetricMatrixWhenResultVectorGiven([Values(1, 2, 5, 10, 50, 100)] int order)
        {
            var A = Matrix<float>.Build.RandomPositiveDefinite(order, 1);
            MatrixHelpers.ForceSymmetric(A);
            var ACopy = A.Clone();
            var evd = A.Evd();

            var b = Vector<float>.Build.Random(order, 2);
            var bCopy = b.Clone();

            var x = new DenseVector(order);
            evd.Solve(b, x);

            var bReconstruct = A * x;

            // Check the reconstruction.
            AssertHelpers.AlmostEqual(b, bReconstruct, -1);

            // Make sure A/B didn't change.
            AssertHelpers.AlmostEqual(ACopy, A, 14);
            AssertHelpers.AlmostEqual(bCopy, b, 14);
        }

        /// <summary>
        /// Can solve a system of linear equations for a random matrix and symmetric matrix (AX=B) into result matrix.
        /// </summary>
        /// <param name="order">Matrix order.</param>
        [Test]
        public void CanSolveForRandomMatrixAndSymmetricMatrixWhenResultMatrixGiven([Values(1, 2, 5, 10, 50, 100)] int order)
        {
            var A = Matrix<float>.Build.RandomPositiveDefinite(order, 1);
            MatrixHelpers.ForceSymmetric(A);
            var ACopy = A.Clone();
            var evd = A.Evd();

            var B = Matrix<float>.Build.Random(order, order, 2);
            var BCopy = B.Clone();

            var X = new DenseMatrix(order, order);
            evd.Solve(B, X);

            // The solution X row dimension is equal to the column dimension of A
            Assert.AreEqual(A.ColumnCount, X.RowCount);

            // The solution X has the same number of columns as B
            Assert.AreEqual(B.ColumnCount, X.ColumnCount);

            var BReconstruct = A * X;

            // Check the reconstruction.
            AssertHelpers.AlmostEqual(B, BReconstruct, -1);

            // Make sure A/B didn't change.
            AssertHelpers.AlmostEqual(ACopy, A, 14);
            AssertHelpers.AlmostEqual(BCopy, B, 14);
        }
    }
}
