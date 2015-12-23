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
    public class MatrixStorageSerializationTests
    {
        static readonly Type[] KnownTypes =
        {
            typeof(DenseColumnMajorMatrixStorage<double>), typeof(SparseCompressedRowMatrixStorage<double>), typeof(DiagonalMatrixStorage<double>),
            typeof(DenseColumnMajorMatrixStorage<float>), typeof(SparseCompressedRowMatrixStorage<float>), typeof(DiagonalMatrixStorage<float>),
            typeof(DenseColumnMajorMatrixStorage<Numerics.Complex32>), typeof(SparseCompressedRowMatrixStorage<Numerics.Complex32>), typeof(DiagonalMatrixStorage<Numerics.Complex32>),
            typeof(DenseColumnMajorMatrixStorage<Complex64>), typeof(SparseCompressedRowMatrixStorage<Complex64>), typeof(DiagonalMatrixStorage<Complex64>)
        };

        [Test]
        public void DenseColumnMajorMatrixStorageOfDoubleDataContractSerializationTest()
        {
            MatrixStorage<double> expected = Matrix<double>.Build.Dense(2, 2, new[] { 1.0d, 2.0d, 0.0d, 3.0d }).Storage;

            var serializer = new DataContractSerializer(typeof(MatrixStorage<double>), KnownTypes);
            var stream = new MemoryStream();
            serializer.WriteObject(stream, expected);
            stream.Position = 0;

            var actual = (MatrixStorage<double>)serializer.ReadObject(stream);

            Assert.That(actual.RowCount, Is.EqualTo(expected.RowCount));
            Assert.That(actual.ColumnCount, Is.EqualTo(expected.ColumnCount));
            Assert.That(actual, Is.TypeOf(typeof(DenseColumnMajorMatrixStorage<double>)));
            Assert.That(actual, Is.EqualTo(expected));
            Assert.That(actual, Is.Not.SameAs(expected));
        }

        [Test]
        public void DenseColumnMajorMatrixStorageOfSingleDataContractSerializationTest()
        {
            MatrixStorage<float> expected = Matrix<float>.Build.Dense(2, 2, new[] { 1.0f, 2.0f, 0.0f, 3.0f }).Storage;

            var serializer = new DataContractSerializer(typeof(MatrixStorage<float>), KnownTypes);
            var stream = new MemoryStream();
            serializer.WriteObject(stream, expected);
            stream.Position = 0;

            var actual = (MatrixStorage<float>)serializer.ReadObject(stream);

            Assert.That(actual.RowCount, Is.EqualTo(expected.RowCount));
            Assert.That(actual.ColumnCount, Is.EqualTo(expected.ColumnCount));
            Assert.That(actual, Is.TypeOf(typeof(DenseColumnMajorMatrixStorage<float>)));
            Assert.That(actual, Is.EqualTo(expected));
            Assert.That(actual, Is.Not.SameAs(expected));
        }

        [Test]
        public void DenseColumnMajorMatrixStorageOfComplex64DataContractSerializationTest()
        {
            MatrixStorage<Complex64> expected = Matrix<Complex64>.Build.Dense(2, 2, new[] { new Complex64(1.0, -1.0), 2.0, 0.0, 3.0 }).Storage;

            var serializer = new DataContractSerializer(typeof(MatrixStorage<Complex64>), KnownTypes);
            var stream = new MemoryStream();
            serializer.WriteObject(stream, expected);
            stream.Position = 0;

            var actual = (MatrixStorage<Complex64>)serializer.ReadObject(stream);

            Assert.That(actual.RowCount, Is.EqualTo(expected.RowCount));
            Assert.That(actual.ColumnCount, Is.EqualTo(expected.ColumnCount));
            Assert.That(actual, Is.TypeOf(typeof(DenseColumnMajorMatrixStorage<Complex64>)));
            Assert.That(actual, Is.EqualTo(expected));
            Assert.That(actual, Is.Not.SameAs(expected));
        }

        [Test]
        public void DenseColumnMajorMatrixStorageOfComplex32DataContractSerializationTest()
        {
            MatrixStorage<Numerics.Complex32> expected = Matrix<Numerics.Complex32>.Build.Dense(2, 2, new[] { new Numerics.Complex32(1.0f, -1.0f), 2.0f, 0.0f, 3.0f }).Storage;

            var serializer = new DataContractSerializer(typeof(MatrixStorage<Numerics.Complex32>), KnownTypes);
            var stream = new MemoryStream();
            serializer.WriteObject(stream, expected);
            stream.Position = 0;

            var actual = (MatrixStorage<Numerics.Complex32>)serializer.ReadObject(stream);

            Assert.That(actual.RowCount, Is.EqualTo(expected.RowCount));
            Assert.That(actual.ColumnCount, Is.EqualTo(expected.ColumnCount));
            Assert.That(actual, Is.TypeOf(typeof(DenseColumnMajorMatrixStorage<Numerics.Complex32>)));
            Assert.That(actual, Is.EqualTo(expected));
            Assert.That(actual, Is.Not.SameAs(expected));
        }

        [Test]
        public void SparseCompressedRowMatrixStorageOfDoubleDataContractSerializationTest()
        {
            MatrixStorage<double> expected = Matrix<double>.Build.SparseOfArray(new[,] { {1.0d, 2.0d}, {0.0d, 3.0d} }).Storage;

            var serializer = new DataContractSerializer(typeof(MatrixStorage<double>), KnownTypes);
            var stream = new MemoryStream();
            serializer.WriteObject(stream, expected);
            stream.Position = 0;

            var actual = (MatrixStorage<double>)serializer.ReadObject(stream);

            Assert.That(actual.RowCount, Is.EqualTo(expected.RowCount));
            Assert.That(actual.ColumnCount, Is.EqualTo(expected.ColumnCount));
            Assert.That(actual, Is.TypeOf(typeof(SparseCompressedRowMatrixStorage<double>)));
            Assert.That(actual, Is.EqualTo(expected));
            Assert.That(actual, Is.Not.SameAs(expected));
        }

        [Test]
        public void DiagonalMatrixStorageOfDoubleDataContractSerializationTest()
        {
            MatrixStorage<double> expected = Matrix<double>.Build.Diagonal(new[] { 1.0d, 2.0d, 0.0d, 3.0d }).Storage;

            var serializer = new DataContractSerializer(typeof(MatrixStorage<double>), KnownTypes);
            var stream = new MemoryStream();
            serializer.WriteObject(stream, expected);
            stream.Position = 0;

            var actual = (MatrixStorage<double>)serializer.ReadObject(stream);

            Assert.That(actual.RowCount, Is.EqualTo(expected.RowCount));
            Assert.That(actual.ColumnCount, Is.EqualTo(expected.ColumnCount));
            Assert.That(actual, Is.TypeOf(typeof(DiagonalMatrixStorage<double>)));
            Assert.That(actual, Is.EqualTo(expected));
            Assert.That(actual, Is.Not.SameAs(expected));
        }
    }
}
