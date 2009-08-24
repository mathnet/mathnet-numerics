// <copyright file="WeibullTests.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://mathnet.opensourcedotnet.info
//
// Copyright (c) 2009 Math.NET
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

namespace MathNet.Numerics.UnitTests.DistributionTests
{
    using System;
    using System.Linq;
    using MbUnit.Framework;
    using MathNet.Numerics.Distributions;

    [TestFixture]
    public class WeibullTests
    {
        [SetUp]
        public void SetUp()
        {
            Control.CheckDistributionParameters = true;
        }

        [Test, MultipleAsserts]
        [Row(1.0, 0.1)]
        [Row(1.0, 1.0)]
        [Row(10.0, 10.0)]
        [Row(10.0, 1.0)]
        [Row(10.0, Double.PositiveInfinity)]
        public void CanCreateWeibull(double shape, double scale)
        {
            var n = new Weibull(shape, scale);
            AssertEx.AreEqual<double>(shape, n.Shape);
            AssertEx.AreEqual<double>(scale, n.Scale);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [Row(Double.NaN, 1.0)]
        [Row(1.0, Double.NaN)]
        [Row(Double.NaN, Double.NaN)]
        [Row(1.0, -1.0)]
        [Row(-1.0, 1.0)]
        [Row(-1.0, -1.0)]
        [Row(0.0, 0.0)]
        [Row(0.0, 1.0)]
        [Row(1.0, 0.0)]
        public void WeibullCreateFailsWithBadParameters(double shape, double scale)
        {
            var n = new Weibull(shape, scale);
        }

        [Test]
        public void ValidateToString()
        {
            var n = new Weibull(1.0, 2.0);
            AssertEx.AreEqual<string>("Weibull(Shape = 1, Scale = 2)", n.ToString());
        }

        [Test]
        [Row(0.1)]
        [Row(1.0)]
        [Row(10.0)]
        [Row(Double.PositiveInfinity)]
        public void CanSetShape(double shape)
        {
            var n = new Weibull(1.0, 1.0);
            n.Shape = shape;
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [Row(-0.0)]
        [Row(0.0)]
        [Row(-1.0)]
        public void SetShapeFailsWithNegativeShape(double shape)
        {
            var n = new Weibull(1.0, 1.0);
            n.Shape = shape;
        }

        [Test]
        [Row(0.1)]
        [Row(1.0)]
        [Row(10.0)]
        [Row(Double.PositiveInfinity)]
        public void CanSetScale(double scale)
        {
            var n = new Weibull(1.0, 1.0);
            n.Scale = scale;
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [Row(-0.0)]
        [Row(0.0)]
        [Row(-1.0)]
        public void SetScaleFailsWithNegativeScale(double scale)
        {
            var n = new Weibull(1.0, 1.0);
            n.Scale = scale;
        }

        [Test]
        [Row(1.0, 0.1, 0.1)]
        [Row(1.0, 1.0, 1.0)]
        [Row(10.0, 10.0, 9.5135076986687318362924871772654021925505786260884)]
        [Row(10.0, 1.0, 0.95135076986687318362924871772654021925505786260884)]
        public void ValidateMean(double shape, double scale, double mean)
        {
            var n = new Weibull(shape, scale);
            AssertHelpers.AlmostEqual(mean, n.Mean, 13);
        }

        [Test]
        [Row(1.0, 0.1, 0.01)]
        [Row(1.0, 1.0, 1.0)]
        [Row(10.0, 10.0, 1.3100455073468309147154581687505295026863354547057)]
        [Row(10.0, 1.0, 0.013100455073468309147154581687505295026863354547057)]
        public void ValidateVariance(double shape, double scale, double var)
        {
            var n = new Weibull(shape, scale);
            AssertHelpers.AlmostEqual(var, n.Variance, 13);
        }

        [Test]
        [Row(1.0, 0.1, 0.1)]
        [Row(1.0, 1.0, 1.0)]
        [Row(10.0, 10.0, 1.1445721940300799194124723631014002560036613065794)]
        [Row(10.0, 1.0, 0.11445721940300799194124723631014002560036613065794)]
        public void ValidateStdDev(double shape, double scale, double sdev)
        {
            var n = new Weibull(shape, scale);
            AssertHelpers.AlmostEqual(sdev, n.StdDev, 13);
        }

        [Test]
        [Row(1.0, 0.1, 2.0)]
        [Row(1.0, 1.0, 2.0)]
        [Row(10.0, 10.0, -0.63763713390314440916597757156663888653981696212127)]
        [Row(10.0, 1.0, -0.63763713390314440916597757156663888653981696212127)]
        public void ValidateSkewness(double shape, double scale, double skewness)
        {
            var n = new Weibull(shape, scale);
            AssertHelpers.AlmostEqual(skewness, n.Skewness, 11);
        }

        [Test]
        [Row(1.0, 0.1, 0.0)]
        [Row(1.0, 1.0, 0.0)]
        [Row(10.0, 10.0, 9.8951925820621439264623017041980483215553841533709)]
        [Row(10.0, 1.0, 0.98951925820621439264623017041980483215553841533709)]
        public void ValidateMode(double shape, double scale, double mode)
        {
            var n = new Weibull(shape, scale);
            AssertEx.AreEqual<double>(mode, n.Mode);
        }

        [Test]
        [Row(1.0, 0.1, 0.069314718055994530941723212145817656807550013436026)]
        [Row(1.0, 1.0, 0.69314718055994530941723212145817656807550013436026)]
        [Row(10.0, 10.0, 9.6401223546778973665856033763604752124634905617583)]
        [Row(10.0, 1.0, 0.96401223546778973665856033763604752124634905617583)]
        public void ValidateMedian(double shape, double scale, double median)
        {
            var n = new Weibull(shape, scale);
            AssertHelpers.AlmostEqual(median, n.Median, 13);
        }

        [Test]
        public void ValidateMinimum()
        {
            var n = new Weibull(1.0,1.0);
            AssertEx.AreEqual<double>(0.0, n.Minimum);
        }

        [Test]
        public void ValidateMaximum()
        {
            var n = new Weibull(1.0, 1.0);
            AssertEx.AreEqual<double>(System.Double.PositiveInfinity, n.Maximum);
        }
        
        [Test]
        [Row(1.0, 0.1, 0.0, 10.0)]
        [Row(1.0, 0.1, 1.0, 0.00045399929762484851535591515560550610237918088866565)]
        [Row(1.0, 0.1, 10.0, 3.7200759760208359629596958038631183373588922923768e-43)]
        [Row(1.0, 1.0, 0.0, 1.0)]
        [Row(1.0, 1.0, 1.0, 0.36787944117144232159552377016146086744581113103177)]
        [Row(1.0, 1.0, 10.0, 0.000045399929762484851535591515560550610237918088866565)]
        [Row(10.0, 10.0, 0.0, 0.0)]
        [Row(10.0, 10.0, 1.0, 9.9999999990000000000499999999983333333333750000000e-10)]
        [Row(10.0, 10.0, 10.0, 0.36787944117144232159552377016146086744581113103177)]
        [Row(10.0, 1.0, 0.0, 0.0)]
        [Row(10.0, 1.0, 1.0, 3.6787944117144232159552377016146086744581113103177)]
        [Row(10.0, 1.0, 10.0, 0.0)]
        public void ValidateDensity(double shape, double scale, double x, double pdf)
        {
            var n = new Weibull(shape, scale);
            AssertHelpers.AlmostEqual(pdf, n.Density(x), 14);
        }
        
        [Test]
        [Row(1.0, 0.1, 0.0, 2.3025850929940456840179914546843642076011014886288)]
        [Row(1.0, 0.1, 1.0, -7.6974149070059543159820085453156357923988985113712)]
        [Row(1.0, 0.1, 10.0, -97.697414907005954315982008545315635792398898511371)]
        [Row(1.0, 1.0, 0.0, 0.0)]
        [Row(1.0, 1.0, 1.0, -1.0)]
        [Row(1.0, 1.0, 10.0, -10.0)]
        [Row(10.0, 10.0, 0.0, Double.NegativeInfinity)]
        [Row(10.0, 10.0, 1.0, -20.723265837046411156161923092159277868409913397659)]
        [Row(10.0, 10.0, 10.0, -1.0)]
        [Row(10.0, 1.0, 0.0, Double.NegativeInfinity)]
        [Row(10.0, 1.0, 1.0, 1.3025850929940456840179914546843642076011014886288)]
        [Row(10.0, 1.0, 10.0, -9.999999976974149070059543159820085453156357923988985113712e9)]
        public void ValidateDensityLn(double shape, double scale, double x, double pdfln)
        {
            var n = new Weibull(shape, scale);
            AssertHelpers.AlmostEqual(pdfln, n.DensityLn(x), 14);
        }

        [Test]
        public void CanSampleStatic()
        {
            var d = Weibull.Sample(new Random(), 1.0, 1.0);
        }

        [Test]
        public void CanSampleSequenceStatic()
        {
            var ied = Weibull.Samples(new Random(), 1.0, 1.0);
            var arr = ied.Take(5).ToArray();
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void FailSampleStatic()
        {
            var d = Normal.Sample(new Random(), 1.0, -1.0);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void FailSampleSequenceStatic()
        {
            var ied = Normal.Samples(new Random(), 1.0, -1.0).First();
        }

        [Test]
        public void CanSample()
        {
            var n = new Normal();
            var d = n.Sample();
        }

        [Test]
        public void CanSampleSequence()
        {
            var n = new Normal();
            var ied = n.Samples();
            var e = ied.Take(5).ToArray();
        }
        
        [Test]
        [Row(1.0, 0.1, 0.0, 0.0)]
        [Row(1.0, 0.1, 1.0, 0.99995460007023751514846440848443944938976208191113)]
        [Row(1.0, 0.1, 10.0, 0.99999999999999999999999999999999999999999996279924)]
        [Row(1.0, 1.0, 0.0, 0.0)]
        [Row(1.0, 1.0, 1.0, 0.63212055882855767840447622983853913255418886896823)]
        [Row(1.0, 1.0, 10.0, 0.99995460007023751514846440848443944938976208191113)]
        [Row(10.0, 10.0, 0.0, 0.0)]
        [Row(10.0, 10.0, 1.0, 9.9999999995000000000166666666662500000000083333333e-11)]
        [Row(10.0, 10.0, 10.0, 0.63212055882855767840447622983853913255418886896823)]
        [Row(10.0, 1.0, 0.0, 0.0)]
        [Row(10.0, 1.0, 1.0, 0.63212055882855767840447622983853913255418886896823)]
        [Row(10.0, 1.0, 10.0, 1.0)]
        public void ValidateCumulativeDistribution(double shape, double scale, double x, double cdf)
        {
            var n = new Weibull(shape, scale);
            AssertHelpers.AlmostEqual(cdf, n.CumulativeDistribution(x), 15);
        }
    }
}
