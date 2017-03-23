// <copyright file="FourierTest.cs" company="Math.NET">
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
using NUnit.Framework;

namespace MathNet.Numerics.UnitTests.IntegralTransformsTests
{

#if !NOSYSNUMERICS
    using System.Numerics;
#endif

    /// <summary>
    /// Fourier test.
    /// </summary>
    [TestFixture, Category("FFT")]
    public class FourierTest
    {
        /// <summary>
        /// Continuous uniform distribution.
        /// </summary>
        IContinuousDistribution GetUniform(int seed)
        {
            return new ContinuousUniform(-1, 1, new System.Random(seed));
        }

        /// <summary>
        /// Naive transforms real sine correctly.
        /// </summary>
        [Test]
        public void NaiveTransformsRealSineCorrectly32()
        {
            var samples = Generate.PeriodicMap(16, w => new Complex32((float)Math.Sin(w), 0), 16, 1.0, Constants.Pi2);

            // real-odd transforms to imaginary odd
            var spectrum = Fourier.NaiveForward(samples, FourierOptions.Matlab);

            // all real components must be zero
            foreach (var c in spectrum)
            {
                Assert.AreEqual(0, c.Real, 1e-6, "real");
            }

            // all imaginary components except second and last musth be zero
            for (var i = 0; i < spectrum.Length; i++)
            {
                if (i == 1)
                {
                    Assert.AreEqual(-8, spectrum[i].Imaginary, 1e-12, "imag second");
                }
                else if (i == spectrum.Length - 1)
                {
                    Assert.AreEqual(8, spectrum[i].Imaginary, 1e-12, "imag last");
                }
                else
                {
                    Assert.AreEqual(0, spectrum[i].Imaginary, 1e-6, "imag");
                }
            }
        }

        /// <summary>
        /// Naive transforms real sine correctly.
        /// </summary>
        [Test]
        public void NaiveTransformsRealSineCorrectly()
        {
            var samples = Generate.PeriodicMap(16, w => new Complex(Math.Sin(w), 0), 16, 1.0, Constants.Pi2);

            // real-odd transforms to imaginary odd
            var spectrum = Fourier.NaiveForward(samples, FourierOptions.Matlab);

            // all real components must be zero
            foreach (var c in spectrum)
            {
                Assert.AreEqual(0, c.Real, 1e-12, "real");
            }

            // all imaginary components except second and last musth be zero
            for (var i = 0; i < spectrum.Length; i++)
            {
                if (i == 1)
                {
                    Assert.AreEqual(-8, spectrum[i].Imaginary, 1e-12, "imag second");
                }
                else if (i == spectrum.Length - 1)
                {
                    Assert.AreEqual(8, spectrum[i].Imaginary, 1e-12, "imag last");
                }
                else
                {
                    Assert.AreEqual(0, spectrum[i].Imaginary, 1e-12, "imag");
                }
            }
        }

        /// <summary>
        /// Radix2XXX when not power of two throws <c>ArgumentException</c>.
        /// </summary>
        [Test]
        public void Radix2ThrowsWhenNotPowerOfTwo32()
        {
            var samples = Generate.RandomComplex32(0x7F, GetUniform(1));

            Assert.Throws(typeof(ArgumentException), () => Fourier.Radix2Forward(samples, FourierOptions.Default));
            Assert.Throws(typeof(ArgumentException), () => Fourier.Radix2Inverse(samples, FourierOptions.Default));
            Assert.Throws(typeof(ArgumentException), () => Fourier.Radix2(samples, -1));
            Assert.Throws(typeof(ArgumentException), () => Fourier.Radix2Parallel(samples, -1));
        }

        /// <summary>
        /// Radix2XXX when not power of two throws <c>ArgumentException</c>.
        /// </summary>
        [Test]
        public void Radix2ThrowsWhenNotPowerOfTwo()
        {
            var samples = Generate.RandomComplex(0x7F, GetUniform(1));

            Assert.Throws(typeof (ArgumentException), () => Fourier.Radix2Forward(samples, FourierOptions.Default));
            Assert.Throws(typeof (ArgumentException), () => Fourier.Radix2Inverse(samples, FourierOptions.Default));
            Assert.Throws(typeof (ArgumentException), () => Fourier.Radix2(samples, -1));
            Assert.Throws(typeof (ArgumentException), () => Fourier.Radix2Parallel(samples, -1));
        }
    }
}
