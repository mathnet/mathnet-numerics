// <copyright file="Integration.cs" company="Math.NET">
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

namespace Examples
{
    /// <summary>
    /// Numeric Integration (Quadrature)
    /// </summary>
    /// <seealso cref="http://reference.wolfram.com/mathematica/ref/Integrate.html"/>
    public class Integration : IExample
    {
        /// <summary>
        /// Gets the name of this example
        /// </summary>
        public string Name
        {
            get
            {
                return "Numeric Integration";
            }
        }

        /// <summary>
        /// Gets the description of this example
        /// </summary>
        public string Description
        {
            get
            {
                return "Analytic integration of smooth functions with no discontinuitie or derivative discontinuities and no poles inside the interval";
            }
        }

        /// <summary>
        /// Run example
        /// </summary>
        /// <seealso cref="http://en.wikipedia.org/wiki/Trapezoidal_rule">Trapezoidal rule</seealso>
        public void Run()
        {
            // 1. Integrate x*x on interval [0, 10]
            Console.WriteLine(@"1. Integrate x*x on interval [0, 10]");
            var result = Integrate.OnClosedInterval(x => x * x, 0, 10);
            Console.WriteLine(result);
            Console.WriteLine();

            // 2. Integrate 1/(x^3 + 1) on interval [0, 1]
            Console.WriteLine(@"2. Integrate 1/(x^3 + 1) on interval [0, 1]");
            result = Integrate.OnClosedInterval(x => 1 / (Math.Pow(x, 3) + 1), 0, 1);
            Console.WriteLine(result);
            Console.WriteLine();

            // 3. Integrate f(x) = exp(-x/5) (2 + sin(2 * x)) on [0, 10]
            Console.WriteLine(@"3. Integrate f(x) = exp(-x/5) (2 + sin(2 * x)) on [0, 10]");
            result = Integrate.OnClosedInterval(x => Math.Exp(-x / 5) * (2 + Math.Sin(2 * x)), 0, 100);
            Console.WriteLine(result);
            Console.WriteLine();

            // 4. Integrate target function with absolute error = 1E-4
            Console.WriteLine(@"4. Integrate target function with absolute error = 1E-4 on [0, 10]");
            Console.WriteLine(@"public static double TargetFunctionA(double x)
{
    return Math.Exp(-x / 5) * (2 + Math.Sin(2 * x));
}");
            result = Integrate.OnClosedInterval(TargetFunctionA, 0, 100, 1e-4);
            Console.WriteLine(result);
            Console.WriteLine();
        }

        /// <summary>
        /// Test Function: f(x) = exp(-x/5) (2 + sin(2 * x))
        /// </summary>
        /// <param name="x">X parameter value</param>
        /// <returns>Calculation result</returns>
        public static double TargetFunctionA(double x)
        {
            return Math.Exp(-x / 5) * (2 + Math.Sin(2 * x));
        }
    }
}
