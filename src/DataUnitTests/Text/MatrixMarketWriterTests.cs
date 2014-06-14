// <copyright file="MatrixMarketWriterTests.cs" company="Math.NET">
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

using System;
using System.IO;
using System.Numerics;
using MathNet.Numerics.Data.Text;
using NUnit.Framework;

namespace MathNet.Numerics.Data.UnitTests.Text
{
    [TestFixture]
    public class MatrixMarketWriterTests
    {
        [Test]
        public void CanWriteRealDenseMatrix()
        {
            var m = LinearAlgebra.Double.DenseMatrix.OfArray(new[,] {{1.5d, 2}, {3, 4}, {5, 6}});
            const string expected = "%%MatrixMarket matrix array real general\n3 2\n1.5\n3\n5\n2\n4\n6\n";
            var writer = new StringWriter();
            MatrixMarketWriter.WriteMatrix(writer, m);
            Assert.AreEqual(expected.Replace("\n",Environment.NewLine), writer.ToString());
        }

        [Test]
        public void CanWriteComplexDenseMatrix()
        {
            var m = LinearAlgebra.Complex.DenseMatrix.OfArray(new[,] { { new Complex(1.5, 0), new Complex(0, 1) }, { new Complex(4, 5), new Complex(6, 7) } });
            const string expected = "%%MatrixMarket matrix array complex general\n2 2\n1.5 0\n4 5\n0 1\n6 7\n";
            var writer = new StringWriter();
            MatrixMarketWriter.WriteMatrix(writer, m);
            Assert.AreEqual(expected.Replace("\n", Environment.NewLine), writer.ToString());
        }

        [Test]
        public void CanWriteRealSparseMatrix()
        {
            var m = LinearAlgebra.Double.SparseMatrix.OfArray(new[,] { { 1.5d, 2 }, { 0, 4 }, { 5, 0 } });
            const string expected = "%%MatrixMarket matrix coordinate real general\n3 2 4\n1 1 1.5\n1 2 2\n2 2 4\n3 1 5\n";
            var writer = new StringWriter();
            MatrixMarketWriter.WriteMatrix(writer, m);
            Assert.AreEqual(expected.Replace("\n", Environment.NewLine), writer.ToString());
        }
    }
}
