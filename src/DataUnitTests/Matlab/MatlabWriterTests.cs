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
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Storage;
using NUnit.Framework;

namespace MathNet.Numerics.Data.UnitTests.Matlab
{
    /// <summary>
    /// MATLAB matrix writer tests.
    /// </summary>
    [TestFixture]
    public class MatlabWriterTests
    {
        [Test]
        public void WriteBadMatricesThrowsArgumentException()
        {
            Matrix<float> matrix = Matrix<float>.Build.Dense(1, 1);
            Assert.Throws<ArgumentException>(() => MatlabWriter.Write("somefile3", matrix, string.Empty));
            Assert.Throws<ArgumentException>(() => MatlabWriter.Write("somefile3", matrix, null));
            Assert.Throws<ArgumentException>(() => MatlabWriter.Write("somefile3", matrix, "some matrix"));
            Assert.Throws<ArgumentException>(() => MatlabWriter.Write("somefile3", new[] { matrix }, new[] { string.Empty }));
            Assert.Throws<ArgumentException>(() => MatlabWriter.Write("somefile3", new[] { matrix }, new string[] { null }));
            Assert.Throws<ArgumentException>(() => MatlabWriter.Write("somefile3", new[] { matrix, matrix }, new[] { "matrix" }));
            Assert.Throws<ArgumentException>(() => MatlabWriter.Write("somefile3", new[] { matrix }, new[] { "some matrix" }));
        }

        [Test]
        public void CanWriteDoubleMatrices()
        {
            Matrix<double> mat1 = Matrix<double>.Build.Dense(5, 5);
            for (var i = 0; i < mat1.ColumnCount; i++)
            {
                mat1[i, i] = i + .1;
            }

            Matrix<double> mat2 = Matrix<double>.Build.Dense(4, 5);
            for (var i = 0; i < mat2.RowCount; i++)
            {
                mat2[i, i] = i + .1;
            }

            Matrix<double> mat3 = Matrix<double>.Build.Sparse(5, 4);
            mat3[0, 0] = 1.1;
            mat3[0, 2] = 2.2;
            mat3[4, 3] = 3.3;

            Matrix<double> mat4 = Matrix<double>.Build.Sparse(3, 5);
            mat4[0, 0] = 1.1;
            mat4[0, 2] = 2.2;
            mat4[2, 4] = 3.3;

            Matrix<double>[] write = { mat1, mat2, mat3, mat4 };
            string[] names = { "mat1", "dense_matrix_2", "s1", "sparse2" };

            if (File.Exists("testd.mat"))
            {
                File.Delete("testd.mat");
            }

            MatlabWriter.Write("testd.mat", write, names);

            var read = MatlabReader.ReadAll<double>("testd.mat", names);
            Assert.AreEqual(write.Length, read.Count);

            for (var i = 0; i < write.Length; i++)
            {
                var w = write[i];
                var r = read[names[i]];
                Assert.AreEqual(w.RowCount, r.RowCount);
                Assert.AreEqual(w.ColumnCount, r.ColumnCount);
                Assert.IsTrue(w.Equals(r));
            }

            File.Delete("testd.mat");
        }

        [Test]
        public void CanWriteFloatMatrices()
        {
            Matrix<float> mat1 = Matrix<float>.Build.Dense(5, 3);
            for (var i = 0; i < mat1.ColumnCount; i++)
            {
                mat1[i, i] = i + .1f;
            }

            Matrix<float> mat2 = Matrix<float>.Build.Dense(4, 5);
            for (var i = 0; i < mat2.RowCount; i++)
            {
                mat2[i, i] = i + .1f;
            }

            Matrix<float> mat3 = Matrix<float>.Build.Sparse(5, 4);
            mat3[0, 0] = 1.1f;
            mat3[0, 2] = 2.2f;
            mat3[4, 3] = 3.3f;

            Matrix<float> mat4 = Matrix<float>.Build.Sparse(3, 5);
            mat4[0, 0] = 1.1f;
            mat4[0, 2] = 2.2f;
            mat4[2, 4] = 3.3f;

            Matrix<float>[] write = { mat1, mat2, mat3, mat4 };
            string[] names = { "mat1", "dense_matrix_2", "s1", "sparse2" };

            if (File.Exists("tests.mat"))
            {
                File.Delete("tests.mat");
            }

            MatlabWriter.Write("tests.mat", write, names);

            var read = MatlabReader.ReadAll<float>("tests.mat", names);
            Assert.AreEqual(write.Length, read.Count);

            for (var i = 0; i < write.Length; i++)
            {
                var w = write[i];
                var r = read[names[i]];
                Assert.AreEqual(w.RowCount, r.RowCount);
                Assert.AreEqual(w.ColumnCount, r.ColumnCount);
                Assert.IsTrue(w.Equals(r));
            }

            File.Delete("tests.mat");
        }

        /// <summary>
        /// Can write complex32 matrices.
        /// </summary>
        [Test]
        public void CanWriteComplex32Matrices()
        {
            Matrix<Complex32> mat1 = Matrix<Complex32>.Build.Dense(5, 3);
            for (var i = 0; i < mat1.ColumnCount; i++)
            {
                mat1[i, i] = new Complex32(i + .1f, i + .1f);
            }

            Matrix<Complex32> mat2 = Matrix<Complex32>.Build.Dense(4, 5);
            for (var i = 0; i < mat2.RowCount; i++)
            {
                mat2[i, i] = new Complex32(i + .1f, i + .1f);
            }

            Matrix<Complex32> mat3 = Matrix<Complex32>.Build.Sparse(5, 4);
            mat3[0, 0] = new Complex32(1.1f, 1.1f);
            mat3[0, 2] = new Complex32(2.2f, 2.2f);
            mat3[4, 3] = new Complex32(3.3f, 3.3f);

            Matrix<Complex32> mat4 = Matrix<Complex32>.Build.Sparse(3, 5);
            mat4[0, 0] = new Complex32(1.1f, 1.1f);
            mat4[0, 2] = new Complex32(2.2f, 2.2f);
            mat4[2, 4] = new Complex32(3.3f, 3.3f);

            Matrix<Complex32>[] write = { mat1, mat2, mat3, mat4 };
            string[] names = { "mat1", "dense_matrix_2", "s1", "sparse2" };

            if (File.Exists("testc.mat"))
            {
                File.Delete("testc.mat");
            }

            MatlabWriter.Write("testc.mat", write, names);
            var read = MatlabReader.ReadAll<Complex32>("testc.mat", names);

            Assert.AreEqual(write.Length, read.Count);

            for (var i = 0; i < write.Length; i++)
            {
                var w = write[i];
                var r = read[names[i]];
                Assert.AreEqual(w.RowCount, r.RowCount);
                Assert.AreEqual(w.ColumnCount, r.ColumnCount);
                Assert.IsTrue(w.Equals(r));
            }

            File.Delete("testc.mat");
        }

        /// <summary>
        /// Can write complex matrices.
        /// </summary>
        [Test]
        public void CanWriteComplexMatrices()
        {
            Matrix<Complex> mat1 = Matrix<Complex>.Build.Dense(5, 3);
            for (var i = 0; i < mat1.ColumnCount; i++)
            {
                mat1[i, i] = new Complex(i + .1, i + .1);
            }

            Matrix<Complex> mat2 = Matrix<Complex>.Build.Dense(4, 5);
            for (var i = 0; i < mat2.RowCount; i++)
            {
                mat2[i, i] = new Complex(i + .1, i + .1);
            }

            Matrix<Complex> mat3 = Matrix<Complex>.Build.Sparse(5, 4);
            mat3[0, 0] = new Complex(1.1, 1.1);
            mat3[0, 2] = new Complex(2.2, 2.2);
            mat3[4, 3] = new Complex(3.3, 3.3);

            Matrix<Complex> mat4 = Matrix<Complex>.Build.Sparse(3, 5);
            mat4[0, 0] = new Complex(1.1, 1.1);
            mat4[0, 2] = new Complex(2.2, 2.2);
            mat4[2, 4] = new Complex(3.3, 3.3);

            Matrix<Complex>[] write = { mat1, mat2, mat3, mat4 };
            string[] names = { "mat1", "dense_matrix_2", "s1", "sparse2" };

            if (File.Exists("testz.mat"))
            {
                File.Delete("testz.mat");
            }

            MatlabWriter.Write("testz.mat", write, names);

            var read = MatlabReader.ReadAll<Complex>("testz.mat", names);
            Assert.AreEqual(write.Length, read.Count);

            for (var i = 0; i < write.Length; i++)
            {
                var w = write[i];
                var r = read[names[i]];
                Assert.AreEqual(w.RowCount, r.RowCount);
                Assert.AreEqual(w.ColumnCount, r.ColumnCount);
                Assert.IsTrue(w.Equals(r));
            }

            File.Delete("testz.mat");
        }

        /// <summary>
        /// Write bad matrix throws <c>ArgumentException</c>.
        /// </summary>
        [Test]
        public void WriteBadMatrixThrowsArgumentException()
        {
            var matrix = Matrix<float>.Build.Dense(1, 1);
            Assert.Throws<ArgumentException>(() => MatlabWriter.Write("somefile1", matrix, string.Empty));
            Assert.Throws<ArgumentException>(() => MatlabWriter.Write("somefile1", matrix, null));
        }

        /// <summary>
        /// Write <c>null</c> matrix throws <c>ArgumentNullException</c>.
        /// </summary>
        [Test]
        public void WriteNullMatrixThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => MatlabWriter.Write<double>("somefile2", null, "matrix"));
        }

        [Test]
        public void MatlabMatrixRoundtrip()
        {
            var denseDouble = Matrix<double>.Build.Random(20, 20);
            var denseComplex = Matrix<Complex>.Build.Random(10, 10);
            var diagonalDouble = Matrix<double>.Build.DiagonalOfDiagonalArray(new[] { 1.0, 2.0, 3.0 });
            var sparseDouble = Matrix<double>.Build.Sparse(20, 20, (i, j) => i%(j+1) == 2 ? i + 10*j : 0);

            var denseDoubleP = MatlabWriter.Pack(denseDouble, "denseDouble");
            var denseComplexP = MatlabWriter.Pack(denseComplex, "denseComplex");
            var diagonalDoubleP = MatlabWriter.Pack(diagonalDouble, "diagonalDouble");
            var sparseDoubleP = MatlabWriter.Pack(sparseDouble, "sparseDouble");

            Assert.That(MatlabReader.Unpack<double>(denseDoubleP).Equals(denseDouble));
            Assert.That(MatlabReader.Unpack<Complex>(denseComplexP).Equals(denseComplex));
            Assert.That(MatlabReader.Unpack<double>(diagonalDoubleP).Equals(diagonalDouble));
            Assert.That(MatlabReader.Unpack<double>(sparseDoubleP).Equals(sparseDouble));

            Assert.That(MatlabReader.Unpack<double>(denseDoubleP).Storage, Is.TypeOf<DenseColumnMajorMatrixStorage<double>>());
            Assert.That(MatlabReader.Unpack<Complex>(denseComplexP).Storage, Is.TypeOf<DenseColumnMajorMatrixStorage<Complex>>());
            Assert.That(MatlabReader.Unpack<double>(diagonalDoubleP).Storage, Is.TypeOf<SparseCompressedRowMatrixStorage<double>>());
            Assert.That(MatlabReader.Unpack<double>(sparseDoubleP).Storage, Is.TypeOf<SparseCompressedRowMatrixStorage<double>>());

            if (File.Exists("testrt.mat"))
            {
                File.Delete("testrt.mat");
            }

            MatlabWriter.Store("testrt.mat", new[] { denseDoubleP, denseComplexP, diagonalDoubleP, sparseDoubleP });

            Assert.That(MatlabReader.Read<double>("testrt.mat", "denseDouble").Equals(denseDouble));
            Assert.That(MatlabReader.Read<Complex>("testrt.mat", "denseComplex").Equals(denseComplex));
            Assert.That(MatlabReader.Read<double>("testrt.mat", "diagonalDouble").Equals(diagonalDouble));
            Assert.That(MatlabReader.Read<double>("testrt.mat", "sparseDouble").Equals(sparseDouble));

            Assert.That(MatlabReader.Read<double>("testrt.mat", "denseDouble").Storage, Is.TypeOf<DenseColumnMajorMatrixStorage<double>>());
            Assert.That(MatlabReader.Read<Complex>("testrt.mat", "denseComplex").Storage, Is.TypeOf<DenseColumnMajorMatrixStorage<Complex>>());
            Assert.That(MatlabReader.Read<double>("testrt.mat", "diagonalDouble").Storage, Is.TypeOf<SparseCompressedRowMatrixStorage<double>>());
            Assert.That(MatlabReader.Read<double>("testrt.mat", "sparseDouble").Storage, Is.TypeOf<SparseCompressedRowMatrixStorage<double>>());

            File.Delete("testrt.mat");
        }
    }
}
