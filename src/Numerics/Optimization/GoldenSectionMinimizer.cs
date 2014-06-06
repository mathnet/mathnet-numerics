using System;

namespace MathNet.Numerics.Optimization
{
    public class GoldenSectionMinimizer
    {
        public double XTolerance { get; set; }
        public int MaximumIterations { get; set; }
        public int MaximumExpansionSteps { get; set; }
        public double LowerExpansionFactor { get; set; }
        public double UpperExpansionFactor { get; set; }

        public GoldenSectionMinimizer(double xTolerance = 1e-5, int maxIterations = 1000, int maxExpansionSteps = 10, double lowerExpansionFactor = 2.0, double upperExpansionFactor = 2.0)
        {
            XTolerance = xTolerance;
            MaximumIterations = maxIterations;
            MaximumExpansionSteps = maxExpansionSteps;
            LowerExpansionFactor = lowerExpansionFactor;
            UpperExpansionFactor = upperExpansionFactor;
        }

        public MinimizationResult1D FindMinimum(IObjectiveFunction1D objective, double lowerBound, double upperBound)
        {
            if (upperBound <= lowerBound)
                throw new OptimizationException("Lower bound must be lower than upper bound.");

            double middlePointX = lowerBound + (upperBound - lowerBound)/(1 + Constants.GoldenRatio);
            IEvaluation1D lower = objective.Evaluate(lowerBound);
            IEvaluation1D middle = objective.Evaluate(middlePointX);
            IEvaluation1D upper = objective.Evaluate(upperBound);

            ValueChecker(lower.Value, lowerBound);
            ValueChecker(middle.Value, middlePointX);
            ValueChecker(upper.Value, upperBound);

            int expansion_steps = 0;
            while ((expansion_steps < this.MaximumExpansionSteps) && (upper.Value < middle.Value || lower.Value < middle.Value))
            {
                if (lower.Value < middle.Value)
                {
                    lowerBound = 0.5*(upperBound + lowerBound) - this.LowerExpansionFactor*0.5*(upperBound - lowerBound);
                    lower = objective.Evaluate(lowerBound);
                }

                if (upper.Value < middle.Value)
                {
                    upperBound = 0.5*(upperBound + lowerBound) + this.UpperExpansionFactor*0.5*(upperBound - lowerBound);
                    upper = objective.Evaluate(upperBound);
                }

                middlePointX = lowerBound + (upperBound - lowerBound)/(1 + Constants.GoldenRatio);
                middle = objective.Evaluate(middlePointX);

                expansion_steps += 1;
            }

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

        void ValueChecker(double value, double point)
        {
            if (Double.IsNaN(value) || Double.IsInfinity(value))
                throw new Exception("Objective function returned non-finite value.");
        }
    }
}
