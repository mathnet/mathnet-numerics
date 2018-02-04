// <copyright file="CategoricalTests.cs" company="Math.NET">
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

using System;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.Statistics;
using NUnit.Framework;

namespace MathNet.Numerics.UnitTests.DistributionTests.Discrete
{
    /// <summary>
    /// Categorical distribution tests.
    /// </summary>
    [TestFixture, Category("Distributions")]
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
            GC.KeepAlive(new Categorical(_largeP));
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
// ReSharper disable ExpressionIsAlwaysNull
            Assert.That(() => new Categorical(h), Throws.TypeOf<ArgumentNullException>());
// ReSharper restore ExpressionIsAlwaysNull
        }

        /// <summary>
        /// Categorical create fails with negative ratios.
        /// </summary>
        [Test]
        public void CategoricalCreateFailsWithNegativeRatios()
        {
            Assert.That(() => new Categorical(_badP), Throws.ArgumentException);
        }

        /// <summary>
        /// Categorical create fails with all zero ratios.
        /// </summary>
        [Test]
        public void CategoricalCreateFailsWithAllZeroRatios()
        {
            Assert.That(() => new Categorical(_badP2), Throws.ArgumentException);
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
        /// Validate mean.
        /// </summary>
        /// <param name="p">An array of nonnegative ratios.</param>
        /// <param name="mean">Expected value.</param>
        [TestCase(new[] { 0.0, 0.25, 0.5, 0.25 }, 2)]
        [TestCase(new[] { 0.0, 1, 2, 1 }, 2)]
        [TestCase(new[] { 0.0, 0.5, 0.5 }, 1.5)]
        [TestCase(new[] { 0.75, 0.25 }, 0.25)]
        [TestCase(new double[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, 5)]
        public void ValidateMean(double[] p, double mean)
        {
            Assert.That(new Categorical(p).Mean, Is.EqualTo(mean).Within(1e-14));
        }

        /// <summary>
        /// Validate standard deviation.
        /// </summary>
        /// <param name="p">An array of nonnegative ratios.</param>
        /// <param name="stdDev">Standard deviation.</param>
        [TestCase(new[] { 0.0, 0.25, 0.5, 0.25 }, 0.70710678118654752440084436210485)]
        [TestCase(new[] { 0.0, 1, 2, 1 }, 0.70710678118654752440084436210485)]
        [TestCase(new[] { 0.0, 0.5, 0.5 }, 0.5)]
        [TestCase(new[] { 0.75, 0.25 }, 0.43301270189221932338186158537647)]  //Sqrt((0.25*0.25)*.75+(.75*.75)*.25)
        [TestCase(new double[] { 1, 0, 1 }, 1)]
        public void ValidateStdDev(double[] p, double stdDev)
        {
            Assert.That(new Categorical(p).StdDev, Is.EqualTo(stdDev).Within(1e-14));
        }

        /// <summary>
        /// Validate variance.
        /// </summary>
        /// <param name="p">An array of nonnegative ratios.</param>
        /// <param name="variance">Variance.</param>
        [TestCase(new[] { 0.0, 0.25, 0.5, 0.25 }, 0.5)]
        [TestCase(new[] { 0.0, 1, 2, 1 }, 0.5)]
        [TestCase(new[] { 0.0, 0.5, 0.5 }, 0.25)]
        [TestCase(new[] { 0.75, 0.25 }, 0.1875)]  //(0.25*0.25)*.75+(.75*.75)*.25)
        [TestCase(new[] { 1.0, 0, 1 }, 1)]
        public void ValidateVariance(double[] p, double variance)
        {
            Assert.That(new Categorical(p).Variance, Is.EqualTo(variance).Within(1e-14));
        }

        /// <summary>
        /// Validate median.
        /// </summary>
        /// <param name="p">An array of nonnegative ratios.</param>
        /// <param name="median">Median.</param>
        [TestCase(new[] { 0.0, 0.25, 0.5, 0.25 }, 2)]
        [TestCase(new[] { 0.0, 1, 2, 1 }, 2)]
        [TestCase(new[] { 0.75, 0.25 }, 0)]
        // The following test case has median of 5, because:
        // P(X < 5) = (1+2+6+3+2)/29 = 14/29 < 0.5.
        // P(X <= 5) = 19/29 > 0.5.
        [TestCase(new[] { 1.0, 2, 6, 3, 2, 5, 1, 1, 0, 1, 7 }, 5)]
        // TODO: Find out the expected behavior of Median in ambiguous cases like the following:
        //[TestCase(new double[] { 0, 0.5, 0.5 }, ???)]
        //[TestCase(new double[] { 1, 0, 1 }, ???)]
        public void ValidateMedian(double[] p, int median)
        {
            Assert.That(new Categorical(p).Median, Is.EqualTo(median).Within(1e-14));
        }

        /// <summary>
        /// Can sample static.
        /// </summary>
        [Test]
        public void CanSampleStatic()
        {
            Categorical.Sample(new System.Random(0), _largeP);
        }

        /// <summary>
        /// Sample static fails with a bad parameters.
        /// </summary>
        [Test]
        public void FailSampleStatic()
        {
            Assert.That(() => Categorical.Sample(new System.Random(0), _badP), Throws.ArgumentException);
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
