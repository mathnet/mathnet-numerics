// <copyright file="VectorStorageCombinatorsTests.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2016 Math.NET
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
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Storage;
using NUnit.Framework;

namespace MathNet.Numerics.Tests.LinearAlgebraTests
{
    [TestFixture, Category("LA")]
    public class MatrixTests
    {
        [Test]
        public void DenseMatrixBuilderMethos_ZeroLength_DoNotThrowException()
        {
            Assert.DoesNotThrow(() => Matrix<double>.Build.Dense(0, 0));
            Assert.DoesNotThrow(() => Matrix<double>.Build.Dense(0, 0, 42));
            Assert.DoesNotThrow(() => Matrix<double>.Build.Dense(0, 0, Array.Empty<double>()));
            Assert.DoesNotThrow(() => Matrix<double>.Build.Dense(0, 0, (row, column) => 42));
        }

        [Test]
        public void SparseMatrixBuilderMethos_ZeroLength_DoNotThrowException()
        {
            Assert.DoesNotThrow(() => Matrix<double>.Build.Sparse(0, 0));
            Assert.DoesNotThrow(() => Matrix<double>.Build.Sparse(0, 0, 42));
            Assert.DoesNotThrow(() => Matrix<double>.Build.Sparse(0, 0, (row, column) => 42));
        }

        [Test]
        public void DenseColumnMajorMatrixStorageBuilderMethods_ZeroLength_DoNotThrowException()
        {
            Assert.DoesNotThrow(() => new DenseColumnMajorMatrixStorage<double>(0, 0));
            Assert.DoesNotThrow(() => new DenseColumnMajorMatrixStorage<double>(0, 0, Array.Empty<double>()));
        }

        [Test]
        public void DiagonalMatrixStorageBuilderMethods_ZeroLength_DoNotThrowException()
        {
            Assert.DoesNotThrow(() => new DiagonalMatrixStorage<double>(0, 0));
            Assert.DoesNotThrow(() => new DiagonalMatrixStorage<double>(0, 0, Array.Empty<double>()));
        }

        [Test]
        public void SparseCompressedRowMatrixStorageBuilderMethods_ZeroLength_DoNotThrowException()
        {
            Assert.DoesNotThrow(() => new SparseCompressedRowMatrixStorage<double>(0, 0));
        }

        [Test]
        public void SparseCompressedRowMatrixStorage_CoordinateFormatDuplicateRemovalCheck()
        {
            const double tol = 1e-6;
            var matDuplicates = MatrixHelpers.ReadTestDataSparseMatrixDoubleCoordinateFormat("coo_torsion_duplicates.csv");
            var matNoDuplicates = MatrixHelpers.ReadTestDataSparseMatrixDoubleCoordinateFormat("coo_torsion_no_duplicates.csv");

            var rowCount = matDuplicates.RowCount;
            var columnCount = matDuplicates.ColumnCount;

            Assert.AreEqual(matNoDuplicates.RowCount, rowCount);
            Assert.AreEqual(matNoDuplicates.ColumnCount, columnCount);

            for (var r = 0; r < rowCount; r++)
            for (var c = 0; c < columnCount; c++)
            {
                var actual = matDuplicates[r, c];
                var expected = matNoDuplicates[r, c];
                Assert.True(Math.Abs(actual - expected) < tol, $"Expected {expected:E6} at ({r}, {c}), but got {actual:E6}");
            }
        }

        [Test]
        public void PointwiseMultiplication_SparseReturnsSameResultAsDenseTest()
        {
            var x = SparseMatrix.OfDiagonalArray(new double[] { 1, 2, 3, 4 });
            var y = DenseMatrix.OfDiagonalArray(new double[] { 5, -6, 7, -8 });
            x.PointwiseMultiply(y, x);
            var result = SparseMatrix.OfDiagonalArray(new double[] { 5, -12, 21, -32 });
            Assert.AreEqual(result, x);
        }

        [Test]
        public void PointwiseDivision_SparseReturnsSameResultAsDenseTest()
        {
            var x = SparseMatrix.OfDiagonalArray(new double[] { 30, 20, 10 });
            var y = DenseMatrix.OfDiagonalArray(new double[] { 3, 4, 5 });
            x.PointwiseDivide(y, x);
            var result = SparseMatrix.OfDiagonalArray(new double[] { 10, 5, 2 });
            Assert.AreEqual(result, x);
        }
    }
}
