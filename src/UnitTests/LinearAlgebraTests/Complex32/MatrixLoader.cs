// <copyright file="MatrixLoader.cs" company="Math.NET">
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

using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;
using NUnit.Framework;

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Complex32
{
    using Numerics;

    /// <summary>
    /// Base class for matrix tests.
    /// </summary>
    public abstract class MatrixLoader
    {
        /// <summary>
        /// Gets or sets test matrices values to use.
        /// </summary>
        protected Dictionary<string, Complex32[,]> TestData2D { get; set; }

        /// <summary>
        /// Gets or sets test matrices instances to use.
        /// </summary>
        protected Dictionary<string, Matrix<Complex32>> TestMatrices { get; set; }

        /// <summary>
        /// Creates a matrix for the given number of rows and columns.
        /// </summary>
        /// <param name="rows">The number of rows.</param>
        /// <param name="columns">The number of columns.</param>
        /// <returns>A matrix with the given dimensions.</returns>
        protected abstract Matrix<Complex32> CreateMatrix(int rows, int columns);

        /// <summary>
        /// Creates a matrix from a 2D array.
        /// </summary>
        /// <param name="data">The 2D array to create this matrix from.</param>
        /// <returns>A matrix with the given values.</returns>
        protected abstract Matrix<Complex32> CreateMatrix(Complex32[,] data);

        /// <summary>
        /// Setup test matrices.
        /// </summary>
        [SetUp]
        public virtual void SetupMatrices()
        {
            TestData2D = new Dictionary<string, Complex32[,]>
            {
                { "Singular3x3", new[,] { { new Complex32(1.0f, 1), new Complex32(1.0f, 1), new Complex32(2.0f, 1) }, { new Complex32(1.0f, 1), new Complex32(1.0f, 1), new Complex32(2.0f, 1) }, { new Complex32(1.0f, 1), new Complex32(1.0f, 1), new Complex32(2.0f, 1) } } },
                { "Square3x3", new[,] { { new Complex32(-1.1f, 1), new Complex32(-2.2f, 1), new Complex32(-3.3f, 1) }, { Complex32.Zero, new Complex32(1.1f, 1), new Complex32(2.2f, 1) }, { new Complex32(-4.4f, 1), new Complex32(5.5f, 1), new Complex32(6.6f, 1) } } },
                { "Square4x4", new[,] { { new Complex32(-1.1f, 1), new Complex32(-2.2f, 1), new Complex32(-3.3f, 1), new Complex32(-4.4f, 1) }, { Complex32.Zero, new Complex32(1.1f, 1), new Complex32(2.2f, 1), new Complex32(3.3f, 1) }, { new Complex32(1.0f, 1), new Complex32(2.1f, 1), new Complex32(6.2f, 1), new Complex32(4.3f, 1) }, { new Complex32(-4.4f, 1), new Complex32(5.5f, 1), new Complex32(6.6f, 1), new Complex32(-7.7f, 1) } } },
                { "Singular4x4", new[,] { { new Complex32(-1.1f, 1), new Complex32(-2.2f, 1), new Complex32(-3.3f, 1), new Complex32(-4.4f, 1) }, { new Complex32(-1.1f, 1), new Complex32(-2.2f, 1), new Complex32(-3.3f, 1), new Complex32(-4.4f, 1) }, { new Complex32(-1.1f, 1), new Complex32(-2.2f, 1), new Complex32(-3.3f, 1), new Complex32(-4.4f, 1) }, { new Complex32(-1.1f, 1), new Complex32(-2.2f, 1), new Complex32(-3.3f, 1), new Complex32(-4.4f, 1) } } },
                { "Tall3x2", new[,] { { new Complex32(-1.1f, 1), new Complex32(-2.2f, 1) }, { Complex32.Zero, new Complex32(1.1f, 1) }, { new Complex32(-4.4f, 1), new Complex32(5.5f, 1) } } },
                { "Wide2x3", new[,] { { new Complex32(-1.1f, 1), new Complex32(-2.2f, 1), new Complex32(-3.3f, 1) }, { Complex32.Zero, new Complex32(1.1f, 1), new Complex32(2.2f, 1) } } },
                { "Symmetric3x3", new[,] { { Complex32.One, 2.0f, 3.0f }, { 2.0f, 2.0f, 0.0f }, { 3.0f, 0.0f, 3.0f } } }
            };

            TestMatrices = new Dictionary<string, Matrix<Complex32>>();
            foreach (var name in TestData2D.Keys)
            {
                TestMatrices.Add(name, CreateMatrix(TestData2D[name]));
            }
        }
    }
}
