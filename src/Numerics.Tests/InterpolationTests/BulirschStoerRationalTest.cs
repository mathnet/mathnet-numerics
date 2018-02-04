// <copyright file="BulirschStoerRationalTest.cs" company="Math.NET">
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
    /// <summary>
    /// BulirschStoerRational test case.
    /// </summary>
    [TestFixture, Category("Interpolation")]
    public class BulirschStoerRationalTest
    {
        /// <summary>
        /// Sample points.
        /// </summary>
        readonly double[] _t = { 0d, 1, 3, 4, 5 };

        /// <summary>
        /// Sample values.
        /// </summary>
        readonly double[] _x = { 0d, 3, 1000, -1000, 3 };

        /// <summary>
        /// Verifies that the interpolation matches the given value at all the provided sample points.
        /// </summary>
        [Test]
        public void FitsAtSamplePoints()
        {
            IInterpolation interpolation = BulirschStoerRationalInterpolation.Interpolate(_t, _x);

            for (int i = 0; i < _x.Length; i++)
            {
                Assert.AreEqual(_x[i], interpolation.Interpolate(_t[i]), "A Exact Point " + i);
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
        /// evalf(subs({x=0.1},RationalInterpolation([[0,0],[1,3],[3,1000],[4,-1000], [5,3]], x)),20);
        /// </remarks>
        [TestCase(0.1, .19389203383553566255, 1e-14)]
        [TestCase(0.4, .88132900698869875369, 1e-14)]
        [TestCase(1.1, 3.5057665681580626913, 1e-15)]
        [TestCase(3.01, 1548.7666642693586902, 1e-10)]
        [TestCase(3.02, 3362.2564334253633516, 1e-10)]
        [TestCase(3.03, -22332.603641443806014, 1e-8)]
        [TestCase(3.1, -440.30323769822443789, 1e-11)]
        [TestCase(3.2, -202.42421196280566349, 1e-12)]
        [TestCase(4.5, 21.208249625210155439, 1e-12)]
        [TestCase(10.0, -4.8936986959784751517, 1e-13)]
        [TestCase(-10.0, -3.6017584308603731307, 1e-13)]
        public void FitsAtArbitraryPointsWithMaple(double t, double x, double maxAbsoluteError)
        {
            IInterpolation interpolation = BulirschStoerRationalInterpolation.Interpolate(_t, _x);

            Assert.AreEqual(x, interpolation.Interpolate(t), maxAbsoluteError, "Interpolation at {0}", t);
        }

        [Test]
        public void FewSamples()
        {
            Assert.That(() => BulirschStoerRationalInterpolation.Interpolate(new double[0], new double[0]), Throws.ArgumentException);
            Assert.That(BulirschStoerRationalInterpolation.Interpolate(new[] { 1.0 }, new[] { 2.0 }).Interpolate(1.0), Is.EqualTo(2.0));
        }

        // NOTE: No test for the linear case because this algorithms is incredibly bad at this.
    }
}
