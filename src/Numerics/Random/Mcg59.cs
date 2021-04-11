// <copyright file="Mcg59.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
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
using System.Runtime.Serialization;
using System;
using System.Runtime;

namespace MathNet.Numerics.Random
{
    /// <summary>
    /// Multiplicative congruential generator using a modulus of 2^59 and a multiplier of 13^13.
    /// </summary>
    [Serializable]
    [DataContract(Namespace = "urn:MathNet/Numerics/Random")]
    public class Mcg59 : RandomSource
    {
        const ulong Modulus = 576460752303423488;
        const ulong Multiplier = 302875106592253;
        const double Reciprocal = 1.0/Modulus;

        [DataMember(Order = 1)]
        ulong _xn;

        /// <summary>
        /// Initializes a new instance of the <see cref="Mcg59"/> class using
        /// a seed based on time and unique GUIDs.
        /// </summary>
        public Mcg59() : this(RandomSeed.Robust())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Mcg59"/> class using
        /// a seed based on time and unique GUIDs.
        /// </summary>
        /// <param name="threadSafe">if set to <c>true</c> , the class is thread safe.</param>
        public Mcg59(bool threadSafe) : this(RandomSeed.Robust(), threadSafe)
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
        /// Returns a random double-precision floating point number greater than or equal to 0.0, and less than 1.0.
        /// </summary>
        protected sealed override double DoSample()
        {
            double ret = _xn*Reciprocal;
            _xn = (_xn*Multiplier)%Modulus;
            return ret;
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

            ulong xn = (uint)seed%Modulus;

            for (int i = 0; i < values.Length; i++)
            {
                values[i] = xn*Reciprocal;
                xn = (xn*Multiplier)%Modulus;
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

            ulong xn = (uint)seed%Modulus;

            while (true)
            {
                yield return xn*Reciprocal;
                xn = (xn*Multiplier)%Modulus;
            }
        }
    }
}
