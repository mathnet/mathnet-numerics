// <copyright file="Parser.cs" company="Math.NET">
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
    /// Parse a MATLAB file
    /// </summary>
    internal static class Parser
    {
        /// <summary>
        /// Little Endian Indicator
        /// </summary>
        const byte LittleEndianIndicator = 0x49;

        /// <summary>
        /// Small Block Size
        /// </summary>
        const int SmallBlockSize = 4;

        /// <summary>
        /// Large Block Size
        /// </summary>
        const int LargeBlockSize = 8;

        /// <summary>
        /// Extracts all matrix blocks in a format we support from a stream.
        /// </summary>
        internal static List<MatlabMatrix> ParseFile(Stream stream)
        {
            var matrices = new List<MatlabMatrix>();

            using (var reader = new BinaryReader(stream))
            {
                // skip header (116 bytes)
                // skip subsystem data offset (8 bytes)
                // skip version (2 bytes)
                reader.BaseStream.Position = 126;

                // endian indicator (2 bytes)
                if (reader.ReadByte() != LittleEndianIndicator)
                {
                    throw new NotSupportedException(Resources.BigEndianNotSupported);
                }

                // set position to first data element, right after full file header (128 bytes)
                reader.BaseStream.Position = 128;
                var length = stream.Length;

                // for each data element add a MATLAB object to the file.
                while (reader.BaseStream.Position < length)
                {
                    // small format: size (2 bytes), type (2 bytes), data (4 bytes)
                    // long format: type (4 bytes), size (4 bytes), data (size, aligned to 8 bytes)

                    DataType type;
                    int size;
                    bool smallBlock;
                    ReadElementTag(reader, out type, out size, out smallBlock);

                    // read element data of the size provided in the element header
                    // uncompress if compressed
                    byte[] data;
                    if (type == DataType.Compressed)
                    {
                        data = UnpackCompressedBlock(reader.ReadBytes(size), out type);
                    }
                    else
                    {
                        data = new byte[size];
                        reader.Read(data, 0, size);
                        SkipElementPadding(reader, size, smallBlock);
                    }

                    if (type == DataType.Matrix)
                    {
                        using (var matrixStream = new MemoryStream(data))
                        using (var matrixReader = new BinaryReader(matrixStream))
                        {
                            matrixReader.BaseStream.Seek(20, SeekOrigin.Current);
                            var matrixDim = matrixReader.ReadInt32()/8;
                            if (matrixDim > 2)
                            {
                                continue;
                            }

                            matrixReader.BaseStream.Seek(10, SeekOrigin.Current);
                            int matrixSize = matrixReader.ReadInt16();
                            if (matrixSize == 0)
                            {
                                matrixSize = matrixReader.ReadInt32();
                            }

                            var matrixName = Encoding.ASCII.GetString(matrixReader.ReadBytes(matrixSize));

                            matrices.Add(new MatlabMatrix(matrixName, data));
                        }
                    }
                }
            }

            return matrices;
        }

        /// <summary>
        /// Parse a matrix block byte array
        /// </summary>
        internal static Matrix<T> ParseMatrix<T>(byte[] data)
            where T : struct, IEquatable<T>, IFormattable
        {
            using (var stream = new MemoryStream(data))
            using (var reader = new BinaryReader(stream))
            {
                // Array Flags tag (8 bytes)
                reader.BaseStream.Seek(8, SeekOrigin.Current);

                // Array Flags data: flags (byte 3), class (byte 4) (8 bytes)
                var arrayClass = (ArrayClass)reader.ReadByte();
                var flags = reader.ReadByte();
                var complex = (flags & (byte)ArrayFlags.Complex) == (byte)ArrayFlags.Complex;
                reader.BaseStream.Seek(6, SeekOrigin.Current);

                // Dimensions Array tag (8 bytes)
                reader.BaseStream.Seek(4, SeekOrigin.Current);
                var numDimensions = reader.ReadInt32()/8;
                if (numDimensions > 2)
                {
                    throw new NotSupportedException(Resources.MoreThan2D);
                }

                // Dimensions Array data: row and column count (8 bytes)
                var rows = reader.ReadInt32();
                var columns = reader.ReadInt32();

                // Array name
                DataType type;
                int size;
                bool smallBlock;
                ReadElementTag(reader, out type, out size, out smallBlock);
                reader.BaseStream.Seek(size, SeekOrigin.Current);
                SkipElementPadding(reader, size, smallBlock);

                // Data
                switch (arrayClass)
                {
                    case ArrayClass.Sparse:
                        return PopulateSparseMatrix<T>(reader, complex, rows, columns);
                    case ArrayClass.Function:
                    case ArrayClass.Character:
                    case ArrayClass.Object:
                    case ArrayClass.Structure:
                    case ArrayClass.Cell:
                    case ArrayClass.Unknown:
                        throw new NotSupportedException();
                    default:
                        return PopulateDenseMatrix<T>(reader, complex, rows, columns);
                }
            }
        }

        /// <summary>
        /// Populates a dense matrix.
        /// </summary>
        /// <param name="reader">The reader to read from.</param>
        /// <param name="complex">if set to <c>true</c> if the MATLAB complex flag is set.</param>
        /// <param name="rows">The number of rows.</param>
        /// <param name="columns">The number of columns.</param>
        /// <returns>Returns a populated dense matrix.</returns>
        static Matrix<T> PopulateDenseMatrix<T>(BinaryReader reader, bool complex, int rows, int columns)
            where T : struct, IEquatable<T>, IFormattable
        {
            var dataType = typeof(T);
            var count = rows*columns;
            var data = new T[count];

            DataType type;
            int size;
            bool smallBlock;

            // read real part array
            ReadElementTag(reader, out type, out size, out smallBlock);

            // direct copy if possible
            if ((type == DataType.Double && dataType == typeof(double)) || (type == DataType.Single && dataType == typeof(float)))
            {
                Buffer.BlockCopy(reader.ReadBytes(size), 0, data, 0, size);
            }
            else if (dataType == typeof(double))
            {
                if (complex)
                {
                    throw new ArgumentException("Invalid TDataType. Matrix is stored as a complex matrix, but a real data type was given.");
                }

                PopulateDoubleArray(reader, (double[])(object)data, type);
            }
            else if (dataType == typeof(float))
            {
                if (complex)
                {
                    throw new ArgumentException("Invalid TDataType. Matrix is stored as a complex matrix, but a real data type was given.");
                }

                PopulateSingleArray(reader, (float[])(object)data, type);
            }
            else if (dataType == typeof(Complex))
            {
                PopulateComplexArray(reader, (Complex[])(object)data, complex, type, ref size, ref smallBlock);
            }
            else if (dataType == typeof(Complex32))
            {
                PopulateComplex32Array(reader, (Complex32[])(object)data, complex, type, ref size, ref smallBlock);
            }
            else
            {
                throw new NotSupportedException();
            }

            SkipElementPadding(reader, size, smallBlock);
            return Matrix<T>.Build.Dense(rows, columns, data);
        }

        /// <summary>
        /// Populates a sparse matrix.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="complex">if set to <c>true</c> if the MATLAB complex flag is set.</param>
        /// <param name="rows">The number of rows.</param>
        /// <param name="columns">The number of columns.</param>
        /// <returns>A populated sparse matrix.</returns>
        static Matrix<T> PopulateSparseMatrix<T>(BinaryReader reader, bool complex, int rows, int columns)
            where T : struct, IEquatable<T>, IFormattable
        {
            // Create matrix with CSR storage.
            var matrix = Matrix<T>.Build.Sparse(columns, rows);

            // MATLAB sparse matrices are actually stored as CSC, so just read the data and then transpose.
            var storage = matrix.Storage as SparseCompressedRowMatrixStorage<T>;

            DataType type;
            int size;
            bool smallBlock;

            // populate the row data array
            ReadElementTag(reader, out type, out size, out smallBlock);
            var ir = storage.ColumnIndices = new int[size/4];
            for (var i = 0; i < ir.Length; i++)
            {
                ir[i] = reader.ReadInt32();
            }

            SkipElementPadding(reader, size, smallBlock);

            // populate the column data array
            ReadElementTag(reader, out type, out size, out smallBlock);
            var jc = storage.RowPointers;
            if (jc.Length != size/4)
            {
                throw new Exception("invalid jcsize");
            }

            for (var j = 0; j < jc.Length; j++)
            {
                jc[j] = reader.ReadInt32();
            }

            SkipElementPadding(reader, size, smallBlock);

            // populate the values
            ReadElementTag(reader, out type, out size, out smallBlock);
            var dataType = typeof(T);
            var data = storage.Values = new T[jc[columns]];

            if (dataType == typeof(double))
            {
                if (complex)
                {
                    throw new ArgumentException("Invalid TDataType. Matrix is stored as a complex matrix, but a real data type was given.");
                }

                PopulateDoubleArray(reader, (double[])(object)data, type);
            }
            else if (dataType == typeof(float))
            {
                if (complex)
                {
                    throw new ArgumentException("Invalid TDataType. Matrix is stored as a complex matrix, but a real data type was given.");
                }

                PopulateSingleArray(reader, (float[])(object)data, type);
            }
            else if (dataType == typeof(Complex))
            {
                PopulateComplexArray(reader, (Complex[])(object)data, complex, type, ref size, ref smallBlock);
            }
            else if (dataType == typeof(Complex32))
            {
                PopulateComplex32Array(reader, (Complex32[])(object)data, complex, type, ref size, ref smallBlock);
            }
            else
            {
                throw new NotSupportedException();
            }

            SkipElementPadding(reader, size, smallBlock);
            return matrix.Transpose();
        }

        /// <summary>
        /// Populates the double dense matrix.
        /// </summary>
        static void PopulateDoubleArray(BinaryReader reader, double[] data, DataType type)
        {
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = ReadDoubleValue(reader, type);
            }
        }

        /// <summary>
        /// Populates the float dense matrix.
        /// </summary>
        static void PopulateSingleArray(BinaryReader reader, float[] data, DataType type)
        {
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = (float)ReadDoubleValue(reader, type);
            }
        }

        /// <summary>
        /// Populates the complex dense matrix.
        /// </summary>
        static void PopulateComplexArray(BinaryReader reader, Complex[] data, bool complex, DataType type, ref int size, ref bool smallBlock)
        {
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = ReadDoubleValue(reader, type);
            }

            if (complex)
            {
                SkipElementPadding(reader, size, smallBlock);
                ReadElementTag(reader, out type, out size, out smallBlock);

                for (int i = 0; i < data.Length; i++)
                {
                    data[i] = new Complex(data[i].Real, ReadDoubleValue(reader, type));
                }
            }
        }

        /// <summary>
        /// Populates the complex32 dense matrix.
        /// </summary>
        static void PopulateComplex32Array(BinaryReader reader, Complex32[] data, bool complex, DataType type, ref int size, ref bool smallBlock)
        {
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = (float)ReadDoubleValue(reader, type);
            }

            if (complex)
            {
                SkipElementPadding(reader, size, smallBlock);
                ReadElementTag(reader, out type, out size, out smallBlock);

                for (int i = 0; i < data.Length; i++)
                {
                    data[i] = new Complex32(data[i].Real, (float)ReadDoubleValue(reader, type));
                }
            }
        }

        static double ReadDoubleValue(BinaryReader reader, DataType type)
        {
            switch (type)
            {
                case DataType.Double:
                    return reader.ReadDouble();
                case DataType.Int8:
                    return reader.ReadSByte();
                case DataType.UInt8:
                    return reader.ReadByte();
                case DataType.Int16:
                    return reader.ReadInt16();
                case DataType.UInt16:
                    return reader.ReadUInt16();
                case DataType.Int32:
                    return reader.ReadInt32();
                case DataType.UInt32:
                    return reader.ReadUInt32();
                case DataType.Single:
                    return reader.ReadSingle();
                case DataType.Int64:
                    return reader.ReadInt64();
                case DataType.UInt64:
                    return reader.ReadUInt64();
                default:
                    throw new NotSupportedException();
            }
        }

        static void ReadElementTag(BinaryReader reader, out DataType dataType, out int size, out bool smallBlock)
        {
            // assume small format
            smallBlock = true;

            // small type (2 bytes)
            dataType = (DataType)reader.ReadInt16();

            // small size (2 bytes)
            size = reader.ReadInt16();

            if (size == 0)
            {
                // long format detected
                smallBlock = false;

                // long size (4 bytes)
                size = reader.ReadInt32();
            }
        }

        static void SkipElementPadding(BinaryReader reader, int size, bool smallBlock)
        {
            var blockSize = smallBlock ? SmallBlockSize : LargeBlockSize;
            var offset = 0;
            var mod = size%blockSize;
            if (mod != 0)
            {
                offset = blockSize - mod;
            }

            reader.BaseStream.Seek(offset, SeekOrigin.Current);
        }

        /// <summary>
        /// Unpacks a compressed block.
        /// </summary>
        /// <param name="compressed">The compressed data.</param>
        /// <param name="type">The type data type contained in the block.</param>
        /// <returns>The decompressed block.</returns>
        static byte[] UnpackCompressedBlock(byte[] compressed, out DataType type)
        {
            byte[] data;
            using (var decompressed = new MemoryStream())
            {
                using (var compressedStream = new MemoryStream(compressed, 2, compressed.Length - 6))
                using (var decompressor = new DeflateStream(compressedStream, CompressionMode.Decompress))
                {
                    decompressor.CopyTo(decompressed);
                }

                decompressed.Position = 0;
                var buf = new byte[4];
                decompressed.Read(buf, 0, 4);
                type = (DataType)BitConverter.ToInt32(buf, 0);
                decompressed.Read(buf, 0, 4);
                var size = BitConverter.ToInt32(buf, 0);
                data = new byte[size];
                decompressed.Read(data, 0, size);
            }

            return data;
        }
    }
}
