// <copyright file="ArrayStatistics.Int32.cs" company="Math.NET">
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

namespace MathNet.Numerics.Statistics
{
    public static partial class ArrayStatistics
    {
        /// <summary>
        /// Estimates the arithmetic sample mean from the unsorted data array.
        /// Returns NaN if data is empty or any entry is NaN.
        /// </summary>
        /// <param name="data">Sample array, no sorting is assumed.</param>
        public static double Mean(int[] data)
        {
            if (data.Length == 0)
            {
                return double.NaN;
            }

            double mean = 0;
            ulong m = 0;
            for (int i = 0; i < data.Length; i++)
            {
                mean += (data[i] - mean) / ++m;
            }

            return mean;
        }

        /// <summary>
        /// Evaluates the geometric mean of the unsorted data array.
        /// Returns NaN if data is empty or any entry is NaN.
        /// </summary>
        /// <param name="data">Sample array, no sorting is assumed.</param>
        public static double GeometricMean(int[] data)
        {
            if (data.Length == 0)
            {
                return double.NaN;
            }

            double sum = 0;
            for (int i = 0; i < data.Length; i++)
            {
                sum += Math.Log(data[i]);
            }

            return Math.Exp(sum / data.Length);
        }

        /// <summary>
        /// Evaluates the harmonic mean of the unsorted data array.
        /// Returns NaN if data is empty or any entry is NaN.
        /// </summary>
        /// <param name="data">Sample array, no sorting is assumed.</param>
        public static double HarmonicMean(int[] data)
        {
            if (data.Length == 0)
            {
                return double.NaN;
            }

            double sum = 0;
            for (int i = 0; i < data.Length; i++)
            {
                sum += 1.0 / data[i];
            }

            return data.Length / sum;
        }

        /// <summary>
        /// Estimates the unbiased population variance from the provided samples as unsorted array.
        /// On a dataset of size N will use an N-1 normalizer (Bessel's correction).
        /// Returns NaN if data has less than two entries or if any entry is NaN.
        /// </summary>
        /// <param name="samples">Sample array, no sorting is assumed.</param>
        public static double Variance(int[] samples)
        {
            if (samples.Length <= 1)
            {
                return double.NaN;
            }

            double variance = 0;
            double t = samples[0];
            for (int i = 1; i < samples.Length; i++)
            {
                t += samples[i];
                double diff = ((i + 1) * samples[i]) - t;
                variance += (diff * diff) / ((i + 1.0) * i);
            }

            return variance / (samples.Length - 1);
        }

        /// <summary>
        /// Evaluates the population variance from the full population provided as unsorted array.
        /// On a dataset of size N will use an N normalizer and would thus be biased if applied to a subset.
        /// Returns NaN if data is empty or if any entry is NaN.
        /// </summary>
        /// <param name="population">Sample array, no sorting is assumed.</param>
        public static double PopulationVariance(int[] population)
        {
            if (population.Length == 0)
            {
                return double.NaN;
            }

            double variance = 0;
            double t = population[0];
            for (int i = 1; i < population.Length; i++)
            {
                t += population[i];
                double diff = ((i + 1) * population[i]) - t;
                variance += (diff * diff) / ((i + 1.0) * i);
            }

            return variance / population.Length;
        }

        /// <summary>
        /// Estimates the unbiased population standard deviation from the provided samples as unsorted array.
        /// On a dataset of size N will use an N-1 normalizer (Bessel's correction).
        /// Returns NaN if data has less than two entries or if any entry is NaN.
        /// </summary>
        /// <param name="samples">Sample array, no sorting is assumed.</param>
        public static double StandardDeviation(int[] samples)
        {
            return Math.Sqrt(Variance(samples));
        }

        /// <summary>
        /// Evaluates the population standard deviation from the full population provided as unsorted array.
        /// On a dataset of size N will use an N normalizer and would thus be biased if applied to a subset.
        /// Returns NaN if data is empty or if any entry is NaN.
        /// </summary>
        /// <param name="population">Sample array, no sorting is assumed.</param>
        public static double PopulationStandardDeviation(int[] population)
        {
            return Math.Sqrt(PopulationVariance(population));
        }

        /// <summary>
        /// Estimates the arithmetic sample mean and the unbiased population variance from the provided samples as unsorted array.
        /// On a dataset of size N will use an N-1 normalizer (Bessel's correction).
        /// Returns NaN for mean if data is empty or any entry is NaN and NaN for variance if data has less than two entries or if any entry is NaN.
        /// </summary>
        /// <param name="samples">Sample array, no sorting is assumed.</param>
        public static (double Mean, double Variance) MeanVariance(int[] samples)
        {
            return (Mean(samples), Variance(samples));
        }

        /// <summary>
        /// Estimates the arithmetic sample mean and the unbiased population standard deviation from the provided samples as unsorted array.
        /// On a dataset of size N will use an N-1 normalizer (Bessel's correction).
        /// Returns NaN for mean if data is empty or any entry is NaN and NaN for standard deviation if data has less than two entries or if any entry is NaN.
        /// </summary>
        /// <param name="samples">Sample array, no sorting is assumed.</param>
        public static (double Mean, double StandardDeviation) MeanStandardDeviation(int[] samples)
        {
            return (Mean(samples), StandardDeviation(samples));
        }

        /// <summary>
        /// Estimates the unbiased population covariance from the provided two sample arrays.
        /// On a dataset of size N will use an N-1 normalizer (Bessel's correction).
        /// Returns NaN if data has less than two entries or if any entry is NaN.
        /// </summary>
        /// <param name="samples1">First sample array.</param>
        /// <param name="samples2">Second sample array.</param>
        public static double Covariance(int[] samples1, int[] samples2)
        {
            if (samples1.Length != samples2.Length)
            {
                throw new ArgumentException("All vectors must have the same dimensionality.");
            }

            if (samples1.Length <= 1)
            {
                return double.NaN;
            }

            double mean1 = Mean(samples1);
            double mean2 = Mean(samples2);
            double covariance = 0.0;
            for (int i = 0; i < samples1.Length; i++)
            {
                covariance += (samples1[i] - mean1) * (samples2[i] - mean2);
            }

            return covariance / (samples1.Length - 1);
        }

        /// <summary>
        /// Evaluates the population covariance from the full population provided as two arrays.
        /// On a dataset of size N will use an N normalizer and would thus be biased if applied to a subset.
        /// Returns NaN if data is empty or if any entry is NaN.
        /// </summary>
        /// <param name="population1">First population array.</param>
        /// <param name="population2">Second population array.</param>
        public static double PopulationCovariance(int[] population1, int[] population2)
        {
            if (population1.Length != population2.Length)
            {
                throw new ArgumentException("All vectors must have the same dimensionality.");
            }

            if (population1.Length == 0)
            {
                return double.NaN;
            }

            double mean1 = Mean(population1);
            double mean2 = Mean(population2);
            double covariance = 0.0;
            for (int i = 0; i < population1.Length; i++)
            {
                covariance += (population1[i] - mean1) * (population2[i] - mean2);
            }

            return covariance / population1.Length;
        }

        /// <summary>
        /// Estimates the root mean square (RMS) also known as quadratic mean from the unsorted data array.
        /// Returns NaN if data is empty or any entry is NaN.
        /// </summary>
        /// <param name="data">Sample array, no sorting is assumed.</param>
        public static double RootMeanSquare(int[] data)
        {
            if (data.Length == 0)
            {
                return double.NaN;
            }

            double mean = 0;
            ulong m = 0;
            for (int i = 0; i < data.Length; i++)
            {
                mean += (data[i] * data[i] - mean) / ++m;
            }

            return Math.Sqrt(mean);
        }
    }
}
