// <copyright file="Gamma.cs" company="Math.NET">
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
    /// Special Functions: Gamma
    /// </summary>
    /// <seealso cref="http://reference.wolfram.com/mathematica/ref/Gamma.html"/>
    public class Gamma : IExample
    {
        /// <summary>
        /// Gets the name of this example
        /// </summary>
        public string Name
        {
            get
            {
                return "Special Functions: Gamma";
            }
        }

        /// <summary>
        /// Gets the description of this example
        /// </summary>
        public string Description
        {
            get
            {
                return "Gamma, incomplete Gamma, regularized Gamma";
            }
        }

        /// <summary>
        /// Run example
        /// </summary>
        /// <seealso cref="http://en.wikipedia.org/wiki/Gamma_function">Gamma function</seealso>
        public void Run()
        {
            // 1. Compute the Gamma function of 10
            Console.WriteLine(@"1. Compute the Gamma function of 10");
            Console.WriteLine(SpecialFunctions.Gamma(10).ToString("N"));
            Console.WriteLine();

            // 2. Compute the logarithm of the Gamma function of 10
            Console.WriteLine(@"2. Compute the logarithm of the Gamma function of 10");
            Console.WriteLine(SpecialFunctions.GammaLn(10).ToString("N"));
            Console.WriteLine();

            // 3. Compute the lower incomplete gamma(a, x) function at a = 10, x = 14 
            Console.WriteLine(@"3. Compute the lower incomplete gamma(a, x) function at a = 10, x = 14");
            Console.WriteLine(SpecialFunctions.GammaLowerIncomplete(10, 14).ToString("N"));
            Console.WriteLine();

            // 4. Compute the lower incomplete gamma(a, x) function at a = 10, x = 100 
            Console.WriteLine(@"4. Compute the lower incomplete gamma(a, x) function at a = 10, x = 100");
            Console.WriteLine(SpecialFunctions.GammaLowerIncomplete(10, 100).ToString("N"));
            Console.WriteLine();

            // 5. Compute the upper incomplete gamma(a, x) function at a = 10, x = 0 
            Console.WriteLine(@"5. Compute the upper incomplete gamma(a, x) function at a = 10, x = 0");
            Console.WriteLine(SpecialFunctions.GammaUpperIncomplete(10, 0).ToString("N"));
            Console.WriteLine();

            // 6. Compute the upper incomplete gamma(a, x) function at a = 10, x = 10 
            Console.WriteLine(@"6. Compute the upper incomplete gamma(a, x) function at a = 10, x = 100");
            Console.WriteLine(SpecialFunctions.GammaLowerIncomplete(10, 10).ToString("N"));
            Console.WriteLine();

            // 7. Compute the lower regularized gamma(a, x) function at a = 10, x = 14 
            Console.WriteLine(@"7. Compute the lower regularized gamma(a, x) function at a = 10, x = 14");
            Console.WriteLine(SpecialFunctions.GammaLowerRegularized(10, 14).ToString("N"));
            Console.WriteLine();

            // 8. Compute the lower regularized gamma(a, x) function at a = 10, x = 100 
            Console.WriteLine(@"8. Compute the lower regularized gamma(a, x) function at a = 10, x = 100");
            Console.WriteLine(SpecialFunctions.GammaLowerRegularized(10, 100).ToString("N"));
            Console.WriteLine();

            // 9. Compute the upper regularized gamma(a, x) function at a = 10, x = 0 
            Console.WriteLine(@"9. Compute the upper regularized gamma(a, x) function at a = 10, x = 0");
            Console.WriteLine(SpecialFunctions.GammaUpperRegularized(10, 0).ToString("N"));
            Console.WriteLine();

            // 10. Compute the upper regularized gamma(a, x) function at a = 10, x = 10 
            Console.WriteLine(@"10. Compute the upper regularized gamma(a, x) function at a = 10, x = 100");
            Console.WriteLine(SpecialFunctions.GammaUpperRegularized(10, 10).ToString("N"));
            Console.WriteLine();
        }
    }
}
