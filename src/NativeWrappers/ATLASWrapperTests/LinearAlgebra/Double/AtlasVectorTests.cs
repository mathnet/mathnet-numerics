namespace MathNet.Numerics.ATLASWrapperTests.LinearAlgebra.Double
{
    using MbUnit.Framework;
    using Numerics.LinearAlgebra.Double;

    public class AtlasVectorTests : NativeVectorTests
    {
        [FixtureSetUp] 
        public void SetUpProvider()
        {
            Control.LinearAlgebraProvider  = new Algorithms.LinearAlgebra.Atlas.AtlasLinearAlgebraProvider();
        }
    }
}
