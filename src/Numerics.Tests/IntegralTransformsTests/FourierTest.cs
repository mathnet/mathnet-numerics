// <copyright file="FourierTest.cs" company="Math.NET">
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
using Complex = System.Numerics.Complex;
using MathNet.Numerics.IntegralTransforms;
using NUnit.Framework;

namespace MathNet.Numerics.Tests.IntegralTransformsTests
{
    /// <summary>
    /// Fourier test.
    /// </summary>
    [TestFixture, Category("FFT")]
    public class FourierTest
    {
        [Test]
        public void ReferenceDftTransformsRealSineCorrectly32()
        {
            var samples = Generate.PeriodicMap(16, w => new Complex32((float)Math.Sin(w), 0), 16, 1.0, Constants.Pi2);

            // real-odd transforms to imaginary odd
            ReferenceDiscreteFourierTransform.Forward(samples, FourierOptions.Matlab);

            // all real components must be zero
            foreach (var c in samples)
            {
                Assert.AreEqual(0, c.Real, 1e-6, "real");
            }

            // all imaginary components except second and last musth be zero
            for (var i = 0; i < samples.Length; i++)
            {
                if (i == 1)
                {
                    Assert.AreEqual(-8, samples[i].Imaginary, 1e-12, "imag second");
                }
                else if (i == samples.Length - 1)
                {
                    Assert.AreEqual(8, samples[i].Imaginary, 1e-12, "imag last");
                }
                else
                {
                    Assert.AreEqual(0, samples[i].Imaginary, 1e-6, "imag");
                }
            }
        }

        [Test]
        public void ReferenceDftTransformsRealSineCorrectly64()
        {
            var samples = Generate.PeriodicMap(16, w => new Complex(Math.Sin(w), 0), 16, 1.0, Constants.Pi2);

            // real-odd transforms to imaginary odd
            ReferenceDiscreteFourierTransform.Forward(samples, FourierOptions.Matlab);

            // all real components must be zero
            foreach (var c in samples)
            {
                Assert.AreEqual(0, c.Real, 1e-12, "real");
            }

            // all imaginary components except second and last musth be zero
            for (var i = 0; i < samples.Length; i++)
            {
                if (i == 1)
                {
                    Assert.AreEqual(-8, samples[i].Imaginary, 1e-12, "imag second");
                }
                else if (i == samples.Length - 1)
                {
                    Assert.AreEqual(8, samples[i].Imaginary, 1e-12, "imag last");
                }
                else
                {
                    Assert.AreEqual(0, samples[i].Imaginary, 1e-12, "imag");
                }
            }
        }

        [Test]
        public void FourierDefaultTransformsRealSineCorrectly32()
        {
            var samples = Generate.PeriodicMap(16, w => new Complex32((float)Math.Sin(w), 0), 16, 1.0, Constants.Pi2);

            // real-odd transforms to imaginary odd
            Fourier.Forward(samples, FourierOptions.Matlab);

            // all real components must be zero
            foreach (var c in samples)
            {
                Assert.AreEqual(0, c.Real, 1e-6, "real");
            }

            // all imaginary components except second and last musth be zero
            for (var i = 0; i < samples.Length; i++)
            {
                if (i == 1)
                {
                    Assert.AreEqual(-8, samples[i].Imaginary, 1e-12, "imag second");
                }
                else if (i == samples.Length - 1)
                {
                    Assert.AreEqual(8, samples[i].Imaginary, 1e-12, "imag last");
                }
                else
                {
                    Assert.AreEqual(0, samples[i].Imaginary, 1e-6, "imag");
                }
            }
        }

        [Test]
        public void FourierDefaultTransformsRealSineCorrectly64()
        {
            var samples = Generate.PeriodicMap(16, w => new Complex(Math.Sin(w), 0), 16, 1.0, Constants.Pi2);

            // real-odd transforms to imaginary odd
            Fourier.Forward(samples, FourierOptions.Matlab);

            // all real components must be zero
            foreach (var c in samples)
            {
                Assert.AreEqual(0, c.Real, 1e-12, "real");
            }

            // all imaginary components except second and last musth be zero
            for (var i = 0; i < samples.Length; i++)
            {
                if (i == 1)
                {
                    Assert.AreEqual(-8, samples[i].Imaginary, 1e-12, "imag second");
                }
                else if (i == samples.Length - 1)
                {
                    Assert.AreEqual(8, samples[i].Imaginary, 1e-12, "imag last");
                }
                else
                {
                    Assert.AreEqual(0, samples[i].Imaginary, 1e-12, "imag");
                }
            }
        }
        [Test]
        public void Forward2DAndInverse2DRoundtrip64()
        {
            const int rows = 4;
            const int cols = 8;
            var original = new Complex[rows * cols];
            var rng = new System.Random(42);
            for (int i = 0; i < original.Length; i++)
            {
                original[i] = new Complex(rng.NextDouble(), rng.NextDouble());
            }

            var samples = (Complex[])original.Clone();

            Fourier.Forward2D(samples, rows, cols, FourierOptions.Default);

            // Verify the transform actually changed the data
            bool anyDifferent = false;
            for (int i = 0; i < samples.Length; i++)
            {
                if ((samples[i] - original[i]).Magnitude > 1e-10)
                {
                    anyDifferent = true;
                    break;
                }
            }
            Assert.IsTrue(anyDifferent, "Forward2D should modify the data");

            Fourier.Inverse2D(samples, rows, cols, FourierOptions.Default);

            // Verify roundtrip recovers original data
            for (int i = 0; i < samples.Length; i++)
            {
                Assert.AreEqual(original[i].Real, samples[i].Real, 1e-10, $"Real part mismatch at index {i}");
                Assert.AreEqual(original[i].Imaginary, samples[i].Imaginary, 1e-10, $"Imaginary part mismatch at index {i}");
            }
        }

        [Test]
        public void Forward2DAndInverse2DRoundtrip32()
        {
            const int rows = 4;
            const int cols = 8;
            var original = new Complex32[rows * cols];
            var rng = new System.Random(42);
            for (int i = 0; i < original.Length; i++)
            {
                original[i] = new Complex32((float)rng.NextDouble(), (float)rng.NextDouble());
            }

            var samples = (Complex32[])original.Clone();

            Fourier.Forward2D(samples, rows, cols, FourierOptions.Default);
            Fourier.Inverse2D(samples, rows, cols, FourierOptions.Default);

            // Verify roundtrip recovers original data
            for (int i = 0; i < samples.Length; i++)
            {
                Assert.AreEqual(original[i].Real, samples[i].Real, 1e-4, $"Real part mismatch at index {i}");
                Assert.AreEqual(original[i].Imaginary, samples[i].Imaginary, 1e-4, $"Imaginary part mismatch at index {i}");
            }
        }

        [Test]
        public void Forward2DKnownDCValue64()
        {
            // A constant 4x4 array should produce all energy in the DC bin
            const int rows = 4;
            const int cols = 4;
            var samples = new Complex[rows * cols];
            for (int i = 0; i < samples.Length; i++)
            {
                samples[i] = new Complex(1.0, 0.0);
            }

            Fourier.Forward2D(samples, rows, cols, FourierOptions.NoScaling);

            // DC component should equal rows*cols = 16
            Assert.AreEqual(16.0, samples[0].Real, 1e-10, "DC real");
            Assert.AreEqual(0.0, samples[0].Imaginary, 1e-10, "DC imag");

            // All other components should be zero
            for (int i = 1; i < samples.Length; i++)
            {
                Assert.AreEqual(0.0, samples[i].Real, 1e-10, $"Non-DC real at {i}");
                Assert.AreEqual(0.0, samples[i].Imaginary, 1e-10, $"Non-DC imag at {i}");
            }
        }

        [Test]
        public void Forward2DNonPowerOfTwoDimensions64()
        {
            // Test with non-power-of-two dimensions to exercise Bluestein path
            const int rows = 3;
            const int cols = 5;
            var original = new Complex[rows * cols];
            var rng = new System.Random(123);
            for (int i = 0; i < original.Length; i++)
            {
                original[i] = new Complex(rng.NextDouble(), rng.NextDouble());
            }

            var samples = (Complex[])original.Clone();

            Fourier.Forward2D(samples, rows, cols, FourierOptions.Default);
            Fourier.Inverse2D(samples, rows, cols, FourierOptions.Default);

            for (int i = 0; i < samples.Length; i++)
            {
                Assert.AreEqual(original[i].Real, samples[i].Real, 1e-10, $"Real part mismatch at index {i}");
                Assert.AreEqual(original[i].Imaginary, samples[i].Imaginary, 1e-10, $"Imaginary part mismatch at index {i}");
            }
        }
    }
}
