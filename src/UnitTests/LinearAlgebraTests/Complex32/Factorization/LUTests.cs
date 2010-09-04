// <copyright file="LUTests.cs" company="Math.NET">
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

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Complex32.Factorization
{
    using Numerics;
    using LinearAlgebra.Generic.Factorization;
    using MbUnit.Framework;
    using LinearAlgebra.Complex32;
    
    public class LUTests
    {
        [Test]
        [Row(1)]
        [Row(10)]
        [Row(100)]
        public void CanFactorizeIdentity(int order)
        {
            var matrixI = DenseMatrix.Identity(order);
            var factorLU = matrixI.LU();

            // Check lower triangular part.
            var matrixL = factorLU.L;
            Assert.AreEqual(matrixI.RowCount, matrixL.RowCount);
            Assert.AreEqual(matrixI.ColumnCount, matrixL.ColumnCount);
            for (var i = 0; i < matrixL.RowCount; i++)
            {
                for (var j = 0; j < matrixL.ColumnCount; j++)
                {
                    Assert.AreEqual(i == j ? Complex32.One : Complex32.Zero, matrixL[i, j]);
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
                    Assert.AreEqual(i == j ? Complex32.One : Complex32.Zero, matrixU[i, j]);
                }
            }
        }

        [Test]
        [Row(3,5)]
        [Row(5,3)]
        [ExpectedArgumentException]
        public void LUFailsWithNonSquareMatrix(int row, int col)
        {
            var I = new DenseMatrix(row, col);
            I.LU();
        }

        [Test]
        [Row(1)]
        [Row(10)]
        [Row(100)]
        public void IdentityDeterminantIsOne(int order)
        {
            var I = DenseMatrix.Identity(order);
            var lu = I.LU();
            Assert.AreEqual(Complex32.One, lu.Determinant);
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
            var matrixX = MatrixLoader.GenerateRandomDenseMatrix(order, order);
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
                Assert.AreEqual(1.0f, matrixL[i, i]);
                for (var j = i+1; j < matrixL.ColumnCount; j++)
                {
                    Assert.AreEqual(0.0f, matrixL[i, j]);
                }
            }

            // Make sure the U factor is upper triangular.
            for (var i = 0; i < matrixL.RowCount; i++)
            {
                for (var j = 0; j < i; j++)
                {
                    Assert.AreEqual(0.0f, matrixU[i, j]);
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
                    Assert.AreApproximatelyEqual(matrixX[i, j].Real, matrixXfromLU[i, j].Real, 1e-3f);
                    Assert.AreApproximatelyEqual(matrixX[i, j].Imaginary, matrixXfromLU[i, j].Imaginary, 1e-3f);
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
            var matrixA = MatrixLoader.GenerateRandomDenseMatrix(order, order);
            var matrixACopy = matrixA.Clone();
            var factorLU = matrixA.LU();

            var vectorb = MatrixLoader.GenerateRandomDenseVector(order);
            var resultx = factorLU.Solve(vectorb);

            Assert.AreEqual(matrixA.ColumnCount, resultx.Count);

            var bReconstruct = matrixA * resultx;

            // Check the reconstruction.
            for (var i = 0; i < order; i++)
            {
                Assert.AreApproximatelyEqual(vectorb[i].Real, bReconstruct[i].Real, 1e-3f);
                Assert.AreApproximatelyEqual(vectorb[i].Imaginary, bReconstruct[i].Imaginary, 1e-3f);
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

        [Test]
        [Row(1)]
        [Row(4)]
        [Row(8)]
        [Row(10)]
        [Row(50)]
        [Row(100)]
        [MultipleAsserts]
        public void CanSolveForRandomMatrix(int order)
        {
            var matrixA = MatrixLoader.GenerateRandomDenseMatrix(order, order);
            var matrixACopy = matrixA.Clone();
            var factorLU = matrixA.LU();

            var matrixB = MatrixLoader.GenerateRandomDenseMatrix(order, order);
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
                    Assert.AreApproximatelyEqual(matrixB[i, j].Real, matrixBReconstruct[i, j].Real, 1e-3f);
                    Assert.AreApproximatelyEqual(matrixB[i, j].Imaginary, matrixBReconstruct[i, j].Imaginary, 1e-3f);
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
            var matrixA = MatrixLoader.GenerateRandomDenseMatrix(order, order);
            var matrixACopy = matrixA.Clone();
            var factorLU = matrixA.LU();
            var vectorb = MatrixLoader.GenerateRandomDenseVector(order);
            var vectorbCopy = vectorb.Clone();
            var resultx = new DenseVector(order);
            factorLU.Solve(vectorb, resultx);

            Assert.AreEqual(vectorb.Count, resultx.Count);

            var bReconstruct = matrixA * resultx;

            // Check the reconstruction.
            for (var i = 0; i < vectorb.Count; i++)
            {
                Assert.AreApproximatelyEqual(vectorb[i].Real, bReconstruct[i].Real, 1e-3f);
                Assert.AreApproximatelyEqual(vectorb[i].Imaginary, bReconstruct[i].Imaginary, 1e-3f);
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

        [Test]
        [Row(1)]
        [Row(4)]
        [Row(8)]
        [Row(10)]
        [Row(50)]
        [Row(100)]
        [MultipleAsserts]
        public void CanSolveForRandomMatrixWhenResultMatrixGiven(int order)
        {
            var matrixA = MatrixLoader.GenerateRandomDenseMatrix(order, order);
            var matrixACopy = matrixA.Clone();
            var factorLU = matrixA.LU();

            var matrixB = MatrixLoader.GenerateRandomDenseMatrix(order, order);
            var matrixBCopy = matrixB.Clone();

            var matrixX = new DenseMatrix(order, order);
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
                    Assert.AreApproximatelyEqual(matrixB[i, j].Real, matrixBReconstruct[i, j].Real, 1e-3f);
                    Assert.AreApproximatelyEqual(matrixB[i, j].Imaginary, matrixBReconstruct[i, j].Imaginary, 1e-3f);
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

        [Test]
        [Row(1)]
        [Row(4)]
        [Row(8)]
        [Row(10)]
        [Row(50)]
        [Row(100)]
        [MultipleAsserts]
        public void CanInverse(int order)
        {
            var matrixA = MatrixLoader.GenerateRandomDenseMatrix(order, order);
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
                Assert.AreApproximatelyEqual(matrixIdentity[i, i].Real, Complex32.One.Real, 1e-3f);
                Assert.AreApproximatelyEqual(matrixIdentity[i, i].Imaginary, Complex32.One.Imaginary, 1e-3f);
            }
        }
    }
}
