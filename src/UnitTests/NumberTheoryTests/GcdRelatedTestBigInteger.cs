// <copyright file="GcdRelatedTest.cs" company="Math.NET">
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

namespace MathNet.Numerics.UnitTests.NumberTheoryTests
{
    using System;
    using System.Numerics;
    using MbUnit.Framework;
    using NumberTheory;

    [TestFixture]
    public class GcdRelatedTestBigInteger
    {
        [Test]
        public void GcdHandlesNormalInputCorrectly()
        {
			Assert.AreEqual(0, IntegerTheory.GreatestCommonDivisor(BigInteger.Zero, BigInteger.Zero), "Gcd(0,0)");
			Assert.AreEqual(6, IntegerTheory.GreatestCommonDivisor(BigInteger.Zero, 6), "Gcd(0,6)");
			Assert.AreEqual(1, IntegerTheory.GreatestCommonDivisor((BigInteger)7, 13), "Gcd(7,13)");
			Assert.AreEqual(7, IntegerTheory.GreatestCommonDivisor((BigInteger)7, 14), "Gcd(7,14)");
			Assert.AreEqual(1, IntegerTheory.GreatestCommonDivisor((BigInteger)7, 15), "Gcd(7,15)");
			Assert.AreEqual(3, IntegerTheory.GreatestCommonDivisor((BigInteger)6, 15), "Gcd(6,15)");
        }

        [Test]
        public void GcdHandlesNegativeInputCorrectly()
        {
			Assert.AreEqual(5, IntegerTheory.GreatestCommonDivisor((BigInteger)(-5), 0), "Gcd(-5,0)");
			Assert.AreEqual(5, IntegerTheory.GreatestCommonDivisor(BigInteger.Zero, -5), "Gcd(0, -5)");
			Assert.AreEqual(1, IntegerTheory.GreatestCommonDivisor((BigInteger)(-7), 15), "Gcd(-7,15)");
			Assert.AreEqual(1, IntegerTheory.GreatestCommonDivisor((BigInteger)(-7), -15), "Gcd(-7,-15)");
        }

        [Test]
        public void GcdSupportsLargeInput()
        {
			Assert.AreEqual(Int32.MaxValue, IntegerTheory.GreatestCommonDivisor(BigInteger.Zero, Int32.MaxValue), "Gcd(0,Int32Max)");
			Assert.AreEqual(Int64.MaxValue, IntegerTheory.GreatestCommonDivisor(BigInteger.Zero, Int64.MaxValue), "Gcd(0,Int64Max)");
			Assert.AreEqual(1, IntegerTheory.GreatestCommonDivisor((BigInteger)Int32.MaxValue, Int64.MaxValue), "Gcd(Int32Max,Int64Max)");
			Assert.AreEqual(1 << 18, IntegerTheory.GreatestCommonDivisor((BigInteger)(1 << 18), 1 << 20), "Gcd(1>>18,1<<20)");
			Assert.AreEqual(1 << 18, IntegerTheory.GreatestCommonDivisor((BigInteger)(1 << 18), 1 << 20), "Gcd(1>>18,1<<20)");
			Assert.AreEqual(4569031055798, IntegerTheory.GreatestCommonDivisor(BigInteger.Parse("7305316061155559483748611586449542122662"), BigInteger.Parse("57377277362010117405715236427413896")), "Gcd(large)");
        }

		[Test]
		public void ExtendedGcdHandlesNormalInputCorrectly()
		{
			BigInteger x, y;

			Assert.AreEqual(3, IntegerTheory.ExtendedGreatestCommonDivisor((BigInteger)6, 15, out x, out y), "Egcd(6,15)");
			Assert.AreEqual(3, 6 * x + 15 * y, "Egcd(6,15) -> a*x+b*y");

			Assert.AreEqual(3, IntegerTheory.ExtendedGreatestCommonDivisor((BigInteger)(-6), 15, out x, out y), "Egcd(-6,15)");
			Assert.AreEqual(3, -6 * x + 15 * y, "Egcd(-6,15) -> a*x+b*y");

			Assert.AreEqual(3, IntegerTheory.ExtendedGreatestCommonDivisor((BigInteger)(-6), -15, out x, out y), "Egcd(-6,-15)");
			Assert.AreEqual(3, -6 * x + -15 * y, "Egcd(-6,-15) -> a*x+b*y");

			var a = BigInteger.Parse("7305316061155559483748611586449542122662");
			var b = BigInteger.Parse("57377277362010117405715236427413896");
			Assert.AreEqual(4569031055798, IntegerTheory.ExtendedGreatestCommonDivisor(a, b, out x, out y), "Egcd(large)");
			Assert.AreEqual(4569031055798, a * x + b * y, "Egcd(large) -> a*x+b*y");
			Assert.AreEqual(4569031055798, IntegerTheory.ExtendedGreatestCommonDivisor(-a, b, out x, out y), "Egcd(-large)");
			Assert.AreEqual(4569031055798, -a * x + b * y, "Egcd(-large) -> a*x+b*y");
		}

        [Test]
        public void ListGcdHandlesNormalInputCorrectly()
        {
			Assert.AreEqual(2, IntegerTheory.GreatestCommonDivisor((BigInteger)(-10), 6, -8), "Gcd(-10,6,-8)");
			Assert.AreEqual(1, IntegerTheory.GreatestCommonDivisor((BigInteger)(-10), 6, -8, 5, 9, 13), "Gcd(-10,6,-8,5,9,13)");
			Assert.AreEqual(5, IntegerTheory.GreatestCommonDivisor((BigInteger)(-10), 20, 120, 60, -15, 1000), "Gcd(-10,20,120,60,-15,1000)");
			Assert.AreEqual(3, IntegerTheory.GreatestCommonDivisor((BigInteger)(Int64.MaxValue - 1), Int64.MaxValue - 4, Int64.MaxValue - 7), "Gcd(Int64Max-1,Int64Max-4,Int64Max-7)");
			Assert.AreEqual(123, IntegerTheory.GreatestCommonDivisor((BigInteger)492, -2 * 492, 492 / 4), "Gcd(492, -984, 123)");
        }

        [Test]
        public void ListGcdHandlesSpecialInputCorrectly()
        {
            Assert.AreEqual(0, IntegerTheory.GreatestCommonDivisor(new BigInteger[0]), "Gcd()");
			Assert.AreEqual(100, IntegerTheory.GreatestCommonDivisor((BigInteger)(-100)), "Gcd(-100)");
        }

        [Test]
        public void ListGcdChecksForNullArguments()
        {
            Assert.Throws(
                typeof (ArgumentNullException),
				() => IntegerTheory.GreatestCommonDivisor((BigInteger[])null));
        }

        [Test]
        public void LcmHandlesNormalInputCorrectly()
        {
			Assert.AreEqual(10, IntegerTheory.LeastCommonMultiple((BigInteger)10, 10), "Lcm(10,10)");

			Assert.AreEqual(0, IntegerTheory.LeastCommonMultiple(BigInteger.Zero, 10), "Lcm(0,10)");
			Assert.AreEqual(0, IntegerTheory.LeastCommonMultiple((BigInteger)10, 0), "Lcm(10,0)");

			Assert.AreEqual(77, IntegerTheory.LeastCommonMultiple((BigInteger)11, 7), "Lcm(11,7)");
			Assert.AreEqual(33, IntegerTheory.LeastCommonMultiple((BigInteger)11, 33), "Lcm(11,33)");
			Assert.AreEqual(374, IntegerTheory.LeastCommonMultiple((BigInteger)11, 34), "Lcm(11,34)");
        }

        [Test]
        public void LcmHandlesNegativeInputCorrectly()
        {
			Assert.AreEqual(352, IntegerTheory.LeastCommonMultiple((BigInteger)11, -32), "Lcm(11,-32)");
			Assert.AreEqual(352, IntegerTheory.LeastCommonMultiple((BigInteger)(-11), 32), "Lcm(-11,32)");
			Assert.AreEqual(352, IntegerTheory.LeastCommonMultiple((BigInteger)(-11), -32), "Lcm(-11,-32)");
        }

        [Test]
        public void LcmSupportsLargeInput()
        {
			Assert.AreEqual(Int32.MaxValue, IntegerTheory.LeastCommonMultiple((BigInteger)Int32.MaxValue, Int32.MaxValue), "Lcm(Int32Max,Int32Max)");
			Assert.AreEqual(Int64.MaxValue, IntegerTheory.LeastCommonMultiple((BigInteger)Int64.MaxValue, Int64.MaxValue), "Lcm(Int64Max,Int64Max)");
			Assert.AreEqual(Int64.MaxValue, IntegerTheory.LeastCommonMultiple((BigInteger)(-Int64.MaxValue), -Int64.MaxValue), "Lcm(-Int64Max,-Int64Max)");
			Assert.AreEqual(Int64.MaxValue, IntegerTheory.LeastCommonMultiple((BigInteger)(-Int64.MaxValue), Int64.MaxValue), "Lcm(-Int64Max,Int64Max)");
			Assert.AreEqual(BigInteger.Parse("91739176367857263082719902034485224119528064014300888465614024"), IntegerTheory.LeastCommonMultiple(BigInteger.Parse("7305316061155559483748611586449542122662"), BigInteger.Parse("57377277362010117405715236427413896")), "Lcm(large)");
        }

        [Test]
        public void ListLcmHandlesNormalInputCorrectly()
        {
			Assert.AreEqual(120, IntegerTheory.LeastCommonMultiple((BigInteger)(-10), 6, -8), "Lcm(-10,6,-8)");
			Assert.AreEqual(4680, IntegerTheory.LeastCommonMultiple((BigInteger)(-10), 6, -8, 5, 9, 13), "Lcm(-10,6,-8,5,9,13)");
			Assert.AreEqual(3000, IntegerTheory.LeastCommonMultiple((BigInteger)(-10), 20, 120, 60, -15, 1000), "Lcm(-10,20,120,60,-15,1000)");
			Assert.AreEqual(984, IntegerTheory.LeastCommonMultiple((BigInteger)492, -2 * 492, 492 / 4), "Lcm(492, -984, 123)");
			Assert.AreEqual(2016, IntegerTheory.LeastCommonMultiple((BigInteger)32, 42, 36, 18), "Lcm(32,42,36,18)");
        }

        [Test]
        public void ListLcmHandlesSpecialInputCorrectly()
        {
            Assert.AreEqual(1, IntegerTheory.LeastCommonMultiple(new BigInteger[0]), "Lcm()");
			Assert.AreEqual(100, IntegerTheory.LeastCommonMultiple((BigInteger)(-100)), "Lcm(-100)");
        }

        [Test]
        public void ListLcmChecksForNullArguments()
        {
            Assert.Throws(
                typeof(ArgumentNullException),
                () => IntegerTheory.LeastCommonMultiple((BigInteger[])null));
        }
    }
}
