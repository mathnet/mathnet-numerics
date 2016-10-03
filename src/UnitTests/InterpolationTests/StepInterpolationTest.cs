// <copyright file="LinearSplineTest.cs" company="Math.NET">
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
using MathNet.Numerics.Interpolation;
using NUnit.Framework;

namespace MathNet.Numerics.UnitTests.InterpolationTests
{
    [TestFixture, Category("Interpolation")]
    public class StepInterpolationTest
    {
        readonly double[] _t = { -2.0, -1.0, 0.0, 1.0, 2.0, 3.0 };
        readonly double[] _y = { 1.0, 2.0, -1.0, 0.0, 1.0, 0.0 };

        [Test]
        public void FirstDerivative()
        {
            IInterpolation ip = new StepInterpolation(_t, _y);
            Assert.That(ip.Differentiate(-3.0), Is.EqualTo(0.0));
            Assert.That(ip.Differentiate(-2.0), Is.EqualTo(double.NaN));
            Assert.That(ip.Differentiate(-1.5), Is.EqualTo(0.0));
            Assert.That(ip.Differentiate(-1.0), Is.EqualTo(double.NaN));
            Assert.That(ip.Differentiate(-0.5), Is.EqualTo(0.0));
            Assert.That(ip.Differentiate(0.0), Is.EqualTo(double.NaN));
            Assert.That(ip.Differentiate(0.5), Is.EqualTo(0.0));
            Assert.That(ip.Differentiate(1.0), Is.EqualTo(double.NaN));
            Assert.That(ip.Differentiate(2.0), Is.EqualTo(double.NaN));
            Assert.That(ip.Differentiate(3.0), Is.EqualTo(double.NaN));
            Assert.That(ip.Differentiate(4.0), Is.EqualTo(0.0));
        }

        [Test]
        public void DefiniteIntegral()
        {
            IInterpolation ip = new StepInterpolation(_t, _y);
            Assert.That(ip.Integrate(-3.0, -2.0), Is.EqualTo(0.0));
            Assert.That(ip.Integrate(-2.0, -1.0), Is.EqualTo(1.0));
            Assert.That(ip.Integrate(-1.0, 0.0), Is.EqualTo(2.0));
            Assert.That(ip.Integrate(0.0, 1.0), Is.EqualTo(-1.0));
            Assert.That(ip.Integrate(1.0, 2.0), Is.EqualTo(0.0));
            Assert.That(ip.Integrate(2.0, 3.0), Is.EqualTo(1.0));
            Assert.That(ip.Integrate(3.0, 4.0), Is.EqualTo(0.0));
            Assert.That(ip.Integrate(0.0, 4.0), Is.EqualTo(0.0));
            Assert.That(ip.Integrate(-3.0, -1.0), Is.EqualTo(1.0));
            Assert.That(ip.Integrate(-3.0, 4.0), Is.EqualTo(3.0));
            Assert.That(ip.Integrate(0.5, 1.5), Is.EqualTo(-0.5));
            Assert.That(ip.Integrate(-1.5, -0.5), Is.EqualTo(1.5));
            Assert.That(ip.Integrate(3.0, 4.0), Is.EqualTo(0.0));
        }

        /// <summary>
        /// Verifies that the interpolation matches the given value at all the provided sample points.
        /// </summary>
        [Test]
        public void FitsAtSamplePoints()
        {
            IInterpolation ip = new StepInterpolation(_t, _y);
            for (int i = 0; i < _y.Length; i++)
            {
                Assert.AreEqual(_y[i], ip.Interpolate(_t[i]), "A Exact Point " + i);
            }
        }

        [Test]
        public void FewSamples()
        {
            Assert.That(() => StepInterpolation.Interpolate(new double[0], new double[0]), Throws.ArgumentException);
            Assert.That(StepInterpolation.Interpolate(new[] { 1.0 }, new[] { 2.0 }).Interpolate(1.0), Is.EqualTo(2.0));
        }
    }
}
