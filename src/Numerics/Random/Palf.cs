// <copyright file="Palf.cs" company="Math.NET">
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
    /// Represents a Parallel Additive Lagged Fibonacci pseudo-random number generator.
    /// </summary>
    /// <remarks>
    /// The <see cref="Palf"/> type bases upon the implementation in the
    /// <a href="http://www.boost.org/libs/random/index.html">Boost Random Number Library</a>.
    /// It uses the modulus 2<sup>32</sup> and by default the "lags" 418 and 1279. Some popular pairs are presented on
    /// <a href="http://en.wikipedia.org/wiki/Lagged_Fibonacci_generator">Wikipedia - Lagged Fibonacci generator</a>.
    /// </remarks>
    [Serializable]
    [DataContract(Namespace = "urn:MathNet/Numerics/Random")]
    public class Palf : RandomSource
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
        const double Reciprocal = 1.0/4294967296.0; // 1.0/(uint.MaxValue + 1.0)

        /// <summary>
        /// Initializes a new instance of the <see cref="Palf"/> class using
        /// a seed based on time and unique GUIDs.
        /// </summary>
        /// <remarks>If the seed value is zero, it is set to one. Uses the
        /// value of <see cref="Control.ThreadSafeRandomNumberGenerators"/> to
        /// set whether the instance is thread safe.</remarks>
        public Palf() : this(RandomSeed.Robust(), Control.ThreadSafeRandomNumberGenerators, DefaultShortLag, DefaultLongLag)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Palf"/> class using
        /// a seed based on time and unique GUIDs.
        /// </summary>
        /// <param name="threadSafe">if set to <c>true</c> , the class is thread safe.</param>
        public Palf(bool threadSafe) : this(RandomSeed.Robust(), threadSafe, DefaultShortLag, DefaultLongLag)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Palf"/> class.
        /// </summary>
        /// <param name="seed">The seed value.</param>
        /// <remarks>If the seed value is zero, it is set to one. Uses the
        /// value of <see cref="Control.ThreadSafeRandomNumberGenerators"/> to
        /// set whether the instance is thread safe.</remarks>
        public Palf(int seed) : this(seed, Control.ThreadSafeRandomNumberGenerators, DefaultShortLag, DefaultLongLag)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Palf"/> class.
        /// </summary>
        /// <param name="seed">The seed value.</param>
        /// <param name="threadSafe">if set to <c>true</c> , the class is thread safe.</param>
        public Palf(int seed, bool threadSafe) : this(seed, threadSafe, DefaultShortLag, DefaultLongLag)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Palf"/> class.
        /// </summary>
        /// <param name="seed">The seed value.</param>
        /// <param name="threadSafe">if set to <c>true</c>, the class is thread safe.</param>
        /// <param name="shortLag">The ShortLag value</param>
        /// <param name="longLag">TheLongLag value</param>
        public Palf(int seed, bool threadSafe, int shortLag, int longLag) : base(threadSafe)
        {
            if (shortLag < 1)
            {
                throw new ArgumentException("Value must be positive.", nameof(shortLag));
            }

            if (longLag <= shortLag)
            {
                throw new ArgumentException("The upper bound must be strictly larger than the lower bound.", nameof(longLag));
            }

            if (seed == 0)
            {
                seed = 1;
            }

            _threads = Control.MaxDegreeOfParallelism;
            ShortLag = shortLag;

            // Align LongLag to number of worker threads.
            if (longLag%_threads == 0)
            {
                LongLag = longLag;
            }
            else
            {
                LongLag = ((longLag/_threads) + 1)*_threads;
            }

            _x = Generate.Map(MersenneTwister.Doubles(LongLag, seed), uniform => (uint)(uniform*uint.MaxValue));
            _k = LongLag;
        }

        /// <summary>
        /// Gets the short lag of the Lagged Fibonacci pseudo-random number generator.
        /// </summary>
        [DataMember(Order = 1)]
        public int ShortLag { get; private set; }

        /// <summary>
        /// Gets the long lag of the Lagged Fibonacci pseudo-random number generator.
        /// </summary>
        [DataMember(Order = 2)]
        public int LongLag { get; private set; }

        /// <summary>
        /// Stores an array of <see cref="LongLag"/> random numbers
        /// </summary>
        [DataMember(Order = 3)]
        readonly uint[] _x;

        [DataMember(Order = 4)]
        readonly int _threads;

        /// <summary>
        /// Stores an index for the random number array element that will be accessed next.
        /// </summary>
        [DataMember(Order = 5)]
        int _k;

        /// <summary>
        /// Fills the array <see cref="_x"/> with <see cref="LongLag"/> new unsigned random numbers.
        /// </summary>
        /// <remarks>
        /// Generated random numbers are 32-bit unsigned integers greater than or equal to 0
        /// and less than or equal to <see cref="Int32.MaxValue"/>.
        /// </remarks>
        void Fill()
        {
            //CommonParallel.For(0, Control.NumberOfParallelWorkerThreads, (u, v) =>
            //{
            //    for (int index = u; index < v; index++)
            //    {
            //        // Two loops to avoid costly modulo operations
            //        for (var j = index; j < ShortLag; j = j + Control.NumberOfParallelWorkerThreads)
            //        {
            //            _x[j] += _x[j + (LongLag - ShortLag)];
            //        }

            //        for (var j = ShortLag + index; j < LongLag; j = j + Control.NumberOfParallelWorkerThreads)
            //        {
            //            _x[j] += _x[j - ShortLag - index];
            //        }
            //    }
            //});

            for (int index = 0; index < _threads; index++)
            {
                // Two loops to avoid costly modulo operations
                for (var j = index; j < ShortLag; j = j + _threads)
                {
                    _x[j] += _x[j + (LongLag - ShortLag)];
                }

                for (var j = ShortLag + index; j < LongLag; j = j + _threads)
                {
                    _x[j] += _x[j - ShortLag - index];
                }
            }

            _k = 0;
        }

        /// <summary>
        /// Returns a random double-precision floating point number greater than or equal to 0.0, and less than 1.0.
        /// </summary>
        protected sealed override double DoSample()
        {
            if (_k >= LongLag)
            {
                Fill();
            }

            return _x[_k++] * Reciprocal;
        }

        /// <summary>
        /// Returns a random 32-bit signed integer greater than or equal to zero and less than <see cref="F:System.Int32.MaxValue"/>
        /// </summary>
        protected override int DoSampleInteger()
        {
            if (_k >= LongLag)
            {
                Fill();
            }

            uint uint32 = _x[_k++];
            int int31 = (int)(uint32 >> 1);
            if (int31 == int.MaxValue)
            {
                return DoSampleInteger();
            }

            return int31;
        }

        /// <summary>
        /// Fills an array with random numbers greater than or equal to 0.0 and less than 1.0.
        /// </summary>
        /// <remarks>Supports being called in parallel from multiple threads.</remarks>
        public static void Doubles(double[] values, int seed)
        {
            if (seed == 0)
            {
                seed = 1;
            }

            int threads = Control.MaxDegreeOfParallelism;
            const int shortLag = DefaultShortLag;
            var longLag = DefaultLongLag;

            // Align LongLag to number of worker threads.
            if (longLag%threads != 0)
            {
                longLag = ((longLag/threads) + 1)*threads;
            }

            var x = Generate.Map(MersenneTwister.Doubles(longLag, seed), uniform => (uint)(uniform*uint.MaxValue));
            var k = longLag;

            for (int i = 0; i < values.Length; i++)
            {
                if (k >= longLag)
                {
                    for (int index = 0; index < threads; index++)
                    {
                        // Two loops to avoid costly modulo operations
                        for (var j = index; j < shortLag; j = j + threads)
                        {
                            x[j] += x[j + (longLag - shortLag)];
                        }

                        for (var j = shortLag + index; j < longLag; j = j + threads)
                        {
                            x[j] += x[j - shortLag - index];
                        }
                    }

                    k = 0;
                }

                values[i] = x[k++]*Reciprocal;
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
            if (seed == 0)
            {
                seed = 1;
            }

            int threads = Control.MaxDegreeOfParallelism;
            const int shortLag = DefaultShortLag;
            var longLag = DefaultLongLag;

            // Align LongLag to number of worker threads.
            if (longLag%threads != 0)
            {
                longLag = ((longLag/threads) + 1)*threads;
            }

            var x = Generate.Map(MersenneTwister.Doubles(longLag, seed), uniform => (uint)(uniform*uint.MaxValue));
            var k = longLag;

            while (true)
            {
                if (k >= longLag)
                {
                    for (int index = 0; index < threads; index++)
                    {
                        // Two loops to avoid costly modulo operations
                        for (var j = index; j < shortLag; j = j + threads)
                        {
                            x[j] += x[j + (longLag - shortLag)];
                        }

                        for (var j = shortLag + index; j < longLag; j = j + threads)
                        {
                            x[j] += x[j - shortLag - index];
                        }
                    }

                    k = 0;
                }

                yield return x[k++]*Reciprocal;
            }
        }
    }
}
