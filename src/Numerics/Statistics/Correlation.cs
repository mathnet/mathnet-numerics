// <copyright file="Correlation.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
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

using System;
using System.Collections.Generic;
using System.Linq;
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
        /// autocorrelation function (ACF) based on fft (usually faster then direct brute force implementation) for all possible lags k 
        /// First element is hidden since ACF(k = 0) = 1 </summary>
        /// <param name="x"> data array to calculate auto correlation for</param>
        /// <returns>an array with the ACF as a function of the lags k</returns>
        public static double[] AutoCorrelation(double[] x)
        {
            return autoCorrFft(x, tmpk, 0, x.Length-1);
        }

        /// <summary> 
        /// autocorrelation function (ACF) based on fft (usually faster then direct brute force implementation) for lags
        /// between kMin and kMax
        /// First element is hidden since ACF(k = 0) = 1 </summary>
        /// <param name="x"> the data array to calculate auto correlation for</param>
        /// <param name="kMax"> max lag to calculate ACF for must be positive and smaller than x.Length-1</param>
        /// <param name="kMax"> min lag to calculate ACF for (0 = no shift with acf=1) must be zero or positive and smaller than x.Length-1</param>
        public static double[] AutoCorrelation(double[] x, int kMax, int kMin = 0)
        {
            // assert max and min in proper order
            var kMax2 = Math.Max(kMax, kMin);
            var kMin2 = Math.Min(kMax, kMin);

            return (CorrCov.autoCorrFft(x, kMin2, kMax2));
        }


        /// <summary> 
        /// autocorrelation function based on fft for lags k (faster than brute force calculation for big sample sizes). 
        /// First element is skipped since ACF(k = 0) = 1 </summary>
        /// <param name="x"> the data array to calculate auto correlation for</param>
        /// <param name="k"> array with lags to calculate ACF for</param>
        public static double[] AutoCorrelation(double[] x, int[] k)
        {
            if (k == null)
                throw new ArgumentNullException("k");

            if (k.Length < 1)
                throw new ArgumentException("k");

            // get acf between full range
            var acf = autoCorrFft(x, k.Min(), k.Max());

            // map output by indexing
            var acfReturn = new double[k.Length];
            for (int i = 0; i < acfReturn.Length; i++)
                acfReturn[i] = acf[k[i]];

            return acfReturn;
        }

        private static double[] autoCorrFft(double[] x, int k_low, int k_high)
        {
            if(x == null)
                throw new ArgumentNullException("x");

            if (k_low < 0 || k_low >= x.Length)
                throw new ArgumentOutOfRangeException("kMin must be zero or positive and smaller than x.Length");
            if (k_high < 0 || k_high >= x.Length)
                throw new ArgumentOutOfRangeException("kMax must be positive and smaller than x.Length");

            if (x.Length < 1)
                return new double[0];

            int N = x.Length;    // Sample size

            int[] idx = new int[k_high - k_low];
            idx[0] = k_low;

            for (int ii = 1; ii < idx.Length; ii++)
                idx[ii] = idx[ii - 1] + 1;

            int numLags = N - 1;
            int nFFT = (int)Math.Pow(2, Euclid.CeilingToPowerOfTwo(N) + 1);

            Complex[] x_fft = new Complex[nFFT];
            Complex[] x_fft2 = new Complex[nFFT];

            double x_dash = Statistics.Mean(x);

            for (int ii = 0; ii < x_fft.Length; ii++)
            {
                if (ii < N)
                    x_fft[ii] = new Complex(x[ii] - x_dash, 0.0);    // copy values in range and substract mean
                else
                    x_fft[ii] = new Complex(0.0, 0.0);      // pad all remaining points
            }

            Fourier.Forward(x_fft, FourierOptions.Matlab);


            for (int ii = 0; ii < x_fft.Length; ii++)
            {
                x_fft2[ii] = Complex.Multiply(x_fft[ii], Complex.Conjugate(x_fft[ii]));
            }

            Fourier.Inverse(x_fft2, FourierOptions.Matlab);
            double acf_Val1 = x_fft2[0].Real;

            double[] acf_Vec = new double[idx.Length];
            double[] acf_Val = new double[numLags];

            // normalize such that acf[0] would be 1.0 and drop the first element
            for (int ii = 0; ii < numLags; ii++)
            {
                acf_Val[ii] = x_fft2[ii + 1].Real / acf_Val1;

            }

            // only return requested lags
            for (int ii = 0; ii < idx.Length; ii++)
            {
                acf_Vec[ii] = acf_Val[idx[ii]];
            }

            return (acf_Vec);
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
                        throw new ArgumentOutOfRangeException("dataB", Resources.ArgumentArraysSameLength);
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
                    throw new ArgumentOutOfRangeException("dataA", Resources.ArgumentArraysSameLength);
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
                        throw new ArgumentOutOfRangeException("dataB", Resources.ArgumentArraysSameLength);
                    }
                    if (!ieW.MoveNext())
                    {
                        throw new ArgumentOutOfRangeException("weights", Resources.ArgumentArraysSameLength);
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
                    throw new ArgumentOutOfRangeException("dataB", Resources.ArgumentArraysSameLength);
                }
                if (ieW.MoveNext())
                {
                    throw new ArgumentOutOfRangeException("weights", Resources.ArgumentArraysSameLength);
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
