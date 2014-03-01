// <copyright file="MatchingNaiveTransformTest.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
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
using MathNet.Numerics.Distributions;
using MathNet.Numerics.IntegralTransforms;
using NUnit.Framework;

namespace MathNet.Numerics.UnitTests.IntegralTransformsTests
{

#if !NOSYSNUMERICS
    using System.Numerics;
#endif

    /// <summary>
    /// Matching Naive transform tests.
    /// </summary>
    [TestFixture, Category("FFT")]
    public class MatchingNaiveTransformTest
    {
        /// <summary>
        /// Continuous uniform distribution.
        /// </summary>
        IContinuousDistribution GetUniform(int seed)
        {
            return new ContinuousUniform(-1, 1, new System.Random(seed));
        }

        static void Verify(
            Complex[] samples,
            int maximumErrorDecimalPlaces,
            FourierOptions options,
            Func<Complex[], FourierOptions, Complex[]> naive,
            Action<Complex[], FourierOptions> fast)
        {
            var spectrumNaive = naive(samples, options);

            var spectrumFast = new Complex[samples.Length];
            samples.CopyTo(spectrumFast, 0);
            fast(spectrumFast, options);

            AssertHelpers.AlmostEqual(spectrumNaive, spectrumFast, maximumErrorDecimalPlaces);
        }

        /// <summary>
        /// Fourier Radix2XX matches naive on real sine.
        /// </summary>
        /// <param name="options">Fourier options.</param>
        [TestCase(FourierOptions.Default)]
        [TestCase(FourierOptions.Matlab)]
        [TestCase(FourierOptions.NumericalRecipes)]
        public void FourierRadix2MatchesNaive_RealSine(FourierOptions options)
        {
            var samples = Generate.PeriodicMap(16, w => new Complex(Math.Sin(w), 0), 16, 1.0, Constants.Pi2);

            Verify(samples, 12, options, Fourier.NaiveForward, Fourier.Radix2Forward);
            Verify(samples, 12, options, Fourier.NaiveInverse, Fourier.Radix2Inverse);
        }

        /// <summary>
        /// Fourier Radix2XX matches naive on random.
        /// </summary>
        /// <param name="options">Fourier options.</param>
        [TestCase(FourierOptions.Default)]
        [TestCase(FourierOptions.Matlab)]
        [TestCase(FourierOptions.NumericalRecipes)]
        public void FourierRadix2MatchesNaive_Random(FourierOptions options)
        {
            var samples = Generate.RandomComplex(0x80, GetUniform(1));

            Verify(samples, 10, options, Fourier.NaiveForward, Fourier.Radix2Forward);
            Verify(samples, 10, options, Fourier.NaiveInverse, Fourier.Radix2Inverse);
        }

        /// <summary>
        /// Fourier bluestein matches naive on real sine non-power of two.
        /// </summary>
        /// <param name="options">Fourier options.</param>
        [TestCase(FourierOptions.Default)]
        [TestCase(FourierOptions.Matlab)]
        [TestCase(FourierOptions.NumericalRecipes)]
        public void FourierBluesteinMatchesNaive_RealSine_Arbitrary(FourierOptions options)
        {
            var samples = Generate.PeriodicMap(14, w => new Complex(Math.Sin(w), 0), 14, 1.0, Constants.Pi2);

            Verify(samples, 12, options, Fourier.NaiveForward, Fourier.BluesteinForward);
            Verify(samples, 12, options, Fourier.NaiveInverse, Fourier.BluesteinInverse);
        }

        /// <summary>
        /// Fourier bluestein matches naive on random power of two.
        /// </summary>
        /// <param name="options">Fourier options.</param>
        [TestCase(FourierOptions.Default)]
        [TestCase(FourierOptions.Matlab)]
        [TestCase(FourierOptions.NumericalRecipes)]
        public void FourierBluesteinMatchesNaive_Random_PowerOfTwo(FourierOptions options)
        {
            var samples = Generate.RandomComplex(0x80, GetUniform(1));

            Verify(samples, 10, options, Fourier.NaiveForward, Fourier.BluesteinForward);
            Verify(samples, 10, options, Fourier.NaiveInverse, Fourier.BluesteinInverse);
        }

        /// <summary>
        /// Fourier bluestein matches naive on random non-power of two.
        /// </summary>
        /// <param name="options">Fourier options.</param>
        [TestCase(FourierOptions.Default)]
        [TestCase(FourierOptions.Matlab)]
        [TestCase(FourierOptions.NumericalRecipes)]
        public void FourierBluesteinMatchesNaive_Random_Arbitrary(FourierOptions options)
        {
            var samples = Generate.RandomComplex(0x7F, GetUniform(1));

            Verify(samples, 10, options, Fourier.NaiveForward, Fourier.BluesteinForward);
            Verify(samples, 10, options, Fourier.NaiveInverse, Fourier.BluesteinInverse);
        }

        [Test, Explicit("Long-Running")]
        public void AlgorithmsMatchNaive_PowerOfTwo_Large()
        {
            // 65536 = 2^16
            const FourierOptions options = FourierOptions.NoScaling;
            var samples = Generate.RandomComplex(65536, GetUniform(1));
            var naive = Fourier.NaiveForward(samples, options);

            Verify(samples, 10, options, (a, b) => naive, Fourier.Radix2Forward);
            Verify(samples, 10, options, (a, b) => naive, Fourier.BluesteinForward);
        }

        [Test, Explicit("Long-Running")]
        public void AlgorithmsMatchNaive_Arbitrary_Large()
        {
            // 30870 = 2*3*3*5*7*7*7
            const FourierOptions options = FourierOptions.NoScaling;
            var samples = Generate.RandomComplex(30870, GetUniform(1));
            var naive = Fourier.NaiveForward(samples, options);

            Verify(samples, 10, options, (a, b) => naive, Fourier.BluesteinForward);
        }
    }
}
