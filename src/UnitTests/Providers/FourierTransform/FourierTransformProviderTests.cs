// <copyright file="FourierTransformProviderTests.cs" company="Math.NET">
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
using MathNet.Numerics.Providers.FourierTransform;
using MathNet.Numerics.Statistics;
using NUnit.Framework;

namespace MathNet.Numerics.UnitTests.Providers.FourierTransform
{
#if NOSYSNUMERICS
    using Complex = Numerics.Complex;
#else
    using Complex = System.Numerics.Complex;
#endif

    /// <summary>
    /// Fourier transform provider tests.
    /// </summary>
    [TestFixture, Category("LAProvider")]
    public class FourierTransformProviderTests
    {
        [Test]
        public void ForwardInplaceRealSine()
        {
            var samples = Generate.PeriodicMap(16, w => new Complex(Math.Sin(w), 0), 16, 1.0, Constants.Pi2);
            var spectrum = new Complex[samples.Length];

            // real-odd transforms to imaginary odd
            samples.Copy(spectrum);
            Control.FourierTransformProvider.Forward(spectrum, FourierTransformScaling.BackwardScaling);

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

        [TestCase(0x1000)]
        [TestCase(0x7FF)]
        public void ForwardInplaceParsevalTheorem(int count)
        {
            var samples = Generate.RandomComplex(count, GetUniform(1));
            var timeSpaceEnergy = Generate.Map(samples, s => s.MagnitudeSquared()).Mean();

            Control.FourierTransformProvider.Forward(samples, FourierTransformScaling.SymmetricScaling);
            var frequencySpaceEnergy = Generate.Map(samples, s => s.MagnitudeSquared()).Mean();

            Assert.AreEqual(timeSpaceEnergy, frequencySpaceEnergy, 1e-12);
        }

        IContinuousDistribution GetUniform(int seed)
        {
            return new ContinuousUniform(-1, 1, new System.Random(seed));
        }

    }
}
