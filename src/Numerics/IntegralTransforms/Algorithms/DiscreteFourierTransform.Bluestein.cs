// <copyright file="DiscreteFourierTransform.Bluestein.cs" company="Math.NET">
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

namespace MathNet.Numerics.IntegralTransforms.Algorithms
{
    using System;
    using System.Numerics;
    using NumberTheory;
    using Threading;

    /// <summary>
    /// Complex Fast (FFT) Implementation of the Discrete Fourier Transform (DFT).
    /// </summary>
    public partial class DiscreteFourierTransform
    {
        /// <summary>
        /// Generate the bluestein sequence for the provided problem size.
        /// </summary>
        /// <param name="n">Number of samples.</param>
        /// <returns>Bluestein sequence exp(I*Pi*k^2/N)</returns>
        private static Complex[] BluesteinSequence(int n)
        {
            double s = Constants.Pi / n;
            var sequence = new Complex[n];

            for (int k = 0; k < sequence.Length; k++)
            {
                double t = s * (k * k);
                sequence[k] = new Complex(Math.Cos(t), Math.Sin(t));
            }

            return sequence;
        }

        /// <summary>
        /// Convolution with the bluestein sequence (Parallel Version).
        /// </summary>
        /// <param name="samples">Sample Vector.</param>
        private static void BluesteinConvolutionParallel(Complex[] samples)
        {
            int n = samples.Length;
            Complex[] sequence = BluesteinSequence(n);

            // Padding to power of two >= 2N–1 so we can apply Radix-2 FFT.
            int m = ((n << 1) - 1).CeilingToPowerOfTwo();
            Complex[] b = new Complex[m];
            Complex[] a = new Complex[m];

            CommonParallel.Invoke(
                () =>
                {
                    // Build and transform padded sequence b_k = exp(I*Pi*k^2/N)
                    for (int i = 0; i < n; i++)
                    {
                        b[i] = sequence[i];
                    }

                    for (int i = m - n + 1; i < b.Length; i++)
                    {
                        b[i] = sequence[m - i];
                    }

                    Radix2(b, -1);
                },
                () =>
                {
                    // Build and transform padded sequence a_k = x_k * exp(-I*Pi*k^2/N)
                    for (int i = 0; i < samples.Length; i++)
                    {
                        a[i] = sequence[i].Conjugate() * samples[i];
                    }

                    Radix2(a, -1);
                });

            for (int i = 0; i < a.Length; i++)
            {
                a[i] *= b[i];
            }

            Radix2Parallel(a, 1);

            var nbinv = 1.0 / m;
            for (int i = 0; i < samples.Length; i++)
            {
                samples[i] = nbinv * sequence[i].Conjugate() * a[i];
            }
        }

        /// <summary>
        /// Swap the real and imaginary parts of each sample.
        /// </summary>
        /// <param name="samples">Sample Vector.</param>
        private static void SwapRealImaginary(Complex[] samples)
        {
            for (int i = 0; i < samples.Length; i++)
            {
                samples[i] = new Complex(samples[i].Imaginary, samples[i].Real);
            }
        }

        /// <summary>
        /// Bluestein generic FFT for arbitrary sized sample vectors.
        /// </summary>
        /// <param name="samples">Time-space sample vector.</param>
        /// <param name="exponentSign">Fourier series exponent sign.</param>
        internal static void Bluestein(Complex[] samples, int exponentSign)
        {
            int n = samples.Length;
            if (n.IsPowerOfTwo())
            {
                Radix2(samples, exponentSign);
                return;
            }

            if (exponentSign == 1)
            {
                SwapRealImaginary(samples);
            }

            BluesteinConvolutionParallel(samples);

            if (exponentSign == 1)
            {
                SwapRealImaginary(samples);
            }
        }

        /// <summary>
        /// Bluestein forward FFT for arbitrary sized sample vectors.
        /// </summary>
        /// <param name="samples">Sample vector, where the FFT is evaluated in place.</param>
        /// <param name="options">Fourier Transform Convention Options.</param>
        public void BluesteinForward(Complex[] samples, FourierOptions options)
        {
            Bluestein(samples, SignByOptions(options));
            ForwardScaleByOptions(options, samples);
        }

        /// <summary>
        /// Bluestein inverse FFT for arbitrary sized sample vectors.
        /// </summary>
        /// <param name="samples">Sample vector, where the FFT is evaluated in place.</param>
        /// <param name="options">Fourier Transform Convention Options.</param>
        public void BluesteinInverse(Complex[] samples, FourierOptions options)
        {
            Bluestein(samples, -SignByOptions(options));
            InverseScaleByOptions(options, samples);
        }
    }
}