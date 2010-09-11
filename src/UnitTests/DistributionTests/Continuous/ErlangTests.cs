// <copyright file="ErlangTests.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
//
// Copyright (c) 2009-2010 Math.NET
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

namespace MathNet.Numerics.UnitTests.DistributionTests.Continuous
{
    using System;
    using System.Linq;
    using MbUnit.Framework;
    using Distributions;

    [TestFixture]
    public class ErlangTests
    {
        [SetUp]
        public void SetUp()
        {
            Control.CheckDistributionParameters = true;
        }

        [Test, MultipleAsserts]
        [Row(0, 0.0)]
        [Row(1, 0.1)]
        [Row(1, 1.0)]
        [Row(10, 10.0)]
        [Row(10, 1.0)]
        [Row(10, Double.PositiveInfinity)]
        public void CanCreateErlang(int shape, double invScale)
        {
            var n = new Erlang(shape, invScale);
            Assert.AreEqual<double>(shape, n.Shape);
            Assert.AreEqual<double>(invScale, n.InvScale);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [Row(1, Double.NaN)]
        [Row(1, -1.0)]
        [Row(-1, 1.0)]
        [Row(-1, -1.0)]
        [Row(-1, Double.NaN)]
        public void ErlangCreateFailsWithBadParameters(int shape, double invScale)
        {
            var n = new Erlang(shape, invScale);
        }

        [Test, MultipleAsserts]
        [Row(0, 0.0)]
        [Row(1, 0.1)]
        [Row(1, 1.0)]
        [Row(10, 10.0)]
        [Row(10, 1.0)]
        [Row(10, Double.PositiveInfinity)]
        public void CanCreateErlangWithShapeInvScale(int shape, double invScale)
        {
            var n = Erlang.WithShapeInvScale(shape, invScale);
            Assert.AreEqual<double>(shape, n.Shape);
            Assert.AreEqual<double>(invScale, n.InvScale);
        }

        [Test, MultipleAsserts]
        [Row(0, 0.0)]
        [Row(1, 0.1)]
        [Row(1, 1.0)]
        [Row(10, 10.0)]
        [Row(10, 1.0)]
        [Row(10, Double.PositiveInfinity)]
        public void CanCreateGammaWithShapeScale(int shape, double scale)
        {
            var n = Erlang.WithShapeScale(shape, scale);
            Assert.AreEqual<double>(shape, n.Shape);
            Assert.AreEqual<double>(scale, n.Scale);
        }

        [Test]
        public void ValidateToString()
        {
            var n = new Erlang(1, 2.0);
            Assert.AreEqual<string>("Erlang(Shape = 1, Inverse Scale = 2)", n.ToString());
        }

        [Test]
        [Row(-0)]
        [Row(0)]
        [Row(1)]
        [Row(10)]
        public void CanSetShape(int shape)
        {
            var n = new Erlang(1, 1.0);
            n.Shape = shape;
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void SetShapeFailsWithNegativeShape()
        {
            var n = new Erlang(1, 1.0);
            n.Shape = -1;
        }

        [Test]
        [Row(-0.0)]
        [Row(0.0)]
        [Row(0.1)]
        [Row(1.0)]
        [Row(10.0)]
        [Row(Double.PositiveInfinity)]
        public void CanSetScale(double scale)
        {
            var n = new Erlang(1, 1.0);
            n.Scale = scale;
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void SetScaleFailsWithNegativeScale()
        {
            var n = new Erlang(1, 1.0);
            n.Scale = -1.0;
        }

        [Test]
        [Row(-0.0)]
        [Row(0.0)]
        [Row(0.1)]
        [Row(1.0)]
        [Row(10.0)]
        [Row(Double.PositiveInfinity)]
        public void CanSetInvScale(double invScale)
        {
            var n = new Erlang(1, 1.0);
            n.InvScale = invScale;
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void SetInvScaleFailsWithNegativeInvScale()
        {
            var n = new Erlang(1, 1.0);
            n.InvScale = -1.0;
        }

        [Test]
        [Row(0, 0.0, Double.NaN)]
        [Row(1, 0.1, 10.0)]
        [Row(1, 1.0, 1.0)]
        [Row(10, 10.0, 1.0)]
        [Row(10, 1.0, 10.0)]
        [Row(10, Double.PositiveInfinity, 10.0)]
        public void ValidateMean(int shape, double invScale, double mean)
        {
            var n = new Erlang(shape, invScale);
            Assert.AreEqual<double>(mean, n.Mean);
        }

        [Test]
        [Row(0, 0.0, Double.NaN)]
        [Row(1, 0.1, 100.0)]
        [Row(1, 1.0, 1.0)]
        [Row(10, 10.0, 0.1)]
        [Row(10, 1.0, 10.0)]
        [Row(10, Double.PositiveInfinity, 0.0)]
        public void ValidateVariance(int shape, double invScale, double var)
        {
            var n = new Erlang(shape, invScale);
            AssertHelpers.AlmostEqual(var, n.Variance, 15);
        }

        [Test]
        [Row(0, 0.0, Double.NaN)]
        [Row(1, 0.1, 10.0)]
        [Row(1, 1.0, 1.0)]
        [Row(10, 10.0, 0.31622776601683794197697302588502426416723164097476643)]
        [Row(10, 1.0, 3.1622776601683793319988935444327185337195551393252168)]
        [Row(10, Double.PositiveInfinity, 0.0)]
        public void ValidateStdDev(int shape, double invScale, double sdev)
        {
            var n = new Erlang(shape, invScale);
            AssertHelpers.AlmostEqual(sdev, n.StdDev, 15);
        }

        [Test]
        [Row(0, 0.0, Double.NaN)]
        [Row(1, 0.1, 3.3025850929940456285068402234265387271634735938763824)]
        [Row(1, 1.0, 1.0)]
        [Row(10, 10.0, 0.23346908548693395836262094490967812177376750477943892)]
        [Row(10, 1.0, 2.5360541784809796423806123995940423293748689934081866)]
        [Row(10, Double.PositiveInfinity, 0.0)]
        public void ValidateEntropy(int shape, double invScale, double entropy)
        {
            var n = new Erlang(shape, invScale);
            AssertHelpers.AlmostEqual(entropy, n.Entropy, 13);
        }

        [Test]
        [Row(0, 0.0, Double.NaN)]
        [Row(1, 0.1, 2.0)]
        [Row(1, 1.0, 2.0)]
        [Row(10, 10.0, 0.63245553203367586639977870888654370674391102786504337)]
        [Row(10, 1.0, 0.63245553203367586639977870888654370674391102786504337)]
        [Row(10, Double.PositiveInfinity, 0.0)]
        public void ValidateSkewness(int shape, double invScale, double skewness)
        {
            var n = new Erlang(shape, invScale);
            AssertHelpers.AlmostEqual(skewness, n.Skewness, 15);
        }

        [Test]
        [Row(1, 0.1, 0.0)]
        [Row(1, 1.0, 0.0)]
        [Row(10, 10.0, 0.9)]
        [Row(10, 1.0, 9.0)]
        [Row(10, Double.PositiveInfinity, 10.0)]
        public void ValidateMode(int shape, double invScale, double mode)
        {
            var n = new Erlang(shape, invScale);
            Assert.AreEqual<double>(mode, n.Mode);
        }

        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        [Row(0, 0.0)]
        [Row(1, 0.1)]
        [Row(1, 1.0)]
        [Row(10, 10.0)]
        [Row(10, 1.0)]
        [Row(10, Double.PositiveInfinity)]
        public void ValidateMedian(int shape, double invScale)
        {
            var n = new Erlang(shape, invScale);
            var median = n.Median;
        }

        [Test]
        public void ValidateMinimum()
        {
            var n = new Erlang(1, 1.0);
            Assert.AreEqual<double>(0.0, n.Minimum);
        }

        [Test]
        public void ValidateMaximum()
        {
            var n = new Erlang(1, 1.0);
            Assert.AreEqual<double>(System.Double.PositiveInfinity, n.Maximum);
        }

        [Test]
        [Row(0, 0.0, 0.0, 0.0)]
        [Row(0, 0.0, 1.0, 0.0)]
        [Row(0, 0.0, 10.0, 0.0)]
        [Row(1, 0.1, 0.0, 0.10000000000000000555111512312578270211815834045410156)]
        [Row(1, 0.1, 1.0, 0.090483741803595961836995913651194571475319347018875963)]
        [Row(1, 0.1, 10.0, 0.036787944117144234201693506390001264039984687455876246)]
        [Row(1, 1.0, 0.0, 1.0)]
        [Row(1, 1.0, 1.0, 0.36787944117144232159552377016146086744581113103176804)]
        [Row(1, 1.0, 10.0, 0.000045399929762484851535591515560550610237918088866564953)]
        [Row(10, 10.0, 0.0, 0.0)]
        [Row(10, 10.0, 1.0, 1.2511003572113329898476497894772544708420990097708588)]
        [Row(10, 10.0, 10.0, 1.0251532120868705806216092933926141802686541811003037e-30)]
        [Row(10, 1.0, 0.0, 0.0)]
        [Row(10, 1.0, 1.0, 0.0000010137771196302974029859010421116095333052555418644397)]
        [Row(10, 1.0, 10.0, 0.12511003572113329898476497894772544708420990097708601)]
        [Row(10, Double.PositiveInfinity, 0.0, 0.0)]
        [Row(10, Double.PositiveInfinity, 1.0, 0.0)]
        [Row(10, Double.PositiveInfinity, 10.0, Double.PositiveInfinity)]
        public void ValidateDensity(int shape, double invScale, double x, double pdf)
        {
            var n = new Erlang(shape, invScale);
            AssertHelpers.AlmostEqual(pdf, n.Density(x), 14);
        }

        [Test]
        [Row(0, 0.0, 0.0, Double.NegativeInfinity)]
        [Row(0, 0.0, 1.0, Double.NegativeInfinity)]
        [Row(0, 0.0, 10.0, Double.NegativeInfinity)]
        [Row(1, 0.1, 0.0, -2.3025850929940456285068402234265387271634735938763824)]
        [Row(1, 0.1, 1.0, -2.402585092994045634057955346552321429281631934330484)]
        [Row(1, 0.1, 10.0, -3.3025850929940456285068402234265387271634735938763824)]
        [Row(1, 1.0, 0.0, 0.0)]
        [Row(1, 1.0, 1.0, -1.0)]
        [Row(1, 1.0, 10.0, -10.0)]
        [Row(10, 10.0, 0.0, Double.NegativeInfinity)]
        [Row(10, 10.0, 1.0, 0.22402344985898722897219667227693591172986563062456522)]
        [Row(10, 10.0, 10.0, -69.052710713194601614865880235563786219860220971716511)]
        [Row(10, 1.0, 0.0, Double.NegativeInfinity)]
        [Row(10, 1.0, 1.0, -13.801827480081469611207717874566706164281149255663166)]
        [Row(10, 1.0, 10.0, -2.0785616431350584550457947824074282958712358580042068)]
        [Row(10, Double.PositiveInfinity, 0.0, Double.NegativeInfinity)]
        [Row(10, Double.PositiveInfinity, 1.0, Double.NegativeInfinity)]
        [Row(10, Double.PositiveInfinity, 10.0, Double.PositiveInfinity)]
        public void ValidateDensityLn(int shape, double invScale, double x, double pdfln)
        {
            var n = new Erlang(shape, invScale);
            AssertHelpers.AlmostEqual(pdfln, n.DensityLn(x), 14);
        }

        [Test]
        public void CanSample()
        {
            var n = new Erlang(1, 2.0);
            var d = n.Sample();
        }

        [Test]
        public void CanSampleSequence()
        {
            var n = new Erlang(1, 2.0);
            var ied = n.Samples();
            var e = ied.Take(5).ToArray();
        }

        [Test]
        [Row(0, 0.0, 0.0, 0.0)]
        [Row(0, 0.0, 1.0, 0.0)]
        [Row(0, 0.0, 10.0, 0.0)]
        [Row(1, 0.1, 0.0, 0.0)]
        [Row(1, 0.1, 1.0, 0.095162581964040431858607615783064404690935346242622848)]
        [Row(1, 0.1, 10.0, 0.63212055882855767840447622983853913255418886896823196)]
        [Row(1, 1.0, 0.0, 0.0)]
        [Row(1, 1.0, 1.0, 0.63212055882855767840447622983853913255418886896823196)]
        [Row(1, 1.0, 10.0, 0.99995460007023751514846440848443944938976208191113396)]
        [Row(10, 10.0, 0.0, 0.0)]
        [Row(10, 10.0, 1.0, 0.54207028552814779168583514294066541824736464003242184)]
        [Row(10, 10.0, 10.0, 0.99999999999999999999999999999988746526039157266114706)]
        [Row(10, 1.0, 0.0, 0.0)]
        [Row(10, 1.0, 1.0, 0.00000011142547833872067735305068724025236288094949815466035)]
        [Row(10, 1.0, 10.0, 0.54207028552814779168583514294066541824736464003242184)]
        [Row(10, Double.PositiveInfinity, 0.0, 0.0)]
        [Row(10, Double.PositiveInfinity, 1.0, 0.0)]
        [Row(10, Double.PositiveInfinity, 10.0, 1.0)]
        public void ValidateCumulativeDistribution(int shape, double invScale, double x, double cdf)
        {
            var n = new Erlang(shape, invScale);
            AssertHelpers.AlmostEqual(cdf, n.CumulativeDistribution(x), 14);
        }
    }
}
