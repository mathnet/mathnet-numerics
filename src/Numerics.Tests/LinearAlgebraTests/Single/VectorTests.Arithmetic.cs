// <copyright file="VectorTests.Arithmetic.cs" company="Math.NET">
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

using MathNet.Numerics.LinearAlgebra;
using NUnit.Framework;
using System;

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Single
{
    /// <summary>
    /// Abstract class with the common arithmetic set of vector tests.
    /// </summary>
    public abstract partial class VectorTests
    {
        /// <summary>
        /// Can call unary "+" operator.
        /// </summary>
        [Test]
        public void CanCallUnaryPlusOperator()
        {
            var vector = CreateVector(Data);
            var other = +vector;
            CollectionAssert.AreEqual(vector, other);
        }

        /// <summary>
        /// Can add a scalar to a vector.
        /// </summary>
        [Test]
        public void CanAddScalarToVector()
        {
            var copy = CreateVector(Data);
            var vector = copy.Add(2.0f);

            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(Data[i] + 2.0f, vector[i]);
            }

            vector.Add(0.0f);
            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(Data[i] + 2.0f, vector[i]);
            }
        }

        /// <summary>
        /// Can add a scalar to a vector using result vector.
        /// </summary>
        [Test]
        public void CanAddScalarToVectorIntoResultVector()
        {
            var vector = CreateVector(Data);
            var result = CreateVector(Data.Length);
            vector.Add(2.0f, result);

            CollectionAssert.AreEqual(Data, vector, "Making sure the original vector wasn't modified.");
            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(Data[i] + 2.0f, result[i]);
            }

            vector.Add(0.0f, result);
            CollectionAssert.AreEqual(Data, result);
        }

        /// <summary>
        /// Adding scalar to a vector using result vector with wrong size throws an exception.
        /// </summary>
        [Test]
        public void AddingScalarWithWrongSizeResultVectorThrowsArgumentException()
        {
            var vector = CreateVector(Data.Length);
            var result = CreateVector(Data.Length + 1);
            Assert.That(() => vector.Add(0.0f, result), Throws.ArgumentException);
        }

        /// <summary>
        /// Adding two vectors of different size throws an exception.
        /// </summary>
        [Test]
        public void AddingTwoVectorsOfDifferentSizeThrowsArgumentException()
        {
            var vector = CreateVector(Data.Length);
            var other = CreateVector(Data.Length + 1);
            Assert.That(() => vector.Add(other), Throws.ArgumentException);
        }

        /// <summary>
        /// Adding two vectors when a result vector is different size throws an exception.
        /// </summary>
        [Test]
        public void AddingTwoVectorsAndResultIsDifferentSizeThrowsArgumentException()
        {
            var vector = CreateVector(Data.Length);
            var other = CreateVector(Data.Length);
            var result = CreateVector(Data.Length + 1);
            Assert.That(() => vector.Add(other, result), Throws.ArgumentException);
        }

        /// <summary>
        /// Addition operator throws <c>ArgumentException</c> if vectors are different size.
        /// </summary>
        [Test]
        public void AdditionOperatorIfVectorsAreDifferentSizeThrowsArgumentException()
        {
            var a = CreateVector(Data.Length);
            var b = CreateVector(Data.Length + 1);
            Assert.That(() => a += b, Throws.ArgumentException);
        }

        /// <summary>
        /// Can add two vectors.
        /// </summary>
        [Test]
        public void CanAddTwoVectors()
        {
            var copy = CreateVector(Data);
            var other = CreateVector(Data);
            var vector = copy.Add(other);

            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(Data[i] * 2.0f, vector[i]);
            }
        }

        /// <summary>
        /// Can add two vectors using a result vector.
        /// </summary>
        [Test]
        public void CanAddTwoVectorsIntoResultVector()
        {
            var vector = CreateVector(Data);
            var other = CreateVector(Data);
            var result = CreateVector(Data.Length);
            vector.Add(other, result);

            CollectionAssert.AreEqual(Data, vector, "Making sure the original vector wasn't modified.");
            CollectionAssert.AreEqual(Data, other, "Making sure the original vector wasn't modified.");
            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(Data[i] * 2.0f, result[i]);
            }
        }

        /// <summary>
        /// Can add two vectors using "+" operator.
        /// </summary>
        [Test]
        public void CanAddTwoVectorsUsingOperator()
        {
            var vector = CreateVector(Data);
            var other = CreateVector(Data);
            var result = vector + other;

            CollectionAssert.AreEqual(Data, vector, "Making sure the original vector wasn't modified.");
            CollectionAssert.AreEqual(Data, other, "Making sure the original vector wasn't modified.");
            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(Data[i] * 2.0f, result[i]);
            }
        }

        /// <summary>
        /// Can add a vector to itself.
        /// </summary>
        [Test]
        public void CanAddVectorToItself()
        {
            var copy = CreateVector(Data);
            var vector = copy.Add(copy);

            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(Data[i] * 2.0f, vector[i]);
            }
        }

        /// <summary>
        /// Can add a vector to itself using a result vector.
        /// </summary>
        [Test]
        public void CanAddVectorToItselfIntoResultVector()
        {
            var vector = CreateVector(Data);
            var result = CreateVector(Data.Length);
            vector.Add(vector, result);

            CollectionAssert.AreEqual(Data, vector, "Making sure the original vector wasn't modified.");
            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(Data[i] * 2.0f, result[i]);
            }
        }

        /// <summary>
        /// Can add a vector to itself as result vector.
        /// </summary>
        [Test]
        public void CanAddTwoVectorsUsingItselfAsResultVector()
        {
            var vector = CreateVector(Data);
            var other = CreateVector(Data);
            vector.Add(other, vector);

            CollectionAssert.AreEqual(Data, other, "Making sure the original vector wasn't modified.");
            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(Data[i] * 2.0f, vector[i]);
            }
        }

        /// <summary>
        /// Can negate a vector.
        /// </summary>
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

        /// <summary>
        /// Can call unary negation operator.
        /// </summary>
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

        /// <summary>
        /// Can subtract a scalar from a vector.
        /// </summary>
        [Test]
        public void CanSubtractScalarFromVector()
        {
            var copy = CreateVector(Data);
            var vector = copy.Subtract(2.0f);

            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(Data[i] - 2.0f, vector[i]);
            }

            vector.Subtract(0.0f);
            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(Data[i] - 2.0f, vector[i]);
            }
        }

        /// <summary>
        /// Can subtract a scalar from a vector using a result vector.
        /// </summary>
        [Test]
        public void CanSubtractScalarFromVectorIntoResultVector()
        {
            var vector = CreateVector(Data);
            var result = CreateVector(Data.Length);
            vector.Subtract(2.0f, result);

            CollectionAssert.AreEqual(Data, vector, "Making sure the original vector wasn't modified.");
            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(Data[i], vector[i], "Making sure the original vector wasn't modified.");
                Assert.AreEqual(Data[i] - 2.0f, result[i]);
            }

            vector.Subtract(0.0f, result);
            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(Data[i], result[i]);
            }
        }

        /// <summary>
        /// Subtracting a scalar with wrong size result vector throws <c>ArgumentException</c>.
        /// </summary>
        [Test]
        public void SubtractingScalarWithWrongSizeResultVectorThrowsArgumentException()
        {
            var vector = CreateVector(Data.Length);
            var result = CreateVector(Data.Length + 1);
            Assert.That(() => vector.Subtract(0.0f, result), Throws.ArgumentException);
        }

        /// <summary>
        /// Subtracting two vectors of differing size throws <c>ArgumentException</c>.
        /// </summary>
        [Test]
        public void SubtractingTwoVectorsOfDifferingSizeThrowsArgumentException()
        {
            var vector = CreateVector(Data.Length);
            var other = CreateVector(Data.Length + 1);
            Assert.That(() => vector.Subtract(other), Throws.ArgumentException);
        }

        /// <summary>
        /// Subtracting two vectors when a result vector is different size throws <c>ArgumentException</c>.
        /// </summary>
        [Test]
        public void SubtractingTwoVectorsAndResultIsDifferentSizeThrowsArgumentException()
        {
            var vector = CreateVector(Data.Length);
            var other = CreateVector(Data.Length);
            var result = CreateVector(Data.Length + 1);
            Assert.That(() => vector.Subtract(other, result), Throws.ArgumentException);
        }

        /// <summary>
        /// Subtraction operator throws <c>ArgumentException</c> if vectors are different size.
        /// </summary>
        [Test]
        public void SubtractionOperatorIfVectorsAreDifferentSizeThrowsArgumentException()
        {
            var a = CreateVector(Data.Length);
            var b = CreateVector(Data.Length + 1);
            Assert.That(() => a -= b, Throws.ArgumentException);
        }

        /// <summary>
        /// Can subtract two vectors.
        /// </summary>
        [Test]
        public void CanSubtractTwoVectors()
        {
            var copy = CreateVector(Data);
            var other = CreateVector(Data);
            var vector = copy.Subtract(other);

            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(0.0f, vector[i]);
            }
        }

        /// <summary>
        /// Can subtract two vectors using a result vector.
        /// </summary>
        [Test]
        public void CanSubtractTwoVectorsIntoResultVector()
        {
            var vector = CreateVector(Data);
            var other = CreateVector(Data);
            var result = CreateVector(Data.Length);
            vector.Subtract(other, result);

            CollectionAssert.AreEqual(Data, vector, "Making sure the original vector wasn't modified.");
            CollectionAssert.AreEqual(Data, other, "Making sure the original vector wasn't modified.");
            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(0.0f, result[i]);
            }
        }

        /// <summary>
        /// Can subtract two vectors using "-" operator.
        /// </summary>
        [Test]
        public void CanSubtractTwoVectorsUsingOperator()
        {
            var vector = CreateVector(Data);
            var other = CreateVector(Data);
            var result = vector - other;

            CollectionAssert.AreEqual(Data, vector, "Making sure the original vector wasn't modified.");
            CollectionAssert.AreEqual(Data, other, "Making sure the original vector wasn't modified.");
            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(0.0f, result[i]);
            }
        }

        /// <summary>
        /// Can subtract a vector from itself.
        /// </summary>
        [Test]
        public void CanSubtractVectorFromItself()
        {
            var copy = CreateVector(Data);
            var vector = copy.Subtract(copy);

            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(0.0f, vector[i]);
            }
        }

        /// <summary>
        /// Can subtract a vector from itself using a result vector.
        /// </summary>
        [Test]
        public void CanSubtractVectorFromItselfIntoResultVector()
        {
            var vector = CreateVector(Data);
            var result = CreateVector(Data.Length);
            vector.Subtract(vector, result);

            CollectionAssert.AreEqual(Data, vector, "Making sure the original vector wasn't modified.");
            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(0.0f, result[i]);
            }
        }

        /// <summary>
        /// Can subtract two vectors using itself as result vector.
        /// </summary>
        [Test]
        public void CanSubtractTwoVectorsUsingItselfAsResultVector()
        {
            var vector = CreateVector(Data);
            var other = CreateVector(Data);
            vector.Subtract(other, vector);

            CollectionAssert.AreEqual(Data, other, "Making sure the original vector wasn't modified.");
            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(0.0f, vector[i]);
            }
        }

        /// <summary>
        /// Can divide a vector by a scalar.
        /// </summary>
        [Test]
        public void CanDivideVectorByScalar()
        {
            var copy = CreateVector(Data);
            var vector = copy.Divide(2.0f);

            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(Data[i] / 2.0f, vector[i]);
            }

            vector.Divide(1.0f);
            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(Data[i] / 2.0f, vector[i]);
            }
        }

        /// <summary>
        /// Can divide a vector by a scalar using a result vector.
        /// </summary>
        [Test]
        public void CanDivideVectorByScalarIntoResultVector()
        {
            var vector = CreateVector(Data);
            var result = CreateVector(Data.Length);
            vector.Divide(2.0f, result);

            CollectionAssert.AreEqual(Data, vector, "Making sure the original vector wasn't modified.");
            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(Data[i] / 2.0f, result[i]);
            }

            vector.Divide(1.0f, result);
            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(Data[i], result[i]);
            }
        }

        /// <summary>
        /// Can multiply a vector by a scalar.
        /// </summary>
        [Test]
        public void CanMultiplyVectorByScalar()
        {
            var copy = CreateVector(Data);
            var vector = copy.Multiply(2.0f);

            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(Data[i] * 2.0f, vector[i]);
            }

            vector.Multiply(1.0f);
            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(Data[i] * 2.0f, vector[i]);
            }
        }

        /// <summary>
        /// Can multiply a vector by a scalar using a result vector.
        /// </summary>
        [Test]
        public void CanMultiplyVectorByScalarIntoResultVector()
        {
            var vector = CreateVector(Data);
            var result = CreateVector(Data.Length);
            vector.Multiply(2.0f, result);

            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(Data[i], vector[i], "Making sure the original vector wasn't modified.");
                Assert.AreEqual(Data[i] * 2.0f, result[i]);
            }

            vector.Multiply(1.0f, result);
            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(Data[i], result[i]);
            }
        }

        /// <summary>
        /// Multiplying by scalar with wrong result vector size throws <c>ArgumentException</c>.
        /// </summary>
        [Test]
        public void MultiplyingScalarWithWrongSizeResultVectorThrowsArgumentException()
        {
            var vector = CreateVector(Data.Length);
            var result = CreateVector(Data.Length + 1);
            Assert.That(() => vector.Multiply(0.0f, result), Throws.ArgumentException);
        }

        /// <summary>
        /// Dividing by scalar with wrong result vector size throws <c>ArgumentException</c>.
        /// </summary>
        [Test]
        public void DividingScalarWithWrongSizeResultVectorThrowsArgumentException()
        {
            var vector = CreateVector(Data.Length);
            var result = CreateVector(Data.Length + 1);
            Assert.That(() => vector.Divide(0.0f, result), Throws.ArgumentException);
        }

        /// <summary>
        /// Can multiply a vector by scalar using operators.
        /// </summary>
        [Test]
        public void CanMultiplyVectorByScalarUsingOperators()
        {
            var vector = CreateVector(Data);
            vector = vector * 2.0f;

            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(Data[i] * 2.0f, vector[i]);
            }

            vector = vector * 1.0f;
            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(Data[i] * 2.0f, vector[i]);
            }

            vector = CreateVector(Data);
            vector = 2.0f * vector;

            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(Data[i] * 2.0f, vector[i]);
            }

            vector = 1.0f * vector;
            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(Data[i] * 2.0f, vector[i]);
            }
        }

        /// <summary>
        /// Can divide a vector by scalar using operators.
        /// </summary>
        [Test]
        public void CanDivideVectorByScalarUsingOperators()
        {
            var vector = CreateVector(Data);
            vector = vector / 2.0f;

            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(Data[i] / 2.0f, vector[i]);
            }

            vector = vector / 1.0f;
            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(Data[i] / 2.0f, vector[i]);
            }
        }

        /// <summary>
        /// Can calculate the dot product
        /// </summary>
        [Test]
        public void CanDotProduct()
        {
            var dataA = CreateVector(Data);
            var dataB = CreateVector(Data);

            Assert.AreEqual(55.0f, dataA.DotProduct(dataB));
        }

        /// <summary>
        /// Dot product throws <c>ArgumentException</c> when an argument has different size
        /// </summary>
        [Test]
        public void DotProductWhenDifferentSizeThrowsArgumentException()
        {
            var dataA = CreateVector(Data);
            var dataB = CreateVector(new float[] { 1, 2, 3, 4, 5, 6 });

            Assert.That(() => dataA.DotProduct(dataB), Throws.ArgumentException);
        }

        /// <summary>
        /// Can calculate the dot product using "*" operator.
        /// </summary>
        [Test]
        public void CanDotProductUsingOperator()
        {
            var dataA = CreateVector(Data);
            var dataB = CreateVector(Data);

            Assert.AreEqual(55.0f, dataA * dataB);
        }

        /// <summary>
        /// Operator "*" throws <c>ArgumentException</c> when the argument has different size.
        /// </summary>
        [Test]
        public void OperatorDotProductWhenDifferentSizeThrowsArgumentException()
        {
            var dataA = CreateVector(Data);
            var dataB = CreateVector(new float[] { 1, 2, 3, 4, 5, 6 });

            Assert.Throws<ArgumentException>(() => { var d = dataA * dataB; });
        }

        /// <summary>
        /// Can pointwise multiply two vectors.
        /// </summary>
        [Test]
        public void CanPointwiseMultiply()
        {
            var vector1 = CreateVector(Data);
            var vector2 = vector1.Clone();
            var result = vector1.PointwiseMultiply(vector2);
            for (var i = 0; i < vector1.Count; i++)
            {
                Assert.AreEqual(Data[i] * Data[i], result[i]);
            }
        }

        /// <summary>
        /// Can pointwise multiply two vectors using a result vector.
        /// </summary>
        [Test]
        public void CanPointwiseMultiplyIntoResultVector()
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

        /// <summary>
        /// Pointwise multiply with a result vector wrong size throws <c>ArgumentException</c>.
        /// </summary>
        [Test]
        public void PointwiseMultiplyWithInvalidResultLengthThrowsArgumentException()
        {
            var vector1 = CreateVector(Data);
            var vector2 = vector1.Clone();
            var result = CreateVector(vector1.Count + 1);
            Assert.That(() => vector1.PointwiseMultiply(vector2, result), Throws.ArgumentException);
        }

        /// <summary>
        /// Can pointwise divide two vectors using a result vector.
        /// </summary>
        [Test]
        public void CanPointWiseDivide()
        {
            var vector1 = CreateVector(Data);
            var vector2 = vector1.Clone();
            var result = vector1.PointwiseDivide(vector2);
            for (var i = 0; i < vector1.Count; i++)
            {
                Assert.AreEqual(Data[i] / Data[i], result[i]);
            }
        }

        /// <summary>
        /// Can pointwise divide two vectors using a result vector.
        /// </summary>
        [Test]
        public void CanPointWiseDivideIntoResultVector()
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

        /// <summary>
        /// Pointwise divide with a result vector wrong size throws <c>ArgumentException</c>.
        /// </summary>
        [Test]
        public void PointwiseDivideWithInvalidResultLengthThrowsArgumentException()
        {
            var vector1 = CreateVector(Data);
            var vector2 = vector1.Clone();
            var result = CreateVector(vector1.Count + 1);
            Assert.That(() => vector1.PointwiseDivide(vector2, result), Throws.ArgumentException);
        }

        /// <summary>
        /// Can calculate the outer product of two vectors.
        /// </summary>
        [Test]
        public void CanCalculateOuterProduct()
        {
            var vector1 = CreateVector(Data);
            var vector2 = CreateVector(Data);
            var m = Vector<float>.OuterProduct(vector1, vector2);
            for (var i = 0; i < vector1.Count; i++)
            {
                for (var j = 0; j < vector2.Count; j++)
                {
                    Assert.AreEqual(m[i, j], vector1[i] * vector2[j]);
                }
            }
        }

        /// <summary>
        /// Can calculate the tensor multiply.
        /// </summary>
        [Test]
        public void CanCalculateTensorMultiply()
        {
            var vector1 = CreateVector(Data);
            var vector2 = CreateVector(Data);
            var m = vector1.OuterProduct(vector2);
            for (var i = 0; i < vector1.Count; i++)
            {
                for (var j = 0; j < vector2.Count; j++)
                {
                    Assert.AreEqual(m[i, j], vector1[i] * vector2[j]);
                }
            }
        }

        [Test]
        public void CanComputeRemainderUsingOperator()
        {
            var vector = CreateVector(Data);
            var mod = vector % (-4.5f);
            for (var index = 0; index < Data.Length; index++)
            {
                AssertHelpers.AlmostEqualRelative(Euclid.Remainder(Data[index], -4.5f), mod[index], 14);
            }
        }

        [Test]
        public void CanComputeRemainder()
        {
            var vector = CreateVector(Data);
            var mod = vector.Remainder(-3.2f);
            for (var index = 0; index < Data.Length; index++)
            {
                AssertHelpers.AlmostEqualRelative(Euclid.Remainder(Data[index], -3.2f), mod[index], 14);
            }
        }

        [Test]
        public void CanComputeRemainderUsingResultVector()
        {
            var vector = CreateVector(Data);
            var mod = CreateVector(vector.Count);
            vector.Remainder(-3.2f, mod);
            for (var index = 0; index < Data.Length; index++)
            {
                AssertHelpers.AlmostEqualRelative(Euclid.Remainder(Data[index], -3.2f), mod[index], 14);
            }
        }

        [Test]
        public void CanComputeRemainderUsingSameResultVector()
        {
            var vector = CreateVector(Data);
            vector.Remainder(-3.2f, vector);
            for (var index = 0; index < Data.Length; index++)
            {
                AssertHelpers.AlmostEqualRelative(Euclid.Remainder(Data[index], -3.2f), vector[index], 14);
            }
        }

        [Test]
        public void CanComputeModulus()
        {
            var vector = CreateVector(Data);
            var mod = vector.Modulus(-3.2f);
            for (var index = 0; index < Data.Length; index++)
            {
                AssertHelpers.AlmostEqualRelative(Euclid.Modulus(Data[index], -3.2f), mod[index], 14);
            }
        }

        [Test]
        public void CanComputeModulusUsingResultVector()
        {
            var vector = CreateVector(Data);
            var mod = CreateVector(vector.Count);
            vector.Modulus(-3.2f, mod);
            for (var index = 0; index < Data.Length; index++)
            {
                AssertHelpers.AlmostEqualRelative(Euclid.Modulus(Data[index], -3.2f), mod[index], 14);
            }
        }

        [Test]
        public void CanComputeModulusUsingSameResultVector()
        {
            var vector = CreateVector(Data);
            vector.Modulus(-3.2f, vector);
            for (var index = 0; index < Data.Length; index++)
            {
                AssertHelpers.AlmostEqualRelative(Euclid.Modulus(Data[index], -3.2f), vector[index], 14);
            }
        }
    }
}
