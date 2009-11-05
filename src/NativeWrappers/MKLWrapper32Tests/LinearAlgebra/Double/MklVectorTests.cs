namespace MathNet.Numerics.MKLWrapperTests.LinearAlgebra.Double
{
    using MbUnit.Framework;
    using Numerics.LinearAlgebra.Double;

    public class MklVectorTests : NativeVectorTests
    {
        [FixtureSetUp] 
        public void SetUpProvider()
        {
            Control.LinearAlgebraProvider = new Algorithms.LinearAlgebra.Mkl.MklLinearAlgebraProvider();
        }
    }
}
