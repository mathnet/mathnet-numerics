// <copyright file="StandardErrorTest.cs" company="Math.NET">
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
    public class StandardErrorTest
    {
        [Test]
        public void ComputesPopulationStandardErrorOfTheRegression()
        {
            // Definition as described at: http://onlinestatbook.com/lms/regression/accuracy.html
            var xes = new[] { 1.0, 2, 3, 4, 5 };
            var ys = new[] { 1, 2, 1.3, 3.75, 2.25 };
            var fit = Fit.Line(xes, ys);
            var a = fit.Item1;
            var b = fit.Item2;
            var predictedYs = xes.Select(x => a + b * x);
            var standardError = Numerics.GoodnessOfFit.PopulationStandardError(predictedYs, ys);

            Assert.AreEqual(0.747, standardError, 1e-3);
        }

        [Test]
        public void ComputesStandardErrorOfTheRegression()
        {
            // Definition as described at: http://onlinestatbook.com/lms/regression/accuracy.html
            var xes = new[] { 1.0, 2, 3, 4, 5 };
            var ys = new[] { 1, 2, 1.3, 3.75, 2.25 };
            var fit = Fit.Line(xes, ys);
            var a = fit.Item1;
            var b = fit.Item2;
            var predictedYs = xes.Select(x => a + b * x);
            var standardError = Numerics.GoodnessOfFit.StandardError(predictedYs, ys, degreesOfFreedom: 2);

            Assert.AreEqual(0.964, standardError, 1e-3);
        }

        [Test]
        public void PopulationStandardErrorShouldThrowIfInputsSequencesDifferInLength()
        {
            var y1 = new[] { 0.0, 1 };
            var y2 = new[] { 1.0 };

            Assert.Throws<ArgumentOutOfRangeException>(() => Numerics.GoodnessOfFit.PopulationStandardError(y1, y2));
        }

        [Test]
        public void StandardErrorShouldThrowIfSampleSizeIsSmallerThanGivenDegreesOfFreedom()
        {
            var modelled = new[] { 1.0 };
            var observed = new[] { 1.0 };
            Assert.Throws<ArgumentOutOfRangeException>(() => Numerics.GoodnessOfFit.StandardError(modelled, observed, 2));
        }
    }
}
