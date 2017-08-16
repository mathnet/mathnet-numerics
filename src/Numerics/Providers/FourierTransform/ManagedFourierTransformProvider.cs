// <copyright file="ManagedFourierTransformProvider.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// https://numerics.mathdotnet.com
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
using MathNet.Numerics.IntegralTransforms;

namespace MathNet.Numerics.Providers.FourierTransform
{
#if !NOSYSNUMERICS
    using Complex = System.Numerics.Complex;
#endif

    public class ManagedFourierTransformProvider : IFourierTransformProvider
    {
        /// <summary>
        /// Try to find out whether the provider is available, at least in principle.
        /// Verification may still fail if available, but it will certainly fail if unavailable.
        /// </summary>
        public bool IsAvailable()
        {
            return true;
        }

        /// <summary>
        /// Initialize and verify that the provided is indeed available. If not, fall back to alternatives like the managed provider
        /// </summary>
        public void InitializeVerify()
        {
        }

        public override string ToString()
        {
            return "Managed";
        }

        public void Forward(Complex32[] samples, FourierTransformScaling scaling)
        {
            switch (scaling)
            {
                case FourierTransformScaling.SymmetricScaling:
                    Fourier.BluesteinForward(samples, FourierOptions.Default);
                    break;
                case FourierTransformScaling.ForwardScaling:
                    // Only backward scaling can be expressed with options, hence the double-inverse
                    Fourier.BluesteinInverse(samples, FourierOptions.AsymmetricScaling | FourierOptions.InverseExponent);
                    break;
                default:
                    Fourier.BluesteinForward(samples, FourierOptions.NoScaling);
                    break;
            }
        }

        public void Forward(Complex[] samples, FourierTransformScaling scaling)
        {
            switch (scaling)
            {
                case FourierTransformScaling.SymmetricScaling:
                    Fourier.BluesteinForward(samples, FourierOptions.Default);
                    break;
                case FourierTransformScaling.ForwardScaling:
                    // Only backward scaling can be expressed with options, hence the double-inverse
                    Fourier.BluesteinInverse(samples, FourierOptions.AsymmetricScaling | FourierOptions.InverseExponent);
                    break;
                default:
                    Fourier.BluesteinForward(samples, FourierOptions.NoScaling);
                    break;
            }
        }

        public void Backward(Complex32[] spectrum, FourierTransformScaling scaling)
        {
            switch (scaling)
            {
                case FourierTransformScaling.SymmetricScaling:
                    Fourier.BluesteinInverse(spectrum, FourierOptions.Default);
                    break;
                case FourierTransformScaling.BackwardScaling:
                    Fourier.BluesteinInverse(spectrum, FourierOptions.AsymmetricScaling);
                    break;
                default:
                    Fourier.BluesteinInverse(spectrum, FourierOptions.NoScaling);
                    break;
            }
        }

        public void Backward(Complex[] spectrum, FourierTransformScaling scaling)
        {
            switch (scaling)
            {
                case FourierTransformScaling.SymmetricScaling:
                    Fourier.BluesteinInverse(spectrum, FourierOptions.Default);
                    break;
                case FourierTransformScaling.BackwardScaling:
                    Fourier.BluesteinInverse(spectrum, FourierOptions.AsymmetricScaling);
                    break;
                default:
                    Fourier.BluesteinInverse(spectrum, FourierOptions.NoScaling);
                    break;
            }
        }

        public void ForwardReal(float[] samples, int n, FourierTransformScaling scaling)
        {
            // TODO: backport proper, optimized implementation from Iridium

            Complex32[] data = new Complex32[n];
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = new Complex32(samples[i], 0.0f);
            }

            Forward(data, scaling);

            samples[0] = data[0].Real;
            samples[1] = 0f;
            for (int i = 1, j = 2; i < data.Length / 2; i++)
            {
                samples[j++] = data[i].Real;
                samples[j++] = data[i].Imaginary;
            }
            if (n.IsEven())
            {
                samples[n] = data[data.Length / 2].Real;
                samples[n + 1] = 0f;
            }
            else
            {
                samples[n - 1] = data[data.Length / 2].Real;
                samples[n] = data[data.Length / 2].Imaginary;
            }
        }

        public void ForwardReal(double[] samples, int n, FourierTransformScaling scaling)
        {
            // TODO: backport proper, optimized implementation from Iridium

            Complex[] data = new Complex[n];
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = new Complex(samples[i], 0.0);
            }

            Forward(data, scaling);

            samples[0] = data[0].Real;
            samples[1] = 0d;
            for (int i = 1, j = 2; i < data.Length/2; i++)
            {
                samples[j++] = data[i].Real;
                samples[j++] = data[i].Imaginary;
            }
            if (n.IsEven())
            {
                samples[n] = data[data.Length/2].Real;
                samples[n+1] = 0d;
            }
            else
            {
                samples[n-1] = data[data.Length / 2].Real;
                samples[n] = data[data.Length / 2].Imaginary;
            }
        }

        public void BackwardReal(float[] spectrum, int n, FourierTransformScaling scaling)
        {
            // TODO: backport proper, optimized implementation from Iridium

            Complex32[] data = new Complex32[n];
            data[0] = new Complex32(spectrum[0], 0f);
            for (int i = 1, j = 2; i < data.Length / 2; i++)
            {
                data[i] = new Complex32(spectrum[j++], spectrum[j++]);
                data[data.Length - i] = data[i].Conjugate();
            }
            if (n.IsEven())
            {
                data[data.Length / 2] = new Complex32(spectrum[n], 0f);
            }
            else
            {
                data[data.Length / 2] = new Complex32(spectrum[n - 1], spectrum[n]);
                data[data.Length / 2 + 1] = data[data.Length / 2].Conjugate();
            }

            Backward(data, scaling);

            for (int i = 0; i < data.Length; i++)
            {
                spectrum[i] = data[i].Real;
            }
            spectrum[n] = 0f;
        }

        public void BackwardReal(double[] spectrum, int n, FourierTransformScaling scaling)
        {
            // TODO: backport proper, optimized implementation from Iridium

            Complex[] data = new Complex[n];
            data[0] = new Complex(spectrum[0], 0d);
            for (int i = 1, j = 2; i < data.Length/2; i++)
            {
                data[i] = new Complex(spectrum[j++], spectrum[j++]);
                data[data.Length - i] = data[i].Conjugate();
            }
            if (n.IsEven())
            {
                data[data.Length/2] = new Complex(spectrum[n], 0d);
            }
            else
            {
                data[data.Length/2] = new Complex(spectrum[n-1], spectrum[n]);
                data[data.Length/2 + 1] = data[data.Length/2].Conjugate();
            }

            Backward(data, scaling);

            for (int i = 0; i < data.Length; i++)
            {
                spectrum[i] = data[i].Real;
            }
            spectrum[n] = 0d;
        }

        public void ForwardMultidim(Complex32[] samples, int[] dimensions, FourierTransformScaling scaling)
        {
            throw new NotSupportedException();
        }

        public void ForwardMultidim(Complex[] samples, int[] dimensions, FourierTransformScaling scaling)
        {
            throw new NotSupportedException();
        }

        public void BackwardMultidim(Complex32[] spectrum, int[] dimensions, FourierTransformScaling scaling)
        {
            throw new NotSupportedException();
        }

        public void BackwardMultidim(Complex[] spectrum, int[] dimensions, FourierTransformScaling scaling)
        {
            throw new NotSupportedException();
        }
    }
}
