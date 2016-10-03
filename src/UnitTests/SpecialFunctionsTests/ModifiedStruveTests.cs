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

using NUnit.Framework;

namespace MathNet.Numerics.UnitTests.SpecialFunctionsTests
{
    /// <summary>
    /// Modified Struve functions tests.
    /// </summary>
    [TestFixture, Category("Functions")]
    public class ModifiedStruveTests
    {
        [TestCase(0.0, 0.0)]
        [TestCase(0.005, 0.003183107703788032)]
        [TestCase(0.5, 0.3272406993941808)]
        [TestCase(1.5, 1.216162510717182)]
        [TestCase(10.0, 2815.652249374595)]
        [TestCase(100.0, 1.073751707131074e+42)]
        [TestCase(-0.005, -0.003183107703788032)]
        [TestCase(-10.0, -2815.652249374595)]
        public void StruveL0Exact(double x, double expected)
        {
            AssertHelpers.AlmostEqualRelative(expected, SpecialFunctions.StruveL0(x), 13);
        }

        [TestCase(0.0, 0.0)]
        [TestCase(0.005, 5.305173611677443e-6)]
        [TestCase(0.5, 0.05394218262352266)]
        [TestCase(1.5, 0.5538569084469910)]
        [TestCase(10.0, 2670.358285208483)]
        [TestCase(100.0, 1.068369390338162e+42)]
        [TestCase(-0.005, 5.305173611677443e-6)]
        [TestCase(-10.0, 2670.358285208483)]
        public void StruveL1Exact(double x, double expected)
        {
            AssertHelpers.AlmostEqualRelative(expected, SpecialFunctions.StruveL1(x), 14);
        }

        [TestCase(0.0, 1.0)]
        [TestCase(0.1, 0.938769)]
        [TestCase(0.4, 0.781198)]
        [TestCase(2.0, 0.342152)]
        [TestCase(2.4, 0.289765)]
        [TestCase(4.5, 0.150279)]
        [TestCase(4.9, 0.136938)]
        [TestCase(10.0, 0.064379)]
        [TestCase(20.0, 0.031912)]
        //[TestCase(100.0, 0.006367)] Needs direct approximation
        public void BesselI0MStruveL0Exact(double x, double expected)
        {
            // Abramowitz/Stegun Table 12.1, 12.2
            AssertHelpers.AlmostEqualRelative(expected, SpecialFunctions.BesselI0MStruveL0(x), 4);
        }

        [TestCase(0.0, 0.0)]
        [TestCase(0.1, 0.047939)]
        [TestCase(0.4, 0.169710)]
        [TestCase(2.0, 0.487877)]
        [TestCase(2.4, 0.521712)]
        [TestCase(4.5, 0.600147)]
        [TestCase(4.9, 0.606142)]
        [TestCase(10.0, 0.630018)]
        [TestCase(20.0, 0.635016)]
        //[TestCase(100.0, 0.636556)] Needs direct approximation
        public void BesselI1MStruveL1Exact(double x, double expected)
        {
            // Abramowitz/Stegun Table 12.1, 12.2
            AssertHelpers.AlmostEqualRelative(expected, SpecialFunctions.BesselI1MStruveL1(x), 5);
        }
    }
}
