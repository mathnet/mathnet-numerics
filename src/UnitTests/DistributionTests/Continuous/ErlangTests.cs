// <copyright file="ErlangTests.cs" company="Math.NET">
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
    /// Erlang distribution tests.
    /// </summary>
    [TestFixture]
    public class ErlangTests
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
        /// Can create Erlang.
        /// </summary>
        /// <param name="shape">Shape value.</param>
        /// <param name="invScale">Inverse scale value.</param>
        [Test, Sequential]
        public void CanCreateErlang([Values(0, 1, 1, 10, 10, 10)] int shape, [Values(0.0, 0.1, 1.0, 10.0, 1.0, Double.PositiveInfinity)] double invScale)
        {
            var n = new Erlang(shape, invScale);
            Assert.AreEqual(shape, n.Shape);
            Assert.AreEqual(invScale, n.InvScale);
        }

        /// <summary>
        /// Create Erlang fails with bad parameters.
        /// </summary>
        /// <param name="shape">Shape value.</param>
        /// <param name="invScale">Inverse scale value.</param>
        [Test, Sequential]
        public void ErlangCreateFailsWithBadParameters([Values(1, 1, -1, -1, -1)] int shape, [Values(Double.NaN, -1.0, 1.0, -1.0, Double.NaN)] double invScale)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Erlang(shape, invScale));
        }

        /// <summary>
        /// Can create Erlang with inverse scale.
        /// </summary>
        /// <param name="shape">Shape value.</param>
        /// <param name="invScale">Inverse scale value.</param>
        [Test, Sequential]
        public void CanCreateErlangWithShapeInvScale([Values(0, 1, 1, 10, 10, 10)] int shape, [Values(0.0, 0.1, 1.0, 10.0, 1.0, Double.PositiveInfinity)] double invScale)
        {
            var n = Erlang.WithShapeInvScale(shape, invScale);
            Assert.AreEqual(shape, n.Shape);
            Assert.AreEqual(invScale, n.InvScale);
        }

        /// <summary>
        /// Can create Erlang with shape and scale.
        /// </summary>
        /// <param name="shape">Shape value.</param>
        /// <param name="scale">Scale value.</param>
        [Test, Sequential]
        public void CanCreateErlangWithShapeScale([Values(0, 1, 1, 10, 10, 10)] int shape, [Values(0.0, 0.1, 1.0, 10.0, 1.0, Double.PositiveInfinity)] double scale)
        {
            var n = Erlang.WithShapeScale(shape, scale);
            Assert.AreEqual(shape, n.Shape);
            Assert.AreEqual(scale, n.Scale);
        }

        /// <summary>
        /// Validate ToString.
        /// </summary>
        [Test]
        public void ValidateToString()
        {
            var n = new Erlang(1, 2.0);
            Assert.AreEqual("Erlang(Shape = 1, Inverse Scale = 2)", n.ToString());
        }

        /// <summary>
        /// Can set shape.
        /// </summary>
        /// <param name="shape">New shape value.</param>
        [Test]
        public void CanSetShape([Values(-0, 0, 1, 10)] int shape)
        {
            new Erlang(1, 1.0)
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
            var n = new Erlang(1, 1.0);
            Assert.Throws<ArgumentOutOfRangeException>(() => n.Shape = -1);
        }

        /// <summary>
        /// Can set scale
        /// </summary>
        /// <param name="scale">New scale value.</param>
        [Test]
        public void CanSetScale([Values(-0.0, 0.0, 0.1, 1.0, 10.0, Double.PositiveInfinity)] double scale)
        {
            new Erlang(1, 1.0)
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
            var n = new Erlang(1, 1.0);
            Assert.Throws<ArgumentOutOfRangeException>(() => n.Scale = -1.0);
        }

        /// <summary>
        /// Can set inverse scale.
        /// </summary>
        /// <param name="invScale">Inverse scale value.</param>
        [Test]
        public void CanSetInvScale([Values(-0.0, 0.0, 0.1, 1.0, 10.0, Double.PositiveInfinity)] double invScale)
        {
            new Erlang(1, 1.0)
            {
                InvScale = invScale
            };
        }

        /// <summary>
        /// Set inverse scale fails with negative inverse scale.
        /// </summary>
        [Test]
        public void SetInvScaleFailsWithNegativeInvScale()
        {
            var n = new Erlang(1, 1.0);
            Assert.Throws<ArgumentOutOfRangeException>(() => n.InvScale = -1.0);
        }

        /// <summary>
        /// Validate mean.
        /// </summary>
        /// <param name="shape">Shape value.</param>
        /// <param name="invScale">Inverse scale value.</param>
        /// <param name="mean">Expected mean.</param>
        [Test, Sequential]
        public void ValidateMean([Values(0, 1, 1, 10, 10, 10)] int shape, [Values(0.0, 0.1, 1.0, 10.0, 1.0, Double.PositiveInfinity)] double invScale, [Values(Double.NaN, 10.0, 1.0, 1.0, 10.0, 10.0)] double mean)
        {
            var n = new Erlang(shape, invScale);
            Assert.AreEqual(mean, n.Mean);
        }

        /// <summary>
        /// Validate variance.
        /// </summary>
        /// <param name="shape">Shape value.</param>
        /// <param name="invScale">Inverse scale value.</param>
        /// <param name="var">Expected variance.</param>
        [Test, Sequential]
        public void ValidateVariance([Values(0, 1, 1, 10, 10, 10)] int shape, [Values(0.0, 0.1, 1.0, 10.0, 1.0, Double.PositiveInfinity)] double invScale, [Values(Double.NaN, 100.0, 1.0, 0.1, 10.0, 0.0)] double var)
        {
            var n = new Erlang(shape, invScale);
            AssertHelpers.AlmostEqual(var, n.Variance, 15);
        }

        /// <summary>
        /// Validate standard deviation.
        /// </summary>
        /// <param name="shape">Shape value.</param>
        /// <param name="invScale">Inverse scale value.</param>
        /// <param name="sdev">Expected value.</param>
        [Test, Sequential]
        public void ValidateStdDev([Values(0, 1, 1, 10, 10, 10)] int shape, [Values(0.0, 0.1, 1.0, 10.0, 1.0, Double.PositiveInfinity)] double invScale, [Values(Double.NaN, 10.0, 1.0, 0.31622776601683794197697302588502426416723164097476643, 3.1622776601683793319988935444327185337195551393252168, 0.0)] double sdev)
        {
            var n = new Erlang(shape, invScale);
            AssertHelpers.AlmostEqual(sdev, n.StdDev, 15);
        }

        /// <summary>
        /// Validate entropy.
        /// </summary>
        /// <param name="shape">Shape value.</param>
        /// <param name="invScale">Inverse scale value.</param>
        /// <param name="entropy">Expected value.</param>
        [Test, Sequential]
        public void ValidateEntropy([Values(0, 1, 1, 10, 10, 10)] int shape, [Values(0.0, 0.1, 1.0, 10.0, 1.0, Double.PositiveInfinity)] double invScale, [Values(Double.NaN, 3.3025850929940456285068402234265387271634735938763824, 1.0, 0.23346908548693395836262094490967812177376750477943892, 2.5360541784809796423806123995940423293748689934081866, 0.0)] double entropy)
        {
            var n = new Erlang(shape, invScale);
            AssertHelpers.AlmostEqual(entropy, n.Entropy, 13);
        }

        /// <summary>
        /// Validate skewness
        /// </summary>
        /// <param name="shape">Shape value.</param>
        /// <param name="invScale">Inverse scale value.</param>
        /// <param name="skewness">Expected value.</param>
        [Test, Sequential]
        public void ValidateSkewness([Values(0, 1, 1, 10, 10, 10)] int shape, [Values(0.0, 0.1, 1.0, 10.0, 1.0, Double.PositiveInfinity)] double invScale, [Values(Double.NaN, 2.0, 2.0, 0.63245553203367586639977870888654370674391102786504337, 0.63245553203367586639977870888654370674391102786504337, 0.0)] double skewness)
        {
            var n = new Erlang(shape, invScale);
            AssertHelpers.AlmostEqual(skewness, n.Skewness, 15);
        }

        /// <summary>
        /// Validate mode.
        /// </summary>
        /// <param name="shape">Shape value.</param>
        /// <param name="invScale">Inverse scale value.</param>
        /// <param name="mode">Expected value.</param>
        [Test, Sequential]
        public void ValidateMode([Values(1, 1, 10, 10, 10)] int shape, [Values(0.1, 1.0, 10.0, 1.0, Double.PositiveInfinity)] double invScale, [Values(0.0, 0.0, 0.9, 9.0, 10.0)] double mode)
        {
            var n = new Erlang(shape, invScale);
            Assert.AreEqual(mode, n.Mode);
        }

        /// <summary>
        /// Validate median throws <c>NotSupportedException</c>.
        /// </summary>
        [Test]
        public void ValidateMedianThrowsNotSupportedException()
        {
            var n = new Erlang(1, 1.0);
            Assert.Throws<NotSupportedException>(() => { var median = n.Median; });
        }

        /// <summary>
        /// Validate minimum.
        /// </summary>
        [Test]
        public void ValidateMinimum()
        {
            var n = new Erlang(1, 1.0);
            Assert.AreEqual(0.0, n.Minimum);
        }

        /// <summary>
        /// Validate maximum.
        /// </summary>
        [Test]
        public void ValidateMaximum()
        {
            var n = new Erlang(1, 1.0);
            Assert.AreEqual(Double.PositiveInfinity, n.Maximum);
        }

        /// <summary>
        /// Validate density.
        /// </summary>
        /// <param name="shape">Shape value.</param>
        /// <param name="invScale">Inverse scale value.</param>
        /// <param name="x">Input X value.</param>
        /// <param name="pdf">Expected value.</param>
        [Test, Sequential]
        public void ValidateDensity(
            [Values(0, 0, 0, 1, 1, 1, 1, 1, 1, 10, 10, 10, 10, 10, 10, 10, 10, 10)] int shape, 
            [Values(0.0, 0.0, 0.0, 0.1, 0.1, 0.1, 1.0, 1.0, 1.0, 10.0, 10.0, 10.0, 1.0, 1.0, 1.0, Double.PositiveInfinity, Double.PositiveInfinity, Double.PositiveInfinity)] double invScale, 
            [Values(0.0, 1.0, 10.0, 0.0, 1.0, 10.0, 0.0, 1.0, 10.0, 0.0, 1.0, 10.0, 0.0, 1.0, 10.0, 0.0, 1.0, 10.0)] double x, 
            [Values(0.0, 0.0, 0.0, 0.10000000000000000555111512312578270211815834045410156, 0.090483741803595961836995913651194571475319347018875963, 0.036787944117144234201693506390001264039984687455876246, 1.0, 0.36787944117144232159552377016146086744581113103176804, 0.000045399929762484851535591515560550610237918088866564953, 0.0, 1.2511003572113329898476497894772544708420990097708588, 1.0251532120868705806216092933926141802686541811003037e-30, 0.0, 0.0000010137771196302974029859010421116095333052555418644397, 0.12511003572113329898476497894772544708420990097708601, 0.0, 0.0, Double.PositiveInfinity)] double pdf)
        {
            var n = new Erlang(shape, invScale);
            AssertHelpers.AlmostEqual(pdf, n.Density(x), 14);
        }

        /// <summary>
        /// Validate density log.
        /// </summary>
        /// <param name="shape">Shape value.</param>
        /// <param name="invScale">Inverse scale value.</param>
        /// <param name="x">Input X value.</param>
        /// <param name="pdfln">Expected value.</param>
        [Test, Sequential]
        public void ValidateDensityLn(
            [Values(0, 0, 0, 1, 1, 1, 1, 1, 1, 10, 10, 10, 10, 10, 10, 10, 10, 10)] int shape, 
            [Values(0.0, 0.0, 0.0, 0.1, 0.1, 0.1, 1.0, 1.0, 1.0, 10.0, 10.0, 10.0, 1.0, 1.0, 1.0, Double.PositiveInfinity, Double.PositiveInfinity, Double.PositiveInfinity)] double invScale, 
            [Values(0.0, 1.0, 10.0, 0.0, 1.0, 10.0, 0.0, 1.0, 10.0, 0.0, 1.0, 10.0, 0.0, 1.0, 10.0, 0.0, 1.0, 10.0)] double x, 
            [Values(Double.NegativeInfinity, Double.NegativeInfinity, Double.NegativeInfinity, -2.3025850929940456285068402234265387271634735938763824, -2.402585092994045634057955346552321429281631934330484, -3.3025850929940456285068402234265387271634735938763824, 0.0, -1.0, -10.0, Double.NegativeInfinity, 0.22402344985898722897219667227693591172986563062456522, -69.052710713194601614865880235563786219860220971716511, Double.NegativeInfinity, -13.801827480081469611207717874566706164281149255663166, -2.0785616431350584550457947824074282958712358580042068, Double.NegativeInfinity, Double.NegativeInfinity, Double.PositiveInfinity)] double pdfln)
        {
            var n = new Erlang(shape, invScale);
            AssertHelpers.AlmostEqual(pdfln, n.DensityLn(x), 14);
        }

        /// <summary>
        /// Can sample.
        /// </summary>
        [Test]
        public void CanSample()
        {
            var n = new Erlang(1, 2.0);
            n.Sample();
        }

        /// <summary>
        /// Can sample sequence.
        /// </summary>
        [Test]
        public void CanSampleSequence()
        {
            var n = new Erlang(1, 2.0);
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
        [Test, Sequential]
        public void ValidateCumulativeDistribution(
            [Values(0, 0, 0, 1, 1, 1, 1, 1, 1, 10, 10, 10, 10, 10, 10, 10, 10, 10)] int shape, 
            [Values(0.0, 0.0, 0.0, 0.1, 0.1, 0.1, 1.0, 1.0, 1.0, 10.0, 10.0, 10.0, 1.0, 1.0, 1.0, Double.PositiveInfinity, Double.PositiveInfinity, Double.PositiveInfinity)] double invScale, 
            [Values(0.0, 1.0, 10.0, 0.0, 1.0, 10.0, 0.0, 1.0, 10.0, 0.0, 1.0, 10.0, 0.0, 1.0, 10.0, 0.0, 1.0, 10.0)] double x, 
            [Values(0.0, 0.0, 0.0, 0.0, 0.095162581964040431858607615783064404690935346242622848, 0.63212055882855767840447622983853913255418886896823196, 0.0, 0.63212055882855767840447622983853913255418886896823196, 0.99995460007023751514846440848443944938976208191113396, 0.0, 0.54207028552814779168583514294066541824736464003242184, 0.99999999999999999999999999999988746526039157266114706, 0.0, 0.00000011142547833872067735305068724025236288094949815466035, 0.54207028552814779168583514294066541824736464003242184, 0.0, 0.0, 1.0)] double cdf)
        {
            var n = new Erlang(shape, invScale);
            AssertHelpers.AlmostEqual(cdf, n.CumulativeDistribution(x), 14);
        }
    }
}
