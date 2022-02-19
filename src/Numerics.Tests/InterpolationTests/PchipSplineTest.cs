// <copyright file="PchipSplineTest.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2021 Math.NET
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

namespace MathNet.Numerics.Tests.InterpolationTests
{
    [TestFixture, Category("Interpolation")]
    public class PchipSplineTest
    {
        readonly double[] _t = { -2.0, -1.0, 0.0, 1.0, 2.0 };
        readonly double[] _y = { 1.0, 2.0, -1.0, 0.0, 1.0 };

        readonly double[] _tNag = { 7.99, 8.09, 8.19, 8.70, 9.20, 10.00, 12.00, 15.00, 20.00 };
        readonly double[] _yNag = { 0.00000E+0, 0.27643E-4, 0.43750E-1, 0.16918E+0, 0.46943E+0, 0.94374E+0, 0.99864E+0, 0.99992E+0, 0.99999E+0 };

        /// <summary>
        /// Verifies that the interpolation matches the given value at all the provided sample points.
        /// </summary>
        [Test]
        public void FitsAtSamplePoints()
        {
            IInterpolation it = CubicSpline.InterpolatePchip(_t, _y);
            for (int i = 0; i < _y.Length; i++)
            {
                Assert.AreEqual(_y[i], it.Interpolate(_t[i]), "A Exact Point " + i);
            }
        }

        /// <summary>
        /// Verifies that at points other than the provided sample points, the interpolation matches the one computed by Octave as a reference.
        /// </summary>
        /// <param name="t">Sample point.</param>
        /// <param name="x">Sample value.</param>
        /// <param name="maxAbsoluteError">Maximum absolute error.</param>
        [TestCase(-2.4, -0.7440, 1e-15)]
        [TestCase(-0.9, 1.9160, 1e-15)]
        [TestCase(-0.5, 0.5000, 1e-15)]
        [TestCase(-0.1, -0.9160, 1e-15)]
        [TestCase(0.1, -0.9810, 1e-15)]
        [TestCase(0.4, -0.7440, 1e-15)]
        [TestCase(1.2, 0.2000, 1e-15)]
        [TestCase(10.0, 9.0000, 1e-15)]
        [TestCase(-10.0, -727.0000, 1e-15)]
        public void FitsAtArbitraryPoints(double t, double x, double maxAbsoluteError)
        {
            IInterpolation it = CubicSpline.InterpolatePchip(_t, _y);

            Assert.AreEqual(x, it.Interpolate(t), maxAbsoluteError, "Interpolation at {0}", t);
        }

        /// <summary>
        /// Verifies that at points other than the provided sample points, the interpolation matches the one computed by NAG as a reference.
        /// Reference: https://www.nag.com/numeric/cl/nagdoc_cl25/html/e01/e01bec.html
        /// </summary>
        /// <param name="t">Sample point.</param>
        /// <param name="x">Sample value.</param>
        /// <param name="maxAbsoluteError">Maximum absolute error.</param>
        [TestCase(7.9900, 0.0000, 5e-5)]
        [TestCase(9.1910, 0.4640, 5e-5)]
        [TestCase(10.3920, 0.9645, 5e-5)]
        [TestCase(11.5930, 0.9965, 5e-5)]
        [TestCase(12.7940, 0.9992, 5e-5)]
        [TestCase(13.9950, 0.9998, 5e-5)]
        [TestCase(15.1960, 0.9999, 5e-5)]
        [TestCase(16.3970, 1.0000, 5e-5)]
        [TestCase(17.5980, 1.0000, 5e-5)]
        [TestCase(18.7990, 1.0000, 5e-5)]
        [TestCase(20.0000, 1.0000, 5e-5)]
        public void FitsAtNagExamplePoints(double t, double x, double maxAbsoluteError)
        {
            IInterpolation it = CubicSpline.InterpolatePchip(_tNag, _yNag);

            Assert.AreEqual(x, it.Interpolate(t), maxAbsoluteError, "Interpolation at {0}", t);
        }

        /// <summary>
        /// Verifies that the interpolation supports the linear case appropriately
        /// </summary>
        /// <param name="samples">Samples array.</param>
        [TestCase(5)]
        [TestCase(7)]
        [TestCase(15)]
        public void SupportsLinearCase(int samples)
        {
            double[] x, y, xtest, ytest;
            LinearInterpolationCase.Build(out x, out y, out xtest, out ytest, samples);
            IInterpolation it = CubicSpline.InterpolatePchip(x, y);
            for (int i = 0; i < xtest.Length; i++)
            {
                Assert.AreEqual(ytest[i], it.Interpolate(xtest[i]), 1e-15, "Linear with {0} samples, sample {1}", samples, i);
            }
        }

        [Test]
        public void FewSamples()
        {
            Assert.That(() => CubicSpline.InterpolatePchip(new double[0], new double[0]), Throws.ArgumentException);
            Assert.That(() => CubicSpline.InterpolatePchip(new double[2], new double[2]), Throws.ArgumentException);
            Assert.That(CubicSpline.InterpolatePchip(new[] { 1.0, 2.0, 3.0 }, new[] { 2.0, 2.0, 2.0 }).Interpolate(1.0), Is.EqualTo(2.0));
        }
    }
}
