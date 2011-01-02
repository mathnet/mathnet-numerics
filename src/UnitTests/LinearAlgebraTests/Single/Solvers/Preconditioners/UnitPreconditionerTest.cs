namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Single.Solvers.Preconditioners
{
    using LinearAlgebra.Single;
    using LinearAlgebra.Single.Solvers.Preconditioners;
    using MbUnit.Framework;

    [TestFixture]
    public sealed class UnitPreconditionerTest : PreconditionerTest
    {
        internal override IPreConditioner CreatePreconditioner()
        {
            return new UnitPreconditioner();
        }

        protected override void CheckResult(IPreConditioner preconditioner, SparseMatrix matrix, Vector vector, Vector result)
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
