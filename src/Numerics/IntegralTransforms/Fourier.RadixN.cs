// <copyright file="Fourier.RadixN.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2016 Math.NET
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
using MathNet.Numerics.Properties;
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
        /// Radix-2 Reorder Helper Method
        /// </summary>
        /// <typeparam name="T">Sample type</typeparam>
        /// <param name="samples">Sample vector</param>
        static void Radix2Reorder<T>(T[] samples)
        {
            var j = 0;
            for (var i = 0; i < samples.Length - 1; i++)
            {
                if (i < j)
                {
                    var temp = samples[i];
                    samples[i] = samples[j];
                    samples[j] = temp;
                }

                var m = samples.Length;

                do
                {
                    m >>= 1;
                    j ^= m;
                }
                while ((j & m) == 0);
            }
        }

        /// <summary>
        /// Radix-2 Step Helper Method
        /// </summary>
        /// <param name="samples">Sample vector.</param>
        /// <param name="exponentSign">Fourier series exponent sign.</param>
        /// <param name="levelSize">Level Group Size.</param>
        /// <param name="k">Index inside of the level.</param>
        static void Radix2Step(Complex[] samples, int exponentSign, int levelSize, int k)
        {
            // Twiddle Factor
            var exponent = (exponentSign*k)*Constants.Pi/levelSize;
            var w = new Complex(Math.Cos(exponent), Math.Sin(exponent));

            var step = levelSize << 1;
            for (var i = k; i < samples.Length; i += step)
            {
                var ai = samples[i];
                var t = w*samples[i + levelSize];
                samples[i] = ai + t;
                samples[i + levelSize] = ai - t;
            }
        }

        /// <summary>
        /// Radix-2 generic FFT for power-of-two sized sample vectors.
        /// </summary>
        /// <param name="samples">Sample vector, where the FFT is evaluated in place.</param>
        /// <param name="positiveExponentSign">Fourier series exponent sign: true for positive, false for negative.</param>
        /// <exception cref="ArgumentException"/>
        internal static void Radix2(Complex[] samples, bool positiveExponentSign)
        {
            if (!samples.Length.IsPowerOfTwo())
            {
                throw new ArgumentException(Resources.ArgumentPowerOfTwo);
            }

            Radix2Reorder(samples);

            for (int level = 0, levelSize = 1; levelSize < samples.Length; level++, levelSize *= 2)
            {
                Complex[] twiddles = TwiddleFactors(level);
                for (int k = 0; k < levelSize; k++)
                {
                    Complex twiddle = positiveExponentSign ? twiddles[k] : twiddles[k].Conjugate();
                    int step = levelSize << 1;
                    for (int i = k; i < samples.Length; i += step)
                    {
                        Complex ai = samples[i];
                        Complex t = twiddle * samples[i + levelSize];
                        samples[i] = ai + t;
                        samples[i + levelSize] = ai - t;
                    }
                }
            }
        }

        /// <summary>
        /// Radix-2 generic FFT for power-of-two sample vectors (Parallel Version).
        /// </summary>
        /// <param name="samples">Sample vector, where the FFT is evaluated in place.</param>
        /// <param name="positiveExponentSign">Fourier series exponent sign: true for positive, false for negative.</param>
        /// <exception cref="ArgumentException"/>
        internal static void Radix2Parallel(Complex[] samples, bool positiveExponentSign)
        {
            if (!samples.Length.IsPowerOfTwo())
            {
                throw new ArgumentException(Resources.ArgumentPowerOfTwo);
            }

            Radix2Reorder(samples);

            for (int level = 0, levelSize = 1; levelSize < samples.Length; level++, levelSize *= 2)
            {
                Complex[] twiddles = TwiddleFactors(level);
                var step = levelSize << 1;


                int size = levelSize;
                CommonParallel.For(0, size, 64, (u, v) =>
                {
                    for (int k = u; k < v; k++)
                    {
                        Complex twiddle = twiddles[k];
                        double tr = twiddle.Real;
                        double ti = positiveExponentSign ? twiddle.Imaginary : -twiddle.Imaginary;
                        for (var i = k; i < samples.Length; i += step)
                        {
                            var ai = samples[i];
                            var b = samples[i + size];
                            double ttr = tr * b.Real - ti * b.Imaginary;
                            double tti = ti * b.Real + tr * b.Imaginary;
                            samples[i] = new Complex(ai.Real + ttr, ai.Imaginary + tti);
                            samples[i + size] = new Complex(ai.Real - ttr, ai.Imaginary - tti);
                        }
                    }
                });
            }
        }

        static readonly Complex[][] Twiddle = new Complex[64][];

        static Complex[] TwiddleFactors(int level)
        {
            Complex[] tf = Twiddle[level];
            if (tf == null)
            {
                tf = new Complex[level.PowerOfTwo()];
                for (int i = 0; i < tf.Length; i++)
                {
                    double exponent = i * Constants.Pi / tf.Length;
                    tf[i] = new Complex(Math.Cos(exponent), Math.Sin(exponent));
                }
                Twiddle[level] = tf;
            }

            return tf;
        }
    }
}
