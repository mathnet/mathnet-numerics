// <copyright file="Fourier.Naive.cs" company="Math.NET">
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
using MathNet.Numerics.Threading;

namespace MathNet.Numerics.IntegralTransforms
{
#if !NOSYSNUMERICS
    using Complex = System.Numerics.Complex;
#endif

    /// <summary>
    /// Complex Fast (FFT) Implementation of the Discrete Fourier Transform (DFT).
    /// </summary>
    public static partial class Fourier
    {
        /// <summary>
        /// Naive generic DFT, useful e.g. to verify faster algorithms.
        /// </summary>
        /// <param name="samples">Time-space sample vector.</param>
        /// <param name="exponentSign">Fourier series exponent sign.</param>
        /// <returns>Corresponding frequency-space vector.</returns>
        internal static Complex32[] Naive(Complex32[] samples, int exponentSign)
        {
            var w0 = exponentSign * Constants.Pi2 / samples.Length;
            var spectrum = new Complex32[samples.Length];

            CommonParallel.For(0, samples.Length, (u, v) =>
            {
                for (int i = u; i < v; i++)
                {
                    var wk = w0 * i;
                    var sum = Complex32.Zero;
                    for (var n = 0; n < samples.Length; n++)
                    {
                        var w = n * wk;
                        sum += samples[n] * new Complex32((float)Math.Cos(w), (float)Math.Sin(w));
                    }

                    spectrum[i] = sum;
                }
            });

            return spectrum;
        }

        /// <summary>
        /// Naive generic DFT, useful e.g. to verify faster algorithms.
        /// </summary>
        /// <param name="samples">Time-space sample vector.</param>
        /// <param name="exponentSign">Fourier series exponent sign.</param>
        /// <returns>Corresponding frequency-space vector.</returns>
        internal static Complex[] Naive(Complex[] samples, int exponentSign)
        {
            var w0 = exponentSign*Constants.Pi2/samples.Length;
            var spectrum = new Complex[samples.Length];

            CommonParallel.For(0, samples.Length, (u, v) =>
            {
                for (int i = u; i < v; i++)
                {
                    var wk = w0*i;
                    var sum = Complex.Zero;
                    for (var n = 0; n < samples.Length; n++)
                    {
                        var w = n*wk;
                        sum += samples[n]*new Complex(Math.Cos(w), Math.Sin(w));
                    }

                    spectrum[i] = sum;
                }
            });

            return spectrum;
        }
    }
}
