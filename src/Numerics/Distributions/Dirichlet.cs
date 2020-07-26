// <copyright file="Dirichlet.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2011 Math.NET
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
using MathNet.Numerics.Random;

namespace MathNet.Numerics.Distributions
{
    /// <summary>
    /// Multivariate Dirichlet distribution. For details about this distribution, see
    /// <a href="http://en.wikipedia.org/wiki/Dirichlet_distribution">Wikipedia - Dirichlet distribution</a>.
    /// </summary>
    public class Dirichlet : IDistribution
    {
        System.Random _random;

        readonly double[] _alpha;

        /// <summary>
        /// Initializes a new instance of the Dirichlet class. The distribution will
        /// be initialized with the default <seealso cref="System.Random"/> random number generator.
        /// </summary>
        /// <param name="alpha">An array with the Dirichlet parameters.</param>
        public Dirichlet(double[] alpha)
        {
            if (Control.CheckDistributionParameters && !IsValidParameterSet(alpha))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            _random = SystemRandomSource.Default;
            _alpha = (double[])alpha.Clone();
        }

        /// <summary>
        /// Initializes a new instance of the Dirichlet class. The distribution will
        /// be initialized with the default <seealso cref="System.Random"/> random number generator.
        /// </summary>
        /// <param name="alpha">An array with the Dirichlet parameters.</param>
        /// <param name="randomSource">The random number generator which is used to draw random samples.</param>
        public Dirichlet(double[] alpha, System.Random randomSource)
        {
            if (Control.CheckDistributionParameters && !IsValidParameterSet(alpha))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            _random = randomSource ?? SystemRandomSource.Default;
            _alpha = (double[])alpha.Clone();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Dirichlet"/> class.
        /// <seealso cref="System.Random"/>random number generator.</summary>
        /// <param name="alpha">The value of each parameter of the Dirichlet distribution.</param>
        /// <param name="k">The dimension of the Dirichlet distribution.</param>
        public Dirichlet(double alpha, int k)
        {
            // Create a parameter structure.
            var parm = new double[k];
            for (var i = 0; i < k; i++)
            {
                parm[i] = alpha;
            }

            _random = SystemRandomSource.Default;
            if (Control.CheckDistributionParameters && !IsValidParameterSet(parm))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            _alpha = (double[])parm.Clone();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Dirichlet"/> class.
        /// <seealso cref="System.Random"/>random number generator.</summary>
        /// <param name="alpha">The value of each parameter of the Dirichlet distribution.</param>
        /// <param name="k">The dimension of the Dirichlet distribution.</param>
        /// <param name="randomSource">The random number generator which is used to draw random samples.</param>
        public Dirichlet(double alpha, int k, System.Random randomSource)
        {
            // Create a parameter structure.
            var parm = new double[k];
            for (var i = 0; i < k; i++)
            {
                parm[i] = alpha;
            }

            _random = randomSource ?? SystemRandomSource.Default;
            if (Control.CheckDistributionParameters && !IsValidParameterSet(parm))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            _alpha = (double[])parm.Clone();
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"Dirichlet(Dimension = {Dimension})";
        }

        /// <summary>
        /// Tests whether the provided values are valid parameters for this distribution.
        /// No parameter can be less than zero and at least one parameter should be larger than zero.
        /// </summary>
        /// <param name="alpha">The parameters of the Dirichlet distribution.</param>
        public static bool IsValidParameterSet(double[] alpha)
        {
            var allzero = true;

            for (int i = 0; i < alpha.Length; i++)
            {
                var t = alpha[i];
                if (t < 0.0)
                {
                    return false;
                }

                if (t > 0.0)
                {
                    allzero = false;
                }
            }

            return !allzero;
        }

        /// <summary>
        /// Gets or sets the parameters of the Dirichlet distribution.
        /// </summary>
        public double[] Alpha => _alpha;

        /// <summary>
        /// Gets or sets the random number generator which is used to draw random samples.
        /// </summary>
        public System.Random RandomSource
        {
            get => _random;
            set => _random = value ?? SystemRandomSource.Default;
        }

        /// <summary>
        /// Gets the dimension of the Dirichlet distribution.
        /// </summary>
        public int Dimension => _alpha.Length;

        /// <summary>
        /// Gets the sum of the Dirichlet parameters.
        /// </summary>
        double AlphaSum => _alpha.Sum();

        /// <summary>
        /// Gets the mean of the Dirichlet distribution.
        /// </summary>
        public double[] Mean
        {
            get
            {
                var sum = AlphaSum;
                var parm = new double[Dimension];
                for (var i = 0; i < Dimension; i++)
                {
                    parm[i] = _alpha[i]/sum;
                }

                return parm;
            }
        }

        /// <summary>
        /// Gets the variance of the Dirichlet distribution.
        /// </summary>
        public double[] Variance
        {
            get
            {
                var s = AlphaSum;
                var v = new double[_alpha.Length];
                for (var i = 0; i < _alpha.Length; i++)
                {
                    v[i] = _alpha[i]*(s - _alpha[i])/(s*s*(s + 1.0));
                }

                return v;
            }
        }

        /// <summary>
        /// Gets the entropy of the distribution.
        /// </summary>
        public double Entropy
        {
            get
            {
                var num = _alpha.Sum(t => (t - 1)*SpecialFunctions.DiGamma(t));
                return SpecialFunctions.GammaLn(AlphaSum) + ((AlphaSum - Dimension)*SpecialFunctions.DiGamma(AlphaSum)) - num;
            }
        }

        /// <summary>
        /// Computes the density of the distribution.
        /// </summary>
        /// <param name="x">The locations at which to compute the density.</param>
        /// <returns>the density at <paramref name="x"/>.</returns>
        /// <remarks>The Dirichlet distribution requires that the sum of the components of x equals 1.
        /// You can also leave out the last <paramref name="x"/> component, and it will be computed from the others. </remarks>
        public double Density(double[] x)
        {
            return Math.Exp(DensityLn(x));
        }

        /// <summary>
        /// Computes the log density of the distribution.
        /// </summary>
        /// <param name="x">The locations at which to compute the density.</param>
        /// <returns>the density at <paramref name="x"/>.</returns>
        public double DensityLn(double[] x)
        {
            if (x == null)
            {
                throw new ArgumentNullException(nameof(x));
            }

            var shortVersion = x.Length == (_alpha.Length - 1);
            if ((x.Length != _alpha.Length) && !shortVersion)
            {
                throw new ArgumentException("x");
            }

            var term = 0.0;
            var sumxi = 0.0;
            var sumalpha = 0.0;
            for (var i = 0; i < x.Length; i++)
            {
                var xi = x[i];
                if ((xi <= 0.0) || (xi >= 1.0))
                {
                    return 0.0;
                }

                term += (_alpha[i] - 1.0)*Math.Log(xi) - SpecialFunctions.GammaLn(_alpha[i]);
                sumxi += xi;
                sumalpha += _alpha[i];
            }

            // Calculate x[Length - 1] element, if needed
            if (shortVersion)
            {
                if (sumxi >= 1.0)
                {
                    return 0.0;
                }

                term += (_alpha[_alpha.Length - 1] - 1.0)*Math.Log(1.0 - sumxi) - SpecialFunctions.GammaLn(_alpha[_alpha.Length - 1]);
                sumalpha += _alpha[_alpha.Length - 1];
            }
            else if (!sumxi.AlmostEqualRelative(1.0, 8))
            {
                return 0.0;
            }

            return term + SpecialFunctions.GammaLn(sumalpha);
        }

        /// <summary>
        /// Samples a Dirichlet distributed random vector.
        /// </summary>
        /// <returns>A sample from this distribution.</returns>
        public double[] Sample()
        {
            return Sample(_random, _alpha);
        }

        /// <summary>
        /// Samples a Dirichlet distributed random vector.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="alpha">The Dirichlet distribution parameter.</param>
        /// <returns>a sample from the distribution.</returns>
        public static double[] Sample(System.Random rnd, double[] alpha)
        {
            if (Control.CheckDistributionParameters && !IsValidParameterSet(alpha))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            var n = alpha.Length;
            var gv = new double[n];
            var sum = 0.0;
            for (var i = 0; i < n; i++)
            {
                if (alpha[i] == 0.0)
                {
                    gv[i] = 0.0;
                }
                else
                {
                    gv[i] = Gamma.Sample(rnd, alpha[i], 1.0);
                }

                sum += gv[i];
            }

            for (var i = 0; i < n; i++)
            {
                gv[i] /= sum;
            }

            return gv;
        }
    }
}
