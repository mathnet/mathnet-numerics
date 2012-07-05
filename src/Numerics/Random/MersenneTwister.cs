// <copyright file="MersenneTwister.cs" company="Math.NET">
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

namespace MathNet.Numerics.Random
{
    using System;

    /// <summary>
    /// Random number generator using Mersenne Twister 19937 algorithm.
    /// </summary>
    public class MersenneTwister : AbstractRandomNumberGenerator, IDisposable
    {
        /// <summary>
        /// Mersenne twister constant.
        /// </summary>
        private const uint _lower_mask = 0x7fffffff;

        /// <summary>
        /// Mersenne twister constant.
        /// </summary>
        private const int _m = 397;

        /// <summary>
        /// Mersenne twister constant.
        /// </summary>
        private const uint _matrix_a = 0x9908b0df;

        /// <summary>
        /// Mersenne twister constant.
        /// </summary>
        private const int _n = 624;

        /// <summary>
        /// Mersenne twister constant.
        /// </summary>
        private const double _reciprocal = 1.0/4294967296.0;

        /// <summary>
        /// Mersenne twister constant.
        /// </summary>
        private const uint _upper_mask = 0x80000000;

        /// <summary>
        /// Mersenne twister constant.
        /// </summary>
        private static readonly uint[] _mag01 = {0x0U, _matrix_a};

        /// <summary>
        /// Mersenne twister constant.
        /// </summary>
        private readonly uint[] _mt = new uint[624];

        /// <summary>
        /// Mersenne twister constant.
        /// </summary>
        private int mti = _n + 1;

        /// <summary>
        /// Initializes a new instance of the <see cref="MersenneTwister"/> class using
        /// the current time as the seed.
        /// </summary>
        /// <remarks>If the seed value is zero, it is set to one. Uses the
        /// value of <see cref="Control.ThreadSafeRandomNumberGenerators"/> to
        /// set whether the instance is thread safe.</remarks>
        public MersenneTwister() : this((int) DateTime.Now.Ticks)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MersenneTwister"/> class using
        /// the current time as the seed.
        /// </summary>
        /// <param name="threadSafe">if set to <c>true</c> , the class is thread safe.</param>
        public MersenneTwister(bool threadSafe) : this((int) DateTime.Now.Ticks, threadSafe)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MersenneTwister"/> class.
        /// </summary>
        /// <param name="seed">The seed value.</param>
        /// <remarks>Uses the value of <see cref="Control.ThreadSafeRandomNumberGenerators"/> to
        /// set whether the instance is thread safe.</remarks>        
        public MersenneTwister(int seed) : this(seed, Control.ThreadSafeRandomNumberGenerators)
        {
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

        private void init_genrand(uint s)
        {
            _mt[0] = s & 0xffffffff;
            for (mti = 1; mti < _n; mti++)
            {
                _mt[mti] = (1812433253*(_mt[mti - 1] ^ (_mt[mti - 1] >> 30)) + (uint) mti);
                /* See Knuth TAOCP Vol2. 3rd Ed. P.106 for multiplier. */
                /* In the previous versions, MSBs of the seed affect   */
                /* only MSBs of the array _mt[].                        */
                /* 2002/01/09 modified by Makoto Matsumoto             */
                _mt[mti] &= 0xffffffff;
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

        private uint genrand_int32()
        {
            uint y;

            /* mag01[x] = x * MATRIX_A  for x=0,1 */

            if (mti >= _n)
            {
                /* generate _n words at one time */
                int kk;

                if (mti == _n + 1) /* if init_genrand() has not been called, */
                    init_genrand(5489); /* a default initial seed is used */

                for (kk = 0; kk < _n - _m; kk++)
                {
                    y = (_mt[kk] & _upper_mask) | (_mt[kk + 1] & _lower_mask);
                    _mt[kk] = _mt[kk + _m] ^ (y >> 1) ^ _mag01[y & 0x1];
                }
                for (; kk < _n - 1; kk++)
                {
                    y = (_mt[kk] & _upper_mask) | (_mt[kk + 1] & _lower_mask);
                    _mt[kk] = _mt[kk + (_m - _n)] ^ (y >> 1) ^ _mag01[y & 0x1];
                }
                y = (_mt[_n - 1] & _upper_mask) | (_mt[0] & _lower_mask);
                _mt[_n - 1] = _mt[_m - 1] ^ (y >> 1) ^ _mag01[y & 0x1];

                mti = 0;
            }

            y = _mt[mti++];

            /* Tempering */
            y ^= (y >> 11);
            y ^= (y << 7) & 0x9d2c5680;
            y ^= (y << 15) & 0xefc60000;
            y ^= (y >> 18);

            return y;
        }

        /// <summary>
        /// Returns a random number between 0.0 and 1.0.
        /// </summary>
        /// <returns>
        /// A double-precision floating point number greater than or equal to 0.0, and less than 1.0.
        /// </returns>
        protected override double DoSample()
        {
            return genrand_int32() * _reciprocal;
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

        #region IDisposable Members

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            //do nothing in the managed version.
        }

        #endregion
    }
}