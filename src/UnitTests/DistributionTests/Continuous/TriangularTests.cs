// <copyright file="TriangularTests.cs" company="Math.NET">
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
using System.Linq;
using MathNet.Numerics.Distributions;
using NUnit.Framework;

namespace MathNet.Numerics.UnitTests.DistributionTests.Continuous
{
    using Random = System.Random;

    /// <summary>
    /// Triangular distribution tests.
    /// </summary>
    [TestFixture, Category("Distributions")]
    public class TriangularTests
    {
        /// <summary>
        /// Can create Triangular distribution.
        /// </summary>
        /// <param name="lower">Lower Bound.</param>
        /// <param name="upper">Upper Bound.</param>
        /// <param name="mode">Mode.</param>
        [TestCase(0.0, 0.0, 0.0)]
        [TestCase(-1.0, 1.0, 0.0)]
        [TestCase(1.0, 2.0, 1.0)]
        [TestCase(5.0, 25.0, 25.0)]
        [TestCase(1.0e-5, 1.0e5, 1.0e-3)]
        [TestCase(0.0, 1.0, 0.9)]
        [TestCase(-4.0, -0.5, -2.0)]
        [TestCase(-13.039, 8.42, 1.17)]
        public void CanCreateTriangular(double lower, double upper, double mode)
        {
            var n = new Triangular(lower, upper, mode);
            Assert.AreEqual(lower, n.LowerBound);
            Assert.AreEqual(upper, n.UpperBound);
            Assert.AreEqual(mode, n.Mode);
        }

        /// <summary>
        /// Triangular create fails with bad parameters.
        /// </summary>
        [TestCase(0.0, 1.0, -0.1)]
        [TestCase(0.0, 1.0, 1.1)]
        [TestCase(0.0, -1.0, 0.5)]
        [TestCase(2.0, 1.0, 1.5)]
        [TestCase(Double.NaN, 1.0, 0.5)]
        [TestCase(0.2, Double.NaN, 0.5)]
        [TestCase(0.5, 1.0, Double.NaN)]
        [TestCase(Double.NaN, Double.NaN, Double.NaN)]
        [TestCase(Double.NegativeInfinity, 1.0, 0.5)]
        [TestCase(0.0, Double.PositiveInfinity, 0.5)]
        public void TriangularCreateFailsWithBadParameters(double lower, double upper, double mode)
        {
            Assert.That(() => new Triangular(lower, upper, mode), Throws.ArgumentException);
        }

        /// <summary>
        /// Validate to string.
        /// </summary>
        [Test]
        public void ValidateToString()
        {
            var n = new Triangular(0d, 2d, 1d);
            Assert.AreEqual("Triangular(Lower = 0, Upper = 2, Mode = 1)", n.ToString());
        }

        // Todo: Add tests for:
        // - Mean,
        // - Entropy,
        // - Skewness,
        // - Mode (note: Mode is an input parameter for this distribution),
        // - Median
        // - Minimum (should be the same as LowerBound)
        // - Maximum (should be the same as UpperBound)

        /// <summary>
        /// Can sample static.
        /// </summary>
        [Test]
        public void CanSampleStatic()
        {
            Triangular.Sample(new Random(0), 2.0, 3.0, 2.5);
            Triangular.Sample(10.0, 100.0, 30.0);
        }

        /// <summary>
        /// Can sample sequence static.
        /// </summary>
        [Test]
        public void CanSampleSequenceStatic()
        {
            var ied = Triangular.Samples(new Random(0), 2.0, 3.0, 2.5);
            GC.KeepAlive(ied.Take(5).ToArray());
            ied = Triangular.Samples(10.0, 100.0, 30.0);
            GC.KeepAlive(ied.Take(5).ToArray());
        }

        /// <summary>
        /// Fail sample static with wrong parameters.
        /// </summary>
        [Test]
        public void FailSampleStatic()
        {
            Assert.That(() => Triangular.Sample(new Random(0), 1.0, -1.0, 0.1), Throws.ArgumentException);
        }

        /// <summary>
        /// Fail sample sequence static with wrong parameters.
        /// </summary>
        [Test]
        public void FailSampleSequenceStatic()
        {
            Assert.That(() => Triangular.Samples(new Random(0), 1.0, -1.0, 0.1).First(), Throws.ArgumentException);
        }

        /// <summary>
        /// Can sample.
        /// </summary>
        [Test]
        public void CanSample()
        {
            var n = new Triangular(0.1, 0.3, 0.2);
            n.Sample();
        }

        /// <summary>
        /// Can sample sequence.
        /// </summary>
        [Test]
        public void CanSampleSequence()
        {
            var n = new Triangular(0.1, 0.3, 0.2);
            var ied = n.Samples();
            GC.KeepAlive(ied.Take(5).ToArray());
        }

        // Todo: Add tests for:
        // - Density
        // - DensityLn
        // - Cumulative Dist.
        // - Inverse Cumulative Dist.
    }
}
