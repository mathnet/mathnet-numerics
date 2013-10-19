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

using MathNet.Numerics.Distributions;
using MathNet.Numerics.IntegralTransforms;
using MathNet.Numerics.IntegralTransforms.Algorithms;
using MathNet.Numerics.Signals;
using NUnit.Framework;

namespace MathNet.Numerics.UnitTests.IntegralTransformsTests
{
    using Random = System.Random;

#if NOSYSNUMERICS
    using Complex = Numerics.Complex;
#else
    using Complex = System.Numerics.Complex;
#endif

    /// <summary>
    /// Inverse Transform test.
    /// </summary>
    [TestFixture]
    public class InverseTransformTest
    {
        /// <summary>
        /// Continuous uniform distribution.
        /// </summary>
        IContinuousDistribution GetUniform(int seed)
        {
            return new ContinuousUniform(-1, 1, new Random(seed));
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

            var samples = SignalGenerator.Random((u, v) => new Complex(u, v), GetUniform(1), 0x80);
            var work = new Complex[samples.Length];
            samples.CopyTo(work, 0);

            work = dft.NaiveForward(work, options);
            Assert.IsFalse(work.ListAlmostEqual(samples, 6));

            work = dft.NaiveInverse(work, options);
            AssertHelpers.ListAlmostEqual(samples, work, 12);
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

            var samples = SignalGenerator.Random((u, v) => new Complex(u, v), GetUniform(1), 0x8000);
            var work = new Complex[samples.Length];
            samples.CopyTo(work, 0);

            dft.Radix2Forward(work, options);
            Assert.IsFalse(work.ListAlmostEqual(samples, 6));

            dft.Radix2Inverse(work, options);
            AssertHelpers.ListAlmostEqual(samples, work, 12);
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

            var samples = SignalGenerator.Random((u, v) => new Complex(u, v), GetUniform(1), 0x7FFF);
            var work = new Complex[samples.Length];
            samples.CopyTo(work, 0);

            dft.BluesteinForward(work, options);
            Assert.IsFalse(work.ListAlmostEqual(samples, 6));

            dft.BluesteinInverse(work, options);
            AssertHelpers.ListAlmostEqual(samples, work, 10);
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

            var samples = SignalGenerator.Random(x => x, GetUniform(1), 0x80);
            var work = new double[samples.Length];
            samples.CopyTo(work, 0);

            work = dht.NaiveForward(work, options);
            Assert.IsFalse(work.ListAlmostEqual(samples, 6));

            work = dht.NaiveInverse(work, options);
            AssertHelpers.ListAlmostEqual(samples, work, 12);
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
            Assert.IsFalse(work.ListAlmostEqual(samples, 6));

            Transform.FourierInverse(work);
            AssertHelpers.ListAlmostEqual(samples, work, 10);

            Transform.FourierInverse(work, FourierOptions.Default);
            Assert.IsFalse(work.ListAlmostEqual(samples, 6));

            Transform.FourierForward(work, FourierOptions.Default);
            AssertHelpers.ListAlmostEqual(samples, work, 10);
        }
    }
}
