// <copyright file="Combinatorics.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
//
// Copyright (c) 2009-2010 Math.NET
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

namespace MathNet.Numerics
{
    using System;

    /// <summary>
    /// Enumerative Combinatorics and Counting.
    /// </summary>
    public static class Combinatorics
    {
        /// <summary>
        /// Counts the number of possible variations without repetition.
        /// The order matters and each object can be chosen only once.
        /// </summary>
        /// <param name="n">Number of elements in the set.</param>
        /// <param name="k">Number of elements to choose from the set. Each element is chosen at most once.</param>
        /// <returns>Maximum number of distinct variations.</returns>
        public static double Variations(int n, int k)
        {
            if (k < 0 || n < 0 || k > n)
            {
                return 0;
            }

            return Math.Floor(
                0.5 + Math.Exp(
                          SpecialFunctions.FactorialLn(n)
                          - SpecialFunctions.FactorialLn(n - k)));
        }

        /// <summary>
        /// Counts the number of possible variations with repetition.
        /// The order matters and each object can be chosen more than once.
        /// </summary>
        /// <param name="n">Number of elements in the set.</param>
        /// <param name="k">Number of elements to choose from the set. Each element is chosen 0, 1 or multiple times.</param>
        /// <returns>Maximum number of distinct variations with repetition.</returns>
        public static double VariationsWithRepetition(int n, int k)
        {
            if (k < 0 || n < 0)
            {
                return 0;
            }

            return Math.Pow(n, k);
        }

        /// <summary>
        /// Counts the number of possible combinations without repetition.
        /// The order does not matter and each object can be chosen only once.
        /// </summary>
        /// <param name="n">Number of elements in the set.</param>
        /// <param name="k">Number of elements to choose from the set. Each element is chosen at most once.</param>
        /// <returns>Maximum number of combinations.</returns>
        public static double Combinations(int n, int k)
        {
            return SpecialFunctions.Binomial(n, k);
        }

        /// <summary>
        /// Counts the number of possible combinations with repetition.
        /// The order does not matter and an object can be chosen more than once.
        /// </summary>
        /// <param name="n">Number of elements in the set.</param>
        /// <param name="k">Number of elements to choose from the set. Each element is chosen 0, 1 or multiple times.</param>
        /// <returns>Maximum number of combinations with repetition.</returns>
        public static double CombinationsWithRepetition(int n, int k)
        {
            if (k < 0 || n < 0 || (n == 0 && k > 0))
            {
                return 0;
            }

            if (n == 0 && k == 0)
            {
                return 1;
            }

            return Math.Floor(
                0.5 + Math.Exp(
                          SpecialFunctions.FactorialLn(n + k - 1)
                          - SpecialFunctions.FactorialLn(k)
                          - SpecialFunctions.FactorialLn(n - 1)));
        }

        /// <summary>
        /// Counts the number of possible permutations (without repetition). 
        /// </summary>
        /// <param name="n">Number of (distinguishable) elements in the set.</param>
        /// <returns>Maximum number of permutations without repetition.</returns>
        public static double Permutations(int n)
        {
            return SpecialFunctions.Factorial(n);
        }
    }
}