// <copyright file="Fourier.cs" company="Math.NET">
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

using System;

namespace MathNet.Numerics.IntegralTransforms
{
#if !NOSYSNUMERICS
    using System.Numerics;
#endif

    /// <summary>
    /// Complex Fast (FFT) Implementation of the Discrete Fourier Transform (DFT).
    /// </summary>
    public static partial class Fourier
    {
        /// <summary>
        /// Applies the forward Fast Fourier Transform (FFT) to arbitrary-length sample vectors.
        /// </summary>
        /// <param name="samples">Sample vector, where the FFT is evaluated in place.</param>
        public static void Forward(Complex[] samples)
        {
            BluesteinForward(samples, FourierOptions.Default);
        }

        /// <summary>
        /// Applies the forward Fast Fourier Transform (FFT) to arbitrary-length sample vectors.
        /// </summary>
        /// <param name="samples">Sample vector, where the FFT is evaluated in place.</param>
        /// <param name="options">Fourier Transform Convention Options.</param>
        public static void Forward(Complex[] samples, FourierOptions options)
        {
            BluesteinForward(samples, options);
        }

        /// <summary>
        /// Applies the inverse Fast Fourier Transform (iFFT) to arbitrary-length sample vectors.
        /// </summary>
        /// <param name="samples">Sample vector, where the FFT is evaluated in place.</param>
        public static void Inverse(Complex[] samples)
        {
            BluesteinInverse(samples, FourierOptions.Default);
        }

        /// <summary>
        /// Applies the inverse Fast Fourier Transform (iFFT) to arbitrary-length sample vectors.
        /// </summary>
        /// <param name="samples">Sample vector, where the FFT is evaluated in place.</param>
        /// <param name="options">Fourier Transform Convention Options.</param>
        public static void Inverse(Complex[] samples, FourierOptions options)
        {
            BluesteinInverse(samples, options);
        }

        /// <summary>
        /// Naive forward DFT, useful e.g. to verify faster algorithms.
        /// </summary>
        /// <param name="timeSpace">Time-space sample vector.</param>
        /// <param name="options">Fourier Transform Convention Options.</param>
        /// <returns>Corresponding frequency-space vector.</returns>
        public static Complex[] NaiveForward(Complex[] timeSpace, FourierOptions options)
        {
            var frequencySpace = Naive(timeSpace, SignByOptions(options));
            ForwardScaleByOptions(options, frequencySpace);
            return frequencySpace;
        }

        /// <summary>
        /// Naive inverse DFT, useful e.g. to verify faster algorithms.
        /// </summary>
        /// <param name="frequencySpace">Frequency-space sample vector.</param>
        /// <param name="options">Fourier Transform Convention Options.</param>
        /// <returns>Corresponding time-space vector.</returns>
        public static Complex[] NaiveInverse(Complex[] frequencySpace, FourierOptions options)
        {
            var timeSpace = Naive(frequencySpace, -SignByOptions(options));
            InverseScaleByOptions(options, timeSpace);
            return timeSpace;
        }

        /// <summary>
        /// Radix-2 forward FFT for power-of-two sized sample vectors.
        /// </summary>
        /// <param name="samples">Sample vector, where the FFT is evaluated in place.</param>
        /// <param name="options">Fourier Transform Convention Options.</param>
        /// <exception cref="ArgumentException"/>
        public static void Radix2Forward(Complex[] samples, FourierOptions options)
        {
            Radix2Parallel(samples, SignByOptions(options));
            ForwardScaleByOptions(options, samples);
        }

        /// <summary>
        /// Radix-2 inverse FFT for power-of-two sized sample vectors.
        /// </summary>
        /// <param name="samples">Sample vector, where the FFT is evaluated in place.</param>
        /// <param name="options">Fourier Transform Convention Options.</param>
        /// <exception cref="ArgumentException"/>
        public static void Radix2Inverse(Complex[] samples, FourierOptions options)
        {
            Radix2Parallel(samples, -SignByOptions(options));
            InverseScaleByOptions(options, samples);
        }

        /// <summary>
        /// Bluestein forward FFT for arbitrary sized sample vectors.
        /// </summary>
        /// <param name="samples">Sample vector, where the FFT is evaluated in place.</param>
        /// <param name="options">Fourier Transform Convention Options.</param>
        public static void BluesteinForward(Complex[] samples, FourierOptions options)
        {
            Bluestein(samples, SignByOptions(options));
            ForwardScaleByOptions(options, samples);
        }

        /// <summary>
        /// Bluestein inverse FFT for arbitrary sized sample vectors.
        /// </summary>
        /// <param name="samples">Sample vector, where the FFT is evaluated in place.</param>
        /// <param name="options">Fourier Transform Convention Options.</param>
        public static void BluesteinInverse(Complex[] samples, FourierOptions options)
        {
            Bluestein(samples, -SignByOptions(options));
            InverseScaleByOptions(options, samples);
        }

        /// <summary>
        /// Extract the exponent sign to be used in forward transforms according to the
        /// provided convention options.
        /// </summary>
        /// <param name="options">Fourier Transform Convention Options.</param>
        /// <returns>Fourier series exponent sign.</returns>
        static int SignByOptions(FourierOptions options)
        {
            return (options & FourierOptions.InverseExponent) == FourierOptions.InverseExponent ? 1 : -1;
        }

        /// <summary>
        /// Rescale FFT-the resulting vector according to the provided convention options.
        /// </summary>
        /// <param name="options">Fourier Transform Convention Options.</param>
        /// <param name="samples">Sample Vector.</param>
        static void ForwardScaleByOptions(FourierOptions options, Complex[] samples)
        {
            if ((options & FourierOptions.NoScaling) == FourierOptions.NoScaling ||
                (options & FourierOptions.AsymmetricScaling) == FourierOptions.AsymmetricScaling)
            {
                return;
            }

            var scalingFactor = Math.Sqrt(1.0/samples.Length);
            for (int i = 0; i < samples.Length; i++)
            {
                samples[i] *= scalingFactor;
            }
        }

        /// <summary>
        /// Rescale the iFFT-resulting vector according to the provided convention options.
        /// </summary>
        /// <param name="options">Fourier Transform Convention Options.</param>
        /// <param name="samples">Sample Vector.</param>
        static void InverseScaleByOptions(FourierOptions options, Complex[] samples)
        {
            if ((options & FourierOptions.NoScaling) == FourierOptions.NoScaling)
            {
                return;
            }

            var scalingFactor = 1.0/samples.Length;
            if ((options & FourierOptions.AsymmetricScaling) != FourierOptions.AsymmetricScaling)
            {
                scalingFactor = Math.Sqrt(scalingFactor);
            }

            for (int i = 0; i < samples.Length; i++)
            {
                samples[i] *= scalingFactor;
            }
        }
    }
}
