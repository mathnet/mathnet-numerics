// <copyright file="GoldenSectionMinimizerTests.cs" company="Math.NET">
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
using MathNet.Numerics.Optimization;
using NUnit.Framework;

namespace MathNet.Numerics.UnitTests.OptimizationTests
{
    [TestFixture]
    public class GoldenSectionMinimizerTests
    {
        [Test]
        public void Test_Works()
        {
            var algorithm = new GoldenSectionMinimizer(1e-5, 1000);
            var f1 = new Func<double, double>(x => (x - 3)*(x - 3));
            var obj = new SimpleObjectiveFunction1D(f1);
            var r1 = algorithm.FindMinimum(obj, -100, 100);

            Assert.That(Math.Abs(r1.MinimizingPoint - 3.0), Is.LessThan(1e-4));
        }

        [Test]
        public void Test_ExpansionWorks()
        {
            var algorithm = new GoldenSectionMinimizer(1e-5, 1000);
            var f1 = new Func<double, double>(x => (x - 3)*(x - 3));
            var obj = new SimpleObjectiveFunction1D(f1);
            var r1 = algorithm.FindMinimum(obj, -5, 5);

            Assert.That(Math.Abs(r1.MinimizingPoint - 3.0), Is.LessThan(1e-4));
        }
    }
}
