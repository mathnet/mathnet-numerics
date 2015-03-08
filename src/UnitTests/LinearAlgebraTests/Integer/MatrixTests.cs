// <copyright file="MatrixTests.cs" company="Math.NET">
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

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Integer
{
    using System;
    using NUnit.Framework;

    /// <summary>
    /// Abstract class with the common set of matrix tests
    /// </summary>
    public abstract partial class MatrixTests : MatrixLoader
    {
        /// <summary>
        /// Can transpose a matrix.
        /// </summary>
        /// <param name="name">Matrix name.</param>
        [TestCase("Singular3x3")]
        [TestCase("Square3x3")]
        [TestCase("Square4x4")]
        [TestCase("Tall3x2")]
        [TestCase("Wide2x3")]
        public void CanTransposeMatrix(string name)
        {
            var matrix = TestMatrices[name];
            var transpose = matrix.Transpose();

            Assert.AreNotSame(matrix, transpose);
            Assert.AreEqual(matrix.RowCount, transpose.ColumnCount);
            Assert.AreEqual(matrix.ColumnCount, transpose.RowCount);
            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    Assert.AreEqual(matrix[i, j], transpose[j, i]);
                }
            }
        }

        /// <summary>
        /// Can compute Frobenius norm.
        /// </summary>
        [Test]
        public virtual void CanComputeFrobeniusNorm()
        {
            var matrix = TestMatrices["Square3x3"];
            AssertHelpers.AlmostEqualRelative(107.7775486824598f, matrix.FrobeniusNorm(), 6);

            matrix = TestMatrices["Wide2x3"];
            AssertHelpers.AlmostEqualRelative(47.9478883789474f, matrix.FrobeniusNorm(), 6);

            matrix = TestMatrices["Tall3x2"];
            AssertHelpers.AlmostEqualRelative(75.4122006044115f, matrix.FrobeniusNorm(), 6);
        }

        /// <summary>
        /// Can compute Infinity norm.
        /// </summary>
        [Test]
        public virtual void CanComputeInfinityNorm()
        {
            var matrix = TestMatrices["Square3x3"];
            AssertHelpers.AlmostEqualRelative(165f, matrix.InfinityNorm(), 6);

            matrix = TestMatrices["Wide2x3"];
            AssertHelpers.AlmostEqualRelative(66f, matrix.InfinityNorm(), 6);

            matrix = TestMatrices["Tall3x2"];
            AssertHelpers.AlmostEqualRelative(99f, matrix.InfinityNorm(), 6);
        }

        /// <summary>
        /// Can compute L1 norm.
        /// </summary>
        [Test]
        public virtual void CanComputeL1Norm()
        {
            var matrix = TestMatrices["Square3x3"];
            AssertHelpers.AlmostEqualRelative(121f, matrix.L1Norm(), 6);

            matrix = TestMatrices["Wide2x3"];
            AssertHelpers.AlmostEqualRelative(55f, matrix.L1Norm(), 6);

            matrix = TestMatrices["Tall3x2"];
            AssertHelpers.AlmostEqualRelative(88f, matrix.L1Norm(), 6);
        }

        /// <summary>
        /// Can NOT compute L2 norm.
        /// Throws <code>NotSupportedException</code> due to use of Svd internally
        /// EXCEPT for DiagonalMatrix, which CAN compute this value safely!
        /// </summary>
        [Test]
        public virtual void ComputeL2NormThrowsNotSupportedException()
        {
            var matrix = TestMatrices["Square3x3"];
            Assert.Throws<NotSupportedException>(() =>  matrix.L2Norm());
            matrix = TestMatrices["Wide2x3"];
            Assert.Throws<NotSupportedException>(() => matrix.L2Norm());
            matrix = TestMatrices["Tall3x2"];
            Assert.Throws<NotSupportedException>(() => matrix.L2Norm());
        }

        /// <summary>
        /// Can check if a matrix is symmetric.
        /// </summary>
        [Test]
        public virtual void CanCheckIfMatrixIsSymmetric()
        {
            var matrix = TestMatrices["Symmetric3x3"];
            Assert.IsTrue(matrix.IsSymmetric());

            matrix = TestMatrices["Square3x3"];
            Assert.IsFalse(matrix.IsSymmetric());
        }
    }
}
