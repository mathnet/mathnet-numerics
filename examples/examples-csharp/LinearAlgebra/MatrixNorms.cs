// <copyright file="MatrixNorms.cs" company="Math.NET">
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
    /// Matrix norms 
    /// </summary>
    public class MatrixNorms : IExample
    {
        /// <summary>
        /// Gets the name of this example
        /// </summary>
        public string Name
        {
            get
            {
                return "Matrix norms";
            }
        }

        /// <summary>
        /// Gets the description of this example
        /// </summary>
        public string Description
        {
            get
            {
                return "Examples of matrix norms: L1 norm, L2 norm, Frobenius norm and infinity norm";
            }
        }

        /// <summary>
        /// Run example
        /// </summary>
        /// <seealso cref="http://en.wikipedia.org/wiki/Matrix_norm">Matrix norm</seealso>
        public void Run()
        {
            // Format matrix output to console
            var formatProvider = (CultureInfo)CultureInfo.InvariantCulture.Clone();
            formatProvider.TextInfo.ListSeparator = " ";

            // Create square matrix
            var matrix = DenseMatrix.OfArray(new[,] { { 1.0, 2.0, 3.0 }, { 6.0, 5.0, 4.0 }, { 8.0, 9.0, 7.0 } });
            Console.WriteLine(@"Initial square matrix");
            Console.WriteLine(matrix.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // 1. 1-norm of the matrix
            Console.WriteLine(@"1. 1-norm of the matrix");
            Console.WriteLine(matrix.L1Norm());
            Console.WriteLine();

            // 2. 2-norm of the matrix
            Console.WriteLine(@"2. 2-norm of the matrix");
            Console.WriteLine(matrix.L2Norm());
            Console.WriteLine();

            // 3. Frobenius norm of the matrix
            Console.WriteLine(@"3. Frobenius norm of the matrix");
            Console.WriteLine(matrix.FrobeniusNorm());
            Console.WriteLine();

            // 4. Infinity norm of the matrix
            Console.WriteLine(@"4. Infinity norm of the matrix");
            Console.WriteLine(matrix.InfinityNorm());
            Console.WriteLine();

            // 5. Normalize matrix columns
            Console.WriteLine(@"5. Normalize matrix columns: before normalize");
            foreach (var keyValuePair in matrix.EnumerateColumnsIndexed())
            {
                Console.WriteLine(@"Column {0} 2-nd norm is: {1}", keyValuePair.Item1, keyValuePair.Item2.L2Norm());
            }

            Console.WriteLine();
            var normalized = matrix.NormalizeColumns(2);
            Console.WriteLine(@"5. Normalize matrix columns: after normalize");
            foreach (var keyValuePair in normalized.EnumerateColumnsIndexed())
            {
                Console.WriteLine(@"Column {0} 2-nd norm is: {1}", keyValuePair.Item1, keyValuePair.Item2.L2Norm());
            }

            Console.WriteLine();

            // 6. Normalize matrix columns
            Console.WriteLine(@"6. Normalize matrix rows: before normalize");
            foreach (var keyValuePair in matrix.EnumerateRowsIndexed())
            {
                Console.WriteLine(@"Row {0} 2-nd norm is: {1}", keyValuePair.Item1, keyValuePair.Item2.L2Norm());
            }

            Console.WriteLine();
            normalized = matrix.NormalizeRows(2);
            Console.WriteLine(@"6. Normalize matrix rows: after normalize");
            foreach (var keyValuePair in normalized.EnumerateRowsIndexed())
            {
                Console.WriteLine(@"Row {0} 2-nd norm is: {1}", keyValuePair.Item1, keyValuePair.Item2.L2Norm());
            }
        }
    }
}
