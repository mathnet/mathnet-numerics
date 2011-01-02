namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Double.Solvers.Preconditioners
{
    using System;
    using System.Reflection;
    using LinearAlgebra.Double;
    using LinearAlgebra.Double.Solvers.Preconditioners;
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

        private static Matrix GetUpperTriangle(IncompleteLU ilu)
        {
            return GetMethod<Matrix>(ilu, "UpperTriangle");
        }

        private static Matrix GetLowerTriangle(IncompleteLU ilu)
        {
            return GetMethod<Matrix>(ilu, "LowerTriangle");
        }

        internal override IPreConditioner CreatePreconditioner()
        {
            return new IncompleteLU();
        }

        protected override void CheckResult(IPreConditioner preconditioner, SparseMatrix matrix, Vector vector, Vector result)
        {
            Assert.AreEqual(typeof(IncompleteLU), preconditioner.GetType(), "#01");

            // Compute M * result = product
            // compare vector and product. Should be equal
            Vector product = new DenseVector(result.Count);
            matrix.Multiply(result, product);

            for (var i = 0; i < product.Count; i++)
            {
                Assert.IsTrue(vector[i].AlmostEqual(product[i], -Epsilon.Magnitude()), "#02-" + i);
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
                    Assert.IsTrue(sparseMatrix[i, j].AlmostEqual(original[i, j], -Epsilon.Magnitude()), "#01-" + i + "-" + j);
                }
            }
        }
    }
}
