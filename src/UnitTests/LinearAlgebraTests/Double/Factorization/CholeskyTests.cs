// <copyright file="CholeskyTests.cs" company="Math.NET">
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

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Double.Factorization
{
    using System.Collections.Generic;
    using MbUnit.Framework;
    using LinearAlgebra.Double;
    using LinearAlgebra.Double.Factorization;

    public class CholeskyTests
    {
        [Test]
        [Row(1)]
        [Row(10)]
        [Row(100)]
        public void CanFactorizeIdentity(int order)
        {
            var I = DenseMatrix.Identity(order);
            var C = I.Cholesky();

            for (var i = 0; i < C.Factor.RowCount; i++)
            {
                for (var j = 0; j < C.Factor.ColumnCount; j++)
                {
                    if (i == j)
                    {
                        Assert.AreEqual(1.0, C.Factor[i, j]);
                    }
                    else
                    {
                        Assert.AreEqual(0.0, C.Factor[i, j]);
                    }
                }
            }
        }

        [Test]
        [ExpectedArgumentException]
        public void CholeskyFailsWithDiagonalNonPositiveDefiniteMatrix()
        {
            var I = DenseMatrix.Identity(10);
            I[3, 3] = -4.0;
            var C = I.Cholesky();
        }

        [Test]
        [Row(3,5)]
        [Row(5,3)]
        [ExpectedArgumentException]
        public void CholeskyFailsWithNonSquareMatrix(int row, int col)
        {
            var I = new DenseMatrix(row, col);
            var C = I.Cholesky();
        }

        [Test]
        [Row(1)]
        [Row(10)]
        [Row(100)]
        public void IdentityDeterminantIsOne(int order)
        {
            var I = DenseMatrix.Identity(order);
            var C = I.Cholesky();
            Assert.AreEqual(1.0, C.Determinant);
            Assert.AreEqual(0.0, C.DeterminantLn);
        }

        [Test]
        [Row(1)]
        [Row(2)]
        [Row(5)]
        [Row(10)]
        [Row(50)]
        [Row(100)]
        [MultipleAsserts]
        public void CanFactorizeRandomMatrix(int order)
        {
            var X = MatrixLoader.GenerateRandomPositiveDefiniteMatrix(order);
            var chol = X.Cholesky();
            var C = chol.Factor;

            // Make sure the Cholesky factor has the right dimensions.
            Assert.AreEqual(order, C.RowCount);
            Assert.AreEqual(order, C.ColumnCount);

            // Make sure the Cholesky factor is lower triangular.
            for (int i = 0; i < C.RowCount; i++) 
            {
                for (int j = i+1; j < C.ColumnCount; j++)
                {
                    Assert.AreEqual(0.0, C[i, j]);
                }
            }

            // Make sure the cholesky factor times it's transpose is the original matrix.
            var XfromC = C * C.Transpose();
            for (int i = 0; i < XfromC.RowCount; i++) 
            {
                for (int j = 0; j < XfromC.ColumnCount; j++)
                {
                    Assert.AreApproximatelyEqual(X[i,j], XfromC[i, j], 1.0e-11);
                }
            }
        }

        [Test]
        [Row(1)]
        [Row(2)]
        [Row(5)]
        [Row(10)]
        [Row(50)]
        [Row(100)]
        [MultipleAsserts]
        public void CanSolveForRandomVector(int order)
        {
            var A = MatrixLoader.GenerateRandomPositiveDefiniteMatrix(order);
            var ACopy = A.Clone();
            var chol = A.Cholesky();
            var b = MatrixLoader.GenerateRandomVector(order);
            var x = chol.Solve(b);

            Assert.AreEqual(b.Count, x.Count);

            var bReconstruct = A * x;

            // Check the reconstruction.
            for (int i = 0; i < order; i++)
            {
                Assert.AreApproximatelyEqual(b[i], bReconstruct[i], 1.0e-11);
            }

            // Make sure A didn't change.
            for (int i = 0; i < A.RowCount; i++)
            {
                for (int j = 0; j < A.ColumnCount; j++)
                {
                    Assert.AreEqual(ACopy[i, j], A[i, j]);
                }
            }
        }

        [Test]
        [Row(1,1)]
        [Row(2,4)]
        [Row(5,8)]
        [Row(10,3)]
        [Row(50,10)]
        [Row(100,100)]
        [MultipleAsserts]
        public void CanSolveForRandomMatrix(int row, int col)
        {
            var A = MatrixLoader.GenerateRandomPositiveDefiniteMatrix(row);
            var ACopy = A.Clone();
            var chol = A.Cholesky();
            var B = MatrixLoader.GenerateRandomMatrix(row, col);
            var X = chol.Solve(B);

            Assert.AreEqual(B.RowCount, X.RowCount);
            Assert.AreEqual(B.ColumnCount, X.ColumnCount);

            var BReconstruct = A * X;

            // Check the reconstruction.
            for (int i = 0; i < B.RowCount; i++)
            {
                for (int j = 0; j < B.ColumnCount; j++)
                {
                    Assert.AreApproximatelyEqual(B[i, j], BReconstruct[i, j], 1.0e-11);
                }
            }

            // Make sure A didn't change.
            for (int i = 0; i < A.RowCount; i++)
            {
                for (int j = 0; j < A.ColumnCount; j++)
                {
                    Assert.AreEqual(ACopy[i, j], A[i, j]);
                }
            }
        }

        [Test]
        [Row(1)]
        [Row(2)]
        [Row(5)]
        [Row(10)]
        [Row(50)]
        [Row(100)]
        [MultipleAsserts]
        public void CanSolveForRandomVectorWhenResultVectorGiven(int order)
        {
            var A = MatrixLoader.GenerateRandomPositiveDefiniteMatrix(order);
            var ACopy = A.Clone();
            var chol = A.Cholesky();
            var b = MatrixLoader.GenerateRandomVector(order);
            var bCopy = b.Clone();
            var x = new DenseVector(order);
            chol.Solve(b, x);

            Assert.AreEqual(b.Count, x.Count);

            var bReconstruct = A * x;

            // Check the reconstruction.
            for (int i = 0; i < order; i++)
            {
                Assert.AreApproximatelyEqual(b[i], bReconstruct[i], 1.0e-11);
            }

            // Make sure A didn't change.
            for (int i = 0; i < A.RowCount; i++)
            {
                for (int j = 0; j < A.ColumnCount; j++)
                {
                    Assert.AreEqual(ACopy[i, j], A[i, j]);
                }
            }

            // Make sure b didn't change.
            for (int i = 0; i < order; i++)
            {
                Assert.AreEqual(bCopy[i], b[i]);
            }
        }

        [Test]
        [Row(1, 1)]
        [Row(2, 4)]
        [Row(5, 8)]
        [Row(10, 3)]
        [Row(50, 10)]
        [Row(100, 100)]
        [MultipleAsserts]
        public void CanSolveForRandomMatrixWhenResultMatrixGiven(int row, int col)
        {
            var A = MatrixLoader.GenerateRandomPositiveDefiniteMatrix(row);
            var ACopy = A.Clone();
            var chol = A.Cholesky();
            var B = MatrixLoader.GenerateRandomMatrix(row, col);
            var BCopy = B.Clone();
            var X = new DenseMatrix(row, col);
            chol.Solve(B, X);

            Assert.AreEqual(B.RowCount, X.RowCount);
            Assert.AreEqual(B.ColumnCount, X.ColumnCount);

            var BReconstruct = A * X;

            // Check the reconstruction.
            for (int i = 0; i < B.RowCount; i++)
            {
                for (int j = 0; j < B.ColumnCount; j++)
                {
                    Assert.AreApproximatelyEqual(B[i, j], BReconstruct[i, j], 1.0e-11);
                }
            }

            // Make sure A didn't change.
            for (int i = 0; i < A.RowCount; i++)
            {
                for (int j = 0; j < A.ColumnCount; j++)
                {
                    Assert.AreEqual(ACopy[i, j], A[i, j]);
                }
            }

            // Make sure B didn't change.
            for (int i = 0; i < B.RowCount; i++)
            {
                for (int j = 0; j < B.ColumnCount; j++)
                {
                    Assert.AreEqual(BCopy[i, j], B[i, j]);
                }
            }
        }
    }
}
