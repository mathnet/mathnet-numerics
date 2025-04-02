// <copyright file="ParameterStatistics.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// https://numerics.mathdotnet.com
// https://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-$CURRENT_YEAR$ Math.NET
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
// </copyright>

using MathNet.Numerics.Distributions;
using MathNet.Numerics.LinearAlgebra;
using System;

namespace MathNet.Numerics.Statistics
{
    /// <summary>
    /// Provides statistical measures for parameter estimates based on covariance matrix.
    /// </summary>
    public static class ParameterStatistics
    {
        /// <summary>
        /// Calculates standard errors for parameters from a covariance matrix.
        /// </summary>
        /// <param name="covariance">The covariance matrix of the parameters.</param>
        /// <returns>Vector of standard errors for each parameter.</returns>
        public static Vector<double> StandardErrors(Matrix<double> covariance)
        {
            if (covariance == null)
                throw new ArgumentNullException(nameof(covariance));

            if (covariance.RowCount != covariance.ColumnCount)
                throw new ArgumentException("Covariance matrix must be square.", nameof(covariance));

            return covariance.ToSymmetric().Diagonal().PointwiseSqrt();
        }

        /// <summary>
        /// Calculates t-statistics for parameters (parameter value / standard error).
        /// </summary>
        /// <param name="parameters">The parameter values.</param>
        /// <param name="standardErrors">The standard errors of the parameters.</param>
        /// <returns>Vector of t-statistics for each parameter.</returns>
        public static Vector<double> TStatistics(Vector<double> parameters, Vector<double> standardErrors)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            if (standardErrors == null)
                throw new ArgumentNullException(nameof(standardErrors));

            if (parameters.Count != standardErrors.Count)
                throw new ArgumentException("Parameters and standard errors must have the same length.");

            var result = Vector<double>.Build.Dense(parameters.Count);

            for (var i = 0; i < parameters.Count; i++)
            {
                result[i] = standardErrors[i] > double.Epsilon
                    ? parameters[i] / standardErrors[i]
                    : double.NaN;
            }

            return result;
        }

        /// <summary>
        /// Calculates t-statistics for parameters directly from covariance matrix.
        /// </summary>
        /// <param name="parameters">The parameter values.</param>
        /// <param name="covariance">The covariance matrix of the parameters.</param>
        /// <returns>Vector of t-statistics for each parameter.</returns>
        public static Vector<double> TStatistics(Vector<double> parameters, Matrix<double> covariance)
        {
            var standardErrors = StandardErrors(covariance);
            return TStatistics(parameters, standardErrors);
        }

        /// <summary>
        /// Calculates p-values for parameters based on t-distribution.
        /// </summary>
        /// <param name="tStatistics">The t-statistics for the parameters.</param>
        /// <param name="degreesOfFreedom">The degrees of freedom.</param>
        /// <returns>Vector of p-values for each parameter.</returns>
        public static Vector<double> PValues(Vector<double> tStatistics, int degreesOfFreedom)
        {
            if (tStatistics == null)
                throw new ArgumentNullException(nameof(tStatistics));

            if (degreesOfFreedom < 1)
                throw new ArgumentOutOfRangeException(nameof(degreesOfFreedom), "Degrees of freedom must be positive.");

            var tDist = new StudentT(0, 1, degreesOfFreedom);
            var result = Vector<double>.Build.Dense(tStatistics.Count);

            for (var i = 0; i < tStatistics.Count; i++)
            {
                var tStat = Math.Abs(tStatistics[i]);
                // Two-tailed p-value
                result[i] = double.IsNaN(tStat) ? double.NaN : 2 * (1 - tDist.CumulativeDistribution(tStat));
            }

            return result;
        }

        /// <summary>
        /// Calculates confidence interval half-widths for parameters at the specified confidence level.
        /// </summary>
        /// <param name="standardErrors">The standard errors of the parameters.</param>
        /// <param name="degreesOfFreedom">The degrees of freedom.</param>
        /// <param name="confidenceLevel">The confidence level (between 0 and 1, default is 0.95 for 95% confidence).</param>
        /// <returns>Vector of confidence interval half-widths for each parameter.</returns>
        public static Vector<double> ConfidenceIntervalHalfWidths(
            Vector<double> standardErrors, int degreesOfFreedom, double confidenceLevel = 0.95)
        {
            if (standardErrors == null)
                throw new ArgumentNullException(nameof(standardErrors));

            if (degreesOfFreedom < 1)
                throw new ArgumentOutOfRangeException(nameof(degreesOfFreedom), "Degrees of freedom must be positive.");

            if (confidenceLevel <= 0 || confidenceLevel >= 1)
                throw new ArgumentOutOfRangeException(nameof(confidenceLevel), "Confidence level must be between 0 and 1.");

            var alpha = 1 - confidenceLevel;
            var tDist = new StudentT(0, 1, degreesOfFreedom);
            var tCritical = tDist.InverseCumulativeDistribution(1 - alpha / 2);

            return standardErrors.Multiply(tCritical);
        }

        /// <summary>
        /// Calculates confidence interval half-widths for parameters directly from covariance matrix.
        /// </summary>
        /// <param name="covariance">The covariance matrix of the parameters.</param>
        /// <param name="degreesOfFreedom">The degrees of freedom.</param>
        /// <param name="confidenceLevel">The confidence level (between 0 and 1, default is 0.95 for 95% confidence).</param>
        /// <returns>Vector of confidence interval half-widths for each parameter.</returns>
        public static Vector<double> ConfidenceIntervalHalfWidths(
            Matrix<double> covariance, int degreesOfFreedom, double confidenceLevel = 0.95)
        {
            var standardErrors = StandardErrors(covariance);
            return ConfidenceIntervalHalfWidths(standardErrors, degreesOfFreedom, confidenceLevel);
        }

        /// <summary>
        /// Calculates dependency values for parameters, measuring multicollinearity.
        /// Values close to 1 indicate high dependency between parameters.
        /// </summary>
        /// <param name="correlation">The correlation matrix of the parameters.</param>
        /// <returns>Vector of dependency values for each parameter.</returns>
        public static Vector<double> DependenciesFromCorrelation(Matrix<double> correlation)
        {
            if (correlation == null)
                throw new ArgumentNullException(nameof(correlation));

            if (correlation.RowCount != correlation.ColumnCount)
                throw new ArgumentException("Correlation matrix must be symmetric.", nameof(correlation));

            var symmetricCorrelation = correlation.ToSymmetric();
            var n = symmetricCorrelation.RowCount;
            var result = Vector<double>.Build.Dense(n);

            for (var i = 0; i < n; i++)
            {
                // Extract correlations for parameter i with all other parameters
                var correlations = Vector<double>.Build.Dense(n - 1);
                var index = 0;

                for (var j = 0; j < n; j++)
                {
                    if (j != i)
                    {
                        correlations[index++] = symmetricCorrelation[i, j];
                    }
                }

                // Find maximum squared correlation
                var maxSquaredCorrelation = correlations.PointwiseMultiply(correlations).Maximum();

                // Calculate dependency (1 - 1/VIF)
                var maxSquaredCorrelationCapped = Math.Min(maxSquaredCorrelation, 0.9999);
                var vif = 1.0 / (1.0 - maxSquaredCorrelationCapped);
                result[i] = 1.0 - 1.0 / vif;
            }

            return result;
        }

        /// <summary>
        /// Calculates dependency values for parameters directly from covariance matrix.
        /// </summary>
        /// <param name="covariance">The covariance matrix of the parameters.</param>
        /// <returns>Vector of dependency values for each parameter.</returns>
        public static Vector<double> DependenciesFromCovariance(Matrix<double> covariance)
        {
            var correlation = CorrelationFromCovariance(covariance);
            return DependenciesFromCorrelation(correlation);
        }

        /// <summary>
        /// Calculates correlation matrix from covariance matrix.
        /// </summary>
        /// <param name="covariance">The covariance matrix of the parameters.</param>
        /// <returns>The correlation matrix.</returns>
        public static Matrix<double> CorrelationFromCovariance(Matrix<double> covariance)
        {
            if (covariance == null)
                throw new ArgumentNullException(nameof(covariance));

            if (covariance.RowCount != covariance.ColumnCount)
                throw new ArgumentException("Covariance matrix must be square.", nameof(covariance));

            var symmetricCovariance = covariance.ToSymmetric();
            var d = symmetricCovariance.Diagonal().PointwiseSqrt();
            var dd = d.OuterProduct(d);
            return symmetricCovariance.PointwiseDivide(dd);
        }

        /// <summary>
        /// Computes all parameter statistics at once.
        /// </summary>
        /// <param name="parameters">The parameter values.</param>
        /// <param name="covariance">The covariance matrix of the parameters.</param>
        /// <param name="degreesOfFreedom">The degrees of freedom.</param>
        /// <param name="confidenceLevel">The confidence level (between 0 and 1, default is 0.95 for 95% confidence).</param>
        /// <returns>A tuple containing all parameter statistics.</returns>
        public static (Vector<double> StandardErrors,
                       Vector<double> TStatistics,
                       Vector<double> PValues,
                       Vector<double> ConfidenceIntervalHalfWidths,
                       Vector<double> Dependencies,
                       Matrix<double> Correlation)
            ComputeStatistics(Vector<double> parameters, Matrix<double> covariance, int degreesOfFreedom, double confidenceLevel = 0.95)
        {
            var standardErrors = StandardErrors(covariance);
            var tStatistics = TStatistics(parameters, standardErrors);
            var pValues = PValues(tStatistics, degreesOfFreedom);
            var confidenceIntervals = ConfidenceIntervalHalfWidths(standardErrors, degreesOfFreedom, confidenceLevel);
            var correlation = CorrelationFromCovariance(covariance);
            var dependencies = DependenciesFromCorrelation(correlation);

            return (standardErrors, tStatistics, pValues, confidenceIntervals, dependencies, correlation);
        }

        /// <summary>
        /// Creates a symmetric version of the matrix by averaging elements across the diagonal.
        /// This is useful for covariance matrices that may not be perfectly symmetric 
        /// due to numerical precision issues in computation.
        /// </summary>
        /// <param name="matrix">The matrix to make symmetric.</param>
        /// <returns>A new symmetric matrix.</returns>
        /// <exception cref="ArgumentException">Thrown when the matrix is not square.</exception>
        private static Matrix<double> ToSymmetric(this Matrix<double> matrix)
        {
            if (matrix.RowCount != matrix.ColumnCount)
                throw new ArgumentException("Matrix must be square.", nameof(matrix));

            var result = matrix.Clone();

            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = i + 1; j < matrix.ColumnCount; j++)
                {
                    var avg = (matrix[i, j] + matrix[j, i]) / 2;
                    result[i, j] = avg;
                    result[j, i] = avg;
                }
            }

            return result;
        }

        #region Building Covariance Matrix

        /// <summary>
        /// Computes the parameter covariance matrix for linear regression.
        /// </summary>
        /// <param name="X">The design matrix where each row represents an observation and each column 
        /// represents a feature (including the intercept column of ones if an intercept is included
        /// in the model). For a model y = b0 + b1*x1 + b2*x2 + ... with n observations, X would be an 
        /// n x (p+1) matrix where p is the number of predictor variables.</param>
        /// <param name="residualVariance">The residual variance (SSR/degrees of freedom).</param>
        /// <returns>The parameter covariance matrix, which is a p+1 x p+1 matrix where p is the number 
        /// of predictors in the model (including intercept if present).</returns>
        public static Matrix<double> CovarianceMatrixForLinearRegression(Matrix<double> X, double residualVariance)
        {
            if (X == null)
                throw new ArgumentNullException(nameof(X));

            // Calculate (X'X)^(-1)
            var XtX = X.TransposeThisAndMultiply(X);
            var XtXInverse = XtX.Inverse();

            // Multiply by residual variance to get covariance matrix
            return XtXInverse.Multiply(residualVariance);
        }

        /// <summary>
        /// Computes the parameter covariance matrix for linear regression.
        /// </summary>
        /// <param name="X">The design matrix (each row is an observation, each column is a feature).</param>
        /// <param name="residuals">The residual vector.</param>
        /// <param name="degreesOfFreedom">The degrees of freedom (typically n-p, where n is sample size and p is parameter count).</param>
        /// <returns>The parameter covariance matrix.</returns>
        public static Matrix<double> CovarianceMatrixForLinearRegression(Matrix<double> X, Vector<double> residuals, int degreesOfFreedom)
        {
            if (degreesOfFreedom <= 0)
                throw new ArgumentOutOfRangeException(nameof(degreesOfFreedom), "Degrees of freedom must be positive.");

            // Calculate residual variance (RSS/df)
            var residualVariance = residuals.DotProduct(residuals) / degreesOfFreedom;

            return CovarianceMatrixForLinearRegression(X, residualVariance);
        }

        /// <summary>
        /// Computes the parameter covariance matrix for weighted linear regression.
        /// </summary>
        /// <param name="X">The design matrix (each row is an observation, each column is a feature).</param>
        /// <param name="weights">The weight vector for observations.</param>
        /// <param name="residualVariance">The weighted residual variance.</param>
        /// <returns>The parameter covariance matrix.</returns>
        public static Matrix<double> CovarianceMatrixForWeightedLinearRegression(Matrix<double> X, Vector<double> weights, double residualVariance)
        {
            if (X == null)
                throw new ArgumentNullException(nameof(X));

            if (weights == null)
                throw new ArgumentNullException(nameof(weights));

            if (X.RowCount != weights.Count)
                throw new ArgumentException("The number of rows in X must match the length of the weights vector.");

            // Create weight matrix (diagonal matrix of weights)
            var W = Matrix<double>.Build.DenseOfDiagonalVector(weights);

            // Calculate (X'WX)^(-1)
            var XtWX = X.TransposeThisAndMultiply(W).Multiply(X);
            var XtWXInverse = XtWX.Inverse();

            // Multiply by residual variance to get covariance matrix
            return XtWXInverse.Multiply(residualVariance);
        }

        /// <summary>
        /// Computes the parameter covariance matrix for nonlinear regression from the Jacobian.
        /// </summary>
        /// <param name="jacobian">The Jacobian matrix at the solution.</param>
        /// <param name="residualVariance">The residual variance (SSR/degrees of freedom).</param>
        /// <returns>The parameter covariance matrix.</returns>
        public static Matrix<double> CovarianceMatrixFromJacobian(Matrix<double> jacobian, double residualVariance)
        {
            if (jacobian == null)
                throw new ArgumentNullException(nameof(jacobian));

            // Calculate (J'J)^(-1)
            var JtJ = jacobian.TransposeThisAndMultiply(jacobian);
            var JtJInverse = JtJ.Inverse();

            // Multiply by residual variance to get covariance matrix
            return JtJInverse.Multiply(residualVariance);
        }

        /// <summary>
        /// Computes the parameter covariance matrix for nonlinear regression from the Jacobian.
        /// </summary>
        /// <param name="jacobian">The Jacobian matrix at the solution.</param>
        /// <param name="residuals">The residual vector at the solution.</param>
        /// <param name="degreesOfFreedom">The degrees of freedom (typically n-p, where n is sample size and p is parameter count).</param>
        /// <returns>The parameter covariance matrix.</returns>
        public static Matrix<double> CovarianceMatrixFromJacobian(Matrix<double> jacobian, Vector<double> residuals, int degreesOfFreedom)
        {
            if (residuals == null)
                throw new ArgumentNullException(nameof(residuals));

            if (degreesOfFreedom <= 0)
                throw new ArgumentOutOfRangeException(nameof(degreesOfFreedom), "Degrees of freedom must be positive.");

            // Calculate residual variance (RSS/df)
            var residualVariance = residuals.DotProduct(residuals) / degreesOfFreedom;

            return CovarianceMatrixFromJacobian(jacobian, residualVariance);
        }

        /// <summary>
        /// Computes the parameter covariance matrix for weighted nonlinear regression from the Jacobian.
        /// </summary>
        /// <param name="jacobian">The Jacobian matrix at the solution.</param>
        /// <param name="weights">The weight vector for observations.</param>
        /// <param name="residualVariance">The weighted residual variance.</param>
        /// <returns>The parameter covariance matrix.</returns>
        public static Matrix<double> CovarianceMatrixFromWeightedJacobian(Matrix<double> jacobian, Vector<double> weights, double residualVariance)
        {
            if (jacobian == null)
                throw new ArgumentNullException(nameof(jacobian));

            if (weights == null)
                throw new ArgumentNullException(nameof(weights));

            if (jacobian.RowCount != weights.Count)
                throw new ArgumentException("The number of rows in the Jacobian must match the length of the weights vector.");

            // Apply weights to Jacobian (multiply each row by sqrt(weight))
            var weightedJacobian = jacobian.Clone();
            for (var i = 0; i < jacobian.RowCount; i++)
            {
                var sqrtWeight = Math.Sqrt(weights[i]);
                for (var j = 0; j < jacobian.ColumnCount; j++)
                {
                    weightedJacobian[i, j] *= sqrtWeight;
                }
            }

            // Calculate (J'WJ)^(-1) using the weighted Jacobian
            var JtJ = weightedJacobian.TransposeThisAndMultiply(weightedJacobian);
            var JtJInverse = JtJ.Inverse();

            // Multiply by residual variance to get covariance matrix
            return JtJInverse.Multiply(residualVariance);
        }

        #endregion Building Covariance Matrix
    }
}
