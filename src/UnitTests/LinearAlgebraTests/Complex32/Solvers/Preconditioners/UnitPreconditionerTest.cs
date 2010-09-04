namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Complex32.Solvers.Preconditioners
{
    using Numerics;
    using LinearAlgebra.Complex32;
    using LinearAlgebra.Complex32.Solvers.Preconditioners;
    using LinearAlgebra.Generic;
    using LinearAlgebra.Generic.Solvers.Preconditioners;
    using MbUnit.Framework;

    [TestFixture]
    public sealed class UnitPreconditionerTest : PreconditionerTest
    {
        internal override IPreConditioner<Complex32> CreatePreconditioner()
        {
            return new UnitPreconditioner();
        }

        protected override void CheckResult(IPreConditioner<Complex32> preconditioner, SparseMatrix matrix, Vector<Complex32> vector, Vector<Complex32> result)
        {
            Assert.AreEqual(typeof(UnitPreconditioner), preconditioner.GetType(), "#01");

            // Unit preconditioner is doing nothing. Vector and result should be equal

            for (var i = 0; i < vector.Count; i++)
            {
                Assert.IsTrue(vector[i] == result[i], "#02-" + i);
            }
        }
    }
}
