using System.Collections.Generic;
using MbUnit.Framework;

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Double
{
    using LinearAlgebra.Double;

    public class DenseVectorTests : VectorTests
    {
        protected override Vector CreateVector(int size)
        {
            return new DenseVector(size);
        }

        protected override Vector CreateVector(IList<double> data)
        {
            var vector = new DenseVector(data.Count);
            for(var index = 0; index < data.Count; index++)
            {
                vector[index] = data[index];
            }

            return vector;
        }

        [Test]
        [MultipleAsserts]
        public void CanCreateVectorFromArray()
        {
            var data = new[] { 1.0, 2.0, 3.0, 4.0 };
            var vector = new DenseVector(data);
            Assert.AreSame(data, vector.Data);
            for( var i = 0; i < data.Length; i++)
            {
                Assert.AreEqual(data[i], vector[i]);
            }
            vector[0] = 100.0;
            Assert.AreEqual(100.0, data[0]);
        }

        [Test]
        [MultipleAsserts]
        public void CanCreateMatrix()
        {
            var vector = new DenseVector(3);
            var matrix = vector.CreateMatrix(2, 3);
            Assert.AreEqual(2, matrix.RowCount);
            Assert.AreEqual(3, matrix.ColumnCount);
        }
    }
}
