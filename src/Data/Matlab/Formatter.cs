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

            var typeT = typeof (T);
            bool sparse = matrix.Storage.GetType().GetGenericTypeDefinition() == typeof (SparseCompressedRowMatrixStorage<>);
            bool doublePrecision = typeT == typeof (double) || typeT == typeof (Complex);
            bool complex = typeT == typeof (Complex) || typeT == typeof (Complex32);

            int sparseNonZeroValues = 0;
            if (sparse)
            {
                var sparseStorage = (SparseCompressedRowMatrixStorage<T>)matrix.Storage;
                sparseNonZeroValues = sparseStorage.ValueCount;
            }

            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                // Array Flags tag: data type + size (8 bytes)
                writer.Write((int)DataType.UInt32);
                writer.Write(8);

                // Array Flags data: flags (byte 3), class (byte 4) (8 bytes)
                writer.Write((byte)(sparse ? ArrayClass.Sparse : doublePrecision ? ArrayClass.Double : ArrayClass.Single));
                writer.Write((byte)(complex ? ArrayFlags.Complex : 0));
                writer.Write((short)0);
                writer.Write((int)sparseNonZeroValues);

                // Dimensions Array tag: data type + size (8 bytes)
                writer.Write((int)DataType.Int32);
                writer.Write(8);

                // Dimensions Array data: row and column count (8 bytes)
                writer.Write(matrix.RowCount);
                writer.Write(matrix.ColumnCount);

                // Array Name:
                var nameBytes = Encoding.ASCII.GetBytes(name);
                if (nameBytes.Length > 4)
                {
                    // long format
                    writer.Write((int)DataType.Int8);
                    writer.Write(nameBytes.Length);
                    writer.Write(nameBytes);
                    PadData(writer, 8 - (nameBytes.Length%8));
                }
                else
                {
                    // small format
                    writer.Write((short)DataType.Int8);
                    writer.Write((short)nameBytes.Length);
                    writer.Write(nameBytes);
                    PadData(writer, 4 - nameBytes.Length);
                }

                if (doublePrecision && !complex)
                {
                    var sparseMatrix = matrix as LinearAlgebra.Double.SparseMatrix;
                    if (sparseMatrix != null)
                    {
                        SparseArrayFormatter.Write(writer, sparseMatrix);
                    }
                    else
                    {
                        NumericArrayFormatter.Write(writer, (LinearAlgebra.Double.Matrix)(object)matrix);
                    }
                }
                else if (!doublePrecision && !complex)
                {
                    var sparseMatrix = matrix as LinearAlgebra.Single.SparseMatrix;
                    if (sparseMatrix != null)
                    {
                        SparseArrayFormatter.Write(writer, sparseMatrix);
                    }
                    else
                    {
                        NumericArrayFormatter.Write(writer, (LinearAlgebra.Single.Matrix)(object)matrix);
                    }
                }
                else if (doublePrecision)
                {
                    var sparseMatrix = matrix as LinearAlgebra.Complex.SparseMatrix;
                    if (sparseMatrix != null)
                    {
                        SparseArrayFormatter.Write(writer, sparseMatrix);
                    }
                    else
                    {
                        NumericArrayFormatter.Write(writer, (LinearAlgebra.Complex.Matrix)(object)matrix);
                    }
                }
                else
                {
                    var sparseMatrix = matrix as LinearAlgebra.Complex32.SparseMatrix;
                    if (sparseMatrix != null)
                    {
                        SparseArrayFormatter.Write(writer, sparseMatrix);
                    }
                    else
                    {
                        NumericArrayFormatter.Write(writer, (LinearAlgebra.Complex32.Matrix)(object)matrix);
                    }
                }

                writer.Flush();
                return new MatlabMatrix(name, stream.ToArray());
            }
        }

        /// <summary>
        /// Writes all matrix blocks to a stream.
        /// </summary>
        internal static void FormatFile(Stream stream, IEnumerable<MatlabMatrix> matrices)
        {
            using (var buffer = new BufferedStream(stream))
            using (var writer = new BinaryWriter(buffer))
            {
                // write header and subsystem data offset (all space)
                var header = Encoding.ASCII.GetBytes(HeaderText + DateTime.Now.ToString(Resources.MatlabDateHeaderFormat));
                writer.Write(header);
                PadData(writer, 116 - header.Length + 8, 32);

                // write version
                writer.Write((short)0x100);

                // write little endian indicator
                writer.Write((byte)0x49);
                writer.Write((byte)0x4D);

                foreach (var matrix in matrices)
                {
                    // write data type
                    writer.Write((int)DataType.Compressed);

                    // compress data
                    var compressedData = PackCompressedBlock(matrix.Data, DataType.Matrix);

                    // write compressed data to file
                    writer.Write(compressedData.Length);
                    writer.Write(compressedData);
                }

                writer.Flush();
                writer.Close();
            }
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
        /// Packs a compressed block
        /// </summary>
        static byte[] PackCompressedBlock(byte[] data, DataType dataType)
        {
            var adler = BitConverter.GetBytes(Adler32.Compute(data));
            using (var compressedStream = new MemoryStream())
            {
                compressedStream.WriteByte(0x58);
                compressedStream.WriteByte(0x85);

                using (var outputStream = new DeflateStream(compressedStream, CompressionMode.Compress, true))
                {
                    outputStream.Write(BitConverter.GetBytes((int)dataType), 0, 4);
                    outputStream.Write(BitConverter.GetBytes(data.Length), 0, 4);
                    outputStream.Write(data, 0, data.Length);
                    outputStream.Flush();
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