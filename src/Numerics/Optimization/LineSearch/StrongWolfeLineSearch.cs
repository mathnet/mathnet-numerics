using System;
using MathNet.Numerics.LinearAlgebra;

namespace MathNet.Numerics.Optimization.LineSearch
{
    public class StrongWolfeLineSearch
    {
        public double C1 { get; set; }
        public double C2 { get; set; }
        public double ParameterTolerance { get; set; }
        public int MaximumIterations { get; set; }

        public StrongWolfeLineSearch(double c1, double c2, double parameterTolerance, int maxIterations = 10)
        {
            C1 = c1;
            C2 = c2;
            ParameterTolerance = parameterTolerance;
            MaximumIterations = maxIterations;
        }

        // Implemented following http://www.math.washington.edu/~burke/crs/408/lectures/L9-weak-Wolfe.pdf
        public LineSearchResult FindConformingStep(IObjectiveFunctionEvaluation objective, Vector<double> searchDirection, double initialStep, double upperBound = Double.PositiveInfinity)
        {
            double lowerBound = 0.0;
            double step = initialStep;

            double initialValue = objective.Value;
            Vector<double> initialGradient = objective.Gradient;

            double initialDd = searchDirection*initialGradient;

            int ii;
            IObjectiveFunction candidateEval = objective.CreateNew();
            MinimizationResult.ExitCondition reasonForExit = MinimizationResult.ExitCondition.None;
            for (ii = 0; ii < this.MaximumIterations; ++ii)
            {
                candidateEval.EvaluateAt(objective.Point + searchDirection*step);

                double stepDd = searchDirection*candidateEval.Gradient;

                if (candidateEval.Value > initialValue + C1*step*initialDd)
                {
                    upperBound = step;
                    step = 0.5*(lowerBound + upperBound);
                }
                else if (Math.Abs(stepDd) > C2*Math.Abs(initialDd))
                {
                    lowerBound = step;
                    step = Double.IsPositiveInfinity(upperBound) ? 2*lowerBound : 0.5*(lowerBound + upperBound);
                }
                else
                {
                    reasonForExit = MinimizationResult.ExitCondition.StrongWolfeCriteria;
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
                        reasonForExit = MinimizationResult.ExitCondition.LackOfProgress;
                        break;
                    }
                }
            }

            if (ii == MaximumIterations && Double.IsPositiveInfinity(upperBound))
                throw new MaximumIterationsException(String.Format("Maximum iterations ({0}) reached. Function appears to be unbounded in search direction.", MaximumIterations));
            if (ii == MaximumIterations)
                throw new MaximumIterationsException(String.Format("Maximum iterations ({0}) reached.", MaximumIterations));

            return new LineSearchResult(candidateEval, ii, step, reasonForExit);
        }

        bool Conforms(IObjectiveFunction startingPoint, Vector<double> searchDirection, double step, IObjectiveFunction endingPoint)
        {
            bool sufficientDecrease = endingPoint.Value <= startingPoint.Value + C1*step*(startingPoint.Gradient*searchDirection);
            bool notTooSteep = endingPoint.Gradient*searchDirection >= C2*startingPoint.Gradient*searchDirection;

            return step > 0 && sufficientDecrease && notTooSteep;
        }

        void ValidateValue(IObjectiveFunction eval)
        {
            if (!IsFinite(eval.Value))
                throw new EvaluationException(String.Format("Non-finite value returned by objective function: {0}", eval.Value), eval);
        }

        void ValidateGradient(IObjectiveFunction eval)
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
