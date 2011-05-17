// <copyright file="EquidistantPolynomialTest.cs" company="Math.NET">
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

    /// <summary>
    /// EquidistantPolynomial Test case.
    /// </summary>
    [TestFixture]
    public class EquidistantPolynomialTest
    {
        /// <summary>
        /// Left bound;
        /// </summary>
        private const double Tmin = 0.0;

        /// <summary>
        /// Right bound.
        /// </summary>
        private const double Tmax = 4.0;

        /// <summary>
        /// Sample values.
        /// </summary>
        private readonly double[] _x = new[] { 0.0, 3.0, 2.5, 1.0, 3.0 };

        /// <summary>
        /// Verifies that the interpolation matches the given value at all the provided sample points.
        /// </summary>
        [Test]
        public void FitsAtSamplePoints()
        {
            IInterpolation interpolation = new EquidistantPolynomialInterpolation(Tmin, Tmax, _x);

            for (int i = 0; i < _x.Length; i++)
            {
                Assert.AreEqual(_x[i], interpolation.Interpolate(i), "A Exact Point " + i);
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
        public void FitsAtArbitraryPointsWithMaple(double t, double x, double maxAbsoluteError)
        {
            IInterpolation interpolation = new EquidistantPolynomialInterpolation(Tmin, Tmax, _x);

            Assert.AreEqual(x, interpolation.Interpolate(t), maxAbsoluteError, "Interpolation at {0}", t);
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
            IInterpolation interpolation = new EquidistantPolynomialInterpolation(x, y);
            for (int i = 0; i < xtest.Length; i++)
            {
                Assert.AreEqual(ytest[i], interpolation.Interpolate(xtest[i]), 1e-12, "Linear with {0} samples, sample {1}", samples, i);
            }
        }
    }
}
