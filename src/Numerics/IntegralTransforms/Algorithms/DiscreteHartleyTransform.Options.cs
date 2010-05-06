// <copyright file="DiscreteHartleyTransform.Options.cs" company="Math.NET">
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

    /// <summary>
    /// Fast (FHT) Implementation of the Discrete Hartley Transform (DHT).
    /// </summary>
    public partial class DiscreteHartleyTransform
    {
        /// <summary>
        /// Rescale FFT-the resulting vector according to the provided convention options.
        /// </summary>
        /// <param name="options">Fourier Transform Convention Options.</param>
        /// <param name="samples">Sample Vector.</param>
        private static void ForwardScaleByOptions(HartleyOptions options, double[] samples)
        {
            if ((options & HartleyOptions.NoScaling) == HartleyOptions.NoScaling ||
                (options & HartleyOptions.AsymmetricScaling) == HartleyOptions.AsymmetricScaling)
            {
                return;
            }

            var scalingFactor = Math.Sqrt(1.0 / samples.Length);
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
        private static void InverseScaleByOptions(HartleyOptions options, double[] samples)
        {
            if ((options & HartleyOptions.NoScaling) == HartleyOptions.NoScaling)
            {
                return;
            }

            var scalingFactor = 1.0 / samples.Length;
            if ((options & HartleyOptions.AsymmetricScaling) != HartleyOptions.AsymmetricScaling)
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
