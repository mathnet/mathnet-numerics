// <copyright file="MatchingNaiveTransformTest.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2018 Math.NET
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
using System.Numerics;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.IntegralTransforms;
using MathNet.Numerics.Providers.FourierTransform;
using NUnit.Framework;

namespace MathNet.Numerics.UnitTests.IntegralTransformsTests
{
    /// <summary>
    /// Matching Naive transform tests.
    /// </summary>
    [TestFixture, Category("FFT")]
    public class MatchingReferenceTransformTest
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

        static void Verify(
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

        static void Verify(
            Complex32[] samples,
            int maximumErrorDecimalPlaces,
            FourierTransformScaling options,
            Action<Complex32[], FourierTransformScaling> expected,
            Action<Complex32[], FourierTransformScaling> actual)
        {
            var spectrumExpected = new Complex32[samples.Length];
            samples.CopyTo(spectrumExpected, 0);
            expected(spectrumExpected, options);

            var spectrumActual = new Complex32[samples.Length];
            samples.CopyTo(spectrumActual, 0);
            actual(spectrumActual, options);

            AssertHelpers.AlmostEqual(spectrumExpected, spectrumActual, maximumErrorDecimalPlaces);
        }

        static void Verify(
            Complex[] samples,
            int maximumErrorDecimalPlaces,
            FourierTransformScaling options,
            Action<Complex[], FourierTransformScaling> expected,
            Action<Complex[], FourierTransformScaling> actual)
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
        public void FourierRadix2MatchesReferenceRealSine32(FourierOptions options)
        {
            var samples = Generate.PeriodicMap(16, w => new Complex32((float)Math.Sin(w), 0), 16, 1.0, Constants.Pi2);
            Verify(samples, 6, options, ReferenceDiscreteFourierTransform.Forward, Fourier.Forward);
            Verify(samples, 6, options, ReferenceDiscreteFourierTransform.Inverse, Fourier.Inverse);
        }

        /// <summary>
        /// Fourier Radix2XX matches naive on real sine.
        /// </summary>
        /// <param name="options">Fourier options.</param>
        [TestCase(FourierOptions.Default)]
        [TestCase(FourierOptions.Matlab)]
        [TestCase(FourierOptions.NumericalRecipes)]
        public void FourierRadix2MatchesReferenceRealSine64(FourierOptions options)
        {
            var samples = Generate.PeriodicMap(16, w => new Complex(Math.Sin(w), 0), 16, 1.0, Constants.Pi2);
            Verify(samples, 12, options, ReferenceDiscreteFourierTransform.Forward, Fourier.Forward);
            Verify(samples, 12, options, ReferenceDiscreteFourierTransform.Inverse, Fourier.Inverse);
        }

        /// <summary>
        /// Fourier Radix2XX matches naive on random.
        /// </summary>
        /// <param name="options">Fourier options.</param>
        [TestCase(FourierOptions.Default)]
        [TestCase(FourierOptions.Matlab)]
        [TestCase(FourierOptions.NumericalRecipes)]
        public void FourierRadix2MatchesReferenceRandom32(FourierOptions options)
        {
            var samples = Generate.RandomComplex32(0x80, GetUniform(1));
            Verify(samples, 5, options, ReferenceDiscreteFourierTransform.Forward, Fourier.Forward);
            Verify(samples, 5, options, ReferenceDiscreteFourierTransform.Inverse, Fourier.Inverse);
        }

        /// <summary>
        /// Fourier Radix2XX matches naive on random.
        /// </summary>
        /// <param name="options">Fourier options.</param>
        [TestCase(FourierOptions.Default)]
        [TestCase(FourierOptions.Matlab)]
        [TestCase(FourierOptions.NumericalRecipes)]
        public void FourierRadix2MatchesReferenceRandom64(FourierOptions options)
        {
            var samples = Generate.RandomComplex(0x80, GetUniform(1));
            Verify(samples, 10, options, ReferenceDiscreteFourierTransform.Forward, Fourier.Forward);
            Verify(samples, 10, options, ReferenceDiscreteFourierTransform.Inverse, Fourier.Inverse);
        }

        /// <summary>
        /// Fourier bluestein matches naive on real sine non-power of two.
        /// </summary>
        /// <param name="options">Fourier options.</param>
        [TestCase(FourierOptions.Default)]
        [TestCase(FourierOptions.Matlab)]
        [TestCase(FourierOptions.NumericalRecipes)]
        public void FourierBluesteinMatchesReferenceRealSineArbitrary32(FourierOptions options)
        {
            var samples = Generate.PeriodicMap(14, w => new Complex32((float)Math.Sin(w), 0), 14, 1.0, Constants.Pi2);
            Verify(samples, 6, options, ReferenceDiscreteFourierTransform.Forward, Fourier.Forward);
            Verify(samples, 6, options, ReferenceDiscreteFourierTransform.Inverse, Fourier.Inverse);
        }

        /// <summary>
        /// Fourier bluestein matches naive on real sine non-power of two.
        /// </summary>
        /// <param name="options">Fourier options.</param>
        [TestCase(FourierOptions.Default)]
        [TestCase(FourierOptions.Matlab)]
        [TestCase(FourierOptions.NumericalRecipes)]
        public void FourierBluesteinMatchesReferenceRealSineArbitrary64(FourierOptions options)
        {
            var samples = Generate.PeriodicMap(14, w => new Complex(Math.Sin(w), 0), 14, 1.0, Constants.Pi2);
            Verify(samples, 12, options, ReferenceDiscreteFourierTransform.Forward, Fourier.Forward);
            Verify(samples, 12, options, ReferenceDiscreteFourierTransform.Inverse, Fourier.Inverse);
        }

        /// <summary>
        /// Fourier bluestein matches naive on random power of two.
        /// </summary>
        /// <param name="options">Fourier options.</param>
        [TestCase(FourierOptions.Default)]
        [TestCase(FourierOptions.Matlab)]
        [TestCase(FourierOptions.NumericalRecipes)]
        public void FourierBluesteinMatchesReferenceRandomPowerOfTwo32(FourierOptions options)
        {
            var samples = Generate.RandomComplex32(0x80, GetUniform(1));
            Verify(samples, 5, options, ReferenceDiscreteFourierTransform.Forward, Fourier.Forward);
            Verify(samples, 5, options, ReferenceDiscreteFourierTransform.Inverse, Fourier.Inverse);
        }

        /// <summary>
        /// Fourier bluestein matches naive on random power of two.
        /// </summary>
        /// <param name="options">Fourier options.</param>
        [TestCase(FourierOptions.Default)]
        [TestCase(FourierOptions.Matlab)]
        [TestCase(FourierOptions.NumericalRecipes)]
        public void FourierBluesteinMatchesReferenceRandomPowerOfTwo64(FourierOptions options)
        {
            var samples = Generate.RandomComplex(0x80, GetUniform(1));
            Verify(samples, 10, options, ReferenceDiscreteFourierTransform.Forward, Fourier.Forward);
            Verify(samples, 10, options, ReferenceDiscreteFourierTransform.Inverse, Fourier.Inverse);
        }

        /// <summary>
        /// Fourier bluestein matches naive on random non-power of two.
        /// </summary>
        /// <param name="options">Fourier options.</param>
        [TestCase(FourierOptions.Default)]
        [TestCase(FourierOptions.Matlab)]
        [TestCase(FourierOptions.NumericalRecipes)]
        public void FourierBluesteinMatchesReferenceRandomArbitrary32(FourierOptions options)
        {
            var samples = Generate.RandomComplex32(0x7F, GetUniform(1));
            Verify(samples, 5, options, ReferenceDiscreteFourierTransform.Forward, Fourier.Forward);
            Verify(samples, 5, options, ReferenceDiscreteFourierTransform.Inverse, Fourier.Inverse);
        }

        /// <summary>
        /// Fourier bluestein matches naive on random non-power of two.
        /// </summary>
        /// <param name="options">Fourier options.</param>
        [TestCase(FourierOptions.Default)]
        [TestCase(FourierOptions.Matlab)]
        [TestCase(FourierOptions.NumericalRecipes)]
        public void FourierBluesteinMatchesReferenceRandomArbitrary64(FourierOptions options)
        {
            var samples = Generate.RandomComplex(0x7F, GetUniform(1));
            Verify(samples, 10, options, ReferenceDiscreteFourierTransform.Forward, Fourier.Forward);
            Verify(samples, 10, options, ReferenceDiscreteFourierTransform.Inverse, Fourier.Inverse);
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
        public void RealMatchesComplex64(FourierOptions options, int n)
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
        public void ProviderMatchesManagedProviderPowerOfTwoLarge32()
        {
            // 65536 = 2^16
            var samples = Generate.RandomComplex32(65536, GetUniform(1));
            Verify(samples, 5, FourierTransformScaling.NoScaling, FourierTransformControl.CreateManaged().Forward, FourierTransformControl.Provider.Forward);
        }

        [Test]
        public void ProviderMatchesManagedProviderPowerOfTwoLarge64()
        {
            // 65536 = 2^16
            var samples = Generate.RandomComplex(65536, GetUniform(1));
            Verify(samples, 10, FourierTransformScaling.NoScaling, FourierTransformControl.CreateManaged().Forward, FourierTransformControl.Provider.Forward);
        }

        [Test]
        public void ProviderMatchesManagedProviderArbitraryLarge32()
        {
            // 30870 = 2*3*3*5*7*7*7
            var samples = Generate.RandomComplex32(30870, GetUniform(1));
            Verify(samples, 5, FourierTransformScaling.NoScaling, FourierTransformControl.CreateManaged().Forward, FourierTransformControl.Provider.Forward);
        }

        [Test]
        public void ProviderMatchesManagedProviderArbitraryLarge64()
        {
            // 30870 = 2*3*3*5*7*7*7
            var samples = Generate.RandomComplex(30870, GetUniform(1));
            Verify(samples, 10, FourierTransformScaling.NoScaling, FourierTransformControl.CreateManaged().Forward, FourierTransformControl.Provider.Forward);
        }

        [Test]
        public void ProviderMatchesManagedProviderArbitraryLarge32_GH286()
        {
            var samples = Generate.RandomComplex32(46500, GetUniform(1));
            Verify(samples, 5, FourierTransformScaling.NoScaling, FourierTransformControl.CreateManaged().Forward, FourierTransformControl.Provider.Forward);
        }

        [Test]
        public void ProviderMatchesManagedProviderArbitraryLarge64_GH286()
        {
            var samples = Generate.RandomComplex(46500, GetUniform(1));
            Verify(samples, 10, FourierTransformScaling.NoScaling, FourierTransformControl.CreateManaged().Forward, FourierTransformControl.Provider.Forward);
        }

        [Test, Explicit("Long-Running")]
        public void AlgorithmsMatchReferencePowerOfTwoLarge32()
        {
            // 65536 = 2^16
            var samples = Generate.RandomComplex32(65536, GetUniform(1));
            Verify(samples, 3, FourierOptions.NoScaling, ReferenceDiscreteFourierTransform.Forward, Fourier.Forward);
        }

        [Test, Explicit("Long-Running")]
        public void AlgorithmsMatchReferencePowerOfTwoLarge64()
        {
            // 65536 = 2^16
            var samples = Generate.RandomComplex(65536, GetUniform(1));
            Verify(samples, 10, FourierOptions.NoScaling, ReferenceDiscreteFourierTransform.Forward, Fourier.Forward);
        }

        [Test, Explicit("Long-Running")]
        public void AlgorithmsMatchReferenceArbitraryLarge32()
        {
            // 30870 = 2*3*3*5*7*7*7
            var samples = Generate.RandomComplex32(30870, GetUniform(1));
            Verify(samples, 4, FourierOptions.NoScaling, ReferenceDiscreteFourierTransform.Forward, Fourier.Forward);
        }

        [Test, Explicit("Long-Running")]
        public void AlgorithmsMatchReferenceArbitraryLarge64()
        {
            // 30870 = 2*3*3*5*7*7*7
            var samples = Generate.RandomComplex(30870, GetUniform(1));
            Verify(samples, 10, FourierOptions.NoScaling, ReferenceDiscreteFourierTransform.Forward, Fourier.Forward);
        }

        [Test, Explicit("Long-Running")]
        public void AlgorithmsMatchReferenceArbitraryLarge32_GH286()
        {
            var samples = Generate.RandomComplex32(46500, GetUniform(1));
            Verify(samples, 4, FourierOptions.NoScaling, ReferenceDiscreteFourierTransform.Forward, Fourier.Forward);
        }

        [Test, Explicit("Long-Running")]
        public void AlgorithmsMatchReferenceArbitraryLarge64_GH286()
        {
            var samples = Generate.RandomComplex(46500, GetUniform(1));
            Verify(samples, 10, FourierOptions.NoScaling, ReferenceDiscreteFourierTransform.Forward, Fourier.Forward);
        }
    }
}
