using System;
using System.Collections.Generic;
using System.Globalization;
using MathNet.Numerics.LinearAlgebra.Double;
using MbUnit.Framework;

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Double
{
    public abstract partial class VectorTests
    {
        private readonly double[] _data = {1, 2, 3, 4, 5};

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
        [MultipleAsserts]
        public void CanEnumerateOverVector()
        {
            var vector = CreateVector(_data);
            var index = 0;

            foreach (var element in vector)
            {
                Assert.AreEqual(index + 1, element);
                index++;
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
        public void CanGetIndexedEnumerator()
        {
            var vector = CreateVector(_data);
            var index = 0;

            foreach (var pair in vector.GetIndexedEnumerator())
            {
                Assert.AreEqual(index, pair.Key);
                Assert.AreEqual(++index, pair.Value);
            }
        }

        [Test]
        [MultipleAsserts]
        public void CanGetIndexedEnumeratorOverRange()
        {
            var vector = CreateVector(_data);
            var index = 2;

            foreach (var pair in vector.GetIndexedEnumerator(2, 2))
            {
                Assert.AreEqual(index, pair.Key);
                Assert.AreEqual(++index, pair.Value);
            }
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
        
        protected abstract Vector CreateVector(int size);

        protected abstract Vector CreateVector(IList<double> data);
    }
}