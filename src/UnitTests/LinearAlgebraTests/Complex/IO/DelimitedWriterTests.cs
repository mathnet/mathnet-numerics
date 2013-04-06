// <copyright file="DelimitedWriterTests.cs" company="Math.NET">
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

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Complex.IO
{
    using LinearAlgebra.Complex;
    using LinearAlgebra.IO;
    using NUnit.Framework;
    using System;
    using System.Globalization;
    using System.IO;
    using System.Numerics;

    /// <summary>
    /// Delimited writer tests.
    /// </summary>
    [TestFixture]
    public class DelimitedWriterTests
    {
        /// <summary>
        /// Can write comma delimited data.
        /// </summary>
        [Test]
        public void CanWriteCommaDelimitedData()
        {
            var matrix = DenseMatrix.OfArray(new[,] {{new Complex(1.1, 1.1), new Complex(2.2, 2.2), new Complex(3.3, 3.3)}, {new Complex(4.4, 4.4), new Complex(5.5, 5.5), new Complex(6.6, 6.6)}, {new Complex(7.7, 7.7), new Complex(8.8, 8.8), new Complex(9.9, 9.9)}});
            var writer = new DelimitedWriter(',')
                {
                    CultureInfo = CultureInfo.InvariantCulture
                };
            var stream = new MemoryStream();
            writer.WriteMatrix(matrix, stream);
            var data = stream.ToArray();
            var reader = new StreamReader(new MemoryStream(data));
            var text = reader.ReadToEnd();
            var expected = "(1.1, 1.1),(2.2, 2.2),(3.3, 3.3)" + Environment.NewLine
                + "(4.4, 4.4),(5.5, 5.5),(6.6, 6.6)" + Environment.NewLine
                + "(7.7, 7.7),(8.8, 8.8),(9.9, 9.9)";
            Assert.AreEqual(expected, text);
        }

        /// <summary>
        /// Can write period delimited data.
        /// </summary>
        [Test]
        public void CanWritePeriodDelimitedData()
        {
            var matrix = DenseMatrix.OfArray(new[,] {{new Complex(1.1, 1.1), new Complex(2.2, 2.2), new Complex(3.3, 3.3)}, {new Complex(4.4, 4.4), new Complex(5.5, 5.5), new Complex(6.6, 6.6)}, {new Complex(7.7, 7.7), new Complex(8.8, 8.8), new Complex(9.9, 9.9)}});
            var culture = new CultureInfo("tr-TR");
            var writer = new DelimitedWriter('.')
                {
                    CultureInfo = culture
                };
            var stream = new MemoryStream();
            writer.WriteMatrix(matrix, stream);
            var data = stream.ToArray();
            var reader = new StreamReader(new MemoryStream(data));
            var text = reader.ReadToEnd();
            var expected = "(1,1, 1,1).(2,2, 2,2).(3,3, 3,3)" + Environment.NewLine
                + "(4,4, 4,4).(5,5, 5,5).(6,6, 6,6)" + Environment.NewLine
                + "(7,7, 7,7).(8,8, 8,8).(9,9, 9,9)";
            Assert.AreEqual(expected, text);
        }

        /// <summary>
        /// Can write space delimited data.
        /// </summary>
        [Test]
        public void CanWriteSpaceDelimitedData()
        {
            var matrix = DenseMatrix.OfArray(new[,] {{new Complex(1.1, 1.1), new Complex(2.2, 2.2), new Complex(3.3, 3.3)}, {new Complex(4.4, 4.4), new Complex(5.5, 5.5), new Complex(6.6, 6.6)}, {new Complex(7.7, 7.7), new Complex(8.8, 8.8), new Complex(9.9, 9.9)}});
            var writer = new DelimitedWriter(' ')
                {
                    CultureInfo = CultureInfo.InvariantCulture
                };
            var stream = new MemoryStream();
            writer.WriteMatrix(matrix, stream);
            var data = stream.ToArray();
            var reader = new StreamReader(new MemoryStream(data));
            var text = reader.ReadToEnd();
            var expected = "(1.1, 1.1) (2.2, 2.2) (3.3, 3.3)" + Environment.NewLine
                + "(4.4, 4.4) (5.5, 5.5) (6.6, 6.6)" + Environment.NewLine
                + "(7.7, 7.7) (8.8, 8.8) (9.9, 9.9)";
            Assert.AreEqual(expected, text);
        }

        /// <summary>
        /// Can write tab delimited data.
        /// </summary>
        [Test]
        public void CanWriteTabDelimitedData()
        {
            var matrix = DenseMatrix.OfArray(new[,] {{new Complex(1.1, 1.1), new Complex(2.2, 2.2), new Complex(3.3, 3.3)}, {new Complex(4.4, 4.4), new Complex(5.5, 5.5), new Complex(6.6, 6.6)}, {new Complex(7.7, 7.7), new Complex(8.8, 8.8), new Complex(9.9, 9.9)}});
            var headers = new[] {"a", "b", "c"};
            var writer = new DelimitedWriter('\t')
                {
                    ColumnHeaders = headers,
                    CultureInfo = CultureInfo.InvariantCulture
                };
            var stream = new MemoryStream();
            writer.WriteMatrix(matrix, stream);
            var data = stream.ToArray();
            var reader = new StreamReader(new MemoryStream(data));
            var text = reader.ReadToEnd();
            var expected = "a\tb\tc" + Environment.NewLine
                + "(1.1, 1.1)\t(2.2, 2.2)\t(3.3, 3.3)" + Environment.NewLine
                + "(4.4, 4.4)\t(5.5, 5.5)\t(6.6, 6.6)" + Environment.NewLine
                + "(7.7, 7.7)\t(8.8, 8.8)\t(9.9, 9.9)";
            Assert.AreEqual(expected, text);
        }
    }
}
