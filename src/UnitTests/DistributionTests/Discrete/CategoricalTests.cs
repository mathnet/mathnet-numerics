// <copyright file="CategoricalTests.cs" company="Math.NET">
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

namespace MathNet.Numerics.UnitTests.DistributionTests.Discrete
{
    using System;
    using Distributions;
    using NUnit.Framework;
    using Statistics;

    /// <summary>
    /// Categorical distribution tests.
    /// </summary>
    [TestFixture]
    public class CategoricalTests
    {
        /// <summary>
        /// Bad probability vector.
        /// </summary>
        private double[] _badP;

        /// <summary>
        /// Another bad probability vector.
        /// </summary>
        private double[] _badP2;

        /// <summary>
        /// Small probability vector.
        /// </summary>
        private double[] _smallP;

        /// <summary>
        /// Large probability vector.
        /// </summary>
        private double[] _largeP;

        /// <summary>
        /// Set-up test parameters.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            Control.CheckDistributionParameters = true;
            _badP = new[] { -1.0, 1.0 };
            _badP2 = new[] { 0.0, 0.0 };
            _smallP = new[] { 1.0, 1.0, 1.0 };
            _largeP = new[] { 1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0, 8.0, 9.0 };
        }

        /// <summary>
        /// Can create categorical.
        /// </summary>
        [Test]
        public void CanCreateCategorical()
        {
            new Categorical(_largeP);
        }

        /// <summary>
        /// Can create categorical from histogram.
        /// </summary>
        [Test]
        public void CanCreateCategoricalFromHistogram()
        {
            double[] smallDataset = { 0.5, 1.5, 2.5, 3.5, 4.5, 5.5, 6.5, 7.5, 8.5, 9.5 };
            var hist = new Histogram(smallDataset, 10, 0.0, 10.0);
            var m = new Categorical(hist);

            for (var i = 0; i <= m.Maximum; i++)
            {
                Assert.AreEqual(1.0 / 10.0, m.P[i]);
            }
        }

        /// <summary>
        /// Categorical create fails with <c>null</c> histogram.
        /// </summary>
        [Test]
        public void CategoricalCreateFailsWithNullHistogram()
        {
            Histogram h = null;
            Assert.Throws<ArgumentNullException>(() => new Categorical(h));
        }

        /// <summary>
        /// Categorical create fails with negative ratios.
        /// </summary>
        [Test]
        public void CategoricalCreateFailsWithNegativeRatios()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Categorical(_badP));
        }

        /// <summary>
        /// Categorical create fails with all zero ratios.
        /// </summary>
        [Test]
        public void CategoricalCreateFailsWithAllZeroRatios()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Categorical(_badP2));
        }

        /// <summary>
        /// Validate ToString.
        /// </summary>
        [Test]
        public void ValidateToString()
        {
            var b = new Categorical(_smallP);
            Assert.AreEqual("Categorical(Dimension = 3)", b.ToString());
        }

        /// <summary>
        /// Can set probability.
        /// </summary>
        [Test]
        public void CanSetProbability()
        {
            new Categorical(_largeP)
            {
                P = _smallP
            };
        }

        /// <summary>
        /// Set probability with a bad array fails.
        /// </summary>
        [Test]
        public void SetProbabilityFails()
        {
            var b = new Categorical(_largeP);
            Assert.Throws<ArgumentOutOfRangeException>(() => b.P = _badP);
        }

        /// <summary>
        /// Can sample static.
        /// </summary>
        [Test]
        public void CanSampleStatic()
        {
            Categorical.SampleWithProbabilityMass(new Random(), _largeP);
        }

        /// <summary>
        /// Sample static fails with a bad parameters.
        /// </summary>
        [Test]
        public void FailSampleStatic()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Categorical.SampleWithProbabilityMass(new Random(), _badP));
        }

        /// <summary>
        /// Can sample.
        /// </summary>
        [Test]
        public void CanSample()
        {
            var n = new Categorical(_largeP);
            n.Sample();
        }
    }
}
