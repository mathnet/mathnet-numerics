// <copyright file="SignalGenerator.Equidistant.cs" company="Math.NET">
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
    using System.Collections.Generic;

    /// <summary>
    /// Generic Function Sampling and Quantization Provider
    /// </summary>
    public static partial class SignalGenerator
    {
        /// <summary>
        /// Samples a function equidistant within the provided interval.
        /// </summary>
        /// <param name="function">The real-domain function to sample.</param>
        /// <param name="intervalBegin">The real domain interval begin where to start sampling.</param>
        /// <param name="intervalEnd">The real domain interval end where to stop sampling.</param>
        /// <param name="sampleCount">The number of samples to generate.</param>
        /// <typeparam name="T">The value type of the function to sample.</typeparam>
        /// <returns>The generated sample vector.</returns>
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="ArgumentOutOfRangeException" />
        [Obsolete]
        public static T[] EquidistantInterval<T>(
            Func<double, T> function,
            double intervalBegin,
            double intervalEnd,
            int sampleCount)
        {
            return Generate.LinearSpacedMap(sampleCount, intervalBegin, intervalEnd, function);
        }

        /// <summary>
        /// Samples a function equidistant within the provided interval.
        /// </summary>
        /// <param name="function">The real-domain function to sample.</param>
        /// <param name="intervalBegin">The real domain interval begin where to start sampling.</param>
        /// <param name="intervalEnd">The real domain interval end where to stop sampling.</param>
        /// <param name="sampleCount">The number of samples to generate.</param>
        /// <param name="samplePoints">The real domain points where the samples are taken at.</param>
        /// <typeparam name="T">The value type of the function to sample.</typeparam>
        /// <returns>The generated sample vector.</returns>
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="ArgumentOutOfRangeException" />
        [Obsolete]
        public static T[] EquidistantInterval<T>(
            Func<double, T> function,
            double intervalBegin,
            double intervalEnd,
            int sampleCount,
            out double[] samplePoints)
        {
            samplePoints = Generate.LinearSpaced(sampleCount, intervalBegin, intervalEnd);
            return Generate.Map(samplePoints, function);
        }

        /// <summary>
        /// Samples a periodic function equidistant within one period, but omits the last sample such that the sequence
        /// can be concatenated together.
        /// </summary>
        /// <param name="function">The real-domain function to sample.</param>
        /// <param name="periodLength">The real domain full period length.</param>
        /// <param name="periodOffset">The real domain offset where to start the sampling period.</param>
        /// <param name="sampleCount">The number of samples to generate.</param>
        /// <typeparam name="T">The value type of the function to sample.</typeparam>
        /// <returns>The generated sample vector.</returns>
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="ArgumentOutOfRangeException"/>
        [Obsolete]
        public static T[] EquidistantPeriodic<T>(
            Func<double, T> function,
            double periodLength,
            double periodOffset,
            int sampleCount)
        {
            return Generate.PeriodicMap(sampleCount, function, sampleCount, 1.0, periodLength, periodOffset);
        }

        /// <summary>
        /// Samples a periodic function equidistant within one period, but omits the last sample such that the sequence
        /// can be concatenated together.
        /// </summary>
        /// <param name="function">The real-domain function to sample.</param>
        /// <param name="periodLength">The real domain full period length.</param>
        /// <param name="periodOffset">The real domain offset where to start the sampling period.</param>
        /// <param name="sampleCount">The number of samples to generate.</param>
        /// <param name="samplePoints">The real domain points where the samples are taken at.</param>
        /// <typeparam name="T">The value type of the function to sample.</typeparam>
        /// <returns>The generated sample vector.</returns>
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="ArgumentOutOfRangeException"/>
        [Obsolete]
        public static T[] EquidistantPeriodic<T>(
            Func<double, T> function,
            double periodLength,
            double periodOffset,
            int sampleCount,
            out double[] samplePoints)
        {
            samplePoints = Generate.Periodic(sampleCount, sampleCount, 1.0, periodLength, periodOffset);
            return Generate.Map(samplePoints, function);
        }

        /// <summary>
        /// Samples a function equidistant starting from the provided location with a fixed step length.
        /// </summary>
        /// <param name="function">The real-domain function to sample.</param>
        /// <param name="start">The real domain location offset where to start sampling.</param>
        /// <param name="step">The real domain step length between the equidistant samples.</param>
        /// <param name="sampleCount">The number of samples to generate.</param>
        /// <typeparam name="T">The value type of the function to sample.</typeparam>
        /// <returns>The generated sample vector.</returns>
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="ArgumentOutOfRangeException"/>
        [Obsolete]
        public static T[] EquidistantStartingAt<T>(
            Func<double, T> function,
            double start,
            double step,
            int sampleCount)
        {
            return Generate.LinearSpacedMap(sampleCount, start, start + (sampleCount - 1)*step, function);
        }

        /// <summary>
        /// Samples a function equidistant starting from the provided location with a fixed step length.
        /// </summary>
        /// <param name="function">The real-domain function to sample.</param>
        /// <param name="start">The real domain location offset where to start sampling.</param>
        /// <param name="step">The real domain step length between the equidistant samples.</param>
        /// <param name="sampleCount">The number of samples to generate.</param>
        /// <param name="samplePoints">The real domain points where the samples are taken at.</param>
        /// <typeparam name="T">The value type of the function to sample.</typeparam>
        /// <returns>The generated sample vector.</returns>
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="ArgumentOutOfRangeException"/>
        [Obsolete]
        public static T[] EquidistantStartingAt<T>(
            Func<double, T> function,
            double start,
            double step,
            int sampleCount,
            out double[] samplePoints)
        {
            samplePoints = Generate.LinearSpaced(sampleCount, start, start + (sampleCount - 1)*step);
            return Generate.Map(samplePoints, function);
        }

        /// <summary>
        /// Samples a function equidistant continuously starting from the provided location with a fixed step length.
        /// </summary>
        /// <param name="function">The real-domain function to sample.</param>
        /// <param name="start">The real domain location offset where to start sampling.</param>
        /// <param name="step">The real domain step length between the equidistant samples.</param>
        /// <typeparam name="T">The value type of the function to sample.</typeparam>
        /// <returns>The generated sample enumerator.</returns>
        /// <exception cref="ArgumentNullException" />
        [Obsolete]
        public static IEnumerable<T> EquidistantContinuous<T>(
            Func<double, T> function,
            double start,
            double step)
        {
            if (ReferenceEquals(function, null))
            {
                throw new ArgumentNullException("function");
            }

            var current = start;

            while (true)
            {
                yield return function(current);
                current += step;
            }
        }

        /// <summary>
        /// Samples a function equidistant with the provided start and step length to an integer-domain function
        /// </summary>
        /// <param name="function">The real-domain function to sample.</param>
        /// <param name="start">The real domain location where to start sampling.</param>
        /// <param name="step">The real domain step length between the equidistant samples.</param>
        /// <typeparam name="T">The value type of the function to sample.</typeparam>
        /// <returns>The generated samples integer-domain function.</returns>
        /// <exception cref="ArgumentNullException" />
        [Obsolete]
        public static Func<int, T> EquidistantToFunction<T>(
            Func<double, T> function,
            double start,
            double step)
        {
            if (ReferenceEquals(function, null))
            {
                throw new ArgumentNullException("function");
            }

            return k => function(start + k * step);
        }
    }
}