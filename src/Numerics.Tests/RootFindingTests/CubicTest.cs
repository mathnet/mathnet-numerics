// <copyright file="CubicTest.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2020 Math.NET
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

using MathNet.Numerics.RootFinding;
using NUnit.Framework;

namespace MathNet.Numerics.Tests.RootFindingTests
{
    [TestFixture, Category("RootFinding")]
    internal class CubicTest
    {
        [Test]
        public void RealRoots_SingleReal()
        {
            const double a0 = 1d;
            const double a1 = 5d;
            const double a2 = 2d;
            var roots = Cubic.RealRoots(a0, a1, a2);
            var root = roots.Item1;
            var funcValue = root * (root * (root + a2) + a1) + a0;
            Assert.That(funcValue, Is.EqualTo(0).Within(1e-14));
            Assert.AreEqual(double.NaN, roots.Item2);
            Assert.AreEqual(double.NaN, roots.Item3);
        }

        [Test]
        public void RealRoots_DoubleReal1()
        {
            const double a0 = -2d;
            const double a1 = 5d;
            const double a2 = -4d;
            var roots = Cubic.RealRoots(a0, a1, a2);
            Assert.That(roots.Item1, Is.EqualTo(2).Within(1e-14));
            Assert.That(roots.Item2, Is.EqualTo(1).Within(1e-14));
            Assert.AreEqual(double.NaN, roots.Item3);
        }

        [Test]
        public void RealRoots_DoubleReal2()
        {
            const double a0 = -4d;
            const double a1 = 8d;
            const double a2 = -5d;
            var roots = Cubic.RealRoots(a0, a1, a2);
            Assert.That(roots.Item1, Is.EqualTo(1).Within(1e-14));
            Assert.That(roots.Item2, Is.EqualTo(2).Within(1e-14));
            Assert.AreEqual(double.NaN, roots.Item3);
        }

        [Test]
        public void RealRoots_TripleReal()
        {
            const double a0 = 6d;
            const double a1 = -5d;
            const double a2 = -2d;
            var roots = Cubic.RealRoots(a0, a1, a2);
            Assert.That(roots.Item1, Is.EqualTo(3).Within(1e-14));
            Assert.That(roots.Item2, Is.EqualTo(-2).Within(1e-14));
            Assert.That(roots.Item3, Is.EqualTo(1).Within(1e-14));
        }

        [TestCase(6.0, -5.0, -2.0, 1.0, -2.0, 3.0, 1.0)]
        public void ComplexRoots_TripleReal(double d, double c, double b, double a, double x1, double x2, double x3)
        {
            var roots = FindRoots.Cubic(d, c, b, a);
            Assert.That(roots.Item1.Real, Is.EqualTo(x1).Within(1e-14));
            Assert.That(roots.Item1.Imaginary, Is.EqualTo(0).Within(1e-14));
            Assert.That(roots.Item2.Real, Is.EqualTo(x2).Within(1e-14));
            Assert.That(roots.Item2.Imaginary, Is.EqualTo(0).Within(1e-14));
            Assert.That(roots.Item3.Real, Is.EqualTo(x3).Within(1e-14));
            Assert.That(roots.Item3.Imaginary, Is.EqualTo(0).Within(1e-14));
        }

        [TestCase(6.0, -5.0, -2.0, 1.0, -2.0, 3.0, 1.0)]
        public void ComplexRoots_TripleReal_AsPolynomial(double d, double c, double b, double a, double x1, double x2, double x3)
        {
            var expectedReal = new[] { x1, x2, x3 };
            var actual = FindRoots.Polynomial(new[] { d, c, b, a });
            var actualReal = Generate.Map(actual, x => x.Real);
            var actualImag = Generate.Map(actual, x => x.Imaginary);
            Sorting.Sort(expectedReal);
            Sorting.Sort(actualReal, actualImag);
            Assert.That(actual.Length, Is.EqualTo(3));
            Assert.That(actualReal[0], Is.EqualTo(expectedReal[0]).Within(1e-14));
            Assert.That(actualImag[0], Is.EqualTo(0d).Within(1e-14));
            Assert.That(actualReal[1], Is.EqualTo(expectedReal[1]).Within(1e-14));
            Assert.That(actualImag[1], Is.EqualTo(0d).Within(1e-14));
            Assert.That(actualReal[2], Is.EqualTo(expectedReal[2]).Within(1e-14));
            Assert.That(actualImag[2], Is.EqualTo(0d).Within(1e-14));
        }

        [TestCase(-350.0, 162.0, -30.0, 2.0)]
        [TestCase(6.0, -5.0, -2.0, 1.0)]
        [TestCase(1.0, 5.0, 2.0, 1.0)]
        [TestCase(1.0, 5.0, 0.0, 1.0)]
        [TestCase(1.0, 0.0, 2.0, 1.0)]
        [TestCase(0.0, 0.0, 0.0, 2.0)]
        public void ComplexRootsAreRoots(double d, double c, double b, double a)
        {
            var roots = FindRoots.Cubic(d, c, b, a);
            Assert.That(Polynomial.Evaluate(roots.Item1, d, c, b, a).Real, Is.EqualTo(0).Within(1e-12));
            Assert.That(Polynomial.Evaluate(roots.Item1, d, c, b, a).Imaginary, Is.EqualTo(0).Within(1e-12));
            Assert.That(Polynomial.Evaluate(roots.Item2, d, c, b, a).Real, Is.EqualTo(0).Within(1e-12));
            Assert.That(Polynomial.Evaluate(roots.Item2, d, c, b, a).Imaginary, Is.EqualTo(0).Within(1e-12));
            Assert.That(Polynomial.Evaluate(roots.Item3, d, c, b, a).Real, Is.EqualTo(0).Within(1e-12));
            Assert.That(Polynomial.Evaluate(roots.Item3, d, c, b, a).Imaginary, Is.EqualTo(0).Within(1e-12));
        }

        [TestCase(-350.0, 162.0, -30.0, 2.0)]
        [TestCase(6.0, -5.0, -2.0, 1.0)]
        [TestCase(1.0, 5.0, 2.0, 1.0)]
        [TestCase(1.0, 5.0, 0.0, 1.0)]
        [TestCase(1.0, 0.0, 2.0, 1.0)]
        [TestCase(0.0, 0.0, 0.0, 2.0)]
        public void ComplexRootsAreRoots_AsPolynomial(double d, double c, double b, double a)
        {
            var roots = FindRoots.Polynomial(new[] { d, c, b, a });
            Assert.That(roots.Length, Is.EqualTo(3));
            Assert.That(Polynomial.Evaluate(roots[0], d, c, b, a).Real, Is.EqualTo(0).Within(1e-12));
            Assert.That(Polynomial.Evaluate(roots[0], d, c, b, a).Imaginary, Is.EqualTo(0).Within(1e-12));
            Assert.That(Polynomial.Evaluate(roots[1], d, c, b, a).Real, Is.EqualTo(0).Within(1e-12));
            Assert.That(Polynomial.Evaluate(roots[1], d, c, b, a).Imaginary, Is.EqualTo(0).Within(1e-12));
            Assert.That(Polynomial.Evaluate(roots[2], d, c, b, a).Real, Is.EqualTo(0).Within(1e-12));
            Assert.That(Polynomial.Evaluate(roots[2], d, c, b, a).Imaginary, Is.EqualTo(0).Within(1e-12));
        }
    }
}
