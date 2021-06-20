// <copyright file="Statistics.cs" company="Math.NET">
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
    /// Extension methods to return basic statistics on set of data.
    /// </summary>
    public static class Statistics
    {
        /// <summary>
        /// Returns the minimum value in the sample data.
        /// Returns NaN if data is empty or if any entry is NaN.
        /// </summary>
        /// <param name="data">The sample data.</param>
        /// <returns>The minimum value in the sample data.</returns>
        public static double Minimum(this IEnumerable<double> data)
        {
            return data is double[] array
                ? ArrayStatistics.Minimum(array)
                : StreamingStatistics.Minimum(data);
        }

        /// <summary>
        /// Returns the minimum value in the sample data.
        /// Returns NaN if data is empty or if any entry is NaN.
        /// </summary>
        /// <param name="data">The sample data.</param>
        /// <returns>The minimum value in the sample data.</returns>
        public static float Minimum(this IEnumerable<float> data)
        {
            return data is float[] array
                ? ArrayStatistics.Minimum(array)
                : StreamingStatistics.Minimum(data);
        }


        /// <summary>
        /// Returns the minimum value in the sample data.
        /// Returns NaN if data is empty or if any entry is NaN.
        /// Null-entries are ignored.
        /// </summary>
        /// <param name="data">The sample data.</param>
        /// <returns>The minimum value in the sample data.</returns>
        public static double Minimum(this IEnumerable<double?> data)
        {
            return StreamingStatistics.Minimum(data.Where(d => d.HasValue).Select(d => d.Value));
        }

        /// <summary>
        /// Returns the maximum value in the sample data.
        /// Returns NaN if data is empty or if any entry is NaN.
        /// </summary>
        /// <param name="data">The sample data.</param>
        /// <returns>The maximum value in the sample data.</returns>
        public static double Maximum(this IEnumerable<double> data)
        {
            return data is double[] array
                ? ArrayStatistics.Maximum(array)
                : StreamingStatistics.Maximum(data);
        }

        /// <summary>
        /// Returns the maximum value in the sample data.
        /// Returns NaN if data is empty or if any entry is NaN.
        /// </summary>
        /// <param name="data">The sample data.</param>
        /// <returns>The maximum value in the sample data.</returns>
        public static float Maximum(this IEnumerable<float> data)
        {
            return data is float[] array
                ? ArrayStatistics.Maximum(array)
                : StreamingStatistics.Maximum(data);
        }

        /// <summary>
        /// Returns the maximum value in the sample data.
        /// Returns NaN if data is empty or if any entry is NaN.
        /// Null-entries are ignored.
        /// </summary>
        /// <param name="data">The sample data.</param>
        /// <returns>The maximum value in the sample data.</returns>
        public static double Maximum(this IEnumerable<double?> data)
        {
            return StreamingStatistics.Maximum(data.Where(d => d.HasValue).Select(d => d.Value));
        }

        /// <summary>
        /// Returns the minimum absolute value in the sample data.
        /// Returns NaN if data is empty or if any entry is NaN.
        /// </summary>
        /// <param name="data">The sample data.</param>
        /// <returns>The minimum value in the sample data.</returns>
        public static double MinimumAbsolute(this IEnumerable<double> data)
        {
            return data is double[] array
                ? ArrayStatistics.MinimumAbsolute(array)
                : StreamingStatistics.MinimumAbsolute(data);
        }

        /// <summary>
        /// Returns the minimum absolute value in the sample data.
        /// Returns NaN if data is empty or if any entry is NaN.
        /// </summary>
        /// <param name="data">The sample data.</param>
        /// <returns>The minimum value in the sample data.</returns>
        public static float MinimumAbsolute(this IEnumerable<float> data)
        {
            return data is float[] array
                ? ArrayStatistics.MinimumAbsolute(array)
                : StreamingStatistics.MinimumAbsolute(data);
        }

        /// <summary>
        /// Returns the maximum absolute value in the sample data.
        /// Returns NaN if data is empty or if any entry is NaN.
        /// </summary>
        /// <param name="data">The sample data.</param>
        /// <returns>The maximum value in the sample data.</returns>
        public static double MaximumAbsolute(this IEnumerable<double> data)
        {
            return data is double[] array
                ? ArrayStatistics.MaximumAbsolute(array)
                : StreamingStatistics.MaximumAbsolute(data);
        }

        /// <summary>
        /// Returns the maximum absolute value in the sample data.
        /// Returns NaN if data is empty or if any entry is NaN.
        /// </summary>
        /// <param name="data">The sample data.</param>
        /// <returns>The maximum value in the sample data.</returns>
        public static float MaximumAbsolute(this IEnumerable<float> data)
        {
            return data is float[] array
                ? ArrayStatistics.MaximumAbsolute(array)
                : StreamingStatistics.MaximumAbsolute(data);
        }

        /// <summary>
        /// Returns the minimum magnitude and phase value in the sample data.
        /// Returns NaN if data is empty or if any entry is NaN.
        /// </summary>
        /// <param name="data">The sample data.</param>
        /// <returns>The minimum value in the sample data.</returns>
        public static Complex MinimumMagnitudePhase(this IEnumerable<Complex> data)
        {
            return data is Complex[] array
                ? ArrayStatistics.MinimumMagnitudePhase(array)
                : StreamingStatistics.MinimumMagnitudePhase(data);
        }

        /// <summary>
        /// Returns the minimum magnitude and phase value in the sample data.
        /// Returns NaN if data is empty or if any entry is NaN.
        /// </summary>
        /// <param name="data">The sample data.</param>
        /// <returns>The minimum value in the sample data.</returns>
        public static Complex32 MinimumMagnitudePhase(this IEnumerable<Complex32> data)
        {
            return data is Complex32[] array
                ? ArrayStatistics.MinimumMagnitudePhase(array)
                : StreamingStatistics.MinimumMagnitudePhase(data);
        }

        /// <summary>
        /// Returns the maximum magnitude and phase value in the sample data.
        /// Returns NaN if data is empty or if any entry is NaN.
        /// </summary>
        /// <param name="data">The sample data.</param>
        /// <returns>The minimum value in the sample data.</returns>
        public static Complex MaximumMagnitudePhase(this IEnumerable<Complex> data)
        {
            return data is Complex[] array
                ? ArrayStatistics.MaximumMagnitudePhase(array)
                : StreamingStatistics.MaximumMagnitudePhase(data);
        }

        /// <summary>
        /// Returns the maximum magnitude and phase value in the sample data.
        /// Returns NaN if data is empty or if any entry is NaN.
        /// </summary>
        /// <param name="data">The sample data.</param>
        /// <returns>The minimum value in the sample data.</returns>
        public static Complex32 MaximumMagnitudePhase(this IEnumerable<Complex32> data)
        {
            return data is Complex32[] array
                ? ArrayStatistics.MaximumMagnitudePhase(array)
                : StreamingStatistics.MaximumMagnitudePhase(data);
        }

        /// <summary>
        /// Evaluates the sample mean, an estimate of the population mean.
        /// Returns NaN if data is empty or if any entry is NaN.
        /// </summary>
        /// <param name="data">The data to calculate the mean of.</param>
        /// <returns>The mean of the sample.</returns>
        public static double Mean(this IEnumerable<double> data)
        {
            return data is double[] array
                ? ArrayStatistics.Mean(array)
                : StreamingStatistics.Mean(data);
        }

        /// <summary>
        /// Evaluates the sample mean, an estimate of the population mean.
        /// Returns NaN if data is empty or if any entry is NaN.
        /// </summary>
        /// <param name="data">The data to calculate the mean of.</param>
        /// <returns>The mean of the sample.</returns>
        public static double Mean(this IEnumerable<float> data)
        {
            return data is float[] array
                ? ArrayStatistics.Mean(array)
                : StreamingStatistics.Mean(data);
        }

        /// <summary>
        /// Evaluates the sample mean, an estimate of the population mean.
        /// Returns NaN if data is empty or if any entry is NaN.
        /// Null-entries are ignored.
        /// </summary>
        /// <param name="data">The data to calculate the mean of.</param>
        /// <returns>The mean of the sample.</returns>
        public static double Mean(this IEnumerable<double?> data)
        {
            return StreamingStatistics.Mean(data.Where(d => d.HasValue).Select(d => d.Value));
        }

        /// <summary>
        /// Evaluates the geometric mean.
        /// Returns NaN if data is empty or if any entry is NaN.
        /// </summary>
        /// <param name="data">The data to calculate the geometric mean of.</param>
        /// <returns>The geometric mean of the sample.</returns>
        public static double GeometricMean(this IEnumerable<double> data)
        {
            return data is double[] array
                ? ArrayStatistics.GeometricMean(array)
                : StreamingStatistics.GeometricMean(data);
        }

        /// <summary>
        /// Evaluates the geometric mean.
        /// Returns NaN if data is empty or if any entry is NaN.
        /// </summary>
        /// <param name="data">The data to calculate the geometric mean of.</param>
        /// <returns>The geometric mean of the sample.</returns>
        public static double GeometricMean(this IEnumerable<float> data)
        {
            return data is float[] array
                ? ArrayStatistics.GeometricMean(array)
                : StreamingStatistics.GeometricMean(data);
        }

        /// <summary>
        /// Evaluates the harmonic mean.
        /// Returns NaN if data is empty or if any entry is NaN.
        /// </summary>
        /// <param name="data">The data to calculate the harmonic mean of.</param>
        /// <returns>The harmonic mean of the sample.</returns>
        public static double HarmonicMean(this IEnumerable<double> data)
        {
            return data is double[] array
                ? ArrayStatistics.HarmonicMean(array)
                : StreamingStatistics.HarmonicMean(data);
        }

        /// <summary>
        /// Evaluates the harmonic mean.
        /// Returns NaN if data is empty or if any entry is NaN.
        /// </summary>
        /// <param name="data">The data to calculate the harmonic mean of.</param>
        /// <returns>The harmonic mean of the sample.</returns>
        public static double HarmonicMean(this IEnumerable<float> data)
        {
            return data is float[] array
                ? ArrayStatistics.HarmonicMean(array)
                : StreamingStatistics.HarmonicMean(data);
        }

        /// <summary>
        /// Estimates the unbiased population variance from the provided samples.
        /// On a dataset of size N will use an N-1 normalizer (Bessel's correction).
        /// Returns NaN if data has less than two entries or if any entry is NaN.
        /// </summary>
        /// <param name="samples">A subset of samples, sampled from the full population.</param>
        public static double Variance(this IEnumerable<double> samples)
        {
            return samples is double[] array
                ? ArrayStatistics.Variance(array)
                : StreamingStatistics.Variance(samples);
        }

        /// <summary>
        /// Estimates the unbiased population variance from the provided samples.
        /// On a dataset of size N will use an N-1 normalizer (Bessel's correction).
        /// Returns NaN if data has less than two entries or if any entry is NaN.
        /// </summary>
        /// <param name="samples">A subset of samples, sampled from the full population.</param>
        public static double Variance(this IEnumerable<float> samples)
        {
            return samples is float[] array
                ? ArrayStatistics.Variance(array)
                : StreamingStatistics.Variance(samples);
        }

        /// <summary>
        /// Estimates the unbiased population variance from the provided samples.
        /// On a dataset of size N will use an N-1 normalizer (Bessel's correction).
        /// Returns NaN if data has less than two entries or if any entry is NaN.
        /// Null-entries are ignored.
        /// </summary>
        /// <param name="samples">A subset of samples, sampled from the full population.</param>
        public static double Variance(this IEnumerable<double?> samples)
        {
            return StreamingStatistics.Variance(samples.Where(d => d.HasValue).Select(d => d.Value));
        }

        /// <summary>
        /// Evaluates the variance from the provided full population.
        /// On a dataset of size N will use an N normalizer and would thus be biased if applied to a subset.
        /// Returns NaN if data is empty or if any entry is NaN.
        /// </summary>
        /// <param name="population">The full population data.</param>
        public static double PopulationVariance(this IEnumerable<double> population)
        {
            return population is double[] array
                ? ArrayStatistics.PopulationVariance(array)
                : StreamingStatistics.PopulationVariance(population);
        }

        /// <summary>
        /// Evaluates the variance from the provided full population.
        /// On a dataset of size N will use an N normalizer and would thus be biased if applied to a subset.
        /// Returns NaN if data is empty or if any entry is NaN.
        /// </summary>
        /// <param name="population">The full population data.</param>
        public static double PopulationVariance(this IEnumerable<float> population)
        {
            return population is float[] array
                ? ArrayStatistics.PopulationVariance(array)
                : StreamingStatistics.PopulationVariance(population);
        }

        /// <summary>
        /// Evaluates the variance from the provided full population.
        /// On a dataset of size N will use an N normalize and would thus be biased if applied to a subset.
        /// Returns NaN if data is empty or if any entry is NaN.
        /// Null-entries are ignored.
        /// </summary>
        /// <param name="population">The full population data.</param>
        public static double PopulationVariance(this IEnumerable<double?> population)
        {
            return StreamingStatistics.PopulationVariance(population.Where(d => d.HasValue).Select(d => d.Value));
        }

        /// <summary>
        /// Estimates the unbiased population standard deviation from the provided samples.
        /// On a dataset of size N will use an N-1 normalizer (Bessel's correction).
        /// Returns NaN if data has less than two entries or if any entry is NaN.
        /// </summary>
        /// <param name="samples">A subset of samples, sampled from the full population.</param>
        public static double StandardDeviation(this IEnumerable<double> samples)
        {
            return samples is double[] array
                ? ArrayStatistics.StandardDeviation(array)
                : StreamingStatistics.StandardDeviation(samples);
        }

        /// <summary>
        /// Estimates the unbiased population standard deviation from the provided samples.
        /// On a dataset of size N will use an N-1 normalizer (Bessel's correction).
        /// Returns NaN if data has less than two entries or if any entry is NaN.
        /// </summary>
        /// <param name="samples">A subset of samples, sampled from the full population.</param>
        public static double StandardDeviation(this IEnumerable<float> samples)
        {
            return samples is float[] array
                ? ArrayStatistics.StandardDeviation(array)
                : StreamingStatistics.StandardDeviation(samples);
        }

        /// <summary>
        /// Estimates the unbiased population standard deviation from the provided samples.
        /// On a dataset of size N will use an N-1 normalizer (Bessel's correction).
        /// Returns NaN if data has less than two entries or if any entry is NaN.
        /// Null-entries are ignored.
        /// </summary>
        /// <param name="samples">A subset of samples, sampled from the full population.</param>
        public static double StandardDeviation(this IEnumerable<double?> samples)
        {
            return StreamingStatistics.StandardDeviation(samples.Where(d => d.HasValue).Select(d => d.Value));
        }

        /// <summary>
        /// Evaluates the standard deviation from the provided full population.
        /// On a dataset of size N will use an N normalizer and would thus be biased if applied to a subset.
        /// Returns NaN if data is empty or if any entry is NaN.
        /// </summary>
        /// <param name="population">The full population data.</param>
        public static double PopulationStandardDeviation(this IEnumerable<double> population)
        {
            return population is double[] array
                ? ArrayStatistics.PopulationStandardDeviation(array)
                : StreamingStatistics.PopulationStandardDeviation(population);
        }

        /// <summary>
        /// Evaluates the standard deviation from the provided full population.
        /// On a dataset of size N will use an N normalizer and would thus be biased if applied to a subset.
        /// Returns NaN if data is empty or if any entry is NaN.
        /// </summary>
        /// <param name="population">The full population data.</param>
        public static double PopulationStandardDeviation(this IEnumerable<float> population)
        {
            return population is float[] array
                ? ArrayStatistics.PopulationStandardDeviation(array)
                : StreamingStatistics.PopulationStandardDeviation(population);
        }

        /// <summary>
        /// Evaluates the standard deviation from the provided full population.
        /// On a dataset of size N will use an N normalizer and would thus be biased if applied to a subset.
        /// Returns NaN if data is empty or if any entry is NaN.
        /// Null-entries are ignored.
        /// </summary>
        /// <param name="population">The full population data.</param>
        public static double PopulationStandardDeviation(this IEnumerable<double?> population)
        {
            return StreamingStatistics.PopulationStandardDeviation(population.Where(d => d.HasValue).Select(d => d.Value));
        }

        /// <summary>
        /// Estimates the unbiased population skewness from the provided samples.
        /// Uses a normalizer (Bessel's correction; type 2).
        /// Returns NaN if data has less than three entries or if any entry is NaN.
        /// </summary>
        /// <param name="samples">A subset of samples, sampled from the full population.</param>
        public static double Skewness(this IEnumerable<double> samples)
        {
            return new RunningStatistics(samples).Skewness;
        }

        /// <summary>
        /// Estimates the unbiased population skewness from the provided samples.
        /// Uses a normalizer (Bessel's correction; type 2).
        /// Returns NaN if data has less than three entries or if any entry is NaN.
        /// Null-entries are ignored.
        /// </summary>
        /// <param name="samples">A subset of samples, sampled from the full population.</param>
        public static double Skewness(this IEnumerable<double?> samples)
        {
            return new RunningStatistics(samples.Where(d => d.HasValue).Select(d => d.Value)).Skewness;
        }

        /// <summary>
        /// Evaluates the skewness from the full population.
        /// Does not use a normalizer and would thus be biased if applied to a subset (type 1).
        /// Returns NaN if data has less than two entries or if any entry is NaN.
        /// </summary>
        /// <param name="population">The full population data.</param>
        public static double PopulationSkewness(this IEnumerable<double> population)
        {
            return new RunningStatistics(population).PopulationSkewness;
        }

        /// <summary>
        /// Evaluates the skewness from the full population.
        /// Does not use a normalizer and would thus be biased if applied to a subset (type 1).
        /// Returns NaN if data has less than two entries or if any entry is NaN.
        /// Null-entries are ignored.
        /// </summary>
        /// <param name="population">The full population data.</param>
        public static double PopulationSkewness(this IEnumerable<double?> population)
        {
            return new RunningStatistics(population.Where(d => d.HasValue).Select(d => d.Value)).PopulationSkewness;
        }

        /// <summary>
        /// Estimates the unbiased population kurtosis from the provided samples.
        /// Uses a normalizer (Bessel's correction; type 2).
        /// Returns NaN if data has less than four entries or if any entry is NaN.
        /// </summary>
        /// <param name="samples">A subset of samples, sampled from the full population.</param>
        public static double Kurtosis(this IEnumerable<double> samples)
        {
            return new RunningStatistics(samples).Kurtosis;
        }

        /// <summary>
        /// Estimates the unbiased population kurtosis from the provided samples.
        /// Uses a normalizer (Bessel's correction; type 2).
        /// Returns NaN if data has less than four entries or if any entry is NaN.
        /// Null-entries are ignored.
        /// </summary>
        /// <param name="samples">A subset of samples, sampled from the full population.</param>
        public static double Kurtosis(this IEnumerable<double?> samples)
        {
            return new RunningStatistics(samples.Where(d => d.HasValue).Select(d => d.Value)).Kurtosis;
        }

        /// <summary>
        /// Evaluates the kurtosis from the full population.
        /// Does not use a normalizer and would thus be biased if applied to a subset (type 1).
        /// Returns NaN if data has less than three entries or if any entry is NaN.
        /// </summary>
        /// <param name="population">The full population data.</param>
        public static double PopulationKurtosis(this IEnumerable<double> population)
        {
            return new RunningStatistics(population).PopulationKurtosis;
        }

        /// <summary>
        /// Evaluates the kurtosis from the full population.
        /// Does not use a normalizer and would thus be biased if applied to a subset (type 1).
        /// Returns NaN if data has less than three entries or if any entry is NaN.
        /// Null-entries are ignored.
        /// </summary>
        /// <param name="population">The full population data.</param>
        public static double PopulationKurtosis(this IEnumerable<double?> population)
        {
            return new RunningStatistics(population.Where(d => d.HasValue).Select(d => d.Value)).PopulationKurtosis;
        }

        /// <summary>
        /// Estimates the sample mean and the unbiased population variance from the provided samples.
        /// On a dataset of size N will use an N-1 normalizer (Bessel's correction).
        /// Returns NaN for mean if data is empty or if any entry is NaN and NaN for variance if data has less than two entries or if any entry is NaN.
        /// </summary>
        /// <param name="samples">The data to calculate the mean of.</param>
        /// <returns>The mean of the sample.</returns>
        public static (double Mean, double Variance) MeanVariance(this IEnumerable<double> samples)
        {
            return samples is double[] array
                ? ArrayStatistics.MeanVariance(array)
                : StreamingStatistics.MeanVariance(samples);
        }

        /// <summary>
        /// Estimates the sample mean and the unbiased population variance from the provided samples.
        /// On a dataset of size N will use an N-1 normalizer (Bessel's correction).
        /// Returns NaN for mean if data is empty or if any entry is NaN and NaN for variance if data has less than two entries or if any entry is NaN.
        /// </summary>
        /// <param name="samples">The data to calculate the mean of.</param>
        /// <returns>The mean of the sample.</returns>
        public static (double Mean, double Variance) MeanVariance(this IEnumerable<float> samples)
        {
            return samples is float[] array
                ? ArrayStatistics.MeanVariance(array)
                : StreamingStatistics.MeanVariance(samples);
        }

        /// <summary>
        /// Estimates the sample mean and the unbiased population standard deviation from the provided samples.
        /// On a dataset of size N will use an N-1 normalizer (Bessel's correction).
        /// Returns NaN for mean if data is empty or if any entry is NaN and NaN for standard deviation if data has less than two entries or if any entry is NaN.
        /// </summary>
        /// <param name="samples">The data to calculate the mean of.</param>
        /// <returns>The mean of the sample.</returns>
        public static (double Mean, double StandardDeviation) MeanStandardDeviation(this IEnumerable<double> samples)
        {
            return samples is double[] array
                ? ArrayStatistics.MeanStandardDeviation(array)
                : StreamingStatistics.MeanStandardDeviation(samples);
        }

        /// <summary>
        /// Estimates the sample mean and the unbiased population standard deviation from the provided samples.
        /// On a dataset of size N will use an N-1 normalizer (Bessel's correction).
        /// Returns NaN for mean if data is empty or if any entry is NaN and NaN for standard deviation if data has less than two entries or if any entry is NaN.
        /// </summary>
        /// <param name="samples">The data to calculate the mean of.</param>
        /// <returns>The mean of the sample.</returns>
        public static (double Mean, double StandardDeviation) MeanStandardDeviation(this IEnumerable<float> samples)
        {
            return samples is float[] array
                ? ArrayStatistics.MeanStandardDeviation(array)
                : StreamingStatistics.MeanStandardDeviation(samples);
        }

        /// <summary>
        /// Estimates the unbiased population skewness and kurtosis from the provided samples in a single pass.
        /// Uses a normalizer (Bessel's correction; type 2).
        /// </summary>
        /// <param name="samples">A subset of samples, sampled from the full population.</param>
        public static (double Skewness, double Kurtosis) SkewnessKurtosis(this IEnumerable<double> samples)
        {
            var stats = new RunningStatistics(samples);
            return (stats.Skewness, stats.Kurtosis);
        }

        /// <summary>
        /// Evaluates the skewness and kurtosis from the full population.
        /// Does not use a normalizer and would thus be biased if applied to a subset (type 1).
        /// </summary>
        /// <param name="population">The full population data.</param>
        public static (double Skewness, double Kurtosis) PopulationSkewnessKurtosis(this IEnumerable<double> population)
        {
            var stats = new RunningStatistics(population);
            return (stats.PopulationSkewness, stats.PopulationKurtosis);
        }

        /// <summary>
        /// Estimates the unbiased population covariance from the provided samples.
        /// On a dataset of size N will use an N-1 normalizer (Bessel's correction).
        /// Returns NaN if data has less than two entries or if any entry is NaN.
        /// </summary>
        /// <param name="samples1">A subset of samples, sampled from the full population.</param>
        /// <param name="samples2">A subset of samples, sampled from the full population.</param>
        public static double Covariance(this IEnumerable<double> samples1, IEnumerable<double> samples2)
        {
            return samples1 is double[] array1 && samples2 is double[] array2
                ? ArrayStatistics.Covariance(array1, array2)
                : StreamingStatistics.Covariance(samples1, samples2);
        }

        /// <summary>
        /// Estimates the unbiased population covariance from the provided samples.
        /// On a dataset of size N will use an N-1 normalizer (Bessel's correction).
        /// Returns NaN if data has less than two entries or if any entry is NaN.
        /// </summary>
        /// <param name="samples1">A subset of samples, sampled from the full population.</param>
        /// <param name="samples2">A subset of samples, sampled from the full population.</param>
        public static double Covariance(this IEnumerable<float> samples1, IEnumerable<float> samples2)
        {
            return samples1 is float[] array1 && samples2 is float[] array2
                ? ArrayStatistics.Covariance(array1, array2)
                : StreamingStatistics.Covariance(samples1, samples2);
        }

        /// <summary>
        /// Estimates the unbiased population covariance from the provided samples.
        /// On a dataset of size N will use an N-1 normalizer (Bessel's correction).
        /// Returns NaN if data has less than two entries or if any entry is NaN.
        /// Null-entries are ignored.
        /// </summary>
        /// <param name="samples1">A subset of samples, sampled from the full population.</param>
        /// <param name="samples2">A subset of samples, sampled from the full population.</param>
        public static double Covariance(this IEnumerable<double?> samples1, IEnumerable<double?> samples2)
        {
            return StreamingStatistics.Covariance(samples1.Where(d => d.HasValue).Select(d => d.Value), samples2.Where(d => d.HasValue).Select(d => d.Value));
        }

        /// <summary>
        /// Evaluates the population covariance from the provided full populations.
        /// On a dataset of size N will use an N normalizer and would thus be biased if applied to a subset.
        /// Returns NaN if data is empty or if any entry is NaN.
        /// </summary>
        /// <param name="population1">The full population data.</param>
        /// <param name="population2">The full population data.</param>
        public static double PopulationCovariance(this IEnumerable<double> population1, IEnumerable<double> population2)
        {
            return population1 is double[] array1 && population2 is double[] array2
                ? ArrayStatistics.PopulationCovariance(array1, array2)
                : StreamingStatistics.PopulationCovariance(population1, population2);
        }

        /// <summary>
        /// Evaluates the population covariance from the provided full populations.
        /// On a dataset of size N will use an N normalizer and would thus be biased if applied to a subset.
        /// Returns NaN if data is empty or if any entry is NaN.
        /// </summary>
        /// <param name="population1">The full population data.</param>
        /// <param name="population2">The full population data.</param>
        public static double PopulationCovariance(this IEnumerable<float> population1, IEnumerable<float> population2)
        {
            return population1 is float[] array1 && population2 is float[] array2
                ? ArrayStatistics.PopulationCovariance(array1, array2)
                : StreamingStatistics.PopulationCovariance(population1, population2);
        }

        /// <summary>
        /// Evaluates the population covariance from the provided full populations.
        /// On a dataset of size N will use an N normalize and would thus be biased if applied to a subset.
        /// Returns NaN if data is empty or if any entry is NaN.
        /// Null-entries are ignored.
        /// </summary>
        /// <param name="population1">The full population data.</param>
        /// <param name="population2">The full population data.</param>
        public static double PopulationCovariance(this IEnumerable<double?> population1, IEnumerable<double?> population2)
        {
            return StreamingStatistics.PopulationCovariance(population1.Where(d => d.HasValue).Select(d => d.Value), population2.Where(d => d.HasValue).Select(d => d.Value));
        }

        /// <summary>
        /// Evaluates the root mean square (RMS) also known as quadratic mean.
        /// Returns NaN if data is empty or if any entry is NaN.
        /// </summary>
        /// <param name="data">The data to calculate the RMS of.</param>
        public static double RootMeanSquare(this IEnumerable<double> data)
        {
            return data is double[] array
                ? ArrayStatistics.RootMeanSquare(array)
                : StreamingStatistics.RootMeanSquare(data);
        }

        /// <summary>
        /// Evaluates the root mean square (RMS) also known as quadratic mean.
        /// Returns NaN if data is empty or if any entry is NaN.
        /// </summary>
        /// <param name="data">The data to calculate the RMS of.</param>
        public static double RootMeanSquare(this IEnumerable<float> data)
        {
            return data is float[] array
                ? ArrayStatistics.RootMeanSquare(array)
                : StreamingStatistics.RootMeanSquare(data);
        }

        /// <summary>
        /// Evaluates the root mean square (RMS) also known as quadratic mean.
        /// Returns NaN if data is empty or if any entry is NaN.
        /// Null-entries are ignored.
        /// </summary>
        /// <param name="data">The data to calculate the mean of.</param>
        public static double RootMeanSquare(this IEnumerable<double?> data)
        {
            return StreamingStatistics.RootMeanSquare(data.Where(d => d.HasValue).Select(d => d.Value));
        }

        /// <summary>
        /// Estimates the sample median from the provided samples (R8).
        /// </summary>
        /// <param name="data">The data sample sequence.</param>
        public static double Median(this IEnumerable<double> data)
        {
            double[] array = data.ToArray();
            return ArrayStatistics.MedianInplace(array);
        }

        /// <summary>
        /// Estimates the sample median from the provided samples (R8).
        /// </summary>
        /// <param name="data">The data sample sequence.</param>
        public static float Median(this IEnumerable<float> data)
        {
            float[] array = data.ToArray();
            return ArrayStatistics.MedianInplace(array);
        }

        /// <summary>
        /// Estimates the sample median from the provided samples (R8).
        /// </summary>
        /// <param name="data">The data sample sequence.</param>
        public static double Median(this IEnumerable<double?> data)
        {
            double[] array = data.Where(d => d.HasValue).Select(d => d.Value).ToArray();
            return ArrayStatistics.MedianInplace(array);
        }

        /// <summary>
        /// Estimates the tau-th quantile from the provided samples.
        /// The tau-th quantile is the data value where the cumulative distribution
        /// function crosses tau.
        /// Approximately median-unbiased regardless of the sample distribution (R8).
        /// </summary>
        /// <param name="data">The data sample sequence.</param>
        /// <param name="tau">Quantile selector, between 0.0 and 1.0 (inclusive).</param>
        public static double Quantile(this IEnumerable<double> data, double tau)
        {
            double[] array = data.ToArray();
            return ArrayStatistics.QuantileInplace(array, tau);
        }

        /// <summary>
        /// Estimates the tau-th quantile from the provided samples.
        /// The tau-th quantile is the data value where the cumulative distribution
        /// function crosses tau.
        /// Approximately median-unbiased regardless of the sample distribution (R8).
        /// </summary>
        /// <param name="data">The data sample sequence.</param>
        /// <param name="tau">Quantile selector, between 0.0 and 1.0 (inclusive).</param>
        public static float Quantile(this IEnumerable<float> data, double tau)
        {
            float[] array = data.ToArray();
            return ArrayStatistics.QuantileInplace(array, tau);
        }

        /// <summary>
        /// Estimates the tau-th quantile from the provided samples.
        /// The tau-th quantile is the data value where the cumulative distribution
        /// function crosses tau.
        /// Approximately median-unbiased regardless of the sample distribution (R8).
        /// </summary>
        /// <param name="data">The data sample sequence.</param>
        /// <param name="tau">Quantile selector, between 0.0 and 1.0 (inclusive).</param>
        public static double Quantile(this IEnumerable<double?> data, double tau)
        {
            double[] array = data.Where(d => d.HasValue).Select(d => d.Value).ToArray();
            return ArrayStatistics.QuantileInplace(array, tau);
        }

        /// <summary>
        /// Estimates the tau-th quantile from the provided samples.
        /// The tau-th quantile is the data value where the cumulative distribution
        /// function crosses tau.
        /// Approximately median-unbiased regardless of the sample distribution (R8).
        /// </summary>
        /// <param name="data">The data sample sequence.</param>
        public static Func<double, double> QuantileFunc(this IEnumerable<double> data)
        {
            double[] array = data.ToArray();
            Array.Sort(array);
            return tau => SortedArrayStatistics.Quantile(array, tau);
        }

        /// <summary>
        /// Estimates the tau-th quantile from the provided samples.
        /// The tau-th quantile is the data value where the cumulative distribution
        /// function crosses tau.
        /// Approximately median-unbiased regardless of the sample distribution (R8).
        /// </summary>
        /// <param name="data">The data sample sequence.</param>
        public static Func<float, float> QuantileFunc(this IEnumerable<float> data)
        {
            float[] array = data.ToArray();
            Array.Sort(array);
            return tau => SortedArrayStatistics.Quantile(array, tau);
        }

        /// <summary>
        /// Estimates the tau-th quantile from the provided samples.
        /// The tau-th quantile is the data value where the cumulative distribution
        /// function crosses tau.
        /// Approximately median-unbiased regardless of the sample distribution (R8).
        /// </summary>
        /// <param name="data">The data sample sequence.</param>
        public static Func<double, double> QuantileFunc(this IEnumerable<double?> data)
        {
            double[] array = data.Where(d => d.HasValue).Select(d => d.Value).ToArray();
            Array.Sort(array);
            return tau => SortedArrayStatistics.Quantile(array, tau);
        }

        /// <summary>
        /// Estimates the tau-th quantile from the provided samples.
        /// The tau-th quantile is the data value where the cumulative distribution
        /// function crosses tau. The quantile definition can be specified to be compatible
        /// with an existing system.
        /// </summary>
        /// <param name="data">The data sample sequence.</param>
        /// <param name="tau">Quantile selector, between 0.0 and 1.0 (inclusive).</param>
        /// <param name="definition">Quantile definition, to choose what product/definition it should be consistent with</param>
        public static double QuantileCustom(this IEnumerable<double> data, double tau, QuantileDefinition definition)
        {
            double[] array = data.ToArray();
            return ArrayStatistics.QuantileCustomInplace(array, tau, definition);
        }

        /// <summary>
        /// Estimates the tau-th quantile from the provided samples.
        /// The tau-th quantile is the data value where the cumulative distribution
        /// function crosses tau. The quantile definition can be specified to be compatible
        /// with an existing system.
        /// </summary>
        /// <param name="data">The data sample sequence.</param>
        /// <param name="tau">Quantile selector, between 0.0 and 1.0 (inclusive).</param>
        /// <param name="definition">Quantile definition, to choose what product/definition it should be consistent with</param>
        public static float QuantileCustom(this IEnumerable<float> data, double tau, QuantileDefinition definition)
        {
            float[] array = data.ToArray();
            return ArrayStatistics.QuantileCustomInplace(array, tau, definition);
        }

        /// <summary>
        /// Estimates the tau-th quantile from the provided samples.
        /// The tau-th quantile is the data value where the cumulative distribution
        /// function crosses tau. The quantile definition can be specified to be compatible
        /// with an existing system.
        /// </summary>
        /// <param name="data">The data sample sequence.</param>
        /// <param name="tau">Quantile selector, between 0.0 and 1.0 (inclusive).</param>
        /// <param name="definition">Quantile definition, to choose what product/definition it should be consistent with</param>
        public static double QuantileCustom(this IEnumerable<double?> data, double tau, QuantileDefinition definition)
        {
            double[] array = data.Where(d => d.HasValue).Select(d => d.Value).ToArray();
            return ArrayStatistics.QuantileCustomInplace(array, tau, definition);
        }

        /// <summary>
        /// Estimates the tau-th quantile from the provided samples.
        /// The tau-th quantile is the data value where the cumulative distribution
        /// function crosses tau. The quantile definition can be specified to be compatible
        /// with an existing system.
        /// </summary>
        /// <param name="data">The data sample sequence.</param>
        /// <param name="definition">Quantile definition, to choose what product/definition it should be consistent with</param>
        public static Func<double, double> QuantileCustomFunc(this IEnumerable<double> data, QuantileDefinition definition)
        {
            double[] array = data.ToArray();
            Array.Sort(array);
            return tau => SortedArrayStatistics.QuantileCustom(array, tau, definition);
        }

        /// <summary>
        /// Estimates the tau-th quantile from the provided samples.
        /// The tau-th quantile is the data value where the cumulative distribution
        /// function crosses tau. The quantile definition can be specified to be compatible
        /// with an existing system.
        /// </summary>
        /// <param name="data">The data sample sequence.</param>
        /// <param name="definition">Quantile definition, to choose what product/definition it should be consistent with</param>
        public static Func<float, float> QuantileCustomFunc(this IEnumerable<float> data, QuantileDefinition definition)
        {
            float[] array = data.ToArray();
            Array.Sort(array);
            return tau => SortedArrayStatistics.QuantileCustom(array, tau, definition);
        }

        /// <summary>
        /// Estimates the tau-th quantile from the provided samples.
        /// The tau-th quantile is the data value where the cumulative distribution
        /// function crosses tau. The quantile definition can be specified to be compatible
        /// with an existing system.
        /// </summary>
        /// <param name="data">The data sample sequence.</param>
        /// <param name="definition">Quantile definition, to choose what product/definition it should be consistent with</param>
        public static Func<double, double> QuantileCustomFunc(this IEnumerable<double?> data, QuantileDefinition definition)
        {
            double[] array = data.Where(d => d.HasValue).Select(d => d.Value).ToArray();
            Array.Sort(array);
            return tau => SortedArrayStatistics.QuantileCustom(array, tau, definition);
        }

        /// <summary>
        /// Estimates the p-Percentile value from the provided samples.
        /// If a non-integer Percentile is needed, use Quantile instead.
        /// Approximately median-unbiased regardless of the sample distribution (R8).
        /// </summary>
        /// <param name="data">The data sample sequence.</param>
        /// <param name="p">Percentile selector, between 0 and 100 (inclusive).</param>
        public static double Percentile(this IEnumerable<double> data, int p)
        {
            double[] array = data.ToArray();
            return ArrayStatistics.PercentileInplace(array, p);
        }

        /// <summary>
        /// Estimates the p-Percentile value from the provided samples.
        /// If a non-integer Percentile is needed, use Quantile instead.
        /// Approximately median-unbiased regardless of the sample distribution (R8).
        /// </summary>
        /// <param name="data">The data sample sequence.</param>
        /// <param name="p">Percentile selector, between 0 and 100 (inclusive).</param>
        public static float Percentile(this IEnumerable<float> data, int p)
        {
            float[] array = data.ToArray();
            return ArrayStatistics.PercentileInplace(array, p);
        }

        /// <summary>
        /// Estimates the p-Percentile value from the provided samples.
        /// If a non-integer Percentile is needed, use Quantile instead.
        /// Approximately median-unbiased regardless of the sample distribution (R8).
        /// </summary>
        /// <param name="data">The data sample sequence.</param>
        /// <param name="p">Percentile selector, between 0 and 100 (inclusive).</param>
        public static double Percentile(this IEnumerable<double?> data, int p)
        {
            double[] array = data.Where(d => d.HasValue).Select(d => d.Value).ToArray();
            return ArrayStatistics.PercentileInplace(array, p);
        }

        /// <summary>
        /// Estimates the p-Percentile value from the provided samples.
        /// If a non-integer Percentile is needed, use Quantile instead.
        /// Approximately median-unbiased regardless of the sample distribution (R8).
        /// </summary>
        /// <param name="data">The data sample sequence.</param>
        public static Func<int, double> PercentileFunc(this IEnumerable<double> data)
        {
            double[] array = data.ToArray();
            Array.Sort(array);
            return p => SortedArrayStatistics.Percentile(array, p);
        }

        /// <summary>
        /// Estimates the p-Percentile value from the provided samples.
        /// If a non-integer Percentile is needed, use Quantile instead.
        /// Approximately median-unbiased regardless of the sample distribution (R8).
        /// </summary>
        /// <param name="data">The data sample sequence.</param>
        public static Func<int, float> PercentileFunc(this IEnumerable<float> data)
        {
            float[] array = data.ToArray();
            Array.Sort(array);
            return p => SortedArrayStatistics.Percentile(array, p);
        }

        /// <summary>
        /// Estimates the p-Percentile value from the provided samples.
        /// If a non-integer Percentile is needed, use Quantile instead.
        /// Approximately median-unbiased regardless of the sample distribution (R8).
        /// </summary>
        /// <param name="data">The data sample sequence.</param>
        public static Func<int, double> PercentileFunc(this IEnumerable<double?> data)
        {
            double[] array = data.Where(d => d.HasValue).Select(d => d.Value).ToArray();
            Array.Sort(array);
            return p => SortedArrayStatistics.Percentile(array, p);
        }

        /// <summary>
        /// Estimates the first quartile value from the provided samples.
        /// Approximately median-unbiased regardless of the sample distribution (R8).
        /// </summary>
        /// <param name="data">The data sample sequence.</param>
        public static double LowerQuartile(this IEnumerable<double> data)
        {
            double[] array = data.ToArray();
            return ArrayStatistics.LowerQuartileInplace(array);
        }

        /// <summary>
        /// Estimates the first quartile value from the provided samples.
        /// Approximately median-unbiased regardless of the sample distribution (R8).
        /// </summary>
        /// <param name="data">The data sample sequence.</param>
        public static float LowerQuartile(this IEnumerable<float> data)
        {
            float[] array = data.ToArray();
            return ArrayStatistics.LowerQuartileInplace(array);
        }

        /// <summary>
        /// Estimates the first quartile value from the provided samples.
        /// Approximately median-unbiased regardless of the sample distribution (R8).
        /// </summary>
        /// <param name="data">The data sample sequence.</param>
        public static double LowerQuartile(this IEnumerable<double?> data)
        {
            double[] array = data.Where(d => d.HasValue).Select(d => d.Value).ToArray();
            return ArrayStatistics.LowerQuartileInplace(array);
        }

        /// <summary>
        /// Estimates the third quartile value from the provided samples.
        /// Approximately median-unbiased regardless of the sample distribution (R8).
        /// </summary>
        /// <param name="data">The data sample sequence.</param>
        public static double UpperQuartile(this IEnumerable<double> data)
        {
            double[] array = data.ToArray();
            return ArrayStatistics.UpperQuartileInplace(array);
        }

        /// <summary>
        /// Estimates the third quartile value from the provided samples.
        /// Approximately median-unbiased regardless of the sample distribution (R8).
        /// </summary>
        /// <param name="data">The data sample sequence.</param>
        public static float UpperQuartile(this IEnumerable<float> data)
        {
            float[] array = data.ToArray();
            return ArrayStatistics.UpperQuartileInplace(array);
        }

        /// <summary>
        /// Estimates the third quartile value from the provided samples.
        /// Approximately median-unbiased regardless of the sample distribution (R8).
        /// </summary>
        /// <param name="data">The data sample sequence.</param>
        public static double UpperQuartile(this IEnumerable<double?> data)
        {
            double[] array = data.Where(d => d.HasValue).Select(d => d.Value).ToArray();
            return ArrayStatistics.UpperQuartileInplace(array);
        }

        /// <summary>
        /// Estimates the inter-quartile range from the provided samples.
        /// Approximately median-unbiased regardless of the sample distribution (R8).
        /// </summary>
        /// <param name="data">The data sample sequence.</param>
        public static double InterquartileRange(this IEnumerable<double> data)
        {
            double[] array = data.ToArray();
            return ArrayStatistics.InterquartileRangeInplace(array);
        }

        /// <summary>
        /// Estimates the inter-quartile range from the provided samples.
        /// Approximately median-unbiased regardless of the sample distribution (R8).
        /// </summary>
        /// <param name="data">The data sample sequence.</param>
        public static float InterquartileRange(this IEnumerable<float> data)
        {
            float[] array = data.ToArray();
            return ArrayStatistics.InterquartileRangeInplace(array);
        }

        /// <summary>
        /// Estimates the inter-quartile range from the provided samples.
        /// Approximately median-unbiased regardless of the sample distribution (R8).
        /// </summary>
        /// <param name="data">The data sample sequence.</param>
        public static double InterquartileRange(this IEnumerable<double?> data)
        {
            double[] array = data.Where(d => d.HasValue).Select(d => d.Value).ToArray();
            return ArrayStatistics.InterquartileRangeInplace(array);
        }

        /// <summary>
        /// Estimates {min, lower-quantile, median, upper-quantile, max} from the provided samples.
        /// Approximately median-unbiased regardless of the sample distribution (R8).
        /// </summary>
        /// <param name="data">The data sample sequence.</param>
        public static double[] FiveNumberSummary(this IEnumerable<double> data)
        {
            double[] array = data.ToArray();
            return ArrayStatistics.FiveNumberSummaryInplace(array);
        }

        /// <summary>
        /// Estimates {min, lower-quantile, median, upper-quantile, max} from the provided samples.
        /// Approximately median-unbiased regardless of the sample distribution (R8).
        /// </summary>
        /// <param name="data">The data sample sequence.</param>
        public static float[] FiveNumberSummary(this IEnumerable<float> data)
        {
            float[] array = data.ToArray();
            return ArrayStatistics.FiveNumberSummaryInplace(array);
        }

        /// <summary>
        /// Estimates {min, lower-quantile, median, upper-quantile, max} from the provided samples.
        /// Approximately median-unbiased regardless of the sample distribution (R8).
        /// </summary>
        /// <param name="data">The data sample sequence.</param>
        public static double[] FiveNumberSummary(this IEnumerable<double?> data)
        {
            double[] array = data.Where(d => d.HasValue).Select(d => d.Value).ToArray();
            return ArrayStatistics.FiveNumberSummaryInplace(array);
        }

        /// <summary>
        /// Returns the order statistic (order 1..N) from the provided samples.
        /// </summary>
        /// <param name="data">The data sample sequence.</param>
        /// <param name="order">One-based order of the statistic, must be between 1 and N (inclusive).</param>
        public static double OrderStatistic(IEnumerable<double> data, int order)
        {
            double[] array = data.ToArray();
            return ArrayStatistics.OrderStatisticInplace(array, order);
        }

        /// <summary>
        /// Returns the order statistic (order 1..N) from the provided samples.
        /// </summary>
        /// <param name="data">The data sample sequence.</param>
        /// <param name="order">One-based order of the statistic, must be between 1 and N (inclusive).</param>
        public static float OrderStatistic(IEnumerable<float> data, int order)
        {
            float[] array = data.ToArray();
            return ArrayStatistics.OrderStatisticInplace(array, order);
        }

        /// <summary>
        /// Returns the order statistic (order 1..N) from the provided samples.
        /// </summary>
        /// <param name="data">The data sample sequence.</param>
        public static Func<int, double> OrderStatisticFunc(IEnumerable<double> data)
        {
            double[] array = data.ToArray();
            Array.Sort(array);
            return order => SortedArrayStatistics.OrderStatistic(array, order);
        }

        /// <summary>
        /// Returns the order statistic (order 1..N) from the provided samples.
        /// </summary>
        /// <param name="data">The data sample sequence.</param>
        public static Func<int, float> OrderStatisticFunc(IEnumerable<float> data)
        {
            float[] array = data.ToArray();
            Array.Sort(array);
            return order => SortedArrayStatistics.OrderStatistic(array, order);
        }

        /// <summary>
        /// Evaluates the rank of each entry of the provided samples.
        /// The rank definition can be specified to be compatible
        /// with an existing system.
        /// </summary>
        /// <param name="data">The data sample sequence.</param>
        /// <param name="definition">Rank definition, to choose how ties should be handled and what product/definition it should be consistent with</param>
        public static double[] Ranks(this IEnumerable<double> data, RankDefinition definition = RankDefinition.Default)
        {
            double[] array = data.ToArray();
            return ArrayStatistics.RanksInplace(array, definition);
        }

        /// <summary>
        /// Evaluates the rank of each entry of the provided samples.
        /// The rank definition can be specified to be compatible
        /// with an existing system.
        /// </summary>
        /// <param name="data">The data sample sequence.</param>
        /// <param name="definition">Rank definition, to choose how ties should be handled and what product/definition it should be consistent with</param>
        public static float[] Ranks(this IEnumerable<float> data, RankDefinition definition = RankDefinition.Default)
        {
            float[] array = data.ToArray();
            return ArrayStatistics.RanksInplace(array, definition);
        }

        /// <summary>
        /// Evaluates the rank of each entry of the provided samples.
        /// The rank definition can be specified to be compatible
        /// with an existing system.
        /// </summary>
        /// <param name="data">The data sample sequence.</param>
        /// <param name="definition">Rank definition, to choose how ties should be handled and what product/definition it should be consistent with</param>
        public static double[] Ranks(this IEnumerable<double?> data, RankDefinition definition = RankDefinition.Default)
        {
            return Ranks(data.Where(d => d.HasValue).Select(d => d.Value), definition);
        }

        /// <summary>
        /// Estimates the quantile tau from the provided samples.
        /// The tau-th quantile is the data value where the cumulative distribution
        /// function crosses tau. The quantile definition can be specified to be compatible
        /// with an existing system.
        /// </summary>
        /// <param name="data">The data sample sequence.</param>
        /// <param name="x">Quantile value.</param>
        /// <param name="definition">Rank definition, to choose how ties should be handled and what product/definition it should be consistent with</param>
        public static double QuantileRank(this IEnumerable<double> data, double x, RankDefinition definition = RankDefinition.Default)
        {
            double[] array = data.ToArray();
            Array.Sort(array);
            return SortedArrayStatistics.QuantileRank(array, x, definition);
        }

        /// <summary>
        /// Estimates the quantile tau from the provided samples.
        /// The tau-th quantile is the data value where the cumulative distribution
        /// function crosses tau. The quantile definition can be specified to be compatible
        /// with an existing system.
        /// </summary>
        /// <param name="data">The data sample sequence.</param>
        /// <param name="x">Quantile value.</param>
        /// <param name="definition">Rank definition, to choose how ties should be handled and what product/definition it should be consistent with</param>
        public static double QuantileRank(this IEnumerable<float> data, float x, RankDefinition definition = RankDefinition.Default)
        {
            float[] array = data.ToArray();
            Array.Sort(array);
            return SortedArrayStatistics.QuantileRank(array, x, definition);
        }

        /// <summary>
        /// Estimates the quantile tau from the provided samples.
        /// The tau-th quantile is the data value where the cumulative distribution
        /// function crosses tau. The quantile definition can be specified to be compatible
        /// with an existing system.
        /// </summary>
        /// <param name="data">The data sample sequence.</param>
        /// <param name="x">Quantile value.</param>
        /// <param name="definition">Rank definition, to choose how ties should be handled and what product/definition it should be consistent with</param>
        public static double QuantileRank(this IEnumerable<double?> data, double x, RankDefinition definition = RankDefinition.Default)
        {
            return QuantileRank(data.Where(d => d.HasValue).Select(d => d.Value), x, definition);
        }

        /// <summary>
        /// Estimates the quantile tau from the provided samples.
        /// The tau-th quantile is the data value where the cumulative distribution
        /// function crosses tau. The quantile definition can be specified to be compatible
        /// with an existing system.
        /// </summary>
        /// <param name="data">The data sample sequence.</param>
        /// <param name="definition">Rank definition, to choose how ties should be handled and what product/definition it should be consistent with</param>
        public static Func<double, double> QuantileRankFunc(this IEnumerable<double> data, RankDefinition definition = RankDefinition.Default)
        {
            double[] array = data.ToArray();
            Array.Sort(array);
            return x => SortedArrayStatistics.QuantileRank(array, x, definition);
        }

        /// <summary>
        /// Estimates the quantile tau from the provided samples.
        /// The tau-th quantile is the data value where the cumulative distribution
        /// function crosses tau. The quantile definition can be specified to be compatible
        /// with an existing system.
        /// </summary>
        /// <param name="data">The data sample sequence.</param>
        /// <param name="definition">Rank definition, to choose how ties should be handled and what product/definition it should be consistent with</param>
        public static Func<float, double> QuantileRankFunc(this IEnumerable<float> data, RankDefinition definition = RankDefinition.Default)
        {
            float[] array = data.ToArray();
            Array.Sort(array);
            return x => SortedArrayStatistics.QuantileRank(array, x, definition);
        }

        /// <summary>
        /// Estimates the quantile tau from the provided samples.
        /// The tau-th quantile is the data value where the cumulative distribution
        /// function crosses tau. The quantile definition can be specified to be compatible
        /// with an existing system.
        /// </summary>
        /// <param name="data">The data sample sequence.</param>
        /// <param name="definition">Rank definition, to choose how ties should be handled and what product/definition it should be consistent with</param>
        public static Func<double, double> QuantileRankFunc(this IEnumerable<double?> data, RankDefinition definition = RankDefinition.Default)
        {
            return QuantileRankFunc(data.Where(d => d.HasValue).Select(d => d.Value), definition);
        }

        /// <summary>
        /// Estimates the empirical cumulative distribution function (CDF) at x from the provided samples.
        /// </summary>
        /// <param name="data">The data sample sequence.</param>
        /// <param name="x">The value where to estimate the CDF at.</param>
        public static double EmpiricalCDF(this IEnumerable<double> data, double x)
        {
            double[] array = data.ToArray();
            Array.Sort(array);
            return SortedArrayStatistics.EmpiricalCDF(array, x);
        }

        /// <summary>
        /// Estimates the empirical cumulative distribution function (CDF) at x from the provided samples.
        /// </summary>
        /// <param name="data">The data sample sequence.</param>
        /// <param name="x">The value where to estimate the CDF at.</param>
        public static double EmpiricalCDF(this IEnumerable<float> data, float x)
        {
            float[] array = data.ToArray();
            Array.Sort(array);
            return SortedArrayStatistics.EmpiricalCDF(array, x);
        }

        /// <summary>
        /// Estimates the empirical cumulative distribution function (CDF) at x from the provided samples.
        /// </summary>
        /// <param name="data">The data sample sequence.</param>
        /// <param name="x">The value where to estimate the CDF at.</param>
        public static double EmpiricalCDF(this IEnumerable<double?> data, double x)
        {
            return EmpiricalCDF(data.Where(d => d.HasValue).Select(d => d.Value), x);
        }

        /// <summary>
        /// Estimates the empirical cumulative distribution function (CDF) at x from the provided samples.
        /// </summary>
        /// <param name="data">The data sample sequence.</param>
        public static Func<double, double> EmpiricalCDFFunc(this IEnumerable<double> data)
        {
            double[] array = data.ToArray();
            Array.Sort(array);
            return x => SortedArrayStatistics.EmpiricalCDF(array, x);
        }

        /// <summary>
        /// Estimates the empirical cumulative distribution function (CDF) at x from the provided samples.
        /// </summary>
        /// <param name="data">The data sample sequence.</param>
        public static Func<float, double> EmpiricalCDFFunc(this IEnumerable<float> data)
        {
            float[] array = data.ToArray();
            Array.Sort(array);
            return x => SortedArrayStatistics.EmpiricalCDF(array, x);
        }

        /// <summary>
        /// Estimates the empirical cumulative distribution function (CDF) at x from the provided samples.
        /// </summary>
        /// <param name="data">The data sample sequence.</param>
        public static Func<double, double> EmpiricalCDFFunc(this IEnumerable<double?> data)
        {
            return EmpiricalCDFFunc(data.Where(d => d.HasValue).Select(d => d.Value));
        }

        /// <summary>
        /// Estimates the empirical inverse CDF at tau from the provided samples.
        /// </summary>
        /// <param name="data">The data sample sequence.</param>
        /// <param name="tau">Quantile selector, between 0.0 and 1.0 (inclusive).</param>
        public static double EmpiricalInvCDF(this IEnumerable<double> data, double tau)
        {
            double[] array = data.ToArray();
            return ArrayStatistics.QuantileCustomInplace(array, tau, QuantileDefinition.EmpiricalInvCDF);
        }

        /// <summary>
        /// Estimates the empirical inverse CDF at tau from the provided samples.
        /// </summary>
        /// <param name="data">The data sample sequence.</param>
        /// <param name="tau">Quantile selector, between 0.0 and 1.0 (inclusive).</param>
        public static float EmpiricalInvCDF(this IEnumerable<float> data, double tau)
        {
            float[] array = data.ToArray();
            return ArrayStatistics.QuantileCustomInplace(array, tau, QuantileDefinition.EmpiricalInvCDF);
        }

        /// <summary>
        /// Estimates the empirical inverse CDF at tau from the provided samples.
        /// </summary>
        /// <param name="data">The data sample sequence.</param>
        /// <param name="tau">Quantile selector, between 0.0 and 1.0 (inclusive).</param>
        public static double EmpiricalInvCDF(this IEnumerable<double?> data, double tau)
        {
            return EmpiricalInvCDF(data.Where(d => d.HasValue).Select(d => d.Value), tau);
        }

        /// <summary>
        /// Estimates the empirical inverse CDF at tau from the provided samples.
        /// </summary>
        /// <param name="data">The data sample sequence.</param>
        public static Func<double, double> EmpiricalInvCDFFunc(this IEnumerable<double> data)
        {
            double[] array = data.ToArray();
            Array.Sort(array);
            return tau => SortedArrayStatistics.QuantileCustom(array, tau, QuantileDefinition.EmpiricalInvCDF);
        }

        /// <summary>
        /// Estimates the empirical inverse CDF at tau from the provided samples.
        /// </summary>
        /// <param name="data">The data sample sequence.</param>
        public static Func<double, float> EmpiricalInvCDFFunc(this IEnumerable<float> data)
        {
            float[] array = data.ToArray();
            Array.Sort(array);
            return tau => SortedArrayStatistics.QuantileCustom(array, tau, QuantileDefinition.EmpiricalInvCDF);
        }

        /// <summary>
        /// Estimates the empirical inverse CDF at tau from the provided samples.
        /// </summary>
        /// <param name="data">The data sample sequence.</param>
        public static Func<double, double> EmpiricalInvCDFFunc(this IEnumerable<double?> data)
        {
            return EmpiricalInvCDFFunc(data.Where(d => d.HasValue).Select(d => d.Value));
        }

        /// <summary>
        /// Calculates the entropy of a stream of double values in bits.
        /// Returns NaN if any of the values in the stream are NaN.
        /// </summary>
        /// <param name="data">The data sample sequence.</param>
        public static double Entropy(IEnumerable<double> data)
        {
            return StreamingStatistics.Entropy(data);
        }

        /// <summary>
        /// Calculates the entropy of a stream of double values in bits.
        /// Returns NaN if any of the values in the stream are NaN.
        /// Null-entries are ignored.
        /// </summary>
        /// <param name="data">The data sample sequence.</param>
        public static double Entropy(IEnumerable<double?> data)
        {
            return StreamingStatistics.Entropy(data.Where(d => d.HasValue).Select(d => d.Value));
        }

        /// <summary>
        /// Evaluates the sample mean over a moving window, for each samples.
        /// Returns NaN if no data is empty or if any entry is NaN.
        /// </summary>
        /// <param name="samples">The sample stream to calculate the mean of.</param>
        /// <param name="windowSize">The number of last samples to consider.</param>
        public static IEnumerable<double> MovingAverage(this IEnumerable<double> samples, int windowSize)
        {
            var movingStatistics = new MovingStatistics(windowSize);
            return samples.Select(sample =>
            {
                movingStatistics.Push(sample);
                return movingStatistics.Mean;
            });
        }
    }
}
