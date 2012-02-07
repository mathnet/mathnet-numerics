// <copyright file="CubicSplineTest.cs" company="Math.NET">
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
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using Interpolation;
    using Interpolation.Algorithms;
    using NUnit.Framework;

    /// <summary>
    /// CubicSpline Test case.
    /// </summary>
    [TestFixture]
    public class CubicSplineTest
    {
        /// <summary>
        /// Sample points.
        /// </summary>
        private readonly double[] _t = new[] { -2.0, -1.0, 0.0, 1.0, 2.0 };

        /// <summary>
        /// Sample values.
        /// </summary>
        private readonly double[] _x = new[] { 1.0, 2.0, -1.0, 0.0, 1.0 };

        /// <summary>
        /// Verifies that the interpolation matches the given value at all the provided sample points.
        /// </summary>
        [Test]
        public void NaturalFitsAtSamplePoints()
        {
            IInterpolation interpolation = new CubicSplineInterpolation(_t, _x);

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
        /// <param name="t">Sample point.</param>
        /// <param name="x">Sample value.</param>
        /// <param name="maxAbsoluteError">Maximum absolute error.</param>
        /// <remarks>
        /// Maple:
        /// with(CurveFitting);
        /// evalf(subs({x=-2.4},Spline([[-2,1],[-1,2],[0,-1],[1,0],[2,1]], x, degree=3, endpoints='natural')),20);
        /// </remarks>
        [TestCase(-2.4, .144, 1e-15)]
        [TestCase(-0.9, 1.7906428571428571429, 1e-15)]
        [TestCase(-0.5, .47321428571428571431, 1e-15)]
        [TestCase(-0.1, -.80992857142857142857, 1e-15)]
        [TestCase(0.1, -1.1089285714285714286, 1e-15)]
        [TestCase(0.4, -1.0285714285714285714, 1e-15)]
        [TestCase(1.2, .30285714285714285716, 1e-15)]
        [TestCase(10.0, 189, 1e-15)]
        [TestCase(-10.0, 677, 1e-12)]
        public void NaturalFitsAtArbitraryPointsWithMaple(double t, double x, double maxAbsoluteError)
        {
            IInterpolation interpolation = new CubicSplineInterpolation(_t, _x);

            Assert.AreEqual(x, interpolation.Interpolate(t), maxAbsoluteError, "Interpolation at {0}", t);

            double interpolatedValue;
            double secondDerivative;
            interpolation.Differentiate(t, out interpolatedValue, out secondDerivative);
            Assert.AreEqual(x, interpolatedValue, maxAbsoluteError, "Interpolation as by-product of differentiation at {0}", t);
        }

        /// <summary>
        /// Verifies that the interpolation matches the given value at all the provided sample points.
        /// </summary>
        [Test]
        public void FixedFirstDerivativeFitsAtSamplePoints()
        {
            IInterpolation interpolation = new CubicSplineInterpolation(_t, _x, SplineBoundaryCondition.FirstDerivative, 1.0, SplineBoundaryCondition.FirstDerivative, -1.0);

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
        /// <param name="t">Sample point.</param>
        /// <param name="x">Sample value.</param>
        /// <param name="maxAbsoluteError">Maximum absolute error.</param>
        /// <remarks>
        /// Maple:
        /// with(CurveFitting);
        /// evalf(subs({x=-2.4},Spline([[-2,1],[-1,2],[0,-1],[1,0],[2,1]], x, degree=3, endpoints=[1,-1])),20);
        /// </remarks>
        [TestCase(-2.4, 1.12, 1e-15)]
        [TestCase(-0.9, 1.8243928571428571428, 1e-15)]
        [TestCase(-0.5, .54910714285714285715, 1e-15)]
        [TestCase(-0.1, -.78903571428571428572, 1e-15)]
        [TestCase(0.1, -1.1304642857142857143, 1e-15)]
        [TestCase(0.4, -1.1040000000000000000, 1e-15)]
        [TestCase(1.2, .4148571428571428571, 1e-15)]
        [TestCase(10.0, -608.14285714285714286, 1e-12)]
        [TestCase(-10.0, 1330.1428571428571429, 1e-12)]
        public void FixedFirstDerivativeFitsAtArbitraryPointsWithMaple(double t, double x, double maxAbsoluteError)
        {
            IInterpolation interpolation = new CubicSplineInterpolation(_t, _x, SplineBoundaryCondition.FirstDerivative, 1.0, SplineBoundaryCondition.FirstDerivative, -1.0);

            Assert.AreEqual(x, interpolation.Interpolate(t), maxAbsoluteError, "Interpolation at {0}", t);

            double interpolatedValue;
            double secondDerivative;
            interpolation.Differentiate(t, out interpolatedValue, out secondDerivative);
            Assert.AreEqual(x, interpolatedValue, maxAbsoluteError, "Interpolation as by-product of differentiation at {0}", t);
        }

        /// <summary>
        /// Verifies that the interpolation matches the given value at all the provided sample points.
        /// </summary>
        [Test]
        public void FixedSecondDerivativeFitsAtSamplePoints()
        {
            IInterpolation interpolation = new CubicSplineInterpolation(_t, _x, SplineBoundaryCondition.SecondDerivative, -5.0, SplineBoundaryCondition.SecondDerivative, -1.0);

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
        /// <param name="t">Sample point.</param>
        /// <param name="x">Sample value.</param>
        /// <param name="maxAbsoluteError">Maximum absolute error.</param>
        /// <remarks>
        /// Maple:
        /// with(CurveFitting);
        /// evalf(subs({x=-2.4},Spline([[-2,1],[-1,2],[0,-1],[1,0],[2,1]], x, degree=3, endpoints=Matrix(2,13,{(1,3)=1,(1,13)=-5,(2,10)=1,(2,13)=-1}))),20);
        /// </remarks>
        [TestCase(-2.4, -.8999999999999999993, 1e-15)]
        [TestCase(-0.9, 1.7590357142857142857, 1e-15)]
        [TestCase(-0.5, .41517857142857142854, 1e-15)]
        [TestCase(-0.1, -.82010714285714285714, 1e-15)]
        [TestCase(0.1, -1.1026071428571428572, 1e-15)]
        [TestCase(0.4, -1.0211428571428571429, 1e-15)]
        [TestCase(1.2, .31771428571428571421, 1e-15)]
        [TestCase(10.0, 39, 1e-13)]
        [TestCase(-10.0, -37, 1e-12)]
        public void FixedSecondDerivativeFitsAtArbitraryPointsWithMaple(double t, double x, double maxAbsoluteError)
        {
            IInterpolation interpolation = new CubicSplineInterpolation(_t, _x, SplineBoundaryCondition.SecondDerivative, -5.0, SplineBoundaryCondition.SecondDerivative, -1.0);

            Assert.AreEqual(x, interpolation.Interpolate(t), maxAbsoluteError, "Interpolation at {0}", t);

            double interpolatedValue;
            double secondDerivative;
            interpolation.Differentiate(t, out interpolatedValue, out secondDerivative);
            Assert.AreEqual(x, interpolatedValue, maxAbsoluteError, "Interpolation as by-product of differentiation at {0}", t);
        }

        /// <summary>
        /// Verifies that the interpolation supports the linear case appropriately
        /// </summary>
        /// <param name="samples">Samples array.</param>
        [TestCase(2)]
        [TestCase(4)]
        [TestCase(12)]
        public void NaturalSupportsLinearCase(int samples)
        {
            double[] x, y, xtest, ytest;
            LinearInterpolationCase.Build(out x, out y, out xtest, out ytest, samples);
            IInterpolation interpolation = new CubicSplineInterpolation(x, y);
            for (int i = 0; i < xtest.Length; i++)
            {
                Assert.AreEqual(ytest[i], interpolation.Interpolate(xtest[i]), 1e-15, "Linear with {0} samples, sample {1}", samples, i);
            }
        }

        /// <summary>
        /// Verifies that sample points are required to be sorted in strictly monotonically ascending order.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void Constructor_SamplePointsNotStrictlyAscending_Throws()
        {
            var x = new[] { -1.0, 0.0, 1.5, 1.5, 2.5, 4.0 };
            var y = new[] { 1.0, 0.3, -0.7, -0.6, -0.1, 0.4 };
            var interpolation = new CubicSplineInterpolation(x, y);
        }
    }
}
