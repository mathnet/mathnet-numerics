// <copyright file="SystemRandomExtensions.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
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
using System.Collections.Generic;
using System.Numerics;
using System.Linq;

namespace MathNet.Numerics.Random
{
    /// <summary>
    /// This class implements extension methods for the System.Random class. The extension methods generate
    /// pseudo-random distributed numbers for types other than double and int32.
    /// </summary>
    public static class RandomExtensions
    {
        /// <summary>
        /// Fills an array with uniform random numbers greater than or equal to 0.0 and less than 1.0.
        /// </summary>
        /// <param name="rnd">The random number generator.</param>
        /// <param name="values">The array to fill with random values.</param>
        /// <remarks>
        /// This extension is thread-safe if and only if called on an random number
        /// generator provided by Math.NET Numerics or derived from the RandomSource class.
        /// </remarks>
        public static void NextDoubles(this System.Random rnd, double[] values)
        {
            if (rnd is RandomSource rs)
            {
                rs.NextDoubles(values);
                return;
            }

            for (var i = 0; i < values.Length; i++)
            {
                values[i] = rnd.NextDouble();
            }
        }

        /// <summary>
        /// Returns an array of uniform random numbers greater than or equal to 0.0 and less than 1.0.
        /// </summary>
        /// <param name="rnd">The random number generator.</param>
        /// <param name="count">The size of the array to fill.</param>
        /// <remarks>
        /// This extension is thread-safe if and only if called on an random number
        /// generator provided by Math.NET Numerics or derived from the RandomSource class.
        /// </remarks>
        public static double[] NextDoubles(this System.Random rnd, int count)
        {
            var values = new double[count];
            NextDoubles(rnd, values);
            return values;
        }

        /// <summary>
        /// Returns an infinite sequence of uniform random numbers greater than or equal to 0.0 and less than 1.0.
        /// </summary>
        /// <remarks>
        /// This extension is thread-safe if and only if called on an random number
        /// generator provided by Math.NET Numerics or derived from the RandomSource class.
        /// </remarks>
        public static IEnumerable<double> NextDoubleSequence(this System.Random rnd)
        {
            if (rnd is RandomSource rs)
            {
                return rs.NextDoubleSequence();
            }

            return NextDoubleSequenceEnumerable(rnd);
        }

        static IEnumerable<double> NextDoubleSequenceEnumerable(System.Random rnd)
        {
            while (true)
            {
                yield return rnd.NextDouble();
            }
        }

        /// <summary>
        /// Returns an array of uniform random bytes.
        /// </summary>
        /// <param name="rnd">The random number generator.</param>
        /// <param name="count">The size of the array to fill.</param>
        /// <remarks>
        /// This extension is thread-safe if and only if called on an random number
        /// generator provided by Math.NET Numerics or derived from the RandomSource class.
        /// </remarks>
        public static byte[] NextBytes(this System.Random rnd, int count)
        {
            var values = new byte[count];
            rnd.NextBytes(values);
            return values;
        }

        /// <summary>
        /// Fills an array with uniform random 32-bit signed integers greater than or equal to zero and less than <see cref="F:System.Int32.MaxValue"/>.
        /// </summary>
        /// <param name="rnd">The random number generator.</param>
        /// <param name="values">The array to fill with random values.</param>
        /// <remarks>
        /// This extension is thread-safe if and only if called on an random number
        /// generator provided by Math.NET Numerics or derived from the RandomSource class.
        /// </remarks>
        public static void NextInt32s(this System.Random rnd, int[] values)
        {
            if (rnd is RandomSource rs)
            {
                rs.NextInt32s(values);
                return;
            }

            for (var i = 0; i < values.Length; i++)
            {
                values[i] = rnd.Next();
            }
        }

        /// <summary>
        /// Fills an array with uniform random 32-bit signed integers within the specified range.
        /// </summary>
        /// <param name="rnd">The random number generator.</param>
        /// <param name="values">The array to fill with random values.</param>
        /// <param name="minInclusive">Lower bound, inclusive.</param>
        /// <param name="maxExclusive">Upper bound, exclusive.</param>
        /// <remarks>
        /// This extension is thread-safe if and only if called on an random number
        /// generator provided by Math.NET Numerics or derived from the RandomSource class.
        /// </remarks>
        public static void NextInt32s(this System.Random rnd, int[] values, int minInclusive, int maxExclusive)
        {
            if (rnd is RandomSource rs)
            {
                rs.NextInt32s(values, minInclusive, maxExclusive);
                return;
            }

            for (var i = 0; i < values.Length; i++)
            {
                values[i] = rnd.Next(minInclusive, maxExclusive);
            }
        }

        /// <summary>
        /// Returns an infinite sequence of uniform random 32-bit signed integers within the specified range.
        /// </summary>
        /// <remarks>
        /// This extension is thread-safe if and only if called on an random number
        /// generator provided by Math.NET Numerics or derived from the RandomSource class.
        /// </remarks>
        public static IEnumerable<int> NextInt32Sequence(this System.Random rnd, int minInclusive, int maxExclusive)
        {
            if (rnd is RandomSource rs)
            {
                return rs.NextInt32Sequence(minInclusive, maxExclusive);
            }

            return NextInt32SequenceEnumerable(rnd, minInclusive, maxExclusive);
        }

        static IEnumerable<int> NextInt32SequenceEnumerable(System.Random rnd, int minInclusive, int maxExclusive)
        {
            while (true)
            {
                yield return rnd.Next(minInclusive, maxExclusive);
            }
        }

        /// <summary>
        /// Returns an infinite sequence of uniform random <see cref="System.Numerics.BigInteger"/> within the specified range.
        /// </summary>
        /// <remarks>
        /// This extension is thread-safe if and only if called on an random number
        /// generator provided by Math.NET Numerics or derived from the RandomSource class.
        /// </remarks>
        public static IEnumerable<BigInteger> NextBigIntegerSequence(this System.Random rnd, BigInteger minInclusive, BigInteger maxExclusive)
        {
            BigInteger absoluteRange = maxExclusive - minInclusive;
            int numBytes = (int)Math.Ceiling(BigInteger.Log(absoluteRange, byte.MaxValue) * 2) + 1;
            byte[] byteSequence = Generate.Repeat(numBytes + 1, byte.MaxValue);
            byteSequence[numBytes] = 0;
            BigInteger randomNumber = new BigInteger(byteSequence);
            BigInteger validRange = randomNumber - randomNumber % absoluteRange;

            while(true)
            {
                do
                {
                    rnd.NextBytes(byteSequence);
                    byteSequence[numBytes] = 0;
                    randomNumber = new BigInteger(byteSequence);
                }
                while (randomNumber >= validRange);
                yield return randomNumber % absoluteRange + minInclusive;
            }
        }

        /// <summary>
        /// Returns a nonnegative random number less than <see cref="Int64.MaxValue"/>.
        /// </summary>
        /// <param name="rnd">The random number generator.</param>
        /// <returns>
        /// A 64-bit signed integer greater than or equal to 0, and less than <see cref="Int64.MaxValue"/>; that is,
        /// the range of return values includes 0 but not <see cref="Int64.MaxValue"/>.
        /// </returns>
        /// <seealso cref="NextFullRangeInt64"/>
        /// <remarks>
        /// This extension is thread-safe if and only if called on an random number
        /// generator provided by Math.NET Numerics or derived from the RandomSource class.
        /// </remarks>
        public static long NextInt64(this System.Random rnd)
        {
            var buffer = new byte[8];

            rnd.NextBytes(buffer);
            var candidate = BitConverter.ToInt64(buffer, 0);

            // wrap negative numbers around, mapping every negative number to a distinct nonnegative number
            // MinValue -> 0, -1 -> MaxValue
            candidate &= long.MaxValue;

            // skip candidate if it is MaxValue. Recursive since rare.
            return (candidate == long.MaxValue) ? rnd.NextInt64() : candidate;
        }

        /// <summary>
        /// Returns a random number of the full Int32 range.
        /// </summary>
        /// <param name="rnd">The random number generator.</param>
        /// <returns>
        /// A 32-bit signed integer of the full range, including 0, negative numbers,
        /// <see cref="Int32.MaxValue"/> and <see cref="Int32.MinValue"/>.
        /// </returns>
        /// <seealso cref="System.Random.Next()"/>
        /// <remarks>
        /// This extension is thread-safe if and only if called on an random number
        /// generator provided by Math.NET Numerics or derived from the RandomSource class.
        /// </remarks>
        public static int NextFullRangeInt32(this System.Random rnd)
        {
            var buffer = new byte[4];
            rnd.NextBytes(buffer);
            return BitConverter.ToInt32(buffer, 0);
        }

        /// <summary>
        /// Returns a random number of the full Int64 range.
        /// </summary>
        /// <param name="rnd">The random number generator.</param>
        /// <returns>
        /// A 64-bit signed integer of the full range, including 0, negative numbers,
        /// <see cref="Int64.MaxValue"/> and <see cref="Int64.MinValue"/>.
        /// </returns>
        /// <seealso cref="NextInt64"/>
        /// <remarks>
        /// This extension is thread-safe if and only if called on an random number
        /// generator provided by Math.NET Numerics or derived from the RandomSource class.
        /// </remarks>
        public static long NextFullRangeInt64(this System.Random rnd)
        {
            var buffer = new byte[8];
            rnd.NextBytes(buffer);
            return BitConverter.ToInt64(buffer, 0);
        }

        /// <summary>
        /// Returns a nonnegative decimal floating point random number less than 1.0.
        /// </summary>
        /// <param name="rnd">The random number generator.</param>
        /// <returns>
        /// A decimal floating point number greater than or equal to 0.0, and less than 1.0; that is,
        /// the range of return values includes 0.0 but not 1.0.
        /// </returns>
        /// <remarks>
        /// This extension is thread-safe if and only if called on an random number
        /// generator provided by Math.NET Numerics or derived from the RandomSource class.
        /// </remarks>
        public static decimal NextDecimal(this System.Random rnd)
        {
            decimal candidate;

            // 50.049 % chance that the number is below 1.0. Try until we have one.
            // Guarantees that any decimal in the interval can
            // indeed be reached, with uniform probability.
            do
            {
                candidate = new decimal(
                    rnd.NextFullRangeInt32(),
                    rnd.NextFullRangeInt32(),
                    rnd.NextFullRangeInt32(),
                    false,
                    28);
            }
            while (candidate >= 1.0m);

            return candidate;
        }

        /// <summary>
        /// Returns a random boolean.
        /// </summary>
        /// <param name="rnd">The random number generator.</param>
        /// <remarks>
        /// This extension is thread-safe if and only if called on an random number
        /// generator provided by Math.NET Numerics or derived from the RandomSource class.
        /// </remarks>
        public static bool NextBoolean(this System.Random rnd)
        {
            return rnd.NextDouble() >= 0.5;
        }
    }
}
