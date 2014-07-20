// <copyright file="NumericArrayReader.cs" company="Math.NET">
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

namespace MathNet.Numerics.Data.Matlab
{
    internal static class NumericArrayReader<TDataType>
        where TDataType : struct, IEquatable<TDataType>, IFormattable
    {
        /// <summary>
        /// Populates a dense matrix.
        /// </summary>
        /// <param name="type">The MATLAB data type.</param>
        /// <param name="reader">The reader to read from.</param>
        /// <param name="isComplex">if set to <c>true</c> if the MATLAB complex flag is set.</param>
        /// <param name="rows">The number of rows.</param>
        /// <param name="columns">The number of columns.</param>
        /// <param name="size">The length of the stored data.</param>
        /// <returns>Returns a populated dense matrix.</returns>
        public static Matrix<TDataType> PopulateDenseMatrix(DataType type, BinaryReader reader, bool isComplex, int rows, int columns, int size)
        {
            var dataType = typeof (TDataType);
            Matrix<TDataType> matrix;
            if (type == DataType.Double && dataType == typeof (double))
            {
                var count = rows*columns;
                var data = new double[count];
                Buffer.BlockCopy(reader.ReadBytes(count*Constants.SizeOfDouble), 0, data, 0, count*Constants.SizeOfDouble);
                matrix = (Matrix<TDataType>)(object)new LinearAlgebra.Double.DenseMatrix(rows, columns, data);
            }
            else if (type == DataType.Single && dataType == typeof (float))
            {
                var count = rows*columns;
                var data = new float[count];
                Buffer.BlockCopy(reader.ReadBytes(count*Constants.SizeOfFloat), 0, data, 0, count*Constants.SizeOfFloat);
                matrix = (Matrix<TDataType>)(object)new LinearAlgebra.Single.DenseMatrix(rows, columns, data);
            }
            else
            {
                matrix = Matrix<TDataType>.Build.Dense(rows, columns);

                if (dataType == typeof (double))
                {
                    if (isComplex)
                    {
                        throw new ArgumentException("Invalid TDataType. Matrix is stored as a complex matrix, but a real data type was given.");
                    }

                    PopulateDoubleDenseMatrix((Matrix<double>)(object)matrix, type, reader, rows, columns);
                }
                else if (dataType == typeof (float))
                {
                    if (isComplex)
                    {
                        throw new ArgumentException("Invalid TDataType. Matrix is stored as a complex matrix, but a real data type was given.");
                    }

                    PopulateSingleDenseMatrix((Matrix<float>)(object)matrix, type, reader, rows, columns);
                }
                else if (dataType == typeof (Complex))
                {
                    PopulateComplexDenseMatrix((Matrix<Complex>)(object)matrix, type, isComplex, reader, rows, columns, size);
                }
                else if (dataType == typeof (Complex32))
                {
                    PopulateComplex32DenseMatrix((Matrix<Complex32>)(object)matrix, type, isComplex, reader, rows, columns, size);
                }
                else
                {
                    throw new NotSupportedException();
                }
            }

            return matrix;
        }

        /// <summary>
        /// Populates the double dense matrix.
        /// </summary>
        /// <param name="matrix">The matrix to populate.</param>
        /// <param name="type">The MATLAB data type.</param>
        /// <param name="reader">The reader to read from.</param>
        /// <param name="rows">The number of rows.</param>
        /// <param name="columns">The number of columns.</param>
        static void PopulateDoubleDenseMatrix(Matrix<double> matrix, DataType type, BinaryReader reader, int rows, int columns)
        {
            for (var j = 0; j < columns; j++)
            {
                for (var i = 0; i < rows; i++)
                {
                    matrix.At(i, j, ReadDoubleValue(type, reader));
                }
            }
        }

        /// <summary>
        /// Populates the complex dense matrix.
        /// </summary>
        /// <param name="matrix">The matrix to populate.</param>
        /// <param name="type">The MATLAB data type.</param>
        /// <param name="isComplex">if set to <c>true</c> if the MATLAB complex flag is set.</param>
        /// <param name="reader">The reader to read from.</param>
        /// <param name="rows">The number of rows.</param>
        /// <param name="columns">The number of columns.</param>
        /// <param name="dataSize">The length of the stored data.</param>
        static void PopulateComplexDenseMatrix(Matrix<Complex> matrix, DataType type, bool isComplex, BinaryReader reader, int rows, int columns, int dataSize)
        {
            for (var j = 0; j < columns; j++)
            {
                for (var i = 0; i < rows; i++)
                {
                    matrix.At(i, j, ReadDoubleValue(type, reader));
                }
            }

            if (isComplex)
            {
                var skip = dataSize%8;

                // skip pad
                reader.ReadBytes(skip);

                // skip header
                type = (DataType)reader.ReadInt32();
                reader.ReadInt32();

                for (var j = 0; j < columns; j++)
                {
                    for (var i = 0; i < rows; i++)
                    {
                        matrix.At(i, j, new Complex(matrix.At(i, j).Real, ReadDoubleValue(type, reader)));
                    }
                }
            }
        }

        /// <summary>
        /// Populates the complex32 dense matrix.
        /// </summary>
        /// <param name="matrix">The matrix to populate.</param>
        /// <param name="type">The MATLAB data type.</param>
        /// <param name="isComplex">if set to <c>true</c> if the MATLAB complex flag is set.</param>
        /// <param name="reader">The reader to read from.</param>
        /// <param name="rows">The number of rows.</param>
        /// <param name="columns">The number of columns.</param>
        /// <param name="dataSize">The length of the stored data.</param>
        static void PopulateComplex32DenseMatrix(Matrix<Complex32> matrix, DataType type, bool isComplex, BinaryReader reader, int rows, int columns, int dataSize)
        {
            for (var j = 0; j < columns; j++)
            {
                for (var i = 0; i < rows; i++)
                {
                    matrix.At(i, j, (float)ReadDoubleValue(type, reader));
                }
            }

            if (isComplex)
            {
                var skip = dataSize%8;

                // skip pad
                reader.ReadBytes(skip);

                // skip header
                type = (DataType)reader.ReadInt32();
                reader.ReadInt32();

                for (var j = 0; j < columns; j++)
                {
                    for (var i = 0; i < rows; i++)
                    {
                        matrix.At(i, j, new Complex32(matrix.At(i, j).Real, (float)ReadDoubleValue(type, reader)));
                    }
                }
            }
        }

        /// <summary>
        /// Populates the float dense matrix.
        /// </summary>
        /// <param name="matrix">The matrix to populate.</param>
        /// <param name="type">The MATLAB data type.</param>
        /// <param name="reader">The reader to read from.</param>
        /// <param name="rows">The number of rows.</param>
        /// <param name="columns">The number of columns.</param>
        static void PopulateSingleDenseMatrix(Matrix<float> matrix, DataType type, BinaryReader reader, int rows, int columns)
        {
            for (var j = 0; j < columns; j++)
            {
                for (var i = 0; i < rows; i++)
                {
                    matrix.At(i, j, (float)ReadDoubleValue(type, reader));
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
