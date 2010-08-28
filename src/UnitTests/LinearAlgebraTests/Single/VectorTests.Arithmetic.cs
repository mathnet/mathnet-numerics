// <copyright file="VectorTests.Arithmetic.cs" company="Math.NET">
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

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Single
{
    using System;
    using LinearAlgebra.Generic;
    using MbUnit.Framework;

    public abstract partial class VectorTests
    {
        [Test]
        public void CanCallPlus()
        {
            var vector = CreateVector(Data);
            var other = vector.Plus();

            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(vector[i], other[i]);
            }
        }

        [Test]
        public void OperatorPlusThrowsArgumentNullExceptionWhenCallOnNullVector()
        {
            Vector<float> vector = null;
            Vector<float> other = null;
            Assert.Throws<ArgumentNullException>(() => other = +vector);
        }

        [Test]
        public void CanCallUnaryPlusOperator()
        {
            var vector = CreateVector(Data);
            var other = +vector;

            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(vector[i], other[i]);
            }
        }

        [Test]
        [MultipleAsserts]
        public void CanAddScalarToVector()
        {
            var copy = CreateVector(Data);
            var vector = copy.Add(2.0f);

            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(Data[i] + 2.0, vector[i]);
            }

            vector.Add(0.0f);
            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(Data[i] + 2.0, vector[i]);
            }
        }

        [Test]
        [MultipleAsserts]
        public void CanAddScalarToVectorUsingResultVector()
        {
            var vector = CreateVector(Data);
            var result = CreateVector(Data.Length);
            vector.Add(2.0f, result);

            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(Data[i], vector[i], "Making sure the original vector wasn't modified.");
                Assert.AreEqual(Data[i] + 2.0, result[i]);
            }

            vector.Add(0.0f, result);
            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(Data[i], result[i]);
            }
        }

        [Test]
        public void ThrowsArgumentNullExceptionWhenAddingScalarWithNullResultVector()
        {
            var vector = CreateVector(Data.Length);
            Assert.Throws<ArgumentNullException>(() => vector.Add(0.0f, null));
        }

        [Test]
        public void ThrowsArgumentExceptionWhenAddingScalarWithWrongSizeResultVector()
        {
            var vector = CreateVector(Data.Length);
            var result = CreateVector(Data.Length + 1);
            Assert.Throws<ArgumentException>(() => vector.Add(0.0f, result));
        }

        [Test]
        public void ThrowsArgumentNullExceptionWhenAddingTwoVectorsAndOneIsNull()
        {
            var vector = CreateVector(Data);
            Assert.Throws<ArgumentNullException>(() => vector.Add(null));
        }

        [Test]
        public void ThrowsArgumentExceptionWhenAddingTwoVectorsOfDifferingSize()
        {
            var vector = CreateVector(Data.Length);
            var other = CreateVector(Data.Length + 1);
            Assert.Throws<ArgumentException>(() => vector.Add(other));
        }

        [Test]
        public void ThrowsArgumentNullExceptionWhenAddingTwoVectorsAndResultIsNull()
        {
            var vector = CreateVector(Data.Length);
            var other = CreateVector(Data.Length + 1);
            Assert.Throws<ArgumentNullException>(() => vector.Add(other, null));
        }

        [Test]
        public void ThrowsArgumentExceptionWhenAddingTwoVectorsAndResultIsDifferentSize()
        {
            var vector = CreateVector(Data.Length);
            var other = CreateVector(Data.Length);
            var result = CreateVector(Data.Length + 1);
            Assert.Throws<ArgumentException>(() => vector.Add(other, result));
        }

        [Test]
        public void AdditionOperatorThrowsArgumentNullExpectionIfAVectorIsNull()
        {
            Vector<float> a = null;
            var b = CreateVector(Data.Length);
            Assert.Throws<ArgumentNullException>(() => a += b);

            a = b;
            b = null;
            Assert.Throws<ArgumentNullException>(() => a += b);
        }

        [Test]
        public void AdditionOperatorThrowsArgumentExpectionIfVectorsAreDifferentSize()
        {
            var a = CreateVector(Data.Length);
            var b = CreateVector(Data.Length + 1);
            Assert.Throws<ArgumentException>(() => a += b);
        }

        [Test]
        public void CanAddTwoVectors()
        {
            var copy = CreateVector(Data);
            var other = CreateVector(Data);
            var vector = copy.Add(other);

            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(Data[i] * 2.0, vector[i]);
            }
        }

        [Test]
        [MultipleAsserts]
        public void CanAddTwoVectorsUsingResultVector()
        {
            var vector = CreateVector(Data);
            var other = CreateVector(Data);
            var result = CreateVector(Data.Length);
            vector.Add(other, result);

            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(Data[i], vector[i], "Making sure the original vector wasn't modified.");
                Assert.AreEqual(Data[i], other[i], "Making sure the original vector wasn't modified.");
                Assert.AreEqual(Data[i] * 2.0, result[i]);
            }
        }

        [Test]
        [MultipleAsserts]
        public void CanAddTwoVectorsUsingOperator()
        {
            var vector = CreateVector(Data);
            var other = CreateVector(Data);
            var result = vector + other;

            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(Data[i], vector[i], "Making sure the original vector wasn't modified.");
                Assert.AreEqual(Data[i], other[i], "Making sure the original vector wasn't modified.");
                Assert.AreEqual(Data[i] * 2.0, result[i]);
            }
        }

        [Test]
        public void CanAddVectorToItself()
        {
            var copy = CreateVector(Data);
            var vector = copy.Add(copy);

            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(Data[i] * 2.0, vector[i]);
            }
        }

        [Test]
        [MultipleAsserts]
        public void CanAddVectorToItselfUsingResultVector()
        {
            var vector = CreateVector(Data);
            var result = CreateVector(Data.Length);
            vector.Add(vector, result);

            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(Data[i], vector[i], "Making sure the original vector wasn't modified.");
                Assert.AreEqual(Data[i] * 2.0, result[i]);
            }
        }

        [Test]
        [MultipleAsserts]
        public void CanAddTwoVectorsUsingItselfAsResultVector()
        {
            var vector = CreateVector(Data);
            var other = CreateVector(Data);
            vector.Add(other, vector);

            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(Data[i], other[i], "Making sure the original vector wasn't modified.");
                Assert.AreEqual(Data[i] * 2.0, vector[i]);
            }
        }

        [Test]
        public void CanCallNegate()
        {
            var vector = CreateVector(Data);
            var other = vector.Negate();
            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(-Data[i], other[i]);
            }
        }

        [Test]
        public void OperatorNegateThrowsArgumentNullExceptionWhenCallOnNullVector()
        {
            Vector<float> vector = null;
            Vector<float> other = null;
            Assert.Throws<ArgumentNullException>(() => other = -vector);
        }

        [Test]
        public void CanCallUnaryNegationOperator()
        {
            var vector = CreateVector(Data);
            var other = -vector;
            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(-Data[i], other[i]);
            }
        }

        [Test]
        [MultipleAsserts]
        public void CanSubtractScalarFromVector()
        {
            var copy = CreateVector(Data);
            var vector = copy.Subtract(2.0f);

            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(Data[i] - 2.0, vector[i]);
            }

            vector.Subtract(0.0f);
            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(Data[i] - 2.0, vector[i]);
            }
        }

        [Test]
        [MultipleAsserts]
        public void CanSubtractScalarFromVectorUsingResultVector()
        {
            var vector = CreateVector(Data);
            var result = CreateVector(Data.Length);
            vector.Subtract(2.0f, result);

            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(Data[i], vector[i], "Making sure the original vector wasn't modified.");
                Assert.AreEqual(Data[i] - 2.0, result[i]);
            }

            vector.Subtract(0.0f, result);
            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(Data[i], result[i]);
            }
        }

        [Test]
        public void ThrowsArgumentNullExceptionWhenSubtractingScalarWithNullResultVector()
        {
            var vector = CreateVector(Data.Length);
            Assert.Throws<ArgumentNullException>(() => vector.Subtract(0.0f, null));
        }

        [Test]
        public void ThrowsArgumentExceptionWhenSubtractingScalarWithWrongSizeResultVector()
        {
            var vector = CreateVector(Data.Length);
            var result = CreateVector(Data.Length + 1);
            Assert.Throws<ArgumentException>(() => vector.Subtract(0.0f, result));
        }

        [Test]
        public void ThrowsArgumentNullExceptionWhenSubtractingTwoVectorsAndOneIsNull()
        {
            var vector = CreateVector(Data);
            Assert.Throws<ArgumentNullException>(() => vector.Subtract(null));
        }

        [Test]
        public void ThrowsArgumentExceptionWhenSubtractingTwoVectorsOfDifferingSize()
        {
            var vector = CreateVector(Data.Length);
            var other = CreateVector(Data.Length + 1);
            Assert.Throws<ArgumentException>(() => vector.Subtract(other));
        }

        [Test]
        public void ThrowsArgumentNullExceptionWhenSubtractingTwoVectorsAndResultIsNull()
        {
            var vector = CreateVector(Data.Length);
            var other = CreateVector(Data.Length + 1);
            Assert.Throws<ArgumentNullException>(() => vector.Subtract(other, null));
        }

        [Test]
        public void ThrowsArgumentExceptionWhenSubtractingTwoVectorsAndResultIsDifferentSize()
        {
            var vector = CreateVector(Data.Length);
            var other = CreateVector(Data.Length);
            var result = CreateVector(Data.Length + 1);
            Assert.Throws<ArgumentException>(() => vector.Subtract(other, result));
        }

        [Test]
        public void SubtractionOperatorThrowsArgumentNullExpectionIfAVectorIsNull()
        {
            Vector<float> a = null;
            var b = CreateVector(Data.Length);
            Assert.Throws<ArgumentNullException>(() => a -= b);

            a = b;
            b = null;
            Assert.Throws<ArgumentNullException>(() => a -= b);
        }

        [Test]
        public void SubtractionOperatorThrowsArgumentExpectionIfVectorsAreDifferentSize()
        {
            var a = CreateVector(Data.Length);
            var b = CreateVector(Data.Length + 1);
            Assert.Throws<ArgumentException>(() => a -= b);
        }

        [Test]
        public void CanSubtractTwoVectors()
        {
            var copy = CreateVector(Data);
            var other = CreateVector(Data);
            var vector = copy.Subtract(other);

            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(0.0, vector[i]);
            }
        }

        [Test]
        [MultipleAsserts]
        public void CanSubtractTwoVectorsUsingResultVector()
        {
            var vector = CreateVector(Data);
            var other = CreateVector(Data);
            var result = CreateVector(Data.Length);
            vector.Subtract(other, result);

            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(Data[i], vector[i], "Making sure the original vector wasn't modified.");
                Assert.AreEqual(Data[i], other[i], "Making sure the original vector wasn't modified.");
                Assert.AreEqual(0.0, result[i]);
            }
        }

        [Test]
        [MultipleAsserts]
        public void CanSubtractTwoVectorsUsingOperator()
        {
            var vector = CreateVector(Data);
            var other = CreateVector(Data);
            var result = vector - other;

            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(Data[i], vector[i], "Making sure the original vector wasn't modified.");
                Assert.AreEqual(Data[i], other[i], "Making sure the original vector wasn't modified.");
                Assert.AreEqual(0.0, result[i]);
            }
        }

        [Test]
        public void CanSubtractVectorFromItself()
        {
            var copy = CreateVector(Data);
            var vector = copy.Subtract(copy);

            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(0.0, vector[i]);
            }
        }

        [Test]
        [MultipleAsserts]
        public void CanSubtractVectorFromItselfUsingResultVector()
        {
            var vector = CreateVector(Data);
            var result = CreateVector(Data.Length);
            vector.Subtract(vector, result);

            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(Data[i], vector[i], "Making sure the original vector wasn't modified.");
                Assert.AreEqual(0.0, result[i]);
            }
        }

        [Test]
        [MultipleAsserts]
        public void CanSubtractTwoVectorsUsingItselfAsResultVector()
        {
            var vector = CreateVector(Data);
            var other = CreateVector(Data);
            vector.Subtract(other, vector);

            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(Data[i], other[i], "Making sure the original vector wasn't modified.");
                Assert.AreEqual(0.0, vector[i]);
            }
        }

        [Test]
        [MultipleAsserts]
        public void CanDivideVectorByScalar()
        {
            var copy = CreateVector(Data);
            var vector = copy.Divide(2.0f);

            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(Data[i] / 2.0, vector[i]);
            }

            vector.Divide(1.0f);
            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(Data[i] / 2.0, vector[i]);
            }
        }

        [Test]
        [MultipleAsserts]
        public void CanDivideVectorByScalarUsingResultVector()
        {
            var vector = CreateVector(Data);
            var result = CreateVector(Data.Length);
            vector.Divide(2.0f, result);

            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(Data[i], vector[i], "Making sure the original vector wasn't modified.");
                Assert.AreEqual(Data[i] / 2.0, result[i]);
            }

            vector.Divide(1.0f, result);
            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(Data[i], result[i]);
            }
        }

        [Test]
        [MultipleAsserts]
        public void CanMultiplyVectorByScalar()
        {
            var copy = CreateVector(Data);
            var vector = copy.Multiply(2.0f);

            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(Data[i] * 2.0, vector[i]);
            }

            vector.Multiply(1.0f);
            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(Data[i] * 2.0, vector[i]);
            }
        }

        [Test]
        [MultipleAsserts]
        public void CanMultiplyVectorByScalarUsingResultVector()
        {
            var vector = CreateVector(Data);
            var result = CreateVector(Data.Length);
            vector.Multiply(2.0f, result);

            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(Data[i], vector[i], "Making sure the original vector wasn't modified.");
                Assert.AreEqual(Data[i] * 2.0, result[i]);
            }

            vector.Multiply(1.0f, result);
            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(Data[i], result[i]);
            }
        }

        [Test]
        public void ThrowsArgumentNullExceptionWhenMultiplyingScalarWithNullResultVector()
        {
            var vector = CreateVector(Data.Length);
            Assert.Throws<ArgumentNullException>(() => vector.Multiply(1.0f, null));
        }

        [Test]
        public void ThrowsArgumentNullExceptionWhenDividingScalarWithNullResultVector()
        {
            var vector = CreateVector(Data.Length);
            Assert.Throws<ArgumentNullException>(() => vector.Divide(1.0f, null));
        }

        [Test]
        public void ThrowsArgumentExceptionWhenMultiplyingScalarWithWrongSizeResultVector()
        {
            var vector = CreateVector(Data.Length);
            var result = CreateVector(Data.Length + 1);
            Assert.Throws<ArgumentException>(() => vector.Multiply(0.0f, result));
        }

        [Test]
        public void ThrowsArgumentExceptionWhenDividingScalarWithWrongSizeResultVector()
        {
            var vector = CreateVector(Data.Length);
            var result = CreateVector(Data.Length + 1);
            Assert.Throws<ArgumentException>(() => vector.Divide(0.0f, result));
        }

        [Test]
        [MultipleAsserts]
        public void CanMultiplyVectorByScalarUsingOperators()
        {
            var vector = CreateVector(Data);
            vector = vector * 2.0f;

            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(Data[i] * 2.0, vector[i]);
            }

            vector = vector * 1.0f;
            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(Data[i] * 2.0, vector[i]);
            }

            vector = CreateVector(Data);
            vector = 2.0f * vector;

            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(Data[i] * 2.0, vector[i]);
            }

            vector = 1.0f * vector;
            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(Data[i] * 2.0, vector[i]);
            }
        }

        [Test]
        [MultipleAsserts]
        public void CanDivideVectorByScalarUsingOperators()
        {
            var vector = CreateVector(Data);
            vector = vector / 2.0f;

            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(Data[i] / 2.0, vector[i]);
            }

            vector = vector / 1.0f;
            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(Data[i] / 2.0, vector[i]);
            }
        }

        [Test]
        [MultipleAsserts]
        public void OperatorMultiplyThrowsArgumentNullExceptionWhenVectorIsNull()
        {
            Vector<float> vector = null;
            Vector<float> result = null;
            Assert.Throws<ArgumentNullException>(() => result = vector * 2.0f);
            Assert.Throws<ArgumentNullException>(() => result = 2.0f * vector);
        }

        [Test]
        public void OperatorDivideThrowsArgumentNullExceptionWhenVectorIsNull()
        {
            Vector<float> vector = null;
            Assert.Throws<ArgumentNullException>(() => vector = vector / 2.0f);
        }

        [Test]
        public void CanDotProduct()
        {
            var dataA = CreateVector(Data);
            var dataB = CreateVector(Data);

            Assert.AreEqual(55.0, dataA.DotProduct(dataB));
        }

        [Test]
        [ExpectedArgumentNullException]
        public void DotProductThrowsExceptionWhenArgumentIsNull()
        {
            var dataA = CreateVector(Data);
            Vector<float> dataB = null;

            dataA.DotProduct(dataB);
        }

        [Test]
        [ExpectedArgumentException]
        public void DotProductThrowsExceptionWhenArgumentHasDifferentSize()
        {
            var dataA = CreateVector(Data);
            var dataB = CreateVector(new float[] { 1, 2, 3, 4, 5, 6 });

            dataA.DotProduct(dataB);
        }

        [Test]
        public void CanDotProductUsingOperator()
        {
            var dataA = CreateVector(Data);
            var dataB = CreateVector(Data);

            Assert.AreEqual(55.0, dataA * dataB);
        }

        [Test]
        [ExpectedArgumentNullException]
        public void OperatorDotProductThrowsExceptionWhenLeftArgumentIsNull()
        {
            var dataA = CreateVector(Data);
            Vector<float> dataB = null;

            var d = dataA * dataB;
        }

        [Test]
        [ExpectedArgumentNullException]
        public void OperatorDotProductThrowsExceptionWhenRightArgumentIsNull()
        {
            Vector<float> dataA = null;
            var dataB = CreateVector(Data);

            var d = dataA * dataB;
        }

        [Test]
        [ExpectedArgumentException]
        public void OperatorDotProductThrowsExceptionWhenArgumentHasDifferentSize()
        {
            var dataA = CreateVector(Data);
            var dataB = CreateVector(new float[] { 1, 2, 3, 4, 5, 6 });

            var d = dataA * dataB;
        }

        [Test]
        public void PointwiseMultiply()
        {
            var vector1 = CreateVector(Data);
            var vector2 = vector1.Clone();
            var result = vector1.PointwiseMultiply(vector2);
            for (var i = 0; i < vector1.Count; i++)
            {
                Assert.AreEqual(Data[i] * Data[i], result[i]);
            }
        }

        [Test]
        public void PointwiseMultiplyUsingResultVector()
        {
            var vector1 = CreateVector(Data);
            var vector2 = vector1.Clone();
            var result = CreateVector(vector1.Count);
            vector1.PointwiseMultiply(vector2, result);
            for (var i = 0; i < vector1.Count; i++)
            {
                Assert.AreEqual(Data[i] * Data[i], result[i]);
            }
        }

        [Test]
        [ExpectedArgumentNullException]
        public void PointwiseMultiplyWithOtherNullShouldThrowException()
        {
            var vector1 = CreateVector(Data);
            Vector<float> vector2 = null;
            var result = CreateVector(vector1.Count);
            vector1.PointwiseMultiply(vector2, result);
        }

        [Test]
        [ExpectedArgumentNullException]
        public void PointwiseMultiplyWithResultNullShouldThrowException()
        {
            var vector1 = CreateVector(Data);
            var vector2 = vector1.Clone();
            Vector<float> result = null;
            vector1.PointwiseMultiply(vector2, result);
        }

        [Test]
        [ExpectedArgumentException]
        public void PointwiseMultiplyWithInvalidResultLengthShouldThrowException()
        {
            var vector1 = CreateVector(Data);
            var vector2 = vector1.Clone();
            var result = CreateVector(vector1.Count + 1);
            vector1.PointwiseMultiply(vector2, result);
        }

        [Test]
        public void PointWiseDivide()
        {
            var vector1 = CreateVector(Data);
            var vector2 = vector1.Clone();
            var result = vector1.PointwiseDivide(vector2);
            for (var i = 0; i < vector1.Count; i++)
            {
                Assert.AreEqual(Data[i] / Data[i], result[i]);
            }
        }
       
        [Test]
        public void PointWiseDivideUsingResultVector()
        {
            var vector1 = CreateVector(Data);
            var vector2 = vector1.Clone();
            var result = CreateVector(vector1.Count);
            vector1.PointwiseDivide(vector2, result);
            for (var i = 0; i < vector1.Count; i++)
            {
                Assert.AreEqual(Data[i] / Data[i], result[i]);
            }
        }

        [Test]
        [ExpectedArgumentNullException]
        public void PointwiseDivideWithOtherNullShouldThrowException()
        {
            var vector1 = CreateVector(Data);
            Vector<float> vector2 = null;
            var result = CreateVector(vector1.Count);
            vector1.PointwiseDivide(vector2, result);
        }

        [Test]
        [ExpectedArgumentNullException]
        public void PointwiseDivideWithResultNullShouldThrowException()
        {
            var vector1 = CreateVector(Data);
            var vector2 = vector1.Clone();
            Vector<float> result = null;
            vector1.PointwiseDivide(vector2, result);
        }

        [Test]
        [ExpectedArgumentException]
        public void PointwiseDivideWithInvalidResultLengthShouldThrowException()
        {
            var vector1 = CreateVector(Data);
            var vector2 = vector1.Clone();
            var result = CreateVector(vector1.Count + 1);
            vector1.PointwiseDivide(vector2, result);
        }

        [Test]
        public void CanCalculateOuterProduct()
        {
            var vector1 = CreateVector(Data);
            var vector2 = CreateVector(Data);
            Matrix<float> m = Vector<float>.OuterProduct(vector1, vector2);
            for (var i = 0; i < vector1.Count; i++)
            {
                for (var j = 0; j < vector2.Count; j++)
                {
                    Assert.AreEqual(m[i, j], vector1[i] * vector2[j]);
                }
            }
        }

        [Test]
        [ExpectedArgumentNullException]
        public void OuterProductWithFirstParameterNullShouldThrowException()
        {
            Vector<float> vector1 = null;
            var vector2 = CreateVector(Data);
            Vector<float>.OuterProduct(vector1, vector2);
        }

        [Test]
        [ExpectedArgumentNullException]
        public void OutercProductWithSecondParameterNullShouldThrowException()
        {
            var vector1 = CreateVector(Data);
            Vector<float> vector2 = null;
            Vector<float>.OuterProduct(vector1, vector2);
        }

        [Test]
        public void CanCalculateTensorMultiply()
        {
            var vector1 = CreateVector(Data);
            var vector2 = CreateVector(Data);
            var m = vector1.TensorMultiply(vector2);
            for (var i = 0; i < vector1.Count; i++)
            {
                for (var j = 0; j < vector2.Count; j++)
                {
                    Assert.AreEqual(m[i, j], vector1[i] * vector2[j]);
                }
            }
        }

        [Test]
        [ExpectedArgumentNullException]
        public void TensorMultiplyWithNullParameterNullShouldThrowException()
        {
            var vector1 = CreateVector(Data);
            Vector<float> vector2 = null;
            vector1.TensorMultiply(vector2);
        }
    }
}