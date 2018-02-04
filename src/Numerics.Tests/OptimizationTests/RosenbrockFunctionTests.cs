// <copyright file="RosenbrockFunctionTests.cs" company="Math.NET">
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

using System;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.UnitTests.OptimizationTests.TestFunctions;
using NUnit.Framework;

namespace MathNet.Numerics.UnitTests.OptimizationTests
{
    [TestFixture]
    class RosenbrockFunctionTests
    {
        [Test]
        public void TestGradient()
        {
            var input = new DenseVector(new[]{ -0.9, -0.5 } );

            var v1 = RosenbrockFunction.Value(input);
            var g = RosenbrockFunction.Gradient(input);

            var eps = 1e-5;
            var eps0 = (new DenseVector(new[] { 1.0, 0.0 })) * eps;
            var eps1 = (new DenseVector(new[] { 0.0, 1.0 })) * eps;

            var g0 = (RosenbrockFunction.Value(input + eps0) - RosenbrockFunction.Value(input - eps0)) / (2 * eps);
            var g1 = (RosenbrockFunction.Value(input + eps1) - RosenbrockFunction.Value(input - eps1)) / (2 * eps);

            Assert.That(Math.Abs(g0 - g[0]) < 1e-3);
            Assert.That(Math.Abs(g1 - g[1]) < 1e-3);
        }

        [Test]
        public void TestHessian()
        {
            var input = new DenseVector(new[] { -0.9, -0.5 });

            var v1 = RosenbrockFunction.Value(input);
            var h = RosenbrockFunction.Hessian(input);

            var eps = 1e-5;

            var eps0 = (new DenseVector(new[] { 1.0, 0.0 })) * eps;
            var eps1 = (new DenseVector(new[] { 0.0, 1.0 })) * eps;

            var epsuu = (new DenseVector(new[] { 1.0, 1.0 })) * eps;
            var epsud = (new DenseVector(new[] { 1.0, -1.0 })) * eps;

            var h00 = (RosenbrockFunction.Value(input + eps0) - 2*RosenbrockFunction.Value(input) + RosenbrockFunction.Value(input - eps0)) / (eps*eps);
            var h11 = (RosenbrockFunction.Value(input + eps1) - 2 * RosenbrockFunction.Value(input) + RosenbrockFunction.Value(input - eps1)) / (eps * eps);
            var h01 = (RosenbrockFunction.Value(input + epsuu) - RosenbrockFunction.Value(input + epsud) - RosenbrockFunction.Value(input - epsud) + RosenbrockFunction.Value(input - epsuu)) / (4*eps * eps);

            Assert.That(Math.Abs(h00 - h[0,0]) < 1e-3);
            Assert.That(Math.Abs(h11 - h[1,1]) < 1e-3);
            Assert.That(Math.Abs(h01 - h[0, 1]) < 1e-3);
            Assert.That(Math.Abs(h01 - h[1, 0]) < 1e-3);
        }
    }
}
