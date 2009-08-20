// <copyright file="ComplexTest.TextHandling.cs" company="Math.NET">
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

namespace MathNet.Numerics.UnitTests.ComplexTests
{
    using System;
    using System.Globalization;
    using MbUnit.Framework;

    [TestFixture]
    public class ComplexTextHandlingTest
    {
        [Test]
        [Row(1, -2, "1 -2i")]
        [Row(1, 2, "1 + 2i")]
        [Row(1, 0, "1")]
        [Row(0, -2, "-2i")]
        [Row(0, 2, "2i")]
        [Row(0, 2, "2i")]
        [Row(0, 0, "0")]
        [Row(Double.NaN, Double.NaN, "{1}")]
        [Row(Double.NaN, 0, "{1}")]
        [Row(0, Double.NaN, "{1}")]
        [Row(Double.PositiveInfinity, Double.PositiveInfinity, "{2}")]
        [Row(1.1, 0, "1{0}1")]
        [Row(-1.1, 0, "-1{0}1")]
        [Row(0, 1.1, "1{0}1i")]
        [Row(0, -1.1, "-1{0}1i")]
        [Row(1.1, 1.1, "1{0}1 + 1{0}1i")]
        public void CanFormatComplexToString(double real, double imag, string expected)
        {
            var numberFormat = NumberFormatInfo.CurrentInfo;
            var a = new Complex(real, imag);
            Assert.AreEqual(
                String.Format(
                    expected,
                    numberFormat.NumberDecimalSeparator,
                    numberFormat.NaNSymbol,
                    numberFormat.PositiveInfinitySymbol),
                a.ToString());
        }

        [Test]
        [MultipleAsserts]
        [Row("en-US", "NaN", "Infinity", "1.1")]
        [Row("tr-TR", "NaN", "Infinity", "1,1")]
        [Row("de-DE", "n. def.", "+unendlich", "1,1")]
        [Row("de-CH", "n. def.", "+unendlich", "1.1")]
        [Row("he-IL", "לא מספר", "אינסוף חיובי", "1.1")]
        public void CanFormatComplexToStringWithCulture(
            string cultureName, string nan, string infinity, string number)
        {
            var provider = CultureInfo.GetCultureInfo(cultureName);
            Assert.AreEqual(nan, Complex.NaN.ToString(provider));
            Assert.AreEqual(infinity, Complex.Infinity.ToString(provider));
            Assert.AreEqual("0", Complex.Zero.ToString(provider));
            Assert.AreEqual(String.Format("{0}", number), new Complex(1.1, 0).ToString(provider));
            Assert.AreEqual(String.Format("-{0}", number), new Complex(-1.1, 0).ToString(provider));
            Assert.AreEqual(String.Format("-{0}i", number), new Complex(0, -1.1).ToString(provider));
            Assert.AreEqual(String.Format("{0}i", number), new Complex(0, 1.1).ToString(provider));
            Assert.AreEqual(String.Format("{0} + {0}i", number), new Complex(1.1, 1.1).ToString(provider));
        }

        [Test]
        [MultipleAsserts]
        public void CanFormatComplexToStringWithFormat()
        {
            Assert.AreEqual("0", String.Format("{0:G}", Complex.Zero));
            Assert.AreEqual("1 + 2i", String.Format("{0:G}", new Complex(1, 2)));
            Assert.AreEqual("001 + 002i", String.Format("{0:000;minus 000;zero}", new Complex(1, 2)));
            Assert.AreEqual("minus 002i", String.Format("{0:000;minus 000;zero}", new Complex(0, -2)));
            Assert.AreEqual("zero", String.Format("{0:000;minus 000;zero}", Complex.Zero));

            Assert.AreEqual("0", Complex.Zero.ToString("G"));
            Assert.AreEqual("1 + 2i", new Complex(1, 2).ToString("G"));
            Assert.AreEqual("001 + 002i", new Complex(1, 2).ToString("#000;minus 000;zero"));
            Assert.AreEqual("minus 002i", new Complex(0, -2).ToString("#000;minus 000;zero"));
            Assert.AreEqual("zero", Complex.Zero.ToString("#000;minus 000;zero"));
        }

        [Test]
        [MultipleAsserts]
        public void CanFormatComplexToStringWithFormatInvariant()
        {
            var culture = CultureInfo.InvariantCulture;

            Assert.AreEqual("NaN", String.Format(culture, "{0:.000}", Complex.NaN));
            Assert.AreEqual(".000", String.Format(culture, "{0:.000}", Complex.Zero));
            Assert.AreEqual("1.100", String.Format(culture, "{0:.000}", new Complex(1.1, 0)));
            Assert.AreEqual("1.100 + 1.100i", String.Format(culture, "{0:.000}", new Complex(1.1, 1.1)));

            Assert.AreEqual("NaN", Complex.NaN.ToString("#.000", culture));
            Assert.AreEqual("Infinity", Complex.Infinity.ToString("#.000", culture));
            Assert.AreEqual(".000", Complex.Zero.ToString("#.000", culture));
            Assert.AreEqual("1.100", new Complex(1.1, 0).ToString("#.000", culture));
            Assert.AreEqual("-1.100i", new Complex(0, -1.1).ToString("#.000", culture));
            Assert.AreEqual("1.100i", new Complex(0, 1.1).ToString("#.000", culture));
            Assert.AreEqual("1.100 + 1.100i", new Complex(1.1, 1.1).ToString("#.000", culture));
        }

        [Test]
        [Row("-1 -2i", -1, -2, "en-US")]
        [Row("-1 - 2i ", -1, -2, "de-CH")]
        public void CanParseStringToComplexWithCulture(
            string text, double expectedReal, double expectedImaginary, string cultureName)
        {
            Complex parsed = Complex.Parse(text, CultureInfo.GetCultureInfo(cultureName));
            Assert.AreEqual(expectedReal, parsed.Real);
            Assert.AreEqual(expectedImaginary, parsed.Imaginary);
        }

        [Test]
        [Row("1", 1, 0)]
        [Row("-1", -1, 0)]
        [Row("-i", 0, -1)]
        [Row("i", 0, 1)]
        [Row("2i", 0, 2)]
        [Row("1 + 2i", 1, 2)]
        [Row("1+2i", 1, 2)]
        [Row("1 - 2i", 1, -2)]
        [Row("1-2i", 1, -2)]
        [Row("1,2 ", 1, 2)]
        [Row("1 , 2", 1, 2)]
        [Row("1,2i", 1, 2)]
        [Row("-1, -2i", -1, -2)]
        [Row(" - 1 , - 2 i ", -1, -2)]
        [Row("(+1,2i)", 1, 2)]
        [Row("(-1 , -2)", -1, -2)]
        [Row("(-1 , -2i)", -1, -2)]
        [Row("(+1e1 , -2e-2i)", 10, -0.02)]
        [Row("(-1E1 -2e2i)", -10, -200)]
        [Row("(-1e+1 -2e2i)", -10, -200)]
        [Row("(-1e1 -2e+2i)", -10, -200)]
        [Row("(-1e-1  -2E2i)", -0.1, -200)]
        [Row("(-1e1  -2e-2i)", -10, -0.02)]
        [Row("(-1E+1 -2e+2i)", -10, -200)]
        [Row("(-1e-1,-2e-2i)", -0.1, -0.02)]
        [Row("(+1 +2i)", 1, 2)]
        [Row("(-1E+1 -2e+2i)", -10, -200)]
        [Row("(-1e-1,-2e-2i)", -0.1, -0.02)]
        public void CanTryParseStringToComplexWithInvariant(string str, double expectedReal, double expectedImaginary)
        {
            var invariantCulture = CultureInfo.InvariantCulture;
            Complex z;
            var ret = Complex.TryParse(str, invariantCulture, out z);
            Assert.IsTrue(ret);
            Assert.AreEqual(expectedReal, z.Real);
            Assert.AreEqual(expectedImaginary, z.Imaginary);
        }

        [Test]
        public void ParseThrowsFormatExceptionIfMissingClosingParen()
        {
            Assert.Throws<FormatException>(() => Complex.Parse("(1,2"));
        }

        [Test]
        public void TryParseCanHandleSymbols()
        {
            Complex z;
            var ni = NumberFormatInfo.CurrentInfo;
            var separator = CultureInfo.CurrentCulture.TextInfo.ListSeparator;
            var ret = Complex.TryParse(
                ni.NegativeInfinitySymbol + separator + ni.PositiveInfinitySymbol, out z);
            Assert.IsTrue(ret, "A1");
            Assert.AreEqual(double.NegativeInfinity, z.Real, "A2");
            Assert.AreEqual(double.PositiveInfinity, z.Imaginary, "A3");

            ret = Complex.TryParse(ni.NaNSymbol + separator + ni.NaNSymbol, out z);
            Assert.IsTrue(ret, "B1");
            Assert.AreEqual(double.NaN, z.Real, "B2");
            Assert.AreEqual(double.NaN, z.Imaginary, "B3");

            ret = Complex.TryParse(ni.NegativeInfinitySymbol + "+" + ni.PositiveInfinitySymbol + "i", out z);
            Assert.IsTrue(ret, "C1");
            Assert.AreEqual(double.NegativeInfinity, z.Real, "C2");
            Assert.AreEqual(double.PositiveInfinity, z.Imaginary, "C3");

            ret = Complex.TryParse(ni.NaNSymbol + "+" + ni.NaNSymbol + "i", out z);
            Assert.IsTrue(ret, "D1");
            Assert.AreEqual(double.NaN, z.Real, "D2");
            Assert.AreEqual(double.NaN, z.Imaginary, "D3");

            ret = Complex.TryParse(
                double.MaxValue.ToString("R") + " " + double.MinValue.ToString("R") + "i",
                out z);
            Assert.IsTrue(ret, "E1");
            Assert.AreEqual(double.MaxValue, z.Real, "E2");
            Assert.AreEqual(double.MinValue, z.Imaginary, "E3");
        }

        [Test]
        [Row("en-US")]
        [Row("tr-TR")]
        [Row("de-DE")]
        [Row("de-CH")]
        [Row("he-IL")]
        public void TryParseCanHandleSymbolsWithCulture(string cultureName)
        {
            Complex z;
            var culture = CultureInfo.GetCultureInfo(cultureName);
            var ni = culture.NumberFormat;
            var separator = culture.TextInfo.ListSeparator;
            var ret = Complex.TryParse(
                ni.NegativeInfinitySymbol + separator + ni.PositiveInfinitySymbol, culture, out z);
            Assert.IsTrue(ret, "A1");
            Assert.AreEqual(double.NegativeInfinity, z.Real, "A2");
            Assert.AreEqual(double.PositiveInfinity, z.Imaginary, "A3");

            ret = Complex.TryParse(ni.NaNSymbol + separator + ni.NaNSymbol, culture, out z);
            Assert.IsTrue(ret, "B1");
            Assert.AreEqual(double.NaN, z.Real, "B2");
            Assert.AreEqual(double.NaN, z.Imaginary, "B3");

            ret = Complex.TryParse(ni.NegativeInfinitySymbol + "+" + ni.PositiveInfinitySymbol + "i", culture, out z);
            Assert.IsTrue(ret, "C1");
            Assert.AreEqual(double.NegativeInfinity, z.Real, "C2");
            Assert.AreEqual(double.PositiveInfinity, z.Imaginary, "C3");

            ret = Complex.TryParse(ni.NaNSymbol + "+" + ni.NaNSymbol + "i", culture, out z);
            Assert.IsTrue(ret, "D1");
            Assert.AreEqual(double.NaN, z.Real, "D2");
            Assert.AreEqual(double.NaN, z.Imaginary, "D3");

            ret = Complex.TryParse(
                double.MaxValue.ToString("R", culture) + " " + double.MinValue.ToString("R", culture) + "i",
                culture,
                out z);
            Assert.IsTrue(ret, "E1");
            Assert.AreEqual(double.MaxValue, z.Real, "E2");
            Assert.AreEqual(double.MinValue, z.Imaginary, "E3");
        }

        [Test]
        [Row("")]
        [Row("+")]
        [Row("1-")]
        [Row("i+")]
        [Row("1/2i")]
        [Row("1i+2i")]
        [Row("i1i")]
        [Row("(1i,2)")]
        [Row("1e+")]
        [Row("1e")]
        [Row("1,")]
        [Row(",1")]
        [Row(null)]
        [Row("()")]
        [Row("(  )")]
        public void TryParseReturnsFalseWhenGivenBadValueWithInvariant(string str)
        {
            Complex z;
            var ret = Complex.TryParse(str, CultureInfo.InvariantCulture, out z);
            Assert.IsFalse(ret);
            Assert.AreEqual(0, z.Real);
            Assert.AreEqual(0, z.Imaginary);
        }
    }
}