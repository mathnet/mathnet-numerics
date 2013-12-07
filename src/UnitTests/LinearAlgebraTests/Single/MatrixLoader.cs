// <copyright file="MatrixLoader.cs" company="Math.NET">
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

using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;
using NUnit.Framework;

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Single
{
    /// <summary>
    /// Base class for matrix tests.
    /// </summary>
    public abstract class MatrixLoader
    {
        /// <summary>
        /// Gets or sets test matrices values to use.
        /// </summary>
        protected Dictionary<string, float[,]> TestData2D { get; set; }

        /// <summary>
        /// Gets or sets test matrices instances to use.
        /// </summary>
        protected Dictionary<string, Matrix<float>> TestMatrices { get; set; }

        /// <summary>
        /// Creates a matrix for the given number of rows and columns.
        /// </summary>
        /// <param name="rows">The number of rows.</param>
        /// <param name="columns">The number of columns.</param>
        /// <returns>A matrix with the given dimensions.</returns>
        protected abstract Matrix<float> CreateMatrix(int rows, int columns);

        /// <summary>
        /// Creates a matrix from a 2D array.
        /// </summary>
        /// <param name="data">The 2D array to create this matrix from.</param>
        /// <returns>A matrix with the given values.</returns>
        protected abstract Matrix<float> CreateMatrix(float[,] data);

        /// <summary>
        /// Creates a vector of the given size.
        /// </summary>
        /// <param name="size">The size of the vector to create.
        /// </param>
        /// <returns>The new vector. </returns>
        protected abstract Vector<float> CreateVector(int size);

        /// <summary>
        /// Creates a vector from an array.
        /// </summary>
        /// <param name="data">The array to create this vector from.</param>
        /// <returns>The new vector. </returns>
        protected abstract Vector<float> CreateVector(float[] data);

        /// <summary>
        /// Setup test matrices.
        /// </summary>
        [SetUp]
        public virtual void SetupMatrices()
        {
            TestData2D = new Dictionary<string, float[,]>
            {
                { "Singular3x3", new[,] { { 1.0f, 1.0f, 2.0f }, { 1.0f, 1.0f, 2.0f }, { 1.0f, 1.0f, 2.0f } } },
                { "Square3x3", new[,] { { -1.1f, -2.2f, -3.3f }, { 0.0f, 1.1f, 2.2f }, { -4.4f, 5.5f, 6.6f } } },
                { "Square4x4", new[,] { { -1.1f, -2.2f, -3.3f, -4.4f }, { 0.0f, 1.1f, 2.2f, 3.3f }, { 1.0f, 2.1f, 6.2f, 4.3f }, { -4.4f, 5.5f, 6.6f, -7.7f } } },
                { "Singular4x4", new[,] { { -1.1f, -2.2f, -3.3f, -4.4f }, { -1.1f, -2.2f, -3.3f, -4.4f }, { -1.1f, -2.2f, -3.3f, -4.4f }, { -1.1f, -2.2f, -3.3f, -4.4f } } },
                { "Tall3x2", new[,] { { -1.1f, -2.2f }, { 0.0f, 1.1f }, { -4.4f, 5.5f } } },
                { "Wide2x3", new[,] { { -1.1f, -2.2f, -3.3f }, { 0.0f, 1.1f, 2.2f } } },
                { "Symmetric3x3", new[,] { { 1.0f, 2.0f, 3.0f }, { 2.0f, 2.0f, 0.0f }, { 3.0f, 0.0f, 3.0f } } }
            };

            TestMatrices = new Dictionary<string, Matrix<float>>();
            foreach (var name in TestData2D.Keys)
            {
                TestMatrices.Add(name, CreateMatrix(TestData2D[name]));
            }
        }

        public static Matrix<float> GenerateRandomDenseMatrix(int row, int col)
        {
            return Matrix<float>.Build.Random(row, col, 1);
        }

        public static Matrix<float> GenerateRandomPositiveDefiniteDenseMatrix(int order)
        {
            return Matrix<float>.Build.RandomPositiveDefinite(order, 1);
        }

        public static Vector<float> GenerateRandomDenseVector(int order)
        {
            return Vector<float>.Build.Random(order, 1);
        }

        public static Matrix<float> GenerateRandomUserDefinedMatrix(int row, int col)
        {
            return new UserDefinedMatrix(GenerateRandomDenseMatrix(row, col).ToArray());
        }

        public static Matrix<float> GenerateRandomPositiveDefiniteUserDefinedMatrix(int order)
        {
            return new UserDefinedMatrix(GenerateRandomPositiveDefiniteDenseMatrix(order).ToArray());
        }

        public static Vector<float> GenerateRandomUserDefinedVector(int order)
        {
            return new UserDefinedVector(GenerateRandomDenseVector(order).ToArray());
        }
    }
}
