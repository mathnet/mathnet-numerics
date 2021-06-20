// <copyright file="FindMinimum.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2021 Math.NET
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
using MathNet.Numerics.Optimization;

namespace MathNet.Numerics
{
    public static class FindMinimum
    {
        /// <summary>
        /// Find value x that minimizes the scalar function f(x), constrained within bounds, using the Golden Section algorithm.
        /// For more options and diagnostics consider to use <see cref="GoldenSectionMinimizer"/> directly.
        /// </summary>
        public static double OfScalarFunctionConstrained(Func<double, double> function, double lowerBound, double upperBound, double tolerance=1e-5, int maxIterations=1000)
        {
            var objective = ObjectiveFunction.ScalarValue(function);
            var result = GoldenSectionMinimizer.Minimum(objective, lowerBound, upperBound, tolerance, maxIterations);
            return result.MinimizingPoint;
        }

        /// <summary>
        /// Find vector x that minimizes the function f(x) using the Nelder-Mead Simplex algorithm.
        /// For more options and diagnostics consider to use <see cref="NelderMeadSimplex"/> directly.
        /// </summary>
        public static double OfScalarFunction(Func<double, double> function, double initialGuess, double tolerance = 1e-8, int maxIterations = 1000)
        {
            var objective = ObjectiveFunction.Value(v => function(v[0]));
            var result = NelderMeadSimplex.Minimum(objective, CreateVector.Dense(new[] { initialGuess }), tolerance, maxIterations);
            return result.MinimizingPoint[0];
        }

        /// <summary>
        /// Find vector x that minimizes the function f(x) using the Nelder-Mead Simplex algorithm.
        /// For more options and diagnostics consider to use <see cref="NelderMeadSimplex"/> directly.
        /// </summary>
        public static (double P0, double P1) OfFunction(Func<double, double, double> function, double initialGuess0, double initialGuess1, double tolerance = 1e-8, int maxIterations = 1000)
        {
            var objective = ObjectiveFunction.Value(v => function(v[0], v[1]));
            var result = NelderMeadSimplex.Minimum(objective, CreateVector.Dense(new[] { initialGuess0, initialGuess1 }), tolerance, maxIterations);
            return (result.MinimizingPoint[0], result.MinimizingPoint[1]);
        }

        /// <summary>
        /// Find vector x that minimizes the function f(x) using the Nelder-Mead Simplex algorithm.
        /// For more options and diagnostics consider to use <see cref="NelderMeadSimplex"/> directly.
        /// </summary>
        public static (double P0, double P1, double P2) OfFunction(Func<double, double, double, double> function, double initialGuess0, double initialGuess1, double initialGuess2, double tolerance = 1e-8, int maxIterations = 1000)
        {
            var objective = ObjectiveFunction.Value(v => function(v[0], v[1], v[2]));
            var result = NelderMeadSimplex.Minimum(objective, CreateVector.Dense(new[] { initialGuess0, initialGuess1, initialGuess2 }), tolerance, maxIterations);
            return (result.MinimizingPoint[0], result.MinimizingPoint[1], result.MinimizingPoint[2]);
        }

        /// <summary>
        /// Find vector x that minimizes the function f(x) using the Nelder-Mead Simplex algorithm.
        /// For more options and diagnostics consider to use <see cref="NelderMeadSimplex"/> directly.
        /// </summary>
        public static (double P0, double P1, double P2, double P3) OfFunction(Func<double, double, double, double, double> function, double initialGuess0, double initialGuess1, double initialGuess2, double initialGuess3, double tolerance = 1e-8, int maxIterations = 1000)
        {
            var objective = ObjectiveFunction.Value(v => function(v[0], v[1], v[2], v[3]));
            var result = NelderMeadSimplex.Minimum(objective, CreateVector.Dense(new[] { initialGuess0, initialGuess1, initialGuess2, initialGuess3 }), tolerance, maxIterations);
            return (result.MinimizingPoint[0], result.MinimizingPoint[1], result.MinimizingPoint[2], result.MinimizingPoint[3]);
        }

        /// <summary>
        /// Find vector x that minimizes the function f(x) using the Nelder-Mead Simplex algorithm.
        /// For more options and diagnostics consider to use <see cref="NelderMeadSimplex"/> directly.
        /// </summary>
        public static (double P0, double P1, double P2, double P3, double P4) OfFunction(Func<double, double, double, double, double, double> function, double initialGuess0, double initialGuess1, double initialGuess2, double initialGuess3, double initialGuess4, double tolerance = 1e-8, int maxIterations = 1000)
        {
            var objective = ObjectiveFunction.Value(v => function(v[0], v[1], v[2], v[3], v[4]));
            var result = NelderMeadSimplex.Minimum(objective, CreateVector.Dense(new[] { initialGuess0, initialGuess1, initialGuess2, initialGuess3, initialGuess4 }), tolerance, maxIterations);
            return (result.MinimizingPoint[0], result.MinimizingPoint[1], result.MinimizingPoint[2], result.MinimizingPoint[3], result.MinimizingPoint[4]);
        }

        /// <summary>
        /// Find vector x that minimizes the function f(x) using the Nelder-Mead Simplex algorithm.
        /// For more options and diagnostics consider to use <see cref="NelderMeadSimplex"/> directly.
        /// </summary>
        public static Vector<double> OfFunction(Func<Vector<double>, double> function, Vector<double> initialGuess, double tolerance=1e-8, int maxIterations=1000)
        {
            var objective = ObjectiveFunction.Value(function);
            var result = NelderMeadSimplex.Minimum(objective, initialGuess, tolerance, maxIterations);
            return result.MinimizingPoint;
        }

        /// <summary>
        /// Find vector x that minimizes the function f(x), constrained within bounds, using the Broyden–Fletcher–Goldfarb–Shanno Bounded (BFGS-B) algorithm.
        /// The missing gradient is evaluated numerically (forward difference).
        /// For more options and diagnostics consider to use <see cref="BfgsBMinimizer"/> directly.
        /// </summary>
        public static Vector<double> OfFunctionConstrained(Func<Vector<double>, double> function, Vector<double> lowerBound, Vector<double> upperBound, Vector<double> initialGuess, double gradientTolerance=1e-5, double parameterTolerance=1e-5, double functionProgressTolerance=1e-5, int maxIterations=1000)
        {
            var objective = ObjectiveFunction.Value(function);
            var objectiveWithGradient = new Optimization.ObjectiveFunctions.ForwardDifferenceGradientObjectiveFunction(objective, lowerBound, upperBound);
            var algorithm = new BfgsBMinimizer(gradientTolerance, parameterTolerance, functionProgressTolerance, maxIterations);
            var result = algorithm.FindMinimum(objectiveWithGradient, lowerBound, upperBound, initialGuess);
            return result.MinimizingPoint;
        }

        /// <summary>
        /// Find vector x that minimizes the function f(x) using the Broyden–Fletcher–Goldfarb–Shanno (BFGS) algorithm.
        /// For more options and diagnostics consider to use <see cref="BfgsMinimizer"/> directly.
        /// An alternative routine using conjugate gradients (CG) is available in <see cref="ConjugateGradientMinimizer"/>.
        /// </summary>
        public static Vector<double> OfFunctionGradient(Func<Vector<double>, double> function, Func<Vector<double>, Vector<double>> gradient, Vector<double> initialGuess, double gradientTolerance=1e-5, double parameterTolerance=1e-5, double functionProgressTolerance=1e-5, int maxIterations=1000)
        {
            var objective = ObjectiveFunction.Gradient(function, gradient);
            var algorithm = new BfgsMinimizer(gradientTolerance, parameterTolerance, functionProgressTolerance, maxIterations);
            var result = algorithm.FindMinimum(objective, initialGuess);
            return result.MinimizingPoint;
        }

        /// <summary>
        /// Find vector x that minimizes the function f(x) using the Broyden–Fletcher–Goldfarb–Shanno (BFGS) algorithm.
        /// For more options and diagnostics consider to use <see cref="BfgsMinimizer"/> directly.
        /// An alternative routine using conjugate gradients (CG) is available in <see cref="ConjugateGradientMinimizer"/>.
        /// </summary>
        public static Vector<double> OfFunctionGradient(Func<Vector<double>, (double, Vector<double>)> functionGradient, Vector<double> initialGuess, double gradientTolerance=1e-5, double parameterTolerance=1e-5, double functionProgressTolerance=1e-5, int maxIterations=1000)
        {
            var objective = ObjectiveFunction.Gradient(functionGradient);
            var algorithm = new BfgsMinimizer(gradientTolerance, parameterTolerance, functionProgressTolerance, maxIterations);
            var result = algorithm.FindMinimum(objective, initialGuess);
            return result.MinimizingPoint;
        }

        /// <summary>
        /// Find vector x that minimizes the function f(x), constrained within bounds, using the Broyden–Fletcher–Goldfarb–Shanno Bounded (BFGS-B) algorithm.
        /// For more options and diagnostics consider to use <see cref="BfgsBMinimizer"/> directly.
        /// </summary>
        public static Vector<double> OfFunctionGradientConstrained(Func<Vector<double>, double> function, Func<Vector<double>, Vector<double>> gradient, Vector<double> lowerBound, Vector<double> upperBound, Vector<double> initialGuess, double gradientTolerance=1e-5, double parameterTolerance=1e-5, double functionProgressTolerance=1e-5, int maxIterations=1000)
        {
            var objective = ObjectiveFunction.Gradient(function, gradient);
            var algorithm = new BfgsBMinimizer(gradientTolerance, parameterTolerance, functionProgressTolerance, maxIterations);
            var result = algorithm.FindMinimum(objective, lowerBound, upperBound, initialGuess);
            return result.MinimizingPoint;
        }

        /// <summary>
        /// Find vector x that minimizes the function f(x), constrained within bounds, using the Broyden–Fletcher–Goldfarb–Shanno Bounded (BFGS-B) algorithm.
        /// For more options and diagnostics consider to use <see cref="BfgsBMinimizer"/> directly.
        /// </summary>
        public static Vector<double> OfFunctionGradientConstrained(Func<Vector<double>, (double, Vector<double>)> functionGradient, Vector<double> lowerBound, Vector<double> upperBound, Vector<double> initialGuess, double gradientTolerance=1e-5, double parameterTolerance=1e-5, double functionProgressTolerance=1e-5, int maxIterations=1000)
        {
            var objective = ObjectiveFunction.Gradient(functionGradient);
            var algorithm = new BfgsBMinimizer(gradientTolerance, parameterTolerance, functionProgressTolerance, maxIterations);
            var result = algorithm.FindMinimum(objective, lowerBound, upperBound, initialGuess);
            return result.MinimizingPoint;
        }

        /// <summary>
        /// Find vector x that minimizes the function f(x) using the Newton algorithm.
        /// For more options and diagnostics consider to use <see cref="NewtonMinimizer"/> directly.
        /// </summary>
        public static Vector<double> OfFunctionGradientHessian(Func<Vector<double>, double> function, Func<Vector<double>, Vector<double>> gradient, Func<Vector<double>, Matrix<double>> hessian, Vector<double> initialGuess, double gradientTolerance=1e-8, int maxIterations=1000)
        {
            var objective = ObjectiveFunction.GradientHessian(function, gradient, hessian);
            var result = NewtonMinimizer.Minimum(objective, initialGuess, gradientTolerance, maxIterations);
            return result.MinimizingPoint;
        }

        /// <summary>
        /// Find vector x that minimizes the function f(x) using the Newton algorithm.
        /// For more options and diagnostics consider to use <see cref="NewtonMinimizer"/> directly.
        /// </summary>
        public static Vector<double> OfFunctionGradientHessian(Func<Vector<double>, (double, Vector<double>, Matrix<double>)> functionGradientHessian, Vector<double> initialGuess, double gradientTolerance=1e-8, int maxIterations=1000)
        {
            var objective = ObjectiveFunction.GradientHessian(functionGradientHessian);
            var result = NewtonMinimizer.Minimum(objective, initialGuess, gradientTolerance, maxIterations);
            return result.MinimizingPoint;
        }
    }
}
