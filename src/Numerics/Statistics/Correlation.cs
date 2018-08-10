// <copyright file="Correlation.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2018 Math.NET
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
using System.Numerics;
using MathNet.Numerics.IntegralTransforms;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Properties;

namespace MathNet.Numerics.Statistics
{
    /// <summary>
    /// A class with correlation measures between two datasets.
    /// </summary>
    public static class Correlation
    {
        /// <summary>
        /// Autocorrelation function (ACF) based on FFT for all possible lags k.
        /// </summary>
        /// <param name="x">Data array to calculate auto correlation for.</param>
        /// <returns>An array with the ACF as a function of the lags k.</returns>
        public static double[] Auto(IEnumerable<double> x)
        {
            return AutoCorrelationFft(x, 0, x.Count() - 1);
        }

        /// <summary>
        /// Autocorrelation function (ACF) based on FFT for lags between kMin and kMax.
        /// </summary>
        /// <param name="x">The data array to calculate auto correlation for.</param>
        /// <param name="kMax">Max lag to calculate ACF for must be positive and smaller than x.Length.</param>
        /// <param name="kMin">Min lag to calculate ACF for (0 = no shift with acf=1) must be zero or positive and smaller than x.Length.</param>
        /// <returns>An array with the ACF as a function of the lags k.</returns>
        public static double[] Auto(IEnumerable<double> x, int kMax, int kMin = 0)
        {
            // assert max and min in proper order
            var kMax2 = Math.Max(kMax, kMin);
            var kMin2 = Math.Min(kMax, kMin);

            return AutoCorrelationFft(x, kMin2, kMax2);
        }

        /// <summary>
        /// Autocorrelation function based on FFT for lags k.
        /// </summary>
        /// <param name="x">The data array to calculate auto correlation for.</param>
        /// <param name="k">Array with lags to calculate ACF for.</param>
        /// <returns>An array with the ACF as a function of the lags k.</returns>
        public static double[] Auto(IEnumerable<double> x, int[] k)
        {
            if (k == null)
            {
                throw new ArgumentNullException(nameof(k));
            }

            if (k.Length < 1)
            {
                throw new ArgumentException("k");
            }

            var k_min = k.Min();
            var k_max = k.Max();
            // get acf between full range
            var acf = AutoCorrelationFft(x, k_min, k_max);

            // map output by indexing
            var result = new double[k.Length];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = acf[k[i] - k_min];
            }

            return result;
        }

        /// <summary>
        /// The internal core method for calculating the autocorrelation.
        /// </summary>
        /// <param name="x">The data array to calculate auto correlation for</param>
        /// <param name="k_low">Min lag to calculate ACF for (0 = no shift with acf=1) must be zero or positive and smaller than x.Length</param>
        /// <param name="k_high">Max lag (EXCLUSIVE) to calculate ACF for must be positive and smaller than x.Length</param>
        /// <returns>An array with the ACF as a function of the lags k.</returns>
        private static double[] AutoCorrelationFft(IEnumerable<double> x, int k_low, int k_high)
        {
            if (x == null)
                throw new ArgumentNullException(nameof(x));

            int N = x.Count();    // Sample size

            if (k_low < 0 || k_low >= N)
                throw new ArgumentOutOfRangeException(nameof(k_low), "kMin must be zero or positive and smaller than x.Length");
            if (k_high < 0 || k_high >= N)
                throw new ArgumentOutOfRangeException(nameof(k_high), "kMax must be positive and smaller than x.Length");

            if (N < 1)
                return new double[0];

            int nFFT = Euclid.CeilingToPowerOfTwo(N) * 2;

            Complex[] x_fft = new Complex[nFFT];
            Complex[] x_fft2 = new Complex[nFFT];

            double x_dash = Statistics.Mean(x);
            double xArrNow = 0.0d;

            using (IEnumerator<double> iex = x.GetEnumerator())
            {
                for (int ii = 0; ii < nFFT; ii++)
                {

                    if (ii < N)
                    {
                        if (!iex.MoveNext())
                            throw new ArgumentOutOfRangeException(nameof(x));
                        xArrNow = iex.Current;
                        x_fft[ii] = new Complex(xArrNow - x_dash, 0.0);    // copy values in range and substract mean
                    }
                    else
                        x_fft[ii] = new Complex(0.0, 0.0);      // pad all remaining points
                }

            }

            Fourier.Forward(x_fft, FourierOptions.Matlab);

            // maybe a Vector<Complex> implementation here would be faster
            for (int ii = 0; ii < x_fft.Length; ii++)
            {
                x_fft2[ii] = Complex.Multiply(x_fft[ii], Complex.Conjugate(x_fft[ii]));
            }

            Fourier.Inverse(x_fft2, FourierOptions.Matlab);

            double acf_Val1 = x_fft2[0].Real;

            double[] acf_Vec = new double[k_high - k_low + 1];

            // normalize such that acf[0] would be 1.0
            for (int ii = 0; ii < (k_high - k_low + 1); ii++)
            {
                acf_Vec[ii] = x_fft2[k_low + ii].Real / acf_Val1;
            }

            return acf_Vec;
        }

        /// <summary>
        /// Computes the Pearson Product-Moment Correlation coefficient.
        /// </summary>
        /// <param name="dataA">Sample data A.</param>
        /// <param name="dataB">Sample data B.</param>
        /// <returns>The Pearson product-moment correlation coefficient.</returns>
        public static double Pearson(IEnumerable<double> dataA, IEnumerable<double> dataB)
        {
            int n = 0;
            double r = 0.0;

            double meanA = 0;
            double meanB = 0;
            double varA = 0;
            double varB = 0;

            // WARNING: do not try to "optimize" by summing up products instead of using differences.
            // It would indeed be faster, but numerically much less robust if large mean + low variance.

            using (IEnumerator<double> ieA = dataA.GetEnumerator())
            using (IEnumerator<double> ieB = dataB.GetEnumerator())
            {
                while (ieA.MoveNext())
                {
                    if (!ieB.MoveNext())
                    {
                        throw new ArgumentOutOfRangeException(nameof(dataB), Resources.ArgumentArraysSameLength);
                    }

                    double currentA = ieA.Current;
                    double currentB = ieB.Current;

                    double deltaA = currentA - meanA;
                    double scaleDeltaA = deltaA/++n;

                    double deltaB = currentB - meanB;
                    double scaleDeltaB = deltaB/n;

                    meanA += scaleDeltaA;
                    meanB += scaleDeltaB;

                    varA += scaleDeltaA*deltaA*(n - 1);
                    varB += scaleDeltaB*deltaB*(n - 1);
                    r += (deltaA*deltaB*(n - 1))/n;
                }

                if (ieB.MoveNext())
                {
                    throw new ArgumentOutOfRangeException(nameof(dataA), Resources.ArgumentArraysSameLength);
                }
            }

            return r/Math.Sqrt(varA*varB);
        }

        /// <summary>
        /// Computes the Weighted Pearson Product-Moment Correlation coefficient.
        /// </summary>
        /// <param name="dataA">Sample data A.</param>
        /// <param name="dataB">Sample data B.</param>
        /// <param name="weights">Corresponding weights of data.</param>
        /// <returns>The Weighted Pearson product-moment correlation coefficient.</returns>
        public static double WeightedPearson(IEnumerable<double> dataA, IEnumerable<double> dataB, IEnumerable<double> weights)
        {
            int n = 0;

            double meanA = 0;
            double meanB = 0;
            double varA = 0;
            double varB = 0;
            double sumWeight = 0;

            double covariance = 0;

            using (IEnumerator<double> ieA = dataA.GetEnumerator())
            using (IEnumerator<double> ieB = dataB.GetEnumerator())
            using (IEnumerator<double> ieW = weights.GetEnumerator())
            {
                while (ieA.MoveNext())
                {
                    if (!ieB.MoveNext())
                    {
                        throw new ArgumentOutOfRangeException(nameof(dataB), Resources.ArgumentArraysSameLength);
                    }
                    if (!ieW.MoveNext())
                    {
                        throw new ArgumentOutOfRangeException(nameof(weights), Resources.ArgumentArraysSameLength);
                    }
                    ++n;

                    double xi = ieA.Current;
                    double yi = ieB.Current;
                    double wi = ieW.Current;

                    double temp = sumWeight + wi;

                    double deltaX = xi - meanA;
                    double rX = deltaX*wi/temp;
                    meanA += rX;
                    varA += sumWeight*deltaX*rX;

                    double deltaY = yi - meanB;
                    double rY = deltaY*wi/temp;
                    meanB += rY;
                    varB += sumWeight*deltaY*rY;

                    covariance += deltaX*deltaY*wi*(sumWeight/temp);
                    sumWeight = temp;
                }
                if (ieB.MoveNext())
                {
                    throw new ArgumentOutOfRangeException(nameof(dataB), Resources.ArgumentArraysSameLength);
                }
                if (ieW.MoveNext())
                {
                    throw new ArgumentOutOfRangeException(nameof(weights), Resources.ArgumentArraysSameLength);
                }
            }
            return covariance/Math.Sqrt(varA*varB);
        }

        /// <summary>
        /// Computes the Pearson Product-Moment Correlation matrix.
        /// </summary>
        /// <param name="vectors">Array of sample data vectors.</param>
        /// <returns>The Pearson product-moment correlation matrix.</returns>
        public static Matrix<double> PearsonMatrix(params double[][] vectors)
        {
            var m = Matrix<double>.Build.DenseIdentity(vectors.Length);
            for (int i = 0; i < vectors.Length; i++)
            {
                for (int j = i + 1; j < vectors.Length; j++)
                {
                    var c = Pearson(vectors[i], vectors[j]);
                    m.At(i, j, c);
                    m.At(j, i, c);
                }
            }

            return m;
        }

        /// <summary>
        /// Computes the Pearson Product-Moment Correlation matrix.
        /// </summary>
        /// <param name="vectors">Enumerable of sample data vectors.</param>
        /// <returns>The Pearson product-moment correlation matrix.</returns>
        public static Matrix<double> PearsonMatrix(IEnumerable<double[]> vectors)
        {
            return PearsonMatrix(vectors as double[][] ?? vectors.ToArray());
        }

        /// <summary>
        /// Computes the Spearman Ranked Correlation coefficient.
        /// </summary>
        /// <param name="dataA">Sample data series A.</param>
        /// <param name="dataB">Sample data series B.</param>
        /// <returns>The Spearman ranked correlation coefficient.</returns>
        public static double Spearman(IEnumerable<double> dataA, IEnumerable<double> dataB)
        {
            return Pearson(Rank(dataA), Rank(dataB));
        }

        /// <summary>
        /// Computes the Spearman Ranked Correlation matrix.
        /// </summary>
        /// <param name="vectors">Array of sample data vectors.</param>
        /// <returns>The Spearman ranked correlation matrix.</returns>
        public static Matrix<double> SpearmanMatrix(params double[][] vectors)
        {
            return PearsonMatrix(vectors.Select(Rank).ToArray());
        }

        /// <summary>
        /// Computes the Spearman Ranked Correlation matrix.
        /// </summary>
        /// <param name="vectors">Enumerable of sample data vectors.</param>
        /// <returns>The Spearman ranked correlation matrix.</returns>
        public static Matrix<double> SpearmanMatrix(IEnumerable<double[]> vectors)
        {
            return PearsonMatrix(vectors.Select(Rank).ToArray());
        }

        static double[] Rank(IEnumerable<double> series)
        {
            if (series == null)
            {
                return new double[0];
            }

            // WARNING: do not try to cast series to an array and use it directly,
            // as we need to sort it (inplace operation)

            var data = series.ToArray();
            return ArrayStatistics.RanksInplace(data, RankDefinition.Average);
        }
    }
}
