// <copyright file="PrecisionTest.cs" company="Math.NET">
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
using NUnit.Framework;

namespace MathNet.Numerics.UnitTests
{
    /// <summary>
    /// Precision tests.
    /// </summary>
    [TestFixture]
    public sealed class PrecisionTest
    {
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

            Assert.AreEqual(-7, 1e-7.Magnitude());
            Assert.AreEqual(-8, 1e-8.Magnitude());
            Assert.AreEqual(-9, 1e-9.Magnitude());
            Assert.AreEqual(-10, 1e-10.Magnitude());
            Assert.AreEqual(-11, 1e-11.Magnitude());
            Assert.AreEqual(-12, 1e-12.Magnitude());

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

            Assert.AreEqual(-7, (-1e-7).Magnitude());
            Assert.AreEqual(-8, (-1e-8).Magnitude());
            Assert.AreEqual(-9, (-1e-9).Magnitude());
            Assert.AreEqual(-10, (-1e-10).Magnitude());
            Assert.AreEqual(-11, (-1e-11).Magnitude());
            Assert.AreEqual(-12, (-1e-12).Magnitude());

            Assert.AreEqual(5, (-1.1e5).Magnitude());
            Assert.AreEqual(-5, (-2.2e-5).Magnitude());
            Assert.AreEqual(9, (-3.3e9).Magnitude());
            Assert.AreEqual(-11, (-4.4e-11).Magnitude());
        }

        /// <summary>
        /// Get magnitude scaled value.
        /// </summary>
        [Test]
        public void ScaleUnitMagnitude()
        {
            Assert.AreEqual(0, Precision.ScaleUnitMagnitude(0), 1e-12);

            Assert.AreEqual(1, Precision.ScaleUnitMagnitude(1), 1e-12);
            Assert.AreEqual(1, Precision.ScaleUnitMagnitude(10), 1e-12);
            Assert.AreEqual(1, Precision.ScaleUnitMagnitude(100), 1e-12);
            Assert.AreEqual(1, Precision.ScaleUnitMagnitude(1000), 1e-12);
            Assert.AreEqual(1, Precision.ScaleUnitMagnitude(10000), 1e-12);
            Assert.AreEqual(1, Precision.ScaleUnitMagnitude(100000), 1e-12);
            Assert.AreEqual(1, Precision.ScaleUnitMagnitude(1000000), 1e-12);

            Assert.AreEqual(1.1, 1.1e5.ScaleUnitMagnitude(), 1e-12);
            Assert.AreEqual(2.2, 2.2e-5.ScaleUnitMagnitude(), 1e-12);
            Assert.AreEqual(3.3, 3.3e9.ScaleUnitMagnitude(), 1e-12);
            Assert.AreEqual(4.4, 4.4e-11.ScaleUnitMagnitude(), 1e-12);
        }

        /// <summary>
        /// Can get magnitude scaled value with a negative values.
        /// </summary>
        [Test]
        public void ScaleUnitMagnitudeWithNegativeValues()
        {
            Assert.AreEqual(0, Precision.ScaleUnitMagnitude(0), 1e-12);

            Assert.AreEqual(-1, Precision.ScaleUnitMagnitude(-1), 1e-12);
            Assert.AreEqual(-1, Precision.ScaleUnitMagnitude(-10), 1e-12);
            Assert.AreEqual(-1, Precision.ScaleUnitMagnitude(-100), 1e-12);
            Assert.AreEqual(-1, Precision.ScaleUnitMagnitude(-1000), 1e-12);
            Assert.AreEqual(-1, Precision.ScaleUnitMagnitude(-10000), 1e-12);
            Assert.AreEqual(-1, Precision.ScaleUnitMagnitude(-100000), 1e-12);
            Assert.AreEqual(-1, Precision.ScaleUnitMagnitude(-1000000), 1e-12);

            Assert.AreEqual(-1.1, (-1.1e5).ScaleUnitMagnitude(), 1e-12);
            Assert.AreEqual(-2.2, (-2.2e-5).ScaleUnitMagnitude(), 1e-12);
            Assert.AreEqual(-3.3, (-3.3e9).ScaleUnitMagnitude(), 1e-12);
            Assert.AreEqual(-4.4, (-4.4e-11).ScaleUnitMagnitude(), 1e-12);
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
        public void CoerceZeroNumbersBetween()
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
        public void CoerceZeroRelativeTolerance()
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
            Assert.That(() => Precision.RangeOfMatchingFloatingPointNumbers(10, -1), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        /// <summary>
        /// Range of matching floating point numbers with value at infinity.
        /// </summary>
        [Test]
        public void RangeOfMatchingFloatingPointNumbersWithValueAtInfinity()
        {
            var minmax = double.PositiveInfinity.RangeOfMatchingFloatingPointNumbers(1);
            Assert.AreEqual(double.PositiveInfinity, minmax.Item1);
            Assert.AreEqual(double.PositiveInfinity, minmax.Item2);

            minmax = double.NegativeInfinity.RangeOfMatchingFloatingPointNumbers(1);
            Assert.AreEqual(double.NegativeInfinity, minmax.Item1);
            Assert.AreEqual(double.NegativeInfinity, minmax.Item2);
        }

        /// <summary>
        /// Range of matching floating point numbers with value at NaN.
        /// </summary>
        [Test]
        public void RangeOfMatchingFloatingPointNumbersWithValueAtNaN()
        {
            var minmax = double.NaN.RangeOfMatchingFloatingPointNumbers(1);
            Assert.IsTrue(double.IsNaN(minmax.Item1));
            Assert.IsTrue(double.IsNaN(minmax.Item2));
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
            var minmax = input.RangeOfMatchingFloatingPointNumbers(10);
            Assert.AreEqual(expectedMin, minmax.Item1);
            Assert.AreEqual(expectedMax, minmax.Item2);
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
            var minmax = input.RangeOfMatchingFloatingPointNumbers(10);
            Assert.AreEqual(expectedMin, minmax.Item1);
            Assert.AreEqual(expectedMax, minmax.Item2);
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
        public void RangeOfMatchingUlpsWithNegativeRelativeDifferenceThrows()
        {
            Assert.That(() => 1d.RangeOfMatchingNumbers(-1d), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        /// <summary>
        /// Range of matching Ulps with value at infinity throws <c>ArgumentOutOfRangeException</c>.
        /// </summary>
        [Test]
        public void RangeOfMatchingUlpsWithValueAtInfinityThrows()
        {
            Assert.That(() => double.PositiveInfinity.RangeOfMatchingNumbers(-1d), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        /// <summary>
        /// Range of matching Ulps with value at Nan throws <c>ArgumentOutOfRangeException</c>.
        /// </summary>
        [Test]
        public void RangeOfMatchingUlpsWithValueAtNaNThrows()
        {
            Assert.That(() => double.NaN.RangeOfMatchingNumbers(-1d), Throws.TypeOf<ArgumentOutOfRangeException>());
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
            var minmax = input.RangeOfMatchingNumbers(relativeDifference);
            Assert.AreEqual(expectedMin, minmax.Item1);
            Assert.AreEqual(expectedMax, minmax.Item2);
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
            var minmax = input.RangeOfMatchingNumbers(relativeDifference);
            Assert.AreEqual(expectedMin, minmax.Item1);
            Assert.AreEqual(expectedMax, minmax.Item2);
        }

        /// <summary>
        /// Test numbers between.
        /// </summary>
        [Test]
        public void NumbersBetween()
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
        public void AlmostEqualWithMaxNumbersBetweenWithLessThanOneNumberThrows()
        {
            Assert.That(() => Precision.AlmostEqualNumbersBetween(1, 2, 0), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void AlmostEqual()
        {
            Assert.That(1.0.AlmostEqual(1.0), "1.0 equals 1.0.");
            Assert.That(1.0.AlmostEqual(1.0 + _doublePrecision), "1.0 equals 1.0 + 2^(-53).");
            Assert.That(1.0.AlmostEqual(1.0 + (_doublePrecision * 5)), "1.0 equals 1.0 + 5*2^(-53).");
            Assert.That(!1.0.AlmostEqual(1.0 + (_doublePrecision * 100)), "1.0 does not equal 1.0 + 2^(-51).");
            Assert.That(!1.0.AlmostEqual(2.0), "1.0 does not equal 2.0");
        }

        [Test]
        public void AlmostEqualRelative()
        {
            // compare zero and negative zero
            Assert.IsTrue(0.0.AlmostEqualRelative(-0.0, 1e-5));
            Assert.IsTrue(0.0.AlmostEqualRelative(-0.0, 1e-15));

            // compare two nearby numbers
            Assert.IsTrue(1.0.AlmostEqualRelative(1.0 + (3 * _doublePrecision), 1e-15));
            Assert.IsTrue(1.0.AlmostEqualRelative(1.0 + _doublePrecision, 1e-15));
            Assert.IsTrue(1.0.AlmostEqualRelative(1.0 + 1e-16, 1e-15));
            Assert.IsFalse(1.0.AlmostEqualRelative(1.0 + 1e-15, 1e-15));
            Assert.IsFalse(1.0.AlmostEqualRelative(1.0 + 1e-14, 1e-15));

            // compare with the two numbers reversed in compare order
            Assert.IsTrue((1.0 + (3 * _doublePrecision)).AlmostEqualRelative(1.0, 1e-15));
            Assert.IsTrue((1.0 + _doublePrecision).AlmostEqualRelative(1.0, 1e-15));
            Assert.IsTrue((1.0 + 1e-16).AlmostEqualRelative(1.0, 1e-15));
            Assert.IsFalse((1.0 + 1e-15).AlmostEqualRelative(1.0, 1e-15));
            Assert.IsFalse((1.0 + 1e-14).AlmostEqualRelative(1.0, 1e-15));

            // compare different numbers
            Assert.IsFalse(2.0.AlmostEqualRelative(1.0, 1e-15));
            Assert.IsFalse(1.0.AlmostEqualRelative(2.0, 1e-15));

            // compare different numbers with large tolerance
            Assert.IsFalse(2.0.AlmostEqualRelative(1.0, 1e-5));
            Assert.IsFalse(1.0.AlmostEqualRelative(2.0, 1e-5));
            Assert.IsTrue(2.0.AlmostEqualRelative(1.0, 1e+1));
            Assert.IsTrue(1.0.AlmostEqualRelative(2.0, 1e+1));

            // compare inf & inf
            Assert.IsTrue(double.PositiveInfinity.AlmostEqualRelative(double.PositiveInfinity, 1e-15));
            Assert.IsTrue(double.NegativeInfinity.AlmostEqualRelative(double.NegativeInfinity, 1e-15));

            // compare -inf and inf
            Assert.IsFalse(double.PositiveInfinity.AlmostEqualRelative(double.NegativeInfinity, 1e-15));
            Assert.IsFalse(double.NegativeInfinity.AlmostEqualRelative(double.PositiveInfinity, 1e-15));

            // compare inf and non-inf
            Assert.IsFalse(double.PositiveInfinity.AlmostEqualRelative(1.0, 1e-15));
            Assert.IsFalse(1.0.AlmostEqualRelative(double.PositiveInfinity, 1e-15));
            Assert.IsFalse(double.NegativeInfinity.AlmostEqualRelative(1.0, 1e-15));
            Assert.IsFalse(1.0.AlmostEqualRelative(double.NegativeInfinity, 1e-15));

            // compare tiny numbers with opposite signs
            Assert.IsFalse(1e-12.AlmostEqualRelative(-1e-12, 1e-14));
            Assert.IsFalse((-1e-12).AlmostEqualRelative(1e-12, 1e-14));

            Assert.IsFalse((-2.0).AlmostEqualRelative(2.0, 1e-14));
            Assert.IsFalse(2.0.AlmostEqualRelative(-2.0, 1e-14));
        }

        [Test]
        public void AlmostEqualNumbersBetween()
        {
            // compare zero and negative zero
            Assert.IsTrue(Precision.AlmostEqualNumbersBetween(0, -0, 1));

            // compare two nearby numbers
            Assert.IsFalse(1.0.AlmostEqualNumbersBetween(1.0 + (3 * _doublePrecision), 1));
            Assert.IsTrue(1.0.AlmostEqualNumbersBetween(1.0 + _doublePrecision, 1));
            Assert.IsTrue(1.0.AlmostEqualNumbersBetween(1.0 - _doublePrecision, 1));
            Assert.IsFalse(1.0.AlmostEqualNumbersBetween(1.0 - (3 * _doublePrecision), 1));

            // compare with the two numbers reversed in compare order
            Assert.IsFalse((1.0 + (3 * _doublePrecision)).AlmostEqualNumbersBetween(1.0, 1));
            Assert.IsTrue((1.0 + _doublePrecision).AlmostEqualNumbersBetween(1.0, 1));
            Assert.IsTrue((1.0 - _doublePrecision).AlmostEqualNumbersBetween(1.0, 1));
            Assert.IsFalse((1.0 - (3 * _doublePrecision)).AlmostEqualNumbersBetween(1.0, 1));

            // compare two slightly more different numbers
            Assert.IsFalse(1.0.AlmostEqualNumbersBetween(1.0 + (10 * _doublePrecision), 1));
            Assert.IsTrue(1.0.AlmostEqualNumbersBetween(1.0 + (10 * _doublePrecision), 10));
            Assert.IsTrue(1.0.AlmostEqualNumbersBetween(1.0 - (10 * _doublePrecision), 10));
            Assert.IsFalse(1.0.AlmostEqualNumbersBetween(1.0 - (10 * _doublePrecision), 1));

            // compare different numbers
            Assert.IsFalse(2.0.AlmostEqualNumbersBetween(1.0, 1));
            Assert.IsFalse(1.0.AlmostEqualNumbersBetween(2.0, 1));

            // compare different numbers with large tolerance
            Assert.IsFalse(1.0.AlmostEqualNumbersBetween(1.0 + (1e5 * _doublePrecision), 1));
            Assert.IsTrue(1.0.AlmostEqualNumbersBetween(1.0 - (1e5 * _doublePrecision), 200000));
            Assert.IsFalse(1.0.AlmostEqualNumbersBetween(1.0 - (1e5 * _doublePrecision), 1));

            // compare inf & inf
            Assert.IsTrue(double.PositiveInfinity.AlmostEqualNumbersBetween(double.PositiveInfinity, 1));
            Assert.IsTrue(double.NegativeInfinity.AlmostEqualNumbersBetween(double.NegativeInfinity, 1));

            // compare -inf and inf
            Assert.IsFalse(double.PositiveInfinity.AlmostEqualNumbersBetween(double.NegativeInfinity, 1));
            Assert.IsFalse(double.NegativeInfinity.AlmostEqualNumbersBetween(double.PositiveInfinity, 1));

            // compare inf and non-inf
            Assert.IsFalse(double.PositiveInfinity.AlmostEqualNumbersBetween(1.0, 1));
            Assert.IsFalse(1.0.AlmostEqualNumbersBetween(double.PositiveInfinity, 1));

            Assert.IsFalse(double.NegativeInfinity.AlmostEqualNumbersBetween(1.0, 1));
            Assert.IsFalse(1.0.AlmostEqualNumbersBetween(double.NegativeInfinity, 1));

            // compare tiny numbers with opposite signs
            Assert.IsFalse(double.Epsilon.AlmostEqualNumbersBetween(-double.Epsilon, 1));
            Assert.IsFalse((-double.Epsilon).AlmostEqualNumbersBetween(double.Epsilon, 1));

            Assert.IsFalse((-2.0).AlmostEqualNumbersBetween(2.0, 1));
            Assert.IsFalse(2.0.AlmostEqualNumbersBetween(-2.0, 1));
        }

        [Test]
        public void AlmostEqualDecimalPlaces()
        {
            Assert.IsFalse(1d.AlmostEqual(double.NaN, 2));
            Assert.IsFalse(double.NaN.AlmostEqual(2d, 2));
            Assert.IsFalse(double.NaN.AlmostEqual(double.NaN, 2));

            Assert.IsFalse(double.NegativeInfinity.AlmostEqual(2, 2));
            Assert.IsFalse(1d.AlmostEqual(double.NegativeInfinity, 2));
            Assert.IsFalse(double.PositiveInfinity.AlmostEqual(2, 2));
            Assert.IsFalse(1d.AlmostEqual(double.PositiveInfinity, 2));
            Assert.IsFalse(double.NegativeInfinity.AlmostEqual(double.PositiveInfinity, 2));
            Assert.IsFalse(double.PositiveInfinity.AlmostEqual(double.NegativeInfinity, 2));
            Assert.IsTrue(double.PositiveInfinity.AlmostEqual(double.PositiveInfinity, 2));
            Assert.IsTrue(double.NegativeInfinity.AlmostEqual(double.NegativeInfinity, 2));

            // 1 -> +/- 0.05 (0.5e-1)
            Assert.IsFalse(1d.AlmostEqual(1.06, 1));
            Assert.IsTrue(1d.AlmostEqual(1.04, 1));
            Assert.IsTrue(1d.AlmostEqual(1.00, 1));
            Assert.IsTrue(1d.AlmostEqual(0.96, 1));
            Assert.IsFalse(1d.AlmostEqual(0.94, 1));
            Assert.IsFalse(1d.AlmostEqual(0.0, 1));

            // -1 -> +/- 5 (0.5e+1)
            Assert.IsFalse(100d.AlmostEqual(106.0, -1));
            Assert.IsTrue(100d.AlmostEqual(104.0, -1));
            Assert.IsTrue(100d.AlmostEqual(100.0, -1));
            Assert.IsTrue(100d.AlmostEqual(96.0, -1));
            Assert.IsFalse(100d.AlmostEqual(94.0, -1));
            Assert.IsFalse(100d.AlmostEqual(0.0, -1));

            // 3 -> +/- 0.0005 (0.5e-3)
            Assert.IsFalse(0.01.AlmostEqual(0.0106, 3));
            Assert.IsTrue(0.01.AlmostEqual(0.0104, 3));
            Assert.IsTrue(0.01.AlmostEqual(0.0100, 3));
            Assert.IsTrue(0.01.AlmostEqual(0.0096, 3));
            Assert.IsFalse(0.01.AlmostEqual(0.0094, 3));
            Assert.IsFalse(0.01.AlmostEqual(0.0, 3));

            // 12 -> +/- 0.5e-12
            Assert.IsTrue(0d.AlmostEqual(4 * _doublePrecision, 12));
            Assert.IsTrue(0d.AlmostEqual(-4 * _doublePrecision, 12));
        }

        /// <summary>
        /// <c>AlmostEqual</c> within relative decimal places.
        /// </summary>
        [Test]
        public void AlmostEqualRelativeDecimalPlaces()
        {
            Assert.IsFalse(1d.AlmostEqualRelative(double.NaN, 2));
            Assert.IsFalse(double.NaN.AlmostEqualRelative(2d, 2));
            Assert.IsFalse(double.NaN.AlmostEqualRelative(double.NaN, 2));

            Assert.IsFalse(double.NegativeInfinity.AlmostEqualRelative(2, 2));
            Assert.IsFalse(1d.AlmostEqualRelative(double.NegativeInfinity, 2));
            Assert.IsFalse(double.PositiveInfinity.AlmostEqualRelative(2, 2));
            Assert.IsFalse(1d.AlmostEqualRelative(double.PositiveInfinity, 2));
            Assert.IsFalse(double.NegativeInfinity.AlmostEqualRelative(double.PositiveInfinity, 2));
            Assert.IsFalse(double.PositiveInfinity.AlmostEqualRelative(double.NegativeInfinity, 2));
            Assert.IsTrue(double.PositiveInfinity.AlmostEqualRelative(double.PositiveInfinity, 2));
            Assert.IsTrue(double.NegativeInfinity.AlmostEqualRelative(double.NegativeInfinity, 2));

            // 1 -> +/- max * 0.05 (0.5e-1)
            Assert.IsTrue(1d.AlmostEqualRelative(1.04, 1));
            Assert.IsFalse(1d.AlmostEqualRelative(1.06, 1));
            Assert.IsTrue(1d.AlmostEqualRelative(0.96, 1));
            Assert.IsFalse(1d.AlmostEqualRelative(0.94, 1));

            // 1 -> +/- max * 0.05 (0.5e-1)
            Assert.IsTrue(100d.AlmostEqualRelative(104.00, 1));
            Assert.IsFalse(100d.AlmostEqualRelative(106.00, 1));
            Assert.IsTrue(100d.AlmostEqualRelative(96.000, 1));
            Assert.IsFalse(100d.AlmostEqualRelative(94.000, 1));

            // 0 -> +/- max * 0.5 (0.5e-0)
            Assert.IsTrue(0.01.AlmostEqualRelative(0.014, 0));
            Assert.IsFalse(0.01.AlmostEqualRelative(0.016, 0));
            Assert.IsTrue(0.01.AlmostEqualRelative(0.006, 0));
            Assert.IsFalse(0.01.AlmostEqualRelative(0.004, 0));

            Assert.IsTrue(0d.AlmostEqualRelative(4 * _doublePrecision, 12));
            Assert.IsTrue(0d.AlmostEqualRelative(-4 * _doublePrecision, 12));
        }

        /// <summary>
        /// <c>AlmostEqual</c> within decimal places with negative decimal places throws <c>ArgumentOutOfRangeException</c>.
        /// </summary>
        [Test]
        public void AlmostEqualRelativeDecimalPlacesWithNegativeDecimalPlacesThrows()
        {
            Assert.That(() => Precision.AlmostEqualRelative(1, 2, -1), Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.DoesNotThrow(() => Precision.AlmostEqual(1, 2, -1));
        }

        /// <summary>
        /// <c>AlmostEqual</c> with small numbers and small number of decimal places.
        /// </summary>
        [Test]
        public void AlmostEqualWithSmallNumbersAndSmallNumberOfDecimalPlaces()
        {
            Assert.IsTrue(0.0.AlmostEqualRelative(1e-12, 1));
            Assert.IsTrue(0.0.AlmostEqualRelative(-1e-12, 1));

            Assert.IsFalse(0.0.AlmostEqualRelative(1e-12, 13));
            Assert.IsFalse(0.0.AlmostEqualRelative(-1e-12, 13));

            Assert.IsFalse(0.0.AlmostEqualRelative(1, 1));
            Assert.IsFalse(0.0.AlmostEqualRelative(-1, 1));
        }

        /// <summary>
        /// <c>AlmostEqual</c> with decimal places with sign revert.
        /// </summary>
        [Test]
        public void AlmostEqualWithDecimalPlacesWithSignRevert()
        {
            Assert.IsFalse(0.5.AlmostEqualRelative(0.3, 1));
        }

        /// <summary>
        /// Is larger with max numbers between.
        /// </summary>
        [Test]
        public void IsLargerWithMaxNumbersBetween()
        {
            // compare zero and negative zero
            Assert.IsFalse(Precision.IsLargerNumbersBetween(0, -0, 1));

            // compare two nearby numbers
            Assert.IsFalse(1.0.IsLargerNumbersBetween(1.0 + (3 * _doublePrecision), 1));
            Assert.IsFalse(1.0.IsLargerNumbersBetween(1.0 + _doublePrecision, 1));
            Assert.IsFalse(1.0.IsLargerNumbersBetween(1.0 - _doublePrecision, 1));
            Assert.IsTrue(1.0.IsLargerNumbersBetween(1.0 - (3 * _doublePrecision), 1));

            // compare with the two numbers reversed in compare order
            Assert.IsTrue((1.0 + (3 * _doublePrecision)).IsLargerNumbersBetween(1.0, 1));
            Assert.IsFalse((1.0 + _doublePrecision).IsLargerNumbersBetween(1.0, 1));
            Assert.IsFalse((1.0 - _doublePrecision).IsLargerNumbersBetween(1.0, 1));
            Assert.IsFalse((1.0 - (3 * _doublePrecision)).IsLargerNumbersBetween(1.0, 1));

            // compare two slightly more different numbers
            Assert.IsFalse(1.0.IsLargerNumbersBetween(1.0 + (10 * _doublePrecision), 1));
            Assert.IsFalse(1.0.IsLargerNumbersBetween(1.0 + (10 * _doublePrecision), 10));
            Assert.IsFalse(1.0.IsLargerNumbersBetween(1.0 - (10 * _doublePrecision), 10));
            Assert.IsTrue(1.0.IsLargerNumbersBetween(1.0 - (10 * _doublePrecision), 1));

            // compare different numbers
            Assert.IsTrue(2.0.IsLargerNumbersBetween(1.0, 1));
            Assert.IsFalse(1.0.IsLargerNumbersBetween(2.0, 1));

            // compare different numbers with large tolerance
            Assert.IsFalse(1.0.IsLargerNumbersBetween(1.0 + (1e5 * _doublePrecision), 1));
            Assert.IsFalse(1.0.IsLargerNumbersBetween(1.0 - (1e5 * _doublePrecision), 200000));
            Assert.IsTrue(1.0.IsLargerNumbersBetween(1.0 - (1e5 * _doublePrecision), 1));

            // compare inf & inf
            Assert.IsFalse(double.PositiveInfinity.IsLargerNumbersBetween(double.PositiveInfinity, 1));
            Assert.IsFalse(double.NegativeInfinity.IsLargerNumbersBetween(double.NegativeInfinity, 1));

            // compare -inf and inf
            Assert.IsTrue(double.PositiveInfinity.IsLargerNumbersBetween(double.NegativeInfinity, 1));
            Assert.IsFalse(double.NegativeInfinity.IsLargerNumbersBetween(double.PositiveInfinity, 1));

            // compare inf and non-inf
            Assert.IsTrue(double.PositiveInfinity.IsLargerNumbersBetween(1.0, 1));
            Assert.IsFalse(1.0.IsLargerNumbersBetween(double.PositiveInfinity, 1));

            Assert.IsFalse(double.NegativeInfinity.IsLargerNumbersBetween(1.0, 1));
            Assert.IsTrue(1.0.IsLargerNumbersBetween(double.NegativeInfinity, 1));

            // compare tiny numbers with opposite signs
            Assert.IsTrue(double.Epsilon.IsLargerNumbersBetween(-double.Epsilon, 1));
            Assert.IsFalse((-double.Epsilon).IsLargerNumbersBetween(double.Epsilon, 1));
        }

        /// <summary>
        /// Is larger with decimal places.
        /// </summary>
        [Test]
        public void IsLargerWithDecimalPlaces()
        {
            Assert.IsFalse(Precision.IsLarger(1, double.NaN, 2));
            Assert.IsFalse(double.NaN.IsLarger(2, 2));
            Assert.IsFalse(double.NaN.IsLarger(double.NaN, 2));

            Assert.IsFalse(double.NegativeInfinity.IsLarger(2, 2));
            Assert.IsTrue(Precision.IsLarger(1, double.NegativeInfinity, 2));

            Assert.IsTrue(double.PositiveInfinity.IsLarger(2, 2));
            Assert.IsFalse(Precision.IsLarger(1, double.PositiveInfinity, 2));

            Assert.IsFalse(double.NegativeInfinity.IsLarger(double.PositiveInfinity, 2));
            Assert.IsTrue(double.PositiveInfinity.IsLarger(double.NegativeInfinity, 2));

            Assert.IsFalse(double.PositiveInfinity.IsLarger(double.PositiveInfinity, 2));
            Assert.IsFalse(double.NegativeInfinity.IsLarger(double.NegativeInfinity, 2));

            Assert.IsFalse(1.0.IsLarger(1.006, 2));
            Assert.IsFalse(1.0.IsLarger(1.004, 2));
            Assert.IsFalse(1.0.IsLarger(0.996, 2));
            Assert.IsTrue(1.0.IsLarger(0.994, 2));

            Assert.IsFalse(1.0.IsLargerRelative(1.006, 2));
            Assert.IsFalse(1.0.IsLargerRelative(1.004, 2));
            Assert.IsFalse(1.0.IsLargerRelative(0.996, 2));
            Assert.IsTrue(1.0.IsLargerRelative(0.994, 2));

            Assert.IsFalse(100.0.IsLargerRelative(100.6, 2));
            Assert.IsFalse(100.0.IsLargerRelative(100.4, 2));
            Assert.IsFalse(100.0.IsLargerRelative(99.6, 2));
            Assert.IsTrue(100.0.IsLargerRelative(99.4, 2));

            var max = 0.4 * Math.Pow(10, _doublePrecision.Magnitude());
            Assert.IsFalse(0.0.IsLarger(max, -_doublePrecision.Magnitude()));
            Assert.IsFalse(0.0.IsLarger(-max, -_doublePrecision.Magnitude()));

            max = 0.6 * Math.Pow(10, _doublePrecision.Magnitude());
            Assert.IsFalse(0.0.IsLarger(max, -_doublePrecision.Magnitude()));
            Assert.IsTrue(0.0.IsLarger(-max, -_doublePrecision.Magnitude()));
        }

        /// <summary>
        /// Is smaller with max numbers between.
        /// </summary>
        [Test]
        public void IsSmallerWithMaxNumbersBetween()
        {
            // compare zero and negative zero
            Assert.IsFalse(Precision.IsSmallerNumbersBetween(0, -0, 1));

            // compare two nearby numbers
            Assert.IsTrue(1.0.IsSmallerNumbersBetween(1.0 + (3 * _doublePrecision), 1));
            Assert.IsFalse(1.0.IsSmallerNumbersBetween(1.0 + _doublePrecision, 1));
            Assert.IsFalse(1.0.IsSmallerNumbersBetween(1.0 - _doublePrecision, 1));
            Assert.IsFalse(1.0.IsSmallerNumbersBetween(1.0 - (3 * _doublePrecision), 1));

            // compare with the two numbers reversed in compare order
            Assert.IsFalse((1.0 + (3 * _doublePrecision)).IsSmallerNumbersBetween(1.0, 1));
            Assert.IsFalse((1.0 + _doublePrecision).IsSmallerNumbersBetween(1.0, 1));
            Assert.IsFalse((1.0 - _doublePrecision).IsSmallerNumbersBetween(1.0, 1));
            Assert.IsTrue((1.0 - (3 * _doublePrecision)).IsSmallerNumbersBetween(1.0, 1));

            // compare two slightly more different numbers
            Assert.IsTrue(1.0.IsSmallerNumbersBetween(1.0 + (10 * _doublePrecision), 1));
            Assert.IsFalse(1.0.IsSmallerNumbersBetween(1.0 + (10 * _doublePrecision), 10));
            Assert.IsFalse(1.0.IsSmallerNumbersBetween(1.0 - (10 * _doublePrecision), 10));
            Assert.IsFalse(1.0.IsSmallerNumbersBetween(1.0 - (10 * _doublePrecision), 1));

            // compare different numbers
            Assert.IsFalse(2.0.IsSmallerNumbersBetween(1.0, 1));
            Assert.IsTrue(1.0.IsSmallerNumbersBetween(2.0, 1));

            // compare different numbers with large tolerance
            Assert.IsTrue(1.0.IsSmallerNumbersBetween(1.0 + (1e5 * _doublePrecision), 1));
            Assert.IsFalse(1.0.IsSmallerNumbersBetween(1.0 - (1e5 * _doublePrecision), 200000));
            Assert.IsFalse(1.0.IsSmallerNumbersBetween(1.0 - (1e5 * _doublePrecision), 1));

            // compare inf & inf
            Assert.IsFalse(double.PositiveInfinity.IsSmallerNumbersBetween(double.PositiveInfinity, 1));
            Assert.IsFalse(double.NegativeInfinity.IsSmallerNumbersBetween(double.NegativeInfinity, 1));

            // compare -inf and inf
            Assert.IsFalse(double.PositiveInfinity.IsSmallerNumbersBetween(double.NegativeInfinity, 1));
            Assert.IsTrue(double.NegativeInfinity.IsSmallerNumbersBetween(double.PositiveInfinity, 1));

            // compare inf and non-inf
            Assert.IsFalse(double.PositiveInfinity.IsSmallerNumbersBetween(1.0, 1));
            Assert.IsTrue(1.0.IsSmallerNumbersBetween(double.PositiveInfinity, 1));

            Assert.IsTrue(double.NegativeInfinity.IsSmallerNumbersBetween(1.0, 1));
            Assert.IsFalse(1.0.IsSmallerNumbersBetween(double.NegativeInfinity, 1));

            // compare tiny numbers with opposite signs
            Assert.IsFalse(double.Epsilon.IsSmallerNumbersBetween(-double.Epsilon, 1));
            Assert.IsTrue((-double.Epsilon).IsSmallerNumbersBetween(double.Epsilon, 1));
        }

        /// <summary>
        /// Is smaller with decimal places.
        /// </summary>
        [Test]
        public void IsSmallerWithDecimalPlaces()
        {
            Assert.IsFalse(Precision.IsSmaller(1, double.NaN, 2));
            Assert.IsFalse(double.NaN.IsSmaller(2, 2));
            Assert.IsFalse(double.NaN.IsSmaller(double.NaN, 2));

            Assert.IsTrue(double.NegativeInfinity.IsSmaller(2, 2));
            Assert.IsFalse(Precision.IsSmaller(1, double.NegativeInfinity, 2));

            Assert.IsFalse(double.PositiveInfinity.IsSmaller(2, 2));
            Assert.IsTrue(Precision.IsSmaller(1, double.PositiveInfinity, 2));

            Assert.IsTrue(double.NegativeInfinity.IsSmaller(double.PositiveInfinity, 2));
            Assert.IsFalse(double.PositiveInfinity.IsSmaller(double.NegativeInfinity, 2));

            Assert.IsFalse(double.PositiveInfinity.IsSmaller(double.PositiveInfinity, 2));
            Assert.IsFalse(double.NegativeInfinity.IsSmaller(double.NegativeInfinity, 2));

            Assert.IsTrue(1.0.IsSmaller(1.006, 2));
            Assert.IsFalse(1.0.IsSmaller(1.004, 2));
            Assert.IsFalse(1.0.IsSmaller(0.996, 2));
            Assert.IsFalse(1.0.IsSmaller(0.994, 2));

            Assert.IsTrue(1.0.IsSmallerRelative(1.006, 2));
            Assert.IsFalse(1.0.IsSmallerRelative(1.004, 2));
            Assert.IsFalse(1.0.IsSmallerRelative(0.996, 2));
            Assert.IsFalse(1.0.IsSmallerRelative(0.994, 2));

            Assert.IsTrue(100.0.IsSmallerRelative(100.6, 2));
            Assert.IsFalse(100.0.IsSmallerRelative(100.4, 2));
            Assert.IsFalse(100.0.IsSmallerRelative(99.6, 2));
            Assert.IsFalse(100.0.IsSmallerRelative(99.4, 2));

            var max = 0.4 * Math.Pow(10, _doublePrecision.Magnitude());
            Assert.IsFalse(0.0.IsSmaller(max, -_doublePrecision.Magnitude()));
            Assert.IsFalse(0.0.IsSmaller(-max, -_doublePrecision.Magnitude()));

            max = 0.6 * Math.Pow(10, _doublePrecision.Magnitude());
            Assert.IsTrue(0.0.IsSmaller(max, -_doublePrecision.Magnitude()));
            Assert.IsFalse(0.0.IsSmaller(-max, -_doublePrecision.Magnitude()));
        }

        /// <summary>
        /// Compare to with max numbers between with negative number throws <c>ArgumentOutOfRangeException</c>.
        /// </summary>
        [Test]
        public void CompareToWithMaxNumbersBetweenWithNegativeNumberThrows()
        {
            const double Value = 10.0;
            Assert.That(() => Value.CompareToNumbersBetween(Value, -1), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        /// <summary>
        /// Compare to with max numbers between with zero number throws <c>ArgumentOutOfRangeException</c>.
        /// </summary>
        [Test]
        public void CompareToWithMaxNumbersBetweenWithZeroNumberThrows()
        {
            const double Value = 10.0;
            Assert.That(() => Value.CompareToNumbersBetween(Value, 0), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        /// <summary>
        /// Compare to with max numbers between with infinity number.
        /// </summary>
        [Test]
        public void CompareToWithMaxNumbersBetweenWithInfinityValue()
        {
            Assert.AreEqual(0, double.PositiveInfinity.CompareToNumbersBetween(double.PositiveInfinity, 1));
            Assert.AreEqual(0, double.NegativeInfinity.CompareToNumbersBetween(double.NegativeInfinity, 1));

            Assert.AreEqual(1, double.PositiveInfinity.CompareToNumbersBetween(double.NegativeInfinity, 1));
            Assert.AreEqual(-1, double.NegativeInfinity.CompareToNumbersBetween(double.PositiveInfinity, 1));

            Assert.AreEqual(-1, Precision.CompareToNumbersBetween(1, double.PositiveInfinity, 1));
            Assert.AreEqual(1, double.PositiveInfinity.CompareToNumbersBetween(1, 1));

            Assert.AreEqual(1, Precision.CompareToNumbersBetween(1, double.NegativeInfinity, 1));
            Assert.AreEqual(-1, double.NegativeInfinity.CompareToNumbersBetween(1, 1));
        }

        /// <summary>
        /// Compare to with max numbers between with NaN value.
        /// </summary>
        [Test]
        public void CompareToWithMaxNumbersBetweenWithNaNValue()
        {
            // compare nan & not-nan
            Assert.AreEqual(1, Precision.CompareToNumbersBetween(1, double.NaN, 1));
            Assert.AreEqual(0, double.NaN.CompareToNumbersBetween(double.NaN, 1));
            Assert.AreEqual(-1, double.NaN.CompareToNumbersBetween(1, 1));
        }

        /// <summary>
        /// Compare to with max numbers between for values overflow a long.
        /// </summary>
        [Test]
        public void CompareToWithMaxNumbersBetweenForValuesThatOverFlowALong()
        {
            Assert.AreEqual(1, 2.0.CompareToNumbersBetween(-2.0, 1));
            Assert.AreEqual(-1, (-2.0).CompareToNumbersBetween(2.0, 1));
        }

        /// <summary>
        /// Compare to with max numbers between.
        /// </summary>
        [Test]
        public void CompareToWithMaxNumbersBetween()
        {
            // compare zero and negative zero
            Assert.AreEqual(0, Precision.CompareToNumbersBetween(0, -0, 1));

            // compare two nearby numbers
            Assert.AreEqual(-1, 1.0.CompareToNumbersBetween(1.0 + (3 * _doublePrecision), 1));
            Assert.AreEqual(0, 1.0.CompareToNumbersBetween(1.0 + _doublePrecision, 1));
            Assert.AreEqual(0, 1.0.CompareToNumbersBetween(1.0 - _doublePrecision, 1));
            Assert.AreEqual(1, 1.0.CompareToNumbersBetween(1.0 - (3 * _doublePrecision), 1));

            // compare with the two numbers reversed in compare order
            Assert.AreEqual(1, (1.0 + (3 * _doublePrecision)).CompareToNumbersBetween(1.0, 1));
            Assert.AreEqual(0, (1.0 + _doublePrecision).CompareToNumbersBetween(1.0, 1));
            Assert.AreEqual(0, (1.0 - _doublePrecision).CompareToNumbersBetween(1.0, 1));
            Assert.AreEqual(-1, (1.0 - (3 * _doublePrecision)).CompareToNumbersBetween(1.0, 1));

            // compare two slightly more different numbers
            Assert.AreEqual(-1, 1.0.CompareToNumbersBetween(1.0 + (10 * _doublePrecision), 1));
            Assert.AreEqual(0, 1.0.CompareToNumbersBetween(1.0 + (10 * _doublePrecision), 10));
            Assert.AreEqual(0, 1.0.CompareToNumbersBetween(1.0 - (10 * _doublePrecision), 10));
            Assert.AreEqual(1, 1.0.CompareToNumbersBetween(1.0 - (10 * _doublePrecision), 1));

            // compare different numbers
            Assert.AreEqual(1, 2.0.CompareToNumbersBetween(1.0, 1));
            Assert.AreEqual(-1, 1.0.CompareToNumbersBetween(2.0, 1));

            // compare different numbers with large tolerance
            Assert.AreEqual(-1, 1.0.CompareToNumbersBetween(1.0 + (1e5 * _doublePrecision), 1));
            Assert.AreEqual(0, 1.0.CompareToNumbersBetween(1.0 - (1e5 * _doublePrecision), 200000));
            Assert.AreEqual(1, 1.0.CompareToNumbersBetween(1.0 - (1e5 * _doublePrecision), 1));

            // compare inf & inf
            Assert.AreEqual(0, double.PositiveInfinity.CompareToNumbersBetween(double.PositiveInfinity, 1));
            Assert.AreEqual(0, double.NegativeInfinity.CompareToNumbersBetween(double.NegativeInfinity, 1));

            // compare -inf and inf
            Assert.AreEqual(1, double.PositiveInfinity.CompareToNumbersBetween(double.NegativeInfinity, 1));
            Assert.AreEqual(-1, double.NegativeInfinity.CompareToNumbersBetween(double.PositiveInfinity, 1));

            // compare inf and non-inf
            Assert.AreEqual(1, double.PositiveInfinity.CompareToNumbersBetween(1.0, 1));
            Assert.AreEqual(-1, 1.0.CompareToNumbersBetween(double.PositiveInfinity, 1));

            Assert.AreEqual(-1, double.NegativeInfinity.CompareToNumbersBetween(1.0, 1));
            Assert.AreEqual(1, 1.0.CompareToNumbersBetween(double.NegativeInfinity, 1));

            // compare tiny numbers with opposite signs
            Assert.AreEqual(1, double.Epsilon.CompareToNumbersBetween(-double.Epsilon, 1));
            Assert.AreEqual(-1, (-double.Epsilon).CompareToNumbersBetween(double.Epsilon, 1));
        }

        /// <summary>
        /// Compare to with decimal places.
        /// </summary>
        [Test]
        public void CompareToWithDecimalPlaces()
        {
            // compare zero and negative zero
            Assert.AreEqual(0, Precision.CompareTo(0, -0, 1));
            Assert.AreEqual(0, Precision.CompareTo(0, -0, Precision.DoubleDecimalPlaces));
            Assert.AreEqual(0, Precision.CompareTo(0, -0, Precision.SingleDecimalPlaces));

            // compare two nearby numbers
            Assert.AreEqual(-1, 1.0.CompareTo(1.0 + 10*_doublePrecision, Precision.DoubleDecimalPlaces));
            Assert.AreEqual(0, 1.0.CompareTo(1.0 + _doublePrecision, Precision.DoubleDecimalPlaces));
            Assert.AreEqual(0, 1.0.CompareTo(1.0 - _doublePrecision, Precision.DoubleDecimalPlaces));
            Assert.AreEqual(1, 1.0.CompareTo(1.0 - 10*_doublePrecision, Precision.DoubleDecimalPlaces));

            // compare with the two numbers reversed in compare order
            Assert.AreEqual(1, (1.0 + 10*_doublePrecision).CompareTo(1.0, Precision.DoubleDecimalPlaces));
            Assert.AreEqual(0, (1.0 + _doublePrecision).CompareTo(1.0, Precision.DoubleDecimalPlaces));
            Assert.AreEqual(0, (1.0 - _doublePrecision).CompareTo(1.0, Precision.DoubleDecimalPlaces));
            Assert.AreEqual(-1, (1.0 - 10*_doublePrecision).CompareTo(1.0, Precision.DoubleDecimalPlaces));

            // compare two slightly more different numbers
            Assert.AreEqual(-1, 1.0.CompareTo(1.0 + (50*_doublePrecision), Precision.DoubleDecimalPlaces));
            Assert.AreEqual(0, 1.0.CompareTo(1.0 + (50*_doublePrecision), Precision.DoubleDecimalPlaces - 2));
            Assert.AreEqual(0, 1.0.CompareTo(1.0 - (50*_doublePrecision), Precision.DoubleDecimalPlaces - 2));
            Assert.AreEqual(1, 1.0.CompareTo(1.0 - (50*_doublePrecision), Precision.DoubleDecimalPlaces));

            // compare different numbers
            Assert.AreEqual(1, 2.0.CompareTo(1.0, Precision.DoubleDecimalPlaces));
            Assert.AreEqual(-1, 1.0.CompareTo(2.0, Precision.DoubleDecimalPlaces));

            // compare different numbers with large tolerance
            Assert.AreEqual(-1, 1.0.CompareTo(1.0 + (1e5 * _doublePrecision), Precision.DoubleDecimalPlaces));
            Assert.AreEqual(0, 1.0.CompareTo(1.0 - (1e5 * _doublePrecision), 10));
            Assert.AreEqual(1, 1.0.CompareTo(1.0 - (1e5 * _doublePrecision), Precision.DoubleDecimalPlaces));

            // compare inf & inf
            Assert.AreEqual(0, double.PositiveInfinity.CompareTo(double.PositiveInfinity, Precision.DoubleDecimalPlaces));
            Assert.AreEqual(0, double.NegativeInfinity.CompareTo(double.NegativeInfinity, Precision.DoubleDecimalPlaces));

            // compare -inf and inf
            Assert.AreEqual(1, double.PositiveInfinity.CompareTo(double.NegativeInfinity, Precision.DoubleDecimalPlaces));
            Assert.AreEqual(-1, double.NegativeInfinity.CompareTo(double.PositiveInfinity, Precision.DoubleDecimalPlaces));

            // compare inf and non-inf
            Assert.AreEqual(1, double.PositiveInfinity.CompareTo(1.0, Precision.DoubleDecimalPlaces));
            Assert.AreEqual(-1, 1.0.CompareTo(double.PositiveInfinity, Precision.DoubleDecimalPlaces));
            Assert.AreEqual(-1, double.NegativeInfinity.CompareTo(1.0, Precision.DoubleDecimalPlaces));
            Assert.AreEqual(1, 1.0.CompareTo(double.NegativeInfinity, Precision.DoubleDecimalPlaces));
        }
    }
}
