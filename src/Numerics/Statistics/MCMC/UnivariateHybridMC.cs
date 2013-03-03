// <copyright file="UnivariateHybridMC.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
//
// Copyright (c) 2009-2010 Math.NET
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

namespace MathNet.Numerics.Statistics.Mcmc
{
    using System;
    using Distributions;

    /// <summary>
    /// A hybrid Monte Carlo sampler for univariate distributions.
    /// </summary>
    public class UnivariateHybridMC : HybridMCGeneric<double>
    {
        /// <summary>
        /// Distribution to sample momentum from.
        /// </summary>
        private readonly Normal _distribution;

        /// <summary>
        /// Standard deviations used in the sampling of the
        /// momentum.
        /// </summary>
        private double _sdv;

        /// <summary>
        /// Gets or sets the standard deviation used in the sampling of the
        /// momentum.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">When standard deviation is negative.</exception>
        public double MomentumStdDev
        {
            get { return _sdv; }
            set
            {
                if (_sdv != value)
                {
                    _sdv = SetPositive(value);
                }
            }
        }

        /// <summary>
        /// Constructs a new Hybrid Monte Carlo sampler for a univariate probability distribution.
        /// The burn interval will be set to 0.
        /// The momentum will be sampled from a normal distribution with standard deviation
        /// 1 using the default <see cref="System.Random"/> random
        /// number generator. A three point estimation will be used for differentiation.
        /// </summary>
        /// <param name="x0">The initial sample.</param>
        /// <param name="pdfLnP">The log density of the distribution we want to sample from.</param>
        /// <param name="frogLeapSteps">Number frogleap simulation steps.</param>
        /// <param name="stepSize">Size of the frogleap simulation steps.</param>
        public UnivariateHybridMC(double x0, DensityLn<double> pdfLnP, int frogLeapSteps, double stepSize)
            : this(x0, pdfLnP, frogLeapSteps, stepSize, 0)
        {
        }

        /// <summary>
        /// Constructs a new Hybrid Monte Carlo sampler for a univariate probability distribution.
        /// The momentum will be sampled from a normal distribution with standard deviation
        /// 1 using the default <see cref="System.Random"/> random
        /// number generator. A three point estimation will be used for differentiation.
        /// This constructor will set the burn interval.
        /// </summary>
        /// <param name="x0">The initial sample.</param>
        /// <param name="pdfLnP">The log density of the distribution we want to sample from.</param>
        /// <param name="frogLeapSteps">Number frogleap simulation steps.</param>
        /// <param name="stepSize">Size of the frogleap simulation steps.</param>
        /// <param name="burnInterval">The number of iterations in between returning samples.</param>
        /// <exception cref="ArgumentOutOfRangeException">When the number of burnInterval iteration is negative.</exception>
        public UnivariateHybridMC(double x0, DensityLn<double> pdfLnP, int frogLeapSteps, double stepSize, int burnInterval)
            : this(x0, pdfLnP, frogLeapSteps, stepSize, burnInterval, 1)
        {
        }

        /// <summary>
        /// Constructs a new Hybrid Monte Carlo sampler for a univariate probability distribution.
        /// The momentum will be sampled from a normal distribution with standard deviation
        /// specified by pSdv using the default <see cref="System.Random"/> random
        /// number generator. A three point estimation will be used for differentiation.
        /// This constructor will set the burn interval.
        /// </summary>
        /// <param name="x0">The initial sample.</param>
        /// <param name="pdfLnP">The log density of the distribution we want to sample from.</param>
        /// <param name="frogLeapSteps">Number frogleap simulation steps.</param>
        /// <param name="stepSize">Size of the frogleap simulation steps.</param>
        /// <param name="burnInterval">The number of iterations in between returning samples.</param>
        /// <param name="pSdv">The standard deviation of the normal distribution that is used to sample
        /// the momentum.</param>
        /// <exception cref="ArgumentOutOfRangeException">When the number of burnInterval iteration is negative.</exception>
        public UnivariateHybridMC(double x0, DensityLn<double> pdfLnP, int frogLeapSteps, double stepSize, int burnInterval, double pSdv)
            : this(x0, pdfLnP, frogLeapSteps, stepSize, burnInterval, pSdv, new Random())
        {
        }

        /// <summary>
        /// Constructs a new Hybrid Monte Carlo sampler for a univariate probability distribution.
        /// The momentum will be sampled from a normal distribution with standard deviation
        /// specified by pSdv using a random
        /// number generator provided by the user. A three point estimation will be used for differentiation.
        /// This constructor will set the burn interval.
        /// </summary>
        /// <param name="x0">The initial sample.</param>
        /// <param name="pdfLnP">The log density of the distribution we want to sample from.</param>
        /// <param name="frogLeapSteps">Number frogleap simulation steps.</param>
        /// <param name="stepSize">Size of the frogleap simulation steps.</param>
        /// <param name="burnInterval">The number of iterations in between returning samples.</param>
        /// <param name="pSdv">The standard deviation of the normal distribution that is used to sample
        /// the momentum.</param>
        /// <param name="randomSource">Random number generator used to sample the momentum.</param>
        /// <exception cref="ArgumentOutOfRangeException">When the number of burnInterval iteration is negative.</exception>
        public UnivariateHybridMC(double x0, DensityLn<double> pdfLnP, int frogLeapSteps, double stepSize, int burnInterval, double pSdv, Random randomSource)
            : this(x0, pdfLnP, frogLeapSteps, stepSize, burnInterval, pSdv, randomSource, new DiffMethod(UnivariateHybridMC.Grad))
        {
        }

        /// <summary>
        /// Constructs a new Hybrid Monte Carlo sampler for a multivariate probability distribution.
        /// The momentum will be sampled from a normal distribution with standard deviation
        /// given by pSdv using a random
        /// number generator provided by the user.  This constructor will set both the burn interval and the method used for
        /// numerical differentiation.
        /// </summary>
        /// <param name="x0">The initial sample.</param>
        /// <param name="pdfLnP">The log density of the distribution we want to sample from.</param>
        /// <param name="frogLeapSteps">Number frogleap simulation steps.</param>
        /// <param name="stepSize">Size of the frogleap simulation steps.</param>
        /// <param name="burnInterval">The number of iterations in between returning samples.</param>
        /// <param name="pSdv">The standard deviation of the normal distribution that is used to sample
        /// the momentum.</param>
        /// <param name="diff">The method used for numerical differentiation.</param>
        /// <param name="randomSource">Random number generator used for sampling the momentum.</param>
        /// <exception cref="ArgumentOutOfRangeException">When the number of burnInterval iteration is negative.</exception>
        public UnivariateHybridMC(double x0, DensityLn<double> pdfLnP, int frogLeapSteps, double stepSize, int burnInterval, double pSdv, Random randomSource, DiffMethod diff)
            : base(x0, pdfLnP, frogLeapSteps, stepSize, burnInterval, randomSource, diff)
        {
            MomentumStdDev = pSdv;
            _distribution = new Normal(0, MomentumStdDev) {RandomSource = RandomSource};
            Burn(BurnInterval);

        }

        /// <summary>
        /// Use for copying objects in the Burn method.
        /// </summary>
        /// <param name="source">The source of copying.</param>
        /// <returns>A copy of the source object.</returns>
        protected override double Copy(double source)
        {
            return source;
        }

        /// <summary>
        /// Use for creating temporary objects in the Burn method.
        /// </summary>
        /// <returns>An object of type T.</returns>
        protected override double Create()
        {
            return 0;
        }

        /// <inheritdoc/>
        protected override void DoAdd(ref double first, double factor, double second)
        {
            first += factor * second;
        }

        /// <inheritdoc/>
        protected override double DoProduct(double first, double second)
        {
            return first * second;
        }

        /// <inheritdoc/>
        protected override void DoSubtract(ref double first, double factor, double second)
        {
            first -= factor * second;
        }

        /// <summary>
        /// Samples the momentum from a normal distribution.
        /// </summary>
        /// <param name="p">The momentum to be randomized.</param>
        protected override void RandomizeMomentum(ref double p)
        {
            p = _distribution.Sample();
        }

        /// <summary>
        /// The default method used for computing the derivative. Uses a simple three point estimation.
        /// </summary>
        /// <param name="function">Function for which the derivative is to be evaluated.</param>
        /// <param name="x">The location where the derivative is to be evaluated.</param>
        /// <returns>The derivative of the function at the point x.</returns>
        static private double Grad(DensityLn<double> function, double x)
        {
            double h = Math.Max(10e-4, (10e-7) * x);
            double increment = x + h;
            double decrement = x - h;
            return (function(increment) - function(decrement)) / (2 * h);
        }
    }
}
