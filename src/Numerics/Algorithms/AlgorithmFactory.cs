// <copyright file="AlgorithmFactory.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://mathnet.opensourcedotnet.info
// Copyright (c) 2009 Math.NET
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
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
using MathNet.Numerics.Algorithms.LinearAlgebra;

namespace MathNet.Numerics.Algorithms
{
    /// <summary>
    /// Enumerates the linear algebra providers.
    /// </summary>
    public enum LinearAlgebraProvider
    {
        /// <summary>
        /// Managed provider
        /// </summary>
        Managed, 

        /// <summary>
        /// User defined provider
        /// </summary>
        UserDefined

/*        /// <summary>
        /// Intel's Math Kernel Library (MKL) Provider
        /// </summary>
        Mkl,

        /// <summary>
        /// Automatically Tuned Linear Algebra Software (ATLAS) Provider
        /// </summary>
        Atlas,

        /// <summary>
        /// AMD Core Math Library (ACML) Provider
        /// </summary>
        Acml,

        /// <summary>
        /// NVIDIA Compute Unified Device Architecture (CUDA) Provider
        /// </summary>
        Cuda,
 */
    }

    /// <summary>
    /// Lets users choose which algorithm providers to use.
    /// </summary>
    public static class AlgorithmFactory
    {
        /// <summary>
        /// The current linear algebra provider - starts as the managed provider.
        /// </summary>
        private static Type _linearAlgebraProvider = typeof(ManagedLinearAlgebra);

        /// <summary>
        /// The type of the user defined linear algebra provider.
        /// </summary>
        private static Type _userLinearAlgebraProvider;

        /// <summary>
        /// Gets the linear algebra provider.
        /// </summary>
        /// <value>The linear algebra provider.</value>
        public static ILinearAlgebra LinearAlgebra
        {
            get { return (ILinearAlgebra) Activator.CreateInstance(_linearAlgebraProvider); }
        }

        /// <summary>
        /// Sets the linear algebra provider to use.
        /// </summary>
        /// <param name="provider">
        /// The linear algebra provider to use.
        /// </param>
        public static void SetLinearAlgebraProvider(LinearAlgebraProvider provider)
        {
            switch (provider)
            {
                case LinearAlgebraProvider.Managed:
                    _linearAlgebraProvider = typeof(ManagedLinearAlgebra);
                    break;
                case LinearAlgebraProvider.UserDefined:
                    if (_userLinearAlgebraProvider == null)
                    {
                        throw new ArgumentException();
                    }

                    _linearAlgebraProvider = _userLinearAlgebraProvider;
                    break;
            }
        }

        /// <summary>
        /// Registers the user defined linear algebra provider.
        /// </summary>
        /// <param name="provider">
        /// The user defined provider.
        /// </param>
        public static void RegisterLinearAlgebraProvider(Type provider)
        {
            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }

            _userLinearAlgebraProvider = provider;
        }
    }
}