// <copyright file="NegativeBinomialDistribution.cs" company="Math.NET">
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
    /// NegativeBinomial distribution example
    /// </summary>
    public class NegativeBinomialDistribution : IExample
    {
        /// <summary>
        /// Gets the name of this example
        /// </summary>
        /// <seealso cref="http://reference.wolfram.com/mathematica/ref/NegativeBinomialDistribution.html"/>
        public string Name
        {
            get
            {
                return "NegativeBinomial distribution";
            }
        }

        /// <summary>
        /// Gets the description of this example
        /// </summary>
        public string Description
        {
            get
            {
                return "NegativeBinomial distribution properties and samples generating examples";
            }
        }

        /// <summary>
        /// Run example
        /// </summary>
        /// <a href="http://en.wikipedia.org/wiki/Negative_binomial">NegativeBinomial distribution</a>
        public void Run()
        {
            // 1. Initialize the new instance of the NegativeBinomial distribution class with parameters P = 0.2, R = 20
            var negativeBinomial = new NegativeBinomial(20, 0.2);
            Console.WriteLine(@"1. Initialize the new instance of the NegativeBinomial distribution class with parameters P = {0}, N = {1}", negativeBinomial.P, negativeBinomial.R);
            Console.WriteLine();

            // 2. Distributuion properties:
            Console.WriteLine(@"2. {0} distributuion properties:", negativeBinomial);

            // Cumulative distribution function
            Console.WriteLine(@"{0} - Сumulative distribution at location '3'", negativeBinomial.CumulativeDistribution(3).ToString(" #0.00000;-#0.00000"));

            // Probability density
            Console.WriteLine(@"{0} - Probability mass at location '3'", negativeBinomial.Probability(3).ToString(" #0.00000;-#0.00000"));

            // Log probability density
            Console.WriteLine(@"{0} - Log probability mass at location '3'", negativeBinomial.ProbabilityLn(3).ToString(" #0.00000;-#0.00000"));

            // Largest element in the domain
            Console.WriteLine(@"{0} - Largest element in the domain", negativeBinomial.Maximum.ToString(" #0.00000;-#0.00000"));

            // Smallest element in the domain
            Console.WriteLine(@"{0} - Smallest element in the domain", negativeBinomial.Minimum.ToString(" #0.00000;-#0.00000"));

            // Mean
            Console.WriteLine(@"{0} - Mean", negativeBinomial.Mean.ToString(" #0.00000;-#0.00000"));
            
            // Mode
            Console.WriteLine(@"{0} - Mode", negativeBinomial.Mode.ToString(" #0.00000;-#0.00000"));

            // Variance
            Console.WriteLine(@"{0} - Variance", negativeBinomial.Variance.ToString(" #0.00000;-#0.00000"));

            // Standard deviation
            Console.WriteLine(@"{0} - Standard deviation", negativeBinomial.StdDev.ToString(" #0.00000;-#0.00000"));

            // Skewness
            Console.WriteLine(@"{0} - Skewness", negativeBinomial.Skewness.ToString(" #0.00000;-#0.00000"));
            Console.WriteLine();

            // 3. Generate 10 samples of the NegativeBinomial distribution
            Console.WriteLine(@"3. Generate 10 samples of the NegativeBinomial distribution");
            for (var i = 0; i < 10; i++)
            {
                Console.Write(negativeBinomial.Sample().ToString("N05") + @" ");
            }

            Console.WriteLine();
            Console.WriteLine();

            // 4. Generate 100000 samples of the NegativeBinomial(0.2, 20) distribution and display histogram
            Console.WriteLine(@"4. Generate 100000 samples of the NegativeBinomial(20, 0.2) distribution and display histogram");
            var data = new int[100000];
            NegativeBinomial.Samples(data, 20, 0.2);
            ConsoleHelper.DisplayHistogram(data);
            Console.WriteLine();

            // 5. Generate 100000 samples of the NegativeBinomial(0.7, 20) distribution and display histogram
            Console.WriteLine(@"5. Generate 100000 samples of the NegativeBinomial(20, 0.7) distribution and display histogram");
            NegativeBinomial.Samples(data, 20, 0.7);
            ConsoleHelper.DisplayHistogram(data);
            Console.WriteLine();

            // 6. Generate 100000 samples of the NegativeBinomial(0.5, 1) distribution and display histogram
            Console.WriteLine(@"6. Generate 100000 samples of the NegativeBinomial(1, 0.5) distribution and display histogram");
            NegativeBinomial.Samples(data, 1, 0.5);
            ConsoleHelper.DisplayHistogram(data);
        }
    }
}
