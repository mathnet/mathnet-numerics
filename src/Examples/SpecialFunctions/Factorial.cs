// <copyright file="Factorial.cs" company="Math.NET">
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

namespace Examples.SpecialFunctionsExamples
{
    /// <summary>
    /// Special Functions: Factorial
    /// </summary>
    /// <seealso cref="http://reference.wolfram.com/mathematica/ref/Factorial.html"/>
    /// <seealso cref="http://reference.wolfram.com/mathematica/ref/Binomial.html"/>
    /// <seealso cref="http://reference.wolfram.com/mathematica/ref/Multinomial.html"/>
    public class Factorial : IExample
    {
        /// <summary>
        /// Gets the name of this example
        /// </summary>
        public string Name
        {
            get
            {
                return "Special Functions: Factorial";
            }
        }

        /// <summary>
        /// Gets the description of this example
        /// </summary>
        public string Description
        {
            get
            {
                return "Factorial, Binomial, Multinomial";
            }
        }

        /// <summary>
        /// Run example
        /// </summary>
        /// <seealso cref="http://en.wikipedia.org/wiki/Factorial">Factorial</seealso>
        /// <seealso cref="http://en.wikipedia.org/wiki/Binomial_coefficient">Binomial coefficient</seealso>
        /// <seealso cref="http://en.wikipedia.org/wiki/Multinomial_theorem#Multinomial_coefficients">Multinomial coefficients</seealso>
        public void Run()
        {
            // 1. Compute the factorial of 5
            Console.WriteLine(@"1. Compute the factorial of 5");
            Console.WriteLine(SpecialFunctions.Factorial(5).ToString("N"));
            Console.WriteLine();

            // 2. Compute the logarithm of the factorial of 5
            Console.WriteLine(@"2. Compute the logarithm of the factorial of 5");
            Console.WriteLine(SpecialFunctions.FactorialLn(5).ToString("N"));
            Console.WriteLine();

            // 3. Compute the binomial coefficient: 10 choose 8
            Console.WriteLine(@"3. Compute the binomial coefficient: 10 choose 8");
            Console.WriteLine(SpecialFunctions.Binomial(10, 8).ToString("N"));
            Console.WriteLine();

            // 4. Compute the logarithm of the binomial coefficient: 10 choose 8
            Console.WriteLine(@"4. Compute the logarithm of the binomial coefficient: 10 choose 8");
            Console.WriteLine(SpecialFunctions.BinomialLn(10, 8).ToString("N"));
            Console.WriteLine();

            // 5. Compute the multinomial coefficient: 10 choose 2, 3, 5 
            Console.WriteLine(@"5. Compute the multinomial coefficient: 10 choose 2, 3, 5");
            Console.WriteLine(SpecialFunctions.Multinomial(10, new[] { 2, 3, 5 }).ToString("N"));
            Console.WriteLine();
        }
    }
}
