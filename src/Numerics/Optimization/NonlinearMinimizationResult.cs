using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MathNet.Numerics.Optimization
{
    public class NonlinearMinimizationResult
    {
        public IObjectiveModel ModelInfoAtMinimum { get; private set; }

        /// <summary>
        /// Returns the best fit parameters.
        /// </summary>
        public Vector<double> BestFitParameters { get { return ModelInfoAtMinimum.Point; } }

        /// <summary>
        /// Returns the standard errors of the corresponding parameters 
        /// </summary>
        public Vector<double> StandardErrors { get; private set; }

        /// <summary>
        /// Returns the y-values of the fitted model that correspond to the independent values.
        /// </summary>
        public Vector<double> BestFitValues { get { return ModelInfoAtMinimum.ModelValues; } }

        /// <summary>
        /// Returns the residual sum of squares.
        /// </summary>
        public double Residue { get { return ModelInfoAtMinimum.Value; } }
        public double DegreeOfFreedom { get { return ModelInfoAtMinimum.DegreeOfFreedom; } }
        public Matrix<double> Covariance { get; private set; }
        public Matrix<double> Correlation { get; private set; }

        public int Iterations { get; private set; }
        public ExitCondition ReasonForExit { get; private set; }

        public NonlinearMinimizationResult(IObjectiveModel modelInfo, int iterations, ExitCondition reasonForExit)
        {
            ModelInfoAtMinimum = modelInfo;
            Iterations = iterations;
            ReasonForExit = reasonForExit;

            AnalyzeResult(modelInfo);
        }

        private void AnalyzeResult(IObjectiveModel objective)
        {
            objective.IsFinished = true;
            objective.EvaluateAt(objective.Point);

            var Hessian = objective.Hessian;
            if (Hessian == null || DegreeOfFreedom < 1)
            {
                Covariance = null;
                Correlation = null;
                StandardErrors = null;
                return;
            }

            Covariance = Hessian.PseudoInverse() * objective.Value / DegreeOfFreedom;

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
