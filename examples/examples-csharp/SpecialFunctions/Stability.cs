// <copyright file="Stability.cs" company="Math.NET">
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
    /// Special Functions: Stability
    /// </summary>
    public class Stability : IExample
    {
        /// <summary>
        /// Gets the name of this example
        /// </summary>
        public string Name
        {
            get
            {
                return "Special Functions: Stability";
            }
        }

        /// <summary>
        /// Gets the description of this example
        /// </summary>
        public string Description
        {
            get
            {
                return "Exponential, Hypotenuse, Series";
            }
        }

        /// <summary>
        /// Run example
        /// </summary>
        /// <seealso cref="http://en.wikipedia.org/wiki/Hypotenuse">Hypotenuse</seealso>
        public void Run()
        {
            // 1. Compute numerically stable exponential of 10 minus one 
            Console.WriteLine(@"1. Compute numerically stable exponential of 4.2876 minus one");
            Console.WriteLine(SpecialFunctions.ExponentialMinusOne(4.2876));
            Console.WriteLine();

            // 2. Compute regular System.Math exponential of 15.28 minus one 
            Console.WriteLine(@"2. Compute regular System.Math exponential of 4.2876 minus one ");
            Console.WriteLine(Math.Exp(4.2876) - 1);
            Console.WriteLine();

            // 3. Compute numerically stable hypotenuse of a right angle triangle with a = 5, b = 3
            Console.WriteLine(@"3. Compute numerically stable hypotenuse of a right angle triangle with a = 5, b = 3");
            Console.WriteLine(SpecialFunctions.Hypotenuse(5, 3));
            Console.WriteLine();
        }
    }
}
