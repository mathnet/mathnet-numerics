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

using System;

namespace MathNet.Numerics.Providers.Common.Mkl
{
    internal static class MklProvider
    {
        static int _nativeRevision;
        static bool _nativeX86;
        static bool _nativeX64;
        static bool _nativeIA64;

        public static void Load(int minRevision)
        {
            int a, b;
            try
            {
                // Load the native library
                NativeProviderLoader.TryLoad(SafeNativeMethods.DllName);

                a = SafeNativeMethods.query_capability(0);
                b = SafeNativeMethods.query_capability(1);

                _nativeX86 = SafeNativeMethods.query_capability((int)ProviderPlatform.x86) > 0;
                _nativeX64 = SafeNativeMethods.query_capability((int)ProviderPlatform.x64) > 0;
                _nativeIA64 = SafeNativeMethods.query_capability((int)ProviderPlatform.ia64) > 0;

                _nativeRevision = SafeNativeMethods.query_capability((int)ProviderConfig.Revision);
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
        }

        public static string Describe()
        {
            return string.Format("Intel MKL ({1}; revision {0})",
                _nativeRevision,
                _nativeX86 ? "x86" : _nativeX64 ? "x64" : _nativeIA64 ? "IA64" : "unknown");
        }
    }
}
