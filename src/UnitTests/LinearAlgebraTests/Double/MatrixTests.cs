// <copyright file="MatrixTests.cs" company="Math.NET">
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

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Double
{
	using System;
	using LinearAlgebra.Double;
	using MbUnit.Framework;

    [TestFixture]
    public abstract partial class MatrixTests : MatrixLoader
    {
        [Test]
        [Row("Singular3x3")]
        [Row("Square3x3")]
        [Row("Square4x4")]
        [Row("Tall3x2")]
        [Row("Wide2x3")]
        [MultipleAsserts]
        public void CanCloneMatrix(string name)
        {
            var matrix = CreateMatrix(testData2D[name]);
            var clone = matrix.Clone();

            Assert.AreNotSame(matrix, clone);
            Assert.AreEqual(matrix.RowCount, clone.RowCount);
            Assert.AreEqual(matrix.ColumnCount, clone.ColumnCount);
            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    Assert.AreEqual(matrix[i,j], clone[i,j]);
                }
            }
        }

        [Test]
        [Row("Singular3x3")]
        [Row("Square3x3")]
        [Row("Square4x4")]
        [Row("Tall3x2")]
        [Row("Wide2x3")]
        [MultipleAsserts]
        public void CanCloneMatrixUsingICloneable(string name)
        {
            var matrix = testMatrices[name];
            var clone = (Matrix)((ICloneable)matrix).Clone();

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

        [Test]
        [Row("Singular3x3")]
        [Row("Square3x3")]
        [Row("Square4x4")]
        [Row("Tall3x2")]
        [Row("Wide2x3")]
        [MultipleAsserts]
        public void CanCopyTo(string name)
        {
            var matrix = testMatrices[name];
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

        [Test]
        [ExpectedArgumentNullException]
        public void CopyToFailsWhenTargetIsNull()
        {
            Matrix matrix = testMatrices["Singular3x3"];
            Matrix target = null;
            matrix.CopyTo(target);
        }

        [Test]
        [ExpectedArgumentException]
        public void CopyToFailsWhenTargetHasMoreRows()
        {
            Matrix matrix = testMatrices["Singular3x3"];
            Matrix target = CreateMatrix(matrix.RowCount + 1, matrix.ColumnCount);
            matrix.CopyTo(target);
        }

        [Test]
        [ExpectedArgumentException]
        public void CopyToFailsWhenTargetHasMoreColumns()
        {
            Matrix matrix = testMatrices["Singular3x3"];
            Matrix target = CreateMatrix(matrix.RowCount + 1, matrix.ColumnCount);
            matrix.CopyTo(target);
        }

        [Test]
        [Ignore]
        public void CanConvertVectorToString()
        {
        }

        [Test]
        public void CanCreateMatrix()
        {
            var expected = CreateMatrix(5, 6);
            var actual = expected.CreateMatrix(5, 6);
            Assert.AreEqual(expected.GetType(), actual.GetType(), "Matrices are same type.");
        }

        [Test]
        [Row("Singular3x3")]
        [Row("Square3x3")]
        [Row("Square4x4")]
        [Row("Tall3x2")]
        [Row("Wide2x3")]
        [MultipleAsserts]
        public void CanEquateMatrices(string name)
        {
            var matrix1 = CreateMatrix(testData2D[name]);
            var matrix2 = CreateMatrix(testData2D[name]);
            var matrix3 = CreateMatrix(testData2D[name].GetLength(0), testData2D[name].GetLength(1));
            Assert.IsTrue(matrix1.Equals(matrix1));
            Assert.IsTrue(matrix1.Equals(matrix2));
            Assert.IsFalse(matrix1.Equals(matrix3));
            Assert.IsFalse(matrix1.Equals(null));
        }

        [Test]
        [Row(0, 2)]
        [Row(2, 0)]
        [Row(0, 0)]
        [Row(-1, 1)]
        [Row(1, -1)]
        [ExpectedArgumentOutOfRangeException]
        public void ThrowsArgumentExceptionIfSizeIsNotPositive(int rows, int columns)
        {
            var A = CreateMatrix(rows, columns);
        }

        [Test]
        [Row("Singular3x3")]
        [Row("Square3x3")]
        [Row("Square4x4")]
        [Row("Tall3x2")]
        [Row("Wide2x3")]
        public void TestingForEqualityWithNonMatrixReturnsFalse(string name)
        {
            var matrix = CreateMatrix(testData2D[name]);
            Assert.IsFalse(matrix.Equals(2));
        }

        [Test]
        [Row("Singular3x3")]
        [Row("Square3x3")]
        [Row("Square4x4")]
        [Row("Tall3x2")]
        [Row("Wide2x3")]
        public void CanTestForEqualityUsingObjectEquals(string name)
        {
            var matrix1 = CreateMatrix(testData2D[name]);
            var matrix2 = CreateMatrix(testData2D[name]);
            Assert.IsTrue(matrix1.Equals((object)matrix2));
        }

        [Test]
        [Row(-1, 1, "Singular3x3")]
        [Row(1, -1, "Singular3x3")]
        [Row(4, 2, "Square3x3")]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void RangeCheckFails(int i, int j, string name)
        {
            var d = testMatrices[name][i, j];
        }

        [Test]
        [Ignore]
        public void MatrixGetHashCode()
        {
        }

        [Test]
        public void CanClearMatrix()
        {
            Matrix matrix = (Matrix) testMatrices["Singular3x3"].Clone();
            matrix.Clear();
            for (int i = 0; i < matrix.RowCount; i++)
            {
                for (int j = 0; j < matrix.ColumnCount; j++)
                {
                    Assert.AreEqual(0, matrix[i, j]);
                }
            }
        }

        [Test]
        [Row(0, "Singular3x3")]
        [Row(1, "Singular3x3")]
        [Row(2, "Singular3x3")]
        [Row(2, "Square3x3")]
        public void CanGetRow(int rowIndex, string name)
        {
            var matrix = testMatrices[name];
            var row = matrix.GetRow(rowIndex);

            Assert.AreEqual(matrix.ColumnCount, row.Count);
            for (int j = 0; j < matrix.ColumnCount; j++)
            {
                Assert.AreEqual(matrix[rowIndex, j], row[j]);
            }
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void GetRowThrowsArgumentOutOfRangeWithNegativeIndex()
        {
            var matrix = testMatrices["Singular3x3"];
            matrix.GetRow(-1);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void GetRowThrowsArgumentOutOfRangeWithOverflowingRowIndex()
        {
            var matrix = testMatrices["Singular3x3"];
            matrix.GetRow(matrix.RowCount);
        }

        [Test]
        [Row(0, "Singular3x3")]
        [Row(1, "Singular3x3")]
        [Row(2, "Singular3x3")]
        [Row(2, "Square3x3")]
        public void CanGetRowWithResult(int rowIndex, string name)
        {
            var matrix = testMatrices[name];
            var row = CreateVector(matrix.ColumnCount);
            matrix.GetRow(rowIndex, row);

            Assert.AreEqual(matrix.ColumnCount, row.Count);
            for (int j = 0; j < matrix.ColumnCount; j++)
            {
                Assert.AreEqual(matrix[rowIndex, j], row[j]);
            }
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetRowWithResultFailsWhenResultIsNull()
        {
            var matrix = testMatrices["Singular3x3"];
            matrix.GetRow(0, null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void GetRowWithResultThrowsArgumentOutOfRangeWithNegativeIndex()
        {
            var matrix = testMatrices["Singular3x3"];
            var row = CreateVector(matrix.ColumnCount);
            matrix.GetRow(-1, row);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void GetRowWithResultThrowsArgumentOutOfRangeWithOverflowingRowIndex()
        {
            var matrix = testMatrices["Singular3x3"];
            var row = CreateVector(matrix.ColumnCount);
            matrix.GetRow(matrix.RowCount, row);
        }

        [Test]
        [Row(0, 0, 1, "Singular3x3")]
        [Row(1, 1, 2, "Singular3x3")]
        [Row(2, 0, 3, "Singular3x3")]
        [Row(2, 0, 3, "Square3x3")]
        public void CanGetRowWithRange(int rowIndex, int start, int length, string name)
        {
            var matrix = testMatrices[name];
            var row = matrix.GetRow(rowIndex, start, length);

            Assert.AreEqual(length, row.Count);
            for (int j = start; j < start + length; j++)
            {
                Assert.AreEqual(matrix[rowIndex, j], row[j - start]);
            }
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void GetRowWithRangeResultArgumentExeptionWhenLengthIsZero()
        {
            var matrix = testMatrices["Singular3x3"];
            var result = CreateVector(matrix.ColumnCount);
            matrix.GetRow(0, 0, 0, result);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void GetRowWithRangeFailsWithTooSmallResultVector()
        {
            var matrix = testMatrices["Singular3x3"];
            var result = CreateVector(matrix.ColumnCount - 1);
            matrix.GetRow(0, 0, 0, result);
        }

        [Test]
        [Row(0, "Singular3x3")]
        [Row(1, "Singular3x3")]
        [Row(2, "Singular3x3")]
        [Row(2, "Square3x3")]
        public void CanGetColumn(int colIndex, string name)
        {
            var matrix = testMatrices[name];
            var col = matrix.GetColumn(colIndex);

            Assert.AreEqual(matrix.RowCount, col.Count);
            for (int j = 0; j < matrix.RowCount; j++)
            {
                Assert.AreEqual(matrix[j, colIndex], col[j]);
            }
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void GetColumnThrowsArgumentOutOfRangeWithNegativeIndex()
        {
            var matrix = testMatrices["Singular3x3"];
            matrix.GetColumn(-1);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void GetColumnThrowsArgumentOutOfRangeWithOverflowingRowIndex()
        {
            var matrix = testMatrices["Singular3x3"];
            matrix.GetColumn(matrix.ColumnCount);
        }

        [Test]
        [Row(0, "Singular3x3")]
        [Row(1, "Singular3x3")]
        [Row(2, "Singular3x3")]
        [Row(2, "Square3x3")]
        public void CanGetColumnWithResult(int colIndex, string name)
        {
            var matrix = testMatrices[name];
            var col = CreateVector(matrix.RowCount);
            matrix.GetColumn(colIndex, col);

            Assert.AreEqual(matrix.RowCount, col.Count);
            for (int j = 0; j < matrix.RowCount; j++)
            {
                Assert.AreEqual(matrix[j, colIndex], col[j]);
            }
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetColumnFailsWhenResultIsNull()
        {
            var matrix = testMatrices["Singular3x3"];
            matrix.GetColumn(0, null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void GetColumnWithResultThrowsArgumentOutOfRangeWithNegativeIndex()
        {
            var matrix = testMatrices["Singular3x3"];
            var column = CreateVector(matrix.ColumnCount);
            matrix.GetColumn(-1, column);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void GetColumnWithResultThrowsArgumentOutOfRangeWithOverflowingRowIndex()
        {
            var matrix = testMatrices["Singular3x3"];
            var column = CreateVector(matrix.RowCount);
            matrix.GetRow(matrix.ColumnCount, column);
        }

        [Test]
        [Row(0, 0, 1, "Singular3x3")]
        [Row(1, 1, 2, "Singular3x3")]
        [Row(2, 0, 3, "Singular3x3")]
        [Row(2, 0, 3, "Square3x3")]
        public void CanGetColumnWithRange(int colIndex, int start, int length, string name)
        {
            var matrix = testMatrices[name];
            var col = matrix.GetColumn(colIndex, start, length);

            Assert.AreEqual(length, col.Count);
            for (int j = start; j < start+length; j++)
            {
                Assert.AreEqual(matrix[j, colIndex], col[j - start]);
            }
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void GetColumnWithRangeResultArgumentExeptionWhenLengthIsZero()
        {
            var matrix = testMatrices["Singular3x3"];
            var col = CreateVector(matrix.RowCount);
            matrix.GetColumn(0, 0, 0, col);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void GetColumnWithRangeFailsWithTooSmallResultVector()
        {
            var matrix = testMatrices["Singular3x3"];
            Vector result = CreateVector(matrix.RowCount - 1);
            matrix.GetColumn(0, 0, matrix.RowCount, result);
        }

        [Test]
        [Row("Singular3x3")]
        [Row("Square3x3")]
        [Row("Square4x4")]
        [Row("Tall3x2")]
        [Row("Wide2x3")]
        [MultipleAsserts]
        public void CanTransposeMatrix(string name)
        {
            var matrix = CreateMatrix(testData2D[name]);
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
    }
}