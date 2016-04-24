// <copyright file="EquidistantPolynomialTest.cs" company="Math.NET">
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
    public class EquidistantPolynomialTest
    {
        const double Tmin = 0.0;
        const double Tmax = 4.0;
        readonly double[] _y = { 0.0, 3.0, 2.5, 1.0, 3.0 };

        /// <summary>
        /// Verifies that the interpolation matches the given value at all the provided sample points.
        /// </summary>
        [Test]
        public void FitsAtSamplePoints()
        {
            IInterpolation it = Barycentric.InterpolatePolynomialEquidistant(Tmin, Tmax, _y);
            for (int i = 0; i < _y.Length; i++)
            {
                Assert.AreEqual(_y[i], it.Interpolate(i), "A Exact Point " + i);
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
        /// with(CurveFitting);
        /// evalf(subs({x=0.1},PolynomialInterpolation([[0,0],[1,3],[2,2.5],[3,1],[4,3]], x)),20);
        /// </remarks>
        [TestCase(0.1, .487425, 1e-15)]
        [TestCase(0.4, 1.6968, 1e-15)]
        [TestCase(1.1, 3.081925, 1e-15)]
        [TestCase(3.2, .9408, 1e-15)]
        [TestCase(4.5, 7.265625, 1e-14)]
        [TestCase(10.0, 592.5, 1e-10)]
        [TestCase(-10.0, 657.5, 1e-9)]
        public void FitsAtArbitraryPoints(double t, double x, double maxAbsoluteError)
        {
            IInterpolation it = Barycentric.InterpolatePolynomialEquidistant(Tmin, Tmax, _y);
            Assert.AreEqual(x, it.Interpolate(t), maxAbsoluteError, "Interpolation at {0}", t);
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
            IInterpolation it = Barycentric.InterpolatePolynomialEquidistant(x, y);
            for (int i = 0; i < xtest.Length; i++)
            {
                Assert.AreEqual(ytest[i], it.Interpolate(xtest[i]), 1e-12, "Linear with {0} samples, sample {1}", samples, i);
            }
        }

        [Test]
        public void FewSamples()
        {
            Assert.That(() => Barycentric.InterpolatePolynomialEquidistant(new double[0], new double[0]), Throws.ArgumentException);
            Assert.That(Barycentric.InterpolatePolynomialEquidistant(new[] { 1.0 }, new[] { 2.0 }).Interpolate(1.0), Is.EqualTo(2.0));
        }
    }
}
