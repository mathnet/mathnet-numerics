// <copyright file="FourierTest.cs" company="Math.NET">
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
    /// Fourier test.
    /// </summary>
    [TestFixture]
    public class FourierTest
    {
        /// <summary>
        /// Continuous uniform distribution.
        /// </summary>
        private IContinuousDistribution GetUniform(int seed)
        {
            return new ContinuousUniform(-1, 1)
            {
                RandomSource = new Random(seed)
            };
        }

        /// <summary>
        /// Naive transforms real sine correctly.
        /// </summary>
        [Test]
        public void NaiveTransformsRealSineCorrectly()
        {
            var samples = SignalGenerator.EquidistantPeriodic(w => new Complex(Math.Sin(w), 0), Constants.Pi2, 0, 16);

            // real-odd transforms to imaginary odd
            var dft = new DiscreteFourierTransform();
            var spectrum = dft.NaiveForward(samples, FourierOptions.Matlab);

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
        public void Radix2ThrowsWhenNotPowerOfTwo()
        {
            var samples = SignalGenerator.Random((u, v) => new Complex(u, v), GetUniform(1), 0x7F);

            var dft = new DiscreteFourierTransform();

            Assert.Throws(
                typeof(ArgumentException),
                () => dft.Radix2Forward(samples, FourierOptions.Default));

            Assert.Throws(
                typeof(ArgumentException),
                () => dft.Radix2Inverse(samples, FourierOptions.Default));

            Assert.Throws(
                typeof(ArgumentException),
                () => DiscreteFourierTransform.Radix2(samples, -1));
            Assert.Throws(
                typeof(ArgumentException),
                () => DiscreteFourierTransform.Radix2Parallel(samples, -1));
        }
    }
}
