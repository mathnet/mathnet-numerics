// <copyright file="BernoulliDistribution.cs" company="Math.NET">
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
    /// Bernoulli distribution example
    /// </summary>
    public class BernoulliDistribution : IExample
    {
        /// <summary>
        /// Gets the name of this example
        /// </summary>
        /// <seealso cref="http://reference.wolfram.com/mathematica/ref/BernoulliDistribution.html"/>
        public string Name
        {
            get
            {
                return "Bernoulli distribution";
            }
        }

        /// <summary>
        /// Gets the description of this example
        /// </summary>
        public string Description
        {
            get
            {
                return "Bernoulli distribution properties and samples generating examples";
            }
        }

        /// <summary>
        /// Run example
        /// </summary>
        /// <a href="http://en.wikipedia.org/wiki/Bernoulli_distribution">Bernoulli distribution</a>
        public void Run()
        {
            // 1. Initialize the new instance of the Bernoulli distribution class with parameter P = 0.2
            var bernoulli = new Bernoulli(0.2);
            Console.WriteLine(@"1. Initialize the new instance of the Bernoulli distribution class with parameter P = {0}", bernoulli.P);
            Console.WriteLine();

            // 2. Distributuion properties:
            Console.WriteLine(@"2. {0} distributuion properties:", bernoulli);

            // Cumulative distribution function
            Console.WriteLine(@"{0} - Сumulative distribution at location '3'", bernoulli.CumulativeDistribution(3).ToString(" #0.00000;-#0.00000"));

            // Probability density
            Console.WriteLine(@"{0} - Probability mass at location '3'", bernoulli.Probability(3).ToString(" #0.00000;-#0.00000"));

            // Log probability density
            Console.WriteLine(@"{0} - Log probability mass at location '3'", bernoulli.ProbabilityLn(3).ToString(" #0.00000;-#0.00000"));

            // Entropy
            Console.WriteLine(@"{0} - Entropy", bernoulli.Entropy.ToString(" #0.00000;-#0.00000"));

            // Largest element in the domain
            Console.WriteLine(@"{0} - Largest element in the domain", bernoulli.Maximum.ToString(" #0.00000;-#0.00000"));

            // Smallest element in the domain
            Console.WriteLine(@"{0} - Smallest element in the domain", bernoulli.Minimum.ToString(" #0.00000;-#0.00000"));

            // Mean
            Console.WriteLine(@"{0} - Mean", bernoulli.Mean.ToString(" #0.00000;-#0.00000"));

            // Mode
            Console.WriteLine(@"{0} - Mode", bernoulli.Mode.ToString(" #0.00000;-#0.00000"));

            // Variance
            Console.WriteLine(@"{0} - Variance", bernoulli.Variance.ToString(" #0.00000;-#0.00000"));

            // Standard deviation
            Console.WriteLine(@"{0} - Standard deviation", bernoulli.StdDev.ToString(" #0.00000;-#0.00000"));

            // Skewness
            Console.WriteLine(@"{0} - Skewness", bernoulli.Skewness.ToString(" #0.00000;-#0.00000"));
            Console.WriteLine();

            // 3. Generate 10 samples of the Bernoulli distribution
            Console.WriteLine(@"3. Generate 10 samples of the Bernoulli distribution");
            for (var i = 0; i < 10; i++)
            {
                Console.Write(bernoulli.Sample().ToString("N05") + @" ");
            }

            Console.WriteLine();
            Console.WriteLine();

            // 4. Generate 100000 samples of the Bernoulli(0.2) distribution and display histogram
            Console.WriteLine(@"4. Generate 100000 samples of the Bernoulli(0.2) distribution and display histogram");
            var data = new int[100000];
            Bernoulli.Samples(data, 0.2);
            ConsoleHelper.DisplayHistogram(data);
            Console.WriteLine();

            // 5. Generate 100000 samples of the Bernoulli(4) distribution and display histogram
            Console.WriteLine(@"5. Generate 100000 samples of the Bernoulli(0.9) distribution and display histogram");
            Bernoulli.Samples(data, 0.9);
            ConsoleHelper.DisplayHistogram(data);
            Console.WriteLine();

            // 6. Generate 100000 samples of the Bernoulli(8) distribution and display histogram
            Console.WriteLine(@"6. Generate 100000 samples of the Bernoulli(0.5) distribution and display histogram");
            Bernoulli.Samples(data, 0.5);
            ConsoleHelper.DisplayHistogram(data);
        }
    }
}
