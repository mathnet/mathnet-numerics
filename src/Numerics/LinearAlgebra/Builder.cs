// <copyright file="Builder.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2014 Math.NET
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
using MathNet.Numerics.Random;

namespace MathNet.Numerics.LinearAlgebra.Double
{
    internal class MatrixBuilder : MatrixBuilder<double>
    {
        public override double Zero => 0d;

        public override double One => 1d;

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
            return Dense(rows, columns, Generate.Random(rows*columns, distribution));
        }

        public override IIterationStopCriterion<double>[] IterativeSolverStopCriteria(int maxIterations = 1000)
        {
            return new IIterationStopCriterion<double>[]
            {
                new FailureStopCriterion<double>(),
                new DivergenceStopCriterion<double>(),
                new IterationCountStopCriterion<double>(maxIterations),
                new ResidualStopCriterion<double>(1e-12)
            };
        }

        internal override double Add(double x, double y)
        {
            return x + y;
        }
    }

    internal class VectorBuilder : VectorBuilder<double>
    {
        public override double Zero => 0d;

        public override double One => 1d;

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
            return Dense(Generate.Random(length, distribution));
        }
    }
}

namespace MathNet.Numerics.LinearAlgebra.Single
{
    internal class MatrixBuilder : MatrixBuilder<float>
    {
        public override float Zero => 0f;

        public override float One => 1f;

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
            return Dense(rows, columns, Generate.RandomSingle(rows*columns, distribution));
        }

        public override IIterationStopCriterion<float>[] IterativeSolverStopCriteria(int maxIterations = 1000)
        {
            return new IIterationStopCriterion<float>[]
            {
                new FailureStopCriterion<float>(),
                new DivergenceStopCriterion<float>(),
                new IterationCountStopCriterion<float>(maxIterations),
                new ResidualStopCriterion<float>(1e-6)
            };
        }

        internal override float Add(float x, float y)
        {
            return x + y;
        }
    }

    internal class VectorBuilder : VectorBuilder<float>
    {
        public override float Zero => 0f;

        public override float One => 1f;

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
            return Dense(Generate.RandomSingle(length, distribution));
        }
    }
}

namespace MathNet.Numerics.LinearAlgebra.Complex
{
    using Complex = System.Numerics.Complex;

    internal class MatrixBuilder : MatrixBuilder<Complex>
    {
        public override Complex Zero => Complex.Zero;

        public override Complex One => Complex.One;

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
            return Dense(rows, columns, Generate.RandomComplex(rows*columns, distribution));
        }

        public override IIterationStopCriterion<Complex>[] IterativeSolverStopCriteria(int maxIterations = 1000)
        {
            return new IIterationStopCriterion<Complex>[]
            {
                new FailureStopCriterion<Complex>(),
                new DivergenceStopCriterion<Complex>(),
                new IterationCountStopCriterion<Complex>(maxIterations),
                new ResidualStopCriterion<Complex>(1e-12)
            };
        }

        internal override Complex Add(Complex x, Complex y)
        {
            return x + y;
        }
    }

    internal class VectorBuilder : VectorBuilder<Complex>
    {
        public override Complex Zero => Complex.Zero;

        public override Complex One => Complex.One;

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
            return Dense(Generate.RandomComplex(length, distribution));
        }
    }
}

namespace MathNet.Numerics.LinearAlgebra.Complex32
{
    internal class MatrixBuilder : MatrixBuilder<Numerics.Complex32>
    {
        public override Numerics.Complex32 Zero => Numerics.Complex32.Zero;

        public override Numerics.Complex32 One => Numerics.Complex32.One;

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
            return Dense(rows, columns, Generate.RandomComplex32(rows*columns, distribution));
        }

        public override IIterationStopCriterion<Numerics.Complex32>[] IterativeSolverStopCriteria(int maxIterations = 1000)
        {
            return new IIterationStopCriterion<Numerics.Complex32>[]
            {
                new FailureStopCriterion<Numerics.Complex32>(),
                new DivergenceStopCriterion<Numerics.Complex32>(),
                new IterationCountStopCriterion<Numerics.Complex32>(maxIterations),
                new ResidualStopCriterion<Numerics.Complex32>(1e-6)
            };
        }

        internal override Numerics.Complex32 Add(Numerics.Complex32 x, Numerics.Complex32 y)
        {
            return x + y;
        }
    }

    internal class VectorBuilder : VectorBuilder<Numerics.Complex32>
    {
        public override Numerics.Complex32 Zero => Numerics.Complex32.Zero;

        public override Numerics.Complex32 One => Numerics.Complex32.One;

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
            return Dense(Generate.RandomComplex32(length, distribution));
        }
    }
}

namespace MathNet.Numerics.LinearAlgebra
{
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

        internal abstract T Add(T x, T y);

        /// <summary>
        /// Create a new matrix straight from an initialized matrix storage instance.
        /// If you have an instance of a discrete storage type instead, use their direct methods instead.
        /// </summary>
        public Matrix<T> OfStorage(MatrixStorage<T> storage)
        {
            if (storage == null) throw new ArgumentNullException(nameof(storage));

            if (storage is DenseColumnMajorMatrixStorage<T> dense) return Dense(dense);
            if (storage is SparseCompressedRowMatrixStorage<T> sparse) return Sparse(sparse);
            if (storage is DiagonalMatrixStorage<T> diagonal) return Diagonal(diagonal);

            throw new NotSupportedException(FormattableString.Invariant($"Matrix storage type '{storage.GetType().Name}' is not supported. Only DenseColumnMajorMatrixStorage, SparseCompressedRowMatrixStorage and DiagonalMatrixStorage are supported as this point."));
        }

        /// <summary>
        /// Create a new matrix with the same kind of the provided example.
        /// </summary>
        public Matrix<T> SameAs<TU>(Matrix<TU> example, int rows, int columns, bool fullyMutable = false)
            where TU : struct, IEquatable<TU>, IFormattable
        {
            var storage = example.Storage;
            if (storage is DenseColumnMajorMatrixStorage<T>) return Dense(rows, columns);
            if (storage is DiagonalMatrixStorage<T>) return fullyMutable ? Sparse(rows, columns) : Diagonal(rows, columns);
            if (storage is SparseCompressedRowMatrixStorage<T>) return Sparse(rows, columns);
            return Dense(rows, columns);
        }

        /// <summary>
        /// Create a new matrix with the same kind and dimensions of the provided example.
        /// </summary>
        public Matrix<T> SameAs<TU>(Matrix<TU> example)
            where TU : struct, IEquatable<TU>, IFormattable
        {
            return SameAs(example, example.RowCount, example.ColumnCount);
        }

        /// <summary>
        /// Create a new matrix with the same kind of the provided example.
        /// </summary>
        public Matrix<T> SameAs(Vector<T> example, int rows, int columns)
        {
            return example.Storage.IsDense ? Dense(rows, columns) : Sparse(rows, columns);
        }

        /// <summary>
        /// Create a new matrix with a type that can represent and is closest to both provided samples.
        /// </summary>
        public Matrix<T> SameAs(Matrix<T> example, Matrix<T> otherExample, int rows, int columns, bool fullyMutable = false)
        {
            var storage1 = example.Storage;
            var storage2 = otherExample.Storage;
            if (storage1 is DenseColumnMajorMatrixStorage<T> || storage2 is DenseColumnMajorMatrixStorage<T>) return Dense(rows, columns);
            if (storage1 is DiagonalMatrixStorage<T> && storage2 is DiagonalMatrixStorage<T>) return fullyMutable ? Sparse(rows, columns) : Diagonal(rows, columns);
            if (storage1 is SparseCompressedRowMatrixStorage<T> || storage2 is SparseCompressedRowMatrixStorage<T>) return Sparse(rows, columns);
            return Dense(rows, columns);
        }

        /// <summary>
        /// Create a new matrix with a type that can represent and is closest to both provided samples and the dimensions of example.
        /// </summary>
        public Matrix<T> SameAs(Matrix<T> example, Matrix<T> otherExample)
        {
            return SameAs(example, otherExample, example.RowCount, example.ColumnCount);
        }

        /// <summary>
        /// Create a new dense matrix with values sampled from the provided random distribution.
        /// </summary>
        public abstract Matrix<T> Random(int rows, int columns, IContinuousDistribution distribution);

        /// <summary>
        /// Create a new dense matrix with values sampled from the standard distribution with a system random source.
        /// </summary>
        public Matrix<T> Random(int rows, int columns)
        {
            return Random(rows, columns, new Normal(SystemRandomSource.Default));
        }

        /// <summary>
        /// Create a new dense matrix with values sampled from the standard distribution with a system random source.
        /// </summary>
        public Matrix<T> Random(int rows, int columns, int seed)
        {
            return Random(rows, columns, new Normal(new SystemRandomSource(seed, true)));
        }

        /// <summary>
        /// Create a new positive definite dense matrix where each value is the product
        /// of two samples from the provided random distribution.
        /// </summary>
        public Matrix<T> RandomPositiveDefinite(int order, IContinuousDistribution distribution)
        {
            var a = Random(order, order, distribution);
            return a.ConjugateTransposeThisAndMultiply(a);
        }

        /// <summary>
        /// Create a new positive definite dense matrix where each value is the product
        /// of two samples from the standard distribution.
        /// </summary>
        public Matrix<T> RandomPositiveDefinite(int order)
        {
            var a = Random(order, order, new Normal(SystemRandomSource.Default));
            return a.ConjugateTransposeThisAndMultiply(a);
        }

        /// <summary>
        /// Create a new positive definite dense matrix where each value is the product
        /// of two samples from the provided random distribution.
        /// </summary>
        public Matrix<T> RandomPositiveDefinite(int order, int seed)
        {
            var a = Random(order, order, new Normal(new SystemRandomSource(seed, true)));
            return a.ConjugateTransposeThisAndMultiply(a);
        }

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
            return Dense(DenseColumnMajorMatrixStorage<T>.OfValue(rows, columns, value));
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
            return Dense(DenseColumnMajorMatrixStorage<T>.OfDiagonalInit(rows, columns, _ => value));
        }

        /// <summary>
        /// Create a new diagonal dense matrix and initialize each diagonal value to the same provided value.
        /// </summary>
        public Matrix<T> DenseDiagonal(int order, T value)
        {
            if (Zero.Equals(value)) return Dense(order, order);
            return Dense(DenseColumnMajorMatrixStorage<T>.OfDiagonalInit(order, order, _ => value));
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
            return Dense(DenseColumnMajorMatrixStorage<T>.OfDiagonalInit(rows, columns, _ => One));
        }

        /// <summary>
        /// Create a new diagonal dense identity matrix with a one-diagonal.
        /// </summary>
        public Matrix<T> DenseIdentity(int order)
        {
            return Dense(DenseColumnMajorMatrixStorage<T>.OfDiagonalInit(order, order, _ => One));
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
        /// Create a new dense matrix as a copy of the given indexed enumerable.
        /// Keys must be provided at most once, zero is assumed if a key is omitted.
        /// This new matrix will be independent from the enumerable.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        public Matrix<T> DenseOfIndexed(int rows, int columns, IEnumerable<(int, int, T)> enumerable)
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
        /// Create a new dense matrix as a copy of the given enumerable.
        /// The enumerable is assumed to be in row-major order (row by row).
        /// This new matrix will be independent from the enumerable.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        public Matrix<T> DenseOfRowMajor(int rows, int columns, IEnumerable<T> columnMajor)
        {
            return Dense(DenseColumnMajorMatrixStorage<T>.OfRowMajorEnumerable(rows, columns, columnMajor));
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
                    m.SetSubMatrix(rowoffset, coloffset, matrices[i, j]);
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
        /// <param name="storage">The SparseCompressedRowMatrixStorage</param>
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
            return Sparse(SparseCompressedRowMatrixStorage<T>.OfValue(rows, columns, value));
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
            return Sparse(SparseCompressedRowMatrixStorage<T>.OfDiagonalInit(rows, columns, _ => value));
        }

        /// <summary>
        /// Create a new diagonal sparse matrix and initialize each diagonal value to the same provided value.
        /// </summary>
        public Matrix<T> SparseDiagonal(int order, T value)
        {
            if (Zero.Equals(value)) return Sparse(order, order);
            return Sparse(SparseCompressedRowMatrixStorage<T>.OfDiagonalInit(order, order, _ => value));
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
            return Sparse(SparseCompressedRowMatrixStorage<T>.OfDiagonalInit(rows, columns, _ => One));
        }

        /// <summary>
        /// Create a new diagonal dense identity matrix with a one-diagonal.
        /// </summary>
        public Matrix<T> SparseIdentity(int order)
        {
            return Sparse(SparseCompressedRowMatrixStorage<T>.OfDiagonalInit(order, order, _ => One));
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
        /// Create a new sparse matrix as a copy of the given indexed enumerable.
        /// Keys must be provided at most once, zero is assumed if a key is omitted.
        /// This new matrix will be independent from the enumerable.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        public Matrix<T> SparseOfIndexed(int rows, int columns, IEnumerable<(int, int, T)> enumerable)
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


        // Representation of Sparse Matrix
        //
        // Matrix A = [ 0 b 0 h 0 0 ]
        //            [ a c e i 0 0 ]
        //            [ 0 0 f j l n ]
        //            [ 0 d g k m 0 ]
        //
        // rows = 4, columns = 6, valueCount = 14
        //
        // (1) COO, Coordinate, ijv, or triplet format:
        //     cooRowIndices     = { 0, 0, 1, 1, 1, 1, 2, 2, 2, 2, 3, 3, 3, 3 }
        //     cooColumnIndices  = { 1, 3, 0, 1, 2, 3, 2, 3, 4, 5, 1, 2, 3, 4 }
        //     cooValues         = { b, h, a, c, e, i, f, j, l, n, d, g, k, m }
        //
        // (2) CSR, Compressed Sparse Row or Compressed Row Storage(CRS) or Yale format:
        //     csrRowPointers    = { 0, 2, 6, 10, 14 }
        //     csrColumnIndices  = { 1, 3, 0, 1, 2, 3, 2, 3, 4, 5, 1, 2, 3, 4 }
        //     csrValues         = { b, h, a, c, e, i, f, j, l, n, d, g, k, m }
        //
        // (3) CSC, Compressed Sparse Column or Compressed Column Storage(CCS) format:
        //     cscColumnPointers = { 0, 1, 4, 7, 11, 13, 14 }
        //     cscRowIndices     = { 1, 0, 1, 3, 1, 2, 3, 0, 1, 2, 3, 2, 3, 2 }
        //     cscValues         = { a, b, c, d, e, f, g, h, i, j, k, l, m, n }

        /// <summary>
        /// Create a new sparse matrix from a coordinate format.
        /// This new matrix will be independent from the given arrays.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        /// <param name="rows">The number of rows.</param>
        /// <param name="columns">The number of columns.</param>
        /// <param name="valueCount">The number of stored values including explicit zeros.</param>
        /// <param name="rowIndices">The row index array of the coordinate format.</param>
        /// <param name="columnIndices">The column index array of the coordinate format.</param>
        /// <param name="values">The data array of the coordinate format.</param>
        /// <returns>The sparse matrix from the coordinate format.</returns>
        /// <remarks>Duplicate entries will be summed together and explicit zeros will be not intentionally removed.</remarks>
        public Matrix<T> SparseFromCoordinateFormat(int rows, int columns, int valueCount, int[] rowIndices, int[] columnIndices, T[] values)
        {
            return Sparse(SparseCompressedRowMatrixStorage<T>.OfCoordinateFormat(rows, columns, valueCount, rowIndices, columnIndices, values));
        }

        /// <summary>
        /// Create a new sparse matrix from a compressed sparse row format.
        /// This new matrix will be independent from the given arrays.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        /// <param name="rows">The number of rows.</param>
        /// <param name="columns">The number of columns.</param>
        /// <param name="valueCount">The number of stored values including explicit zeros.</param>
        /// <param name="rowPointers">The row pointer array of the compressed sparse row format.</param>
        /// <param name="columnIndices">The column index array of the compressed sparse row format.</param>
        /// <param name="values">The data array of the compressed sparse row format.</param>
        /// <returns>The sparse matrix from the compressed sparse row format.</returns>
        /// <remarks>Duplicate entries will be summed together and explicit zeros will be not intentionally removed.</remarks>
        public Matrix<T> SparseFromCompressedSparseRowFormat(int rows, int columns, int valueCount, int[] rowPointers, int[] columnIndices, T[] values)
        {
            return Sparse(SparseCompressedRowMatrixStorage<T>.OfCompressedSparseRowFormat(rows, columns, valueCount, rowPointers, columnIndices, values));
        }

        /// <summary>
        /// Create a new sparse matrix from a compressed sparse column format.
        /// This new matrix will be independent from the given arrays.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        /// <param name="rows">The number of rows.</param>
        /// <param name="columns">The number of columns.</param>
        /// <param name="valueCount">The number of stored values including explicit zeros.</param>
        /// <param name="rowIndices">The row index array of the compressed sparse column format.</param>
        /// <param name="columnPointers">The column pointer array of the compressed sparse column format.</param>
        /// <param name="values">The data array of the compressed sparse column format.</param>
        /// <returns>The sparse matrix from the compressed sparse column format.</returns>
        /// <remarks>Duplicate entries will be summed together and explicit zeros will be not intentionally removed.</remarks>
        public Matrix<T> SparseFromCompressedSparseColumnFormat(int rows, int columns, int valueCount, int[] rowIndices, int[] columnPointers, T[] values)
        {
            return Sparse(SparseCompressedRowMatrixStorage<T>.OfCompressedSparseColumnFormat(rows, columns, valueCount, rowIndices, columnPointers, values));
        }

        /// <summary>
        /// Create a new diagonal matrix straight from an initialized matrix storage instance.
        /// The storage is used directly without copying.
        /// Intended for advanced scenarios where you're working directly with
        /// storage for performance or interop reasons.
        /// </summary>
        public abstract Matrix<T> Diagonal(DiagonalMatrixStorage<T> storage);

        /// <summary>
        /// Create a new diagonal matrix with the given number of rows and columns.
        /// All cells of the matrix will be initialized to zero.
        /// </summary>
        public Matrix<T> Diagonal(int rows, int columns)
        {
            return Diagonal(new DiagonalMatrixStorage<T>(rows, columns));
        }

        /// <summary>
        /// Create a new diagonal matrix with the given number of rows and columns directly binding to a raw array.
        /// The array is assumed to represent the diagonal values and is used directly without copying.
        /// Very efficient, but changes to the array and the matrix will affect each other.
        /// </summary>
        public Matrix<T> Diagonal(int rows, int columns, T[] storage)
        {
            return Diagonal(new DiagonalMatrixStorage<T>(rows, columns, storage));
        }

        /// <summary>
        /// Create a new square diagonal matrix directly binding to a raw array.
        /// The array is assumed to represent the diagonal values and is used directly without copying.
        /// Very efficient, but changes to the array and the matrix will affect each other.
        /// </summary>
        public Matrix<T> Diagonal(T[] storage)
        {
            return Diagonal(new DiagonalMatrixStorage<T>(storage.Length, storage.Length, storage));
        }

        /// <summary>
        /// Create a new diagonal matrix and initialize each diagonal value to the same provided value.
        /// </summary>
        public Matrix<T> Diagonal(int rows, int columns, T value)
        {
            if (Zero.Equals(value)) return Diagonal(rows, columns);
            return Diagonal(DiagonalMatrixStorage<T>.OfValue(rows, columns, value));
        }

        /// <summary>
        /// Create a new diagonal matrix and initialize each diagonal value using the provided init function.
        /// </summary>
        public Matrix<T> Diagonal(int rows, int columns, Func<int, T> init)
        {
            return Diagonal(DiagonalMatrixStorage<T>.OfInit(rows, columns, init));
        }

        /// <summary>
        /// Create a new diagonal identity matrix with a one-diagonal.
        /// </summary>
        public Matrix<T> DiagonalIdentity(int rows, int columns)
        {
            return Diagonal(DiagonalMatrixStorage<T>.OfValue(rows, columns, One));
        }

        /// <summary>
        /// Create a new diagonal identity matrix with a one-diagonal.
        /// </summary>
        public Matrix<T> DiagonalIdentity(int order)
        {
            return Diagonal(DiagonalMatrixStorage<T>.OfValue(order, order, One));
        }


        /// <summary>
        /// Create a new diagonal matrix with the diagonal as a copy of the given vector.
        /// This new matrix will be independent from the vector.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        public Matrix<T> DiagonalOfDiagonalVector(Vector<T> diagonal)
        {
            var m = Diagonal(diagonal.Count, diagonal.Count);
            m.SetDiagonal(diagonal);
            return m;
        }

        /// <summary>
        /// Create a new diagonal matrix with the diagonal as a copy of the given vector.
        /// This new matrix will be independent from the vector.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        public Matrix<T> DiagonalOfDiagonalVector(int rows, int columns, Vector<T> diagonal)
        {
            var m = Diagonal(rows, columns);
            m.SetDiagonal(diagonal);
            return m;
        }

        /// <summary>
        /// Create a new diagonal matrix with the diagonal as a copy of the given array.
        /// This new matrix will be independent from the array.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        public Matrix<T> DiagonalOfDiagonalArray(T[] diagonal)
        {
            var m = Diagonal(diagonal.Length, diagonal.Length);
            m.SetDiagonal(diagonal);
            return m;
        }

        /// <summary>
        /// Create a new diagonal matrix with the diagonal as a copy of the given array.
        /// This new matrix will be independent from the array.
        /// A new memory block will be allocated for storing the matrix.
        /// </summary>
        public Matrix<T> DiagonalOfDiagonalArray(int rows, int columns, T[] diagonal)
        {
            var m = Diagonal(rows, columns);
            m.SetDiagonal(diagonal);
            return m;
        }

        public abstract IIterationStopCriterion<T>[] IterativeSolverStopCriteria(int maxIterations = 1000);
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
            if (storage == null) throw new ArgumentNullException(nameof(storage));

            if (storage is DenseVectorStorage<T> dense) return Dense(dense);
            if (storage is SparseVectorStorage<T> sparse) return Sparse(sparse);

            throw new NotSupportedException(FormattableString.Invariant($"Vector storage type '{storage.GetType().Name}' is not supported. Only DenseVectorStorage and SparseVectorStorage are supported as this point."));
        }

        /// <summary>
        /// Create a new vector with the same kind of the provided example.
        /// </summary>
        public Vector<T> SameAs<TU>(Vector<TU> example, int length)
            where TU : struct, IEquatable<TU>, IFormattable
        {
            return example.Storage.IsDense ? Dense(length) : Sparse(length);
        }

        /// <summary>
        /// Create a new vector with the same kind and dimension of the provided example.
        /// </summary>
        public Vector<T> SameAs<TU>(Vector<TU> example)
            where TU : struct, IEquatable<TU>, IFormattable
        {
            return example.Storage.IsDense ? Dense(example.Count) : Sparse(example.Count);
        }

        /// <summary>
        /// Create a new vector with the same kind of the provided example.
        /// </summary>
        public Vector<T> SameAs<TU>(Matrix<TU> example, int length)
            where TU : struct, IEquatable<TU>, IFormattable
        {
            return example.Storage.IsDense ? Dense(length) : Sparse(length);
        }

        /// <summary>
        /// Create a new vector with a type that can represent and is closest to both provided samples.
        /// </summary>
        public Vector<T> SameAs(Vector<T> example, Vector<T> otherExample, int length)
        {
            return example.Storage.IsDense || otherExample.Storage.IsDense ? Dense(length) : Sparse(length);
        }

        /// <summary>
        /// Create a new vector with a type that can represent and is closest to both provided samples and the dimensions of example.
        /// </summary>
        public Vector<T> SameAs(Vector<T> example, Vector<T> otherExample)
        {
            return example.Storage.IsDense || otherExample.Storage.IsDense ? Dense(example.Count) : Sparse(example.Count);
        }

        /// <summary>
        /// Create a new vector with a type that can represent and is closest to both provided samples.
        /// </summary>
        public Vector<T> SameAs(Matrix<T> matrix, Vector<T> vector, int length)
        {
            return matrix.Storage.IsDense || vector.Storage.IsDense ? Dense(length) : Sparse(length);
        }

        /// <summary>
        /// Create a new dense vector with values sampled from the provided random distribution.
        /// </summary>
        public abstract Vector<T> Random(int length, IContinuousDistribution distribution);

        /// <summary>
        /// Create a new dense vector with values sampled from the standard distribution with a system random source.
        /// </summary>
        public Vector<T> Random(int length)
        {
            return Random(length, new Normal(SystemRandomSource.Default));
        }

        /// <summary>
        /// Create a new dense vector with values sampled from the standard distribution with a system random source.
        /// </summary>
        public Vector<T> Random(int length, int seed)
        {
            return Random(length, new Normal(new SystemRandomSource(seed, true)));
        }

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
            return Dense(DenseVectorStorage<T>.OfValue(length, value));
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
        /// Create a new dense vector as a copy of the given indexed enumerable.
        /// Keys must be provided at most once, zero is assumed if a key is omitted.
        /// This new vector will be independent from the enumerable.
        /// A new memory block will be allocated for storing the vector.
        /// </summary>
        public Vector<T> DenseOfIndexed(int length, IEnumerable<(int, T)> enumerable)
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
            return Sparse(SparseVectorStorage<T>.OfValue(length, value));
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

        /// <summary>
        /// Create a new sparse vector as a copy of the given indexed enumerable.
        /// Keys must be provided at most once, zero is assumed if a key is omitted.
        /// This new vector will be independent from the enumerable.
        /// A new memory block will be allocated for storing the vector.
        /// </summary>
        public Vector<T> SparseOfIndexed(int length, IEnumerable<(int, T)> enumerable)
        {
            return Sparse(SparseVectorStorage<T>.OfIndexedEnumerable(length, enumerable));
        }
    }

    internal static class BuilderInstance<T> where T : struct, IEquatable<T>, IFormattable
    {
        static Lazy<Tuple<MatrixBuilder<T>, VectorBuilder<T>>> _singleton = new Lazy<Tuple<MatrixBuilder<T>, VectorBuilder<T>>>(Create);

        static Tuple<MatrixBuilder<T>, VectorBuilder<T>> Create()
        {
            if (typeof (T) == typeof (System.Numerics.Complex))
            {
                return new Tuple<MatrixBuilder<T>, VectorBuilder<T>>(
                    (MatrixBuilder<T>)(object)new Complex.MatrixBuilder(),
                    (VectorBuilder<T>)(object)new Complex.VectorBuilder());
            }

            if (typeof (T) == typeof (Numerics.Complex32))
            {
                return new Tuple<MatrixBuilder<T>, VectorBuilder<T>>(
                    (MatrixBuilder<T>)(object)new Complex32.MatrixBuilder(),
                    (VectorBuilder<T>)(object)new Complex32.VectorBuilder());
            }

            if (typeof (T) == typeof (double))
            {
                return new Tuple<MatrixBuilder<T>, VectorBuilder<T>>(
                    (MatrixBuilder<T>)(object)new Double.MatrixBuilder(),
                    (VectorBuilder<T>)(object)new Double.VectorBuilder());
            }

            if (typeof (T) == typeof (float))
            {
                return new Tuple<MatrixBuilder<T>, VectorBuilder<T>>(
                    (MatrixBuilder<T>)(object)new Single.MatrixBuilder(),
                    (VectorBuilder<T>)(object)new Single.VectorBuilder());
            }

            throw new NotSupportedException(FormattableString.Invariant($"Matrices and vectors of type '{typeof(T).Name}' are not supported. Only Double, Single, Complex or Complex32 are supported at this point."));
        }

        public static void Register(MatrixBuilder<T> matrixBuilder, VectorBuilder<T> vectorBuilder)
        {
            _singleton = new Lazy<Tuple<MatrixBuilder<T>, VectorBuilder<T>>>(() => new Tuple<MatrixBuilder<T>, VectorBuilder<T>>(matrixBuilder, vectorBuilder));
        }

        public static MatrixBuilder<T> Matrix => _singleton.Value.Item1;

        public static VectorBuilder<T> Vector => _singleton.Value.Item2;
    }
}
