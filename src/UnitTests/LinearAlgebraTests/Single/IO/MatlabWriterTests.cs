namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Single.IO
{
    using System;
    using System.IO;
    using LinearAlgebra.IO;
    using LinearAlgebra.Single;
    using LinearAlgebra.Single.IO;
    using MbUnit.Framework;

    [TestFixture]
    public class MatlabMatrixWriterTests
    {
        [Test]
        public void Constructor_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new MatlabMatrixWriter(string.Empty));
            Assert.Throws<ArgumentException>(() => new MatlabMatrixWriter(null));
        }

        [Test]
        public void WriteMatrices_ThrowsArgumentException()
        {
            Matrix matrix = new DenseMatrix(1, 1);
            var writer = new MatlabMatrixWriter("somefile3");
            Assert.Throws<ArgumentException>(() => writer.WriteMatrices(new[] { matrix }, new[] { string.Empty }));
            Assert.Throws<ArgumentException>(() => writer.WriteMatrices(new[] { matrix }, new string[] { null }));
            Assert.Throws<ArgumentException>(() => writer.WriteMatrices(new[] { matrix, matrix }, new[] { "matrix" }));
            Assert.Throws<ArgumentException>(() => writer.WriteMatrices(new[] { matrix }, new[] { "some matrix" }));
            writer.Dispose();
        }

        [Test]
        public void WriteMatrices_ThrowsArgumentNullException()
        {
            var writer = new MatlabMatrixWriter("somefile4");
            Assert.Throws<ArgumentNullException>(() => writer.WriteMatrices(new Matrix[] { null }, new[] { "matrix" }));
            Matrix matrix = new DenseMatrix(1, 1);
            Assert.Throws<ArgumentNullException>(() => writer.WriteMatrices(new[] { matrix }, null));
            writer.Dispose();
        }

        [Test]
        public void WriteMatricesTest()
        {
            Matrix mat1 = new DenseMatrix(5, 3);
            for (var i = 0; i < mat1.ColumnCount; i++)
            {
                mat1[i, i] = i + .1f;
            }

            Matrix mat2 = new DenseMatrix(4, 5);
            for (var i = 0; i < mat2.RowCount; i++)
            {
                mat2[i, i] = i + .1f;
            }

            Matrix mat3 = new SparseMatrix(5, 4);
            for (var i = 0; i < mat3.ColumnCount; i++)
            {
                mat3[i, i] = i + .1f;
            }

            Matrix mat4 = new SparseMatrix(3, 5);
            for (var i = 0; i < mat4.RowCount; i++)
            {
                mat4[i, i] = i + .1f;
            }

            var write = new[] { mat1, mat2, mat3, mat4 };

            var names = new[] { "mat1", "dense_matrix_2", "s1", "sparse2" };
            if (File.Exists("test.mat"))
            {
                File.Delete("test.mat");
            }

            var writer = new MatlabMatrixWriter("test.mat");
            writer.WriteMatrices(write, names);
            writer.Dispose();

            var reader = new MatlabMatrixReader("test.mat");
            var read = reader.ReadMatrices(names);

            Assert.AreEqual(write.Length, read.Count);

            for (var i = 0; i < write.Length; i++ )
            {
                var w = write[i];
                var r = read[names[i]];

                Assert.AreEqual(w.RowCount, r.RowCount);
                Assert.AreEqual(w.ColumnCount, r.ColumnCount);
                Assert.IsTrue(w.Equals(r));
            }
        }

        [Test]
        public void WriteMatrix_ThrowsArgumentException()
        {
            Matrix matrix = new DenseMatrix(1, 1);
            var writer = new MatlabMatrixWriter("somefile1");
            Assert.Throws<ArgumentException>(() => writer.WriteMatrix(matrix, string.Empty));
            Assert.Throws<ArgumentException>(() => writer.WriteMatrix(matrix, null));
            writer.Dispose();
        }

        [Test]
        public void WriteMatrix_ThrowsArgumentNullException()
        {
            var writer = new MatlabMatrixWriter("somefile2");
            Assert.Throws<ArgumentNullException>(() => writer.WriteMatrix<double>(null, "matrix"));
            writer.Dispose();
        }
    }
}
