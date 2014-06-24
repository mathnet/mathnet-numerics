// <copyright file="DiscreteUniformDistribution.cs" company="Math.NET">
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
using MathNet.Numerics.Distributions;

namespace Examples.DiscreteDistributionsExamples
{
    /// <summary>
    /// DiscreteUniform distribution example
    /// </summary>
    public class DiscreteUniformDistribution : IExample
    {
        /// <summary>
        /// Gets the name of this example
        /// </summary>
        /// <seealso cref="http://reference.wolfram.com/mathematica/ref/DiscreteUniformDistribution.html"/>
        public string Name
        {
            get
            {
                return "DiscreteUniform distribution";
            }
        }

        /// <summary>
        /// Gets the description of this example
        /// </summary>
        public string Description
        {
            get
            {
                return "DiscreteUniform distribution properties and samples generating examples";
            }
        }

        /// <summary>
        /// Run example
        /// </summary>
        /// <a href="http://en.wikipedia.org/wiki/Discrete_uniform">DiscreteUniform distribution</a>
        public void Run()
        {
            // 1. Initialize the new instance of the DiscreteUniform distribution class with parameters LowerBound = 2, UpperBound = 10
            var discreteUniform = new DiscreteUniform(2, 10);
            Console.WriteLine(@"1. Initialize the new instance of the DiscreteUniform distribution class with parameters LowerBound = {0}, UpperBound = {1}", discreteUniform.LowerBound, discreteUniform.UpperBound);
            Console.WriteLine();

            // 2. Distributuion properties:
            Console.WriteLine(@"2. {0} distributuion properties:", discreteUniform);

            // Cumulative distribution function
            Console.WriteLine(@"{0} - Сumulative distribution at location '3'", discreteUniform.CumulativeDistribution(3).ToString(" #0.00000;-#0.00000"));

            // Probability density
            Console.WriteLine(@"{0} - Probability mass at location '3'", discreteUniform.Probability(3).ToString(" #0.00000;-#0.00000"));

            // Log probability density
            Console.WriteLine(@"{0} - Log probability mass at location '3'", discreteUniform.ProbabilityLn(3).ToString(" #0.00000;-#0.00000"));

            // Entropy
            Console.WriteLine(@"{0} - Entropy", discreteUniform.Entropy.ToString(" #0.00000;-#0.00000"));

            // Largest element in the domain
            Console.WriteLine(@"{0} - Largest element in the domain", discreteUniform.Maximum.ToString(" #0.00000;-#0.00000"));

            // Smallest element in the domain
            Console.WriteLine(@"{0} - Smallest element in the domain", discreteUniform.Minimum.ToString(" #0.00000;-#0.00000"));

            // Mean
            Console.WriteLine(@"{0} - Mean", discreteUniform.Mean.ToString(" #0.00000;-#0.00000"));
            
            // Median
            Console.WriteLine(@"{0} - Median", discreteUniform.Median.ToString(" #0.00000;-#0.00000"));
            
            // Mode
            Console.WriteLine(@"{0} - Mode", discreteUniform.Mode.ToString(" #0.00000;-#0.00000"));

            // Variance
            Console.WriteLine(@"{0} - Variance", discreteUniform.Variance.ToString(" #0.00000;-#0.00000"));

            // Standard deviation
            Console.WriteLine(@"{0} - Standard deviation", discreteUniform.StdDev.ToString(" #0.00000;-#0.00000"));

            // Skewness
            Console.WriteLine(@"{0} - Skewness", discreteUniform.Skewness.ToString(" #0.00000;-#0.00000"));
            Console.WriteLine();

            // 3. Generate 10 samples of the DiscreteUniform distribution
            Console.WriteLine(@"3. Generate 10 samples of the DiscreteUniform distribution");
            for (var i = 0; i < 10; i++)
            {
                Console.Write(discreteUniform.Sample().ToString("N05") + @" ");
            }

            Console.WriteLine();
            Console.WriteLine();

            // 4. Generate 100000 samples of the DiscreteUniform(2, 10) distribution and display histogram
            Console.WriteLine(@"4. Generate 100000 samples of the DiscreteUniform(2, 10) distribution and display histogram");
            var data = new int[100000];
            DiscreteUniform.Samples(data, 2, 10);
            ConsoleHelper.DisplayHistogram(data);
            Console.WriteLine();

            // 5. Generate 100000 samples of the DiscreteUniform(-10, 10) distribution and display histogram
            Console.WriteLine(@"5. Generate 100000 samples of the DiscreteUniform(-10, 10) distribution and display histogram");
            DiscreteUniform.Samples(data, -10, 10);
            ConsoleHelper.DisplayHistogram(data);
            Console.WriteLine();

            // 6. Generate 100000 samples of the DiscreteUniform(0, 40) distribution and display histogram
            Console.WriteLine(@"6. Generate 100000 samples of the DiscreteUniform(0, 40) distribution and display histogram");
            DiscreteUniform.Samples(data, 0, 40);
            ConsoleHelper.DisplayHistogram(data);
        }
    }
}
