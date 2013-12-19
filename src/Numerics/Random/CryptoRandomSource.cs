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

#if !PORTABLE

using System;
using System.Security.Cryptography;

namespace MathNet.Numerics.Random
{
    /// <summary>
    /// A random number generator based on the <see cref="System.Security.Cryptography.RandomNumberGenerator"/> class in the .NET library.
    /// </summary>
    public class CryptoRandomSource : RandomSource, IDisposable
    {
        const double Reciprocal = 1.0/uint.MaxValue;
        readonly RandomNumberGenerator _crypto;

        /// <summary>
        /// Construct a new random number generator with a random seed.
        /// </summary>
        /// <remarks>Uses <see cref="System.Security.Cryptography.RNGCryptoServiceProvider"/> and uses the value of 
        /// <see cref="Control.ThreadSafeRandomNumberGenerators"/> to set whether the instance is thread safe.</remarks>
        public CryptoRandomSource()
        {
            _crypto = new RNGCryptoServiceProvider();
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
        /// <remarks>Uses <see cref="System.Security.Cryptography.RNGCryptoServiceProvider"/></remarks>
        /// <param name="threadSafe">if set to <c>true</c> , the class is thread safe.</param>
        public CryptoRandomSource(bool threadSafe) : base(threadSafe)
        {
            _crypto = new RNGCryptoServiceProvider();
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
        /// Returns a random number between 0.0 and 1.0.
        /// </summary>
        /// <returns>
        /// A double-precision floating point number greater than or equal to 0.0, and less than 1.0.
        /// </returns>
        protected override sealed double DoSample()
        {
            var bytes = new byte[4];
            _crypto.GetBytes(bytes);
            return BitConverter.ToUInt32(bytes, 0)*Reciprocal;
        }

        public void Dispose()
        {
#if !NET35
            _crypto.Dispose();
#endif
        }

        /// <summary>
        /// Returns an array of random numbers greater than or equal to 0.0 and less than 1.0.
        /// </summary>
        public static double[] Doubles(int length)
        {
            var rnd = new RNGCryptoServiceProvider();
            var bytes = new byte[length*4];
            rnd.GetBytes(bytes);
            var data = new double[length];
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = BitConverter.ToUInt32(bytes, i*4)*Reciprocal;
            }
            return data;
        }

        /// <summary>
        /// Returns an infinite sequence of random numbers greater than or equal to 0.0 and less than 1.0.
        /// </summary>
        public static IEnumerable<double> DoubleSequence()
        {
            var rnd = new RNGCryptoServiceProvider();
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

#endif
