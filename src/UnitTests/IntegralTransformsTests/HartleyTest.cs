// <copyright file="HartleyTest.cs" company="Math.NET">
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
    /// Hartley tests.
    /// </summary>
    [TestFixture]
    public class HartleyTest
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
        /// Verify if matches DFT.
        /// </summary>
        /// <param name="samples">Samples array.</param>
        /// <param name="maximumError">Maximum error value.</param>
        /// <param name="inverse">Is inverse.</param>
        /// <param name="dft">DFT function delegate.</param>
        /// <param name="hartley">Hartley transform delegate.</param>
        private static void VerifyMatchesDft(
            double[] samples,
            double maximumError,
            bool inverse,
            Action<Complex[]> dft,
            Func<double[], double[]> hartley)
        {
            var hartleyReal = hartley(samples);

            var fourierComplex = ArrayHelpers.ConvertAll(samples, s => new Complex(s, inverse ? -s : s));
            dft(fourierComplex);
            var fourierReal = ArrayHelpers.ConvertAll(fourierComplex, s => s.Real);

            AssertHelpers.AlmostEqualList(fourierReal, hartleyReal, maximumError);
        }

        /// <summary>
        /// Native matches DFT.
        /// </summary>
        /// <param name="hartleyOptions">Hartley transformation options.</param>
        /// <param name="fourierOptions">Fourier transformation options.</param>
        [TestCase(HartleyOptions.Default, FourierOptions.Default)]
        [TestCase(HartleyOptions.AsymmetricScaling, FourierOptions.AsymmetricScaling)]
        [TestCase(HartleyOptions.NoScaling, FourierOptions.NoScaling)]
        public void NaiveMatchesDft(HartleyOptions hartleyOptions, FourierOptions fourierOptions)
        {
            var dht = new DiscreteHartleyTransform();
            var samples = SignalGenerator.Random(x => x, GetUniform(1), 0x80);

            VerifyMatchesDft(
                samples,
                1e-5,
                false,
                s => Transform.FourierForward(s, fourierOptions),
                s => dht.NaiveForward(s, hartleyOptions));
            VerifyMatchesDft(
                samples,
                1e-5,
                true,
                s => Transform.FourierInverse(s, fourierOptions),
                s => dht.NaiveInverse(s, hartleyOptions));
        }
    }
}
