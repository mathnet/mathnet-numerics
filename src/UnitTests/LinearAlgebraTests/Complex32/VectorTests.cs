// <copyright file="VectorTests.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2016 Math.NET
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
// </copyright>

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Complex32;
using NUnit.Framework;

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Complex32
{
    using Numerics;

    /// <summary>
    /// Abstract class with the common set of vector tests.
    /// </summary>
    public abstract partial class VectorTests
    {
        /// <summary>
        /// Test vector values.
        /// </summary>
        protected readonly Complex32[] Data = { new Complex32(1, 1), new Complex32(2, 1), new Complex32(3, 1), new Complex32(4, 1), new Complex32(5, 1) };

        /// <summary>
        /// Can clone a vector.
        /// </summary>
        [Test]
        public void CanCloneVector()
        {
            var vector = CreateVector(Data);
            var clone = vector.Clone();

            Assert.AreNotSame(vector, clone);
            Assert.AreEqual(vector.Count, clone.Count);
            CollectionAssert.AreEqual(vector, clone);
        }

#if !PORTABLE
        /// <summary>
        /// Can clone a vector using <c>IClonable</c> interface method.
        /// </summary>
        [Test]
        public void CanCloneVectorUsingICloneable()
        {
            var vector = CreateVector(Data);
            var clone = (Vector<Complex32>)((ICloneable)vector).Clone();

            Assert.AreNotSame(vector, clone);
            Assert.AreEqual(vector.Count, clone.Count);
            CollectionAssert.AreEqual(vector, clone);
        }
#endif

        /// <summary>
        /// Can copy part of a vector to another vector.
        /// </summary>
        [Test]
        public void CanCopyPartialVectorToAnother()
        {
            var vector = CreateVector(Data);
            var other = CreateVector(Data.Length);

            vector.CopySubVectorTo(other, 2, 2, 2);

            AssertHelpers.AlmostEqual(Complex32.Zero, other[0]);
            AssertHelpers.AlmostEqual(Complex32.Zero, other[1]);
            AssertHelpers.AlmostEqual(new Complex32(3, 1), other[2]);
            AssertHelpers.AlmostEqual(new Complex32(4, 1), other[3]);
            AssertHelpers.AlmostEqual(Complex32.Zero, other[4]);
        }

        /// <summary>
        /// Can copy part of a vector to the same vector.
        /// </summary>
        [Test]
        public void CanCopyPartialVectorToSelf()
        {
            var vector = CreateVector(Data);
            vector.CopySubVectorTo(vector, 0, 2, 2);

            AssertHelpers.AlmostEqual(new Complex32(1, 1), vector[0]);
            AssertHelpers.AlmostEqual(new Complex32(2, 1), vector[1]);
            AssertHelpers.AlmostEqual(new Complex32(1, 1), vector[2]);
            AssertHelpers.AlmostEqual(new Complex32(2, 1), vector[3]);
            AssertHelpers.AlmostEqual(new Complex32(5, 1), vector[4]);
        }

        /// <summary>
        /// Can copy a vector to another vector.
        /// </summary>
        [Test]
        public void CanCopyVectorToAnother()
        {
            var vector = CreateVector(Data);
            var other = CreateVector(Data.Length);

            vector.CopyTo(other);
            CollectionAssert.AreEqual(vector, other);
        }

        /// <summary>
        /// Can create a matrix using instance of a vector.
        /// </summary>
        [Test]
        public void CanCreateMatrix()
        {
            var vector = CreateVector(Data);
            var matrix = Matrix<Complex32>.Build.SameAs(vector, 10, 10);
            Assert.AreEqual(matrix.RowCount, 10);
            Assert.AreEqual(matrix.ColumnCount, 10);
        }

        /// <summary>
        /// Can create a vector using the instance of vector.
        /// </summary>
        [Test]
        public void CanCreateVector()
        {
            var expected = CreateVector(5);
            var actual = Vector<Complex32>.Build.SameAs(expected, 5);
            Assert.AreEqual(expected.Storage.IsDense, actual.Storage.IsDense, "vectors are same kind.");
        }

        /// <summary>
        /// Can enumerate over a vector.
        /// </summary>
        [Test]
        public void CanEnumerateOverVector()
        {
            var vector = CreateVector(Data);
            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(Data[i], vector[i]);
            }
        }

        /// <summary>
        /// Can enumerate over a vector using <c>IEnumerable</c> interface.
        /// </summary>
        [Test]
        public void CanEnumerateOverVectorUsingIEnumerable()
        {
            var enumerable = (IEnumerable)CreateVector(Data);
            var index = 0;
            foreach (var element in enumerable)
            {
                Assert.AreEqual(Data[index++], (Complex32)element);
            }
        }

        /// <summary>
        /// Can equate vectors.
        /// </summary>
        [Test]
        public void CanEquateVectors()
        {
            var vector1 = CreateVector(Data);
            var vector2 = CreateVector(Data);
            var vector3 = CreateVector(4);
            Assert.IsTrue(vector1.Equals(vector1));
            Assert.IsTrue(vector1.Equals(vector2));
            Assert.IsFalse(vector1.Equals(vector3));
            Assert.IsFalse(vector1.Equals(null));
            Assert.IsFalse(vector1.Equals(2));
        }

        /// <summary>
        /// <c>CreateVector</c> throws <c>ArgumentOutOfRangeException</c> if size is not positive.
        /// </summary>
        [Test]
        public void SizeIsNotPositiveThrowsArgumentOutOfRangeException()
        {
            Assert.That(() => CreateVector(-1), Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => CreateVector(0), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        /// <summary>
        /// Can equate vectors using Object.Equals.
        /// </summary>
        [Test]
        public void CanEquateVectorsUsingObjectEquals()
        {
            var vector1 = CreateVector(Data);
            var vector2 = CreateVector(Data);
            Assert.IsTrue(vector1.Equals((object)vector2));
        }

        /// <summary>
        /// Can get hash code of a vector.
        /// </summary>
        [Test]
        public void CanGetHashCode()
        {
            var vector = CreateVector(new[] { new Complex32(1, 1), new Complex32(2, 1), new Complex32(3, 1), new Complex32(4, 1), new Complex32(5, 1) });
            Assert.AreEqual(vector.GetHashCode(), vector.GetHashCode());
            Assert.AreEqual(vector.GetHashCode(), CreateVector(new[] { new Complex32(1, 1), new Complex32(2, 1), new Complex32(3, 1), new Complex32(4, 1), new Complex32(5, 1) }).GetHashCode());
            Assert.AreNotEqual(vector.GetHashCode(), CreateVector(new[] { new Complex32(1, 1) }).GetHashCode());
        }

        /// <summary>
        /// Can enumerate over a vector using indexed enumerator.
        /// </summary>
        [Test]
        public void CanEnumerateOverVectorUsingIndexedEnumerator()
        {
            var vector = CreateVector(Data);
            foreach (var pair in vector.EnumerateIndexed())
            {
                Assert.AreEqual(Data[pair.Item1], pair.Item2);
            }
        }

        /// <summary>
        /// Can enumerate over a vector using non-zero enumerator.
        /// </summary>
        [Test]
        public void CanEnumerateOverVectorUsingNonZeroEnumerator()
        {
            var vector = CreateVector(Data);
            foreach (var pair in vector.EnumerateIndexed(Zeros.AllowSkip))
            {
                Assert.AreEqual(Data[pair.Item1], pair.Item2);
                Assert.AreNotEqual(Complex32.Zero, pair.Item2);
            }
        }

        /// <summary>
        /// Can convert a vector to array.
        /// </summary>
        [Test]
        public void CanConvertVectorToArray()
        {
            var vector = CreateVector(Data);
            var array = vector.ToArray();
            CollectionAssert.AreEqual(array, vector);
        }

        /// <summary>
        /// Can convert a vector to column matrix.
        /// </summary>
        [Test]
        public void CanConvertVectorToColumnMatrix()
        {
            var vector = CreateVector(Data);
            var matrix = vector.ToColumnMatrix();

            Assert.AreEqual(vector.Count, matrix.RowCount);
            Assert.AreEqual(1, matrix.ColumnCount);

            for (var i = 0; i < vector.Count; i++)
            {
                Assert.AreEqual(vector[i], matrix[i, 0]);
            }
        }

        /// <summary>
        /// Can convert a vector to row matrix.
        /// </summary>
        [Test]
        public void CanConvertVectorToRowMatrix()
        {
            var vector = CreateVector(Data);
            var matrix = vector.ToRowMatrix();

            Assert.AreEqual(vector.Count, matrix.ColumnCount);
            Assert.AreEqual(1, matrix.RowCount);

            for (var i = 0; i < vector.Count; i++)
            {
                Assert.AreEqual(vector[i], matrix[0, i]);
            }
        }

        /// <summary>
        /// Can set values in vector.
        /// </summary>
        [Test]
        public void CanSetValues()
        {
            var vector = CreateVector(Data);
            vector.SetValues(Data);
            CollectionAssert.AreEqual(vector, Data);
        }

        /// <summary>
        /// Can get subvector from a vector.
        /// </summary>
        /// <param name="index">The first element to begin copying from.</param>
        /// <param name="length">The number of elements to copy.</param>
        [TestCase(0, 5)]
        [TestCase(2, 2)]
        [TestCase(1, 4)]
        public void CanGetSubVector(int index, int length)
        {
            var vector = CreateVector(Data);
            var sub = vector.SubVector(index, length);
            Assert.AreEqual(length, sub.Count);
            for (var i = 0; i < length; i++)
            {
                Assert.AreEqual(vector[i + index], sub[i]);
            }
        }

        /// <summary>
        /// Getting subvector using wrong parameters throw an exception.
        /// </summary>
        /// <param name="index">The first element to begin copying from.</param>
        /// <param name="length">The number of elements to copy.</param>
        [TestCase(6, 10)]
        [TestCase(1, -10)]
        public void CanGetSubVectorWithWrongValuesShouldThrowException(int index, int length)
        {
            var vector = CreateVector(Data);
            Assert.That(() => vector.SubVector(index, length), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        /// <summary>
        /// Can find absolute minimum value index.
        /// </summary>
        [Test]
        public void CanFindAbsoluteMinimumIndex()
        {
            var source = CreateVector(Data);
            const int Expected = 0;
            var actual = source.AbsoluteMinimumIndex();
            Assert.AreEqual(Expected, actual);
        }

        /// <summary>
        /// Can find absolute minimum value of a vector.
        /// </summary>
        [Test]
        public void CanFindAbsoluteMinimum()
        {
            var source = CreateVector(Data);
            var expected = new Complex32(1, 1).Magnitude;
            var actual = source.AbsoluteMinimum();
            Assert.AreEqual(expected, actual.Real);
        }

        /// <summary>
        /// Can find absolute maximum value index.
        /// </summary>
        [Test]
        public void CanFindAbsoluteMaximumIndex()
        {
            var source = CreateVector(Data);
            const int Expected = 4;
            var actual = source.AbsoluteMaximumIndex();
            Assert.AreEqual(Expected, actual);
        }

        /// <summary>
        /// Can find absolute maximum value of a vector.
        /// </summary>
        [Test]
        public void CanFindAbsoluteMaximum()
        {
            var source = CreateVector(Data);
            var expected = new Complex32(5, 1).Magnitude;
            var actual = source.AbsoluteMaximum();
            Assert.AreEqual(expected, actual.Real);
        }

        /// <summary>
        /// Find maximum value index throws <c>NotSupportedException</c>.
        /// </summary>
        [Test]
        public void FindMaximumIndexThrowsNotSupportedException()
        {
            var vector = CreateVector(Data);
            Assert.That(() => { var actual = vector.MaximumIndex(); }, Throws.TypeOf<NotSupportedException>());
        }

        /// <summary>
        /// Find maximum value throws <c>NotSupportedException</c>.
        /// </summary>
        [Test]
        public void FindMaximumThrowsNotSupportedException()
        {
            var vector = CreateVector(Data);
            Assert.That(() => { var actual = vector.Maximum(); }, Throws.TypeOf<NotSupportedException>());
        }

        /// <summary>
        /// Find minimum value index throws <c>NotSupportedException</c>.
        /// </summary>
        [Test]
        public void FindMinimumIndexThrowsNotSupportedException()
        {
            var vector = CreateVector(Data);
            Assert.That(() => { var actual = vector.MinimumIndex(); }, Throws.TypeOf<NotSupportedException>());
        }

        /// <summary>
        /// Find minimum value throws <c>NotSupportedException</c>.
        /// </summary>
        [Test]
        public void FindMinimumThrowsNotSupportedException()
        {
            var vector = CreateVector(Data);
            Assert.That(() => { var actual = vector.Minimum(); }, Throws.TypeOf<NotSupportedException>());
        }

        /// <summary>
        /// Can compute the sum of a vector elements.
        /// </summary>
        [Test]
        public void CanSum()
        {
            Complex32[] testData = { new Complex32(-20, -1), new Complex32(-10, -1), new Complex32(10, 1), new Complex32(20, 1), new Complex32(30, 1) };
            var vector = CreateVector(testData);
            var actual = vector.Sum();
            var expected = new Complex32(30, 1);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Can compute the sum of the absolute value a vector elements.
        /// </summary>
        [Test]
        public void CanSumMagnitudes()
        {
            Complex32[] testData = { new Complex32(-20, -1), new Complex32(-10, -1), new Complex32(10, 1), new Complex32(20, 1), new Complex32(30, 1) };
            var vector = CreateVector(testData);
            var actual = vector.SumMagnitudes();
            var expected = testData.Sum(complex => complex.Magnitude);
            Assert.AreEqual(expected, (float) actual);
        }

        /// <summary>
        /// Set values with <c>null</c> parameter throw exception.
        /// </summary>
        [Test]
        public void SetValuesWithNullParameterThrowsArgumentException()
        {
            var vector = CreateVector(Data);
            Assert.That(() => vector.SetValues(null), Throws.TypeOf<ArgumentNullException>());
        }

        /// <summary>
        /// Set values with non-equal data length throw exception.
        /// </summary>
        [Test]
        public void SetValuesWithNonEqualDataLengthThrowsArgumentException()
        {
            var vector = CreateVector(Data.Length + 2);
            Assert.That(() => vector.SetValues(Data), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        /// <summary>
        /// Generate a vector with number of elements less than zero throw an exception.
        /// </summary>
        [Test]
        public void RandomWithNumberOfElementsLessThanZeroThrowsArgumentException()
        {
            Assert.That(() => DenseVector.CreateRandom(-2, new ContinuousUniform()), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        /// <summary>
        /// Can clear a vector.
        /// </summary>
        [Test]
        public void CanClearVector()
        {
            Complex32[] testData = { new Complex32(-20, -1), new Complex32(-10, -1), new Complex32(10, 1), new Complex32(20, 1), new Complex32(30, 1) };
            var vector = CreateVector(testData);
            vector.Clear();
            foreach (var element in vector)
            {
                Assert.AreEqual(Complex32.Zero, element);
            }
        }

        /// <summary>
        /// Creates a new instance of the Vector class.
        /// </summary>
        /// <param name="size">The size of the <strong>Vector</strong> to construct.</param>
        /// <returns>The new <c>Vector</c>.</returns>
        protected abstract Vector<Complex32> CreateVector(int size);

        /// <summary>
        /// Creates a new instance of the Vector class.
        /// </summary>
        /// <param name="data">The array to create this vector from.</param>
        /// <returns>The new <c>Vector</c>.</returns>
        protected abstract Vector<Complex32> CreateVector(IList<Complex32> data);
    }
}
