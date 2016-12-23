// <copyright file="MklLinearAlgebraProvider.cs" company="Math.NET">
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

#if NATIVE

using System;
using MathNet.Numerics.Providers.Common.Mkl;

namespace MathNet.Numerics.Providers.LinearAlgebra.Mkl
{
    /// <summary>
    /// Error codes return from the MKL provider.
    /// </summary>
    public enum MklError : int
    {
        /// <summary>
        /// Unable to allocate memory.
        /// </summary>
        MemoryAllocation = -999999
    }

    /// <summary>
    /// Consistency vs. performance trade-off between runs on different machines.
    /// </summary>
    [Obsolete("Will be removed in the next major version. Use the enums in the Common namespace instead.")]
    public enum MklConsistency : int
    {
        /// <summary>Consistent on the same CPU only (maximum performance)</summary>
        Auto = 2,
        /// <summary>Consistent on Intel and compatible CPUs with SSE2 support (maximum compatibility)</summary>
        Compatible = 3,
        /// <summary>Consistent on Intel CPUs supporting SSE2 or later</summary>
        SSE2 = 4,
        /// <summary>Consistent on Intel CPUs supporting SSE4.2 or later</summary>
        SSE4_2 = 8,
        /// <summary>Consistent on Intel CPUs supporting AVX or later</summary>
        AVX = 9,
        /// <summary>Consistent on Intel CPUs supporting AVX2 or later</summary>
        AVX2 = 10
    }

    [CLSCompliant(false)]
    [Obsolete("Will be removed in the next major version. Use the enums in the Common namespace instead.")]
    public enum MklAccuracy : uint
    {
        Low = 0x1,
        High = 0x2
    }

    [CLSCompliant(false)]
    [Obsolete("Will be removed in the next major version. Use the enums in the Common namespace instead.")]
    public enum MklPrecision : uint
    {
        Single = 0x10,
        Double = 0x20
    }

    /// <summary>
    /// Intel's Math Kernel Library (MKL) linear algebra provider.
    /// </summary>
    public partial class MklLinearAlgebraProvider : ManagedLinearAlgebraProvider
    {
        readonly Common.Mkl.MklConsistency _consistency;
        readonly Common.Mkl.MklPrecision _precision;
        readonly Common.Mkl.MklAccuracy _accuracy;

        int _linearAlgebraMajor;
        int _linearAlgebraMinor;
        int _vectorFunctionsMajor;
        int _vectorFunctionsMinor;

        /// <param name="consistency">
        /// Sets the desired bit consistency on repeated identical computations on varying CPU architectures,
        /// as a trade-off with performance.
        /// </param>
        /// <param name="precision">VML optimal precision and rounding.</param>
        /// <param name="accuracy">VML accuracy mode.</param>
        [CLSCompliant(false)]
        [Obsolete("Will be removed in the next major version. Use the enums in the Common namespace instead.")]
        public MklLinearAlgebraProvider(
            MklConsistency consistency = MklConsistency.Auto,
            MklPrecision precision = MklPrecision.Double,
            MklAccuracy accuracy = MklAccuracy.High)
        {
            _consistency = (Common.Mkl.MklConsistency)consistency;
            _precision = (Common.Mkl.MklPrecision)precision;
            _accuracy = (Common.Mkl.MklAccuracy)accuracy;
        }

        /// <param name="consistency">
        /// Sets the desired bit consistency on repeated identical computations on varying CPU architectures,
        /// as a trade-off with performance.
        /// </param>
        /// <param name="precision">VML optimal precision and rounding.</param>
        /// <param name="accuracy">VML accuracy mode.</param>
        [CLSCompliant(false)]
        public MklLinearAlgebraProvider(
            Common.Mkl.MklConsistency consistency = Common.Mkl.MklConsistency.Auto,
            Common.Mkl.MklPrecision precision = Common.Mkl.MklPrecision.Double,
            Common.Mkl.MklAccuracy accuracy = Common.Mkl.MklAccuracy.High)
        {
            _consistency = consistency;
            _precision = precision;
            _accuracy = accuracy;
        }

        public MklLinearAlgebraProvider()
        {
            _consistency = Common.Mkl.MklConsistency.Auto;
            _precision = Common.Mkl.MklPrecision.Double;
            _accuracy = Common.Mkl.MklAccuracy.High;
        }

        /// <summary>
        /// Try to find out whether the provider is available, at least in principle.
        /// Verification may still fail if available, but it will certainly fail if unavailable.
        /// </summary>
        public override bool IsAvailable()
        {
            return MklProvider.IsAvailable(minRevision: 4);
        }

        /// <summary>
        /// Initialize and verify that the provided is indeed available.
        /// If calling this method fails, consider to fall back to alternatives like the managed provider.
        /// </summary>
        public override void InitializeVerify()
        {
            MklProvider.Load(minRevision: 4);
            MklProvider.ConfigurePrecision(_consistency, _precision, _accuracy);

            _linearAlgebraMajor = SafeNativeMethods.query_capability((int)ProviderCapability.LinearAlgebraMajor);
            _linearAlgebraMinor = SafeNativeMethods.query_capability((int)ProviderCapability.LinearAlgebraMinor);
            _vectorFunctionsMajor = SafeNativeMethods.query_capability((int)ProviderCapability.VectorFunctionsMajor);
            _vectorFunctionsMinor = SafeNativeMethods.query_capability((int)ProviderCapability.VectorFunctionsMinor);

            // we only support exactly one major version, since major version changes imply a breaking change.
            if (_linearAlgebraMajor != 2)
            {
                throw new NotSupportedException(string.Format("MKL Native Provider not compatible. Expecting linear algebra v2 but provider implements v{0}.", _linearAlgebraMajor));
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
    }
}

#endif
