// <copyright file="QR.cs" company="Math.NET">
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
    /// QR factorization example. Any real square matrix A (m x n) may be decomposed as A = QR where Q is an orthogonal matrix (m x m)
    /// (its columns are orthogonal unit vectors meaning QTQ = I) and R (m x n) is an upper triangular matrix 
    /// (also called right triangular matrix).
    /// In this example two methods for actually computing the QR decomposition presented: by means of the Gram–Schmidt process and Householder transformations.
    /// </summary>
    /// <seealso cref="http://reference.wolfram.com/mathematica/ref/QRDecomposition.html"/>
    public class QR : IExample
    {
        /// <summary>
        /// Gets the name of this example
        /// </summary>
        public string Name
        {
            get
            {
                return "QR factorization";
            }
        }

        /// <summary>
        /// Gets the description of this example
        /// </summary>
        public string Description
        {
            get
            {
                return "Perform the QR factorization by means of the Gram–Schmidt process and Householder transformations";
            }
        }

        /// <summary>
        /// Run example
        /// </summary>
        /// <seealso cref="http://en.wikipedia.org/wiki/QR_decomposition">QR decomposition</seealso>
        public void Run()
        {
            // Format matrix output to console
            var formatProvider = (CultureInfo)CultureInfo.InvariantCulture.Clone();
            formatProvider.TextInfo.ListSeparator = " ";

            // Create 3 x 2 matrix
            var matrix = DenseMatrix.OfArray(new[,] { { 1.0, 2.0 }, { 3.0, 4.0 }, { 5.0, 6.0 } });
            Console.WriteLine(@"Initial 3x2 matrix");
            Console.WriteLine(matrix.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // Perform QR decomposition (Householder transformations)
            var qr = matrix.QR();
            Console.WriteLine(@"QR decomposition (Householder transformations)");

            // 1. Orthogonal Q matrix
            Console.WriteLine(@"1. Orthogonal Q matrix");
            Console.WriteLine(qr.Q.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // 2. Multiply Q matrix by its transpose gives identity matrix
            Console.WriteLine(@"2. Multiply Q matrix by its transpose gives identity matrix");
            Console.WriteLine(qr.Q.TransposeAndMultiply(qr.Q).ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // 3. Upper triangular factor R
            Console.WriteLine(@"3. Upper triangular factor R");
            Console.WriteLine(qr.R.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // 4. Reconstruct initial matrix: A = Q * R
            var reconstruct = qr.Q * qr.R;
            Console.WriteLine(@"4. Reconstruct initial matrix: A = Q*R");
            Console.WriteLine(reconstruct.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // Perform QR decomposition (Gram–Schmidt process)
            var gramSchmidt = matrix.GramSchmidt();
            Console.WriteLine(@"QR decomposition (Gram–Schmidt process)");

            // 5. Orthogonal Q matrix
            Console.WriteLine(@"5. Orthogonal Q matrix");
            Console.WriteLine(gramSchmidt.Q.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // 6. Multiply Q matrix by its transpose gives identity matrix
            Console.WriteLine(@"6. Multiply Q matrix by its transpose gives identity matrix");
            Console.WriteLine((gramSchmidt.Q.Transpose() * gramSchmidt.Q).ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // 7. Upper triangular factor R
            Console.WriteLine(@"7. Upper triangular factor R");
            Console.WriteLine(gramSchmidt.R.ToString("#0.00\t", formatProvider));
            Console.WriteLine();
            
            // 8. Reconstruct initial matrix: A = Q * R
            reconstruct = gramSchmidt.Q * gramSchmidt.R;
            Console.WriteLine(@"8. Reconstruct initial matrix: A = Q*R");
            Console.WriteLine(reconstruct.ToString("#0.00\t", formatProvider));
            Console.WriteLine();
        }
    }
}
