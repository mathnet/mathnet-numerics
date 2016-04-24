// <copyright file="LUTests.cs" company="Math.NET">
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
using MathNet.Numerics.LinearAlgebra.Double;
using NUnit.Framework;

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Double.Factorization
{
    /// <summary>
    /// LU factorization tests for a dense matrix.
    /// </summary>
    [TestFixture, Category("LAFactorization")]
    public class LUTests
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
            var matrixI = Matrix<double>.Build.DenseIdentity(order);
            var factorLU = matrixI.LU();

            // Check lower triangular part.
            var matrixL = factorLU.L;
            Assert.AreEqual(matrixI.RowCount, matrixL.RowCount);
            Assert.AreEqual(matrixI.ColumnCount, matrixL.ColumnCount);
            for (var i = 0; i < matrixL.RowCount; i++)
            {
                for (var j = 0; j < matrixL.ColumnCount; j++)
                {
                    Assert.AreEqual(i == j ? 1.0 : 0.0, matrixL[i, j]);
                }
            }

            // Check upper triangular part.
            var matrixU = factorLU.U;
            Assert.AreEqual(matrixI.RowCount, matrixU.RowCount);
            Assert.AreEqual(matrixI.ColumnCount, matrixU.ColumnCount);
            for (var i = 0; i < matrixU.RowCount; i++)
            {
                for (var j = 0; j < matrixU.ColumnCount; j++)
                {
                    Assert.AreEqual(i == j ? 1.0 : 0.0, matrixU[i, j]);
                }
            }
        }

        /// <summary>
        /// LU factorization fails with a non-square matrix.
        /// </summary>
        [Test]
        public void LUFailsWithNonSquareMatrix()
        {
            var matrix = Matrix<double>.Build.Dense(3, 2);
            Assert.That(() => matrix.LU(), Throws.ArgumentException);
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
            var matrixI = Matrix<double>.Build.DenseIdentity(order);
            var lu = matrixI.LU();
            Assert.AreEqual(1.0, lu.Determinant);
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
            var matrixX = Matrix<double>.Build.Random(order, order, 1);
            var factorLU = matrixX.LU();
            var matrixL = factorLU.L;
            var matrixU = factorLU.U;

            // Make sure the factors have the right dimensions.
            Assert.AreEqual(order, matrixL.RowCount);
            Assert.AreEqual(order, matrixL.ColumnCount);
            Assert.AreEqual(order, matrixU.RowCount);
            Assert.AreEqual(order, matrixU.ColumnCount);

            // Make sure the L factor is lower triangular.
            for (var i = 0; i < matrixL.RowCount; i++)
            {
                Assert.AreEqual(1.0, matrixL[i, i]);
                for (var j = i + 1; j < matrixL.ColumnCount; j++)
                {
                    Assert.AreEqual(0.0, matrixL[i, j]);
                }
            }

            // Make sure the U factor is upper triangular.
            for (var i = 0; i < matrixL.RowCount; i++)
            {
                for (var j = 0; j < i; j++)
                {
                    Assert.AreEqual(0.0, matrixU[i, j]);
                }
            }

            // Make sure the LU factor times it's transpose is the original matrix.
            var matrixXfromLU = matrixL * matrixU;
            var permutationInverse = factorLU.P.Inverse();
            matrixXfromLU.PermuteRows(permutationInverse);
            for (var i = 0; i < matrixXfromLU.RowCount; i++)
            {
                for (var j = 0; j < matrixXfromLU.ColumnCount; j++)
                {
                    Assert.AreEqual(matrixX[i, j], matrixXfromLU[i, j], 1.0e-11);
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
            var matrixA = Matrix<double>.Build.Random(order, order, 1);
            var matrixACopy = matrixA.Clone();
            var factorLU = matrixA.LU();

            var vectorb = Vector<double>.Build.Random(order, 1);
            var resultx = factorLU.Solve(vectorb);

            Assert.AreEqual(matrixA.ColumnCount, resultx.Count);

            var matrixBReconstruct = matrixA * resultx;

            // Check the reconstruction.
            for (var i = 0; i < order; i++)
            {
                Assert.AreEqual(vectorb[i], matrixBReconstruct[i], 1.0e-11);
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
            var matrixA = Matrix<double>.Build.Random(order, order, 1);
            var matrixACopy = matrixA.Clone();
            var factorLU = matrixA.LU();

            var matrixB = Matrix<double>.Build.Random(order, order, 1);
            var matrixX = factorLU.Solve(matrixB);

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
                    Assert.AreEqual(matrixB[i, j], matrixBReconstruct[i, j], 1.0e-11);
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
            var matrixA = Matrix<double>.Build.Random(order, order, 1);
            var matrixACopy = matrixA.Clone();
            var factorLU = matrixA.LU();
            var vectorb = Vector<double>.Build.Random(order, 1);
            var vectorbCopy = vectorb.Clone();
            var resultx = new DenseVector(order);
            factorLU.Solve(vectorb, resultx);

            Assert.AreEqual(vectorb.Count, resultx.Count);

            var matrixBReconstruct = matrixA * resultx;

            // Check the reconstruction.
            for (var i = 0; i < vectorb.Count; i++)
            {
                Assert.AreEqual(vectorb[i], matrixBReconstruct[i], 1.0e-11);
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
        /// <param name="order">Matrix row number.</param>
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(5)]
        [TestCase(10)]
        [TestCase(50)]
        [TestCase(100)]
        public void CanSolveForRandomMatrixWhenResultMatrixGiven(int order)
        {
            var matrixA = Matrix<double>.Build.Random(order, order, 1);
            var matrixACopy = matrixA.Clone();
            var factorLU = matrixA.LU();

            var matrixB = Matrix<double>.Build.Random(order, order, 1);
            var matrixBCopy = matrixB.Clone();

            var matrixX = Matrix<double>.Build.Dense(order, order);
            factorLU.Solve(matrixB, matrixX);

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
                    Assert.AreEqual(matrixB[i, j], matrixBReconstruct[i, j], 1.0e-11);
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
        /// Can inverse a matrix.
        /// </summary>
        /// <param name="order">Matrix order.</param>
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(5)]
        [TestCase(10)]
        [TestCase(50)]
        [TestCase(100)]
        public void CanInverse(int order)
        {
            var matrixA = Matrix<double>.Build.Random(order, order, 1);
            var matrixACopy = matrixA.Clone();
            var factorLU = matrixA.LU();

            var matrixAInverse = factorLU.Inverse();

            // The inverse dimension is equal A
            Assert.AreEqual(matrixAInverse.RowCount, matrixAInverse.RowCount);
            Assert.AreEqual(matrixAInverse.ColumnCount, matrixAInverse.ColumnCount);

            var matrixIdentity = matrixA * matrixAInverse;

            // Make sure A didn't change.
            for (var i = 0; i < matrixA.RowCount; i++)
            {
                for (var j = 0; j < matrixA.ColumnCount; j++)
                {
                    Assert.AreEqual(matrixACopy[i, j], matrixA[i, j]);
                }
            }

            // Check if multiplication of A and AI produced identity matrix.
            for (var i = 0; i < matrixIdentity.RowCount; i++)
            {
                Assert.AreEqual(matrixIdentity[i, i], 1.0, 1.0e-11);
            }
        }
    }
}
