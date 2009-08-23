// <copyright file="Dirichlet.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://mathnet.opensourcedotnet.info
//
// Copyright (c) 2009 Math.NET
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
    using Properties;

    /// <summary>
    /// Implements the multivariate Dirichlet distribution. For details about this distribution, see 
    /// <a href="http://en.wikipedia.org/wiki/Dirichlet_distribution">Wikipedia - Dirichlet distribution</a>.
    /// </summary>
    /// <remarks><para>The distribution will use the <see cref="System.Random"/> by default. 
    /// Users can get/set the random number generator by using the <see cref="RandomSource"/> property.</para>
    /// <para>The statistics classes will check all the incoming parameters whether they are in the allowed
    /// range. This might involve heavy computation. Optionally, by setting Control.CheckDistributionParameters
    /// to false, all parameter checks can be turned off.</para></remarks>
    public class Dirichlet
    {
        // The Dirichlet distribution parameters.
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
        /// Constructs a new symmetric Dirichlet distribution. The distribution will
        /// be initialized with the default <seealso cref="System.Random"/> random number generator.
        /// </summary>
        /// <param name="alpha">The value of each parameter of the Dirichlet distribution.</param>
        /// <param name="k">The dimension of the Dirichlet distribution.</param>
        public Dirichlet(double alpha, int k)
        {
            // Create a parameter structure.
            double[] parm = new double[k];
            for (int i = 0; i < k; i++)
            {
                parm[i] = alpha;
            }

            SetParameters(parm);
            RandomSource = new Random();
        }

        /// <summary>
        /// Checks whether the parameters of the distribution are valid: no parameter can be less than zero and
        /// at least one parameter should be larger than zero.
        /// </summary>
        /// <param name="alpha">The parameters of the Dirichlet distribution.</param>
        /// <returns>True when the parameters are valid, false otherwise.</returns>
        public static bool IsValidParameterSet(double[] alpha)
        {
            bool allzero = true;

            for (int i = 0; i < alpha.Length; i++)
            {
                if (alpha[i] < 0.0)
                {
                    return false;
                }
                else if (alpha[i] > 0.0)
                {
                    allzero = false;
                }
            }

            if (allzero)
            {
                return false;
            }

            return true;
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

            _alpha = (double[]) alpha.Clone();
        }

        /// <summary>
        /// A string representation of the distribution.
        /// </summary>
        public override string ToString()
        {
            return "Dirichlet(Dimension = " + this.Dimension + ")";
        }

        /// <summary>
        /// Gets the dimension of the Dirichlet distribution.
        /// </summary>
        public int Dimension
        {
            get { return _alpha.Length; }
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
        /// The sum of the Dirichlet parameters.
        /// </summary>
        private double AlphaSum
        {
            get
            {
                double s = 0.0;
                for (int i = 0; i < _alpha.Length; i++)
                {
                    s += _alpha[i];
                }
                return s;
            }
        }

        /// <summary>
        /// Gets the mean of the Dirichlet distribution.
        /// </summary>
        public double[] Mean
        {
            get
            {
                double sum = AlphaSum;
                double[] parm = new double[Dimension];
                for (int i = 0; i < Dimension; i++)
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
                double s = this.AlphaSum;
                double[] v = new double[_alpha.Length];
                for (int i = 0; i < _alpha.Length; i++)
                {
                    v[i] = _alpha[i]*(s - _alpha[i])/(s*s*(s + 1.0));
                }
                return v;
            }
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
        public double[] Sample()
        {
            return Sample(RandomSource, _alpha);
        }

        /// <summary>
        /// Samples a Dirichlet distributed random vector.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="alpha">The Dirichlet distribution parameter.</param>
        public static double[] Sample(System.Random rnd, double[] alpha)
        {
            if (Control.CheckDistributionParameters && ! IsValidParameterSet(alpha))
            {
                throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);
            }

            int n = alpha.Length;
            double[] gv = new double[n];
            double sum = 0.0;
            for (int i = 0; i < n; i++)
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

            for (int i = 0; i < n; i++)
            {
                gv[i] /= sum;
            }

            return gv;
        }
    }
}