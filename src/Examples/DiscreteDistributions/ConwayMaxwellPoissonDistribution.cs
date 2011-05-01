// <copyright file="ConwayMaxwellPoissonDistribution.cs" company="Math.NET">
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
    /// ConwayMaxwellPoisson distribution example
    /// </summary>
    public class ConwayMaxwellPoissonDistribution : IExample
    {
        /// <summary>
        /// Gets the name of this example
        /// </summary>
        public string Name
        {
            get
            {
                return "ConwayMaxwellPoisson distribution";
            }
        }

        /// <summary>
        /// Gets the description of this example
        /// </summary>
        public string Description
        {
            get
            {
                return "ConwayMaxwellPoisson distribution properties and samples generating examples";
            }
        }

        /// <summary>
        /// Run example
        /// </summary>
        /// <a href="http://en.wikipedia.org/wiki/Conway%E2%80%93Maxwell%E2%80%93Poisson_distribution">ConwayMaxwellPoisson distribution</a>
        public void Run()
        {
            // 1. Initialize the new instance of the ConwayMaxwellPoisson distribution class with parameters Lambda = 2, Nu = 1
            var binomial = new ConwayMaxwellPoisson(2, 1);
            Console.WriteLine(@"1. Initialize the new instance of the ConwayMaxwellPoisson distribution class with parameters Lambda = {0}, Nu = {1}", binomial.Lambda, binomial.Nu);
            Console.WriteLine();

            // 2. Distributuion properties:
            Console.WriteLine(@"2. {0} distributuion properties:", binomial);

            // Cumulative distribution function
            Console.WriteLine(@"{0} - Сumulative distribution at location '3'", binomial.CumulativeDistribution(3).ToString(" #0.00000;-#0.00000"));

            // Probability density
            Console.WriteLine(@"{0} - Probability mass at location '3'", binomial.Probability(3).ToString(" #0.00000;-#0.00000"));

            // Log probability density
            Console.WriteLine(@"{0} - Log probability mass at location '3'", binomial.ProbabilityLn(3).ToString(" #0.00000;-#0.00000"));

            // Smallest element in the domain
            Console.WriteLine(@"{0} - Smallest element in the domain", binomial.Minimum.ToString(" #0.00000;-#0.00000"));

            // Mean
            Console.WriteLine(@"{0} - Mean", binomial.Mean.ToString(" #0.00000;-#0.00000"));
            
            // Variance
            Console.WriteLine(@"{0} - Variance", binomial.Variance.ToString(" #0.00000;-#0.00000"));

            // Standard deviation
            Console.WriteLine(@"{0} - Standard deviation", binomial.StdDev.ToString(" #0.00000;-#0.00000"));
            Console.WriteLine();

            // 3. Generate 10 samples of the ConwayMaxwellPoisson distribution
            Console.WriteLine(@"3. Generate 10 samples of the ConwayMaxwellPoisson distribution");
            for (var i = 0; i < 10; i++)
            {
                Console.Write(binomial.Sample().ToString("N05") + @" ");
            }

            Console.WriteLine();
            Console.WriteLine();

            // 4. Generate 100000 samples of the ConwayMaxwellPoisson(4, 1) distribution and display histogram
            Console.WriteLine(@"4. Generate 100000 samples of the ConwayMaxwellPoisson(4, 1) distribution and display histogram");
            var data = new double[100000];
            for (var i = 0; i < data.Length; i++)
            {
                data[i] = binomial.Sample();
            }

            ConsoleHelper.DisplayHistogram(data);
            Console.WriteLine();

            // 5. Generate 100000 samples of the ConwayMaxwellPoisson(2, 1) distribution and display histogram
            Console.WriteLine(@"5. Generate 100000 samples of the ConwayMaxwellPoisson(2, 1) distribution and display histogram");
            binomial.Lambda = 2;
            for (var i = 0; i < data.Length; i++)
            {
                data[i] = binomial.Sample();
            }

            ConsoleHelper.DisplayHistogram(data);
            Console.WriteLine();

            // 6. Generate 100000 samples of the ConwayMaxwellPoisson(5, 2) distribution and display histogram
            Console.WriteLine(@"6. Generate 100000 samples of the ConwayMaxwellPoisson(5, 2) distribution and display histogram");
            binomial.Lambda = 5;
            binomial.Nu = 2;
            for (var i = 0; i < data.Length; i++)
            {
                data[i] = binomial.Sample();
            }

            ConsoleHelper.DisplayHistogram(data);
        }
    }
}
