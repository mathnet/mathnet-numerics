// <copyright file="MatrixStructureTheory.Reform.cs" company="Math.NET">
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
    partial class MatrixStructureTheory<T>
    {
        [Theory]
        public void CanPermuteRows(TestMatrix testMatrix)
        {
            Matrix<T> matrix = Get(testMatrix);
            Assume.That(matrix.Storage.IsFullyMutable);

            var m = matrix.Clone();
            var rnd = new System.Random(0);
            var permutation = new Permutation(Enumerable.Range(0, matrix.RowCount).OrderBy(i => rnd.Next()).ToArray());

            m.PermuteRows(permutation);

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
        public void CanPermuteColumns(TestMatrix testMatrix)
        {
            Matrix<T> matrix = Get(testMatrix);
            Assume.That(matrix.Storage.IsFullyMutable);

            var m = matrix.Clone();
            var rnd = new System.Random(0);
            var permutation = new Permutation(Enumerable.Range(0, matrix.ColumnCount).OrderBy(i => rnd.Next()).ToArray());

            m.PermuteColumns(permutation);

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
        public void CanInsertRow(TestMatrix testMatrix)
        {
            Matrix<T> matrix = Get(testMatrix);
            var row = Vector<T>.Build.Random(matrix.ColumnCount, 0);
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
            Assert.That(() => matrix.InsertRow(0, default(Vector<T>)), Throws.Exception);
            Assert.That(() => matrix.InsertRow(-1, Vector<T>.Build.Dense(matrix.ColumnCount)), Throws.InstanceOf<ArgumentOutOfRangeException>());
            Assert.That(() => matrix.InsertRow(matrix.RowCount + 1, Vector<T>.Build.Dense(matrix.ColumnCount)), Throws.InstanceOf<ArgumentOutOfRangeException>());
            Assert.That(() => matrix.InsertRow(0, Vector<T>.Build.Dense(matrix.ColumnCount - 1)), Throws.ArgumentException);
            Assert.That(() => matrix.InsertRow(0, Vector<T>.Build.Dense(matrix.ColumnCount + 1)), Throws.ArgumentException);
        }

        [Theory]
        public void CanRemoveRow(TestMatrix testMatrix)
        {
            Matrix<T> matrix = Get(testMatrix);
            for (var position = 0; position < matrix.RowCount; position++)
            {
                var result = matrix.RemoveRow(position);
                Assert.That(result.RowCount, Is.EqualTo(matrix.RowCount - 1));
                for (int ir = 0, im = 0; ir < result.RowCount; ir++, im++)
                {
                    if (ir == position)
                    {
                        im++;
                    }
                    for (var j = 0; j < result.ColumnCount; j++)
                    {
                        Assert.That(result[ir, j], Is.EqualTo(matrix[im, j]), "A({0},{1}) for {2}", ir, j, matrix.GetType().FullName);
                    }
                }
            }

            Assert.That(() => matrix.RemoveRow(-1), Throws.InstanceOf<ArgumentOutOfRangeException>());
            Assert.That(() => matrix.RemoveRow(matrix.RowCount + 1), Throws.InstanceOf<ArgumentOutOfRangeException>());
        }

        [Theory]
        public void CanInsertColumn(TestMatrix testMatrix)
        {
            Matrix<T> matrix = Get(testMatrix);
            var column = Vector<T>.Build.Random(matrix.RowCount, 0);
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
            Assert.That(() => matrix.InsertColumn(0, default(Vector<T>)), Throws.Exception);
            Assert.That(() => matrix.InsertColumn(-1, Vector<T>.Build.Dense(matrix.RowCount)), Throws.InstanceOf<ArgumentOutOfRangeException>());
            Assert.That(() => matrix.InsertColumn(matrix.ColumnCount + 1, Vector<T>.Build.Dense(matrix.RowCount)), Throws.InstanceOf<ArgumentOutOfRangeException>());
            Assert.That(() => matrix.InsertColumn(0, Vector<T>.Build.Dense(matrix.RowCount - 1)), Throws.ArgumentException);
            Assert.That(() => matrix.InsertColumn(0, Vector<T>.Build.Dense(matrix.RowCount + 1)), Throws.ArgumentException);
        }

        [Theory]
        public void CanRemoveColumn(TestMatrix testMatrix)
        {
            Matrix<T> matrix = Get(testMatrix);
            for (var position = 0; position < matrix.ColumnCount; position++)
            {
                var result = matrix.RemoveColumn(position);
                Assert.That(result.ColumnCount, Is.EqualTo(matrix.ColumnCount - 1));
                for (int jr = 0, jm = 0; jr < result.ColumnCount; jr++, jm++)
                {
                    if (jr == position)
                    {
                        jm++;
                    }
                    for (var i = 0; i < result.RowCount; i++)
                    {
                        Assert.That(result[i, jr], Is.EqualTo(matrix[i, jm]));
                    }
                }
            }

            // Invalid
            Assert.That(() => matrix.RemoveColumn(-1), Throws.InstanceOf<ArgumentOutOfRangeException>());
            Assert.That(() => matrix.RemoveColumn(matrix.ColumnCount + 1), Throws.InstanceOf<ArgumentOutOfRangeException>());
        }

        [Theory]
        public void CanAppend(TestMatrix leftTestMatrix, TestMatrix rightTestMatrix)
        {
            Matrix<T> left = Get(leftTestMatrix);
            Matrix<T> right = Get(rightTestMatrix);
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
        public void CanAppendIntoResult(TestMatrix leftTestMatrix, TestMatrix rightTestMatrix)
        {
            Matrix<T> left = Get(leftTestMatrix);
            Matrix<T> right = Get(rightTestMatrix);
            Assume.That(left.RowCount, Is.EqualTo(right.RowCount));

            // THEN
            var result = Matrix<T>.Build.Dense(left.RowCount, left.ColumnCount + right.ColumnCount);
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
            Assert.That(() => left.Append(right, Matrix<T>.Build.Dense(left.RowCount + 1, left.ColumnCount + right.ColumnCount)), Throws.ArgumentException);
            Assert.That(() => left.Append(right, Matrix<T>.Build.Dense(left.RowCount - 1, left.ColumnCount + right.ColumnCount)), Throws.ArgumentException);
            Assert.That(() => left.Append(right, Matrix<T>.Build.Dense(left.RowCount, left.ColumnCount + right.ColumnCount + 1)), Throws.ArgumentException);
            Assert.That(() => left.Append(right, Matrix<T>.Build.Dense(left.RowCount, left.ColumnCount + right.ColumnCount - 1)), Throws.ArgumentException);
        }

        [Theory]
        public void CanStack(TestMatrix topTestMatrix, TestMatrix bottomTestMatrix)
        {
            Matrix<T> top = Get(topTestMatrix);
            Matrix<T> bottom = Get(bottomTestMatrix);

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
        public void CanStackIntoResult(TestMatrix topTestMatrix, TestMatrix bottomTestMatrix)
        {
            Matrix<T> top = Get(topTestMatrix);
            Matrix<T> bottom = Get(bottomTestMatrix);
            Assume.That(top.ColumnCount, Is.EqualTo(bottom.ColumnCount));

            // THEN
            var result = Matrix<T>.Build.Dense(top.RowCount + bottom.RowCount, top.ColumnCount);
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
            Assert.That(() => top.Stack(bottom, Matrix<T>.Build.Dense(top.RowCount + bottom.RowCount + 1, top.ColumnCount)), Throws.ArgumentException);
            Assert.That(() => top.Stack(bottom, Matrix<T>.Build.Dense(top.RowCount + bottom.RowCount - 1, top.ColumnCount)), Throws.ArgumentException);
            Assert.That(() => top.Stack(bottom, Matrix<T>.Build.Dense(top.RowCount + bottom.RowCount, top.ColumnCount + 1)), Throws.ArgumentException);
            Assert.That(() => top.Stack(bottom, Matrix<T>.Build.Dense(top.RowCount + bottom.RowCount, top.ColumnCount - 1)), Throws.ArgumentException);
        }

        [Theory]
        public void CanDiagonalStack(TestMatrix leftTestMatrix, TestMatrix rightTestMatrix)
        {
            Matrix<T> left = Get(leftTestMatrix);
            Matrix<T> right = Get(rightTestMatrix);

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
        public void CanDiagonalStackIntoResult(TestMatrix leftTestMatrix, TestMatrix rightTestMatrix)
        {
            Matrix<T> left = Get(leftTestMatrix);
            Matrix<T> right = Get(rightTestMatrix);

            var result = Matrix<T>.Build.Dense(left.RowCount + right.RowCount, left.ColumnCount + right.ColumnCount);
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
            Assert.That(() => left.DiagonalStack(right, Matrix<T>.Build.Dense(left.RowCount + right.RowCount + 1, left.ColumnCount + right.ColumnCount)), Throws.ArgumentException);
            Assert.That(() => left.DiagonalStack(right, Matrix<T>.Build.Dense(left.RowCount + right.RowCount - 1, left.ColumnCount + right.ColumnCount)), Throws.ArgumentException);
            Assert.That(() => left.DiagonalStack(right, Matrix<T>.Build.Dense(left.RowCount + right.RowCount, left.ColumnCount + right.ColumnCount + 1)), Throws.ArgumentException);
            Assert.That(() => left.DiagonalStack(right, Matrix<T>.Build.Dense(left.RowCount + right.RowCount, left.ColumnCount + right.ColumnCount - 1)), Throws.ArgumentException);
        }
    }
}
