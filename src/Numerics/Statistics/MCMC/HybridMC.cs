// <copyright file="HybridMC.cs" company="Math.NET">
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
    using System.Linq;
    using Distributions;

    /// <summary>
    /// A hybrid Monte Carlo sampler for multivariate distributions.
    /// </summary>
    public class HybridMC : HybridMCGeneric<double[]>
    {
        /// <summary>
        /// Number of parameters in the density function.
        /// </summary>
        private readonly int _length;

        /// <summary>
        /// Distribution to sample momentum from.
        /// </summary>
        private Normal _pDistribution;

        /// <summary>
        /// Standard deviations used in the sampling of different components of the
        /// momentum.
        /// </summary>
        private double[] _mpSdv;

        /// <summary>
        /// Gets or sets the standard deviations used in the sampling of different components of the
        /// momentum.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">When the length of pSdv is not the same as Length.</exception>
        public double[] MomentumStdDev
        {
            get { return (double[])_mpSdv.Clone(); }
            set
            {
                CheckVariance(value);
                _mpSdv = (double[])value.Clone();
            }
        }

        /// <summary>
        /// Constructs a new Hybrid Monte Carlo sampler for a multivariate probability distribution.
        /// The burn interval will be set to 0.
        /// The components of the momentum will be sampled from a normal distribution with standard deviation
        /// 1 using the default <see cref="System.Random"/> random
        /// number generator. A three point estimation will be used for differentiation.
        /// </summary>
        /// <param name="x0">The initial sample.</param>
        /// <param name="pdfLnP">The log density of the distribution we want to sample from.</param>
        /// <param name="frogLeapSteps">Number frogleap simulation steps.</param>
        /// <param name="stepSize">Size of the frogleap simulation steps.</param>
        public HybridMC(double[] x0, DensityLn<double[]> pdfLnP, int frogLeapSteps, double stepSize)
            : this(x0, pdfLnP, frogLeapSteps, stepSize, 0)
        {
        }

        /// <summary>
        /// Constructs a new Hybrid Monte Carlo sampler for a multivariate probability distribution.
        /// The components of the momentum will be sampled from a normal distribution with standard deviation
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
        public HybridMC(double[] x0, DensityLn<double[]> pdfLnP, int frogLeapSteps, double stepSize, int burnInterval)
            : this(x0, pdfLnP, frogLeapSteps, stepSize, burnInterval, new double[x0.Count()], new Random(), Grad)
        {
            for (int i = 0; i < _length; i++)
            {
                _mpSdv[i] = 1;
            }
        }

        /// <summary>
        /// Constructs a new Hybrid Monte Carlo sampler for a multivariate probability distribution.
        /// The components of the momentum will be sampled from a normal distribution with standard deviation
        /// specified by pSdv using the default <see cref="System.Random"/> random
        /// number generator. A three point estimation will be used for differentiation.
        /// This constructor will set the burn interval.
        /// </summary>
        /// <param name="x0">The initial sample.</param>
        /// <param name="pdfLnP">The log density of the distribution we want to sample from.</param>
        /// <param name="frogLeapSteps">Number frogleap simulation steps.</param>
        /// <param name="stepSize">Size of the frogleap simulation steps.</param>
        /// <param name="burnInterval">The number of iterations in between returning samples.</param>
        /// <param name="pSdv">The standard deviations of the normal distributions that are used to sample
        /// the components of the momentum.</param>
        /// <exception cref="ArgumentOutOfRangeException">When the number of burnInterval iteration is negative.</exception>
        public HybridMC(double[] x0, DensityLn<double[]> pdfLnP, int frogLeapSteps, double stepSize, int burnInterval, double[] pSdv)
            : this(x0, pdfLnP, frogLeapSteps, stepSize, burnInterval, pSdv, new Random())
        {
        }

        /// <summary>
        /// Constructs a new Hybrid Monte Carlo sampler for a multivariate probability distribution.
        /// The components of the momentum will be sampled from a normal distribution with standard deviation
        /// specified by pSdv using the a random number generator provided by the user.
        /// A three point estimation will be used for differentiation.
        /// This constructor will set the burn interval.
        /// </summary>
        /// <param name="x0">The initial sample.</param>
        /// <param name="pdfLnP">The log density of the distribution we want to sample from.</param>
        /// <param name="frogLeapSteps">Number frogleap simulation steps.</param>
        /// <param name="stepSize">Size of the frogleap simulation steps.</param>
        /// <param name="burnInterval">The number of iterations in between returning samples.</param>
        /// <param name="pSdv">The standard deviations of the normal distributions that are used to sample
        /// the components of the momentum.</param>
        /// <param name="randomSource">Random number generator used for sampling the momentum.</param>
        /// <exception cref="ArgumentOutOfRangeException">When the number of burnInterval iteration is negative.</exception>
        public HybridMC(double[] x0, DensityLn<double[]> pdfLnP, int frogLeapSteps, double stepSize, int burnInterval, double[] pSdv, Random randomSource)
            : this(x0, pdfLnP, frogLeapSteps, stepSize, burnInterval, pSdv, randomSource, Grad)
        {
        }

        /// <summary>
        /// Constructs a new Hybrid Monte Carlo sampler for a multivariate probability distribution.
        /// The components of the momentum will be sampled from a normal distribution with standard deviations
        /// given by pSdv. This constructor will set the burn interval, the method used for
        /// numerical differentiation and the random number generator.
        /// </summary>
        /// <param name="x0">The initial sample.</param>
        /// <param name="pdfLnP">The log density of the distribution we want to sample from.</param>
        /// <param name="frogLeapSteps">Number frogleap simulation steps.</param>
        /// <param name="stepSize">Size of the frogleap simulation steps.</param>
        /// <param name="burnInterval">The number of iterations in between returning samples.</param>
        /// <param name="pSdv">The standard deviations of the normal distributions that are used to sample
        /// the components of the momentum.</param>
        /// <param name="randomSource">Random number generator used for sampling the momentum.</param>
        /// <param name="diff">The method used for numerical differentiation.</param>
        /// <exception cref="ArgumentOutOfRangeException">When the number of burnInterval iteration is negative.</exception>
        /// <exception cref="ArgumentOutOfRangeException">When the length of pSdv is not the same as x0.</exception>
        public HybridMC(double[] x0, DensityLn<double[]> pdfLnP, int frogLeapSteps, double stepSize, int burnInterval, double[] pSdv, Random randomSource, DiffMethod diff)
            : base(x0, pdfLnP, frogLeapSteps, stepSize, burnInterval, randomSource, diff)
        {
            _length = x0.Count();
            MomentumStdDev = pSdv;

            Initialize(x0);

            Burn(BurnInterval);
        }

        /// <summary>
        /// Initialize parameters.
        /// </summary>
        /// <param name="x0">The current location of the sampler.</param>
        private void Initialize(double[] x0)
        {
            Current = (double[])x0.Clone();
            _pDistribution = new Normal(0, 1)
                {
                    RandomSource = RandomSource
                };
        }

        /// <summary>
        /// Checking that the location and the momentum are of the same dimension and that each component is positive.
        /// </summary>
        /// <param name="pSdv">The standard deviations used for sampling the momentum.</param>
        /// <exception cref="ArgumentOutOfRangeException">When the length of pSdv is not the same as Length or if any
        /// component is negative.</exception>
        /// <exception cref="ArgumentNullException">When pSdv is null.</exception>
        private void CheckVariance(double[] pSdv)
        {
            if (pSdv == null)
            {
                throw new ArgumentNullException("Standard deviation cannot be null.");
            }
            if (pSdv.Count() != _length)
            {
                throw new ArgumentOutOfRangeException("Standard deviation of momentum must have same length as sample.");
            }
            if (pSdv.Any(sdv => sdv < 0))
            {
                throw new ArgumentOutOfRangeException("Standard deviation must be positive.");
            }
        }

        /// <summary>
        /// Use for copying objects in the Burn method.
        /// </summary>
        /// <param name="source">The source of copying.</param>
        /// <returns>A copy of the source object.</returns>
        protected override double[] Copy(double[] source)
        {
            var destination = new double[_length];
            Array.Copy(source, destination, _length);
            return destination;
        }

        /// <summary>
        /// Use for creating temporary objects in the Burn method.
        /// </summary>
        /// <returns>An object of type T.</returns>
        protected override double[] Create()
        {
            return new double[_length];
        }

        ///<inheritdoc/>
        protected override void DoAdd(ref double[] first, double factor, double[] second)
        {
            for (int i = 0; i < _length; i++)
            {
                first[i] += factor * second[i];
            }
        }

        /// <inheritdoc/>
        protected override void DoSubtract(ref double[] first, double factor, double[] second)
        {
            for (int i = 0; i < _length; i++)
            {
                first[i] -= factor * second[i];
            }
        }

        /// <inheritdoc/>
        protected override double DoProduct(double[] first, double[] second)
        {
            double prod = 0;
            for (int i = 0; i < _length; i++)
            {
                prod += first[i] * second[i];
            }
            return prod;
        }

        /// <summary>
        /// Samples the momentum from a normal distribution.
        /// </summary>
        /// <param name="p">The momentum to be randomized.</param>
        protected override void RandomizeMomentum(ref double[] p)
        {
            for (int j = 0; j < _length; j++)
            {
                p[j] = _mpSdv[j] * _pDistribution.Sample();
            }
        }

        /// <summary>
        /// The default method used for computing the gradient. Uses a simple three point estimation.
        /// </summary>
        /// <param name="function">Function which the gradient is to be evaluated.</param>
        /// <param name="x">The location where the gradient is to be evaluated.</param>
        /// <returns>The gradient of the function at the point x.</returns>
        static private double[] Grad(DensityLn<double[]> function, double[] x)
        {
            int length = x.Length;
            var returnValue = new double[length];
            var increment = new double[length];
            var decrement = new double[length];

            Array.Copy(x, increment, length);
            Array.Copy(x, decrement, length);

            for (int i = 0; i < length; i++)
            {
                double y = x[i];
                double h = Math.Max(10e-4, (10e-7) * y);
                increment[i] += h;
                decrement[i] -= h;
                returnValue[i] = (function(increment) - function(decrement)) / (2 * h);
                increment[i] = y;
                decrement[i] = y;
            }

            return returnValue;
        }
    }
}
