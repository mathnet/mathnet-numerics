// <copyright file="Complex32Test.TextHandling.cs" company="Math.NET">
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

namespace MathNet.Numerics.UnitTests.ComplexTests
{
    /// <summary>
    /// Complex32 text handling tests.
    /// </summary>
    [TestFixture]
    public class Complex32TextHandlingTest
    {
        /// <summary>
        /// Can format complex to string.
        /// </summary>
        /// <param name="real">Real part.</param>
        /// <param name="imag">Imaginary part.</param>
        /// <param name="expected">Expected value.</param>
        [TestCase(1, -2, "(1, -2)")]
        [TestCase(1, 2, "(1, 2)")]
        [TestCase(1, 0, "(1, 0)")]
        [TestCase(0, -2, "(0, -2)")]
        [TestCase(0, 2, "(0, 2)")]
        [TestCase(0, 0, "(0, 0)")]
        [TestCase(Single.NaN, Single.NaN, "({1}, {1})")]
        [TestCase(Single.NaN, 0, "({1}, 0)")]
        [TestCase(0, Single.NaN, "(0, {1})")]
        [TestCase(Single.PositiveInfinity, Single.PositiveInfinity, "({2}, {2})")]
        [TestCase(1.1f, 0, "(1{0}1, 0)")]
        [TestCase(-1.1f, 0, "(-1{0}1, 0)")]
        [TestCase(0, 1.1f, "(0, 1{0}1)")]
        [TestCase(0, -1.1f, "(0, -1{0}1)")]
        [TestCase(1.1f, 1.1f, "(1{0}1, 1{0}1)")]
        public void CanFormatComplexToString(float real, float imag, string expected)
        {
            var numberFormat = NumberFormatInfo.CurrentInfo;
            var a = new Complex32(real, imag);
            Assert.AreEqual(
                String.Format(
                    expected,
                    numberFormat.NumberDecimalSeparator,
                    numberFormat.NaNSymbol,
                    numberFormat.PositiveInfinitySymbol),
                a.ToString());
        }

#if !PORTABLE
        /// <summary>
        /// Can format complex to string with culture.
        /// </summary>
        /// <param name="cultureName">Culture ID.</param>
        /// <param name="number">Complex Number.</param>
        [TestCase("en-US", "1.1")]
        [TestCase("tr-TR", "1,1")]
        [TestCase("de-DE", "1,1")]
        //[TestCase("de-CH", "1.1")] Windows 8.1 issue, see http://bit.ly/W81deCH
        //[TestCase("he-IL", "1.1")] Mono 4 Issue
        public void CanFormatComplexToStringWithCulture(string cultureName, string number)
        {
            var provider = new CultureInfo(cultureName);
            var nan = double.NaN.ToString(provider);
            var infinity = double.PositiveInfinity.ToString(provider);

            Assert.AreEqual("(" + nan + ", " + nan + ")", Complex32.NaN.ToString(provider));
            Assert.AreEqual("(" + infinity + ", " + infinity + ")", Complex32.PositiveInfinity.ToString(provider));
            Assert.AreEqual("(0, 0)", Complex32.Zero.ToString(provider));
            Assert.AreEqual("(" + String.Format("{0}", number) + ", 0)", new Complex32(1.1f, 0.0f).ToString(provider));
            Assert.AreEqual("(" + String.Format("-{0}", number) + ", 0)", new Complex32(-1.1f, 0f).ToString(provider));
            Assert.AreEqual("(0, " + String.Format("-{0}", number) + ")", new Complex32(0.0f, -1.1f).ToString(provider));
            Assert.AreEqual("(0, " + String.Format("{0}", number) + ")", new Complex32(0.0f, 1.1f).ToString(provider));
            Assert.AreEqual("(" + String.Format("{0}", number) + ", " + String.Format("{0}", number) + ")", new Complex32(1.1f, 1.1f).ToString(provider));
        }
#endif

        /// <summary>
        /// Can format complex to string with format.
        /// </summary>
        [Test]
        public void CanFormatComplexToStringWithFormat()
        {
            Assert.AreEqual("(0, 0)", String.Format("{0:G}", Complex32.Zero));
            Assert.AreEqual("(1, 2)", String.Format("{0:G}", new Complex32(1, 2)));
            Assert.AreEqual("(001, 002)", String.Format("{0:000;minus 000;zero}", new Complex32(1, 2)));
            Assert.AreEqual("(zero, minus 002)", String.Format("{0:000;minus 000;zero}", new Complex32(0, -2)));
            Assert.AreEqual("(zero, zero)", String.Format("{0:000;minus 000;zero}", Complex32.Zero));

            Assert.AreEqual("(0, 0)", Complex32.Zero.ToString("G"));
            Assert.AreEqual("(1, 2)", new Complex32(1, 2).ToString("G"));
            Assert.AreEqual("(001, 002)", new Complex32(1, 2).ToString("#000;minus 000;zero"));
            Assert.AreEqual("(zero, minus 002)", new Complex32(0, -2).ToString("#000;minus 000;zero"));
            Assert.AreEqual("(zero, zero)", Complex32.Zero.ToString("#000;minus 000;zero"));
        }

        /// <summary>
        /// Can format complex to string with format invariant.
        /// </summary>
        [Test]
        public void CanFormatComplexToStringWithFormatInvariant()
        {
            var culture = CultureInfo.InvariantCulture;

            Assert.AreEqual("(NaN, NaN)", String.Format(culture, "{0:.000}", Complex32.NaN));
            Assert.AreEqual("(.000, .000)", String.Format(culture, "{0:.000}", Complex32.Zero));
            Assert.AreEqual("(1.100, .000)", String.Format(culture, "{0:.000}", new Complex32(1.1f, 0.0f)));
            Assert.AreEqual("(1.100, 1.100)", String.Format(culture, "{0:.000}", new Complex32(1.1f, 1.1f)));

            Assert.AreEqual("(NaN, NaN)", Complex32.NaN.ToString("#.000", culture));
            Assert.AreEqual("(Infinity, Infinity)", Complex32.PositiveInfinity.ToString("#.000", culture));
            Assert.AreEqual("(.000, .000)", Complex32.Zero.ToString("#.000", culture));
            Assert.AreEqual("(1.100, .000)", new Complex32(1.1f, 0.0f).ToString("#.000", culture));
            Assert.AreEqual("(.000, -1.100)", new Complex32(0.0f, -1.1f).ToString("#.000", culture));
            Assert.AreEqual("(.000, 1.100)", new Complex32(0.0f, 1.1f).ToString("#.000", culture));
            Assert.AreEqual("(1.100, 1.100)", new Complex32(1.1f, 1.1f).ToString("#.000", culture));
        }

        /// <summary>
        /// Can parse string to complex with a culture.
        /// </summary>
        /// <param name="text">Text to parse.</param>
        /// <param name="expectedReal">Expected real part.</param>
        /// <param name="expectedImaginary">Expected imaginary part.</param>
        /// <param name="cultureName">Culture ID.</param>
        [TestCase("-1 -2i", -1, -2, "en-US")]
        [TestCase("-1 - 2i ", -1, -2, "de-CH")]
        public void CanParseStringToComplexWithCulture(string text, float expectedReal, float expectedImaginary, string cultureName)
        {
            var parsed = Complex32.Parse(text, new CultureInfo(cultureName));
            Assert.AreEqual(expectedReal, parsed.Real);
            Assert.AreEqual(expectedImaginary, parsed.Imaginary);
        }

        /// <summary>
        /// Can try parse string to complex with invariant.
        /// </summary>
        /// <param name="str">String to parse.</param>
        /// <param name="expectedReal">Expected real part.</param>
        /// <param name="expectedImaginary">Expected imaginary part.</param>
        [TestCase("1", 1, 0)]
        [TestCase("-1", -1, 0)]
        [TestCase("-i", 0, -1)]
        [TestCase("i", 0, 1)]
        [TestCase("2i", 0, 2)]
        [TestCase("1 + 2i", 1, 2)]
        [TestCase("1+2i", 1, 2)]
        [TestCase("1 - 2i", 1, -2)]
        [TestCase("1-2i", 1, -2)]
        [TestCase("1,2 ", 1, 2)]
        [TestCase("1 , 2", 1, 2)]
        [TestCase("1,2i", 1, 2)]
        [TestCase("-1, -2i", -1, -2)]
        [TestCase(" - 1 , - 2 i ", -1, -2)]
        [TestCase("(+1,2i)", 1, 2)]
        [TestCase("(-1 , -2)", -1, -2)]
        [TestCase("(-1 , -2i)", -1, -2)]
        [TestCase("(+1e1 , -2e-2i)", 10, -0.02f)]
        [TestCase("(-1E1 -2e2i)", -10, -200)]
        [TestCase("(-1e+1 -2e2i)", -10, -200)]
        [TestCase("(-1e1 -2e+2i)", -10, -200)]
        [TestCase("(-1e-1  -2E2i)", -0.1f, -200)]
        [TestCase("(-1e1  -2e-2i)", -10, -0.02f)]
        [TestCase("(-1E+1 -2e+2i)", -10, -200)]
        [TestCase("(-1e-1,-2e-2i)", -0.1f, -0.02f)]
        [TestCase("(+1 +2i)", 1, 2)]
        [TestCase("(-1E+1 -2e+2i)", -10, -200)]
        [TestCase("(-1e-1,-2e-2i)", -0.1f, -0.02f)]
        public void CanTryParseStringToComplexWithInvariant(string str, float expectedReal, float expectedImaginary)
        {
            var invariantCulture = CultureInfo.InvariantCulture;
            Complex32 z;
            var ret = Complex32.TryParse(str, invariantCulture, out z);
            Assert.IsTrue(ret);
            Assert.AreEqual(expectedReal, z.Real);
            Assert.AreEqual(expectedImaginary, z.Imaginary);
        }

        /// <summary>
        /// If missing closing paren parse throws <c>FormatException</c>.
        /// </summary>
        [Test]
        public void IfMissingClosingParenParseThrowsFormatException()
        {
            Assert.That(() => Complex32.Parse("(1,2"), Throws.TypeOf<FormatException>());
        }

        /// <summary>
        /// Try parse can handle symbols.
        /// </summary>
        [Test]
        public void TryParseCanHandleSymbols()
        {
            Complex32 z;
            var ni = NumberFormatInfo.CurrentInfo;
            var separator = CultureInfo.CurrentCulture.TextInfo.ListSeparator;
            var ret = Complex32.TryParse(
                ni.NegativeInfinitySymbol + separator + ni.PositiveInfinitySymbol, out z);
            Assert.IsTrue(ret, "A1");
            Assert.AreEqual(float.NegativeInfinity, z.Real, "A2");
            Assert.AreEqual(float.PositiveInfinity, z.Imaginary, "A3");

            ret = Complex32.TryParse(ni.NaNSymbol + separator + ni.NaNSymbol, out z);
            Assert.IsTrue(ret, "B1");
            Assert.AreEqual(float.NaN, z.Real, "B2");
            Assert.AreEqual(float.NaN, z.Imaginary, "B3");

            ret = Complex32.TryParse(ni.NegativeInfinitySymbol + "+" + ni.PositiveInfinitySymbol + "i", out z);
            Assert.IsTrue(ret, "C1");
            Assert.AreEqual(float.NegativeInfinity, z.Real, "C2");
            Assert.AreEqual(float.PositiveInfinity, z.Imaginary, "C3");

            ret = Complex32.TryParse(ni.NaNSymbol + "+" + ni.NaNSymbol + "i", out z);
            Assert.IsTrue(ret, "D1");
            Assert.AreEqual(float.NaN, z.Real, "D2");
            Assert.AreEqual(float.NaN, z.Imaginary, "D3");

            ret = Complex32.TryParse(
                float.MaxValue.ToString("R") + " " + float.MinValue.ToString("R") + "i",
                out z);
            Assert.IsTrue(ret, "E1");
            Assert.AreEqual(float.MaxValue, z.Real, "E2");
            Assert.AreEqual(float.MinValue, z.Imaginary, "E3");
        }

#if !PORTABLE
        /// <summary>
        /// Try parse can handle symbols with a culture.
        /// </summary>
        /// <param name="cultureName">Culture ID.</param>
        [TestCase("en-US")]
        [TestCase("tr-TR")]
        [TestCase("de-DE")]
        [TestCase("de-CH")]
        //[TestCase("he-IL")] Mono 4 Issue
        public void TryParseCanHandleSymbolsWithCulture(string cultureName)
        {
            Complex32 z;
            var culture = new CultureInfo(cultureName);
            var ni = culture.NumberFormat;
            var separator = culture.TextInfo.ListSeparator;
            var ret = Complex32.TryParse(
                ni.NegativeInfinitySymbol + separator + ni.PositiveInfinitySymbol, culture, out z);
            Assert.IsTrue(ret, "A1");
            Assert.AreEqual(float.NegativeInfinity, z.Real, "A2");
            Assert.AreEqual(float.PositiveInfinity, z.Imaginary, "A3");

            ret = Complex32.TryParse(ni.NaNSymbol + separator + ni.NaNSymbol, culture, out z);
            Assert.IsTrue(ret, "B1");
            Assert.AreEqual(float.NaN, z.Real, "B2");
            Assert.AreEqual(float.NaN, z.Imaginary, "B3");

            ret = Complex32.TryParse(ni.NegativeInfinitySymbol + "+" + ni.PositiveInfinitySymbol + "i", culture, out z);
            Assert.IsTrue(ret, "C1");
            Assert.AreEqual(float.NegativeInfinity, z.Real, "C2");
            Assert.AreEqual(float.PositiveInfinity, z.Imaginary, "C3");

            ret = Complex32.TryParse(ni.NaNSymbol + "+" + ni.NaNSymbol + "i", culture, out z);
            Assert.IsTrue(ret, "D1");
            Assert.AreEqual(float.NaN, z.Real, "D2");
            Assert.AreEqual(float.NaN, z.Imaginary, "D3");

            ret = Complex32.TryParse(
                float.MaxValue.ToString("R", culture) + " " + float.MinValue.ToString("R", culture) + "i",
                culture,
                out z);
            Assert.IsTrue(ret, "E1");
            Assert.AreEqual(float.MaxValue, z.Real, "E2");
            Assert.AreEqual(float.MinValue, z.Imaginary, "E3");
        }
#endif

        /// <summary>
        /// Try parse returns false when given bad value with invariant.
        /// </summary>
        /// <param name="str">String to parse.</param>
        [TestCase("")]
        [TestCase("+")]
        [TestCase("1-")]
        [TestCase("i+")]
        [TestCase("1/2i")]
        [TestCase("1i+2i")]
        [TestCase("i1i")]
        [TestCase("(1i,2)")]
        [TestCase("1e+")]
        [TestCase("1e")]
        [TestCase("1,")]
        [TestCase(",1")]
        [TestCase(null)]
        [TestCase("()")]
        [TestCase("(  )")]
        public void TryParseReturnsFalseWhenGivenBadValueWithInvariant(string str)
        {
            Complex32 z;
            var ret = Complex32.TryParse(str, CultureInfo.InvariantCulture, out z);
            Assert.IsFalse(ret);
            Assert.AreEqual(0, z.Real);
            Assert.AreEqual(0, z.Imaginary);
        }
    }
}
