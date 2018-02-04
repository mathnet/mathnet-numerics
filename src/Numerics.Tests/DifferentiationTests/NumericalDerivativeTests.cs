// <copyright file="NumericalDerivativeTests.cs" company="Math.NET">
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
using MathNet.Numerics.Differentiation;
using NUnit.Framework;

namespace MathNet.Numerics.UnitTests.DifferentiationTests
{

    [TestFixture, Category("Differentiation")]
    class NumericalDerivativeTests
    {
        [Test]
        public void SinFirstDerivativeAtZeroTest()
        {
            Func<double, double> f = Math.Sin;
            var df = new NumericalDerivative();
            Assert.AreEqual(1, df.EvaluateDerivative(f, 0, 1), 1e-10);
        }

        [Test]
        public void CubicPolynomialThirdDerivativeAtAnyValTest()
        {
            Func<double, double> f = x => 3 * Math.Pow(x, 3) + 2 * x - 6;
            var df = new NumericalDerivative(5, 2);
            Assert.AreEqual(18, df.EvaluateDerivative(f, 0, 3));
            Assert.AreEqual(18, df.EvaluateDerivative(f, 10, 3));
            df.Center = 0;
            Assert.AreEqual(18, df.EvaluateDerivative(f, 0, 3));
            Assert.AreEqual(18, df.EvaluateDerivative(f, 10, 3));
            df.Center = 1;
            Assert.AreEqual(18, df.EvaluateDerivative(f, 0, 3));
            Assert.AreEqual(18, df.EvaluateDerivative(f, 10, 3));
            df.Center = 2;
            Assert.AreEqual(18, df.EvaluateDerivative(f, 0, 3));
            Assert.AreEqual(18, df.EvaluateDerivative(f, 10, 3));
            df.Center = 3;
            Assert.AreEqual(18, df.EvaluateDerivative(f, 0, 3));
            Assert.AreEqual(18, df.EvaluateDerivative(f, 10, 3));
            df.Center = 4;
            Assert.AreEqual(18, df.EvaluateDerivative(f, 0, 3));
            Assert.AreEqual(18, df.EvaluateDerivative(f, 10, 3));
        }

        [Test]
        public void CubicPolynomialFunctionValueTest()
        {
            Func<double, double> f = x => 3 * Math.Pow(x, 3) + 2 * x - 6;
            var current = f(2);
            var df = new NumericalDerivative(3, 0);
            Assert.AreEqual(38, df.EvaluateDerivative(f, 2, 1, current), 1e-8);
        }

        [Test]
        public void CreateDerivativeFunctionHandleTest()
        {
            Func<double, double> f = x => 3 * Math.Pow(x, 3) + 2 * x - 6;
            var nd = new NumericalDerivative(5, 2);
            var df = nd.CreateDerivativeFunctionHandle(f, 3);

            Assert.AreEqual(18, df(0));

            // Test new function with same nd class
            Func<double, double> f2 = x => 2 * Math.Pow(x, 3) + 2 * x - 6;
            var df2 = nd.CreateDerivativeFunctionHandle(f2, 3);

            Assert.AreEqual(12, df2(0));

            // Original delegate not changed
            Assert.AreEqual(18, df(0));

        }

        [Test]
        public void ExponentialFunctionPartialDerivativeTest()
        {
            //Test Function
            Func<double[], double> f = (x) => Math.Sin(x[0] * x[1]) + Math.Exp(-x[0] / 2) + x[1] / x[0];

            //Analytical partial dfdx
            Func<double[], double> dfdx =
                (x) => Math.Cos(x[0] * x[1]) * x[1] - Math.Exp(-x[0] / 2) / 2 - x[1] / Math.Pow(x[0], 2);

            //Analytical partial dfdy
            Func<double[], double> dfdy = (x) => Math.Cos(x[0] * x[1]) * x[0] + 1 / x[0];

            var df = new NumericalDerivative(3, 1);
            var x1 = new double[] { 3, 3 };
            Assert.AreEqual(dfdx(x1), df.EvaluatePartialDerivative(f, x1, 0, 1), 1e-8);

            Assert.AreEqual(dfdy(x1), df.EvaluatePartialDerivative(f, x1, 1, 1), 1e-8);

            var x2 = new double[] { 300, -50 };
            df.StepType = StepType.Absolute;
            Assert.AreEqual(dfdx(x2), df.EvaluatePartialDerivative(f, x2, 0, 1), 1e-5);
            Assert.AreEqual(dfdy(x2), df.EvaluatePartialDerivative(f, x2, 1, 1), 1e-2);
        }

        [Test]
        public void ExponentialFunctionPartialDerivativeCurrentValueTest()
        {
            //Test Function
            Func<double[], double> f = (x) => Math.Sin(x[0] * x[1]) + Math.Exp(-x[0] / 2) + x[1] / x[0];

            //Analytical partial dfdx
            Func<double[], double> dfdx =
                (x) => Math.Cos(x[0] * x[1]) * x[1] - Math.Exp(-x[0] / 2) / 2 - x[1] / Math.Pow(x[0], 2);

            //Analytical partial dfdy
            Func<double[], double> dfdy = (x) => Math.Cos(x[0] * x[1]) * x[0] + 1 / x[0];

            // Current value
            var x1 = new double[] { 3, 3 };
            var current = f(x1);

            var df = new NumericalDerivative(5, 2);
            Assert.AreEqual(dfdx(x1), df.EvaluatePartialDerivative(f, x1, 0, 1, current), 1e-8);

            Assert.AreEqual(dfdy(x1), df.EvaluatePartialDerivative(f, x1, 1, 1, current), 1e-8);
        }

        [Test]
        public void RosenbrockFunctionMixedDerivativeOneVariableSecondOrderTest()
        {
            Func<double[], double> f = x => Math.Pow(1 - x[0], 2) + 100 * Math.Pow(x[1] - Math.Pow(x[0], 2), 2);
            var df = new NumericalDerivative();
            var x0 = new double[] { 2, 2 };
            var parameterindex = new int[] { 0, 0 };
            Assert.AreEqual(1602, df.EvaluatePartialDerivative(f, x0, 0, 1), 1e-6);
            Assert.AreEqual(4002, df.EvaluateMixedPartialDerivative(f, x0, parameterindex, 2));
        }

        [Test]
        public void RosenbrockFunctionMixedDerivativeTwoVariableSecondOrderTest()
        {
            Func<double[], double> f = x => Math.Pow(1 - x[0], 2) + 100 * Math.Pow(x[1] - Math.Pow(x[0], 2), 2);
            var df = new NumericalDerivative();
            var x0 = new double[] { 2, 2 };
            var parameterIndex = new[] { 0, 1 };

            Assert.AreEqual(-800, df.EvaluateMixedPartialDerivative(f, x0, parameterIndex, 2));
        }

        [Test]
        public void VectorFunction1PartialDerivativeTest()
        {
            Func<double[], double>[] f =
            {
                (x) => Math.Pow(x[0],2) - 3*x[1],
                (x) => x[1]*x[1] + 2*x[0]*x[1]
            };

            var x0 = new double[] { 2, 2 };
            var g = new double[] { 4, 4 };

            var df = new NumericalDerivative();
            Assert.AreEqual(g, df.EvaluatePartialDerivative(f, x0, 0, 1));
            Assert.AreEqual(new double[] { 2, 0 }, df.EvaluatePartialDerivative(f, x0, 0, 2));
            Assert.AreEqual(new double[] { -3, 8 }, df.EvaluatePartialDerivative(f, x0, 1, 1));
        }

        [Test]
        public void VectorFunctionMixedPartialDerivativeTest()
        {
            Func<double[], double>[] f =
            {
                (x) => Math.Pow(x[0],2) - 3*x[1],
                (x) => x[1]*x[1] + 2*x[0]*x[1]
            };

            var x0 = new double[] { 2, 2 };

            var df = new NumericalDerivative();
            Assert.AreEqual(new double[] { 0, 2 }, df.EvaluateMixedPartialDerivative(f, x0, new int[] { 0, 1 }, 2));
            Assert.AreEqual(new double[] { 0, 2 }, df.EvaluateMixedPartialDerivative(f, x0, new int[] { 1, 0 }, 2));

        }
    }
}
