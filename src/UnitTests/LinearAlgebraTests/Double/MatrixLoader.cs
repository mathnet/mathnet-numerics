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
        protected Dictionary<string, double[,]> testData2D;
        protected Dictionary<string, Matrix> testMatrices;

        protected abstract Matrix CreateMatrix(int rows, int columns);
        protected abstract Matrix CreateMatrix(double[,] data);
        protected abstract Vector CreateVector(int size);
        protected abstract Vector CreateVector(double[] data);

        [SetUp]
        public void SetupMatrices()
        {
            testData2D = new Dictionary<string, double[,]>();
            testData2D.Add("Singular3x3", new double[,] { { 1, 1, 2 }, { 1, 1, 2 }, { 1, 1, 2 } });
            testData2D.Add("Square3x3", new double[,] { { -1.1, -2.2, -3.3 }, { 0, 1.1, 2.2 }, { -4.4, 5.5, 6.6 } });
            testData2D.Add("Square4x4", new double[,] { { -1.1, -2.2, -3.3, -4.4 }, { 0, 1.1, 2.2, 3.3 }, { 1.0, 2.1, 6.2, 4.3 }, { -4.4, 5.5, 6.6, -7.7 } });
            testData2D.Add("Singular4x4", new double[,] { { -1.1, -2.2, -3.3, -4.4 }, { -1.1, -2.2, -3.3, -4.4 }, { -1.1, -2.2, -3.3, -4.4 }, { -1.1, -2.2, -3.3, -4.4 } });
            testData2D.Add("Tall3x2", new double[,] { { -1.1, -2.2 }, { 0, 1.1 }, { -4.4, 5.5 } });
            testData2D.Add("Wide2x3", new double[,] { { -1.1, -2.2, -3.3 }, { 0, 1.1, 2.2 } });

            testMatrices = new Dictionary<string, Matrix>();
            foreach (var name in testData2D.Keys)
            {
                testMatrices.Add(name, CreateMatrix(testData2D[name]));
            }
        }

        public static Matrix GenerateRandomMatrix(int row, int col)
        {
            // Fill a matrix with standard random numbers.
            var normal = new Distributions.Normal();
            normal.RandomSource = new Random.MersenneTwister(1);
            var A = new DenseMatrix(row, col);
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < col; j++)
                {
                    A[i, j] = normal.Sample();
                }
            }

            // Generate a matrix which is positive definite.
            return A;
        }

        public static Matrix GenerateRandomPositiveDefiniteMatrix(int order)
        {
            // Fill a matrix with standard random numbers.
            var normal = new Distributions.Normal();
            normal.RandomSource = new Random.MersenneTwister(1);
            var A = new DenseMatrix(order);
            for (int i = 0; i < order; i++)
            {
                for (int j = 0; j < order; j++)
                {
                    A[i, j] = normal.Sample();
                }
            }

            // Generate a matrix which is positive definite.
            return A.Transpose() * A;
        }

        public static Vector GenerateRandomVector(int order)
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
    }
}
