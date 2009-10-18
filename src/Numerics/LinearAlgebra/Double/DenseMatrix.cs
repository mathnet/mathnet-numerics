// <copyright file="DenseMatrix.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://mathnet.opensourcedotnet.info
//
// Copyright (c) 2009 Math.NET
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

namespace MathNet.Numerics.LinearAlgebra.Double
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using Algorithms;
    using Algorithms.LinearAlgebra;
    using NumberTheory;
    using Properties;
    using Threading;

    /// <summary>
    /// A Matrix class with dense storage.
    /// </summary>
    public class DenseMatrix : Matrix
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DenseMatrix"/> class. This matrix is square with a given size.
        /// </summary>
        /// <param name="order">the size of the square matrix.</param>
        /// <exception cref="ArgumentException">
        /// If <paramref name="order"/> is less than one.
        /// </exception>
        public DenseMatrix(int order)
            : base(order)
        {
            Data = new double[order*order];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DenseMatrix"/> class.
        /// </summary>
        /// <param name="rows">
        /// The number of rows.
        /// </param>
        /// <param name="columns">
        /// The number of columns.
        /// </param>
        public DenseMatrix(int rows, int columns)
            : base(rows, columns)
        {
            Data = new double[rows * columns];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DenseMatrix"/> class with all entries set to a particular value.
        /// </summary>
        /// <param name="rows">
        /// The number of rows.
        /// </param>
        /// <param name="columns">
        /// The number of columns.
        /// </param
        /// <param name="value">The value which we assign to each element of the matrix.</param>
        public DenseMatrix(int rows, int columns, double value)
            : base(rows, columns)
        {
            Data = new double[rows * columns];
            for (int i = 0; i < Data.Length; i++)
            {
                Data[i] = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DenseMatrix"/> class from a 2D array. This constructor
        /// will allocate a completely new memory block for storing the dense matrix.
        /// </summary>
        /// <param name="array">The 2D array to create this matrix from.</param>
        public DenseMatrix(double[,] array)
            : base(array.GetLength(0), array.GetLength(1))
        {
            Data = new double[array.GetLength(0) * array.GetLength(1)];
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    At(i, j, array[i,j]);
                }
            }
        }

        /// <summary>
        /// Gets or sets the matrix's data.
        /// </summary>
        /// <value>The matrix's data.</value>
        internal double[] Data
        {
            get;
            private set;
        }

        /// <summary>
        /// Creates a <strong>DenseMatrix</strong> for the given number of rows and columns.
        /// </summary>
        /// <param name="numberOfRows">
        /// The number of rows.
        /// </param>
        /// <param name="numberOfColumns">
        /// The number of columns.
        /// </param>
        /// <returns>
        /// A <strong>DenseMatrix</strong> with the given dimensions.
        /// </returns>
        public override Matrix CreateMatrix(int numberOfRows, int numberOfColumns)
        {
            return new DenseMatrix(numberOfRows, numberOfColumns);
        }

        /// <summary>
        /// Retrieves the requested element without range checking.
        /// </summary>
        /// <param name="row">
        /// The row of the element.
        /// </param>
        /// <param name="column">
        /// The column of the element.
        /// </param>
        /// <returns>
        /// The requested element.
        /// </returns>
        public override double At(int row, int column)
        {
            return Data[column * RowCount + row];
        }

        /// <summary>
        /// Sets the value of the given element.
        /// </summary>
        /// <param name="row">
        /// The row of the element.
        /// </param>
        /// <param name="column">
        /// The column of the element.
        /// </param>
        /// <param name="value">
        /// The value to set the element to.
        /// </param>
        public override void At(int row, int column, double value)
        {
            Data[column * RowCount + row] = value;
        }
    }
}
