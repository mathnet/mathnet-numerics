// <copyright file="GammaTests.cs" company="Math.NET">
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
    public class CommonDistributionTests
    {
        private IDistribution[] dists;

        [SetUp]
        public void SetupDistributions()
        {
            dists = new IDistribution[5];

            dists[0] = new Beta(1.0, 1.0);
            dists[1] = new ContinuousUniform(0.0, 1.0);
            dists[2] = new Gamma(1.0, 1.0);
            dists[3] = new Normal(0.0, 1.0);
            dists[4] = new Bernoulli(0.6);
        }

        [Test]
        [Row(0)]
        [Row(1)]
        [Row(2)]
        [Row(3)]
        [Row(4)]
        public void ValidateThatUnivariateDistributionsHaveRandomSource(int i)
        {
            Assert.IsNotNull(dists[i].RandomSource);
        }

        [Test]
        [Row(0)]
        [Row(1)]
        [Row(2)]
        [Row(3)]
        [Row(4)]
        public void CanSetRandomSource(int i)
        {
            dists[i].RandomSource = new Random();
        }

        [Test]
        [Row(0)]
        [Row(1)]
        [Row(2)]
        [Row(3)]
        [Row(4)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FailSetRandomSourceWithNullReference(int i)
        {
            dists[i].RandomSource = null;
        }
    }
}
