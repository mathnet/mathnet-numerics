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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace MathNet.Numerics.Statistics.Mcmc
{
    using Distributions;
    using Properties;
    using Numerics.Random;

    /// <summary>
    /// A hybrid Monte Carlo sampler for multivariate distributions.
    /// </summary>
    public class HybridMC : HybridMCGeneric<double[]>
    {

        #region Variables for internal use.

        /// <summary>
        /// Number of parameters in the density function.
        /// </summary>
        private int Length;

        /// <summary>
        /// Distribution to sample momentum from.
        /// </summary>
        private Normal pDistribution;

        #endregion

        /// <summary>
        /// Standard deviations used in the sampling of different components of the 
        /// momentum.
        /// </summary>
        private double[] mpSdv;

        /// <summary>
        /// Gets or sets the standard deviations used in the sampling of different components of the 
        /// momentum.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">When the length of pSdv is not the same as Length.</exception>
        public double[] MomentumStdDev
        {
            get { return (double[])mpSdv.Clone(); }
            set
            {
                CheckVariance(value);
                mpSdv = (double[])value.Clone();


            }

        }


        #region Ctor
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
        public HybridMC(double[] x0, DensityLn<double[]> pdfLnP, int frogLeapSteps, double stepSize) :
            this(x0, pdfLnP, frogLeapSteps, stepSize, 0)
        { }

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
        public HybridMC(double[] x0, DensityLn<double[]> pdfLnP, int frogLeapSteps, double stepSize, int burnInterval) :
            this(x0, pdfLnP, frogLeapSteps, stepSize, burnInterval, new double[x0.Count()], new DiffMethod(HybridMC.Grad))
        {
            for (int i = 0; i < Length; i++)
            { mpSdv[i] = 1; }
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
        public HybridMC(double[] x0, DensityLn<double[]> pdfLnP, int frogLeapSteps, double stepSize, int burnInterval, double[] pSdv) :
            this(x0, pdfLnP, frogLeapSteps, stepSize, burnInterval, pSdv, new DiffMethod(HybridMC.Grad))
        { }


        /// <summary>
        /// Constructs a new Hybrid Monte Carlo sampler for a multivariate probability distribution. 
        /// The components of the momentum will be sampled from a normal distribution with standard deviations
        /// given by pSdv using the default <see cref="System.Random"/> random 
        /// number generator.  This constructor will set both the burn interval and the method used for 
        /// numerical differentiation.
        /// </summary>
        /// <param name="x0">The initial sample.</param>
        /// <param name="pdfLnP">The log density of the distribution we want to sample from.</param>
        /// <param name="frogLeapSteps">Number frogleap simulation steps.</param>
        /// <param name="stepSize">Size of the frogleap simulation steps.</param>
        /// <param name="burnInterval">The number of iterations in between returning samples.</param>
        /// <param name="pSdv">The standard deviations of the normal distributions that are used to sample 
        /// the components of the momentum.</param>
        /// <param name="diff">The method used for numerical differentiation.</param>
        /// <exception cref="ArgumentOutOfRangeException">When the number of burnInterval iteration is negative.</exception>
        /// <exception cref="ArgumentOutOfRangeException">When the length of pSdv is not the same as x0.</exception>
        public HybridMC(double[] x0, DensityLn<double[]> pdfLnP, int frogLeapSteps, double stepSize, int burnInterval, double[] pSdv, DiffMethod diff)
            : base(x0, pdfLnP, frogLeapSteps, stepSize, burnInterval, diff)
        {
            Length = x0.Count();
            MomentumStdDev = pSdv;

            Initialize(x0);

            Burn(BurnInterval);

        }

        #endregion


        /// <summary>
        /// Initialize parameters.
        /// </summary>
        /// <param name="x0">The current location of the sampler.</param>
        private void Initialize(double[] x0)
        {
            mCurrent = (double[])x0.Clone();
            pDistribution = new Normal(0, 1);
            pDistribution.RandomSource = RandomSource;
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
                throw new ArgumentNullException("Standard deviation cannot be null.");
            if (pSdv.Count() != Length)
                throw new ArgumentOutOfRangeException("Standard deviation of momentum must have same length as sample.");
            foreach (double sdv in pSdv)
            {
                if (sdv < 0)
                { throw new ArgumentOutOfRangeException("Standard deviation must be positive."); }
            }
        }

        #region Inherited from HybridMCGeneric

        /// <summary>
        /// Use for copying objects in the Burn method.
        /// </summary>
        /// <param name="source">The source of copying.</param>
        /// <returns>A copy of the source object.</returns>
        protected override double[] Copy(double[] source)
        {
            double[] destination = new double[Length];
            Array.Copy(source, destination, Length);
            return destination;
        }

        /// <summary>
        /// Use for creating temporary objects in the Burn method.
        /// </summary>
        /// <returns>An object of type T.</returns>
        protected override double[] Create()
        {
            return new double[Length];
        }

        ///<inheritdoc/>
        protected override void DoAdd(ref double[] first, double factor, double[] second)
        {
            for (int i = 0; i < Length; i++)
            { first[i] += factor * second[i]; }
        }

        /// <inheritdoc/>
        protected override void DoSubtract(ref double[] first, double factor, double[] second)
        {
            for (int i = 0; i < Length; i++)
            { first[i] -= factor * second[i]; }
        }

        /// <inheritdoc/>
        protected override double DoProduct(double[] first, double[] second)
        {
            double prod = 0;
            for (int i = 0; i < Length; i++)
            { prod += first[i] * second[i]; }
            return prod;
        }

        /// <summary>
        /// Samples the momentum from a normal distribution. 
        /// </summary>
        /// <param name="p">The momentum to be randomized.</param>
        protected override void RandomizeMomentum(ref double[] p)
        {
            for (int j = 0; j < Length; j++)
            { p[j] = mpSdv[j] * pDistribution.Sample(); }
        }
        #endregion

        /// <summary>
        /// The default method used for computing the gradient. Uses a simple three point estimation.
        /// </summary>
        /// <param name="function">Function which the gradient is to be evaluated.</param>
        /// <param name="x">The location where the gradient is to be evaluated.</param>
        /// <returns>The gradient of the function at the point x.</returns>
        static private double[] Grad(DensityLn<double[]> function, double[] x)
        {
            int Length = x.Length;
            double[] ReturnValue = new double[Length];
            double[] Increment = new double[Length];
            Array.Copy(x, Increment, Length);
            double[] Decrement = new double[Length];
            Array.Copy(x, Decrement, Length);
            for (int i = 0; i < Length; i++)
            {
                double y = x[i];
                double h = Math.Max(10e-4, (10e-7) * y);
                Increment[i] += h;
                Decrement[i] -= h;
                ReturnValue[i] = (function(Increment) - function(Decrement)) / (2 * h);
                Increment[i] = y;
                Decrement[i] = y;
            }
            return ReturnValue;
        }


    }
}
