// <copyright file="FitTests.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2018 Math.NET
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

using MathNet.Numerics.LinearRegression;
using MathNet.Numerics.Statistics;
using NUnit.Framework;

namespace MathNet.Numerics.UnitTests
{
    [TestFixture, Category("Regression")]
    public class FitTests
    {
        [Test]
        public void FitsToExactLineWhenPointsAreOnLine()
        {
            var x = new[] {30.0, 40.0, 50.0, 12.0, -3.4, 100.5};
            var y = x.Select(z => 4.0 - 1.5*z).ToArray();

            var resp = Fit.Line(x, y);
            Assert.AreEqual(4.0, resp.Item1, 1e-12);
            Assert.AreEqual(-1.5, resp.Item2, 1e-12);

            var resf = Fit.LineFunc(x, y);
            foreach (var z in Enumerable.Range(-3, 10))
            {
                Assert.AreEqual(4.0 - 1.5*z, resf(z), 1e-12);
            }
        }

        [Test]
        public void FitsToBestLine()
        {
            // Mathematica: Fit[{{1,4.986},{2,2.347},{3,2.061},{4,-2.995},{5,-2.352},{6,-5.782}}, {1, x}, x]
            // -> 7.01013 - 2.08551*x

            var x = Enumerable.Range(1, 6).Select(Convert.ToDouble).ToArray();
            var y = new[] {4.986, 2.347, 2.061, -2.995, -2.352, -5.782};

            var resp = Fit.Line(x, y);
            Assert.AreEqual(7.01013, resp.Item1, 1e-4);
            Assert.AreEqual(-2.08551, resp.Item2, 1e-4);

            var resf = Fit.LineFunc(x, y);
            foreach (var z in Enumerable.Range(-3, 10))
            {
                Assert.AreEqual(7.01013 - 2.08551 * z, resf(z), 1e-4);
            }
        }

        [Test]
        public void FitsToBestLineThroughOrigin()
        {
            // Mathematica: Fit[{{1,4.986},{2,2.347},{3,2.061},{4,-2.995},{5,-2.352},{6,-5.782}}, {x}, x]
            // -> -0.467791 x

            var x = Enumerable.Range(1, 6).Select(Convert.ToDouble).ToArray();
            var y = new[] { 4.986, 2.347, 2.061, -2.995, -2.352, -5.782 };

            var resp = Fit.LineThroughOrigin(x, y);
            Assert.AreEqual(-0.467791, resp, 1e-4);

            var resf = Fit.LineThroughOriginFunc(x, y);
            foreach (var z in Enumerable.Range(-3, 10))
            {
                Assert.AreEqual(-0.467791 * z, resf(z), 1e-4);
            }

            var respSeq = SimpleRegression.FitThroughOrigin(Generate.Map2(x, y, Tuple.Create));
            Assert.AreEqual(-0.467791, respSeq, 1e-4);
        }

        [Test]
        public void FitsToLineSameAsExcelTrendLine()
        {
            // X	Y
            // 1   0.2
            // 2   0.3
            // 4   1.3
            // 6   4.2
            // -> y = -1.078 + 0.7932*x

            var x = new[] { 1.0, 2.0, 4.0, 6.0 };
            var y = new[] { 0.2, 0.3, 1.3, 4.2 };

            var resp = Fit.Line(x, y);
            Assert.AreEqual(-1.078, resp.Item1, 1e-3);
            Assert.AreEqual(0.7932, resp.Item2, 1e-3);

            var resf = Fit.LineFunc(x, y);
            foreach (var z in Enumerable.Range(-3, 10))
            {
                Assert.AreEqual(-1.078 + 0.7932*z, resf(z), 1e-2);
            }
        }

        [Test]
        public void FitsToExponentialSameAsExcelTrendLine()
        {
            // X	Y
            // 1   0.2
            // 2   0.3
            // 4   1.3
            // 6   4.2
            // -> y = 0.0981*exp(0.6284*x)

            var x = new[] { 1.0, 2.0, 4.0, 6.0 };
            var y = new[] { 0.2, 0.3, 1.3, 4.2 };

            var resp = Fit.Exponential(x, y);
            Assert.AreEqual(0.0981, resp.Item1, 1e-3);
            Assert.AreEqual(0.6284, resp.Item2, 1e-3);

            var resf = Fit.ExponentialFunc(x, y);
            foreach (var z in Enumerable.Range(-3, 10))
            {
                Assert.AreEqual(0.0981 * Math.Exp(0.6284 * z), resf(z), 1e-2);
            }
        }

        [Test]
        public void FitsToLogarithmSameAsExcelTrendLine()
        {
            // X	Y
            // 1   0.2
            // 2   0.3
            // 4   1.3
            // 6   4.2
            // -> y = -0.4338 + 1.9981*ln(x)

            var x = new[] { 1.0, 2.0, 4.0, 6.0 };
            var y = new[] { 0.2, 0.3, 1.3, 4.2 };

            var resp = Fit.Logarithm(x, y);
            Assert.AreEqual(-0.4338, resp.Item1, 1e-3);
            Assert.AreEqual(1.9981, resp.Item2, 1e-3);

            var resf = Fit.LogarithmFunc(x, y);
            foreach (var z in Enumerable.Range(-3, 10))
            {
                Assert.AreEqual(-0.4338 + 1.9981 * Math.Log(z), resf(z), 1e-2);
            }
        }

        [Test]
        public void FitsToPowerSameAsExcelTrendLine()
        {
            // X	Y
            // 1   0.2
            // 2   0.3
            // 4   1.3
            // 6   4.2
            // -> y = 0.1454*x^1.7044

            var x = new[] { 1.0, 2.0, 4.0, 6.0 };
            var y = new[] { 0.2, 0.3, 1.3, 4.2 };

            var resp = Fit.Power(x, y);
            Assert.AreEqual(0.1454, resp.Item1, 1e-3);
            Assert.AreEqual(1.7044, resp.Item2, 1e-3);

            var resf = Fit.PowerFunc(x, y);
            foreach (var z in Enumerable.Range(-3, 10))
            {
                Assert.AreEqual(0.1454 * Math.Pow(z, 1.7044), resf(z), 1e-2);
            }
        }

        [Test]
        public void FitsToOrder2PolynomialSameAsExcelTrendLine()
        {
            // X	Y
            // 1   0.2
            // 2   0.3
            // 4   1.3
            // 6   4.2
            // -> y = 0.7101 - 0.6675*x + 0.2077*x^2

            var x = new[] { 1.0, 2.0, 4.0, 6.0 };
            var y = new[] { 0.2, 0.3, 1.3, 4.2 };

            var resp = Fit.Polynomial(x, y, 2);
            Assert.AreEqual(0.7101, resp[0], 1e-3);
            Assert.AreEqual(-0.6675, resp[1], 1e-3);
            Assert.AreEqual(0.2077, resp[2], 1e-3);

            var resf = Fit.PolynomialFunc(x, y, 2);
            foreach (var z in Enumerable.Range(-3, 10))
            {
                Assert.AreEqual(0.7101 - 0.6675*z + 0.2077*z*z, resf(z), 1e-2);
            }
        }

        [Test]
        public void FitsToMeanOnOrder0Polynomial()
        {
            // Mathematica: Fit[{{1,4.986},{2,2.347},{3,2.061},{4,-2.995},{5,-2.352},{6,-5.782}}, {1}, x]
            // -> -0.289167

            var x = Enumerable.Range(1, 6).Select(Convert.ToDouble).ToArray();
            var y = new[] { 4.986, 2.347, 2.061, -2.995, -2.352, -5.782 };

            var resp = Fit.Polynomial(x, y, 0);
            Assert.AreEqual(1, resp.Length);
            Assert.AreEqual(-0.289167, resp[0], 1e-4);
            Assert.AreEqual(y.Mean(), resp[0], 1e-4);
        }

        [Test]
        public void FitsToLineOnOrder1Polynomial()
        {
            // Mathematica: Fit[{{1,4.986},{2,2.347},{3,2.061},{4,-2.995},{5,-2.352},{6,-5.782}}, {1, x}, x]
            // -> 7.01013 - 2.08551 x

            var x = Enumerable.Range(1, 6).Select(Convert.ToDouble).ToArray();
            var y = new[] { 4.986, 2.347, 2.061, -2.995, -2.352, -5.782 };

            var resp = Fit.Polynomial(x, y, 1);
            Assert.AreEqual(2, resp.Length);
            Assert.AreEqual(7.01013, resp[0], 1e-4);
            Assert.AreEqual(-2.08551, resp[1], 1e-4);

            var resf = Fit.PolynomialFunc(x, y, 1);
            foreach (var z in Enumerable.Range(-3, 10))
            {
                Assert.AreEqual(7.01013 - 2.08551 * z, resf(z), 1e-4);
            }
        }

        [Test]
        public void FitsToOrder2Polynomial()
        {
            // Mathematica: Fit[{{1,4.986},{2,2.347},{3,2.061},{4,-2.995},{5,-2.352},{6,-5.782}}, {1, x, x^2}, x]
            // -> 6.9703 - 2.05564 x - 0.00426786 x^2

            var x = Enumerable.Range(1, 6).Select(Convert.ToDouble).ToArray();
            var y = new[] { 4.986, 2.347, 2.061, -2.995, -2.352, -5.782 };

            var resp = Fit.Polynomial(x, y, 2);
            Assert.AreEqual(3, resp.Length);
            Assert.AreEqual(6.9703, resp[0], 1e-4);
            Assert.AreEqual(-2.05564, resp[1], 1e-4);
            Assert.AreEqual(-0.00426786, resp[2], 1e-6);

            var resf = Fit.PolynomialFunc(x, y, 2);
            foreach (var z in Enumerable.Range(-3, 10))
            {
                Assert.AreEqual(Evaluate.Polynomial(z, resp), resf(z), 1e-4);
            }
        }

        [Test]
        public void FitsToOrder2PolynomialWeighted()
        {
            // Mathematica: LinearModelFit[{{1,4.986},{2,2.347},{3,2.061},{4,-2.995},{5,-2.352},{6,-5.782}}, {1, x, x^2}, x, Weights -> {0.5, 1.0, 1.0, 1.0, 1.0, 0.5}]
            // -> 7.01451 - 2.12819 x + 0.0115 x^2

            var x = Enumerable.Range(1, 6).Select(Convert.ToDouble).ToArray();
            var y = new[] { 4.986, 2.347, 2.061, -2.995, -2.352, -5.782 };

            var respUnweighted = Fit.PolynomialWeighted(x, y, new[] { 1.0, 1.0, 1.0, 1.0, 1.0, 1.0 }, 2);
            Assert.AreEqual(3, respUnweighted.Length);
            Assert.AreEqual(6.9703, respUnweighted[0], 1e-4);
            Assert.AreEqual(-2.05564, respUnweighted[1], 1e-4);
            Assert.AreEqual(-0.00426786, respUnweighted[2], 1e-6);

            var resp = Fit.PolynomialWeighted(x, y, new[] { 0.5, 1.0, 1.0, 1.0, 1.0, 0.5 }, 2);
            Assert.AreEqual(3, resp.Length);
            Assert.AreEqual(7.01451, resp[0], 1e-4);
            Assert.AreEqual(-2.12819, resp[1], 1e-4);
            Assert.AreEqual(0.0115, resp[2], 1e-6);
        }

        [Test]
        public void FitsToTrigonometricLinearCombination()
        {
            // Mathematica: Fit[{{1,4.986},{2,2.347},{3,2.061},{4,-2.995},{5,-2.352},{6,-5.782}}, {1, sin(x), cos(x)}, x]
            // -> 4.02159 sin(x) - 1.46962 cos(x) - 0.287476

            var x = Enumerable.Range(1, 6).Select(Convert.ToDouble).ToArray();
            var y = new[] { 4.986, 2.347, 2.061, -2.995, -2.352, -5.782 };

            var resp = Fit.LinearCombination(x, y, z => 1.0, Math.Sin, Math.Cos);
            Assert.AreEqual(3, resp.Length);
            Assert.AreEqual(-0.287476, resp[0], 1e-4);
            Assert.AreEqual(4.02159, resp[1], 1e-4);
            Assert.AreEqual(-1.46962, resp[2], 1e-4);

            var resf = Fit.LinearCombinationFunc(x, y, z => 1.0, Math.Sin, Math.Cos);
            foreach (var z in Enumerable.Range(-3, 10))
            {
                Assert.AreEqual(4.02159*Math.Sin(z) - 1.46962*Math.Cos(z) - 0.287476, resf(z), 1e-4);
            }
        }
    }
}
