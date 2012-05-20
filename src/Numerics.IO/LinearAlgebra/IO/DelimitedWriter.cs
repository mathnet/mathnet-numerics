// <copyright file="DelimitedWriter.cs" company="Math.NET">
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

namespace MathNet.Numerics.LinearAlgebra.IO
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using Generic;

    /// <summary>
    /// Writes an <see cref="Matrix{TDataType}"/> to delimited text file. If the user does not
    /// specify a delimiter, a tab separator is used.
    /// </summary>
    public class DelimitedWriter : MatrixWriter
    {
        /// <summary>
        /// The delimiter to use.
        /// </summary>
        private readonly string _delimiter;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelimitedWriter"/> class. 
        /// a comma as the delimiter.
        /// </summary>
        public DelimitedWriter()
        {
            _delimiter = ",";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DelimitedWriter"/> class. 
        /// using the given delimiter.
        /// </summary>
        /// <param name="delimiter">
        /// the delimiter to use.
        /// </param>
        public DelimitedWriter(char delimiter)
        {
            _delimiter = new string(delimiter, 1);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DelimitedWriter"/> class. 
        /// using the given delimiter.
        /// </summary>
        /// <param name="delimiter">
        /// the delimiter to use.
        /// </param>
        public DelimitedWriter(string delimiter)
        {
            _delimiter = delimiter;
        }

        /// <summary>
        /// Gets or sets the column header values.
        /// </summary>
        /// <value>The column header values.</value>
        /// <remarks>Will write the column headers if the list is not empty or <c>null</c>.</remarks>
        public IList<string> ColumnHeaders
        {
            get;
            set;
        }

        /// <summary>
        /// Writes the given <see cref="Matrix{TDataType}"/> to the given <see cref="TextWriter"/>.
        /// </summary>
        /// <typeparam name="TDataType">The data type of the Matrix. It can be either: double, float, Complex, or Complex32.</typeparam>
        /// <param name="matrix">The matrix to write.</param>
        /// <param name="writer">The <see cref="TextWriter"/> to write the matrix to.</param>
        /// <param name="format">The number format to use on each element.</param>
        /// <param name="cultureInfo">The culture to use.</param>
        /// <exception cref="ArgumentNullException">If either <paramref name="matrix"/> or <paramref name="writer"/> is <c>null</c>.</exception>
        protected override void DoWriteMatrix<TDataType>(Matrix<TDataType> matrix, TextWriter writer, string format, CultureInfo cultureInfo)
        {
            if (matrix == null)
            {
                throw new ArgumentNullException("matrix");
            }

            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }

            if (ColumnHeaders != null && ColumnHeaders.Count > 0)
            {
                for (var i = 0; i < ColumnHeaders.Count - 1; i++)
                {
                    writer.Write(ColumnHeaders[i]);
                    writer.Write(_delimiter);
                }

                writer.WriteLine(ColumnHeaders[ColumnHeaders.Count - 1]);
            }

            var cols = matrix.ColumnCount - 1;
            var rows = matrix.RowCount - 1;
            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    writer.Write(matrix[i, j].ToString(format, cultureInfo));
                    if (j != cols)
                    {
                        writer.Write(_delimiter);
                    }
                }

                if (i != rows)
                {
                    writer.Write(Environment.NewLine);
                }
            }
        }
    }
}
