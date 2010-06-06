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

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Double.Factorization
{
    using System.Collections.Generic;
    using MbUnit.Framework;
    using LinearAlgebra.Double;
    using LinearAlgebra.Double.Factorization;

    public class LUTests
    {
        [Test]
        [Row(1)]
        [Row(10)]
        [Row(100)]
        public void CanFactorizeIdentity(int order)
        {
            var I = DenseMatrix.Identity(order);
            var lu = I.LU();

            // Check lower triangular part.
            var L = lu.L;
            Assert.AreEqual(I.RowCount, L.RowCount);
            Assert.AreEqual(I.ColumnCount, L.ColumnCount);
            for (var i = 0; i < L.RowCount; i++)
            {
                for (var j = 0; j < L.ColumnCount; j++)
                {
                    if (i == j)
                    {
                        Assert.AreEqual(1.0, L[i, j]);
                    }
                    else
                    {
                        Assert.AreEqual(0.0, L[i, j]);
                    }
                }
            }

            // Check upper triangular part.
            var U = lu.U;
            Assert.AreEqual(I.RowCount, U.RowCount);
            Assert.AreEqual(I.ColumnCount, U.ColumnCount);
            for (var i = 0; i < U.RowCount; i++)
            {
                for (var j = 0; j < U.ColumnCount; j++)
                {
                    if (i == j)
                    {
                        Assert.AreEqual(1.0, U[i, j]);
                    }
                    else
                    {
                        Assert.AreEqual(0.0, U[i, j]);
                    }
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
            var lu = I.LU();
        }

        [Test]
        [Row(1)]
        [Row(10)]
        [Row(100)]
        public void IdentityDeterminantIsOne(int order)
        {
            var I = DenseMatrix.Identity(order);
            var lu = I.LU();
            Assert.AreEqual(1.0, lu.Determinant);
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
            var X = MatrixLoader.GenerateRandomMatrix(order, order);
            var lu = X.LU();
            var L = lu.L;
            var U = lu.U;

            // Make sure the factors have the right dimensions.
            Assert.AreEqual(order, L.RowCount);
            Assert.AreEqual(order, L.ColumnCount);
            Assert.AreEqual(order, U.RowCount);
            Assert.AreEqual(order, U.ColumnCount);

            // Make sure the L factor is lower triangular.
            for (int i = 0; i < L.RowCount; i++) 
            {
                Assert.AreEqual(1.0, L[i, i]);
                for (int j = i+1; j < L.ColumnCount; j++)
                {
                    Assert.AreEqual(0.0, L[i, j]);
                }
            }

            // Make sure the U factor is upper triangular.
            for (int i = 0; i < L.RowCount; i++)
            {
                for (int j = 0; j < i; j++)
                {
                    Assert.AreEqual(0.0, U[i, j]);
                }
            }

            // Make sure the cholesky factor times it's transpose is the original matrix.
            var XfromLU = L * U;
            var Pinv = lu.P.Inverse();
            XfromLU.PermuteRows(Pinv);
            for (int i = 0; i < XfromLU.RowCount; i++) 
            {
                for (int j = 0; j < XfromLU.ColumnCount; j++)
                {
                    Assert.AreApproximatelyEqual(X[i, j], XfromLU[i, j], 1.0e-11);
                }
            }
        }
    }
}
