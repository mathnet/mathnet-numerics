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
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Generic.Factorization;
using MathNet.Numerics.Properties;
using MathNet.Numerics.Statistics;

namespace MathNet.Numerics
{
    /// <summary>
    /// Least-Squares Curve Fitting Routines
    /// </summary>
    public static class Fit
    {
        /// <summary>
        /// Least-Squares fitting the points (x,y) to a line y : x -> a+b*x,
        /// returning its best fitting parameters as [a, b] array.
        /// </summary>
        public static double[] Line(double[] x, double[] y)
        {
            if (x == null) throw new ArgumentNullException("x");
            if (y == null) throw new ArgumentNullException("y");
            if (x.Length != y.Length) throw new ArgumentException(Resources.ArgumentVectorsSameLength);
            if (x.Length <= 1) throw new ArgumentException(string.Format(Resources.ArrayTooSmall, 2));

            var mx = ArrayStatistics.Mean(x);
            var my = ArrayStatistics.Mean(y);

            double xsum = x[0];
            double xvariance = 0;
            double covariance = (x[0] - mx)*(y[0] - my);
            for (int i = 1; i < x.Length; i++)
            {
                covariance += (x[i] - mx)*(y[i] - my);

                xsum += x[i];
                double diff = (i + 1)*x[i] - xsum;
                xvariance += (diff*diff)/((i + 1)*i);
            }

            var b = covariance/xvariance;
            return new[] {my - b*mx, b};

            // General Solution:
            //return DenseMatrix
            //    .OfColumns(x.Length, 2, new[] {DenseVector.Create(x.Length, i => 1.0), new DenseVector(x)})
            //    .QR(QRMethod.Thin).Solve(new DenseVector(y))
            //    .ToArray();
        }

        /// <summary>
        /// Least-Squares fitting the points (x,y) to a line y : x -> a+b*x,
        /// returning a function y' for the best fitting line.
        /// </summary>
        public static Func<double, double> LineFunc(double[] x, double[] y)
        {
            var parameters = Line(x, y);
            double a = parameters[0], b = parameters[1];
            return z => a + b*z;
        }

        /// <summary>
        /// Least-Squares fitting the points (x,y) to a k-order polynomial y : x -> p0 + p1*x + p2*x^2 + ... + pk*x^k,
        /// returning its best fitting parameters as [p0, p1, p2, ..., pk] array, compatible with Evaluate.Polynomial.
        /// </summary>
        public static double[] Polynomial(double[] x, double[] y, int order)
        {
            return DenseMatrix
                .OfColumns(x.Length, order + 1, Enumerable.Range(0, order + 1).Select(j => DenseVector.Create(x.Length, i => Math.Pow(x[i], j))))
                .QR(QRMethod.Thin).Solve(new DenseVector(y))
                .ToArray();
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
            // TODO: consider to use a specific algorithm instead
            return DenseMatrix
                .OfColumns(x.Length, functions.Length, functions.Select(f => DenseVector.Create(x.Length, i => f(x[i]))))
                .QR(QRMethod.Thin).Solve(new DenseVector(y))
                .ToArray();
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
    }
}
