// <copyright file="DivergenceStopCriterium.cs" company="Math.NET">
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

namespace MathNet.Numerics.LinearAlgebra.Complex32.Solvers.StopCriterium
{
    using System;
    using System.Diagnostics;
    using Generic.Solvers.Status;
    using Generic.Solvers.StopCriterium;

    /// <summary>
    /// Monitors an iterative calculation for signs of divergence.
    /// </summary>
    public sealed class DivergenceStopCriterium : IIterationStopCriterium
    {
        /// <summary>
        /// Default value for the maximum relative increase that the 
        /// residual may experience before a divergence warning is issued.
        /// </summary>
        public const double DefaultMaximumRelativeIncrease = 0.08;
        
        /// <summary>
        /// Default value for the minimum number of iterations over which 
        /// the residual must grow before a divergence warning is issued.
        /// </summary>
        public const int DefaultMinimumNumberOfIterations = 10;

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
        /// The maximum relative increase the residual may experience without triggering a divergence warning.
        /// </summary>
        private double _maximumRelativeIncrease;

        /// <summary>
        /// The number of iterations over which a residual increase should be tracked before issuing a divergence warning.
        /// </summary>
        private int _minimumNumberOfIterations;

        /// <summary>
        /// The status of the calculation
        /// </summary>
        private ICalculationStatus _status = DefaultStatus;

        /// <summary>
        /// The array that holds the tracking information.
        /// </summary>
        private double[] _residualHistory;

        /// <summary>
        /// The iteration number of the last iteration.
        /// </summary>
        private int _lastIteration = DefaultLastIterationNumber;

        /// <summary>
        /// Initializes a new instance of the <see cref="DivergenceStopCriterium"/> class with the default maximum 
        /// relative increase and the default minimum number of tracking iterations.
        /// </summary>
        public DivergenceStopCriterium() : this(DefaultMaximumRelativeIncrease, DefaultMinimumNumberOfIterations)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DivergenceStopCriterium"/> class with the specified maximum 
        /// relative increase and the default minimum number of tracking iterations.
        /// </summary>
        /// <param name="maximumRelativeIncrease">The maximum relative increase that the residual may experience before a divergence warning is issued.</param>
        public DivergenceStopCriterium(double maximumRelativeIncrease) : this(maximumRelativeIncrease, DefaultMinimumNumberOfIterations)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DivergenceStopCriterium"/> class with the default maximum 
        /// relative increase and the specified minimum number of tracking iterations.
        /// </summary>
        /// <param name="minimumIterations">The minimum number of iterations over which the residual must grow before a divergence warning is issued. </param>
        public DivergenceStopCriterium(int minimumIterations) : this(DefaultMinimumNumberOfIterations,  minimumIterations)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DivergenceStopCriterium"/> class with the specified maximum 
        /// relative increase and the specified minimum number of tracking iterations.
        /// </summary>
        /// <param name="maximumRelativeIncrease">The maximum relative increase that the residual may experience before a divergence warning is issued. </param>
        /// <param name="minimumIterations">The minimum number of iterations over which the residual must grow before a divergence warning is issued.</param>
        public DivergenceStopCriterium(double maximumRelativeIncrease, int minimumIterations)
        {
            if (maximumRelativeIncrease <= 0)
            {
                throw new ArgumentOutOfRangeException("maximumRelativeIncrease");
            }

            // There must be at least three iterations otherwise we can't calculate the relative increase
            if (minimumIterations < 3)
            {
                throw new ArgumentOutOfRangeException("minimumIterations");
            }

            _maximumRelativeIncrease = maximumRelativeIncrease;
            _minimumNumberOfIterations = minimumIterations;
        }

        /// <summary>
        /// Gets or sets the maximum relative increase that the residual may experience before a divergence warning is issued.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the <c>Maximum</c> is set to zero or below.</exception>
        public double MaximumRelativeIncrease
        {
            [DebuggerStepThrough]
            get
            {
                return _maximumRelativeIncrease;
            }

            [DebuggerStepThrough]
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException("value");
                }

                _maximumRelativeIncrease = value;
            }
        }

        /// <summary>
        /// Returns the maximum relative increase to the default.
        /// </summary>
        public void ResetMaximumRelativeIncreaseToDefault()
        {
            _maximumRelativeIncrease = DefaultMaximumRelativeIncrease;
        }

        /// <summary>
        /// Gets or sets the minimum number of iterations over which the residual must grow before
        /// issuing a divergence warning.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the <c>value</c> is set to less than one.</exception>
        public int MinimumNumberOfIterations
        {
            [DebuggerStepThrough]
            get
            {
                return _minimumNumberOfIterations;
            }

            [DebuggerStepThrough]
            set
            {
                // There must be at least three iterations otherwise we can't calculate
                // the relative increase
                if (value < 3)
                {
                    throw new ArgumentOutOfRangeException("value");
                }

                _minimumNumberOfIterations = value;
            }
        }

        /// <summary>
        /// Returns the minimum number of iterations to the default.
        /// </summary>
        public void ResetNumberOfIterationsToDefault()
        {
            _minimumNumberOfIterations = DefaultMinimumNumberOfIterations;
        }

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

            if (residualVector == null)
            {
                throw new ArgumentNullException("residualVector");
            }

            if (_lastIteration >= iterationNumber)
            {
                // We have already stored the actual last iteration number
                // For now do nothing. We only care about the next step.
                return;
            }

            if ((_residualHistory == null) || (_residualHistory.Length != RequiredHistoryLength))
            {
                _residualHistory = new double[RequiredHistoryLength];
            }

            // We always track the residual.
            // Move the old versions one element up in the array.
            for (var i = 1; i < _residualHistory.Length; i++)
            {
                _residualHistory[i - 1] = _residualHistory[i];
            }

            // Store the infinity norms of both the solution and residual vectors
            // These values will be used to calculate the relative drop in residuals later on.
            _residualHistory[_residualHistory.Length - 1] = residualVector.Norm(Double.PositiveInfinity).Real;

            // Check if we have NaN's. If so we've gone way beyond normal divergence.
            // Stop the iteration.
            if (double.IsNaN(_residualHistory[_residualHistory.Length - 1]))
            {
                SetStatusToDiverged();
                return;
            }

            // Check if we are diverging and if so set the status
            if (IsDiverging())
            {
                SetStatusToDiverged();
            }
            else
            {
                SetStatusToRunning();
            }

            _lastIteration = iterationNumber;
        }

        /// <summary>
        /// Detect if solution is diverging
        /// </summary>
        /// <returns><c>true</c> if diverging, otherwise <c>false</c></returns>
        private bool IsDiverging()
        {
            // Run for each variable
            for (var i = 1; i < _residualHistory.Length; i++)
            {
                var difference = _residualHistory[i] - _residualHistory[i - 1];

                // Divergence is occurring if:
                // - the last residual is larger than the previous one
                // - the relative increase of the residual is larger than the setting allows
                if ((difference < 0) || (_residualHistory[i - 1] * (1 + _maximumRelativeIncrease) >= _residualHistory[i]))
                {
                    // No divergence taking place within the required number of iterations
                    // So reset and stop the iteration. There is no way we can get to the
                    // required number of iterations anymore.
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Gets required history Length
        /// </summary>
        private int RequiredHistoryLength
        {
            [DebuggerStepThrough]
            get
            {
                return _minimumNumberOfIterations + 1;
            }
        }

        /// <summary>
        /// Set status to <see cref="CalculationDiverged"/>
        /// </summary>
        private void SetStatusToDiverged()
        {
            if (!(_status is CalculationDiverged))
            {
                _status = new CalculationDiverged();
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
            _residualHistory = null;
        }

        /// <summary>
        /// Gets the <see cref="StopLevel"/> which indicates what sort of stop criterium this
        /// <see cref="IIterationStopCriterium"/> monitors.
        /// </summary>
        /// <value>Returns <see cref="Generic.Solvers.StopCriterium.StopLevel.Divergence"/>.</value>
        public StopLevel StopLevel
        {
            [DebuggerStepThrough]
            get
            {
                return StopLevel.Divergence;
            }
        }

        /// <summary>
        /// Clones the current <see cref="DivergenceStopCriterium"/> and its settings.
        /// </summary>
        /// <returns>A new instance of the <see cref="DivergenceStopCriterium"/> class.</returns>
        public IIterationStopCriterium Clone()
        {
            return new DivergenceStopCriterium(_maximumRelativeIncrease, _minimumNumberOfIterations);
        }

#if !PORTABLE
        /// <summary>
        /// Clone this object
        /// </summary>
        /// <returns>Object clone</returns>
        object ICloneable.Clone()
        {
            return Clone();
        }
#endif
    }
}
