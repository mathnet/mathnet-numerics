// <copyright file="SignalGenerator.Random.cs" company="Math.NET">
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

namespace MathNet.Numerics.Signals
{
    using System;
    using Distributions;

    /// <summary>
    /// Generic Function Sampling and Quantization Provider
    /// </summary>
    public static partial class SignalGenerator
    {
        /// <summary>
        /// Samples a function randomly with the provided distribution.
        /// </summary>
        /// <param name="function">The real-domain function to sample.</param>
        /// <param name="distribution">Random distribution of the real domain sample points.</param>
        /// <param name="sampleCount">The number of samples to generate.</param>
        /// <typeparam name="T">The value type of the function to sample.</typeparam>
        /// <returns>The generated sample vector.</returns>
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="ArgumentOutOfRangeException" />
        public static T[] Random<T>(
            Func<double, T> function,
            IContinuousDistribution distribution,
            int sampleCount)
        {
            if (ReferenceEquals(function, null))
            {
                throw new ArgumentNullException("function");
            }

            if (ReferenceEquals(distribution, null))
            {
                throw new ArgumentNullException("distribution");
            }

            if (sampleCount < 0)
            {
                throw new ArgumentOutOfRangeException("sampleCount");
            }

            var samples = new T[sampleCount];

            for (int i = 0; i < samples.Length; i++)
            {
                samples[i] = function(distribution.Sample());
            }

            return samples;
        }

        /// <summary>
        /// Samples a function randomly with the provided distribution.
        /// </summary>
        /// <param name="function">The real-domain function to sample.</param>
        /// <param name="distribution">Random distribution of the real domain sample points.</param>
        /// <param name="sampleCount">The number of samples to generate.</param>
        /// <param name="samplePoints">The real domain points where the samples are taken at.</param>
        /// <typeparam name="T">The value type of the function to sample.</typeparam>
        /// <returns>The generated sample vector.</returns>
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="ArgumentOutOfRangeException" />
        public static T[] Random<T>(
            Func<double, T> function,
            IContinuousDistribution distribution,
            int sampleCount,
            out double[] samplePoints)
        {
            if (ReferenceEquals(function, null))
            {
                throw new ArgumentNullException("function");
            }

            if (ReferenceEquals(distribution, null))
            {
                throw new ArgumentNullException("distribution");
            }

            if (sampleCount < 0)
            {
                throw new ArgumentOutOfRangeException("sampleCount");
            }

            var samples = new T[sampleCount];
            samplePoints = new double[sampleCount];

            for (int i = 0; i < samples.Length; i++)
            {
                double current = distribution.Sample();
                samplePoints[i] = current;
                samples[i] = function(current);
            }

            return samples;
        }

        /// <summary>
        /// Samples a two-domain function randomly with the provided distribution.
        /// </summary>
        /// <param name="function">The real-domain function to sample.</param>
        /// <param name="distribution">Random distribution of the real domain sample points.</param>
        /// <param name="sampleCount">The number of samples to generate.</param>
        /// <typeparam name="T">The value type of the function to sample.</typeparam>
        /// <returns>The generated sample vector.</returns>
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="ArgumentOutOfRangeException" />
        public static T[] Random<T>(
            Func<double, double, T> function,
            IContinuousDistribution distribution,
            int sampleCount)
        {
            if (ReferenceEquals(function, null))
            {
                throw new ArgumentNullException("function");
            }

            if (ReferenceEquals(distribution, null))
            {
                throw new ArgumentNullException("distribution");
            }

            if (sampleCount < 0)
            {
                throw new ArgumentOutOfRangeException("sampleCount");
            }

            var samples = new T[sampleCount];

            for (int i = 0; i < samples.Length; i++)
            {
                samples[i] = function(distribution.Sample(), distribution.Sample());
            }

            return samples;
        }
    }
}