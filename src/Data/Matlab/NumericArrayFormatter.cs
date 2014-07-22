// <copyright file="NumericArrayFormatter.cs" company="Math.NET">
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
using MathNet.Numerics.LinearAlgebra;

namespace MathNet.Numerics.Data.Matlab
{
    internal static class NumericArrayFormatter
    {
        internal static void Write(BinaryWriter writer, Matrix<double> matrix)
        {
            // write data
            writer.Write((int)DataType.Double);
            writer.Write(matrix.RowCount*matrix.ColumnCount*8);

            for (var j = 0; j < matrix.ColumnCount; j++)
            {
                var column = matrix.Column(j);
                foreach (var value in column)
                {
                    writer.Write(value);
                }
            }
        }

        internal static void Write(BinaryWriter writer, Matrix<float> matrix)
        {
            // write data
            int size = matrix.RowCount*matrix.ColumnCount*4;
            writer.Write((int)DataType.Single);
            writer.Write(size);

            for (var j = 0; j < matrix.ColumnCount; j++)
            {
                var column = matrix.Column(j);
                foreach (var value in column)
                {
                    writer.Write(value);
                }
            }

            PadData(writer, size%8);
        }

        internal static void Write(BinaryWriter writer, Matrix<Complex> matrix)
        {
            // write data
            int size = matrix.RowCount*matrix.ColumnCount*8;
            writer.Write((int)DataType.Double);
            writer.Write(size);

            for (var j = 0; j < matrix.ColumnCount; j++)
            {
                var column = matrix.Column(j);
                foreach (var value in column)
                {
                    writer.Write(value.Real);
                }
            }

            writer.Write((int)DataType.Double);
            writer.Write(size);

            for (var j = 0; j < matrix.ColumnCount; j++)
            {
                var column = matrix.Column(j);
                foreach (var value in column)
                {
                    writer.Write(value.Imaginary);
                }
            }
        }

        internal static void Write(BinaryWriter writer, Matrix<Complex32> matrix)
        {
            // write data
            int size = matrix.RowCount*matrix.ColumnCount*4;
            writer.Write((int)DataType.Single);
            writer.Write(size);

            for (var j = 0; j < matrix.ColumnCount; j++)
            {
                var column = matrix.Column(j);
                foreach (var value in column)
                {
                    writer.Write(value.Real);
                }
            }

            PadData(writer, size%8);

            writer.Write((int)DataType.Single);
            writer.Write(size);

            for (var j = 0; j < matrix.ColumnCount; j++)
            {
                var column = matrix.Column(j);
                foreach (var value in column)
                {
                    writer.Write(value.Real);
                }
            }

            PadData(writer, size%8);
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
