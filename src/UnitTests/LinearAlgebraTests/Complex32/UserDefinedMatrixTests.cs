// <copyright file="UserDefinedMatrixTests.cs" company="Math.NET">
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

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Complex32
{
    using LinearAlgebra.Complex32;
    using LinearAlgebra.Generic;
    using Complex32 = Numerics.Complex32;

    internal class UserDefinedMatrix : Matrix
    {
        private readonly Complex32[,] _data;

        public UserDefinedMatrix(int order) : base(order, order)
        {
            _data = new Complex32[order, order];
        }

        public UserDefinedMatrix(int rows, int columns) : base(rows, columns)
        {
            _data = new Complex32[rows, columns];
        }

        public UserDefinedMatrix(Complex32[,] data) : base(data.GetLength(0), data.GetLength(1))
        {
            _data = (Complex32[,])data.Clone();
        }

        public override Complex32 At(int row, int column)
        {
            return _data[row, column];
        }

        public override void At(int row, int column, Complex32 value)
        {
            _data[row, column] = value;
        }

        public override Matrix<Complex32> CreateMatrix(int numberOfRows, int numberOfColumns)
        {
            return new UserDefinedMatrix(numberOfRows, numberOfColumns);
        }

        public override Vector<Complex32> CreateVector(int size)
        {
            return new UserDefinedVector(size);
        }

        public static UserDefinedMatrix Identity(int order)
        {
            var m = new UserDefinedMatrix(order, order);
            for (var i = 0; i < order; i++)
            {
                m[i, i] = 1.0f;
            }

            return m;
        }
    }

    public class UserDefinedMatrixTests : MatrixTests
    {
        protected override Matrix<Complex32> CreateMatrix(int rows, int columns)
        {
            return new UserDefinedMatrix(rows, columns);
        }

        protected override Matrix<Complex32> CreateMatrix(Complex32[,] data)
        {
            return new UserDefinedMatrix(data);
        }

        protected override Vector<Complex32> CreateVector(int size)
        {
            return new UserDefinedVector(size);
        }

        protected override Vector<Complex32> CreateVector(Complex32[] data)
        {
            return new UserDefinedVector(data);
        }
    }
}
