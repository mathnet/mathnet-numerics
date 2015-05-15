// <copyright file="BfgsTest.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
//
// Copyright (c) 2009-2015 Math.NET
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

using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.Optimization;
using NUnit.Framework;

namespace MathNet.Numerics.UnitTests.OptimizationTests
{
    [TestFixture, Category("RootFinding")]
    internal class BfgsTest
    {
        private const double Precision = 1e-4;

        [Test]
        public void MinimizeRosenbrock()
        {
            CheckRosenbrock(15.0, 8.0, expectedMin: 0.0);
            CheckRosenbrock(-1.2, 1.0, expectedMin: 0.0);
            CheckRosenbrock(-1.2, 100.0, expectedMin: 0.0);
        }

        private static void CheckRosenbrock(double a, double b, double expectedMin)
        {
            var x = BfgsSolver.Solve(new DenseVector(new[] { a, b }), Rosenbrock, RosenbrockGradient);
            Numerics.Precision.AlmostEqual(expectedMin, Rosenbrock(x), Precision);
        }

        private static double Rosenbrock(Vector<double> x)
        {
            double t1 = (1 - x[0]);
            double t2 = (x[1] - x[0] * x[0]);
            return t1 * t1 + 100 * t2 * t2;
        }

        private static Vector<double> RosenbrockGradient(Vector<double> x)
        {
            var grad = new DenseVector(2);
            grad[0]  = -2 * (1 - x[0]) + 200 * (x[1] - x[0] * x[0]) * (-2 * x[0]);
            grad[1]  = 200 * (x[1] - x[0] * x[0]);
            return grad;
        }
    }
}
