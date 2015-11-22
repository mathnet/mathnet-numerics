using System.IO;
using System.Runtime.Serialization;
using MathNet.Numerics.Statistics;
using NUnit.Framework;

namespace MathNet.Numerics.UnitTests.StatisticsTests
{
    [TestFixture]
    public class StatisticsSerializationTests
    {
        [Test]
        public void RunningStatisticsDataContractSerializationTest()
        {
            var expected = new RunningStatistics(new[] { 1.0, 2.0, 3.0 });

            var serializer = new DataContractSerializer(typeof(RunningStatistics));
            var stream = new MemoryStream();
            serializer.WriteObject(stream, expected);

            stream.Position = 0;
            var actual = (RunningStatistics)serializer.ReadObject(stream);

            Assert.That(actual.Count, Is.EqualTo(expected.Count));
            Assert.That(actual.Maximum, Is.EqualTo(expected.Maximum));
            Assert.That(actual.Mean, Is.EqualTo(expected.Mean));
        }

        [Test]
        public void RunningStatisticsWithInfinityNaNDataContractSerializationTest()
        {
            var expected = new RunningStatistics(new[] { 1.0, 2.0, 3.0, double.PositiveInfinity, double.NaN });

            var serializer = new DataContractSerializer(typeof(RunningStatistics));
            var stream = new MemoryStream();
            serializer.WriteObject(stream, expected);

            stream.Position = 0;
            var actual = (RunningStatistics)serializer.ReadObject(stream);

            Assert.That(actual.Count, Is.EqualTo(expected.Count));
            Assert.That(actual.Maximum, Is.EqualTo(expected.Maximum));
            Assert.That(actual.Mean, Is.EqualTo(expected.Mean));
        }

        [Test]
        public void DescriptiveStatisticsDataContractSerializationTest()
        {
            var expected = new DescriptiveStatistics(new[] { 1.0, 2.0, 3.0 });

            var serializer = new DataContractSerializer(typeof(DescriptiveStatistics));
            var stream = new MemoryStream();
            serializer.WriteObject(stream, expected);

            stream.Position = 0;
            var actual = (DescriptiveStatistics)serializer.ReadObject(stream);

            Assert.That(actual.Count, Is.EqualTo(expected.Count));
            Assert.That(actual.Maximum, Is.EqualTo(expected.Maximum));
            Assert.That(actual.Mean, Is.EqualTo(expected.Mean));
        }

        [Test]
        public void HistogramDataContractSerializationTest()
        {
            var expected = new Histogram(new[] { 1.0, 2.0, 3.0, 4.0 }, 2);

            var serializer = new DataContractSerializer(typeof(Histogram));
            var stream = new MemoryStream();
            serializer.WriteObject(stream, expected);

            stream.Position = 0;
            var actual = (Histogram)serializer.ReadObject(stream);

            Assert.That(actual.BucketCount, Is.EqualTo(expected.BucketCount));
            Assert.That(actual.DataCount, Is.EqualTo(expected.DataCount));
            Assert.That(actual.LowerBound, Is.EqualTo(expected.LowerBound));
            Assert.That(actual[0].Width, Is.EqualTo(expected[0].Width));
            Assert.That(actual[0].UpperBound, Is.EqualTo(expected[0].UpperBound));
        }
    }
}
