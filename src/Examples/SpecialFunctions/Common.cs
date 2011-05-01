// <copyright file="Common.cs" company="Math.NET">
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
    /// Special Functions
    /// </summary>
    /// <seealso cref="http://reference.wolfram.com/mathematica/ref/PolyGamma.html"/>
    /// <seealso cref="http://reference.wolfram.com/mathematica/ref/HarmonicNumber.html"/>
    public class Common : IExample
    {
        /// <summary>
        /// Gets the name of this example
        /// </summary>
        public string Name
        {
            get
            {
                return "Special Functions";
            }
        }

        /// <summary>
        /// Gets the description of this example
        /// </summary>
        public string Description
        {
            get
            {
                return "Harmonic, DiGamma, Logit, Logistic";
            }
        }

        /// <summary>
        /// Run example
        /// </summary>
        /// <seealso cref="http://en.wikipedia.org/wiki/Digamma_function">Digamma function</seealso>
        /// <seealso cref="http://en.wikipedia.org/wiki/Harmonic_number">Harmonic number</seealso>
        /// <seealso cref="http://en.wikipedia.org/wiki/Harmonic_number#Generalized_harmonic_numbers">Generalized harmonic numbers</seealso>
        /// <seealso cref="http://en.wikipedia.org/wiki/Logistic_function">Logistic function</seealso>
        /// <seealso cref="http://en.wikipedia.org/wiki/Logit">Logit function</seealso>
        public void Run()
        {
            // 1. Calculate the Digamma function at point 5.0
            Console.WriteLine(@"1. Calculate the Digamma function at point 5.0");
            Console.WriteLine(SpecialFunctions.DiGamma(5.0));
            Console.WriteLine();

            // 2. Calculate the inverse Digamma function at point 1.5
            Console.WriteLine(@"2. Calculate the inverse Digamma function at point 1.5");
            Console.WriteLine(SpecialFunctions.DiGammaInv(1.5));
            Console.WriteLine();

            // 3. Calculate the 10'th Harmonic number
            Console.WriteLine(@"3. Calculate the 10'th Harmonic number");
            Console.WriteLine(SpecialFunctions.Harmonic(10));
            Console.WriteLine();

            // 4. Calculate the generalized harmonic number of order 10 of 3.0.
            Console.WriteLine(@"4. Calculate the generalized harmonic number of order 10 of 3.0");
            Console.WriteLine(SpecialFunctions.GeneralHarmonic(10, 3.0));
            Console.WriteLine();

            // 5. Calculate the logistic function of 3.0
            Console.WriteLine(@"5. Calculate the logistic function of 3.0");
            Console.WriteLine(SpecialFunctions.Logistic(3.0));
            Console.WriteLine();

            // 6. Calculate the logit function of 0.3
            Console.WriteLine(@"6. Calculate the logit function of 0.3");
            Console.WriteLine(SpecialFunctions.Logit(0.3));
            Console.WriteLine();
        }
    }
}
