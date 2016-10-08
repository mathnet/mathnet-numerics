// <copyright file="MklFourierTransformProvider.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://mathnet.opensourcedotnet.info
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

#if NATIVE

using System;
using System.Numerics;
using MathNet.Numerics.Providers.Common.Mkl;

namespace MathNet.Numerics.Providers.FourierTransform.Mkl
{
    public class MklFourierTransformProvider : IFourierTransformProvider
    {
        /// <summary>
        /// Try to find out whether the provider is available, at least in principle.
        /// Verification may still fail if available, but it will certainly fail if unavailable.
        /// </summary>
        public bool IsAvailable()
        {
            return MklProvider.IsAvailable(minRevision: 11);
        }

        /// <summary>
        /// Initialize and verify that the provided is indeed available. If not, fall back to alternatives like the managed provider
        /// </summary>
        public void InitializeVerify()
        {
            MklProvider.Load(minRevision: 11);

            // we only support exactly one major version, since major version changes imply a breaking change.
            int fftMajor = SafeNativeMethods.query_capability((int)ProviderCapability.FourierTransformMajor);
            int fftMinor = SafeNativeMethods.query_capability((int)ProviderCapability.FourierTransformMinor);
            if (!(fftMajor == 1 && fftMinor >= 0))
            {
                throw new NotSupportedException(string.Format("MKL Native Provider not compatible. Expecting fourier transform v1 but provider implements v{0}.", fftMajor));
            }
        }

        /// <summary>
        /// Frees the memory allocated to the MKL memory pool.
        /// </summary>
        public void FreeBuffers()
        {
            MklProvider.FreeBuffers();
        }

        /// <summary>
        /// Frees the memory allocated to the MKL memory pool on the current thread.
        /// </summary>
        public void ThreadFreeBuffers()
        {
            MklProvider.ThreadFreeBuffers();
        }

        /// <summary>
        /// Disable the MKL memory pool. May impact performance.
        /// </summary>
        public void DisableMemoryPool()
        {
            MklProvider.DisableMemoryPool();
        }

        /// <summary>
        /// Retrieves information about the MKL memory pool.
        /// </summary>
        /// <param name="allocatedBuffers">On output, returns the number of memory buffers allocated.</param>
        /// <returns>Returns the number of bytes allocated to all memory buffers.</returns>
        public long MemoryStatistics(out int allocatedBuffers)
        {
            return MklProvider.MemoryStatistics(out allocatedBuffers);
        }

        /// <summary>
        /// Enable gathering of peak memory statistics of the MKL memory pool.
        /// </summary>
        public void EnablePeakMemoryStatistics()
        {
            MklProvider.EnablePeakMemoryStatistics();
        }

        /// <summary>
        /// Disable gathering of peak memory statistics of the MKL memory pool.
        /// </summary>
        public void DisablePeakMemoryStatistics()
        {
            MklProvider.DisablePeakMemoryStatistics();
        }

        /// <summary>
        /// Measures peak memory usage of the MKL memory pool.
        /// </summary>
        /// <param name="reset">Whether the usage counter should be reset.</param>
        /// <returns>The peak number of bytes allocated to all memory buffers.</returns>
        public long PeakMemoryStatistics(bool reset = true)
        {
            return MklProvider.PeakMemoryStatistics(reset);
        }

        public override string ToString()
        {
            return MklProvider.Describe();
        }

        public void ForwardInplace(Complex[] complex, FourierTransformScaling scaling)
        {
            SafeNativeMethods.z_fft_forward_inplace(complex.Length, ForwardScaling(scaling, complex.Length), complex);
        }

        public void BackwardInplace(Complex[] complex, FourierTransformScaling scaling)
        {
            SafeNativeMethods.z_fft_backward_inplace(complex.Length, BackwardScaling(scaling, complex.Length), complex);
        }

        public Complex[] Forward(Complex[] complexTimeSpace, FourierTransformScaling scaling)
        {
            Complex[] work = new Complex[complexTimeSpace.Length];
            complexTimeSpace.Copy(work);
            ForwardInplace(work, scaling);
            return work;
        }

        public Complex[] Backward(Complex[] complexFrequenceSpace, FourierTransformScaling scaling)
        {
            Complex[] work = new Complex[complexFrequenceSpace.Length];
            complexFrequenceSpace.Copy(work);
            BackwardInplace(work, scaling);
            return work;
        }

        static double ForwardScaling(FourierTransformScaling scaling, int length)
        {
            switch (scaling)
            {
                case FourierTransformScaling.SymmetricScaling:
                    return Math.Sqrt(1.0/length);
                case FourierTransformScaling.ForwardScaling:
                    return 1.0/length;
                default:
                    return 1.0;
            }
        }

        static double BackwardScaling(FourierTransformScaling scaling, int length)
        {
            switch (scaling)
            {
                case FourierTransformScaling.SymmetricScaling:
                    return Math.Sqrt(1.0/length);
                case FourierTransformScaling.BackwardScaling:
                    return 1.0/length;
                default:
                    return 1.0;
            }
        }
    }
}

#endif
