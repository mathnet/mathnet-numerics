// <copyright file="BfgsTest.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2017 Math.NET
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

using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.Optimization;
using MathNet.Numerics.UnitTests.OptimizationTests.TestFunctions;
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
            var x = BfgsSolver.Solve(new DenseVector(new[] { a, b }), RosenbrockFunction.Value, RosenbrockFunction.Gradient);
            Numerics.Precision.AlmostEqual(expectedMin, RosenbrockFunction.Value(x), Precision);
        }
    }
}
