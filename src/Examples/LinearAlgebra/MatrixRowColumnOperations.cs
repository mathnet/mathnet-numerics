// <copyright file="MatrixRowColumnOperations.cs" company="Math.NET">
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
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra.Double;

namespace Examples.LinearAlgebraExamples
{
    /// <summary>
    /// Matrix operations with rows and columns
    /// </summary>
    public class MatrixRowColumnOperations : IExample
    {
        /// <summary>
        /// Gets the name of this example
        /// </summary>
        public string Name
        {
            get
            {
                return "Matrix row and column operations";
            }
        }

        /// <summary>
        /// Gets the description of this example
        /// </summary>
        public string Description
        {
            get
            {
                return "Examples of permuting, modifying, inserting columns and rows in a matrix";
            }
        }

        /// <summary>
        /// Run example
        /// </summary>
        public void Run()
        {
            // Format matrix output to console
            var formatProvider = (CultureInfo)CultureInfo.InvariantCulture.Clone();
            formatProvider.TextInfo.ListSeparator = " ";
            
            // Create square matrix
            var matrix = new DenseMatrix(5);
            var k = 0;
            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    matrix[i, j] = k++;
                }
            }

            Console.WriteLine(@"Initial matrix");
            Console.WriteLine(matrix.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // Create vector
            var vector = new DenseVector(new[] { 50.0, 51.0, 52.0, 53.0, 54.0 });
            Console.WriteLine(@"Sample vector");
            Console.WriteLine(vector.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // 1. Insert new column
            var result = matrix.InsertColumn(3, vector);
            Console.WriteLine(@"1. Insert new column");
            Console.WriteLine(result.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // 2. Insert new row
            result = matrix.InsertRow(3, vector);
            Console.WriteLine(@"2. Insert new row");
            Console.WriteLine(result.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // 3. Set column values
            matrix.SetColumn(2, vector);
            Console.WriteLine(@"3. Set column values");
            Console.WriteLine(matrix.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // 4. Set row values. 
            matrix.SetRow(3, (double[])vector);
            Console.WriteLine(@"4. Set row values");
            Console.WriteLine(matrix.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // 5. Set diagonal values. SetRow/SetColumn/SetDiagonal accepts Vector and double[] as input parameter
            matrix.SetDiagonal(new[] { 5.0, 4.0, 3.0, 2.0, 1.0 });
            Console.WriteLine(@"5. Set diagonal values");
            Console.WriteLine(matrix.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // 6. Set submatrix values
            matrix.SetSubMatrix(1, 3, 1, 3, DenseMatrix.CreateIdentity(3));
            Console.WriteLine(@"6. Set submatrix values");
            Console.WriteLine(matrix.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // Permutations. 
            // Initialize a new instance of the Permutation class. An array represents where each integer is permuted too: 
            // indices[i] represents that integer "i" is permuted to location indices[i]
            var permutations = new Permutation(new[] { 0, 1, 3, 2, 4 });
            
            // 7. Permute rows 3 and 4
            matrix.PermuteRows(permutations);
            Console.WriteLine(@"7. Permute rows 3 and 4");
            Console.WriteLine(matrix.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // 8. Permute columns 1 and 2, 3 and 5
            permutations = new Permutation(new[] { 1, 0, 4, 3, 2 });
            matrix.PermuteColumns(permutations);
            Console.WriteLine(@"8. Permute columns 1 and 2, 3 and 5");
            Console.WriteLine(matrix.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // 9. Concatenate the matrix with the given matrix
            var append = matrix.Append(matrix);

            // Concatenate into result matrix
            matrix.Append(matrix, append);
            Console.WriteLine(@"9. Append matrix to matrix");
            Console.WriteLine(append.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

             // 10. Stack the matrix on top of the given matrix matrix
            var stack = matrix.Stack(matrix);

            // Stack into result matrix
            matrix.Stack(matrix, stack);
            Console.WriteLine(@"10. Stack the matrix on top of the given matrix matrix");
            Console.WriteLine(stack.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // 11. Diagonally stack the matrix on top of the given matrix matrix
            var diagoinalStack = matrix.DiagonalStack(matrix);

            // Diagonally stack into result matrix
            matrix.DiagonalStack(matrix, diagoinalStack);
            Console.WriteLine(@"11. Diagonally stack the matrix on top of the given matrix matrix");
            Console.WriteLine(diagoinalStack.ToString("#0.00\t", formatProvider));
            Console.WriteLine();
        }
    }
}
