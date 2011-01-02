namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Single.Solvers.Preconditioners
{
    using LinearAlgebra.Single;
    using LinearAlgebra.Single.Solvers.Preconditioners;
    using MbUnit.Framework;

    [TestFixture]
    public sealed class DiagonalTest : PreconditionerTest
    {
        internal override IPreConditioner CreatePreconditioner()
        {
            return new Diagonal();
        }

        protected override void CheckResult(IPreConditioner preconditioner, SparseMatrix matrix, Vector vector, Vector result)
        {
            Assert.AreEqual(typeof(Diagonal), preconditioner.GetType(), "#01");

            // Compute M * result = product
            // compare vector and product. Should be equal
            Vector product = new DenseVector(result.Count);
            matrix.Multiply(result, product);

            for (var i = 0; i < product.Count; i++)
            {
                Assert.IsTrue(((double)vector[i]).AlmostEqual(product[i], -Epsilon.Magnitude()), "#02-" + i);
            }
        }
    }
}
