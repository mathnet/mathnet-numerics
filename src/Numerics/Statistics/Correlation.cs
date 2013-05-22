// <copyright file="Correlation.cs" company="Math.NET">
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

namespace MathNet.Numerics.Statistics
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// A class with correlation measures between two datasets.
    /// </summary>
    public static class Correlation
    {
        /// <summary>
        /// Computes the Pearson product-moment correlation coefficient.
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

            using (IEnumerator<double> ieA = dataA.GetEnumerator())
            using (IEnumerator<double> ieB = dataB.GetEnumerator())
            {
                while (ieA.MoveNext())
                {
                    if (!ieB.MoveNext())
                    {
                        throw new ArgumentOutOfRangeException("dataB", "Datasets dataA and dataB need to have the same length. dataB is shorter.");
                    }
                    double Acurrent = ieA.Current;
                    double Bcurrent = ieB.Current;

                    double deltaA = Acurrent - meanA;
                    double scaleDeltaA = deltaA / ++n;

                    double deltaB = Bcurrent - meanB;
                    double scaleDeltaB = deltaB / n;

                    meanA += scaleDeltaA;
                    meanB += scaleDeltaB;

                    varA += scaleDeltaA * deltaA * (n - 1);
                    varB += scaleDeltaB * deltaB * (n - 1);
                    r += ((deltaA * deltaB * (n - 1)) / n);
                }
                if (ieB.MoveNext())
                {
                    throw new ArgumentOutOfRangeException("dataA", "Datasets dataA and dataB need to have the same length. dataA is shorter.");
                }
            }

            return r / Math.Sqrt(varA * varB);
        }

        /// <summary>
        /// Computes the Spearman Ranked Correlation Coefficient.
        /// </summary>
        /// <param name="dataA">Sample data series A.</param>
        /// <param name="dataB">Sample data series B.</param>
        /// <returns>The Spearman Ranked Correlation Coefficient.</returns>
        public static double Spearman(IEnumerable<double> dataA, IEnumerable<double> dataB)
        {
            return Pearson(RankedSeries(dataA.ToList()), RankedSeries(dataB.ToList()));
        }

        private static IEnumerable<double> RankedSeries(ICollection<double> series)
        {
            if (series == null || series.Count == 0)
                return Enumerable.Empty<double>();

            var rankedSamples = series.Select((sample, index) => new { Sample = sample, RankIndex = index }).OrderBy(s => s.Sample).ToList();

            var rankedArray = new double[series.Count];

            var previousSample = rankedSamples.Select((sampleIndex, index) => new { SampleIndex = sampleIndex, LoopIndex = index }).First();
            foreach (var rankedSampleIndex in rankedSamples.Select((sampleIndex, index) => new { SampleIndex = sampleIndex, LoopIndex = index }))
            {
                var currentSample = rankedSampleIndex;

                if (Math.Abs(currentSample.SampleIndex.Sample - previousSample.SampleIndex.Sample) <= 0)
                    continue;

                var rankedValue = (currentSample.LoopIndex + previousSample.LoopIndex - 1) / 2d + 1;
                foreach (var index in Enumerable.Range(previousSample.LoopIndex, currentSample.LoopIndex - previousSample.LoopIndex))
                    rankedArray[rankedSamples[index].RankIndex] = rankedValue;

                previousSample = currentSample;
            }

            var finalValue = (rankedSamples.Count + previousSample.LoopIndex - 1) / 2d + 1;
            foreach (var index in Enumerable.Range(previousSample.LoopIndex, rankedSamples.Count - previousSample.LoopIndex))
                rankedArray[rankedSamples[index].RankIndex] = finalValue;

            return rankedArray;
        }
    }
}
