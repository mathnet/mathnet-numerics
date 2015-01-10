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

using System;
using System.Globalization;
using System.IO;
using System.Numerics;
using MathNet.Numerics.Data.Text;
using MathNet.Numerics.LinearAlgebra.Double;
using NUnit.Framework;

namespace MathNet.Numerics.Data.UnitTests.Text
{
    /// <summary>
    /// Delimited writer tests.
    /// </summary>
    [TestFixture]
    public class DelimitedWriterTests
    {
        [Test]
        public void CanWriteCommaDelimitedComplex32Data()
        {
            var matrix =
                LinearAlgebra.Complex32.DenseMatrix.OfArray(new[,]
                    {
                        {new Complex32(1.1f, 1.1f), new Complex32(2.2f, 2.2f), new Complex32(3.3f, 3.3f)},
                        {new Complex32(4.4f, 4.4f), new Complex32(5.5f, 5.5f), new Complex32(6.6f, 6.6f)},
                        {new Complex32(7.7f, 7.7f), new Complex32(8.8f, 8.8f), new Complex32(9.9f, 9.9f)}
                    });
            var stream = new MemoryStream();
            DelimitedWriter.Write(stream, matrix, ",", formatProvider: CultureInfo.InvariantCulture);
            var data = stream.ToArray();
            var reader = new StreamReader(new MemoryStream(data));
            var text = reader.ReadToEnd();
            var expected = "(1.1, 1.1),(2.2, 2.2),(3.3, 3.3)" + Environment.NewLine
                           + "(4.4, 4.4),(5.5, 5.5),(6.6, 6.6)" + Environment.NewLine
                           + "(7.7, 7.7),(8.8, 8.8),(9.9, 9.9)";
            Assert.AreEqual(expected, text);
        }

        [Test]
        public void CanWriteCommaDelimitedComplexData()
        {
            var matrix =
                LinearAlgebra.Complex.DenseMatrix.OfArray(new[,]
                    {
                        {new Complex(1.1, 1.1), new Complex(2.2, 2.2), new Complex(3.3, 3.3)},
                        {new Complex(4.4, 4.4), new Complex(5.5, 5.5), new Complex(6.6, 6.6)},
                        {new Complex(7.7, 7.7), new Complex(8.8, 8.8), new Complex(9.9, 9.9)}
                    });
            var stream = new MemoryStream();
            DelimitedWriter.Write(stream, matrix, ",", formatProvider: CultureInfo.InvariantCulture);
            var data = stream.ToArray();
            var reader = new StreamReader(new MemoryStream(data));
            var text = reader.ReadToEnd();
            var expected = "(1.1, 1.1),(2.2, 2.2),(3.3, 3.3)" + Environment.NewLine
                           + "(4.4, 4.4),(5.5, 5.5),(6.6, 6.6)" + Environment.NewLine
                           + "(7.7, 7.7),(8.8, 8.8),(9.9, 9.9)";
            Assert.AreEqual(expected, text);
        }

        [Test]
        public void CanWriteCommaDelimitedDoubleData()
        {
            var matrix = DenseMatrix.OfArray(new[,] {{1.1, 2.2, 3.3}, {4.4, 5.5, 6.6}, {7.7, 8.8, 9.9}});
            var stream = new MemoryStream();
            DelimitedWriter.Write(stream, matrix, ",", formatProvider: CultureInfo.InvariantCulture);
            var data = stream.ToArray();
            var reader = new StreamReader(new MemoryStream(data));
            var text = reader.ReadToEnd();
            var expected = @"1.1,2.2,3.3" + Environment.NewLine
                           + "4.4,5.5,6.6" + Environment.NewLine
                           + "7.7,8.8,9.9";
            Assert.AreEqual(expected, text);
        }

        [Test]
        public void CanWriteCommaDelimitedSingleData()
        {
            var matrix = LinearAlgebra.Single.DenseMatrix.OfArray(new[,] {{1.1f, 2.2f, 3.3f}, {4.4f, 5.5f, 6.6f}, {7.7f, 8.8f, 9.9f}});
            var stream = new MemoryStream();
            DelimitedWriter.Write(stream, matrix, ",", formatProvider: CultureInfo.InvariantCulture);
            var data = stream.ToArray();
            var reader = new StreamReader(new MemoryStream(data));
            var text = reader.ReadToEnd();
            var expected = @"1.1,2.2,3.3" + Environment.NewLine
                           + "4.4,5.5,6.6" + Environment.NewLine
                           + "7.7,8.8,9.9";
            Assert.AreEqual(expected, text);
        }

        /// <summary>
        /// Can write period delimited data.
        /// </summary>
        [Test]
        public void CanWritePeriodDelimitedData()
        {
            var matrix = DenseMatrix.OfArray(new[,] {{1.1, 2.2, 3.3}, {4.4, 5.5, 6.6}, {7.7, 8.8, 9.9}});
            var stream = new MemoryStream();
            DelimitedWriter.Write(stream, matrix, delimiter: ".", formatProvider: new CultureInfo("tr-TR"));
            var data = stream.ToArray();
            var reader = new StreamReader(new MemoryStream(data));
            var text = reader.ReadToEnd();
            var expected = @"1,1.2,2.3,3" + Environment.NewLine
                           + "4,4.5,5.6,6" + Environment.NewLine
                           + "7,7.8,8.9,9";
            Assert.AreEqual(expected, text);
        }

        /// <summary>
        /// Can write space delimited data.
        /// </summary>
        [Test]
        public void CanWriteSpaceDelimitedData()
        {
            var matrix = SparseMatrix.OfArray(new[,] {{1.1, 0, 0}, {0, 5.5, 0}, {0, 0, 9.9}});
            var stream = new MemoryStream();
            DelimitedWriter.Write(stream, matrix, " ");
            var data = stream.ToArray();
            var reader = new StreamReader(new MemoryStream(data));
            var text = reader.ReadToEnd();
            var expected = @"1.1 0 0" + Environment.NewLine
                           + "0 5.5 0" + Environment.NewLine
                           + "0 0 9.9";
            Assert.AreEqual(expected, text);
        }

        /// <summary>
        /// Can write comma delimited data with missing values.
        /// </summary>
        [Test]
        public void CanWriteCommaDelimitedDataWithMissingValues()
        {
            var matrix = SparseMatrix.OfArray(new[,] { { 1.1, 0, 0 }, { 0, 5.5, 0 }, { 0, 0, 9.9 } });
            var stream = new MemoryStream();
            DelimitedWriter.Write(stream, matrix, ",", missingValue: 0);
            var data = stream.ToArray();
            var reader = new StreamReader(new MemoryStream(data));
            var text = reader.ReadToEnd();
            var expected = @"1.1,," + Environment.NewLine
                           + ",5.5," + Environment.NewLine
                           + ",,9.9";
            Assert.AreEqual(expected, text);
        }


        /// <summary>
        /// Can write space delimited data with missing values.
        /// </summary>
        [Test]
        public void CanWriteTabDelimitedDataWithMissingValues()
        {
            var matrix = DenseMatrix.OfArray(new[,] { { 1.1, Double.NaN, 0 }, { 0, 5.5, 0 }, { Double.NaN, Double.NaN, 9.9 } });
            var stream = new MemoryStream();
            DelimitedWriter.Write(stream, matrix, "\t", missingValue: Double.NaN);
            var data = stream.ToArray();
            var reader = new StreamReader(new MemoryStream(data));
            var text = reader.ReadToEnd();
            var expected = "1.1\t\t0" + Environment.NewLine
                           + "0\t5.5\t0" + Environment.NewLine
                           + "\t\t9.9";
            Assert.AreEqual(expected, text);
        }
    }
}