// <copyright file="VectorStorageSerializationTests.cs" company="Math.NET">
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
using System.IO;
using System.Runtime.Serialization;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Storage;
using NUnit.Framework;

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests
{
#if NOSYSNUMERICS
    using Complex64 = Numerics.Complex;
#else
    using Complex64 = System.Numerics.Complex;
#endif

    [TestFixture]
    public class VectorStorageSerializationTests
    {
        static readonly Type[] KnownTypes =
        {
            typeof(DenseVectorStorage<double>), typeof(SparseVectorStorage<double>),
            typeof(DenseVectorStorage<float>), typeof(SparseVectorStorage<float>),
            typeof(DenseVectorStorage<Numerics.Complex32>), typeof(SparseVectorStorage<Numerics.Complex32>),
            typeof(DenseVectorStorage<Complex64>), typeof(SparseVectorStorage<Complex64>)
        };

        [Test]
        public void DenseVectorStorageOfDoubleDataContractSerializationTest()
        {
            VectorStorage<double> expected = Vector<double>.Build.DenseOfArray(new[] { 1.0d, 2.0d, 0.0d, 3.0d }).Storage;

            var serializer = new DataContractSerializer(typeof(VectorStorage<double>), KnownTypes);
            var stream = new MemoryStream();
            serializer.WriteObject(stream, expected);
            stream.Position = 0;

            var actual = (VectorStorage<double>)serializer.ReadObject(stream);

            Assert.That(actual.Length, Is.EqualTo(expected.Length));
            Assert.That(actual, Is.TypeOf(typeof(DenseVectorStorage<double>)));
            Assert.That(actual, Is.EqualTo(expected));
            Assert.That(actual, Is.Not.SameAs(expected));
        }

        [Test]
        public void DenseVectorStorageOfSingleDataContractSerializationTest()
        {
            VectorStorage<float> expected = Vector<float>.Build.DenseOfArray(new[] { 1.0f, 2.0f, 0.0f, 3.0f }).Storage;

            var serializer = new DataContractSerializer(typeof(VectorStorage<float>), KnownTypes);
            var stream = new MemoryStream();
            serializer.WriteObject(stream, expected);
            stream.Position = 0;

            var actual = (VectorStorage<float>)serializer.ReadObject(stream);

            Assert.That(actual.Length, Is.EqualTo(expected.Length));
            Assert.That(actual, Is.TypeOf(typeof(DenseVectorStorage<float>)));
            Assert.That(actual, Is.EqualTo(expected));
            Assert.That(actual, Is.Not.SameAs(expected));
        }

        [Test]
        public void DenseVectorStorageOfComplex64DataContractSerializationTest()
        {
            VectorStorage<Complex64> expected = Vector<Complex64>.Build.DenseOfArray(new[] { new Complex64(1.0, -1.0), 2.0, 0.0, 3.0 }).Storage;

            var serializer = new DataContractSerializer(typeof(VectorStorage<Complex64>), KnownTypes);
            var stream = new MemoryStream();
            serializer.WriteObject(stream, expected);
            stream.Position = 0;

            var actual = (VectorStorage<Complex64>)serializer.ReadObject(stream);

            Assert.That(actual.Length, Is.EqualTo(expected.Length));
            Assert.That(actual, Is.TypeOf(typeof(DenseVectorStorage<Complex64>)));
            Assert.That(actual, Is.EqualTo(expected));
            Assert.That(actual, Is.Not.SameAs(expected));
        }

        [Test]
        public void DenseVectorStorageOfComplex32DataContractSerializationTest()
        {
            VectorStorage<Numerics.Complex32> expected = Vector<Numerics.Complex32>.Build.DenseOfArray(new[] { new Numerics.Complex32(1.0f, -1.0f), 2.0f, 0.0f, 3.0f }).Storage;

            var serializer = new DataContractSerializer(typeof(VectorStorage<Numerics.Complex32>), KnownTypes);
            var stream = new MemoryStream();
            serializer.WriteObject(stream, expected);
            stream.Position = 0;

            var actual = (VectorStorage<Numerics.Complex32>)serializer.ReadObject(stream);

            Assert.That(actual.Length, Is.EqualTo(expected.Length));
            Assert.That(actual, Is.TypeOf(typeof(DenseVectorStorage<Numerics.Complex32>)));
            Assert.That(actual, Is.EqualTo(expected));
            Assert.That(actual, Is.Not.SameAs(expected));
        }

        [Test]
        public void SparseVectorStorageOfDoubleDataContractSerializationTest()
        {
            VectorStorage<double> expected = Vector<double>.Build.SparseOfArray(new[] { 1.0d, 2.0d, 0.0d, 3.0d }).Storage;

            var serializer = new DataContractSerializer(typeof(VectorStorage<double>), KnownTypes);
            var stream = new MemoryStream();
            serializer.WriteObject(stream, expected);
            stream.Position = 0;

            var actual = (VectorStorage<double>)serializer.ReadObject(stream);

            Assert.That(actual.Length, Is.EqualTo(expected.Length));
            Assert.That(actual, Is.TypeOf(typeof(SparseVectorStorage<double>)));
            Assert.That(actual, Is.EqualTo(expected));
            Assert.That(actual, Is.Not.SameAs(expected));
        }
    }
}
