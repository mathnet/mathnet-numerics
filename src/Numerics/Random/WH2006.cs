// <copyright file="WH2006.cs" company="Math.NET">
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
    /// Wichmann-Hill’s 2006 combined multiplicative congruential generator.
    /// </summary>
    /// <remarks>See: Wichmann, B. A. &amp; Hill, I. D. (2006), "Generating good pseudo-random numbers".
    /// Computational Statistics &amp; Data Analysis 51:3 (2006) 1614-1622
    /// </remarks>
    [Serializable]
    [DataContract(Namespace = "urn:MathNet/Numerics/Random")]
    public class WH2006 : RandomSource
    {
        const uint Modw = 2147483123;
        const double ModwRecip = 1.0/Modw;
        const uint Modx = 2147483579;
        const double ModxRecip = 1.0/Modx;
        const uint Mody = 2147483543;
        const double ModyRecip = 1.0/Mody;
        const uint Modz = 2147483423;
        const double ModzRecip = 1.0/Modz;

        [DataMember(Order = 1)]
        ulong _wn = 1;
        [DataMember(Order = 2)]
        ulong _xn;
        [DataMember(Order = 3)]
        ulong _yn = 1;
        [DataMember(Order = 4)]
        ulong _zn = 1;

        /// <summary>
        /// Initializes a new instance of the <see cref="WH2006"/> class using
        /// a seed based on time and unique GUIDs.
        /// </summary>
        public WH2006() : this(RandomSeed.Robust())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WH2006"/> class using
        /// a seed based on time and unique GUIDs.
        /// </summary>
        /// <param name="threadSafe">if set to <c>true</c> , the class is thread safe.</param>
        public WH2006(bool threadSafe) : this(RandomSeed.Robust(), threadSafe)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WH2006"/> class.
        /// </summary>
        /// <param name="seed">The seed value.</param>
        /// <remarks>If the seed value is zero, it is set to one. Uses the
        /// value of <see cref="Control.ThreadSafeRandomNumberGenerators"/> to
        /// set whether the instance is thread safe.</remarks>
        public WH2006(int seed)
        {
            if (seed == 0)
            {
                seed = 1;
            }

            _xn = (uint)seed%Modx;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WH2006"/> class.
        /// </summary>
        /// <param name="seed">The seed value.</param>
        /// <remarks>The seed is set to 1, if the zero is used as the seed.</remarks>
        /// <param name="threadSafe">if set to <c>true</c> , the class is thread safe.</param>
        public WH2006(int seed, bool threadSafe) : base(threadSafe)
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
            _xn = 11600*_xn%Modx;
            _yn = 47003*_yn%Mody;
            _zn = 23000*_zn%Modz;
            _wn = 33000*_wn%Modw;

            double u = _xn*ModxRecip + _yn*ModyRecip + _zn*ModzRecip + _wn*ModwRecip;
            u -= (int)u;
            return u;
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

            ulong wn = 1;
            ulong xn = (uint)seed%Modx;
            ulong yn = 1;
            ulong zn = 1;

            for (int i = 0; i < values.Length; i++)
            {
                xn = 11600*xn%Modx;
                yn = 47003*yn%Mody;
                zn = 23000*zn%Modz;
                wn = 33000*wn%Modw;

                double u = xn*ModxRecip + yn*ModyRecip + zn*ModzRecip + wn*ModwRecip;
                values[i] = u - (int)u;
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

            ulong wn = 1;
            ulong xn = (uint)seed%Modx;
            ulong yn = 1;
            ulong zn = 1;

            while (true)
            {
                xn = 11600*xn%Modx;
                yn = 47003*yn%Mody;
                zn = 23000*zn%Modz;
                wn = 33000*wn%Modw;

                double u = xn*ModxRecip + yn*ModyRecip + zn*ModzRecip + wn*ModwRecip;
                yield return u - (int)u;
            }
        }
    }
}
