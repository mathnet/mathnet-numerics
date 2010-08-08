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


namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Double
{
	using System.Collections.Generic;
	using LinearAlgebra.Double;
    using MbUnit.Framework;

    public abstract class MatrixLoader
    {
        protected Dictionary<string, double[,]> TestData2D;
        protected Dictionary<string, Matrix> TestMatrices;

        protected abstract Matrix CreateMatrix(int rows, int columns);
        protected abstract Matrix CreateMatrix(double[,] data);
        protected abstract Vector CreateVector(int size);
        protected abstract Vector CreateVector(double[] data);

        [SetUp]
        public virtual void SetupMatrices()
        {
            TestData2D = new Dictionary<string, double[,]>
                         {
                             { "Singular3x3", new [,] { { 1.0, 1.0, 2.0 }, { 1.0, 1.0, 2.0 }, { 1.0, 1.0, 2.0 } } },
                             { "Square3x3", new[,] { { -1.1, -2.2, -3.3 }, { 0.0, 1.1, 2.2 }, { -4.4, 5.5, 6.6 } } },
                             { "Square4x4", new[,] { { -1.1, -2.2, -3.3, -4.4 }, { 0.0, 1.1, 2.2, 3.3 }, { 1.0, 2.1, 6.2, 4.3 }, { -4.4, 5.5, 6.6, -7.7 } } },
                             { "Singular4x4", new[,] { { -1.1, -2.2, -3.3, -4.4 }, { -1.1, -2.2, -3.3, -4.4 }, { -1.1, -2.2, -3.3, -4.4 }, { -1.1, -2.2, -3.3, -4.4 } } },
                             { "Tall3x2", new[,] { { -1.1, -2.2 }, { 0.0, 1.1 }, { -4.4, 5.5 } } },
                             { "Wide2x3", new[,] { { -1.1, -2.2, -3.3 }, { 0.0, 1.1, 2.2 } } },
                         };

            TestMatrices = new Dictionary<string, Matrix>();
            foreach (var name in TestData2D.Keys)
            {
                TestMatrices.Add(name, CreateMatrix(TestData2D[name]));
            }
        }

        public static Matrix GenerateRandomDenseMatrix(int row, int col)
        {
            // Fill a matrix with standard random numbers.
            var normal = new Distributions.Normal();
            normal.RandomSource = new Random.MersenneTwister(1);
            var matrixA = new DenseMatrix(row, col);
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < col; j++)
                {
                    matrixA[i, j] = normal.Sample();
                }
            }

            // Generate a matrix which is positive definite.
            return matrixA;
        }

        public static Matrix GenerateRandomPositiveDefiniteDenseMatrix(int order)
        {
            // Fill a matrix with standard random numbers.
            var normal = new Distributions.Normal();
            normal.RandomSource = new Random.MersenneTwister(1);
            var matrixA = new DenseMatrix(order);
            for (int i = 0; i < order; i++)
            {
                for (int j = 0; j < order; j++)
                {
                    matrixA[i, j] = normal.Sample();
                }
            }

            // Generate a matrix which is positive definite.
            return matrixA.Transpose() * matrixA;
        }

        public static Vector GenerateRandomDenseVector(int order)
        {
            // Fill a matrix with standard random numbers.
            var normal = new Distributions.Normal();
            normal.RandomSource = new Random.MersenneTwister(1);
            var v = new DenseVector(order);
            for (int i = 0; i < order; i++)
            {
                v[i] = normal.Sample();
            }

            // Generate a matrix which is positive definite.
            return v;
        }

        public static Matrix GenerateRandomUserDefinedMatrix(int row, int col)
        {
            // Fill a matrix with standard random numbers.
            var normal = new Distributions.Normal();
            normal.RandomSource = new Random.MersenneTwister(1);
            var matrixA = new UserDefinedMatrix(row, col);
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < col; j++)
                {
                    matrixA[i, j] = normal.Sample();
                }
            }

            // Generate a matrix which is positive definite.
            return matrixA;
        }

        public static Matrix GenerateRandomPositiveDefiniteUserDefinedMatrix(int order)
        {
            // Fill a matrix with standard random numbers.
            var normal = new Distributions.Normal();
            normal.RandomSource = new Random.MersenneTwister(1);
            var matrixA = new UserDefinedMatrix(order);
            for (int i = 0; i < order; i++)
            {
                for (int j = 0; j < order; j++)
                {
                    matrixA[i, j] = normal.Sample();
                }
            }

            // Generate a matrix which is positive definite.
            return matrixA.Transpose() * matrixA;
        }

        public static Vector GenerateRandomUserDefinedVector(int order)
        {
            // Fill a matrix with standard random numbers.
            var normal = new Distributions.Normal();
            normal.RandomSource = new Random.MersenneTwister(1);
            var v = new UserDefinedVector(order);
            for (int i = 0; i < order; i++)
            {
                v[i] = normal.Sample();
            }

            // Generate a matrix which is positive definite.
            return v;
        }
    }
}
