// <copyright file="MatchingNaiveTransformTest.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://mathnet.opensourcedotnet.info
//
// Copyright (c) 2009 Math.NET
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

namespace MathNet.Numerics.UnitTests.IntegralTransformsTests
{
    using System;
    using System.Numerics;
    using Distributions;
    using IntegralTransforms;
    using IntegralTransforms.Algorithms;
    using MbUnit.Framework;
    using Sampling;

    [TestFixture]
    public class MatchingNaiveTransformTest
    {
        private IContinuousDistribution _uniform = new ContinuousUniform(-1, 1);

        private static void VerifyMatchesNaiveComplex(
            Complex[] samples,
            double maximumError,
            Func<Complex[], Complex[]> naive,
            Action<Complex[]> fast)
        {
            var spectrumNaive = naive(samples);

            var spectrumFast = new Complex[samples.Length];
            samples.CopyTo(spectrumFast, 0);
            fast(spectrumFast);

            AssertHelpers.AlmostEqualList(spectrumNaive, spectrumFast, maximumError);
        }

        [Test]
        [Row(FourierOptions.Default)]
        [Row(FourierOptions.Matlab)]
        [Row(FourierOptions.NumericalRecipes)]
        public void FourierRadix2MatchesNaiveOnRealSine(FourierOptions options)
        {
            var dft = new DiscreteFourierTransform();
            var samples = Sample.EquidistantPeriodic(w => new Complex(Math.Sin(w), 0), Constants.Pi2, 0, 16);

            VerifyMatchesNaiveComplex(
                samples,
                1e-12,
                s => dft.NaiveForward(s, options),
                s => dft.Radix2Forward(s, options));

            VerifyMatchesNaiveComplex(
                samples,
                1e-12,
                s => dft.NaiveInverse(s, options),
                s => dft.Radix2Inverse(s, options));
        }

        [Test]
        [Row(FourierOptions.Default)]
        [Row(FourierOptions.Matlab)]
        [Row(FourierOptions.NumericalRecipes)]
        public void FourierRadix2MatchesNaiveOnRandom(FourierOptions options)
        {
            var dft = new DiscreteFourierTransform();
            var samples = Sample.Random((u, v) => new Complex(u, v), _uniform, 0x80);

            VerifyMatchesNaiveComplex(
                samples,
                1e-12,
                s => dft.NaiveForward(s, options),
                s => dft.Radix2Forward(s, options));

            VerifyMatchesNaiveComplex(
                samples,
                1e-12,
                s => dft.NaiveInverse(s, options),
                s => dft.Radix2Inverse(s, options));
        }

        [Test]
        [Row(FourierOptions.Default)]
        [Row(FourierOptions.Matlab)]
        [Row(FourierOptions.NumericalRecipes)]
        public void FourierBluesteinMatchesNaiveOnRealSineNonPowerOfTwo(FourierOptions options)
        {
            var dft = new DiscreteFourierTransform();
            var samples = Sample.EquidistantPeriodic(w => new Complex(Math.Sin(w), 0), Constants.Pi2, 0, 14);

            VerifyMatchesNaiveComplex(
                samples,
                1e-12,
                s => dft.NaiveForward(s, options),
                s => dft.BluesteinForward(s, options));

            VerifyMatchesNaiveComplex(
                samples,
                1e-12,
                s => dft.NaiveInverse(s, options),
                s => dft.BluesteinInverse(s, options));
        }

        [Test]
        [Row(FourierOptions.Default)]
        [Row(FourierOptions.Matlab)]
        [Row(FourierOptions.NumericalRecipes)]
        public void FourierBluesteinMatchesNaiveOnRandomPowerOfTwo(FourierOptions options)
        {
            var dft = new DiscreteFourierTransform();
            var samples = Sample.Random((u, v) => new Complex(u, v), _uniform, 0x80);

            VerifyMatchesNaiveComplex(
                samples,
                1e-12,
                s => dft.NaiveForward(s, options),
                s => dft.BluesteinForward(s, options));

            VerifyMatchesNaiveComplex(
                samples,
                1e-12,
                s => dft.NaiveInverse(s, options),
                s => dft.BluesteinInverse(s, options));
        }

        [Test]
        [Row(FourierOptions.Default)]
        [Row(FourierOptions.Matlab)]
        [Row(FourierOptions.NumericalRecipes)]
        public void FourierBluesteinMatchesNaiveOnRandomNonPowerOfTwo(FourierOptions options)
        {
            var dft = new DiscreteFourierTransform();
            var samples = Sample.Random((u, v) => new Complex(u, v), _uniform, 0x7F);

            VerifyMatchesNaiveComplex(
                samples,
                1e-12,
                s => dft.NaiveForward(s, options),
                s => dft.BluesteinForward(s, options));

            VerifyMatchesNaiveComplex(
                samples,
                1e-12,
                s => dft.NaiveInverse(s, options),
                s => dft.BluesteinInverse(s, options));
        }
    }
}