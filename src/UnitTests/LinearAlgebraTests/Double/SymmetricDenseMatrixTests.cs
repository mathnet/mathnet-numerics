// <copyright file="SymmetricDenseMatrixTests.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
// Copyright (c) 2009-2010 Math.NET
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
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

    using MathNet.Numerics.LinearAlgebra.Double;

    using NUnit.Framework;

    /// <summary>
    /// Symmetric Dense matrix tests.
    /// </summary>
    public class SymmetricDenseMatrixTests : SymmetricMatrixTests
    {
        /// <summary>
        /// Creates a matrix for the given number of rows and columns.
        /// </summary>
        /// <param name="rows">
        /// The number of rows.
        /// </param>
        /// <param name="columns">
        /// The number of columns.
        /// </param>
        /// <returns>
        /// A matrix with the given dimensions.
        /// </returns>
        protected override Matrix CreateMatrix(int rows, int columns)
        {
            return new DenseMatrix(rows, columns);
        }

        /// <summary>
        /// Creates a matrix from a 2D array.
        /// </summary>
        /// <param name="data">
        /// The 2D array to create this matrix from.
        /// </param>
        /// <returns>
        /// A matrix with the given values.
        /// </returns>
        protected override Matrix CreateMatrix(double[,] data)
        {
            if (SymmetricMatrix.CheckIfSymmetric(data))
            {
                return new SymmetricDenseMatrix(data);
            }

            return new DenseMatrix(data);
        }

        /// <summary>
        /// Creates a vector of the given size.
        /// </summary>
        /// <param name="size">
        /// The size of the vector to create.
        /// </param>
        /// <returns>
        /// The new vector. 
        /// </returns>
        protected override Vector CreateVector(int size)
        {
            return new DenseVector(size);
        }

        /// <summary>
        /// Creates a vector from an array.
        /// </summary>
        /// <param name="data">
        /// The array to create this vector from.
        /// </param>
        /// <returns>
        /// The new vector. 
        /// </returns>
        protected override Vector CreateVector(double[] data)
        {
            return new DenseVector(data);
        }

        /// <summary>
        /// Can create a matrix form array.
        /// </summary>
        [Test]
        public void CanCreateMatrixFrom1DArray()
        {
            var testData = new Dictionary<string, Matrix>
                           {
                             { "Singular3x3", new SymmetricDenseMatrix(3, new[] { 1.0, 2.0, 0.0, 3.0, 0.0, 0.0 }) }, 
                             { "Square3x3", new SymmetricDenseMatrix(3, new[] { -1.1, 2.0, 1.1, 3.0, 0.0, 6.6 }) }, 
                             { "Square4x4", new SymmetricDenseMatrix(4, new[] { 1.1, 2.0, 5.0, -3.0, -6.0, 8.0, 4.4, 7.0, 9.0, 10.0 }) }, 
                             { "Singular4x4", new SymmetricDenseMatrix(4, new[] { 1.0, 2.0, 5.0, 0.0, 0.0, 0.0, 4.0, 7.0, 0.0, 10.0 }) }, 
                             { "Symmetric3x3", new SymmetricDenseMatrix(3, new[] { 1.0, 2.0, 2.0, 3.0, 0.0, 3.0 }) },
                             { "IndexTester4x4", new SymmetricDenseMatrix(4, new double[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }) }
                           };

            foreach (var name in testData.Keys)
            {
                Assert.AreEqual(TestMatrices[name], testData[name]);
            }
        }

        /// <summary>
        /// Can create a matrix form array.
        /// </summary>
        [Test]
        public void CanCreateMatrixFrom2DArray()
        {
            var testData = new Dictionary<string, Matrix>
                          {
                             { "Singular3x3", new SymmetricDenseMatrix(new[,] { { 1.0, 2.0, 3.0 }, { 2.0, 0.0, 0.0 }, { 3.0, 0.0, 0.0 } }) }, 
                             { "Square3x3", new SymmetricDenseMatrix(new[,] { { -1.1, 2.0, 3.0 }, { 2.0, 1.1, 0.0 }, { 3.0, 0.0, 6.6 } }) }, 
                             { "Square4x4", new SymmetricDenseMatrix(new[,] { { 1.1, 2.0, -3.0, 4.4 }, { 2.0, 5.0, -6.0, 7.0 }, { -3.0, -6.0, 8.0, 9.0 }, { 4.4, 7.0, 9.0, 10.0 } }) }, 
                             { "Singular4x4", new SymmetricDenseMatrix(new[,] { { 1.0, 2.0, 0.0, 4.0 }, { 2.0, 5.0, 0.0, 7.0 }, { 0.0, 0.0, 0.0, 0.0 }, { 4.0, 7.0, 0.0, 10.0 } }) }, 
                             { "Symmetric3x3", new SymmetricDenseMatrix(new[,] { { 1.0, 2.0, 3.0 }, { 2.0, 2.0, 0.0 }, { 3.0, 0.0, 3.0 } }) },
                             { "IndexTester4x4", new SymmetricDenseMatrix(new double[,] { { 0, 1, 3, 6 }, { 1, 2, 4, 7 }, { 3, 4, 5, 8 }, { 6, 7, 8, 9 } }) }
                         };

            foreach (var name in testData.Keys)
            {
                Assert.AreEqual(TestMatrices[name], testData[name]);
            }
        }
    }
}