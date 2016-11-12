// <copyright file="MatrixStructureTheory.cs" company="Math.NET">
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
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using NUnit.Framework;

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests
{
    [TestFixture, Category("LA")]
    public abstract partial class MatrixStructureTheory<T>
        where T : struct, IEquatable<T>, IFormattable
    {
        protected abstract Matrix<T> Get(TestMatrix matrix);

        protected readonly T Zero = Matrix<T>.Build.Zero;

        protected Matrix<T> CreateDenseFor(Matrix<T> m, int rows = -1, int columns = -1, int seed = 1)
        {
            return m.Storage.IsFullyMutable
                ? Matrix<T>.Build.Random(rows >= 0 ? rows : m.RowCount, columns >= 0 ? columns : m.ColumnCount, seed)
                : Matrix<T>.Build.Dense(rows >= 0 ? rows : m.RowCount, columns >= 0 ? columns : m.ColumnCount);
        }

        protected Vector<T> CreateVectorFor(Matrix<T> m, int size, int seed = 1)
        {
            return m.Storage.IsFullyMutable
                ? Vector<T>.Build.Random(size, seed)
                : Vector<T>.Build.Dense(size);
        }

        [Theory]
        public void IsEqualToItself(TestMatrix testMatrix)
        {
            Matrix<T> matrix = Get(testMatrix);
            object matrixObject = matrix;
            Assert.That(matrix, Is.EqualTo(matrix));
            Assert.IsTrue(matrix.Equals(matrix));
            Assert.IsTrue(matrix.Equals(matrixObject));
            Assert.IsTrue(matrixObject.Equals(matrix));
            Assert.IsTrue(matrix == matrixObject);
            Assert.IsTrue(matrixObject == matrix);
        }

        [Theory]
        public void IsNotEqualToOthers(TestMatrix leftTestMatrix, TestMatrix rightTestmatrix)
        {
            Matrix<T> left = Get(leftTestMatrix);
            Matrix<T> right = Get(rightTestmatrix);
            Assume.That(leftTestMatrix, Is.Not.EqualTo(rightTestmatrix));

            // THEN
            object leftObject = left;
            object rightObject = right;
            Assert.That(left, Is.Not.EqualTo(right));
            Assert.IsFalse(left.Equals(right));
            Assert.IsFalse(left.Equals(rightObject));
            Assert.IsFalse(leftObject.Equals(right));
            Assert.IsFalse(left == rightObject);
            Assert.IsFalse(leftObject == right);
        }

        [Theory]
        public void IsNotEqualToPermutation(TestMatrix testMatrix)
        {
            Matrix<T> matrix = Get(testMatrix);
            if (!matrix.Storage.IsFullyMutable)
            {
                return;
            }

            Matrix<T> permutation;
            if (matrix.RowCount >= 2 && matrix.Row(1).Any(x => !Zero.Equals(x)))
            {
                matrix.ClearRow(0);
                permutation = matrix.Clone();
                permutation.ClearRow(1);
                permutation.SetRow(0, matrix.Row(1));
            }
            else if (matrix.ColumnCount >= 2 && matrix.Column(1).Any(x => !Zero.Equals(x)))
            {
                matrix.ClearColumn(0);
                permutation = matrix.Clone();
                permutation.ClearColumn(1);
                permutation.SetColumn(0, matrix.Column(1));
            }
            else
            {
                return;
            }

            Assert.That(matrix, Is.Not.EqualTo(permutation));
            Assert.IsFalse(matrix.Equals(permutation));
        }

        [Theory]
        public void IsNotEqualToNonMatrixType(TestMatrix testMatrix)
        {
            Matrix<T> matrix = Get(testMatrix);
            Assert.That(matrix, Is.Not.EqualTo(2));
            Assert.IsFalse(matrix.Equals(2));
            Assert.IsFalse(matrix.Equals((object)2));
            Assert.IsFalse(((object)matrix).Equals(2));
            Assert.IsFalse(matrix == (object)2);
        }

        [Theory]
        public void CanClone(TestMatrix testMatrix)
        {
            Matrix<T> matrix = Get(testMatrix);
            var clone = matrix.Clone();
            Assert.That(clone, Is.Not.SameAs(matrix));
            Assert.That(clone, Is.EqualTo(matrix));
            Assert.That(clone.RowCount, Is.EqualTo(matrix.RowCount));
            Assert.That(clone.ColumnCount, Is.EqualTo(matrix.ColumnCount));
        }

#if !PORTABLE
        [Theory]
        public void CanCloneUsingICloneable(TestMatrix testMatrix)
        {
            Matrix<T> matrix = Get(testMatrix);
            var clone = (Matrix<T>)((ICloneable)matrix).Clone();
            Assert.That(clone, Is.Not.SameAs(matrix));
            Assert.That(clone, Is.EqualTo(matrix));
            Assert.That(clone.RowCount, Is.EqualTo(matrix.RowCount));
            Assert.That(clone.ColumnCount, Is.EqualTo(matrix.ColumnCount));
        }
#endif

        [Theory]
        public void CanCopyTo(TestMatrix testMatrix)
        {
            Matrix<T> matrix = Get(testMatrix);
            var dense = Matrix<T>.Build.Dense(matrix.RowCount, matrix.ColumnCount);
            matrix.CopyTo(dense);
            Assert.That(dense, Is.EqualTo(matrix));

            var sparse = Matrix<T>.Build.Sparse(matrix.RowCount, matrix.ColumnCount);
            matrix.CopyTo(sparse);
            Assert.That(sparse, Is.EqualTo(matrix));

            // null arg
            Assert.That(() => matrix.CopyTo(null), Throws.InstanceOf<ArgumentNullException>());

            // bad arg
            Assert.That(() => matrix.CopyTo(Matrix<T>.Build.Dense(matrix.RowCount + 1, matrix.ColumnCount)), Throws.ArgumentException);
            Assert.That(() => matrix.CopyTo(Matrix<T>.Build.Dense(matrix.RowCount, matrix.ColumnCount + 1)), Throws.ArgumentException);
        }

        [Theory]
        public void CanGetHashCode(TestMatrix testMatrix)
        {
            Matrix<T> matrix = Get(testMatrix);
            Assert.That(matrix.GetHashCode(), Is.Not.EqualTo(Matrix<T>.Build.SameAs(matrix).GetHashCode()));
        }

        [Theory]
        public void CanClear(TestMatrix testMatrix)
        {
            Matrix<T> matrix = Get(testMatrix);
            var cleared = matrix.Clone();
            cleared.Clear();
            Assert.That(cleared, Is.EqualTo(Matrix<T>.Build.SameAs(matrix)));
        }

        [Theory]
        public void CanClearSubMatrix(TestMatrix testMatrix)
        {
            Matrix<T> matrix = Get(testMatrix);
            Assume.That(matrix.RowCount, Is.GreaterThanOrEqualTo(2));
            Assume.That(matrix.ColumnCount, Is.GreaterThanOrEqualTo(2));

            var cleared = matrix.Clone();
            cleared.ClearSubMatrix(0, 2, 1, 1);
            Assert.That(cleared.At(0, 0), Is.EqualTo(matrix.At(0, 0)));
            Assert.That(cleared.At(1, 0), Is.EqualTo(matrix.At(1, 0)));
            Assert.That(cleared.At(0, 1), Is.EqualTo(Zero));
            Assert.That(cleared.At(1, 1), Is.EqualTo(Zero));
        }

        [Theory]
        public void CanClearRows(TestMatrix testMatrix)
        {
            Matrix<T> matrix = Get(testMatrix);
            Assume.That(matrix.RowCount, Is.GreaterThanOrEqualTo(2));

            var cleared = matrix.Clone();
            cleared.ClearRows(1);
            Assert.That(cleared.At(0, 0), Is.EqualTo(matrix.At(0, 0)));
            Assert.That(cleared.At(1, 0), Is.EqualTo(Zero));
        }

        [Theory]
        public void CanClearColumns(TestMatrix testMatrix)
        {
            Matrix<T> matrix = Get(testMatrix);
            Assume.That(matrix.ColumnCount, Is.GreaterThanOrEqualTo(2));

            var cleared = matrix.Clone();
            cleared.ClearColumns(1);
            Assert.That(cleared.At(0, 0), Is.EqualTo(matrix.At(0, 0)));
            Assert.That(cleared.At(0, 1), Is.EqualTo(Zero));
        }

        [Theory]
        public void CanToArray(TestMatrix testMatrix)
        {
            Matrix<T> matrix = Get(testMatrix);
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
        public void CanToColumnArrays(TestMatrix testMatrix)
        {
            Matrix<T> matrix = Get(testMatrix);
            var columnArrays = matrix.ToColumnArrays();
            Assert.That(columnArrays.Length, Is.EqualTo(matrix.ColumnCount));
            Assert.That(columnArrays[0].Length, Is.EqualTo(matrix.RowCount));
            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    Assert.That(columnArrays[j][i], Is.EqualTo(matrix[i, j]));
                }
            }
        }

        [Theory]
        public void CanToRowArrays(TestMatrix testMatrix)
        {
            Matrix<T> matrix = Get(testMatrix);
            var rowArrays = matrix.ToRowArrays();
            Assert.That(rowArrays.Length, Is.EqualTo(matrix.RowCount));
            Assert.That(rowArrays[0].Length, Is.EqualTo(matrix.ColumnCount));
            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    Assert.That(rowArrays[i][j], Is.EqualTo(matrix[i, j]));
                }
            }
        }

        [Theory]
        public void CanToColumnMajorArray(TestMatrix testMatrix)
        {
            Matrix<T> matrix = Get(testMatrix);
            var array = matrix.ToColumnMajorArray();
            Assert.That(array.Length, Is.EqualTo(matrix.RowCount*matrix.ColumnCount));
            for (int i = 0; i < array.Length; i++)
            {
                Assert.That(array[i], Is.EqualTo(matrix[i%matrix.RowCount, i/matrix.RowCount]));
            }
        }

        [Theory]
        public void CanToRowMajorArray(TestMatrix testMatrix)
        {
            Matrix<T> matrix = Get(testMatrix);
            var array = matrix.ToRowMajorArray();
            Assert.That(array.Length, Is.EqualTo(matrix.RowCount*matrix.ColumnCount));
            for (int i = 0; i < array.Length; i++)
            {
                Assert.That(array[i], Is.EqualTo(matrix[i/matrix.ColumnCount, i%matrix.ColumnCount]));
            }
        }

        [Theory]
        public void CanAsArray(TestMatrix testMatrix)
        {
            Matrix<T> matrix = Get(testMatrix);
            var array = matrix.AsArray();
            if (array == null)
            {
                return;
            }

            Assert.That(array, Is.SameAs(matrix.AsArray()));
            Assert.That(array, Is.Not.SameAs(matrix.ToArray()));
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
        public void CanAsColumnArrays(TestMatrix testMatrix)
        {
            Matrix<T> matrix = Get(testMatrix);
            var columnArrays = matrix.AsColumnArrays();
            if (columnArrays == null)
            {
                return;
            }

            Assert.That(columnArrays, Is.SameAs(matrix.AsColumnArrays()));
            Assert.That(columnArrays, Is.Not.SameAs(matrix.ToColumnArrays()));
            Assert.That(columnArrays.Length, Is.EqualTo(matrix.ColumnCount));
            Assert.That(columnArrays[0].Length, Is.EqualTo(matrix.RowCount));
            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    Assert.That(columnArrays[j][i], Is.EqualTo(matrix[i, j]));
                }
            }
        }

        [Theory]
        public void CanAsRowArrays(TestMatrix testMatrix)
        {
            Matrix<T> matrix = Get(testMatrix);
            var rowArrays = matrix.AsRowArrays();
            if (rowArrays == null)
            {
                return;
            }

            Assert.That(rowArrays, Is.SameAs(matrix.AsRowArrays()));
            Assert.That(rowArrays, Is.Not.SameAs(matrix.ToRowArrays()));
            Assert.That(rowArrays.Length, Is.EqualTo(matrix.RowCount));
            Assert.That(rowArrays[0].Length, Is.EqualTo(matrix.ColumnCount));
            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    Assert.That(rowArrays[i][j], Is.EqualTo(matrix[i, j]));
                }
            }
        }

        [Theory]
        public void CanAsColumnMajorArray(TestMatrix testMatrix)
        {
            Matrix<T> matrix = Get(testMatrix);
            var array = matrix.AsColumnMajorArray();
            if (array == null)
            {
                return;
            }

            Assert.That(array, Is.SameAs(matrix.AsColumnMajorArray()));
            Assert.That(array, Is.Not.SameAs(matrix.ToColumnMajorArray()));
            Assert.That(array.Length, Is.EqualTo(matrix.RowCount * matrix.ColumnCount));
            for (int i = 0; i < array.Length; i++)
            {
                Assert.That(array[i], Is.EqualTo(matrix[i % matrix.RowCount, i / matrix.RowCount]));
            }
        }

        [Theory]
        public void CanAsRowMajorArray(TestMatrix testMatrix)
        {
            Matrix<T> matrix = Get(testMatrix);
            var array = matrix.AsRowMajorArray();
            if (array == null)
            {
                return;
            }

            Assert.That(array, Is.SameAs(matrix.AsRowMajorArray()));
            Assert.That(array, Is.Not.SameAs(matrix.ToRowMajorArray()));
            Assert.That(array.Length, Is.EqualTo(matrix.RowCount * matrix.ColumnCount));
            for (int i = 0; i < array.Length; i++)
            {
                Assert.That(array[i], Is.EqualTo(matrix[i / matrix.ColumnCount, i % matrix.ColumnCount]));
            }
        }

        [Theory]
        public void CanCreateSameKind(TestMatrix testMatrix)
        {
            Matrix<T> matrix = Get(testMatrix);
            var empty = Matrix<T>.Build.SameAs(matrix, 5, 6);
            Assert.That(empty, Is.EqualTo(Matrix<T>.Build.Dense(5, 6)));
            Assert.That(empty.Storage.IsDense, Is.EqualTo(matrix.Storage.IsDense));

            Assert.That(() => Matrix<T>.Build.SameAs(matrix, 0, 2), Throws.InstanceOf<ArgumentOutOfRangeException>());
            Assert.That(() => Matrix<T>.Build.SameAs(matrix, 2, 0), Throws.InstanceOf<ArgumentOutOfRangeException>());
            Assert.That(() => Matrix<T>.Build.SameAs(matrix, -1, -1), Throws.InstanceOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void CanCreateDenseFromMultiDimArray()
        {
            T[,] array = Matrix<T>.Build.Random(4, 3, 0).ToArray();
            var matrix = Matrix<T>.Build.DenseOfArray(array);
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
            T[,] array = Matrix<T>.Build.Random(4, 3, 0).ToArray();
            var matrix = Matrix<T>.Build.SparseOfArray(array);
            Assert.That(matrix.GetType().Name, Is.EqualTo("SparseMatrix"));
            Assert.That(matrix.RowCount, Is.EqualTo(4));
            Assert.That(matrix.ColumnCount, Is.EqualTo(3));
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 3; j++)
                    Assert.That(matrix[i, j], Is.EqualTo(array[i, j]));
        }

        [Test]
        public void CanCreateDenseFromColumnMajor()
        {
            var columnMajor = Vector<T>.Build.Random(6, 0).ToArray();
            var matrix = Matrix<T>.Build.DenseOfColumnMajor(2, 3, columnMajor);
            Assert.That(matrix.GetType().Name, Is.EqualTo("DenseMatrix"));
            Assert.That(matrix.RowCount, Is.EqualTo(2));
            Assert.That(matrix.ColumnCount, Is.EqualTo(3));
            for (int i = 0; i < 2; i++)
                for (int j = 0; j < 3; j++)
                    Assert.That(matrix[i, j], Is.EqualTo(columnMajor[j*2 + i]));
        }

        [Test]
        public void CanCreateDenseFromRowMajor()
        {
            var columnMajor = Vector<T>.Build.Random(6, 0).ToArray();
            var matrix = Matrix<T>.Build.DenseOfRowMajor(2, 3, columnMajor);
            Assert.That(matrix.GetType().Name, Is.EqualTo("DenseMatrix"));
            Assert.That(matrix.RowCount, Is.EqualTo(2));
            Assert.That(matrix.ColumnCount, Is.EqualTo(3));
            for (int i = 0; i < 2; i++)
                for (int j = 0; j < 3; j++)
                    Assert.That(matrix[i, j], Is.EqualTo(columnMajor[i * 3 + j]));
        }

        [Test]
        public void CanCreateDenseFromJaggedArray()
        {
            T[][] array =
            {
                Vector<T>.Build.Random(4, 0).ToArray(),
                Vector<T>.Build.Random(4, 1).ToArray(),
                Vector<T>.Build.Random(4, 3).ToArray()
            };
            var matrix = Matrix<T>.Build.DenseOfRows(3, 4, array);
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
            T[][] array =
            {
                Vector<T>.Build.Random(4, 0).ToArray(),
                Vector<T>.Build.Random(4, 1).ToArray(),
                Vector<T>.Build.Random(4, 3).ToArray()
            };
            var matrix = Matrix<T>.Build.SparseOfRows(3, 4, array);
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
                Vector<T>.Build.Random(4, 0),
                Vector<T>.Build.Random(4, 1),
                Vector<T>.Build.Random(4, 3)
            };
            var matrix = Matrix<T>.Build.DenseOfColumns(4, 3, columns);
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
                Vector<T>.Build.Random(4, 0),
                Vector<T>.Build.Random(4, 1),
                Vector<T>.Build.Random(4, 3)
            };
            var matrix = Matrix<T>.Build.SparseOfColumns(4, 3, columns);
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
                Vector<T>.Build.Random(4, 0),
                Vector<T>.Build.Random(4, 1),
                Vector<T>.Build.Random(4, 3)
            };
            var matrix = Matrix<T>.Build.DenseOfRows(3, 4, rows);
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
                Vector<T>.Build.Random(4, 0),
                Vector<T>.Build.Random(4, 1),
                Vector<T>.Build.Random(4, 3)
            };
            var matrix = Matrix<T>.Build.SparseOfRows(3, 4, rows);
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
            var dense = Matrix<T>.Build.Random(2, 3, 0);
            int rowIdxSum = 0, colIdxSum = 0;
            foreach (var value in dense.EnumerateIndexed())
            {
                rowIdxSum += value.Item1;
                colIdxSum += value.Item2;
                Assert.AreEqual(dense[value.Item1, value.Item2], value.Item3);
            }
            Assert.AreEqual(dense.RowCount*(dense.RowCount - 1)/2*dense.ColumnCount, rowIdxSum);
            Assert.AreEqual(dense.ColumnCount*(dense.ColumnCount - 1)/2*dense.RowCount, colIdxSum);
        }
    }
}
