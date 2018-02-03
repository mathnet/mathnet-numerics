// <copyright file="Cholesky.cs" company="Math.NET">
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
    /// Cholesky factorization example. For a symmetric, positive definite matrix A, the Cholesky factorization
    /// is an lower triangular matrix L so that A = L*L'
    /// </summary>
    /// <seealso cref="http://reference.wolfram.com/mathematica/ref/CholeskyDecomposition.html"/>
    public class Cholesky : IExample
    {
        /// <summary>
        /// Gets the name of this example
        /// </summary>
        public string Name
        {
            get
            {
                return "Cholesky factorization";
            }
        }

        /// <summary>
        /// Gets the description of this example
        /// </summary>
        public string Description
        {
            get
            {
                return "Perform the Cholesky factorization to the appropriate class";
            }
        }

        /// <summary>
        /// Run example
        /// </summary>
        /// <seealso cref="http://en.wikipedia.org/wiki/Cholesky_decomposition">Cholesky decomposition</seealso>
        public void Run()
        {
            // Format matrix output to console
            var formatProvider = (CultureInfo)CultureInfo.InvariantCulture.Clone();
            formatProvider.TextInfo.ListSeparator = " ";

            // Create square, symmetric, positive definite matrix
            var matrix = DenseMatrix.OfArray(new[,] { { 2.0, 1.0 }, { 1.0, 2.0 } });
            Console.WriteLine(@"Initial square, symmetric, positive definite matrix");
            Console.WriteLine(matrix.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // Perform Cholesky decomposition
            var cholesky = matrix.Cholesky();
            Console.WriteLine(@"Perform Cholesky decomposition");

            // 1. Lower triangular form of the Cholesky matrix
            Console.WriteLine(@"1. Lower triangular form of the Cholesky matrix");
            Console.WriteLine(cholesky.Factor.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // 2. Reconstruct initial matrix: A = L * LT
            var reconstruct = cholesky.Factor * cholesky.Factor.Transpose();
            Console.WriteLine(@"2. Reconstruct initial matrix: A = L*LT");
            Console.WriteLine(reconstruct.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // 3. Get determinant of the matrix
            Console.WriteLine(@"3. Determinant of the matrix");
            Console.WriteLine(cholesky.Determinant);
            Console.WriteLine();

            // 4. Get log determinant of the matrix
            Console.WriteLine(@"4. Log determinant of the matrix");
            Console.WriteLine(cholesky.DeterminantLn);
            Console.WriteLine();
        }
    }
}
