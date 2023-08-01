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
                    throw new NotSupportedException("Big endian files are not supported.");
                }

                // set position to first data element, right after full file header (128 bytes)
                reader.BaseStream.Position = 128;
                var length = stream.Length;

                // for each data element add a MATLAB object to the file.
                while (reader.BaseStream.Position < length)
                {
                    // small format: size (2 bytes), type (2 bytes), data (4 bytes)
                    // long format: type (4 bytes), size (4 bytes), data (size, aligned to 8 bytes)

                    ReadElementTag(reader, out var type, out var size, out var isSmallBlock);

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
                        SkipElementPadding(reader, size, isSmallBlock);
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
            Func<BinaryReader, ArrayClass, bool, int, int, Matrix<T>> parser = (BinaryReader r, ArrayClass a, bool complex, int rows, int columns) =>
            {
                // Data
                switch (a)
                {
                    case ArrayClass.Sparse:
                        return PopulateSparseMatrix<T>(r, complex, rows, columns);
                    case ArrayClass.Function:
                    case ArrayClass.Character:
                    case ArrayClass.Object:
                    case ArrayClass.Structure:
                    case ArrayClass.Cell:
                    case ArrayClass.Unknown:
                        throw new NotSupportedException();
                    default:
                        return PopulateDenseMatrix<T>(r, complex, rows, columns);
                }
            };

            return ParseObject(data, parser);
        }

        /// <summary>
        /// For parsing nayhting that cannot be mapped to a MathNet.Numerics matrix
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        internal static NestedObject ParseNonNumeric(byte[] data)
        {
            Func<BinaryReader, ArrayClass, bool, int, int, NestedObject> parser = (BinaryReader r, ArrayClass a, bool complex, int rows, int columns) =>
            {
                // Data
                switch (a)
                {
                    case ArrayClass.Character:
                        return PopulateCharacterMatrix(r, rows, columns);
                    case ArrayClass.Structure:
                        return PopulateStructure(r);
                    case ArrayClass.Cell:
                        return PopulateCellMatrix(r, complex, rows, columns);
                    case ArrayClass.Unknown:
                        throw new NotSupportedException();
                    default:
                        throw new NotSupportedException();
                }
            };

            return ParseObject(data, parser);
        }

        private static NestedObject PopulateCharacterMatrix(BinaryReader reader, int rows, int columns)
        {
            ReadElementTag(reader, out var type, out var size, out var isSmallBlock);

            MatlabCharMatrix result;
            Encoding encoding;

            switch (type)
            {
                case DataType.Utf8:
                    encoding = Encoding.UTF8;
                    break;
                case DataType.Utf16:
                    encoding = Encoding.Unicode;
                    break;
                case DataType.Utf32:
                    encoding = Encoding.UTF32;
                    break;
                default:
                    throw new NotImplementedException($"Could not parse char array due to unsupported encoding: {type}");
            }

            result = new MatlabCharMatrix(rows, columns, encoding);

            for (int col = 0; col < columns; col++)
            {
                for (int row = 0; row < rows; row++)
                {
                    byte[] newChar;
                    if(encoding.IsSingleByte)
                    {
                        newChar = reader.ReadBytes(1);
                    }
                    else
                    {
                        newChar = reader.ReadBytes(2);
                    }

                    result.Data[row, col] = encoding.GetString(newChar);
                }
            }

            return new NestedObject(result);
        }

        internal static T ParseObject<T>(byte[] data, Func<BinaryReader, ArrayClass, bool,int,int,T> parser)
        {
            using (var stream = new MemoryStream(data))
            using (var reader = new BinaryReader(stream))
            {
                (ArrayClass arrayClass,
                bool complex,
                int rows, int columns, _) = ParseObjectHeader(reader);

                // Data
                return parser(reader, arrayClass, complex, rows, columns);
            }
        }

        /// <summary>
        /// Reads the object header and skips any remaining padding
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        private static (ArrayClass arrayClass, bool complex, int rows, int columns, string name) ParseObjectHeader(BinaryReader reader)
        {
            // Array Flags tag (8 bytes)
            reader.BaseStream.Seek(8, SeekOrigin.Current);

            // Array Flags data: flags (byte 3), class (byte 4) (8 bytes)
            ArrayClass arrayClass = (ArrayClass)reader.ReadByte();
            var flags = reader.ReadByte();
            bool complex = (flags & (byte)ArrayFlags.Complex) == (byte)ArrayFlags.Complex;
            reader.BaseStream.Seek(6, SeekOrigin.Current);

            // Dimensions Array tag (8 bytes)
            reader.BaseStream.Seek(4, SeekOrigin.Current);
            var numDimensions = reader.ReadInt32() / 8;
            if (numDimensions > 2)
            {
                throw new NotSupportedException("Only 1 and 2 dimensional arrays are supported.");
            }

            // Dimensions Array data: row and column count (8 bytes)
            int rows = reader.ReadInt32();
            int columns = reader.ReadInt32();

            // Array name
            ReadElementTag(reader, out _, out var size, out var isSmallBlock);
            byte[] nameBytes = new byte[size];
            reader.BaseStream.Read(nameBytes,0, size);
            string name = Encoding.UTF8.GetString(nameBytes);

            SkipElementPadding(reader, size, isSmallBlock);

            return (arrayClass, complex, rows, columns, name);
        }

        private static NestedObject PopulateStructure(BinaryReader reader)
        {
            // after the common fields for all arrays a structure has the length for the field names as a short data element
            // acording to the docs MATLAB always sets this to 32 bytes (31 chars + NULL) so we don't actually need to check it
            ReadElementTag(reader, out _, out _, out _);

            int nameLength = reader.ReadInt32();

            // field names are saved as an miINT8 data element
            // each name is padded to align on 32 bytes and NULL terminated
            ReadElementTag(reader, out _, out var size, out var isSmallBlock);

            List<string> fieldNames = new List<string>();
            int bytesRead = 0;

            while(bytesRead < size)
            {
                byte[] currentName = reader.ReadBytes(nameLength);
                fieldNames.Add(Encoding.UTF8.GetString(currentName).TrimEnd((char)0));
                bytesRead += nameLength;
            }

            SkipElementPadding(reader, size, isSmallBlock);

            // each field of the structure could be any type supported by a matlab file
            MatlabStructure result = new MatlabStructure();

            for (int i = 0; i<fieldNames.Count; i++)
            {
                // to use the regular array parsing methods we need to know how much data to give them
                ReadElementTag(reader, out _, out var fieldSize, out _);

                // we also need to know what the array class is (maybe a nested structure or a cell)
                (ArrayClass arrayClass, _, _, _, string name) = ParseObjectHeader(reader);

                // reset reader back to expected position for further parsers
                // the header has array flags (16 bytes), dimensions array (16 bytes) and array name (8 bytes)
                reader.BaseStream.Seek(-40, SeekOrigin.Current);

                byte[] arrayData = reader.ReadBytes(fieldSize);

                switch (arrayClass)
                {
                    case ArrayClass.Structure:
                    case ArrayClass.Cell:
                    case ArrayClass.Character:
                        result.Add(fieldNames[i], ParseNonNumeric(arrayData));
                        break;
                    default:
                        result.Add(fieldNames[i], new NestedObject(new MatlabMatrix(name, arrayData)));
                        break;
                }
            }

            return new NestedObject(result);
        }

        private static NestedObject PopulateCellMatrix(BinaryReader reader, bool complex, int rows, int columns)
        {
            MatlabCellMatrix result = new MatlabCellMatrix(rows, columns);

            for(int col = 0; col<columns; col++)
            {
                for(int row = 0; row<rows; row++)
                {
                    // to use the regular array parsing methods we need to know how much data to give them
                    ReadElementTag(reader, out _, out var fieldSize, out _);

                    // we also need to know what the array class is (maybe a nested structure or a cell)
                    (ArrayClass arrayClass, _, _, _, string name) = ParseObjectHeader(reader);

                    // reset reader back to expected position for further parsers
                    // the header has array flags (16 bytes), dimensions array (16 bytes) and array name (8 bytes)
                    reader.BaseStream.Seek(-40, SeekOrigin.Current);

                    byte[] arrayData = reader.ReadBytes(fieldSize);

                    switch (arrayClass)
                    {
                        case ArrayClass.Structure:
                        case ArrayClass.Cell:
                        case ArrayClass.Character:
                            result.Data[row, col] = ParseNonNumeric(arrayData);
                            break;
                        default:
                            result.Data[row, col]  = new NestedObject(new MatlabMatrix(name, arrayData));
                            break;
                    }
                }
            }

            return new NestedObject(result);
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

            // read real part array
            ReadElementTag(reader, out var type, out var size, out var isSmallBlock);

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
                PopulateComplexArray(reader, (Complex[])(object)data, complex, type, ref size, ref isSmallBlock);
            }
            else if (dataType == typeof(Complex32))
            {
                PopulateComplex32Array(reader, (Complex32[])(object)data, complex, type, ref size, ref isSmallBlock);
            }
            else
            {
                throw new NotSupportedException();
            }

            SkipElementPadding(reader, size, isSmallBlock);
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

            // populate the row data array
            ReadElementTag(reader, out var type, out var size, out var isSmallBlock);
            var ir = storage.ColumnIndices = new int[size/4];
            for (var i = 0; i < ir.Length; i++)
            {
                ir[i] = reader.ReadInt32();
            }

            SkipElementPadding(reader, size, isSmallBlock);

            // populate the column data array
            ReadElementTag(reader, out type, out size, out isSmallBlock);
            var jc = storage.RowPointers;
            if (jc.Length != size/4)
            {
                throw new Exception("invalid jcsize");
            }

            for (var j = 0; j < jc.Length; j++)
            {
                jc[j] = reader.ReadInt32();
            }

            SkipElementPadding(reader, size, isSmallBlock);

            // populate the values
            ReadElementTag(reader, out type, out size, out isSmallBlock);
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
                PopulateComplexArray(reader, (Complex[])(object)data, complex, type, ref size, ref isSmallBlock);
            }
            else if (dataType == typeof(Complex32))
            {
                PopulateComplex32Array(reader, (Complex32[])(object)data, complex, type, ref size, ref isSmallBlock);
            }
            else
            {
                throw new NotSupportedException();
            }

            SkipElementPadding(reader, size, isSmallBlock);
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
