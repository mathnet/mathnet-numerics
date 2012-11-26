// <copyright file="SquareMatrix.cs" company="Math.NET">
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

namespace MathNet.Numerics.LinearAlgebra.Complex
{ 
    using System;
    using System.Numerics;
    using Properties;
    using Storage;

    /// <summary>
    /// Abstract class for square matrices. 
    /// </summary>
    [Serializable]
    public abstract class SquareMatrix : Matrix
    { 
        /// <summary>
        ///   Number of rows or columns.
        /// </summary>
        protected readonly int Order;

        /// <summary>
        /// Initializes a new instance of the <see cref="SquareMatrix"/> class. 
        /// </summary>
        /// <exception cref="ArgumentException">
        /// If the matrix is not square.
        /// </exception>
        protected SquareMatrix(MatrixStorage<Complex> storage)
            : base(storage)
        {
            if (storage.RowCount != storage.ColumnCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixSquare);
            }

            Order = storage.RowCount;
        }
    }
}
