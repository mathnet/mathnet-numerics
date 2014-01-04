// <copyright file="NewtonMinimizer.cs" company="Math.NET">
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
    public class NewtonMinimizer
    {
        public double GradientTolerance { get; set; }
        public int MaximumIterations { get; set; }
        public bool UseLineSearch { get; set; }

        public NewtonMinimizer(double gradientTolerance, int maximumIterations, bool useLineSearch = false)
        {
            GradientTolerance = gradientTolerance;
            MaximumIterations = maximumIterations;
            UseLineSearch = useLineSearch;
        }

        public MinimizationOutput FindMinimum(IObjectiveFunction objective, Vector<double> initialGuess)
        {
            if (!objective.GradientSupported)
                throw new IncompatibleObjectiveException("Gradient not supported in objective function, but required for Newton minimization.");

            if (!objective.HessianSupported)
                throw new IncompatibleObjectiveException("Hessian not supported in objective function, but required for Newton minimization.");

            if (!(objective is ObjectiveChecker))
                objective = new ObjectiveChecker(objective, ValidateObjective, ValidateGradient, ValidateHessian);

            IEvaluation initialEval = objective.Evaluate(initialGuess);

            // Check that we're not already done
            if (ExitCriteriaSatisfied(initialGuess, initialEval.Gradient))
                return new MinimizationOutput(initialEval, 0, ExitCondition.AbsoluteGradient);

            // Set up line search algorithm
            var lineSearcher = new WeakWolfeLineSearch(1e-4, 0.9, 1e-4, maxIterations: 1000);

            // Declare state variables
            IEvaluation candidatePoint = initialEval;

            // Subsequent steps
            int iterations = 0;
            int totalLineSearchSteps = 0;
            int iterationsWithNontrivialLineSearch = 0;
            bool tmpLineSearch = false;
            while (!ExitCriteriaSatisfied(candidatePoint.Point, candidatePoint.Gradient) && iterations < MaximumIterations)
            {

                Vector<double> searchDirection = candidatePoint.Hessian.LU().Solve(-candidatePoint.Gradient);

                if (searchDirection*candidatePoint.Gradient >= 0)
                {
                    searchDirection = -candidatePoint.Gradient;
                    tmpLineSearch = true;
                }

                if (UseLineSearch || tmpLineSearch)
                {
                    LineSearchOutput result;
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
                    candidatePoint = result.FunctionInfoAtMinimum;
                }
                else
                {
                    candidatePoint = objective.Evaluate(candidatePoint.Point + searchDirection);
                }

                tmpLineSearch = false;

                iterations += 1;
            }

            if (iterations == MaximumIterations)
                throw new MaximumIterationsException(String.Format("Maximum iterations ({0}) reached.", MaximumIterations));

            return new MinimizationWithLineSearchOutput(candidatePoint, iterations, ExitCondition.AbsoluteGradient, totalLineSearchSteps, iterationsWithNontrivialLineSearch);
        }

        bool ExitCriteriaSatisfied(Vector<double> candidatePoint, Vector<double> gradient)
        {
            return gradient.Norm(2.0) < GradientTolerance;
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

        void ValidateHessian(IEvaluation eval)
        {
            for (int ii = 0; ii < eval.Hessian.RowCount; ++ii)
            {
                for (int jj = 0; jj < eval.Hessian.ColumnCount; ++jj)
                {
                    if (Double.IsNaN(eval.Hessian[ii, jj]) || Double.IsInfinity(eval.Hessian[ii, jj]))
                        throw new EvaluationException("Non-finite Hessian returned.", eval);
                }
            }
        }
    }
}
