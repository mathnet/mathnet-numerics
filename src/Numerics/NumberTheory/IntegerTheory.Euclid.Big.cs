// <copyright file="IntegerTheory.Euclid.Big.cs" company="Math.NET">
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

#if !PORTABLE
namespace MathNet.Numerics.NumberTheory
{
    using System;
    using System.Collections.Generic;
    using System.Numerics;

    /// <summary>
    /// Number theory utility functions for integers.
    /// </summary>
    public static partial class IntegerTheory
    {
        /// <summary>
        /// Returns the greatest common divisor (<c>gcd</c>) of two big integers.
        /// </summary>
        /// <param name="a">First Integer: a.</param>
        /// <param name="b">Second Integer: b.</param>
        /// <returns>Greatest common divisor <c>gcd</c>(a,b)</returns>
        public static BigInteger GreatestCommonDivisor(BigInteger a, BigInteger b)
        {
            return BigInteger.GreatestCommonDivisor(a, b);
        }

        /// <summary>
        /// Returns the greatest common divisor (<c>gcd</c>) of a set of big integers.
        /// </summary>
        /// <param name="integers">List of Integers.</param>
        /// <returns>Greatest common divisor <c>gcd</c>(list of integers)</returns>
        public static BigInteger GreatestCommonDivisor(IList<BigInteger> integers)
        {
            if (null == integers)
            {
                throw new ArgumentNullException("integers");
            }

            if (integers.Count == 0)
            {
                return 0;
            }

            var gcd = BigInteger.Abs(integers[0]);

            for (int i = 1; (i < integers.Count) && (gcd > BigInteger.One); i++)
            {
                gcd = GreatestCommonDivisor(gcd, integers[i]);
            }

            return gcd;
        }

        /// <summary>
        /// Returns the greatest common divisor (<c>gcd</c>) of a set of big integers.
        /// </summary>
        /// <param name="integers">List of Integers.</param>
        /// <returns>Greatest common divisor <c>gcd</c>(list of integers)</returns>
        public static BigInteger GreatestCommonDivisor(params BigInteger[] integers)
        {
            return GreatestCommonDivisor((IList<BigInteger>)integers);
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
        public static BigInteger ExtendedGreatestCommonDivisor(
            BigInteger a,
            BigInteger b,
            out BigInteger x,
            out BigInteger y)
        {
            BigInteger mp = BigInteger.One, np = BigInteger.Zero, m = BigInteger.Zero, n = BigInteger.One;

            while (!b.IsZero)
            {
                BigInteger rem;
                BigInteger quot = BigInteger.DivRem(a, b, out rem);
                a = b;
                b = rem;

                BigInteger tmp = m;
                m = mp - (quot * m);
                mp = tmp;

                tmp = n;
                n = np - (quot * n);
                np = tmp;
            }

            if (a >= BigInteger.Zero)
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
        /// Returns the least common multiple (<c>lcm</c>) of two big integers.
        /// </summary>
        /// <param name="a">First Integer: a.</param>
        /// <param name="b">Second Integer: b.</param>
        /// <returns>Least common multiple <c>lcm</c>(a,b)</returns>
        public static BigInteger LeastCommonMultiple(BigInteger a, BigInteger b)
        {
            if (a.IsZero || b.IsZero)
            {
                return BigInteger.Zero;
            }

            return BigInteger.Abs((a / BigInteger.GreatestCommonDivisor(a, b)) * b);
        }

        /// <summary>
        /// Returns the least common multiple (<c>lcm</c>) of a set of big integers.
        /// </summary>
        /// <param name="integers">List of Integers.</param>
        /// <returns>Least common multiple <c>lcm</c>(list of integers)</returns>
        public static BigInteger LeastCommonMultiple(IList<BigInteger> integers)
        {
            if (null == integers)
            {
                throw new ArgumentNullException("integers");
            }

            if (integers.Count == 0)
            {
                return 1;
            }

            var lcm = BigInteger.Abs(integers[0]);

            for (int i = 1; i < integers.Count; i++)
            {
                lcm = LeastCommonMultiple(lcm, integers[i]);
            }

            return lcm;
        }

        /// <summary>
        /// Returns the least common multiple (<c>lcm</c>) of a set of big integers.
        /// </summary>
        /// <param name="integers">List of Integers.</param>
        /// <returns>Least common multiple <c>lcm</c>(list of integers)</returns>
        public static BigInteger LeastCommonMultiple(params BigInteger[] integers)
        {
            return LeastCommonMultiple((IList<BigInteger>)integers);
        }
    }
}
#endif
