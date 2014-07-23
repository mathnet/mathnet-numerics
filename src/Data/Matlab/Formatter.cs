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
        /// Small Block Size
        /// </summary>
        const int SmallBlockSize = 4;

        /// <summary>
        /// Large Block Size
        /// </summary>
        const int LargeBlockSize = 8;

        /// <summary>
        /// Writes all matrix blocks to a stream.
        /// </summary>
        internal static void FormatFile(Stream stream, IEnumerable<MatlabMatrix> matrices)
        {
            using (var buffer = new BufferedStream(stream))
            using (var writer = new BinaryWriter(buffer))
            {
                // write header and subsystem data offset (116+8 bytes)
                var header = Encoding.ASCII.GetBytes(HeaderText + DateTime.Now.ToString(Resources.MatlabDateHeaderFormat));
                writer.Write(header);
                Pad(writer, 116 - header.Length + 8, 32);

                // write version (2 bytes)
                writer.Write((short)0x100);

                // write little endian indicator (2 bytes)
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

            var dataType = typeof(T);
            bool doublePrecision = dataType == typeof(double) || dataType == typeof(Complex);
            bool complex = dataType == typeof(Complex) || dataType == typeof(Complex32);

            bool sparse;
            Type storageType = matrix.Storage.GetType().GetGenericTypeDefinition();
            if (storageType == typeof (DenseColumnMajorMatrixStorage<>))
            {
                sparse = false;
            }
            else if (storageType == typeof (SparseCompressedRowMatrixStorage<>))
            {
                sparse = true;
            }
            else if (storageType == typeof (DiagonalMatrixStorage<>))
            {
                // convert diagonal matrices to sparse
                sparse = true;
                var sparseMatrix = Matrix<T>.Build.Sparse(matrix.RowCount, matrix.ColumnCount);
                matrix.CopyTo(sparseMatrix);
                matrix = sparseMatrix;
            }
            else
            {
                // convert unknown matrices to dense
                sparse = false;
                var denseMatrix = Matrix<T>.Build.Dense(matrix.RowCount, matrix.ColumnCount);
                matrix.CopyTo(denseMatrix);
                matrix = denseMatrix;
            }

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
                bool smallBlock;
                var nameBytes = Encoding.ASCII.GetBytes(name);
                WriteElementTag(writer, DataType.Int8, nameBytes.Length, out smallBlock);
                writer.Write(nameBytes);
                PadElement(writer, nameBytes.Length, smallBlock);

                if (sparse)
                {
                    WriteSparseMatrix(writer, matrix, complex, doublePrecision);
                }
                else
                {
                    WriteDenseMatrix(writer, matrix, complex, doublePrecision);
                }

                writer.Flush();
                return new MatlabMatrix(name, stream.ToArray());
            }
        }

        static void WriteDenseMatrix<T>(BinaryWriter writer, Matrix<T> matrix, bool complex, bool doublePrecision)
            where T : struct, IEquatable<T>, IFormattable
        {
            int count = matrix.RowCount*matrix.ColumnCount;

            bool smallBlock;
            int size = doublePrecision ? count*8 : count*4;
            WriteElementTag(writer, doublePrecision ? DataType.Double : DataType.Single, size, out smallBlock);

            var data = ((DenseColumnMajorMatrixStorage<T>)matrix.Storage).Data;

            if (doublePrecision && !complex)
            {
                WriteDoubleArray(writer, (double[])(object)data, data.Length);
            }
            else if (!doublePrecision && !complex)
            {
                WriteSingleArray(writer, (float[])(object)data, data.Length);
            }
            else if (doublePrecision)
            {
                WriteComplexArray(writer, (Complex[])(object)data, data.Length, size, ref smallBlock);
            }
            else
            {
                WriteComplex32Array(writer, (Complex32[])(object)data, data.Length, size, ref smallBlock);
            }

            PadElement(writer, size, smallBlock);
        }

        static void WriteSparseMatrix<T>(BinaryWriter writer, Matrix<T> matrix, bool complex, bool doublePrecision)
            where T : struct, IEquatable<T>, IFormattable
        {
            var transposed = matrix.Transpose();
            var storage = (SparseCompressedRowMatrixStorage<T>)transposed.Storage;

            bool smallBlock;
            int nzcount = storage.ValueCount;

            // row data array
            var ir = storage.ColumnIndices;
            WriteElementTag(writer, DataType.Int32, nzcount*4, out smallBlock);
            for (var i = 0; i < nzcount; i++)
            {
                writer.Write(ir[i]);
            }

            PadElement(writer, nzcount*4, smallBlock);

            // column data array
            var jc = storage.RowPointers;
            WriteElementTag(writer, DataType.Int32, jc.Length*4, out smallBlock);
            for (var i = 0; i < jc.Length; i++)
            {
                writer.Write(jc[i]);
            }

            PadElement(writer, jc.Length*4, smallBlock);

            // values
            int size = doublePrecision ? nzcount*8 : nzcount*4;
            WriteElementTag(writer, doublePrecision ? DataType.Double : DataType.Single, size, out smallBlock);

            if (doublePrecision && !complex)
            {
                WriteDoubleArray(writer, (double[])(object)storage.Values, nzcount);
            }
            else if (!doublePrecision && !complex)
            {
                WriteSingleArray(writer, (float[])(object)storage.Values, nzcount);
            }
            else if (doublePrecision)
            {
                WriteComplexArray(writer, (Complex[])(object)storage.Values, nzcount, size, ref smallBlock);
            }
            else
            {
                WriteComplex32Array(writer, (Complex32[])(object)storage.Values, nzcount, size, ref smallBlock);
            }

            PadElement(writer, size, smallBlock);
        }

        static void WriteDoubleArray(BinaryWriter writer, double[] data, int count)
        {
            for (int i = 0; i < count; i++)
            {
                writer.Write(data[i]);
            }
        }

        static void WriteSingleArray(BinaryWriter writer, float[] data, int count)
        {
            for (int i = 0; i < count; i++)
            {
                writer.Write(data[i]);
            }
        }

        static void WriteComplexArray(BinaryWriter writer, Complex[] data, int count, int size, ref bool smallBlock)
        {
            for (int i = 0; i < count; i++)
            {
                writer.Write(data[i].Real);
            }

            PadElement(writer, size, smallBlock);
            WriteElementTag(writer, DataType.Double, size, out smallBlock);

            for (int i = 0; i < count; i++)
            {
                writer.Write(data[i].Imaginary);
            }
        }

        static void WriteComplex32Array(BinaryWriter writer, Complex32[] data, int count, int size, ref bool smallBlock)
        {
            for (int i = 0; i < count; i++)
            {
                writer.Write(data[i].Real);
            }

            PadElement(writer, size, smallBlock);
            WriteElementTag(writer, DataType.Single, size, out smallBlock);

            for (int i = 0; i < count; i++)
            {
                writer.Write(data[i].Imaginary);
            }
        }

        static void WriteElementTag(BinaryWriter writer, DataType dataType, int size, out bool smallBlock)
        {
            if (size > 4)
            {
                // long format
                smallBlock = false;
                writer.Write((int)dataType);
                writer.Write(size);
            }
            else
            {
                // small format
                smallBlock = true;
                writer.Write((short)dataType);
                writer.Write((short)size);
            }
        }

        static void PadElement(BinaryWriter writer, int size, bool smallBlock, byte padValue = (byte)0)
        {
            var blockSize = smallBlock ? SmallBlockSize : LargeBlockSize;
            var offset = 0;
            var mod = size%blockSize;
            if (mod != 0)
            {
                offset = blockSize - mod;
            }

            for (var i = 0; i < offset; i++)
            {
                writer.Write(padValue);
            }
        }

        static void Pad(BinaryWriter writer, int count, byte padValue = (byte)0)
        {
            for (var i = 0; i < count; i++)
            {
                writer.Write(padValue);
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