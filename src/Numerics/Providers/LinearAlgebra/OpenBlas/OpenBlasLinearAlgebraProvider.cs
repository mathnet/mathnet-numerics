// <copyright file="OpenBlasLinearAlgebraProvider.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
//
// Copyright (c) 2009-2015 Math.NET
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

namespace MathNet.Numerics.Providers.LinearAlgebra.OpenBlas
{
    internal enum ParallelType : int
    {
        Sequential = 0,
        Thread = 1,
        OpenMP = 2
    }

    /// <summary>
    /// OpenBLAS linear algebra provider.
    /// </summary>
    public partial class OpenBlasLinearAlgebraProvider : ManagedLinearAlgebraProvider
    {
        int _nativeRevision;
        bool _nativeIX86;
        bool _nativeX64;
        bool _nativeIA64;
        bool _nativeARM;

        public override void InitializeVerify()
        {
            int a, b, linearAlgebra;
            try
            {
                // Load the native library
                NativeProviderLoader.TryLoad(SafeNativeMethods.DllName);

                a = SafeNativeMethods.query_capability(0);
                b = SafeNativeMethods.query_capability(1);

                _nativeIX86 = SafeNativeMethods.query_capability((int)ProviderPlatform.x86) > 0;
                _nativeX64 = SafeNativeMethods.query_capability((int)ProviderPlatform.x64) > 0;
                _nativeIA64 = SafeNativeMethods.query_capability((int)ProviderPlatform.ia64) > 0;
                _nativeARM = SafeNativeMethods.query_capability((int)ProviderPlatform.arm) > 0;

                _nativeRevision = SafeNativeMethods.query_capability((int)ProviderConfig.Revision);
                linearAlgebra = SafeNativeMethods.query_capability((int)ProviderCapability.LinearAlgebra);
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

            if (a != 0 || b != -1 || linearAlgebra <=0 || _nativeRevision < 1)
            {
                throw new NotSupportedException("OpenBLAS Native Provider too old or not compatible. Consider upgrading to a newer version.");
            }

            // set threading settings, if supported
            if (SafeNativeMethods.query_capability((int)ProviderConfig.Threading) > 0)
            {
                SafeNativeMethods.set_max_threads(Control.MaxDegreeOfParallelism);
            }
        }

        public override string ToString()
        {
            return string.Format("OpenBLAS ({1}; revision {0})",
                _nativeRevision,
                _nativeIX86 ? "x86" : _nativeX64 ? "x64" : _nativeIA64 ? "IA64" : _nativeARM ? "ARM" : "unknown");
        }
    }
}

#endif
