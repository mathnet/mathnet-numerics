// <copyright file="DenseVectorTest.TextHandling.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
//
// Copyright (c) 2009-2013 Math.NET
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
    using LinearAlgebra.Double;
    using NUnit.Framework;
    using System;
    using System.Globalization;

    /// <summary>
    /// Dense vector text handling tests.
    /// </summary>
    [TestFixture]
    public class DenseVectorTextHandlingTest
    {
        /// <summary>
        /// Can parse a double dense vectors with invariant culture.
        /// </summary>
        /// <param name="stringToParse">String to parse.</param>
        /// <param name="expectedToString">Expected result.</param>
        [TestCase("2", "2")]
        [TestCase("(3)", "3")]
        [TestCase("[1,2,3]", "1 2 3")]
        [TestCase(" [ 1 , 2 , 3 ] ", "1 2 3")]
        [TestCase(" [ -1 , 2 , +3 ] ", "-1 2 3")]
        [TestCase(" [1.2,3.4 , 5.6] ", "1.2 3.4 5.6")]
        public void CanParseDoubleDenseVectorsWithInvariant(string stringToParse, string expectedToString)
        {
            var formatProvider = CultureInfo.InvariantCulture;
            var vector = DenseVector.Parse(stringToParse, formatProvider);

            Assert.AreEqual(expectedToString, vector.ToVectorString(1, int.MaxValue, 1, "G", formatProvider));
        }

        /// <summary>
        /// Can parse a double dense vectors with culture.
        /// </summary>
        /// <param name="stringToParse">String to parse.</param>
        /// <param name="expectedToString">Expected result.</param>
        /// <param name="culture">Culture name.</param>
        [TestCase(" 1.2,3.4 , 5.6 ", "1.2 3.4 5.6", "en-US")]
        //[TestCase(" 1.2;3.4 ; 5.6 ", "1.2 3.4 5.6", "de-CH")] Windows 8.1 issue, see http://bit.ly/W81deCH
        [TestCase(" 1,2;3,4 ; 5,6 ", "1,2 3,4 5,6", "de-DE")]
        public void CanParseDoubleDenseVectorsWithCulture(string stringToParse, string expectedToString, string culture)
        {
            var formatProvider = new CultureInfo(culture);
            var vector = DenseVector.Parse(stringToParse, formatProvider);

            Assert.AreEqual(expectedToString, vector.ToVectorString(1, int.MaxValue, 1, "G", formatProvider));
        }

        /// <summary>
        /// Can parse double dense vectors.
        /// </summary>
        /// <param name="vectorAsString">Vector as string.</param>
        [TestCase("15")]
        [TestCase("1{0}2 3{0}4 5{0}6")]
        public void CanParseDoubleDenseVectors(string vectorAsString)
        {
            var mappedString = String.Format(
                vectorAsString,
                CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);

            var vector = DenseVector.Parse(mappedString);

            Assert.AreEqual(mappedString, vector.ToVectorString(1, int.MaxValue, 1));
        }

        /// <summary>
        /// Parse if missing closing paren throws <c>FormatException</c>.
        /// </summary>
        [Test]
        public void ParseIfMissingClosingParenThrowsFormatException()
        {
            Assert.Throws<FormatException>(() => DenseVector.Parse("(1"));
            Assert.Throws<FormatException>(() => DenseVector.Parse("[1"));
        }

        /// <summary>
        /// Can try parse a double dense vector.
        /// </summary>
        [Test]
        public void CanTryParseDoubleDenseVector()
        {
            var data = new[] { 1.2, 3.4, 5.6e-78 };
            var text = String.Format(
                "{1}{0}{2}{0}{3}",
                CultureInfo.CurrentCulture.TextInfo.ListSeparator,
                data[0],
                data[1],
                data[2]);

            DenseVector vector;
            var ret = DenseVector.TryParse(text, out vector);
            Assert.IsTrue(ret);
            AssertHelpers.ListAlmostEqualRelative(data, (double[])vector, 14);

            ret = DenseVector.TryParse(text, CultureInfo.CurrentCulture, out vector);
            Assert.IsTrue(ret);
            AssertHelpers.ListAlmostEqualRelative(data, (double[])vector, 14);
        }

        /// <summary>
        /// Try parse a bad value with invariant returns <c>false</c>.
        /// </summary>
        /// <param name="str">Input string.</param>
        [TestCase(null)]
        [TestCase("")]
        [TestCase(",")]
        [TestCase("1e+")]
        [TestCase("1e")]
        [TestCase("()")]
        [TestCase("[  ]")]
        public void TryParseBadValueWithInvariantReturnsFalse(string str)
        {
            DenseVector vector;
            var ret = DenseVector.TryParse(str, CultureInfo.InvariantCulture, out vector);
            Assert.IsFalse(ret);
            Assert.IsNull(vector);
        }
    }
}
