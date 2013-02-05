// <copyright file="IirCoefficients.cs" company="Math.NET">
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

namespace MathNet.Numerics.Filtering.IIR
{
    using System;

    /// <summary>
    /// IirCoefficients provides basic coefficient evaluation
    /// algorithms for the four most important filter types for
    /// Infinite Impulse Response (IIR) Filters.
    /// </summary>
    public static class IirCoefficients
    {
        /// <summary>
        /// Calculates IIR LowPass Filter Coefficients.
        /// </summary>
        /// <param name="samplingRate">Samples per unit.</param>
        /// <param name="cutoff">Cutoff frequency in samples per unit.</param>
        /// <param name="width">bandwidth in samples per unit.</param>
        /// <returns>The calculated filter coefficients.</returns>
        public static
        double[]
        LowPass(
            double samplingRate,
            double cutoff,
            double width
            )
        {
            double beta, gamma, theta;

            BetaGamma(
                out beta,
                out gamma,
                out theta,
                samplingRate,
                cutoff,
                0d, // lowHalfPower
                width // highHalfPower
                );

            return BuildCoefficients(
                beta,
                gamma,
                (0.5d + beta - gamma) * 0.25d, // alpha
                2, // mu
                1 // sigma
                );
        }

        /// <summary>
        /// Calculates IIR HighPass Filter Coefficients.
        /// </summary>
        /// <param name="samplingRate">Samples per unit.</param>
        /// <param name="cutoff">Cutoff frequency in samples per unit.</param>
        /// <param name="width">bandwidth in samples per unit.</param>
        /// <returns>The calculated filter coefficients.</returns>
        public static
        double[]
        HighPass(
            double samplingRate,
            double cutoff,
            double width
            )
        {
            double beta, gamma, theta;

            BetaGamma(
                out beta,
                out gamma,
                out theta,
                samplingRate,
                cutoff,
                0d, // lowHalfPower
                width // highHalfPower
                );

            return BuildCoefficients(
                beta,
                gamma,
                (0.5d + beta + gamma) * 0.25d, // alpha
                -2, // mu
                1 // sigmas
                );
        }

        /// <summary>
        /// Calculates IIR Bandpass Filter Coefficients.
        /// </summary>
        /// <param name="samplingRate">Samples per unit.</param>
        /// <param name="cutoffLow">Low Cutoff frequency in samples per unit.</param>
        /// <param name="cutoffHigh">High Cutoff frequency in samples per unit.</param>
        /// <returns>The calculated filter coefficients.</returns>
        public static
        double[]
        BandPass(
            double samplingRate,
            double cutoffLow,
            double cutoffHigh
            )
        {
            double beta, gamma, theta;

            BetaGamma(
                out beta,
                out gamma,
                out theta,
                samplingRate,
                (cutoffLow + cutoffHigh) * 0.5d, // cutoff
                cutoffLow, // lowHalfPower
                cutoffHigh // highHalfPower
                );

            return BuildCoefficients(
                beta,
                gamma,
                (0.5d - beta) * 0.5d, // alpha
                0, // mu
                -1 // sigma
                );
        }

        /// <summary>
        /// Calculates IIR Bandstop Filter Coefficients.
        /// </summary>
        /// <param name="samplingRate">Samples per unit.</param>
        /// <param name="cutoffLow">Low Cutoff frequency in samples per unit.</param>
        /// <param name="cutoffHigh">High Cutoff frequency in samples per unit.</param>
        /// <returns>The calculated filter coefficients.</returns>
        public static
        double[]
        BandStop(
            double samplingRate,
            double cutoffLow,
            double cutoffHigh
            )
        {
            double beta, gamma, theta;

            BetaGamma(
                out beta,
                out gamma,
                out theta,
                samplingRate,
                (cutoffLow + cutoffHigh) * 0.5d, // cutoff
                cutoffLow, // lowHalfPower
                cutoffHigh // highHalfPower
                );

            return BuildCoefficients(
                beta,
                gamma,
                (0.5d + beta) * 0.5d, // alpha
                -2 * Math.Cos(theta), // mu
                1 // sigma
                );
        }

        static
        double[]
        BuildCoefficients(
            double beta,
            double gamma,
            double alpha,
            double mu,
            double sigma
            )
        {
            return new double[] {
                2d*alpha,
                2d*gamma,
                -2d*beta,
                1,
                mu,
                sigma
                };
        }

        static
        void
        BetaGamma(
            out double beta,
            out double gamma,
            out double theta,
            double sampling,
            double cutoff,
            double lowHalfPower,
            double highHalfPower
            )
        {
            double tan = Math.Tan(Math.PI * (highHalfPower - lowHalfPower) / sampling);
            beta = 0.5d * (1 - tan) / (1 + tan);
            theta = 2 * Math.PI * cutoff / sampling;
            gamma = (0.5d + beta) * Math.Cos(theta);
        }

    }
}
