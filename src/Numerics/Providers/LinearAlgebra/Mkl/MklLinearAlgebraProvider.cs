// <copyright file="MklLinearAlgebraProvider.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
//
// Copyright (c) 2009-2014 Math.NET
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

#if NATIVEMKL

namespace MathNet.Numerics.Providers.LinearAlgebra.Mkl
{
    /// <summary>
    /// Consistency vs. performance trade-off between runs on different machines.
    /// </summary>
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
    public enum MklAccuracy : uint
    {
        Low = 0x1,
        High = 0x2
    }

    [CLSCompliant(false)]
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
        int _nativeRevision;
        bool _nativeIX86;
        bool _nativeX64;
        bool _nativeIA64;

        readonly MklConsistency _consistency;
        readonly MklPrecision _precision;
        readonly MklAccuracy _accuracy;

        /// <param name="consistency">
        /// Sets the desired bit consistency on repeated identical computations on varying CPU architectures,
        /// as a trade-off with performance.
        /// </param>
        /// <param name="precision">VML optimal precision and rounding.</param>
        /// <param name="accuracy">VML accuracy mode.</param>
        [CLSCompliant(false)]
        public MklLinearAlgebraProvider(
            MklConsistency consistency = MklConsistency.Auto,
            MklPrecision precision = MklPrecision.Double,
            MklAccuracy accuracy = MklAccuracy.High)
        {
            _consistency = consistency;
            _precision = precision;
            _accuracy = accuracy;
        }

        public MklLinearAlgebraProvider()
        {
            _consistency = MklConsistency.Auto;
            _precision = MklPrecision.Double;
            _accuracy = MklAccuracy.High;
        }

        /// <summary>
        /// Initialize and verify that the provided is indeed available.
        /// If calling this method fails, consider to fall back to alternatives like the managed provider.
        /// </summary>
        public override void InitializeVerify()
        {
            // TODO: Choose x86 or x64 based on Environment.Is64BitProcess

            int a, b, linearAlgebra;
            try
            {
                a = SafeNativeMethods.query_capability(0);
                b = SafeNativeMethods.query_capability(1);

                _nativeIX86 = SafeNativeMethods.query_capability(8) > 0;
                _nativeX64 = SafeNativeMethods.query_capability(9) > 0;
                _nativeIA64 = SafeNativeMethods.query_capability(10) > 0;

                _nativeRevision = SafeNativeMethods.query_capability(64);
                linearAlgebra = SafeNativeMethods.query_capability(128);
            }
            catch (DllNotFoundException e)
            {
                throw new NotSupportedException("MKL Native Provider not found.", e);
            }
            catch (BadImageFormatException e)
            {
                throw new NotSupportedException("MKL Native Provider found but failed to load. Please verify that the platform matches (x64 vs x32, Windows vs Linux).", e);
            }
            catch (EntryPointNotFoundException e)
            {
                // we currently accept this to continue to support the old version for a while.
                // however, this is planned to be dropped for the final v3 release at latest.
                // TODO: drop return statement and instead fail with the exception below
                return;

                throw new NotSupportedException("MKL Native Provider found but does not support capability querying and is therefore not compatible. Try to upgrade to a newer version.", e);
            }

            if (a != 0 || b != -1 || linearAlgebra <=0 || _nativeRevision < 4)
            {
                throw new NotSupportedException("MKL Native Provider found but too old or not compatible.");
            }

            if (SafeNativeMethods.query_capability(65) > 0)
            {
                SafeNativeMethods.set_consistency_mode((int)_consistency);
                SafeNativeMethods.set_vml_mode((uint)_precision | (uint)_accuracy);
            }
        }

        public override string ToString()
        {
            return string.Format("Intel MKL ({1}; revision {0})", _nativeRevision, _nativeIX86 ? "x86" : _nativeX64 ? "x64" : _nativeIA64 ? "IA64" : "unknown");
        }
    }
}

#endif
