// <copyright file="Euclid.cs" company="Math.NET">
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
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using BigInteger = System.Numerics.BigInteger;

namespace MathNet.Numerics
{
    /// <summary>
    /// Integer number theory functions.
    /// </summary>
    public static class Euclid
    {
        /// <summary>
        /// Canonical Modulus. The result has the sign of the divisor.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Modulus(double dividend, double divisor)
        {
            return ((dividend%divisor) + divisor)%divisor;
        }

        /// <summary>
        /// Canonical Modulus. The result has the sign of the divisor.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Modulus(float dividend, float divisor)
        {
            return ((dividend%divisor) + divisor)%divisor;
        }

        /// <summary>
        /// Canonical Modulus. The result has the sign of the divisor.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Modulus(int dividend, int divisor)
        {
            return ((dividend%divisor) + divisor)%divisor;
        }

        /// <summary>
        /// Canonical Modulus. The result has the sign of the divisor.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long Modulus(long dividend, long divisor)
        {
            return ((dividend%divisor) + divisor)%divisor;
        }

        /// <summary>
        /// Canonical Modulus. The result has the sign of the divisor.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BigInteger Modulus(BigInteger dividend, BigInteger divisor)
        {
            return ((dividend%divisor) + divisor)%divisor;
        }

        /// <summary>
        /// Remainder (% operator). The result has the sign of the dividend.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Remainder(double dividend, double divisor)
        {
            return dividend%divisor;
        }

        /// <summary>
        /// Remainder (% operator). The result has the sign of the dividend.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Remainder(float dividend, float divisor)
        {
            return dividend%divisor;
        }

        /// <summary>
        /// Remainder (% operator). The result has the sign of the dividend.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Remainder(int dividend, int divisor)
        {
            return dividend%divisor;
        }

        /// <summary>
        /// Remainder (% operator). The result has the sign of the dividend.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long Remainder(long dividend, long divisor)
        {
            return dividend%divisor;
        }

        /// <summary>
        /// Remainder (% operator). The result has the sign of the dividend.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BigInteger Remainder(BigInteger dividend, BigInteger divisor)
        {
            return dividend%divisor;
        }

        /// <summary>
        /// Find out whether the provided 32 bit integer is an even number.
        /// </summary>
        /// <param name="number">The number to very whether it's even.</param>
        /// <returns>True if and only if it is an even number.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsEven(this int number)
        {
            return (number & 0x1) == 0x0;
        }

        /// <summary>
        /// Find out whether the provided 64 bit integer is an even number.
        /// </summary>
        /// <param name="number">The number to very whether it's even.</param>
        /// <returns>True if and only if it is an even number.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsEven(this long number)
        {
            return (number & 0x1) == 0x0;
        }

        /// <summary>
        /// Find out whether the provided 32 bit integer is an odd number.
        /// </summary>
        /// <param name="number">The number to very whether it's odd.</param>
        /// <returns>True if and only if it is an odd number.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsOdd(this int number)
        {
            return (number & 0x1) == 0x1;
        }

        /// <summary>
        /// Find out whether the provided 64 bit integer is an odd number.
        /// </summary>
        /// <param name="number">The number to very whether it's odd.</param>
        /// <returns>True if and only if it is an odd number.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsOdd(this long number)
        {
            return (number & 0x1) == 0x1;
        }

        /// <summary>
        /// Find out whether the provided 32 bit integer is a perfect power of two.
        /// </summary>
        /// <param name="number">The number to very whether it's a power of two.</param>
        /// <returns>True if and only if it is a power of two.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPowerOfTwo(this int number)
        {
            return number > 0 && (number & (number - 1)) == 0x0;
        }

        /// <summary>
        /// Find out whether the provided 64 bit integer is a perfect power of two.
        /// </summary>
        /// <param name="number">The number to very whether it's a power of two.</param>
        /// <returns>True if and only if it is a power of two.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPowerOfTwo(this long number)
        {
            return number > 0 && (number & (number - 1)) == 0x0;
        }

        /// <summary>
        /// Find out whether the provided 32 bit integer is a perfect square, i.e. a square of an integer.
        /// </summary>
        /// <param name="number">The number to very whether it's a perfect square.</param>
        /// <returns>True if and only if it is a perfect square.</returns>
        public static bool IsPerfectSquare(this int number)
        {
            if (number < 0)
            {
                return false;
            }

            int lastHexDigit = number & 0xF;
            if (lastHexDigit > 9)
            {
                return false; // return immediately in 6 cases out of 16.
            }

            if (lastHexDigit == 0 || lastHexDigit == 1 || lastHexDigit == 4 || lastHexDigit == 9)
            {
                int t = (int)Math.Floor(Math.Sqrt(number) + 0.5);
                return (t * t) == number;
            }

            return false;
        }

        /// <summary>
        /// Find out whether the provided 64 bit integer is a perfect square, i.e. a square of an integer.
        /// </summary>
        /// <param name="number">The number to very whether it's a perfect square.</param>
        /// <returns>True if and only if it is a perfect square.</returns>
        public static bool IsPerfectSquare(this long number)
        {
            if (number < 0)
            {
                return false;
            }

            int lastHexDigit = (int)(number & 0xF);
            if (lastHexDigit > 9)
            {
                return false; // return immediately in 6 cases out of 16.
            }

            if (lastHexDigit == 0 || lastHexDigit == 1 || lastHexDigit == 4 || lastHexDigit == 9)
            {
                long t = (long)Math.Floor(Math.Sqrt(number) + 0.5);
                return (t * t) == number;
            }

            return false;
        }

        /// <summary>
        /// Raises 2 to the provided integer exponent (0 &lt;= exponent &lt; 31).
        /// </summary>
        /// <param name="exponent">The exponent to raise 2 up to.</param>
        /// <returns>2 ^ exponent.</returns>
        /// <exception cref="ArgumentOutOfRangeException"/>
        public static int PowerOfTwo(this int exponent)
        {
            if (exponent < 0 || exponent >= 31)
            {
                throw new ArgumentOutOfRangeException(nameof(exponent));
            }

            return 1 << exponent;
        }

        /// <summary>
        /// Raises 2 to the provided integer exponent (0 &lt;= exponent &lt; 63).
        /// </summary>
        /// <param name="exponent">The exponent to raise 2 up to.</param>
        /// <returns>2 ^ exponent.</returns>
        /// <exception cref="ArgumentOutOfRangeException"/>
        public static long PowerOfTwo(this long exponent)
        {
            if (exponent < 0 || exponent >= 63)
            {
                throw new ArgumentOutOfRangeException(nameof(exponent));
            }

            return ((long)1) << (int)exponent;
        }

        /// <summary>
        /// Evaluate the binary logarithm of an integer number.
        /// </summary>
        /// <remarks>Two-step method using a De Bruijn-like sequence table lookup.</remarks>
        public static int Log2(this int number)
        {
            number |= number >> 1;
            number |= number >> 2;
            number |= number >> 4;
            number |= number >> 8;
            number |= number >> 16;

            return MultiplyDeBruijnBitPosition[(uint)(number * 0x07C4ACDDU) >> 27];
        }

        static readonly int[] MultiplyDeBruijnBitPosition = {
            0, 9, 1, 10, 13, 21, 2, 29, 11, 14, 16, 18, 22, 25, 3, 30,
            8, 12, 20, 28, 15, 17, 24, 7, 19, 27, 23, 6, 26, 5, 4, 31
        };

        /// <summary>
        /// Find the closest perfect power of two that is larger or equal to the provided
        /// 32 bit integer.
        /// </summary>
        /// <param name="number">The number of which to find the closest upper power of two.</param>
        /// <returns>A power of two.</returns>
        /// <exception cref="ArgumentOutOfRangeException"/>
        public static int CeilingToPowerOfTwo(this int number)
        {
            if (number == int.MinValue)
            {
                return 0;
            }

            const int maxPowerOfTwo = 0x40000000;
            if (number > maxPowerOfTwo)
            {
                throw new ArgumentOutOfRangeException(nameof(number));
            }

            number--;
            number |= number >> 1;
            number |= number >> 2;
            number |= number >> 4;
            number |= number >> 8;
            number |= number >> 16;
            return number + 1;
        }

        /// <summary>
        /// Find the closest perfect power of two that is larger or equal to the provided
        /// 64 bit integer.
        /// </summary>
        /// <param name="number">The number of which to find the closest upper power of two.</param>
        /// <returns>A power of two.</returns>
        /// <exception cref="ArgumentOutOfRangeException"/>
        public static long CeilingToPowerOfTwo(this long number)
        {
            if (number == long.MinValue)
            {
                return 0;
            }

            const long maxPowerOfTwo = 0x4000000000000000;
            if (number > maxPowerOfTwo)
            {
                throw new ArgumentOutOfRangeException(nameof(number));
            }

            number--;
            number |= number >> 1;
            number |= number >> 2;
            number |= number >> 4;
            number |= number >> 8;
            number |= number >> 16;
            number |= number >> 32;
            return number + 1;
        }

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
                var remainder = a%b;
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
                throw new ArgumentNullException(nameof(integers));
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
        public static long ExtendedGreatestCommonDivisor(long a, long b, out long x, out long y)
        {
            long mp = 1, np = 0, m = 0, n = 1;

            while (b != 0)
            {
                long quot = Math.DivRem(a, b, out long rem);
                a = b;
                b = rem;

                var tmp = m;
                m = mp - (quot*m);
                mp = tmp;

                tmp = n;
                n = np - (quot*n);
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

            return Math.Abs((a/GreatestCommonDivisor(a, b))*b);
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
                throw new ArgumentNullException(nameof(integers));
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
                throw new ArgumentNullException(nameof(integers));
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
        public static BigInteger ExtendedGreatestCommonDivisor(BigInteger a, BigInteger b, out BigInteger x, out BigInteger y)
        {
            BigInteger mp = BigInteger.One, np = BigInteger.Zero, m = BigInteger.Zero, n = BigInteger.One;

            while (!b.IsZero)
            {
                BigInteger quot = BigInteger.DivRem(a, b, out BigInteger rem);
                a = b;
                b = rem;

                BigInteger tmp = m;
                m = mp - (quot*m);
                mp = tmp;

                tmp = n;
                n = np - (quot*n);
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

            return BigInteger.Abs((a/BigInteger.GreatestCommonDivisor(a, b))*b);
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
                throw new ArgumentNullException(nameof(integers));
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
