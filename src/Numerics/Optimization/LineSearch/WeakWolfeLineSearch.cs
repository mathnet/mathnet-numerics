using System;
using MathNet.Numerics.LinearAlgebra;

namespace MathNet.Numerics.Optimization.LineSearch
{
    /// <summary>
    /// Search for a step size alpha that satisfies the weak wolfe conditions. The weak Wolfe
    /// Conditions are
    /// i)  Armijo Rule:         f(x_k + alpha_k p_k) <= f(x_k) + c1 alpha_k p_k^T g(x_k)
    /// ii) Curvature Condition: p_k^T g(x_k + alpha_k p_k) >= c2 p_k^T g(x_k)
    /// where g(x) is the gradient of f(x), 0 < c1 < c2 < 1.
    ///
    /// Implementation is based on http://www.math.washington.edu/~burke/crs/408/lectures/L9-weak-Wolfe.pdf
    ///
    /// references:
    /// http://en.wikipedia.org/wiki/Wolfe_conditions
    /// http://www.math.washington.edu/~burke/crs/408/lectures/L9-weak-Wolfe.pdf
    /// </summary>
    public class WeakWolfeLineSearch : WolfeLineSearch
    {
        public WeakWolfeLineSearch(double c1, double c2, double parameterTolerance, int maxIterations = 10)
            : base(c1,c2,parameterTolerance,maxIterations)
        {
            // Validation in base class
        }

        protected override MinimizationResult.ExitCondition WolfeExitCondition { get { return MinimizationResult.ExitCondition.WeakWolfeCriteria; } }

        protected override bool WolfeCondition(double stepDd, double initialDd)
        {
            return stepDd < C2 * initialDd;
        }

        protected override void ValidateValue(IObjectiveFunction eval)
        {
            if (!IsFinite(eval.Value))
            {
                throw new EvaluationException(String.Format("Non-finite value returned by objective function: {0}", eval.Value), eval);
            }
        }

        protected override void ValidateInputArguments(IObjectiveFunctionEvaluation startingPoint, Vector<double> searchDirection, double initialStep, double upperBound)
        {
            if (!startingPoint.IsGradientSupported)
                throw new ArgumentException("objective function does not support gradient");
        }

        protected override void ValidateGradient(IObjectiveFunction eval)
        {
            foreach (double x in eval.Gradient)
            {
                if (!IsFinite(x))
                {
                    throw new EvaluationException(string.Format("Non-finite value returned by gradient: {0}", x), eval);
                }
            }
        }

        static bool IsFinite(double x)
        {
            return !(double.IsNaN(x) || double.IsInfinity(x));
        }
    }
}
