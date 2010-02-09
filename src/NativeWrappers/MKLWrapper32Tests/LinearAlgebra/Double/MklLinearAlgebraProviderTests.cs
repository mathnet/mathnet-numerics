namespace MathNet.Numerics.MklWrapperTests.LinearAlgebra.Double
{
    using MbUnit.Framework;
    using UnitTests.LinearAlgebraTests.Double;

    public class MklLinearAlgebraProviderTests : LinearAlgebraProviderTests
    {
        [FixtureSetUp] 
        public void SetUpProvider()
        {
            Provider = new Algorithms.LinearAlgebra.Mkl.MklLinearAlgebraProvider();
        }
    }
}
