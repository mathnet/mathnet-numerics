// <copyright file="StreamingStatistics.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2015 Math.NET
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
using System.Collections.Generic;
using System.Linq;
using Complex = System.Numerics.Complex;

namespace MathNet.Numerics.Statistics
{
    /// <summary>
    /// Statistics operating on an IEnumerable in a single pass, without keeping the full data in memory.
    /// Can be used in a streaming way, e.g. on large datasets not fitting into memory.
    /// </summary>
    /// <seealso cref="SortedArrayStatistics"/>
    /// <seealso cref="StreamingStatistics"/>
    /// <seealso cref="Statistics"/>
    public static class StreamingStatistics
    {
        /// <summary>
        /// Returns the smallest value from the enumerable, in a single pass without memoization.
        /// Returns NaN if data is empty or any entry is NaN.
        /// </summary>
        /// <param name="stream">Sample stream, no sorting is assumed.</param>
        public static double Minimum(IEnumerable<double> stream)
        {
            double min = double.PositiveInfinity;
            bool any = false;

            foreach (var d in stream)
            {
                if (d < min || double.IsNaN(d))
                {
                    min = d;
                }

                any = true;
            }

            return any ? min : double.NaN;
        }

        /// <summary>
        /// Returns the smallest value from the enumerable, in a single pass without memoization.
        /// Returns NaN if data is empty or any entry is NaN.
        /// </summary>
        /// <param name="stream">Sample stream, no sorting is assumed.</param>
        public static float Minimum(IEnumerable<float> stream)
        {
            float min = float.PositiveInfinity;
            bool any = false;

            foreach (var d in stream)
            {
                if (d < min || float.IsNaN(d))
                {
                    min = d;
                }

                any = true;
            }

            return any ? min : float.NaN;
        }

        /// <summary>
        /// Returns the largest value from the enumerable, in a single pass without memoization.
        /// Returns NaN if data is empty or any entry is NaN.
        /// </summary>
        /// <param name="stream">Sample stream, no sorting is assumed.</param>
        public static double Maximum(IEnumerable<double> stream)
        {
            double max = double.NegativeInfinity;
            bool any = false;

            foreach (var d in stream)
            {
                if (d > max || double.IsNaN(d))
                {
                    max = d;
                }

                any = true;
            }

            return any ? max : double.NaN;
        }

        /// <summary>
        /// Returns the largest value from the enumerable, in a single pass without memoization.
        /// Returns NaN if data is empty or any entry is NaN.
        /// </summary>
        /// <param name="stream">Sample stream, no sorting is assumed.</param>
        public static float Maximum(IEnumerable<float> stream)
        {
            float max = float.NegativeInfinity;
            bool any = false;

            foreach (var d in stream)
            {
                if (d > max || float.IsNaN(d))
                {
                    max = d;
                }

                any = true;
            }

            return any ? max : float.NaN;
        }

        /// <summary>
        /// Returns the smallest absolute value from the enumerable, in a single pass without memoization.
        /// Returns NaN if data is empty or any entry is NaN.
        /// </summary>
        /// <param name="stream">Sample stream, no sorting is assumed.</param>
        public static double MinimumAbsolute(IEnumerable<double> stream)
        {
            double min = double.PositiveInfinity;
            bool any = false;

            foreach (var d in stream)
            {
                if (Math.Abs(d) < min || double.IsNaN(d))
                {
                    min = Math.Abs(d);
                }

                any = true;
            }

            return any ? min : double.NaN;
        }

        /// <summary>
        /// Returns the smallest absolute value from the enumerable, in a single pass without memoization.
        /// Returns NaN if data is empty or any entry is NaN.
        /// </summary>
        /// <param name="stream">Sample stream, no sorting is assumed.</param>
        public static float MinimumAbsolute(IEnumerable<float> stream)
        {
            float min = float.PositiveInfinity;
            bool any = false;

            foreach (var d in stream)
            {
                if (Math.Abs(d) < min || float.IsNaN(d))
                {
                    min = Math.Abs(d);
                }

                any = true;
            }

            return any ? min : float.NaN;
        }

        /// <summary>
        /// Returns the largest absolute value from the enumerable, in a single pass without memoization.
        /// Returns NaN if data is empty or any entry is NaN.
        /// </summary>
        /// <param name="stream">Sample stream, no sorting is assumed.</param>
        public static double MaximumAbsolute(IEnumerable<double> stream)
        {
            double max = 0.0d;
            bool any = false;

            foreach (var d in stream)
            {
                if (Math.Abs(d) > max || double.IsNaN(d))
                {
                    max = Math.Abs(d);
                }

                any = true;
            }

            return any ? max : double.NaN;
        }

        /// <summary>
        /// Returns the largest absolute value from the enumerable, in a single pass without memoization.
        /// Returns NaN if data is empty or any entry is NaN.
        /// </summary>
        /// <param name="stream">Sample stream, no sorting is assumed.</param>
        public static float MaximumAbsolute(IEnumerable<float> stream)
        {
            float max = 0.0f;
            bool any = false;

            foreach (var d in stream)
            {
                if (Math.Abs(d) > max || float.IsNaN(d))
                {
                    max = Math.Abs(d);
                }

                any = true;
            }

            return any ? max : float.NaN;
        }

        /// <summary>
        /// Returns the smallest absolute value from the enumerable, in a single pass without memoization.
        /// Returns NaN if data is empty or any entry is NaN.
        /// </summary>
        /// <param name="stream">Sample stream, no sorting is assumed.</param>
        public static Complex MinimumMagnitudePhase(IEnumerable<Complex> stream)
        {
            double minMagnitude = double.PositiveInfinity;
            Complex min = new Complex(double.PositiveInfinity, double.PositiveInfinity);
            bool any = false;

            foreach (var d in stream)
            {
                double magnitude = d.Magnitude;
                if (double.IsNaN(magnitude))
                {
                    return new Complex(double.NaN, double.NaN);
                }
                if (magnitude < minMagnitude || magnitude == minMagnitude && d.Phase < min.Phase)
                {
                    minMagnitude = magnitude;
                    min = d;
                }

                any = true;
            }

            return any ? min : new Complex(double.NaN, double.NaN);
        }

        /// <summary>
        /// Returns the smallest absolute value from the enumerable, in a single pass without memoization.
        /// Returns NaN if data is empty or any entry is NaN.
        /// </summary>
        /// <param name="stream">Sample stream, no sorting is assumed.</param>
        public static Complex32 MinimumMagnitudePhase(IEnumerable<Complex32> stream)
        {
            float minMagnitude = float.PositiveInfinity;
            Complex32 min = new Complex32(float.PositiveInfinity, float.PositiveInfinity);
            bool any = false;

            foreach (var d in stream)
            {
                float magnitude = d.Magnitude;
                if (float.IsNaN(magnitude))
                {
                    return new Complex32(float.NaN, float.NaN);
                }
                if (magnitude < minMagnitude || magnitude == minMagnitude && d.Phase < min.Phase)
                {
                    minMagnitude = magnitude;
                    min = d;
                }

                any = true;
            }

            return any ? min : new Complex32(float.NaN, float.NaN);
        }

        /// <summary>
        /// Returns the largest absolute value from the enumerable, in a single pass without memoization.
        /// Returns NaN if data is empty or any entry is NaN.
        /// </summary>
        /// <param name="stream">Sample stream, no sorting is assumed.</param>
        public static Complex MaximumMagnitudePhase(IEnumerable<Complex> stream)
        {
            double maxMagnitude = 0.0d;
            Complex max = Complex.Zero;
            bool any = false;

            foreach (var d in stream)
            {
                double magnitude = d.Magnitude;
                if (double.IsNaN(magnitude))
                {
                    return new Complex(double.NaN, double.NaN);
                }
                if (magnitude > maxMagnitude || magnitude == maxMagnitude && d.Phase > max.Phase)
                {
                    maxMagnitude = magnitude;
                    max = d;
                }

                any = true;
            }

            return any ? max : new Complex(double.NaN, double.NaN);
        }

        /// <summary>
        /// Returns the largest absolute value from the enumerable, in a single pass without memoization.
        /// Returns NaN if data is empty or any entry is NaN.
        /// </summary>
        /// <param name="stream">Sample stream, no sorting is assumed.</param>
        public static Complex32 MaximumMagnitudePhase(IEnumerable<Complex32> stream)
        {
            float maxMagnitude = 0.0f;
            Complex32 max = Complex32.Zero;
            bool any = false;

            foreach (var d in stream)
            {
                float magnitude = d.Magnitude;
                if (float.IsNaN(magnitude))
                {
                    return new Complex32(float.NaN, float.NaN);
                }
                if (magnitude > maxMagnitude || magnitude == maxMagnitude && d.Phase > max.Phase)
                {
                    maxMagnitude = magnitude;
                    max = d;
                }

                any = true;
            }

            return any ? max : new Complex32(float.NaN, float.NaN);
        }

        /// <summary>
        /// Estimates the arithmetic sample mean from the enumerable, in a single pass without memoization.
        /// Returns NaN if data is empty or any entry is NaN.
        /// </summary>
        /// <param name="stream">Sample stream, no sorting is assumed.</param>
        public static double Mean(IEnumerable<double> stream)
        {
            double mean = 0;
            ulong m = 0;
            bool any = false;

            foreach (var d in stream)
            {
                mean += (d - mean)/++m;
                any = true;
            }

            return any ? mean : double.NaN;
        }

        /// <summary>
        /// Estimates the arithmetic sample mean from the enumerable, in a single pass without memoization.
        /// Returns NaN if data is empty or any entry is NaN.
        /// </summary>
        /// <param name="stream">Sample stream, no sorting is assumed.</param>
        public static double Mean(IEnumerable<float> stream)
        {
            return Mean(stream.Select(x => (double)x));
        }

        /// <summary>
        /// Evaluates the geometric mean of the enumerable, in a single pass without memoization.
        /// Returns NaN if data is empty or any entry is NaN.
        /// </summary>
        /// <param name="stream">Sample stream, no sorting is assumed.</param>
        public static double GeometricMean(IEnumerable<double> stream)
        {
            ulong m = 0;
            double sum = 0;

            foreach (var d in stream)
            {
                sum += Math.Log(d);
                m++;
            }

            return m > 0 ? Math.Exp(sum / m) : double.NaN;
        }

        /// <summary>
        /// Evaluates the geometric mean of the enumerable, in a single pass without memoization.
        /// Returns NaN if data is empty or any entry is NaN.
        /// </summary>
        /// <param name="stream">Sample stream, no sorting is assumed.</param>
        public static double GeometricMean(IEnumerable<float> stream)
        {
            return GeometricMean(stream.Select(x => (double)x));
        }

        /// <summary>
        /// Evaluates the harmonic mean of the enumerable, in a single pass without memoization.
        /// Returns NaN if data is empty or any entry is NaN.
        /// </summary>
        /// <param name="stream">Sample stream, no sorting is assumed.</param>
        public static double HarmonicMean(IEnumerable<double> stream)
        {
            ulong m = 0;
            double sum = 0;

            foreach (var d in stream)
            {
                sum += 1.0 / d;
                m++;
            }

            return m > 0 ? m/sum : double.NaN;
        }

        /// <summary>
        /// Evaluates the harmonic mean of the enumerable, in a single pass without memoization.
        /// Returns NaN if data is empty or any entry is NaN.
        /// </summary>
        /// <param name="stream">Sample stream, no sorting is assumed.</param>
        public static double HarmonicMean(IEnumerable<float> stream)
        {
            return HarmonicMean(stream.Select(x => (double)x));
        }

        /// <summary>
        /// Estimates the unbiased population variance from the provided samples as enumerable sequence, in a single pass without memoization.
        /// On a dataset of size N will use an N-1 normalizer (Bessel's correction).
        /// Returns NaN if data has less than two entries or if any entry is NaN.
        /// </summary>
        /// <param name="samples">Sample stream, no sorting is assumed.</param>
        public static double Variance(IEnumerable<double> samples)
        {
            double variance = 0;
            double sum = 0;
            ulong count = 0;

            using (var iterator = samples.GetEnumerator())
            {
                if (iterator.MoveNext())
                {
                    count++;
                    sum = iterator.Current;
                }

                while (iterator.MoveNext())
                {
                    count++;
                    double xi = iterator.Current;
                    sum += xi;
                    double diff = (count*xi) - sum;
                    variance += (diff*diff)/(count*(count - 1));
                }
            }

            return count > 1 ? variance/(count - 1) : double.NaN;
        }

        /// <summary>
        /// Estimates the unbiased population variance from the provided samples as enumerable sequence, in a single pass without memoization.
        /// On a dataset of size N will use an N-1 normalizer (Bessel's correction).
        /// Returns NaN if data has less than two entries or if any entry is NaN.
        /// </summary>
        /// <param name="samples">Sample stream, no sorting is assumed.</param>
        public static double Variance(IEnumerable<float> samples)
        {
            return Variance(samples.Select(x => (double)x));
        }

        /// <summary>
        /// Evaluates the population variance from the full population provided as enumerable sequence, in a single pass without memoization.
        /// On a dataset of size N will use an N normalizer and would thus be biased if applied to a subset.
        /// Returns NaN if data is empty or if any entry is NaN.
        /// </summary>
        /// <param name="population">Sample stream, no sorting is assumed.</param>
        public static double PopulationVariance(IEnumerable<double> population)
        {
            double variance = 0;
            double sum = 0;
            ulong count = 0;

            using (var iterator = population.GetEnumerator())
            {
                if (iterator.MoveNext())
                {
                    count++;
                    sum = iterator.Current;
                }

                while (iterator.MoveNext())
                {
                    count++;
                    double xi = iterator.Current;
                    sum += xi;
                    double diff = (count*xi) - sum;
                    variance += (diff*diff)/(count*(count - 1));
                }
            }

            return variance/count;
        }

        /// <summary>
        /// Evaluates the population variance from the full population provided as enumerable sequence, in a single pass without memoization.
        /// On a dataset of size N will use an N normalizer and would thus be biased if applied to a subset.
        /// Returns NaN if data is empty or if any entry is NaN.
        /// </summary>
        /// <param name="population">Sample stream, no sorting is assumed.</param>
        public static double PopulationVariance(IEnumerable<float> population)
        {
            return PopulationVariance(population.Select(x => (double)x));
        }

        /// <summary>
        /// Estimates the unbiased population standard deviation from the provided samples as enumerable sequence, in a single pass without memoization.
        /// On a dataset of size N will use an N-1 normalizer (Bessel's correction).
        /// Returns NaN if data has less than two entries or if any entry is NaN.
        /// </summary>
        /// <param name="samples">Sample stream, no sorting is assumed.</param>
        public static double StandardDeviation(IEnumerable<double> samples)
        {
            return Math.Sqrt(Variance(samples));
        }

        /// <summary>
        /// Estimates the unbiased population standard deviation from the provided samples as enumerable sequence, in a single pass without memoization.
        /// On a dataset of size N will use an N-1 normalizer (Bessel's correction).
        /// Returns NaN if data has less than two entries or if any entry is NaN.
        /// </summary>
        /// <param name="samples">Sample stream, no sorting is assumed.</param>
        public static double StandardDeviation(IEnumerable<float> samples)
        {
            return Math.Sqrt(Variance(samples));
        }

        /// <summary>
        /// Evaluates the population standard deviation from the full population provided as enumerable sequence, in a single pass without memoization.
        /// On a dataset of size N will use an N normalizer and would thus be biased if applied to a subset.
        /// Returns NaN if data is empty or if any entry is NaN.
        /// </summary>
        /// <param name="population">Sample stream, no sorting is assumed.</param>
        public static double PopulationStandardDeviation(IEnumerable<double> population)
        {
            return Math.Sqrt(PopulationVariance(population));
        }

        /// <summary>
        /// Evaluates the population standard deviation from the full population provided as enumerable sequence, in a single pass without memoization.
        /// On a dataset of size N will use an N normalizer and would thus be biased if applied to a subset.
        /// Returns NaN if data is empty or if any entry is NaN.
        /// </summary>
        /// <param name="population">Sample stream, no sorting is assumed.</param>
        public static double PopulationStandardDeviation(IEnumerable<float> population)
        {
            return Math.Sqrt(PopulationVariance(population));
        }

        /// <summary>
        /// Estimates the arithmetic sample mean and the unbiased population variance from the provided samples as enumerable sequence, in a single pass without memoization.
        /// On a dataset of size N will use an N-1 normalizer (Bessel's correction).
        /// Returns NaN for mean if data is empty or any entry is NaN, and NaN for variance if data has less than two entries or if any entry is NaN.
        /// </summary>
        /// <param name="samples">Sample stream, no sorting is assumed.</param>
        public static (double Mean, double Variance) MeanVariance(IEnumerable<double> samples)
        {
            double mean = 0;
            double variance = 0;
            double sum = 0;
            ulong count = 0;

            using (var iterator = samples.GetEnumerator())
            {
                if (iterator.MoveNext())
                {
                    count++;
                    sum = mean = iterator.Current;
                }

                while (iterator.MoveNext())
                {
                    count++;
                    double xi = iterator.Current;
                    sum += xi;
                    double diff = (count * xi) - sum;
                    variance += (diff * diff) / (count * (count - 1));
                    mean += (xi - mean) / count;
                }
            }

            return (count > 0 ? mean : double.NaN, count > 1 ? variance/(count - 1) : double.NaN);
        }

        /// <summary>
        /// Estimates the arithmetic sample mean and the unbiased population variance from the provided samples as enumerable sequence, in a single pass without memoization.
        /// On a dataset of size N will use an N-1 normalizer (Bessel's correction).
        /// Returns NaN for mean if data is empty or any entry is NaN, and NaN for variance if data has less than two entries or if any entry is NaN.
        /// </summary>
        /// <param name="samples">Sample stream, no sorting is assumed.</param>
        public static (double Mean, double Variance) MeanVariance(IEnumerable<float> samples)
        {
            return MeanVariance(samples.Select(x => (double)x));
        }

        /// <summary>
        /// Estimates the arithmetic sample mean and the unbiased population standard deviation from the provided samples as enumerable sequence, in a single pass without memoization.
        /// On a dataset of size N will use an N-1 normalizer (Bessel's correction).
        /// Returns NaN for mean if data is empty or any entry is NaN, and NaN for standard deviation if data has less than two entries or if any entry is NaN.
        /// </summary>
        /// <param name="samples">Sample stream, no sorting is assumed.</param>
        public static (double Mean, double StandardDeviation) MeanStandardDeviation(IEnumerable<double> samples)
        {
            var meanVariance = MeanVariance(samples);
            return (meanVariance.Item1, Math.Sqrt(meanVariance.Item2));
        }

        /// <summary>
        /// Estimates the arithmetic sample mean and the unbiased population standard deviation from the provided samples as enumerable sequence, in a single pass without memoization.
        /// On a dataset of size N will use an N-1 normalizer (Bessel's correction).
        /// Returns NaN for mean if data is empty or any entry is NaN, and NaN for standard deviation if data has less than two entries or if any entry is NaN.
        /// </summary>
        /// <param name="samples">Sample stream, no sorting is assumed.</param>
        public static (double Mean, double StandardDeviation) MeanStandardDeviation(IEnumerable<float> samples)
        {
            return MeanStandardDeviation(samples.Select(x => (double)x));
        }

        /// <summary>
        /// Estimates the unbiased population covariance from the provided two sample enumerable sequences, in a single pass without memoization.
        /// On a dataset of size N will use an N-1 normalizer (Bessel's correction).
        /// Returns NaN if data has less than two entries or if any entry is NaN.
        /// </summary>
        /// <param name="samples1">First sample stream.</param>
        /// <param name="samples2">Second sample stream.</param>
        public static double Covariance(IEnumerable<double> samples1, IEnumerable<double> samples2)
        {
            // https://en.wikipedia.org/wiki/Algorithms_for_calculating_variance
            var n = 0;
            var mean1 = 0.0;
            var mean2 = 0.0;
            var comoment = 0.0;

            using (var s1 = samples1.GetEnumerator())
            using (var s2 = samples2.GetEnumerator())
            {
                while (s1.MoveNext())
                {
                    if (!s2.MoveNext())
                    {
                        throw new ArgumentException("All vectors must have the same dimensionality.");
                    }

                    var mean2Prev = mean2;
                    n++;
                    mean1 += (s1.Current - mean1)/n;
                    mean2 += (s2.Current - mean2)/n;
                    comoment += (s1.Current - mean1)*(s2.Current - mean2Prev);
                }

                if (s2.MoveNext())
                {
                    throw new ArgumentException("All vectors must have the same dimensionality.");
                }
            }

            return n > 1 ? comoment/(n - 1) : double.NaN;
        }

        /// <summary>
        /// Estimates the unbiased population covariance from the provided two sample enumerable sequences, in a single pass without memoization.
        /// On a dataset of size N will use an N-1 normalizer (Bessel's correction).
        /// Returns NaN if data has less than two entries or if any entry is NaN.
        /// </summary>
        /// <param name="samples1">First sample stream.</param>
        /// <param name="samples2">Second sample stream.</param>
        public static double Covariance(IEnumerable<float> samples1, IEnumerable<float> samples2)
        {
            return Covariance(samples1.Select(x => (double)x), samples2.Select(x => (double)x));
        }

        /// <summary>
        /// Evaluates the population covariance from the full population provided as two enumerable sequences, in a single pass without memoization.
        /// On a dataset of size N will use an N normalizer and would thus be biased if applied to a subset.
        /// Returns NaN if data is empty or if any entry is NaN.
        /// </summary>
        /// <param name="population1">First population stream.</param>
        /// <param name="population2">Second population stream.</param>
        public static double PopulationCovariance(IEnumerable<double> population1, IEnumerable<double> population2)
        {
            // https://en.wikipedia.org/wiki/Algorithms_for_calculating_variance
            var n = 0;
            var mean1 = 0.0;
            var mean2 = 0.0;
            var comoment = 0.0;

            using (var p1 = population1.GetEnumerator())
            using (var p2 = population2.GetEnumerator())
            {
                while (p1.MoveNext())
                {
                    if (!p2.MoveNext())
                    {
                        throw new ArgumentException("All vectors must have the same dimensionality.");
                    }

                    var mean2Prev = mean2;
                    n++;
                    mean1 += (p1.Current - mean1) / n;
                    mean2 += (p2.Current - mean2) / n;
                    comoment += (p1.Current - mean1) * (p2.Current - mean2Prev);
                }

                if (p2.MoveNext())
                {
                    throw new ArgumentException("All vectors must have the same dimensionality.");
                }
            }

            return comoment/n;
        }

        /// <summary>
        /// Evaluates the population covariance from the full population provided as two enumerable sequences, in a single pass without memoization.
        /// On a dataset of size N will use an N normalizer and would thus be biased if applied to a subset.
        /// Returns NaN if data is empty or if any entry is NaN.
        /// </summary>
        /// <param name="population1">First population stream.</param>
        /// <param name="population2">Second population stream.</param>
        public static double PopulationCovariance(IEnumerable<float> population1, IEnumerable<float> population2)
        {
            return PopulationCovariance(population1.Select(x => (double)x), population2.Select(x => (double)x));
        }

        /// <summary>
        /// Estimates the root mean square (RMS) also known as quadratic mean from the enumerable, in a single pass without memoization.
        /// Returns NaN if data is empty or any entry is NaN.
        /// </summary>
        /// <param name="stream">Sample stream, no sorting is assumed.</param>
        public static double RootMeanSquare(IEnumerable<double> stream)
        {
            double mean = 0;
            ulong m = 0;
            bool any = false;

            foreach (var d in stream)
            {
                mean += (d*d - mean)/++m;
                any = true;
            }

            return any ? Math.Sqrt(mean) : double.NaN;
        }

        /// <summary>
        /// Estimates the root mean square (RMS) also known as quadratic mean from the enumerable, in a single pass without memoization.
        /// Returns NaN if data is empty or any entry is NaN.
        /// </summary>
        /// <param name="stream">Sample stream, no sorting is assumed.</param>
        public static double RootMeanSquare(IEnumerable<float> stream)
        {
            return RootMeanSquare(stream.Select(x => (double)x));
        }

        /// <summary>
        /// Calculates the entropy of a stream of double values.
        /// Returns NaN if any of the values in the stream are NaN.
        /// </summary>
        /// <param name="stream">The input stream to evaluate.</param>
        /// <returns></returns>
        public static double Entropy(IEnumerable<double> stream)
        {
            // http://en.wikipedia.org/wiki/Shannon_entropy

            var index = new Dictionary<double, double>();

            // count the number of occurrences of each item in the stream
            int totalCount = 0;
            foreach (double value in stream)
            {
                if (double.IsNaN(value))
                {
                    return double.NaN;
                }

                double currentValueCount;
                if (index.TryGetValue(value, out currentValueCount))
                {
                    index[value] = ++currentValueCount;
                }
                else
                {
                    index.Add(value, 1);
                }

                ++totalCount;
            }

            // calculate the entropy of the stream
            double entropy = 0;
            foreach (var item in index)
            {
                double p = item.Value / totalCount;
                entropy += p * Math.Log(p, 2);
            }

            return -entropy;
        }
    }
}
