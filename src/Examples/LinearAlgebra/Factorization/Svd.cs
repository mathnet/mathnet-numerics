// <copyright file="Svd.cs" company="Math.NET">
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
    /// SVD factorization example. Suppose M is an m-by-n matrix whose entries are real numbers. 
    /// Then there exists a factorization of the form M = UΣVT where:
    /// - U is an m-by-m unitary matrix;
    /// - Σ is m-by-n diagonal matrix with nonnegative real numbers on the diagonal;
    /// - VT denotes transpose of V, an n-by-n unitary matrix; 
    /// Such a factorization is called a singular-value decomposition of M. A common convention is to order the diagonal 
    /// entries Σ(i,i) in descending order. In this case, the diagonal matrix Σ is uniquely determined 
    /// by M (though the matrices U and V are not). The diagonal entries of Σ are known as the singular values of M.
    /// </summary>
    /// <seealso cref="http://reference.wolfram.com/mathematica/ref/SingularValueDecomposition.html"/>
    public class Svd : IExample
    {
        /// <summary>
        /// Gets the name of this example
        /// </summary>
        public string Name
        {
            get
            {
                return "Svd factorization";
            }
        }

        /// <summary>
        /// Gets the description of this example
        /// </summary>
        public string Description
        {
            get
            {
                return "Perform the Svd factorization";
            }
        }

        /// <summary>
        /// Run example
        /// </summary>
        /// <seealso cref="http://en.wikipedia.org/wiki/Singular_value_decomposition">SVD decomposition</seealso>
        public void Run()
        {
            // Format matrix output to console
            var formatProvider = (CultureInfo) CultureInfo.InvariantCulture.Clone();
            formatProvider.TextInfo.ListSeparator = " ";

            // Create square matrix
            var matrix = DenseMatrix.OfArray(new[,] {{4.0, 1.0}, {3.0, 2.0}});
            Console.WriteLine(@"Initial square matrix");
            Console.WriteLine(matrix.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // Perform full SVD decomposition
            var svd = matrix.Svd();
            Console.WriteLine(@"Perform full SVD decomposition");

            // 1. Left singular vectors
            Console.WriteLine(@"1. Left singular vectors");
            Console.WriteLine(svd.U.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // 2. Singular values as vector
            Console.WriteLine(@"2. Singular values as vector");
            Console.WriteLine(svd.S.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // 3. Singular values as diagonal matrix
            Console.WriteLine(@"3. Singular values as diagonal matrix");
            Console.WriteLine(svd.W.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // 4. Right singular vectors
            Console.WriteLine(@"4. Right singular vectors");
            Console.WriteLine(svd.VT.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // 5. Multiply U matrix by its transpose
            var identinty = svd.U*svd.U.Transpose();
            Console.WriteLine(@"5. Multiply U matrix by its transpose");
            Console.WriteLine(identinty.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // 6. Multiply V matrix by its transpose
            identinty = svd.VT.TransposeAndMultiply(svd.VT);
            Console.WriteLine(@"6. Multiply V matrix by its transpose");
            Console.WriteLine(identinty.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // 7. Reconstruct initial matrix: A = U*Σ*VT
            var reconstruct = svd.U*svd.W*svd.VT;
            Console.WriteLine(@"7. Reconstruct initial matrix: A = U*S*VT");
            Console.WriteLine(reconstruct.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // 8. Condition Number of the matrix
            Console.WriteLine(@"8. Condition Number of the matrix");
            Console.WriteLine(svd.ConditionNumber);
            Console.WriteLine();

            // 9. Determinant of the matrix
            Console.WriteLine(@"9. Determinant of the matrix");
            Console.WriteLine(svd.Determinant);
            Console.WriteLine();

            // 10. 2-norm of the matrix
            Console.WriteLine(@"10. 2-norm of the matrix");
            Console.WriteLine(svd.L2Norm);
            Console.WriteLine();

            // 11. Rank of the matrix
            Console.WriteLine(@"11. Rank of the matrix");
            Console.WriteLine(svd.Rank);
            Console.WriteLine();

            // Perform partial SVD decomposition, without computing the singular U and VT vectors
            svd = matrix.Svd(false);
            Console.WriteLine(@"Perform partial SVD decomposition, without computing the singular U and VT vectors");

            // 12. Singular values as vector
            Console.WriteLine(@"12. Singular values as vector");
            Console.WriteLine(svd.S.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // 13. Singular values as diagonal matrix
            Console.WriteLine(@"13. Singular values as diagonal matrix");
            Console.WriteLine(svd.W.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // 14. Access to left singular vectors when partial SVD decomposition was performed
            try
            {
                Console.WriteLine(@"14. Access to left singular vectors when partial SVD decomposition was performed");
                Console.WriteLine(svd.U.ToString("#0.00\t", formatProvider));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine();
            }

            // 15. Access to right singular vectors when partial SVD decomposition was performed
            try
            {
                Console.WriteLine(@"15. Access to right singular vectors when partial SVD decomposition was performed");
                Console.WriteLine(svd.VT.ToString("#0.00\t", formatProvider));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine();
            }
        }
    }
}
