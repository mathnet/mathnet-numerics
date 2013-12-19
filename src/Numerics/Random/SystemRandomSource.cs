// <copyright file="SystemCrypto.cs" company="Math.NET">
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

using System.Collections.Generic;
using System.Threading;
using MathNet.Numerics.Threading;

namespace MathNet.Numerics.Random
{
    /// <summary>
    /// A random number generator based on the <see cref="System.Random"/> class in the .NET library.
    /// </summary>
    public class SystemRandomSource : RandomSource
    {
        readonly System.Random _random;

        /// <summary>
        /// Construct a new random number generator with a random seed.
        /// </summary>
        public SystemRandomSource() : this(RandomSeed.Robust())
        {
        }

        /// <summary>
        /// Construct a new random number generator with random seed.
        /// </summary>
        /// <param name="threadSafe">if set to <c>true</c> , the class is thread safe.</param>
        public SystemRandomSource(bool threadSafe) : this(RandomSeed.Robust(), threadSafe)
        {
        }

        /// <summary>
        /// Construct a new random number generator with random seed.
        /// </summary>
        /// <param name="seed">The seed value.</param>
        public SystemRandomSource(int seed)
        {
            _random = new System.Random(seed);
        }

        /// <summary>
        /// Construct a new random number generator with random seed.
        /// </summary>
        /// <param name="seed">The seed value.</param>
        /// <param name="threadSafe">if set to <c>true</c> , the class is thread safe.</param>
        public SystemRandomSource(int seed, bool threadSafe) : base(threadSafe)
        {
            _random = new System.Random(seed);
        }

#if PORTABLE
        [ThreadStatic]
        static SystemRandomSource DefaultInstance;

        /// <summary>
        /// Default instance, thread-safe.
        /// </summary>
        public static SystemRandomSource Default
        {
            get
            {
                if (DefaultInstance == null)
                {
                    DefaultInstance = new SystemRandomSource(RandomSeed.Robust(), true);
                }
                return DefaultInstance;
            }
        }
#else
        static readonly ThreadLocal<SystemRandomSource> DefaultInstance = new ThreadLocal<SystemRandomSource>(() => new SystemRandomSource(RandomSeed.Robust(), true));

        /// <summary>
        /// Default instance, thread-safe.
        /// </summary>
        public static SystemRandomSource Default
        {
            get { return DefaultInstance.Value; }
        }
#endif

        /// <summary>
        /// Returns a random number between 0.0 and 1.0.
        /// </summary>
        /// <returns>
        /// A double-precision floating point number greater than or equal to 0.0, and less than 1.0.
        /// </returns>
        protected override double DoSample()
        {
            return _random.NextDouble();
        }

        /// <summary>
        /// Returns an array of uniform random numbers greater than or equal to 0.0 and less than 1.0.
        /// </summary>
        /// <remarks>Parallelized on large length, but also supports being called in parallel from multiple threads</remarks>
        public static double[] Samples(int length)
        {
            if (length < 2048)
            {
                return Default.NextDoubles(length);
            }

            var data = new double[length];
            CommonParallel.For(0, length, length >= 65536 ? 8192 : length >= 16384 ? 2048 : 1024, (a, b) =>
            {
                var rnd = new System.Random(RandomSeed.Robust());
                for (int i = a; i < b; i++)
                {
                    data[i] = rnd.NextDouble();
                }
            });
            return data;
        }

        /// <summary>
        /// Returns an infinite sequence of uniform random numbers greater than or equal to 0.0 and less than 1.0.
        /// </summary>
        /// <remarks>Supports being called in parallel from multiple threads, but the result must be enumerated from a single thread each.</remarks>
        public static IEnumerable<double> SampleSequence()
        {
            var rnd1 = Default;
            for (int i = 0; i < 128; i++)
            {
                yield return rnd1.NextDouble();
            }

            var rnd2 = new System.Random(RandomSeed.Robust());
            while (true)
            {
                yield return rnd2.NextDouble();
            }
        }

        /// <summary>
        /// Returns an array of random numbers greater than or equal to 0.0 and less than 1.0.
        /// </summary>
        /// <remarks>Supports being called in parallel from multiple threads.</remarks>
        public static double[] Samples(int length, int seed)
        {
            var rnd = new System.Random(seed);

            var data = new double[length];
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = rnd.NextDouble();
            }
            return data;
        }

        /// <summary>
        /// Returns an infinite sequence of random numbers greater than or equal to 0.0 and less than 1.0.
        /// </summary>
        /// <remarks>Supports being called in parallel from multiple threads, but the result must be enumerated from a single thread each.</remarks>
        public static IEnumerable<double> SampleSequence(int seed)
        {
            var rnd = new System.Random(seed);

            while (true)
            {
                yield return rnd.NextDouble();
            }
        }
    }
}
