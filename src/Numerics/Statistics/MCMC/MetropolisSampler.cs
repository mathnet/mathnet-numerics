// <copyright file="MetropolisSampler.cs" company="Math.NET">
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
    using Distributions;

    /// <summary>
    /// Metropolis sampling produces samples from distribition P by sampling from a proposal distribution Q
    /// and accepting/rejecting based on the density of P. Metropolis sampling requires that the proposal
    /// distribution Q is symmetric. All densities are required to be in log space.
    /// 
    /// The Metropolis sampler is a stateful sampler. It keeps track of where it currently is in the domain
    /// of the distribution P.
    /// </summary>
    /// <typeparam name="T">The type of samples this sampler produces.</typeparam>
    public class MetropolisSampler<T> : McmcSampler<T>
    {
        /// <summary>
        /// Evaluates the log density function of the sampling distribution.
        /// </summary>
        private readonly DensityLn<T> mPdfLnP;

        /// <summary>
        /// A function which samples from a proposal distribution.
        /// </summary>
        private readonly LocalProposalSampler<T> mProposal;

        /// <summary>
        /// The current location of the sampler.
        /// </summary>
        private T mCurrent;

        /// <summary>
        /// The log density at the current location.
        /// </summary>
        private double mCurrentDensityLn;

        /// <summary>
        /// The number of burn iterations between two samples.
        /// </summary>
        private int mBurnInterval;

        /// <summary>
        /// Constructs a new Metropolis sampler using the default <see cref="System.Random"/> random 
        /// number generator. The burnInterval interval will be set to 0.
        /// </summary>
        /// <param name="x0">The initial sample.</param>
        /// <param name="pdfLnP">The log density of the distribution we want to sample from.</param>
        /// <param name="proposal">A method that samples from the symmetric proposal distribution.</param>
        public MetropolisSampler(T x0, DensityLn<T> pdfLnP, LocalProposalSampler<T> proposal) :
            this(x0, pdfLnP, proposal, 0)
        {
        }

        /// <summary>
        /// Constructs a new Metropolis sampler using the default <see cref="System.Random"/> random number generator.
        /// </summary>
        /// <param name="x0">The initial sample.</param>
        /// <param name="pdfLnP">The log density of the distribution we want to sample from.</param>
        /// <param name="proposal">A method that samples from the symmetric proposal distribution.</param>
        /// <param name="burnInterval">The number of iterations in between returning samples.</param>
        /// <exception cref="ArgumentOutOfRangeException">When the number of burnInterval iteration is negative.</exception>
        public MetropolisSampler(T x0, DensityLn<T> pdfLnP, LocalProposalSampler<T> proposal, int burnInterval)
        {
            mCurrent = x0;
            mCurrentDensityLn = pdfLnP(x0);
            mPdfLnP = pdfLnP;
            mProposal = proposal;
            BurnInterval = burnInterval;

            Burn(BurnInterval);
        }

        /// <summary>
        /// Gets or sets the number of iterations in between returning samples.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">When burn interval is negative.</exception>
        public int BurnInterval
        {
            get { return mBurnInterval; }

            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(Resources.ArgumentNotNegative);
                }
                mBurnInterval = value;
            }
        }

        /// <summary>
        /// This method runs the sampler for a number of iterations without returning a sample
        /// </summary>
        private void Burn(int n)
        {
            for (int i = 0; i < n; i++)
            {
                // Get a sample from the proposal.
                T next = mProposal(mCurrent);
                // Evaluate the density at the next sample.
                double p = mPdfLnP(next);

                Samples++;

                double acc = Math.Min(0.0, p - mCurrentDensityLn);
                if (acc == 0.0)
                {
                    mCurrent = next;
                    mCurrentDensityLn = p;
                    Accepts++;
                }
                else if (Bernoulli.Sample(RandomSource, Math.Exp(acc)) == 1)
                {
                    mCurrent = next;
                    mCurrentDensityLn = p;
                    Accepts++;
                }
            }
        }

        /// <summary>
        /// Returns a sample from the distribution P.
        /// </summary>
        public override T Sample()
        {
            Burn(BurnInterval + 1);

            return mCurrent;
        }
    }
}