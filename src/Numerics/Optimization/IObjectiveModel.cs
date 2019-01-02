using MathNet.Numerics.LinearAlgebra;

namespace MathNet.Numerics.Optimization
{
    public interface IObjectiveModelEvaluation
    {
        IObjectiveModel CreateNew();

        /// <summary>
        /// Get the y-values of the fitted model that correspond to the independent values.
        /// </summary>
        Vector<double> Values { get; }

        /// <summary>
        /// Get the values of the parameters.
        /// </summary>
        Vector<double> Parameters { get; }

        /// <summary>
        /// Get the residual sum of squares.
        /// </summary>
        double Residue { get; }

        /// <summary>
        /// Get the Jacobian matrix, J(x; p) = df(x; p)/dp.
        /// </summary>
        Matrix<double> Jacobian { get; }
        /// <summary>
        /// Get the Gradient vector. G = J'(y - f(x; p))
        /// </summary>
        Vector<double> Gradient { get; }
        /// <summary>
        /// Get the approximated Hessian matrix. H = J'J
        /// </summary>
        Matrix<double> Hessian { get; }
        /// <summary>
        /// Get the covariance matrix.
        /// </summary>
        Matrix<double> Covariance { get; }

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

        /// <summary>
        /// Get whether or not the analytical jacobian is supported.
        /// </summary>
        bool IsJacobianSupported { get; }
    }

    public interface IObjectiveModel : IObjectiveModelEvaluation
    {
        void EvaluateFunction(Vector<double> parameters);
        void EvaluateJacobian(Vector<double> parameters);
        void EvaluateCovariance(Vector<double> parameters);

        /// <summary>Create a new independent copy of this objective function, evaluated at the same point.</summary>
        IObjectiveModel Fork();

        IObjectiveFunction ToObjectiveFunction();
    }
}
