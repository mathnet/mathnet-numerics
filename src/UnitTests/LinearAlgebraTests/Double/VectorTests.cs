// <copyright file="VectorTests.cs" company="Math.NET">
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

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Double
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using Distributions;
    using LinearAlgebra.Double;
    using MbUnit.Framework;

    [TestFixture]
    public abstract partial class VectorTests
    {
        protected readonly double[] _data = { 1, 2, 3, 4, 5 };

        [Test]
        [MultipleAsserts]
        public void CanCloneVector()
        {
            var vector = this.CreateVector(this._data);
            var clone = vector.Clone();

            Assert.AreNotSame(vector, clone);
            Assert.AreEqual(vector.Count, clone.Count);
            for (var index = 0; index < this._data.Length; index++)
            {
                Assert.AreEqual(vector[index], clone[index]);
            }
        }

        [Test]
        [MultipleAsserts]
        public void CanCloneVectorUsingICloneable()
        {
            var vector = this.CreateVector(this._data);
            var clone = (Vector)((ICloneable)vector).Clone();

            Assert.AreNotSame(vector, clone);
            Assert.AreEqual(vector.Count, clone.Count);
            for (var index = 0; index < this._data.Length; index++)
            {
                Assert.AreEqual(vector[index], clone[index]);
            }
        }

        [Test]
        public void CanConvertVectorToString()
        {
            var vector = this.CreateVector(this._data);
            var str = vector.ToString();
            var sep = CultureInfo.CurrentCulture.TextInfo.ListSeparator;
            Assert.AreEqual(string.Format("1{0}2{0}3{0}4{0}5", sep), str);
        }

        [Test]
        [MultipleAsserts]
        public void CanCopyPartialVectorToAnother()
        {
            var vector = this.CreateVector(this._data);
            var other = this.CreateVector(this._data.Length);

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
            var vector = this.CreateVector(this._data);
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
            var vector = this.CreateVector(this._data);
            var other = this.CreateVector(this._data.Length);

            vector.CopyTo(other);

            for (var index = 0; index < this._data.Length; index++)
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
            var expected = this.CreateVector(5);
            var actual = expected.CreateVector(5);
            Assert.AreEqual(expected.GetType(), actual.GetType(), "vectors are same type.");
        }

        [Test]
        public void CanEnumerateOverVector()
        {
            var vector = this.CreateVector(this._data);
            Assert.AreElementsEqual(this._data, vector);
        }

        [Test]
        [MultipleAsserts]
        public void CanEnumerateOverVectorUsingIEnumerable()
        {
            var enumerable = (IEnumerable)this.CreateVector(this._data);
            var index = 0;
            foreach (var element in enumerable)
            {
                Assert.AreEqual(this._data[index++], (double)element);
            }
        }

        [Test]
        [MultipleAsserts]
        public void CanEquateVectors()
        {
            var vector1 = this.CreateVector(this._data);
            var vector2 = this.CreateVector(this._data);
            var vector3 = this.CreateVector(4);
            Assert.IsTrue(vector1.Equals(vector1));
            Assert.IsTrue(vector1.Equals(vector2));
            Assert.IsFalse(vector1.Equals(vector3));
            Assert.IsFalse(vector1.Equals(null));
        }

        [Test]
        [MultipleAsserts]
        public void ThrowsArgumentExceptionIfSizeIsNotPositive()
        {
            Assert.Throws<ArgumentException>(() => this.CreateVector(-1));
            Assert.Throws<ArgumentException>(() => this.CreateVector(0));
        }

        [Test]
        public void TestingForEqualityWithNonVectorReturnsFalse()
        {
            var vector = this.CreateVector(this._data);
            Assert.IsFalse(vector.Equals(2));
        }

        [Test]
        public void CanTestForEqualityUsingObjectEquals()
        {
            var vector1 = this.CreateVector(this._data);
            var vector2 = this.CreateVector(this._data);
            Assert.IsTrue(vector1.Equals((object)vector2));
        }

        [Test]
        public void VectorGetHashCode()
        {
            var vector = this.CreateVector(new double[] { 1, 2, 3, 4 });
            Assert.AreEqual(2145910784, vector.GetHashCode());
        }

        [Test]
        public void GetIndexedEnumerator()
        {
            var vector = this.CreateVector(this._data);
            foreach (var pair in vector.GetIndexedEnumerator())
            {
                Assert.AreEqual(this._data[pair.Key], pair.Value);
            }
        }

        [Test]
        public void CanConvertVectorToArray()
        {
            var vector = this.CreateVector(this._data);
            var array = vector.ToArray();
            Assert.AreElementsEqual(vector, array);
        }

        [Test, Ignore]
        [MultipleAsserts]
        public void CanConvertVectorToColumnMatrix()
        {
            var vector = this.CreateVector(this._data);
            var matrix = vector.ToColumnMatrix();

            Assert.AreEqual(vector.Count, matrix.RowCount);
            Assert.AreEqual(1, matrix.ColumnCount);

            for (var i = 0; i < vector.Count; i++)
            {
                Assert.AreEqual(vector[i], matrix[i, 0]);
            }
        }

        [Test, Ignore]
        [MultipleAsserts]
        public void CanConvertVectorToRowMatrix()
        {
            var vector = this.CreateVector(this._data);
            var matrix = vector.ToRowMatrix();

            Assert.AreEqual(vector.Count, matrix.ColumnCount);
            Assert.AreEqual(1, matrix.RowCount);

            for (var i = 0; i < vector.Count; i++)
            {
                Assert.AreEqual(vector[i], matrix[0, i]);
            }
        }

        [Test]
        public void CanSetValues()
        {
            var vector = this.CreateVector(this._data);
            vector.SetValues(this._data);
            for (var i = 0; i < this._data.Length; i++)
            {
                Assert.AreEqual(vector[i], this._data[i]);
            }
        }

        [Test]
        [Row(0, 5)]
        [Row(2, 2)]
        [Row(1, 4)]
        [Row(6, 10, ExpectedException = typeof(ArgumentOutOfRangeException))]
        [Row(1, 10, ExpectedException = typeof(ArgumentOutOfRangeException))]
        [Row(1, -10, ExpectedException = typeof(ArgumentOutOfRangeException))]
        public void CanCalculateSubVector(int index, int length)
        {
            var vector = this.CreateVector(this._data);
            var sub = vector.SubVector(index, length);
            Assert.AreEqual(length, sub.Count);
            for (var i = 0; i < length; i++)
            {
                Assert.AreEqual(vector[i + index], sub[i]);
            }
        }

        [Test]
        public void CanFindAbsoluteMinimumIndex()
        {
            var source = this.CreateVector(this._data);
            var expected = 0;
            var actual = source.AbsoluteMinimumIndex();
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void CanFindAbsoluteMinimum()
        {
            var source = this.CreateVector(this._data);
            double expected = 1;
            var actual = source.AbsoluteMinimum();
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void CanFindAbsoluteMaximumIndex()
        {
            var source = this.CreateVector(this._data);
            var expected = 4;
            var actual = source.AbsoluteMaximumIndex();
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void CanFindAbsoluteMaximum()
        {
            var source = this.CreateVector(this._data);
            double expected = 5;
            var actual = source.AbsoluteMaximum();
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void CanFindMaximumIndex()
        {
            var vector = this.CreateVector(this._data);

            var expected = 4;
            var actual = vector.MaximumIndex();

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void CanFindMaximum()
        {
            var vector = this.CreateVector(this._data);

            double expected = 5;
            var actual = vector.Maximum();

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void CanFindMinimumIndex()
        {
            var vector = this.CreateVector(this._data);

            var expected = 0;
            var actual = vector.MinimumIndex();

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void CanFindMinimum()
        {
            var vector = this.CreateVector(this._data);

            double expected = 1;
            var actual = vector.Minimum();

            Assert.AreEqual(expected, actual);
        }

        [Test]
        [Row(0, 5)]
        [Row(2, 2)]
        [Row(1, 4)]
        [Row(6, 10, ExpectedException = typeof(ArgumentOutOfRangeException))]
        [Row(1, 10, ExpectedException = typeof(ArgumentOutOfRangeException))]
        [Row(1, -10, ExpectedException = typeof(ArgumentOutOfRangeException))]
        public void CanGetSubVector(int index, int length)
        {
            var vector = this.CreateVector(this._data);
            var sub = vector.SubVector(index, length);
            Assert.AreEqual(length, sub.Count);
            for (var i = 0; i < length; i++)
            {
                Assert.AreEqual(vector[i + index], sub[i]);
            }
        }

        [Test]
        public void CanSum()
        {
            double[] testData = { -20, -10, 10, 20, 30, };
            var vector = CreateVector(testData);
            var actual = vector.Sum();
            double expected = 30;
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void CanSumMagnitudes()
        {
            double[] testData = { -20, -10, 10, 20, 30, };
            var vector = CreateVector(testData);
            var actual = vector.SumMagnitudes();
            double expected = 90;
            Assert.AreEqual(expected, actual);
        }

        [Test]
        [ExpectedArgumentNullException]
        public void CanSetValuesWithNullParameterShouldThrowException()
        {
            var vector = this.CreateVector(this._data);
            vector.SetValues(null);
        }

        [Test]
        [ExpectedArgumentException]
        public void CanSetValuesWithNonEqualDataLengthShouldThrowException()
        {
            var vector = this.CreateVector(this._data.Length + 2);
            vector.SetValues(this._data);
        }


        [Test]
        [ExpectedArgumentException]
        public void RandomWithNumberOfElementsLessThanZeroShouldThrowException()
        {
            var vector = this.CreateVector(4);
            vector = vector.Random(-2, new ContinuousUniform());
        }

        protected abstract Vector CreateVector(int size);
        protected abstract Vector CreateVector(IList<double> data);
    }
}