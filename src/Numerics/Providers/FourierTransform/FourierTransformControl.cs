// <copyright file="FourierTransformControl.cs" company="Math.NET">
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

using System;

namespace MathNet.Numerics.Providers.FourierTransform
{
    public static class FourierTransformControl
    {
        const string EnvVarFFTProvider = "MathNetNumericsFFTProvider";
        const string EnvVarFFTProviderPath = "MathNetNumericsFFTProviderPath";

        static IFourierTransformProvider _fourierTransformProvider;
        static readonly object StaticLock = new object();

        /// <summary>
        /// Gets or sets the Fourier transform provider. Consider to use UseNativeMKL or UseManaged instead.
        /// </summary>
        /// <value>The linear algebra provider.</value>
        public static IFourierTransformProvider Provider
        {
            get
            {
                if (_fourierTransformProvider == null)
                {
                    lock (StaticLock)
                    {
                        if (_fourierTransformProvider == null)
                        {
                            UseDefault();
                        }
                    }
                }

                return _fourierTransformProvider;
            }
            set
            {
                value.InitializeVerify();

                // only actually set if verification did not throw
                _fourierTransformProvider = value;
            }
        }

        /// <summary>
        /// Optional path to try to load native provider binaries from.
        /// If not set, Numerics will fall back to the environment variable
        /// `MathNetNumericsFFTProviderPath` or the default probing paths.
        /// </summary>
        public static string HintPath { get; set; }

        public static IFourierTransformProvider CreateManaged()
        {
            return new Managed.ManagedFourierTransformProvider();
        }

        public static void UseManaged()
        {
            Provider = CreateManaged();
        }

#if NATIVE
        public static IFourierTransformProvider CreateNativeMKL()
        {
            return new Mkl.MklFourierTransformProvider(GetCombinedHintPath());
        }

        public static void UseNativeMKL()
        {
            Provider = CreateNativeMKL();
        }

        public static bool TryUseNativeMKL()
        {
            return TryUse(CreateNativeMKL());
        }

        /// <summary>
        /// Try to use a native provider, if available.
        /// </summary>
        public static bool TryUseNative()
        {
            if (AppSwitches.DisableNativeProviders || AppSwitches.DisableNativeProviderProbing)
            {
                return false;
            }

            return TryUseNativeMKL();
        }
#endif

        static bool TryUse(IFourierTransformProvider provider)
        {
            try
            {
                if (!provider.IsAvailable())
                {
                    return false;
                }

                Provider = provider;
                return true;
            }
            catch
            {
                // intentionally swallow exceptions here - use the explicit variants if you're interested in why
                return false;
            }
        }

        /// <summary>
        /// Use the best provider available.
        /// </summary>
        public static void UseBest()
        {
            if (AppSwitches.DisableNativeProviders || AppSwitches.DisableNativeProviderProbing)
            {
                UseManaged();
                return;
            }

#if NATIVE
            if (!TryUseNative())
            {
                UseManaged();
            }
#else
            UseManaged();
#endif
        }

        /// <summary>
        /// Use a specific provider if configured, e.g. using the
        /// "MathNetNumericsFFTProvider" environment variable,
        /// or fall back to the best provider.
        /// </summary>
        public static void UseDefault()
        {
            if (AppSwitches.DisableNativeProviders)
            {
                UseManaged();
                return;
            }

#if NATIVE
            var value = Environment.GetEnvironmentVariable(EnvVarFFTProvider);
            switch (value != null ? value.ToUpperInvariant() : string.Empty)
            {

                case "MKL":
                    UseNativeMKL();
                    break;

                default:
                    UseBest();
                    break;
            }
#else
            UseBest();
#endif
        }

        public static void FreeResources()
        {
            Provider.FreeResources();
        }

        static string GetCombinedHintPath()
        {
            if (!String.IsNullOrEmpty(HintPath))
            {
                return HintPath;
            }

            var value = Environment.GetEnvironmentVariable(EnvVarFFTProviderPath);
            if (!String.IsNullOrEmpty(value))
            {
                return value;
            }

            return null;
        }
    }
}
