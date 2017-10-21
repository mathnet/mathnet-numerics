// <copyright file="KernelDensityEstimatorTests.cs" company="Math.NET">
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using MathNet.Numerics.Statistics;

namespace MathNet.Numerics.UnitTests.StatisticsTests
{
  /// <summary>
  /// Kernel Density Estimator tests.
  /// </summary>
  [TestFixture, Category("Statistics")]
  public class KernelDensityEstimatorTests
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
      var kde = new KernelDensityEstimator(_testData);

      Assert.AreEqual(KDEKernelType.Gaussian, kde.KernelType);
      Assert.AreEqual(1.0d, kde.Bandwidth);
      AssertHelpers.AlmostEqualRelative(0.398942280401433, kde.Kernel(0), 10); //Density of standard normal distribution at 0

      var estimate = kde.EstimateDensity(-3.5);
      AssertHelpers.AlmostEqualRelative(0.004115405028907, estimate, 10);

      estimate = kde.EstimateDensity(0);
      AssertHelpers.AlmostEqualRelative(0.310485907659139, estimate, 10);

      estimate = kde.EstimateDensity(2);
      AssertHelpers.AlmostEqualRelative(0.099698581377801, estimate, 10);
    }

    [Test]
    public void KDETestTriangularKernelBandwidth1()
    {
      var kde = new KernelDensityEstimator(_testData);
      kde.KernelType = KDEKernelType.Triangular;

      Assert.AreEqual(KDEKernelType.Triangular, kde.KernelType);
      Assert.AreEqual(1.0d, kde.Bandwidth);
      Assert.AreEqual(1.0d, kde.Kernel(0)); //Density of standard normal distribution at 0

      var estimate = kde.EstimateDensity(-3.5);
      AssertHelpers.AlmostEqualRelative(0, estimate, 10);

      estimate = kde.EstimateDensity(0);
      AssertHelpers.AlmostEqualRelative(0.347688490533868, estimate, 10);

      estimate = kde.EstimateDensity(2);
      AssertHelpers.AlmostEqualRelative(0.004216757636608, estimate, 10);
    }

    [Test]
    public void KDETestUniformKernelBandwidth1()
    {
      var kde = new KernelDensityEstimator(_testData);
      kde.KernelType = KDEKernelType.Uniform;

      Assert.AreEqual(KDEKernelType.Uniform, kde.KernelType);
      Assert.AreEqual(1.0d, kde.Bandwidth);
      Assert.AreEqual(0.5d, kde.Kernel(0));

      var estimate = kde.EstimateDensity(-3.5);
      AssertHelpers.AlmostEqualRelative(0, estimate, 10);

      estimate = kde.EstimateDensity(0);
      AssertHelpers.AlmostEqualRelative(0.35, estimate, 10);

      estimate = kde.EstimateDensity(2);
      AssertHelpers.AlmostEqualRelative(0.1, estimate, 10);
    }

    [Test]
    public void KDETestEpanechnikovKernelBandwidth1()
    {
      var kde = new KernelDensityEstimator(_testData);
      kde.KernelType = KDEKernelType.Epanechnikov;

      Assert.AreEqual(KDEKernelType.Epanechnikov, kde.KernelType);
      Assert.AreEqual(1.0d, kde.Bandwidth);
      Assert.AreEqual(0.75d, kde.Kernel(0));

      var estimate = kde.EstimateDensity(-3.5);
      AssertHelpers.AlmostEqualRelative(0, estimate, 10);

      estimate = kde.EstimateDensity(0);
      AssertHelpers.AlmostEqualRelative(0.353803214812608, estimate, 10);

      estimate = kde.EstimateDensity(2);
      AssertHelpers.AlmostEqualRelative(0.006248168996717, estimate, 10);
    }

    [Test]
    public void KDETestGaussianKernelBandwidth0p5()
    {
      var kde = new KernelDensityEstimator(_testData);
      kde.Bandwidth = 0.5d;

      var estimate = kde.EstimateDensity(-3.5);
      AssertHelpers.AlmostEqualRelative(5.311490430807364e-007, estimate, 10);

      estimate = kde.EstimateDensity(0);
      AssertHelpers.AlmostEqualRelative(0.369994803886827, estimate, 10);

      estimate = kde.EstimateDensity(2);
      AssertHelpers.AlmostEqualRelative(0.032447347007482, estimate, 10);
    }

    [Test]
    public void KDETestGaussianKernelBandwidth2()
    {
      var kde = new KernelDensityEstimator(_testData);
      kde.Bandwidth = 2.0d;

      var estimate = kde.EstimateDensity(-3.5);
      AssertHelpers.AlmostEqualRelative(0.046875864115900, estimate, 10);

      estimate = kde.EstimateDensity(0);
      AssertHelpers.AlmostEqualRelative(0.186580447512078, estimate, 10);

      estimate = kde.EstimateDensity(2);
      AssertHelpers.AlmostEqualRelative(0.123339405007761, estimate, 10);
    }

    [Test]
    public void KDETestCustomKernelBandwidth1()
    {
      var kde = new KernelDensityEstimator(_testData);
      kde.Bandwidth = 1.0d;
      kde.Kernel = x => 0.5d * Math.Exp(-Math.Abs(x)); //Picard-Kernel

      Assert.AreEqual(KDEKernelType.Custom, kde.KernelType);
      Assert.AreEqual(1.0d, kde.Bandwidth);
      Assert.AreEqual(0.5d, kde.Kernel(0));

      var estimate = kde.EstimateDensity(-3.5);
      AssertHelpers.AlmostEqualRelative(0.018396636706009, estimate, 10);

      estimate = kde.EstimateDensity(0);
      AssertHelpers.AlmostEqualRelative(0.272675897096678, estimate, 10);

      estimate = kde.EstimateDensity(2);
      AssertHelpers.AlmostEqualRelative(0.092580285110347, estimate, 10);
    }
  }
}
