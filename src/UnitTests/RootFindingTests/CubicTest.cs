using System;
using MathNet.Numerics.RootFinding;
using NUnit.Framework;

namespace MathNet.Numerics.UnitTests.RootFindingTests
{
    [TestFixture, Category("RootFinding")]
    internal class CubicTest
    {
        [Test]
        public void RealRoots_SingleReal()
        {
            var a0 = 1d;
            var a1 = 5d;
            var a2 = 2d;

            var roots = Cubic.RealRoots(a2, a1, a0);
            var root = roots.Item1;
            var funcValue = root * (root * (root + a2) + a1) + a0;
            Assert.That(funcValue, Is.EqualTo(0).Within(1e-14));
            Assert.AreEqual(double.NaN, roots.Item2);
            Assert.AreEqual(double.NaN, roots.Item3);
        }

        [Test]
        public void RealRoots_DoubleReal1()
        {
            var a0 = -2d;
            var a1 = 5d;
            var a2 = -4d;
            var roots = Cubic.RealRoots(a2, a1, a0);
            Assert.That(roots.Item1, Is.EqualTo(2).Within(1e-14));
            Assert.That(roots.Item2, Is.EqualTo(1).Within(1e-14));
            Assert.AreEqual(double.NaN, roots.Item3);
        }
        
        [Test]
        public void RealRoots_DoubleReal2()
        {
            var a0 = -4d;
            var a1 = 8d;
            var a2 = -5d;
            var roots = Cubic.RealRoots(a2, a1, a0);
            Assert.That(roots.Item1, Is.EqualTo(1).Within(1e-14));
            Assert.That(roots.Item2, Is.EqualTo(2).Within(1e-14));
            Assert.AreEqual(double.NaN, roots.Item3);
        }

        [Test]
        public void RealRoots_TripleReal()
        {
            var a0 = 6d;
            var a1 = -5d;
            var a2 = -2d;
            var roots = Cubic.RealRoots(a2, a1, a0);
            Assert.That(roots.Item1, Is.EqualTo(3).Within(1e-14));
            Assert.That(roots.Item2, Is.EqualTo(-2).Within(1e-14));
            Assert.That(roots.Item3, Is.EqualTo(1).Within(1e-14));
        }
    }
}
