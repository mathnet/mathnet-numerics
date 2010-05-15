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
            // Fill a matrix with standard random numbers.
            var normal = new Distributions.Normal();
            normal.RandomSource = new Random.MersenneTwister(1);
            var A = new DenseMatrix(order);
            for (int i = 0; i < order; i++)
            {
                for (int j = 0; j < order; j++)
                {
                    A[i, j] = normal.Sample();
                }
            }

            // Generate a matrix which is positive definite.
            var X = A.Transpose() * A;
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
                    Assert.AreApproximatelyEqual(X[i,j], XfromC[i, j], 1.0e-13);
                }
            }
        }
    }
}
