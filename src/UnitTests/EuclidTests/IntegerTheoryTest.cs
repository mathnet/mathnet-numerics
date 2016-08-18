// <copyright file="IntegerTheoryTest.cs" company="Math.NET">
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
using System.Globalization;
using NUnit.Framework;

namespace MathNet.Numerics.UnitTests.EuclidTests
{
    /// <summary>
    /// Integer theory tests.
    /// </summary>
    [TestFixture, Category("Functions")]
    public class IntegerTheoryTest
    {
        [Test]
        public void TestModulus()
        {
            Assert.That(Euclid.Modulus(0, 3), Is.EqualTo(0));
            Assert.That(Euclid.Modulus(2, 3), Is.EqualTo(2));
            Assert.That(Euclid.Modulus(3, 3), Is.EqualTo(0));
            Assert.That(Euclid.Modulus(4, 3), Is.EqualTo(1));
            Assert.That(Euclid.Modulus(6, 3), Is.EqualTo(0));
            Assert.That(Euclid.Modulus(-1, 3), Is.EqualTo(2));
            Assert.That(Euclid.Modulus(-2, 3), Is.EqualTo(1));
            Assert.That(Euclid.Modulus(-3, 3), Is.EqualTo(0));
            Assert.That(Euclid.Modulus(-4, 3), Is.EqualTo(2));

            Assert.That(Euclid.Modulus(0, -3), Is.EqualTo(0));
            Assert.That(Euclid.Modulus(2, -3), Is.EqualTo(-1));
            Assert.That(Euclid.Modulus(3, -3), Is.EqualTo(0));
            Assert.That(Euclid.Modulus(4, -3), Is.EqualTo(-2));
            Assert.That(Euclid.Modulus(6, -3), Is.EqualTo(0));
            Assert.That(Euclid.Modulus(-1, -3), Is.EqualTo(-1));
            Assert.That(Euclid.Modulus(-2, -3), Is.EqualTo(-2));
            Assert.That(Euclid.Modulus(-3, -3), Is.EqualTo(0));
            Assert.That(Euclid.Modulus(-4, -3), Is.EqualTo(-1));
        }

        [Test]
        public void TestModulusFloatingPoint()
        {
            Assert.That(Euclid.Modulus(0.2, 3), Is.EqualTo(0.2).Within(1e-12));
            Assert.That(Euclid.Modulus(2.2, 3), Is.EqualTo(2.2).Within(1e-12));
            Assert.That(Euclid.Modulus(3.2, 3), Is.EqualTo(0.2).Within(1e-12));
            Assert.That(Euclid.Modulus(4.2, 3), Is.EqualTo(1.2).Within(1e-12));
            Assert.That(Euclid.Modulus(6.2, 3), Is.EqualTo(0.2).Within(1e-12));
            Assert.That(Euclid.Modulus(-1.2, 3), Is.EqualTo(1.8).Within(1e-12));
            Assert.That(Euclid.Modulus(-2.2, 3), Is.EqualTo(0.8).Within(1e-12));
            Assert.That(Euclid.Modulus(-3.2, 3), Is.EqualTo(2.8).Within(1e-12));
            Assert.That(Euclid.Modulus(-4.2, 3), Is.EqualTo(1.8).Within(1e-12));

            Assert.That(Euclid.Modulus(0.2, -3), Is.EqualTo(-2.8).Within(1e-12));
            Assert.That(Euclid.Modulus(2.2, -3), Is.EqualTo(-0.8).Within(1e-12));
            Assert.That(Euclid.Modulus(3.2, -3), Is.EqualTo(-2.8).Within(1e-12));
            Assert.That(Euclid.Modulus(4.2, -3), Is.EqualTo(-1.8).Within(1e-12));
            Assert.That(Euclid.Modulus(6.2, -3), Is.EqualTo(-2.8).Within(1e-12));
            Assert.That(Euclid.Modulus(-1.2, -3), Is.EqualTo(-1.2).Within(1e-12));
            Assert.That(Euclid.Modulus(-2.2, -3), Is.EqualTo(-2.2).Within(1e-12));
            Assert.That(Euclid.Modulus(-3.2, -3), Is.EqualTo(-0.2).Within(1e-12));
            Assert.That(Euclid.Modulus(-4.2, -3), Is.EqualTo(-1.2).Within(1e-12));
        }

        [Test]
        public void TestRemainder()
        {
            Assert.That(Euclid.Remainder(0, 3), Is.EqualTo(0));
            Assert.That(Euclid.Remainder(2, 3), Is.EqualTo(2));
            Assert.That(Euclid.Remainder(3, 3), Is.EqualTo(0));
            Assert.That(Euclid.Remainder(4, 3), Is.EqualTo(1));
            Assert.That(Euclid.Remainder(6, 3), Is.EqualTo(0));
            Assert.That(Euclid.Remainder(-1, 3), Is.EqualTo(-1));
            Assert.That(Euclid.Remainder(-2, 3), Is.EqualTo(-2));
            Assert.That(Euclid.Remainder(-3, 3), Is.EqualTo(0));
            Assert.That(Euclid.Remainder(-4, 3), Is.EqualTo(-1));

            Assert.That(Euclid.Remainder(0, -3), Is.EqualTo(0));
            Assert.That(Euclid.Remainder(2, -3), Is.EqualTo(2));
            Assert.That(Euclid.Remainder(3, -3), Is.EqualTo(0));
            Assert.That(Euclid.Remainder(4, -3), Is.EqualTo(1));
            Assert.That(Euclid.Remainder(6, -3), Is.EqualTo(0));
            Assert.That(Euclid.Remainder(-1, -3), Is.EqualTo(-1));
            Assert.That(Euclid.Remainder(-2, -3), Is.EqualTo(-2));
            Assert.That(Euclid.Remainder(-3, -3), Is.EqualTo(0));
            Assert.That(Euclid.Remainder(-4, -3), Is.EqualTo(-1));
        }
        [Test]
        public void TestRemainderFloatingPoint()
        {
            Assert.That(Euclid.Remainder(0.2, 3), Is.EqualTo(0.2).Within(1e-12));
            Assert.That(Euclid.Remainder(2.2, 3), Is.EqualTo(2.2).Within(1e-12));
            Assert.That(Euclid.Remainder(3.2, 3), Is.EqualTo(0.2).Within(1e-12));
            Assert.That(Euclid.Remainder(4.2, 3), Is.EqualTo(1.2).Within(1e-12));
            Assert.That(Euclid.Remainder(6.2, 3), Is.EqualTo(0.2).Within(1e-12));
            Assert.That(Euclid.Remainder(-1.2, 3), Is.EqualTo(-1.2).Within(1e-12));
            Assert.That(Euclid.Remainder(-2.2, 3), Is.EqualTo(-2.2).Within(1e-12));
            Assert.That(Euclid.Remainder(-3.2, 3), Is.EqualTo(-0.2).Within(1e-12));
            Assert.That(Euclid.Remainder(-4.2, 3), Is.EqualTo(-1.2).Within(1e-12));

            Assert.That(Euclid.Remainder(0.2, -3), Is.EqualTo(0.2).Within(1e-12));
            Assert.That(Euclid.Remainder(2.2, -3), Is.EqualTo(2.2).Within(1e-12));
            Assert.That(Euclid.Remainder(3.2, -3), Is.EqualTo(0.2).Within(1e-12));
            Assert.That(Euclid.Remainder(4.2, -3), Is.EqualTo(1.2).Within(1e-12));
            Assert.That(Euclid.Remainder(6.2, -3), Is.EqualTo(0.2).Within(1e-12));
            Assert.That(Euclid.Remainder(-1.2, -3), Is.EqualTo(-1.2).Within(1e-12));
            Assert.That(Euclid.Remainder(-2.2, -3), Is.EqualTo(-2.2).Within(1e-12));
            Assert.That(Euclid.Remainder(-3.2, -3), Is.EqualTo(-0.2).Within(1e-12));
            Assert.That(Euclid.Remainder(-4.2, -3), Is.EqualTo(-1.2).Within(1e-12));
        }

        /// <summary>
        /// Test even/odd int32.
        /// </summary>
        [Test]
        public void TestEvenOdd32()
        {
            Assert.IsTrue(0.IsEven(), "0 is even");
            Assert.IsFalse(0.IsOdd(), "0 is not odd");

            Assert.IsFalse(1.IsEven(), "1 is not even");
            Assert.IsTrue(1.IsOdd(), "1 is odd");

            Assert.IsFalse((-1).IsEven(), "-1 is not even");
            Assert.IsTrue((-1).IsOdd(), "-1 is odd");

            Assert.IsFalse(Int32.MaxValue.IsEven(), "Int32.Max is not even");
            Assert.IsTrue(Int32.MaxValue.IsOdd(), "Int32.Max is odd");

            Assert.IsTrue(Int32.MinValue.IsEven(), "Int32.Min is even");
            Assert.IsFalse(Int32.MinValue.IsOdd(), "Int32.Min is not odd");
        }

        /// <summary>
        /// Test even/odd int64.
        /// </summary>
        [Test]
        public void TestEvenOdd64()
        {
            Assert.IsTrue(((long)0).IsEven(), "0 is even");
            Assert.IsFalse(((long)0).IsOdd(), "0 is not odd");

            Assert.IsFalse(((long)1).IsEven(), "1 is not even");
            Assert.IsTrue(((long)1).IsOdd(), "1 is odd");

            Assert.IsFalse(((long)-1).IsEven(), "-1 is not even");
            Assert.IsTrue(((long)-1).IsOdd(), "-1 is odd");

            Assert.IsFalse(Int64.MaxValue.IsEven(), "Int64.Max is not even");
            Assert.IsTrue(Int64.MaxValue.IsOdd(), "Int64.Max is odd");

            Assert.IsTrue(Int64.MinValue.IsEven(), "Int64.Min is even");
            Assert.IsFalse(Int64.MinValue.IsOdd(), "Int64.Min is not odd");
        }

        /// <summary>
        /// Test if int32 is power of 2.
        /// </summary>
        [Test]
        public void TestIsPowerOfTwo32()
        {
            for (var i = 2; i < 31; i++)
            {
                var x = 1 << i;
                Assert.IsTrue(x.IsPowerOfTwo(), x + " (+)");
                Assert.IsFalse((x - 1).IsPowerOfTwo(), x + "-1 (-)");
                Assert.IsFalse((x + 1).IsPowerOfTwo(), x + "+1 (-)");
                Assert.IsFalse((-x).IsPowerOfTwo(), "-" + x + " (-)");
                Assert.IsFalse((-x + 1).IsPowerOfTwo(), "-" + x + "+1 (-)");
                Assert.IsFalse((-x - 1).IsPowerOfTwo(), "-" + x + "-1 (-)");
            }

            Assert.IsTrue(4.IsPowerOfTwo(), "4 (+)");
            Assert.IsFalse(3.IsPowerOfTwo(), "3 (-)");
            Assert.IsTrue(2.IsPowerOfTwo(), "2 (+)");
            Assert.IsTrue(1.IsPowerOfTwo(), "1 (+)");
            Assert.IsFalse(0.IsPowerOfTwo(), "0 (-)");
            Assert.IsFalse((-1).IsPowerOfTwo(), "-1 (-)");
            Assert.IsFalse((-2).IsPowerOfTwo(), "-2 (-)");
            Assert.IsFalse((-3).IsPowerOfTwo(), "-3 (-)");
            Assert.IsFalse((-4).IsPowerOfTwo(), "-4 (-)");
            Assert.IsFalse(Int32.MinValue.IsPowerOfTwo(), "Int32.MinValue (-)");
            Assert.IsFalse((Int32.MinValue + 1).IsPowerOfTwo(), "Int32.MinValue+1 (-)");
            Assert.IsFalse(Int32.MaxValue.IsPowerOfTwo(), "Int32.MaxValue (-)");
            Assert.IsFalse((Int32.MaxValue - 1).IsPowerOfTwo(), "Int32.MaxValue-1 (-)");
        }

        /// <summary>
        /// Test if int64 is power of 2.
        /// </summary>
        [Test]
        public void TestIsPowerOfTwo64()
        {
            for (var i = 2; i < 63; i++)
            {
                var x = ((long)1) << i;
                Assert.IsTrue(x.IsPowerOfTwo(), x + " (+)");
                Assert.IsFalse((x - 1).IsPowerOfTwo(), x + "-1 (-)");
                Assert.IsFalse((x + 1).IsPowerOfTwo(), x + "+1 (-)");
                Assert.IsFalse((-x).IsPowerOfTwo(), "-" + x + " (-)");
                Assert.IsFalse((-x + 1).IsPowerOfTwo(), "-" + x + "+1 (-)");
                Assert.IsFalse((-x - 1).IsPowerOfTwo(), "-" + x + "-1 (-)");
            }

            Assert.IsTrue(((long)4).IsPowerOfTwo(), "4 (+)");
            Assert.IsFalse(((long)3).IsPowerOfTwo(), "3 (-)");
            Assert.IsTrue(((long)2).IsPowerOfTwo(), "2 (+)");
            Assert.IsTrue(((long)1).IsPowerOfTwo(), "1 (+)");
            Assert.IsFalse(((long)0).IsPowerOfTwo(), "0 (-)");
            Assert.IsFalse(((long)-1).IsPowerOfTwo(), "-1 (-)");
            Assert.IsFalse(((long)-2).IsPowerOfTwo(), "-2 (-)");
            Assert.IsFalse(((long)-3).IsPowerOfTwo(), "-3 (-)");
            Assert.IsFalse(((long)-4).IsPowerOfTwo(), "-4 (-)");
            Assert.IsFalse(Int64.MinValue.IsPowerOfTwo(), "Int32.MinValue (-)");
            Assert.IsFalse((Int64.MinValue + 1).IsPowerOfTwo(), "Int32.MinValue+1 (-)");
            Assert.IsFalse(Int64.MaxValue.IsPowerOfTwo(), "Int32.MaxValue (-)");
            Assert.IsFalse((Int64.MaxValue - 1).IsPowerOfTwo(), "Int32.MaxValue-1 (-)");
        }

        /// <summary>
        /// Ceiling to power of two handles positive int32 correctly.
        /// </summary>
        [Test]
        public void CeilingToPowerOfHandlesPositiveIntegersCorrectly32()
        {
            Assert.AreEqual(0, 0.CeilingToPowerOfTwo(), "0");
            Assert.AreEqual(1, 1.CeilingToPowerOfTwo(), "1");
            Assert.AreEqual(2, 2.CeilingToPowerOfTwo(), "2");
            Assert.AreEqual(4, 3.CeilingToPowerOfTwo(), "3");
            Assert.AreEqual(4, 4.CeilingToPowerOfTwo(), "4");

            for (var i = 2; i < 31; i++)
            {
                var x = 1 << i;
                Assert.AreEqual(x, x.CeilingToPowerOfTwo(), x.ToString(CultureInfo.InvariantCulture));
                Assert.AreEqual(x, (x - 1).CeilingToPowerOfTwo(), x + "-1");
                Assert.AreEqual(x, ((x >> 1) + 1).CeilingToPowerOfTwo(), x + "/2+1");
                Assert.AreEqual(0, (-x).CeilingToPowerOfTwo(), "-" + x);
            }

            const int MaxPowerOfTwo = 0x40000000;
            Assert.AreEqual(MaxPowerOfTwo, MaxPowerOfTwo.CeilingToPowerOfTwo(), "max");
            Assert.AreEqual(MaxPowerOfTwo, (MaxPowerOfTwo - 1).CeilingToPowerOfTwo(), "max");
        }

        /// <summary>
        /// Ceiling to power of two handles positive int64 correctly.
        /// </summary>
        [Test]
        public void CeilingToPowerOfHandlesPositiveIntegersCorrectly64()
        {
            Assert.AreEqual(0, ((long)0).CeilingToPowerOfTwo(), "0");
            Assert.AreEqual(1, ((long)1).CeilingToPowerOfTwo(), "1");
            Assert.AreEqual(2, ((long)2).CeilingToPowerOfTwo(), "2");
            Assert.AreEqual(4, ((long)3).CeilingToPowerOfTwo(), "3");
            Assert.AreEqual(4, ((long)4).CeilingToPowerOfTwo(), "4");

            for (var i = 2; i < 63; i++)
            {
                var x = ((long)1) << i;
                Assert.AreEqual(x, x.CeilingToPowerOfTwo(), x.ToString(CultureInfo.InvariantCulture));
                Assert.AreEqual(x, (x - 1).CeilingToPowerOfTwo(), x + "-1");
                Assert.AreEqual(x, ((x >> 1) + 1).CeilingToPowerOfTwo(), x + "/2+1");
                Assert.AreEqual(0, (-x).CeilingToPowerOfTwo(), "-" + x);
            }

            const long MaxPowerOfTwo = 0x4000000000000000;
            Assert.AreEqual(MaxPowerOfTwo, MaxPowerOfTwo.CeilingToPowerOfTwo(), "max");
            Assert.AreEqual(MaxPowerOfTwo, (MaxPowerOfTwo - 1).CeilingToPowerOfTwo(), "max");
        }

        /// <summary>
        /// Ceiling to power of two returns zero for negative int32.
        /// </summary>
        [Test]
        public void CeilingToPowerOfTwoReturnsZeroForNegativeNumbers32()
        {
            Assert.AreEqual(0, (-1).CeilingToPowerOfTwo(), "-1");
            Assert.AreEqual(0, (-2).CeilingToPowerOfTwo(), "-2");
            Assert.AreEqual(0, (-3).CeilingToPowerOfTwo(), "-3");
            Assert.AreEqual(0, (-4).CeilingToPowerOfTwo(), "-4");
            Assert.AreEqual(0, Int32.MinValue.CeilingToPowerOfTwo(), "Int32.MinValue");
            Assert.AreEqual(0, (Int32.MinValue + 1).CeilingToPowerOfTwo(), "Int32.MinValue+1");
        }

        /// <summary>
        /// Ceiling to power of two returns zero for negative int64.
        /// </summary>
        [Test]
        public void CeilingToPowerOfTwoReturnsZeroForNegativeNumbers64()
        {
            Assert.AreEqual(0, ((long)-1).CeilingToPowerOfTwo(), "-1");
            Assert.AreEqual(0, ((long)-2).CeilingToPowerOfTwo(), "-2");
            Assert.AreEqual(0, ((long)-3).CeilingToPowerOfTwo(), "-3");
            Assert.AreEqual(0, ((long)-4).CeilingToPowerOfTwo(), "-4");
            Assert.AreEqual(0, Int64.MinValue.CeilingToPowerOfTwo(), "Int64.MinValue");
            Assert.AreEqual(0, (Int64.MinValue + 1).CeilingToPowerOfTwo(), "Int64.MinValue+1");
        }

        /// <summary>
        /// Ceiling to power of two throws <c>ArgumentOutOfRangeException</c> when result would overflow int32.
        /// </summary>
        [Test]
        public void CeilingToPowerOfTwoThrowsWhenResultWouldOverflow32()
        {
            Assert.Throws(
                typeof(ArgumentOutOfRangeException),
                () => Int32.MaxValue.CeilingToPowerOfTwo());

            const int MaxPowerOfTwo = 0x40000000;
            Assert.Throws(
                typeof(ArgumentOutOfRangeException),
                () => (MaxPowerOfTwo + 1).CeilingToPowerOfTwo());

            Assert.DoesNotThrow(
                () => (MaxPowerOfTwo - 1).CeilingToPowerOfTwo());
        }

        /// <summary>
        /// Ceiling to power of two throws <c>ArgumentOutOfRangeException</c> when result would overflow int64.
        /// </summary>
        [Test]
        public void CeilingToPowerOfTwoThrowsWhenResultWouldOverflow64()
        {
            Assert.Throws(
                typeof(ArgumentOutOfRangeException),
                () => Int64.MaxValue.CeilingToPowerOfTwo());

            const long MaxPowerOfTwo = 0x4000000000000000;
            Assert.Throws(
                typeof(ArgumentOutOfRangeException),
                () => (MaxPowerOfTwo + 1).CeilingToPowerOfTwo());

            Assert.DoesNotThrow(
                () => (MaxPowerOfTwo - 1).CeilingToPowerOfTwo());
        }

        /// <summary>
        /// Power of two matches floating point power int32.
        /// </summary>
        [Test]
        public void PowerOfTwoMatchesFloatingPointPower32()
        {
            for (var i = 0; i < 31; i++)
            {
                Assert.AreEqual(Math.Round(Math.Pow(2, i)), i.PowerOfTwo());
            }
        }

        /// <summary>
        /// Power of two matches floating point power int64.
        /// </summary>
        [Test]
        public void PowerOfTwoMatchesFloatingPointPower64()
        {
            for (var i = 0; i < 63; i++)
            {
                Assert.AreEqual(Math.Round(Math.Pow(2, i)), ((long)i).PowerOfTwo());
            }
        }

        /// <summary>
        /// Power of two throws <c>ArgumentOutOfRangeException</c> when int32 is out of range.
        /// </summary>
        [Test]
        public void PowerOfTwoThrowsWhenOutOfRange32()
        {
            Assert.Throws(
                typeof(ArgumentOutOfRangeException),
                () => (-1).PowerOfTwo());

            Assert.Throws(
                typeof(ArgumentOutOfRangeException),
                () => 31.PowerOfTwo());

            Assert.Throws(
                typeof(ArgumentOutOfRangeException),
                () => Int32.MinValue.PowerOfTwo());

            Assert.Throws(
                typeof(ArgumentOutOfRangeException),
                () => Int32.MaxValue.PowerOfTwo());

            Assert.DoesNotThrow(
                () => 30.PowerOfTwo());

            Assert.DoesNotThrow(
                () => 0.PowerOfTwo());
        }

        /// <summary>
        /// Power of two throws <c>ArgumentOutOfRangeException</c> when int64 is out of range.
        /// </summary>
        [Test]
        public void PowerOfTwoThrowsWhenOutOfRange64()
        {
            Assert.Throws(
                typeof(ArgumentOutOfRangeException),
                () => ((long)-1).PowerOfTwo());

            Assert.Throws(
                typeof(ArgumentOutOfRangeException),
                () => ((long)63).PowerOfTwo());

            Assert.Throws(
                typeof(ArgumentOutOfRangeException),
                () => Int64.MinValue.PowerOfTwo());

            Assert.Throws(
                typeof(ArgumentOutOfRangeException),
                () => Int64.MaxValue.PowerOfTwo());

            Assert.DoesNotThrow(
                () => ((long)62).PowerOfTwo());

            Assert.DoesNotThrow(
                () => ((long)0).PowerOfTwo());
        }

        /// <summary>
        /// Log2 matches floating point for int32.
        /// </summary>
        [Test]
        public void Log2MatchesFloatingPoint32()
        {
            for (var i = 0; i < 31; i++)
            {
                int number = i.PowerOfTwo();
                Assert.AreEqual((int)Math.Log(number, 2), number.Log2());
                Assert.AreEqual((int)Math.Log(number + 1, 2), (number + 1).Log2());
                if (number > 1)
                {
                    Assert.AreEqual((int)Math.Log(number - 1, 2), (number - 1).Log2());
                }
            }
        }

        /// <summary>
        /// Test if int32 is perfect square.
        /// </summary>
        [Test]
        public void TestIsPerfectSquare32()
        {
            // Test all known suares
            var lastRadix = (int)Math.Floor(Math.Sqrt(Int32.MaxValue));
            for (var i = 0; i <= lastRadix; i++)
            {
                Assert.IsTrue((i * i).IsPerfectSquare(), i + "^2 (+)");
            }

            // Test 1-offset from all known squares
            for (var i = 2; i <= lastRadix; i++)
            {
                Assert.IsFalse(((i * i) - 1).IsPerfectSquare(), i + "^2-1 (-)");
                Assert.IsFalse(((i * i) + 1).IsPerfectSquare(), i + "^2+1 (-)");
            }

            // Selected Cases
            Assert.IsTrue(100000000.IsPerfectSquare(), "100000000 (+)");
            Assert.IsFalse(100000001.IsPerfectSquare(), "100000001 (-)");
            Assert.IsFalse(99999999.IsPerfectSquare(), "99999999 (-)");
            Assert.IsFalse((-4).IsPerfectSquare(), "-4 (-)");
            Assert.IsFalse(Int32.MinValue.IsPerfectSquare(), "Int32.MinValue (-)");
            Assert.IsFalse(Int32.MaxValue.IsPerfectSquare(), "Int32.MaxValue (-)");
            Assert.IsTrue(1.IsPerfectSquare(), "1 (+)");
            Assert.IsTrue(0.IsPerfectSquare(), "0 (+)");
            Assert.IsFalse((-1).IsPerfectSquare(), "-1 (-)");
        }

        /// <summary>
        /// Test if int64 is perfect square.
        /// </summary>
        [Test]
        public void TestIsPerfectSquare64()
        {
            // Test all known suares
            for (var i = 0; i < 32; i++)
            {
                var t = ((long)1) << i;
                Assert.IsTrue((t * t).IsPerfectSquare(), t + "^2 (+)");
            }

            // Test 1-offset from all known squares
            for (var i = 1; i < 32; i++)
            {
                var t = ((long)1) << i;
                Assert.IsFalse(((t * t) - 1).IsPerfectSquare(), t + "^2-1 (-)");
                Assert.IsFalse(((t * t) + 1).IsPerfectSquare(), t + "^2+1 (-)");
            }

            // Selected Cases
            Assert.IsTrue(1000000000000000000.IsPerfectSquare(), "1000000000000000000 (+)");
            Assert.IsFalse(1000000000000000001.IsPerfectSquare(), "1000000000000000001 (-)");
            Assert.IsFalse(999999999999999999.IsPerfectSquare(), "999999999999999999 (-)");
            Assert.IsFalse(999999999999999993.IsPerfectSquare(), "999999999999999993 (-)");
            Assert.IsFalse(((long)-4).IsPerfectSquare(), "-4 (-)");
            Assert.IsFalse(Int64.MinValue.IsPerfectSquare(), "Int32.MinValue (-)");
            Assert.IsFalse(Int64.MaxValue.IsPerfectSquare(), "Int32.MaxValue (-)");
            Assert.IsTrue(((long)1).IsPerfectSquare(), "1 (+)");
            Assert.IsTrue(((long)0).IsPerfectSquare(), "0 (+)");
            Assert.IsFalse(((long)-1).IsPerfectSquare(), "-1 (-)");
        }
    }
}
