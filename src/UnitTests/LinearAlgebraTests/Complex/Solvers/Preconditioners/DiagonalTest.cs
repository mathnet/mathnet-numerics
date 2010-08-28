namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Complex.Solvers.Preconditioners
{
    using System.Numerics;
    using LinearAlgebra.Complex;
    using LinearAlgebra.Complex.Solvers.Preconditioners;
    using LinearAlgebra.Generic;
    using LinearAlgebra.Generic.Solvers.Preconditioners;
    using MbUnit.Framework;

    [TestFixture]
    public sealed class DiagonalTest : PreconditionerTest
    {
        internal override IPreConditioner<Complex> CreatePreconditioner()
        {
            return new Diagonal();
        }

        protected override void CheckResult(IPreConditioner<Complex> preconditioner, SparseMatrix matrix, Vector<Complex> vector, Vector<Complex> result)
        {
            Assert.AreEqual(typeof(Diagonal), preconditioner.GetType(), "#01");

            // Compute M * result = product
            // compare vector and product. Should be equal
            Vector<Complex> product = new DenseVector(result.Count);
            matrix.Multiply(result, product);

            for (var i = 0; i < product.Count; i++)
            {
                Assert.IsTrue(vector[i].Real.AlmostEqual(product[i].Real, -Epsilon.Magnitude()), "#02-" + i);
                Assert.IsTrue(vector[i].Imaginary.AlmostEqual(product[i].Imaginary, -Epsilon.Magnitude()), "#03-" + i);
            }
        }
    }
}
