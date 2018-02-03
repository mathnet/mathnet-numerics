// <copyright file="LU.cs" company="Math.NET">
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

namespace Examples.LinearAlgebra.FactorizationExamples
{
    /// <summary>
    /// LU factorization example. For a matrix A, the LU factorization is a pair of lower triangular matrix L and
    /// upper triangular matrix U so that A = L*U.
    /// In the Math.Net implementation we also store a set of pivot elements for increased 
    /// numerical stability. The pivot elements encode a permutation matrix P such that P*A = L*U
    /// </summary>
    /// <seealso cref="http://reference.wolfram.com/mathematica/ref/LUDecomposition.html"/>
    public class LU : IExample
    {
        /// <summary>
        /// Gets the name of this example
        /// </summary>
        public string Name
        {
            get
            {
                return "LU factorization";
            }
        }

        /// <summary>
        /// Gets the description of this example
        /// </summary>
        public string Description
        {
            get
            {
                return "Perform the LU factorization to the appropriate class";
            }
        }

        /// <summary>
        /// Run example
        /// </summary>
        /// <seealso cref="http://en.wikipedia.org/wiki/LU_decomposition">LU decomposition</seealso>
        /// <seealso cref="http://en.wikipedia.org/wiki/Invertible_matrix">Invertible matrix</seealso>
        public void Run()
        {
            // Format matrix output to console
            var formatProvider = (CultureInfo)CultureInfo.InvariantCulture.Clone();
            formatProvider.TextInfo.ListSeparator = " ";

            // Create square matrix
            var matrix = DenseMatrix.OfArray(new[,] { { 1.0, 2.0 }, { 3.0, 4.0 } });
            Console.WriteLine(@"Initial square matrix");
            Console.WriteLine(matrix.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // Perform LU decomposition
            var lu = matrix.LU();
            Console.WriteLine(@"Perform LU decomposition");

            // 1. Lower triangular factor
            Console.WriteLine(@"1. Lower triangular factor");
            Console.WriteLine(lu.L.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // 2. Upper triangular factor
            Console.WriteLine(@"2. Upper triangular factor");
            Console.WriteLine(lu.U.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // 3. Permutations applied to LU factorization
            Console.WriteLine(@"3. Permutations applied to LU factorization");
            for (var i = 0; i < lu.P.Dimension; i++)
            {
                if (lu.P[i] > i)
                {
                    Console.WriteLine(@"Row {0} permuted with row {1}", lu.P[i], i);
                }
            }

            Console.WriteLine();

            // 4. Reconstruct initial matrix: PA = L * U
            var reconstruct = lu.L * lu.U;

            // The rows of the reconstructed matrix should be permuted to get the initial matrix
            reconstruct.PermuteRows(lu.P.Inverse());
            Console.WriteLine(@"4. Reconstruct initial matrix: PA = L*U");
            Console.WriteLine(reconstruct.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // 5. Get the determinant of the matrix
            Console.WriteLine(@"5. Determinant of the matrix");
            Console.WriteLine(lu.Determinant);
            Console.WriteLine();

            // 6. Get the inverse of the matrix
            var matrixInverse = lu.Inverse();
            Console.WriteLine(@"6. Inverse of the matrix");
            Console.WriteLine(matrixInverse.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // 7. Matrix multiplied by its inverse 
            var identity = matrix * matrixInverse;
            Console.WriteLine(@"7. Matrix multiplied by its inverse ");
            Console.WriteLine(identity.ToString("#0.00\t", formatProvider));
            Console.WriteLine();
        }
    }
}
