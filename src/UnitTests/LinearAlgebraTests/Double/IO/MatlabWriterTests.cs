// <copyright file="MatlabWriterTests.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
// Copyright (c) 2009-2010 Math.NET
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
// </copyright>

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Double.IO
{
    using System;
    using System.IO;
    using LinearAlgebra.Double;
    using LinearAlgebra.Double.IO;
    using LinearAlgebra.IO;
    using NUnit.Framework;

    /// <summary>
    /// Matlab matrix writer tests.
    /// </summary>
    [TestFixture]
    public class MatlabMatrixWriterTests
    {
        /// <summary>
        /// Invalid constructor throws <c>ArgumentException</c>.
        /// </summary>
        [Test]
        public void InvalidConstructorThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new MatlabMatrixWriter(string.Empty));
            Assert.Throws<ArgumentException>(() => new MatlabMatrixWriter(null));
        }

        /// <summary>
        /// Write bad matrices throws <c>ArgumentException</c>.
        /// </summary>
        [Test]
        public void WriteBadMatricesThrowsArgumentException()
        {
            Matrix matrix = new DenseMatrix(1, 1);
            var writer = new MatlabMatrixWriter("somefile3");
            Assert.Throws<ArgumentException>(() => writer.WriteMatrices(new[] { matrix }, new[] { string.Empty }));
            Assert.Throws<ArgumentException>(() => writer.WriteMatrices(new[] { matrix }, new string[] { null }));
            Assert.Throws<ArgumentException>(() => writer.WriteMatrices(new[] { matrix, matrix }, new[] { "matrix" }));
            Assert.Throws<ArgumentException>(() => writer.WriteMatrices(new[] { matrix }, new[] { "some matrix" }));
            writer.Dispose();
        }

        /// <summary>
        /// Write <c>null</c> matrices throws <c>ArgumentNullException</c>.
        /// </summary>
        [Test]
        public void WriteNullMatricesThrowsArgumentNullException()
        {
            var writer = new MatlabMatrixWriter("somefile4");
            Assert.Throws<ArgumentNullException>(() => writer.WriteMatrices(new Matrix[] { null }, new[] { "matrix" }));
            Matrix matrix = new DenseMatrix(1, 1);
            Assert.Throws<ArgumentNullException>(() => writer.WriteMatrices(new[] { matrix }, null));
            writer.Dispose();
        }

        /// <summary>
        /// Can write matrices.
        /// </summary>
        [Test]
        public void CanWriteMatrices()
        {
            Matrix mat1 = new DenseMatrix(5, 3);
            for (var i = 0; i < mat1.ColumnCount; i++)
            {
                mat1[i, i] = i + .1;
            }

            Matrix mat2 = new DenseMatrix(4, 5);
            for (var i = 0; i < mat2.RowCount; i++)
            {
                mat2[i, i] = i + .1;
            }

            Matrix mat3 = new SparseMatrix(5, 4);
            mat3[0, 0] = 1.1;
            mat3[0, 2] = 2.2;
            mat3[4, 3] = 3.3;

            Matrix mat4 = new SparseMatrix(3, 5);
            mat4[0, 0] = 1.1;
            mat4[0, 2] = 2.2;
            mat4[2, 4] = 3.3;

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

            for (var i = 0; i < write.Length; i++)
            {
                var w = write[i];
                var r = read[names[i]];

                Assert.AreEqual(w.RowCount, r.RowCount);
                Assert.AreEqual(w.ColumnCount, r.ColumnCount);
                Assert.IsTrue(w.Equals(r));
            }
        }

        /// <summary>
        /// Write bad matrix throws <c>ArgumentException</c>.
        /// </summary>
        [Test]
        public void WriteBadMatrixThrowsArgumentException()
        {
            Matrix matrix = new DenseMatrix(1, 1);
            var writer = new MatlabMatrixWriter("somefile1");
            Assert.Throws<ArgumentException>(() => writer.WriteMatrix(matrix, string.Empty));
            Assert.Throws<ArgumentException>(() => writer.WriteMatrix(matrix, null));
            writer.Dispose();
        }

        /// <summary>
        /// Write <c>null</c> matrix throws <c>ArgumentNullException</c>.
        /// </summary>
        [Test]
        public void WriteNullMatrixThrowsArgumentNullException()
        {
            var writer = new MatlabMatrixWriter("somefile2");
            Assert.Throws<ArgumentNullException>(() => writer.WriteMatrix<double>(null, "matrix"));
            writer.Dispose();
        }
    }
}
