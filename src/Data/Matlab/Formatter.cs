// <copyright file="Formatter.cs" company="Math.NET">
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
using System.IO.Compression;
using System.Numerics;
using System.Text;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Storage;
using MathNet.Numerics.Properties;

namespace MathNet.Numerics.Data.Matlab
{
    /// <summary>
    /// Format a MATLAB file.
    /// </summary>
    internal static class Formatter
    {
        /// <summary>
        /// The file header value
        /// </summary>
        const string HeaderText = "MATLAB 5.0 MAT-file, Platform: .NET 4 - Math.NET Numerics, Created on: ";

        /// <summary>
        /// The length of the header text.
        /// </summary>
        const int HeaderTextLength = 116;

        /// <summary>
        /// Format a matrix block byte array
        /// </summary>
        internal static MatlabMatrix FormatMatrix<T>(Matrix<T> matrix, string name)
            where T : struct, IEquatable<T>, IFormattable
        {
            if (matrix == null)
            {
                throw new ArgumentNullException("matrix");
            }

            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException(Resources.StringNullOrEmpty, "name");
            }

            if (name.IndexOf(' ') > -1)
            {
                throw new ArgumentException(string.Format(Resources.NameCannotContainASpace, name), "name");
            }

            if (typeof(T) == typeof(double))
            {
                var sparse = matrix as LinearAlgebra.Double.SparseMatrix;
                return sparse != null
                    ? GetSparseDataArray(sparse, name)
                    : GetDenseDataArray((LinearAlgebra.Double.Matrix)(object)matrix, name);
            }

            if (typeof(T) == typeof(float))
            {
                var sparse = matrix as LinearAlgebra.Single.SparseMatrix;
                return sparse != null
                    ? GetSparseDataArray(sparse, name)
                    : GetDenseDataArray((LinearAlgebra.Single.Matrix)(object)matrix, name);
            }

            if (typeof(T) == typeof(Complex))
            {
                var sparse = matrix as LinearAlgebra.Complex.SparseMatrix;
                return sparse != null
                    ? GetSparseDataArray(sparse, name)
                    : GetDenseDataArray((LinearAlgebra.Complex.Matrix)(object)matrix, name);
            }

            if (typeof(T) == typeof(Complex32))
            {
                var sparse = matrix as LinearAlgebra.Complex32.SparseMatrix;
                return sparse != null
                    ? GetSparseDataArray(sparse, name)
                    : GetDenseDataArray((LinearAlgebra.Complex32.Matrix)(object)matrix, name);
            }

            throw new NotSupportedException();
        }

        /// <summary>
        /// Writes all matrix blocks to a stream.
        /// </summary>
        internal static void FormatFile(Stream stream, IEnumerable<MatlabMatrix> matrices)
        {
            using (var buffer = new BufferedStream(stream))
            using (var writer = new BinaryWriter(buffer))
            {
                WriteHeader(writer);

                foreach (var matrix in matrices)
                {
                    // write data type
                    writer.Write((int)DataType.Compressed);

                    WriteCompressedData(writer, matrix.Data);
                }

                writer.Flush();
                writer.Close();
            }
        }

        /// <summary>
        /// Writes the matrix tag and name.
        /// </summary>
        /// <param name="writer">The writer we are using.</param>
        /// <param name="arrayClass">The array class we are writing.</param>
        /// <param name="isComplex">if set to <c>true</c> if this a complex matrix.</param>
        /// <param name="name">The name of the matrix.</param>
        /// <param name="rows">The number of rows.</param>
        /// <param name="columns">The columns of columns.</param>
        /// <param name="nzmax">The maximum number of non-zero elements.</param>
        static void WriteMatrixTagAndName(BinaryWriter writer, ArrayClass arrayClass, bool isComplex,
            string name, int rows, int columns, int nzmax)
        {
            writer.Write((int)DataType.Matrix);

            // add place holder for data size
            writer.Write(0);

            // write flag, data type and size
            writer.Write((int)DataType.UInt32);
            writer.Write(8);

            // write array class and flags
            writer.Write((byte)arrayClass);
            if (isComplex)
            {
                writer.Write((byte)ArrayFlags.Complex);
            }
            else
            {
                writer.Write((byte)0);
            }

            writer.Write((short)0);
            writer.Write(nzmax);

            // write dimensions
            writer.Write((int)DataType.Int32);
            writer.Write(8);
            writer.Write(rows);
            writer.Write(columns);

            var nameBytes = Encoding.ASCII.GetBytes(name);

            // write name
            if (nameBytes.Length > 4)
            {
                writer.Write((int)DataType.Int8);
                writer.Write(nameBytes.Length);
                writer.Write(nameBytes);
                var pad = 8 - (nameBytes.Length%8);
                PadData(writer, pad);
            }
            else
            {
                writer.Write((short)DataType.Int8);
                writer.Write((short)nameBytes.Length);
                writer.Write(nameBytes);
                PadData(writer, 4 - nameBytes.Length);
            }
        }

        /// <summary>
        /// Gets the dense data array.
        /// </summary>
        /// <param name="matrix">The matrix to get the data from.</param>
        /// <param name="name">The name of the matrix.</param>
        /// <returns>The matrix data as an array.</returns>
        static MatlabMatrix GetDenseDataArray(Matrix<double> matrix, string name)
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                WriteMatrixTagAndName(writer, ArrayClass.Double, false, name, matrix.RowCount, matrix.ColumnCount, 0);

                // write data
                writer.Write((int)DataType.Double);
                writer.Write(matrix.RowCount*matrix.ColumnCount*8);

                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    var column = matrix.Column(j);
                    foreach (var value in column)
                    {
                        writer.Write(value);
                    }
                }

                writer.Flush();
                return new MatlabMatrix(name, stream.ToArray());
            }

        }

        /// <summary>
        /// Gets the dense data array.
        /// </summary>
        /// <param name="matrix">The matrix to get the data from.</param>
        /// <param name="name">The name of the matrix.</param>
        /// <returns>The matrix data as an array.</returns>
        static MatlabMatrix GetDenseDataArray(Matrix<float> matrix, string name)
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                WriteMatrixTagAndName(writer, ArrayClass.Single, false, name, matrix.RowCount, matrix.ColumnCount, 0);

                // write data
                int size = matrix.RowCount*matrix.ColumnCount*4;
                writer.Write((int)DataType.Single);
                writer.Write(size);

                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    var column = matrix.Column(j);
                    foreach (var value in column)
                    {
                        writer.Write(value);
                    }
                }

                PadData(writer, size%8);

                writer.Flush();
                return new MatlabMatrix(name, stream.ToArray());
            }
        }

        /// <summary>
        /// Gets the dense data array.
        /// </summary>
        /// <param name="matrix">The matrix to get the data from.</param>
        /// <param name="name">The name of the matrix.</param>
        /// <returns>The matrix data as an array.</returns>
        static MatlabMatrix GetDenseDataArray(Matrix<Complex> matrix, string name)
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                WriteMatrixTagAndName(writer, ArrayClass.Double, true, name, matrix.RowCount, matrix.ColumnCount, 0);

                // write data
                int size = matrix.RowCount*matrix.ColumnCount*8;
                writer.Write((int)DataType.Double);
                writer.Write(size);

                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    var column = matrix.Column(j);
                    foreach (var value in column)
                    {
                        writer.Write(value.Real);
                    }
                }

                writer.Write((int)DataType.Double);
                writer.Write(size);

                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    var column = matrix.Column(j);
                    foreach (var value in column)
                    {
                        writer.Write(value.Imaginary);
                    }
                }

                writer.Flush();
                return new MatlabMatrix(name, stream.ToArray());
            }
        }

        /// <summary>
        /// Gets the dense data array.
        /// </summary>
        /// <param name="matrix">The matrix to get the data from.</param>
        /// <param name="name">The name of the matrix.</param>
        /// <returns>The matrix data as an array.</returns>
        static MatlabMatrix GetDenseDataArray(Matrix<Complex32> matrix, string name)
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                WriteMatrixTagAndName(writer, ArrayClass.Single, true, name, matrix.RowCount, matrix.ColumnCount, 0);

                // write data
                int size = matrix.RowCount*matrix.ColumnCount*4;
                writer.Write((int)DataType.Single);
                writer.Write(size);

                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    var column = matrix.Column(j);
                    foreach (var value in column)
                    {
                        writer.Write(value.Real);
                    }
                }

                PadData(writer, size%8);

                writer.Write((int)DataType.Single);
                writer.Write(size);

                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    var column = matrix.Column(j);
                    foreach (var value in column)
                    {
                        writer.Write(value.Real);
                    }
                }

                PadData(writer, size%8);

                writer.Flush();
                return new MatlabMatrix(name, stream.ToArray());
            }
        }

        /// <summary>
        /// Gets the sparse data array.
        /// </summary>
        /// <param name="matrix">The matrix to get the data from.</param>
        /// <param name="name">The name of the matrix.</param>
        /// <returns>The matrix data as an array.</returns>
        static MatlabMatrix GetSparseDataArray(LinearAlgebra.Double.SparseMatrix matrix, string name)
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                var nzmax = matrix.NonZerosCount;
                WriteMatrixTagAndName(writer, ArrayClass.Sparse, false, name, matrix.RowCount, matrix.ColumnCount, nzmax);

                // write ir
                writer.Write((int)DataType.Int32);
                writer.Write(nzmax*4);

                foreach (var column in matrix.EnumerateColumns())
                {
                    foreach (var row in column.EnumerateNonZeroIndexed())
                    {
                        writer.Write(row.Item1);
                    }
                }

                // add pad if needed
                if (nzmax%2 == 1)
                {
                    writer.Write(0);
                }

                // write jc
                writer.Write((int)DataType.Int32);
                writer.Write((matrix.ColumnCount + 1)*4);
                writer.Write(0);
                var count = 0;
                foreach (var column in matrix.EnumerateColumns())
                {
                    count += ((SparseVectorStorage<double>)column.Storage).ValueCount;
                    writer.Write(count);
                }

                // add pad if needed
                if (matrix.ColumnCount%2 == 0)
                {
                    writer.Write(0);
                }

                // write data
                writer.Write((int)DataType.Double);
                writer.Write(nzmax*8);

                foreach (var column in matrix.EnumerateColumns())
                {
                    foreach (var row in column.EnumerateNonZeroIndexed())
                    {
                        writer.Write(row.Item2);
                    }
                }

                writer.Flush();
                return new MatlabMatrix(name, stream.ToArray());
            }
        }

        /// <summary>
        /// Gets the sparse data array.
        /// </summary>
        /// <param name="matrix">The matrix to get the data from.</param>
        /// <param name="name">The name of the matrix.</param>
        /// <returns>The matrix data as an array.</returns>
        static MatlabMatrix GetSparseDataArray(LinearAlgebra.Single.SparseMatrix matrix, string name)
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                var nzmax = matrix.NonZerosCount;
                WriteMatrixTagAndName(writer, ArrayClass.Sparse, false, name, matrix.RowCount, matrix.ColumnCount,
                    nzmax);

                // write ir
                writer.Write((int)DataType.Int32);
                writer.Write(nzmax*4);

                foreach (var column in matrix.EnumerateColumns())
                {
                    foreach (var row in column.EnumerateNonZeroIndexed())
                    {
                        writer.Write(row.Item1);
                    }
                }

                // add pad if needed
                if (nzmax%2 == 1)
                {
                    writer.Write(0);
                }

                // write jc
                writer.Write((int)DataType.Int32);
                writer.Write((matrix.ColumnCount + 1)*4);
                writer.Write(0);
                var count = 0;
                foreach (var column in matrix.EnumerateColumns())
                {
                    count += ((SparseVectorStorage<float>)column.Storage).ValueCount;
                    writer.Write(count);
                }

                // add pad if needed
                if (matrix.ColumnCount%2 == 0)
                {
                    writer.Write(0);
                }

                // write data
                writer.Write((int)DataType.Single);
                writer.Write(nzmax*4);

                foreach (var column in matrix.EnumerateColumns())
                {
                    foreach (var row in column.EnumerateNonZeroIndexed())
                    {
                        writer.Write(row.Item2);
                    }
                }

                var pad = nzmax*4%8;
                PadData(writer, pad);

                writer.Flush();
                return new MatlabMatrix(name, stream.ToArray());
            }
        }

        /// <summary>
        /// Gets the sparse data array.
        /// </summary>
        /// <param name="matrix">The matrix to get the data from.</param>
        /// <param name="name">The name of the matrix.</param>
        /// <returns>The matrix data as an array.</returns>
        static MatlabMatrix GetSparseDataArray(LinearAlgebra.Complex.SparseMatrix matrix, string name)
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                var nzmax = matrix.NonZerosCount;
                WriteMatrixTagAndName(writer, ArrayClass.Sparse, true, name, matrix.RowCount, matrix.ColumnCount,
                    nzmax);

                // write ir
                writer.Write((int)DataType.Int32);
                writer.Write(nzmax*4);

                foreach (var column in matrix.EnumerateColumns())
                {
                    foreach (var row in column.EnumerateNonZeroIndexed())
                    {
                        writer.Write(row.Item1);
                    }
                }

                // add pad if needed
                if (nzmax%2 == 1)
                {
                    writer.Write(0);
                }

                // write jc
                writer.Write((int)DataType.Int32);
                writer.Write((matrix.ColumnCount + 1)*4);
                writer.Write(0);
                var count = 0;
                foreach (var column in matrix.EnumerateColumns())
                {
                    count += ((SparseVectorStorage<Complex>)column.Storage).ValueCount;
                    writer.Write(count);
                }

                // add pad if needed
                if (matrix.ColumnCount%2 == 0)
                {
                    writer.Write(0);
                }

                // write data
                writer.Write((int)DataType.Double);
                writer.Write(nzmax*8);

                foreach (var column in matrix.EnumerateColumns())
                {
                    foreach (var row in column.EnumerateNonZeroIndexed())
                    {
                        writer.Write(row.Item2.Real);
                    }
                }

                writer.Write((int)DataType.Double);
                writer.Write(nzmax*8);

                foreach (var column in matrix.EnumerateColumns())
                {
                    foreach (var row in column.EnumerateNonZeroIndexed())
                    {
                        writer.Write(row.Item2.Imaginary);
                    }
                }

                writer.Flush();
                return new MatlabMatrix(name, stream.ToArray());
            }
        }

        /// <summary>
        /// Gets the sparse data array.
        /// </summary>
        /// <param name="matrix">The matrix to get the data from.</param>
        /// <param name="name">The name of the matrix.</param>
        /// <returns>The matrix data as an array.</returns>
        static MatlabMatrix GetSparseDataArray(LinearAlgebra.Complex32.SparseMatrix matrix, string name)
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                var nzmax = matrix.NonZerosCount;
                WriteMatrixTagAndName(writer, ArrayClass.Sparse, true, name, matrix.RowCount, matrix.ColumnCount,
                    nzmax);

                // write ir
                writer.Write((int)DataType.Int32);
                writer.Write(nzmax*4);

                foreach (var column in matrix.EnumerateColumns())
                {
                    foreach (var row in column.EnumerateNonZeroIndexed())
                    {
                        writer.Write(row.Item1);
                    }
                }

                // add pad if needed
                if (nzmax%2 == 1)
                {
                    writer.Write(0);
                }

                // write jc
                writer.Write((int)DataType.Int32);
                writer.Write((matrix.ColumnCount + 1)*4);
                writer.Write(0);
                var count = 0;
                foreach (var column in matrix.EnumerateColumns())
                {
                    count += ((SparseVectorStorage<Complex32>)column.Storage).ValueCount;
                    writer.Write(count);
                }

                // add pad if needed
                if (matrix.ColumnCount%2 == 0)
                {
                    writer.Write(0);
                }

                // write data
                writer.Write((int)DataType.Single);
                writer.Write(nzmax*4);

                foreach (var column in matrix.EnumerateColumns())
                {
                    foreach (var row in column.EnumerateNonZeroIndexed())
                    {
                        writer.Write(row.Item2.Real);
                    }
                }

                var pad = nzmax*4%8;
                PadData(writer, pad);

                writer.Write((int)DataType.Single);
                writer.Write(nzmax*4);

                foreach (var column in matrix.EnumerateColumns())
                {
                    foreach (var row in column.EnumerateNonZeroIndexed())
                    {
                        writer.Write(row.Item2.Imaginary);
                    }
                }

                PadData(writer, pad);

                writer.Flush();
                return new MatlabMatrix(name, stream.ToArray());
            }
        }

        /// <summary>
        /// Writes the file header.
        /// </summary>
        static void WriteHeader(BinaryWriter writer)
        {
            var header = Encoding.ASCII.GetBytes(HeaderText + DateTime.Now.ToString(Resources.MatlabDateHeaderFormat));
            writer.Write(header);
            PadData(writer, HeaderTextLength - header.Length + 8, 32);

            // write version
            writer.Write((short)0x100);

            // write little endian indicator
            writer.Write((byte)0x49);
            writer.Write((byte)0x4D);
        }

        /// <summary>
        /// Pads the data with the given byte.
        /// </summary>
        /// <param name="writer">Where to write the pad values.</param>
        /// <param name="bytes">The number of bytes to pad.</param>
        /// <param name="pad">What value to pad with.</param>
        static void PadData(BinaryWriter writer, int bytes, byte pad = (byte)0)
        {
            for (var i = 0; i < bytes; i++)
            {
                writer.Write(pad);
            }
        }

        /// <summary>
        /// Writes the compressed data.
        /// </summary>
        /// <param name="data">The data to write.</param>
        static void WriteCompressedData(BinaryWriter writer, byte[] data)
        {
            // fill in data size
            var size = BitConverter.GetBytes(data.Length);
            data[4] = size[0];
            data[5] = size[1];
            data[6] = size[2];
            data[7] = size[3];

            // compress data
            var compressedData = CompressData(data);

            // write compressed data to file
            writer.Write(compressedData.Length);
            writer.Write(compressedData);
        }

        /// <summary>
        /// Compresses the data array.
        /// </summary>
        /// <param name="data">The data to compress.</param>
        /// <returns>The compressed data.</returns>
        static byte[] CompressData(byte[] data)
        {
            var adler = BitConverter.GetBytes(Adler32.Compute(data));
            using (var compressedStream = new MemoryStream())
            {
                compressedStream.WriteByte(0x58);
                compressedStream.WriteByte(0x85);
                using (var outputStream = new DeflateStream(compressedStream, CompressionMode.Compress, true))
                {
                    outputStream.Write(data, 0, data.Length);
                }

                compressedStream.WriteByte(adler[3]);
                compressedStream.WriteByte(adler[2]);
                compressedStream.WriteByte(adler[1]);
                compressedStream.WriteByte(adler[0]);
                return compressedStream.ToArray();
            }
        }
    }
}