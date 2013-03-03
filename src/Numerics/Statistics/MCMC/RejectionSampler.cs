// <copyright file="RejectionSampler.cs" company="Math.NET">
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
    /// Rejection sampling produces samples from distribition P by sampling from a proposal distribution Q
    /// and accepting/rejecting based on the density of P and Q. The density of P and Q don't need to
    /// to be normalized, but we do need that for each x, P(x) &lt; Q(x).
    /// </summary>
    /// <typeparam name="T">The type of samples this sampler produces.</typeparam>
    public class RejectionSampler<T> : McmcSampler<T>
    {
        /// <summary>
        /// Evaluates the density function of the sampling distribution.
        /// </summary>
        private readonly Density<T> _pdfP;

        /// <summary>
        /// Evaluates the density function of the proposal distribution.
        /// </summary>
        private readonly Density<T> _pdfQ;

        /// <summary>
        /// A function which samples from a proposal distribution.
        /// </summary>
        private readonly GlobalProposalSampler<T> _proposal;

        /// <summary>
        /// Constructs a new rejection sampler using the default <see cref="System.Random"/> random number generator.
        /// </summary>
        /// <param name="pdfP">The density of the distribution we want to sample from.</param>
        /// <param name="pdfQ">The density of the proposal distribution.</param>
        /// <param name="proposal">A method that samples from the proposal distribution.</param>
        public RejectionSampler(Density<T> pdfP, Density<T> pdfQ, GlobalProposalSampler<T> proposal)
        {
            _pdfP = pdfP;
            _pdfQ = pdfQ;
            _proposal = proposal;
        }

        /// <summary>
        /// Returns a sample from the distribution P.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">When the algorithms detects that the proposal
        /// distribution doesn't upper bound the target distribution.</exception>
        public override T Sample()
        {
            while (true)
            {
                // Get a sample from the proposal.
                T x = _proposal();
                // Evaluate the density for proposal.
                double q = _pdfQ(x);
                // Evaluate the density for the target density.
                double p = _pdfP(x);
                // Sample a variable between 0.0 and proposal density.
                double u = RandomSource.NextDouble() * q;

                Samples++;

                if (q < p)
                {
                    throw new ArgumentOutOfRangeException(Resources.ProposalDistributionNoUpperBound);
                }
                if (u < p)
                {
                    Accepts++;
                    return x;
                }
            }
        }
    }
}