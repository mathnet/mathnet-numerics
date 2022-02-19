// <copyright file="Fit.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2018 Math.NET
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
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearRegression;
using MathNet.Numerics.Providers.LinearAlgebra;

namespace MathNet.Numerics
{
    /// <summary>
    /// Least-Squares Curve Fitting Routines
    /// </summary>
    public static class Fit
    {
        /// <summary>
        /// Least-Squares fitting the points (x,y) to a line y : x -> a+b*x,
        /// returning its best fitting parameters as (a, b) tuple,
        /// where a is the intercept and b the slope.
        /// </summary>
        public static (double A, double B) Line(double[] x, double[] y)
        {
            return SimpleRegression.Fit(x, y);
        }

        /// <summary>
        /// Least-Squares fitting the points (x,y) to a line y : x -> a+b*x,
        /// returning a function y' for the best fitting line.
        /// </summary>
        public static Func<double, double> LineFunc(double[] x, double[] y)
        {
            (double intercept, double slope) = SimpleRegression.Fit(x, y);
            return z => intercept + slope * z;
        }

        /// <summary>
        /// Least-Squares fitting the points (x,y) to a line through origin y : x -> b*x,
        /// returning its best fitting parameter b,
        /// where the intercept is zero and b the slope.
        /// </summary>
        public static double LineThroughOrigin(double[] x, double[] y)
        {
            return SimpleRegression.FitThroughOrigin(x, y);
        }

        /// <summary>
        /// Least-Squares fitting the points (x,y) to a line through origin y : x -> b*x,
        /// returning a function y' for the best fitting line.
        /// </summary>
        public static Func<double, double> LineThroughOriginFunc(double[] x, double[] y)
        {
            double slope = SimpleRegression.FitThroughOrigin(x, y);
            return z => slope * z;
        }

        /// <summary>
        /// Least-Squares fitting the points (x,y) to an exponential y : x -> a*exp(r*x),
        /// returning its best fitting parameters as (a, r) tuple.
        /// </summary>
        public static (double A, double R) Exponential(double[] x, double[] y, DirectRegressionMethod method = DirectRegressionMethod.QR)
        {
            // Transformation: y_h := ln(y) ~> y_h : x -> ln(a) + r*x;
            double[] lny = Generate.Map(y, Math.Log);
            double[] p = LinearCombination(x, lny, method, _ => 1.0, t => t);
            return (Math.Exp(p[0]), p[1]);
        }

        /// <summary>
        /// Least-Squares fitting the points (x,y) to an exponential y : x -> a*exp(r*x),
        /// returning a function y' for the best fitting line.
        /// </summary>
        public static Func<double, double> ExponentialFunc(double[] x, double[] y, DirectRegressionMethod method = DirectRegressionMethod.QR)
        {
            (double a, double r) = Exponential(x, y, method);
            return z => a * Math.Exp(r * z);
        }

        /// <summary>
        /// Least-Squares fitting the points (x,y) to a logarithm y : x -> a + b*ln(x),
        /// returning its best fitting parameters as (a, b) tuple.
        /// </summary>
        public static (double A, double B) Logarithm(double[] x, double[] y, DirectRegressionMethod method = DirectRegressionMethod.QR)
        {
            double[] lnx = Generate.Map(x, Math.Log);
            double[] p = LinearCombination(lnx, y, method, _ => 1.0, t => t);
            return (p[0], p[1]);
        }

        /// <summary>
        /// Least-Squares fitting the points (x,y) to a logarithm y : x -> a + b*ln(x),
        /// returning a function y' for the best fitting line.
        /// </summary>
        public static Func<double, double> LogarithmFunc(double[] x, double[] y, DirectRegressionMethod method = DirectRegressionMethod.QR)
        {
            (double a, double b) = Logarithm(x, y, method);
            return z => a + b * Math.Log(z);
        }

        /// <summary>
        /// Least-Squares fitting the points (x,y) to a power y : x -> a*x^b,
        /// returning its best fitting parameters as (a, b) tuple.
        /// </summary>
        public static (double A, double B) Power(double[] x, double[] y, DirectRegressionMethod method = DirectRegressionMethod.QR)
        {
            // Transformation: y_h := ln(y) ~> y_h : x -> ln(a) + b*ln(x);
            double[] lny = Generate.Map(y, Math.Log);
            double[] p = LinearCombination(x, lny, method, _ => 1.0, Math.Log);
            return (Math.Exp(p[0]), p[1]);
        }

        /// <summary>
        /// Least-Squares fitting the points (x,y) to a power y : x -> a*x^b,
        /// returning a function y' for the best fitting line.
        /// </summary>
        public static Func<double, double> PowerFunc(double[] x, double[] y, DirectRegressionMethod method = DirectRegressionMethod.QR)
        {
            (double a, double b) = Power(x, y, method);
            return z => a * Math.Pow(z, b);
        }

        /// <summary>
        /// Least-Squares fitting the points (x,y) to a k-order polynomial y : x -> p0 + p1*x + p2*x^2 + ... + pk*x^k,
        /// returning its best fitting parameters as [p0, p1, p2, ..., pk] array, compatible with Polynomial.Evaluate.
        /// A polynomial with order/degree k has (k+1) coefficients and thus requires at least (k+1) samples.
        /// </summary>
        public static double[] Polynomial(double[] x, double[] y, int order, DirectRegressionMethod method = DirectRegressionMethod.QR)
        {
            var design = Matrix<double>.Build.Dense(x.Length, order + 1, (i, j) => Math.Pow(x[i], j));
            return MultipleRegression.DirectMethod(design, Vector<double>.Build.Dense(y), method).ToArray();
        }

        /// <summary>
        /// Least-Squares fitting the points (x,y) to a k-order polynomial y : x -> p0 + p1*x + p2*x^2 + ... + pk*x^k,
        /// returning a function y' for the best fitting polynomial.
        /// A polynomial with order/degree k has (k+1) coefficients and thus requires at least (k+1) samples.
        /// </summary>
        public static Func<double, double> PolynomialFunc(double[] x, double[] y, int order, DirectRegressionMethod method = DirectRegressionMethod.QR)
        {
            var parameters = Polynomial(x, y, order, method);
            return z => Numerics.Polynomial.Evaluate(z, parameters);
        }

        /// <summary>
        /// Weighted Least-Squares fitting the points (x,y) and weights w to a k-order polynomial y : x -> p0 + p1*x + p2*x^2 + ... + pk*x^k,
        /// returning its best fitting parameters as [p0, p1, p2, ..., pk] array, compatible with Polynomial.Evaluate.
        /// A polynomial with order/degree k has (k+1) coefficients and thus requires at least (k+1) samples.
        /// </summary>
        public static double[] PolynomialWeighted(double[] x, double[] y, double[] w, int order)
        {
            var design = Matrix<double>.Build.Dense(x.Length, order + 1, (i, j) => Math.Pow(x[i], j));
            return WeightedRegression.Weighted(design, Vector<double>.Build.Dense(y), Matrix<double>.Build.Diagonal(w)).ToArray();
        }

        /// <summary>
        /// Least-Squares fitting the points (x,y) to an arbitrary linear combination y : x -> p0*f0(x) + p1*f1(x) + ... + pk*fk(x),
        /// returning its best fitting parameters as [p0, p1, p2, ..., pk] array.
        /// </summary>
        public static double[] LinearCombination(double[] x, double[] y, params Func<double,double>[] functions)
        {
            var design = Matrix<double>.Build.Dense(x.Length, functions.Length, (i, j) => functions[j](x[i]));
            return MultipleRegression.QR(design, Vector<double>.Build.Dense(y)).ToArray();
        }

        /// <summary>
        /// Least-Squares fitting the points (x,y) to an arbitrary linear combination y : x -> p0*f0(x) + p1*f1(x) + ... + pk*fk(x),
        /// returning a function y' for the best fitting combination.
        /// </summary>
        public static Func<double, double> LinearCombinationFunc(double[] x, double[] y, params Func<double, double>[] functions)
        {
            var parameters = LinearCombination(x, y, functions);
            return z => functions.Zip(parameters, (f, p) => p*f(z)).Sum();
        }

        /// <summary>
        /// Least-Squares fitting the points (x,y) to an arbitrary linear combination y : x -> p0*f0(x) + p1*f1(x) + ... + pk*fk(x),
        /// returning its best fitting parameters as [p0, p1, p2, ..., pk] array.
        /// </summary>
        public static double[] LinearCombination(double[] x, double[] y, DirectRegressionMethod method, params Func<double, double>[] functions)
        {
            var design = Matrix<double>.Build.Dense(x.Length, functions.Length, (i, j) => functions[j](x[i]));
            return MultipleRegression.DirectMethod(design, Vector<double>.Build.Dense(y), method).ToArray();
        }

        /// <summary>
        /// Least-Squares fitting the points (x,y) to an arbitrary linear combination y : x -> p0*f0(x) + p1*f1(x) + ... + pk*fk(x),
        /// returning a function y' for the best fitting combination.
        /// </summary>
        public static Func<double, double> LinearCombinationFunc(double[] x, double[] y, DirectRegressionMethod method, params Func<double, double>[] functions)
        {
            var parameters = LinearCombination(x, y, method, functions);
            return z => functions.Zip(parameters, (f, p) => p*f(z)).Sum();
        }

        /// <summary>
        /// Least-Squares fitting the points (X,y) = ((x0,x1,..,xk),y) to a linear surface y : X -> p0*x0 + p1*x1 + ... + pk*xk,
        /// returning its best fitting parameters as [p0, p1, p2, ..., pk] array.
        /// If an intercept is added, its coefficient will be prepended to the resulting parameters.
        /// </summary>
        public static double[] MultiDim(double[][] x, double[] y, bool intercept = false, DirectRegressionMethod method = DirectRegressionMethod.NormalEquations)
        {
            return MultipleRegression.DirectMethod(x, y, intercept, method);
        }

        /// <summary>
        /// Least-Squares fitting the points (X,y) = ((x0,x1,..,xk),y) to a linear surface y : X -> p0*x0 + p1*x1 + ... + pk*xk,
        /// returning a function y' for the best fitting combination.
        /// If an intercept is added, its coefficient will be prepended to the resulting parameters.
        /// </summary>
        public static Func<double[], double> MultiDimFunc(double[][] x, double[] y, bool intercept = false, DirectRegressionMethod method = DirectRegressionMethod.NormalEquations)
        {
            var parameters = MultipleRegression.DirectMethod(x, y, intercept, method);
            return z => LinearAlgebraControl.Provider.DotProduct(parameters, z);
        }

        /// <summary>
        /// Weighted Least-Squares fitting the points (X,y) = ((x0,x1,..,xk),y) and weights w to a linear surface y : X -> p0*x0 + p1*x1 + ... + pk*xk,
        /// returning its best fitting parameters as [p0, p1, p2, ..., pk] array.
        /// </summary>
        public static double[] MultiDimWeighted(double[][] x, double[] y, double[] w)
        {
            return WeightedRegression.Weighted(x, y, w);
        }

        /// <summary>
        /// Least-Squares fitting the points (X,y) = ((x0,x1,..,xk),y) to an arbitrary linear combination y : X -> p0*f0(x) + p1*f1(x) + ... + pk*fk(x),
        /// returning its best fitting parameters as [p0, p1, p2, ..., pk] array.
        /// </summary>
        public static double[] LinearMultiDim(double[][] x, double[] y, params Func<double[], double>[] functions)
        {
            var design = Matrix<double>.Build.Dense(x.Length, functions.Length, (i, j) => functions[j](x[i]));
            return MultipleRegression.QR(design, Vector<double>.Build.Dense(y)).ToArray();
        }

        /// <summary>
        /// Least-Squares fitting the points (X,y) = ((x0,x1,..,xk),y) to an arbitrary linear combination y : X -> p0*f0(x) + p1*f1(x) + ... + pk*fk(x),
        /// returning a function y' for the best fitting combination.
        /// </summary>
        public static Func<double[], double> LinearMultiDimFunc(double[][] x, double[] y, params Func<double[], double>[] functions)
        {
            var parameters = LinearMultiDim(x, y, functions);
            return z => functions.Zip(parameters, (f, p) => p * f(z)).Sum();
        }

        /// <summary>
        /// Least-Squares fitting the points (X,y) = ((x0,x1,..,xk),y) to an arbitrary linear combination y : X -> p0*f0(x) + p1*f1(x) + ... + pk*fk(x),
        /// returning its best fitting parameters as [p0, p1, p2, ..., pk] array.
        /// </summary>
        public static double[] LinearMultiDim(double[][] x, double[] y, DirectRegressionMethod method, params Func<double[], double>[] functions)
        {
            var design = Matrix<double>.Build.Dense(x.Length, functions.Length, (i, j) => functions[j](x[i]));
            return MultipleRegression.DirectMethod(design, Vector<double>.Build.Dense(y), method).ToArray();
        }

        /// <summary>
        /// Least-Squares fitting the points (X,y) = ((x0,x1,..,xk),y) to an arbitrary linear combination y : X -> p0*f0(x) + p1*f1(x) + ... + pk*fk(x),
        /// returning a function y' for the best fitting combination.
        /// </summary>
        public static Func<double[], double> LinearMultiDimFunc(double[][] x, double[] y, DirectRegressionMethod method, params Func<double[], double>[] functions)
        {
            var parameters = LinearMultiDim(x, y, method, functions);
            return z => functions.Zip(parameters, (f, p) => p * f(z)).Sum();
        }

        /// <summary>
        /// Least-Squares fitting the points (T,y) = (T,y) to an arbitrary linear combination y : X -> p0*f0(T) + p1*f1(T) + ... + pk*fk(T),
        /// returning its best fitting parameters as [p0, p1, p2, ..., pk] array.
        /// </summary>
        public static double[] LinearGeneric<T>(T[] x, double[] y, params Func<T, double>[] functions)
        {
            var design = Matrix<double>.Build.Dense(x.Length, functions.Length, (i, j) => functions[j](x[i]));
            return MultipleRegression.QR(design, Vector<double>.Build.Dense(y)).ToArray();
        }

        /// <summary>
        /// Least-Squares fitting the points (T,y) = (T,y) to an arbitrary linear combination y : X -> p0*f0(T) + p1*f1(T) + ... + pk*fk(T),
        /// returning a function y' for the best fitting combination.
        /// </summary>
        public static Func<T, double> LinearGenericFunc<T>(T[] x, double[] y, params Func<T, double>[] functions)
        {
            var parameters = LinearGeneric(x, y, functions);
            return z => functions.Zip(parameters, (f, p) => p * f(z)).Sum();
        }

        /// <summary>
        /// Least-Squares fitting the points (T,y) = (T,y) to an arbitrary linear combination y : X -> p0*f0(T) + p1*f1(T) + ... + pk*fk(T),
        /// returning its best fitting parameters as [p0, p1, p2, ..., pk] array.
        /// </summary>
        public static double[] LinearGeneric<T>(T[] x, double[] y, DirectRegressionMethod method, params Func<T, double>[] functions)
        {
            var design = Matrix<double>.Build.Dense(x.Length, functions.Length, (i, j) => functions[j](x[i]));
            return MultipleRegression.DirectMethod(design, Vector<double>.Build.Dense(y), method).ToArray();
        }

        /// <summary>
        /// Least-Squares fitting the points (T,y) = (T,y) to an arbitrary linear combination y : X -> p0*f0(T) + p1*f1(T) + ... + pk*fk(T),
        /// returning a function y' for the best fitting combination.
        /// </summary>
        public static Func<T, double> LinearGenericFunc<T>(T[] x, double[] y, DirectRegressionMethod method, params Func<T, double>[] functions)
        {
            var parameters = LinearGeneric(x, y, method, functions);
            return z => functions.Zip(parameters, (f, p) => p * f(z)).Sum();
        }

        /// <summary>
        /// Non-linear least-squares fitting the points (x,y) to an arbitrary function y : x -> f(p, x),
        /// returning its best fitting parameter p.
        /// </summary>
        public static double Curve(double[] x, double[] y, Func<double, double, double> f, double initialGuess, double tolerance = 1e-8, int maxIterations = 1000)
        {
            return FindMinimum.OfScalarFunction(p => Distance.Euclidean(Generate.Map(x, t => f(p, t)), y), initialGuess, tolerance, maxIterations);
        }

        /// <summary>
        /// Non-linear least-squares fitting the points (x,y) to an arbitrary function y : x -> f(p0, p1, x),
        /// returning its best fitting parameter p0 and p1.
        /// </summary>
        public static (double P0, double P1) Curve(double[] x, double[] y, Func<double, double, double, double> f, double initialGuess0, double initialGuess1, double tolerance = 1e-8, int maxIterations = 1000)
        {
            return FindMinimum.OfFunction((p0, p1) => Distance.Euclidean(Generate.Map(x, t => f(p0, p1, t)), y), initialGuess0, initialGuess1, tolerance, maxIterations);
        }

        /// <summary>
        /// Non-linear least-squares fitting the points (x,y) to an arbitrary function y : x -> f(p0, p1, p2, x),
        /// returning its best fitting parameter p0, p1 and p2.
        /// </summary>
        public static (double P0, double P1, double P2) Curve(double[] x, double[] y, Func<double, double, double, double, double> f, double initialGuess0, double initialGuess1, double initialGuess2, double tolerance = 1e-8, int maxIterations = 1000)
        {
            return FindMinimum.OfFunction((p0, p1, p2) => Distance.Euclidean(Generate.Map(x, t => f(p0, p1, p2, t)), y), initialGuess0, initialGuess1, initialGuess2, tolerance, maxIterations);
        }

        /// <summary>
        /// Non-linear least-squares fitting the points (x,y) to an arbitrary function y : x -> f(p0, p1, p2, p3, x),
        /// returning its best fitting parameter p0, p1, p2 and p3.
        /// </summary>
        public static (double P0, double P1, double P2, double P3) Curve(double[] x, double[] y, Func<double, double, double, double, double, double> f, double initialGuess0, double initialGuess1, double initialGuess2, double initialGuess3, double tolerance = 1e-8, int maxIterations = 1000)
        {
            return FindMinimum.OfFunction((p0, p1, p2, p3) => Distance.Euclidean(Generate.Map(x, t => f(p0, p1, p2, p3, t)), y), initialGuess0, initialGuess1, initialGuess2, initialGuess3, tolerance, maxIterations);
        }

        /// <summary>
        /// Non-linear least-squares fitting the points (x,y) to an arbitrary function y : x -> f(p0, p1, p2, p3, p4, x),
        /// returning its best fitting parameter p0, p1, p2, p3 and p4.
        /// </summary>
        public static (double P0, double P1, double P2, double P3, double P4) Curve(double[] x, double[] y, Func<double, double, double, double, double, double, double> f, double initialGuess0, double initialGuess1, double initialGuess2, double initialGuess3, double initialGuess4, double tolerance = 1e-8, int maxIterations = 1000)
        {
            return FindMinimum.OfFunction((p0, p1, p2, p3, p4) => Distance.Euclidean(Generate.Map(x, t => f(p0, p1, p2, p3, p4, t)), y), initialGuess0, initialGuess1, initialGuess2, initialGuess3, initialGuess4, tolerance, maxIterations);
        }

        /// <summary>
        /// Non-linear least-squares fitting the points (x,y) to an arbitrary function y : x -> f(p, x),
        /// returning a function y' for the best fitting curve.
        /// </summary>
        public static Func<double, double> CurveFunc(double[] x, double[] y, Func<double, double, double> f, double initialGuess, double tolerance = 1e-8, int maxIterations = 1000)
        {
            var parameters = Curve(x, y, f, initialGuess, tolerance, maxIterations);
            return z => f(parameters, z);
        }

        /// <summary>
        /// Non-linear least-squares fitting the points (x,y) to an arbitrary function y : x -> f(p0, p1, x),
        /// returning a function y' for the best fitting curve.
        /// </summary>
        public static Func<double, double> CurveFunc(double[] x, double[] y, Func<double, double, double, double> f, double initialGuess0, double initialGuess1, double tolerance = 1e-8, int maxIterations = 1000)
        {
            var (p0, p1) = Curve(x, y, f, initialGuess0, initialGuess1, tolerance, maxIterations);
            return z => f(p0, p1, z);
        }

        /// <summary>
        /// Non-linear least-squares fitting the points (x,y) to an arbitrary function y : x -> f(p0, p1, p2, x),
        /// returning a function y' for the best fitting curve.
        /// </summary>
        public static Func<double, double> CurveFunc(double[] x, double[] y, Func<double, double, double, double, double> f, double initialGuess0, double initialGuess1, double initialGuess2, double tolerance = 1e-8, int maxIterations = 1000)
        {
            var (p0, p1, p2) = Curve(x, y, f, initialGuess0, initialGuess1, initialGuess2, tolerance, maxIterations);
            return z => f(p0, p1, p2, z);
        }

        /// <summary>
        /// Non-linear least-squares fitting the points (x,y) to an arbitrary function y : x -> f(p0, p1, p2, p3, x),
        /// returning a function y' for the best fitting curve.
        /// </summary>
        public static Func<double, double> CurveFunc(double[] x, double[] y, Func<double, double, double, double, double, double> f, double initialGuess0, double initialGuess1, double initialGuess2, double initialGuess3, double tolerance = 1e-8, int maxIterations = 1000)
        {
            var (p0, p1, p2, p3) = Curve(x, y, f, initialGuess0, initialGuess1, initialGuess2, initialGuess3, tolerance, maxIterations);
            return z => f(p0, p1, p2, p3, z);
        }

        /// <summary>
        /// Non-linear least-squares fitting the points (x,y) to an arbitrary function y : x -> f(p0, p1, p2, p3, p4, x),
        /// returning a function y' for the best fitting curve.
        /// </summary>
        public static Func<double, double> CurveFunc(double[] x, double[] y, Func<double, double, double, double, double, double, double> f, double initialGuess0, double initialGuess1, double initialGuess2, double initialGuess3, double initialGuess4, double tolerance = 1e-8, int maxIterations = 1000)
        {
            (double p0, double p1, double p2, double p3, double p4) = Curve(x, y, f, initialGuess0, initialGuess1, initialGuess2, initialGuess3, initialGuess4, tolerance, maxIterations);
            return z => f(p0, p1, p2, p3, p4, z);
        }
    }
}
