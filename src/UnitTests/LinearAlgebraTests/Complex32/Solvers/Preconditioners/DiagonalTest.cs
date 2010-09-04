namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Complex32.Solvers.Preconditioners
{
    using Numerics;
    using LinearAlgebra.Complex32;
    using LinearAlgebra.Complex32.Solvers.Preconditioners;
    using LinearAlgebra.Generic;
    using LinearAlgebra.Generic.Solvers.Preconditioners;
    using MbUnit.Framework;

    [TestFixture]
    public sealed class DiagonalTest : PreconditionerTest
    {
        internal override IPreConditioner<Complex32> CreatePreconditioner()
        {
            return new Diagonal();
        }

        protected override void CheckResult(IPreConditioner<Complex32> preconditioner, SparseMatrix matrix, Vector<Complex32> vector, Vector<Complex32> result)
        {
            Assert.AreEqual(typeof(Diagonal), preconditioner.GetType(), "#01");

            // Compute M * result = product
            // compare vector and product. Should be equal
            Vector<Complex32> product = new DenseVector(result.Count);
            matrix.Multiply(result, product);

            for (var i = 0; i < product.Count; i++)
            {
                Assert.IsTrue(vector[i].Real.AlmostEqual(product[i].Real, -Epsilon.Magnitude()), "#02-" + i);
                Assert.IsTrue(vector[i].Imaginary.AlmostEqual(product[i].Imaginary, -Epsilon.Magnitude()), "#03-" + i);
            }
        }
    }
}
