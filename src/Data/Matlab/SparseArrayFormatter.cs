// <copyright file="SparseArrayFormatter.cs" company="Math.NET">
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

using System.IO;
using System.Numerics;
using MathNet.Numerics.LinearAlgebra.Storage;

namespace MathNet.Numerics.Data.Matlab
{
    internal static class SparseArrayFormatter
    {
        //public static void FormatSparseMatrix<T>(BinaryWriter writer, Matrix<T> matrix, string name, bool isComplex, int rows, int columns, int size)
        //    where T : struct, IEquatable<T>, IFormattable
        //{
        //    var transposed = matrix.Transpose();
        //    var storage = (SparseCompressedRowMatrixStorage<T>)transposed.Storage;

        //    WriteMatrixTagAndName(writer, ArrayClass.Sparse, isComplex, name, storage);
        //}


        internal static void Write(BinaryWriter writer, LinearAlgebra.Double.SparseMatrix matrix)
        {
            var nzmax = matrix.NonZerosCount;

            // write ir
            writer.Write((int)DataType.Int32);
            writer.Write(nzmax*4);

            foreach (var column in matrix.EnumerateColumns())
            {
                foreach (var row in column.EnumerateNonZeroIndexed())
                {
                    writer.Write(row.Item1);
                }
            }

            // add pad if needed
            if (nzmax%2 == 1)
            {
                writer.Write(0);
            }

            // write jc
            writer.Write((int)DataType.Int32);
            writer.Write((matrix.ColumnCount + 1)*4);
            writer.Write(0);
            var count = 0;
            foreach (var column in matrix.EnumerateColumns())
            {
                count += ((SparseVectorStorage<double>)column.Storage).ValueCount;
                writer.Write(count);
            }

            // add pad if needed
            if (matrix.ColumnCount%2 == 0)
            {
                writer.Write(0);
            }

            // write data
            writer.Write((int)DataType.Double);
            writer.Write(nzmax*8);

            foreach (var column in matrix.EnumerateColumns())
            {
                foreach (var row in column.EnumerateNonZeroIndexed())
                {
                    writer.Write(row.Item2);
                }
            }
        }

        internal static void Write(BinaryWriter writer, LinearAlgebra.Single.SparseMatrix matrix)
        {
            var nzmax = matrix.NonZerosCount;

            // write ir
            writer.Write((int)DataType.Int32);
            writer.Write(nzmax*4);

            foreach (var column in matrix.EnumerateColumns())
            {
                foreach (var row in column.EnumerateNonZeroIndexed())
                {
                    writer.Write(row.Item1);
                }
            }

            // add pad if needed
            if (nzmax%2 == 1)
            {
                writer.Write(0);
            }

            // write jc
            writer.Write((int)DataType.Int32);
            writer.Write((matrix.ColumnCount + 1)*4);
            writer.Write(0);
            var count = 0;
            foreach (var column in matrix.EnumerateColumns())
            {
                count += ((SparseVectorStorage<float>)column.Storage).ValueCount;
                writer.Write(count);
            }

            // add pad if needed
            if (matrix.ColumnCount%2 == 0)
            {
                writer.Write(0);
            }

            // write data
            writer.Write((int)DataType.Single);
            writer.Write(nzmax*4);

            foreach (var column in matrix.EnumerateColumns())
            {
                foreach (var row in column.EnumerateNonZeroIndexed())
                {
                    writer.Write(row.Item2);
                }
            }

            var pad = nzmax*4%8;
            PadData(writer, pad);
        }

        internal static void Write(BinaryWriter writer, LinearAlgebra.Complex.SparseMatrix matrix)
        {
            var nzmax = matrix.NonZerosCount;

            // write ir
            writer.Write((int)DataType.Int32);
            writer.Write(nzmax*4);

            foreach (var column in matrix.EnumerateColumns())
            {
                foreach (var row in column.EnumerateNonZeroIndexed())
                {
                    writer.Write(row.Item1);
                }
            }

            // add pad if needed
            if (nzmax%2 == 1)
            {
                writer.Write(0);
            }

            // write jc
            writer.Write((int)DataType.Int32);
            writer.Write((matrix.ColumnCount + 1)*4);
            writer.Write(0);
            var count = 0;
            foreach (var column in matrix.EnumerateColumns())
            {
                count += ((SparseVectorStorage<Complex>)column.Storage).ValueCount;
                writer.Write(count);
            }

            // add pad if needed
            if (matrix.ColumnCount%2 == 0)
            {
                writer.Write(0);
            }

            // write data
            writer.Write((int)DataType.Double);
            writer.Write(nzmax*8);

            foreach (var column in matrix.EnumerateColumns())
            {
                foreach (var row in column.EnumerateNonZeroIndexed())
                {
                    writer.Write(row.Item2.Real);
                }
            }

            writer.Write((int)DataType.Double);
            writer.Write(nzmax*8);

            foreach (var column in matrix.EnumerateColumns())
            {
                foreach (var row in column.EnumerateNonZeroIndexed())
                {
                    writer.Write(row.Item2.Imaginary);
                }
            }
        }

        internal static void Write(BinaryWriter writer, LinearAlgebra.Complex32.SparseMatrix matrix)
        {
            var nzmax = matrix.NonZerosCount;

            // write ir
            writer.Write((int)DataType.Int32);
            writer.Write(nzmax*4);

            foreach (var column in matrix.EnumerateColumns())
            {
                foreach (var row in column.EnumerateNonZeroIndexed())
                {
                    writer.Write(row.Item1);
                }
            }

            // add pad if needed
            if (nzmax%2 == 1)
            {
                writer.Write(0);
            }

            // write jc
            writer.Write((int)DataType.Int32);
            writer.Write((matrix.ColumnCount + 1)*4);
            writer.Write(0);
            var count = 0;
            foreach (var column in matrix.EnumerateColumns())
            {
                count += ((SparseVectorStorage<Complex32>)column.Storage).ValueCount;
                writer.Write(count);
            }

            // add pad if needed
            if (matrix.ColumnCount%2 == 0)
            {
                writer.Write(0);
            }

            // write data
            writer.Write((int)DataType.Single);
            writer.Write(nzmax*4);

            foreach (var column in matrix.EnumerateColumns())
            {
                foreach (var row in column.EnumerateNonZeroIndexed())
                {
                    writer.Write(row.Item2.Real);
                }
            }

            var pad = nzmax*4%8;
            PadData(writer, pad);

            writer.Write((int)DataType.Single);
            writer.Write(nzmax*4);

            foreach (var column in matrix.EnumerateColumns())
            {
                foreach (var row in column.EnumerateNonZeroIndexed())
                {
                    writer.Write(row.Item2.Imaginary);
                }
            }

            PadData(writer, pad);
        }

        /// <summary>
        /// Pads the data with the given byte.
        /// </summary>
        /// <param name="writer">Where to write the pad values.</param>
        /// <param name="bytes">The number of bytes to pad.</param>
        /// <param name="pad">What value to pad with.</param>
        static void PadData(BinaryWriter writer, int bytes, byte pad = (byte)0)
        {
            for (var i = 0; i < bytes; i++)
            {
                writer.Write(pad);
            }
        }
    }
}
