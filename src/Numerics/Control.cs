// <copyright file="Control.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
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

using MathNet.Numerics.Providers.LinearAlgebra;
using System;
using System.Threading.Tasks;

namespace MathNet.Numerics
{
    /// <summary>
    /// Sets parameters for the library.
    /// </summary>
    public static class Control
    {
        const string EnvVarLAProvider = "MathNetNumericsLAProvider";

        static int _maxDegreeOfParallelism;
        static int _blockSize;
        static int _parallelizeOrder;
        static int _parallelizeElements;
        static ILinearAlgebraProvider _linearAlgebraProvider;
        static readonly object _staticLock = new object();

        static Control()
        {
            ConfigureAuto();
        }

        public static void ConfigureAuto()
        {
            // Random Numbers & Distributions
            CheckDistributionParameters = true;

            // Parallelization & Threading
            ThreadSafeRandomNumberGenerators = true;
            _maxDegreeOfParallelism = Environment.ProcessorCount;
            _blockSize = 512;
            _parallelizeOrder = 64;
            _parallelizeElements = 300;
            TaskScheduler = TaskScheduler.Default;
        }

        private static void InitializeDefaultLinearAlgebraProvider()
        {
            lock (_staticLock)
            {
                if (_linearAlgebraProvider == null)
                {
#if NATIVE
                    try
                    {
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
                                if (!TryUseNative())
                                {
                                    UseManaged();
                                }
                                break;
                        }
                    }
                    catch
                    {
                        // We don't care about any failures here at all (because "auto")
                        UseManaged();
                    }
#else
                    UseManaged();
#endif
                }
            }
        }

        public static void UseManaged()
        {
            LinearAlgebraProvider = new ManagedLinearAlgebraProvider();
        }

#if NATIVE

        /// <summary>
        /// Use the Intel MKL native provider for linear algebra.
        /// Throws if it is not available or failed to initialize, in which case the previous provider is still active.
        /// </summary>
        public static void UseNativeMKL()
        {
            LinearAlgebraProvider = new Providers.LinearAlgebra.Mkl.MklLinearAlgebraProvider();
        }

        /// <summary>
        /// Use the Intel MKL native provider for linear algebra, with the specified configuration parameters.
        /// Throws if it is not available or failed to initialize, in which case the previous provider is still active.
        /// </summary>
        [CLSCompliant(false)]
        public static void UseNativeMKL(
            Providers.LinearAlgebra.Mkl.MklConsistency consistency = Providers.LinearAlgebra.Mkl.MklConsistency.Auto,
            Providers.LinearAlgebra.Mkl.MklPrecision precision = Providers.LinearAlgebra.Mkl.MklPrecision.Double,
            Providers.LinearAlgebra.Mkl.MklAccuracy accuracy = Providers.LinearAlgebra.Mkl.MklAccuracy.High)
        {
            LinearAlgebraProvider = new Providers.LinearAlgebra.Mkl.MklLinearAlgebraProvider(consistency, precision, accuracy);
        }

        /// <summary>
        /// Try to use the Intel MKL native provider for linear algebra.
        /// </summary>
        /// <returns>
        /// True if the provider was found and initialized successfully.
        /// False if it failed and the previous provider is still active.
        /// </returns>
        public static bool TryUseNativeMKL()
        {
            return Try(UseNativeMKL);
        }

        /// <summary>
        /// Use the Nvidia CUDA native provider for linear algebra.
        /// Throws if it is not available or failed to initialize, in which case the previous provider is still active.
        /// </summary>
        public static void UseNativeCUDA()
        {
            LinearAlgebraProvider = new Providers.LinearAlgebra.Cuda.CudaLinearAlgebraProvider();
        }

        /// <summary>
        /// Try to use the Nvidia CUDA native provider for linear algebra.
        /// </summary>
        /// <returns>
        /// True if the provider was found and initialized successfully.
        /// False if it failed and the previous provider is still active.
        /// </returns>
        public static bool TryUseNativeCUDA()
        {
            return Try(UseNativeCUDA);
        }

        /// <summary>
        /// Use the OpenBLAS native provider for linear algebra.
        /// Throws if it is not available or failed to initialize, in which case the previous provider is still active.
        /// </summary>
        public static void UseNativeOpenBLAS()
        {
            LinearAlgebraProvider = new Providers.LinearAlgebra.OpenBlas.OpenBlasLinearAlgebraProvider();
        }

        /// <summary>
        /// Try to use the OpenBLAS native provider for linear algebra.
        /// </summary>
        /// <returns>
        /// True if the provider was found and initialized successfully.
        /// False if it failed and the previous provider is still active.
        /// </returns>
        public static bool TryUseNativeOpenBLAS()
        {
            return Try(UseNativeOpenBLAS);
        }

        /// <summary>
        /// Try to use any available native provider in an undefined order.
        /// </summary>
        /// <returns>
        /// True if one of the native providers was found and successfully initialized.
        /// False if it failed and the previous provider is still active.
        /// </returns>
        public static bool TryUseNative()
        {
            return TryUseNativeCUDA() || TryUseNativeMKL() || TryUseNativeOpenBLAS();
        }

        static bool Try(Action action)
        {
            try
            {
                action();
                return true;
            }
            catch
            {
                // intentionally swallow exceptions here - use the non-try variants if you're interested in why
                return false;
            }
        }
#endif

        public static void UseSingleThread()
        {
            _maxDegreeOfParallelism = 1;
            ThreadSafeRandomNumberGenerators = false;

            LinearAlgebraProvider.InitializeVerify();
        }

        public static void UseMultiThreading()
        {
            _maxDegreeOfParallelism = Environment.ProcessorCount;
            ThreadSafeRandomNumberGenerators = true;

            LinearAlgebraProvider.InitializeVerify();
        }

        /// <summary>
        /// Gets or sets a value indicating whether the distribution classes check validate each parameter.
        /// For the multivariate distributions this could involve an expensive matrix factorization.
        /// The default setting of this property is <c>true</c>.
        /// </summary>
        public static bool CheckDistributionParameters { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use thread safe random number generators (RNG).
        /// Thread safe RNG about two and half time slower than non-thread safe RNG.
        /// </summary>
        /// <value>
        ///     <c>true</c> to use thread safe random number generators ; otherwise, <c>false</c>.
        /// </value>
        public static bool ThreadSafeRandomNumberGenerators { get; set; }

        /// <summary>
        /// Optional path to try to load native provider binaries from.
        /// </summary>
        public static string NativeProviderPath { get; set; }

        /// <summary>
        /// Gets or sets the linear algebra provider. Consider to use UseNativeMKL or UseManaged instead.
        /// </summary>
        /// <value>The linear algebra provider.</value>
        public static ILinearAlgebraProvider LinearAlgebraProvider
        {
            get
            {
                if (_linearAlgebraProvider == null)
                    InitializeDefaultLinearAlgebraProvider();

                return _linearAlgebraProvider;
            }
            set
            {
                value.InitializeVerify();

                // only actually set if verification did not throw
                _linearAlgebraProvider = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating how many parallel worker threads shall be used
        /// when parallelization is applicable.
        /// </summary>
        /// <remarks>Default to the number of processor cores, must be between 1 and 1024 (inclusive).</remarks>
        public static int MaxDegreeOfParallelism
        {
            get { return _maxDegreeOfParallelism; }
            set
            {
                _maxDegreeOfParallelism = Math.Max(1, Math.Min(1024, value));

                // Reinitialize providers:
                LinearAlgebraProvider.InitializeVerify();
            }
        }

        /// <summary>
        /// Gets or sets the TaskScheduler used to schedule the worker tasks.
        /// </summary>
        public static TaskScheduler TaskScheduler { get; set; }

        /// <summary>
        /// Gets or sets the the block size to use for
        /// the native linear algebra provider.
        /// </summary>
        /// <value>The block size. Default 512, must be at least 32.</value>
        public static int BlockSize
        {
            get { return _blockSize; }
            set { _blockSize = Math.Max(32, value); }
        }

        /// <summary>
        /// Gets or sets the order of the matrix when linear algebra provider
        /// must calculate multiply in parallel threads.
        /// </summary>
        /// <value>The order. Default 64, must be at least 3.</value>
        internal static int ParallelizeOrder
        {
            get { return _parallelizeOrder; }
            set { _parallelizeOrder = Math.Max(3, value); }
        }

        /// <summary>
        /// Gets or sets the number of elements a vector or matrix
        /// must contain before we multiply threads.
        /// </summary>
        /// <value>Number of elements. Default 300, must be at least 3.</value>
        internal static int ParallelizeElements
        {
            get { return _parallelizeElements; }
            set { _parallelizeElements = Math.Max(3, value); }
        }
    }
}
