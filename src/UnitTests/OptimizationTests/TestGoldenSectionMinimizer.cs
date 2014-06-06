using System;
using MathNet.Numerics.Optimization;
using NUnit.Framework;

namespace MathNet.Numerics.UnitTests.OptimizationTests
{
    [TestFixture]
    public class TestGoldenSectionMinimizer
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
