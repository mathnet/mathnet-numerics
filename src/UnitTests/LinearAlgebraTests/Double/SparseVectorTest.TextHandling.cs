// <copyright file="SparseVector.TextHandling.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
//
// Copyright (c) 2009-2010 Math.NET
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

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Double
{
    using System;
    using System.Globalization;
    using LinearAlgebra.Double;
    using MbUnit.Framework;

    public class SparseVectorTextHandlingTest
    {
        [Test]
        [Row("2", "2")]
        [Row("(3)", "3")]
        [Row("[1,2,3]", "1,2,3")]
        [Row(" [ 1 , 2 , 3 ] ", "1,2,3")]
        [Row(" [ -1 , 2 , +3 ] ", "-1,2,3")]
        [Row(" [1.2,3.4 , 5.6] ", "1.2,3.4,5.6")]
        public void CanParseDoubleSparseVectorsWithInvariant(string stringToParse, string expectedToString)
        {
            var formatProvider = CultureInfo.InvariantCulture;
            SparseVector vector = SparseVector.Parse(stringToParse, formatProvider);

            Assert.AreEqual(expectedToString, vector.ToString(formatProvider));
        }

        [Test]
        [Row(" 1.2,3.4 , 5.6 ", "1.2,3.4,5.6", "en-US")]
        [Row(" 1.2;3.4 ; 5.6 ", "1.2;3.4;5.6", "de-CH")]
        [Row(" 1,2;3,4 ; 5,6 ", "1,2;3,4;5,6", "de-DE")]
        public void CanParseDoubleSparseVectorsWithCulture(string stringToParse, string expectedToString, string culture)
        {
            var formatProvider = CultureInfo.GetCultureInfo(culture);
            SparseVector vector = SparseVector.Parse(stringToParse, formatProvider);

            Assert.AreEqual(expectedToString, vector.ToString(formatProvider));
        }

        [Test]
        [Row("15")]
        [Row("1{0}2{1}3{0}4{1}5{0}6")]
        public void CanParseDoubleSparseVectors(string vectorAsString)
        {
            var mappedString = String.Format(
                vectorAsString,
                CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator,
                CultureInfo.CurrentCulture.TextInfo.ListSeparator);

            SparseVector vector = SparseVector.Parse(mappedString);

            Assert.AreEqual(mappedString, vector.ToString());
        }

        [Test]
        [MultipleAsserts]
        public void ParseThrowsFormatExceptionIfMissingClosingParen()
        {
            Assert.Throws<FormatException>(() => SparseVector.Parse("(1"));
            Assert.Throws<FormatException>(() => SparseVector.Parse("[1"));
        }

        [Test]
        public void CanTryParseDoubleSparseVector()
        {
            var data = new[] { 1.2, 3.4, 5.6e-78 };
            var text = String.Format(
                "{1}{0}{2}{0}{3}",
                CultureInfo.CurrentCulture.TextInfo.ListSeparator,
                data[0],
                data[1],
                data[2]);

            SparseVector vector;
            var ret = SparseVector.TryParse(text, out vector);
            Assert.IsTrue(ret);
            AssertHelpers.AlmostEqualList(data, vector.ToArray(), 1e-15);

            ret = SparseVector.TryParse(text, CultureInfo.CurrentCulture, out vector);
            Assert.IsTrue(ret);
            AssertHelpers.AlmostEqualList(data, vector.ToArray(), 1e-15);
        }

        [Test]
        [Row(null)]
        [Row("")]
        [Row(",")]
        [Row("1,")]
        [Row(",1")]
        [Row("1,2,")]
        [Row(",1,2,")]
        [Row("1,,2,,3")]
        [Row("1e+")]
        [Row("1e")]
        [Row("()")]
        [Row("[  ]")]
        public void TryParseReturnsFalseWhenGivenBadValueWithInvariant(string str)
        {
            SparseVector vector;
            var ret = SparseVector.TryParse(str, CultureInfo.InvariantCulture, out vector);
            Assert.IsFalse(ret);
            Assert.IsNull(vector);
        }
    }
}
