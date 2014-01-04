// <copyright file="TestBisectionRootFinder" company="Math.NET">
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
using NUnit.Framework;
using MathNet.Numerics.Optimization;

namespace MathNet.Numerics.UnitTests.OptimizationTests
{
    [TestFixture]
    internal class TestBisectionRootFinder
    {
        [Test]
        public void FindRoot_Works()
        {
            var algorithm = new BisectionRootFinder(0.001, 0.001);
            var f1 = new Func<double, double>(x => (x - 3)*(x - 4));
            var r1 = algorithm.FindRoot(f1, 2.1, 3.9);
            Assert.That(Math.Abs(f1(r1)), Is.LessThan(0.001));
            Assert.That(Math.Abs(r1 - 3.0), Is.LessThan(0.001));

            var f2 = new Func<double, double>(x => (x - 3)*(x - 4));
            var r2 = algorithm.FindRoot(f1, 2.1, 3.4);
            Assert.That(Math.Abs(f2(r2)), Is.LessThan(0.001));
            Assert.That(Math.Abs(r2 - 3.0), Is.LessThan(0.001));
        }

    }
}
