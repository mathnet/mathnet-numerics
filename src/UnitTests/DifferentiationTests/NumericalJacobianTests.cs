// <copyright file="NumericalJacobianTests.cs" company="Math.NET">
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
    class NumericalJacobianTests
    {
        [Test]
        public void OneEquationVectorFunctionTest()
        {
            Func<double[], double> f = x => Math.Sin(x[0] * x[1]) * Math.Exp(-x[0] / 2) + x[1] / x[0];
            var J = new NumericalJacobian();
            var jeval = J.Evaluate(f, new double[] { 1, 1 });
            Assert.AreEqual(-0.92747906175, jeval[0], 1e-9);
            Assert.AreEqual(1.32770991402, jeval[1], 1e-9);
            Assert.AreEqual(4, J.FunctionEvaluations);
            J.ResetFunctionEvaluations();
            Assert.AreEqual(0, J.FunctionEvaluations);
        }

        [Test]
        public void ScalarFunctonDerivativeTest()
        {
            Func<double, double> f = x => 1 / x;
            var J = new NumericalJacobian();
            var jeval = J.Evaluate(f, 2);
            Assert.AreEqual((double)-1 / 4, jeval[0], 1e-7);
        }

        [Test]
        public void VectorFunctionJacobianTest()
        {
            Func<double[], double>[] f =
            {
                (x) => Math.Pow(x[0], 2)*x[1],
                (x) => 5*x[0] + Math.Sin(x[1])
            };
            var j = new NumericalJacobian();
            var jeval = j.Evaluate(f, new double[] { 1, 1 });
            Assert.AreEqual(2, jeval[0, 0]);
            Assert.AreEqual(1, jeval[0, 1]);
            Assert.AreEqual(5, jeval[1, 0]);
            Assert.AreEqual(Math.Cos(1), jeval[1, 1], 1e-7);

        }
    }
}
