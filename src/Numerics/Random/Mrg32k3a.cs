// <copyright file="Mrg32k3a.cs" company="Math.NET">
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
    /// A 32-bit combined multiple recursive generator with 2 components of order 3.
    /// </summary>
    /// <remarks>Based off of P. L'Ecuyer, "Combined Multiple Recursive Random Number Generators," Operations Research, 44, 5 (1996), 816--822. </remarks>
    ///
    [Serializable]
    [DataContract(Namespace = "urn:MathNet/Numerics/Random")]
    public class Mrg32k3a : RandomSource
    {
        const double A12 = 1403580;
        const double A13 = 810728;
        const double A21 = 527612;
        const double A23 = 1370589;
        const double Modulus1 = 4294967087;
        const double Modulus2 = 4294944443;

        const double Reciprocal = 1.0/Modulus1;

        [DataMember(Order = 1)]
        double _xn1 = 1;
        [DataMember(Order = 2)]
        double _xn2 = 1;
        [DataMember(Order = 3)]
        double _xn3;
        [DataMember(Order = 4)]
        double _yn1 = 1;
        [DataMember(Order = 5)]
        double _yn2 = 1;
        [DataMember(Order = 6)]
        double _yn3 = 1;

        /// <summary>
        /// Initializes a new instance of the <see cref="Mcg31m1"/> class using
        /// a seed based on time and unique GUIDs.
        /// </summary>
        /// <remarks>If the seed value is zero, it is set to one. Uses the
        /// value of <see cref="Control.ThreadSafeRandomNumberGenerators"/> to
        /// set whether the instance is thread safe.</remarks>
        public Mrg32k3a() : this(RandomSeed.Robust())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Mcg31m1"/> class using
        /// a seed based on time and unique GUIDs.
        /// </summary>
        /// <param name="threadSafe">if set to <c>true</c> , the class is thread safe.</param>
        public Mrg32k3a(bool threadSafe) : this(RandomSeed.Robust(), threadSafe)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Mrg32k3a"/> class.
        /// </summary>
        /// <param name="seed">The seed value.</param>
        /// <remarks>If the seed value is zero, it is set to one. Uses the
        /// value of <see cref="Control.ThreadSafeRandomNumberGenerators"/> to
        /// set whether the instance is thread safe.</remarks>
        public Mrg32k3a(int seed)
        {
            if (seed == 0)
            {
                seed = 1;
            }

            _xn3 = (uint)seed;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Mrg32k3a"/> class.
        /// </summary>
        /// <param name="seed">The seed value.</param>
        /// <param name="threadSafe">if set to <c>true</c>, the class is thread safe.</param>
        public Mrg32k3a(int seed, bool threadSafe) : base(threadSafe)
        {
            if (seed == 0)
            {
                seed = 1;
            }

            _xn3 = (uint)seed;
        }

        /// <summary>
        /// Returns a random double-precision floating point number greater than or equal to 0.0, and less than 1.0.
        /// </summary>
        protected sealed override double DoSample()
        {
            double xn = A12*_xn2 - A13*_xn3;
            double k = (long)(xn/Modulus1);
            xn -= k*Modulus1;
            if (xn < 0)
            {
                xn += Modulus1;
            }

            double yn = A21*_yn1 - A23*_yn3;
            k = (long)(yn/Modulus2);
            yn -= k*Modulus2;
            if (yn < 0)
            {
                yn += Modulus2;
            }

            _xn3 = _xn2;
            _xn2 = _xn1;
            _xn1 = xn;
            _yn3 = _yn2;
            _yn2 = _yn1;
            _yn1 = yn;

            if (xn <= yn)
            {
                return (xn - yn + Modulus1)*Reciprocal;
            }

            return (xn - yn)*Reciprocal;
        }

        /// <summary>
        /// Fills an array with random numbers greater than or equal to 0.0 and less than 1.0.
        /// </summary>
        /// <remarks>Supports being called in parallel from multiple threads.</remarks>
        public static void Doubles(double[] values, int seed)
        {
            double x1 = 1;
            double x2 = 1;
            double x3 = (uint)seed;
            double y1 = 1;
            double y2 = 1;
            double y3 = 1;

            for (int i = 0; i < values.Length; i++)
            {
                double xn = A12*x2 - A13*x3;
                double k = (long)(xn/Modulus1);
                xn -= k*Modulus1;
                if (xn < 0)
                {
                    xn += Modulus1;
                }

                double yn = A21*y1 - A23*y3;
                k = (long)(yn/Modulus2);
                yn -= k*Modulus2;
                if (yn < 0)
                {
                    yn += Modulus2;
                }

                x3 = x2;
                x2 = x1;
                x1 = xn;
                y3 = y2;
                y2 = y1;
                y1 = yn;

                values[i] = xn <= yn ? (xn - yn + Modulus1)*Reciprocal : (xn - yn)*Reciprocal;
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
            double x1 = 1;
            double x2 = 1;
            double x3 = (uint)seed;
            double y1 = 1;
            double y2 = 1;
            double y3 = 1;

            while (true)
            {
                double xn = A12*x2 - A13*x3;
                double k = (long)(xn/Modulus1);
                xn -= k*Modulus1;
                if (xn < 0)
                {
                    xn += Modulus1;
                }

                double yn = A21*y1 - A23*y3;
                k = (long)(yn/Modulus2);
                yn -= k*Modulus2;
                if (yn < 0)
                {
                    yn += Modulus2;
                }

                x3 = x2;
                x2 = x1;
                x1 = xn;
                y3 = y2;
                y2 = y1;
                y1 = yn;

                yield return xn <= yn ? (xn - yn + Modulus1)*Reciprocal : (xn - yn)*Reciprocal;
            }
        }
    }
}
