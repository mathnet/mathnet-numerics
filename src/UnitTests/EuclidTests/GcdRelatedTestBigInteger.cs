// <copyright file="GcdRelatedTestBigInteger.cs" company="Math.NET">
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

#if !NOSYSNUMERICS

using System;
using System.Numerics;
using NUnit.Framework;

namespace MathNet.Numerics.UnitTests.EuclidTests
{
    /// <summary>
    /// GreatestCommonDivisor related test for <c>BigInteger</c>.
    /// </summary>
    [TestFixture, Category("Functions")]
    public class GcdRelatedTestBigInteger
    {
        /// <summary>
        /// GreatestCommonDivisor handles normal input correctly.
        /// </summary>
        [Test]
        public void GcdHandlesNormalInputCorrectly()
        {
            Assert.AreEqual((BigInteger)0, Euclid.GreatestCommonDivisor(BigInteger.Zero, BigInteger.Zero), "Gcd(0,0)");
            Assert.AreEqual((BigInteger)6, Euclid.GreatestCommonDivisor(BigInteger.Zero, 6), "Gcd(0,6)");
            Assert.AreEqual((BigInteger)1, Euclid.GreatestCommonDivisor((BigInteger)7, 13), "Gcd(7,13)");
            Assert.AreEqual((BigInteger)7, Euclid.GreatestCommonDivisor((BigInteger)7, 14), "Gcd(7,14)");
            Assert.AreEqual((BigInteger)1, Euclid.GreatestCommonDivisor((BigInteger)7, 15), "Gcd(7,15)");
            Assert.AreEqual((BigInteger)3, Euclid.GreatestCommonDivisor((BigInteger)6, 15), "Gcd(6,15)");
        }

        /// <summary>
        /// GreatestCommonDivisor handles negative input correctly.
        /// </summary>
        [Test]
        public void GcdHandlesNegativeInputCorrectly()
        {
            Assert.AreEqual((BigInteger)5, Euclid.GreatestCommonDivisor((BigInteger)(-5), 0), "Gcd(-5,0)");
            Assert.AreEqual((BigInteger)5, Euclid.GreatestCommonDivisor(BigInteger.Zero, -5), "Gcd(0, -5)");
            Assert.AreEqual((BigInteger)1, Euclid.GreatestCommonDivisor((BigInteger)(-7), 15), "Gcd(-7,15)");
            Assert.AreEqual((BigInteger)1, Euclid.GreatestCommonDivisor((BigInteger)(-7), -15), "Gcd(-7,-15)");
        }

        /// <summary>
        /// GreatestCommonDivisor supports large input.
        /// </summary>
        [Test]
        public void GcdSupportsLargeInput()
        {
            Assert.AreEqual((BigInteger)Int32.MaxValue, Euclid.GreatestCommonDivisor(BigInteger.Zero, Int32.MaxValue), "Gcd(0,Int32Max)");
            Assert.AreEqual((BigInteger)Int64.MaxValue, Euclid.GreatestCommonDivisor(BigInteger.Zero, Int64.MaxValue), "Gcd(0,Int64Max)");
            Assert.AreEqual((BigInteger)1, Euclid.GreatestCommonDivisor((BigInteger)Int32.MaxValue, Int64.MaxValue), "Gcd(Int32Max,Int64Max)");
            Assert.AreEqual((BigInteger)(1 << 18), Euclid.GreatestCommonDivisor((BigInteger)(1 << 18), 1 << 20), "Gcd(1>>18,1<<20)");
            Assert.AreEqual((BigInteger)(1 << 18), Euclid.GreatestCommonDivisor((BigInteger)(1 << 18), 1 << 20), "Gcd(1>>18,1<<20)");
#if !PORTABLE
            Assert.AreEqual((BigInteger)4569031055798, Euclid.GreatestCommonDivisor(BigInteger.Parse("7305316061155559483748611586449542122662"), BigInteger.Parse("57377277362010117405715236427413896")), "Gcd(large)");
#endif
        }

        /// <summary>
        /// Extended GreatestCommonDivisor handles normal input correctly.
        /// </summary>
        [Test]
        public void ExtendedGcdHandlesNormalInputCorrectly()
        {
            BigInteger x, y;

            Assert.AreEqual((BigInteger)3, Euclid.ExtendedGreatestCommonDivisor(6, 15, out x, out y), "Egcd(6,15)");
            Assert.AreEqual((BigInteger)3, (6*x) + (15*y), "Egcd(6,15) -> a*x+b*y");

            Assert.AreEqual((BigInteger)3, Euclid.ExtendedGreatestCommonDivisor(-6, 15, out x, out y), "Egcd(-6,15)");
            Assert.AreEqual((BigInteger)3, (-6*x) + (15*y), "Egcd(-6,15) -> a*x+b*y");

            Assert.AreEqual((BigInteger)3, Euclid.ExtendedGreatestCommonDivisor(-6, -15, out x, out y), "Egcd(-6,-15)");
            Assert.AreEqual((BigInteger)3, (-6*x) + (-15*y), "Egcd(-6,-15) -> a*x+b*y");

#if !PORTABLE
            var a = BigInteger.Parse("7305316061155559483748611586449542122662");
            var b = BigInteger.Parse("57377277362010117405715236427413896");
            Assert.AreEqual((BigInteger)4569031055798, Euclid.ExtendedGreatestCommonDivisor(a, b, out x, out y), "Egcd(large)");
            Assert.AreEqual((BigInteger)4569031055798, (a*x) + (b*y), "Egcd(large) -> a*x+b*y");
            Assert.AreEqual((BigInteger)4569031055798, Euclid.ExtendedGreatestCommonDivisor(-a, b, out x, out y), "Egcd(-large)");
            Assert.AreEqual((BigInteger)4569031055798, (-a*x) + (b*y), "Egcd(-large) -> a*x+b*y");
#endif
        }

        /// <summary>
        /// List GreatestCommonDivisor handles normal input Correctly
        /// </summary>
        [Test]
        public void ListGcdHandlesNormalInputCorrectly()
        {
            Assert.AreEqual((BigInteger)2, Euclid.GreatestCommonDivisor((BigInteger)(-10), 6, -8), "Gcd(-10,6,-8)");
            Assert.AreEqual((BigInteger)1, Euclid.GreatestCommonDivisor((BigInteger)(-10), 6, -8, 5, 9, 13), "Gcd(-10,6,-8,5,9,13)");
            Assert.AreEqual((BigInteger)5, Euclid.GreatestCommonDivisor((BigInteger)(-10), 20, 120, 60, -15, 1000), "Gcd(-10,20,120,60,-15,1000)");
            Assert.AreEqual((BigInteger)3, Euclid.GreatestCommonDivisor((BigInteger)(Int64.MaxValue - 1), Int64.MaxValue - 4, Int64.MaxValue - 7), "Gcd(Int64Max-1,Int64Max-4,Int64Max-7)");
            Assert.AreEqual((BigInteger)123, Euclid.GreatestCommonDivisor((BigInteger)492, -2*492, 492/4), "Gcd(492, -984, 123)");
        }

        /// <summary>
        /// List GreatestCommonDivisor handles special input correctly.
        /// </summary>
        [Test]
        public void ListGcdHandlesSpecialInputCorrectly()
        {
            Assert.AreEqual((BigInteger)0, Euclid.GreatestCommonDivisor(new BigInteger[0]), "Gcd()");
            Assert.AreEqual((BigInteger)100, Euclid.GreatestCommonDivisor((BigInteger)(-100)), "Gcd(-100)");
        }

        /// <summary>
        /// List GreatestCommonDivisor checks for <c>null</c> all arguments.
        /// </summary>
        [Test]
        public void ListGcdChecksForNullArguments()
        {
            Assert.Throws(
                typeof (ArgumentNullException),
                () => Euclid.GreatestCommonDivisor((BigInteger[])null));
        }

        /// <summary>
        /// LeastCommonMultiple handles normal input correctly.
        /// </summary>
        [Test]
        public void LcmHandlesNormalInputCorrectly()
        {
            Assert.AreEqual((BigInteger)10, Euclid.LeastCommonMultiple((BigInteger)10, 10), "Lcm(10,10)");

            Assert.AreEqual((BigInteger)0, Euclid.LeastCommonMultiple(BigInteger.Zero, 10), "Lcm(0,10)");
            Assert.AreEqual((BigInteger)0, Euclid.LeastCommonMultiple((BigInteger)10, 0), "Lcm(10,0)");

            Assert.AreEqual((BigInteger)77, Euclid.LeastCommonMultiple((BigInteger)11, 7), "Lcm(11,7)");
            Assert.AreEqual((BigInteger)33, Euclid.LeastCommonMultiple((BigInteger)11, 33), "Lcm(11,33)");
            Assert.AreEqual((BigInteger)374, Euclid.LeastCommonMultiple((BigInteger)11, 34), "Lcm(11,34)");
        }

        /// <summary>
        /// LeastCommonMultiple handles negative input correctly.
        /// </summary>
        [Test]
        public void LcmHandlesNegativeInputCorrectly()
        {
            Assert.AreEqual((BigInteger)352, Euclid.LeastCommonMultiple((BigInteger)11, -32), "Lcm(11,-32)");
            Assert.AreEqual((BigInteger)352, Euclid.LeastCommonMultiple((BigInteger)(-11), 32), "Lcm(-11,32)");
            Assert.AreEqual((BigInteger)352, Euclid.LeastCommonMultiple((BigInteger)(-11), -32), "Lcm(-11,-32)");
        }

        /// <summary>
        /// LeastCommonMultiple supports large input.
        /// </summary>
        [Test]
        public void LcmSupportsLargeInput()
        {
            Assert.AreEqual((BigInteger)Int32.MaxValue, Euclid.LeastCommonMultiple((BigInteger)Int32.MaxValue, Int32.MaxValue), "Lcm(Int32Max,Int32Max)");
            Assert.AreEqual((BigInteger)Int64.MaxValue, Euclid.LeastCommonMultiple((BigInteger)Int64.MaxValue, Int64.MaxValue), "Lcm(Int64Max,Int64Max)");
            Assert.AreEqual((BigInteger)Int64.MaxValue, Euclid.LeastCommonMultiple((BigInteger)(-Int64.MaxValue), -Int64.MaxValue), "Lcm(-Int64Max,-Int64Max)");
            Assert.AreEqual((BigInteger)Int64.MaxValue, Euclid.LeastCommonMultiple((BigInteger)(-Int64.MaxValue), Int64.MaxValue), "Lcm(-Int64Max,Int64Max)");
#if !PORTABLE
            Assert.AreEqual(BigInteger.Parse("91739176367857263082719902034485224119528064014300888465614024"), Euclid.LeastCommonMultiple(BigInteger.Parse("7305316061155559483748611586449542122662"), BigInteger.Parse("57377277362010117405715236427413896")), "Lcm(large)");
#endif
        }

        /// <summary>
        /// List LeastCommonMultiple handles normal input correctly.
        /// </summary>
        [Test]
        public void ListLcmHandlesNormalInputCorrectly()
        {
            Assert.AreEqual((BigInteger)120, Euclid.LeastCommonMultiple((BigInteger)(-10), 6, -8), "Lcm(-10,6,-8)");
            Assert.AreEqual((BigInteger)4680, Euclid.LeastCommonMultiple((BigInteger)(-10), 6, -8, 5, 9, 13), "Lcm(-10,6,-8,5,9,13)");
            Assert.AreEqual((BigInteger)3000, Euclid.LeastCommonMultiple((BigInteger)(-10), 20, 120, 60, -15, 1000), "Lcm(-10,20,120,60,-15,1000)");
            Assert.AreEqual((BigInteger)984, Euclid.LeastCommonMultiple((BigInteger)492, -2*492, 492/4), "Lcm(492, -984, 123)");
            Assert.AreEqual((BigInteger)2016, Euclid.LeastCommonMultiple((BigInteger)32, 42, 36, 18), "Lcm(32,42,36,18)");
        }

        /// <summary>
        /// List LeastCommonMultiple handles special input correctly.
        /// </summary>
        [Test]
        public void ListLcmHandlesSpecialInputCorrectly()
        {
            Assert.AreEqual((BigInteger)1, Euclid.LeastCommonMultiple(new BigInteger[0]), "Lcm()");
            Assert.AreEqual((BigInteger)100, Euclid.LeastCommonMultiple((BigInteger)(-100)), "Lcm(-100)");
        }

        /// <summary>
        /// List LeastCommonMultiple checks for <c>null</c> arguments.
        /// </summary>
        [Test]
        public void ListLcmChecksForNullArguments()
        {
            Assert.Throws(
                typeof (ArgumentNullException),
                () => Euclid.LeastCommonMultiple((BigInteger[])null));
        }
    }
}

#endif
