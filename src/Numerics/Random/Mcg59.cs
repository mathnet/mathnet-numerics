// <copyright file="Mcg59.cs" company="Math.NET">
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
    /// Multiplicative congruential generator using a modulus of 2^59 and a multiplier of 13^13.
    /// </summary>
    public partial class Mcg59 : AbstractRandomNumberGenerator
    {
        private const double _reciprocal = 1.0 / _modulus;
        private const ulong _modulus = 576460752303423488;
        private const ulong _multiplier = 302875106592253;
        private ulong _xn;

        /// <summary>
        /// Initializes a new instance of the <see cref="Mcg59"/> class using
        /// the current time as the seed.
        /// </summary>
        public Mcg59() : this((int) DateTime.Now.Ticks)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Mcg59"/> class using
        /// the current time as the seed.
        /// </summary>
        /// <param name="threadSafe">if set to <c>true</c> , the class is thread safe.</param>
        public Mcg59(bool threadSafe) : this((int)DateTime.Now.Ticks, threadSafe)
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Mcg59"/> class.
        /// </summary>
        /// <param name="seed">The seed value.</param>
        /// <remarks>If the seed value is zero, it is set to one. Uses the
        /// value of <see cref="Control.ThreadSafeRandomNumberGenerators"/> to
        /// set whether the instance is thread safe.</remarks>
        public Mcg59(int seed) : this(seed, Control.ThreadSafeRandomNumberGenerators)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Mcg59"/> class.
        /// </summary>
        /// <param name="seed">The seed value.</param>
        /// <remarks>The seed is set to 1, if the zero is used as the seed.</remarks>
        /// <param name="threadSafe">if set to <c>true</c> , the class is thread safe.</param>
        public Mcg59(int seed, bool threadSafe) : base(threadSafe)
        {
            if( seed == 0)
            {
                seed = 1;
            }
            _xn = (uint) seed % _modulus;
        }

        /// <summary>
        /// Returns a random number between 0.0 and 1.0.
        /// </summary>
        /// <returns>
        /// A double-precision floating point number greater than or equal to 0.0, and less than 1.0.
        /// </returns>
        protected override double DoSample()
        {
            double ret = _xn * _reciprocal;
            _xn = (_xn * _multiplier) % _modulus;
            return ret;
        }
    }
}