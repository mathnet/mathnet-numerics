// <copyright file="FindRoots.cs" company="Math.NET">
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
using MathNet.Numerics.Properties;
using MathNet.Numerics.RootFinding;

#if NOSYSNUMERICS
    using Complex = Numerics.Complex;
#else
    using Complex = System.Numerics.Complex;
#endif

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
            double root;

            if (Brent.TryFindRoot(f, lowerBound, upperBound, accuracy, maxIterations, out root))
            {
                return root;
            }

            if (Bisection.TryFindRoot(f, lowerBound, upperBound, accuracy, maxIterations, out root))
            {
                return root;
            }

            throw new NonConvergenceException(Resources.RootFindingFailed);
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

            if (Bisection.TryFindRoot(f, lowerBound, upperBound, accuracy, maxIterations, out root))
            {
                return root;
            }

            throw new NonConvergenceException(Resources.RootFindingFailed);
        }

        /// <summary>
        /// Find both complex roots of the quadratic equation c + b*x + a*x^2 = 0.
        /// Note the special coefficient order ascending by exponent (consistent with polynomials).
        /// </summary>
        public static Tuple<Complex, Complex> Quadratic(double c, double b, double a)
        {
            if (b == 0d)
            {
                var t = new Complex(-c/a, 0d).SquareRoot();
                return new Tuple<Complex, Complex>(t, -t);
            }

            var q = b > 0d
                ? -0.5*(b + new Complex(b*b - 4*a*c, 0d).SquareRoot())
                : -0.5*(b - new Complex(b*b - 4*a*c, 0d).SquareRoot());

            return new Tuple<Complex, Complex>(q/a, c/q);
        }
    }
}
