// <copyright file="VectorStorageTests.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
//
// Copyright (c) 2009-2014 Math.NET
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

using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Storage;
using NUnit.Framework;

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests
{
    [TestFixture, Category("LA")]
    public class VectorStorageTests
    {
        [Test]
        public void MapToSkipZeros()
        {
            double[] a = { 1.0, 2.0, 0.0, 4.0 };
            var adense = DenseVectorStorage<double>.OfEnumerable(a);
            var asparse = SparseVectorStorage<double>.OfEnumerable(a);

            var rdense = new DenseVectorStorage<double>(a.Length);
            var rsparse = new SparseVectorStorage<double>(a.Length);

            var expected = new DenseVectorStorage<double>(4, new[] { -1.0, -2.0, 0.0, -4.0 });

            rdense.Clear();
            adense.MapTo(rdense, u => -u, Zeros.AllowSkip);
            Assert.That(rdense.Equals(expected), "dense->dense");

            rsparse.Clear();
            adense.MapTo(rsparse, u => -u, Zeros.AllowSkip);
            Assert.That(rsparse.Equals(expected), "dense->sparse");

            rdense.Clear();
            asparse.MapTo(rdense, u => -u, Zeros.AllowSkip);
            Assert.That(rdense.Equals(expected), "sparse->dense");

            rsparse.Clear();
            asparse.MapTo(rsparse, u => -u, Zeros.AllowSkip);
            Assert.That(rsparse.Equals(expected), "sparse->sparse");
        }

        [Test]
        public void MapToForceIncludeZeros()
        {
            double[] a = { 1.0, 2.0, 0.0, 4.0 };
            var adense = DenseVectorStorage<double>.OfEnumerable(a);
            var asparse = SparseVectorStorage<double>.OfEnumerable(a);

            var rdense = new DenseVectorStorage<double>(a.Length);
            var rsparse = new SparseVectorStorage<double>(a.Length);

            var expected = new DenseVectorStorage<double>(4, new[] { 0.0, -1.0, 1.0, -3.0 });

            rdense.Clear();
            adense.MapTo(rdense, u => -u + 1.0, Zeros.Include);
            Assert.That(rdense.Equals(expected), "dense->dense");

            rsparse.Clear();
            adense.MapTo(rsparse, u => -u + 1.0, Zeros.Include);
            Assert.That(rsparse.Equals(expected), "dense->sparse");

            rdense.Clear();
            asparse.MapTo(rdense, u => -u + 1.0, Zeros.Include);
            Assert.That(rdense.Equals(expected), "sparse->dense");

            rsparse.Clear();
            asparse.MapTo(rsparse, u => -u + 1.0, Zeros.Include);
            Assert.That(rsparse.Equals(expected), "sparse->sparse");
        }

        [Test]
        public void MapToAutoIncludeZeros()
        {
            double[] a = { 1.0, 2.0, 0.0, 4.0 };
            var adense = DenseVectorStorage<double>.OfEnumerable(a);
            var asparse = SparseVectorStorage<double>.OfEnumerable(a);

            var rdense = new DenseVectorStorage<double>(a.Length);
            var rsparse = new SparseVectorStorage<double>(a.Length);

            var expected = new DenseVectorStorage<double>(4, new[] { 0.0, -1.0, 1.0, -3.0 });

            rdense.Clear();
            adense.MapTo(rdense, u => -u + 1.0, Zeros.AllowSkip);
            Assert.That(rdense.Equals(expected), "dense->dense");

            rsparse.Clear();
            adense.MapTo(rsparse, u => -u + 1.0, Zeros.AllowSkip);
            Assert.That(rsparse.Equals(expected), "dense->sparse");

            rdense.Clear();
            asparse.MapTo(rdense, u => -u + 1.0, Zeros.AllowSkip);
            Assert.That(rdense.Equals(expected), "sparse->dense");

            rsparse.Clear();
            asparse.MapTo(rsparse, u => -u + 1.0, Zeros.AllowSkip);
            Assert.That(rsparse.Equals(expected), "sparse->sparse");
        }

        [Test]
        public void Map2ToSkipZeros()
        {
            double[] a = { 1.0, 2.0, 0.0, 4.0, 0.0, 6.0 };
            double[] b = { 11.0, 12.0, 13.0, 0.0, 0.0, 16.0 };
            var adense = DenseVectorStorage<double>.OfEnumerable(a);
            var asparse = SparseVectorStorage<double>.OfEnumerable(a);
            var bdense = DenseVectorStorage<double>.OfEnumerable(b);
            var bsparse = SparseVectorStorage<double>.OfEnumerable(b);

            var rdense = new DenseVectorStorage<double>(a.Length);
            var rsparse = new SparseVectorStorage<double>(a.Length);

            var expected = new DenseVectorStorage<double>(6, new[] { 12.0, 14.0, 13.0, 4.0, 0.0, 22.0 });

            rdense.Clear();
            adense.Map2To(rdense, bdense, (u, v) => u + v, Zeros.AllowSkip);
            Assert.That(rdense.Equals(expected), "dense*dense->dense");

            rsparse.Clear();
            adense.Map2To(rsparse, bdense, (u, v) => u + v, Zeros.AllowSkip);
            Assert.That(rsparse.Equals(expected), "dense*dense->sparse");

            rdense.Clear();
            adense.Map2To(rdense, bsparse, (u, v) => u + v, Zeros.AllowSkip);
            Assert.That(rdense.Equals(expected), "dense*sparse->dense");

            rsparse.Clear();
            adense.Map2To(rsparse, bsparse, (u, v) => u + v, Zeros.AllowSkip);
            Assert.That(rsparse.Equals(expected), "dense*sparse->sparse");

            rdense.Clear();
            asparse.Map2To(rdense, bdense, (u, v) => u + v, Zeros.AllowSkip);
            Assert.That(rdense.Equals(expected), "sparse*dense->dense");

            rsparse.Clear();
            asparse.Map2To(rsparse, bdense, (u, v) => u + v, Zeros.AllowSkip);
            Assert.That(rsparse.Equals(expected), "sparse*dense->sparse");

            rdense.Clear();
            asparse.Map2To(rdense, bsparse, (u, v) => u + v, Zeros.AllowSkip);
            Assert.That(rdense.Equals(expected), "sparse*sparse->dense");

            rsparse.Clear();
            asparse.Map2To(rsparse, bsparse, (u, v) => u + v, Zeros.AllowSkip);
            Assert.That(rsparse.Equals(expected), "sparse*sparse->sparse");
        }

        [Test]
        public void Map2ToForceIncludeZeros()
        {
            double[] a = { 1.0, 2.0, 0.0, 4.0, 0.0, 6.0 };
            double[] b = { 11.0, 12.0, 13.0, 0.0, 0.0, 16.0 };
            var adense = DenseVectorStorage<double>.OfEnumerable(a);
            var asparse = SparseVectorStorage<double>.OfEnumerable(a);
            var bdense = DenseVectorStorage<double>.OfEnumerable(b);
            var bsparse = SparseVectorStorage<double>.OfEnumerable(b);

            var rdense = new DenseVectorStorage<double>(a.Length);
            var rsparse = new SparseVectorStorage<double>(a.Length);

            var expected = new DenseVectorStorage<double>(6, new[] { 13.0, 15.0, 14.0, 5.0, 1.0, 23.0 });

            rdense.Clear();
            adense.Map2To(rdense, bdense, (u, v) => u + v + 1.0, Zeros.Include);
            Assert.That(rdense.Equals(expected), "dense*dense->dense");

            rsparse.Clear();
            adense.Map2To(rsparse, bdense, (u, v) => u + v + 1.0, Zeros.Include);
            Assert.That(rsparse.Equals(expected), "dense*dense->sparse");

            rdense.Clear();
            adense.Map2To(rdense, bsparse, (u, v) => u + v + 1.0, Zeros.Include);
            Assert.That(rdense.Equals(expected), "dense*sparse->dense");

            rsparse.Clear();
            adense.Map2To(rsparse, bsparse, (u, v) => u + v + 1.0, Zeros.Include);
            Assert.That(rsparse.Equals(expected), "dense*sparse->sparse");

            rdense.Clear();
            asparse.Map2To(rdense, bdense, (u, v) => u + v + 1.0, Zeros.Include);
            Assert.That(rdense.Equals(expected), "sparse*dense->dense");

            rsparse.Clear();
            asparse.Map2To(rsparse, bdense, (u, v) => u + v + 1.0, Zeros.Include);
            Assert.That(rsparse.Equals(expected), "sparse*dense->sparse");

            rdense.Clear();
            asparse.Map2To(rdense, bsparse, (u, v) => u + v + 1.0, Zeros.Include);
            Assert.That(rdense.Equals(expected), "sparse*sparse->dense");

            rsparse.Clear();
            asparse.Map2To(rsparse, bsparse, (u, v) => u + v + 1.0, Zeros.Include);
            Assert.That(rsparse.Equals(expected), "sparse*sparse->sparse");
        }

        [Test]
        public void Map2ToAutoIncludeZeros()
        {
            double[] a = { 1.0, 2.0, 0.0, 4.0, 0.0, 6.0 };
            double[] b = { 11.0, 12.0, 13.0, 0.0, 0.0, 16.0 };
            var adense = DenseVectorStorage<double>.OfEnumerable(a);
            var asparse = SparseVectorStorage<double>.OfEnumerable(a);
            var bdense = DenseVectorStorage<double>.OfEnumerable(b);
            var bsparse = SparseVectorStorage<double>.OfEnumerable(b);

            var rdense = new DenseVectorStorage<double>(a.Length);
            var rsparse = new SparseVectorStorage<double>(a.Length);

            var expected = new DenseVectorStorage<double>(6, new[] { 13.0, 15.0, 14.0, 5.0, 1.0, 23.0 });

            rdense.Clear();
            adense.Map2To(rdense, bdense, (u, v) => u + v + 1.0, Zeros.AllowSkip);
            Assert.That(rdense.Equals(expected), "dense*dense->dense");

            rsparse.Clear();
            adense.Map2To(rsparse, bdense, (u, v) => u + v + 1.0, Zeros.AllowSkip);
            Assert.That(rsparse.Equals(expected), "dense*dense->sparse");

            rdense.Clear();
            adense.Map2To(rdense, bsparse, (u, v) => u + v + 1.0, Zeros.AllowSkip);
            Assert.That(rdense.Equals(expected), "dense*sparse->dense");

            rsparse.Clear();
            adense.Map2To(rsparse, bsparse, (u, v) => u + v + 1.0, Zeros.AllowSkip);
            Assert.That(rsparse.Equals(expected), "dense*sparse->sparse");

            rdense.Clear();
            asparse.Map2To(rdense, bdense, (u, v) => u + v + 1.0, Zeros.AllowSkip);
            Assert.That(rdense.Equals(expected), "sparse*dense->dense");

            rsparse.Clear();
            asparse.Map2To(rsparse, bdense, (u, v) => u + v + 1.0, Zeros.AllowSkip);
            Assert.That(rsparse.Equals(expected), "sparse*dense->sparse");

            rdense.Clear();
            asparse.Map2To(rdense, bsparse, (u, v) => u + v + 1.0, Zeros.AllowSkip);
            Assert.That(rdense.Equals(expected), "sparse*sparse->dense");

            rsparse.Clear();
            asparse.Map2To(rsparse, bsparse, (u, v) => u + v + 1.0, Zeros.AllowSkip);
            Assert.That(rsparse.Equals(expected), "sparse*sparse->sparse");
        }
    }
}
