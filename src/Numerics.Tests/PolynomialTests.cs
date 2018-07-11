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
using MathNet.Numerics;
using MathNet.Numerics.LinearRegression;
using MathNet.Numerics.Statistics;
using NUnit.Framework;

namespace MathNet.Numerics.UnitTests
{
    [TestFixture, Category("Calculus")]
    public class PolynomialTests
    {

        [TestCase(new double[] { 5, 4, 3, 0, 2 }, "5 + 4x^1 + 3x^2 + 0x^3 + 2x^4")]
        [TestCase(new double[0], "")]
        [TestCase(new double[] { 0, 4, 3, 0, 0 }, "0 + 4x^1 + 3x^2 + 0x^3 + 0x^4")]
        public void ToStringTest(double[] x, string expected)
        {
            var p = new Polynomial(x);
            Assert.AreEqual(expected, p.ToString());
        }

        [TestCase(new double[] { 5, 4, 3, 0, 2 }, new double[] { 4*1, 3*2, 0*3, 2*4 })]
        [TestCase(new double[0], null)]
        [TestCase(new double[] { 0, 4, 3, 0, 0 }, new double[] { 4*1, 3*2 })]
        public void DifferentiateTest(double[] x, double[] expected)
        {
            var p = new Polynomial(x);
            var p_res = p.Differentiate();

            if (expected == null)
            {
                Assert.IsNull(p_res);
                return;
            }
            else
            {
                Assert.AreEqual(expected.Length, p_res.Length, "length mismatch");
                for (int k = 0; k < p_res.Length; k++)
                {
                    Assert.AreEqual(expected[k], p_res.Coeffs[k], "idx: " + k + " mismatch");
                }
            }
            
        }

        [TestCase(new double[] { 5, 4, 3, 0, 2 }, new double[] { 0, 5.0/1.0, 4.0/2.0, 3.0/3.0, 0.0/4.0, 2.0/5.0 })]
        [TestCase(new double[0], new double[1] { 0 })]
        [TestCase(new double[] { 0, 1, 6, 8 }, new double[] {0, 0.0/1.0, 1.0/2.0, 6.0/3.0, 8.0/4.0})]
        public void IntegrateTest(double[] x, double[] expected)
        {
            var p = new Polynomial(x);
            var p_res = p.Integrate();

            if (expected == null)
            {
                Assert.IsNull(p_res);
                return;
            }
            else
            {
                Assert.AreEqual(expected.Length, p_res.Length, "length mismatch");
                for (int k = 0; k < p_res.Length; k++)
                {
                    Assert.AreEqual(expected[k], p_res.Coeffs[k], "idx: " + k + " mismatch");
                }
            }
        }

        [Test]
        public void AddTest()
        {
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    var msg = String.Format("At i={0}, j={1}", i, j);
                    var n = Math.Max(i, j) + 1;
                    var tgt = new double[n];
                    tgt[i] += 1;
                    tgt[j] += 1;

                    var c1 = new double[i + 1];
                    var c2 = new double[j + 1];

                    c1[i] = 1.0;
                    c2[j] = 1.0;

                    var p1 = new Polynomial(c1);
                    var p2 = new Polynomial(c2);

                    var p_res = Polynomial.add(p1, p2);
                    var p_tar = new Polynomial(tgt);

                    p_res.CutTrailZeros();
                    p_tar.CutTrailZeros();

                    Assert.AreEqual(p_tar.Length, p_res.Length, "length mismatch");
                    for (int k = 0; k < p_res.Length; k++)
                    {
                        Assert.AreEqual(p_tar.Coeffs[k], p_res.Coeffs[k], msg);
                    }
                }
            }
        }

        [Test]
        public void SubstractTest()
        {
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    var msg = String.Format("At i={0}, j={1}", i, j);
                    var n = Math.Max(i, j) + 1;
                    var tgt = new double[n];
                    tgt[i] += 1;
                    tgt[j] -= 1;

                    var c1 = new double[i + 1];
                    var c2 = new double[j + 1];

                    c1[i] = 1.0;
                    c2[j] = 1.0;

                    var p1 = new Polynomial(c1);
                    var p2 = new Polynomial(c2);

                    var p_res = Polynomial.substract(p1, p2);
                    var p_tar = new Polynomial(tgt);

                    p_res.CutTrailZeros();
                    p_tar.CutTrailZeros();

                    Assert.AreEqual(p_tar.Length, p_res.Length, "length mismatch");
                    for (int k = 0; k < p_res.Length; k++)
                    {
                        Assert.AreEqual(p_tar.Coeffs[k], p_res.Coeffs[k], msg);
                    }
                }
            }
        }

        [Test]
        public void MultiplyTest()
        {
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    var msg = String.Format("At i={0}, j={1}", i, j);
                    var n = i + j + 1;
                    var tgt = new double[n];
                    tgt[i + j] += 1;

                    var c1 = new double[i + 1];
                    var c2 = new double[j + 1];

                    c1[i] = 1.0;
                    c2[j] = 1.0;

                    var p1 = new Polynomial(c1);
                    var p2 = new Polynomial(c2);

                    var p_res = p1 * p2;
                    var p_tar = new Polynomial(tgt);

                    p_res.CutTrailZeros();
                    p_tar.CutTrailZeros();

                    Assert.AreEqual(p_tar.Length, p_res.Length, "length mismatch");
                    for (int k = 0; k < p_res.Length; k++)
                    {
                        Assert.AreEqual(p_tar.Coeffs[k], p_res.Coeffs[k], msg);
                    }
                }
            }
        }
    }
}
