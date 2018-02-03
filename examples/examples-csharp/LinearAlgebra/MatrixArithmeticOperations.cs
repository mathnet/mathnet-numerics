// <copyright file="MatrixArithmeticOperations.cs" company="Math.NET">
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
    /// Basic matrix arithmetic operations as "+", "-", "*", "/"
    /// </summary>
    /// <seealso cref="http://reference.wolfram.com/mathematica/tutorial/VectorsAndMatrices.html"/>
    public class MatrixArithmeticOperations : IExample
    {
        /// <summary>
        /// Gets the name of this example
        /// </summary>
        public string Name
        {
            get
            {
                return "Matrix Arithmetics";
            }
        }

        /// <summary>
        /// Gets the description of this example
        /// </summary>
        public string Description
        {
            get
            {
                return "Basic operations between matrix/matrix and matrix/vecor";
            }
        }

        /// <summary>
        /// Run example
        /// </summary>
        /// <seealso cref="http://en.wikipedia.org/wiki/Matrix_multiplication#Scalar_multiplication">Multiply matrix by scalar</seealso>
        /// <seealso cref="http://reference.wolfram.com/mathematica/tutorial/MultiplyingVectorsAndMatrices.html">Multiply matrix by vector</seealso>
        /// <seealso cref="http://en.wikipedia.org/wiki/Matrix_multiplication#Matrix_product">Multiply matrix by matrix</seealso>
        /// <seealso cref="http://en.wikipedia.org/wiki/Matrix_multiplication#Hadamard_product">Pointwise multiplies matrix with another matrix</seealso>
        /// <seealso cref="http://en.wikipedia.org/wiki/Matrix_%28mathematics%29#Basic_operations">Addition and subtraction</seealso>
        public void Run()
        {
            // Initialize IFormatProvider to print matrix/vector data 
            var formatProvider = (CultureInfo)CultureInfo.InvariantCulture.Clone();
            formatProvider.TextInfo.ListSeparator = " ";

            // Create matrix "A"
            var matrixA = DenseMatrix.OfArray(new[,] { { 1.0, 2.0, 3.0 }, { 4.0, 5.0, 6.0 }, { 7.0, 8.0, 9.0 } });
            Console.WriteLine(@"Matrix A");
            Console.WriteLine(matrixA.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // Create matrix "B"
            var matrixB = DenseMatrix.OfArray(new[,] { { 1.0, 3.0, 5.0 }, { 2.0, 4.0, 6.0 }, { 3.0, 5.0, 7.0 } });
            Console.WriteLine(@"Matrix B");
            Console.WriteLine(matrixB.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // Multiply matrix by scalar
            // 1. Using operator "*"
            var resultM = 3.0 * matrixA;
            Console.WriteLine(@"Multiply matrix by scalar using operator *. (result = 3.0 * A)");
            Console.WriteLine(resultM.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // 2. Using Multiply method and getting result into different matrix instance
            resultM = (DenseMatrix)matrixA.Multiply(3.0);
            Console.WriteLine(@"Multiply matrix by scalar using method Multiply. (result = A.Multiply(3.0))");
            Console.WriteLine(resultM.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // 3. Using Multiply method and updating matrix itself
            matrixA.Multiply(3.0, matrixA);
            Console.WriteLine(@"Multiply matrix by scalar using method Multiply. (A.Multiply(3.0, A))");
            Console.WriteLine(matrixA.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // Multiply matrix by vector (right-multiply)
            var vector = new DenseVector(new[] { 1.0, 2.0, 3.0 });
            Console.WriteLine(@"Vector");
            Console.WriteLine(vector.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // 1. Using operator "*"
            var resultV = matrixA * vector;
            Console.WriteLine(@"Multiply matrix by vector using operator *. (result = A * vec)");
            Console.WriteLine(resultV.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // 2. Using Multiply method and getting result into different vector instance
            resultV = (DenseVector)matrixA.Multiply(vector);
            Console.WriteLine(@"Multiply matrix by vector using method Multiply. (result = A.Multiply(vec))");
            Console.WriteLine(resultV.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // 3. Using Multiply method and updating vector itself
            matrixA.Multiply(vector, vector);
            Console.WriteLine(@"Multiply matrix by vector using method Multiply. (A.Multiply(vec, vec))");
            Console.WriteLine(vector.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // Multiply vector by matrix (left-multiply)
            // 1. Using operator "*"
            resultV = vector * matrixA;
            Console.WriteLine(@"Multiply vector by matrix using operator *. (result = vec * A)");
            Console.WriteLine(resultV.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // 2. Using LeftMultiply method and getting result into different vector instance
            resultV = (DenseVector)matrixA.LeftMultiply(vector);
            Console.WriteLine(@"Multiply vector by matrix using method LeftMultiply. (result = A.LeftMultiply(vec))");
            Console.WriteLine(resultV.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // 3. Using LeftMultiply method and updating vector itself
            matrixA.LeftMultiply(vector, vector);
            Console.WriteLine(@"Multiply vector by matrix using method LeftMultiply. (A.LeftMultiply(vec, vec))");
            Console.WriteLine(vector.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // Multiply matrix by matrix
            // 1. Using operator "*"
            resultM = matrixA * matrixB;
            Console.WriteLine(@"Multiply matrix by matrix using operator *. (result = A * B)");
            Console.WriteLine(resultM.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // 2. Using Multiply method and getting result into different matrix instance
            resultM = (DenseMatrix)matrixA.Multiply(matrixB);
            Console.WriteLine(@"Multiply matrix by matrix using method Multiply. (result = A.Multiply(B))");
            Console.WriteLine(resultM.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // 3. Using Multiply method and updating matrix itself
            matrixA.Multiply(matrixB, matrixA);
            Console.WriteLine(@"Multiply matrix by matrix using method Multiply. (A.Multiply(B, A))");
            Console.WriteLine(matrixA.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // Pointwise multiplies matrix with another matrix
            // 1. Using PointwiseMultiply method and getting result into different matrix instance
            resultM = (DenseMatrix)matrixA.PointwiseMultiply(matrixB);
            Console.WriteLine(@"Pointwise multiplies matrix with another matrix using method PointwiseMultiply. (result = A.PointwiseMultiply(B))");
            Console.WriteLine(resultM.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // 2. Using PointwiseMultiply method and updating matrix itself
            matrixA.PointwiseMultiply(matrixB, matrixA);
            Console.WriteLine(@"Pointwise multiplies matrix with another matrix using method PointwiseMultiply. (A.PointwiseMultiply(B, A))");
            Console.WriteLine(matrixA.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // Pointwise divide matrix with another matrix
            // 1. Using PointwiseDivide method and getting result into different matrix instance
            resultM = (DenseMatrix)matrixA.PointwiseDivide(matrixB);
            Console.WriteLine(@"Pointwise divide matrix with another matrix using method PointwiseDivide. (result = A.PointwiseDivide(B))");
            Console.WriteLine(resultM.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // 2. Using PointwiseDivide method and updating matrix itself
            matrixA.PointwiseDivide(matrixB, matrixA);
            Console.WriteLine(@"Pointwise divide matrix with another matrix using method PointwiseDivide. (A.PointwiseDivide(B, A))");
            Console.WriteLine(matrixA.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // Addition
            // 1. Using operator "+"
            resultM = matrixA + matrixB;
            Console.WriteLine(@"Add matrices using operator +. (result = A + B)");
            Console.WriteLine(resultM.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // 2. Using Add method and getting result into different matrix instance
            resultM = (DenseMatrix)matrixA.Add(matrixB);
            Console.WriteLine(@"Add matrices using method Add. (result = A.Add(B))");
            Console.WriteLine(resultM.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // 3. Using Add method and updating matrix itself
            matrixA.Add(matrixB, matrixA);
            Console.WriteLine(@"Add matrices using method Add. (A.Add(B, A))");
            Console.WriteLine(matrixA.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // Subtraction
            // 1. Using operator "-"
            resultM = matrixA - matrixB;
            Console.WriteLine(@"Subtract matrices using operator -. (result = A - B)");
            Console.WriteLine(resultM.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // 2. Using Subtract method and getting result into different matrix instance
            resultM = (DenseMatrix)matrixA.Subtract(matrixB);
            Console.WriteLine(@"Subtract matrices using method Subtract. (result = A.Subtract(B))");
            Console.WriteLine(resultM.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // 3. Using Subtract method and updating matrix itself
            matrixA.Subtract(matrixB, matrixA);
            Console.WriteLine(@"Subtract matrices using method Subtract. (A.Subtract(B, A))");
            Console.WriteLine(matrixA.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // Divide by scalar
            // 1. Using Divide method and getting result into different matrix instance
            resultM = (DenseMatrix)matrixA.Divide(3.0);
            Console.WriteLine(@"Divide matrix by scalar using method Divide. (result = A.Divide(3.0))");
            Console.WriteLine(resultM.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // 2. Using Divide method and updating matrix itself
            matrixA.Divide(3.0, matrixA);
            Console.WriteLine(@"Divide matrix by scalar using method Divide. (A.Divide(3.0, A))");
            Console.WriteLine(matrixA.ToString("#0.00\t", formatProvider));
            Console.WriteLine();
        }
    }
}
