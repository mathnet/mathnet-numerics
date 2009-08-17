using System;
using System.Collections.Generic;
using System.Globalization;
using MathNet.Numerics.LinearAlgebra.Double;
using MbUnit.Framework;

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Double
{
    public abstract class VectorTests
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
        public void CanCallPlus()
        {
            var vector = CreateVector(_data);
            var other = vector.Plus();
            Assert.AreSame(vector, other, "Should be the same vector");
        }

        [Test]
        public void OperatorPlusThrowsArgumentNullExceptionWhenCallOnNullVector()
        {
            Vector vector = null;
            Vector other = null;
            Assert.Throws<ArgumentNullException>(() => other = +vector);
        }

        [Test]
        public void CanCallUnaryPlusOperator()
        {
            var vector = CreateVector(_data);
            var other = +vector;
            Assert.AreSame(vector, other, "Should be the same vector");
        }

        [Test]
        [MultipleAsserts]
        public void CanAddScalarToVector()
        {
            var vector = CreateVector(_data);
            vector.Add(2.0);

            for( var i = 0; i < _data.Length; i++)
            {
                Assert.AreEqual(_data[i]+2.0, vector[i]);
            }

            vector.Add(0.0);
            for (var i = 0; i < _data.Length; i++)
            {
                Assert.AreEqual(_data[i] + 2.0, vector[i]);
            }
        }

        [Test]
        [MultipleAsserts]
        public void CanAddScalarToVectorUsingResultVector()
        {
            var vector = CreateVector(_data);
            var result = CreateVector(_data.Length);
            vector.Add(2.0, result);

            for (var i = 0; i < _data.Length; i++)
            {
                Assert.AreEqual(_data[i], vector[i], "Making sure the original vector wasn't modified.");
                Assert.AreEqual(_data[i] + 2.0, result[i]);
            }

            vector.Add(0.0, result);
            for (var i = 0; i < _data.Length; i++)
            {
                Assert.AreEqual(_data[i], result[i]);
            }
        }

        [Test]
        public void ThrowsArgumentNullExceptionWhenAddingScalarWithNullResultVector()
        {
            var vector = CreateVector(_data.Length);
            Assert.Throws<ArgumentNullException>(() => vector.Add(0.0, null));
        }

        [Test]
        public void ThrowsArgumentExceptionWhenAddingScalarWithWrongSizeResultVector()
        {
            var vector = CreateVector(_data.Length);
            var result = CreateVector(_data.Length + 1);
            Assert.Throws<ArgumentException>(() => vector.Add(0.0, result));
        }

        [Test]
        public void ThrowsArgumentNullExceptionWhenAddingTwoVectorsAndOneIsNull()
        {
            var vector = CreateVector(_data);
            Assert.Throws<ArgumentNullException>(() => vector.Add(null));
        }

        [Test]
        public void ThrowsArgumentExceptionWhenAddingTwoVectorsOfDifferingSize()
        {
            var vector = CreateVector(_data.Length);
            var other = CreateVector(_data.Length +1);
            Assert.Throws<ArgumentException>(() => vector.Add(other));
        }

        [Test]
        public void ThrowsArgumentNullExceptionWhenAddingTwoVectorsAndResultIsNull()
        {
            var vector = CreateVector(_data.Length);
            var other = CreateVector(_data.Length+1);
            Assert.Throws<ArgumentNullException>(() => vector.Add(other,null));
        }

        [Test]
        public void ThrowsArgumentExceptionWhenAddingTwoVectorsAndResultIsDifferentSize()
        {
            var vector = CreateVector(_data.Length);
            var other = CreateVector(_data.Length);
            var result = CreateVector(_data.Length + 1);
            Assert.Throws<ArgumentException>(() => vector.Add(other, result));
        }

        [Test]
        public void AdditionOperatorThrowsArgumentNullExpectionIfAVectorIsNull()
        {
            Vector a = null;
            var b = CreateVector(_data.Length);
            Assert.Throws<ArgumentNullException>(()=> a += b);

            a = b;
            b = null;
            Assert.Throws<ArgumentNullException>(() => a += b);
        }

        [Test]
        public void AdditionOperatorThrowsArgumentExpectionIfVectorsAreDifferentSize()
        {
            var a = CreateVector(_data.Length);
            var b = CreateVector(_data.Length + 1);
            Assert.Throws<ArgumentException>(() => a += b);
        }

        [Test]
        public void CanAddTwoVectors()
        {
            var vector = CreateVector(_data);
            var other = CreateVector(_data);
            vector.Add(other);

            for (var i = 0; i < _data.Length; i++)
            {
                Assert.AreEqual(_data[i] * 2.0, vector[i]);
            }
        }

        [Test]
        [MultipleAsserts]
        public void CanAddTwoVectorsUsingResultVector()
        {
            var vector = CreateVector(_data);
            var other = CreateVector(_data);
            var result = CreateVector(_data.Length);
            vector.Add(other, result);

            for (var i = 0; i < _data.Length; i++)
            {
                Assert.AreEqual(_data[i], vector[i], "Making sure the original vector wasn't modified.");
                Assert.AreEqual(_data[i], other[i], "Making sure the original vector wasn't modified.");
                Assert.AreEqual(_data[i] * 2.0, result[i]);
            }
        }

        [Test]
        [MultipleAsserts]
        public void CanAddTwoVectorsUsingOperator()
        {
            var vector = CreateVector(_data);
            var other = CreateVector(_data);
            var result = vector + other;

            for (var i = 0; i < _data.Length; i++)
            {
                Assert.AreEqual(_data[i], vector[i], "Making sure the original vector wasn't modified.");
                Assert.AreEqual(_data[i], other[i], "Making sure the original vector wasn't modified.");
                Assert.AreEqual(_data[i] * 2.0, result[i]);
            }
        }

        [Test]
        public void CanAddVectorToItself()
        {
            var vector = CreateVector(_data);
            vector.Add(vector);

            for (var i = 0; i < _data.Length; i++)
            {
                Assert.AreEqual(_data[i] * 2.0, vector[i]);
            }
        }

        [Test]
        [MultipleAsserts]
        public void CanAddVectorToItselfUsingResultVector()
        {
            var vector = CreateVector(_data);
            var result = CreateVector(_data.Length);
            vector.Add(vector, result);

            for (var i = 0; i < _data.Length; i++)
            {
                Assert.AreEqual(_data[i], vector[i], "Making sure the original vector wasn't modified.");
                Assert.AreEqual(_data[i] * 2.0, result[i]);
            }
        }

        [Test]
        [MultipleAsserts]
        public void CanAddTwoVectorsUsingItselfAsResultVector()
        {
            var vector = CreateVector(_data);
            var other = CreateVector(_data);
            vector.Add(other, vector);

            for (var i = 0; i < _data.Length; i++)
            {
                Assert.AreEqual(_data[i], other[i], "Making sure the original vector wasn't modified.");
                Assert.AreEqual(_data[i] * 2.0, vector[i]);
            }
        }

        [Test]
        public void CanCallNegate()
        {
            var vector = CreateVector(_data);
            var other = vector.Negate();
            for (var i = 0; i < _data.Length; i++)
            {
                Assert.AreEqual(-_data[i], other[i]);
            }
        }

        [Test]
        public void OperatorNegateThrowsArgumentNullExceptionWhenCallOnNullVector()
        {
            Vector vector = null;
            Vector other = null;
            Assert.Throws<ArgumentNullException>(() => other = -vector);
        }

        [Test]
        public void CanCallUnaryNegationOperator()
        {
            var vector = CreateVector(_data);
            var other = -vector;
            for (var i = 0; i < _data.Length; i++)
            {
                Assert.AreEqual(-_data[i], other[i]);
            }
        }

        [Test]
        [MultipleAsserts]
        public void CanSubtractScalarFromVector()
        {
            var vector = CreateVector(_data);
            vector.Subtract(2.0);

            for (var i = 0; i < _data.Length; i++)
            {
                Assert.AreEqual(_data[i] - 2.0, vector[i]);
            }

            vector.Subtract(0.0);
            for (var i = 0; i < _data.Length; i++)
            {
                Assert.AreEqual(_data[i] - 2.0, vector[i]);
            }
        }

        [Test]
        [MultipleAsserts]
        public void CanSubtractScalarFromVectorUsingResultVector()
        {
            var vector = CreateVector(_data);
            var result = CreateVector(_data.Length);
            vector.Subtract(2.0, result);

            for (var i = 0; i < _data.Length; i++)
            {
                Assert.AreEqual(_data[i], vector[i], "Making sure the original vector wasn't modified.");
                Assert.AreEqual(_data[i] - 2.0, result[i]);
            }

            vector.Subtract(0.0, result);
            for (var i = 0; i < _data.Length; i++)
            {
                Assert.AreEqual(_data[i], result[i]);
            }
        }

        [Test]
        public void ThrowsArgumentNullExceptionWhenSubtractingScalarWithNullResultVector()
        {
            var vector = CreateVector(_data.Length);
            Assert.Throws<ArgumentNullException>(() => vector.Subtract(0.0, null));
        }

        [Test]
        public void ThrowsArgumentExceptionWhenSubtractingScalarWithWrongSizeResultVector()
        {
            var vector = CreateVector(_data.Length);
            var result = CreateVector(_data.Length + 1);
            Assert.Throws<ArgumentException>(() => vector.Subtract(0.0, result));
        }

        [Test]
        public void ThrowsArgumentNullExceptionWhenSubtractingTwoVectorsAndOneIsNull()
        {
            var vector = CreateVector(_data);
            Assert.Throws<ArgumentNullException>(() => vector.Subtract(null));
        }

        [Test]
        public void ThrowsArgumentExceptionWhenSubtractingTwoVectorsOfDifferingSize()
        {
            var vector = CreateVector(_data.Length);
            var other = CreateVector(_data.Length + 1);
            Assert.Throws<ArgumentException>(() => vector.Subtract(other));
        }

        [Test]
        public void ThrowsArgumentNullExceptionWhenSubtractingTwoVectorsAndResultIsNull()
        {
            var vector = CreateVector(_data.Length);
            var other = CreateVector(_data.Length + 1);
            Assert.Throws<ArgumentNullException>(() => vector.Subtract(other, null));
        }

        [Test]
        public void ThrowsArgumentExceptionWhenSubtractingTwoVectorsAndResultIsDifferentSize()
        {
            var vector = CreateVector(_data.Length);
            var other = CreateVector(_data.Length);
            var result = CreateVector(_data.Length + 1);
            Assert.Throws<ArgumentException>(() => vector.Subtract(other, result));
        }

        [Test]
        public void SubtractionOperatorThrowsArgumentNullExpectionIfAVectorIsNull()
        {
            Vector a = null;
            var b = CreateVector(_data.Length);
            Assert.Throws<ArgumentNullException>(() => a -= b);

            a = b;
            b = null;
            Assert.Throws<ArgumentNullException>(() => a -= b);
        }

        [Test]
        public void SubtractionOperatorThrowsArgumentExpectionIfVectorsAreDifferentSize()
        {
            var a = CreateVector(_data.Length);
            var b = CreateVector(_data.Length + 1);
            Assert.Throws<ArgumentException>(() => a -= b);
        }

        [Test]
        public void CanSubtractTwoVectors()
        {
            var vector = CreateVector(_data);
            var other = CreateVector(_data);
            vector.Subtract(other);

            for (var i = 0; i < _data.Length; i++)
            {
                Assert.AreEqual(0.0, vector[i]);
            }
        }

        [Test]
        [MultipleAsserts]
        public void CanSubtractTwoVectorsUsingResultVector()
        {
            var vector = CreateVector(_data);
            var other = CreateVector(_data);
            var result = CreateVector(_data.Length);
            vector.Subtract(other, result);

            for (var i = 0; i < _data.Length; i++)
            {
                Assert.AreEqual(_data[i], vector[i], "Making sure the original vector wasn't modified.");
                Assert.AreEqual(_data[i], other[i], "Making sure the original vector wasn't modified.");
                Assert.AreEqual(0.0, result[i]);
            }
        }

        [Test]
        [MultipleAsserts]
        public void CanSubtractTwoVectorsUsingOperator()
        {
            var vector = CreateVector(_data);
            var other = CreateVector(_data);
            var result = vector - other;

            for (var i = 0; i < _data.Length; i++)
            {
                Assert.AreEqual(_data[i], vector[i], "Making sure the original vector wasn't modified.");
                Assert.AreEqual(_data[i], other[i], "Making sure the original vector wasn't modified.");
                Assert.AreEqual(0.0, result[i]);
            }
        }

        [Test]
        public void CanSubtractVectorFromItself()
        {
            var vector = CreateVector(_data);
            vector.Subtract(vector);

            for (var i = 0; i < _data.Length; i++)
            {
                Assert.AreEqual(0.0, vector[i]);
            }
        }

        [Test]
        [MultipleAsserts]
        public void CanSubtractVectorFromItselfUsingResultVector()
        {
            var vector = CreateVector(_data);
            var result = CreateVector(_data.Length);
            vector.Subtract(vector, result);

            for (var i = 0; i < _data.Length; i++)
            {
                Assert.AreEqual(_data[i], vector[i], "Making sure the original vector wasn't modified.");
                Assert.AreEqual(0.0, result[i]);
            }
        }

        [Test]
        [MultipleAsserts]
        public void CanSubtractTwoVectorsUsingItselfAsResultVector()
        {
            var vector = CreateVector(_data);
            var other = CreateVector(_data);
            vector.Subtract(other, vector);

            for (var i = 0; i < _data.Length; i++)
            {
                Assert.AreEqual(_data[i], other[i], "Making sure the original vector wasn't modified.");
                Assert.AreEqual(0.0, vector[i]);
            }
        }

        [Test]
        [MultipleAsserts]
        public void CanDivideVectorByScalar()
        {
            var vector = CreateVector(_data);
            vector.Divide(2.0);

            for (var i = 0; i < _data.Length; i++)
            {
                Assert.AreEqual(_data[i] / 2.0, vector[i]);
            }

            vector.Divide(1.0);
            for (var i = 0; i < _data.Length; i++)
            {
                Assert.AreEqual(_data[i] / 2.0, vector[i]);
            }
        }

        [Test]
        [MultipleAsserts]
        public void CanDivideVectorByScalarUsingResultVector()
        {
            var vector = CreateVector(_data);
            var result = CreateVector(_data.Length);
            vector.Divide(2.0, result);

            for (var i = 0; i < _data.Length; i++)
            {
                Assert.AreEqual(_data[i], vector[i], "Making sure the original vector wasn't modified.");
                Assert.AreEqual(_data[i] / 2.0, result[i]);
            }

            vector.Divide(1.0, result);
            for (var i = 0; i < _data.Length; i++)
            {
                Assert.AreEqual(_data[i], result[i]);
            }
        }

        [Test]
        [MultipleAsserts]
        public void CanMultiplyVectorByScalar()
        {
            var vector = CreateVector(_data);
            vector.Multiply(2.0);

            for (var i = 0; i < _data.Length; i++)
            {
                Assert.AreEqual(_data[i] * 2.0, vector[i]);
            }

            vector.Multiply(1.0);
            for (var i = 0; i < _data.Length; i++)
            {
                Assert.AreEqual(_data[i] * 2.0, vector[i]);
            }
        }

        [Test]
        [MultipleAsserts]
        public void CanMultiplyVectorByScalarUsingResultVector()
        {
            var vector = CreateVector(_data);
            var result = CreateVector(_data.Length);
            vector.Multiply(2.0, result);

            for (var i = 0; i < _data.Length; i++)
            {
                Assert.AreEqual(_data[i], vector[i], "Making sure the original vector wasn't modified.");
                Assert.AreEqual(_data[i] * 2.0, result[i]);
            }

            vector.Multiply(1.0, result);
            for (var i = 0; i < _data.Length; i++)
            {
                Assert.AreEqual(_data[i], result[i]);
            }
        }

        [Test]
        public void ThrowsArgumentNullExceptionWhenMultiplyingScalarWithNullResultVector()
        {
            var vector = CreateVector(_data.Length);
            Assert.Throws<ArgumentNullException>(() => vector.Multiply(1.0, null));
        }

        [Test]
        public void ThrowsArgumentNullExceptionWhenDividingScalarWithNullResultVector()
        {
            var vector = CreateVector(_data.Length);
            Assert.Throws<ArgumentNullException>(() => vector.Divide(1.0, null));
        }

        [Test]
        public void ThrowsArgumentExceptionWhenMultiplyingScalarWithWrongSizeResultVector()
        {
            var vector = CreateVector(_data.Length);
            var result = CreateVector(_data.Length + 1);
            Assert.Throws<ArgumentException>(() => vector.Multiply(0.0, result));
        }

        [Test]
        public void ThrowsArgumentExceptionWhenDividingScalarWithWrongSizeResultVector()
        {
            var vector = CreateVector(_data.Length);
            var result = CreateVector(_data.Length + 1);
            Assert.Throws<ArgumentException>(() => vector.Divide(0.0, result));
        }

        [Test]
        [MultipleAsserts]
        public void CanMultiplyVectorByScalarUsingOperators()
        {
            var vector = CreateVector(_data);
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

            vector = CreateVector(_data);
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
        public void CanDivideVectorByScalarUsingOperators()
        {
            var vector = CreateVector(_data);
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

        [Test]
        [MultipleAsserts]
        public void OperatorMultiplyThrowsArgumentNullExceptionWhenVectorIsNull()
        {
            Vector vector = null;
            Vector result = null;
            Assert.Throws<ArgumentNullException>(() => result = vector * 2.0);
            Assert.Throws<ArgumentNullException>(() => result = 2.0 * vector);
        }

        [Test]
        public void OperatorDivideThrowsArgumentNullExceptionWhenVectorIsNull()
        {
            Vector vector = null;
            Assert.Throws<ArgumentNullException>(() => vector = vector / 2.0);
        }
        
        protected abstract Vector CreateVector(int size);

        protected abstract Vector CreateVector(IList<double> data);
    }
}