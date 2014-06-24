// <copyright file="LogNormalDistribution.cs" company="Math.NET">
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

namespace Examples.ContinuousDistributionsExamples
{
    /// <summary>
    /// LogNormal distribution example
    /// </summary>
    public class LogNormalDistribution : IExample
    {
        /// <summary>
        /// Gets the name of this example
        /// </summary>
        /// <seealso cref="http://reference.wolfram.com/mathematica/ref/LogNormalDistribution.html"/>
        public string Name
        {
            get
            {
                return "LogNormal distribution";
            }
        }

        /// <summary>
        /// Gets the description of this example
        /// </summary>
        public string Description
        {
            get
            {
                return "LogNormal distribution properties and samples generating examples";
            }
        }

        /// <summary>
        /// Run example
        /// </summary>
        /// <a href="http://en.wikipedia.org/wiki/Log-normal_distribution">LogNormal distribution</a>
        public void Run()
        {
            // 1. Initialize the new instance of the LogNormal distribution class with parameters Mu = 0, Sigma = 1
            var logNormal = new LogNormal(0, 1);
            Console.WriteLine(@"1. Initialize the new instance of the LogNormal distribution class with parameters Mu = {0}, Sigma = {1}", logNormal.Mu, logNormal.Sigma);
            Console.WriteLine();

            // 2. Distributuion properties:
            Console.WriteLine(@"2. {0} distributuion properties:", logNormal);

            // Cumulative distribution function
            Console.WriteLine(@"{0} - Сumulative distribution at location '0.3'", logNormal.CumulativeDistribution(0.3).ToString(" #0.00000;-#0.00000"));

            // Probability density
            Console.WriteLine(@"{0} - Probability density at location '0.3'", logNormal.Density(0.3).ToString(" #0.00000;-#0.00000"));

            // Log probability density
            Console.WriteLine(@"{0} - Log probability density at location '0.3'", logNormal.DensityLn(0.3).ToString(" #0.00000;-#0.00000"));

            // Entropy
            Console.WriteLine(@"{0} - Entropy", logNormal.Entropy.ToString(" #0.00000;-#0.00000"));

            // Largest element in the domain
            Console.WriteLine(@"{0} - Largest element in the domain", logNormal.Maximum.ToString(" #0.00000;-#0.00000"));

            // Smallest element in the domain
            Console.WriteLine(@"{0} - Smallest element in the domain", logNormal.Minimum.ToString(" #0.00000;-#0.00000"));

            // Mean
            Console.WriteLine(@"{0} - Mean", logNormal.Mean.ToString(" #0.00000;-#0.00000"));

            // Median
            Console.WriteLine(@"{0} - Median", logNormal.Median.ToString(" #0.00000;-#0.00000"));

            // Mode
            Console.WriteLine(@"{0} - Mode", logNormal.Mode.ToString(" #0.00000;-#0.00000"));

            // Variance
            Console.WriteLine(@"{0} - Variance", logNormal.Variance.ToString(" #0.00000;-#0.00000"));

            // Standard deviation
            Console.WriteLine(@"{0} - Standard deviation", logNormal.StdDev.ToString(" #0.00000;-#0.00000"));

            // Skewness
            Console.WriteLine(@"{0} - Skewness", logNormal.Skewness.ToString(" #0.00000;-#0.00000"));
            Console.WriteLine();

            // 3. Generate 10 samples
            Console.WriteLine(@"3. Generate 10 samples");
            for (var i = 0; i < 10; i++)
            {
                Console.Write(logNormal.Sample().ToString("N05") + @" ");
            }

            Console.WriteLine();
            Console.WriteLine();

            // 4. Generate 100000 samples of the LogNormal(0, 1) distribution and display histogram
            Console.WriteLine(@"4. Generate 100000 samples of the LogNormal(0, 1) distribution and display histogram");
            var data = new double[100000];
            LogNormal.Samples(data, 0.0, 1.0);
            ConsoleHelper.DisplayHistogram(data);
            Console.WriteLine();

            // 5. Generate 100000 samples of the LogNormal(0, 0.5) distribution and display histogram
            Console.WriteLine(@"5. Generate 100000 samples of the LogNormal(0, 0.5) distribution and display histogram");
            LogNormal.Samples(data, 0.0, 0.5);
            ConsoleHelper.DisplayHistogram(data);
            Console.WriteLine();

            // 6. Generate 100000 samples of the LogNormal(5, 0.25) distribution and display histogram
            Console.WriteLine(@"6. Generate 100000 samples of the LogNormal(5, 0.25) distribution and display histogram");
            LogNormal.Samples(data, 5.0, 0.25);
            ConsoleHelper.DisplayHistogram(data);
        }
    }
}
