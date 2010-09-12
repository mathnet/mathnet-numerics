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

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Complex
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Numerics;
    using Distributions;
    using LinearAlgebra.Generic;
    using MbUnit.Framework;

    [TestFixture]
    public abstract partial class VectorTests
    {
        protected readonly Complex[] Data = { new Complex(1, 1), new Complex(2, 1), new Complex(3, 1), new Complex(4, 1), new Complex(5, 1) };

        [Test]
        [MultipleAsserts]
        public void CanCloneVector()
        {
            var vector = CreateVector(Data);
            var clone = vector.Clone();

            Assert.AreNotSame(vector, clone);
            Assert.AreEqual(vector.Count, clone.Count);
            for (var index = 0; index < Data.Length; index++)
            {
                AssertHelpers.AreEqual(vector[index], clone[index]);
            }
        }

        [Test]
        [MultipleAsserts]
        public void CanCloneVectorUsingICloneable()
        {
            var vector = CreateVector(Data);
            var clone = (Vector<Complex>)((ICloneable)vector).Clone();

            Assert.AreNotSame(vector, clone);
            Assert.AreEqual(vector.Count, clone.Count);
            for (var index = 0; index < Data.Length; index++)
            {
                AssertHelpers.AreEqual(vector[index], clone[index]);
            }
        }

        [Test]
        [Ignore]
        public void CanConvertVectorToString()
        {
            var vector = CreateVector(Data);
            var str = vector.ToString();
            var sep = CultureInfo.CurrentCulture.TextInfo.ListSeparator;
            var decsep = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            Assert.AreEqual(string.Format("(1{0} 1){1}(2{0} 1){1}(3{0} 1){1}(4{0} 1){1}(5{0} 1)", decsep, sep), str);
        }

        [Test]
        [MultipleAsserts]
        public void CanCopyPartialVectorToAnother()
        {
            var vector = CreateVector(Data);
            var other = CreateVector(Data.Length);

            vector.CopyTo(other, 2, 2, 2);

            AssertHelpers.AreEqual(Complex.Zero, other[0]);
            AssertHelpers.AreEqual(Complex.Zero, other[1]);
            AssertHelpers.AreEqual(new Complex(3, 1), other[2]);
            AssertHelpers.AreEqual(new Complex(4, 1), other[3]);
            AssertHelpers.AreEqual(Complex.Zero, other[4]);
        }

        [Test]
        [MultipleAsserts]
        public void CanCopyPartialVectorToSelf()
        {
            var vector = CreateVector(Data);
            vector.CopyTo(vector, 0, 2, 2);

            AssertHelpers.AreEqual(new Complex(1, 1), vector[0]);
            AssertHelpers.AreEqual(new Complex(2, 1), vector[1]);
            AssertHelpers.AreEqual(new Complex(1, 1), vector[2]);
            AssertHelpers.AreEqual(new Complex(2, 1), vector[3]);
            AssertHelpers.AreEqual(new Complex(5, 1), vector[4]);
        }

        [Test]
        [MultipleAsserts]
        public void CanCopyVectorToAnother()
        {
            var vector = CreateVector(Data);
            var other = CreateVector(Data.Length);

            vector.CopyTo(other);

            for (var index = 0; index < Data.Length; index++)
            {
                AssertHelpers.AreEqual(vector[index], other[index]);
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
            var vector = CreateVector(Data);
            Assert.AreElementsEqual(Data, vector);
        }

        [Test]
        [MultipleAsserts]
        public void CanEnumerateOverVectorUsingIEnumerable()
        {
            var enumerable = (IEnumerable)CreateVector(Data);
            var index = 0;
            foreach (var element in enumerable)
            {
                AssertHelpers.AreEqual(Data[index++], (Complex)element);
            }
        }

        [Test]
        [MultipleAsserts]
        public void CanEquateVectors()
        {
            var vector1 = CreateVector(Data);
            var vector2 = CreateVector(Data);
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
            var vector = CreateVector(Data);
            Assert.IsFalse(vector.Equals(2));
        }

        [Test]
        public void CanTestForEqualityUsingObjectEquals()
        {
            var vector1 = CreateVector(Data);
            var vector2 = CreateVector(Data);
            Assert.IsTrue(vector1.Equals((object)vector2));
        }

        [Test]
        public void VectorGetHashCode()
        {
            var vector = CreateVector(new [] { new Complex(1, 1), new Complex(2, 1), new Complex(3, 1), new Complex(4, 1), new Complex(5, 1) });
            Assert.AreEqual(1104007323, vector.GetHashCode());
        }

        [Test]
        public void GetIndexedEnumerator()
        {
            var vector = CreateVector(Data);
            foreach (var pair in vector.GetIndexedEnumerator())
            {
                AssertHelpers.AreEqual(Data[pair.Key], pair.Value);
            }
        }

        [Test]
        public void CanConvertVectorToArray()
        {
            var vector = CreateVector(Data);
            var array = vector.ToArray();
            Assert.AreElementsEqual(vector, array);
        }

        [Test]
        [MultipleAsserts]
        public void CanConvertVectorToColumnMatrix()
        {
            var vector = CreateVector(Data);
            var matrix = vector.ToColumnMatrix();

            Assert.AreEqual(vector.Count, matrix.RowCount);
            Assert.AreEqual(1, matrix.ColumnCount);

            for (var i = 0; i < vector.Count; i++)
            {
                AssertHelpers.AreEqual(vector[i], matrix[i, 0]);
            }
        }

        [Test]
        [MultipleAsserts]
        public void CanConvertVectorToRowMatrix()
        {
            var vector = CreateVector(Data);
            var matrix = vector.ToRowMatrix();

            Assert.AreEqual(vector.Count, matrix.ColumnCount);
            Assert.AreEqual(1, matrix.RowCount);

            for (var i = 0; i < vector.Count; i++)
            {
                AssertHelpers.AreEqual(vector[i], matrix[0, i]);
            }
        }

        [Test]
        public void CanSetValues()
        {
            var vector = CreateVector(Data);
            vector.SetValues(Data);
            for (var i = 0; i < Data.Length; i++)
            {
                AssertHelpers.AreEqual(vector[i], Data[i]);
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
            var vector = CreateVector(Data);
            var sub = vector.SubVector(index, length);
            Assert.AreEqual(length, sub.Count);
            for (var i = 0; i < length; i++)
            {
                AssertHelpers.AreEqual(vector[i + index], sub[i]);
            }
        }

        [Test]
        public void CanFindAbsoluteMinimumIndex()
        {
            var source = CreateVector(Data);
            const int Expected = 0;
            var actual = source.AbsoluteMinimumIndex();
            Assert.AreEqual(Expected, actual);
        }

        [Test]
        public void CanFindAbsoluteMinimum()
        {
            var source = CreateVector(Data);
            var expected = new Complex(1, 1).Magnitude;
            var actual = source.AbsoluteMinimum();
            AssertHelpers.AreEqual(expected, actual);
        }

        [Test]
        public void CanFindAbsoluteMaximumIndex()
        {
            var source = CreateVector(Data);
            const int Expected = 4;
            var actual = source.AbsoluteMaximumIndex();
            Assert.AreEqual(Expected, actual);
        }

        [Test]
        public void CanFindAbsoluteMaximum()
        {
            var source = CreateVector(Data);
            var expected = new Complex(5, 1).Magnitude;
            var actual = source.AbsoluteMaximum();
            AssertHelpers.AreEqual(expected, actual);
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
            var vector = CreateVector(Data);
            var sub = vector.SubVector(index, length);
            Assert.AreEqual(length, sub.Count);
            for (var i = 0; i < length; i++)
            {
                AssertHelpers.AreEqual(vector[i + index], sub[i]);
            }
        }

        [Test]
        public void CanSum()
        {
            Complex[] testData = { new Complex(-20, -1), new Complex(-10, -1), new Complex(10, 1), new Complex(20, 1), new Complex(30,1) };
            var vector = CreateVector(testData);
            var actual = vector.Sum();
            var expected = new Complex(30, 1);
            AssertHelpers.AreEqual(expected, actual);
        }

        [Test]
        public void CanSumMagnitudes()
        {
            Complex[] testData = { new Complex(-20, -1), new Complex(-10, -1), new Complex(10, 1), new Complex(20, 1), new Complex(30, 1) };
            var vector = CreateVector(testData);
            var actual = vector.SumMagnitudes();
            var expected = testData.Sum(complex => complex.Magnitude);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        [ExpectedArgumentNullException]
        public void CanSetValuesWithNullParameterShouldThrowException()
        {
            var vector = CreateVector(Data);
            vector.SetValues(null);
        }

        [Test]
        [ExpectedArgumentException]
        public void CanSetValuesWithNonEqualDataLengthShouldThrowException()
        {
            var vector = CreateVector(Data.Length + 2);
            vector.SetValues(Data);
        }


        [Test]
        [ExpectedArgumentException]
        public void RandomWithNumberOfElementsLessThanZeroShouldThrowException()
        {
            var vector = CreateVector(4);
            vector.Random(-2, new ContinuousUniform());
        }

        [Test]
        public  void CanClearVector()
        {
            Complex[] testData = { new Complex(-20, -1), new Complex(-10, -1), new Complex(10, 1), new Complex(20, 1), new Complex(30, 1) };
            var vector = CreateVector(testData);
            vector.Clear();
            foreach (var element in vector)
            {
                AssertHelpers.AreEqual(Complex.Zero, element);
            }
        }

        protected abstract Vector<Complex> CreateVector(int size);
        protected abstract Vector<Complex> CreateVector(IList<Complex> data);
    }
}