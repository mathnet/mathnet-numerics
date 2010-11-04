using System;
using System.Collections.Generic;
using System.Text;
using Gallio.Framework;
using MbUnit.Framework;
using MbUnit.Framework.ContractVerifiers;

namespace MathNet.Numerics.UnitTests.StatisticsTests
{
    using Statistics;

    [TestFixture]
    public class PercentileTests
    {

        private static readonly double[] _data = {
                                                     95.1772,
                                                     95.1567,
                                                     95.1937,
                                                     95.1959,
                                                     95.1442,
                                                     95.061,
                                                     95.1591,
                                                     95.1195,
                                                     95.1065,
                                                     95.0925,
                                                     95.199,
                                                     95.1682
                                                 };


        [Test]
        public void CanComputePercentileUsingNistMethod()
        {
            var percentile = new Percentile(_data);
            percentile.Method = PercentileMethod.Nist;
            Assert.AreEqual(95.19807, percentile.Compute(.9));
        }

        [Test]
        public void CanComputePercentileUsingExcelMethod()
        {
            var percentile = new Percentile(_data);
            percentile.Method = PercentileMethod.Excel;
            Assert.AreEqual(95.19568, percentile.Compute(.9));
        }


        [Test]
        public void CanComputePercentileUsingNearestMethod()
        {
            var percentile = new Percentile(_data);
            percentile.Method = PercentileMethod.Nearest;
            Assert.AreEqual(95.1959, percentile.Compute(.9));
        }


        [Test]
        public void CanComputePercentileUsingInterpolationMethod()
        {
            var data = new double[] { 1, 2, 3, 4, 5 };
            var percentile = new Percentile(data);
            percentile.Method = PercentileMethod.Interpolation;
            var values = new[] { .25, .5, .75 };
            var percentiles = percentile.Compute(values);
            Assert.AreEqual(1.75, percentiles[0]);
            Assert.AreEqual(3.0, percentiles[1]);
            Assert.AreEqual(4.25, percentiles[2]);
        }

        [Test]
        public void SmallDataSetThrowArgumentException()
        {
            var data = new double[] { };
            Assert.Throws<ArgumentException>(() => new Percentile(data));
            data = new double[] { 1 };
            Assert.Throws<ArgumentException>(() => new Percentile(data));
            data = new double[] { 1, 2 };
            Assert.Throws<ArgumentException>(() => new Percentile(data));
        }

        [Test]
        public void InvalidPercentileValuesThrowArgumentExecption()
        {
            var percentile = new Percentile(_data);
            Assert.Throws<ArgumentException>(() => percentile.Compute(-0.1));
            Assert.Throws<ArgumentException>(() => percentile.Compute(100.1));
        }
    }
}
