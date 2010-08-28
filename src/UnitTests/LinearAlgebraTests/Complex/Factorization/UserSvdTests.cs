// <copyright file="UserSvdTests.cs" company="Math.NET">
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

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Complex.Factorization
{
    using System;
    using System.Numerics;
    using LinearAlgebra.Generic.Factorization;
    using MbUnit.Framework;
    using LinearAlgebra.Complex.Factorization;

    public class UserSvdTests
    {

        [Test]
        [ExpectedArgumentNullException]
        public void ConstructorNull()
        {
            new UserSvd(null, true);
        }

        [Test]
        [Row(1)]
        [Row(10)]
        [Row(100)]
        public void CanFactorizeIdentity(int order)
        {
            var I = UserDefinedMatrix.Identity(order);
            var factorSvd = I.Svd(true);

            Assert.AreEqual(I.RowCount, factorSvd.U().RowCount);
            Assert.AreEqual(I.RowCount, factorSvd.U().ColumnCount);

            Assert.AreEqual(I.ColumnCount, factorSvd.VT().RowCount);
            Assert.AreEqual(I.ColumnCount, factorSvd.VT().ColumnCount);

            Assert.AreEqual(I.RowCount, factorSvd.W().RowCount);
            Assert.AreEqual(I.ColumnCount, factorSvd.W().ColumnCount);

            for (var i = 0; i < factorSvd.W().RowCount; i++)
            {
                for (var j = 0; j < factorSvd.W().ColumnCount; j++)
                {
                    Assert.AreEqual(i == j ? Complex.One : Complex.Zero, factorSvd.W()[i, j]);
                }
            }
        }

        [Test]
        [Row(1,1)]
        [Row(2,2)]
        [Row(5,5)]
        [Row(10,6)]
        [Row(48,52)]
        [Row(100,93)]
        [MultipleAsserts]
        public void CanFactorizeRandomMatrix(int row, int column)
        {
            var matrixA = MatrixLoader.GenerateRandomUserDefinedMatrix(row, column);
            var factorSvd = matrixA.Svd(true);

            // Make sure the U has the right dimensions.
            Assert.AreEqual(row, factorSvd.U().RowCount);
            Assert.AreEqual(row, factorSvd.U().ColumnCount);

            // Make sure the VT has the right dimensions.
            Assert.AreEqual(column, factorSvd.VT().RowCount);
            Assert.AreEqual(column, factorSvd.VT().ColumnCount);

            // Make sure the W has the right dimensions.
            Assert.AreEqual(row, factorSvd.W().RowCount);
            Assert.AreEqual(column, factorSvd.W().ColumnCount);

            // Make sure the U*W*VT is the original matrix.
            var matrix = factorSvd.U() * factorSvd.W() * factorSvd.VT();
            for (var i = 0; i < matrix.RowCount; i++) 
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    Assert.AreApproximatelyEqual(matrixA[i, j].Real, matrix[i, j].Real, 1.0e-9);
                    Assert.AreApproximatelyEqual(matrixA[i, j].Imaginary, matrix[i, j].Imaginary, 1.0e-9);
                }
            }
        }

        [Test]
        [Row(10, 8)]
        [Row(48, 52)]
        [Row(100, 93)]
        [MultipleAsserts]
        public void CheckRankOfNonSquare(int row, int column)
        {
            var matrixA = MatrixLoader.GenerateRandomUserDefinedMatrix(row, column);
            var factorSvd = matrixA.Svd(true);

            var mn = Math.Min(row, column);
            Assert.AreEqual(factorSvd.Rank, mn);
        }

        [Test]
        [Row(1)]
        [Row(2)]
        [Row(5)]
        [Row(9)]
        [Row(50)]
        [Row(90)]
        [MultipleAsserts]
        public void CheckRankSquare(int order)
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

        [Test]
        [Row(10)]
        [Row(50)]
        [Row(100)]
        [MultipleAsserts]
        public void CheckRankOfSquareSingular(int order)
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

            Assert.AreEqual(factorSvd.Determinant, 0);
            Assert.AreEqual(factorSvd.Rank, order - 1);
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CannotSolveMatrixIfVectorsNotComputed()
        {
            var matrixA = MatrixLoader.GenerateRandomUserDefinedMatrix(10, 10);
            var factorSvd = matrixA.Svd(false);

            var matrixB = MatrixLoader.GenerateRandomUserDefinedMatrix(10, 10);
            factorSvd.Solve(matrixB);
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CannotSolveVectorIfVectorsNotComputed()
        {
            var matrixA = MatrixLoader.GenerateRandomUserDefinedMatrix(10, 10);
            var factorSvd = matrixA.Svd(false);

            var vectorb = MatrixLoader.GenerateRandomUserDefinedVector(10);
            factorSvd.Solve(vectorb);
        }

        [Test]
        [Row(1, 1)]
        [Row(2, 2)]
        [Row(5, 5)]
        [Row(9, 10)]
        [Row(50, 50)]
        [Row(90, 100)]
        [MultipleAsserts]
        public void CanSolveForRandomVector(int row, int column)
        {
            var matrixA = MatrixLoader.GenerateRandomUserDefinedMatrix(row, column);
            var matrixACopy = matrixA.Clone();
            var factorSvd = matrixA.Svd(true);

            var vectorb = MatrixLoader.GenerateRandomUserDefinedVector(row);
            var resultx = factorSvd.Solve(vectorb);

            Assert.AreEqual(matrixA.ColumnCount, resultx.Count);

            var bReconstruct = matrixA * resultx;

            // Check the reconstruction.
            for (var i = 0; i < vectorb.Count; i++)
            {
                Assert.AreApproximatelyEqual(vectorb[i].Real, bReconstruct[i].Real, 1.0e-9);
                Assert.AreApproximatelyEqual(vectorb[i].Imaginary, bReconstruct[i].Imaginary, 1.0e-9);
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
        [Row(1, 1)]
        [Row(4, 4)]
        [Row(7, 8)]
        [Row(10, 10)]
        [Row(45, 50)]
        [Row(80, 100)]
        [MultipleAsserts]
        public void CanSolveForRandomMatrix(int row, int count)
        {
            var matrixA = MatrixLoader.GenerateRandomUserDefinedMatrix(row, count);
            var matrixACopy = matrixA.Clone();
            var factorSvd = matrixA.Svd(true);

            var matrixB = MatrixLoader.GenerateRandomUserDefinedMatrix(row, count);
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
                    Assert.AreApproximatelyEqual(matrixB[i, j].Real, matrixBReconstruct[i, j].Real, 1.0e-9);
                    Assert.AreApproximatelyEqual(matrixB[i, j].Imaginary, matrixBReconstruct[i, j].Imaginary, 1.0e-9);
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
        [Row(1, 1)]
        [Row(2, 2)]
        [Row(5, 5)]
        [Row(9, 10)]
        [Row(50, 50)]
        [Row(90, 100)]
        [MultipleAsserts]
        public void CanSolveForRandomVectorWhenResultVectorGiven(int row, int column)
        {
            var matrixA = MatrixLoader.GenerateRandomUserDefinedMatrix(row, column);
            var matrixACopy = matrixA.Clone();
            var factorSvd = matrixA.Svd(true);
            var vectorb = MatrixLoader.GenerateRandomUserDefinedVector(row);
            var vectorbCopy = vectorb.Clone();
            var resultx = new UserDefinedVector(column);
            factorSvd.Solve(vectorb,resultx);

            var bReconstruct = matrixA * resultx;

            // Check the reconstruction.
            for (var i = 0; i < vectorb.Count; i++)
            {
                Assert.AreApproximatelyEqual(vectorb[i].Real, bReconstruct[i].Real, 1.0e-9);
                Assert.AreApproximatelyEqual(vectorb[i].Imaginary, bReconstruct[i].Imaginary, 1.0e-9);
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
        [Row(1, 1)]
        [Row(4, 4)]
        [Row(7, 8)]
        [Row(10, 10)]
        [Row(45, 50)]
        [Row(80, 100)]
        [MultipleAsserts]
        public void CanSolveForRandomMatrixWhenResultMatrixGiven(int row, int column)
        {
            var matrixA = MatrixLoader.GenerateRandomUserDefinedMatrix(row, column);
            var matrixACopy = matrixA.Clone();
            var factorSvd = matrixA.Svd(true);

            var matrixB = MatrixLoader.GenerateRandomUserDefinedMatrix(row, column);
            var matrixBCopy = matrixB.Clone();

            var matrixX = new UserDefinedMatrix(column, column);
            factorSvd.Solve(matrixB,matrixX);

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
                    Assert.AreApproximatelyEqual(matrixB[i, j].Real, matrixBReconstruct[i, j].Real, 1.0e-9);
                    Assert.AreApproximatelyEqual(matrixB[i, j].Imaginary, matrixBReconstruct[i, j].Imaginary, 1.0e-9);
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
