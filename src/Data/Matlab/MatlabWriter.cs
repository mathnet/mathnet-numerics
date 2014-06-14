// <copyright file="MatlabWriter.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
//
// Copyright (c) 2009-2013 Math.NET
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
    /// Writes matrices to a Matlab file.
    /// </summary>
    public class MatlabMatrixWriter : IDisposable
    {
        /// <summary>
        /// The file header value
        /// </summary>
        const string HeaderText = "MATLAB 5.0 MAT-file, Platform: .NET 4 - Math.NET Numerics, Created on: ";

        /// <summary>
        /// The length of the header text.
        /// </summary>
        const int HeaderTextLength = 116;

        /// <summary>
        /// Have we written the header yet.
        /// </summary>
        bool _headerWritten;

        /// <summary>
        /// The binary writer to write to.
        /// </summary>
        BinaryWriter _writer;

        /// <summary>
        /// Initializes a new instance of the <see cref="MatlabMatrixWriter"/> class.
        /// </summary>
        /// <param name="filename">The name of the Matlab file to save the matrices to.</param>
        public MatlabMatrixWriter(string filename)
        {
            if (string.IsNullOrEmpty(filename))
            {
                throw new ArgumentException(Resources.StringNullOrEmpty, "filename");
            }

            _writer =
                new BinaryWriter(
                    new BufferedStream(new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None)));
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (_writer != null)
            {
                _writer.Flush();
                _writer.Close();
                _writer = null;
            }
        }

        /// <summary>
        /// Writes the given <see cref="Matrix{T}"/> to the file. 
        /// </summary>
        /// <param name="matrix">The matrix to write.</param>
        /// <param name="name">The name of the matrix to store in the file.</param>
        /// <exception cref="ArgumentNullException">If either <paramref name="matrix"/> or <paramref name="name"/> is <c>null</c>.</exception>
        /// <typeparam name="TDataType">The data type of the Matrix. It can be either: double, float, Complex, or Complex32.</typeparam>
        public void WriteMatrix<TDataType>(Matrix<TDataType> matrix, string name)
            where TDataType : struct, IEquatable<TDataType>, IFormattable
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

            if (!_headerWritten)
            {
                WriteHeader();
                _headerWritten = true;
            }

            // write datatype
            _writer.Write((int) DataType.Compressed);

            byte[] data;

            if (typeof (TDataType) == typeof (double))
            {
                var o = matrix as LinearAlgebra.Double.SparseMatrix;
                data = o != null
                    ? GetSparseDataArray((LinearAlgebra.Double.SparseMatrix) (object) matrix, name)
                    : GetDenseDataArray((LinearAlgebra.Double.Matrix) (object) matrix, name);
            }
            else if (typeof (TDataType) == typeof (float))
            {
                var o = matrix as LinearAlgebra.Single.SparseMatrix;
                data = o != null
                    ? GetSparseDataArray((LinearAlgebra.Single.SparseMatrix) (object) matrix, name)
                    : GetDenseDataArray((LinearAlgebra.Single.Matrix) (object) matrix, name);
            }
            else if (typeof (TDataType) == typeof (Complex))
            {
                var o = matrix as LinearAlgebra.Complex.SparseMatrix;
                data = o != null
                    ? GetSparseDataArray((LinearAlgebra.Complex.SparseMatrix) (object) matrix, name)
                    : GetDenseDataArray((LinearAlgebra.Complex.Matrix) (object) matrix, name);
            }
            else if (typeof (TDataType) == typeof (Complex32))
            {
                var o = matrix as LinearAlgebra.Complex32.SparseMatrix;
                data = o != null
                    ? GetSparseDataArray((LinearAlgebra.Complex32.SparseMatrix) (object) matrix, name)
                    : GetDenseDataArray((LinearAlgebra.Complex32.Matrix) (object) matrix, name);
            }
            else
            {
                throw new NotSupportedException();
            }

            WriteCompressedData(data);
        }

        /// <summary>
        /// Writes the given <see cref="Matrix{TDataType}"/> to the file.
        /// </summary>
        /// <param name="matrices">The matrices to write.</param>
        /// <param name="names">The names of the matrices to store in the file.</param>
        /// <exception cref="ArgumentNullException">If either <paramref name="matrices"/> or <paramref name="names"/> is null.</exception>
        /// <typeparam name="TDataType">The data type of the Matrix. It can be either: double, float, Complex, or Complex32.</typeparam>
        public void WriteMatrices<TDataType>(IList<Matrix<TDataType>> matrices, IList<string> names)
            where TDataType : struct, IEquatable<TDataType>, IFormattable
        {
            if (matrices == null)
            {
                throw new ArgumentNullException("matrices");
            }

            if (names == null)
            {
                throw new ArgumentNullException("names");
            }

            if (matrices.Count != names.Count)
            {
                throw new ArgumentException(Resources.ArgumentMatrixDimensions);
            }

            for (var i = 0; i < matrices.Count; i++)
            {
                WriteMatrix(matrices[i], names[i]);
            }
        }

        /// <summary>
        /// Closes the stream the being written to.
        /// </summary>
        /// <remarks>Calls <see cref="IDisposable.Dispose"/>.</remarks>
        public void Close()
        {
            Dispose();
        }

        /// <summary>
        /// Writes the matrix tag and name.
        /// </summary>
        /// <param name="writer">The writer we are using.</param>
        /// <param name="arrayClass">The array class we are writing.</param>
        /// <param name="isComplex">if set to <c>true</c> if this a complex matrix.</param>
        /// <param name="name">The name name of the matrix.</param>
        /// <param name="rows">The number of rows.</param>
        /// <param name="columns">The columns of columns.</param>
        /// <param name="nzmax">The maximum number of non-zero elements.</param>
        static void WriteMatrixTagAndName(BinaryWriter writer, ArrayClass arrayClass, bool isComplex,
            string name, int rows, int columns, int nzmax)
        {
            writer.Write((int) DataType.Matrix);

            // add place holder for data size
            writer.Write(0);

            // write flag, data type and size
            writer.Write((int) DataType.UInt32);
            writer.Write(8);

            // write array class and flags 
            writer.Write((byte) arrayClass);
            if (isComplex)
            {
                writer.Write((byte) ArrayFlags.Complex);
            }
            else
            {
                writer.Write((byte) 0);
            }

            writer.Write((short) 0);
            writer.Write(nzmax);

            // write dimensions
            writer.Write((int) DataType.Int32);
            writer.Write(8);
            writer.Write(rows);
            writer.Write(columns);

            var nameBytes = Encoding.ASCII.GetBytes(name);

            // write name
            if (nameBytes.Length > 4)
            {
                writer.Write((int) DataType.Int8);
                writer.Write(nameBytes.Length);
                writer.Write(nameBytes);
                var pad = 8 - (nameBytes.Length%8);
                PadData(writer, pad);
            }
            else
            {
                writer.Write((short) DataType.Int8);
                writer.Write((short) nameBytes.Length);
                writer.Write(nameBytes);
                PadData(writer, 4 - nameBytes.Length);
            }
        }

        /// <summary>
        /// Compresses the data array.
        /// </summary>
        /// <param name="data">The data to compress.</param>
        /// <returns>The compressed data.</returns>
        static byte[] CompressData(byte[] data)
        {
            var adler = BitConverter.GetBytes(Adler32.Compute(data));
            using (var compressedStream = new MemoryStream())
            {
                compressedStream.WriteByte(0x58);
                compressedStream.WriteByte(0x85);
                using (var outputStream = new DeflateStream(compressedStream, CompressionMode.Compress, true))
                {
                    outputStream.Write(data, 0, data.Length);
                }
                compressedStream.WriteByte(adler[3]);
                compressedStream.WriteByte(adler[2]);
                compressedStream.WriteByte(adler[1]);
                compressedStream.WriteByte(adler[0]);
                return compressedStream.ToArray();
            }
        }

        /// <summary>
        /// Gets the dense data array.
        /// </summary>
        /// <param name="matrix">The matrix to get the data from.</param>
        /// <param name="name">The name of the matrix.</param>
        /// <returns>The matrix data as an array.</returns>
        static byte[] GetDenseDataArray(Matrix<double> matrix, string name)
        {
            byte[] data;
            using (var dataMemoryStream = new MemoryStream())
            using (var dataWriter = new BinaryWriter(dataMemoryStream))
            {
                WriteMatrixTagAndName(dataWriter, ArrayClass.Double, false, name, matrix.RowCount, matrix.ColumnCount, 0);

                // write data
                dataWriter.Write((int) DataType.Double);
                dataWriter.Write(matrix.RowCount*matrix.ColumnCount*8);

                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    var column = matrix.Column(j);
                    foreach (var value in column)
                    {
                        dataWriter.Write(value);
                    }
                }

                data = dataMemoryStream.ToArray();
            }

            return data;
        }

        /// <summary>
        /// Gets the dense data array.
        /// </summary>
        /// <param name="matrix">The matrix to get the data from.</param>
        /// <param name="name">The name of the matrix.</param>
        /// <returns>The matrix data as an array.</returns>
        static byte[] GetDenseDataArray(Matrix<float> matrix, string name)
        {
            byte[] data;
            using (var dataMemoryStream = new MemoryStream())
            using (var dataWriter = new BinaryWriter(dataMemoryStream))
            {
                WriteMatrixTagAndName(dataWriter, ArrayClass.Single, false, name, matrix.RowCount, matrix.ColumnCount, 0);

                // write data
                dataWriter.Write((int) DataType.Single);

                dataWriter.Write(matrix.RowCount*matrix.ColumnCount*4);

                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    var column = matrix.Column(j);
                    foreach (var value in column)
                    {
                        dataWriter.Write(value);
                    }
                }

                var pad = (matrix.RowCount*matrix.ColumnCount*4)%8;
                PadData(dataWriter, pad);

                data = dataMemoryStream.ToArray();
            }

            return data;
        }

        /// <summary>
        /// Gets the dense data array.
        /// </summary>
        /// <param name="matrix">The matrix to get the data from.</param>
        /// <param name="name">The name of the matrix.</param>
        /// <returns>The matrix data as an array.</returns>
        static byte[] GetDenseDataArray(Matrix<Complex> matrix, string name)
        {
            byte[] data;
            using (var dataMemoryStream = new MemoryStream())
            using (var dataWriter = new BinaryWriter(dataMemoryStream))
            {
                WriteMatrixTagAndName(dataWriter, ArrayClass.Double, true, name, matrix.RowCount, matrix.ColumnCount, 0);

                // write data
                dataWriter.Write((int) DataType.Double);
                dataWriter.Write(matrix.RowCount*matrix.ColumnCount*8);

                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    var column = matrix.Column(j);
                    foreach (var value in column)
                    {
                        dataWriter.Write(value.Real);
                    }
                }

                dataWriter.Write((int) DataType.Double);
                dataWriter.Write(matrix.RowCount*matrix.ColumnCount*8);

                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    var column = matrix.Column(j);
                    foreach (var value in column)
                    {
                        dataWriter.Write(value.Imaginary);
                    }
                }

                data = dataMemoryStream.ToArray();
            }

            return data;
        }

        /// <summary>
        /// Gets the dense data array.
        /// </summary>
        /// <param name="matrix">The matrix to get the data from.</param>
        /// <param name="name">The name of the matrix.</param>
        /// <returns>The matrix data as an array.</returns>
        static byte[] GetDenseDataArray(Matrix<Complex32> matrix, string name)
        {
            byte[] data;
            using (var dataMemoryStream = new MemoryStream())
            using (var dataWriter = new BinaryWriter(dataMemoryStream))
            {
                WriteMatrixTagAndName(dataWriter, ArrayClass.Single, true, name, matrix.RowCount, matrix.ColumnCount, 0);

                // write data
                dataWriter.Write((int) DataType.Single);
                dataWriter.Write(matrix.RowCount*matrix.ColumnCount*4);

                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    var column = matrix.Column(j);
                    foreach (var value in column)
                    {
                        dataWriter.Write(value.Real);
                    }
                }

                var pad = (matrix.RowCount*matrix.ColumnCount*4)%8;
                PadData(dataWriter, pad);

                dataWriter.Write((int) DataType.Single);
                dataWriter.Write(matrix.RowCount*matrix.ColumnCount*4);

                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    var column = matrix.Column(j);
                    foreach (var value in column)
                    {
                        dataWriter.Write(value.Real);
                    }
                }

                PadData(dataWriter, pad);

                data = dataMemoryStream.ToArray();
            }

            return data;
        }

        /// <summary>
        /// Gets the sparse data array.
        /// </summary>
        /// <param name="matrix">The matrix to get the data from.</param>
        /// <param name="name">The name of the matrix.</param>
        /// <returns>The matrix data as an array.</returns>
        static byte[] GetSparseDataArray(LinearAlgebra.Double.SparseMatrix matrix, string name)
        {
            byte[] data;
            using (var dataMemoryStream = new MemoryStream())
            using (var dataWriter = new BinaryWriter(dataMemoryStream))
            {
                var nzmax = matrix.NonZerosCount;
                WriteMatrixTagAndName(dataWriter, ArrayClass.Sparse, false, name, matrix.RowCount, matrix.ColumnCount,
                    nzmax);

                // write ir
                dataWriter.Write((int) DataType.Int32);
                dataWriter.Write(nzmax*4);

                foreach (var column in matrix.EnumerateColumns())
                {
                    foreach (var row in column.EnumerateNonZeroIndexed())
                    {
                        dataWriter.Write(row.Item1);
                    }
                }

                // add pad if needed
                if (nzmax%2 == 1)
                {
                    dataWriter.Write(0);
                }

                // write jc
                dataWriter.Write((int) DataType.Int32);
                dataWriter.Write((matrix.ColumnCount + 1)*4);
                dataWriter.Write(0);
                var count = 0;
                foreach (var column in matrix.EnumerateColumns())
                {
                    count += ((SparseVectorStorage<double>) column.Storage).ValueCount;
                    dataWriter.Write(count);
                }

                // add pad if needed
                if (matrix.ColumnCount%2 == 0)
                {
                    dataWriter.Write(0);
                }

                // write data
                dataWriter.Write((int) DataType.Double);
                dataWriter.Write(nzmax*8);

                foreach (var column in matrix.EnumerateColumns())
                {
                    foreach (var row in column.EnumerateNonZeroIndexed())
                    {
                        dataWriter.Write(row.Item2);
                    }
                }

                data = dataMemoryStream.ToArray();
            }

            return data;
        }

        /// <summary>
        /// Gets the sparse data array.
        /// </summary>
        /// <param name="matrix">The matrix to get the data from.</param>
        /// <param name="name">The name of the matrix.</param>
        /// <returns>The matrix data as an array.</returns>
        static byte[] GetSparseDataArray(LinearAlgebra.Single.SparseMatrix matrix, string name)
        {
            byte[] data;
            using (var dataMemoryStream = new MemoryStream())
            using (var dataWriter = new BinaryWriter(dataMemoryStream))
            {
                var nzmax = matrix.NonZerosCount;
                WriteMatrixTagAndName(dataWriter, ArrayClass.Sparse, false, name, matrix.RowCount, matrix.ColumnCount,
                    nzmax);

                // write ir
                dataWriter.Write((int) DataType.Int32);
                dataWriter.Write(nzmax*4);

                foreach (var column in matrix.EnumerateColumns())
                {
                    foreach (var row in column.EnumerateNonZeroIndexed())
                    {
                        dataWriter.Write(row.Item1);
                    }
                }

                // add pad if needed
                if (nzmax%2 == 1)
                {
                    dataWriter.Write(0);
                }

                // write jc
                dataWriter.Write((int) DataType.Int32);
                dataWriter.Write((matrix.ColumnCount + 1)*4);
                dataWriter.Write(0);
                var count = 0;
                foreach (var column in matrix.EnumerateColumns())
                {
                    count += ((SparseVectorStorage<float>) column.Storage).ValueCount;
                    dataWriter.Write(count);
                }

                // add pad if needed
                if (matrix.ColumnCount%2 == 0)
                {
                    dataWriter.Write(0);
                }

                // write data
                dataWriter.Write((int) DataType.Single);
                dataWriter.Write(nzmax*4);

                foreach (var column in matrix.EnumerateColumns())
                {
                    foreach (var row in column.EnumerateNonZeroIndexed())
                    {
                        dataWriter.Write(row.Item2);
                    }
                }

                var pad = (nzmax*4)%8;
                PadData(dataWriter, pad);

                data = dataMemoryStream.ToArray();
            }

            return data;
        }

        /// <summary>
        /// Gets the sparse data array.
        /// </summary>
        /// <param name="matrix">The matrix to get the data from.</param>
        /// <param name="name">The name of the matrix.</param>
        /// <returns>The matrix data as an array.</returns>
        static byte[] GetSparseDataArray(LinearAlgebra.Complex.SparseMatrix matrix, string name)
        {
            byte[] data;
            using (var dataMemoryStream = new MemoryStream())
            using (var dataWriter = new BinaryWriter(dataMemoryStream))
            {
                var nzmax = matrix.NonZerosCount;
                WriteMatrixTagAndName(dataWriter, ArrayClass.Sparse, true, name, matrix.RowCount, matrix.ColumnCount,
                    nzmax);

                // write ir
                dataWriter.Write((int) DataType.Int32);
                dataWriter.Write(nzmax*4);

                foreach (var column in matrix.EnumerateColumns())
                {
                    foreach (var row in column.EnumerateNonZeroIndexed())
                    {
                        dataWriter.Write(row.Item1);
                    }
                }

                // add pad if needed
                if (nzmax%2 == 1)
                {
                    dataWriter.Write(0);
                }

                // write jc
                dataWriter.Write((int) DataType.Int32);
                dataWriter.Write((matrix.ColumnCount + 1)*4);
                dataWriter.Write(0);
                var count = 0;
                foreach (var column in matrix.EnumerateColumns())
                {
                    count += ((SparseVectorStorage<Complex>) column.Storage).ValueCount;
                    dataWriter.Write(count);
                }

                // add pad if needed
                if (matrix.ColumnCount%2 == 0)
                {
                    dataWriter.Write(0);
                }

                // write data
                dataWriter.Write((int) DataType.Double);
                dataWriter.Write(nzmax*8);

                foreach (var column in matrix.EnumerateColumns())
                {
                    foreach (var row in column.EnumerateNonZeroIndexed())
                    {
                        dataWriter.Write(row.Item2.Real);
                    }
                }

                dataWriter.Write((int) DataType.Double);
                dataWriter.Write(nzmax*8);

                foreach (var column in matrix.EnumerateColumns())
                {
                    foreach (var row in column.EnumerateNonZeroIndexed())
                    {
                        dataWriter.Write(row.Item2.Real);
                    }
                }

                data = dataMemoryStream.ToArray();
            }

            return data;
        }

        /// <summary>
        /// Gets the sparse data array.
        /// </summary>
        /// <param name="matrix">The matrix to get the data from.</param>
        /// <param name="name">The name of the matrix.</param>
        /// <returns>The matrix data as an array.</returns>
        static byte[] GetSparseDataArray(LinearAlgebra.Complex32.SparseMatrix matrix, string name)
        {
            byte[] data;
            using (var dataMemoryStream = new MemoryStream())
            using (var dataWriter = new BinaryWriter(dataMemoryStream))
            {
                var nzmax = matrix.NonZerosCount;
                WriteMatrixTagAndName(dataWriter, ArrayClass.Sparse, true, name, matrix.RowCount, matrix.ColumnCount,
                    nzmax);

                // write ir
                dataWriter.Write((int) DataType.Int32);
                dataWriter.Write(nzmax*4);

                foreach (var column in matrix.EnumerateColumns())
                {
                    foreach (var row in column.EnumerateNonZeroIndexed())
                    {
                        dataWriter.Write(row.Item1);
                    }
                }

                // add pad if needed
                if (nzmax%2 == 1)
                {
                    dataWriter.Write(0);
                }

                // write jc
                dataWriter.Write((int) DataType.Int32);
                dataWriter.Write((matrix.ColumnCount + 1)*4);
                dataWriter.Write(0);
                var count = 0;
                foreach (var column in matrix.EnumerateColumns())
                {
                    count += ((SparseVectorStorage<Complex32>) column.Storage).ValueCount;
                    dataWriter.Write(count);
                }

                // add pad if needed
                if (matrix.ColumnCount%2 == 0)
                {
                    dataWriter.Write(0);
                }

                // write data
                dataWriter.Write((int) DataType.Single);
                dataWriter.Write(nzmax*4);

                foreach (var column in matrix.EnumerateColumns())
                {
                    foreach (var row in column.EnumerateNonZeroIndexed())
                    {
                        dataWriter.Write(row.Item2.Real);
                    }
                }

                var pad = (nzmax*4)%8;
                PadData(dataWriter, pad);

                dataWriter.Write((int) DataType.Single);
                dataWriter.Write(nzmax*4);

                foreach (var column in matrix.EnumerateColumns())
                {
                    foreach (var row in column.EnumerateNonZeroIndexed())
                    {
                        dataWriter.Write(row.Item2.Real);
                    }
                }

                PadData(dataWriter, pad);

                data = dataMemoryStream.ToArray();
            }

            return data;
        }

        /// <summary>
        /// Writes the compressed data.
        /// </summary>
        /// <param name="data">The data to write.</param>
        void WriteCompressedData(byte[] data)
        {
            // fill in data size
            var size = BitConverter.GetBytes(data.Length);
            data[4] = size[0];
            data[5] = size[1];
            data[6] = size[2];
            data[7] = size[3];

            // compress data
            var compressedData = CompressData(data);

            // write compressed data to file
            _writer.Write(compressedData.Length);
            _writer.Write(compressedData);
        }

        /// <summary>
        /// Writes the file header.
        /// </summary>
        void WriteHeader()
        {
            var header = Encoding.ASCII.GetBytes(HeaderText + DateTime.Now.ToString(Resources.MatlabDateHeaderFormat));
            _writer.Write(header);
            PadData(_writer, HeaderTextLength - header.Length + 8, 32);

            // write version
            _writer.Write((short) 0x100);

            // write little endian indicator
            _writer.Write((byte) 0x49);
            _writer.Write((byte) 0x4D);
        }

        /// <summary>
        /// Pads the data with the given byte.
        /// </summary>
        /// <param name="writer">Where to write the pad values.</param>
        /// <param name="bytes">The number of bytes to pad.</param>
        /// <param name="pad">What value to pad with.</param>
        static void PadData(BinaryWriter writer, int bytes, byte pad = (byte) 0)
        {
            for (var i = 0; i < bytes; i++)
            {
                writer.Write(pad);
            }
        }
    }
}
