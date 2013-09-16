// <copyright file="FailureStopCriterium.cs" company="Math.NET">
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

using System;
using System.Diagnostics;
using MathNet.Numerics.LinearAlgebra.Solvers;
using MathNet.Numerics.Properties;

namespace MathNet.Numerics.LinearAlgebra.Complex32.Solvers.StopCriterium
{
    using Numerics;

    /// <summary>
    /// Defines an <see cref="IIterationStopCriterium{T}"/> that monitors residuals for NaN's.
    /// </summary>
    public sealed class FailureStopCriterium : IIterationStopCriterium<Complex32>
    {
        /// <summary>
        /// Defines the default last iteration number. Set to -1 because iterations normally
        /// start at 0.
        /// </summary>
        const int DefaultLastIterationNumber = -1;

        /// <summary>
        /// The status of the calculation
        /// </summary>
        IterationStatus _status = IterationStatus.Indetermined;

        /// <summary>
        /// The iteration number of the last iteration.
        /// </summary>
        int _lastIteration = DefaultLastIterationNumber;

        /// <summary>
        /// Determines the status of the iterative calculation based on the stop criteria stored
        /// by the current <see cref="IIterationStopCriterium{T}"/>. Result is set into <c>Status</c> field.
        /// </summary>
        /// <param name="iterationNumber">The number of iterations that have passed so far.</param>
        /// <param name="solutionVector">The vector containing the current solution values.</param>
        /// <param name="sourceVector">The right hand side vector.</param>
        /// <param name="residualVector">The vector containing the current residual vectors.</param>
        /// <remarks>
        /// The individual stop criteria may internally track the progress of the calculation based
        /// on the invocation of this method. Therefore this method should only be called if the 
        /// calculation has moved forwards at least one step.
        /// </remarks>
        public IterationStatus DetermineStatus(int iterationNumber, Vector<Complex32> solutionVector, Vector<Complex32> sourceVector, Vector<Complex32> residualVector)
        {
            if (iterationNumber < 0)
            {
                throw new ArgumentOutOfRangeException("iterationNumber");
            }

            if (solutionVector.Count != residualVector.Count)
            {
                throw new ArgumentException(Resources.ArgumentArraysSameLength);
            }

            if (_lastIteration >= iterationNumber)
            {
                // We have already stored the actual last iteration number
                // For now do nothing. We only care about the next step.
                return _status;
            }

            // Store the infinity norms of both the solution and residual vectors
            var residualNorm = residualVector.InfinityNorm();
            var solutionNorm = solutionVector.InfinityNorm();

            _status = float.IsNaN(solutionNorm.Real) || float.IsNaN(residualNorm.Real) ? IterationStatus.Failure : IterationStatus.Running;

            _lastIteration = iterationNumber;
            return _status;
        }

        /// <summary>
        /// Gets the current calculation status.
        /// </summary>
        public IterationStatus Status
        {
            [DebuggerStepThrough]
            get
            {
                return _status;
            }
        }

        /// <summary>
        /// Resets the <see cref="IIterationStopCriterium{T}"/> to the pre-calculation state.
        /// </summary>
        public void Reset()
        {
            _status = IterationStatus.Indetermined;
            _lastIteration = DefaultLastIterationNumber;
        }

        /// <summary>
        /// Clones the current <see cref="FailureStopCriterium"/> and its settings.
        /// </summary>
        /// <returns>A new instance of the <see cref="FailureStopCriterium"/> class.</returns>
        public IIterationStopCriterium<Complex32> Clone()
        {
            return new FailureStopCriterium();
        }
    }
}
