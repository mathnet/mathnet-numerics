// <copyright file="StatisticsSerializationTests.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2016 Math.NET
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
