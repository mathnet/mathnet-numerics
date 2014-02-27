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

        /// <summary>
        /// Verify matches naive complex.
        /// </summary>
        static void VerifyMatchesNaiveComplex(
            Complex[] samples,
            int maximumErrorDecimalPlaces,
            Func<Complex[], Complex[]> naive,
            Action<Complex[]> fast)
        {
            var spectrumNaive = naive(samples);

            var spectrumFast = new Complex[samples.Length];
            samples.CopyTo(spectrumFast, 0);
            fast(spectrumFast);

            AssertHelpers.ListAlmostEqual(spectrumNaive, spectrumFast, maximumErrorDecimalPlaces);
        }

        /// <summary>
        /// Fourier Radix2XX matches naive on real sine.
        /// </summary>
        /// <param name="options">Fourier options.</param>
        [TestCase(FourierOptions.Default)]
        [TestCase(FourierOptions.Matlab)]
        [TestCase(FourierOptions.NumericalRecipes)]
        public void FourierRadix2MatchesNaiveOnRealSine(FourierOptions options)
        {
            var samples = Generate.PeriodicMap(16, w => new Complex(Math.Sin(w), 0), 16, 1.0, Constants.Pi2);

            VerifyMatchesNaiveComplex(
                samples,
                12,
                s => Fourier.NaiveForward(s, options),
                s => Fourier.Radix2Forward(s, options));

            VerifyMatchesNaiveComplex(
                samples,
                12,
                s => Fourier.NaiveInverse(s, options),
                s => Fourier.Radix2Inverse(s, options));
        }

        /// <summary>
        /// Fourier Radix2XX matches naive on random.
        /// </summary>
        /// <param name="options">Fourier options.</param>
        [TestCase(FourierOptions.Default)]
        [TestCase(FourierOptions.Matlab)]
        [TestCase(FourierOptions.NumericalRecipes)]
        public void FourierRadix2MatchesNaiveOnRandom(FourierOptions options)
        {
            var samples = Generate.RandomComplex(0x80, GetUniform(1));

            VerifyMatchesNaiveComplex(
                samples,
                10,
                s => Fourier.NaiveForward(s, options),
                s => Fourier.Radix2Forward(s, options));

            VerifyMatchesNaiveComplex(
                samples,
                10,
                s => Fourier.NaiveInverse(s, options),
                s => Fourier.Radix2Inverse(s, options));
        }

        /// <summary>
        /// Fourier bluestein matches naive on real sine non-power of two.
        /// </summary>
        /// <param name="options">Fourier options.</param>
        [TestCase(FourierOptions.Default)]
        [TestCase(FourierOptions.Matlab)]
        [TestCase(FourierOptions.NumericalRecipes)]
        public void FourierBluesteinMatchesNaiveOnRealSineNonPowerOfTwo(FourierOptions options)
        {
            var samples = Generate.PeriodicMap(14, w => new Complex(Math.Sin(w), 0), 14, 1.0, Constants.Pi2);

            VerifyMatchesNaiveComplex(
                samples,
                12,
                s => Fourier.NaiveForward(s, options),
                s => Fourier.BluesteinForward(s, options));

            VerifyMatchesNaiveComplex(
                samples,
                12,
                s => Fourier.NaiveInverse(s, options),
                s => Fourier.BluesteinInverse(s, options));
        }

        /// <summary>
        /// Fourier bluestein matches naive on random power of two.
        /// </summary>
        /// <param name="options">Fourier options.</param>
        [TestCase(FourierOptions.Default)]
        [TestCase(FourierOptions.Matlab)]
        [TestCase(FourierOptions.NumericalRecipes)]
        public void FourierBluesteinMatchesNaiveOnRandomPowerOfTwo(FourierOptions options)
        {
            var samples = Generate.RandomComplex(0x80, GetUniform(1));

            VerifyMatchesNaiveComplex(
                samples,
                10,
                s => Fourier.NaiveForward(s, options),
                s => Fourier.BluesteinForward(s, options));

            VerifyMatchesNaiveComplex(
                samples,
                10,
                s => Fourier.NaiveInverse(s, options),
                s => Fourier.BluesteinInverse(s, options));
        }

        /// <summary>
        /// Fourier bluestein matches naive on random non-power of two.
        /// </summary>
        /// <param name="options">Fourier options.</param>
        [TestCase(FourierOptions.Default)]
        [TestCase(FourierOptions.Matlab)]
        [TestCase(FourierOptions.NumericalRecipes)]
        public void FourierBluesteinMatchesNaiveOnRandomNonPowerOfTwo(FourierOptions options)
        {
            var samples = Generate.RandomComplex(0x7F, GetUniform(1));

            VerifyMatchesNaiveComplex(
                samples,
                10,
                s => Fourier.NaiveForward(s, options),
                s => Fourier.BluesteinForward(s, options));
            VerifyMatchesNaiveComplex(
                samples,
                10,
                s => Fourier.NaiveInverse(s, options),
                s => Fourier.BluesteinInverse(s, options));
        }
    }
}
