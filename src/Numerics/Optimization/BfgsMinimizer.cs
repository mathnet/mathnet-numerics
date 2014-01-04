// <copyright file="BfgsMinimizer.cs" company="Math.NET">
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
using MathNet.Numerics.LinearAlgebra;

namespace MathNet.Numerics.Optimization
{
    public class BfgsMinimizer
    {
        public double GradientTolerance { get; set; }
        public double ParameterTolerance { get; set; }
        public int MaximumIterations { get; set; }

        public BfgsMinimizer(double gradientTolerance, double parameterTolerance, int maximumIterations = 1000)
        {
            GradientTolerance = gradientTolerance;
            ParameterTolerance = parameterTolerance;
            MaximumIterations = maximumIterations;
        }

        public MinimizationOutput FindMinimum(IObjectiveFunction objective, Vector<double> initialGuess)
        {
            if (!objective.GradientSupported)
                throw new IncompatibleObjectiveException("Gradient not supported in objective function, but required for BFGS minimization.");

            if (!(objective is ObjectiveChecker))
                objective = new ObjectiveChecker(objective, ValidateObjective, ValidateGradient, null);

            IEvaluation initialEval = objective.Evaluate(initialGuess);

            // Check that we're not already done
            ExitCondition currentExitCondition = ExitCriteriaSatisfied(initialEval, null);
            if (currentExitCondition != ExitCondition.None)
                return new MinimizationOutput(initialEval, 0, currentExitCondition);

            // Set up line search algorithm
            var lineSearcher = new WeakWolfeLineSearch(1e-4, 0.9, ParameterTolerance, maxIterations: 1000);

            // Declare state variables
            Vector<double> gradient;

            // First step
            Matrix<double> inversePseudoHessian = Matrix<double>.Build.DiagonalIdentity(initialGuess.Count);
            Vector<double> searchDirection = -initialEval.Gradient;
            double stepSize = 100*GradientTolerance/(searchDirection*searchDirection);

            LineSearchOutput result;
            try
            {
                result = lineSearcher.FindConformingStep(objective, initialEval, searchDirection, stepSize);
            }
            catch (Exception e)
            {
                throw new InnerOptimizationException("Line search failed.", e);
            }

            IEvaluation previousPoint = initialEval;
            IEvaluation candidatePoint = result.FunctionInfoAtMinimum;
            gradient = candidatePoint.Gradient;
            Vector<double> step = candidatePoint.Point - initialGuess;
            stepSize = result.FinalStep;

            // Subsequent steps
            int iterations;
            int totalLineSearchSteps = result.Iterations;
            int iterationsWithNontrivialLineSearch = result.Iterations > 0 ? 0 : 1;
            for (iterations = 1; iterations < MaximumIterations; ++iterations)
            {
                var y = candidatePoint.Gradient - previousPoint.Gradient;

                double sy = step*y;
                inversePseudoHessian = inversePseudoHessian + ((sy + y*inversePseudoHessian*y)/Math.Pow(sy, 2.0))*step.OuterProduct(step) - ((inversePseudoHessian*y.ToColumnMatrix())*step.ToRowMatrix() + step.ToColumnMatrix()*(y.ToRowMatrix()*inversePseudoHessian))*(1.0/sy);

                searchDirection = -inversePseudoHessian*candidatePoint.Gradient;

                if (searchDirection*candidatePoint.Gradient >= -GradientTolerance*GradientTolerance)
                {
                    searchDirection = -candidatePoint.Gradient;
                    inversePseudoHessian = Matrix<double>.Build.DiagonalIdentity(initialGuess.Count);
                }

                try
                {
                    result = lineSearcher.FindConformingStep(objective, candidatePoint, searchDirection, 1.0);
                }
                catch (Exception e)
                {
                    throw new InnerOptimizationException("Line search failed.", e);
                }

                iterationsWithNontrivialLineSearch += result.Iterations > 0 ? 1 : 0;
                totalLineSearchSteps += result.Iterations;

                stepSize = result.FinalStep;
                step = result.FunctionInfoAtMinimum.Point - candidatePoint.Point;
                previousPoint = candidatePoint;
                candidatePoint = result.FunctionInfoAtMinimum;

                currentExitCondition = ExitCriteriaSatisfied(candidatePoint, previousPoint);
                if (currentExitCondition != ExitCondition.None)
                    break;
            }

            if (iterations == MaximumIterations && currentExitCondition == ExitCondition.None)
                throw new MaximumIterationsException(String.Format("Maximum iterations ({0}) reached.", MaximumIterations));

            return new MinimizationWithLineSearchOutput(candidatePoint, iterations, currentExitCondition, totalLineSearchSteps, iterationsWithNontrivialLineSearch);
        }

        ExitCondition ExitCriteriaSatisfied(IEvaluation candidatePoint, IEvaluation lastPoint)
        {
            Vector<double> relGrad = new LinearAlgebra.Double.DenseVector(candidatePoint.Point.Count);
            double relativeGradient = 0.0;
            double normalizer = Math.Max(Math.Abs(candidatePoint.Value), 1.0);
            for (int ii = 0; ii < relGrad.Count; ++ii)
            {
                double tmp = candidatePoint.Gradient[ii]*Math.Max(Math.Abs(candidatePoint.Point[ii]), 1.0)/normalizer;
                relativeGradient = Math.Max(relativeGradient, Math.Abs(tmp));
            }
            if (relativeGradient < GradientTolerance)
            {
                return ExitCondition.RelativeGradient;
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
                    return ExitCondition.LackOfProgress;
                }
            }

            return ExitCondition.None;
        }

        void ValidateGradient(IEvaluation eval)
        {
            foreach (var x in eval.Gradient)
            {
                if (Double.IsNaN(x) || Double.IsInfinity(x))
                    throw new EvaluationException("Non-finite gradient returned.", eval);
            }
        }

        void ValidateObjective(IEvaluation eval)
        {
            if (Double.IsNaN(eval.Value) || Double.IsInfinity(eval.Value))
                throw new EvaluationException("Non-finite objective function returned.", eval);
        }
    }
}
