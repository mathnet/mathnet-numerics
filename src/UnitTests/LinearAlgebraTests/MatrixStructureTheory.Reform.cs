using System;
using System.Linq;
using MathNet.Numerics.LinearAlgebra.Generic;
using NUnit.Framework;

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests
{
    partial class MatrixStructureTheory<T>
    {
        [Theory]
        public void CanPermuteRows(Matrix<T> matrix)
        {
            var m = matrix.Clone();
            var rnd = new System.Random(0);
            var permutation = new Permutation(Enumerable.Range(0, matrix.RowCount).OrderBy(i => rnd.Next()).ToArray());

            try
            {
                m.PermuteRows(permutation);
            }
            catch (InvalidOperationException)
            {
                Assert.Ignore("Matrix type {0} does not support permutations", matrix.GetType().FullName);
            }

            Assert.That(m, Is.Not.SameAs(matrix));
            Assert.That(m.RowCount, Is.EqualTo(matrix.RowCount));
            Assert.That(m.ColumnCount, Is.EqualTo(matrix.ColumnCount));
            var inverse = permutation.Inverse();
            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    Assert.That(m[i, j], Is.EqualTo(matrix[inverse[i], j]));
                }
            }
        }

        [Theory]
        public void CanPermuteColumns(Matrix<T> matrix)
        {
            var m = matrix.Clone();
            var rnd = new System.Random(0);
            var permutation = new Permutation(Enumerable.Range(0, matrix.ColumnCount).OrderBy(i => rnd.Next()).ToArray());

            try
            {
                m.PermuteColumns(permutation);
            }
            catch (InvalidOperationException)
            {
                Assert.Ignore("Matrix type {0} does not support permutations", matrix.GetType().FullName);
            }

            Assert.That(m, Is.Not.SameAs(matrix));
            Assert.That(m.RowCount, Is.EqualTo(matrix.RowCount));
            Assert.That(m.ColumnCount, Is.EqualTo(matrix.ColumnCount));
            var inverse = permutation.Inverse();
            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    Assert.That(m[i, j], Is.EqualTo(matrix[i, inverse[j]]));
                }
            }
        }

        [Theory]
        public void CanInsertRow(Matrix<T> matrix)
        {
            var row = CreateVectorRandom(matrix.ColumnCount, 0);
            for (var position = 0; position < matrix.RowCount + 1; position++)
            {
                var result = matrix.InsertRow(position, row);
                Assert.That(result.RowCount, Is.EqualTo(matrix.RowCount + 1));
                for (int ir = 0, im = 0; ir < result.RowCount; ir++, im++)
                {
                    if (ir == position)
                    {
                        im--;
                        for (var j = 0; j < result.ColumnCount; j++)
                        {
                            Assert.That(result[ir, j], Is.EqualTo(row[j]), "A({0},{1}) for {2}", ir, j, matrix.GetType().FullName);
                        }
                    }
                    else
                    {
                        for (var j = 0; j < result.ColumnCount; j++)
                        {
                            Assert.That(result[ir, j], Is.EqualTo(matrix[im, j]), "A({0},{1}) for {2}", ir, j, matrix.GetType().FullName);
                        }
                    }
                }
            }

            // Invalid
            Assert.That(() => matrix.InsertRow(0, default(Vector<T>)), Throws.InstanceOf<ArgumentNullException>());
            Assert.That(() => matrix.InsertRow(-1, CreateVectorZero(matrix.ColumnCount)), Throws.InstanceOf<ArgumentOutOfRangeException>());
            Assert.That(() => matrix.InsertRow(matrix.RowCount + 1, CreateVectorZero(matrix.ColumnCount)), Throws.InstanceOf<ArgumentOutOfRangeException>());
            Assert.That(() => matrix.InsertRow(0, CreateVectorZero(matrix.ColumnCount - 1)), Throws.ArgumentException);
            Assert.That(() => matrix.InsertRow(0, CreateVectorZero(matrix.ColumnCount + 1)), Throws.ArgumentException);
        }

        [Theory]
        public void CanInsertColumn(Matrix<T> matrix)
        {
            var column = CreateVectorRandom(matrix.RowCount, 0);
            for (var position = 0; position < matrix.ColumnCount + 1; position++)
            {
                var result = matrix.InsertColumn(position, column);
                Assert.That(result.ColumnCount, Is.EqualTo(matrix.ColumnCount + 1));
                for (int jr = 0, jm = 0; jr < result.ColumnCount; jr++, jm++)
                {
                    if (jr == position)
                    {
                        jm--;
                        for (var i = 0; i < result.RowCount; i++)
                        {
                            Assert.That(result[i, jr], Is.EqualTo(column[i]));
                        }
                    }
                    else
                    {
                        for (var i = 0; i < result.RowCount; i++)
                        {
                            Assert.That(result[i, jr], Is.EqualTo(matrix[i, jm]));
                        }
                    }
                }
            }

            // Invalid
            Assert.That(() => matrix.InsertColumn(0, default(Vector<T>)), Throws.InstanceOf<ArgumentNullException>());
            Assert.That(() => matrix.InsertColumn(-1, CreateVectorZero(matrix.RowCount)), Throws.InstanceOf<ArgumentOutOfRangeException>());
            Assert.That(() => matrix.InsertColumn(matrix.ColumnCount + 1, CreateVectorZero(matrix.RowCount)), Throws.InstanceOf<ArgumentOutOfRangeException>());
            Assert.That(() => matrix.InsertColumn(0, CreateVectorZero(matrix.RowCount - 1)), Throws.ArgumentException);
            Assert.That(() => matrix.InsertColumn(0, CreateVectorZero(matrix.RowCount + 1)), Throws.ArgumentException);
        }

        [Theory]
        public void CanAppend(Matrix<T> left, Matrix<T> right)
        {
            // IF
            Assume.That(left.RowCount, Is.EqualTo(right.RowCount));

            // THEN
            var result = left.Append(right);

            Assert.That(result.ColumnCount, Is.EqualTo(left.ColumnCount + right.ColumnCount));
            for (var i = 0; i < result.RowCount; i++)
            {
                for (var j = 0; j < result.ColumnCount; j++)
                {
                    Assert.That(result[i, j], Is.EqualTo(j < left.ColumnCount ? left[i, j] : right[i, j - left.ColumnCount]));
                }
            }

            // Invalid
            Assert.That(() => left.Append(default(Matrix<T>)), Throws.InstanceOf<ArgumentNullException>());
        }

        [Theory]
        public void CanAppendIntoResult(Matrix<T> left, Matrix<T> right)
        {
            // IF
            Assume.That(left.RowCount, Is.EqualTo(right.RowCount));

            // THEN
            var result = CreateDenseZero(left.RowCount, left.ColumnCount + right.ColumnCount);
            left.Append(right, result);

            Assert.That(result.ColumnCount, Is.EqualTo(left.ColumnCount + right.ColumnCount));
            for (var i = 0; i < result.RowCount; i++)
            {
                for (var j = 0; j < result.ColumnCount; j++)
                {
                    Assert.That(result[i, j], Is.EqualTo(j < left.ColumnCount ? left[i, j] : right[i, j - left.ColumnCount]));
                }
            }

            // Invalid
            Assert.That(() => left.Append(right, default(Matrix<T>)), Throws.InstanceOf<ArgumentNullException>());
            Assert.That(() => left.Append(right, CreateDenseZero(left.RowCount + 1, left.ColumnCount + right.ColumnCount)), Throws.ArgumentException);
            Assert.That(() => left.Append(right, CreateDenseZero(left.RowCount - 1, left.ColumnCount + right.ColumnCount)), Throws.ArgumentException);
            Assert.That(() => left.Append(right, CreateDenseZero(left.RowCount, left.ColumnCount + right.ColumnCount + 1)), Throws.ArgumentException);
            Assert.That(() => left.Append(right, CreateDenseZero(left.RowCount, left.ColumnCount + right.ColumnCount - 1)), Throws.ArgumentException);
        }

        [Theory]
        public void CanStack(Matrix<T> top, Matrix<T> bottom)
        {
            // IF
            Assume.That(top.ColumnCount, Is.EqualTo(bottom.ColumnCount));

            // THEN
            var result = top.Stack(bottom);

            Assert.That(result.RowCount, Is.EqualTo(top.RowCount + bottom.RowCount));
            for (var i = 0; i < result.RowCount; i++)
            {
                for (var j = 0; j < result.ColumnCount; j++)
                {
                    Assert.That(result[i, j], Is.EqualTo(i < top.RowCount ? top[i, j] : bottom[i - top.RowCount, j]));
                }
            }

            // Invalid
            Assert.That(() => top.Stack(default(Matrix<T>)), Throws.InstanceOf<ArgumentNullException>());
        }

        [Theory]
        public void CanStackIntoResult(Matrix<T> top, Matrix<T> bottom)
        {
            // IF
            Assume.That(top.ColumnCount, Is.EqualTo(bottom.ColumnCount));

            // THEN
            var result = CreateDenseZero(top.RowCount + bottom.RowCount, top.ColumnCount);
            top.Stack(bottom, result);

            Assert.That(result.RowCount, Is.EqualTo(top.RowCount + bottom.RowCount));
            for (var i = 0; i < result.RowCount; i++)
            {
                for (var j = 0; j < result.ColumnCount; j++)
                {
                    Assert.That(result[i, j], Is.EqualTo(i < top.RowCount ? top[i, j] : bottom[i - top.RowCount, j]));
                }
            }

            // Invalid
            Assert.That(() => top.Stack(bottom, default(Matrix<T>)), Throws.InstanceOf<ArgumentNullException>());
            Assert.That(() => top.Stack(bottom, CreateDenseZero(top.RowCount + bottom.RowCount + 1, top.ColumnCount)), Throws.ArgumentException);
            Assert.That(() => top.Stack(bottom, CreateDenseZero(top.RowCount + bottom.RowCount - 1, top.ColumnCount)), Throws.ArgumentException);
            Assert.That(() => top.Stack(bottom, CreateDenseZero(top.RowCount + bottom.RowCount, top.ColumnCount + 1)), Throws.ArgumentException);
            Assert.That(() => top.Stack(bottom, CreateDenseZero(top.RowCount + bottom.RowCount, top.ColumnCount - 1)), Throws.ArgumentException);
        }

        [Theory]
        public void CanDiagonalStack(Matrix<T> left, Matrix<T> right)
        {
            var result = left.DiagonalStack(right);

            Assert.That(result.RowCount, Is.EqualTo(left.RowCount + right.RowCount));
            Assert.That(result.ColumnCount, Is.EqualTo(left.ColumnCount + right.ColumnCount));
            for (var i = 0; i < left.RowCount; i++)
            {
                for (var j = 0; j < left.ColumnCount; j++)
                {
                    Assert.That(result[i, j], Is.EqualTo(left[i, j]), "{0}+{1}->{2}", left.GetType(), right.GetType(), result.GetType());
                }
            }
            for (var i = 0; i < right.RowCount; i++)
            {
                for (var j = 0; j < right.ColumnCount; j++)
                {
                    Assert.That(result[left.RowCount + i, left.ColumnCount + j], Is.EqualTo(right[i, j]), "{0}+{1}->{2}", left.GetType(), right.GetType(), result.GetType());
                }
            }

            // Invalid
            Assert.That(() => left.DiagonalStack(default(Matrix<T>)), Throws.InstanceOf<ArgumentNullException>(), "{0}+{1}->{2}", left.GetType(), right.GetType(), result.GetType());
        }

        [Theory]
        public void CanDiagonalStackIntoResult(Matrix<T> left, Matrix<T> right)
        {
            var result = CreateDenseZero(left.RowCount + right.RowCount, left.ColumnCount + right.ColumnCount);
            left.DiagonalStack(right, result);

            Assert.That(result.RowCount, Is.EqualTo(left.RowCount + right.RowCount));
            Assert.That(result.ColumnCount, Is.EqualTo(left.ColumnCount + right.ColumnCount));
            for (var i = 0; i < left.RowCount; i++)
            {
                for (var j = 0; j < left.ColumnCount; j++)
                {
                    Assert.That(result[i, j], Is.EqualTo(left[i, j]));
                }
            }
            for (var i = 0; i < right.RowCount; i++)
            {
                for (var j = 0; j < right.ColumnCount; j++)
                {
                    Assert.That(result[left.RowCount + i, left.ColumnCount + j], Is.EqualTo(right[i, j]));
                }
            }

            // Invalid
            Assert.That(() => left.DiagonalStack(right, default(Matrix<T>)), Throws.InstanceOf<ArgumentNullException>());
            Assert.That(() => left.DiagonalStack(right, CreateDenseZero(left.RowCount + right.RowCount + 1, left.ColumnCount + right.ColumnCount)), Throws.ArgumentException);
            Assert.That(() => left.DiagonalStack(right, CreateDenseZero(left.RowCount + right.RowCount - 1, left.ColumnCount + right.ColumnCount)), Throws.ArgumentException);
            Assert.That(() => left.DiagonalStack(right, CreateDenseZero(left.RowCount + right.RowCount, left.ColumnCount + right.ColumnCount + 1)), Throws.ArgumentException);
            Assert.That(() => left.DiagonalStack(right, CreateDenseZero(left.RowCount + right.RowCount, left.ColumnCount + right.ColumnCount - 1)), Throws.ArgumentException);
        }
    }
}
