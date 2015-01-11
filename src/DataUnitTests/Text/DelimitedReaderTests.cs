// <copyright file="DelimitedReaderTests.cs" company="Math.NET">
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
using System.Text;
using MathNet.Numerics.LinearAlgebra.Storage;
using NUnit.Framework;
using MathNet.Numerics.Data.Text;

namespace MathNet.Numerics.Data.UnitTests.Text
{
    /// <summary>
    /// Delimited reader tests.
    /// </summary>
    [TestFixture]
    public class DelimitedReaderTests
    {
        /// <summary>
        /// Can parse comma delimited data.
        /// </summary>
        [Test]
        public void CanParseCommaDelimitedData()
        {
            var data = "a,b,c" + Environment.NewLine
                       + "1" + Environment.NewLine
                       + "\"2.2\",0.3e1" + Environment.NewLine
                       + "'4',5,6" + Environment.NewLine;

            var matrix = DelimitedReader.Read<double>(new MemoryStream(Encoding.UTF8.GetBytes(data)), delimiter: ",", hasHeaders: true, formatProvider: CultureInfo.InvariantCulture);
            Assert.AreEqual(3, matrix.RowCount);
            Assert.AreEqual(3, matrix.ColumnCount);
            Assert.AreEqual(1.0, matrix[0, 0]);
            Assert.AreEqual(double.NaN, matrix[0, 1]);
            Assert.AreEqual(double.NaN, matrix[0, 2]);
            Assert.AreEqual(2.2, matrix[1, 0]);
            Assert.AreEqual(3.0, matrix[1, 1]);
            Assert.AreEqual(double.NaN, matrix[1, 2]);
            Assert.AreEqual(4.0, matrix[2, 0]);
            Assert.AreEqual(5.0, matrix[2, 1]);
            Assert.AreEqual(6.0, matrix[2, 2]);
        }

        /// <summary>
        /// Can parse tab delimited data.
        /// </summary>
        [Test]
        public void CanParseTabDelimitedData()
        {
            var data = "1" + Environment.NewLine
                       + "\"2.2\"\t\t0.3e1" + Environment.NewLine
                       + "'4'\t5\t6";

            var matrix = DelimitedReader.Read<float>(new MemoryStream(Encoding.UTF8.GetBytes(data)), delimiter: "\t", formatProvider: CultureInfo.InvariantCulture, missingValue:0.0f);
            Assert.AreEqual(3, matrix.RowCount);
            Assert.AreEqual(3, matrix.ColumnCount);
            Assert.AreEqual(1.0f, matrix[0, 0]);
            Assert.AreEqual(0.0f, matrix[0, 1]);
            Assert.AreEqual(0.0f, matrix[0, 2]);
            Assert.AreEqual(2.2f, matrix[1, 0]);
            Assert.AreEqual(0.0f, matrix[1, 1]);
            Assert.AreEqual(3.0f, matrix[1, 2]);
            Assert.AreEqual(4.0f, matrix[2, 0]);
            Assert.AreEqual(5.0f, matrix[2, 1]);
            Assert.AreEqual(6.0f, matrix[2, 2]);
        }

        /// <summary>
        /// Can parse white space delimited data.
        /// </summary>
        [Test]
        public void CanParseWhiteSpaceDelimitedData()
        {
            var data = "1" + Environment.NewLine
                       + "\"2.2\" 0.3e1" + Environment.NewLine
                       + "'4'   5      6" + Environment.NewLine;

            var matrix = DelimitedReader.Read<float>(new MemoryStream(Encoding.UTF8.GetBytes(data)), formatProvider: CultureInfo.InvariantCulture, missingValue:0.0f);
            Assert.AreEqual(3, matrix.RowCount);
            Assert.AreEqual(3, matrix.ColumnCount);
            Assert.AreEqual(1.0f, matrix[0, 0]);
            Assert.AreEqual(0.0f, matrix[0, 1]);
            Assert.AreEqual(0.0f, matrix[0, 2]);
            Assert.AreEqual(2.2f, matrix[1, 0]);
            Assert.AreEqual(3.0f, matrix[1, 1]);
            Assert.AreEqual(0.0f, matrix[1, 2]);
            Assert.AreEqual(4.0f, matrix[2, 0]);
            Assert.AreEqual(5.0f, matrix[2, 1]);
            Assert.AreEqual(6.0f, matrix[2, 2]);
        }

        /// <summary>
        /// Can parse period delimited data.
        /// </summary>
        [Test]
        public void CanParsePeriodDelimitedData()
        {
            var data = "a.b.c" + Environment.NewLine
                       + "1" + Environment.NewLine
                       + "\"2,2\".0,3e1" + Environment.NewLine
                       + "'4,0'.5,0.6,0" + Environment.NewLine;

            var matrix = DelimitedReader.Read<double>(new MemoryStream(Encoding.UTF8.GetBytes(data)), delimiter: ".", hasHeaders: true, formatProvider: new CultureInfo("tr-TR"), missingValue:0.0);
            Assert.AreEqual(3, matrix.RowCount);
            Assert.AreEqual(3, matrix.ColumnCount);
            Assert.AreEqual(1.0, matrix[0, 0]);
            Assert.AreEqual(0.0, matrix[0, 1]);
            Assert.AreEqual(0.0, matrix[0, 2]);
            Assert.AreEqual(2.2, matrix[1, 0]);
            Assert.AreEqual(3.0, matrix[1, 1]);
            Assert.AreEqual(0.0, matrix[1, 2]);
            Assert.AreEqual(4.0, matrix[2, 0]);
            Assert.AreEqual(5.0, matrix[2, 1]);
            Assert.AreEqual(6.0, matrix[2, 2]);
        }


        /// <summary>
        /// Can parse comma delimited complex data.
        /// </summary>
        [Test]
        public void CanParseComplexCommaDelimitedData()
        {
            var data = "a,b,c" + Environment.NewLine
                       + "(1,2)" + Environment.NewLine
                       + "\"2.2\",0.3e1" + Environment.NewLine
                       + "'(4,-5)',5,6" + Environment.NewLine;

            var matrix = DelimitedReader.Read<Complex>(new StringReader(data), delimiter: ",", hasHeaders: true, formatProvider: CultureInfo.InvariantCulture);
            var nan = new Complex(double.NaN, double.NaN);
            Assert.AreEqual(3, matrix.RowCount);
            Assert.AreEqual(3, matrix.ColumnCount);
            Assert.AreEqual(1.0, matrix[0, 0].Real);
            Assert.AreEqual(2.0, matrix[0, 0].Imaginary);
            Assert.AreEqual(nan, matrix[0, 1]);
            Assert.AreEqual(nan, matrix[0, 2]);
            Assert.AreEqual((Complex)2.2, matrix[1, 0]);
            Assert.AreEqual((Complex)3.0, matrix[1, 1]);
            Assert.AreEqual(nan, matrix[1, 2]);
            Assert.AreEqual(4.0, matrix[2, 0].Real);
            Assert.AreEqual(-5.0, matrix[2, 0].Imaginary);
            Assert.AreEqual((Complex)5.0, matrix[2, 1]);
            Assert.AreEqual((Complex)6.0, matrix[2, 2]);
        }

        /// <summary>
        /// Can parse comma delimited complex data.
        /// </summary>
        [Test]
        public void CanParseComplex32CommaDelimitedData()
        {
            var data = "a,b,c" + Environment.NewLine
                       + "(1,2)" + Environment.NewLine
                       + "\"2.2\",0.3e1" + Environment.NewLine
                       + "'(4,-5)',5,6" + Environment.NewLine;

            var matrix = DelimitedReader.Read<Complex32>(new MemoryStream(Encoding.UTF8.GetBytes(data)), delimiter: ",", hasHeaders: true, formatProvider: CultureInfo.InvariantCulture, missingValue:Complex32.Zero);
            Assert.AreEqual(3, matrix.RowCount);
            Assert.AreEqual(3, matrix.ColumnCount);
            Assert.AreEqual(1.0f, matrix[0, 0].Real);
            Assert.AreEqual(2.0f, matrix[0, 0].Imaginary);
            Assert.AreEqual((Complex32)0.0f, matrix[0, 1]);
            Assert.AreEqual((Complex32)0.0f, matrix[0, 2]);
            Assert.AreEqual((Complex32)2.2f, matrix[1, 0]);
            Assert.AreEqual((Complex32)3.0f, matrix[1, 1]);
            Assert.AreEqual((Complex32)0.0f, matrix[1, 2]);
            Assert.AreEqual(4.0f, matrix[2, 0].Real);
            Assert.AreEqual(-5.0f, matrix[2, 0].Imaginary);
            Assert.AreEqual((Complex32)5.0f, matrix[2, 1]);
            Assert.AreEqual((Complex32)6.0f, matrix[2, 2]);
        }

        /// <summary>
        /// Can parse comma delimited sparse data.
        /// </summary>
        [Test]
        public void CanParseSparseCommaDelimitedData()
        {
            var data = "a,b,c" + Environment.NewLine
                       + "1" + Environment.NewLine
                       + "\"2.2\",0.3e1" + Environment.NewLine
                       + "'4',0,6" + Environment.NewLine;

            var matrix = DelimitedReader.Read<double>(new StringReader(data), true, ",", true, CultureInfo.InvariantCulture, missingValue:0.0);
            Assert.IsTrue(matrix is LinearAlgebra.Double.SparseMatrix);
            Assert.AreEqual(3, matrix.RowCount);
            Assert.AreEqual(3, matrix.ColumnCount);
            Assert.AreEqual(1.0, matrix[0, 0]);
            Assert.AreEqual(0.0, matrix[0, 1]);
            Assert.AreEqual(0.0, matrix[0, 2]);
            Assert.AreEqual(2.2, matrix[1, 0]);
            Assert.AreEqual(3.0, matrix[1, 1]);
            Assert.AreEqual(0.0, matrix[1, 2]);
            Assert.AreEqual(4.0, matrix[2, 0]);
            Assert.AreEqual(0.0, matrix[2, 1]);
            Assert.AreEqual(6.0, matrix[2, 2]);
        }


        /// <summary>
        /// Can parse comma delimited data with missing data.
        /// </summary>
        [Test]
        public void CanParseMissingDataCommaDelimitedData()
        {
            var data = "a,b,c" + Environment.NewLine
                       + ", , , ," + Environment.NewLine
                       + "\"2.2\", ,   ,0.3e1" + Environment.NewLine
                       + "'4',0,6" + Environment.NewLine
                       + "," + Environment.NewLine
                       + ",,3, 4" + Environment.NewLine
                       + ",, , ,," + Environment.NewLine
                       + ",,,,," + Environment.NewLine
                       + ", ,,, ," + Environment.NewLine
                       + ",  , , ,," + Environment.NewLine;

            var matrix = DelimitedReader.Read<double>(new StringReader(data), false, ",", true, CultureInfo.InvariantCulture);

            Assert.AreEqual(9, matrix.RowCount);
            Assert.AreEqual(5, matrix.ColumnCount);
            Assert.AreEqual(double.NaN, matrix[0, 0]);
            Assert.AreEqual(double.NaN, matrix[0, 1]);
            Assert.AreEqual(double.NaN, matrix[0, 2]);
            Assert.AreEqual(double.NaN, matrix[0, 3]);
            Assert.AreEqual(double.NaN, matrix[0, 4]);
            Assert.AreEqual(2.2, matrix[1, 0]);
            Assert.AreEqual(double.NaN, matrix[1, 1]);
            Assert.AreEqual(double.NaN, matrix[1, 2]);
            Assert.AreEqual(3.0, matrix[1, 3]);
            Assert.AreEqual(double.NaN, matrix[1, 4]);
            Assert.AreEqual(4.0, matrix[2, 0]);
            Assert.AreEqual(0.0, matrix[2, 1]);
            Assert.AreEqual(6.0, matrix[2, 2]);
            Assert.AreEqual(double.NaN, matrix[2, 3]);
            Assert.AreEqual(double.NaN, matrix[2, 4]);
            Assert.AreEqual(double.NaN, matrix[3, 0]);
            Assert.AreEqual(double.NaN, matrix[3, 1]);
            Assert.AreEqual(double.NaN, matrix[3, 2]);
            Assert.AreEqual(double.NaN, matrix[3, 3]);
            Assert.AreEqual(double.NaN, matrix[3, 4]);
            Assert.AreEqual(double.NaN, matrix[4, 0]);
            Assert.AreEqual(double.NaN, matrix[4, 1]);
            Assert.AreEqual(3.0, matrix[4, 2]);
            Assert.AreEqual(4.0, matrix[4, 3]);
            Assert.AreEqual(double.NaN, matrix[4, 4]);
            Assert.AreEqual(double.NaN, matrix[5, 0]);
            Assert.AreEqual(double.NaN, matrix[5, 1]);
            Assert.AreEqual(double.NaN, matrix[5, 2]);
            Assert.AreEqual(double.NaN, matrix[5, 3]);
            Assert.AreEqual(double.NaN, matrix[5, 4]);
            Assert.AreEqual(double.NaN, matrix[6, 0]);
            Assert.AreEqual(double.NaN, matrix[6, 1]);
            Assert.AreEqual(double.NaN, matrix[6, 2]);
            Assert.AreEqual(double.NaN, matrix[6, 3]);
            Assert.AreEqual(double.NaN, matrix[6, 4]);
            Assert.AreEqual(double.NaN, matrix[7, 0]);
            Assert.AreEqual(double.NaN, matrix[7, 1]);
            Assert.AreEqual(double.NaN, matrix[7, 2]);
            Assert.AreEqual(double.NaN, matrix[7, 3]);
            Assert.AreEqual(double.NaN, matrix[7, 4]);
            Assert.AreEqual(double.NaN, matrix[8, 0]);
            Assert.AreEqual(double.NaN, matrix[8, 1]);
            Assert.AreEqual(double.NaN, matrix[8, 2]);
            Assert.AreEqual(double.NaN, matrix[8, 3]);
            Assert.AreEqual(double.NaN, matrix[8, 4]);
        }

        /// <summary>
        /// Can parse backslash delimited sparse data.
        /// </summary>
        [Test]
        public void CanParseSparseBackSlashDelimitedData()
        {
            var data = "a\\b\\c" + Environment.NewLine
                       + "1" + Environment.NewLine
                       + "\"2.2\"\\0.3e1" + Environment.NewLine
                       + "'4'\\0\\6" + Environment.NewLine;

            var matrix = DelimitedReader.Read<double>(new StringReader(data), true, "\\", true, CultureInfo.InvariantCulture);
            Assert.IsTrue(matrix is LinearAlgebra.Double.SparseMatrix);
            Assert.AreEqual(3, matrix.RowCount);
            Assert.AreEqual(3, matrix.ColumnCount);
            Assert.AreEqual(5, ((SparseCompressedRowMatrixStorage<double>)matrix.Storage).ValueCount);
            Assert.AreEqual(1.0, matrix[0, 0]);
            Assert.AreEqual(0.0, matrix[0, 1]);
            Assert.AreEqual(0.0, matrix[0, 2]);
            Assert.AreEqual(2.2, matrix[1, 0]);
            Assert.AreEqual(3.0, matrix[1, 1]);
            Assert.AreEqual(0.0, matrix[1, 2]);
            Assert.AreEqual(4.0, matrix[2, 0]);
            Assert.AreEqual(0.0, matrix[2, 1]);
            Assert.AreEqual(6.0, matrix[2, 2]);
        }

        /// <summary>
        /// Can parse dash delimited sparse data.
        /// </summary>
        [Test]
        public void CanParseSparseDashDelimitedData()
        {
            var data = "a-b-c" + Environment.NewLine
                       + "1" + Environment.NewLine
                       + "\"2.2\"-.3e1" + Environment.NewLine
                       + "'4'-0-6" + Environment.NewLine;

            var matrix = DelimitedReader.Read<double>(new StringReader(data), true, "-", true, CultureInfo.InvariantCulture, missingValue:0.0);
            Assert.IsTrue(matrix is LinearAlgebra.Double.SparseMatrix);
            Assert.AreEqual(3, matrix.RowCount);
            Assert.AreEqual(3, matrix.ColumnCount);
            Assert.AreEqual(5, ((SparseCompressedRowMatrixStorage<double>)matrix.Storage).ValueCount);
            Assert.AreEqual(1.0, matrix[0, 0]);
            Assert.AreEqual(0.0, matrix[0, 1]);
            Assert.AreEqual(0.0, matrix[0, 2]);
            Assert.AreEqual(2.2, matrix[1, 0]);
            Assert.AreEqual(3.0, matrix[1, 1]);
            Assert.AreEqual(0.0, matrix[1, 2]);
            Assert.AreEqual(4.0, matrix[2, 0]);
            Assert.AreEqual(0.0, matrix[2, 1]);
            Assert.AreEqual(6.0, matrix[2, 2]);
        }

        /// <summary>
        /// Can parse underscore delimited sparse data.
        /// </summary>
        [Test]
        public void CanParseUnderscoreDelimitedData()
        {
            var data = "a_b_c" + Environment.NewLine
                       + "1" + Environment.NewLine
                       + "\"2.2\"_.3e1" + Environment.NewLine
                       + "'4'_0_6" + Environment.NewLine;

            var matrix = DelimitedReader.Read<double>(new StringReader(data), false, "_", true, CultureInfo.InvariantCulture, missingValue:0.0f);
            Assert.AreEqual(3, matrix.RowCount);
            Assert.AreEqual(3, matrix.ColumnCount);
            Assert.AreEqual(1.0, matrix[0, 0]);
            Assert.AreEqual(0.0, matrix[0, 1]);
            Assert.AreEqual(0.0, matrix[0, 2]);
            Assert.AreEqual(2.2, matrix[1, 0]);
            Assert.AreEqual(3.0, matrix[1, 1]);
            Assert.AreEqual(0.0, matrix[1, 2]);
            Assert.AreEqual(4.0, matrix[2, 0]);
            Assert.AreEqual(0.0, matrix[2, 1]);
            Assert.AreEqual(6.0, matrix[2, 2]);
        }
    }
}
