// <copyright file="MatrixMarketReaderTests.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
//
// Copyright (c) 2009-2013 Math.NET
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
// </copyright>

using System.IO;
using System.Numerics;
using System.Text;
using MathNet.Numerics.Data.Text;
using NUnit.Framework;

namespace MathNet.Numerics.Data.UnitTests.Text
{
    [TestFixture]
    public class MatrixMarketReaderTests
    {
        [Test]
        public void CanReadFudao007AsDouble()
        {
            using (Stream stream = TestData.Data.ReadStream("MatrixMarket.fidap007.mtx"))
            {
                var m = MatrixMarketReader.ReadMatrix<double>(stream);
                Assert.IsInstanceOf<LinearAlgebra.Double.SparseMatrix>(m);
                Assert.AreEqual(1633, m.RowCount);
                Assert.AreEqual(1633, m.ColumnCount);
                Assert.GreaterOrEqual(54487, ((LinearAlgebra.Double.SparseMatrix)m).NonZerosCount);
                Assert.Less(46000, ((LinearAlgebra.Double.SparseMatrix)m).NonZerosCount);
                Assert.AreEqual(-6.8596032449032e+06d, m[1604, 1631]);
                Assert.AreEqual(-9.1914585107976e+06d, m[1616, 1628]);
                Assert.AreEqual(7.9403870156486e+07d, m[905, 726]);
            }
        }

        [Test]
        public void CanReadFudao007AsSingle()
        {
            using (Stream stream = TestData.Data.ReadStream("MatrixMarket.fidap007.mtx"))
            {
                var m = MatrixMarketReader.ReadMatrix<float>(stream);
                Assert.IsInstanceOf<LinearAlgebra.Single.SparseMatrix>(m);
                Assert.AreEqual(1633, m.RowCount);
                Assert.AreEqual(1633, m.ColumnCount);
                Assert.GreaterOrEqual(54487, ((LinearAlgebra.Single.SparseMatrix)m).NonZerosCount);
                Assert.Less(46000, ((LinearAlgebra.Single.SparseMatrix)m).NonZerosCount);
                Assert.AreEqual(-6.8596032449032e+06f, m[1604, 1631]);
                Assert.AreEqual(-9.1914585107976e+06f, m[1616, 1628]);
                Assert.AreEqual(7.9403870156486e+07f, m[905, 726]);
            }
        }

        [Test]
        public void CanReadFudao007AsComplex()
        {
            using (TextReader reader = TestData.Data.ReadText("MatrixMarket.fidap007.mtx"))
            {
                var m = MatrixMarketReader.ReadMatrix<Complex>(reader);
                Assert.IsInstanceOf<LinearAlgebra.Complex.SparseMatrix>(m);
                Assert.AreEqual(1633, m.RowCount);
                Assert.AreEqual(1633, m.ColumnCount);
                Assert.GreaterOrEqual(54487, ((LinearAlgebra.Complex.SparseMatrix)m).NonZerosCount);
                Assert.Less(46000, ((LinearAlgebra.Complex.SparseMatrix)m).NonZerosCount);
                Assert.AreEqual(-6.8596032449032e+06d, m[1604, 1631].Real);
                Assert.AreEqual(0.0d, m[1604, 1631].Imaginary);
                Assert.AreEqual(-9.1914585107976e+06d, m[1616, 1628].Real);
                Assert.AreEqual(0.0d, m[1616, 1628].Imaginary);
                Assert.AreEqual(7.9403870156486e+07d, m[905, 726].Real);
                Assert.AreEqual(0.0d, m[905, 726].Imaginary);
            }
        }

        [Test]
        public void CanReadFudao007AsComplex32()
        {
            using (TextReader reader = TestData.Data.ReadText("MatrixMarket.fidap007.mtx"))
            {
                var m = MatrixMarketReader.ReadMatrix<Complex32>(reader);
                Assert.IsInstanceOf<LinearAlgebra.Complex32.SparseMatrix>(m);
                Assert.AreEqual(1633, m.RowCount);
                Assert.AreEqual(1633, m.ColumnCount);
                Assert.GreaterOrEqual(54487, ((LinearAlgebra.Complex32.SparseMatrix)m).NonZerosCount);
                Assert.Less(46000, ((LinearAlgebra.Complex32.SparseMatrix)m).NonZerosCount);
                Assert.AreEqual(-6.8596032449032e+06f, m[1604, 1631].Real);
                Assert.AreEqual(0.0f, m[1604, 1631].Imaginary);
                Assert.AreEqual(-9.1914585107976e+06f, m[1616, 1628].Real);
                Assert.AreEqual(0.0f, m[1616, 1628].Imaginary);
                Assert.AreEqual(7.9403870156486e+07f, m[905, 726].Real);
                Assert.AreEqual(0.0f, m[905, 726].Imaginary);
            }
        }

        [Test]
        public void CanReadSparseHermitianComplexMatrix()
        {
            var sb = new StringBuilder();
            sb.AppendLine("%%MatrixMarket matrix coordinate complex hermitian");
            sb.AppendLine("5 5 7");
            sb.AppendLine("1 1  1.0 0");
            sb.AppendLine("2 2  10.5 0");
            sb.AppendLine("4 2  250.5 22.22");
            sb.AppendLine("3 3  1.5e-2 0");
            sb.AppendLine("4 4  -2.8e2 0.0");
            sb.AppendLine("5 5  12. 0.");
            sb.AppendLine("5 4  0 33.32");

            var m = MatrixMarketReader.ReadMatrix<Complex>(new StringReader(sb.ToString()));
            Assert.IsInstanceOf<LinearAlgebra.Complex.SparseMatrix>(m);
            Assert.AreEqual(5, m.RowCount);
            Assert.AreEqual(5, m.ColumnCount);
            Assert.AreEqual(9, ((LinearAlgebra.Complex.SparseMatrix)m).NonZerosCount);
            Assert.AreEqual(1.0d, m[0, 0].Real);
            Assert.AreEqual(10.5d, m[1, 1].Real);
            Assert.AreEqual(250.5d, m[3, 1].Real);
            Assert.AreEqual(22.22d, m[3, 1].Imaginary);
            Assert.AreEqual(-22.22d, m[1, 3].Imaginary);
            Assert.AreEqual(-280d, m[3, 3].Real);
            Assert.AreEqual(0d, m[4, 3].Real);
            Assert.AreEqual(33.32d, m[4, 3].Imaginary);
            Assert.AreEqual(-33.32d, m[3, 4].Imaginary);
        }

        [Test]
        public void CanReadDenseSkewSymmetricMatrix()
        {
            /*  0  1  2  3
                1  0  6  7
                2  6  0 11
                3  7 11  0  */

            var sb = new StringBuilder();
            sb.AppendLine("%%MatrixMarket matrix array integer skew-symmetric");
            sb.AppendLine("4 4");
            sb.Append("1\n2\n3\n6\n7\n11");

            var m = MatrixMarketReader.ReadMatrix<double>(new StringReader(sb.ToString()));
            Assert.IsInstanceOf<LinearAlgebra.Double.DenseMatrix>(m);
            Assert.AreEqual(4, m.RowCount);
            Assert.AreEqual(4, m.ColumnCount);
            Assert.AreEqual(0.0, m.Diagonal().InfinityNorm());
            Assert.AreEqual(0d, m[0, 0]);
            Assert.AreEqual(1d, m[1, 0]);
            Assert.AreEqual(1d, m[0, 1]);
            Assert.AreEqual(7d, m[3, 1]);
            Assert.AreEqual(7d, m[1, 3]);
        }
    }
}
