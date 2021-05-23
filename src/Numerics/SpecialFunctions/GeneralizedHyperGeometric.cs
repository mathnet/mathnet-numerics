// <copyright file="GeneralizedHyperGeometric.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2020 Math.NET
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
//    Andrew J. Willshire
// </contribution>

using System;
using System.Linq;

// ReSharper disable once CheckNamespace
namespace MathNet.Numerics
{
    public static partial class SpecialFunctions
    {
        //Rising and falling factorials - reference here:
        //https://en.wikipedia.org/wiki/Falling_and_rising_factorials

        /// <summary>
        /// Computes the Rising Factorial (Pochhammer function)  x -> (x)n, n>= 0. see: https://en.wikipedia.org/wiki/Falling_and_rising_factorials
        /// </summary>
        /// <returns>The real value of the Rising Factorial for x and n</returns>
        public static double RisingFactorial(double x, int n)
        {
                double accumulator = 1.0;

                for (int k = 0; k < n; k++)
                {
                    accumulator *= (x + k);
                }
                return accumulator;
            }

        /// <summary>
        /// Computes the Falling Factorial (Pochhammer function)  x -> x(n), n>= 0. see: https://en.wikipedia.org/wiki/Falling_and_rising_factorials
        /// </summary>
        /// <returns>The real value of the Falling Factorial for x and n</returns>
        public static double FallingFactorial(double x, int n)
        {
                double accumulator = 1.0;

                for (int k = 0; k < n; k++)
                {
                    accumulator *= (x - k);
                }
                return accumulator;
        }

        /// <summary>
        /// A generalized hypergeometric series is a power series in which the ratio of successive coefficients indexed by n is a rational function of n.
        /// This is the most common pFq(a1, ..., ap; b1,...,bq; z) representation
        /// see: https://en.wikipedia.org/wiki/Generalized_hypergeometric_function
        /// </summary>
        /// <param name="a">The list of coefficients in the numerator</param>
        /// <param name="b">The list of coefficients in the denominator</param>
        /// <param name="z">The variable in the power series</param>
        /// <returns>The value of the Generalized HyperGeometric Function.</returns>
        public static double GeneralizedHypergeometric(double[] a, double[] b, int z)
        {
            const double epsilon = 0.000000000000001;

            double cumulatives = 0.0;
            double currentIncrement;
            int n = 0;

            do
            {
                currentIncrement = HGIncrement(a, b, z, n);
                cumulatives += currentIncrement;
                n += 1;
            }
            while (Math.Abs(currentIncrement) > epsilon && Math.Abs(currentIncrement) > 0 && currentIncrement.IsFinite());

            return cumulatives;
        }

        //Calculate each iteration of the function
        static double HGIncrement(double[] a, double[] b, int z, int currentN)
        {
            double incrementAs = 1.0;
            double incrementBs = 1.0;

            double[] incrementAArray = new double[a.Length];
            double[] incrementBArray = new double[b.Length];

            for (int p = 0; p < a.Length; p++)
            {
                incrementAs *= RisingFactorial(a[p], currentN);
                incrementAArray[p] = RisingFactorial(a[p], currentN);
            }

            for (int q = 0; q < b.Length; q++)
            {
                incrementBs *= RisingFactorial(b[q], currentN);
                incrementBArray[q] = RisingFactorial(b[q], currentN);
            }

            double numZeros = (from x in incrementAArray where x == 0 select x).Count();
            double numPoles = (from x in incrementBArray where x == 0 select x).Count();

            if (numZeros > 0 && numZeros >= numPoles)
            {
                return 0.0;
            }

            if (numPoles > 0 && numPoles > numZeros)
            {
                return double.PositiveInfinity;
            }

            return incrementAs / incrementBs * Math.Pow(z, currentN) / Factorial(currentN);
        }

    }
}
