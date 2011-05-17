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
    using Complex32 = Numerics.Complex32;

    /// <summary>
    /// User-defined vector implementation (internal class for testing purposes)
    /// </summary>
    internal class UserDefinedVector : Vector
    {
        /// <summary>
        /// Values storage
        /// </summary>
        private readonly Complex32[] _data;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserDefinedVector"/> class with a given size.
        /// </summary>
        /// <param name="size">The size of the vector.</param>
        public UserDefinedVector(int size) : base(size)
        {
            _data = new Complex32[size];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserDefinedVector"/> class for an array.
        /// </summary>
        /// <param name="data">The array to create this vector from.</param>
        public UserDefinedVector(Complex32[] data) : base(data.Length)
        {
            _data = (Complex32[])data.Clone();
        }

        /// <summary>Gets or sets the value at the given <paramref name="index"/>.</summary>
        /// <param name="index">The index of the value to get or set.</param>
        /// <returns>The value of the vector at the given <paramref name="index"/>.</returns> 
        public override Complex32 this[int index]
        {
            get
            {
                return _data[index];
            }

            set
            {
                _data[index] = value;
            }
        }

        /// <summary>Gets the value at the given <paramref name="index"/>.</summary>
        /// <param name="index">The index of the value to get or set.</param>
        /// <returns>The value of the vector at the given <paramref name="index"/>.</returns> 
        protected internal override Complex32 At(int index)
        {
            return this[index];
        }

        /// <summary>Sets the <paramref name="value"/> at the given <paramref name="index"/>.</summary>
        /// <param name="index">The index of the value to get or set.</param>
        /// <param name="value">The value to set.</param>
        protected internal override void At(int index, Complex32 value)
        {
            this[index] = value;
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
