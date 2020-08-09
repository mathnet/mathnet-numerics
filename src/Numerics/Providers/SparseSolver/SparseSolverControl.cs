using System;

namespace MathNet.Numerics.Providers.SparseSolver
{
    public static class SparseSolverControl
    {
        const string EnvVarSSProvider = "MathNetNumericsSSProvider";
        const string EnvVarSSProviderPath = "MathNetNumericsSSProviderPath";

        static ISparseSolverProvider _sparseSolverProvider;
        static readonly object StaticLock = new object();

        /// <summary>
        /// Gets or sets the sparse solver provider. Consider to use UseNativeMKL or UseManaged instead.
        /// </summary>
        /// <value>The linear algebra provider.</value>
        public static ISparseSolverProvider Provider
        {
            get
            {
                if (_sparseSolverProvider == null)
                {
                    lock (StaticLock)
                    {
                        if (_sparseSolverProvider == null)
                        {
                            UseDefault();
                        }
                    }
                }

                return _sparseSolverProvider;
            }
            set
            {
                value.InitializeVerify();

                // only actually set if verification did not throw
                _sparseSolverProvider = value;
            }
        }

        /// <summary>
        /// Optional path to try to load native provider binaries from.
        /// If not set, Numerics will fall back to the environment variable
        /// `MathNetNumericsSSProviderPath` or the default probing paths.
        /// </summary>
        public static string HintPath { get; set; }

        public static ISparseSolverProvider CreateManaged()
        {
            return new Managed.ManagedSparseSolverProvider();
        }

        public static void UseManaged()
        {
            Provider = CreateManaged();
        }

#if NATIVE
        public static ISparseSolverProvider CreateNativeMKL()
        {
            return new Mkl.MklSparseSolverProvider(GetCombinedHintPath());
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
            return TryUseNativeMKL();
        }
#endif

        static bool TryUse(ISparseSolverProvider provider)
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
        /// "MathNetNumericsDSSProvider" environment variable,
        /// or fall back to the best provider.
        /// </summary>
        public static void UseDefault()
        {
#if NATIVE
            var value = Environment.GetEnvironmentVariable(EnvVarSSProvider);
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

            var value = Environment.GetEnvironmentVariable(EnvVarSSProviderPath);
            if (!String.IsNullOrEmpty(value))
            {
                return value;
            }

            return null;
        }
    }
}
