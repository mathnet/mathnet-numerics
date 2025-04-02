// <copyright file="ParameterStatisticsTests.cs" company="Math.NET">
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

using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Statistics;
using NUnit.Framework;
using System;
using System.Linq;

namespace MathNet.Numerics.Tests.StatisticsTests
{
    [TestFixture]
    public class ParameterStatisticsTests
    {
        #region Polynomial Regression Tests

        [Test]
        public void PolynomialRegressionTest()
        {
            // https://github.com/mathnet/mathnet-numerics/discussions/801

            // Y = B0 + B1*X + B2*X^2
            // Parameter Value     Error     t-value    Pr(>|t|)    LCL         UCL         CI half_width
            // --------------------------------------------------------------------------------------------
            // B0        -0.24     3.07019   -0.07817   0.94481     -13.44995   12.96995    13.20995
            // B1        3.46286   2.33969   1.48005    0.27700     -6.60401    13.52972    10.06686
            // B2        2.64286   0.38258   6.90799    0.02032     0.99675     4.28897     1.64611
            // --------------------------------------------------------------------------------------------
            //
            // Fit statistics
            // -----------------------------------------
            // Degree of freedom        2
            // Reduced Chi-Sqr          2.04914
            // Residual Sum of Sqaures  4.09829
            // R Value                  0.99947
            // R-Square(COD)            0.99893
            // Adj. R-Square            0.99786
            // Root-MSE(SD)             1.43148
            // -----------------------------------------

            double[] x = { 1, 2, 3, 4, 5 };
            double[] y = { 6.2, 16.9, 33, 57.5, 82.5 };
            var order = 2;

            var Ns = x.Length;
            var k = order + 1; // number of parameters
            var dof = Ns - k; // degree of freedom

            // Create the [Ns X k] design matrix
            // This matrix transforms the polynomial regression problem into a linear system
            // Each row represents one data point, and columns represent polynomial terms:
            // - First column: constant term (x^0 = 1)
            // - Second column: linear term (x^1)
            // - Third column: quadratic term (x^2)
            // The matrix looks like:
            // [ 1  x1  x1^2 ]
            // [ 1  x2  x2^2 ]
            // [ ...         ]
            // [ 1  xN  xN^2 ]
            var X = Matrix<double>.Build.Dense(Ns, k, (i, j) => Math.Pow(x[i], j));

            // Create the Y vector
            var Y = Vector<double>.Build.DenseOfArray(y);

            // Calculate best-fitted parameters using normal equations
            var XtX = X.TransposeThisAndMultiply(X);
            var XtXInv = XtX.Inverse();
            var Xty = X.TransposeThisAndMultiply(Y);
            var parameters = XtXInv.Multiply(Xty);

            // Calculate the residuals
            var residuals = X.Multiply(parameters) - Y;

            // Calculate residual variance (RSS/dof)
            var RSS = residuals.DotProduct(residuals);
            var residualVariance = RSS / dof;

            var covariance = ParameterStatistics.CovarianceMatrixForLinearRegression(X, residualVariance);
            var standardErrors = ParameterStatistics.StandardErrors(covariance);
            var tStatistics = ParameterStatistics.TStatistics(parameters, standardErrors);
            var pValues = ParameterStatistics.PValues(tStatistics, dof);
            var confIntervals = ParameterStatistics.ConfidenceIntervalHalfWidths(standardErrors, dof, 0.95);

            // Calculate total sum of squares for R-squared
            var yMean = Y.Average();
            var TSS = Y.Select(y_i => Math.Pow(y_i - yMean, 2)).Sum();
            var rSquared = 1.0 - RSS / TSS;
            var adjustedRSquared = 1 - (1 - rSquared) * (Ns - 1) / dof;
            var rootMSE = Math.Sqrt(residualVariance);

            // Check parameters
            Assert.That(parameters[0], Is.EqualTo(-0.24).Within(0.001));
            Assert.That(parameters[1], Is.EqualTo(3.46286).Within(0.001));
            Assert.That(parameters[2], Is.EqualTo(2.64286).Within(0.001));

            // Check standard errors
            Assert.That(standardErrors[0], Is.EqualTo(3.07019).Within(0.001));
            Assert.That(standardErrors[1], Is.EqualTo(2.33969).Within(0.001));
            Assert.That(standardErrors[2], Is.EqualTo(0.38258).Within(0.001));

            // Check t-statistics
            Assert.That(tStatistics[0], Is.EqualTo(-0.07817).Within(0.001));
            Assert.That(tStatistics[1], Is.EqualTo(1.48005).Within(0.001));
            Assert.That(tStatistics[2], Is.EqualTo(6.90799).Within(0.001));

            // Check p-values
            Assert.That(pValues[0], Is.EqualTo(0.94481).Within(0.001));
            Assert.That(pValues[1], Is.EqualTo(0.27700).Within(0.001));
            Assert.That(pValues[2], Is.EqualTo(0.02032).Within(0.001));

            // Check confidence intervals
            Assert.That(confIntervals[0], Is.EqualTo(13.20995).Within(0.001));
            Assert.That(confIntervals[1], Is.EqualTo(10.06686).Within(0.001));
            Assert.That(confIntervals[2], Is.EqualTo(1.64611).Within(0.001));

            // Check fit statistics
            Assert.That(dof, Is.EqualTo(2));
            Assert.That(residualVariance, Is.EqualTo(2.04914).Within(0.001));
            Assert.That(RSS, Is.EqualTo(4.09829).Within(0.001));
            Assert.That(Math.Sqrt(rSquared), Is.EqualTo(0.99947).Within(0.001)); // R value
            Assert.That(rSquared, Is.EqualTo(0.99893).Within(0.001));
            Assert.That(adjustedRSquared, Is.EqualTo(0.99786).Within(0.001));
            Assert.That(rootMSE, Is.EqualTo(1.43148).Within(0.001));
        }

        #endregion

        #region Matrix Utility Tests

        [Test]
        public void CorrelationFromCovarianceTest()
        {
            var covariance = Matrix<double>.Build.DenseOfArray(new double[,] {
                {4.0, 1.2, -0.8},
                {1.2, 9.0, 0.6},
                {-0.8, 0.6, 16.0}
            });

            var correlation = ParameterStatistics.CorrelationFromCovariance(covariance);

            Assert.That(correlation.RowCount, Is.EqualTo(3));
            Assert.That(correlation.ColumnCount, Is.EqualTo(3));

            // Diagonal elements should be 1
            for (var i = 0; i < correlation.RowCount; i++)
            {
                Assert.That(correlation[i, i], Is.EqualTo(1.0).Within(1e-10));
            }

            // Off-diagonal elements should be between -1 and 1
            for (var i = 0; i < correlation.RowCount; i++)
            {
                for (var j = 0; j < correlation.ColumnCount; j++)
                {
                    if (i != j)
                    {
                        Assert.That(correlation[i, j], Is.GreaterThanOrEqualTo(-1.0).And.LessThanOrEqualTo(1.0));
                    }
                }
            }

            // Check specific values (manually calculated)
            Assert.That(correlation[0, 1], Is.EqualTo(0.2).Within(1e-10));
            Assert.That(correlation[0, 2], Is.EqualTo(-0.1).Within(1e-10));
            Assert.That(correlation[1, 2], Is.EqualTo(0.05).Within(1e-10));
        }
                
        #endregion

        #region Special Cases Tests

        [Test]
        public void DependenciesTest()
        {
            // Create a correlation matrix with high multicollinearity
            var correlation = Matrix<double>.Build.DenseOfArray(new double[,] {
                {1.0, 0.95, 0.3},
                {0.95, 1.0, 0.2},
                {0.3, 0.2, 1.0}
            });

            var dependencies = ParameterStatistics.DependenciesFromCorrelation(correlation);

            Assert.That(dependencies.Count, Is.EqualTo(3));

            // First two parameters should have high dependency values
            Assert.That(dependencies[0], Is.GreaterThan(0.8));
            Assert.That(dependencies[1], Is.GreaterThan(0.8));

            // Third parameter should have lower dependency
            Assert.That(dependencies[2], Is.LessThan(0.3));
        }

        [Test]
        public void ConfidenceIntervalsTest()
        {
            var standardErrors = Vector<double>.Build.Dense(new double[] { 0.1, 0.2, 0.5 });
            var df = 10; // Degrees of freedom
            var confidenceLevel = 0.95; // 95% confidence

            var halfWidths = ParameterStatistics.ConfidenceIntervalHalfWidths(standardErrors, df, confidenceLevel);

            Assert.That(halfWidths.Count, Is.EqualTo(3));

            // t-critical for df=10, 95% confidence (two-tailed) is approximately 2.228
            var expectedFactor = 2.228;
            Assert.That(halfWidths[0], Is.EqualTo(standardErrors[0] * expectedFactor).Within(0.1));
            Assert.That(halfWidths[1], Is.EqualTo(standardErrors[1] * expectedFactor).Within(0.1));
            Assert.That(halfWidths[2], Is.EqualTo(standardErrors[2] * expectedFactor).Within(0.1));
        }

        #endregion
    }
}
