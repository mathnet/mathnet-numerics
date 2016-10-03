// <copyright file="FiniteDifferenceCoefficientTests.cs" company="Math.NET">
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

using MathNet.Numerics.Differentiation;
using NUnit.Framework;

namespace MathNet.Numerics.UnitTests.DifferentiationTests
{

    [TestFixture, Category("Differentiation")]
    public class FiniteDifferenceCoefficientsTests
    {
        [Test]
        public void CentralDifferenceFirstOrderThreePointTest()
        {
            double[] results = { -0.5, 0, 0.5 };
            var finite = new FiniteDifferenceCoefficients(3);
            var coeff = finite.GetCoefficients(1, 1);
            Assert.AreEqual(results, coeff);
        }

        [Test]
        public void CentralDifferenceSecondOrderFivePointsTest()
        {
            double[] results = { (double)-1 / 12, (double)4 / 3, (double)-5 / 2, (double)4 / 3, (double)-1 / 12 };
            var finite = new FiniteDifferenceCoefficients(5);
            var coeff = finite.GetCoefficients(2, 2);
            for (int i = 0; i < coeff.Length; i++)
                Assert.AreEqual(results[i], coeff[i]);
        }

        [Test]
        public void ForwardDifferenceThirdOrderEightPointsTest()
        {
            double[] results = { (double)-967 / 120, (double)638 / 15, (double)-3929 / 40, (double)389 / 3,
                                 (double)-2545 / 24, (double)268 / 5, (double)-1849 / 120, (double)29 / 15 };
            var finite = new FiniteDifferenceCoefficients(8);
            var coeff = finite.GetCoefficients(0, 3);
            for (int i = 0; i < coeff.Length; i++)
                Assert.AreEqual(results[i], coeff[i]);
        }

        [Test]
        public void BackwardDifferenceThirdOrderFourPointsTest()
        {
            double[] results = { -1, 3, -3, 1 };
            var finite = new FiniteDifferenceCoefficients(4);
            var coeff = finite.GetCoefficients(3, 3);
            for (int i = 0; i < coeff.Length; i++)
                Assert.AreEqual(results[i], coeff[i]);

        }
    }
}
