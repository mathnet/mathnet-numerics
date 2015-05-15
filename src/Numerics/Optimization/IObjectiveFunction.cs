using MathNet.Numerics.LinearAlgebra;

namespace MathNet.Numerics.Optimization
{
    /// <summary>
    /// Objective function with a frozen evaluation that must not be changed from the outside.
    /// </summary>
    public interface IObjectiveFunctionEvaluation
    {
        /// <summary>Create a new unevaluated and independent copy of this objective function</summary>
        IObjectiveFunction CreateNew();

        /// <summary>Create a new independent copy of this objective function, evaluated at the same point.</summary>
        IObjectiveFunction Fork();

        Vector<double> Point { get; }
        double Value { get; }

        bool IsGradientSupported { get; }
        Vector<double> Gradient { get; }

        bool IsHessianSupported { get; }
        Matrix<double> Hessian { get; }
    }

    /// <summary>
    /// Objective function with a mutable evaluation.
    /// </summary>
    public interface IObjectiveFunction : IObjectiveFunctionEvaluation
    {
        void EvaluateAt(Vector<double> point);
    }
}
