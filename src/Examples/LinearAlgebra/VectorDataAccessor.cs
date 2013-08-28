// <copyright file="VectorDataAccessor.cs" company="Math.NET">
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
    /// Vector data access, copying and conversion examples
    /// </summary>
    public class VectorDataAccessor
    {
        /// <summary>
        /// Gets the name of this example
        /// </summary>
        public string Name
        {
            get
            {
                return "Vector data access, copying and conversion";
            }
        }

        /// <summary>
        /// Gets the description of this example
        /// </summary>
        public string Description
        {
            get
            {
                return "Examples of setting/getting values of a vector, copying and conversion vector into another vector";
            }
        }

        /// <summary>
        /// Run example
        /// </summary>
        public void Run()
        {
            // Format vector output to console
            var formatProvider = (CultureInfo)CultureInfo.InvariantCulture.Clone();
            formatProvider.TextInfo.ListSeparator = " ";

            // Create new empty vector
            var vectorA = new DenseVector(10);
            Console.WriteLine(@"Empty vector A");
            Console.WriteLine(vectorA.ToString("#0.00\t", formatProvider));
            Console.WriteLine();
            
            // 1. Fill vector by data using indexer []
            for (var i = 0; i < vectorA.Count; i++)
            {
                vectorA[i] = i;
            }

            Console.WriteLine(@"1. Fill vector by data using indexer []");
            Console.WriteLine(vectorA.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // 2. Fill vector by data using SetValues method
            vectorA.SetValues(new[] { 9.0, 8.0, 7.0, 6.0, 5.0, 4.0, 3.0, 2.0, 1.0, 0.0 });
            Console.WriteLine(@"2. Fill vector by data using SetValues method");
            Console.WriteLine(vectorA.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // 3. Convert Vector to double[]
            var data = vectorA.ToArray();
            Console.WriteLine(@"3. Convert vector to double array");
            for (var i = 0; i < data.Length; i++)
            {
                Console.Write(data[i].ToString("#0.00\t", formatProvider) + @" ");
            }

            Console.WriteLine();
            Console.WriteLine();

            // 4. Convert Vector to column matrix. A matrix based on this vector in column form (one single column)
            var columnMatrix = vectorA.ToColumnMatrix();
            Console.WriteLine(@"4. Convert vector to column matrix");
            Console.WriteLine(columnMatrix.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // 5. Convert Vector to row matrix. A matrix based on this vector in row form (one single row)
            var rowMatrix = vectorA.ToRowMatrix();
            Console.WriteLine(@"5. Convert vector to row matrix");
            Console.WriteLine(rowMatrix.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // 6. Clone vector
            var cloneA = vectorA.Clone();
            Console.WriteLine(@"6. Clone vector");
            Console.WriteLine(cloneA.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // 7. Clear vector
            cloneA.Clear();
            Console.WriteLine(@"7. Clear vector");
            Console.WriteLine(cloneA.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // 8. Copy part of vector into another vector. If you need to copy all data then use CopoTy(vector) method.
            vectorA.CopySubVectorTo(cloneA, 3, 3, 4);
            Console.WriteLine(@"8. Copy part of vector into another vector");
            Console.WriteLine(cloneA.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // 9. Get part of vector as another vector
            var subvector = vectorA.SubVector(0, 5);
            Console.WriteLine(@"9. Get subvector");
            Console.WriteLine(subvector.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

           // 10. Enumerator usage
            Console.WriteLine(@"10. Enumerator usage");
            foreach (var value in vectorA)
            {
                Console.Write(value.ToString("#0.00\t", formatProvider) + @" ");
            }

            Console.WriteLine();
            Console.WriteLine();

            // 11. Indexed enumerator usage
            Console.WriteLine(@"11. Enumerator usage");
            foreach (var value in vectorA.EnumerateIndexed())
            {
                Console.WriteLine(@"Index = {0}; Value = {1}", value.Item1, value.Item2.ToString("#0.00\t", formatProvider));
            }

            Console.WriteLine();
            Console.WriteLine();

            // 12. Indexed non-zero enumerator usage
            Console.WriteLine(@"11. Non-Zero Enumerator usage");
            foreach (var value in vectorA.EnumerateNonZeroIndexed())
            {
                Console.WriteLine(@"Index = {0}; Value = {1}", value.Item1, value.Item2.ToString("#0.00\t", formatProvider));
            }

            Console.WriteLine();
        }
    }
}
