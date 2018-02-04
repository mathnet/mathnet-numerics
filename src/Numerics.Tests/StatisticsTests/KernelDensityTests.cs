// <copyright file="KernelDensityEstimatorTests.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2018 Math.NET
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
using NUnit.Framework;
using MathNet.Numerics.Statistics;

namespace MathNet.Numerics.UnitTests.StatisticsTests
{
    /// <summary>
    /// Kernel Density Estimator tests.
    /// </summary>
    [TestFixture, Category("Statistics")]
    public class KernelDensityTests
    {
        private readonly double[] _testData =
        {
            0.899822328897223,
            -0.300111005615676,
            1.029365712103099,
            -0.345065971567321,
            1.012801864262980,
            0.629334584931419,
            -0.213015082641055,
            -0.865697308360524,
            -1.043108301337627,
            -0.270068812648099
        };

        [Test]
        public void KDETestGaussianKernelBandwidth1()
        {
            //Density of standard normal distribution at 0
            AssertHelpers.AlmostEqualRelative(0.398942280401433, KernelDensity.GaussianKernel(0), 10);

            var estimate = KernelDensity.EstimateGaussian(-3.5d, 1.0d, _testData);
            AssertHelpers.AlmostEqualRelative(0.004115405028907, estimate, 10);

            estimate = KernelDensity.EstimateGaussian(0.0d, 1.0d, _testData);
            AssertHelpers.AlmostEqualRelative(0.310485907659139, estimate, 10);

            estimate = KernelDensity.EstimateGaussian(2.0d, 1.0d, _testData);
            AssertHelpers.AlmostEqualRelative(0.099698581377801, estimate, 10);
        }

        [Test]
        public void KDETestTriangularKernelBandwidth1()
        {
            Assert.AreEqual(1.0d, KernelDensity.TriangularKernel(0));

            var estimate = KernelDensity.EstimateTriangular(-3.5d, 1.0d, _testData);
            AssertHelpers.AlmostEqualRelative(0, estimate, 10);

            estimate = KernelDensity.EstimateTriangular(0.0d, 1.0d, _testData);
            AssertHelpers.AlmostEqualRelative(0.347688490533868, estimate, 10);

            estimate = KernelDensity.EstimateTriangular(2.0d, 1.0d, _testData);
            AssertHelpers.AlmostEqualRelative(0.004216757636608, estimate, 10);
        }

        [Test]
        public void KDETestUniformKernelBandwidth1()
        {
            Assert.AreEqual(0.5d, KernelDensity.UniformKernel(0));

            var estimate = KernelDensity.EstimateUniform(-3.5d, 1.0d, _testData);
            AssertHelpers.AlmostEqualRelative(0, estimate, 10);

            estimate = KernelDensity.EstimateUniform(0.0d, 1.0d, _testData);
            AssertHelpers.AlmostEqualRelative(0.35, estimate, 10);

            estimate = KernelDensity.EstimateUniform(2.0d, 1.0d, _testData);
            AssertHelpers.AlmostEqualRelative(0.1, estimate, 10);
        }

        [Test]
        public void KDETestEpanechnikovKernelBandwidth1()
        {
            Assert.AreEqual(0.75d, KernelDensity.EpanechnikovKernel(0));

            var estimate = KernelDensity.EstimateEpanechnikov(-3.5d, 1.0d, _testData);
            AssertHelpers.AlmostEqualRelative(0, estimate, 10);

            estimate = KernelDensity.EstimateEpanechnikov(0.0d, 1.0d, _testData);
            AssertHelpers.AlmostEqualRelative(0.353803214812608, estimate, 10);

            estimate = KernelDensity.EstimateEpanechnikov(2.0d, 1.0d, _testData);
            AssertHelpers.AlmostEqualRelative(0.006248168996717, estimate, 10);
        }

        [Test]
        public void KDETestGaussianKernelBandwidth0p5()
        {
            var estimate = KernelDensity.EstimateGaussian(-3.5d, 0.5d, _testData);
            AssertHelpers.AlmostEqualRelative(5.311490430807364e-007, estimate, 10);

            estimate = KernelDensity.EstimateGaussian(0.0d, 0.5d, _testData);
            AssertHelpers.AlmostEqualRelative(0.369994803886827, estimate, 10);

            estimate = KernelDensity.EstimateGaussian(2.0d, 0.5d, _testData);
            AssertHelpers.AlmostEqualRelative(0.032447347007482, estimate, 10);
        }

        [Test]
        public void KDETestGaussianKernelBandwidth2()
        {
            var estimate = KernelDensity.EstimateGaussian(-3.5d, 2.0d, _testData);
            AssertHelpers.AlmostEqualRelative(0.046875864115900, estimate, 10);

            estimate = KernelDensity.EstimateGaussian(0.0d, 2.0d, _testData);
            AssertHelpers.AlmostEqualRelative(0.186580447512078, estimate, 10);

            estimate = KernelDensity.EstimateGaussian(2.0d, 2.0d, _testData);
            AssertHelpers.AlmostEqualRelative(0.123339405007761, estimate, 10);
        }

        [Test]
        public void KDETestCustomKernelBandwidth1()
        {
            double Kernel(double x) => 0.5d * Math.Exp(-Math.Abs(x));
            Assert.AreEqual(0.5d, Kernel(0));

            var estimate = KernelDensity.Estimate(-3.5d, 1.0d, _testData, Kernel);
            AssertHelpers.AlmostEqualRelative(0.018396636706009, estimate, 10);

            estimate = KernelDensity.Estimate(0.0d, 1.0d, _testData, Kernel);
            AssertHelpers.AlmostEqualRelative(0.272675897096678, estimate, 10);

            estimate = KernelDensity.Estimate(2.0d, 1.0d, _testData, Kernel);
            AssertHelpers.AlmostEqualRelative(0.092580285110347, estimate, 10);
        }
    }
}
