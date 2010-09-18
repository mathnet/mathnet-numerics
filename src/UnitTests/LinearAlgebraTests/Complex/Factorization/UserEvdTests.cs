// <copyright file="UserEvdTests.cs" company="Math.NET">
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
    using System.Numerics;
    using LinearAlgebra.Generic.Factorization;
    using MbUnit.Framework;
    using LinearAlgebra.Complex.Factorization;

    public class UserEvdTests
    {

        [Test]
        [ExpectedArgumentNullException]
        public void ConstructorNull()
        {
            new UserEvd(null);
        }

        [Test]
        [Row(1)]
        [Row(10)]
        [Row(100)]
        public void CanFactorizeIdentity(int order)
        {
            var I = UserDefinedMatrix.Identity(order);
            var factorEvd = I.Evd();

            Assert.AreEqual(I.RowCount, factorEvd.EVectors().RowCount);
            Assert.AreEqual(I.RowCount, factorEvd.EVectors().ColumnCount);

            Assert.AreEqual(I.ColumnCount, factorEvd.D().RowCount);
            Assert.AreEqual(I.ColumnCount, factorEvd.D().ColumnCount);

            for (var i = 0; i < factorEvd.EValues().Count; i++)
            {
                Assert.AreEqual(Complex.One, factorEvd.EValues()[i]);
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
        public void CanFactorizeRandomMatrix(int order)
        {
            var matrixA = MatrixLoader.GenerateRandomUserDefinedMatrix(order, order);
            var factorEvd = matrixA.Evd();

            Assert.AreEqual(order, factorEvd.EVectors().RowCount);
            Assert.AreEqual(order, factorEvd.EVectors().ColumnCount);

            Assert.AreEqual(order, factorEvd.D().RowCount);
            Assert.AreEqual(order, factorEvd.D().ColumnCount);

            // Make sure the A*V = λ*V 
            var matrixAv = matrixA * factorEvd.EVectors();
            var matrixLv = factorEvd.EVectors() * factorEvd.D();

            for (var i = 0; i < matrixAv.RowCount; i++) 
            {
                for (var j = 0; j < matrixAv.ColumnCount; j++)
                {
                    Assert.AreApproximatelyEqual(matrixAv[i, j].Real, matrixLv[i, j].Real, 1e-9);
                    Assert.AreApproximatelyEqual(matrixAv[i, j].Imaginary, matrixLv[i, j].Imaginary, 1e-9);
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
        public void CanFactorizeRandomSymmetricMatrix(int order)
        {
            var matrixA = MatrixLoader.GenerateRandomPositiveDefiniteHermitianUserDefinedMatrix(order);
            var factorEvd = matrixA.Evd();

            Assert.AreEqual(order, factorEvd.EVectors().RowCount);
            Assert.AreEqual(order, factorEvd.EVectors().ColumnCount);

            Assert.AreEqual(order, factorEvd.D().RowCount);
            Assert.AreEqual(order, factorEvd.D().ColumnCount);

            // Make sure the A = V*λ*VT 
            var matrix = factorEvd.EVectors() * factorEvd.D() * factorEvd.EVectors().ConjugateTranspose();

            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    Assert.AreApproximatelyEqual(matrix[i, j].Real, matrixA[i, j].Real, 1e-9);
                    Assert.AreApproximatelyEqual(matrix[i, j].Imaginary, matrixA[i, j].Imaginary, 1e-9);
                }
            }
        }

        [Test]
        [Row(10)]
        [Row(50)]
        [Row(100)]
        [MultipleAsserts]
        public void CheckRankSquare(int order)
        {
            var matrixA = MatrixLoader.GenerateRandomUserDefinedMatrix(order, order);
            var factorEvd = matrixA.Evd();

            Assert.AreEqual(factorEvd.Rank, order);
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
            var factorEvd = matrixA.Evd();

            Assert.AreEqual(factorEvd.Determinant, 0);
            Assert.AreEqual(factorEvd.Rank, order - 1);
        }

        [Test]
        [Row(1)]
        [Row(10)]
        [Row(100)]
        public void IdentityDeterminantIsOne(int order)
        {
            var I = UserDefinedMatrix.Identity(order);
            var factorEvd = I.Evd();
            Assert.AreEqual(1.0, factorEvd.Determinant);
        }

        [Test]
        [Row(1)]
        [Row(2)]
        [Row(5)]
        [Row(10)]
        [Row(50)]
        [Row(100)]
        [MultipleAsserts]
        public void CanSolveForRandomVectorAndSymmetricMatrix(int order)
        {
            var matrixA = MatrixLoader.GenerateRandomPositiveDefiniteHermitianUserDefinedMatrix(order);
            var matrixACopy = matrixA.Clone();
            var factorSvd = matrixA.Svd(true);

            var vectorb = MatrixLoader.GenerateRandomUserDefinedVector(order);
            var resultx = factorSvd.Solve(vectorb);

            Assert.AreEqual(matrixA.ColumnCount, resultx.Count);

            var bReconstruct = matrixA * resultx;

            // Check the reconstruction.
            for (var i = 0; i < vectorb.Count; i++)
            {
                Assert.AreApproximatelyEqual(vectorb[i].Real, bReconstruct[i].Real, 1e-9);
                Assert.AreApproximatelyEqual(vectorb[i].Imaginary, bReconstruct[i].Imaginary, 1e-9);
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
        public void CanSolveForRandomMatrixAndSymmetricMatrix(int order)
        {
            var matrixA = MatrixLoader.GenerateRandomPositiveDefiniteHermitianUserDefinedMatrix(order);
            var matrixACopy = matrixA.Clone();
            var factorSvd = matrixA.Svd(true);

            var matrixB = MatrixLoader.GenerateRandomUserDefinedMatrix(order, order);
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
                    Assert.AreApproximatelyEqual(matrixB[i, j].Real, matrixBReconstruct[i, j].Real, 1e-9);
                    Assert.AreApproximatelyEqual(matrixB[i, j].Imaginary, matrixBReconstruct[i, j].Imaginary, 1e-9);
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
        public void CanSolveForRandomVectorAndSymmetricMatrixWhenResultVectorGiven(int order)
        {
            var matrixA = MatrixLoader.GenerateRandomPositiveDefiniteHermitianUserDefinedMatrix(order);
            var matrixACopy = matrixA.Clone();
            var factorSvd = matrixA.Svd(true);
            var vectorb = MatrixLoader.GenerateRandomUserDefinedVector(order);
            var vectorbCopy = vectorb.Clone();
            var resultx = new UserDefinedVector(order);
            factorSvd.Solve(vectorb, resultx);

            var bReconstruct = matrixA * resultx;

            // Check the reconstruction.
            for (var i = 0; i < vectorb.Count; i++)
            {
                Assert.AreApproximatelyEqual(vectorb[i].Real, bReconstruct[i].Real, 1e-9);
                Assert.AreApproximatelyEqual(vectorb[i].Imaginary, bReconstruct[i].Imaginary, 1e-9);
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
        [Row(2)]
        [Row(5)]
        [Row(10)]
        [Row(50)]
        [Row(100)]
        [MultipleAsserts]
        public void CanSolveForRandomMatrixAndSymmetricMatrixWhenResultMatrixGiven(int order)
        {
            var matrixA = MatrixLoader.GenerateRandomPositiveDefiniteHermitianUserDefinedMatrix(order);
            var matrixACopy = matrixA.Clone();
            var factorSvd = matrixA.Svd(true);

            var matrixB = MatrixLoader.GenerateRandomUserDefinedMatrix(order, order);
            var matrixBCopy = matrixB.Clone();

            var matrixX = new UserDefinedMatrix(order, order);
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
                    Assert.AreApproximatelyEqual(matrixB[i, j].Real, matrixBReconstruct[i, j].Real, 1e-9);
                    Assert.AreApproximatelyEqual(matrixB[i, j].Imaginary, matrixBReconstruct[i, j].Imaginary, 1e-9);
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
