// <copyright file="Mcg59.cs" company="Math.NET">
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

namespace MathNet.Numerics.Random
{
    /// <summary>
    /// Multiplicative congruential generator using a modulus of 2^59 and a multiplier of 13^13.
    /// </summary>
    public class Mcg59 : RandomSource
    {
        const double Reciprocal = 1.0/Modulus;
        const ulong Modulus = 576460752303423488;
        const ulong Multiplier = 302875106592253;
        ulong _xn;

        /// <summary>
        /// Initializes a new instance of the <see cref="Mcg59"/> class using
        /// a seed based on time and unique GUIDs.
        /// </summary>
        public Mcg59() : this(RandomSeed.Guid())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Mcg59"/> class using
        /// a seed based on time and unique GUIDs.
        /// </summary>
        /// <param name="threadSafe">if set to <c>true</c> , the class is thread safe.</param>
        public Mcg59(bool threadSafe) : this(RandomSeed.Guid(), threadSafe)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Mcg59"/> class.
        /// </summary>
        /// <param name="seed">The seed value.</param>
        /// <remarks>If the seed value is zero, it is set to one. Uses the
        /// value of <see cref="Control.ThreadSafeRandomNumberGenerators"/> to
        /// set whether the instance is thread safe.</remarks>
        public Mcg59(int seed)
        {
            if (seed == 0)
            {
                seed = 1;
            }
            _xn = (uint)seed%Modulus;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Mcg59"/> class.
        /// </summary>
        /// <param name="seed">The seed value.</param>
        /// <remarks>The seed is set to 1, if the zero is used as the seed.</remarks>
        /// <param name="threadSafe">if set to <c>true</c> , the class is thread safe.</param>
        public Mcg59(int seed, bool threadSafe) : base(threadSafe)
        {
            if (seed == 0)
            {
                seed = 1;
            }
            _xn = (uint)seed%Modulus;
        }

        /// <summary>
        /// Returns a random number between 0.0 and 1.0.
        /// </summary>
        /// <returns>
        /// A double-precision floating point number greater than or equal to 0.0, and less than 1.0.
        /// </returns>
        protected override double DoSample()
        {
            double ret = _xn*Reciprocal;
            _xn = (_xn*Multiplier)%Modulus;
            return ret;
        }

        /// <summary>
        /// Returns an array of random numbers greater than or equal to 0.0 and less than 1.0.
        /// </summary>
        public static double[] Samples(int length, int seed)
        {
            if (seed == 0)
            {
                seed = 1;
            }
            ulong xn = (uint)seed%Modulus;

            var data = new double[length];
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = xn*Reciprocal;
                xn = (xn*Multiplier)%Modulus;
            }
            return data;
        }

        /// <summary>
        /// Returns an infinite sequence of random numbers greater than or equal to 0.0 and less than 1.0.
        /// </summary>
        public static IEnumerable<double> SampleSequence(int seed)
        {
            if (seed == 0)
            {
                seed = 1;
            }
            ulong xn = (uint)seed%Modulus;

            while (true)
            {
                yield return xn*Reciprocal;
                xn = (xn*Multiplier)%Modulus;
            }
        }
    }
}
