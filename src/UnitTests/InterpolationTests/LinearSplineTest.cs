// <copyright file="LinearSplineTest.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
//
// Copyright (c) 2002-2011 Math.NET
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

namespace MathNet.Numerics.UnitTests.InterpolationTests
{
    using Interpolation;
    using Interpolation.Algorithms;
    using NUnit.Framework;

    [TestFixture]
    public class LinearSplineTest
    {
        readonly double[] _t = new[] { -2.0, -1.0, 0.0, 1.0, 2.0 };
        readonly double[] _x = new[] { 1.0, 2.0, -1.0, 0.0, 1.0 };

        /// <summary>
        /// Verifies that the interpolation matches the given value at all the provided sample points.
        /// </summary>
        [Test]
        public void FitsAtSamplePoints()
        {
            IInterpolation interpolation = new LinearSplineInterpolation(_t, _x);

            for (int i = 0; i < _x.Length; i++)
            {
                Assert.AreEqual(_x[i], interpolation.Interpolate(_t[i]), "A Exact Point " + i);

                double interpolatedValue;
                double secondDerivative;
                interpolation.Differentiate(_t[i], out interpolatedValue, out secondDerivative);
                Assert.AreEqual(_x[i], interpolatedValue, "B Exact Point " + i);
            }
        }

        /// <summary>
        /// Verifies that at points other than the provided sample points, the interpolation matches the one computed by Maple as a reference.
        /// </summary>
        /// <remarks>
        /// Maple:
        /// f := x -> piecewise(x&lt;-1,3+x,x&lt;0,-1-3*x,x&lt;1,-1+x,-1+x);
        /// f(x)
        /// </remarks>
        [Test, Sequential]
        public void FitsAtArbitraryPointsWithMaple(
            [Values(-2.4, -0.9, -0.5, -0.1, 0.1, 0.4, 1.2, 10.0, -10.0)] double t,
            [Values(.6, 1.7, .5, -.7, -.9, -.6, .2, 9.0, -7.0)] double x,
            [Values(1e-15, 1e-15, 1e-15, 1e-15, 1e-15, 1e-15, 1e-15, 1e-15, 1e-15)] double maxAbsoluteError)
        {
            IInterpolation interpolation = new LinearSplineInterpolation(_t, _x);

            Assert.AreEqual(x, interpolation.Interpolate(t), maxAbsoluteError, "Interpolation at {0}", t);

            double interpolatedValue;
            double secondDerivative;
            interpolation.Differentiate(t, out interpolatedValue, out secondDerivative);
            Assert.AreEqual(x, interpolatedValue, maxAbsoluteError, "Interpolation as by-product of differentiation at {0}", t);
        }

        /// <summary>
        /// Verifies that the interpolation supports the linear case appropriately
        /// </summary>
        [Test]
        public void SupportsLinearCase([Values(2, 4, 12)] int samples)
        {
            double[] x, y, xtest, ytest;
            LinearInterpolationCase.Build(out x, out y, out xtest, out ytest, samples);
            IInterpolation interpolation = new LinearSplineInterpolation(x, y);
            for (int i = 0; i < xtest.Length; i++)
            {
                Assert.AreEqual(ytest[i], interpolation.Interpolate(xtest[i]), 1e-15, "Linear with {0} samples, sample {1}", samples, i);
            }
        }
    }
}
