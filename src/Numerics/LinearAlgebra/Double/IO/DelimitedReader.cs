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

namespace MathNet.Numerics.LinearAlgebra.Double.IO
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using Generic;

    /// <summary>
    /// Creates a <see cref="Matrix{T}"/> from a delimited text file. If the user does not
    /// specify a delimiter, then any whitespace is used.
    /// </summary>
    /// <typeparam name="TMatrix">The type of the matrix to return.</typeparam>
    public class DelimitedReader<TMatrix> : MatrixReader<TMatrix>
        where TMatrix : Matrix<double>
    {
        /// <summary>
        /// Constructor to create matrix instance.
        /// </summary>
        private static readonly ConstructorInfo Constructor = typeof(TMatrix).GetConstructor(new[] { typeof(int), typeof(int) });

        /// <summary>
        /// Whitespace regular expression.
        /// </summary>
        private static readonly Regex WhiteSpace = new Regex(@"[\s]+");

        /// <summary>
        /// The delimiter to use.
        /// </summary>
        private readonly Regex _delimiter;

        /// <summary>
        /// The <see cref="CultureInfo"/> to use.
        /// </summary>
        private CultureInfo _cultureInfo = CultureInfo.CurrentCulture;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelimitedReader{TMatrix}"/> class using
        /// any whitespace as the delimiter.
        /// </summary>
        public DelimitedReader()
        {
            _delimiter = WhiteSpace;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DelimitedReader{TMatrix}"/> class.
        /// </summary>
        /// <param name="delimiter">The delimiter to use.</param>
        public DelimitedReader(char delimiter)
        {
            _delimiter = new Regex("[" + new string(delimiter, 1) + "]+");
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
        public DelimitedReader(string delimiter)
        {
            if (delimiter == null)
            {
                throw new ArgumentNullException("delimiter");
            }

            _delimiter = new Regex("[" + delimiter + "]+");
        }

        /// <summary>
        /// Gets or sets the <see cref="CultureInfo"/> to use when parsing the numbers.
        /// </summary>
        /// <value>The culture info.</value>
        /// <remarks>Defaults to <c>CultureInfo.CurrentCulture</c>.</remarks>
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
        /// Gets or sets a value indicating whether the files has a header row.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has a header row; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>Defaults to <see langword="false"/>.</remarks>
        public bool HasHeaderRow
        {
            get;
            set;
        }

        /// <summary>
        /// Performs the actual reading.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to read the matrix from.</param>
        /// <returns>
        /// A matrix containing the data from the <see cref="Stream"/>. <see langword="null"/> is returned if the <see cref="Stream"/> is empty.
        /// </returns>
        protected override TMatrix DoReadMatrix(Stream stream)
        {
            var data = new List<string[]>();

            // max is used to supports files like:
            // 1,2
            // 3,4,5,6
            // 7
            // this creates a 3x4 matrix:
            // 1, 2, 0 ,0 
            // 3, 4, 5, 6
            // 7, 0, 0, 0
            var max = -1;
            var reader = new StreamReader(stream);
            var line = reader.ReadLine();
            if (HasHeaderRow)
            {
                line = reader.ReadLine();
            }

            while (line != null)
            {
                line = line.Trim();
                if (line.Length > 0)
                {
                    var row = _delimiter.Split(line);
                    max = Math.Max(max, row.Length);
                    data.Add(row);
                }

                line = reader.ReadLine();
            }

            var ret = (TMatrix)Constructor.Invoke(new object[] { data.Count, max });
            
            if (data.Count != 0)
            {
                for (var i = 0; i < data.Count; i++)
                {
                    var row = data[i];
                    for (var j = 0; j < row.Length; j++)
                    {
                        // strip off quotes
                        var value = row[j].Replace("'", string.Empty).Replace("\"", string.Empty);
                        ret[i, j] = double.Parse(value, NumberStyles.Any, _cultureInfo);
                    }
                }
            }

            reader.Close();
            reader.Dispose();
            return ret;
        }
    }
}
