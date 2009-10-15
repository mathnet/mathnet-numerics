using System.Collections.Generic;
using MbUnit.Framework;

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Double
{
    using LinearAlgebra.Double;

    public class DenseMatrixTests : MatrixTests
    {
        protected override Matrix CreateMatrix(int rows, int columns)
        {
            return new DenseMatrix(rows, columns);
        }

        protected override Matrix CreateMatrix(double[,] data)
        {
            return new DenseMatrix(data);
        }

        [Test]
        [Row("Singular3x3")]
        [Row("Singular3x3")]
        [Row("Square3x3")]
        [Row("Square4x4")]
        [Row("Tall3x2")]
        [Row("Wide2x3")]
        public void CanCreateMatrixFromArray(string name)
        {
            var matrix = new DenseMatrix(testData[name]);
            for (var i = 0; i < testData[name].GetLength(0); i++)
            {
                for (var j = 0; j < testData[name].GetLength(0); j++)
                {
                    Assert.AreEqual(testData[name][i,j], matrix[i,j]);
                }
            }
        }

        [Test]
        public void CanCreateMatrixWithUniformValues()
        {
            var matrix = new DenseMatrix(10, 10, 10.0);
            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    Assert.AreEqual(matrix[i, j], 10.0);
                }
            }
        }
    }
}
