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

namespace MathNet.Numerics.LinearAlgebra.Double.Solvers.StopCriterium
{
    using System;
    using System.Diagnostics;
    using Generic.Solvers.Status;
    using Generic.Solvers.StopCriterium;
    using Properties;

    /// <summary>
    /// Defines an <see cref="IIterationStopCriterium"/> that monitors residuals for NaN's.
    /// </summary>
    public sealed class FailureStopCriterium : IIterationStopCriterium
    {
        /// <summary>
        /// Defines the default last iteration number. Set to -1 because iterations normally
        /// start at 0.
        /// </summary>
        private const int DefaultLastIterationNumber = -1;

        /// <summary>
        /// The default status.
        /// </summary>
        private static readonly ICalculationStatus DefaultStatus = new CalculationIndetermined();

        /// <summary>
        /// The status of the calculation
        /// </summary>
        private ICalculationStatus _status = DefaultStatus;

        /// <summary>
        /// The iteration number of the last iteration.
        /// </summary>
        private int _lastIteration = DefaultLastIterationNumber;

        /// <summary>
        /// Determines the status of the iterative calculation based on the stop criteria stored
        /// by the current <see cref="IIterationStopCriterium"/>. Result is set into <c>Status</c> field.
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
        public void DetermineStatus(int iterationNumber, Vector solutionVector, Vector sourceVector, Vector residualVector)
        {
            if (iterationNumber < 0)
            {
                throw new ArgumentOutOfRangeException("iterationNumber");
            }

            if (solutionVector == null)
            {
                throw new ArgumentNullException("solutionVector");
            }

            if (residualVector == null)
            {
                throw new ArgumentNullException("residualVector");
            }

            if (solutionVector.Count != residualVector.Count)
            {
                throw new ArgumentException(Resources.ArgumentArraysSameLength);
            }

            if (_lastIteration >= iterationNumber)
            {
                // We have already stored the actual last iteration number
                // For now do nothing. We only care about the next step.
                return;
            }

            // Store the infinity norms of both the solution and residual vectors
            var residualNorm = residualVector.Norm(Double.PositiveInfinity);
            var solutionNorm = solutionVector.Norm(Double.PositiveInfinity);

            if (Double.IsNaN(solutionNorm) || Double.IsNaN(residualNorm))
            {
                SetStatusToFailed();
            }
            else
            {
                SetStatusToRunning();
            }

            _lastIteration = iterationNumber;
        }

        /// <summary>
        /// Set status to <see cref="CalculationFailure"/>
        /// </summary>
        private void SetStatusToFailed()
        {
            if (!(_status is CalculationFailure))
            {
                _status = new CalculationFailure();
            }
        }

        /// <summary>
        /// Set status to <see cref="CalculationRunning"/>
        /// </summary>
        private void SetStatusToRunning()
        {
            if (!(_status is CalculationRunning))
            {
                _status = new CalculationRunning();
            }
        }

        /// <summary>
        /// Gets the current calculation status.
        /// </summary>
        public ICalculationStatus Status
        {
            [DebuggerStepThrough]
            get
            {
                return _status;
            }
        }

        /// <summary>
        /// Resets the <see cref="IIterationStopCriterium"/> to the pre-calculation state.
        /// </summary>
        public void ResetToPrecalculationState()
        {
            _status = DefaultStatus;
            _lastIteration = DefaultLastIterationNumber;
        }

        /// <summary>
        /// Gets the <see cref="StopLevel"/>which indicates what sort of stop criterium this
        /// <see cref="IIterationStopCriterium"/> monitors.
        /// </summary>
        /// <value>Returns <see cref="CalculationFailure"/>.</value>
        public StopLevel StopLevel
        {
            [DebuggerStepThrough]
            get
            {
                return StopLevel.CalculationFailure;
            }
        }

        /// <summary>
        /// Clones the current <see cref="FailureStopCriterium"/> and its settings.
        /// </summary>
        /// <returns>A new instance of the <see cref="FailureStopCriterium"/> class.</returns>
        public IIterationStopCriterium Clone()
        {
            return new FailureStopCriterium();
        }

#if !PORTABLE
        /// <summary>
        /// Clones the current <see cref="FailureStopCriterium"/> and its settings.
        /// </summary>
        /// <returns>A new instance of the <see cref="FailureStopCriterium"/> class.</returns>
        object ICloneable.Clone()
        {
            return Clone();
        }
#endif
    }
}
