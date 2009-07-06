// <copyright file="Combinatorics.cs" company="Math.NET">
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

namespace MathNet.Numerics.UnitTests
{
    using System;
    using MbUnit.Framework;
    using MathNet.Numerics.Distributions;

    [TestFixture]
    public class NormalTests
    {
        [Test, MultipleAsserts]
        public void CanCreateStandardNormal()
        {
            var n = new Normal();
            AssertEx.AreEqual<double>(0.0, n.Mean);
            AssertEx.AreEqual<double>(1.0, n.StdDev);
        }

        [Test, MultipleAsserts]
        [Row(0.0, 0.0)]
        [Row(0.0, 0.1)]
        [Row(0.0, 1.0)]
        [Row(0.0, 10.0)]
        [Row(10.0, 1.0)]
        [Row(-5.0, 100.0)]
        [Row(0.0, Double.PositiveInfinity)]
        public void CanCreateNormal(double mean, double sdev)
        {
            var n = new Normal(mean, sdev);
            AssertEx.AreEqual<double>(mean, n.Mean);
            AssertEx.AreEqual<double>(sdev, n.StdDev);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void NormalCreateFailsWithMeanIsNaN()
        {
            var n = new Normal(Double.NaN, 1.0);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void NormalCreateFailsWithStdDevIsNaN()
        {
            var n = new Normal(0.0, Double.NaN);
        }

        [Test, MultipleAsserts]
        [Row(0.0, 0.0)]
        [Row(0.0, 0.1)]
        [Row(0.0, 1.0)]
        [Row(0.0, 10.0)]
        [Row(10.0, 1.0)]
        [Row(-5.0, 100.0)]
        [Row(0.0, Double.PositiveInfinity)]
        public void CanCreateNormalFromMeanAndStdDev(double mean, double sdev)
        {
            var n = Normal.WithMeanStdDev(mean, sdev);
            AssertEx.AreEqual<double>(mean, n.Mean);
            AssertEx.AreEqual<double>(sdev, n.StdDev);
        }

        [Test, MultipleAsserts]
        [Row(0.0, 0.0)]
        [Row(0.0, 0.1)]
        [Row(0.0, 1.0)]
        [Row(0.0, 10.0)]
        [Row(10.0, 1.0)]
        [Row(-5.0, 100.0)]
        [Row(0.0, Double.PositiveInfinity)]
        public void CanCreateNormalFromMeanAndVariance(double mean, double var)
        {
            var n = Normal.WithMeanVariance(mean, var);
            AssertEx.AreEqual<double>(mean, n.Mean);
            AssertEx.AreEqual<double>(var, n.Variance);
        }

        [Test, MultipleAsserts]
        [Row(0.0, 0.0)]
        [Row(0.0, 0.1)]
        [Row(0.0, 1.0)]
        [Row(0.0, 10.0)]
        [Row(10.0, 1.0)]
        [Row(-5.0, 100.0)]
        [Row(0.0, Double.PositiveInfinity)]
        public void CanCreateNormalFromMeanAndPrecision(double mean, double prec)
        {
            var n = Normal.WithMeanAndPrecision(mean, prec);
            AssertEx.AreEqual<double>(mean, n.Mean);
            AssertEx.AreEqual<double>(prec, n.Precision);
        }

        [Test]
        public void ToStringTest()
        {
            var n = new Normal(1.0, 2.0);
            AssertEx.AreEqual<string>("Normal(Mean = 1, StdDev = 2)", n.ToString());
        }

        [Test]
        public void CanGetRandomNumberGenerator()
        {
            var n = new Normal();
            var rs = n.RandomNumberGenerator;
            Assert.IsNotNull(rs);
        }

        [Test]
        [Row(-0.0)]
        [Row(0.0)]
        [Row(0.1)]
        [Row(1.0)]
        [Row(10.0)]
        [Row(0.0, Double.PositiveInfinity)]
        public void CanSetRandomNumberGenerator(double prec)
        {
            var n = new Normal();
            n.Precision = prec;
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void SetPrecisionFailsWithNegativePrecision()
        {
            var n = new Normal();
            n.Precision = -1.0;
        }
    }
}
