// <copyright file="BisectionTest.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
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
using MathNet.Numerics.RootFinding.Algorithms;
using NUnit.Framework;

namespace MathNet.Numerics.UnitTests.RootFindingTests
{
    [TestFixture]
    internal class BisectionTest
    {
        [Test]
        public void MultipleRoots()
        {
            // Roots at -2, 2
            Func<double, double> f1 = x => x * x - 4;
            Assert.AreEqual(0, f1(Bisection.FindRoot(f1, 0, 5, 1e-14)));
            Assert.AreEqual(0, f1(Bisection.FindRootExpand(f1, 3, 4, 1e-14)));
            Assert.AreEqual(-2, Bisection.FindRoot(f1, -5, -1, 1e-14));
            Assert.AreEqual(2, Bisection.FindRoot(f1, 1, 4, 1e-14));
            Assert.AreEqual(0, f1(Bisection.FindRoot(x => -f1(x), 0, 5, 1e-14)));
            Assert.AreEqual(-2, Bisection.FindRoot(x => -f1(x), -5, -1, 1e-14));
            Assert.AreEqual(2, Bisection.FindRoot(x => -f1(x), 1, 4, 1e-14));

            // Roots at 3, 4
            Func<double, double> f2 = x => (x - 3) * (x - 4);
            Assert.AreEqual(0, f2(Bisection.FindRoot(f2, 3.5, 5, 1e-14)), 1e-14);
            Assert.AreEqual(3, Bisection.FindRoot(f2, -5, 3.5, 1e-14));
            Assert.AreEqual(4, Bisection.FindRoot(f2, 3.2, 5, 1e-14));
            Assert.AreEqual(3, Bisection.FindRoot(f2, 2.1, 3.9, 0.001), 0.001);
            Assert.AreEqual(3, Bisection.FindRoot(f2, 2.1, 3.4, 0.001), 0.001);
        }

        [Test]
        public void LocalMinima()
        {
            Func<double, double> f1 = x => x * x * x - 2 * x + 2;
            Assert.AreEqual(0, f1(Bisection.FindRoot(f1, -5, 5, 1e-14)), 1e-14);
            Assert.AreEqual(0, f1(Bisection.FindRoot(f1, -2, 4, 1e-14)), 1e-14);
        }

        [Test]
        public void NoRoot()
        {
            Func<double, double> f1 = x => x * x + 4;
            Assert.Throws<NonConvergenceException>(() => Bisection.FindRoot(f1, -5, 5, 1e-14));
        }
    }
}
