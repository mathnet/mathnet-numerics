// <copyright file="RunningStatistics.cs" company="Math.NET">
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
//
// Adapted from the old DescriptiveStatistics and inspired in design
// among others by http://www.johndcook.com/skewness_kurtosis.html

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MathNet.Numerics.Statistics
{
    /// <summary>
    /// Running weighted statistics accumulator, allows updating by adding values
    /// or by combining two accumulators. Weights are reliability weights, not frequency weights.
    /// </summary>
    /// <remarks>
    /// This type declares a DataContract for out of the box ephemeral serialization
    /// with engines like DataContractSerializer, Protocol Buffers and FsPickler,
    /// but does not guarantee any compatibility between versions.
    /// It is not recommended to rely on this mechanism for durable persistence.
    /// </remarks>
    [DataContract(Namespace = "urn:MathNet/Numerics")]
    public class RunningWeightedStatistics
    {
        [DataMember(Order = 1)]
        long _n;

        [DataMember(Order = 2)]
        double _min = double.PositiveInfinity;

        [DataMember(Order = 3)]
        double _max = double.NegativeInfinity;

        // Sample mean
        [DataMember(Order = 4)]
        double _m1;

        /// <summary>
        /// Second moment
        /// </summary>
        [DataMember(Order = 5)]
        double _m2;

        /// <summary>
        /// Third moment
        /// </summary>
        [DataMember(Order = 6)]
        double _m3;

        /// <summary>
        /// Fourth moment
        /// </summary>
        [DataMember(Order = 7)]
        double _m4;

        /// <summary>
        /// Total weight
        /// </summary>
        [DataMember(Order = 8)]
        double _w1;

        /// <summary>
        /// Total of weights to the second power
        /// </summary>
        [DataMember(Order = 9)]
        double _w2;

        /// <summary>
        /// Total of weights to the third power
        /// </summary>
        [DataMember(Order = 10)]
        double _w3;

        /// <summary>
        /// Total of weights to the fourth power
        /// </summary>
        [DataMember(Order = 11)]
        double _w4;

        /// <summary>
        /// The denominator in the variance calculation to perform a Bessel correction.
        /// </summary>
        [DataMember(Order = 12)]
        double _den;

        public RunningWeightedStatistics()
        {
        }

        public RunningWeightedStatistics(IEnumerable<System.Tuple<double, double>> values)
        {
            PushRange(values);
        }

        /// <summary>
        /// Gets the total number of samples with non-zero weight.
        /// </summary>
        public long Count => _n;

        /// <summary>
        /// Returns the minimum value in the sample data.
        /// Returns NaN if data is empty or if any entry is NaN.
        /// </summary>
        public double Minimum => _n > 0 ? _min : double.NaN;

        /// <summary>
        /// Returns the maximum value in the sample data.
        /// Returns NaN if data is empty or if any entry is NaN.
        /// </summary>
        public double Maximum => _n > 0 ? _max : double.NaN;

        /// <summary>
        /// Evaluates the sample mean, an estimate of the population mean.
        /// Returns NaN if data is empty or if any entry is NaN.
        /// </summary>
        public double Mean => _n > 0 ? _m1 : double.NaN;

        /// <summary>
        /// Estimates the unbiased population variance from the provided samples.
        /// Will use the Bessel correction for reliability weighting.
        /// Returns NaN if data has less than two entries or if any entry is NaN.
        /// </summary>
        public double Variance => _n < 2 ? double.NaN : _m2 / _den;

        /// <summary>
        /// Evaluates the variance from the provided full population.
        /// On a dataset of size N will use an N normalizer and would thus be biased if applied to a subset.
        /// Returns NaN if data is empty or if any entry is NaN.
        /// </summary>
        public double PopulationVariance => _n < 2 ? double.NaN : _m2 / _w1;

        /// <summary>
        /// Estimates the unbiased population standard deviation from the provided samples.
        /// Will use the Bessel correction for reliability weighting.
        /// Returns NaN if data has less than two entries or if any entry is NaN.
        /// </summary>
        public double StandardDeviation => _n < 2 ? double.NaN : Math.Sqrt(_m2 / _den);

        /// <summary>
        /// Evaluates the standard deviation from the provided full population.
        /// On a dataset of size N will use an N normalizer and would thus be biased if applied to a subset.
        /// Returns NaN if data is empty or if any entry is NaN.
        /// </summary>
        public double PopulationStandardDeviation => _n < 2 ? double.NaN : Math.Sqrt(_m2 / _w1);

        /// <summary>
        /// Estimates the unbiased population skewness from the provided samples.
        /// Will use the Bessel correction for reliability weighting.
        /// Returns NaN if data has less than three entries or if any entry is NaN.
        /// </summary>
        public double Skewness
        {
            get
            {
                if (_n < 3)
                    return double.NaN;
                else
                {
                    var skewDen = (_w1 * (_w1 * _w1 - 3.0 * _w2) + 2.0 * _w3) / (_w1 * _w1);
                    return _m3 / (skewDen * Math.Pow(_m2 / _den, 1.5));
                }

            }
        }
        /// <summary>
        /// Evaluates the population skewness from the full population.
        /// Does not use a normalizer and would thus be biased if applied to a subset (type 1).
        /// Returns NaN if data has less than two entries or if any entry is NaN.
        /// </summary>
        public double PopulationSkewness => _n < 2 ? double.NaN : _m3 * Math.Sqrt(_w1) / Math.Pow(_m2, 1.5);

        /// <summary>
        /// Estimates the unbiased population excess kurtosis from the provided samples.
        /// Will use the Bessel correction for reliability weighting.
        /// Returns NaN if data has less than four entries or if any entry is NaN.
        /// Equivalent formula for this for weighted distributions are unknown.
        /// </summary>
        public double Kurtosis
        {
            get
            {
                if (_n < 4)
                    return double.NaN;
                else
                {
                    double p2 = _w1 * _w1;
                    double p4 = p2 * p2;
                    double w2p2 = _w2 * _w2;
                    double poly = p4 - 6.0 * p2 * _w2 + 8.0 * _w1 * _w3 + 3.0 * w2p2 - 6.0 * _w4;
                    double a = p4 - 4.0 * _w1 * _w3 + 3.0 * w2p2;
                    double b = 3.0 * (p4 - 2.0 * p2 * _w2 + 4.0 * _w1 * _w3 - 3.0 * w2p2);
                    return (a * _w1 * _m4 / (_m2 * _m2) - b) * (_den / (_w1 * poly));
                }
            }
        }

        /// <summary>
        /// Evaluates the population excess kurtosis from the full population.
        /// Does not use a normalizer and would thus be biased if applied to a subset (type 1).
        /// Returns NaN if data has less than three entries or if any entry is NaN.
        /// </summary>
        public double PopulationKurtosis => _n < 3 ? double.NaN : _w1 * _m4 / (_m2 * _m2) - 3.0;

        /// <summary>
        /// Evaluates the total weight of the population.
        /// </summary>
        public double TotalWeight => _w1;

        /// <summary>
        /// The Kish's Effective Sample Size
        /// </summary>
        public double EffectiveSampleSize => _w2 / _w1;

        /// <summary>
        /// Update the running statistics by adding another observed sample (in-place).
        /// </summary>
        public void Push(double weight, double value)
        {
            if (weight == 0.0)
                return;
            if (weight < 0.0)
                throw new ArgumentOutOfRangeException(nameof(weight), weight, "Expected non-negative weighting of sample");

            _n++;
            double prevW = _w1;
            double pow = weight;
            _w1 += pow;
            pow *= weight;
            _w2 += pow;
            pow *= weight;
            _w3 += pow;
            pow *= weight;
            _w4 += pow;
            _den += weight * ( 2.0 * prevW - _den) / _w1;

            double d = value - _m1;
            double s = d * weight / _w1;
            double s2 = s*s;
            double t = d*s*prevW;

            _m1 += s;
            double r = prevW / weight;
            _m4 += t*s2*(r*r + 1.0 - r) + 6*s2*_m2 - 4*s*_m3;
            _m3 += t*s*(r - 1.0) - 3*s*_m2;
            _m2 += t;

            if (value < _min || double.IsNaN(value))
            {
                _min = value;
            }

            if (value > _max || double.IsNaN(value))
            {
                _max = value;
            }
        }

        /// <summary>
        /// Update the running statistics by adding a sequence of weighted observatopms (in-place).
        /// </summary>
        public void PushRange(IEnumerable<System.Tuple<double,double>> values)
        {
            foreach (var v in values)
            {
                Push(v.Item1, v.Item2);
            }
        }

        /// <summary>
        /// Update the running statistics by adding a sequence of weighted observatopms (in-place).
        /// </summary>
        public void PushRange(IEnumerable<double> weights, IEnumerable<double> values)
        {
            using (var itW = weights.GetEnumerator())
            {
                using (var itV = values.GetEnumerator())
                {
                    var w = itW.MoveNext();
                    var v = itV.MoveNext();
                    while (v & w)
                    {
                        if (v != w)
                            throw new ArgumentException("Weights and values need to be same length", nameof(values));
                        Push(itW.Current, itV.Current);
                        w = itW.MoveNext();
                        v = itV.MoveNext();
                    }
                }
            }
        }
        /// <summary>
        /// Create a new running statistics over the combined samples of two existing running statistics.
        /// </summary>
        public static RunningWeightedStatistics Combine(RunningWeightedStatistics a, RunningWeightedStatistics b)
        {
            if (a._n == 0)
            {
                return b;
            }
            else if (b._n == 0)
            {
                return a;
            }

            long n = a._n + b._n;
            double w1 = a._w1 + b._w1;
            double w2 = a._w2 + b._w2;
            double w3 = a._w3 + b._w3;
            double w4 = a._w4 + b._w4;

            double d = b._m1 - a._m1;
            double d2 = d*d;
            double d3 = d2*d;
            double d4 = d2*d2;

            double m1 = (a._w1 * a._m1 + b._w1 * b._m1) / w1;
            double m2 = a._m2 + b._m2 + d2 * a._w1 *b._w1 / w1;
            double m3 = a._m3 + b._m3 + d3 * a._w1 * b._w1 * (a._w1 - b._w1 ) / (w1 * w1)
                        + 3 * d *(a._w1 * b._m2 - b._w1 * a._m2) / w1;
            double m4 = a._m4 + b._m4 + d4 * a._w1 * b._w1 * (a._w1 * a._w1 - a._w1 * b._w1 + b._w1 * b._w1) / (w1 * w1 * w1)
                        + 6 * d2 * (a._w1 * a._w1 * b._m2 + b._w1 * b._w1 * a._m2)/(w1 * w1) + 4 * d * (a._w1 * b._m3 - b._w1 * a._m3) / w1;
            double min = Math.Min(a._min, b._min);
            double max = Math.Max(a._max, b._max);
            double den = w1 - ((a._w1 - a._den) * a._w1 + (b._w1 - b._den) * b._w1) / w1;

            return new RunningWeightedStatistics { _n = n, _m1 = m1, _m2 = m2, _m3 = m3, _m4 = m4, _min = min, _max = max, _w1 = w1, _den = den, _w2 = w2, _w3 = w3, _w4 = w4 };
        }

        public static RunningWeightedStatistics operator +(RunningWeightedStatistics a, RunningWeightedStatistics b)
        {
            return Combine(a, b);
        }
    }
}
