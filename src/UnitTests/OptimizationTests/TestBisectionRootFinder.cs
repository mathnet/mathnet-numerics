using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

using MathNet.Numerics.Optimization;

namespace MathNet.Numerics.UnitTests.OptimizationTests
{
    [TestFixture]
    class TestBisectionRootFinder
    {
        [Test]
        public void FindRoot_Works()
        {
            var algorithm = new BisectionRootFinder(0.001, 0.001);
            var f1 = new Func<double, double>((x) => (x - 3) * (x - 4));
            var r1 = algorithm.FindRoot(f1, 2.1, 3.9);
            Assert.That(Math.Abs(f1(r1)), Is.LessThan(0.001));
            Assert.That(Math.Abs(r1 - 3.0), Is.LessThan(0.001));

            var f2 = new Func<double, double>((x) => (x - 3) * (x - 4));
            var r2 = algorithm.FindRoot(f1, 2.1, 3.4);
            Assert.That(Math.Abs(f2(r2)), Is.LessThan(0.001));
            Assert.That(Math.Abs(r2 - 3.0), Is.LessThan(0.001));
        }

    }
}
