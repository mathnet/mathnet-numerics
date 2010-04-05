// <copyright file="NormalGammaTests.cs" company="Math.NET">
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
    public class NormalGammaTests
    {
        [Test, MultipleAsserts]
        [Row(0.0, 1.0, 1.0, 1.0)]
        [Row(10.0, 1.0, 2.0, 2.0)]
        public void CanCreateNormalGamma(double meanLocation, double meanScale, double precShape, double precInvScale)
        {
            NormalGamma ng = new NormalGamma(meanLocation, meanScale, precShape, precInvScale);

            AssertEx.AreEqual<double>(meanLocation, ng.MeanLocation);
            AssertEx.AreEqual<double>(meanScale, ng.MeanScale);
            AssertEx.AreEqual<double>(precShape, ng.PrecisionShape);
            AssertEx.AreEqual<double>(precInvScale, ng.PrecisionInverseScale);
        }

        [Test]
        [Row(1.0, -1.3, 2.0, 2.0)]
        [Row(1.0, 1.0, -1.0, 1.0)]
        [Row(1.0, 1.0, 1.0, -1.0)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void NormalGammaConstructorFailsWithInvalidParams(double meanLocation, double meanScale, double precShape, double precInvScale)
        {
            var nb = new NormalGamma(meanLocation, meanScale, precShape, precInvScale);
        }

        [Test]
        [Row(0.0, 1.0, 1.0, 1.0)]
        [Row(10.0, 1.0, 2.0, 2.0)]
        public void CanGetMeanLocation(double meanLocation, double meanScale, double precShape, double precInvScale)
        {
            NormalGamma ng = new NormalGamma(meanLocation, meanScale, precShape, precInvScale);
            AssertEx.AreEqual<double>(meanLocation, ng.MeanLocation);
        }

        [Test, MultipleAsserts]
        [Row(0.0, 1.0, 1.0, 1.0)]
        [Row(10.0, 1.0, 2.0, 2.0)]
        public void CanSetMeanLocation(double meanLocation, double meanScale, double precShape, double precInvScale)
        {
            NormalGamma ng = new NormalGamma(meanLocation, meanScale, precShape, precInvScale);
            ng.MeanLocation = -5.0;

            AssertEx.AreEqual<double>(-5.0, ng.MeanLocation);
            AssertEx.AreEqual<double>(meanScale, ng.MeanScale);
            AssertEx.AreEqual<double>(precShape, ng.PrecisionShape);
            AssertEx.AreEqual<double>(precInvScale, ng.PrecisionInverseScale);
        }

        [Test]
        [Row(0.0, 1.0, 1.0, 1.0)]
        [Row(10.0, 1.0, 2.0, 2.0)]
        public void CanGetMeanScale(double meanLocation, double meanScale, double precShape, double precInvScale)
        {
            NormalGamma ng = new NormalGamma(meanLocation, meanScale, precShape, precInvScale);
            AssertEx.AreEqual<double>(meanScale, ng.MeanScale);
        }

        [Test, MultipleAsserts]
        [Row(0.0, 1.0, 1.0, 1.0)]
        [Row(10.0, 1.0, 2.0, 2.0)]
        public void CanSetMeanScale(double meanLocation, double meanScale, double precShape, double precInvScale)
        {
            NormalGamma ng = new NormalGamma(meanLocation, meanScale, precShape, precInvScale);
            ng.MeanScale = 5.0;
            AssertEx.AreEqual<double>(meanLocation, ng.MeanLocation);
            AssertEx.AreEqual<double>(5.0, ng.MeanScale);
            AssertEx.AreEqual<double>(precShape, ng.PrecisionShape);
            AssertEx.AreEqual<double>(precInvScale, ng.PrecisionInverseScale);
        }

        [Test]
        [Row(0.0, 1.0, 1.0, 1.0)]
        [Row(10.0, 1.0, 2.0, 2.0)]
        public void CanGetPrecisionShape(double meanLocation, double meanScale, double precShape, double precInvScale)
        {
            NormalGamma ng = new NormalGamma(meanLocation, meanScale, precShape, precInvScale);
            AssertEx.AreEqual<double>(precShape, ng.PrecisionShape);
        }

        [Test, MultipleAsserts]
        [Row(0.0, 1.0, 1.0, 1.0)]
        [Row(10.0, 1.0, 2.0, 2.0)]
        public void CanSetPrecisionShape(double meanLocation, double meanScale, double precShape, double precInvScale)
        {
            NormalGamma ng = new NormalGamma(meanLocation, meanScale, precShape, precInvScale);
            ng.PrecisionShape = 5.0;
            AssertEx.AreEqual<double>(meanLocation, ng.MeanLocation);
            AssertEx.AreEqual<double>(meanScale, ng.MeanScale);
            AssertEx.AreEqual<double>(5.0, ng.PrecisionShape);
            AssertEx.AreEqual<double>(precInvScale, ng.PrecisionInverseScale);
        }

        [Test]
        [Row(0.0, 1.0, 1.0, 1.0)]
        [Row(10.0, 1.0, 2.0, 2.0)]
        public void CanGetPrecisionInverseScale(double meanLocation, double meanScale, double precShape, double precInvScale)
        {
            NormalGamma ng = new NormalGamma(meanLocation, meanScale, precShape, precInvScale);
            AssertEx.AreEqual<double>(precInvScale, ng.PrecisionInverseScale);
        }

        [Test, MultipleAsserts]
        [Row(0.0, 1.0, 1.0, 1.0)]
        [Row(10.0, 1.0, 2.0, 2.0)]
        public void CanSetPrecisionPrecisionInverseScale(double meanLocation, double meanScale, double precShape, double precInvScale)
        {
            NormalGamma ng = new NormalGamma(meanLocation, meanScale, precShape, precInvScale);
            ng.PrecisionInverseScale = 5.0;
            AssertEx.AreEqual<double>(meanLocation, ng.MeanLocation);
            AssertEx.AreEqual<double>(meanScale, ng.MeanScale);
            AssertEx.AreEqual<double>(precShape, ng.PrecisionShape);
            AssertEx.AreEqual<double>(5.0, ng.PrecisionInverseScale);
        }

        [Test, MultipleAsserts]
        [Row(0.0, 1.0, 1.0, 1.0, 0.0, 1.0, 2.0)]
        [Row(10.0, 1.0, 2.0, 2.0, 10.0, 1.0, 4.0)]
        [Row(10.0, 1.0, 2.0, Double.PositiveInfinity, 10.0, 2.0, Double.PositiveInfinity)]
        public void CanGetMeanMarginal(double meanLocation, double meanScale, double precShape, double precInvScale,
            double meanMarginalMean, double meanMarginalScale, double meanMarginalDoF)
        {
            NormalGamma ng = new NormalGamma(meanLocation, meanScale, precShape, precInvScale);
            var mm = ng.MeanMarginal();
            AssertEx.AreEqual<double>(meanMarginalMean, mm.Location);
            AssertEx.AreEqual<double>(meanMarginalScale, mm.Scale);
            AssertEx.AreEqual<double>(meanMarginalDoF, mm.DegreesOfFreedom);
        }

        [Test, MultipleAsserts]
        [Row(0.0, 1.0, 1.0, 1.0)]
        [Row(10.0, 1.0, 2.0, 2.0)]
        [Row(10.0, 1.0, 2.0, Double.PositiveInfinity)]
        public void CanGetPrecisionMarginal(double meanLocation, double meanScale, double precShape, double precInvScale)
        {
            NormalGamma ng = new NormalGamma(meanLocation, meanScale, precShape, precInvScale);
            var pm = ng.PrecisionMarginal();
            AssertEx.AreEqual<double>(precShape, pm.Shape);
            AssertEx.AreEqual<double>(precInvScale, pm.InvScale);
        }

        [Test, MultipleAsserts]
        [Row(0.0, 1.0, 1.0, 1.0, 0.0, 1.0)]
        [Row(10.0, 1.0, 2.0, 2.0, 10.0, 1.0)]
        [Row(10.0, 1.0, 2.0, Double.PositiveInfinity, 10.0, 2.0)]
        public void CanGetMean(double meanLocation, double meanScale, double precShape, double precInvScale,
            double meanMean, double meanPrecision)
        {
            NormalGamma ng = new NormalGamma(meanLocation, meanScale, precShape, precInvScale);
            AssertEx.AreEqual<double>(meanMean, ng.Mean.Mean);
            AssertEx.AreEqual<double>(meanPrecision, ng.Mean.Precision);
        }

        [Test]
        public void HasRandomSource()
        {
            NormalGamma ng = new NormalGamma(0.0, 1.0, 1.0, 1.0);
            Assert.IsNotNull(ng.RandomSource);
        }

        [Test]
        public void CanSetRandomSource()
        {
            NormalGamma ng = new NormalGamma(0.0, 1.0, 1.0, 1.0);
            ng.RandomSource = new Random();
        }
    }
}