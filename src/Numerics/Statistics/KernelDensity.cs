// <copyright file="KernelDensityEstimator.cs" company="Math.NET">
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
using System.Collections.Generic;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.Threading;

namespace MathNet.Numerics.Statistics
{
    /// <summary>
    /// Kernel density estimation (KDE).
    /// </summary>
    public static class KernelDensity
    {
        /// <summary>
        /// Estimate the probability density function of a random variable.
        /// </summary>
        /// <remarks>
        /// The routine assumes that the provided kernel is well defined, i.e. a real non-negative function that integrates to 1.
        /// </remarks>
        public static double Estimate(double x, double bandwidth, IList<double> samples, Func<double, double> kernel)
        {
            if (bandwidth <= 0)
            {
                throw new ArgumentException("The bandwidth must be a positive number!");
            }

            var n = samples.Count;
            var estimate = CommonParallel.Aggregate(0, n,
                               i => kernel((x - samples[i]) / bandwidth),
                               (a, b) => a + b,
                               0d) / (n * bandwidth);

            return estimate;
        }

        /// <summary>
        /// Estimate the probability density function of a random variable with a Gaussian kernel.
        /// </summary>
        public static double EstimateGaussian(double x, double bandwidth, IList<double> samples)
        {
            return Estimate(x, bandwidth, samples, GaussianKernel);
        }

        /// <summary>
        /// Estimate the probability density function of a random variable with an Epanechnikov kernel.
        /// The Epanechnikov kernel is optimal in a mean square error sense.
        /// </summary>
        public static double EstimateEpanechnikov(double x, double bandwidth, IList<double> samples)
        {
            return Estimate(x, bandwidth, samples, EpanechnikovKernel);
        }

        /// <summary>
        /// Estimate the probability density function of a random variable with a uniform kernel.
        /// </summary>
        public static double EstimateUniform(double x, double bandwidth, IList<double> samples)
        {
            return Estimate(x, bandwidth, samples, UniformKernel);
        }

        /// <summary>
        /// Estimate the probability density function of a random variable with a triangular kernel.
        /// </summary>
        public static double EstimateTriangular(double x, double bandwidth, IList<double> samples)
        {
            return Estimate(x, bandwidth, samples, TriangularKernel);
        }

        /// <summary>
        /// A Gaussian kernel (PDF of Normal distribution with mean 0 and variance 1).
        /// This kernel is the default.
        /// </summary>
        public static double GaussianKernel(double x)
        {
            return Normal.PDF(0.0, 1.0, x);
        }

        /// <summary>
        /// Epanechnikov Kernel:
        /// x =&gt; Math.Abs(x) &lt;= 1.0 ? 3.0/4.0(1.0-x^2) : 0.0
        /// </summary>
        public static double EpanechnikovKernel(double x)
        {
            return Math.Abs(x) <= 1.0 ? 0.75 * (1 - x * x) : 0.0;
        }

        /// <summary>
        /// Uniform Kernel:
        /// x =&gt; Math.Abs(x) &lt;= 1.0 ? 1.0/2.0 : 0.0
        /// </summary>
        public static double UniformKernel(double x)
        {
            return ContinuousUniform.PDF(-1.0, 1.0, x);
        }

        /// <summary>
        /// Triangular Kernel:
        /// x =&gt; Math.Abs(x) &lt;= 1.0 ? (1.0-Math.Abs(x)) : 0.0
        /// </summary>
        public static double TriangularKernel(double x)
        {
            return Triangular.PDF(-1.0, 1.0, 0.0, x);
        }
    }
}
