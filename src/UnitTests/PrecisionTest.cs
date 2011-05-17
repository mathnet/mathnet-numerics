// <copyright file="PrecisionTest.cs" company="Math.NET">
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

namespace MathNet.Numerics.UnitTests
{
    using System;
    using NUnit.Framework;

    /// <summary>
    /// Precision tests.
    /// </summary>
    [TestFixture]
    public sealed class PrecisionTest
    {
        /// <summary>
        /// Acceptable error.
        /// </summary>
        private const double AcceptableError = 1e-12;

        /// <summary>
        /// Double precision.
        /// </summary>
        private readonly double _doublePrecision = Math.Pow(2, -53);

        /// <summary>
        /// Magnitude tests.
        /// </summary>
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

            Assert.AreEqual(7, 1e7.Magnitude());
            Assert.AreEqual(8, 1e8.Magnitude());
            Assert.AreEqual(9, 1e9.Magnitude());
            Assert.AreEqual(10, 1e10.Magnitude());
            Assert.AreEqual(11, 1e11.Magnitude());
            Assert.AreEqual(12, 1e12.Magnitude());

            Assert.AreEqual(5, 1.1e5.Magnitude());
            Assert.AreEqual(-5, 2.2e-5.Magnitude());
            Assert.AreEqual(9, 3.3e9.Magnitude());
            Assert.AreEqual(-11, 4.4e-11.Magnitude());
        }

        /// <summary>
        /// Magnitude with negative values.
        /// </summary>
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

            Assert.AreEqual(7, (-1e7).Magnitude());
            Assert.AreEqual(8, (-1e8).Magnitude());
            Assert.AreEqual(9, (-1e9).Magnitude());
            Assert.AreEqual(10, (-1e10).Magnitude());
            Assert.AreEqual(11, (-1e11).Magnitude());
            Assert.AreEqual(12, (-1e12).Magnitude());

            Assert.AreEqual(5, (-1.1e5).Magnitude());
            Assert.AreEqual(-5, (-2.2e-5).Magnitude());
            Assert.AreEqual(9, (-3.3e9).Magnitude());
            Assert.AreEqual(-11, (-4.4e-11).Magnitude());
        }

        /// <summary>
        /// Get magnitude scaled value.
        /// </summary>
        [Test]
        public void Value()
        {
            Assert.AreEqual(0, Precision.GetMagnitudeScaledValue(0), AcceptableError);

            Assert.AreEqual(1, Precision.GetMagnitudeScaledValue(1), AcceptableError);
            Assert.AreEqual(1, Precision.GetMagnitudeScaledValue(10), AcceptableError);
            Assert.AreEqual(1, Precision.GetMagnitudeScaledValue(100), AcceptableError);
            Assert.AreEqual(1, Precision.GetMagnitudeScaledValue(1000), AcceptableError);
            Assert.AreEqual(1, Precision.GetMagnitudeScaledValue(10000), AcceptableError);
            Assert.AreEqual(1, Precision.GetMagnitudeScaledValue(100000), AcceptableError);
            Assert.AreEqual(1, Precision.GetMagnitudeScaledValue(1000000), AcceptableError);

            Assert.AreEqual(1.1, 1.1e5.GetMagnitudeScaledValue(), AcceptableError);
            Assert.AreEqual(2.2, 2.2e-5.GetMagnitudeScaledValue(), AcceptableError);
            Assert.AreEqual(3.3, 3.3e9.GetMagnitudeScaledValue(), AcceptableError);
            Assert.AreEqual(4.4, 4.4e-11.GetMagnitudeScaledValue(), AcceptableError);
        }

        /// <summary>
        /// Can get magnitude scaled value with a negative values.
        /// </summary>
        [Test]
        public void ValueWithNegativeValues()
        {
            Assert.AreEqual(0, Precision.GetMagnitudeScaledValue(0), AcceptableError);

            Assert.AreEqual(-1, Precision.GetMagnitudeScaledValue(-1), AcceptableError);
            Assert.AreEqual(-1, Precision.GetMagnitudeScaledValue(-10), AcceptableError);
            Assert.AreEqual(-1, Precision.GetMagnitudeScaledValue(-100), AcceptableError);
            Assert.AreEqual(-1, Precision.GetMagnitudeScaledValue(-1000), AcceptableError);
            Assert.AreEqual(-1, Precision.GetMagnitudeScaledValue(-10000), AcceptableError);
            Assert.AreEqual(-1, Precision.GetMagnitudeScaledValue(-100000), AcceptableError);
            Assert.AreEqual(-1, Precision.GetMagnitudeScaledValue(-1000000), AcceptableError);

            Assert.AreEqual(-1.1, (-1.1e5).GetMagnitudeScaledValue(), AcceptableError);
            Assert.AreEqual(-2.2, (-2.2e-5).GetMagnitudeScaledValue(), AcceptableError);
            Assert.AreEqual(-3.3, (-3.3e9).GetMagnitudeScaledValue(), AcceptableError);
            Assert.AreEqual(-4.4, (-4.4e-11).GetMagnitudeScaledValue(), AcceptableError);
        }

        /// <summary>
        /// Increment at zero.
        /// </summary>
        [Test]
        public void IncrementAtZero()
        {
            var x = -2 * double.Epsilon;
            Assert.AreEqual(-2 * double.Epsilon, x);
            x = x.Increment();
            x = x.Increment();
            Assert.AreEqual(0.0, x);
            x = x.Increment();
            x = x.Increment();
            Assert.AreEqual(2 * double.Epsilon, x);
        }

        /// <summary>
        /// Decrement at zero.
        /// </summary>
        [Test]
        public void DecrementAtZero()
        {
            var x = 2 * double.Epsilon;
            Assert.AreEqual(2 * double.Epsilon, x);
            x = x.Decrement();
            x = x.Decrement();
            Assert.AreEqual(0.0, x);
            x = x.Decrement();
            x = x.Decrement();
            Assert.AreEqual(-2 * double.Epsilon, x);
        }

        /// <summary>
        /// Increment at min/max.
        /// </summary>
        [Test]
        public void IncrementAtMinMax()
        {
            var x = double.MaxValue;
            x = x.Increment();
            Assert.AreEqual(double.PositiveInfinity, x);

            x = double.MinValue;
            Assert.AreEqual(double.MinValue, x);
            x = x.Increment();
            Assert.Greater(x, double.MinValue);
        }

        /// <summary>
        /// Decrement at min/max.
        /// </summary>
        [Test]
        public void DecrementAtMinMax()
        {
            var x = double.MaxValue;
            Assert.AreEqual(double.MaxValue, x);
            x = x.Decrement();
            Assert.Greater(double.MaxValue, x);

            x = double.MinValue;
            Assert.AreEqual(double.MinValue, x);
            x = x.Decrement();
            Assert.AreEqual(double.NegativeInfinity, x);
        }

        /// <summary>
        /// Coerce zero.
        /// </summary>
        [Test]
        public void CoerceZero()
        {
            Assert.AreEqual(0.0, 0d.CoerceZero());
            Assert.AreEqual(0.0, 0.0.Increment().CoerceZero());
            Assert.AreEqual(0.0, 0.0.Decrement().CoerceZero());

            Assert.AreEqual(1.0, 1d.CoerceZero());
            Assert.AreEqual(-1.0, (-1d).CoerceZero());
            Assert.AreEqual(0.5, 0.5d.CoerceZero());
            Assert.AreEqual(-0.5, (-0.5d).CoerceZero());

            Assert.AreEqual(double.PositiveInfinity, double.PositiveInfinity.CoerceZero());
            Assert.AreEqual(double.NegativeInfinity, double.NegativeInfinity.CoerceZero());
            Assert.AreEqual(double.MaxValue, double.MaxValue.CoerceZero());
            Assert.AreEqual(double.MinValue, double.MinValue.CoerceZero());
            Assert.AreEqual(double.NaN, double.NaN.CoerceZero());
        }

        /// <summary>
        /// Coerce zero based on max numbers between.
        /// </summary>
        [Test]
        public void CoerceZeroBasedOnMaxNumbersBetween()
        {
            Assert.AreEqual(0.0, (5 * double.Epsilon).CoerceZero(5));
            Assert.AreEqual(0.0, (-5 * double.Epsilon).CoerceZero(5));

            Assert.AreNotEqual(0.0, (5 * double.Epsilon).CoerceZero(4));
            Assert.AreNotEqual(0.0, (-5 * double.Epsilon).CoerceZero(4));
        }

        /// <summary>
        /// Coerce zero based on relative tolerance.
        /// </summary>
        [Test]
        public void CoerceZeroBasedOnRelativeTolerance()
        {
            Assert.AreEqual(0.0, 1e-6.CoerceZero(1e-5));
            Assert.AreEqual(0.0, (-1e-6).CoerceZero(1e-5));
            Assert.AreEqual(0.0, 1e+4.CoerceZero(1e+5));
            Assert.AreEqual(0.0, (-1e+4).CoerceZero(1e+5));

            Assert.AreNotEqual(0.0, 1e-4.CoerceZero(1e-5));
            Assert.AreNotEqual(0.0, 1e-5.CoerceZero(1e-5));
            Assert.AreNotEqual(0.0, (-1e-4).CoerceZero(1e-5));
            Assert.AreNotEqual(0.0, (-1e-5).CoerceZero(1e-5));
            Assert.AreNotEqual(0.0, 1e+6.CoerceZero(1e+5));
            Assert.AreNotEqual(0.0, 1e+5.CoerceZero(1e+5));
            Assert.AreNotEqual(0.0, (-1e+6).CoerceZero(1e+5));
            Assert.AreNotEqual(0.0, (-1e+5).CoerceZero(1e+5));
        }

        /// <summary>
        /// Range of matching floating point numbers with negative Ulps.
        /// </summary>
        [Test]
        public void RangeOfMatchingFloatingPointNumbersWithNegativeUlps()
        {
            double max;
            double min;
            Assert.Throws<ArgumentOutOfRangeException>(() => Precision.RangeOfMatchingFloatingPointNumbers(10, -1, out min, out max));
        }

        /// <summary>
        /// Range of matching floating point numbers with value at infinity.
        /// </summary>
        [Test]
        public void RangeOfMatchingFloatingPointNumbersWithValueAtInfinity()
        {
            double max;
            double min;

            double.PositiveInfinity.RangeOfMatchingFloatingPointNumbers(1, out min, out max);
            Assert.AreEqual(double.PositiveInfinity, min);
            Assert.AreEqual(double.PositiveInfinity, max);

            double.NegativeInfinity.RangeOfMatchingFloatingPointNumbers(1, out min, out max);
            Assert.AreEqual(double.NegativeInfinity, min);
            Assert.AreEqual(double.NegativeInfinity, max);
        }

        /// <summary>
        /// Range of matching floating point numbers with value at NaN.
        /// </summary>
        [Test]
        public void RangeOfMatchingFloatingPointNumbersWithValueAtNaN()
        {
            double max;
            double min;

            double.NaN.RangeOfMatchingFloatingPointNumbers(1, out min, out max);
            Assert.IsTrue(double.IsNaN(min));
            Assert.IsTrue(double.IsNaN(max));
        }

        /// <summary>
        /// Range of matching floating point numbers with positive value.
        /// </summary>
        /// <param name="input">Input value.</param>
        /// <param name="expectedMin">Expected minimum.</param>
        /// <param name="expectedMax">Expected maximum.</param>
        /// <remarks>Numbers are calculated with the UlpsCalculator program which can be found in UlpsCalculator.cs</remarks>
        [TestCase(0, -10 * double.Epsilon, 10 * double.Epsilon)]
        [TestCase(1.0, 0.99999999999999889, 1.0000000000000022)]
        [TestCase(2.0, 1.9999999999999978, 2.0000000000000044)]
        [TestCase(3.0, 2.9999999999999956, 3.0000000000000044)]
        [TestCase(4.0, 3.9999999999999956, 4.0000000000000089)]
        [TestCase(5.0, 4.9999999999999911, 5.0000000000000089)]
        [TestCase(6.0, 5.9999999999999911, 6.0000000000000089)]
        [TestCase(7.0, 6.9999999999999911, 7.0000000000000089)]
        [TestCase(8.0, 7.9999999999999911, 8.0000000000000178)]
        public void RangeOfMatchingFloatingPointNumbersWithPositiveValue(double input, double expectedMin, double expectedMax)
        {
            double max;
            double min;

            input.RangeOfMatchingFloatingPointNumbers(10, out min, out max);
            Assert.AreEqual(expectedMin, min);
            Assert.AreEqual(expectedMax, max);
        }

        /// <summary>
        /// Range of matching floating point numbers with negative value.
        /// </summary>
        /// <param name="input">Input value.</param>
        /// <param name="expectedMin">Expected minimum.</param>
        /// <param name="expectedMax">Expected maximum.</param>
        /// <remarks>Numbers are calculated with the UlpsCalculator program which can be found in UlpsCalculator.cs</remarks>
        [TestCase(0, -10 * double.Epsilon, 10 * double.Epsilon)]
        [TestCase(-1.0, -1.0000000000000022, -0.99999999999999889)]
        [TestCase(-2.0, -2.0000000000000044, -1.9999999999999978)]
        [TestCase(-3.0, -3.0000000000000044, -2.9999999999999956)]
        [TestCase(-4.0, -4.0000000000000089, -3.9999999999999956)]
        [TestCase(-5.0, -5.0000000000000089, -4.9999999999999911)]
        [TestCase(-6.0, -6.0000000000000089, -5.9999999999999911)]
        [TestCase(-7.0, -7.0000000000000089, -6.9999999999999911)]
        [TestCase(-8.0, -8.0000000000000178, -7.9999999999999911)]
        public void RangeOfMatchingFloatingPointNumbersWithNegativeValue(double input, double expectedMin, double expectedMax)
        {
            double max;
            double min;

            input.RangeOfMatchingFloatingPointNumbers(10, out min, out max);
            Assert.AreEqual(expectedMin, min);
            Assert.AreEqual(expectedMax, max);
        }

        /// <summary>
        /// Maximum matching floating point number with positive value.
        /// </summary>
        /// <param name="input">Input value.</param>
        /// <param name="expectedMax">Expected maximum.</param>
        [TestCase(0, 10 * double.Epsilon)]
        [TestCase(1.0, 1.0000000000000022)]
        [TestCase(2.0, 2.0000000000000044)]
        [TestCase(3.0, 3.0000000000000044)]
        [TestCase(4.0, 4.0000000000000089)]
        [TestCase(5.0, 5.0000000000000089)]
        [TestCase(6.0, 6.0000000000000089)]
        [TestCase(7.0, 7.0000000000000089)]
        [TestCase(8.0, 8.0000000000000178)]
        public void MaximumMatchingFloatingPointNumberWithPositiveValue(double input, double expectedMax)
        {
            var max = input.MaximumMatchingFloatingPointNumber(10);
            Assert.AreEqual(expectedMax, max);
        }

        /// <summary>
        /// Maximum matching floating point number with negative value.
        /// </summary>
        /// <param name="input">Input value.</param>
        /// <param name="expectedMax">Expected maximum.</param>
        [TestCase(0, 10 * double.Epsilon)]
        [TestCase(-1.0, -0.99999999999999889)]
        [TestCase(-2.0, -1.9999999999999978)]
        [TestCase(-3.0, -2.9999999999999956)]
        [TestCase(-4.0, -3.9999999999999956)]
        [TestCase(-5.0, -4.9999999999999911)]
        [TestCase(-6.0, -5.9999999999999911)]
        [TestCase(-7.0, -6.9999999999999911)]
        [TestCase(-8.0, -7.9999999999999911)]
        public void MaximumMatchingFloatingPointNumberWithNegativeValue(double input, double expectedMax)
        {
            var max = input.MaximumMatchingFloatingPointNumber(10);
            Assert.AreEqual(expectedMax, max);
        }

        /// <summary>
        /// Minimum matching floating point number with positive value.
        /// </summary>
        /// <param name="input">Input value.</param>
        /// <param name="expectedMin">Expected minimum.</param>
        [TestCase(0, -10 * double.Epsilon)]
        [TestCase(1.0, 0.99999999999999889)]
        [TestCase(2.0, 1.9999999999999978)]
        [TestCase(3.0, 2.9999999999999956)]
        [TestCase(4.0, 3.9999999999999956)]
        [TestCase(5.0, 4.9999999999999911)]
        [TestCase(6.0, 5.9999999999999911)]
        [TestCase(7.0, 6.9999999999999911)]
        [TestCase(8.0, 7.9999999999999911)]
        public void MinimumMatchingFloatingPointNumberWithPositiveValue(double input, double expectedMin)
        {
            var min = input.MinimumMatchingFloatingPointNumber(10);
            Assert.AreEqual(expectedMin, min);
        }

        /// <summary>
        /// Minimum matching floating point number with negative value.
        /// </summary>
        /// <param name="input">Input value.</param>
        /// <param name="expectedMin">Expected minimum.</param>
        [TestCase(0, -10 * double.Epsilon)]
        [TestCase(-1.0, -1.0000000000000022)]
        [TestCase(-2.0, -2.0000000000000044)]
        [TestCase(-3.0, -3.0000000000000044)]
        [TestCase(-4.0, -4.0000000000000089)]
        [TestCase(-5.0, -5.0000000000000089)]
        [TestCase(-6.0, -6.0000000000000089)]
        [TestCase(-7.0, -7.0000000000000089)]
        [TestCase(-8.0, -8.0000000000000178)]
        public void MinimumMatchingFloatingPointNumberWithNegativeValue(double input, double expectedMin)
        {
            var min = input.MinimumMatchingFloatingPointNumber(10);
            Assert.AreEqual(expectedMin, min);
        }

        /// <summary>
        /// Range of matching Ulps with negative relative difference throws <c>ArgumentOutOfRangeException</c>.
        /// </summary>
        [Test]
        public void RangeOfMatchingUlpsWithNegativeRelativeDifferenceThrowsArgumentOutOfRangeException()
        {
            long min;
            long max;
            Assert.Throws<ArgumentOutOfRangeException>(() => Precision.RangeOfMatchingNumbers(1, -1, out min, out max));
        }

        /// <summary>
        /// Range of matching Ulps with value at infinity throws <c>ArgumentOutOfRangeException</c>.
        /// </summary>
        [Test]
        public void RangeOfMatchingUlpsWithValueAtInfinityThrowsArgumentOutOfRangeException()
        {
            long min;
            long max;
            Assert.Throws<ArgumentOutOfRangeException>(() => double.PositiveInfinity.RangeOfMatchingNumbers(-1, out min, out max));
        }

        /// <summary>
        /// Range of matching Ulps with value at Nan throws <c>ArgumentOutOfRangeException</c>.
        /// </summary>
        [Test]
        public void RangeOfMatchingUlpsWithValueAtNaNThrowsArgumentOutOfRangeException()
        {
            long min;
            long max;
            Assert.Throws<ArgumentOutOfRangeException>(() => double.NaN.RangeOfMatchingNumbers(-1, out min, out max));
        }

        /// <summary>
        /// Range of matching Ulps with positive value.
        /// </summary>
        /// <param name="input">Input value.</param>
        /// <param name="relativeDifference">Relative difference value.</param>
        /// <param name="expectedMin">Expected minimum.</param>
        /// <param name="expectedMax">Expected maximum.</param>
        [TestCase(0, 10 * double.Epsilon, 10, 10)]
        [TestCase(1.0, (1.0000000000000022 - 1.0) / 1.0, 20, 10)]
        [TestCase(2.0, (2.0000000000000044 - 2.0) / 2.0, 20, 10)]
        [TestCase(3.0, (3.0000000000000044 - 3.0) / 3.0, 10, 10)]
        [TestCase(4.0, (4.0000000000000089 - 4.0) / 4.0, 20, 10)]
        [TestCase(5.0, (5.0000000000000089 - 5.0) / 5.0, 10, 10)]
        [TestCase(6.0, (6.0000000000000089 - 6.0) / 6.0, 10, 10)]
        [TestCase(7.0, (7.0000000000000089 - 7.0) / 7.0, 10, 10)]
        [TestCase(8.0, (8.0000000000000178 - 8.0) / 8.0, 20, 10)]
        public void RangeOfMatchingUlpsWithPositiveValue(double input, double relativeDifference, long expectedMin, long expectedMax)
        {
            long min;
            long max;

            input.RangeOfMatchingNumbers(relativeDifference, out min, out max);
            Assert.AreEqual(expectedMin, min);
            Assert.AreEqual(expectedMax, max);
        }

        /// <summary>
        /// Range of matching Ulps with negative value.
        /// </summary>
        /// <param name="input">Input value.</param>
        /// <param name="relativeDifference">Relative difference value.</param>
        /// <param name="expectedMin">Expected minimum.</param>
        /// <param name="expectedMax">Expected maximum.</param>
        [TestCase(0, 10 * double.Epsilon, 10, 10)]
        [TestCase(-1.0, (1.0000000000000022 - 1.0) / 1.0, 10, 20)]
        [TestCase(-2.0, (2.0000000000000044 - 2.0) / 2.0, 10, 20)]
        [TestCase(-3.0, (3.0000000000000044 - 3.0) / 3.0, 10, 10)]
        [TestCase(-4.0, (4.0000000000000089 - 4.0) / 4.0, 10, 20)]
        [TestCase(-5.0, (5.0000000000000089 - 5.0) / 5.0, 10, 10)]
        [TestCase(-6.0, (6.0000000000000089 - 6.0) / 6.0, 10, 10)]
        [TestCase(-7.0, (7.0000000000000089 - 7.0) / 7.0, 10, 10)]
        [TestCase(-8.0, (8.0000000000000178 - 8.0) / 8.0, 10, 20)]
        public void RangeOfMatchingUlpsWithNegativeValue(double input, double relativeDifference, long expectedMin, long expectedMax)
        {
            long min;
            long max;

            input.RangeOfMatchingNumbers(relativeDifference, out min, out max);
            Assert.AreEqual(expectedMin, min);
            Assert.AreEqual(expectedMax, max);
        }

        /// <summary>
        /// Test numbers between.
        /// </summary>
        [Test]
        public void TestNumbersBetween()
        {
            Assert.AreEqual(0, 1.0.NumbersBetween(1.0));
            Assert.AreEqual(0, Precision.NumbersBetween(0, 0));
            Assert.AreEqual(0, (-1.0).NumbersBetween(-1.0));

            Assert.AreEqual(1, Precision.NumbersBetween(0, double.Epsilon));
            Assert.AreEqual(1, Precision.NumbersBetween(0, -double.Epsilon));
            Assert.AreEqual(1, double.Epsilon.NumbersBetween(0));
            Assert.AreEqual(1, (-double.Epsilon).NumbersBetween(0));

            Assert.AreEqual(2, Precision.NumbersBetween(0, 2 * double.Epsilon));
            Assert.AreEqual(2, Precision.NumbersBetween(0, -2 * double.Epsilon));

            Assert.AreEqual(3, (-double.Epsilon).NumbersBetween(2 * double.Epsilon));
            Assert.AreEqual(3, double.Epsilon.NumbersBetween(-2 * double.Epsilon));
        }

        /// <summary>
        /// <c>AlmostEqual</c> with max numbers between with less than one number throws <c>ArgumentOutOfRangeException</c>.
        /// </summary>
        [Test]
        public void AlmostEqualWithMaxNumbersBetweenWithLessThanOneNumberThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Precision.AlmostEqual(1, 2, 0));
        }

        /// <summary>
        /// Almost Equal.
        /// </summary>
        [Test]
        public void AlmostEqual()
        {
            Assert.That(1.0.AlmostEqual(1.0), "1.0 equals 1.0.");
            Assert.That(1.0.AlmostEqual(1.0 + _doublePrecision), "1.0 equals 1.0 + 2^(-53).");
            Assert.That(1.0.AlmostEqual(1.0 + (_doublePrecision * 10)), "1.0 equals 1.0 + 2^(-52).");
            Assert.That(!1.0.AlmostEqual(1.0 + (_doublePrecision * 100)), "1.0 does not equal 1.0 + 2^(-51).");
            Assert.That(!1.0.AlmostEqual(2.0), "1.0 does not equal 2.0");
        }

        /// <summary>
        /// <c>AlmostEqual</c> with relative error.
        /// </summary>
        [Test]
        public void AlmostEqualWithRelativeError()
        {
            // compare zero and negative zero
            Assert.IsTrue(0.0.AlmostEqualWithError(-0.0, 1e-5));
            Assert.IsTrue(0.0.AlmostEqualWithError(-0.0, 1e-15));

            // compare two nearby numbers
            Assert.IsTrue(1.0.AlmostEqualWithError(1.0 + (3 * _doublePrecision), 1e-15));
            Assert.IsTrue(1.0.AlmostEqualWithError(1.0 + _doublePrecision, 1e-15));
            Assert.IsTrue(1.0.AlmostEqualWithError(1.0 + 1e-16, 1e-15));
            Assert.IsFalse(1.0.AlmostEqualWithError(1.0 + 1e-15, 1e-15));
            Assert.IsFalse(1.0.AlmostEqualWithError(1.0 + 1e-14, 1e-15));

            // compare with the two numbers reversed in compare order
            Assert.IsTrue((1.0 + (3 * _doublePrecision)).AlmostEqualWithError(1.0, 1e-15));
            Assert.IsTrue((1.0 + _doublePrecision).AlmostEqualWithError(1.0, 1e-15));
            Assert.IsTrue((1.0 + 1e-16).AlmostEqualWithError(1.0, 1e-15));
            Assert.IsFalse((1.0 + 1e-15).AlmostEqualWithError(1.0, 1e-15));
            Assert.IsFalse((1.0 + 1e-14).AlmostEqualWithError(1.0, 1e-15));

            // compare different numbers
            Assert.IsFalse(2.0.AlmostEqualWithError(1.0, 1e-15));
            Assert.IsFalse(1.0.AlmostEqualWithError(2.0, 1e-15));

            // compare different numbers with large tolerance
            Assert.IsFalse(2.0.AlmostEqualWithError(1.0, 1e-5));
            Assert.IsFalse(1.0.AlmostEqualWithError(2.0, 1e-5));
            Assert.IsTrue(2.0.AlmostEqualWithError(1.0, 1e+1));
            Assert.IsTrue(1.0.AlmostEqualWithError(2.0, 1e+1));

            // compare inf & inf
            Assert.IsTrue(double.PositiveInfinity.AlmostEqualWithError(double.PositiveInfinity, 1e-15));
            Assert.IsTrue(double.NegativeInfinity.AlmostEqualWithError(double.NegativeInfinity, 1e-15));

            // compare -inf and inf
            Assert.IsFalse(double.PositiveInfinity.AlmostEqualWithError(double.NegativeInfinity, 1e-15));
            Assert.IsFalse(double.NegativeInfinity.AlmostEqualWithError(double.PositiveInfinity, 1e-15));

            // compare inf and non-inf
            Assert.IsFalse(double.PositiveInfinity.AlmostEqualWithError(1.0, 1e-15));
            Assert.IsFalse(1.0.AlmostEqualWithError(double.PositiveInfinity, 1e-15));
            Assert.IsFalse(double.NegativeInfinity.AlmostEqualWithError(1.0, 1e-15));
            Assert.IsFalse(1.0.AlmostEqualWithError(double.NegativeInfinity, 1e-15));

            // compare tiny numbers with opposite signs
            Assert.IsFalse(1e-12.AlmostEqualWithError(-1e-12, 1e-14));
            Assert.IsFalse((-1e-12).AlmostEqualWithError(1e-12, 1e-14));

            Assert.IsFalse((-2.0).AlmostEqualWithError(2.0, 1e-14));
            Assert.IsFalse(2.0.AlmostEqualWithError(-2.0, 1e-14));
        }

        /// <summary>
        /// <c>AlmostEqual</c> with max numbers between.
        /// </summary>
        [Test]
        public void AlmostEqualWithMaxNumbersBetween()
        {
            // compare zero and negative zero
            Assert.IsTrue(Precision.AlmostEqual(0, -0, 1));

            // compare two nearby numbers
            Assert.IsFalse(1.0.AlmostEqual(1.0 + (3 * _doublePrecision), 1));
            Assert.IsTrue(1.0.AlmostEqual(1.0 + _doublePrecision, 1));
            Assert.IsTrue(1.0.AlmostEqual(1.0 - _doublePrecision, 1));
            Assert.IsFalse(1.0.AlmostEqual(1.0 - (3 * _doublePrecision), 1));

            // compare with the two numbers reversed in compare order
            Assert.IsFalse((1.0 + (3 * _doublePrecision)).AlmostEqual(1.0, 1));
            Assert.IsTrue((1.0 + _doublePrecision).AlmostEqual(1.0, 1));
            Assert.IsTrue((1.0 - _doublePrecision).AlmostEqual(1.0, 1));
            Assert.IsFalse((1.0 - (3 * _doublePrecision)).AlmostEqual(1.0, 1));

            // compare two slightly more different numbers
            Assert.IsFalse(1.0.AlmostEqual(1.0 + (10 * _doublePrecision), 1));
            Assert.IsTrue(1.0.AlmostEqual(1.0 + (10 * _doublePrecision), 10));
            Assert.IsTrue(1.0.AlmostEqual(1.0 - (10 * _doublePrecision), 10));
            Assert.IsFalse(1.0.AlmostEqual(1.0 - (10 * _doublePrecision), 1));

            // compare different numbers
            Assert.IsFalse(2.0.AlmostEqual(1.0, 1));
            Assert.IsFalse(1.0.AlmostEqual(2.0, 1));

            // compare different numbers with large tolerance
            Assert.IsFalse(1.0.AlmostEqual(1.0 + (1e5 * _doublePrecision), 1));
            Assert.IsTrue(1.0.AlmostEqual(1.0 - (1e5 * _doublePrecision), 200000));
            Assert.IsFalse(1.0.AlmostEqual(1.0 - (1e5 * _doublePrecision), 1));

            // compare inf & inf
            Assert.IsTrue(double.PositiveInfinity.AlmostEqual(double.PositiveInfinity, 1));
            Assert.IsTrue(double.NegativeInfinity.AlmostEqual(double.NegativeInfinity, 1));

            // compare -inf and inf
            Assert.IsFalse(double.PositiveInfinity.AlmostEqual(double.NegativeInfinity, 1));
            Assert.IsFalse(double.NegativeInfinity.AlmostEqual(double.PositiveInfinity, 1));

            // compare inf and non-inf
            Assert.IsFalse(double.PositiveInfinity.AlmostEqual(1.0, 1));
            Assert.IsFalse(1.0.AlmostEqual(double.PositiveInfinity, 1));

            Assert.IsFalse(double.NegativeInfinity.AlmostEqual(1.0, 1));
            Assert.IsFalse(1.0.AlmostEqual(double.NegativeInfinity, 1));

            // compare tiny numbers with opposite signs
            Assert.IsFalse(double.Epsilon.AlmostEqual(-double.Epsilon, 1));
            Assert.IsFalse((-double.Epsilon).AlmostEqual(double.Epsilon, 1));

            Assert.IsFalse((-2.0).AlmostEqual(2.0, 1));
            Assert.IsFalse(2.0.AlmostEqual(-2.0, 1));
        }

        /// <summary>
        /// <c>AlmostEqual</c> within decimal places with decimal places less than one throws <c>ArgumentOutOfRangeException</c>.
        /// </summary>
        [Test]
        public void AlmostEqualWithinDecimalPlacesWithDecimalPlacesLessThanOneThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Precision.AlmostEqualInDecimalPlaces(1, 2, 0));
        }

        /// <summary>
        /// <c>AlmostEqual</c> within decimal places.
        /// </summary>
        [Test]
        public void AlmostEqualWithinDecimalPlaces()
        {
            Assert.IsFalse(Precision.AlmostEqualInDecimalPlaces(1, double.NaN, 2));
            Assert.IsFalse(double.NaN.AlmostEqualInDecimalPlaces(2, 2));
            Assert.IsFalse(double.NaN.AlmostEqualInDecimalPlaces(double.NaN, 2));

            Assert.IsFalse(double.NegativeInfinity.AlmostEqualInDecimalPlaces(2, 2));
            Assert.IsFalse(Precision.AlmostEqualInDecimalPlaces(1, double.NegativeInfinity, 2));

            Assert.IsFalse(double.PositiveInfinity.AlmostEqualInDecimalPlaces(2, 2));
            Assert.IsFalse(Precision.AlmostEqualInDecimalPlaces(1, double.PositiveInfinity, 2));

            Assert.IsFalse(double.NegativeInfinity.AlmostEqualInDecimalPlaces(double.PositiveInfinity, 2));
            Assert.IsFalse(double.PositiveInfinity.AlmostEqualInDecimalPlaces(double.NegativeInfinity, 2));

            Assert.IsTrue(double.PositiveInfinity.AlmostEqualInDecimalPlaces(double.PositiveInfinity, 2));
            Assert.IsTrue(double.NegativeInfinity.AlmostEqualInDecimalPlaces(double.NegativeInfinity, 2));

            Assert.IsTrue(1.0.AlmostEqualInDecimalPlaces(1.04, 2));
            Assert.IsTrue(1.0.AlmostEqualInDecimalPlaces(0.96, 2));

            Assert.IsFalse(1.0.AlmostEqualInDecimalPlaces(1.06, 2));
            Assert.IsFalse(1.0.AlmostEqualInDecimalPlaces(0.94, 2));

            Assert.IsTrue(100.0.AlmostEqualInDecimalPlaces(104.00, 2));
            Assert.IsTrue(100.0.AlmostEqualInDecimalPlaces(96.000, 2));

            Assert.IsFalse(100.0.AlmostEqualInDecimalPlaces(106.00, 2));
            Assert.IsFalse(100.0.AlmostEqualInDecimalPlaces(94.000, 2));

            Assert.IsTrue(0.0.AlmostEqualInDecimalPlaces(4 * _doublePrecision, 12));
            Assert.IsTrue(0.0.AlmostEqualInDecimalPlaces(-4 * _doublePrecision, 12));
        }

        /// <summary>
        /// <c>AlmostEqual</c> with small numbers and small number of decimal places.
        /// </summary>
        [Test]
        public void AlmostEqualWithSmallNumbersAndSmallNumberOfDecimalPlaces()
        {
            Assert.IsTrue(0.0.AlmostEqualInDecimalPlaces(1e-12, 1));
            Assert.IsTrue(0.0.AlmostEqualInDecimalPlaces(-1e-12, 1));

            Assert.IsFalse(0.0.AlmostEqualInDecimalPlaces(1e-12, 13));
            Assert.IsFalse(0.0.AlmostEqualInDecimalPlaces(-1e-12, 13));

            Assert.IsFalse(0.0.AlmostEqualInDecimalPlaces(1, 1));
            Assert.IsFalse(0.0.AlmostEqualInDecimalPlaces(-1, 1));
        }

        /// <summary>
        /// <c>AlmostEqual</c> with decimal places with sign revert.
        /// </summary>
        [Test]
        public void AlmostEqualWithDecimalPlacesWithSignRevert()
        {
            Assert.IsFalse(0.5.AlmostEqualInDecimalPlaces(0.3, 1));
        }

        /// <summary>
        /// Is larger with max numbers between.
        /// </summary>
        [Test]
        public void IsLargerWithMaxNumbersBetween()
        {
            // compare zero and negative zero
            Assert.IsFalse(Precision.IsLarger(0, -0, 1));

            // compare two nearby numbers
            Assert.IsFalse(1.0.IsLarger(1.0 + (3 * _doublePrecision), 1));
            Assert.IsFalse(1.0.IsLarger(1.0 + _doublePrecision, 1));
            Assert.IsFalse(1.0.IsLarger(1.0 - _doublePrecision, 1));
            Assert.IsTrue(1.0.IsLarger(1.0 - (3 * _doublePrecision), 1));

            // compare with the two numbers reversed in compare order
            Assert.IsTrue((1.0 + (3 * _doublePrecision)).IsLarger(1.0, 1));
            Assert.IsFalse((1.0 + _doublePrecision).IsLarger(1.0, 1));
            Assert.IsFalse((1.0 - _doublePrecision).IsLarger(1.0, 1));
            Assert.IsFalse((1.0 - (3 * _doublePrecision)).IsLarger(1.0, 1));

            // compare two slightly more different numbers
            Assert.IsFalse(1.0.IsLarger(1.0 + (10 * _doublePrecision), 1));
            Assert.IsFalse(1.0.IsLarger(1.0 + (10 * _doublePrecision), 10));
            Assert.IsFalse(1.0.IsLarger(1.0 - (10 * _doublePrecision), 10));
            Assert.IsTrue(1.0.IsLarger(1.0 - (10 * _doublePrecision), 1));

            // compare different numbers
            Assert.IsTrue(2.0.IsLarger(1.0, 1));
            Assert.IsFalse(1.0.IsLarger(2.0, 1));

            // compare different numbers with large tolerance
            Assert.IsFalse(1.0.IsLarger(1.0 + (1e5 * _doublePrecision), 1));
            Assert.IsFalse(1.0.IsLarger(1.0 - (1e5 * _doublePrecision), 200000));
            Assert.IsTrue(1.0.IsLarger(1.0 - (1e5 * _doublePrecision), 1));

            // compare inf & inf
            Assert.IsFalse(double.PositiveInfinity.IsLarger(double.PositiveInfinity, 1));
            Assert.IsFalse(double.NegativeInfinity.IsLarger(double.NegativeInfinity, 1));

            // compare -inf and inf
            Assert.IsTrue(double.PositiveInfinity.IsLarger(double.NegativeInfinity, 1));
            Assert.IsFalse(double.NegativeInfinity.IsLarger(double.PositiveInfinity, 1));

            // compare inf and non-inf
            Assert.IsTrue(double.PositiveInfinity.IsLarger(1.0, 1));
            Assert.IsFalse(1.0.IsLarger(double.PositiveInfinity, 1));

            Assert.IsFalse(double.NegativeInfinity.IsLarger(1.0, 1));
            Assert.IsTrue(1.0.IsLarger(double.NegativeInfinity, 1));

            // compare tiny numbers with opposite signs
            Assert.IsTrue(double.Epsilon.IsLarger(-double.Epsilon, 1));
            Assert.IsFalse((-double.Epsilon).IsLarger(double.Epsilon, 1));
        }

        /// <summary>
        /// Is larger with decimal places.
        /// </summary>
        [Test]
        public void IsLargerWithDecimalPlaces()
        {
            Assert.IsFalse(Precision.IsLargerWithDecimalPlaces(1, double.NaN, 2));
            Assert.IsFalse(double.NaN.IsLargerWithDecimalPlaces(2, 2));
            Assert.IsFalse(double.NaN.IsLargerWithDecimalPlaces(double.NaN, 2));

            Assert.IsFalse(double.NegativeInfinity.IsLargerWithDecimalPlaces(2, 2));
            Assert.IsTrue(Precision.IsLargerWithDecimalPlaces(1, double.NegativeInfinity, 2));

            Assert.IsTrue(double.PositiveInfinity.IsLargerWithDecimalPlaces(2, 2));
            Assert.IsFalse(Precision.IsLargerWithDecimalPlaces(1, double.PositiveInfinity, 2));

            Assert.IsFalse(double.NegativeInfinity.IsLargerWithDecimalPlaces(double.PositiveInfinity, 2));
            Assert.IsTrue(double.PositiveInfinity.IsLargerWithDecimalPlaces(double.NegativeInfinity, 2));

            Assert.IsFalse(double.PositiveInfinity.IsLargerWithDecimalPlaces(double.PositiveInfinity, 2));
            Assert.IsFalse(double.NegativeInfinity.IsLargerWithDecimalPlaces(double.NegativeInfinity, 2));

            Assert.IsFalse(1.0.IsLargerWithDecimalPlaces(1.04, 2));
            Assert.IsFalse(1.0.IsLargerWithDecimalPlaces(0.96, 2));

            Assert.IsFalse(1.0.IsLargerWithDecimalPlaces(1.06, 2));
            Assert.IsTrue(1.0.IsLargerWithDecimalPlaces(0.94, 2));

            Assert.IsFalse(100.0.IsLargerWithDecimalPlaces(104.00, 2));
            Assert.IsFalse(100.0.IsLargerWithDecimalPlaces(96.000, 2));

            Assert.IsFalse(100.0.IsLargerWithDecimalPlaces(106.00, 2));
            Assert.IsTrue(100.0.IsLargerWithDecimalPlaces(94.000, 2));

            var max = 4 * Math.Pow(10, _doublePrecision.Magnitude());
            Assert.IsFalse(0.0.IsLargerWithDecimalPlaces(max, -_doublePrecision.Magnitude()));
            Assert.IsFalse(0.0.IsLargerWithDecimalPlaces(-max, -_doublePrecision.Magnitude()));

            max = 6 * Math.Pow(10, _doublePrecision.Magnitude());
            Assert.IsFalse(0.0.IsLargerWithDecimalPlaces(max, -_doublePrecision.Magnitude()));
            Assert.IsTrue(0.0.IsLargerWithDecimalPlaces(-max, -_doublePrecision.Magnitude()));
        }

        /// <summary>
        /// Is smaller with max numbers between.
        /// </summary>
        [Test]
        public void IsSmallerWithMaxNumbersBetween()
        {
            // compare zero and negative zero
            Assert.IsFalse(Precision.IsSmaller(0, -0, 1));

            // compare two nearby numbers
            Assert.IsTrue(1.0.IsSmaller(1.0 + (3 * _doublePrecision), 1));
            Assert.IsFalse(1.0.IsSmaller(1.0 + _doublePrecision, 1));
            Assert.IsFalse(1.0.IsSmaller(1.0 - _doublePrecision, 1));
            Assert.IsFalse(1.0.IsSmaller(1.0 - (3 * _doublePrecision), 1));

            // compare with the two numbers reversed in compare order
            Assert.IsFalse((1.0 + (3 * _doublePrecision)).IsSmaller(1.0, 1));
            Assert.IsFalse((1.0 + _doublePrecision).IsSmaller(1.0, 1));
            Assert.IsFalse((1.0 - _doublePrecision).IsSmaller(1.0, 1));
            Assert.IsTrue((1.0 - (3 * _doublePrecision)).IsSmaller(1.0, 1));

            // compare two slightly more different numbers
            Assert.IsTrue(1.0.IsSmaller(1.0 + (10 * _doublePrecision), 1));
            Assert.IsFalse(1.0.IsSmaller(1.0 + (10 * _doublePrecision), 10));
            Assert.IsFalse(1.0.IsSmaller(1.0 - (10 * _doublePrecision), 10));
            Assert.IsFalse(1.0.IsSmaller(1.0 - (10 * _doublePrecision), 1));

            // compare different numbers
            Assert.IsFalse(2.0.IsSmaller(1.0, 1));
            Assert.IsTrue(1.0.IsSmaller(2.0, 1));

            // compare different numbers with large tolerance
            Assert.IsTrue(1.0.IsSmaller(1.0 + (1e5 * _doublePrecision), 1));
            Assert.IsFalse(1.0.IsSmaller(1.0 - (1e5 * _doublePrecision), 200000));
            Assert.IsFalse(1.0.IsSmaller(1.0 - (1e5 * _doublePrecision), 1));

            // compare inf & inf
            Assert.IsFalse(double.PositiveInfinity.IsSmaller(double.PositiveInfinity, 1));
            Assert.IsFalse(double.NegativeInfinity.IsSmaller(double.NegativeInfinity, 1));

            // compare -inf and inf
            Assert.IsFalse(double.PositiveInfinity.IsSmaller(double.NegativeInfinity, 1));
            Assert.IsTrue(double.NegativeInfinity.IsSmaller(double.PositiveInfinity, 1));

            // compare inf and non-inf
            Assert.IsFalse(double.PositiveInfinity.IsSmaller(1.0, 1));
            Assert.IsTrue(1.0.IsSmaller(double.PositiveInfinity, 1));

            Assert.IsTrue(double.NegativeInfinity.IsSmaller(1.0, 1));
            Assert.IsFalse(1.0.IsSmaller(double.NegativeInfinity, 1));

            // compare tiny numbers with opposite signs
            Assert.IsFalse(double.Epsilon.IsSmaller(-double.Epsilon, 1));
            Assert.IsTrue((-double.Epsilon).IsSmaller(double.Epsilon, 1));
        }

        /// <summary>
        /// Is smaller with decimal places.
        /// </summary>
        [Test]
        public void IsSmallerWithDecimalPlaces()
        {
            Assert.IsFalse(Precision.IsSmallerWithDecimalPlaces(1, double.NaN, 2));
            Assert.IsFalse(double.NaN.IsSmallerWithDecimalPlaces(2, 2));
            Assert.IsFalse(double.NaN.IsSmallerWithDecimalPlaces(double.NaN, 2));

            Assert.IsTrue(double.NegativeInfinity.IsSmallerWithDecimalPlaces(2, 2));
            Assert.IsFalse(Precision.IsSmallerWithDecimalPlaces(1, double.NegativeInfinity, 2));

            Assert.IsFalse(double.PositiveInfinity.IsSmallerWithDecimalPlaces(2, 2));
            Assert.IsTrue(Precision.IsSmallerWithDecimalPlaces(1, double.PositiveInfinity, 2));

            Assert.IsTrue(double.NegativeInfinity.IsSmallerWithDecimalPlaces(double.PositiveInfinity, 2));
            Assert.IsFalse(double.PositiveInfinity.IsSmallerWithDecimalPlaces(double.NegativeInfinity, 2));

            Assert.IsFalse(double.PositiveInfinity.IsSmallerWithDecimalPlaces(double.PositiveInfinity, 2));
            Assert.IsFalse(double.NegativeInfinity.IsSmallerWithDecimalPlaces(double.NegativeInfinity, 2));

            Assert.IsFalse(1.0.IsSmallerWithDecimalPlaces(1.04, 2));
            Assert.IsFalse(1.0.IsSmallerWithDecimalPlaces(0.96, 2));

            Assert.IsTrue(1.0.IsSmallerWithDecimalPlaces(1.06, 2));
            Assert.IsFalse(1.0.IsSmallerWithDecimalPlaces(0.94, 2));

            Assert.IsFalse(100.0.IsSmallerWithDecimalPlaces(104.00, 2));
            Assert.IsFalse(100.0.IsSmallerWithDecimalPlaces(96.000, 2));

            Assert.IsTrue(100.0.IsSmallerWithDecimalPlaces(106.00, 2));
            Assert.IsFalse(100.0.IsSmallerWithDecimalPlaces(94.000, 2));

            var max = 4 * Math.Pow(10, _doublePrecision.Magnitude());
            Assert.IsFalse(0.0.IsSmallerWithDecimalPlaces(max, -_doublePrecision.Magnitude()));
            Assert.IsFalse(0.0.IsSmallerWithDecimalPlaces(-max, -_doublePrecision.Magnitude()));

            max = 6 * Math.Pow(10, _doublePrecision.Magnitude());
            Assert.IsTrue(0.0.IsSmallerWithDecimalPlaces(max, -_doublePrecision.Magnitude()));
            Assert.IsFalse(0.0.IsSmallerWithDecimalPlaces(-max, -_doublePrecision.Magnitude()));
        }

        /// <summary>
        /// Compare to with max numbers between with negative number throws <c>ArgumentOutOfRangeException</c>.
        /// </summary>
        [Test]
        public void CompareToWithMaxNumbersBetweenWithNegativeNumberThrowsArgumentOutOfRangeException()
        {
            const double Value = 10.0;
            Assert.Throws<ArgumentOutOfRangeException>(() => Value.CompareTo(Value, -1));
        }

        /// <summary>
        /// Compare to with max numbers between with zero number throws <c>ArgumentOutOfRangeException</c>.
        /// </summary>
        [Test]
        public void CompareToWithMaxNumbersBetweenWithZeroNumberThrowsArgumentOutOfRangeException()
        {
            const double Value = 10.0;
            Assert.Throws<ArgumentOutOfRangeException>(() => Value.CompareTo(Value, 0));
        }

        /// <summary>
        /// Compare to with max numbers between with infinity number.
        /// </summary>
        [Test]
        public void CompareToWithMaxNumbersBetweenWithInfinityValue()
        {
            Assert.AreEqual(0, double.PositiveInfinity.CompareTo(double.PositiveInfinity, 1));
            Assert.AreEqual(0, double.NegativeInfinity.CompareTo(double.NegativeInfinity, 1));

            Assert.AreEqual(1, double.PositiveInfinity.CompareTo(double.NegativeInfinity, 1));
            Assert.AreEqual(-1, double.NegativeInfinity.CompareTo(double.PositiveInfinity, 1));

            Assert.AreEqual(-1, Precision.CompareTo(1, double.PositiveInfinity, 1));
            Assert.AreEqual(1, double.PositiveInfinity.CompareTo(1, 1));

            Assert.AreEqual(1, Precision.CompareTo(1, double.NegativeInfinity, 1));
            Assert.AreEqual(-1, double.NegativeInfinity.CompareTo(1, 1));
        }

        /// <summary>
        /// Compare to with max numbers between with NaN value.
        /// </summary>
        [Test]
        public void CompareToWithMaxNumbersBetweenWithNaNValue()
        {
            // compare nan & not-nan
            Assert.AreEqual(1, Precision.CompareTo(1, double.NaN, 1));
            Assert.AreEqual(0, double.NaN.CompareTo(double.NaN, 1));
            Assert.AreEqual(-1, double.NaN.CompareTo(1, 1));
        }

        /// <summary>
        /// Compare to with max numbers between for values overflow a long.
        /// </summary>
        [Test]
        public void CompareToWithMaxNumbersBetweenForValuesThatOverFlowALong()
        {
            Assert.AreEqual(1, 2.0.CompareTo(-2.0, 1));
            Assert.AreEqual(-1, (-2.0).CompareTo(2.0, 1));
        }

        /// <summary>
        /// Compare to with max numbers between.
        /// </summary>
        [Test]
        public void CompareToWithMaxNumbersBetween()
        {
            // compare zero and negative zero
            Assert.AreEqual(0, Precision.CompareTo(0, -0, 1));

            // compare two nearby numbers
            Assert.AreEqual(-1, 1.0.CompareTo(1.0 + (3 * _doublePrecision), 1));
            Assert.AreEqual(0, 1.0.CompareTo(1.0 + _doublePrecision, 1));
            Assert.AreEqual(0, 1.0.CompareTo(1.0 - _doublePrecision, 1));
            Assert.AreEqual(1, 1.0.CompareTo(1.0 - (3 * _doublePrecision), 1));

            // compare with the two numbers reversed in compare order
            Assert.AreEqual(1, (1.0 + (3 * _doublePrecision)).CompareTo(1.0, 1));
            Assert.AreEqual(0, (1.0 + _doublePrecision).CompareTo(1.0, 1));
            Assert.AreEqual(0, (1.0 - _doublePrecision).CompareTo(1.0, 1));
            Assert.AreEqual(-1, (1.0 - (3 * _doublePrecision)).CompareTo(1.0, 1));

            // compare two slightly more different numbers
            Assert.AreEqual(-1, 1.0.CompareTo(1.0 + (10 * _doublePrecision), 1));
            Assert.AreEqual(0, 1.0.CompareTo(1.0 + (10 * _doublePrecision), 10));
            Assert.AreEqual(0, 1.0.CompareTo(1.0 - (10 * _doublePrecision), 10));
            Assert.AreEqual(1, 1.0.CompareTo(1.0 - (10 * _doublePrecision), 1));

            // compare different numbers
            Assert.AreEqual(1, 2.0.CompareTo(1.0, 1));
            Assert.AreEqual(-1, 1.0.CompareTo(2.0, 1));

            // compare different numbers with large tolerance
            Assert.AreEqual(-1, 1.0.CompareTo(1.0 + (1e5 * _doublePrecision), 1));
            Assert.AreEqual(0, 1.0.CompareTo(1.0 - (1e5 * _doublePrecision), 200000));
            Assert.AreEqual(1, 1.0.CompareTo(1.0 - (1e5 * _doublePrecision), 1));

            // compare inf & inf
            Assert.AreEqual(0, double.PositiveInfinity.CompareTo(double.PositiveInfinity, 1));
            Assert.AreEqual(0, double.NegativeInfinity.CompareTo(double.NegativeInfinity, 1));

            // compare -inf and inf
            Assert.AreEqual(1, double.PositiveInfinity.CompareTo(double.NegativeInfinity, 1));
            Assert.AreEqual(-1, double.NegativeInfinity.CompareTo(double.PositiveInfinity, 1));

            // compare inf and non-inf
            Assert.AreEqual(1, double.PositiveInfinity.CompareTo(1.0, 1));
            Assert.AreEqual(-1, 1.0.CompareTo(double.PositiveInfinity, 1));

            Assert.AreEqual(-1, double.NegativeInfinity.CompareTo(1.0, 1));
            Assert.AreEqual(1, 1.0.CompareTo(double.NegativeInfinity, 1));

            // compare tiny numbers with opposite signs
            Assert.AreEqual(1, double.Epsilon.CompareTo(-double.Epsilon, 1));
            Assert.AreEqual(-1, (-double.Epsilon).CompareTo(double.Epsilon, 1));
        }

        /// <summary>
        /// Compare to with decimal places.
        /// </summary>
        [Test]
        public void CompareToWithDecimalPlaces()
        {
            // compare zero and negative zero
            Assert.AreEqual(0, Precision.CompareToInDecimalPlaces(0, -0, 1));
            Assert.AreEqual(0, Precision.CompareToInDecimalPlaces(0, -0, Precision.NumberOfDecimalPlacesForDoubles));
            Assert.AreEqual(0, Precision.CompareToInDecimalPlaces(0, -0, Precision.NumberOfDecimalPlacesForFloats));

            // compare two nearby numbers
            Assert.AreEqual(-1, 1.0.CompareToInDecimalPlaces(1.0 + (10 * _doublePrecision), Precision.NumberOfDecimalPlacesForDoubles));
            Assert.AreEqual(0, 1.0.CompareToInDecimalPlaces(1.0 + _doublePrecision, Precision.NumberOfDecimalPlacesForDoubles));
            Assert.AreEqual(0, 1.0.CompareToInDecimalPlaces(1.0 - _doublePrecision, Precision.NumberOfDecimalPlacesForDoubles));
            Assert.AreEqual(1, 1.0.CompareToInDecimalPlaces(1.0 - (10 * _doublePrecision), Precision.NumberOfDecimalPlacesForDoubles));

            // compare with the two numbers reversed in compare order
            Assert.AreEqual(1, (1.0 + (10 * _doublePrecision)).CompareToInDecimalPlaces(1.0, Precision.NumberOfDecimalPlacesForDoubles));
            Assert.AreEqual(0, (1.0 + _doublePrecision).CompareToInDecimalPlaces(1.0, Precision.NumberOfDecimalPlacesForDoubles));
            Assert.AreEqual(0, (1.0 - _doublePrecision).CompareToInDecimalPlaces(1.0, Precision.NumberOfDecimalPlacesForDoubles));
            Assert.AreEqual(-1, (1.0 - (10 * _doublePrecision)).CompareToInDecimalPlaces(1.0, Precision.NumberOfDecimalPlacesForDoubles));

            // compare two slightly more different numbers
            Assert.AreEqual(-1, 1.0.CompareToInDecimalPlaces(1.0 + (50 * _doublePrecision), Precision.NumberOfDecimalPlacesForDoubles));
            Assert.AreEqual(0, 1.0.CompareToInDecimalPlaces(1.0 + (50 * _doublePrecision), Precision.NumberOfDecimalPlacesForDoubles - 2));
            Assert.AreEqual(0, 1.0.CompareToInDecimalPlaces(1.0 - (50 * _doublePrecision), Precision.NumberOfDecimalPlacesForDoubles - 2));
            Assert.AreEqual(1, 1.0.CompareToInDecimalPlaces(1.0 - (50 * _doublePrecision), Precision.NumberOfDecimalPlacesForDoubles));

            // compare different numbers
            Assert.AreEqual(1, 2.0.CompareToInDecimalPlaces(1.0, Precision.NumberOfDecimalPlacesForDoubles));
            Assert.AreEqual(-1, 1.0.CompareToInDecimalPlaces(2.0, Precision.NumberOfDecimalPlacesForDoubles));

            // compare different numbers with large tolerance
            Assert.AreEqual(-1, 1.0.CompareToInDecimalPlaces(1.0 + (1e5 * _doublePrecision), Precision.NumberOfDecimalPlacesForDoubles));
            Assert.AreEqual(0, 1.0.CompareToInDecimalPlaces(1.0 - (1e5 * _doublePrecision), 10));
            Assert.AreEqual(1, 1.0.CompareToInDecimalPlaces(1.0 - (1e5 * _doublePrecision), Precision.NumberOfDecimalPlacesForDoubles));

            // compare inf & inf
            Assert.AreEqual(0, double.PositiveInfinity.CompareToInDecimalPlaces(double.PositiveInfinity, Precision.NumberOfDecimalPlacesForDoubles));
            Assert.AreEqual(0, double.NegativeInfinity.CompareToInDecimalPlaces(double.NegativeInfinity, Precision.NumberOfDecimalPlacesForDoubles));

            // compare -inf and inf
            Assert.AreEqual(1, double.PositiveInfinity.CompareToInDecimalPlaces(double.NegativeInfinity, Precision.NumberOfDecimalPlacesForDoubles));
            Assert.AreEqual(-1, double.NegativeInfinity.CompareToInDecimalPlaces(double.PositiveInfinity, Precision.NumberOfDecimalPlacesForDoubles));

            // compare inf and non-inf
            Assert.AreEqual(1, double.PositiveInfinity.CompareToInDecimalPlaces(1.0, Precision.NumberOfDecimalPlacesForDoubles));
            Assert.AreEqual(-1, 1.0.CompareToInDecimalPlaces(double.PositiveInfinity, Precision.NumberOfDecimalPlacesForDoubles));
            Assert.AreEqual(-1, double.NegativeInfinity.CompareToInDecimalPlaces(1.0, Precision.NumberOfDecimalPlacesForDoubles));
            Assert.AreEqual(1, 1.0.CompareToInDecimalPlaces(double.NegativeInfinity, Precision.NumberOfDecimalPlacesForDoubles));
        }
    }
}
