using System.Collections.Generic;

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests
{
    using System;
    using LinearAlgebra.Generic;
    using NUnit.Framework;

    [TestFixture]
    public abstract partial class MatrixStructureTheory<T>
        where T : struct, IEquatable<T>, IFormattable
    {
        protected abstract Matrix<T> CreateDenseZero(int rows, int columns);
        protected abstract Matrix<T> CreateDenseRandom(int rows, int columns, int seed);
        protected abstract Matrix<T> CreateSparseZero(int rows, int columns);
        protected abstract Vector<T> CreateVectorZero(int size);
        protected abstract Vector<T> CreateVectorRandom(int size, int seed);
        protected abstract T Zero { get; }

        protected Matrix<T> CreateDenseFor(Matrix<T> m, int rows = -1, int columns = -1, int seed = 1)
        {
            return m.Storage.IsFullyMutable
                ? CreateDenseRandom(rows >= 0 ? rows : m.RowCount, columns >= 0 ? columns : m.ColumnCount, seed)
                : CreateDenseZero(rows >= 0 ? rows : m.RowCount, columns >= 0 ? columns : m.ColumnCount);
        }

        protected Vector<T> CreateVectorFor(Matrix<T> m, int size, int seed = 1)
        {
            return m.Storage.IsFullyMutable
                ? CreateVectorRandom(size, seed)
                : CreateVectorZero(size);
        }

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
            var dense = CreateDenseZero(matrix.RowCount, matrix.ColumnCount);
            matrix.CopyTo(dense);
            Assert.That(dense, Is.EqualTo(matrix));

            var sparse = CreateSparseZero(matrix.RowCount, matrix.ColumnCount);
            matrix.CopyTo(sparse);
            Assert.That(sparse, Is.EqualTo(matrix));

            // null arg
            Assert.That(() => matrix.CopyTo(null), Throws.InstanceOf<ArgumentNullException>());

            // bad arg
            Assert.That(() => matrix.CopyTo(CreateDenseZero(matrix.RowCount + 1, matrix.ColumnCount)), Throws.ArgumentException);
            Assert.That(() => matrix.CopyTo(CreateDenseZero(matrix.RowCount, matrix.ColumnCount + 1)), Throws.ArgumentException);
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
        public void CanClearSubMatrix(Matrix<T> matrix)
        {
            var cleared = matrix.Clone();
            Assume.That(cleared.RowCount, Is.GreaterThanOrEqualTo(2));
            Assume.That(cleared.ColumnCount, Is.GreaterThanOrEqualTo(2));

            cleared.Storage.Clear(0,2,1,1);
            Assert.That(cleared.At(0, 0), Is.EqualTo(matrix.At(0, 0)));
            Assert.That(cleared.At(1, 0), Is.EqualTo(matrix.At(1, 0)));
            Assert.That(cleared.At(0, 1), Is.EqualTo(Zero));
            Assert.That(cleared.At(1, 1), Is.EqualTo(Zero));
        }

        [Theory, Timeout(200)]
        public void CanToArray(Matrix<T> matrix)
        {
            var array = matrix.ToArray();
            Assert.That(array.GetLength(0), Is.EqualTo(matrix.RowCount));
            Assert.That(array.GetLength(1), Is.EqualTo(matrix.ColumnCount));
            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    Assert.That(array[i, j], Is.EqualTo(matrix[i, j]));
                }
            }
        }

        [Theory, Timeout(200)]
        public void CanToColumnWiseArray(Matrix<T> matrix)
        {
            var array = matrix.ToColumnWiseArray();
            Assert.That(array.Length, Is.EqualTo(matrix.RowCount * matrix.ColumnCount));
            for (int i = 0; i < array.Length; i++)
            {
                Assert.That(array[i], Is.EqualTo(matrix[i % matrix.RowCount, i / matrix.RowCount]));
            }
        }

        [Theory, Timeout(200)]
        public void CanToRowWiseArray(Matrix<T> matrix)
        {
            var array = matrix.ToRowWiseArray();
            Assert.That(array.Length, Is.EqualTo(matrix.RowCount * matrix.ColumnCount));
            for (int i = 0; i < array.Length; i++)
            {
                Assert.That(array[i], Is.EqualTo(matrix[i / matrix.ColumnCount, i % matrix.ColumnCount]));
            }
        }

        [Theory, Timeout(200)]
        public void CanCreateSameType(Matrix<T> matrix)
        {
            var empty = matrix.CreateMatrix(5, 6);
            Assert.That(empty, Is.EqualTo(CreateDenseZero(5, 6)));
            Assert.That(empty.GetType(), Is.EqualTo(matrix.GetType()));

            Assert.That(() => matrix.CreateMatrix(0, 2), Throws.InstanceOf<ArgumentOutOfRangeException>());
            Assert.That(() => matrix.CreateMatrix(2, 0), Throws.InstanceOf<ArgumentOutOfRangeException>());
            Assert.That(() => matrix.CreateMatrix(-1, -1), Throws.InstanceOf<ArgumentOutOfRangeException>());
        }

        [Test, Timeout(200)]
        public void CanCreateFromColumns()
        {
            var column1 = CreateVectorRandom(1, 0);
            var column2 = CreateVectorRandom(4, 1);
            var column3 = CreateVectorRandom(2, 3);

            var matrix = Matrix<T>.CreateFromColumns(new List<Vector<T>>
                {
                    column1,
                    column2,
                    column3
                });

            Assert.That(matrix.RowCount, Is.EqualTo(4));
            Assert.That(matrix.ColumnCount, Is.EqualTo(3));

            Assert.That(matrix[0, 0], Is.EqualTo(column1[0]));
            Assert.That(matrix[0, 1], Is.EqualTo(column2[0]));
            Assert.That(matrix[1, 1], Is.EqualTo(column2[1]));
            Assert.That(matrix[2, 1], Is.EqualTo(column2[2]));
            Assert.That(matrix[3, 1], Is.EqualTo(column2[3]));
            Assert.That(matrix[0, 2], Is.EqualTo(column3[0]));
            Assert.That(matrix[1, 2], Is.EqualTo(column3[1]));

            Assert.That(matrix[1, 0], Is.EqualTo(Zero));
            Assert.That(matrix[2, 0], Is.EqualTo(Zero));
            Assert.That(matrix[3, 0], Is.EqualTo(Zero));
            Assert.That(matrix[2, 2], Is.EqualTo(Zero));
            Assert.That(matrix[3, 2], Is.EqualTo(Zero));
        }

        [Test, Timeout(200)]
        public void CanCreateFromRows()
        {
            var row1 = CreateVectorRandom(1, 0);
            var row2 = CreateVectorRandom(4, 1);
            var row3 = CreateVectorRandom(2, 3);

            var matrix = Matrix<T>.CreateFromRows(new List<Vector<T>>
                {
                    row1,
                    row2,
                    row3
                });

            Assert.That(matrix.RowCount, Is.EqualTo(3));
            Assert.That(matrix.ColumnCount, Is.EqualTo(4));

            Assert.That(matrix[0, 0], Is.EqualTo(row1[0]));
            Assert.That(matrix[1, 0], Is.EqualTo(row2[0]));
            Assert.That(matrix[1, 1], Is.EqualTo(row2[1]));
            Assert.That(matrix[1, 2], Is.EqualTo(row2[2]));
            Assert.That(matrix[1, 3], Is.EqualTo(row2[3]));
            Assert.That(matrix[2, 0], Is.EqualTo(row3[0]));
            Assert.That(matrix[2, 1], Is.EqualTo(row3[1]));

            Assert.That(matrix[0, 1], Is.EqualTo(Zero));
            Assert.That(matrix[0, 2], Is.EqualTo(Zero));
            Assert.That(matrix[0, 3], Is.EqualTo(Zero));
            Assert.That(matrix[2, 2], Is.EqualTo(Zero));
            Assert.That(matrix[2, 3], Is.EqualTo(Zero));
        }

        [Test, Timeout(200)]
        public void CanEnumerateWithIndex()
        {
            var dense = CreateDenseRandom(2, 3, 0);
            using(var enumerator = dense.IndexedEnumerator().GetEnumerator())
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    enumerator.MoveNext();
                    Assert.AreEqual(i, enumerator.Current.Item1);
                    Assert.AreEqual(j, enumerator.Current.Item2);
                    Assert.AreEqual(dense[i, j], enumerator.Current.Item3);
                }
            }
        }
    }
}
