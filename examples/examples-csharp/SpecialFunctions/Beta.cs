// <copyright file="Beta.cs" company="Math.NET">
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
    /// Special Functions: Beta
    /// </summary>
    /// <seealso cref="http://reference.wolfram.com/mathematica/ref/Beta.html"/>
    public class Beta : IExample
    {
        /// <summary>
        /// Gets the name of this example
        /// </summary>
        public string Name
        {
            get
            {
                return "Special Functions: Beta";
            }
        }

        /// <summary>
        /// Gets the description of this example
        /// </summary>
        public string Description
        {
            get
            {
                return "Beta, incomplete Beta, regularized Beta";
            }
        }

        /// <summary>
        /// Run example
        /// </summary>
        /// <seealso cref="http://en.wikipedia.org/wiki/Beta_function">Beta function</seealso>
        public void Run()
        {
            // 1. Compute the Beta function at z = 1.0, w = 3.0
            Console.WriteLine(@"1. Compute the Beta function at z = 1.0, w = 3.0");
            Console.WriteLine(SpecialFunctions.Beta(1.0, 3.0));
            Console.WriteLine();

            // 2. Compute the logarithm of the Beta function at z = 1.0, w = 3.0
            Console.WriteLine(@"2. Compute the logarithm of the Beta function at z = 1.0, w = 3.0");
            Console.WriteLine(SpecialFunctions.BetaLn(1.0, 3.0));
            Console.WriteLine();

            // 3. Compute the Beta incomplete function at z = 1.0, w = 3.0, x = 0.7 
            Console.WriteLine(@"3. Compute the Beta incomplete function at z = 1.0, w = 3.0, x = 0.7");
            Console.WriteLine(SpecialFunctions.BetaIncomplete(1.0, 3.0, 0.7));
            Console.WriteLine();

            // 4. Compute the Beta incomplete function at z = 1.0, w = 3.0, x = 1.0 
            Console.WriteLine(@"4. Compute the Beta incomplete function at z = 1.0, w = 3.0, x = 1.0");
            Console.WriteLine(SpecialFunctions.BetaIncomplete(1.0, 3.0, 1.0));
            Console.WriteLine();

            // 5. Compute the Beta regularized function at z = 1.0, w = 3.0, x = 0.7
            Console.WriteLine(@"5. Compute the Beta regularized function at z = 1.0, w = 3.0, x = 0.7");
            Console.WriteLine(SpecialFunctions.BetaRegularized(1.0, 3.0, 0.7));
            Console.WriteLine();

            // 6. Compute the Beta regularized  function at z = 1.0, w = 3.0, x = 1.0 
            Console.WriteLine(@"6. Compute the Beta regularized function at z = 1.0, w = 3.0, x = 1.0");
            Console.WriteLine(SpecialFunctions.BetaRegularized(1.0, 3.0, 1.0));
            Console.WriteLine();
        }
    }
}
