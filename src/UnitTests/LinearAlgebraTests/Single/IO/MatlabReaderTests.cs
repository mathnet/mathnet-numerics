
namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Single.IO
{
    using LinearAlgebra.Single;
    using LinearAlgebra.Single.IO;
    using MbUnit.Framework;

    [TestFixture]
    public class MatlabMatrixReaderTest
    {
        [Test]
        public void CanReadAllMatrices()
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
        public void CanReadFirstMatrix()
        {
            var dmr = new MatlabMatrixReader("./data/Matlab/A.mat");
            var matrix = dmr.ReadMatrix();
            Assert.AreEqual(100, matrix.RowCount);
            Assert.AreEqual(100, matrix.ColumnCount);
            Assert.AreEqual(typeof(DenseMatrix), matrix.GetType());
            AssertHelpers.AlmostEqual(100.108979553704f, matrix.FrobeniusNorm(), 6);
        }


        [Test]
        public void CanReadNamedMatrices()
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
        public void CanReadNamedMatrix()
        {
            var dmr = new MatlabMatrixReader("./data/Matlab/collection.mat");
            var matrices = dmr.ReadMatrices(new[] { "Ad" });
            Assert.AreEqual(1, matrices.Count);
            var ad = matrices["Ad"];
            Assert.AreEqual(100, ad.RowCount);
            Assert.AreEqual(100, ad.ColumnCount);
            AssertHelpers.AlmostEqual(100.431635988639f, ad.FrobeniusNorm(), 6);
            Assert.AreEqual(typeof(DenseMatrix), ad.GetType());
        }

        [Test]
        public void CanReadNamedSparseMatrix()
        {
            var dmr = new MatlabMatrixReader("./data/Matlab/sparse-small.mat");
            var matrix = dmr.ReadMatrix("S");
            Assert.AreEqual(100, matrix.RowCount);
            Assert.AreEqual(100, matrix.ColumnCount);
            Assert.AreEqual(typeof(SparseMatrix), matrix.GetType());
            AssertHelpers.AlmostEqual(17.6385090630805f, matrix.FrobeniusNorm(), 6);
        }
    }
}
