// <copyright file="NumericalHessianTests.cs" company="Math.NET">
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
    class NumericalHessianTests
    {
        [Test]
        public void ScalarFunctonSecondDerivativeTest()
        {
            // d2/dx2((2x)^2) = 8
            Func<double, double> f = x => Math.Pow(2 * x, 2);
            var J = new NumericalHessian();
            var jeval = J.Evaluate(f, 2);
            Assert.AreEqual((double)8, jeval[0], 1e-7);
        }

        [Test]
        public void OneEquationTwoVarVectorFunctionTest()
        {
            Func<double[], double> f = x => 4 * Math.Pow(x[0], 2) + 3 * Math.Pow(x[1], 3);
            var H = new NumericalHessian();
            var heval = H.Evaluate(f, new double[] { 1, 1 });
            var solution = new double[,] { { 8, 0 }, { 0, 18 } };
            Assert.AreEqual(solution, heval);
            Assert.AreEqual(12, H.FunctionEvaluations);
            H.ResetFunctionEvaluations();
            Assert.AreEqual(0, H.FunctionEvaluations);
        }

        [Test]
        public void RosenbrockFunctionHessianTest()
        {
            Func<double[], double> f = x => Math.Pow((1 - x[0]), 2) + 100 * Math.Pow(x[1] - Math.Pow(x[0], 2), 2);
            var H = new NumericalHessian(5, 2);
            var heval = H.Evaluate(f, new double[] { 2, 3 });
            var solution = new double[,] { { 3602, -800 }, { -800, 200 } };
            for (int row = 0; row < solution.Rank; row++)
            {
                for (int col = 0; col < solution.Rank; col++)
                {
                    Assert.AreEqual(solution[row, col], heval[row, col], 1e-7);
                }
            }

        }
    }
}
