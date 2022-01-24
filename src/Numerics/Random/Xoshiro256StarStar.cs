// <copyright file="Xoshiro256StarStar.cs" company="Math.NET">
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

/*
   Original code's copyright and license:
   Written in 2018 by David Blackman and Sebastiano Vigna (vigna@acm.org)

   To the extent possible under law, the author has dedicated all copyright
   and related and neighboring rights to this software to the public domain
   worldwide. This software is distributed without any warranty.

   See <http://creativecommons.org/publicdomain/zero/1.0/>.
*/

using System.Collections.Generic;
using System.Runtime.Serialization;
using System;
using System.Runtime;

namespace MathNet.Numerics.Random
{
    /// <summary>
    /// Xoshiro256** pseudo random number generator.
    /// A random number generator based on the <see cref="System.Random"/> class in the .NET library.
    /// </summary>
    /// <remarks>
    /// This is xoshiro256** 1.0, our all-purpose, rock-solid generator. It has
    /// excellent(sub-ns) speed, a state space(256 bits) that is large enough
    /// for any parallel application, and it passes all tests we are aware of.
    ///
    /// For generating just floating-point numbers, xoshiro256+ is even faster.
    ///
    /// The state must be seeded so that it is not everywhere zero.If you have
    /// a 64-bit seed, we suggest to seed a splitmix64 generator and use its
    /// output to fill s.
    ///
    /// For further details see:
    /// David Blackman &amp; Sebastiano Vigna (2018), "Scrambled Linear Pseudorandom Number Generators".
    /// https://arxiv.org/abs/1805.01407
    /// </remarks>
    [Serializable]
    [DataContract(Namespace = "urn:MathNet/Numerics/Random")]
    public class Xoshiro256StarStar : RandomSource
    {
        // Constants.
        const double REAL_UNIT_UINT = 1.0 / (1UL << 53);

        // RNG state.
        [DataMember(Order = 1)]
        ulong _s0;
        [DataMember(Order = 2)]
        ulong _s1;
        [DataMember(Order = 3)]
        ulong _s2;
        [DataMember(Order = 4)]
        ulong _s3;

        /// <summary>
        /// Construct a new random number generator with a random seed.
        /// </summary>
        public Xoshiro256StarStar() : this(RandomSeed.Robust())
        {
        }

        /// <summary>
        /// Construct a new random number generator with random seed.
        /// </summary>
        /// <param name="threadSafe">if set to <c>true</c> , the class is thread safe.</param>
        public Xoshiro256StarStar(bool threadSafe) : this(RandomSeed.Robust(), threadSafe)
        {
        }

        /// <summary>
        /// Construct a new random number generator with random seed.
        /// </summary>
        /// <param name="seed">The seed value.</param>
        public Xoshiro256StarStar(int seed)
        {
            Initialise(seed);
        }

        /// <summary>
        /// Construct a new random number generator with random seed.
        /// </summary>
        /// <param name="seed">The seed value.</param>
        /// <param name="threadSafe">if set to <c>true</c> , the class is thread safe.</param>
        public Xoshiro256StarStar(int seed, bool threadSafe) : base(threadSafe)
        {
            Initialise(seed);
        }

        /// <summary>
        /// Returns a random double-precision floating point number greater than or equal to 0.0, and less than 1.0.
        /// </summary>
        protected sealed override double DoSample()
        {
            // Note. Here we generate a random integer between 0 and 2^53-1 (i.e. 53 binary 1s) and multiply
            // by the fractional unit value 1.0 / 2^53, thus the result has a max value of
            // 1.0 - (1.0 / 2^53), or 0.99999999999999989 in decimal.
            return (NextInnerULong() >> 11) * REAL_UNIT_UINT;
        }

        /// <summary>
        /// Returns a random 32-bit signed integer greater than or equal to zero and less than <see cref="F:System.Int32.MaxValue"/>
        /// </summary>
        protected override int DoSampleInteger()
        {
            retry:
            // Handle the special case where the value int.MaxValue is generated; this is outside
            // the range of permitted return values for this method.
            ulong rtn = NextInnerULong() & 0x7fff_ffffUL;
            if (rtn == 0x7fff_ffffUL)
            {
                goto retry;
            }
            return (int)rtn;
        }

        /// <summary>
        /// Fills the elements of a specified array of bytes with random numbers in full range, including zero and 255 (<see cref="F:System.Byte.MaxValue"/>).
        /// </summary>
        protected override void DoSampleBytes(byte[] buffer)
        {
            // For improved performance the below loop operates on these stack allocated copies of the heap variables.
            // Notes. doing this means that these heavily used variables are located near to other local/stack variables,
            // thus they will very likely be cached in the same CPU cache line.
            ulong s0 = _s0;
            ulong s1 = _s1;
            ulong s2 = _s2;
            ulong s3 = _s3;

            int i = 0;

            // Fill up the bulk of the buffer in chunks of 8 bytes at a time.
            int bound = buffer.Length - (buffer.Length % 8);
            while (i < bound)
            {
                // Generate 64 random bits.
                ulong x = RotateLeft(s1 * 5, 7) * 9;

                // Update PRNG state.
                ulong t = s1 << 17;
                s2 ^= s0;
                s3 ^= s1;
                s1 ^= s2;
                s0 ^= s3;
                s2 ^= t;
                s3 = RotateLeft(s3, 45);

                // Assign bits to a segment of 8 bytes.
                buffer[i++] = (byte)x;
                buffer[i++] = (byte)(x >> 8);
                buffer[i++] = (byte)(x >> 16);
                buffer[i++] = (byte)(x >> 24);
                buffer[i++] = (byte)(x >> 32);
                buffer[i++] = (byte)(x >> 40);
                buffer[i++] = (byte)(x >> 48);
                buffer[i++] = (byte)(x >> 56);
            }

            // Fill up any remaining bytes in the buffer.
            if (i < buffer.Length)
            {
                // Generate 64 random bits.
                ulong x = RotateLeft(s1 * 5, 7) * 9;

                // Update PRNG state.
                ulong t = s1 << 17;
                s2 ^= s0;
                s3 ^= s1;
                s1 ^= s2;
                s0 ^= s3;
                s2 ^= t;
                s3 = RotateLeft(s3, 45);

                // Allocate one byte at a time until we reach the end of the buffer.
                while (i < buffer.Length)
                {
                    buffer[i++] = (byte)x;
                    x >>= 8;
                }
            }

            // Update the state variables on the heap.
            _s0 = s0;
            _s1 = s1;
            _s2 = s2;
            _s3 = s3;
        }

        /// <summary>
        /// Returns a random N-bit signed integer greater than or equal to zero and less than 2^N.
        /// N (bit count) is expected to be greater than zero and less than 32 (not verified).
        /// </summary>
        protected override int DoSampleInt32WithNBits(int bitCount)
        {
            return (int)(NextInnerULong() >> (64 - bitCount));
        }

        /// <summary>
        /// Returns a random N-bit signed long integer greater than or equal to zero and less than 2^N.
        /// N (bit count) is expected to be greater than zero and less than 64 (not verified).
        /// </summary>
        protected override long DoSampleInt64WithNBits(int bitCount)
        {
            return (long)(NextInnerULong() >> (64 - bitCount));
        }

        void Initialise(int seed)
        {
            // Notes.
            // xoroshiro256** requires that at least one of the state variable be non-zero, use of splitmix64
            // satisfies that requirement because its outputs are equidistributed, i.e. if a zero is output
            // then the next zero will be after a further 2^64 outputs.
            // Splitmix will also accept a zero input, thus all possible seeds can be accepted here and will
            // all generate good initial state for xoshiro256**.
            ulong longSeed = (ulong)seed;

            // Use the splitmix64 RNG to hash the seed.
            _s0 = Splitmix64(ref longSeed);
            _s1 = Splitmix64(ref longSeed);
            _s2 = Splitmix64(ref longSeed);
            _s3 = Splitmix64(ref longSeed);
        }

        ulong NextInnerULong()
        {
            ulong s0 = _s0;
            ulong s1 = _s1;
            ulong s2 = _s2;
            ulong s3 = _s3;

            ulong result = RotateLeft(s1 * 5, 7) * 9;

            ulong t = s1 << 17;
            s2 ^= s0;
            s3 ^= s1;
            s1 ^= s2;
            s0 ^= s3;
            s2 ^= t;
            s3 = RotateLeft(s3, 45);

            _s0 = s0;
            _s1 = s1;
            _s2 = s2;
            _s3 = s3;

            return result;
        }

        /// <summary>
        /// Fills an array with random numbers greater than or equal to 0.0 and less than 1.0.
        /// </summary>
        /// <remarks>Supports being called in parallel from multiple threads.</remarks>
        public static void Doubles(double[] values, int seed)
        {
            // Init state.
            ulong longSeed = (ulong)seed;
            ulong s0 = Splitmix64(ref longSeed);
            ulong s1 = Splitmix64(ref longSeed);
            ulong s2 = Splitmix64(ref longSeed);
            ulong s3 = Splitmix64(ref longSeed);

            for (int i = 0; i < values.Length; i++)
            {
                // Generate sample.
                values[i] = ((RotateLeft(s1 * 5, 7) * 9) >> 11) * REAL_UNIT_UINT;

                // Update PRNG state.
                ulong t = s1 << 17;
                s2 ^= s0;
                s3 ^= s1;
                s1 ^= s2;
                s0 ^= s3;
                s2 ^= t;
                s3 = RotateLeft(s3, 45);
            }
        }

        /// <summary>
        /// Returns an array of random numbers greater than or equal to 0.0 and less than 1.0.
        /// </summary>
        /// <remarks>Supports being called in parallel from multiple threads.</remarks>
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static double[] Doubles(int length, int seed)
        {
            var data = new double[length];
            Doubles(data, seed);
            return data;
        }

        /// <summary>
        /// Returns an infinite sequence of random numbers greater than or equal to 0.0 and less than 1.0.
        /// </summary>
        /// <remarks>Supports being called in parallel from multiple threads, but the result must be enumerated from a single thread each.</remarks>
        public static IEnumerable<double> DoubleSequence(int seed)
        {
            // Init state.
            ulong longSeed = (ulong)seed;
            ulong s0 = Splitmix64(ref longSeed);
            ulong s1 = Splitmix64(ref longSeed);
            ulong s2 = Splitmix64(ref longSeed);
            ulong s3 = Splitmix64(ref longSeed);

            while (true)
            {
                // Generate sample.
                double x = ((RotateLeft(s1 * 5, 7) * 9) >> 11) * REAL_UNIT_UINT;

                // Update PRNG state.
                ulong t = s1 << 17;
                s2 ^= s0;
                s3 ^= s1;
                s1 ^= s2;
                s0 ^= s3;
                s2 ^= t;
                s3 = RotateLeft(s3, 45);

                // Yield sample.
                yield return x;
            }
        }

        /// <summary>
        /// Splitmix64 RNG.
        /// </summary>
        /// <param name="x">RNG state. This can take any value, including zero.</param>
        /// <returns>A new random UInt64.</returns>
        /// <remarks>
        /// Splitmix64 produces equidistributed outputs, thus if a zero is generated then the
        /// next zero will be after a further 2^64 outputs.
        /// </remarks>
        static ulong Splitmix64(ref ulong x)
        {
            ulong z = (x += 0x9E3779B97F4A7C15UL);
            z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
            z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
            return z ^ (z >> 31);
        }

        static ulong RotateLeft(ulong x, int k)
        {
            // Note. RyuJIT will compile this to a single rotate CPU instruction (as of about .NET 4.6.1 and dotnet core 2.0).
            return (x << k) | (x >> (64 - k));
        }
    }
}
