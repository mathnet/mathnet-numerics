// <copyright file="ErrorFunction.cs" company="Math.NET">
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
    /// Special Functions: error functions
    /// </summary>
    public class ErrorFunction : IExample
    {
        /// <summary>
        /// Gets the name of this example
        /// </summary>
        public string Name
        {
            get
            {
                return "Special Functions: error functions";
            }
        }

        /// <summary>
        /// Gets the description of this example
        /// </summary>
        public string Description
        {
            get
            {
                return "Error function (Gauss error function or probability integral)";
            }
        }

        /// <summary>
        /// Run example
        /// </summary>
        /// <seealso cref="http://en.wikipedia.org/wiki/Error_function">Error function</seealso>
        public void Run()
        {
            // 1. Calculate the error function at point 2
            Console.WriteLine(@"1. Calculate the error function at point 2");
            Console.WriteLine(SpecialFunctions.Erf(2));
            Console.WriteLine();

            // 2. Sample 10 values of the error function in [-1.0; 1.0]
            Console.WriteLine(@"2. Sample 10 values of the error function in [-1.0; 1.0]");
            var data = Generate.LinearSpacedMap(10, -1.0, 1.0, SpecialFunctions.Erf);
            for (var i = 0; i < data.Length; i++)
            {
                Console.Write(data[i].ToString("N") + @" ");
            }

            Console.WriteLine();
            Console.WriteLine();

            // 3. Calculate the complementary error function at point 2
            Console.WriteLine(@"3. Calculate the complementary error function at point 2");
            Console.WriteLine(SpecialFunctions.Erfc(2));
            Console.WriteLine();

            // 4. Sample 10 values of the complementary error function in [-1.0; 1.0]
            Console.WriteLine(@"4. Sample 10 values of the complementary error function in [-1.0; 1.0]");
            data = Generate.LinearSpacedMap(10, -1.0, 1.0, SpecialFunctions.Erfc);
            for (var i = 0; i < data.Length; i++)
            {
                Console.Write(data[i].ToString("N") + @" ");
            }

            Console.WriteLine();
            Console.WriteLine();

            // 5. Calculate the inverse error function at point z=0.5
            Console.WriteLine(@"5. Calculate the inverse error function at point z=0.5");
            Console.WriteLine(SpecialFunctions.ErfInv(0.5));
            Console.WriteLine();

            // 6. Sample 10 values of the inverse error function in [-1.0; 1.0]
            Console.WriteLine(@"6. Sample 10 values of the inverse error function in [-1.0; 1.0]");
            data = Generate.LinearSpacedMap(10, -1.0, 1.0, SpecialFunctions.ErfInv);
            for (var i = 0; i < data.Length; i++)
            {
                Console.Write(data[i].ToString("N") + @" ");
            }

            Console.WriteLine();
            Console.WriteLine();

            // 7. Calculate the complementary inverse error function at point z=0.5
            Console.WriteLine(@"7. Calculate the complementary inverse error function at point z=0.5");
            Console.WriteLine(SpecialFunctions.ErfcInv(0.5));
            Console.WriteLine();

            // 8. Sample 10 values of the complementary inverse error function in [-1.0; 1.0]
            Console.WriteLine(@"8. Sample 10 values of the complementary inverse error function in [-1.0; 1.0]");
            data = Generate.LinearSpacedMap(10, -1.0, 1.0, SpecialFunctions.ErfcInv);
            for (var i = 0; i < data.Length; i++)
            {
                Console.Write(data[i].ToString("N") + @" ");
            }

            Console.WriteLine();
        }
    }
}
