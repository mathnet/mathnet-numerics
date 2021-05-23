// <copyright file="BfgsMinimizerBase.cs" company="Math.NET">
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
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Optimization.LineSearch;

namespace MathNet.Numerics.Optimization
{
    public abstract class BfgsMinimizerBase : MinimizerBase
    {
        /// <inheritdoc />
        /// <summary>
        /// Creates a base class for BFGS minimization
        /// </summary>
        protected BfgsMinimizerBase(double gradientTolerance, double parameterTolerance, double functionProgressTolerance, int maximumIterations) : base(gradientTolerance, parameterTolerance, functionProgressTolerance, maximumIterations)
        {
        }


        protected int DoBfgsUpdate(ref ExitCondition currentExitCondition, WolfeLineSearch lineSearcher, ref Matrix<double> inversePseudoHessian, ref Vector<double> lineSearchDirection, ref IObjectiveFunction previousPoint, ref LineSearchResult lineSearchResult, ref IObjectiveFunction candidate, ref Vector<double> step, ref int totalLineSearchSteps, ref int iterationsWithNontrivialLineSearch)
        {
            int iterations;
            for (iterations = 1; iterations < MaximumIterations; ++iterations)
            {
                lineSearchDirection = CalculateSearchDirection(ref inversePseudoHessian, out var maxLineSearchStep, out var startingStepSize, previousPoint, candidate, step);

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
