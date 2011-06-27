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
        /// Can clone a matrix.
        /// </summary>
        /// <param name="name">Matrix name.</param>
        [TestCase("Singular3x3")]
        [TestCase("Square3x3")]
        [TestCase("Square4x4")]
        [TestCase("Tall3x2")]
        [TestCase("Wide2x3")]
        public void CanCloneMatrix(string name)
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
        [TestCase("Singular3x3")]
        [TestCase("Square3x3")]
        [TestCase("Square4x4")]
        [TestCase("Tall3x2")]
        [TestCase("Wide2x3")]
        public void CanCloneMatrixUsingICloneable(string name)
        {
            var matrix = TestMatrices[name];
            var clone = (Matrix<Complex>)((ICloneable)matrix).Clone();

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
        [TestCase("Singular3x3")]
        [TestCase("Square3x3")]
        [TestCase("Square4x4")]
        [TestCase("Tall3x2")]
        [TestCase("Wide2x3")]
        public void CanCopyTo(string name)
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
            Matrix<Complex> target = null;
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
        [TestCase("Singular3x3")]
        [TestCase("Square3x3")]
        [TestCase("Square4x4")]
        [TestCase("Tall3x2")]
        [TestCase("Wide2x3")]
        public void CanEquateMatrices(string name)
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
        [TestCase(0, 2)]
        [TestCase(2, 0)]
        [TestCase(0, 0)]
        [TestCase(-1, 1)]
        [TestCase(1, -1)]
        public void IfSizeIsNotPositiveThrowsArgumentException(int rows, int columns)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => CreateMatrix(rows, columns));
        }

        /// <summary>
        /// Testing for equality with non-matrix returns <c>false</c>.
        /// </summary>
        /// <param name="name">Matrix name.</param>
        [TestCase("Singular3x3")]
        [TestCase("Square3x3")]
        [TestCase("Square4x4")]
        [TestCase("Tall3x2")]
        [TestCase("Wide2x3")]
        public void TestingForEqualityWithNonMatrixReturnsFalse(string name)
        {
            var matrix = CreateMatrix(TestData2D[name]);
            Assert.IsFalse(matrix.Equals(2));
        }

        /// <summary>
        /// Can test for equality using Object.Equals.
        /// </summary>
        /// <param name="name">Matrix name.</param>
        [TestCase("Singular3x3")]
        [TestCase("Square3x3")]
        [TestCase("Square4x4")]
        [TestCase("Tall3x2")]
        [TestCase("Wide2x3")]
        public void CanTestForEqualityUsingObjectEquals(string name)
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
        [TestCase(-1, 1, "Singular3x3")]
        [TestCase(1, -1, "Singular3x3")]
        [TestCase(4, 2, "Square3x3")]
        public void RangeCheckWithInvalidIndicesThrowsArgumentOutOfRangeException(int i, int j, string name)
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
                    Assert.AreEqual(Complex.Zero, matrix[i, j]);
                }
            }
        }

        /// <summary>
        /// Can get a row of a matrix.
        /// </summary>
        /// <param name="rowIndex">Row index.</param>
        /// <param name="name">Matrix name.</param>
        [TestCase(0, "Singular3x3")]
        [TestCase(1, "Square3x3")]
        [TestCase(2, "Square3x3")]
        public void CanGetRow(int rowIndex, string name)
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
        [TestCase(0, "Singular3x3")]
        [TestCase(1, "Square3x3")]
        [TestCase(2, "Square3x3")]
        public void CanGetRowIntoResult(int rowIndex, string name)
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
        [TestCase(0, 0, 1, "Singular3x3")]
        [TestCase(1, 1, 2, "Singular3x3")]
        [TestCase(2, 0, 3, "Singular3x3")]
        [TestCase(2, 0, 3, "Square3x3")]
        public void CanGetRowWithRange(int rowIndex, int start, int length, string name)
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
        [TestCase(0, "Singular3x3")]
        [TestCase(1, "Square3x3")]
        [TestCase(2, "Square3x3")]
        public void CanGetColumn(int colIndex, string name)
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
        [TestCase(0, "Singular3x3")]
        [TestCase(1, "Square3x3")]
        [TestCase(2, "Square3x3")]
        public void CanGetColumnIntoResult(int colIndex, string name)
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
        [TestCase(0, 0, 1, "Singular3x3")]
        [TestCase(1, 1, 2, "Singular3x3")]
        [TestCase(2, 0, 3, "Singular3x3")]
        [TestCase(2, 0, 3, "Square3x3")]
        public void CanGetColumnWithRange(int colIndex, int start, int length, string name)
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
        [TestCase(0, "Singular3x3")]
        [TestCase(1, "Square3x3")]
        [TestCase(2, "Square3x3")]
        public void CanSetRow(int rowIndex, string name)
        {
            var matrix = TestMatrices[name].Clone();
            matrix.SetRow(rowIndex, CreateVector(matrix.ColumnCount));

            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    Assert.AreEqual(i == rowIndex ? Complex.Zero : TestMatrices[name][i, j], matrix[i, j]);
                }
            }
        }

        /// <summary>
        /// Can set a column.
        /// </summary>
        /// <param name="colIndex">Column index.</param>
        /// <param name="name">Matrix name.</param>
        [TestCase(0, "Singular3x3")]
        [TestCase(1, "Square3x3")]
        [TestCase(2, "Square3x3")]
        public void CanSetColumn(int colIndex, string name)
        {
            var matrix = TestMatrices[name].Clone();
            matrix.SetColumn(colIndex, CreateVector(matrix.ColumnCount));

            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    Assert.AreEqual(j == colIndex ? Complex.Zero : TestMatrices[name][i, j], matrix[i, j]);
                }
            }
        }

        /// <summary>
        /// Get an upper triangle matrix.
        /// </summary>
        /// <param name="name">Matrix name.</param>
        [TestCase("Singular3x3")]
        [TestCase("Square3x3")]
        [TestCase("Square4x4")]
        [TestCase("Tall3x2")]
        [TestCase("Wide2x3")]
        public void CanUpperTriangle(string name)
        {
            var data = TestMatrices[name];
            var upper = data.UpperTriangle();
            for (var i = 0; i < data.RowCount; i++)
            {
                for (var j = 0; j < data.ColumnCount; j++)
                {
                    Assert.AreEqual(i <= j ? data[i, j] : Complex.Zero, upper[i, j]);
                }
            }
        }

        /// <summary>
        /// Get an upper triangle matrix into a result matrix.
        /// </summary>
        /// <param name="name">Matrix name.</param>
        [TestCase("Singular3x3")]
        [TestCase("Square3x3")]
        [TestCase("Square4x4")]
        [TestCase("Tall3x2")]
        [TestCase("Wide2x3")]
        public void CanUpperTriangleIntoResult(string name)
        {
            var data = TestMatrices[name];
            var result = CreateMatrix(data.RowCount, data.ColumnCount);
            data.UpperTriangle(result);
            for (var i = 0; i < data.RowCount; i++)
            {
                for (var j = 0; j < data.ColumnCount; j++)
                {
                    Assert.AreEqual(i <= j ? data[i, j] : Complex.Zero, result[i, j]);
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
            Matrix<Complex> result = null;
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
        [TestCase("Singular3x3")]
        [TestCase("Square3x3")]
        [TestCase("Square4x4")]
        [TestCase("Tall3x2")]
        [TestCase("Wide2x3")]
        public void CanLowerTriangle(string name)
        {
            var data = TestMatrices[name];
            var lower = data.LowerTriangle();
            for (var i = 0; i < data.RowCount; i++)
            {
                for (var j = 0; j < data.ColumnCount; j++)
                {
                    Assert.AreEqual(i >= j ? data[i, j] : Complex.Zero, lower[i, j]);
                }
            }
        }

        /// <summary>
        /// Get a lower triangle matrix into a result matrix.
        /// </summary>
        /// <param name="name">Matrix name.</param>
        [TestCase("Singular3x3")]
        [TestCase("Square3x3")]
        [TestCase("Square4x4")]
        [TestCase("Tall3x2")]
        [TestCase("Wide2x3")]
        public void CanLowerTriangleIntoResult(string name)
        {
            var data = TestMatrices[name];
            var result = CreateMatrix(data.RowCount, data.ColumnCount);
            data.LowerTriangle(result);
            for (var i = 0; i < data.RowCount; i++)
            {
                for (var j = 0; j < data.ColumnCount; j++)
                {
                    Assert.AreEqual(i >= j ? data[i, j] : Complex.Zero, result[i, j]);
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
            Matrix<Complex> result = null;
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
                        Assert.AreEqual(i > j ? data[i, j] : Complex.Zero, lower[i, j]);
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
                        Assert.AreEqual(i > j ? data[i, j] : Complex.Zero, lower[i, j]);
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
            Matrix<Complex> lower = null;
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
                        Assert.AreEqual(i < j ? data[i, j] : Complex.Zero, lower[i, j]);
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
                        Assert.AreEqual(i < j ? data[i, j] : Complex.Zero, lower[i, j]);
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
            Matrix<Complex> lower = null;
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
        /// Can set a column with an array.
        /// </summary>
        /// <param name="name">Matrix name.</param>
        /// <param name="real">Column real values array.</param>
        [TestCase("Singular3x3", new double[] { 1, 2, 3 })]
        [TestCase("Square3x3", new double[] { 1, 2, 3 })]
        [TestCase("Tall3x2", new double[] { 1, 2, 3 })]
        [TestCase("Wide2x3", new double[] { 1, 2 })]
        public virtual void CanSetColumnWithArray(string name, double[] real)
        {
            var column = new Complex[real.Length];
            for (var i = 0; i < real.Length; i++)
            {
                column[i] = new Complex(real[i], 1);
            }

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
            Complex[] vec = null;
            Assert.Throws<ArgumentNullException>(() => TestMatrices["Singular3x3"].SetColumn(1, vec));
        }

        /// <summary>
        /// Set a column with unequal array length throws <c>ArgumentException</c>.
        /// </summary>
        [Test]
        public virtual void SetColumnWithArrayUnequalLengthThrowsArgumentException()
        {
            var array = new Complex[] { 1, 2, 3, 4, 5 };
            Assert.Throws<ArgumentException>(() => TestMatrices["Singular3x3"].SetColumn(1, array));
        }

        /// <summary>
        /// Set a column array with invalid column index throws <c>ArgumentOutOfRangeException</c>.
        /// </summary>
        [Test]
        public void SetColumnWithArrayWithInvalidColumnIndexThrowsArgumentOutOfRangeException()
        {
            var matrix = TestMatrices["Square3x3"];
            Complex[] column = { 1, 2, 3 };
            Assert.Throws<ArgumentOutOfRangeException>(() => matrix.SetColumn(matrix.ColumnCount + 1, column));
            Assert.Throws<ArgumentOutOfRangeException>(() => matrix.SetColumn(-1, column));
        }

        /// <summary>
        /// Can set a column with a vector.
        /// </summary>
        /// <param name="name">Matrix name.</param>
        /// <param name="real">Column real values.</param>
        [TestCase("Singular3x3", new double[] { 1, 2, 3 })]
        [TestCase("Square3x3", new double[] { 1, 2, 3 })]
        [TestCase("Tall3x2", new double[] { 1, 2, 3 })]
        [TestCase("Wide2x3", new double[] { 1, 2 })]
        public virtual void CanSetColumnWithVector(string name, double[] real)
        {
            var column = new Complex[real.Length];
            for (var i = 0; i < real.Length; i++)
            {
                column[i] = new Complex(real[i], 1);
            }

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
            var columnVector = CreateVector(new Complex[] { 1, 2, 3, 4, 5 });
            Assert.Throws<ArgumentException>(() => matrix.SetColumn(1, columnVector));
        }

        /// <summary>
        /// Set a column with <c>null</c> vector throw <c>ArgumentNullException</c>.
        /// </summary>
        [Test]
        public void SetColumnWithNullVectorThrowsArgumentNullException()
        {
            var matrix = TestMatrices["Square3x3"];
            Vector<Complex> columnVector = null;
            Assert.Throws<ArgumentNullException>(() => matrix.SetColumn(1, columnVector));
        }

        /// <summary>
        /// Set column vector with invalid column index throws <c>ArgumentOutOfRangeException</c>.
        /// </summary>
        [Test]
        public void SetColumnWithVectorWithInvalidColumnIndexThrowsArgumentOutOfRangeException()
        {
            var matrix = TestMatrices["Square3x3"];
            var column = CreateVector(new Complex[] { 1, 2, 3 });
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
        /// Can set a row with an array.
        /// </summary>
        /// <param name="name">Matrix name.</param>
        /// <param name="real">Row index.</param>
        [TestCase("Singular3x3", new double[] { 1, 2, 3 })]
        [TestCase("Square3x3", new double[] { 1, 2, 3 })]
        [TestCase("Tall3x2", new double[] { 1, 2 })]
        [TestCase("Wide2x3", new double[] { 1, 2, 3 })]
        public virtual void CanSetRowWithArray(string name, double[] real)
        {
            var row = new Complex[real.Length];
            for (var i = 0; i < real.Length; i++)
            {
                row[i] = new Complex(real[i], 1);
            }

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
            Complex[] arr = null;
            Assert.Throws<ArgumentNullException>(() => TestMatrices["Square3x3"].SetRow(1, arr));
        }

        /// <summary>
        /// Set a row with an array with invalid row index throws <c>ArgumentOutOfRangeException</c>.
        /// </summary>
        [Test]
        public void SetRowWithArrayWithInvalidRowIndexThrowsArgumentOutOfRangeException()
        {
            var matrix = TestMatrices["Square3x3"];
            Complex[] row = { 1, 2, 3 };
            Assert.Throws<ArgumentOutOfRangeException>(() => matrix.SetRow(-1, row));
            Assert.Throws<ArgumentOutOfRangeException>(() => matrix.SetRow(matrix.RowCount + 1, row));
        }

        /// <summary>
        /// Can set a row with a vector.
        /// </summary>
        /// <param name="name">Matrix name.</param>
        /// <param name="real">Row real values.</param>
        [TestCase("Singular3x3", new double[] { 1, 2, 3 })]
        [TestCase("Square3x3", new double[] { 1, 2, 3 })]
        [TestCase("Tall3x2", new double[] { 1, 2 })]
        [TestCase("Wide2x3", new double[] { 1, 2, 3 })]
        public virtual void CanSetRowWithVector(string name, double[] real)
        {
            var row = new Complex[real.Length];
            for (var i = 0; i < real.Length; i++)
            {
                row[i] = new Complex(real[i], 1);
            }

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
            var vec = CreateVector(new Complex[] { 1, 2, 3, 4, 5 });
            Assert.Throws<ArgumentException>(() => TestMatrices["Square3x3"].SetRow(1, vec));
        }

        /// <summary>
        /// Set a row with a <c>null</c> vector throws <c>ArgumentNullException</c>.
        /// </summary>
        [Test]
        public void SetRowWithNullVectorThrowsArgumentNullException()
        {
            var matrix = TestMatrices["Square3x3"];
            Vector<Complex> rowVector = null;
            Assert.Throws<ArgumentNullException>(() => matrix.SetRow(1, rowVector));
        }

        /// <summary>
        /// Set a row with a vector with invalid row index throws <c>ArgumentOutOfRangeException</c>.
        /// </summary>
        [Test]
        public void SetRowWithVectorWithInvalidRowIndexThrowsArgumentOutOfRangeException()
        {
            var matrix = TestMatrices["Square3x3"];
            var row = CreateVector(new Complex[] { 1, 2, 3 });
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
        /// Can set a diagonal vector.
        /// </summary>
        /// <param name="name">Matrix name.</param>
        /// <param name="real">Diagonal real values.</param>
        [TestCase("Square3x3", new double[] { 1, 2, 3 })]
        [TestCase("Wide2x3", new double[] { 1, 2 })]
        [TestCase("Tall3x2", new double[] { 1, 2 })]
        public void CanSetDiagonalVector(string name, double[] real)
        {
            var diagonal = new Complex[real.Length];
            for (var i = 0; i < real.Length; i++)
            {
                diagonal[i] = new Complex(real[i], 1);
            }

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
            var vector = CreateVector(new Complex[] { 1, 2, 3 });
            Assert.Throws<ArgumentException>(() => TestMatrices["Wide2x3"].SetDiagonal(vector));
        }

        /// <summary>
        /// Set a diagonal with <c>null</c> vector throws <c>ArgumentNullException</c>.
        /// </summary>
        [Test]
        public void SetDiagonalWithNullVectorThrowsArgumentNullException()
        {
            Vector<Complex> vector = null;
            Assert.Throws<ArgumentNullException>(() => TestMatrices["Square3x3"].SetDiagonal(vector));
        }

        /// <summary>
        /// Can set a diagonal array.
        /// </summary>
        /// <param name="name">Matrix name.</param>
        /// <param name="real">Diagonal real values.</param>
        [TestCase("Square3x3", new double[] { 1, 2, 3 })]
        [TestCase("Wide2x3", new double[] { 1, 2 })]
        [TestCase("Tall3x2", new double[] { 1, 2 })]
        public void CanSetDiagonalArray(string name, double[] real)
        {
            var diagonal = new Complex[real.Length];
            for (var i = 0; i < real.Length; i++)
            {
                diagonal[i] = new Complex(real[i], 1);
            }

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
            var array = new Complex[] { 1, 2, 3 };
            Assert.Throws<ArgumentException>(() => TestMatrices["Wide2x3"].SetDiagonal(array));
        }

        /// <summary>
        /// Set a diagonal with <c>null</c> array throws <c>ArgumentNullException</c>.
        /// </summary>
        [Test]
        public void SetDiagonalWithNullArrayThrowsArgumentNullException()
        {
            Complex[] array = null;
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
            var enumerator = matrix.IndexedEnumerator().GetEnumerator();

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
