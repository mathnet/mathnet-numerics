// <copyright file="IntegerTheory.Euclid.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
// Copyright (c) 2009-2010 Math.NET
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
// </copyright>

namespace MathNet.Numerics.NumberTheory
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Number theory utility functions for integers.
    /// </summary>
    public static partial class IntegerTheory
    {
        /// <summary>
        /// Returns the greatest common divisor (<c>gcd</c>) of two integers using Euclid's algorithm.
        /// </summary>
        /// <param name="a">First Integer: a.</param>
        /// <param name="b">Second Integer: b.</param>
        /// <returns>Greatest common divisor <c>gcd</c>(a,b)</returns>
        public static long GreatestCommonDivisor(long a, long b)
        {
            while (b != 0)
            {
                var remainder = a % b;
                a = b;
                b = remainder;
            }

            return Math.Abs(a);
        }

        /// <summary>
        /// Returns the greatest common divisor (<c>gcd</c>) of a set of integers using Euclid's
        /// algorithm.
        /// </summary>
        /// <param name="integers">List of Integers.</param>
        /// <returns>Greatest common divisor <c>gcd</c>(list of integers)</returns>
        public static long GreatestCommonDivisor(IList<long> integers)
        {
            if (null == integers)
            {
                throw new ArgumentNullException("integers");
            }

            if (integers.Count == 0)
            {
                return 0;
            }

            var gcd = Math.Abs(integers[0]);

            for (var i = 1; (i < integers.Count) && (gcd > 1); i++)
            {
                gcd = GreatestCommonDivisor(gcd, integers[i]);
            }

            return gcd;
        }

        /// <summary>
        /// Returns the greatest common divisor (<c>gcd</c>) of a set of integers using Euclid's algorithm.
        /// </summary>
        /// <param name="integers">List of Integers.</param>
        /// <returns>Greatest common divisor <c>gcd</c>(list of integers)</returns>
        public static long GreatestCommonDivisor(params long[] integers)
        {
            return GreatestCommonDivisor((IList<long>)integers);
        }

        /// <summary>
        /// Computes the extended greatest common divisor, such that a*x + b*y = <c>gcd</c>(a,b).
        /// </summary>
        /// <param name="a">First Integer: a.</param>
        /// <param name="b">Second Integer: b.</param>
        /// <param name="x">Resulting x, such that a*x + b*y = <c>gcd</c>(a,b).</param>
        /// <param name="y">Resulting y, such that a*x + b*y = <c>gcd</c>(a,b)</param>
        /// <returns>Greatest common divisor <c>gcd</c>(a,b)</returns>
        /// <example>
        /// <code>
        /// long x,y,d;
        /// d = Fn.GreatestCommonDivisor(45,18,out x, out y);
        /// -> d == 9 &amp;&amp; x == 1 &amp;&amp; y == -2
        /// </code>
        /// The <c>gcd</c> of 45 and 18 is 9: 18 = 2*9, 45 = 5*9. 9 = 1*45 -2*18, therefore x=1 and y=-2.
        /// </example>
        public static long ExtendedGreatestCommonDivisor(
            long a, 
            long b, 
            out long x, 
            out long y)
        {
            long mp = 1, np = 0, m = 0, n = 1;

            while (b != 0)
            {
                long rem;
#if PORTABLE
                rem = a / b;
                var quot = a % b;
#else
                long quot = Math.DivRem(a, b, out rem);
#endif
                a = b;
                b = rem;

                var tmp = m;
                m = mp - (quot * m);
                mp = tmp;

                tmp = n;
                n = np - (quot * n);
                np = tmp;
            }

            if (a >= 0)
            {
                x = mp;
                y = np;
                return a;
            }

            x = -mp;
            y = -np;
            return -a;
        }

        /// <summary>
        /// Returns the least common multiple (<c>lcm</c>) of two integers using Euclid's algorithm.
        /// </summary>
        /// <param name="a">First Integer: a.</param>
        /// <param name="b">Second Integer: b.</param>
        /// <returns>Least common multiple <c>lcm</c>(a,b)</returns>
        public static long LeastCommonMultiple(long a, long b)
        {
            if ((a == 0) || (b == 0))
            {
                return 0;
            }

            return Math.Abs((a / GreatestCommonDivisor(a, b)) * b);
        }

        /// <summary>
        /// Returns the least common multiple (<c>lcm</c>) of a set of integers using Euclid's algorithm.
        /// </summary>
        /// <param name="integers">List of Integers.</param>
        /// <returns>Least common multiple <c>lcm</c>(list of integers)</returns>
        public static long LeastCommonMultiple(IList<long> integers)
        {
            if (null == integers)
            {
                throw new ArgumentNullException("integers");
            }

            if (integers.Count == 0)
            {
                return 1;
            }

            var lcm = Math.Abs(integers[0]);

            for (var i = 1; i < integers.Count; i++)
            {
                lcm = LeastCommonMultiple(lcm, integers[i]);
            }

            return lcm;
        }

        /// <summary>
        /// Returns the least common multiple (<c>lcm</c>) of a set of integers using Euclid's algorithm.
        /// </summary>
        /// <param name="integers">List of Integers.</param>
        /// <returns>Least common multiple <c>lcm</c>(list of integers)</returns>
        public static long LeastCommonMultiple(params long[] integers)
        {
            return LeastCommonMultiple((IList<long>)integers);
        }
    }
}