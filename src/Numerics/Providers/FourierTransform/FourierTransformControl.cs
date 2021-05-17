// <copyright file="FourierTransformControl.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2021 Math.NET
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

        static IFourierTransformProvider _fourierTransformProvider;
        static readonly object StaticLock = new object();

        const string MklTypeName = "MathNet.Numerics.Providers.MKL.FourierTransform.MklFourierTransformControl, MathNet.Numerics.Providers.MKL";
        static readonly ProviderProbe<IFourierTransformProvider> MklProbe = new ProviderProbe<IFourierTransformProvider>(MklTypeName, AppSwitches.DisableMklNativeProvider);

        /// <summary>
        /// Optional path to try to load native provider binaries from,
        /// if the provider specific hint path is not set.
        /// If neither is set, Numerics falls back to the provider specific
        /// environment variables, or the default probing paths.
        /// </summary>
        public static string HintPath { get; set; }

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

        public static void UseManaged() => Provider = ManagedFourierTransformProvider.Instance;

        public static void UseNativeMKL() => Provider = MklProbe.Create();
        public static bool TryUseNativeMKL() => TryUse(MklProbe.TryCreate());


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

        public static bool TryUse(IFourierTransformProvider provider)
        {
            try
            {
                if (provider == null || !provider.IsAvailable())
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

            if (!TryUseNative())
            {
                UseManaged();
            }
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
        }

        public static void FreeResources() => Provider.FreeResources();
    }
}
