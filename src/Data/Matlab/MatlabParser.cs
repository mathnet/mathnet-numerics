// <copyright file="MatlabParser.cs" company="Math.NET">
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
    /// <typeparam name="TDataType">The data type of the matrix.</typeparam>
    internal class MatlabParser<TDataType>
        where TDataType : struct, IEquatable<TDataType>, IFormattable
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
        /// Holds the names of the matrices in the file.
        /// </summary>
        readonly IList<string> _names = new List<string>();

        /// <summary>
        /// The stream to read the MATLAB file from.
        /// </summary>
        readonly Stream _stream;

        /// <summary>
        /// Initializes a new instance of the <see cref="MatlabParser{TDataType}"/> class.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        internal MatlabParser(string fileName)
            : this(fileName, new string[0])
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MatlabParser{TDataType}"/> class.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        internal MatlabParser(Stream stream)
            : this(stream, new string[0])
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MatlabParser{TDataType}"/> class.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="objectNames">The name of the objects to retrieve.</param>
        internal MatlabParser(Stream stream, IEnumerable<string> objectNames)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            _stream = stream;
            SetNames(objectNames);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MatlabParser{TDataType}"/> class.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="objectNames">The name of the objects to retrieve.</param>
        MatlabParser(string fileName, IEnumerable<string> objectNames)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException(Resources.StringNullOrEmpty, "fileName");
            }

            _stream = File.OpenRead(fileName);
            SetNames(objectNames);
        }

        /// <summary>
        /// Copies the names of the objects to retrieve to a local field.
        /// </summary>
        /// <param name="objectNames">The name of the objects to retrieve.</param>
        void SetNames(IEnumerable<string> objectNames)
        {
            foreach (var name in objectNames)
            {
                _names.Add(name);
            }
        }

        /// <summary>
        /// Parses the file.
        /// </summary>
        /// <returns>The parsed MATLAB file as a <see cref="MatlabFile{TDataType}"/> object.</returns>
        internal MatlabFile<TDataType> Parse()
        {
            var file = new MatlabFile<TDataType>();

            using (var reader = new BinaryReader(_stream))
            {
                file.HeaderText = Encoding.ASCII.GetString(reader.ReadBytes(116));

                // skipping subsystem offsets
                reader.BaseStream.Position = 126;

                if (reader.ReadByte() != LittleEndianIndicator)
                {
                    throw new NotSupportedException(Resources.BigEndianNotSupported);
                }

                // skip version since it is always 0x0100.
                reader.BaseStream.Position = 128;
                var length = _stream.Length;

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
                        AddMatrix(data, file);
                    }
                    else
                    {
                        throw new NotSupportedException(string.Format(Resources.NotSupportedType, type));
                    }
                }
            }

            return file;
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

        /// <summary>
        /// Adds a matrix from the actual file into our presentation of a MATLAB file.
        /// </summary>
        /// <param name="data">The data of the matrix.</param>
        /// <param name="file">The <see cref="MatlabFile{TDataType}"/> instance.</param>
        void AddMatrix(byte[] data, MatlabFile<TDataType> file)
        {
            using (var ms = new MemoryStream(data))
            using (var reader = new BinaryReader(ms))
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

                // skip unneeded bytes
                reader.BaseStream.Seek(2, SeekOrigin.Current);
                int size = reader.ReadInt16();
                var smallBlock = true;
                if (size == 0)
                {
                    size = reader.ReadInt32();
                    smallBlock = false;
                }

                var name = Encoding.ASCII.GetString(reader.ReadBytes(size));
                AlignData(reader.BaseStream, size, smallBlock);

                // only grab wanted objects
                if (_names.Count != 0 && !_names.Contains(name))
                {
                    return;
                }

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

                file.Matrices.Add(name, matrix);
                if (file.FirstMatrixName == null)
                {
                    file.FirstMatrixName = name;
                }
            }
        }
    }
}
