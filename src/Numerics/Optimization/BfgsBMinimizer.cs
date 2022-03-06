// <copyright file="BfgsBMinimizer.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2017 Math.NET
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
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.Optimization.LineSearch;

namespace MathNet.Numerics.Optimization
{
    /// <summary>
    /// Broyden–Fletcher–Goldfarb–Shanno Bounded (BFGS-B) algorithm is an iterative method for solving box-constrained nonlinear optimization problems
    /// http://www.ece.northwestern.edu/~nocedal/PSfiles/limited.ps.gz
    /// </summary>
    public class BfgsBMinimizer : BfgsMinimizerBase
    {
        public BfgsBMinimizer(double gradientTolerance, double parameterTolerance, double functionProgressTolerance, int maximumIterations = 1000)
            : base(gradientTolerance,parameterTolerance,functionProgressTolerance,maximumIterations)
        {
        }

        /// <summary>
        /// Find the minimum of the objective function given lower and upper bounds
        /// </summary>
        /// <param name="objective">The objective function, must support a gradient</param>
        /// <param name="lowerBound">The lower bound</param>
        /// <param name="upperBound">The upper bound</param>
        /// <param name="initialGuess">The initial guess</param>
        /// <returns>The MinimizationResult which contains the minimum and the ExitCondition</returns>
        public MinimizationResult FindMinimum(IObjectiveFunction objective, Vector<double> lowerBound, Vector<double> upperBound, Vector<double> initialGuess)
        {
            _lowerBound = lowerBound;
            _upperBound = upperBound;
            if (!objective.IsGradientSupported)
                throw new IncompatibleObjectiveException("Gradient not supported in objective function, but required for BFGS minimization.");

            // Check that dimensions match
            if (lowerBound.Count != upperBound.Count || lowerBound.Count != initialGuess.Count)
                throw new ArgumentException("Dimensions of bounds and/or initial guess do not match.");

            // Check that initial guess is feasible
            for (int ii = 0; ii < initialGuess.Count; ++ii)
                if (initialGuess[ii] < lowerBound[ii] || initialGuess[ii] > upperBound[ii])
                    throw new ArgumentException("Initial guess is not in the feasible region");

            objective.EvaluateAt(initialGuess);
            ValidateGradientAndObjective(objective);

            // Check that we're not already done
            var currentExitCondition = ExitCriteriaSatisfied(objective, null, 0);
            if (currentExitCondition != ExitCondition.None)
                return new MinimizationResult(objective, 0, currentExitCondition);

            // Set up line search algorithm
            var lineSearcher = new StrongWolfeLineSearch(1e-4, 0.9, Math.Max(ParameterTolerance, 1e-5), maxIterations: 1000);

            // Declare state variables
            Vector<double> reducedSolution1, reducedGradient, reducedInitialPoint, reducedCauchyPoint, solution1;
            Matrix<double> reducedHessian;
            List<int> reducedMap;

            // First step
            var pseudoHessian = CreateMatrix.DiagonalIdentity<double>(initialGuess.Count);

            // Determine active set
            var gradientProjectionResult = QuadraticGradientProjectionSearch.Search(objective.Point, objective.Gradient, pseudoHessian, lowerBound, upperBound);
            var cauchyPoint = gradientProjectionResult.CauchyPoint;
            var fixedCount = gradientProjectionResult.FixedCount;
            var isFixed = gradientProjectionResult.IsFixed;
            var freeCount = lowerBound.Count - fixedCount;

            if (freeCount > 0)
            {
                reducedGradient = new DenseVector(freeCount);
                reducedHessian = new DenseMatrix(freeCount, freeCount);
                reducedMap = new List<int>(freeCount);
                reducedInitialPoint = new DenseVector(freeCount);
                reducedCauchyPoint = new DenseVector(freeCount);

                CreateReducedData(objective.Point, cauchyPoint, isFixed, lowerBound, upperBound, objective.Gradient, pseudoHessian, reducedInitialPoint, reducedCauchyPoint, reducedGradient, reducedHessian, reducedMap);

                // Determine search direction and maximum step size
                reducedSolution1 = reducedInitialPoint + reducedHessian.Cholesky().Solve(-reducedGradient);

                solution1 = ReducedToFull(reducedMap, reducedSolution1, cauchyPoint);
            }
            else
            {
                solution1 = cauchyPoint;
            }

            var directionFromCauchy = solution1 - cauchyPoint;
            var maxStepFromCauchyPoint = FindMaxStep(cauchyPoint, directionFromCauchy, lowerBound, upperBound);

            var solution2 = cauchyPoint + Math.Min(maxStepFromCauchyPoint, 1.0)*directionFromCauchy;

            var lineSearchDirection = solution2 - objective.Point;
            var maxLineSearchStep = FindMaxStep(objective.Point, lineSearchDirection, lowerBound, upperBound);
            var estStepSize = -objective.Gradient*lineSearchDirection/(lineSearchDirection*pseudoHessian*lineSearchDirection);

            var startingStepSize = Math.Min(Math.Max(estStepSize, 1.0), maxLineSearchStep);

            // Line search
            LineSearchResult lineSearchResult;
            try
            {
                lineSearchResult = lineSearcher.FindConformingStep(objective, lineSearchDirection, startingStepSize, upperBound: maxLineSearchStep);
            }
            catch (Exception e)
            {
                throw new InnerOptimizationException("Line search failed.", e);
            }

            var previousPoint = objective.Fork();
            var candidatePoint = lineSearchResult.FunctionInfoAtMinimum;
            ValidateGradientAndObjective(candidatePoint);

            // Check that we're not done
            currentExitCondition = ExitCriteriaSatisfied(candidatePoint, previousPoint, 0);
            if (currentExitCondition != ExitCondition.None)
                return new MinimizationResult(candidatePoint, 0, currentExitCondition);

            var step = candidatePoint.Point - initialGuess;

            // Subsequent steps
            int totalLineSearchSteps = lineSearchResult.Iterations;
            int iterationsWithNontrivialLineSearch = lineSearchResult.Iterations > 0 ? 0 : 1;

            int iterations = DoBfgsUpdate(ref currentExitCondition, lineSearcher, ref pseudoHessian, ref lineSearchDirection, ref previousPoint, ref lineSearchResult, ref candidatePoint, ref step, ref totalLineSearchSteps, ref iterationsWithNontrivialLineSearch);

            if (iterations == MaximumIterations && currentExitCondition == ExitCondition.None)
                throw new MaximumIterationsException(FormattableString.Invariant($"Maximum iterations ({MaximumIterations}) reached."));

            return new MinimizationWithLineSearchResult(candidatePoint, iterations, currentExitCondition, totalLineSearchSteps, iterationsWithNontrivialLineSearch);
        }

        protected override Vector<double> CalculateSearchDirection(ref Matrix<double> pseudoHessian,
            out double maxLineSearchStep,
            out double startingStepSize,
            IObjectiveFunction previousPoint,
            IObjectiveFunction candidatePoint,
            Vector<double> step)
        {
            Vector<double> lineSearchDirection;
            var y = candidatePoint.Gradient - previousPoint.Gradient;

            double sy = step * y;
            if (sy > 0.0) // only do update if it will create a positive definite matrix
            {
                var Hs = pseudoHessian * step;
                var sHs = step * pseudoHessian * step;
                pseudoHessian = pseudoHessian + y.OuterProduct(y) * (1.0 / sy) - Hs.OuterProduct(Hs) * (1.0 / sHs);
            }
            else
            {
                //pseudo_hessian = LinearAlgebra.Double.DiagonalMatrix.Identity(initial_guess.Count);
            }

            // Determine active set
            var gradientProjectionResult = QuadraticGradientProjectionSearch.Search(candidatePoint.Point, candidatePoint.Gradient, pseudoHessian, _lowerBound, _upperBound);
            var cauchyPoint = gradientProjectionResult.CauchyPoint;
            var fixedCount = gradientProjectionResult.FixedCount;
            var isFixed = gradientProjectionResult.IsFixed;
            var freeCount = _lowerBound.Count - fixedCount;
            Vector<double> solution1;
            if (freeCount > 0)
            {
                var reducedGradient = new DenseVector(freeCount);
                var reducedHessian = new DenseMatrix(freeCount, freeCount);
                var reducedMap = new List<int>(freeCount);
                var reducedInitialPoint = new DenseVector(freeCount);
                var reducedCauchyPoint = new DenseVector(freeCount);

                CreateReducedData(candidatePoint.Point, cauchyPoint, isFixed, _lowerBound, _upperBound, candidatePoint.Gradient, pseudoHessian, reducedInitialPoint, reducedCauchyPoint, reducedGradient, reducedHessian, reducedMap);

                // Determine search direction and maximum step size
                Vector<double> reducedSolution1 = reducedInitialPoint + reducedHessian.Cholesky().Solve(-reducedGradient);

                solution1 = ReducedToFull(reducedMap, reducedSolution1, cauchyPoint);
            }
            else
            {
                solution1 = cauchyPoint;
            }

            var directionFromCauchy = solution1 - cauchyPoint;
            var maxStepFromCauchyPoint = FindMaxStep(cauchyPoint, directionFromCauchy, _lowerBound, _upperBound);

            var solution2 = cauchyPoint + Math.Min(maxStepFromCauchyPoint, 1.0) * directionFromCauchy;

            lineSearchDirection = solution2 - candidatePoint.Point;
            maxLineSearchStep = FindMaxStep(candidatePoint.Point, lineSearchDirection, _lowerBound, _upperBound);

            if (maxLineSearchStep == 0.0)
            {
                lineSearchDirection = cauchyPoint - candidatePoint.Point;
                maxLineSearchStep = FindMaxStep(candidatePoint.Point, lineSearchDirection, _lowerBound, _upperBound);
            }

            double estStepSize = -candidatePoint.Gradient * lineSearchDirection / (lineSearchDirection * pseudoHessian * lineSearchDirection);

            startingStepSize = Math.Min(Math.Max(estStepSize, 1.0), maxLineSearchStep);
            return lineSearchDirection;
        }

        static Vector<double> ReducedToFull(List<int> reducedMap, Vector<double> reducedVector, Vector<double> fullVector)
        {
            var output = fullVector.Clone();
            for (int ii = 0; ii < reducedMap.Count; ++ii)
                output[reducedMap[ii]] = reducedVector[ii];
            return output;
        }

        Vector<double> _lowerBound;
        Vector<double> _upperBound;

        static double FindMaxStep(Vector<double> startingPoint, Vector<double> searchDirection, Vector<double> lowerBound, Vector<double> upperBound)
        {
            double maxStep = double.PositiveInfinity;
            for (int ii = 0; ii < startingPoint.Count; ++ii)
            {
                double paramMaxStep;
                if (searchDirection[ii] > 0)
                    paramMaxStep = (upperBound[ii] - startingPoint[ii])/searchDirection[ii];
                else if (searchDirection[ii] < 0)
                    paramMaxStep = (startingPoint[ii] - lowerBound[ii])/-searchDirection[ii];
                else
                    paramMaxStep = double.PositiveInfinity;

                if (paramMaxStep < maxStep)
                    maxStep = paramMaxStep;
            }
            return maxStep;
        }

        static void CreateReducedData(Vector<double> initialPoint, Vector<double> cauchyPoint, List<bool> isFixed, Vector<double> lowerBound, Vector<double> upperBound, Vector<double> gradient, Matrix<double> pseudoHessian, Vector<double> reducedInitialPoint, Vector<double> reducedCauchyPoint, Vector<double> reducedGradient, Matrix<double> reducedHessian, List<int> reducedMap)
        {
            int ll = 0;
            for (int ii = 0; ii < lowerBound.Count; ++ii)
            {
                if (!isFixed[ii])
                {
                    // hessian
                    int mm = 0;
                    for (int jj = 0; jj < lowerBound.Count; ++jj)
                    {
                        if (!isFixed[jj])
                        {
                            reducedHessian[ll, mm++] = pseudoHessian[ii, jj];
                        }
                    }

                    // gradient
                    reducedInitialPoint[ll] = initialPoint[ii];
                    reducedCauchyPoint[ll] = cauchyPoint[ii];
                    reducedGradient[ll] = gradient[ii];
                    ll += 1;
                    reducedMap.Add(ii);

                }
            }
        }

        protected override double GetProjectedGradient(IObjectiveFunctionEvaluation candidatePoint, int ii)
        {
            double projectedGradient;
            bool atLowerBound = candidatePoint.Point[ii] - _lowerBound[ii] < VerySmall;
            bool atUpperBound = _upperBound[ii] - candidatePoint.Point[ii] < VerySmall;

            if (atLowerBound && atUpperBound)
                projectedGradient = 0.0;
            else if (atLowerBound)
                projectedGradient = Math.Min(candidatePoint.Gradient[ii], 0.0);
            else if (atUpperBound)
                projectedGradient = Math.Max(candidatePoint.Gradient[ii], 0.0);
            else
                projectedGradient = base.GetProjectedGradient(candidatePoint, ii);
            return projectedGradient;
        }
    }
}
