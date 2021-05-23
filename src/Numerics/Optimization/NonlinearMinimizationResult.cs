using MathNet.Numerics.LinearAlgebra;

namespace MathNet.Numerics.Optimization
{
    public class NonlinearMinimizationResult
    {
        public IObjectiveModel ModelInfoAtMinimum { get; }

        /// <summary>
        /// Returns the best fit parameters.
        /// </summary>
        public Vector<double> MinimizingPoint => ModelInfoAtMinimum.Point;

        /// <summary>
        /// Returns the standard errors of the corresponding parameters
        /// </summary>
        public Vector<double> StandardErrors { get; private set; }

        /// <summary>
        /// Returns the y-values of the fitted model that correspond to the independent values.
        /// </summary>
        public Vector<double> MinimizedValues => ModelInfoAtMinimum.ModelValues;

        /// <summary>
        /// Returns the covariance matrix at minimizing point.
        /// </summary>
        public Matrix<double> Covariance { get; private set; }

        /// <summary>
        ///  Returns the correlation matrix at minimizing point.
        /// </summary>
        public Matrix<double> Correlation { get; private set; }

        public int Iterations { get; }

        public ExitCondition ReasonForExit { get; }

        public NonlinearMinimizationResult(IObjectiveModel modelInfo, int iterations, ExitCondition reasonForExit)
        {
            ModelInfoAtMinimum = modelInfo;
            Iterations = iterations;
            ReasonForExit = reasonForExit;

            EvaluateCovariance(modelInfo);
        }

        void EvaluateCovariance(IObjectiveModel objective)
        {
            objective.EvaluateAt(objective.Point); // Hessian may be not yet updated.

            var Hessian = objective.Hessian;
            if (Hessian == null || objective.DegreeOfFreedom < 1)
            {
                Covariance = null;
                Correlation = null;
                StandardErrors = null;
                return;
            }

            Covariance = Hessian.PseudoInverse() * objective.Value / objective.DegreeOfFreedom;

            if (Covariance != null)
            {
                StandardErrors = Covariance.Diagonal().PointwiseSqrt();

                var correlation = Covariance.Clone();
                var d = correlation.Diagonal().PointwiseSqrt();
                var dd = d.OuterProduct(d);
                Correlation = correlation.PointwiseDivide(dd);
            }
            else
            {
                StandardErrors = null;
                Correlation = null;
            }
        }
    }
}
