// <copyright file="MatrixStructureTheory.Access.cs" company="Math.NET">
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

using MathNet.Numerics.LinearAlgebra;
using NUnit.Framework;
using System;

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests
{
    partial class MatrixStructureTheory<T>
    {
        [Theory]
        public void CanGetFieldsByIndex(TestMatrix testMatrix)
        {
            Matrix<T> matrix = Get(testMatrix);

            Assert.That(() => matrix[0, 0], Throws.Nothing);
            Assert.That(() => matrix[0, matrix.ColumnCount - 1], Throws.Nothing);
            Assert.That(() => matrix[matrix.RowCount - 1, 0], Throws.Nothing);

            Assert.That(() => matrix[-1, 1], Throws.InstanceOf<ArgumentOutOfRangeException>());
            Assert.That(() => matrix[1, -1], Throws.InstanceOf<ArgumentOutOfRangeException>());
            Assert.That(() => matrix[0, matrix.ColumnCount], Throws.InstanceOf<ArgumentOutOfRangeException>());
        }

        [Theory]
        public void CanGetRow(TestMatrix testMatrix)
        {
            Matrix<T> matrix = Get(testMatrix);

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

        [Theory]
        public void CanGetRowIntoResult(TestMatrix testMatrix)
        {
            Matrix<T> matrix = Get(testMatrix);

            var row = Vector<T>.Build.Dense(matrix.ColumnCount);
            matrix.Row(0, row);

            for (var j = 0; j < matrix.ColumnCount; j++)
            {
                Assert.AreEqual(matrix[0, j], row[j]);
            }

            Assert.That(() => matrix.Row(0, null), Throws.InstanceOf<ArgumentNullException>());
            Assert.That(() => matrix.Row(-1, row), Throws.InstanceOf<ArgumentOutOfRangeException>());
            Assert.That(() => matrix.Row(matrix.RowCount, row), Throws.InstanceOf<ArgumentOutOfRangeException>());
        }

        [Theory]
        public void CanGetRowWithRange(TestMatrix testMatrix)
        {
            Matrix<T> matrix = Get(testMatrix);

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

        [Theory]
        public void CanGetRowWithRangeIntoResult(TestMatrix testMatrix)
        {
            Matrix<T> matrix = Get(testMatrix);

            var row = Vector<T>.Build.Dense(matrix.ColumnCount - 1);
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

        [Theory]
        public void CanGetColumn(TestMatrix testMatrix)
        {
            Matrix<T> matrix = Get(testMatrix);

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

        [Theory]
        public void CanGetColumnIntoResult(TestMatrix testMatrix)
        {
            Matrix<T> matrix = Get(testMatrix);

            var col = Vector<T>.Build.Dense(matrix.RowCount);
            matrix.Column(0, col);

            for (var i = 0; i < matrix.RowCount; i++)
            {
                Assert.AreEqual(matrix[i, 0], col[i]);
            }

            Assert.That(() => matrix.Column(0, null), Throws.InstanceOf<ArgumentNullException>());
            Assert.That(() => matrix.Column(-1, col), Throws.InstanceOf<ArgumentOutOfRangeException>());
            Assert.That(() => matrix.Column(matrix.ColumnCount, col), Throws.InstanceOf<ArgumentOutOfRangeException>());
        }

        [Theory]
        public void CanGetColumnWithRange(TestMatrix testMatrix)
        {
            Matrix<T> matrix = Get(testMatrix);

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

        [Theory]
        public void CanGetColumnWithRangeIntoResult(TestMatrix testMatrix)
        {
            Matrix<T> matrix = Get(testMatrix);

            var col = Vector<T>.Build.Dense(matrix.RowCount - 1);
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

        [Theory]
        public void CanSetRow(TestMatrix testMatrix)
        {
            Matrix<T> matrix = Get(testMatrix);

            // First Row
            var m = matrix.Clone();
            var v = CreateVectorFor(m, matrix.ColumnCount);
            m.SetRow(0, v);
            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    Assert.That(m[i, j], Is.EqualTo(i == 0 ? v[j] : matrix[i, j]));
                }
            }

            // Last Row
            m = matrix.Clone();
            v = CreateVectorFor(m, matrix.ColumnCount);
            m.SetRow(matrix.RowCount - 1, v);
            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    Assert.That(m[i, j], Is.EqualTo(i == matrix.RowCount - 1 ? v[j] : matrix[i, j]));
                }
            }

            // Invalid Rows
            Assert.That(() => matrix.SetRow(0, default(Vector<T>)), Throws.InstanceOf<ArgumentNullException>());
            Assert.That(() => matrix.SetRow(-1, Vector<T>.Build.Dense(matrix.ColumnCount)), Throws.InstanceOf<ArgumentOutOfRangeException>());
            Assert.That(() => matrix.SetRow(matrix.RowCount, Vector<T>.Build.Dense(matrix.ColumnCount)), Throws.InstanceOf<ArgumentOutOfRangeException>());
            Assert.That(() => matrix.SetRow(0, Vector<T>.Build.Dense(matrix.ColumnCount - 1)), Throws.ArgumentException);
            Assert.That(() => matrix.SetRow(0, Vector<T>.Build.Dense(matrix.ColumnCount + 1)), Throws.ArgumentException);
        }

        [Theory]
        public void CanSetRowArray(TestMatrix testMatrix)
        {
            Matrix<T> matrix = Get(testMatrix);

            // First Row
            var m = matrix.Clone();
            m.SetRow(0, Vector<T>.Build.Dense(matrix.ColumnCount).ToArray());
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

        [Theory]
        public void CanSetColumn(TestMatrix testMatrix)
        {
            Matrix<T> matrix = Get(testMatrix);

            // First Column
            var m = matrix.Clone();
            var v = CreateVectorFor(m, matrix.RowCount);
            m.SetColumn(0, v);
            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    Assert.That(m[i, j], Is.EqualTo(j == 0 ? v[i] : matrix[i, j]));
                }
            }

            // Last Column
            m = matrix.Clone();
            v = CreateVectorFor(m, matrix.RowCount);
            m.SetColumn(matrix.ColumnCount - 1, v);
            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    Assert.That(m[i, j], Is.EqualTo(j == matrix.ColumnCount - 1 ? v[i] : matrix[i, j]));
                }
            }

            // Invalid Rows
            Assert.That(() => matrix.SetColumn(0, default(Vector<T>)), Throws.InstanceOf<ArgumentNullException>());
            Assert.That(() => matrix.SetColumn(-1, Vector<T>.Build.Dense(matrix.RowCount)), Throws.InstanceOf<ArgumentOutOfRangeException>());
            Assert.That(() => matrix.SetColumn(matrix.ColumnCount, Vector<T>.Build.Dense(matrix.RowCount)), Throws.InstanceOf<ArgumentOutOfRangeException>());
            Assert.That(() => matrix.SetColumn(0, Vector<T>.Build.Dense(matrix.RowCount - 1)), Throws.ArgumentException);
            Assert.That(() => matrix.SetColumn(0, Vector<T>.Build.Dense(matrix.RowCount + 1)), Throws.ArgumentException);
        }

        [Theory]
        public void CanSetColumnArray(TestMatrix testMatrix)
        {
            Matrix<T> matrix = Get(testMatrix);

            // First Column
            var m = matrix.Clone();
            m.SetColumn(0, Vector<T>.Build.Dense(matrix.RowCount).ToArray());
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

        [Theory]
        public void CanGetUpperTriangle(TestMatrix testMatrix)
        {
            Matrix<T> matrix = Get(testMatrix);

            var upper = matrix.UpperTriangle();
            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    Assert.That(upper[i, j], Is.EqualTo(i <= j ? matrix[i, j] : Zero));
                }
            }
        }

        [Theory]
        public void CanGetUpperTriangleIntoResult(TestMatrix testMatrix)
        {
            Matrix<T> matrix = Get(testMatrix);

            var dense = Matrix<T>.Build.Dense(matrix.RowCount, matrix.ColumnCount);
            matrix.UpperTriangle(dense);
            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    Assert.That(dense[i, j], Is.EqualTo(i <= j ? matrix[i, j] : Zero));
                }
            }

            var sparse = Matrix<T>.Build.Sparse(matrix.RowCount, matrix.ColumnCount);
            matrix.UpperTriangle(sparse);
            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    Assert.That(sparse[i, j], Is.EqualTo(i <= j ? matrix[i, j] : Zero));
                }
            }

            Assert.That(() => matrix.UpperTriangle(null), Throws.Exception);
            Assert.That(() => matrix.UpperTriangle(Matrix<T>.Build.Sparse(matrix.RowCount + 1, matrix.ColumnCount)), Throws.ArgumentException);
            Assert.That(() => matrix.UpperTriangle(Matrix<T>.Build.Dense(matrix.RowCount, matrix.ColumnCount + 1)), Throws.ArgumentException);
        }

        [Theory]
        public void CanGetLowerTriangle(TestMatrix testMatrix)
        {
            Matrix<T> matrix = Get(testMatrix);

            var upper = matrix.LowerTriangle();
            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    Assert.That(upper[i, j], Is.EqualTo(i >= j ? matrix[i, j] : Zero));
                }
            }
        }

        [Theory]
        public void CanGetLowerTriangleIntoResult(TestMatrix testMatrix)
        {
            Matrix<T> matrix = Get(testMatrix);

            var dense = Matrix<T>.Build.Dense(matrix.RowCount, matrix.ColumnCount);
            matrix.LowerTriangle(dense);
            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    Assert.That(dense[i, j], Is.EqualTo(i >= j ? matrix[i, j] : Zero));
                }
            }

            var sparse = Matrix<T>.Build.Sparse(matrix.RowCount, matrix.ColumnCount);
            matrix.LowerTriangle(sparse);
            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    Assert.That(sparse[i, j], Is.EqualTo(i >= j ? matrix[i, j] : Zero));
                }
            }

            Assert.That(() => matrix.LowerTriangle(null), Throws.Exception);
            Assert.That(() => matrix.LowerTriangle(Matrix<T>.Build.Sparse(matrix.RowCount + 1, matrix.ColumnCount)), Throws.ArgumentException);
            Assert.That(() => matrix.LowerTriangle(Matrix<T>.Build.Dense(matrix.RowCount, matrix.ColumnCount + 1)), Throws.ArgumentException);
        }

        [Theory]
        public void CanGetStrictlyUpperTriangle(TestMatrix testMatrix)
        {
            Matrix<T> matrix = Get(testMatrix);

            var upper = matrix.StrictlyUpperTriangle();
            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    Assert.That(upper[i, j], Is.EqualTo(i < j ? matrix[i, j] : Zero));
                }
            }
        }

        [Theory]
        public void CanGetStrictlyUpperTriangleIntoResult(TestMatrix testMatrix)
        {
            Matrix<T> matrix = Get(testMatrix);

            var dense = Matrix<T>.Build.Dense(matrix.RowCount, matrix.ColumnCount);
            matrix.StrictlyUpperTriangle(dense);
            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    Assert.That(dense[i, j], Is.EqualTo(i < j ? matrix[i, j] : Zero));
                }
            }

            var sparse = Matrix<T>.Build.Sparse(matrix.RowCount, matrix.ColumnCount);
            matrix.StrictlyUpperTriangle(sparse);
            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    Assert.That(sparse[i, j], Is.EqualTo(i < j ? matrix[i, j] : Zero));
                }
            }

            Assert.That(() => matrix.StrictlyUpperTriangle(null), Throws.Exception);
            Assert.That(() => matrix.StrictlyUpperTriangle(Matrix<T>.Build.Sparse(matrix.RowCount + 1, matrix.ColumnCount)), Throws.ArgumentException);
            Assert.That(() => matrix.StrictlyUpperTriangle(Matrix<T>.Build.Dense(matrix.RowCount, matrix.ColumnCount + 1)), Throws.ArgumentException);
        }

        [Theory]
        public void CanGetStrictlyLowerTriangle(TestMatrix testMatrix)
        {
            Matrix<T> matrix = Get(testMatrix);

            var upper = matrix.StrictlyLowerTriangle();
            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    Assert.That(upper[i, j], Is.EqualTo(i > j ? matrix[i, j] : Zero));
                }
            }
        }

        [Theory]
        public void CanGetStrictlyLowerTriangleIntoResult(TestMatrix testMatrix)
        {
            Matrix<T> matrix = Get(testMatrix);

            var dense = Matrix<T>.Build.Dense(matrix.RowCount, matrix.ColumnCount);
            matrix.StrictlyLowerTriangle(dense);
            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    Assert.That(dense[i, j], Is.EqualTo(i > j ? matrix[i, j] : Zero));
                }
            }

            var sparse = Matrix<T>.Build.Sparse(matrix.RowCount, matrix.ColumnCount);
            matrix.StrictlyLowerTriangle(sparse);
            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    Assert.That(sparse[i, j], Is.EqualTo(i > j ? matrix[i, j] : Zero));
                }
            }

            Assert.That(() => matrix.StrictlyLowerTriangle(null), Throws.Exception);
            Assert.That(() => matrix.StrictlyLowerTriangle(Matrix<T>.Build.Sparse(matrix.RowCount + 1, matrix.ColumnCount)), Throws.ArgumentException);
            Assert.That(() => matrix.StrictlyLowerTriangle(Matrix<T>.Build.Dense(matrix.RowCount, matrix.ColumnCount + 1)), Throws.ArgumentException);
        }

        [Theory]
        public void CanGetDiagonal(TestMatrix testMatrix)
        {
            Matrix<T> matrix = Get(testMatrix);

            var diag = matrix.Diagonal();
            Assert.That(diag.Count, Is.EqualTo(Math.Min(matrix.RowCount, matrix.ColumnCount)));
            for (var i = 0; i < Math.Min(matrix.RowCount, matrix.ColumnCount); i++)
            {
                Assert.That(diag[i], Is.EqualTo(matrix[i, i]));
            }
        }

        [Theory]
        public void CanSetDiagonal(TestMatrix testMatrix)
        {
            Matrix<T> matrix = Get(testMatrix);

            var m = matrix.Clone();
            var v = CreateVectorFor(m, Math.Min(matrix.RowCount, matrix.ColumnCount));
            m.SetDiagonal(v);
            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    Assert.That(m[i, j], Is.EqualTo(i == j ? v[i] : matrix[i, j]));
                }
            }

            // Invalid
            Assert.That(() => matrix.SetDiagonal(default(Vector<T>)), Throws.InstanceOf<ArgumentNullException>());
            Assert.That(() => matrix.SetDiagonal(Vector<T>.Build.Dense(Math.Min(matrix.RowCount, matrix.ColumnCount) - 1)), Throws.ArgumentException);
            Assert.That(() => matrix.SetDiagonal(Vector<T>.Build.Dense(Math.Min(matrix.RowCount, matrix.ColumnCount) + 1)), Throws.ArgumentException);
        }

        [Theory]
        public void CanSetDiagonalArray(TestMatrix testMatrix)
        {
            Matrix<T> matrix = Get(testMatrix);

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
            Assert.That(() => matrix.SetDiagonal(default(T[])), Throws.Exception);
            Assert.That(() => matrix.SetDiagonal(new T[Math.Min(matrix.RowCount, matrix.ColumnCount) - 1]), Throws.ArgumentException);
            Assert.That(() => matrix.SetDiagonal(new T[Math.Min(matrix.RowCount, matrix.ColumnCount) + 1]), Throws.ArgumentException);
        }

        [Theory]
        public void CanGetSubmatrix(TestMatrix testMatrix)
        {
            Matrix<T> matrix = Get(testMatrix);

            // Top Left Corner 2x2
            var topleft = matrix.SubMatrix(0, 2, 0, 2);
            Assert.That(topleft.RowCount, Is.EqualTo(2));
            Assert.That(topleft.ColumnCount, Is.EqualTo(2));
            for (var i = 0; i < 2; i++)
            {
                for (var j = 0; j < 2; j++)
                {
                    Assert.That(topleft[i,j], Is.EqualTo(matrix[i,j]));
                }
            }

            // Bottom Right Cornet 1x2
            var bottomright = matrix.SubMatrix(matrix.RowCount - 1, 1, matrix.ColumnCount - 2, 2);
            Assert.That(bottomright.RowCount, Is.EqualTo(1));
            Assert.That(bottomright.ColumnCount, Is.EqualTo(2));
            for (var i = 0; i < 1; i++)
            {
                for (var j = 0; j < 2; j++)
                {
                    Assert.That(bottomright[i, j], Is.EqualTo(matrix[matrix.RowCount - 1 + i, matrix.ColumnCount - 2 + j]));
                }
            }

            // Left Field 1x1
            var field = matrix.SubMatrix(1, 1, 0, 1);
            Assert.That(field.RowCount, Is.EqualTo(1));
            Assert.That(field.ColumnCount, Is.EqualTo(1));
            Assert.That(field[0, 0], Is.EqualTo(matrix[1, 0]), "{0}->{1}", matrix.GetType().FullName, field.GetType().FullName);

            // Invalid
            Assert.That(() => matrix.SubMatrix(-1, 1, 0, 1), Throws.InstanceOf<ArgumentOutOfRangeException>());
            Assert.That(() => matrix.SubMatrix(matrix.RowCount, 1, 0, 1), Throws.InstanceOf<ArgumentOutOfRangeException>());
            Assert.That(() => matrix.SubMatrix(0, 0, 0, 1), Throws.InstanceOf<ArgumentOutOfRangeException>());
            Assert.That(() => matrix.SubMatrix(0, 1, -1, 1), Throws.InstanceOf<ArgumentOutOfRangeException>());
            Assert.That(() => matrix.SubMatrix(0, 1, matrix.ColumnCount, 1), Throws.InstanceOf<ArgumentOutOfRangeException>());
            Assert.That(() => matrix.SubMatrix(0, 1, 0, 0), Throws.InstanceOf<ArgumentOutOfRangeException>());
        }

        [Theory]
        public void CanSetSubmatrix(TestMatrix testMatrix)
        {
            Matrix<T> matrix = Get(testMatrix);

            // Top Left Corner 2x2
            var topleft = CreateDenseFor(matrix, 2, 2);
            var m = matrix.Clone();
            m.SetSubMatrix(0, 2, 0, 2, topleft);
            for (var i = 0; i < m.RowCount; i++)
            {
                for (var j = 0; j < m.ColumnCount; j++)
                {
                    Assert.That(m[i, j], Is.EqualTo(i < 2 && j < 2 ? topleft[i, j] : matrix[i, j]));
                }
            }

            // Bottom Right Cornet 1x2
            var bottomright = CreateDenseFor(matrix, 1, 2);
            m = matrix.Clone();
            m.SetSubMatrix(matrix.RowCount - 1, 1, matrix.ColumnCount - 2, 2, bottomright);
            for (var i = 0; i < m.RowCount; i++)
            {
                for (var j = 0; j < m.ColumnCount; j++)
                {
                    Assert.That(m[i, j], Is.EqualTo(i >= matrix.RowCount - 1 && j >= matrix.ColumnCount - 2 ? bottomright[i - matrix.RowCount + 1, j - matrix.ColumnCount + 2] : matrix[i, j]));
                }
            }

            // Invalid
            m = matrix.Clone();
            Assert.That(() => m.SetSubMatrix(0, 1, 0, 1, default(Matrix<T>)), Throws.InstanceOf<NullReferenceException>());
            Assert.That(() => m.SetSubMatrix(-1, 1, 0, 1, Matrix<T>.Build.Dense(1, 1)), Throws.InstanceOf<ArgumentOutOfRangeException>());
            Assert.That(() => m.SetSubMatrix(matrix.RowCount, 1, 0, 1, Matrix<T>.Build.Dense(1, 1)), Throws.InstanceOf<ArgumentOutOfRangeException>());
            Assert.That(() => m.SetSubMatrix(0, 1, -1, 1, Matrix<T>.Build.Dense(1, 1)), Throws.InstanceOf<ArgumentOutOfRangeException>());
            Assert.That(() => m.SetSubMatrix(0, 1, matrix.ColumnCount, 1, Matrix<T>.Build.Dense(1, 1)), Throws.InstanceOf<ArgumentOutOfRangeException>());

            // Usually invalid, but not for SetSubMatrix (since size is explicitly provided)
            Assert.That(() => m.SetSubMatrix(0, 1, 0, 1, Matrix<T>.Build.Dense(1, 2)), Throws.Nothing);
            Assert.That(() => m.SetSubMatrix(0, 1, 0, 1, Matrix<T>.Build.Dense(2, 1)), Throws.Nothing);
            Assert.That(() => m.SetSubMatrix(0, 0, 0, 1, Matrix<T>.Build.Dense(1, 1)), Throws.Nothing);
            Assert.That(() => m.SetSubMatrix(0, 1, 0, 0, Matrix<T>.Build.Dense(1, 1)), Throws.Nothing);
        }
    }
}
