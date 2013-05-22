// <copyright file="Control.cs" company="Math.NET">
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

namespace MathNet.Numerics
{
    using Algorithms.LinearAlgebra;
    using System;

    /// <summary>
    /// Sets parameters for the library.
    /// </summary>
    public static class Control
    {
        private static int _numberOfThreads;
        private static int _blockSize;
        private static int _parallelizeOrder;
        private static int _parallelizeElements;

        static Control()
        {
            ConfigureAuto();
        }

        public static void ConfigureAuto()
        {
            // Random Numbers & Distributions
            CheckDistributionParameters = true;
            ThreadSafeRandomNumberGenerators = true;

            // ToString & Formatting
            MaxToStringColumns = 6;
            MaxToStringRows = 8;

            // Parallelization & Threading
            _numberOfThreads = Environment.ProcessorCount;
            DisableParallelization = _numberOfThreads < 2;
            _blockSize = 512;
            _parallelizeOrder = 64;
            _parallelizeElements = 300;

            // Linear Algebra Provider
#if PORTABLE
            LinearAlgebraProvider = new ManagedLinearAlgebraProvider();
#else
            try
            {
                const string name = "MathNetNumericsLAProvider";
                var value = Environment.GetEnvironmentVariable(name);
                switch (value != null ? value.ToUpperInvariant() : string.Empty)
                {
                    case "MKL":
                        LinearAlgebraProvider = new Algorithms.LinearAlgebra.Mkl.MklLinearAlgebraProvider();
                        break;
                    default:
                        LinearAlgebraProvider = new ManagedLinearAlgebraProvider();
                        break;
                }
            }
            catch
            {
                // We don't care about any failures here at all
                LinearAlgebraProvider = new ManagedLinearAlgebraProvider();
            }
#endif
        }

        public static void ConfigureSingleThread()
        {
            _numberOfThreads = 1;
            DisableParallelization = true;
            ThreadSafeRandomNumberGenerators = false;
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
        /// Gets or sets a value indicating whether parallelization shall be disabled globally.
        /// </summary>
        public static bool DisableParallelization { get; set; }

        /// <summary>
        /// Gets or sets the linear algebra provider.
        /// </summary>
        /// <value>The linear algebra provider.</value>
        public static ILinearAlgebraProvider LinearAlgebraProvider { get; set; }

        /// <summary>
        /// Gets or sets a value indicating how many parallel worker threads shall be used
        /// when parallelization is applicable.
        /// </summary>
        /// <remarks>Default to the number of processor cores, must be between 1 and 1024 (inclusive).</remarks>
        public static int NumberOfParallelWorkerThreads
        {
            get { return _numberOfThreads; }
            set { _numberOfThreads = Math.Max(1, Math.Min(1024, value)); }
        }

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
        public static int ParallelizeOrder
        {
            get { return _parallelizeOrder; }
            set { _parallelizeOrder = Math.Max(3, value); }
        }

        /// <summary>
        /// Gets or sets the number of elements a vector or matrix
        /// must contain before we multiply threads.
        /// </summary>
        /// <value>Number of elements. Default 300, must be at least 3.</value>
        public static int ParallelizeElements
        {
            get { return _parallelizeElements; }
            set { _parallelizeElements = Math.Max(3, value); }
        }

        /// <summary>
        /// Given the number elements, should the operation be parallelized.
        /// </summary>
        /// <param name="elements">The number elements to check.</param>
        /// <returns><c>true</c> if the operation should be parallelized; <c>false</c> otherwise.</returns>
        public static bool ParallelizeOperation(int elements)
        {
            return !DisableParallelization && NumberOfParallelWorkerThreads >= 2 && elements >= ParallelizeElements;
        }

        /// <summary>
        /// Maximum number of columns to print in ToString methods by default.
        /// </summary>
        public static int MaxToStringColumns { get; set; }

        /// <summary>
        /// Maximum number of rows to print in ToString methods by default.
        /// </summary>
        public static int MaxToStringRows { get; set; }
    }
}
