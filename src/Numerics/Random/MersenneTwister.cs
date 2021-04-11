// <copyright file="MersenneTwister.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2017 Math.NET
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
   Copyright (C) 1997 - 2002, Makoto Matsumoto and Takuji Nishimura,
   All rights reserved.

   Redistribution and use in source and binary forms, with or without
   modification, are permitted provided that the following conditions
   are met:

     1. Redistributions of source code must retain the above copyright
        notice, this list of conditions and the following disclaimer.

     2. Redistributions in binary form must reproduce the above copyright
        notice, this list of conditions and the following disclaimer in the
        documentation and/or other materials provided with the distribution.

     3. The names of its contributors may not be used to endorse or promote
        products derived from this software without specific prior written
        permission.

   THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
   "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
   LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
   A PARTICULAR PURPOSE ARE DISCLAIMED.  IN NO EVENT SHALL THE COPYRIGHT OWNER OR
   CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
   EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
   PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
   PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
   LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
   NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
   SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.


   Any feedback is very welcome.
   http://www.math.sci.hiroshima-u.ac.jp/~m-mat/MT/emt.html
   email: m-mat @ math.sci.hiroshima-u.ac.jp (remove space)
*/

using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using System;
using System.Runtime;

namespace MathNet.Numerics.Random
{
    /// <summary>
    /// Random number generator using Mersenne Twister 19937 algorithm.
    /// </summary>
    [Serializable]
    [DataContract(Namespace = "urn:MathNet/Numerics/Random")]
    public class MersenneTwister : RandomSource
    {
        /// <summary>
        /// Mersenne twister constant.
        /// </summary>
        const uint LowerMask = 0x7fffffff;

        /// <summary>
        /// Mersenne twister constant.
        /// </summary>
        const int M = 397;

        /// <summary>
        /// Mersenne twister constant.
        /// </summary>
        const uint MatrixA = 0x9908b0df;

        /// <summary>
        /// Mersenne twister constant.
        /// </summary>
        const int N = 624;

        /// <summary>
        /// Mersenne twister constant.
        /// </summary>
        const double Reciprocal = 1.0/4294967296.0; // 1.0/(uint.MaxValue + 1.0)

        /// <summary>
        /// Mersenne twister constant.
        /// </summary>
        const uint UpperMask = 0x80000000;

        /// <summary>
        /// Mersenne twister constant.
        /// </summary>
        static readonly uint[] Mag01 = { 0x0U, MatrixA };

        /// <summary>
        /// Mersenne twister constant.
        /// </summary>
        [DataMember(Order = 1)]
        readonly uint[] _mt = new uint[N];

        /// <summary>
        /// Mersenne twister constant.
        /// </summary>
        [DataMember(Order = 2)]
        int _mti = N + 1;

        /// <summary>
        /// Initializes a new instance of the <see cref="MersenneTwister"/> class using
        /// a seed based on time and unique GUIDs.
        /// </summary>
        /// <remarks>If the seed value is zero, it is set to one. Uses the
        /// value of <see cref="Control.ThreadSafeRandomNumberGenerators"/> to
        /// set whether the instance is thread safe.</remarks>
        public MersenneTwister() : this(RandomSeed.Robust())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MersenneTwister"/> class using
        /// a seed based on time and unique GUIDs.
        /// </summary>
        /// <param name="threadSafe">if set to <c>true</c> , the class is thread safe.</param>
        public MersenneTwister(bool threadSafe) : this(RandomSeed.Robust(), threadSafe)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MersenneTwister"/> class.
        /// </summary>
        /// <param name="seed">The seed value.</param>
        /// <remarks>Uses the value of <see cref="Control.ThreadSafeRandomNumberGenerators"/> to
        /// set whether the instance is thread safe.</remarks>
        public MersenneTwister(int seed)
        {
            init_genrand((uint)seed);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MersenneTwister"/> class.
        /// </summary>
        /// <param name="seed">The seed value.</param>
        /// <param name="threadSafe">if set to <c>true</c>, the class is thread safe.</param>
        public MersenneTwister(int seed, bool threadSafe) : base(threadSafe)
        {
            init_genrand((uint)seed);
        }

        static readonly ThreadLocal<MersenneTwister> DefaultInstance = new ThreadLocal<MersenneTwister>(() => new MersenneTwister(RandomSeed.Robust(), true));

        /// <summary>
        /// Default instance, thread-safe.
        /// </summary>
        public static MersenneTwister Default => DefaultInstance.Value;

        /*/// <summary>
        /// Initializes a new instance of the <see cref="MersenneTwister"/> class.
        /// </summary>
        /// <param name="init_key">The initialization key.</param>
        public MersenneTwister(int[] init_key)
        {
            if (init_key == null)
            {
                throw new ArgumentNullException("init_key");
            }
            uint[] array = new uint[init_key.Length];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = (uint) init_key[i];
            }
            init_by_array(array);
        }
        */
        /* initializes _mt[_n] with a seed */

        void init_genrand(uint s)
        {
            _mt[0] = s & 0xffffffff;
            for (_mti = 1; _mti < N; _mti++)
            {
                _mt[_mti] = 1812433253*(_mt[_mti - 1] ^ (_mt[_mti - 1] >> 30)) + (uint)_mti;
                /* See Knuth TAOCP Vol2. 3rd Ed. P.106 for multiplier. */
                /* In the previous versions, MSBs of the seed affect   */
                /* only MSBs of the array _mt[].                        */
                /* 2002/01/09 modified by Makoto Matsumoto             */
                _mt[_mti] &= 0xffffffff;
                /* for >32 bit machines */
            }
        }

        /* initialize by an array with array-length */
        /* init_key is the array for initializing keys */
        /* slight change for C++, 2004/2/26 */

        /* private void init_by_array(uint[] init_key)
        {
            uint key_length = (uint) init_key.Length;
            init_genrand(19650218);
            uint i = 1;
            uint j = 0;
            uint k = (_n > key_length ? _n : key_length);
            for (; k > 0; k--)
            {
                _mt[i] = (_mt[i] ^ ((_mt[i - 1] ^ (_mt[i - 1] >> 30))*1664525)) + init_key[j] + j; //non linear
                _mt[i] &= 0xffffffff; // for WORDSIZE > 32 machines
                i++;
                j++;
                if (i >= _n)
                {
                    _mt[0] = _mt[_n - 1];
                    i = 1;
                }
                if (j >= key_length) j = 0;
            }
            for (k = _n - 1; k > 0; k--)
            {
                _mt[i] = (_mt[i] ^ ((_mt[i - 1] ^ (_mt[i - 1] >> 30))*1566083941)) - i; // non linear
                _mt[i] &= 0xffffffff; // for WORDSIZE > 32 machines
                i++;
                if (i >= _n)
                {
                    _mt[0] = _mt[_n - 1];
                    i = 1;
                }
            }

            _mt[0] = 0x80000000; // MSB is 1; assuring non-zero initial array
        }*/

        /* generates a random number on [0,0xffffffff]-interval */

        uint genrand_int32()
        {
            uint y;

            /* mag01[x] = x * MATRIX_A  for x=0,1 */

            if (_mti >= N)
            {
                /* generate _n words at one time */
                int kk;

                if (_mti == N + 1) /* if init_genrand() has not been called, */
                {
                    init_genrand(5489); /* a default initial seed is used */
                }

                for (kk = 0; kk < N - M; kk++)
                {
                    y = (_mt[kk] & UpperMask) | (_mt[kk + 1] & LowerMask);
                    _mt[kk] = _mt[kk + M] ^ (y >> 1) ^ Mag01[y & 0x1];
                }

                for (; kk < N - 1; kk++)
                {
                    y = (_mt[kk] & UpperMask) | (_mt[kk + 1] & LowerMask);
                    _mt[kk] = _mt[kk + (M - N)] ^ (y >> 1) ^ Mag01[y & 0x1];
                }

                y = (_mt[N - 1] & UpperMask) | (_mt[0] & LowerMask);
                _mt[N - 1] = _mt[M - 1] ^ (y >> 1) ^ Mag01[y & 0x1];

                _mti = 0;
            }

            y = _mt[_mti++];

            /* Tempering */
            y ^= y >> 11;
            y ^= (y << 7) & 0x9d2c5680;
            y ^= (y << 15) & 0xefc60000;
            y ^= y >> 18;

            return y;
        }

        /// <summary>
        /// Returns a random double-precision floating point number greater than or equal to 0.0, and less than 1.0.
        /// </summary>
        protected sealed override double DoSample()
        {
            return genrand_int32()*Reciprocal;
        }

        /// <summary>
        /// Returns a random 32-bit signed integer greater than or equal to zero and less than <see cref="F:System.Int32.MaxValue"/>
        /// </summary>
        protected sealed override int DoSampleInteger()
        {
            uint uint32 = genrand_int32();
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
                buffer[i] = (byte)(genrand_int32() % 256);
            }
        }

        /* /// <summary>
        /// Generates a random number on [0,1) with 53-bit resolution.
        /// </summary>
        /// <returns>A random number on [0,1) with 53-bit resolution.</returns>
        public double NextDoubleResolution53()
        {
            ulong a = genrand_int32() >> 5, b = genrand_int32() >> 6;
            return (a * 67108864.0 + b) * (1.0 / 9007199254740992.0);
        }*/

        /// <summary>
        /// Fills an array with random numbers greater than or equal to 0.0 and less than 1.0.
        /// </summary>
        /// <remarks>Supports being called in parallel from multiple threads.</remarks>
        public static void Doubles(double[] values, int seed)
        {
            uint[] t = new uint[624];
            int k;
            uint s = (uint)seed;

            t[0] = s & 0xffffffff;
            for (k = 1; k < N; k++)
            {
                t[k] = 1812433253*(t[k - 1] ^ (t[k - 1] >> 30)) + (uint)k;
                t[k] &= 0xffffffff;
            }

            for (int i = 0; i < values.Length; i++)
            {
                uint y;

                if (k >= N)
                {
                    int kk;
                    for (kk = 0; kk < N - M; kk++)
                    {
                        y = (t[kk] & UpperMask) | (t[kk + 1] & LowerMask);
                        t[kk] = t[kk + M] ^ (y >> 1) ^ Mag01[y & 0x1];
                    }

                    for (; kk < N - 1; kk++)
                    {
                        y = (t[kk] & UpperMask) | (t[kk + 1] & LowerMask);
                        t[kk] = t[kk + (M - N)] ^ (y >> 1) ^ Mag01[y & 0x1];
                    }

                    y = (t[N - 1] & UpperMask) | (t[0] & LowerMask);
                    t[N - 1] = t[M - 1] ^ (y >> 1) ^ Mag01[y & 0x1];

                    k = 0;
                }

                y = t[k++];

                /* Tempering */
                y ^= y >> 11;
                y ^= (y << 7) & 0x9d2c5680;
                y ^= (y << 15) & 0xefc60000;
                y ^= y >> 18;

                values[i] = y*Reciprocal;
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
            uint[] t = new uint[624];
            int k;
            uint s = (uint)seed;

            t[0] = s & 0xffffffff;
            for (k = 1; k < N; k++)
            {
                t[k] = 1812433253*(t[k - 1] ^ (t[k - 1] >> 30)) + (uint)k;
                t[k] &= 0xffffffff;
            }

            while (true)
            {
                uint y;

                if (k >= N)
                {
                    int kk;
                    for (kk = 0; kk < N - M; kk++)
                    {
                        y = (t[kk] & UpperMask) | (t[kk + 1] & LowerMask);
                        t[kk] = t[kk + M] ^ (y >> 1) ^ Mag01[y & 0x1];
                    }

                    for (; kk < N - 1; kk++)
                    {
                        y = (t[kk] & UpperMask) | (t[kk + 1] & LowerMask);
                        t[kk] = t[kk + (M - N)] ^ (y >> 1) ^ Mag01[y & 0x1];
                    }

                    y = (t[N - 1] & UpperMask) | (t[0] & LowerMask);
                    t[N - 1] = t[M - 1] ^ (y >> 1) ^ Mag01[y & 0x1];

                    k = 0;
                }

                y = t[k++];

                /* Tempering */
                y ^= y >> 11;
                y ^= (y << 7) & 0x9d2c5680;
                y ^= (y << 15) & 0xefc60000;
                y ^= y >> 18;

                yield return y*Reciprocal;
            }
        }
    }
}
