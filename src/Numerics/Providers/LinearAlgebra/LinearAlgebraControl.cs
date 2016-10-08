// <copyright file="LinearAlgebraControl.cs" company="Math.NET">
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

namespace MathNet.Numerics.Providers.LinearAlgebra
{
    internal static class LinearAlgebraControl
    {
        const string EnvVarLAProvider = "MathNetNumericsLAProvider";

        public static void UseManaged()
        {
            Control.LinearAlgebraProvider = new ManagedLinearAlgebraProvider();
        }

#if NATIVE
        public static void UseNativeMKL(
            Common.Mkl.MklConsistency consistency = Common.Mkl.MklConsistency.Auto,
            Common.Mkl.MklPrecision precision = Common.Mkl.MklPrecision.Double,
            Common.Mkl.MklAccuracy accuracy = Common.Mkl.MklAccuracy.High)
        {
            Control.LinearAlgebraProvider = new Mkl.MklLinearAlgebraProvider(consistency, precision, accuracy);
        }

        public static void UseNativeCUDA()
        {
            Control.LinearAlgebraProvider = new Cuda.CudaLinearAlgebraProvider();
        }

        public static void UseNativeOpenBLAS()
        {
            Control.LinearAlgebraProvider = new OpenBlas.OpenBlasLinearAlgebraProvider();
        }
#endif

        public static bool TryUse(ILinearAlgebraProvider provider)
        {
            try
            {
                if (!provider.IsAvailable())
                {
                    return false;
                }

                Control.LinearAlgebraProvider = provider;
                return true;
            }
            catch
            {
                // intentionally swallow exceptions here - use the explicit variants if you're interested in why
                return false;
            }
        }

        public static void UseBest()
        {
#if NATIVE
            if (!(TryUse(new Cuda.CudaLinearAlgebraProvider())
                  || TryUse(new Mkl.MklLinearAlgebraProvider())
                  || TryUse(new OpenBlas.OpenBlasLinearAlgebraProvider())))
            {
                UseManaged();
            }
#else
            UseManaged();
#endif
        }

        public static void UseDefault()
        {
#if NATIVE
            var value = Environment.GetEnvironmentVariable(EnvVarLAProvider);
            switch (value != null ? value.ToUpperInvariant() : string.Empty)
            {
                case "MKL":
                    UseNativeMKL();
                    break;

                case "CUDA":
                    UseNativeCUDA();
                    break;

                case "OPENBLAS":
                    UseNativeOpenBLAS();
                    break;
                default:
                    UseBest();
                    break;
            }
#else
            UseManaged();
#endif
        }
    }
}
