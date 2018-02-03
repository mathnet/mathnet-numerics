// <copyright file="Chebyshev.cs" company="Math.NET">
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
using MathNet.Numerics;

namespace Examples.SignalsExamples
{
    /// <summary>
    /// Example of generic function sampling and quantization provider
    /// </summary>
    public class Chebyshev : IExample
    {
              /// <summary>
        /// Gets the name of this example
        /// </summary>
        public string Name
        {
            get
            {
                return "Sampling - Chebyshev";
            }
        }

        /// <summary>
        /// Gets the description of this example
        /// </summary>
        public string Description
        {
            get
            {
                return "Samples a function at the roots of the Chebyshev polynomial";
            }
        }

        /// <summary>
        /// Run example
        /// </summary>
        public void Run()
        {
            // 1. Get 20 samples of f(x) = (x * x) / 2 at the roots of the Chebyshev polynomial of the first kind within interval [0, 10] 
            var roots = FindRoots.ChebychevPolynomialFirstKind(20, 0, 10);
            var result = Generate.Map(roots, Function);
            Console.WriteLine(@"1. Get 20 samples of f(x) = (x * x) / 2 at the roots of the Chebyshev polynomial of the first kind within interval [0, 10]");
            for (var i = 0; i < result.Length; i++)
            {
                Console.Write(result[i].ToString("N") + @" ");
            }

            Console.WriteLine();
            Console.WriteLine();

            // 2. Get 20 samples of f(x) = (x * x) / 2 at the roots of the Chebyshev polynomial of the second kind within interval [0, 10]
            roots = FindRoots.ChebychevPolynomialSecondKind(20, 0, 10);
            result = Generate.Map(roots, Function);
            Console.WriteLine(@"2. Get 20 samples of f(x) = (x * x) / 2 at the roots of the Chebyshev polynomial of the second kind within interval [0, 10]");
            for (var i = 0; i < result.Length; i++)
            {
                Console.Write(result[i].ToString("N") + @" ");
            }

            Console.WriteLine();
        }

        /// <summary>
        /// Fucntion f(x) = (x * x) / 2
        /// </summary>
        /// <param name="x">Input value</param>
        /// <returns>Calculation result</returns>
        public double Function(double x)
        {
            return Math.Pow(x, 2) / 2;
        }
    }
}
