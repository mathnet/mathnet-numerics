// <copyright file="UserDefinedMatrix.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
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

using MathNet.Numerics.LinearAlgebra.Complex32;
using MathNet.Numerics.LinearAlgebra.Storage;

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Complex32
{
    using Numerics;

    /// <summary>
    /// User-defined matrix implementation (internal class for testing purposes)
    /// </summary>
    internal class UserDefinedMatrix : Matrix
    {
        class UserDefinedMatrixStorage : MatrixStorage<Complex32>
        {
            public readonly Complex32[,] Data;

            public UserDefinedMatrixStorage(int rowCount, int columnCount)
                : base(rowCount, columnCount)
            {
                Data = new Complex32[rowCount, columnCount];
            }

            public UserDefinedMatrixStorage(int rowCount, int columnCount, Complex32[,] data)
                : base(rowCount, columnCount)
            {
                Data = data;
            }

            public override bool IsDense
            {
                get { return true; }
            }

            public override bool IsFullyMutable
            {
                get { return true; }
            }

            public override bool IsMutableAt(int row, int column)
            {
                return true;
            }

            public override Complex32 At(int row, int column)
            {
                return Data[row, column];
            }

            public override void At(int row, int column, Complex32 value)
            {
                Data[row, column] = value;
            }

            public override Complex32[,] AsArray()
            {
                return Data;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserDefinedMatrix"/> class. This matrix is square with a given size.
        /// </summary>
        /// <param name="order">the size of the square matrix.</param>
        public UserDefinedMatrix(int order)
            : base(new UserDefinedMatrixStorage(order, order))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserDefinedMatrix"/> class.
        /// </summary>
        /// <param name="rows">The number of rows.</param>
        /// <param name="columns">The number of columns.</param>
        public UserDefinedMatrix(int rows, int columns)
            : base(new UserDefinedMatrixStorage(rows, columns))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserDefinedMatrix"/> class from a 2D array. 
        /// </summary>
        /// <param name="data">The 2D array to create this matrix from.</param>
        public UserDefinedMatrix(Complex32[,] data)
            : base(new UserDefinedMatrixStorage(data.GetLength(0), data.GetLength(1), (Complex32[,])data.Clone()))
        {
        }

        /// <summary>
        /// Initializes a square matrix with all zero's except for ones on the diagonal.
        /// </summary>
        /// <param name="order">the size of the square matrix.</param>
        /// <returns>A identity matrix.</returns>
        public static UserDefinedMatrix Identity(int order)
        {
            var m = new UserDefinedMatrix(order, order);
            for (var i = 0; i < order; i++)
            {
                m[i, i] = 1f;
            }

            return m;
        }
    }
}
