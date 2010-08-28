// <copyright file="MatrixLoader.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
//
// Copyright (c) 2009-2010 Math.NET
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


namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Complex
{
	using System.Collections.Generic;
    using System.Numerics;
	using LinearAlgebra.Complex;
	using LinearAlgebra.Generic;
	using MbUnit.Framework;

    public abstract class MatrixLoader
    {
        protected Dictionary<string, Complex[,]> TestData2D;
        protected Dictionary<string, Matrix<Complex>> TestMatrices;

        protected abstract Matrix<Complex> CreateMatrix(int rows, int columns);
        protected abstract Matrix<Complex> CreateMatrix(Complex[,] data);
        protected abstract Vector<Complex> CreateVector(int size);
        protected abstract Vector<Complex> CreateVector(Complex[] data);

        [SetUp]
        public virtual void SetupMatrices()
        {
            TestData2D = new Dictionary<string, Complex[,]>
                         {
                             { "Singular3x3", new[,] { { new Complex(1.0, 1), new Complex(1.0, 1), new Complex(2.0, 1) }, { new Complex(1.0, 1), new Complex(1.0, 1), new Complex(2.0, 1) }, { new Complex(1.0, 1), new Complex(1.0, 1), new Complex(2.0, 1) } } },
                             { "Square3x3",   new[,] { { new Complex(-1.1, 1), new Complex(-2.2, 1), new Complex(-3.3, 1) }, { Complex.Zero, new Complex(1.1, 1), new Complex(2.2, 1) }, { new Complex(-4.4, 1), new Complex(5.5, 1), new Complex(6.6, 1) } } },
                             { "Square4x4",   new[,] { { new Complex(-1.1, 1), new Complex(-2.2, 1), new Complex(-3.3, 1), new Complex(-4.4, 1) }, { Complex.Zero, new Complex(1.1, 1), new Complex(2.2, 1), new Complex(3.3, 1) }, { new Complex(1.0, 1), new Complex(2.1, 1), new Complex(6.2, 1), new Complex(4.3, 1) }, { new Complex(-4.4, 1), new Complex(5.5, 1), new Complex(6.6, 1), new Complex(-7.7, 1) } } },
                             { "Singular4x4", new[,] { { new Complex(-1.1, 1), new Complex(-2.2, 1), new Complex(-3.3, 1), new Complex(-4.4, 1) }, { new Complex(-1.1, 1), new Complex(-2.2, 1), new Complex(-3.3, 1), new Complex(-4.4, 1) }, { new Complex(-1.1, 1), new Complex(-2.2, 1), new Complex(-3.3, 1), new Complex(-4.4, 1) }, { new Complex(-1.1, 1), new Complex(-2.2, 1), new Complex(-3.3, 1), new Complex(-4.4, 1) } } },
                             { "Tall3x2",     new[,] { { new Complex(-1.1, 1), new Complex(-2.2, 1) }, { Complex.Zero, new Complex(1.1, 1) }, { new Complex(-4.4, 1), new Complex(5.5, 1) } } },
                             { "Wide2x3",     new[,] { { new Complex(-1.1, 1), new Complex(-2.2, 1), new Complex(-3.3, 1) }, { Complex.Zero, new Complex(1.1, 1), new Complex(2.2, 1) } } },
                         };

            TestMatrices = new Dictionary<string, Matrix<Complex>>();
            foreach (var name in TestData2D.Keys)
            {
                TestMatrices.Add(name, CreateMatrix(TestData2D[name]));
            }
        }

        public static Matrix<Complex> GenerateRandomDenseMatrix(int row, int col)
        {
            // Fill a matrix with standard random numbers.
            var normal = new Distributions.Normal
                         {
                             RandomSource = new Random.MersenneTwister(1)
                         };
            var matrixA = new DenseMatrix(row, col);
            for (var i = 0; i < row; i++)
            {
                for (var j = 0; j < col; j++)
                {
                    matrixA[i, j] = new Complex(normal.Sample(), normal.Sample());
                }
            }

            // Generate a matrix which is positive definite.
            return matrixA;
        }

        public static Matrix<Complex> GenerateRandomPositiveDefiniteHermitianDenseMatrix(int order)
        {
            // Fill a matrix with standard random numbers.
            var normal = new Distributions.Normal
                         {
                             RandomSource = new Random.MersenneTwister(1)
                         };
            var matrixA = new DenseMatrix(order);
            for (var i = 0; i < order; i++)
            {
                for (var j = 0; j < order; j++)
                {
                    matrixA[i, j] = new Complex(normal.Sample(), normal.Sample());
                }
            }

            // Generate a Hermitian matrix which is positive definite.
            return matrixA.ConjugateTranspose() * matrixA;
        }

        public static Vector<Complex> GenerateRandomDenseVector(int order)
        {
            // Fill a matrix with standard random numbers.
            var normal = new Distributions.Normal
                         {
                             RandomSource = new Random.MersenneTwister(1)
                         };
            var v = new DenseVector(order);
            for (var i = 0; i < order; i++)
            {
                v[i] = new Complex(normal.Sample(), normal.Sample());
            }

            // Generate a matrix which is positive definite.
            return v;
        }

        public static Matrix<Complex> GenerateRandomUserDefinedMatrix(int row, int col)
        {
            // Fill a matrix with standard random numbers.
            var normal = new Distributions.Normal
                         {
                             RandomSource = new Random.MersenneTwister(1)
                         };
            var matrixA = new UserDefinedMatrix(row, col);
            for (var i = 0; i < row; i++)
            {
                for (var j = 0; j < col; j++)
                {
                    matrixA[i, j] = new Complex(normal.Sample(), normal.Sample());
                }
            }

            // Generate a matrix which is positive definite.
            return matrixA;
        }

        public static Matrix<Complex> GenerateRandomPositiveDefiniteHermitianUserDefinedMatrix(int order)
        {
            // Fill a matrix with standard random numbers.
            var normal = new Distributions.Normal
                         {
                             RandomSource = new Random.MersenneTwister(1)
                         };
            var matrixA = new UserDefinedMatrix(order);
            for (var i = 0; i < order; i++)
            {
                for (var j = 0; j < order; j++)
                {
                    matrixA[i, j] = new Complex(normal.Sample(), normal.Sample());
                }
            }

            // Generate a Hermitian matrix which is positive definite.
            return matrixA.ConjugateTranspose() * matrixA;
        }

        public static Vector<Complex> GenerateRandomUserDefinedVector(int order)
        {
            // Fill a matrix with standard random numbers.
            var normal = new Distributions.Normal
                         {
                             RandomSource = new Random.MersenneTwister(1)
                         };
            var v = new UserDefinedVector(order);
            for (var i = 0; i < order; i++)
            {
                v[i] = new Complex(normal.Sample(), normal.Sample());
            }

            // Generate a matrix which is positive definite.
            return v;
        }
    }
}
