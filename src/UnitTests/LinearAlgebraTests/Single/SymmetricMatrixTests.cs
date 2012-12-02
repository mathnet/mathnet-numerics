// <copyright file="SymmetricMatrixTests.cs" company="Math.NET">
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

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Single
{
    using MathNet.Numerics.LinearAlgebra.Single;
    using NUnit.Framework;

    /// <summary>
    /// Abstract class with the common set of matrix tests for symmetric matrices. 
    /// </summary>
    public abstract partial class SymmetricMatrixTests : MatrixTests
    {
        /// <summary>
        /// Can check if a matrix is symmetric.
        /// </summary>
        [Test]
        public override void CanCheckIfMatrixIsSymmetric()
        {
            var matrix = TestMatrices["Square3x3"];
            Assert.IsTrue(matrix.IsSymmetric);

            matrix = TestMatrices["NonSymmetric3x3"];
            Assert.IsFalse(matrix.IsSymmetric);
        }

        /// <summary>
        /// Can check if a [,] array is symmetric. 
        /// </summary>
        [Test]
        public void CanCheckIfArrayIsSymmetric()
        {
            Assert.IsTrue(SymmetricMatrix.CheckIfSymmetric(TestData2D["Square3x3"]));
            Assert.IsFalse(SymmetricMatrix.CheckIfSymmetric(TestData2D["NonSymmetric3x3"]));
        }

        /// <summary>
        /// Test whether the index enumerator returns the correct values.
        /// </summary>
        [Test]
        public void CanUseIndexedEnumerator()
        {
            var matrix = TestMatrices["Singular3x3"];
            var enumerator = matrix.IndexedEnumerator().GetEnumerator();
            enumerator.MoveNext();
            var item = enumerator.Current;
            Assert.AreEqual(0, item.Item1);
            Assert.AreEqual(0, item.Item2);
            Assert.AreEqual(1.0, item.Item3);

            enumerator.MoveNext();
            item = enumerator.Current;
            Assert.AreEqual(0, item.Item1);
            Assert.AreEqual(1, item.Item2);
            Assert.AreEqual(2.0, item.Item3);

            enumerator.MoveNext();
            item = enumerator.Current;
            Assert.AreEqual(0, item.Item1);
            Assert.AreEqual(2, item.Item2);
            Assert.AreEqual(3.0, item.Item3);

            enumerator.MoveNext();
            item = enumerator.Current;
            Assert.AreEqual(1, item.Item1);
            Assert.AreEqual(0, item.Item2);
            Assert.AreEqual(2.0, item.Item3);

            enumerator.MoveNext();
            item = enumerator.Current;
            Assert.AreEqual(1, item.Item1);
            Assert.AreEqual(1, item.Item2);
            Assert.AreEqual(0.0, item.Item3);

            enumerator.MoveNext();
            item = enumerator.Current;
            Assert.AreEqual(1, item.Item1);
            Assert.AreEqual(2, item.Item2);
            Assert.AreEqual(0.0, item.Item3);

            enumerator.MoveNext();
            item = enumerator.Current;
            Assert.AreEqual(2, item.Item1);
            Assert.AreEqual(0, item.Item2);
            Assert.AreEqual(3.0, item.Item3);

            enumerator.MoveNext();
            item = enumerator.Current;
            Assert.AreEqual(2, item.Item1);
            Assert.AreEqual(1, item.Item2);
            Assert.AreEqual(0.0, item.Item3);

            enumerator.MoveNext();
            item = enumerator.Current;
            Assert.AreEqual(2, item.Item1);
            Assert.AreEqual(2, item.Item2);
            Assert.AreEqual(0.0, item.Item3);
        }
    }
}