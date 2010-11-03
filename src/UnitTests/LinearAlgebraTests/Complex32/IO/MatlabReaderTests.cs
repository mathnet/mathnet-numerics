
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
            Assert.AreEqual(3, matrices.Count);
            foreach (var matrix in matrices)
            {
                Assert.AreEqual(typeof(DenseMatrix), matrix.Value.GetType());
            }

            var a = matrices["a"];

            Assert.AreEqual(100, a.RowCount);
            Assert.AreEqual(100, a.ColumnCount);
            AssertHelpers.AlmostEqual(27.232498979698409, a.L2Norm().Real, 6);
        }

        [Test]
        public void CanReadSparseComplexAllMatrices()
        {
            var dmr = new MatlabMatrixReader("./data/Matlab/sparse_complex.mat");
            var matrices = dmr.ReadMatrices();
            Assert.AreEqual(3, matrices.Count);
            foreach (var matrix in matrices)
            {
                Assert.AreEqual(typeof(SparseMatrix), matrix.Value.GetType());
            }

            var a = matrices["sa"];

            Assert.AreEqual(100, a.RowCount);
            Assert.AreEqual(100, a.ColumnCount);
            AssertHelpers.AlmostEqual(13.223654390985379, a.L2Norm().Real, 7);
        }

        [Test]
        public void CanReadNonComplexAllMatrices()
        {
            var dmr = new MatlabMatrixReader("./data/Matlab/collection.mat");
            var matrices = dmr.ReadMatrices();
            Assert.AreEqual(30, matrices.Count);
            foreach (var matrix in matrices)
            {
                Assert.AreEqual(typeof(DenseMatrix), matrix.Value.GetType());
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
            AssertHelpers.AlmostEqual(100.108979553704, matrix.FrobeniusNorm().Real, 6);
        }


        [Test]
        public void CanReadNonComplexNamedMatrices()
        {
            var dmr = new MatlabMatrixReader("./data/Matlab/collection.mat");
            var matrices = dmr.ReadMatrices(new[] { "Ad", "Au64" });
            Assert.AreEqual(2, matrices.Count);
            foreach (var matrix in matrices)
            {
                Assert.AreEqual(typeof(DenseMatrix), matrix.Value.GetType());
            }
        }

        [Test]
        public void CanReadNonComplexNamedMatrix()
        {
            var dmr = new MatlabMatrixReader("./data/Matlab/collection.mat");
            var matrices = dmr.ReadMatrices(new[] { "Ad" });
            Assert.AreEqual(1, matrices.Count);
            var ad = matrices["Ad"];
            Assert.AreEqual(100, ad.RowCount);
            Assert.AreEqual(100, ad.ColumnCount);
            AssertHelpers.AlmostEqual(100.431635988639, ad.FrobeniusNorm().Real, 6);
            Assert.AreEqual(typeof(DenseMatrix), ad.GetType());
        }

        [Test]
        public void CanReadNonComplexNamedSparseMatrix()
        {
            var dmr = new MatlabMatrixReader("./data/Matlab/sparse-small.mat");
            var matrix = dmr.ReadMatrix("S");
            Assert.AreEqual(100, matrix.RowCount);
            Assert.AreEqual(100, matrix.ColumnCount);
            Assert.AreEqual(typeof(SparseMatrix), matrix.GetType());
            AssertHelpers.AlmostEqual(17.6385090630805, matrix.FrobeniusNorm().Real, 6);
        }
    }
}
