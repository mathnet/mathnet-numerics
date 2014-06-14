// <copyright file="MatlabReader.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
//
// Copyright (c) 2009-2014 Math.NET
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Properties;

namespace MathNet.Numerics.Data.Matlab
{
    /// <summary>
    /// Creates matrices from Matlab files.
    /// </summary>
    /// <typeparam name="TDataType">The data type of the Matrix. It can be either: double, float, Complex, or Complex32.</typeparam>
    public class MatlabMatrixReader<TDataType> where TDataType : struct, IEquatable<TDataType>, IFormattable
    {
        /// <summary>
        /// The name of the file to read from.
        /// </summary>
        private readonly string _filename;

        /// <summary>
        /// The stream to read from if we are not reading from a file directly.
        /// </summary>
        private readonly Stream _stream;

        /// <summary>
        /// Initializes a new instance of the <see cref="MatlabMatrixReader{TDataType}"/> class.
        /// </summary>
        /// <param name="filename">Name of the file to read matrices from.</param>
        public MatlabMatrixReader(string filename)
        {
            if (string.IsNullOrEmpty(filename))
            {
                throw new ArgumentException(Resources.StringNullOrEmpty, "filename");
            }

            if (!File.Exists(filename))
            {
                throw new FileNotFoundException(Resources.FileDoesNotExist, "filename");
            }

            _filename = filename;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MatlabMatrixReader{TDataType}"/> class.
        /// </summary>
        /// <param name="stream">The stream to reader matrices from.</param>
        public MatlabMatrixReader(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            _stream = stream;
        }

        public static Matrix<TDataType> ReadMatrix(Stream stream, string matrixName = null)
        {
            var names = string.IsNullOrEmpty(matrixName) ? new string[] { } : new[] { matrixName };
            var parser = new MatlabParser<TDataType>(stream, names);
            var file = parser.Parse();

            if (string.IsNullOrEmpty(matrixName))
            {
                return file.FirstMatrix;
            }
            if (!file.Matrices.ContainsKey(matrixName))
            {
                throw new KeyNotFoundException("Matrix with the provided name was not found.");
            }
            return file.Matrices[matrixName];
        }

        public static Matrix<TDataType> ReadMatrix(string filePath, string matrixName = null)
        {
            using (var stream = File.OpenRead(filePath))
            {
                return ReadMatrix(stream, matrixName);
            }
        }

        public static Dictionary<string, Matrix<TDataType>> ReadMatrices(Stream stream, params string[] matrixNames)
        {
            var reader = new MatlabMatrixReader<TDataType>(stream);
            return reader.ReadMatrices(matrixNames);
        }

        public static Dictionary<string, Matrix<TDataType>> ReadMatrices(string filePath, params string[] matrixNames)
        {
            using (var stream = File.OpenRead(filePath))
            {
                return ReadMatrices(stream, matrixNames);
            }
        }

        /// <summary>
        /// Reads the first matrix from the file or stream.
        /// </summary>
        /// <returns>
        /// A sparse or dense matrix depending on how the matrix
        /// is defined in the Matlab file.
        /// </returns>
        public Matrix<TDataType> ReadMatrix()
        {
            return ReadMatrix(null);
        }

        /// <summary>
        /// Reads the named matrix from the file or stream.
        /// </summary>
        /// <param name="matrixName">The name of the matrix to read.</param>
        /// <returns>
        /// A sparse or dense matrix depending on how the matrix
        /// is defined in the Matlab file.
        /// <see langword="null"/> is returned if a matrix with the requests name doesn't exist.
        /// </returns>
        public Matrix<TDataType> ReadMatrix(string matrixName)
        {
            Stream stream;
            if (_filename == null)
            {
                stream = _stream;
                _stream.Seek(0, SeekOrigin.Begin);
            }
            else
            {
                stream = new FileStream(_filename, FileMode.Open, FileAccess.Read);
            }

            var names = string.IsNullOrEmpty(matrixName) ? new string[] { } : new[] { matrixName };
            var parser = new MatlabParser<TDataType>(stream, names);
            var file = parser.Parse();

            Matrix<TDataType> matrix = null;
            if (string.IsNullOrEmpty(matrixName))
            {
                matrix = file.FirstMatrix;
            }
            else if (file.Matrices.ContainsKey(matrixName))
            {
                matrix = file.Matrices[matrixName];
            }

            if (_filename != null)
            {
                stream.Close();
                stream.Dispose();
            }

            return matrix;
        }

        /// <summary>
        /// Reads all matrices from the file or stream.
        /// </summary>
        /// <returns>All matrices from the file or stream. The key to the <see cref="IDictionary{T,K}"/>
        /// is the matrix's name.</returns>
        public Dictionary<string, Matrix<TDataType>> ReadMatrices()
        {
            return ReadMatrices(new string[] { });
        }

        /// <summary>
        /// Reads the named matrices from the file or stream.
        /// </summary>
        /// <param name="names">The names of the matrices to retrieve.</param>
        /// <returns>
        /// The named matrices from the file or stream. The key to the <see cref="IDictionary{T,K}"/>
        /// is the matrix's name.</returns>
        public Dictionary<string, Matrix<TDataType>> ReadMatrices(IEnumerable<string> names)
        {
            Stream stream;
            if (_filename == null)
            {
                stream = _stream;
                _stream.Seek(0, SeekOrigin.Begin);
            }
            else
            {
                stream = new BufferedStream(new FileStream(_filename, FileMode.Open, FileAccess.Read));
            }

            var parser = new MatlabParser<TDataType>(stream, names);
            var file = parser.Parse();

            if (_filename != null)
            {
                stream.Close();
                stream.Dispose();
            }

            return file.Matrices.ToDictionary(matrix => matrix.Key, matrix => matrix.Value);
        }
    }
}
