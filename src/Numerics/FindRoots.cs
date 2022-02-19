// <copyright file="FindRoots.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2014 Math.NET
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
using MathNet.Numerics.RootFinding;
using Complex = System.Numerics.Complex;

namespace MathNet.Numerics
{
    public static class FindRoots
    {
        /// <summary>Find a solution of the equation f(x)=0.</summary>
        /// <param name="f">The function to find roots from.</param>
        /// <param name="lowerBound">The low value of the range where the root is supposed to be.</param>
        /// <param name="upperBound">The high value of the range where the root is supposed to be.</param>
        /// <param name="accuracy">Desired accuracy. The root will be refined until the accuracy or the maximum number of iterations is reached. Example: 1e-14.</param>
        /// <param name="maxIterations">Maximum number of iterations. Example: 100.</param>
        public static double OfFunction(Func<double, double> f, double lowerBound, double upperBound, double accuracy = 1e-8, int maxIterations = 100)
        {
            if (!ZeroCrossingBracketing.ExpandReduce(f, ref lowerBound, ref upperBound, 1.6, maxIterations, maxIterations*10))
            {
                throw new NonConvergenceException("The algorithm has failed, exceeded the number of iterations allowed or there is no root within the provided bounds.");
            }

            if (Brent.TryFindRoot(f, lowerBound, upperBound, accuracy, maxIterations, out var root))
            {
                return root;
            }

            if (Bisection.TryFindRoot(f, lowerBound, upperBound, accuracy, maxIterations, out root))
            {
                return root;
            }

            throw new NonConvergenceException("The algorithm has failed, exceeded the number of iterations allowed or there is no root within the provided bounds.");
        }

        /// <summary>Find a solution of the equation f(x)=0.</summary>
        /// <param name="f">The function to find roots from.</param>
        /// <param name="df">The first derivative of the function to find roots from.</param>
        /// <param name="lowerBound">The low value of the range where the root is supposed to be.</param>
        /// <param name="upperBound">The high value of the range where the root is supposed to be.</param>
        /// <param name="accuracy">Desired accuracy. The root will be refined until the accuracy or the maximum number of iterations is reached. Example: 1e-14.</param>
        /// <param name="maxIterations">Maximum number of iterations. Example: 100.</param>
        public static double OfFunctionDerivative(Func<double, double> f, Func<double, double> df, double lowerBound, double upperBound, double accuracy = 1e-8, int maxIterations = 100)
        {
            double root;

            if (RobustNewtonRaphson.TryFindRoot(f, df, lowerBound, upperBound, accuracy, maxIterations, 20, out root))
            {
                return root;
            }

            return OfFunction(f, lowerBound, upperBound, accuracy, maxIterations);
        }

        /// <summary>
        /// Find both complex roots of the quadratic equation c + b*x + a*x^2 = 0.
        /// Note the special coefficient order ascending by exponent (consistent with polynomials).
        /// </summary>
        public static (Complex, Complex) Quadratic(double c, double b, double a)
        {
            if (b == 0d)
            {
                var t = new Complex(-c/a, 0d).SquareRoot();
                return (t, -t);
            }

            var q = b > 0d
                ? -0.5*(b + new Complex(b*b - 4*a*c, 0d).SquareRoot())
                : -0.5*(b - new Complex(b*b - 4*a*c, 0d).SquareRoot());

            return (q/a, c/q);
        }

        /// <summary>
        /// Find all three complex roots of the cubic equation d + c*x + b*x^2 + a*x^3 = 0.
        /// Note the special coefficient order ascending by exponent (consistent with polynomials).
        /// </summary>
        public static (Complex, Complex, Complex) Cubic(double d, double c, double b, double a)
        {
            return RootFinding.Cubic.Roots(d, c, b, a);
        }

        /// <summary>
        /// Find all roots of a polynomial by calculating the characteristic polynomial of the companion matrix
        /// </summary>
        /// <param name="coefficients">The coefficients of the polynomial in ascending order, e.g. new double[] {5, 0, 2} = "5 + 0 x^1 + 2 x^2"</param>
        /// <returns>The roots of the polynomial</returns>
        public static Complex[] Polynomial(double[] coefficients)
        {
            return new Polynomial(coefficients).Roots();
        }

        /// <summary>
        /// Find all roots of a polynomial by calculating the characteristic polynomial of the companion matrix
        /// </summary>
        /// <param name="polynomial">The polynomial.</param>
        /// <returns>The roots of the polynomial</returns>
        public static Complex[] Polynomial(Polynomial polynomial)
        {
            return polynomial.Roots();
        }

        /// <summary>
        /// Find all roots of the Chebychev polynomial of the first kind.
        /// </summary>
        /// <param name="degree">The polynomial order and therefore the number of roots.</param>
        /// <param name="intervalBegin">The real domain interval begin where to start sampling.</param>
        /// <param name="intervalEnd">The real domain interval end where to stop sampling.</param>
        /// <returns>Samples in [a,b] at (b+a)/2+(b-1)/2*cos(pi*(2i-1)/(2n))</returns>
        public static double[] ChebychevPolynomialFirstKind(int degree, double intervalBegin = -1d, double intervalEnd = 1d)
        {
            if (degree < 1)
            {
                return Array.Empty<double>();
            }

            // transform to map to [-1..1] interval
            double location = 0.5*(intervalBegin + intervalEnd);
            double scale = 0.5*(intervalEnd - intervalBegin);

            // evaluate first kind chebychev nodes
            double angleFactor = Constants.Pi/(2*degree);

            var samples = new double[degree];
            for (int i = 0; i < samples.Length; i++)
            {
                samples[i] = location + scale*Math.Cos(((2*i) + 1)*angleFactor);
            }
            return samples;
        }

        /// <summary>
        /// Find all roots of the Chebychev polynomial of the second kind.
        /// </summary>
        /// <param name="degree">The polynomial order and therefore the number of roots.</param>
        /// <param name="intervalBegin">The real domain interval begin where to start sampling.</param>
        /// <param name="intervalEnd">The real domain interval end where to stop sampling.</param>
        /// <returns>Samples in [a,b] at (b+a)/2+(b-1)/2*cos(pi*i/(n-1))</returns>
        public static double[] ChebychevPolynomialSecondKind(int degree, double intervalBegin = -1d, double intervalEnd = 1d)
        {
            if (degree < 1)
            {
                return Array.Empty<double>();
            }

            // transform to map to [-1..1] interval
            double location = 0.5*(intervalBegin + intervalEnd);
            double scale = 0.5*(intervalEnd - intervalBegin);

            // evaluate second kind chebychev nodes
            double angleFactor = Constants.Pi/(degree + 1);

            var samples = new double[degree];
            for (int i = 0; i < samples.Length; i++)
            {
                samples[i] = location + scale*Math.Cos((i + 1)*angleFactor);
            }
            return samples;
        }
    }
}
