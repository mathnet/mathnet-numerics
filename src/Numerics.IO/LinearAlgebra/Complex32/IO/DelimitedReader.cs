// <copyright file="DelimitedReader.cs" company="Math.NET">
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

namespace MathNet.Numerics.LinearAlgebra.Complex32.IO
{
    using System;
    using Generic;
    using LinearAlgebra.IO;
    using Numerics;

    /// <summary>
    /// Creates a <see cref="Matrix{T}"/> from a delimited text file. If the user does not
    /// specify a delimiter, then any whitespace is used.
    /// </summary>
    /// <typeparam name="TMatrix">The type of the matrix to return.</typeparam>
    public class DelimitedReader<TMatrix> : DelimitedReader<TMatrix, Complex32>
        where TMatrix : Matrix<Complex32>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DelimitedReader{TMatrix}"/> class using
        /// any whitespace as the delimiter.
        /// </summary>
        public DelimitedReader() 
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DelimitedReader{TMatrix}"/> class.
        /// </summary>
        /// <param name="delimiter">The delimiter to use.</param>
        public DelimitedReader(char delimiter) : base(delimiter)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DelimitedReader{TMatrix}"/> class. 
        /// </summary>
        /// <param name="delimiter">
        /// The delimiter to use.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="delimiter"/> is <see langword="null"/>.
        /// </exception>
        public DelimitedReader(string delimiter) : base(delimiter)
        {
        }
    }
}
