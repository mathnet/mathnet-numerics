// <copyright file="AssertHelpers.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2016 Math.NET
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
using System.Globalization;
using System.IO;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace MathNet.Numerics.Tests.LinearAlgebraTests
{
    using Complex64 = System.Numerics.Complex;

    /// <summary>
    /// Matrix utility functions to simplify tests.
    /// </summary>
    static public class MatrixHelpers
    {
        /// <summary>
        /// Forces a matrix elements to symmetric. Copies the lower triangle to the upper triangle.
        /// </summary>
        /// <typeparam name="T">The matrix type.</typeparam>
        /// <param name="matrix">The matrix to make symmetric.</param>
        static public void ForceSymmetric<T>(Matrix<T> matrix) where T : struct, IEquatable<T>, IFormattable
        {
            if (matrix.RowCount != matrix.ColumnCount)
            {
                throw new ArgumentException("matrix must be square.", nameof(matrix));
            }
            for (var row = 0; row < matrix.RowCount; row++)
            {
                for (var column = 0; column < row; column++)
                {
                    matrix.At(column, row, matrix.At(row, column));
                }
            }
        }

        /// <summary>
        /// Forces a matrix elements to hermitian (conjugate symmetric). Copies the conjugate of the values
        /// from the lower triangle to the upper triangle.
        /// </summary>
        /// <param name="matrix">The matrix to make conjugate symmetric.</param>
        static public void ForceHermitian(Matrix<Complex64> matrix)
        {
            if (matrix.RowCount != matrix.ColumnCount)
            {
                throw new ArgumentException("matrix must be square.", nameof(matrix));
            }
            for (var row = 0; row < matrix.RowCount; row++)
            {
                for (var column = 0; column < row; column++)
                {
                    matrix.At(column, row, matrix.At(row, column).Conjugate());
                }
            }
        }

        /// <summary>
        /// Forces a matrix elements to conjugate symmetric. Copies the conjugate of the values
        /// from the lower triangle to the upper triangle.
        /// </summary>
        /// <param name="matrix">The matrix to make conjugate symmetric.</param>
        public static void ForceHermitian(Matrix<Numerics.Complex32> matrix)
        {
            if (matrix.RowCount != matrix.ColumnCount)
            {
                throw new ArgumentException("matrix must be square.", nameof(matrix));
            }
            for (var row = 0; row < matrix.RowCount; row++)
            {
                for (var column = 0; column < row; column++)
                {
                    matrix.At(column, row, matrix.At(row, column).Conjugate());
                }
            }
        }

        public static Matrix<double> ReadTestDataSparseMatrixDoubleCoordinateFormat(string name)
        {
            var stream = MathNet.Numerics.TestData.Data.ReadStream(name);
            if (stream == null)
                throw new FileNotFoundException($"Could not find test data file '{name}'");
            var reader = new StreamReader(stream);

            var first = reader.ReadLine()?.Trim();
            if (first == null)
                throw new InvalidDataException("Could not read first line of data file");
            if (!int.TryParse(first, out var nnz))
                throw new InvalidDataException("Could not parse first line of file (expected integer count of coordinate tuples)");

            var cooRows = new int[nnz];
            var cooCols = new int[nnz];
            var cooVals = new double[nnz];
            var nRows = 0;
            var nCols = 0;

            for (var i = 0; i < nnz; i++)
            {
                var line = reader.ReadLine()?.Trim();
                if (line == null)
                    throw new InvalidDataException($"File ended unexpectedly on line {i+1}. Expecting {nnz} coordinate tuples.");

                var split = line.Split(',');
                if (split.Length != 3)
                    throw new InvalidDataException($"Invalid sized tuple on line {i + 1} (expected 3 but detected {split.Length})");

                if (!int.TryParse(split[0], out cooRows[i]))
                    throw new InvalidDataException($"Could not parse row integer on line {i + 1}");

                if (!int.TryParse(split[1], out cooCols[i]))
                    throw new InvalidDataException($"Could not parse column integer on line {i + 1}");

                if (!double.TryParse(split[2], NumberStyles.Float, CultureInfo.InvariantCulture, out cooVals[i]))
                    throw new InvalidDataException($"Could not parse double value on line {i + 1}");

                nRows = Math.Max(nRows, cooRows[i] + 1);
                nCols = Math.Max(nCols, cooCols[i] + 1);
            }

            return SparseMatrix.Build.SparseFromCoordinateFormat(nRows, nCols, nnz, cooRows, cooCols, cooVals);
        }
    }
}
