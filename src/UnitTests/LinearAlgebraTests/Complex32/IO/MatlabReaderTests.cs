
namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Complex32.IO
{
    using LinearAlgebra.Complex32;
    using LinearAlgebra.Complex32.IO;
    using MbUnit.Framework;

    [TestFixture]
    public class MatlabMatrixReaderTest
    {

        [Test]
        public void CanReadComplexAllMatrices()
        {
            var dmr = new MatlabMatrixReader("./data/Matlab/complex.mat");
            var matrices = dmr.ReadMatrices();
            Assert.AreEqual(3, matrices.Length);
            foreach (var matrix in matrices)
            {
                Assert.AreEqual(typeof(DenseMatrix), matrix.GetType());
            }

            var a = matrices[0];

            Assert.AreEqual(100, a.RowCount);
            Assert.AreEqual(100, a.ColumnCount);
            AssertHelpers.AlmostEqual(27.232498979698409, a.L2Norm(), 6);
        }

        [Test]
        public void CanReadSparseComplexAllMatrices()
        {
            var dmr = new MatlabMatrixReader("./data/Matlab/sparse_complex.mat");
            var matrices = dmr.ReadMatrices();
            Assert.AreEqual(3, matrices.Length);
            foreach (var matrix in matrices)
            {
                Assert.AreEqual(typeof(SparseMatrix), matrix.GetType());
            }

            var a = matrices[0];

            Assert.AreEqual(100, a.RowCount);
            Assert.AreEqual(100, a.ColumnCount);
            AssertHelpers.AlmostEqual(13.223654390985379, a.L2Norm(), 7);
        }

        [Test]
        public void CanReadNonComplexAllMatrices()
        {
            var dmr = new MatlabMatrixReader("./data/Matlab/collection.mat");
            var matrices = dmr.ReadMatrices();
            Assert.AreEqual(30, matrices.Length);
            foreach (var matrix in matrices)
            {
                Assert.AreEqual(typeof(DenseMatrix), matrix.GetType());
            }
        }

        [Test]
        public void CanReadNonComplexFirstMatrix()
        {
            var dmr = new MatlabMatrixReader("./data/Matlab/A.mat");
            var matrix = dmr.ReadMatrix();
            Assert.AreEqual(100, matrix.RowCount);
            Assert.AreEqual(100, matrix.ColumnCount);
            Assert.AreEqual(typeof(DenseMatrix), matrix.GetType());
            AssertHelpers.AlmostEqual(100.108979553704, matrix.FrobeniusNorm(), 6);
        }


        [Test]
        public void CanReadNonComplexNamedMatrices()
        {
            var dmr = new MatlabMatrixReader("./data/Matlab/collection.mat");
            var matrices = dmr.ReadMatrices(new[] { "Ad", "Au64" });
            Assert.AreEqual(2, matrices.Length);
            foreach (var matrix in matrices)
            {
                Assert.AreEqual(typeof(DenseMatrix), matrix.GetType());
            }
        }

        [Test]
        public void CanReadNonComplexNamedMatrix()
        {
            var dmr = new MatlabMatrixReader("./data/Matlab/collection.mat");
            var matrices = dmr.ReadMatrices(new[] { "Ad" });
            Assert.AreEqual(1, matrices.Length);
            Assert.AreEqual(100, matrices[0].RowCount);
            Assert.AreEqual(100, matrices[0].ColumnCount);
            AssertHelpers.AlmostEqual(100.431635988639, matrices[0].FrobeniusNorm(), 6);
            Assert.AreEqual(typeof(DenseMatrix), matrices[0].GetType());
        }

        [Test]
        public void CanReadNonComplexNamedSparseMatrix()
        {
            var dmr = new MatlabMatrixReader("./data/Matlab/sparse-small.mat");
            var matrix = dmr.ReadMatrix("S");
            Assert.AreEqual(100, matrix.RowCount);
            Assert.AreEqual(100, matrix.ColumnCount);
            Assert.AreEqual(typeof(SparseMatrix), matrix.GetType());
            AssertHelpers.AlmostEqual(17.6385090630805, matrix.FrobeniusNorm(), 6);
        }
    }
}
