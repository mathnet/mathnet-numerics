// <copyright file="DirectSolvers.cs" company="Math.NET">
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
    /// Direct solvers (using matrix decompositions)
    /// </summary>
    /// <seealso cref="http://en.wikipedia.org/wiki/Numerical_analysis#Direct_and_iterative_methods"/>
    public class DirectSolvers : IExample
    {
        /// <summary>
        /// Gets the name of this example
        /// </summary>
        public string Name
        {
            get
            {
                return "Direct solvers";
            }
        }

        /// <summary>
        /// Gets the description of this example
        /// </summary>
        public string Description
        {
            get
            {
                return "Solve linear equations using matrix decompositions";
            }
        }

        /// <summary>
        /// Run example
        /// </summary>
        public void Run()
        {
            // Format matrix output to console
            var formatProvider = (CultureInfo) CultureInfo.InvariantCulture.Clone();
            formatProvider.TextInfo.ListSeparator = " ";

            // Solve next system of linear equations (Ax=b):
            // 5*x + 2*y - 4*z = -7
            // 3*x - 7*y + 6*z = 38
            // 4*x + 1*y + 5*z = 43

            // Create matrix "A" with coefficients 
            var matrixA = DenseMatrix.OfArray(new[,] {{5.00, 2.00, -4.00}, {3.00, -7.00, 6.00}, {4.00, 1.00, 5.00}});
            Console.WriteLine(@"Matrix 'A' with coefficients");
            Console.WriteLine(matrixA.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // Create vector "b" with the constant terms.
            var vectorB = new DenseVector(new[] {-7.0, 38.0, 43.0});
            Console.WriteLine(@"Vector 'b' with the constant terms");
            Console.WriteLine(vectorB.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // 1. Solve linear equations using LU decomposition
            var resultX = matrixA.LU().Solve(vectorB);
            Console.WriteLine(@"1. Solution using LU decomposition");
            Console.WriteLine(resultX.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // 2. Solve linear equations using QR decomposition
            resultX = matrixA.QR().Solve(vectorB);
            Console.WriteLine(@"2. Solution using QR decomposition");
            Console.WriteLine(resultX.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // 3. Solve linear equations using SVD decomposition
            matrixA.Svd().Solve(vectorB, resultX);
            Console.WriteLine(@"3. Solution using SVD decomposition");
            Console.WriteLine(resultX.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // 4. Solve linear equations using Gram-Shmidt decomposition
            matrixA.GramSchmidt().Solve(vectorB, resultX);
            Console.WriteLine(@"4. Solution using Gram-Shmidt decomposition");
            Console.WriteLine(resultX.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // 5. Verify result. Multiply coefficient matrix "A" by result vector "x"
            var reconstructVecorB = matrixA*resultX;
            Console.WriteLine(@"5. Multiply coefficient matrix 'A' by result vector 'x'");
            Console.WriteLine(reconstructVecorB.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // To use Cholesky or Eigenvalue decomposition coefficient matrix must be 
            // symmetric (for Evd and Cholesky) and positive definite (for Cholesky)
            // Multipy matrix "A" by its transpose - the result will be symmetric and positive definite matrix
            var newMatrixA = matrixA.TransposeAndMultiply(matrixA);
            Console.WriteLine(@"Symmetric positive definite matrix");
            Console.WriteLine(newMatrixA.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // 6. Solve linear equations using Cholesky decomposition
            newMatrixA.Cholesky().Solve(vectorB, resultX);
            Console.WriteLine(@"6. Solution using Cholesky decomposition");
            Console.WriteLine(resultX.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // 7. Solve linear equations using eigen value decomposition
            newMatrixA.Evd().Solve(vectorB, resultX);
            Console.WriteLine(@"7. Solution using eigen value decomposition");
            Console.WriteLine(resultX.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // 8. Verify result. Multiply new coefficient matrix "A" by result vector "x"
            reconstructVecorB = newMatrixA*resultX;
            Console.WriteLine(@"8. Multiply new coefficient matrix 'A' by result vector 'x'");
            Console.WriteLine(reconstructVecorB.ToString("#0.00\t", formatProvider));
            Console.WriteLine();
        }
    }
}
