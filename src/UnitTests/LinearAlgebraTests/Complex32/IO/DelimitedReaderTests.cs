namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Complex32.IO
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using LinearAlgebra.Complex32;
    using LinearAlgebra.Complex32.IO;
    using MbUnit.Framework;
    using Numerics;

    [TestFixture]
    public class DelimitedReaderTests
    {
        [Test]
        [MultipleAsserts]
        public void CanParseCommaDelimitedData()
        {
            const string data =
                @"a,b,c
(1,2)
""2.2"",0.3e1
'(4,-5)',5,6
";
            var reader = new DelimitedReader<DenseMatrix>(',')
                         {
                             HasHeaderRow = true
                         };
            var matrix = reader.ReadMatrix(new MemoryStream(Encoding.UTF8.GetBytes(data)));
            Assert.AreEqual(3, matrix.RowCount);
            Assert.AreEqual(3, matrix.ColumnCount);
            Assert.AreEqual(1.0f, matrix[0, 0].Real);
            Assert.AreEqual(2.0f, matrix[0, 0].Imaginary);
            Assert.AreEqual(0.0f, matrix[0, 1]);
            Assert.AreEqual(0.0f, matrix[0, 2]);
            Assert.AreEqual(2.2f, matrix[1, 0]);
            Assert.AreEqual(3.0f, matrix[1, 1]);
            Assert.AreEqual(0.0f, matrix[1, 2]);
            Assert.AreEqual(4.0f, matrix[2, 0].Real);
            Assert.AreEqual(-5.0f, matrix[2, 0].Imaginary);
            Assert.AreEqual(5.0f, matrix[2, 1]);
            Assert.AreEqual(6.0f, matrix[2, 2]);
        }

        [Test]
        [MultipleAsserts]
        public void CanParseTabDelimtedData()
        {
            var data = "1"
                       + Environment.NewLine
                       + "\"2.2\"\t\t0.3e1"
                       + Environment.NewLine
                       + "'4'\t5\t6";

            var reader = new DelimitedReader<SparseMatrix>('\t');
            var matrix = reader.ReadMatrix(new MemoryStream(Encoding.UTF8.GetBytes(data)));
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

        [Test]
        [MultipleAsserts]
        public void CanParseWhiteSpaceDelimitedData()
        {
            const string data =
                @"1
""(2.2,3.3)"" 0.3e1
'4'   5      6
";
            var reader = new DelimitedReader<UserDefinedMatrix>();
            var matrix = reader.ReadMatrix(new MemoryStream(Encoding.UTF8.GetBytes(data)));
            Assert.AreEqual(3, matrix.RowCount);
            Assert.AreEqual(3, matrix.ColumnCount);
            Assert.AreEqual(1.0f, matrix[0, 0]);
            Assert.AreEqual(0.0f, matrix[0, 1]);
            Assert.AreEqual(0.0f, matrix[0, 2]);
            Assert.AreEqual(2.2f, matrix[1, 0].Real);
            Assert.AreEqual(3.3f, matrix[1, 0].Imaginary);
            Assert.AreEqual(3.0f, matrix[1, 1]);
            Assert.AreEqual(0.0f, matrix[1, 2]);
            Assert.AreEqual(4.0f, matrix[2, 0]);
            Assert.AreEqual(5.0f, matrix[2, 1]);
            Assert.AreEqual(6.0f, matrix[2, 2]);
        }

        [Test]
        [MultipleAsserts]
        public void CanParsePeriodDelimitedData()
        {
            const string data =
                @"a.b.c
1
""2,2"".0,3e1+0,2e1i
'4,0'.5,0.6,0
";
            var reader = new DelimitedReader<DenseMatrix>('.')
                         {
                             HasHeaderRow = true, 
                             CultureInfo = new CultureInfo("tr-TR")
                         };

            var matrix = reader.ReadMatrix(new MemoryStream(Encoding.UTF8.GetBytes(data)));
            Assert.AreEqual(3, matrix.RowCount);
            Assert.AreEqual(3, matrix.ColumnCount);
            Assert.AreEqual(1.0f, matrix[0, 0]);
            Assert.AreEqual(0.0f, matrix[0, 1]);
            Assert.AreEqual(0.0f, matrix[0, 2]);
            Assert.AreEqual(2.2f, matrix[1, 0]);
            Assert.AreEqual(3.0f, matrix[1, 1].Real);
            Assert.AreEqual(2.0f, matrix[1, 1].Imaginary);
            Assert.AreEqual(0.0f, matrix[1, 2]);
            Assert.AreEqual(4.0f, matrix[2, 0]);
            Assert.AreEqual(5.0f, matrix[2, 1]);
            Assert.AreEqual(6.0f, matrix[2, 2]);
        }
    }
}
