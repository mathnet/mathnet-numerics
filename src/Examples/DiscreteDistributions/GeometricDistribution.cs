// <copyright file="GeometricDistribution.cs" company="Math.NET">
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
    /// Geometric distribution example
    /// </summary>
    public class GeometricDistribution : IExample
    {
        /// <summary>
        /// Gets the name of this example
        /// </summary>
        /// <seealso cref="http://reference.wolfram.com/mathematica/ref/GeometricDistribution.html"/>
        public string Name
        {
            get
            {
                return "Geometric distribution";
            }
        }

        /// <summary>
        /// Gets the description of this example
        /// </summary>
        public string Description
        {
            get
            {
                return "Geometric distribution properties and samples generating examples";
            }
        }

        /// <summary>
        /// Run example
        /// </summary>
        /// <a href="http://en.wikipedia.org/wiki/Geometric_distribution">Geometric distribution</a>
        public void Run()
        {
            // 1. Initialize the new instance of the Geometric distribution class with parameter P = 0.2
            var geometric = new Geometric(0.2);
            Console.WriteLine(@"1. Initialize the new instance of the Geometric distribution class with parameter P = {0}", geometric.P);
            Console.WriteLine();

            // 2. Distributuion properties:
            Console.WriteLine(@"2. {0} distributuion properties:", geometric);

            // Cumulative distribution function
            Console.WriteLine(@"{0} - Сumulative distribution at location '3'", geometric.CumulativeDistribution(3).ToString(" #0.00000;-#0.00000"));

            // Probability density
            Console.WriteLine(@"{0} - Probability mass at location '3'", geometric.Probability(3).ToString(" #0.00000;-#0.00000"));

            // Log probability density
            Console.WriteLine(@"{0} - Log probability mass at location '3'", geometric.ProbabilityLn(3).ToString(" #0.00000;-#0.00000"));

            // Entropy
            Console.WriteLine(@"{0} - Entropy", geometric.Entropy.ToString(" #0.00000;-#0.00000"));

            // Largest element in the domain
            Console.WriteLine(@"{0} - Largest element in the domain", geometric.Maximum.ToString(" #0.00000;-#0.00000"));

            // Smallest element in the domain
            Console.WriteLine(@"{0} - Smallest element in the domain", geometric.Minimum.ToString(" #0.00000;-#0.00000"));

            // Mean
            Console.WriteLine(@"{0} - Mean", geometric.Mean.ToString(" #0.00000;-#0.00000"));
            
            // Median
            Console.WriteLine(@"{0} - Median", geometric.Median.ToString(" #0.00000;-#0.00000"));
            
            // Mode
            Console.WriteLine(@"{0} - Mode", geometric.Mode.ToString(" #0.00000;-#0.00000"));

            // Variance
            Console.WriteLine(@"{0} - Variance", geometric.Variance.ToString(" #0.00000;-#0.00000"));

            // Standard deviation
            Console.WriteLine(@"{0} - Standard deviation", geometric.StdDev.ToString(" #0.00000;-#0.00000"));

            // Skewness
            Console.WriteLine(@"{0} - Skewness", geometric.Skewness.ToString(" #0.00000;-#0.00000"));
            Console.WriteLine();

            // 3. Generate 10 samples of the Geometric distribution
            Console.WriteLine(@"3. Generate 10 samples of the Geometric distribution");
            for (var i = 0; i < 10; i++)
            {
                Console.Write(geometric.Sample().ToString("N05") + @" ");
            }

            Console.WriteLine();
            Console.WriteLine();

            // 4. Generate 100000 samples of the Geometric(0.2, 20) distribution and display histogram
            Console.WriteLine(@"4. Generate 100000 samples of the Geometric(0.2) distribution and display histogram");
            var data = new int[100000];
            Geometric.Samples(data, 0.2);
            ConsoleHelper.DisplayHistogram(data);
            Console.WriteLine();

            // 5. Generate 100000 samples of the Geometric(0.5) distribution and display histogram
            Console.WriteLine(@"5. Generate 100000 samples of the Geometric(0.5) distribution and display histogram");
            Geometric.Samples(data, 0.5);
            ConsoleHelper.DisplayHistogram(data);
            Console.WriteLine();

            // 6. Generate 100000 samples of the Geometric(0.8) distribution and display histogram
            Console.WriteLine(@"6. Generate 100000 samples of the Geometric(0.8) distribution and display histogram");
            Geometric.Samples(data, 0.8);
            ConsoleHelper.DisplayHistogram(data);
        }
    }
}
