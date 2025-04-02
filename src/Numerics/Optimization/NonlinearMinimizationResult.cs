using MathNet.Numerics.Distributions;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Statistics;
using System;
using System.Linq;

namespace MathNet.Numerics.Optimization
{
    /// <summary>
    /// Represents the result of a nonlinear minimization operation, including
    /// the optimal parameters and various statistical measures of fitness.
    /// </summary>
    public class NonlinearMinimizationResult
    {
        /// <summary>
        /// The objective model evaluated at the minimum point.
        /// </summary>
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
        /// Returns the t-statistics for each parameter (parameter value / standard error).
        /// These measure how many standard deviations each parameter is from zero.
        /// </summary>
        public Vector<double> TStatistics { get; private set; }

        /// <summary>
        /// Returns the p-values for each parameter based on t-distribution.
        /// Lower p-values indicate statistically significant parameters.
        /// </summary>
        public Vector<double> PValues { get; private set; }

        /// <summary>
        /// Returns the dependency values for each parameter, measuring how linearly related
        /// each parameter is to the others. Values close to 1 indicate high dependency (multicollinearity).
        /// </summary>
        public Vector<double> Dependencies { get; private set; }

        /// <summary>
        /// Returns the half-width of the confidence intervals for each parameter at the 95% confidence level.
        /// These represent the margin of error for each parameter estimate.
        /// </summary>
        public Vector<double> ConfidenceIntervalHalfWidths { get; private set; }

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

        /// <summary>
        /// Returns the residual sum of squares at the minimum point, 
        /// calculated as F(p) = 1/2 * ∑(residuals²).
        /// </summary>
        public double MinimizedValue => ModelInfoAtMinimum.Value;

        /// <summary>
        /// Root Mean Squared Error (RMSE) - measures the average magnitude of errors.
        /// </summary>
        public double RootMeanSquaredError { get; private set; }

        /// <summary>
        /// Coefficient of determination (R-Square) - proportion of variance explained by the model.
        /// </summary>
        public double RSquared { get; private set; }

        /// <summary>
        /// Adjusted R-Squre - accounts for the number of predictors in the model.
        /// </summary>
        public double AdjustedRSquared { get; private set; }

        /// <summary>
        /// Residual standard error of the regression, calculated as sqrt(RSS/df) where RSS is the 
        /// residual sum of squares and df is the degrees of freedom. This measures the average 
        /// distance between the observed values and the fitted model.
        /// </summary>
        public double StandardError { get; private set; }

        /// <summary>
        /// Pearson correlation coefficient between observed and predicted values.
        /// </summary>
        public double CorrelationCoefficient { get; private set; }

        /// <summary>
        /// Number of iterations performed during optimization.
        /// </summary>
        public int Iterations { get; }

        /// <summary>
        /// Reason why the optimization algorithm terminated.
        /// </summary>
        public ExitCondition ReasonForExit { get; }

        /// <summary>
        /// Creates a new instance of the NonlinearMinimizationResult class.
        /// </summary>
        /// <param name="modelInfo">The objective model at the minimizing point.</param>
        /// <param name="iterations">The number of iterations performed.</param>
        /// <param name="reasonForExit">The reason why the algorithm terminated.</param>
        public NonlinearMinimizationResult(IObjectiveModel modelInfo, int iterations, ExitCondition reasonForExit)
        {
            ModelInfoAtMinimum = modelInfo;
            Iterations = iterations;
            ReasonForExit = reasonForExit;

            EvaluateCovariance(modelInfo);
            EvaluateGoodnessOfFit(modelInfo);
        }

        /// <summary>
        /// Evaluates the covariance matrix, correlation matrix, standard errors, t-statistics and p-values.
        /// </summary>
        /// <param name="objective">The objective model at the minimizing point.</param>
        void EvaluateCovariance(IObjectiveModel objective)
        {
            objective.EvaluateAt(objective.Point); // Hessian may be not yet updated.

            var Hessian = objective.Hessian;
            if (Hessian == null || objective.DegreeOfFreedom < 1)
            {
                Covariance = null;
                Correlation = null;
                StandardErrors = null;
                TStatistics = null;
                PValues = null;
                ConfidenceIntervalHalfWidths = null;
                Dependencies = null;
                return;
            }

            // The factor of 2.0 compensates for the 1/2 factor in the objective function definition
            // F(p) = 1/2 * ∑{ Wi * (yi - f(xi; p))^2 }
            // Without this compensation, the covariance and standard errors would be underestimated by a factor of 2
            Covariance = 2.0 * Hessian.PseudoInverse() * objective.Value / objective.DegreeOfFreedom;

            if (Covariance != null)
            {
                // Use ParameterStatistics class to compute all statistics at once
                var stats = ParameterStatistics.ComputeStatistics(
                    objective.Point,
                    Covariance,
                    objective.DegreeOfFreedom
                );

                StandardErrors = stats.StandardErrors;
                TStatistics = stats.TStatistics;
                PValues = stats.PValues;
                ConfidenceIntervalHalfWidths = stats.ConfidenceIntervalHalfWidths;
                Correlation = stats.Correlation;
                Dependencies = stats.Dependencies;
            }
        }

        /// <summary>
        /// Evaluates goodness of fit statistics like R-squared, RMSE, etc.
        /// </summary>
        /// <param name="objective">The objective model at the minimizing point.</param>
        void EvaluateGoodnessOfFit(IObjectiveModel objective)
        {
            // Note: GoodnessOfFit class methods do not support weights, so we apply weighting manually here.

            // Check if we have the essentials for calculating statistics
            var hasResiduals = objective.Residuals != null;
            var hasObservations = objective.ObservedY != null;
            var hasModelValues = objective.ModelValues != null;
            var hasSufficientDof = objective.DegreeOfFreedom >= 1;

            // Set values to NaN if we can't calculate them
            RootMeanSquaredError = double.NaN;
            RSquared = double.NaN;
            AdjustedRSquared = double.NaN;
            StandardError = double.NaN;
            CorrelationCoefficient = double.NaN;

            // Need residuals and sufficient DOF for most calculations
            if (!hasResiduals || !hasSufficientDof)
            {
                return;
            }
            
            var n = hasObservations ? objective.ObservedY.Count : objective.Residuals.Count;
            var dof = objective.DegreeOfFreedom;

            // Calculate sum of squared residuals
            var ssRes = 2.0 * objective.Value;

            // Guard against zero or negative SSR
            if (ssRes <= 0)
            {
                RootMeanSquaredError = 0;
                StandardError = 0;

                // Only calculate these if we have observations
                if (hasObservations)
                {
                    RSquared = 1.0;
                    AdjustedRSquared = 1.0;
                }

                // Only calculate if we have model values and observations
                if (hasModelValues && hasObservations)
                {
                    CorrelationCoefficient = 1.0;
                }
                return;
            }

            // Calculate standard error and RMSE, which only require residuals
            StandardError = Math.Sqrt(ssRes / dof);
            RootMeanSquaredError = Math.Sqrt(ssRes / n);

            // The following statistics require observations
            if (!hasObservations)
            {
                return;
            }

            // Calculate total sum of squares
            double ssTot;

            // If weights are present, calculate weighted total sum of squares
            if (objective.Weights != null)
            {
                var weightSum = 0.0;
                var weightedSum = 0.0;

                for (var i = 0; i < n; i++)
                {
                    var weight = objective.Weights[i, i];
                    weightSum += weight;
                    weightedSum += weight * objective.ObservedY[i];
                }

                // Avoid division by zero
                var weightedMean = weightSum > double.Epsilon ? weightedSum / weightSum : 0;

                ssTot = 0.0;
                for (var i = 0; i < n; i++)
                {
                    var weight = objective.Weights[i, i];
                    var dev = objective.ObservedY[i] - weightedMean;
                    ssTot += weight * dev * dev;
                }
            }
            else
            {
                // Unweighted case - use vector operations for total sum of squares
                var yMean = objective.ObservedY.Average();
                var deviations = objective.ObservedY.Subtract(yMean);
                ssTot = deviations.DotProduct(deviations);
            }

            // Guard against zero or negative total sum of squares
            if (ssTot <= double.Epsilon)
            {
                RSquared = 0.0;
                AdjustedRSquared = 0.0;
            }
            else
            {
                // Calculate R-squared directly
                RSquared = 1 - (ssRes / ssTot);

                // Calculate adjusted R-squared using the ratio of mean squares
                AdjustedRSquared = 1 - (ssRes / dof) / (ssTot / (n - 1));

                // Ensure adjusted R-squared is not greater than 1 or less than 0
                AdjustedRSquared = Math.Min(1.0, Math.Max(0.0, AdjustedRSquared));
            }

            // Only calculate correlation coefficient if we have model values
            if (hasModelValues && hasObservations)
            {
                // Calculate correlation coefficient between observed and predicted
                CorrelationCoefficient = GoodnessOfFit.R(objective.ModelValues, objective.ObservedY);
            }
        }
    }
}
