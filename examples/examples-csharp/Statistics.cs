// <copyright file="Statistics.cs" company="Math.NET">
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
using MathNet.Numerics.Distributions;
using MathNet.Numerics.Statistics;

namespace Examples
{
    /// <summary>
    /// Statistics on set of data
    /// </summary>
    public class Statistics : IExample
    {
        /// <summary>
        /// Gets the name of this example
        /// </summary>
        public string Name
        {
            get
            {
                return "Statistics";
            }
        }

        /// <summary>
        /// Gets the description of this example
        /// </summary>
        public string Description
        {
            get
            {
                return "Basic statistics on set of data, correlation";
            }
        }

        /// <summary>
        /// Run example
        /// </summary>
        /// <seealso cref="http://en.wikipedia.org/wiki/Pearson_product-moment_correlation_coefficient">Pearson product-moment correlation coefficient</seealso>
        public void Run()
        {
            // 1. Initialize the new instance of the ChiSquare distribution class with parameter dof = 5.
            var chiSquare = new ChiSquared(5);
            Console.WriteLine(@"1. Initialize the new instance of the ChiSquare distribution class with parameter DegreesOfFreedom = {0}", chiSquare.DegreesOfFreedom);
            Console.WriteLine(@"{0} distributuion properties:", chiSquare);
            Console.WriteLine(@"{0} - Largest element", chiSquare.Maximum.ToString(" #0.00000;-#0.00000"));
            Console.WriteLine(@"{0} - Smallest element", chiSquare.Minimum.ToString(" #0.00000;-#0.00000"));
            Console.WriteLine(@"{0} - Mean", chiSquare.Mean.ToString(" #0.00000;-#0.00000"));
            Console.WriteLine(@"{0} - Median", chiSquare.Median.ToString(" #0.00000;-#0.00000"));
            Console.WriteLine(@"{0} - Mode", chiSquare.Mode.ToString(" #0.00000;-#0.00000"));
            Console.WriteLine(@"{0} - Variance", chiSquare.Variance.ToString(" #0.00000;-#0.00000"));
            Console.WriteLine(@"{0} - Standard deviation", chiSquare.StdDev.ToString(" #0.00000;-#0.00000"));
            Console.WriteLine(@"{0} - Skewness", chiSquare.Skewness.ToString(" #0.00000;-#0.00000"));
            Console.WriteLine();

            // 2. Generate 1000 samples of the ChiSquare(5) distribution
            Console.WriteLine(@"2. Generate 1000 samples of the ChiSquare(5) distribution");
            var data = new double[1000];
            for (var i = 0; i < data.Length; i++)
            {
                data[i] = chiSquare.Sample();
            }

            // 3. Get basic statistics on set of generated data using extention methods
            Console.WriteLine(@"3. Get basic statistics on set of generated data using extention methods");
            Console.WriteLine(@"{0} - Largest element", data.Maximum().ToString(" #0.00000;-#0.00000"));
            Console.WriteLine(@"{0} - Smallest element", data.Minimum().ToString(" #0.00000;-#0.00000"));
            Console.WriteLine(@"{0} - Mean", data.Mean().ToString(" #0.00000;-#0.00000"));
            Console.WriteLine(@"{0} - Median", data.Median().ToString(" #0.00000;-#0.00000"));
            Console.WriteLine(@"{0} - Biased population variance", data.PopulationVariance().ToString(" #0.00000;-#0.00000"));
            Console.WriteLine(@"{0} - Variance", data.Variance().ToString(" #0.00000;-#0.00000"));
            Console.WriteLine(@"{0} - Standard deviation", data.StandardDeviation().ToString(" #0.00000;-#0.00000"));
            Console.WriteLine(@"{0} - Biased sample standard deviation", data.PopulationStandardDeviation().ToString(" #0.00000;-#0.00000"));
            Console.WriteLine();

            // 4. Compute the basic statistics of data set using DescriptiveStatistics class
            Console.WriteLine(@"4. Compute the basic statistics of data set using DescriptiveStatistics class");
            var descriptiveStatistics = new DescriptiveStatistics(data);
            Console.WriteLine(@"{0} - Kurtosis", descriptiveStatistics.Kurtosis.ToString(" #0.00000;-#0.00000"));
            Console.WriteLine(@"{0} - Largest element", descriptiveStatistics.Maximum.ToString(" #0.00000;-#0.00000"));
            Console.WriteLine(@"{0} - Smallest element", descriptiveStatistics.Minimum.ToString(" #0.00000;-#0.00000"));
            Console.WriteLine(@"{0} - Mean", descriptiveStatistics.Mean.ToString(" #0.00000;-#0.00000"));
            Console.WriteLine(@"{0} - Variance", descriptiveStatistics.Variance.ToString(" #0.00000;-#0.00000"));
            Console.WriteLine(@"{0} - Standard deviation", descriptiveStatistics.StandardDeviation.ToString(" #0.00000;-#0.00000"));
            Console.WriteLine(@"{0} - Skewness", descriptiveStatistics.Skewness.ToString(" #0.00000;-#0.00000"));
            Console.WriteLine();

            // Generate 1000 samples of the ChiSquare(2.5) distribution
            var chiSquareB = new ChiSquared(2);
            var dataB = new double[1000];
            for (var i = 0; i < data.Length; i++)
            {
                dataB[i] = chiSquareB.Sample();
            }

            // 5. Correlation coefficient between 1000 samples of ChiSquare(5) and ChiSquare(2.5)
            Console.WriteLine(@"5. Correlation coefficient between 1000 samples of ChiSquare(5) and ChiSquare(2.5) is {0}", Correlation.Pearson(data, dataB).ToString("N04"));
            Console.WriteLine(@"6. Ranked correlation coefficient between 1000 samples of ChiSquare(5) and ChiSquare(2.5) is {0}", Correlation.Spearman(data, dataB).ToString("N04"));
            Console.WriteLine();

            // 6. Correlation coefficient between 1000 samples of f(x) = x * 2 and f(x) = x * x
            data = Generate.LinearSpacedMap(1000, 0, 100, x => x * 2);
            dataB = Generate.LinearSpacedMap(1000, 0, 100, x => x * x);
            Console.WriteLine(@"7. Correlation coefficient between 1000 samples of f(x) = x * 2 and f(x) = x * x is {0}", Correlation.Pearson(data, dataB).ToString("N04"));
            Console.WriteLine(@"8. Ranked correlation coefficient between 1000 samples of f(x) = x * 2 and f(x) = x * x is {0}", Correlation.Spearman(data, dataB).ToString("N04"));
            Console.WriteLine();
        }
    }
}
