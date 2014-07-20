// <copyright file="AbstractRandomNumberGenerator.cs" company="Math.NET">
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

using System;
using System.Collections.Generic;
using MathNet.Numerics.Properties;

namespace MathNet.Numerics.Random
{
    /// <summary>
    /// Base class for random number generators. This class introduces a layer between <see cref="System.Random"/>
    /// and the Math.Net Numerics random number generators to provide thread safety.
    /// When used directly it use the System.Random as random number source.
    /// </summary>
    public abstract class RandomSource : System.Random
    {
        readonly bool _threadSafe;
        readonly object _lock = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="RandomSource"/> class using
        /// the value of <see cref="Control.ThreadSafeRandomNumberGenerators"/> to set whether
        /// the instance is thread safe or not.
        /// </summary>
        protected RandomSource() : base(RandomSeed.Robust())
        {
            _threadSafe = Control.ThreadSafeRandomNumberGenerators;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RandomSource"/> class.
        /// </summary>
        /// <param name="threadSafe">if set to <c>true</c> , the class is thread safe.</param>
        /// <remarks>Thread safe instances are two and half times slower than non-thread
        /// safe classes.</remarks>
        protected RandomSource(bool threadSafe) : base(RandomSeed.Robust())
        {
            _threadSafe = threadSafe;
        }

        /// <summary>
        /// Fills an array with uniform random numbers greater than or equal to 0.0 and less than 1.0.
        /// </summary>
        /// <param name="values">The array to fill with random values.</param>
        public void NextDoubles(double[] values)
        {
            if (_threadSafe)
            {
                lock (_lock)
                {
                    for (var i = 0; i < values.Length; i++)
                    {
                        values[i] = DoSample();
                    }
                }
            }
            else
            {
                for (var i = 0; i < values.Length; i++)
                {
                    values[i] = DoSample();
                }
            }
        }

        /// <summary>
        /// Returns an infinite sequence of uniform random numbers greater than or equal to 0.0 and less than 1.0.
        /// </summary>
        public IEnumerable<double> NextDoubleSequence()
        {
            for (int i = 0; i < 64; i++)
            {
                yield return NextDouble();
            }

            var buffer = new double[64];
            while (true)
            {
                NextDoubles(buffer);
                for (int i = 0; i < buffer.Length; i++)
                {
                    yield return buffer[i];
                }
            }
        }

        /// <summary>
        /// Returns a nonnegative random number.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer greater than or equal to zero and less than <see cref="F:System.Int32.MaxValue"/>.
        /// </returns>
        public override sealed int Next()
        {
            if (_threadSafe)
            {
                lock (_lock)
                {
                    return (int)(DoSample()*int.MaxValue);
                }
            }

            return (int)(DoSample()*int.MaxValue);
        }

        /// <summary>
        /// Returns a random number less then a specified maximum.
        /// </summary>
        /// <param name="maxValue">The exclusive upper bound of the random number returned.</param>
        /// <returns>A 32-bit signed integer less than <paramref name="maxValue"/>.</returns>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="maxValue"/> is negative. </exception>
        public override sealed int Next(int maxValue)
        {
            if (maxValue <= 0)
            {
                throw new ArgumentException(Resources.ArgumentMustBePositive);
            }

            if (_threadSafe)
            {
                lock (_lock)
                {
                    return (int)(DoSample()*maxValue);
                }
            }

            return (int)(DoSample()*maxValue);
        }

        /// <summary>
        /// Returns a random number within a specified range.
        /// </summary>
        /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
        /// <param name="maxValue">The exclusive upper bound of the random number returned. <paramref name="maxValue"/> must be greater than or equal to <paramref name="minValue"/>.</param>
        /// <returns>
        /// A 32-bit signed integer greater than or equal to <paramref name="minValue"/> and less than <paramref name="maxValue"/>; that is, the range of return values includes <paramref name="minValue"/> but not <paramref name="maxValue"/>. If <paramref name="minValue"/> equals <paramref name="maxValue"/>, <paramref name="minValue"/> is returned.
        /// </returns>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="minValue"/> is greater than <paramref name="maxValue"/>. </exception>
        public override sealed int Next(int minValue, int maxValue)
        {
            if (minValue > maxValue)
            {
                throw new ArgumentException(Resources.ArgumentMinValueGreaterThanMaxValue);
            }

            if (_threadSafe)
            {
                lock (_lock)
                {
                    return (int)(DoSample()*(maxValue - minValue)) + minValue;
                }
            }

            return (int)(DoSample()*(maxValue - minValue)) + minValue;
        }

        /// <summary>
        /// Fills an array with random numbers within a specified range.
        /// </summary>
        /// <param name="values">The array to fill with random values.</param>
        /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
        /// <param name="maxValue">The exclusive upper bound of the random number returned. <paramref name="maxValue"/> must be greater than or equal to <paramref name="minValue"/>.</param>
        public void NextInt32s(int[] values, int minValue, int maxValue)
        {
            if (_threadSafe)
            {
                lock (_lock)
                {
                    for (var i = 0; i < values.Length; i++)
                    {
                        values[i] = (int)(DoSample()*(maxValue - minValue)) + minValue;
                    }
                }
            }
            else
            {
                for (var i = 0; i < values.Length; i++)
                {
                    values[i] = (int)(DoSample()*(maxValue - minValue)) + minValue;
                }
            }
        }

        /// <summary>
        /// Returns an infinite sequence of random numbers within a specified range.
        /// </summary>
        /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
        /// <param name="maxValue">The exclusive upper bound of the random number returned. <paramref name="maxValue"/> must be greater than or equal to <paramref name="minValue"/>.</param>
        public IEnumerable<int> NextInt32Sequence(int minValue, int maxValue)
        {
            for (int i = 0; i < 64; i++)
            {
                yield return Next(minValue, maxValue);
            }

            var buffer = new int[64];
            while (true)
            {
                NextInt32s(buffer, minValue, maxValue);
                for (int i = 0; i < buffer.Length; i++)
                {
                    yield return buffer[i];
                }
            }
        }

        /// <summary>
        /// Fills the elements of a specified array of bytes with random numbers.
        /// </summary>
        /// <param name="buffer">An array of bytes to contain random numbers.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="buffer"/> is null. </exception>
        public override void NextBytes(byte[] buffer)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }

            if (_threadSafe)
            {
                lock (_lock)
                {
                    for (var i = 0; i < buffer.Length; i++)
                    {
                        buffer[i] = (byte)(((int)(DoSample()*int.MaxValue))%256);
                    }
                }

                return;
            }

            for (var i = 0; i < buffer.Length; i++)
            {
                buffer[i] = (byte)(((int)(DoSample()*int.MaxValue))%256);
            }
        }

        /// <summary>
        /// Returns a random number between 0.0 and 1.0.
        /// </summary>
        /// <returns>A double-precision floating point number greater than or equal to 0.0, and less than 1.0.</returns>
        protected override sealed double Sample()
        {
            if (_threadSafe)
            {
                lock (_lock)
                {
                    return DoSample();
                }
            }

            return DoSample();
        }

        /// <summary>
        /// Returns a random number between 0.0 and 1.0.
        /// </summary>
        /// <returns>
        /// A double-precision floating point number greater than or equal to 0.0, and less than 1.0.
        /// </returns>
        protected abstract double DoSample();
    }
}
