// <copyright file="DenseVectorTest.TextHandling.cs" company="Math.NET">
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

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Complex32
{
    using System;
    using System.Globalization;
    using LinearAlgebra.Complex32;
    using MbUnit.Framework;

    [TestFixture]
    public class DenseVectorTextHandlingTest
    {
        [Test]
        [Row("2", "(2, 0)")]
        [Row("(3)", "(3, 0)")]
        [Row("[1,2,3]", "(1, 0),(2, 0),(3, 0)")]
        [Row(" [ 1.1 , 2.1 , 3.1 ] ", "(1.1, 0),(2.1, 0),(3.1, 0)")]
        [Row(" [ -1.1 , 2.1 , +3.1 ] ", "(-1.1, 0),(2.1, 0),(3.1, 0)")]
        [Row(" [1.2,3.4 , 5.6] ", "(1.2, 0),(3.4, 0),(5.6, 0)")]
        [Row("[1+1i,2+1i,3+1i]", "(1, 1),(2, 1),(3, 1)")]
        [Row(" [ 1.1 + 1i , 2.1+1i , 3.1+1i ] ", "(1.1, 1),(2.1, 1),(3.1, 1)")]
        [Row(" [ -1.1 + 1i , 2.1-1i , +3.1+1i ] ", "(-1.1, 1),(2.1, -1),(3.1, 1)")]
        [Row(" [1.2+2.3i ,3.4+4.5i , 5.6+ 6.7i] ", "(1.2, 2.3),(3.4, 4.5),(5.6, 6.7)")]
        public void CanParseComplexDenseVectorsWithInvariant(string stringToParse, string expectedToString)
        {
            var formatProvider = CultureInfo.InvariantCulture;
            DenseVector vector = DenseVector.Parse(stringToParse, formatProvider);

            Assert.AreEqual(expectedToString, vector.ToString(formatProvider));
        }

        [Test]
        [Row(" 1.2 + 1i , 3.4 + 1i , 5.6 + 1i ", "(1.2, 1),(3.4, 1),(5.6, 1)", "en-US")]
        [Row(" 1.2 + 1i ; 3.4 + 1i ; 5.6 + 1i ", "(1.2, 1);(3.4, 1);(5.6, 1)", "de-CH")]
        [Row(" 1,2 + 1i ; 3,4 + 1i ; 5,6 + 1i ", "(1,2, 1);(3,4, 1);(5,6, 1)", "de-DE")]
        public void CanParseComplexDenseVectorsWithCulture(string stringToParse, string expectedToString, string culture)
        {
            var formatProvider = CultureInfo.GetCultureInfo(culture);
            DenseVector vector = DenseVector.Parse(stringToParse, formatProvider);

            Assert.AreEqual(expectedToString, vector.ToString(formatProvider));
        }

        [Test]
        [MultipleAsserts]
        public void ParseThrowsFormatExceptionIfMissingClosingParen()
        {
            Assert.Throws<FormatException>(() => DenseVector.Parse("(1"));
            Assert.Throws<FormatException>(() => DenseVector.Parse("[1"));
        }

        [Test]
        [Row(null)]
        [Row("")]
        [Row(",")]
        [Row("1,")]
        [Row(",1")]
        [Row(",1,2,")]
        [Row("1,,2,,3")]
        [Row("1e+")]
        [Row("1e")]
        [Row("()")]
        [Row("[  ]")]
        public void TryParseReturnsFalseWhenGivenBadValueWithInvariant(string str)
        {
            DenseVector vector;
            var ret = DenseVector.TryParse(str, CultureInfo.InvariantCulture, out vector);
            Assert.IsFalse(ret);
            Assert.IsNull(vector);
        }
    }
}
