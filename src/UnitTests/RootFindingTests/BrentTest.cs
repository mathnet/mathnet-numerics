using MathNet.Numerics.RootFinding;
using NUnit.Framework;

namespace MathNet.Numerics.UnitTests.RootFindingTests
{
    [TestFixture]
    public class BrentTest
    {
        [Test]
        public void MultipleRoots()
        {
            double root = FindRoots.BrentMethod(x => x*x - 4, -5, 5, 1e-14, 100);
            Assert.AreEqual(0, root*root - 4);
        }
    }
}
