// <copyright file="MklLinearAlgebraProvider.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
//
// Copyright (c) 2009-2013 Math.NET
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
    /// Intel's Math Kernel Library (MKL) linear algebra provider.
    /// </summary>
    public partial class MklLinearAlgebraProvider : ManagedLinearAlgebraProvider
    {
        int _nativeRevision = 0;
        bool _nativeIX86 = false;
        bool _nativeX64 = false;
        bool _nativeIA64 = false;

        /// <param name="bitConsistent">If true improves MKL Consistency to get bit consistent results on repeated identical calculations</param>
        public MklLinearAlgebraProvider(bool bitConsistent = false)
        {
            if (bitConsistent)
            {
                SafeNativeMethods.SetImprovedConsistency();
            }
        }

        /// <summary>
        /// Initialize and verify that the provided is indeed available. If not, fall back to alernatives like the managed provider
        /// </summary>
        public override void InitializeVerify()
        {
            // TODO: Choose x86 or x64 based on Environment.Is64BitProcess

            int a = 0, b = 0, linearAlgebra = 0;
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
            catch (BadImageFormatException e)
            {
                throw new NotSupportedException("MKL Native Provider failed to load. Please verify that the platform matches (x64 vs x32, Windows vs Linux).", e);
            }
            catch (EntryPointNotFoundException e)
            {
                // we currently accept this to continue to support the old version for a while.
                // however, this is planned to be dropped for the final v3 release at latest.
                // TODO: drop return statement and instead fail with the exception below
                return;

                throw new NotSupportedException("MKL Native Provider does not support capability querying and is therefore not compatible. Try to upgrade to a newer version (1).", e);
            }

            if (a != 0 || b != -1 || linearAlgebra <=0 || _nativeRevision < 4)
            {
                throw new NotSupportedException("MKL Native Provider too old or not compatible (2).");
            }
        }

        public override string ToString()
        {
            return string.Format("Intel MKL ({1}; revision {0})", _nativeRevision, _nativeIX86 ? "x86" : _nativeX64 ? "x64" : _nativeIA64 ? "IA64" : "unknown");
        }
    }
}

#endif
