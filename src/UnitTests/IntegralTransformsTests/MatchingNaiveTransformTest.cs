// <copyright file="MatchingNaiveTransformTest.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2016 Math.NET
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
using MathNet.Numerics.Providers.FourierTransform;
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
            Complex32[] samples,
            int maximumErrorDecimalPlaces,
            FourierOptions options,
            Func<Complex32[], FourierOptions, Complex32[]> naive,
            Action<Complex32[], FourierOptions> fast)
        {
            var spectrumNaive = naive(samples, options);

            var spectrumFast = new Complex32[samples.Length];
            samples.CopyTo(spectrumFast, 0);
            fast(spectrumFast, options);

            AssertHelpers.AlmostEqual(spectrumNaive, spectrumFast, maximumErrorDecimalPlaces);
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

        static void VerifyInplace(
            Complex32[] samples,
            int maximumErrorDecimalPlaces,
            FourierOptions options,
            Action<Complex32[], FourierOptions> expected,
            Action<Complex32[], FourierOptions> actual)
        {
            var spectrumExpected = new Complex32[samples.Length];
            samples.CopyTo(spectrumExpected, 0);
            expected(spectrumExpected, options);

            var spectrumActual = new Complex32[samples.Length];
            samples.CopyTo(spectrumActual, 0);
            actual(spectrumActual, options);

            AssertHelpers.AlmostEqual(spectrumExpected, spectrumActual, maximumErrorDecimalPlaces);
        }

        static void VerifyInplace(
            Complex[] samples,
            int maximumErrorDecimalPlaces,
            FourierOptions options,
            Action<Complex[], FourierOptions> expected,
            Action<Complex[], FourierOptions> actual)
        {
            var spectrumExpected = new Complex[samples.Length];
            samples.CopyTo(spectrumExpected, 0);
            expected(spectrumExpected, options);

            var spectrumActual = new Complex[samples.Length];
            samples.CopyTo(spectrumActual, 0);
            actual(spectrumActual, options);

            AssertHelpers.AlmostEqual(spectrumExpected, spectrumActual, maximumErrorDecimalPlaces);
        }

        /// <summary>
        /// Fourier Radix2XX matches naive on real sine.
        /// </summary>
        /// <param name="options">Fourier options.</param>
        [TestCase(FourierOptions.Default)]
        [TestCase(FourierOptions.Matlab)]
        [TestCase(FourierOptions.NumericalRecipes)]
        public void FourierRadix2MatchesNaive_RealSine_32(FourierOptions options)
        {
            var samples = Generate.PeriodicMap(16, w => new Complex32((float)Math.Sin(w), 0), 16, 1.0, Constants.Pi2);

            Verify(samples, 6, options, Fourier.NaiveForward, Fourier.Radix2Forward);
            Verify(samples, 6, options, Fourier.NaiveInverse, Fourier.Radix2Inverse);
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
        public void FourierRadix2MatchesNaive_Random_32(FourierOptions options)
        {
            var samples = Generate.RandomComplex32(0x80, GetUniform(1));

            Verify(samples, 5, options, Fourier.NaiveForward, Fourier.Radix2Forward);
            Verify(samples, 5, options, Fourier.NaiveInverse, Fourier.Radix2Inverse);
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
        public void FourierBluesteinMatchesNaive_RealSine_Arbitrary_32(FourierOptions options)
        {
            var samples = Generate.PeriodicMap(14, w => new Complex32((float)Math.Sin(w), 0), 14, 1.0, Constants.Pi2);

            Verify(samples, 6, options, Fourier.NaiveForward, Fourier.BluesteinForward);
            Verify(samples, 6, options, Fourier.NaiveInverse, Fourier.BluesteinInverse);
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
        public void FourierBluesteinMatchesNaive_Random_PowerOfTwo_32(FourierOptions options)
        {
            var samples = Generate.RandomComplex32(0x80, GetUniform(1));

            Verify(samples, 5, options, Fourier.NaiveForward, Fourier.BluesteinForward);
            Verify(samples, 5, options, Fourier.NaiveInverse, Fourier.BluesteinInverse);
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
        public void FourierBluesteinMatchesNaive_Random_Arbitrary_32(FourierOptions options)
        {
            var samples = Generate.RandomComplex32(0x7F, GetUniform(1));

            Verify(samples, 5, options, Fourier.NaiveForward, Fourier.BluesteinForward);
            Verify(samples, 5, options, Fourier.NaiveInverse, Fourier.BluesteinInverse);
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

        /// <summary>
        /// Fourier bluestein matches providers on random power of two.
        /// </summary>
        /// <param name="options">Fourier options.</param>
        [TestCase(FourierOptions.Default)]
        [TestCase(FourierOptions.NoScaling)]
        [TestCase(FourierOptions.AsymmetricScaling)]
        [TestCase(FourierOptions.InverseExponent)]
        [TestCase(FourierOptions.InverseExponent | FourierOptions.NoScaling)]
        [TestCase(FourierOptions.InverseExponent | FourierOptions.AsymmetricScaling)]
        public void FourierBluesteinMatchesProvider_Random_Arbitrary_32(FourierOptions options)
        {
            var samples = Generate.RandomComplex32(0x7F, GetUniform(1));

            VerifyInplace(samples, 5, options, Fourier.Forward, Fourier.BluesteinForward);
            VerifyInplace(samples, 5, options, Fourier.Inverse, Fourier.BluesteinInverse);
        }

        /// <summary>
        /// Fourier bluestein matches providers on random power of two.
        /// </summary>
        /// <param name="options">Fourier options.</param>
        [TestCase(FourierOptions.Default)]
        [TestCase(FourierOptions.NoScaling)]
        [TestCase(FourierOptions.AsymmetricScaling)]
        [TestCase(FourierOptions.InverseExponent)]
        [TestCase(FourierOptions.InverseExponent | FourierOptions.NoScaling)]
        [TestCase(FourierOptions.InverseExponent | FourierOptions.AsymmetricScaling)]
        public void FourierBluesteinMatchesProvider_Random_Arbitrary(FourierOptions options)
        {
            var samples = Generate.RandomComplex(0x7F, GetUniform(1));

            VerifyInplace(samples, 10, options, Fourier.Forward, Fourier.BluesteinForward);
            VerifyInplace(samples, 10, options, Fourier.Inverse, Fourier.BluesteinInverse);
        }

        [TestCase(FourierOptions.Default, 128)]
        [TestCase(FourierOptions.Default, 129)]
        [TestCase(FourierOptions.NoScaling, 128)]
        [TestCase(FourierOptions.NoScaling, 129)]
        [TestCase(FourierOptions.AsymmetricScaling, 128)]
        [TestCase(FourierOptions.AsymmetricScaling, 129)]
        public void RealMatchesComplex32(FourierOptions options, int n)
        {
            var real = Generate.RandomSingle(n.IsEven() ? n + 2 : n + 1, GetUniform(1));
            real[n] = 0f;
            if (n.IsEven()) real[n + 1] = 0f;
            var complex = new Complex32[n];
            for (int i = 0; i < complex.Length; i++)
            {
                complex[i] = new Complex32(real[i], 0f);
            }

            Fourier.Forward(complex, options);
            Fourier.ForwardReal(real, n, options);

            int m = (n + 1) / 2;
            for (int i = 0, j = 0; i < m; i++)
            {
                AssertHelpers.AlmostEqual(complex[i], new Complex32(real[j++], real[j++]), 5);
            }
        }

        [TestCase(FourierOptions.Default, 128)]
        [TestCase(FourierOptions.Default, 129)]
        [TestCase(FourierOptions.NoScaling, 128)]
        [TestCase(FourierOptions.NoScaling, 129)]
        [TestCase(FourierOptions.AsymmetricScaling, 128)]
        [TestCase(FourierOptions.AsymmetricScaling, 129)]
        public void RealMatchesComplex(FourierOptions options, int n)
        {
            var real = Generate.Random(n.IsEven() ? n + 2 : n + 1, GetUniform(1));
            real[n] = 0d;
            if (n.IsEven()) real[n+1] = 0d;
            var complex = new Complex[n];
            for (int i = 0; i < complex.Length; i++)
            {
                complex[i] = new Complex(real[i], 0d);
            }

            Fourier.Forward(complex, options);
            Fourier.ForwardReal(real, n, options);

            int m = (n + 1)/2;
            for (int i = 0, j = 0; i < m; i++)
            {
                AssertHelpers.AlmostEqual(complex[i], new Complex(real[j++], real[j++]), 10);
            }
        }

        [Test]
        public void AlgorithmsMatchProvider_PowerOfTwo_Large_32()
        {
            // 65536 = 2^16
            var samples = Generate.RandomComplex32(65536, GetUniform(1));

            VerifyInplace(samples, 5, FourierOptions.NoScaling, (s, o) => Control.FourierTransformProvider.Forward(s, FourierTransformScaling.NoScaling), Fourier.Radix2Forward);
            VerifyInplace(samples, 5, FourierOptions.NoScaling, (s, o) => Control.FourierTransformProvider.Forward(s, FourierTransformScaling.NoScaling), Fourier.BluesteinForward);
        }

        [Test]
        public void AlgorithmsMatchProvider_PowerOfTwo_Large()
        {
            // 65536 = 2^16
            var samples = Generate.RandomComplex(65536, GetUniform(1));

            VerifyInplace(samples, 10, FourierOptions.NoScaling, (s,o) => Control.FourierTransformProvider.Forward(s, FourierTransformScaling.NoScaling), Fourier.Radix2Forward);
            VerifyInplace(samples, 10, FourierOptions.NoScaling, (s, o) => Control.FourierTransformProvider.Forward(s, FourierTransformScaling.NoScaling), Fourier.BluesteinForward);
        }

        [Test]
        public void AlgorithmsMatchProvider_Arbitrary_Large_32()
        {
            // 30870 = 2*3*3*5*7*7*7
            const FourierOptions options = FourierOptions.NoScaling;
            var samples = Generate.RandomComplex32(30870, GetUniform(1));

            var provider = new Complex32[samples.Length];
            samples.Copy(provider);
            Control.FourierTransformProvider.Forward(provider, FourierTransformScaling.NoScaling);

            Verify(samples, 5, options, (a, b) => provider, Fourier.BluesteinForward);
        }

        [Test]
        public void AlgorithmsMatchProvider_Arbitrary_Large()
        {
            // 30870 = 2*3*3*5*7*7*7
            const FourierOptions options = FourierOptions.NoScaling;
            var samples = Generate.RandomComplex(30870, GetUniform(1));

            var provider = new Complex[samples.Length];
            samples.Copy(provider);
            Control.FourierTransformProvider.Forward(provider, FourierTransformScaling.NoScaling);

            Verify(samples, 10, options, (a, b) => provider, Fourier.BluesteinForward);
        }

        [Test]
        public void AlgorithmsMatchProvider_Arbitrary_Large_GH286_32()
        {
            const FourierOptions options = FourierOptions.NoScaling;
            var samples = Generate.RandomComplex32(46500, GetUniform(1));

            var provider = new Complex32[samples.Length];
            samples.Copy(provider);
            Control.FourierTransformProvider.Forward(provider, FourierTransformScaling.NoScaling);

            Verify(samples, 5, options, (a, b) => provider, Fourier.BluesteinForward);
        }

        [Test]
        public void AlgorithmsMatchProvider_Arbitrary_Large_GH286()
        {
            const FourierOptions options = FourierOptions.NoScaling;
            var samples = Generate.RandomComplex(46500, GetUniform(1));

            var provider = new Complex[samples.Length];
            samples.Copy(provider);
            Control.FourierTransformProvider.Forward(provider, FourierTransformScaling.NoScaling);

            Verify(samples, 10, options, (a, b) => provider, Fourier.BluesteinForward);
        }

        [Test, Explicit("Long-Running")]
        public void AlgorithmsMatchNaive_PowerOfTwo_Large_32()
        {
            // 65536 = 2^16
            const FourierOptions options = FourierOptions.NoScaling;
            var samples = Generate.RandomComplex32(65536, GetUniform(1));
            var naive = Fourier.NaiveForward(samples, options);

            Verify(samples, 5, options, (a, b) => naive, Fourier.Radix2Forward);
            Verify(samples, 5, options, (a, b) => naive, Fourier.BluesteinForward);
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
        public void AlgorithmsMatchNaive_Arbitrary_Large_32()
        {
            // 30870 = 2*3*3*5*7*7*7
            const FourierOptions options = FourierOptions.NoScaling;
            var samples = Generate.RandomComplex32(30870, GetUniform(1));

            Verify(samples, 5, options, Fourier.NaiveForward, Fourier.BluesteinForward);
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

        [Test, Explicit("Long-Running")]
        public void AlgorithmsMatchNaive_Arbitrary_Large_GH286_32()
        {
            const FourierOptions options = FourierOptions.NoScaling;
            var samples = Generate.RandomComplex32(46500, GetUniform(1));

            Verify(samples, 5, options, Fourier.NaiveForward, Fourier.BluesteinForward);
        }

        [Test, Explicit("Long-Running")]
        public void AlgorithmsMatchNaive_Arbitrary_Large_GH286()
        {
            const FourierOptions options = FourierOptions.NoScaling;
            var samples = Generate.RandomComplex(46500, GetUniform(1));
            var naive = Fourier.NaiveForward(samples, options);

            Verify(samples, 10, options, (a, b) => naive, Fourier.BluesteinForward);
        }
    }
}
