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
        public void CanCreateDenseVectorFromArray()
        {
            var vector = new DenseVector(_data);
            Assert.AreSame(_data, vector.Data);
            for (var i = 0; i < _data.Length; i++)
            {
                Assert.AreEqual(_data[i], vector[i]);
            }
            vector[0] = 100.0;
            Assert.AreEqual(100.0, _data[0]);
        }

        [Test]
        [MultipleAsserts]
        public void CanCreateDenseVectorFromAnotherDenseVector()
        {
            var vector = new DenseVector(_data);
            var other = new DenseVector(vector);
            
            
            Assert.AreNotSame(vector, other);
            for (var i = 0; i < _data.Length; i++)
            {
                Assert.AreEqual(vector[i], other[i]);
            }
        }

        [Test]
        [MultipleAsserts]
        public void CanCreateDenseVectorFromAnotherVector()
        {
            var vector = (Vector)new DenseVector(_data);
            var other = new DenseVector(vector);

            Assert.AreNotSame(vector, other);
            for (var i = 0; i < _data.Length; i++)
            {
                Assert.AreEqual(vector[i], other[i]);
            }
        }

        [Test]
        [MultipleAsserts]
        public void CanCreateDenseVectorFromUserDefinedVector()
        {
            var vector = new UserDefinedVector(_data);
            var other = new DenseVector(vector);

            for (var i = 0; i < _data.Length; i++)
            {
                Assert.AreEqual(vector[i], other[i]);
            }
        }

        [Test]
        public void CanCreateDenseVectorWithConstantValues()
        {
            var vector = new DenseVector(5, 5);
            Assert.ForAll(vector, value => value == 5);
        }
        
        [Test]
        [MultipleAsserts]
        public void CanCreateDenseMatrix()
        {
            var vector = new DenseVector(3);
            var matrix = vector.CreateMatrix(2, 3);
            Assert.AreEqual(2, matrix.RowCount);
            Assert.AreEqual(3, matrix.ColumnCount);
        }


        [Test]
        [MultipleAsserts]
        public void CanConvertDenseVectorToArray()
        {
            var vector = new DenseVector(_data);
            var array = (double[])vector;
            Assert.IsInstanceOfType(typeof(double[]), array);
            Assert.AreSame(vector.Data, array);
            Assert.AreElementsEqual(vector, array);
        }

        [Test]
        [MultipleAsserts]
        public void CanConvertArrayToDenseVector()
        {
            var array = new[] { 0.0, 1.0, 2.0, 3.0, 4.0 };
            var vector = (DenseVector)array;
            Assert.IsInstanceOfType(typeof(DenseVector), vector);
            Assert.AreElementsEqual(array, array);
        }

        [Test]
        public void CanCallUnaryPlusOperatorOnDenseVector()
        {
            var vector = new DenseVector(_data);
            var other = +vector;
            Assert.AreSame(vector, other, "Should be the same vector");
        }

        [Test]
        [MultipleAsserts]
        public void CanAddTwoDenseVectorsUsingOperator()
        {
            var vector = new DenseVector(_data);
            var other = new DenseVector(_data);
            var result = vector + other;

            for (var i = 0; i < _data.Length; i++)
            {
                Assert.AreEqual(_data[i], vector[i], "Making sure the original vector wasn't modified.");
                Assert.AreEqual(_data[i], other[i], "Making sure the original vector wasn't modified.");
                Assert.AreEqual(_data[i] * 2.0, result[i]);
            }
        }

        [Test]
        public void CanCallUnaryNegationOperatorOnDenseVector()
        {
            var vector = new DenseVector(_data);
            var other = -vector;
            for (var i = 0; i < _data.Length; i++)
            {
                Assert.AreEqual(-_data[i], other[i]);
            }
        }

        [Test]
        [MultipleAsserts]
        public void CanSubtractTwoDenseVectorsUsingOperator()
        {
            var vector = new DenseVector(_data);
            var other = new DenseVector(_data);
            var result = vector - other;

            for (var i = 0; i < _data.Length; i++)
            {
                Assert.AreEqual(_data[i], vector[i], "Making sure the original vector wasn't modified.");
                Assert.AreEqual(_data[i], other[i], "Making sure the original vector wasn't modified.");
                Assert.AreEqual(0.0, result[i]);
            }
        }

        [Test]
        [MultipleAsserts]
        public void CanMultiplyDenseVectorByScalarUsingOperators()
        {
            var vector = new DenseVector(_data);
            vector = vector * 2.0;

            for (var i = 0; i < _data.Length; i++)
            {
                Assert.AreEqual(_data[i] * 2.0, vector[i]);
            }

            vector = vector * 1.0;
            for (var i = 0; i < _data.Length; i++)
            {
                Assert.AreEqual(_data[i] * 2.0, vector[i]);
            }

            vector = new DenseVector(_data);
            vector = 2.0 * vector;

            for (var i = 0; i < _data.Length; i++)
            {
                Assert.AreEqual(_data[i] * 2.0, vector[i]);
            }

            vector = 1.0 * vector;
            for (var i = 0; i < _data.Length; i++)
            {
                Assert.AreEqual(_data[i] * 2.0, vector[i]);
            }
        }

        [Test]
        [MultipleAsserts]
        public void CanDivideDenseVectorByScalarUsingOperators()
        {
            var vector = new DenseVector(_data);
            vector = vector / 2.0;

            for (var i = 0; i < _data.Length; i++)
            {
                Assert.AreEqual(_data[i] / 2.0, vector[i]);
            }

            vector = vector / 1.0;
            for (var i = 0; i < _data.Length; i++)
            {
                Assert.AreEqual(_data[i] / 2.0, vector[i]);
            }
        }
    }
}
