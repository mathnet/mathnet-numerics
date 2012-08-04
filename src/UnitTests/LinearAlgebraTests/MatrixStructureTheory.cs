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

        [Theory, Timeout(200)]
        public void CanGetFieldsByIndex(Matrix<T> matrix)
        {
            Assert.That(() => matrix[0, 0], Throws.Nothing);
            Assert.That(() => matrix[0, matrix.ColumnCount - 1], Throws.Nothing);
            Assert.That(() => matrix[matrix.RowCount - 1, 0], Throws.Nothing);

            Assert.That(() => matrix[-1, 1], Throws.InstanceOf<ArgumentOutOfRangeException>());
            Assert.That(() => matrix[1, -1], Throws.InstanceOf<ArgumentOutOfRangeException>());
            Assert.That(() => matrix[0, matrix.ColumnCount], Throws.InstanceOf<ArgumentOutOfRangeException>());
        }

        #region Row & Column Access

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
            Assert.That(() => matrix.Row(-1), Throws.InstanceOf<ArgumentOutOfRangeException>());
            Assert.That(() => matrix.Row(matrix.RowCount), Throws.InstanceOf<ArgumentOutOfRangeException>());
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
            Assert.That(() => matrix.Row(-1, 0, 2), Throws.InstanceOf<ArgumentOutOfRangeException>());
            Assert.That(() => matrix.Row(matrix.RowCount, 0, 1), Throws.InstanceOf<ArgumentOutOfRangeException>());
            Assert.That(() => matrix.Row(0, -1, 1), Throws.InstanceOf<ArgumentOutOfRangeException>());
            Assert.That(() => matrix.Row(0, 1, 0), Throws.InstanceOf<ArgumentOutOfRangeException>());
            Assert.That(() => matrix.Row(0, 0, matrix.ColumnCount + 1), Throws.InstanceOf<ArgumentOutOfRangeException>());
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

        [Theory, Timeout(200)]
        public void CanGetColumn(Matrix<T> matrix)
        {
            // First Column
            var firstcol = matrix.Column(0);
            Assert.That(firstcol.Count, Is.EqualTo(matrix.RowCount));
            for (var i = 0; i < matrix.RowCount; i++)
            {
                Assert.AreEqual(matrix[i, 0], firstcol[i]);
            }

            // Last Column
            var lastcol = matrix.Column(matrix.ColumnCount - 1);
            Assert.That(lastcol.Count, Is.EqualTo(matrix.RowCount));
            for (var i = 0; i < matrix.RowCount; i++)
            {
                Assert.AreEqual(matrix[i, matrix.ColumnCount - 1], lastcol[i]);
            }

            // Invalid Columns
            Assert.That(() => matrix.Column(-1), Throws.InstanceOf<ArgumentOutOfRangeException>());
            Assert.That(() => matrix.Column(matrix.ColumnCount), Throws.InstanceOf<ArgumentOutOfRangeException>());
        }

        [Theory, Timeout(200)]
        public void CanGetColumnIntoResult(Matrix<T> matrix)
        {
            var col = CreateVector(matrix.RowCount);
            matrix.Column(0, col);

            for (var i = 0; i < matrix.RowCount; i++)
            {
                Assert.AreEqual(matrix[i, 0], col[i]);
            }

            Assert.That(() => matrix.Column(0, null), Throws.InstanceOf<ArgumentNullException>());
            Assert.That(() => matrix.Column(-1, col), Throws.InstanceOf<ArgumentOutOfRangeException>());
            Assert.That(() => matrix.Column(matrix.ColumnCount, col), Throws.InstanceOf<ArgumentOutOfRangeException>());
        }

        [Theory, Timeout(200)]
        public virtual void CanGetColumnWithRange(Matrix<T> matrix)
        {
            // First Column, Rows 0..1
            var firstcol = matrix.Column(0, 0, 2);
            Assert.That(firstcol.Count, Is.EqualTo(2));
            for (var i = 0; i < 2; i++)
            {
                Assert.AreEqual(matrix[i, 0], firstcol[i]);
            }

            // Second Column, Full Rows
            var secondcol = matrix.Column(1, 0, matrix.RowCount);
            Assert.That(secondcol.Count, Is.EqualTo(matrix.RowCount));
            for (var i = 0; i < matrix.RowCount; i++)
            {
                Assert.AreEqual(matrix[i, 1], secondcol[i]);
            }

            // Last Column, Rows 1
            var lastcol = matrix.Column(matrix.ColumnCount - 1, 1, 1);
            Assert.That(lastcol.Count, Is.EqualTo(1));
            for (var i = 0; i < 1; i++)
            {
                Assert.AreEqual(matrix[i + 1, matrix.ColumnCount - 1], lastcol[i]);
            }

            // Invalid Rows
            Assert.That(() => matrix.Column(-1, 0, 2), Throws.InstanceOf<ArgumentOutOfRangeException>());
            Assert.That(() => matrix.Column(matrix.ColumnCount, 0, 1), Throws.InstanceOf<ArgumentOutOfRangeException>());
            Assert.That(() => matrix.Column(0, -1, 1), Throws.InstanceOf<ArgumentOutOfRangeException>());
            Assert.That(() => matrix.Column(0, 1, 0), Throws.InstanceOf<ArgumentOutOfRangeException>());
            Assert.That(() => matrix.Column(0, 0, matrix.RowCount + 1), Throws.InstanceOf<ArgumentOutOfRangeException>());
        }

        [Theory, Timeout(200)]
        public void CanGetColumnWithRangeIntoResult(Matrix<T> matrix)
        {
            var col = CreateVector(matrix.RowCount - 1);
            matrix.Column(0, 1, matrix.RowCount - 1, col);

            for (var i = 0; i < matrix.RowCount - 1; i++)
            {
                Assert.AreEqual(matrix[i + 1, 0], col[i]);
            }

            Assert.That(() => matrix.Column(0, 0, matrix.RowCount - 1, null), Throws.InstanceOf<ArgumentNullException>());
            Assert.That(() => matrix.Column(-1, 0, matrix.RowCount - 1, col), Throws.InstanceOf<ArgumentOutOfRangeException>());
            Assert.That(() => matrix.Column(matrix.ColumnCount, 0, matrix.ColumnCount - 1, col), Throws.InstanceOf<ArgumentOutOfRangeException>());
            Assert.That(() => matrix.Column(0, 0, matrix.RowCount, col), Throws.InstanceOf<ArgumentOutOfRangeException>());
        }

        [Theory, Timeout(200)]
        public void CanSetRow(Matrix<T> matrix)
        {
            // First Row
            var m = matrix.Clone();
            m.SetRow(0, CreateVector(matrix.ColumnCount));
            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    Assert.That(m[i, j], Is.EqualTo(i == 0 ? Zero : matrix[i, j]));
                }
            }

            // Last Row
            m = matrix.Clone();
            m.SetRow(matrix.RowCount - 1, CreateVector(matrix.ColumnCount));
            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    Assert.That(m[i, j], Is.EqualTo(i == matrix.RowCount - 1 ? Zero : matrix[i, j]));
                }
            }

            // Invalid Rows
            Assert.That(() => matrix.SetRow(0, default(Vector<T>)), Throws.InstanceOf<ArgumentNullException>());
            Assert.That(() => matrix.SetRow(-1, CreateVector(matrix.ColumnCount)), Throws.InstanceOf<ArgumentOutOfRangeException>());
            Assert.That(() => matrix.SetRow(matrix.RowCount, CreateVector(matrix.ColumnCount)), Throws.InstanceOf<ArgumentOutOfRangeException>());
            Assert.That(() => matrix.SetRow(0, CreateVector(matrix.ColumnCount - 1)), Throws.ArgumentException);
            Assert.That(() => matrix.SetRow(0, CreateVector(matrix.ColumnCount + 1)), Throws.ArgumentException);
        }

        [Theory, Timeout(200)]
        public void CanSetRowArray(Matrix<T> matrix)
        {
            // First Row
            var m = matrix.Clone();
            m.SetRow(0, CreateVector(matrix.ColumnCount).ToArray());
            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    Assert.That(m[i, j], Is.EqualTo(i == 0 ? Zero : matrix[i, j]));
                }
            }

            // Last Row
            m = matrix.Clone();
            m.SetRow(matrix.RowCount - 1, new T[matrix.ColumnCount]);
            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    Assert.That(m[i, j], Is.EqualTo(i == matrix.RowCount - 1 ? Zero : matrix[i, j]));
                }
            }

            // Invalid Rows
            Assert.That(() => matrix.SetRow(0, default(T[])), Throws.InstanceOf<ArgumentNullException>());
            Assert.That(() => matrix.SetRow(-1, new T[matrix.ColumnCount]), Throws.InstanceOf<ArgumentOutOfRangeException>());
            Assert.That(() => matrix.SetRow(matrix.RowCount, new T[matrix.ColumnCount]), Throws.InstanceOf<ArgumentOutOfRangeException>());
            Assert.That(() => matrix.SetRow(0, new T[matrix.ColumnCount - 1]), Throws.ArgumentException);
            Assert.That(() => matrix.SetRow(0, new T[matrix.ColumnCount + 1]), Throws.ArgumentException);
        }

        [Theory, Timeout(200)]
        public void CanSetColumn(Matrix<T> matrix)
        {
            // First Column
            var m = matrix.Clone();
            m.SetColumn(0, CreateVector(matrix.RowCount));
            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    Assert.That(m[i, j], Is.EqualTo(j == 0 ? Zero : matrix[i, j]));
                }
            }

            // Last Column
            m = matrix.Clone();
            m.SetColumn(matrix.ColumnCount - 1, CreateVector(matrix.RowCount));
            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    Assert.That(m[i, j], Is.EqualTo(j == matrix.ColumnCount - 1 ? Zero : matrix[i, j]));
                }
            }

            // Invalid Rows
            Assert.That(() => matrix.SetColumn(0, default(Vector<T>)), Throws.InstanceOf<ArgumentNullException>());
            Assert.That(() => matrix.SetColumn(-1, CreateVector(matrix.RowCount)), Throws.InstanceOf<ArgumentOutOfRangeException>());
            Assert.That(() => matrix.SetColumn(matrix.ColumnCount, CreateVector(matrix.RowCount)), Throws.InstanceOf<ArgumentOutOfRangeException>());
            Assert.That(() => matrix.SetColumn(0, CreateVector(matrix.RowCount - 1)), Throws.ArgumentException);
            Assert.That(() => matrix.SetColumn(0, CreateVector(matrix.RowCount + 1)), Throws.ArgumentException);
        }

        [Theory, Timeout(200)]
        public void CanSetColumnArray(Matrix<T> matrix)
        {
            // First Column
            var m = matrix.Clone();
            m.SetColumn(0, CreateVector(matrix.RowCount).ToArray());
            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    Assert.That(m[i, j], Is.EqualTo(j == 0 ? Zero : matrix[i, j]));
                }
            }

            // Last Column
            m = matrix.Clone();
            m.SetColumn(matrix.ColumnCount - 1, new T[matrix.RowCount]);
            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    Assert.That(m[i, j], Is.EqualTo(j == matrix.ColumnCount - 1 ? Zero : matrix[i, j]));
                }
            }

            // Invalid Rows
            Assert.That(() => matrix.SetColumn(0, default(T[])), Throws.InstanceOf<ArgumentNullException>());
            Assert.That(() => matrix.SetColumn(-1, new T[matrix.RowCount]), Throws.InstanceOf<ArgumentOutOfRangeException>());
            Assert.That(() => matrix.SetColumn(matrix.ColumnCount, new T[matrix.RowCount]), Throws.InstanceOf<ArgumentOutOfRangeException>());
            Assert.That(() => matrix.SetColumn(0, new T[matrix.RowCount - 1]), Throws.ArgumentException);
            Assert.That(() => matrix.SetColumn(0, new T[matrix.RowCount + 1]), Throws.ArgumentException);
        }

        #endregion

        #region Triangle Access

        [Theory, Timeout(200)]
        public void CanGetUpperTriangle(Matrix<T> matrix)
        {
            var upper = matrix.UpperTriangle();
            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    Assert.That(upper[i, j], Is.EqualTo(i <= j ? matrix[i, j] : Zero));
                }
            }
        }

        [Theory, Timeout(200)]
        public void CanGetUpperTriangleIntoResult(Matrix<T> matrix)
        {
            var dense = CreateDense(matrix.RowCount, matrix.ColumnCount);
            matrix.UpperTriangle(dense);
            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    Assert.That(dense[i, j], Is.EqualTo(i <= j ? matrix[i, j] : Zero));
                }
            }

            var sparse = CreateSparse(matrix.RowCount, matrix.ColumnCount);
            matrix.UpperTriangle(sparse);
            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    Assert.That(sparse[i, j], Is.EqualTo(i <= j ? matrix[i, j] : Zero));
                }
            }

            Assert.That(() => matrix.UpperTriangle(null), Throws.InstanceOf<ArgumentNullException>());
            Assert.That(() => matrix.UpperTriangle(CreateSparse(matrix.RowCount + 1, matrix.ColumnCount)), Throws.ArgumentException);
            Assert.That(() => matrix.UpperTriangle(CreateDense(matrix.RowCount, matrix.ColumnCount + 1)), Throws.ArgumentException);
        }

        [Theory, Timeout(200)]
        public void CanGetLowerTriangle(Matrix<T> matrix)
        {
            var upper = matrix.LowerTriangle();
            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    Assert.That(upper[i, j], Is.EqualTo(i >= j ? matrix[i, j] : Zero));
                }
            }
        }

        [Theory, Timeout(200)]
        public void CanGetLowerTriangleIntoResult(Matrix<T> matrix)
        {
            var dense = CreateDense(matrix.RowCount, matrix.ColumnCount);
            matrix.LowerTriangle(dense);
            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    Assert.That(dense[i, j], Is.EqualTo(i >= j ? matrix[i, j] : Zero));
                }
            }

            var sparse = CreateSparse(matrix.RowCount, matrix.ColumnCount);
            matrix.LowerTriangle(sparse);
            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    Assert.That(sparse[i, j], Is.EqualTo(i >= j ? matrix[i, j] : Zero));
                }
            }

            Assert.That(() => matrix.LowerTriangle(null), Throws.InstanceOf<ArgumentNullException>());
            Assert.That(() => matrix.LowerTriangle(CreateSparse(matrix.RowCount + 1, matrix.ColumnCount)), Throws.ArgumentException);
            Assert.That(() => matrix.LowerTriangle(CreateDense(matrix.RowCount, matrix.ColumnCount + 1)), Throws.ArgumentException);
        }

        [Theory, Timeout(200)]
        public void CanGetStrictlyUpperTriangle(Matrix<T> matrix)
        {
            var upper = matrix.StrictlyUpperTriangle();
            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    Assert.That(upper[i, j], Is.EqualTo(i < j ? matrix[i, j] : Zero));
                }
            }
        }

        [Theory, Timeout(200)]
        public void CanGetStrictlyUpperTriangleIntoResult(Matrix<T> matrix)
        {
            var dense = CreateDense(matrix.RowCount, matrix.ColumnCount);
            matrix.StrictlyUpperTriangle(dense);
            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    Assert.That(dense[i, j], Is.EqualTo(i < j ? matrix[i, j] : Zero));
                }
            }

            var sparse = CreateSparse(matrix.RowCount, matrix.ColumnCount);
            matrix.StrictlyUpperTriangle(sparse);
            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    Assert.That(sparse[i, j], Is.EqualTo(i < j ? matrix[i, j] : Zero));
                }
            }

            Assert.That(() => matrix.StrictlyUpperTriangle(null), Throws.InstanceOf<ArgumentNullException>());
            Assert.That(() => matrix.StrictlyUpperTriangle(CreateSparse(matrix.RowCount + 1, matrix.ColumnCount)), Throws.ArgumentException);
            Assert.That(() => matrix.StrictlyUpperTriangle(CreateDense(matrix.RowCount, matrix.ColumnCount + 1)), Throws.ArgumentException);
        }

        [Theory, Timeout(200)]
        public void CanGetStrictlyLowerTriangle(Matrix<T> matrix)
        {
            var upper = matrix.StrictlyLowerTriangle();
            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    Assert.That(upper[i, j], Is.EqualTo(i > j ? matrix[i, j] : Zero));
                }
            }
        }

        [Theory, Timeout(200)]
        public void CanGetStrictlyLowerTriangleIntoResult(Matrix<T> matrix)
        {
            var dense = CreateDense(matrix.RowCount, matrix.ColumnCount);
            matrix.StrictlyLowerTriangle(dense);
            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    Assert.That(dense[i, j], Is.EqualTo(i > j ? matrix[i, j] : Zero));
                }
            }

            var sparse = CreateSparse(matrix.RowCount, matrix.ColumnCount);
            matrix.StrictlyLowerTriangle(sparse);
            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    Assert.That(sparse[i, j], Is.EqualTo(i > j ? matrix[i, j] : Zero));
                }
            }

            Assert.That(() => matrix.StrictlyLowerTriangle(null), Throws.InstanceOf<ArgumentNullException>());
            Assert.That(() => matrix.StrictlyLowerTriangle(CreateSparse(matrix.RowCount + 1, matrix.ColumnCount)), Throws.ArgumentException);
            Assert.That(() => matrix.StrictlyLowerTriangle(CreateDense(matrix.RowCount, matrix.ColumnCount + 1)), Throws.ArgumentException);
        }

        #endregion

        #region Diagonal Access

        [Theory, Timeout(200)]
        public void CanGetDiagonal(Matrix<T> matrix)
        {
            var diag = matrix.Diagonal();
            Assert.That(diag.Count, Is.EqualTo(Math.Min(matrix.RowCount, matrix.ColumnCount)));
            for (var i = 0; i < Math.Min(matrix.RowCount, matrix.ColumnCount); i++)
            {
                Assert.That(diag[i], Is.EqualTo(matrix[i, i]));
            }
        }

        [Theory, Timeout(200)]
        public void CanSetDiagonal(Matrix<T> matrix)
        {
            var m = matrix.Clone();
            m.SetDiagonal(CreateVector(Math.Min(matrix.RowCount, matrix.ColumnCount)));
            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    Assert.That(m[i, j], Is.EqualTo(i == j ? Zero : matrix[i, j]));
                }
            }

            // Invalid
            Assert.That(() => matrix.SetDiagonal(default(Vector<T>)), Throws.InstanceOf<ArgumentNullException>());
            Assert.That(() => matrix.SetDiagonal(CreateVector(Math.Min(matrix.RowCount, matrix.ColumnCount) - 1)), Throws.ArgumentException);
            Assert.That(() => matrix.SetDiagonal(CreateVector(Math.Min(matrix.RowCount, matrix.ColumnCount) + 1)), Throws.ArgumentException);
        }

        [Theory, Timeout(200)]
        public void CanSetDiagonalArray(Matrix<T> matrix)
        {
            var m = matrix.Clone();
            m.SetDiagonal(new T[Math.Min(matrix.RowCount, matrix.ColumnCount)]);
            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    Assert.That(m[i, j], Is.EqualTo(i == j ? Zero : matrix[i, j]));
                }
            }

            // Invalid
            Assert.That(() => matrix.SetDiagonal(default(T[])), Throws.InstanceOf<ArgumentNullException>());
            Assert.That(() => matrix.SetDiagonal(new T[Math.Min(matrix.RowCount, matrix.ColumnCount) - 1]), Throws.ArgumentException);
            Assert.That(() => matrix.SetDiagonal(new T[Math.Min(matrix.RowCount, matrix.ColumnCount) + 1]), Throws.ArgumentException);
        }

        #endregion
    }
}
