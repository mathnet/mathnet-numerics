using MathNet.Numerics.LinearAlgebra;
using System.Collections.Generic;

namespace MathNet.Numerics.Optimization
{
    public interface IObjectiveModelEvaluation
    {
        IObjectiveModel CreateNew();

        /// <summary>
        /// Get the y-values of the observations.
        /// </summary>
        Vector<double> ObservedY { get; }

        /// <summary>
        /// Get the values of the weights for the observations.
        /// </summary>
        Matrix<double> Weights { get; }

        /// <summary>
        /// Get the y-values of the fitted model that correspond to the independent values.
        /// </summary>
        Vector<double> ModelValues { get; }

        /// <summary>
        /// Get the values of the parameters.
        /// </summary>
        Vector<double> Point { get; }

        /// <summary>
        /// Get the residual sum of squares.
        /// </summary>
        double Value { get; }

        /// <summary>
        /// Get the Gradient vector. G = J'(y - f(x; p))
        /// </summary>
        Vector<double> Gradient { get; }

        /// <summary>
        /// Get the approximated Hessian matrix. H = J'J
        /// </summary>
        Matrix<double> Hessian { get; }

        /// <summary>
        /// Get the number of calls to function.
        /// </summary>
        int FunctionEvaluations { get; set; }

        /// <summary>
        /// Get the number of calls to jacobian.
        /// </summary>
        int JacobianEvaluations { get; set; }

        /// <summary>
        /// Get the degree of freedom.
        /// </summary>
        int DegreeOfFreedom { get; }

        bool IsGradientSupported { get; }
        bool IsHessianSupported { get; }
    }

    public interface IObjectiveModel : IObjectiveModelEvaluation
    {
        void SetParameters(Vector<double> initialGuess, List<bool> isFixed = null);

        void EvaluateAt(Vector<double> parameters);

        IObjectiveModel Fork();

        IObjectiveFunction ToObjectiveFunction();
    }
}
