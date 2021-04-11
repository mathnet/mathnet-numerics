// <copyright file="Xorshift.cs" company="Math.NET">
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
using System.Runtime.Serialization;
using System.Runtime;

namespace MathNet.Numerics.Random
{
    /// <summary>
    /// Implements a multiply-with-carry Xorshift pseudo random number generator (RNG) specified in Marsaglia, George. (2003). Xorshift RNGs.
    /// <code>Xn = a * Xn−3 + c mod 2^32</code>
    /// http://www.jstatsoft.org/v08/i14/paper
    /// </summary>
    [Serializable]
    [DataContract(Namespace = "urn:MathNet/Numerics/Random")]
    public class Xorshift : RandomSource
    {
        /// <summary>
        /// The default value for X1.
        /// </summary>
        const uint YSeed = 362436069;

        /// <summary>
        /// The default value for X2.
        /// </summary>
        const uint ZSeed = 77465321;

        /// <summary>
        /// The default value for the multiplier.
        /// </summary>
        const uint ASeed = 916905990;

        /// <summary>
        /// The default value for the carry over.
        /// </summary>
        const uint CSeed = 13579;

        /// <summary>
        /// The multiplier to compute a double-precision floating point number [0, 1)
        /// </summary>
        const double UlongToDoubleMultiplier = 1.0/4294967296.0; // 1.0/(uint.MaxValue + 1.0)

        /// <summary>
        /// Seed or last but three unsigned random number.
        /// </summary>
        [DataMember(Order = 1)]
        ulong _x;

        /// <summary>
        /// Last but two unsigned random number.
        /// </summary>
        [DataMember(Order = 2)]
        ulong _y;

        /// <summary>
        /// Last but one unsigned random number.
        /// </summary>
        [DataMember(Order = 3)]
        ulong _z;

        /// <summary>
        /// The value of the carry over.
        /// </summary>
        [DataMember(Order = 4)]
        ulong _c;

        /// <summary>
        /// The multiplier.
        /// </summary>
        [DataMember(Order = 5)]
        readonly ulong _a;

        /// <summary>
        /// Initializes a new instance of the <see cref="Xorshift"/> class using
        /// a seed based on time and unique GUIDs.
        /// </summary>
        /// <remarks>If the seed value is zero, it is set to one. Uses the
        /// value of <see cref="Control.ThreadSafeRandomNumberGenerators"/> to
        /// set whether the instance is thread safe.
        /// Uses the default values of:
        /// <list>
        /// <item>a = 916905990</item>
        /// <item>c = 13579</item>
        /// <item>X1 = 77465321</item>
        /// <item>X2 = 362436069</item>
        /// </list></remarks>
        public Xorshift() : this(RandomSeed.Robust())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Xorshift"/> class using
        /// a seed based on time and unique GUIDs.
        /// </summary>
        /// <param name="a">The multiply value</param>
        /// <param name="c">The initial carry value.</param>
        /// <param name="x1">The initial value if X1.</param>
        /// <param name="x2">The initial value if X2.</param>
        /// <remarks>If the seed value is zero, it is set to one. Uses the
        /// value of <see cref="Control.ThreadSafeRandomNumberGenerators"/> to
        /// set whether the instance is thread safe.
        /// Note: <paramref name="c"/> must be less than <paramref name="a"/>.
        /// </remarks>
        public Xorshift(long a, long c, long x1, long x2) : this(RandomSeed.Robust(), a, c, x1, x2)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Xorshift"/> class using
        /// a seed based on time and unique GUIDs.
        /// </summary>
        /// <param name="threadSafe">if set to <c>true</c> , the class is thread safe.</param>
        /// <remarks>
        /// Uses the default values of:
        /// <list>
        /// <item>a = 916905990</item>
        /// <item>c = 13579</item>
        /// <item>X1 = 77465321</item>
        /// <item>X2 = 362436069</item>
        /// </list></remarks>
        public Xorshift(bool threadSafe) : this(RandomSeed.Robust(), threadSafe)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Xorshift"/> class using
        /// a seed based on time and unique GUIDs.
        /// </summary>
        /// <param name="threadSafe">if set to <c>true</c> , the class is thread safe.</param>
        /// <param name="a">The multiply value</param>
        /// <param name="c">The initial carry value.</param>
        /// <param name="x1">The initial value if X1.</param>
        /// <param name="x2">The initial value if X2.</param>
        /// <remarks><paramref name="c"/> must be less than <paramref name="a"/>.</remarks>
        public Xorshift(bool threadSafe, long a, long c, long x1, long x2) : this(RandomSeed.Robust(), threadSafe, a, c, x1, x2)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Xorshift"/> class.
        /// </summary>
        /// <param name="seed">The seed value.</param>
        /// <remarks>If the seed value is zero, it is set to one. Uses the
        /// value of <see cref="Control.ThreadSafeRandomNumberGenerators"/> to
        /// set whether the instance is thread safe.
        /// Uses the default values of:
        /// <list>
        /// <item>a = 916905990</item>
        /// <item>c = 13579</item>
        /// <item>X1 = 77465321</item>
        /// <item>X2 = 362436069</item>
        /// </list></remarks>
        public Xorshift(int seed) : this(seed, Control.ThreadSafeRandomNumberGenerators)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Xorshift"/> class.
        /// </summary>
        /// <param name="seed">The seed value.</param>
        /// <remarks>If the seed value is zero, it is set to one. Uses the
        /// value of <see cref="Control.ThreadSafeRandomNumberGenerators"/> to
        /// set whether the instance is thread safe.</remarks>
        /// <param name="a">The multiply value</param>
        /// <param name="c">The initial carry value.</param>
        /// <param name="x1">The initial value if X1.</param>
        /// <param name="x2">The initial value if X2.</param>
        /// <remarks><paramref name="c"/> must be less than <paramref name="a"/>.</remarks>
        public Xorshift(int seed, long a, long c, long x1, long x2) : this(seed, Control.ThreadSafeRandomNumberGenerators, a, c, x1, x2)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Xorshift"/> class.
        /// </summary>
        /// <param name="seed">The seed value.</param>
        /// <param name="threadSafe">if set to <c>true</c>, the class is thread safe.</param>
        /// <remarks>
        /// Uses the default values of:
        /// <list>
        /// <item>a = 916905990</item>
        /// <item>c = 13579</item>
        /// <item>X1 = 77465321</item>
        /// <item>X2 = 362436069</item>
        /// </list></remarks>
        public Xorshift(int seed, bool threadSafe) : base(threadSafe)
        {
            if (seed == 0)
            {
                seed = 1;
            }

            _x = (uint)seed;
            _y = YSeed;
            _z = ZSeed;
            _c = CSeed;
            _a = ASeed;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Xorshift"/> class.
        /// </summary>
        /// <param name="seed">The seed value.</param>
        /// <param name="threadSafe">if set to <c>true</c>, the class is thread safe.</param>
        /// <param name="a">The multiply value</param>
        /// <param name="c">The initial carry value.</param>
        /// <param name="x1">The initial value if X1.</param>
        /// <param name="x2">The initial value if X2.</param>
        /// <remarks><paramref name="c"/> must be less than <paramref name="a"/>.</remarks>
        public Xorshift(int seed, bool threadSafe, long a, long c, long x1, long x2) : base(threadSafe)
        {
            if (seed == 0)
            {
                seed = 1;
            }

            if (a <= c)
            {
                throw new ArgumentException("a must be greater than c.", nameof(a));
            }

            _x = (uint)seed;
            _y = (ulong)x1;
            _z = (ulong)x2;
            _a = (ulong)a;
            _c = (ulong)c;
        }

        /// <summary>
        /// Returns a random double-precision floating point number greater than or equal to 0.0, and less than 1.0.
        /// </summary>
        protected sealed override double DoSample()
        {
            var t = (_a*_x) + _c;
            _x = _y;
            _y = _z;
            _c = t >> 32;
            _z = t & 0xffffffff;
            return _z*UlongToDoubleMultiplier;
        }

        /// <summary>
        /// Returns a random 32-bit signed integer greater than or equal to zero and less than <see cref="F:System.Int32.MaxValue"/>
        /// </summary>
        protected sealed override int DoSampleInteger()
        {
            var t = (_a * _x) + _c;
            _x = _y;
            _y = _z;
            _c = t >> 32;
            _z = t & 0xffffffff;
            uint uint32 = (uint)_z;
            int int31 = (int)(uint32 >> 1);
            if (int31 == int.MaxValue)
            {
                return DoSampleInteger();
            }

            return int31;
        }

        /// <summary>
        /// Fills the elements of a specified array of bytes with random numbers in full range, including zero and 255 (<see cref="F:System.Byte.MaxValue"/>).
        /// </summary>
        protected sealed override void DoSampleBytes(byte[] buffer)
        {
            for (var i = 0; i < buffer.Length; i++)
            {
                var t = (_a * _x) + _c;
                _x = _y;
                _y = _z;
                _c = t >> 32;
                _z = t & 0xffffffff;
                buffer[i] = (byte)(_z % 256);
            }
        }

        /// <summary>
        /// Fills an array with random numbers greater than or equal to 0.0 and less than 1.0.
        /// </summary>
        /// <remarks>Supports being called in parallel from multiple threads.</remarks>
        [CLSCompliant(false)]
        public static void Doubles(double[] values, int seed, ulong a = ASeed, ulong c = CSeed, ulong x1 = YSeed, ulong x2 = ZSeed)
        {
            if (a <= c)
            {
                throw new ArgumentException("a must be greater than c.", nameof(a));
            }

            if (seed == 0)
            {
                seed = 1;
            }

            ulong x = (uint)seed;

            for (int i = 0; i < values.Length; i++)
            {
                var t = (a*x) + c;
                x = x1;
                x1 = x2;
                c = t >> 32;
                x2 = t & 0xffffffff;
                values[i] = x2*UlongToDoubleMultiplier;
            }
        }

        /// <summary>
        /// Returns an array of random numbers greater than or equal to 0.0 and less than 1.0.
        /// </summary>
        /// <remarks>Supports being called in parallel from multiple threads.</remarks>
        [CLSCompliant(false)]
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static double[] Doubles(int length, int seed, ulong a = ASeed, ulong c = CSeed, ulong x1 = YSeed, ulong x2 = ZSeed)
        {
            var data = new double[length];
            Doubles(data, seed, a, c, x1, x2);
            return data;
        }

        /// <summary>
        /// Returns an infinite sequence of random numbers greater than or equal to 0.0 and less than 1.0.
        /// </summary>
        /// <remarks>Supports being called in parallel from multiple threads, but the result must be enumerated from a single thread each.</remarks>
        [CLSCompliant(false)]
        public static IEnumerable<double> DoubleSequence(int seed, ulong a = ASeed, ulong c = CSeed, ulong x1 = YSeed, ulong x2 = ZSeed)
        {
            if (a <= c)
            {
                throw new ArgumentException("a must be greater than c.", nameof(a));
            }

            if (seed == 0)
            {
                seed = 1;
            }

            ulong x = (uint)seed;

            while (true)
            {
                var t = (a*x) + c;
                x = x1;
                x1 = x2;
                c = t >> 32;
                x2 = t & 0xffffffff;
                yield return x2*UlongToDoubleMultiplier;
            }
        }
    }
}
