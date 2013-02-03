// <copyright file="Control.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
//
// Copyright (c) 2009-2010 Math.NET
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
    using System;
    using Algorithms.LinearAlgebra;

    /// <summary>
    /// Sets parameters for the library.
    /// </summary>
    public static class Control
    {
        /// <summary>
        /// Initial number of threads to use;
        /// </summary>
        private static int _numberOfThreads = Environment.ProcessorCount;

        /// <summary>
        /// Initial block size for the native linear algebra provider.
        /// </summary>
        private static int _blockSize = 512;

        /// <summary>
        /// Initial parallel order size for the matrix multiply in linear algebra provider.
        /// </summary>
        private static int _parallelizeOrder = 64;

        /// <summary>
        /// The default cutoff point for order size for the matrix multiply in linear algebra provider.
        /// </summary>
        private static int _parallelizeElements = 300;

        /// <summary>
        /// Initializes static members of the Control class.
        /// </summary>
        static Control()
        {
            CheckDistributionParameters = true;
            ThreadSafeRandomNumberGenerators = true;
            DisableParallelization = false;

            #if PORTABLE
            LinearAlgebraProvider = new ManagedLinearAlgebraProvider();
            #else
            try
            {
                const string name = "MathNetNumericsLAProvider";
                var value = Environment.GetEnvironmentVariable(name);
                switch (value != null ? value.ToUpper() : string.Empty)
                {
                    case "MKL":
                        LinearAlgebraProvider = new MathNet.Numerics.Algorithms.LinearAlgebra.Mkl.MklLinearAlgebraProvider();
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
        public static ILinearAlgebraProvider LinearAlgebraProvider
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating how many parallel worker threads shall be used
        /// when parallelization is applicable.
        /// </summary>
        /// <remarks>The Silverlight version of the library defaults to one thread.</remarks>
        public static int NumberOfParallelWorkerThreads
        {
            get { return _numberOfThreads; }
            set
            {   // instead of throwing an out of range exception, simply normalize
                _numberOfThreads = Math.Max(1, Math.Min(1024, value));
            }
        }

        /// <summary>
        /// Gets or sets the the block size to use for the native linear
        /// algebra provider.
        /// </summary>
        /// <value>The block size. Must be at least 32.</value>
        public static int BlockSize
        {
            get { return _blockSize; }
            set
            {
                if (_blockSize > 31)
                {
                    _blockSize = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the order of the matrix when linear algebra provider must calculate multiply in parallel threads.
        /// </summary>
        /// <value>The order. Default is 64.</value>
        public static int ParallelizeOrder
        {
            get { return _parallelizeOrder; }
            set
            {
                if (_parallelizeOrder > 2)
                {
                    _parallelizeOrder = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the number of elements a vector or matrix must contain before we multiply threads.
        /// </summary>
        /// <value>Number of elements. Default is 300.</value>
        public static int ParallelizeElements
        {
            get { return _parallelizeElements; }
            set
            {
                if (_parallelizeElements > 2)
                {
                    _parallelizeElements = value;
                }
            }
        }

        /// <summary>
        /// Given the number elements, should the operation be parallelized.
        /// </summary>
        /// <param name="elements">The number elements to check.</param>
        /// <returns><c>true</c> if the operation should be parallelized; <c>false</c> otherwise.</returns>
        public static bool ParallelizeOperation(int elements)
        {
            return elements < ParallelizeElements || DisableParallelization || NumberOfParallelWorkerThreads < 2;
        }
    }
}
