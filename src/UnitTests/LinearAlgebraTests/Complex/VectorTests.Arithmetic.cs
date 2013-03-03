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

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Complex
{
    using System;
    using System.Numerics;
    using LinearAlgebra.Generic;
    using NUnit.Framework;

    /// <summary>
    /// Abstract class with the common arithmetic set of vector tests.
    /// </summary>
    public abstract partial class VectorTests
    {
        /// <summary>
        /// Operator "+" throws <c>ArgumentNullException</c> when call on <c>null</c> vector.
        /// </summary>
        [Test]
        public void OperatorPlusWhenVectorIsNullThrowsArgumentNullException()
        {
            Vector<Complex> vector = null;
            Vector<Complex> other = null;
            Assert.Throws<ArgumentNullException>(() => other = +vector);
        }

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
            var vector = copy.Add(2.0);

            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(Data[i] + 2.0, vector[i]);
            }

            vector.Add(0.0);
            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(Data[i] + 2.0, vector[i]);
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
            vector.Add(2.0, result);

            CollectionAssert.AreEqual(Data, vector, "Making sure the original vector wasn't modified.");
            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(Data[i] + 2.0, result[i]);
            }

            vector.Add(0.0, result);
            CollectionAssert.AreEqual(Data, result);
        }

        /// <summary>
        /// Adding scalar to a vector when result vector is <c>null</c> throws an exception.
        /// </summary>
        [Test]
        public void AddingScalarWithNullResultVectorThrowsArgumentNullException()
        {
            var vector = CreateVector(Data.Length);
            Assert.Throws<ArgumentNullException>(() => vector.Add(0.0, null));
        }

        /// <summary>
        /// Adding scalar to a vector using result vector with wrong size throws an exception.
        /// </summary>
        [Test]
        public void AddingScalarWithWrongSizeResultVectorThrowsArgumentException()
        {
            var vector = CreateVector(Data.Length);
            var result = CreateVector(Data.Length + 1);
            Assert.Throws<ArgumentException>(() => vector.Add(0.0, result));
        }

        /// <summary>
        /// Adding two vectors when one is <c>null</c> throws an exception.
        /// </summary>
        [Test]
        public void AddingTwoVectorsAndOneIsNullThrowsArgumentNullException()
        {
            var vector = CreateVector(Data);
            Assert.Throws<ArgumentNullException>(() => vector.Add(null));
        }

        /// <summary>
        /// Adding two vectors of different size throws an exception.
        /// </summary>
        [Test]
        public void AddingTwoVectorsOfDifferentSizeThrowsArgumentException()
        {
            var vector = CreateVector(Data.Length);
            var other = CreateVector(Data.Length + 1);
            Assert.Throws<ArgumentException>(() => vector.Add(other));
        }

        /// <summary>
        /// Adding two vectors when a result vector is <c>null</c> throws an exception.
        /// </summary>
        [Test]
        public void AddingTwoVectorsAndResultIsNullThrowsArgumentNullException()
        {
            var vector = CreateVector(Data.Length);
            var other = CreateVector(Data.Length + 1);
            Assert.Throws<ArgumentNullException>(() => vector.Add(other, null));
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
            Assert.Throws<ArgumentException>(() => vector.Add(other, result));
        }

        /// <summary>
        /// Addition operator throws <c>ArgumentNullException</c> if a vector is <c>null</c>.
        /// </summary>
        [Test]
        public void AdditionOperatorIfAVectorIsNullThrowsArgumentNullException()
        {
            Vector<Complex> a = null;
            var b = CreateVector(Data.Length);
            Assert.Throws<ArgumentNullException>(() => a += b);

            a = b;
            b = null;
            Assert.Throws<ArgumentNullException>(() => a += b);
        }

        /// <summary>
        /// Addition operator throws <c>ArgumentException</c> if vectors are different size.
        /// </summary>
        [Test]
        public void AdditionOperatorIfVectorsAreDifferentSizeThrowsArgumentException()
        {
            var a = CreateVector(Data.Length);
            var b = CreateVector(Data.Length + 1);
            Assert.Throws<ArgumentException>(() => a += b);
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
                Assert.AreEqual(Data[i] * 2.0, vector[i]);
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
                Assert.AreEqual(Data[i] * 2.0, result[i]);
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
                Assert.AreEqual(Data[i] * 2.0, result[i]);
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
                Assert.AreEqual(Data[i] * 2.0, vector[i]);
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
                Assert.AreEqual(Data[i] * 2.0, result[i]);
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
                Assert.AreEqual(Data[i] * 2.0, vector[i]);
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
        /// Negate operator throws <c>ArgumentNullException</c> when call on <c>null</c> vector.
        /// </summary>
        [Test]
        public void OperatorNegateWhenVectorIsNullThrowsArgumentNullException()
        {
            Vector<Complex> vector = null;
            Vector<Complex> other = null;
            Assert.Throws<ArgumentNullException>(() => other = -vector);
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
            var vector = copy.Subtract(2.0);

            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(Data[i] - 2.0, vector[i]);
            }

            vector.Subtract(0.0);
            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(Data[i] - 2.0, vector[i]);
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
            vector.Subtract(2.0, result);

            CollectionAssert.AreEqual(Data, vector, "Making sure the original vector wasn't modified.");
            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(Data[i], vector[i], "Making sure the original vector wasn't modified.");
                Assert.AreEqual(Data[i] - 2.0, result[i]);
            }

            vector.Subtract(0.0, result);
            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(Data[i], result[i]);
            }
        }

        /// <summary>
        /// Subtracting a scalar with <c>null</c> result vector throws <c>ArgumentNullException</c>.
        /// </summary>
        [Test]
        public void SubtractingScalarWithNullResultVectorThrowsArgumentNullException()
        {
            var vector = CreateVector(Data.Length);
            Assert.Throws<ArgumentNullException>(() => vector.Subtract(0.0, null));
        }

        /// <summary>
        /// Subtracting a scalar with wrong size result vector throws <c>ArgumentException</c>.
        /// </summary>
        [Test]
        public void SubtractingScalarWithWrongSizeResultVectorThrowsArgumentException()
        {
            var vector = CreateVector(Data.Length);
            var result = CreateVector(Data.Length + 1);
            Assert.Throws<ArgumentException>(() => vector.Subtract(0.0, result));
        }

        /// <summary>
        /// Subtracting two vectors when one is <c>null</c> throws <c>ArgumentNullException</c>.
        /// </summary>
        [Test]
        public void SubtractingTwoVectorsAndOneIsNullThrowsArgumentNullException()
        {
            var vector = CreateVector(Data);
            Assert.Throws<ArgumentNullException>(() => vector.Subtract(null));
        }

        /// <summary>
        /// Subtracting two vectors of differing size throws <c>ArgumentException</c>.
        /// </summary>
        [Test]
        public void SubtractingTwoVectorsOfDifferingSizeThrowsArgumentException()
        {
            var vector = CreateVector(Data.Length);
            var other = CreateVector(Data.Length + 1);
            Assert.Throws<ArgumentException>(() => vector.Subtract(other));
        }

        /// <summary>
        /// Subtracting two vectors when a result vector is <c>null</c> throws <c>ArgumentNullException</c>.
        /// </summary>
        [Test]
        public void SubtractingTwoVectorsAndResultIsNullThrowsArgumentNullException()
        {
            var vector = CreateVector(Data.Length);
            var other = CreateVector(Data.Length + 1);
            Assert.Throws<ArgumentNullException>(() => vector.Subtract(other, null));
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
            Assert.Throws<ArgumentException>(() => vector.Subtract(other, result));
        }

        /// <summary>
        /// Subtraction operator throws <c>ArgumentNullException</c> if a vector is <c>null</c>.
        /// </summary>
        [Test]
        public void SubtractionOperatorIfAVectorIsNullThrowsArgumentNullException()
        {
            Vector<Complex> a = null;
            var b = CreateVector(Data.Length);
            Assert.Throws<ArgumentNullException>(() => a -= b);

            a = b;
            b = null;
            Assert.Throws<ArgumentNullException>(() => a -= b);
        }

        /// <summary>
        /// Subtraction operator throws <c>ArgumentException</c> if vectors are different size.
        /// </summary>
        [Test]
        public void SubtractionOperatorIfVectorsAreDifferentSizeThrowsArgumentException()
        {
            var a = CreateVector(Data.Length);
            var b = CreateVector(Data.Length + 1);
            Assert.Throws<ArgumentException>(() => a -= b);
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
                Assert.AreEqual(Complex.Zero, vector[i]);
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
                Assert.AreEqual(Complex.Zero, result[i]);
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
                Assert.AreEqual(Complex.Zero, result[i]);
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
                Assert.AreEqual(Complex.Zero, vector[i]);
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
                Assert.AreEqual(Complex.Zero, result[i]);
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
                Assert.AreEqual(Complex.Zero, vector[i]);
            }
        }

        /// <summary>
        /// Can divide a vector by a scalar.
        /// </summary>
        [Test]
        public void CanDivideVectorByScalar()
        {
            var copy = CreateVector(Data);
            var vector = copy.Divide(2.0);

            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(Data[i] / 2.0, vector[i]);
            }

            vector.Divide(1.0);
            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(Data[i] / 2.0, vector[i]);
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
            vector.Divide(2.0, result);

            CollectionAssert.AreEqual(Data, vector, "Making sure the original vector wasn't modified.");
            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(Data[i] / 2.0, result[i]);
            }

            vector.Divide(1.0, result);
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
            var vector = copy.Multiply(2.0);

            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(Data[i] * 2.0, vector[i]);
            }

            vector.Multiply(1.0);
            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(Data[i] * 2.0, vector[i]);
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
            vector.Multiply(2.0, result);

            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(Data[i], vector[i], "Making sure the original vector wasn't modified.");
                Assert.AreEqual(Data[i] * 2.0, result[i]);
            }

            vector.Multiply(1.0, result);
            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(Data[i], result[i]);
            }
        }

        /// <summary>
        /// Multiplying by scalar with <c>null</c> result vector throws <c>ArgumentNullException</c>.
        /// </summary>
        [Test]
        public void MultiplyingScalarWithNullResultVectorThrowsArgumentNullException()
        {
            var vector = CreateVector(Data.Length);
            Assert.Throws<ArgumentNullException>(() => vector.Multiply(1.0, null));
        }

        /// <summary>
        /// Dividing by scalar with <c>null</c> result vector throws <c>ArgumentNullException</c>.
        /// </summary>
        [Test]
        public void DividingScalarWithNullResultVectorThrowsArgumentNullException()
        {
            var vector = CreateVector(Data.Length);
            Assert.Throws<ArgumentNullException>(() => vector.Divide(1.0, null));
        }

        /// <summary>
        /// Multiplying by scalar with wrong result vector size throws <c>ArgumentException</c>.
        /// </summary>
        [Test]
        public void MultiplyingScalarWithWrongSizeResultVectorThrowsArgumentException()
        {
            var vector = CreateVector(Data.Length);
            var result = CreateVector(Data.Length + 1);
            Assert.Throws<ArgumentException>(() => vector.Multiply(0.0, result));
        }

        /// <summary>
        /// Dividing by scalar with wrong result vector size throws <c>ArgumentException</c>.
        /// </summary>
        [Test]
        public void DividingScalarWithWrongSizeResultVectorThrowsArgumentException()
        {
            var vector = CreateVector(Data.Length);
            var result = CreateVector(Data.Length + 1);
            Assert.Throws<ArgumentException>(() => vector.Divide(0.0, result));
        }

        /// <summary>
        /// Can multiply a vector by scalar using operators.
        /// </summary>
        [Test]
        public void CanMultiplyVectorByScalarUsingOperators()
        {
            var vector = CreateVector(Data);
            vector = vector * 2.0;

            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(Data[i] * 2.0, vector[i]);
            }

            vector = vector * 1.0;
            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(Data[i] * 2.0, vector[i]);
            }

            vector = CreateVector(Data);
            vector = 2.0 * vector;

            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(Data[i] * 2.0, vector[i]);
            }

            vector = 1.0 * vector;
            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(Data[i] * 2.0, vector[i]);
            }
        }

        /// <summary>
        /// Can divide a vector by scalar using operators.
        /// </summary>
        [Test]
        public void CanDivideVectorByScalarUsingOperators()
        {
            var vector = CreateVector(Data);
            vector = vector / 2.0;

            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(Data[i] / 2.0, vector[i]);
            }

            vector = vector / 1.0;
            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(Data[i] / 2.0, vector[i]);
            }
        }

        /// <summary>
        /// Operator multiply throws <c>ArgumentNullException</c> when a vector is <c>null</c>.
        /// </summary>
        [Test]
        public void OperatorMultiplyWhenVectorIsNullThrowsArgumentNullException()
        {
            Vector<Complex> vector = null;
            Vector<Complex> result = null;
            Assert.Throws<ArgumentNullException>(() => result = vector * 2.0);
            Assert.Throws<ArgumentNullException>(() => result = 2.0 * vector);
        }

        /// <summary>
        /// Operator divide throws <c>ArgumentNullException</c> when a vector is <c>null</c>.
        /// </summary>
        [Test]
        public void OperatorDivideWhenVectorIsNullThrowsArgumentNullException()
        {
            Vector<Complex> vector = null;
            Assert.Throws<ArgumentNullException>(() => vector = vector / 2.0);
        }

        /// <summary>
        /// Can calculate the dot product
        /// </summary>
        [Test]
        public void CanDotProduct()
        {
            var dataA = CreateVector(Data);
            var dataB = CreateVector(Data);

            Assert.AreEqual(new Complex(50, 30), dataA.DotProduct(dataB));
        }

        /// <summary>
        /// Dot product throws <c>ArgumentNullException</c> when an argument is null
        /// </summary>
        [Test]
        public void DotProductWhenArgumentIsNullThrowsArgumentNullException()
        {
            var dataA = CreateVector(Data);
            Vector<Complex> dataB = null;

            Assert.Throws<ArgumentNullException>(() => dataA.DotProduct(dataB));
        }

        /// <summary>
        /// Dot product throws <c>ArgumentException</c> when an argument has different size
        /// </summary>
        [Test]
        public void DotProductWhenDifferentSizeThrowsArgumentException()
        {
            var dataA = CreateVector(Data);
            var dataB = CreateVector(new[] { new Complex(1, 1), new Complex(2, 1), new Complex(3, 1), new Complex(4, 1), new Complex(5, 1), new Complex(6, 1) });

            Assert.Throws<ArgumentException>(() => dataA.DotProduct(dataB));
        }

        /// <summary>
        /// Can calculate the dot product using "*" operator.
        /// </summary>
        [Test]
        public void CanDotProductUsingOperator()
        {
            var dataA = CreateVector(Data);
            var dataB = CreateVector(Data);

            Assert.AreEqual(new Complex(50, 30), dataA * dataB);
        }

        /// <summary>
        /// Operator "*" throws <c>ArgumentNullException</c> when the left argument is <c>null</c>.
        /// </summary>
        [Test]
        public void OperatorDotProductWhenLeftArgumentIsNullThrowsArgumentNullException()
        {
            var dataA = CreateVector(Data);
            Vector<Complex> dataB = null;

            Assert.Throws<ArgumentNullException>(() => { var d = dataA * dataB; });
        }

        /// <summary>
        /// Operator "*" throws <c>ArgumentNullException</c> when the right argument is <c>null</c>.
        /// </summary>
        [Test]
        public void OperatorDotProductWhenRightArgumentIsNullThrowsArgumentNullException()
        {
            Vector<Complex> dataA = null;
            var dataB = CreateVector(Data);

            Assert.Throws<ArgumentNullException>(() => { var d = dataA * dataB; });
        }

        /// <summary>
        /// Operator "*" throws <c>ArgumentException</c> when the argument has different size.
        /// </summary>
        [Test]
        public void OperatorDotProductWhenDifferentSizeThrowsArgumentException()
        {
            var dataA = CreateVector(Data);
            var dataB = CreateVector(new Complex[] { 1, 2, 3, 4, 5, 6 });

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
        /// Pointwise multiply with <c>null</c> throws <c>ArgumentNullException</c>.
        /// </summary>
        [Test]
        public void PointwiseMultiplyWithNullThrowsArgumentNullException()
        {
            var vector1 = CreateVector(Data);
            Vector<Complex> vector2 = null;
            var result = CreateVector(vector1.Count);
            Assert.Throws<ArgumentNullException>(() => vector1.PointwiseMultiply(vector2, result));
        }

        /// <summary>
        /// Pointwise multiply with a result vector is <c>null</c> throws <c>ArgumentNullException</c>.
        /// </summary>
        [Test]
        public void PointwiseMultiplyWithResultNullThrowsArgumentNullException()
        {
            var vector1 = CreateVector(Data);
            var vector2 = vector1.Clone();
            Vector<Complex> result = null;
            Assert.Throws<ArgumentNullException>(() => vector1.PointwiseMultiply(vector2, result));
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
            Assert.Throws<ArgumentException>(() => vector1.PointwiseMultiply(vector2, result));
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
        /// Pointwise divide with <c>null</c> throws <c>ArgumentNullException</c>.
        /// </summary>
        [Test]
        public void PointwiseDivideWithNullThrowsArgumentNullException()
        {
            var vector1 = CreateVector(Data);
            Vector<Complex> vector2 = null;
            var result = CreateVector(vector1.Count);
            Assert.Throws<ArgumentNullException>(() => vector1.PointwiseDivide(vector2, result));
        }

        /// <summary>
        /// Pointwise divide with a result vector is <c>null</c> throws <c>ArgumentNullException</c>.
        /// </summary>
        [Test]
        public void PointwiseDivideWithResultNullThrowsArgumentNullException()
        {
            var vector1 = CreateVector(Data);
            var vector2 = vector1.Clone();
            Vector<Complex> result = null;
            Assert.Throws<ArgumentNullException>(() => vector1.PointwiseDivide(vector2, result));
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
            Assert.Throws<ArgumentException>(() => vector1.PointwiseDivide(vector2, result));
        }

        /// <summary>
        /// Can calculate the outer product of two vectors.
        /// </summary>
        [Test]
        public void CanCalculateOuterProduct()
        {
            var vector1 = CreateVector(Data);
            var vector2 = CreateVector(Data);
            var m = Vector<Complex>.OuterProduct(vector1, vector2);
            for (var i = 0; i < vector1.Count; i++)
            {
                for (var j = 0; j < vector2.Count; j++)
                {
                    Assert.AreEqual(m[i, j], vector1[i] * vector2[j]);
                }
            }
        }

        /// <summary>
        /// Outer product with <c>null</c> first parameter throws <c>ArgumentNullException</c>.
        /// </summary>
        [Test]
        public void OuterProductWhenFirstIsNullThrowsArgumentNullException()
        {
            Vector<Complex> vector1 = null;
            var vector2 = CreateVector(Data);
            Assert.Throws<ArgumentNullException>(() => Vector<Complex>.OuterProduct(vector1, vector2));
        }

        /// <summary>
        /// Outer product with <c>null</c> second parameter throws <c>ArgumentNullException</c>.
        /// </summary>
        [Test]
        public void OuterProductWhenSecondIsNullThrowsArgumentNullException()
        {
            var vector1 = CreateVector(Data);
            Vector<Complex> vector2 = null;
            Assert.Throws<ArgumentNullException>(() => Vector<Complex>.OuterProduct(vector1, vector2));
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

        /// <summary>
        /// Tensor multiply with <c>null</c> parameter throws <c>ArgumentNullException</c>.
        /// </summary>
        [Test]
        public void TensorMultiplyWithNullThrowsArgumentNullException()
        {
            var vector1 = CreateVector(Data);
            Vector<Complex> vector2 = null;
            Assert.Throws<ArgumentNullException>(() => vector1.OuterProduct(vector2));
        }
    }
}
