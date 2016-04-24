// <copyright file="QRTests.cs" company="Math.NET">
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
using MathNet.Numerics.LinearAlgebra.Factorization;
using MathNet.Numerics.LinearAlgebra.Single;
using MathNet.Numerics.LinearAlgebra.Single.Factorization;
using NUnit.Framework;

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Single.Factorization
{
    /// <summary>
    /// QR factorization tests for a dense matrix.
    /// </summary>
    [TestFixture, Category("LAFactorization")]
    public class QRTests
    {
        /// <summary>
        /// Constructor with wide matrix throws <c>ArgumentException</c>.
        /// </summary>
        [Test]
        public void ConstructorWideMatrixThrowsInvalidMatrixOperationException()
        {
            Assert.That(() => UserQR.Create(new DenseMatrix(3, 4)), Throws.ArgumentException);
        }

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
            var factorQR = matrixI.QR();
            var r = factorQR.R;

            Assert.AreEqual(matrixI.RowCount, r.RowCount);
            Assert.AreEqual(matrixI.ColumnCount, r.ColumnCount);

            for (var i = 0; i < r.RowCount; i++)
            {
                for (var j = 0; j < r.ColumnCount; j++)
                {
                    if (i == j)
                    {
                        Assert.AreEqual(1.0, Math.Abs(r[i, j]));
                    }
                    else
                    {
                        Assert.AreEqual(0.0, r[i, j]);
                    }
                }
            }
        }

        /// <summary>
        /// Can factorize identity matrix using thin QR.
        /// </summary>
        /// <param name="order">Matrix order.</param>
        [TestCase(1)]
        [TestCase(10)]
        [TestCase(100)]
        public void CanFactorizeIdentityUsingThinQR(int order)
        {
            var matrixI = DenseMatrix.CreateIdentity(order);
            var factorQR = matrixI.QR(QRMethod.Thin);
            var r = factorQR.R;

            Assert.AreEqual(matrixI.ColumnCount, r.RowCount);
            Assert.AreEqual(matrixI.ColumnCount, r.ColumnCount);

            for (var i = 0; i < r.RowCount; i++)
            {
                for (var j = 0; j < r.ColumnCount; j++)
                {
                    if (i == j)
                    {
                        Assert.AreEqual(1.0, Math.Abs(r[i, j]));
                    }
                    else
                    {
                        Assert.AreEqual(0.0, r[i, j]);
                    }
                }
            }
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
            var factorQR = matrixI.QR();
            Assert.AreEqual(1.0, factorQR.Determinant);
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
            var matrixA = Matrix<float>.Build.Random(row, column, 1);
            var factorQR = matrixA.QR(QRMethod.Full);
            var q = factorQR.Q;
            var r = factorQR.R;

            // Make sure the R has the right dimensions.
            Assert.AreEqual(row, r.RowCount);
            Assert.AreEqual(column, r.ColumnCount);

            // Make sure the Q has the right dimensions.
            Assert.AreEqual(row, q.RowCount);
            Assert.AreEqual(row, q.ColumnCount);

            // Make sure the R factor is upper triangular.
            for (var i = 0; i < r.RowCount; i++)
            {
                for (var j = 0; j < r.ColumnCount; j++)
                {
                    if (i > j)
                    {
                        Assert.AreEqual(0.0, r[i, j]);
                    }
                }
            }

            // Make sure the Q*R is the original matrix.
            var matrixQfromR = q * r;
            for (var i = 0; i < matrixQfromR.RowCount; i++)
            {
                for (var j = 0; j < matrixQfromR.ColumnCount; j++)
                {
                    Assert.AreEqual(matrixA[i, j], matrixQfromR[i, j], 1e-4);
                }
            }

            // Make sure the Q is unitary --> (Q*)x(Q) = I
            var matrixQtQ = q.Transpose() * q;

            for (var i = 0; i < matrixQtQ.RowCount; i++)
            {
                for (var j = 0; j < matrixQtQ.ColumnCount; j++)
                {
                    Assert.AreEqual(matrixQtQ[i, j], i == j ? 1.0f : 0.0f, 1e-3f);
                }
            }
        }

        /// <summary>
        /// Can factorize a random matrix using thin QR.
        /// </summary>
        /// <param name="row">Matrix row number.</param>
        /// <param name="column">Matrix column number.</param>
        [TestCase(1, 1)]
        [TestCase(2, 2)]
        [TestCase(5, 5)]
        [TestCase(10, 6)]
        [TestCase(50, 48)]
        [TestCase(100, 98)]
        public void CanFactorizeRandomMatrixUsingThinQR(int row, int column)
        {
            var matrixA = Matrix<float>.Build.Random(row, column, 1);
            var factorQR = matrixA.QR(QRMethod.Thin);
            var q = factorQR.Q;
            var r = factorQR.R;

            // Make sure the R has the right dimensions.
            Assert.AreEqual(column, r.RowCount);
            Assert.AreEqual(column, r.ColumnCount);

            // Make sure the Q has the right dimensions.
            Assert.AreEqual(row, q.RowCount);
            Assert.AreEqual(column, q.ColumnCount);

            // Make sure the R factor is upper triangular.
            for (var i = 0; i < r.RowCount; i++)
            {
                for (var j = 0; j < r.ColumnCount; j++)
                {
                    if (i > j)
                    {
                        Assert.AreEqual(0.0, r[i, j]);
                    }
                }
            }

            // Make sure the Q*R is the original matrix.
            var matrixQfromR = q * r;
            for (var i = 0; i < matrixQfromR.RowCount; i++)
            {
                for (var j = 0; j < matrixQfromR.ColumnCount; j++)
                {
                    Assert.AreEqual(matrixA[i, j], matrixQfromR[i, j], 1.0e-4);
                }
            }

            // Make sure the Q is unitary --> (Q*)x(Q) = I
            var matrixQtQ = q.Transpose() * q;
            for (var i = 0; i < matrixQtQ.RowCount; i++)
            {
                for (var j = 0; j < matrixQtQ.ColumnCount; j++)
                {
                    Assert.AreEqual(matrixQtQ[i, j], i == j ? 1.0f : 0.0f, 1e-3f);
                }
            }
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
            var matrixA = Matrix<float>.Build.Random(order, order, 1);
            var matrixACopy = matrixA.Clone();
            var factorQR = matrixA.QR();

            var vectorb = Vector<float>.Build.Random(order, 1);
            var resultx = factorQR.Solve(vectorb);

            Assert.AreEqual(matrixA.ColumnCount, resultx.Count);

            var matrixBReconstruct = matrixA * resultx;

            // Check the reconstruction.
            for (var i = 0; i < order; i++)
            {
                Assert.AreEqual(vectorb[i], matrixBReconstruct[i], 1e-4);
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
        /// <param name="order">Matrix order.</param>
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(5)]
        [TestCase(10)]
        [TestCase(50)]
        [TestCase(100)]
        public void CanSolveForRandomMatrix(int order)
        {
            var matrixA = Matrix<float>.Build.Random(order, order, 1);
            var matrixACopy = matrixA.Clone();
            var factorQR = matrixA.QR();

            var matrixB = Matrix<float>.Build.Random(order, order, 1);
            var matrixX = factorQR.Solve(matrixB);

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
                    Assert.AreEqual(matrixB[i, j], matrixBReconstruct[i, j], 1e-4);
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
            var matrixA = Matrix<float>.Build.Random(order, order, 1);
            var matrixACopy = matrixA.Clone();
            var factorQR = matrixA.QR();
            var vectorb = Vector<float>.Build.Random(order, 1);
            var vectorbCopy = vectorb.Clone();
            var resultx = new DenseVector(order);
            factorQR.Solve(vectorb, resultx);

            Assert.AreEqual(vectorb.Count, resultx.Count);

            var matrixBReconstruct = matrixA * resultx;

            // Check the reconstruction.
            for (var i = 0; i < vectorb.Count; i++)
            {
                Assert.AreEqual(vectorb[i], matrixBReconstruct[i], 1e-4);
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
        /// <param name="order">Matrix order.</param>
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(5)]
        [TestCase(10)]
        [TestCase(50)]
        [TestCase(100)]
        public void CanSolveForRandomMatrixWhenResultMatrixGiven(int order)
        {
            var matrixA = Matrix<float>.Build.Random(order, order, 1);
            var matrixACopy = matrixA.Clone();
            var factorQR = matrixA.QR();

            var matrixB = Matrix<float>.Build.Random(order, order, 1);
            var matrixBCopy = matrixB.Clone();

            var matrixX = new DenseMatrix(order, order);
            factorQR.Solve(matrixB, matrixX);

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
                    Assert.AreEqual(matrixB[i, j], matrixBReconstruct[i, j], 1e-4);
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
        public void CanSolveForRandomVectorUsingThinQR(int order)
        {
            var matrixA = Matrix<float>.Build.Random(order, order, 1);
            var matrixACopy = matrixA.Clone();
            var factorQR = matrixA.QR(QRMethod.Thin);

            var vectorb = Vector<float>.Build.Random(order, 1);
            var resultx = factorQR.Solve(vectorb);

            Assert.AreEqual(matrixA.ColumnCount, resultx.Count);

            var matrixBReconstruct = matrixA * resultx;

            // Check the reconstruction.
            for (var i = 0; i < order; i++)
            {
                AssertHelpers.AlmostEqual(vectorb[i], matrixBReconstruct[i], 3);
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
        /// <param name="order">Matrix order.</param>
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(5)]
        [TestCase(10)]
        [TestCase(50)]
        [TestCase(100)]
        public void CanSolveForRandomMatrixUsingThinQR(int order)
        {
            var matrixA = Matrix<float>.Build.Random(order, order, 1);
            var matrixACopy = matrixA.Clone();
            var factorQR = matrixA.QR(QRMethod.Thin);

            var matrixB = Matrix<float>.Build.Random(order, order, 1);
            var matrixX = factorQR.Solve(matrixB);

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
                    Assert.AreEqual(matrixB[i, j], matrixBReconstruct[i, j], 1e-3);
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
        public void CanSolveForRandomVectorWhenResultVectorGivenUsingThinQR(int order)
        {
            var matrixA = Matrix<float>.Build.Random(order, order, 1);
            var matrixACopy = matrixA.Clone();
            var factorQR = matrixA.QR(QRMethod.Thin);
            var vectorb = Vector<float>.Build.Random(order, 1);
            var vectorbCopy = vectorb.Clone();
            var resultx = new DenseVector(order);
            factorQR.Solve(vectorb, resultx);

            Assert.AreEqual(vectorb.Count, resultx.Count);

            var matrixBReconstruct = matrixA * resultx;

            // Check the reconstruction.
            for (var i = 0; i < vectorb.Count; i++)
            {
                AssertHelpers.AlmostEqual(vectorb[i], matrixBReconstruct[i], 3);
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
        /// <param name="order">Matrix order.</param>
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(5)]
        [TestCase(10)]
        [TestCase(50)]
        [TestCase(100)]
        public void CanSolveForRandomMatrixWhenResultMatrixGivenUsingThinQR(int order)
        {
            var matrixA = Matrix<float>.Build.Random(order, order, 1);
            var matrixACopy = matrixA.Clone();
            var factorQR = matrixA.QR(QRMethod.Thin);

            var matrixB = Matrix<float>.Build.Random(order, order, 1);
            var matrixBCopy = matrixB.Clone();

            var matrixX = new DenseMatrix(order, order);
            factorQR.Solve(matrixB, matrixX);

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
                    Assert.AreEqual(matrixB[i, j], matrixBReconstruct[i, j], 1e-3);
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

        /// <summary>
        /// Can solve when using a tall matrix.
        /// </summary>
        /// <param name="method">The QR decomp method to use.</param>
        [TestCase(QRMethod.Full)]
        [TestCase(QRMethod.Thin)]
        public void CanSolveForMatrixWithTallRandomMatrix(QRMethod method)
        {
            var matrixA = Matrix<float>.Build.Random(20, 10, 1);
            var matrixACopy = matrixA.Clone();
            var factorQR = matrixA.QR(method);

            var matrixB = Matrix<float>.Build.Random(20, 5, 1);
            var matrixX = factorQR.Solve(matrixB);

            // The solution X row dimension is equal to the column dimension of A
            Assert.AreEqual(matrixA.ColumnCount, matrixX.RowCount);

            // The solution X has the same number of columns as B
            Assert.AreEqual(matrixB.ColumnCount, matrixX.ColumnCount);

            var test = (matrixA.Transpose() * matrixA).Inverse() * matrixA.Transpose() * matrixB;

            for (var i = 0; i < matrixX.RowCount; i++)
            {
                for (var j = 0; j < matrixX.ColumnCount; j++)
                {
                    AssertHelpers.AlmostEqual(test[i, j], matrixX[i, j], 5);
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
        /// Can solve when using a tall matrix.
        /// </summary>
        /// <param name="method">The QR decomp method to use.</param>
        [TestCase(QRMethod.Full)]
        [TestCase(QRMethod.Thin)]
        public void CanSolveForVectorWithTallRandomMatrix(QRMethod method)
        {
            var matrixA = Matrix<float>.Build.Random(20, 10, 1);
            var matrixACopy = matrixA.Clone();
            var factorQR = matrixA.QR(method);

            var vectorB = Vector<float>.Build.Random(20, 1);
            var vectorX = factorQR.Solve(vectorB);

            // The solution x dimension is equal to the column dimension of A
            Assert.AreEqual(matrixA.ColumnCount, vectorX.Count);

            var test = (matrixA.Transpose() * matrixA).Inverse() * matrixA.Transpose() * vectorB;

            for (var i = 0; i < vectorX.Count; i++)
            {
                AssertHelpers.AlmostEqual(test[i], vectorX[i], 5);
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
    }
}
