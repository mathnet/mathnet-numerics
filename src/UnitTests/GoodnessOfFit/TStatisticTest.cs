// <copyright file="TStatisticTest.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2013 Math.NET
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
using System.Linq;
using NUnit.Framework;

namespace MathNet.Numerics.UnitTests.GoodnessOfFit
{
    [TestFixture, Category("Regression")]
    public class TStatisticTest
    {
        [Test]
        public void ComputesTStatisticForSlopeCoefficient()
        {
            var tstats = GetTStatistics();
            Assert.AreEqual(37.4058, tstats.Item2, 1e-3);
        }

        [Test]
        public void ComputesTStatisticForInterceptTerm()
        {
            var tstats = GetTStatistics();
            Assert.AreEqual(4.1752, tstats.Item1, 1e-3);
        }

        [Test]
        public void TStatisticsShouldThrowIfSampleSizeIsSmallerThanDegreesOfFreedom()
        {
            var independent = new[] {1.0 };
            var dependent = new[] { 1.0 };
            var predictions = new[] { 1.0 };
            var slope = 1.0;
            var intercept = 1.0;

            Assert.Throws<ArgumentOutOfRangeException>(() => Numerics.GoodnessOfFit.TStatistics(slope, intercept, independent, dependent, predictions));
        }

        private static double[] xes()
        {
            return new[] { 50, 53, 54, 55, 56, 59, 62, 65, 67, 71, 72, 74, 75, 76, 79, 80, 82, 85, 87, 90, 93, 94, 95, 97, 100 }
                .Select(Convert.ToDouble)
                .ToArray();
        }

        private static double[] ys()
        {
            return new[] { 122, 118, 128, 121, 125, 136, 144, 142, 149, 161, 167, 168, 162, 171, 175, 182, 180, 183, 188, 200, 194, 206, 207, 210, 219 }
                .Select(Convert.ToDouble)
                .ToArray();
        }

        private Tuple<double, double> GetTStatistics()
        {
            var line = Fit.Line(xes(), ys());
            var intercept = line.Item1;
            var slope = line.Item2;
            var predictions = xes().Select(x => intercept + x * slope).ToArray();
            var tstats = Numerics.GoodnessOfFit.TStatistics(slope, intercept, xes(), ys(), predictions);
            return tstats;
        }
    }
}

