﻿// <copyright file="BfgsMinimizerBase.cs" company="Math.NET">
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

using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.Optimization.LineSearch;
using System;

namespace MathNet.Numerics.Optimization
{
    public abstract class BfgsMinimizerBase
    {
        public double GradientTolerance { get; set; }
        public double ParameterTolerance { get; set; }
        public double FunctionProgressTolerance { get; set; }
        public int MaximumIterations { get; set; }

        protected const double VerySmall = 1e-15;

        /// <summary>
        /// Creates a base class for BFGS minimization
        /// </summary>
        /// <param name="gradientTolerance">The gradient tolerance</param>
        /// <param name="parameterTolerance">The parameter tolerance</param>
        /// <param name="functionProgressTolerance">The funciton progress tolerance</param>
        /// <param name="maximumIterations">The maximum number of iterations</param>
        protected BfgsMinimizerBase(double gradientTolerance, double parameterTolerance, double functionProgressTolerance, int maximumIterations)
        {
            GradientTolerance = gradientTolerance;
            ParameterTolerance = parameterTolerance;
            FunctionProgressTolerance = functionProgressTolerance;
            MaximumIterations = maximumIterations;
        }

        protected ExitCondition ExitCriteriaSatisfied(IObjectiveFunctionEvaluation candidatePoint, IObjectiveFunctionEvaluation lastPoint, int iterations)
        {
            Vector<double> relGrad = new DenseVector(candidatePoint.Point.Count);
            double relativeGradient = 0.0;
            double normalizer = Math.Max(Math.Abs(candidatePoint.Value), 1.0);
            for (int ii = 0; ii < relGrad.Count; ++ii)
            {
                double projectedGradient = GetProjectedGradient(candidatePoint, ii);

                double tmp = projectedGradient *
                    Math.Max(Math.Abs(candidatePoint.Point[ii]), 1.0) / normalizer;
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
                    var tmp = Math.Abs(candidatePoint.Point[ii] - lastPoint.Point[ii]) /
                        Math.Max(Math.Abs(lastPoint.Point[ii]), 1.0);
                    mostProgress = Math.Max(mostProgress, tmp);
                }
                if (mostProgress < ParameterTolerance)
                {
                    return ExitCondition.LackOfProgress;
                }

                double functionChange = candidatePoint.Value - lastPoint.Value;
                if (iterations > 500 && functionChange < 0 && Math.Abs(functionChange) < FunctionProgressTolerance)
                    return ExitCondition.LackOfProgress;
            }

            return ExitCondition.None;
        }

        protected virtual double GetProjectedGradient(IObjectiveFunctionEvaluation candidatePoint, int ii)
        {
            return candidatePoint.Gradient[ii];
        }

        protected void ValidateGradientAndObjective(IObjectiveFunctionEvaluation eval)
        {
            foreach (var x in eval.Gradient)
            {
                if (Double.IsNaN(x) || Double.IsInfinity(x))
                    throw new EvaluationException("Non-finite gradient returned.", eval);
            }
            if (Double.IsNaN(eval.Value) || Double.IsInfinity(eval.Value))
                throw new EvaluationException("Non-finite objective function returned.", eval);
        }

        protected int DoBfgsUpdate(ref ExitCondition currentExitCondition, WolfeLineSearch lineSearcher, ref Matrix<double> inversePseudoHessian, ref Vector<double> lineSearchDirection, ref IObjectiveFunction previousPoint, ref LineSearchResult lineSearchResult, ref IObjectiveFunction candidate, ref Vector<double> step, ref int totalLineSearchSteps, ref int iterationsWithNontrivialLineSearch)
        {
            int iterations;
            for (iterations = 1; iterations < MaximumIterations; ++iterations)
            {
                double startingStepSize;
                double maxLineSearchStep;
                lineSearchDirection = CalculateSearchDirection(ref inversePseudoHessian, out maxLineSearchStep, out startingStepSize, previousPoint, candidate, step);

                try
                {
                    lineSearchResult = lineSearcher.FindConformingStep(candidate, lineSearchDirection, startingStepSize, maxLineSearchStep);
                }
                catch (Exception e)
                {
                    throw new InnerOptimizationException("Line search failed.", e);
                }

                iterationsWithNontrivialLineSearch += lineSearchResult.Iterations > 0 ? 1 : 0;
                totalLineSearchSteps += lineSearchResult.Iterations;

                step = lineSearchResult.FunctionInfoAtMinimum.Point - candidate.Point;
                previousPoint = candidate;
                candidate = lineSearchResult.FunctionInfoAtMinimum;

                currentExitCondition = ExitCriteriaSatisfied(candidate, previousPoint, iterations);
                if (currentExitCondition != ExitCondition.None)
                    break;
            }

            return iterations;
        }

        protected abstract Vector<double> CalculateSearchDirection(ref Matrix<double> inversePseudoHessian,
            out double maxLineSearchStep,
            out double startingStepSize,
            IObjectiveFunction previousPoint,
            IObjectiveFunction candidate,
            Vector<double> step);
    }
}
