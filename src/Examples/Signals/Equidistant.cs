// <copyright file="Equidistant.cs" company="Math.NET">
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
    public class Equidistant : IExample
    {
              /// <summary>
        /// Gets the name of this example
        /// </summary>
        public string Name
        {
            get
            {
                return "Sampling - Equidistant";
            }
        }

        /// <summary>
        /// Gets the description of this example
        /// </summary>
        public string Description
        {
            get
            {
                return "Samples a function equidistant";
            }
        }

        /// <summary>
        /// Run example
        /// </summary>
        public void Run()
        {
            // 1. Get 11 samples of f(x) = (x * x) / 2 equidistant within interval [-5, 5] 
            var result = Generate.LinearSpacedMap(11, -5, 5, Function);
            Console.WriteLine(@"1. Get 11 samples of f(x) = (x * x) / 2 equidistant within interval [-5, 5]");
            for (var i = 0; i < result.Length; i++)
            {
                Console.Write(result[i].ToString("N") + @" ");
            }

            Console.WriteLine();
            Console.WriteLine();

            // 2. Get 10 samples of f(x) = (x * x) / 2 equidistant starting at x=1 with step = 0.5 and retrieve sample points
            double[] samplePoints = Generate.LinearSpaced(10, 1.0, 5.5);
            result = Generate.Map(samplePoints, Function);
            Console.WriteLine(@"2. Get 10 samples of f(x) = (x * x) / 2 equidistant starting at x=1 with step = 0.5 and retrieve sample points");
            Console.Write(@"Points: ");
            for (var i = 0; i < samplePoints.Length; i++)
            {
                Console.Write(samplePoints[i].ToString("N") + @" ");
            }

            Console.WriteLine();
            Console.Write(@"Values: ");
            for (var i = 0; i < result.Length; i++)
            {
                Console.Write(result[i].ToString("N") + @" ");
            }

            Console.WriteLine();
            Console.WriteLine();

            // 3. Get 10 samples of f(x) = (x * x) / 2 equidistant within period = 10 and period offset = 5
            result = Generate.PeriodicMap(10, Function, 10, 1.0, 10, 5);
            Console.WriteLine(@"3. Get 10 samples of f(x) = (x * x) / 2 equidistant within period = 10 and period offset = 5");
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
