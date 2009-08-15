// <copyright file="FourierTest.cs" company="Math.NET">
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
    using MbUnit.Framework;
    using IntegralTransforms;
    using IntegralTransforms.Algorithms;
    using Sampling;

    [TestFixture]
    public class FourierTest
    {
        [Test]
        public void NaiveTransformsRealSineCorrectly()
        {
            var samples = Sample.EquidistantPeriodic(w => new Complex(Math.Sin(w), 0), Constants.Pi2, 0, 16);

            // real-odd transforms to imaginary odd
            var dft = new DiscreteFourierTransform();
            var spectrum = dft.NaiveForward(samples, FourierOptions.Matlab);

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
        public void Radix2ThrowsWhenNotPowerOfTwo()
        {
            var samples = SampleProvider.ProvideComplexSamples(0x7F);

            var dft = new DiscreteFourierTransform();

            Assert.Throws(
                typeof(ArgumentException),
                () => dft.Radix2Forward(samples, FourierOptions.Default));

            Assert.Throws(
                typeof(ArgumentException),
                () => dft.Radix2Inverse(samples, FourierOptions.Default));

            Assert.Throws(
                typeof (ArgumentException),
                () => DiscreteFourierTransform.Radix2(samples, -1));

            Assert.Throws(
                typeof(ArgumentException),
                () => DiscreteFourierTransform.Radix2Parallel(samples, -1));
        }
    }
}
