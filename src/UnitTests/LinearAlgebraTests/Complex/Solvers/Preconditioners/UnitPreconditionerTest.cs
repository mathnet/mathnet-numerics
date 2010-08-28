namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Complex.Solvers.Preconditioners
{
    using System.Numerics;
    using LinearAlgebra.Complex;
    using LinearAlgebra.Complex.Solvers.Preconditioners;
    using LinearAlgebra.Generic;
    using LinearAlgebra.Generic.Solvers.Preconditioners;
    using MbUnit.Framework;

    [TestFixture]
    public sealed class UnitPreconditionerTest : PreconditionerTest
    {
        internal override IPreConditioner<Complex> CreatePreconditioner()
        {
            return new UnitPreconditioner();
        }

        protected override void CheckResult(IPreConditioner<Complex> preconditioner, SparseMatrix matrix, Vector<Complex> vector, Vector<Complex> result)
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
