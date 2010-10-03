namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Complex.IO
{
    using System;
    using System.Globalization;
    using System.Numerics;
    using System.IO;
    using LinearAlgebra.Complex;
    using LinearAlgebra.IO;
    using MbUnit.Framework;

    [TestFixture]
    public class DelimitedWriterTests
    {
        [Test]
        public void CanWriteCommaDelimitedData()
        {
            var matrix = new DenseMatrix(new[,] { { new Complex(1.1, 1.1), new Complex(2.2, 2.2), new Complex(3.3, 3.3) }, { new Complex(4.4, 4.4), new Complex(5.5, 5.5), new Complex(6.6, 6.6) }, { new Complex(7.7, 7.7), new Complex(8.8, 8.8), new Complex(9.9, 9.9) } });
            var writer = new DelimitedWriter<Complex>(',');
            var stream = new MemoryStream();
            writer.WriteMatrix(matrix, stream);
            var data = stream.ToArray();
            var reader = new StreamReader(new MemoryStream(data));
            var text = reader.ReadToEnd();
            const string expected = @"(1.1, 1.1),(2.2, 2.2),(3.3, 3.3)
(4.4, 4.4),(5.5, 5.5),(6.6, 6.6)
(7.7, 7.7),(8.8, 8.8),(9.9, 9.9)";
            Assert.AreEqual(expected, text);
        }

        [Test]
        public void CanWritePeriodDelimitedData()
        {
            var matrix = new DenseMatrix(new[,] { { new Complex(1.1, 1.1), new Complex(2.2, 2.2), new Complex(3.3, 3.3) }, { new Complex(4.4, 4.4), new Complex(5.5, 5.5), new Complex(6.6, 6.6) }, { new Complex(7.7, 7.7), new Complex(8.8, 8.8), new Complex(9.9, 9.9) } });
            var culture = new CultureInfo("tr-TR");
            var writer = new DelimitedWriter<Complex>('.')
                         {
                             CultureInfo = culture
                         };
            var stream = new MemoryStream();
            writer.WriteMatrix(matrix, stream);
            var data = stream.ToArray();
            var reader = new StreamReader(new MemoryStream(data));
            var text = reader.ReadToEnd();
            const string expected = @"(1,1, 1,1).(2,2, 2,2).(3,3, 3,3)
(4,4, 4,4).(5,5, 5,5).(6,6, 6,6)
(7,7, 7,7).(8,8, 8,8).(9,9, 9,9)";
            Assert.AreEqual(expected, text);
        }

        [Test]
        public void CanWriteSpaceDelimitedData()
        {
            var matrix = new DenseMatrix(new[,] { { new Complex(1.1, 1.1), new Complex(2.2, 2.2), new Complex(3.3, 3.3) }, { new Complex(4.4, 4.4), new Complex(5.5, 5.5), new Complex(6.6, 6.6) }, { new Complex(7.7, 7.7), new Complex(8.8, 8.8), new Complex(9.9, 9.9) } });
            var writer = new DelimitedWriter<Complex>(' ');
            var stream = new MemoryStream();
            writer.WriteMatrix(matrix, stream);
            var data = stream.ToArray();
            var reader = new StreamReader(new MemoryStream(data));
            var text = reader.ReadToEnd();
            const string expected = @"(1.1, 1.1) (2.2, 2.2) (3.3, 3.3)
(4.4, 4.4) (5.5, 5.5) (6.6, 6.6)
(7.7, 7.7) (8.8, 8.8) (9.9, 9.9)";
            Assert.AreEqual(expected, text);
        }

        [Test]
        public void CanWriteTabDelimitedData()
        {
            var matrix = new DenseMatrix(new[,] { { new Complex(1.1, 1.1), new Complex(2.2, 2.2), new Complex(3.3, 3.3) }, { new Complex(4.4, 4.4), new Complex(5.5, 5.5), new Complex(6.6, 6.6) }, { new Complex(7.7, 7.7), new Complex(8.8, 8.8), new Complex(9.9, 9.9) } });
            var headers = new[] { "a", "b", "c" };
            var writer = new DelimitedWriter<Complex>('\t')
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
                + "(1.1, 1.1)\t(2.2, 2.2)\t(3.3, 3.3)"
                + Environment.NewLine
                + "(4.4, 4.4)\t(5.5, 5.5)\t(6.6, 6.6)"
                + Environment.NewLine
                + "(7.7, 7.7)\t(8.8, 8.8)\t(9.9, 9.9)";
            Assert.AreEqual(expected, text);
        }
    }
}
