// <copyright file="FactorialTest.cs" company="Math.NET">
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

namespace MathNet.Numerics.UnitTests.SpecialFunctionsTests
{
    /// <summary>
    /// Factorial tests.
    /// </summary>
    [TestFixture, Category("Functions")]
    public class FactorialTest
    {
        /// <summary>
        /// Can compute factorial.
        /// </summary>
        [Test]
        public void CanComputeFactorial()
        {
            // exact
            var factorial = 1.0;
            for (var i = 1; i < 23; i++)
            {
                factorial *= i;
                AssertHelpers.AlmostEqualRelative(factorial, SpecialFunctions.Factorial(i), 14);
                AssertHelpers.AlmostEqualRelative(Math.Log(factorial), SpecialFunctions.FactorialLn(i), 14);
            }

            // approximation
            for (var i = 23; i < 171; i++)
            {
                factorial *= i;
                AssertHelpers.AlmostEqualRelative(factorial, SpecialFunctions.Factorial(i), 14);
                AssertHelpers.AlmostEqualRelative(Math.Log(factorial), SpecialFunctions.FactorialLn(i), 14);
            }
        }

        /// <summary>
        /// Throws <c>ArgumentOutOfRangeException</c> on a negative argument.
        /// </summary>
        [Test]
        public void ThrowsOnNegativeArgument()
        {
            Assert.That(() => SpecialFunctions.Factorial(Int32.MinValue), Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => SpecialFunctions.Factorial(-1), Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => SpecialFunctions.FactorialLn(-1), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        /// <summary>
        /// Factorial overflows to infinity.
        /// </summary>
        [Test]
        public void FactorialOverflowsToInfinity()
        {
            Assert.AreEqual(Double.PositiveInfinity, SpecialFunctions.Factorial(172));
            Assert.AreEqual(Double.PositiveInfinity, SpecialFunctions.Factorial(Int32.MaxValue));
        }

        /// <summary>
        /// Log factorial does not overflows.
        /// </summary>
        [Test]
        public void FactorialLnDoesNotOverflow()
        {
            AssertHelpers.AlmostEqualRelative(6078.2118847500501140, SpecialFunctions.FactorialLn(1 << 10), 14);
            AssertHelpers.AlmostEqualRelative(29978.648060844048236, SpecialFunctions.FactorialLn(1 << 12), 14);
            AssertHelpers.AlmostEqualRelative(307933.81973375485425, SpecialFunctions.FactorialLn(1 << 15), 14);
            AssertHelpers.AlmostEqualRelative(1413421.9939462073242, SpecialFunctions.FactorialLn(1 << 17), 14);
        }

        /// <summary>
        /// Can compute binomial.
        /// </summary>
        [Test]
        public void CanComputeBinomial()
        {
            AssertHelpers.AlmostEqualRelative(1, SpecialFunctions.Binomial(1, 1), 14);
            AssertHelpers.AlmostEqualRelative(10, SpecialFunctions.Binomial(5, 2), 14);
            AssertHelpers.AlmostEqualRelative(35, SpecialFunctions.Binomial(7, 3), 14);
            AssertHelpers.AlmostEqualRelative(1, SpecialFunctions.Binomial(1, 0), 14);
            AssertHelpers.AlmostEqualRelative(0, SpecialFunctions.Binomial(0, 1), 14);
            AssertHelpers.AlmostEqualRelative(0, SpecialFunctions.Binomial(5, 7), 14);
            AssertHelpers.AlmostEqualRelative(0, SpecialFunctions.Binomial(5, -7), 14);
        }

        /// <summary>
        /// Can compute log binomial.
        /// </summary>
        [Test]
        public void CanComputeBinomialLn()
        {
            AssertHelpers.AlmostEqualRelative(Math.Log(1), SpecialFunctions.BinomialLn(1, 1), 14);
            AssertHelpers.AlmostEqualRelative(Math.Log(10), SpecialFunctions.BinomialLn(5, 2), 14);
            AssertHelpers.AlmostEqualRelative(Math.Log(35), SpecialFunctions.BinomialLn(7, 3), 14);
            AssertHelpers.AlmostEqualRelative(Math.Log(1), SpecialFunctions.BinomialLn(1, 0), 14);
            AssertHelpers.AlmostEqualRelative(Math.Log(0), SpecialFunctions.BinomialLn(0, 1), 14);
            AssertHelpers.AlmostEqualRelative(Math.Log(0), SpecialFunctions.BinomialLn(5, 7), 14);
            AssertHelpers.AlmostEqualRelative(Math.Log(0), SpecialFunctions.BinomialLn(5, -7), 14);
        }

        /// <summary>
        /// Can compute multinomial.
        /// </summary>
        [Test]
        public void CanComputeMultinomial()
        {
            AssertHelpers.AlmostEqualRelative(1, SpecialFunctions.Multinomial(1, new[] { 1, 0 }), 14);
            AssertHelpers.AlmostEqualRelative(10, SpecialFunctions.Multinomial(5, new[] { 3, 2 }), 14);
            AssertHelpers.AlmostEqualRelative(10, SpecialFunctions.Multinomial(5, new[] { 2, 3 }), 14);
            AssertHelpers.AlmostEqualRelative(35, SpecialFunctions.Multinomial(7, new[] { 3, 4 }), 14);
        }
    }
}
