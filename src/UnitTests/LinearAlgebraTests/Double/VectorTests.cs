using System;
using System.Collections.Generic;
using System.Globalization;
using MathNet.Numerics.LinearAlgebra.Double;
using MbUnit.Framework;

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Double
{
    using System.Collections;

    public abstract partial class VectorTests
    {
        protected readonly double[] _data = {1, 2, 3, 4, 5};

        [Test]
        [MultipleAsserts]
        public void CanCloneVector()
        {
            var vector = CreateVector(_data);
            var clone = vector.Clone();

            Assert.AreNotSame(vector, clone);
            Assert.AreEqual(vector.Count, clone.Count);
            for (var index = 0; index < _data.Length; index++)
            {
                Assert.AreEqual(vector[index], clone[index]);
            }
        }

        [Test]
        [MultipleAsserts]
        public void CanCloneVectorUsingICloneable()
        {
            var vector = CreateVector(_data);
            var clone = (Vector)((ICloneable)vector).Clone();

            Assert.AreNotSame(vector, clone);
            Assert.AreEqual(vector.Count, clone.Count);
            for (var index = 0; index < _data.Length; index++)
            {
                Assert.AreEqual(vector[index], clone[index]);
            }
        }

        [Test]
        public void CanConvertVectorToString()
        {
            var vector = CreateVector(_data);
            var str = vector.ToString();
            var sep = CultureInfo.CurrentCulture.TextInfo.ListSeparator;
            Assert.AreEqual(string.Format("1{0}2{0}3{0}4{0}5", sep), str);
        }

        [Test]
        [MultipleAsserts]
        public void CanCopyPartialVectorToAnother()
        {
            var vector = CreateVector(_data);
            var other = CreateVector(_data.Length);

            vector.CopyTo(other, 2, 2, 2);

            Assert.AreEqual(0.0, other[0]);
            Assert.AreEqual(0.0, other[1]);
            Assert.AreEqual(3.0, other[2]);
            Assert.AreEqual(4.0, other[3]);
            Assert.AreEqual(0.0, other[4]);
        }

        [Test]
        [MultipleAsserts]
        public void CanCopyPartialVectorToSelf()
        {
            var vector = CreateVector(_data);
            vector.CopyTo(vector, 0, 2, 2);

            Assert.AreEqual(1.0, vector[0]);
            Assert.AreEqual(2.0, vector[1]);
            Assert.AreEqual(1.0, vector[2]);
            Assert.AreEqual(2.0, vector[3]);
            Assert.AreEqual(5.0, vector[4]);
        }

        [Test]
        [MultipleAsserts]
        public void CanCopyVectorToAnother()
        {
            var vector = CreateVector(_data);
            var other = CreateVector(_data.Length);

            vector.CopyTo(other);

            for (var index = 0; index < _data.Length; index++)
            {
                Assert.AreEqual(vector[index], other[index]);
            }
        }

        [Test]
        [Ignore]
        public void CanCreateMatrix()
        {
        }

        [Test]
        public void CanCreateVector()
        {
            var expected = CreateVector(5);
            var actual = expected.CreateVector(5);
            Assert.AreEqual(expected.GetType(), actual.GetType(), "vectors are same type.");
        }

        [Test]
        public void CanEnumerateOverVector()
        {
            var vector = CreateVector(_data);
            Assert.AreElementsEqual(_data, vector);

        }

        [Test]
        [MultipleAsserts]
        public void CanEnumerateOverVectorUsingIEnumerable()
        {
            var enumerable = (IEnumerable)CreateVector(_data);
            var index = 0;
            foreach (var element in enumerable)
            {
                Assert.AreEqual(_data[index++], (double)element);
            }
        }

        [Test]
        [MultipleAsserts]
        public void CanEquateVectors()
        {
            var vector1 = CreateVector(_data);
            var vector2 = CreateVector(_data);
            var vector3 = CreateVector(4);
            Assert.IsTrue(vector1.Equals(vector1));
            Assert.IsTrue(vector1.Equals(vector2));
            Assert.IsFalse(vector1.Equals(vector3));
            Assert.IsFalse(vector1.Equals(null));
        }

        [Test]
        [MultipleAsserts]
        public void ThrowsArgumentExceptionIfSizeIsNotPositive()
        {
            Assert.Throws<ArgumentException>(() => CreateVector(-1));
            Assert.Throws<ArgumentException>(() => CreateVector(0));
        }

        [Test]
        public void TestingForEqualityWithNonVectorReturnsFalse()
        {
            var vector = CreateVector(_data);
            Assert.IsFalse(vector.Equals(2));
        }

        [Test]
        public void CanTestForEqualityUsingObjectEquals()
        {
            var vector1 = CreateVector(_data);
            var vector2 = CreateVector(_data);
            Assert.IsTrue(vector1.Equals((object)vector2));
        }

        [Test]
        public void VectorGetHashCode()
        {
            var vector = CreateVector(new double[] { 1, 2, 3, 4 });
            Assert.AreEqual(2145910784, vector.GetHashCode());
        }

        [Test]
        public void GetIndexedEnumerator()
        {
            Vector vector = CreateVector(_data);
            foreach (KeyValuePair<int, double> pair in vector.GetIndexedEnumerator())
            {
                Assert.AreEqual(_data[pair.Key], pair.Value);
            }
        }

        [Test]
        public void CanConvertVectorToArray()
        {
            var vector = CreateVector(_data);
            var array = vector.ToArray();
            Assert.AreElementsEqual(vector, array);
        }

        [Test]
        [MultipleAsserts]
        public void CanConvertVectorToColumnMatrix()
        {
            var vector = CreateVector(_data);
            var matrix = vector.ToColumnMatrix();
            
            Assert.AreEqual(vector.Count, matrix.RowCount);
            Assert.AreEqual(1, matrix.ColumnCount);
            
            for(var i = 0; i < vector.Count; i++)
            {
                Assert.AreEqual(vector[i], matrix[i, 0]);
            }
        }

        [Test]
        [MultipleAsserts]
        public void CanConvertVectorToRowMatrix()
        {
            var vector = CreateVector(_data);
            var matrix = vector.ToRowMatrix();

            Assert.AreEqual(vector.Count, matrix.ColumnCount);
            Assert.AreEqual(1, matrix.RowCount);

            for (var i = 0; i < vector.Count; i++)
            {
                Assert.AreEqual(vector[i], matrix[0, i]);
            }
        }

        protected abstract Vector CreateVector(int size);

        protected abstract Vector CreateVector(IList<double> data);
    }
}