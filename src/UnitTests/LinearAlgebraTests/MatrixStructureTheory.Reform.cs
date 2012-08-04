using System;
using MathNet.Numerics.LinearAlgebra.Generic;
using NUnit.Framework;

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests
{
    partial class MatrixStructureTheory<T>
    {
        [Theory, Timeout(200)]
        public void CanInsertRow(Matrix<T> matrix)
        {
            var row = CreateVector(matrix.ColumnCount, 0);
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
            Assert.That(() => matrix.InsertRow(-1, CreateVector(matrix.ColumnCount)), Throws.InstanceOf<ArgumentOutOfRangeException>());
            Assert.That(() => matrix.InsertRow(matrix.RowCount + 1, CreateVector(matrix.ColumnCount)), Throws.InstanceOf<ArgumentOutOfRangeException>());
            Assert.That(() => matrix.InsertRow(0, CreateVector(matrix.ColumnCount - 1)), Throws.ArgumentException);
            Assert.That(() => matrix.InsertRow(0, CreateVector(matrix.ColumnCount + 1)), Throws.ArgumentException);
        }

        [Theory, Timeout(200)]
        public void CanInsertColumn(Matrix<T> matrix)
        {
            var column = CreateVector(matrix.RowCount, 0);
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
            Assert.That(() => matrix.InsertColumn(-1, CreateVector(matrix.RowCount)), Throws.InstanceOf<ArgumentOutOfRangeException>());
            Assert.That(() => matrix.InsertColumn(matrix.ColumnCount + 1, CreateVector(matrix.RowCount)), Throws.InstanceOf<ArgumentOutOfRangeException>());
            Assert.That(() => matrix.InsertColumn(0, CreateVector(matrix.RowCount - 1)), Throws.ArgumentException);
            Assert.That(() => matrix.InsertColumn(0, CreateVector(matrix.RowCount + 1)), Throws.ArgumentException);
        }
    }
}
