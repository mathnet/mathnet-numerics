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
using System.Text;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Properties;

namespace MathNet.Numerics.Data.Matlab
{
    /// <summary>
    /// Parse a MATLAB file
    /// </summary>
    internal static class Parser
    {
        /// <summary>
        /// Large Block Size
        /// </summary>
        const int LargeBlockSize = 8;

        /// <summary>
        /// Little Endian Indicator
        /// </summary>
        const byte LittleEndianIndicator = 0x49;

        /// <summary>
        /// Small Block Size
        /// </summary>
        const int SmallBlockSize = 4;

        /// <summary>
        /// Extracts all matrix blocks in a format we support.
        /// </summary>
        internal static List<MatlabMatrix> ParseAll(Stream stream)
        {
            var matrices = new List<MatlabMatrix>();

            using (var reader = new BinaryReader(stream))
            {
                reader.BaseStream.Position = 126;
                if (reader.ReadByte() != LittleEndianIndicator)
                {
                    throw new NotSupportedException(Resources.BigEndianNotSupported);
                }

                // skip version since it is always 0x0100.
                reader.BaseStream.Position = 128;
                var length = stream.Length;

                // for each data block add a MATLAB object to the file.
                while (reader.BaseStream.Position < length)
                {
                    var type = (DataType)reader.ReadInt16();
                    int size = reader.ReadInt16();
                    var smallBlock = true;
                    if (size == 0)
                    {
                        size = reader.ReadInt32();
                        smallBlock = false;
                    }

                    byte[] data;
                    if (type == DataType.Compressed)
                    {
                        data = DecompressBlock(reader.ReadBytes(size), out type);
                    }
                    else
                    {
                        data = new byte[size];
                        reader.Read(data, 0, size);
                        AlignData(reader.BaseStream, size, smallBlock);
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

                            matrices.Add(new MatlabMatrix(matrixName, matrixSize, matrixDim, data));
                        }
                    }
                }
            }

            return matrices;
        }

        /// <summary>
        /// Aligns the data.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="size">The size of the array.</param>
        /// <param name="smallBlock">if set to <c>true</c> if reading from a small block.</param>
        internal static void AlignData(Stream stream, int size, bool smallBlock)
        {
            var blockSize = smallBlock ? SmallBlockSize : LargeBlockSize;
            var offset = 0;
            var mod = size%blockSize;
            if (mod != 0)
            {
                offset = blockSize - mod;
            }

            stream.Seek(offset, SeekOrigin.Current);
        }

        /// <summary>
        /// Decompresses the block.
        /// </summary>
        /// <param name="compressed">The compressed data.</param>
        /// <param name="type">The type data type contained in the block.</param>
        /// <returns>The decompressed block.</returns>
        static byte[] DecompressBlock(byte[] compressed, out DataType type)
        {
            byte[] data;
            using (var compressedStream = new MemoryStream(compressed, 2, compressed.Length - 6))
            using (var decompressor = new DeflateStream(compressedStream, CompressionMode.Decompress))
            using (var decompressed = new MemoryStream())
            {
                decompressor.CopyTo(decompressed);
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

        internal static Matrix<TDataType> ReadMatrixBlock<TDataType>(byte[] data)
            where TDataType : struct, IEquatable<TDataType>, IFormattable
        {
            using (var stream = new MemoryStream(data))
            using (var reader = new BinaryReader(stream))
            {
                // skip tag - doesn't tell us anything we don't already know
                reader.BaseStream.Seek(8, SeekOrigin.Current);

                var arrayClass = (ArrayClass)reader.ReadByte();
                var flags = reader.ReadByte();
                var isComplex = (flags & (byte)ArrayFlags.Complex) == (byte)ArrayFlags.Complex;

                // skip unneeded bytes
                reader.BaseStream.Seek(10, SeekOrigin.Current);

                var numDimensions = reader.ReadInt32()/8;
                if (numDimensions > 2)
                {
                    throw new NotSupportedException(Resources.MoreThan2D);
                }

                var rows = reader.ReadInt32();
                var columns = reader.ReadInt32();

                // skip name and unneeded bytes
                reader.BaseStream.Seek(2, SeekOrigin.Current);
                int size = reader.ReadInt16();
                var smallBlock = true;
                if (size == 0)
                {
                    size = reader.ReadInt32();
                    smallBlock = false;
                }
                reader.BaseStream.Seek(size, SeekOrigin.Current);
                AlignData(reader.BaseStream, size, smallBlock);

                var type = (DataType)reader.ReadInt16();
                size = reader.ReadInt16();
                if (size == 0)
                {
                    size = reader.ReadInt32();
                }

                Matrix<TDataType> matrix;
                switch (arrayClass)
                {
                    case ArrayClass.Sparse:
                        matrix = SparseArrayReader<TDataType>.PopulateSparseMatrix(reader, isComplex, rows, columns, size);
                        break;
                    case ArrayClass.Function:
                    case ArrayClass.Character:
                    case ArrayClass.Object:
                    case ArrayClass.Structure:
                    case ArrayClass.Cell:
                    case ArrayClass.Unknown:
                        throw new NotSupportedException();
                    default:
                        matrix = NumericArrayReader<TDataType>.PopulateDenseMatrix(type, reader, isComplex, rows, columns, size);
                        break;
                }

                return matrix;
            }
        }
    }
}
