// <copyright file="SystemRandomExtensions.cs" company="Math.NET">
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

namespace MathNet.Numerics.Random
{
    using System;

    /// <summary>
    /// This class implements extension methods for the System.Random class. The extension methods generate
    /// pseudo-random distributed numbers for types other than double and int32.
    /// </summary>
    public static class SystemRandomExtensions
    {
        /// <summary>
        /// Returns a nonnegative random number less than <see cref="Int64.MaxValue"/>.
        /// </summary>
        /// <param name="rnd">
        /// The random object to extend.
        /// </param>
        /// <returns>
        /// A 64-bit signed integer greater than or equal to 0, and less than <see cref="Int64.MaxValue"/>; that is, 
        /// the range of return values includes 0 but not <see cref="Int64.MaxValue"/>.
        /// </returns>
        /// <seealso cref="NextFullRangeInt64"/>
        public static long NextInt64(this Random rnd)
        {
            var buffer = new byte[sizeof(long)];

            rnd.NextBytes(buffer);
            var candidate = BitConverter.ToInt64(buffer, 0);

            // wrap negative numbers around, mapping every negative number to a distinct nonnegative number
            // MinValue -> 0, -1 -> MaxValue
            candidate &= Int64.MaxValue;

            // skip candidate if it is MaxValue. Recursive since rare.
            return (candidate == Int64.MaxValue) ? rnd.NextInt64() : candidate;
        }

        /// <summary>
        /// Returns a random number of the full Int32 range.
        /// </summary>
        /// <param name="rnd">
        /// The random object to extend.
        /// </param>
        /// <returns>
        /// A 32-bit signed integer of the full range, including 0, negative numbers,
        /// <see cref="Int32.MaxValue"/> and <see cref="Int32.MinValue"/>.
        /// </returns>
        /// <seealso cref="System.Random.Next()"/>
        public static int NextFullRangeInt32(this Random rnd)
        {
            var buffer = new byte[sizeof(int)];
            rnd.NextBytes(buffer);
            return BitConverter.ToInt32(buffer, 0);
        }

        /// <summary>
        /// Returns a random number of the full Int64 range.
        /// </summary>
        /// <param name="rnd">
        /// The random object to extend.
        /// </param>
        /// <returns>
        /// A 64-bit signed integer of the full range, including 0, negative numbers,
        /// <see cref="Int64.MaxValue"/> and <see cref="Int64.MinValue"/>.
        /// </returns>
        /// <seealso cref="NextInt64"/>
        public static long NextFullRangeInt64(this Random rnd)
        {
            var buffer = new byte[sizeof(long)];
            rnd.NextBytes(buffer);
            return BitConverter.ToInt64(buffer, 0);
        }

        /// <summary>
        /// Returns a nonnegative decimal floating point random number less than 1.0.
        /// </summary>
        /// <param name="rnd">
        /// The random object to extend.
        /// </param>
        /// <returns>
        /// A decimal floating point number greater than or equal to 0.0, and less than 1.0; that is, 
        /// the range of return values includes 0.0 but not 1.0.
        /// </returns>
        public static decimal NextDecimal(this Random rnd)
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
    }
}
