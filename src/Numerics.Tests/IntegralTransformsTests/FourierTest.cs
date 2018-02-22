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
using System.Numerics;
using MathNet.Numerics.IntegralTransforms;
using NUnit.Framework;

namespace MathNet.Numerics.UnitTests.IntegralTransformsTests
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
    }
}
