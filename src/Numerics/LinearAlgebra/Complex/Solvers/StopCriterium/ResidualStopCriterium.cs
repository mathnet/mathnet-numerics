// <copyright file="ResidualStopCriterium.cs" company="Math.NET">
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

namespace MathNet.Numerics.LinearAlgebra.Complex.Solvers.StopCriterium
{
    using System;
    using System.Diagnostics;
    using Generic.Solvers.Status;
    using Generic.Solvers.StopCriterium;
    using Properties;

    /// <summary>
    /// Defines an <see cref="IIterationStopCriterium"/> that monitors residuals as stop criterium.
    /// </summary>
    public sealed class ResidualStopCriterium : IIterationStopCriterium
    {
        /// <summary>
        /// The default value for the maximum value of the residual.
        /// </summary>
        public const double DefaultMaximumResidual = 1e-12;

        /// <summary>
        /// The default value for the minimum number of iterations.
        /// </summary>
        public const int DefaultMinimumIterationsBelowMaximum = 0;

        /// <summary>
        /// Defines the default last iteration number. Set to -1 because iterations normally start at 0.
        /// </summary>
        private const int DefaultLastIterationNumber = -1;

        /// <summary>
        /// The default status.
        /// </summary>
        private static readonly ICalculationStatus DefaultStatus = new CalculationIndetermined();

        /// <summary>
        /// The maximum value for the residual below which the calculation is considered converged.
        /// </summary>
        private double _maximum;

        /// <summary>
        /// The minimum number of iterations for which the residual has to be below the maximum before
        /// the calculation is considered converged.
        /// </summary>
        private int _minimumIterationsBelowMaximum;

        /// <summary>
        /// The status of the calculation
        /// </summary>
        private ICalculationStatus _status = DefaultStatus;

        /// <summary>
        /// The number of iterations since the residuals got below the maximum.
        /// </summary>
        private int _iterationCount;

        /// <summary>
        /// The iteration number of the last iteration.
        /// </summary>
        private int _lastIteration = DefaultLastIterationNumber;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResidualStopCriterium"/> class with the default maximum 
        /// residual and the default minimum number of iterations.
        /// </summary>
        public ResidualStopCriterium() : this(DefaultMaximumResidual, DefaultMinimumIterationsBelowMaximum)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResidualStopCriterium"/> class with the specified 
        /// maximum residual and the default minimum number of iterations.
        /// </summary>
        /// <param name="maximum">The maximum value for the residual below which the calculation is considered converged.</param>
        public ResidualStopCriterium(double maximum) : this(maximum, DefaultMinimumIterationsBelowMaximum)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResidualStopCriterium"/> class with the default maximum residual
        /// and specified minimum number of iterations.
        /// </summary>
        /// <param name="minimumIterationsBelowMaximum">
        /// The minimum number of iterations for which the residual has to be below the maximum before
        /// the calculation is considered converged.
        /// </param>
        public ResidualStopCriterium(int minimumIterationsBelowMaximum) : this(DefaultMaximumResidual, minimumIterationsBelowMaximum)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResidualStopCriterium"/> class with the specified 
        /// maximum residual and minimum number of iterations.
        /// </summary>
        /// <param name="maximum">
        /// The maximum value for the residual below which the calculation is considered converged.
        /// </param>
        /// <param name="minimumIterationsBelowMaximum">
        /// The minimum number of iterations for which the residual has to be below the maximum before
        /// the calculation is considered converged.
        /// </param>
        public ResidualStopCriterium(double maximum, int minimumIterationsBelowMaximum)
        {
            if (maximum < 0)
            {
                throw new ArgumentOutOfRangeException("maximum");
            }

            if (minimumIterationsBelowMaximum < 0)
            {
                throw new ArgumentOutOfRangeException("minimumIterationsBelowMaximum");
            }

            _maximum = maximum;
            _minimumIterationsBelowMaximum = minimumIterationsBelowMaximum;
        }

        /// <summary>
        /// Gets or sets the maximum value for the residual below which the calculation is considered 
        /// converged.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the <c>Maximum</c> is set to a negative value.</exception>
        public double Maximum
        {
            [DebuggerStepThrough]
            get
            {
                return _maximum;
            }

            [DebuggerStepThrough]
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("value");
                }

                _maximum = value;
            }
        }

        /// <summary>
        /// Returns the maximum residual to the default.
        /// </summary>
        public void ResetMaximumResidualToDefault()
        {
            _maximum = DefaultMaximumResidual;
        }

        /// <summary>
        /// Gets or sets the minimum number of iterations for which the residual has to be
        /// below the maximum before the calculation is considered converged.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the <c>BelowMaximumFor</c> is set to a value less than 1.</exception>
        public int MinimumIterationsBelowMaximum
        {
            [DebuggerStepThrough]
            get
            {
                return _minimumIterationsBelowMaximum;
            }

            [DebuggerStepThrough]
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("value");
                }

                _minimumIterationsBelowMaximum = value;
            }
        }

        /// <summary>
        /// Returns the minimum number of iterations to the default.
        /// </summary>
        public void ResetMinimumIterationsBelowMaximumToDefault()
        {
            _minimumIterationsBelowMaximum = DefaultMinimumIterationsBelowMaximum;
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

            if (solutionVector == null)
            {
                throw new ArgumentNullException("solutionVector");
            }

            if (sourceVector == null)
            {
                throw new ArgumentNullException("sourceVector");
            }
            
            if (residualVector == null)
            {
                throw new ArgumentNullException("residualVector");
            }

            if (solutionVector.Count != sourceVector.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "sourceVector");
            }

            if (solutionVector.Count != residualVector.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "residualVector");
            }

            // Store the infinity norms of both the solution and residual vectors
            // These values will be used to calculate the relative drop in residuals
            // later on.
            var residualNorm = residualVector.Norm(Double.PositiveInfinity);
            
            // Check the residuals by calculating:
            // ||r_i|| <= stop_tol * ||b||
            var stopCriterium = ComputeStopCriterium(sourceVector.Norm(Double.PositiveInfinity).Real);

            // First check that we have real numbers not NaN's.
            // NaN's can occur when the iterative process diverges so we
            // stop if that is the case.
            if (double.IsNaN(stopCriterium) || double.IsNaN(residualNorm.Real))
            {
                _iterationCount = 0;
                SetStatusToDiverged();
                return;
            }

            // ||r_i|| <= stop_tol * ||b||
            // Stop the calculation if it's clearly smaller than the tolerance
            var decimalMagnitude = Math.Abs(stopCriterium.Magnitude()) + 1;
            if (residualNorm.Real.IsSmallerWithDecimalPlaces(stopCriterium, decimalMagnitude))
            {
                if (_lastIteration <= iterationNumber)
                {
                    _iterationCount = iterationNumber - _lastIteration;
                    if (_iterationCount >= _minimumIterationsBelowMaximum)
                    {
                        SetStatusToConverged();
                    }
                    else
                    {
                        SetStatusToRunning();
                    }
                }
            }
            else
            {
                _iterationCount = 0;
                SetStatusToRunning();
            }

            _lastIteration = iterationNumber;
        }

        /// <summary>
        /// Calculate stop criterium
        /// </summary>
        /// <param name="solutionNorm">Solution vector norm</param>
        /// <returns>Criterium value</returns>
        private double ComputeStopCriterium(double solutionNorm)
        {
            // This is criterium 1 from Templates for the solution of linear systems.
            // The problem with this criterium is that it's not limiting enough. For now 
            // we won't use it. Later on we might get back to it.
            // return mMaximumResidual * (System.Math.Abs(mMatrixNorm) * System.Math.Abs(solutionNorm) + System.Math.Abs(mVectorNorm));

            // For now use criterium 2 from Templates for the solution of linear systems. See page 60.
            return _maximum * Math.Abs(solutionNorm);
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
        /// Set status to <see cref="CalculationConverged"/>
        /// </summary>
        private void SetStatusToConverged()
        {
            if (!(_status is CalculationConverged))
            {
                _status = new CalculationConverged();
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
            _iterationCount = 0;
            _lastIteration = DefaultLastIterationNumber;
        }

        /// <summary>
        /// Gets the <see cref="StopLevel"/> which indicates what sort of stop criterium this
        /// <see cref="IIterationStopCriterium"/> monitors.
        /// </summary>
        /// <value>Returns <see cref="StopLevel"/>.</value>
        public StopLevel StopLevel
        {
            [DebuggerStepThrough]
            get
            {
                return StopLevel.Convergence;
            }
        }

        /// <summary>
        /// Clones the current <see cref="ResidualStopCriterium"/> and its settings.
        /// </summary>
        /// <returns>A new instance of the <see cref="ResidualStopCriterium"/> class.</returns>
        public IIterationStopCriterium Clone()
        {
            return new ResidualStopCriterium(_maximum, _minimumIterationsBelowMaximum);
        }

#if !PORTABLE
        /// <summary>
        /// Clones the current <see cref="ResidualStopCriterium"/> and its settings.
        /// </summary>
        /// <returns>A new instance of the <see cref="ResidualStopCriterium"/> object.</returns>
        object ICloneable.Clone()
        {
            return Clone();
        }
#endif
    }
}
