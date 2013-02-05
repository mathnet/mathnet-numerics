// <copyright file="FirCoefficients.cs" company="Math.NET">
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

namespace MathNet.Numerics.Filtering.FIR
{
    using System;

    /// <summary>
    /// FirCoefficients provides basic coefficient evaluation
    /// algorithms for the four most important filter types for
    /// Finite Impulse Response (FIR) Filters.
    /// </summary>
    public static class FirCoefficients
    {
        /// <summary>
        /// Calculates FIR LowPass Filter Coefficients.
        /// </summary>
        /// <param name="samplingRate">Samples per unit.</param>
        /// <param name="cutoff">Cutoff frequency in samples per unit.</param>
        /// <param name="halforder">halforder Q, so that Order N = 2*Q+1. Usually between 20 and 150.</param>
        /// <returns>The calculated filter coefficients.</returns>
        public static
        double[]
        LowPass(
            double samplingRate,
            double cutoff,
            int halforder
            )
        {
            double nu = 2d * cutoff / samplingRate; //normalized frequency
            int order = 2 * halforder + 1;
            double[] c = new double[order];
            c[halforder] = nu;
            for(int i = 0, n = halforder; i < halforder; i++, n--)
            {
                double npi = n * Math.PI;
                c[i] = Math.Sin(npi * nu) / npi;
                c[n + halforder] = c[i];
            }
            return c;
        }

        /// <summary>
        /// Calculates FIR HighPass Filter Coefficients.
        /// </summary>
        /// <param name="samplingRate">Samples per unit.</param>
        /// <param name="cutoff">Cutoff frequency in samples per unit.</param>
        /// <param name="halforder">halforder Q, so that Order N = 2*Q+1</param>
        /// <returns>The calculated filter coefficients.</returns>
        public static
        double[]
        HighPass(
            double samplingRate,
            double cutoff,
            int halforder
            )
        {
            double nu = 2d * cutoff / samplingRate; //normalized frequency
            int order = 2 * halforder + 1;
            double[] c = new double[order];
            c[halforder] = 1 - nu;
            for(int i = 0, n = halforder; i < halforder; i++, n--)
            {
                double npi = n * Math.PI;
                c[i] = -Math.Sin(npi * nu) / npi;
                c[n + halforder] = c[i];
            }
            return c;
        }

        /// <summary>
        /// Calculates FIR Bandpass Filter Coefficients.
        /// </summary>
        /// <param name="samplingRate">Samples per unit.</param>
        /// <param name="cutoffLow">Low Cutoff frequency in samples per unit.</param>
        /// <param name="cutoffHigh">High Cutoff frequency in samples per unit.</param>
        /// <param name="halforder">halforder Q, so that Order N = 2*Q+1</param>
        /// <returns>The calculated filter coefficients.</returns>
        public static
        double[]
        BandPass(
            double samplingRate,
            double cutoffLow,
            double cutoffHigh,
            int halforder
            )
        {
            double nu1 = 2d * cutoffLow / samplingRate; //normalized low frequency
            double nu2 = 2d * cutoffHigh / samplingRate; //normalized high frequency
            int order = 2 * halforder + 1;
            double[] c = new double[order];
            c[halforder] = nu2 - nu1;
            for(int i = 0, n = halforder; i < halforder; i++, n--)
            {
                double npi = n * Math.PI;
                c[i] = (Math.Sin(npi * nu2) - Math.Sin(npi * nu1)) / npi;
                c[n + halforder] = c[i];
            }
            return c;
        }

        /// <summary>
        /// Calculates FIR Bandstop Filter Coefficients.
        /// </summary>
        /// <param name="samplingRate">Samples per unit.</param>
        /// <param name="cutoffLow">Low Cutoff frequency in samples per unit.</param>
        /// <param name="cutoffHigh">High Cutoff frequency in samples per unit.</param>
        /// <param name="halforder">halforder Q, so that Order N = 2*Q+1</param>
        /// <returns>The calculated filter coefficients.</returns>
        public static
        double[]
        BandStop(
            double samplingRate,
            double cutoffLow,
            double cutoffHigh,
            int halforder
            )
        {
            double nu1 = 2d * cutoffLow / samplingRate; //normalized low frequency
            double nu2 = 2d * cutoffHigh / samplingRate; //normalized high frequency
            int order = 2 * halforder + 1;
            double[] c = new double[order];
            c[halforder] = 1 - (nu2 - nu1);
            for(int i = 0, n = halforder; i < halforder; i++, n--)
            {
                double npi = n * Math.PI;
                c[i] = (Math.Sin(npi * nu1) - Math.Sin(npi * nu2)) / npi;
                c[n + halforder] = c[i];
            }
            return c;
        }
    }
}
