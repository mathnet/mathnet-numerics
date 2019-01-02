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
        public Vector<double> BestFitParameters { get { return ModelInfoAtMinimum.Parameters; } }

        /// <summary>
        /// Returns the standard errors of the corresponding parameters 
        /// </summary>
        public Vector<double> StandardErrors
        {
            get
            {
                if (ModelInfoAtMinimum.Covariance == null)
                    return null;
                return ModelInfoAtMinimum.Covariance.Diagonal().PointwiseSqrt();
            }
        }

        /// <summary>
        /// Returns the y-values of the fitted model that correspond to the independent values.
        /// </summary>
        public Vector<double> BestFitValues { get { return ModelInfoAtMinimum.Values; } }

        /// <summary>
        /// Returns the residual sum of squares.
        /// </summary>
        public double Residue { get { return ModelInfoAtMinimum.Residue; } }
        public double DegreeOfFreedom {  get { return ModelInfoAtMinimum.DegreeOfFreedom; } }

        public int Iterations { get; private set; }
        public ExitCondition ReasonForExit { get; private set; }

        public NonlinearMinimizationResult(IObjectiveModel modelInfo, int iterations, ExitCondition reasonForExit)
        {
            ModelInfoAtMinimum = modelInfo;
            Iterations = iterations;
            ReasonForExit = reasonForExit;
        }
    }
}
