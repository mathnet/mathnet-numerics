// <copyright file="ModifiedBesselTests.cs" company="Math.NET">
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
using NUnit.Framework;

namespace MathNet.Numerics.UnitTests.SpecialFunctionsTests
{
    /// <summary>
    /// Modified Bessel functions tests.
    /// </summary>
    [TestFixture, Category("Functions")]
    public class ModifiedBesselTests
    {
        [Test]
        public void BesselI0Approx([Range(-3.75, 3.75, 0.25)] double x)
        {
            // Approx by Abramowitz/Stegun 9.8.1
            Assert.AreEqual(Evaluate.Polynomial(x/3.75, 1.0, 0.0, 3.5156229, 0.0, 3.0899424, 0.0, 1.2067492, 0.0, 0.2659732, 0.0, 0.0360768, 0.0, 0.0045813), SpecialFunctions.BesselI0(x), 1e-7);
        }

        [TestCase(0.0, 1.0)]
        [TestCase(0.005, 1.000006250009766)]
        [TestCase(0.5, 1.063483370741324)]
        [TestCase(1.5, 1.646723189772891)]
        [TestCase(10.0, 2815.716628466254)]
        [TestCase(100.0, 1.073751707131074e+42)]
        [TestCase(-0.005, 1.000006250009766)]
        [TestCase(-10.0, 2815.716628466254)]
        public void BesselI0Exact(double x, double expected)
        {
            AssertHelpers.AlmostEqualRelative(expected, SpecialFunctions.BesselI0(x), 14);
        }

        [Test]
        public void BesselI1Approx([Range(-3.75, 3.75, 0.25)] double x)
        {
            // Approx by Abramowitz/Stegun 9.8.3
            Assert.AreEqual(Evaluate.Polynomial(x/3.75, 0.5, 0.0, 0.87890594, 0.0, 0.51498869, 0.0, 0.15084934, 0.0, 0.02658733, 0.0, 0.00301532, 0.0, 0.00032411)*x, SpecialFunctions.BesselI1(x), 1e-8);
        }

        [TestCase(0.0, 0.0)]
        [TestCase(0.005, 0.002500007812508138)]
        [TestCase(0.5, 0.2578943053908963)]
        [TestCase(1.5, 0.9816664285779076)]
        [TestCase(10.0, 2670.988303701255)]
        [TestCase(100.0, 1.068369390338162e+42)]
        [TestCase(-0.005, -0.002500007812508138)]
        [TestCase(-10.0, -2670.988303701255)]
        public void BesselI1Exact(double x, double expected)
        {
            AssertHelpers.AlmostEqualRelative(expected, SpecialFunctions.BesselI1(x), 14);
        }

        [Test]
        public void BesselK0Approx([Range(0.20, 2.0, 0.20)] double x)
        {
            // Approx by Abramowitz/Stegun 9.8.5
            Assert.AreEqual(Evaluate.Polynomial(x/2.0, -Math.Log(x/2.0)*SpecialFunctions.BesselI0(x) - 0.57721566, 0.0, 0.42278420, 0.0, 0.23069756, 0.0, 0.03488590, 0.0, 0.00262698, 0.0, 0.00010750, 0.0, 0.00000740), SpecialFunctions.BesselK0(x), 1e-8);
        }

        [TestCase(1e-10, 23.14178244559887)]
        [TestCase(1e-5, 11.62885698094436)]
        [TestCase(0.005, 5.414288971329485)]
        [TestCase(0.5, 0.9244190712276659)]
        [TestCase(1.5, 0.2138055626475257)]
        [TestCase(10.0, 0.00001778006231616765)]
        [TestCase(100.0, 4.656628229175902e-45)]
        public void BesselK0Exact(double x, double expected)
        {
            AssertHelpers.AlmostEqualRelative(expected, SpecialFunctions.BesselK0(x), 14);
        }

        [Test]
        public void BesselK1Approx([Range(0.20, 2.0, 0.20)] double x)
        {
            // Approx by Abramowitz/Stegun 9.8.7
            Assert.AreEqual(Evaluate.Polynomial(x/2.0, x*Math.Log(x/2.0)*SpecialFunctions.BesselI1(x) + 1.0, 0.0, 0.15443144, 0.0, -0.67278579, 0.0, -0.18156897, 0.0, -0.01919402, 0.0, -0.00110404, 0.0, -0.00004686), SpecialFunctions.BesselK1(x)*x, 1e-8);
        }

        [TestCase(1e-10, 1.0e+10)]
        [TestCase(1e-5, 99999.99993935572)]
        [TestCase(0.005, 199.9852143257300)]
        [TestCase(0.5, 1.656441120003301)]
        [TestCase(1.5, 0.2773878004568438)]
        [TestCase(10.0, 0.00001864877345382558)]
        [TestCase(100.0, 4.679853735636909e-45)]
        public void BesselK1Exact(double x, double expected)
        {
            AssertHelpers.AlmostEqualRelative(expected, SpecialFunctions.BesselK1(x), 14);
        }
    }
}
