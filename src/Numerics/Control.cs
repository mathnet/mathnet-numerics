// <copyright file="Control.cs" company="Math.NET">
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
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.Providers.SparseSolver;
using MathNet.Numerics.Providers.FourierTransform;
using MathNet.Numerics.Providers.LinearAlgebra;

#if !NET461
using System.Runtime.InteropServices;
#endif

namespace MathNet.Numerics
{
    /// <summary>
    /// Sets parameters for the library.
    /// </summary>
    public static class Control
    {
        static int _maxDegreeOfParallelism;
        static int _parallelizeOrder;
        static int _parallelizeElements;
        static string _nativeProviderHintPath;

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
            _parallelizeOrder = 64;
            _parallelizeElements = 300;
            TaskScheduler = TaskScheduler.Default;
        }

        public static void UseManaged()
        {
            LinearAlgebraControl.UseManaged();
            FourierTransformControl.UseManaged();
            SparseSolverControl.UseManaged();
        }

        /// <summary>
        /// Use a specific provider if configured, e.g. using
        /// environment variables, or fall back to the best providers.
        /// </summary>
        public static void UseDefaultProviders()
        {
            if (AppSwitches.DisableNativeProviders)
            {
                UseManaged();
                return;
            }

            LinearAlgebraControl.UseDefault();
            FourierTransformControl.UseDefault();
            SparseSolverControl.UseDefault();
        }

        /// <summary>
        /// Use the best provider available.
        /// </summary>
        public static void UseBestProviders()
        {
            if (AppSwitches.DisableNativeProviders || AppSwitches.DisableNativeProviderProbing)
            {
                UseManaged();
                return;
            }

            LinearAlgebraControl.UseBest();
            FourierTransformControl.UseBest();
            SparseSolverControl.UseBest();
        }

        /// <summary>
        /// Use the Intel MKL native provider for linear algebra.
        /// Throws if it is not available or failed to initialize, in which case the previous provider is still active.
        /// </summary>
        public static void UseNativeMKL()
        {
            LinearAlgebraControl.UseNativeMKL();
            FourierTransformControl.UseNativeMKL();
            SparseSolverControl.UseNativeMKL();
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
            bool linearAlgebra = LinearAlgebraControl.TryUseNativeMKL();
            bool fourierTransform = FourierTransformControl.TryUseNativeMKL();
            bool directSparseSolver = SparseSolverControl.TryUseNativeMKL();
            return linearAlgebra || fourierTransform || directSparseSolver;
        }

        /// <summary>
        /// Use the Nvidia CUDA native provider for linear algebra.
        /// Throws if it is not available or failed to initialize, in which case the previous provider is still active.
        /// </summary>
        public static void UseNativeCUDA()
        {
            LinearAlgebraControl.UseNativeCUDA();
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
            bool linearAlgebra = LinearAlgebraControl.TryUseNativeCUDA();
            return linearAlgebra;
        }

        /// <summary>
        /// Use the OpenBLAS native provider for linear algebra.
        /// Throws if it is not available or failed to initialize, in which case the previous provider is still active.
        /// </summary>
        public static void UseNativeOpenBLAS()
        {
            LinearAlgebraControl.UseNativeOpenBLAS();
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
            bool linearAlgebra = LinearAlgebraControl.TryUseNativeOpenBLAS();
            return linearAlgebra;
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
            if (AppSwitches.DisableNativeProviders || AppSwitches.DisableNativeProviderProbing)
            {
                return false;
            }

            bool linearAlgebra = LinearAlgebraControl.TryUseNative();
            bool fourierTransform = FourierTransformControl.TryUseNative();
            bool directSparseSolver = SparseSolverControl.TryUseNative();
            return linearAlgebra || fourierTransform || directSparseSolver;
        }

        public static void FreeResources()
        {
            LinearAlgebraControl.FreeResources();
            FourierTransformControl.FreeResources();
            SparseSolverControl.FreeResources();
        }

        public static void UseSingleThread()
        {
            _maxDegreeOfParallelism = 1;
            ThreadSafeRandomNumberGenerators = false;

            LinearAlgebraControl.Provider.InitializeVerify();
            FourierTransformControl.Provider.InitializeVerify();
            SparseSolverControl.Provider.InitializeVerify();
        }

        public static void UseMultiThreading()
        {
            _maxDegreeOfParallelism = Environment.ProcessorCount;
            ThreadSafeRandomNumberGenerators = true;

            LinearAlgebraControl.Provider.InitializeVerify();
            FourierTransformControl.Provider.InitializeVerify();
            SparseSolverControl.Provider.InitializeVerify();
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
        public static string NativeProviderPath
        {
            get => _nativeProviderHintPath;
            set
            {
                _nativeProviderHintPath = value;
                LinearAlgebraControl.HintPath = value;
                FourierTransformControl.HintPath = value;
                SparseSolverControl.HintPath = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating how many parallel worker threads shall be used
        /// when parallelization is applicable.
        /// </summary>
        /// <remarks>Default to the number of processor cores, must be between 1 and 1024 (inclusive).</remarks>
        public static int MaxDegreeOfParallelism
        {
            get => _maxDegreeOfParallelism;
            set
            {
                _maxDegreeOfParallelism = Math.Max(1, Math.Min(1024, value));

                // Reinitialize providers:
                LinearAlgebraControl.Provider.InitializeVerify();
                FourierTransformControl.Provider.InitializeVerify();
                SparseSolverControl.Provider.InitializeVerify();
            }
        }

        /// <summary>
        /// Gets or sets the TaskScheduler used to schedule the worker tasks.
        /// </summary>
        public static TaskScheduler TaskScheduler { get; set; }

        /// <summary>
        /// Gets or sets the order of the matrix when linear algebra provider
        /// must calculate multiply in parallel threads.
        /// </summary>
        /// <value>The order. Default 64, must be at least 3.</value>
        internal static int ParallelizeOrder
        {
            get => _parallelizeOrder;
            set => _parallelizeOrder = Math.Max(3, value);
        }

        /// <summary>
        /// Gets or sets the number of elements a vector or matrix
        /// must contain before we multiply threads.
        /// </summary>
        /// <value>Number of elements. Default 300, must be at least 3.</value>
        internal static int ParallelizeElements
        {
            get => _parallelizeElements;
            set => _parallelizeElements = Math.Max(3, value);
        }

        public static string Describe()
        {
            var versionAttribute = typeof(Control).GetTypeInfo().Assembly.GetCustomAttribute(typeof(AssemblyInformationalVersionAttribute)) as AssemblyInformationalVersionAttribute;

            var sb = new StringBuilder();
            sb.AppendLine("Math.NET Numerics Configuration:");
            sb.AppendLine($"Version {versionAttribute?.InformationalVersion}");
#if NET5_0
            sb.AppendLine("Built for .NET 5.0");
#elif NET5_0_OR_GREATER
            sb.AppendLine("Built for .NET 5.0+");
#elif NETSTANDARD2_0
            sb.AppendLine("Built for .NET Standard 2.0");
#elif NET48
            sb.AppendLine("Built for .NET Framework 4.8");
#elif NET461
            sb.AppendLine("Built for .NET Framework 4.6.1");
#endif

            sb.AppendLine($"Linear Algebra Provider: {LinearAlgebraControl.Provider}");
            sb.AppendLine($"Fourier Transform Provider: {FourierTransformControl.Provider}");
            sb.AppendLine($"Sparse Solver Provider: {SparseSolverControl.Provider}");
            sb.AppendLine($"Max Degree of Parallelism: {MaxDegreeOfParallelism}");
            sb.AppendLine($"Parallelize Elements: {ParallelizeElements}");
            sb.AppendLine($"Parallelize Order: {ParallelizeOrder}");
            sb.AppendLine($"Check Distribution Parameters: {CheckDistributionParameters}");
            sb.AppendLine($"Thread-Safe RNGs: {ThreadSafeRandomNumberGenerators}");
#if NET461
            sb.AppendLine($"Operating System: {Environment.OSVersion}");
            sb.AppendLine($"Framework: {Environment.Version}");
#else
            // This would also work in .NET 4.0, but we don't want the dependency just for that.
            sb.AppendLine($"Operating System: {RuntimeInformation.OSDescription}");
            sb.AppendLine($"Operating System Architecture: {RuntimeInformation.OSArchitecture}");
            sb.AppendLine($"Framework: {RuntimeInformation.FrameworkDescription}");
            sb.AppendLine($"Process Architecture: {RuntimeInformation.ProcessArchitecture}");
#endif
            var processorArchitecture = Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE");
            if (!string.IsNullOrEmpty(processorArchitecture))
            {
                sb.AppendLine($"Processor Architecture: {processorArchitecture}");
            }
            var processorId = Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER");
            if (!string.IsNullOrEmpty(processorId))
            {
                sb.AppendLine($"Processor Identifier: {processorId}");
            }

            return sb.ToString();
        }
    }
}
