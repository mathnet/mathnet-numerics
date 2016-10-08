// <copyright file="OpenBlasLinearAlgebraProvider.cs" company="Math.NET">
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
using MathNet.Numerics.Providers.Common.OpenBlas;

namespace MathNet.Numerics.Providers.LinearAlgebra.OpenBlas
{
    /// <summary>
    /// Error codes return from the native OpenBLAS provider.
    /// </summary>
    public enum NativeError : int
    {
        /// <summary>
        /// Unable to allocate memory.
        /// </summary>
        MemoryAllocation = -999999
    }

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
        /// <summary>
        /// Try to find out whether the provider is available, at least in principle.
        /// Verification may still fail if available, but it will certainly fail if unavailable.
        /// </summary>
        public override bool IsAvailable()
        {
            return OpenBlasProvider.IsAvailable(minRevision: 1);
        }

        /// <summary>
        /// Initialize and verify that the provided is indeed available.
        /// If not, fall back to alternatives like the managed provider
        /// </summary>
        public override void InitializeVerify()
        {
            OpenBlasProvider.Load(minRevision: 1);

            int linearAlgebra = SafeNativeMethods.query_capability((int)ProviderCapability.LinearAlgebraMajor);

            // we only support exactly one major version, since major version changes imply a breaking change.
            if (linearAlgebra != 1)
            {
                throw new NotSupportedException(string.Format("OpenBLAS Native Provider not compatible. Expecting linear algebra v1 but provider implements v{0}.", linearAlgebra));
            }
        }

        public override string ToString()
        {
            return OpenBlasProvider.Describe();
        }
    }
}

#endif
