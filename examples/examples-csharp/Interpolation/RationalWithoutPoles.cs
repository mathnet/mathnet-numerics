// <copyright file="RationalWithoutPoles.cs" company="Math.NET">
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
using MathNet.Numerics.Random;

namespace Examples.InterpolationExamples
{
    /// <summary>
    /// Interpolation example
    /// </summary>
    /// <seealso cref="http://reference.wolfram.com/mathematica/ref/Interpolation.html"/>
    public class RationalWithoutPoles : IExample
    {
        /// <summary>
        /// Gets the name of this example
        /// </summary>
        public string Name
        {
            get
            {
                return "Interpolation - Rational Without Poles";
            }
        }

        /// <summary>
        /// Gets the description of this example
        /// </summary>
        public string Description
        {
            get
            {
                return "Barycentric Rational Interpolation without poles, using Mike Floater and Kai Hormann's Algorithm";
            }
        }

        /// <summary>
        /// Run example
        /// </summary>
        /// <seealso cref="http://en.wikipedia.org/wiki/Interpolation">Interpolation</seealso>
        public void Run()
        {
            // 1. Generate 10 samples of the function 1/(1+x*x) on interval [-5, 5]
            Console.WriteLine(@"1. Generate 10 samples of the function 1/(1+x*x) on interval [-5, 5]");
            double[] points = Generate.LinearSpaced(10, -5, 5);
            var values = Generate.Map(points, TargetFunction);
            Console.WriteLine();
            
            // 2. Create a floater hormann rational pole-free interpolation based on arbitrary points
            // This method is used by default when create an interpolation using Interpolate.Common method
            var method = Interpolate.RationalWithoutPoles(points, values);
            Console.WriteLine(@"2. Create a floater hormann rational pole-free interpolation based on arbitrary points");
            Console.WriteLine();

            // 3. Check if interpolation support integration
            Console.WriteLine(@"3. Support integration = {0}", method.SupportsIntegration);
            Console.WriteLine();

            // 4. Check if interpolation support differentiation
            Console.WriteLine(@"4. Support differentiation = {0}", method.SupportsDifferentiation);
            Console.WriteLine();

            // 5. Interpolate ten random points and compare to function results
            Console.WriteLine(@"5. Interpolate ten random points and compare to function results");
            var rng = new MersenneTwister(1);
            for (var i = 0; i < 10; i++)
            {
                // Generate random value from [0, 5]
                var point = rng.NextDouble() * 5;
                Console.WriteLine(@"Interpolate at {0} = {1}. Function({0}) = {2}", point.ToString("N05"), method.Interpolate(point).ToString("N05"), TargetFunction(point).ToString("N05"));
            }

            Console.WriteLine();
        }

        /// <summary>
        /// Test Function: f(x) = 1 / (1 + (x * x))
        /// </summary>
        /// <param name="x">X parameter value</param>
        /// <returns>Calculation result</returns>
        public static double TargetFunction(double x)
        {
            return 1 / (1 + (x * x));
        }
    }
}
