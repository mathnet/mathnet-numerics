// <copyright file="FourierTransformControl.cs" company="Math.NET">
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

namespace MathNet.Numerics.Providers.FourierTransform
{
    internal static class FourierTransformControl
    {
        const string EnvVarFFTProvider = "MathNetNumericsFFTProvider";

        public static void UseManaged()
        {
            Control.FourierTransformProvider = new ManagedFourierTransformProvider();
        }

#if NATIVE
        public static void UseNativeMKL()
        {
            Control.FourierTransformProvider = new Mkl.MklFourierTransformProvider();
        }
#endif

        public static bool TryUse(IFourierTransformProvider provider)
        {
            try
            {
                if (!provider.IsAvailable())
                {
                    return false;
                }

                Control.FourierTransformProvider = provider;
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
            if (!TryUse(new Mkl.MklFourierTransformProvider()))
            {
                UseManaged();
            }
#else
            UseManaged();
#endif
        }

        public static void UseDefault()
        {
            var value = Environment.GetEnvironmentVariable(EnvVarFFTProvider);
            switch (value != null ? value.ToUpperInvariant() : string.Empty)
            {
#if NATIVE
                case "MKL":
                    UseNativeMKL();
                    break;
#endif
                default:
                    UseBest();
                    break;
            }
        }
    }
}
