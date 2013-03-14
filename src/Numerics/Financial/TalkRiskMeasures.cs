// <copyright file="TalkRiskMeasures.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
//
// Copyright (c) 2009-2010 Math.NET
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
// </copyright>

namespace MathNet.Numerics.Financial
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using MathNet.Numerics.Statistics;

    public static class TalkRiskMeasures
    {
        //ValueAtRisk

        //ModifiedValueAtRisk

        //ExpectedTailLoss
        //Expected shortfall is also called conditional value at risk (CVaR), average value at risk (AVaR), and expected tail loss (ETL).
        ///<a href="http://en.wikipedia.org/wiki/Expected_shortfall">Expected shortfall</a>
        ///<a href="http://www.bis.org/bcbs/ca/acertasc.pdf"></a>

        //ModifiedExpectedTailLoss
        //Same as ExpectedTailLoss except using Modified Value At Risk
        
        /// <summary>
        /// Jarque-Bera = n/6 [S^2 + 0.25(K – 3)^2]
        /// Jarque–Bera test is a goodness-of-fit test of whether sample data have the skewness and 
        /// kurtosis matching a normal distribution. 
        /// 
        /// n = the number of returns
        /// S = the skewness of the returns
        /// K = the kurtosis of the returns
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        /// <a href="http://en.wikipedia.org/wiki/Jarque%E2%80%93Bera_test">Jarque-Bera test</a>
        public static double JarqueBera(this IEnumerable<double> data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            var descStats = new DescriptiveStatistics(data);
            return data.Count() / 6 * (Math.Pow(descStats.Skewness, 2) + 0.25 * Math.Pow(descStats.Kurtosis - 3.0, 2));
        }

        //StarrRatio
        ///<a href="http://www.pstat.ucsb.edu/research/papers/optimFinPortf_final%5B1%5D.pdf">6.2 The STARR Ratio</a>

        //RachevRatio
        ///<a href="http://www.pstat.ucsb.edu/research/papers/optimFinPortf_final%5B1%5D.pdf">8.1 The Rachev Ratio</a>
        ///

        
    }
}
