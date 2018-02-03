// <copyright file="Evd.cs" company="Math.NET">
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
    /// EVD factorization example. If A is symmetric, then A = V*D*V' where the eigenvalue matrix D is
    /// diagonal and the eigenvector matrix V is orthogonal. I.e. A = V*D*V' and V*VT=I.
    /// If A is not symmetric, then the eigenvalue matrix D is block diagonal
    /// with the real eigenvalues in 1-by-1 blocks and any complex eigenvalues,
    /// lambda + i*mu, in 2-by-2 blocks, [lambda, mu; -mu, lambda].  The
    /// columns of V represent the eigenvectors in the sense thatA * V = V * D. 
    /// The matrix V may be badly conditioned, or even singular, so the validity of the equation
    /// A = V*D*Inverse(V) depends upon V.Condition()
    /// </summary>
    /// <seealso cref="http://reference.wolfram.com/mathematica/ref/Norm.html"/>
    public class Evd : IExample
    {
         /// <summary>
        /// Gets the name of this example
        /// </summary>
        public string Name
        {
            get
            {
                return "Evd factorization";
            }
        }

        /// <summary>
        /// Gets the description of this example
        /// </summary>
        public string Description
        {
            get
            {
                return "Perform the Evd factorization: eigenvalues and eigenvectors calculation";
            }
        }

        /// <summary>
        /// Run example
        /// </summary>
        /// <seealso cref="http://en.wikipedia.org/wiki/Eigenvalue,_eigenvector_and_eigenspace">EVD decomposition</seealso>
        public void Run()
        {
            // Format matrix output to console
            var formatProvider = (CultureInfo)CultureInfo.InvariantCulture.Clone();
            formatProvider.TextInfo.ListSeparator = " ";

            // Create square symmetric matrix
            var matrix = DenseMatrix.OfArray(new[,] { { 1.0, 2.0, 3.0 }, { 2.0, 1.0, 4.0 }, { 3.0, 4.0, 1.0 } });
            Console.WriteLine(@"Initial square symmetric matrix");
            Console.WriteLine(matrix.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // Perform eigenvalue decomposition of symmetric matrix
            var evd = matrix.Evd();
            Console.WriteLine(@"Perform eigenvalue decomposition of symmetric matrix");

            // 1. Eigen vectors
            Console.WriteLine(@"1. Eigen vectors");
            Console.WriteLine(evd.EigenVectors.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // 2. Eigen values as a complex vector
            Console.WriteLine(@"2. Eigen values as a complex vector");
            Console.WriteLine(evd.EigenValues.ToString("N", formatProvider));
            Console.WriteLine();

            // 3. Eigen values as the block diagonal matrix
            Console.WriteLine(@"3. Eigen values as the block diagonal matrix");
            Console.WriteLine(evd.D.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // 4. Multiply V by its transpose VT
            var identity = evd.EigenVectors.TransposeAndMultiply(evd.EigenVectors);
            Console.WriteLine(@"4. Multiply V by its transpose VT: V*VT = I");
            Console.WriteLine(identity.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // 5. Reconstruct initial matrix: A = V*D*V'
            var reconstruct = evd.EigenVectors * evd.D * evd.EigenVectors.Transpose();
            Console.WriteLine(@"5. Reconstruct initial matrix: A = V*D*V'");
            Console.WriteLine(reconstruct.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // 6. Determinant of the matrix
            Console.WriteLine(@"6. Determinant of the matrix");
            Console.WriteLine(evd.Determinant);
            Console.WriteLine();

            // 7. Rank of the matrix
            Console.WriteLine(@"7. Rank of the matrix");
            Console.WriteLine(evd.Rank);
            Console.WriteLine();

            // Fill matrix by random values
            var rnd = new Random(1);
            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    matrix[i, j] = rnd.NextDouble();
                }
            }

            Console.WriteLine(@"Fill matrix by random values");
            Console.WriteLine(matrix.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // Perform eigenvalue decomposition of non-symmetric matrix
            evd = matrix.Evd();
            Console.WriteLine(@"Perform eigenvalue decomposition of non-symmetric matrix");

            // 8. Eigen vectors
            Console.WriteLine(@"8. Eigen vectors");
            Console.WriteLine(evd.EigenVectors.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // 9. Eigen values as a complex vector
            Console.WriteLine(@"9. Eigen values as a complex vector");
            Console.WriteLine(evd.EigenValues.ToString("N", formatProvider));
            Console.WriteLine();

            // 10. Eigen values as the block diagonal matrix
            Console.WriteLine(@"10. Eigen values as the block diagonal matrix");
            Console.WriteLine(evd.D.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // 11. Multiply A * V
            var av = matrix * evd.EigenVectors;
            Console.WriteLine(@"11. Multiply A * V");
            Console.WriteLine(av.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // 12. Multiply V * D
            var vd = evd.EigenVectors * evd.D;
            Console.WriteLine(@"12. Multiply V * D");
            Console.WriteLine(vd.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // 13. Reconstruct non-symmetriv matrix A = V * D * Vinverse
            reconstruct = evd.EigenVectors * evd.D * evd.EigenVectors.Inverse();
            Console.WriteLine(@"13. Reconstruct non-symmetriv matrix A = V * D * Vinverse");
            Console.WriteLine(reconstruct.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // 14. Determinant of the matrix
            Console.WriteLine(@"14. Determinant of the matrix");
            Console.WriteLine(evd.Determinant);
            Console.WriteLine();

            // 15. Rank of the matrix
            Console.WriteLine(@"15. Rank of the matrix");
            Console.WriteLine(evd.Rank);
            Console.WriteLine();
        }
    }
}
