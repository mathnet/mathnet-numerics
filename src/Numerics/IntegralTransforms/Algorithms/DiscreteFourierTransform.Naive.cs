// <copyright file="DiscreteFourierTransform.Naive.cs" company="Math.NET">
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

namespace MathNet.Numerics.IntegralTransforms.Algorithms
{
    using System;
    using System.Numerics;
    using Threading;

    /// <summary>
    /// Complex Fast (FFT) Implementation of the Discrete Fourier Transform (DFT).
    /// </summary>
    public partial class DiscreteFourierTransform
    {
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

        /// <summary>
        /// Naive forward DFT, useful e.g. to verify faster algorithms.
        /// </summary>
        /// <param name="timeSpace">Time-space sample vector.</param>
        /// <param name="options">Fourier Transform Convention Options.</param>
        /// <returns>Corresponding frequency-space vector.</returns>
        public Complex[] NaiveForward(Complex[] timeSpace, FourierOptions options)
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
        public Complex[] NaiveInverse(Complex[] frequencySpace, FourierOptions options)
        {
            var timeSpace = Naive(frequencySpace, -SignByOptions(options));
            InverseScaleByOptions(options, timeSpace);
            return timeSpace;
        }
    }
}
