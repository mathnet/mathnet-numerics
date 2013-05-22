// <copyright file="Evaluate.cs" company="Math.NET">
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

// <contribution>
//    CERN - European Laboratory for Particle Physics
//        http://www.docjar.com/html/api/cern/jet/math/Bessel.java.html
//        Copyright 1999 CERN - European Laboratory for Particle Physics.
//        Permission to use, copy, modify, distribute and sell this software and its documentation for any purpose 
//        is hereby granted without fee, provided that the above copyright notice appear in all copies and 
//        that both that copyright notice and this permission notice appear in supporting documentation. 
//        CERN makes no representations about the suitability of this software for any purpose. 
//        It is provided "as is" without expressed or implied warranty.
//    TOMS757 - Uncommon Special Functions (Fortran77) by Allan McLeod
//        http://people.sc.fsu.edu/~jburkardt/f77_src/toms757/toms757.html
//    Wei Wu
//    Cephes Math Library, Stephen L. Moshier
//    ALGLIB 2.0.1, Sergey Bochkanov
// </contribution>

// ReSharper disable CheckNamespace
namespace MathNet.Numerics
// ReSharper restore CheckNamespace
{
    using System;

    /// <summary>
    /// Evaluation functions, useful for function approximation.
    /// </summary>
    public static class Evaluate
    {
        /// <summary>
        /// Evaluate polynomials.
        /// </summary>
        /// <param name="coefficients">The coefficients of the polynomial.</param>
        /// <param name="z">The location where to evaluate the polynomial at.</param>
        /// <returns>the evaluation of the polynomial.</returns>
        [Obsolete("Use Polynomial(z, params coefficients) instead.")]
        public static double Polynomial(double[] coefficients, double z)
        {
            int count = coefficients.Length;
            double sum = coefficients[count - 1];
            for (int i = count - 2; i >= 0; --i)
            {
                sum *= z;
                sum += coefficients[i];
            }

            return sum;
        }

        /// <summary>
        /// Evaluate a polynomial at point x.
        /// Coefficients are ordered by power with power k at index k.
        /// Example: coefficients [3,-1,2] represent y=2x^2-x+3.
        /// </summary>
        /// <param name="z">The location where to evaluate the polynomial at.</param>
        /// <param name="coefficients">The coefficients of the polynomial, coefficient for power k at index k.</param>
        public static double Polynomial(double z, params double[] coefficients)
        {
            double sum = coefficients[coefficients.Length - 1];
            for (int i = coefficients.Length - 2; i >= 0; --i)
            {
                sum *= z;
                sum += coefficients[i];
            }

            return sum;
        }

        /// <summary>
        /// Numerically stable series summation
        /// </summary>
        /// <param name="nextSummand">provides the summands sequentially</param>
        /// <returns>Sum</returns>
        internal static double Series(Func<double> nextSummand)
        {
            double compensation = 0.0;
            double current;
            const double factor = 1 << 16;

            double sum = nextSummand();

            do
            {
                // Kahan Summation
                // NOTE (ruegg): do NOT optimize. Now, how to tell that the compiler?
                current = nextSummand();
                double y = current - compensation;
                double t = sum + y;
                compensation = t - sum;
                compensation -= y;
                sum = t;
            }
            while (Math.Abs(sum) < Math.Abs(factor * current));

            return sum;
        }

        /// <summary> Evaluates the series of Chebyshev polynomials Ti at argument x/2.
        /// The series is given by
        /// <pre>
        ///       N-1
        ///        - '
        /// y  =   >   coef[i] T (x/2)
        ///        -            i
        ///       i=0
        /// </pre>
        /// Coefficients are stored in reverse order, i.e. the zero
        /// order term is last in the array.  Note N is the number of
        /// coefficients, not the order.
        /// <p/>
        /// If coefficients are for the interval a to b, x must
        /// have been transformed to x -> 2(2x - b - a)/(b-a) before
        /// entering the routine.  This maps x from (a, b) to (-1, 1),
        /// over which the Chebyshev polynomials are defined.
        /// <p/>
        /// If the coefficients are for the inverted interval, in
        /// which (a, b) is mapped to (1/b, 1/a), the transformation
        /// required is x -> 2(2ab/x - b - a)/(b-a).  If b is infinity,
        /// this becomes x -> 4a/x - 1.
        /// <p/>
        /// SPEED:
        /// <p/>
        /// Taking advantage of the recurrence properties of the
        /// Chebyshev polynomials, the routine requires one more
        /// addition per loop than evaluating a nested polynomial of
        /// the same degree.
        /// </summary>
        /// <param name="coefficients">The coefficients of the polynomial.</param>
        /// <param name="x">Argument to the polynomial.</param>
        /// <remarks>
        /// Reference: https://bpm2.svn.codeplex.com/svn/Common.Numeric/Arithmetic.cs
        /// <p/>
        /// Marked as Deprecated in
        /// http://people.apache.org/~isabel/mahout_site/mahout-matrix/apidocs/org/apache/mahout/jet/math/Arithmetic.html
        /// </remarks>
        internal static double ChebyshevA(double[] coefficients, double x)
        {
            // TODO: Unify, normalize, then make public

            double b2;

            int p = 0;

            double b0 = coefficients[p++];
            double b1 = 0.0;
            int i = coefficients.Length - 1;

            do
            {
                b2 = b1;
                b1 = b0;
                b0 = x * b1 - b2 + coefficients[p++];
            }
            while (--i > 0);

            return (0.5 * (b0 - b2));
        }

        /// <summary>
        /// Summation of Chebyshev polynomials, using the Clenshaw method with Reinsch modification.
        /// </summary>
        /// <param name="n">The no. of terms in the sequence.</param>
        /// <param name="coefficients">The coefficients of the Chebyshev series, length n+1.</param>
        /// <param name="x">The value at which the series is to be evaluated.</param>
        /// <remarks>
        /// ORIGINAL AUTHOR:
        ///    Dr. Allan J. MacLeod; Dept. of Mathematics and Statistics, University of Paisley; High St., PAISLEY, SCOTLAND
        /// REFERENCES:
        ///    "An error analysis of the modified Clenshaw method for evaluating Chebyshev and Fourier series"
        ///    J. Oliver, J.I.M.A., vol. 20, 1977, pp379-391
        /// </remarks>
        internal static double ChebyshevSum(int n, double[] coefficients, double x)
        {
            // TODO: Unify, normalize, then make public

            // If |x|  < 0.6 use the standard Clenshaw method
            if (Math.Abs(x) < 0.6)
            {
                double u0 = 0.0;
                double u1 = 0.0;
                double u2 = 0.0;
                double xx = x + x;

                for (int i = n; i >= 0; i--)
                {
                    u2 = u1;
                    u1 = u0;
                    u0 = xx * u1 + coefficients[i] - u2;
                }

                return (u0 - u2) / 2.0;
            }

            // If ABS ( T )  > =  0.6 use the Reinsch modification
            // T > =  0.6 code
            if (x > 0.0)
            {
                double u1 = 0.0;
                double d1 = 0.0;
                double d2 = 0.0;
                double xx = (x - 0.5) - 0.5;
                xx = xx + xx;

                for (int i = n; i >= 0; i--)
                {
                    d2 = d1;
                    double u2 = u1;
                    d1 = xx * u2 + coefficients[i] + d2;
                    u1 = d1 + u2;
                }

                return (d1 + d2) / 2.0;
            }
            else
            {
                // T < =  -0.6 code
                double u1 = 0.0;
                double d1 = 0.0;
                double d2 = 0.0;
                double xx = (x + 0.5) + 0.5;
                xx = xx + xx;

                for (int i = n; i >= 0; i--)
                {
                    d2 = d1;
                    double u2 = u1;
                    d1 = xx * u2 + coefficients[i] - d2;
                    u1 = d1 - u2;
                }

                return (d1 - d2) / 2.0;
            }
        }
    }
}
