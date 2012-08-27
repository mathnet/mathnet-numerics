// <copyright file="SystemCrypto.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
//
// Copyright (c) 2009-2012 Math.NET
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

#if !PORTABLE
namespace MathNet.Numerics.Random
{
    using System;
    using System.Security.Cryptography;

    /// <summary>
    /// A random number generator based on the <see cref="System.Security.Cryptography.RandomNumberGenerator"/> class in the .NET library.
    /// </summary>
    public class SystemCryptoRandomNumberGenerator : AbstractRandomNumberGenerator, IDisposable
    {
        private const double mReciprocal = 1.0 / uint.MaxValue;
        private readonly RandomNumberGenerator mRandom;

        /// <summary>
        /// Construct a new random number generator with a random seed.
        /// </summary>
        /// <remarks>Uses <see cref="System.Security.Cryptography.RNGCryptoServiceProvider"/> and uses the value of 
        /// <see cref="Control.ThreadSafeRandomNumberGenerators"/> to set whether the instance is thread safe.</remarks>
        public SystemCryptoRandomNumberGenerator(): this(new RNGCryptoServiceProvider(), Control.ThreadSafeRandomNumberGenerators)
        {
            mRandom = new RNGCryptoServiceProvider();
        }

        /// <summary>
        /// Construct a new random number generator with random seed.
        /// </summary>
        /// <param name="rng">The <see cref="RandomNumberGenerator"/> to use.</param>
        /// <remarks>Uses the value of  <see cref="Control.ThreadSafeRandomNumberGenerators"/> to set whether the instance is thread safe.</remarks>
        public SystemCryptoRandomNumberGenerator(RandomNumberGenerator rng) : this(rng, Control.ThreadSafeRandomNumberGenerators)
        {
        }

        /// <summary>
        /// Construct a new random number generator with random seed.
        /// </summary>
        /// <remarks>Uses <see cref="System.Security.Cryptography.RNGCryptoServiceProvider"/></remarks>
        /// <param name="threadSafe">if set to <c>true</c> , the class is thread safe.</param>
        public SystemCryptoRandomNumberGenerator(bool threadSafe): this(new RNGCryptoServiceProvider(), threadSafe)
        {
        }

        /// <summary>
        /// Construct a new random number generator with random seed.
        /// </summary>
        /// <param name="rng">The <see cref="RandomNumberGenerator"/> to use.</param>
        /// <param name="threadSafe">if set to <c>true</c> , the class is thread safe.</param>
        public SystemCryptoRandomNumberGenerator(RandomNumberGenerator rng, bool threadSafe) : base(threadSafe)
        {
            if (rng == null)
            {
                throw new ArgumentNullException("rng");
            }
            mRandom = rng;
        }


        /// <summary>
        /// Returns a random number between 0.0 and 1.0.
        /// </summary>
        /// <returns>
        /// A double-precision floating point number greater than or equal to 0.0, and less than 1.0.
        /// </returns>
        protected override double DoSample()
        {
            byte[] bytes = new byte[4];
            mRandom.GetBytes(bytes);
            return BitConverter.ToUInt32(bytes, 0) * mReciprocal;
        }

        public void Dispose()
        {
            mRandom.Dispose();
        }
    }
}
#endif
