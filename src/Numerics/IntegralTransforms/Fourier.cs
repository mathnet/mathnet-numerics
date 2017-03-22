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
using MathNet.Numerics.Properties;
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
        public static void Forward(Complex32[] samples)
        {
            Control.FourierTransformProvider.Forward(samples, FourierTransformScaling.SymmetricScaling);
        }

        /// <summary>
        /// Applies the forward Fast Fourier Transform (FFT) to arbitrary-length sample vectors.
        /// </summary>
        /// <param name="samples">Sample vector, where the FFT is evaluated in place.</param>
        public static void Forward(Complex[] samples)
        {
            Control.FourierTransformProvider.Forward(samples, FourierTransformScaling.SymmetricScaling);
        }
        
        /// <summary>
        /// Applies the forward Fast Fourier Transform (FFT) to arbitrary-length sample vectors.
        /// </summary>
        /// <param name="samples">Sample vector, where the FFT is evaluated in place.</param>
        /// <param name="options">Fourier Transform Convention Options.</param>
        public static void Forward(Complex32[] samples, FourierOptions options)
        {
            switch (options)
            {
                case FourierOptions.NoScaling:
                case FourierOptions.AsymmetricScaling:
                    Control.FourierTransformProvider.Forward(samples, FourierTransformScaling.NoScaling);
                    break;
                case FourierOptions.InverseExponent:
                    Control.FourierTransformProvider.Backward(samples, FourierTransformScaling.SymmetricScaling);
                    break;
                case FourierOptions.InverseExponent | FourierOptions.NoScaling:
                case FourierOptions.InverseExponent | FourierOptions.AsymmetricScaling:
                    Control.FourierTransformProvider.Backward(samples, FourierTransformScaling.NoScaling);
                    break;
                default:
                    Control.FourierTransformProvider.Forward(samples, FourierTransformScaling.SymmetricScaling);
                    break;
            }
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
                    Control.FourierTransformProvider.Forward(samples, FourierTransformScaling.NoScaling);
                    break;
                case FourierOptions.InverseExponent:
                    Control.FourierTransformProvider.Backward(samples, FourierTransformScaling.SymmetricScaling);
                    break;
                case FourierOptions.InverseExponent | FourierOptions.NoScaling:
                case FourierOptions.InverseExponent | FourierOptions.AsymmetricScaling:
                    Control.FourierTransformProvider.Backward(samples, FourierTransformScaling.NoScaling);
                    break;
                default:
                    Control.FourierTransformProvider.Forward(samples, FourierTransformScaling.SymmetricScaling);
                    break;
            }
        }
        
        /// <summary>
        /// Applies the forward Fast Fourier Transform (FFT) to arbitrary-length sample vectors.
        /// </summary>
        /// <param name="real">Real part of the sample vector, where the FFT is evaluated in place.</param>
        /// <param name="imaginary">Imaginary part of the sample vector, where the FFT is evaluated in place.</param>
        /// <param name="options">Fourier Transform Convention Options.</param>
        public static void Forward(float[] real, float[] imaginary, FourierOptions options = FourierOptions.Default)
        {
            if (real.Length != imaginary.Length)
            {
                throw new ArgumentException(Resources.ArgumentArraysSameLength);
            }

            // TODO: consider to support this natively by the provider, without the need for copying
            // TODO: otherwise, consider ArrayPool

            Complex32[] data = new Complex32[real.Length];
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = new Complex32(real[i], imaginary[i]);
            }

            Forward(data, options);

            for (int i = 0; i < data.Length; i++)
            {
                real[i] = data[i].Real;
                imaginary[i] = data[i].Imaginary;
            }
        }

        /// <summary>
        /// Applies the forward Fast Fourier Transform (FFT) to arbitrary-length sample vectors.
        /// </summary>
        /// <param name="real">Real part of the sample vector, where the FFT is evaluated in place.</param>
        /// <param name="imaginary">Imaginary part of the sample vector, where the FFT is evaluated in place.</param>
        /// <param name="options">Fourier Transform Convention Options.</param>
        public static void Forward(double[] real, double[] imaginary, FourierOptions options = FourierOptions.Default)
        {
            if (real.Length != imaginary.Length)
            {
                throw new ArgumentException(Resources.ArgumentArraysSameLength);
            }

            // TODO: consider to support this natively by the provider, without the need for copying
            // TODO: otherwise, consider ArrayPool

            Complex[] data = new Complex[real.Length];
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = new Complex(real[i], imaginary[i]);
            }

            Forward(data, options);

            for (int i = 0; i < data.Length; i++)
            {
                real[i] = data[i].Real;
                imaginary[i] = data[i].Imaginary;
            }
        }
        
        /// <summary>
        /// Packed Real-Complex forward Fast Fourier Transform (FFT) to arbitrary-length sample vectors.
        /// Since for real-valued time samples the complex spectrum is conjugate-even (symmetry),
        /// the spectrum can be fully reconstructed form the positive frequencies only (first half).
        /// The data array needs to be N+2 (if N is even) or N+1 (if N is odd) long in order to support such a packed spectrum.
        /// </summary>
        /// <param name="data">Data array of length N+2 (if N is even) or N+1 (if N is odd).</param>
        /// <param name="n">The number of samples.</param>
        /// <param name="options">Fourier Transform Convention Options.</param>
        public static void ForwardReal(float[] data, int n, FourierOptions options = FourierOptions.Default)
        {
            int length = n.IsEven() ? n + 2 : n + 1;
            if (data.Length < length)
            {
                throw new ArgumentException(string.Format(Resources.ArrayTooSmall, length));
            }

            if ((options & FourierOptions.InverseExponent) == FourierOptions.InverseExponent)
            {
                throw new NotSupportedException();
            }

            switch (options)
            {
                case FourierOptions.NoScaling:
                case FourierOptions.AsymmetricScaling:
                    Control.FourierTransformProvider.ForwardReal(data, n, FourierTransformScaling.NoScaling);
                    break;
                default:
                    Control.FourierTransformProvider.ForwardReal(data, n, FourierTransformScaling.SymmetricScaling);
                    break;
            }
        }

        /// <summary>
        /// Packed Real-Complex forward Fast Fourier Transform (FFT) to arbitrary-length sample vectors.
        /// Since for real-valued time samples the complex spectrum is conjugate-even (symmetry),
        /// the spectrum can be fully reconstructed form the positive frequencies only (first half).
        /// The data array needs to be N+2 (if N is even) or N+1 (if N is odd) long in order to support such a packed spectrum.
        /// </summary>
        /// <param name="data">Data array of length N+2 (if N is even) or N+1 (if N is odd).</param>
        /// <param name="n">The number of samples.</param>
        /// <param name="options">Fourier Transform Convention Options.</param>
        public static void ForwardReal(double[] data, int n, FourierOptions options = FourierOptions.Default)
        {
            int length = n.IsEven() ? n + 2 : n + 1;
            if (data.Length < length)
            {
                throw new ArgumentException(string.Format(Resources.ArrayTooSmall, length));
            }

            if ((options & FourierOptions.InverseExponent) == FourierOptions.InverseExponent)
            {
                throw new NotSupportedException();
            }

            switch (options)
            {
                case FourierOptions.NoScaling:
                case FourierOptions.AsymmetricScaling:
                    Control.FourierTransformProvider.ForwardReal(data, n, FourierTransformScaling.NoScaling);
                    break;
                default:
                    Control.FourierTransformProvider.ForwardReal(data, n, FourierTransformScaling.SymmetricScaling);
                    break;
            }
        }

        /// <summary>
        /// Applies the forward Fast Fourier Transform (FFT) to multiple dimensional sample data.
        /// </summary>
        /// <param name="samples">Sample data, where the FFT is evaluated in place.</param>
        /// <param name="dimensions">
        /// The data size per dimension. The first dimension is the major one.
        /// For example, with two dimensions "rows" and "columns" the samples are assumed to be organized row by row.
        /// </param>
        /// <param name="options">Fourier Transform Convention Options.</param>
        public static void ForwardMultiDim(Complex32[] samples, int[] dimensions, FourierOptions options = FourierOptions.Default)
        {
            switch (options)
            {
                case FourierOptions.NoScaling:
                case FourierOptions.AsymmetricScaling:
                    Control.FourierTransformProvider.ForwardMultidim(samples, dimensions, FourierTransformScaling.NoScaling);
                    break;
                case FourierOptions.InverseExponent:
                    Control.FourierTransformProvider.BackwardMultidim(samples, dimensions, FourierTransformScaling.SymmetricScaling);
                    break;
                case FourierOptions.InverseExponent | FourierOptions.NoScaling:
                case FourierOptions.InverseExponent | FourierOptions.AsymmetricScaling:
                    Control.FourierTransformProvider.BackwardMultidim(samples, dimensions, FourierTransformScaling.NoScaling);
                    break;
                default:
                    Control.FourierTransformProvider.ForwardMultidim(samples, dimensions, FourierTransformScaling.SymmetricScaling);
                    break;
            }
        }

        /// <summary>
        /// Applies the forward Fast Fourier Transform (FFT) to multiple dimensional sample data.
        /// </summary>
        /// <param name="samples">Sample data, where the FFT is evaluated in place.</param>
        /// <param name="dimensions">
        /// The data size per dimension. The first dimension is the major one.
        /// For example, with two dimensions "rows" and "columns" the samples are assumed to be organized row by row.
        /// </param>
        /// <param name="options">Fourier Transform Convention Options.</param>
        public static void ForwardMultiDim(Complex[] samples, int[] dimensions, FourierOptions options = FourierOptions.Default)
        {
            switch (options)
            {
                case FourierOptions.NoScaling:
                case FourierOptions.AsymmetricScaling:
                    Control.FourierTransformProvider.ForwardMultidim(samples, dimensions, FourierTransformScaling.NoScaling);
                    break;
                case FourierOptions.InverseExponent:
                    Control.FourierTransformProvider.BackwardMultidim(samples, dimensions, FourierTransformScaling.SymmetricScaling);
                    break;
                case FourierOptions.InverseExponent | FourierOptions.NoScaling:
                case FourierOptions.InverseExponent | FourierOptions.AsymmetricScaling:
                    Control.FourierTransformProvider.BackwardMultidim(samples, dimensions, FourierTransformScaling.NoScaling);
                    break;
                default:
                    Control.FourierTransformProvider.ForwardMultidim(samples, dimensions, FourierTransformScaling.SymmetricScaling);
                    break;
            }
        }
        
        /// <summary>
        /// Applies the forward Fast Fourier Transform (FFT) to two dimensional sample data.
        /// </summary>
        /// <param name="samplesRowWise">Sample data, organized row by row, where the FFT is evaluated in place</param>
        /// <param name="rows">The number of rows.</param>
        /// <param name="columns">The number of columns.</param>
        /// <remarks>Data available organized column by column instead of row by row can be processed directly by swapping the rows and columns arguments.</remarks>
        /// <param name="options">Fourier Transform Convention Options.</param>
        public static void Forward2D(Complex32[] samplesRowWise, int rows, int columns, FourierOptions options = FourierOptions.Default)
        {
            ForwardMultiDim(samplesRowWise, new[] { rows, columns }, options);
        }

        /// <summary>
        /// Applies the forward Fast Fourier Transform (FFT) to two dimensional sample data.
        /// </summary>
        /// <param name="samplesRowWise">Sample data, organized row by row, where the FFT is evaluated in place</param>
        /// <param name="rows">The number of rows.</param>
        /// <param name="columns">The number of columns.</param>
        /// <remarks>Data available organized column by column instead of row by row can be processed directly by swapping the rows and columns arguments.</remarks>
        /// <param name="options">Fourier Transform Convention Options.</param>
        public static void Forward2D(Complex[] samplesRowWise, int rows, int columns, FourierOptions options = FourierOptions.Default)
        {
            ForwardMultiDim(samplesRowWise, new[] { rows, columns }, options);
        }
        
        /// <summary>
        /// Applies the forward Fast Fourier Transform (FFT) to a two dimensional data in form of a matrix.
        /// </summary>
        /// <param name="samples">Sample matrix, where the FFT is evaluated in place</param>
        /// <param name="options">Fourier Transform Convention Options.</param>
        public static void Forward2D(Matrix<Complex32> samples, FourierOptions options = FourierOptions.Default)
        {
            var rowMajorArray = samples.AsRowMajorArray();
            if (rowMajorArray != null)
            {
                ForwardMultiDim(rowMajorArray, new[] { samples.RowCount, samples.ColumnCount }, options);
                return;
            }

            var columnMajorArray = samples.AsColumnMajorArray();
            if (columnMajorArray != null)
            {
                ForwardMultiDim(columnMajorArray, new[] { samples.ColumnCount, samples.RowCount }, options);
                return;
            }

            // Fall Back
            columnMajorArray = samples.ToColumnMajorArray();
            ForwardMultiDim(columnMajorArray, new[] { samples.ColumnCount, samples.RowCount }, options);
            var denseStorage = new DenseColumnMajorMatrixStorage<Complex32>(samples.RowCount, samples.ColumnCount, columnMajorArray);
            denseStorage.CopyToUnchecked(samples.Storage, ExistingData.Clear);
        }

        /// <summary>
        /// Applies the forward Fast Fourier Transform (FFT) to a two dimensional data in form of a matrix.
        /// </summary>
        /// <param name="samples">Sample matrix, where the FFT is evaluated in place</param>
        /// <param name="options">Fourier Transform Convention Options.</param>
        public static void Forward2D(Matrix<Complex> samples, FourierOptions options = FourierOptions.Default)
        {
            var rowMajorArray = samples.AsRowMajorArray();
            if (rowMajorArray != null)
            {
                ForwardMultiDim(rowMajorArray, new[] { samples.RowCount, samples.ColumnCount }, options);
                return;
            }

            var columnMajorArray = samples.AsColumnMajorArray();
            if (columnMajorArray != null)
            {
                ForwardMultiDim(columnMajorArray, new[] { samples.ColumnCount, samples.RowCount }, options);
                return;
            }

            // Fall Back
            columnMajorArray = samples.ToColumnMajorArray();
            ForwardMultiDim(columnMajorArray, new[] { samples.ColumnCount, samples.RowCount }, options);
            var denseStorage = new DenseColumnMajorMatrixStorage<Complex>(samples.RowCount, samples.ColumnCount, columnMajorArray);
            denseStorage.CopyToUnchecked(samples.Storage, ExistingData.Clear);
        }

        /// <summary>
        /// Applies the inverse Fast Fourier Transform (iFFT) to arbitrary-length sample vectors.
        /// </summary>
        /// <param name="spectrum">Spectrum data, where the iFFT is evaluated in place.</param>
        public static void Inverse(Complex32[] spectrum)
        {
            Control.FourierTransformProvider.Backward(spectrum, FourierTransformScaling.SymmetricScaling);
        }

        /// <summary>
        /// Applies the inverse Fast Fourier Transform (iFFT) to arbitrary-length sample vectors.
        /// </summary>
        /// <param name="spectrum">Spectrum data, where the iFFT is evaluated in place.</param>
        public static void Inverse(Complex[] spectrum)
        {
            Control.FourierTransformProvider.Backward(spectrum, FourierTransformScaling.SymmetricScaling);
        }

        /// <summary>
        /// Applies the inverse Fast Fourier Transform (iFFT) to arbitrary-length sample vectors.
        /// </summary>
        /// <param name="spectrum">Spectrum data, where the iFFT is evaluated in place.</param>
        /// <param name="options">Fourier Transform Convention Options.</param>
        public static void Inverse(Complex32[] spectrum, FourierOptions options)
        {
            switch (options)
            {
                case FourierOptions.NoScaling:
                    Control.FourierTransformProvider.Backward(spectrum, FourierTransformScaling.NoScaling);
                    break;
                case FourierOptions.AsymmetricScaling:
                    Control.FourierTransformProvider.Backward(spectrum, FourierTransformScaling.BackwardScaling);
                    break;
                case FourierOptions.InverseExponent:
                    Control.FourierTransformProvider.Forward(spectrum, FourierTransformScaling.SymmetricScaling);
                    break;
                case FourierOptions.InverseExponent | FourierOptions.NoScaling:
                    Control.FourierTransformProvider.Forward(spectrum, FourierTransformScaling.NoScaling);
                    break;
                case FourierOptions.InverseExponent | FourierOptions.AsymmetricScaling:
                    Control.FourierTransformProvider.Forward(spectrum, FourierTransformScaling.ForwardScaling);
                    break;
                default:
                    Control.FourierTransformProvider.Backward(spectrum, FourierTransformScaling.SymmetricScaling);
                    break;
            }
        }

        /// <summary>
        /// Applies the inverse Fast Fourier Transform (iFFT) to arbitrary-length sample vectors.
        /// </summary>
        /// <param name="spectrum">Spectrum data, where the iFFT is evaluated in place.</param>
        /// <param name="options">Fourier Transform Convention Options.</param>
        public static void Inverse(Complex[] spectrum, FourierOptions options)
        {
            switch (options)
            {
                case FourierOptions.NoScaling:
                    Control.FourierTransformProvider.Backward(spectrum, FourierTransformScaling.NoScaling);
                    break;
                case FourierOptions.AsymmetricScaling:
                    Control.FourierTransformProvider.Backward(spectrum, FourierTransformScaling.BackwardScaling);
                    break;
                case FourierOptions.InverseExponent:
                    Control.FourierTransformProvider.Forward(spectrum, FourierTransformScaling.SymmetricScaling);
                    break;
                case FourierOptions.InverseExponent | FourierOptions.NoScaling:
                    Control.FourierTransformProvider.Forward(spectrum, FourierTransformScaling.NoScaling);
                    break;
                case FourierOptions.InverseExponent | FourierOptions.AsymmetricScaling:
                    Control.FourierTransformProvider.Forward(spectrum, FourierTransformScaling.ForwardScaling);
                    break;
                default:
                    Control.FourierTransformProvider.Backward(spectrum, FourierTransformScaling.SymmetricScaling);
                    break;
            }
        }

        /// <summary>
        /// Applies the inverse Fast Fourier Transform (iFFT) to arbitrary-length sample vectors.
        /// </summary>
        /// <param name="real">Real part of the sample vector, where the iFFT is evaluated in place.</param>
        /// <param name="imaginary">Imaginary part of the sample vector, where the iFFT is evaluated in place.</param>
        /// <param name="options">Fourier Transform Convention Options.</param>
        public static void Inverse(float[] real, float[] imaginary, FourierOptions options = FourierOptions.Default)
        {
            if (real.Length != imaginary.Length)
            {
                throw new ArgumentException(Resources.ArgumentArraysSameLength);
            }

            // TODO: consider to support this natively by the provider, without the need for copying
            // TODO: otherwise, consider ArrayPool

            Complex32[] data = new Complex32[real.Length];
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = new Complex32(real[i], imaginary[i]);
            }

            Inverse(data, options);

            for (int i = 0; i < data.Length; i++)
            {
                real[i] = data[i].Real;
                imaginary[i] = data[i].Imaginary;
            }
        }
        
        /// <summary>
        /// Applies the inverse Fast Fourier Transform (iFFT) to arbitrary-length sample vectors.
        /// </summary>
        /// <param name="real">Real part of the sample vector, where the iFFT is evaluated in place.</param>
        /// <param name="imaginary">Imaginary part of the sample vector, where the iFFT is evaluated in place.</param>
        /// <param name="options">Fourier Transform Convention Options.</param>
        public static void Inverse(double[] real, double[] imaginary, FourierOptions options = FourierOptions.Default)
        {
            if (real.Length != imaginary.Length)
            {
                throw new ArgumentException(Resources.ArgumentArraysSameLength);
            }

            // TODO: consider to support this natively by the provider, without the need for copying
            // TODO: otherwise, consider ArrayPool

            Complex[] data = new Complex[real.Length];
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = new Complex(real[i], imaginary[i]);
            }

            Inverse(data, options);

            for (int i = 0; i < data.Length; i++)
            {
                real[i] = data[i].Real;
                imaginary[i] = data[i].Imaginary;
            }
        }

        /// <summary>
        /// Packed Real-Complex inverse Fast Fourier Transform (iFFT) to arbitrary-length sample vectors.
        /// Since for real-valued time samples the complex spectrum is conjugate-even (symmetry),
        /// the spectrum can be fully reconstructed form the positive frequencies only (first half).
        /// The data array needs to be N+2 (if N is even) or N+1 (if N is odd) long in order to support such a packed spectrum.
        /// </summary>
        /// <param name="data">Data array of length N+2 (if N is even) or N+1 (if N is odd).</param>
        /// <param name="n">The number of samples.</param>
        /// <param name="options">Fourier Transform Convention Options.</param>
        public static void InverseReal(float[] data, int n, FourierOptions options = FourierOptions.Default)
        {
            int length = n.IsEven() ? n + 2 : n + 1;
            if (data.Length < length)
            {
                throw new ArgumentException(string.Format(Resources.ArrayTooSmall, length));
            }

            if ((options & FourierOptions.InverseExponent) == FourierOptions.InverseExponent)
            {
                throw new NotSupportedException();
            }

            switch (options)
            {
                case FourierOptions.NoScaling:
                    Control.FourierTransformProvider.BackwardReal(data, n, FourierTransformScaling.NoScaling);
                    break;
                case FourierOptions.AsymmetricScaling:
                    Control.FourierTransformProvider.BackwardReal(data, n, FourierTransformScaling.BackwardScaling);
                    break;
                default:
                    Control.FourierTransformProvider.BackwardReal(data, n, FourierTransformScaling.SymmetricScaling);
                    break;
            }
        }

        /// <summary>
        /// Packed Real-Complex inverse Fast Fourier Transform (iFFT) to arbitrary-length sample vectors.
        /// Since for real-valued time samples the complex spectrum is conjugate-even (symmetry),
        /// the spectrum can be fully reconstructed form the positive frequencies only (first half).
        /// The data array needs to be N+2 (if N is even) or N+1 (if N is odd) long in order to support such a packed spectrum.
        /// </summary>
        /// <param name="data">Data array of length N+2 (if N is even) or N+1 (if N is odd).</param>
        /// <param name="n">The number of samples.</param>
        /// <param name="options">Fourier Transform Convention Options.</param>
        public static void InverseReal(double[] data, int n, FourierOptions options = FourierOptions.Default)
        {
            int length = n.IsEven() ? n + 2 : n + 1;
            if (data.Length < length)
            {
                throw new ArgumentException(string.Format(Resources.ArrayTooSmall, length));
            }

            if ((options & FourierOptions.InverseExponent) == FourierOptions.InverseExponent)
            {
                throw new NotSupportedException();
            }

            switch (options)
            {
                case FourierOptions.NoScaling:
                    Control.FourierTransformProvider.BackwardReal(data, n, FourierTransformScaling.NoScaling);
                    break;
                case FourierOptions.AsymmetricScaling:
                    Control.FourierTransformProvider.BackwardReal(data, n, FourierTransformScaling.BackwardScaling);
                    break;
                default:
                    Control.FourierTransformProvider.BackwardReal(data, n, FourierTransformScaling.SymmetricScaling);
                    break;
            }
        }

        /// <summary>
        /// Applies the inverse Fast Fourier Transform (iFFT) to multiple dimensional sample data.
        /// </summary>
        /// <param name="spectrum">Spectrum data, where the iFFT is evaluated in place.</param>
        /// <param name="dimensions">
        /// The data size per dimension. The first dimension is the major one.
        /// For example, with two dimensions "rows" and "columns" the samples are assumed to be organized row by row.
        /// </param>
        /// <param name="options">Fourier Transform Convention Options.</param>
        public static void InverseMultiDim(Complex32[] spectrum, int[] dimensions, FourierOptions options = FourierOptions.Default)
        {
            switch (options)
            {
                case FourierOptions.NoScaling:
                    Control.FourierTransformProvider.BackwardMultidim(spectrum, dimensions, FourierTransformScaling.NoScaling);
                    break;
                case FourierOptions.AsymmetricScaling:
                    Control.FourierTransformProvider.BackwardMultidim(spectrum, dimensions, FourierTransformScaling.BackwardScaling);
                    break;
                case FourierOptions.InverseExponent:
                    Control.FourierTransformProvider.ForwardMultidim(spectrum, dimensions, FourierTransformScaling.SymmetricScaling);
                    break;
                case FourierOptions.InverseExponent | FourierOptions.NoScaling:
                    Control.FourierTransformProvider.ForwardMultidim(spectrum, dimensions, FourierTransformScaling.NoScaling);
                    break;
                case FourierOptions.InverseExponent | FourierOptions.AsymmetricScaling:
                    Control.FourierTransformProvider.ForwardMultidim(spectrum, dimensions, FourierTransformScaling.ForwardScaling);
                    break;
                default:
                    Control.FourierTransformProvider.BackwardMultidim(spectrum, dimensions, FourierTransformScaling.SymmetricScaling);
                    break;
            }
        }

        /// <summary>
        /// Applies the inverse Fast Fourier Transform (iFFT) to multiple dimensional sample data.
        /// </summary>
        /// <param name="spectrum">Spectrum data, where the iFFT is evaluated in place.</param>
        /// <param name="dimensions">
        /// The data size per dimension. The first dimension is the major one.
        /// For example, with two dimensions "rows" and "columns" the samples are assumed to be organized row by row.
        /// </param>
        /// <param name="options">Fourier Transform Convention Options.</param>
        public static void InverseMultiDim(Complex[] spectrum, int[] dimensions, FourierOptions options = FourierOptions.Default)
        {
            switch (options)
            {
                case FourierOptions.NoScaling:
                    Control.FourierTransformProvider.BackwardMultidim(spectrum, dimensions, FourierTransformScaling.NoScaling);
                    break;
                case FourierOptions.AsymmetricScaling:
                    Control.FourierTransformProvider.BackwardMultidim(spectrum, dimensions, FourierTransformScaling.BackwardScaling);
                    break;
                case FourierOptions.InverseExponent:
                    Control.FourierTransformProvider.ForwardMultidim(spectrum, dimensions, FourierTransformScaling.SymmetricScaling);
                    break;
                case FourierOptions.InverseExponent | FourierOptions.NoScaling:
                    Control.FourierTransformProvider.ForwardMultidim(spectrum, dimensions, FourierTransformScaling.NoScaling);
                    break;
                case FourierOptions.InverseExponent | FourierOptions.AsymmetricScaling:
                    Control.FourierTransformProvider.ForwardMultidim(spectrum, dimensions, FourierTransformScaling.ForwardScaling);
                    break;
                default:
                    Control.FourierTransformProvider.BackwardMultidim(spectrum, dimensions, FourierTransformScaling.SymmetricScaling);
                    break;
            }
        }

        /// <summary>
        /// Applies the inverse Fast Fourier Transform (iFFT) to two dimensional sample data.
        /// </summary>
        /// <param name="spectrumRowWise">Sample data, organized row by row, where the iFFT is evaluated in place</param>
        /// <param name="rows">The number of rows.</param>
        /// <param name="columns">The number of columns.</param>
        /// <remarks>Data available organized column by column instead of row by row can be processed directly by swapping the rows and columns arguments.</remarks>
        /// <param name="options">Fourier Transform Convention Options.</param>
        public static void Inverse2D(Complex32[] spectrumRowWise, int rows, int columns, FourierOptions options = FourierOptions.Default)
        {
            InverseMultiDim(spectrumRowWise, new[] { rows, columns }, options);
        }

        /// <summary>
        /// Applies the inverse Fast Fourier Transform (iFFT) to two dimensional sample data.
        /// </summary>
        /// <param name="spectrumRowWise">Sample data, organized row by row, where the iFFT is evaluated in place</param>
        /// <param name="rows">The number of rows.</param>
        /// <param name="columns">The number of columns.</param>
        /// <remarks>Data available organized column by column instead of row by row can be processed directly by swapping the rows and columns arguments.</remarks>
        /// <param name="options">Fourier Transform Convention Options.</param>
        public static void Inverse2D(Complex[] spectrumRowWise, int rows, int columns, FourierOptions options = FourierOptions.Default)
        {
            InverseMultiDim(spectrumRowWise, new[] { rows, columns }, options);
        }

        /// <summary>
        /// Applies the inverse Fast Fourier Transform (iFFT) to a two dimensional data in form of a matrix.
        /// </summary>
        /// <param name="spectrum">Sample matrix, where the iFFT is evaluated in place</param>
        /// <param name="options">Fourier Transform Convention Options.</param>
        public static void Inverse2D(Matrix<Complex32> spectrum, FourierOptions options = FourierOptions.Default)
        {
            var rowMajorArray = spectrum.AsRowMajorArray();
            if (rowMajorArray != null)
            {
                InverseMultiDim(rowMajorArray, new[] { spectrum.RowCount, spectrum.ColumnCount }, options);
                return;
            }

            var columnMajorArray = spectrum.AsColumnMajorArray();
            if (columnMajorArray != null)
            {
                InverseMultiDim(columnMajorArray, new[] { spectrum.ColumnCount, spectrum.RowCount }, options);
                return;
            }

            // Fall Back
            columnMajorArray = spectrum.ToColumnMajorArray();
            InverseMultiDim(columnMajorArray, new[] { spectrum.ColumnCount, spectrum.RowCount }, options);
            var denseStorage = new DenseColumnMajorMatrixStorage<Complex32>(spectrum.RowCount, spectrum.ColumnCount, columnMajorArray);
            denseStorage.CopyToUnchecked(spectrum.Storage, ExistingData.Clear);
        }

        /// <summary>
        /// Applies the inverse Fast Fourier Transform (iFFT) to a two dimensional data in form of a matrix.
        /// </summary>
        /// <param name="spectrum">Sample matrix, where the iFFT is evaluated in place</param>
        /// <param name="options">Fourier Transform Convention Options.</param>
        public static void Inverse2D(Matrix<Complex> spectrum, FourierOptions options = FourierOptions.Default)
        {
            var rowMajorArray = spectrum.AsRowMajorArray();
            if (rowMajorArray != null)
            {
                InverseMultiDim(rowMajorArray, new[] { spectrum.RowCount, spectrum.ColumnCount }, options);
                return;
            }

            var columnMajorArray = spectrum.AsColumnMajorArray();
            if (columnMajorArray != null)
            {
                InverseMultiDim(columnMajorArray, new[] { spectrum.ColumnCount, spectrum.RowCount }, options);
                return;
            }

            // Fall Back
            columnMajorArray = spectrum.ToColumnMajorArray();
            InverseMultiDim(columnMajorArray, new[] { spectrum.ColumnCount, spectrum.RowCount }, options);
            var denseStorage = new DenseColumnMajorMatrixStorage<Complex>(spectrum.RowCount, spectrum.ColumnCount, columnMajorArray);
            denseStorage.CopyToUnchecked(spectrum.Storage, ExistingData.Clear);
        }

        /// <summary>
        /// Naive forward DFT, useful e.g. to verify faster algorithms.
        /// </summary>
        /// <param name="samples">Time-space sample vector.</param>
        /// <param name="options">Fourier Transform Convention Options.</param>
        /// <returns>Corresponding frequency-space vector.</returns>
        public static Complex32[] NaiveForward(Complex32[] samples, FourierOptions options = FourierOptions.Default)
        {
            var frequencySpace = Naive(samples, SignByOptions(options));
            ForwardScaleByOptions(options, frequencySpace);
            return frequencySpace;
        }

        /// <summary>
        /// Naive forward DFT, useful e.g. to verify faster algorithms.
        /// </summary>
        /// <param name="samples">Time-space sample vector.</param>
        /// <param name="options">Fourier Transform Convention Options.</param>
        /// <returns>Corresponding frequency-space vector.</returns>
        public static Complex[] NaiveForward(Complex[] samples, FourierOptions options = FourierOptions.Default)
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
        public static Complex32[] NaiveInverse(Complex32[] spectrum, FourierOptions options = FourierOptions.Default)
        {
            var timeSpace = Naive(spectrum, -SignByOptions(options));
            InverseScaleByOptions(options, timeSpace);
            return timeSpace;
        }

        /// <summary>
        /// Naive inverse DFT, useful e.g. to verify faster algorithms.
        /// </summary>
        /// <param name="spectrum">Frequency-space sample vector.</param>
        /// <param name="options">Fourier Transform Convention Options.</param>
        /// <returns>Corresponding time-space vector.</returns>
        public static Complex[] NaiveInverse(Complex[] spectrum, FourierOptions options = FourierOptions.Default)
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
        public static void Radix2Forward(Complex32[] samples, FourierOptions options = FourierOptions.Default)
        {
            Radix2Parallel(samples, SignByOptions(options));
            ForwardScaleByOptions(options, samples);
        }

        /// <summary>
        /// Radix-2 forward FFT for power-of-two sized sample vectors.
        /// </summary>
        /// <param name="samples">Sample vector, where the FFT is evaluated in place.</param>
        /// <param name="options">Fourier Transform Convention Options.</param>
        /// <exception cref="ArgumentException"/>
        public static void Radix2Forward(Complex[] samples, FourierOptions options = FourierOptions.Default)
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
        public static void Radix2Inverse(Complex32[] spectrum, FourierOptions options = FourierOptions.Default)
        {
            Radix2Parallel(spectrum, -SignByOptions(options));
            InverseScaleByOptions(options, spectrum);
        }

        /// <summary>
        /// Radix-2 inverse FFT for power-of-two sized sample vectors.
        /// </summary>
        /// <param name="spectrum">Sample vector, where the FFT is evaluated in place.</param>
        /// <param name="options">Fourier Transform Convention Options.</param>
        /// <exception cref="ArgumentException"/>
        public static void Radix2Inverse(Complex[] spectrum, FourierOptions options = FourierOptions.Default)
        {
            Radix2Parallel(spectrum, -SignByOptions(options));
            InverseScaleByOptions(options, spectrum);
        }

        /// <summary>
        /// Bluestein forward FFT for arbitrary sized sample vectors.
        /// </summary>
        /// <param name="samples">Sample vector, where the FFT is evaluated in place.</param>
        /// <param name="options">Fourier Transform Convention Options.</param>
        public static void BluesteinForward(Complex32[] samples, FourierOptions options = FourierOptions.Default)
        {
            Bluestein(samples, SignByOptions(options));
            ForwardScaleByOptions(options, samples);
        }

        /// <summary>
        /// Bluestein forward FFT for arbitrary sized sample vectors.
        /// </summary>
        /// <param name="samples">Sample vector, where the FFT is evaluated in place.</param>
        /// <param name="options">Fourier Transform Convention Options.</param>
        public static void BluesteinForward(Complex[] samples, FourierOptions options = FourierOptions.Default)
        {
            Bluestein(samples, SignByOptions(options));
            ForwardScaleByOptions(options, samples);
        }

        /// <summary>
        /// Bluestein inverse FFT for arbitrary sized sample vectors.
        /// </summary>
        /// <param name="spectrum">Sample vector, where the FFT is evaluated in place.</param>
        /// <param name="options">Fourier Transform Convention Options.</param>
        public static void BluesteinInverse(Complex32[] spectrum, FourierOptions options = FourierOptions.Default)
        {
            Bluestein(spectrum, -SignByOptions(options));
            InverseScaleByOptions(options, spectrum);
        }

        /// <summary>
        /// Bluestein inverse FFT for arbitrary sized sample vectors.
        /// </summary>
        /// <param name="spectrum">Sample vector, where the FFT is evaluated in place.</param>
        /// <param name="options">Fourier Transform Convention Options.</param>
        public static void BluesteinInverse(Complex[] spectrum, FourierOptions options = FourierOptions.Default)
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
        static void ForwardScaleByOptions(FourierOptions options, Complex32[] samples)
        {
            if ((options & FourierOptions.NoScaling) == FourierOptions.NoScaling ||
                (options & FourierOptions.AsymmetricScaling) == FourierOptions.AsymmetricScaling)
            {
                return;
            }

            var scalingFactor = (float)Math.Sqrt(1.0 / samples.Length);
            for (int i = 0; i < samples.Length; i++)
            {
                samples[i] *= scalingFactor;
            }
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
        static void InverseScaleByOptions(FourierOptions options, Complex32[] samples)
        {
            if ((options & FourierOptions.NoScaling) == FourierOptions.NoScaling)
            {
                return;
            }

            var scalingFactor = (float)1.0 / samples.Length;
            if ((options & FourierOptions.AsymmetricScaling) != FourierOptions.AsymmetricScaling)
            {
                scalingFactor = (float)Math.Sqrt(scalingFactor);
            }

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
