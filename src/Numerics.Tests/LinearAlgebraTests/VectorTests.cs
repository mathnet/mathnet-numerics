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
using MathNet.Numerics.LinearAlgebra.Storage;
using NUnit.Framework;

namespace MathNet.Numerics.Tests.LinearAlgebraTests
{
    [TestFixture, Category("LA")]
    public class VectorTests
    {
        [Test]
        public void DenseVectorBuilderMethod_ZeroLength_DoNotThrowException()
        {
            Assert.DoesNotThrow(() => Vector<double>.Build.Dense(0));
            Assert.DoesNotThrow(() => Vector<double>.Build.Dense(0, 42));
            Assert.DoesNotThrow(() => Vector<double>.Build.Dense(0, index => 42));
            Assert.DoesNotThrow(() => Vector<double>.Build.Dense(Array.Empty<double>()));
        }

        [Test]
        public void SparseVectorBuilderMethods_ZeroLength_DoNotThrowException()
        {
            Assert.DoesNotThrow(() => Vector<double>.Build.Sparse(0));
            Assert.DoesNotThrow(() => Vector<double>.Build.Sparse(0, 42));
            Assert.DoesNotThrow(() => Vector<double>.Build.Sparse(0, index => 42));
        }

        [Test]
        public void DenseVectorStorageBuilderMethods_ZeroLength_DoNotThrowException()
        {
            Assert.DoesNotThrow(() => new DenseVectorStorage<double>(0));
            Assert.DoesNotThrow(() => new DenseVectorStorage<double>(0, Array.Empty<double>()));
            Assert.DoesNotThrow(() => DenseVectorStorage<double>.OfValue(0, 42));
            Assert.DoesNotThrow(() => DenseVectorStorage<double>.OfInit(0, index => 42));
        }

        [Test]
        public void SparseVectorStorageBuilderMethods_ZeroLength_DoNotThrowException()
        {
            Assert.DoesNotThrow(() => new SparseVectorStorage<double>(0));
            Assert.DoesNotThrow(() => SparseVectorStorage<double>.OfValue(0, 42));
            Assert.DoesNotThrow(() => SparseVectorStorage<double>.OfInit(0, index => 42));
        }

        [Test]
        public void DenseVectorStorageOfInit_NegativeLength_ThrowsArgumentException()
        {
            Assert.That(() => DenseVectorStorage<double>.OfInit(-1, index => 42),
                Throws.TypeOf<ArgumentOutOfRangeException>()
                .With.Message.Contains("Value must not be negative (zero is ok)."));
        }

        [Test]
        public void DenseVectorStorageOfValue_NegativeLength_ThrowsArgumentException()
        {
            Assert.That(() => DenseVectorStorage<double>.OfValue(-1, 42),
                Throws.TypeOf<ArgumentOutOfRangeException>()
                .With.Message.Contains("Value must not be negative (zero is ok)."));
        }

        [Test]
        public void SparseVectorStorageOfInit_NegativeLength_ThrowsArgumentException()
        {
            Assert.That(() => SparseVectorStorage<double>.OfInit(-1, index => 42),
                Throws.TypeOf<ArgumentOutOfRangeException>()
                .With.Message.Contains("Value must not be negative (zero is ok)."));
        }

        [Test]
        public void SparseVectorStorageOfValue_NegativeLength_ThrowsArgumentException()
        {
            Assert.That(() => SparseVectorStorage<double>.OfValue(-1, 42),
                Throws.TypeOf<ArgumentOutOfRangeException>()
                .With.Message.Contains("Value must not be negative (zero is ok)."));
        }
    }
}
