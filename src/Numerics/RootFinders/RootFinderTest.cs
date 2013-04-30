namespace MathNet.Numerics.RootFinders
{
    using System;
    using System.Collections.Generic;
    using MbUnit.Framework;
    using Gallio.Framework;
    using SuntrustPortfolio.Numerics;

    [TestFixture]
    public class RootFinderTest
    {
        BrentRootFinder _solver = new BrentRootFinder(100, 1e-14);

        [Test]
        public void MultipleRoots()
        {
            Func<double, double> f = (x) => { return x * x - 4; };
            _solver.Func = f;
            double root = _solver.Solve(-5, 5);

            Assert.AreEqual(0, f(root));
        }
    }
}
