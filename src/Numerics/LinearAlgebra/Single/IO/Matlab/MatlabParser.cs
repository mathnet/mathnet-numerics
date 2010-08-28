// <copyright file="MatlabParser.cs" company="Math.NET">
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

namespace MathNet.Numerics.LinearAlgebra.Single.IO.Matlab
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using Common.IO.Matlab;
    using Generic;
    using Properties;
    using zlib;

    /// <summary>
    /// Parse a Matlab file
    /// </summary>
    internal class MatlabParser
    {
        /// <summary>
        /// Large Block Size
        /// </summary>
        private const int LargeBlockSize = 8;

        /// <summary>
        /// Little Endian Indicator
        /// </summary>
        private const byte LittleEndianIndicator = 0x49;

        /// <summary>
        /// Small Block Size
        /// </summary>
        private const int SmallBlockSize = 4;

        /// <summary>
        /// Holds the names of the matrices in the file.
        /// </summary>
        private readonly IList<string> _names = new List<string>();

        /// <summary>
        /// The stream to read the matlab file from.
        /// </summary>
        private readonly Stream _stream;

        /// <summary>
        /// Initializes a new instance of the <see cref="MatlabParser"/> class.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        public MatlabParser(string fileName)
            : this(fileName, new string[0])
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MatlabParser"/> class.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        public MatlabParser(Stream stream)
            : this(stream, new string[0])
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MatlabParser"/> class.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="objectNames">The name of the objects to retrieve.</param>
        public MatlabParser(Stream stream, IEnumerable<string> objectNames)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            _stream = stream;
            SetNames(objectNames);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MatlabParser"/> class.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="objectNames">The name of the objects to retrieve.</param>
        public MatlabParser(string fileName, IEnumerable<string> objectNames)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException(Resources.StringNullOrEmpty, "filename");
            }

            _stream = File.OpenRead(fileName);
            SetNames(objectNames);
        }

        /// <summary>
        /// Copies the names of the objects to retrieve to a local field.
        /// </summary>
        /// <param name="objectNames">The name of the objects to retrieve.</param>
        private void SetNames(IEnumerable<string> objectNames)
        {
            foreach (var name in objectNames)
            {
                _names.Add(name);
            }
        }

        /// <summary>
        /// Parses the file.
        /// </summary>
        /// <returns>The parsed Matlab file as a <see cref="MatlabFile"/> object.</returns>
        public MatlabFile Parse()
        {
            var file = new MatlabFile();

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

                // for each data block add a matlab object to the file.
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
                        data = DecompressBlock(reader.ReadBytes(size), ref type);
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
        private static void AlignData(Stream stream, int size, bool smallBlock)
        {
            var blockSize = smallBlock ? SmallBlockSize : LargeBlockSize;
            var offset = 0;
            var mod = size % blockSize;
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
        private static byte[] DecompressBlock(byte[] compressed, ref DataType type)
        {
            byte[] data;
            using (var decompressed = new MemoryStream())
            {
                using (var decompressor = new ZOutputStream(decompressed))
                {
                    decompressor.Write(compressed, 0, compressed.Length);
                    decompressed.Position = 0;
                    var buf = new byte[4];
                    decompressed.Read(buf, 0, 4);
                    type = (DataType)BitConverter.ToInt32(buf, 0);
                    decompressed.Read(buf, 0, 4);
                    var size = BitConverter.ToInt32(buf, 0);
                    data = new byte[size];
                    decompressed.Read(data, 0, size);
                }
            }

            return data;
        }

        /// <summary>
        /// Adds a matrix from the actual file into our presentation of a matlab file.
        /// </summary>
        /// <param name="data">The data of the matrix.</param>
        /// <param name="file">The <see cref="MatlabFile"/> instance.</param>
        private void AddMatrix(byte[] data, MatlabFile file)
        {
            using (var ms = new MemoryStream(data))
            {
                using (var reader = new BinaryReader(ms))
                {
                    // skip tag - doesn't tell us anything we don't already know
                    reader.BaseStream.Seek(8, SeekOrigin.Current);

                    var arrayClass = (ArrayClass)reader.ReadByte();
                    var flags = reader.ReadByte();
                    var isComplex = (flags & (byte)ArrayFlags.Complex) == (byte)ArrayFlags.Complex;

                    if (isComplex)
                    {
                        throw new NotSupportedException(Resources.ComplexMatricesNotSupported);
                    }

                    // skip unneeded bytes
                    reader.BaseStream.Seek(10, SeekOrigin.Current);

                    var numDimensions = reader.ReadInt32() / 8;
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

                    Matrix<float> matrix;
                    switch (arrayClass)
                    {
                        case ArrayClass.Sparse:
                            matrix = PopulateSparseMatrix(reader, rows, columns, size);
                            break;
                        case ArrayClass.Function:
                        case ArrayClass.Character:
                        case ArrayClass.Object:
                        case ArrayClass.Structure:
                        case ArrayClass.Cell:
                        case ArrayClass.Unknown:
                            throw new NotImplementedException();
                        default:
                            matrix = PopulateDenseMatrix(type, reader, rows, columns);
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

        /// <summary>
        /// Populates a sparse matrix.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="rows">The number of rows.</param>
        /// <param name="columns">The number of columns.</param>
        /// <param name="size">The size of the block.</param>
        /// <returns>A populated sparse matrix.</returns>
        private static Matrix<float> PopulateSparseMatrix(BinaryReader reader, int rows, int columns, int size)
        {
            // populate the row data array
            var ir = new int[size / 4];
            for (var i = 0; i < ir.Length; i++)
            {
                ir[i] = reader.ReadInt32();
            }

            AlignData(reader.BaseStream, size, false);

            // skip data type since it will always be int32
            reader.BaseStream.Seek(4, SeekOrigin.Current);

            // populate the column data array
            var jcsize = reader.ReadInt32();
            var jc = new int[jcsize / 4];
            for (var j = 0; j < jc.Length; j++)
            {
                jc[j] = reader.ReadInt32();
            }

            AlignData(reader.BaseStream, jcsize, false);

            var type = (DataType)reader.ReadInt32();

            // skip length since we already no it for the number of rows
            reader.BaseStream.Seek(4, SeekOrigin.Current);

            Matrix<float> matrix = new SparseMatrix(rows, columns);
            var col = 0;
            for (var i = 0; i < ir.Length; i++)
            {
                var row = ir[i];
                if (jc[col + 1] == i)
                {
                    col++;
                }

                switch (type)
                {
                    case DataType.Int8:
                        matrix[row, col] = reader.ReadSByte();
                        break;
                    case DataType.UInt8:
                        matrix[row, col] = reader.ReadByte();
                        break;
                    case DataType.Int16:
                        matrix[row, col] = reader.ReadInt16();
                        break;

                    case DataType.UInt16:
                        matrix[row, col] = reader.ReadUInt16();
                        break;
                    case DataType.Int32:
                        matrix[row, col] = reader.ReadInt32();
                        break;

                    case DataType.UInt32:
                        matrix[row, col] = reader.ReadUInt32();
                        break;
                    case DataType.Single:
                        matrix[row, col] = reader.ReadSingle();
                        break;
                    case DataType.Int64:
                        matrix[row, col] = reader.ReadInt64();
                        break;
                    case DataType.UInt64:
                        matrix[row, col] = reader.ReadUInt64();
                        break;
                    case DataType.Double:
                        matrix[row, col] = (float)reader.ReadDouble();
                        break;
                    default:
                        throw new NotSupportedException();
                }
            }

            return matrix;
        }

        /// <summary>
        /// Populates a dense matrix.
        /// </summary>
        /// <param name="type">The type of data.</param>
        /// <param name="reader">The reader.</param>
        /// <param name="rows">The number of rows.</param>
        /// <param name="columns">The number of columns.</param>
        /// <returns>Returns a populated dense matrix.</returns>
        private static Matrix<float> PopulateDenseMatrix(DataType type, BinaryReader reader, int rows, int columns)
        {
            Matrix<float> matrix = new DenseMatrix(rows, columns);
            switch (type)
            {
                case DataType.Int8:
                    for (var j = 0; j < columns; j++)
                    {
                        for (var i = 0; i < rows; i++)
                        {
                            matrix[i, j] = reader.ReadSByte();
                        }
                    }

                    break;
                case DataType.UInt8:
                    for (var j = 0; j < columns; j++)
                    {
                        for (var i = 0; i < rows; i++)
                        {
                            matrix[i, j] = reader.ReadByte();
                        }
                    }

                    break;
                case DataType.Int16:
                    for (var j = 0; j < columns; j++)
                    {
                        for (var i = 0; i < rows; i++)
                        {
                            matrix[i, j] = reader.ReadInt16();
                        }
                    }

                    break;
                case DataType.UInt16:
                    for (var j = 0; j < columns; j++)
                    {
                        for (var i = 0; i < rows; i++)
                        {
                            matrix[i, j] = reader.ReadUInt16();
                        }
                    }

                    break;
                case DataType.Int32:
                    for (var j = 0; j < columns; j++)
                    {
                        for (var i = 0; i < rows; i++)
                        {
                            matrix[i, j] = reader.ReadInt32();
                        }
                    }

                    break;
                case DataType.UInt32:
                    for (var j = 0; j < columns; j++)
                    {
                        for (var i = 0; i < rows; i++)
                        {
                            matrix[i, j] = reader.ReadUInt32();
                        }
                    }

                    break;
                case DataType.Single:
                    for (var j = 0; j < columns; j++)
                    {
                        for (var i = 0; i < rows; i++)
                        {
                            matrix[i, j] = reader.ReadSingle();
                        }
                    }

                    break;
                case DataType.Int64:
                    for (var j = 0; j < columns; j++)
                    {
                        for (var i = 0; i < rows; i++)
                        {
                            matrix[i, j] = reader.ReadInt64();
                        }
                    }

                    break;
                case DataType.UInt64:
                    for (var j = 0; j < columns; j++)
                    {
                        for (var i = 0; i < rows; i++)
                        {
                            matrix[i, j] = reader.ReadUInt64();
                        }
                    }

                    break;
                case DataType.Double:
                    for (var j = 0; j < columns; j++)
                    {
                        for (var i = 0; i < rows; i++)
                        {
                            matrix[i, j] = (float)reader.ReadDouble();
                        }
                    }

                    break;
                default:
                    throw new NotSupportedException();
            }

            return matrix;
        }
    }
}
