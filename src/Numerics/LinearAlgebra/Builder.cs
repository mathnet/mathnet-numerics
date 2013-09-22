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

namespace MathNet.Numerics.LinearAlgebra.Double
{
    using Solvers;

    internal class Builder : Builder<double>
    {
        public override double Zero
        {
            get { return 0d; }
        }

        public override double One
        {
            get { return 1d; }
        }

        public override Matrix<double> DenseMatrix(DenseColumnMajorMatrixStorage<double> storage)
        {
            return new DenseMatrix(storage);
        }

        public override Matrix<double> SparseMatrix(SparseCompressedRowMatrixStorage<double> storage)
        {
            return new SparseMatrix(storage);
        }

        public override Matrix<double> DiagonalMatrix(DiagonalMatrixStorage<double> storage)
        {
            return new DiagonalMatrix(storage);
        }

        public override Vector<double> DenseVector(DenseVectorStorage<double> storage)
        {
            return new DenseVector(storage);
        }

        public override Vector<double> SparseVector(SparseVectorStorage<double> storage)
        {
            return new SparseVector(storage);
        }

        public override Matrix<double> DenseMatrixRandom(int rows, int columns, IContinuousDistribution distribution)
        {
            return Double.DenseMatrix.CreateRandom(rows, columns, distribution);
        }

        public override Vector<double> DenseVectorRandom(int length, IContinuousDistribution distribution)
        {
            return new DenseVector(DenseVectorStorage<double>.OfInit(length, i => distribution.Sample()));
        }

        public override IIterationStopCriterium<double>[] IterativeSolverStopCriteria(int maxIterations = 1000)
        {
            return new IIterationStopCriterium<double>[]
            {
                new FailureStopCriterium(),
                new DivergenceStopCriterium(),
                new IterationCountStopCriterium<double>(maxIterations),
                new ResidualStopCriterium()
            };
        }
    }
}

namespace MathNet.Numerics.LinearAlgebra.Single
{
    using Solvers;

    internal class Builder : Builder<float>
    {
        public override float Zero
        {
            get { return 0f; }
        }

        public override float One
        {
            get { return 1f; }
        }

        public override Matrix<float> DenseMatrix(DenseColumnMajorMatrixStorage<float> storage)
        {
            return new DenseMatrix(storage);
        }

        public override Matrix<float> SparseMatrix(SparseCompressedRowMatrixStorage<float> storage)
        {
            return new SparseMatrix(storage);
        }

        public override Matrix<float> DiagonalMatrix(DiagonalMatrixStorage<float> storage)
        {
            return new DiagonalMatrix(storage);
        }

        public override Vector<float> DenseVector(DenseVectorStorage<float> storage)
        {
            return new DenseVector(storage);
        }

        public override Vector<float> SparseVector(SparseVectorStorage<float> storage)
        {
            return new SparseVector(storage);
        }

        public override Matrix<float> DenseMatrixRandom(int rows, int columns, IContinuousDistribution distribution)
        {
            return Single.DenseMatrix.CreateRandom(rows, columns, distribution);
        }

        public override Vector<float> DenseVectorRandom(int length, IContinuousDistribution distribution)
        {
            return new DenseVector(DenseVectorStorage<float>.OfInit(length, i => (float)distribution.Sample()));
        }

        public override IIterationStopCriterium<float>[] IterativeSolverStopCriteria(int maxIterations = 1000)
        {
            return new IIterationStopCriterium<float>[]
            {
                new FailureStopCriterium(),
                new DivergenceStopCriterium(),
                new IterationCountStopCriterium<float>(maxIterations),
                new ResidualStopCriterium()
            };
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

    internal class Builder : Builder<Complex>
    {
        public override Complex Zero
        {
            get { return Complex.Zero; }
        }

        public override Complex One
        {
            get { return Complex.One; }
        }

        public override Matrix<Complex> DenseMatrix(DenseColumnMajorMatrixStorage<Complex> storage)
        {
            return new DenseMatrix(storage);
        }

        public override Matrix<Complex> SparseMatrix(SparseCompressedRowMatrixStorage<Complex> storage)
        {
            return new SparseMatrix(storage);
        }

        public override Matrix<Complex> DiagonalMatrix(DiagonalMatrixStorage<Complex> storage)
        {
            return new DiagonalMatrix(storage);
        }

        public override Vector<Complex> DenseVector(DenseVectorStorage<Complex> storage)
        {
            return new DenseVector(storage);
        }

        public override Vector<Complex> SparseVector(SparseVectorStorage<Complex> storage)
        {
            return new SparseVector(storage);
        }

        public override Matrix<Complex> DenseMatrixRandom(int rows, int columns, IContinuousDistribution distribution)
        {
            return LinearAlgebra.Complex.DenseMatrix.CreateRandom(rows, columns, distribution);
        }

        public override Vector<Complex> DenseVectorRandom(int length, IContinuousDistribution distribution)
        {
            return new DenseVector(DenseVectorStorage<Complex>.OfInit(length, i => new Complex(distribution.Sample(), distribution.Sample())));
        }

        public override IIterationStopCriterium<Complex>[] IterativeSolverStopCriteria(int maxIterations = 1000)
        {
            return new IIterationStopCriterium<Complex>[]
            {
                new FailureStopCriterium(),
                new DivergenceStopCriterium(),
                new IterationCountStopCriterium<Complex>(maxIterations),
                new ResidualStopCriterium()
            };
        }
    }
}

namespace MathNet.Numerics.LinearAlgebra.Complex32
{
    using Solvers;

    internal class Builder : Builder<Numerics.Complex32>
    {
        public override Numerics.Complex32 Zero
        {
            get { return Numerics.Complex32.Zero; }
        }

        public override Numerics.Complex32 One
        {
            get { return Numerics.Complex32.One; }
        }

        public override Matrix<Numerics.Complex32> DenseMatrix(DenseColumnMajorMatrixStorage<Numerics.Complex32> storage)
        {
            return new DenseMatrix(storage);
        }

        public override Matrix<Numerics.Complex32> SparseMatrix(SparseCompressedRowMatrixStorage<Numerics.Complex32> storage)
        {
            return new SparseMatrix(storage);
        }

        public override Matrix<Numerics.Complex32> DiagonalMatrix(DiagonalMatrixStorage<Numerics.Complex32> storage)
        {
            return new DiagonalMatrix(storage);
        }

        public override Vector<Numerics.Complex32> DenseVector(DenseVectorStorage<Numerics.Complex32> storage)
        {
            return new DenseVector(storage);
        }

        public override Vector<Numerics.Complex32> SparseVector(SparseVectorStorage<Numerics.Complex32> storage)
        {
            return new SparseVector(storage);
        }

        public override Matrix<Numerics.Complex32> DenseMatrixRandom(int rows, int columns, IContinuousDistribution distribution)
        {
            return Complex32.DenseMatrix.CreateRandom(rows, columns, distribution);
        }

        public override Vector<Numerics.Complex32> DenseVectorRandom(int length, IContinuousDistribution distribution)
        {
            return new DenseVector(DenseVectorStorage<Numerics.Complex32>.OfInit(length, i => new Numerics.Complex32((float)distribution.Sample(), (float)distribution.Sample())));
        }

        public override IIterationStopCriterium<Numerics.Complex32>[] IterativeSolverStopCriteria(int maxIterations = 1000)
        {
            return new IIterationStopCriterium<Numerics.Complex32>[]
            {
                new FailureStopCriterium(),
                new DivergenceStopCriterium(),
                new IterationCountStopCriterium<Numerics.Complex32>(maxIterations),
                new ResidualStopCriterium()
            };
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
    public abstract class Builder<T> where T : struct, IEquatable<T>, IFormattable
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
        public Matrix<T> Matrix(MatrixStorage<T> storage)
        {
            var dense = storage as DenseColumnMajorMatrixStorage<T>;
            if (dense != null) return DenseMatrix(dense);

            var sparse = storage as SparseCompressedRowMatrixStorage<T>;
            if (sparse != null) return SparseMatrix(sparse);

            var diagonal = storage as DiagonalMatrixStorage<T>;
            if (diagonal != null) return DiagonalMatrix(diagonal);

            throw new NotSupportedException();
        }

        /// <summary>
        /// Create a new vector straight from an initialized matrix storage instance.
        /// If you have an instance of a discrete storage type instead, use their direct methods instead.
        /// </summary>
        public Vector<T> Vector(VectorStorage<T> storage)
        {
            var dense = storage as DenseVectorStorage<T>;
            if (dense != null) return DenseVector(dense);

            var sparse = storage as SparseVectorStorage<T>;
            if (sparse != null) return SparseVector(sparse);

            throw new NotSupportedException();
        }

        /// <summary>
        /// Create a new dense matrix straight from an initialized matrix storage instance.
        /// The storage is used directly without copying.
        /// Intended for advanced scenarios where you're working directly with
        /// storage for performance or interop reasons.
        /// </summary>
        public abstract Matrix<T> DenseMatrix(DenseColumnMajorMatrixStorage<T> storage);

        /// <summary>
        /// Create a new dense matrix with the given number of rows and columns.
        /// All cells of the matrix will be initialized to zero.
        /// Zero-length matrices are not supported.
        /// </summary>
        public Matrix<T> DenseMatrix(int rows, int columns)
        {
            return DenseMatrix(new DenseColumnMajorMatrixStorage<T>(rows, columns));
        }

        /// <summary>
        /// Create a new dense matrix with the given number of rows and columns directly binding to a raw array.
        /// The array is assumed to be in column-major order (column by column) and is used directly without copying.
        /// Very efficient, but changes to the array and the matrix will affect each other.
        /// </summary>
        /// <seealso href="http://en.wikipedia.org/wiki/Row-major_order"/>
        public Matrix<T> DenseMatrix(int rows, int columns, T[] storage)
        {
            return DenseMatrix(new DenseColumnMajorMatrixStorage<T>(rows, columns, storage));
        }

        /// <summary>
        /// Create a new dense matrix and initialize each value to the same provided value.
        /// </summary>
        public Matrix<T> DenseMatrix(int rows, int columns, T value)
        {
            if (Zero.Equals(value)) return DenseMatrix(rows, columns);
            return DenseMatrix(DenseColumnMajorMatrixStorage<T>.OfInit(rows, columns, (i, j) => value));
        }

        /// <summary>
        /// Create a new dense matrix and initialize each value using the provided init function.
        /// </summary>
        public Matrix<T> DenseMatrix(int rows, int columns, Func<int, int, T> init)
        {
            return DenseMatrix(DenseColumnMajorMatrixStorage<T>.OfInit(rows, columns, init));
        }

        /// <summary>
        /// Create a new diagonal dense matrix and initialize each diagonal value to the same provided value.
        /// </summary>
        public Matrix<T> DenseMatrixDiagonal(int rows, int columns, T value)
        {
            if (Zero.Equals(value)) return DenseMatrix(rows, columns);
            return DenseMatrix(DenseColumnMajorMatrixStorage<T>.OfDiagonalInit(rows, columns, i => value));
        }

        /// <summary>
        /// Create a new diagonal dense matrix and initialize each diagonal value using the provided init function.
        /// </summary>
        public Matrix<T> DenseMatrixDiagonal(int rows, int columns, Func<int, T> init)
        {
            return DenseMatrix(DenseColumnMajorMatrixStorage<T>.OfDiagonalInit(rows, columns, init));
        }

        /// <summary>
        /// Create a new dense matrix with values sampled from the provided random distribution.
        /// </summary>
        public abstract Matrix<T> DenseMatrixRandom(int rows, int columns, IContinuousDistribution distribution);

        /// <summary>
        /// Create a new dense matrix as a copy of the given other matrix.
        /// This new matrix will be independent from the other matrix.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        public Matrix<T> DenseMatrixOfMatrix(Matrix<T> matrix)
        {
            return DenseMatrix(DenseColumnMajorMatrixStorage<T>.OfMatrix(matrix.Storage));
        }

        /// <summary>
        /// Create a new dense matrix as a copy of the given two-dimensional array.
        /// This new matrix will be independent from the provided array.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        public Matrix<T> DenseMatrixOfArray(T[,] array)
        {
            return DenseMatrix(DenseColumnMajorMatrixStorage<T>.OfArray(array));
        }

        /// <summary>
        /// Create a new dense matrix as a copy of the given indexed enumerable.
        /// Keys must be provided at most once, zero is assumed if a key is omitted.
        /// This new matrix will be independent from the enumerable.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        public Matrix<T> DenseMatrixOfIndexed(int rows, int columns, IEnumerable<Tuple<int, int, T>> enumerable)
        {
            return DenseMatrix(DenseColumnMajorMatrixStorage<T>.OfIndexedEnumerable(rows, columns, enumerable));
        }

        /// <summary>
        /// Create a new dense matrix as a copy of the given enumerable.
        /// The enumerable is assumed to be in column-major order (column by column).
        /// This new matrix will be independent from the enumerable.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        public Matrix<T> DenseMatrixOfColumnMajor(int rows, int columns, IEnumerable<T> columnMajor)
        {
            return DenseMatrix(DenseColumnMajorMatrixStorage<T>.OfColumnMajorEnumerable(rows, columns, columnMajor));
        }

        /// <summary>
        /// Create a new dense matrix as a copy of the given enumerable of enumerable columns.
        /// Each enumerable in the master enumerable specifies a column.
        /// This new matrix will be independent from the enumerables.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        public Matrix<T> DenseMatrixOfColumns(IEnumerable<IEnumerable<T>> data)
        {
            return DenseMatrix(DenseColumnMajorMatrixStorage<T>.OfColumnArrays(data.Select(v => v.ToArray()).ToArray()));
        }

        /// <summary>
        /// Create a new dense matrix as a copy of the given enumerable of enumerable columns.
        /// Each enumerable in the master enumerable specifies a column.
        /// This new matrix will be independent from the enumerables.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        public Matrix<T> DenseMatrixOfColumns(int rows, int columns, IEnumerable<IEnumerable<T>> data)
        {
            return DenseMatrix(DenseColumnMajorMatrixStorage<T>.OfColumnEnumerables(rows, columns, data));
        }

        /// <summary>
        /// Create a new dense matrix of T as a copy of the given column arrays.
        /// This new matrix will be independent from the arrays.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        public Matrix<T> DenseMatrixOfColumnArrays(params T[][] columns)
        {
            return DenseMatrix(DenseColumnMajorMatrixStorage<T>.OfColumnArrays(columns));
        }

        /// <summary>
        /// Create a new dense matrix as a copy of the given column vectors.
        /// This new matrix will be independent from the vectors.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        public Matrix<T> DenseMatrixOfColumnVectors(params Vector<T>[] columns)
        {
            var storage = new VectorStorage<T>[columns.Length];
            for (int i = 0; i < columns.Length; i++)
            {
                storage[i] = columns[i].Storage;
            }
            return DenseMatrix(DenseColumnMajorMatrixStorage<T>.OfColumnVectors(storage));
        }

        /// <summary>
        /// Create a new dense matrix as a copy of the given enumerable of enumerable rows.
        /// Each enumerable in the master enumerable specifies a row.
        /// This new matrix will be independent from the enumerables.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        public Matrix<T> DenseMatrixOfRows(IEnumerable<IEnumerable<T>> data)
        {
            return DenseMatrix(DenseColumnMajorMatrixStorage<T>.OfRowArrays(data.Select(v => v.ToArray()).ToArray()));
        }

        /// <summary>
        /// Create a new dense matrix as a copy of the given enumerable of enumerable rows.
        /// Each enumerable in the master enumerable specifies a row.
        /// This new matrix will be independent from the enumerables.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        public Matrix<T> DenseMatrixOfRows(int rows, int columns, IEnumerable<IEnumerable<T>> data)
        {
            return DenseMatrix(DenseColumnMajorMatrixStorage<T>.OfRowEnumerables(rows, columns, data));
        }

        /// <summary>
        /// Create a new dense matrix of T as a copy of the given row arrays.
        /// This new matrix will be independent from the arrays.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        public Matrix<T> DenseMatrixOfRowArrays(params T[][] rows)
        {
            return DenseMatrix(DenseColumnMajorMatrixStorage<T>.OfRowArrays(rows));
        }

        /// <summary>
        /// Create a new dense matrix as a copy of the given row vectors.
        /// This new matrix will be independent from the vectors.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        public Matrix<T> DenseMatrixOfRowVectors(params Vector<T>[] rows)
        {
            var storage = new VectorStorage<T>[rows.Length];
            for (int i = 0; i < rows.Length; i++)
            {
                storage[i] = rows[i].Storage;
            }
            return DenseMatrix(DenseColumnMajorMatrixStorage<T>.OfRowVectors(storage));
        }

        /// <summary>
        /// Create a new dense matrix with the diagonal as a copy of the given vector.
        /// This new matrix will be independent from the vector.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        public Matrix<T> DenseMatrixOfDiagonalVector(Vector<T> diagonal)
        {
            var m = DenseMatrix(diagonal.Count, diagonal.Count);
            m.SetDiagonal(diagonal);
            return m;
        }

        /// <summary>
        /// Create a new dense matrix with the diagonal as a copy of the given vector.
        /// This new matrix will be independent from the vector.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        public Matrix<T> DenseMatrixOfDiagonalVector(int rows, int columns, Vector<T> diagonal)
        {
            var m = DenseMatrix(rows, columns);
            m.SetDiagonal(diagonal);
            return m;
        }

        /// <summary>
        /// Create a new dense matrix with the diagonal as a copy of the given array.
        /// This new matrix will be independent from the array.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        public Matrix<T> DenseMatrixOfDiagonalArray(T[] diagonal)
        {
            var m = DenseMatrix(diagonal.Length, diagonal.Length);
            m.SetDiagonal(diagonal);
            return m;
        }

        /// <summary>
        /// Create a new dense matrix with the diagonal as a copy of the given array.
        /// This new matrix will be independent from the array.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        public Matrix<T> DenseMatrixOfDiagonalArray(int rows, int columns, T[] diagonal)
        {
            var m = DenseMatrix(rows, columns);
            m.SetDiagonal(diagonal);
            return m;
        }

        /// <summary>
        /// Create a new sparse matrix straight from an initialized matrix storage instance.
        /// The storage is used directly without copying.
        /// Intended for advanced scenarios where you're working directly with
        /// storage for performance or interop reasons.
        /// </summary>
        public abstract Matrix<T> SparseMatrix(SparseCompressedRowMatrixStorage<T> storage);

        /// <summary>
        /// Create a sparse matrix of T with the given number of rows and columns.
        /// </summary>
        /// <param name="rows">The number of rows.</param>
        /// <param name="columns">The number of columns.</param>
        public Matrix<T> SparseMatrix(int rows, int columns)
        {
            return SparseMatrix(new SparseCompressedRowMatrixStorage<T>(rows, columns));
        }

        /// <summary>
        /// Create a new sparse matrix and initialize each value to the same provided value.
        /// </summary>
        public Matrix<T> SparseMatrix(int rows, int columns, T value)
        {
            if (Zero.Equals(value)) return SparseMatrix(rows, columns);
            return SparseMatrix(SparseCompressedRowMatrixStorage<T>.OfInit(rows, columns, (i, j) => value));
        }

        /// <summary>
        /// Create a new sparse matrix and initialize each value using the provided init function.
        /// </summary>
        public Matrix<T> SparseMatrix(int rows, int columns, Func<int, int, T> init)
        {
            return SparseMatrix(SparseCompressedRowMatrixStorage<T>.OfInit(rows, columns, init));
        }

        /// <summary>
        /// Create a new diagonal sparse matrix and initialize each diagonal value to the same provided value.
        /// </summary>
        public Matrix<T> SparseMatrixDiagonal(int rows, int columns, T value)
        {
            if (Zero.Equals(value)) return SparseMatrix(rows, columns);
            return SparseMatrix(SparseCompressedRowMatrixStorage<T>.OfDiagonalInit(rows, columns, i => value));
        }

        /// <summary>
        /// Create a new diagonal sparse matrix and initialize each diagonal value using the provided init function.
        /// </summary>
        public Matrix<T> SparseMatrixDiagonal(int rows, int columns, Func<int, T> init)
        {
            return SparseMatrix(SparseCompressedRowMatrixStorage<T>.OfDiagonalInit(rows, columns, init));
        }

        /// <summary>
        /// Create a new sparse matrix as a copy of the given other matrix.
        /// This new matrix will be independent from the other matrix.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        public Matrix<T> SparseMatrixOfMatrix(Matrix<T> matrix)
        {
            return SparseMatrix(SparseCompressedRowMatrixStorage<T>.OfMatrix(matrix.Storage));
        }

        /// <summary>
        /// Create a new sparse matrix as a copy of the given two-dimensional array.
        /// This new matrix will be independent from the provided array.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        public Matrix<T> SparseMatrixOfArray(T[,] array)
        {
            return SparseMatrix(SparseCompressedRowMatrixStorage<T>.OfArray(array));
        }

        /// <summary>
        /// Create a new sparse matrix as a copy of the given indexed enumerable.
        /// Keys must be provided at most once, zero is assumed if a key is omitted.
        /// This new matrix will be independent from the enumerable.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        public Matrix<T> SparseMatrixOfIndexed(int rows, int columns, IEnumerable<Tuple<int, int, T>> enumerable)
        {
            return SparseMatrix(SparseCompressedRowMatrixStorage<T>.OfIndexedEnumerable(rows, columns, enumerable));
        }

        /// <summary>
        /// Create a new sparse matrix as a copy of the given enumerable.
        /// The enumerable is assumed to be in row-major order (row by row).
        /// This new matrix will be independent from the enumerable.
        /// A new memory block will be allocated for storing the vector.
        /// </summary>
        /// <seealso href="http://en.wikipedia.org/wiki/Row-major_order"/>
        public Matrix<T> SparseMatrixOfRowMajor(int rows, int columns, IEnumerable<T> rowMajor)
        {
            return SparseMatrix(SparseCompressedRowMatrixStorage<T>.OfRowMajorEnumerable(rows, columns, rowMajor));
        }

        /// <summary>
        /// Create a new sparse matrix with the given number of rows and columns as a copy of the given array.
        /// The array is assumed to be in column-major order (column by column).
        /// This new matrix will be independent from the provided array.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        /// <seealso href="http://en.wikipedia.org/wiki/Row-major_order"/>
        public Matrix<T> SparseMatrixOfColumnMajor(int rows, int columns, IList<T> columnMajor)
        {
            return SparseMatrix(SparseCompressedRowMatrixStorage<T>.OfColumnMajorList(rows, columns, columnMajor));
        }

        /// <summary>
        /// Create a new sparse matrix as a copy of the given enumerable of enumerable columns.
        /// Each enumerable in the master enumerable specifies a column.
        /// This new matrix will be independent from the enumerables.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        public Matrix<T> SparseMatrixOfColumns(IEnumerable<IEnumerable<T>> data)
        {
            return SparseMatrix(SparseCompressedRowMatrixStorage<T>.OfColumnArrays(data.Select(v => v.ToArray()).ToArray()));
        }

        /// <summary>
        /// Create a new sparse matrix as a copy of the given enumerable of enumerable columns.
        /// Each enumerable in the master enumerable specifies a column.
        /// This new matrix will be independent from the enumerables.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        public Matrix<T> SparseMatrixOfColumns(int rows, int columns, IEnumerable<IEnumerable<T>> data)
        {
            return SparseMatrix(SparseCompressedRowMatrixStorage<T>.OfColumnEnumerables(rows, columns, data));
        }

        /// <summary>
        /// Create a new sparse matrix as a copy of the given column arrays.
        /// This new matrix will be independent from the arrays.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        public Matrix<T> SparseMatrixOfColumnArrays(params T[][] columns)
        {
            return SparseMatrix(SparseCompressedRowMatrixStorage<T>.OfColumnArrays(columns));
        }

        /// <summary>
        /// Create a new sparse matrix as a copy of the given column vectors.
        /// This new matrix will be independent from the vectors.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        public Matrix<T> SparseMatrixOfColumnVectors(params Vector<T>[] columns)
        {
            var storage = new VectorStorage<T>[columns.Length];
            for (int i = 0; i < columns.Length; i++)
            {
                storage[i] = columns[i].Storage;
            }
            return SparseMatrix(SparseCompressedRowMatrixStorage<T>.OfColumnVectors(storage));
        }

        /// <summary>
        /// Create a new sparse matrix as a copy of the given enumerable of enumerable rows.
        /// Each enumerable in the master enumerable specifies a row.
        /// This new matrix will be independent from the enumerables.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        public Matrix<T> SparseMatrixOfRows(IEnumerable<IEnumerable<T>> data)
        {
            return SparseMatrix(SparseCompressedRowMatrixStorage<T>.OfRowArrays(data.Select(v => v.ToArray()).ToArray()));
        }

        /// <summary>
        /// Create a new sparse matrix as a copy of the given enumerable of enumerable rows.
        /// Each enumerable in the master enumerable specifies a row.
        /// This new matrix will be independent from the enumerables.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        public Matrix<T> SparseMatrixOfRows(int rows, int columns, IEnumerable<IEnumerable<T>> data)
        {
            return SparseMatrix(SparseCompressedRowMatrixStorage<T>.OfRowEnumerables(rows, columns, data));
        }

        /// <summary>
        /// Create a new sparse matrix as a copy of the given row arrays.
        /// This new matrix will be independent from the arrays.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        public Matrix<T> SparseMatrixOfRowArrays(params T[][] rows)
        {
            return SparseMatrix(SparseCompressedRowMatrixStorage<T>.OfRowArrays(rows));
        }

        /// <summary>
        /// Create a new sparse matrix as a copy of the given row vectors.
        /// This new matrix will be independent from the vectors.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        public Matrix<T> SparseMatrixOfRowVectors(params Vector<T>[] rows)
        {
            var storage = new VectorStorage<T>[rows.Length];
            for (int i = 0; i < rows.Length; i++)
            {
                storage[i] = rows[i].Storage;
            }
            return SparseMatrix(SparseCompressedRowMatrixStorage<T>.OfRowVectors(storage));
        }

        /// <summary>
        /// Create a new sparse matrix with the diagonal as a copy of the given vector.
        /// This new matrix will be independent from the vector.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        public Matrix<T> SparseMatrixOfDiagonalVector(Vector<T> diagonal)
        {
            var m = SparseMatrix(diagonal.Count, diagonal.Count);
            m.SetDiagonal(diagonal);
            return m;
        }

        /// <summary>
        /// Create a new sparse matrix with the diagonal as a copy of the given vector.
        /// This new matrix will be independent from the vector.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        public Matrix<T> SparseMatrixOfDiagonalVector(int rows, int columns, Vector<T> diagonal)
        {
            var m = SparseMatrix(rows, columns);
            m.SetDiagonal(diagonal);
            return m;
        }

        /// <summary>
        /// Create a new sparse matrix with the diagonal as a copy of the given array.
        /// This new matrix will be independent from the array.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        public Matrix<T> SparseMatrixOfDiagonalArray(T[] diagonal)
        {
            var m = SparseMatrix(diagonal.Length, diagonal.Length);
            m.SetDiagonal(diagonal);
            return m;
        }

        /// <summary>
        /// Create a new sparse matrix with the diagonal as a copy of the given array.
        /// This new matrix will be independent from the array.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        public Matrix<T> SparseMatrixOfDiagonalArray(int rows, int columns, T[] diagonal)
        {
            var m = SparseMatrix(rows, columns);
            m.SetDiagonal(diagonal);
            return m;
        }

        /// <summary>
        /// Create a new diagonal matrix straight from an initialized matrix storage instance.
        /// The storage is used directly without copying.
        /// Intended for advanced scenarios where you're working directly with
        /// storage for performance or interop reasons.
        /// </summary>
        public abstract Matrix<T> DiagonalMatrix(DiagonalMatrixStorage<T> storage);

        /// <summary>
        /// Create a new dense vector straight from an initialized vector storage instance.
        /// The storage is used directly without copying.
        /// Intended for advanced scenarios where you're working directly with
        /// storage for performance or interop reasons.
        /// </summary>
        public abstract Vector<T> DenseVector(DenseVectorStorage<T> storage);

        /// <summary>
        /// Create a dense vector of T with the given size.
        /// </summary>
        /// <param name="size">The size of the vector.</param>
        public Vector<T> DenseVector(int size)
        {
            return DenseVector(new DenseVectorStorage<T>(size));
        }

        /// <summary>
        /// Create a dense vector of T that is directly bound to the specified array.
        /// </summary>
        public Vector<T> DenseVector(T[] array)
        {
            return DenseVector(new DenseVectorStorage<T>(array.Length, array));
        }

        /// <summary>
        /// Create a new dense vector and initialize each value using the provided value.
        /// </summary>
        public Vector<T> DenseVector(int length, T value)
        {
            if (Zero.Equals(value)) return DenseVector(length);
            return DenseVector(DenseVectorStorage<T>.OfInit(length, i => value));
        }

        /// <summary>
        /// Create a new dense vector and initialize each value using the provided init function.
        /// </summary>
        public Vector<T> DenseVector(int length, Func<int, T> init)
        {
            return DenseVector(DenseVectorStorage<T>.OfInit(length, init));
        }

        /// <summary>
        /// Create a new dense vector with values sampled from the provided random distribution.
        /// </summary>
        public abstract Vector<T> DenseVectorRandom(int length, IContinuousDistribution distribution);

        /// <summary>
        /// Create a new dense vector as a copy of the given other vector.
        /// This new vector will be independent from the other vector.
        /// A new memory block will be allocated for storing the vector.
        /// </summary>
        public  Vector<T> DenseVectorOfVector(Vector<T> vector)
        {
            return DenseVector(DenseVectorStorage<T>.OfVector(vector.Storage));
        }

        /// <summary>
        /// Create a new dense vector as a copy of the given enumerable.
        /// This new vector will be independent from the enumerable.
        /// A new memory block will be allocated for storing the vector.
        /// </summary>
        public  Vector<T> DenseVectorOfEnumerable(IEnumerable<T> enumerable)
        {
            return DenseVector(DenseVectorStorage<T>.OfEnumerable(enumerable));
        }

        /// <summary>
        /// Create a new dense vector as a copy of the given indexed enumerable.
        /// Keys must be provided at most once, zero is assumed if a key is omitted.
        /// This new vector will be independent from the enumerable.
        /// A new memory block will be allocated for storing the vector.
        /// </summary>
        public Vector<T> DenseVectorOfIndexedEnumerable(int length, IEnumerable<Tuple<int, T>> enumerable)
        {
            return DenseVector(DenseVectorStorage<T>.OfIndexedEnumerable(length, enumerable));
        }

        /// <summary>
        /// Create a new sparse vector straight from an initialized vector storage instance.
        /// The storage is used directly without copying.
        /// Intended for advanced scenarios where you're working directly with
        /// storage for performance or interop reasons.
        /// </summary>
        public abstract Vector<T> SparseVector(SparseVectorStorage<T> storage);

        /// <summary>
        /// Create a sparse vector of T with the given size.
        /// </summary>
        /// <param name="size">The size of the vector.</param>
        public Vector<T> SparseVector(int size)
        {
            return SparseVector(new SparseVectorStorage<T>(size));
        }

        /// <summary>
        /// Create a new sparse vector and initialize each value using the provided value.
        /// </summary>
        public Vector<T> SparseVector(int length, T value)
        {
            if (Zero.Equals(value)) return SparseVector(length);
            return SparseVector(SparseVectorStorage<T>.OfInit(length, i => value));
        }

        /// <summary>
        /// Create a new sparse vector and initialize each value using the provided init function.
        /// </summary>
        public Vector<T> SparseVector(int length, Func<int, T> init)
        {
            return SparseVector(SparseVectorStorage<T>.OfInit(length, init));
        }

        /// <summary>
        /// Create a new sparse vector as a copy of the given other vector.
        /// This new vector will be independent from the other vector.
        /// A new memory block will be allocated for storing the vector.
        /// </summary>
        public Vector<T> SparseVectorOfVector(Vector<T> vector)
        {
            return SparseVector(SparseVectorStorage<T>.OfVector(vector.Storage));
        }

        /// <summary>
        /// Create a new sparse vector as a copy of the given enumerable.
        /// This new vector will be independent from the enumerable.
        /// A new memory block will be allocated for storing the vector.
        /// </summary>
        public Vector<T> SparseVectorOfEnumerable(IEnumerable<T> enumerable)
        {
            return SparseVector(SparseVectorStorage<T>.OfEnumerable(enumerable));
        }

        /// <summary>
        /// Create a new sparse vector as a copy of the given indexed enumerable.
        /// Keys must be provided at most once, zero is assumed if a key is omitted.
        /// This new vector will be independent from the enumerable.
        /// A new memory block will be allocated for storing the vector.
        /// </summary>
        public Vector<T> SparseVectorOfIndexed(int length, IEnumerable<Tuple<int, T>> enumerable)
        {
            return SparseVector(SparseVectorStorage<T>.OfIndexedEnumerable(length, enumerable));
        }

        public abstract IIterationStopCriterium<T>[] IterativeSolverStopCriteria(int maxIterations = 1000);
    }

    internal static class BuilderInstance<T> where T : struct, IEquatable<T>, IFormattable
    {
        static Lazy<Builder<T>> _singleton = new Lazy<Builder<T>>(Create);

        static Builder<T> Create()
        {
            if (typeof (T) == typeof (Complex64))
            {
                return (Builder<T>) (object) new Complex.Builder();
            }

            if (typeof (T) == typeof (Numerics.Complex32))
            {
                return (Builder<T>) (object) new Complex32.Builder();
            }

            if (typeof (T) == typeof (double))
            {
                return (Builder<T>) (object) new Double.Builder();
            }

            if (typeof (T) == typeof (float))
            {
                return (Builder<T>) (object) new Single.Builder();
            }

            throw new NotSupportedException();
        }

        public static void Register(Builder<T> builder)
        {
            _singleton = new Lazy<Builder<T>>(() => builder);
        }

        public static Builder<T> Instance
        {
            get { return _singleton.Value; }
        }
    }
}
