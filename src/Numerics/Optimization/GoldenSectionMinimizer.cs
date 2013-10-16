using System;

namespace MathNet.Numerics.Optimization
{
    public class GoldenSectionMinimizer
    {
        public double XTolerance { get; set; }
        public int MaximumIterations { get; set; }

        public GoldenSectionMinimizer(double xTolerance=1e-5, int maxIterations=1000)
        {
            XTolerance = xTolerance;
            MaximumIterations = maxIterations;
        }

        public MinimizationResult1D FindMinimum(IObjectiveFunction1D objective, double lowerBound, double upperBound)
        {
            double middlePointX = lowerBound + (upperBound - lowerBound) / (1 + Constants.GoldenRatio);
            IEvaluation1D lower = objective.Evaluate(lowerBound);
            IEvaluation1D middle = objective.Evaluate(middlePointX);
            IEvaluation1D upper = objective.Evaluate(upperBound);

            ValueChecker(lower.Value, lowerBound);
            ValueChecker(middle.Value, middlePointX);
            ValueChecker(upper.Value, upperBound);

            if (upperBound <= lowerBound)
                throw new OptimizationException("Lower bound must be lower than upper bound.");

            if (upper.Value < middle.Value || lower.Value < middle.Value)
                throw new OptimizationException("Lower and upper bounds do not necessarily bound a minimum.");

            int iterations = 0;
            while (Math.Abs(upper.Point - lower.Point) > XTolerance && iterations < MaximumIterations)
            {
                double testX = lower.Point + (upper.Point - middle.Point);
                var test = objective.Evaluate(testX);
                ValueChecker(test.Value, testX);

                if (test.Point < middle.Point)
                {
                    if (test.Value > middle.Value)
                    {
                        lower = test;
                    }
                    else
                    {
                        upper = middle;
                        middle = test;
                    }
                }
                else
                {
                    if (test.Value > middle.Value)
                    {
                        upper = test;
                    }
                    else
                    {
                        lower = middle;
                        middle = test;
                    }
                }

                iterations += 1;
            }

            if (iterations == MaximumIterations)
                throw new MaximumIterationsException("Max iterations reached.");

            return new MinimizationResult1D(middle, iterations, MinimizationResult.ExitCondition.BoundTolerance);
        }

        private void ValueChecker(double value, double point)
        {
            if (Double.IsNaN(value) || Double.IsInfinity(value))
                throw new Exception("Objective function returned non-finite value.");
        }
    }
}
