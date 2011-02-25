// <copyright file="MatchingNaiveTransformTest.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
// Copyright (c) 2009-2010 Math.NET
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
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
    using NUnit.Framework;
    using Signals;

    /// <summary>
    /// Matching Naive transform tests.
    /// </summary>
    [TestFixture]
    public class MatchingNaiveTransformTest
    {
        /// <summary>
        /// Continuous uniform distribution.
        /// </summary>
        private readonly IContinuousDistribution _uniform = new ContinuousUniform(-1, 1);

        /// <summary>
        /// Verify matches naive complex.
        /// </summary>
        /// <param name="samples">Samples count.</param>
        /// <param name="maximumError">Maximum error.</param>
        /// <param name="naive">Naive transform.</param>
        /// <param name="fast">Fast delegate.</param>
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

        /// <summary>
        /// Fourier Radix2XX matches naive on real sine.
        /// </summary>
        /// <param name="options">Fourier options.</param>
        [Test]
        public void FourierRadix2MatchesNaiveOnRealSine([Values(FourierOptions.Default, FourierOptions.Matlab, FourierOptions.NumericalRecipes)] FourierOptions options)
        {
            var dft = new DiscreteFourierTransform();
            var samples = SignalGenerator.EquidistantPeriodic(w => new Complex(Math.Sin(w), 0), Constants.Pi2, 0, 16);

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

        /// <summary>
        /// Fourier Radix2XX matches naive on random.
        /// </summary>
        /// <param name="options">Fourier options.</param>
        [Test]
        public void FourierRadix2MatchesNaiveOnRandom([Values(FourierOptions.Default, FourierOptions.Matlab, FourierOptions.NumericalRecipes)] FourierOptions options)
        {
            var dft = new DiscreteFourierTransform();
            var samples = SignalGenerator.Random((u, v) => new Complex(u, v), _uniform, 0x80);

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

        /// <summary>
        /// Fourier bluestein matches naive on real sine non-power of two.
        /// </summary>
        /// <param name="options">Fourier options.</param>
        [Test]
        public void FourierBluesteinMatchesNaiveOnRealSineNonPowerOfTwo([Values(FourierOptions.Default, FourierOptions.Matlab, FourierOptions.NumericalRecipes)] FourierOptions options)
        {
            var dft = new DiscreteFourierTransform();
            var samples = SignalGenerator.EquidistantPeriodic(w => new Complex(Math.Sin(w), 0), Constants.Pi2, 0, 14);

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

        /// <summary>
        /// Fourier bluestein matches naive on random power of two.
        /// </summary>
        /// <param name="options">Fourier options.</param>
        [Test]
        public void FourierBluesteinMatchesNaiveOnRandomPowerOfTwo([Values(FourierOptions.Default, FourierOptions.Matlab, FourierOptions.NumericalRecipes)] FourierOptions options)
        {
            var dft = new DiscreteFourierTransform();
            var samples = SignalGenerator.Random((u, v) => new Complex(u, v), _uniform, 0x80);

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

        /// <summary>
        /// Fourier bluestein matches naive on random non-power of two.
        /// </summary>
        /// <param name="options">Fourier options.</param>
        [Test]
        public void FourierBluesteinMatchesNaiveOnRandomNonPowerOfTwo([Values(FourierOptions.Default, FourierOptions.Matlab, FourierOptions.NumericalRecipes)] FourierOptions options)
        {
            var dft = new DiscreteFourierTransform();
            var samples = SignalGenerator.Random((u, v) => new Complex(u, v), _uniform, 0x7F);

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
