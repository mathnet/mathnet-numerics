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

using MathNet.Numerics.Interpolation;
using NUnit.Framework;

namespace MathNet.Numerics.UnitTests.InterpolationTests
{
    [TestFixture, Category("Interpolation")]
    public class LinearSplineTest
    {
        readonly double[] _t = { -2.0, -1.0, 0.0, 1.0, 2.0 };
        readonly double[] _y = { 1.0, 2.0, -1.0, 0.0, 1.0 };

        [Test]
        public void FirstDerivative()
        {
            IInterpolation ip = LinearSpline.Interpolate(_t, _y);
            Assert.That(ip.Differentiate(-3.0), Is.EqualTo(1.0));
            Assert.That(ip.Differentiate(-2.0), Is.EqualTo(1.0));
            Assert.That(ip.Differentiate(-1.5), Is.EqualTo(1.0));
            Assert.That(ip.Differentiate(-1.0), Is.EqualTo(-3.0));
            Assert.That(ip.Differentiate(-0.5), Is.EqualTo(-3.0));
            Assert.That(ip.Differentiate(0.0), Is.EqualTo(1.0));
            Assert.That(ip.Differentiate(0.5), Is.EqualTo(1.0));
            Assert.That(ip.Differentiate(1.0), Is.EqualTo(1.0));
            Assert.That(ip.Differentiate(2.0), Is.EqualTo(1.0));
            Assert.That(ip.Differentiate(3.0), Is.EqualTo(1.0));
        }

        [Test]
        public void DefiniteIntegral()
        {
            IInterpolation ip = LinearSpline.Interpolate(_t, _y);
            Assert.That(ip.Integrate(-4.0, -3.0), Is.EqualTo(-0.5));
            Assert.That(ip.Integrate(-3.0, -2.0), Is.EqualTo(0.5));
            Assert.That(ip.Integrate(-2.0, -1.0), Is.EqualTo(1.5));
            Assert.That(ip.Integrate(-1.0, 0.0), Is.EqualTo(0.5));
            Assert.That(ip.Integrate(0.0, 1.0), Is.EqualTo(-0.5));
            Assert.That(ip.Integrate(1.0, 2.0), Is.EqualTo(0.5));
            Assert.That(ip.Integrate(2.0, 3.0), Is.EqualTo(1.5));
            Assert.That(ip.Integrate(3.0, 4.0), Is.EqualTo(2.5));
            Assert.That(ip.Integrate(0.0, 4.0), Is.EqualTo(4.0));
            Assert.That(ip.Integrate(-3.0, -1.0), Is.EqualTo(2.0));
            Assert.That(ip.Integrate(-3.0, 4.0), Is.EqualTo(6.5));
            Assert.That(ip.Integrate(0.5, 1.5), Is.EqualTo(0.0));
            Assert.That(ip.Integrate(-2.5, -1.0), Is.EqualTo(1.875));
        }

        /// <summary>
        /// Verifies that the interpolation matches the given value at all the provided sample points.
        /// </summary>
        [Test]
        public void FitsAtSamplePoints()
        {
            IInterpolation ip = LinearSpline.Interpolate(_t, _y);
            for (int i = 0; i < _y.Length; i++)
            {
                Assert.AreEqual(_y[i], ip.Interpolate(_t[i]), "A Exact Point " + i);
            }
        }

        /// <summary>
        /// Verifies that at points other than the provided sample points, the interpolation matches the one computed by Maple as a reference.
        /// </summary>
        /// <param name="t">Sample point.</param>
        /// <param name="x">Sample value.</param>
        /// <param name="maxAbsoluteError">Maximum absolute error.</param>
        /// <remarks>
        /// Maple:
        /// f := x -> piecewise(x&lt;-1,3+x,x&lt;0,-1-3*x,x&lt;1,-1+x,-1+x);
        /// f(x)
        /// </remarks>
        [TestCase(-2.4, .6, 1e-15)]
        [TestCase(-0.9, 1.7, 1e-15)]
        [TestCase(-0.5, .5, 1e-15)]
        [TestCase(-0.1, -.7, 1e-15)]
        [TestCase(0.1, -.9, 1e-15)]
        [TestCase(0.4, -.6, 1e-15)]
        [TestCase(1.2, .2, 1e-15)]
        [TestCase(10.0, 9.0, 1e-15)]
        [TestCase(-10.0, -7.0, 1e-15)]
        public void FitsAtArbitraryPoints(double t, double x, double maxAbsoluteError)
        {
            IInterpolation ip = LinearSpline.Interpolate(_t, _y);
            Assert.AreEqual(x, ip.Interpolate(t), maxAbsoluteError, "Interpolation at {0}", t);
        }

        /// <summary>
        /// Verifies that the interpolation supports the linear case appropriately
        /// </summary>
        /// <param name="samples">Samples array.</param>
        [TestCase(2)]
        [TestCase(4)]
        [TestCase(12)]
        public void SupportsLinearCase(int samples)
        {
            double[] x, y, xtest, ytest;
            LinearInterpolationCase.Build(out x, out y, out xtest, out ytest, samples);

            IInterpolation ip = LinearSpline.Interpolate(x, y);
            for (int i = 0; i < xtest.Length; i++)
            {
                Assert.AreEqual(ytest[i], ip.Interpolate(xtest[i]), 1e-15, "Linear with {0} samples, sample {1}", samples, i);
            }
        }

        [Test]
        public void FewSamples()
        {
            Assert.That(() => LinearSpline.Interpolate(new double[0], new double[0]), Throws.ArgumentException);
            Assert.That(() => LinearSpline.Interpolate(new double[1], new double[1]), Throws.ArgumentException);
            Assert.That(LinearSpline.Interpolate(new[] { 1.0, 2.0 }, new[] { 2.0, 2.0 }).Interpolate(1.0), Is.EqualTo(2.0));
        }
    }
}
