// <copyright file="LinearAlgebraControl.cs" company="Math.NET">
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

namespace MathNet.Numerics.Providers.LinearAlgebra
{
    public static class LinearAlgebraControl
    {
        const string EnvVarLAProvider = "MathNetNumericsLAProvider";

        static ILinearAlgebraProvider _linearAlgebraProvider;
        static readonly object StaticLock = new object();

        const string MklTypeName = "MathNet.Numerics.Providers.MKL.LinearAlgebra.MklLinearAlgebraControl, MathNet.Numerics.Providers.MKL";
        static readonly ProviderProbe<ILinearAlgebraProvider> MklProbe = new ProviderProbe<ILinearAlgebraProvider>(MklTypeName, AppSwitches.DisableMklNativeProvider);

        const string OpenBlasTypeName = "MathNet.Numerics.Providers.OpenBLAS.LinearAlgebra.OpenBlasLinearAlgebraControl, MathNet.Numerics.Providers.OpenBLAS";
        static readonly ProviderProbe<ILinearAlgebraProvider> OpenBlasProbe = new ProviderProbe<ILinearAlgebraProvider>(OpenBlasTypeName, AppSwitches.DisableOpenBlasNativeProvider);

        const string CudaTypeName = "MathNet.Numerics.Providers.CUDA.LinearAlgebra.CudaLinearAlgebraControl, MathNet.Numerics.Providers.CUDA";
        static readonly ProviderProbe<ILinearAlgebraProvider> CudaProbe = new ProviderProbe<ILinearAlgebraProvider>(CudaTypeName, AppSwitches.DisableCudaNativeProvider);

        /// <summary>
        /// Optional path to try to load native provider binaries from,
        /// if the provider specific hint path is not set.
        /// If neither is set, Numerics falls back to the provider specific
        /// environment variables, or the default probing paths.
        /// </summary>
        public static string HintPath { get; set; }

        /// <summary>
        /// Gets or sets the linear algebra provider.
        /// Consider to use UseNativeMKL or UseManaged instead.
        /// </summary>
        /// <value>The linear algebra provider.</value>
        public static ILinearAlgebraProvider Provider
        {
            get
            {
                if (_linearAlgebraProvider == null)
                {
                    lock (StaticLock)
                    {
                        if (_linearAlgebraProvider == null)
                        {
                            UseDefault();
                        }
                    }
                }

                return _linearAlgebraProvider;
            }
            set
            {
                value.InitializeVerify();

                // only actually set if verification did not throw
                _linearAlgebraProvider = value;
            }
        }

        public static void UseManaged() => Provider = ManagedLinearAlgebraProvider.Instance;

        public static void UseNativeMKL() => Provider = MklProbe.Create();
        public static bool TryUseNativeMKL() => TryUse(MklProbe.TryCreate());

        public static void UseNativeCUDA() => Provider = CudaProbe.Create();
        public static bool TryUseNativeCUDA() => TryUse(CudaProbe.TryCreate());

        public static void UseNativeOpenBLAS() => Provider = OpenBlasProbe.Create();
        public static bool TryUseNativeOpenBLAS() => TryUse(OpenBlasProbe.TryCreate());

        /// <summary>
        /// Try to use a native provider, if available.
        /// </summary>
        public static bool TryUseNative()
        {
            if (AppSwitches.DisableNativeProviders || AppSwitches.DisableNativeProviderProbing)
            {
                return false;
            }

            return TryUseNativeMKL() || TryUseNativeOpenBLAS() || TryUseNativeCUDA();
        }

        public static bool TryUse(ILinearAlgebraProvider provider)
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
        /// "MathNetNumericsLAProvider" environment variable,
        /// or fall back to the best provider.
        /// </summary>
        public static void UseDefault()
        {
            if (AppSwitches.DisableNativeProviders)
            {
                UseManaged();
                return;
            }

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
        }

        public static void FreeResources() => Provider.FreeResources();
    }
}
