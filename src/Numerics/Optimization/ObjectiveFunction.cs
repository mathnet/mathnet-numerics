// <copyright file="ObjectiveFunction.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2017 Math.NET
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
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Optimization.ObjectiveFunctions;

namespace MathNet.Numerics.Optimization
{
    public static class ObjectiveFunction
    {
        /// <summary>
        /// Objective function where neither Gradient nor Hessian is available.
        /// </summary>
        public static IObjectiveFunction Value(Func<Vector<double>, double> function)
        {
            return new ValueObjectiveFunction(function);
        }

        /// <summary>
        /// Objective function where the Gradient is available. Greedy evaluation.
        /// </summary>
        public static IObjectiveFunction Gradient(Func<Vector<double>, (double, Vector<double>)> function)
        {
            return new GradientObjectiveFunction(function);
        }

        /// <summary>
        /// Objective function where the Gradient is available. Lazy evaluation.
        /// </summary>
        public static IObjectiveFunction Gradient(Func<Vector<double>, double> function, Func<Vector<double>, Vector<double>> gradient)
        {
            return new LazyObjectiveFunction(function, gradient: gradient);
        }

        /// <summary>
        /// Objective function where the Hessian is available. Greedy evaluation.
        /// </summary>
        public static IObjectiveFunction Hessian(Func<Vector<double>, (double, Matrix<double>)> function)
        {
            return new HessianObjectiveFunction(function);
        }

        /// <summary>
        /// Objective function where the Hessian is available. Lazy evaluation.
        /// </summary>
        public static IObjectiveFunction Hessian(Func<Vector<double>, double> function, Func<Vector<double>, Matrix<double>> hessian)
        {
            return new LazyObjectiveFunction(function, hessian: hessian);
        }

        /// <summary>
        /// Objective function where both Gradient and Hessian are available. Greedy evaluation.
        /// </summary>
        public static IObjectiveFunction GradientHessian(Func<Vector<double>, (double, Vector<double>, Matrix<double>)> function)
        {
            return new GradientHessianObjectiveFunction(function);
        }

        /// <summary>
        /// Objective function where both Gradient and Hessian are available. Lazy evaluation.
        /// </summary>
        public static IObjectiveFunction GradientHessian(Func<Vector<double>, double> function, Func<Vector<double>, Vector<double>> gradient, Func<Vector<double>, Matrix<double>> hessian)
        {
            return new LazyObjectiveFunction(function, gradient: gradient, hessian: hessian);
        }

        /// <summary>
        /// Objective function where neither first nor second derivative is available.
        /// </summary>
        public static IScalarObjectiveFunction ScalarValue(Func<double, double> function)
        {
            return new ScalarValueObjectiveFunction(function);
        }

        /// <summary>
        /// Objective function where the first derivative is available.
        /// </summary>
        public static IScalarObjectiveFunction ScalarDerivative(Func<double, double> function, Func<double, double> derivative)
        {
            return new ScalarObjectiveFunction(function, derivative);
        }

        /// <summary>
        /// Objective function where the first and second derivatives are available.
        /// </summary>
        public static IScalarObjectiveFunction ScalarSecondDerivative(Func<double, double> function, Func<double, double> derivative, Func<double,double> secondDerivative)
        {
            return new ScalarObjectiveFunction(function, derivative, secondDerivative);
        }

        /// <summary>
        /// Creates an objective model with a user-supplied model function and Jacobian for non-linear least squares regression.
        /// Uses the form F(p) = 1/2 * sum(w_i * (y_i - f(x_i; p))^2) where f(x; p) is the model function.
        /// </summary>
        /// <param name="function">The model function f(x; p) that maps from x to y given parameters p</param>
        /// <param name="derivatives">The Jacobian of the model function with respect to parameters</param>
        /// <param name="observedX">The observed x values</param>
        /// <param name="observedY">The observed y values</param>
        /// <param name="weight">Optional weights for the observations</param>
        /// <returns>An objective model configured for the specified model and observations</returns>
        public static IObjectiveModel NonlinearModel(
            Func<Vector<double>, Vector<double>, Vector<double>> function,
            Func<Vector<double>, Vector<double>, Matrix<double>> derivatives,
            Vector<double> observedX, Vector<double> observedY, Vector<double> weight = null)
        {
            var objective = new NonlinearObjectiveModel(function, derivatives);
            objective.SetObserved(observedX, observedY, weight);
            return objective;
        }

        /// <summary>
        /// Creates an objective model for non-linear least squares regression with numerical differentiation.
        /// Uses the form F(p) = 1/2 * sum(w_i * (y_i - f(x_i; p))^2) where f(x; p) is the model function.
        /// </summary>
        /// <param name="function">The model function f(x; p) that maps from x to y given parameters p</param>
        /// <param name="observedX">The observed x values</param>
        /// <param name="observedY">The observed y values</param>
        /// <param name="weight">Optional weights for the observations</param>
        /// <param name="accuracyOrder">Accuracy order for numerical differentiation (1-6)</param>
        /// <returns>An objective model configured for the specified model and observations</returns>
        public static IObjectiveModel NonlinearModel(
            Func<Vector<double>, Vector<double>, Vector<double>> function,
            Vector<double> observedX, Vector<double> observedY, Vector<double> weight = null,
            int accuracyOrder = 2)
        {
            var objective = new NonlinearObjectiveModel(function, accuracyOrder: accuracyOrder);
            objective.SetObserved(observedX, observedY, weight);
            return objective;
        }

        /// <summary>
        /// Creates an objective model with a user-supplied model function and Jacobian for non-linear least squares regression.
        /// This overload accepts scalar x values with function f(p, x) and converts them to vector operations internally.
        /// Uses the form F(p) = 1/2 * sum(w_i * (y_i - f(p, x_i))^2).
        /// </summary>
        /// <param name="function">The model function f(p, x) that maps from scalar x to y given parameters p</param>
        /// <param name="derivatives">The derivatives of the model function with respect to parameters</param>
        /// <param name="observedX">The observed x values</param>
        /// <param name="observedY">The observed y values</param>
        /// <param name="weight">Optional weights for the observations</param>
        /// <returns>An objective model configured for the specified model and observations</returns>
        public static IObjectiveModel NonlinearModel(
            Func<Vector<double>, double, double> function,
            Func<Vector<double>, double, Vector<double>> derivatives,
            Vector<double> observedX, Vector<double> observedY, Vector<double> weight = null)
        {
            Vector<double> Func(Vector<double> point, Vector<double> x)
            {
                var functionValues = CreateVector.Dense<double>(x.Count);
                for (int i = 0; i < x.Count; i++)
                {
                    functionValues[i] = function(point, x[i]);
                }

                return functionValues;
            }

            Matrix<double> Prime(Vector<double> point, Vector<double> x)
            {
                var derivativeValues = CreateMatrix.Dense<double>(x.Count, point.Count);
                for (int i = 0; i < x.Count; i++)
                {
                    derivativeValues.SetRow(i, derivatives(point, x[i]));
                }

                return derivativeValues;
            }

            var objective = new NonlinearObjectiveModel(Func, Prime);
            objective.SetObserved(observedX, observedY, weight);
            return objective;
        }

        /// <summary>
        /// Creates an objective model for non-linear least squares regression with numerical differentiation.
        /// This overload accepts scalar x values with function f(p, x) and converts them to vector operations internally.
        /// Uses the form F(p) = 1/2 * sum(w_i * (y_i - f(p, x_i))^2).
        /// </summary>
        /// <param name="function">The model function f(p, x) that maps from scalar x to y given parameters p</param>
        /// <param name="observedX">The observed x values</param>
        /// <param name="observedY">The observed y values</param>
        /// <param name="weight">Optional weights for the observations</param>
        /// <param name="accuracyOrder">Accuracy order for numerical differentiation (1-6)</param>
        /// <returns>An objective model configured for the specified model and observations</returns>
        public static IObjectiveModel NonlinearModel(
            Func<Vector<double>, double, double> function,
            Vector<double> observedX, Vector<double> observedY, Vector<double> weight = null,
            int accuracyOrder = 2)
        {
            Vector<double> Func(Vector<double> point, Vector<double> x)
            {
                var functionValues = CreateVector.Dense<double>(x.Count);
                for (int i = 0; i < x.Count; i++)
                {
                    functionValues[i] = function(point, x[i]);
                }

                return functionValues;
            }

            var objective = new NonlinearObjectiveModel(Func, accuracyOrder: accuracyOrder);
            objective.SetObserved(observedX, observedY, weight);
            return objective;
        }

        /// <summary>
        /// Creates an objective model from a direct residual function for non-linear optimization.
        /// Uses the form F(p) = 1/2 * sum(r_i(p)^2) where r(p) is the residual function.
        /// </summary>
        /// <param name="residualFunction">Function that calculates residuals directly from parameters</param>
        /// <param name="jacobian">Optional Jacobian of the residual function</param>
        /// <param name="observationCount">Number of observations for degree of freedom calculations (optional)</param>
        /// <param name="accuracyOrder">Accuracy order for numerical differentiation (1-6)</param>
        /// <returns>An objective model configured for the specified residual function</returns>
        public static IObjectiveModel NonlinearModel(
            Func<Vector<double>, Vector<double>> residualFunction,
            Func<Vector<double>, Matrix<double>> jacobian = null,
            int? observationCount = null,
            int accuracyOrder = 2)
        {
            return new NonlinearObjectiveModel(residualFunction, jacobian, observationCount, accuracyOrder);
        }

        /// <summary>
        /// Creates an objective function with a user-supplied model function and Jacobian for non-linear least squares regression.
        /// Uses the form F(p) = 1/2 * sum(w_i * (y_i - f(x_i; p))^2) where f(x; p) is the model function.
        /// </summary>
        /// <param name="function">The model function f(x; p) that maps from x to y given parameters p</param>
        /// <param name="derivatives">The Jacobian of the model function with respect to parameters</param>
        /// <param name="observedX">The observed x values</param>
        /// <param name="observedY">The observed y values</param>
        /// <param name="weight">Optional weights for the observations</param>
        /// <returns>An objective function configured for the specified model and observations</returns>
        public static IObjectiveFunction NonlinearFunction(
            Func<Vector<double>, Vector<double>, Vector<double>> function,
            Func<Vector<double>, Vector<double>, Matrix<double>> derivatives,
            Vector<double> observedX, Vector<double> observedY, Vector<double> weight = null)
        {
            var objective = new NonlinearObjectiveModel(function, derivatives);
            objective.SetObserved(observedX, observedY, weight);
            return objective.ToObjectiveFunction();
        }

        /// <summary>
        /// Creates an objective function for non-linear least squares regression with numerical differentiation.
        /// Uses the form F(p) = 1/2 * sum(w_i * (y_i - f(x_i; p))^2) where f(x; p) is the model function.
        /// </summary>
        /// <param name="function">The model function f(x; p) that maps from x to y given parameters p</param>
        /// <param name="observedX">The observed x values</param>
        /// <param name="observedY">The observed y values</param>
        /// <param name="weight">Optional weights for the observations</param>
        /// <param name="accuracyOrder">Accuracy order for numerical differentiation (1-6)</param>
        /// <returns>An objective function configured for the specified model and observations</returns>
        public static IObjectiveFunction NonlinearFunction(
            Func<Vector<double>, Vector<double>, Vector<double>> function,
            Vector<double> observedX, Vector<double> observedY, Vector<double> weight = null,
            int accuracyOrder = 2)
        {
            var objective = new NonlinearObjectiveModel(function, null, accuracyOrder: accuracyOrder);
            objective.SetObserved(observedX, observedY, weight);
            return objective.ToObjectiveFunction();
        }

        /// <summary>
        /// Creates an objective function from a direct residual function for non-linear optimization.
        /// Uses the form F(p) = 1/2 * sum(r_i(p)^2) where r(p) is the residual function.
        /// </summary>
        /// <param name="residualFunction">Function that calculates residuals directly from parameters</param>
        /// <param name="jacobian">Optional Jacobian of the residual function</param>
        /// <param name="observationCount">Number of observations for degree of freedom calculations (optional)</param>
        /// <param name="accuracyOrder">Accuracy order for numerical differentiation (1-6)</param>
        /// <returns>An objective function configured for the specified residual function</returns>
        public static IObjectiveFunction NonlinearFunction(
            Func<Vector<double>, Vector<double>> residualFunction,
            Func<Vector<double>, Matrix<double>> jacobian = null,
            int? observationCount = null,
            int accuracyOrder = 2)
        {
            var objective = new NonlinearObjectiveModel(residualFunction, jacobian, observationCount, accuracyOrder);
            return objective.ToObjectiveFunction();
        }
    }
}
