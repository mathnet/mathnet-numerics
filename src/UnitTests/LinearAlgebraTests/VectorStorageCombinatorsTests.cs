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
    public class VectorStorageCombinatorsTests
    {
        [Datapoints]
        TestVectorStorage[] _storage =
        {
            TestVectorStorage.DenseVector,
            TestVectorStorage.SparseVector,
        };

        [Theory]
        public void MapToSkipZeros(TestVectorStorage aType, TestVectorStorage resultType)
        {
            var a = TestData.VectorStorage(aType, new[] { 1.0, 2.0, 0.0, 4.0 });
            var result = TestData.VectorStorage<double>(resultType, 4);
            var expected = new DenseVectorStorage<double>(4, new[] { -1.0, -2.0, 0.0, -4.0 });
            a.MapTo(result, u => -u, Zeros.AllowSkip);
            Assert.That(result.Equals(expected));
        }

        [Theory]
        public void MapToForceIncludeZeros(TestVectorStorage aType, TestVectorStorage resultType)
        {
            var a = TestData.VectorStorage(aType, new[] { 1.0, 2.0, 0.0, 4.0 });
            var result = TestData.VectorStorage<double>(resultType, 4);
            var expected = new DenseVectorStorage<double>(4, new[] { 0.0, -1.0, 1.0, -3.0 });
            a.MapTo(result, u => -u + 1.0, Zeros.Include);
            Assert.That(result.Equals(expected));
        }

        [Theory]
        public void MapToAutoIncludeZeros(TestVectorStorage aType, TestVectorStorage resultType)
        {
            var a = TestData.VectorStorage(aType, new[] { 1.0, 2.0, 0.0, 4.0 });
            var result = TestData.VectorStorage<double>(resultType, 4);
            var expected = new DenseVectorStorage<double>(4, new[] { 0.0, -1.0, 1.0, -3.0 });
            a.MapTo(result, u => -u + 1.0, Zeros.AllowSkip);
            Assert.That(result.Equals(expected));
        }

        [Theory]
        public void MapIndexedToSkipZeros(TestVectorStorage aType, TestVectorStorage resultType)
        {
            var a = TestData.VectorStorage(aType, new[] { 1.0, 2.0, 0.0, 4.0 });
            var result = TestData.VectorStorage<double>(resultType, 4);
            var expected = new DenseVectorStorage<double>(4, new[] { -1.0, -2.0, 0.0, -4.0 });
            int badValueCount = 0; // one time is OK for zero-check
            a.MapIndexedTo(result, (i, u) => { if (a.At(i) != u) Interlocked.Increment(ref badValueCount); return -u; }, Zeros.AllowSkip);
            Assert.That(badValueCount, Is.LessThanOrEqualTo(1));
            Assert.That(result.Equals(expected));
        }

        [Theory]
        public void MapIndexedToForceIncludeZeros(TestVectorStorage aType, TestVectorStorage resultType)
        {
            var a = TestData.VectorStorage(aType, new[] { 1.0, 2.0, 0.0, 4.0 });
            var result = TestData.VectorStorage<double>(resultType, 4);
            var expected = new DenseVectorStorage<double>(4, new[] { 0.0, -1.0, 1.0, -3.0 });
            int badValueCount = 0; // one time is OK for zero-check
            a.MapIndexedTo(result, (i, u) => { if (a.At(i) != u) Interlocked.Increment(ref badValueCount); return -u + 1.0; }, Zeros.Include);
            Assert.That(badValueCount, Is.LessThanOrEqualTo(1));
            Assert.That(result.Equals(expected));
        }

        [Theory]
        public void MapIndexedToAutoIncludeZeros(TestVectorStorage aType, TestVectorStorage resultType)
        {
            var a = TestData.VectorStorage(aType, new[] { 1.0, 2.0, 0.0, 4.0 });
            var result = TestData.VectorStorage<double>(resultType, 4);
            var expected = new DenseVectorStorage<double>(4, new[] { 0.0, -1.0, 1.0, -3.0 });
            int badValueCount = 0; // one time is OK for zero-check
            a.MapIndexedTo(result, (i, u) => { if (a.At(i) != u) Interlocked.Increment(ref badValueCount); return -u + 1.0; }, Zeros.AllowSkip);
            Assert.That(badValueCount, Is.LessThanOrEqualTo(1));
            Assert.That(result.Equals(expected));
        }

        [Theory]
        public void Map2ToSkipZeros(TestVectorStorage aType, TestVectorStorage bType, TestVectorStorage resultType)
        {
            var a = TestData.VectorStorage(aType, new[] { 1.0, 2.0, 0.0, 4.0, 0.0, 6.0 });
            var b = TestData.VectorStorage(bType, new[] { 11.0, 12.0, 13.0, 0.0, 0.0, 16.0 });
            var result = TestData.VectorStorage<double>(resultType, 6);
            var expected = new DenseVectorStorage<double>(6, new[] { 12.0, 14.0, 13.0, 4.0, 0.0, 22.0 });
            a.Map2To(result, b, (u, v) => u + v, Zeros.AllowSkip);
            Assert.That(result.Equals(expected));
        }

        [Theory]
        public void Map2ToForceIncludeZeros(TestVectorStorage aType, TestVectorStorage bType, TestVectorStorage resultType)
        {
            var a = TestData.VectorStorage(aType, new[] { 1.0, 2.0, 0.0, 4.0, 0.0, 6.0 });
            var b = TestData.VectorStorage(bType, new[] { 11.0, 12.0, 13.0, 0.0, 0.0, 16.0 });
            var result = TestData.VectorStorage<double>(resultType, 6);
            var expected = new DenseVectorStorage<double>(6, new[] { 13.0, 15.0, 14.0, 5.0, 1.0, 23.0 });
            a.Map2To(result, b, (u, v) => u + v + 1.0, Zeros.Include);
            Assert.That(result.Equals(expected));
        }

        [Theory]
        public void Map2ToAutoIncludeZeros(TestVectorStorage aType, TestVectorStorage bType, TestVectorStorage resultType)
        {
            var a = TestData.VectorStorage(aType, new[] { 1.0, 2.0, 0.0, 4.0, 0.0, 6.0 });
            var b = TestData.VectorStorage(bType, new[] { 11.0, 12.0, 13.0, 0.0, 0.0, 16.0 });
            var result = TestData.VectorStorage<double>(resultType, 6);
            var expected = new DenseVectorStorage<double>(6, new[] { 13.0, 15.0, 14.0, 5.0, 1.0, 23.0 });
            a.Map2To(result, b, (u, v) => u + v + 1.0, Zeros.AllowSkip);
            Assert.That(result.Equals(expected));
        }

        [Theory]
        public void Fold2SkipZeros(TestVectorStorage aType, TestVectorStorage bType)
        {
            var a = TestData.VectorStorage(aType, new[] { 1.0, 2.0, 0.0, 4.0, 0.0, 6.0 });
            var b = TestData.VectorStorage(bType, new[] { 11.0, 12.0, 13.0, 0.0, 0.0, 16.0 });
            var result = a.Fold2(b, (acc, u, v) => acc + u + v, 0.0, Zeros.AllowSkip);
            Assert.That(result, Is.EqualTo(65));
        }

        [Theory]
        public void Fold2ForceIncludeZeros(TestVectorStorage aType, TestVectorStorage bType)
        {
            var a = TestData.VectorStorage(aType, new[] { 1.0, 2.0, 0.0, 4.0, 0.0, 6.0 });
            var b = TestData.VectorStorage(bType, new[] { 11.0, 12.0, 13.0, 0.0, 0.0, 16.0 });
            var result = a.Fold2(b, (acc, u, v) => acc + u + v + 1.0, 0.0, Zeros.Include);
            Assert.That(result, Is.EqualTo(71));
        }
    }
}
