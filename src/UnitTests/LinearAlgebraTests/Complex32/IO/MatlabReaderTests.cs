// <copyright file="MatlabReaderTests.cs" company="Math.NET">
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

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Complex32.IO
{
    using LinearAlgebra.Complex32;
    using LinearAlgebra.Complex32.IO;
    using NUnit.Framework;

    /// <summary>
    /// Matlab matrix reader test.
    /// </summary>
    [TestFixture]
    public class MatlabMatrixReaderTests
    {
        /// <summary>
        /// Can read all complex matrices.
        /// </summary>
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

        /// <summary>
        /// Can read spapse complex matrices.
        /// </summary>
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
            AssertHelpers.AlmostEqual(13.223654390985379, a.L2Norm().Real, 5);
        }

        /// <summary>
        /// Can read non-complex matrices.
        /// </summary>
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

        /// <summary>
        /// Can read non-complex first matrix.
        /// </summary>
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

        /// <summary>
        /// Can read non-complex named matrices.
        /// </summary>
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

        /// <summary>
        /// Can read non-complex named matrix.
        /// </summary>
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

        /// <summary>
        /// Can read non-complex named sparse matrix.
        /// </summary>
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
