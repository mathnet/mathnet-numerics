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

        public MinimizationResult FindMinimum(IObjectiveFunction1D objective, double lowerBound, double upperBound)
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

                if (test.Value > middle.Value)
                {
                    if (test.Point < middle.Point)
                        lower = test;
                    else
                        upper = test;
                }
                else
                {
                    if (test.Point < middle.Point)
                        upper = middle;
                    else
                        lower = middle;
                }

                iterations += 1;
            }

            if (iterations == MaximumIterations)
                throw new MaximumIterationsException("Max iterations reached.");

            return null;
        }

        private void ValueChecker(double value, double point)
        {
            if (Double.IsNaN(value) || Double.IsInfinity(value))
                throw new Exception("Objective function returned non-finite value.");
        }
    }
}
