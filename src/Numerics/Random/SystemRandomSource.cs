// <copyright file="SystemCrypto.cs" company="Math.NET">
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

using System.Collections.Generic;
using System.Runtime.Serialization;
using MathNet.Numerics.Threading;
using System.Threading;
using System;
using System.Runtime;

namespace MathNet.Numerics.Random
{
    /// <summary>
    /// A random number generator based on the <see cref="System.Random"/> class in the .NET library.
    /// </summary>
    [Serializable]
    [DataContract(Namespace = "urn:MathNet/Numerics/Random")]
    public class SystemRandomSource : RandomSource
    {
        [DataMember(Order = 1)]
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

        static readonly ThreadLocal<SystemRandomSource> DefaultInstance = new ThreadLocal<SystemRandomSource>(() => new SystemRandomSource(RandomSeed.Robust(), true));

        /// <summary>
        /// Default instance, thread-safe.
        /// </summary>
        public static SystemRandomSource Default => DefaultInstance.Value;

        /// <summary>
        /// Returns a random double-precision floating point number greater than or equal to 0.0, and less than 1.0.
        /// </summary>
        protected sealed override double DoSample()
        {
            return _random.NextDouble();
        }

        /// <summary>
        /// Returns a random 32-bit signed integer greater than or equal to zero and less than <see cref="F:System.Int32.MaxValue"/>
        /// </summary>
        protected override int DoSampleInteger()
        {
            return _random.Next();
        }

        /// <summary>
        /// Returns a random 32-bit signed integer within the specified range.
        /// </summary>
        /// <param name="maxExclusive">The exclusive upper bound of the random number returned. Range: maxExclusive ≥ 2 (not verified, must be ensured by caller).</param>
        protected override int DoSampleInteger(int maxExclusive)
        {
            return _random.Next(maxExclusive);
        }

        /// <summary>
        /// Returns a random 32-bit signed integer within the specified range.
        /// </summary>
        /// <param name="minInclusive">The inclusive lower bound of the random number returned.</param>
        /// <param name="maxExclusive">The exclusive upper bound of the random number returned. Range: maxExclusive ≥ minExclusive + 2 (not verified, must be ensured by caller).</param>
        protected override int DoSampleInteger(int minInclusive, int maxExclusive)
        {
            return _random.Next(minInclusive, maxExclusive);
        }

        /// <summary>
        /// Fills the elements of a specified array of bytes with random numbers in full range, including zero and 255 (<see cref="F:System.Byte.MaxValue"/>).
        /// </summary>
        protected override void DoSampleBytes(byte[] buffer)
        {
            _random.NextBytes(buffer);
        }

        /// <summary>
        /// Fill an array with uniform random numbers greater than or equal to 0.0 and less than 1.0.
        /// WARNING: potentially very short random sequence length, can generate repeated partial sequences.
        /// </summary>
        /// <remarks>Parallelized on large length, but also supports being called in parallel from multiple threads</remarks>
        public static void FastDoubles(double[] values)
        {
            if (values.Length < 2048)
            {
                Default.NextDoubles(values);
                return;
            }

            CommonParallel.For(0, values.Length, values.Length >= 65536 ? 8192 : values.Length >= 16384 ? 2048 : 1024, (a, b) =>
            {
                var rnd = new System.Random(RandomSeed.Robust());
                for (int i = a; i < b; i++)
                {
                    values[i] = rnd.NextDouble();
                }
            });
        }

        /// <summary>
        /// Returns an array of uniform random numbers greater than or equal to 0.0 and less than 1.0.
        /// WARNING: potentially very short random sequence length, can generate repeated partial sequences.
        /// </summary>
        /// <remarks>Parallelized on large length, but also supports being called in parallel from multiple threads</remarks>
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static double[] FastDoubles(int length)
        {
            var data = new double[length];
            FastDoubles(data);
            return data;
        }

        /// <summary>
        /// Returns an infinite sequence of uniform random numbers greater than or equal to 0.0 and less than 1.0.
        /// </summary>
        /// <remarks>Supports being called in parallel from multiple threads, but the result must be enumerated from a single thread each.</remarks>
        public static IEnumerable<double> DoubleSequence()
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
        /// Fills an array with random numbers greater than or equal to 0.0 and less than 1.0.
        /// </summary>
        /// <remarks>Supports being called in parallel from multiple threads.</remarks>
        public static void Doubles(double[] values, int seed)
        {
            var rnd = new System.Random(seed);
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = rnd.NextDouble();
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
            var rnd = new System.Random(seed);
            while (true)
            {
                yield return rnd.NextDouble();
            }
        }
    }
}
