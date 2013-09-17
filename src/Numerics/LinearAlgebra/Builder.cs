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
using MathNet.Numerics.LinearAlgebra.Solvers;

namespace MathNet.Numerics.LinearAlgebra.Double
{
    using Solvers.StopCriterium;

    internal class GenericBuilder : IGenericBuilder<double>
    {
        public double Zero
        {
            get { return 0d; }
        }

        public double One
        {
            get { return 1d; }
        }

        public Matrix<double> DenseMatrix(int rows, int columns)
        {
            return new DenseMatrix(rows, columns);
        }

        public Matrix<double> SparseMatrix(int rows, int columns)
        {
            return new SparseMatrix(rows, columns);
        }

        public Vector<double> DenseVector(int size)
        {
            return new DenseVector(size);
        }

        public Vector<double> SparseVector(int size)
        {
            return new SparseVector(size);
        }

        public IIterationStopCriterium<double>[] IterativeSolverStopCriteria(int maxIterations)
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
    using Solvers.StopCriterium;

    internal class GenericBuilder : IGenericBuilder<float>
    {
        public float Zero
        {
            get { return 0f; }
        }

        public float One
        {
            get { return 1f; }
        }

        public Matrix<float> DenseMatrix(int rows, int columns)
        {
            return new DenseMatrix(rows, columns);
        }

        public Matrix<float> SparseMatrix(int rows, int columns)
        {
            return new SparseMatrix(rows, columns);
        }

        public Vector<float> DenseVector(int size)
        {
            return new DenseVector(size);
        }

        public Vector<float> SparseVector(int size)
        {
            return new SparseVector(size);
        }

        public IIterationStopCriterium<float>[] IterativeSolverStopCriteria(int maxIterations)
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
    using Solvers.StopCriterium;

#if NOSYSNUMERICS
    using Complex = Numerics.Complex;
#else
    using Complex = System.Numerics.Complex;
#endif

    internal class GenericBuilder : IGenericBuilder<Complex>
    {
        public Complex Zero
        {
            get { return Complex.Zero; }
        }

        public Complex One
        {
            get { return Complex.One; }
        }

        public Matrix<Complex> DenseMatrix(int rows, int columns)
        {
            return new DenseMatrix(rows, columns);
        }

        public Matrix<Complex> SparseMatrix(int rows, int columns)
        {
            return new SparseMatrix(rows, columns);
        }

        public Vector<Complex> DenseVector(int size)
        {
            return new DenseVector(size);
        }

        public Vector<Complex> SparseVector(int size)
        {
            return new SparseVector(size);
        }

        public IIterationStopCriterium<Complex>[] IterativeSolverStopCriteria(int maxIterations)
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
    using Solvers.StopCriterium;

    internal class GenericBuilder : IGenericBuilder<Numerics.Complex32>
    {
        public Numerics.Complex32 Zero
        {
            get { return Numerics.Complex32.Zero; }
        }

        public Numerics.Complex32 One
        {
            get { return Numerics.Complex32.One; }
        }

        public Matrix<Numerics.Complex32> DenseMatrix(int rows, int columns)
        {
            return new DenseMatrix(rows, columns);
        }

        public Matrix<Numerics.Complex32> SparseMatrix(int rows, int columns)
        {
            return new SparseMatrix(rows, columns);
        }

        public Vector<Numerics.Complex32> DenseVector(int size)
        {
            return new DenseVector(size);
        }

        public Vector<Numerics.Complex32> SparseVector(int size)
        {
            return new SparseVector(size);
        }

        public IIterationStopCriterium<Numerics.Complex32>[] IterativeSolverStopCriteria(int maxIterations)
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
    public interface IGenericBuilder<T> where T : struct, IEquatable<T>, IFormattable
    {
        /// <summary>
        /// Gets the value of <c>0.0</c> for type T.
        /// </summary>
        T Zero { get; }

        /// <summary>
        /// Gets the value of <c>1.0</c> for type T.
        /// </summary>
        T One { get; }

        /// <summary>
        /// Create a dense matrix of T with the given number of rows and columns.
        /// </summary>
        /// <param name="rows">The number of rows.</param>
        /// <param name="columns">The number of columns.</param>
        Matrix<T> DenseMatrix(int rows, int columns);

        /// <summary>
        /// Create a sparse matrix of T with the given number of rows and columns.
        /// </summary>
        /// <param name="rows">The number of rows.</param>
        /// <param name="columns">The number of columns.</param>
        Matrix<T> SparseMatrix(int rows, int columns);

        /// <summary>
        /// Create a dense vector of T with the given size.
        /// </summary>
        /// <param name="size">The size of the vector.</param>
        Vector<T> DenseVector(int size);

        /// <summary>
        /// Create a sparse vector of T with the given size.
        /// </summary>
        /// <param name="size">The size of the vector.</param>
        Vector<T> SparseVector(int size);

        IIterationStopCriterium<T>[] IterativeSolverStopCriteria(int maxIterations = 1000);
    }

    internal static class Builder<T> where T : struct, IEquatable<T>, IFormattable
    {
        static Lazy<IGenericBuilder<T>> _singleton = new Lazy<IGenericBuilder<T>>(Create);

        static IGenericBuilder<T> Create()
        {
            if (typeof (T) == typeof (Complex64))
            {
                return (IGenericBuilder<T>) new Complex.GenericBuilder();
            }

            if (typeof (T) == typeof (Numerics.Complex32))
            {
                return (IGenericBuilder<T>) new Complex32.GenericBuilder();
            }

            if (typeof (T) == typeof (double))
            {
                return (IGenericBuilder<T>) new Double.GenericBuilder();
            }

            if (typeof (T) == typeof (float))
            {
                return (IGenericBuilder<T>) new Single.GenericBuilder();
            }

            throw new NotSupportedException();
        }

        public static void Register(IGenericBuilder<T> builder)
        {
            _singleton = new Lazy<IGenericBuilder<T>>(() => builder);
        }

        public static IGenericBuilder<T> Instance
        {
            get { return _singleton.Value; }
        }
    }
}
