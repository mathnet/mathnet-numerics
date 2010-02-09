namespace MathNet.Numerics.ATLASWrapperTests.LinearAlgebra.Double
{
    using MbUnit.Framework;
    using UnitTests.LinearAlgebraTests.Double;

    public class AtlasLinearAlgebraProviderTests : LinearAlgebraProviderTests
    {
        [FixtureSetUp] 
        public void SetUpProvider()
        {
            Provider  = new Algorithms.LinearAlgebra.Atlas.AtlasLinearAlgebraProvider();
        }
    }
}
