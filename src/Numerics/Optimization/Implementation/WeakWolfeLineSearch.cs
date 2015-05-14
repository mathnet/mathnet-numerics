using System;
using MathNet.Numerics.LinearAlgebra;

namespace MathNet.Numerics.Optimization.Implementation
{
    public class WeakWolfeLineSearch
    {
        readonly double _c1;
        readonly double _c2;
        readonly double _parameterTolerance;
        readonly int _maximumIterations;

        public WeakWolfeLineSearch(double c1, double c2, double parameterTolerance, int maxIterations = 10)
        {
            _c1 = c1;
            _c2 = c2;
            _parameterTolerance = parameterTolerance;
            _maximumIterations = maxIterations;
        }

        // Implemented following http://www.math.washington.edu/~burke/crs/408/lectures/L9-weak-Wolfe.pdf
        public LineSearchOutput FindConformingStep(IEvaluation objective, IEvaluation startingPoint, Vector<double> searchDirection, double initialStep)
        {
            if (!(objective is CheckedEvaluation))
            {
                objective = new CheckedEvaluation(objective, ValidateValue, ValidateGradient, null);
            }

            double lowerBound = 0.0;
            double upperBound = Double.PositiveInfinity;
            double step = initialStep;

            double initialValue = startingPoint.Value;
            Vector<double> initialGradient = startingPoint.Gradient;

            double initialDd = searchDirection * initialGradient;

            int ii;
            IEvaluation candidateEval = objective.CreateNew();
            MinimizationOutput.ExitCondition reasonForExit = MinimizationOutput.ExitCondition.None;
            for (ii = 0; ii < _maximumIterations; ++ii)
            {
                candidateEval.Point = startingPoint.Point + searchDirection * step;

                double stepDd = searchDirection * candidateEval.Gradient;

                if (candidateEval.Value > initialValue + _c1 * step * initialDd)
                {
                    upperBound = step;
                    step = 0.5 * (lowerBound + upperBound);
                }
                else if (stepDd < _c2 * initialDd)
                {
                    lowerBound = step;
                    step = Double.IsPositiveInfinity(upperBound) ? 2 * lowerBound : 0.5 * (lowerBound + upperBound);
                }
                else
                {
                    reasonForExit = MinimizationOutput.ExitCondition.WeakWolfeCriteria;
                    break;
                }

                if (!Double.IsInfinity(upperBound))
                {
                    double maxRelChange = 0.0;
                    for (int jj = 0; jj < candidateEval.Point.Count; ++jj)
                    {
                        double tmp = Math.Abs(searchDirection[jj] * (upperBound - lowerBound)) / Math.Max(Math.Abs(candidateEval.Point[jj]), 1.0);
                        maxRelChange = Math.Max(maxRelChange, tmp);
                    }
                    if (maxRelChange < _parameterTolerance)
                    {
                        reasonForExit = MinimizationOutput.ExitCondition.LackOfProgress;
                        break;
                    }
                }
            }

            if (ii == _maximumIterations && Double.IsPositiveInfinity(upperBound))
            {
                throw new MaximumIterationsException(String.Format("Maximum iterations ({0}) reached. Function appears to be unbounded in search direction.", _maximumIterations));
            }

            if (ii == _maximumIterations)
            {
                throw new MaximumIterationsException(String.Format("Maximum iterations ({0}) reached.", _maximumIterations));
            }

            return new LineSearchOutput(candidateEval, ii, step, reasonForExit);
        }

        bool Conforms(IEvaluation startingPoint, Vector<double> searchDirection, double step, IEvaluation endingPoint)
        {
            bool sufficientDecrease = endingPoint.Value <= startingPoint.Value + _c1 * step * (startingPoint.Gradient * searchDirection);
            bool notTooSteep = endingPoint.Gradient * searchDirection >= _c2 * startingPoint.Gradient * searchDirection;

            return step > 0 && sufficientDecrease && notTooSteep;
        }

        void ValidateValue(IEvaluation eval)
        {
            if (!IsFinite(eval.Value))
            {
                throw new EvaluationException(String.Format("Non-finite value returned by objective function: {0}", eval.Value), eval);
            }
        }

        void ValidateGradient(IEvaluation eval)
        {
            foreach (double x in eval.Gradient)
            {
                if (!IsFinite(x))
                {
                    throw new EvaluationException(String.Format("Non-finite value returned by gradient: {0}", x), eval);
                }
            }
        }

        bool IsFinite(double x)
        {
            return !(Double.IsNaN(x) || Double.IsInfinity(x));
        }
    }
}
