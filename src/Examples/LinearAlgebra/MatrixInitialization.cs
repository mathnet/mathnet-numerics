// <copyright file="MatrixInitialization.cs" company="Math.NET">
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
    /// Matrix initialization examples
    /// </summary>
    public class MatrixInitialization : IExample
    {
        /// <summary>
        /// Gets the name of this example
        /// </summary>
        public string Name
        {
            get
            {
                return "Matrix initialization";
            }
        }

        /// <summary>
        /// Gets the description of this example
        /// </summary>
        public string Description
        {
            get
            {
                return "Examples of creating matrix instances";
            }
        }

        /// <summary>
        /// Run example
        /// </summary>
        public void Run()
        {
            // 1. Initialize a new instance of the matrix from a 2D array. This constructor will allocate a completely new memory block for storing the dense matrix.
            var matrix1 = DenseMatrix.OfArray(new[,] { { 1.0, 2.0, 3.0 }, { 4.0, 5.0, 6.0 } });

            // 2. Initialize a new instance of the empty square matrix with a given order.
            var matrix2 = new DenseMatrix(3);

            // 3. Initialize a new instance of the empty matrix with a given size.
            var matrix3 = new DenseMatrix(2, 3);

            // 4. Initialize a new instance of the matrix with all entries set to a particular value.
            var matrix4 = DenseMatrix.Create(2, 3, (i, j) => 3.0);

            // 4. Initialize a new instance of the matrix from a one dimensional array. This array should store the matrix in column-major order. 
            var matrix5 = new DenseMatrix(2, 3, new[] { 1.0, 4.0, 2.0, 5.0, 3.0, 6.0 });

            // 5. Initialize a square matrix with all zero's except for ones on the diagonal. Identity matrix (http://en.wikipedia.org/wiki/Identity_matrix).
            var matrixI = DenseMatrix.CreateIdentity(5);

            // Format matrix output to console
            var formatProvider = (CultureInfo)CultureInfo.InvariantCulture.Clone();
            formatProvider.TextInfo.ListSeparator = " ";

            Console.WriteLine(@"Matrix 1");
            Console.WriteLine(matrix1.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            Console.WriteLine(@"Matrix 2");
            Console.WriteLine(matrix2.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            Console.WriteLine(@"Matrix 3");
            Console.WriteLine(matrix3.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            Console.WriteLine(@"Matrix 4");
            Console.WriteLine(matrix4.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            Console.WriteLine(@"Matrix 5");
            Console.WriteLine(matrix5.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            Console.WriteLine(@"Identity matrix");
            Console.WriteLine(matrixI.ToString("#0.00\t", formatProvider));
            Console.WriteLine();
        }
    }
}
