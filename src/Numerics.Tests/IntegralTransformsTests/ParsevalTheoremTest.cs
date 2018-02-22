// <copyright file="ParsevalTheoremTest.cs" company="Math.NET">
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

using System.Linq;
using System.Numerics;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.IntegralTransforms;
using MathNet.Numerics.Statistics;
using NUnit.Framework;

namespace MathNet.Numerics.UnitTests.IntegralTransformsTests
{
    /// <summary>
    /// Parseval's theorem verification tests.
    /// </summary>
    [TestFixture, Category("FFT")]
    public class ParsevalTheoremTest
    {
        /// <summary>
        /// Continuous uniform distribution.
        /// </summary>
        IContinuousDistribution GetUniform(int seed)
        {
            return new ContinuousUniform(-1, 1, new System.Random(seed));
        }

        /// <summary>
        /// Fourier default transform satisfies Parseval's theorem.
        /// </summary>
        /// <param name="count">Samples count.</param>
        [TestCase(0x1000)]
        [TestCase(0x7FF)]
        public void ReferenceDftSatisfiesParsevalsTheorem32(int count)
        {
            var samples = Generate.RandomComplex32(count, GetUniform(1));
            var timeSpaceEnergy = (from s in samples select s.MagnitudeSquared()).Mean();

            var spectrum = new Complex32[samples.Length];
            samples.CopyTo(spectrum, 0);
            ReferenceDiscreteFourierTransform.Forward(spectrum);
            var frequencySpaceEnergy = (from s in spectrum select s.MagnitudeSquared()).Mean();

            Assert.AreEqual(timeSpaceEnergy, frequencySpaceEnergy, 1e-7);
        }

        /// <summary>
        /// Fourier default transform satisfies Parseval's theorem.
        /// </summary>
        /// <param name="count">Samples count.</param>
        [TestCase(0x1000)]
        [TestCase(0x7FF)]
        public void ReferenceDftSatisfiesParsevalsTheorem64(int count)
        {
            var samples = Generate.RandomComplex(count, GetUniform(1));
            var timeSpaceEnergy = (from s in samples select s.MagnitudeSquared()).Mean();

            var spectrum = new Complex[samples.Length];
            samples.CopyTo(spectrum, 0);
            ReferenceDiscreteFourierTransform.Forward(spectrum);
            var frequencySpaceEnergy = (from s in spectrum select s.MagnitudeSquared()).Mean();

            Assert.AreEqual(timeSpaceEnergy, frequencySpaceEnergy, 1e-12);
        }

        /// <summary>
        /// Fourier default transform satisfies Parseval's theorem.
        /// </summary>
        /// <param name="count">Samples count.</param>
        [TestCase(0x1000)]
        [TestCase(0x7FF)]
        public void FourierDefaultTransformSatisfiesParsevalsTheorem32(int count)
        {
            var samples = Generate.RandomComplex32(count, GetUniform(1));
            var timeSpaceEnergy = (from s in samples select s.MagnitudeSquared()).Mean();

            var spectrum = new Complex32[samples.Length];
            samples.CopyTo(spectrum, 0);
            Fourier.Forward(spectrum);
            var frequencySpaceEnergy = (from s in spectrum select s.MagnitudeSquared()).Mean();

            Assert.AreEqual(timeSpaceEnergy, frequencySpaceEnergy, 1e-7);
        }

        /// <summary>
        /// Fourier default transform satisfies Parseval's theorem.
        /// </summary>
        /// <param name="count">Samples count.</param>
        [TestCase(0x1000)]
        [TestCase(0x7FF)]
        public void FourierDefaultTransformSatisfiesParsevalsTheorem64(int count)
        {
            var samples = Generate.RandomComplex(count, GetUniform(1));
            var timeSpaceEnergy = (from s in samples select s.MagnitudeSquared()).Mean();

            var spectrum = new Complex[samples.Length];
            samples.CopyTo(spectrum, 0);
            Fourier.Forward(spectrum);
            var frequencySpaceEnergy = (from s in spectrum select s.MagnitudeSquared()).Mean();

            Assert.AreEqual(timeSpaceEnergy, frequencySpaceEnergy, 1e-12);
        }

        /// <summary>
        /// Hartley default naive satisfies Parseval's theorem.
        /// </summary>
        /// <param name="count">Samples count.</param>
        [TestCase(0x40)]
        [TestCase(0x1F)]
        public void HartleyDefaultNaiveSatisfiesParsevalsTheorem64(int count)
        {
            var samples = Generate.Random(count, GetUniform(1));
            var timeSpaceEnergy = (from s in samples select s*s).Mean();

            var spectrum = new double[samples.Length];
            samples.CopyTo(spectrum, 0);
            spectrum = Hartley.NaiveForward(spectrum, HartleyOptions.Default);

            var frequencySpaceEnergy = (from s in spectrum select s*s).Mean();
            Assert.AreEqual(timeSpaceEnergy, frequencySpaceEnergy, 1e-12);
        }
    }
}
