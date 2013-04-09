// <copyright file="MatrixStructureTheory.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
//
// Copyright (c) 2009-2013 Math.NET
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

        protected readonly T Zero;
        protected readonly dynamic Dense;
        protected readonly dynamic Sparse;
        protected readonly dynamic Diagonal;
        protected readonly dynamic DenseVector;
        protected readonly dynamic SparseVector;

        protected MatrixStructureTheory(T zero, Type dense, Type sparse, Type diagonal, Type denseVector, Type sparseVector)
        {
            Zero = zero;
            Dense = new StaticDynamicWrapper(dense);
            Sparse = new StaticDynamicWrapper(sparse);
            Diagonal = new StaticDynamicWrapper(diagonal);
            DenseVector = new StaticDynamicWrapper(denseVector);
            SparseVector = new StaticDynamicWrapper(sparseVector);
        }

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

        [Theory]
        public void IsEqualToItself(Matrix<T> matrix)
        {
            Assert.That(matrix, Is.EqualTo(matrix));
            Assert.IsTrue(matrix.Equals(matrix));
            Assert.IsTrue(matrix.Equals((object) matrix));
            Assert.IsTrue(((object) matrix).Equals(matrix));
            Assert.IsTrue(matrix == (object) matrix);
            Assert.IsTrue((object) matrix == matrix);
        }

        [Theory]
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

        [Theory]
        public void IsNotEqualToNonMatrixType(Matrix<T> matrix)
        {
            Assert.That(matrix, Is.Not.EqualTo(2));
            Assert.IsFalse(matrix.Equals(2));
            Assert.IsFalse(matrix.Equals((object) 2));
            Assert.IsFalse(((object) matrix).Equals(2));
            Assert.IsFalse(matrix == (object) 2);
        }

        [Theory]
        public void CanClone(Matrix<T> matrix)
        {
            var clone = matrix.Clone();
            Assert.That(clone, Is.Not.SameAs(matrix));
            Assert.That(clone, Is.EqualTo(matrix));
            Assert.That(clone.RowCount, Is.EqualTo(matrix.RowCount));
            Assert.That(clone.ColumnCount, Is.EqualTo(matrix.ColumnCount));
        }

        [Theory]
        public void CanCloneUsingICloneable(Matrix<T> matrix)
        {
            var clone = (Matrix<T>) ((ICloneable) matrix).Clone();
            Assert.That(clone, Is.Not.SameAs(matrix));
            Assert.That(clone, Is.EqualTo(matrix));
            Assert.That(clone.RowCount, Is.EqualTo(matrix.RowCount));
            Assert.That(clone.ColumnCount, Is.EqualTo(matrix.ColumnCount));
        }

        [Theory]
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

        [Theory]
        public void CanGetHashCode(Matrix<T> matrix)
        {
            Assert.That(matrix.GetHashCode(), Is.Not.EqualTo(matrix.CreateMatrix(matrix.RowCount, matrix.ColumnCount).GetHashCode()));
        }

        [Theory]
        public void CanClear(Matrix<T> matrix)
        {
            var cleared = matrix.Clone();
            cleared.Clear();
            Assert.That(cleared, Is.EqualTo(matrix.CreateMatrix(matrix.RowCount, matrix.ColumnCount)));
        }

        [Theory]
        public void CanClearSubMatrix(Matrix<T> matrix)
        {
            var cleared = matrix.Clone();
            Assume.That(cleared.RowCount, Is.GreaterThanOrEqualTo(2));
            Assume.That(cleared.ColumnCount, Is.GreaterThanOrEqualTo(2));

            cleared.Storage.Clear(0, 2, 1, 1);
            Assert.That(cleared.At(0, 0), Is.EqualTo(matrix.At(0, 0)));
            Assert.That(cleared.At(1, 0), Is.EqualTo(matrix.At(1, 0)));
            Assert.That(cleared.At(0, 1), Is.EqualTo(Zero));
            Assert.That(cleared.At(1, 1), Is.EqualTo(Zero));
        }

        [Theory]
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

        [Theory]
        public void CanToColumnWiseArray(Matrix<T> matrix)
        {
            var array = matrix.ToColumnWiseArray();
            Assert.That(array.Length, Is.EqualTo(matrix.RowCount*matrix.ColumnCount));
            for (int i = 0; i < array.Length; i++)
            {
                Assert.That(array[i], Is.EqualTo(matrix[i%matrix.RowCount, i/matrix.RowCount]));
            }
        }

        [Theory]
        public void CanToRowWiseArray(Matrix<T> matrix)
        {
            var array = matrix.ToRowWiseArray();
            Assert.That(array.Length, Is.EqualTo(matrix.RowCount*matrix.ColumnCount));
            for (int i = 0; i < array.Length; i++)
            {
                Assert.That(array[i], Is.EqualTo(matrix[i/matrix.ColumnCount, i%matrix.ColumnCount]));
            }
        }

        [Theory]
        public void CanCreateSameType(Matrix<T> matrix)
        {
            var empty = matrix.CreateMatrix(5, 6);
            Assert.That(empty, Is.EqualTo(CreateDenseZero(5, 6)));
            Assert.That(empty.GetType(), Is.EqualTo(matrix.GetType()));

            Assert.That(() => matrix.CreateMatrix(0, 2), Throws.InstanceOf<ArgumentOutOfRangeException>());
            Assert.That(() => matrix.CreateMatrix(2, 0), Throws.InstanceOf<ArgumentOutOfRangeException>());
            Assert.That(() => matrix.CreateMatrix(-1, -1), Throws.InstanceOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void CanCreateDenseFromMultiDimArray()
        {
            T[,] array = CreateDenseRandom(4, 3, 0).ToArray();
            var matrix = Dense.OfArray(array);
            Assert.That(matrix.GetType().Name, Is.EqualTo("DenseMatrix"));
            Assert.That(matrix.RowCount, Is.EqualTo(4));
            Assert.That(matrix.ColumnCount, Is.EqualTo(3));
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 3; j++)
                    Assert.That(matrix[i, j], Is.EqualTo(array[i, j]));
        }

        [Test]
        public void CanCreateSparseFromMultiDimArray()
        {
            T[,] array = CreateDenseRandom(4, 3, 0).ToArray();
            var matrix = Sparse.OfArray(array);
            Assert.That(matrix.GetType().Name, Is.EqualTo("SparseMatrix"));
            Assert.That(matrix.RowCount, Is.EqualTo(4));
            Assert.That(matrix.ColumnCount, Is.EqualTo(3));
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 3; j++)
                    Assert.That(matrix[i, j], Is.EqualTo(array[i, j]));
        }

        [Test]
        public void CanCreateDenseFromJaggedArray()
        {
            T[][] array = new[]
                {
                    CreateVectorRandom(4, 0).ToArray(),
                    CreateVectorRandom(4, 1).ToArray(),
                    CreateVectorRandom(4, 3).ToArray()
                };
            var matrix = Dense.OfRows(3, 4, array);
            Assert.That(matrix.GetType().Name, Is.EqualTo("DenseMatrix"));
            Assert.That(matrix.RowCount, Is.EqualTo(3));
            Assert.That(matrix.ColumnCount, Is.EqualTo(4));
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 4; j++)
                    Assert.That(matrix[i, j], Is.EqualTo(array[i][j]));
        }

        [Test]
        public void CanCreateSparseFromJaggedArray()
        {
            T[][] array = new[]
                {
                    CreateVectorRandom(4, 0).ToArray(),
                    CreateVectorRandom(4, 1).ToArray(),
                    CreateVectorRandom(4, 3).ToArray()
                };
            var matrix = Sparse.OfRows(3, 4, array);
            Assert.That(matrix.GetType().Name, Is.EqualTo("SparseMatrix"));
            Assert.That(matrix.RowCount, Is.EqualTo(3));
            Assert.That(matrix.ColumnCount, Is.EqualTo(4));
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 4; j++)
                    Assert.That(matrix[i, j], Is.EqualTo(array[i][j]));
        }

        [Test]
        public void CanCreateDenseFromColumnVectors()
        {
            var columns = new[]
                {
                    CreateVectorRandom(4, 0),
                    CreateVectorRandom(4, 1),
                    CreateVectorRandom(4, 3)
                };
            var matrix = Dense.OfColumns(4, 3, columns);
            Assert.That(matrix.GetType().Name, Is.EqualTo("DenseMatrix"));
            Assert.That(matrix.RowCount, Is.EqualTo(4));
            Assert.That(matrix.ColumnCount, Is.EqualTo(3));
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 3; j++)
                    Assert.That(matrix[i, j], Is.EqualTo(columns[j][i]));
        }

        [Test]
        public void CanCreateSparseFromColumnVectors()
        {
            var columns = new[]
                {
                    CreateVectorRandom(4, 0),
                    CreateVectorRandom(4, 1),
                    CreateVectorRandom(4, 3)
                };
            var matrix = Sparse.OfColumns(4, 3, columns);
            Assert.That(matrix.GetType().Name, Is.EqualTo("SparseMatrix"));
            Assert.That(matrix.RowCount, Is.EqualTo(4));
            Assert.That(matrix.ColumnCount, Is.EqualTo(3));
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 3; j++)
                    Assert.That(matrix[i, j], Is.EqualTo(columns[j][i]));
        }

        [Test]
        public void CanCreateDenseFromRowVectors()
        {
            var rows = new[]
                {
                    CreateVectorRandom(4, 0),
                    CreateVectorRandom(4, 1),
                    CreateVectorRandom(4, 3)
                };
            var matrix = Dense.OfRows(3, 4, rows);
            Assert.That(matrix.GetType().Name, Is.EqualTo("DenseMatrix"));
            Assert.That(matrix.RowCount, Is.EqualTo(3));
            Assert.That(matrix.ColumnCount, Is.EqualTo(4));
            for (int j = 0; j < 4; j++)
                for (int i = 0; i < 3; i++)
                    Assert.That(matrix[i, j], Is.EqualTo(rows[i][j]));
        }

        [Test]
        public void CanCreateSparseFromRowVectors()
        {
            var rows = new[]
                {
                    CreateVectorRandom(4, 0),
                    CreateVectorRandom(4, 1),
                    CreateVectorRandom(4, 3)
                };
            var matrix = Sparse.OfRows(3, 4, rows);
            Assert.That(matrix.GetType().Name, Is.EqualTo("SparseMatrix"));
            Assert.That(matrix.RowCount, Is.EqualTo(3));
            Assert.That(matrix.ColumnCount, Is.EqualTo(4));
            for (int j = 0; j < 4; j++)
                for (int i = 0; i < 3; i++)
                    Assert.That(matrix[i, j], Is.EqualTo(rows[i][j]));
        }

        [Test]
        public void CanEnumerateWithIndex()
        {
            var dense = CreateDenseRandom(2, 3, 0);
            using (var enumerator = dense.IndexedEnumerator().GetEnumerator())
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
