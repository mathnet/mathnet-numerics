// <copyright file="WolfeLineSearch.cs" company="Math.NET">
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
using static System.FormattableString;

namespace MathNet.Numerics.Optimization.LineSearch
{
    public abstract class WolfeLineSearch
    {
        protected double C1 { get; }
        protected double C2 { get; }
        protected double ParameterTolerance { get; }
        protected int MaximumIterations { get; }

        public WolfeLineSearch(double c1, double c2, double parameterTolerance, int maxIterations = 10)
        {
            if (c1 <= 0)
                throw new ArgumentException(Invariant($"c1 {c1} should be greater than 0"));
            if (c2 <= c1)
                throw new ArgumentException(Invariant($"c1 {c1} should be less than c2 {c2}"));
            if (c2 >= 1)
                throw new ArgumentException(Invariant($"c2 {c2} should be less than 1"));

            C1 = c1;
            C2 = c2;
            ParameterTolerance = parameterTolerance;
            MaximumIterations = maxIterations;
        }

        /// <summary>Implemented following http://www.math.washington.edu/~burke/crs/408/lectures/L9-weak-Wolfe.pdf</summary>
        /// <param name="startingPoint">The objective function being optimized, evaluated at the starting point of the search</param>
        /// <param name="searchDirection">Search direction</param>
        /// <param name="initialStep">Initial size of the step in the search direction</param>
        public LineSearchResult FindConformingStep(IObjectiveFunctionEvaluation startingPoint, Vector<double> searchDirection, double initialStep)
        {
            return FindConformingStep(startingPoint, searchDirection, initialStep, double.PositiveInfinity);
        }

        /// <summary></summary>
        /// <param name="startingPoint">The objective function being optimized, evaluated at the starting point of the search</param>
        /// <param name="searchDirection">Search direction</param>
        /// <param name="initialStep">Initial size of the step in the search direction</param>
        /// <param name="upperBound">The upper bound</param>
        public LineSearchResult FindConformingStep(IObjectiveFunctionEvaluation startingPoint, Vector<double> searchDirection, double initialStep, double upperBound)
        {
            ValidateInputArguments(startingPoint, searchDirection, initialStep, upperBound);

            double lowerBound = 0.0;
            double step = initialStep;

            double initialValue = startingPoint.Value;
            Vector<double> initialGradient = startingPoint.Gradient;

            double initialDd = searchDirection * initialGradient;

            IObjectiveFunction objective = startingPoint.CreateNew();
            int ii;
            ExitCondition reasonForExit = ExitCondition.None;
            for (ii = 0; ii < MaximumIterations; ++ii)
            {
                objective.EvaluateAt(startingPoint.Point + searchDirection * step);
                ValidateGradient(objective);
                ValidateValue(objective);

                double stepDd = searchDirection * objective.Gradient;

                if (objective.Value > initialValue + C1 * step * initialDd)
                {
                    upperBound = step;
                    step = 0.5 * (lowerBound + upperBound);
                }
                else if (WolfeCondition(stepDd,initialDd))
                {
                    lowerBound = step;
                    step = double.IsPositiveInfinity(upperBound) ? 2 * lowerBound : 0.5 * (lowerBound + upperBound);
                }
                else
                {
                    reasonForExit = WolfeExitCondition;
                    break;
                }

                if (!double.IsInfinity(upperBound))
                {
                    double maxRelChange = 0.0;
                    var objectivePoint = objective.Point;
                    for (int jj = 0; jj < objective.Point.Count; ++jj)
                    {
                        double tmp = Math.Abs(searchDirection[jj] * (upperBound - lowerBound)) / Math.Max(Math.Abs(objectivePoint[jj]), 1.0);
                        maxRelChange = Math.Max(maxRelChange, tmp);
                    }
                    if (maxRelChange < ParameterTolerance)
                    {
                        reasonForExit = ExitCondition.LackOfProgress;
                        break;
                    }
                }
            }

            if (ii == MaximumIterations && double.IsPositiveInfinity(upperBound))
            {
                throw new MaximumIterationsException(Invariant($"Maximum iterations ({MaximumIterations}) reached. Function appears to be unbounded in search direction."));
            }

            if (ii == MaximumIterations)
            {
                throw new MaximumIterationsException(Invariant($"Maximum iterations ({MaximumIterations}) reached."));
            }

            return new LineSearchResult(objective, ii, step, reasonForExit);
        }

        protected abstract ExitCondition WolfeExitCondition { get; }

        protected abstract bool WolfeCondition(double stepDd, double initialDd);

        protected virtual void ValidateGradient(IObjectiveFunctionEvaluation objective)
        {
        }

        protected virtual void ValidateValue(IObjectiveFunctionEvaluation objective)
        {
        }

        protected virtual void ValidateInputArguments(IObjectiveFunctionEvaluation startingPoint, Vector<double> searchDirection, double initialStep, double upperBound)
        {
        }
    }
}
