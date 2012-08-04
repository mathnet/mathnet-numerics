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

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Complex
{
    using System;
    using System.Numerics;
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
        /// Can insert a column.
        /// </summary>
        [Test]
        public void CanInsertColumn()
        {
            var matrix = CreateMatrix(3, 3);
            var column = CreateVector(matrix.RowCount);
            for (var i = 0; i < column.Count; i++)
            {
                column[i] = i;
            }

            for (var k = 0; k < matrix.ColumnCount + 1; k++)
            {
                var result = matrix.InsertColumn(k, column);
                Assert.AreEqual(result.ColumnCount, matrix.ColumnCount + 1);
                for (var col = 0; col < result.ColumnCount; col++)
                {
                    for (var row = 0; row < result.RowCount; row++)
                    {
                        AssertHelpers.AreEqual(col == k ? column[row] : 0, result[row, col]);
                    }
                }
            }
        }

        /// <summary>
        /// Insert <c>null</c> column throws <c>ArgumentNullException</c>.
        /// </summary>
        [Test]
        public void InsertNullColumnThrowsArgumentNullException()
        {
            var matrix = TestMatrices["Square3x3"];
            Assert.Throws<ArgumentNullException>(() => matrix.InsertColumn(0, null));
        }

        /// <summary>
        /// Insert a column with invalid column index throws <c>ArgumentOutOfRangeException</c>.
        /// </summary>
        [Test]
        public void InsertColumnWithInvalidColumnIndexThrowsArgumentOutOfRangeException()
        {
            var matrix = CreateMatrix(3, 3);
            var column = CreateVector(matrix.RowCount);
            Assert.Throws<ArgumentOutOfRangeException>(() => matrix.InsertColumn(-1, column));
            Assert.Throws<ArgumentOutOfRangeException>(() => matrix.InsertColumn(5, column));
        }

        /// <summary>
        /// Insert a column with invalid number of elements throws <c>ArgumentException</c>.
        /// </summary>
        [Test]
        public void InsertColumnWithUnequalNumberOfElementsThrowsArgumentException()
        {
            var matrix = CreateMatrix(3, 3);
            var column = CreateVector(matrix.RowCount + 1);
            Assert.Throws<ArgumentException>(() => matrix.InsertColumn(0, column));
        }

        /// <summary>
        /// Can set a submatrix.
        /// </summary>
        /// <param name="rowStart">The row to start copying to.</param>
        /// <param name="rowLength">The number of rows to copy.</param>
        /// <param name="colStart">The column to start copying to.</param>
        /// <param name="colLength">The number of columns to copy.</param>
        [TestCase(0, 2, 0, 2)]
        [TestCase(1, 1, 1, 1)]
        public virtual void CanSetSubMatrix(int rowStart, int rowLength, int colStart, int colLength)
        {
            foreach (var matrix in TestMatrices.Values)
            {
                var subMatrix = matrix.SubMatrix(0, 2, 0, 2);
                subMatrix[0, 0] = 10.0;
                subMatrix[0, 1] = -1.0;
                subMatrix[1, 0] = 3.0;
                subMatrix[1, 1] = 4.0;
                matrix.SetSubMatrix(rowStart, rowLength, colStart, colLength, subMatrix);

                for (int i = rowStart, ii = 0; i < rowLength; i++, ii++)
                {
                    for (int j = colStart, jj = 0; j < colLength; j++, jj++)
                    {
                        Assert.AreEqual(matrix[i, j], subMatrix[ii, jj]);
                    }
                }
            }
        }

        /// <summary>
        /// Set submatrix with invalid ranges throws <c>ArgumentOutOfRangeException</c>.
        /// </summary>
        /// <param name="rowStart">The row to start copying to.</param>
        /// <param name="rowLength">The number of rows to copy.</param>
        /// <param name="colStart">The column to start copying to.</param>
        /// <param name="colLength">The number of columns to copy.</param>
        [TestCase(0, 4, 0, 2)]
        [TestCase(0, 2, 0, 4)]
        [TestCase(4, 2, 0, 2)]
        [TestCase(0, 2, 4, 2)]
        [TestCase(-1, 2, 0, 2)]
        [TestCase(0, 2, -1, 2)]
        public virtual void SetSubMatrixWithInvalidRangesThrowsArgumentOutOfRangeException(int rowStart, int rowLength, int colStart, int colLength)
        {
            var subMatrix = TestMatrices["Square3x3"].SubMatrix(0, 2, 0, 2);
            subMatrix[0, 0] = 10.0;
            subMatrix[0, 1] = -1.0;
            subMatrix[1, 0] = 3.0;
            subMatrix[1, 1] = 4.0;
            Assert.Throws<ArgumentOutOfRangeException>(() => TestMatrices["Square3x3"].SetSubMatrix(rowStart, rowLength, colStart, colLength, subMatrix));
        }

        /// <summary>
        /// Set submatrix with invalid length throws <c>ArgumentOutOfRangeException</c>.
        /// </summary>
        /// <param name="rowStart">The row to start copying to.</param>
        /// <param name="rowLength">The number of rows to copy.</param>
        /// <param name="colStart">The column to start copying to.</param>
        /// <param name="colLength">The number of columns to copy.</param>
        [TestCase(0, -1, 0, 2)]
        [TestCase(0, 2, 0, -1)]
        public virtual void SetSubMatrixWithInvalidLengthsThrowsArgumentException(int rowStart, int rowLength, int colStart, int colLength)
        {
            var subMatrix = TestMatrices["Square3x3"].SubMatrix(0, 2, 0, 2);
            subMatrix[0, 0] = 10.0;
            subMatrix[0, 1] = -1.0;
            subMatrix[1, 0] = 3.0;
            subMatrix[1, 1] = 4.0;
            Assert.Throws<ArgumentException>(() => TestMatrices["Square3x3"].SetSubMatrix(rowStart, rowLength, colStart, colLength, subMatrix));
        }

        /// <summary>
        /// Set a submatrix with <c>null</c> submatrix throws <c>ArgumentNullException</c>.
        /// </summary>
        [Test]
        public void SetSubMatrixWithNullSubMatrixThrowsArgumentNullException()
        {
            Matrix<Complex> subMatrix = null;
            Assert.Throws<ArgumentNullException>(() => TestMatrices["Square3x3"].SetSubMatrix(0, 2, 0, 2, subMatrix));
        }

        /// <summary>
        /// Can insert a row.
        /// </summary>
        [Test]
        public void CanInsertRow()
        {
            var matrix = CreateMatrix(3, 3);
            var row = CreateVector(matrix.ColumnCount);
            for (var i = 0; i < row.Count; i++)
            {
                row[i] = i;
            }

            for (var insertedRowIndex = 0; insertedRowIndex < matrix.RowCount + 1; insertedRowIndex++)
            {
                var result = matrix.InsertRow(insertedRowIndex, row);
                Assert.AreEqual(result.RowCount, matrix.ColumnCount + 1);
                for (var i = 0; i < result.RowCount; i++)
                {
                    for (var j = 0; j < result.ColumnCount; j++)
                    {
                        Assert.AreEqual(i == insertedRowIndex ? row[j] : Complex.Zero, result[i, j]);
                    }
                }
            }
        }

        /// <summary>
        /// Insert <c>null</c> row throws <c>ArgumentNullException</c>.
        /// </summary>
        [Test]
        public void InsertNullRowThrowsArgumentNullException()
        {
            var matrix = TestMatrices["Square3x3"];
            Assert.Throws<ArgumentNullException>(() => matrix.InsertRow(0, null));
        }

        /// <summary>
        /// Insert a row with invalid row index throws <c>ArgumentOutOfRangeException</c>.
        /// </summary>
        [Test]
        public void InsertRowWithInvalidRowIndexThrowsArgumentOutOfRangeException()
        {
            var matrix = CreateMatrix(3, 3);
            var row = CreateVector(matrix.ColumnCount);
            Assert.Throws<ArgumentOutOfRangeException>(() => matrix.InsertRow(-1, row));
            Assert.Throws<ArgumentOutOfRangeException>(() => matrix.InsertRow(5, row));
        }

        /// <summary>
        /// Insert a row with invalid number of elements throws <c>ArgumentException</c>.
        /// </summary>
        [Test]
        public void InsertRowWithInvalidNumberOfElementsThrowsArgumentException()
        {
            var matrix = CreateMatrix(3, 3);
            var row = CreateVector(matrix.ColumnCount + 1);
            Assert.Throws<ArgumentException>(() => matrix.InsertRow(0, row));
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
            Matrix<Complex> right = null;
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
            Matrix<Complex> result = null;
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
            Matrix<Complex> bottom = null;
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
            Matrix<Complex> result = null;
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
                        Assert.AreEqual(Complex.Zero, result[i, j]);
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
            Matrix<Complex> lower = null;
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
                        Assert.AreEqual(Complex.Zero, result[i, j]);
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
            Matrix<Complex> result = null;
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
            AssertHelpers.AlmostEqual(10.8819655930903, matrix.FrobeniusNorm(), 14);

            matrix = TestMatrices["Wide2x3"];
            AssertHelpers.AlmostEqual(5.19052560084774, matrix.FrobeniusNorm(), 14);

            matrix = TestMatrices["Tall3x2"];
            AssertHelpers.AlmostEqual(7.59041159967795, matrix.FrobeniusNorm(), 14);
        }

        /// <summary>
        /// Can compute Infinity norm.
        /// </summary>
        [Test]
        public virtual void CanComputeInfinityNorm()
        {
            var matrix = TestMatrices["Square3x3"];
            AssertHelpers.AlmostEqual(16.7777033201323, matrix.InfinityNorm(), 14);

            matrix = TestMatrices["Wide2x3"];
            AssertHelpers.AlmostEqual(7.3514039993641, matrix.InfinityNorm(), 14);

            matrix = TestMatrices["Tall3x2"];
            AssertHelpers.AlmostEqual(10.1023756128209, matrix.InfinityNorm(), 14);
        }

        /// <summary>
        /// Can compute L1 norm.
        /// </summary>
        [Test]
        public virtual void CanComputeL1Norm()
        {
            var matrix = TestMatrices["Square3x3"];
            AssertHelpers.AlmostEqual(12.5401248319437, matrix.L1Norm(), 14);

            matrix = TestMatrices["Wide2x3"];
            AssertHelpers.AlmostEqual(5.86479712463225, matrix.L1Norm(), 14);

            matrix = TestMatrices["Tall3x2"];
            AssertHelpers.AlmostEqual(9.49338601320024, matrix.L1Norm(), 14);
        }

        /// <summary>
        /// Can compute L2 norm.
        /// </summary>
        [Test]
        public virtual void CanComputeL2Norm()
        {
            var matrix = TestMatrices["Square3x3"];
            AssertHelpers.AlmostEqual(10.638175225153, matrix.L2Norm(), 14);

            matrix = TestMatrices["Wide2x3"];
            AssertHelpers.AlmostEqual(5.2058554445283, matrix.L2Norm(), 14);
            matrix = TestMatrices["Tall3x2"];
            AssertHelpers.AlmostEqual(7.35826643761172, matrix.L2Norm(), 14);
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
                Assert.AreEqual(new Complex(1.0, 1.0), item.Item3);

                enumerator.MoveNext();
                item = enumerator.Current;
                Assert.AreEqual(0, item.Item1);
                Assert.AreEqual(1, item.Item2);
                Assert.AreEqual(new Complex(1.0, 1.0), item.Item3);

                enumerator.MoveNext();
                item = enumerator.Current;
                Assert.AreEqual(0, item.Item1);
                Assert.AreEqual(2, item.Item2);
                Assert.AreEqual(new Complex(2.0, 1.0), item.Item3);

                enumerator.MoveNext();
                item = enumerator.Current;
                Assert.AreEqual(1, item.Item1);
                Assert.AreEqual(0, item.Item2);
                Assert.AreEqual(new Complex(1.0, 1.0), item.Item3);

                enumerator.MoveNext();
                item = enumerator.Current;
                Assert.AreEqual(1, item.Item1);
                Assert.AreEqual(1, item.Item2);
                Assert.AreEqual(new Complex(1.0, 1.0), item.Item3);

                enumerator.MoveNext();
                item = enumerator.Current;
                Assert.AreEqual(1, item.Item1);
                Assert.AreEqual(2, item.Item2);
                Assert.AreEqual(new Complex(2.0, 1.0), item.Item3);

                enumerator.MoveNext();
                item = enumerator.Current;
                Assert.AreEqual(2, item.Item1);
                Assert.AreEqual(0, item.Item2);
                Assert.AreEqual(new Complex(1.0, 1.0), item.Item3);

                enumerator.MoveNext();
                item = enumerator.Current;
                Assert.AreEqual(2, item.Item1);
                Assert.AreEqual(1, item.Item2);
                Assert.AreEqual(new Complex(1.0, 1.0), item.Item3);

                enumerator.MoveNext();
                item = enumerator.Current;
                Assert.AreEqual(2, item.Item1);
                Assert.AreEqual(2, item.Item2);
                Assert.AreEqual(new Complex(2.0, 1.0), item.Item3);
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
        /// Can get a sub-matrix.
        /// </summary>
        [Test]
        public virtual void CanGetASubMatrix()
        {
            var matrix = CreateMatrix(10, 10);
            for (var row = 0; row < matrix.RowCount; row++)
            {
                for (var column = 0; column < matrix.ColumnCount; column++)
                {
                    matrix[row, column] = 1.0;
                }
            }

            var submatrix = matrix.SubMatrix(8, 2, 0, 2);
            Assert.AreEqual(2, submatrix.RowCount);
            Assert.AreEqual(2, submatrix.ColumnCount);

            for (var row = 0; row < submatrix.RowCount; row++)
            {
                for (var column = 0; column < submatrix.ColumnCount; column++)
                {
                    Assert.AreEqual(Complex.One, submatrix[row, column]);
                }
            }
        }

        /// <summary>
        /// Test whether we can create a matrix from a list of column vectors.
        /// </summary>
        [Test]
        public virtual void CanCreateMatrixFromColumns()
        {
            var column1 = CreateVector(new Complex[] { 1.0 });
            var column2 = CreateVector(new Complex[] { 1.0, 2.0, 3.0, 4.0 });
            var column3 = CreateVector(new Complex[] { 1.0, 2.0 });
            var columnVectors = new System.Collections.Generic.List<Vector<Complex>>
                                {
                                    column1,
                                    column2,
                                    column3
                                };
            var matrix = Matrix<Complex>.CreateFromColumns(columnVectors);

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
            var row1 = CreateVector(new Complex[] { 1.0 });
            var row2 = CreateVector(new Complex[] { 1.0, 2.0, 3.0, 4.0 });
            var row3 = CreateVector(new Complex[] { 1.0, 2.0 });
            var rowVectors = new System.Collections.Generic.List<Vector<Complex>>
                                {
                                    row1,
                                    row2,
                                    row3
                                };
            var matrix = Matrix<Complex>.CreateFromRows(rowVectors);

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
