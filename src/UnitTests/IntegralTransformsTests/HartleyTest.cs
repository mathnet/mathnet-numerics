// <copyright file="HartleyTest.cs" company="Math.NET">
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
    using System.Numerics;
    using Distributions;
    using IntegralTransforms;
    using IntegralTransforms.Algorithms;
    using MbUnit.Framework;
    using Sampling;

    [TestFixture]
    public class HartleyTest
    {
        private IContinuousDistribution _uniform = new ContinuousUniform(-1, 1);

        private static void VerifyMatchesDFT(
            double[] samples,
            double maximumError,
            bool inverse,
            Action<Complex[]> dft,
            Func<double[], double[]> hartley)
        {
            var hartleyReal = hartley(samples);

            var fourierComplex = Array.ConvertAll(samples, s => new Complex(s, inverse ? -s : s));
            dft(fourierComplex);
            var fourierReal = Array.ConvertAll(fourierComplex, s => s.Real);

            AssertHelpers.AlmostEqualList(fourierReal, hartleyReal, maximumError);
        }

        [Test]
        [Row(HartleyOptions.Default, FourierOptions.Default)]
        [Row(HartleyOptions.AsymmetricScaling, FourierOptions.AsymmetricScaling)]
        [Row(HartleyOptions.NoScaling, FourierOptions.NoScaling)]
        public void NaiveMatchesDFT(HartleyOptions hartleyOptions, FourierOptions fourierOptions)
        {
            var dht = new DiscreteHartleyTransform();
            var samples = Sample.Random(x => x, _uniform, 0x80);

            VerifyMatchesDFT(
                samples,
                1e-10,
                false,
                s => Transform.FourierForward(s, fourierOptions),
                s => dht.NaiveForward(s, hartleyOptions));

            VerifyMatchesDFT(
                samples,
                1e-10,
                true,
                s => Transform.FourierInverse(s, fourierOptions),
                s => dht.NaiveInverse(s, hartleyOptions));
        }
    }
}