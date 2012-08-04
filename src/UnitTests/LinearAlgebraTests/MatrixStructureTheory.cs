namespace MathNet.Numerics.UnitTests.LinearAlgebraTests
{
    using System;
    using LinearAlgebra.Generic;
    using NUnit.Framework;

    [TestFixture]
    public abstract partial class MatrixStructureTheory<T>
        where T : struct, IEquatable<T>, IFormattable
    {
        protected abstract Matrix<T> CreateDense(int rows, int columns);
        protected abstract Matrix<T> CreateDense(int rows, int columns, int seed);
        protected abstract Matrix<T> CreateSparse(int rows, int columns);
        protected abstract Vector<T> CreateVector(int size);
        protected abstract Vector<T> CreateVector(int size, int seed);
        protected abstract T Zero { get; }

        [Theory, Timeout(200)]
        public void IsEqualToItself(Matrix<T> matrix)
        {
            Assert.That(matrix, Is.EqualTo(matrix));
            Assert.IsTrue(matrix.Equals(matrix));
            Assert.IsTrue(matrix.Equals((object) matrix));
            Assert.IsTrue(((object) matrix).Equals(matrix));
            Assert.IsTrue(matrix == (object) matrix);
            Assert.IsTrue((object) matrix == matrix);
        }

        [Theory, Timeout(200)]
        public void IsNotEqualToOthers(Matrix<T> left, Matrix<T> right)
        {
            // IF (assuming we don't have duplicate data points)
            Assume.That(left, Is.Not.SameAs(right));

            // THEN
            Assert.That(left, Is.Not.EqualTo(right));
            Assert.IsFalse(left.Equals(right));
            Assert.IsFalse(left.Equals((object) right));
            Assert.IsFalse(((object) left).Equals(right));
            Assert.IsFalse(left == (object) right);
            Assert.IsFalse((object) left == right);
        }

        [Theory, Timeout(200)]
        public void IsNotEqualToNonMatrixType(Matrix<T> matrix)
        {
            Assert.That(matrix, Is.Not.EqualTo(2));
            Assert.IsFalse(matrix.Equals(2));
            Assert.IsFalse(matrix.Equals((object) 2));
            Assert.IsFalse(((object) matrix).Equals(2));
            Assert.IsFalse(matrix == (object) 2);
        }

        [Theory, Timeout(200)]
        public void CanClone(Matrix<T> matrix)
        {
            var clone = matrix.Clone();
            Assert.That(clone, Is.Not.SameAs(matrix));
            Assert.That(clone, Is.EqualTo(matrix));
            Assert.That(clone.RowCount, Is.EqualTo(matrix.RowCount));
            Assert.That(clone.ColumnCount, Is.EqualTo(matrix.ColumnCount));
        }

        [Theory, Timeout(200)]
        public void CanCloneUsingICloneable(Matrix<T> matrix)
        {
            var clone = (Matrix<T>) ((ICloneable) matrix).Clone();
            Assert.That(clone, Is.Not.SameAs(matrix));
            Assert.That(clone, Is.EqualTo(matrix));
            Assert.That(clone.RowCount, Is.EqualTo(matrix.RowCount));
            Assert.That(clone.ColumnCount, Is.EqualTo(matrix.ColumnCount));
        }

        [Theory, Timeout(200)]
        public void CanCopyTo(Matrix<T> matrix)
        {
            var dense = CreateDense(matrix.RowCount, matrix.ColumnCount);
            matrix.CopyTo(dense);
            Assert.That(dense, Is.EqualTo(matrix));

            var sparse = CreateSparse(matrix.RowCount, matrix.ColumnCount);
            matrix.CopyTo(sparse);
            Assert.That(sparse, Is.EqualTo(matrix));

            // null arg
            Assert.That(() => matrix.CopyTo(null), Throws.InstanceOf<ArgumentNullException>());

            // bad arg
            Assert.That(() => matrix.CopyTo(CreateDense(matrix.RowCount + 1, matrix.ColumnCount)), Throws.ArgumentException);
            Assert.That(() => matrix.CopyTo(CreateDense(matrix.RowCount, matrix.ColumnCount + 1)), Throws.ArgumentException);
        }

        [Theory, Timeout(200)]
        public void CanCreateSameType(Matrix<T> matrix)
        {
            var empty = matrix.CreateMatrix(5, 6);
            Assert.That(empty, Is.EqualTo(CreateDense(5, 6)));
            Assert.That(empty.GetType(), Is.EqualTo(matrix.GetType()));

            Assert.That(() => matrix.CreateMatrix(0, 2), Throws.InstanceOf<ArgumentOutOfRangeException>());
            Assert.That(() => matrix.CreateMatrix(2, 0), Throws.InstanceOf<ArgumentOutOfRangeException>());
            Assert.That(() => matrix.CreateMatrix(-1, -1), Throws.InstanceOf<ArgumentOutOfRangeException>());
        }

        [Theory, Timeout(200)]
        public void CanGetHashCode(Matrix<T> matrix)
        {
            Assert.That(matrix.GetHashCode(), Is.Not.EqualTo(matrix.CreateMatrix(matrix.RowCount, matrix.ColumnCount).GetHashCode()));
        }

        [Theory, Timeout(200)]
        public void CanClear(Matrix<T> matrix)
        {
            var cleared = matrix.Clone();
            cleared.Clear();
            Assert.That(cleared, Is.EqualTo(matrix.CreateMatrix(matrix.RowCount, matrix.ColumnCount)));
        }
    }
}
