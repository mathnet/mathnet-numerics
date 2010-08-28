// <copyright file="MatrixWriter.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
//
// Copyright (c) 2009-2010 Math.NET
//
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

using System;
using System.IO;

namespace MathNet.Numerics.LinearAlgebra.Double.IO
{
    using Generic;

    /// <summary>
    /// Base class to write a single <see cref="Matrix{T}"/> to a file or stream.
    /// </summary>
    public abstract class MatrixWriter 
    {
        /// <summary>
        /// Writes the given <see cref="Matrix{T}"/> to the given file. If the file already exists, 
        /// the file will be overwritten.
        /// </summary>
        /// <param name="matrix">The matrix to write.</param>
        /// <param name="file">The file to write the matrix to.</param>
        /// <exception cref="ArgumentNullException">If either <paramref name="matrix"/> or <paramref name="file"/> is <c>null</c>.</exception>
        public void WriteMatrix(Matrix<double> matrix, string file)
        {
            if (matrix == null)
            {
                throw new ArgumentNullException("matrix");
            }

            if (file == null)
            {
                throw new ArgumentNullException("file");
            }

            using (var writer = new StreamWriter(file))
            {
                DoWriteMatrix(matrix, writer, null);
            }
        }

        /// <summary>
        /// Writes the given <see cref="Matrix{T}"/> to the given file. If the file already exists, 
        /// the file will be overwritten.
        /// </summary>
        /// <param name="matrix">the matrix to write.</param>
        /// <param name="file">The file to write the matrix to.</param>
        /// <param name="format">The format to use on each element.</param>
        /// <exception cref="ArgumentNullException">If either <paramref name="matrix"/> or <paramref name="file"/> is <c>null</c>.</exception>
        public void WriteMatrix(Matrix<double> matrix, string file, string format)
        {
            if (matrix == null)
            {
                throw new ArgumentNullException("matrix");
            }

            if (file == null)
            {
                throw new ArgumentNullException("file");
            }

            if (File.Exists(file))
            {
                File.Delete(file);
            }

            using (var writer = new StreamWriter(file))
            {
                DoWriteMatrix(matrix, writer, format);
            }
        }

        /// <summary>
        /// Writes the given <see cref="Matrix{T}"/> to the given stream.
        /// </summary>
        /// <param name="matrix">The matrix to write.</param>
        /// <param name="stream">The <see cref="Stream"/> to write the matrix to.</param>
        /// <exception cref="ArgumentNullException">If either <paramref name="matrix"/> or <paramref name="stream"/> is <c>null</c>.</exception>
        public void WriteMatrix(Matrix<double> matrix, Stream stream)
        {
            if (matrix == null)
            {
                throw new ArgumentNullException("matrix");
            }

            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            using (var writer = new StreamWriter(stream))
            {
                DoWriteMatrix(matrix, writer, null);
            }
        }

        /// <summary>
        /// Writes the given <see cref="Matrix{T}"/> to the given stream.
        /// </summary>
        /// <param name="matrix">The <see cref="TextWriter"/> to write.</param>
        /// <param name="stream">The <see cref="Stream"/> to write the matrix to.</param>
        /// <param name="format">The format to use on each element.</param>
        /// <exception cref="ArgumentNullException">If either <paramref name="matrix"/> or <paramref name="stream"/> is <c>null</c>.</exception>
        public void WriteMatrix(Matrix<double> matrix, Stream stream, string format)
        {
            if (matrix == null)
            {
                throw new ArgumentNullException("matrix");
            }

            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            using (var writer = new StreamWriter(stream))
            {
                DoWriteMatrix(matrix, writer, format);
            }
        }

        /// <summary>
        /// Writes the given <see cref="Matrix{T}"/> to the given <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="matrix">The matrix to write.</param>
        /// <param name="writer">The <see cref="TextWriter"/> to write the matrix to.</param>
        /// <exception cref="ArgumentNullException">If either <paramref name="matrix"/> or <paramref name="writer"/> is <c>null</c>.</exception>
        public void WriteMatrix(Matrix<double> matrix, TextWriter writer)
        {
            if (matrix == null)
            {
                throw new ArgumentNullException("matrix");
            }

            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }

            DoWriteMatrix(matrix, writer, null);
        }

        /// <summary>
        /// Writes the given <see cref="Matrix{T}"/> to the given <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="matrix">The matrix to write.</param>
        /// <param name="writer">The <see cref="TextWriter"/> to write the matrix to.</param>
        /// <param name="format">The format to use on each element.</param>
        /// <exception cref="ArgumentNullException">If either <paramref name="matrix"/> or <paramref name="writer"/> is <c>null</c>.</exception>
        public void WriteMatrix(Matrix<double> matrix, TextWriter writer, string format)
        {
            if (matrix == null)
            {
                throw new ArgumentNullException("matrix");
            }

            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }

            DoWriteMatrix(matrix, writer, format);
        }

        /// <summary>
        /// Subclasses must implement this method to do the actually writing.
        /// </summary>
        /// <param name="matrix">The matrix to serialize.</param>
        /// <param name="writer">The <see cref="TextWriter"/> to write the matrix to.</param>
        /// <param name="format">The format for the new matrix.</param>
        protected abstract void DoWriteMatrix(Matrix<double> matrix, TextWriter writer, string format);
    }
}
