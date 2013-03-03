// <copyright file="UnivariateSliceSampler.cs" company="Math.NET">
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
    using Properties;

    /// <summary>
    /// Slice sampling produces samples from distribition P by uniformly sampling from under the pdf of P using
    /// a technique described in "Slice Sampling", R. Neal, 2003. All densities are required to be in log space.
    ///
    /// The slice sampler is a stateful sampler. It keeps track of where it currently is in the domain
    /// of the distribution P.
    /// </summary>
    public class UnivariateSliceSampler : McmcSampler<double>
    {
        /// <summary>
        /// Evaluates the log density function of the target distribution.
        /// </summary>
        private readonly DensityLn<double> _pdfLnP;

        /// <summary>
        /// The current location of the sampler.
        /// </summary>
        private double _current;

        /// <summary>
        /// The log density at the current location.
        /// </summary>
        private double _currentDensityLn;

        /// <summary>
        /// The number of burn iterations between two samples.
        /// </summary>
        private int _burnInterval;

        /// <summary>
        /// The scale of the slice sampler.
        /// </summary>
        private double _scale;

        /// <summary>
        /// Constructs a new Slice sampler using the default <see cref="System.Random"/> random
        /// number generator. The burn interval will be set to 0.
        /// </summary>
        /// <param name="x0">The initial sample.</param>
        /// <param name="pdfLnP">The density of the distribution we want to sample from.</param>
        /// <param name="scale">The scale factor of the slice sampler.</param>
        /// <exception cref="ArgumentOutOfRangeException">When the scale of the slice sampler is not positive.</exception>
        public UnivariateSliceSampler(double x0, DensityLn<double> pdfLnP, double scale)
            : this(x0, pdfLnP, 0, scale)
        {
        }

        /// <summary>
        /// Constructs a new slice sampler using the default <see cref="System.Random"/> random number generator. It
        /// will set the number of burnInterval iterations and run a burnInterval phase.
        /// </summary>
        /// <param name="x0">The initial sample.</param>
        /// <param name="pdfLnP">The density of the distribution we want to sample from.</param>
        /// <param name="burnInterval">The number of iterations in between returning samples.</param>
        /// <param name="scale">The scale factor of the slice sampler.</param>
        /// <exception cref="ArgumentOutOfRangeException">When the number of burnInterval iteration is negative.</exception>
        /// <exception cref="ArgumentOutOfRangeException">When the scale of the slice sampler is not positive.</exception>
        public UnivariateSliceSampler(double x0, DensityLn<double> pdfLnP, int burnInterval, double scale)
        {
            _current = x0;
            _currentDensityLn = pdfLnP(x0);
            _pdfLnP = pdfLnP;
            Scale = scale;
            BurnInterval = burnInterval;

            Burn(BurnInterval);
        }

        /// <summary>
        /// Gets or sets the number of iterations in between returning samples.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">When burn interval is negative.</exception>
        public int BurnInterval
        {
            get { return _burnInterval; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(Resources.ArgumentNotNegative);
                }
                _burnInterval = value;
            }
        }

        /// <summary>
        /// Gets or sets the scale of the slice sampler.
        /// </summary>
        public double Scale
        {
            get { return _scale; }
            set
            {
                if (value <= 0.0)
                {
                    throw new ArgumentOutOfRangeException(Resources.ArgumentPositive);
                }
                _scale = value;
            }
        }

        /// <summary>
        /// This method runs the sampler for a number of iterations without returning a sample
        /// </summary>
        private void Burn(int n)
        {
            for (int i = 0; i < n; i++)
            {
                // The logarithm of the slice height.
                double lu = Math.Log(RandomSource.NextDouble()) + _currentDensityLn;

                // Create a horizontal interval (x_l, x_r) enclosing x.
                double r = RandomSource.NextDouble();
                double xL = _current - r * Scale;
                double xR = _current + (1.0 - r) * Scale;

                // Stepping out procedure.
                while (_pdfLnP(xL) > lu) { xL -= Scale; }
                while (_pdfLnP(xR) > lu) { xR += Scale; }

                // Shrinking: propose new x and shrink interval until good one found.
                while (true)
                {
                    double xnew = RandomSource.NextDouble() * (xR - xL) + xL;
                    _currentDensityLn = _pdfLnP(xnew);
                    if (_currentDensityLn > lu)
                    {
                        _current = xnew;
                        Accepts++;
                        Samples++;
                        break;
                    }
                    if (xnew > _current)
                    {
                        xR = xnew;
                    }
                    else
                    {
                        xL = xnew;
                    }
                }
            }
        }

        /// <summary>
        /// Returns a sample from the distribution P.
        /// </summary>
        public override double Sample()
        {
            Burn(BurnInterval + 1);

            return _current;
        }
    }
}
