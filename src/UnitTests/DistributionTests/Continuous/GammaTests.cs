// <copyright file="GammaTests.cs" company="Math.NET">
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

namespace MathNet.Numerics.UnitTests.DistributionTests.Continuous
{
    using System;
    using System.Linq;
    using Distributions;
    using NUnit.Framework;

    /// <summary>
    /// Gamma distribution tests.
    /// </summary>
    [TestFixture]
    public class GammaTests
    {
        /// <summary>
        /// Set-up parameters.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            Control.CheckDistributionParameters = true;
        }

        /// <summary>
        /// Can create gamma.
        /// </summary>
        /// <param name="shape">Shape value.</param>
        /// <param name="invScale">Inverse scale value.</param>
        [TestCase(0.0, 0.0)]
        [TestCase(1.0, 0.1)]
        [TestCase(1.0, 1.0)]
        [TestCase(10.0, 10.0)]
        [TestCase(10.0, 1.0)]
        [TestCase(10.0, Double.PositiveInfinity)]
        public void CanCreateGamma(double shape, double invScale)
        {
            var n = new Gamma(shape, invScale);
            Assert.AreEqual(shape, n.Shape);
            Assert.AreEqual(invScale, n.InvScale);
        }

        /// <summary>
        /// Gamma create fails with bad parameters.
        /// </summary>
        /// <param name="shape">Shape value.</param>
        /// <param name="invScale">Inverse scale value.</param>
        [TestCase(1.0, Double.NaN)]
        [TestCase(1.0, -1.0)]
        [TestCase(-1.0, 1.0)]
        [TestCase(-1.0, -1.0)]
        [TestCase(-1.0, Double.NaN)]
        public void GammaCreateFailsWithBadParameters(double shape, double invScale)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Gamma(shape, invScale));
        }

        /// <summary>
        /// Can create gamma with shape and inverse scale.
        /// </summary>
        /// <param name="shape">Shape value.</param>
        /// <param name="invScale">Inverse scale value.</param>
        [TestCase(0.0, 0.0)]
        [TestCase(1.0, 0.1)]
        [TestCase(1.0, 1.0)]
        [TestCase(10.0, 10.0)]
        [TestCase(10.0, 1.0)]
        [TestCase(10.0, Double.PositiveInfinity)]
        public void CanCreateGammaWithShapeInvScale(double shape, double invScale)
        {
            var n = Gamma.WithShapeInvScale(shape, invScale);
            Assert.AreEqual(shape, n.Shape);
            Assert.AreEqual(invScale, n.InvScale);
        }

        /// <summary>
        /// Can create gamma with shape and scale.
        /// </summary>
        /// <param name="shape">Shape value.</param>
        /// <param name="scale">Scale value.</param>
        [TestCase(0.0, 0.0)]
        [TestCase(1.0, 0.1)]
        [TestCase(1.0, 1.0)]
        [TestCase(10.0, 10.0)]
        [TestCase(10.0, 1.0)]
        [TestCase(10.0, Double.PositiveInfinity)]
        public void CanCreateGammaWithShapeScale(double shape, double scale)
        {
            var n = Gamma.WithShapeScale(shape, scale);
            Assert.AreEqual(shape, n.Shape);
            Assert.AreEqual(scale, n.Scale);
        }

        /// <summary>
        /// Validate ToString.
        /// </summary>
        [Test]
        public void ValidateToString()
        {
            var n = new Gamma(1.0, 2.0);
            Assert.AreEqual("Gamma(Shape = 1, Inverse Scale = 2)", n.ToString());
        }

        /// <summary>
        /// Can set shape.
        /// </summary>
        /// <param name="shape">Shape value.</param>
        [TestCase(-0.0)]
        [TestCase(0.0)]
        [TestCase(0.1)]
        [TestCase(1.0)]
        [TestCase(10.0)]
        [TestCase(Double.PositiveInfinity)]
        public void CanSetShape(double shape)
        {
            new Gamma(1.0, 1.0)
            {
                Shape = shape
            };
        }

        /// <summary>
        /// Set shape fails with negative shape.
        /// </summary>
        [Test]
        public void SetShapeFailsWithNegativeShape()
        {
            var n = new Gamma(1.0, 1.0);
            Assert.Throws<ArgumentOutOfRangeException>(() => n.Shape = -1.0);
        }

        /// <summary>
        /// Can set scale.
        /// </summary>
        /// <param name="scale">Scale value.</param>
        [TestCase(-0.0)]
        [TestCase(0.0)]
        [TestCase(0.1)]
        [TestCase(1.0)]
        [TestCase(10.0)]
        [TestCase(Double.PositiveInfinity)]
        public void CanSetScale(double scale)
        {
            new Gamma(1.0, 1.0)
            {
                Scale = scale
            };
        }

        /// <summary>
        /// Set scale fails with negative scale.
        /// </summary>
        [Test]
        public void SetScaleFailsWithNegativeScale()
        {
            var n = new Gamma(1.0, 1.0);
            Assert.Throws<ArgumentOutOfRangeException>(() => n.Scale = -1.0);
        }

        /// <summary>
        /// Can set inverse scale.
        /// </summary>
        /// <param name="invScale">Inverse scale value.</param>
        [TestCase(-0.0)]
        [TestCase(0.0)]
        [TestCase(0.1)]
        [TestCase(1.0)]
        [TestCase(10.0)]
        [TestCase(Double.PositiveInfinity)]
        public void CanSetInvScale(double invScale)
        {
            new Gamma(1.0, 1.0)
            {
                InvScale = invScale
            };
        }

        /// <summary>
        /// Set inverse scale fails with negative value.
        /// </summary>
        [Test]
        public void SetInvScaleFailsWithNegativeInvScale()
        {
            var n = new Gamma(1.0, 1.0);
            Assert.Throws<ArgumentOutOfRangeException>(() => n.InvScale = -1.0);
        }

        /// <summary>
        /// Validate mean.
        /// </summary>
        /// <param name="shape">Shape value.</param>
        /// <param name="invScale">Inverse scale value.</param>
        /// <param name="mean">Expected value.</param>
        [TestCase(0.0, 0.0, Double.NaN)]
        [TestCase(1.0, 0.1, 10.0)]
        [TestCase(1.0, 1.0, 1.0)]
        [TestCase(10.0, 10.0, 1.0)]
        [TestCase(10.0, 1.0, 10.0)]
        [TestCase(10.0, Double.PositiveInfinity, 10.0)]
        public void ValidateMean(double shape, double invScale, double mean)
        {
            var n = new Gamma(shape, invScale);
            Assert.AreEqual(mean, n.Mean);
        }

        /// <summary>
        /// Validate variance.
        /// </summary>
        /// <param name="shape">Shape value.</param>
        /// <param name="invScale">Inverse scale value.</param>
        /// <param name="var">Expected value.</param>
        [TestCase(0.0, 0.0, Double.NaN)]
        [TestCase(1.0, 0.1, 100.0)]
        [TestCase(1.0, 1.0, 1.0)]
        [TestCase(10.0, 10.0, 0.1)]
        [TestCase(10.0, 1.0, 10.0)]
        [TestCase(10.0, Double.PositiveInfinity, 0.0)]
        public void ValidateVariance(double shape, double invScale, double var)
        {
            var n = new Gamma(shape, invScale);
            AssertHelpers.AlmostEqual(var, n.Variance, 15);
        }

        /// <summary>
        /// Validate standard deviation.
        /// </summary>
        /// <param name="shape">Shape value.</param>
        /// <param name="invScale">Inverse scale value.</param>
        /// <param name="sdev">Expected value.</param>
        [TestCase(0.0, 0.0, Double.NaN)]
        [TestCase(1.0, 0.1, 10.0)]
        [TestCase(1.0, 1.0, 1.0)]
        [TestCase(10.0, 10.0, 0.31622776601683794197697302588502426416723164097476643)]
        [TestCase(10.0, 1.0, 3.1622776601683793319988935444327185337195551393252168)]
        [TestCase(10.0, Double.PositiveInfinity, 0.0)]
        public void ValidateStdDev(double shape, double invScale, double sdev)
        {
            var n = new Gamma(shape, invScale);
            AssertHelpers.AlmostEqual(sdev, n.StdDev, 15);
        }

        /// <summary>
        /// Validate entropy.
        /// </summary>
        /// <param name="shape">Shape value.</param>
        /// <param name="invScale">Inverse scale value.</param>
        /// <param name="entropy">Expected value.</param>
        [TestCase(0.0, 0.0, Double.NaN)]
        [TestCase(1.0, 0.1, 3.3025850929940456285068402234265387271634735938763824)]
        [TestCase(1.0, 1.0, 1.0)]
        [TestCase(10.0, 10.0, 0.23346908548693395836262094490967812177376750477943892)]
        [TestCase(10.0, 1.0, 2.5360541784809796423806123995940423293748689934081866)]
        [TestCase(10.0, Double.PositiveInfinity, 0.0)]
        public void ValidateEntropy(double shape, double invScale, double entropy)
        {
            var n = new Gamma(shape, invScale);
            AssertHelpers.AlmostEqual(entropy, n.Entropy, 13);
        }

        /// <summary>
        /// Validate skewness.
        /// </summary>
        /// <param name="shape">Shape value.</param>
        /// <param name="invScale">Inverse scale value.</param>
        /// <param name="skewness">Expected value.</param>
        [TestCase(0.0, 0.0, Double.NaN)]
        [TestCase(1.0, 0.1, 2.0)]
        [TestCase(1.0, 1.0, 2.0)]
        [TestCase(10.0, 10.0, 0.63245553203367586639977870888654370674391102786504337)]
        [TestCase(10.0, 1.0, 0.63245553203367586639977870888654370674391102786504337)]
        [TestCase(10.0, Double.PositiveInfinity, 0.0)]
        public void ValidateSkewness(double shape, double invScale, double skewness)
        {
            var n = new Gamma(shape, invScale);
            AssertHelpers.AlmostEqual(skewness, n.Skewness, 15);
        }

        /// <summary>
        /// Validate mode.
        /// </summary>
        /// <param name="shape">Shape value.</param>
        /// <param name="invScale">Inverse scale value.</param>
        /// <param name="mode">Expected value.</param>
        [TestCase(0.0, 0.0, Double.NaN)]
        [TestCase(1.0, 0.1, 0.0)]
        [TestCase(1.0, 1.0, 0.0)]
        [TestCase(10.0, 10.0, 0.9)]
        [TestCase(10.0, 1.0, 9.0)]
        [TestCase(10.0, Double.PositiveInfinity, 10.0)]
        public void ValidateMode(double shape, double invScale, double mode)
        {
            var n = new Gamma(shape, invScale);
            Assert.AreEqual(mode, n.Mode);
        }

        /// <summary>
        /// Validate median.
        /// </summary>
        [Test]
        public void ValidateMedian()
        {
            var n = new Gamma(0.0, 0.0);
            Assert.Throws<NotSupportedException>(() => { var median = n.Median; });
        }

        /// <summary>
        /// Validate minimum.
        /// </summary>
        [Test]
        public void ValidateMinimum()
        {
            var n = new Gamma(1.0, 1.0);
            Assert.AreEqual(0.0, n.Minimum);
        }

        /// <summary>
        /// Validate maximum.
        /// </summary>
        [Test]
        public void ValidateMaximum()
        {
            var n = new Gamma(1.0, 1.0);
            Assert.AreEqual(Double.PositiveInfinity, n.Maximum);
        }

        /// <summary>
        /// Validate density.
        /// </summary>
        /// <param name="shape">Shape value.</param>
        /// <param name="invScale">Inverse scale value.</param>
        /// <param name="x">Input X value.</param>
        /// <param name="pdf">Expected value.</param>
        [TestCase(0, 0.0, 0.0, 0.0)]
        [TestCase(0, 0.0, 1.0, 0.0)]
        [TestCase(0, 0.0, 10.0, 0.0)]
        [TestCase(1, 0.1, 0.0, 0.10000000000000000555111512312578270211815834045410156)]
        [TestCase(1, 0.1, 1.0, 0.090483741803595961836995913651194571475319347018875963)]
        [TestCase(1, 0.1, 10.0, 0.036787944117144234201693506390001264039984687455876246)]
        [TestCase(1, 1.0, 0.0, 1.0)]
        [TestCase(1, 1.0, 1.0, 0.36787944117144232159552377016146086744581113103176804)]
        [TestCase(1, 1.0, 10.0, 0.000045399929762484851535591515560550610237918088866564953)]
        [TestCase(10, 10.0, 0.0, 0.0)]
        [TestCase(10, 10.0, 1.0, 1.2511003572113329898476497894772544708420990097708588)]
        [TestCase(10, 10.0, 10.0, 1.0251532120868705806216092933926141802686541811003037e-30)]
        [TestCase(10, 1.0, 0.0, 0.0)]
        [TestCase(10, 1.0, 1.0, 0.0000010137771196302974029859010421116095333052555418644397)]
        [TestCase(10, 1.0, 10.0, 0.12511003572113329898476497894772544708420990097708601)]
        [TestCase(10, Double.PositiveInfinity, 0.0, 0.0)]
        [TestCase(10, Double.PositiveInfinity, 1.0, 0.0)]
        [TestCase(10, Double.PositiveInfinity, 10.0, Double.PositiveInfinity)]
        public void ValidateDensity(int shape, double invScale, double x, double pdf)
        {
            var n = new Gamma(shape, invScale);
            AssertHelpers.AlmostEqual(pdf, n.Density(x), 14);
        }

        /// <summary>
        /// Validate density log.
        /// </summary>
        /// <param name="shape">Shape value.</param>
        /// <param name="invScale">Inverse scale value.</param>
        /// <param name="x">Input X value.</param>
        /// <param name="pdfln">Expected value.</param>
        [TestCase(0, 0.0, 0.0, Double.NegativeInfinity)]
        [TestCase(0, 0.0, 1.0, Double.NegativeInfinity)]
        [TestCase(0, 0.0, 10.0, Double.NegativeInfinity)]
        [TestCase(1, 0.1, 0.0, -2.3025850929940456285068402234265387271634735938763824)]
        [TestCase(1, 0.1, 1.0, -2.402585092994045634057955346552321429281631934330484)]
        [TestCase(1, 0.1, 10.0, -3.3025850929940456285068402234265387271634735938763824)]
        [TestCase(1, 1.0, 0.0, 0.0)]
        [TestCase(1, 1.0, 1.0, -1.0)]
        [TestCase(1, 1.0, 10.0, -10.0)]
        [TestCase(10, 10.0, 0.0, Double.NegativeInfinity)]
        [TestCase(10, 10.0, 1.0, 0.22402344985898722897219667227693591172986563062456522)]
        [TestCase(10, 10.0, 10.0, -69.052710713194601614865880235563786219860220971716511)]
        [TestCase(10, 1.0, 0.0, Double.NegativeInfinity)]
        [TestCase(10, 1.0, 1.0, -13.801827480081469611207717874566706164281149255663166)]
        [TestCase(10, 1.0, 10.0, -2.0785616431350584550457947824074282958712358580042068)]
        [TestCase(10, Double.PositiveInfinity, 0.0, Double.NegativeInfinity)]
        [TestCase(10, Double.PositiveInfinity, 1.0, Double.NegativeInfinity)]
        [TestCase(10, Double.PositiveInfinity, 10.0, Double.PositiveInfinity)]
        public void ValidateDensityLn(int shape, double invScale, double x, double pdfln)
        {
            var n = new Gamma(shape, invScale);
            AssertHelpers.AlmostEqual(pdfln, n.DensityLn(x), 14);
        }

        /// <summary>
        /// Can sample static.
        /// </summary>
        [Test]
        public void CanSampleStatic()
        {
            Gamma.Sample(new Random(), 1.0, 1.0);
        }

        /// <summary>
        /// Can sample sequence static.
        /// </summary>
        [Test]
        public void CanSampleSequenceStatic()
        {
            var ied = Gamma.Samples(new Random(), 1.0, 1.0);
            ied.Take(5).ToArray();
        }

        /// <summary>
        /// Sample static fails with bad parameters.
        /// </summary>
        [Test]
        public void FailSampleStatic()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Normal.Sample(new Random(), 1.0, -1.0));
        }

        /// <summary>
        /// Sample sequence static fails with bad parameters.
        /// </summary>
        [Test]
        public void FailSampleSequenceStatic()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Normal.Samples(new Random(), 1.0, -1.0).First());
        }

        /// <summary>
        /// Can sample.
        /// </summary>
        [Test]
        public void CanSample()
        {
            var n = new Normal();
            n.Sample();
        }

        /// <summary>
        /// Can sample sequence.
        /// </summary>
        [Test]
        public void CanSampleSequence()
        {
            var n = new Normal();
            var ied = n.Samples();
            ied.Take(5).ToArray();
        }

        /// <summary>
        /// Validate cumulative distribution.
        /// </summary>
        /// <param name="shape">Shape value.</param>
        /// <param name="invScale">Inverse scale value.</param>
        /// <param name="x">Input X value.</param>
        /// <param name="cdf">Expected value.</param>
        [TestCase(0, 0.0, -1.0, 0.0)]
        [TestCase(0, 0.0, 0.0, 0.0)]
        [TestCase(0, 0.0, 1.0, 0.0)]
        [TestCase(0, 0.0, 10.0, 0.0)]
        [TestCase(1, 0.1, -1.0, 0.0)]
        [TestCase(1, 0.1, 0.0, 0.0)]
        [TestCase(1, 0.1, 1.0, 0.095162581964040431858607615783064404690935346242622848)]
        [TestCase(1, 0.1, 10.0, 0.63212055882855767840447622983853913255418886896823196)]
        [TestCase(1, 1.0, -1.0, 0.0)]
        [TestCase(1, 1.0, 0.0, 0.0)]
        [TestCase(1, 1.0, 1.0, 0.63212055882855767840447622983853913255418886896823196)]
        [TestCase(1, 1.0, 10.0, 0.99995460007023751514846440848443944938976208191113396)]
        [TestCase(10, 10.0, -1.0, 0.0)]
        [TestCase(10, 10.0, 0.0, 0.0)]
        [TestCase(10, 10.0, 1.0, 0.54207028552814779168583514294066541824736464003242184)]
        [TestCase(10, 10.0, 10.0, 0.99999999999999999999999999999988746526039157266114706)]
        [TestCase(10, 1.0, -1.0, 0.0)]
        [TestCase(10, 1.0, 0.0, 0.0)]
        [TestCase(10, 1.0, 1.0, 0.00000011142547833872067735305068724025236288094949815466035)]
        [TestCase(10, 1.0, 10.0, 0.54207028552814779168583514294066541824736464003242184)]
        [TestCase(10, Double.PositiveInfinity, -1.0, 0.0)]
        [TestCase(10, Double.PositiveInfinity, 0.0, 0.0)]
        [TestCase(10, Double.PositiveInfinity, 1.0, 0.0)]
        [TestCase(10, Double.PositiveInfinity, 10.0, 1.0)]
        public void ValidateCumulativeDistribution(int shape, double invScale, double x, double cdf)
        {
            var n = new Gamma(shape, invScale);
            AssertHelpers.AlmostEqual(cdf, n.CumulativeDistribution(x), 14);
        }
    }
}
