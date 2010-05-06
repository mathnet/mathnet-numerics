// <copyright file="Transform.cs" company="Math.NET">
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

namespace MathNet.Numerics.IntegralTransforms
{
    using System.Numerics;
    using Algorithms;

    /// <summary>
    /// Integral Transforms (including FFT).
    /// </summary>
    public static class Transform
    {
        /// <summary>
        /// Shared internal DET algorithm.
        /// </summary>
        private static readonly DiscreteFourierTransform _dft = new DiscreteFourierTransform();

        /// <summary>
        /// Applies the forward Fast Fourier Transform (FFT) to arbitrary-length sample vectors.
        /// </summary>
        /// <param name="samples">Sample vector, where the FFT is evaluated in place.</param>
        public static void FourierForward(Complex[] samples)
        {
            _dft.BluesteinForward(samples, FourierOptions.Default);
        }

        /// <summary>
        /// Applies the forward Fast Fourier Transform (FFT) to arbitrary-length sample vectors.
        /// </summary>
        /// <param name="samples">Sample vector, where the FFT is evaluated in place.</param>
        /// <param name="options">Fourier Transform Convention Options.</param>
        public static void FourierForward(Complex[] samples, FourierOptions options)
        {
            _dft.BluesteinForward(samples, options);
        }

        /// <summary>
        /// Applies the inverse Fast Fourier Transform (iFFT) to arbitrary-length sample vectors.
        /// </summary>
        /// <param name="samples">Sample vector, where the FFT is evaluated in place.</param>
        public static void FourierInverse(Complex[] samples)
        {
            _dft.BluesteinInverse(samples, FourierOptions.Default);
        }

        /// <summary>
        /// Applies the inverse Fast Fourier Transform (iFFT) to arbitrary-length sample vectors.
        /// </summary>
        /// <param name="samples">Sample vector, where the FFT is evaluated in place.</param>
        /// <param name="options">Fourier Transform Convention Options.</param>
        public static void FourierInverse(Complex[] samples, FourierOptions options)
        {
            _dft.BluesteinInverse(samples, options);
        }
    }
}
