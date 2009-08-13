// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VectorTests.cs" company="">
// </copyright>
// <summary>
//   vector tests.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Double
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    using MathNet.Numerics.LinearAlgebra.Double;

    using MbUnit.Framework;

    /// <summary>
    /// The vector tests.
    /// </summary>
    public abstract class VectorTests
    {
        /// <summary>
        /// The test data.
        /// </summary>
        private readonly double[] _data = { 1, 2, 3, 4, 5 };

        /// <summary>
        /// can clone vector.
        /// </summary>
        [Test]
        public void CanCloneVector()
        {
            var vector = CreateVector(_data);
            var clone = vector.Clone();

            Assert.AreEqual(vector.Count, clone.Count);
            for (var index = 0; index < _data.Length; index++)
            {
                Assert.AreEqual(vector[index], clone[index]);
            }
        }

        /// <summary>
        /// can convert vector to string.
        /// </summary>
        [Test]
        public void CanConvertVectorToString()
        {
            var vector = CreateVector(_data);
            var str = vector.ToString();
            var sep = CultureInfo.CurrentCulture.TextInfo.ListSeparator;
            Assert.AreEqual(string.Format("1{0}2{0}3{0}4{0}5", sep), str);
        }

        /// <summary>
        /// can copy partial vector to another.
        /// </summary>
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

        /// <summary>
        /// can copy vector to another.
        /// </summary>
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

        /// <summary>
        /// can create matrix.
        /// </summary>
        [Test]
        [Ignore]
        public void CanCreateMatrix()
        {
        }

        /// <summary>
        /// can create vector.
        /// </summary>
        [Test]
        public void CanCreateVector()
        {
            var expected = CreateVector(5);
            var actual = expected.CreateVector(5);
            Assert.AreEqual(expected.GetType(), actual.GetType(), "vectors are same type.");
        }

        /// <summary>
        /// can enumerate over vector.
        /// </summary>
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

        /// <summary>
        /// can equate vectors.
        /// </summary>
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

        /// <summary>
        /// can get indexed enumerator.
        /// </summary>
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

        /// <summary>
        /// can get indexed enumerator over range.
        /// </summary>
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

        /// <summary>
        /// throws argument exception if size is not positive.
        /// </summary>
        [Test]
        public void ThrowsArgumentExceptionIfSizeIsNotPositive()
        {
            Assert.Throws<ArgumentException>(() => CreateVector(-1));
            Assert.Throws<ArgumentException>(() => CreateVector(0));
        }

        protected abstract Vector CreateVector(int size);

        protected abstract Vector CreateVector(IList<double> data);
    }
}