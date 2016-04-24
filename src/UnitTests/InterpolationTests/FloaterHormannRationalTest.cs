// <copyright file="FloaterHormannRationalTest.cs" company="Math.NET">
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
    public class FloaterHormannRationalTest
    {
        readonly double[] _t = { -2.0, -1.0, 0.0, 1.0, 2.0 };
        readonly double[] _y = { 1.0, 2.0, -1.0, 0.0, 1.0 };

        /// <summary>
        /// Verifies that the interpolation matches the given value at all the provided polynomial sample points.
        /// </summary>
        [Test]
        public void PolyomnialFitsAtSamplePoints()
        {
            IInterpolation it = Barycentric.InterpolateRationalFloaterHormann(_t, _y);
            for (int i = 0; i < _y.Length; i++)
            {
                Assert.AreEqual(_y[i], it.Interpolate(_t[i]), "A Exact Point " + i);
            }
        }

        /// <summary>
        /// Verifies that at points other than the provided polynomial sample points, the interpolation matches the one computed by Maple as a reference.
        /// </summary>
        /// <param name="t">Sample point.</param>
        /// <param name="x">Sample value.</param>
        /// <param name="maxAbsoluteError">Maximum absolute error.</param>
        /// <remarks>
        /// Maple:
        /// with(CurveFitting);
        /// PolynomialInterpolation([[-2,1],[-1,2],[0,-1],[1,0],[2,1]], x);
        /// </remarks>
        [TestCase(-2.4, -4.5968, 1e-14)]
        [TestCase(-0.9, 1.65395, 1e-15)]
        [TestCase(-0.5, 0.21875, 1e-15)]
        [TestCase(-0.1, -0.84205, 1e-15)]
        [TestCase(0.1, -1.10805, 1e-15)]
        [TestCase(0.4, -1.1248, 1e-15)]
        [TestCase(1.2, 0.5392, 1e-15)]
        [TestCase(10.0, -4431.0, 1e-8)]
        [TestCase(-10.0, -5071.0, 1e-8)]
        public void PolynomialFitsAtArbitraryPointsWithMaple(double t, double x, double maxAbsoluteError)
        {
            IInterpolation it = Barycentric.InterpolateRationalFloaterHormann(_t, _y);
            Assert.AreEqual(x, it.Interpolate(t), maxAbsoluteError, "Interpolation at {0}", t);
        }

        /// <summary>
        /// Verifies that the interpolation matches the given value at all the provided rational sample points.
        /// </summary>
        [Test]
        public void RationalFitsAtSamplePoints()
        {
            var t = new double[40];
            var x = new double[40];

            const double Step = 10.0/39.0;
            for (int i = 0; i < t.Length; i++)
            {
                double tt = -5 + (i*Step);
                t[i] = tt;
                x[i] = 1.0/(1.0 + (tt*tt));
            }

            IInterpolation it = Barycentric.InterpolateRationalFloaterHormann(t, x);
            for (int i = 0; i < x.Length; i++)
            {
                Assert.AreEqual(x[i], it.Interpolate(t[i]), "A Exact Point " + i);
            }
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
            IInterpolation it = Barycentric.InterpolateRationalFloaterHormann(x, y);
            for (int i = 0; i < xtest.Length; i++)
            {
                Assert.AreEqual(ytest[i], it.Interpolate(xtest[i]), 1e-14, "Linear with {0} samples, sample {1}", samples, i);
            }
        }

        [Test]
        public void FewSamples()
        {
            Assert.That(() => Barycentric.InterpolateRationalFloaterHormann(new double[0], new double[0]), Throws.ArgumentException);
            Assert.That(Barycentric.InterpolateRationalFloaterHormann(new[] { 1.0 }, new[] { 2.0 }).Interpolate(1.0), Is.EqualTo(2.0));
        }
    }
}
