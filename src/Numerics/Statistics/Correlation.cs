// <copyright file="Correlation.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
//
// Copyright (c) 2009-2013 Math.NET
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
                    double scaleDeltaA = deltaA / ++n;

                    double deltaB = currentB - meanB;
                    double scaleDeltaB = deltaB / n;

                    meanA += scaleDeltaA;
                    meanB += scaleDeltaB;

                    varA += scaleDeltaA * deltaA * (n - 1);
                    varB += scaleDeltaB * deltaB * (n - 1);
                    r += ((deltaA * deltaB * (n - 1)) / n);
                }
                if (ieB.MoveNext())
                {
                    throw new ArgumentOutOfRangeException("dataA", Resources.ArgumentArraysSameLength);
                }
            }

            return r / Math.Sqrt(varA * varB);
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
                for (int j = i + 1; j < vectors.Length; j++)
                {
                    var c = Pearson(vectors[i], vectors[j]);
                    m.At(i, j, c);
                    m.At(j, i, c);
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

        private static double[] Rank(IEnumerable<double> series)
        {
            if (series == null)
            {
                return new double[0];
            }

            // WARNING: do not try to cast series to an array and use it directly,
            // as we need to sort it (and thus modify id)

            double[] samples = series.ToArray();
            int[] index = new int[samples.Length];
            for (int i = 0; i < index.Length; i++)
            {
                index[i] = i;
            }
            Sorting.Sort(samples, index);

            double[] rankedArray = new double[samples.Length];
            int previousIndex = 0;
            for (int i = 1; i < samples.Length; i++)
            {
                if (Math.Abs(samples[i] - samples[previousIndex]) <= 0d)
                {
                    continue;
                }

                var rankedValue = (i + previousIndex - 1) / 2d + 1;
                for (int k = previousIndex; k < i; k++)
                {
                    rankedArray[index[k]] = rankedValue;
                }

                previousIndex = i;
            }

            var finalValue = (samples.Length + previousIndex - 1) / 2d + 1;
            for (int k = previousIndex; k < index.Length; k++)
            {
                rankedArray[index[k]] = finalValue;
            }

            return rankedArray;
        }
    }
}
