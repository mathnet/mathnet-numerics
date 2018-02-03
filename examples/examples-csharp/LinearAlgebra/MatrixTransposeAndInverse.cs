// <copyright file="MatrixTransposeAndInverse.cs" company="Math.NET">
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
    /// Matrix transpose and inverse
    /// </summary>
    /// <seealso cref="http://reference.wolfram.com/mathematica/ref/Transpose.html">Transpose</seealso>
    /// <seealso cref="http://reference.wolfram.com/mathematica/ref/Inverse.html">Inverse</seealso>
    public class MatrixTransposeAndInverse : IExample
    {
        /// <summary>
        /// Gets the name of this example
        /// </summary>
        public string Name
        {
            get
            {
                return "Matrix transpose and inverse";
            }
        }

        /// <summary>
        /// Gets the description of this example
        /// </summary>
        public string Description
        {
            get
            {
                return "Transpose matrix, inverse matrix, transpose-and-multiply matrix examples";
            }
        }

        /// <summary>
        /// Run example
        /// </summary>
        /// <seealso cref="http://en.wikipedia.org/wiki/Transpose">Transpose</seealso>
        /// <seealso cref="http://en.wikipedia.org/wiki/Invertible_matrix">Invertible matrix</seealso>
        public void Run()
        {
            // Format matrix output to console
            var formatProvider = (CultureInfo)CultureInfo.InvariantCulture.Clone();
            formatProvider.TextInfo.ListSeparator = " ";

            // Create random square matrix
            var matrix = new DenseMatrix(5);
            var rnd = new Random(1);
            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    matrix[i, j] = rnd.NextDouble();
                }
            }

            Console.WriteLine(@"Initial matrix");
            Console.WriteLine(matrix.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // 1. Get matrix inverse
            var inverse = matrix.Inverse();
            Console.WriteLine(@"1. Matrix inverse");
            Console.WriteLine(inverse.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // 2. Matrix multiplied by its inverse gives identity matrix
            var identity = matrix * inverse;
            Console.WriteLine(@"2. Matrix multiplied by its inverse");
            Console.WriteLine(identity.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // 3. Get matrix transpose
            var transpose = matrix.Transpose();
            Console.WriteLine(@"3. Matrix transpose");
            Console.WriteLine(transpose.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // 4. Get orthogonal  matrix, i.e. do QR decomposition and get matrix Q
            var orthogonal = matrix.QR().Q;
            Console.WriteLine(@"4. Orthogonal  matrix");
            Console.WriteLine(orthogonal.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // 5. Transpose and multiply orthogonal matrix by iteslf gives identity matrix
            identity = orthogonal.TransposeAndMultiply(orthogonal);
            Console.WriteLine(@"Transpose and multiply orthogonal matrix by iteslf");
            Console.WriteLine(identity.ToString("#0.00\t", formatProvider));
            Console.WriteLine();
        }
    }
}
