// <copyright file="BrentTest.cs" company="Math.NET">
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
using MathNet.Numerics.RootFinding;
using NUnit.Framework;

namespace MathNet.Numerics.UnitTests.RootFindingTests
{
    [TestFixture, Category("RootFinding")]
    public class SecantTest
    {
        [Test]
        public void MultipleRoots()
        {
            // Roots at -2, 2
            Func<double, double> f1 = x => x * x - 4;
            Assert.AreEqual(0, f1(Secant.FindRoot(f1, 0, 6, -10, 10, 1e-14)));
            Assert.AreEqual(-2, Secant.FindRoot(f1, -3, 0, -10, 10, 1e-14));
            Assert.AreEqual(2, Secant.FindRoot(f1, 1, 4, -10, 10, 1e-14));
            Assert.AreEqual(0, f1(Secant.FindRoot(x => f1(x), -6, 0, -10, 10, 1e-14)));
            Assert.AreEqual(-2, Secant.FindRoot(x => f1(x), -5, -1, -10, 10, 1e-14));
            Assert.AreEqual(2, Secant.FindRoot(x => f1(x), 1, 4, -10, 10, 1e-14));

            // Roots at 3, 4
            Func<double, double> f2 = x => (x - 3) * (x - 4);
            Assert.AreEqual(0, f2(Secant.FindRoot(f2, 3.5, 6, 0, 10, 1e-14)));
            Assert.AreEqual(3, Secant.FindRoot(f2, -5, 3.5, -10, 10, 1e-14));
            Assert.AreEqual(4, Secant.FindRoot(f2, 3.2, 4.2, 0, 10, 1e-14));
            Assert.AreEqual(3, Secant.FindRoot(f2, 2.1, 3.1, 0, 10, 0.001, 50), 0.001);
            Assert.AreEqual(3, Secant.FindRoot(f2, 2.1, 3.4, 0, 10, 0.001, 50), 0.001);
        }

        [Test]
        public void LocalMinima()
        {
            Func<double, double> f1 = x => x * x * x - 2 * x + 2;
            Assert.AreEqual(0, f1(Secant.FindRoot(f1, -5, 6, -10, 10, 1e-14)));
        }

        [Test]
        public void Cubic()
        {
            // with complex roots (looking for the real root only): 3x^3 + 4x^2 + 5x + 6
            Func<double, double> f1 = x => Evaluate.Polynomial(x, 6, 5, 4, 3);
            Assert.AreEqual(-1.265328088928, Secant.FindRoot(f1, -2, -1, -10, 10, 1e-10), 1e-6);
            Assert.AreEqual(-1.265328088928, Secant.FindRoot(f1, -5, 6, -10, 10, 1e-10), 1e-6);

            // real roots only: 2x^3 + 4x^2 - 50x + 6
            Func<double, double> f2 = x => Evaluate.Polynomial(x, 6, -50, 4, 2);
            Assert.AreEqual(-6.1466562197069, Secant.FindRoot(f2, -8, -5, -10, 10, 1e-10), 1e-6);
            Assert.AreEqual(0.12124737195841, Secant.FindRoot(f2, -1, 1, -10, 10, 1e-10), 1e-6);
            Assert.AreEqual(4.0254088477485, Secant.FindRoot(f2, 3, 5, 0, 10, 1e-10), 1e-6);
        }

        [Test]
        public void NoRoot()
        {
            Func<double, double> f1 = x => x * x + 4;
            Assert.That(() => Secant.FindRoot(f1, -5, 5, -10, 10, 1e-14, 50), Throws.TypeOf<NonConvergenceException>());
        }
    }
}
