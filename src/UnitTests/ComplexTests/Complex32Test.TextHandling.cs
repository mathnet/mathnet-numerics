// <copyright file="Complex32Test.TextHandling.cs" company="Math.NET">
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

namespace MathNet.Numerics.UnitTests.ComplexTests
{
    using System;
    using System.Globalization;
    using NUnit.Framework;

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
        [Test, Sequential]
        public void CanFormatComplexToString(
            [Values(1, 1, 1, 0, 0, 0, Single.NaN, Single.NaN, 0, Single.PositiveInfinity, 1.1f, -1.1f, 0, 0, 1.1f)] float real, 
            [Values(-2, 2, 0, -2, 2, 0, Single.NaN, 0, Single.NaN, Single.PositiveInfinity, 0, 0, 1.1f, -1.1f, 1.1f)] float imag, 
            [Values("(1, -2)", "(1, 2)", "(1, 0)", "(0, -2)", "(0, 2)", "(0, 0)", "({1}, {1})", "({1}, 0)", "(0, {1})", "({2}, {2})", "(1{0}1, 0)", "(-1{0}1, 0)", "(0, 1{0}1)", "(0, -1{0}1)", "(1{0}1, 1{0}1)")] string expected)
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

        /// <summary>
        /// Can format complex to string with culture.
        /// </summary>
        /// <param name="cultureName">Culture ID.</param>
        /// <param name="nan">Not a number name.</param>
        /// <param name="infinity">Infinity name.</param>
        /// <param name="number">Complex Number.</param>
        [Test, Sequential]
        public void CanFormatComplexToStringWithCulture(
            [Values("en-US", "tr-TR", "de-DE", "de-CH", "he-IL")] string cultureName, 
            [Values("NaN", "NaN", "n. def.", "n. def.", "לא מספר")] string nan, 
            [Values("Infinity", "Infinity", "+unendlich", "+unendlich", "אינסוף חיובי")] string infinity, 
            [Values("1.1", "1,1", "1,1", "1.1", "1.1")] string number)
        {
            var provider = CultureInfo.GetCultureInfo(cultureName);
            Assert.AreEqual("(" + nan + ", " + nan + ")", Complex32.NaN.ToString(provider));
            Assert.AreEqual("(" + infinity + ", " + infinity + ")", Complex32.Infinity.ToString(provider));
            Assert.AreEqual("(0, 0)", Complex32.Zero.ToString(provider));
            Assert.AreEqual("(" + String.Format("{0}", number) + ", 0)", new Complex32(1.1f, 0.0f).ToString(provider));
            Assert.AreEqual("(" + String.Format("-{0}", number) + ", 0)", new Complex32(-1.1f, 0f).ToString(provider));
            Assert.AreEqual("(0, " + String.Format("-{0}", number) + ")", new Complex32(0.0f, -1.1f).ToString(provider));
            Assert.AreEqual("(0, " + String.Format("{0}", number) + ")", new Complex32(0.0f, 1.1f).ToString(provider));
            Assert.AreEqual("(" + String.Format("{0}", number) + ", " + String.Format("{0}", number) + ")", new Complex32(1.1f, 1.1f).ToString(provider));
        }

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
            Assert.AreEqual("(Infinity, Infinity)", Complex32.Infinity.ToString("#.000", culture));
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
        [Test, Sequential]
        public void CanParseStringToComplexWithCulture(
            [Values("-1 -2i", "-1 - 2i ")] string text, 
            [Values(-1, -1)] float expectedReal, 
            [Values(-2, -2)] float expectedImaginary, 
            [Values("en-US", "de-CH")] string cultureName)
        {
            var parsed = Complex32.Parse(text, CultureInfo.GetCultureInfo(cultureName));
            Assert.AreEqual(expectedReal, parsed.Real);
            Assert.AreEqual(expectedImaginary, parsed.Imaginary);
        }

        /// <summary>
        /// Can try parse string to complex with invariant.
        /// </summary>
        /// <param name="str">String to parse.</param>
        /// <param name="expectedReal">Expected real part.</param>
        /// <param name="expectedImaginary">Expected imaginary part.</param>
        [Test, Sequential]
        public void CanTryParseStringToComplexWithInvariant(
            [Values("1", "-1", "-i", "i", "2i", "1 + 2i", "1+2i", "1 - 2i", "1-2i", "1,2 ", "1 , 2", "1,2i", "-1, -2i", " - 1 , - 2 i ", "(+1,2i)", "(-1 , -2)", "(-1 , -2i)", "(+1e1 , -2e-2i)", "(-1E1 -2e2i)", "(-1e+1 -2e2i)", "(-1e1 -2e+2i)", "(-1e-1  -2E2i)", "(-1e1  -2e-2i)", "(-1E+1 -2e+2i)", "(-1e-1,-2e-2i)", "(+1 +2i)", "(-1E+1 -2e+2i)", "(-1e-1,-2e-2i)")] string str, 
            [Values(1, -1, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, -1, -1, 1, -1, -1, 10, -10, -10, -10, -0.1f, -10, -10, -0.1f, 1, -10, -0.1f)] float expectedReal, 
            [Values(0, 0, -1, 1, 2, 2, 2, -2, -2, 2, 2, 2, -2, -2, 2, -2, -2, -0.02f, -200, -200, -200, -200, -0.02f, -200, -0.02f, 2, -200, -0.02f)] float expectedImaginary)
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
            Assert.Throws<FormatException>(() => Complex32.Parse("(1,2"));
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

        /// <summary>
        /// Try parse can handle symbols with a culture.
        /// </summary>
        /// <param name="cultureName">Culture ID.</param>
        [Test]
        public void TryParseCanHandleSymbolsWithCulture([Values("en-US", "tr-TR", "de-DE", "de-CH", "he-IL")] string cultureName)
        {
            Complex32 z;
            var culture = CultureInfo.GetCultureInfo(cultureName);
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

        /// <summary>
        /// Try parse returns false when given bad value with invariant.
        /// </summary>
        /// <param name="str">String to parse.</param>
        [Test]
        public void TryParseReturnsFalseWhenGivenBadValueWithInvariant([Values("", "+", "1-", "i+", "1/2i", "1i+2i", "i1i", "(1i,2)", "1e+", "1e", "1,", ",1", null, "()", "(  )")] string str)
        {
            Complex32 z;
            var ret = Complex32.TryParse(str, CultureInfo.InvariantCulture, out z);
            Assert.IsFalse(ret);
            Assert.AreEqual(0, z.Real);
            Assert.AreEqual(0, z.Imaginary);
        }
    }
}
