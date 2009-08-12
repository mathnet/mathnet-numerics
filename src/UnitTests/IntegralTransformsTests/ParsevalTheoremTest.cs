// <copyright file="ParsevalTheoremTest.cs" company="Math.NET">
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

using MathNet.Numerics.IntegralTransforms.Algorithms;

namespace MathNet.Numerics.UnitTests.IntegralTransformsTests
{
    using System.Linq;
    using MbUnit.Framework;
    using IntegralTransforms;
    using Statistics;

    [TestFixture]
    public class ParsevalTheoremTest
    {
        [Test]
        [Row(0x1000)]
        [Row(0x7FF)]
        public void FourierDefaultTransformSatisfiesParsevalsTheorem(int count)
        {
            var samples = SampleProvider.ProvideComplexSamples(count);

            var timeSpaceEnergy = (from s in samples select s.ModulusSquared).Mean();

            var work = new Complex[samples.Length];
            samples.CopyTo(work, 0);

            // Default -> Symmetric Scaling
            Transform.FourierForward(work);

            var frequencySpaceEnergy = (from s in work select s.ModulusSquared).Mean();

            Assert.AreApproximatelyEqual(timeSpaceEnergy, frequencySpaceEnergy, 1e-12);
        }

        [Test]
        [Row(0x40)]
        [Row(0x1F)]
        public void HartleyDefaultNaiveSatisfiesParsevalsTheorem(int count)
        {
            var samples = SampleProvider.ProvideRealSamples(count);

            var timeSpaceEnergy = (from s in samples select s * s).Mean();

            var work = new double[samples.Length];
            samples.CopyTo(work, 0);

            // Default -> Symmetric Scaling
            var dht = new DiscreteHartleyTransform();
            work = dht.NaiveForward(work, HartleyOptions.Default);

            var frequencySpaceEnergy = (from s in work select s * s).Mean();

            Assert.AreApproximatelyEqual(timeSpaceEnergy, frequencySpaceEnergy, 1e-12);
        }
    }
}
