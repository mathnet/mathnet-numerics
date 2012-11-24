// <copyright file="UserDefinedVector.cs" company="Math.NET">
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
    using LinearAlgebra.Storage;
    using Complex32 = Numerics.Complex32;

    /// <summary>
    /// User-defined vector implementation (internal class for testing purposes)
    /// </summary>
    internal class UserDefinedVector : Vector
    {
        class UserDefinedVectorStorage : VectorStorage<Complex32>
        {
            public readonly Complex32[] Data;

            public UserDefinedVectorStorage(int size)
                : base(size)
            {
                Data = new Complex32[size];
            }

            public UserDefinedVectorStorage(int size, Complex32[] data)
                : base(size)
            {
                Data = data;
            }

            public override Complex32 At(int index)
            {
                return Data[index];
            }

            public override void At(int index, Complex32 value)
            {
                Data[index] = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserDefinedVector"/> class with a given size.
        /// </summary>
        /// <param name="size">The size of the vector.</param>
        public UserDefinedVector(int size)
            : base(new UserDefinedVectorStorage(size))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserDefinedVector"/> class for an array.
        /// </summary>
        /// <param name="data">The array to create this vector from.</param>
        public UserDefinedVector(Complex32[] data)
            : base(new UserDefinedVectorStorage(data.Length, (Complex32[])data.Clone()))
        {
        }

        /// <summary>
        /// Creates a matrix with the given dimensions using the same storage type as this vector.
        /// </summary>
        /// <param name="rows">The number of rows.</param>
        /// <param name="columns">The number of columns.</param>
        /// <returns>A matrix with the given dimensions.</returns>
        public override Matrix<Complex32> CreateMatrix(int rows, int columns)
        {
            return new UserDefinedMatrix(rows, columns);
        }

        /// <summary>
        /// Creates a <strong>Vector</strong> of the given size using the same storage type as this vector.
        /// </summary>
        /// <param name="size">The size of the <strong>Vector</strong> to create.</param>
        /// <returns>The new <c>Vector</c>.</returns>
        public override Vector<Complex32> CreateVector(int size)
        {
            return new UserDefinedVector(size);
        }
    }
}
