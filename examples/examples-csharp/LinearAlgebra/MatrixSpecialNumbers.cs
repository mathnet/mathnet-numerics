// <copyright file="MatrixSpecialNumbers.cs" company="Math.NET">
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
    /// Special numbers associated with any square matrix
    /// </summary>
    /// <seealso cref="http://reference.wolfram.com/mathematica/ref/Det.html">The determinant of the square matrix</seealso>
    /// <seealso cref="http://reference.wolfram.com/mathematica/ref/Tr.html">The trace of the matrix</seealso>
    /// <seealso cref="http://reference.wolfram.com/mathematica/ref/MatrixRank.html">The rank of the matrix</seealso>
    public class MatrixSpecialNumbers : IExample
    {
        /// <summary>
        /// Gets the name of this example
        /// </summary>
        public string Name
        {
            get
            {
                return "Special numbers associated with any square matrix";
            }
        }

        /// <summary>
        /// Gets the description of this example
        /// </summary>
        public string Description
        {
            get
            {
                return "Matrix properties as: Determinant, Condition Number, Rank and Trace";
            }
        }

        /// <summary>
        /// Run example
        /// </summary>
        /// <seealso cref="http://en.wikipedia.org/wiki/Determinant">Determinant</seealso>
        /// <seealso cref="http://en.wikipedia.org/wiki/Rank_%28linear_algebra%29">Rank (linear algebra)</seealso>
        /// <seealso cref="http://en.wikipedia.org/wiki/Trace_%28linear_algebra%29">Trace (linear algebra)</seealso>
        /// <seealso cref="http://en.wikipedia.org/wiki/Condition_number">Condition number</seealso>
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

            // 1. Determinant
            Console.WriteLine(@"1. Determinant");
            Console.WriteLine(matrix.Determinant());
            Console.WriteLine();

            // 2. Rank
            Console.WriteLine(@"2. Rank");
            Console.WriteLine(matrix.Rank());
            Console.WriteLine();

            // 3. Condition number
            Console.WriteLine(@"2. Condition number");
            Console.WriteLine(matrix.ConditionNumber());
            Console.WriteLine();

            // 4. Trace
            Console.WriteLine(@"4. Trace");
            Console.WriteLine(matrix.Trace());
            Console.WriteLine();
        }
    }
}
