// <copyright file="MatrixDataAccessor.cs" company="Math.NET">
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

using System;
using System.Globalization;
using MathNet.Numerics.LinearAlgebra.Double;

namespace Examples.LinearAlgebraExamples
{
    /// <summary>
    /// Matrix data access, copying and conversion examples
    /// </summary>
    public class MatrixDataAccessor
    {
        /// <summary>
        /// Gets the name of this example
        /// </summary>
        public string Name
        {
            get
            {
                return "Matrix data access, copying and conversion";
            }
        }

        /// <summary>
        /// Gets the description of this example
        /// </summary>
        public string Description
        {
            get
            {
                return "Examples of setting/getting values of a matrix, copying and conversion matrix into another matrix";
            }
        }

        /// <summary>
        /// Run example
        /// </summary>
        public void Run()
        {
            // Format vector output to console
            var formatProvider = (CultureInfo)CultureInfo.InvariantCulture.Clone();
            formatProvider.TextInfo.ListSeparator = " ";

            // Create new empty square matrix
            var matrix = new DenseMatrix(10);
            Console.WriteLine(@"Empty matrix");
            Console.WriteLine(matrix.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // 1. Fill matrix by data using indexer []
            var k = 0;
            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    matrix[i, j] = k++;
                }
            }

            Console.WriteLine(@"1. Fill matrix by data using indexer []");
            Console.WriteLine(matrix.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // 2. Fill matrix by data using At. The element is set without range checking.
            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    matrix.At(i, j, k--);
                }
            }

            Console.WriteLine(@"2. Fill matrix by data using At");
            Console.WriteLine(matrix.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // 3. Clone matrix
            var clone = matrix.Clone();
            Console.WriteLine(@"3. Clone matrix");
            Console.WriteLine(clone.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // 4. Clear matrix
            clone.Clear();
            Console.WriteLine(@"4. Clear matrix");
            Console.WriteLine(clone.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // 5. Copy matrix into another matrix
            matrix.CopyTo(clone);
            Console.WriteLine(@"5. Copy matrix into another matrix");
            Console.WriteLine(clone.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // 6. Get submatrix into another matrix
            var submatrix = matrix.SubMatrix(2, 2, 3, 3);
            Console.WriteLine(@"6. Copy submatrix into another matrix");
            Console.WriteLine(submatrix.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // 7. Get part of the row as vector. In this example: get 4 elements from row 5 starting from column 3
            var row = matrix.Row(5, 3, 4);
            Console.WriteLine(@"7. Get part of the row as vector");
            Console.WriteLine(row.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // 8. Get part of the column as vector. In this example: get 3 elements from column 2 starting from row 6
            var column = matrix.Column(2, 6, 3);
            Console.WriteLine(@"8. Get part of the column as vector");
            Console.WriteLine(column.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // 9. Get columns using column enumerator. If you need all columns you may use ColumnEnumerator without parameters
            Console.WriteLine(@"9. Get columns using column enumerator");
            foreach (var keyValuePair in matrix.EnumerateColumnsIndexed(2, 4))
            {
                Console.WriteLine(@"Column {0}: {1}", keyValuePair.Item1, keyValuePair.Item2.ToString("#0.00\t", formatProvider));
            }

            Console.WriteLine();

            // 10. Get rows using row enumerator. If you need all rows you may use RowEnumerator without parameters
            Console.WriteLine(@"10. Get rows using row enumerator");
            foreach (var keyValuePair in matrix.EnumerateRowsIndexed(4, 3))
            {
                Console.WriteLine(@"Row {0}: {1}", keyValuePair.Item1, keyValuePair.Item2.ToString("#0.00\t", formatProvider));
            }

            Console.WriteLine();

            // 11. Convert matrix into multidimensional array
            var data = matrix.ToArray();
            Console.WriteLine(@"11. Convert matrix into multidimensional array");
            for (var i = 0; i < data.GetLongLength(0); i++)
            {
                for (var j = 0; j < data.GetLongLength(1); j++)
                {
                    Console.Write(data[i, j].ToString("#0.00\t"));
                }

                Console.WriteLine();
            }

            Console.WriteLine();

            // 12. Convert matrix into row-wise array
            var rowwise = matrix.ToRowWiseArray();
            Console.WriteLine(@"12. Convert matrix into row-wise array");
            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    Console.Write(rowwise[(i * matrix.ColumnCount) + j].ToString("#0.00\t"));
                }

                Console.WriteLine();
            }

            Console.WriteLine();

            // 13. Convert matrix into column-wise array
            var columnise = matrix.ToColumnWiseArray();
            Console.WriteLine(@"13. Convert matrix into column-wise array");
            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    Console.Write(columnise[(j * matrix.RowCount) + i].ToString("#0.00\t"));
                }

                Console.WriteLine();
            }

            Console.WriteLine();

            // 14. Get matrix diagonal as vector
            var diagonal = matrix.Diagonal();
            Console.WriteLine(@"14. Get matrix diagonal as vector");
            Console.WriteLine(diagonal.ToString("#0.00\t", formatProvider));
            Console.WriteLine();
        }
    }
}
