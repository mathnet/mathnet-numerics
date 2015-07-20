// <copyright file="CudaException.cs" company="Math.NET">
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MathNet.Numerics.Providers.LinearAlgebra.Cuda
{
    /// <summary>
    /// Exception thrown by the Cuda Runtime API
    /// </summary>
    public class CudaException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CudaException"/> class.
        /// </summary>
        /// <param name="errorCode">The error code returned by the API</param>
        public CudaException(int errorCode)
            : base(CudaException.GetErrorMessage(errorCode))
        {
            this.ErrorCode = errorCode;
        }

        /// <summary>
        /// Gets the error code returned by the Cuda Runtime API.
        /// </summary>
        public int ErrorCode { get; private set; }

        /// <summary>
        /// Gets the error message for a particular error code.
        /// </summary>
        /// <param name="errorCode">The error code returned by the API</param>
        /// <returns>The corresponding error message</returns>
        private static string GetErrorMessage(int errorCode)
        {
            switch (errorCode)
            {
                case 0: // cudaSuccess
                    return "The API call returned with no errors.";

                case 2: // cudaErrorMemoryAllocation
                    return "The API call failed because it was unable to allocate enough memory to perform the requested operation.";

                case 3: // cudaErrorInitializationError
                    return "The API call failed because the CUDA driver and runtime could not be initialized.";

                case 11: // cudaErrorInvalidValue
                    return "This indicates that one or more of the parameters passed to the API call is not within an acceptable range of values.";

                case 17: // cudaErrorInvalidDevicePointer
                    return "This indicates that at least one device pointer passed to the API call is not a valid device pointer. ";

                case 21: // cudaErrorInvalidMemcpyDirection
                    return "This indicates that the direction of the memcpy passed to the API call is not one of the types specified by cudaMemcpyKind. ";

                default:
                    return "Unknown Cuda Runtime error code";
            }
        }
    }
}
