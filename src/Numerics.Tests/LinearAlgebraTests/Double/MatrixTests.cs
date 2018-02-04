// <copyright file="MatrixTests.cs" company="Math.NET">
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
using NUnit.Framework;

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Double
{
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
        /// Can transpose a matrix into a result.
        /// </summary>
        /// <param name="name">Matrix name.</param>
        [TestCase("Singular3x3")]
        [TestCase("Square3x3")]
        [TestCase("Square4x4")]
        [TestCase("Tall3x2")]
        [TestCase("Wide2x3")]
        public void CanTransposeMatrixResult(string name)
        {
            var matrix = TestMatrices[name];
            var transpose = Matrix<double>.Build.SameAs(matrix, matrix.ColumnCount, matrix.RowCount);
            matrix.Transpose(transpose);

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
        /// Can transpose a square matrix inplace.
        /// </summary>
        /// <param name="name">Matrix name.</param>
        [TestCase("Square4x4")]
        public void CanTransposeSquareMatrixInplace(string name)
        {
            var matrix = TestMatrices[name];
            var transpose = matrix.Clone();
            transpose.Transpose(transpose);

            Assert.AreNotSame(matrix, transpose);
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
            AssertHelpers.AlmostEqualRelative(10.77775486824598, matrix.FrobeniusNorm(), 7);

            matrix = TestMatrices["Wide2x3"];
            AssertHelpers.AlmostEqualRelative(4.79478883789474, matrix.FrobeniusNorm(), 7);

            matrix = TestMatrices["Tall3x2"];
            AssertHelpers.AlmostEqualRelative(7.54122006044115, matrix.FrobeniusNorm(), 7);
        }

        /// <summary>
        /// Can compute Infinity norm.
        /// </summary>
        [Test]
        public virtual void CanComputeInfinityNorm()
        {
            var matrix = TestMatrices["Square3x3"];
            Assert.AreEqual(16.5, matrix.InfinityNorm());

            matrix = TestMatrices["Wide2x3"];
            Assert.AreEqual(6.6, matrix.InfinityNorm(), 1e-7);

            matrix = TestMatrices["Tall3x2"];
            Assert.AreEqual(9.9, matrix.InfinityNorm(), 1e-4);
        }

        /// <summary>
        /// Can compute L1 norm.
        /// </summary>
        [Test]
        public virtual void CanComputeL1Norm()
        {
            var matrix = TestMatrices["Square3x3"];
            Assert.AreEqual(12.1, matrix.L1Norm(), 1e-4);

            matrix = TestMatrices["Wide2x3"];
            Assert.AreEqual(5.5, matrix.L1Norm());

            matrix = TestMatrices["Tall3x2"];
            Assert.AreEqual(8.8, matrix.L1Norm(), 1e-4);
        }

        /// <summary>
        /// Can compute L2 norm.
        /// </summary>
        [Test]
        public virtual void CanComputeL2Norm()
        {
            var matrix = TestMatrices["Square3x3"];
            AssertHelpers.AlmostEqualRelative(10.391347375312632, matrix.L2Norm(), 14);

            matrix = TestMatrices["Wide2x3"];
            AssertHelpers.AlmostEqualRelative(4.7540849434107635, matrix.L2Norm(), 14);
            matrix = TestMatrices["Tall3x2"];
            AssertHelpers.AlmostEqualRelative(7.182727033856683, matrix.L2Norm(), 14);
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
