// <copyright file="CuSolverException.cs" company="Math.NET">
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
    /// Exceptions thrown by the cuSolverDn API.
    /// </summary>
    public class CuSolverException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CuSolverException"/> class.
        /// </summary>
        /// <param name="statusCode">The status code returned from the API</param>
        public CuSolverException(int statusCode)
            : base(CuSolverException.GetErrorMessage(statusCode))
        {
            this.StatusCode = statusCode;
        }

        /// <summary>
        /// Gets the status code returned by the cuSolverDn API
        /// </summary>
        public int StatusCode { get; private set; }

        /// <summary>
        /// Returns the appropriate error message for each status code.
        /// </summary>
        /// <param name="code">The status code returned from the API</param>
        /// <returns>The corresponding error message</returns>
        private static string GetErrorMessage(int statusCode)
        {
            switch (statusCode)
            {
                case 0:  // CUSOLVER_STATUS_SUCCESS
                    return "The operation completed successfully.";

                case 1:  // CUSOLVER_STATUS_NOT_INITIALIZED
                    return "The cuSolver library was not initialized. This is usually caused by the lack of a prior call, an error in the CUDA Runtime API called by the cuSolver routine, or an error in the hardware setup.";

                case 2: // CUSOLVER_STATUS_ALLOC_FAILED
                    return "Resource allocation failed inside the cuSolver library. This is usually caused by a cudaMalloc() failure.";

                case 3: // CUSOLVER_STATUS_INVALID_VALUE
                    return "An unsupported value or parameter was passed to the function (a negative vector size, for example).";

                case 4: // CUSOLVER_STATUS_ARCH_MISMATCH
                    return "The function requires a feature absent from the device architecture; usually caused by the lack of support for atomic operations or double precision.";

                case 5: // CUSOLVER_STATUS_MAPPING_ERROR
                    return "Mapping Error";

                case 6: // CUSOLVER_STATUS_EXECUTION_FAILED
                    return "The GPU program failed to execute. This is often caused by a launch failure of the kernel on the GPU, which can be caused by multiple reasons.";

                case 7: //CUSOLVER_STATUS_INTERNAL_ERROR
                    return "An internal cuSolver operation failed. This error is usually caused by a cudaMemcpyAsync() failure.";

                case 8: // CUSOLVER_STATUS_MATRIX_TYPE_NOT_SUPPORTED
                    return "The matrix type is not supported by this function. This is usually caused by passing an invalid matrix descriptor to the function.";

                case 9: // CUSOLVER_STATUS_NOT_SUPPORTED
                    return "The functionality requested is not supported";

                case 10: // CUSOLVER_STATUS_ZERO_PIVOT
                    return "Zero Pivot";

                case 11: //CUSOLVER_STATUS_INVALID_LICENSE
                    return "The functionality requested requires some license and an error was detected when trying to check the current licensing. This error can happen if the license is not present or is expired or if the environment variable NVIDIA_LICENSE_FILE is not set properly.";

                default:
                    throw new Exception("Unrecognized cuSolverDn status code");


            }
        }
    }
}
