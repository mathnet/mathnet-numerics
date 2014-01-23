using System;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.Optimization.LineSearch;

namespace MathNet.Numerics.Optimization
{
    public class BfgsBMinimizer
    {
        public double GradientTolerance { get; set; }
        public double ParameterTolerance { get; set; }
        public int MaximumIterations { get; set; }
        public double FunctionProgressTolerance { get; set; }

        public BfgsBMinimizer(double gradientTolerance, double parameterTolerance, double functionProgressTolerance, int maximumIterations = 1000)
        {
            GradientTolerance = gradientTolerance;
            ParameterTolerance = parameterTolerance;
            MaximumIterations = maximumIterations;
            FunctionProgressTolerance = functionProgressTolerance;
        }

        public MinimizationResult FindMinimum(IObjectiveFunction objective, Vector<double> lowerBound, Vector<double> upperBound, Vector<double> initialGuess)
        {
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

            // Check that we're not already done
            MinimizationResult.ExitCondition currentExitCondition = ExitCriteriaSatisfied(objective, null, lowerBound, upperBound, 0);
            if (currentExitCondition != MinimizationResult.ExitCondition.None)
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
            var cauchyPoint = gradientProjectionResult.Item1;
            var fixedCount = gradientProjectionResult.Item2;
            var isFixed = gradientProjectionResult.Item3;
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
            LineSearchResult result;
            try
            {
                result = lineSearcher.FindConformingStep(objective, lineSearchDirection, startingStepSize, upperBound: maxLineSearchStep);
            }
            catch (Exception e)
            {
                throw new InnerOptimizationException("Line search failed.", e);
            }

            var previousPoint = objective.Fork();
            var candidatePoint = result.FunctionInfoAtMinimum;
            var gradient = candidatePoint.Gradient;
            var step = candidatePoint.Point - initialGuess;

            // Subsequent steps
            int iterations;
            int totalLineSearchSteps = result.Iterations;
            int iterationsWithNontrivialLineSearch = result.Iterations > 0 ? 0 : 1;
            for (iterations = 1; iterations < MaximumIterations; ++iterations)
            {
                // Do BFGS update
                var y = candidatePoint.Gradient - previousPoint.Gradient;

                double sy = step*y;
                if (sy > 0.0) // only do update if it will create a positive definite matrix
                {
                    double sts = step*step;
                    //inverse_pseudo_hessian = inverse_pseudo_hessian + ((sy + y * inverse_pseudo_hessian * y) / Math.Pow(sy, 2.0)) * step.OuterProduct(step) - ((inverse_pseudo_hessian * y.ToColumnMatrix()) * step.ToRowMatrix() + step.ToColumnMatrix() * (y.ToRowMatrix() * inverse_pseudo_hessian)) * (1.0 / sy);
                    var Hs = pseudoHessian*step;
                    var sHs = step*pseudoHessian*step;
                    pseudoHessian = pseudoHessian + y.OuterProduct(y)*(1.0/sy) - Hs.OuterProduct(Hs)*(1.0/sHs);
                }
                else
                {
                    //pseudo_hessian = LinearAlgebra.Double.DiagonalMatrix.Identity(initial_guess.Count);
                }

                // Determine active set
                gradientProjectionResult = QuadraticGradientProjectionSearch.Search(candidatePoint.Point, candidatePoint.Gradient, pseudoHessian, lowerBound, upperBound);
                cauchyPoint = gradientProjectionResult.Item1;
                fixedCount = gradientProjectionResult.Item2;
                isFixed = gradientProjectionResult.Item3;
                freeCount = lowerBound.Count - fixedCount;

                if (freeCount > 0)
                {
                    reducedGradient = new DenseVector(freeCount);
                    reducedHessian = new DenseMatrix(freeCount, freeCount);
                    reducedMap = new List<int>(freeCount);
                    reducedInitialPoint = new DenseVector(freeCount);
                    reducedCauchyPoint = new DenseVector(freeCount);

                    CreateReducedData(candidatePoint.Point, cauchyPoint, isFixed, lowerBound, upperBound, candidatePoint.Gradient, pseudoHessian, reducedInitialPoint, reducedCauchyPoint, reducedGradient, reducedHessian, reducedMap);

                    // Determine search direction and maximum step size
                    reducedSolution1 = reducedInitialPoint + reducedHessian.Cholesky().Solve(-reducedGradient);

                    solution1 = ReducedToFull(reducedMap, reducedSolution1, cauchyPoint);
                }
                else
                {
                    solution1 = cauchyPoint;
                }

                directionFromCauchy = solution1 - cauchyPoint;
                maxStepFromCauchyPoint = FindMaxStep(cauchyPoint, directionFromCauchy, lowerBound, upperBound);
                //var cauchy_eval = objective.Evaluate(cauchy_point);

                solution2 = cauchyPoint + Math.Min(maxStepFromCauchyPoint, 1.0)*directionFromCauchy;

                lineSearchDirection = solution2 - candidatePoint.Point;
                maxLineSearchStep = FindMaxStep(candidatePoint.Point, lineSearchDirection, lowerBound, upperBound);

                //line_search_direction = solution1 - candidate_point.Point;
                //max_line_search_step = FindMaxStep(candidate_point.Point, line_search_direction, lower_bound, upper_bound);

                if (maxLineSearchStep == 0.0)
                {
                    lineSearchDirection = cauchyPoint - candidatePoint.Point;
                    maxLineSearchStep = FindMaxStep(candidatePoint.Point, lineSearchDirection, lowerBound, upperBound);
                }

                estStepSize = -candidatePoint.Gradient*lineSearchDirection/(lineSearchDirection*pseudoHessian*lineSearchDirection);

                startingStepSize = Math.Min(Math.Max(estStepSize, 1.0), maxLineSearchStep);

                // Line search
                try
                {
                    result = lineSearcher.FindConformingStep(candidatePoint, lineSearchDirection, startingStepSize, upperBound: maxLineSearchStep);
                    //result = line_searcher.FindConformingStep(objective, cauchy_eval, direction_from_cauchy, Math.Min(1.0, max_step_from_cauchy_point), upper_bound: max_step_from_cauchy_point);
                }
                catch (Exception e)
                {
                    throw new InnerOptimizationException("Line search failed.", e);
                }

                iterationsWithNontrivialLineSearch += result.Iterations > 0 ? 1 : 0;
                totalLineSearchSteps += result.Iterations;

                step = result.FunctionInfoAtMinimum.Point - candidatePoint.Point;
                previousPoint = candidatePoint;
                candidatePoint = result.FunctionInfoAtMinimum;

                currentExitCondition = ExitCriteriaSatisfied(candidatePoint, previousPoint, lowerBound, upperBound, iterations);
                if (currentExitCondition != MinimizationResult.ExitCondition.None)
                    break;
            }

            if (iterations == MaximumIterations && currentExitCondition == MinimizationResult.ExitCondition.None)
                throw new MaximumIterationsException(String.Format("Maximum iterations ({0}) reached.", MaximumIterations));

            return new MinimizationWithLineSearchResult(candidatePoint, iterations, currentExitCondition, totalLineSearchSteps, iterationsWithNontrivialLineSearch);
        }

        static Vector<double> ReducedToFull(List<int> reducedMap, Vector<double> reducedVector, Vector<double> fullVector)
        {
            var output = fullVector.Clone();
            for (int ii = 0; ii < reducedMap.Count; ++ii)
                output[reducedMap[ii]] = reducedVector[ii];
            return output;
        }

        static double FindMaxStep(Vector<double> startingPoint, Vector<double> searchDirection, Vector<double> lowerBound, Vector<double> upperBound)
        {
            double maxStep = Double.PositiveInfinity;
            for (int ii = 0; ii < startingPoint.Count; ++ii)
            {
                double paramMaxStep;
                if (searchDirection[ii] > 0)
                    paramMaxStep = (upperBound[ii] - startingPoint[ii])/searchDirection[ii];
                else if (searchDirection[ii] < 0)
                    paramMaxStep = (startingPoint[ii] - lowerBound[ii])/-searchDirection[ii];
                else
                    paramMaxStep = Double.PositiveInfinity;

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

        const double VerySmall = 1e-15;

        MinimizationResult.ExitCondition ExitCriteriaSatisfied(IObjectiveFunction candidatePoint, IObjectiveFunction lastPoint, Vector<double> lowerBound, Vector<double> upperBound, int iterations)
        {
            Vector<double> relGrad = new DenseVector(candidatePoint.Point.Count);
            double relativeGradient = 0.0;
            double normalizer = Math.Max(Math.Abs(candidatePoint.Value), 1.0);
            for (int ii = 0; ii < relGrad.Count; ++ii)
            {
                double projectedGradient;

                bool atLowerBound = candidatePoint.Point[ii] - lowerBound[ii] < VerySmall;
                bool atUpperBound = upperBound[ii] - candidatePoint.Point[ii] < VerySmall;

                if (atLowerBound && atUpperBound)
                    projectedGradient = 0.0;
                else if (atLowerBound)
                    projectedGradient = Math.Min(candidatePoint.Gradient[ii], 0.0);
                else if (atUpperBound)
                    projectedGradient = Math.Max(candidatePoint.Gradient[ii], 0.0);
                else
                    projectedGradient = candidatePoint.Gradient[ii];

                double tmp = projectedGradient*Math.Max(Math.Abs(candidatePoint.Point[ii]), 1.0)/normalizer;
                relativeGradient = Math.Max(relativeGradient, Math.Abs(tmp));
            }
            if (relativeGradient < GradientTolerance)
            {
                return MinimizationResult.ExitCondition.RelativeGradient;
            }

            if (lastPoint != null)
            {
                double mostProgress = 0.0;
                for (int ii = 0; ii < candidatePoint.Point.Count; ++ii)
                {
                    var tmp = Math.Abs(candidatePoint.Point[ii] - lastPoint.Point[ii])/Math.Max(Math.Abs(lastPoint.Point[ii]), 1.0);
                    mostProgress = Math.Max(mostProgress, tmp);
                }
                if (mostProgress < ParameterTolerance)
                {
                    return MinimizationResult.ExitCondition.LackOfProgress;
                }

                double functionChange = candidatePoint.Value - lastPoint.Value;
                if (iterations > 500 && functionChange < 0 && Math.Abs(functionChange) < FunctionProgressTolerance)
                    return MinimizationResult.ExitCondition.LackOfProgress;
            }

            return MinimizationResult.ExitCondition.None;
        }

        void ValidateGradient(IObjectiveFunction eval)
        {
            foreach (var x in eval.Gradient)
            {
                if (Double.IsNaN(x) || Double.IsInfinity(x))
                    throw new EvaluationException("Non-finite gradient returned.", eval);
            }
        }

        void ValidateObjective(IObjectiveFunction eval)
        {
            if (Double.IsNaN(eval.Value) || Double.IsInfinity(eval.Value))
                throw new EvaluationException("Non-finite objective function returned.", eval);
        }
    }
}
