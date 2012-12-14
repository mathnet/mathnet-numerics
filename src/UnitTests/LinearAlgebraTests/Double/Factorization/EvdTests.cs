// <copyright file="EvdTests.cs" company="Math.NET">
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

using MathNet.Numerics.LinearAlgebra.Generic;

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Double.Factorization
{
    using System;
    using System.Numerics;
    using LinearAlgebra.Double;
    using LinearAlgebra.Double.Factorization;
    using NUnit.Framework;

    /// <summary>
    /// Eigenvalues factorization tests for a dense matrix.
    /// </summary>
    public class EvdTests
    {
        /// <summary>
        /// Constructor <c>null</c> throws <c>ArgumentNullException</c>.
        /// </summary>
        [Test]
        public void ConstructorNullThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new DenseEvd(null));
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
            var matrixI = DenseMatrix.Identity(order);
            var factorEvd = matrixI.Evd();
            var eigenValues = factorEvd.EigenValues();
            var eigenVectors = factorEvd.EigenVectors();
            var d = factorEvd.D();

            Assert.AreEqual(matrixI.RowCount, eigenVectors.RowCount);
            Assert.AreEqual(matrixI.RowCount, eigenVectors.ColumnCount);

            Assert.AreEqual(matrixI.ColumnCount, d.RowCount);
            Assert.AreEqual(matrixI.ColumnCount, d.ColumnCount);

            for (var i = 0; i < eigenValues.Count; i++)
            {
                Assert.AreEqual(Complex.One, eigenValues[i]);
            }
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
            var matrixA = MatrixLoader.GenerateRandomDenseMatrix(order, order);
            var factorEvd = matrixA.Evd();
            var eigenVectors = factorEvd.EigenVectors();
            var d = factorEvd.D();

            Assert.AreEqual(order, eigenVectors.RowCount);
            Assert.AreEqual(order, eigenVectors.ColumnCount);

            Assert.AreEqual(order, d.RowCount);
            Assert.AreEqual(order, d.ColumnCount);

            // Make sure the A*V = λ*V 
            var matrixAv = matrixA * eigenVectors;
            var matrixLv = eigenVectors * factorEvd.D();

            for (var i = 0; i < matrixAv.RowCount; i++)
            {
                for (var j = 0; j < matrixAv.ColumnCount; j++)
                {
                    Assert.AreEqual(matrixAv[i, j], matrixLv[i, j], 1.0e-10);
                }
            }
        }

        /// <summary>
        /// Can factorize a symmetric random square matrix.
        /// </summary>
        /// <param name="order">Matrix order.</param>
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(5)]
        [TestCase(10)]
        [TestCase(50)]
        [TestCase(100)]
        public void CanFactorizeRandomSymmetricMatrix(int order)
        {
            var matrixA = MatrixLoader.GenerateRandomPositiveDefiniteDenseMatrix(order);
            MatrixHelpers.ForceSymmetric(matrixA);
            var factorEvd = matrixA.Evd();
            var eigenVectors = factorEvd.EigenVectors();
            var d = factorEvd.D();

            Assert.AreEqual(order, eigenVectors.RowCount);
            Assert.AreEqual(order, eigenVectors.ColumnCount);

            Assert.AreEqual(order, d.RowCount);
            Assert.AreEqual(order, d.ColumnCount);

            // Make sure the A = V*λ*VT 
            var matrix = eigenVectors * d * eigenVectors.Transpose();

            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    Assert.AreEqual(matrix[i, j], matrixA[i, j], 1.0e-10);
                }
            }
        }

        /// <summary>
        /// Can check rank of square matrix.
        /// </summary>
        /// <param name="order">Matrix order.</param>
        [TestCase(10)]
        [TestCase(50)]
        [TestCase(100)]
        public void CanCheckRankSquare(int order)
        {
            var matrixA = MatrixLoader.GenerateRandomDenseMatrix(order, order);
            var factorEvd = matrixA.Evd();

            Assert.AreEqual(factorEvd.Rank, order);
        }

        /// <summary>
        /// Can check rank of square singular matrix.
        /// </summary>
        /// <param name="order">Matrix order.</param>
        [TestCase(10)]
        [TestCase(50)]
        [TestCase(100)]
        public void CanCheckRankOfSquareSingular(int order)
        {
            var matrixA = new DenseMatrix(order, order);
            matrixA[0, 0] = 1;
            matrixA[order - 1, order - 1] = 1;
            for (var i = 1; i < order - 1; i++)
            {
                matrixA[i, i - 1] = 1;
                matrixA[i, i + 1] = 1;
                matrixA[i - 1, i] = 1;
                matrixA[i + 1, i] = 1;
            }

            var factorEvd = matrixA.Evd();

            Assert.AreEqual(factorEvd.Determinant, 0);
            Assert.AreEqual(factorEvd.Rank, order - 1);
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
            var matrixI = DenseMatrix.Identity(order);
            var factorEvd = matrixI.Evd();
            Assert.AreEqual(1.0, factorEvd.Determinant);
        }

        /// <summary>
        /// Can solve a system of linear equations for a random vector and symmetric matrix (Ax=b).
        /// </summary>
        /// <param name="order">Matrix order.</param>
        [Test]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(5)]
        [TestCase(10)]
        [TestCase(50)]
        [TestCase(100)]
        public void CanSolveForRandomVectorAndSymmetricMatrix(int order)
        {
            var matrixA = MatrixLoader.GenerateRandomPositiveDefiniteDenseMatrix(order);
            MatrixHelpers.ForceSymmetric(matrixA);
            var matrixACopy = matrixA.Clone();
            var factorEvd = matrixA.Evd();

            var vectorb = MatrixLoader.GenerateRandomDenseVector(order);
            var resultx = factorEvd.Solve(vectorb);

            Assert.AreEqual(matrixA.ColumnCount, resultx.Count);

            var matrixBReconstruct = matrixA * resultx;

            // Check the reconstruction.
            for (var i = 0; i < vectorb.Count; i++)
            {
                Assert.AreEqual(vectorb[i], matrixBReconstruct[i], 1.0e-10);
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

        //private 
        /// <summary>
        /// Can solve a system of linear equations for a random matrix and symmetric matrix (AX=B).
        /// </summary>
        /// <param name="order">Matrix order.</param>
        [Test]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(5)]
        [TestCase(10)]
        [TestCase(50)]
        [TestCase(100)]
        public void CanSolveForRandomMatrixAndSymmetricMatrix(int order)
        {
            var matrixA = MatrixLoader.GenerateRandomPositiveDefiniteDenseMatrix(order);
            MatrixHelpers.ForceSymmetric(matrixA);
            var matrixACopy = matrixA.Clone();
            var factorEvd = matrixA.Evd();

            var matrixB = MatrixLoader.GenerateRandomDenseMatrix(order, order);

            var matrixX = factorEvd.Solve(matrixB);

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
                    Assert.AreEqual(matrixB[i, j], matrixBReconstruct[i, j], 1.0e-10);
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
        /// Can solve a system of linear equations for a random vector and symmetric matrix (Ax=b) into a result vector.
        /// </summary>
        /// <param name="order">Matrix order.</param>
        [Test]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(5)]
        [TestCase(10)]
        [TestCase(50)]
        [TestCase(100)]
        public void CanSolveForRandomVectorAndSymmetricMatrixWhenResultVectorGiven(int order)
        {
            var matrixA = MatrixLoader.GenerateRandomPositiveDefiniteDenseMatrix(order);
            MatrixHelpers.ForceSymmetric(matrixA);
            var matrixACopy = matrixA.Clone();
            var factorEvd = matrixA.Evd();
            var vectorb = MatrixLoader.GenerateRandomDenseVector(order);
            var vectorbCopy = vectorb.Clone();
            var resultx = new DenseVector(order);
            factorEvd.Solve(vectorb, resultx);

            var matrixBReconstruct = matrixA * resultx;

            // Check the reconstruction.
            for (var i = 0; i < vectorb.Count; i++)
            {
                Assert.AreEqual(vectorb[i], matrixBReconstruct[i], 1.0e-10);
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
        /// Can solve a system of linear equations for a random matrix and symmetric matrix (AX=B) into result matrix.
        /// </summary>
        /// <param name="order">Matrix order.</param>
        [Test]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(5)]
        [TestCase(10)]
        [TestCase(50)]
        [TestCase(100)]
        public void CanSolveForRandomMatrixAndSymmetricMatrixWhenResultMatrixGiven(int order)
        {
            var matrixA = MatrixLoader.GenerateRandomPositiveDefiniteDenseMatrix(order);
            MatrixHelpers.ForceSymmetric(matrixA);
            var matrixACopy = matrixA.Clone();
            var factorEvd = matrixA.Evd();

            var matrixB = MatrixLoader.GenerateRandomDenseMatrix(order, order);
            var matrixBCopy = matrixB.Clone();

            var matrixX = new DenseMatrix(order, order);
            factorEvd.Solve(matrixB, matrixX);

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
                    Assert.AreEqual(matrixB[i, j], matrixBReconstruct[i, j], 1.0e-10);
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
