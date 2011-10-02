// <copyright file="Dirichlet.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
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

namespace MathNet.Numerics.Distributions
{
    using System;
    using System.Linq;
    using Properties;

    /// <summary>
    /// Implements the multivariate Dirichlet distribution. For details about this distribution, see 
    /// <a href="http://en.wikipedia.org/wiki/Dirichlet_distribution">Wikipedia - Dirichlet distribution</a>.
    /// </summary>
    /// <remarks><para>The distribution will use the <see cref="System.Random"/> by default. 
    /// Users can get/set the random number generator by using the <see cref="RandomSource"/> property.</para>
    /// <para>The statistics classes will check all the incoming parameters whether they are in the allowed
    /// range. This might involve heavy computation. Optionally, by setting Control.CheckDistributionParameters
    /// to <c>false</c>, all parameter checks can be turned off.</para></remarks>
    public class Dirichlet
    {
        /// <summary>
        /// The Dirichlet distribution parameters.
        /// </summary>
        private double[] _alpha;

        /// <summary>
        /// The distribution's random number generator.
        /// </summary>
        private Random _random;

        /// <summary>
        /// Initializes a new instance of the Dirichlet class. The distribution will
        /// be initialized with the default <seealso cref="System.Random"/> random number generator.
        /// </summary>
        /// <param name="alpha">An array with the Dirichlet parameters.</param>
        public Dirichlet(double[] alpha)
        {
            SetParameters(alpha);
            RandomSource = new Random();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Dirichlet"/> class. 
        /// <seealso cref="System.Random"/>
        /// random number generator.
        /// </summary>
        /// <param name="alpha">
        /// The value of each parameter of the Dirichlet distribution.
        /// </param>
        /// <param name="k">
        /// The dimension of the Dirichlet distribution.
        /// </param>
        public Dirichlet(double alpha, int k)
        {
            // Create a parameter structure.
            var parm = new double[k];
            for (var i = 0; i < k; i++)
            {
                parm[i] = alpha;
            }

            SetParameters(parm);
            RandomSource = new Random();
        }

        /// <summary>
        /// Checks whether the parameters of the distribution are valid: no
        /// parameter can be less than zero and at least one parameter should be
        /// larger than zero.
        /// </summary>
        /// <param name="alpha">The parameters of the Dirichlet distribution.
        /// </param>
        /// <returns><c>true</c> when the parameters are valid, <c>false</c>
        /// otherwise.</returns>
        public static bool IsValidParameterSet(double[] alpha)
        {
            var allzero = true;

            foreach (var t in alpha)
            {
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
        /// Sets the parameters of the distribution after checking their validity.
        /// </summary>
        /// <param name="alpha">The parameters of the Dirichlet distribution.</param>
        /// <exception cref="ArgumentOutOfRangeException">When the parameters don't pass the <see cref="IsValidParameterSet"/> function.</exception>
        private void SetParameters(double[] alpha)
        {
            if (Control.CheckDistributionParameters && !IsValidParameterSet(alpha))
            {
                throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);
            }

            _alpha = (double[])alpha.Clone();
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return "Dirichlet(Dimension = " + Dimension + ")";
        }

        /// <summary>
        /// Gets the dimension of the Dirichlet distribution.
        /// </summary>
        public int Dimension
        {
            get
            {
                return _alpha.Length;
            }
        }

        /// <summary>
        /// Gets or sets the parameters of the Dirichlet distribution.
        /// </summary>
        public double[] Alpha
        {
            get
            {
                return _alpha;
            }

            set
            {
                SetParameters(value);
            }
        }

        /// <summary>
        /// Gets the sum of the Dirichlet parameters.
        /// </summary>
        private double AlphaSum
        {
            get
            {
                return _alpha.Sum();
            }
        }

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
                    parm[i] = _alpha[i] / sum;
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
                    v[i] = _alpha[i] * (s - _alpha[i]) / (s * s * (s + 1.0));
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
                var num = _alpha.Sum(t => (t - 1) * SpecialFunctions.DiGamma(t));
                return SpecialFunctions.GammaLn(AlphaSum) + ((AlphaSum - Dimension) * SpecialFunctions.DiGamma(AlphaSum)) - num;
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
                throw new ArgumentNullException("x");
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

                term += (_alpha[i] - 1.0) * Math.Log(xi) - SpecialFunctions.GammaLn(_alpha[i]);
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

                term += (_alpha[_alpha.Length - 1] - 1.0) * Math.Log(1.0 - sumxi) - SpecialFunctions.GammaLn(_alpha[_alpha.Length - 1]);
                sumalpha += _alpha[_alpha.Length - 1];
            }
            else if (!sumxi.AlmostEqualInDecimalPlaces(1.0, 8))
            {
                return 0.0;
            }

            return term + SpecialFunctions.GammaLn(sumalpha);
        }

        /// <summary>
        /// Gets or sets the random number generator which is used to draw random samples.
        /// </summary>
        public Random RandomSource
        {
            get
            {
                return _random;
            }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException();
                }

                _random = value;
            }
        }

        /// <summary>
        /// Samples a Dirichlet distributed random vector.
        /// </summary>
        /// <returns>A sample from this distribution.</returns>
        public double[] Sample()
        {
            return Sample(RandomSource, _alpha);
        }

        /// <summary>
        /// Samples a Dirichlet distributed random vector.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="alpha">The Dirichlet distribution parameter.</param>
        /// <returns>a sample from the distribution.</returns>
        public static double[] Sample(Random rnd, double[] alpha)
        {
            if (Control.CheckDistributionParameters && ! IsValidParameterSet(alpha))
            {
                throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);
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
