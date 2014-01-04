// <copyright file="ObjectiveFunction1D.cs" company="Math.NET">
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
    public class WeakWolfeLineSearch
    {
        public double C1 { get; set; }
        public double C2 { get; set; }
        public double ParameterTolerance { get; set; }
        public int MaximumIterations { get; set; }

        public WeakWolfeLineSearch(double c1, double c2, double parameterTolerance, int maxIterations = 10)
        {
            C1 = c1;
            C2 = c2;
            ParameterTolerance = parameterTolerance;
            MaximumIterations = maxIterations;
        }

        // Implemented following http://www.math.washington.edu/~burke/crs/408/lectures/L9-weak-Wolfe.pdf
        public LineSearchOutput FindConformingStep(IObjectiveFunction objective, IEvaluation startingPoint, Vector<double> searchDirection, double initialStep)
        {

            if (!(objective is ObjectiveChecker))
                objective = new ObjectiveChecker(objective, ValidateValue, ValidateGradient, null);

            double lowerBound = 0.0;
            double upperBound = Double.PositiveInfinity;
            double step = initialStep;

            double initialValue = startingPoint.Value;
            Vector<double> initialGradient = startingPoint.Gradient;

            double initialDd = searchDirection*initialGradient;

            int ii;
            IEvaluation candidateEval = null;
            var reasonForExit = ExitCondition.None;
            for (ii = 0; ii < MaximumIterations; ++ii)
            {
                candidateEval = objective.Evaluate(startingPoint.Point + searchDirection*step);

                double stepDd = searchDirection*candidateEval.Gradient;

                if (candidateEval.Value > initialValue + C1*step*initialDd)
                {
                    upperBound = step;
                    step = 0.5*(lowerBound + upperBound);
                }
                else if (stepDd < C2*initialDd)
                {
                    lowerBound = step;
                    step = Double.IsPositiveInfinity(upperBound) ? 2*lowerBound : 0.5*(lowerBound + upperBound);
                }
                else
                {
                    reasonForExit = ExitCondition.WeakWolfeCriteria;
                    break;
                }

                if (!Double.IsInfinity(upperBound))
                {
                    double maxRelChange = 0.0;
                    for (int jj = 0; jj < candidateEval.Point.Count; ++jj)
                    {
                        double tmp = Math.Abs(searchDirection[jj]*(upperBound - lowerBound))/Math.Max(Math.Abs(candidateEval.Point[jj]), 1.0);
                        maxRelChange = Math.Max(maxRelChange, tmp);
                    }
                    if (maxRelChange < ParameterTolerance)
                    {
                        reasonForExit = ExitCondition.LackOfProgress;
                        break;
                    }
                }
            }

            if (ii == MaximumIterations && Double.IsPositiveInfinity(upperBound))
                throw new MaximumIterationsException(String.Format("Maximum iterations ({0}) reached. Function appears to be unbounded in search direction.", MaximumIterations));
            if (ii == MaximumIterations)
                throw new MaximumIterationsException(String.Format("Maximum iterations ({0}) reached.", MaximumIterations));
            return new LineSearchOutput(candidateEval, ii, step, reasonForExit);
        }

        bool Conforms(IEvaluation startingPoint, Vector<double> searchDirection, double step, IEvaluation endingPoint)
        {
            bool sufficientDecrease = endingPoint.Value <= startingPoint.Value + C1*step*(startingPoint.Gradient*searchDirection);
            bool notTooSteep = endingPoint.Gradient*searchDirection >= C2*startingPoint.Gradient*searchDirection;

            return step > 0 && sufficientDecrease && notTooSteep;
        }

        void ValidateValue(IEvaluation eval)
        {
            if (!IsFinite(eval.Value))
                throw new EvaluationException(String.Format("Non-finite value returned by objective function: {0}", eval.Value), eval);
        }

        void ValidateGradient(IEvaluation eval)
        {
            foreach (double x in eval.Gradient)
                if (!IsFinite(x))
                {
                    throw new EvaluationException(String.Format("Non-finite value returned by gradient: {0}", x), eval);
                }
        }

        bool IsFinite(double x)
        {
            return !(Double.IsNaN(x) || Double.IsInfinity(x));
        }
    }
}
