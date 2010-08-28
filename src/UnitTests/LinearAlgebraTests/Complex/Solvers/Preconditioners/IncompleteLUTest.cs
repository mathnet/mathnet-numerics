namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Complex.Solvers.Preconditioners
{
    using System;
    using System.Numerics;
    using System.Reflection;
    using LinearAlgebra.Complex;
    using LinearAlgebra.Complex.Solvers.Preconditioners;
    using LinearAlgebra.Generic;
    using LinearAlgebra.Generic.Solvers.Preconditioners;
    using MbUnit.Framework;

    [TestFixture]
    public sealed class IncompleteLUFactorizationTest : PreconditionerTest
    {
        private static T GetMethod<T>(IncompleteLU ilu, string methodName)
        {
            var type = ilu.GetType();
            var methodInfo = type.GetMethod(methodName,
                                            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static,
                                            null,
                                            CallingConventions.Standard,
                                            new Type[0],
                                            null);
            var obj = methodInfo.Invoke(ilu, null);
            return (T)obj;
        }

        private static Matrix<Complex> GetUpperTriangle(IncompleteLU ilu)
        {
            return GetMethod<Matrix<Complex>>(ilu, "UpperTriangle");
        }

        private static Matrix<Complex> GetLowerTriangle(IncompleteLU ilu)
        {
            return GetMethod<Matrix<Complex>>(ilu, "LowerTriangle");
        }

        internal override IPreConditioner<Complex> CreatePreconditioner()
        {
            return new IncompleteLU();
        }

        protected override void CheckResult(IPreConditioner<Complex> preconditioner, SparseMatrix matrix, Vector<Complex> vector, Vector<Complex> result)
        {
            Assert.AreEqual(typeof(IncompleteLU), preconditioner.GetType(), "#01");

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

        [Test]
        [MultipleAsserts]
        public void CompareWithOriginalDenseMatrix()
        {
            var sparseMatrix = new SparseMatrix(3);
            sparseMatrix[0, 0] = -1;
            sparseMatrix[0, 1] = 5;
            sparseMatrix[0, 2] = 6;
            sparseMatrix[1, 0] = 3;
            sparseMatrix[1, 1] = -6;
            sparseMatrix[1, 2] = 1;
            sparseMatrix[2, 0] = 6;
            sparseMatrix[2, 1] = 8;
            sparseMatrix[2, 2] = 9;
            var ilu = new IncompleteLU();
            ilu.Initialize(sparseMatrix);
            var original = GetLowerTriangle(ilu).Multiply(GetUpperTriangle(ilu));

            for (var i = 0; i < sparseMatrix.RowCount; i++)
            {
                for (var j = 0; j < sparseMatrix.ColumnCount; j++)
                {
                    Assert.IsTrue(sparseMatrix[i, j].Real.AlmostEqual(original[i, j].Real, -Epsilon.Magnitude()), "#01-" + i + "-" + j);
                    Assert.IsTrue(sparseMatrix[i, j].Imaginary.AlmostEqual(original[i, j].Imaginary, -Epsilon.Magnitude()), "#02-" + i + "-" + j);
                }
            }
        }
    }
}
