// <copyright file="StabilityTests.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// https://numerics.mathdotnet.com
// https://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-$CURRENT_YEAR$ Math.NET
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

using NUnit.Framework;

namespace MathNet.Numerics.Tests.SpecialFunctionsTests
{
    /// <summary>
    /// Special functions tests.
    /// </summary>
    [TestFixture, Category("Functions")]
    public class StabilityTests
    {
        /// <summary>
        /// Hypotenuse function (double).
        /// </summary>
        /// <param name="x">Input X value.</param>
        /// <param name="y">Input Y value.</param>
        /// <param name="f">Function result.</param>
        [TestCase(double.NaN, 0, double.NaN)]
        [TestCase(0, double.NaN, double.NaN)]
        [TestCase(double.NaN, double.NaN, double.NaN)]
        [TestCase(double.NegativeInfinity, double.NaN, double.NaN)]
        [TestCase(double.NaN, double.NegativeInfinity, double.NaN)]
        [TestCase(double.PositiveInfinity, double.NaN, double.NaN)]
        [TestCase(double.NaN, double.PositiveInfinity, double.NaN)]
        [TestCase(double.NegativeInfinity, 0, double.PositiveInfinity)]
        [TestCase(double.NegativeInfinity, double.NegativeInfinity, double.PositiveInfinity)]
        [TestCase(double.NegativeInfinity, double.PositiveInfinity, double.PositiveInfinity)]
        [TestCase(0, double.NegativeInfinity, double.PositiveInfinity)]
        [TestCase(double.PositiveInfinity, 0, double.PositiveInfinity)]
        [TestCase(double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity)]
        [TestCase(double.PositiveInfinity, double.NegativeInfinity, double.PositiveInfinity)]
        [TestCase(0, double.NegativeInfinity, double.PositiveInfinity)]
        [TestCase(0, double.MaxValue, double.MaxValue)]
        [TestCase(double.MaxValue, 0, double.MaxValue)]
        [TestCase(0, -double.MinValue, -double.MinValue)]
        [TestCase(0, double.MinValue, -double.MinValue)]
        [TestCase(-double.MinValue, 0, -double.MinValue)]
        [TestCase(double.MinValue, 0, -double.MinValue)]
        [TestCase(0, double.Epsilon, double.Epsilon)]
        [TestCase(0, -double.Epsilon, double.Epsilon)]
        [TestCase(double.Epsilon, 0, double.Epsilon)]
        [TestCase(-double.Epsilon, 0, double.Epsilon)]
        [TestCase(0, 0, 0)]
        [TestCase(0, 7, 7)]
        [TestCase(0, -7, 7)]
        [TestCase(7, 0, 7)]
        [TestCase(-7, 0, 7)]
        [TestCase(3, 4, 5)]
        [TestCase(3, -4, 5)]
        [TestCase(-3, 4, 5)]
        [TestCase(-3, -4, 5)]
        [TestCase(4, 3, 5)]
        [TestCase(4, -3, 5)]
        [TestCase(-4, 3, 5)]
        [TestCase(-4, -3, 5)]
        [TestCase(1, 2.98023223876953125E-8, 1.000000000000000444089210)] // TwoToThePowerOfMinus25 = 2.98023223876953125E-8;
        [TestCase(1, -2.98023223876953125E-8, 1.000000000000000444089210)]
        [TestCase(-1, 2.98023223876953125E-8, 1.000000000000000444089210)]
        [TestCase(-1, -2.98023223876953125E-8, 1.000000000000000444089210)]
        [TestCase(2.98023223876953125E-8, 1, 1.000000000000000444089210)]
        [TestCase(2.98023223876953125E-8, -1, 1.000000000000000444089210)]
        [TestCase(-2.98023223876953125E-8, 1, 1.000000000000000444089210)]
        [TestCase(-2.98023223876953125E-8, -1, 1.000000000000000444089210)]
        public void Hypotenuse(double x, double y, double f)
        {
            var h = SpecialFunctions.Hypotenuse(x, y);
            if (double.IsNaN(f))
            {
                Assert.IsTrue(double.IsNaN(h));
            }
            else
            {
                Assert.AreEqual(f, h);
            }
        }

        /// <summary>
        /// Hypotenuse function (float).
        /// </summary>
        /// <param name="x">Input X value.</param>
        /// <param name="y">Input Y value.</param>
        /// <param name="f">Function result.</param>
        [TestCase(float.NaN, 0, float.NaN)]
        [TestCase(0, float.NaN, float.NaN)]
        [TestCase(float.NaN, float.NaN, float.NaN)]
        [TestCase(float.NegativeInfinity, float.NaN, float.NaN)]
        [TestCase(float.NaN, float.NegativeInfinity, float.NaN)]
        [TestCase(float.PositiveInfinity, float.NaN, float.NaN)]
        [TestCase(float.NaN, float.PositiveInfinity, float.NaN)]
        [TestCase(float.NegativeInfinity, 0, float.PositiveInfinity)]
        [TestCase(float.NegativeInfinity, float.NegativeInfinity, float.PositiveInfinity)]
        [TestCase(float.NegativeInfinity, float.PositiveInfinity, float.PositiveInfinity)]
        [TestCase(0, float.NegativeInfinity, float.PositiveInfinity)]
        [TestCase(float.PositiveInfinity, 0, float.PositiveInfinity)]
        [TestCase(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity)]
        [TestCase(float.PositiveInfinity, float.NegativeInfinity, float.PositiveInfinity)]
        [TestCase(0, float.NegativeInfinity, float.PositiveInfinity)]
        [TestCase(0, float.MaxValue, float.MaxValue)]
        [TestCase(float.MaxValue, 0, float.MaxValue)]
        [TestCase(0, -float.MinValue, -float.MinValue)]
        [TestCase(0, float.MinValue, -float.MinValue)]
        [TestCase(-float.MinValue, 0, -float.MinValue)]
        [TestCase(float.MinValue, 0, -float.MinValue)]
        [TestCase(0, float.Epsilon, float.Epsilon)]
        [TestCase(0, -float.Epsilon, float.Epsilon)]
        [TestCase(float.Epsilon, 0, float.Epsilon)]
        [TestCase(-float.Epsilon, 0, float.Epsilon)]
        [TestCase(0, 0, 0)]
        [TestCase(0, 7, 7)]
        [TestCase(0, -7, 7)]
        [TestCase(7, 0, 7)]
        [TestCase(-7, 0, 7)]
        [TestCase(3, 4, 5)]
        [TestCase(3, -4, 5)]
        [TestCase(-3, 4, 5)]
        [TestCase(-3, -4, 5)]
        [TestCase(4, 3, 5)]
        [TestCase(4, -3, 5)]
        [TestCase(-4, 3, 5)]
        [TestCase(-4, -3, 5)]
        [TestCase(1, 0.00048828125f, 1.000000119209282445354739f)] // TwoToThePowerOfMinus11 = 0.00048828125f
        [TestCase(1, -0.00048828125f, 1.000000119209282445354739f)]
        [TestCase(-1, 0.00048828125f, 1.000000119209282445354739f)]
        [TestCase(-1, -0.00048828125f, 1.000000119209282445354739f)]
        [TestCase(0.00048828125f, 1, 1.000000119209282445354739f)]
        [TestCase(0.00048828125f, -1, 1.000000119209282445354739f)]
        [TestCase(-0.00048828125f, 1, 1.000000119209282445354739f)]
        [TestCase(-0.00048828125f, -1, 1.000000119209282445354739f)]
        public void Hypotenuse(float x, float y, float f)
        {
            var h = SpecialFunctions.Hypotenuse(x, y);
            if (float.IsNaN(f))
            {
                Assert.IsTrue(float.IsNaN(h));
            }
            else
            {
                Assert.AreEqual(f, h);
            }
        }
    }
}
