// <copyright file="MatrixReader.cs" company="Math.NET">
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

namespace MathNet.Numerics.LinearAlgebra.IO
{
    using System;
    using System.IO;
    using Generic;
    using Properties;

    /// <summary>
    /// Base class to read a single <see cref="Matrix{T}"/> from a file or stream.
    /// </summary>
    /// <typeparam name="TMatrix">The type of Matrix to return.</typeparam>
    /// <typeparam name="TDataType">The data type of the Matrix. It can be either: double, float, Complex, or Complex32.</typeparam>
    public abstract class MatrixReader<TMatrix, TDataType>
        where TMatrix : Matrix<TDataType>
        where TDataType : struct, IEquatable<TDataType>, IFormattable
    {
        /// <summary>
        /// Reads a <see cref="Matrix{T}"/> from a file.
        /// </summary>
        /// <param name="file">The file to read the matrix from.</param>
        /// <returns>A <see cref="Matrix{T}"/> containing the data from the file. <see langword="null" /> is returned if the file is empty.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="file"/> is <see langword="null" />.</exception>
        /// <exception cref="IOException">If the file doesn't exist.</exception>
        /// <exception cref="FormatException">If a value is not a number or not in a valid format.</exception>
        /// <exception cref="OverflowException">If a value represents a number less than <see cref="Double.MinValue"/> or greater than <see cref="Double.MaxValue"/>.</exception>
        public TMatrix ReadMatrix(string file)
        {
            if (file == null)
            {
                throw new ArgumentNullException("file", Resources.StringNullOrEmpty);
            }

            return ReadMatrix(File.OpenRead(file));
        }

        /// <summary>
        /// Reads a <see cref="Matrix{T}"/> from a <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to read the matrix from.</param>
        /// <returns>A matrix containing the data from the <see cref="Stream"/>. <see langword="null" /> is returned if the <see cref="Stream"/> is empty.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="stream"/> is <see langword="null" />.</exception>
        /// <exception cref="FormatException">If a value is not a number or not in a valid format.</exception>
        /// <exception cref="OverflowException">If a value represents a number less than <see cref="Double.MinValue"/> or greater than <see cref="Double.MaxValue"/>.</exception>
        public TMatrix ReadMatrix(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            return DoReadMatrix(stream);
        }

        /// <summary>
        /// Subclasses override this method to do the actual reading.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to read the matrix from.</param>
        /// <returns>A matrix containing the data from the <see cref="Stream"/>. <see langword="null" /> is returned if the <see cref="Stream"/> is empty.</returns>
        protected abstract TMatrix DoReadMatrix(Stream stream);
    }
}
