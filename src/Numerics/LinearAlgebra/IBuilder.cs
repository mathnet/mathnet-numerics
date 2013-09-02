// <copyright file="IBuilder.cs" company="Math.NET">
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

using System;

namespace MathNet.Numerics.LinearAlgebra
{
    public interface IBuilder<T> where T : struct, IEquatable<T>, IFormattable
    {
        /// <summary>
        /// Creates a <strong>Matrix</strong> for the given number of rows and columns.
        /// </summary>
        /// <param name="numberOfRows">The number of rows.</param>
        /// <param name="numberOfColumns">The number of columns.</param>
        /// <param name="fullyMutable">True if all fields must be mutable (e.g. not a diagonal matrix).</param>
        /// <returns>
        /// A <strong>Matrix</strong> with the given dimensions.
        /// </returns>
        /// <remarks>
        /// Creates a matrix of the same matrix type as the current matrix.
        /// </remarks>
        Matrix<T> CreateMatrix(int numberOfRows, int numberOfColumns, bool fullyMutable = false);

        /// <summary>
        /// Creates a Vector with a the given dimension.
        /// </summary>
        /// <param name="size">The size of the vector.</param>
        /// <param name="fullyMutable">True if all fields must be mutable.</param>
        /// <returns>
        /// A Vector with the given dimension.
        /// </returns>
        /// <remarks>
        /// Creates a vector of the same type as the current matrix.
        /// </remarks>
        Vector<T> CreateVector(int size, bool fullyMutable = false);
    }
}
