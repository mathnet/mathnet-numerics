// <copyright file="Generate.cs" company="Math.NET">
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
using System.Linq;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.Properties;
using MathNet.Numerics.Random;

namespace MathNet.Numerics
{
#if !NOSYSNUMERICS
    using System.Numerics;
#endif

    public static class Generate
    {
        /// <summary>
        /// Generate a linearly spaced sample vector of the given length between the specified values (inclusive).
        /// Equivalent to MATLAB linspace but with the length as first instead of last argument.
        /// </summary>
        public static double[] LinearSpaced(int length, double start, double stop)
        {
            if (length <= 0) return new double[0];
            if (length == 1) return new[] { stop };

            double step = (stop - start)/(length - 1);

            var data = new double[length];
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = start + i*step;
            }
            data[data.Length - 1] = stop;
            return data;
        }

        /// <summary>
        /// Generate a base 10 logarithmically spaced sample vector of the given length between the specified decade exponents (inclusive).
        /// Equivalent to MATLAB logspace but with the length as first instead of last argument.
        /// </summary>
        public static double[] LogSpaced(int length, double startExponent, double stopExponent)
        {
            if (length <= 0) return new double[0];
            if (length == 1) return new[] { Math.Pow(10, stopExponent) };

            double step = (stopExponent - startExponent)/(length - 1);

            var data = new double[length];
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = Math.Pow(10, startExponent + i*step);
            }
            data[data.Length - 1] = Math.Pow(10, stopExponent);
            return data;
        }

        /// <summary>
        /// Generate a linearly spaced sample vector within the inclusive interval (start, stop) and step 1.
        /// Equivalent to MATLAB colon operator (:).
        /// </summary>
        public static double[] LinearRange(int start, int stop)
        {
            if (start == stop) return new double[] { start };
            if (start < stop)
            {
                var data = new double[stop - start + 1];
                for (int i = 0; i < data.Length; i++)
                {
                    data[i] = start + i;
                }
                return data;
            }
            else
            {
                var data = new double[start - stop + 1];
                for (int i = 0; i < data.Length; i++)
                {
                    data[i] = start - i;
                }
                return data;
            }
        }

        /// <summary>
        /// Generate a linearly spaced sample vector within the inclusive interval (start, stop) and the provide step.
        /// The start value is aways included as first value, but stop is only included if it stop-start is a multiple of step.
        /// Equivalent to MATLAB double colon operator (::).
        /// </summary>
        public static double[] LinearRange(int start, int step, int stop)
        {
            if (start == stop) return new double[] { start };
            if (start < stop && step < 0 || start > stop && step > 0 || step == 0d)
            {
                return new double[0];
            }

            var data = new double[(stop - start)/step + 1];
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = start + i*step;
            }
            return data;
        }

        /// <summary>
        /// Generate a linearly spaced sample vector within the inclusive interval (start, stop) and the provide step.
        /// The start value is aways included as first value, but stop is only included if it stop-start is a multiple of step.
        /// Equivalent to MATLAB double colon operator (::).
        /// </summary>
        public static double[] LinearRange(double start, double step, double stop)
        {
            if (start == stop) return new double[] { start };
            if (start < stop && step < 0 || start > stop && step > 0 || step == 0d)
            {
                return new double[0];
            }

            var data = new double[(int)Math.Floor((stop - start)/step + 1d)];
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = start + i*step;
            }
            return data;
        }

        /// <summary>
        /// Create a Sine sample vector.
        /// </summary>
        /// <param name="length">The number of samples to generate.</param>
        /// <param name="samplingRate">Samples per unit.</param>
        /// <param name="frequency">Frequency in samples per unit.</param>
        /// <param name="amplitude">The maximal reached peak.</param>
        /// <param name="mean">The mean, or dc part, of the signal.</param>
        /// <param name="phase">Optional phase offset.</param>
        /// <param name="delay">Optional delay, relative to the phase.</param>
        public static double[] Sinusoidal(int length, double samplingRate, double frequency, double amplitude, double mean = 0.0, double phase = 0.0, int delay = 0)
        {
            double step = frequency/samplingRate*Constants.Pi2;
            phase = (phase - delay*step)%Constants.Pi2;

            var data = new double[length];
            for (int i = 0; i < length; i++)
            {
                data[i] = mean + amplitude*Math.Sin(phase + i*step);
            }
            return data;
        }

        /// <summary>
        /// Create an infinite Sine sample sequence.
        /// </summary>
        /// <param name="samplingRate">Samples per unit.</param>
        /// <param name="frequency">Frequency in samples per unit.</param>
        /// <param name="amplitude">The maximal reached peak.</param>
        /// <param name="mean">The mean, or dc part, of the signal.</param>
        /// <param name="phase">Optional phase offset.</param>
        /// <param name="delay">Optional delay, relative to the phase.</param>
        public static IEnumerable<double> SinusoidalSequence(double samplingRate, double frequency, double amplitude, double mean = 0.0, double phase = 0.0, int delay = 0)
        {
            double step = frequency/samplingRate*Constants.Pi2;
            phase = (phase - delay*step)%Constants.Pi2;

            while (true)
            {
                for (int i = 0; i < 1000; i++)
                {
                    yield return mean + amplitude*Math.Sin(phase + i*step);
                }
                phase = (phase + 1000*step)%Constants.Pi2;
            }
        }

        /// <summary>
        /// Create a Heaviside Step sample vector.
        /// </summary>
        /// <param name="length">The number of samples to generate.</param>
        /// <param name="amplitude">The maximal reached peak.</param>
        /// <param name="delay">Offset to the time axis.</param>
        public static double[] Step(int length, double amplitude, int delay)
        {
            var data = new double[length];
            for (int i = Math.Max(0, delay); i < data.Length; i++)
            {
                data[i] = amplitude;
            }
            return data;
        }

        /// <summary>
        /// Create an infinite Heaviside Step sample sequence.
        /// </summary>
        /// <param name="amplitude">The maximal reached peak.</param>
        /// <param name="delay">Offset to the time axis.</param>
        public static IEnumerable<double> StepSequence(double amplitude, int delay)
        {
            for (int i = 0; i < delay; i++)
            {
                yield return 0d;
            }

            while (true)
            {
                yield return amplitude;
            }
        }

        /// <summary>
        /// Create a Dirac Delta Impulse sample vector.
        /// </summary>
        /// <param name="length">The number of samples to generate.</param>
        /// <param name="period">impulse sequence period. -1 for single impulse only.</param>
        /// <param name="amplitude">The maximal reached peak.</param>
        /// <param name="delay">Offset to the time axis. Zero or positive.</param>
        public static double[] Impulse(int length, int period, double amplitude, int delay)
        {
            var data = new double[length];
            if (period <= 0)
            {
                if (delay >= 0 && delay < length)
                {
                    data[delay] = amplitude;
                }
            }
            else
            {
                delay = ((delay%period) + period)%period;
                while (delay < length)
                {
                    data[delay] = amplitude;
                    delay += period;
                }
            }
            return data;
        }


        /// <summary>
        /// Create a Dirac Delta Impulse sample vector.
        /// </summary>
        /// <param name="period">impulse sequence period. -1 for single impulse only.</param>
        /// <param name="amplitude">The maximal reached peak.</param>
        /// <param name="delay">Offset to the time axis. Zero or positive.</param>
        public static IEnumerable<double> ImpulseSequence(int period, double amplitude, int delay)
        {
            if (period <= 0)
            {
                for (int i = 0; i < delay; i++)
                {
                    yield return 0d;
                }

                yield return amplitude;

                while (true)
                {
                    yield return 0d;
                }
            }
            else
            {
                delay = ((delay%period) + period)%period;

                for (int i = 0; i < delay; i++)
                {
                    yield return 0d;
                }

                while (true)
                {
                    yield return amplitude;

                    for (int i = 1; i < period; i++)
                    {
                        yield return 0d;
                    }
                }
            }
        }

        /// <summary>
        /// Generate samples by sampling a function at the provided points.
        /// </summary>
        public static TR[] Map<T, TR>(T[] points, Func<T, TR> map)
        {
            var res = new TR[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                res[i] = map(points[i]);
            }
            return res;
        }

        /// <summary>
        /// Generate a sample sequence by sampling a function at the provided point sequence.
        /// </summary>
        public static IEnumerable<TR> MapSequence<T, TR>(IEnumerable<T> points, Func<T, TR> map)
        {
            return points.Select(map);
        }

        /// <summary>
        /// Generate samples by sampling a function at the provided points.
        /// </summary>
        public static TR[] Map2<TA, TB, TR>(TA[] pointsA, TB[] pointsB, Func<TA, TB, TR> map)
        {
            if (pointsA.Length != pointsB.Length)
            {
                throw new ArgumentException(Resources.ArgumentArraysSameLength, "pointsB");
            }

            var res = new TR[pointsA.Length];
            for (int i = 0; i < res.Length; i++)
            {
                res[i] = map(pointsA[i], pointsB[i]);
            }
            return res;
        }

        /// <summary>
        /// Generate a sample sequence by sampling a function at the provided point sequence.
        /// </summary>
        public static IEnumerable<TR> Map2Sequence<TA, TB, TR>(IEnumerable<TA> pointsA, IEnumerable<TB> pointsB, Func<TA, TB, TR> map)
        {
            return pointsA.Zip(pointsB, map);
        }

        /// <summary>
        /// Generate samples by sampling a function at samples from a probability distribution.
        /// </summary>
        public static T[] RandomMap<T>(int length, IContinuousDistribution distribution, Func<double, T> map)
        {
            var res = new T[length];
            for (int i = 0; i < res.Length; i++)
            {
                res[i] = map(distribution.Sample());
            }
            return res;
        }

        /// <summary>
        /// Generate a sample sequence by sampling a function at samples from a probability distribution.
        /// </summary>
        public static IEnumerable<T> RandomMapSequence<T>(IContinuousDistribution distribution, Func<double, T> map)
        {
            return distribution.Samples().Select(map);
        }

        /// <summary>
        /// Generate samples by sampling a function at sample pairs from a probability distribution.
        /// </summary>
        public static T[] RandomMap2<T>(int length, IContinuousDistribution distribution, Func<double, double, T> map)
        {
            var res = new T[length];
            for (int i = 0; i < res.Length; i++)
            {
                res[i] = map(distribution.Sample(), distribution.Sample());
            }
            return res;
        }

        /// <summary>
        /// Generate a sample sequence by sampling a function at sample pairs from a probability distribution.
        /// </summary>
        public static IEnumerable<T> RandomMap2Sequence<T>(IContinuousDistribution distribution, Func<double, double, T> map)
        {
            return distribution.Samples().Zip(distribution.Samples(), map);
        }

        /// <summary>
        /// Create random samples.
        /// </summary>
        public static double[] Random(int length, IContinuousDistribution distribution)
        {
            return distribution.Samples().Take(length).ToArray();
        }

        /// <summary>
        /// Create an infinite random sample sequence.
        /// </summary>
        public static IEnumerable<double> Random(IContinuousDistribution distribution)
        {
            return distribution.Samples();
        }

        /// <summary>
        /// Create random samples.
        /// </summary>
        public static Complex[] RandomComplex(int length, IContinuousDistribution distribution)
        {
            return RandomMap2(length, distribution, (r, i) => new Complex(r, i));
        }

        /// <summary>
        /// Create an infinite random sample sequence.
        /// </summary>
        public static IEnumerable<Complex> RandomComplex(IContinuousDistribution distribution)
        {
            return RandomMap2Sequence(distribution, (r, i) => new Complex(r, i));
        }

        /// <summary>
        /// Create samples with independent amplitudes of normal distribution and a flat spectral density.
        /// </summary>
        public static double[] WhiteGaussianNoise(int length, double mean, double standardDeviation)
        {
            return Normal.Samples(MersenneTwister.Default, mean, standardDeviation).Take(length).ToArray();
        }

        /// <summary>
        /// Create an infinite sample sequence with independent amplitudes of normal distribution and a flat spectral density.
        /// </summary>
        public static IEnumerable<double> WhiteGaussianNoiseSequence(double mean, double standardDeviation)
        {
            return Normal.Samples(MersenneTwister.Default, mean, standardDeviation);
        }

        /// <summary>
        /// Create skew alpha stable samples.
        /// </summary>
        /// <param name="length">The number of samples to generate.</param>
        /// <param name="alpha">Stability alpha-parameter of the stable distribution</param>
        /// <param name="beta">Skewness beta-parameter of the stable distribution</param>
        /// <param name="scale">Scale c-parameter of the stable distribution</param>
        /// <param name="location">Location mu-parameter of the stable distribution</param>
        public static double[] StableNoise(int length, double alpha, double beta, double scale, double location)
        {
            return Stable.Samples(MersenneTwister.Default, alpha, beta, scale, location).Take(length).ToArray();
        }

        /// <summary>
        /// Create skew alpha stable samples.
        /// </summary>
        /// <param name="alpha">Stability alpha-parameter of the stable distribution</param>
        /// <param name="beta">Skewness beta-parameter of the stable distribution</param>
        /// <param name="scale">Scale c-parameter of the stable distribution</param>
        /// <param name="location">Location mu-parameter of the stable distribution</param>
        public static IEnumerable<double> StableNoiseSequence(double alpha, double beta, double scale, double location)
        {
            return Stable.Samples(MersenneTwister.Default, alpha, beta, scale, location);
        }
    }
}
