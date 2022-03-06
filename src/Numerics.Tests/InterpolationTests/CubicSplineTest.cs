// <copyright file="CubicSplineTest.cs" company="Math.NET">
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
using System.Linq;
using MathNet.Numerics.Interpolation;
using NUnit.Framework;

namespace MathNet.Numerics.Tests.InterpolationTests
{
    [TestFixture, Category("Interpolation")]
    public class CubicSplineTest
    {
        readonly double[] _t = { -2.0, -1.0, 0.0, 1.0, 2.0 };
        readonly double[] _y = { 1.0, 2.0, -1.0, 0.0, 1.0 };
        //test data for min max values
        readonly double[] _x = { -4, -3, -2, -1, 0, 1, 2, 3, 4 };
        readonly double[] _z = { -7, 2, 5, 0, -3, -1, -4, 0, 6 };

        /// <summary>
        /// Verifies that the interpolation matches the given value at all the provided sample points.
        /// </summary>
        [Test]
        public void NaturalFitsAtSamplePoints()
        {
            IInterpolation it = CubicSpline.InterpolateNatural(_t, _y);
            for (int i = 0; i < _y.Length; i++)
            {
                Assert.AreEqual(_y[i], it.Interpolate(_t[i]), "A Exact Point " + i);
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
        public void NaturalFitsAtArbitraryPoints(double t, double x, double maxAbsoluteError)
        {
            IInterpolation it = CubicSpline.InterpolateNatural(_t, _y);
            Assert.AreEqual(x, it.Interpolate(t), maxAbsoluteError, "Interpolation at {0}", t);
        }

        /// <summary>
        /// Verifies that the interpolation matches the given value at all the provided sample points.
        /// </summary>
        [Test]
        public void FixedFirstDerivativeFitsAtSamplePoints()
        {
            IInterpolation it = CubicSpline.InterpolateBoundaries(_t, _y, SplineBoundaryCondition.FirstDerivative, 1.0, SplineBoundaryCondition.FirstDerivative, -1.0);
            for (int i = 0; i < _y.Length; i++)
            {
                Assert.AreEqual(_y[i], it.Interpolate(_t[i]), "A Exact Point " + i);
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
        public void FixedFirstDerivativeFitsAtArbitraryPoints(double t, double x, double maxAbsoluteError)
        {
            IInterpolation it = CubicSpline.InterpolateBoundaries(_t, _y, SplineBoundaryCondition.FirstDerivative, 1.0, SplineBoundaryCondition.FirstDerivative, -1.0);
            Assert.AreEqual(x, it.Interpolate(t), maxAbsoluteError, "Interpolation at {0}", t);
        }

        /// <summary>
        /// Verifies that the interpolation matches the given value at all the provided sample points.
        /// </summary>
        [Test]
        public void FixedSecondDerivativeFitsAtSamplePoints()
        {
            IInterpolation it = CubicSpline.InterpolateBoundaries(_t, _y, SplineBoundaryCondition.SecondDerivative, -5.0, SplineBoundaryCondition.SecondDerivative, -1.0);
            for (int i = 0; i < _y.Length; i++)
            {
                Assert.AreEqual(_y[i], it.Interpolate(_t[i]), "A Exact Point " + i);
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
        public void FixedSecondDerivativeFitsAtArbitraryPoints(double t, double x, double maxAbsoluteError)
        {
            IInterpolation it = CubicSpline.InterpolateBoundaries(_t, _y, SplineBoundaryCondition.SecondDerivative, -5.0, SplineBoundaryCondition.SecondDerivative, -1.0);
            Assert.AreEqual(x, it.Interpolate(t), maxAbsoluteError, "Interpolation at {0}", t);
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
            LinearInterpolationCase.Build(out var x, out var y, out var xtest, out var ytest, samples);
            IInterpolation it = CubicSpline.InterpolateNatural(x, y);
            for (int i = 0; i < xtest.Length; i++)
            {
                Assert.AreEqual(ytest[i], it.Interpolate(xtest[i]), 1e-15, "Linear with {0} samples, sample {1}", samples, i);
            }
        }

        [Test]
        public void FewSamples()
        {
            Assert.That(() => CubicSpline.InterpolateNatural(Array.Empty<double>(), Array.Empty<double>()), Throws.ArgumentException);
            Assert.That(() => CubicSpline.InterpolateNatural(new double[1], new double[1]), Throws.ArgumentException);
            Assert.That(CubicSpline.InterpolateNatural(new[] { 1.0, 2.0 }, new[] { 2.0, 2.0 }).Interpolate(1.0), Is.EqualTo(2.0));
        }

        [Test]
        public void InterpolateAkimaSorted_MustBeThreadSafe_GitHub219([Values(8, 32, 256, 1024)] int samples)
        {
            var x = Generate.LinearSpaced(samples + 1, 0.0, 2.0*Math.PI);
            var y = new double[samples][];
            for (var i = 0; i < samples; ++i)
            {
                y[i] = x.Select(xx => Math.Sin(xx)/(i + 1)).ToArray();
            }

            var yipol = new double[samples];
            System.Threading.Tasks.Parallel.For(0, samples, i =>
            {
                var spline = CubicSpline.InterpolateAkimaSorted(x, y[i]);
                yipol[i] = spline.Interpolate(1.0);
            });

            CollectionAssert.DoesNotContain(yipol, double.NaN);
        }

        /// <summary>
        /// Tests that the cubic spline returns the correct t values where the derivative is 0
        /// </summary>
        [Test]
        public void NaturalSplineGetHorizontalDerivativeTValues()
        {
            CubicSpline it = CubicSpline.InterpolateBoundaries(_x, _z, SplineBoundaryCondition.SecondDerivative, -4.0, SplineBoundaryCondition.SecondDerivative, 4.0);
            var horizontalDerivatives = it.StationaryPoints();
            //readonly double[] _x = { -4, -3, -2, -1,  0,  1,  2, 3, 4 };
            //readonly double[] _z = { -7,  2,  5,  0, -3, -1, -4, 0, 6 };
            Assert.AreEqual(4, horizontalDerivatives.Length, "Incorrect number of points with derivative value equal to 0");
            Assert.IsTrue(horizontalDerivatives[0]>=-3 && horizontalDerivatives[0] <= -2,"Spline returns wrong t value: "+horizontalDerivatives[0]+" for first point");
            Assert.IsTrue(horizontalDerivatives[1] >= -1 && horizontalDerivatives[1] <= 0, "Spline returns wrong t value: " + horizontalDerivatives[1] + " for second point");
            Assert.IsTrue(horizontalDerivatives[2] >= 0 && horizontalDerivatives[2] <= 1, "Spline returns wrong t value: " + horizontalDerivatives[2] + " for third point");
            Assert.IsTrue(horizontalDerivatives[3] >= 2 && horizontalDerivatives[3] <= 3, "Spline returns wrong t value: " + horizontalDerivatives[3] + " for fourth point");
            Console.WriteLine("GetHorizontalDerivativeTValues checked out ok for cubic spline.");
         }

        /// <summary>
        /// Tests that the min and max values for the natural spline are correct
        /// </summary>
        [Test]
        public void NaturalSplineGetMinMaxTvalues()
        {
            CubicSpline it = CubicSpline.InterpolateBoundaries(_x, _z, SplineBoundaryCondition.SecondDerivative, -4.0, SplineBoundaryCondition.SecondDerivative, 4.0);
            var minMax = it.Extrema();
            Assert.AreEqual(-4, minMax.Item1, "Spline returns wrong t value for global minimum");
            Assert.AreEqual(4, minMax.Item2, "Spline returns wrong t value for global maximum");
            Console.WriteLine("GetMinMaxTValues checked out ok for cubic spline.");
        }

        /// <summary>
        /// This tests that the min and max values for a natural spline interpolated on an oscilating decaying function are correct and
        /// spints out the time it takes to do the calculation for 100 thousand points
        /// </summary>
        [Test]
        public void CheckNaturalSplineMinMaxValuesPerformance()
        {
            //first generate the test data and spline
            double amplitude = 100;
            double ofset = 0;
            double period = 10;
            double decay = 0.1;
            double minX = -50;
            double maxX = 50;
            int points = 100000;
            var data = GenerateSplineData(amplitude, period, decay, minX, maxX, ofset, points);
            CubicSpline it = CubicSpline.InterpolateBoundaries(data.Item1, data.Item2, SplineBoundaryCondition.Natural, 0, SplineBoundaryCondition.Natural, 0);
            //start the time and calculate the min max values
            var t = DateTime.Now;
            var minMax = it.Extrema();
            Assert.IsTrue(minMax.Item2.AlmostEqual(ofset, 0.3), "Expexted max value near ofset.");
            Assert.IsTrue(minMax.Item1.AlmostEqual(ofset+period/2, 0.3) || minMax.Item1.AlmostEqual(ofset - period / 2, 0.3), "Expexted min value near ofset +- period/2.");
            //spit out the time it took to calculate
            Console.WriteLine("Extrema took: " + (DateTime.Now - t).TotalMilliseconds.ToString("000.00")+" ms for "+points.ToString()+" points.");
            //determine if the values are correct
            var sp = it.StationaryPoints();
            foreach (var x in sp)
            {
                //check that the stationary point falls roughly at a half period
                Assert.IsTrue(Math.Abs((Math.Abs((x - ofset) * 2 / period) - Math.Round(Math.Abs(x - ofset) * 2 / period, 0))).AlmostEqual(0,0.3), "Stationary point found outside of period/2 for x="+x.ToString());
            }
        }

        /// <summary>
        /// Generates a set of points representing an oscilating decaying function
        /// </summary>
        /// <param name="amplitude">The max amplitude</param>
        /// <param name="period">The period of oscilation</param>
        /// <param name="decay">The decaying exponent, the larger the value the faster the functioon decays</param>
        /// <param name="minX">The min value for X</param>
        /// <param name="maxX">The max value for X</param>
        /// <param name="ofset">The x - ofset of the max value of the function</param>
        /// <param name="points">The number of points to generate, must be greater than 1</param>
        /// <returns></returns>
        private Tuple<double[],double[]> GenerateSplineData(double amplitude, double period, double decay, double minX, double maxX,  double ofset, int points)
        {
            double delta = (maxX-minX)/(points-1);
            double[] _x = new double[points];
            double[] _y = new double[points];
            for (int i = 0; i < points; i++)
            {
                double x = i * delta + minX;
                double y = amplitude * Math.Cos((x - ofset) * 2 * Math.PI / period) * Math.Exp(-Math.Abs((x - ofset) * decay));
                _x[i] = x;
                _y[i] = y;
            }
            return Tuple.Create(_x, _y);
        }

    }
}
