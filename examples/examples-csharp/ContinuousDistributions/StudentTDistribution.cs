// <copyright file="StudentTDistribution.cs" company="Math.NET">
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
    /// StudentT distribution example
    /// </summary>
    public class StudentTDistribution : IExample
    {
        /// <summary>
        /// Gets the name of this example
        /// </summary>
        /// <seealso cref="http://reference.wolfram.com/mathematica/ref/StudentTDistribution.html"/>
        public string Name
        {
            get
            {
                return "StudentT distribution";
            }
        }

        /// <summary>
        /// Gets the description of this example
        /// </summary>
        public string Description
        {
            get
            {
                return "StudentT distribution properties and samples generating examples";
            }
        }

        /// <summary>
        /// Run example
        /// </summary>
        /// <a href="http://en.wikipedia.org/wiki/StudentT_distribution">StudentT distribution</a>
        public void Run()
        {
            // 1. Initialize the new instance of the StudentT distribution class with parameters Location = 0, Scale = 1, DegreesOfFreedom = 1
            var studentT = new StudentT();
            Console.WriteLine(@"1. Initialize the new instance of the StudentT distribution class with parameters Location = {0}, Scale = {1}, DegreesOfFreedom = {2}", studentT.Location, studentT.Scale, studentT.DegreesOfFreedom);
            Console.WriteLine();

            // 2. Distributuion properties:
            Console.WriteLine(@"2. {0} distributuion properties:", studentT);

            // Cumulative distribution function
            Console.WriteLine(@"{0} - Сumulative distribution at location '0.3'", studentT.CumulativeDistribution(0.3).ToString(" #0.00000;-#0.00000"));

            // Probability density
            Console.WriteLine(@"{0} - Probability density at location '0.3'", studentT.Density(0.3).ToString(" #0.00000;-#0.00000"));

            // Log probability density
            Console.WriteLine(@"{0} - Log probability density at location '0.3'", studentT.DensityLn(0.3).ToString(" #0.00000;-#0.00000"));

            // Entropy
            Console.WriteLine(@"{0} - Entropy", studentT.Entropy.ToString(" #0.00000;-#0.00000"));

            // Largest element in the domain
            Console.WriteLine(@"{0} - Largest element in the domain", studentT.Maximum.ToString(" #0.00000;-#0.00000"));

            // Smallest element in the domain
            Console.WriteLine(@"{0} - Smallest element in the domain", studentT.Minimum.ToString(" #0.00000;-#0.00000"));

            // Mean
            Console.WriteLine(@"{0} - Mean", studentT.Mean.ToString(" #0.00000;-#0.00000"));

            // Median
            Console.WriteLine(@"{0} - Median", studentT.Median.ToString(" #0.00000;-#0.00000"));

            // Mode
            Console.WriteLine(@"{0} - Mode", studentT.Mode.ToString(" #0.00000;-#0.00000"));

            // Variance
            Console.WriteLine(@"{0} - Variance", studentT.Variance.ToString(" #0.00000;-#0.00000"));

            // Standard deviation
            Console.WriteLine(@"{0} - Standard deviation", studentT.StdDev.ToString(" #0.00000;-#0.00000"));

            // 3. Generate 10 samples of the StudentT distribution
            Console.WriteLine(@"3. Generate 10 samples of the StudentT distribution");
            for (var i = 0; i < 10; i++)
            {
                Console.Write(studentT.Sample().ToString("N05") + @" ");
            }

            Console.WriteLine();
            Console.WriteLine();

            // 4. Generate 100000 samples of the StudentT(0, 1, 1) distribution and display histogram
            Console.WriteLine(@"4. Generate 100000 samples of the StudentT(0, 1, 1) distribution and display histogram");
            var data = new double[100000];
            StudentT.Samples(data, 0, 1, 1);
            ConsoleHelper.DisplayHistogram(data);

            // 5. Generate 100000 samples of the StudentT(0, 1, 5) distribution and display histogram
            Console.WriteLine(@"5. Generate 100000 samples of the StudentT(0, 1, 5) distribution and display histogram");
            StudentT.Samples(data, 0, 1, 5);
            ConsoleHelper.DisplayHistogram(data);
            Console.WriteLine();

            // 6. Generate 100000 samples of the StudentT(0, 1, 10) distribution and display histogram
            Console.WriteLine(@"6. Generate 100000 samples of the StudentT(0, 1, 10) distribution and display histogram");
            StudentT.Samples(data, 0, 1, 10);
            ConsoleHelper.DisplayHistogram(data);
        }
    }
}
