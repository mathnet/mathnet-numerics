// <copyright file="CategoricalDistribution.cs" company="Math.NET">
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
    /// Categorical distribution example
    /// </summary>
    public class CategoricalDistribution : IExample
    {
        /// <summary>
        /// Gets the name of this example
        /// </summary>
        public string Name
        {
            get
            {
                return "Categorical distribution";
            }
        }

        /// <summary>
        /// Gets the description of this example
        /// </summary>
        public string Description
        {
            get
            {
                return "Categorical distribution properties and samples generating examples";
            }
        }

        /// <summary>
        /// Run example
        /// </summary>
        /// <a href="http://en.wikipedia.org/wiki/Categorical_distribution">Categorical distribution</a>
        public void Run()
        {
            // 1. Initialize the new instance of the Categorical distribution class with parameters P = (0.1, 0.2, 0.25, 0.45)
            var binomial = new Categorical(new[] { 0.1, 0.2, 0.25, 0.45 });
            Console.WriteLine(@"1. Initialize the new instance of the Categorical distribution class with parameters P = (0.1, 0.2, 0.25, 0.45)");
            Console.WriteLine();

            // 2. Distributuion properties:
            Console.WriteLine(@"2. {0} distributuion properties:", binomial);

            // Cumulative distribution function
            Console.WriteLine(@"{0} - Сumulative distribution at location '3'", binomial.CumulativeDistribution(3).ToString(" #0.00000;-#0.00000"));

            // Probability density
            Console.WriteLine(@"{0} - Probability mass at location '3'", binomial.Probability(3).ToString(" #0.00000;-#0.00000"));

            // Log probability density
            Console.WriteLine(@"{0} - Log probability mass at location '3'", binomial.ProbabilityLn(3).ToString(" #0.00000;-#0.00000"));

            // Entropy
            Console.WriteLine(@"{0} - Entropy", binomial.Entropy.ToString(" #0.00000;-#0.00000"));

            // Largest element in the domain
            Console.WriteLine(@"{0} - Largest element in the domain", binomial.Maximum.ToString(" #0.00000;-#0.00000"));

            // Smallest element in the domain
            Console.WriteLine(@"{0} - Smallest element in the domain", binomial.Minimum.ToString(" #0.00000;-#0.00000"));

            // Mean
            Console.WriteLine(@"{0} - Mean", binomial.Mean.ToString(" #0.00000;-#0.00000"));
            
            // Median
            Console.WriteLine(@"{0} - Median", binomial.Median.ToString(" #0.00000;-#0.00000"));
            
            // Variance
            Console.WriteLine(@"{0} - Variance", binomial.Variance.ToString(" #0.00000;-#0.00000"));

            // Standard deviation
            Console.WriteLine(@"{0} - Standard deviation", binomial.StdDev.ToString(" #0.00000;-#0.00000"));

            // 3. Generate 10 samples of the Categorical distribution
            Console.WriteLine(@"3. Generate 10 samples of the Categorical distribution");
            for (var i = 0; i < 10; i++)
            {
                Console.Write(binomial.Sample().ToString("N05") + @" ");
            }

            Console.WriteLine();
            Console.WriteLine();

            // 4. Generate 100000 samples of the Categorical(new []{ 0.1, 0.2, 0.25, 0.45 }) distribution and display histogram
            Console.WriteLine(@"4. Generate 100000 samples of the Categorical(0.1, 0.2, 0.25, 0.45) distribution and display histogram");
            var data = new int[100000];
            Categorical.Samples(data, new[] { 0.1, 0.2, 0.25, 0.45 });
            ConsoleHelper.DisplayHistogram(data);
            Console.WriteLine();

            // 5. Generate 100000 samples of the Categorical(new []{ 0.6, 0.2, 0.1, 0.1 }) distribution and display histogram
            Console.WriteLine(@"5. Generate 100000 samples of the Categorical(0.6, 0.2, 0.1, 0.1) distribution and display histogram");
            Categorical.Samples(data, new[] { 0.6, 0.2, 0.1, 0.1 });
            ConsoleHelper.DisplayHistogram(data);
        }
    }
}
