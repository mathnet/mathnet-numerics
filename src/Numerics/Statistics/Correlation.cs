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
using MathNet.Numerics.IntegralTransforms;
using MathNet.Numerics.LinearAlgebra;
using Complex = System.Numerics.Complex;

namespace MathNet.Numerics.Statistics
{
    /// <summary>
    /// A class with correlation measures between two datasets.
    /// </summary>
    public static class Correlation
    {
        /// <summary>
        /// Auto-correlation function (ACF) based on FFT for all possible lags k.
        /// </summary>
        /// <param name="x">Data array to calculate auto correlation for.</param>
        /// <returns>An array with the ACF as a function of the lags k.</returns>
        public static double[] Auto(double[] x)
        {
            return AutoCorrelationFft(x, 0, x.Length - 1);
        }

        /// <summary>
        /// Auto-correlation function (ACF) based on FFT for lags between kMin and kMax.
        /// </summary>
        /// <param name="x">The data array to calculate auto correlation for.</param>
        /// <param name="kMax">Max lag to calculate ACF for must be positive and smaller than x.Length.</param>
        /// <param name="kMin">Min lag to calculate ACF for (0 = no shift with acf=1) must be zero or positive and smaller than x.Length.</param>
        /// <returns>An array with the ACF as a function of the lags k.</returns>
        public static double[] Auto(double[] x, int kMax, int kMin = 0)
        {
            // assert max and min in proper order
            var kMax2 = Math.Max(kMax, kMin);
            var kMin2 = Math.Min(kMax, kMin);

            return AutoCorrelationFft(x, kMin2, kMax2);
        }

        /// <summary>
        /// Auto-correlation function based on FFT for lags k.
        /// </summary>
        /// <param name="x">The data array to calculate auto correlation for.</param>
        /// <param name="k">Array with lags to calculate ACF for.</param>
        /// <returns>An array with the ACF as a function of the lags k.</returns>
        public static double[] Auto(double[] x, int[] k)
        {
            if (k == null)
            {
                throw new ArgumentNullException(nameof(k));
            }

            if (k.Length < 1)
            {
                throw new ArgumentException("k");
            }

            var kMin = k.Min();
            var kMax = k.Max();

            // get acf between full range
            var acf = AutoCorrelationFft(x, kMin, kMax);

            // map output by indexing
            var result = new double[k.Length];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = acf[k[i] - kMin];
            }

            return result;
        }

        /// <summary>
        /// The internal method for calculating the auto-correlation.
        /// </summary>
        /// <param name="x">The data array to calculate auto-correlation for</param>
        /// <param name="kLow">Min lag to calculate ACF for (0 = no shift with acf=1) must be zero or positive and smaller than x.Length</param>
        /// <param name="kHigh">Max lag (EXCLUSIVE) to calculate ACF for must be positive and smaller than x.Length</param>
        /// <returns>An array with the ACF as a function of the lags k.</returns>
        static double[] AutoCorrelationFft(double[] x, int kLow, int kHigh)
        {
            if (x == null)
                throw new ArgumentNullException(nameof(x));

            int N = x.Length;    // Sample size

            if (kLow < 0 || kLow >= N)
                throw new ArgumentOutOfRangeException(nameof(kLow), "kMin must be zero or positive and smaller than x.Length");
            if (kHigh < 0 || kHigh >= N)
                throw new ArgumentOutOfRangeException(nameof(kHigh), "kMax must be positive and smaller than x.Length");

            int nFFT = Euclid.CeilingToPowerOfTwo(N) * 2;

            Complex[] xFFT = new Complex[nFFT];
            Complex[] xFFT2 = new Complex[nFFT];

            double xDash = ArrayStatistics.Mean(x);

            // copy values in range and subtract mean - all the remaining parts are padded with zero.
            for (int i = 0; i < x.Length; i++)
            {
                xFFT[i] = new Complex(x[i] - xDash, 0.0);    // copy values in range and subtract mean
            }

            Fourier.Forward(xFFT, FourierOptions.Matlab);

            // maybe a Vector<Complex> implementation here would be faster
            for (int i = 0; i < xFFT.Length; i++)
            {
                xFFT2[i] = Complex.Multiply(xFFT[i], Complex.Conjugate(xFFT[i]));
            }

            Fourier.Inverse(xFFT2, FourierOptions.Matlab);

            double dc = xFFT2[0].Real;

            double[] result = new double[kHigh - kLow + 1];

            // normalize such that acf[0] would be 1.0
            for (int i = 0; i < (kHigh - kLow + 1); i++)
            {
                result[i] = xFFT2[kLow + i].Real / dc;
            }

            return result;
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
                        throw new ArgumentOutOfRangeException(nameof(dataB), "The array arguments must have the same length.");
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
                    throw new ArgumentOutOfRangeException(nameof(dataA), "The array arguments must have the same length.");
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
                        throw new ArgumentOutOfRangeException(nameof(dataB), "The array arguments must have the same length.");
                    }
                    if (!ieW.MoveNext())
                    {
                        throw new ArgumentOutOfRangeException(nameof(weights), "The array arguments must have the same length.");
                    }

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
                    throw new ArgumentOutOfRangeException(nameof(dataB), "The array arguments must have the same length.");
                }
                if (ieW.MoveNext())
                {
                    throw new ArgumentOutOfRangeException(nameof(weights), "The array arguments must have the same length.");
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
                return Array.Empty<double>();
            }

            // WARNING: do not try to cast series to an array and use it directly,
            // as we need to sort it (inplace operation)

            var data = series.ToArray();
            return ArrayStatistics.RanksInplace(data, RankDefinition.Average);
        }
    }
}
