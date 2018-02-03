// <copyright file="VectorInitialization.cs" company="Math.NET">
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
using MathNet.Numerics.LinearAlgebra;

namespace Examples.LinearAlgebraExamples
{
    /// <summary>
    /// Vector initialization examples
    /// </summary>
    public class VectorInitialization : IExample
    {
        /// <summary>
        /// Gets the name of this example
        /// </summary>
        public string Name
        {
            get
            {
                return "Vector initialization";
            }
        }

        /// <summary>
        /// Gets the description of this example
        /// </summary>
        public string Description
        {
            get
            {
                return "Examples of creating vector instances";
            }
        }

        /// <summary>
        /// Run example
        /// </summary>
        public void Run()
        {
            // 1. Initialize a new instance of the empty vector with a given size
            var vector1 = Vector<double>.Build.Dense(5);

            // 2. Initialize a new instance of the vector with a given size and each element set to the given value
            var vector2 = Vector<double>.Build.Dense(5, i => i + 3.0);

            // 3. Initialize a new instance of the vector from an array.
            var vector3 = Vector<double>.Build.Dense(new[] { 1.0, 2.0, 3.0, 4.0, 5.0 });

            // 4. Initialize a new instance of the vector by copying the values from another.
            var vector4 = Vector<double>.Build.DenseOfVector(vector3);

            // Format vector output to console
            var formatProvider = (CultureInfo)CultureInfo.InvariantCulture.Clone();
            formatProvider.TextInfo.ListSeparator = " ";

            Console.WriteLine(@"Vector 1");
            Console.WriteLine(vector1.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            Console.WriteLine(@"Vector 2");
            Console.WriteLine(vector2.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            Console.WriteLine(@"Vector 3");
            Console.WriteLine(vector3.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            Console.WriteLine(@"Vector 4");
            Console.WriteLine(vector4.ToString("#0.00\t", formatProvider));
            Console.WriteLine();
        }
    }
}
