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

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Double.Decomposition
{
    using System.Collections.Generic;
    using MbUnit.Framework;
    using LinearAlgebra.Double;
    using LinearAlgebra.Double.Decomposition;

    public class CholeskyTests
    {
        [Test]
        [Row(1)]
        [Row(10)]
        [Row(100)]
        [Row(1000)]
        public void CanDecomposeIdentity(int order)
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
        [Row(1000)]
        public void IdentityDeterminantIsOne(int order)
        {
            var I = DenseMatrix.Identity(order);
            var C = I.Cholesky();
            Assert.AreEqual(1.0, C.Determinant);
            Assert.AreEqual(0.0, C.DeterminantLn);
        }
    }
}
