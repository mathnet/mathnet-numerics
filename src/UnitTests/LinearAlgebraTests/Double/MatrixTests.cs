using System;
using System.Collections.Generic;
using System.Globalization;
using MathNet.Numerics.LinearAlgebra.Double;
using MbUnit.Framework;

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Double
{
    [TestFixture]
    public abstract partial class MatrixTests
    {
        protected Dictionary<string, double[,]> testData2D;
        protected Dictionary<string, Matrix> testMatrices;

        protected abstract Matrix CreateMatrix(int rows, int columns);
        protected abstract Matrix CreateMatrix(double[,] data);
        protected abstract Vector CreateVector(int size);
        protected abstract Vector CreateVector(double[] data);

        [SetUp]
        public void SetupMatrices()
        {
            testData2D = new Dictionary<string, double[,]>();
            testData2D.Add("Singular3x3", new double[,] { { 1, 1, 2 }, { 1, 1, 2 }, { 1, 1, 2 } });
            testData2D.Add("Square3x3", new double[,] { { -1.1, -2.2, -3.3 }, { 0, 1.1, 2.2 }, { -4.4, 5.5, 6.6 } });
            testData2D.Add("Square4x4", new double[,] { { -1.1, -2.2, -3.3, -4.4 }, { 0, 1.1, 2.2, 3.3 }, { 1.0, 2.1, 6.2, 4.3 }, { -4.4, 5.5, 6.6, -7.7 } });
            testData2D.Add("Singular4x4", new double[,] { { -1.1, -2.2, -3.3, -4.4 }, { -1.1, -2.2, -3.3, -4.4 }, { -1.1, -2.2, -3.3, -4.4 }, { -1.1, -2.2, -3.3, -4.4 } });
            testData2D.Add("Tall3x2", new double[,] { { -1.1, -2.2 }, { 0, 1.1 }, { -4.4, 5.5 } });
            testData2D.Add("Wide2x3", new double[,] { { -1.1, -2.2, -3.3 }, { 0, 1.1, 2.2 } });

            testMatrices = new Dictionary<string, Matrix>();
            foreach(var name in testData2D.Keys)
            {
                testMatrices.Add(name, CreateMatrix(testData2D[name]));
            }
        }

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
            var matrix = CreateMatrix(testData2D[name]);
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

        #region Elementary operations
        [Test]
        [Row("Singular3x3", "Square3x3")]
        [Row("Singular4x4", "Square4x4")]
        public void AddMatrix(string mtxA, string mtxB)
        {
            var A = testMatrices[mtxA];
            var B = testMatrices[mtxB];

            Matrix matrix = A.Clone();
            matrix.Add(B);
            for (int i = 0; i < matrix.RowCount; i++)
            {
                for (int j = 0; j < matrix.ColumnCount; j++)
                {
                    Assert.AreEqual(matrix[i, j], A[i, j] + B[i, j]);
                }
            }
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddMatrixThrowsExceptionWhenArgumentIsNull()
        {
            Matrix matrix = testMatrices["Singular4x4"];
            Matrix other = null;
            matrix.Add(other);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void AddMatrixThrowsExceptionArgumentHasTooFewColumns()
        {
            Matrix matrix = testMatrices["Singular3x3"];
            Matrix other = testMatrices["Tall3x2"];
            matrix.Add(other);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void AddMatrixThrowsExceptionArgumentHasTooFewRows()
        {
            Matrix matrix = testMatrices["Singular3x3"];
            Matrix other = testMatrices["Wide2x3"];
            matrix.Add(other);
        }

        [Test]
        [Row("Singular3x3", "Square3x3")]
        [Row("Singular4x4", "Square4x4")]
        public void AddOperator(string mtxA, string mtxB)
        {
            var A = testMatrices[mtxA];
            var B = testMatrices[mtxB];
            
            Matrix result = A + B;
            for (int i = 0; i < A.RowCount; i++)
            {
                for (int j = 0; j < A.ColumnCount; j++)
                {
                    Assert.AreEqual(result[i,j], A[i, j] + B[i, j]);
                }
            }
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddOperatorThrowsExceptionWhenLeftsideIsNull()
        {
            Matrix matrix = null;
            Matrix other = testMatrices["Singular3x3"];
            Matrix result = matrix + other;
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddOperatorThrowsExceptionWhenRightsideIsNull()
        {
            Matrix matrix = testMatrices["Singular3x3"];
            Matrix other = null;
            Matrix result = matrix + other;
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void AddOperatorThrowsExceptionWhenRightsideHasTooFewColumns()
        {
            Matrix matrix = testMatrices["Singular3x3"];
            Matrix other = testMatrices["Tall3x2"];
            Matrix result = matrix + other;
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void AddOperatorThrowsExceptionWhenRightsideHasTooFewRows()
        {
            Matrix matrix = testMatrices["Singular3x3"];
            Matrix other = testMatrices["Wide2x3"];
            Matrix result = matrix + other;
        }

        [Test]
        [Row("Singular3x3", "Square3x3")]
        [Row("Singular4x4", "Square4x4")]
        public void SubtractMatrix(string mtxA, string mtxB)
        {
            var A = testMatrices[mtxA];
            var B = testMatrices[mtxB];

            Matrix matrix = A.Clone();
            matrix.Subtract(B);
            for (int i = 0; i < matrix.RowCount; i++)
            {
                for (int j = 0; j < matrix.ColumnCount; j++)
                {
                    Assert.AreEqual(matrix[i, j], A[i, j] - B[i, j]);
                }
            }
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SubtractMatrixThrowsExceptionWhenRightSideIsNull()
        {
            Matrix matrix = testMatrices["Singular4x4"];
            Matrix other = null;
            matrix.Subtract(other);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void SubtractMatrixThrowsExceptionWhenRightSideHasTooFewColumns()
        {
            Matrix matrix = testMatrices["Singular3x3"];
            Matrix other = testMatrices["Tall3x2"];
            matrix.Subtract(other);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void SubtractMatrixThrowsExceptionWhenRightSideHasTooFewRows()
        {
            Matrix matrix = testMatrices["Singular3x3"];
            Matrix other = testMatrices["Wide2x3"];
            matrix.Subtract(other);
        }

        [Test]
        [Row("Singular3x3", "Square3x3")]
        [Row("Singular4x4", "Square4x4")]
        public void SubtractOperator(string mtxA, string mtxB)
        {
            var A = testMatrices[mtxA];
            var B = testMatrices[mtxB];

            Matrix result = A - B;
            for (int i = 0; i < A.RowCount; i++)
            {
                for (int j = 0; j < A.ColumnCount; j++)
                {
                    Assert.AreEqual(result[i, j], A[i, j] - B[i, j]);
                }
            }
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SubtractOperatorThrowsExceptionWhenLeftsideIsNull()
        {
            Matrix matrix = null;
            Matrix other = testMatrices["Singular3x3"];
            Matrix result = matrix - other;
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SubtractOperatorThrowsExceptionWhenRightsideIsNull()
        {
            Matrix matrix = testMatrices["Singular3x3"];
            Matrix other = null;
            Matrix result = matrix - other;
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void SubtractOperatorThrowsExceptionWhenRightsideHasTooFewColumns()
        {
            Matrix matrix = testMatrices["Singular3x3"];
            Matrix other = testMatrices["Tall3x2"];
            Matrix result = matrix - other;
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void SubtractOperatorThrowsExceptionWhenRightsideHasTooFewRows()
        {
            Matrix matrix = testMatrices["Singular3x3"];
            Matrix other = testMatrices["Wide2x3"];
            Matrix result = matrix - other;
        }

        [Test]
        [Row("Singular3x3", "Square3x3")]
        [Row("Singular4x4", "Square4x4")]
        [Row("Wide2x3", "Square3x3")]
        [Row("Wide2x3", "Tall3x2")]
        [Row("Tall3x2", "Wide2x3")]
        [MultipleAsserts]
        public void CanMultiplyMatrixWithMatrix(string nameA, string nameB)
        {
            Matrix A = testMatrices[nameA];
            Matrix B = testMatrices[nameB];
            Matrix C = A * B;

            Assert.AreEqual(C.RowCount, A.RowCount);
            Assert.AreEqual(C.ColumnCount, B.ColumnCount);

            for (int i = 0; i < C.RowCount; i++)
            {
                for (int j = 0; j < C.ColumnCount; j++)
                {
                    Assert.AreEqual(A.GetRow(i) * B.GetColumn(j), C[i, j]);
                }
            }
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void MultiplyMatrixMatrixFailsWhenSizesAreIncompatible()
        {
            Matrix matrix = testMatrices["Singular3x3"];
            Matrix other = testMatrices["Wide2x3"];
            Matrix result = matrix * other;
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void MultiplyMatrixMatrixFailsWhenLeftArgumentIsNull()
        {
            Matrix matrix = null;
            Matrix other = testMatrices["Wide2x3"];
            Matrix result = matrix * other;
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void MultiplyMatrixMatrixFailsWhenRightArgumentIsNull()
        {
            Matrix matrix = testMatrices["Wide2x3"];
            Matrix other = null;
            Matrix result = matrix * other;
        }

        [Test]
        [Row("Singular3x3", "Square3x3")]
        [Row("Singular4x4", "Square4x4")]
        [Row("Wide2x3", "Square3x3")]
        [Row("Wide2x3", "Tall3x2")]
        [Row("Tall3x2", "Wide2x3")]
        [MultipleAsserts]
        public void CanMultiplyMatrixWithMatrixIntoResult(string nameA, string nameB)
        {
            Matrix A = testMatrices[nameA];
            Matrix B = testMatrices[nameB];
            Matrix C = CreateMatrix(A.RowCount, B.ColumnCount);
            A.Multiply(B, C);

            Assert.AreEqual(C.RowCount, A.RowCount);
            Assert.AreEqual(C.ColumnCount, B.ColumnCount);

            for (int i = 0; i < C.RowCount; i++)
            {
                for (int j = 0; j < C.ColumnCount; j++)
                {
                    Assert.AreEqual(A.GetRow(i) * B.GetColumn(j), C[i, j]);
                }
            }
        }

        #endregion
    }
}