// <copyright file="PolynomialTests.cs" company="Math.NET">
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Complex = System.Numerics.Complex;
using System.Runtime.Serialization;
using NUnit.Framework;

namespace MathNet.Numerics.Tests
{
    /// <Note>
    /// some of these tests were inspired by numpys tests in python for the Polynomial functions.
    /// Thanks to the numpy contributers!
    /// </Note>
    [TestFixture, Category("Calculus")]
    public class PolynomialTests
    {

        [TestCase(new double[] { 5, 4, 3, 0, 2 }, "5 + 4x + 3x^2 + 2x^4")]
        [TestCase(new double[0], "0")]
        [TestCase(new double[] { 0, 4, 3, 0, 0 }, "4x + 3x^2")]
        [TestCase(new double[] { 0, -4, 3 }, "-4x + 3x^2")]
        [TestCase(new double[] { 0, -4, -3 }, "-4x - 3x^2")]
        [TestCase(new double[] { 0, 4, -3 }, "4x - 3x^2")]
        public void ToStringTest(double[] x, string expected)
        {
            var p = new Polynomial(x);
            Assert.AreEqual(expected, p.ToString());
        }

        [TestCase(new double[] { 5, 4, 3, 0, 2 }, "2x^4 + 3x^2 + 4x + 5")]
        [TestCase(new double[0], "0")]
        [TestCase(new double[] { 0, 4, 3, 0, 0 }, "3x^2 + 4x")]
        [TestCase(new double[] { 0, -4, 3 }, "3x^2 - 4x")]
        [TestCase(new double[] { 0, -4, -3 }, "-3x^2 - 4x")]
        [TestCase(new double[] { 0, 4, -3 }, "-3x^2 + 4x")]
        public void ToStringTestDescending(double[] x, string expected)
        {
            var p = new Polynomial(x);
            Assert.AreEqual(expected, p.ToStringDescending());
        }

        [TestCase(new double[] { 5, 4, 3, 0, 2 }, new double[] { 4*1, 3*2, 0*3, 2*4 })]
        [TestCase(new double[0], new double[0])]
        [TestCase(new double[] { 0, 4, 3, 0, 0 }, new double[] { 4*1, 3*2 })]
        public void DifferentiateTest(double[] x, double[] expected)
        {
            var p = new Polynomial(x);
            var p_res = p.Differentiate();

            Assert.AreEqual(expected.Length, p_res.Coefficients.Length, "length mismatch");
            for (int k = 0; k < p_res.Coefficients.Length; k++)
            {
                Assert.AreEqual(expected[k], p_res.Coefficients[k], "idx: " + k + " mismatch");
            }
        }

        [TestCase(new double[] { 5, 4, 3, 0, 2 }, new double[] { 0, 5.0/1.0, 4.0/2.0, 3.0/3.0, 0.0/4.0, 2.0/5.0 })]
        [TestCase(new double[0], new double[0])]
        [TestCase(new double[] { 0, 1, 6, 8 }, new double[] {0, 0.0/1.0, 1.0/2.0, 6.0/3.0, 8.0/4.0})]
        public void IntegrateTest(double[] x, double[] expected)
        {
            var p = new Polynomial(x);
            var p_res = p.Integrate();

            Assert.AreEqual(expected.Length, p_res.Coefficients.Length, "length mismatch");
            for (int k = 0; k < p_res.Coefficients.Length; k++)
            {
                Assert.AreEqual(expected[k], p_res.Coefficients[k], "idx: " + k + " mismatch");
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

                    var p_res = Polynomial.Add(p1, p2);
                    var p_tar = new Polynomial(tgt);

                    Assert.AreEqual(p_tar.Coefficients.Length, p_res.Coefficients.Length, "length mismatch");
                    for (int k = 0; k < p_res.Coefficients.Length; k++)
                    {
                        Assert.AreEqual(p_tar.Coefficients[k], p_res.Coefficients[k], msg);
                    }
                }
            }
        }

        [Test]
        public void SubtractTest()
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

                    var p_res = Polynomial.Subtract(p1, p2);
                    var p_tar = new Polynomial(tgt);

                    Assert.AreEqual(p_tar.Coefficients.Length, p_res.Coefficients.Length, "length mismatch");
                    for (int k = 0; k < p_res.Coefficients.Length; k++)
                    {
                        Assert.AreEqual(p_tar.Coefficients[k], p_res.Coefficients[k], msg);
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

                    Assert.AreEqual(p_tar.Coefficients.Length, p_res.Coefficients.Length, "length mismatch");
                    for (int k = 0; k < p_res.Coefficients.Length; k++)
                    {
                        Assert.AreEqual(p_tar.Coefficients[k], p_res.Coefficients[k], msg);
                    }
                }
            }
        }

        // 2020-10-07 jbialogrodzki #730 This test focuses particularly on the issue at hand,
        // i.e. multiplication of zero polynomials, but also attempts to provide more thorough UT
        // for multiplication in general.
        [TestCase(new double[] { }, new double[] { }, new double[] { })]
        [TestCase(new double[] { 0 }, new double[] { }, new double[] { })]
        [TestCase(new double[] { 1 }, new double[] { }, new double[] { })]
        [TestCase(new double[] { 1, 2, 3 }, new double[] { }, new double[] { })]
        [TestCase(new double[] { 0 }, new double[] { 0 }, new double[] { })]
        [TestCase(new double[] { 1 }, new double[] { 0 }, new double[] { })]
        [TestCase(new double[] { 1, 2, 3 }, new double[] { 0 }, new double[] { })]
        [TestCase(new double[] { 2 }, new double[] { 3 }, new double[] { 6 })]
        [TestCase(new double[] { 2, 3 }, new double[] { 4 }, new double[] { 8, 12 })]
        [TestCase(new double[] { 2, 3 }, new double[] { 4, 5 }, new double[] { 8, 22, 15 })]
        public void MultiplyTest2(double[] cLeft, double[] cRight, double[] cExpected)
        {

            var left = new Polynomial(cLeft);
            var right = new Polynomial(cRight);
            var expected = new Polynomial(cExpected);

            var actualLR = left * right;
            PolynomialTests.TestEqual(actualLR, expected);

            var actualRL = right * left;
            PolynomialTests.TestEqual(actualRL, expected);

        }

        [TestCase(new double[] { 5, 4, 0 }, "5 + 4x")]
        [TestCase(new double[] { 0, 0, 0 }, "0")]
        [TestCase(new double[] { 5, 4, 3, 0, 2 }, "5 + 4x + 3x^2 + 2x^4")]
        [TestCase(new double[] { 0, 0, 8, 0, 0 }, "8x^2")]
        [TestCase(new double[] { 0, 4, 3, 0, 0 }, "4x + 3x^2")]
        public void TrimTest(double[] x, string expected)
        {
            var p = new Polynomial(x);
            Assert.AreEqual(expected, p.ToString());

        }

        [Test]
        public void DivideLongTestWrongInputs()
        {
            Assert.Throws(typeof(DivideByZeroException), () =>
            {
                var p1 = new Polynomial(1.0d);
                var p2 = Polynomial.Zero;
                GC.KeepAlive(Polynomial.DivideRemainder(p1, p2));
            });
            Assert.DoesNotThrow(() =>
            {
                var p1 = Polynomial.Zero;
                var p2 = new Polynomial(1.0d);
                GC.KeepAlive(Polynomial.DivideRemainder(p1, p2));
            });
            Assert.Throws(typeof(DivideByZeroException), () =>
            {
                var p1 = Polynomial.Zero;
                var p2 = Polynomial.Zero;
                GC.KeepAlive(Polynomial.DivideRemainder(p1, p2));
            });
            Assert.Throws(typeof(DivideByZeroException), () =>
            {
                var p1 = new Polynomial(1.0d);
                var p2 = Polynomial.Zero;
                GC.KeepAlive(Polynomial.DivideRemainder(p1, p2));
            });
        }

        [Test]
        public void DivideLongTest()
        {
            var p11 = new Polynomial(2.0d);
            var p21 = new Polynomial(2.0d);
            var tpl1 = Polynomial.DivideRemainder(p11, p21);
            TestEqual(new double[] { 1.0 }, tpl1.Item1);
            TestEqual(new double[0], tpl1.Item2);

            var p12 = new Polynomial(2.0d, 2.0d);
            var p22 = new Polynomial(2.0d);
            var tpl2 = Polynomial.DivideRemainder(p12, p22);
            TestEqual(new double[] { 1.0, 1.0 }, tpl2.Item1);
            TestEqual(new double[0], tpl2.Item2);

            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    var msg = String.Format("At i={0}, j={1}", i, j);
                    var ci = new double[i + 2];
                    var cj = new double[j + 2];
                    ci[ci.Length - 1] = 2;
                    ci[ci.Length - 2] = 1;
                    cj[cj.Length - 1] = 2;
                    cj[cj.Length - 2] = 1;

                    var pi = new Polynomial(ci);
                    var pj = new Polynomial(cj);
                    var tgt = Polynomial.Add(pi, pj);
                    var tpl3 = Polynomial.DivideRemainder(tgt, pi);
                    var pquo = tpl3.Item1;
                    var prem = tpl3.Item2;
                    var pres = (pquo * pi) + prem;

                    TestEqual(pres, tgt, msg);
                }
            }
        }

        [Test]
        public void GetRootsTest()
        {
            var tol = 1e-14;

            // 0 = 1 -> no roots
            var p1 = new Polynomial(1.0);
            var r = p1.Roots();
            Assert.AreEqual(0, r.Length, "length mismatch");

            // 0 = 1 + 2*x -> single root at -1/2
            var p2 = new Polynomial(1, 2);
            var r2 = p2.Roots();
            Assert.AreEqual(1, r2.Length, "length mismatch");
            Assert.AreEqual(-0.5, r2.FirstOrDefault().Real, tol);

            // T.G: the following expected values were generated using
            // numpys np.roots(x) method
            // which is equivalent to np.polynomial.polynomial.polyroots

            var x_2 = new double[] { -1.0, 1.0 };
            var expected_2 = new List<Complex>();
            expected_2.Add(new Complex(1.0, 0.0));
            TestRootsEqual(x_2, expected_2);

            var x_3 = new double[] { -1.0, 0.0, 1.0 };
            var expected_3 = new List<Complex>();
            expected_3.Add(new Complex(1.0, 0.0));
            expected_3.Add(new Complex(-1.0, 0.0));
            TestRootsEqual(x_3, expected_3);

            var x_4 = new double[] { -1.0, -0.33333333333333337, 0.33333333333333326, 1.0 };
            var expected_4 = new List<Complex>();
            expected_4.Add(new Complex(0.9999999999999996, 0.0));
            expected_4.Add(new Complex(-0.6666666666666666, 0.7453559924999296));
            expected_4.Add(new Complex(-0.6666666666666666, -0.7453559924999296));
            TestRootsEqual(x_4, expected_4);
        }

        static void TestRootsEqual(double[] x, List<Complex> eIn)
        {
            var tol = 1e-10;
            var r0 = new Polynomial(x).Roots().ToList();

            var e = eIn.OrderBy(v => v.Real).ToArray();
            var r = r0.OrderBy(v => v.Real).ToArray();

            Assert.IsNotNull(r);
            Assert.AreEqual(e.Length, r.Length, "Length mismatch");
            for (int k = 0; k < r.Length; k++)
            {
                var msg = String.Format("At k={0}", k);
                Assert.AreEqual(e[k].Real, r[k].Real, tol, msg);
                Assert.AreEqual(e[k].Imaginary, r[k].Imaginary, tol, msg);
            }
        }

        [Test]
        public void DataContractSerializationTest()
        {
            Polynomial expected = new Polynomial(1.0d, 2.0d, 0.0d, 3.0d) { VariableName = "z" };

            var serializer = new DataContractSerializer(typeof(Polynomial));
            var stream = new MemoryStream();
            serializer.WriteObject(stream, expected);
            stream.Position = 0;

            var actual = (Polynomial)serializer.ReadObject(stream);

            Assert.That(actual.Degree, Is.EqualTo(expected.Degree));
            Assert.That(actual.VariableName, Is.EqualTo(expected.VariableName));
            Assert.That(actual, Is.EqualTo(expected));
            Assert.That(actual, Is.Not.SameAs(expected));
        }

        static void TestEqual(double[] p_tar, double[] p_res, string msg = null)
        {
            Assert.AreEqual(p_tar.Length, p_res.Length, "Length mismatch");
            for (int k = 0; k < p_res.Length; k++)
            {
                Assert.AreEqual(p_tar[k], p_res[k], msg);
            }
        }

        static void TestEqual(double[] p_tar, Polynomial p_res, string msg = null)
        {
            Assert.AreEqual(p_tar.Length, p_res.Degree + 1, "Degree mismatch");
            for (int k = 0; k <= p_res.Degree; k++)
            {
                Assert.AreEqual(p_tar[k], p_res.Coefficients[k], msg);
            }
        }

        static void TestEqual(Polynomial p_tar, Polynomial p_res, string msg = null)
        {
            Assert.AreEqual(p_tar.Degree, p_res.Degree, "Degree mismatch");
            for (int k = 0; k <= p_res.Degree; k++)
            {
                Assert.AreEqual(p_tar.Coefficients[k], p_res.Coefficients[k], msg);
            }
        }

        // 2020-10-07 jbialogrodzki #730 This test focuses particularly on the issue at hand,
        // i.e. evaluating zero polynomials, but also attempts to provide some UT for evaluation
        // in general (as there has been none). Note the Complex API is tested with real values only.
        [TestCase(new double[] { }, 0, 0)]
        [TestCase(new double[] { }, 123, 0)]
        [TestCase(new double[] { 0 }, 0, 0)]
        [TestCase(new double[] { 0 }, 123, 0)]
        [TestCase(new double[] { 1 }, 0, 1)]
        [TestCase(new double[] { 1 }, 123, 1)]
        [TestCase(new double[] { 2 }, 0, 2)]
        [TestCase(new double[] { 2 }, 123, 2)]
        [TestCase(new double[] { 1, 2 }, 0, 1)]
        [TestCase(new double[] { 1, 2 }, 3, 7)]
        [TestCase(new double[] { 1, 2, 3 }, 0, 1)]
        [TestCase(new double[] { 1, 2, 3 }, 4, 57)]
        public void EvaluateTest(double[] c, double z, double expected)
        {

            Complex DoubleToComplex(double value) => new Complex(value, 0);

            var cComplex = c.Select(DoubleToComplex).ToArray();
            var zComplex = DoubleToComplex(z);
            var expectedComplex = DoubleToComplex(expected);

            var p = new Polynomial(c);

            // static double Evaluate(double, double[])
            {
                var actual = Polynomial.Evaluate(z, c);
                Assert.AreEqual(expected, actual);
            }

            // static Complex Evaluate(Complex, double[])
            {
                var actual = Polynomial.Evaluate(zComplex, c);
                Assert.AreEqual(expectedComplex, actual);
            }

            // static Complex Evaluate(Complex, Complex[])
            {
                var actual = Polynomial.Evaluate(zComplex, cComplex);
                Assert.AreEqual(expectedComplex, actual);
            }

            // double Evaluate(double)
            {
                var actual = p.Evaluate(z);
                Assert.AreEqual(expected, actual);
            }

            // Complex Evaluate(Complex)
            {
                var actual = p.Evaluate(zComplex);
                Assert.AreEqual(expectedComplex, actual);
            }

        }

#if NET5_0_OR_GREATER
        [Test]
        public void JsonDeserializationTest()
        {
            var polynomial = new Polynomial(0, 1, 2);
            var json = System.Text.Json.JsonSerializer.Serialize(polynomial);
            var deserialize = System.Text.Json.JsonSerializer.Deserialize<Polynomial>(json);
            Assert.NotNull(deserialize);
            Assert.AreEqual(polynomial.Coefficients.Length, deserialize.Coefficients.Length);
            Assert.IsTrue(polynomial.Coefficients.SequenceEqual(deserialize.Coefficients));
        }
#endif

    }

}
