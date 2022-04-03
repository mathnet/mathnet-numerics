// <copyright file="ManagedFourierTransformProvider.Radix2.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// https://numerics.mathdotnet.com
//
// Copyright (c) 2009-2021 Math.NET
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
using System.Runtime.CompilerServices;
using MathNet.Numerics.Threading;
using Complex = System.Numerics.Complex;

namespace MathNet.Numerics.Providers.FourierTransform
{
    public partial class ManagedFourierTransformProvider
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
                    (samples[i], samples[j]) = (samples[j], samples[i]);
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void Radix2Step(Complex32[] samples, int exponentSign, int levelSize, int k)
        {
            // Twiddle Factor
            var exponent = (exponentSign * k) * Constants.Pi / levelSize;
            var w = new Complex32((float)Math.Cos(exponent), (float)Math.Sin(exponent));

            var step = levelSize << 1;
            for (var i = k; i < samples.Length; i += step)
            {
                var ai = samples[i];
                var t = w * samples[i + levelSize];
                samples[i] = ai + t;
                samples[i + levelSize] = ai - t;
            }
        }

        /// <summary>
        /// Radix-2 Step Helper Method
        /// </summary>
        /// <param name="samples">Sample vector.</param>
        /// <param name="exponentSign">Fourier series exponent sign.</param>
        /// <param name="levelSize">Level Group Size.</param>
        /// <param name="k">Index inside of the level.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void Radix2Step(Complex[] samples, int exponentSign, int levelSize, int k)
        {
            // Twiddle Factor
            var exponent = (exponentSign * k) * Constants.Pi / levelSize;
            var w = new Complex(Math.Cos(exponent), Math.Sin(exponent));

            var step = levelSize << 1;
            for (var i = k; i < samples.Length; i += step)
            {
                var ai = samples[i];
                var t = w * samples[i + levelSize];
                samples[i] = ai + t;
                samples[i + levelSize] = ai - t;
            }
        }

        /// <summary>
        /// Radix-2 generic FFT for power-of-two sized sample vectors.
        /// </summary>
        static void Radix2Forward(Complex32[] data)
        {
            Radix2Reorder(data);
            for (var levelSize = 1; levelSize < data.Length; levelSize *= 2)
            {
                for (var k = 0; k < levelSize; k++)
                {
                    Radix2Step(data, -1, levelSize, k);
                }
            }
        }

        /// <summary>
        /// Radix-2 generic FFT for power-of-two sized sample vectors.
        /// </summary>
        static void Radix2Forward(Complex[] data)
        {
            Radix2Reorder(data);
            for (var levelSize = 1; levelSize < data.Length; levelSize *= 2)
            {
                for (var k = 0; k < levelSize; k++)
                {
                    Radix2Step(data, -1, levelSize, k);
                }
            }
        }

        /// <summary>
        /// Radix-2 generic FFT for power-of-two sized sample vectors.
        /// </summary>
        static void Radix2Inverse(Complex32[] data)
        {
            Radix2Reorder(data);
            for (var levelSize = 1; levelSize < data.Length; levelSize *= 2)
            {
                for (var k = 0; k < levelSize; k++)
                {
                    Radix2Step(data, 1, levelSize, k);
                }
            }
        }

        /// <summary>
        /// Radix-2 generic FFT for power-of-two sized sample vectors.
        /// </summary>
        static void Radix2Inverse(Complex[] data)
        {
            Radix2Reorder(data);
            for (var levelSize = 1; levelSize < data.Length; levelSize *= 2)
            {
                for (var k = 0; k < levelSize; k++)
                {
                    Radix2Step(data, 1, levelSize, k);
                }
            }
        }

        /// <summary>
        /// Radix-2 generic FFT for power-of-two sample vectors (Parallel Version).
        /// </summary>
        static void Radix2ForwardParallel(Complex32[] data)
        {
            Radix2Reorder(data);
            for (var levelSize = 1; levelSize < data.Length; levelSize *= 2)
            {
                var size = levelSize;

                CommonParallel.For(0, size, 64, (u, v) =>
                {
                    for (int i = u; i < v; i++)
                    {
                        Radix2Step(data, -1, size, i);
                    }
                });
            }
        }

        /// <summary>
        /// Radix-2 generic FFT for power-of-two sample vectors (Parallel Version).
        /// </summary>
        static void Radix2ForwardParallel(Complex[] data)
        {
            Radix2Reorder(data);
            for (var levelSize = 1; levelSize < data.Length; levelSize *= 2)
            {
                var size = levelSize;

                CommonParallel.For(0, size, 64, (u, v) =>
                {
                    for (int i = u; i < v; i++)
                    {
                        Radix2Step(data, -1, size, i);
                    }
                });
            }
        }

        /// <summary>
        /// Radix-2 generic FFT for power-of-two sample vectors (Parallel Version).
        /// </summary>
        static void Radix2InverseParallel(Complex32[] data)
        {
            Radix2Reorder(data);
            for (var levelSize = 1; levelSize < data.Length; levelSize *= 2)
            {
                var size = levelSize;

                CommonParallel.For(0, size, 64, (u, v) =>
                {
                    for (int i = u; i < v; i++)
                    {
                        Radix2Step(data, 1, size, i);
                    }
                });
            }
        }

        /// <summary>
        /// Radix-2 generic FFT for power-of-two sample vectors (Parallel Version).
        /// </summary>
        static void Radix2InverseParallel(Complex[] data)
        {
            Radix2Reorder(data);
            for (var levelSize = 1; levelSize < data.Length; levelSize *= 2)
            {
                var size = levelSize;

                CommonParallel.For(0, size, 64, (u, v) =>
                {
                    for (int i = u; i < v; i++)
                    {
                        Radix2Step(data, 1, size, i);
                    }
                });
            }
        }
    }
}
