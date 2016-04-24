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

using System.Threading;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Storage;
using NUnit.Framework;

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests
{
    [TestFixture, Category("LA")]
    public class MatrixStorageCombinatorsTests
    {
        [Datapoints]
        TestMatrixStorage[] _storage =
        {
            TestMatrixStorage.DenseMatrix,
            TestMatrixStorage.SparseMatrix,
        };

        [Theory]
        public void MapToSkipZeros(TestMatrixStorage aType, TestMatrixStorage resultType)
        {
            var a = TestData.MatrixStorage(aType, new[,] { {1.0, 2.0}, {0.0, 4.0} });
            var result = TestData.MatrixStorage<double>(resultType, 2, 2);
            var expected = DenseColumnMajorMatrixStorage<double>.OfArray(new[,] { {-1.0, -2.0}, {0.0, -4.0} });
            a.MapTo(result, u => -u, Zeros.AllowSkip);
            Assert.That(result.Equals(expected));
        }

        [Theory]
        public void MapToForceIncludeZeros(TestMatrixStorage aType, TestMatrixStorage resultType)
        {
            var a = TestData.MatrixStorage(aType, new[,] { {1.0, 2.0}, {0.0, 4.0} });
            var result = TestData.MatrixStorage<double>(resultType, 2, 2);
            var expected = DenseColumnMajorMatrixStorage<double>.OfArray(new[,] { {0.0, -1.0}, {1.0, -3.0} });
            a.MapTo(result, u => -u + 1.0, Zeros.Include);
            Assert.That(result.Equals(expected));
        }

        [Theory]
        public void MapToAutoIncludeZeros(TestMatrixStorage aType, TestMatrixStorage resultType)
        {
            var a = TestData.MatrixStorage(aType, new[,] { { 1.0, 2.0 }, { 0.0, 4.0 } });
            var result = TestData.MatrixStorage<double>(resultType, 2, 2);
            var expected = DenseColumnMajorMatrixStorage<double>.OfArray(new[,] { {0.0, -1.0}, {1.0, -3.0} });
            a.MapTo(result, u => -u + 1.0, Zeros.AllowSkip);
            Assert.That(result.Equals(expected));
        }

        [Theory]
        public void MapIndexedToSkipZeros(TestMatrixStorage aType, TestMatrixStorage resultType)
        {
            var a = TestData.MatrixStorage(aType, new[,] { { 1.0, 2.0 }, { 0.0, 4.0 } });
            var result = TestData.MatrixStorage<double>(resultType, 2, 2);
            var expected = DenseColumnMajorMatrixStorage<double>.OfArray(new[,] { {-1.0, -2.0}, {0.0, -4.0} });
            int badValueCount = 0; // one time is OK for zero-check
            a.MapIndexedTo(result, (i, j, u) => { if (a.At(i, j) != u) Interlocked.Increment(ref badValueCount); return -u; }, Zeros.AllowSkip);
            Assert.That(badValueCount, Is.LessThanOrEqualTo(1));
            Assert.That(result.Equals(expected));
        }

        [Theory]
        public void MapIndexedToForceIncludeZeros(TestMatrixStorage aType, TestMatrixStorage resultType)
        {
            var a = TestData.MatrixStorage(aType, new[,] { { 1.0, 2.0 }, { 0.0, 4.0 } });
            var result = TestData.MatrixStorage<double>(resultType, 2, 2);
            var expected = DenseColumnMajorMatrixStorage<double>.OfArray(new[,] { {0.0, -1.0}, {1.0, -3.0} });
            int badValueCount = 0; // one time is OK for zero-check
            a.MapIndexedTo(result, (i, j, u) => { if (a.At(i, j) != u) Interlocked.Increment(ref badValueCount); return -u + 1.0; }, Zeros.Include);
            Assert.That(badValueCount, Is.LessThanOrEqualTo(1));
            Assert.That(result.Equals(expected));
        }

        [Theory]
        public void MapIndexedToAutoIncludeZeros(TestMatrixStorage aType, TestMatrixStorage resultType)
        {
            var a = TestData.MatrixStorage(aType, new[,] { { 1.0, 2.0 }, { 0.0, 4.0 } });
            var result = TestData.MatrixStorage<double>(resultType, 2, 2);
            var expected = DenseColumnMajorMatrixStorage<double>.OfArray(new[,] { {0.0, -1.0}, {1.0, -3.0} });
            int badValueCount = 0; // one time is OK for zero-check
            a.MapIndexedTo(result, (i, j, u) => { if (a.At(i, j) != u) Interlocked.Increment(ref badValueCount); return -u + 1.0; }, Zeros.AllowSkip);
            Assert.That(badValueCount, Is.LessThanOrEqualTo(1));
            Assert.That(result.Equals(expected));
        }

        [Theory]
        public void Map2ToSkipZeros(TestMatrixStorage aType, TestMatrixStorage bType, TestMatrixStorage resultType)
        {
            var a = TestData.MatrixStorage(aType, new[,] { {1.0, 2.0, 0.0}, {4.0, 0.0, 6.0} });
            var b = TestData.MatrixStorage(bType, new[,] { {11.0, 12.0, 13.0}, {0.0, 0.0, 16.0} });
            var result = TestData.MatrixStorage<double>(resultType, 2, 3);
            var expected = DenseColumnMajorMatrixStorage<double>.OfArray(new[,] { {12.0, 14.0, 13.0}, {4.0, 0.0, 22.0} });
            a.Map2To(result, b, (u, v) => u + v, Zeros.AllowSkip, ExistingData.AssumeZeros);
            Assert.That(result.Equals(expected));
        }

        [Theory]
        public void Map2ToForceIncludeZeros(TestMatrixStorage aType, TestMatrixStorage bType, TestMatrixStorage resultType)
        {
            var a = TestData.MatrixStorage(aType, new[,] { {1.0, 2.0, 0.0}, {4.0, 0.0, 6.0} });
            var b = TestData.MatrixStorage(bType, new[,] { {11.0, 12.0, 13.0}, {0.0, 0.0, 16.0} });
            var result = TestData.MatrixStorage<double>(resultType, 2, 3);
            var expected = DenseColumnMajorMatrixStorage<double>.OfArray(new[,] { {13.0, 15.0, 14.0}, {5.0, 1.0, 23.0} });
            a.Map2To(result, b, (u, v) => u + v + 1.0, Zeros.Include, ExistingData.AssumeZeros);
            Assert.That(result.Equals(expected));
        }

        [Theory]
        public void Map2ToAutoIncludeZeros(TestMatrixStorage aType, TestMatrixStorage bType, TestMatrixStorage resultType)
        {
            var a = TestData.MatrixStorage(aType, new[,] { {1.0, 2.0, 0.0}, {4.0, 0.0, 6.0} });
            var b = TestData.MatrixStorage(bType, new[,] { {11.0, 12.0, 13.0}, {0.0, 0.0, 16.0} });
            var result = TestData.MatrixStorage<double>(resultType, 2, 3);
            var expected = DenseColumnMajorMatrixStorage<double>.OfArray(new[,] { {13.0, 15.0, 14.0}, {5.0, 1.0, 23.0} });
            a.Map2To(result, b, (u, v) => u + v + 1.0, Zeros.AllowSkip, ExistingData.AssumeZeros);
            Assert.That(result.Equals(expected));
        }

        [Theory]
        public void Fold2SkipZeros(TestMatrixStorage aType, TestMatrixStorage bType)
        {
            var a = TestData.MatrixStorage(aType, new[,] { {1.0, 2.0, 0.0}, {4.0, 0.0, 6.0} });
            var b = TestData.MatrixStorage(bType, new[,] { {11.0, 12.0, 13.0}, {0.0, 0.0, 16.0} });
            var result = a.Fold2(b, (acc, u, v) => acc + u + v, 0.0, Zeros.AllowSkip);
            Assert.That(result, Is.EqualTo(65));
        }

        [Theory]
        public void Fold2ForceIncludeZeros(TestMatrixStorage aType, TestMatrixStorage bType)
        {
            var a = TestData.MatrixStorage(aType, new[,] { {1.0, 2.0, 0.0}, {4.0, 0.0, 6.0} });
            var b = TestData.MatrixStorage(bType, new[,] { {11.0, 12.0, 13.0}, {0.0, 0.0, 16.0} });
            var result = a.Fold2(b, (acc, u, v) => acc + u + v + 1.0, 0.0, Zeros.Include);
            Assert.That(result, Is.EqualTo(71));
        }
    }
}
