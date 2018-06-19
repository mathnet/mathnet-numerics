using MathNet.Numerics.RootFinding;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathNet.Numerics.Tests.RootFindingTests
{
    [TestFixture, Category("RootFinding")]
    public class NewtonRaphsonRnTests
    {
        [Test]
        public void R2Example()
        {
            // Example from http://www.nibis.de/~lbs-gym/Verschiedenespdf/NewtonsVerfahrenmehrdim.pdf

            var f = new Func<double[], double>[]
            {
                x => 3 * x[0] * x[0] - 3 * x[1],
                x => 3 * x[1] * x[1] - 3 * x[0]
            };

            var intialGuess = new double[] { 2, 2 };

            var root = NewtonRaphsonRn.FindRootNearGuess(f, intialGuess);

            Assert.AreEqual(1, root[0]);
            Assert.AreEqual(1, root[1]);
        }

        [Test]
        public void R3Example()
        {
            // Example from https://www.frustfrei-lernen.de/mathematik/drei-gleichungen-loesen-drei-unbekannte.html

            var f = new Func<double[], double>[]
            {
                x => -x[0] + x[1] + x[2],
                x => x[0] - 3 * x[1] - 2 * x[2] - 5,
                x => 5 * x[0] + x[1] + 4 * x[2] - 3,
            };

            var root = NewtonRaphsonRn.FindRoot(f);

            Assert.AreEqual(-1, root[0], 1e-14);
            Assert.AreEqual(-4, root[1], 1e-14);
            Assert.AreEqual(3, root[2], 1e-14);
        }

        [Test]
        public void MultipleRoots()
        {
            // Test case from NewtonRaphson R1

            // Roots at -2, 2
            Func<double[], double> f = x => x[0] * x[0] - 4;
            var f1a = new Func<double[], double>[] { f };
            var f1b = new Func<double[], double>[] { x => -f(x) };

            Assert.AreEqual(0, f(NewtonRaphsonRn.FindRoot(f1a, -5, 6, 1e-14)));
            Assert.AreEqual(-2, NewtonRaphsonRn.FindRoot(f1a, -5, -1, 1e-14)[0]);
            Assert.AreEqual(2, NewtonRaphsonRn.FindRoot(f1a, 1, 4, 1e-14)[0]);
            Assert.AreEqual(0, f(NewtonRaphsonRn.FindRoot(f1b, -5, 6, 1e-14)));
            Assert.AreEqual(-2, NewtonRaphsonRn.FindRoot(f1b, -5, -1, 1e-14)[0]);
            Assert.AreEqual(2, NewtonRaphsonRn.FindRoot(f1b, 1, 4, 1e-14)[0]);

            // Roots at 3, 4
            Func<double[], double> f2 = x => (x[0] - 3) * (x[0] - 4);
            var f2a = new Func<double[], double>[] { f2 };

            Assert.AreEqual(0, f2(NewtonRaphsonRn.FindRoot(f2a, -5, 6, 1e-14)));
            Assert.AreEqual(3, NewtonRaphsonRn.FindRoot(f2a, -5, 3.5, 1e-14)[0]);
            Assert.AreEqual(4, NewtonRaphsonRn.FindRoot(f2a, 3.2, 5, 1e-14)[0]);
            Assert.AreEqual(3, NewtonRaphsonRn.FindRoot(f2a, 2.1, 3.9, 0.001, 50)[0], 0.001);
            Assert.AreEqual(3, NewtonRaphsonRn.FindRoot(f2a, 2.1, 3.4, 0.001, 50)[0], 0.001);
        }

        [Test]
        public void MultipleRootsNearGuess()
        {
            // Test case from NewtonRaphson R1

            // Roots at -2, 2
            Func<double[], double> f1 = x => x[0] * x[0] - 4;
            var f1a = new Func<double[], double>[] { f1 };
            var f1b = new Func<double[], double>[] { x => -f1(x) };

            Assert.AreEqual(0, f1(NewtonRaphsonRn.FindRootNearGuess(f1a, new double[] { 0.5 }, accuracy: 1e-14)));
            Assert.AreEqual(-2, NewtonRaphsonRn.FindRootNearGuess(f1a, new double[] { -3.0 }, accuracy: 1e-14)[0]);
            Assert.AreEqual(2, NewtonRaphsonRn.FindRootNearGuess(f1a, new double[] { 2.5 }, accuracy: 1e-14)[0]);
            Assert.AreEqual(0, f1(NewtonRaphsonRn.FindRootNearGuess(f1b, new double[] { 0.6 }, accuracy: 1e-14)));
            Assert.AreEqual(-2, NewtonRaphsonRn.FindRootNearGuess(f1b, new double[] { -3 }, accuracy: 1e-14)[0]);
            Assert.AreEqual(2, NewtonRaphsonRn.FindRootNearGuess(f1b, new double[] { 2.5 }, accuracy: 1e-14)[0]);

            // Roots at 3, 4
            Func<double[], double> f2 = x => (x[0] - 3) * (x[0] - 4);
            var f2a = new Func<double[], double>[] { f2 };
            Assert.AreEqual(0, f2(NewtonRaphsonRn.FindRootNearGuess(f2a, new double[] { 0.5 }, accuracy: 1e-14)));
            Assert.AreEqual(3, NewtonRaphsonRn.FindRootNearGuess(f2a, new double[] { -0.75 }, accuracy: 1e-14)[0]);
            Assert.AreEqual(4, NewtonRaphsonRn.FindRootNearGuess(f2a, new double[] { 4.1 }, accuracy: 1e-14)[0]);
            Assert.AreEqual(3, NewtonRaphsonRn.FindRootNearGuess(f2a, new double[] { 3 }, accuracy: 0.001, maxIterations: 50)[0], 0.001);
            Assert.AreEqual(3, NewtonRaphsonRn.FindRootNearGuess(f2a,  new double[] { 2.75 }, accuracy: 0.001, maxIterations: 50)[0], 0.001);
        }

        [Test]
        public void LocalMinima()
        {
            // Test case from NewtonRaphson R1

            Func<double[], double> f1 = x => x[0] * x[0] * x[0] - 2 * x[0] + 2;
            var f1a = new Func<double[], double>[] { f1 };
            Assert.AreEqual(0, f1(NewtonRaphsonRn.FindRoot(f1a, -5, 6, 1e-14)));
        }

        [Test]
        public void Cubic()
        {
            // Test case from NewtonRaphson R1

            // with complex roots (looking for the real root only): 3x^3 + 4x^2 + 5x + 6, derivative 9x^2 + 8x + 5
            Func<double[], double> f1 = x => Evaluate.Polynomial(x[0], 6, 5, 4, 3);
            var f1a = new Func<double[], double>[] { f1 };
            Assert.AreEqual(-1.265328088928, NewtonRaphsonRn.FindRoot(f1a, -2, -1, 1e-10)[0], 1e-6);
            Assert.AreEqual(-1.265328088928, NewtonRaphsonRn.FindRoot(f1a, -5, 5, 1e-10)[0], 1e-6);

            // real roots only: 2x^3 + 4x^2 - 50x + 6, derivative 6x^2 + 8x - 50
            Func<double[], double> f2 = x => Evaluate.Polynomial(x[0], 6, -50, 4, 2);
            var f2a = new Func<double[], double>[] { f2 };
            Assert.AreEqual(-6.1466562197069, NewtonRaphsonRn.FindRoot(f2a, -8, -5, 1e-10)[0], 1e-6);
            Assert.AreEqual(0.12124737195841, NewtonRaphsonRn.FindRoot(f2a, -1, 1, 1e-10)[0], 1e-6);
            Assert.AreEqual(4.0254088477485, NewtonRaphsonRn.FindRoot(f2a, 3, 5, 1e-10)[0], 1e-6);
        }

        [Test]
        public void NoRoot()
        {
            // Test case from NewtonRaphson R1

            Func<double[], double> f1 = x => x[0] * x[0] + 4;
            var f1a = new Func<double[], double>[] { f1 };
            Assert.That(() => NewtonRaphsonRn.FindRoot(f1a, -5, 5, 1e-14, 50), Throws.TypeOf<NonConvergenceException>());
        }
    }
}
