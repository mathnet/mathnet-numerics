// <copyright file="IContinuousDistribution.cs" company="Math.NET">
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

namespace MathNet.Numerics.Distributions
{
    using System.Collections.Generic;

    /// <summary>
    /// The interface for continuous univariate distributions.
    /// </summary>
    public interface IContinuousDistribution : IDistribution
    {
        /// <summary>
        /// Gets the mode of the distribution.
        /// </summary>
        double Mode { get; }

        /// <summary>
        /// Gets the median of the distribution.
        /// </summary>
        double Median { get; }

        /// <summary>
        /// Gets the smallest element in the domain of the distributions which can be represented by a double.
        /// </summary>
        double Minimum { get; }

        /// <summary>
        /// Gets the largest element in the domain of the distributions which can be represented by a double.
        /// </summary>
        double Maximum { get; }

        /// <summary>
        /// The probability density of the distribution.
        /// </summary>
        /// <param name="x">The location at which to compute the density.</param>
        /// <returns>the density at <paramref name="x"/>.</returns>
        double Density(double x);

        /// <summary>
        /// The log probability density of the distribution.
        /// </summary>
        /// <param name="x">The location at which to compute the log density.</param>
        /// <returns>the log density at <paramref name="x"/>.</returns>
        double DensityLn(double x);

        /// <summary>
        /// Draws a random sample from the distribution.
        /// </summary>
        /// <returns>a sample from the distribution.</returns>
        double Sample();

        /// <summary>
        /// Draws a sequence of random samples from the distribution.
        /// </summary>
        /// <returns>a sequence of samples from the distribution.</returns>
        IEnumerable<double> Samples();
    }
}
