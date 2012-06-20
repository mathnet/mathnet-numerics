// <copyright file="InverseTransformTest.cs" company="Math.NET">
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
    /// Inverse Transform test.
    /// </summary>
    [TestFixture]
    public class InverseTransformTest
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
        /// Verify if is reversible complex.
        /// </summary>
        /// <param name="count">Samples count.</param>
        /// <param name="maximumError">Maximum error value.</param>
        /// <param name="forward">Forward delegate.</param>
        /// <param name="inverse">Inverse delegate.</param>
        private void VerifyIsReversibleComplex(
            int count,
            double maximumError,
            Func<Complex[], Complex[]> forward,
            Func<Complex[], Complex[]> inverse)
        {
            var samples = SignalGenerator.Random((u, v) => new Complex(u, v), GetUniform(1), count);
            var work = new Complex[samples.Length];
            samples.CopyTo(work, 0);

            work = forward(work);

            Assert.IsFalse(work.AlmostEqualListWithError(samples, maximumError));

            work = inverse(work);

            AssertHelpers.AlmostEqualList(samples, work, maximumError);
        }

        /// <summary>
        /// Verify if is reversible real.
        /// </summary>
        /// <param name="count">Samples count.</param>
        /// <param name="maximumError">Maximum error value.</param>
        /// <param name="forward">Forward delegate.</param>
        /// <param name="inverse">Inverse delegate.</param>
        private void VerifyIsReversibleReal(
            int count,
            double maximumError,
            Func<double[], double[]> forward,
            Func<double[], double[]> inverse)
        {
            var samples = SignalGenerator.Random(x => x, GetUniform(1), count);
            var work = new double[samples.Length];
            samples.CopyTo(work, 0);

            work = forward(work);

            Assert.IsFalse(work.AlmostEqualListWithError(samples, maximumError));

            work = inverse(work);

            AssertHelpers.AlmostEqualList(samples, work, maximumError);
        }

        /// <summary>
        /// Fourier naive is reversible.
        /// </summary>
        /// <param name="options">Fourier options.</param>
        [TestCase(FourierOptions.Default)]
        [TestCase(FourierOptions.Matlab)]
        public void FourierNaiveIsReversible(FourierOptions options)
        {
            var dft = new DiscreteFourierTransform();

            VerifyIsReversibleComplex(
                0x80,
                1e-12,
                s => dft.NaiveForward(s, options),
                s => dft.NaiveInverse(s, options));
        }

        /// <summary>
        /// Fourier radix2xx is reversible.
        /// </summary>
        /// <param name="options">Fourier options.</param>
        [TestCase(FourierOptions.Default)]
        [TestCase(FourierOptions.Matlab)]
        public void FourierRadix2IsReversible(FourierOptions options)
        {
            var dft = new DiscreteFourierTransform();

            VerifyIsReversibleComplex(
                0x8000,
                1e-12,
                s =>
                {
                    dft.Radix2Forward(s, options);
                    return s;
                },
                s =>
                {
                    dft.Radix2Inverse(s, options);
                    return s;
                });
        }

        /// <summary>
        /// Fourier bluestein is reversible.
        /// </summary>
        /// <param name="options">Fourier options.</param>
        [TestCase(FourierOptions.Default)]
        [TestCase(FourierOptions.Matlab)]
        public void FourierBluesteinIsReversible(FourierOptions options)
        {
            var dft = new DiscreteFourierTransform();

            VerifyIsReversibleComplex(
                0x7FFF,
                1e-12,
                s =>
                {
                    dft.BluesteinForward(s, options);
                    return s;
                },
                s =>
                {
                    dft.BluesteinInverse(s, options);
                    return s;
                });
        }

        /// <summary>
        /// Hartley naive is reversible.
        /// </summary>
        /// <param name="options">Hartley options.</param>
        [TestCase(HartleyOptions.Default)]
        [TestCase(HartleyOptions.AsymmetricScaling)]
        public void HartleyNaiveIsReversible(HartleyOptions options)
        {
            var dht = new DiscreteHartleyTransform();

            VerifyIsReversibleReal(
                0x80,
                1e-9,
                s => dht.NaiveForward(s, options),
                s => dht.NaiveInverse(s, options));
        }

        /// <summary>
        /// Fourier default transform is reversible.
        /// </summary>
        [Test]
        public void FourierDefaultTransformIsReversible()
        {
            var samples = SignalGenerator.Random((u, v) => new Complex(u, v), GetUniform(1), 0x7FFF);
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
    }
}
