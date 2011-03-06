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

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Single
{
    using System;
    using LinearAlgebra.Generic;
    using NUnit.Framework;

    /// <summary>
    /// Abstract class with the common set of matrix tests
    /// </summary>
    [TestFixture]
    public abstract partial class MatrixTests : MatrixLoader
    {
        /// <summary>
        /// Can clone a matrix.
        /// </summary>
        /// <param name="name">Matrix name.</param>
        [Test, Sequential]
        public void CanCloneMatrix([Values("Singular3x3", "Square3x3", "Square4x4", "Tall3x2", "Wide2x3")] string name)
        {
            var matrix = CreateMatrix(TestData2D[name]);
            var clone = matrix.Clone();

            Assert.AreNotSame(matrix, clone);
            Assert.AreEqual(matrix.RowCount, clone.RowCount);
            Assert.AreEqual(matrix.ColumnCount, clone.ColumnCount);
            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    Assert.AreEqual(matrix[i, j], clone[i, j]);
                }
            }
        }

        /// <summary>
        /// Can clone a matrix using <c>ICloneable</c> interface.
        /// </summary>
        /// <param name="name">Matrix name.</param>
        [Test, Sequential]
        public void CanCloneMatrixUsingICloneable([Values("Singular3x3", "Square3x3", "Square4x4", "Tall3x2", "Wide2x3")] string name)
        {
            var matrix = TestMatrices[name];
            var clone = (Matrix<float>)((ICloneable)matrix).Clone();

            Assert.AreNotSame(matrix, clone);
            Assert.AreEqual(matrix.RowCount, clone.RowCount);
            Assert.AreEqual(matrix.ColumnCount, clone.ColumnCount);
            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    Assert.AreEqual(matrix[i, j], clone[i, j]);
                }
            }
        }

        /// <summary>
        /// Can copy a matrix to another matrix.
        /// </summary>
        /// <param name="name">Matrix name.</param>
        [Test, Sequential]
        public void CanCopyTo([Values("Singular3x3", "Square3x3", "Square4x4", "Tall3x2", "Wide2x3")] string name)
        {
            var matrix = TestMatrices[name];
            var copy = CreateMatrix(matrix.RowCount, matrix.ColumnCount);
            matrix.CopyTo(copy);

            Assert.AreNotSame(matrix, copy);
            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    Assert.AreEqual(matrix[i, j], copy[i, j]);
                }
            }
        }

        /// <summary>
        /// Copy a matrix to another matrix fails when target is <c>null</c>.
        /// </summary>
        [Test]
        public void CopyToWhenTargetIsNullThrowsArgumentNullException()
        {
            var matrix = TestMatrices["Singular3x3"];
            Matrix<float> target = null;
            Assert.Throws<ArgumentNullException>(() => matrix.CopyTo(target));
        }

        /// <summary>
        /// Copy a matrix to another matrix fails when target has more rows.
        /// </summary>
        [Test]
        public void CopyToWhenTargetHasMoreRowsThrowsArgumentException()
        {
            var matrix = TestMatrices["Singular3x3"];
            var target = CreateMatrix(matrix.RowCount + 1, matrix.ColumnCount);
            Assert.Throws<ArgumentException>(() => matrix.CopyTo(target));
        }

        /// <summary>
        /// Copy a matrix to another matrix fails when target has more columns.
        /// </summary>
        [Test]
        public void CopyToWhenTargetHasMoreColumnsThrowsArgumentException()
        {
            var matrix = TestMatrices["Singular3x3"];
            var target = CreateMatrix(matrix.RowCount + 1, matrix.ColumnCount);
            Assert.Throws<ArgumentException>(() => matrix.CopyTo(target));
        }

        /// <summary>
        /// Can create a matrix.
        /// </summary>
        [Test]
        public void CanCreateMatrix()
        {
            var expected = CreateMatrix(5, 6);
            var actual = expected.CreateMatrix(5, 6);
            Assert.AreEqual(expected.GetType(), actual.GetType(), "Matrices are same type.");
        }

        /// <summary>
        /// Can equate matrices.
        /// </summary>
        /// <param name="name">Matrix name.</param>
        [Test, Sequential]
        public void CanEquateMatrices([Values("Singular3x3", "Square3x3", "Square4x4", "Tall3x2", "Wide2x3")] string name)
        {
            var matrix1 = CreateMatrix(TestData2D[name]);
            var matrix2 = CreateMatrix(TestData2D[name]);
            var matrix3 = CreateMatrix(TestData2D[name].GetLength(0), TestData2D[name].GetLength(1));
            Assert.IsTrue(matrix1.Equals(matrix1));
            Assert.IsTrue(matrix1.Equals(matrix2));
            Assert.IsFalse(matrix1.Equals(matrix3));
            Assert.IsFalse(matrix1.Equals(null));
        }

        /// <summary>
        /// Create a matrix throws <c>ArgumentOutOfRangeException</c> if size is not positive.
        /// </summary>
        /// <param name="rows">The number of rows.</param>
        /// <param name="columns">The number of columns.</param>
        [Test, Sequential]
        public void IfSizeIsNotPositiveThrowsArgumentException([Values(0, 2, 0, -1, 1)] int rows, [Values(2, 0, 0, 1, -1)] int columns)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => CreateMatrix(rows, columns));
        }

        /// <summary>
        /// Testing for equality with non-matrix returns <c>false</c>.
        /// </summary>
        /// <param name="name">Matrix name.</param>
        [Test, Sequential]
        public void TestingForEqualityWithNonMatrixReturnsFalse([Values("Singular3x3", "Square3x3", "Square4x4", "Tall3x2", "Wide2x3")] string name)
        {
            var matrix = CreateMatrix(TestData2D[name]);
            Assert.IsFalse(matrix.Equals(2));
        }

        /// <summary>
        /// Can test for equality using Object.Equals.
        /// </summary>
        /// <param name="name">Matrix name.</param>
        [Test, Sequential]
        public void CanTestForEqualityUsingObjectEquals([Values("Singular3x3", "Square3x3", "Square4x4", "Tall3x2", "Wide2x3")] string name)
        {
            var matrix1 = CreateMatrix(TestData2D[name]);
            var matrix2 = CreateMatrix(TestData2D[name]);
            Assert.IsTrue(matrix1.Equals((object)matrix2));
        }

        /// <summary>
        /// Range check fails with wrong parameters.
        /// </summary>
        /// <param name="i">Row index.</param>
        /// <param name="j">Column index.</param>
        /// <param name="name">Matrix name.</param>
        [Test, Sequential]
        public void RangeCheckWithInvalidIndicesThrowsArgumentOutOfRangeException([Values(-1, 1, 4)] int i, [Values(1, -1, 2)] int j, [Values("Singular3x3", "Singular3x3", "Square3x3")] string name)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => { var x = TestMatrices[name][i, j]; });
        }

        /// <summary>
        /// Can get matrix hash code.
        /// </summary>
        [Test]
        public void CanMatrixGetHashCode()
        {
            var hash = TestMatrices["Singular3x3"].GetHashCode();
        }

        /// <summary>
        /// Can clear matrix.
        /// </summary>
        [Test]
        public void CanClearMatrix()
        {
            var matrix = TestMatrices["Singular3x3"].Clone();
            matrix.Clear();
            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    Assert.AreEqual(0, matrix[i, j]);
                }
            }
        }

        /// <summary>
        /// Can get a row of a matrix.
        /// </summary>
        /// <param name="rowIndex">Row index.</param>
        /// <param name="name">Matrix name.</param>
        [Test, Combinatorial]
        public void CanGetRow([Values(0, 1, 2)] int rowIndex, [Values("Singular3x3", "Square3x3")] string name)
        {
            var matrix = TestMatrices[name];
            var row = matrix.Row(rowIndex);

            Assert.AreEqual(matrix.ColumnCount, row.Count);
            for (var j = 0; j < matrix.ColumnCount; j++)
            {
                Assert.AreEqual(matrix[rowIndex, j], row[j]);
            }
        }

        /// <summary>
        /// Get row throws ArgumentOutOfRange with negative index.
        /// </summary>
        [Test]
        public void GetRowWithNegativeIndexThrowsArgumentOutOfRange()
        {
            var matrix = TestMatrices["Singular3x3"];
            Assert.Throws<ArgumentOutOfRangeException>(() => matrix.Row(-1));
        }

        /// <summary>
        /// Get row throws <c>ArgumentOutOfRangeException</c> with overflowing row index.
        /// </summary>
        [Test]
        public void GetRowWithOverflowingRowIndexThrowsArgumentOutOfRange()
        {
            var matrix = TestMatrices["Singular3x3"];
            Assert.Throws<ArgumentOutOfRangeException>(() => matrix.Row(matrix.RowCount));
        }

        /// <summary>
        /// Can get row of a matrix into a result vector.
        /// </summary>
        /// <param name="rowIndex">Row index.</param>
        /// <param name="name">Matrix name.</param>
        [Test, Combinatorial]
        public void CanGetRowIntoResult([Values(0, 1, 2)] int rowIndex, [Values("Singular3x3", "Square3x3")] string name)
        {
            var matrix = TestMatrices[name];
            var row = CreateVector(matrix.ColumnCount);
            matrix.Row(rowIndex, row);

            Assert.AreEqual(matrix.ColumnCount, row.Count);
            for (var j = 0; j < matrix.ColumnCount; j++)
            {
                Assert.AreEqual(matrix[rowIndex, j], row[j]);
            }
        }

        /// <summary>
        /// Get row of a matrix into <c>null</c> result vector throws <c>ArgumentNullException</c>.
        /// </summary>
        [Test]
        public void GetRowWhenResultIsNullThrowsArgumentNullException()
        {
            var matrix = TestMatrices["Singular3x3"];
            Assert.Throws<ArgumentNullException>(() => matrix.Row(0, null));
        }

        /// <summary>
        /// Get row into a result with the negative row index throws <c>ArgumentOutOfRangeException</c>.
        /// </summary>
        [Test]
        public void GetRowIntoResultWithNegativeRowIndexThrowsArgumentOutOfRangeException()
        {
            var matrix = TestMatrices["Singular3x3"];
            var row = CreateVector(matrix.ColumnCount);
            Assert.Throws<ArgumentOutOfRangeException>(() => matrix.Row(-1, row));
        }

        /// <summary>
        /// Get row into a vector with overflowing row index throws <c>ArgumentOutOfRangeException</c>.
        /// </summary>
        [Test]
        public void GetRowIntoResultWithOverflowingRowIndexThrowsArgumentOutOfRangeException()
        {
            var matrix = TestMatrices["Singular3x3"];
            var row = CreateVector(matrix.ColumnCount);
            Assert.Throws<ArgumentOutOfRangeException>(() => matrix.Row(matrix.RowCount, row));
        }

        /// <summary>
        /// Can get a row at specific start position and length of a matrix into a vector.
        /// </summary>
        /// <param name="rowIndex">Row index.</param>
        /// <param name="start">Column start.</param>
        /// <param name="length">Row length.</param>
        /// <param name="name">Matrix name.</param>
        [Test, Sequential]
        public void CanGetRowWithRange([Values(0, 1, 2, 2)] int rowIndex, [Values(0, 1, 0, 0)] int start, [Values(1, 2, 3, 3)] int length, [Values("Singular3x3", "Singular3x3", "Singular3x3", "Square3x3")] string name)
        {
            var matrix = TestMatrices[name];
            var row = matrix.Row(rowIndex, start, length);

            Assert.AreEqual(length, row.Count);
            for (var j = start; j < start + length; j++)
            {
                Assert.AreEqual(matrix[rowIndex, j], row[j - start]);
            }
        }

        /// <summary>
        /// Get a row of a matrix at specific start position and zero length throws <c>ArgumentException</c>.
        /// </summary>
        [Test]
        public void GetRowWithRangeIntoResultWhenLengthIsZeroThrowsArgumentException()
        {
            var matrix = TestMatrices["Singular3x3"];
            var result = CreateVector(matrix.ColumnCount);
            Assert.Throws<ArgumentException>(() => matrix.Row(0, 0, 0, result));
        }

        /// <summary>
        /// Get a row at specific start position and length of a matrix into a too small vector throws <c>ArgumentException</c>.
        /// </summary>
        [Test]
        public void GetRowWithRangeIntoTooSmallResultVectorThrowsArgumentException()
        {
            var matrix = TestMatrices["Singular3x3"];
            var result = CreateVector(matrix.ColumnCount - 1);
            Assert.Throws<ArgumentException>(() => matrix.Row(0, 0, 0, result));
        }

        /// <summary>
        /// Can get a column of a matrix.
        /// </summary>
        /// <param name="colIndex">Row index.</param>
        /// <param name="name">Matrix name.</param>
        [Test, Combinatorial]
        public void CanGetColumn([Values(0, 1, 2)] int colIndex, [Values("Singular3x3", "Square3x3")] string name)
        {
            var matrix = TestMatrices[name];
            var col = matrix.Column(colIndex);

            Assert.AreEqual(matrix.RowCount, col.Count);
            for (var j = 0; j < matrix.RowCount; j++)
            {
                Assert.AreEqual(matrix[j, colIndex], col[j]);
            }
        }

        /// <summary>
        /// Get a column with negative index throws <c>ArgumentOutOfRangeException</c>.
        /// </summary>
        [Test]
        public void GetColumnWithNegativeIndexThrowsArgumentOutOfRangeException()
        {
            var matrix = TestMatrices["Singular3x3"];
            Assert.Throws<ArgumentOutOfRangeException>(() => matrix.Column(-1));
        }

        /// <summary>
        /// Get column with overflowing row index throws <c>ArgumentOutOfRangeException</c>.
        /// </summary>
        [Test]
        public void GetColumnWithOverflowingRowIndexThrowsArgumentOutOfRangeException()
        {
            var matrix = TestMatrices["Singular3x3"];
            Assert.Throws<ArgumentOutOfRangeException>(() => matrix.Column(matrix.ColumnCount));
        }

        /// <summary>
        /// Can get a column into a result vector.
        /// </summary>
        /// <param name="colIndex">Column index.</param>
        /// <param name="name">Matrix name.</param>
        [Test, Combinatorial]
        public void CanGetColumnIntoResult([Values(0, 1, 2)] int colIndex, [Values("Singular3x3", "Square3x3")] string name)
        {
            var matrix = TestMatrices[name];
            var col = CreateVector(matrix.RowCount);
            matrix.Column(colIndex, col);

            Assert.AreEqual(matrix.RowCount, col.Count);
            for (var j = 0; j < matrix.RowCount; j++)
            {
                Assert.AreEqual(matrix[j, colIndex], col[j]);
            }
        }

        /// <summary>
        /// Get a column when result vector is <c>null</c> throws <c>ArgumentNullException</c>.
        /// </summary>
        [Test]
        public void GetColumnWhenResultIsNullThrowsArgumentNullException()
        {
            var matrix = TestMatrices["Singular3x3"];
            Assert.Throws<ArgumentNullException>(() => matrix.Column(0, null));
        }

        /// <summary>
        /// Get a column into result vector with negative index throws <c>ArgumentOutOfRangeException</c>.
        /// </summary>
        [Test]
        public void GetColumnIntoResultWithNegativeIndexThrowsArgumentOutOfRangeException()
        {
            var matrix = TestMatrices["Singular3x3"];
            var column = CreateVector(matrix.ColumnCount);
            Assert.Throws<ArgumentOutOfRangeException>(() => matrix.Column(-1, column));
        }

        /// <summary>
        /// Get a column into result with overflowing row index throws <c>ArgumentOutOfRangeException</c>.
        /// </summary>
        [Test]
        public void GetColumnIntoResultWithOverflowingRowIndexThrowsArgumentOutOfRangeException()
        {
            var matrix = TestMatrices["Singular3x3"];
            var column = CreateVector(matrix.RowCount);
            Assert.Throws<ArgumentOutOfRangeException>(() => matrix.Row(matrix.ColumnCount, column));
        }

        /// <summary>
        /// Can get a column with range.
        /// </summary>
        /// <param name="colIndex">Column index.</param>
        /// <param name="start">Start index.</param>
        /// <param name="length">Column length.</param>
        /// <param name="name">Matrix name.</param>
        [Test, Sequential]
        public void CanGetColumnWithRange([Values(0, 1, 2, 2)] int colIndex, [Values(0, 1, 0, 0)] int start, [Values(1, 2, 3, 3)] int length, [Values("Singular3x3", "Singular3x3", "Singular3x3", "Square3x3")] string name)
        {
            var matrix = TestMatrices[name];
            var col = matrix.Column(colIndex, start, length);

            Assert.AreEqual(length, col.Count);
            for (var j = start; j < start + length; j++)
            {
                Assert.AreEqual(matrix[j, colIndex], col[j - start]);
            }
        }

        /// <summary>
        /// Get a column range into a result vector when length is zero throws <c>ArgumentException</c>.
        /// </summary>
        [Test]
        public void GetColumnRangeIntoResultWhenLengthIsZeroThrowsArgumentException()
        {
            var matrix = TestMatrices["Singular3x3"];
            var col = CreateVector(matrix.RowCount);
            Assert.Throws<ArgumentException>(() => matrix.Column(0, 0, 0, col));
        }

        /// <summary>
        /// Get a column range into too small result vector throws <c>ArgumentException</c>.
        /// </summary>
        [Test]
        public void GetColumnRangeIntoTooSmallResultVectorThrowsArgumentException()
        {
            var matrix = TestMatrices["Singular3x3"];
            var result = CreateVector(matrix.RowCount - 1);
            Assert.Throws<ArgumentException>(() => matrix.Column(0, 0, matrix.RowCount, result));
        }

        /// <summary>
        /// Can set a row.
        /// </summary>
        /// <param name="rowIndex">Row index.</param>
        /// <param name="name">Matrix name.</param>
        [Test, Combinatorial]
        public void CanSetRow([Values(0, 1, 2)] int rowIndex, [Values("Singular3x3", "Square3x3")] string name)
        {
            var matrix = TestMatrices[name].Clone();
            matrix.SetRow(rowIndex, CreateVector(matrix.ColumnCount));

            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    Assert.AreEqual(i == rowIndex ? 0.0f : TestMatrices[name][i, j], matrix[i, j]);
                }
            }
        }

        /// <summary>
        /// Can set a column.
        /// </summary>
        /// <param name="colIndex">Column index.</param>
        /// <param name="name">Matrix name.</param>
        [Test, Combinatorial]
        public void CanSetColumn([Values(0, 1, 2)] int colIndex, [Values("Singular3x3", "Square3x3")] string name)
        {
            var matrix = TestMatrices[name].Clone();
            matrix.SetColumn(colIndex, CreateVector(matrix.ColumnCount));

            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    Assert.AreEqual(j == colIndex ? 0.0f : TestMatrices[name][i, j], matrix[i, j]);
                }
            }
        }

        /// <summary>
        /// Get an upper triangle matrix.
        /// </summary>
        /// <param name="name">Matrix name.</param>
        [Test, Sequential]
        public void CanUpperTriangle([Values("Singular3x3", "Square3x3", "Square4x4", "Tall3x2", "Wide2x3")] string name)
        {
            var data = TestMatrices[name];
            var upper = data.UpperTriangle();
            for (var i = 0; i < data.RowCount; i++)
            {
                for (var j = 0; j < data.ColumnCount; j++)
                {
                    Assert.AreEqual(i <= j ? data[i, j] : 0, upper[i, j]);
                }
            }
        }

        /// <summary>
        /// Get an upper triangle matrix into a result matrix.
        /// </summary>
        /// <param name="name">Matrix name.</param>
        [Test, Sequential]
        public void CanUpperTriangleIntoResult([Values("Singular3x3", "Square3x3", "Square4x4", "Tall3x2", "Wide2x3")] string name)
        {
            var data = TestMatrices[name];
            var result = CreateMatrix(data.RowCount, data.ColumnCount);
            data.UpperTriangle(result);
            for (var i = 0; i < data.RowCount; i++)
            {
                for (var j = 0; j < data.ColumnCount; j++)
                {
                    Assert.AreEqual(i <= j ? data[i, j] : 0, result[i, j]);
                }
            }
        }

        /// <summary>
        /// Get an upper triangle matrix into <c>null</c> result matrix throws <c>ArgumentNullException</c>.
        /// </summary>
        [Test]
        public void UpperTriangleIntoResultNullThrowsArgumentNullException()
        {
            var data = TestMatrices["Square3x3"];
            Matrix<float> result = null;
            Assert.Throws<ArgumentNullException>(() => data.UpperTriangle(result));
        }

        /// <summary>
        /// Get an upper triangle into a result matrix with unequal rows throws <c>ArgumentException</c>.
        /// </summary>
        [Test]
        public void UpperTriangleIntoResultWithUnEqualRowsThrowsArgumentException()
        {
            var data = TestMatrices["Square3x3"];
            var result = CreateMatrix(data.RowCount + 1, data.ColumnCount);
            Assert.Throws<ArgumentException>(() => data.UpperTriangle(result));
        }

        /// <summary>
        /// Get an upper triangle into a result matrix with unequal columns throws <c>ArgumentException</c>.
        /// </summary>
        [Test]
        public void UpperTriangleIntoResultWithUnEqualColumnsThrowsArgumentException()
        {
            var data = TestMatrices["Square3x3"];
            var result = CreateMatrix(data.RowCount, data.ColumnCount + 1);
            Assert.Throws<ArgumentException>(() => data.UpperTriangle(result));
        }

        /// <summary>
        /// Get a lower triangle matrix.
        /// </summary>
        /// <param name="name">Matrix name.</param>
        [Test, Sequential]
        public void CanLowerTriangle([Values("Singular3x3", "Square3x3", "Square4x4", "Tall3x2", "Wide2x3")] string name)
        {
            var data = TestMatrices[name];
            var lower = data.LowerTriangle();
            for (var i = 0; i < data.RowCount; i++)
            {
                for (var j = 0; j < data.ColumnCount; j++)
                {
                    Assert.AreEqual(i >= j ? data[i, j] : 0, lower[i, j]);
                }
            }
        }

        /// <summary>
        /// Get a lower triangle matrix into a result matrix.
        /// </summary>
        /// <param name="name">Matrix name.</param>
        [Test, Sequential]
        public void CanLowerTriangleIntoResult([Values("Singular3x3", "Square3x3", "Square4x4", "Tall3x2", "Wide2x3")] string name)
        {
            var data = TestMatrices[name];
            var result = CreateMatrix(data.RowCount, data.ColumnCount);
            data.LowerTriangle(result);
            for (var i = 0; i < data.RowCount; i++)
            {
                for (var j = 0; j < data.ColumnCount; j++)
                {
                    Assert.AreEqual(i >= j ? data[i, j] : 0, result[i, j]);
                }
            }
        }

        /// <summary>
        /// Get a lower triangle matrix into <c>null</c> result matrix throws <c>ArgumentNullException</c>.
        /// </summary>
        [Test]
        public void LowerTriangleIntoResultNullThrowsArgumentNullException()
        {
            var data = TestMatrices["Square3x3"];
            Matrix<float> result = null;
            Assert.Throws<ArgumentNullException>(() => data.LowerTriangle(result));
        }

        /// <summary>
        /// Get a lower triangle into a result matrix with unequal rows throws <c>ArgumentException</c>.
        /// </summary>
        [Test]
        public void LowerTriangleIntoResultWithUnEqualRowsThrowsArgumentException()
        {
            var data = TestMatrices["Square3x3"];
            var result = CreateMatrix(data.RowCount + 1, data.ColumnCount);
            Assert.Throws<ArgumentException>(() => data.LowerTriangle(result));
        }

        /// <summary>
        /// Get a lower triangle into a result matrix with unequal columns throws <c>ArgumentException</c>.
        /// </summary>
        [Test]
        public void LowerTriangleIntoResultWithUnEqualColumnsThrowsArgumentException()
        {
            var data = TestMatrices["Square3x3"];
            var result = CreateMatrix(data.RowCount, data.ColumnCount + 1);
            Assert.Throws<ArgumentException>(() => data.LowerTriangle(result));
        }

        /// <summary>
        /// Get a strictly lower triangle.
        /// </summary>
        [Test]
        public void CanStrictlyLowerTriangle()
        {
            foreach (var data in TestMatrices.Values)
            {
                var lower = data.StrictlyLowerTriangle();
                for (var i = 0; i < data.RowCount; i++)
                {
                    for (var j = 0; j < data.ColumnCount; j++)
                    {
                        Assert.AreEqual(i > j ? data[i, j] : 0, lower[i, j]);
                    }
                }
            }
        }

        /// <summary>
        /// Get a strictly lower triangle into a result matrix.
        /// </summary>
        [Test]
        public void CanStrictlyLowerTriangleIntoResult()
        {
            foreach (var data in TestMatrices.Values)
            {
                var lower = CreateMatrix(data.RowCount, data.ColumnCount);
                data.StrictlyLowerTriangle(lower);
                for (var i = 0; i < data.RowCount; i++)
                {
                    for (var j = 0; j < data.ColumnCount; j++)
                    {
                        Assert.AreEqual(i > j ? data[i, j] : 0, lower[i, j]);
                    }
                }
            }
        }

        /// <summary>
        /// Get a strictly lower triangle with <c>null</c> parameter throws <c>ArgumentNullException</c>.
        /// </summary>
        [Test]
        public void StrictlyLowerTriangleWithNullParameterThrowsArgumentNullException()
        {
            var data = TestMatrices["Square3x3"];
            Matrix<float> lower = null;
            Assert.Throws<ArgumentNullException>(() => data.StrictlyLowerTriangle(lower));
        }

        /// <summary>
        /// Get a strictly lower triangle into result with unequal column number throws <c>ArgumentException</c>.
        /// </summary>
        [Test]
        public void StrictlyLowerTriangleIntoResultWithUnequalColumnNumberThrowsArgumentException()
        {
            var data = TestMatrices["Square3x3"];
            var lower = CreateMatrix(data.RowCount, data.ColumnCount + 1);
            Assert.Throws<ArgumentException>(() => data.StrictlyLowerTriangle(lower));
        }

        /// <summary>
        /// Get a strictly lower triangle into result with unequal row number throws <c>ArgumentException</c>.
        /// </summary>
        [Test]
        public void StrictlyLowerTriangleIntoResultWithUnequalRowNumberThrowsArgumentException()
        {
            var data = TestMatrices["Square3x3"];
            var lower = CreateMatrix(data.RowCount + 1, data.ColumnCount);
            Assert.Throws<ArgumentException>(() => data.StrictlyLowerTriangle(lower));
        }

        /// <summary>
        /// Get a strictly upper triangle.
        /// </summary>
        [Test]
        public void CanStrictlyUpperTriangle()
        {
            foreach (var data in TestMatrices.Values)
            {
                var lower = data.StrictlyUpperTriangle();
                for (var i = 0; i < data.RowCount; i++)
                {
                    for (var j = 0; j < data.ColumnCount; j++)
                    {
                        Assert.AreEqual(i < j ? data[i, j] : 0, lower[i, j]);
                    }
                }
            }
        }

        /// <summary>
        /// Get a strictly upper triangle into a result matrix.
        /// </summary>
        [Test]
        public void CanStrictlyUpperTriangleIntoResult()
        {
            foreach (var data in TestMatrices.Values)
            {
                var lower = CreateMatrix(data.RowCount, data.ColumnCount);
                data.StrictlyUpperTriangle(lower);
                for (var i = 0; i < data.RowCount; i++)
                {
                    for (var j = 0; j < data.ColumnCount; j++)
                    {
                        Assert.AreEqual(i < j ? data[i, j] : 0, lower[i, j]);
                    }
                }
            }
        }

        /// <summary>
        /// Get a strictly upper triangle with <c>null</c> parameter throws <c>ArgumentNullException</c>.
        /// </summary>
        [Test]
        public void StrictlyUpperTriangleWithNullParameterThrowsArgumentNullException()
        {
            var data = TestMatrices["Square3x3"];
            Matrix<float> lower = null;
            Assert.Throws<ArgumentNullException>(() => data.StrictlyUpperTriangle(lower));
        }

        /// <summary>
        /// Get a strictly upper triangle into a result matrix with unequal column number throws <c>ArgumentException</c>.
        /// </summary>
        [Test]
        public void StrictlyUpperTriangleIntoResultWithUnequalColumnNumberThrowsArgumentException()
        {
            var data = TestMatrices["Square3x3"];
            var lower = CreateMatrix(data.RowCount, data.ColumnCount + 1);
            Assert.Throws<ArgumentException>(() => data.StrictlyUpperTriangle(lower));
        }

        /// <summary>
        /// Get a strictly upper triangle into a result matrix with unequal row number throws <c>ArgumentException</c>.
        /// </summary>
        [Test]
        public void StrictlyUpperTriangleIntoResultWithUnequalRowNumberThrowsArgumentException()
        {
            var data = TestMatrices["Square3x3"];
            var lower = CreateMatrix(data.RowCount + 1, data.ColumnCount);
            Assert.Throws<ArgumentException>(() => data.StrictlyUpperTriangle(lower));
        }

        /// <summary>
        /// Can transpose a matrix.
        /// </summary>
        /// <param name="name">Matrix name.</param>
        [Test, Sequential]
        public void CanTransposeMatrix([Values("Singular3x3", "Square3x3", "Square4x4", "Tall3x2", "Wide2x3")] string name)
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
        /// Can set a column with an array.
        /// </summary>
        /// <param name="name">Matrix name.</param>
        /// <param name="column">Column array.</param>
        [Test, Sequential]
        public virtual void CanSetColumnWithArray([Values("Singular3x3", "Square3x3", "Tall3x2", "Wide2x3")] string name, [Values(new float[] { 1, 2, 3 }, new float[] { 1, 2, 3 }, new float[] { 1, 2, 3 }, new float[] { 1, 2 })] float[] column)
        {
            var matrix = TestMatrices[name];
            for (var i = 0; i < matrix.ColumnCount; i++)
            {
                matrix.SetColumn(i, column);
                for (var j = 0; j < matrix.RowCount; j++)
                {
                    Assert.AreEqual(matrix[j, i], column[j]);
                }
            }
        }

        /// <summary>
        /// Set a column with <c>null</c> array throws <c>ArgumentNullException</c>.
        /// </summary>
        [Test]
        public virtual void SetColumnWithNullArrayThrowsArgumentNullException()
        {
            float[] vec = null;
            Assert.Throws<ArgumentNullException>(() => TestMatrices["Singular3x3"].SetColumn(1, vec));
        }

        /// <summary>
        /// Set a column with unequal array length throws <c>ArgumentException</c>.
        /// </summary>
        [Test]
        public virtual void SetColumnWithArrayUnequalLengthThrowsArgumentException()
        {
            var array = new float[] { 1, 2, 3, 4, 5 };
            Assert.Throws<ArgumentException>(() => TestMatrices["Singular3x3"].SetColumn(1, array));
        }

        /// <summary>
        /// Set a column array with invalid column index throws <c>ArgumentOutOfRangeException</c>.
        /// </summary>
        [Test]
        public void SetColumnWithArrayWithInvalidColumnIndexThrowsArgumentOutOfRangeException()
        {
            var matrix = TestMatrices["Square3x3"];
            float[] column = { 1, 2, 3 };
            Assert.Throws<ArgumentOutOfRangeException>(() => matrix.SetColumn(matrix.ColumnCount + 1, column));
            Assert.Throws<ArgumentOutOfRangeException>(() => matrix.SetColumn(-1, column));
        }

        /// <summary>
        /// Can set a column with a vector.
        /// </summary>
        /// <param name="name">Matrix name.</param>
        /// <param name="column">Column values.</param>
        [Test, Sequential]
        public virtual void CanSetColumnWithVector([Values("Singular3x3", "Square3x3", "Tall3x2", "Wide2x3")] string name, [Values(new float[] { 1, 2, 3 }, new float[] { 1, 2, 3 }, new float[] { 1, 2, 3 }, new float[] { 1, 2 })] float[] column)
        {
            var matrix = TestMatrices[name];
            var columnVector = CreateVector(column);
            for (var i = 0; i < matrix.ColumnCount; i++)
            {
                matrix.SetColumn(i, column);
                for (var j = 0; j < matrix.RowCount; j++)
                {
                    Assert.AreEqual(matrix[j, i], columnVector[j]);
                }
            }
        }

        /// <summary>
        /// Set a column with vector with wrong length throws <c>ArgumentException</c>.
        /// </summary>
        [Test]
        public virtual void SetColumnWithVectorWithUnequalLengthThrowsArgumentException()
        {
            var matrix = TestMatrices["Singular3x3"];
            var columnVector = CreateVector(new float[] { 1, 2, 3, 4, 5 });
            Assert.Throws<ArgumentException>(() => matrix.SetColumn(1, columnVector));
        }

        /// <summary>
        /// Set a column with <c>null</c> vector throw <c>ArgumentNullException</c>.
        /// </summary>
        [Test]
        public void SetColumnWithNullVectorThrowsArgumentNullException()
        {
            var matrix = TestMatrices["Square3x3"];
            Vector<float> columnVector = null;
            Assert.Throws<ArgumentNullException>(() => matrix.SetColumn(1, columnVector));
        }

        /// <summary>
        /// Set column vector with invalid column index throws <c>ArgumentOutOfRangeException</c>.
        /// </summary>
        [Test]
        public void SetColumnWithVectorWithInvalidColumnIndexThrowsArgumentOutOfRangeException()
        {
            var matrix = TestMatrices["Square3x3"];
            var column = CreateVector(new float[] { 1, 2, 3 });
            Assert.Throws<ArgumentOutOfRangeException>(() => matrix.SetColumn(-1, column));
            Assert.Throws<ArgumentOutOfRangeException>(() => matrix.SetColumn(matrix.ColumnCount + 1, column));
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
                        Assert.AreEqual(col == k ? row : 0, result[row, col]);
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
        /// Can set a row with an array.
        /// </summary>
        /// <param name="name">Matrix name.</param>
        /// <param name="row">Row index.</param>
        [Test, Sequential]
        public virtual void CanSetRowWithArray([Values("Singular3x3", "Square3x3", "Tall3x2", "Wide2x3")] string name, [Values(new float[] { 1, 2, 3 }, new float[] { 1, 2, 3 }, new float[] { 1, 2 }, new float[] { 1, 2, 3 })] float[] row)
        {
            var matrix = TestMatrices[name];
            for (var i = 0; i < matrix.RowCount; i++)
            {
                matrix.SetRow(i, row);
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    Assert.AreEqual(matrix[i, j], row[j]);
                }
            }
        }

        /// <summary>
        /// Set a row with <c>null</c> array throws <c>ArgumentNullException</c>.
        /// </summary>
        [Test]
        public virtual void SetRowWithNullArrayThrowsArgumentNullException()
        {
            float[] arr = null;
            Assert.Throws<ArgumentNullException>(() => TestMatrices["Square3x3"].SetRow(1, arr));
        }

        /// <summary>
        /// Set a row with an array with invalid row index throws <c>ArgumentOutOfRangeException</c>.
        /// </summary>
        [Test]
        public void SetRowWithArrayWithInvalidRowIndexThrowsArgumentOutOfRangeException()
        {
            var matrix = TestMatrices["Square3x3"];
            float[] row = { 1, 2, 3 };
            Assert.Throws<ArgumentOutOfRangeException>(() => matrix.SetRow(-1, row));
            Assert.Throws<ArgumentOutOfRangeException>(() => matrix.SetRow(matrix.RowCount + 1, row));
        }

        /// <summary>
        /// Can set a row with a vector.
        /// </summary>
        /// <param name="name">Matrix name.</param>
        /// <param name="row">Row index.</param>
        [Test, Sequential]
        public virtual void CanSetRowWithVector([Values("Singular3x3", "Square3x3", "Tall3x2", "Wide2x3")] string name, [Values(new float[] { 1, 2, 3 }, new float[] { 1, 2, 3 }, new float[] { 1, 2 }, new float[] { 1, 2, 3 })] float[] row)
        {
            var matrix = TestMatrices[name];
            var rowVector = CreateVector(row);
            for (var i = 0; i < matrix.RowCount; i++)
            {
                matrix.SetRow(i, row);
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    Assert.AreEqual(matrix[i, j], rowVector[j]);
                }
            }
        }

        /// <summary>
        /// Set a row with a vector of unequal length throws <c>ArgumentException</c>.
        /// </summary>
        [Test]
        public virtual void SetRowWithVectorWithUnequalLengthThrowsArgumentException()
        {
            var vec = CreateVector(new float[] { 1, 2, 3, 4, 5 });
            Assert.Throws<ArgumentException>(() => TestMatrices["Square3x3"].SetRow(1, vec));
        }

        /// <summary>
        /// Set a row with a <c>null</c> vector throws <c>ArgumentNullException</c>.
        /// </summary>
        [Test]
        public void SetRowWithNullVectorThrowsArgumentNullException()
        {
            var matrix = TestMatrices["Square3x3"];
            Vector<float> rowVector = null;
            Assert.Throws<ArgumentNullException>(() => matrix.SetRow(1, rowVector));
        }

        /// <summary>
        /// Set a row with a vector with invalid row index throws <c>ArgumentOutOfRangeException</c>.
        /// </summary>
        [Test]
        public void SetRowWithVectorWithInvalidRowIndexThrowsArgumentOutOfRangeException()
        {
            var matrix = TestMatrices["Square3x3"];
            var row = CreateVector(new float[] { 1, 2, 3 });
            Assert.Throws<ArgumentOutOfRangeException>(() => matrix.SetRow(-1, row));
            Assert.Throws<ArgumentOutOfRangeException>(() => matrix.SetRow(matrix.RowCount + 1, row));
        }

        /// <summary>
        /// Can set a submatrix.
        /// </summary>
        /// <param name="rowStart">The row to start copying to.</param>
        /// <param name="rowLength">The number of rows to copy.</param>
        /// <param name="colStart">The column to start copying to.</param>
        /// <param name="colLength">The number of columns to copy.</param>
        [Test, Sequential]
        public virtual void CanSetSubMatrix([Values(0, 1)] int rowStart, [Values(2, 1)] int rowLength, [Values(0, 1)] int colStart, [Values(2, 1)] int colLength)
        {
            foreach (var matrix in TestMatrices.Values)
            {
                var subMatrix = matrix.SubMatrix(0, 2, 0, 2);
                subMatrix[0, 0] = 10.0f;
                subMatrix[0, 1] = -1.0f;
                subMatrix[1, 0] = 3.0f;
                subMatrix[1, 1] = 4.0f;
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
        [Test, Sequential]
        public virtual void SetSubMatrixWithInvalidRangesThrowsArgumentOutOfRangeException([Values(0, 0, 4, 0, -1, 0)] int rowStart, [Values(4, 2, 2, 2, 2, 2)] int rowLength, [Values(0, 0, 0, 4, 0, -1)] int colStart, [Values(2, 4, 2, 2, 2, 2)] int colLength)
        {
            var subMatrix = TestMatrices["Square3x3"].SubMatrix(0, 2, 0, 2);
            subMatrix[0, 0] = 10.0f;
            subMatrix[0, 1] = -1.0f;
            subMatrix[1, 0] = 3.0f;
            subMatrix[1, 1] = 4.0f;
            Assert.Throws<ArgumentOutOfRangeException>(() => TestMatrices["Square3x3"].SetSubMatrix(rowStart, rowLength, colStart, colLength, subMatrix));
        }

        /// <summary>
        /// Set submatrix with invalid length throws <c>ArgumentOutOfRangeException</c>.
        /// </summary>
        /// <param name="rowStart">The row to start copying to.</param>
        /// <param name="rowLength">The number of rows to copy.</param>
        /// <param name="colStart">The column to start copying to.</param>
        /// <param name="colLength">The number of columns to copy.</param>
        [Test, Sequential]
        public virtual void SetSubMatrixWithInvalidLengthsThrowsArgumentException([Values(0, 0)] int rowStart, [Values(-1, 2)] int rowLength, [Values(0, 0)] int colStart, [Values(2, -1)] int colLength)
        {
            var subMatrix = TestMatrices["Square3x3"].SubMatrix(0, 2, 0, 2);
            subMatrix[0, 0] = 10.0f;
            subMatrix[0, 1] = -1.0f;
            subMatrix[1, 0] = 3.0f;
            subMatrix[1, 1] = 4.0f;
            Assert.Throws<ArgumentException>(() => TestMatrices["Square3x3"].SetSubMatrix(rowStart, rowLength, colStart, colLength, subMatrix));
        }

        /// <summary>
        /// Set a submatrix with <c>null</c> submatrix throws <c>ArgumentNullException</c>.
        /// </summary>
        [Test]
        public void SetSubMatrixWithNullSubMatrixThrowsArgumentNullException()
        {
            Matrix<float> subMatrix = null;
            Assert.Throws<ArgumentNullException>(() => TestMatrices["Square3x3"].SetSubMatrix(0, 2, 0, 2, subMatrix));
        }

        /// <summary>
        /// Can set a diagonal vector.
        /// </summary>
        /// <param name="name">Matrix name.</param>
        /// <param name="diagonal">Diagonal vector.</param>
        [Test, Sequential]
        public void CanSetDiagonalVector([Values("Square3x3", "Wide2x3", "Tall3x2")] string name, [Values(new float[] { 1, 2, 3 }, new float[] { 1, 2 }, new float[] { 1, 2 })] float[] diagonal)
        {
            var matrix = TestMatrices[name];
            var vector = CreateVector(diagonal);
            matrix.SetDiagonal(vector);

            var min = Math.Min(matrix.ColumnCount, matrix.RowCount);
            Assert.AreEqual(diagonal.Length, min);

            for (var i = 0; i < vector.Count; i++)
            {
                Assert.AreEqual(vector[i], matrix[i, i]);
            }
        }

        /// <summary>
        /// Set a diagonal vector with unequal length throws <c>ArgumentException</c>.
        /// </summary>
        [Test]
        public void SetDiagonalVectorWithUnequalLengthThrowsArgumentException()
        {
            var vector = CreateVector(new float[] { 1, 2, 3 });
            Assert.Throws<ArgumentException>(() => TestMatrices["Wide2x3"].SetDiagonal(vector));
        }

        /// <summary>
        /// Set a diagonal with <c>null</c> vector throws <c>ArgumentNullException</c>.
        /// </summary>
        [Test]
        public void SetDiagonalWithNullVectorThrowsArgumentNullException()
        {
            Vector<float> vector = null;
            Assert.Throws<ArgumentNullException>(() => TestMatrices["Square3x3"].SetDiagonal(vector));
        }

        /// <summary>
        /// Can set a diagonal array.
        /// </summary>
        /// <param name="name">Matrix name.</param>
        /// <param name="diagonal">Diagonal array.</param>
        [Test, Sequential]
        public void CanSetDiagonalArray([Values("Square3x3", "Wide2x3", "Tall3x2")] string name, [Values(new float[] { 1, 2, 3 }, new float[] { 1, 2 }, new float[] { 1, 2 })] float[] diagonal)
        {
            var matrix = TestMatrices[name];
            matrix.SetDiagonal(diagonal);

            var min = Math.Min(matrix.ColumnCount, matrix.RowCount);
            Assert.AreEqual(diagonal.Length, min);

            for (var i = 0; i < diagonal.Length; i++)
            {
                Assert.AreEqual(diagonal[i], matrix[i, i]);
            }
        }

        /// <summary>
        /// Set a diagonal array with unequal length throws <c>ArgumentException</c>.
        /// </summary>
        [Test]
        public void SetDiagonalArrayWithUnequalLengthThrowsArgumentException()
        {
            var array = new float[] { 1, 2, 3 };
            Assert.Throws<ArgumentException>(() => TestMatrices["Wide2x3"].SetDiagonal(array));
        }

        /// <summary>
        /// Set a diagonal with <c>null</c> array throws <c>ArgumentNullException</c>.
        /// </summary>
        [Test]
        public void SetDiagonalWithNullArrayThrowsArgumentNullException()
        {
            float[] array = null;
            Assert.Throws<ArgumentNullException>(() => TestMatrices["Square3x3"].SetDiagonal(array));
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
                        Assert.AreEqual(i == insertedRowIndex ? row[j] : 0, result[i, j]);
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
        [Test, Sequential]
        public virtual void CanPermuteMatrixRows([Values("Singular3x3", "Square3x3", "Tall3x2")] string name)
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
        [Test, Sequential]
        public virtual void CanPermuteMatrixColumns([Values("Singular3x3", "Square3x3", "Wide2x3")] string name)
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
            Matrix<float> right = null;
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
            Matrix<float> result = null;
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
            Matrix<float> bottom = null;
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
            Matrix<float> result = null;
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
            Matrix<float> lower = null;
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
            Matrix<float> result = null;
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
            AssertHelpers.AlmostEqual(10.77775486824598f, matrix.FrobeniusNorm(), 7);

            matrix = TestMatrices["Wide2x3"];
            AssertHelpers.AlmostEqual(4.79478883789474f, matrix.FrobeniusNorm(), 7);

            matrix = TestMatrices["Tall3x2"];
            AssertHelpers.AlmostEqual(7.54122006044115f, matrix.FrobeniusNorm(), 7);
        }

        /// <summary>
        /// Can compute Infinity norm.
        /// </summary>
        [Test]
        public virtual void CanComputeInfinityNorm()
        {
            var matrix = TestMatrices["Square3x3"];
            AssertHelpers.AlmostEqual(16.5f, matrix.InfinityNorm(), 6);

            matrix = TestMatrices["Wide2x3"];
            AssertHelpers.AlmostEqual(6.6f, matrix.InfinityNorm(), 6);

            matrix = TestMatrices["Tall3x2"];
            AssertHelpers.AlmostEqual(9.9f, matrix.InfinityNorm(), 6);
        }

        /// <summary>
        /// Can compute L1 norm.
        /// </summary>
        [Test]
        public virtual void CanComputeL1Norm()
        {
            var matrix = TestMatrices["Square3x3"];
            Assert.AreEqual(12.1f, matrix.L1Norm());

            matrix = TestMatrices["Wide2x3"];
            Assert.AreEqual(5.5f, matrix.L1Norm());

            matrix = TestMatrices["Tall3x2"];
            Assert.AreEqual(8.8f, matrix.L1Norm());
        }

        /// <summary>
        /// Can compute L2 norm.
        /// </summary>
        [Test]
        public virtual void CanComputeL2Norm()
        {
            var matrix = TestMatrices["Square3x3"];
            AssertHelpers.AlmostEqual(10.391347375312632f, matrix.L2Norm(), 6);

            matrix = TestMatrices["Wide2x3"];
            AssertHelpers.AlmostEqual(4.7540849434107635f, matrix.L2Norm(), 6);
            matrix = TestMatrices["Tall3x2"];
            AssertHelpers.AlmostEqual(7.182727033856683f, matrix.L2Norm(), 5);
        }
        
        /// <summary>
        /// Test whether the index enumerator returns the correct values.
        /// </summary>
        [Test]
        public virtual void CanUseIndexedEnumerator()
        {
            var matrix = TestMatrices["Singular3x3"];
            var enumerator = matrix.IndexedEnumerator().GetEnumerator();

            enumerator.MoveNext();
            var item = enumerator.Current;
            Assert.AreEqual(0, item.Item1);
            Assert.AreEqual(0, item.Item2);
            Assert.AreEqual(1.0f, item.Item3);

            enumerator.MoveNext();
            item = enumerator.Current;
            Assert.AreEqual(0, item.Item1);
            Assert.AreEqual(1, item.Item2);
            Assert.AreEqual(1.0f, item.Item3);

            enumerator.MoveNext();
            item = enumerator.Current;
            Assert.AreEqual(0, item.Item1);
            Assert.AreEqual(2, item.Item2);
            Assert.AreEqual(2.0f, item.Item3);

            enumerator.MoveNext();
            item = enumerator.Current;
            Assert.AreEqual(1, item.Item1);
            Assert.AreEqual(0, item.Item2);
            Assert.AreEqual(1.0f, item.Item3);

            enumerator.MoveNext();
            item = enumerator.Current;
            Assert.AreEqual(1, item.Item1);
            Assert.AreEqual(1, item.Item2);
            Assert.AreEqual(1.0f, item.Item3);

            enumerator.MoveNext();
            item = enumerator.Current;
            Assert.AreEqual(1, item.Item1);
            Assert.AreEqual(2, item.Item2);
            Assert.AreEqual(2.0f, item.Item3);

            enumerator.MoveNext();
            item = enumerator.Current;
            Assert.AreEqual(2, item.Item1);
            Assert.AreEqual(0, item.Item2);
            Assert.AreEqual(1.0f, item.Item3);

            enumerator.MoveNext();
            item = enumerator.Current;
            Assert.AreEqual(2, item.Item1);
            Assert.AreEqual(1, item.Item2);
            Assert.AreEqual(1.0f, item.Item3);

            enumerator.MoveNext();
            item = enumerator.Current;
            Assert.AreEqual(2, item.Item1);
            Assert.AreEqual(2, item.Item2);
            Assert.AreEqual(2.0f, item.Item3);
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
                    matrix[row, column] = 1.0f;
                }
            }

            var submatrix = matrix.SubMatrix(8, 2, 0, 2);
            Assert.AreEqual(2, submatrix.RowCount);
            Assert.AreEqual(2, submatrix.ColumnCount);

            for (var row = 0; row < submatrix.RowCount; row++)
            {
                for (var column = 0; column < submatrix.ColumnCount; column++)
                {
                    Assert.AreEqual(1.0f, submatrix[row, column]);
                }
            }
        }
    }
}
