// <copyright file="Factorial.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
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

using System;
using MathNet.Numerics.Properties;

#if !NOSYSNUMERICS
using System.Numerics;
#endif

// ReSharper disable CheckNamespace
namespace MathNet.Numerics
// ReSharper restore CheckNamespace
{
    public partial class SpecialFunctions
    {
        const int FactorialMaxArgument = 170;
        static double[] _factorialCache;

        /// <summary>
        /// Initializes static members of the SpecialFunctions class.
        /// </summary>
        static SpecialFunctions()
        {
            InitializeFactorial();
        }

        static void InitializeFactorial()
        {
            _factorialCache = new double[FactorialMaxArgument + 1];
            _factorialCache[0] = 1.0;
            for (int i = 1; i < _factorialCache.Length; i++)
            {
                _factorialCache[i] = _factorialCache[i - 1]*i;
            }
        }

        /// <summary>
        /// Computes the factorial function x -> x! of an integer number > 0. The function can represent all number up
        /// to 22! exactly, all numbers up to 170! using a double representation. All larger values will overflow.
        /// </summary>
        /// <returns>A value value! for value > 0</returns>
        /// <remarks>
        /// If you need to multiply or divide various such factorials, consider using the logarithmic version
        /// <see cref="FactorialLn"/> instead so you can add instead of multiply and subtract instead of divide, and
        /// then exponentiate the result using <see cref="System.Math.Exp"/>. This will also circumvent the problem that
        /// factorials become very large even for small parameters.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException" />
        public static double Factorial(int x)
        {
            if (x < 0)
            {
                throw new ArgumentOutOfRangeException("x", Resources.ArgumentPositive);
            }

            if (x < _factorialCache.Length)
            {
                return _factorialCache[x];
            }

            return double.PositiveInfinity;
        }

#if !NOSYSNUMERICS
        /// <summary>
        /// Computes the factorial of an integer.
        /// </summary>
        public static BigInteger Factorial(BigInteger x)
        {
            if (x < 0)
            {
                throw new ArgumentOutOfRangeException("x", Resources.ArgumentPositive);
            }

            if (x == 0)
            {
                return BigInteger.One;
            }

            BigInteger r = x;
            while (--x > 1)
            {
                r *= x;
            }

            return r;
        }
#endif

        /// <summary>
        /// Computes the logarithmic factorial function x -> ln(x!) of an integer number > 0.
        /// </summary>
        /// <returns>A value value! for value > 0</returns>
        public static double FactorialLn(int x)
        {
            if (x < 0)
            {
                throw new ArgumentOutOfRangeException("x", Resources.ArgumentPositive);
            }

            if (x <= 1)
            {
                return 0d;
            }

            if (x < _factorialCache.Length)
            {
                return Math.Log(_factorialCache[x]);
            }

            return GammaLn(x + 1.0);
        }

        /// <summary>
        /// Computes the binomial coefficient: n choose k.
        /// </summary>
        /// <param name="n">A nonnegative value n.</param>
        /// <param name="k">A nonnegative value h.</param>
        /// <returns>The binomial coefficient: n choose k.</returns>
        public static double Binomial(int n, int k)
        {
            if (k < 0 || n < 0 || k > n)
            {
                return 0.0;
            }

            return Math.Floor(0.5 + Math.Exp(FactorialLn(n) - FactorialLn(k) - FactorialLn(n - k)));
        }

        /// <summary>
        /// Computes the natural logarithm of the binomial coefficient: ln(n choose k).
        /// </summary>
        /// <param name="n">A nonnegative value n.</param>
        /// <param name="k">A nonnegative value h.</param>
        /// <returns>The logarithmic binomial coefficient: ln(n choose k).</returns>
        public static double BinomialLn(int n, int k)
        {
            if (k < 0 || n < 0 || k > n)
            {
                return double.NegativeInfinity;
            }

            return FactorialLn(n) - FactorialLn(k) - FactorialLn(n - k);
        }

        /// <summary>
        /// Computes the multinomial coefficient: n choose n1, n2, n3, ...
        /// </summary>
        /// <param name="n">A nonnegative value n.</param>
        /// <param name="ni">An array of nonnegative values that sum to <paramref name="n"/>.</param>
        /// <returns>The multinomial coefficient.</returns>
        /// <exception cref="ArgumentNullException">if <paramref name="ni"/> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException">If <paramref name="n"/> or any of the <paramref name="ni"/> are negative.</exception>
        /// <exception cref="ArgumentException">If the sum of all <paramref name="ni"/> is not equal to <paramref name="n"/>.</exception>
        public static double Multinomial(int n, int[] ni)
        {
            if (n < 0)
            {
                throw new ArgumentException(Resources.ArgumentMustBePositive, "n");
            }

            if (ni == null)
            {
                throw new ArgumentNullException("ni");
            }

            int sum = 0;
            double ret = FactorialLn(n);
            for (int i = 0; i < ni.Length; i++)
            {
                if (ni[i] < 0)
                {
                    throw new ArgumentException(Resources.ArgumentMustBePositive, "ni[" + i + "]");
                }

                ret -= FactorialLn(ni[i]);
                sum += ni[i];
            }

            // Before returning, check that the sum of all elements was equal to n.
            if (sum != n)
            {
                throw new ArgumentException(Resources.ArgumentParameterSetInvalid, "ni");
            }

            return Math.Floor(0.5 + Math.Exp(ret));
        }
    }
}
