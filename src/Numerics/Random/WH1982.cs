// <copyright file="WH1982.cs" company="Math.NET">
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
using System;
using System.Runtime;

namespace MathNet.Numerics.Random
{
    /// <summary>
    /// Wichmann-Hill’s 1982 combined multiplicative congruential generator.
    /// </summary>
    /// <remarks>See: Wichmann, B. A. &amp; Hill, I. D. (1982), "Algorithm AS 183:
    /// An efficient and portable pseudo-random number generator". Applied Statistics 31 (1982) 188-190
    /// </remarks>
    [Serializable]
    [DataContract(Namespace = "urn:MathNet/Numerics/Random")]
    public class WH1982 : RandomSource
    {
        const uint Modx = 30269;
        const double ModxRecip = 1.0/Modx;
        const uint Mody = 30307;
        const double ModyRecip = 1.0/Mody;
        const uint Modz = 30323;
        const double ModzRecip = 1.0/Modz;

        [DataMember(Order = 1)]
        uint _xn;
        [DataMember(Order = 2)]
        uint _yn = 1;
        [DataMember(Order = 3)]
        uint _zn = 1;

        /// <summary>
        /// Initializes a new instance of the <see cref="WH1982"/> class using
        /// a seed based on time and unique GUIDs.
        /// </summary>
        public WH1982() : this(RandomSeed.Robust())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WH1982"/> class using
        /// a seed based on time and unique GUIDs.
        /// </summary>
        /// <param name="threadSafe">if set to <c>true</c> , the class is thread safe.</param>
        public WH1982(bool threadSafe) : this(RandomSeed.Robust(), threadSafe)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WH1982"/> class.
        /// </summary>
        /// <param name="seed">The seed value.</param>
        /// <remarks>If the seed value is zero, it is set to one. Uses the
        /// value of <see cref="Control.ThreadSafeRandomNumberGenerators"/> to
        /// set whether the instance is thread safe.</remarks>
        public WH1982(int seed)
        {
            if (seed == 0)
            {
                seed = 1;
            }

            _xn = (uint)seed%Modx;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WH1982"/> class.
        /// </summary>
        /// <param name="seed">The seed value.</param>
        /// <remarks>The seed is set to 1, if the zero is used as the seed.</remarks>
        /// <param name="threadSafe">if set to <c>true</c> , the class is thread safe.</param>
        public WH1982(int seed, bool threadSafe)
            : base(threadSafe)
        {
            if (seed == 0)
            {
                seed = 1;
            }

            _xn = (uint)seed%Modx;
        }

        /// <summary>
        /// Returns a random double-precision floating point number greater than or equal to 0.0, and less than 1.0.
        /// </summary>
        protected sealed override double DoSample()
        {
            _xn = (171*_xn)%Modx;
            _yn = (172*_yn)%Mody;
            _zn = (170*_zn)%Modz;

            double w = _xn*ModxRecip + _yn*ModyRecip + _zn*ModzRecip;
            w -= (int)w;
            return w;
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

            uint xn = (uint)seed%Modx;
            uint yn = 1;
            uint zn = 1;

            for (int i = 0; i < values.Length; i++)
            {
                xn = (171*xn)%Modx;
                yn = (172*yn)%Mody;
                zn = (170*zn)%Modz;

                double w = xn*ModxRecip + yn*ModyRecip + zn*ModzRecip;
                values[i] = w - (int)w;
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

            uint xn = (uint)seed%Modx;
            uint yn = 1;
            uint zn = 1;

            while (true)
            {
                xn = (171*xn)%Modx;
                yn = (172*yn)%Mody;
                zn = (170*zn)%Modz;

                double w = xn*ModxRecip + yn*ModyRecip + zn*ModzRecip;
                yield return w - (int)w;
            }
        }
    }
}
