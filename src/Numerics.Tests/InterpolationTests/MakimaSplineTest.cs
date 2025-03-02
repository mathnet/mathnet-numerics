// <copyright file="MakimaSplineTest.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// https://numerics.mathdotnet.com
// https://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-$CURRENT_YEAR$ Math.NET
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
    public class MakimaSplineTest
    {
        // Test sample data
        readonly double[] _t = { -2.0, -1.0, 0.0, 1.0, 2.0 };
        readonly double[] _y = { 1.0, 2.0, -1.0, 0.0, 1.0 };

        /// <summary>
        /// Verifies that the Makima interpolation exactly fits the sample points.
        /// </summary>
        [Test]
        public void FitsAtSamplePoints()
        {
            IInterpolation it = CubicSpline.InterpolateMakima(_t, _y);
            for (var i = 0; i < _y.Length; i++)
            {
                Assert.AreEqual(_y[i], it.Interpolate(_t[i]), 1e-15, "Exact point " + i);
            }
        }

        /// <summary>
        /// Verifies that at arbitrary points, the interpolation matches the reference values.
        /// (Note: Replace the expected values with verified reference values.)
        /// </summary>
        /// <param name="t">The x-value to interpolate.</param>
        /// <param name="expected">The expected interpolated value.</param>
        /// <param name="maxAbsoluteError">Maximum allowed absolute error.</param>
        [TestCase(-2.4, 0.14266666666666683, 1e-10)]
        [TestCase(-0.9, 1.805, 1e-10)]
        [TestCase(-0.5, 0.29166666666666674, 1e-10)]
        [TestCase(-0.1, -0.955, 1e-10)]
        [TestCase(0.1, -0.954, 1e-10)]
        [TestCase(0.4, -0.696, 1e-10)]
        [TestCase(1.2, 0.2, 1e-10)]
        [TestCase(10.0, 9.0, 1e-10)]
        [TestCase(-10.0, 527.0, 1e-10)]
        public void FitsAtArbitraryPoints(double t, double expected, double maxAbsoluteError)
        {
            IInterpolation it = CubicSpline.InterpolateMakima(_t, _y);
            Assert.AreEqual(expected, it.Interpolate(t), maxAbsoluteError, "Interpolation at {0}", t);
        }

        /// <summary>
        /// Verifies that Makima interpolation handles linear data correctly.
        /// </summary>
        /// <param name="samples">The number of sample points.</param>
        [TestCase(5)]
        [TestCase(7)]
        [TestCase(15)]
        public void SupportsLinearCase(int samples)
        {
            double[] x, y, xtest, ytest;
            // LinearInterpolationCase.Build is a utility to generate linear test data.
            LinearInterpolationCase.Build(out x, out y, out xtest, out ytest, samples);
            IInterpolation it = CubicSpline.InterpolateMakima(x, y);
            for (var i = 0; i < xtest.Length; i++)
            {
                Assert.AreEqual(ytest[i], it.Interpolate(xtest[i]), 1e-15, "Linear case with {0} samples, sample {1}", samples, i);
            }
        }

        /// <summary>
        /// Verifies that the interpolation throws an exception when provided with too few samples.
        /// </summary>
        [Test]
        public void FewSamples()
        {
            Assert.That(() => CubicSpline.InterpolateMakima(new double[0], new double[0]), Throws.ArgumentException);
            Assert.That(() => CubicSpline.InterpolateMakima(new double[4], new double[4]), Throws.ArgumentException);
            Assert.That(CubicSpline.InterpolateMakima(new[] { 1.0, 2.0, 3.0, 4.0, 5.0 },
                                                      new[] { 2.0, 2.0, 2.0, 2.0, 2.0 })
                          .Interpolate(1.0), Is.EqualTo(2.0));
        }
    }
}
