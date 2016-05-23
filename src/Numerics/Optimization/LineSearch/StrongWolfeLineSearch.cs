using System;
using MathNet.Numerics.LinearAlgebra;

namespace MathNet.Numerics.Optimization.LineSearch
{
    public class StrongWolfeLineSearch : WolfeLineSearch
    {
        public StrongWolfeLineSearch(double c1, double c2, double parameterTolerance, int maxIterations = 10)
            : base(c1, c2, parameterTolerance, maxIterations)
        {
            // Validation in base class
        }

        protected override MinimizationResult.ExitCondition WolfeExitCondition { get { return MinimizationResult.ExitCondition.StrongWolfeCriteria; } }

        protected override bool WolfeCondition(double stepDd, double initialDd)
        {
            return Math.Abs(stepDd) > C2 * Math.Abs(initialDd);
        }
    }
}
