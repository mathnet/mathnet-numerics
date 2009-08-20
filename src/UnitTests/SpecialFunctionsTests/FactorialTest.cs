// <copyright file="FactorialTest.cs" company="Math.NET">
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

namespace MathNet.Numerics.UnitTests.SpecialFunctionsTests
{
    using System;
    using MbUnit.Framework;

    [TestFixture]
    public class FactorialTest
    {
        [Test]
        public void CanComputeFactorial()
        {
            // exact
            double factorial = 1.0;
            for (int i = 1; i < 23; i++)
            {
                factorial *= i;
                AssertHelpers.AlmostEqual(factorial, SpecialFunctions.Factorial(i), 14);
                AssertHelpers.AlmostEqual(Math.Log(factorial), SpecialFunctions.FactorialLn(i), 14);
            }

            // approximation
            for (int i = 23; i < 171; i++)
            {
                factorial *= i;
                AssertHelpers.AlmostEqual(factorial, SpecialFunctions.Factorial(i), 14);
                AssertHelpers.AlmostEqual(Math.Log(factorial), SpecialFunctions.FactorialLn(i), 14);
            }
        }

        [Test]
        public void ThrowsOnNegativeArgument()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => SpecialFunctions.Factorial(Int32.MinValue));
            Assert.Throws<ArgumentOutOfRangeException>(() => SpecialFunctions.Factorial(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => SpecialFunctions.FactorialLn(-1));
        }

        [Test]
        public void FactorialOverflowsToInfinity()
        {
            Assert.AreEqual(Double.PositiveInfinity, SpecialFunctions.Factorial(172));
            Assert.AreEqual(Double.PositiveInfinity, SpecialFunctions.Factorial(Int32.MaxValue));
        }

        [Test]
        public void FactorialLnDoesNotOverflow()
        {
            AssertHelpers.AlmostEqual(6078.2118847500501140, SpecialFunctions.FactorialLn(1 << 10), 14);
            AssertHelpers.AlmostEqual(29978.648060844048236, SpecialFunctions.FactorialLn(1 << 12), 14);
            AssertHelpers.AlmostEqual(307933.81973375485425, SpecialFunctions.FactorialLn(1 << 15), 14);
            AssertHelpers.AlmostEqual(1413421.9939462073242, SpecialFunctions.FactorialLn(1 << 17), 14);
        }
    }
}