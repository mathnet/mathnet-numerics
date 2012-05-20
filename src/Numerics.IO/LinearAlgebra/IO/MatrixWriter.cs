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

namespace MathNet.Numerics.LinearAlgebra.IO
{
    using System.Globalization;
    using Generic;

    /// <summary>
    /// Base class to write a single <see cref="Matrix{DataType}"/> to a file or stream.
    /// </summary>
    public abstract class MatrixWriter
    {
        /// <summary>
        /// The <see cref="CultureInfo"/> to use.
        /// </summary>
        private CultureInfo _cultureInfo = CultureInfo.CurrentCulture;

        /// <summary>
        /// Gets or sets the <see cref="CultureInfo"/> to use when parsing the numbers.
        /// </summary>
        /// <value>The culture info.</value>
        /// <remarks>Defaults to <c>CultureInfo.CurrentCulture</c>.</remarks>
        /// <remarks>This property is only used for matrix writers that write out text files.</remarks>
        public CultureInfo CultureInfo
        {
            get
            {
                return _cultureInfo;
            }

            set
            {
                if (value != null)
                {
                    _cultureInfo = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets he number format to use.
        /// </summary>
        /// <value>The number format to use when writing out each element.</value>
        /// <remarks>This property is only used for matrix writers that write out text files.</remarks>
        public string Format
        {
            get;
            set;
        }
        
        /// <summary>
        /// Writes the given <see cref="Matrix{DataType}"/> to the given file. If the file already exists, 
        /// the file will be overwritten.
        /// </summary>
        /// <param name="matrix">The matrix to write.</param>
        /// <param name="file">The file to write the matrix to.</param>
        /// <exception cref="ArgumentNullException">If either <paramref name="matrix"/> or <paramref name="file"/> is <c>null</c>.</exception>
        /// <typeparam name="TDataType">The data type of the Matrix. It can be either: double, float, Complex, or Complex32.</typeparam>
        public void WriteMatrix<TDataType>(Matrix<TDataType> matrix, string file) where TDataType : struct, IEquatable<TDataType>, IFormattable
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
                DoWriteMatrix(matrix, writer, Format, _cultureInfo);
            }
        }

        /// <summary>
        /// Writes the given <see cref="Matrix{DataType}"/> to the given stream.
        /// </summary>
        /// <param name="matrix">The matrix to write.</param>
        /// <param name="stream">The <see cref="Stream"/> to write the matrix to.</param>
        /// <exception cref="ArgumentNullException">If either <paramref name="matrix"/> or <paramref name="stream"/> is <c>null</c>.</exception>
        /// <typeparam name="TDataType">The data type of the Matrix. It can be either: double, float, Complex, or Complex32.</typeparam>
        public void WriteMatrix<TDataType>(Matrix<TDataType> matrix, Stream stream) where TDataType : struct, IEquatable<TDataType>, IFormattable
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
                DoWriteMatrix(matrix, writer, Format, _cultureInfo);
            }
        }

        /// <summary>
        /// Writes the given <see cref="Matrix{DataType}"/> to the given <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="matrix">The matrix to write.</param>
        /// <param name="writer">The <see cref="TextWriter"/> to write the matrix to.</param>
        /// <exception cref="ArgumentNullException">If either <paramref name="matrix"/> or <paramref name="writer"/> is <c>null</c>.</exception>
        /// <typeparam name="TDataType">The data type of the Matrix. It can be either: double, float, Complex, or Complex32.</typeparam>
        public void WriteMatrix<TDataType>(Matrix<TDataType> matrix, TextWriter writer) where TDataType : struct, IEquatable<TDataType>, IFormattable
        {
            if (matrix == null)
            {
                throw new ArgumentNullException("matrix");
            }

            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }

            DoWriteMatrix(matrix, writer, Format, _cultureInfo);
        }

        /// <summary>
        /// Subclasses must implement this method to do the actually writing.
        /// </summary>
        /// <typeparam name="TDataType">The data type of the Matrix. It can be either: double, float, Complex, or Complex32.</typeparam>
        /// <param name="matrix">The matrix to serialize.</param>
        /// <param name="writer">The <see cref="TextWriter"/> to write the matrix to.</param>
        /// <param name="format">The number format to use.</param>
        /// <param name="cultureInfo">The culture to use.</param>
        protected abstract void DoWriteMatrix<TDataType>(Matrix<TDataType> matrix, TextWriter writer, string format, CultureInfo cultureInfo) where TDataType : struct, IEquatable<TDataType>, IFormattable;
    }
}
