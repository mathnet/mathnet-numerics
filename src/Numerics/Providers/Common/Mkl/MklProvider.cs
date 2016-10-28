// <copyright file="MklProvider.cs" company="Math.NET">
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
using System.Collections.Generic;

namespace MathNet.Numerics.Providers.Common.Mkl
{
    internal static class MklProvider
    {
        const int _designTimeRevision = 11;
        static int _nativeRevision;
        static Version _mklVersion;
        static bool _nativeX86;
        static bool _nativeX64;
        static bool _nativeIA64;

        public static bool IsAvailable(int minRevision)
        {
            try
            {
                if (!NativeProviderLoader.TryLoad(SafeNativeMethods.DllName))
                {
                    return false;
                }

                int a = SafeNativeMethods.query_capability(0);
                int b = SafeNativeMethods.query_capability(1);
                int nativeRevision = SafeNativeMethods.query_capability((int)ProviderConfig.Revision);
                return a == 0 && b == -1 && nativeRevision >= minRevision;
            }
            catch
            {
                return false;
            }
        }

        public static void Load(int minRevision)
        {
            int a, b;
            try
            {
                NativeProviderLoader.TryLoad(SafeNativeMethods.DllName);

                a = SafeNativeMethods.query_capability(0);
                b = SafeNativeMethods.query_capability(1);
                _nativeRevision = SafeNativeMethods.query_capability((int)ProviderConfig.Revision);

                _nativeX86 = SafeNativeMethods.query_capability((int)ProviderPlatform.x86) > 0;
                _nativeX64 = SafeNativeMethods.query_capability((int)ProviderPlatform.x64) > 0;
                _nativeIA64 = SafeNativeMethods.query_capability((int)ProviderPlatform.ia64) > 0;

                _mklVersion = new Version(
                    SafeNativeMethods.query_capability((int)ProviderConfig.MklMajorVersion),
                    SafeNativeMethods.query_capability((int)ProviderConfig.MklMinorVersion),
                    SafeNativeMethods.query_capability((int)ProviderConfig.MklUpdateVersion));
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
                throw new NotSupportedException("MKL Native Provider does not support capability querying and is therefore not compatible. Consider upgrading to a newer version.", e);
            }

            if (a != 0 || b != -1 || _nativeRevision < minRevision)
            {
                throw new NotSupportedException("MKL Native Provider too old. Consider upgrading to a newer version.");
            }

            ConfigureThreading();
        }

        static void ConfigureThreading()
        {
            // set threading settings, if supported
            if (SafeNativeMethods.query_capability((int)ProviderConfig.Threading) > 0)
            {
                SafeNativeMethods.set_max_threads(Control.MaxDegreeOfParallelism);
            }
        }

        public static void ConfigurePrecision(MklConsistency consistency, MklPrecision precision, MklAccuracy accuracy)
        {
            // set numerical consistency, precision and accuracy modes, if supported
            if (SafeNativeMethods.query_capability((int)ProviderConfig.Precision) > 0)
            {
                SafeNativeMethods.set_consistency_mode((int)consistency);
                SafeNativeMethods.set_vml_mode((uint)precision | (uint)accuracy);
            }
        }

        /// <summary>
        /// Frees the memory allocated to the MKL memory pool.
        /// </summary>
        public static void FreeBuffers()
        {
            if (SafeNativeMethods.query_capability((int)ProviderConfig.Memory) < 1)
            {
                throw new NotSupportedException("MKL Native Provider does not support memory management functions. Consider upgrading to a newer version.");
            }

            SafeNativeMethods.free_buffers();
        }

        /// <summary>
        /// Frees the memory allocated to the MKL memory pool on the current thread.
        /// </summary>
        public static void ThreadFreeBuffers()
        {
            if (SafeNativeMethods.query_capability((int)ProviderConfig.Memory) < 1)
            {
                throw new NotSupportedException("MKL Native Provider does not support memory management functions. Consider upgrading to a newer version.");
            }

            SafeNativeMethods.thread_free_buffers();
        }

        /// <summary>
        /// Disable the MKL memory pool. May impact performance.
        /// </summary>
        public static void DisableMemoryPool()
        {
            if (SafeNativeMethods.query_capability((int)ProviderConfig.Memory) < 1)
            {
                throw new NotSupportedException("MKL Native Provider does not support memory management functions. Consider upgrading to a newer version.");
            }

            SafeNativeMethods.disable_fast_mm();
        }

        /// <summary>
        /// Retrieves information about the MKL memory pool.
        /// </summary>
        /// <param name="allocatedBuffers">On output, returns the number of memory buffers allocated.</param>
        /// <returns>Returns the number of bytes allocated to all memory buffers.</returns>
        public static long MemoryStatistics(out int allocatedBuffers)
        {
            if (SafeNativeMethods.query_capability((int)ProviderConfig.Memory) < 1)
            {
                throw new NotSupportedException("MKL Native Provider does not support memory management functions. Consider upgrading to a newer version.");
            }

            return SafeNativeMethods.mem_stat(out allocatedBuffers);
        }

        /// <summary>
        /// Enable gathering of peak memory statistics of the MKL memory pool.
        /// </summary>
        public static void EnablePeakMemoryStatistics()
        {
            if (SafeNativeMethods.query_capability((int)ProviderConfig.Memory) < 1)
            {
                throw new NotSupportedException("MKL Native Provider does not support memory management functions. Consider upgrading to a newer version.");
            }

            SafeNativeMethods.peak_mem_usage((int)MklMemoryRequestMode.Enable);
        }

        /// <summary>
        /// Disable gathering of peak memory statistics of the MKL memory pool.
        /// </summary>
        public static void DisablePeakMemoryStatistics()
        {
            if (SafeNativeMethods.query_capability((int)ProviderConfig.Memory) < 1)
            {
                throw new NotSupportedException("MKL Native Provider does not support memory management functions. Consider upgrading to a newer version.");
            }

            SafeNativeMethods.peak_mem_usage((int)MklMemoryRequestMode.Disable);
        }

        /// <summary>
        /// Measures peak memory usage of the MKL memory pool.
        /// </summary>
        /// <param name="reset">Whether the usage counter should be reset.</param>
        /// <returns>The peak number of bytes allocated to all memory buffers.</returns>
        public static long PeakMemoryStatistics(bool reset = true)
        {
            if (SafeNativeMethods.query_capability((int)ProviderConfig.Memory) < 1)
            {
                throw new NotSupportedException("MKL Native Provider does not support memory management functions. Consider upgrading to a newer version.");
            }

            return SafeNativeMethods.peak_mem_usage((int)(reset ? MklMemoryRequestMode.PeakMemoryReset : MklMemoryRequestMode.PeakMemory));
        }

        public static string Describe()
        {
            var parts = new List<string>();
            if (_nativeX86) parts.Add("x86");
            if (_nativeX64) parts.Add("x64");
            if (_nativeIA64) parts.Add("IA64");
            parts.Add("revision " + _nativeRevision);
            if (_nativeRevision > _designTimeRevision) parts.Add("ahead revision " + _designTimeRevision);
            if (_nativeRevision < _designTimeRevision) parts.Add("behind revision " + _designTimeRevision);
            if (_mklVersion.Major > 0)
            {
                parts.Add(_mklVersion.Build == 0
                    ? string.Concat("MKL ", _mklVersion.ToString(2))
                    : string.Concat("MKL ", _mklVersion.ToString(2), " Update ", _mklVersion.Build));
            }

            return string.Concat("Intel MKL (", string.Join("; ", parts), ")");
        }

        enum MklMemoryRequestMode : int
        {
            /// <summary>
            /// Disable gathering memory usage
            /// </summary>
            Disable = 0,

            /// <summary>
            /// Enable gathering memory usage
            /// </summary>
            Enable = 1,

            /// <summary>
            /// Return peak memory usage
            /// </summary>
            PeakMemory = 2,

            /// <summary>
            /// Return peak memory usage and reset counter
            /// </summary>
            PeakMemoryReset = -1
        }
    }
}

#endif
