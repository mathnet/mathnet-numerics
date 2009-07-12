// <copyright file="PrecisionTest.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://mathnet.opensourcedotnet.info
//
// Copyright (c) 2009 Math.NET
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
using MbUnit.Framework;

namespace MathNet.Numerics.UnitTests
{
    [TestFixture]
    public sealed class PrecisionTest
    {
        private const double mAcceptableError = 1e-12;
        private readonly double mDoublePrecision = System.Math.Pow(2, -53);

        [Test]
        public void Magnitude()
        {
            Assert.AreEqual(0, Precision.Magnitude(0));

            Assert.AreEqual(0, Precision.Magnitude(1));
            Assert.AreEqual(1, Precision.Magnitude(10));
            Assert.AreEqual(2, Precision.Magnitude(100));
            Assert.AreEqual(3, Precision.Magnitude(1000));
            Assert.AreEqual(4, Precision.Magnitude(10000));
            Assert.AreEqual(5, Precision.Magnitude(100000));
            Assert.AreEqual(6, Precision.Magnitude(1000000));

            Assert.AreEqual(7, Precision.Magnitude(1e7));
            Assert.AreEqual(8, Precision.Magnitude(1e8));
            Assert.AreEqual(9, Precision.Magnitude(1e9));
            Assert.AreEqual(10, Precision.Magnitude(1e10));
            Assert.AreEqual(11, Precision.Magnitude(1e11));
            Assert.AreEqual(12, Precision.Magnitude(1e12));

            Assert.AreEqual(5, Precision.Magnitude(1.1e5));
            Assert.AreEqual(-5, Precision.Magnitude(2.2e-5));
            Assert.AreEqual(9, Precision.Magnitude(3.3e9));
            Assert.AreEqual(-11, Precision.Magnitude(4.4e-11));
        }

        // Bug fix test: Magnitude of negative numbers returns zero
        [Test]
        public void MagnitudeWithNegativeValues()
        {
            Assert.AreEqual(0, Precision.Magnitude(-1));
            Assert.AreEqual(1, Precision.Magnitude(-10));
            Assert.AreEqual(2, Precision.Magnitude(-100));
            Assert.AreEqual(3, Precision.Magnitude(-1000));
            Assert.AreEqual(4, Precision.Magnitude(-10000));
            Assert.AreEqual(5, Precision.Magnitude(-100000));
            Assert.AreEqual(6, Precision.Magnitude(-1000000));

            Assert.AreEqual(7, Precision.Magnitude(-1e7));
            Assert.AreEqual(8, Precision.Magnitude(-1e8));
            Assert.AreEqual(9, Precision.Magnitude(-1e9));
            Assert.AreEqual(10, Precision.Magnitude(-1e10));
            Assert.AreEqual(11, Precision.Magnitude(-1e11));
            Assert.AreEqual(12, Precision.Magnitude(-1e12));

            Assert.AreEqual(5, Precision.Magnitude(-1.1e5));
            Assert.AreEqual(-5, Precision.Magnitude(-2.2e-5));
            Assert.AreEqual(9, Precision.Magnitude(-3.3e9));
            Assert.AreEqual(-11, Precision.Magnitude(-4.4e-11));
        }

        [Test]
        public void Value()
        {
            Assert.AreApproximatelyEqual<double,double>(0, Precision.Value(0), mAcceptableError);

            Assert.AreApproximatelyEqual<double, double>(1, Precision.Value(1), mAcceptableError);
            Assert.AreApproximatelyEqual<double, double>(1, Precision.Value(10), mAcceptableError);
            Assert.AreApproximatelyEqual<double, double>(1, Precision.Value(100), mAcceptableError);
            Assert.AreApproximatelyEqual<double, double>(1, Precision.Value(1000), mAcceptableError);
            Assert.AreApproximatelyEqual<double, double>(1, Precision.Value(10000), mAcceptableError);
            Assert.AreApproximatelyEqual<double, double>(1, Precision.Value(100000), mAcceptableError);
            Assert.AreApproximatelyEqual<double, double>(1, Precision.Value(1000000), mAcceptableError);

            Assert.AreApproximatelyEqual<double,double>(1.1, Precision.Value(1.1e5), mAcceptableError);
            Assert.AreApproximatelyEqual<double,double>(2.2, Precision.Value(2.2e-5), mAcceptableError);
            Assert.AreApproximatelyEqual<double, double>(3.3, Precision.Value(3.3e9), mAcceptableError);
            Assert.AreApproximatelyEqual<double, double>(4.4, Precision.Value(4.4e-11), mAcceptableError);
        }

        // Bug fix test: Magnitude of negative numbers returns zero
        [Test]
        public void ValueWithNegativeValues()
        {
            Assert.AreApproximatelyEqual<double, double>(0, Precision.Value(0), mAcceptableError);

            Assert.AreApproximatelyEqual<double, double>(-1, Precision.Value(-1), mAcceptableError);
            Assert.AreApproximatelyEqual<double, double>(-1, Precision.Value(-10), mAcceptableError);
            Assert.AreApproximatelyEqual<double, double>(-1, Precision.Value(-100), mAcceptableError);
            Assert.AreApproximatelyEqual<double, double>(-1, Precision.Value(-1000), mAcceptableError);
            Assert.AreApproximatelyEqual<double, double>(-1, Precision.Value(-10000), mAcceptableError);
            Assert.AreApproximatelyEqual<double, double>(-1, Precision.Value(-100000), mAcceptableError);
            Assert.AreApproximatelyEqual<double, double>(-1, Precision.Value(-1000000), mAcceptableError);

            Assert.AreApproximatelyEqual<double, double>(-1.1, Precision.Value(-1.1e5), mAcceptableError);
            Assert.AreApproximatelyEqual<double, double>(-2.2, Precision.Value(-2.2e-5), mAcceptableError);
            Assert.AreApproximatelyEqual<double, double>(-3.3, Precision.Value(-3.3e9), mAcceptableError);
            Assert.AreApproximatelyEqual<double, double>(-4.4, Precision.Value(-4.4e-11), mAcceptableError);
        }

        [Test]
        public void IncrementAtZero()
        {
            double x = -2 * double.Epsilon;
            Assert.AreEqual<double>(-2 * double.Epsilon, x);
            x = Precision.Increment(x);
            x = Precision.Increment(x);
            Assert.AreEqual<double>(0.0, x);
            x = Precision.Increment(x);
            x = Precision.Increment(x);
            Assert.AreEqual<double>(2 * double.Epsilon, x);
        }

        [Test]
        public void DecrementAtZero()
        {
            double x = 2 * double.Epsilon;
            Assert.AreEqual<double>(2 * double.Epsilon, x);
            x = Precision.Decrement(x);
            x = Precision.Decrement(x);
            Assert.AreEqual<double>(0.0, x);
            x = Precision.Decrement(x);
            x = Precision.Decrement(x);
            Assert.AreEqual<double>(-2 * double.Epsilon, x);
        }

        [Test]
        public void IncrementAtMinMax()
        {
            double x = double.MaxValue;
            x = Precision.Increment(x);
            Assert.AreEqual<double>(double.PositiveInfinity,x);

            x = double.MinValue;
            Assert.AreEqual<double>(double.MinValue, x);
            x = Precision.Increment(x);
            Assert.GreaterThan(x, double.MinValue);
        }

        [Test]
        public void DecrementAtMinMax()
        {
            double x = double.MaxValue;
            Assert.AreEqual<double>(double.MaxValue, x);
            x = Precision.Decrement(x);
            Assert.GreaterThan(double.MaxValue, x);

            x = double.MinValue;
            Assert.AreEqual<double>(double.MinValue, x);
            x = Precision.Decrement(x);
            Assert.AreEqual<double>(double.NegativeInfinity, x);
        }

        [Test]
        public void CoerceZero()
        {
            Assert.AreEqual<double>(0.0, Precision.CoerceZero(0d));
            Assert.AreEqual<double>(0.0, Precision.CoerceZero(Precision.Increment(0.0)));
            Assert.AreEqual<double>(0.0, Precision.CoerceZero(Precision.Decrement(0.0)));

            Assert.AreEqual<double>(1.0, Precision.CoerceZero(1d));
            Assert.AreEqual<double>(-1.0, Precision.CoerceZero(-1d));
            Assert.AreEqual<double>(0.5, Precision.CoerceZero(0.5d));
            Assert.AreEqual<double>(-0.5, Precision.CoerceZero(-0.5d));

            Assert.AreEqual<double>(double.PositiveInfinity, Precision.CoerceZero(double.PositiveInfinity));
            Assert.AreEqual<double>(double.NegativeInfinity, Precision.CoerceZero(double.NegativeInfinity));
            Assert.AreEqual<double>(double.MaxValue, Precision.CoerceZero(double.MaxValue));
            Assert.AreEqual<double>(double.MinValue, Precision.CoerceZero(double.MinValue));
            Assert.AreEqual<double>(double.NaN, Precision.CoerceZero(double.NaN));
        }

        [Test]
        public void CoerceZeroBasedOnMaxNumbersBetween()
        {
            Assert.AreEqual<double>(0.0, Precision.CoerceZero(5 * double.Epsilon, 5));
            Assert.AreEqual<double>(0.0, Precision.CoerceZero(-5 * double.Epsilon, 5));

            Assert.AreNotEqual<double>(0.0, Precision.CoerceZero(5 * double.Epsilon, 4));
            Assert.AreNotEqual<double>(0.0, Precision.CoerceZero(-5 * double.Epsilon, 4));
        }

        [Test]
        public void CoerceZeroBasedOnRelativeTolerance()
        {
            Assert.AreEqual<double>(0.0, Precision.CoerceZero(1e-6, 1e-5));
            Assert.AreEqual<double>(0.0, Precision.CoerceZero(-1e-6, 1e-5));
            Assert.AreEqual<double>(0.0, Precision.CoerceZero(1e+4, 1e+5));
            Assert.AreEqual<double>(0.0, Precision.CoerceZero(-1e+4, 1e+5));

            Assert.AreNotEqual<double>(0.0, Precision.CoerceZero(1e-4, 1e-5));
            Assert.AreNotEqual<double>(0.0, Precision.CoerceZero(1e-5, 1e-5));
            Assert.AreNotEqual<double>(0.0, Precision.CoerceZero(-1e-4, 1e-5));
            Assert.AreNotEqual<double>(0.0, Precision.CoerceZero(-1e-5, 1e-5));
            Assert.AreNotEqual<double>(0.0, Precision.CoerceZero(1e+6, 1e+5));
            Assert.AreNotEqual<double>(0.0, Precision.CoerceZero(1e+5, 1e+5));
            Assert.AreNotEqual<double>(0.0, Precision.CoerceZero(-1e+6, 1e+5));
            Assert.AreNotEqual<double>(0.0, Precision.CoerceZero(-1e+5, 1e+5));
        }

        [Test]
        public void RangeOfMatchingFloatingPointNumbersWithNegativeUlps()
        {
            double max;
            double min;
            Assert.Throws<ArgumentOutOfRangeException>(() => Precision.RangeOfMatchingFloatingPointNumbers(10, -1, out min, out max));
        }

        [Test]
        public void RangeOfMatchingFloatingPointNumbersWithValueAtInfinity()
        {
            double max;
            double min;

            Precision.RangeOfMatchingFloatingPointNumbers(double.PositiveInfinity, 1, out min, out max);
            Assert.AreEqual(double.PositiveInfinity, min);
            Assert.AreEqual(double.PositiveInfinity, max);

            Precision.RangeOfMatchingFloatingPointNumbers(double.NegativeInfinity, 1, out min, out max);
            Assert.AreEqual(double.NegativeInfinity, min);
            Assert.AreEqual(double.NegativeInfinity, max);
        }

        [Test]
        public void RangeOfMatchingFloatingPointNumbersWithValueAtNaN()
        {
            double max;
            double min;

            Precision.RangeOfMatchingFloatingPointNumbers(double.NaN, 1, out min, out max);
            Assert.IsTrue(double.IsNaN(min));
            Assert.IsTrue(double.IsNaN(max));
        }

        // Numbers are calculated with the UlpsCalculator program which can be found in UlpsCalculator.cs
        [Row(0, -10 * double.Epsilon, 10 * double.Epsilon)]
        [Row(1.0, 0.99999999999999889, 1.0000000000000022)]
        [Row(2.0, 1.9999999999999978, 2.0000000000000044)]
        [Row(3.0, 2.9999999999999956, 3.0000000000000044)]
        [Row(4.0, 3.9999999999999956, 4.0000000000000089)]
        [Row(5.0, 4.9999999999999911, 5.0000000000000089)]
        [Row(6.0, 5.9999999999999911, 6.0000000000000089)]
        [Row(7.0, 6.9999999999999911, 7.0000000000000089)]
        [Row(8.0, 7.9999999999999911, 8.0000000000000178)]
        public void RangeOfMatchingFloatingPointNumbersWithPositiveValue(double input, double expectedMin, double expectedMax)
        {
            double max;
            double min;

            Precision.RangeOfMatchingFloatingPointNumbers(input, 10, out min, out max);
            Assert.AreEqual(expectedMin, min);
            Assert.AreEqual(expectedMax, max);
        }

        // Numbers are calculated with the UlpsCalculator program which can be found in UlpsCalculator.cs
        [Row(0, -10 * double.Epsilon, 10 * double.Epsilon)]
        [Row(-1.0, -1.0000000000000022, -0.99999999999999889)]
        [Row(-2.0, -2.0000000000000044, -1.9999999999999978)]
        [Row(-3.0, -3.0000000000000044, -2.9999999999999956)]
        [Row(-4.0, -4.0000000000000089, -3.9999999999999956)]
        [Row(-5.0, -5.0000000000000089, -4.9999999999999911)]
        [Row(-6.0, -6.0000000000000089, -5.9999999999999911)]
        [Row(-7.0, -7.0000000000000089, -6.9999999999999911)]
        [Row(-8.0, -8.0000000000000178, -7.9999999999999911)]
        public void RangeOfMatchingFloatingPointNumbersWithNegativeValue(double input, double expectedMin, double expectedMax)
        {
            double max;
            double min;

            Precision.RangeOfMatchingFloatingPointNumbers(input, 10, out min, out max);
            Assert.AreEqual(expectedMin, min);
            Assert.AreEqual(expectedMax, max);
        }

        [Row(0, 10 * double.Epsilon)]
        [Row(1.0, 1.0000000000000022)]
        [Row(2.0, 2.0000000000000044)]
        [Row(3.0, 3.0000000000000044)]
        [Row(4.0, 4.0000000000000089)]
        [Row(5.0, 5.0000000000000089)]
        [Row(6.0, 6.0000000000000089)]
        [Row(7.0, 7.0000000000000089)]
        [Row(8.0, 8.0000000000000178)]
        public void MaximumMatchingFloatingPointNumberWithPositiveValue(double input, double expectedMax)
        {
            double max = Precision.MaximumMatchingFloatingPointNumber(input, 10);
            Assert.AreEqual(expectedMax, max);
        }

        [Row(0, 10 * double.Epsilon)]
        [Row(-1.0, -0.99999999999999889)]
        [Row(-2.0, -1.9999999999999978)]
        [Row(-3.0, -2.9999999999999956)]
        [Row(-4.0, -3.9999999999999956)]
        [Row(-5.0, -4.9999999999999911)]
        [Row(-6.0, -5.9999999999999911)]
        [Row(-7.0, -6.9999999999999911)]
        [Row(-8.0, -7.9999999999999911)]
        public void MaximumMatchingFloatingPointNumberWithNegativeValue(double input, double expectedMax)
        {
            double max = Precision.MaximumMatchingFloatingPointNumber(input, 10);
            Assert.AreEqual(expectedMax, max);
        }

        [Row(0, -10 * double.Epsilon)]
        [Row(1.0, 0.99999999999999889)]
        [Row(2.0, 1.9999999999999978)]
        [Row(3.0, 2.9999999999999956)]
        [Row(4.0, 3.9999999999999956)]
        [Row(5.0, 4.9999999999999911)]
        [Row(6.0, 5.9999999999999911)]
        [Row(7.0, 6.9999999999999911)]
        [Row(8.0, 7.9999999999999911)]
        public void MinimumMatchingFloatingPointNumberWithPositiveValue(double input, double expectedMin)
        {
            double min = Precision.MinimumMatchingFloatingPointNumber(input, 10);
            Assert.AreEqual(expectedMin, min);
        }

        [Row(0, -10 * double.Epsilon)]
        [Row(-1.0, -1.0000000000000022)]
        [Row(-2.0, -2.0000000000000044)]
        [Row(-3.0, -3.0000000000000044)]
        [Row(-4.0, -4.0000000000000089)]
        [Row(-5.0, -5.0000000000000089)]
        [Row(-6.0, -6.0000000000000089)]
        [Row(-7.0, -7.0000000000000089)]
        [Row(-8.0, -8.0000000000000178)]
        public void MinimumMatchingFloatingPointNumberWithNegativeValue(double input, double expectedMin)
        {
            double min = Precision.MinimumMatchingFloatingPointNumber(input, 10);
            Assert.AreEqual(expectedMin, min);
        }

        [Test]
        public void RangeOfMatchingUlpsWithNegativeRelativeDifference()
        {
            long min;
            long max;
            Assert.Throws<ArgumentOutOfRangeException>(() => Precision.RangeOfMatchingNumbers(1, -1, out min, out max));
        }

        [Test]
        public void RangeOfMatchingUlpsWithValueAtInfinity()
        {
            long min;
            long max;
            Assert.Throws<ArgumentOutOfRangeException>(() => Precision.RangeOfMatchingNumbers(double.PositiveInfinity, -1, out min, out max));
        }

        [Test]
        public void RangeOfMatchingUlpsWithValueAtNaN()
        {
            long min;
            long max;
            Assert.Throws<ArgumentOutOfRangeException>(() => Precision.RangeOfMatchingNumbers(double.NaN, -1, out min, out max));
        }

        [Row(0, 10 * double.Epsilon, 10, 10)]
        [Row(1.0, (1.0000000000000022 - 1.0) / 1.0, 20, 10)]
        [Row(2.0, (2.0000000000000044 - 2.0) / 2.0, 20, 10)]
        [Row(3.0, (3.0000000000000044 - 3.0) / 3.0, 10, 10)]
        [Row(4.0, (4.0000000000000089 - 4.0) / 4.0, 20, 10)]
        [Row(5.0, (5.0000000000000089 - 5.0) / 5.0, 10, 10)]
        [Row(6.0, (6.0000000000000089 - 6.0) / 6.0, 10, 10)]
        [Row(7.0, (7.0000000000000089 - 7.0) / 7.0, 10, 10)]
        [Row(8.0, (8.0000000000000178 - 8.0) / 8.0, 20, 10)]
        public void RangeOfMatchingUlpsWithPositiveValue(double input, double relativeDifference, long expectedMin, long expectedMax)
        {
            long min;
            long max;

            Precision.RangeOfMatchingNumbers(input, relativeDifference, out min, out max);
            Assert.AreEqual(expectedMin, min);
            Assert.AreEqual(expectedMax, max);
        }

        [Row(0, 10 * double.Epsilon, 10, 10)]
        [Row(-1.0, (1.0000000000000022 - 1.0) / 1.0, 10, 20)]
        [Row(-2.0, (2.0000000000000044 - 2.0) / 2.0, 10, 20)]
        [Row(-3.0, (3.0000000000000044 - 3.0) / 3.0, 10, 10)]
        [Row(-4.0, (4.0000000000000089 - 4.0) / 4.0, 10, 20)]
        [Row(-5.0, (5.0000000000000089 - 5.0) / 5.0, 10, 10)]
        [Row(-6.0, (6.0000000000000089 - 6.0) / 6.0, 10, 10)]
        [Row(-7.0, (7.0000000000000089 - 7.0) / 7.0, 10, 10)]
        [Row(-8.0, (8.0000000000000178 - 8.0) / 8.0, 10, 20)]
        public void RangeOfMatchingUlpsWithNegativeValue(double input, double relativeDifference, long expectedMin, long expectedMax)
        {
            long min;
            long max;

            Precision.RangeOfMatchingNumbers(input, relativeDifference, out min, out max);
            Assert.AreEqual(expectedMin, min);
            Assert.AreEqual(expectedMax, max);
        }

        [Test]
        public void TestNumbersBetween()
        {
            Assert.AreEqual<ulong>(0, Precision.NumbersBetween(1.0, 1.0));
            Assert.AreEqual<ulong>(0, Precision.NumbersBetween(0, 0));
            Assert.AreEqual<ulong>(0, Precision.NumbersBetween(-1.0, -1.0));

            Assert.AreEqual<ulong>(1, Precision.NumbersBetween(0, double.Epsilon));
            Assert.AreEqual<ulong>(1, Precision.NumbersBetween(0, -double.Epsilon));
            Assert.AreEqual<ulong>(1, Precision.NumbersBetween(double.Epsilon, 0));
            Assert.AreEqual<ulong>(1, Precision.NumbersBetween(-double.Epsilon, 0));

            Assert.AreEqual<ulong>(2, Precision.NumbersBetween(0, 2 * double.Epsilon));
            Assert.AreEqual<ulong>(2, Precision.NumbersBetween(0, -2 * double.Epsilon));

            Assert.AreEqual<ulong>(3, Precision.NumbersBetween(-double.Epsilon, 2 * double.Epsilon));
            Assert.AreEqual<ulong>(3, Precision.NumbersBetween(double.Epsilon, -2 * double.Epsilon));
        }

        // AlmostZero
        [Test]
        public void AlmostZeroWithMaxNumbersBetween()
        {
            Assert.IsTrue(Precision.AlmostZero(0, 1));
            Assert.IsTrue(Precision.AlmostZero(1 * double.Epsilon, 1));
            Assert.IsTrue(Precision.AlmostZero(10 * double.Epsilon, 10));

            Assert.IsFalse(Precision.AlmostZero(double.NegativeInfinity, 1));
            Assert.IsFalse(Precision.AlmostZero(double.PositiveInfinity, 1));
            Assert.IsFalse(Precision.AlmostZero(double.NaN, 1));
        }

        [Test]
        public void AlmostZeroWithTolerance()
        {
            Assert.IsTrue(Precision.AlmostZero(0));
            Assert.IsTrue(Precision.AlmostZero(1 * double.Epsilon, 2 * double.Epsilon));
            Assert.IsTrue(Precision.AlmostZero(10 * double.Epsilon, 20 * double.Epsilon));

            Assert.IsFalse(Precision.AlmostZero(double.NegativeInfinity, 1.0));
            Assert.IsFalse(Precision.AlmostZero(double.PositiveInfinity, 1.0));
            Assert.IsFalse(Precision.AlmostZero(double.NaN, 1.0));
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void AlmostEqualWithMaxNumbersBetweenWithLessThanOneNumber()
        {
            Precision.AlmostEqual(1, 2, 0);
        }

        [Test]
        public void AlmostEqualWithMaxNumbersBetween()
        {
            // compare zero and negative zero
            Assert.IsTrue(Precision.AlmostEqual(0, -0, 1));

            // compare two nearby numbers
            Assert.IsFalse(Precision.AlmostEqual(1.0, 1.0 + 3 * mDoublePrecision, 1));
            Assert.IsTrue(Precision.AlmostEqual(1.0, 1.0 + mDoublePrecision, 1));
            Assert.IsTrue(Precision.AlmostEqual(1.0, 1.0 - mDoublePrecision, 1));
            Assert.IsFalse(Precision.AlmostEqual(1.0, 1.0 - 3 * mDoublePrecision, 1));

            // compare with the two numbers reversed in compare order
            Assert.IsFalse(Precision.AlmostEqual(1.0 + 3 * mDoublePrecision, 1.0, 1));
            Assert.IsTrue(Precision.AlmostEqual(1.0 + mDoublePrecision, 1.0, 1));
            Assert.IsTrue(Precision.AlmostEqual(1.0 - mDoublePrecision, 1.0, 1));
            Assert.IsFalse(Precision.AlmostEqual(1.0 - 3 * mDoublePrecision, 1.0, 1));

            // compare two slightly more different numbers
            Assert.IsFalse(Precision.AlmostEqual(1.0, 1.0 + 10 * mDoublePrecision, 1));
            Assert.IsTrue(Precision.AlmostEqual(1.0, 1.0 + 10 * mDoublePrecision, 10));
            Assert.IsTrue(Precision.AlmostEqual(1.0, 1.0 - 10 * mDoublePrecision, 10));
            Assert.IsFalse(Precision.AlmostEqual(1.0, 1.0 - 10 * mDoublePrecision, 1));

            // compare different numbers
            Assert.IsFalse(Precision.AlmostEqual(2.0, 1.0, 1));
            Assert.IsFalse(Precision.AlmostEqual(1.0, 2.0, 1));

            // compare different numbers with large tolerance
            Assert.IsFalse(Precision.AlmostEqual(1.0, 1.0 + 1e5 * mDoublePrecision, 1));
            Assert.IsTrue(Precision.AlmostEqual(1.0, 1.0 - 1e5 * mDoublePrecision, 200000));
            Assert.IsFalse(Precision.AlmostEqual(1.0, 1.0 - 1e5 * mDoublePrecision, 1));

            // compare inf & inf
            Assert.IsTrue(Precision.AlmostEqual(double.PositiveInfinity, double.PositiveInfinity, 1));
            Assert.IsTrue(Precision.AlmostEqual(double.NegativeInfinity, double.NegativeInfinity, 1));

            // compare -inf and inf
            Assert.IsFalse(Precision.AlmostEqual(double.PositiveInfinity, double.NegativeInfinity, 1));
            Assert.IsFalse(Precision.AlmostEqual(double.NegativeInfinity, double.PositiveInfinity, 1));

            // compare inf and non-inf
            Assert.IsFalse(Precision.AlmostEqual(double.PositiveInfinity, 1.0, 1));
            Assert.IsFalse(Precision.AlmostEqual(1.0, double.PositiveInfinity, 1));

            Assert.IsFalse(Precision.AlmostEqual(double.NegativeInfinity, 1.0, 1));
            Assert.IsFalse(Precision.AlmostEqual(1.0, double.NegativeInfinity, 1));

            // compare tiny numbers with opposite signs
            Assert.IsFalse(Precision.AlmostEqual(double.Epsilon, -double.Epsilon, 1));
            Assert.IsFalse(Precision.AlmostEqual(-double.Epsilon, double.Epsilon, 1));

            Assert.IsFalse(Precision.AlmostEqual(-2.0, 2.0, 1));
            Assert.IsFalse(Precision.AlmostEqual(2.0, -2.0, 1));
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void AlmostEqualWithinDecimalPlacesWithDecimalPlacesLessThanOne()
        {
            Precision.AlmostEqualInDecimalPlaces(1, 2, 0);
        }

        [Test]
        public void AlmostEqualWithinDecimalPlaces()
        {
            Assert.IsFalse(Precision.AlmostEqualInDecimalPlaces(1, double.NaN, 2));
            Assert.IsFalse(Precision.AlmostEqualInDecimalPlaces(double.NaN, 2, 2));
            Assert.IsFalse(Precision.AlmostEqualInDecimalPlaces(double.NaN, double.NaN, 2));

            Assert.IsFalse(Precision.AlmostEqualInDecimalPlaces(double.NegativeInfinity, 2, 2));
            Assert.IsFalse(Precision.AlmostEqualInDecimalPlaces(1, double.NegativeInfinity, 2));

            Assert.IsFalse(Precision.AlmostEqualInDecimalPlaces(double.PositiveInfinity, 2, 2));
            Assert.IsFalse(Precision.AlmostEqualInDecimalPlaces(1, double.PositiveInfinity, 2));

            Assert.IsFalse(Precision.AlmostEqualInDecimalPlaces(double.NegativeInfinity, double.PositiveInfinity, 2));
            Assert.IsFalse(Precision.AlmostEqualInDecimalPlaces(double.PositiveInfinity, double.NegativeInfinity, 2));

            Assert.IsTrue(Precision.AlmostEqualInDecimalPlaces(double.PositiveInfinity, double.PositiveInfinity, 2));
            Assert.IsTrue(Precision.AlmostEqualInDecimalPlaces(double.NegativeInfinity, double.NegativeInfinity, 2));

            Assert.IsTrue(Precision.AlmostEqualInDecimalPlaces(1.0, 1.04, 2));
            Assert.IsTrue(Precision.AlmostEqualInDecimalPlaces(1.0, 0.96, 2));

            Assert.IsFalse(Precision.AlmostEqualInDecimalPlaces(1.0, 1.06, 2));
            Assert.IsFalse(Precision.AlmostEqualInDecimalPlaces(1.0, 0.94, 2));

            Assert.IsTrue(Precision.AlmostEqualInDecimalPlaces(100.0, 104.00, 2));
            Assert.IsTrue(Precision.AlmostEqualInDecimalPlaces(100.0, 96.000, 2));

            Assert.IsFalse(Precision.AlmostEqualInDecimalPlaces(100.0, 106.00, 2));
            Assert.IsFalse(Precision.AlmostEqualInDecimalPlaces(100.0, 94.000, 2));

            Assert.IsTrue(Precision.AlmostEqualInDecimalPlaces(0.0, 4 * mDoublePrecision, 12));
            Assert.IsTrue(Precision.AlmostEqualInDecimalPlaces(0.0, -4 * mDoublePrecision, 12));
        }

        [Test]
        public void AlmostEqualWithSmallNumbersAndSmallNumberOfDecimalPlaces()
        {
            Assert.IsTrue(Precision.AlmostEqualInDecimalPlaces(0.0, 1e-12, 1));
            Assert.IsTrue(Precision.AlmostEqualInDecimalPlaces(0.0, -1e-12, 1));

            Assert.IsFalse(Precision.AlmostEqualInDecimalPlaces(0.0, 1e-12, 13));
            Assert.IsFalse(Precision.AlmostEqualInDecimalPlaces(0.0, -1e-12, 13));

            Assert.IsFalse(Precision.AlmostEqualInDecimalPlaces(0.0, 1, 1));
            Assert.IsFalse(Precision.AlmostEqualInDecimalPlaces(0.0, -1, 1));
        }

        [Test]
        public void AlmostEqualWithDecimalPlacesWithSignRevert()
        {
            Assert.IsFalse(Precision.AlmostEqualInDecimalPlaces(0.5, 0.3, 1));
        }

        [Test]
        public void IsLargerWithMaxNumbersBetween()
        {
            // compare zero and negative zero
            Assert.IsFalse(Precision.IsLarger(0, -0, 1));

            // compare two nearby numbers
            Assert.IsFalse(Precision.IsLarger(1.0, 1.0 + 3 * mDoublePrecision, 1));
            Assert.IsFalse(Precision.IsLarger(1.0, 1.0 + mDoublePrecision, 1));
            Assert.IsFalse(Precision.IsLarger(1.0, 1.0 - mDoublePrecision, 1));
            Assert.IsTrue(Precision.IsLarger(1.0, 1.0 - 3 * mDoublePrecision, 1));

            // compare with the two numbers reversed in compare order
            Assert.IsTrue(Precision.IsLarger(1.0 + 3 * mDoublePrecision, 1.0, 1));
            Assert.IsFalse(Precision.IsLarger(1.0 + mDoublePrecision, 1.0, 1));
            Assert.IsFalse(Precision.IsLarger(1.0 - mDoublePrecision, 1.0, 1));
            Assert.IsFalse(Precision.IsLarger(1.0 - 3 * mDoublePrecision, 1.0, 1));

            // compare two slightly more different numbers
            Assert.IsFalse(Precision.IsLarger(1.0, 1.0 + 10 * mDoublePrecision, 1));
            Assert.IsFalse(Precision.IsLarger(1.0, 1.0 + 10 * mDoublePrecision, 10));
            Assert.IsFalse(Precision.IsLarger(1.0, 1.0 - 10 * mDoublePrecision, 10));
            Assert.IsTrue(Precision.IsLarger(1.0, 1.0 - 10 * mDoublePrecision, 1));

            // compare different numbers
            Assert.IsTrue(Precision.IsLarger(2.0, 1.0, 1));
            Assert.IsFalse(Precision.IsLarger(1.0, 2.0, 1));

            // compare different numbers with large tolerance
            Assert.IsFalse(Precision.IsLarger(1.0, 1.0 + 1e5 * mDoublePrecision, 1));
            Assert.IsFalse(Precision.IsLarger(1.0, 1.0 - 1e5 * mDoublePrecision, 200000));
            Assert.IsTrue(Precision.IsLarger(1.0, 1.0 - 1e5 * mDoublePrecision, 1));

            // compare inf & inf
            Assert.IsFalse(Precision.IsLarger(double.PositiveInfinity, double.PositiveInfinity, 1));
            Assert.IsFalse(Precision.IsLarger(double.NegativeInfinity, double.NegativeInfinity, 1));

            // compare -inf and inf
            Assert.IsTrue(Precision.IsLarger(double.PositiveInfinity, double.NegativeInfinity, 1));
            Assert.IsFalse(Precision.IsLarger(double.NegativeInfinity, double.PositiveInfinity, 1));

            // compare inf and non-inf
            Assert.IsTrue(Precision.IsLarger(double.PositiveInfinity, 1.0, 1));
            Assert.IsFalse(Precision.IsLarger(1.0, double.PositiveInfinity, 1));

            Assert.IsFalse(Precision.IsLarger(double.NegativeInfinity, 1.0, 1));
            Assert.IsTrue(Precision.IsLarger(1.0, double.NegativeInfinity, 1));

            // compare tiny numbers with opposite signs
            Assert.IsTrue(Precision.IsLarger(double.Epsilon, -double.Epsilon, 1));
            Assert.IsFalse(Precision.IsLarger(-double.Epsilon, double.Epsilon, 1));
        }

        [Test]
        public void IsLargerWithDecimalPlaces()
        {
            Assert.IsFalse(Precision.IsLargerWithDecimalPlaces(1, double.NaN, 2));
            Assert.IsFalse(Precision.IsLargerWithDecimalPlaces(double.NaN, 2, 2));
            Assert.IsFalse(Precision.IsLargerWithDecimalPlaces(double.NaN, double.NaN, 2));

            Assert.IsFalse(Precision.IsLargerWithDecimalPlaces(double.NegativeInfinity, 2, 2));
            Assert.IsTrue(Precision.IsLargerWithDecimalPlaces(1, double.NegativeInfinity, 2));

            Assert.IsTrue(Precision.IsLargerWithDecimalPlaces(double.PositiveInfinity, 2, 2));
            Assert.IsFalse(Precision.IsLargerWithDecimalPlaces(1, double.PositiveInfinity, 2));

            Assert.IsFalse(Precision.IsLargerWithDecimalPlaces(double.NegativeInfinity, double.PositiveInfinity, 2));
            Assert.IsTrue(Precision.IsLargerWithDecimalPlaces(double.PositiveInfinity, double.NegativeInfinity, 2));

            Assert.IsFalse(Precision.IsLargerWithDecimalPlaces(double.PositiveInfinity, double.PositiveInfinity, 2));
            Assert.IsFalse(Precision.IsLargerWithDecimalPlaces(double.NegativeInfinity, double.NegativeInfinity, 2));

            Assert.IsFalse(Precision.IsLargerWithDecimalPlaces(1.0, 1.04, 2));
            Assert.IsFalse(Precision.IsLargerWithDecimalPlaces(1.0, 0.96, 2));

            Assert.IsFalse(Precision.IsLargerWithDecimalPlaces(1.0, 1.06, 2));
            Assert.IsTrue(Precision.IsLargerWithDecimalPlaces(1.0, 0.94, 2));

            Assert.IsFalse(Precision.IsLargerWithDecimalPlaces(100.0, 104.00, 2));
            Assert.IsFalse(Precision.IsLargerWithDecimalPlaces(100.0, 96.000, 2));

            Assert.IsFalse(Precision.IsLargerWithDecimalPlaces(100.0, 106.00, 2));
            Assert.IsTrue(Precision.IsLargerWithDecimalPlaces(100.0, 94.000, 2));

            double max = 4 * System.Math.Pow(10, Precision.Magnitude(mDoublePrecision));
            Assert.IsFalse(Precision.IsLargerWithDecimalPlaces(0.0, max, -Precision.Magnitude(mDoublePrecision)));
            Assert.IsFalse(Precision.IsLargerWithDecimalPlaces(0.0, -max, -Precision.Magnitude(mDoublePrecision)));

            max = 6 * System.Math.Pow(10, Precision.Magnitude(mDoublePrecision));
            Assert.IsFalse(Precision.IsLargerWithDecimalPlaces(0.0, max, -Precision.Magnitude(mDoublePrecision)));
            Assert.IsTrue(Precision.IsLargerWithDecimalPlaces(0.0, -max, -Precision.Magnitude(mDoublePrecision)));
        }

        [Test]
        public void IsSmallerWithMaxNumbersBetween()
        {
            // compare zero and negative zero
            Assert.IsFalse(Precision.IsSmaller(0, -0, 1));

            // compare two nearby numbers
            Assert.IsTrue(Precision.IsSmaller(1.0, 1.0 + 3 * mDoublePrecision, 1));
            Assert.IsFalse(Precision.IsSmaller(1.0, 1.0 + mDoublePrecision, 1));
            Assert.IsFalse(Precision.IsSmaller(1.0, 1.0 - mDoublePrecision, 1));
            Assert.IsFalse(Precision.IsSmaller(1.0, 1.0 - 3 * mDoublePrecision, 1));

            // compare with the two numbers reversed in compare order
            Assert.IsFalse(Precision.IsSmaller(1.0 + 3 * mDoublePrecision, 1.0, 1));
            Assert.IsFalse(Precision.IsSmaller(1.0 + mDoublePrecision, 1.0, 1));
            Assert.IsFalse(Precision.IsSmaller(1.0 - mDoublePrecision, 1.0, 1));
            Assert.IsTrue(Precision.IsSmaller(1.0 - 3 * mDoublePrecision, 1.0, 1));

            // compare two slightly more different numbers
            Assert.IsTrue(Precision.IsSmaller(1.0, 1.0 + 10 * mDoublePrecision, 1));
            Assert.IsFalse(Precision.IsSmaller(1.0, 1.0 + 10 * mDoublePrecision, 10));
            Assert.IsFalse(Precision.IsSmaller(1.0, 1.0 - 10 * mDoublePrecision, 10));
            Assert.IsFalse(Precision.IsSmaller(1.0, 1.0 - 10 * mDoublePrecision, 1));

            // compare different numbers
            Assert.IsFalse(Precision.IsSmaller(2.0, 1.0, 1));
            Assert.IsTrue(Precision.IsSmaller(1.0, 2.0, 1));

            // compare different numbers with large tolerance
            Assert.IsTrue(Precision.IsSmaller(1.0, 1.0 + 1e5 * mDoublePrecision, 1));
            Assert.IsFalse(Precision.IsSmaller(1.0, 1.0 - 1e5 * mDoublePrecision, 200000));
            Assert.IsFalse(Precision.IsSmaller(1.0, 1.0 - 1e5 * mDoublePrecision, 1));

            // compare inf & inf
            Assert.IsFalse(Precision.IsSmaller(double.PositiveInfinity, double.PositiveInfinity, 1));
            Assert.IsFalse(Precision.IsSmaller(double.NegativeInfinity, double.NegativeInfinity, 1));

            // compare -inf and inf
            Assert.IsFalse(Precision.IsSmaller(double.PositiveInfinity, double.NegativeInfinity, 1));
            Assert.IsTrue(Precision.IsSmaller(double.NegativeInfinity, double.PositiveInfinity, 1));

            // compare inf and non-inf
            Assert.IsFalse(Precision.IsSmaller(double.PositiveInfinity, 1.0, 1));
            Assert.IsTrue(Precision.IsSmaller(1.0, double.PositiveInfinity, 1));

            Assert.IsTrue(Precision.IsSmaller(double.NegativeInfinity, 1.0, 1));
            Assert.IsFalse(Precision.IsSmaller(1.0, double.NegativeInfinity, 1));

            // compare tiny numbers with opposite signs
            Assert.IsFalse(Precision.IsSmaller(double.Epsilon, -double.Epsilon, 1));
            Assert.IsTrue(Precision.IsSmaller(-double.Epsilon, double.Epsilon, 1));
        }

        [Test]
        public void IsSmallerWithDecimalPlaces()
        {
            Assert.IsFalse(Precision.IsSmallerWithDecimalPlaces(1, double.NaN, 2));
            Assert.IsFalse(Precision.IsSmallerWithDecimalPlaces(double.NaN, 2, 2));
            Assert.IsFalse(Precision.IsSmallerWithDecimalPlaces(double.NaN, double.NaN, 2));

            Assert.IsTrue(Precision.IsSmallerWithDecimalPlaces(double.NegativeInfinity, 2, 2));
            Assert.IsFalse(Precision.IsSmallerWithDecimalPlaces(1, double.NegativeInfinity, 2));

            Assert.IsFalse(Precision.IsSmallerWithDecimalPlaces(double.PositiveInfinity, 2, 2));
            Assert.IsTrue(Precision.IsSmallerWithDecimalPlaces(1, double.PositiveInfinity, 2));

            Assert.IsTrue(Precision.IsSmallerWithDecimalPlaces(double.NegativeInfinity, double.PositiveInfinity, 2));
            Assert.IsFalse(Precision.IsSmallerWithDecimalPlaces(double.PositiveInfinity, double.NegativeInfinity, 2));

            Assert.IsFalse(Precision.IsSmallerWithDecimalPlaces(double.PositiveInfinity, double.PositiveInfinity, 2));
            Assert.IsFalse(Precision.IsSmallerWithDecimalPlaces(double.NegativeInfinity, double.NegativeInfinity, 2));

            Assert.IsFalse(Precision.IsSmallerWithDecimalPlaces(1.0, 1.04, 2));
            Assert.IsFalse(Precision.IsSmallerWithDecimalPlaces(1.0, 0.96, 2));

            Assert.IsTrue(Precision.IsSmallerWithDecimalPlaces(1.0, 1.06, 2));
            Assert.IsFalse(Precision.IsSmallerWithDecimalPlaces(1.0, 0.94, 2));

            Assert.IsFalse(Precision.IsSmallerWithDecimalPlaces(100.0, 104.00, 2));
            Assert.IsFalse(Precision.IsSmallerWithDecimalPlaces(100.0, 96.000, 2));

            Assert.IsTrue(Precision.IsSmallerWithDecimalPlaces(100.0, 106.00, 2));
            Assert.IsFalse(Precision.IsSmallerWithDecimalPlaces(100.0, 94.000, 2));

            double max = 4 * System.Math.Pow(10, Precision.Magnitude(mDoublePrecision));
            Assert.IsFalse(Precision.IsSmallerWithDecimalPlaces(0.0, max, -Precision.Magnitude(mDoublePrecision)));
            Assert.IsFalse(Precision.IsSmallerWithDecimalPlaces(0.0, -max, -Precision.Magnitude(mDoublePrecision)));

            max = 6 * System.Math.Pow(10, Precision.Magnitude(mDoublePrecision));
            Assert.IsTrue(Precision.IsSmallerWithDecimalPlaces(0.0, max, -Precision.Magnitude(mDoublePrecision)));
            Assert.IsFalse(Precision.IsSmallerWithDecimalPlaces(0.0, -max, -Precision.Magnitude(mDoublePrecision)));
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void CompareToWithMaxNumbersBetweenWithNegativeNumber()
        {
            double value = 10.0;
            Precision.CompareTo(value, value, -1);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void CompareToWithMaxNumbersBetweenWithZeroNumber()
        {
            double value = 10.0;
            Precision.CompareTo(value, value, 0);
        }

        [Test]
        public void CompareToWithMaxNumbersBetweenWithInfinityValue()
        {
            Assert.AreEqual(0, Precision.CompareTo(double.PositiveInfinity, double.PositiveInfinity, 1));
            Assert.AreEqual(0, Precision.CompareTo(double.NegativeInfinity, double.NegativeInfinity, 1));

            Assert.AreEqual(1, Precision.CompareTo(double.PositiveInfinity, double.NegativeInfinity, 1));
            Assert.AreEqual(-1, Precision.CompareTo(double.NegativeInfinity, double.PositiveInfinity, 1));

            Assert.AreEqual(-1, Precision.CompareTo(1, double.PositiveInfinity, 1));
            Assert.AreEqual(1, Precision.CompareTo(double.PositiveInfinity, 1, 1));

            Assert.AreEqual(1, Precision.CompareTo(1, double.NegativeInfinity, 1));
            Assert.AreEqual(-1, Precision.CompareTo(double.NegativeInfinity, 1, 1));
        }

        [Test]
        public void CompareToWithMaxNumbersBetweenWithNaNValue()
        {
            // compare nan & not-nan
            Assert.AreEqual(1, Precision.CompareTo(1, double.NaN, 1));
            Assert.AreEqual(0, Precision.CompareTo(double.NaN, double.NaN, 1));
            Assert.AreEqual(-1, Precision.CompareTo(double.NaN, 1, 1));
        }

        // Bug fix test: In the old system comparing -2 and 2 creates a long value that is equal
        // to long.MinValue and you can't get the absolute value of that because long.MinValue is 
        // 1 bigger than long.MaxValue

        [Test]
        public void CompareToWithMaxNumbersBetweenForValuesThatOverFlowALong()
        {
            Assert.AreEqual(1, Precision.CompareTo(2.0, -2.0, 1));
            Assert.AreEqual(-1, Precision.CompareTo(-2.0, 2.0, 1));
        }

        [Test]
        public void CompareToWithMaxNumbersBetween()
        {
            // compare zero and negative zero
            Assert.AreEqual(0, Precision.CompareTo(0, -0, 1));

            // compare two nearby numbers
            Assert.AreEqual(-1, Precision.CompareTo(1.0, 1.0 + 3 * mDoublePrecision, 1));
            Assert.AreEqual(0, Precision.CompareTo(1.0, 1.0 + mDoublePrecision, 1));
            Assert.AreEqual(0, Precision.CompareTo(1.0, 1.0 - mDoublePrecision, 1));
            Assert.AreEqual(1, Precision.CompareTo(1.0, 1.0 - 3 * mDoublePrecision, 1));

            // compare with the two numbers reversed in compare order
            Assert.AreEqual(1, Precision.CompareTo(1.0 + 3 * mDoublePrecision, 1.0, 1));
            Assert.AreEqual(0, Precision.CompareTo(1.0 + mDoublePrecision, 1.0, 1));
            Assert.AreEqual(0, Precision.CompareTo(1.0 - mDoublePrecision, 1.0, 1));
            Assert.AreEqual(-1, Precision.CompareTo(1.0 - 3 * mDoublePrecision, 1.0, 1));

            // compare two slightly more different numbers
            Assert.AreEqual(-1, Precision.CompareTo(1.0, 1.0 + 10 * mDoublePrecision, 1));
            Assert.AreEqual(0, Precision.CompareTo(1.0, 1.0 + 10 * mDoublePrecision, 10));
            Assert.AreEqual(0, Precision.CompareTo(1.0, 1.0 - 10 * mDoublePrecision, 10));
            Assert.AreEqual(1, Precision.CompareTo(1.0, 1.0 - 10 * mDoublePrecision, 1));

            // compare different numbers
            Assert.AreEqual(1, Precision.CompareTo(2.0, 1.0, 1));
            Assert.AreEqual(-1, Precision.CompareTo(1.0, 2.0, 1));

            // compare different numbers with large tolerance
            Assert.AreEqual(-1, Precision.CompareTo(1.0, 1.0 + 1e5 * mDoublePrecision, 1));
            Assert.AreEqual(0, Precision.CompareTo(1.0, 1.0 - 1e5 * mDoublePrecision, 200000));
            Assert.AreEqual(1, Precision.CompareTo(1.0, 1.0 - 1e5 * mDoublePrecision, 1));

            // compare inf & inf
            Assert.AreEqual(0, Precision.CompareTo(double.PositiveInfinity, double.PositiveInfinity, 1));
            Assert.AreEqual(0, Precision.CompareTo(double.NegativeInfinity, double.NegativeInfinity, 1));

            // compare -inf and inf
            Assert.AreEqual(1, Precision.CompareTo(double.PositiveInfinity, double.NegativeInfinity, 1));
            Assert.AreEqual(-1, Precision.CompareTo(double.NegativeInfinity, double.PositiveInfinity, 1));

            // compare inf and non-inf
            Assert.AreEqual(1, Precision.CompareTo(double.PositiveInfinity, 1.0, 1));
            Assert.AreEqual(-1, Precision.CompareTo(1.0, double.PositiveInfinity, 1));

            Assert.AreEqual(-1, Precision.CompareTo(double.NegativeInfinity, 1.0, 1));
            Assert.AreEqual(1, Precision.CompareTo(1.0, double.NegativeInfinity, 1));

            // compare tiny numbers with opposite signs
            Assert.AreEqual(1, Precision.CompareTo(double.Epsilon, -double.Epsilon, 1));
            Assert.AreEqual(-1, Precision.CompareTo(-double.Epsilon, double.Epsilon, 1));
        }

        [Test]
        public void CompareToWithDecimalPlaces()
        {
            // compare zero and negative zero
            Assert.AreEqual(0, Precision.CompareToInDecimalPlaces(0, -0, 1));
            Assert.AreEqual(0, Precision.CompareToInDecimalPlaces(0, -0, Precision.NumberOfDecimalPlacesForDoubles));
            Assert.AreEqual(0, Precision.CompareToInDecimalPlaces(0, -0, Precision.NumberOfDecimalPlacesForFloats));

            // compare two nearby numbers
            Assert.AreEqual(-1, Precision.CompareToInDecimalPlaces(1.0, 1.0 + 10 * mDoublePrecision, Precision.NumberOfDecimalPlacesForDoubles));
            Assert.AreEqual(0, Precision.CompareToInDecimalPlaces(1.0, 1.0 + mDoublePrecision, Precision.NumberOfDecimalPlacesForDoubles));
            Assert.AreEqual(0, Precision.CompareToInDecimalPlaces(1.0, 1.0 - mDoublePrecision, Precision.NumberOfDecimalPlacesForDoubles));
            Assert.AreEqual(1, Precision.CompareToInDecimalPlaces(1.0, 1.0 - 10 * mDoublePrecision, Precision.NumberOfDecimalPlacesForDoubles));

            // compare with the two numbers reversed in compare order
            Assert.AreEqual(1, Precision.CompareToInDecimalPlaces(1.0 + 10 * mDoublePrecision, 1.0, Precision.NumberOfDecimalPlacesForDoubles));
            Assert.AreEqual(0, Precision.CompareToInDecimalPlaces(1.0 + mDoublePrecision, 1.0, Precision.NumberOfDecimalPlacesForDoubles));
            Assert.AreEqual(0, Precision.CompareToInDecimalPlaces(1.0 - mDoublePrecision, 1.0, Precision.NumberOfDecimalPlacesForDoubles));
            Assert.AreEqual(-1, Precision.CompareToInDecimalPlaces(1.0 - 10 * mDoublePrecision, 1.0, Precision.NumberOfDecimalPlacesForDoubles));

            // compare two slightly more different numbers
            Assert.AreEqual(-1, Precision.CompareToInDecimalPlaces(1.0, 1.0 + 50 * mDoublePrecision, Precision.NumberOfDecimalPlacesForDoubles));
            Assert.AreEqual(0, Precision.CompareToInDecimalPlaces(1.0, 1.0 + 50 * mDoublePrecision, Precision.NumberOfDecimalPlacesForDoubles - 2));
            Assert.AreEqual(0, Precision.CompareToInDecimalPlaces(1.0, 1.0 - 50 * mDoublePrecision, Precision.NumberOfDecimalPlacesForDoubles - 2));
            Assert.AreEqual(1, Precision.CompareToInDecimalPlaces(1.0, 1.0 - 50 * mDoublePrecision, Precision.NumberOfDecimalPlacesForDoubles));

            // compare different numbers
            Assert.AreEqual(1, Precision.CompareToInDecimalPlaces(2.0, 1.0, Precision.NumberOfDecimalPlacesForDoubles));
            Assert.AreEqual(-1, Precision.CompareToInDecimalPlaces(1.0, 2.0, Precision.NumberOfDecimalPlacesForDoubles));

            // compare different numbers with large tolerance
            Assert.AreEqual(-1, Precision.CompareToInDecimalPlaces(1.0, 1.0 + 1e5 * mDoublePrecision, Precision.NumberOfDecimalPlacesForDoubles));
            Assert.AreEqual(0, Precision.CompareToInDecimalPlaces(1.0, 1.0 - 1e5 * mDoublePrecision, 10));
            Assert.AreEqual(1, Precision.CompareToInDecimalPlaces(1.0, 1.0 - 1e5 * mDoublePrecision, Precision.NumberOfDecimalPlacesForDoubles));

            // compare inf & inf
            Assert.AreEqual(0, Precision.CompareToInDecimalPlaces(double.PositiveInfinity, double.PositiveInfinity, Precision.NumberOfDecimalPlacesForDoubles));
            Assert.AreEqual(0, Precision.CompareToInDecimalPlaces(double.NegativeInfinity, double.NegativeInfinity, Precision.NumberOfDecimalPlacesForDoubles));

            // compare -inf and inf
            Assert.AreEqual(1, Precision.CompareToInDecimalPlaces(double.PositiveInfinity, double.NegativeInfinity, Precision.NumberOfDecimalPlacesForDoubles));
            Assert.AreEqual(-1, Precision.CompareToInDecimalPlaces(double.NegativeInfinity, double.PositiveInfinity, Precision.NumberOfDecimalPlacesForDoubles));

            // compare inf and non-inf
            Assert.AreEqual(1, Precision.CompareToInDecimalPlaces(double.PositiveInfinity, 1.0, Precision.NumberOfDecimalPlacesForDoubles));
            Assert.AreEqual(-1, Precision.CompareToInDecimalPlaces(1.0, double.PositiveInfinity, Precision.NumberOfDecimalPlacesForDoubles));

            Assert.AreEqual(-1, Precision.CompareToInDecimalPlaces(double.NegativeInfinity, 1.0, Precision.NumberOfDecimalPlacesForDoubles));
            Assert.AreEqual(1, Precision.CompareToInDecimalPlaces(1.0, double.NegativeInfinity, Precision.NumberOfDecimalPlacesForDoubles));
        }
    }
}
