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

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Double.IO
{
    using LinearAlgebra.Double;
    using LinearAlgebra.Double.IO;
    using NUnit.Framework;

    /// <summary>
    /// Matlab matrix reader test.
    /// </summary>
    [TestFixture]
    public class MatlabMatrixReaderTests
    {
        /// <summary>
        /// Can read all matrices.
        /// </summary>
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

        /// <summary>
        /// Can read first matrix.
        /// </summary>
        [Test]
        public void CanReadFirstMatrix()
        {
            var dmr = new MatlabMatrixReader("./data/Matlab/A.mat");
            var matrix = dmr.ReadMatrix();
            Assert.AreEqual(100, matrix.RowCount);
            Assert.AreEqual(100, matrix.ColumnCount);
            Assert.AreEqual(typeof(DenseMatrix), matrix.GetType());
            AssertHelpers.AlmostEqual(100.108979553704, matrix.FrobeniusNorm(), 5);
        }

        /// <summary>
        /// Can read named matrices.
        /// </summary>
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

        /// <summary>
        /// Can read named matrix.
        /// </summary>
        [Test]
        public void CanReadNamedMatrix()
        {
            var dmr = new MatlabMatrixReader("./data/Matlab/collection.mat");
            var matrices = dmr.ReadMatrices(new[] { "Ad" });
            Assert.AreEqual(1, matrices.Count);
            var ad = matrices["Ad"];
            Assert.AreEqual(100, ad.RowCount);
            Assert.AreEqual(100, ad.ColumnCount);
            AssertHelpers.AlmostEqual(100.431635988639, ad.FrobeniusNorm(), 5);
            Assert.AreEqual(typeof(DenseMatrix), ad.GetType());
        }

        /// <summary>
        /// Can read named sparse matrix.
        /// </summary>
        [Test]
        public void CanReadNamedSparseMatrix()
        {
            var dmr = new MatlabMatrixReader("./data/Matlab/sparse-small.mat");
            var matrix = dmr.ReadMatrix("S");
            Assert.AreEqual(100, matrix.RowCount);
            Assert.AreEqual(100, matrix.ColumnCount);
            Assert.AreEqual(typeof(SparseMatrix), matrix.GetType());
            AssertHelpers.AlmostEqual(17.6385090630805, matrix.FrobeniusNorm(), 12);
        }
    }
}
