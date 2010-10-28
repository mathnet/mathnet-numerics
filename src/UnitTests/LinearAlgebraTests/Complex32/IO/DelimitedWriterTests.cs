namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Complex32.IO
{
    using System;
    using System.Globalization;
    using System.IO;
    using LinearAlgebra.Complex32;
    using LinearAlgebra.Complex32.IO;
    using Numerics;
    using MbUnit.Framework;

    [TestFixture]
    public class DelimitedWriterTests
    {
        [Test]
        public void CanWriteCommaDelimitedData()
        {
            var matrix = new DenseMatrix(new[,] { { new Complex32(1.1f, 1.1f), new Complex32(2.2f, 2.2f), new Complex32(3.3f, 3.3f) }, { new Complex32(4.4f, 4.4f), new Complex32(5.5f, 5.5f), new Complex32(6.6f, 6.6f) }, { new Complex32(7.7f, 7.7f), new Complex32(8.8f, 8.8f), new Complex32(9.9f, 9.9f) } });
            var writer = new DelimitedWriter(',');
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

        [Test]
        public void CanWritePeriodDelimitedData()
        {
            var matrix = new DenseMatrix(new[,] { { new Complex32(1.1f, 1.1f), new Complex32(2.2f, 2.2f), new Complex32(3.3f, 3.3f) }, { new Complex32(4.4f, 4.4f), new Complex32(5.5f, 5.5f), new Complex32(6.6f, 6.6f) }, { new Complex32(7.7f, 7.7f), new Complex32(8.8f, 8.8f), new Complex32(9.9f, 9.9f) } });
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

        [Test]
        public void CanWriteSpaceDelimitedData()
        {
            var matrix = new DenseMatrix(new[,] { { new Complex32(1.1f, 1.1f), new Complex32(2.2f, 2.2f), new Complex32(3.3f, 3.3f) }, { new Complex32(4.4f, 4.4f), new Complex32(5.5f, 5.5f), new Complex32(6.6f, 6.6f) }, { new Complex32(7.7f, 7.7f), new Complex32(8.8f, 8.8f), new Complex32(9.9f, 9.9f) } });
            var writer = new DelimitedWriter(' ');
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

        [Test]
        public void CanWriteTabDelimitedData()
        {
            var matrix = new DenseMatrix(new[,] { { new Complex32(1.1f, 1.1f), new Complex32(2.2f, 2.2f), new Complex32(3.3f, 3.3f) }, { new Complex32(4.4f, 4.4f), new Complex32(5.5f, 5.5f), new Complex32(6.6f, 6.6f) }, { new Complex32(7.7f, 7.7f), new Complex32(8.8f, 8.8f), new Complex32(9.9f, 9.9f) } });
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
            var expected = "a\tb\tc" + Environment.NewLine
                           + "(1.1, 1.1)\t(2.2, 2.2)\t(3.3, 3.3)" + Environment.NewLine
                           + "(4.4, 4.4)\t(5.5, 5.5)\t(6.6, 6.6)" + Environment.NewLine
                           + "(7.7, 7.7)\t(8.8, 8.8)\t(9.9, 9.9)";
            Assert.AreEqual(expected, text);
        }
    }
}
