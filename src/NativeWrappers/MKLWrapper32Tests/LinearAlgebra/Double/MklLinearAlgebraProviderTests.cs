namespace MathNet.Numerics.MKLWrapperTests.LinearAlgebra.Double
{
    using MbUnit.Framework;
    using UnitTests.LinearAlgebraTests.Double;

    public class MklLinearAlgebraProviderTests : LinearAlgebraProviderTests
    {
        [FixtureSetUp] 
        public void SetUpProvider()
        {
            Control.LinearAlgebraProvider = new Algorithms.LinearAlgebra.Mkl.MklLinearAlgebraProvider();
        }
    }
}
