// <copyright file="Builder.cs" company="Math.NET">
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

using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.LinearAlgebra.Solvers;
using MathNet.Numerics.LinearAlgebra.Storage;

// TODO: split up and move to proper folders

namespace MathNet.Numerics.LinearAlgebra.Double
{
    using Solvers;

    internal class MatrixBuilder : MatrixBuilder<double>
    {
        public override double Zero
        {
            get { return 0d; }
        }

        public override double One
        {
            get { return 1d; }
        }

        public override Matrix<double> Dense(DenseColumnMajorMatrixStorage<double> storage)
        {
            return new DenseMatrix(storage);
        }

        public override Matrix<double> Sparse(SparseCompressedRowMatrixStorage<double> storage)
        {
            return new SparseMatrix(storage);
        }

        public override Matrix<double> Diagonal(DiagonalMatrixStorage<double> storage)
        {
            return new DiagonalMatrix(storage);
        }

        public override Matrix<double> Random(int rows, int columns, IContinuousDistribution distribution)
        {
            return Dense(rows, columns, (i, j) => distribution.Sample());
        }

        public override IIterationStopCriterium<double>[] IterativeSolverStopCriteria(int maxIterations = 1000)
        {
            return new IIterationStopCriterium<double>[]
            {
                new FailureStopCriterium<double>(),
                new DivergenceStopCriterium<double>(),
                new IterationCountStopCriterium<double>(maxIterations),
                new ResidualStopCriterium<double>(1e-12)
            };
        }
    }

    internal class VectorBuilder : VectorBuilder<double>
    {
        public override double Zero
        {
            get { return 0d; }
        }

        public override double One
        {
            get { return 1d; }
        }

        public override Vector<double> Dense(DenseVectorStorage<double> storage)
        {
            return new DenseVector(storage);
        }

        public override Vector<double> Sparse(SparseVectorStorage<double> storage)
        {
            return new SparseVector(storage);
        }

        public override Vector<double> Random(int length, IContinuousDistribution distribution)
        {
            return Dense(length, i => distribution.Sample());
        }
    }
}

namespace MathNet.Numerics.LinearAlgebra.Single
{
    using Solvers;

    internal class MatrixBuilder : MatrixBuilder<float>
    {
        public override float Zero
        {
            get { return 0f; }
        }

        public override float One
        {
            get { return 1f; }
        }

        public override Matrix<float> Dense(DenseColumnMajorMatrixStorage<float> storage)
        {
            return new DenseMatrix(storage);
        }

        public override Matrix<float> Sparse(SparseCompressedRowMatrixStorage<float> storage)
        {
            return new SparseMatrix(storage);
        }

        public override Matrix<float> Diagonal(DiagonalMatrixStorage<float> storage)
        {
            return new DiagonalMatrix(storage);
        }

        public override Matrix<float> Random(int rows, int columns, IContinuousDistribution distribution)
        {
            return Dense(rows, columns, (i, j) => (float) distribution.Sample());
        }

        public override IIterationStopCriterium<float>[] IterativeSolverStopCriteria(int maxIterations = 1000)
        {
            return new IIterationStopCriterium<float>[]
            {
                new FailureStopCriterium<float>(),
                new DivergenceStopCriterium<float>(),
                new IterationCountStopCriterium<float>(maxIterations),
                new ResidualStopCriterium<float>(1e-6)
            };
        }
    }

    internal class VectorBuilder : VectorBuilder<float>
    {
        public override float Zero
        {
            get { return 0f; }
        }

        public override float One
        {
            get { return 1f; }
        }

        public override Vector<float> Dense(DenseVectorStorage<float> storage)
        {
            return new DenseVector(storage);
        }

        public override Vector<float> Sparse(SparseVectorStorage<float> storage)
        {
            return new SparseVector(storage);
        }

        public override Vector<float> Random(int length, IContinuousDistribution distribution)
        {
            return Dense(length, i => (float) distribution.Sample());
        }
    }
}

namespace MathNet.Numerics.LinearAlgebra.Complex
{
    using Solvers;

#if NOSYSNUMERICS
    using Complex = Numerics.Complex;
#else
    using Complex = System.Numerics.Complex;
#endif

    internal class MatrixBuilder : MatrixBuilder<Complex>
    {
        public override Complex Zero
        {
            get { return Complex.Zero; }
        }

        public override Complex One
        {
            get { return Complex.One; }
        }

        public override Matrix<Complex> Dense(DenseColumnMajorMatrixStorage<Complex> storage)
        {
            return new DenseMatrix(storage);
        }

        public override Matrix<Complex> Sparse(SparseCompressedRowMatrixStorage<Complex> storage)
        {
            return new SparseMatrix(storage);
        }

        public override Matrix<Complex> Diagonal(DiagonalMatrixStorage<Complex> storage)
        {
            return new DiagonalMatrix(storage);
        }

        public override Matrix<Complex> Random(int rows, int columns, IContinuousDistribution distribution)
        {
            return Dense(rows, columns, (i, j) => new Complex(distribution.Sample(), distribution.Sample()));
        }

        public override IIterationStopCriterium<Complex>[] IterativeSolverStopCriteria(int maxIterations = 1000)
        {
            return new IIterationStopCriterium<Complex>[]
            {
                new FailureStopCriterium<Complex>(),
                new DivergenceStopCriterium<Complex>(),
                new IterationCountStopCriterium<Complex>(maxIterations),
                new ResidualStopCriterium<Complex>(1e-12)
            };
        }
    }

    internal class VectorBuilder : VectorBuilder<Complex>
    {
        public override Complex Zero
        {
            get { return Complex.Zero; }
        }

        public override Complex One
        {
            get { return Complex.One; }
        }

        public override Vector<Complex> Dense(DenseVectorStorage<Complex> storage)
        {
            return new DenseVector(storage);
        }

        public override Vector<Complex> Sparse(SparseVectorStorage<Complex> storage)
        {
            return new SparseVector(storage);
        }

        public override Vector<Complex> Random(int length, IContinuousDistribution distribution)
        {
            return Dense(length, i => new Complex(distribution.Sample(), distribution.Sample()));
        }
    }
}

namespace MathNet.Numerics.LinearAlgebra.Complex32
{
    using Solvers;

    internal class MatrixBuilder : MatrixBuilder<Numerics.Complex32>
    {
        public override Numerics.Complex32 Zero
        {
            get { return Numerics.Complex32.Zero; }
        }

        public override Numerics.Complex32 One
        {
            get { return Numerics.Complex32.One; }
        }

        public override Matrix<Numerics.Complex32> Dense(DenseColumnMajorMatrixStorage<Numerics.Complex32> storage)
        {
            return new DenseMatrix(storage);
        }

        public override Matrix<Numerics.Complex32> Sparse(SparseCompressedRowMatrixStorage<Numerics.Complex32> storage)
        {
            return new SparseMatrix(storage);
        }

        public override Matrix<Numerics.Complex32> Diagonal(DiagonalMatrixStorage<Numerics.Complex32> storage)
        {
            return new DiagonalMatrix(storage);
        }

        public override Matrix<Numerics.Complex32> Random(int rows, int columns, IContinuousDistribution distribution)
        {
            return Dense(rows, columns, (i, j) => new Numerics.Complex32((float) distribution.Sample(), (float) distribution.Sample()));
        }

        public override IIterationStopCriterium<Numerics.Complex32>[] IterativeSolverStopCriteria(int maxIterations = 1000)
        {
            return new IIterationStopCriterium<Numerics.Complex32>[]
            {
                new FailureStopCriterium<Numerics.Complex32>(),
                new DivergenceStopCriterium<Numerics.Complex32>(),
                new IterationCountStopCriterium<Numerics.Complex32>(maxIterations),
                new ResidualStopCriterium<Numerics.Complex32>(1e-6)
            };
        }
    }

    internal class VectorBuilder : VectorBuilder<Numerics.Complex32>
    {
        public override Numerics.Complex32 Zero
        {
            get { return Numerics.Complex32.Zero; }
        }

        public override Numerics.Complex32 One
        {
            get { return Numerics.Complex32.One; }
        }

        public override Vector<Numerics.Complex32> Dense(DenseVectorStorage<Numerics.Complex32> storage)
        {
            return new DenseVector(storage);
        }

        public override Vector<Numerics.Complex32> Sparse(SparseVectorStorage<Numerics.Complex32> storage)
        {
            return new SparseVector(storage);
        }

        public override Vector<Numerics.Complex32> Random(int length, IContinuousDistribution distribution)
        {
            return Dense(length, i => new Numerics.Complex32((float) distribution.Sample(), (float) distribution.Sample()));
        }
    }
}

namespace MathNet.Numerics.LinearAlgebra
{

#if NOSYSNUMERICS
    using Complex64 = Numerics.Complex;
#else
    using Complex64 = System.Numerics.Complex;

#endif

    /// <summary>
    /// Generic linear algebra type builder, for situations where a matrix or vector
    /// must be created in a generic way. Usage of generic builders should not be
    /// required in normal user code.
    /// </summary>
    public abstract class MatrixBuilder<T> where T : struct, IEquatable<T>, IFormattable
    {
        /// <summary>
        /// Gets the value of <c>0.0</c> for type T.
        /// </summary>
        public abstract T Zero { get; }

        /// <summary>
        /// Gets the value of <c>1.0</c> for type T.
        /// </summary>
        public abstract T One { get; }

        /// <summary>
        /// Create a new matrix straight from an initialized matrix storage instance.
        /// If you have an instance of a discrete storage type instead, use their direct methods instead.
        /// </summary>
        public Matrix<T> OfStorage(MatrixStorage<T> storage)
        {
            var dense = storage as DenseColumnMajorMatrixStorage<T>;
            if (dense != null) return Dense(dense);

            var sparse = storage as SparseCompressedRowMatrixStorage<T>;
            if (sparse != null) return Sparse(sparse);

            var diagonal = storage as DiagonalMatrixStorage<T>;
            if (diagonal != null) return Diagonal(diagonal);

            throw new NotSupportedException();
        }

        /// <summary>
        /// Create a new dense matrix with values sampled from the provided random distribution.
        /// </summary>
        public abstract Matrix<T> Random(int rows, int columns, IContinuousDistribution distribution);

        /// <summary>
        /// Create a new dense matrix straight from an initialized matrix storage instance.
        /// The storage is used directly without copying.
        /// Intended for advanced scenarios where you're working directly with
        /// storage for performance or interop reasons.
        /// </summary>
        public abstract Matrix<T> Dense(DenseColumnMajorMatrixStorage<T> storage);

        /// <summary>
        /// Create a new dense matrix with the given number of rows and columns.
        /// All cells of the matrix will be initialized to zero.
        /// Zero-length matrices are not supported.
        /// </summary>
        public Matrix<T> Dense(int rows, int columns)
        {
            return Dense(new DenseColumnMajorMatrixStorage<T>(rows, columns));
        }

        /// <summary>
        /// Create a new dense matrix with the given number of rows and columns directly binding to a raw array.
        /// The array is assumed to be in column-major order (column by column) and is used directly without copying.
        /// Very efficient, but changes to the array and the matrix will affect each other.
        /// </summary>
        /// <seealso href="http://en.wikipedia.org/wiki/Row-major_order"/>
        public Matrix<T> Dense(int rows, int columns, T[] storage)
        {
            return Dense(new DenseColumnMajorMatrixStorage<T>(rows, columns, storage));
        }

        /// <summary>
        /// Create a new dense matrix and initialize each value to the same provided value.
        /// </summary>
        public Matrix<T> Dense(int rows, int columns, T value)
        {
            if (Zero.Equals(value)) return Dense(rows, columns);
            return Dense(DenseColumnMajorMatrixStorage<T>.OfInit(rows, columns, (i, j) => value));
        }

        /// <summary>
        /// Create a new dense matrix and initialize each value using the provided init function.
        /// </summary>
        public Matrix<T> Dense(int rows, int columns, Func<int, int, T> init)
        {
            return Dense(DenseColumnMajorMatrixStorage<T>.OfInit(rows, columns, init));
        }

        /// <summary>
        /// Create a new diagonal dense matrix and initialize each diagonal value to the same provided value.
        /// </summary>
        public Matrix<T> DenseDiagonal(int rows, int columns, T value)
        {
            if (Zero.Equals(value)) return Dense(rows, columns);
            return Dense(DenseColumnMajorMatrixStorage<T>.OfDiagonalInit(rows, columns, i => value));
        }

        /// <summary>
        /// Create a new diagonal dense matrix and initialize each diagonal value to the same provided value.
        /// </summary>
        public Matrix<T> DenseDiagonal(int order, T value)
        {
            if (Zero.Equals(value)) return Dense(order, order);
            return Dense(DenseColumnMajorMatrixStorage<T>.OfDiagonalInit(order, order, i => value));
        }

        /// <summary>
        /// Create a new diagonal dense matrix and initialize each diagonal value using the provided init function.
        /// </summary>
        public Matrix<T> DenseDiagonal(int rows, int columns, Func<int, T> init)
        {
            return Dense(DenseColumnMajorMatrixStorage<T>.OfDiagonalInit(rows, columns, init));
        }

        /// <summary>
        /// Create a new diagonal dense identity matrix with a one-diagonal.
        /// </summary>
        public Matrix<T> DenseIdentity(int rows, int columns)
        {
            return Dense(DenseColumnMajorMatrixStorage<T>.OfDiagonalInit(rows, columns, i => One));
        }

        /// <summary>
        /// Create a new diagonal dense identity matrix with a one-diagonal.
        /// </summary>
        public Matrix<T> DenseIdentity(int order)
        {
            return Dense(DenseColumnMajorMatrixStorage<T>.OfDiagonalInit(order, order, i => One));
        }

        /// <summary>
        /// Create a new dense matrix as a copy of the given other matrix.
        /// This new matrix will be independent from the other matrix.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        public Matrix<T> DenseOfMatrix(Matrix<T> matrix)
        {
            return Dense(DenseColumnMajorMatrixStorage<T>.OfMatrix(matrix.Storage));
        }

        /// <summary>
        /// Create a new dense matrix as a copy of the given two-dimensional array.
        /// This new matrix will be independent from the provided array.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        public Matrix<T> DenseOfArray(T[,] array)
        {
            return Dense(DenseColumnMajorMatrixStorage<T>.OfArray(array));
        }

        /// <summary>
        /// Create a new dense matrix as a copy of the given indexed enumerable.
        /// Keys must be provided at most once, zero is assumed if a key is omitted.
        /// This new matrix will be independent from the enumerable.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        public Matrix<T> DenseOfIndexed(int rows, int columns, IEnumerable<Tuple<int, int, T>> enumerable)
        {
            return Dense(DenseColumnMajorMatrixStorage<T>.OfIndexedEnumerable(rows, columns, enumerable));
        }

        /// <summary>
        /// Create a new dense matrix as a copy of the given enumerable.
        /// The enumerable is assumed to be in column-major order (column by column).
        /// This new matrix will be independent from the enumerable.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        public Matrix<T> DenseOfColumnMajor(int rows, int columns, IEnumerable<T> columnMajor)
        {
            return Dense(DenseColumnMajorMatrixStorage<T>.OfColumnMajorEnumerable(rows, columns, columnMajor));
        }

        /// <summary>
        /// Create a new dense matrix as a copy of the given enumerable of enumerable columns.
        /// Each enumerable in the master enumerable specifies a column.
        /// This new matrix will be independent from the enumerables.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        public Matrix<T> DenseOfColumns(IEnumerable<IEnumerable<T>> data)
        {
            return Dense(DenseColumnMajorMatrixStorage<T>.OfColumnArrays(data.Select(v => (v as T[]) ?? v.ToArray()).ToArray()));
        }

        /// <summary>
        /// Create a new dense matrix as a copy of the given enumerable of enumerable columns.
        /// Each enumerable in the master enumerable specifies a column.
        /// This new matrix will be independent from the enumerables.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        public Matrix<T> DenseOfColumns(int rows, int columns, IEnumerable<IEnumerable<T>> data)
        {
            return Dense(DenseColumnMajorMatrixStorage<T>.OfColumnEnumerables(rows, columns, data));
        }

        /// <summary>
        /// Create a new dense matrix of T as a copy of the given column arrays.
        /// This new matrix will be independent from the arrays.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        public Matrix<T> DenseOfColumnArrays(params T[][] columns)
        {
            return Dense(DenseColumnMajorMatrixStorage<T>.OfColumnArrays(columns));
        }

        /// <summary>
        /// Create a new dense matrix of T as a copy of the given column arrays.
        /// This new matrix will be independent from the arrays.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        public Matrix<T> DenseOfColumnArrays(IEnumerable<T[]> columns)
        {
            return Dense(DenseColumnMajorMatrixStorage<T>.OfColumnArrays((columns as T[][]) ?? columns.ToArray()));
        }

        /// <summary>
        /// Create a new dense matrix as a copy of the given column vectors.
        /// This new matrix will be independent from the vectors.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        public Matrix<T> DenseOfColumnVectors(params Vector<T>[] columns)
        {
            var storage = new VectorStorage<T>[columns.Length];
            for (int i = 0; i < columns.Length; i++)
            {
                storage[i] = columns[i].Storage;
            }
            return Dense(DenseColumnMajorMatrixStorage<T>.OfColumnVectors(storage));
        }

        /// <summary>
        /// Create a new dense matrix as a copy of the given column vectors.
        /// This new matrix will be independent from the vectors.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        public Matrix<T> DenseOfColumnVectors(IEnumerable<Vector<T>> columns)
        {
            return Dense(DenseColumnMajorMatrixStorage<T>.OfColumnVectors(columns.Select(c => c.Storage).ToArray()));
        }

        /// <summary>
        /// Create a new dense matrix as a copy of the given enumerable of enumerable rows.
        /// Each enumerable in the master enumerable specifies a row.
        /// This new matrix will be independent from the enumerables.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        public Matrix<T> DenseOfRows(IEnumerable<IEnumerable<T>> data)
        {
            return Dense(DenseColumnMajorMatrixStorage<T>.OfRowArrays(data.Select(v => (v as T[]) ?? v.ToArray()).ToArray()));
        }

        /// <summary>
        /// Create a new dense matrix as a copy of the given enumerable of enumerable rows.
        /// Each enumerable in the master enumerable specifies a row.
        /// This new matrix will be independent from the enumerables.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        public Matrix<T> DenseOfRows(int rows, int columns, IEnumerable<IEnumerable<T>> data)
        {
            return Dense(DenseColumnMajorMatrixStorage<T>.OfRowEnumerables(rows, columns, data));
        }

        /// <summary>
        /// Create a new dense matrix of T as a copy of the given row arrays.
        /// This new matrix will be independent from the arrays.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        public Matrix<T> DenseOfRowArrays(params T[][] rows)
        {
            return Dense(DenseColumnMajorMatrixStorage<T>.OfRowArrays(rows));
        }

        /// <summary>
        /// Create a new dense matrix of T as a copy of the given row arrays.
        /// This new matrix will be independent from the arrays.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        public Matrix<T> DenseOfRowArrays(IEnumerable<T[]> rows)
        {
            return Dense(DenseColumnMajorMatrixStorage<T>.OfRowArrays((rows as T[][]) ?? rows.ToArray()));
        }

        /// <summary>
        /// Create a new dense matrix as a copy of the given row vectors.
        /// This new matrix will be independent from the vectors.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        public Matrix<T> DenseOfRowVectors(params Vector<T>[] rows)
        {
            var storage = new VectorStorage<T>[rows.Length];
            for (int i = 0; i < rows.Length; i++)
            {
                storage[i] = rows[i].Storage;
            }
            return Dense(DenseColumnMajorMatrixStorage<T>.OfRowVectors(storage));
        }

        /// <summary>
        /// Create a new dense matrix as a copy of the given row vectors.
        /// This new matrix will be independent from the vectors.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        public Matrix<T> DenseOfRowVectors(IEnumerable<Vector<T>> rows)
        {
            return Dense(DenseColumnMajorMatrixStorage<T>.OfRowVectors(rows.Select(r => r.Storage).ToArray()));
        }

        /// <summary>
        /// Create a new dense matrix with the diagonal as a copy of the given vector.
        /// This new matrix will be independent from the vector.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        public Matrix<T> DenseOfDiagonalVector(Vector<T> diagonal)
        {
            var m = Dense(diagonal.Count, diagonal.Count);
            m.SetDiagonal(diagonal);
            return m;
        }

        /// <summary>
        /// Create a new dense matrix with the diagonal as a copy of the given vector.
        /// This new matrix will be independent from the vector.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        public Matrix<T> DenseOfDiagonalVector(int rows, int columns, Vector<T> diagonal)
        {
            var m = Dense(rows, columns);
            m.SetDiagonal(diagonal);
            return m;
        }

        /// <summary>
        /// Create a new dense matrix with the diagonal as a copy of the given array.
        /// This new matrix will be independent from the array.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        public Matrix<T> DenseOfDiagonalArray(T[] diagonal)
        {
            var m = Dense(diagonal.Length, diagonal.Length);
            m.SetDiagonal(diagonal);
            return m;
        }

        /// <summary>
        /// Create a new dense matrix with the diagonal as a copy of the given array.
        /// This new matrix will be independent from the array.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        public Matrix<T> DenseOfDiagonalArray(int rows, int columns, T[] diagonal)
        {
            var m = Dense(rows, columns);
            m.SetDiagonal(diagonal);
            return m;
        }

        /// <summary>
        /// Create a new dense matrix from a 2D array of existing matrices.
        /// The matrices in the array are not required to be dense already.
        /// If the matrices do not align properly, they are placed on the top left
        /// corner of their cell with the remaining fields left zero.
        /// </summary>
        public Matrix<T> DenseOfMatrixArray(Matrix<T>[,] matrices)
        {
            var rowspans = new int[matrices.GetLength(0)];
            var colspans = new int[matrices.GetLength(1)];
            for (int i = 0; i < rowspans.Length; i++)
            {
                for (int j = 0; j < colspans.Length; j++)
                {
                    rowspans[i] = Math.Max(rowspans[i], matrices[i, j].RowCount);
                    colspans[j] = Math.Max(colspans[j], matrices[i, j].ColumnCount);
                }
            }
            var m = Dense(rowspans.Sum(), colspans.Sum());
            int rowoffset = 0;
            for (int i = 0; i < rowspans.Length; i++)
            {
                int coloffset = 0;
                for (int j = 0; j < colspans.Length; j++)
                {
                    m.SetSubMatrix(rowoffset, coloffset, matrices[i,j]);
                    coloffset += colspans[j];
                }
                rowoffset += rowspans[i];
            }
            return m;
        }

        /// <summary>
        /// Create a new sparse matrix straight from an initialized matrix storage instance.
        /// The storage is used directly without copying.
        /// Intended for advanced scenarios where you're working directly with
        /// storage for performance or interop reasons.
        /// </summary>
        public abstract Matrix<T> Sparse(SparseCompressedRowMatrixStorage<T> storage);

        /// <summary>
        /// Create a sparse matrix of T with the given number of rows and columns.
        /// </summary>
        /// <param name="rows">The number of rows.</param>
        /// <param name="columns">The number of columns.</param>
        public Matrix<T> Sparse(int rows, int columns)
        {
            return Sparse(new SparseCompressedRowMatrixStorage<T>(rows, columns));
        }

        /// <summary>
        /// Create a new sparse matrix and initialize each value to the same provided value.
        /// </summary>
        public Matrix<T> Sparse(int rows, int columns, T value)
        {
            if (Zero.Equals(value)) return Sparse(rows, columns);
            return Sparse(SparseCompressedRowMatrixStorage<T>.OfInit(rows, columns, (i, j) => value));
        }

        /// <summary>
        /// Create a new sparse matrix and initialize each value using the provided init function.
        /// </summary>
        public Matrix<T> Sparse(int rows, int columns, Func<int, int, T> init)
        {
            return Sparse(SparseCompressedRowMatrixStorage<T>.OfInit(rows, columns, init));
        }

        /// <summary>
        /// Create a new diagonal sparse matrix and initialize each diagonal value to the same provided value.
        /// </summary>
        public Matrix<T> SparseDiagonal(int rows, int columns, T value)
        {
            if (Zero.Equals(value)) return Sparse(rows, columns);
            return Sparse(SparseCompressedRowMatrixStorage<T>.OfDiagonalInit(rows, columns, i => value));
        }

        /// <summary>
        /// Create a new diagonal sparse matrix and initialize each diagonal value to the same provided value.
        /// </summary>
        public Matrix<T> SparseDiagonal(int order, T value)
        {
            if (Zero.Equals(value)) return Sparse(order, order);
            return Sparse(SparseCompressedRowMatrixStorage<T>.OfDiagonalInit(order, order, i => value));
        }

        /// <summary>
        /// Create a new diagonal sparse matrix and initialize each diagonal value using the provided init function.
        /// </summary>
        public Matrix<T> SparseDiagonal(int rows, int columns, Func<int, T> init)
        {
            return Sparse(SparseCompressedRowMatrixStorage<T>.OfDiagonalInit(rows, columns, init));
        }

        /// <summary>
        /// Create a new diagonal dense identity matrix with a one-diagonal.
        /// </summary>
        public Matrix<T> SparseIdentity(int rows, int columns)
        {
            return Sparse(SparseCompressedRowMatrixStorage<T>.OfDiagonalInit(rows, columns, i => One));
        }

        /// <summary>
        /// Create a new diagonal dense identity matrix with a one-diagonal.
        /// </summary>
        public Matrix<T> SparseIdentity(int order)
        {
            return Sparse(SparseCompressedRowMatrixStorage<T>.OfDiagonalInit(order, order, i => One));
        }

        /// <summary>
        /// Create a new sparse matrix as a copy of the given other matrix.
        /// This new matrix will be independent from the other matrix.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        public Matrix<T> SparseOfMatrix(Matrix<T> matrix)
        {
            return Sparse(SparseCompressedRowMatrixStorage<T>.OfMatrix(matrix.Storage));
        }

        /// <summary>
        /// Create a new sparse matrix as a copy of the given two-dimensional array.
        /// This new matrix will be independent from the provided array.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        public Matrix<T> SparseOfArray(T[,] array)
        {
            return Sparse(SparseCompressedRowMatrixStorage<T>.OfArray(array));
        }

        /// <summary>
        /// Create a new sparse matrix as a copy of the given indexed enumerable.
        /// Keys must be provided at most once, zero is assumed if a key is omitted.
        /// This new matrix will be independent from the enumerable.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        public Matrix<T> SparseOfIndexed(int rows, int columns, IEnumerable<Tuple<int, int, T>> enumerable)
        {
            return Sparse(SparseCompressedRowMatrixStorage<T>.OfIndexedEnumerable(rows, columns, enumerable));
        }

        /// <summary>
        /// Create a new sparse matrix as a copy of the given enumerable.
        /// The enumerable is assumed to be in row-major order (row by row).
        /// This new matrix will be independent from the enumerable.
        /// A new memory block will be allocated for storing the vector.
        /// </summary>
        /// <seealso href="http://en.wikipedia.org/wiki/Row-major_order"/>
        public Matrix<T> SparseOfRowMajor(int rows, int columns, IEnumerable<T> rowMajor)
        {
            return Sparse(SparseCompressedRowMatrixStorage<T>.OfRowMajorEnumerable(rows, columns, rowMajor));
        }

        /// <summary>
        /// Create a new sparse matrix with the given number of rows and columns as a copy of the given array.
        /// The array is assumed to be in column-major order (column by column).
        /// This new matrix will be independent from the provided array.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        /// <seealso href="http://en.wikipedia.org/wiki/Row-major_order"/>
        public Matrix<T> SparseOfColumnMajor(int rows, int columns, IList<T> columnMajor)
        {
            return Sparse(SparseCompressedRowMatrixStorage<T>.OfColumnMajorList(rows, columns, columnMajor));
        }

        /// <summary>
        /// Create a new sparse matrix as a copy of the given enumerable of enumerable columns.
        /// Each enumerable in the master enumerable specifies a column.
        /// This new matrix will be independent from the enumerables.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        public Matrix<T> SparseOfColumns(IEnumerable<IEnumerable<T>> data)
        {
            return Sparse(SparseCompressedRowMatrixStorage<T>.OfColumnArrays(data.Select(v => (v as T[]) ?? v.ToArray()).ToArray()));
        }

        /// <summary>
        /// Create a new sparse matrix as a copy of the given enumerable of enumerable columns.
        /// Each enumerable in the master enumerable specifies a column.
        /// This new matrix will be independent from the enumerables.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        public Matrix<T> SparseOfColumns(int rows, int columns, IEnumerable<IEnumerable<T>> data)
        {
            return Sparse(SparseCompressedRowMatrixStorage<T>.OfColumnEnumerables(rows, columns, data));
        }

        /// <summary>
        /// Create a new sparse matrix as a copy of the given column arrays.
        /// This new matrix will be independent from the arrays.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        public Matrix<T> SparseOfColumnArrays(params T[][] columns)
        {
            return Sparse(SparseCompressedRowMatrixStorage<T>.OfColumnArrays(columns));
        }

        /// <summary>
        /// Create a new sparse matrix as a copy of the given column arrays.
        /// This new matrix will be independent from the arrays.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        public Matrix<T> SparseOfColumnArrays(IEnumerable<T[]> columns)
        {
            return Sparse(SparseCompressedRowMatrixStorage<T>.OfColumnArrays((columns as T[][]) ?? columns.ToArray()));
        }

        /// <summary>
        /// Create a new sparse matrix as a copy of the given column vectors.
        /// This new matrix will be independent from the vectors.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        public Matrix<T> SparseOfColumnVectors(params Vector<T>[] columns)
        {
            var storage = new VectorStorage<T>[columns.Length];
            for (int i = 0; i < columns.Length; i++)
            {
                storage[i] = columns[i].Storage;
            }
            return Sparse(SparseCompressedRowMatrixStorage<T>.OfColumnVectors(storage));
        }

        /// <summary>
        /// Create a new sparse matrix as a copy of the given column vectors.
        /// This new matrix will be independent from the vectors.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        public Matrix<T> SparseOfColumnVectors(IEnumerable<Vector<T>> columns)
        {
            return Sparse(SparseCompressedRowMatrixStorage<T>.OfColumnVectors(columns.Select(c => c.Storage).ToArray()));
        }

        /// <summary>
        /// Create a new sparse matrix as a copy of the given enumerable of enumerable rows.
        /// Each enumerable in the master enumerable specifies a row.
        /// This new matrix will be independent from the enumerables.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        public Matrix<T> SparseOfRows(IEnumerable<IEnumerable<T>> data)
        {
            return Sparse(SparseCompressedRowMatrixStorage<T>.OfRowArrays(data.Select(v => (v as T[]) ?? v.ToArray()).ToArray()));
        }

        /// <summary>
        /// Create a new sparse matrix as a copy of the given enumerable of enumerable rows.
        /// Each enumerable in the master enumerable specifies a row.
        /// This new matrix will be independent from the enumerables.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        public Matrix<T> SparseOfRows(int rows, int columns, IEnumerable<IEnumerable<T>> data)
        {
            return Sparse(SparseCompressedRowMatrixStorage<T>.OfRowEnumerables(rows, columns, data));
        }

        /// <summary>
        /// Create a new sparse matrix as a copy of the given row arrays.
        /// This new matrix will be independent from the arrays.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        public Matrix<T> SparseOfRowArrays(params T[][] rows)
        {
            return Sparse(SparseCompressedRowMatrixStorage<T>.OfRowArrays(rows));
        }

        /// <summary>
        /// Create a new sparse matrix as a copy of the given row arrays.
        /// This new matrix will be independent from the arrays.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        public Matrix<T> SparseOfRowArrays(IEnumerable<T[]> rows)
        {
            return Sparse(SparseCompressedRowMatrixStorage<T>.OfRowArrays((rows as T[][]) ?? rows.ToArray()));
        }

        /// <summary>
        /// Create a new sparse matrix as a copy of the given row vectors.
        /// This new matrix will be independent from the vectors.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        public Matrix<T> SparseOfRowVectors(params Vector<T>[] rows)
        {
            var storage = new VectorStorage<T>[rows.Length];
            for (int i = 0; i < rows.Length; i++)
            {
                storage[i] = rows[i].Storage;
            }
            return Sparse(SparseCompressedRowMatrixStorage<T>.OfRowVectors(storage));
        }

        /// <summary>
        /// Create a new sparse matrix as a copy of the given row vectors.
        /// This new matrix will be independent from the vectors.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        public Matrix<T> SparseOfRowVectors(IEnumerable<Vector<T>> rows)
        {
            return Sparse(SparseCompressedRowMatrixStorage<T>.OfRowVectors(rows.Select(r => r.Storage).ToArray()));
        }

        /// <summary>
        /// Create a new sparse matrix with the diagonal as a copy of the given vector.
        /// This new matrix will be independent from the vector.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        public Matrix<T> SparseOfDiagonalVector(Vector<T> diagonal)
        {
            var m = Sparse(diagonal.Count, diagonal.Count);
            m.SetDiagonal(diagonal);
            return m;
        }

        /// <summary>
        /// Create a new sparse matrix with the diagonal as a copy of the given vector.
        /// This new matrix will be independent from the vector.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        public Matrix<T> SparseOfDiagonalVector(int rows, int columns, Vector<T> diagonal)
        {
            var m = Sparse(rows, columns);
            m.SetDiagonal(diagonal);
            return m;
        }

        /// <summary>
        /// Create a new sparse matrix with the diagonal as a copy of the given array.
        /// This new matrix will be independent from the array.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        public Matrix<T> SparseOfDiagonalArray(T[] diagonal)
        {
            var m = Sparse(diagonal.Length, diagonal.Length);
            m.SetDiagonal(diagonal);
            return m;
        }

        /// <summary>
        /// Create a new sparse matrix with the diagonal as a copy of the given array.
        /// This new matrix will be independent from the array.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        public Matrix<T> SparseOfDiagonalArray(int rows, int columns, T[] diagonal)
        {
            var m = Sparse(rows, columns);
            m.SetDiagonal(diagonal);
            return m;
        }

        /// <summary>
        /// Create a new sparse matrix from a 2D array of existing matrices.
        /// The matrices in the array are not required to be sparse already.
        /// If the matrices do not align properly, they are placed on the top left
        /// corner of their cell with the remaining fields left zero.
        /// </summary>
        public Matrix<T> SparseOfMatrixArray(Matrix<T>[,] matrices)
        {
            var rowspans = new int[matrices.GetLength(0)];
            var colspans = new int[matrices.GetLength(1)];
            for (int i = 0; i < rowspans.Length; i++)
            {
                for (int j = 0; j < colspans.Length; j++)
                {
                    rowspans[i] = Math.Max(rowspans[i], matrices[i, j].RowCount);
                    colspans[j] = Math.Max(colspans[j], matrices[i, j].ColumnCount);
                }
            }
            var m = Sparse(rowspans.Sum(), colspans.Sum());
            int rowoffset = 0;
            for (int i = 0; i < rowspans.Length; i++)
            {
                int coloffset = 0;
                for (int j = 0; j < colspans.Length; j++)
                {
                    m.SetSubMatrix(rowoffset, coloffset, matrices[i, j]);
                    coloffset += colspans[j];
                }
                rowoffset += rowspans[i];
            }
            return m;
        }

        /// <summary>
        /// Create a new diagonal matrix straight from an initialized matrix storage instance.
        /// The storage is used directly without copying.
        /// Intended for advanced scenarios where you're working directly with
        /// storage for performance or interop reasons.
        /// </summary>
        public abstract Matrix<T> Diagonal(DiagonalMatrixStorage<T> storage);

        public abstract IIterationStopCriterium<T>[] IterativeSolverStopCriteria(int maxIterations = 1000);
    }

    /// <summary>
    /// Generic linear algebra type builder, for situations where a matrix or vector
    /// must be created in a generic way. Usage of generic builders should not be
    /// required in normal user code.
    /// </summary>
    public abstract class VectorBuilder<T> where T : struct, IEquatable<T>, IFormattable
    {
        /// <summary>
        /// Gets the value of <c>0.0</c> for type T.
        /// </summary>
        public abstract T Zero { get; }

        /// <summary>
        /// Gets the value of <c>1.0</c> for type T.
        /// </summary>
        public abstract T One { get; }

        /// <summary>
        /// Create a new vector straight from an initialized matrix storage instance.
        /// If you have an instance of a discrete storage type instead, use their direct methods instead.
        /// </summary>
        public Vector<T> OfStorage(VectorStorage<T> storage)
        {
            var dense = storage as DenseVectorStorage<T>;
            if (dense != null) return Dense(dense);

            var sparse = storage as SparseVectorStorage<T>;
            if (sparse != null) return Sparse(sparse);

            throw new NotSupportedException();
        }

        /// <summary>
        /// Create a new dense vector with values sampled from the provided random distribution.
        /// </summary>
        public abstract Vector<T> Random(int length, IContinuousDistribution distribution);

        /// <summary>
        /// Create a new dense vector straight from an initialized vector storage instance.
        /// The storage is used directly without copying.
        /// Intended for advanced scenarios where you're working directly with
        /// storage for performance or interop reasons.
        /// </summary>
        public abstract Vector<T> Dense(DenseVectorStorage<T> storage);

        /// <summary>
        /// Create a dense vector of T with the given size.
        /// </summary>
        /// <param name="size">The size of the vector.</param>
        public Vector<T> Dense(int size)
        {
            return Dense(new DenseVectorStorage<T>(size));
        }

        /// <summary>
        /// Create a dense vector of T that is directly bound to the specified array.
        /// </summary>
        public Vector<T> Dense(T[] array)
        {
            return Dense(new DenseVectorStorage<T>(array.Length, array));
        }

        /// <summary>
        /// Create a new dense vector and initialize each value using the provided value.
        /// </summary>
        public Vector<T> Dense(int length, T value)
        {
            if (Zero.Equals(value)) return Dense(length);
            return Dense(DenseVectorStorage<T>.OfInit(length, i => value));
        }

        /// <summary>
        /// Create a new dense vector and initialize each value using the provided init function.
        /// </summary>
        public Vector<T> Dense(int length, Func<int, T> init)
        {
            return Dense(DenseVectorStorage<T>.OfInit(length, init));
        }

        /// <summary>
        /// Create a new dense vector as a copy of the given other vector.
        /// This new vector will be independent from the other vector.
        /// A new memory block will be allocated for storing the vector.
        /// </summary>
        public Vector<T> DenseOfVector(Vector<T> vector)
        {
            return Dense(DenseVectorStorage<T>.OfVector(vector.Storage));
        }

        /// <summary>
        /// Create a new dense vector as a copy of the given array.
        /// This new vector will be independent from the array.
        /// A new memory block will be allocated for storing the vector.
        /// </summary>
        public Vector<T> DenseOfArray(T[] array)
        {
            return Dense(DenseVectorStorage<T>.OfVector(new DenseVectorStorage<T>(array.Length, array)));
        }

        /// <summary>
        /// Create a new dense vector as a copy of the given enumerable.
        /// This new vector will be independent from the enumerable.
        /// A new memory block will be allocated for storing the vector.
        /// </summary>
        public Vector<T> DenseOfEnumerable(IEnumerable<T> enumerable)
        {
            return Dense(DenseVectorStorage<T>.OfEnumerable(enumerable));
        }

        /// <summary>
        /// Create a new dense vector as a copy of the given indexed enumerable.
        /// Keys must be provided at most once, zero is assumed if a key is omitted.
        /// This new vector will be independent from the enumerable.
        /// A new memory block will be allocated for storing the vector.
        /// </summary>
        public Vector<T> DenseOfIndexed(int length, IEnumerable<Tuple<int, T>> enumerable)
        {
            return Dense(DenseVectorStorage<T>.OfIndexedEnumerable(length, enumerable));
        }

        /// <summary>
        /// Create a new sparse vector straight from an initialized vector storage instance.
        /// The storage is used directly without copying.
        /// Intended for advanced scenarios where you're working directly with
        /// storage for performance or interop reasons.
        /// </summary>
        public abstract Vector<T> Sparse(SparseVectorStorage<T> storage);

        /// <summary>
        /// Create a sparse vector of T with the given size.
        /// </summary>
        /// <param name="size">The size of the vector.</param>
        public Vector<T> Sparse(int size)
        {
            return Sparse(new SparseVectorStorage<T>(size));
        }

        /// <summary>
        /// Create a new sparse vector and initialize each value using the provided value.
        /// </summary>
        public Vector<T> Sparse(int length, T value)
        {
            if (Zero.Equals(value)) return Sparse(length);
            return Sparse(SparseVectorStorage<T>.OfInit(length, i => value));
        }

        /// <summary>
        /// Create a new sparse vector and initialize each value using the provided init function.
        /// </summary>
        public Vector<T> Sparse(int length, Func<int, T> init)
        {
            return Sparse(SparseVectorStorage<T>.OfInit(length, init));
        }

        /// <summary>
        /// Create a new sparse vector as a copy of the given other vector.
        /// This new vector will be independent from the other vector.
        /// A new memory block will be allocated for storing the vector.
        /// </summary>
        public Vector<T> SparseOfVector(Vector<T> vector)
        {
            return Sparse(SparseVectorStorage<T>.OfVector(vector.Storage));
        }

        /// <summary>
        /// Create a new sparse vector as a copy of the given array.
        /// This new vector will be independent from the array.
        /// A new memory block will be allocated for storing the vector.
        /// </summary>
        public Vector<T> SparseOfArray(T[] array)
        {
            return Sparse(SparseVectorStorage<T>.OfEnumerable(array));
        }

        /// <summary>
        /// Create a new sparse vector as a copy of the given enumerable.
        /// This new vector will be independent from the enumerable.
        /// A new memory block will be allocated for storing the vector.
        /// </summary>
        public Vector<T> SparseOfEnumerable(IEnumerable<T> enumerable)
        {
            return Sparse(SparseVectorStorage<T>.OfEnumerable(enumerable));
        }

        /// <summary>
        /// Create a new sparse vector as a copy of the given indexed enumerable.
        /// Keys must be provided at most once, zero is assumed if a key is omitted.
        /// This new vector will be independent from the enumerable.
        /// A new memory block will be allocated for storing the vector.
        /// </summary>
        public Vector<T> SparseOfIndexed(int length, IEnumerable<Tuple<int, T>> enumerable)
        {
            return Sparse(SparseVectorStorage<T>.OfIndexedEnumerable(length, enumerable));
        }
    }

    internal static class BuilderInstance<T> where T : struct, IEquatable<T>, IFormattable
    {
        static Lazy<Tuple<MatrixBuilder<T>, VectorBuilder<T>>> _singleton = new Lazy<Tuple<MatrixBuilder<T>, VectorBuilder<T>>>(Create);

        static Tuple<MatrixBuilder<T>, VectorBuilder<T>> Create()
        {
            if (typeof (T) == typeof (Complex64))
            {
                return new Tuple<MatrixBuilder<T>, VectorBuilder<T>>(
                    (MatrixBuilder<T>) (object) new Complex.MatrixBuilder(),
                    (VectorBuilder<T>) (object) new Complex.VectorBuilder());
            }

            if (typeof (T) == typeof (Numerics.Complex32))
            {
                return new Tuple<MatrixBuilder<T>, VectorBuilder<T>>(
                    (MatrixBuilder<T>) (object) new Complex32.MatrixBuilder(),
                    (VectorBuilder<T>) (object) new Complex32.VectorBuilder());
            }

            if (typeof (T) == typeof (double))
            {
                return new Tuple<MatrixBuilder<T>, VectorBuilder<T>>(
                    (MatrixBuilder<T>) (object) new Double.MatrixBuilder(),
                    (VectorBuilder<T>) (object) new Double.VectorBuilder());
            }

            if (typeof (T) == typeof (float))
            {
                return new Tuple<MatrixBuilder<T>, VectorBuilder<T>>(
                    (MatrixBuilder<T>) (object) new Single.MatrixBuilder(),
                    (VectorBuilder<T>) (object) new Single.VectorBuilder());
            }

            throw new NotSupportedException();
        }

        public static void Register(MatrixBuilder<T> matrixBuilder, VectorBuilder<T> vectorBuilder)
        {
            _singleton = new Lazy<Tuple<MatrixBuilder<T>, VectorBuilder<T>>>(() => new Tuple<MatrixBuilder<T>, VectorBuilder<T>>(matrixBuilder, vectorBuilder));
        }

        public static MatrixBuilder<T> Matrix
        {
            get { return _singleton.Value.Item1; }
        }

        public static VectorBuilder<T> Vector
        {
            get { return _singleton.Value.Item2; }
        }
    }
}
