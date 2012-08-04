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

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Double
{
    using System;
    using LinearAlgebra.Generic;
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
        /// Can permute matrix rows.
        /// </summary>
        /// <param name="name">Matrix name.</param>
        [TestCase("Singular3x3")]
        [TestCase("Square3x3")]
        [TestCase("Tall3x2")]
        public virtual void CanPermuteMatrixRows(string name)
        {
            var matrix = CreateMatrix(TestData2D[name]);
            var matrixp = CreateMatrix(TestData2D[name]);

            var permutation = new Permutation(new[] { 2, 0, 1 });
            matrixp.PermuteRows(permutation);

            Assert.AreNotSame(matrix, matrixp);
            Assert.AreEqual(matrix.RowCount, matrixp.RowCount);
            Assert.AreEqual(matrix.ColumnCount, matrixp.ColumnCount);
            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    Assert.AreEqual(matrix[i, j], matrixp[permutation[i], j]);
                }
            }
        }

        /// <summary>
        /// Can permute matrix columns.
        /// </summary>
        /// <param name="name">Matrix name.</param>
        [TestCase("Singular3x3")]
        [TestCase("Square3x3")]
        [TestCase("Wide2x3")]
        public virtual void CanPermuteMatrixColumns(string name)
        {
            var matrix = CreateMatrix(TestData2D[name]);
            var matrixp = CreateMatrix(TestData2D[name]);

            var permutation = new Permutation(new[] { 2, 0, 1 });
            matrixp.PermuteColumns(permutation);

            Assert.AreNotSame(matrix, matrixp);
            Assert.AreEqual(matrix.RowCount, matrixp.RowCount);
            Assert.AreEqual(matrix.ColumnCount, matrixp.ColumnCount);
            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    Assert.AreEqual(matrix[i, j], matrixp[i, permutation[j]]);
                }
            }
        }

        /// <summary>
        /// Can append matrices.
        /// </summary>
        [Test]
        public void CanAppendMatrices()
        {
            var left = CreateMatrix(TestData2D["Singular3x3"]);
            var right = CreateMatrix(TestData2D["Tall3x2"]);
            var result = left.Append(right);
            Assert.AreEqual(left.ColumnCount + right.ColumnCount, result.ColumnCount);
            Assert.AreEqual(left.RowCount, right.RowCount);

            for (var i = 0; i < result.RowCount; i++)
            {
                for (var j = 0; j < result.ColumnCount; j++)
                {
                    Assert.AreEqual(j < left.ColumnCount ? left[i, j] : right[i, j - left.ColumnCount], result[i, j]);
                }
            }
        }

        /// <summary>
        /// Append with right <c>null</c> throws <c>ArgumentNullException</c>.
        /// </summary>
        [Test]
        public void AppendWithRightNullThrowsArgumentNullException()
        {
            var left = TestMatrices["Square3x3"];
            Matrix<double> right = null;
            Assert.Throws<ArgumentNullException>(() => left.Append(right));
        }

        /// <summary>
        /// Append with result <c>null</c> throws <c>ArgumentNullException</c>.
        /// </summary>
        [Test]
        public void AppendWithResultNullThrowsArgumentNullException()
        {
            var left = TestMatrices["Square3x3"];
            var right = TestMatrices["Tall3x2"];
            Matrix<double> result = null;
            Assert.Throws<ArgumentNullException>(() => left.Append(right, result));
        }

        /// <summary>
        /// Appending two matrices with different row count throws <c>ArgumentException</c>.
        /// </summary>
        [Test]
        public void AppendWithDifferentRowCountThrowsArgumentException()
        {
            var left = TestMatrices["Square3x3"];
            var right = TestMatrices["Wide2x3"];
            Assert.Throws<ArgumentException>(() => { var result = left.Append(right); });
        }

        /// <summary>
        /// Appending with invalid result matrix columns throws <c>ArgumentException</c>.
        /// </summary>
        [Test]
        public void AppendWithInvalidResultMatrixColumnsThrowsArgumentException()
        {
            var left = TestMatrices["Square3x3"];
            var right = TestMatrices["Tall3x2"];
            var result = CreateMatrix(3, 2);
            Assert.Throws<ArgumentException>(() => left.Append(right, result));
        }

        /// <summary>
        /// Can stack matrices.
        /// </summary>
        [Test]
        public void CanStackMatrices()
        {
            var top = TestMatrices["Square3x3"];
            var bottom = TestMatrices["Wide2x3"];
            var result = top.Stack(bottom);
            Assert.AreEqual(top.RowCount + bottom.RowCount, result.RowCount);
            Assert.AreEqual(top.ColumnCount, result.ColumnCount);

            for (var i = 0; i < result.RowCount; i++)
            {
                for (var j = 0; j < result.ColumnCount; j++)
                {
                    Assert.AreEqual(result[i, j], i < top.RowCount ? top[i, j] : bottom[i - top.RowCount, j]);
                }
            }
        }

        /// <summary>
        /// Stacking with a bottom <c>null</c> throws <c>ArgumentNullException</c>.
        /// </summary>
        [Test]
        public void StackWithBottomNullThrowsArgumentNullException()
        {
            var top = TestMatrices["Square3x3"];
            Matrix<double> bottom = null;
            var result = CreateMatrix(top.RowCount + top.RowCount, top.ColumnCount);
            Assert.Throws<ArgumentNullException>(() => top.Stack(bottom, result));
        }

        /// <summary>
        /// Stacking with a result <c>null</c> throws <c>ArgumentNullException</c>.
        /// </summary>
        [Test]
        public void StackWithResultNullThrowsArgumentNullException()
        {
            var top = TestMatrices["Square3x3"];
            var bottom = TestMatrices["Square3x3"];
            Matrix<double> result = null;
            Assert.Throws<ArgumentNullException>(() => top.Stack(bottom, result));
        }

        /// <summary>
        /// Stacking matrices with different columns throws <c>ArgumentException</c>.
        /// </summary>
        [Test]
        public void StackMatricesWithDifferentColumnsThrowsArgumentException()
        {
            var top = TestMatrices["Square3x3"];
            var lower = TestMatrices["Tall3x2"];
            var result = CreateMatrix(top.RowCount + lower.RowCount, top.ColumnCount);
            Assert.Throws<ArgumentException>(() => top.Stack(lower, result));
        }

        /// <summary>
        /// Stacking with invalid result matrix rows throws <c>ArgumentException</c>.
        /// </summary>
        [Test]
        public void StackWithInvalidResultMatrixRowsThrowsArgumentException()
        {
            var top = TestMatrices["Square3x3"];
            var bottom = TestMatrices["Wide2x3"];
            var result = CreateMatrix(1, 3);
            Assert.Throws<ArgumentException>(() => top.Stack(bottom, result));
        }

        /// <summary>
        /// Can diagonally stack matrices.
        /// </summary>
        [Test]
        public void CanDiagonallyStackMatrices()
        {
            var top = TestMatrices["Tall3x2"];
            var bottom = TestMatrices["Wide2x3"];
            var result = top.DiagonalStack(bottom);
            Assert.AreEqual(top.RowCount + bottom.RowCount, result.RowCount);
            Assert.AreEqual(top.ColumnCount + bottom.ColumnCount, result.ColumnCount);

            for (var i = 0; i < result.RowCount; i++)
            {
                for (var j = 0; j < result.ColumnCount; j++)
                {
                    if (i < top.RowCount && j < top.ColumnCount)
                    {
                        Assert.AreEqual(top[i, j], result[i, j]);
                    }
                    else if (i >= top.RowCount && j >= top.ColumnCount)
                    {
                        Assert.AreEqual(bottom[i - top.RowCount, j - top.ColumnCount], result[i, j]);
                    }
                    else
                    {
                        Assert.AreEqual(0, result[i, j]);
                    }
                }
            }
        }

        /// <summary>
        /// Diagonal stack with lower <c>null</c> throws <c>ArgumentNullException</c>.
        /// </summary>
        [Test]
        public void DiagonalStackWithLowerNullThrowsArgumentNullException()
        {
            var top = TestMatrices["Square3x3"];
            Matrix<double> lower = null;
            Assert.Throws<ArgumentNullException>(() => top.DiagonalStack(lower));
        }

        /// <summary>
        /// Can diagonally stack matrices into a result matrix.
        /// </summary>
        [Test]
        public virtual void CanDiagonallyStackMatricesIntoResult()
        {
            var top = TestMatrices["Tall3x2"];
            var bottom = TestMatrices["Wide2x3"];
            var result = CreateMatrix(top.RowCount + bottom.RowCount, top.ColumnCount + bottom.ColumnCount);
            top.DiagonalStack(bottom, result);
            Assert.AreEqual(top.RowCount + bottom.RowCount, result.RowCount);
            Assert.AreEqual(top.ColumnCount + bottom.ColumnCount, result.ColumnCount);

            for (var i = 0; i < result.RowCount; i++)
            {
                for (var j = 0; j < result.ColumnCount; j++)
                {
                    if (i < top.RowCount && j < top.ColumnCount)
                    {
                        Assert.AreEqual(top[i, j], result[i, j]);
                    }
                    else if (i >= top.RowCount && j >= top.ColumnCount)
                    {
                        Assert.AreEqual(bottom[i - top.RowCount, j - top.ColumnCount], result[i, j]);
                    }
                    else
                    {
                        Assert.AreEqual(0, result[i, j]);
                    }
                }
            }
        }

        /// <summary>
        /// Diagonal stack into <c>null</c> result throws <c>ArgumentNullException</c>.
        /// </summary>
        [Test]
        public void DiagonalStackIntoResultNullThrowsArgumentNullException()
        {
            var top = TestMatrices["Square3x3"];
            var lower = TestMatrices["Wide2x3"];
            Matrix<double> result = null;
            Assert.Throws<ArgumentNullException>(() => top.DiagonalStack(lower, result));
        }

        /// <summary>
        /// Diagonal stack into invalid result matrix throws <c>ArgumentException</c>.
        /// </summary>
        [Test]
        public void DiagonalStackIntoInvalidResultMatrixThrowsArgumentException()
        {
            var top = TestMatrices["Square3x3"];
            var lower = TestMatrices["Wide2x3"];
            var result = CreateMatrix(top.RowCount + lower.RowCount + 2, top.ColumnCount + lower.ColumnCount);
            Assert.Throws<ArgumentException>(() => top.DiagonalStack(lower, result));
        }

        /// <summary>
        /// Can compute Frobenius norm.
        /// </summary>
        [Test]
        public virtual void CanComputeFrobeniusNorm()
        {
            var matrix = TestMatrices["Square3x3"];
            AssertHelpers.AlmostEqual(10.77775486824598, matrix.FrobeniusNorm(), 14);

            matrix = TestMatrices["Wide2x3"];
            AssertHelpers.AlmostEqual(4.79478883789474, matrix.FrobeniusNorm(), 14);

            matrix = TestMatrices["Tall3x2"];
            AssertHelpers.AlmostEqual(7.54122006044115, matrix.FrobeniusNorm(), 14);
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
            Assert.AreEqual(6.6, matrix.InfinityNorm());

            matrix = TestMatrices["Tall3x2"];
            Assert.AreEqual(9.9, matrix.InfinityNorm());
        }

        /// <summary>
        /// Can compute L1 norm.
        /// </summary>
        [Test]
        public virtual void CanComputeL1Norm()
        {
            var matrix = TestMatrices["Square3x3"];
            Assert.AreEqual(12.1, matrix.L1Norm());

            matrix = TestMatrices["Wide2x3"];
            Assert.AreEqual(5.5, matrix.L1Norm());

            matrix = TestMatrices["Tall3x2"];
            Assert.AreEqual(8.8, matrix.L1Norm());
        }

        /// <summary>
        /// Can compute L2 norm.
        /// </summary>
        [Test]
        public virtual void CanComputeL2Norm()
        {
            var matrix = TestMatrices["Square3x3"];
            AssertHelpers.AlmostEqual(10.391347375312632, matrix.L2Norm(), 14);

            matrix = TestMatrices["Wide2x3"];
            AssertHelpers.AlmostEqual(4.7540849434107635, matrix.L2Norm(), 14);
            matrix = TestMatrices["Tall3x2"];
            AssertHelpers.AlmostEqual(7.182727033856683, matrix.L2Norm(), 14);
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
                Assert.AreEqual(1.0, item.Item3);

                enumerator.MoveNext();
                item = enumerator.Current;
                Assert.AreEqual(0, item.Item1);
                Assert.AreEqual(1, item.Item2);
                Assert.AreEqual(1.0, item.Item3);

                enumerator.MoveNext();
                item = enumerator.Current;
                Assert.AreEqual(0, item.Item1);
                Assert.AreEqual(2, item.Item2);
                Assert.AreEqual(2.0, item.Item3);

                enumerator.MoveNext();
                item = enumerator.Current;
                Assert.AreEqual(1, item.Item1);
                Assert.AreEqual(0, item.Item2);
                Assert.AreEqual(1.0, item.Item3);

                enumerator.MoveNext();
                item = enumerator.Current;
                Assert.AreEqual(1, item.Item1);
                Assert.AreEqual(1, item.Item2);
                Assert.AreEqual(1.0, item.Item3);

                enumerator.MoveNext();
                item = enumerator.Current;
                Assert.AreEqual(1, item.Item1);
                Assert.AreEqual(2, item.Item2);
                Assert.AreEqual(2.0, item.Item3);

                enumerator.MoveNext();
                item = enumerator.Current;
                Assert.AreEqual(2, item.Item1);
                Assert.AreEqual(0, item.Item2);
                Assert.AreEqual(1.0, item.Item3);

                enumerator.MoveNext();
                item = enumerator.Current;
                Assert.AreEqual(2, item.Item1);
                Assert.AreEqual(1, item.Item2);
                Assert.AreEqual(1.0, item.Item3);

                enumerator.MoveNext();
                item = enumerator.Current;
                Assert.AreEqual(2, item.Item1);
                Assert.AreEqual(2, item.Item2);
                Assert.AreEqual(2.0, item.Item3);
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
            var column1 = CreateVector(new[] { 1.0 });
            var column2 = CreateVector(new[] { 1.0, 2.0, 3.0, 4.0 });
            var column3 = CreateVector(new[] { 1.0, 2.0 });
            var columnVectors = new System.Collections.Generic.List<Vector<double>>
                                {
                                    column1,
                                    column2,
                                    column3
                                };
            var matrix = Matrix<double>.CreateFromColumns(columnVectors);

            Assert.AreEqual(matrix.RowCount, 4);
            Assert.AreEqual(matrix.ColumnCount, 3);
            Assert.AreEqual(1.0, matrix[0, 0]);
            Assert.AreEqual(0.0, matrix[1, 0]);
            Assert.AreEqual(0.0, matrix[2, 0]);
            Assert.AreEqual(0.0, matrix[3, 0]);
            Assert.AreEqual(1.0, matrix[0, 1]);
            Assert.AreEqual(2.0, matrix[1, 1]);
            Assert.AreEqual(3.0, matrix[2, 1]);
            Assert.AreEqual(4.0, matrix[3, 1]);
            Assert.AreEqual(1.0, matrix[0, 2]);
            Assert.AreEqual(2.0, matrix[1, 2]);
            Assert.AreEqual(0.0, matrix[2, 2]);
            Assert.AreEqual(0.0, matrix[3, 2]);
        }

        /// <summary>
        /// Test whether we can create a matrix from a list of row vectors.
        /// </summary>
        [Test]
        public virtual void CanCreateMatrixFromRows()
        {
            var row1 = CreateVector(new[] { 1.0 });
            var row2 = CreateVector(new[] { 1.0, 2.0, 3.0, 4.0 });
            var row3 = CreateVector(new[] { 1.0, 2.0 });
            var rowVectors = new System.Collections.Generic.List<Vector<double>>
                                {
                                    row1,
                                    row2,
                                    row3
                                };
            var matrix = Matrix<double>.CreateFromRows(rowVectors);

            Assert.AreEqual(matrix.RowCount, 3);
            Assert.AreEqual(matrix.ColumnCount, 4);
            Assert.AreEqual(1.0, matrix[0, 0]);
            Assert.AreEqual(0.0, matrix[0, 1]);
            Assert.AreEqual(0.0, matrix[0, 2]);
            Assert.AreEqual(0.0, matrix[0, 3]);
            Assert.AreEqual(1.0, matrix[1, 0]);
            Assert.AreEqual(2.0, matrix[1, 1]);
            Assert.AreEqual(3.0, matrix[1, 2]);
            Assert.AreEqual(4.0, matrix[1, 3]);
            Assert.AreEqual(1.0, matrix[2, 0]);
            Assert.AreEqual(2.0, matrix[2, 1]);
            Assert.AreEqual(0.0, matrix[2, 2]);
            Assert.AreEqual(0.0, matrix[2, 3]);
        }
    }
}
