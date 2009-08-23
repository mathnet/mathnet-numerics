// <copyright file="MultinomialTests.cs" company="Math.NET">
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
    public class MultinomialTests
    {
        double[] badP;
        double[] badP2;
        double[] smallP;
        double[] largeP;

        [SetUp]
        public void SetUp()
        {
            Control.CheckDistributionParameters = true;
            badP = new double[] { -1.0, 1.0 };
            badP2 = new double[] { 0.0, 0.0 };
            smallP = new double[] {1.0, 1.0, 1.0};
            largeP = new double[] {1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0, 8.0, 9.0};
        }

        [Test]
        public void CanCreateMultinomial()
        {
            var m = new Multinomial(largeP);
            AssertEx.AreEqual<double[]>(largeP, m.P);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void MultinomialCreateFailsWithNegativeRatios()
        {
            var m = new Multinomial(badP);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void MultinomialCreateFailsWithAllZeroRatios()
        {
            var m = new Multinomial(badP2);
        }

        [Test]
        public void ValidateToString()
        {
            var b = new Multinomial(smallP);
            AssertEx.AreEqual<string>("Multinomial(Dimension = 3)", b.ToString());
        }

        [Test]
        public void CanSetProbability()
        {
            var b = new Multinomial(largeP);
            b.P = smallP;
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void SetProbabilityFails()
        {
            var b = new Multinomial(largeP);
            b.P = badP;
        }

        [Test]
        public void CanSampleStatic()
        {
            var d = Multinomial.Sample(new Random(), largeP);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void FailSampleStatic()
        {
            var d = Multinomial.Sample(new Random(), badP);
        }

        [Test]
        public void CanSample()
        {
            var n = new Multinomial(largeP);
            var d = n.Sample();
        }
    }
}