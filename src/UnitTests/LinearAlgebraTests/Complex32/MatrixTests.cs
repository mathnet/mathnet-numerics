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

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Complex32
{
    using System;
    using LinearAlgebra.Generic;
    using NUnit.Framework;
    using Complex32 = Numerics.Complex32;

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
            var matrix = CreateMatrix(TestData2D[name]);
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
        /// Can conjugate transpose a matrix.
        /// </summary>
        /// <param name="name">Matrix name.</param>
        [TestCase("Singular3x3")]
        [TestCase("Square3x3")]
        [TestCase("Square4x4")]
        [TestCase("Tall3x2")]
        [TestCase("Wide2x3")]
        public void CanConjugateTransposeMatrix(string name)
        {
            var matrix = CreateMatrix(TestData2D[name]);
            var transpose = matrix.ConjugateTranspose();

            Assert.AreNotSame(matrix, transpose);
            Assert.AreEqual(matrix.RowCount, transpose.ColumnCount);
            Assert.AreEqual(matrix.ColumnCount, transpose.RowCount);
            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    Assert.AreEqual(matrix[i, j], transpose[j, i].Conjugate());
                }
            }
        }

        /// <summary>
        /// Can convert a matrix to a multidimensional array.
        /// </summary>
        [Test]
        public void CanToArray()
        {
            foreach (var data in TestMatrices.Values)
            {
                var array = data.ToArray();
                Assert.AreEqual(data.RowCount, array.GetLength(0));
                Assert.AreEqual(data.ColumnCount, array.GetLength(1));

                for (var i = 0; i < data.RowCount; i++)
                {
                    for (var j = 0; j < data.ColumnCount; j++)
                    {
                        Assert.AreEqual(data[i, j], array[i, j]);
                    }
                }
            }
        }

        /// <summary>
        /// Can convert a matrix to a column-wise array.
        /// </summary>
        [Test]
        public void CanToColumnWiseArray()
        {
            foreach (var data in TestMatrices.Values)
            {
                var array = data.ToColumnWiseArray();
                Assert.AreEqual(data.RowCount * data.ColumnCount, array.Length);

                for (var i = 0; i < data.RowCount; i++)
                {
                    for (var j = 0; j < data.ColumnCount; j++)
                    {
                        Assert.AreEqual(data[i, j], array[(j * data.RowCount) + i]);
                    }
                }
            }
        }

        /// <summary>
        /// Can convert a matrix to a row-wise array.
        /// </summary>
        [Test]
        public void CanToRowWiseArray()
        {
            foreach (var data in TestMatrices.Values)
            {
                var array = data.ToRowWiseArray();
                Assert.AreEqual(data.RowCount * data.ColumnCount, array.Length);

                for (var i = 0; i < data.RowCount; i++)
                {
                    for (var j = 0; j < data.ColumnCount; j++)
                    {
                        Assert.AreEqual(data[i, j], array[(i * data.ColumnCount) + j]);
                    }
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
            AssertHelpers.AlmostEqual(10.8819655f, matrix.FrobeniusNorm().Real, 7);

            matrix = TestMatrices["Wide2x3"];
            AssertHelpers.AlmostEqual(5.1905256f, matrix.FrobeniusNorm().Real, 7);

            matrix = TestMatrices["Tall3x2"];
            AssertHelpers.AlmostEqual(7.5904115f, matrix.FrobeniusNorm().Real, 7);
        }

        /// <summary>
        /// Can compute Infinity norm.
        /// </summary>
        [Test]
        public virtual void CanComputeInfinityNorm()
        {
            var matrix = TestMatrices["Square3x3"];
            AssertHelpers.AlmostEqual(16.7777033f, matrix.InfinityNorm().Real, 6);

            matrix = TestMatrices["Wide2x3"];
            AssertHelpers.AlmostEqual(7.3514039f, matrix.InfinityNorm().Real, 6);

            matrix = TestMatrices["Tall3x2"];
            AssertHelpers.AlmostEqual(10.1023756f, matrix.InfinityNorm().Real, 6);
        }

        /// <summary>
        /// Can compute L1 norm.
        /// </summary>
        [Test]
        public virtual void CanComputeL1Norm()
        {
            var matrix = TestMatrices["Square3x3"];
            AssertHelpers.AlmostEqual(12.5401248f, matrix.L1Norm().Real, 7);

            matrix = TestMatrices["Wide2x3"];
            AssertHelpers.AlmostEqual(5.8647971f, matrix.L1Norm().Real, 7);

            matrix = TestMatrices["Tall3x2"];
            AssertHelpers.AlmostEqual(9.4933860f, matrix.L1Norm().Real, 7);
        }

        /// <summary>
        /// Can compute L2 norm.
        /// </summary>
        [Test]
        public virtual void CanComputeL2Norm()
        {
            var matrix = TestMatrices["Square3x3"];
            AssertHelpers.AlmostEqual(10.6381752f, matrix.L2Norm().Real, 6);

            matrix = TestMatrices["Wide2x3"];
            AssertHelpers.AlmostEqual(5.2058554f, matrix.L2Norm().Real, 6);
            matrix = TestMatrices["Tall3x2"];
            AssertHelpers.AlmostEqual(7.3582664f, matrix.L2Norm().Real, 6);
        }

        /// <summary>
        /// Test whether the index enumerator returns the correct values.
        /// </summary>
        [Test]
        public virtual void CanUseIndexedEnumerator()
        {
            var matrix = TestMatrices["Singular3x3"];
            using (var enumerator = matrix.IndexedEnumerator().GetEnumerator())
            {
                enumerator.MoveNext();
                var item = enumerator.Current;
                Assert.AreEqual(0, item.Item1);
                Assert.AreEqual(0, item.Item2);
                Assert.AreEqual(new Complex32(1.0f, 1.0f), item.Item3);

                enumerator.MoveNext();
                item = enumerator.Current;
                Assert.AreEqual(0, item.Item1);
                Assert.AreEqual(1, item.Item2);
                Assert.AreEqual(new Complex32(1.0f, 1.0f), item.Item3);

                enumerator.MoveNext();
                item = enumerator.Current;
                Assert.AreEqual(0, item.Item1);
                Assert.AreEqual(2, item.Item2);
                Assert.AreEqual(new Complex32(2.0f, 1.0f), item.Item3);

                enumerator.MoveNext();
                item = enumerator.Current;
                Assert.AreEqual(1, item.Item1);
                Assert.AreEqual(0, item.Item2);
                Assert.AreEqual(new Complex32(1.0f, 1.0f), item.Item3);

                enumerator.MoveNext();
                item = enumerator.Current;
                Assert.AreEqual(1, item.Item1);
                Assert.AreEqual(1, item.Item2);
                Assert.AreEqual(new Complex32(1.0f, 1.0f), item.Item3);

                enumerator.MoveNext();
                item = enumerator.Current;
                Assert.AreEqual(1, item.Item1);
                Assert.AreEqual(2, item.Item2);
                Assert.AreEqual(new Complex32(2.0f, 1.0f), item.Item3);

                enumerator.MoveNext();
                item = enumerator.Current;
                Assert.AreEqual(2, item.Item1);
                Assert.AreEqual(0, item.Item2);
                Assert.AreEqual(new Complex32(1.0f, 1.0f), item.Item3);

                enumerator.MoveNext();
                item = enumerator.Current;
                Assert.AreEqual(2, item.Item1);
                Assert.AreEqual(1, item.Item2);
                Assert.AreEqual(new Complex32(1.0f, 1.0f), item.Item3);

                enumerator.MoveNext();
                item = enumerator.Current;
                Assert.AreEqual(2, item.Item1);
                Assert.AreEqual(2, item.Item2);
                Assert.AreEqual(new Complex32(2.0f, 1.0f), item.Item3);
            }
        }

        /// <summary>
        /// Can check if a matrix is symmetric.
        /// </summary>
        [Test]
        public virtual void CanCheckIfMatrixIsSymmetric()
        {
            var matrix = TestMatrices["Symmetric3x3"];
            Assert.IsTrue(matrix.IsSymmetric);

            matrix = TestMatrices["Square3x3"];
            Assert.IsFalse(matrix.IsSymmetric);
        }

        /// <summary>
        /// Test whether we can create a matrix from a list of column vectors.
        /// </summary>
        [Test]
        public virtual void CanCreateMatrixFromColumns()
        {
            var column1 = CreateVector(new Complex32[] { 1.0f });
            var column2 = CreateVector(new Complex32[] { 1.0f, 2.0f, 3.0f, 4.0f });
            var column3 = CreateVector(new Complex32[] { 1.0f, 2.0f });
            var columnVectors = new System.Collections.Generic.List<Vector<Complex32>>
                                {
                                    column1,
                                    column2,
                                    column3
                                };
            var matrix = Matrix<Complex32>.CreateFromColumns(columnVectors);

            Assert.AreEqual(matrix.RowCount, 4);
            Assert.AreEqual(matrix.ColumnCount, 3);
            Assert.AreEqual(1.0, matrix[0, 0].Real);
            Assert.AreEqual(0.0, matrix[1, 0].Real);
            Assert.AreEqual(0.0, matrix[2, 0].Real);
            Assert.AreEqual(0.0, matrix[3, 0].Real);
            Assert.AreEqual(1.0, matrix[0, 1].Real);
            Assert.AreEqual(2.0, matrix[1, 1].Real);
            Assert.AreEqual(3.0, matrix[2, 1].Real);
            Assert.AreEqual(4.0, matrix[3, 1].Real);
            Assert.AreEqual(1.0, matrix[0, 2].Real);
            Assert.AreEqual(2.0, matrix[1, 2].Real);
            Assert.AreEqual(0.0, matrix[2, 2].Real);
            Assert.AreEqual(0.0, matrix[3, 2].Real);
        }

        /// <summary>
        /// Test whether we can create a matrix from a list of row vectors.
        /// </summary>
        [Test]
        public virtual void CanCreateMatrixFromRows()
        {
            var row1 = CreateVector(new Complex32[] { 1.0f });
            var row2 = CreateVector(new Complex32[] { 1.0f, 2.0f, 3.0f, 4.0f });
            var row3 = CreateVector(new Complex32[] { 1.0f, 2.0f });
            var rowVectors = new System.Collections.Generic.List<Vector<Complex32>>
                                {
                                    row1,
                                    row2,
                                    row3
                                };
            var matrix = Matrix<Complex32>.CreateFromRows(rowVectors);

            Assert.AreEqual(matrix.RowCount, 3);
            Assert.AreEqual(matrix.ColumnCount, 4);
            Assert.AreEqual(1.0, matrix[0, 0].Real);
            Assert.AreEqual(0.0, matrix[0, 1].Real);
            Assert.AreEqual(0.0, matrix[0, 2].Real);
            Assert.AreEqual(0.0, matrix[0, 3].Real);
            Assert.AreEqual(1.0, matrix[1, 0].Real);
            Assert.AreEqual(2.0, matrix[1, 1].Real);
            Assert.AreEqual(3.0, matrix[1, 2].Real);
            Assert.AreEqual(4.0, matrix[1, 3].Real);
            Assert.AreEqual(1.0, matrix[2, 0].Real);
            Assert.AreEqual(2.0, matrix[2, 1].Real);
            Assert.AreEqual(0.0, matrix[2, 2].Real);
            Assert.AreEqual(0.0, matrix[2, 3].Real);
        }
    }
}
