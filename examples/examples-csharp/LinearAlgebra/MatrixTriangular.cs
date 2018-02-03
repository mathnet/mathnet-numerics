// <copyright file="MatrixTriangular.cs" company="Math.NET">
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
    /// Triangular matrices
    /// </summary>
    /// <seealso cref="http://reference.wolfram.com/mathematica/guide/PartsOfMatrices.html"/>
    public class MatrixTriangular : IExample
    {
        /// <summary>
        /// Gets the name of this example
        /// </summary>
        public string Name
        {
            get
            {
                return "Retrieving special forms of the matrix";
            }
        }

        /// <summary>
        /// Gets the description of this example
        /// </summary>
        public string Description
        {
            get
            {
                return "Retrieving different forms of triangular matrices from existing matrix";
            }
        }

        /// <summary>
        /// Run example
        /// </summary>
        /// <seealso cref="http://en.wikipedia.org/wiki/Triangular_matrix">Triangular matrix</seealso>
        public void Run()
        {
            // Format matrix output to console
            var formatProvider = (CultureInfo)CultureInfo.InvariantCulture.Clone();
            formatProvider.TextInfo.ListSeparator = " ";

            // Create square matrix
            var matrix = new DenseMatrix(10);
            var k = 0;
            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    matrix[i, j] = k++;
                }
            }

            Console.WriteLine(@"Initial square matrix");
            Console.WriteLine(matrix.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // 1. Retrieve a new matrix containing the lower triangle of the matrix
            var lower = matrix.LowerTriangle();

            // Puts the lower triangle of the matrix into the result matrix.
            matrix.LowerTriangle(lower);
            Console.WriteLine(@"1. Lower triangle of the matrix");
            Console.WriteLine(lower.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // 2. Retrieve a new matrix containing the upper triangle of the matrix
            var upper = matrix.UpperTriangle();

            // Puts the upper triangle of the matrix into the result matrix.
            matrix.UpperTriangle(lower);
            Console.WriteLine(@"2. Upper triangle of the matrix");
            Console.WriteLine(upper.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // 3. Retrieve a new matrix containing the strictly lower triangle of the matrix
            var strictlylower = matrix.StrictlyLowerTriangle();

            // Puts the strictly lower triangle of the matrix into the result matrix.
            matrix.StrictlyLowerTriangle(strictlylower);
            Console.WriteLine(@"3. Strictly lower triangle of the matrix");
            Console.WriteLine(strictlylower.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // 4. Retrieve a new matrix containing the strictly upper triangle of the matrix
            var strictlyupper = matrix.StrictlyUpperTriangle();

            // Puts the strictly upper triangle of the matrix into the result matrix.
            matrix.StrictlyUpperTriangle(strictlyupper);
            Console.WriteLine(@"4. Strictly upper triangle of the matrix");
            Console.WriteLine(strictlyupper.ToString("#0.00\t", formatProvider));
            Console.WriteLine();
        }
    }
}
