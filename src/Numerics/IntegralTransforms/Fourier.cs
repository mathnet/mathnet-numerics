// <copyright file="Fourier.cs" company="Math.NET">
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
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Storage;
using MathNet.Numerics.Providers.FourierTransform;

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
            Control.FourierTransformProvider.ForwardInplace(samples, FourierTransformScaling.SymmetricScaling);
        }

        public static void ForwardMultiDim(Complex[] samples, int[] dimensions)
        {
            Control.FourierTransformProvider.ForwardInplaceMultidim(samples, dimensions, FourierTransformScaling.SymmetricScaling);
        }

        public static void Forward2D(Complex[] samplesRowWise, int rows, int columns)
        {
            ForwardMultiDim(samplesRowWise, new[] {rows, columns});
        }

        public static void Forward2D(Matrix<Complex> samples)
        {
            Forward2D(samples, FourierOptions.Default);
        }

        /// <summary>
        /// Applies the forward Fast Fourier Transform (FFT) to arbitrary-length sample vectors.
        /// </summary>
        /// <param name="samples">Sample vector, where the FFT is evaluated in place.</param>
        /// <param name="options">Fourier Transform Convention Options.</param>
        public static void Forward(Complex[] samples, FourierOptions options)
        {
            switch (options)
            {
                case FourierOptions.NoScaling:
                case FourierOptions.AsymmetricScaling:
                    Control.FourierTransformProvider.ForwardInplace(samples, FourierTransformScaling.NoScaling);
                    break;
                case FourierOptions.InverseExponent:
                    Control.FourierTransformProvider.BackwardInplace(samples, FourierTransformScaling.SymmetricScaling);
                    break;
                case FourierOptions.InverseExponent | FourierOptions.NoScaling:
                case FourierOptions.InverseExponent | FourierOptions.AsymmetricScaling:
                    Control.FourierTransformProvider.BackwardInplace(samples, FourierTransformScaling.NoScaling);
                    break;
                default:
                    Control.FourierTransformProvider.ForwardInplace(samples, FourierTransformScaling.SymmetricScaling);
                    break;
            }
        }

        public static void ForwardMultiDim(Complex[] samples, int[] dimensions, FourierOptions options)
        {
            switch (options)
            {
                case FourierOptions.NoScaling:
                case FourierOptions.AsymmetricScaling:
                    Control.FourierTransformProvider.ForwardInplaceMultidim(samples, dimensions, FourierTransformScaling.NoScaling);
                    break;
                case FourierOptions.InverseExponent:
                    Control.FourierTransformProvider.BackwardInplaceMultidim(samples, dimensions, FourierTransformScaling.SymmetricScaling);
                    break;
                case FourierOptions.InverseExponent | FourierOptions.NoScaling:
                case FourierOptions.InverseExponent | FourierOptions.AsymmetricScaling:
                    Control.FourierTransformProvider.BackwardInplaceMultidim(samples, dimensions, FourierTransformScaling.NoScaling);
                    break;
                default:
                    Control.FourierTransformProvider.ForwardInplaceMultidim(samples, dimensions, FourierTransformScaling.SymmetricScaling);
                    break;
            }
        }

        public static void Forward2D(Complex[] samplesRowWise, int rows, int columns, FourierOptions options)
        {
            ForwardMultiDim(samplesRowWise, new[] { rows, columns }, options);
        }

        public static void Forward2D(Matrix<Complex> samples, FourierOptions options)
        {
            // since dense matrix data is column major, we switch rows and columns

            var denseStorage = samples.Storage as DenseColumnMajorMatrixStorage<Complex>;
            if (denseStorage == null)
            {
                var samplesColumnMajor = samples.ToColumnWiseArray();
                ForwardMultiDim(samplesColumnMajor, new[] { samples.ColumnCount, samples.RowCount }, options);
                denseStorage = new DenseColumnMajorMatrixStorage<Complex>(samples.RowCount, samples.ColumnCount, samplesColumnMajor);
                denseStorage.CopyToUnchecked(samples.Storage, ExistingData.Clear);
                return;
            }

            ForwardMultiDim(denseStorage.Data, new[] { samples.ColumnCount, samples.RowCount }, options);
        }

        /// <summary>
        /// Applies the inverse Fast Fourier Transform (iFFT) to arbitrary-length sample vectors.
        /// </summary>
        /// <param name="spectrum">Sample vector, where the FFT is evaluated in place.</param>
        public static void Inverse(Complex[] spectrum)
        {
            Control.FourierTransformProvider.BackwardInplace(spectrum, FourierTransformScaling.SymmetricScaling);
        }

        public static void InverseMultiDim(Complex[] spectrum, int[] dimensions)
        {
            Control.FourierTransformProvider.BackwardInplaceMultidim(spectrum, dimensions, FourierTransformScaling.SymmetricScaling);
        }

        public static void Inverse2D(Complex[] spectrumRowWise, int rows, int columns)
        {
            InverseMultiDim(spectrumRowWise, new[] { rows, columns });
        }

        public static void Inverse2D(Matrix<Complex> samples)
        {
            Inverse2D(samples, FourierOptions.Default);
        }

        /// <summary>
        /// Applies the inverse Fast Fourier Transform (iFFT) to arbitrary-length sample vectors.
        /// </summary>
        /// <param name="specrum">Sample vector, where the FFT is evaluated in place.</param>
        /// <param name="options">Fourier Transform Convention Options.</param>
        public static void Inverse(Complex[] specrum, FourierOptions options)
        {
            switch (options)
            {
                case FourierOptions.NoScaling:
                    Control.FourierTransformProvider.BackwardInplace(specrum, FourierTransformScaling.NoScaling);
                    break;
                case FourierOptions.AsymmetricScaling:
                    Control.FourierTransformProvider.BackwardInplace(specrum, FourierTransformScaling.BackwardScaling);
                    break;
                case FourierOptions.InverseExponent:
                    Control.FourierTransformProvider.ForwardInplace(specrum, FourierTransformScaling.SymmetricScaling);
                    break;
                case FourierOptions.InverseExponent | FourierOptions.NoScaling:
                    Control.FourierTransformProvider.ForwardInplace(specrum, FourierTransformScaling.NoScaling);
                    break;
                case FourierOptions.InverseExponent | FourierOptions.AsymmetricScaling:
                    Control.FourierTransformProvider.ForwardInplace(specrum, FourierTransformScaling.ForwardScaling);
                    break;
                default:
                    Control.FourierTransformProvider.BackwardInplace(specrum, FourierTransformScaling.SymmetricScaling);
                    break;
            }
        }

        public static void InverseMultiDim(Complex[] specrum, int[] dimensions, FourierOptions options)
        {
            switch (options)
            {
                case FourierOptions.NoScaling:
                    Control.FourierTransformProvider.BackwardInplaceMultidim(specrum, dimensions, FourierTransformScaling.NoScaling);
                    break;
                case FourierOptions.AsymmetricScaling:
                    Control.FourierTransformProvider.BackwardInplaceMultidim(specrum, dimensions, FourierTransformScaling.BackwardScaling);
                    break;
                case FourierOptions.InverseExponent:
                    Control.FourierTransformProvider.ForwardInplaceMultidim(specrum, dimensions, FourierTransformScaling.SymmetricScaling);
                    break;
                case FourierOptions.InverseExponent | FourierOptions.NoScaling:
                    Control.FourierTransformProvider.ForwardInplaceMultidim(specrum, dimensions, FourierTransformScaling.NoScaling);
                    break;
                case FourierOptions.InverseExponent | FourierOptions.AsymmetricScaling:
                    Control.FourierTransformProvider.ForwardInplaceMultidim(specrum, dimensions, FourierTransformScaling.ForwardScaling);
                    break;
                default:
                    Control.FourierTransformProvider.BackwardInplaceMultidim(specrum, dimensions, FourierTransformScaling.SymmetricScaling);
                    break;
            }
        }

        public static void Inverse2D(Complex[] spectrumRowWise, int rows, int columns, FourierOptions options)
        {
            InverseMultiDim(spectrumRowWise, new[] { rows, columns }, options);
        }

        public static void Inverse2D(Matrix<Complex> spectrum, FourierOptions options)
        {
            // since dense matrix data is column major, we switch rows and columns

            var denseStorage = spectrum.Storage as DenseColumnMajorMatrixStorage<Complex>;
            if (denseStorage == null)
            {
                var samplesColumnMajor = spectrum.ToColumnWiseArray();
                InverseMultiDim(samplesColumnMajor, new[] { spectrum.ColumnCount, spectrum.RowCount }, options);
                denseStorage = new DenseColumnMajorMatrixStorage<Complex>(spectrum.RowCount, spectrum.ColumnCount, samplesColumnMajor);
                denseStorage.CopyToUnchecked(spectrum.Storage, ExistingData.Clear);
                return;
            }

            InverseMultiDim(denseStorage.Data, new[] { spectrum.ColumnCount, spectrum.RowCount }, options);
        }

        /// <summary>
        /// Naive forward DFT, useful e.g. to verify faster algorithms.
        /// </summary>
        /// <param name="samples">Time-space sample vector.</param>
        /// <param name="options">Fourier Transform Convention Options.</param>
        /// <returns>Corresponding frequency-space vector.</returns>
        public static Complex[] NaiveForward(Complex[] samples, FourierOptions options)
        {
            var frequencySpace = Naive(samples, SignByOptions(options));
            ForwardScaleByOptions(options, frequencySpace);
            return frequencySpace;
        }

        /// <summary>
        /// Naive inverse DFT, useful e.g. to verify faster algorithms.
        /// </summary>
        /// <param name="spectrum">Frequency-space sample vector.</param>
        /// <param name="options">Fourier Transform Convention Options.</param>
        /// <returns>Corresponding time-space vector.</returns>
        public static Complex[] NaiveInverse(Complex[] spectrum, FourierOptions options)
        {
            var timeSpace = Naive(spectrum, -SignByOptions(options));
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
        /// <param name="spectrum">Sample vector, where the FFT is evaluated in place.</param>
        /// <param name="options">Fourier Transform Convention Options.</param>
        /// <exception cref="ArgumentException"/>
        public static void Radix2Inverse(Complex[] spectrum, FourierOptions options)
        {
            Radix2Parallel(spectrum, -SignByOptions(options));
            InverseScaleByOptions(options, spectrum);
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
        /// <param name="spectrum">Sample vector, where the FFT is evaluated in place.</param>
        /// <param name="options">Fourier Transform Convention Options.</param>
        public static void BluesteinInverse(Complex[] spectrum, FourierOptions options)
        {
            Bluestein(spectrum, -SignByOptions(options));
            InverseScaleByOptions(options, spectrum);
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

        /// <summary>
        /// Generate the frequencies corresponding to each index in frequency space.
        /// The frequency space has a resolution of sampleRate/N.
        /// Index 0 corresponds to the DC part, the following indices correspond to
        /// the positive frequencies up to the Nyquist frequency (sampleRate/2),
        /// followed by the negative frequencies wrapped around.
        /// </summary>
        /// <param name="length">Number of samples.</param>
        /// <param name="sampleRate">The sampling rate of the time-space data.</param>
        public static double[] FrequencyScale(int length, double sampleRate)
        {
            double[] scale = new double[length];
            double f = 0, step = sampleRate / length;
            int secondHalf = (length >> 1) + 1;
            for (int i = 0; i < secondHalf; i++)
            {
                scale[i] = f;
                f += step;
            }

            f = -step * (secondHalf - 2);
            for (int i = secondHalf; i < length; i++)
            {
                scale[i] = f;
                f += step;
            }

            return scale;
        }
    }
}
