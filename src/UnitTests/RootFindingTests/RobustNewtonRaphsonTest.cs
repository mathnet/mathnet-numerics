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
    public class RobustNewtonRaphsonTest
    {
        [Test]
        public void MultipleRoots()
        {
            // Roots at -2, 2
            Func<double, double> f1 = x => x * x - 4;
            Func<double, double> df1 = x => 2 * x;
            Assert.AreEqual(0, f1(RobustNewtonRaphson.FindRoot(f1, df1, -5, 5, 1e-14, 100, 20)));
            Assert.AreEqual(-2, RobustNewtonRaphson.FindRoot(f1, df1, -5, -1, 1e-14, 100, 20));
            Assert.AreEqual(2, RobustNewtonRaphson.FindRoot(f1, df1, 1, 4, 1e-14, 100, 20));
            Assert.AreEqual(0, f1(RobustNewtonRaphson.FindRoot(x => -f1(x), x => -df1(x), -5, 5, 1e-14, 100, 20)));
            Assert.AreEqual(-2, RobustNewtonRaphson.FindRoot(x => -f1(x), x => -df1(x), -5, -1, 1e-14, 100, 20));
            Assert.AreEqual(2, RobustNewtonRaphson.FindRoot(x => -f1(x), x => -df1(x), 1, 4, 1e-14, 100, 20));

            // Roots at 3, 4
            Func<double, double> f2 = x => (x - 3) * (x - 4);
            Func<double, double> df2 = x => 2 * x - 7;
            Assert.AreEqual(0, f2(RobustNewtonRaphson.FindRoot(f2, df2, -5, 5, 1e-14, 100, 20)));
            Assert.AreEqual(3, RobustNewtonRaphson.FindRoot(f2, df2, -5, 3.5, 1e-14, 100, 20));
            Assert.AreEqual(4, RobustNewtonRaphson.FindRoot(f2, df2, 3.2, 5, 1e-14, 100, 20));
            Assert.AreEqual(3, RobustNewtonRaphson.FindRoot(f2, df2, 2.1, 3.9, 0.001, 50, 20), 0.001);
            Assert.AreEqual(3, RobustNewtonRaphson.FindRoot(f2, df2, 2.1, 3.4, 0.001, 50, 20), 0.001);
        }

        [Test]
        public void LocalMinima()
        {
            Func<double, double> f1 = x => x * x * x - 2 * x + 2;
            Func<double, double> df1 = x => 3 * x * x - 2;
            Assert.AreEqual(0, f1(RobustNewtonRaphson.FindRoot(f1, df1, -5, 5, 1e-14, 100, 20)));
            Assert.AreEqual(0, f1(RobustNewtonRaphson.FindRoot(f1, df1, -2, 4, 1e-14, 100, 20)));
        }

        [Test]
        public void Pole()
        {
            Func<double, double> f1 = x => 1/(x - 2) + 2;
            Func<double, double> df1 = x => -1/(x*x - 4*x + 4);
            Assert.AreEqual(1.5, RobustNewtonRaphson.FindRoot(f1, df1, 1, 2, 1e-14, 100, 20));
            Assert.AreEqual(1.5, RobustNewtonRaphson.FindRoot(f1, df1, 1, 6, 1e-14, 100, 20));
            Assert.AreEqual(1.5, FindRoots.OfFunctionDerivative(f1, df1, 1, 6));

            Func<double, double> f2 = x => -1/(x - 2) + 2;
            Func<double, double> df2 = x => 1/(x*x - 4*x + 4);
            Assert.AreEqual(2.5, RobustNewtonRaphson.FindRoot(f2, df2, 2, 3, 1e-14, 100, 20));
            Assert.AreEqual(2.5, RobustNewtonRaphson.FindRoot(f2, df2, -2, 3, 1e-14, 100, 20));
            Assert.AreEqual(2.5, FindRoots.OfFunctionDerivative(f2, df2, -2, 3));

            Func<double, double> f3 = x => 1/(x - 2) + x + 2;
            Func<double, double> df3 = x => -1/(x*x - 4*x + 4) + 1;
            Assert.AreEqual(-Constants.Sqrt3, RobustNewtonRaphson.FindRoot(f3, df3, -2, -1, 1e-14, 100, 20), 1e-14);
            Assert.AreEqual(Constants.Sqrt3, RobustNewtonRaphson.FindRoot(f3, df3, 1, 1.99, 1e-14, 100, 20));
            Assert.AreEqual(Constants.Sqrt3, RobustNewtonRaphson.FindRoot(f3, df3, -1.5, 1.99, 1e-14, 100, 20));
            Assert.AreEqual(Constants.Sqrt3, RobustNewtonRaphson.FindRoot(f3, df3, 1, 6, 1e-14, 100, 20));

            Func<double, double> f4 = x => 1/(2 - x) - x + 6;
            Func<double, double> df4 = x => 1/(x*x - 4*x + 4) - 1;
            Assert.AreEqual(4 + Constants.Sqrt3, RobustNewtonRaphson.FindRoot(f4, df4, 5, 6, 1e-14, 100, 20), 1e-14);
            Assert.AreEqual(4 - Constants.Sqrt3, RobustNewtonRaphson.FindRoot(f4, df4, 2.01, 3, 1e-14, 100, 20));
            Assert.AreEqual(4 - Constants.Sqrt3, RobustNewtonRaphson.FindRoot(f4, df4, 2.01, 5, 1e-14, 100, 20));
            Assert.AreEqual(4 - Constants.Sqrt3, RobustNewtonRaphson.FindRoot(f4, df4, -2, 4, 1e-14, 100, 20));
        }

        [Test]
        public void Cubic()
        {
            // with complex roots (looking for the real root only): 3x^3 + 4x^2 + 5x + 6, derivative 9x^2 + 8x + 5
            Func<double, double> f1 = x => Evaluate.Polynomial(x, 6, 5, 4, 3);
            Func<double, double> df1 = x => Evaluate.Polynomial(x, 5, 8, 9);
            Assert.AreEqual(-1.265328088928, RobustNewtonRaphson.FindRoot(f1, df1, -2, -1, 1e-10, 100, 20), 1e-6);
            Assert.AreEqual(-1.265328088928, RobustNewtonRaphson.FindRoot(f1, df1, -5, 5, 1e-10, 100, 20), 1e-6);

            // real roots only: 2x^3 + 4x^2 - 50x + 6, derivative 6x^2 + 8x - 50
            Func<double, double> f2 = x => Evaluate.Polynomial(x, 6, -50, 4, 2);
            Func<double, double> df2 = x => Evaluate.Polynomial(x, -50, 8, 6);
            Assert.AreEqual(-6.1466562197069, RobustNewtonRaphson.FindRoot(f2, df2, -8, -5, 1e-10, 100, 20), 1e-6);
            Assert.AreEqual(0.12124737195841, RobustNewtonRaphson.FindRoot(f2, df2, -1, 1, 1e-10, 100, 20), 1e-6);
            Assert.AreEqual(4.0254088477485, RobustNewtonRaphson.FindRoot(f2, df2, 3, 5, 1e-10, 100, 20), 1e-6);
        }

        [Test]
        public void NoRoot()
        {
            Func<double, double> f1 = x => x * x + 4;
            Func<double, double> df1 = x => 2 * x;
            Assert.That(() => RobustNewtonRaphson.FindRoot(f1, df1, -5, 5, 1e-14, 50, 20), Throws.TypeOf<NonConvergenceException>());
        }
    }
}
