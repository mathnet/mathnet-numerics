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
    public class NewtonRaphsonTest
    {
        [Test]
        public void MultipleRoots()
        {
            // Roots at -2, 2
            Func<double, double> f1 = x => x * x - 4;
            Func<double, double> df1 = x => 2 * x;
            Assert.AreEqual(0, f1(NewtonRaphson.FindRoot(f1, df1, -5, 6, 1e-14)));
            Assert.AreEqual(-2, NewtonRaphson.FindRoot(f1, df1, -5, -1, 1e-14));
            Assert.AreEqual(2, NewtonRaphson.FindRoot(f1, df1, 1, 4, 1e-14));
            Assert.AreEqual(0, f1(NewtonRaphson.FindRoot(x => -f1(x), x => -df1(x), -5, 6, 1e-14)));
            Assert.AreEqual(-2, NewtonRaphson.FindRoot(x => -f1(x), x => -df1(x), -5, -1, 1e-14));
            Assert.AreEqual(2, NewtonRaphson.FindRoot(x => -f1(x), x => -df1(x), 1, 4, 1e-14));

            // Roots at 3, 4
            Func<double, double> f2 = x => (x - 3) * (x - 4);
            Func<double, double> df2 = x => 2 * x - 7;
            Assert.AreEqual(0, f2(NewtonRaphson.FindRoot(f2, df2, -5, 6, 1e-14)));
            Assert.AreEqual(3, NewtonRaphson.FindRoot(f2, df2, -5, 3.5, 1e-14));
            Assert.AreEqual(4, NewtonRaphson.FindRoot(f2, df2, 3.2, 5, 1e-14));
            Assert.AreEqual(3, NewtonRaphson.FindRoot(f2, df2, 2.1, 3.9, 0.001, 50), 0.001);
            Assert.AreEqual(3, NewtonRaphson.FindRoot(f2, df2, 2.1, 3.4, 0.001, 50), 0.001);
        }

        [Test]
        public void MultipleRootsNearGuess()
        {
            // Roots at -2, 2
            Func<double, double> f1 = x => x * x - 4;
            Func<double, double> df1 = x => 2 * x;
            Assert.AreEqual(0, f1(NewtonRaphson.FindRootNearGuess(f1, df1, 0.5, accuracy:1e-14)));
            Assert.AreEqual(-2, NewtonRaphson.FindRootNearGuess(f1, df1, -3.0, accuracy: 1e-14));
            Assert.AreEqual(2, NewtonRaphson.FindRootNearGuess(f1, df1, 2.5, accuracy: 1e-14));
            Assert.AreEqual(0, f1(NewtonRaphson.FindRootNearGuess(x => -f1(x), x => -df1(x), 0.6, accuracy: 1e-14)));
            Assert.AreEqual(-2, NewtonRaphson.FindRootNearGuess(x => -f1(x), x => -df1(x), -3, accuracy: 1e-14));
            Assert.AreEqual(2, NewtonRaphson.FindRootNearGuess(x => -f1(x), x => -df1(x), 2.5, accuracy: 1e-14));

            // Roots at 3, 4
            Func<double, double> f2 = x => (x - 3) * (x - 4);
            Func<double, double> df2 = x => 2 * x - 7;
            Assert.AreEqual(0, f2(NewtonRaphson.FindRootNearGuess(f2, df2, 0.5, accuracy: 1e-14)));
            Assert.AreEqual(3, NewtonRaphson.FindRootNearGuess(f2, df2, -0.75, accuracy: 1e-14));
            Assert.AreEqual(4, NewtonRaphson.FindRootNearGuess(f2, df2, 4.1, accuracy: 1e-14));
            Assert.AreEqual(3, NewtonRaphson.FindRootNearGuess(f2, df2, 3, accuracy: 0.001, maxIterations: 50), 0.001);
            Assert.AreEqual(3, NewtonRaphson.FindRootNearGuess(f2, df2, 2.75, accuracy: 0.001, maxIterations: 50), 0.001);
        }

        [Test]
        public void LocalMinima()
        {
            Func<double, double> f1 = x => x * x * x - 2 * x + 2;
            Func<double, double> df1 = x => 3 * x * x - 2;
            Assert.AreEqual(0, f1(NewtonRaphson.FindRoot(f1, df1, -5, 6, 1e-14)));
            //Assert.AreEqual(0, f1(NewtonRaphson.FindRoot(f1, df1, 1, -2, 4, 1e-14, 100)));
        }

        [Test]
        public void Cubic()
        {
            // with complex roots (looking for the real root only): 3x^3 + 4x^2 + 5x + 6, derivative 9x^2 + 8x + 5
            Func<double, double> f1 = x => Evaluate.Polynomial(x, 6, 5, 4, 3);
            Func<double, double> df1 = x => Evaluate.Polynomial(x, 5, 8, 9);
            Assert.AreEqual(-1.265328088928, NewtonRaphson.FindRoot(f1, df1, -2, -1, 1e-10), 1e-6);
            Assert.AreEqual(-1.265328088928, NewtonRaphson.FindRoot(f1, df1, -5, 5, 1e-10), 1e-6);

            // real roots only: 2x^3 + 4x^2 - 50x + 6, derivative 6x^2 + 8x - 50
            Func<double, double> f2 = x => Evaluate.Polynomial(x, 6, -50, 4, 2);
            Func<double, double> df2 = x => Evaluate.Polynomial(x, -50, 8, 6);
            Assert.AreEqual(-6.1466562197069, NewtonRaphson.FindRoot(f2, df2, -8, -5, 1e-10), 1e-6);
            Assert.AreEqual(0.12124737195841, NewtonRaphson.FindRoot(f2, df2, -1, 1, 1e-10), 1e-6);
            Assert.AreEqual(4.0254088477485, NewtonRaphson.FindRoot(f2, df2, 3, 5, 1e-10), 1e-6);
        }

        [Test]
        public void NoRoot()
        {
            Func<double, double> f1 = x => x * x + 4;
            Func<double, double> df1 = x => 2 * x;
            Assert.That(() => NewtonRaphson.FindRoot(f1, df1, -5, 5, 1e-14, 50), Throws.TypeOf<NonConvergenceException>());
        }
    }
}
