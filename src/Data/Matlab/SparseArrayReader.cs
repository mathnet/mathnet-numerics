// <copyright file="SparseArrayReader.cs" company="Math.NET">
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
using System.IO;
using System.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Storage;

namespace MathNet.Numerics.Data.Matlab
{
    internal static class SparseArrayReader<TDataType>
        where TDataType : struct, IEquatable<TDataType>, IFormattable
    {
        /// <summary>
        /// Populates a sparse matrix.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="isComplex">if set to <c>true</c> if the MATLAB complex flag is set.</param>
        /// <param name="rows">The number of rows.</param>
        /// <param name="columns">The number of columns.</param>
        /// <param name="size">The size of the block.</param>
        /// <returns>A populated sparse matrix.</returns>
        public static Matrix<TDataType> PopulateSparseMatrix(BinaryReader reader, bool isComplex, int rows, int columns, int size)
        {
            // Create matrix with CSR storage.
            var matrix = Matrix<TDataType>.Build.Sparse(columns, rows);

            // MATLAB sparse matrices are actually stored as CSC, so just read the data and then transpose.
            var storage = matrix.Storage as SparseCompressedRowMatrixStorage<TDataType>;

            // populate the row data array
            var ir = storage.ColumnIndices = new int[size/4];
            for (var i = 0; i < ir.Length; i++)
            {
                ir[i] = reader.ReadInt32();
            }

            Parser<TDataType>.AlignData(reader.BaseStream, size, false);

            // skip data type since it will always be int32
            reader.BaseStream.Seek(4, SeekOrigin.Current);

            // populate the column data array
            var jcsize = reader.ReadInt32();
            var jc = storage.RowPointers;
            if (jc.Length != jcsize/4)
            {
                throw new Exception("invalid jcsize");
            }

            for (var j = 0; j < jc.Length; j++)
            {
                jc[j] = reader.ReadInt32();
            }

            Parser<TDataType>.AlignData(reader.BaseStream, jcsize, false);

            var type = (DataType)reader.ReadInt32();
            var dataSize = reader.ReadInt32();

            var dataType = typeof (TDataType);

            // Allocate memory for matrix values
            var data = storage.Values = new TDataType[jc[columns]];

            if (dataType == typeof (double))
            {
                if (isComplex)
                {
                    throw new ArgumentException("Invalid TDataType. Matrix is stored as a complex matrix, but a real data type was given.");
                }

                PopulateDoubleSparseMatrix(type, (double[])(object)data, reader);
            }
            else if (dataType == typeof (float))
            {
                if (isComplex)
                {
                    throw new ArgumentException("Invalid TDataType. Matrix is stored as a complex matrix, but a real data type was given.");
                }

                PopulateSingleSparseMatrix(type, (float[])(object)data, reader);
            }
            else if (dataType == typeof (Complex))
            {
                PopulateComplexSparseMatrix(type, isComplex, (Complex[])(object)data, reader, dataSize);
            }
            else if (dataType == typeof (Complex32))
            {
                PopulateComplex32SparseMatrix(type, isComplex, (Complex32[])(object)data, reader, dataSize);
            }
            else
            {
                throw new NotSupportedException();
            }

            return matrix.Transpose();
        }

        /// <summary>
        /// Populates the double sparse matrix.
        /// </summary>
        /// <param name="type">The MATLAB data type.</param>
        /// <param name="data">The matrix values array.</param>
        /// <param name="reader">The reader to read from.</param>
        static void PopulateDoubleSparseMatrix(DataType type, double[] data, BinaryReader reader)
        {
            for (var i = 0; i < data.Length; i++)
            {
                data[i] = ReadDoubleValue(type, reader);
            }
        }

        /// <summary>
        /// Populates the float sparse matrix.
        /// </summary>
        /// <param name="type">The MATLAB data type.</param>
        /// <param name="data">The matrix values array.</param>
        /// <param name="reader">The reader to read from.</param>
        static void PopulateSingleSparseMatrix(DataType type, float[] data, BinaryReader reader)
        {
            for (var i = 0; i < data.Length; i++)
            {
                data[i] = (float)ReadDoubleValue(type, reader);
            }
        }

        /// <summary>
        /// Populates the complex sparse matrix.
        /// </summary>
        /// <param name="type">The MATLAB data type.</param>
        /// <param name="isComplex">if set to <c>true</c> if the MATLAB complex flag is set.</param>
        /// <param name="data">The matrix values array.</param>
        /// <param name="reader">The reader to read from.</param>
        /// <param name="dataSize">The length of the stored data.</param>
        static void PopulateComplexSparseMatrix(DataType type, bool isComplex, Complex[] data, BinaryReader reader, int dataSize)
        {
            for (var i = 0; i < data.Length; i++)
            {
                data[i] = ReadDoubleValue(type, reader);
            }

            if (isComplex)
            {
                var skip = dataSize%8;

                // skip pad
                reader.ReadBytes(skip);

                // skip header
                type = (DataType)reader.ReadInt32();
                reader.ReadInt32();

                for (var i = 0; i < data.Length; i++)
                {
                    data[i] += new Complex(0.0, ReadDoubleValue(type, reader));
                }
            }
        }

        /// <summary>
        /// Populates the complex32 sparse matrix.
        /// </summary>
        /// <param name="type">The MATLAB data type.</param>
        /// <param name="isComplex">if set to <c>true</c> if the MATLAB complex flag is set.</param>
        /// <param name="data">The matrix values array.</param>
        /// <param name="reader">The reader to read from.</param>
        /// <param name="dataSize">The length of the stored data.</param>
        static void PopulateComplex32SparseMatrix(DataType type, bool isComplex, Complex32[] data, BinaryReader reader, int dataSize)
        {
            for (var i = 0; i < data.Length; i++)
            {
                data[i] = (float)ReadDoubleValue(type, reader);
            }

            if (isComplex)
            {
                var skip = dataSize%8;

                // skip pad
                reader.ReadBytes(skip);

                // skip header
                type = (DataType)reader.ReadInt32();
                reader.ReadInt32();

                for (var i = 0; i < data.Length; i++)
                {
                    data[i] += new Complex32(0.0f, (float)ReadDoubleValue(type, reader));
                }
            }
        }

        static double ReadDoubleValue(DataType type, BinaryReader reader)
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
    }
}
