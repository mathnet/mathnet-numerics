// <copyright file="DescriptiveStatistics.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
//
// Copyright (c) 2009-2014 Math.NET
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
//
// Adapted from the old DescriptiveStatistics and inspired in design
// among others by http://www.johndcook.com/skewness_kurtosis.html

using System;
using System.Collections.Generic;

namespace MathNet.Numerics.Statistics
{
    /// <summary>
    /// Running statistics, allows updating by adding values,
    /// or combining
    /// </summary>
    public class RunningStatistics
    {
        long _n;
        double _m1;
        double _m2;
        double _m3;
        double _m4;
        double _min = double.PositiveInfinity;
        double _max = double.NegativeInfinity;

        public RunningStatistics()
        {
        }

        public RunningStatistics(IEnumerable<double> values)
        {
            PushRange(values);
        }

        /// <summary>
        /// Gets the total number of samples.
        /// </summary>
        public long Count
        {
            get { return _n; }
        }

        /// <summary>
        /// Returns the minimum value in the sample data.
        /// Returns NaN if data is empty or if any entry is NaN.
        /// </summary>
        public double Minimum
        {
            get { return _n > 0 ? _min : double.NaN; }
        }

        /// <summary>
        /// Returns the maximum value in the sample data.
        /// Returns NaN if data is empty or if any entry is NaN.
        /// </summary>
        public double Maximum
        {
            get { return _n > 0 ? _max : double.NaN; }
        }

        /// <summary>
        /// Evaluates the sample mean, an estimate of the population mean.
        /// Returns NaN if data is empty or if any entry is NaN.
        /// </summary>
        public double Mean
        {
            get { return _n > 0 ? _m1 : double.NaN; }
        }

        /// <summary>
        /// Estimates the unbiased population variance from the provided samples.
        /// On a dataset of size N will use an N-1 normalizer (Bessel's correction).
        /// Returns NaN if data has less than two entries or if any entry is NaN.
        /// </summary>
        public double Variance
        {
            get { return _n < 2 ? double.NaN : _m2/(_n - 1); }
        }

        /// <summary>
        /// Evaluates the variance from the provided full population.
        /// On a dataset of size N will use an N normalizer and would thus be biased if applied to a subset.
        /// Returns NaN if data is empty or if any entry is NaN.
        /// </summary>
        public double PopulationVariance
        {
            get { return _n < 2 ? double.NaN : _m2/_n; }
        }

        /// <summary>
        /// Estimates the unbiased population standard deviation from the provided samples.
        /// On a dataset of size N will use an N-1 normalizer (Bessel's correction).
        /// Returns NaN if data has less than two entries or if any entry is NaN.
        /// </summary>
        public double StandardDeviation
        {
            get { return _n < 2 ? double.NaN : Math.Sqrt(_m2/(_n - 1)); }
        }

        /// <summary>
        /// Evaluates the standard deviation from the provided full population.
        /// On a dataset of size N will use an N normalizer and would thus be biased if applied to a subset.
        /// Returns NaN if data is empty or if any entry is NaN.
        /// </summary>
        public double PopulationStandardDeviation
        {
            get { return _n < 2 ? double.NaN : Math.Sqrt(_m2/_n); }
        }

        /// <summary>
        /// Estimates the unbiased population skewness from the provided samples.
        /// Uses a normalizer (Bessel's correction; type 2).
        /// Returns NaN if data has less than three entries or if any entry is NaN.
        /// </summary>
        public double Skewness
        {
            get { return _n < 3 ? double.NaN : (_n*_m3*Math.Sqrt(_m2/(_n - 1))/(_m2*_m2*(_n - 2)))*(_n - 1); }
        }

        /// <summary>
        /// Evaluates the population skewness from the full population.
        /// Does not use a normalizer and would thus be biased if applied to a subset (type 1).
        /// Returns NaN if data has less than two entries or if any entry is NaN.
        /// </summary>
        public double PopulationSkewness
        {
            get { return _n < 2 ? double.NaN : Math.Sqrt(_n)*_m3/Math.Pow(_m2, 1.5); }
        }

        /// <summary>
        /// Estimates the unbiased population kurtosis from the provided samples.
        /// Uses a normalizer (Bessel's correction; type 2).
        /// Returns NaN if data has less than four entries or if any entry is NaN.
        /// </summary>
        public double Kurtosis
        {
            get { return _n < 4 ? double.NaN : ((double)_n*_n - 1)/((_n - 2)*(_n - 3))*(_n*_m4/(_m2*_m2) - 3 + 6.0/(_n + 1)); }
        }

        /// <summary>
        /// Evaluates the population kurtosis from the full population.
        /// Does not use a normalizer and would thus be biased if applied to a subset (type 1).
        /// Returns NaN if data has less than three entries or if any entry is NaN.
        /// </summary>
        public double PopulationKurtosis
        {
            get { return _n < 3 ? double.NaN : _n*_m4/(_m2*_m2) - 3.0; }
        }

        /// <summary>
        /// Update the running statistics by adding another observed sample (in-place).
        /// </summary>
        public void Push(double value)
        {
            _n++;
            double d = value - _m1;
            double s = d/_n;
            double s2 = s*s;
            double t = d*s*(_n - 1);

            _m1 += s;
            _m4 += t*s2*(_n*_n - 3*_n + 3) + 6*s2*_m2 - 4*s*_m3;
            _m3 += t*s*(_n - 2) - 3*s*_m2;
            _m2 += t;

            if (_min > value)
            {
                _min = value;
            }

            if (_max < value)
            {
                _max = value;
            }
        }

        /// <summary>
        /// Update the running statistics by adding a sequence of observed sample (in-place).
        /// </summary>
        public void PushRange(IEnumerable<double> values)
        {
            foreach (double value in values)
            {
                Push(value);
            }
        }

        /// <summary>
        /// Create a new running statistics over the combined samples of two existing running statistics.
        /// </summary>
        public static RunningStatistics Combine(RunningStatistics a, RunningStatistics b)
        {
            long n = a._n + b._n;
            double d = b._m1 - a._m1;
            double d2 = d*d;
            double d3 = d2*d;
            double d4 = d2*d2;

            double m1 = (a._n*a._m1 + b._n*b._m1)/n;
            double m2 = a._m2 + b._m2 + d2*a._n*b._n/n;
            double m3 = a._m3 + b._m3 + d3*a._n*b._n*(a._n - b._n)/(n*n)
                        + 3*d*(a._n*b._m2 - b._n*a._m2)/n;
            double m4 = a._m4 + b._m4 + d4*a._n*b._n*(a._n*a._n - a._n*b._n + b._n*b._n)/(n*n*n)
                        + 6*d2*(a._n*a._n*b._m2 + b._n*b._n*a._m2)/(n*n) + 4*d*(a._n*b._m3 - b._n*a._m3)/n;

            return new RunningStatistics { _n = n, _m1 = m1, _m2 = m2, _m3 = m3, _m4 = m4 };
        }

        public static RunningStatistics operator +(RunningStatistics a, RunningStatistics b)
        {
            return Combine(a, b);
        }
    }
}
