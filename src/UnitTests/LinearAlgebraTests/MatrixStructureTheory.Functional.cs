// <copyright file="MatrixStructureTheory.Functional.cs" company="Math.NET">
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

using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using NUnit.Framework;

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests
{
    partial class MatrixStructureTheory<T>
    {
        [Theory]
        public void CanMap(TestMatrix testMatrix)
        {
            Matrix<T> matrix = Get(testMatrix);
            Matrix<T> a = matrix.Map(x => x, Zeros.AllowSkip);
            Assert.That(a, Is.EqualTo(matrix));
            Assert.That(a.Storage.IsDense, Is.EqualTo(matrix.Storage.IsDense));
            Assert.That(a.Storage.IsFullyMutable, Is.EqualTo(matrix.Storage.IsFullyMutable));

            T one = Matrix<T>.Build.One;
            Assert.That(matrix.Map(x => x, Zeros.Include), Is.EqualTo(matrix));
            Assert.That(matrix.Map(x => one, Zeros.Include), Is.EqualTo(Matrix<T>.Build.Dense(matrix.RowCount, matrix.ColumnCount, one)));

            // Map into existing - we skip zeros, but existing values must still be reset to zero
            var dense = Matrix<T>.Build.Dense(matrix.RowCount, matrix.ColumnCount, one);
            matrix.Map(x => x, dense, Zeros.AllowSkip);
            Assert.That(dense, Is.EqualTo(matrix));

            // Map into self, without using the proper MapInplace method:
            var copy = matrix.Clone();
            copy.Map(x => x, copy, Zeros.AllowSkip);
            Assert.That(copy, Is.EqualTo(matrix));
            if (matrix.Storage.IsFullyMutable)
            {
                copy.Map(x => one, copy, Zeros.AllowSkip);
                Assert.That(copy, Is.EqualTo(Matrix<T>.Build.Dense(matrix.RowCount, matrix.ColumnCount, one)));
            }
        }

        [Theory]
        public void CanMapIndexed(TestMatrix testMatrix)
        {
            Matrix<T> matrix = Get(testMatrix);
            Matrix<T> a = matrix.MapIndexed((i, j, x) =>
            {
                if (i != 0 || j != 1) Assert.That(matrix.At(i, j), Is.EqualTo(x));
                return x;
            }, Zeros.AllowSkip);
            Assert.That(a, Is.EqualTo(matrix));
            Assert.That(a.Storage.IsDense, Is.EqualTo(matrix.Storage.IsDense));
            Assert.That(a.Storage.IsFullyMutable, Is.EqualTo(matrix.Storage.IsFullyMutable));

            T one = Matrix<T>.Build.One;
            var d = matrix.MapIndexed((i, j, x) => i == j ? one : x, Zeros.AllowSkip);
            Assert.That(d.Diagonal().All(x => one.Equals(x) || Zero.Equals(x)), Is.True);
            Assert.That(d.EnumerateIndexed().All(z => (z.Item1 == z.Item2) || (matrix.At(z.Item1, z.Item2).Equals(z.Item3))));

            Assert.That(matrix.MapIndexed((i, j, x) => x, Zeros.Include), Is.EqualTo(matrix));
            Assert.That(matrix.MapIndexed((i, j, x) => one, Zeros.Include), Is.EqualTo(Matrix<T>.Build.Dense(matrix.RowCount, matrix.ColumnCount, one)));

            // Map into existing - we skip zeros, but existing values must still be reset to zero
            var dense = Matrix<T>.Build.Dense(matrix.RowCount, matrix.ColumnCount, one);
            matrix.MapIndexed((i, j, x) => x, dense, Zeros.AllowSkip);
            Assert.That(dense, Is.EqualTo(matrix));

            // Map into self, without using the proper MapInplace method:
            var copy = matrix.Clone();
            copy.MapIndexed((i, j, x) => x, copy, Zeros.AllowSkip);
            Assert.That(copy, Is.EqualTo(matrix));
            if (matrix.Storage.IsFullyMutable)
            {
                copy.MapIndexed((i, j, x) => one, copy, Zeros.AllowSkip);
                Assert.That(copy, Is.EqualTo(Matrix<T>.Build.Dense(matrix.RowCount, matrix.ColumnCount, one)));
            }
        }

        [Theory]
        public void CanMapInplace(TestMatrix testMatrix)
        {
            Matrix<T> matrix = Get(testMatrix);
            var a = matrix.Clone();
            a.MapInplace(x => x, Zeros.AllowSkip);
            Assert.That(a, Is.EqualTo(matrix));

            if (matrix.Storage.IsFullyMutable)
            {
                a.MapInplace(x => x, Zeros.Include);
                Assert.That(a, Is.EqualTo(matrix));

                T one = Matrix<T>.Build.One;
                a.MapInplace(x => one, Zeros.Include);
                Assert.That(a, Is.EqualTo(Matrix<T>.Build.Dense(matrix.RowCount, matrix.ColumnCount, one)));
            }
        }

        [Theory]
        public void CanMapIndexedInplace(TestMatrix testMatrix)
        {
            Matrix<T> matrix = Get(testMatrix);
            var a = matrix.Clone();
            a.MapIndexedInplace((i, j, x) =>
            {
                if (i != 0 || j != 1) Assert.That(matrix.At(i, j), Is.EqualTo(x));
                return x;

            }, Zeros.AllowSkip);
            Assert.That(a, Is.EqualTo(matrix));

            if (matrix.Storage.IsFullyMutable)
            {
                a.MapIndexedInplace((i, j, x) => x, Zeros.Include);
                Assert.That(a, Is.EqualTo(matrix));

                T one = Matrix<T>.Build.One;
                a.MapIndexedInplace((i, j, x) => i == j ? one : x, Zeros.AllowSkip);
                Assert.That(a.Diagonal().All(x => one.Equals(x) || Zero.Equals(x)), Is.True);
                Assert.That(a.EnumerateIndexed().All(z => (z.Item1 == z.Item2) || (matrix.At(z.Item1, z.Item2).Equals(z.Item3))));

                a.MapIndexedInplace((i, j, x) => one, Zeros.Include);
                Assert.That(a, Is.EqualTo(Matrix<T>.Build.Dense(matrix.RowCount, matrix.ColumnCount, one)));
            }
        }

        [Theory]
        public void CanMapSubMatrixToSame(TestMatrix testMatrix)
        {
            Matrix<T> matrix = Get(testMatrix);
            T one = Matrix<T>.Build.One;

            // Full Range - not forced
            Matrix<T> target = Matrix<T>.Build.SameAs(matrix);
            matrix.Storage.MapSubMatrixIndexedTo(target.Storage, (i, j, x) => x, 0, 0, matrix.RowCount, 0, 0, matrix.ColumnCount, Zeros.AllowSkip, ExistingData.Clear);
            Assert.That(target, Is.EqualTo(matrix), "Full Range - not forced");
            matrix.Storage.MapSubMatrixIndexedTo(target.Storage, (i, j, x) => Zero.Equals(x) ? Zero : one, 0, 0, matrix.RowCount, 0, 0, matrix.ColumnCount, Zeros.AllowSkip, ExistingData.Clear);
            Assert.That(target.Enumerate().All(x => Zero.Equals(x) || one.Equals(x)), Is.True);

            if (matrix.Storage.IsFullyMutable)
            {
                // Full Range - forced
                target = Matrix<T>.Build.SameAs(matrix);
                matrix.Storage.MapSubMatrixIndexedTo(target.Storage, (i, j, x) => x, 0, 0, matrix.RowCount, 0, 0, matrix.ColumnCount, Zeros.Include, ExistingData.Clear);
                Assert.That(target, Is.EqualTo(matrix), "Full Range - forced");
                matrix.Storage.MapSubMatrixIndexedTo(target.Storage, (i, j, x) => Zero.Equals(x) ? Zero : one, 0, 0, matrix.RowCount, 0, 0, matrix.ColumnCount, Zeros.Include, ExistingData.Clear);
                Assert.That(target.Enumerate().All(x => Zero.Equals(x) || one.Equals(x)), Is.True);
            }
        }

        [Theory]
        public void CanMapSubMatrixToDense(TestMatrix testMatrix)
        {
            Matrix<T> matrix = Get(testMatrix);
            T one = Matrix<T>.Build.One;

            // Full Range - not forced
            Matrix<T> dense = Matrix<T>.Build.Dense(matrix.RowCount, matrix.ColumnCount, one);
            matrix.Storage.MapSubMatrixIndexedTo(dense.Storage, (i, j, x) =>
            {
                if (i != 0 || j != 1) Assert.That(matrix.At(i, j), Is.EqualTo(x));
                return x;
            }, 0, 0, matrix.RowCount, 0, 0, matrix.ColumnCount, Zeros.AllowSkip, ExistingData.Clear);
            Assert.That(dense, Is.EqualTo(matrix), "Full Range - not forced");

            // Full Range - forced
            dense = Matrix<T>.Build.Dense(matrix.RowCount, matrix.ColumnCount, one);
            matrix.Storage.MapSubMatrixIndexedTo(dense.Storage, (i, j, x) =>
            {
                if (i != 0 || j != 1) Assert.That(matrix.At(i, j), Is.EqualTo(x));
                return x;
            }, 0, 0, matrix.RowCount, 0, 0, matrix.ColumnCount, Zeros.Include, ExistingData.Clear);
            Assert.That(dense, Is.EqualTo(matrix), "Full Range - forced");

            // Sub Range - not forced - all except first column padded into 1-border
            dense = Matrix<T>.Build.Dense(matrix.RowCount + 2, matrix.ColumnCount + 1, one);
            matrix.Storage.MapSubMatrixIndexedTo(dense.Storage, (i, j, x) => x, 0, 1, matrix.RowCount, 1, 1, matrix.ColumnCount - 1, Zeros.AllowSkip, ExistingData.Clear);
            Assert.That(dense.SubMatrix(1, dense.RowCount - 2, 1, dense.ColumnCount - 2),
                Is.EqualTo(matrix.SubMatrix(0, matrix.RowCount, 1, matrix.ColumnCount - 1)), "Sub Range - not forced - range");
            dense.SetSubMatrix(1, 0, matrix.RowCount, 1, 0, matrix.ColumnCount - 1, Matrix<T>.Build.Dense(matrix.RowCount, matrix.ColumnCount - 1, one));
            Assert.That(dense.Enumerate().All(one.Equals), Is.True);

            // Sub Range - forced - all except first row padded into 1-border
            dense = Matrix<T>.Build.Dense(matrix.RowCount + 1, matrix.ColumnCount + 2, one);
            matrix.Storage.MapSubMatrixIndexedTo(dense.Storage, (i, j, x) => x, 1, 1, matrix.RowCount - 1, 0, 1, matrix.ColumnCount, Zeros.Include, ExistingData.Clear);
            Assert.That(dense.SubMatrix(1, dense.RowCount - 2, 1, dense.ColumnCount - 2),
                Is.EqualTo(matrix.SubMatrix(1, matrix.RowCount - 1, 0, matrix.ColumnCount)), "Sub Range - forced - range");
            dense.SetSubMatrix(1, 0, matrix.RowCount - 1, 1, 0, matrix.ColumnCount, Matrix<T>.Build.Dense(matrix.RowCount - 1, matrix.ColumnCount, one));
            Assert.That(dense.Enumerate().All(one.Equals), Is.True);
        }

        [Theory]
        public void CanMapSubMatrixToSparse(TestMatrix testMatrix)
        {
            Matrix<T> matrix = Get(testMatrix);
            T one = Matrix<T>.Build.One;

            // Full Range - filled, not forced
            Matrix<T> sparse = Matrix<T>.Build.Sparse(matrix.RowCount, matrix.ColumnCount, one);
            matrix.Storage.MapSubMatrixIndexedTo(sparse.Storage, (i, j, x) =>
            {
                if (i != 0 || j != 1) Assert.That(matrix.At(i, j), Is.EqualTo(x));
                return x;
            }, 0, 0, matrix.RowCount, 0, 0, matrix.ColumnCount, Zeros.AllowSkip, ExistingData.Clear);
            Assert.That(sparse, Is.EqualTo(matrix), "Full Range - filled, not forced");

            // Full Range - empty, not forced
            sparse = Matrix<T>.Build.Sparse(matrix.RowCount, matrix.ColumnCount);
            matrix.Storage.MapSubMatrixIndexedTo(sparse.Storage, (i, j, x) =>
            {
                if (i != 0 || j != 1) Assert.That(matrix.At(i, j), Is.EqualTo(x));
                return x;
            }, 0, 0, matrix.RowCount, 0, 0, matrix.ColumnCount, Zeros.AllowSkip, ExistingData.Clear);
            Assert.That(sparse, Is.EqualTo(matrix), "Full Range - empty, not forced");

            // Full Range - filled, forced
            sparse = Matrix<T>.Build.Sparse(matrix.RowCount, matrix.ColumnCount, one);
            matrix.Storage.MapSubMatrixIndexedTo(sparse.Storage, (i, j, x) =>
            {
                if (i != 0 || j != 1) Assert.That(matrix.At(i, j), Is.EqualTo(x));
                return x;
            }, 0, 0, matrix.RowCount, 0, 0, matrix.ColumnCount, Zeros.Include, ExistingData.Clear);
            Assert.That(sparse, Is.EqualTo(matrix), "Full Range - filled, forced");

            // Sub Range - filled, not forced - all except first column padded into 1-border
            sparse = Matrix<T>.Build.Sparse(matrix.RowCount + 2, matrix.ColumnCount + 1, one);
            matrix.Storage.MapSubMatrixIndexedTo(sparse.Storage, (i, j, x) => x, 0, 1, matrix.RowCount, 1, 1, matrix.ColumnCount - 1, Zeros.AllowSkip, ExistingData.Clear);
            Assert.That(sparse.SubMatrix(1, sparse.RowCount - 2, 1, sparse.ColumnCount - 2),
                Is.EqualTo(matrix.SubMatrix(0, matrix.RowCount, 1, matrix.ColumnCount - 1)), "Sub Range - filled, not forced - range");
            sparse.SetSubMatrix(1, 0, matrix.RowCount, 1, 0, matrix.ColumnCount - 1, Matrix<T>.Build.Dense(matrix.RowCount, matrix.ColumnCount - 1, one));
            Assert.That(sparse.Enumerate().All(one.Equals), Is.True);

            // Sub Range - filled, forced - all except first row padded into 1-border
            sparse = Matrix<T>.Build.Sparse(matrix.RowCount + 1, matrix.ColumnCount + 2, one);
            matrix.Storage.MapSubMatrixIndexedTo(sparse.Storage, (i, j, x) => x, 1, 1, matrix.RowCount - 1, 0, 1, matrix.ColumnCount, Zeros.Include, ExistingData.Clear);
            Assert.That(sparse.SubMatrix(1, sparse.RowCount - 2, 1, sparse.ColumnCount - 2),
                Is.EqualTo(matrix.SubMatrix(1, matrix.RowCount - 1, 0, matrix.ColumnCount)), "Sub Range - filled, forced - range");
            sparse.SetSubMatrix(1, 0, matrix.RowCount - 1, 1, 0, matrix.ColumnCount, Matrix<T>.Build.Dense(matrix.RowCount - 1, matrix.ColumnCount, one));
            Assert.That(sparse.Enumerate().All(one.Equals), Is.True);
        }

        [Theory]
        public void CanFoldRows(TestMatrix testMatrix)
        {
            Matrix<T> matrix = Get(testMatrix);

            // not forced
            T[] rowSum = matrix.FoldByRow((s, x) => Operator<T>.Add(s, x), Operator<T>.Zero, Zeros.AllowSkip);
            for (int i = 0; i < rowSum.Length; i++)
            {
                Assert.That(rowSum[i], Is.EqualTo(matrix.Row(i).Enumerate().Aggregate((a, b) => Operator<T>.Add(a, b))), "not forced");
            }

            // forced
            rowSum = matrix.FoldByRow((s, x) => Operator<T>.Add(s, x), Operator<T>.Zero, Zeros.Include);
            for (int i = 0; i < rowSum.Length; i++)
            {
                Assert.That(rowSum[i], Is.EqualTo(matrix.Row(i).Enumerate().Aggregate((a, b) => Operator<T>.Add(a, b))), "forced");
            }

            Assert.That(matrix.FoldByRow((s, x) => s + 1.0, 0.0, Zeros.Include),
                Is.EqualTo(Vector<double>.Build.Dense(matrix.RowCount, matrix.ColumnCount)), "forced - full coverage");
        }

        [Theory]
        public void CanFoldColumns(TestMatrix testMatrix)
        {
            Matrix<T> matrix = Get(testMatrix);

            // not forced
            T[] colSum = matrix.FoldByColumn((s, x) => Operator<T>.Add(s, x), Operator<T>.Zero, Zeros.AllowSkip);
            for (int i = 0; i < colSum.Length; i++)
            {
                Assert.That(colSum[i], Is.EqualTo(matrix.Column(i).Enumerate().Aggregate((a, b) => Operator<T>.Add(a, b))), "not forced");
            }

            // forced
            colSum = matrix.FoldByColumn((s, x) => Operator<T>.Add(s, x), Operator<T>.Zero, Zeros.Include);
            for (int i = 0; i < colSum.Length; i++)
            {
                Assert.That(colSum[i], Is.EqualTo(matrix.Column(i).Enumerate().Aggregate((a, b) => Operator<T>.Add(a, b))), "forced");
            }

            Assert.That(matrix.FoldByColumn((s, x) => s + 1.0, 0.0, Zeros.Include),
                Is.EqualTo(Vector<double>.Build.Dense(matrix.ColumnCount, matrix.RowCount)), "forced - full coverage");
        }

        [Theory]
        public void CanFold2(TestMatrix testMatrix)
        {
            Matrix<T> matrix = Get(testMatrix);

            var other = -matrix;
            other.Multiply(Operator.Convert<int, T>(2), other);

            // not forced
            T sum = matrix.Fold2((s, x, y) => Operator.Add(Operator.Add(x, y), s), Operator<T>.Zero, other, Zeros.AllowSkip);
            Assert.That(sum, Is.EqualTo(Operator.Negate(matrix.Enumerate().Aggregate((a, b) => Operator<T>.Add(a, b)))));

            // forced
            T sum2 = matrix.Fold2((s, x, y) => Operator.Add(Operator.Add(x, y), s), Operator<T>.Zero, other, Zeros.Include);
            Assert.That(sum2, Is.EqualTo(Operator.Negate(matrix.Enumerate().Aggregate((a, b) => Operator<T>.Add(a, b)))));
        }
    }
}
