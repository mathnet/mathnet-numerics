namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Single.IO
{
    using System;
    using System.Globalization;
    using System.IO;
    using LinearAlgebra.Single;
    using LinearAlgebra.Single.IO;
    using MbUnit.Framework;

    [TestFixture]
    public class DelimitedWriterTests
    {
        [Test]
        public void CanWriteCommaDelimitedData()
        {
            var matrix = new DenseMatrix(new[,] { { 1.1f, 2.2f, 3.3f }, { 4.4f, 5.5f, 6.6f }, { 7.7f, 8.8f, 9.9f } });
            var writer = new DelimitedWriter(',');
            var stream = new MemoryStream();
            writer.WriteMatrix(matrix, stream);
            var data = stream.ToArray();
            var reader = new StreamReader(new MemoryStream(data));
            var text = reader.ReadToEnd();
            const string expected = @"1.1,2.2,3.3
4.4,5.5,6.6
7.7,8.8,9.9";
            Assert.AreEqual(expected, text);
        }

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
            const string expected = @"1,1.2,2.3,3
4,4.5,5.6,6
7,7.8,8.9,9";
            Assert.AreEqual(expected, text);
        }

        [Test]
        public void CanWriteSpaceDelimitedData()
        {
            var matrix = new SparseMatrix(new[,] { { 1.1f, 0, 0 }, { 0, 5.5f, 0 }, { 0, 0, 9.9f } });
            var writer = new DelimitedWriter(' ');
            var stream = new MemoryStream();
            writer.WriteMatrix(matrix, stream);
            var data = stream.ToArray();
            var reader = new StreamReader(new MemoryStream(data));
            var text = reader.ReadToEnd();
            const string expected = @"1.1 0 0
0 5.5 0
0 0 9.9";
            Assert.AreEqual(expected, text);
        }

        [Test]
        public void CanWriteTabDelimitedData()
        {
            var matrix = new UserDefinedMatrix(new[,] { { 1.1f, 2.2f, 3.3f }, { 4.4f, 5.5f, 6.6f }, { 7.7f, 8.8f, 9.9f } });
            var headers = new[] { "a", "b", "c" };
            var writer = new DelimitedWriter('\t')
                         {
                             ColumnHeaders = headers
                         };
            var stream = new MemoryStream();
            writer.WriteMatrix(matrix, stream);
            var data = stream.ToArray();
            var reader = new StreamReader(new MemoryStream(data));
            var text = reader.ReadToEnd();
            var expected = "a\tb\tc"
                + Environment.NewLine
                + "1.1\t2.2\t3.3"
                + Environment.NewLine
                + "4.4\t5.5\t6.6" 
                + Environment.NewLine 
                + "7.7\t8.8\t9.9";
            Assert.AreEqual(expected, text);
        }
    }
}
