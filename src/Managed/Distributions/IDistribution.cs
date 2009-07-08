// <copyright file="Combinatorics.cs" company="Math.NET">
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

    /// <summary>
    /// The interface for univariate distributions.
    /// </summary>
    public interface IDistribution
    {
        /// <summary>
        /// Gets or sets the random number generator which is used to generate random samples from the distribution.
        /// </summary>
        Random RandomSource { get; set; }

        /// <summary>
        /// The mean of the distribution.
        /// </summary>
        double Mean { get; }

        /// <summary>
        /// The variance of the distribution.
        /// </summary>
        double Variance { get; }

        /// <summary>
        /// The standard deviation of the distribution.
        /// </summary>
        double StdDev { get; }

        /// <summary>
        /// The entropy of the distribution.
        /// </summary>
        double Entropy { get; }

        /// <summary>
        /// The skewness of the distribution.
        /// </summary>
        double Skewness { get; }

        /// <summary>
        /// Computes the cumulative distribution function (cdf) for this probability distribution.
        /// </summary>
        double CumulativeDistribution(double x);
    }
}
