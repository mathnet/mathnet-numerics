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

using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Runtime;

namespace MathNet.Numerics.Random
{
    /// <summary>
    /// A random number generator based on the <see cref="System.Security.Cryptography.RandomNumberGenerator"/> class in the .NET library.
    /// </summary>
    public sealed class CryptoRandomSource : RandomSource, IDisposable
    {
        const double Reciprocal = 1.0/4294967296.0; // 1.0/(uint.MaxValue + 1.0)
        readonly RandomNumberGenerator _crypto;

        /// <summary>
        /// Construct a new random number generator with a random seed.
        /// </summary>
        /// <remarks>Uses <see cref="System.Security.Cryptography.RandomNumberGenerator"/> and uses the value of
        /// <see cref="Control.ThreadSafeRandomNumberGenerators"/> to set whether the instance is thread safe.</remarks>
        public CryptoRandomSource()
        {
            _crypto = RandomNumberGenerator.Create();
        }

        /// <summary>
        /// Construct a new random number generator with random seed.
        /// </summary>
        /// <param name="rng">The <see cref="RandomNumberGenerator"/> to use.</param>
        /// <remarks>Uses the value of  <see cref="Control.ThreadSafeRandomNumberGenerators"/> to set whether the instance is thread safe.</remarks>
        public CryptoRandomSource(RandomNumberGenerator rng)
        {
            _crypto = rng;
        }

        /// <summary>
        /// Construct a new random number generator with random seed.
        /// </summary>
        /// <remarks>Uses <see cref="System.Security.Cryptography.RandomNumberGenerator"/></remarks>
        /// <param name="threadSafe">if set to <c>true</c> , the class is thread safe.</param>
        public CryptoRandomSource(bool threadSafe) : base(threadSafe)
        {
            _crypto = RandomNumberGenerator.Create();
        }

        /// <summary>
        /// Construct a new random number generator with random seed.
        /// </summary>
        /// <param name="rng">The <see cref="RandomNumberGenerator"/> to use.</param>
        /// <param name="threadSafe">if set to <c>true</c> , the class is thread safe.</param>
        public CryptoRandomSource(RandomNumberGenerator rng, bool threadSafe) : base(threadSafe)
        {
            _crypto = rng;
        }

        /// <summary>
        /// Fills the elements of a specified array of bytes with random numbers in full range, including zero and 255 (<see cref="F:System.Byte.MaxValue"/>).
        /// </summary>
        protected override void DoSampleBytes(byte[] buffer)
        {
            _crypto.GetBytes(buffer);
        }

        /// <summary>
        /// Returns a random double-precision floating point number greater than or equal to 0.0, and less than 1.0.
        /// </summary>
        protected  override double DoSample()
        {
            var bytes = new byte[4];
            _crypto.GetBytes(bytes);
            return BitConverter.ToUInt32(bytes, 0)*Reciprocal;
        }

        /// <summary>
        /// Returns a random 32-bit signed integer greater than or equal to zero and less than <see cref="F:System.Int32.MaxValue"/>
        /// </summary>
        protected override int DoSampleInteger()
        {
            var bytes = new byte[4];
            _crypto.GetBytes(bytes);
            uint uint32 = BitConverter.ToUInt32(bytes, 0);
            int int31 = (int)(uint32 >> 1);
            if (int31 == int.MaxValue)
            {
                return DoSampleInteger();
            }

            return int31;
        }

        public void Dispose()
        {
            _crypto.Dispose();
        }

        /// <summary>
        /// Fills an array with random numbers greater than or equal to 0.0 and less than 1.0.
        /// </summary>
        /// <remarks>Supports being called in parallel from multiple threads.</remarks>
        public static void Doubles(double[] values)
        {
            var bytes = new byte[values.Length*4];

            using (var rnd = RandomNumberGenerator.Create())
            {
                rnd.GetBytes(bytes);
            }

            for (int i = 0; i < values.Length; i++)
            {
                values[i] = BitConverter.ToUInt32(bytes, i*4)*Reciprocal;
            }
        }

        /// <summary>
        /// Returns an array of random numbers greater than or equal to 0.0 and less than 1.0.
        /// </summary>
        /// <remarks>Supports being called in parallel from multiple threads.</remarks>
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static double[] Doubles(int length)
        {
            var data = new double[length];
            Doubles(data);
            return data;
        }

        /// <summary>
        /// Returns an infinite sequence of random numbers greater than or equal to 0.0 and less than 1.0.
        /// </summary>
        /// <remarks>Supports being called in parallel from multiple threads.</remarks>
        public static IEnumerable<double> DoubleSequence()
        {
            var rnd = RandomNumberGenerator.Create();
            var buffer = new byte[1024*4];

            while (true)
            {
                rnd.GetBytes(buffer);
                for (int i = 0; i < buffer.Length; i += 4)
                {
                    yield return BitConverter.ToUInt32(buffer, i)*Reciprocal;
                }
            }
        }
    }
}
