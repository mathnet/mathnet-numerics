// <copyright file="DelimitedWriterTests.cs" company="Math.NET">
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
namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Single.IO
{
    using System;
    using System.Globalization;
    using System.IO;
    using LinearAlgebra.IO;
    using LinearAlgebra.Single;
    using NUnit.Framework;

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
            var matrix = new DenseMatrix(new[,] { { 1.1f, 2.2f, 3.3f }, { 4.4f, 5.5f, 6.6f }, { 7.7f, 8.8f, 9.9f } });
            var writer = new DelimitedWriter(',')
                         {
                             CultureInfo = CultureInfo.InvariantCulture
                         };
            var stream = new MemoryStream();
            writer.WriteMatrix(matrix, stream);
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
            var matrix = new DenseMatrix(new[,] { { 1.1f, 2.2f, 3.3f }, { 4.4f, 5.5f, 6.6f }, { 7.7f, 8.8f, 9.9f } });
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
            var matrix = new SparseMatrix(new[,] { { 1.1f, 0, 0 }, { 0, 5.5f, 0 }, { 0, 0, 9.9f } });
            var writer = new DelimitedWriter(' ')
                         {
                             CultureInfo = CultureInfo.InvariantCulture
                         };
            var stream = new MemoryStream();
            writer.WriteMatrix(matrix, stream);
            var data = stream.ToArray();
            var reader = new StreamReader(new MemoryStream(data));
            var text = reader.ReadToEnd();
            var expected = @"1.1 0 0" + Environment.NewLine
                           + "0 5.5 0" + Environment.NewLine
                           + "0 0 9.9";
            Assert.AreEqual(expected, text);
        }

        /// <summary>
        /// Can write tab delimited data.
        /// </summary>
        [Test]
        public void CanWriteTabDelimitedData()
        {
            var matrix = new UserDefinedMatrix(new[,] { { 1.1f, 2.2f, 3.3f }, { 4.4f, 5.5f, 6.6f }, { 7.7f, 8.8f, 9.9f } });
            var headers = new[] { "a", "b", "c" };
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
                           + "1.1\t2.2\t3.3" + Environment.NewLine
                           + "4.4\t5.5\t6.6" + Environment.NewLine
                           + "7.7\t8.8\t9.9";
            Assert.AreEqual(expected, text);
        }
    }
}
