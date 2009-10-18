using System;
using System.Collections.Generic;
using System.Globalization;
using MathNet.Numerics.LinearAlgebra.Double;
using MbUnit.Framework;

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Double
{
    public abstract partial class MatrixTests
    {
        protected Dictionary<string, double[,]> testData;
        protected Dictionary<string, Matrix> testMatrices;

        protected abstract Matrix CreateMatrix(int rows, int columns);
        protected abstract Matrix CreateMatrix(double[,] data);

        [SetUp]
        public void SetupDistributions()
        {
            testData = new Dictionary<string, double[,]>();
            testData.Add("Singular3x3", new double[,] { { 1, 1, 2 }, { 1, 1, 2 }, { 1, 1, 2 } });
            testData.Add("Square3x3", new double[,] { { -1.1, -2.2, -3.3 }, { 0, 1.1, 2.2 }, { -4.4, 5.5, 6.6 } });
            testData.Add("Square4x4", new double[,] { { -1.1, -2.2, -3.3, -4.4 }, { 0, 1.1, 2.2, 3.3 }, { -4.4, 5.5, 6.6, -7.7 } });
            testData.Add("Tall3x2", new double[,] { { -1.1, -2.2 }, { 0, 1.1 }, { -4.4, 5.5 } });
            testData.Add("Wide2x3", new double[,] { { -1.1, -2.2, -3.3 }, { 0, 1.1, 2.2 } });

            testMatrices = new Dictionary<string, Matrix>();
            foreach(var name in testData.Keys)
            {
                testMatrices.Add(name, CreateMatrix(testData[name]));
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
            var matrix = CreateMatrix(testData[name]);
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
            var matrix = CreateMatrix(testData[name]);
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
            var matrix1 = CreateMatrix(testData[name]);
            var matrix2 = CreateMatrix(testData[name]);
            var matrix3 = CreateMatrix(testData[name].GetLength(0), testData[name].GetLength(1));
            Assert.IsTrue(matrix1.Equals(matrix1));
            Assert.IsTrue(matrix1.Equals(matrix2));
            Assert.IsFalse(matrix1.Equals(matrix3));
            Assert.IsFalse(matrix1.Equals(null));
        }

        [Test]
        [Row(0, 2)]
        [Row(-1, 1)]
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
            var matrix = CreateMatrix(testData[name]);
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
            var matrix1 = CreateMatrix(testData[name]);
            var matrix2 = CreateMatrix(testData[name]);
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
    }
}