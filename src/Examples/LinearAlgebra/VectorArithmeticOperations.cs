// <copyright file="VectorArithmeticOperations.cs" company="Math.NET">
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
    /// Basic vector arithmetic operations as "+", "-", "*", "/"
    /// </summary>
    /// <seealso cref="http://reference.wolfram.com/mathematica/tutorial/VectorsAndMatrices.html"/>
    public class VectorArithmeticOperations : IExample
    {
        /// <summary>
        /// Gets the name of this example
        /// </summary>
        public string Name
        {
            get
            {
                return "Vector Arithmetics";
            }
        }

        /// <summary>
        /// Gets the description of this example
        /// </summary>
        public string Description
        {
            get
            {
                return "Basic operations between vector/vector and vector/matrix";
            }
        }

        /// <summary>
        /// Run example.
        /// </summary>
        /// <seealso cref="http://en.wikipedia.org/wiki/Euclidean_vector#Scalar_multiplication">Multiply vector by scalar</seealso>
        /// <seealso cref="http://en.wikipedia.org/wiki/Euclidean_vector#Dot_product">Multiply vector by vector (compute the dot product between two vectors)</seealso>
        /// <seealso cref="http://en.wikipedia.org/wiki/Euclidean_vector#Addition_and_subtraction">Vector addition and subtraction</seealso>
        /// <seealso cref="http://en.wikipedia.org/wiki/Outer_product">Outer Product of two vectors</seealso>
        public void Run()
        {
            // Initialize IFormatProvider to print matrix/vector data 
            var formatProvider = (CultureInfo)CultureInfo.InvariantCulture.Clone();
            formatProvider.TextInfo.ListSeparator = " ";

            // Create vector "X"
            var vectorX = new DenseVector(new[] { 1.0, 2.0, 3.0, 4.0, 5.0 });
            Console.WriteLine(@"Vector X");
            Console.WriteLine(vectorX.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // Create vector "Y"
            var vectorY = new DenseVector(new[] { 5.0, 4.0, 3.0, 2.0, 1.0 });
            Console.WriteLine(@"Vector Y");
            Console.WriteLine(vectorY.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // Multiply vector by scalar
            // 1. Using Multiply method and getting result into different vector instance
            var resultV = vectorX.Multiply(3.0);
            Console.WriteLine(@"Multiply vector by scalar using method Multiply. (result = X.Multiply(3.0))");
            Console.WriteLine(resultV.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // 2. Using operator "*"
            resultV = 3.0 * vectorX;
            Console.WriteLine(@"Multiply vector by scalar using operator *. (result = 3.0 * X)");
            Console.WriteLine(resultV.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // 3. Using Multiply method and updating vector itself
            vectorX.Multiply(3.0, vectorX);
            Console.WriteLine(@"Multiply vector by scalar using method Multiply. (X.Multiply(3.0, X))");
            Console.WriteLine(vectorX.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // Multiply vector by vector (compute the dot product between two vectors)
            // 1. Using operator "*"
            var dotProduct = vectorX * vectorY;
            Console.WriteLine(@"Dot product between two vectors using operator *. (result = X * Y)");
            Console.WriteLine(dotProduct);
            Console.WriteLine();

            // 2. Using DotProduct method and getting result into different vector instance
            dotProduct = vectorX.DotProduct(vectorY);
            Console.WriteLine(@"Dot product between two vectors using method DotProduct. (result = X.DotProduct(Y))");
            Console.WriteLine(dotProduct.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // Pointwise multiplies vector with another vector
            // 1. Using PointwiseMultiply method and getting result into different vector instance
            resultV = vectorX.PointwiseMultiply(vectorY);
            Console.WriteLine(@"Pointwise multiplies vector with another vector using method PointwiseMultiply. (result = X.PointwiseMultiply(Y))");
            Console.WriteLine(resultV.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // 2. Using PointwiseMultiply method and updating vector itself
            vectorX.PointwiseMultiply(vectorY, vectorX);
            Console.WriteLine(@"Pointwise multiplies vector with another vector using method PointwiseMultiply. (X.PointwiseMultiply(Y, X))");
            Console.WriteLine(vectorX.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // Pointwise divide vector with another vector
            // 1. Using PointwiseDivide method and getting result into different vector instance
            resultV = vectorX.PointwiseDivide(vectorY);
            Console.WriteLine(@"Pointwise divide vector with another vector using method PointwiseDivide. (result = X.PointwiseDivide(Y))");
            Console.WriteLine(resultV.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // 2. Using PointwiseDivide method and updating vector itself
            vectorX.PointwiseDivide(vectorY, vectorX);
            Console.WriteLine(@"Pointwise divide vector with another vector using method PointwiseDivide. (X.PointwiseDivide(Y, X))");
            Console.WriteLine(vectorX.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // Addition
            // 1. Using operator "+"
            resultV = vectorX + vectorY;
            Console.WriteLine(@"Add vectors using operator +. (result = X + Y)");
            Console.WriteLine(resultV.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // 2. Using Add method and getting result into different vector instance
            resultV = vectorX.Add(vectorY);
            Console.WriteLine(@"Add vectors using method Add. (result = X.Add(Y))");
            Console.WriteLine(resultV.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // 3. Using Add method and updating vector itself
            vectorX.Add(vectorY, vectorX);
            Console.WriteLine(@"Add vectors using method Add. (X.Add(Y, X))");
            Console.WriteLine(vectorX.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // Subtraction
            // 1. Using operator "-"
            resultV = vectorX - vectorY;
            Console.WriteLine(@"Subtract vectors using operator -. (result = X - Y)");
            Console.WriteLine(resultV.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // 2. Using Subtract method and getting result into different vector instance
            resultV = vectorX.Subtract(vectorY);
            Console.WriteLine(@"Subtract vectors using method Subtract. (result = X.Subtract(Y))");
            Console.WriteLine(resultV.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // 3. Using Subtract method and updating vector itself
            vectorX.Subtract(vectorY, vectorX);
            Console.WriteLine(@"Subtract vectors using method Subtract. (X.Subtract(Y, X))");
            Console.WriteLine(vectorX.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // Divide by scalar
            // 1. Using Divide method and getting result into different vector instance
            resultV = vectorX.Divide(3.0);
            Console.WriteLine(@"Divide vector by scalar using method Divide. (result = A.Divide(3.0))");
            Console.WriteLine(resultV.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // 2. Using Divide method and updating vector itself
            vectorX.Divide(3.0, vectorX);
            Console.WriteLine(@"Divide vector by scalar using method Divide. (X.Divide(3.0, X))");
            Console.WriteLine(vectorX.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // Outer Product of two vectors
            // 1. Using instanse method OuterProduct
            var resultM = vectorX.OuterProduct(vectorY);
            Console.WriteLine(@"Outer Product of two vectors using method OuterProduct. (X.OuterProduct(Y))");
            Console.WriteLine(resultM.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // 2. Using static method of the Vector class
            resultM = Vector.OuterProduct(vectorX, vectorY);
            Console.WriteLine(@"Outer Product of two vectors using method OuterProduct. (Vector.OuterProduct(X,Y))");
            Console.WriteLine(resultM.ToString("#0.00\t", formatProvider));
            Console.WriteLine();
        }
    }
}
