// <copyright file="DftTest.cs" company="Math.NET">
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
    using System.Linq;
    using MbUnit.Framework;
    using IntegralTransforms;
    using IntegralTransforms.Algorithms;
    using Statistics;

    [TestFixture]
    public class DftTest
    {
        private static readonly Random _random = new Random();

        private static Complex[] ProvideSamples(int count)
        {
            var samples = new Complex[count];
            for (int i = 0; i < samples.Length; i++)
            {
                samples[i] = Complex.WithRealImaginary(
                    1 - (2 * _random.NextDouble()),
                    1 - (2 * _random.NextDouble()));
            }

            return samples;
        }

        [Test]
        public void NaiveTransformsRealSineCorrectly()
        {
            var realSine = new Complex[16];
            for (int i = 0; i < realSine.Length; i++)
            {
                realSine[i] = Math.Sin(i / 8.0 * Constants.Pi);
            }

            // real-odd transforms to imaginary odd
            var dft = new DiscreteFourierTransform();
            var spectrum = dft.NaiveForward(realSine, FourierOptions.Matlab);

            // all real components must be zero
            foreach (var c in spectrum)
            {
                Assert.AreApproximatelyEqual(0, c.Real, 1e-12, "real");
            }

            // all imaginary components except second and last musth be zero
            for(int i = 0; i<spectrum.Length; i++)
            {
                if(i == 1)
                {
                    Assert.AreApproximatelyEqual(-8, spectrum[i].Imaginary, 1e-12, "imag second");
                }
                else if (i == spectrum.Length - 1)
                {
                    Assert.AreApproximatelyEqual(8, spectrum[i].Imaginary, 1e-12, "imag last");
                }
                else
                {
                    Assert.AreApproximatelyEqual(0, spectrum[i].Imaginary, 1e-12, "imag");
                }
            }
        }

        [Test]
        [Row(FourierOptions.Default)]
        [Row(FourierOptions.Matlab)]
        public void NaiveIsReversible(FourierOptions options)
        {
            var samples = ProvideSamples(0x80);
            var work = new Complex[samples.Length];
            samples.CopyTo(work, 0);

            var dft = new DiscreteFourierTransform();
            work = dft.NaiveForward(work, options);

            Assert.IsFalse(work.AlmostEqualListWithError(samples, 1e-12));

            work = dft.NaiveInverse(work, options);

            AssertHelpers.AlmostEqualList(samples, work, 1e-12);
        }

        [Test]
        public void Radix2MatchesNaiveOnRealSine()
        {
            var realSine = new Complex[16];
            for (int i = 0; i < realSine.Length; i++)
            {
                realSine[i] = Math.Sin(i / 8.0 * Constants.Pi);
            }

            // real-odd transforms to imaginary odd
            var dft = new DiscreteFourierTransform();
            var spectrumNaive = dft.NaiveForward(realSine, FourierOptions.Matlab);

            var spectrumRadix2 = new Complex[realSine.Length];
            realSine.CopyTo(spectrumRadix2, 0);
            dft.Radix2Forward(spectrumRadix2, FourierOptions.Matlab);

            AssertHelpers.AlmostEqualList(spectrumNaive, spectrumRadix2, 1e-12);
        }

        [Test]
        public void Radix2MatchesNaiveOnRandom()
        {
            var samples = ProvideSamples(0x80);
            var work = new Complex[samples.Length];
            samples.CopyTo(work, 0);

            var dft = new DiscreteFourierTransform();
            var spectrumNaive = dft.NaiveForward(samples, FourierOptions.Matlab);
            dft.Radix2Forward(work, FourierOptions.Matlab);

            AssertHelpers.AlmostEqualList(spectrumNaive, work, 1e-12);
        }

        [Test]
        [Row(FourierOptions.Default)]
        [Row(FourierOptions.Matlab)]
        public void Radix2IsReversible(FourierOptions options)
        {
            var samples = ProvideSamples(0x8000);
            var work = new Complex[samples.Length];
            samples.CopyTo(work, 0);

            var dft = new DiscreteFourierTransform();
            dft.Radix2Forward(work, options);

            Assert.IsFalse(work.AlmostEqualListWithError(samples, 1e-12));

            dft.Radix2Inverse(work, options);

            AssertHelpers.AlmostEqualList(samples, work, 1e-12);
        }

        [Test]
        public void Radix2ThrowsWhenNotPowerOfTwo()
        {
            var samples = ProvideSamples(0x7F);

            var dft = new DiscreteFourierTransform();

            Assert.Throws(
                typeof(ArgumentException),
                () => dft.Radix2Forward(samples, FourierOptions.Default));

            Assert.Throws(
                typeof(ArgumentException),
                () => dft.Radix2Inverse(samples, FourierOptions.Default));
        }

        [Test]
        public void BluesteinMatchesNaiveOnRealSine()
        {
            var realSine = new Complex[14];
            for (int i = 0; i < realSine.Length; i++)
            {
                realSine[i] = Math.Sin(i / 7.0 * Constants.Pi);
            }

            // real-odd transforms to imaginary odd
            var dft = new DiscreteFourierTransform();
            var spectrumNaive = dft.NaiveForward(realSine, FourierOptions.Matlab);

            var spectrumBluestein = new Complex[realSine.Length];
            realSine.CopyTo(spectrumBluestein, 0);
            dft.BluesteinForward(spectrumBluestein, FourierOptions.Matlab);

            AssertHelpers.AlmostEqualList(spectrumNaive, spectrumBluestein, 1e-12);
        }

        [Test]
        public void BluesteinMatchesNaiveOnRandomPowerOfTwo()
        {
            var samples = ProvideSamples(0x80);
            var work = new Complex[samples.Length];
            samples.CopyTo(work, 0);

            var dft = new DiscreteFourierTransform();
            var spectrumNaive = dft.NaiveForward(samples, FourierOptions.Matlab);
            dft.BluesteinForward(work, FourierOptions.Matlab);

            AssertHelpers.AlmostEqualList(spectrumNaive, work, 1e-12);
        }

        [Test]
        public void BluesteinMatchesNaiveOnRandomNonPowerOfTwo()
        {
            var samples = ProvideSamples(0x7F);
            var work = new Complex[samples.Length];
            samples.CopyTo(work, 0);

            var dft = new DiscreteFourierTransform();
            var spectrumNaive = dft.NaiveForward(samples, FourierOptions.Matlab);
            dft.BluesteinForward(work, FourierOptions.Matlab);

            AssertHelpers.AlmostEqualList(spectrumNaive, work, 1e-12);
        }

        [Test]
        [Row(FourierOptions.Default)]
        [Row(FourierOptions.Matlab)]
        public void BluesteinIsReversible(FourierOptions options)
        {
            var samples = ProvideSamples(0x7FFF);
            var work = new Complex[samples.Length];
            samples.CopyTo(work, 0);

            var dft = new DiscreteFourierTransform();
            dft.BluesteinForward(work, options);

            Assert.IsFalse(work.AlmostEqualListWithError(samples, 1e-12));

            dft.BluesteinInverse(work, options);

            AssertHelpers.AlmostEqualList(samples, work, 1e-12);
        }

        [Test]
        public void DefaultTransformIsReversible()
        {
            var samples = ProvideSamples(0x7FFF);
            var work = new Complex[samples.Length];
            samples.CopyTo(work, 0);

            Transform.FourierForward(work);

            Assert.IsFalse(work.AlmostEqualListWithError(samples, 1e-12));

            Transform.FourierInverse(work);

            AssertHelpers.AlmostEqualList(samples, work, 1e-12);

            Transform.FourierInverse(work, FourierOptions.Default);

            Assert.IsFalse(work.AlmostEqualListWithError(samples, 1e-12));

            Transform.FourierForward(work, FourierOptions.Default);

            AssertHelpers.AlmostEqualList(samples, work, 1e-12);
        }

        [Test]
        public void TransformSatisfiesParsevalsTheorem()
        {
            var samples = ProvideSamples(0x1000);

            var timeSpaceEnergy = (from s in samples select s.ModulusSquared).Mean();

            var work = new Complex[samples.Length];
            samples.CopyTo(work, 0);

            // Only symmetric scaling scaling satisfies the theorem, hence FourierOptions.Default.
            Transform.FourierForward(work, FourierOptions.Default);

            var frequencySpaceEnergy = (from s in work select s.ModulusSquared).Mean();

            Assert.AreApproximatelyEqual(timeSpaceEnergy, frequencySpaceEnergy, 1e-12);
        }
    }
}
