// <copyright file="DhtTest.cs" company="Math.NET">
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

    [TestFixture]
    public class DhtTest
    {
        private static readonly Random _random = new Random();

        private static double[] ProvideSamples(int count)
        {
            var samples = new double[count];
            for (int i = 0; i < samples.Length; i++)
            {
                samples[i] = 1 - (2 * _random.NextDouble());
            }

            return samples;
        }

        [Test]
        [Row(HartleyOptions.Default, FourierOptions.Default)]
        [Row(HartleyOptions.AsymmetricScaling, FourierOptions.AsymmetricScaling)]
        [Row(HartleyOptions.NoScaling, FourierOptions.NoScaling)]
        public void NaiveMatchesDFT(HartleyOptions hartleyOptions, FourierOptions fourierOptions)
        {
            var samples = ProvideSamples(0x80);

            var dht = new DiscreteHartleyTransform();
            var hartley = dht.NaiveForward(samples, hartleyOptions);

            var fourierComplex = Array.ConvertAll(samples, s => new Complex(s, s));
            Transform.FourierForward(fourierComplex, fourierOptions);
            var fourierReal = Array.ConvertAll(fourierComplex, s => s.Real);

            AssertHelpers.AlmostEqualList(fourierReal, hartley, 1e-10);
        }

        [Test]
        [Row(HartleyOptions.Default)]
        [Row(HartleyOptions.AsymmetricScaling)]
        public void NaiveIsReversible(HartleyOptions options)
        {
            var samples = ProvideSamples(0x80);
            var work = new double[samples.Length];
            samples.CopyTo(work, 0);

            var dht = new DiscreteHartleyTransform();
            work = dht.NaiveForward(work, options);

            Assert.IsFalse(work.AlmostEqualListWithError(samples, 1e-10));

            work = dht.NaiveInverse(work, options);

            AssertHelpers.AlmostEqualList(samples, work, 1e-10);
        }
    }
}
