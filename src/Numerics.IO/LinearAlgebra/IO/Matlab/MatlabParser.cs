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

using System.IO.Compression;

namespace MathNet.Numerics.LinearAlgebra.IO.Matlab
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Numerics;
    using System.Reflection;
    using System.Text;
    using Generic;
    using Properties;
    using Complex32 = Numerics.Complex32;

    /// <summary>
    /// Parse a Matlab file
    /// </summary>
    /// <typeparam name="TDataType">The data type of the matrix.</typeparam>
    internal class MatlabParser<TDataType>
        where TDataType : struct, IEquatable<TDataType>, IFormattable
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
        /// Initializes a new instance of the <see cref="MatlabParser{TDataType}"/> class.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        public MatlabParser(string fileName)
            : this(fileName, new string[0])
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MatlabParser{TDataType}"/> class.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        public MatlabParser(Stream stream)
            : this(stream, new string[0])
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MatlabParser{TDataType}"/> class.
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
        /// Initializes a new instance of the <see cref="MatlabParser{TDataType}"/> class.
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
        /// <returns>The parsed Matlab file as a <see cref="MatlabFile{TDataType}"/> object.</returns>
        public MatlabFile<TDataType> Parse()
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
            using (var compressedStream = new MemoryStream(compressed, 2, compressed.Length-6))
            {
                using (var decompressor = new DeflateStream(compressedStream, CompressionMode.Decompress))
                using(var decompressed = new MemoryStream())
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
            }

            return data;
        }

        /// <summary>
        /// Adds a matrix from the actual file into our presentation of a matlab file.
        /// </summary>
        /// <param name="data">The data of the matrix.</param>
        /// <param name="file">The <see cref="MatlabFile{TDataType}"/> instance.</param>
        private void AddMatrix(byte[] data, MatlabFile<TDataType> file)
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

                    Matrix<TDataType> matrix;
                    switch (arrayClass)
                    {
                        case ArrayClass.Sparse:
                            matrix = PopulateSparseMatrix(reader, isComplex, rows, columns, size);
                            break;
                        case ArrayClass.Function:
                        case ArrayClass.Character:
                        case ArrayClass.Object:
                        case ArrayClass.Structure:
                        case ArrayClass.Cell:
                        case ArrayClass.Unknown:
                            throw new NotSupportedException();
                        default:
                            matrix = PopulateDenseMatrix(type, reader, isComplex, rows, columns, size);
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
        /// <param name="isComplex">if set to <c>true</c> if the Matlab complex flag is set.</param>
        /// <param name="rows">The number of rows.</param>
        /// <param name="columns">The number of columns.</param>
        /// <param name="size">The size of the block.</param>
        /// <returns>A populated sparse matrix.</returns>
        private static Matrix<TDataType> PopulateSparseMatrix(BinaryReader reader, bool isComplex, int rows, int columns, int size)
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
            var dataSize = reader.ReadInt32();

            var matrix = CreateMatrix(true, rows, columns);
            var dataType = typeof(TDataType);

            if (dataType == typeof(double))
            {
                if (isComplex)
                {
                    throw new ArgumentException("Invalid TDataType. Matrix is stored as a complex matrix, but a real data type was given.");
                }

                PopulateDoubleSparseMatrix((Matrix<double>)(object)matrix, type, ir, jc, reader);
            }
            else if (dataType == typeof(float))
            {
                if (isComplex)
                {
                    throw new ArgumentException("Invalid TDataType. Matrix is stored as a complex matrix, but a real data type was given.");
                }

                PopulateSingleSparseMatrix((Matrix<float>)(object)matrix, type, ir, jc, reader);
            }
            else if (dataType == typeof(Complex))
            {
                PopulateComplexSparseMatrix((Matrix<Complex>)(object)matrix, type, isComplex, ir, jc, reader, dataSize);
            }
            else if (dataType == typeof(Complex32))
            {
                PopulateComplex32SparseMatrix((Matrix<Complex32>)(object)matrix, type, isComplex, ir, jc, reader, dataSize);
            }
            else
            {
                throw new NotSupportedException();
            }

            return matrix;
        }

        /// <summary>
        /// Populates the double sparse matrix.
        /// </summary>
        /// <param name="matrix">The matrix to populate</param>
        /// <param name="type">The Matlab data type.</param>
        /// <param name="ir">The row indices.</param>
        /// <param name="jc">The column indices.</param>
        /// <param name="reader">The reader to read from.</param>
        private static void PopulateDoubleSparseMatrix(Matrix<double> matrix, DataType type, IList<int> ir, IList<int> jc, BinaryReader reader)
        {
            var col = 0;
            for (var i = 0; i < ir.Count; i++)
            {
                var row = ir[i];
                while(jc[col + 1] == i)
                {
                    col++;
                }

                switch (type)
                {
                    case DataType.Int8:
                        matrix.At(row, col, reader.ReadSByte());
                        break;
                    case DataType.UInt8:
                        matrix.At(row, col, reader.ReadByte());
                        break;
                    case DataType.Int16:
                        matrix.At(row, col, reader.ReadInt16());
                        break;
                    case DataType.UInt16:
                        matrix.At(row, col, reader.ReadUInt16());
                        break;
                    case DataType.Int32:
                        matrix.At(row, col, reader.ReadInt32());
                        break;
                    case DataType.UInt32:
                        matrix.At(row, col, reader.ReadUInt32());
                        break;
                    case DataType.Single:
                        matrix.At(row, col, reader.ReadSingle());
                        break;
                    case DataType.Int64:
                        matrix.At(row, col, reader.ReadInt64());
                        break;
                    case DataType.UInt64:
                        matrix.At(row, col, reader.ReadUInt64());
                        break;
                    case DataType.Double:
                        matrix.At(row, col, reader.ReadDouble());
                        break;
                    default:
                        throw new NotSupportedException();
                }
            }
        }

        /// <summary>
        /// Populates the float sparse matrix.
        /// </summary>
        /// <param name="matrix">The matrix to populate</param>
        /// <param name="type">The Matlab data type.</param>
        /// <param name="ir">The row indices.</param>
        /// <param name="jc">The column indices.</param>
        /// <param name="reader">The reader to read from.</param>
        private static void PopulateSingleSparseMatrix(Matrix<float> matrix, DataType type, IList<int> ir, IList<int> jc, BinaryReader reader)
        {
            var col = 0;
            for (var i = 0; i < ir.Count; i++)
            {
                var row = ir[i];
                while (jc[col + 1] == i)
                {
                    col++;
                }

                switch (type)
                {
                    case DataType.Int8:
                        matrix.At(row, col, reader.ReadSByte());
                        break;
                    case DataType.UInt8:
                        matrix.At(row, col, reader.ReadByte());
                        break;
                    case DataType.Int16:
                        matrix.At(row, col, reader.ReadInt16());
                        break;
                    case DataType.UInt16:
                        matrix.At(row, col, reader.ReadUInt16());
                        break;
                    case DataType.Int32:
                        matrix.At(row, col, reader.ReadInt32());
                        break;
                    case DataType.UInt32:
                        matrix.At(row, col, reader.ReadUInt32());
                        break;
                    case DataType.Single:
                        matrix.At(row, col, reader.ReadSingle());
                        break;
                    case DataType.Int64:
                        matrix.At(row, col, reader.ReadInt64());
                        break;
                    case DataType.UInt64:
                        matrix.At(row, col, reader.ReadUInt64());
                        break;
                    case DataType.Double:
                        matrix.At(row, col, Convert.ToSingle(reader.ReadDouble()));
                        break;
                    default:
                        throw new NotSupportedException();
                }
            }
        }

        /// <summary>
        /// Populates the complex sparse matrix.
        /// </summary>
        /// <param name="matrix">The matrix to populate</param>
        /// <param name="type">The Matlab data type.</param>
        /// <param name="isComplex">if set to <c>true</c> if the Matlab complex flag is set.</param>
        /// <param name="ir">The row indices.</param>
        /// <param name="jc">The column indices.</param>
        /// <param name="reader">The reader to read from.</param>
        /// <param name="dataSize">The length of the stored data.</param>
        private static void PopulateComplexSparseMatrix(Matrix<Complex> matrix, DataType type, bool isComplex, IList<int> ir, IList<int> jc, BinaryReader reader, int dataSize)
        {
            var col = 0;
            for (var i = 0; i < ir.Count; i++)
            {
                var row = ir[i];
                while (jc[col + 1] == i)
                {
                    col++;
                }

                switch (type)
                {
                    case DataType.Int8:
                        matrix.At(row, col, reader.ReadSByte());
                        break;
                    case DataType.UInt8:
                        matrix.At(row, col, reader.ReadByte());
                        break;
                    case DataType.Int16:
                        matrix.At(row, col, reader.ReadInt16());
                        break;
                    case DataType.UInt16:
                        matrix.At(row, col, reader.ReadUInt16());
                        break;
                    case DataType.Int32:
                        matrix.At(row, col, reader.ReadInt32());
                        break;
                    case DataType.UInt32:
                        matrix.At(row, col, reader.ReadUInt32());
                        break;
                    case DataType.Single:
                        matrix.At(row, col, reader.ReadSingle());
                        break;
                    case DataType.Int64:
                        matrix.At(row, col, reader.ReadInt64());
                        break;
                    case DataType.UInt64:
                        matrix.At(row, col, reader.ReadUInt64());
                        break;
                    case DataType.Double:
                        matrix.At(row, col, reader.ReadDouble());
                        break;
                    default:
                        throw new NotSupportedException();
                }
            }

            if (isComplex)
            {
                var skip = dataSize % 8;

                // skip pad
                reader.ReadBytes(skip);

                // skip header
                type = (DataType)reader.ReadInt32();
                reader.ReadInt32();
                col = 0;
                for (var i = 0; i < ir.Count; i++)
                {
                    var row = ir[i];
                    while (jc[col + 1] == i)
                    {
                        col++;
                    }

                    var real = matrix.At(row, col).Real;
                    switch (type)
                    {
                        case DataType.Int8:
                            matrix.At(row, col, new Complex(real, reader.ReadSByte()));
                            break;
                        case DataType.UInt8:
                            matrix.At(row, col, new Complex(real, reader.ReadByte()));
                            break;
                        case DataType.Int16:
                            matrix.At(row, col, new Complex(real, reader.ReadInt16()));
                            break;
                        case DataType.UInt16:
                            matrix.At(row, col, new Complex(real, reader.ReadUInt16()));
                            break;
                        case DataType.Int32:
                            matrix.At(row, col, new Complex(real, reader.ReadInt32()));
                            break;
                        case DataType.UInt32:
                            matrix.At(row, col, new Complex(real, reader.ReadUInt32()));
                            break;
                        case DataType.Single:
                            matrix.At(row, col, new Complex(real, reader.ReadSingle()));
                            break;
                        case DataType.Int64:
                            matrix.At(row, col, new Complex(real, reader.ReadInt64()));
                            break;
                        case DataType.UInt64:
                            matrix.At(row, col, new Complex(real, reader.ReadUInt64()));
                            break;
                        case DataType.Double:
                            matrix.At(row, col, new Complex(real, reader.ReadDouble()));
                            break;
                        default:
                            throw new NotSupportedException();
                    }
                }
            }
        }

        /// <summary>
        /// Populates the complex32 sparse matrix.
        /// </summary>
        /// <param name="matrix">The matrix to populate</param>
        /// <param name="type">The Matlab data type.</param>
        /// <param name="isComplex">if set to <c>true</c> if the Matlab complex flag is set.</param>
        /// <param name="ir">The row indices.</param>
        /// <param name="jc">The column indices.</param>
        /// <param name="reader">The reader to read from.</param>
        /// <param name="dataSize">The length of the stored data.</param>
        private static void PopulateComplex32SparseMatrix(Matrix<Complex32> matrix, DataType type, bool isComplex, IList<int> ir, IList<int> jc, BinaryReader reader, int dataSize)
        {
            var col = 0;
            for (var i = 0; i < ir.Count; i++)
            {
                var row = ir[i];
                while (jc[col + 1] == i)
                {
                    col++;
                }

                switch (type)
                {
                    case DataType.Int8:
                        matrix.At(row, col, reader.ReadSByte());
                        break;
                    case DataType.UInt8:
                        matrix.At(row, col, reader.ReadByte());
                        break;
                    case DataType.Int16:
                        matrix.At(row, col, reader.ReadInt16());
                        break;
                    case DataType.UInt16:
                        matrix.At(row, col, reader.ReadUInt16());
                        break;
                    case DataType.Int32:
                        matrix.At(row, col, reader.ReadInt32());
                        break;
                    case DataType.UInt32:
                        matrix.At(row, col, reader.ReadUInt32());
                        break;
                    case DataType.Single:
                        matrix.At(row, col, reader.ReadSingle());
                        break;
                    case DataType.Int64:
                        matrix.At(row, col, reader.ReadInt64());
                        break;
                    case DataType.UInt64:
                        matrix.At(row, col, reader.ReadUInt64());
                        break;
                    case DataType.Double:
                        matrix.At(row, col, Convert.ToSingle(reader.ReadDouble()));
                        break;
                    default:
                        throw new NotSupportedException();
                }
            }

            if (isComplex)
            {
                var skip = dataSize % 8;

                // skip pad
                reader.ReadBytes(skip);

                // skip header
                type = (DataType)reader.ReadInt32();
                reader.ReadInt32();
                
                col = 0;
                for (var i = 0; i < ir.Count; i++)
                {
                    var row = ir[i];
                    while (jc[col + 1] == i)
                    {
                        col++;
                    }

                    var real = matrix.At(row, col).Real;
                    switch (type)
                    {
                        case DataType.Int8:
                            matrix.At(row, col, new Complex32(real, reader.ReadSByte()));
                            break;
                        case DataType.UInt8:
                            matrix.At(row, col, new Complex32(real, reader.ReadByte()));
                            break;
                        case DataType.Int16:
                            matrix.At(row, col, new Complex32(real, reader.ReadInt16()));
                            break;
                        case DataType.UInt16:
                            matrix.At(row, col, new Complex32(real, reader.ReadUInt16()));
                            break;
                        case DataType.Int32:
                            matrix.At(row, col, new Complex32(real, reader.ReadInt32()));
                            break;
                        case DataType.UInt32:
                            matrix.At(row, col, new Complex32(real, reader.ReadUInt32()));
                            break;
                        case DataType.Single:
                            matrix.At(row, col, new Complex32(real, reader.ReadSingle()));
                            break;
                        case DataType.Int64:
                            matrix.At(row, col, new Complex32(real, reader.ReadInt64()));
                            break;
                        case DataType.UInt64:
                            matrix.At(row, col, new Complex32(real, reader.ReadUInt64()));
                            break;
                        case DataType.Double:
                            matrix.At(row, col, new Complex32(real, Convert.ToSingle(reader.ReadDouble())));
                            break;
                        default:
                            throw new NotSupportedException();
                    }
                }
            }
        }

        /// <summary>
        /// Populates a dense matrix.
        /// </summary>
        /// <param name="type">The Matlab data type.</param>
        /// <param name="reader">The reader to read from.</param>
        /// <param name="isComplex">if set to <c>true</c> if the Matlab complex flag is set.</param>
        /// <param name="rows">The number of rows.</param>
        /// <param name="columns">The number of columns.</param>
        /// <param name="size">The length of the stored data.</param>
        /// <returns>Returns a populated dense matrix.</returns>
        private static Matrix<TDataType> PopulateDenseMatrix(DataType type, BinaryReader reader, bool isComplex, int rows, int columns, int size)
        {
            var matrix = CreateMatrix(false, rows, columns);

            var dataType = typeof(TDataType);

            if (dataType == typeof(double))
            {
                if (isComplex)
                {
                    throw new ArgumentException("Invalid TDataType. Matrix is stored as a complex matrix, but a real data type was given.");
                }

                PopulateDoubleDenseMatrix((Matrix<double>)(object)matrix, type, reader, rows, columns);
            }
            else if (dataType == typeof(float))
            {
                if (isComplex)
                {
                    throw new ArgumentException("Invalid TDataType. Matrix is stored as a complex matrix, but a real data type was given.");
                }

                PopulateSingleDenseMatrix((Matrix<float>)(object)matrix, type, reader, rows, columns);
            }
            else if (dataType == typeof(Complex))
            {
                PopulateComplexDenseMatrix((Matrix<Complex>)(object)matrix, type, isComplex, reader, rows, columns, size);
            }
            else if (dataType == typeof(Complex32))
            {
                PopulateComplex32DenseMatrix((Matrix<Complex32>)(object)matrix, type, isComplex, reader, rows, columns, size);
            }
            else
            {
                throw new NotSupportedException();
            }

            return matrix;
        }

        /// <summary>
        /// Populates the double dense matrix.
        /// </summary>
        /// <param name="matrix">The matrix to populate.</param>
        /// <param name="type">The Matlab data type.</param>
        /// <param name="reader">The reader to read from.</param>
        /// <param name="rows">The number of rows.</param>
        /// <param name="columns">The number of columns.</param>
        public static void PopulateDoubleDenseMatrix(Matrix<double> matrix, DataType type, BinaryReader reader, int rows, int columns)
        {
            switch (type)
            {
                case DataType.Int8:
                    for (var j = 0; j < columns; j++)
                    {
                        for (var i = 0; i < rows; i++)
                        {
                            matrix.At(i, j, reader.ReadSByte());
                        }
                    }

                    break;
                case DataType.UInt8:
                    for (var j = 0; j < columns; j++)
                    {
                        for (var i = 0; i < rows; i++)
                        {
                            matrix.At(i, j, reader.ReadByte());
                        }
                    }

                    break;
                case DataType.Int16:
                    for (var j = 0; j < columns; j++)
                    {
                        for (var i = 0; i < rows; i++)
                        {
                            matrix.At(i, j, reader.ReadInt16());
                        }
                    }

                    break;
                case DataType.UInt16:
                    for (var j = 0; j < columns; j++)
                    {
                        for (var i = 0; i < rows; i++)
                        {
                            matrix.At(i, j, reader.ReadUInt16());
                        }
                    }

                    break;
                case DataType.Int32:
                    for (var j = 0; j < columns; j++)
                    {
                        for (var i = 0; i < rows; i++)
                        {
                            matrix.At(i, j, reader.ReadInt32());
                        }
                    }

                    break;
                case DataType.UInt32:
                    for (var j = 0; j < columns; j++)
                    {
                        for (var i = 0; i < rows; i++)
                        {
                            matrix.At(i, j, reader.ReadUInt32());
                        }
                    }

                    break;
                case DataType.Single:
                    for (var j = 0; j < columns; j++)
                    {
                        for (var i = 0; i < rows; i++)
                        {
                            matrix.At(i, j, reader.ReadSingle());
                        }
                    }

                    break;
                case DataType.Int64:
                    for (var j = 0; j < columns; j++)
                    {
                        for (var i = 0; i < rows; i++)
                        {
                            matrix.At(i, j, reader.ReadInt64());
                        }
                    }

                    break;
                case DataType.UInt64:
                    for (var j = 0; j < columns; j++)
                    {
                        for (var i = 0; i < rows; i++)
                        {
                            matrix.At(i, j, reader.ReadUInt64());
                        }
                    }

                    break;
                case DataType.Double:
                    for (var j = 0; j < columns; j++)
                    {
                        for (var i = 0; i < rows; i++)
                        {
                            matrix.At(i, j, reader.ReadDouble());
                        }
                    }

                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Populates the complex dense matrix.
        /// </summary>
        /// <param name="matrix">The matrix to populate.</param>
        /// <param name="type">The Matlab data type.</param>
        /// <param name="isComplex">if set to <c>true</c> if the Matlab complex flag is set.</param>
        /// <param name="reader">The reader to read from.</param>
        /// <param name="rows">The number of rows.</param>
        /// <param name="columns">The number of columns.</param>
        /// <param name="dataSize">The length of the stored data.</param>
        public static void PopulateComplexDenseMatrix(Matrix<Complex> matrix, DataType type, bool isComplex, BinaryReader reader, int rows, int columns, int dataSize)
        {
            switch (type)
            {
                case DataType.Int8:
                    for (var j = 0; j < columns; j++)
                    {
                        for (var i = 0; i < rows; i++)
                        {
                            matrix.At(i, j, reader.ReadSByte());
                        }
                    }

                    break;
                case DataType.UInt8:
                    for (var j = 0; j < columns; j++)
                    {
                        for (var i = 0; i < rows; i++)
                        {
                            matrix.At(i, j, reader.ReadByte());
                        }
                    }

                    break;
                case DataType.Int16:
                    for (var j = 0; j < columns; j++)
                    {
                        for (var i = 0; i < rows; i++)
                        {
                            matrix.At(i, j, reader.ReadInt16());
                        }
                    }

                    break;
                case DataType.UInt16:
                    for (var j = 0; j < columns; j++)
                    {
                        for (var i = 0; i < rows; i++)
                        {
                            matrix.At(i, j, reader.ReadUInt16());
                        }
                    }

                    break;
                case DataType.Int32:
                    for (var j = 0; j < columns; j++)
                    {
                        for (var i = 0; i < rows; i++)
                        {
                            matrix.At(i, j, reader.ReadInt32());
                        }
                    }

                    break;
                case DataType.UInt32:
                    for (var j = 0; j < columns; j++)
                    {
                        for (var i = 0; i < rows; i++)
                        {
                            matrix.At(i, j, reader.ReadUInt32());
                        }
                    }

                    break;
                case DataType.Single:
                    for (var j = 0; j < columns; j++)
                    {
                        for (var i = 0; i < rows; i++)
                        {
                            matrix.At(i, j, reader.ReadSingle());
                        }
                    }

                    break;
                case DataType.Int64:
                    for (var j = 0; j < columns; j++)
                    {
                        for (var i = 0; i < rows; i++)
                        {
                            matrix.At(i, j, reader.ReadInt64());
                        }
                    }

                    break;
                case DataType.UInt64:
                    for (var j = 0; j < columns; j++)
                    {
                        for (var i = 0; i < rows; i++)
                        {
                            matrix.At(i, j, reader.ReadUInt64());
                        }
                    }

                    break;
                case DataType.Double:
                    for (var j = 0; j < columns; j++)
                    {
                        for (var i = 0; i < rows; i++)
                        {
                            matrix.At(i, j, reader.ReadDouble());
                        }
                    }

                    break;
                default:
                    throw new NotSupportedException();
            }

            if (isComplex)
            {
                var skip = dataSize % 8;

                // skip pad
                reader.ReadBytes(skip);

                // skip header
                type = (DataType)reader.ReadInt32();
                reader.ReadInt32();

                switch (type)
                {
                    case DataType.Int8:
                        for (var j = 0; j < columns; j++)
                        {
                            for (var i = 0; i < rows; i++)
                            {
                                matrix.At(i, j, new Complex(matrix.At(i, j).Real, reader.ReadSByte()));
                            }
                        }

                        break;
                    case DataType.UInt8:
                        for (var j = 0; j < columns; j++)
                        {
                            for (var i = 0; i < rows; i++)
                            {
                                matrix.At(i, j, new Complex(matrix.At(i, j).Real, reader.ReadByte()));
                            }
                        }

                        break;
                    case DataType.Int16:
                        for (var j = 0; j < columns; j++)
                        {
                            for (var i = 0; i < rows; i++)
                            {
                                matrix.At(i, j, new Complex(matrix.At(i, j).Real, reader.ReadInt16()));
                            }
                        }

                        break;
                    case DataType.UInt16:
                        for (var j = 0; j < columns; j++)
                        {
                            for (var i = 0; i < rows; i++)
                            {
                                matrix.At(i, j, new Complex(matrix.At(i, j).Real, reader.ReadUInt16()));
                            }
                        }

                        break;
                    case DataType.Int32:
                        for (var j = 0; j < columns; j++)
                        {
                            for (var i = 0; i < rows; i++)
                            {
                                matrix.At(i, j, new Complex(matrix.At(i, j).Real, reader.ReadInt32()));
                            }
                        }

                        break;
                    case DataType.UInt32:
                        for (var j = 0; j < columns; j++)
                        {
                            for (var i = 0; i < rows; i++)
                            {
                                matrix.At(i, j, new Complex(matrix.At(i, j).Real, reader.ReadUInt32()));
                            }
                        }

                        break;
                    case DataType.Single:
                        for (var j = 0; j < columns; j++)
                        {
                            for (var i = 0; i < rows; i++)
                            {
                                matrix.At(i, j, new Complex(matrix.At(i, j).Real, reader.ReadSingle()));
                            }
                        }

                        break;
                    case DataType.Int64:
                        for (var j = 0; j < columns; j++)
                        {
                            for (var i = 0; i < rows; i++)
                            {
                                matrix.At(i, j, new Complex(matrix.At(i, j).Real, reader.ReadInt64()));
                            }
                        }

                        break;
                    case DataType.UInt64:
                        for (var j = 0; j < columns; j++)
                        {
                            for (var i = 0; i < rows; i++)
                            {
                                matrix.At(i, j, new Complex(matrix.At(i, j).Real, reader.ReadUInt64()));
                            }
                        }

                        break;
                    case DataType.Double:
                        for (var j = 0; j < columns; j++)
                        {
                            for (var i = 0; i < rows; i++)
                            {
                                matrix.At(i, j, new Complex(matrix.At(i, j).Real, reader.ReadDouble()));
                            }
                        }

                        break;
                    default:
                        throw new NotSupportedException();
                }
            }
        }

        /// <summary>
        /// Populates the complex32 dense matrix.
        /// </summary>
        /// <param name="matrix">The matrix to populate.</param>
        /// <param name="type">The Matlab data type.</param>
        /// <param name="isComplex">if set to <c>true</c> if the Matlab complex flag is set.</param>
        /// <param name="reader">The reader to read from.</param>
        /// <param name="rows">The number of rows.</param>
        /// <param name="columns">The number of columns.</param>
        /// <param name="dataSize">The length of the stored data.</param>
        public static void PopulateComplex32DenseMatrix(Matrix<Complex32> matrix, DataType type, bool isComplex, BinaryReader reader, int rows, int columns, int dataSize)
        {
            switch (type)
            {
                case DataType.Int8:
                    for (var j = 0; j < columns; j++)
                    {
                        for (var i = 0; i < rows; i++)
                        {
                            matrix.At(i, j, reader.ReadSByte());
                        }
                    }

                    break;
                case DataType.UInt8:
                    for (var j = 0; j < columns; j++)
                    {
                        for (var i = 0; i < rows; i++)
                        {
                            matrix.At(i, j, reader.ReadByte());
                        }
                    }

                    break;
                case DataType.Int16:
                    for (var j = 0; j < columns; j++)
                    {
                        for (var i = 0; i < rows; i++)
                        {
                            matrix.At(i, j, reader.ReadInt16());
                        }
                    }

                    break;
                case DataType.UInt16:
                    for (var j = 0; j < columns; j++)
                    {
                        for (var i = 0; i < rows; i++)
                        {
                            matrix.At(i, j, reader.ReadUInt16());
                        }
                    }

                    break;
                case DataType.Int32:
                    for (var j = 0; j < columns; j++)
                    {
                        for (var i = 0; i < rows; i++)
                        {
                            matrix.At(i, j, reader.ReadInt32());
                        }
                    }

                    break;
                case DataType.UInt32:
                    for (var j = 0; j < columns; j++)
                    {
                        for (var i = 0; i < rows; i++)
                        {
                            matrix.At(i, j, reader.ReadUInt32());
                        }
                    }

                    break;
                case DataType.Single:
                    for (var j = 0; j < columns; j++)
                    {
                        for (var i = 0; i < rows; i++)
                        {
                            matrix.At(i, j, reader.ReadSingle());
                        }
                    }

                    break;
                case DataType.Int64:
                    for (var j = 0; j < columns; j++)
                    {
                        for (var i = 0; i < rows; i++)
                        {
                            matrix.At(i, j, reader.ReadInt64());
                        }
                    }

                    break;
                case DataType.UInt64:
                    for (var j = 0; j < columns; j++)
                    {
                        for (var i = 0; i < rows; i++)
                        {
                            matrix.At(i, j, reader.ReadUInt64());
                        }
                    }

                    break;
                case DataType.Double:
                    for (var j = 0; j < columns; j++)
                    {
                        for (var i = 0; i < rows; i++)
                        {
                            matrix.At(i, j, Convert.ToSingle(reader.ReadDouble()));
                        }
                    }

                    break;
                default:
                    throw new NotSupportedException();
            }

            if (isComplex)
            {
                var skip = dataSize % 8;

                // skip pad
                reader.ReadBytes(skip);

                // skip header
                type = (DataType)reader.ReadInt32();
                reader.ReadInt32();

                switch (type)
                {
                    case DataType.Int8:
                        for (var j = 0; j < columns; j++)
                        {
                            for (var i = 0; i < rows; i++)
                            {
                                matrix.At(i, j, new Complex32(matrix.At(i, j).Real, reader.ReadSByte()));
                            }
                        }

                        break;
                    case DataType.UInt8:
                        for (var j = 0; j < columns; j++)
                        {
                            for (var i = 0; i < rows; i++)
                            {
                                matrix.At(i, j, new Complex32(matrix.At(i, j).Real, reader.ReadByte()));
                            }
                        }

                        break;
                    case DataType.Int16:
                        for (var j = 0; j < columns; j++)
                        {
                            for (var i = 0; i < rows; i++)
                            {
                                matrix.At(i, j, new Complex32(matrix.At(i, j).Real, reader.ReadInt16()));
                            }
                        }

                        break;
                    case DataType.UInt16:
                        for (var j = 0; j < columns; j++)
                        {
                            for (var i = 0; i < rows; i++)
                            {
                                matrix.At(i, j, new Complex32(matrix.At(i, j).Real, reader.ReadUInt16()));
                            }
                        }

                        break;
                    case DataType.Int32:
                        for (var j = 0; j < columns; j++)
                        {
                            for (var i = 0; i < rows; i++)
                            {
                                matrix.At(i, j, new Complex32(matrix.At(i, j).Real, reader.ReadInt32()));
                            }
                        }

                        break;
                    case DataType.UInt32:
                        for (var j = 0; j < columns; j++)
                        {
                            for (var i = 0; i < rows; i++)
                            {
                                matrix.At(i, j, new Complex32(matrix.At(i, j).Real, reader.ReadUInt32()));
                            }
                        }

                        break;
                    case DataType.Single:
                        for (var j = 0; j < columns; j++)
                        {
                            for (var i = 0; i < rows; i++)
                            {
                                matrix.At(i, j, new Complex32(matrix.At(i, j).Real, reader.ReadSingle()));
                            }
                        }

                        break;
                    case DataType.Int64:
                        for (var j = 0; j < columns; j++)
                        {
                            for (var i = 0; i < rows; i++)
                            {
                                matrix.At(i, j, new Complex32(matrix.At(i, j).Real, reader.ReadInt64()));
                            }
                        }

                        break;
                    case DataType.UInt64:
                        for (var j = 0; j < columns; j++)
                        {
                            for (var i = 0; i < rows; i++)
                            {
                                matrix.At(i, j, new Complex32(matrix.At(i, j).Real, reader.ReadUInt64()));
                            }
                        }

                        break;
                    case DataType.Double:
                        for (var j = 0; j < columns; j++)
                        {
                            for (var i = 0; i < rows; i++)
                            {
                                matrix.At(i, j, new Complex32(matrix.At(i, j).Real, Convert.ToSingle(reader.ReadDouble())));
                            }
                        }

                        break;
                    default:
                        throw new NotSupportedException();
                }
            }
        }

        /// <summary>
        /// Populates the float dense matrix.
        /// </summary>
        /// <param name="matrix">The matrix to populate.</param>
        /// <param name="type">The Matlab data type.</param>
        /// <param name="reader">The reader to read from.</param>
        /// <param name="rows">The number of rows.</param>
        /// <param name="columns">The number of columns.</param>
        public static void PopulateSingleDenseMatrix(Matrix<float> matrix, DataType type, BinaryReader reader, int rows, int columns)
        {
            switch (type)
            {
                case DataType.Int8:
                    for (var j = 0; j < columns; j++)
                    {
                        for (var i = 0; i < rows; i++)
                        {
                            matrix.At(i, j, reader.ReadSByte());
                        }
                    }

                    break;
                case DataType.UInt8:
                    for (var j = 0; j < columns; j++)
                    {
                        for (var i = 0; i < rows; i++)
                        {
                            matrix.At(i, j, reader.ReadByte());
                        }
                    }

                    break;
                case DataType.Int16:
                    for (var j = 0; j < columns; j++)
                    {
                        for (var i = 0; i < rows; i++)
                        {
                            matrix.At(i, j, reader.ReadInt16());
                        }
                    }

                    break;
                case DataType.UInt16:
                    for (var j = 0; j < columns; j++)
                    {
                        for (var i = 0; i < rows; i++)
                        {
                            matrix.At(i, j, reader.ReadUInt16());
                        }
                    }

                    break;
                case DataType.Int32:
                    for (var j = 0; j < columns; j++)
                    {
                        for (var i = 0; i < rows; i++)
                        {
                            matrix.At(i, j, reader.ReadInt32());
                        }
                    }

                    break;
                case DataType.UInt32:
                    for (var j = 0; j < columns; j++)
                    {
                        for (var i = 0; i < rows; i++)
                        {
                            matrix.At(i, j, reader.ReadUInt32());
                        }
                    }

                    break;
                case DataType.Single:
                    for (var j = 0; j < columns; j++)
                    {
                        for (var i = 0; i < rows; i++)
                        {
                            matrix.At(i, j, reader.ReadSingle());
                        }
                    }

                    break;
                case DataType.Int64:
                    for (var j = 0; j < columns; j++)
                    {
                        for (var i = 0; i < rows; i++)
                        {
                            matrix.At(i, j, reader.ReadInt64());
                        }
                    }

                    break;
                case DataType.UInt64:
                    for (var j = 0; j < columns; j++)
                    {
                        for (var i = 0; i < rows; i++)
                        {
                            matrix.At(i, j, reader.ReadUInt64());
                        }
                    }

                    break;
                case DataType.Double:
                    for (var j = 0; j < columns; j++)
                    {
                        for (var i = 0; i < rows; i++)
                        {
                            matrix.At(i, j, Convert.ToSingle(reader.ReadDouble()));
                        }
                    }

                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Creates a matrix.
        /// </summary>
        /// <param name="sparse">if set to <c>true</c>, creates a sparse matrix.</param>
        /// <param name="rows">The number of rows.</param>
        /// <param name="columns">The number of columns.</param>
        /// <returns>A matrix with the specified storage.</returns>
        private static Matrix<TDataType> CreateMatrix(bool sparse, int rows, int columns)
        {
            ConstructorInfo constructor;
            var dataType = typeof(TDataType);

            if (sparse)
            {
                if (dataType == typeof(double))
                {
                    constructor = typeof(LinearAlgebra.Double.SparseMatrix).GetConstructor(new[] { typeof(int), typeof(int) });
                }
                else if (dataType == typeof(float))
                {
                    constructor = typeof(LinearAlgebra.Single.SparseMatrix).GetConstructor(new[] { typeof(int), typeof(int) });
                }
                else if (dataType == typeof(Complex))
                {
                    constructor = typeof(LinearAlgebra.Complex.SparseMatrix).GetConstructor(new[] { typeof(int), typeof(int) });
                }
                else if (dataType == typeof(Complex32))
                {
                    constructor = typeof(LinearAlgebra.Complex32.SparseMatrix).GetConstructor(new[] { typeof(int), typeof(int) });
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
            else
            {
                if (dataType == typeof(double))
                {
                    constructor = typeof(LinearAlgebra.Double.DenseMatrix).GetConstructor(new[] { typeof(int), typeof(int) });
                }
                else if (dataType == typeof(float))
                {
                    constructor = typeof(LinearAlgebra.Single.DenseMatrix).GetConstructor(new[] { typeof(int), typeof(int) });
                }
                else if (dataType == typeof(Complex))
                {
                    constructor = typeof(LinearAlgebra.Complex.DenseMatrix).GetConstructor(new[] { typeof(int), typeof(int) });
                }
                else if (dataType == typeof(Complex32))
                {
                    constructor = typeof(LinearAlgebra.Complex32.DenseMatrix).GetConstructor(new[] { typeof(int), typeof(int) });
                }
                else
                {
                    throw new NotSupportedException();
                }
            }

            return (Matrix<TDataType>)constructor.Invoke(new object[] { rows, columns });
        }
    }
}
