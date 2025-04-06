// <copyright file="BasinHopping.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// https://numerics.mathdotnet.com
// https://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-$CURRENT_YEAR$ Math.NET
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

using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MathNet.Numerics.Optimization
{
    using Random = System.Random;

    /// <summary>
    /// Indicates the result of an acceptance test in the basin-hopping algorithm.
    /// </summary>
    public enum AcceptanceStatus
    {
        /// <summary>
        /// The trial point is accepted based on regular acceptance criteria.
        /// </summary>
        Accept,

        /// <summary>
        /// The trial point is rejected.
        /// </summary>
        Reject,

        /// <summary>
        /// The trial point is accepted unconditionally, bypassing regular acceptance criteria.
        /// </summary>
        ForceAccept
    }

    /// <summary>
    /// Convergence criteria for the optimization algorithm
    /// </summary>
    internal class GeneralConvergence
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GeneralConvergence"/> class.
        /// </summary>
        /// <param name="numberOfVariables">The number of variables in the optimization problem.</param>
        public GeneralConvergence(int numberOfVariables)
        {
            AbsoluteParameterTolerance = new double[numberOfVariables];
            for (var i = 0; i < numberOfVariables; i++)
            {
                AbsoluteParameterTolerance[i] = 1e-8;
            }

            MaximumEvaluations = 0;
            MaximumTime = TimeSpan.FromMinutes(10);
            StartTime = DateTime.Now;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to cancel the optimization.
        /// </summary>
        public bool Cancel { get; set; }

        /// <summary>
        /// Gets or sets the absolute parameter tolerance for each dimension.
        /// </summary>
        public double[] AbsoluteParameterTolerance { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of function evaluations.
        /// </summary>
        public int MaximumEvaluations { get; set; } = 1000;

        /// <summary>
        /// Gets or sets the maximum time allowed for optimization.
        /// </summary>
        public TimeSpan MaximumTime { get; set; } = TimeSpan.FromMinutes(5);

        /// <summary>
        /// Gets or sets the start time of the optimization.
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Gets or sets the number of function evaluations performed.
        /// </summary>
        public int Evaluations { get; set; }
    }

    /// <summary>
    /// Implements the basin-hopping algorithm to find the global minimum of a function
    /// </summary>
    public sealed class BasinHopping : NonlinearMinimizerBase, ILeastSquaresMinimizer, IUnconstrainedMinimizer
    {
        /// <summary>
        /// Delegate for taking a random step from the current position
        /// </summary>
        /// <param name="x">Current position</param>
        /// <returns>New position after random displacement</returns>
        public delegate Vector<double> TakeStepDelegate(Vector<double> x);

        /// <summary>
        /// Delegate for testing whether to accept a step
        /// </summary>
        /// <param name="newValue">Function value after step</param>
        /// <param name="oldValue">Function value before step</param>
        /// <returns>Status indicating whether to accept the step</returns>
        public delegate AcceptanceStatus AcceptTestDelegate(double newValue, double oldValue);

        /// <summary>
        /// Delegate for callback function called for each minimum found
        /// </summary>
        /// <param name="x">Coordinates of the trial minimum</param>
        /// <param name="f">Function value at the trial minimum</param>
        /// <param name="accepted">Whether the minimum was accepted</param>
        /// <returns>True to stop the algorithm, false to continue</returns>
        public delegate bool CallbackDelegate(Vector<double> x, double f, bool accepted);

        // Internal state variables
        private Vector<double> solution;
        private double functionValue;
        private int iterations;
        private int evaluations;
        private BasinHoppingStatus status;
        private GeneralConvergence convergence;
        private double temperature;
        private double stepSize;
        private int interval;
        private double targetAcceptRate;
        private double stepwiseFactor;
        private int maxIterations;
        private int? successIterations;
        private TakeStepDelegate takeStep;
        private AcceptTestDelegate acceptTest;
        private CallbackDelegate callback;
        private Random random;

        // local minimizer
        private ILeastSquaresMinimizer leastSquaresMinimizer;
        private IUnconstrainedMinimizer unconstrainedMinimizer;

        private List<bool> isFixed;

        // Adaptive step size variables
        private int stepCount;
        private int acceptCount;

        /// <summary>
        /// Gets or sets the "temperature" parameter used in the Metropolis acceptance criterion.
        /// </summary>
        public double Temperature
        {
            get => temperature;
            set => temperature = value;
        }

        /// <summary>
        /// Gets or sets the maximum step size for the random displacement.
        /// </summary>
        public double StepSize
        {
            get => stepSize;
            set => stepSize = value;
        }

        /// <summary>
        /// Gets or sets the interval for step size updates.
        /// </summary>
        public int Interval
        {
            get => interval;
            set => interval = value;
        }

        /// <summary>
        /// Gets or sets the target acceptance rate for step size adjustment.
        /// </summary>
        public double TargetAcceptRate
        {
            get => targetAcceptRate;
            set => targetAcceptRate = value;
        }

        /// <summary>
        /// Gets or sets the factor for step size adjustment.
        /// </summary>
        public double StepwiseFactor
        {
            get => stepwiseFactor;
            set => stepwiseFactor = value;
        }

        #region Using a least squares minimizer

        /// <summary>
        /// Initializes a new instance of the BasinHopping class using a least squares minimizer.
        /// </summary>
        /// <param name="localMinimizer">Local least squares minimizer to use at each step.</param>
        /// <param name="temperature">The "temperature" parameter for the acceptance criterion.</param>
        /// <param name="stepSize">Maximum step size for the random displacement.</param>
        /// <param name="maxIterations">Maximum number of basin-hopping iterations.</param>
        /// <param name="successIterations">Stop if global minimum candidate remains the same for this many iterations.</param>
        /// <param name="interval">Interval for step size updates.</param>
        /// <param name="targetAcceptRate">Target acceptance rate for step size adjustment.</param>
        /// <param name="stepwiseFactor">Factor for step size adjustment.</param>
        public BasinHopping(
            ILeastSquaresMinimizer localMinimizer,
            double temperature = 1.0,
            double stepSize = 0.5,
            int maxIterations = 100,
            int? successIterations = null,
            int interval = 50,
            double targetAcceptRate = 0.5,
            double stepwiseFactor = 0.9)
            : base(gradientTolerance: 1E-8, stepTolerance: 1E-8, functionTolerance: 1E-8)
        {
            this.temperature = temperature;
            this.stepSize = stepSize;
            this.leastSquaresMinimizer = localMinimizer;
            this.maxIterations = maxIterations;
            this.successIterations = successIterations;
            this.interval = interval;
            this.targetAcceptRate = targetAcceptRate;
            this.stepwiseFactor = stepwiseFactor;
            random = new Random();
            convergence = new GeneralConvergence(1);

            // Create default step-taking function
            takeStep = RandomDisplacement;
        }

        /// <inheritdoc/>
        public NonlinearMinimizationResult FindMinimum(IObjectiveModel objective, Vector<double> initialGuess,
            Vector<double> lowerBound = null, Vector<double> upperBound = null, Vector<double> scales = null, List<bool> isFixed = null)
        {
            if (objective == null)
            {
                throw new ArgumentNullException(nameof(objective));
            }

            if (initialGuess == null)
            {
                throw new ArgumentNullException(nameof(initialGuess));
            }

            if (leastSquaresMinimizer == null)
            {
                throw new ArgumentNullException(nameof(leastSquaresMinimizer),
                    "A local minimizer must be provided for basin-hopping optimization.");
            }

            // Use default step function and acceptance test if not set
            if (takeStep == null)
            {
                takeStep = RandomDisplacement;
            }

            if (acceptTest == null)
            {
                acceptTest = MetropolisAcceptance;
            }

            // Proceed with the basin-hopping optimization ignoring additional bounds parameters.
            return Optimize(objective, initialGuess, lowerBound, upperBound, scales, isFixed);
        }

        /// <inheritdoc/>
        public NonlinearMinimizationResult FindMinimum(IObjectiveModel objective, double[] initialGuess,
            double[] lowerBound = null, double[] upperBound = null, double[] scales = null, bool[] isFixed = null)
        {
            if (objective == null)
            {
                throw new ArgumentNullException(nameof(objective));
            }

            if (initialGuess == null)
            {
                throw new ArgumentNullException(nameof(initialGuess));
            }

            var vecInitial = Vector<double>.Build.DenseOfArray(initialGuess);
            var vecLower = lowerBound != null ? Vector<double>.Build.DenseOfArray(lowerBound) : null;
            var vecUpper = upperBound != null ? Vector<double>.Build.DenseOfArray(upperBound) : null;
            var vecScales = scales != null ? Vector<double>.Build.DenseOfArray(scales) : null;
            var listIsFixed = isFixed != null ? isFixed.ToList() : null;

            // Call the vector-based overload (bounds parameters are ignored).
            return FindMinimum(objective, vecInitial, vecLower, vecUpper, vecScales, listIsFixed);
        }

        /// <summary>
        /// Core optimization logic for the basin-hopping algorithm.
        /// </summary>
        private NonlinearMinimizationResult Optimize(IObjectiveModel objectiveModel, Vector<double> initialGuess,
            Vector<double> lowerBound = null, Vector<double> upperBound = null, Vector<double> scales = null, List<bool> isFixed = null)
        {
            // Validate bounds first
            ValidateBounds(initialGuess, lowerBound, upperBound, scales);

            this.isFixed = isFixed;

            // Set up tracking variables
            convergence.StartTime = DateTime.Now;
            convergence.Evaluations = 0;
            evaluations = 0;
            iterations = 0;
            stepCount = 0;
            acceptCount = 0;
            status = BasinHoppingStatus.IterationsCompleted;

            // Perform initial minimization
            var currentResult = leastSquaresMinimizer.FindMinimum(objectiveModel.Fork(), initialGuess, lowerBound, upperBound, scales, isFixed);
            var currentPoint = currentResult.MinimizingPoint;
            var currentValue = currentResult.ModelInfoAtMinimum.Value;

            // Store best result
            var storage = new LeastSqauresStorage(currentResult);

            // Call callback if provided
            if (callback != null)
            {
                var stop = callback(currentPoint, currentValue, true);
                if (stop)
                {
                    status = BasinHoppingStatus.CallbackRequestedStop;
                    return currentResult;
                }
            }

            // Initialize success counter
            var successCount = 0;
            var maxSuccessCount = successIterations ?? (maxIterations + 2);

            // Main iteration loop
            for (iterations = 0; iterations < maxIterations; iterations++)
            {
                // Check stopping conditions
                if (convergence.Cancel)
                {
                    status = BasinHoppingStatus.ForcedStop;
                    break;
                }

                if (convergence.MaximumEvaluations > 0 && evaluations >= convergence.MaximumEvaluations)
                {
                    status = BasinHoppingStatus.MaximumEvaluationsReached;
                    break;
                }

                if (convergence.MaximumTime > TimeSpan.Zero &&
                    DateTime.Now - convergence.StartTime >= convergence.MaximumTime)
                {
                    status = BasinHoppingStatus.MaximumTimeReached;
                    break;
                }

                // Perform Monte Carlo step
                stepCount++;
                var newGlobalMin = false;
                var accepted = false;

                // Take random step
                var trialPoint = takeStep(currentPoint.Clone());

                // Perform local minimization
                var trialFunc = objectiveModel.Fork();
                var trialResult = leastSquaresMinimizer.FindMinimum(trialFunc, trialPoint, lowerBound, upperBound, scales, isFixed);

                evaluations += trialResult.Iterations;
                convergence.Evaluations += trialResult.Iterations;

                // Perform acceptance test
                var testResult = acceptTest(trialResult.ModelInfoAtMinimum.Value, currentResult.ModelInfoAtMinimum.Value);
                accepted = testResult == AcceptanceStatus.Accept || testResult == AcceptanceStatus.ForceAccept;

                // Update current position if step is accepted
                if (accepted)
                {
                    acceptCount++;
                    currentResult = trialResult;
                    currentPoint = trialResult.MinimizingPoint;
                    currentValue = trialResult.ModelInfoAtMinimum.Value;

                    // Check if we found a new global minimum
                    newGlobalMin = storage.Update(trialResult);
                }

                // Adjust step size if needed
                if (stepCount >= interval)
                {
                    AdjustStepSize();
                }

                // Call callback if provided
                if (callback != null)
                {
                    var stop = callback(trialResult.MinimizingPoint, trialResult.ModelInfoAtMinimum.Value, accepted);
                    if (stop)
                    {
                        status = BasinHoppingStatus.CallbackRequestedStop;
                        break;
                    }
                }

                // Check success condition
                if (newGlobalMin)
                {
                    successCount = 0;
                }
                else
                {
                    successCount++;
                    if (successCount > maxSuccessCount)
                    {
                        status = BasinHoppingStatus.SuccessConditionSatisfied;
                        break;
                    }
                }
            }

            // Set final result
            var bestResult = storage.BestResult;
            solution = bestResult.MinimizingPoint;
            functionValue = bestResult.ModelInfoAtMinimum.Value;

            // Create a new result with our exit condition
            var exitCondition = ConvertToExitCondition(status);

            // If using the exact same object is important, you could potentially modify the reasonForExit field 
            // via reflection, but creating a new result is cleaner
            return new NonlinearMinimizationResult(
                bestResult.ModelInfoAtMinimum,
                iterations + 1,
                exitCondition);
        }

        /// <summary>
        /// LeastSqauresStorage class to keep track of the best result found.
        /// </summary>
        private class LeastSqauresStorage
        {
            /// <summary>
            /// Gets the best minimization result found so far.
            /// </summary>
            public NonlinearMinimizationResult BestResult { get; private set; }

            /// <summary>
            /// Initializes a new instance of the LeastSqauresStorage class.
            /// </summary>
            /// <param name="initialResult">The initial minimization result.</param>
            public LeastSqauresStorage(NonlinearMinimizationResult initialResult)
            {
                BestResult = initialResult;
            }

            /// <summary>
            /// Updates the best result if the new result is better.
            /// </summary>
            /// <param name="newResult">The new minimization result to consider.</param>
            /// <returns>True if the best result was updated, false otherwise.</returns>
            public bool Update(NonlinearMinimizationResult newResult)
            {
                if (newResult.ReasonForExit == ExitCondition.Converged &&
                    (newResult.ModelInfoAtMinimum.Value < BestResult.ModelInfoAtMinimum.Value ||
                     BestResult.ReasonForExit != ExitCondition.Converged))
                {
                    BestResult = newResult;
                    return true;
                }
                return false;
            }
        }

        #endregion

        #region using an unconstrained minimizer

        /// <summary>
        /// Initializes a new instance of the BasinHopping class using an unconstrained minimizer.
        /// </summary>
        /// <param name="localMinimizer">Local unconstrained minimizer to use at each step.</param>
        /// <param name="temperature">The "temperature" parameter for the acceptance criterion.</param>
        /// <param name="stepSize">Maximum step size for the random displacement.</param>
        /// <param name="maxIterations">Maximum number of basin-hopping iterations.</param>
        /// <param name="successIterations">Stop if global minimum candidate remains the same for this many iterations.</param>
        /// <param name="interval">Interval for step size updates.</param>
        /// <param name="targetAcceptRate">Target acceptance rate for step size adjustment.</param>
        /// <param name="stepwiseFactor">Factor for step size adjustment.</param>
        public BasinHopping(
            IUnconstrainedMinimizer localMinimizer,
            double temperature = 1.0,
            double stepSize = 0.5,
            int maxIterations = 100,
            int? successIterations = null,
            int interval = 50,
            double targetAcceptRate = 0.5,
            double stepwiseFactor = 0.9)
            : base(gradientTolerance: 1E-8, stepTolerance: 1E-8, functionTolerance: 1E-8)
        {
            this.temperature = temperature;
            this.stepSize = stepSize;
            this.unconstrainedMinimizer = localMinimizer;
            this.maxIterations = maxIterations;
            this.successIterations = successIterations;
            this.interval = interval;
            this.targetAcceptRate = targetAcceptRate;
            this.stepwiseFactor = stepwiseFactor;
            random = new Random();
            convergence = new GeneralConvergence(1);

            // Create default step-taking function
            takeStep = RandomDisplacement;
        }

        /// <inheritdoc/>
        public MinimizationResult FindMinimum(IObjectiveFunction objective, Vector<double> initialGuess)
        {
            if (objective == null)
            {
                throw new ArgumentNullException(nameof(objective));
            }

            if (initialGuess == null)
            {
                throw new ArgumentNullException(nameof(initialGuess));
            }

            if (unconstrainedMinimizer == null)
            {
                throw new ArgumentNullException(nameof(unconstrainedMinimizer),
                    "An unconstrained minimizer must be provided for this optimization.");
            }

            // Use default step function and acceptance test if not set
            if (takeStep == null)
            {
                takeStep = RandomDisplacement;
            }

            if (acceptTest == null)
            {
                acceptTest = MetropolisAcceptance;
            }

            // Proceed with the basin-hopping optimization
            return Optimize(objective, initialGuess);
        }

        /// <summary>
        /// Core optimization logic for the basin-hopping algorithm.
        /// </summary>
        private MinimizationResult Optimize(IObjectiveFunction objective, Vector<double> initialGuess)
        {
            // Validate bounds first
            ValidateBounds(initialGuess, null, null, null);

            // Set up tracking variables
            convergence.StartTime = DateTime.Now;
            convergence.Evaluations = 0;
            evaluations = 0;
            iterations = 0;
            stepCount = 0;
            acceptCount = 0;
            status = BasinHoppingStatus.IterationsCompleted;

            // Perform initial minimization
            var currentResult = unconstrainedMinimizer.FindMinimum(objective, initialGuess);
            var currentPoint = currentResult.MinimizingPoint;
            var currentValue = currentResult.FunctionInfoAtMinimum.Value;
            evaluations += currentResult.Iterations;

            // Store best result
            var storage = new UnconstrainedStorage(currentResult);

            // Call callback if provided
            if (callback != null)
            {
                var stop = callback(currentPoint, currentValue, true);
                if (stop)
                {
                    status = BasinHoppingStatus.CallbackRequestedStop;
                    return currentResult;
                }
            }

            // Initialize success counter
            var successCount = 0;
            var maxSuccessCount = successIterations ?? (maxIterations + 2);

            // Main iteration loop
            for (iterations = 0; iterations < maxIterations; iterations++)
            {
                // Check stopping conditions
                if (convergence.Cancel)
                {
                    status = BasinHoppingStatus.ForcedStop;
                    break;
                }

                if (convergence.MaximumEvaluations > 0 && evaluations >= convergence.MaximumEvaluations)
                {
                    status = BasinHoppingStatus.MaximumEvaluationsReached;
                    break;
                }

                if (convergence.MaximumTime > TimeSpan.Zero &&
                    DateTime.Now - convergence.StartTime >= convergence.MaximumTime)
                {
                    status = BasinHoppingStatus.MaximumTimeReached;
                    break;
                }

                // Perform Monte Carlo step
                stepCount++;
                var newGlobalMin = false;
                var accepted = false;

                // Take random step
                var trialPoint = takeStep(currentPoint.Clone());

                // Perform local minimization
                var trialFunc = objective.Fork();
                var trialResult = unconstrainedMinimizer.FindMinimum(trialFunc, trialPoint);

                evaluations += trialResult.Iterations;
                convergence.Evaluations += trialResult.Iterations;

                // Perform acceptance test
                var testResult = acceptTest(trialResult.FunctionInfoAtMinimum.Value, currentResult.FunctionInfoAtMinimum.Value);
                accepted = testResult == AcceptanceStatus.Accept || testResult == AcceptanceStatus.ForceAccept;

                // Update current position if step is accepted
                if (accepted)
                {
                    acceptCount++;
                    currentResult = trialResult;
                    currentPoint = trialResult.MinimizingPoint;
                    currentValue = trialResult.FunctionInfoAtMinimum.Value;

                    // Check if we found a new global minimum
                    newGlobalMin = storage.Update(trialResult);
                }

                // Adjust step size if needed
                if (stepCount >= interval)
                {
                    AdjustStepSize();
                }

                // Call callback if provided
                if (callback != null)
                {
                    var stop = callback(trialResult.MinimizingPoint, trialResult.FunctionInfoAtMinimum.Value, accepted);
                    if (stop)
                    {
                        status = BasinHoppingStatus.CallbackRequestedStop;
                        break;
                    }
                }

                // Check success condition
                if (newGlobalMin)
                {
                    successCount = 0;
                }
                else
                {
                    successCount++;
                    if (successCount > maxSuccessCount)
                    {
                        status = BasinHoppingStatus.SuccessConditionSatisfied;
                        break;
                    }
                }
            }

            // Set final result
            var bestResult = storage.BestResult;
            solution = bestResult.MinimizingPoint;
            functionValue = bestResult.FunctionInfoAtMinimum.Value;

            // Create a new result with our exit condition
            return new MinimizationResult(
                bestResult.FunctionInfoAtMinimum,
                iterations + 1,
                ConvertToExitCondition(status));
        }

        /// <summary>
        /// Storage class to keep track of the best result found when using IUnconstrainedMinimizer.
        /// </summary>
        private class UnconstrainedStorage
        {
            /// <summary>
            /// Gets the best minimization result found so far.
            /// </summary>
            public MinimizationResult BestResult { get; private set; }

            /// <summary>
            /// Initializes a new instance of the UnconstrainedStorage class.
            /// </summary>
            /// <param name="initialResult">The initial minimization result.</param>
            public UnconstrainedStorage(MinimizationResult initialResult)
            {
                BestResult = initialResult;
            }

            /// <summary>
            /// Updates the best result if the new result is better.
            /// </summary>
            /// <param name="newResult">The new minimization result to consider.</param>
            /// <returns>True if the best result was updated, false otherwise.</returns>
            public bool Update(MinimizationResult newResult)
            {
                var isNewSuccessful = newResult.ReasonForExit == ExitCondition.Converged
                                   || newResult.ReasonForExit == ExitCondition.RelativePoints
                                   || newResult.ReasonForExit == ExitCondition.RelativeGradient
                                   || newResult.ReasonForExit == ExitCondition.AbsoluteGradient;

                var isBestSuccessful = BestResult.ReasonForExit == ExitCondition.Converged
                                    || BestResult.ReasonForExit == ExitCondition.RelativePoints
                                    || BestResult.ReasonForExit == ExitCondition.RelativeGradient
                                    || BestResult.ReasonForExit == ExitCondition.AbsoluteGradient;

                if (isNewSuccessful
                    && (newResult.FunctionInfoAtMinimum.Value < BestResult.FunctionInfoAtMinimum.Value || !isBestSuccessful))
                {
                    BestResult = newResult;
                    return true;
                }
                return false;
            }
        }

        #endregion

        /// <summary>
        /// Sets the step-taking function for the basin-hopping algorithm.
        /// </summary>
        /// <param name="stepFunction">The function that performs random displacements.</param>
        public void SetTakeStep(TakeStepDelegate stepFunction)
        {
            takeStep = stepFunction ?? throw new ArgumentNullException(nameof(stepFunction));
        }

        /// <summary>
        /// Sets the acceptance test function for the basin-hopping algorithm.
        /// </summary>
        /// <param name="testFunction">The function that tests whether to accept steps.</param>
        public void SetAcceptTest(AcceptTestDelegate testFunction)
        {
            acceptTest = testFunction ?? throw new ArgumentNullException(nameof(testFunction));
        }

        /// <summary>
        /// Sets the callback function for monitoring optimization progress.
        /// </summary>
        /// <param name="callbackFunction">The callback function to call for each minimum found.</param>
        public void SetCallback(CallbackDelegate callbackFunction)
        {
            callback = callbackFunction;
        }

        /// <summary>
        /// Default random displacement function.
        /// </summary>
        /// <param name="x">Current position.</param>
        /// <returns>New position after random displacement.</returns>
        private Vector<double> RandomDisplacement(Vector<double> x)
        {
            // Convert external parameters to internal
            var internalParams = ProjectToInternalParameters(x);

            for (var i = 0; i < internalParams.Count; i++)
            {
                if (isFixed != null && i < isFixed.Count && isFixed[i])
                {
                    continue;
                }

                var displacement = (random.NextDouble() * 2 - 1) * stepSize;
                internalParams[i] += displacement;
            }

            // Convert back to external parameters
            return ProjectToExternalParameters(internalParams);
        }

        /// <summary>
        /// Metropolis acceptance criterion.
        /// </summary>
        /// <param name="newValue">New function value.</param>
        /// <param name="oldValue">Previous function value.</param>
        /// <returns>Boolean indicating whether to accept the step.</returns>
        private AcceptanceStatus MetropolisAcceptance(double newValue, double oldValue)
        {
            // Always accept if new value is lower
            if (newValue < oldValue)
            {
                return AcceptanceStatus.Accept;
            }

            // Reject all steps that increase energy if T = 0
            if (temperature == 0)
            {
                return AcceptanceStatus.Reject;
            }

            // Accept with probability based on temperature
            var w = Math.Exp(-(newValue - oldValue) / temperature);
            return random.NextDouble() < w ? AcceptanceStatus.Accept : AcceptanceStatus.Reject;
        }

        /// <summary>
        /// Adjusts the step size based on the acceptance rate.
        /// </summary>
        private void AdjustStepSize()
        {
            var acceptRate = (double)acceptCount / stepCount;

            if (acceptRate > targetAcceptRate)
            {
                // Accepting too many steps - increase step size
                stepSize /= stepwiseFactor;
            }
            else
            {
                // Not accepting enough steps - decrease step size
                stepSize *= stepwiseFactor;
            }

            // Reset counters
            stepCount = 0;
            acceptCount = 0;
        }             

        /// <summary>
        /// Converts BasinHoppingStatus to ExitCondition.
        /// </summary>
        /// <param name="status">The status to convert.</param>
        /// <returns>The corresponding ExitCondition.</returns>
        private static ExitCondition ConvertToExitCondition(BasinHoppingStatus status)
        {
            switch (status)
            {
                case BasinHoppingStatus.IterationsCompleted:
                case BasinHoppingStatus.SuccessConditionSatisfied:
                    return ExitCondition.Converged;
                case BasinHoppingStatus.CallbackRequestedStop:
                case BasinHoppingStatus.ForcedStop:
                    return ExitCondition.ManuallyStopped;
                case BasinHoppingStatus.MaximumTimeReached:
                case BasinHoppingStatus.MaximumEvaluationsReached:
                    return ExitCondition.ExceedIterations;
                default:
                    return ExitCondition.None;
            }
        }

        /// <summary>
        /// Indicates the status of a basin-hopping optimization.
        /// </summary>
        private enum BasinHoppingStatus
        {
            /// <summary>
            /// Completed the requested number of iterations.
            /// </summary>
            IterationsCompleted,

            /// <summary>
            /// Success condition was satisfied.
            /// </summary>
            SuccessConditionSatisfied,

            /// <summary>
            /// The callback function requested early termination.
            /// </summary>
            CallbackRequestedStop,

            /// <summary>
            /// The optimization was forcibly stopped.
            /// </summary>
            ForcedStop,

            /// <summary>
            /// The maximum time limit was reached.
            /// </summary>
            MaximumTimeReached,

            /// <summary>
            /// The maximum number of function evaluations was reached.
            /// </summary>
            MaximumEvaluationsReached
        }
    }
}
