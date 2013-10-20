// <copyright file="Fit.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
// 
// Copyright (c) 2009-2013 Math.NET
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

namespace MathNet.Numerics
{
    /// <summary>
    /// Least-Squares Curve Fitting Routines
    /// </summary>
    public static class Fit
    {
        /// <summary>
        /// Least-Squares fitting the points (x,y) to a line y : x -> a+b*x,
        /// returning its best fitting parameters as [a, b] array,
        /// where a is the intercept and b the slope.
        /// </summary>
        public static Tuple<double, double> Line(double[] x, double[] y)
        {
            return SimpleRegression.Fit(x, y);
        }

        /// <summary>
        /// Least-Squares fitting the points (x,y) to a line y : x -> a+b*x,
        /// returning a function y' for the best fitting line.
        /// </summary>
        public static Func<double, double> LineFunc(double[] x, double[] y)
        {
            var parameters = SimpleRegression.Fit(x, y);
            double intercept = parameters.Item1, slope = parameters.Item2;
            return z => intercept + slope*z;
        }

        /// <summary>
        /// Least-Squares fitting the points (x,y) to a k-order polynomial y : x -> p0 + p1*x + p2*x^2 + ... + pk*x^k,
        /// returning its best fitting parameters as [p0, p1, p2, ..., pk] array, compatible with Evaluate.Polynomial.
        /// </summary>
        public static double[] Polynomial(double[] x, double[] y, int order)
        {
            var design = Matrix<double>.Build.DenseOfColumns(x.Length, order + 1, Enumerable.Range(0, order + 1).Select(j => x.Select(xi => Math.Pow(xi, j))));
            return MultipleRegression.QR(design, Vector<double>.Build.Dense(y)).ToArray();
        }

        /// <summary>
        /// Least-Squares fitting the points (x,y) to a k-order polynomial y : x -> p0 + p1*x + p2*x^2 + ... + pk*x^k,
        /// returning a function y' for the best fitting polynomial.
        /// </summary>
        public static Func<double, double> PolynomialFunc(double[] x, double[] y, int order)
        {
            var parameters = Polynomial(x, y, order);
            return z => Evaluate.Polynomial(z, parameters);
        }

        /// <summary>
        /// Least-Squares fitting the points (x,y) to an arbitrary linear combination y : x -> p0*f0(x) + p1*f1(x) + ... + pk*fk(x),
        /// returning its best fitting parameters as [p0, p1, p2, ..., pk] array.
        /// </summary>
        public static double[] LinearCombination(double[] x, double[] y, params Func<double,double>[] functions)
        {
            var design = Matrix<double>.Build.DenseOfColumns(x.Length, functions.Length, functions.Select(f => x.Select(xi => f(xi))));
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
        /// Least-Squares fitting the points (X,y) = ((x0,x1,..,xk),y) to an arbitrary linear combination y : X -> p0*f0(x) + p1*f1(x) + ... + pk*fk(x),
        /// returning its best fitting parameters as [p0, p1, p2, ..., pk] array.
        /// </summary>
        public static double[] LinearMultiDim(double[][] x, double[] y, params Func<double[], double>[] functions)
        {
            var design = Matrix<double>.Build.DenseOfRows(x.Select(xi => functions.Select(f => f(xi))));
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
        /// Least-Squares fitting the points (T,y) = (T,y) to an arbitrary linear combination y : X -> p0*f0(T) + p1*f1(T) + ... + pk*fk(T),
        /// returning its best fitting parameters as [p0, p1, p2, ..., pk] array.
        /// </summary>
        public static double[] LinearGeneric<T>(T[] x, double[] y, params Func<T, double>[] functions)
        {
            var design = Matrix<double>.Build.DenseOfRows(x.Select(xi => functions.Select(f => f(xi))));
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
    }
}
