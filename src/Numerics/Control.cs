// <copyright file="Control.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://mathnet.opensourcedotnet.info
//
// Copyright (c) 2009 Math.NET
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
    using Threading;

    /// <summary>
    /// Sets parameters for the library.
    /// </summary>
    public static class Control
    {
        /// <summary>
        /// Initializes static members of the Control class.
        /// </summary>
        static Control()
        {
            CheckDistributionParameters = true;
            ThreadSafeRandomNumberGenerators = true;
            DisableParallelization = false;
            InitialThreadBlockSize = 2;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the distribution classes check validate each parameter.
        /// For the multivariate distributions this could involve an expensive matrix factorization.
        /// The default setting of this property is true.
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
        /// Gets or sets a value indicating how many parallel worker threads shall be used
        /// when parallelization is applicable.
        /// </summary>
        public static int NumberOfParallelWorkerThreads
        {
            get { return ThreadQueue.ThreadCount; }
            set { ThreadQueue.Start(value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether parallelization shall be disabled globally.
        /// </summary>
        public static bool DisableParallelization { get; set; }

        /// <summary>
        /// Gets or sets the initial size of a <see cref="Parallel.ForEach{T}"/>
        /// processing block (the number of elements the first thread should process).
        /// </summary>
        /// <value>The initial size of the thread processing bloc.</value>
        public static int InitialThreadBlockSize { get; set; }
    }
}
