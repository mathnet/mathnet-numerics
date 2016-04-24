// <copyright file="NevillePolynomialTest.cs" company="Math.NET">
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
using System.Globalization;
using System.Linq;
using MathNet.Numerics.Interpolation;
using MathNet.Numerics.TestData;
using NUnit.Framework;

namespace MathNet.Numerics.UnitTests.InterpolationTests
{
    /// <summary>
    /// NevillePolynomial test case.
    /// </summary>
    [TestFixture, Category("Interpolation")]
    public class NevillePolynomialTest
    {
        /// <summary>
        /// Sample points.
        /// </summary>
        readonly double[] _t = { 0.0, 1.0, 3.0, 4.0 };

        /// <summary>
        /// Sample values.
        /// </summary>
        readonly double[] _x = { 0.0, 3.0, 1.0, 3.0 };

        /// <summary>
        /// Verifies that the interpolation matches the given value at all the provided sample points.
        /// </summary>
        [Test]
        public void FitsAtSamplePoints()
        {
            IInterpolation interpolation = new NevillePolynomialInterpolation(_t, _x);

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
        /// evalf(subs({x=0.1},PolynomialInterpolation([[0,0],[1,3],[3,1],[4,3]], x)),20);
        /// </remarks>
        [TestCase(0.1, .57225, 1e-15)]
        [TestCase(0.4, 1.884, 1e-15)]
        [TestCase(1.1, 3.0314166666666666667, 1e-15)]
        [TestCase(3.2, 1.034666666666666667, 1e-15)]
        [TestCase(4.5, 6.28125, 1e-15)]
        [TestCase(10.0, 277.5, 1e-15)]
        [TestCase(-10.0, -1010.8333333333333333, 1e-12)]
        public void FitsAtArbitraryPointsWithMaple(double t, double x, double maxAbsoluteError)
        {
            IInterpolation interpolation = new NevillePolynomialInterpolation(_t, _x);

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
            IInterpolation interpolation = new NevillePolynomialInterpolation(x, y);
            for (int i = 0; i < xtest.Length; i++)
            {
                Assert.AreEqual(ytest[i], interpolation.Interpolate(xtest[i]), 1e-12, "Linear with {0} samples, sample {1}", samples, i);
            }
        }

        /// <summary>
        /// Verifies that all sample points must be unique.
        /// </summary>
        [Test]
        public void Constructor_SamplePointsNotUnique_Throws()
        {
            var x = new[] { -1.0, 0.0, 1.5, 1.5, 2.5, 4.0 };
            var y = new[] { 1.0, 0.3, -0.7, -0.6, -0.1, 0.4 };
            Assert.Throws<ArgumentException>(() => new NevillePolynomialInterpolation(x, y));
        }

        /// <summary>
        /// Verifies that interpolation does not yield NaN values for a case where some sample points
        /// has an upper and a lower sample value.
        /// </summary>
        /// <param name="value">Value for which interpolation is requested.</param>
        /// <remarks>Columns 3 and 4 of the .csv file contain the logarithms of energy and mass attenuation
        /// coefficients, respectively.</remarks>
        [Test, Sequential, Ignore("")]
        public void Interpolate_LogLogAttenuationData_InterpolationShouldNotYieldNaN(
            [Values(0.0025, 0.035, 0.45, 5.5, 18.5, 35.0)] double value)
        {
            var data = Data.ReadAllLines(@"Github-Cureos-1.csv")
                .Select(line =>
                {
                    var vals = line.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    return new Tuple<string, string>(vals[2], vals[3]);
                }).ToArray();
            var x = data.Select(tuple => Double.Parse(tuple.Item1, CultureInfo.InvariantCulture)).ToArray();
            var y = data.Select(tuple => Double.Parse(tuple.Item2, CultureInfo.InvariantCulture)).ToArray();
            IInterpolation interpolation = new NevillePolynomialInterpolation(x, y);

            var actual = interpolation.Interpolate(Math.Log(value));
            Assert.That(actual, Is.Not.NaN);
        }

        [Test]
        public void FewSamples()
        {
            Assert.That(() => NevillePolynomialInterpolation.Interpolate(new double[0], new double[0]), Throws.ArgumentException);
            Assert.That(NevillePolynomialInterpolation.Interpolate(new[] { 1.0 }, new[] { 2.0 }).Interpolate(1.0), Is.EqualTo(2.0));
        }
    }
}
