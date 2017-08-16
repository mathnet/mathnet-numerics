// <copyright file="InverseTransformTest.cs" company="Math.NET">
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

using MathNet.Numerics.Distributions;
using MathNet.Numerics.IntegralTransforms;
using NUnit.Framework;

namespace MathNet.Numerics.UnitTests.IntegralTransformsTests
{

#if !NOSYSNUMERICS
    using System.Numerics;
#endif

    /// <summary>
    /// Inverse Transform test.
    /// </summary>
    [TestFixture, Category("FFT")]
    public class InverseTransformTest
    {
        /// <summary>
        /// Continuous uniform distribution.
        /// </summary>
        IContinuousDistribution GetUniform(int seed)
        {
            return new ContinuousUniform(-1, 1, new System.Random(seed));
        }

        /// <summary>
        /// Fourier naive is reversible.
        /// </summary>
        /// <param name="options">Fourier options.</param>
        [TestCase(FourierOptions.Default)]
        [TestCase(FourierOptions.Matlab)]
        public void FourierNaiveIsReversible32(FourierOptions options)
        {
            var samples = Generate.RandomComplex32(0x80, GetUniform(1));
            var work = new Complex32[samples.Length];
            samples.CopyTo(work, 0);

            work = Fourier.NaiveForward(work, options);
            Assert.IsFalse(work.ListAlmostEqual(samples, 6));

            work = Fourier.NaiveInverse(work, options);
            AssertHelpers.AlmostEqual(samples, work, 11);
        }

        /// <summary>
        /// Fourier naive is reversible.
        /// </summary>
        /// <param name="options">Fourier options.</param>
        [TestCase(FourierOptions.Default)]
        [TestCase(FourierOptions.Matlab)]
        public void FourierNaiveIsReversible(FourierOptions options)
        {
            var samples = Generate.RandomComplex(0x80, GetUniform(1));
            var work = new Complex[samples.Length];
            samples.CopyTo(work, 0);

            work = Fourier.NaiveForward(work, options);
            Assert.IsFalse(work.ListAlmostEqual(samples, 6));

            work = Fourier.NaiveInverse(work, options);
            AssertHelpers.AlmostEqual(samples, work, 12);
        }

        /// <summary>
        /// Fourier radix2xx is reversible.
        /// </summary>
        /// <param name="options">Fourier options.</param>
        [TestCase(FourierOptions.Default)]
        [TestCase(FourierOptions.Matlab)]
        public void FourierRadix2IsReversible32(FourierOptions options)
        {
            var samples = Generate.RandomComplex32(0x8000, GetUniform(1));
            var work = new Complex32[samples.Length];
            samples.CopyTo(work, 0);

            Fourier.Radix2Forward(work, options);
            Assert.IsFalse(work.ListAlmostEqual(samples, 6));

            Fourier.Radix2Inverse(work, options);
            AssertHelpers.AlmostEqual(samples, work, 12);
        }

        /// <summary>
        /// Fourier radix2xx is reversible.
        /// </summary>
        /// <param name="options">Fourier options.</param>
        [TestCase(FourierOptions.Default)]
        [TestCase(FourierOptions.Matlab)]
        public void FourierRadix2IsReversible(FourierOptions options)
        {
            var samples = Generate.RandomComplex(0x8000, GetUniform(1));
            var work = new Complex[samples.Length];
            samples.CopyTo(work, 0);

            Fourier.Radix2Forward(work, options);
            Assert.IsFalse(work.ListAlmostEqual(samples, 6));

            Fourier.Radix2Inverse(work, options);
            AssertHelpers.AlmostEqual(samples, work, 12);
        }

        /// <summary>
        /// Fourier bluestein is reversible.
        /// </summary>
        /// <param name="options">Fourier options.</param>
        [TestCase(FourierOptions.Default)]
        [TestCase(FourierOptions.Matlab)]
        public void FourierBluesteinIsReversible32(FourierOptions options)
        {
            var samples = Generate.RandomComplex32(0x7FFF, GetUniform(1));
            var work = new Complex32[samples.Length];
            samples.CopyTo(work, 0);

            Fourier.BluesteinForward(work, options);
            Assert.IsFalse(work.ListAlmostEqual(samples, 6));

            Fourier.BluesteinInverse(work, options);
            AssertHelpers.AlmostEqual(samples, work, 10);
        }

        /// <summary>
        /// Fourier bluestein is reversible.
        /// </summary>
        /// <param name="options">Fourier options.</param>
        [TestCase(FourierOptions.Default)]
        [TestCase(FourierOptions.Matlab)]
        public void FourierBluesteinIsReversible(FourierOptions options)
        {
            var samples = Generate.RandomComplex(0x7FFF, GetUniform(1));
            var work = new Complex[samples.Length];
            samples.CopyTo(work, 0);

            Fourier.BluesteinForward(work, options);
            Assert.IsFalse(work.ListAlmostEqual(samples, 6));

            Fourier.BluesteinInverse(work, options);
            AssertHelpers.AlmostEqual(samples, work, 10);
        }

        /// <summary>
        /// Fourier bluestein is reversible.
        /// </summary>
        /// <param name="options">Fourier options.</param>
        [TestCase(FourierOptions.Default)]
        [TestCase(FourierOptions.Matlab)]
        public void FourierRealIsReversible32(FourierOptions options)
        {
            var samples = Generate.RandomSingle(0x7FFF, GetUniform(1));
            var work = new float[samples.Length + 2];
            samples.CopyTo(work, 0);

            Fourier.ForwardReal(work, samples.Length, options);
            Assert.IsFalse(work.ListAlmostEqual(samples, 6));

            Fourier.InverseReal(work, samples.Length, options);
            AssertHelpers.AlmostEqual(samples, work, 5);
        }

        /// <summary>
        /// Fourier bluestein is reversible.
        /// </summary>
        /// <param name="options">Fourier options.</param>
        [TestCase(FourierOptions.Default)]
        [TestCase(FourierOptions.Matlab)]
        public void FourierRealIsReversible(FourierOptions options)
        {
            var samples = Generate.Random(0x7FFF, GetUniform(1));
            var work = new double[samples.Length+2];
            samples.CopyTo(work, 0);

            Fourier.ForwardReal(work, samples.Length, options);
            Assert.IsFalse(work.ListAlmostEqual(samples, 6));

            Fourier.InverseReal(work, samples.Length, options);
            AssertHelpers.AlmostEqual(samples, work, 10);
        }

        /// <summary>
        /// Hartley naive is reversible.
        /// </summary>
        /// <param name="options">Hartley options.</param>
        [TestCase(HartleyOptions.Default)]
        [TestCase(HartleyOptions.AsymmetricScaling)]
        public void HartleyNaiveIsReversible(HartleyOptions options)
        {
            var samples = Generate.Random(0x80, GetUniform(1));
            var work = new double[samples.Length];
            samples.CopyTo(work, 0);

            work = Hartley.NaiveForward(work, options);
            Assert.IsFalse(work.ListAlmostEqual(samples, 6));

            work = Hartley.NaiveInverse(work, options);
            AssertHelpers.AlmostEqual(samples, work, 12);
        }

        /// <summary>
        /// Fourier default transform is reversible.
        /// </summary>
        [Test]
        public void FourierDefaultTransformIsReversible32()
        {
            var samples = Generate.RandomComplex32(0x7FFF, GetUniform(1));
            var work = new Complex32[samples.Length];
            samples.CopyTo(work, 0);

            Fourier.Forward(work);
            Assert.IsFalse(work.ListAlmostEqual(samples, 6));

            Fourier.Inverse(work);
            AssertHelpers.AlmostEqual(samples, work, 10);

            Fourier.Inverse(work, FourierOptions.Default);
            Assert.IsFalse(work.ListAlmostEqual(samples, 6));

            Fourier.Forward(work, FourierOptions.Default);
            AssertHelpers.AlmostEqual(samples, work, 10);
        }

        /// <summary>
        /// Fourier default transform is reversible.
        /// </summary>
        [Test]
        public void FourierDefaultTransformIsReversible()
        {
            var samples = Generate.RandomComplex(0x7FFF, GetUniform(1));
            var work = new Complex[samples.Length];
            samples.CopyTo(work, 0);

            Fourier.Forward(work);
            Assert.IsFalse(work.ListAlmostEqual(samples, 6));

            Fourier.Inverse(work);
            AssertHelpers.AlmostEqual(samples, work, 10);

            Fourier.Inverse(work, FourierOptions.Default);
            Assert.IsFalse(work.ListAlmostEqual(samples, 6));

            Fourier.Forward(work, FourierOptions.Default);
            AssertHelpers.AlmostEqual(samples, work, 10);
        }
    }
}
