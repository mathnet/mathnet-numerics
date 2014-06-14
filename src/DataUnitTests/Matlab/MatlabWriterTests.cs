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

using System;
using System.IO;
using System.Numerics;
using MathNet.Numerics.Data.Matlab;
using NUnit.Framework;

namespace MathNet.Numerics.Data.UnitTests.Matlab
{
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
            var matrix = new LinearAlgebra.Single.DenseMatrix(1, 1);
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
            Assert.Throws<ArgumentNullException>(() => writer.WriteMatrices(new LinearAlgebra.Single.Matrix[] { null }, new[] { "matrix" }));
            var matrix = new LinearAlgebra.Single.DenseMatrix(1, 1);
            Assert.Throws<ArgumentNullException>(() => writer.WriteMatrices(new LinearAlgebra.Single.Matrix[] { matrix }, null));
            writer.Dispose();
        }

        /// <summary>
        /// Can write double matrices.
        /// </summary>
        [Test]
        public void CanWriteDoubleMatrices()
        {
            var mat1 = new LinearAlgebra.Double.DenseMatrix(5, 3);
            for (var i = 0; i < mat1.ColumnCount; i++)
            {
                mat1[i, i] = i + .1;
            }

            var mat2 = new LinearAlgebra.Double.DenseMatrix(4, 5);
            for (var i = 0; i < mat2.RowCount; i++)
            {
                mat2[i, i] = i + .1;
            }

            var mat3 = new LinearAlgebra.Double.SparseMatrix(5, 4);
            mat3[0, 0] = 1.1;
            mat3[0, 2] = 2.2;
            mat3[4, 3] = 3.3;

            var mat4 = new LinearAlgebra.Double.SparseMatrix(3, 5);
            mat4[0, 0] = 1.1;
            mat4[0, 2] = 2.2;
            mat4[2, 4] = 3.3;

            var write = new LinearAlgebra.Double.Matrix[] { mat1, mat2, mat3, mat4 };

            var names = new[] { "mat1", "dense_matrix_2", "s1", "sparse2" };
            if (File.Exists("testd.mat"))
            {
                File.Delete("testd.mat");
            }

            var writer = new MatlabMatrixWriter("testd.mat");
            writer.WriteMatrices(write, names);
            writer.Dispose();

            var read = MatlabMatrixReader.ReadMatrices<double>("testd.mat", names);

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
        /// Can write float matrices.
        /// </summary>
        [Test]
        public void CanWriteFloatMatrices()
        {
            var mat1 = new LinearAlgebra.Single.DenseMatrix(5, 3);
            for (var i = 0; i < mat1.ColumnCount; i++)
            {
                mat1[i, i] = i + .1f;
            }

            var mat2 = new LinearAlgebra.Single.DenseMatrix(4, 5);
            for (var i = 0; i < mat2.RowCount; i++)
            {
                mat2[i, i] = i + .1f;
            }

            var mat3 = new LinearAlgebra.Single.SparseMatrix(5, 4);
            mat3[0, 0] = 1.1f;
            mat3[0, 2] = 2.2f;
            mat3[4, 3] = 3.3f;

            var mat4 = new LinearAlgebra.Single.SparseMatrix(3, 5);
            mat4[0, 0] = 1.1f;
            mat4[0, 2] = 2.2f;
            mat4[2, 4] = 3.3f;

            var write = new LinearAlgebra.Single.Matrix[] { mat1, mat2, mat3, mat4 };

            var names = new[] { "mat1", "dense_matrix_2", "s1", "sparse2" };
            if (File.Exists("tests.mat"))
            {
                File.Delete("tests.mat");
            }

            var writer = new MatlabMatrixWriter("tests.mat");
            writer.WriteMatrices(write, names);
            writer.Dispose();

            var read = MatlabMatrixReader.ReadMatrices<float>("tests.mat", names);

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
        /// Can write complex32 matrices.
        /// </summary>
        [Test]
        public void CanWriteComplex32Matrices()
        {
            var mat1 = new LinearAlgebra.Complex32.DenseMatrix(5, 3);
            for (var i = 0; i < mat1.ColumnCount; i++)
            {
                mat1[i, i] = new Complex32(i + .1f, i + .1f);
            }

            var mat2 = new LinearAlgebra.Complex32.DenseMatrix(4, 5);
            for (var i = 0; i < mat2.RowCount; i++)
            {
                mat2[i, i] = new Complex32(i + .1f, i + .1f);
            }

            var mat3 = new LinearAlgebra.Complex32.SparseMatrix(5, 4);
            mat3[0, 0] = new Complex32(1.1f, 1.1f);
            mat3[0, 2] = new Complex32(2.2f, 2.2f);
            mat3[4, 3] = new Complex32(3.3f, 3.3f);

            var mat4 = new LinearAlgebra.Complex32.SparseMatrix(3, 5);
            mat4[0, 0] = new Complex32(1.1f, 1.1f);
            mat4[0, 2] = new Complex32(2.2f, 2.2f);
            mat4[2, 4] = new Complex32(3.3f, 3.3f);

            var write = new LinearAlgebra.Complex32.Matrix[] { mat1, mat2, mat3, mat4 };

            var names = new[] { "mat1", "dense_matrix_2", "s1", "sparse2" };
            if (File.Exists("testc.mat"))
            {
                File.Delete("testc.mat");
            }

            var writer = new MatlabMatrixWriter("testc.mat");
            writer.WriteMatrices(write, names);
            writer.Dispose();

            var read = MatlabMatrixReader.ReadMatrices<Complex32>("testc.mat", names);

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
        /// Can write complex matrices.
        /// </summary>
        [Test]
        public void CanWriteComplexMatrices()
        {
            var mat1 = new LinearAlgebra.Complex.DenseMatrix(5, 3);
            for (var i = 0; i < mat1.ColumnCount; i++)
            {
                mat1[i, i] = new Complex(i + .1, i + .1);
            }

            var mat2 = new LinearAlgebra.Complex.DenseMatrix(4, 5);
            for (var i = 0; i < mat2.RowCount; i++)
            {
                mat2[i, i] = new Complex(i + .1, i + .1);
            }

            var mat3 = new LinearAlgebra.Complex.SparseMatrix(5, 4);
            mat3[0, 0] = new Complex(1.1, 1.1);
            mat3[0, 2] = new Complex(2.2, 2.2);
            mat3[4, 3] = new Complex(3.3, 3.3);

            var mat4 = new LinearAlgebra.Complex.SparseMatrix(3, 5);
            mat4[0, 0] = new Complex(1.1, 1.1);
            mat4[0, 2] = new Complex(2.2, 2.2);
            mat4[2, 4] = new Complex(3.3, 3.3);

            var write = new LinearAlgebra.Complex.Matrix[] { mat1, mat2, mat3, mat4 };

            var names = new[] { "mat1", "dense_matrix_2", "s1", "sparse2" };
            if (File.Exists("testz.mat"))
            {
                File.Delete("testz.mat");
            }

            var writer = new MatlabMatrixWriter("testz.mat");
            writer.WriteMatrices(write, names);
            writer.Dispose();

            var read = MatlabMatrixReader.ReadMatrices<Complex>("testz.mat", names);

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
            var matrix = new LinearAlgebra.Single.DenseMatrix(1, 1);
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
