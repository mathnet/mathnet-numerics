// <copyright file="IterationCountStopCriterium.cs" company="Math.NET">
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

    /// <summary>
    /// Defines an <see cref="IIterationStopCriterium"/> that monitors the numbers of iteration 
    /// steps as stop criterium.
    /// </summary>
    public sealed class IterationCountStopCriterium : IIterationStopCriterium
    {
        /// <summary>
        /// The default value for the maximum number of iterations the process is allowed
        /// to perform.
        /// </summary>
        public const int DefaultMaximumNumberOfIterations = 1000;

        /// <summary>
        /// The default status.
        /// </summary>
        private static readonly ICalculationStatus DefaultStatus = new CalculationIndetermined();

        /// <summary>
        /// The maximum number of iterations the calculation is allowed to perform.
        /// </summary>
        private int _maximumNumberOfIterations;

        /// <summary>
        /// The status of the calculation
        /// </summary>
        private ICalculationStatus _status = DefaultStatus;

        /// <summary>
        /// Initializes a new instance of the <see cref="IterationCountStopCriterium"/> class with the default maximum 
        /// number of iterations.
        /// </summary>
        public IterationCountStopCriterium() : this(DefaultMaximumNumberOfIterations)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IterationCountStopCriterium"/> class with the specified maximum
        /// number of iterations.
        /// </summary>
        /// <param name="maximumNumberOfIterations">The maximum number of iterations the calculation is allowed to perform.</param>
        public IterationCountStopCriterium(int maximumNumberOfIterations)
        {
            if (maximumNumberOfIterations < 1)
            {
                throw new ArgumentOutOfRangeException("maximumNumberOfIterations");
            }

            _maximumNumberOfIterations = maximumNumberOfIterations;
        }

        /// <summary>
        /// Gets or sets the maximum number of iterations the calculation is allowed to perform.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the <c>Maximum</c> is set to a negative value.</exception>
        public int MaximumNumberOfIterations
        {
            [DebuggerStepThrough]
            get
            {
                return _maximumNumberOfIterations;
            }

            [DebuggerStepThrough]
            set
            {
                if (value < 1)
                {
                    throw new ArgumentOutOfRangeException("value");
                }

                _maximumNumberOfIterations = value;
            }
        }

        /// <summary>
        /// Returns the maximum number of iterations to the default.
        /// </summary>
        public void ResetMaximumNumberOfIterationsToDefault()
        {
            _maximumNumberOfIterations = DefaultMaximumNumberOfIterations;
        }

        /// <summary>
        /// Determines the status of the iterative calculation based on the stop criteria stored
        /// by the current <see cref="IterationCountStopCriterium"/>. Result is set into <c>Status</c> field.
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

            if (iterationNumber >= _maximumNumberOfIterations)
            {
                SetStatusToFinished();
            }
            else
            {
                SetStatusToRunning();
            }
        }

        /// <summary>
        /// Set status to <see cref="CalculationFailure"/>
        /// </summary>
        private void SetStatusToFinished()
        {
            if (!(_status is CalculationStoppedWithoutConvergence))
            {
                _status = new CalculationStoppedWithoutConvergence();
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
        /// Resets the <see cref="IterationCountStopCriterium"/> to the pre-calculation state.
        /// </summary>
        public void ResetToPrecalculationState()
        {
            _status = DefaultStatus;
        }

        /// <summary>
        /// Gets the <see cref="StopLevel"/> which indicates what sort of stop criterium this
        /// <see cref="IterationCountStopCriterium"/> monitors.
        /// </summary>
        /// <value>Returns <see cref="StopLevel"/>.</value>
        public StopLevel StopLevel
        {
            [DebuggerStepThrough]
            get
            {
                return StopLevel.StoppedWithoutConvergence;
            }
        }

        /// <summary>
        /// Clones the current <see cref="IterationCountStopCriterium"/> and its settings.
        /// </summary>
        /// <returns>A new instance of the <see cref="IterationCountStopCriterium"/> class.</returns>
        public IIterationStopCriterium Clone()
        {
            return new IterationCountStopCriterium(_maximumNumberOfIterations);
        }

#if !PORTABLE
        /// <summary>
        /// Clones the current <see cref="IterationCountStopCriterium"/> and its settings.
        /// </summary>
        /// <returns>A new instance of the <see cref="IterationCountStopCriterium"/> object.</returns>
        object ICloneable.Clone()
        {
            return Clone();
        }
#endif
    }
}
