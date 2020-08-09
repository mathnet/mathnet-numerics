// <copyright file="OpenBlasProvider.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2020 Math.NET
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

namespace MathNet.Numerics.Providers.Common.OpenBlas
{
    public static class OpenBlasProvider
    {
        const int DesignTimeRevision = 1;
        const int MinimumCompatibleRevision = 1;

        static int _nativeRevision;
        static bool _nativeX86;
        static bool _nativeX64;
        static bool _nativeIA64;
        static bool _nativeARM;
        static bool _loaded;

        public static bool IsAvailable(string hintPath = null)
        {
            if (_loaded)
            {
                return true;
            }

            if (AppSwitches.DisableNativeProviders || AppSwitches.DisableOpenBlasNativeProvider)
            {
                return false;
            }

            try
            {
                if (!NativeProviderLoader.TryLoad(SafeNativeMethods.DllName, hintPath))
                {
                    return false;
                }

                int a = SafeNativeMethods.query_capability(0);
                int b = SafeNativeMethods.query_capability(1);
                int nativeRevision = SafeNativeMethods.query_capability((int)ProviderConfig.Revision);
                return a == 0 && b == -1 && nativeRevision >= MinimumCompatibleRevision;
            }
            catch
            {
                return false;
            }
        }

        /// <returns>Revision</returns>
        public static int Load(string hintPath = null)
        {
            if (_loaded)
            {
                return _nativeRevision;
            }

            if (AppSwitches.DisableNativeProviders || AppSwitches.DisableOpenBlasNativeProvider)
            {
                throw new NotSupportedException("OpenBLAS Native Provider support is actively disabled by AppSwitches.");
            }

            int a, b;
            try
            {
                NativeProviderLoader.TryLoad(SafeNativeMethods.DllName, hintPath);

                a = SafeNativeMethods.query_capability(0);
                b = SafeNativeMethods.query_capability(1);
                _nativeRevision = SafeNativeMethods.query_capability((int)ProviderConfig.Revision);

                _nativeX86 = SafeNativeMethods.query_capability((int)ProviderPlatform.x86) > 0;
                _nativeX64 = SafeNativeMethods.query_capability((int)ProviderPlatform.x64) > 0;
                _nativeIA64 = SafeNativeMethods.query_capability((int)ProviderPlatform.ia64) > 0;
                _nativeARM = SafeNativeMethods.query_capability((int)ProviderPlatform.arm) > 0;
            }
            catch (DllNotFoundException e)
            {
                throw new NotSupportedException("OpenBLAS Native Provider not found.", e);
            }
            catch (BadImageFormatException e)
            {
                throw new NotSupportedException("OpenBLAS Native Provider found but failed to load. Please verify that the platform matches (x64 vs x32, Windows vs Linux).", e);
            }
            catch (EntryPointNotFoundException e)
            {
                throw new NotSupportedException("OpenBLAS Native Provider does not support capability querying and is therefore not compatible. Consider upgrading to a newer version.", e);
            }

            if (a != 0 || b != -1 || _nativeRevision < MinimumCompatibleRevision)
            {
                throw new NotSupportedException("OpenBLAS Native Provider too old. Consider upgrading to a newer version.");
            }

            // set threading settings, if supported
            if (SafeNativeMethods.query_capability((int)ProviderConfig.Threading) > 0)
            {
                SafeNativeMethods.set_max_threads(Control.MaxDegreeOfParallelism);
            }

            _loaded = true;
            return _nativeRevision;
        }

        /// <summary>
        /// Frees memory buffers, caches and handles allocated in or to the provider.
        /// Does not unload the provider itself, it is still usable afterwards.
        /// This method is safe to call, even if the provider is not loaded.
        /// </summary>
        public static void FreeResources()
        {
        }

        internal static void ConfigureThreading()
        {
            if (!_loaded)
            {
                throw new InvalidOperationException();
            }

            // set threading settings, if supported
            if (SafeNativeMethods.query_capability((int)ProviderConfig.Threading) > 0)
            {
                SafeNativeMethods.set_max_threads(Control.MaxDegreeOfParallelism);
            }
        }

        public static string Describe()
        {
            if (!_loaded)
            {
                return "OpenBLAS (not loaded)";
            }

            var parts = new List<string>();
            if (_nativeX86) parts.Add("x86");
            if (_nativeX64) parts.Add("x64");
            if (_nativeIA64) parts.Add("IA64");
            if (_nativeARM) parts.Add("ARM");
            parts.Add("revision " + _nativeRevision);

            return string.Concat("OpenBLAS (", string.Join("; ", parts.ToArray()), ")");
        }
    }
}

#endif
