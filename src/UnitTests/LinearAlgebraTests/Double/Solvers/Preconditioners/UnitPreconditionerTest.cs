namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Double.Solvers.Preconditioners
{
    using LinearAlgebra.Double;
    using LinearAlgebra.Double.Solvers.Preconditioners;
    using LinearAlgebra.Generic;
    using LinearAlgebra.Generic.Solvers.Preconditioners;
    using MbUnit.Framework;

    [TestFixture]
    public sealed class UnitPreconditionerTest : PreconditionerTest
    {
        internal override IPreConditioner<double> CreatePreconditioner()
        {
            return new UnitPreconditioner();
        }

        protected override void CheckResult(IPreConditioner<double> preconditioner, SparseMatrix matrix, Vector<double> vector, Vector<double> result)
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
