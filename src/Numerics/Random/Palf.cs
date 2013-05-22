// <copyright file="Palf.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
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

namespace MathNet.Numerics.Random
{
    using System;
    using Properties;
    using Threading;

    /// <summary>
    /// Represents a Parallel Additive Lagged Fibonacci pseudo-random number generator.
    /// </summary>
    /// <remarks>
    /// The <see cref="Palf"/> type bases upon the implementation in the 
    /// <a href="http://www.boost.org/libs/random/index.html">Boost Random Number Library</a>.
    /// It uses the modulus 2<sup>32</sup> and by default the "lags" 418 and 1279. Some popular pairs are presented on 
    /// <a href="http://en.wikipedia.org/wiki/Lagged_Fibonacci_generator">Wikipedia - Lagged Fibonacci generator</a>.
    /// </remarks>
    public class Palf : AbstractRandomNumberGenerator
    {
        /// <summary>
        /// Default value for the ShortLag
        /// </summary>
        const int DefaultShortLag = 418;

        /// <summary>
        /// Default value for the LongLag
        /// </summary>
        const int DefaultLongLag = 1279;

        /// <summary>
        /// The multiplier to compute a double-precision floating point number [0, 1)
        /// </summary>
        const double IntToDoubleMultiplier = 1.0/(int.MaxValue + 1.0);

        /// <summary>
        /// Initializes a new instance of the <see cref="Palf"/> class using
        /// the current time as the seed.
        /// </summary>
        /// <remarks>If the seed value is zero, it is set to one. Uses the
        /// value of <see cref="Control.ThreadSafeRandomNumberGenerators"/> to
        /// set whether the instance is thread safe.</remarks>
        public Palf()
            : this((int) DateTime.Now.Ticks)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Palf"/> class using
        /// the current time as the seed.
        /// </summary>
        /// <param name="threadSafe">if set to <c>true</c> , the class is thread safe.</param>
        public Palf(bool threadSafe)
            : this((int) DateTime.Now.Ticks, threadSafe, DefaultShortLag, DefaultLongLag)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Palf"/> class.
        /// </summary>
        /// <param name="seed">The seed value.</param>
        /// <remarks>If the seed value is zero, it is set to one. Uses the
        /// value of <see cref="Control.ThreadSafeRandomNumberGenerators"/> to
        /// set whether the instance is thread safe.</remarks>
        public Palf(int seed)
            : this(seed, Control.ThreadSafeRandomNumberGenerators, DefaultShortLag, DefaultLongLag)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Palf"/> class.
        /// </summary>
        /// <param name="seed">The seed value.</param>
        /// <param name="threadSafe">if set to <c>true</c>, the class is thread safe.</param>
        /// <param name="shortLag">The ShortLag value</param>
        /// <param name="longLag">TheLongLag value</param>
        public Palf(int seed, bool threadSafe, int shortLag, int longLag)
            : base(threadSafe)
        {
            if (shortLag < 1)
            {
                throw new ArgumentException(Resources.ArgumentMustBePositive, "shortLag");
            }

            if (longLag <= shortLag)
            {
                throw new ArgumentException("longLag");
            }

            if (seed == 0)
            {
                seed = 1;
            }

            ShortLag = shortLag;

            // Align LongLag to number of worker threads. 
            if (longLag%Control.NumberOfParallelWorkerThreads == 0)
            {
                LongLag = longLag;
            }
            else
            {
                LongLag = ((longLag/Control.NumberOfParallelWorkerThreads) + 1)*Control.NumberOfParallelWorkerThreads;
            }

            _x = new uint[LongLag];
            var gen = new MersenneTwister(seed, threadSafe);
            for (var j = 0; j < LongLag; ++j)
            {
                _x[j] = (uint) (gen.NextDouble()*uint.MaxValue);
            }

            _i = LongLag;
        }

        /// <summary>
        /// Gets the short lag of the Lagged Fibonacci pseudo-random number generator.
        /// </summary>
        public int ShortLag { get; private set; }

        /// <summary>
        /// Gets the long lag of the Lagged Fibonacci pseudo-random number generator.
        /// </summary>
        public int LongLag { get; private set; }

        /// <summary>
        /// Stores an array of <see cref="LongLag"/> random numbers
        /// </summary>
        readonly uint[] _x;

        /// <summary>
        /// Stores an index for the random number array element that will be accessed next.
        /// </summary>
        int _i;

        /// <summary>
        /// Fills the array <see cref="_x"/> with <see cref="LongLag"/> new unsigned random numbers.
        /// </summary>
        /// <remarks>
        /// Generated random numbers are 32-bit unsigned integers greater than or equal to 0
        /// and less than or equal to <see cref="Int32.MaxValue"/>.
        /// </remarks>
        void Fill()
        {
            CommonParallel.For(0, Control.NumberOfParallelWorkerThreads, (u, v) =>
                {
                    for (int index = u; index < v; index++)
                    {
                        // Two loops to avoid costly modulo operations
                        for (var j = index; j < ShortLag; j = j + Control.NumberOfParallelWorkerThreads)
                        {
                            _x[j] += _x[j + (LongLag - ShortLag)];
                        }

                        for (var j = ShortLag + index; j < LongLag; j = j + Control.NumberOfParallelWorkerThreads)
                        {
                            _x[j] += _x[j - ShortLag - index];
                        }
                    }
                });
            _i = 0;
        }

        /// <summary>
        /// Returns a random number between 0.0 and 1.0.
        /// </summary>
        /// <returns>
        /// A double-precision floating point number greater than or equal to 0.0, and less than 1.0.
        /// </returns>
        protected override double DoSample()
        {
            if (_i >= LongLag)
            {
                Fill();
            }

            var x = _x[_i++];
            return (int) (x >> 1)*IntToDoubleMultiplier;
        }
    }
}
