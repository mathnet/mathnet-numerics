// <copyright file="NumericalJacobian.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2015 Math.NET
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

using System;

namespace MathNet.Numerics.Differentiation
{
    /// <summary>
    /// Class for evaluating the Jacobian of a function using finite differences.
    /// By default, a central 3-point method is used.
    /// </summary>
    public class NumericalJacobian
    {
        /// <summary>
        /// Number of function evaluations.
        /// </summary>
        public int FunctionEvaluations => _df.Evaluations;

        readonly NumericalDerivative _df;

        /// <summary>
        /// Creates a numerical Jacobian object with a three point central difference method.
        /// </summary>
        public NumericalJacobian() : this(3, 1) { }

        /// <summary>
        /// Creates a numerical Jacobian with a specified differentiation scheme.
        /// </summary>
        /// <param name="points">Number of points for Jacobian evaluation.</param>
        /// <param name="center">Center point for differentiation.</param>
        public NumericalJacobian(int points, int center)
        {
            _df = new NumericalDerivative(points, center);
        }

        /// <summary>
        /// Evaluates the Jacobian of scalar univariate function f at point x.
        /// </summary>
        /// <param name="f">Scalar univariate function handle.</param>
        /// <param name="x">Point at which to evaluate Jacobian.</param>
        /// <returns>Jacobian vector.</returns>
        public double[] Evaluate(Func<double, double> f, double x)
        {
            return new[] { _df.EvaluateDerivative(f, x, 1) };
        }

        /// <summary>
        /// Evaluates the Jacobian of a multivariate function f at vector x.
        /// </summary>
        /// <remarks>
        /// This function assumes that the length of vector x consistent with the argument count of f.
        /// </remarks>
        /// <param name="f">Multivariate function handle.</param>
        /// <param name="x">Points at which to evaluate Jacobian.</param>
        /// <returns>Jacobian vector.</returns>
        public double[] Evaluate(Func<double[], double> f, double[] x)
        {
            var jacobian = new double[x.Length];

            for (var i = 0; i < jacobian.Length; i++)
                jacobian[i] = _df.EvaluatePartialDerivative(f, x, i, 1);

            return jacobian;
        }

        /// <summary>
        /// Evaluates the Jacobian of a multivariate function f at vector x given a current function value.
        /// </summary>
        /// <remarks>
        /// To minimize the number of function evaluations, a user can supply the current value of the function
        /// to be used in computing the Jacobian. This value must correspond to the "center" location for the
        /// finite differencing. If a scheme is used where the center value is not evaluated, this will provide no
        /// added efficiency. This method also assumes that the length of vector x consistent with the argument count of f.
        /// </remarks>
        /// <param name="f">Multivariate function handle.</param>
        /// <param name="x">Points at which to evaluate Jacobian.</param>
        /// <param name="currentValue">Current function value at finite difference center.</param>
        /// <returns>Jacobian vector.</returns>
        public double[] Evaluate(Func<double[], double> f, double[] x, double currentValue)
        {
            var jacobian = new double[x.Length];

            for (var i = 0; i < jacobian.Length; i++)
                jacobian[i] = _df.EvaluatePartialDerivative(f, x, i, 1, currentValue);

            return jacobian;
        }

        /// <summary>
        /// Evaluates the Jacobian of a multivariate function array f at vector x.
        /// </summary>
        /// <param name="f">Multivariate function array handle.</param>
        /// <param name="x">Vector at which to evaluate Jacobian.</param>
        /// <returns>Jacobian matrix.</returns>
        public double[,] Evaluate(Func<double[], double>[] f, double[] x)
        {
            var jacobian = new double[f.Length, x.Length];
            for (int i = 0; i < f.Length; i++)
            {
                var gradient = Evaluate(f[i], x);
                for (int j = 0; j < gradient.Length; j++)
                    jacobian[i, j] = gradient[j];
            }

            return jacobian;
        }

        /// <summary>
        /// Evaluates the Jacobian of a multivariate function array f at vector x given a vector of current function values.
        /// </summary>
        /// <remarks>
        /// To minimize the number of function evaluations, a user can supply a vector of current values of the functions
        /// to be used in computing the Jacobian. These value must correspond to the "center" location for the
        /// finite differencing. If a scheme is used where the center value is not evaluated, this will provide no
        /// added efficiency. This method also assumes that the length of vector x consistent with the argument count of f.
        /// </remarks>
        /// <param name="f">Multivariate function array handle.</param>
        /// <param name="x">Vector at which to evaluate Jacobian.</param>
        /// <param name="currentValues">Vector of current function values.</param>
        /// <returns>Jacobian matrix.</returns>
        public double[,] Evaluate(Func<double[], double>[] f, double[] x, double[] currentValues)
        {
            var jacobian = new double[f.Length, x.Length];
            for (int i = 0; i < f.Length; i++)
            {
                var gradient = Evaluate(f[i], x, currentValues[i]);
                for (int j = 0; j < gradient.Length; j++)
                    jacobian[i, j] = gradient[j];
            }

            return jacobian;
        }

        /// <summary>
        /// Resets the function evaluation counter for the Jacobian.
        /// </summary>
        public void ResetFunctionEvaluations()
        {
            _df.ResetEvaluations();
        }
    }
}
