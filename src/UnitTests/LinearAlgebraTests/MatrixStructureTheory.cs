namespace MathNet.Numerics.UnitTests.LinearAlgebraTests
{
    using System;
    using LinearAlgebra.Generic;
    using NUnit.Framework;

    [TestFixture]
    public abstract class MatrixStructureTheory<T>
        where T : struct, IEquatable<T>, IFormattable
    {
        protected abstract Matrix<T> CreateDense(int rows, int columns);
        protected abstract Matrix<T> CreateSparse(int rows, int columns);
        protected abstract Vector<T> CreateVector(int size);

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
            Assert.IsFalse(matrix.Equals((object)2));
            Assert.IsFalse(((object)matrix).Equals(2));
            Assert.IsFalse(matrix == (object)2);
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

        [Theory, Timeout(200)]
        public void CanGetFieldsByIndex(Matrix<T> matrix)
        {
            Assert.That(() => { var x = matrix[0, 0]; }, Throws.Nothing);
            Assert.That(() => { var x = matrix[0, matrix.ColumnCount - 1]; }, Throws.Nothing);
            Assert.That(() => { var x = matrix[matrix.RowCount - 1, 0]; }, Throws.Nothing);

            Assert.That(() => { var x = matrix[-1, 1]; }, Throws.InstanceOf<ArgumentOutOfRangeException>());
            Assert.That(() => { var x = matrix[1, -1]; }, Throws.InstanceOf<ArgumentOutOfRangeException>());
            Assert.That(() => { var x = matrix[0, matrix.ColumnCount]; }, Throws.InstanceOf<ArgumentOutOfRangeException>());
        }

        [Theory, Timeout(200)]
        public void CanGetRow(Matrix<T> matrix)
        {
            // First Row
            var firstrow = matrix.Row(0);
            Assert.That(firstrow.Count, Is.EqualTo(matrix.ColumnCount));
            for (var j = 0; j < matrix.ColumnCount; j++)
            {
                Assert.AreEqual(matrix[0, j], firstrow[j]);
            }

            // Last Row
            var lastrow = matrix.Row(matrix.RowCount - 1);
            Assert.That(lastrow.Count, Is.EqualTo(matrix.ColumnCount));
            for (var j = 0; j < matrix.ColumnCount; j++)
            {
                Assert.AreEqual(matrix[matrix.RowCount - 1, j], lastrow[j]);
            }

            // Invalid Rows
            Assert.That(() => { matrix.Row(-1); }, Throws.InstanceOf<ArgumentOutOfRangeException>());
            Assert.That(() => { matrix.Row(matrix.RowCount); }, Throws.InstanceOf<ArgumentOutOfRangeException>());
        }

        [Theory, Timeout(200)]
        public void CanGetRowIntoResult(Matrix<T> matrix)
        {
            var row = CreateVector(matrix.ColumnCount);
            matrix.Row(0, row);

            for (var j = 0; j < matrix.ColumnCount; j++)
            {
                Assert.AreEqual(matrix[0, j], row[j]);
            }

            Assert.That(() => matrix.Row(0, null), Throws.InstanceOf<ArgumentNullException>());
            Assert.That(() => matrix.Row(-1, row), Throws.InstanceOf<ArgumentOutOfRangeException>());
            Assert.That(() => matrix.Row(matrix.RowCount, row), Throws.InstanceOf<ArgumentOutOfRangeException>());
        }

        [Theory, Timeout(200)]
        public virtual void CanGetRowWithRange(Matrix<T> matrix)
        {
            // First Row, Columns 0..1
            var firstrow = matrix.Row(0, 0, 2);
            Assert.That(firstrow.Count, Is.EqualTo(2));
            for (var j = 0; j < 2; j++)
            {
                Assert.AreEqual(matrix[0, j], firstrow[j]);
            }

            // Second Row, Full Columns
            var secondrow = matrix.Row(1, 0, matrix.ColumnCount);
            Assert.That(secondrow.Count, Is.EqualTo(matrix.ColumnCount));
            for (var j = 0; j < matrix.ColumnCount; j++)
            {
                Assert.AreEqual(matrix[1, j], secondrow[j]);
            }

            // Last Row, Columns 1
            var lastrow = matrix.Row(matrix.RowCount - 1, 1, 1);
            Assert.That(lastrow.Count, Is.EqualTo(1));
            for (var j = 0; j < 1; j++)
            {
                Assert.AreEqual(matrix[matrix.RowCount - 1, j + 1], lastrow[j]);
            }

            // Invalid Rows
            Assert.That(() => { matrix.Row(-1, 0, 2); }, Throws.InstanceOf<ArgumentOutOfRangeException>());
            Assert.That(() => { matrix.Row(matrix.RowCount, 0, 1); }, Throws.InstanceOf<ArgumentOutOfRangeException>());
            Assert.That(() => { matrix.Row(0, -1, 1); }, Throws.InstanceOf<ArgumentOutOfRangeException>());
            Assert.That(() => { matrix.Row(0, 1, 0); }, Throws.InstanceOf<ArgumentOutOfRangeException>());
            Assert.That(() => { matrix.Row(0, 0, matrix.ColumnCount + 1); }, Throws.InstanceOf<ArgumentOutOfRangeException>());
        }

        [Theory, Timeout(200)]
        public void CanGetRowWithRangeIntoResult(Matrix<T> matrix)
        {
            var row = CreateVector(matrix.ColumnCount - 1);
            matrix.Row(0, 1, matrix.ColumnCount - 1, row);

            for (var j = 0; j < matrix.ColumnCount - 1; j++)
            {
                Assert.AreEqual(matrix[0, j + 1], row[j]);
            }

            Assert.That(() => matrix.Row(0, 0, matrix.ColumnCount - 1, null), Throws.InstanceOf<ArgumentNullException>());
            Assert.That(() => matrix.Row(-1, 0, matrix.ColumnCount - 1, row), Throws.InstanceOf<ArgumentOutOfRangeException>());
            Assert.That(() => matrix.Row(matrix.RowCount, 0, matrix.ColumnCount - 1, row), Throws.InstanceOf<ArgumentOutOfRangeException>());
            Assert.That(() => matrix.Row(0, 0, matrix.ColumnCount, row), Throws.InstanceOf<ArgumentOutOfRangeException>());
        }

    }
}
