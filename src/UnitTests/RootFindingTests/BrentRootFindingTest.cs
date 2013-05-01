using MathNet.Numerics.RootFinding;
using NUnit.Framework;

namespace MathNet.Numerics.UnitTests.RootFindingTests
{
    [TestFixture]
    public class BrentRootFindingTest
    {
        [Test]
        public void MultipleRoots()
        {
            var solver = new BrentRootFinder(100, 1e-14) {Func = x => x*x - 4};
            double root = solver.Solve(-5, 5);
            Assert.AreEqual(0, solver.Func(root));
        }
    }
}
