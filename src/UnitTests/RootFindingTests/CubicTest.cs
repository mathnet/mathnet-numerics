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
            var roots = Cubic.RealRoots(a0, a1, a2);
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
            var roots = Cubic.RealRoots(a0, a1, a2);
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
            var roots = Cubic.RealRoots(a0, a1, a2);
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
            var roots = Cubic.RealRoots(a0, a1, a2);
            Assert.That(roots.Item1, Is.EqualTo(3).Within(1e-14));
            Assert.That(roots.Item2, Is.EqualTo(-2).Within(1e-14));
            Assert.That(roots.Item3, Is.EqualTo(1).Within(1e-14));
        }

        [TestCase(6.0, -5.0, -2.0, 1.0, -2.0, 3.0, 1.0)]
        public void ComplexRoots_TripleReal(double d, double c, double b, double a, double x1, double x2, double x3)
        {
            var roots = FindRoots.Cubic(d, c, b, a);
            Assert.That(roots.Item1.Real, Is.EqualTo(x1).Within(1e-14));
            Assert.That(roots.Item1.Imaginary, Is.EqualTo(0).Within(1e-14));
            Assert.That(roots.Item2.Real, Is.EqualTo(x2).Within(1e-14));
            Assert.That(roots.Item2.Imaginary, Is.EqualTo(0).Within(1e-14));
            Assert.That(roots.Item3.Real, Is.EqualTo(x3).Within(1e-14));
            Assert.That(roots.Item3.Imaginary, Is.EqualTo(0).Within(1e-14));
        }

        [TestCase(-350.0, 162.0, -30.0, 2.0)]
        [TestCase(6.0, -5.0, -2.0, 1.0)]
        [TestCase(1.0, 5.0, 2.0, 1.0)]
        [TestCase(1.0, 5.0, 0.0, 1.0)]
        [TestCase(1.0, 0.0, 2.0, 1.0)]
        [TestCase(0.0, 0.0, 0.0, 2.0)]
        public void ComplexRootsAreRoots(double d, double c, double b, double a)
        {
            var roots = FindRoots.Cubic(d, c, b, a);
            Assert.That(Evaluate.Polynomial(roots.Item1, d, c, b, a).Real, Is.EqualTo(0).Within(1e-12));
            Assert.That(Evaluate.Polynomial(roots.Item1, d, c, b, a).Imaginary, Is.EqualTo(0).Within(1e-12));
            Assert.That(Evaluate.Polynomial(roots.Item2, d, c, b, a).Real, Is.EqualTo(0).Within(1e-12));
            Assert.That(Evaluate.Polynomial(roots.Item2, d, c, b, a).Imaginary, Is.EqualTo(0).Within(1e-12));
            Assert.That(Evaluate.Polynomial(roots.Item3, d, c, b, a).Real, Is.EqualTo(0).Within(1e-12));
            Assert.That(Evaluate.Polynomial(roots.Item3, d, c, b, a).Imaginary, Is.EqualTo(0).Within(1e-12));
        }
    }
}
