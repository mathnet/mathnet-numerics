namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Double
{
    using MbUnit.Framework;

    public class ManagedLinearAlgebraProviderTests : LinearAlgebraProviderTests
    {
        [FixtureSetUp]
        public void SetProvider()
        {
            Provider = new Algorithms.LinearAlgebra.ManagedLinearAlgebraProvider();
        }
    }
}
