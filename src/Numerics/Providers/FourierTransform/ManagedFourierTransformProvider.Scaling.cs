// <copyright file="ManagedFourierTransformProvider.Scaling.cs" company="Math.NET">
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
using Complex = System.Numerics.Complex;

namespace MathNet.Numerics.Providers.FourierTransform
{
    public partial class ManagedFourierTransformProvider
    {
        /// <summary>
        /// Fully rescale the FFT result.
        /// </summary>
        /// <param name="samples">Sample Vector.</param>
        static void FullRescale(Complex32[] samples)
        {
            var scalingFactor = (float)1.0 / samples.Length;
            for (int i = 0; i < samples.Length; i++)
            {
                samples[i] *= scalingFactor;
            }
        }

        /// <summary>
        /// Fully rescale the FFT result.
        /// </summary>
        /// <param name="samples">Sample Vector.</param>
        static void FullRescale(Complex[] samples)
        {
            var scalingFactor = 1.0 / samples.Length;
            for (int i = 0; i < samples.Length; i++)
            {
                samples[i] *= scalingFactor;
            }
        }

        /// <summary>
        /// Half rescale the FFT result (e.g. for symmetric transforms).
        /// </summary>
        /// <param name="samples">Sample Vector.</param>
        static void HalfRescale(Complex32[] samples)
        {
            var scalingFactor = (float)Math.Sqrt(1.0 / samples.Length);
            for (int i = 0; i < samples.Length; i++)
            {
                samples[i] *= scalingFactor;
            }
        }

        /// <summary>
        /// Fully rescale the FFT result (e.g. for symmetric transforms).
        /// </summary>
        /// <param name="samples">Sample Vector.</param>
        static void HalfRescale(Complex[] samples)
        {
            var scalingFactor = Math.Sqrt(1.0 / samples.Length);
            for (int i = 0; i < samples.Length; i++)
            {
                samples[i] *= scalingFactor;
            }
        }
    }
}
