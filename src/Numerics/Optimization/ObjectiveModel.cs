using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Optimization.ObjectiveModels;
using System;

namespace MathNet.Numerics.Optimization
{
    public static class ObjectiveModel
    {
        /// <summary>
        /// Fitting model with a user supplied jacobian for non-linear least squares regression.
        /// </summary>
        public static IObjectiveModel FittingModel(Func<Vector<double>, double, double> function, Func<Vector<double>, double, Vector<double>> derivatives,
            Vector<double> observedX, Vector<double> observedY, Vector<double> weight = null)
        {
            var objective = new FittingObjectiveModel(function, derivatives);
            objective.SetObserved(observedX, observedY, weight);
            return objective;
        }

        /// <summary>
        /// Fitting model for non-linear least squares regression.
        /// </summary>
        public static IObjectiveModel FittingModel(Func<Vector<double>, double, double> function,
            Vector<double> observedX, Vector<double> observedY, Vector<double> weight = null,
            int accuracyOrder = 2)
        {
            var objective = new FittingObjectiveModel(function, accuracyOrder: accuracyOrder);
            objective.SetObserved(observedX, observedY, weight);
            return objective;
        }

        /// <summary>
        /// Fitting function with a user supplied jacobian for nonlinear least squares regression by the line search algorithm.
        /// </summary>
        public static IObjectiveFunction FittingFunction(Func<Vector<double>, double, double> function, Func<Vector<double>, double, Vector<double>> derivatives,
            Vector<double> observedX, Vector<double> observedY, Vector<double> weight = null)
        {
            var objective = new FittingObjectiveModel(function, derivatives);
            objective.SetObserved(observedX, observedY, weight);
            return objective.ToObjectiveFunction();
        }

        /// <summary>
        /// Fitting function for nonlinear least squares regression by the line search algorithm.
        /// The numerical jacobian with accuracy order is used.
        /// </summary>
        public static IObjectiveFunction FittingFunction(Func<Vector<double>, double, double> function,
            Vector<double> observedX, Vector<double> observedY, Vector<double> weight = null,
            int accuracyOrder = 2)
        {
            var objective = new FittingObjectiveModel(function, null, accuracyOrder: accuracyOrder);
            objective.SetObserved(observedX, observedY, weight);
            return objective.ToObjectiveFunction();
        }
    }
}
