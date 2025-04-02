using MathNet.Numerics.LinearAlgebra;
using System.Collections.Generic;

namespace MathNet.Numerics.Optimization
{
    /// <summary>
    /// Interface for objective model evaluation, providing access to optimization-related values
    /// </summary>
    public interface IObjectiveModelEvaluation
    {
        /// <summary>
        /// Creates a new instance of the objective model with the same function but not the same state
        /// </summary>
        /// <returns>A new instance of an objective model</returns>
        IObjectiveModel CreateNew();

        /// <summary>
        /// Get the y-values of the observations.
        /// May be null when using direct residual functions.
        /// </summary>
        Vector<double> ObservedY { get; }

        /// <summary>
        /// Get the values of the weights for the observations.
        /// May be null when using direct residual functions.
        /// </summary>
        Matrix<double> Weights { get; }

        /// <summary>
        /// Get the y-values of the fitted model that correspond to the independent values.
        /// Only applicable when using model functions, may be null when using direct residual functions.
        /// </summary>
        Vector<double> ModelValues { get; }

        /// <summary>
        /// Get the residual values at the current parameters.
        /// For model functions, this is (y - f(x;p)) possibly weighted.
        /// For direct residual functions, this is the raw output of the residual function.
        /// </summary>
        Vector<double> Residuals { get; }

        /// <summary>
        /// Get the values of the parameters.
        /// </summary>
        Vector<double> Point { get; }

        /// <summary>
        /// Get the residual sum of squares, calculated as F(p) = 1/2 * ∑(residuals²).
        /// </summary>
        double Value { get; }

        /// <summary>
        /// Get the Gradient vector. G = J'(y - f(x; p)) for model functions, 
        /// or G = J'r for direct residual functions.
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
        /// For model functions: (number of observations - number of parameters + number of fixed parameters)
        /// For direct residual functions: uses explicit observation count or residual vector length if not provided.
        /// </summary>
        int DegreeOfFreedom { get; }

        /// <summary>
        /// Indicates whether gradient information is supported
        /// </summary>
        bool IsGradientSupported { get; }

        /// <summary>
        /// Indicates whether Hessian information is supported
        /// </summary>
        bool IsHessianSupported { get; }
    }

    /// <summary>
    /// Interface for objective model that can be minimized
    /// </summary>
    public interface IObjectiveModel : IObjectiveModelEvaluation
    {
        /// <summary>
        /// Set parameters and optionally specify which parameters should be fixed
        /// </summary>
        /// <param name="initialGuess">The initial values of parameters</param>
        /// <param name="isFixed">Optional list specifying which parameters are fixed (true) or free (false)</param>
        void SetParameters(Vector<double> initialGuess, List<bool> isFixed = null);

        /// <summary>
        /// Evaluates the objective model at the specified parameters
        /// </summary>
        /// <param name="parameters">Parameters at which to evaluate</param>
        void EvaluateAt(Vector<double> parameters);

        /// <summary>
        /// Creates a copy of this objective model with the same state
        /// </summary>
        /// <returns>A copy of the objective model</returns>
        IObjectiveModel Fork();

        /// <summary>
        /// Converts this objective model to an objective function for optimization.
        /// Creates a function that calculates 1/2 * sum(residuals²) to be minimized.
        /// </summary>
        /// <returns>An IObjectiveFunction that can be used with general optimization algorithms</returns>
        IObjectiveFunction ToObjectiveFunction();
    }
}
