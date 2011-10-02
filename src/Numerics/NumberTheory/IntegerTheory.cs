// <copyright file="IntegerTheory.cs" company="Math.NET">
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

namespace MathNet.Numerics.NumberTheory
{
    using System;

    /// <summary>
    /// Number theory utility functions for integers.
    /// </summary>
    public static partial class IntegerTheory
    {
        /// <summary>
        /// Find out whether the provided 32 bit integer is an even number.
        /// </summary>
        /// <param name="number">The number to very whether it's even.</param>
        /// <returns>True if and only if it is an even number.</returns>
        public static bool IsEven(this int number)
        {
            return (number & 0x1) == 0x0;
        }

        /// <summary>
        /// Find out whether the provided 64 bit integer is an even number.
        /// </summary>
        /// <param name="number">The number to very whether it's even.</param>
        /// <returns>True if and only if it is an even number.</returns>
        public static bool IsEven(this long number)
        {
            return (number & 0x1) == 0x0;
        }

        /// <summary>
        /// Find out whether the provided 32 bit integer is an odd number.
        /// </summary>
        /// <param name="number">The number to very whether it's odd.</param>
        /// <returns>True if and only if it is an odd number.</returns>
        public static bool IsOdd(this int number)
        {
            return (number & 0x1) == 0x1;
        }

        /// <summary>
        /// Find out whether the provided 64 bit integer is an odd number.
        /// </summary>
        /// <param name="number">The number to very whether it's odd.</param>
        /// <returns>True if and only if it is an odd number.</returns>
        public static bool IsOdd(this long number)
        {
            return (number & 0x1) == 0x1;
        }

        /// <summary>
        /// Find out whether the provided 32 bit integer is a perfect power of two.
        /// </summary>
        /// <param name="number">The number to very whether it's a power of two.</param>
        /// <returns>True if and only if it is a power of two.</returns>
        public static bool IsPowerOfTwo(this int number)
        {
            return number > 0 && (number & (number - 1)) == 0x0;
        }

        /// <summary>
        /// Find out whether the provided 64 bit integer is a perfect power of two.
        /// </summary>
        /// <param name="number">The number to very whether it's a power of two.</param>
        /// <returns>True if and only if it is a power of two.</returns>
        public static bool IsPowerOfTwo(this long number)
        {
            return number > 0 && (number & (number - 1)) == 0x0;
        }

        /// <summary>
        /// Find the closest perfect power of two that is larger or equal to the provided
        /// 32 bit integer.
        /// </summary>
        /// <param name="number">The number of which to find the closest upper power of two.</param>
        /// <returns>A power of two.</returns>
        /// <exception cref="ArgumentOutOfRangeException"/>
        public static int CeilingToPowerOfTwo(this int number)
        {
            if (number == Int32.MinValue)
            {
                return 0;
            }

            const int MaxPowerOfTwo = 0x40000000;
            if (number > MaxPowerOfTwo)
            {
                throw new ArgumentOutOfRangeException("number");
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
            if (number == Int64.MinValue)
            {
                return 0;
            }

            const long MaxPowerOfTwo = 0x4000000000000000;
            if (number > MaxPowerOfTwo)
            {
                throw new ArgumentOutOfRangeException("number");
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
        /// Raises 2 to the provided integer exponent (0 &lt;= exponent &lt; 31).
        /// </summary>
        /// <param name="exponent">The exponent to raise 2 up to.</param>
        /// <returns>2 ^ exponent.</returns>
        /// <exception cref="ArgumentOutOfRangeException"/>
        public static int PowerOfTwo(this int exponent)
        {
            if (exponent < 0 || exponent >= 31)
            {
                throw new ArgumentOutOfRangeException("exponent");
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
                throw new ArgumentOutOfRangeException("exponent");
            }

            return ((long)1) << (int)exponent;
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
    }
}
