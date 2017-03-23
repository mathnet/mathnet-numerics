// <copyright file="Fourier.Bluestein.cs" company="Math.NET">
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

using System;
using MathNet.Numerics.Threading;

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
        /// Sequences with length greater than Math.Sqrt(Int32.MaxValue) + 1
        /// will cause k*k in the Bluestein sequence to overflow (GH-286).
        /// </summary>
        const int BluesteinSequenceLengthThreshold = 46341;

        /// <summary>
        /// Generate the bluestein sequence for the provided problem size.
        /// </summary>
        /// <param name="n">Number of samples.</param>
        /// <returns>Bluestein sequence exp(I*Pi*k^2/N)</returns>
        static Complex32[] BluesteinSequence32(int n)
        {
            double s = Constants.Pi / n;
            var sequence = new Complex32[n];

            // TODO: benchmark whether the second variation is significantly
            // faster than the former one. If not just use the former one always.
            if (n > BluesteinSequenceLengthThreshold)
            {
                for (int k = 0; k < sequence.Length; k++)
                {
                    double t = (s * k) * k;
                    sequence[k] = new Complex32((float)Math.Cos(t), (float)Math.Sin(t));
                }
            }
            else
            {
                for (int k = 0; k < sequence.Length; k++)
                {
                    double t = s * (k * k);
                    sequence[k] = new Complex32((float)Math.Cos(t), (float)Math.Sin(t));
                }
            }

            return sequence;
        }

        /// <summary>
        /// Generate the bluestein sequence for the provided problem size.
        /// </summary>
        /// <param name="n">Number of samples.</param>
        /// <returns>Bluestein sequence exp(I*Pi*k^2/N)</returns>
        static Complex[] BluesteinSequence(int n)
        {
            double s = Constants.Pi/n;
            var sequence = new Complex[n];

            // TODO: benchmark whether the second variation is significantly
            // faster than the former one. If not just use the former one always.
            if (n > BluesteinSequenceLengthThreshold)
            {
                for (int k = 0; k < sequence.Length; k++)
                {
                    double t = (s*k)*k;
                    sequence[k] = new Complex(Math.Cos(t), Math.Sin(t));
                }
            }
            else
            {
                for (int k = 0; k < sequence.Length; k++)
                {
                    double t = s*(k*k);
                    sequence[k] = new Complex(Math.Cos(t), Math.Sin(t));
                }
            }

            return sequence;
        }

        /// <summary>
        /// Convolution with the bluestein sequence (Parallel Version).
        /// </summary>
        /// <param name="samples">Sample Vector.</param>
        static void BluesteinConvolutionParallel(Complex32[] samples)
        {
            int n = samples.Length;
            Complex32[] sequence = BluesteinSequence32(n);

            // Padding to power of two >= 2N–1 so we can apply Radix-2 FFT.
            int m = ((n << 1) - 1).CeilingToPowerOfTwo();
            var b = new Complex32[m];
            var a = new Complex32[m];

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

            var nbinv = 1.0f / m;
            for (int i = 0; i < samples.Length; i++)
            {
                samples[i] = nbinv * sequence[i].Conjugate() * a[i];
            }
        }

        /// <summary>
        /// Convolution with the bluestein sequence (Parallel Version).
        /// </summary>
        /// <param name="samples">Sample Vector.</param>
        static void BluesteinConvolutionParallel(Complex[] samples)
        {
            int n = samples.Length;
            Complex[] sequence = BluesteinSequence(n);

            // Padding to power of two >= 2N–1 so we can apply Radix-2 FFT.
            int m = ((n << 1) - 1).CeilingToPowerOfTwo();
            var b = new Complex[m];
            var a = new Complex[m];

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
                        a[i] = sequence[i].Conjugate()*samples[i];
                    }

                    Radix2(a, -1);
                });

            for (int i = 0; i < a.Length; i++)
            {
                a[i] *= b[i];
            }

            Radix2Parallel(a, 1);

            var nbinv = 1.0/m;
            for (int i = 0; i < samples.Length; i++)
            {
                samples[i] = nbinv*sequence[i].Conjugate()*a[i];
            }
        }

        /// <summary>
        /// Swap the real and imaginary parts of each sample.
        /// </summary>
        /// <param name="samples">Sample Vector.</param>
        static void SwapRealImaginary(Complex32[] samples)
        {
            for (int i = 0; i < samples.Length; i++)
            {
                samples[i] = new Complex32(samples[i].Imaginary, samples[i].Real);
            }
        }

        /// <summary>
        /// Swap the real and imaginary parts of each sample.
        /// </summary>
        /// <param name="samples">Sample Vector.</param>
        static void SwapRealImaginary(Complex[] samples)
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
        internal static void Bluestein(Complex32[] samples, int exponentSign)
        {
            int n = samples.Length;
            if (n.IsPowerOfTwo())
            {
                Radix2Parallel(samples, exponentSign);
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
        /// Bluestein generic FFT for arbitrary sized sample vectors.
        /// </summary>
        /// <param name="samples">Time-space sample vector.</param>
        /// <param name="exponentSign">Fourier series exponent sign.</param>
        internal static void Bluestein(Complex[] samples, int exponentSign)
        {
            int n = samples.Length;
            if (n.IsPowerOfTwo())
            {
                Radix2Parallel(samples, exponentSign);
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
    }
}
