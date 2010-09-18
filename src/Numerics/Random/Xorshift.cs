// <copyright file="Xorshift.cs" company="Math.NET">
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
    /// Xor-shift pseudo random number generator (RNG) specified in Marsaglia, George. (2003). Xorshift RNGs.
    /// http://www.jstatsoft.org/v08/i14/xorshift.pdf
    /// </summary>
    public class Xorshift : AbstractRandomNumberGenerator
    {
                /// <summary>
        /// Initializes a new instance of the <see cref="Xorshift"/> class using
        /// the current time as the seed.
        /// </summary>
        /// <remarks>If the seed value is zero, it is set to one. Uses the
        /// value of <see cref="Control.ThreadSafeRandomNumberGenerators"/> to
        /// set whether the instance is thread safe.</remarks>
        public Xorshift() : this((int)DateTime.Now.Ticks)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Xorshift"/> class using
        /// the current time as the seed.
        /// </summary>
        /// <param name="threadSafe">if set to <c>true</c> , the class is thread safe.</param>
        public Xorshift(bool threadSafe) : this((int)DateTime.Now.Ticks, threadSafe)
        {
        }

                /// <summary>
        /// Initializes a new instance of the <see cref="Xorshift"/> class.
        /// </summary>
        /// <param name="seed">The seed value.</param>
        /// <remarks>If the seed value is zero, it is set to one. Uses the
        /// value of <see cref="Control.ThreadSafeRandomNumberGenerators"/> to
        /// set whether the instance is thread safe.</remarks>
        public Xorshift(int seed) : this(seed, Control.ThreadSafeRandomNumberGenerators)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Xorshift"/> class.
        /// </summary>
        /// <param name="seed">The seed value.</param>
        /// <param name="threadSafe">if set to <c>true</c>, the class is thread safe.</param>
        public Xorshift(int seed, bool threadSafe) : base(threadSafe)
        {
            if (seed == 0)
            {
                seed = 1;
            }

            _x = (uint)seed;
            _y = YSeed;
            _z = ZSeed;
            _w = WSeed;
        }

        /// <summary>
        /// Y seed is 423864282.
        /// </summary>
        private const uint YSeed = 423864282;

        /// <summary>
        /// Y seed is 643534723.
        /// </summary>
        private const uint ZSeed = 643534723;

        /// <summary>
        /// W seed is 84452734.
        /// </summary>
        private const uint WSeed = 84452734;

        /// <summary>
        /// The multiplier to compute a double-precision floating point number [0, 1)
        /// </summary>
        private const double IntToDoubleMultiplier = 1.0 / (int.MaxValue + 1.0);

        /// <summary>
        /// Seed or last but three unsigned random number. 
        /// </summary>
        private uint _x;

        /// <summary>
        /// Last but two unsigned random number. 
        /// </summary>
        private uint _y;

        /// <summary>
        /// Last but one unsigned random number. 
        /// </summary>
        private uint _z;

        /// <summary>
        /// Last generated unsigned random number. 
        /// </summary>
        private uint _w;

        /// <summary>
        /// Returns a random number between 0.0 and 1.0.
        /// </summary>
        /// <returns>
        /// A double-precision floating point number greater than or equal to 0.0, and less than 1.0.
        /// </returns>
        protected override double DoSample()
        {
            var t = _x ^ (_x << 11);
            _x = _y;
            _y = _z;
            _z = _w;
            _w = (_w ^ (_w >> 19)) ^ (t ^ (t >> 8));

            return (int)(_w >> 1) * IntToDoubleMultiplier;
        }
    }
}
